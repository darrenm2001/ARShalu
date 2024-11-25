using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class HistoryManager : MonoBehaviour
{
    public Transform content; // Scroll View 的 Content
    public GameObject scoreRecordPrefab; // 預製件，顯示單條分數
    public GameObject loadingPanel; // 顯示載入過程的面板

    void Start()
    {
        // 顯示載入中提示，並開始載入歷史分數
        StartCoroutine(LoadScoreHistoryWithDelay());
    }

    // 延遲載入分數歷史紀錄
    private IEnumerator LoadScoreHistoryWithDelay()
    {
        loadingPanel.SetActive(true); // 顯示載入中的面板
        yield return new WaitForSeconds(2); // 延遲 2 秒

        LoadScoreHistory(); // 真正開始載入分數
    }

    public void LoadScoreHistory()
    {
        Firebase.Auth.FirebaseUser user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null)
        {
        string userId = user.UserId; // 獲取目前登入的使用者 UID
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.GetReference("scores");
        dbRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;  // 過濾並顯示該使用者的分數
                
                ClearOldRecords();    // 清空舊的紀錄

                if (snapshot.ChildrenCount == 0)
                {
                    DisplayNoScoresMessage(); // 顯示沒有紀錄的提示
                }
                else
                {
                    foreach (DataSnapshot child in snapshot.Children)
                    {
                        var scoreEntry = child.Value as Dictionary<string, object>; // 同步現有代碼處理分數顯示的邏輯

                        if (scoreEntry.TryGetValue("score", out var scoreObj)) // 使用 TryGetValue 以防止鍵錯誤
                        {
                            string score = scoreObj.ToString();
                            
                            string displayText = $"Score: {score}"; // 顯示分數

                            GameObject newRecord = Instantiate(scoreRecordPrefab, content);  // 實例化新紀錄
                            TextMeshProUGUI recordText = newRecord.GetComponent<TextMeshProUGUI>();

                            if (recordText != null)
                            {
                                recordText.text = displayText;
                            }
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Failed to load score history: " + task.Exception);
        }

        loadingPanel.SetActive(false);
        });
        }
        else
        {
            Debug.LogError("No user is currently logged in.");
            DisplayNoScoresMessage();
        }
    }

    // 清空舊的紀錄
    private void ClearOldRecords()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    // 顯示沒有分數紀錄的訊息
    private void DisplayNoScoresMessage()
    {
        GameObject noScoresMessage = new GameObject("NoScoresMessage");
        TextMeshProUGUI tmp = noScoresMessage.AddComponent<TextMeshProUGUI>();
        tmp.text = "No score records found.";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 24;
        tmp.color = Color.white;

        // 將訊息顯示在 ScrollView 中
        noScoresMessage.transform.SetParent(content, false);
    }

    public void OnNavigateToHome()
    {
        SceneManager.LoadScene("Home");
    }
    
}
