// OpenPanelButton.cs
using UnityEngine;

public class OpenPanelButton : MonoBehaviour
{
    public enum ButtonMode { OpenPanel, ChangeSkillTree }
    public ButtonMode mode;

    public enum PanelType { Main, Skill, Craft, Shop }
    public PanelType panelType;

    public enum SkillTreeType { Normal, Ascention }
    public SkillTreeType skillTreeType;

    public void OnClick()
    {
        switch (mode)
        {
            case ButtonMode.OpenPanel:
                HandlePanelOpen();
                break;

            case ButtonMode.ChangeSkillTree:
                HandleSkillTreeSwitch();
                break;
        }
    }

    private void HandlePanelOpen()
    {
        var manager = FindFirstObjectByType<MenuManager>();
        if (manager == null)
        {
            Debug.LogWarning("MenuManager를 찾을 수 없습니다.");
            return;
        }

        switch (panelType)
        {
            case PanelType.Main: manager.OpenMainPanel(); break;
            case PanelType.Skill: manager.OpenSkillPanel(); break;
            case PanelType.Craft: manager.OpenCraftPanel(); break;
            case PanelType.Shop: manager.OpenShopPanel(); break;
        }
    }

    private void HandleSkillTreeSwitch()
    {
        var skillTreeUI = FindFirstObjectByType<SkillTreeUI>();
        if (skillTreeUI == null) return;

        skillTreeUI.ChangeTree(skillTreeType.ToString()); // 또는 enum 직접 넘겨도 됨
    }
}

