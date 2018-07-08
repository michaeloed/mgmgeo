using System.Drawing;
using GMap.NET.WindowsForms;

namespace GMap.NET
{
    /// <summary>
    /// Class used to display an image in place of a marker
    /// In this case it is used to display a raster image
    /// </summary>
    public class GMapRasterImage : GMapMarker
    {
        private Image image;

        /// <summary>
        /// Get / Set associated image
        /// </summary>
        public Image Image
        {
            get
            {
                return image;
            }
            set
            {
                image = value;
                if (image != null)
                {
                    this.Size = new Size(image.Width, image.Height);
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p">Top Left coordinates</param>
        public GMapRasterImage(GMap.NET.PointLatLng p)
            : base(p)
        {
            DisableRegionCheck = true;
            IsHitTestVisible = false;
        }

        /// <summary>
        /// Custome render
        /// </summary>
        /// <param name="g">Graphic to render</param>
        public override void OnRender(Graphics g)
        {
            if (image == null)
                return;

            g.DrawImage(image, LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height);
        }
    }
}
