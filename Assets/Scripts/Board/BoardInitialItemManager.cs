using UnityEngine;
using System.Collections.Generic;

public class BoardInitialItemManager : MonoBehaviour
{
    public static BoardInitialItemManager Instance;

    private List<BoardInitialItemData> initialItems = new List<BoardInitialItemData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadInitialItemData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public IEnumerable<BoardInitialItemData> GetInitialItemsForBoard(string boardKey)
    {
        return initialItems.FindAll(i => i.boardKey == boardKey);
    }

    private void LoadInitialItemData()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("BoardInitialItemTable");
        if (csvFile == null)
        {
            Debug.LogError("[BoardInitialItemManager] BoardInitialItemTable.csv �� ã�� �� �����ϴ�.");
            return;
        }

        string[] lines = csvFile.text.Split('\n');
        for (int i = 4; i < lines.Length; i++) // ��� ��ŵ
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            try
            {
                var data = new BoardInitialItemData
                {
                    boardKey = values[0].Trim(),
                    coord = new Vector2Int(int.Parse(values[1]), int.Parse(values[2])),
                    itemKey = values[3].Trim()
                };
                initialItems.Add(data);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BoardInitialItemManager] �Ľ� ����: {line}\n{e}");
            }
        }
    }
}
