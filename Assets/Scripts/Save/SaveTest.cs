// SaveTest.cs
using UnityEngine;

public class SaveTest : MonoBehaviour
{
    public void SaveAndTest()
    {
        GameSaveData save = SaveController.Instance.CurrentSave;
        Debug.Log($"레벨: {save.player.currentLevel}, 경험치: {save.player.currentExp}");

        save.player.currentLevel++;
        save.player.currentExp += 50;
        save.player.learnedSkills["skill_damage_boost"] = 2;

        // 현재 보드들도 저장
        BoardManager.Instance.SaveAllBoards();

        SaveController.Instance.Save();
    }
}