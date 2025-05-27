// BigEffectBlockUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class BigEffectBlockUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Image supplyInputIcon;
    [SerializeField] Image supplyOutputIcon;
    [SerializeField] TMP_Text supplyLabelText;
    [SerializeField] TMP_Text supplyOutputValueText;

    private EffectData effectData;

    public void Set(EffectData data)
    {
        effectData = data;

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

    public void OnPointerClick(PointerEventData eventData)
    {
        GateUnlockHelper.TryUnlock(effectData);
    }
}
