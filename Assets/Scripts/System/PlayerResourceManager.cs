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

            // 초기 자원 세팅 (예시)
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
        Debug.Log($"[PlayerResource] {type} -{value} → {resourceTable[type]} 남음");
        // TODO: UI 업데이트 호출
        return true;
    }

    public void Add(ResourceType type, int value)
    {
        if (!resourceTable.ContainsKey(type))
            resourceTable[type] = 0;

        resourceTable[type] += value;
        Debug.Log($"[PlayerResource] {type} +{value} → {resourceTable[type]}");
        // TODO: UI 업데이트 호출
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
