/*
 * 
 * 
 * Provides a relatively easy way to implement an ordered list interface in Unity
 * 
 * Does not behave in an ideal way in all situations. But it suits my needs for now. 
 * 
 * 
 * HOW TO:
 * ------
 * 
 * Derive a class from ViewList with the type parameter as your value type
 * You can attach that script to a GameObject in the unity editor.
 * The state and order of the items is then managed. See example for more info.
 * 
 * (C) 2019 - Bart van de Sande (Nonline)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityViewList
{
    public abstract class ViewList<T> : MonoBehaviour where T : class
    {
        /// <summary>
        /// All items in the list will be placed as children of this transform
        /// </summary>
        public Transform Container;
        public UnityEvent OnSelectionChange;

        private readonly List<Item> items = new List<Item>();
        private bool _multiSelect;

        /// <summary>
        /// Needs to be overriden to instantiate item prefab
        /// </summary>
        public abstract Item InstantiateNewItem();

        /// <summary>
        /// Instantiates & intializes an item
        /// </summary>
        private Item CreateItem(T value)
        {
            var item = InstantiateNewItem();

            // Register this as the listview container
            // and set the value
            item.Initialize(this, value);

            // Call initialize so that client can set UI to correct values
            item.OnInitialize();

            return item;
        }

        /// <summary>
        /// Is it allowed to selected more than one item?
        /// </summary>
        public bool CanMultiselect
        {
            get
            {
                return _multiSelect;
            }
            set
            {
                _multiSelect = value;
                if (!value)
                {
                    // Unselected multiples
                    bool oneWasSelected = false;
                    foreach (var item in items)
                    {
                        if (oneWasSelected && item.Selected)
                            item.Selected = false;
                        else
                            oneWasSelected |= item.Selected;
                    }
                }
            }
        }

        /// <summary>
        /// Both Canmultiselect and this should return true for multiselect to happen
        /// You can override this with different conditions
        /// </summary>
        /// <value><c>true</c> if should multiselect; otherwise, <c>false</c>.</value>
        public virtual bool ShouldMultiselectRange =>
                Input.GetKey(KeyCode.LeftShift) ||
                Input.GetKey(KeyCode.RightShift);

        /// <summary>
        /// Both Canmultiselect and this should return true for multiselect to happen
        /// You can override this with different conditions
        /// </summary>
        /// <value><c>true</c> if should multiselect; otherwise, <c>false</c>.</value>
        public virtual bool ShouldMultiselect =>
                Input.GetKey(KeyCode.LeftCommand) ||
                Input.GetKey(KeyCode.RightCommand);


        /// <summary>
        /// Returns true if multiple items are selected
        /// </summary>
        public bool HasMultipleSelected
        {
            get
            {
                if (!CanMultiselect)
                    return false;

                bool one = false;
                foreach (var item in items)
                {
                    if (item.Selected)
                    {
                        if (one)
                            return true;
                        one = true;
                    }
                }

                return false;
            }
        }


        /// <summary>
        /// Returns all values in the way they are ordered in the list
        /// </summary>
        public IEnumerable<T> Values
        {
            get
            {
                foreach (var item in items)
                {
                    yield return item.Value;
                }
            }
        }

        // https://stackoverflow.com/questions/5110403/class-with-indexer-and-property-named-item
        [System.Runtime.CompilerServices.IndexerName("MyItem")]
        public T this[int index]
        {
            get
            {
                return items[index].Value;
            }
        }


        /// <summary>
        /// Returns the first selected value
        /// </summary>
        public T GetSelectedValue() 
        {
            foreach (var item in items)
            {
                if (item.Selected)
                    return item.Value;
            }

            return default;
        }

        /// <summary>
        /// Returns the index of the item that is selected, -1 if none
        /// </summary>
        public int GetSelectedIndex()
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Selected)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Returns all selected items
        /// </summary>
        public IEnumerable GetSelectedValues()
        {
            foreach (var item in items)
            {
                if (item.Selected)
                    yield return item.Value;
            }
        }

        /// <summary>
        /// Returns how many items there are
        /// </summary>
        public int Count => items.Count;

        /// <summary>
        /// Appends an item to the list view
        /// </summary>
        public void AddNew(T value)
        {
            Insert(items.Count - 1, value);
        }

        /// <summary>
        /// Inserts item at index in the list view
        /// </summary>
        public void Insert(int index, T value)
        {
            if (this.ContainsValue(value))
                throw new Exception("Duplicate items are not allowed");

            int clampedIndex = Mathf.Clamp(index, 0, items.Count - 1);
            var item = CreateItem(value);

            item.transform.SetSiblingIndex(clampedIndex);
            items.Insert(clampedIndex, item);
        }

        /// <summary>
        /// Returns if a value is contained within this list
        /// </summary>
        public bool ContainsValue(T value)
        {
            foreach (var item in items)
            {
                if (item.Value == value)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Clears the list
        /// </summary>
        public void Clear()
        {
            for (int i = items.Count - 1; i >= 0; i--)
                Destroy(items[i].gameObject);
        }

        /// <summary>
        /// Removes item from listview
        /// </summary>
        public bool Remove(T value)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (items[i].Value == value)
                {
                    Destroy(items[i].gameObject);

                    //items.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes all selected items
        /// </summary>
        public void RemoveSelected()
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (items[i].Selected)
                    Remove(items[i].Value);
            }
        }

        /// <summary>
        /// Moves selected items up or down x number of steps
        /// </summary>
        public void MoveSelected(int value)
        {

            // Returns true when edge is reached
            bool move(Item item, int oldIndex, int newIndex)
            {
                int newIndexClamped = Mathf.Clamp(newIndex, 0, items.Count - 1);

                items.RemoveAt(oldIndex);
                items.Insert(newIndexClamped, item);
                item.transform.SetSiblingIndex(newIndexClamped);

                // When we're over an edge, return true to prevent shifting when multiple items are selected
                return newIndex < 0 || newIndex > items.Count - 1;
            }

            if (value < 0)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].Selected)
                    {
                        if (move(items[i], i, i + value))
                            break; // stop moving when an item reaches edge (in case of multiple selected items)
                    }
                }
            }
            else if (value > 0)
            {
                for (int i = items.Count - 1; i >= 0; i--)
                {
                    if (items[i].Selected)
                    {
                        if (move(items[i], i, i + value))
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Selects all items, can only be done when multiselect is allowed
        /// </summary>
        public void SelectAll()
        {
            if (!CanMultiselect)
                return;

            foreach (var item in items)
                item.SetSelectedWithoutFeedback(true);
        }

        /// <summary>
        /// Unselects all items
        /// </summary>
        public void UnselectAll()
        {
            foreach (var item in items)
                item.Selected = false;

        }

        /// <summary>
        /// Selects item at index
        /// </summary>
        public void SelectIndex(int index)
        {
            items[index].Selected = true;
        }


        #region Internal

        /*
         * 
         * Functions get called by items in the list
         * so that other items get unselected etc...       
         * 
         */

        internal void OnItemSelectionChange(Item item, bool value)
        {

            if (!CanMultiselect || 
                (!ShouldMultiselectRange && !ShouldMultiselect))
            {

                // Unselect all except item is it's selected or there were multiple items selected
                if (HasMultipleSelected || item.Selected)
                {
                    foreach (var otherItem in items)
                    {
                        if (otherItem != item)
                            otherItem.SetSelectedWithoutFeedback(false);
                    }
                }

            }
            else if (ShouldMultiselectRange)
            {
                // NOTE:
                // range selection does not work exactly the same as on MacOS, where it remebers the first item selected
                // possible future improvement

                // Range selection condition is in effect
                int oneIndex = items.IndexOf(item);
                int rangeStart = -1;
                int rangeEnd = -1;

                for (int i = 0; i < items.Count; i++)
                {
                    if (!items[i].Selected)
                        continue;

                    if (rangeStart == -1)
                        rangeStart = i;
                    else
                        rangeEnd = Mathf.Max(rangeEnd, i);
                }


                if (rangeStart >= 0 && rangeEnd >= 0)
                {
                    // We have a range, select all of them
                    for (int i = rangeStart; i <= rangeEnd; i++)
                        items[i].SetSelectedWithoutFeedback(true);
                }

            }

            // Fire event
            OnSelectionChange?.Invoke();
        }

        internal void OnItemDestroy(Item item)
        {
            items.Remove(item);
        }

        #endregion

        /// <summary>
        /// Override this class with your implementation of an item with UI and a custom value
        /// </summary>
        public abstract class Item : MonoBehaviour
        {
            public T Value { get; set; }

            internal void Initialize(ViewList<T> listView, T value)
            {
                if (this.Value != null)
                    throw new Exception("A ViewList item can only be initialized once");

                this.Value = value;
                this.listView = listView;
                this.transform.SetParent(listView.Container);
            }

            private ViewList<T> listView;
            private bool _selected; // operated on directly when selection changes to prevent reinvocation of selected event

            /// <summary>
            /// When this changes the listview get's notified
            /// </summary>
            public bool Selected
            {
                get
                {
                    return _selected;
                }
                set
                {
                    if (value != _selected)
                    {
                        _selected = value;

                        listView.OnItemSelectionChange(this, value);
                        this.OnSelectionChange(_selected);
                    }
                }
            }

            /// <summary>
            /// Sets selected value but does not feed it back to the listview. Does raise the event for the item.
            /// </summary>
            internal void SetSelectedWithoutFeedback(bool value)
            {
                if (value != _selected)
                {
                    _selected = value;
                    this.OnSelectionChange(_selected);
                }
            }

            /// <summary>
            /// When an item is removed it is destroyed. If you have other
            /// work to do in OnDestroy, make sure to call base.OnDestroy()
            /// </summary>
            protected virtual void OnDestroy()
            {
                this.listView.OnItemDestroy(this);
            }

            /// <summary>
            /// Implement setting this item's UI to the value
            /// </summary>
            public abstract void OnInitialize();

            /// <summary>
            /// Implement what should happen to this item when it's selection status changes
            /// </summary>
            public abstract void OnSelectionChange(bool selected);
        }


    }
}