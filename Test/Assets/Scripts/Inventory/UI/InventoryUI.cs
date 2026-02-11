using System.Collections.Generic;
using InventorySystem.Core;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace InventorySystem.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private InventoryManager manager;
        [SerializeField] private Transform gridRoot;
        [SerializeField] private InventorySlotUI slotPrefab;
        [SerializeField] private ItemTooltipUI tooltip;
        [SerializeField] private KeyCode toggleKey = KeyCode.I;

        private readonly List<InventorySlotUI> _slots = new List<InventorySlotUI>();

        private void Start()
        {
            if (manager == null)
            {
                manager = InventoryManager.Instance;
            }
            if (gridRoot == null)
            {
                var grid = transform.Find("SlotGrid");
                if (grid != null) gridRoot = grid;
            }
            if (tooltip == null)
            {
                var t = transform.Find("ItemTooltip");
                if (t != null) tooltip = t.GetComponent<ItemTooltipUI>();
            }

            BuildSlots();
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            var key = ConvertKeyCode(toggleKey);
            if (Keyboard.current != null && key != Key.None && Keyboard.current[key].wasPressedThisFrame)
#else
            if (Input.GetKeyDown(toggleKey))
#endif
            {
                gameObject.SetActive(!gameObject.activeSelf);
            }
        }

#if ENABLE_INPUT_SYSTEM
        private static Key ConvertKeyCode(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.I: return Key.I;
                case KeyCode.Tab: return Key.Tab;
                case KeyCode.Escape: return Key.Escape;
                case KeyCode.Space: return Key.Space;
                case KeyCode.Return: return Key.Enter;
                case KeyCode.Backspace: return Key.Backspace;
                case KeyCode.Alpha1: return Key.Digit1;
                case KeyCode.Alpha2: return Key.Digit2;
                case KeyCode.Alpha3: return Key.Digit3;
                case KeyCode.Alpha4: return Key.Digit4;
                case KeyCode.Alpha5: return Key.Digit5;
                case KeyCode.Alpha6: return Key.Digit6;
                case KeyCode.Alpha7: return Key.Digit7;
                case KeyCode.Alpha8: return Key.Digit8;
                case KeyCode.Alpha9: return Key.Digit9;
                case KeyCode.Alpha0: return Key.Digit0;
                case KeyCode.A: return Key.A;
                case KeyCode.B: return Key.B;
                case KeyCode.C: return Key.C;
                case KeyCode.D: return Key.D;
                case KeyCode.E: return Key.E;
                case KeyCode.F: return Key.F;
                case KeyCode.G: return Key.G;
                case KeyCode.H: return Key.H;
                case KeyCode.J: return Key.J;
                case KeyCode.K: return Key.K;
                case KeyCode.L: return Key.L;
                case KeyCode.M: return Key.M;
                case KeyCode.N: return Key.N;
                case KeyCode.O: return Key.O;
                case KeyCode.P: return Key.P;
                case KeyCode.Q: return Key.Q;
                case KeyCode.R: return Key.R;
                case KeyCode.S: return Key.S;
                case KeyCode.T: return Key.T;
                case KeyCode.U: return Key.U;
                case KeyCode.V: return Key.V;
                case KeyCode.W: return Key.W;
                case KeyCode.X: return Key.X;
                case KeyCode.Y: return Key.Y;
                case KeyCode.Z: return Key.Z;
                default: return Key.None;
            }
        }
#endif

        private void BuildSlots()
        {
            if (manager == null || manager.Inventory == null || gridRoot == null || slotPrefab == null)
            {
                return;
            }

            _slots.Clear();
            foreach (Transform child in gridRoot)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < manager.Inventory.Slots.Count; i++)
            {
                var slotUi = Instantiate(slotPrefab, gridRoot);
                slotUi.Initialize(this, i, tooltip);
                _slots.Add(slotUi);
            }

            manager.Inventory.OnSlotUpdated += HandleSlotUpdated;
            RefreshAll();
        }

        private void HandleSlotUpdated(int index)
        {
            if (index < 0 || index >= _slots.Count)
            {
                return;
            }

            _slots[index].Refresh();
        }

        private void RefreshAll()
        {
            foreach (var slot in _slots)
            {
                slot.Refresh();
            }
        }

        public void HandleMove(int fromIndex, int toIndex)
        {
            if (manager == null || manager.Inventory == null)
            {
                return;
            }

            manager.Inventory.Move(fromIndex, toIndex);
        }

        public InventorySlot GetSlot(int index)
        {
            return manager != null ? manager.Inventory.GetSlot(index) : null;
        }

        public void UseSlot(int index)
        {
            manager?.UseItem(index);
        }
    }
}
