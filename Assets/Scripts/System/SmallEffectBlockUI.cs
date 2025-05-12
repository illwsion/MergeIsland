// SmallEffectBlockUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SmallEffectBlockUI : MonoBehaviour
{
    [SerializeField] TMP_Text smallLabelText;
    [SerializeField] Image smallIconImage;
    [SerializeField] TMP_Text smallValueText;

    public void Set(string label, Sprite icon, string value = null)
    {
        smallLabelText.text = label;
        smallIconImage.sprite = icon;

        if (string.IsNullOrEmpty(value))
        {
            smallValueText.gameObject.SetActive(false);
        }
        else
        {
            smallValueText.gameObject.SetActive(true);
            smallValueText.text = value;
        }
    }
}
