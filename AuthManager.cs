using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class AuthManager : MonoBehaviour
{
    public TMP_InputField usernameInputField;
    public TMP_InputField passwordInputField;
    public TMP_Text feedbackText;

    private FirebaseAuth auth;
    private DatabaseReference dbReference;

    void Start()
    {
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                ShowFeedback("歡迎！");
            }
            else
            {
                ShowFeedback("載入中" + task.Result.ToString());
            }
        });
    }

    public void OnRegisterButtonClicked()
    {
        if (!ValidateInputs()) return;

        string username = usernameInputField.text.Trim();
        string password = passwordInputField.text;

        // 檢查用戶名是否已存在
        dbReference.Child("usernames").Child(username).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted || task.IsCanceled)
            {
                ShowFeedback("檢查用戶名失敗！：" + task.Exception?.Message);
            }
            else if (task.Result.Exists)
            {
                ShowFeedback("用戶名已存在，請選擇其他名稱！");
            }
            else
            {
                RegisterUser(username, password);
            }
        });
    }

    private void RegisterUser(string username, string password)
    {
        // 生成唯一電子郵件
        string sanitizedUsername = username.Replace(" ", "").ToLower(); // 移除空格並轉為小寫
        string email = $"{sanitizedUsername}@yourapp.com";

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                foreach (var exception in task.Exception.Flatten().InnerExceptions)
                {
                    FirebaseException firebaseEx = exception as FirebaseException;
                    if (firebaseEx != null)
                    {
                        ShowFeedback("註冊失敗：" + firebaseEx.Message);
                        Debug.LogError("註冊錯誤：" + firebaseEx.ErrorCode);
                    }
                }
                return;
            }

            FirebaseUser newUser = task.Result.User;
            SaveUsernameToDatabase(newUser.UserId, username, password);
        });
    }

    private void SaveUsernameToDatabase(string userId, string username, string password)
    {
        // 儲存用戶信息
        User newUser = new User(username, password);
        dbReference.Child("users").Child(userId).SetRawJsonValueAsync(JsonUtility.ToJson(newUser))
        .ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                ShowFeedback("儲存用戶資料失敗：" + task.Exception?.Message);
            }
            else
            {
                // 儲存到 usernames 節點
                dbReference.Child("usernames").Child(username).SetValueAsync(userId).ContinueWithOnMainThread(innerTask => {
                    if (innerTask.IsCanceled || innerTask.IsFaulted)
                    {
                        ShowFeedback("儲存用戶名稱失敗：" + innerTask.Exception?.Message);
                    }
                    else
                    {
                        ShowFeedback("註冊成功！");
                        StartCoroutine(LoadSceneAfterDelay(2f)); // 等待2秒後加載場景
                        SceneManager.LoadScene("LoginSystem");// 切換到Login場景
                    }
                });
            }
        });
    }

    public void OnLoginButtonClicked()
    {
        if (!ValidateInputs()) return;

        string username = usernameInputField.text.Trim();
        string password = passwordInputField.text;

        // 查詢用戶名的 UserId
        dbReference.Child("usernames").Child(username).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted || task.IsCanceled || !task.Result.Exists)
            {
                ShowFeedback("用戶名稱無效或登錄失敗");
            }
            else
            {
                string userId = task.Result.Value.ToString();

                // 使用 UserId 確認密碼
                dbReference.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(innerTask => {
                    if (innerTask.IsFaulted || innerTask.IsCanceled || !innerTask.Result.Exists)
                    {
                        ShowFeedback("登錄失敗：用戶數據無效");
                        return;
                    }

                    string storedPassword = innerTask.Result.Child("password").Value.ToString();
                    if (password == storedPassword)
                    {
                        ShowFeedback("登錄成功！");
                        StartCoroutine(LoadSceneAfterDelay(3f)); // 等待2秒後加載場景
                        SceneManager.LoadScene("Home");// 切換到Home場景
                    
                    }
                    else
                    {
                        ShowFeedback("密碼錯誤！");
                    }
                });
            }
        });
    }

    private bool ValidateInputs()
    {
        if (string.IsNullOrEmpty(usernameInputField.text) || string.IsNullOrEmpty(passwordInputField.text))
        {
            ShowFeedback("請輸入用戶名稱和密碼");
            return false;
        }

        if (passwordInputField.text.Length < 6) // 密碼要求至少 6 個字符
        {
            ShowFeedback("密碼至少需要6個字符");
            return false;
        }

        if (usernameInputField.text.Length < 3)
        {
            ShowFeedback("用戶名稱至少需要3個字");
            return false;
        }

        return true;
    }

    private void ShowFeedback(string message)
    {
        feedbackText.text = message;
    }

    public void OnNavigateToRegister()
    {
        SceneManager.LoadScene("RegisterSystem");
    }

    public void OnNavigateToLogin()
    {
        SceneManager.LoadScene("LoginSystem");
    }
    private IEnumerator LoadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // SceneManager.LoadScene("Home");
    }
        public string GetCurrentUserId()
    {
        return auth.CurrentUser != null ? auth.CurrentUser.UserId : null;
    }

    public string GetCurrentUsername()
    {
        return auth.CurrentUser != null ? auth.CurrentUser.DisplayName : null;
    }

    }

[System.Serializable]
public class User
{
    public string username;
    public string password;

    public User(string username, string password)
    {
        this.username = username;
        this.password = password;
    }
    
}
