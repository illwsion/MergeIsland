// StringTableManager.cs
using System.Collections.Generic;
using UnityEngine;

public class StringTableManager : MonoBehaviour
{
    public static StringTableManager Instance;

    // 언어 설정용 enum
    public enum Language
    {
        Korean,
        English
    }

    public Language currentLanguage = Language.Korean;

    // id → (korean, english) 저장
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
            Debug.LogError("[StringTableManager] StringTable.csv 를 찾을 수 없습니다.");
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        for (int i = 3; i < lines.Length; i++) // 첫 세 줄은 헤더
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');
            if (int.TryParse(values[0], out int id))
            {
                string korean = values[3].Trim();  // "한글" 컬럼
                string english = values[4].Trim(); // "영어" 컬럼

                stringMap[id] = new LocalizedString
                {
                    korean = korean,
                    english = english
                };
            }
        }

        Debug.Log($"[StringTableManager] {stringMap.Count}개의 문자열을 로드했습니다.");
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

        return $"[문자열 없음: {id}]";
    }

    public void SetLanguage(Language lang)
    {
        currentLanguage = lang;
    }
}
