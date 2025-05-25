// SaveTest.cs
using UnityEngine;

public class SaveTest : MonoBehaviour
{
    public void SaveAndTest()
    {
        PlayerData data = SaveManager.LoadPlayer();
        Debug.Log($"레벨: {data.currentLevel}, 경험치: {data.currentExp}");

        data.currentLevel++;
        data.currentExp += 50;
        data.learnedSkills["skill_damage_boost"] = 2;

        SaveManager.SavePlayer(data);
    }
}
