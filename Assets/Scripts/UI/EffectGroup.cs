// EffectGroup.cs
using System.Collections.Generic;
using UnityEngine;

public class EffectGroup : MonoBehaviour
{
    [SerializeField] private GameObject smallEffectBlockPrefab;
    [SerializeField] private GameObject bigEffectBlockPrefab;
    [SerializeField] private Transform blockParent;

    public void Clear()
    {
        // �ڽ� Transform���� ����Ʈ�� ���� ����
        var children = new List<Transform>();
        foreach (Transform child in blockParent)
            children.Add(child);

        // ����� ����Ʈ �������� Destroy
        foreach (var child in children)
            Destroy(child.gameObject);
    }

    public void AddEffect(EffectData data)
    {
        GameObject prefab = data.blockSize == EffectBlockSize.Small
            ? smallEffectBlockPrefab
            : bigEffectBlockPrefab;

        var go = Instantiate(prefab, blockParent);
        
        if (data.blockSize == EffectBlockSize.Small)
            go.GetComponent<SmallEffectBlockUI>().Set(data);
        else
            go.GetComponent<BigEffectBlockUI>().Set(data);
    }
}
