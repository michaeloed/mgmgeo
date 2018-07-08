using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GMap.NET.WindowsForms
{
    /// <summary>
    /// A route with different colors and/or width per segment
    /// </summary>
    public class GMapRouteGradient : GMapRoute
    {
        Color[] _colors = null;
        float[] _fwidth = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pts">Points of route</param>
        /// <param name="name">Route name</param>
        /// <param name="colors">Route colors (one per point)</param>
        /// <param name="fwidth">Route width (one per point - can turn ugly)</param>
        public GMapRouteGradient(List<PointLatLng> pts, String name, Color[] colors, float[] fwidth)
            : base(pts, name)
        {
            _colors = colors;
            _fwidth = fwidth;
        }

        /// <summary>
        /// OnRender override
        /// </summary>
        /// <param name="g">Graphic</param>
        public override void OnRender(System.Drawing.Graphics g)
        {
            if ((LocalPoints != null) && (LocalPoints.Count >= 2))
            {
                int nb = LocalPoints.Count;
                for (int i = 1; i < nb; i++)
                {
                    GPoint gp1 = LocalPoints[i - 1];
                    GPoint gp2 = LocalPoints[i];
                    
                    Point p1 = new Point((int)gp1.X, (int)gp1.Y);
                    Point p2 = new Point((int)gp2.X, (int)gp2.Y);

                    g.DrawLine(new Pen(_colors[i], _fwidth[i]), p1, p2);
                }
            }
        }
    }
}
