using UnityEngine;
using UnityEngine.UI;

namespace InventorySystem.UI
{
    public static class InventoryDragContext
    {
        public static bool IsDragging { get; private set; }
        public static int SourceIndex { get; private set; } = -1;

        private static GameObject _dragIcon;
        private static RectTransform _dragRect;

        public static void BeginDrag(int sourceIndex, Sprite icon, Canvas canvas)
        {
            EndDrag();

            if (icon == null || canvas == null)
            {
                return;
            }

            SourceIndex = sourceIndex;
            IsDragging = true;

            _dragIcon = new GameObject("InventoryDragIcon");
            _dragRect = _dragIcon.AddComponent<RectTransform>();
            _dragIcon.transform.SetParent(canvas.transform, false);
            var image = _dragIcon.AddComponent<Image>();
            image.sprite = icon;
            image.raycastTarget = false;
            _dragRect.sizeDelta = new Vector2(48f, 48f);
        }

        public static void UpdateDrag(Vector2 screenPosition)
        {
            if (!IsDragging || _dragRect == null)
            {
                return;
            }

            _dragRect.position = screenPosition;
        }

        public static void EndDrag()
        {
            IsDragging = false;
            SourceIndex = -1;

            if (_dragIcon != null)
            {
                Object.Destroy(_dragIcon);
            }

            _dragIcon = null;
            _dragRect = null;
        }
    }
}
