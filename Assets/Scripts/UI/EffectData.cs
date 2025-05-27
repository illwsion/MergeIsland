using UnityEngine;

public enum EffectType
{
    Gather,
    Produce,
    Sell,
    Damage,
    Supply,
    Drop,
    MaxCap,
    Gate_Level,
    Gate_Supply,
    Gate_Quest,
    Gate_Resource
    // 필요 시 추가
}

public enum EffectBlockSize
{
    Small,
    Large
}

[System.Serializable]
public class EffectData
{
    public EffectType type;
    public EffectBlockSize blockSize;
    public string label;     // 상단 라벨 텍스트 (ex: "획득")
    public Sprite icon1;      // 좌측 아이콘 이미지
    public Sprite icon2;      // 우측 아이콘 이미지
    public string value;     // 하단 수치 텍스트 (ex: "+1000", "500", or null)

    public BoardGate sourceGate;
}
