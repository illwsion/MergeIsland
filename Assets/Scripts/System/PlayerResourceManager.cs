// PlayerResourceManager.cs
using UnityEngine;
using System.Collections.Generic;

public class PlayerResourceManager : MonoBehaviour
{
    public static PlayerResourceManager Instance { get; private set; }

    private Dictionary<ResourceType, int> resourceTable = new Dictionary<ResourceType, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // �ʱ� �ڿ� ���� (����)
            resourceTable[ResourceType.Energy] = 100;
            resourceTable[ResourceType.Gold] = 100;
            resourceTable[ResourceType.Wood] = 50;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool HasEnough(ResourceType type, int value)
    {
        return resourceTable.ContainsKey(type) && resourceTable[type] >= value;
    }

    public bool TrySpend(ResourceType type, int value)
    {
        if (!HasEnough(type, value)) return false;

        resourceTable[type] -= value;
        Debug.Log($"[PlayerResource] {type} -{value} �� {resourceTable[type]} ����");
        // TODO: UI ������Ʈ ȣ��
        return true;
    }

    public void Add(ResourceType type, int value)
    {
        if (!resourceTable.ContainsKey(type))
            resourceTable[type] = 0;

        resourceTable[type] += value;
        Debug.Log($"[PlayerResource] {type} +{value} �� {resourceTable[type]}");
        // TODO: UI ������Ʈ ȣ��
    }

    public int GetAmount(ResourceType type)
    {
        return resourceTable.ContainsKey(type) ? resourceTable[type] : 0;
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"Energy: {GetAmount(ResourceType.Energy)}");
        GUILayout.Label($"Gold: {GetAmount(ResourceType.Gold)}");
        GUILayout.Label($"Wood: {GetAmount(ResourceType.Wood)}");
        GUILayout.EndArea();
    }
}
