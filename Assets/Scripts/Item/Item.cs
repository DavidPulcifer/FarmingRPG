using UnityEngine;

public class Item : MonoBehaviour
{
    [ItemCodeDescription]
    [SerializeField] int _itemCode;

    SpriteRenderer spriteRenderer;

    public int ItemCode { get => _itemCode; set => _itemCode = value; }

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if(_itemCode != 0)
        {
            Init(ItemCode);
        }
    }

    public void Init(int itemCodeParam)
    {
        if (itemCodeParam == 0) return;

        ItemCode = itemCodeParam;

        ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(ItemCode);

        spriteRenderer.sprite = itemDetails.itemSprite;

        if(itemDetails.itemType == ItemType.Reapable_scenery)
        {
            gameObject.AddComponent<ItemNudge>();
        }
    }
}
