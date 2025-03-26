// AtlasManager.cs
using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;

public class AtlasManager : MonoBehaviour
{
    public static AtlasManager Instance;
    public List<SpriteAtlas> atlases;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 이동에도 유지
        }
        else
        {
            Destroy(gameObject); // 중복 방지
        }
    }

    public Sprite GetSprite(string name)
    {
        foreach (var atlas in atlases)
        {
            Sprite sprite = atlas.GetSprite(name);
            if (sprite != null)
                return sprite;
        }
        Debug.LogWarning($"[AtlasManager] Sprite not found: {name}");
        return null;
    }
}
