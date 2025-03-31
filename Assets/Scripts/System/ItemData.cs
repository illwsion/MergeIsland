// ItemData.cs

using static NUnit.Framework.Internal.OSPlatform;

public class ItemData
{
    public int id;
    public string name;
    public string type; // "tree", "log" �� �׷� �뵵
    public int level;
    public int maxLevel;
    public Category category;
    public ProduceType produceType;
    public CostResource costResource;
    public GatherResource gatherResource;

    public enum Category
    {
        Production,
        Collectable,
        NPC,
        Decoration // �ܼ� ���(UI �ٹ̱��, Ŭ�� ����)
    };


    public enum ProduceType
    {
        None,
        Manual, //��ġ�ؼ� ������ ����
        Auto, // �ڵ� ����
        Gather, //��ġ�ؼ� �ڿ� ȹ��(���, ����, ����ġ ��)
        Dialogue //NPC ��ȭ, ��ȣ�ۿ�
    }; 
    public int produceTableID; // ProduceTable/ID

    public enum CostResource
    {
        None,
        Energy,
        Gold,
        Wood
    }
    public int costValue;

    public enum GatherResource
    {
        None,
        Energy,
        Gold,
        Wood
    }
    public int gatherValue; //ȹ�淮

    public float productionInterval; // �����ֱ�(��)
    public int maxProductionAmount; // �ִ����差

    public bool isSellable; // �ǸŰ��ɿ���
    public int sellValue; // �ǸŰ���

    public int itemNameID; // ������ �̸�
    public int descriptionID; // ������ ����

    public bool canMove; // �̵�����
    public bool canInventoryStore; // �κ��丮��������

    
}
