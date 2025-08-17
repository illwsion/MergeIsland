// MenuManager.cs
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("패널들")]
    [SerializeField] private SkillTreeUI skillTreeUI;
    public GameObject skillPanel;
    public GameObject craftPanel;
    public GameObject shopPanel;

    [Header("하단 버튼 프리팹들")]
    public GameObject mainTabPrefab;
    public GameObject skillTabPrefab;
    //public GameObject craftTabPrefab;
    //public GameObject shopTabPrefab;

    [Header("하단 버튼 컨테이너")]
    public Transform bottomTabContainer;

    private void Start()
    {
        OpenMainPanel(); // 시작 시 기본 열기
    }

    public void OpenSkillPanel()
    {
        ShowOnly(skillPanel);
        SetBottomTab(skillTabPrefab);
        skillTreeUI.ChangeTree("Normal");
    }

    public void OpenCraftPanel()
    {
        Debug.Log("OpenCraftPanel");
        ShowOnly(craftPanel);
        //SetBottomTab(craftTabPrefab);
    }

    public void OpenShopPanel()
    {
        Debug.Log("OpenShopPanel");
        ShowOnly(shopPanel);
        //SetBottomTab(shopTabPrefab);
    }

    public void OpenMainPanel()
    {
        ShowOnly(null); // 모든 패널 비활성화
        SetBottomTab(mainTabPrefab);
    }

    private void ShowOnly(GameObject panelToShow)
    {
        skillPanel.SetActive(false);
        craftPanel.SetActive(false);
        shopPanel.SetActive(false);

        if (panelToShow != null)
            panelToShow.SetActive(true);
    }

    private void SetBottomTab(GameObject tabPrefab)
    {
        // 기존 버튼 제거
        foreach (Transform child in bottomTabContainer)
        {
            Destroy(child.gameObject);
        }

        // 새 버튼 추가
        Instantiate(tabPrefab, bottomTabContainer);
    }
}