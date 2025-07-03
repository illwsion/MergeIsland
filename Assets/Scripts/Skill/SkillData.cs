// SkillData.cs

using UnityEngine.UIElements;

public class SkillData
{
    public string key;
    public SkillCategory category;
    public SkillTag tag;

    public int coordX;
    public int coordY;

    public int costSkillPoint;
    public ResourceType costResourceType;
    public int costResourceValue;

    public int unlockLevel;
    public string unlockBoardKey;


    public SkillEffect skillEffect;
    public string targetKey;
    public int skillEffectValue;
    public bool isPercent;

    public string skillNameKey;
    public string skillDescKey;
    public string imageName;

    public enum SkillCategory
    {
        Normal,
        Ascention
    }

    public enum SkillTag
    {
        Combat,
        Production,
        Utility
    }

    public enum SkillEffect
    {
        DamageAdd,
        ResourceGain,
        ResourceCap,
        CooldownReduce,
        UnlockFeature
    }
}
