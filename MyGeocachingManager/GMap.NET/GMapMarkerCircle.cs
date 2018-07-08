using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using GMap.NET.WindowsForms;
using GMap.NET;
using SpaceEyeTools;

namespace GMap.NET.WindowsForms.Markers
{

    /// <summary>
    /// Custom marker: circular area
    /// </summary>
    public class GMapMarkerCircle : GMapMarker
    {
        private int Radius;
        private Pen OutlinePen;
        private Brush FillBrush;
        private bool Fill;
        private GMapControl Map;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="map">Associated map control</param>
        /// <param name="p">Center coordinates</param>
        /// <param name="radius">Radius in meters</param>
        /// <param name="outlinepen">Pen for border drawing</param>
        /// <param name="fillbrush">Brush for filling</param>
        /// <param name="fill">If true, marker will be filled,
        /// If false, only a circle will be drawn</param>
        public GMapMarkerCircle(GMapControl map, PointLatLng p, int radius, Pen outlinepen, Brush fillbrush, bool fill)
            : base(p)
        {
            Map = map;
            Radius = radius;
            OutlinePen = outlinepen;
            FillBrush = fillbrush;
            Fill = fill;
        }

        /// <summary>
        /// Returns bounding rect of this marker
        /// </summary>
        /// <returns></returns>
        public RectLatLng GetBoundRect()
        {
        	double degrees_radius = MyTools.KilometerToDegree(((double)Radius)/1000.0, this.Position.Lat);
        	double latmin = this.Position.Lat - degrees_radius;
            double latmax = this.Position.Lat + degrees_radius;
            double lonmin = this.Position.Lng - degrees_radius;
            double lonmax = this.Position.Lng + degrees_radius;
            return RectLatLng.FromLTRB(lonmin, latmax, lonmax, latmin);
        }
        
        
        /// <summary>
        /// Rendering method
        /// </summary>
        /// <param name="g">Graphics for rendering</param>
        public override void OnRender(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int R = (int)((Radius) / Map.MapProvider.Projection.GetGroundResolution((int)(Map.Zoom), Position.Lat)) * 2;

            if (Fill == true)
            {
                g.FillEllipse(FillBrush, new System.Drawing.Rectangle(LocalPosition.X - R / 2, LocalPosition.Y - R / 2, R, R));
            }
            g.DrawEllipse(OutlinePen, new System.Drawing.Rectangle(LocalPosition.X - R / 2, LocalPosition.Y - R / 2, R, R));
        }
    }
}