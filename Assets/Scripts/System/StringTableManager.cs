// StringTableManager.cs
using System.Collections.Generic;
using UnityEngine;

public class StringTableManager : MonoBehaviour
{
    public static StringTableManager Instance;

    // ��� ������ enum
    public enum Language
    {
        Korean,
        English
    }

    public Language currentLanguage = Language.Korean;

    // id �� (korean, english) ����
    private Dictionary<int, LocalizedString> stringMap = new Dictionary<int, LocalizedString>();

    public class LocalizedString
    {
        public string korean;
        public string english;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadStringTable();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadStringTable()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("StringTable");
        if (csvFile == null)
        {
            Debug.LogError("[StringTableManager] StringTable.csv �� ã�� �� �����ϴ�.");
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        for (int i = 3; i < lines.Length; i++) // ù �� ���� ���
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            if (int.TryParse(values[0], out int id))
            {
                string korean = values[3].Trim();  // "�ѱ�" �÷�
                string english = values[4].Trim(); // "����" �÷�

                stringMap[id] = new LocalizedString
                {
                    korean = korean,
                    english = english
                };
            }
        }

        Debug.Log($"[StringTableManager] {stringMap.Count}���� ���ڿ��� �ε��߽��ϴ�.");
    }

    public string GetLocalized(int id)
    {
        if (stringMap.TryGetValue(id, out var entry))
        {
            return currentLanguage switch
            {
                Language.Korean => entry.korean,
                Language.English => entry.english,
                _ => entry.korean
            };
        }

        return $"[���ڿ� ����: {id}]";
    }

    public void SetLanguage(Language lang)
    {
        currentLanguage = lang;
    }
}
