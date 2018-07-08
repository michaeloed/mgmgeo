using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceEyeTools.HMI
{
    /// <summary>
    /// Custom combobox item
    /// </summary>
    public class ComboItem
    {
        /// <summary>
        /// Get / Set item text
        /// </summary>
        public string Text { get; set; }
        
        /// <summary>
        /// Get / Set item value
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// ToString overrid
        /// </summary>
        /// <returns>Returns Text attribute</returns>
        public override string ToString()
        {
            return Text;
        }
    }
}
