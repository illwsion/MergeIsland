// SkillData.cs

public class SkillData
{
    public string key;
    public string group;
    public SkillCategory category;
    public int level;
    public int maxLevel;

    public int costSkillPoint;
    public ResourceType costResourceType;
    public int costResourceValue;

    public int unlockLevel;
    public string prerequisiteSkill1;
    public string prerequisiteSkill2;

    public SkillEffect skillEffect;
    public string targetKey;
    public int skillEffectValue;

    public string skillNameKey;
    public string skillDescKey;
    public string imageName;

    public enum SkillCategory
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
