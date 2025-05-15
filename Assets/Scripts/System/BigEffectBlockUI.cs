// BigEffectBlockUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BigEffectBlockUI : MonoBehaviour
{
    [SerializeField] Image supplyInputIcon;
    [SerializeField] Image supplyOutputIcon;
    [SerializeField] TMP_Text supplyLabelText;
    [SerializeField] TMP_Text supplyOutputValueText;

    public void Set(EffectData data)
    {
        supplyLabelText.text = data.label;
        supplyInputIcon.sprite = data.icon1;
        supplyOutputIcon.sprite = data.icon2;

        if (string.IsNullOrEmpty(data.value))
        {
            supplyOutputValueText.gameObject.SetActive(false);
        }
        else
        {
            supplyOutputValueText.gameObject.SetActive(true);
            supplyOutputValueText.text = data.value;
        }
    }
}
