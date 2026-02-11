using InventorySystem.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InventorySystem.UI
{
    public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text quantityText;
        [SerializeField] private Image highlight;

        private InventoryUI _inventoryUI;
        private ItemTooltipUI _tooltip;
        private int _index;
        private Canvas _canvas;

        public void Initialize(InventoryUI inventoryUI, int index, ItemTooltipUI tooltip)
        {
            _inventoryUI = inventoryUI;
            _index = index;
            _tooltip = tooltip;
            _canvas = GetComponentInParent<Canvas>();
            if (icon == null)
            {
                var iconTransform = transform.Find("Icon");
                if (iconTransform != null)
                {
                    icon = iconTransform.GetComponent<Image>();
                }
            }
            if (quantityText == null)
            {
                var qtyTransform = transform.Find("Qty");
                if (qtyTransform != null)
                {
                    quantityText = qtyTransform.GetComponent<Text>();
                }
            }
            Refresh();
        }

        public void Refresh()
        {
            var slot = _inventoryUI.GetSlot(_index);
            if (slot == null || slot.IsEmpty)
            {
                if (icon != null) icon.enabled = false;
                if (quantityText != null) quantityText.text = "";
                return;
            }

            if (icon != null)
            {
                icon.sprite = slot.Item.Icon;
                icon.enabled = slot.Item.Icon != null;
            }

            if (quantityText != null)
            {
                quantityText.text = slot.Quantity > 1 ? slot.Quantity.ToString() : "";
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            var slot = _inventoryUI.GetSlot(_index);
            if (slot == null || slot.IsEmpty)
            {
                return;
            }

            InventoryDragContext.BeginDrag(_index, slot.Item.Icon, _canvas);
        }

        public void OnDrag(PointerEventData eventData)
        {
            InventoryDragContext.UpdateDrag(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            InventoryDragContext.EndDrag();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!InventoryDragContext.IsDragging)
            {
                return;
            }

            _inventoryUI.HandleMove(InventoryDragContext.SourceIndex, _index);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                _inventoryUI.UseSlot(_index);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            var slot = _inventoryUI.GetSlot(_index);
            if (slot == null || slot.IsEmpty || _tooltip == null)
            {
                return;
            }

            _tooltip.Show(slot.Item, slot.Quantity, transform as RectTransform);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _tooltip?.Hide();
        }
    }
}
