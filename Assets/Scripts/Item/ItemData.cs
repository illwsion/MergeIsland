// ItemData.cs

using static NUnit.Framework.Internal.OSPlatform;

public class ItemData
{
    public string key;
    public string name;
    public string type; // "tree", "log" 등 그룹 용도
    public Category category;
    public int level;
    public int maxLevel;

    public ProduceType produceType;
    public bool isProductionLimited; // 생산개수가 유한한가?
    public ToolType toolType; //도구타입
    public TargetMaterial targetMaterial; //피격타입
    public string produceTableKey; // 생산테이블
    public string dropTableKey; // 드랍테이블

    public ResourceType costResource; //소모 자원
    public int costValue; //소모량
    public ResourceType gatherResource; //획득 자원
    public int gatherValue; //획득량
    public ResourceType maxCapResource; //최대저장량 자원
    public int maxCapValue; //최대저장량보너스 수치

    public float productionInterval; // 생산주기(초)
    public int maxProductionAmount; // 최대저장량

    public bool isSellable; // 판매가능여부
    public int sellValue; // 판매가격

    public string itemNameKey; // 아이템 이름
    public string itemDescriptionKey; // 아이템 설명

    public bool canMove; // 이동여부
    public bool canInventoryStore; // 인벤토리보관여부

    public int hp; // 최대체력
    public int attackPower; // 공격력

    public string imageName; //이미지 이름

    public enum Category
    {
        Production,
        General,
        NPC,
        Tool,
        Weapon,
        Decoration // 단순 장식(UI 꾸미기용, 클릭 없음)
    };

    public enum ProduceType
    {
        None,
        Manual, //터치해서 아이템 생산
        Auto, // 자동 생산
        Supply, // 아이템을 제공하여 생산
        Gather, //터치해서 자원 획득(골드, 보석, 경험치 등)
        Dialogue //NPC 대화, 상호작용
    };
    public enum ToolType
    {
        None,
        Axe, //도끼
        Pickaxe, //곡괭이
        Weapon // 무기
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
