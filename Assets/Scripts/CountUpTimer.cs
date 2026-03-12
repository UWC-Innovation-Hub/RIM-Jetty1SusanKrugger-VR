using UnityEngine;
using TMPro;

public class CountUpTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;

    private float timer = 0f;
    private float maxTime = 300f;

    void Update()
    {
        if (timer < maxTime)
        {
            timer += Time.deltaTime;

            int minutes = Mathf.FloorToInt(timer / 60);
            int seconds = Mathf.FloorToInt(timer % 60);

            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}
