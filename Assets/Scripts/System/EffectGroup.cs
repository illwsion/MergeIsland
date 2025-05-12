using UnityEngine;

public class EffectGroup : MonoBehaviour
{
    [SerializeField] private GameObject smallEffectBlockPrefab;
    [SerializeField] private Transform blockParent;

    public void Clear()
    {
        foreach (Transform child in blockParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void AddEffect(string label, Sprite icon, string value = null)
    {
        var go = Instantiate(smallEffectBlockPrefab, blockParent);
        var ui = go.GetComponent<SmallEffectBlockUI>();
        ui.Set(label, icon, value);
    }
}
