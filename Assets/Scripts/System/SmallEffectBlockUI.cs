// SmallEffectBlockUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SmallEffectBlockUI : MonoBehaviour
{
    [SerializeField] TMP_Text smallLabelText;
    [SerializeField] Image smallIcon;
    [SerializeField] TMP_Text smallValueText;

    public void Set(EffectData data)
    {
        smallLabelText.text = data.label;
        smallIcon.sprite = data.icon1;

        if (string.IsNullOrEmpty(data.value))
        {
            smallValueText.gameObject.SetActive(false);
        }
        else
        {
            smallValueText.gameObject.SetActive(true);
            smallValueText.text = data.value;
        }
    }
}
