// ThemeTileManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

[DefaultExecutionOrder(-900)]
public class ThemeTileManager : MonoBehaviour
{
    public static ThemeTileManager Instance;
    
    // 앱 실행(세션)마다 바뀌는 시드. 같은 세션에서는 동일 값을 유지
    private int sessionSeed;

    [Header("Tile Settings")]
    [Tooltip("각 테마별 타일 개수")]
    public int tilesPerTheme = 8;
    
    [Tooltip("타일 스프라이트 접두사")]
    public string tileSpritePrefix = "tile_";

    private Dictionary<BoardData.BoardTheme, List<string>> themeTileMap = new Dictionary<BoardData.BoardTheme, List<string>>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // 세션 시드를 초기화 (앱 시작 시 한 번)
            sessionSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            InitializeThemeTiles();

            // 아틀라스가 이미 로드된 경우 즉시 재구성, 아니면 로드 완료 시 재구성
            if (SpriteManager.Instance != null)
            {
                if (SpriteManager.Instance.IsAtlasesLoaded)
                {
                    RebuildThemeTilesFromAtlases();
                }
                else
                {
                    SpriteManager.Instance.OnAtlasesLoaded += RebuildThemeTilesFromAtlases;
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 각 테마별 타일 스프라이트 이름을 초기화합니다.
    /// </summary>
    private void InitializeThemeTiles()
    {
        // Beach 테마 타일들
        var beachTiles = new List<string>();
        for (int i = 0; i < tilesPerTheme; i++)
        {
            beachTiles.Add($"{tileSpritePrefix}beach_{i}");
        }
        themeTileMap[BoardData.BoardTheme.Beach] = beachTiles;

        // Plain 테마 타일들
        var plainTiles = new List<string>();
        for (int i = 0; i < tilesPerTheme; i++)
        {
            plainTiles.Add($"{tileSpritePrefix}plain_{i}");
        }
        themeTileMap[BoardData.BoardTheme.Plain] = plainTiles;

        // Forest 테마 타일들
        var forestTiles = new List<string>();
        for (int i = 0; i < tilesPerTheme; i++)
        {
            forestTiles.Add($"{tileSpritePrefix}forest_{i}");
        }
        themeTileMap[BoardData.BoardTheme.Forest] = forestTiles;
    }

    /// <summary>
    /// 로드된 아틀라스에서 실제 존재하는 타일 스프라이트 이름을 스캔하여 재구성합니다.
    /// 접두사 규칙을 따르는 모든 스프라이트를 자동 수집합니다.
    /// </summary>
    private void RebuildThemeTilesFromAtlases()
    {
        if (SpriteManager.Instance == null || SpriteManager.Instance.atlases == null)
        {
            return;
        }

        var prefixToTheme = new Dictionary<string, BoardData.BoardTheme>
        {
            { $"{tileSpritePrefix}beach", BoardData.BoardTheme.Beach },
            { $"{tileSpritePrefix}plain", BoardData.BoardTheme.Plain },
            { $"{tileSpritePrefix}forest", BoardData.BoardTheme.Forest },
        };

        // 임시 수집 컨테이너
        var collected = new Dictionary<BoardData.BoardTheme, HashSet<string>>
        {
            { BoardData.BoardTheme.Beach, new HashSet<string>() },
            { BoardData.BoardTheme.Plain, new HashSet<string>() },
            { BoardData.BoardTheme.Forest, new HashSet<string>() }
        };

        foreach (var atlas in SpriteManager.Instance.atlases)
        {
            if (atlas == null) continue;

            // 충분히 큰 버퍼로 스프라이트를 가져옴
            var buffer = new Sprite[2048];
            int count = atlas.GetSprites(buffer);
            for (int i = 0; i < count; i++)
            {
                var s = buffer[i];
                if (s == null) continue;
                string name = s.name;

                foreach (var kv in prefixToTheme)
                {
                    if (name.StartsWith(kv.Key))
                    {
                        collected[kv.Value].Add(name);
                    }
                }
            }
        }

        // 결과를 themeTileMap에 반영 (있으면 교체)
        foreach (var kv in collected)
        {
            if (kv.Value.Count > 0)
            {
                themeTileMap[kv.Key] = new List<string>(kv.Value);
            }
        }
    }

    /// <summary>
    /// 지정된 테마에 맞는 랜덤 타일 스프라이트를 반환합니다.
    /// </summary>
    /// <param name="theme">보드 테마</param>
    /// <returns>랜덤으로 선택된 타일 스프라이트</returns>
    public Sprite GetRandomTileSprite(BoardData.BoardTheme theme)
    {
        if (theme == BoardData.BoardTheme.None)
        {
            Debug.LogWarning("[ThemeTileManager] None 테마는 타일을 지원하지 않습니다.");
            return null;
        }

        if (!themeTileMap.TryGetValue(theme, out var tiles))
        {
            Debug.LogError($"[ThemeTileManager] {theme} 테마의 타일을 찾을 수 없습니다.");
            return null;
        }

        // 만약 초기 규칙 이름들이 실제로 없을 수 있으므로, 아틀라스 로드 이후 재구성 시도
        if ((tiles == null || tiles.Count == 0) && SpriteManager.Instance != null && SpriteManager.Instance.IsAtlasesLoaded)
        {
            RebuildThemeTilesFromAtlases();
            themeTileMap.TryGetValue(theme, out tiles);
        }

        if (tiles == null || tiles.Count == 0)
        {
            Debug.LogWarning($"[ThemeTileManager] {theme} 테마의 사용 가능한 타일 이름이 없습니다.");
            return null;
        }

        // 랜덤 인덱스 선택
        int randomIndex = Random.Range(0, tiles.Count);
        string spriteName = tiles[randomIndex];

        // SpriteManager를 통해 스프라이트 로드
        Sprite sprite = SpriteManager.Instance.GetSprite(spriteName);
        
        if (sprite == null)
        {
            Debug.LogWarning($"[ThemeTileManager] 스프라이트를 찾을 수 없습니다: {spriteName}");
        }

        return sprite;
    }

    /// <summary>
    /// 지정된 테마의 모든 타일 스프라이트를 반환합니다.
    /// </summary>
    /// <param name="theme">보드 테마</param>
    /// <returns>해당 테마의 모든 타일 스프라이트 리스트</returns>
    public List<Sprite> GetAllTileSprites(BoardData.BoardTheme theme)
    {
        var sprites = new List<Sprite>();

        if (theme == BoardData.BoardTheme.None)
        {
            return sprites;
        }

        if (!themeTileMap.TryGetValue(theme, out var tiles))
        {
            Debug.LogError($"[ThemeTileManager] {theme} 테마의 타일을 찾을 수 없습니다.");
            return sprites;
        }

        foreach (string spriteName in tiles)
        {
            Sprite sprite = SpriteManager.Instance.GetSprite(spriteName);
            if (sprite != null)
            {
                sprites.Add(sprite);
            }
            else
            {
                Debug.LogWarning($"[ThemeTileManager] 스프라이트를 찾을 수 없습니다: {spriteName}");
            }
        }

        return sprites;
    }

    /// <summary>
    /// 셀에 테마에 맞는 랜덤 타일을 적용합니다.
    /// </summary>
    /// <param name="cellTransform">타일을 적용할 셀 Transform</param>
    /// <param name="theme">보드 테마</param>
    public void ApplyRandomTileToCell(Transform cellTransform, BoardData.BoardTheme theme)
    {
        if (cellTransform == null)
        {
            Debug.LogWarning("[ThemeTileManager] 셀 Transform이 null입니다.");
            return;
        }

        // 셀의 Image 컴포넌트 찾기
        Image cellImage = cellTransform.GetComponent<Image>();
        if (cellImage == null)
        {
            Debug.LogWarning("[ThemeTileManager] 셀에 Image 컴포넌트가 없습니다.");
            return;
        }

        // 아틀라스가 아직 로드되지 않았다면, 로드 완료 시 다시 적용
        if (SpriteManager.Instance != null && !SpriteManager.Instance.IsAtlasesLoaded)
        {
            System.Action onLoaded = null;
            onLoaded = () =>
            {
                // 셀이 이미 파괴되었으면 중단
                if (cellTransform == null) return;

                // 한 번만 실행되도록 구독 해제
                SpriteManager.Instance.OnAtlasesLoaded -= onLoaded;

                // 로드 완료 후 다시 시도
                ApplyRandomTileToCell(cellTransform, theme);
            };

            SpriteManager.Instance.OnAtlasesLoaded += onLoaded;
            return;
        }

        // 랜덤 타일 스프라이트 가져오기
        Sprite tileSprite = GetRandomTileSprite(theme);
        if (tileSprite != null)
        {
            cellImage.sprite = tileSprite;
        }
        else
        {
            Debug.LogWarning($"[ThemeTileManager] {theme} 테마의 타일 스프라이트를 가져올 수 없습니다.");
        }
    }

    /// <summary>
    /// 보드 키와 좌표 기반의 결정적 타일을 셀에 적용합니다.
    /// </summary>
    public void ApplyDeterministicTileToCell(Transform cellTransform, BoardData.BoardTheme theme, string boardKey, Vector2Int coord)
    {
        if (cellTransform == null)
        {
            Debug.LogWarning("[ThemeTileManager] 셀 Transform이 null입니다.");
            return;
        }

        Image cellImage = cellTransform.GetComponent<Image>();
        if (cellImage == null)
        {
            Debug.LogWarning("[ThemeTileManager] 셀에 Image 컴포넌트가 없습니다.");
            return;
        }

        // 아틀라스 로드 전이면 로드 후 재적용
        if (SpriteManager.Instance != null && !SpriteManager.Instance.IsAtlasesLoaded)
        {
            System.Action onLoaded = null;
            onLoaded = () =>
            {
                if (cellTransform == null) return;
                SpriteManager.Instance.OnAtlasesLoaded -= onLoaded;
                ApplyDeterministicTileToCell(cellTransform, theme, boardKey, coord);
            };
            SpriteManager.Instance.OnAtlasesLoaded += onLoaded;
            return;
        }

        Sprite tileSprite = GetDeterministicTileSprite(theme, boardKey, coord);
        if (tileSprite != null)
        {
            cellImage.sprite = tileSprite;
        }
    }

    /// <summary>
    /// 보드 키와 좌표에서 안정적인 해시를 만들어 테마 타일 중 하나를 선택합니다.
    /// </summary>
    public Sprite GetDeterministicTileSprite(BoardData.BoardTheme theme, string boardKey, Vector2Int coord)
    {
        if (!themeTileMap.TryGetValue(theme, out var tiles) || tiles == null || tiles.Count == 0)
        {
            // 필요 시 아틀라스 재스캔
            if (SpriteManager.Instance != null && SpriteManager.Instance.IsAtlasesLoaded)
            {
                RebuildThemeTilesFromAtlases();
                themeTileMap.TryGetValue(theme, out tiles);
            }
        }

        if (tiles == null || tiles.Count == 0)
        {
            return null;
        }

        // 세션 시드를 포함하여 같은 세션에서는 고정, 재시작 시 변경
        string key = sessionSeed + "_" + boardKey + "_" + coord.x + "_" + coord.y + "_" + theme.ToString();
        int hash = ComputeStableHash(key);
        int index = Mathf.Abs(hash) % tiles.Count;
        string spriteName = tiles[index];
        return SpriteManager.Instance.GetSprite(spriteName);
    }

    // FNV-1a 32-bit 안정적 해시
    private int ComputeStableHash(string text)
    {
        unchecked
        {
            const int fnvPrime = 16777619;
            const int offsetBasis = unchecked((int)2166136261);
            int hash = offsetBasis;
            for (int i = 0; i < text.Length; i++)
            {
                hash ^= text[i];
                hash *= fnvPrime;
            }
            return hash;
        }
    }

    /// <summary>
    /// 특정 테마의 타일 개수를 반환합니다.
    /// </summary>
    /// <param name="theme">보드 테마</param>
    /// <returns>해당 테마의 타일 개수</returns>
    public int GetTileCount(BoardData.BoardTheme theme)
    {
        if (themeTileMap.TryGetValue(theme, out var tiles))
        {
            return tiles.Count;
        }
        return 0;
    }

    /// <summary>
    /// 지원되는 모든 테마를 반환합니다.
    /// </summary>
    /// <returns>지원되는 테마 리스트</returns>
    public List<BoardData.BoardTheme> GetSupportedThemes()
    {
        var themes = new List<BoardData.BoardTheme>();
        foreach (var kvp in themeTileMap)
        {
            if (kvp.Key != BoardData.BoardTheme.None)
            {
                themes.Add(kvp.Key);
            }
        }
        return themes;
    }

    /// <summary>
    /// 디버깅용: 특정 테마의 모든 타일 이름을 출력합니다.
    /// </summary>
    /// <param name="theme">확인할 테마</param>
    [ContextMenu("Debug Print Theme Tiles")]
    public void DebugPrintThemeTiles()
    {
        foreach (var kvp in themeTileMap)
        {
            Debug.Log($"[ThemeTileManager] {kvp.Key} 테마 타일들:");
            foreach (string tileName in kvp.Value)
            {
                Debug.Log($"  - {tileName}");
            }
        }
    }
}
