using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Threading;
using MyGeocachingManager.Geocaching;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;
using SpaceEyeTools;
using SpaceEyeTools.HMI;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Data.SQLite;
using SpaceEyeTools.EXControls;
using MyGeocachingManager.HMI;

namespace MyGeocachingManager
{
    /// <summary>
    /// Main Form to display caches information, any html content, map.
    /// </summary>
    public partial class CacheDetail : Form
    {
        static int iIndex = 0;
        	
        // For raster image (optional)
        private GMapRasterImage _rasterImage = null;
        PointLatLng _gtl = new PointLatLng();
        PointLatLng _gbr = new PointLatLng();

        /// <summary>
        /// Used to indicate if the form is totally initialized
        /// i.e. if all its controls are initialized.
        /// The main driver is the WebBrowser.
        /// As long as this one is not initialized and loaded with a dummy document,
        /// we wait...
        /// True : initialized and ready
        /// </summary>
        public bool _bInitialized = false;

        private TabPage _pageDefault = null;
        MainWindow _daddy = null;
        System.Windows.Forms.ToolTip _aToolTip = new System.Windows.Forms.ToolTip();
        
        // La carto
        // Le menu des cartes
        /// <summary>
        /// Key : prefix to identify aggregate
        /// ToolStripMenuItem : Associated submenu
        /// </summary>
        Dictionary<String, ToolStripMenuItem> _dicoAggregateProviders = new Dictionary<string, ToolStripMenuItem>();
        // Le menu des outils
        ToolStripMenuItem _tsiCacheOnly = null;
        ToolStripMenuItem _tsiPrefetchMap = null;

        /// <summary>
        /// Map control used to display any map related information in the CacheDetail Form
        /// </summary>
        public GMapControlCustom _gmap = null;

        private bool _bBigLabels = false;
        
        private bool HasBtn10 = false;
        
        // La boite de controle pour la carto
        private System.Windows.Forms.Panel panelItinerary = null;
        private System.Windows.Forms.Button btn4 = null;
        private System.Windows.Forms.Button btn3 = null;
        private System.Windows.Forms.Button btn2 = null;
        private System.Windows.Forms.Button btn1 = null;
        private System.Windows.Forms.Panel panelArea = null;
        private System.Windows.Forms.Label lbl1 = null;
        private System.Windows.Forms.Button btn9 = null;
        private System.Windows.Forms.Button btn8 = null;
        private System.Windows.Forms.Button btn7 = null;
        private System.Windows.Forms.Button btn6 = null;
        private System.Windows.Forms.Button btn5 = null;
        private System.Windows.Forms.Button btn10 = null;
        private System.Windows.Forms.Button btnCartoConfigure = null;
        private System.Windows.Forms.Label lblCoord = null;
        /// <summary>
        /// Button to enable cachecache
        /// </summary>
        public System.Windows.Forms.Button btnCacheCacheConfigure = null;
        private List<PointLatLng> itinerary_MarkerClicked = new List<PointLatLng>();
        private List<PointLatLng> measure_MarkerClicked = new List<PointLatLng>();

        /// <summary>
        /// Current list of points that are forming an area being drawn
        /// </summary>
        public List<PointLatLng> area_PointsClicked = new List<PointLatLng>();

        bool _bDoingItinerary = false;
        bool _walkingMode = false;
        bool _avoidHighways = false;
        bool _bDoingArea = false;
        bool _bDoingRoad = false;
        bool _bShowingArea = true;

        /// <summary>
        /// True if measurment is in progress
        /// </summary>
        public bool _bDoingMeasure = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="daddy">reference to MainForm, for callback purposes</param>
        public CacheDetail(MainWindow daddy)
        {
            _daddy = daddy;
            _daddy.Log("CacheDetail ctor");
            InitializeComponent();
            
            _daddy.Log("Creating GMap");
            _gmap = new GMap.NET.WindowsForms.GMapControlCustom();
            _gmap.Name = "CacheDetailGMap";
            if (ConfigurationManager.AppSettings["displayscaleonmap"] == "True")
                _gmap.boolUseCustomScale = true;
            else
                _gmap.boolUseCustomScale = false;
            _gmap.boolUseCustomScaleLabel = false;
            if (ConfigurationManager.AppSettings["useportablegmapcache"] == "True")
            {
                _gmap.CacheLocation = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "MapCache";
                _daddy.Log("Changing _gmap.CacheLocation to " + _gmap.CacheLocation);
            }
            else
                _daddy.Log("Keeping _gmap.CacheLocation at " + _gmap.CacheLocation);

            // On va creer le dico des sous menus de providers
            _daddy.Log("Creating providers list");
            _dicoAggregateProviders.Add("Google", new ToolStripMenuItem("Google maps"));
            _dicoAggregateProviders.Add("Bing", new ToolStripMenuItem("Bing maps"));
            _dicoAggregateProviders.Add("Open", new ToolStripMenuItem("OpenStreetMap"));
            _dicoAggregateProviders.Add("Ovi", new ToolStripMenuItem("Ovi maps"));
            _dicoAggregateProviders.Add("Yahoo", new ToolStripMenuItem("Yahoo maps"));
            _dicoAggregateProviders.Add("ArcGIS", new ToolStripMenuItem("ArcGIS"));
            _dicoAggregateProviders.Add("Near", new ToolStripMenuItem("Nearmap"));
            _dicoAggregateProviders.Add("Yandex", new ToolStripMenuItem("Yandex maps"));
            _dicoAggregateProviders.Add("Lithuania", new ToolStripMenuItem("Lithuania maps"));
            _dicoAggregateProviders.Add("Czech", new ToolStripMenuItem("Czech maps"));
            
            _daddy.Log("GMap init");
            _gmap.Daddy = _daddy;
            _gmap.DragButton = MouseButtons.Left;
            _gmap.DisableFocusOnMouseEnter = true;
            _gmap.Dock = System.Windows.Forms.DockStyle.Fill;
            _gmap.Bearing = 0F;
            _gmap.CanDragMap = true;
            _gmap.GrayScaleMode = false;
            _gmap.LevelsKeepInMemmory = 5;
            _gmap.Location = new System.Drawing.Point(0, 0);
            _gmap.MarkersEnabled = true;
            _gmap.MaxZoom = 25; // au lieu de 18
            _gmap.MinZoom = 2;
            _gmap.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
            _gmap.Name = "gmap";
            _gmap.NegativeMode = false;
            _gmap.PolygonsEnabled = true;
            _gmap.RetryLoadTile = 3; // 0 before
            _gmap.RoutesEnabled = true;
            _gmap.ShowTileGridLines = false;
            _gmap.Size = new System.Drawing.Size(150, 150);
            _gmap.TabIndex = 0;
            _gmap.Zoom = 13;

            // On met à jour le provider de notre carte au lieu de Google Map par défaut
            _daddy.Log("Update default provider");
            _daddy.UpdateMapProviderImpl(_gmap);

            _daddy.Log("Use server and cache");
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
            _gmap.Position = new PointLatLng(0, 0);

            _daddy.Log("Creating overlays");
            GMapWrapper.CreateOverlays(_gmap);

            _gmap.OnMapZoomChanged += new GMap.NET.MapZoomChanged(this.gmap_OnMapZoomChanged);
            _gmap.OnMarkerClick += new MarkerClick(cachedetail_OnMarkerClick);
            _gmap.OnRouteClick += new RouteClick(_daddy.cachedetail_OnRouteClick);
			//_gmap.OnPolygonClick += new PolygonClick(_daddy.cachedetail_OnPolygonClick);
            _gmap.MouseMove += new MouseEventHandler(cachedetail_OnMouseMove);
            _gmap.OnMapDrag += new MapDrag(cachedetail_OnMapDrag);

            TranslateForm();

            // on checke le bon provider
            CheckRightProvider(tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPPROVIDERS] as ToolStripMenuItem);
        }
        

        /// <summary>
        /// 
        /// </summary>
        public void DisplayCenterCoord()
        {
        	try
        	{
	        	if (lblCoord != null)
	        	{
	        		var sLat2 = CoordConvHMI.ConvertDegreesToDDMM(_gmap.Position.Lat, true);
	                var sLon2 = CoordConvHMI.ConvertDegreesToDDMM(_gmap.Position.Lng, false);
	                lblCoord.Text =  /*"DD° MM.MMM: " + */sLat2 + " " + sLon2;
	        	}
        	}
        	catch(Exception)
        	{
        		
        	}
        }
        
        /// <summary>
        /// Raise each time the map is move
        /// </summary>
        public void cachedetail_OnMapDrag()
        {
        	DisplayCenterCoord();
            _daddy._cachecache.PlayCacheCache(WorkerCacheCacheData.WhoLaunched.MAPDRAG);
        }
       
        /// <summary>
        /// Create or extend an existing measure by adding a new point to it
        /// This measure is displayed in the map control
        /// The measure shall contain at minimum 2 points to be drawn
        /// </summary>
        /// <param name="pt">New point of the measure</param>
        public void DefinePointOfAMeasure(PointLatLng pt)
        {
            int istart = measure_MarkerClicked.Count() - 1;
            measure_MarkerClicked.Add(pt);
            int iend = measure_MarkerClicked.Count() - 1;
            PointLatLng end = measure_MarkerClicked[iend];
            if (istart != -1)
            {
                // On a déjà un point, on peut creer une route
                PointLatLng start = measure_MarkerClicked[istart];
                List<PointLatLng> ptsRouteTmp = new List<PointLatLng>();
                ptsRouteTmp.Add(start);
                ptsRouteTmp.Add(end);
                GMapRoute route = new GMapRoute(ptsRouteTmp, "tmproute");
                // on créee la route
                route.IsHitTestVisible = true;
                route.Tag = _gmap; // très important pour le tooltip
                Pen pen = new Pen(Color.Red);
                pen.Width = 2; // route.Stroke.Width; 
                route.Stroke = pen;
                // On change le nom de cette route pour le tooltip
                String kmmi = (_daddy._bUseKm) ? _daddy.GetTranslator().GetString("LVKm") : _daddy.GetTranslator().GetString("LVMi");
                double dist = (_daddy._bUseKm) ? route.Distance : route.Distance * _daddy._dConvKmToMi;
                String stotal = "";
                double dtotal = 0.0;
                foreach (GMapRoute r in _gmap.Overlays[GMapWrapper.RESERVED1].Routes)
                {
                    dtotal += r.Distance;
                }
                if (dtotal != 0.0)
                {
                    dtotal += route.Distance;
                    if (!_daddy._bUseKm)
                        dtotal = dtotal * _daddy._dConvKmToMi;
                    stotal = " [" + dtotal.ToString("0.0") + "]";
                }
                String tooltiptext = dist.ToString("0.0") + stotal + " " + kmmi;

                route.Name = tooltiptext; 
                _gmap.Overlays[GMapWrapper.RESERVED1].Routes.Add(route);

                // Le marker de route
                GMapMarkerImage marker = new GMapMarkerImage(_daddy.GetImageSized("PolygonePoint"), end);
                marker.ToolTipMode = MarkerTooltipMode.Always;
                marker.ToolTipText = tooltiptext;
                _gmap.Overlays[GMapWrapper.RESERVED1].Markers.Add(marker);
            }
            else
            {
                // Un marker sur le premier point
                GMapMarkerImage marker = new GMapMarkerImage(_daddy.GetImageSized("PolygonePoint"), end);
                _gmap.Overlays[GMapWrapper.RESERVED1].Markers.Add(marker);
            }
        }

        /// <summary>
        /// Create or extend an existing route by adding a new point to it
        /// This route is displayed in the map control
        /// The route shall contain at minimum 2 points to be drawn
        /// </summary>
        /// <param name="pt">New point of the route</param>
        public void DefinePointOfARoute(PointLatLng pt)
        {
            int istart = itinerary_MarkerClicked.Count() - 1;
            itinerary_MarkerClicked.Add(pt);
            int iend = itinerary_MarkerClicked.Count() - 1;
            PointLatLng end = itinerary_MarkerClicked[iend];
            if (istart != -1)
            {
                // On a déjà un point, on peut creer une route
                PointLatLng start = itinerary_MarkerClicked[istart];
                List<PointLatLng> ptsRouteTmp = new List<PointLatLng>();
                ptsRouteTmp.Add(start);
                ptsRouteTmp.Add(end);
                GMapRoute route = new GMapRoute(ptsRouteTmp, "tmproute");
                // on créee la route
                _daddy.CreateRoutableShunks(_gmap, _gmap.Overlays[GMapWrapper.ITINERARY], route, _avoidHighways, _walkingMode, false);

                // Le marker de route
                GMapMarkerImage marker = new GMapMarkerImage(_daddy.GetImageSized("PolygonePoint"), end);
                _gmap.Overlays[GMapWrapper.ITINERARY].Markers.Add(marker);
            }
            else
            {
                // Un marker sur le premier point
                GMapMarkerImage marker = new GMapMarkerImage(_daddy.GetImageSized("PolygonePoint"), end);
                _gmap.Overlays[GMapWrapper.ITINERARY].Markers.Add(marker);
            }
        }

