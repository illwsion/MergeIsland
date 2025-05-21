// ItemData.cs

using static NUnit.Framework.Internal.OSPlatform;

public class ItemData
{
    public string key;
    public string name;
    public string type; // "tree", "log" �� �׷� �뵵
    public int level;
    public int maxLevel;
    public Category category;
    public ProduceType produceType;
    public bool isProductionLimited; // ���갳���� �����Ѱ�?
    public ToolType toolType;
    public TargetMaterial targetMaterial;
    public ResourceType costResource;
    public ResourceType gatherResource;

    public enum Category
    {
        Production,
        General,
        NPC,
        Tool,
        Weapon,
        Decoration // �ܼ� ���(UI �ٹ̱��, Ŭ�� ����)
    };

    public enum ProduceType
    {
        None,
        Manual, //��ġ�ؼ� ������ ����
        Auto, // �ڵ� ����
        Supply, // �������� �����Ͽ� ����
        Gather, //��ġ�ؼ� �ڿ� ȹ��(���, ����, ����ġ ��)
        Dialogue //NPC ��ȭ, ��ȣ�ۿ�
    };
    public enum ToolType
    {
        None,
        Axe, //����
        Pickaxe, //���
        Weapon // ����
    };
    public enum TargetMaterial
    {
        None,
        Wood,
        Stone,
        Iron,
        Monster,
        Animal
    };
    public string produceTableKey; // �������̺�
    public string dropTableKey; // ������̺�

    public int costValue;
    public int gatherValue; //ȹ�淮

    public float productionInterval; // �����ֱ�(��)
    public int maxProductionAmount; // �ִ����差

    public bool isSellable; // �ǸŰ��ɿ���
    public int sellValue; // �ǸŰ���

    public string itemNameKey; // ������ �̸�
    public string itemDescriptionKey; // ������ ����

    public bool canMove; // �̵�����
    public bool canInventoryStore; // �κ��丮��������

    public int hp; // �ִ�ü��
    public int attackPower; // ���ݷ�

    public string imageName; //�̹��� �̸�
    
}
