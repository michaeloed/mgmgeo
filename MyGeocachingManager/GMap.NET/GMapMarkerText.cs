using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using GMap.NET.WindowsForms;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using SpaceEyeTools;

namespace GMap.NET.WindowsForms.Markers
{
    /// <summary>
    /// Custom marker: text within a label
    /// </summary>
    public class GMapMarkerText : GMap.NET.WindowsForms.GMapMarker
    {
        private Image img;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p">Coordinates of marker</param>
        /// <param name="text">Text to be drawn</param>
        /// <param name="penColor">Text oclor</param>
        /// <param name="backColor">Background color</param>
        /// <param name="offsetX">X Offset for marker coordinates in pixels</param>
        /// <param name="offsetY">Y Offset for marker coordinates in pixels</param>
        /// <param name="bBig">If true, text will be bigger</param>
        public GMapMarkerText(PointLatLng p, String text, Color penColor, Color backColor, int offsetX, int offsetY, bool bBig)
            : base(p)
        {

            img = MyTools.CreateBitmapImage(text, penColor, backColor, bBig);
            Size = img.Size;
            Offset = new System.Drawing.Point(-Size.Width / 2 + offsetX, -Size.Height / 2 + offsetY);
        }

       
        /// <summary>
        /// Rendering method
        /// </summary>
        /// <param name="g">Graphic for rendering</param>
        public override void OnRender(Graphics g)
        {
            g.DrawImage(img, LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height);
        }
    }
}
