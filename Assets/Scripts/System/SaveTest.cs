// SaveTest.cs
using UnityEngine;

public class SaveTest : MonoBehaviour
{
    public void SaveAndTest()
    {
        PlayerData data = SaveManager.LoadPlayer();
        Debug.Log($"����: {data.currentLevel}, ����ġ: {data.currentExp}");

        data.currentLevel++;
        data.currentExp += 50;
        data.learnedSkills["skill_damage_boost"] = 2;

        SaveManager.SavePlayer(data);
    }
}
