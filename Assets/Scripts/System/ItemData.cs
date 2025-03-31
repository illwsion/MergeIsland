// ItemData.cs

using static NUnit.Framework.Internal.OSPlatform;

public class ItemData
{
    public int id;
    public string name;
    public string type; // "tree", "log" 등 그룹 용도
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
        Decoration // 단순 장식(UI 꾸미기용, 클릭 없음)
    };


    public enum ProduceType
    {
        None,
        Manual, //터치해서 아이템 생산
        Auto, // 자동 생산
        Gather, //터치해서 자원 획득(골드, 보석, 경험치 등)
        Dialogue //NPC 대화, 상호작용
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
    public int gatherValue; //획득량

    public float productionInterval; // 생산주기(초)
    public int maxProductionAmount; // 최대저장량

    public bool isSellable; // 판매가능여부
    public int sellValue; // 판매가격

    public int itemNameID; // 아이템 이름
    public int descriptionID; // 아이템 설명

    public bool canMove; // 이동여부
    public bool canInventoryStore; // 인벤토리보관여부

    
}
