using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.IO;

[DefaultExecutionOrder(-950)]
public class SpriteManager : MonoBehaviour
{
    public static SpriteManager Instance;

    [Header("Atlas Settings")]
    public List<SpriteAtlas> atlases;
    
    [Header("Auto Load Settings")]
    [Tooltip("폴더에서 아틀라스를 자동으로 스캔하고 로드할지 여부")]
    public bool autoLoadAtlases = true;
    
    [Tooltip("자동으로 스캔할 폴더 경로들")]
    public string[] atlasScanPaths = { "Assets/Art/Atlases", "Assets/Resources/Atlases" };
    
    [Tooltip("아틀라스 파일 확장자")]
    public string atlasExtension = ".spriteatlasv2";
    
    [Header("Sprite Type Rules")]
    [Tooltip("Atlas에서 로드할 스프라이트 타입들 (접두사)")]
    public string[] atlasSpriteTypes = { "icon_", "item_", "skill_", "effect_", "gate_" };
    
    [Tooltip("개별 파일에서 로드할 스프라이트 타입들 (접두사)")]
    public string[] individualSpriteTypes = { "char_", "anim_", "bg_", "player_", "enemy_", "ui_" };
    
    [Header("Atlas Mapping")]
    [Tooltip("스프라이트 타입별로 사용할 아틀라스 이름")]
    public string[] atlasNames = { "uiAtlas", "iconAtlas", "itemAtlas", "skillAtlas", "effectAtlas" };

    // 아틀라스 로드 완료 이벤트
    public System.Action OnAtlasesLoaded;
    public bool IsAtlasesLoaded { get; private set; } = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 이동에도 유지
            
