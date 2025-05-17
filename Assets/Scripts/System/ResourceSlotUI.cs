// ResourceSlotUI.cs
using UnityEngine;
using TMPro;

public class ResourceSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private RectTransform fillBar; // null이면 '바 없음' 슬롯

    [SerializeField] private bool showRecoveryTimer = false;
    [SerializeField] private TMP_Text recoveryTimerText;

    public void UpdateUI(int current, int max)
    {
        if (fillBar != null)
        {
            float percent = Mathf.Clamp01((float)current / max);
            fillBar.sizeDelta = new Vector2(percent * 200f, fillBar.sizeDelta.y);
            valueText.text = $"{current} / {max}";
        }
        else
        {
            valueText.text = current.ToString();
        }
    }

    private void Update()
    {
        if (!showRecoveryTimer || recoveryTimerText == null)
            return;

        var manager = PlayerResourceManager.Instance;
        int current = manager.GetAmount(ResourceType.Energy);
        int max = manager.MaxEnergy;

        if (current >= max)
        {
            recoveryTimerText.gameObject.SetActive(false);
        }
        else
        {
            float remaining = manager.RecoveryRemainingTime;
            int minutes = Mathf.FloorToInt(remaining / 60f);
            int seconds = Mathf.FloorToInt(remaining % 60f);

            recoveryTimerText.gameObject.SetActive(true);
            recoveryTimerText.text = $"{minutes:D2}:{seconds:D2}";
        }
    }
}
