using UnityEngine;
using InventorySystem.Actions;

namespace InventorySystem.Data
{
    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "Inventory/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id = "";
        [SerializeField] private string displayName = "";
        [TextArea(2, 4)]
        [SerializeField] private string description = "";

        [Header("Visuals")]
        [SerializeField] private Sprite icon;
        [SerializeField] private ItemCategory category;

        [Header("Stacking")]
        [SerializeField] private int maxStackSize = 1;

        [Header("World")]
        [SerializeField] private GameObject worldPrefab;
        [SerializeField] private GameObject pickupVfxPrefab;

        [Header("Use Action")]
        [SerializeField] private ItemActionBase useAction;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public ItemCategory Category => category;
        public int MaxStackSize => Mathf.Max(1, maxStackSize);
        public GameObject WorldPrefab => worldPrefab;
        public GameObject PickupVfxPrefab => pickupVfxPrefab;
        public ItemActionBase UseAction => useAction;
    }
}