        /// <summary>
        /// Raised when a marker on the map is clicked
        /// This can add (DefinePointOfARoute) this marker to a route if we are drawing a route 
        /// Or this can simply call the generic process (anymap_OnMarkerClickImpl)
        /// </summary>
        /// <param name="item">Clicked marker</param>
        /// <param name="e">Mouse event</param>
        public void cachedetail_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            // IL FAUT PRESSER CTRL + LEFT CLICK !
            if (_bDoingItinerary && ((Control.ModifierKeys & Keys.Control) > 0))
            {
                DefinePointOfARoute(item.Position);
            }
            else
            {
                _daddy.anymap_OnMarkerClickImpl(item, e, _gmap);
               // Comportement par défaut
            }
        }


        /// <summary>
        /// Value of entry in the map menu
        /// </summary>
        public static int MENUMAPCIRCLE = 0;

        /// <summary>
        /// Value of entry in the map menu
        /// </summary>
        public static int MENUMAPDT = 0;

        /// <summary>
        /// Value of entry in the map menu
        /// </summary>
        public static int MENUMAPCODE = 0;

        /// <summary>
        /// Value of entry in the map menu
        /// </summary>
        public static int MENUMAPNAME = 0;

        /// <summary>
        /// Value of entry in the map menu
        /// </summary>
        public static int MENUMAPBIG = 0;

        /// <summary>
        /// Value of entry in the map menu
        /// </summary>
        public static int MENUMAPSTATS = 0;

        /// <summary>
        /// Value of entry in the map menu
        /// </summary>
        public static int MENUMAPWPTS = 0;

        /// <summary>
        /// Value of entry in the map menu
        /// </summary>
        public static int MENUMAPPROVIDERS = 0;

        /// <summary>
        /// Value of entry in the map menu
        /// </summary>
        public static int MENUMAPOUTILS = 0;

        /// <summary>
        /// Value of entry in the map menu
        /// </summary>
        public static int MENUMAPSCALE = 0;

        /// <summary>
        /// Value of entry in the map menu
        /// </summary>
        public static int MENUMAPNIGHT = 0;

        
        private ToolStripMenuItem CreateMenuEntryForMap(ContextMenuStrip menu, String translationEntry, Action<object, EventArgs> handler, ref int index)
        {
            ToolStripMenuItem item = _daddy.CreateTSMI(translationEntry, handler);
            menu.Items.Add(item);
            index = menu.Items.Count - 1;
            return item;
        }

        private ToolStripMenuItem CreateMenuEntryForMap(ToolStripMenuItem menu, String translationEntry, Image image, Action<object, EventArgs> handler, ref int index)
        {
            ToolStripMenuItem item = _daddy.CreateTSMI(translationEntry, image, handler);
            menu.DropDownItems.Add(item);
            index = menu.DropDownItems.Count - 1;
            return item;
        }

        /// <summary>
        /// Called when form needs to be translated
        /// </summary>
        public void TranslateForm()
        {
        	_daddy.Log("TranslateForm");
            {
                ContextMenuStrip mnuContextMenu = new ContextMenuStrip();
                mnuContextMenu.Items.Add(_daddy.CreateTSMI("MNUGeo", SetGoToWeb)); // 0 
                mnuContextMenu.Items.Add(_daddy.CreateTSMI("MNUCloseTab", SetCloseTab)); // 1
                mnuContextMenu.Items.Add(_daddy.CreateTSMI("MNUCloseAllTabs", SetCloseAllTabs)); // 2

                CreateMenuEntryForMap(mnuContextMenu, "FMenuDisplayDistMinTwoCaches", SetDisplayCirclesOnMap, ref CacheDetail.MENUMAPCIRCLE);
                CreateMenuEntryForMap(mnuContextMenu, "FMenudisplayDTOnGmaps", SetDisplayDTOnMap, ref CacheDetail.MENUMAPDT);
                CreateMenuEntryForMap(mnuContextMenu, "FMenudisplayStatsOnGmaps", SetDisplayStatsOnMap, ref CacheDetail.MENUMAPSTATS);
                CreateMenuEntryForMap(mnuContextMenu, "GrpHtmlShowWaypoints", SetDisplayWayptsOnMap, ref CacheDetail.MENUMAPWPTS);
                CreateMenuEntryForMap(mnuContextMenu, "LblGmapsCachesCode", SetDisplayCodeOnMap, ref CacheDetail.MENUMAPCODE);
                CreateMenuEntryForMap(mnuContextMenu, "LblGmapsCachesName", SetDisplayNameOnMap, ref CacheDetail.MENUMAPNAME);
                CreateMenuEntryForMap(mnuContextMenu, "ChckBigMarkersGmaps", SetDisplayBiggerLabelsOnMap, ref CacheDetail.MENUMAPBIG);
                ToolStripMenuItem tsi = CreateMenuEntryForMap(mnuContextMenu, "ChckScaleGmaps", SetDisplaySCaleOnMap, ref CacheDetail.MENUMAPSCALE);
                if (ConfigurationManager.AppSettings["displayscaleonmap"] == "True")
                {
                    tsi.Checked = true;
                }
				CreateMenuEntryForMap(mnuContextMenu, "CheckNightMode", SetNightMode, ref CacheDetail.MENUMAPNIGHT);
                
                // On va créer le sous menu de providers
                ToolStripMenuItem subCarto = CreateMenuEntryForMap(mnuContextMenu, "FMenuCarto", null, ref CacheDetail.MENUMAPPROVIDERS);

                // les providers les plus courant (raccourcis)
                List<int> providerIds = new List<int>();
                UpdateMenuTagForProviderAndExistingList(subCarto, _daddy.GetTranslator().GetString("FMenuCartoBing"), GMap.NET.MapProviders.BingMapProvider.Instance, providerIds);
                UpdateMenuTagForProviderAndExistingList(subCarto, _daddy.GetTranslator().GetString("FMenuCartoBingSat"), GMap.NET.MapProviders.BingSatelliteMapProvider.Instance, providerIds);
                UpdateMenuTagForProviderAndExistingList(subCarto, _daddy.GetTranslator().GetString("FMenuCartoBingHybrid"), GMap.NET.MapProviders.BingHybridMapProvider.Instance, providerIds);
                UpdateMenuTagForProviderAndExistingList(subCarto, _daddy.GetTranslator().GetString("FMenuCartoOSM"), GMap.NET.MapProviders.OpenStreetMapProvider.Instance, providerIds);
                UpdateMenuTagForProviderAndExistingList(subCarto, _daddy.GetTranslator().GetString("FMenuCartoGoogle"), GMap.NET.MapProviders.GoogleMapProvider.Instance, providerIds);
                UpdateMenuTagForProviderAndExistingList(subCarto, _daddy.GetTranslator().GetString("FMenuCartoGoogleTerrain"), GMap.NET.MapProviders.GoogleTerrainMapProvider.Instance, providerIds);
                UpdateMenuTagForProviderAndExistingList(subCarto, _daddy.GetTranslator().GetString("FMenuCartoGoogleSat"), GMap.NET.MapProviders.GoogleSatelliteMapProvider.Instance, providerIds);
                UpdateMenuTagForProviderAndExistingList(subCarto, _daddy.GetTranslator().GetString("FMenuCartoGoogleHybrid"), GMap.NET.MapProviders.GoogleHybridMapProvider.Instance, providerIds);

                // Reste des providers
                ToolStripSeparator itemprovider = new ToolStripSeparator();
                subCarto.DropDownItems.Add(itemprovider); // non utilisé
                itemprovider.Tag = null;

                // On peuple avec tous les providers disponibles
                List<GMap.NET.MapProviders.GMapProvider> providerslist = GMap.NET.MapProviders.GMapProviders.List;

                // On ajoute tous les aggregates
                foreach(KeyValuePair<String, ToolStripMenuItem> pair in _dicoAggregateProviders)
                {
                    pair.Value.Enabled = true;
                    subCarto.DropDownItems.Add(pair.Value);
                }

                // On ajoute les providers, éventuellement dans les aggregats
                foreach (GMap.NET.MapProviders.GMapProvider provider in providerslist)
                {
                    _daddy.Log("Found provider " + provider.Name + " " + provider.DbId.ToString());
                    UpdateMenuTagForProviderAndExistingList(subCarto, provider, providerIds);
                }

                // On désactive tous les aggregats vides
                foreach (KeyValuePair<String, ToolStripMenuItem> pair in _dicoAggregateProviders)
                {
                    if (pair.Value.DropDownItems.Count == 0)
                        pair.Value.Enabled = false;
                }

                // On va créer un menu pour les outils
                // FMenuTools
                ToolStripMenuItem subOutils = CreateMenuEntryForMap(mnuContextMenu, "FMenuTools", null, ref CacheDetail.MENUMAPOUTILS);
                subOutils.DropDownItems.Add(_daddy.CreateTSMI("FMenuSaveMapAsBitmap", SaveMapAsBitmap));
                subOutils.DropDownItems.Add(_daddy.CreateTSMI("FMenuSearchCity", LocateACity, _daddy.GetInternetStatus()));
                subOutils.DropDownItems.Add(_daddy.CreateTSMI("FMenuOpenGMapsFromCenter", OpenGoogleMapsFromCenterPostion, _daddy.GetInternetStatus()));
                subOutils.DropDownItems.Add(_daddy.CreateTSMI("FMenuCenterCartoOnCoord", CenterCartoOnCoord));
                subOutils.DropDownItems.Add(_daddy.CreateTSMI("FMenuCopyCenterCartoCoord", CopyCenterCartoCoord));
				
                subOutils.DropDownItems.Add(new ToolStripSeparator());
                _tsiPrefetchMap = _daddy.CreateTSMI("FMenuCacheTileMap", CacheTileMap);
                subOutils.DropDownItems.Add(_tsiPrefetchMap);
                _tsiCacheOnly = _daddy.CreateTSMI("FMenuToggleCacheonly", ToggleCacheOnly);
                subOutils.DropDownItems.Add(_tsiCacheOnly);
								
                subOutils.DropDownItems.Add(new ToolStripSeparator());
                subOutils.DropDownItems.Add(_daddy.CreateTSMI("FMenuLoadRasterImage", LoadRasterImage));
                subOutils.DropDownItems.Add(_daddy.CreateTSMI("FMenuEraseRasterImage", EraseRasterImage));

                tabControlCD._mnuContextMenu = mnuContextMenu;
            }

            this.Text = _daddy.GetTranslator().GetString("CacheDetailTitle");
            TranslateMapToolbar();
            _daddy.TranslateTooltips(this, null);
            _daddy.TranslateTooltips(tabControlCD._mnuContextMenu, null);
        }

        /// <summary>
        /// Enable / disable cache only for ALL maps display
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">params</param>
        public void ToggleCacheOnly(object sender, EventArgs e)
        {
        	if (GMap.NET.GMaps.Instance.Mode == GMap.NET.AccessMode.CacheOnly)
        	{
        		GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
        		if (_tsiCacheOnly != null) _tsiCacheOnly.Checked = false;
        		if (_tsiPrefetchMap != null) _tsiPrefetchMap.Enabled = true;
        		
        	}
        	else
        	{
        		GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.CacheOnly;
        		if (_tsiCacheOnly != null) _tsiCacheOnly.Checked = true;
        		if (_tsiPrefetchMap != null) _tsiPrefetchMap.Enabled = false;
        	}
        	
        	// On restaure l'affichage
            _daddy._cachesPreviewMap.Refresh();
            _gmap.Refresh();

            // On affiche la bonne image
            _daddy.cachesPreviewMap_OnMapZoomChanged();
            gmap_OnMapZoomChanged();
        }
        
        /// <summary>
        /// Add current selection of map (using ALT+Right click) to
        /// cache image, from zoom level 1 to zoom level max.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">params</param>
        public void CacheTileMap(object sender, EventArgs e)
        {
        	
        	try
        	{
	        	RectLatLng area = _gmap.SelectedArea;
	            if (!area.IsEmpty)
	            {
                    _daddy.ClearOverlay_RESERVED2();
	            	
	            	int iMinZoom = (int)_gmap.Zoom;
	            	int iMaxZoom = _gmap.MaxZoom;
                    DialogResult res = DialogResult.OK;

	                for (int i = iMinZoom; i <= iMaxZoom; i++)
	                {
	                    if (res == DialogResult.OK)
	                    {
                            using (TilePrefetcherEnh obj = new TilePrefetcherEnh())
	                        {
	                        	obj.Text = _daddy.GetTranslator().GetString("LblEscToCancel");
                                obj.LblFetchingMessageFormat = _daddy.GetTranslator().GetString("LblFetchingProgressMsg");
                                obj.LblAllTileSaved = _daddy.GetTranslator().GetString("LblAllTileSaved");
                                obj.LblSavingTiles = _daddy.GetTranslator().GetString("LblSavingTiles");
                                obj.LblTileToSave = _daddy.GetTranslator().GetString("LblTileToSave");
                                obj.LblPrefetchComplete = _daddy.GetTranslator().GetString("LblPrefetchComplete");
                                obj.LblPrefetchCancelled = _daddy.GetTranslator().GetString("LblPrefetchCancelled");
                                obj.LblWaiting = _daddy.GetTranslator().GetString("LblWaitingNoTime");
                                
                                obj.zoomfinal = iMaxZoom;
	                            obj.Overlay = _gmap.Overlays[GMapWrapper.RESERVED2]; // set overlay if you want to see cache progress on the map
	                            obj.Shuffle = _gmap.Manager.Mode != AccessMode.CacheOnly;
	                            obj.Owner = this;
	                            obj.ShowCompleteMessage = false;
                                obj.Start(area, i, _gmap.MapProvider, _gmap.Manager.Mode == AccessMode.CacheOnly ? 0 : 100, _gmap.Manager.Mode == AccessMode.CacheOnly ? 0 : 1);
                                res = obj.DialogResult;                            
	                        }
	                    }
	                   
	                    if (res == DialogResult.Abort)
	                        break;
	                }
	                // Déjà inclu dans le cache local, pas besoin de l'exporter pour le réimporter plus tard
	                //_gmap.ShowExportDialog();
	
	               _daddy.MsgActionDone(this);
	                
	                _daddy.ClearOverlay_RESERVED2();
	                
	            }
	            else
	            {
	            	_daddy.MsgActionWarning(this, _daddy.GetTranslator().GetString("LblMissingMapSelection"));
	            }
        	}
        	catch(Exception exc)
        	{
        		_daddy.ShowException("", _daddy.GetTranslator().GetString("FMenuCacheTileMap"), exc);
       		
        		_daddy.ClearOverlay_RESERVED2();
        	}
        
        }
        
        
        /// <summary>
        /// Load a raster image (.ras) and place it on overlay RESERVED3
		/// Format of raster file (.ras), text file of 3 lines:
		/// - Line 1, filename (absolute, relative or local to .ras file): Fichier "filename"
        /// - Line 2, upper left point for which we know coordinates in decimal: HG "X" "Y" "Latitude" "Longitude" 
        /// - Line 3, bottom right point for which we know coordinates in decimal degrees: BD "X" "Y" "Latitude" "Longitude" 
		/// Sample ras file: 
		/// Fichier C:\Users\spaceeye\Desktop\2015-08-11_12-07-22.jpg 
		/// HG 68 130 48.860586 0.995904 
		/// BD 1315 505 48.690882 1.852302 
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event arg</param>
        public void LoadRasterImage(object sender, EventArgs e)
        {
            EraseRasterImage(null, null);
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "Raster image (*.ras)|*.ras";
                openFileDialog1.Multiselect = false;
                openFileDialog1.Title = _daddy.GetTranslator().GetString("FMenuLoadRasterImage");

                DialogResult dr = openFileDialog1.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    string[] filenames = openFileDialog1.FileNames;

                    // First we will read the file
                    // Strucure is as follow (3 lines) :
                    // Fichier C:\Users\spaceeye\Desktop\2015-08-11_12-07-22.jpg
                    // HG 68 130 48.860586 0.995904
                    // BD 1315 505 48.690882 1.852302
                    int iLigne = 0;
                    List<string> _lines = new List<string>();
                    String[] vals = null;
                    using (StreamReader r = new StreamReader(filenames[0]))
                    {
                        string line;
                        while ((line = r.ReadLine()) != null)
                        {
                            _lines.Add(line);
                        }
                    }

                    // Now load raster image
                    String rfile = (_lines[iLigne++].Split(' '))[1];
                    String curdir = Directory.GetCurrentDirectory();
                    Image rasterImg = null;
                    try
                    {
                        String raspath = Path.GetDirectoryName(filenames[0]);
                        Directory.SetCurrentDirectory(raspath);

                        //if (Path.IsPathRooted(rfile) == false) // true = chemin avec un path relatif ou absolu, on va charger direct
                        rasterImg = Image.FromFile(rfile);

                        Directory.SetCurrentDirectory(curdir);
                    }
                    catch (Exception)
                    {
                        Directory.SetCurrentDirectory(curdir);
                        throw;
                    }

                    // And compute geocoding function
                    Size sOri = new Size(rasterImg.Size.Width, rasterImg.Size.Height);
                    vals = _lines[iLigne++].Split(' ');
                    double xHG = double.Parse(vals[1], System.Globalization.CultureInfo.InvariantCulture);
                    double yHG = double.Parse(vals[2], System.Globalization.CultureInfo.InvariantCulture);
                    double latHG = double.Parse(vals[3], System.Globalization.CultureInfo.InvariantCulture);
                    double lonHG = double.Parse(vals[4], System.Globalization.CultureInfo.InvariantCulture);
                    vals = _lines[iLigne++].Split(' ');
                    double xBD = double.Parse(vals[1], System.Globalization.CultureInfo.InvariantCulture);
                    double yBD = double.Parse(vals[2], System.Globalization.CultureInfo.InvariantCulture);
                    double latBD = double.Parse(vals[3], System.Globalization.CultureInfo.InvariantCulture);
                    double lonBD = double.Parse(vals[4], System.Globalization.CultureInfo.InvariantCulture);
                    // le x va avec le lon,
                    // le y va avec le lat.
                    // longitude = Ax * x + Bx
                    // latitude = Ay * y + By
                    double Ax = (lonBD - lonHG) / (xBD - xHG);
                    double Bx = lonHG - Ax * xHG;
                    double Ay = (latBD - latHG) / (yBD - yHG);
                    double By = latHG - Ay * yHG;
                    Point pt = new Point(0, 0);
                    _gtl = Locate(pt, Ax, Bx, Ay, By);
                    pt = new Point(sOri.Width - 1, sOri.Height - 1);
                    _gbr = Locate(pt, Ax, Bx, Ay, By);
                   
                    _rasterImage = new GMapRasterImage(_gtl);

                    List<ParameterObject> lst = new List<ParameterObject>();
                    lst.Add(new ParameterObject(ParameterObject.ParameterType.Int, 50, "percentopacity", _daddy.GetTranslator().GetString("LblPercentageOpacity")));

                    ParametersChanger changer = new ParametersChanger();
                    changer.Title = _daddy.GetTranslator().GetString("FMenuLoadRasterImage");
                    changer.BtnCancel = _daddy.GetTranslator().GetString("BtnCancel");
                    changer.BtnOK = _daddy.GetTranslator().GetString("BtnOk");
                    changer.ErrorFormater = _daddy.GetTranslator().GetString("ErrWrongParameter");
                    changer.ErrorTitle = _daddy.GetTranslator().GetString("Error");
                    changer.Parameters = lst;
                    changer.Font = this.Font;
                    changer.Icon = this.Icon;

                    float opacity = 0.5f;
                    if (changer.ShowDialog() == DialogResult.OK)
                    {
                        opacity = (float)(Int16.Parse(lst[0].Value))/100.0f;
                    }
                    _rasterImage.Image = SetImageOpacity(rasterImg, opacity);
                    _gmap.Overlays[GMapWrapper.RESERVED3].Markers.Add(_rasterImage);
                    DrawRasterImage();
                }
            }
            catch (Exception exc)
            {
            	_daddy.ShowException("", _daddy.GetTranslator().GetString("FMenuLoadRasterImage"), exc);
                
            }
        }

        /// <summary>
        /// Erase a previously loader raster image and refresh the map
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event arg</param>
        public void EraseRasterImage(object sender, EventArgs e)
        {
            if (_rasterImage != null)
            {
                _gmap.Overlays[GMapWrapper.RESERVED3].Markers.Remove(_rasterImage);
            }

            _rasterImage = null;
            _gbr = new PointLatLng();
            _gtl = new PointLatLng();
            _gmap.Refresh();
        }

        /// <summary>
        /// Set opacity of a specified image
        /// </summary>  
        /// <param name="image">image to set opacity on</param>  
        /// <param name="opacity">percentage of opacity</param>  
        /// <returns>new image with adjusted opacity</returns>  
        static public Image SetImageOpacity(Image image, float opacity)
        {
            try
            {
                //create a Bitmap the size of the image provided  
                Bitmap bmp = new Bitmap(image.Width, image.Height);

                //create a graphics object from the image  
                using (Graphics gfx = Graphics.FromImage(bmp))
                {

                    //create a color matrix object  
                    ColorMatrix matrix = new ColorMatrix();

                    //set the opacity  
                    matrix.Matrix33 = opacity;

                    //create image attributes  
                    ImageAttributes attributes = new ImageAttributes();

                    //set the color(opacity) of the image  
                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    //now draw the image  
                    gfx.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
                }
                return bmp;
            }
            catch (Exception)
            {
            	throw;
            }
        } 

        private PointLatLng Locate(Point pt, double Ax, double Bx, double Ay, double By)
        {
            return new PointLatLng(
                pt.Y * Ay + By,
                pt.X * Ax + Bx);
        }

        private void DrawRasterImage()
        {
            if (_rasterImage != null)
            {
                var tl = _gmap.FromLatLngToLocal(_gtl);
                var br = _gmap.FromLatLngToLocal(_gbr);

                _rasterImage.Position = _gtl;
                _rasterImage.Size = new System.Drawing.Size((int)(br.X - tl.X), (int)(br.Y - tl.Y));
            }
        }

        /// <summary>
        /// Will save the map as an image file
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public void SaveMapAsBitmap(object sender, EventArgs e)
		{
		 	SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "PNG (*.png)|*.png";
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
            	try
            	{
            		Image img = _gmap.ToImage();
            		img.Save(saveFileDialog1.FileName, ImageFormat.Png);
            		LoadPage( _daddy.GetTranslator().GetString("LblSavedMap"), saveFileDialog1.FileName);
            	}
            	catch(Exception exc)
            	{
            		_daddy.ShowException("", _daddy.GetTranslator().GetString("FMenuSaveMapAsBitmap"), exc);
            	}
            }
		}
        
        /// <summary>
        /// Will copy the map center coordinates into the clipboard
        /// latitude then longitude, in decimal degress, separated by a space
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public void CopyCenterCartoCoord(object sender, EventArgs e)
        {
            PointLatLng center = _gmap.Position;
            Clipboard.SetText(center.Lat.ToString().Replace(",", ".") + " " + center.Lng.ToString().Replace(",", "."));
        }

        /// <summary>
        /// Will ask user to enter coordinates on which map will be centered
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public void CenterCartoOnCoord(object sender, EventArgs e)
        {
            List<ParameterObject> lst = new List<ParameterObject>();
            
            PointLatLng center = _gmap.Position;
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Coordinates/*good not needed*/,
                                        center.Lat.ToString().Replace(",", ".") + " " + center.Lng.ToString().Replace(",", "."),
                                        "latlon",
                                        _daddy.GetTranslator().GetString("ParamCenterLatLon"),
                                        _daddy.GetTranslator().GetStringM("TooltipParamLatLon")));
            

            ParametersChanger changer = new ParametersChanger();
            changer.Title = _daddy.GetTranslator().GetString("FMenuCenterCartoOnCoord");
            changer.BtnCancel = _daddy.GetTranslator().GetString("BtnCancel");
            changer.BtnOK = _daddy.GetTranslator().GetString("BtnOk");
            changer.ErrorFormater = _daddy.GetTranslator().GetString("ErrWrongParameter");
            changer.ErrorTitle = _daddy.GetTranslator().GetString("Error");
            changer.Parameters = lst;
            changer.Font = this.Font;
            changer.Icon = this.Icon;

            // Force creation du get handler on control
            changer.CreateControls();
            _daddy._cacheDetail._gmap.ControlTextLatLon = null; // no callback here, no point to do that !
            
            if (changer.ShowDialog() == DialogResult.OK)
            {
               
                Double dlon = Double.MaxValue;
                Double dlat = Double.MaxValue;
                if (ParameterObject.SplitLongitudeLatitude(lst[0].Value, ref dlon, ref dlat))
                {
                	_gmap.Position = new PointLatLng(dlat, dlon);
                }
            }
        }

        /// <summary>
        /// Will ask user a city name on which map will be centered
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public void LocateACity(object sender, EventArgs e)
        {
            try
            {
                List<ParameterObject> lst = new List<ParameterObject>();
                lst.Add(new ParameterObject(ParameterObject.ParameterType.String, "", "city", _daddy.GetTranslator().GetString("LblCityName")));

                ParametersChanger changer = new ParametersChanger();
                changer.Title = _daddy.GetTranslator().GetString("FMenuSearchCity");
                changer.BtnCancel = _daddy.GetTranslator().GetString("BtnCancel");
                changer.BtnOK = _daddy.GetTranslator().GetString("BtnOk");
                changer.ErrorFormater = _daddy.GetTranslator().GetString("ErrWrongParameter");
                changer.ErrorTitle = _daddy.GetTranslator().GetString("Error");
                changer.Parameters = lst;
                changer.Font = this.Font;
                changer.Icon = this.Icon;

                if (changer.ShowDialog() == DialogResult.OK)
                {
                    String city = lst[0].Value;

                    String cityFound = "";
                    Coordinate coord;
                    if (MyTools.GetCoordinate(_daddy.GetProxy(), city, out coord, out cityFound))
                    {
                    	_daddy.MsgActionOk(this, cityFound);
                    	_gmap.Position = new PointLatLng(coord.Latitude, coord.Longitude);
                    }
                    else
                    {
                    	_daddy.MsgActionError(this, _daddy.GetTranslator().GetString("lblErrorCityNotFound"));
                    }
                }
            }
            catch (Exception exc)
            {
            	_daddy.ShowException("", _daddy.GetTranslator().GetString("FMenuSearchCity"), exc);
                
            }
        }

        private void OpenGoogleMapsFromCenterPostion(object sender, EventArgs e)
        {
            String url = "https://maps.google.com/maps?q=" + 
                _gmap.Position.Lat.ToString().Replace(",",".") +
                "+" + _gmap.Position.Lng.ToString().Replace(",", ".");
            MyTools.StartInNewThread(url);
        }

        private void UpdateMenuTagForProviderAndExistingList(ToolStripMenuItem dad, String label, GMap.NET.MapProviders.GMapProvider provider, List<int> providerIds)
        {
            if (providerIds.Contains(provider.DbId))
            {
                // Déjà contenu on ne le rajoute pas
            }
            else
            {
                bool bRoutable = false;
                bool bDurationAvailable = false;
                IsRoutableProvider(provider, ref bRoutable, ref bDurationAvailable);
                String lbl = label;
                if (bRoutable)
                    lbl += " " + _daddy.GetTranslator().GetString("LblRoutable");

                // Look for an entry in dico ?
                foreach (KeyValuePair<String, ToolStripMenuItem> pair in _dicoAggregateProviders)
                {
                    if (pair.Value.DropDownItems.Count == 0)
                        pair.Value.Enabled = false;
                }
                ToolStripMenuItem itemprovider = new ToolStripMenuItem(lbl, null, new EventHandler(SetMapProviderGeneric));
                dad.DropDownItems.Add(itemprovider);
                itemprovider.Tag = provider;
                providerIds.Add(provider.DbId);
            }
        }

        private void UpdateMenuTagForProviderAndExistingList(ToolStripMenuItem dad, GMap.NET.MapProviders.GMapProvider provider, List<int> providerIds)
        {
            if (providerIds.Contains(provider.DbId))
            {
                // Déjà contenu on ne le rajoute pas
            }
            else
            {
                bool bRoutable = false;
                bool bDurationAvailable = false;
                IsRoutableProvider(provider, ref bRoutable, ref bDurationAvailable);
                String lbl = provider.Name;
                if (bRoutable)
                    lbl += " " + _daddy.GetTranslator().GetString("LblRoutable");

                ToolStripMenuItem itemprovider = null;
                foreach (KeyValuePair<String, ToolStripMenuItem> pair in _dicoAggregateProviders)
                {
                    if (provider.Name.StartsWith(pair.Key))
                    {
                        // We have a match !
                        itemprovider = new ToolStripMenuItem(lbl, null, new EventHandler(SetMapProviderGeneric));
                        // Add to aggregate
                        pair.Value.DropDownItems.Add(itemprovider);
                        break;
                    }
                }
                // No match found, simple provider
                if (itemprovider == null)
                {
                    itemprovider = new ToolStripMenuItem(lbl, null, new EventHandler(SetMapProviderGeneric));
                    dad.DropDownItems.Add(itemprovider);
                }

                itemprovider.Tag = provider;
                providerIds.Add(provider.DbId);
            }
        }

        private void TranslateMapToolbar()
        {
            if (btn1 != null)
                _aToolTip.SetToolTip(btn1,_daddy.GetTranslator().GetStringM("BtnDrawRoute"));
            if (btn2 != null)
                _aToolTip.SetToolTip(btn2,_daddy.GetTranslator().GetStringM("BtnClearRoute"));
            if (btn3 != null)
                _aToolTip.SetToolTip(btn3,_daddy.GetTranslator().GetStringM("BtnHighwayMode"));
            if (btn4 != null)
               _aToolTip.SetToolTip( btn4,_daddy.GetTranslator().GetStringM("BtnPedestrianMode"));
            if (btn10 != null)
            	_aToolTip.SetToolTip(btn10,_daddy.GetTranslator().GetStringM("BtnDrawroad"));
            if (btn5 != null)
                _aToolTip.SetToolTip(btn5,_daddy.GetTranslator().GetStringM("BtnDrawArea"));
            if (btn6 != null)
                _aToolTip.SetToolTip(btn6,_daddy.GetTranslator().GetStringM("BtnClearArea"));
            if (btn7 != null)
                _aToolTip.SetToolTip(btn7,_daddy.GetTranslator().GetStringM("BtnDisplayAreaMap"));
            if (btn8 != null)
                _aToolTip.SetToolTip(btn8, _daddy.GetTranslator().GetStringM("BtnCenterOnArea"));
            if (btn9 != null)
                _aToolTip.SetToolTip(btn9, _daddy.GetTranslator().GetStringM("BtnMeasureDistance"));
            if (btnCartoConfigure != null)
                _aToolTip.SetToolTip(btnCartoConfigure, _daddy.GetTranslator().GetStringM("BtnCartoConfiguration"));
            if (btnCacheCacheConfigure != null)
                _aToolTip.SetToolTip(btnCacheCacheConfigure, _daddy.GetTranslator().GetStringM("btnCacheCacheConfigure"));
        }

        /// <summary>
        /// Check if a map provider is routable
        /// So far, only Bing and OSM are routable providers
        /// </summary>
        /// <param name="provider">Provider to check</param>
        /// <param name="bRoutable">True if the provider is routable</param>
        /// <param name="bDurationAvailable">True if the provider will give a route duration (always false)</param>
        static public void IsRoutableProvider(GMap.NET.MapProviders.GMapProvider provider, ref bool bRoutable, ref bool bDurationAvailable)
        {
            // On teste déjà le cast en RoutingProvider mais ça ne marche pas toujours !!!
            RoutingProvider rp = provider as RoutingProvider;
            if ((provider == null) || (rp == null))
            {
                bDurationAvailable = bRoutable = false;
                return;
            }
            
            Type typeProvider = provider.GetType();
            if (typeProvider == typeof(GMap.NET.MapProviders.BingMapProvider))
            {
                bRoutable = true;
                bDurationAvailable = false;
            }
            else if (typeProvider == typeof(GMap.NET.MapProviders.OpenStreetMapProvider))
            {
                bRoutable = true;
                bDurationAvailable = false;
            }
            else
            {
                bRoutable = false;
                bDurationAvailable = false;
            }
        }    

        private void CheckRightProvider(ToolStripMenuItem mnup)
        {
            try
            {
            	if (mnup == null)
            		return;
                int dbidcurrent = _gmap.MapProvider.DbId;
                //_daddy.Log("dbidcurrent " + dbidcurrent.ToString());
                foreach (Object o in mnup.DropDownItems)
                {
                    if (o.GetType() == typeof(ToolStripMenuItem))
                    {
                        ToolStripMenuItem item = (ToolStripMenuItem)o;
                        item.Checked = false;
                        if (item.Tag != null)
                        {
                            GMap.NET.MapProviders.GMapProvider provider = item.Tag as GMap.NET.MapProviders.GMapProvider;
                            if (provider != null)
                            {
                                if (provider.DbId == dbidcurrent)
                                    item.Checked = true;
                            }
                        }
                        
                        //if ((item.DropDownItems != null) && (item.DropDownItems.Count != 0))
                        //	CheckRightProvider(item);
                    }
                }

                // On parcourt les providers dans les aggregates
                // Si on se trouve dans un aggregate, on italique l'aggregagte
                foreach (KeyValuePair<String, ToolStripMenuItem> pair in _dicoAggregateProviders)
                {
                    ToolStripMenuItem mnu = pair.Value;
                    mnu.Checked = false;
                    //_daddy.Log("Analyse " + mnu.Text);
                    foreach (Object o in mnu.DropDownItems)
                    {
                        if (o.GetType() == typeof(ToolStripMenuItem))
                        {
                            ToolStripMenuItem item = (ToolStripMenuItem)o;
                            item.Checked = false;
                            //_daddy.Log("    Analyse fils " + item.Text);
                            if (item.Tag != null)
                            {
                                GMap.NET.MapProviders.GMapProvider provider = item.Tag as GMap.NET.MapProviders.GMapProvider;
                                if (provider != null)
                                {
                                	//_daddy.Log("        provider " + provider.DbId.ToString());
                                    if (provider.DbId == dbidcurrent)
                                    {
                                    	//_daddy.Log("            checked");
                                        item.Checked = true;
                                        mnu.Checked = true;
                                    }
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception)
            {
            }
        }

        private void SetMapProviderGeneric(object sender, EventArgs e)
        {
            ToolStripMenuItem mnu = sender as ToolStripMenuItem;
            if (mnu != null)
            {
                GMap.NET.MapProviders.GMapProvider provider = mnu.Tag as GMap.NET.MapProviders.GMapProvider;
                if (provider != null)
                {
                	_gmap.AssignMapProvider(provider);
                    // Mise à jour de DADDY
                    _daddy.UpdateConfFile("MapProvider", _gmap.MapProvider.DbId.ToString());
                    _daddy.UpdateMapProviderImpl(_daddy._cachesPreviewMap);

                    // Et on checke le bon
                    CheckRightProvider(tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPPROVIDERS] as ToolStripMenuItem);
                    
                    EnableOrNotRouting();
                }
            }
        }

        /// <summary>
        /// Based on the provider "routability" (IsRoutableProvider), will activate / deactivate all routing relating functions 
        /// Legacy, does not perform ANYTHING
        /// </summary>
        public void EnableOrNotRouting()
        {
            try
            {
                // On autorise tout le temps le routage pour l'instant, on peut s'en sortir avec les directions
                /*
                bool bRoutable = false;
                bool bDurationAvailable = false;
                IsRoutableProvider(_gmap.MapProvider, ref bRoutable, ref bDurationAvailable);
                if (bRoutable)
                {
                    panelItinerary.Enabled = true;
                }
                else
                {
                    // On desactive
                    panelItinerary.Enabled = false;

                    // Et on stoppe le routing éventuellement en cours
                    if (_bDoingItinerary)
                    {
                        btn1_Click(null, null);
                    }
                }
                */
            }
            catch (Exception ex)
            {
            	_daddy.Log(MainWindow.GetException("EnableOrNotRouting", ex));
            }
        }

        /// <summary>
        /// Remove all decoration markers from map :
        /// - Statistics markers
        /// - Proximity circles
        /// - Difficulty and terrain
        /// - Cache codes
        /// - Cache names
        /// </summary>
        public void EmptyAllDecorationOverlays()
        {
            // Les markers et les waypoints sont gérés ailleurs et ROUTE / AREA ne doit pas être effacée (indépendant des caches)
            //EmptyRouteMarkers();
            EmptyCircledDTCodeNameMarkers();
            EmptyStatsMarkers();
        }

        /// <summary>
        /// Remove all routes and associated markers (route tooltips) from map
        /// </summary>
        public void EmptyRouteMarkers()
        {
            itinerary_MarkerClicked.Clear();
            _gmap.Overlays[GMapWrapper.ITINERARY].Routes.Clear();
            _gmap.Overlays[GMapWrapper.ITINERARY].Markers.Clear(); // Ca c'est pour le marker tooltip qui peut éventuellement trainer là
            // Ca contient potentiellement le marker utilisé pour le tooltip
            _gmap.Overlays[GMapWrapper.RESERVED1].Markers.Clear();
        }

        /// <summary>
        /// Remove all measure and associated markers (measure tooltips) from map
        /// </summary>
        public void EmptyMeasureMarkers()
        {
            measure_MarkerClicked.Clear();
            // Ca contient le marker utilisé pour le tooltip et les mesures
            _gmap.Overlays[GMapWrapper.RESERVED1].Routes.Clear();
            _gmap.Overlays[GMapWrapper.RESERVED1].Markers.Clear();
        }

        /// <summary>
        /// Remove all area and associated markers (area tooltips) from map
        /// </summary>
        public void EmptyAreaMarkers()
        {
            area_PointsClicked.Clear();
            if (lbl1 != null) // Si on n'a jamais créé la fenêtre avant...
                lbl1.Text = "-"; // On efface le nombre de points présents dans la zone
            _gmap.Overlays[GMapWrapper.AREA].Polygons.Clear();
            _gmap.Overlays[GMapWrapper.AREA].Markers.Clear(); // Ca c'est pour le marker tooltip qui peut éventuellement trainer là
            _daddy.ResetAreaFilter();
        }

        /// <summary>
        /// Remove decoration markers from map :
        /// - Statistics markers
        /// </summary>
        public void EmptyStatsMarkers()
        {
            // Et pareil avec les cercles et D/T de cachedetail
            _gmap.Overlays[GMapWrapper.STATS].Markers.Clear();
            _gmap.Overlays[GMapWrapper.STATS].IsVisibile = false;
            (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPSTATS] as ToolStripMenuItem).Checked = false;
        }

        /// <summary>
        /// Remove decoration markers from map :
        /// - Proximity circles
        /// - Difficulty and terrain
        /// - Cache codes
        /// - Cache names
        /// </summary>
        public void EmptyCircledDTCodeNameMarkers()
        {
            // Et pareil avec les cercles et D/T de cachedetail
            _gmap.Overlays[GMapWrapper.CIRCLES].Markers.Clear();
            _gmap.Overlays[GMapWrapper.DT].Markers.Clear();
            _gmap.Overlays[GMapWrapper.CODES].Markers.Clear();
            _gmap.Overlays[GMapWrapper.NAMES].Markers.Clear();
            _gmap.Overlays[GMapWrapper.WAYPOINTS].Markers.Clear();

            _gmap.Overlays[GMapWrapper.CIRCLES].IsVisibile = false;
            _gmap.Overlays[GMapWrapper.DT].IsVisibile = false;
            _gmap.Overlays[GMapWrapper.CODES].IsVisibile = false;
            _gmap.Overlays[GMapWrapper.NAMES].IsVisibile = false;
            _gmap.Overlays[GMapWrapper.WAYPOINTS].IsVisibile = false;

            (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPCIRCLE] as ToolStripMenuItem).Checked = false;
            (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPDT] as ToolStripMenuItem).Checked = false;
            (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPCODE] as ToolStripMenuItem).Checked = false;
            (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPNAME] as ToolStripMenuItem).Checked = false;
            (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPWPTS] as ToolStripMenuItem).Checked = false;
        }

        private void SetDisplayCirclesOnMap(object sender, EventArgs e)
        {
            if ((tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPCIRCLE] as ToolStripMenuItem).Checked)
            {
                (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPCIRCLE] as ToolStripMenuItem).Checked = false;
                _gmap.Overlays[GMapWrapper.CIRCLES].IsVisibile = false;
                // On force à le cacher, quel que soit le zoom level !
                (_gmap.Overlays[GMapWrapper.CIRCLES] as GMapOverlayCustom).ForceHide = true;
            }
            else
            {
                (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPCIRCLE] as ToolStripMenuItem).Checked = true;
                _gmap.Overlays[GMapWrapper.CIRCLES].IsVisibile = true;
                (_gmap.Overlays[GMapWrapper.CIRCLES] as GMapOverlayCustom).ForceHide = false;
                // Si les markers sont présents, bingo, sinon on recrée tout ça :-(
                if (_gmap.Overlays[GMapWrapper.CIRCLES].Markers.Count == 0)
                {
                    // On recrée, et zut
                    GMapWrapper.HandleOverlaysVisibility(_gmap); 
                    _daddy.CreateOverlayCircles(_gmap);
                }
            }
        }

        private void SetDisplayDTOnMap(object sender, EventArgs e)
        {
            if ((tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPDT] as ToolStripMenuItem).Checked)
            {
                (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPDT] as ToolStripMenuItem).Checked = false;
                _gmap.Overlays[GMapWrapper.DT].IsVisibile = false;
                (_gmap.Overlays[GMapWrapper.DT] as GMapOverlayCustom).ForceHide = true;
            }
            else
            {
                (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPDT] as ToolStripMenuItem).Checked = true;
                _gmap.Overlays[GMapWrapper.DT].IsVisibile = true;
                (_gmap.Overlays[GMapWrapper.DT] as GMapOverlayCustom).ForceHide = false;
                // Si les markers sont présents, bingo, sinon on recrée tout ça :-(
                if (_gmap.Overlays[GMapWrapper.DT].Markers.Count == 0)
                {
                    // On recrée, et zut
                    GMapWrapper.HandleOverlaysVisibility(_gmap); 
                    _daddy.CreateOverlayDT(_gmap);
                    
                }
            }
        }

        private void SetDisplayCodeOnMap(object sender, EventArgs e)
        {
            bool bHide = (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPCODE] as ToolStripMenuItem).Checked;
            DisplayCodeOnMap(bHide);
            if ((!bHide) && ((tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPNAME] as ToolStripMenuItem).Checked))
                DisplayNameOnMap(true);
        }

        private void DisplayCodeOnMap(bool bHide)
        {
            if (bHide)
            {
                (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPCODE] as ToolStripMenuItem).Checked = false;
                _gmap.Overlays[GMapWrapper.CODES].IsVisibile = false;
                (_gmap.Overlays[GMapWrapper.CODES] as GMapOverlayCustom).ForceHide = true;
            }
            else
            {
                (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPCODE] as ToolStripMenuItem).Checked = true;
                _gmap.Overlays[GMapWrapper.CODES].IsVisibile = true;
                (_gmap.Overlays[GMapWrapper.CODES] as GMapOverlayCustom).ForceHide = false;
                // Si les markers sont présents, bingo, sinon on recrée tout ça :-(
                if (_gmap.Overlays[GMapWrapper.CODES].Markers.Count == 0)
                {
                    // On recrée, et zut
                    GMapWrapper.HandleOverlaysVisibility(_gmap);
                    _daddy.CreateOverlayCode(_gmap, _bBigLabels);
                    
                }
            }
        }

        private void SetDisplayNameOnMap(object sender, EventArgs e)
        {
            bool bHide = (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPNAME] as ToolStripMenuItem).Checked;
            DisplayNameOnMap(bHide);
            if ((!bHide) && ((tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPCODE] as ToolStripMenuItem).Checked))
                DisplayCodeOnMap(true);
        }

        private void DisplayNameOnMap(bool bHide)
        {
            if (bHide)
            {
                (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPNAME] as ToolStripMenuItem).Checked = false;
                _gmap.Overlays[GMapWrapper.NAMES].IsVisibile = false;
                (_gmap.Overlays[GMapWrapper.NAMES] as GMapOverlayCustom).ForceHide = true;
            }
            else
            {
                (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPNAME] as ToolStripMenuItem).Checked = true;
                _gmap.Overlays[GMapWrapper.NAMES].IsVisibile = true;
                (_gmap.Overlays[GMapWrapper.NAMES] as GMapOverlayCustom).ForceHide = false;
                // Si les markers sont présents, bingo, sinon on recrée tout ça :-(
                if (_gmap.Overlays[GMapWrapper.NAMES].Markers.Count == 0)
                {
                    // On recrée, et zut
                    GMapWrapper.HandleOverlaysVisibility(_gmap);
                    _daddy.CreateOverlayName(_gmap, _bBigLabels);
                    
                }
            }
        }

        private void SetNightMode(object sender, EventArgs e)
        {
        	_gmap.NegativeMode = !(_gmap.NegativeMode);
        	_daddy._cachesPreviewMap.NegativeMode = _gmap.NegativeMode;
        	(tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPNIGHT] as ToolStripMenuItem).Checked = _gmap.NegativeMode;
        }
        
        
        private void SetDisplaySCaleOnMap(object sender, EventArgs e)
        {
            if ((tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPSCALE] as ToolStripMenuItem).Checked == true)
            {
                (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPSCALE] as ToolStripMenuItem).Checked = false;
                _daddy.UpdateConfFile("displayscaleonmap", "False");
                _gmap.boolUseCustomScale = false;
            }
            else
            {
                (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPSCALE] as ToolStripMenuItem).Checked = true;
                _daddy.UpdateConfFile("displayscaleonmap", "True");
                _gmap.boolUseCustomScale = true;
            }
            _gmap.Refresh();
        }


        private void SetDisplayBiggerLabelsOnMap(object sender, EventArgs e)
        {
            if ((tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPBIG] as ToolStripMenuItem).Checked)
            {
                _bBigLabels = false;
                (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPBIG] as ToolStripMenuItem).Checked = false;
            }
            else
            {
                _bBigLabels = true;
                (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPBIG] as ToolStripMenuItem).Checked = true;
            }

            // Doit on recréer les labels ?
            if ((_gmap.Overlays[GMapWrapper.CODES].Markers.Count != 0) || (_gmap.Overlays[GMapWrapper.NAMES].Markers.Count != 0))
            {
                GMapWrapper.HandleOverlaysVisibility(_gmap);
                if (_gmap.Overlays[GMapWrapper.CODES].Markers.Count != 0) // Il existaient déjà, on doit les recréer
                {
                    // On recrée, et zut
                    _daddy.CreateOverlayCode(_gmap, _bBigLabels);
                    
                }
                if (_gmap.Overlays[GMapWrapper.NAMES].Markers.Count != 0) // Il existaient déjà, on doit les recréer
                {
                    // On recrée, et zut
                    _daddy.CreateOverlayName(_gmap, _bBigLabels);
                   
                }
            }
        }

        private void SetDisplayStatsOnMap(object sender, EventArgs e)
        {
            if ((tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPSTATS] as ToolStripMenuItem).Checked)
            {
                (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPSTATS] as ToolStripMenuItem).Checked = false;
                _gmap.Overlays[GMapWrapper.STATS].IsVisibile = false;
                (_gmap.Overlays[GMapWrapper.STATS] as GMapOverlayCustom).ForceHide = true;
            }
            else
            {
                (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPSTATS] as ToolStripMenuItem).Checked = true;
                _gmap.Overlays[GMapWrapper.STATS].IsVisibile = true;
                (_gmap.Overlays[GMapWrapper.STATS] as GMapOverlayCustom).ForceHide = false;
                // Si les markers sont présents, bingo, sinon on recrée tout ça :-(
                if (_gmap.Overlays[GMapWrapper.STATS].Markers.Count == 0)
                {
                    // On recrée, et zut
                    GMapWrapper.HandleOverlaysVisibility(_gmap); 
                    _daddy.CreateOverlayStats(_gmap);
                }
            }
        }

        private void SetDisplayWayptsOnMap(object sender, EventArgs e)
        {
            if ((tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPWPTS] as ToolStripMenuItem).Checked)
            {
                (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPWPTS] as ToolStripMenuItem).Checked = false;
                _gmap.Overlays[GMapWrapper.WAYPOINTS].IsVisibile = false;
                (_gmap.Overlays[GMapWrapper.WAYPOINTS] as GMapOverlayCustom).ForceHide = true;
            }
            else
            {
                (tabControlCD._mnuContextMenu.Items[CacheDetail.MENUMAPWPTS] as ToolStripMenuItem).Checked = true;
                _gmap.Overlays[GMapWrapper.WAYPOINTS].IsVisibile = true;
                (_gmap.Overlays[GMapWrapper.WAYPOINTS] as GMapOverlayCustom).ForceHide = false;
                // Si les markers sont présents, bingo, sinon on recrée tout ça :-(
                if (_gmap.Overlays[GMapWrapper.WAYPOINTS].Markers.Count == 0)
                {
                    // On recrée, et zut
                    GMapWrapper.HandleOverlaysVisibility(_gmap); 
                    _daddy.CreateOverlayWpts(_gmap);
                }
            }
        }

        private void SetCloseTab(object sender, EventArgs e)
        {
            CloseSelectedTab();
        }

        private void SetCloseAllTabs(object sender, EventArgs e)
        {
            if (GetCacheMap() != null)
            {
                for (int i = (tabControlCD.TabCount - 1); i >= 0; i--)
                {
                    tabControlCD.CloseTab(i);
                }
            }
            else
            {
                tabControlCD.TabPages.Clear();
                this.Hide();
            }
        }

        private bool IsATabCache(TabPage page)
        {
            if (page.Tag != null)
            {
                if (page.Tag is Geocache)
                    return true;
            }
            return false;
        }

        private void SetGoToWeb(object sender, EventArgs e)
        {
            TabPage page = tabControlCD.SelectedTab;
            if (IsATabCache(page))
            {
                Geocache cache = (Geocache)(page.Tag);
                if (ConfigurationManager.AppSettings["openGeocachingEmbedded"] == "True") 
                    LoadPage("(WEB) " + cache._Name, cache._Url);
                else
                    MyTools.StartInNewThread(cache._Url);
            }
        }

        private void detail_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url != null)
            {
                String url = e.Url.ToString();
                String mgm_user = "about:MGM_USER:";

                if (url.StartsWith(mgm_user))
                {
                    e.Cancel = true;
                    try
                    {
                        String user = url.Substring(mgm_user.Length);
                        _daddy.GetUserInformation(user);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
       }

        private void CreateTabPage(out TabPage page, out WebBrowser detail, bool useBrowserWrapper = false)
        {
            page = new System.Windows.Forms.TabPage();
            if (useBrowserWrapper)
            	detail = new BrowserWrapper(_daddy);
            else
            	detail = new System.Windows.Forms.WebBrowser();
            detail.ScriptErrorsSuppressed = true;
            detail.Navigating += new WebBrowserNavigatingEventHandler(detail_Navigating);

            tabControlCD.Controls.Add(page);
            // 
            // tabPageDetail
            // 
            page.SuspendLayout();
            page.Controls.Add(detail);
            page.Location = new System.Drawing.Point(4, 22);
            page.Name = "tabPageDetail_" + iIndex.ToString();
            page.Padding = new System.Windows.Forms.Padding(3);
            page.Size = new System.Drawing.Size(818, 491);
            page.TabIndex = 0;
            page.Text = "Detail_" + iIndex.ToString();
            page.UseVisualStyleBackColor = true;
            page.ToolTipText = _daddy.GetTooltipsTranslator().GetStringMV("tabPageDetail");
            // 
            // webBrowserDetail
            // 
            detail.Dock = System.Windows.Forms.DockStyle.Fill;
            detail.Location = new System.Drawing.Point(3, 3);
            detail.MinimumSize = new System.Drawing.Size(20, 20);
            detail.Name = "webBrowserDetail_" + iIndex.ToString();
            detail.Size = new System.Drawing.Size(812, 485);
            detail.TabIndex = 0;

            page.ResumeLayout(false);
            iIndex++;
        }

        private void CreateTabPageGmaps(out TabPage page)
        {
            // Tout ce qu'il faut pour créer un objet carto
            page = new System.Windows.Forms.TabPage();
            page.SuspendLayout();

            // 
            // tabPageGmaps
            // 
            page.Location = new System.Drawing.Point(4, 22);
            page.Name = "tabPageGmaps_" + iIndex.ToString();
            page.Padding = new System.Windows.Forms.Padding(3);
            page.Size = new System.Drawing.Size(818, 491);
            page.TabIndex = 0;
            page.Text = "[" + _daddy.GetTranslator().GetString("TPMap") + "]";
            page.UseVisualStyleBackColor = true;
            page.ToolTipText = _daddy.GetTooltipsTranslator().GetStringMV("tabPageGmaps");

            // On rajoute les boutons !
            // Itinéraire on/off, efface itinéraire

            this.panelItinerary = new System.Windows.Forms.Panel();
            this.btn4 = new System.Windows.Forms.Button();
            this.btn3 = new System.Windows.Forms.Button();
            this.btn2 = new System.Windows.Forms.Button();
            this.btn1 = new System.Windows.Forms.Button();
            this.panelArea = new System.Windows.Forms.Panel();
            this.lbl1 = new System.Windows.Forms.Label();
            this.btn9 = new System.Windows.Forms.Button();
            this.btn8 = new System.Windows.Forms.Button();
            this.btn7 = new System.Windows.Forms.Button();
            this.btn6 = new System.Windows.Forms.Button();
            this.btn5 = new System.Windows.Forms.Button();
            this.btn10 = new System.Windows.Forms.Button();
            this.btnCartoConfigure = new System.Windows.Forms.Button();
            this.btnCacheCacheConfigure = new System.Windows.Forms.Button();
            this.lblCoord = new System.Windows.Forms.Label();
            this.panelItinerary.SuspendLayout();
            this.panelArea.SuspendLayout();

            // 
            // panelItinerary
            // 
            this.panelItinerary.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelItinerary.Controls.Add(this.btn4);
            this.panelItinerary.Controls.Add(this.btn3);
            this.panelItinerary.Controls.Add(this.btn2);
            this.panelItinerary.Controls.Add(this.btn1);
            this.panelItinerary.Location = new System.Drawing.Point(4, 3);
            this.panelItinerary.Name = "panelItinerary";
            this.panelItinerary.Size = new System.Drawing.Size(153, 35);
            this.panelItinerary.TabIndex = 56;
            // 
            // btn4
            // 
            this.btn4.Location = new System.Drawing.Point(115, 5);
            this.btn4.Margin = new System.Windows.Forms.Padding(0);
            this.btn4.Name = "BtnPedestrianMode";
            this.btn4.Size = new System.Drawing.Size(24, 24);
            this.btn4.TabIndex = 59;
            this.btn4.UseVisualStyleBackColor = true;
            // 
            // btn3
            // 
            this.btn3.Location = new System.Drawing.Point(79, 5);
            this.btn3.Margin = new System.Windows.Forms.Padding(0);
            this.btn3.Name = "BtnHighwayMode";
            this.btn3.Size = new System.Drawing.Size(24, 24);
            this.btn3.TabIndex = 58;
            this.btn3.UseVisualStyleBackColor = true;
            // 
            // btn2
            // 
            this.btn2.Location = new System.Drawing.Point(43, 5);
            this.btn2.Margin = new System.Windows.Forms.Padding(0);
            this.btn2.Name = "BtnClearRoute";
            this.btn2.Size = new System.Drawing.Size(24, 24);
            this.btn2.TabIndex = 57;
            this.btn2.UseVisualStyleBackColor = true;
            // 
            // btn1
            // 
            this.btn1.Location = new System.Drawing.Point(10, 5);
            this.btn1.Margin = new System.Windows.Forms.Padding(0);
            this.btn1.Name = "BtnDrawRoute";
            this.btn1.Size = new System.Drawing.Size(24, 24);
            this.btn1.TabIndex = 56;
            this.btn1.UseVisualStyleBackColor = true;
            // 
            // panelArea
            // 
            this.panelArea.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelArea.Controls.Add(this.lbl1);
            this.panelArea.Controls.Add(this.btn9);
            this.panelArea.Controls.Add(this.btn8);
            this.panelArea.Controls.Add(this.btn7);
            this.panelArea.Controls.Add(this.btn6);
            this.panelArea.Controls.Add(this.btn5);
            if (HasBtn10)
            	this.panelArea.Controls.Add(this.btn10);
            this.panelArea.Location = new System.Drawing.Point(163, 3);
            this.panelArea.Name = "panelArea";
            this.panelArea.Size = new System.Drawing.Size(223 + ((HasBtn10)?37:0), 35);
            this.panelArea.TabIndex = 57;
            // 
            // lbl1
            // 
            this.lbl1.Location = new System.Drawing.Point(187 + ((HasBtn10)?37:0), 11);
            this.lbl1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.lbl1.MaximumSize = new System.Drawing.Size(0, 17);
            this.lbl1.Name = "lbl1";
            this.lbl1.Size = new System.Drawing.Size(37, 13);
            this.lbl1.TabIndex = 62;
            this.lbl1.Text = "-";
            this.lbl1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btn10
            // 
            this.btn10.Location = new System.Drawing.Point(187, 5);
            this.btn10.Margin = new System.Windows.Forms.Padding(0);
            this.btn10.Name = "BtnDrawRoad";
            this.btn10.Size = new System.Drawing.Size(24, 24);
            this.btn10.TabIndex = 61;
            this.btn10.UseVisualStyleBackColor = true;
            // 
            // btn9
            // 
            this.btn9.Location = new System.Drawing.Point(151, 5);
            this.btn9.Margin = new System.Windows.Forms.Padding(0);
            this.btn9.Name = "BtnMeasureDistance";
            this.btn9.Size = new System.Drawing.Size(24, 24);
            this.btn9.TabIndex = 60;
            this.btn9.UseVisualStyleBackColor = true;
            // 
            // btn8
            // 
            this.btn8.Location = new System.Drawing.Point(115, 5);
            this.btn8.Margin = new System.Windows.Forms.Padding(0);
            this.btn8.Name = "BtnCenterOnArea";
            this.btn8.Size = new System.Drawing.Size(24, 24);
            this.btn8.TabIndex = 59;
            this.btn8.UseVisualStyleBackColor = true;
            // 
            // btn7
            // 
            this.btn7.Location = new System.Drawing.Point(79, 5);
            this.btn7.Margin = new System.Windows.Forms.Padding(0);
            this.btn7.Name = "BtnDisplayAreaMap";
            this.btn7.Size = new System.Drawing.Size(24, 24);
            this.btn7.TabIndex = 58;
            this.btn7.UseVisualStyleBackColor = true;
            // 
            // btn6
            // 
            this.btn6.Location = new System.Drawing.Point(43, 5);
            this.btn6.Margin = new System.Windows.Forms.Padding(0);
            this.btn6.Name = "BtnClearArea";
            this.btn6.Size = new System.Drawing.Size(24, 24);
            this.btn6.TabIndex = 57;
            this.btn6.UseVisualStyleBackColor = true;
            // 
            // btn5
            // 
            this.btn5.Location = new System.Drawing.Point(10, 5);
            this.btn5.Margin = new System.Windows.Forms.Padding(0);
            this.btn5.Name = "BtnDrawArea";
            this.btn5.Size = new System.Drawing.Size(24, 24);
            this.btn5.TabIndex = 56;
            this.btn5.UseVisualStyleBackColor = true;
            // 
            // btnCartoConfigure
            // 
            this.btnCartoConfigure.Location = new System.Drawing.Point(401 + ((HasBtn10)?37:0), 9);
            this.btnCartoConfigure.Margin = new System.Windows.Forms.Padding(0);
            this.btnCartoConfigure.Name = "BtnCartoConfiguration";
            this.btnCartoConfigure.Size = new System.Drawing.Size(24, 24);
            this.btnCartoConfigure.TabIndex = 58;
            this.btnCartoConfigure.UseVisualStyleBackColor = true;
            // 
            // btnCacheCacheConfigure
            // 
            this.btnCacheCacheConfigure.Location = new System.Drawing.Point(427 + ((HasBtn10)?37:0), 9);
            this.btnCacheCacheConfigure.Margin = new System.Windows.Forms.Padding(0);
            this.btnCacheCacheConfigure.Name = "btnCacheCacheConfigure";
            this.btnCacheCacheConfigure.Size = new System.Drawing.Size(24, 24);
            this.btnCacheCacheConfigure.TabIndex = 59;
            this.btnCacheCacheConfigure.UseVisualStyleBackColor = true;
            this.btnCacheCacheConfigure.Visible = SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled();
            // 
            // lblCoord
            // 
            this.lblCoord.Location = new System.Drawing.Point(443 + 16 + ((HasBtn10)?37:0), 9);
            this.lblCoord.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.lblCoord.MaximumSize = new System.Drawing.Size(0, 17);
            this.lblCoord.Name = "lblCoord";
            this.lblCoord.Size = new System.Drawing.Size(150, 24);
            this.lblCoord.BackColor = Color.Transparent;
            this.lblCoord.TabIndex = 60;
            this.lblCoord.Text = "";
            this.lblCoord.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            
            tabControlCD.Controls.Add(page);
            page.Controls.Add(this.lblCoord);
            page.Controls.Add(this.btnCacheCacheConfigure);
            page.Controls.Add(this.btnCartoConfigure); 
            page.Controls.Add(this.panelArea);
            page.Controls.Add(this.panelItinerary);
            page.Controls.Add(_gmap); // En dernier pour voir les boutons par dessus
            this.panelItinerary.ResumeLayout(false);
            this.panelArea.ResumeLayout(false);
            page.ResumeLayout(false);

            // Les handlers
            this.btn1.Click += new System.EventHandler(this.btn1_Click);
            this.btn2.Click += new System.EventHandler(this.btn2_Click);
            this.btn3.Click += new System.EventHandler(this.btn3_Click);
            this.btn4.Click += new System.EventHandler(this.btn4_Click);
            this.btn5.Click += new System.EventHandler(this.btn5_Click);
            this.btn10.Click += new System.EventHandler(this.btn10_Click);
            this.btn6.Click += new System.EventHandler(this.btn6_Click);
            this.btn7.Click += new System.EventHandler(this.btn7_Click);
            this.btn8.Click += new System.EventHandler(this.btn8_Click);
            this.btn9.Click += new System.EventHandler(this.btn9_Click);
            this.btnCartoConfigure.Click += new System.EventHandler(this.btnCartoConfigure_Click);
            this.btnCacheCacheConfigure.Click += new System.EventHandler(this.btnCacheCacheConfigure_Click);

            // Les icones
            btn1.Image = _daddy.GetImageSized("ActivateItinerary");
            btn2.Image = _daddy.GetImageSized("ClearItinerary");
            btn3.Image = _daddy.GetImageSized("HighwayOn");
            btn4.Image = _daddy.GetImageSized("PedestrianOff");
            btn5.Image = _daddy.GetImageSized("ActivateZone");
            btn10.Image = _daddy.GetImageSized("ActivateRoad");
            btn6.Image = _daddy.GetImageSized("ClearZone");
            btn7.Image = _daddy.GetImageSized("ViewEye");
            btn8.Image = _daddy.GetImageSized("CenterView");
            btn9.Image = _daddy.GetImageSized("MeasureCarto");
            btnCartoConfigure.Image = _daddy.GetImageSized("CartoConfiguration");
            if (_daddy._cachecache._bEnableCacheCache)
                btnCacheCacheConfigure.Image = _daddy.GetImageSized("spyon");
            else
            {
                this.Text = _daddy.GetTranslator().GetString("CacheDetailTitle");
                btnCacheCacheConfigure.Image = _daddy.GetImageSized("spyoff");
            }

            TranslateMapToolbar();
            EnableOrNotRouting();
            iIndex++;
        }

        /// <summary>
        /// Triggered when zoom changed on map control
        /// </summary>
        public void gmap_OnMapZoomChanged()
        {
            GMapWrapper.OnMapZoomChanged(_gmap);
            DrawRasterImage();

            //this.Text = _gmap.Zoom.ToString();
            DisplayCenterCoord();
            
            // Le cachecache
            _daddy._cachecache.PlayCacheCache(WorkerCacheCacheData.WhoLaunched.ZOOMCHANGE);
        }

        /// <summary>
        /// Load a default blank page on the WebBrowser
        /// </summary>
        public void LoadPageTextDefault()
        {
            String title = _daddy.GetTranslator().GetString("CDDefault");
            String text = _daddy.GetTranslator().GetString("CDDefaultText");
            WebBrowser detail;
            CreateTabPage(out _pageDefault, out detail);
            detail.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser_DocumentCompletedDefault);
            _pageDefault.Tag = null;
            _pageDefault.Text = title;
            detail.DocumentText = text;
            tabControlCD.SelectedIndex = tabControlCD.TabCount - 1;
        }

        private void webBrowser_DocumentCompletedDefault(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.AbsolutePath != (sender as WebBrowser).Url.AbsolutePath)
                return;

            if (_pageDefault != null)
                tabControlCD.TabPages.Remove(_pageDefault);
            _bInitialized = true;
        }

        /// <summary>
        /// Load a page on the WebBrowser
        /// </summary>
        /// <param name="title">Page title</param>
        /// <param name="text">Text content (can be HTML)</param>
        /// <param name="bShow">True if page shall be shown</param>
        public void LoadPageText(String title, String text, bool bShow)
        {
            LoadPageTextTag(title, text, bShow, null);
        }

        private void LoadPageTextTag(String title, String text, bool bShow, object obj)
        {
            if (tabControlCD.IsSpecialPageFromTag(obj) == 1)
            {
                // On créé une page pour afficher les caches sur la carte, en utilisant le controle top moumoutte
                TabPage page;
                CreateTabPageGmaps(out page);
                page.Tag = obj;
            }
            else
            {
                // Un page toute standard.
                TabPage page;
                WebBrowser detail;
                CreateTabPage(out page, out detail, true);
                page.Tag = obj;
                page.Text = title;
                detail.DocumentText = text;
            }

            if (bShow)
            {
                this.Show();
                if (this.WindowState == FormWindowState.Minimized)
                    this.WindowState = FormWindowState.Normal;
            }
            tabControlCD.SelectedIndex = tabControlCD.TabCount - 1;
        }

        
        /// <summary>
        /// Display a cache in the WebBrowser. 
        /// </summary>
        /// <param name="geo">Geocache object</param>
        /// <param name="bUseKm">True if distance unit is Kilometer, False if Miles</param>
        /// <param name="ocd">Associated OCD object</param>
        /// <param name="bUseOfflineData">True if display cache in offline mode (using offline data)</param>
        /// <param name="bForceDisplayFromGPX">True if display cache based on GPX data and not on GC.com</param>
        public void LoadPageCache(Geocache geo, bool bUseKm, OfflineCacheData ocd, bool bUseOfflineData, bool bForceDisplayFromGPX)
        {
            // Si on a coché embedded ET QU'ON NE VEUT PAS LES DONNEES HORS LIGNE, on utilise le GPX dans le navigateur par défaut
            // Sinon on affiche GC.com, soit en interne, soit en externe
            if (!bForceDisplayFromGPX && !bUseOfflineData && (ConfigurationManager.AppSettings["openCacheEmbedded"] == "False"))
            {
            	// Page GC.com
            	if (ConfigurationManager.AppSettings["openGeocachingEmbedded"] == "True")
                    LoadPage("(GEO) " + geo._Name, geo._Url);
                else
                    MyTools.StartInNewThread(geo._Url);
            }
            else
            {
            	TabPage page;
	            WebBrowser detail;
	            CreateTabPage(out page, out detail, true);
	            page.Tag = geo;
	            page.Text = geo._Name;
	 
	            detail.DocumentText = geo.ToHTML(bUseKm, ocd, bUseOfflineData);
	
	            this.Show();
	            if (this.WindowState == FormWindowState.Minimized)
	                this.WindowState = FormWindowState.Normal;
	
	            tabControlCD.SelectedIndex = tabControlCD.TabCount - 1;
            }
        }

        /// <summary>
        /// Display a webpage in the WebBrowser
        /// Call LoadPageTag with obj = null
        /// </summary>
        /// <param name="title">Page title</param>
        /// <param name="url">Page URL</param>
        public void LoadPage(String title, String url)
        {
            LoadPageTag(title, url, null);
        }

        /// <summary>
        /// Display a webpage in the WebBrowser
        /// </summary>
        /// <param name="title">Page title</param>
        /// <param name="url">Page URL</param>
        /// <param name="obj">Object stored in Tag attribute of the page</param>
        public void LoadPageTag(String title, String url, object obj)
        {
            TabPage page;
            WebBrowser detail;
            CreateTabPage(out page, out detail);
            page.Tag = obj;
            if (obj != null)
                detail.ScriptErrorsSuppressed = true;
            page.Text = title;
            //page.ToolTipText = "Double click to close";
            detail.Navigate(url);
            this.Show();
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;

            tabControlCD.SelectedIndex = tabControlCD.TabCount - 1;
        }

        private void tabControlCD_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            CloseSelectedTab();
        }

        private void CloseSelectedTab()
        {
            int i = tabControlCD.SelectedIndex;
            tabControlCD.CloseTab(i);
        }

        private void CacheDetail_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Cancel the Closing event
            // Close cache map tab if exists
            CloseCacheMap();

            e.Cancel = true;
            this.Hide();
        }

        private TabPage GetCacheMap()
        {
            foreach (TabPage page in tabControlCD.TabPages)
            {
                // Only one tabpage with tag = true 
                // this is the one for tab cache map
                if (tabControlCD.IsSpecialPage(page) == 1)
                {
                    return page;
                }
            }
            return null;
        }

        /// <summary>
        /// Close a TabPage
        /// </summary>
        /// <param name="page">TabPage to close</param>
        public void ClosePage(TabPage page)
        {
            if (page != null)
            {
                int i = tabControlCD.TabPages.IndexOf(page);
                tabControlCD.CloseTabForce(i, true);
            }
        }

        /// <summary>
        /// Close map display
        /// </summary>
        public void CloseCacheMap()
        {
            TabPage page = GetCacheMap();
            if (page != null)
            {
                int i = tabControlCD.TabPages.IndexOf(page);
                tabControlCD.CloseTabForce(i,true);
                _daddy.CloseTabMap();
            }
        }

        /// <summary>
        /// Display map, center on coordinates
        /// </summary>
        /// <param name="lat">center latitude</param>
        /// <param name="lon">center longitude</param>
        /// <returns>tabpage of maps</returns>
        public TabPage DisplayCacheMap(double lat, double lon)
        {
            TabPage page = GetCacheMap();
            if (page != null)
            {
            }
            else
            {
                // Nothing found
                LoadPageTextTag("[Moving map]", "", true, 1);
            }
            _gmap.Position = new PointLatLng(lat, lon);
            DisplayCenterCoord();
            return page;
        }

        private void btn1_Click(object sender, EventArgs e)
        {
            // active ou pas le mode de dessin d'une route
            if (_bDoingItinerary)
            {
                _bDoingItinerary = false;
                _gmap.OverrideLeftMouseClick = false;
                btn1.Image = _daddy.GetImageSized("ActivateItinerary");
            }
            else
            {
                // Attention, on désactive le mode de création de zone !!!
                if (_bDoingArea)
                    btn5_Click(null, e);

                // Attention, on désactive le mode de création de route !!!
                if (_bDoingRoad)
                    btn10_Click(null, e);

                
                // Attention, on désactive le mode de mesure !!!
                if (_bDoingMeasure)
                    btn9_Click(sender, e);

                _bDoingItinerary = true;
                _gmap.OverrideLeftMouseClick = true;
                btn1.Image = _daddy.GetImageSized("DeactivateItinerary");
            }
        }

        private void btn2_Click(object sender, EventArgs e)
        {
            // Efface la route en cours de création
            //_bDoingItinerary = false;
            //btn1.Image = _daddy.GetImageSized("ActivateItinerary");
            EmptyRouteMarkers();
        }

        private void btn3_Click(object sender, EventArgs e)
        {
            // active ou pas le mode highway
            if (_avoidHighways)
            {
                _avoidHighways = false;
                btn3.Image = _daddy.GetImageSized("HighwayOn");
            }
            else
            {
                _avoidHighways = true;
                btn3.Image = _daddy.GetImageSized("HighwayOff");
            }
        }

        private void btn4_Click(object sender, EventArgs e)
        {
            // active ou pas le mode pieton
            if (_walkingMode)
            {
                _walkingMode = false;
                btn4.Image = _daddy.GetImageSized("PedestrianOff");
            }
            else // On active le walking mode
            {
                _walkingMode = true;
                btn4.Image = _daddy.GetImageSized("PedestrianOn");
            }
        }

        private void btn5_Click(object sender, EventArgs e)
        {
            // active ou pas le mode de dessin d'une zone
            if (_bDoingArea)
            {
                // On arrête le dessin de zone
                _bDoingArea = false;
                lbl1.Text = "-";
                _gmap.OverrideLeftMouseClick = false;
                btn5.Image = _daddy.GetImageSized("ActivateZone");

                // On tente de définir ça dans le filtre
                // Si le sender est null, on est surement appelé par un autre bouton,
                // dans ce cas là on ne veut pas définir la zone
                _daddy.DefineFilterArea(this, area_PointsClicked, (int)(_gmap.Zoom), true, (sender == null)?true: false);
            }
            else
            {
                // Attention, on désactive le mode de création de route !!!
                if (_bDoingItinerary)
                    btn1_Click(sender, e);

                // Attention, on désactive le mode de création de route !!!
                if (_bDoingRoad)
                    btn10_Click(sender, e);

                
                // Attention, on désactive le mode de mesure !!!
                if (_bDoingMeasure)
                    btn9_Click(sender, e);

                _bDoingArea = true;
                _gmap.OverrideLeftMouseClick = true;
                btn5.Image = _daddy.GetImageSized("DeactivateZone");

                // On affiche la zone forcément
                _bShowingArea = true;
                btn7_Click(sender, e);
            }
        }
        
        private void btn10_Click(object sender, EventArgs e)
        {
            // active ou pas le mode de dessin d'une route
            if (_bDoingRoad)
            {
                // On arrête le dessin de zone
                _bDoingRoad = false;
                lbl1.Text = "-";
                _gmap.OverrideLeftMouseClick = false;
                btn10.Image = _daddy.GetImageSized("ActivateRoad");

                // On tente de définir ça dans le filtre
                // Si le sender est null, on est surement appelé par un autre bouton,
                // dans ce cas là on ne veut pas définir la zone
                _daddy.DefineFilterArea(this, area_PointsClicked, (int)(_gmap.Zoom), true, (sender == null)?true: false);
            }
            else
            {
                // Attention, on désactive le mode de création de route !!!
                if (_bDoingItinerary)
                    btn1_Click(sender, e);

                // Attention, on désactive le mode de mesure !!!
                if (_bDoingMeasure)
                    btn9_Click(sender, e);

                // Attention, on désactive le mode de mesure !!!
                if (_bDoingArea)
                    btn5_Click(sender, e);

                _bDoingRoad = true;
                _gmap.OverrideLeftMouseClick = true;
                btn10.Image = _daddy.GetImageSized("DeactivateRoad");

                // On affiche la zone forcément
                _bShowingArea = true;
                btn7_Click(sender, e);
            }
        }
        
        private void btn6_Click(object sender, EventArgs e)
        {
            // Efface la zone en cours
            EmptyAreaMarkers();
        }

        private void btn7_Click(object sender, EventArgs e)
        {
            // active ou pas l'affichage de la zone
            if (_bShowingArea)
            {
                _bShowingArea = false;
                btn7.Image = _daddy.GetImageSized("ViewEye");
                _gmap.Overlays[GMapWrapper.AREA].IsVisibile = true;
            }
            else
            {
                _bShowingArea = true;
                btn7.Image = _daddy.GetImageSized("HideEye");
                _gmap.Overlays[GMapWrapper.AREA].IsVisibile = false;
            }
        }

        private void btn9_Click(object sender, EventArgs e)
        {
            if (_bDoingMeasure)
            {
                // On arrête la mesure en cours
                _bDoingMeasure = false;
                _gmap.OverrideLeftMouseClick = false;
                btn9.Image = _daddy.GetImageSized("MeasureCarto");
                EmptyMeasureMarkers();
            }
            else
            {
                // Attention, on désactive le mode de création de zone !!!
                if (_bDoingArea)
                    btn5_Click(null, e);

                // Attention, on désactive le mode de création de route !!!
                if (_bDoingRoad)
                    btn10_Click(null, e);

                // Attention, on désactive le mode de création de route !!!
                if (_bDoingItinerary)
                    btn1_Click(sender, e);

                EmptyMeasureMarkers();
                _bDoingMeasure = true;
                _gmap.OverrideLeftMouseClick = true;
                btn9.Image = _daddy.GetImageSized("MeasureCartoOff");
            }
            
        }

        private void btn8_Click(object sender, EventArgs e)
        {
        	// Un joli centrage de la zone
        	double left = double.MaxValue;
            double top = double.MinValue;
            double right = double.MinValue;
            double bottom = double.MaxValue;
            foreach (PointLatLng p in area_PointsClicked)
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
            
            if (left != double.MaxValue && right != double.MinValue && top != double.MinValue && bottom != double.MaxValue)
            {
                var ret = RectLatLng.FromLTRB(left, top, right, bottom);
                _gmap.SetZoomToFitRect(ret);
            }
        }


        private void btnCartoConfigure_Click(object sender, EventArgs e)
        {
            Point pt = this.PointToClient(Cursor.Position);
            tabControlCD.DisplayContextMenu(pt);
        }

        private void btnCacheCacheConfigure_Click(object sender, EventArgs e)
        {
            _daddy._cachecache.ToggleCacheCache();

            // Mise à jour de l'icone
            if (_daddy._cachecache._bEnableCacheCache)
            {
                btnCacheCacheConfigure.Image = _daddy.GetImageSized("spyon");
            }
            else
            {
                btnCacheCacheConfigure.Image = _daddy.GetImageSized("spyoff");
                // On rétabli le titre par défaut
                this.Text = _daddy.GetTranslator().GetString("CacheDetailTitle");
            }

        }

        /// <summary>
        /// Activate area drawing mode
        /// </summary>
        public void SelectAreaDrawingMode()
        {
            // On a active le mode de dessin de la zone
            _bDoingArea = false;
            btn5_Click(null, null);
        }

        /// <summary>
        /// Draw an area on map using provided points
        /// </summary>
        /// <param name="pts">List of points to draw</param>
        /// <param name="zoom">Zoom level of map (will change existing map zoom level)</param>
        public void DefineAreaFromPtsList(List<PointLatLng> pts, int zoom)
        {
            // On efface ce qu'il y avait avant quoi qu'il advienne
            EmptyAreaMarkers();
            _gmap.HoldInvalidation = true;

            // On en profite pour calculer le barycentre
            double cX = 0;
            double cY = 0;
            foreach (PointLatLng pt in pts)
            {
                cX += pt.Lng;
                cY += pt.Lat;
                // On renseigne la zone
                area_PointsClicked.Add(pt);
                // On crée les markers intermédiaires
                GMapMarkerImage marker = new GMapMarkerImage(_daddy.GetImageSized("AreaPoint"), pt);
                _gmap.Overlays[GMapWrapper.AREA].Markers.Add(marker);
            }
            // Et on dessine le polygone
            GMapPolygon area = new GMapPolygon(area_PointsClicked, "mazone");
            area.Tag = null;
            //area.IsHitTestVisible = true;
            _gmap.Overlays[GMapWrapper.AREA].Polygons.Add(area);

            // On calcule le barycentre
            if (pts.Count >= 2)
            {
                cX = cX / pts.Count;
                cY = cY / pts.Count;

                // On se centre sur la zone
                _gmap.Position = new PointLatLng(cY, cX);
            }

            // On applique le zoom
            _gmap.Zoom = zoom;

            _gmap.Refresh();
        }

        double SquareDistance(PointLatLng pt1, PointLatLng pt2)
        {
            return (pt1.Lat - pt2.Lat) * (pt1.Lat - pt2.Lat) + (pt1.Lng - pt2.Lng) * (pt1.Lng - pt2.Lng);
        }

        /// <summary>
        /// Terminate polygon edition (Area definition)
        /// </summary>
        /// <param name="e">Mouse event</param>
        public void FinishAnyExistingEdition(MouseEventArgs e)
        {
            // Est-on en mode édition du polygon (déplacement d'un sommet) ?
            if (_bDoingArea && (_gmap.Overlays[GMapWrapper.AREA].Polygons.Count == 1))
            {
                GMapPolygon polygon = _gmap.Overlays[GMapWrapper.AREA].Polygons[0];
                //polygon.IsHitTestVisible = true;
                Object o = polygon.Tag;
                // Si on était en édition et qu'on a recliqué, on arrête l'édition du sommet
                if ((o != null) && (o.GetType() == typeof(Int32)))
                {
                    Int32 iTag = (Int32)o;
                    if (iTag != -1)
                    {
                        // On pose le sommet
                        PointLatLng ptmove = _gmap.FromLocalToLatLng(e.X, e.Y);
                        polygon.Tag = null;
                        polygon.Points[iTag] = ptmove;
                        area_PointsClicked[iTag] = ptmove;
                        
                        // On change l'icone du marker associé
                        GMapMarkerImage marker = _gmap.Overlays[GMapWrapper.AREA].Markers[iTag] as GMapMarkerImage;
                        marker.CurrentImage = _daddy.GetImageSized("AreaPoint");
                        

                        // Cerise sur le gateau, on insère 2 nouveaux markers, avant et après le point dans la liste
                        // SI ON A MAINTENU ALT !
                        if ((Control.ModifierKeys & Keys.Alt) > 0)
                        {
                            // au milieu des droites
                            // Si on a un seul point on ne fait rien
                            // Sinon on insère un point avant et après (attention au cas des premiers et derniers
                            // A FAIRE POUR :
                            // La liste des markers
                            // Les points du polygone
                            // area_PointsClicked
                            Image img = _daddy.GetImageSized("AreaPoint");
                            if (polygon.Points.Count() == 1)
                            {
                                // On ne fait rien
                            }
                            else if (iTag == 0)
                            {
                                // C'était le premier point, on insère juste après
                                PointLatLng pt1 = ComputeMiddlePoint(polygon.Points[iTag], polygon.Points[iTag + 1]);
                                GMapMarkerImage m1 = new GMapMarkerImage(img, pt1);

                                // Puis on ajoute un point après le dernier
                                PointLatLng pt2 = ComputeMiddlePoint(polygon.Points[iTag], polygon.Points[polygon.Points.Count() - 1]);
                                GMapMarkerImage m2 = new GMapMarkerImage(img, pt2);

                                // On insère après le premier
                                _gmap.Overlays[GMapWrapper.AREA].Markers.Insert(iTag + 1, m1);
                                area_PointsClicked.Insert(iTag + 1, pt1);
                                polygon.Points.Insert(iTag + 1, pt1);

                                // On ajoute à la fin
                                _gmap.Overlays[GMapWrapper.AREA].Markers.Add(m2);
                                area_PointsClicked.Add(pt2);
                                polygon.Points.Add(pt2);
                            }
                            else if (iTag == (polygon.Points.Count() - 1))
                            {
                                // C'était le dernier point, on insère juste avant
                                PointLatLng pt1 = ComputeMiddlePoint(polygon.Points[iTag], polygon.Points[iTag - 1]);
                                GMapMarkerImage m1 = new GMapMarkerImage(img, pt1);

                                // Puis on ajoute un point après le dernier
                                PointLatLng pt2 = ComputeMiddlePoint(polygon.Points[0], polygon.Points[iTag]);
                                GMapMarkerImage m2 = new GMapMarkerImage(img, pt2);

                                // On insère avant le dernier
                                _gmap.Overlays[GMapWrapper.AREA].Markers.Insert(iTag, m1);
                                area_PointsClicked.Insert(iTag, pt1);
                                polygon.Points.Insert(iTag, pt1);

                                // On ajoute à la fin
                                _gmap.Overlays[GMapWrapper.AREA].Markers.Add(m2);
                                area_PointsClicked.Add(pt2);
                                polygon.Points.Add(pt2);
                            }
                            else
                            {
                                // on insère juste avant
                                PointLatLng pt1 = ComputeMiddlePoint(polygon.Points[iTag], polygon.Points[iTag - 1]);
                                GMapMarkerImage m1 = new GMapMarkerImage(img, pt1);

                                // on insère juste après
                                PointLatLng pt2 = ComputeMiddlePoint(polygon.Points[iTag], polygon.Points[iTag + 1]);
                                GMapMarkerImage m2 = new GMapMarkerImage(img, pt2);

                                // On insère après
                                _gmap.Overlays[GMapWrapper.AREA].Markers.Insert(iTag + 1, m2);
                                area_PointsClicked.Insert(iTag + 1, pt2);
                                polygon.Points.Insert(iTag + 1, pt2);

                                // On insère avant
                                _gmap.Overlays[GMapWrapper.AREA].Markers.Insert(iTag, m1);
                                area_PointsClicked.Insert(iTag, pt1);
                                polygon.Points.Insert(iTag, pt1);
                            }
                        }

                        // Combien de markers sont présents dans la zone ?
                        NotifyNumberOfMarkersInsideArea();

                        // Et un bon coup de refresh
                        _gmap.UpdatePolygonLocalPosition(polygon);
                        _gmap.UpdateMarkerLocalPosition(marker);
                        _gmap.Invalidate();
                    }
                }
            }
        }

        PointLatLng ComputeMiddlePoint(PointLatLng start, PointLatLng end)
        {
            PointLatLng pt = new PointLatLng();
            pt.Lat = (start.Lat + end.Lat) / 2.0;
            pt.Lng = (start.Lng + end.Lng) / 2.0;
            return pt;
        }

        private void cachedetail_OnMouseMove(object sender, MouseEventArgs e)
        {
            // Déplacement en cours via un SHIFT+Left (déplacement simple) ou ALT+Left (déplacement avec création de points lors de la pose) ? 
            if (_bDoingArea && 
                (((Control.ModifierKeys & Keys.Shift) > 0) || ((Control.ModifierKeys & Keys.Alt) > 0)))
            {
                PointLatLng ptmove = _gmap.FromLocalToLatLng(e.X, e.Y);

                // Y a t'il une zone définie ?
                if (_gmap.Overlays[GMapWrapper.AREA].Polygons.Count == 1)
                {
                    // Est ce qu'on est en train de déplacer un point d'une zone ?
                    GMapPolygon polygon = _gmap.Overlays[GMapWrapper.AREA].Polygons[0];
                    //polygon.IsHitTestVisible = true;
                    Object o = polygon.Tag;
                    if ((o != null) && (o.GetType() == typeof(Int32)))
                    {
                        Int32 iTag = (Int32)o;
                        // on est en train d'éditer un point du polygone
                        if (iTag != -1)
                        {
                            // On déplace ce point ainsi que le marker associé
                            polygon.Points[iTag] = ptmove;
                            area_PointsClicked[iTag] = ptmove;
                            _gmap.UpdatePolygonLocalPosition(polygon);

                            GMapMarkerImage marker = _gmap.Overlays[GMapWrapper.AREA].Markers[iTag] as GMapMarkerImage;
                            marker.Position = ptmove;
                            _gmap.UpdateMarkerLocalPosition(marker);

                            _gmap.Invalidate();
                        }
                    }
                }
            }
            else
            {
                // On regarde si on a laché le SHIFT alors qu'on était en train de bouger un sommet.
                // Si oui, on le pose
                FinishAnyExistingEdition(e);
            }
        }

        private void NotifyNumberOfMarkersInsideArea()
        {
            // DEBUG DEBUG
            if (_gmap.Overlays[GMapWrapper.AREA].Polygons.Count == 1)
            {
                GMapPolygon polygon = _gmap.Overlays[GMapWrapper.AREA].Polygons[0];
                //polygon.IsHitTestVisible = true;
                if (polygon.Points.Count() >= 3)
                {
                    int nbinside = 0;
                    foreach (GMapMarker marker in _gmap.Overlays[GMapWrapper.MARKERS].Markers)
                    {
                        if (polygon.IsInside(marker.Position))
                            nbinside++;
                    }
                    this.lbl1.Text = nbinside.ToString();
                }
            } 
        }

        /// <summary>
        /// Triggered when map is clicked, in particular during any edition (route, area)
        /// </summary>
        /// <param name="e">mouse event</param>
        public void cachedetail_OnMapClick(MouseEventArgs e)
        {
            // IL FAUT PRESSER CTRL + LEFT CLICK !
            // Normalement c'est déjà vérifié par GMapControlCustom, mais on ne sait jamais...
            if (_bDoingMeasure && ((Control.ModifierKeys & Keys.Control) > 0))
            {
                PointLatLng pt = _gmap.FromLocalToLatLng(e.X, e.Y);
                DefinePointOfAMeasure(pt);
            }
            else if (_bDoingItinerary && ((Control.ModifierKeys & Keys.Control) > 0))
            {
                PointLatLng pt = _gmap.FromLocalToLatLng(e.X, e.Y);
                DefinePointOfARoute(pt);
            }
            else if (_bDoingRoad && ((Control.ModifierKeys & Keys.Control) > 0))
            {
            	PointLatLng pt = _gmap.FromLocalToLatLng(e.X, e.Y);
            	// Le marker pour la zone
                GMapMarkerImage marker = new GMapMarkerImage(_daddy.GetImageSized("AreaPoint"), pt);
                _gmap.Overlays[GMapWrapper.AREA].Markers.Add(marker);
            }
            else if (_bDoingArea && ((Control.ModifierKeys & Keys.Control) > 0))
            {
                _gmap.Overlays[GMapWrapper.AREA].Polygons.Clear();
                PointLatLng pt = _gmap.FromLocalToLatLng(e.X, e.Y);
                area_PointsClicked.Add(pt);
                if (area_PointsClicked.Count() > 1)
                {
                    GMapPolygon area = new GMapPolygon(area_PointsClicked, "mazone");
                    //area.IsHitTestVisible = true;
                    area.Tag = null;
                    _gmap.Overlays[GMapWrapper.AREA].Polygons.Add(area);

                    // Le marker pour la zone
                    GMapMarkerImage marker = new GMapMarkerImage(_daddy.GetImageSized("AreaPoint"), pt);
                    _gmap.Overlays[GMapWrapper.AREA].Markers.Add(marker);

                    // Combien de markers sont présents dans la zone ?
                    NotifyNumberOfMarkersInsideArea();
                }
                else
                {
                    GMapMarkerImage marker = new GMapMarkerImage(_daddy.GetImageSized("AreaPoint"), pt);
                    _gmap.Overlays[GMapWrapper.AREA].Markers.Add(marker);
                }
            }
            else if (_bDoingArea && 
                (((Control.ModifierKeys & Keys.Shift) > 0) || ((Control.ModifierKeys & Keys.Alt) > 0)) &&
                (_gmap.Overlays[GMapWrapper.AREA].Polygons.Count() == 1))
            {
                // Est-on en mode édition du polygon (déplacement d'un sommet) ?
                GMapPolygon polygon = _gmap.Overlays[GMapWrapper.AREA].Polygons[0];
                //polygon.IsHitTestVisible = true;
                Object o = polygon.Tag;
                // Si on était en édition et qu'on a recliqué, on arrête l'édition du sommet
                if ((o != null) && (o.GetType() == typeof(Int32)))
                {
                    Int32 iTag = (Int32)o;
                    if (iTag != -1)
                    {
                        // On pose le sommet
                        FinishAnyExistingEdition(e);
                        return;
                    }
                }

                // Bon on entre en mode d'édition d'un sommet
                // Le point clické
                PointLatLng ptclick = _gmap.FromLocalToLatLng(e.X, e.Y);
                int iNbPtsArea = _gmap.Overlays[GMapWrapper.AREA].Polygons[0].Points.Count();
                double dmin = Double.MaxValue;
                int imin = -1;
                // Est-ce qu'on est en mode déplacement d'un sommet ou pas ? 
                // Si oui, Tag contient l'index du somme qu'on bouge
                // On recherche un sommet du polygon le plus proche
                for (int i = 0; i < iNbPtsArea; i++)
                {
                    double d = SquareDistance(ptclick, polygon.Points[i]);
                    // a t'on trouvé un point plus proche ?
                    if (d < dmin)
                    {
                        dmin = d;
                        imin = i;
                    }
                }

                if (imin != -1)
                {
                    // on stocke l'index du point
                    polygon.Tag = imin;

                    // On change l'icone du marker associé
                    GMapMarkerImage marker = _gmap.Overlays[GMapWrapper.AREA].Markers[imin] as GMapMarkerImage;
                    marker.CurrentImage = _daddy.GetImageSized("AreaPointGrey");
                    _gmap.UpdateMarkerLocalPosition(marker);
                    _gmap.Invalidate();
                }
            }
            else
            {
                // Comportement par défaut
                _gmap.BaseOnMouseClick(e);
            }
        }
    }
}
