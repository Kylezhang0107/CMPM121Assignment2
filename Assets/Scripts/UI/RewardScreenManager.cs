using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class RewardScreenManager : MonoBehaviour
{
    public GameObject rewardUI;
    public GameObject endUI;
    public TextMeshProUGUI waveLabel;

    private TextMeshProUGUI endLabel;
    private TextMeshProUGUI statsLabel;

    void Start()
    {
        // find the "Wave" TMP by its GameObject name (it starts inactive so use FindObjectsOfTypeAll)
        if (waveLabel == null)
        {
            foreach (TextMeshProUGUI tmp in Resources.FindObjectsOfTypeAll<TextMeshProUGUI>())
            {
                if (tmp.gameObject.name == $"Wave {GameManager.Instance.currentWave} Completed")
                {
                    waveLabel = tmp;
                    break;
                }
            }
        }

        if (rewardUI != null)
        {
            // stats label (enemies killed this wave)
            GameObject statsObj = new GameObject("StatsLabel");
            statsObj.transform.SetParent(rewardUI.transform, false);
            RectTransform srt = statsObj.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 1f);
            srt.anchorMax = new Vector2(0.5f, 1f);
            srt.pivot = new Vector2(0.5f, 0.5f);
            srt.anchoredPosition = new Vector2(0f, -130f);
            srt.sizeDelta = new Vector2(280f, 60f);
            statsLabel = statsObj.AddComponent<TextMeshProUGUI>();
            statsLabel.alignment = TextAlignmentOptions.Center;
            statsLabel.fontSize = 24;
            statsLabel.color = Color.black;
            if (waveLabel != null) statsLabel.font = waveLabel.font;
        }

        // create a win/lose message label inside endUI
        if (endUI != null)
        {
            GameObject labelObj = new GameObject("EndMessage");
            labelObj.transform.SetParent(endUI.transform, false);
            RectTransform rt = labelObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -60f);
            rt.sizeDelta = new Vector2(300f, 80f);
            endLabel = labelObj.AddComponent<TextMeshProUGUI>();
            endLabel.alignment = TextAlignmentOptions.Center;
            endLabel.fontSize = 36;
            endLabel.color = Color.black;
            if (waveLabel != null) endLabel.font = waveLabel.font;
        }
    }

    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.GAMEOVER)
        {
            if (endLabel != null)
                endLabel.text = GameManager.Instance.playerWon ? "You Win!" : "You Lose!";
            endUI.SetActive(true);
            rewardUI.SetActive(false);
        }
        else if (GameManager.Instance.state == GameManager.GameState.WAVEEND)
        {
            if (waveLabel != null)
                waveLabel.text = $"Wave {GameManager.Instance.currentWave} Completed";
            if (statsLabel != null)
                statsLabel.text = $"Enemies killed: {GameManager.Instance.waveEnemiesKilled}";
            rewardUI.SetActive(true);
            endUI.SetActive(false);
        }
        else
        {
            rewardUI.SetActive(false);
            endUI.SetActive(false);
        }
    }
}
