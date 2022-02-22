using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInventoryBar : MonoBehaviour
{
    [SerializeField] Sprite blank16x16sprite = null;
    [SerializeField] UIInventorySlot[] inventorySlots = null;
    public GameObject inventoryBarDraggedItem;
    [HideInInspector] public GameObject inventoryTextBoxGameobject;
    
    RectTransform rectTransform;

    bool _isInventoryBarPositionBottom = true;

    public bool IsInventoryBarPositionBottom { get => _isInventoryBarPositionBottom; set => _isInventoryBarPositionBottom = value; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        EventsHandler.InventoryUpdatedEvent += InventoryUpdated;
    }

    private void OnDisable()
    {
        EventsHandler.InventoryUpdatedEvent -= InventoryUpdated;
    }

    private void Update()
    {
        SwitchInventoryBarPosition();
    }

    void ClearInventorySlots()
    {
        if (inventorySlots.Length == 0) return;

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].inventorySlotImage.sprite = blank16x16sprite;
            inventorySlots[i].textMeshProUGUI.text = "";
            inventorySlots[i].itemDetails = null;
            inventorySlots[i].itemQuantity = 0;
            SetHighlightedInventorySlots(i);
        }
    }

    void InventoryUpdated(InventoryLocation inventoryLocation, List<InventoryItem> inventoryList)
    {
        if(inventoryLocation == InventoryLocation.player)
        {
            ClearInventorySlots();

            if (inventorySlots.Length == 0 || inventoryList.Count == 0) return;
            
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (i >= inventoryList.Count) break;
                    
                int itemCode = inventoryList[i].itemCode;

                ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(itemCode);

                if (itemDetails == null) continue;
                
                inventorySlots[i].inventorySlotImage.sprite = itemDetails.itemSprite;
                inventorySlots[i].textMeshProUGUI.text = inventoryList[i].itemQuantity.ToString();
                inventorySlots[i].itemDetails = itemDetails;
                inventorySlots[i].itemQuantity = inventoryList[i].itemQuantity;
                SetHighlightedInventorySlots(i);

            }            
        }
    }
    
    private void SwitchInventoryBarPosition()
    {
        Vector3 playerViewportPosition = Player.Instance.GetPlayerViewPortPosition();

        if(playerViewportPosition.y > 0.3f && IsInventoryBarPositionBottom == false)
        {
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(0f, 2.5f);

            IsInventoryBarPositionBottom = true;
        }
        else if(playerViewportPosition.y <= 0.3f && IsInventoryBarPositionBottom == true)
        {
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(0f, -2.5f);

            IsInventoryBarPositionBottom = false;
        }
    }

    public void ClearHighlightOnInventorySlots()
    {
        if(inventorySlots.Length > 0)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventorySlots[i].isSelected)
                {
                    inventorySlots[i].isSelected = false;
                    inventorySlots[i].inventorySlotHighlight.color = new Color(0f, 0f, 0f, 0f);

                    InventoryManager.Instance.ClearSelectedInventoryItem(InventoryLocation.player);
                }
            }
        }
    }

    public void SetHighlightedInventorySlots()
    {
        if(inventorySlots.Length > 0)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                SetHighlightedInventorySlots(i);
            }
        }
    }

    public void SetHighlightedInventorySlots(int itemPosition)
    {
        if(inventorySlots.Length > 0 && inventorySlots[itemPosition].itemDetails != null)
        {
            if (inventorySlots[itemPosition].isSelected)
            {
                inventorySlots[itemPosition].inventorySlotHighlight.color = new Color(1f, 1f, 1f, 1f);

                InventoryManager.Instance.SetSelectedInventoryItem(InventoryLocation.player, inventorySlots[itemPosition].itemDetails.itemCode);
            }
        }
    }
}
