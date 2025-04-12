using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Resources;

public class EnergyUIManager : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform fillBar;
    public TMP_Text energyText;
    public TMP_Text recoveryTimerText;

    [Header("Settings")]
    public float maxBarWidth = 200f; // Fill 바의 최대 길이 (px) 기준

    private PlayerResourceManager resourceManager;

    private void Start()
    {
        resourceManager = PlayerResourceManager.Instance;

        if (fillBar != null)
        {
            maxBarWidth = fillBar.sizeDelta.x; // ← Fill 초기 크기로 자동 세팅
        }
        UpdateUI(resourceManager.GetAmount(ResourceType.Energy));
    }

    private void Update()
    {
        UpdateRecoveryTimerUI();
    }

    public void UpdateUI(int currentEnergy)
    {
        int maxEnergy = PlayerResourceManager.Instance.MaxEnergy;

        // Fill 길이 계산
        float percent = Mathf.Clamp01((float)currentEnergy / maxEnergy);
        fillBar.sizeDelta = new Vector2(percent * maxBarWidth, fillBar.sizeDelta.y);
        Debug.Log($"[EnergyUI] 에너지: {currentEnergy}, Fill width: {fillBar.sizeDelta.x}");
        energyText.text = $"{currentEnergy} / {maxEnergy}";
    }

    private void UpdateRecoveryTimerUI()
    {
        int current = resourceManager.GetAmount(ResourceType.Energy);
        int max = resourceManager.MaxEnergy;

        if (current >= max)
        {
            recoveryTimerText.gameObject.SetActive(false);
            return;
        }

        float remaining = resourceManager.RecoveryRemainingTime;
        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);

        recoveryTimerText.gameObject.SetActive(true);
        recoveryTimerText.text = $"{minutes:D2}:{seconds:D2}";
    }
}
