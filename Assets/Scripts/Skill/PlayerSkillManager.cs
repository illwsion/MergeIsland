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
        
        // 레벨이 0 이하인 경우 1로 보정
        if (data.currentLevel <= 0)
        {
            Debug.LogWarning($"[PlayerSkillManager] 레벨이 {data.currentLevel}에서 1로 보정됨");
            data.currentLevel = 1;
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
    
    public bool IsSkillLearned(string skillKey)
    {
        return GetSkillLevel(skillKey) > 0;
    }
    
    public int GetCurrentLevel()
    {
        // SaveController를 우선적으로 사용
        if (SaveController.Instance?.CurrentSave?.player != null)
        {
            int level = SaveController.Instance.CurrentSave.player.currentLevel;
            // 레벨이 0 이하인 경우 1로 보정
            if (level <= 0)
            {
                level = 1;
                SaveController.Instance.CurrentSave.player.currentLevel = level;
                Debug.LogWarning($"[PlayerSkillManager] SaveController에서 레벨이 {level}로 보정됨");
            }
            return level;
        }
        
        // 폴백: saveData 사용
        return saveData?.currentLevel ?? 1;
    }

    public bool CanLearnSkill(string skillKey)
    {
        if (saveData == null) 
        {
            Debug.LogWarning($"[PlayerSkillManager] CanLearnSkill({skillKey}) 실패: saveData가 null");
            return false;
        }
        
        var skill = SkillDataManager.Instance?.GetSkillData(skillKey);
        if (skill == null) 
        {
            Debug.LogWarning($"[PlayerSkillManager] CanLearnSkill({skillKey}) 실패: SkillData를 찾을 수 없음");
            return false;
        }

        if (saveData.learnedSkills != null && saveData.learnedSkills.ContainsKey(skillKey)) 
        {
            Debug.LogWarning($"[PlayerSkillManager] CanLearnSkill({skillKey}) 실패: 이미 배운 스킬");
            return false;
        }
        
        if (saveData.skillPoints < skill.costSkillPoint) 
        {
            Debug.LogWarning($"[PlayerSkillManager] CanLearnSkill({skillKey}) 실패: 스킬포인트 부족 (필요: {skill.costSkillPoint}, 보유: {saveData.skillPoints})");
            return false;
        }
        
        if (skill.unlockLevel > saveData.currentLevel) 
        {
            Debug.LogWarning($"[PlayerSkillManager] CanLearnSkill({skillKey}) 실패: 레벨 부족 (필요: {skill.unlockLevel}, 현재: {saveData.currentLevel})");
            return false;
        }

        // 선행 스킬 체크
        var requiredSkills = SkillRequireManager.Instance != null
            ? SkillRequireManager.Instance.GetRequiredSkills(skillKey)
            : null;
            
        if (requiredSkills != null && requiredSkills.Count > 0)
        {
            foreach (var req in requiredSkills)
            {
                bool hasRequiredSkill = saveData.learnedSkills != null && saveData.learnedSkills.ContainsKey(req);
                
                if (!hasRequiredSkill)
                {
                    Debug.LogWarning($"[PlayerSkillManager] CanLearnSkill({skillKey}) 실패: 선행 스킬 {req} 미습득");
                    return false;
                }
            }
        }

        return true;
    }

    public bool LearnSkill(string skillKey)
    {
        // CanLearnSkill 체크
        bool canLearn = CanLearnSkill(skillKey);
        
        if (!canLearn) 
        {
            return false;
        }

        var skill = SkillDataManager.Instance.GetSkillData(skillKey);
        
        // SaveController를 통해 스킬 학습 상태 업데이트
        if (SaveController.Instance?.CurrentSave?.player != null)
        {
            var playerSave = SaveController.Instance.CurrentSave.player;
            
            // 스킬 학습 상태 추가
            if (playerSave.learnedSkills == null)
                playerSave.learnedSkills = new System.Collections.Generic.Dictionary<string, int>();
            
            playerSave.learnedSkills[skillKey] = 1;
            
            // PlayerSkillManager의 saveData도 동기화
            saveData.learnedSkills[skillKey] = 1;
        }
        else
        {
            Debug.LogWarning($"[PlayerSkillManager] SaveController를 통한 접근 실패, 직접 saveData 수정");
            saveData.learnedSkills[skillKey] = 1;
        }
        
        // PlayerLevelManager를 통해 스킬 포인트 차감 (자동으로 세이브 동기화)
        if (PlayerLevelManager.Instance != null)
        {
            PlayerLevelManager.Instance.SpendSkillPoints(skill.costSkillPoint);
        }
        else
        {
            // 폴백: SaveController를 통한 직접 차감
            if (SaveController.Instance?.CurrentSave?.player != null)
            {
                SaveController.Instance.CurrentSave.player.skillPoints -= skill.costSkillPoint;
            }
            else
            {
                // 최후의 폴백: 직접 차감
                saveData.skillPoints -= skill.costSkillPoint;
            }
        }

        RecalculateAllEffects();
        
        // 스킬트리 UI 갱신
        var skillTree = FindFirstObjectByType<SkillTreeUI>();
        if (skillTree != null)
        {
            skillTree.OnSkillPointsChanged();
        }
        else
        {
            Debug.LogWarning($"[PlayerSkillManager] SkillTreeUI를 찾을 수 없음");
        }
        
        // 화살표 UI 갱신
        var skillLinkUI = FindFirstObjectByType<SkillLinkUI>();
        if (skillLinkUI != null && skillTree != null)
        {
            skillLinkUI.RefreshArrows(skillTree.GetCurrentCategory());
        }
        else
        {
            Debug.LogWarning($"[PlayerSkillManager] SkillLinkUI 또는 SkillTreeUI를 찾을 수 없음");
        }
        
        // 아이템 정보 UI 갱신 (선택된 아이템이 있다면 즉시 재표시하여 효과 리스트 재구성)
        var selector = ItemSelectorManager.Instance;
        var selectedItem = selector != null ? selector.GetSelectedItem() : null;
        if (selectedItem != null)
        {
            selector.itemInfoUI?.Show(selectedItem);
        }
        
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
