using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MyGeocachingManager;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;
using SpaceEyeTools;
using SpaceEyeTools.HMI;

namespace MyGeocachingManager.HMI
{
    /// <summary>
    /// Form to display a GPX track,
    /// to select a part of this track, with direct callback on CacheDetail map
    /// </summary>
    public partial class TrackSelector : Form
    {
        MainWindow _Daddy = null;
        List<PointLatLng> _Pts = null;
        List<PointLatLng> _PtsRoute = null;
        List<DateTime> _Times = null;
        List<DateTime> _TimesRoute = null;
        List<Double> _Elevations = null;
        List<Double> _ElevationsRoute = null;
        List<Double> _Speeds = null;
        List<Double> _SpeedsRoute = null;
        Double _Min_altitude = Double.MaxValue;
        Double _Max_altitude = Double.MinValue;
        Double _Min_speed = Double.MaxValue;
        Double _Max_speed = Double.MinValue;
        TrackSelector.ColorType _speedColorType = ColorType.SingleColor;
        Color _singlecolor = Color.Red;
        DateTime _Start = DateTime.Now;
        DateTime _End = DateTime.Now;
        bool _ComputeSpeed = true;

        /// <summary>
        /// Enumeration defining which kind of color will be used for the road
        /// </summary>
        public enum ColorType
        {
            /// <summary>
            /// Green-Red gradient
            /// </summary>
            GreenRed = 0,
            /// <summary>
            /// HSL colors
            /// </summary>
            HSL = 1,
            /// <summary>
            /// Single color
            /// </summary>
            SingleColor = 2
        };

