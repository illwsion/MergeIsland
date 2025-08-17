//ExpUIManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExpUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text expText;             // "120 / 200"
    [SerializeField] private TMP_Text levelText;           // "Lv. 5"
    [SerializeField] private RectTransform fillBar;        // Fill 바
    [SerializeField] private float fillBarMaxWidth = 200f; // 기본 너비

    public void UpdateUI(int level, int currentExp, int requiredExp)
    {
        float percent = requiredExp > 0 ? (float)currentExp / requiredExp : 0f;
        float width = Mathf.Clamp01(percent) * fillBarMaxWidth;

        if (fillBar != null)
            fillBar.sizeDelta = new Vector2(width, fillBar.sizeDelta.y);

        if (expText != null)
            expText.text = $"{currentExp} / {requiredExp}";

        if (levelText != null)
            levelText.text = $"Lv. {level}";
    }
}

