using UnityEngine;
using TMPro;

public class ClapConfirmationUI : MonoBehaviour
{
    public TextMeshProUGUI clapText;
    public float displayDuration = 1.0f;

    private float timer = 0f;

    void Update()
    {
        if (!clapText.gameObject.activeSelf)
        {
            timer += Time.deltaTime;

            if (timer >= displayDuration)
            {
                clapText.gameObject.SetActive(true);
                timer = 0f;
            }
        }
    }

    public void ShowClap()
    {
        clapText.gameObject.SetActive(false);
        timer = 0f;
    }
}
