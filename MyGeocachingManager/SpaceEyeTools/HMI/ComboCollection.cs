using System;
using System.Collections;
using System.Windows.Forms;

namespace SpaceEyeTools.EXControls
{

    /// <summary>
    /// Collections of ComboBoxItem.
    /// </summary>
    /// <typeparam name="TComboBoxItem">ComboBoxItem.</typeparam>
    public class ComboCollection<TComboBoxItem> : CollectionBase
    {


        /// <summary>
        /// EvenHandler called to update items
        /// </summary>
        public EventHandler UpdateItems;

        /// <summary>
        /// Get / Set items collection
        /// </summary>
        public ComboBox.ObjectCollection ItemsBase { get; set; }

        /// <summary>
        /// Accessor
        /// </summary>
        /// <param name="index">item index</param>
        /// <returns>ComboBoxItem at specified index</returns>
        public ComboBoxItem this[int index]
        {
            get
            {
                return ((ComboBoxItem)ItemsBase[index]);
            }
            set
            {
                ItemsBase[index] = value;
            }
        }

        /// <summary>
        /// Add an item
        /// </summary>
        /// <param name="value">new item</param>
        /// <returns>item index</returns>
        public int Add(ComboBoxItem value)
        {
            var  result =  ItemsBase.Add(value);
            UpdateItems.Invoke(this, null);
            return result;
        }

        /// <summary>
        /// Returns index of an item
        /// </summary>
        /// <param name="value">item</param>
        /// <returns>item index</returns>
        public int IndexOf(ComboBoxItem value)
        {
            return (ItemsBase.IndexOf(value));
        }

        /// <summary>
        /// Insert an item at a specific position
        /// </summary>
        /// <param name="index">position</param>
        /// <param name="value">item</param>
        public void Insert(int index, ComboBoxItem value)
        {
            ItemsBase.Insert(index, value);
            UpdateItems.Invoke(this, null);
        }

        /// <summary>
        /// Remove an item
        /// </summary>
        /// <param name="value">item to remove</param>
        public void Remove(ComboBoxItem value)
        {
            ItemsBase.Remove(value);
            UpdateItems.Invoke(this, null);
        }

        /// <summary>
        /// Check if item is contained
        /// </summary>
        /// <param name="value">item</param>
        /// <returns>true if item is contained</returns>
        public bool Contains(ComboBoxItem value)
        {
            return (ItemsBase.Contains(value));
        }

    }

}
