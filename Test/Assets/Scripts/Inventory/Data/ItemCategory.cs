using UnityEngine;

namespace InventorySystem.Data
{
    [CreateAssetMenu(fileName = "ItemCategory", menuName = "Inventory/Item Category")]
    public class ItemCategory : ScriptableObject
    {
        [SerializeField] private string id = "";
        [SerializeField] private string displayName = "";
        [SerializeField] private Sprite icon;
        [SerializeField] private Color uiColor = Color.white;

        public string Id => id;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public Color UIColor => uiColor;
    }
}
