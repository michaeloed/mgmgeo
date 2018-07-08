using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace MyGeocachingManager
{
    /// <summary>
    /// Class used for cache attributes display
    /// </summary>
    public class AttributeImage
    {

        /// <summary>
        /// Associated picture box for this attribute
        /// </summary>
        public PictureBox _PictureBox = null;

        /// <summary>
        /// List of images to be used:
        /// - Disabled attribute
        /// - Enabled attribute (positive, such as "dogs allowed")
        /// - Enabled attribute (negative, such as "dogs NOT allowed")
        /// </summary>
        public Image[] _Images = new Image[] { null, null, null };

        /// <summary>
        /// Image index
        /// 0 Disabled attribute
        /// 1 Enabled attribute (positive, such as "dogs allowed")
        /// 2 Enabled attribute (negative, such as "dogs NOT allowed")
        /// </summary>
        public int _Index = 0;

        /// <summary>
        /// Attribute key name, used to retrieve picture, translation, etc.
        /// </summary>
        public String _Key = null;
    }
}