            if (autoLoadAtlases)
            {
                StartCoroutine(LoadAtlasesFromPaths());
            }
        }
        else 
        {
            Destroy(gameObject); // 중복 방지
        }
    }

    /// <summary>
    /// 스프라이트 이름으로 스프라이트를 가져옵니다.
    /// 네이밍 컨벤션에 따라 Atlas 또는 개별 파일에서 자동으로 로드합니다.
    /// </summary>
    /// <param name="spriteName">스프라이트 이름</param>
    /// <returns>로드된 스프라이트</returns>
    public Sprite GetSprite(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
        {
            Debug.LogError("[SpriteManager] 스프라이트 이름이 비어있습니다!");
            return null;
        }

        // 아틀라스가 아직 로드되지 않았으면 대기
        if (!IsAtlasesLoaded)
        {
            Debug.LogWarning($"[SpriteManager] 아틀라스가 아직 로드되지 않았습니다. {spriteName} 요청을 대기합니다.");
            return null;
        }

        // Atlas 사용할 타입들
        if (atlasSpriteTypes.Any(type => spriteName.StartsWith(type)))
        {
            string atlasName = GetAtlasName(spriteName);
            return GetSpriteFromAtlas(atlasName, spriteName);
        }

        // 개별 파일 사용할 타입들
        if (individualSpriteTypes.Any(type => spriteName.StartsWith(type)))
        {
            string resourcePath = GetResourcePath(spriteName);
            return GetSpriteFromResource(resourcePath);
        }

        // 기본값은 Atlas에서 찾기 (기존 AtlasManager와의 호환성)
        Debug.LogWarning($"[SpriteManager] {spriteName}에 대한 타입 규칙이 정의되지 않았습니다. Atlas에서 찾습니다.");
        return GetSpriteFromAtlas("defaultAtlas", spriteName);
    }

    /// <summary>
    /// Atlas에서 스프라이트를 가져옵니다.
    /// </summary>
    private Sprite GetSpriteFromAtlas(string atlasName, string spriteName)
    {
        if (atlases == null || atlases.Count == 0)
        {
            Debug.LogError("[SpriteManager] atlases 리스트가 비어있습니다!");
            return null;
        }

        // 특정 아틀라스 이름으로 찾기
        var targetAtlas = atlases.FirstOrDefault(atlas => atlas.name == atlasName);
        if (targetAtlas != null)
        {
            Sprite sprite = targetAtlas.GetSprite(spriteName);
            if (sprite != null)
                return sprite;
        }

        // 모든 아틀라스에서 찾기 (기존 AtlasManager 방식)
        foreach (var atlas in atlases)
        {
            Sprite sprite = atlas.GetSprite(spriteName);
            if (sprite != null)
                return sprite;
        }

        Debug.LogWarning($"[SpriteManager] Atlas에서 스프라이트를 찾을 수 없습니다: {spriteName}");
        return null;
    }

    /// <summary>
    /// Resources 폴더에서 개별 스프라이트 파일을 로드합니다.
    /// </summary>
    private Sprite GetSpriteFromResource(string resourcePath)
    {
        Sprite sprite = Resources.Load<Sprite>(resourcePath);
        if (sprite == null)
        {
            Debug.LogWarning($"[SpriteManager] Resources에서 스프라이트를 찾을 수 없습니다: {resourcePath}");
        }
        return sprite;
    }

    /// <summary>
    /// 스프라이트 이름에 따라 사용할 아틀라스를 결정합니다.
    /// </summary>
    private string GetAtlasName(string spriteName)
    {
        if (spriteName.StartsWith("ui_")) return "uiAtlas";
        if (spriteName.StartsWith("icon_")) return "iconAtlas";
        if (spriteName.StartsWith("item_")) return "itemAtlas";
        if (spriteName.StartsWith("skill_")) return "skillAtlas";
        if (spriteName.StartsWith("effect_")) return "effectAtlas";
        
        // 기본값
        return "defaultAtlas";
    }

    /// <summary>
    /// 스프라이트 이름에 따라 Resources 경로를 생성합니다.
    /// </summary>
    private string GetResourcePath(string spriteName)
    {
        if (spriteName.StartsWith("char_")) return $"Characters/{spriteName}";
        if (spriteName.StartsWith("anim_")) return $"Animations/{spriteName}";
        if (spriteName.StartsWith("bg_")) return $"Backgrounds/{spriteName}";
        if (spriteName.StartsWith("player_")) return $"Characters/Player/{spriteName}";
        if (spriteName.StartsWith("enemy_")) return $"Characters/Enemy/{spriteName}";
        
        // 기본값
        return spriteName;
    }

    /// <summary>
    /// 기존 AtlasManager와의 호환성을 위한 메서드
    /// </summary>
    public Sprite GetSpriteFromAtlas(string name)
    {
        return GetSpriteFromAtlas("defaultAtlas", name);
    }

    /// <summary>
    /// 현재 설정된 스프라이트 타입 규칙들을 출력합니다 (디버깅용)
    /// </summary>
    [ContextMenu("Print Sprite Type Rules")]
    public void PrintSpriteTypeRules()
    {
        Debug.Log("=== Atlas Sprite Types ===");
        foreach (var type in atlasSpriteTypes)
        {
            Debug.Log($"Atlas: {type}");
        }
        
        Debug.Log("=== Individual Sprite Types ===");
        foreach (var type in individualSpriteTypes)
        {
            Debug.Log($"Individual: {type}");
        }
    }

    /// <summary>
    /// 스프라이트가 어떤 타입으로 분류되는지 확인합니다 (디버깅용)
    /// </summary>
    /// <param name="spriteName">확인할 스프라이트 이름</param>
    public void CheckSpriteType(string spriteName)
    {
        if (atlasSpriteTypes.Any(type => spriteName.StartsWith(type)))
        {
            string atlasName = GetAtlasName(spriteName);
            Debug.Log($"[SpriteManager] {spriteName} -> Atlas 타입 ({atlasName})");
        }
        else if (individualSpriteTypes.Any(type => spriteName.StartsWith(type)))
        {
            string resourcePath = GetResourcePath(spriteName);
            Debug.Log($"[SpriteManager] {spriteName} -> 개별 파일 타입 ({resourcePath})");
        }
        else
        {
            Debug.Log($"[SpriteManager] {spriteName} -> 기본 타입 (Atlas에서 검색)");
        }
    }

    #region Atlas Auto Loading Methods

    /// <summary>
    /// 지정된 경로들에서 아틀라스를 자동으로 스캔하고 로드합니다.
    /// </summary>
    private IEnumerator LoadAtlasesFromPaths()
    {
        Debug.Log("[SpriteManager] 아틀라스 자동 로드를 시작합니다...");
        
        foreach (string path in atlasScanPaths)
        {
            yield return StartCoroutine(LoadAtlasesFromPath(path));
        }
        
        Debug.Log($"[SpriteManager] 아틀라스 자동 로드 완료! 총 {atlases.Count}개의 아틀라스가 로드되었습니다.");
        
        // 로드 완료 상태 설정 및 이벤트 호출
        IsAtlasesLoaded = true;
        OnAtlasesLoaded?.Invoke();
        
        // 로드된 아틀라스 정보 출력
        PrintLoadedAtlasInfo();
    }

    /// <summary>
    /// 특정 경로에서 아틀라스를 스캔하고 로드합니다.
    /// </summary>
    private IEnumerator LoadAtlasesFromPath(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning($"[SpriteManager] 경로가 존재하지 않습니다: {folderPath}");
            yield break;
        }

        Debug.Log($"[SpriteManager] 경로 스캔 중: {folderPath}");

        // 폴더 내 모든 .spriteatlasv2 파일 찾기
        string[] atlasFiles = null;
        try
        {
            atlasFiles = Directory.GetFiles(folderPath, $"*{atlasExtension}", SearchOption.AllDirectories);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SpriteManager] 경로 스캔 중 오류 발생: {folderPath}, 오류: {e.Message}");
            yield break;
        }
        
        if (atlasFiles != null)
        {
            Debug.Log($"[SpriteManager] {folderPath}에서 {atlasFiles.Length}개의 아틀라스 파일을 발견했습니다.");
            
            foreach (string filePath in atlasFiles)
            {
                yield return StartCoroutine(LoadAtlasFromFile(filePath));
            }
        }
    }

    /// <summary>
    /// 개별 아틀라스 파일을 로드합니다.
    /// </summary>
    private IEnumerator LoadAtlasFromFile(string filePath)
    {
        // Unity 에셋 경로로 변환
        string assetPath = filePath.Replace(Application.dataPath, "Assets");
        
        // 아틀라스 로드 (에디터에서만 가능)
        SpriteAtlas atlas = null;
        bool loadSuccess = true;
        string errorMessage = "";
        
        try
        {
            #if UNITY_EDITOR
            atlas = UnityEditor.AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetPath);
            #else
            // 런타임에서는 Resources 폴더에서만 로드 가능
            string resourcePath = GetResourcePathFromAssetPath(assetPath);
            atlas = Resources.Load<SpriteAtlas>(resourcePath);
            #endif
        }
        catch (System.Exception e)
        {
            loadSuccess = false;
            errorMessage = e.Message;
        }
        
        if (!loadSuccess)
        {
            Debug.LogError($"[SpriteManager] 아틀라스 로드 중 오류 발생: {filePath}, 오류: {errorMessage}");
            yield return null;
            yield break;
        }
        
        if (atlas != null)
        {
            if (!atlases.Contains(atlas))
            {
                atlases.Add(atlas);
                Debug.Log($"[SpriteManager] 아틀라스 로드됨: {atlas.name} ({assetPath})");
            }
            else
            {
                Debug.Log($"[SpriteManager] 아틀라스가 이미 등록되어 있습니다: {atlas.name}");
            }
        }
        else
        {
            Debug.LogWarning($"[SpriteManager] 아틀라스를 로드할 수 없습니다: {assetPath}");
        }
        
        yield return null;
    }

    /// <summary>
    /// 에셋 경로를 Resources 경로로 변환합니다.
    /// </summary>
    private string GetResourcePathFromAssetPath(string assetPath)
    {
        // Assets/Resources/ 폴더 이후의 경로만 추출
        if (assetPath.Contains("/Resources/"))
        {
            int resourcesIndex = assetPath.IndexOf("/Resources/") + "/Resources/".Length;
            string resourcePath = assetPath.Substring(resourcesIndex);
            
            // 확장자 제거
            int extensionIndex = resourcePath.LastIndexOf('.');
            if (extensionIndex > 0)
            {
                resourcePath = resourcePath.Substring(0, extensionIndex);
            }
            
            return resourcePath;
        }
        
        return assetPath;
    }

    /// <summary>
    /// 로드된 아틀라스 정보를 출력합니다.
    /// </summary>
    private void PrintLoadedAtlasInfo()
    {
        if (atlases.Count == 0)
        {
            Debug.Log("[SpriteManager] 로드된 아틀라스가 없습니다.");
            return;
        }

        Debug.Log("=== 로드된 아틀라스 목록 ===");
        foreach (var atlas in atlases)
        {
            if (atlas != null)
            {
                Debug.Log($"- {atlas.name}");
            }
        }
    }

    /// <summary>
    /// 런타임에 아틀라스를 동적으로 추가합니다.
    /// </summary>
    public void AddAtlas(SpriteAtlas atlas)
    {
        if (atlas != null && !atlases.Contains(atlas))
        {
            atlases.Add(atlas);
            Debug.Log($"[SpriteManager] 아틀라스가 추가되었습니다: {atlas.name}");
        }
    }

    /// <summary>
    /// 아틀라스를 제거합니다.
    /// </summary>
    public void RemoveAtlas(SpriteAtlas atlas)
    {
        if (atlases.Remove(atlas))
        {
            Debug.Log($"[SpriteManager] 아틀라스가 제거되었습니다: {atlas.name}");
        }
    }

    /// <summary>
    /// 모든 아틀라스를 제거합니다.
    /// </summary>
    public void ClearAllAtlases()
    {
        int count = atlases.Count;
        atlases.Clear();
        Debug.Log($"[SpriteManager] {count}개의 아틀라스가 모두 제거되었습니다.");
    }

    /// <summary>
    /// 아틀라스 자동 로드를 수동으로 실행합니다.
    /// </summary>
    [ContextMenu("Manual Atlas Reload")]
    public void ManualAtlasReload()
    {
        if (Application.isPlaying)
        {
            StartCoroutine(LoadAtlasesFromPaths());
        }
        else
        {
            Debug.LogWarning("[SpriteManager] 아틀라스 자동 로드는 플레이 모드에서만 실행할 수 있습니다.");
        }
    }

    #endregion
}
