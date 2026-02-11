using InventorySystem.Data;
using UnityEngine;
using UnityEngine.UI;

namespace InventorySystem.UI
{
    public class ItemTooltipUI : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text quantityText;
        [SerializeField] private Image icon;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (titleText == null)
            {
                var t = transform.Find("Title");
                if (t != null) titleText = t.GetComponent<Text>();
            }
            if (descriptionText == null)
            {
                var d = transform.Find("Desc");
                if (d != null) descriptionText = d.GetComponent<Text>();
            }
            if (quantityText == null)
            {
                var q = transform.Find("Qty");
                if (q != null) quantityText = q.GetComponent<Text>();
            }
            if (icon == null)
            {
                var i = transform.Find("Icon");
                if (i != null) icon = i.GetComponent<Image>();
            }
            Hide();
        }

        public void Show(ItemDefinition item, int quantity, RectTransform anchor)
        {
            if (item == null)
            {
                return;
            }

            if (titleText != null) titleText.text = item.DisplayName;
            if (descriptionText != null) descriptionText.text = item.Description;
            if (quantityText != null) quantityText.text = quantity > 1 ? $"x{quantity}" : "";
            if (icon != null)
            {
                icon.sprite = item.Icon;
                icon.enabled = item.Icon != null;
            }

            if (_rectTransform != null && anchor != null)
            {
                _rectTransform.position = anchor.position + new Vector3(140f, 0f, 0f);
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
