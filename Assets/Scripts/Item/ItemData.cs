// ItemData.cs

using static NUnit.Framework.Internal.OSPlatform;

public class ItemData
{
    public string key;
    public string name;
    public string type; // "tree", "log" �� �׷� �뵵
    public Category category;
    public int level;
    public int maxLevel;

    public ProduceType produceType;
    public bool isProductionLimited; // ���갳���� �����Ѱ�?
    public ToolType toolType; //����Ÿ��
    public TargetMaterial targetMaterial; //�ǰ�Ÿ��
    public string produceTableKey; // �������̺�
    public string dropTableKey; // ������̺�

    public ResourceType costResource; //�Ҹ� �ڿ�
    public int costValue; //�Ҹ�
    public ResourceType gatherResource; //ȹ�� �ڿ�
    public int gatherValue; //ȹ�淮
    public ResourceType maxCapResource; //�ִ����差 �ڿ�
    public int maxCapValue; //�ִ����差���ʽ� ��ġ

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
    
    
}
