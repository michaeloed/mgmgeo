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
    /// Custom marker: used for statistics display
    /// </summary>
    public class GMapMarkerGCStats : GMap.NET.WindowsForms.GMapMarker
    {
        static int iyOffset = -2; // en fonction de la taille de la fonte : fonte 8 -> 0; fonte 10 -> -2
        static Font objFont = new Font("Arial", 10, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
        static Brush brushText = new SolidBrush(Color.Black);
        static Pen penText = new Pen(Color.Black, 1f);
        
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
        /// <param name="fav">Image for favorites</param>
        /// <param name="scale">Image for scale display</param>
        /// <param name="ifav">Number of favorites</param>
        /// <param name="ipop">Popularity value in % (60% = 60)</param>
        /// <param name="colorBackgroundText">Background color for text</param>
        public GMapMarkerGCStats(PointLatLng p, Image fav, Image scale, int ifav, int ipop, Color colorBackgroundText)
            : base(p)
        {
            img = CreateStatIcon(fav, scale, ifav, ipop, colorBackgroundText);
            Size = img.Size;
            Offset = new System.Drawing.Point(-Size.Width / 2, -Size.Height / 2);
        }

        Image CreateStatIcon(Image fav, Image scale, int ifav, int ipop, Color colorBackgroundText)
        {
            Bitmap objBmpImage = new Bitmap(64, 64);
            String sfav = ifav.ToString();
            String spop = ipop.ToString() + "%";
            int ix, iy;

            // Add the colors to the new bitmap.
            Graphics objGraphics = Graphics.FromImage(objBmpImage);

            // Set Background color
            objGraphics.Clear(Color.Transparent);

            // Options
            //objGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //objGraphics.SmoothingMode = SmoothingMode.AntiAlias;
            //objGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            // On copie l'icone favori
            objGraphics.DrawImage(fav, 4, 16); // (64 - 24 pour l'icone de cache : il reste 20 de chaque coté, avec 16 pour l'icone favori on fixe à 4 depuis la gauche

            // On copie le scale
            if (scale != null)
                objGraphics.DrawImage(scale, 46, 16); // (64 - 24 pour l'icone de cache : il reste 20 de chaque coté => la fin de l'icone de cache est à 44 (on ajoute une marge)

            // Text favori
            iy = 10 + iyOffset;
            if (colorBackgroundText != Color.Transparent)
            {
                ix = 18;
                Brush brush = new SolidBrush(colorBackgroundText);
                // On trace un rectangle rempli derriere les textes avec un rectangle bordure
                System.Drawing.SizeF st = objGraphics.MeasureString(sfav, GMapMarkerGCStats.objFont);
                objGraphics.FillRectangle(brush, ix -1, iy - 1, st.Width + 2f, st.Height + 2f);
                objGraphics.DrawRectangle(penText, ix - 1, iy - 1, st.Width + 2f, st.Height + 2f);
                
                if (ipop >= 0)
                {
                    ix = 38;
                    st = objGraphics.MeasureString(spop, GMapMarkerGCStats.objFont);
                    objGraphics.FillRectangle(brush, ix - 1, iy - 1, st.Width + 2f, st.Height + 2f);
                    objGraphics.DrawRectangle(penText, ix - 1, iy - 1, st.Width + 2f, st.Height + 2f);
                }
            }

            // Text favori
            objGraphics.DrawString(sfav, GMapMarkerGCStats.objFont, GMapMarkerGCStats.brushText, 18, iy); // 16 + 4 pour x

            // Text popularité
            if (ipop >= 0)
                objGraphics.DrawString(spop, GMapMarkerGCStats.objFont, GMapMarkerGCStats.brushText, 38, iy);


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
