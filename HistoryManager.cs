using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Auth;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class HistoryManager : MonoBehaviour
{
    public TextMeshProUGUI TMP_ScoreRecord_1; // 顯示分數的 TMP 元素
    public Transform content; // 父容器
    public GameObject recordPrefab; // 顯示分數記錄的預製件

    private DatabaseReference dbRef;

    void Start()
    {
            DisplayScoreRecord("150", "2024-11-26 14:00:00"); //手動
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogError("User not authenticated. Cannot load score history.");
            TMP_ScoreRecord_1.text = "Not Authenticated.";
            return;
        }

        string userId = user.UserId;
        dbRef = FirebaseDatabase.DefaultInstance.GetReference("scores").Child(userId);

        LoadScoresFromFirebase();
    }
        void DisplayScoreRecord(string score, string timestamp)
        {
            // 創建一個新的記錄物件
            GameObject recordInstance = Instantiate(recordPrefab, content); // 指定父物件 Content
            recordInstance.transform.Find("ScoreText").GetComponent<TMP_Text>().text = score; // 確保名稱正確
            recordInstance.transform.Find("TimestampText").GetComponent<TMP_Text>().text = timestamp;

            // 如果使用額外顯示格式
            // TMP_Text 組件位於不同的名稱 "TMP_ScoreText"，需要額外確認

            // Transform scoreTextTransform = recordInstance.transform.Find("TMP_ScoreText");
            // if (scoreTextTransform != null)
            // {
            //     TMP_Text scoreText = scoreTextTransform.GetComponent<TMP_Text>();
            //     scoreText.text = $"Score: {score} | Time: {timestamp}";
            // }
        }

    private void LoadScoresFromFirebase()
{
    dbRef.GetValueAsync().ContinueWith(task =>
    {
        if (task.IsCompleted)
        {
            DataSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    Dictionary<string, object> scoreEntry = (Dictionary<string, object>)childSnapshot.Value;

                    // 獲取分數和時間戳
                    string score = scoreEntry["score"].ToString();
                    string timestamp = ConvertTimestampToDate((long)scoreEntry["timestamp"]);

                    // 創建記錄
                    GameObject newRecord = Instantiate(recordPrefab, content); // 指定父物件 Content
                    newRecord.transform.Find("ScoreText").GetComponent<TMP_Text>().text = score;
                    newRecord.transform.Find("TimestampText").GetComponent<TMP_Text>().text = timestamp;
                }

                TMP_ScoreRecord_1.text = "Score Records Loaded.";
            }
            else
            {
                TMP_ScoreRecord_1.text = "No Score Records Found.";
            }
        }
        else
        {
            Debug.LogError("Failed to load score history: " + task.Exception);
            TMP_ScoreRecord_1.text = "Error Loading Scores.";
        }
    });
}

    private string ConvertTimestampToDate(long timestamp)
    {
        DateTime dateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
        return dateTime.ToString("yyyy/MM/dd HH:mm:ss");
    }
    public void OnNavigateToHome()
    {
        SceneManager.LoadScene("Home");
    }
}
