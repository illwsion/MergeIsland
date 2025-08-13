// PlayerSkillManager.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[DefaultExecutionOrder(-900)]
public class PlayerSkillManager : MonoBehaviour
{
    public static PlayerSkillManager Instance { get; private set; }

    private PlayerSaveData saveData;

    // 효과 누적 저장: 고정치(flat)와 퍼센트(percent)를 분리해서 관리
    private class AccumulatedEffect
    {
        public int flat;
        public int percent;
    }
    private Dictionary<(SkillData.SkillEffect, string), AccumulatedEffect> cachedSkillEffects = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // 필요하다면 유지
    }

    public void Initialize(PlayerSaveData data)
    {
        if (data == null)
        {
            // 방어: 빈 세이브 구조라도 만들어 둔다
            data = new PlayerSaveData
            {
                currentLevel = 1,
                currentExp = 0,
                skillPoints = 0,
                learnedSkills = new System.Collections.Generic.Dictionary<string, int>()
            };
        }
        saveData = data;
        RecalculateAllEffects();
    }

    public int GetSkillLevel(string skillKey)
    {
        if (string.IsNullOrEmpty(skillKey)) return 0;
        if (saveData == null || saveData.learnedSkills == null) return 0;
        return saveData.learnedSkills.TryGetValue(skillKey, out int level) ? level : 0;
    }

    public bool CanLearnSkill(string skillKey)
    {
        if (saveData == null) return false;
        var skill = SkillDataManager.Instance?.GetSkillData(skillKey);
        if (skill == null) return false;

        if (saveData.learnedSkills != null && saveData.learnedSkills.ContainsKey(skillKey)) return false; // 이미 배운 경우
        if (saveData.skillPoints < skill.costSkillPoint) return false;
        if (skill.unlockLevel > saveData.currentLevel) return false;

        // 선행 스킬 체크
        var requiredSkills = SkillRequireManager.Instance != null
            ? SkillRequireManager.Instance.GetRequiredSkills(skillKey)
            : null;
        if (requiredSkills != null)
        {
            foreach (var req in requiredSkills)
            {
                if (saveData.learnedSkills == null || !saveData.learnedSkills.ContainsKey(req))
                    return false;
            }
        }

        return true;
    }

    public bool LearnSkill(string skillKey)
    {
        if (!CanLearnSkill(skillKey)) return false;

        saveData.learnedSkills[skillKey] = 1;
        var skill = SkillDataManager.Instance.GetSkillData(skillKey);
        
        // PlayerLevelManager를 통해 스킬 포인트 차감 (자동으로 세이브 동기화)
        if (PlayerLevelManager.Instance != null)
        {
            PlayerLevelManager.Instance.SpendSkillPoints(skill.costSkillPoint);
        }
        else
        {
            // 폴백: 직접 차감
            saveData.skillPoints -= skill.costSkillPoint;
        }

        RecalculateAllEffects();
        
        // 스킬트리 UI 갱신
        var skillTree = FindFirstObjectByType<SkillTreeUI>();
        skillTree?.OnSkillPointsChanged();
        
        // 아이템 정보 UI 갱신 (선택된 아이템이 있다면 즉시 재표시하여 효과 리스트 재구성)
        var selector = ItemSelectorManager.Instance;
        var selectedItem = selector != null ? selector.GetSelectedItem() : null;
        if (selectedItem != null)
        {
            selector.itemInfoUI?.Show(selectedItem);
        }
        
        Debug.Log($"[PlayerSkillManager] 스킬 {skillKey} 습득 완료");
        return true;
    }

    // 하위 호환: 기존 호출부는 flat(고정치) 값을 반환
    public int GetEffectValue(SkillData.SkillEffect effect, string targetKey)
    {
        return GetEffectFlat(effect, targetKey);
    }

    public int GetEffectFlat(SkillData.SkillEffect effect, string targetKey)
    {
        if (cachedSkillEffects.TryGetValue((effect, targetKey), out var acc))
            return acc.flat;
        return 0;
    }

    public int GetEffectPercent(SkillData.SkillEffect effect, string targetKey)
    {
        if (cachedSkillEffects.TryGetValue((effect, targetKey), out var acc))
            return acc.percent;
        return 0;
    }

    private void RecalculateAllEffects()
    {
        cachedSkillEffects.Clear();

        if (saveData?.learnedSkills == null) return;

        foreach (var pair in saveData.learnedSkills)
        {
            var skill = SkillDataManager.Instance.GetSkillData(pair.Key);
            if (skill == null) continue;

            var key = (skill.skillEffect, skill.targetKey);
            if (!cachedSkillEffects.TryGetValue(key, out var acc))
            {
                acc = new AccumulatedEffect();
                cachedSkillEffects[key] = acc;
            }

            // isPercent에 따라 누적 방식 분리 (덧셈 방식)
            if (skill.isPercent)
            {
                acc.percent += skill.skillEffectValue;
            }
            else
            {
                acc.flat += skill.skillEffectValue;
            }
        }
        
        // 스킬 효과가 변경되었으므로 자원 관련 UI 갱신
        if (PlayerResourceManager.Instance != null)
        {
            // 모든 자원 타입의 UI 갱신
            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            {
                if (type != ResourceType.None && type != ResourceType.Exp)
                {
                    PlayerResourceManager.Instance.UpdateUI(type);
                }
            }
        }
        
        Debug.Log($"[PlayerSkillManager] 스킬 효과 재계산 완료. 총 {cachedSkillEffects.Count}개 효과 적용됨");
    }

    public IEnumerable<SkillData> GetAllLearnedSkills()
    {
        if (saveData == null || saveData.learnedSkills == null)
            yield break;

        foreach (var key in saveData.learnedSkills.Keys)
        {
            var data = SkillDataManager.Instance?.GetSkillData(key);
            if (data != null)
                yield return data;
        }
    }
    
    // 디버그: 현재 적용된 모든 스킬 효과 출력
    public void DebugPrintAllEffects()
    {
        Debug.Log("=== 현재 적용된 스킬 효과 ===");
        foreach (var kv in cachedSkillEffects)
        {
            Debug.Log($"효과: {kv.Key.Item1}, 대상: {kv.Key.Item2}, flat: {kv.Value.flat}, percent: {kv.Value.percent}%");
        }
        Debug.Log("================================");
    }
    
    // 특정 효과의 현재 값 조회 (디버그용)
    public int GetEffectValueDebug(SkillData.SkillEffect effect, string targetKey)
    {
        int flat = GetEffectFlat(effect, targetKey);
        int percent = GetEffectPercent(effect, targetKey);
        Debug.Log($"[PlayerSkillManager] {effect} - {targetKey}: flat={flat}, percent={percent}%");
        return flat + percent; // 단순 조회용
    }
}
