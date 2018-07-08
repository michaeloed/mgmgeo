using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using GMap.NET.WindowsForms;
using System.Drawing.Text;
using System.Drawing.Drawing2D;

namespace GMap.NET.WindowsForms.Markers
{
    /// <summary>
    /// Custom marker: used for statistics display (Pie)
    /// </summary>
    public class GMapMarkerGCStatsPie : GMap.NET.WindowsForms.GMapMarker
    {
        static Font objFont = new Font("Arial", 10, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
        static Brush brushText = new SolidBrush(Color.Black);

        Image img;

        /// <summary>
        /// The image to display as a marker.
        /// </summary>
        public Image MarkerImage
        {
            get
            {
                return img;
            }
            set
            {
                img = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p">marker coordinates</param>
        /// <param name="pop">Image for popularity</param>
        /// <param name="ifav">Number of favorites</param>
        /// <param name="ipop">Popularity value in % (60% = 60)</param>
        /// <param name="colorBackgroundText">Background color for text</param>
        public GMapMarkerGCStatsPie(PointLatLng p, Image pop, int ifav, int ipop, Color colorBackgroundText)
            : base(p)
        {
            img = CreateStatIcon(pop, ifav, ipop, colorBackgroundText);
            Size = img.Size;
            Offset = new System.Drawing.Point(-Size.Width / 2, -Size.Height / 2);
        }

        Image CreateStatIcon(Image pop, int ifav, int ipop, Color colorBackgroundText)
        {
            Bitmap objBmpImage = new Bitmap(64, 80);
            String sfav = ifav.ToString();
            
            // Add the colors to the new bitmap.
            Graphics objGraphics = Graphics.FromImage(objBmpImage);

            // Set Background color
            objGraphics.Clear(Color.Transparent);

            // On copie l'icone favori
            objGraphics.DrawImage(pop, 16, 0);

            // Text favori
            System.Drawing.SizeF st = objGraphics.MeasureString(sfav, GMapMarkerGCStatsPie.objFont);
            objGraphics.DrawString(sfav, GMapMarkerGCStatsPie.objFont, GMapMarkerGCStatsPie.brushText, (64.0f - st.Width) / 2.0f, (32.0f - st.Height) / 2.0f);

            objGraphics.Flush();

            return (objBmpImage);
        }

        /// <summary>
        /// Rendering function
        /// </summary>
        /// <param name="g">Graphic for rendering</param>
        public override void OnRender(Graphics g)
        {
            g.DrawImage(img, LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height);
        }
    }
}
