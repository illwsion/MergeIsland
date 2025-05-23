//ExpUIManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExpUIManager : MonoBehaviour
{
    [SerializeField] private Image expFillImage;       // Fill ÀÌ¹ÌÁö
    [SerializeField] private TMP_Text expText;         // "120 / 200"
    [SerializeField] private TMP_Text levelText;       // "Lv. 5"

    public void UpdateUI(int level, int currentExp, int requiredExp)
    {
        float percent = requiredExp > 0 ? (float)currentExp / requiredExp : 0f;
        expFillImage.fillAmount = Mathf.Clamp01(percent);

        expText.text = $"{currentExp} / {requiredExp}";
        levelText.text = $"Lv. {level}";
    }
}
