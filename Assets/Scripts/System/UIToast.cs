using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIToast : MonoBehaviour
{
    public static UIToast Instance;

    [Header("UI")]
    public TMP_Text messageText;
    public CanvasGroup canvasGroup;

    [Header("Settings")]
    public float displayDuration = 2f;
    public float fadeSpeed = 2f;

    private float timer;

    private void Awake()
    {
        Instance = this;
        canvasGroup.alpha = 0f;
    }

    private void Update()
    {
        if (canvasGroup.alpha > 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, Time.deltaTime * fadeSpeed);
            }
        }
    }

    public static void Show(string message)
    {
        if (Instance == null) return;

        Instance.messageText.text = message;
        Instance.canvasGroup.alpha = 1f;
        Instance.timer = Instance.displayDuration;
    }
}
