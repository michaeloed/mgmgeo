using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GMap.NET.WindowsForms.Markers;
using MyGeocachingManager;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Reflection;

namespace GMap.NET.WindowsForms
{
    /// <summary>
    /// Custom GMap.NET.WindowsForms.GMapControl control
    /// This control can provide a feedback to 2 TextBox (one for latitude, one for longitude)
    /// These 2 TextBox will be automatically updated with GMapControlCustom center coordinates
    /// when they change.
    /// It is also used to draw itinerary, areas will special left mouse clicks.
    /// </summary>
    public class GMapControlCustom : GMap.NET.WindowsForms.GMapControl
    {
        Control _wndToCheck = null;
        bool _bOverrideLeftMouseClick = false;
        MainWindow _daddy = null;
        Control _controlTextLatLon = null;
        Control _controlTextRadius = null;
        
        /// <summary>
        /// Set reference to main window (MainWindow instance), used for callback purposes
        /// </summary>
        public MainWindow Daddy
        {
            set { _daddy = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        public void AssignMapProvider(GMap.NET.MapProviders.GMapProvider provider)
        {
        	this.MapProvider = provider;
        }
        
        /// <summary>
        /// Get/Set TextBox control for latitude / longitude
        /// </summary>
        public Control ControlTextLatLon
        {
            get { return _controlTextLatLon; }
            set { _controlTextLatLon = value; }
        }

        /// <summary>
        /// Get/Set TextBox control for circle radius
        /// </summary>
        public Control ControlTextRadius
        {
            get { return _controlTextRadius; }
            set { _controlTextRadius = value; }
        }
        
        /// <summary>
        /// Reference to a Form control to give it the focus if needed
        /// </summary>
        public Control WindowsToCheck
        {
            get { return _wndToCheck; }
            set { _wndToCheck = value; }
        }


        /// <summary>
        /// If true, Left mouse click will be overriden on this custom control
        /// </summary>
        public bool OverrideLeftMouseClick
        {
            get { return _bOverrideLeftMouseClick; }
            set { _bOverrideLeftMouseClick = value; }
        }


        private void BaseConstructor(Control wndToCheck, MainWindow daddy)
        {
        	_wndToCheck = wndToCheck;
            _daddy = daddy;
            
            String useragent = "MyGeocachingManager ";
            String ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (MainWindow.AssemblySubVersion != "")
            {
                ver += "." + MainWindow.AssemblySubVersion;
            }
            MapProviders.GMapProvider.UserAgent = useragent;
            
           this.FillEmptyTiles = true;
           //this.HelperLineOption = HelperLineOptions.ShowOnModifierKey; // for SHIFT & ALT only... need to add CTRL
           this.IgnoreMarkerOnMouseWheel = true;
           
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public GMapControlCustom()
            : base()
        {
        	BaseConstructor(null, null);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="wndToCheck">Reference to a Form control to give it the focus if needed</param>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        public GMapControlCustom(Control wndToCheck, MainWindow daddy)
            : base()
        {
        	BaseConstructor(wndToCheck, daddy);
        }

        /// <summary>
        /// OnMouseEnter event
        /// </summary>
        /// <param name="e">event</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            // Si la fenêtre _wndToCheck a le focus, on ne le récupère pas !
            if ((_wndToCheck != null) && (_wndToCheck.ContainsFocus)) // Focused ne marchera qu'avec _gmap qui pique le focus :-)
            {
                // Do nothing !!!
            }
            else
                base.OnMouseEnter(e);
        }

        /// <summary>
        /// OnMouseClick event
        /// </summary>
        /// <param name="e">event</param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            if ((_daddy != null) && (this == _daddy._cacheDetail._gmap)) // Uniquement pour Cachedetail
            {
                // CTRL+LEFT appuyé ? (on fait une route ou une zone)
                if ((e.Button == System.Windows.Forms.MouseButtons.Left) &&
                    _bOverrideLeftMouseClick &&
                    ((Control.ModifierKeys & Keys.Control) > 0) &&
                    (_daddy != null))
                {
                    _daddy._cacheDetail.cachedetail_OnMapClick(e);
                } // SHIFT+Left (déplacement simple) ou ALT+Left (déplacement avec création de points lors de la pose) appuyé ?
                else if ((e.Button == System.Windows.Forms.MouseButtons.Left) &&
                    _bOverrideLeftMouseClick &&
                    (((Control.ModifierKeys & Keys.Shift) > 0) || ((Control.ModifierKeys & Keys.Alt) > 0)) &&
                    (_daddy != null))
                {
                    _daddy._cacheDetail.cachedetail_OnMapClick(e);
                }
                else
                {
                    // On annulé une éventuelle édition (genre un SHIFT+CLICK relaché)
                    _daddy._cacheDetail.FinishAnyExistingEdition(e);
                    BaseOnMouseClick(e);
                }
            }
            else
            {
                BaseOnMouseClick(e);
            }
        }

        /// <summary>
        /// Callback to display coordinates in the 2 TextBox
        /// </summary>
        /// <param name="e">event</param>
        public void BaseOnMouseClick(MouseEventArgs e)
        {
            // Pas de modification, on en profite juste pour coller les coordonnées dans les controles s'ils existent
            PointLatLng pt = this.FromLocalToLatLng(e.X, e.Y);
            if (_controlTextLatLon != null)
            {
                _controlTextLatLon.Text = pt.Lat.ToString().Replace(",", ".") + " " + pt.Lng.ToString().Replace(",", ".");
                // On crée un markeur
                this.Overlays[GMapWrapper.RESERVED2].Markers.Clear();
                GMapMarker marker = new GMarkerGoogle(pt, GMarkerGoogleType.red_pushpin);
                this.Overlays[GMapWrapper.RESERVED2].Markers.Add(marker);
                
                if (_controlTextRadius != null)
                {
                	String s = _controlTextRadius.Text;
                	double radius;
                	if (Double.TryParse(s, out radius))
                	{
                		if (!_daddy._bUseKm)
                		{
                			radius = radius / _daddy._dConvKmToMi;
                		}
                		// On crée un cercle
                		Color c = Color.Green;
                		c = Color.FromArgb(60, c.R, c.G, c.B);
		            	Brush brush = new SolidBrush(c);
		        		Pen pen = new Pen(c, 2);
		        		GMapMarkerCircle circle = new GMapMarkerCircle(this, pt, (int)(radius * 1000.0), pen, brush, true);
		        		this.Overlays[GMapWrapper.RESERVED2].Markers.Add(circle);
                	}
                }
            }

            base.OnMouseClick(e);
        }

        /// <summary>
        /// OnKeyPress event: + and - will change zoom level
        /// </summary>
        /// <param name="e">event</param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (e.KeyChar == '+') // +
            {
                this.Zoom++;
            }
            else if (e.KeyChar == '-') // -
            {
                this.Zoom--;
            }
            else
            {
                base.OnKeyPress(e);
            }
        }

        #region Scale variables

        /// <summary>
        /// The font for the m/km markers
        /// </summary>
        private Font fontCustomScale = new Font("Arial", 6);

        /// <summary>
        /// The font for the scale header 
        /// </summary>
        private Font fontCustomScaleBold = new Font("Arial", 10, FontStyle.Bold);

        /// <summary>
        /// The brush for the scale's background
        /// </summary>
        private Brush brushCustomScaleBackColor = new SolidBrush(Color.FromArgb(200, 185, 215, 255));

        /// <summary>
        /// The Textcolor for the scale's fonts
        /// </summary>
        private Color colorCustomScaleText = Color.FromArgb(20, 65, 140);

        /// <summary>
        /// The width of the scale-rectangle
        /// </summary>
        private int intScaleRectWidth = 300;

        /// <summary>
        /// The height of the scale-rectangle
        /// </summary>
        private int intScaleRectHeight = 50;

        /// <summary>
        /// The height of the scale bar
        /// </summary>
        private int intScaleBarHeight = 10;

        /// <summary>
        /// The padding of the scale
        /// </summary>
        private int intScaleLeftPadding = 10;

        /// <summary>
        /// Usage of custom scale
        /// </summary>
        public bool boolUseCustomScale = false;

        /// <summary>
        /// Usage of custom scale label
        /// </summary>
        public bool boolUseCustomScaleLabel = true;

        #endregion

        /// <summary>
        /// Draw extra stuff here (E.g. a legend for the map)
        /// </summary>
        /// <param name="g"></param>
        protected override void OnPaintOverlays(System.Drawing.Graphics g)
        {
            base.OnPaintOverlays(g);

            g.SmoothingMode = SmoothingMode.HighQuality;

            if (boolUseCustomScale)
            {
                double resolution = this.MapProvider.Projection.GetGroundResolution((int)this.Zoom, Position.Lat);

                int px10 = (int)(10.0 / resolution);            // 10 meters
                int px100 = (int)(100.0 / resolution);          // 100 meters
                int px1000 = (int)(1000.0 / resolution);        // 1km   
                int px10000 = (int)(10000.0 / resolution);      // 10km  
                int px100000 = (int)(100000.0 / resolution);    // 100km  
                int px1000000 = (int)(1000000.0 / resolution);  // 1000km
                int px5000000 = (int)(5000000.0 / resolution);  // 5000km

                //Check how much width we have and set the scale accordingly
                int availableWidth = (intScaleRectWidth - 2 * intScaleLeftPadding);

                //5000 kilometers:
                if (availableWidth >= px5000000)
                    DrawScale(g, px5000000, availableWidth, 5000, "km");
                //1000 kilometers:
                else if (availableWidth >= px1000000)
                    DrawScale(g, px1000000, availableWidth, 1000, "km");
                //100 kilometers:
                else if (availableWidth >= px100000)
                    DrawScale(g, px100000, availableWidth, 100, "km");
                //10 kilometers:
                else if (availableWidth >= px10000)
                    DrawScale(g, px10000, availableWidth, 10, "km");
                //1 kilometers:
                else if (availableWidth >= px1000)
                    DrawScale(g, px1000, availableWidth, 1, "km");
                //100 meters:
                else if (availableWidth >= px100)
                    DrawScale(g, px100, availableWidth, 100, "m");
                //10 meters:
                else if (availableWidth >= px10)
                    DrawScale(g, px10, availableWidth, 10, "m");
            }
        }

        /// <summary>
        /// zooms and centers all polygons
        /// </summary>
        /// <param name="overlayId">overlay id or null to check all</param>
        /// <returns></returns>
        public bool ZoomAndCenterPolygons(string overlayId)
        {
            RectLatLng? rect = GetRectOfAllPolygons(overlayId);
            if (rect.HasValue)
            {
                return SetZoomToFitRect(rect.Value);
            }

            return false;
        }

        /// <summary>
        /// zooms and centers all GMapMarkerCircle
        /// </summary>
        /// <param name="overlayId">overlay id or null to check all</param>
        /// <returns></returns>
        public bool ZoomAndCenterCircles(string overlayId)
        {
            RectLatLng? rect = GetRectOfAllCircles(overlayId);
            if (rect.HasValue)
            {
                return SetZoomToFitRect(rect.Value);
            }

            return false;
        }

        
        /// <summary>
        /// gets rectangle with all GMapMarkerCircle objects inside
        /// </summary>
        /// <param name="overlayId">overlay id or null to check all</param>
        /// <returns></returns>
        public RectLatLng? GetRectOfAllCircles(string overlayId)
        {
            RectLatLng? ret = null;

            double left = double.MaxValue;
            double top = double.MinValue;
            double right = double.MinValue;
            double bottom = double.MaxValue;

            foreach (GMapOverlay o in Overlays)
            {
                if (overlayId == null || o.Id == overlayId)
                {
                    if (o.IsVisibile && o.Markers.Count > 0)
                    {
                        foreach (GMapMarker m in o.Markers)
                        {
                        	GMapMarkerCircle c = m as GMapMarkerCircle;
                        	if (c == null)
                        		continue;
                            if (c.IsVisible)
                            {
                            	RectLatLng r = c.GetBoundRect();
                                // left
                                if (r.Left < left)
                                {
                                    left = r.Left;
                                }

                                // top
                                if (r.Top > top)
                                {
                                    top = r.Top;
                                }

                                // right
                                if (r.Right > right)
                                {
                                    right = r.Right;
                                }

                                // bottom
                                if (r.Bottom < bottom)
                                {
                                    bottom = r.Bottom;
                                }
                            }
                        }
                    }
                }
            }

            if (left != double.MaxValue && right != double.MinValue && top != double.MinValue && bottom != double.MaxValue)
            {
                ret = RectLatLng.FromLTRB(left, top, right, bottom);
            }

            return ret;
        }
        
        /// <summary>
        /// gets rectangle with all polygons objects inside
        /// </summary>
        /// <param name="overlayId">overlay id or null to check all</param>
        /// <returns></returns>
        public RectLatLng? GetRectOfAllPolygons(string overlayId)
        {
            RectLatLng? ret = null;

            double left = double.MaxValue;
            double top = double.MinValue;
            double right = double.MinValue;
            double bottom = double.MaxValue;

            foreach (GMapOverlay o in Overlays)
            {
                if (overlayId == null || o.Id == overlayId)
                {
                    if (o.IsVisibile && o.Polygons.Count > 0)
                    {
                        foreach (GMapPolygon m in o.Polygons)
                        {
                            if (m.IsVisible)
                            {
                            	foreach(var p in m.Points)
                            	{
	                                // left
	                                if (p.Lng < left)
	                                {
	                                    left = p.Lng;
	                                }
	
	                                // top
	                                if (p.Lat > top)
	                                {
	                                    top = p.Lat;
	                                }
	
	                                // right
	                                if (p.Lng > right)
	                                {
	                                    right = p.Lng;
	                                }
	
	                                // bottom
	                                if (p.Lat < bottom)
	                                {
	                                    bottom = p.Lat;
	                                }
                            	}
                            }
                        }
                    }
                }
            }

            if (left != double.MaxValue && right != double.MinValue && top != double.MinValue && bottom != double.MaxValue)
            {
                ret = RectLatLng.FromLTRB(left, top, right, bottom);
            }

            return ret;
        }
        
        /// <summary>
        /// Draw the scale
        /// </summary>
        /// <param name="g"></param>
        /// <param name="resLength"></param>
        /// <param name="availableWidth"></param>
        /// <param name="totalDimenson"></param>
        /// <param name="unit"></param>
        private void DrawScale(System.Drawing.Graphics g, int resLength, int availableWidth, int totalDimenson, String unit)
        {
            int delta = 0;
            if (!boolUseCustomScaleLabel)
                delta = 18;

            Point p = new System.Drawing.Point(this.Width - (intScaleRectWidth + 10), this.Height - (intScaleRectHeight - delta + 10));
            Rectangle rect = new Rectangle(p, new Size(intScaleRectWidth, intScaleRectHeight - delta));
            g.FillRectangle(brushCustomScaleBackColor, rect);
            Pen pen = new Pen(colorCustomScaleText, 1);
            g.DrawRectangle(pen, rect);
            SizeF stringSize = new SizeF();
            Point pos = new Point();

            //Header:
            if (boolUseCustomScaleLabel)
            {
                String scaleString = "Echelle";
                stringSize = g.MeasureString(scaleString, fontCustomScaleBold);
                pos = new Point(p.X + (rect.Width - (int)stringSize.Width) / 2, p.Y + 3);
                g.DrawString(scaleString, fontCustomScaleBold, pen.Brush, pos);
                
                pos = new Point(p.X + intScaleLeftPadding, pos.Y + 30);
            }
            else
            {
                pos = new Point(p.X + intScaleLeftPadding, p.Y + 18);
            }

            //How many rectangles fit?
            int numRects = availableWidth / resLength;
            Size rectSize = new Size(resLength, intScaleBarHeight);
            //Center rectangle
            pos.X += (availableWidth - resLength * numRects) / 2;
            //Draw rectangles:
            for (int i = 0; i < numRects; i++)
            {
                Rectangle r = new Rectangle(pos, rectSize);
                if (i % 2 == 0)
                    g.FillRectangle(pen.Brush, r);
                else
                    g.DrawRectangle(pen, r);
                //Draw little vertical lines
                g.DrawLine(pen, pos, new Point(pos.X, pos.Y - 5));
                //Draw labels:
                int dist = i * totalDimenson;
                stringSize = g.MeasureString(dist + " " + unit, fontCustomScale);
                g.DrawString(dist + " " + unit, fontCustomScale, pen.Brush, new Point(pos.X - (int)stringSize.Width / 2, pos.Y - (7 + (int)stringSize.Height)));
                //Finally set new point
                pos = new Point(pos.X + resLength, pos.Y);
            }
            //Draw last line:
            g.DrawLine(pen, pos, new Point(pos.X, pos.Y - 5));
            //Draw last label
            int m = numRects * totalDimenson;
            stringSize = g.MeasureString(m + " " + unit, fontCustomScale);
            g.DrawString(m + " " + unit, fontCustomScale, pen.Brush, new Point(pos.X - (int)stringSize.Width / 2, pos.Y - (7 + (int)stringSize.Height)));
        }
    }
}
