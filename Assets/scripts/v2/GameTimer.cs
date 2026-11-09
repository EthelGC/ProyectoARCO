using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public TMP_Text timerText;
    public GameObject endLevelPanel;
    public TMP_Text finalTimeText;

    private float elapsedTime = 0f;
    private bool isRunning = true;

    void Start()
    {
        endLevelPanel.SetActive(false);
    }

    void Update()
    {
        if (!isRunning) return;

        elapsedTime += Time.deltaTime;

        if (timerText != null)
            timerText.text = FormatTime(elapsedTime);
    }

    public void FinishLevel()
    {
        if (!isRunning) return;

        isRunning = false;

        string finalTime = FormatTime(elapsedTime);

        if (timerText != null)
            timerText.text = finalTime;

        if (finalTimeText != null)
            finalTimeText.text = "Tu tiempo: " + finalTime;

        endLevelPanel.SetActive(true);
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        float seconds = time % 60f;

        return string.Format("{0:00}:{1:00.00}", minutes, seconds);
    }
}