        /// <summary>
        /// Delete all associated decorations from map display
        /// </summary>
        public void EmptyAssociatedDecorations()
        {
            _Daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED4].Routes.Clear();
            _Daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED4].Markers.Clear();
        }

        private void ComputeMinMax()
        {
            _Min_altitude = Double.MaxValue;
            _Max_altitude = Double.MinValue;
            _Min_speed = Double.MaxValue;
            _Max_speed = Double.MinValue;

            if (_ElevationsRoute != null)
            {
                foreach (Double d in _ElevationsRoute)
                {
                    if (d < _Min_altitude)
                        _Min_altitude = d;
                    if (d > _Max_altitude)
                        _Max_altitude = d;
                }
            }

            if (_ComputeSpeed)
            {
                foreach (Double d in _SpeedsRoute)
                {
                    if (d < _Min_speed)
                        _Min_speed = d;
                    if (d > _Max_speed)
                        _Max_speed = d;
                }
            }
            _Daddy.Log("_Min_altitude " + _Min_altitude.ToString());
            _Daddy.Log("_Max_altitude " + _Max_altitude.ToString());
            _Daddy.Log("_Min_speed " + _Min_speed.ToString());
            _Daddy.Log("_Max_speed " + _Max_speed.ToString());
            
        }

        /// <summary>
        /// Constructor
        /// pts and times shall have the same items count
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        /// <param name="pts">All the points of the track</param>
        /// <param name="times">All the DateTime associated to a point of the track</param>
        /// <param name="elevations">All the elevations (in meters) associated to a point of the track</param>
        /// <param name="bComputeSpeed">If true, speed will be computed from time and positions</param>
        /// <param name="speedColorType">Color type that will be used on road to display speed (only useful if bComputeSpeed is true)</param>
        /// <param name="singlecolor">Color value that will be used on road (only useful if bComputeSpeed is true and speedColorType is SingleValue or if bComputeSpeed is false)</param>
        public TrackSelector(MainWindow daddy, List<PointLatLng> pts, List<DateTime> times, List<Double> elevations, bool bComputeSpeed, TrackSelector.ColorType speedColorType, Color singlecolor)
        {
            _Daddy = daddy;
            _Daddy.Log("TrackSelector Ctor: Misc");
            InitializeComponent();
            this.Text = _Daddy.GetTranslator().GetString("LblTrackSelector");
            btnConfiguretrksel.Image = _Daddy.GetImageSized("CartoConfiguration");
            _Daddy.TranslateTooltips(this, null);

            _Pts = pts;
            _PtsRoute = _Pts;
            _Times = times;
            _TimesRoute = _Times;
            _Elevations = elevations;
            _ElevationsRoute = _Elevations;
            
            // Valeurs de base
            _Start = _Times[0];
            _End = _Times[_Times.Count - 1];
            
            // On crée l'IHM
            _Daddy.Log("TrackSelector Ctor: HMI");
            if (_Times != null)
            {
                // Les textes
                labelTxtStart.Text = _Daddy.GetTranslator().GetString("LblStart");
                labelTxtEnd.Text = _Daddy.GetTranslator().GetString("LblEnd");

                // Les labels
                labelStart.Text = _Start.ToString("MM/dd/yyyy HH:mm:ss");
                labelStart.Tag = _Start;
                labelEnd.Text = _End.ToString("MM/dd/yyyy HH:mm:ss");
                labelEnd.Tag = _End;

                // La durée
                TimeSpan duree = _End - _Start;
                _Daddy.Log("_Start : " + _Start.ToString());
                _Daddy.Log("_End : " + _End.ToString());
                _Daddy.Log("duree.TotalSeconds : " + duree.TotalSeconds.ToString());
                trackBarStarttrksel.Minimum = 0;
                trackBarEndtrksel.Minimum = 0;
                trackBarStarttrksel.Maximum = (int)(duree.TotalSeconds);
                trackBarEndtrksel.Maximum = (int)(duree.TotalSeconds);
                trackBarStarttrksel.Value = 0;
                trackBarEndtrksel.Value = (int)(duree.TotalSeconds);
            }

            // Le callback du graphique
            pnGraph._CallbackMethodMouseMove = CallbackMethodMouseMove;

            _Daddy.Log("TrackSelector Ctor: DrawBasedOnParams");
            DrawBasedOnParameters(bComputeSpeed, speedColorType, singlecolor);
            
            // On affiche
            _Daddy.Log("TrackSelector Ctor: Display Map");
            _Daddy.ShowCacheMapInCacheDetailImpl(_Pts[0]);
        }

        private void DrawBasedOnParameters(bool bComputeSpeed, TrackSelector.ColorType speedColorType, Color singlecolor)
        {
            // Calcul des vitesses
            _ComputeSpeed = bComputeSpeed;
            _singlecolor = singlecolor;
            _speedColorType = speedColorType;
            _Daddy.Log("TrackSelector DrawBasedOnParameters: ComputeSpeeds");
            ComputeSpeeds();
            _Daddy.Log("TrackSelector DrawBasedOnParameters: ComputeMinMax");
            ComputeMinMax();

            if ((_Elevations == null) && (!_ComputeSpeed))
            {
                // Nothing to display in graph
                this.Height = 145;
                pnGraph.Visible = false;
            }
            else
            {
                this.Height = 327;
                pnGraph.Visible = true;
                pnGraph.BackColor = Color.White;
            }

            // On trace la route en ligne droite & les elevations si besoin
            _Daddy.Log("TrackSelector DrawBasedOnParameters: DrawRoute");
            DrawRoute(_Start, _End);
        }

        private void ComputeSpeeds()
        {
            
            _Speeds = new List<double>();
            _Speeds.Add(0.0); // First speed is null !
            for (int i = 1; i < _Pts.Count; i++)
            {
                PointLatLng p1 = _Pts[i - 1];
                PointLatLng p2 = _Pts[i];
                DateTime d1 = _Times[i - 1];
                DateTime d2 = _Times[i];
                TimeSpan ts = d2 - d1;
                Double duration = ts.TotalHours;
                Double distance = MyTools.DistanceBetweenPoints(p1.Lat, p1.Lng, p2.Lat, p2.Lng);
                Double speed_kmh = 0.0;
                if (duration != 0.0)
                    speed_kmh = distance / duration;
                _Speeds.Add(speed_kmh);
            }
            _SpeedsRoute = _Speeds;
        }
        /// <summary>
        /// Callback of OnMouseMove (MyPanel)
        /// </summary>
        /// <param name="index">index under cursor</param>
        /// <returns>true if callback successuflly executed (always true here)</returns>
        public bool CallbackMethodMouseMove(int index)
        {
            //this.Text = index.ToString() + "/" + _PtsRoute.Count.ToString();
            //this.Text = _Daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED4].Markers.Count.ToString();

            // Et on déplace le marker courant
            DrawElevationsSpeed(index);
            
            return true;
        }

        private void DrawElevationsSpeed(int index)
        {
            if ((_Elevations != null) || _ComputeSpeed)
            {
                // Marker courant en index 2 (0 : start, 1 : end, 2 : courant)
                // On met à jour le tooltip
                if (_Daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED4].Markers.Count >= 3)
                {
                    _Daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED4].Markers[2].Position = _PtsRoute[index];
                    String tip = _TimesRoute[index] + "\r\n";

                    if (_ComputeSpeed)
                    {
                        tip += _Daddy.GetTranslator().GetString("LblSpeed") + ": ";
                        if (_Daddy._bUseKm)
                        {
                            tip += String.Format("{0:0.#}", _SpeedsRoute[index]) + " kmh\r\n";
                        }
                        else
                        {
                            tip += String.Format("{0:0.#}", _SpeedsRoute[index] / 1.609344) + " mph\r\n";
                        }
                    }

                    if (_Elevations != null)
                    {
                        tip += _Daddy.GetTranslator().GetString("LblAltitude") + ": ";
                        if (_Daddy._bUseKm)
                        {
                            tip += String.Format("{0:0.#}", _ElevationsRoute[index]) + " m\r\n";
                        }
                        else
                        {
                            tip += String.Format("{0:0.#}", _ElevationsRoute[index] * 3.2808399) + " ft\r\n";
                        }
                    }
                    _Daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED4].Markers[2].ToolTipText = tip;
                }

                DateTime newstart = (DateTime)(labelStart.Tag);
                DateTime newend = (DateTime)(labelEnd.Tag);
                bool bGraphOk = false;
                if (newstart < newend)
                {
                    // Ok, on met à jour
                    pnGraph._Points = new List<List<double>>();
                    pnGraph._Min = new List<double>();
                    pnGraph._Max = new List<double>();
                    pnGraph._Colors = new List<Color>();
                    pnGraph._Names = new List<string>();
                    pnGraph._MinL = new List<String>();
                    pnGraph._MaxL = new List<String>();

                    // Les vitesses
                    if (_ComputeSpeed)
                    {
                        List<Double> newpts = new List<Double>();
                        double min = Double.MaxValue;
                        double max = Double.MinValue;
                        _TimesRoute = new List<DateTime>();

                        for (int i = 0; i < _Times.Count; i++)
                        {
                            Double pt = _Speeds[i];
                            DateTime ptdate = _Times[i];
                            if ((ptdate >= newstart) && (ptdate <= newend))
                            {
                                _TimesRoute.Add(ptdate);
                                newpts.Add(pt);
                                if (pt < min)
                                    min = pt;
                                if (pt > max)
                                    max = pt;
                            }
                        }

                        if (newpts.Count >= 2)
                        {
                            bGraphOk = true;
                            pnGraph._Points.Add(newpts);
                            _SpeedsRoute = newpts;
                            if (min == max)
                            {
                            	min = min - 1.0;
                            	max = max + 1.0;
                            }
                            pnGraph._Min.Add(min);
                            pnGraph._Max.Add(max);
                            pnGraph._Colors.Add(Color.Goldenrod);
                            pnGraph._Names.Add(_Daddy.GetTranslator().GetString("LblSpeed")); // TBD !


                            if (_Daddy._bUseKm)
                            {
                                pnGraph._MinL.Add(String.Format("{0:0.#}", min) + " kmh");
                                pnGraph._MaxL.Add(String.Format("{0:0.#}", max) + " kmh");
                            }
                            else
                            {
                                pnGraph._MinL.Add(String.Format("{0:0.#}", min / 1.609344) + " mph");
                                pnGraph._MaxL.Add(String.Format("{0:0.#}", max / 1.609344) + " mph");
                            }
                        }
                    }

                    if (((_ComputeSpeed && bGraphOk) || (!_ComputeSpeed)) // On a un graphe valide
                        && (_Elevations != null))
                    {
                        List<Double> newpts = new List<Double>();
                        double min = Double.MaxValue;
                        double max = Double.MinValue;

                        for (int i = 0; i < _Times.Count; i++)
                        {
                            Double pt = _Elevations[i];
                            DateTime ptdate = _Times[i];
                            if ((ptdate >= newstart) && (ptdate <= newend))
                            {
                                newpts.Add(pt);
                                if (pt < min)
                                    min = pt;
                                if (pt > max)
                                    max = pt;
                            }
                        }

                        if (newpts.Count >= 2)
                        {
                            bGraphOk = true;
                            pnGraph._Points.Add(newpts);
                            _ElevationsRoute = newpts;
                            if (min == max)
                            {
                            	min = min - 1.0;
                            	max = max + 1.0;
                            }
                            pnGraph._Min.Add(min);
                            pnGraph._Max.Add(max);
                            pnGraph._Colors.Add(Color.Red);
                            pnGraph._Names.Add(_Daddy.GetTranslator().GetString("LblAltitude")); // TBD !

                            if (_Daddy._bUseKm)
                            {
                                pnGraph._MinL.Add(String.Format("{0:0.#}", min) + " m");
                                pnGraph._MaxL.Add(String.Format("{0:0.#}", max) + " m");
                            }
                            else
                            {
                                pnGraph._MinL.Add(String.Format("{0:0.#}", min * 3.2808399) + " ft");
                                pnGraph._MaxL.Add(String.Format("{0:0.#}", max * 3.2808399) + " ft");
                            }
                        }
                    }
                    ComputeMinMax();
                    pnGraph.Refresh();
                }
                
                if (!bGraphOk)
                {
                    // On efface de toute façon
                    pnGraph._Points = null;
                    pnGraph._Min = null;
                    pnGraph._Max = null;
                    pnGraph._Colors = null;
                    pnGraph._Names = null;
                    pnGraph._MinL = null;
                    pnGraph._MaxL = null;
                    pnGraph.Refresh();
                }
            }
        }
        
        private void TrackSelector_Load(object sender, EventArgs e)
        {
            
        }

        private void trackBarStart_Scroll(object sender, EventArgs e)
        {

            DateTime offseted = _Start.AddSeconds(trackBarStarttrksel.Value);
            labelStart.Text = offseted.ToString("MM/dd/yyyy HH:mm:ss");
            labelStart.Tag = offseted;
            PerformMapUpdate();
        }

        private void trackBarEnd_Scroll(object sender, EventArgs e)
        {
            DateTime offseted = _Start.AddSeconds(trackBarEndtrksel.Value);
            labelEnd.Text = offseted.ToString("MM/dd/yyyy HH:mm:ss");
            labelEnd.Tag = offseted;
            PerformMapUpdate();
        }

        private void PerformMapUpdate()
        {
            DateTime newstart = (DateTime)(labelStart.Tag);
            DateTime newend = (DateTime)(labelEnd.Tag);
            DrawRoute(newstart, newend);
        }

        private void DrawRoute(DateTime newstart, DateTime newend)
        {
            bool bRouteValid = false;
            if (newstart < newend)
            {
                // Effacement
                EmptyAssociatedDecorations();

                // Ok, on met à jour
                List<PointLatLng> newpts = new List<PointLatLng>();
                for (int i = 0; i < _Times.Count; i++)
                {
                    PointLatLng pt = _Pts[i];
                    DateTime ptdate = _Times[i];
                    if ((ptdate >= newstart) && (ptdate <= newend))
                    {
                        newpts.Add(pt);
                    }
                }

                if (newpts.Count >= 2)
                {
                    bRouteValid = true;
                    // On ajoute un marker début et fin
                    _Daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED4].Markers.Add(CreateMarker(newpts[0], GMarkerGoogleType.green, _Daddy.GetTranslator().GetString("LblStart")));
                    _Daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED4].Markers.Add(CreateMarker(newpts[newpts.Count - 1], GMarkerGoogleType.orange, _Daddy.GetTranslator().GetString("LblEnd")));

                    // On ajoute un marker mobile au début du parcours
                    // Uniquement si le graphe est affiché
                    if ((_Elevations != null) || _ComputeSpeed)
                        _Daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED4].Markers.Add(CreateMarker(newpts[0], GMarkerGoogleType.blue, _Daddy.GetTranslator().GetString("LblCurrent")));

                    // And draw elevations
                    DrawElevationsSpeed(0);

                    GMapRoute route = null;
                    if (_ComputeSpeed)
                    {
                        // La route en ligne droite
                        // Colors list based on speed
                        Color[] colors = new Color[newpts.Count];
                        float[] fwidth = new float[newpts.Count];
                        int i = 0;
                        foreach (Double d in _SpeedsRoute)
                        {
                            fwidth[i] = 2.0f;
                            // Color is based on speed
                            double percentage = (_SpeedsRoute[i] - _Min_speed) / (_Max_speed - _Min_speed);
                            // Min = green
                            // Max = red
                            if (_speedColorType == ColorType.GreenRed)
                            {
                                int ired = (int)(255.0 * (percentage));
                                int igreen = (int)(255.0 * (1.0 - percentage));
                                ired = Math.Max(0, ired);
                                ired = Math.Min(255, ired);
                                igreen = Math.Max(0, igreen);
                                igreen = Math.Min(255, igreen);
                                Color c = Color.FromArgb(ired, igreen, 0);
                                colors[i] = c;
                            }
                            else if (_speedColorType == ColorType.HSL)
                            {
                                ColorRGB c = ColorRGB.HSL2RGB(percentage, 0.5, 0.5);
                                colors[i] = Color.FromArgb(c.R, c.G, c.B);
                            }
                            else
                                colors[i] = _singlecolor;

                            i++;
                        }

                        route = new GMapRouteGradient(newpts, "Route", colors, fwidth);
                    }
                    else
                        route = new GMapRoute(newpts, "Route");

                    _PtsRoute = newpts;
                    route.IsHitTestVisible = true;
                    route.Tag = _Daddy._cacheDetail._gmap; // très important pour le tooltip
                    Pen pen = new Pen(_singlecolor);
                    pen.Width = 2;// route.Stroke.Width;
                    route.Stroke = pen;
                    // On change le nom de cette route pour le tooltip
                    String kmmi = (_Daddy._bUseKm) ? _Daddy.GetTranslator().GetString("LVKm") : _Daddy.GetTranslator().GetString("LVMi"); 
                    double dist = (_Daddy._bUseKm) ? route.Distance : route.Distance * _Daddy._dConvKmToMi;
                    String tooltiptext = dist.ToString("0.0") + " " + kmmi;
                    route.Name = tooltiptext;
                    _Daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED4].Routes.Add(route);
                }
            }
            else
            {
                // On efface de toute façon
                EmptyAssociatedDecorations();
            }

            if (!_ComputeSpeed)
                lblSpeedMoy.Visible = false;
            else
            {
                // On calcule la vitesse moyenne
                _Daddy.Log("TrackSelector DrawBasedOnParameters: Compute mean speed");
                lblSpeedMoy.Visible = true;

                lblSpeedMoy.Text = _Daddy.GetTranslator().GetString("LblMeanSpeed");
                if (bRouteValid)
                {
                    GMapRoute route = _Daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED4].Routes[0];
                    String kmmph = (_Daddy._bUseKm) ? "kmh" : "mph";
                    double dist = (_Daddy._bUseKm) ? route.Distance : route.Distance * _Daddy._dConvKmToMi;
                    if ((_TimesRoute != null) && (_TimesRoute.Count >= 2))
                    {
                        double deltah = (_TimesRoute[_TimesRoute.Count - 1] - _TimesRoute[0]).TotalHours;
                        double speed = dist / deltah;
                        lblSpeedMoy.Text += speed.ToString("0.0 ") + kmmph;
                    }
                    else
                    {
                        lblSpeedMoy.Text += "-";
                    }
                }
                else
                {
                    DrawElevationsSpeed(0);
                    lblSpeedMoy.Text += "-";
                }
            }
        }

        private GMarkerGoogle CreateMarker(PointLatLng pt, GMarkerGoogleType type, String tooltip)
        {
            GMarkerGoogle marker = new GMarkerGoogle(pt, type);
            marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
            marker.ToolTipText = tooltip;
            return marker;
        }

        /// <summary>
        /// Display an HMI requesting parameters need for this form
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        /// <param name="bComputeSpeed">out, If true, speed will be computed from time and positions</param>
        /// <param name="speedColorType">out, Color type that will be used on road to display speed (only useful if bComputeSpeed is true)</param>
        /// <param name="singlecolor">out, Color value that will be used on road (only useful if bComputeSpeed is true and speedColorType is SingleValue or if bComputeSpeed is false)</param>
        /// <returns>True if parameters successfuly filled</returns>
        static public bool RequestParameters(MainWindow daddy, out bool bComputeSpeed, out TrackSelector.ColorType speedColorType, out Color singlecolor)
        {
            List<ParameterObject> lst = new List<ParameterObject>();
            List<String> lstcompare = new List<string>();
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Color, Color.Red, "color1", daddy.GetTranslator().GetString("LblRouteColorDefault")));
            lstcompare.Add(daddy.GetTranslator().GetString("LblSpeedGreenRed"));
            lstcompare.Add(daddy.GetTranslator().GetString("LblSpeedHSL"));
            lstcompare.Add(daddy.GetTranslator().GetString("LblSpeedUnicolor"));
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "usespeed", daddy.GetTranslator().GetString("LblDisplaySpeed")));
            lst.Add(new ParameterObject(ParameterObject.ParameterType.List, lstcompare, "speedcolor", daddy.GetTranslator().GetString("LblSpeedColorType")));
            
            ParametersChanger changer = new ParametersChanger();
            changer.Title = daddy.GetTranslator().GetString("FMenuDisplayGPXTrack");
            changer.BtnCancel = daddy.GetTranslator().GetString("BtnCancel");
            changer.BtnOK = daddy.GetTranslator().GetString("BtnOk");
            changer.ErrorFormater = daddy.GetTranslator().GetString("ErrWrongParameter");
            changer.ErrorTitle = daddy.GetTranslator().GetString("Error");
            changer.Parameters = lst;
            changer.Font = daddy.Font;
            changer.Icon = daddy.Icon;
            changer.TopMost = true;

            if (changer.ShowDialog() == DialogResult.OK)
            {
                bComputeSpeed = bool.Parse(changer.Parameters[1].Value);
                speedColorType = (TrackSelector.ColorType)(changer.Parameters[2].ValueIndex);
                singlecolor = (Color)(changer.Parameters[0].ValueO);
                return true;
            }
            else
            {
                bComputeSpeed = false;
                speedColorType = ColorType.SingleColor;
                singlecolor = Color.Red;
                return false;
            }
        }

        private void btnConfigure_Click(object sender, EventArgs e)
        {
            bool bUseSpeed;
            TrackSelector.ColorType speedcolor;
            Color singlecolor;
            TrackSelector.RequestParameters(_Daddy, out bUseSpeed, out speedcolor, out singlecolor);
            DrawBasedOnParameters(bUseSpeed, speedcolor, singlecolor);
        }

       
    }
}
