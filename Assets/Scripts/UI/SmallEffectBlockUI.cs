// SmallEffectBlockUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SmallEffectBlockUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] TMP_Text smallLabelText;
    [SerializeField] Image smallIcon;
    [SerializeField] TMP_Text smallValueText;

    private EffectData effectData;

    public void Set(EffectData data)
    {
        effectData = data;

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

    public void OnPointerClick(PointerEventData eventData)
    {
        GateUnlockHelper.TryUnlock(effectData);
    }
}
