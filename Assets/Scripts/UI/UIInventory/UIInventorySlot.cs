using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    Camera mainCamera;
    Canvas parentCanvas;
    Transform parentItem;
    GameObject draggedItem;

    public Image inventorySlotHighlight;
    public Image inventorySlotImage;
    public TextMeshProUGUI textMeshProUGUI;

    [SerializeField] UIInventoryBar inventoryBar = null;
    [SerializeField] GameObject inventoryTextBoxPrefab = null;
    [HideInInspector] public bool isSelected = false;
    [HideInInspector] public ItemDetails itemDetails;
    [SerializeField] GameObject itemPrefab = null;
    [HideInInspector] public int itemQuantity;
    [SerializeField] int slotNumber = 0;

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        mainCamera = Camera.main;
        parentItem = GameObject.FindGameObjectWithTag(Tags.ItemsParentTransform).transform;
    }

    void DropSelectedItemAtMousePosition()
    {
        if (itemDetails == null || !isSelected) return;

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, 
                                                                            Input.mousePosition.y, 
                                                                            -mainCamera.transform.position.z));

        GameObject itemGameObject = Instantiate(itemPrefab, worldPosition, Quaternion.identity, parentItem);
        Item item = itemGameObject.GetComponent<Item>();
        item.ItemCode = itemDetails.itemCode;

        InventoryManager.Instance.RemoveItem(InventoryLocation.player, item.ItemCode);

        if(InventoryManager.Instance.FindItemInInventory(InventoryLocation.player, item.ItemCode) == -1)
        {
            ClearSelectedItem();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemDetails == null) return;

        Player.Instance.DisablePlayerInputAndResetMovement();

        draggedItem = Instantiate(inventoryBar.inventoryBarDraggedItem, inventoryBar.transform);

        Image draggedItemImage = draggedItem.GetComponentInChildren<Image>();
        draggedItemImage.sprite = inventorySlotImage.sprite;

        SetSelectedItem();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedItem == null) return;

        draggedItem.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedItem == null) return;

        Destroy(draggedItem);

        if(eventData.pointerCurrentRaycast.gameObject != null 
            && eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>() != null)
        {
            int toSlotNumber = eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>().slotNumber;

            InventoryManager.Instance.SwapInventoryItems(InventoryLocation.player, slotNumber, toSlotNumber);

            DestroyInventoryTextBox();

            ClearSelectedItem();
        }
        else
        {
            if (itemDetails.canBeDropped)
            {
                DropSelectedItemAtMousePosition();
            }
        }

        Player.Instance.EnablePlayerInput();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Populate text box with item details
        if (itemQuantity == 0) return;

        //Instantiate inventory text box
        inventoryBar.inventoryTextBoxGameobject = Instantiate(inventoryTextBoxPrefab, transform.position, Quaternion.identity);
        inventoryBar.inventoryTextBoxGameobject.transform.SetParent(parentCanvas.transform, false);

        UIInventoryTextBox inventoryTextBox = inventoryBar.inventoryTextBoxGameobject.GetComponent<UIInventoryTextBox>();

        // Set item type description
        string itemTypeDescription = InventoryManager.Instance.GetItemTypeDescription(itemDetails.itemType);

        // Populate text box
        inventoryTextBox.SetTextboxText(itemDetails.itemDescription, itemTypeDescription, "", itemDetails.itemLongDescription, "", "");

       // Set text box position according to inventory bar position
        if (inventoryBar.IsInventoryBarPositionBottom)
        {
            inventoryBar.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
            inventoryBar.inventoryTextBoxGameobject.transform.position = new Vector3(transform.position.x,
                                                                                        transform.position.y + 50f,
                                                                                        transform.position.z);
        }
        else 
        { 

            inventoryBar.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
            inventoryBar.inventoryTextBoxGameobject.transform.position = new Vector3(transform.position.x,
                                                                                        transform.position.y - 50f,
                                                                                        transform.position.z);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DestroyInventoryTextBox();
    }

    public void DestroyInventoryTextBox()
    {
        if(inventoryBar.inventoryTextBoxGameobject != null)
        {
            Destroy(inventoryBar.inventoryTextBoxGameobject);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            if(isSelected == true)
            {
                ClearSelectedItem();
            }
            else
            {
                if(itemQuantity > 0)
                {
                    SetSelectedItem();
                }
            }
        }
    }

    private void SetSelectedItem()
    {
        inventoryBar.ClearHighlightOnInventorySlots();

        isSelected = true;

        inventoryBar.SetHighlightedInventorySlots();

        InventoryManager.Instance.SetSelectedInventoryItem(InventoryLocation.player, itemDetails.itemCode);

        if(itemDetails.canBeCarried == true)
        {
            // Show player carrying item
            Player.Instance.ShowCarriedItem(itemDetails.itemCode);
        }
        else
        {
            Player.Instance.ClearCarriedItem();
        }
    }

    private void ClearSelectedItem()
    {
        inventoryBar.ClearHighlightOnInventorySlots();

        isSelected = false;

        InventoryManager.Instance.ClearSelectedInventoryItem(InventoryLocation.player);

        Player.Instance.ClearCarriedItem();
    }
}
