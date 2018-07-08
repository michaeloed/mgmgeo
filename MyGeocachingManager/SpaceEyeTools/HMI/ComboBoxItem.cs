using System;
using System.Drawing;

namespace SpaceEyeTools.EXControls
{

    /// <summary>
    /// This class represents an ComboBox item of the ImagedComboBox which may contains an image and value.
    /// </summary>
    [Serializable]
    public class ComboBoxItem
    {
        private object _value;
        private Image _image;
        private object _tag;


        /// <summary>
        /// Get / Set Value
        /// </summary>
        public object Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        /// <summary>
        /// Get / Set Tag.
        /// </summary>
        public object Tag
        {
            get
            {
                return _tag;
            }
            set
            {
                _tag = value;
            }
        }

        /// <summary>
        /// Get / Set Item image.
        /// </summary>
        public Image Image
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public ComboBoxItem()
        {
            _value = String.Empty;
            _tag = String.Empty;
            _image  = new Bitmap(1,1);
        }


        /// <summary>
        /// Constructor item without image.
        /// </summary>
        /// <param name="value">Item value.</param>
        public ComboBoxItem(object value)
        {
            _value = value;
            _tag = String.Empty;
            _image = new Bitmap(1, 1);
            
        }

       
        /// <summary>
        ///  Constructor item with image.
        /// </summary>
        /// <param name="value">Item value.</param>
        /// <param name="image">Item image.</param>
        public ComboBoxItem(object value, Image image)
        {
            _value = value;
            _tag = String.Empty;
            _image = image;
        }


        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>returns _value</returns>
        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
