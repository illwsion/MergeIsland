// SaveTest.cs
using UnityEngine;

public class SaveTest : MonoBehaviour
{
    public void SaveAndTest()
    {
        GameSaveData save = SaveController.Instance.CurrentSave;
        Debug.Log($"����: {save.player.currentLevel}, ����ġ: {save.player.currentExp}");

        save.player.currentLevel++;
        save.player.currentExp += 50;
        save.player.learnedSkills["skill_damage_boost"] = 2;

        // ���� ����鵵 ����
        BoardManager.Instance.SaveAllBoards();

        SaveController.Instance.Save();
    }
}
