using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class HistoryManager : MonoBehaviour
{
    public TextMeshProUGUI TMP_Status; // 顯示狀態的 TMP 元素
    public Transform content; // 記錄列表的父容器
    public GameObject recordPrefab; // 記錄顯示的預製件

    void Start()
    {
        TMP_Status.text = "載入記錄中...";

        // 從本地保存的數據載入分數
        LoadScoresFromPlayerPrefs();
    }

    /// 從 PlayerPrefs 載入分數資料並顯示到 UI
    private void LoadScoresFromPlayerPrefs()
    {
        int recordCount = PlayerPrefs.GetInt("RecordCount", 0);

        if (recordCount > 0)
        {
            for (int i = 0; i < recordCount; i++)
            {
                string scoreKey = $"Record_{i}_Score";
                string timestampKey = $"Record_{i}_Timestamp";

                string score = PlayerPrefs.GetString(scoreKey, "0");
                string timestamp = PlayerPrefs.GetString(timestampKey, "未知時間");

                DisplayScoreRecord(score, timestamp);
            }
            TMP_Status.text = "分數記錄載入完成。";
        }
        else
        {
            TMP_Status.text = "尚無分數記錄。";
        }
    }


    /// 顯示單筆分數記錄到 UI
    /// <param name="score">分數</param>
    /// <param name="timestamp">時間戳轉換後的日期字串</param>
    private void DisplayScoreRecord(string score, string timestamp)
    {
        // 創建一個新的記錄物件
        GameObject recordInstance = Instantiate(recordPrefab, content);

        // 設定分數與時間戳文字
        recordInstance.transform.Find("ScoreTextTMP").GetComponent<TMP_Text>().text = score;
        recordInstance.transform.Find("TimestampTextTMP").GetComponent<TMP_Text>().text = timestamp;
    }

    /// 保存新的分數記錄到 PlayerPrefs（可在其他腳本中調用）
    /// <param name="score">分數</param>
    public static void SaveScoreToPlayerPrefs(int score)
    {
        // 獲取當前時間
        string timestamp = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

        // 獲取目前已保存的記錄數量
        int recordCount = PlayerPrefs.GetInt("RecordCount", 0);

        // 保存新記錄
        PlayerPrefs.SetString($"Record_{recordCount}_Score", score.ToString());
        PlayerPrefs.SetString($"Record_{recordCount}_Timestamp", timestamp);

        // 更新記錄數量
        PlayerPrefs.SetInt("RecordCount", recordCount + 1);
        PlayerPrefs.Save();
    }

    /// 返回主畫面的按鈕事件
    public void OnNavigateToHome()
    {
        SceneManager.LoadScene("Home");
    }
}
