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
    // �ʿ� �� �߰�
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
    public string label;     // ��� �� �ؽ�Ʈ (ex: "ȹ��")
    public Sprite icon1;      // ���� ������ �̹���
    public Sprite icon2;      // ���� ������ �̹���
    public string value;     // �ϴ� ��ġ �ؽ�Ʈ (ex: "+1000", "500", or null)

    public BoardGate sourceGate;
}
