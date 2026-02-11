using System;
using System.Collections.Generic;
using InventorySystem.Data;
using UnityEngine;

namespace InventorySystem.Core
{
    public class Inventory
    {
        private readonly List<InventorySlot> _slots;

        public event Action<ItemDefinition, int> OnItemAdded;
        public event Action<ItemDefinition, int> OnItemRemoved;
        public event Action<int, int> OnItemMoved;
        public event Action<int> OnSlotUpdated;

        public IReadOnlyList<InventorySlot> Slots => _slots;

        public Inventory(int slotCount)
        {
            if (slotCount <= 0)
            {
                slotCount = 1;
            }

            _slots = new List<InventorySlot>(slotCount);
            for (int i = 0; i < slotCount; i++)
            {
                _slots.Add(new InventorySlot());
            }
        }

        public bool HasFreeSlot => _slots.Exists(s => s.IsEmpty || s.RemainingCapacity() > 0);

        public int AddItem(ItemDefinition item, int quantity)
        {
            if (item == null || quantity <= 0)
            {
                return quantity;
            }

            int remaining = quantity;

            // Stack into existing slots first
            for (int i = 0; i < _slots.Count && remaining > 0; i++)
            {
                var slot = _slots[i];
                if (!slot.CanStackWith(item))
                {
                    continue;
                }

                int canAdd = Mathf.Min(slot.RemainingCapacity(), remaining);
                if (canAdd <= 0)
                {
                    continue;
                }

                slot.Quantity += canAdd;
                remaining -= canAdd;
                OnItemAdded?.Invoke(item, canAdd);
                OnSlotUpdated?.Invoke(i);
            }

            // Fill empty slots
            for (int i = 0; i < _slots.Count && remaining > 0; i++)
            {
                var slot = _slots[i];
                if (!slot.IsEmpty)
                {
                    continue;
                }

                int add = Mathf.Min(item.MaxStackSize, remaining);
                slot.Item = item;
                slot.Quantity = add;
                remaining -= add;
                OnItemAdded?.Invoke(item, add);
                OnSlotUpdated?.Invoke(i);
            }

            return remaining;
        }

        public bool RemoveAt(int slotIndex, int quantity)
        {
            if (!IsValidIndex(slotIndex) || quantity <= 0)
            {
                return false;
            }

            var slot = _slots[slotIndex];
            if (slot.IsEmpty)
            {
                return false;
            }

            var removedItem = slot.Item;
            int removed = Mathf.Min(quantity, slot.Quantity);
            slot.Quantity -= removed;

            if (slot.Quantity <= 0)
            {
                slot.Clear();
            }

            OnItemRemoved?.Invoke(removedItem, removed);
            OnSlotUpdated?.Invoke(slotIndex);
            return true;
        }

        public void Move(int fromIndex, int toIndex)
        {
            if (!IsValidIndex(fromIndex) || !IsValidIndex(toIndex) || fromIndex == toIndex)
            {
                return;
            }

            var from = _slots[fromIndex];
            var to = _slots[toIndex];

            if (from.IsEmpty)
            {
                return;
            }

            if (to.IsEmpty)
            {
                to.Item = from.Item;
                to.Quantity = from.Quantity;
                from.Clear();
            }
            else if (to.CanStackWith(from.Item))
            {
                int moveQty = Mathf.Min(from.Quantity, to.RemainingCapacity());
                if (moveQty > 0)
                {
                    to.Quantity += moveQty;
                    from.Quantity -= moveQty;
                    if (from.Quantity <= 0)
                    {
                        from.Clear();
                    }
                }
                else
                {
                    Swap(fromIndex, toIndex);
                }
            }
            else
            {
                Swap(fromIndex, toIndex);
            }

            OnItemMoved?.Invoke(fromIndex, toIndex);
            OnSlotUpdated?.Invoke(fromIndex);
            OnSlotUpdated?.Invoke(toIndex);
        }

        public void Swap(int a, int b)
        {
            if (!IsValidIndex(a) || !IsValidIndex(b) || a == b)
            {
                return;
            }

            var tempItem = _slots[a].Item;
            var tempQty = _slots[a].Quantity;

            _slots[a].Item = _slots[b].Item;
            _slots[a].Quantity = _slots[b].Quantity;

            _slots[b].Item = tempItem;
            _slots[b].Quantity = tempQty;
        }

        public void Clear()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                _slots[i].Clear();
                OnSlotUpdated?.Invoke(i);
            }
        }

        public InventorySlot GetSlot(int index)
        {
            if (!IsValidIndex(index))
            {
                return null;
            }
            return _slots[index];
        }

        public List<int> GetSlotsByCategory(InventorySystem.Data.ItemCategory category)
        {
            var indices = new List<int>();
            for (int i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot != null && !slot.IsEmpty && slot.Item.Category == category)
                {
                    indices.Add(i);
                }
            }
            return indices;
        }

        public void SortByName()
        {
            SortInternal((a, b) => string.Compare(a.Item.DisplayName, b.Item.DisplayName, System.StringComparison.Ordinal));
        }

        public void SortByQuantity()
        {
            SortInternal((a, b) => b.Quantity.CompareTo(a.Quantity));
        }

        private void SortInternal(System.Comparison<InventorySlot> comparison)
        {
            var occupied = new List<InventorySlot>();
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i] != null && !_slots[i].IsEmpty)
                {
                    occupied.Add(new InventorySlot { Item = _slots[i].Item, Quantity = _slots[i].Quantity });
                }
            }

            occupied.Sort(comparison);

            for (int i = 0; i < _slots.Count; i++)
            {
                _slots[i].Clear();
            }

            for (int i = 0; i < occupied.Count && i < _slots.Count; i++)
            {
                _slots[i].Item = occupied[i].Item;
                _slots[i].Quantity = occupied[i].Quantity;
            }

            for (int i = 0; i < _slots.Count; i++)
            {
                OnSlotUpdated?.Invoke(i);
            }
        }

        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < _slots.Count;
        }
    }
}
