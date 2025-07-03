// MenuManager.cs
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("�гε�")]
    [SerializeField] private SkillTreeUI skillTreeUI;
    public GameObject skillPanel;
    public GameObject craftPanel;
    public GameObject shopPanel;

    [Header("�ϴ� ��ư �����յ�")]
    public GameObject mainTabPrefab;
    public GameObject skillTabPrefab;
    //public GameObject craftTabPrefab;
    //public GameObject shopTabPrefab;

    [Header("�ϴ� ��ư �����̳�")]
    public Transform bottomTabContainer;

    private void Start()
    {
        OpenMainPanel(); // ���� �� �⺻ ����
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
        ShowOnly(null); // ��� �г� ��Ȱ��ȭ
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
        // ���� ��ư ����
        foreach (Transform child in bottomTabContainer)
        {
            Destroy(child.gameObject);
        }

        // �� ��ư �߰�
        Instantiate(tabPrefab, bottomTabContainer);
    }
}
