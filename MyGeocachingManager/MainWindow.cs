using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Services.Client;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Globalization;
using System.Net;
using System.Timers;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System.Web;
using MyGeocachingManager.HMI;
using MyGeocachingManager.Geocaching;
using MyGeocachingManager.Geocaching.Filters;
using MyGeocachingManager.SpecialFeatures;
using SpaceEyeTools;
using SpaceEyeTools.HMI;
using SpaceEyeTools.EXControls;
using System.Drawing.Drawing2D;
using System.IO.Compression;
using System.Data.SQLite;
using System.Drawing.Imaging;

namespace MyGeocachingManager
{
    /// <summary>
    /// Main form for MGM
    /// </summary>
    public partial class MainWindow : Form
    {

        static String StaticName = "MainWindow";
        /// <summary>
        /// Assembly subversion, used for beta version only
        /// Usually is RCx with x on 1 digit
        /// Refers to release candidate
        /// If null, official released version of MGM
        /// </summary>
        public static String AssemblySubVersion = ""; // "" par défaut !

        /// <summary>
        /// Holds all various geocaching constants
        /// </summary>
        public GeocachingConstants _geocachingConstants =  null;
        
        /// <summary>
        /// Toolbar for shortcuts
        /// </summary>
        public System.Windows.Forms.ToolStrip _shortcutToolstrip = null;
        
        /// <summary>
        /// Saved authentication cookie
        /// </summary>
        private CookieContainer _cookieJar = null;
        
        /// <summary>
        /// MGMDataBase
        /// </summary>
        private MGMDataBase _dbmgm = null;
        
        /// <summary>
        /// Indicates if user is premium
        /// </summary>
        private bool UserIsPremium = false;

        String _sSpoilerDefaultKeywords = "spoiler¤spolier¤indice¤cache¤ici¤aide¤cheat¤help¤here¤auge¤hilfe¤hintweis¤hier";

        bool _bEnableWhiteBackground = false;
        bool _bMajorUpdateDetected = false;
        /// <summary>
        /// 
        /// </summary>
        public bool _bForceClose = false;

        // For XML reading
        /// <summary>
        /// 
        /// </summary>
        public static String[] GROUNDSPEAK_NAMESPACE = {
            "http://www.topografix.com/GPX/1/0",
            "http://www.topografix.com/GPX/1/1",
            "http://www.groundspeak.com/cache/1/1", // PQ 1.1
            "http://www.groundspeak.com/cache/1/0/1", // PQ 1.0.1
            "http://www.groundspeak.com/cache/1/0", // PQ 1.0
        };

        
        // New HMI for attributes
        List<AttributeImage> _listPbIn = null;
        List<AttributeImage> _listPbOut = null;

        // Construction de la liste des icones pour l'affichage optimisé de Gmaps
        // Plein taille 32x32
        // Taille moyenne 16x16
        // Taille mini 12x12
        Dictionary<String, int> _lstIndexForOptimizedMap = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase); // Modif 2.0.3_RC5
        List<Image> _lstImagesForOptimizedMap = new List<Image>();
        int _iOffsetIndexForOptimizedMap;

        /// <summary>
        /// List of offline data for caches
        /// </summary>
        public OfflineData _od = null;
          
        /// <summary>
        /// path to OfflineData file
        /// </summary>
        public String _odfile = "";

        /// <summary>
        /// List of ignore GC code
        /// </summary>
        public Dictionary<String, MiniGeocache> _ignoreList = new Dictionary<string, MiniGeocache>();

        private bool _bFilterMasked = false;

        /// <summary>
        /// Path to ignore file
        /// </summary>
        public String _ignorefile = "";

        /// <summary>
        /// true if image download is in progress
        /// </summary>
        public bool bOfflineDownloadInProgress = false;
        
        ContextMenuStrip _mnuContextMenu;
        ToolStripMenuItem _offline;
        ToolStripMenuItem _waypointsmenu;
        ToolStripMenuItem _modifymenu;
        ToolStripMenuItem _filtersmenu;
        ToolStripMenuItem _filtersmenunear;
        ToolStripMenuItem _cacheimages;
        ToolStripMenuItem _favmenu;
        /// <summary>
        /// True if filter used
        /// </summary>
        public bool _bUseFilter = false;
        
        private CacheFilter _filter = new CacheFilter();
        
        /// <summary>
        /// Custom filter to override _filter
        /// </summary>
        public CacheFilter _filterOverride = null;
        
        /// <summary>
        /// Current filter
        /// </summary>
        public CacheFilter Filter
        {
            get
            {
            	return _filter;
            }
            set 
            { 
            	_filter = value; 
            }
        }
        
        /// <summary>
        /// Current filter or override
        /// </summary>
        public CacheFilter FilterOrOverride
        {
            get
            {
            	if (_filterOverride != null)
            		return _filterOverride;
            	else
            		return _filter;
            }
        }
        
        System.IO.StreamWriter _log;
        
        /// <summary>
        /// List of cache status
        /// </summary>
        public CacheStatus _cacheStatus = null;
        
        /// <summary>
        /// List of profiles
        /// </summary>
        public ProfileMgr _profileMgr = null;
        
        /// <summary>
        /// list of currently loaded caches
        /// key: cache GC code
        /// </summary>
        public Dictionary<String, Geocache> _caches = null;
        
        /// <summary>
        /// list of currently loaded waypoints
        /// key: waypoint name
        /// </summary>
        public Dictionary<String, Waypoint> _waypointsLoaded = null;

        /// <summary>
        /// list of currently loaded waypoints, specifically created by MGM
        /// key: waypoint name
        /// </summary>
        public Dictionary<String, Waypoint> _waypointsMGM = new Dictionary<string,Waypoint>();

        Dictionary<String, int> _indexImages;
        Dictionary<String, int> _indexImagesLowerKey;
        ImageList _listImagesFoo;
        List<Image> _listImagesSized;
        Double _dHomeLat;
        Double _dHomeLon;
        String _sHomeLat;
        String _sHomeLon;

        /// <summary>
        /// Get Home Latitude
        /// </summary>
        public Double HomeLat
        {
            get
            {
                return this._dHomeLat;
            }
        }

        /// <summary>
        /// Get Home Longitude
        /// </summary>
        public Double HomeLon
        {
            get
            {
                return this._dHomeLon;
            }
        }

        /// <summary>
        /// List of currently displayed EXListViewItem objects
        /// </summary>
        public List<EXListViewItem> _listViewCaches;
        
        String _errorMessageLoad = "";
               
        /// <summary>
        /// Reference to CacheDetail instanced object
        /// </summary>
        public CacheDetail _cacheDetail;
        
        // CacheCache related stuff
        /// <summary>
        /// Cachecache DB
        /// </summary>
        public CacheCache _cachecache = null;
       
        Dictionary<String, Image> _imgAttributes = null;
        Dictionary<String, Image> _imgAttributesGreyed = null;
        Dictionary<String, Image> _imgTags = null;
        /// <summary>
        /// /
        /// </summary>
        public Dictionary<String, Image> _imgMenus = null;
        
        /// <summary>
        /// List of available tags for cache tagging
        /// </summary>
        public List<String> _listExistingTags = null;
        
        Dictionary<String, Image> _dicoDT = null;

        
        /// <summary>
        /// True if distance will be displayed in Kilometers
        /// False if distance will be displayed in Miles
        /// </summary>
        public bool _bUseKm = true;
        
        /// <summary>
        /// True: cache popularity uses GC.com formula (siple one)
        /// False: Popularity of geocache, based on Project-GC formula (Lower bound of Wilson score confidence interval for a Bernoulli parameter)
        /// </summary>
        public bool _bUseGCPopularity = true;
        
        /// <summary>
        /// Constant to convert kilometers to miles (0.621371192)
        /// </summary>
        public double _dConvKmToMi = 0.621371192;
        
        Dictionary<String, String> _tableWptsTypeTranslated = new Dictionary<string, string>();
        Dictionary<String, String> _tableAttributes = new Dictionary<string, string>();
        Dictionary<String, String> _tableAttributesCompleted = new Dictionary<string, string>();
        Dictionary<String, int> _tableAttributesCategory = new Dictionary<string, int>();
        private String sDownloadedUpdateFile = "";
        String _zipFileCurrentlyLoaded = "";
        
        // Icons for notes & spoilers
        Image _imgNothing;
        Image _imgNote;
        Image _imgNoteSpoiler;
        Image _imgNoteNoSpoiler;
        Image _imgSpoiler;
        Image _imgNoSpoiler;
        
        /// <summary>
        /// Min cacheid for MGM generated caches
        /// </summary>
        public int _iMinCacheIdMGM = 666000000;
        /// <summary>
        /// Used for all generated caches
        /// </summary>
		public int _iCacheId = 0;
		
        Tsp.Tsp tsp;
        Tsp.Cities cityList;
        ThreadProgress tspprogress;
        CultureInfo defc1 = Thread.CurrentThread.CurrentCulture;
        CultureInfo defc2 = Thread.CurrentThread.CurrentUICulture;

        TrackSelector _trackselector = null;

        Dictionary<String, List<String>> _dicoCountryState = new Dictionary<string,List<string>>();
        Dictionary<int, String> _dicoColumns = new Dictionary<int, string>();

        Thread tsplash = null;
        private Func<String, bool, bool> myUpdateThreadInfo = null;
        
        Splashscreen _newSplashForm = new Splashscreen();

        ToolStripMenuItem _pluginToolStripMenuItem = null;

        // Plugins
        List<IScript> _iListScript = new List<IScript>();
        
        /// <summary>
        /// List of loaded and compiled plugins (V2 format)
        /// </summary>
        public List<IScriptV2> _iListScriptV2 = new List<IScriptV2>();

        // Generic progress bar
        /// <summary>
        /// Title of currently displayed progress bar
        /// </summary>
        public String _ThreadProgressBarTitle = "";
        
        /// <summary>
        /// Reference to current progress bar
        /// </summary>
        public ThreadProgress _ThreadProgressBar = null;
        
        Thread _ThreadRunningProgressBar = null;

        // Internet access ?
        bool _bInternetAvailable = false;

        /// <summary>
        /// Return internet status
        /// Shall be previously updated with checkinternetaccess if needed
        /// </summary>
        /// <returns>true if internet available</returns>
        public bool GetInternetStatus()
        {
        	return _bInternetAvailable;
        }
        
        /// <summary>
        /// Number of modified caches still unsaved in MGM
        /// </summary>
        public int _iNbModifiedCaches = 0;

        // List of currently loaded files
        private List<String> _LoadedFiles = new List<string>();
        private List<String> _LoadedOriginalFiles = new List<string>();

        /// <summary>
        /// True if platform if Linux
        /// False if Windows (or Mac OS ???)
        /// </summary>
        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        // Les langages et les tooltips
        ToolTip _toolTipForMGM = new ToolTip();
        TranslationManager _TranslationManager;
        TranslationManager _TooltipsTranslationManager;
        Dictionary<String, LanguageItem> _languages = new Dictionary<string, LanguageItem>();
        Dictionary<String, LanguageItem> _tooltips = new Dictionary<string, LanguageItem>();
        
        // Si true calcul stupide de popularite
        // Si false, on tente de récupérer la valeur
        bool _bPopulariteBasique = false;

        // Number of currently item selection
        int _iNbCachesSelectedInListview = 0;
        // couple (item, besoin d'internet)
        List<Tuple<ToolStripMenuItem, bool>> _menuEntriesRequiringOnlyOneSelectedCaches = new List<Tuple<ToolStripMenuItem, bool>>();
        // couple (item, besoin d'internet)
        List<Tuple<ToolStripMenuItem, bool>> _menuEntriesRequiringSelectedCaches = new List<Tuple<ToolStripMenuItem, bool>>();
        // Number of currently item displayed
        int _iNbCachesDisplayedInListview = 0;
        // couple (item, besoin d'internet)
        List<Tuple<ToolStripMenuItem, bool>> _menuEntriesRequiringDisplayedCaches = new List<Tuple<ToolStripMenuItem, bool>>();
        // menu ayant besoin d'internet
        ToolStripMenuItem[] _menuEntriesRequiringInternet = null;
        // menus spéciaux
        /// <summary>
        /// 
        /// </summary>
        public ToolStripItem[] _menuEntriesRequiringSpecialFeatures = null;
        
        // .Net highest version detected
        String _dotnet = "";
        
        /// <summary>
        /// returns a reference to a tooltips translator
        /// </summary>
        /// <returns>reference to a tooltips translator</returns>
        public TranslationManager GetTooltipsTranslator()
        {
            return _TooltipsTranslationManager;
        }

        /// <summary>
        /// returns a reference to a language translator
        /// </summary>
        /// <returns>reference to a language translator</returns>
        public TranslationManager GetTranslator()
        {
            return _TranslationManager;
        }

        /// <summary>
        /// returns a reference to a language translator
        /// </summary>
        /// <returns>reference to a language translator</returns>
        public static TranslationManager GetTranslatorStatic()
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f.Name == MainWindow.StaticName)
                {
                    MainWindow mw = f as MainWindow;
                    return mw.GetTranslator();
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public EXListView LvGeocaches
        {
            get { return lvGeocaches; }
        }
        
        private void SplashForm()
        {
            _newSplashForm.ShowDialog();
            _newSplashForm.Dispose();
        }

        /// <summary>
        /// Creates a progress bar for long processing
        /// </summary>
        /// <param name="delay"></param>
        public void CreateThreadProgressBar(int delay = 5000)
        {
            try
            {
                if (ConfigurationManager.AppSettings["disablethreadprogressbar"] == "True")
                return;

                if (_ThreadProgressBar == null)
                {
                	_ThreadRunningProgressBar = new Thread(() => ShowThreadProgressBar(delay));
                    _ThreadRunningProgressBar.Start();
                }
                // Else we do nothing, this thing is supposed to be running
            }
            catch (Exception)
            {
                _ThreadRunningProgressBar = null;
            }
        }

        /// <summary>
        /// Creates an enhanced progress bar for long processing
        /// </summary>
        public void CreateThreadProgressBarEnh()
        {
            try
            {
                if (_ThreadProgressBar == null)
                {
                    _ThreadRunningProgressBar = new Thread(new ThreadStart(ShowThreadProgressBarEnh));
                    _ThreadRunningProgressBar.Start();
                }
                // Else we do nothing, this thing is supposed to be running
            }
            catch (Exception)
            {
                _ThreadRunningProgressBar = null;
            }
        }

        private void ShowThreadProgressBar(int delay)
        {
            if (ConfigurationManager.AppSettings["disablethreadprogressbar"] == "True")
                return;

            try
            {

                if (_ThreadProgressBar == null)
                {
                    // Create it
                    _ThreadProgressBar = new ThreadProgress();
                    _ThreadProgressBar.Font = this.Font;
                    _ThreadProgressBar.Icon = this.Icon;
                    if (_ThreadProgressBarTitle == "")
                        _ThreadProgressBar.Text = GetTranslator().GetString("LblOperationInProgress");
                    else
                        _ThreadProgressBar.Text = _ThreadProgressBarTitle;
                    _ThreadProgressBar.btnAbort.Text = GetTranslator().GetString("BtnAbort");
                    _ThreadProgressBar.btnAbort.Enabled = false;
                    _ThreadProgressBar.progressBar1.Visible = false;
                    _ThreadProgressBar.label1.Visible = false;
                    _ThreadProgressBar.pictureBox1.Visible = true;
                    _ThreadProgressBar.lblWait.Text = GetTranslator().GetString("LblWaitingNoTime");
                }
                // small delay on display
                Thread.Sleep(delay);

                try
                {
                    _ThreadProgressBar.ShowDialog();
                    _ThreadProgressBar.Dispose();
                }
                catch (Exception)
                {
                }
                _ThreadProgressBar = null;
            }
            catch(Exception)
            {
                _ThreadProgressBar = null;
            }
        }

        private void ShowThreadProgressBarEnh()
        {
            try
            {
                if (_ThreadProgressBar == null)
                {
                    // Create it
                    _ThreadProgressBar = new ThreadProgress();
                    _ThreadProgressBar.Font = this.Font;
                    _ThreadProgressBar.Icon = this.Icon;
                    if (_ThreadProgressBarTitle == "")
                        _ThreadProgressBar.Text = GetTranslator().GetString("LblOperationInProgress");
                    else
                        _ThreadProgressBar.Text = _ThreadProgressBarTitle;
                    _ThreadProgressBar.btnAbort.Text = GetTranslator().GetString("BtnAbort");
                    _ThreadProgressBar.btnAbort.Enabled = true;
                    _ThreadProgressBar.progressBar1.Visible = true;
                    _ThreadProgressBar.label1.Visible = true;
                    _ThreadProgressBar.pictureBox1.Visible = false;
                    _ThreadProgressBar.lblWait.Text = GetTranslator().GetString("LblWaitingNoTime");
                }

                try
                {
                    _ThreadProgressBar.ShowDialog();
                    _ThreadProgressBar.Dispose();
                }
                catch (Exception)
                {
                }
                _ThreadProgressBar = null;
            }
            catch (Exception)
            {
                _ThreadProgressBar = null;
            }
        }

        /// <summary>
        /// Kill current progress bar
        /// </summary>
        public void KillThreadProgressBar()
        {
            if (ConfigurationManager.AppSettings["disablethreadprogressbar"] == "True")
                return;

            KillThreadProgressBarEnh();
        }

        /// <summary>
        /// Kill current enhanced progress bar
        /// </summary>
        public void KillThreadProgressBarEnh()
        {
            try
            {
                if (_ThreadProgressBar != null)
                    _ThreadProgressBar.Hide();

                if (_ThreadRunningProgressBar != null)
                    _ThreadRunningProgressBar.Abort();
            }
            catch (Exception)
            {
            }
            _ThreadRunningProgressBar = null;
            _ThreadProgressBar = null;
        }


        private void LogToSplash(String info, bool dostep = true)
        {
            Log(info);
            if ((_newSplashForm != null)&&(myUpdateThreadInfo != null))
            {
            	myUpdateThreadInfo(info, dostep);
            }
        }

        /// <summary>
        /// Simple methode to disply an object (uses MyMessageBox)
        /// </summary>
        /// <param name="o">object to display</param>
        public void MSG(object o)
        {
            MyMessageBox.Show(o.ToString(), "Information", MessageBoxIcon.Information, GetTranslator());
        }

        private void PopulateListOfToolstripItemsWithInternet(List<Tuple<ToolStripMenuItem, bool>> lst, ToolStripMenuItem item, bool needinternet)
        {
        	lst.Add(new Tuple<ToolStripMenuItem, bool> (item, needinternet));
        }
        
        /// <summary>
        /// /
        /// </summary>
        public enum MenuItemConstraintType
        {
        	/// <summary>
        	/// 
        	/// </summary>
            REQUIRE_INTERNET,
            /// <summary>
            /// 
            /// </summary>
            REQUIRE_DISPLAYED_CACHES,
            /// <summary>
            /// 
            /// </summary>
            REQUIRE_SELECTED_CACHES,
            /// <summary>
            /// 
            /// </summary>
            REQUIRE_SELECTED_ONE_CACHE
        };
        
        /// <summary>
        /// Constructor, performs all intialisations
        /// </summary>
        /// <param name="dotnet">highest installed .net version</param>
        public MainWindow(String dotnet)
        {
        	_iCacheId = _iMinCacheIdMGM;
        	_dotnet = dotnet;
            ThreadProgress.CheckForIllegalCrossThreadCalls = false;
            if (MainWindow.AssemblySubVersion != "")
            {
            	_newSplashForm.AssemblySubVersion = MainWindow.AssemblySubVersion;
            	_newSplashForm.ExtraInfo = "Build " + GetBuildTime() + " ";
            }
           	if (_dotnet != "")
            	_newSplashForm.ExtraInfo += ".Net " + _dotnet;
            
            myUpdateThreadInfo = _newSplashForm.UpdateInfo;
            // ***************************************************************
            // DEFINE HERE THE MAXIMUM NUMBER OF STEPS FOR THE SPLASH PROGRESS
            _newSplashForm.SetMaximumSteps(17);
            // ***************************************************************

            // Start splash screen
            CheckConfFileEntry("debugbtn", "False");
            if (ConfigurationManager.AppSettings["debugbtn"] != "True")
            {
                tsplash = new Thread(new ThreadStart(SplashForm));
                tsplash.Start();
            }

            InitializeComponent();
            
            // entries that require DISPLAYED caches to be activated
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringDisplayedCaches, dispCachesToolStripMenuItem, false);
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringDisplayedCaches, displayDTMatrixToolStripMenuItem, false);
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringDisplayedCaches, createMyFindsToolStripMenuItem, true);
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringDisplayedCaches, displayDirectPathToolStripMenuItem, false);
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringDisplayedCaches, displayAPathConnectingAllDisplayedCachesToolStripMenuItem, false);
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringDisplayedCaches, identifyClustersToolStripMenuItem, false);
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringDisplayedCaches, displayFranceCoverageToolStripMenuItem, false);
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringDisplayedCaches, generateFranceCoverageToolStripMenuItem, false);
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringDisplayedCaches, displayWorldCoverageToolStripMenuItem, false);
            
          
            
            // entries that require SELECTED caches to be activated
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringSelectedCaches, selCachesToolStripMenuItem, false);
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringSelectedCaches, translateSelectedCachesToolStripMenuItem, true);
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringSelectedCaches, excludeSelectedCachesToolStripMenuItem, false);
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringSelectedCaches, logselcachesToolStripMenuItem, false);
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringSelectedCaches, updateStatsToolStripMenuItem, true);
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringSelectedCaches, DownloadNotesFromGCToolStripMenuItem, true);
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringSelectedCaches, UploadNotesToGCToolStripMenuItem, true);
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringSelectedCaches, getUpdatedCoordToolStripMenuItem, true);
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringSelectedCaches, setUpdatedCoordToolStripMenuItem, true);
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringSelectedCaches, completeCacheDescToolStripMenuItem, true);
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringSelectedCaches, updateCachesItemsToolStripMenuItem, true);
    		PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringSelectedCaches, computeAltiToolStripMenuItem, true);
    		PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringSelectedCaches, displayCachesWithGPSSpoilersToolStripMenuItem, false);
    		PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringSelectedCaches, downloadSpoilersAuthenticatedToolStripMenuItem, true);
    		PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringSelectedCaches, SelectedCachesToolStripMenuItem, false);
    		
    		// entries that require ONLY ONE SELECTED caches to be activated
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringOnlyOneSelectedCaches, WaypointsToolStripMenuItem, false);
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringOnlyOneSelectedCaches, FilterNearToolStripMenuItem, false);
            PopulateListOfToolstripItemsWithInternet(_menuEntriesRequiringOnlyOneSelectedCaches, OCWriteNoteToolStripMenuItem, false);
            
            // entries that only require internet access
            _menuEntriesRequiringInternet = new ToolStripMenuItem[] {
            	configGCaccountinfoToolStripMenuItem,
            	donateToolStripMenuItem,
            	downloadbetaToolStripMenuItem,
            	downloadPluginsToolStripMenuItem,
            	forceUpdateToolStripMenuItem,
            	usersInformationToolStripMenuItem,
            	getFoundinfoToolStripMenuItem,
            	displayOurDNFToolStripMenuItem,
            	menupublishnotifications,
            	PQToolStripMenuItem,
            	checkUpdatesToolStripMenuItem,
            	SCDisplayOnGCToolStripMenuItem,
            	OCDownloadToolStripMenuItem,
            	geoFranceToolStripMenuItem
            };
            	
            // Special features for Gold MGM users ;-)
            _menuEntriesRequiringSpecialFeatures = new ToolStripItem[] {
            	//AnimateFindsToolStripMenuItem,
            	kmlMaintenanceToolStripMenuItem,
            	filterOnCacheCacheToolStripMenuItem,
            	completeFromCacheCacheFullToolStripMenuItem,
            	//filterOnCountryToolStripMenuItem,
            	//filterOnFrenchRegionToolStripMenuItem,
            	//filterOnFrenchDepartmentToolStripMenuItem,
            	//filterOnFrenchCityToolStripMenuItem,
            	//toolStripSeparator17,
            	completeCacheDescToolStripMenuItem,
				menupublishnotifications,
            	displayCachesWithGPSSpoilersToolStripMenuItem
            	//displayFranceCoverageToolStripMenuItem,
            	//displayWorldCoverageToolStripMenuItem,
            	//generateFranceCoverageToolStripMenuItem
            };
            
        	// On se laisse le temps de l'init
            this.Hide();
            
            try
            {
                // Just check the logfile entryr is present ;-)
                LogToSplash("Opening log framework");
                CheckConfFileEntry("logfile", "log.txt");
                // Ouverture des logs !
                OpenLog();
                // Quelques infos pour le debug
                if (_log != null)
                {
	                Log("Version " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
	                Log("Subversion " + AssemblySubVersion);
	                Log("Build " + GetBuildTime());
	                Log("Highest installed framework " + _dotnet);
	                Log(MyTools.GetOSAssembliesFrameworkInfo());
                }
                
                // Des vérifications d'intégrité
                Log("CheckConfigurationIntegrity");
                LogToSplash("Checking configuration integrity");
                bool restart = CheckConfigurationIntegrity();
            
                LogToSplash("Chosing language");
                LookForLanguages();

                // Force locale ?
                String forceLocale = ConfigurationManager.AppSettings["forceLocale"];
                if (forceLocale != "")
                {
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(forceLocale);
                }

                HandleDefaultLanguageForLocales();

                // Create the ToolTip and associate with the Form container.
                _toolTipForMGM = new ToolTip();
                // Set up the delays for the ToolTip.
                _toolTipForMGM.AutoPopDelay = 5000;
                _toolTipForMGM.InitialDelay = 1000;
                _toolTipForMGM.ReshowDelay = 500;
                // Force the ToolTip text to be displayed whether or not the form is active.
                _toolTipForMGM.ShowAlways = true;

                _TranslationManager = GetTranslatorForCurrentLocale();
                _TooltipsTranslationManager = GetTooltipsTranslatorForCurrentLocale();

                
				// Geocaching constants
				_geocachingConstants = new GeocachingConstants(this);
				
                if (restart)
                {
                    MsgActionWarning(this, GetTranslator().GetString("LblMajorConfigUpgrade"));  
                }

                LogToSplash("Initializing");
                try
                {
                    // Create PQ path
                    try
                    {
                        String pqdatapath = GetUserDataPath() + Path.DirectorySeparatorChar + "GPX" + Path.DirectorySeparatorChar + "PQ" + Path.DirectorySeparatorChar;
                        if (!Directory.Exists(pqdatapath))
                            Directory.CreateDirectory(pqdatapath);
                    }
                    catch (Exception)
                    { }
                    
                    // Create DB path
                    try
                    {
                        String dbdatapath = GetUserDataPath() + Path.DirectorySeparatorChar + "DB" + Path.DirectorySeparatorChar;
                        if (!Directory.Exists(dbdatapath))
                            Directory.CreateDirectory(dbdatapath);
                    }
                    catch (Exception)
                    { }

                    // Ouverture de la base MGM si besoin
                    try
                    {
                    	 if (ConfigurationManager.AppSettings["autosavedb"] == "True")
                    	 {
                    	 	var mgmdbpath = GetDBDataFile();
                    	 	bool create = (File.Exists(mgmdbpath))?false:true;
                    		_dbmgm = new MGMDataBase(this, mgmdbpath, create);
                    	 }
                    }
                    catch(Exception)
                    { }

                    if (ConfigurationManager.AppSettings["usekm"] == "True")
                        _bUseKm = true;
                    else
                        _bUseKm = false;

                    if (ConfigurationManager.AppSettings["useGCPopularityFormula"] == "True")
                        _bUseGCPopularity = true;
                    else
                        _bUseGCPopularity = false;               

                    // Compute home coordinates
                    _dHomeLat = MyTools.ConvertToDouble(ConfigurationManager.AppSettings["mylocationlat"]);
                    _dHomeLon = MyTools.ConvertToDouble(ConfigurationManager.AppSettings["mylocationlon"]);
                    _sHomeLat = ConfigurationManager.AppSettings["mylocationlat"];
                    _sHomeLon = ConfigurationManager.AppSettings["mylocationlon"];

                    // Various postinit
                    CreateTableAttributes();
                    ReadThirdPartiesSpecificities();
                    
                    // init bitmaps for menus
                    _imgMenus = new Dictionary<string, Image>();
            		ParseImageFolderForMenus("Menus", "*.png", false);
            
                    // Check internet access
                   	LogToSplash("Checking internet access", false);
                    CheckInternetAccess();
                    
                    // Un ping
                    DoPing();
                    
                    // Init cachedetail
                    LogToSplash("Initializing rendering engines", false);
                    this.Text += " - " + GetTranslator().GetString("User") + ": " + ConfigurationManager.AppSettings["owner"];
                    _cacheDetail = new CacheDetail(this);
                    _cacheDetail.Icon = this.Icon;
                    _cacheDetail.LoadPageTextDefault();
                    _cacheDetail.Hide();
                    
                    // Initialise CacheCache
                   	// CacheCache
		            Log("Creating CacheCache");
		            _cachecache = new CacheCache(this, _cacheDetail);
		            
                    // Load offline data
                    LogToSplash("Loading offline data");
                    try
                    {
                        _odfile = GetUserDataPath() + Path.DirectorySeparatorChar + "OfflineData.dat";
                        if (File.Exists(_odfile))
                            _od = OfflineData.Deserialize(_odfile);
                        else
                            _od = new OfflineData();

                        // Create list of existing tags
                        CreateListStringTags();

                        // some check for corrupter OD data
                        // Due to some glitch, some OCD does not have _Code filled
                        bool bWrite = false;
                        foreach (KeyValuePair<String, OfflineCacheData> paire in _od._OfflineData)
                        {
                            if (paire.Value._Code == "")
                            {
                                paire.Value._Code = paire.Key;
                                bWrite = true;
                            }
                        }
                        if (bWrite)
                        {
                            _od.Serialize(_odfile);
                        }
                    }
                    catch (Exception exc2)
                    {
                    	Log("!!!! " + GetException("Loading offline data", exc2));
                        _od = new OfflineData();
                    }

                    // Loading ignorelist
                    try
                    {
                        _ignorefile = GetUserDataPath() + Path.DirectorySeparatorChar + "IgnoreList.dat";
                        Log("_ignorefile = " + _ignorefile);
                        if (File.Exists(_ignorefile))
                        {
                            // On va lire le fichier
                            // _ignoreList
                            using (StreamReader r = new StreamReader(_ignorefile, Encoding.Default))
                            {
                                String code = "";
                                while ((code = r.ReadLine()) != null)
                                {
                                    String[] vals = code.Split(';');
                                    MiniGeocache geo = new MiniGeocache();
                                    geo._Code = vals[0];
                                    geo._Name = vals[1];
                                    geo._Type = vals[2];
                                    geo._Container = vals[3];
                                    geo._D = vals[4];
                                    geo._T = vals[5];
                                    _ignoreList.Add(geo._Code, geo);
                                }
                            }
                        }
                    }
                    catch (Exception exc2)
                    {
                        Log("!!!! " + GetException("Loading ignore list", exc2));
                    }

                    // Init image list
                    Log("InitImageList");
                    LogToSplash("Initializing graphics");
                    InitImageList();

                    ImageList imgForTab = new ImageList();
                    imgForTab.ColorDepth = ColorDepth.Depth32Bit;
                    imgForTab.ImageSize = new Size(16, 16);
                    imgForTab.Images.Add("(*)", _listImagesSized[getIndexImages("True")]);
                    imgForTab.Images.Add("min", _listImagesSized[getIndexImages("EarthHigh")]); // "min"
                    imgForTab.Images.Add("max", _listImagesSized[getIndexImages("EarthLow")]); // "max"
                    
                    // Les icones pour les tab actives ou inactives
                    AddIconToTabControlList(imgForTab, "TabAlso"); 
					AddIconToTabControlList(imgForTab, "TabArea"); 
					AddIconToTabControlList(imgForTab, "TabAttM"); 
					AddIconToTabControlList(imgForTab, "TabAttP"); 
					AddIconToTabControlList(imgForTab, "TabCountry"); 
					AddIconToTabControlList(imgForTab, "TabDate"); 
					AddIconToTabControlList(imgForTab, "TabDist"); 
					AddIconToTabControlList(imgForTab, "TabDT"); 
					AddIconToTabControlList(imgForTab, "TabHome"); 
					AddIconToTabControlList(imgForTab, "TabMulti");  
					AddIconToTabControlList(imgForTab, "TabSize"); 
					AddIconToTabControlList(imgForTab, "TabStat"); 
					AddIconToTabControlList(imgForTab, "TabStatus"); 
					AddIconToTabControlList(imgForTab, "TabTB"); 
					AddIconToTabControlList(imgForTab, "TabType"); 

                    tabControl1.ImageList = imgForTab;

                    // Default maximize icon for tabPage15 (tabmap)
                    tabPage15_cachesPreviewMap.ImageKey = "max";
                    
                    // Default icon for other tabpages
                    AddTabPageIconInfo(tabPage7User, "TabAlso");
					AddTabPageIconInfo(tabPage16Area, "TabArea");
					AddTabPageIconInfo(tabPage14AttOut, "TabAttM");
					AddTabPageIconInfo(tabPage13AttIn, "TabAttP");
					AddTabPageIconInfo(tabPage12Region, "TabCountry");
					AddTabPageIconInfo(tabPage18Date, "TabDate");
					AddTabPageIconInfo(tabPage10Near, "TabDist");
					AddTabPageIconInfo(tabPage5DT, "TabDT");
					AddTabPageIconInfo(tabPage3Dist, "TabHome");
					AddTabPageIconInfo(tabPage11Multi, "TabMulti");
					AddTabPageIconInfo(tabPage1Size, "TabSize");
					AddTabPageIconInfo(tabPageFavPop, "TabStat");
					AddTabPageIconInfo(tabPage4Status, "TabStatus");
					AddTabPageIconInfo(tabPage9TB, "TabTB");
					AddTabPageIconInfo(tabPage2Type, "TabType");

					
                    //  Some icons stored in memory for faster display
                    _imgNothing = _listImagesSized[getIndexImages("Nothing")];
                    _imgNote = _listImagesSized[getIndexImages("Note")];
                    _imgNoteSpoiler = _listImagesSized[getIndexImages("NoteSpoiler")];
                    _imgNoteNoSpoiler = _listImagesSized[getIndexImages("NoteNoSpoiler")];
                    _imgSpoiler = _listImagesSized[getIndexImages("Spoiler")];
                    _imgNoSpoiler = _listImagesSized[getIndexImages("NoSpoiler")];
                    
                    // Create cach status
                    Log("Create CacheStatus");
                    _cacheStatus = new CacheStatus(this);
                    _cacheStatus.LoadCacheStatus();
                    
                    // Load profiles
                    Log("Create ProfileMgr");
                    _profileMgr = new ProfileMgr(this);
                    _profileMgr.LoadProfiles();
                    _profileMgr.UpdateBasedOnMGMCurrentProfile(this);
                    
                    Log("LoadDB");
                    LogToSplash("Loading database");
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    _ThreadProgressBarTitle = "Loading database";
                    CreateThreadProgressBar();
                    LoadDB();
                    KillThreadProgressBar();
                    stopWatch.Stop();
                    // Get the elapsed time as a TimeSpan value.
                    TimeSpan ts = stopWatch.Elapsed;

                    // Pin / Unpin button
                    btnPinUnpin.Image = _listImagesSized[getIndexImages("Pinned")];
                    btnPinUnpin.Text = "";
                    btnPinUnpin.BackColor = Color.Transparent;
                    btnPinUnpin.Tag = 178;

                    // Format and display the TimeSpan value. 
                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds);
                    Log(elapsedTime);

                    // Init lstv
                    Log("InitListViewCache");
                    LogToSplash("Initializing display");
                    InitListViewCache();

                    Log("BuildListViewCache");
                    LogToSplash("Initializing caches");
                    BuildListViewCache();

                    Log("Instanciating Map display");
                    LogToSplash("Instanciating Map display");
                    if (ConfigurationManager.AppSettings["useportablegmapcache"] == "True")
                    {
                        _cachesPreviewMap.CacheLocation = GetUserDataPath() + Path.DirectorySeparatorChar + "MapCache";
                        Log("Changing _cachesPreviewMap.CacheLocation to " + _cachesPreviewMap.CacheLocation);
                    }
                    else
                        Log("Keeping _cachesPreviewMap.CacheLocation at " + _cachesPreviewMap.CacheLocation);

                    UpdateMapProviderImpl(_cachesPreviewMap);
                    
                    _cachesPreviewMap.DragButton = MouseButtons.Left;
                    _cachesPreviewMap.DisableFocusOnMouseEnter = true;
                    _cachesPreviewMap.WindowsToCheck = _cacheDetail;

                    GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
                    _cachesPreviewMap.Position = new PointLatLng(0, 0);
                    GMapWrapper.CreateOverlays(_cachesPreviewMap);
                    // On affiche par défaut l'overlay des waypoints ici !!
                    (_cachesPreviewMap.Overlays[GMapWrapper.WAYPOINTS] as GMapOverlayCustom).ForceHide = false;

                    _cachesPreviewMap.OnMapZoomChanged += new GMap.NET.MapZoomChanged(cachesPreviewMap_OnMapZoomChanged);
                    _cachesPreviewMap.OnMarkerClick += new MarkerClick(this.anymap_OnMarkerClick);
                    Log("Update Bookmark overlay");
                    UpdateBookmarkOverlay(GetBookmarks());

                    Log("PopulateListViewCache");
                    LogToSplash("Displaying caches");
                    PopulateListViewCache(null);

                    // Init filters
                    Log("InitFilterImages");
                    LogToSplash("Initializing filters");
                    InitFilterImages();

                    toolStripStatusLabel1.Text += " - " + GetTranslator().GetString("SSDBLoaded") + " " + elapsedTime;
                    Version version = Assembly.GetEntryAssembly().GetName().Version;
                    toolStripStatusLabel2.Text = GetTranslator().GetString("Version") + " " + version;

                    // Final init : change font of every control
                    LogToSplash("Finalize look and feel");
                    
                    // trust all certificates
                    System.Net.ServicePointManager.ServerCertificateValidationCallback =
                        delegate(object sender2, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                        System.Security.Cryptography.X509Certificates.X509Chain chain,
                        System.Net.Security.SslPolicyErrors sslPolicyErrors)
                        {
                            return true; // **** Always accept
                        };

                    // On fixe le proxy de GMap.NET
                    WebProxy proxy = GetProxy();
                    if (proxy != null)
                    {
                        //mapDisplay.gmap.MapProvider
                        GMap.NET.MapProviders.GMapProvider.WebProxy = proxy;
                    }
                    
                    // Hide or show tabpage15 ?
                    if (DisplayHideTabMap()) // DOES NOT includes internet access check                
                    {
                        //tabControl1.SelectedTab = tabPage15;
                    }
                    // end cache maps

                    // Internet access icon
                    statusStrip1.ShowItemToolTips = true;
                    UpdateInternetAccessStatus();

                    // Filter button
                    btnSearch.BackColor = Color.Goldenrod;

                    // Close button for TableControl of CacheDetail
                    _cacheDetail.tabControlCD._CloseButton = _listImagesSized[getIndexImages("Close")];
                    _cacheDetail.tabControlCD._GearButton = _listImagesSized[getIndexImages("Gear")];
                    _cacheDetail.tabControlCD._EarthButton = _listImagesSized[getIndexImages("Earth")];
                    _cacheDetail.tabControlCD._fGearProcessing = null;

                    // Load filters
                    Log("LoadFilters");
                    LogToSplash("Loading filters");
                    LoadFilters();

                    toolStripStatusLabel3.Text = "";
                    InitContextMenu();
                    UpdateHMIForGC();

                    // Le recalcul de la taille du splitter vertical si besoin
                    // splitContainer1.SplitterDistance
                    // On doit voir la droite de groupBox4 avec une marge
                    int minVerticalSplitterWidth = groupBox4.Location.X + groupBox4.Size.Width + groupBox4.Margin.Left - 1;
                    if (splitContainer2.SplitterDistance < minVerticalSplitterWidth)
                    {
                        splitContainer2.SplitterDistance = minVerticalSplitterWidth;
                        splitContainer2.Panel1MinSize = minVerticalSplitterWidth;
                    }
                    // Idem splitter horizontal
                    int minHorizontalSplitterHeight = menuStrip1.Size.Height + lblTipEnlargeAreaWebBrowser.Location.Y + lblTipEnlargeAreaWebBrowser.Size.Height + 1;
                    if (splitContainer1.SplitterDistance < minHorizontalSplitterHeight)
                    {
                        splitContainer1.SplitterDistance = minHorizontalSplitterHeight;
                        splitContainer1.Panel1MinSize = minHorizontalSplitterHeight;
                    }

                    // Populate menu with DB list
                    UpdateMenuWithDB();

                    
                    if (_bInternetAvailable  && (ConfigurationManager.AppSettings["autocheckupdate"] == "True"))
                    {
                        LogToSplash("Checking for updates");
                        CheckUpdate(true, false);
                    }

                    LogToSplash("Loading plugins");
                    String sResult = CompilePlugins();
                    if (sResult != "")
                    {
                        LogToSplash(GetTranslator().GetString("WarTitle"));
                        MsgActionWarning(this, sResult, false);
                    }

                    
                    TranslateForm();
                    
                    
                    
                    LogToSplash("We're almost ready to go...");
                    if (_errorMessageLoad != "")
                    {
                        LogToSplash(GetTranslator().GetString("WarTitle"));
                        MsgActionWarning(this, _errorMessageLoad, false);
                    }
                }
                catch (Exception exc)
                {
                    KillThreadProgressBar();
                    LogToSplash("SH*T, something went wrong!");
                    Log("!!!! " + GetException("General failure during MainWindow construction", exc));
                    CloseLog();
                    throw;
                }

                ActivateDebugBtn(ConfigurationManager.AppSettings["debugbtn"] == "True");
            }
            catch (Exception)
            {
                KillThreadProgressBar();
                LogToSplash("SH*T, something went wrong!");
                throw;
            }
            
            KillSplashAndShow();

            // Mise à jour forcée
            if (_bMajorUpdateDetected)
            {
                CheckUpdate(false, true);
            }
        }

        private void UpdateMGMInternalDB()
        {
        	try
            {
        		if ((ConfigurationManager.AppSettings["autosavedb"] == "True") && (_dbmgm != null))
        		{
        			 _ThreadProgressBarTitle = GetTranslator().GetString("LblDBSaveInProgress");
                	CreateThreadProgressBar(500);
       
	                
        			MGMDataBase.DBinfo dbi = new MGMDataBase.DBinfo ();
        			_dbmgm.InsertGeocaches(_caches.Values.ToList(), false, null, ref dbi);
        			
        			KillThreadProgressBar();
        		}
            }
            catch(Exception)
            { 
            	KillThreadProgressBar();
            }
        }
        		
        /// <summary>
        /// Change map provider, update configuration and map controls
        /// </summary>
        /// <param name="map">map control</param>
        public void UpdateMapProviderImpl(GMapControlCustom map)
        {
            String providerdbid = ConfigurationManager.AppSettings["MapProvider"];
            Log("UpdateMapProviderImpl " + providerdbid);
            if (providerdbid == "")
            {
            	map.AssignMapProvider(GMap.NET.MapProviders.GoogleMapProvider.Instance);
                UpdateConfFile("MapProvider", map.MapProvider.DbId.ToString());
            }
            else
            {
                try
                {
                    // On récupère l'id
                    int dbid = Int32.Parse(providerdbid);

                    // On tente de retrouver le provider en fonction de son nom
                    GMap.NET.MapProviders.GMapProvider provider = GMap.NET.MapProviders.GMapProviders.TryGetProvider(dbid);
                    if (provider != null)
                    {
                        // on a trouvé !
                        map.AssignMapProvider(provider);
                    }
                    else
                    {
                        // ça a raté !
                        map.AssignMapProvider(GMap.NET.MapProviders.GoogleMapProvider.Instance);
                        UpdateConfFile("MapProvider", map.MapProvider.DbId.ToString());
                    }
                }
                catch (Exception e)
                {
                    // ça a raté !
                    map.AssignMapProvider(GMap.NET.MapProviders.GoogleMapProvider.Instance);
                    UpdateConfFile("MapProvider", map.MapProvider.DbId.ToString());
                    Log(GetException("Updating provider", e));
                }
            }
        }

        /// <summary>
        /// Activates / Deactivate debug and information buttons
        /// FOR DEV ONLY!!!
        /// You never know what's behind the button and what will occur... You're warned!
        /// </summary>
        /// <param name="bActivate">if true, displays the buttons, false will hide the buttons</param>
        public void ActivateDebugBtn(bool bActivate)
        {
            btnDEBUG.Visible = bActivate;
            btnINFO.Visible = bActivate;
            //donateToolStripMenuItem.Visible = bActivate;
        }

        /// <summary>
        /// Toggle information / debug button visibility
        /// </summary>
        public void DisplayHideDebugBtn()
        {
            if (ConfigurationManager.AppSettings["debugbtn"] == "False")
            {
                ActivateDebugBtn(true);
                UpdateConfFile("debugbtn", "True");
            }
            else
            {
                ActivateDebugBtn(false);
                UpdateConfFile("debugbtn", "False");
            }
        }

        /// <summary>
        /// Update File menu with existing caches databases
        /// </summary>
        public void UpdateMenuWithDB()
        {
            if (existingDBToolStripMenuItem.DropDownItems != null)
                existingDBToolStripMenuItem.DropDownItems.Clear();

            ListTextCoord b = GetBookmarks();
            if (b._Databases.Count() != 0)
                existingDBToolStripMenuItem.Enabled = true;
            else
                existingDBToolStripMenuItem.Enabled = false;

            foreach (DatabaseOfFiles db in b._Databases)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(db._Name);
                item.Click += new System.EventHandler(dbToolStripMenuItem_Click);
                item.Tag = db._Name;
                item.Name = "existingDBToolStripMenuItem" + db._Name;
                existingDBToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private void dbToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Uncheck everyone....
            UncheckDBMenu();

            ToolStripMenuItem mnu = sender as ToolStripMenuItem;
            if (mnu != null)
            {
                String key = (String)(mnu.Tag);
                ListTextCoord b = GetBookmarks();
                String pathdb = GetUserDataPath() + Path.DirectorySeparatorChar + "GPX";
                foreach (DatabaseOfFiles db in b._Databases)
                {
                    if (db._Name == key)
                    {
                        List<String> fs = new List<string>();
                        foreach (String f in db._Files)
                        {
                            fs.Add(pathdb + f);
                        }
                        LoadBatchOfFiles(fs.ToArray(), true, false);
                        mnu.Checked = true;
                        break;
                    }
                }
            }
        }

        private void UncheckDBMenu()
        {
            foreach (ToolStripMenuItem m in existingDBToolStripMenuItem.DropDownItems)
            {
                m.Checked = false;
            }
        }

        /// <summary>
        /// Enable GC.com menu if password defined
        /// </summary>
        public void UpdateHMIForGC()
        {
            
            if (ConfigurationManager.AppSettings["ownerpassword"] != "") // On ne cherche pas à vérifier ou décrypter !
            {
                // password provided, let's enable everything
                liveToolStripMenuItem.Visible = true;
                liveToolStripMenuItem.Enabled = true;
            }
            else
            {
                // password NOT provided, grey Live and enable the rest
                liveToolStripMenuItem.Enabled = false;
                liveToolStripMenuItem.Visible = true;
            }
        }

        /// <summary>
        /// Create list of tags
        /// </summary>
        public void CreateListStringTags()
        {
            _listExistingTags = new List<string>();
            foreach (KeyValuePair<String, OfflineCacheData> paire in _od._OfflineData)
            {
                if (paire.Value._Tags.Count != 0)
                {
                    foreach (String t in paire.Value._Tags)
                    {
                        if (_listExistingTags.Contains(t) == false)
                            _listExistingTags.Add(t);
                    }
                }
            }

            // and now the pics
            _imgTags = new Dictionary<string,Image>();
            foreach(String tag in _listExistingTags)
            {
                String t = UppercaseFirst(tag);
                Bitmap bmp = MyTools.CreateBitmapImage(t);
                _imgTags.Add(t, bmp);
            }
        }

        /// <summary>
        /// Check if internet access is available
        /// </summary>
        /// <returns>True if internet is accessible</returns>
        public bool CheckInternetAccess()
        {
            try
            {
            	
                var client = new WebClient();
                WebProxy proxy = GetProxy();
                if (proxy != null)
                    client.Proxy = proxy;
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    _bInternetAvailable = true;
                }
                
                
                // Ca peut merder chez certain, chez Aklem par exemple
                /*
            	try
                {
                    String rep = MyTools.GetRequest(new Uri("http://www.google.com"), GetProxy(), 2000);
                    if (rep != "")
                        _bInternetAvailable = true;
                    else
                    	_bInternetAvailable = false;
                }
                catch
                {
                	_bInternetAvailable = false;
                }*/
            }
            catch
            {
                _bInternetAvailable = false;
            }
            Log("Internet available : " + _bInternetAvailable.ToString());
            return _bInternetAvailable;
        }

        private void UpdateInternetAccessStatus()
        {
            toolStripStatusLabel4.Text = "";
            if (!_bInternetAvailable)
            {
                toolStripStatusLabel4.Image = _listImagesSized[getIndexImages("InternetNo")];
                toolStripStatusLabel4.ToolTipText = GetTranslator().GetString("LblInternetNo");
            }
            else
            {
                toolStripStatusLabel4.Image = _listImagesSized[getIndexImages("InternetYes")];
                toolStripStatusLabel4.ToolTipText = GetTranslator().GetString("LblInternetYes");
            }
        }

        private bool DisplayHideTabMap()
        {
            // Check if connected to internet
            // On s'en fout maintenant
            // CheckInternetAccess();

            if ((ConfigurationManager.AppSettings["displaytabmap"] == "True"))// && _bInternetAvailable)
            {
                // Show, nothing to prevent it
                if (tabControl1.Controls.Contains(tabPage15_cachesPreviewMap) == false)
                {
                    // need to add it
                    tabControl1.Controls.Add(tabPage15_cachesPreviewMap);
                }
                return true;
            }
            else
            {
                // Need to remove it if needed
                if (tabControl1.Controls.Contains(tabPage15_cachesPreviewMap) == true)
                {
                    // need to remove it
                    tabControl1.Controls.Remove(tabPage15_cachesPreviewMap);
                }
                return false;
            }
        }

        private TranslationManager GetTranslatorForCurrentLocale()
        {
            String keyLng = Thread.CurrentThread.CurrentUICulture.Name;
            if (_languages.ContainsKey(keyLng) == false)
                keyLng = "en-GB"; // force default language - should be there !!!
            return new TranslationManager(_languages[keyLng]._path, true);
        }

        private TranslationManager GetTooltipsTranslatorForCurrentLocale()
        {
            String keyLng = Thread.CurrentThread.CurrentUICulture.Name;
            if (_tooltips.ContainsKey(keyLng) == false)
                keyLng = "en-GB"; // force default language - should be there !!!
            return new TranslationManager(_tooltips[keyLng]._path, false);
        }

        private static void HandleDefaultLanguageForLocales()
        {
            // Default is English
            // Only other language will be French, so if Culture is "fr"
            // (regardless Specific Culture),
            // We set it to French fr-FR
            if (Thread.CurrentThread.CurrentUICulture.Name.StartsWith("fr"))
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-FR");
            }
            else if (Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de"))
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-DE");
            }
        }

        private void LookForLanguages()
        {
            defaultsystemToolStripMenuItem.Tag = "";

            // On recherche les languages
            // **************************
            String lngPath = GetResourcesDataPath() + Path.DirectorySeparatorChar + "Lang";
            string[] lngFiles = Directory.GetFiles(lngPath, "*.lng", SearchOption.AllDirectories);
            foreach (string f in lngFiles)
            {
                try
                {
                    LanguageItem litem = new LanguageItem();
                    litem._path = f;
                    TranslationManager.GetInfo(litem._path, ref litem._name, ref litem._ename, ref litem._locale);
                    _languages.Add(litem._locale, litem);

                    ToolStripMenuItem item = new ToolStripMenuItem(litem._name);
                    item.Click += new System.EventHandler(langToolStripMenuItem_Click);
                    item.Tag = litem._locale;
                    item.Name = "languageToolStripMenuItem" + litem._locale;
                    languageToolStripMenuItem.DropDownItems.Add(item);
                }
                catch (Exception exc)
                {
                    Log("!!!! " + GetException("Building language menu", exc));
                    Log("!!!! Failed loading language " + f);
                    _errorMessageLoad += "Error loading " + f + "\r\n";
                }
            }

            // On recherche les tooltips associés
            // **********************************
            lngPath = GetResourcesDataPath() + Path.DirectorySeparatorChar + "Tips";
            string[] tipsFiles = Directory.GetFiles(lngPath, "*.tips", SearchOption.AllDirectories);
            foreach (string f in tipsFiles)
            {
                try
                {
                    LanguageItem litem = new LanguageItem();
                    litem._path = f;
                    TranslationManager.GetInfo(litem._path, ref litem._name, ref litem._ename, ref litem._locale);
                    _tooltips.Add(litem._locale, litem);
                }
                catch (Exception exc)
                {
                    Log("!!!! " + GetException("Applying tooltip on language", exc));
                    Log("!!!! Failed loading language " + f);
                    _errorMessageLoad += "Error loading " + f + "\r\n";
                }
            }
        }

        /// <summary>
        /// Kill splashscreen and show main window
        /// Taadaaaa!
        /// </summary>
        public void KillSplashAndShow()
        {
        	Log("KillSplashAndShow");
            if (MainWindow.IsLinux) // We are on Linux / MacOS, thus the wait loop is not mandatory
            {
            }
            else
            {
                if (_cacheDetail != null)
                {
                    while (!_cacheDetail._bInitialized)
                    {
                        System.Threading.Thread.Sleep(50); // pause for 1/20 second
                        Application.DoEvents();
                    }
                }
            }

            Log("Showing !");
            LogToSplash("Let there be light!");
            this.Show();
            this.SetTopLevel(true);
            this.Activate();
            Log("Kill splash");
            KillSplash();
            Log("Ready !");
        }        

        private void KillSplash()
        {
            myUpdateThreadInfo = null;
            if (ConfigurationManager.AppSettings["debugbtn"] != "True")
            {
                if (tsplash != null)
                    tsplash.Abort();
                tsplash = null;
            }
            _newSplashForm = null;
        }
       

        /// <summary>
        /// Create translation key of a cache attribute
        /// </summary>
        /// <param name="attributeValue">cache attribute name</param>
        /// <returns>key usable with translation manager</returns>
        public String CreateAttributeTranslationKey(String attributeValue)
        {
            String s = "att_" + attributeValue;
            s = s.Replace(" ", "_");
            s = s.Replace(" ", "_");
            s = s.Replace("-", "_d_");
            s = s.Replace("(", "_ob_");
            s = s.Replace(")", "_cb_");
            s = s.Replace("+", "_p_");
            s = s.ToLower();
            return s;
        }

        /// <summary>
        /// Update a configuration entry in the configuration file
        /// </summary>
        /// <param name="entry">entry to update</param>
        /// <param name="value">new value</param>
        public void UpdateConfFile(String entry, String value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove(entry);
            // On stocke dans le fichier de config
            config.AppSettings.Settings.Add(entry, value);
            ConfigurationManager.RefreshSection("appSettings");
            config.Save(ConfigurationSaveMode.Modified);
        }

        private void CheckConfFileEntry(String entry, String defval)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.AppSettings.Settings[entry] == null)
            {
                Log("!!!! Configuration entry " + entry + " missing, set default value : " + defval);
                // Missing entry
                config.AppSettings.Settings.Add(entry, defval);
                ConfigurationManager.RefreshSection("appSettings");
                config.Save(ConfigurationSaveMode.Modified);
            }
        }

        private bool CheckConfigurationIntegrity()
        {
            CheckConfFileEntry("urlupdate", "http://mgmgeo.free.fr;http://mgmgeo.ddns.net:666/mgmgeo");
            CheckConfFileEntry("autocheckupdate", "True");
            CheckConfFileEntry("enabletooltips", "True");
            CheckConfFileEntry("disabletooltipsformainlist", "False");
            CheckConfFileEntry("logfile", "log.txt");
            CheckConfFileEntry("forceLocale", "");
            CheckConfFileEntry("openGeocachingEmbedded", "False");
            CheckConfFileEntry("openCacheEmbedded", "True");
            CheckConfFileEntry("datapath", "");
            CheckConfFileEntry("daysfornew", "30");
            CheckConfFileEntry("mylocationlat", "48.769408");
            CheckConfFileEntry("mylocationlon", "1.967473");
            CheckConfFileEntry("key", "");
            CheckConfFileEntry("owner", "");
            CheckConfFileEntry("ownerpassword", "");
            CheckConfFileEntry("ignorefounds", "False");
            CheckConfFileEntry("usekm", "True");
            CheckConfFileEntry("hidencolumns", "");
            CheckConfFileEntry("ordercolumns", "");
            CheckConfFileEntry("forceupdate", "");
            CheckConfFileEntry("debugbtn", "False");
            CheckConfFileEntry("proxyused", "False");
            CheckConfFileEntry("proxydomain", "");
            CheckConfFileEntry("proxylogin", "");
            CheckConfFileEntry("proxypassword", "");
            CheckConfFileEntry("usespoilerskeywords", "False");
            CheckConfFileEntry("getimagesfromgallery", "False");
            CheckConfFileEntry("spoilerskeywords", _sSpoilerDefaultKeywords);
            CheckConfFileEntry("spoilerdelaydownload", "0");
            CheckConfFileEntry("displaytabmap", "True");
            CheckConfFileEntry("disablethreadprogressbar", "False");
            CheckConfFileEntry("excludedfiles","");
            CheckConfFileEntry("yandextrnsl", "trnsl.1.1.20161026T084236Z.3c7207e1a84c59bd.e9ff471f5d54a9ac80db844593c3679a8df439da");
            CheckConfFileEntry("useGCPopularityFormula", "False");
            CheckConfFileEntry("markmissingcaches", "False");
            CheckConfFileEntry("numberlastlogssymbols", "1");
            CheckConfFileEntry("useportablegmapcache", "False");
           	CheckConfFileEntry("dtonmapgradient", "3");
           	CheckConfFileEntry("toolbarids", "");
           	CheckConfFileEntry("autosavedb", "False");
            CheckConfFileEntry("displayscaleonmap", "False");
            /*
            // Ajout du mot de passe proxy chiffré s'il n'est pas chiffré déjà
            String proxypassword = ConfigurationManager.AppSettings["proxypassword"];
            // Est-il déjà chiffré ?
            String proxypassword_decyph = "";
            if (!StringCipher.CustomDecrypt(proxypassword, ref proxypassword_decyph))
            {
                // On n'a pas pu déchiffrer le password ça veut dire qu'il n'était pas déjà chiffré
                // on le chiffre et on l'écrit
                String proxypassword_cyph = StringCipher.CustomEncrypt(proxypassword);
                UpdateConfFile("proxypassword", proxypassword_cyph);
            }
            // else on a réussi à le déchiffrer, donc tout va bien
            */

            CheckForMandatoryConfig();
            return false;
        }

        void CheckForMandatoryConfig()
        {
            // Force specific configuration parameters (used after an updated for instance)
            String data = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + "MANDATORY_CONFIG.dat";
            if (File.Exists(data))
            {
                try
                {
                    
                    // Open file and read all values
                    String line;

                    // Read the file and display it line by line.
                    System.IO.StreamReader file =
                       new System.IO.StreamReader(data, System.Text.Encoding.Default, true);

                    while ((line = file.ReadLine()) != null)
                    {
                        int pos = line.IndexOf("=");
                        if (pos != -1)
                        {
                            String key = line.Substring(0, pos);
                            String val = "";
                            if ((pos + 1) < line.Length) // handle empty value
                                val = line.Substring(pos + 1);
                            
                            // Ensure entry exists
                            CheckConfFileEntry(key, val);
                            // Update conf file
                            UpdateConfFile(key, val);
                        }
                    }
                    file.Close();

                    // Now remove the file, no need to perform this operation after every launch!
                    File.Delete(data);
                }
                catch (Exception)
                {
                }
            }
        }

        private String GetIndexOfAttribute(String attribute)
        {
            return _tableAttributes.FirstOrDefault(x => x.Value == attribute).Key;
        }

        private void _tableAttributesCategoriesAdd(String key, int value)
        {
            _tableAttributesCategory.Add(key, value);
        }

        private void _tableAttributesCompletedAdd(String value, String key)
        {
            _tableAttributesCompleted.Add(key, value);
        }
        

        /// <summary>
        /// Return path where data are stored
        /// </summary>
        /// <returns></returns>
        public String GetInternalDataPath()
        {
        	// Par défaut c'est dans le répertoire de l'exécutable
        	// On teste si "Img" existe, sinon on utilise la variable "datapath"
        	String exepath = Path.GetDirectoryName(Application.ExecutablePath);
        	String path = exepath + Path.DirectorySeparatorChar + "Resources";
        	if (Directory.Exists(path)) // Ok, on semble avoir une installation potable
        		return exepath;
        	else
        	{
        		// Bon c'est raté, on doit être en mode développeur, on retourne le chemin dans la configuration
        		return ConfigurationManager.AppSettings["datapath"];
        	}
        }
        
        /// <summary>
        /// Return path where user data are stored
        /// </summary>
        /// <returns></returns>
        public String GetUserDataPath()
        {
        	return GetInternalDataPath() + Path.DirectorySeparatorChar + "Data";
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public String GetDBDataFile()
        {
        	return GetUserDataPath() + Path.DirectorySeparatorChar + "MainDatabase.db";
        }
        
        /// <summary>
        /// Return path where resources data are stored
        /// </summary>
        /// <returns></returns>
        public String GetResourcesDataPath()
        {
        	return GetInternalDataPath() + Path.DirectorySeparatorChar + "Resources";
        }
        
        void ReadThirdPartiesSpecificities()
        {
            // Pour C:Geo et ses traductions de type de waypoints
            FileStream fs = null;
            try
            {
                Log("Reading Specificities.xml");
                XmlDocument _xmldoc;
                XmlNodeList _xmlnode;
                _xmldoc = new XmlDocument();
                String filespec = GetUserDataPath() + Path.DirectorySeparatorChar + "Specificities.xml";
                _xmldoc.Load(filespec);
                _xmlnode = _xmldoc.SelectNodes("/Specific");
                
                // CGeo et ses types de waypoints traduits
                XmlNode dirs = _xmlnode[0].ChildNodes.Item(0);
                Log("C:Geo");
                foreach (XmlNode elt in dirs.ChildNodes)
                {
                    String eng = elt.Attributes[0].InnerText.Trim();
                    String trad = elt.Attributes[1].InnerText.Trim();
                    Log(eng + " => " + trad);
                    _tableWptsTypeTranslated.Add(eng, trad);
                }
            }
            catch (Exception exc)
            {
                Log("!!!! " + GetException("3rd parties specificities", exc));
                Log("!!!! Error reading specificities");

                if (fs != null)
                    fs.Close();
                fs = null;
            }
        }


        private void CreateTableAttributesImpl(String id, String PlainName, String HtmlName, bool NegativeAvailable, int Categorie)
        {
        	_tableAttributes.Add(id, PlainName);
			_tableAttributesCompletedAdd(id, HtmlName); // includes -yes
			_tableAttributesCategoriesAdd(id, Categorie);
			
			if (NegativeAvailable)
			{
				_tableAttributes.Add(id+"-no", PlainName+"-no");
				_tableAttributesCompletedAdd(id+"-no", HtmlName.Replace("-yes","-no"));
				_tableAttributesCategoriesAdd(id+"-no", Categorie);
			}
        }
        
        void CreateTableAttributes()
        {
        	// And now the categories
            // Permissions : 1
            // Equipment : 2
            // Conditions : 3
            // Hazards : 4
            // Facilities : 5
            // Specials : 6
            // les valeurs impossibles sur GC.com : 19, 20, 21, 22, 23, 39, 18, 43, 11, 12, 26, 2, 3, 4, 5, 44, 48, 49, 50, 51, 60, 64
            
            
        	CreateTableAttributesImpl("1", "Dogs", "dogs-yes", true, 1);
			CreateTableAttributesImpl("2", "Access or Parking Fee", "fee-yes", true, 2);
			CreateTableAttributesImpl("3", "Climbing gear", "rappelling-yes", true, 2);
			CreateTableAttributesImpl("4", "Boat", "boat-yes", true, 2);
			CreateTableAttributesImpl("5", "Scuba Gear", "scuba-yes", true, 2);
			CreateTableAttributesImpl("6", "Recommended for kids", "kids-yes", true, 3);
			CreateTableAttributesImpl("7", "Takes less than an hour", "onehour-yes", true, 3);
			CreateTableAttributesImpl("8", "Scenic view", "scenic-yes", true, 3);
			CreateTableAttributesImpl("9", "Significant Hike", "hiking-yes", true, 3);
			CreateTableAttributesImpl("10", "Difficult climbing", "climbing-yes", true, 3);
			CreateTableAttributesImpl("11", "May require wading", "wading-yes", true, 3);
			CreateTableAttributesImpl("12", "May require swimming", "swimming-yes", true, 3);
			CreateTableAttributesImpl("13", "Available at all times", "available-yes", true, 3);
			CreateTableAttributesImpl("14", "Recommended at night", "night-yes", true, 3);
			CreateTableAttributesImpl("15", "Available during winter", "winter-yes", true, 3);
			CreateTableAttributesImpl("16", "Cactus", "cactus-yes", true, 4); // XXX Not Used(16) XXX
			CreateTableAttributesImpl("17", "Poison plants", "poisonoak-yes", true, 4);
			CreateTableAttributesImpl("18", "Dangerous Animals", "dangerousanimals-yes", true, 4);
			CreateTableAttributesImpl("19", "Ticks", "ticks-yes", true, 4);
			CreateTableAttributesImpl("20", "Abandoned mines", "mine-yes", true, 4);
			CreateTableAttributesImpl("21", "Cliff / falling rocks", "cliff-yes", true, 4);
			CreateTableAttributesImpl("22", "Hunting", "hunting-yes", true, 4);
			CreateTableAttributesImpl("23", "Dangerous area", "danger-yes", true, 4);
			CreateTableAttributesImpl("24", "Wheelchair accessible", "wheelchair-yes", true, 5);
			CreateTableAttributesImpl("25", "Parking available", "parking-yes", true, 5);
			CreateTableAttributesImpl("26", "Public transportation", "public-yes", true, 5);
			CreateTableAttributesImpl("27", "Drinking water nearby", "water-yes", true, 5);
			CreateTableAttributesImpl("28", "Public restrooms nearby", "restrooms-yes", true, 5);
			CreateTableAttributesImpl("29", "Telephone nearby", "phone-yes", true, 5);
			CreateTableAttributesImpl("30", "Picnic tables nearby", "picnic-yes", true, 5);
			CreateTableAttributesImpl("31", "Camping available", "camping-yes", true, 5);
			CreateTableAttributesImpl("32", "Bicycles", "bicycles-yes", true, 1);
			CreateTableAttributesImpl("33", "Motorcycles", "motorcycles-yes", true, 1);
			CreateTableAttributesImpl("34", "Quads", "quads-yes", true, 1);
			CreateTableAttributesImpl("35", "Off-road vehicles", "jeeps-yes", true, 1);
			CreateTableAttributesImpl("36", "Snowmobiles", "snowmobiles-yes", true, 1);
			CreateTableAttributesImpl("37", "Horses", "horses-yes", true, 1);
			CreateTableAttributesImpl("38", "Campfires", "campfires-yes", true, 1);
			CreateTableAttributesImpl("39", "Thorns", "thorn-yes", true, 4);
			CreateTableAttributesImpl("40", "Stealth required", "stealth-yes", true, 3);
			CreateTableAttributesImpl("41", "Stroller accessible", "stroller-yes", true, 5);
			CreateTableAttributesImpl("42", "Needs maintenance", "firstaid-yes", false, 3);
			CreateTableAttributesImpl("43", "Watch for livestock", "cow-yes", true, 3);
			CreateTableAttributesImpl("44", "Flashlight required", "flashlight-yes", true, 2);
			CreateTableAttributesImpl("45", "Lost And Found Tour", "landf-yes", true, 6);
			CreateTableAttributesImpl("46", "Truck Driver/RV", "rv-yes", true, 1);
			CreateTableAttributesImpl("47", "Field Puzzle", "field_puzzle-yes", true, 3);
			CreateTableAttributesImpl("48", "UV Light Required", "uv-yes", true, 2);
			CreateTableAttributesImpl("49", "Snowshoes", "snowshoes-yes", true, 2);
			CreateTableAttributesImpl("50", "Cross Country Skis", "skiis-yes", true, 2);
			CreateTableAttributesImpl("51", "Special Tool Required", "s-tool-yes", true, 2);
			CreateTableAttributesImpl("52", "Night Cache", "nightcache-yes", true, 3);
			CreateTableAttributesImpl("53", "Park and Grab", "parkngrab-yes", true, 3);
			CreateTableAttributesImpl("54", "Abandoned Structure", "abandonedbuilding-yes", true, 3);
			CreateTableAttributesImpl("55", "Short hike (less than 1km)", "hike_short-yes", true, 3);
			CreateTableAttributesImpl("56", "Medium hike (1km-10km)", "hike_med-yes", true, 3);
			CreateTableAttributesImpl("57", "Long Hike (+10km)", "hike_long-yes", true, 3);
			CreateTableAttributesImpl("58", "Fuel Nearby", "fuel-yes", true, 5);
			CreateTableAttributesImpl("59", "Food Nearby", "food-yes", true, 5);
			CreateTableAttributesImpl("60", "Wireless Beacon", "wirelessbeacon-yes", true, 2);
			CreateTableAttributesImpl("61", "Partnership Cache", "partnership-yes", true, 6);
			CreateTableAttributesImpl("62", "Seasonal Access", "seasonal-yes", true, 3);
			CreateTableAttributesImpl("63", "Tourist Friendly", "touristok-yes", true, 3);
			CreateTableAttributesImpl("64", "Tree Climbing", "treeclimbing-yes", true, 2);
			CreateTableAttributesImpl("65", "Front Yard (Private Residence)", "frontyard-yes", true, 3);
			CreateTableAttributesImpl("66", "Teamwork Required", "teamwork-yes", true, 3);
			CreateTableAttributesImpl("67", "Geotour", "geotour-yes", true, 6);

        }

        /// <summary>
        /// Translate "Near" filter, based on Km or Mi usage
        /// </summary>
        public void TranslateFilterNearKmMi()
        {
            String unit = "";
            if (_bUseKm)
            {
                unit = GetTranslator().GetString("LVKm");
            }
            else
            {
                unit = GetTranslator().GetString("LVMi");
            }

            _filtersmenunear.DropDownItems[0].Text = "  5 " + unit;
            _filtersmenunear.DropDownItems[1].Text = " 10 " + unit;
            _filtersmenunear.DropDownItems[2].Text = " 20 " + unit;
            _filtersmenunear.DropDownItems[3].Text = " 50 " + unit;
            _filtersmenunear.DropDownItems[4].Text = "100 " + unit;
        }

        /// <summary>
        /// Create a ToolStripMenuItem using a translation key and an event handler
        /// Mandatory method to handle tooltips
        /// </summary>
        /// <param name="keytrans">Translation key</param>
        /// <param name="img">image</param>
        /// <param name="target">event handler</param>
        /// <param name="enabled">enable or not menu item</param>
        /// <returns>ToolStripMenuItem</returns>
        private ToolStripMenuItem CreateTSMI(String keytrans, Image img, Action<object, EventArgs> target, bool enabled)
        {
            ToolStripMenuItem ts = null;
            if (target == null)
            {
                ts = new ToolStripMenuItem(GetTranslator().GetString(keytrans));
            }
            else
            {
                ts = new ToolStripMenuItem(GetTranslator().GetString(keytrans), img, new EventHandler(target));
            }
            ts.Enabled = enabled;
            ts.Name = keytrans;
            return ts;
        }
        
        /// <summary>
        /// Create a ToolStripMenuItem using a translation key and an event handler
        /// Mandatory method to handle tooltips
        /// </summary>
        /// <param name="keytrans">Translation key</param>
        /// <param name="img">image</param>
        /// <param name="target">event handler</param>
        /// <returns>ToolStripMenuItem</returns>
        public ToolStripMenuItem CreateTSMI(String keytrans, Image img, Action<object, EventArgs> target)
        {
        	return CreateTSMI(keytrans, img, target, true);
        }

        /// <summary>
        /// Create a ToolStripMenuItem using a translation key and an event handler
        /// Mandatory method to handle tooltips
        /// </summary>
        /// <param name="keytrans">Translation key</param>
        /// <param name="target">event handler</param>
        /// <returns>ToolStripMenuItem</returns>
        public ToolStripMenuItem CreateTSMI(String keytrans, Action<object, EventArgs> target)
        {
            return CreateTSMI(keytrans, null, target);
        }

        /// <summary>
        /// Create a ToolStripMenuItem using a translation key and an event handler
        /// Mandatory method to handle tooltips
        /// </summary>
        /// <param name="keytrans">Translation key</param>
        /// <param name="target">event handler</param>
        /// <param name="enabled">enable or not menu item</param>
        /// <returns>ToolStripMenuItem</returns>
        public ToolStripMenuItem CreateTSMI(String keytrans, Action<object, EventArgs> target, bool enabled)
        {
            return CreateTSMI(keytrans, null, target, enabled);
        }
        
        /// <summary>
        /// Create a ToolStripMenuItem using a translation key
        /// Mandatory method to handle tooltips
        /// </summary>
        /// <param name="keytrans">Translation key</param>
        /// <returns>ToolStripMenuItem</returns>
        public ToolStripMenuItem CreateTSMI(String keytrans)
        {
            return CreateTSMI(keytrans, null);
        }

        private void InitContextMenu()
        {
        	
            _mnuContextMenu = new ContextMenuStrip();

            ToolStripMenuItem dispMenu = CreateTSMI("MNUDisplay");
            _mnuContextMenu.Items.Add(dispMenu);
            dispMenu.DropDownItems.Add(CreateTSMI("MNUSelectionDisplayCarto", SetDisplaySelectionOnCarto));
            dispMenu.DropDownItems.Add(CreateTSMI("MNUDetails", SetDisplayDetail));
            dispMenu.DropDownItems.Add(CreateTSMI("MNUGeo", SetDisplayGeocaching, GetInternetStatus()));
            dispMenu.DropDownItems.Add(CreateTSMI("MenuContextDisplayModifications", SetDisplayModifications));
            
            _offline = CreateTSMI("MNUOffline");
            _mnuContextMenu.Items.Add(_offline);
            _offline.DropDownItems.Add(CreateTSMI("MNUNote",WriteNoteOnCacheEvent));
            _offline.DropDownItems.Add(CreateTSMI("MNUViewOffline", ViewOfflineData));
            _offline.DropDownItems.Add(new ToolStripSeparator());
            _offline.DropDownItems.Add(CreateTSMI("MNUMakeOfflineAvail", CreateOfflineData, GetInternetStatus()));
            _offline.DropDownItems.Add(CreateTSMI("MNUDelOfflineData", DeleteOfflineData));
            _cacheimages = CreateTSMI("MNUDisplayimages");
            _offline.DropDownItems.Add(_cacheimages);
            
            _filtersmenu = CreateTSMI("MNUFilter");
            _mnuContextMenu.Items.Add(_filtersmenu);
            _filtersmenu.DropDownItems.Add(CreateTSMI("MNUDefineAsNearLocation", DefineAsLocationForNearFilter));
            _filtersmenunear = CreateTSMI("FMenuLimitToNear");
            _filtersmenu.DropDownItems.Add(_filtersmenunear);
            String unit = "";
            if (_bUseKm)
            {
                unit = GetTranslator().GetString("LVKm");
            }
            else
            {
                unit = GetTranslator().GetString("LVMi");
            }
            ToolStripMenuItem inear = null;
            inear = new ToolStripMenuItem("  5 " + unit, null, new EventHandler(SetLimitCacheCloserThan));
            inear.Tag = 5;
            _filtersmenunear.DropDownItems.Add(inear);
            inear = new ToolStripMenuItem(" 10 " + unit, null, new EventHandler(SetLimitCacheCloserThan));
            inear.Tag = 10;
            _filtersmenunear.DropDownItems.Add(inear);
            inear = new ToolStripMenuItem(" 20 " + unit, null, new EventHandler(SetLimitCacheCloserThan));
            inear.Tag = 20;
            _filtersmenunear.DropDownItems.Add(inear);
            inear = new ToolStripMenuItem(" 50 " + unit, null, new EventHandler(SetLimitCacheCloserThan));
            inear.Tag = 50;
            _filtersmenunear.DropDownItems.Add(inear);
            inear = new ToolStripMenuItem("100 " + unit, null, new EventHandler(SetLimitCacheCloserThan));
            inear.Tag = 100;
            _filtersmenunear.DropDownItems.Add(inear);

            _waypointsmenu = CreateTSMI("FMenuWaypointContext");
            _mnuContextMenu.Items.Add(_waypointsmenu);
            _waypointsmenu.DropDownItems.Add(CreateTSMI("FMenuWaypointContextAdd", WaypointContextAdd));
            _waypointsmenu.DropDownItems.Add(CreateTSMI("FMenuWaypointContextManage", WaypointContextManage));
           
            _modifymenu = CreateTSMI("FMenuToolsModCaches");
            _mnuContextMenu.Items.Add(_modifymenu);
            _modifymenu.DropDownItems.Add(CreateTSMI("FMenuModifyManualCoord", ModifyCoordToolStripMenuItemClick));
            _modifymenu.DropDownItems.Add(CreateTSMI("FMenuToolsModCachesName", modifyNameToolStripMenuItem_Click));
            _modifymenu.DropDownItems.Add(CreateTSMI("FMenuToolsModCachesNameReplace", ModifyNameReplaceToolStripMenuItemClick));
            _modifymenu.DropDownItems.Add(CreateTSMI("FMenuToolsModCachesTag", modifyNameWithTagToolStripMenuItem_Click));
            _modifymenu.DropDownItems.Add(CreateTSMI("FMenuToolsAlti", computeAltiToolStripMenuItem_Click, GetInternetStatus()));

            _mnuContextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem selMenu = CreateTSMI("MNUSelection");
            selMenu.DropDownItems.Add(CreateTSMI("FMenuMarkAsFound", SetManualFoundSelection));
            selMenu.DropDownItems.Add(new ToolStripSeparator());
            selMenu.DropDownItems.Add(CreateTSMI("MNUManualSel", SetManualSelectionToggle));
            selMenu.DropDownItems.Add(CreateTSMI("MNUManualSelAll", SetManualSelectionAll));
            selMenu.DropDownItems.Add(CreateTSMI("MNUManualDeselAll", SetManualDeselectionAll));
            _mnuContextMenu.Items.Add(selMenu);

            _favmenu = CreateTSMI("MNUBookmarks");
            _favmenu.DropDownItems.Add(CreateTSMI("MNUAddToFav", SetAddFav));
            _favmenu.DropDownItems.Add(CreateTSMI("MNURemoveFromFav", SetDelFav));
            _mnuContextMenu.Items.Add(_favmenu);

            _mnuContextMenu.Items.Add(CreateTSMI("MNUAddDelTag", SetAddDelTag));
            _mnuContextMenu.Items.Add(new ToolStripSeparator());
            
            _mnuContextMenu.Items.Add(CreateTSMI("MNUDelCacheFromList", SetRemoveSelectionFromMGM));
            _mnuContextMenu.Items.Add(CreateTSMI("MNUAddToIgnoreList", SetIgnoreSelectionFromMGM));
            TranslateTooltips(_mnuContextMenu, _toolTipForMGM);
        }

        
        
        private void WaypointContextAdd(object sender, EventArgs e)
        {
            List<Geocache> caches = GetSelectedCaches();
            if (caches.Count == 1)
            {
                AddWaypointToCache(caches[0]);
            }
        }

        private void WaypointContextManage(object sender, EventArgs e)
        {
            List<Geocache> caches = GetSelectedCaches();
            if (caches.Count == 1)
            {
            	CloseCacheDetail();
                WaypointsMgr mgr = new WaypointsMgr(this, caches[0]);
                mgr.ShowDialog();
            }
            
        }

        private void SetRemoveSelectionFromMGM(object sender, EventArgs e)
        {
            try
            {
                DialogResult dialogResult = MessageBox.Show(GetTranslator().GetString("AskConfirm"),GetTranslator().GetString("MNUDelCacheFromList"),
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    List<Geocache> caches = GetSelectedCaches();
                    foreach (Geocache geo in caches)
                    {
                        _caches.Remove(geo._Code);
                    }

                    // Cleanup
                    _listViewCaches = new List<EXListViewItem>();
                    lvGeocaches.Items.Clear();
                    EmptywbFastCachePreview();

                    // Now we join Wpts & caches
                    PostTreatmentLoadCache();

                    BuildListViewCache();
                    _bUseFilter = false; // DON'T FORGET TO RESET THE FILTER !
                    PopulateListViewCache(null);
                }
            }
            catch (Exception ex)
            {
                ShowException("", "", ex);
            }
            
        }

        private void SetIgnoreSelectionFromMGM(object sender, EventArgs e)
        {
            try
            {
                DialogResult dialogResult = MessageBox.Show(GetTranslator().GetString("AskConfirm"), GetTranslator().GetString("MNUAddToIgnoreList"),
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {

                    // On ajoute les caches dans l'ignore list
                    // 2 fichiers :
                    // Un fichier avec uniquement les GC code pour un accés ultra rapide
                    // Un fichier données avec toutes les infos
                    Log("_ignorefile = " + _ignorefile);
                    System.IO.StreamWriter file = new System.IO.StreamWriter(_ignorefile, true, Encoding.Default);

                    // on supprime les caches
                    List<Geocache> caches = GetSelectedCaches();
                    foreach (Geocache geo in caches)
                    {
                        _caches.Remove(geo._Code);

                        // Et on se souvient du code
                        String line = geo._Code + ";" +
                            geo._Name.Replace(";", ",") + ";" +
                            geo._Type + ";" +
                            geo._Container + ";" +
                            geo._D + ";" +
                            geo._T;
                        file.WriteLine(line);

                        // Et on met à jour la liste de MGM
                        MiniGeocache mini = new MiniGeocache();
                        mini._Code = geo._Code;
                        mini._Name = geo._Name;
                        mini._Type = geo._Type;
                        mini._Container = geo._Container;
                        mini._D = geo._D;
                        mini._T = geo._T;
                        _ignoreList.Add(mini._Code, mini);
                    }
                    file.Close();


                    // Cleanup
                    _listViewCaches = new List<EXListViewItem>();
                    lvGeocaches.Items.Clear();
                    EmptywbFastCachePreview();

                    // Now we join Wpts & caches
                    PostTreatmentLoadCache();

                    BuildListViewCache();
                    _bUseFilter = false; // DON'T FORGET TO RESET THE FILTER !
                    PopulateListViewCache(null);
                }

            }
            catch (Exception ex)
            {
                ShowException("", "", ex);
            }

        }

        private void ChangeLanguage()
        {
            String forceLocale = ConfigurationManager.AppSettings["forceLocale"];
            if (forceLocale != "")
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(forceLocale);
            }
            else
            {
                Thread.CurrentThread.CurrentCulture = defc1;
                Thread.CurrentThread.CurrentUICulture = defc2;
            }
            HandleDefaultLanguageForLocales();
            _TranslationManager = GetTranslatorForCurrentLocale();
            _TooltipsTranslationManager = GetTooltipsTranslatorForCurrentLocale();
            _toolTipForMGM.RemoveAll();
            TranslateForm();
            InitContextMenu();
            _cacheDetail.TranslateForm();
            int i = GetFooFilterindex();
            if (i == -1)
                cbFilterList.Items.Add(CreateFooFilter());
            else
                cbFilterList.Items[i] = CreateFooFilter();

            i = GetFooFilterMultipleindex();
            if (i == -1)
                clbMltipleFilters.Items.Add(CreateFooFilter("CBCurrentFilter"));
            else
                clbMltipleFilters.Items[i] = CreateFooFilter("CBCurrentFilter");
            

            comboBoxCountry.Items[0] = GetTranslator().GetString("LblAnyCountry");
            comboBoxState.Items[0] = GetTranslator().GetString("LblAnyState");
        }

        private int GetFooFilterindex()
        {
            for (int i = 0; i < cbFilterList.Items.Count; i++ )
            {
                CacheFilter fil = (CacheFilter)(cbFilterList.Items[i]);
                if ((fil != null) && (fil._bToIgnore == true)) // That's the place holder
                    return i;
            }
            return -1;
        }

        private int GetFooFilterMultipleindex()
        {
            for (int i = 0; i < clbMltipleFilters.Items.Count; i++)
            {
                CacheFilter fil = (CacheFilter)(clbMltipleFilters.Items[i]);
                if ((fil != null) && (fil._bToIgnore == true)) // That's the place holder
                    return i;
            }
            return -1;
        }


        private void TranslateListView()
        {
            foreach (KeyValuePair<int, String> paire in _dicoColumns)
            {
                lvGeocaches.Columns[paire.Key].Text = GetTranslator().GetString(paire.Value);
                displayToolStripMenuItem.DropDownItems[2 + paire.Key].Text = GetTranslator().GetString(paire.Value);
            }
        }

        // Je pense que ce truc n'est plus utilisé...
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            CheckUpdate(false, false);
        }

        /// <summary>
        /// Handler to process key pressed
        /// Deals with CTRL+A to select all caches
        /// </summary>
        /// <param name="msg">message</param>
        /// <param name="keyData">key data</param>
        /// <returns>true</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.A))
            {
                foreach (ListViewItem item in lvGeocaches.Items)
                {
                    item.Selected = true;
                }
                lvGeocaches.Focus();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void UpdateMenuChecks()
        {
            String forceLocale = ConfigurationManager.AppSettings["forceLocale"];
            String keyLng = Thread.CurrentThread.CurrentUICulture.Name;
            foreach (ToolStripMenuItem mnu in languageToolStripMenuItem.DropDownItems)
            {
                if ((forceLocale != "") && ((String)(mnu.Tag) == keyLng))
                    mnu.Checked = true;
                else
                    mnu.Checked = false;
            }
            if (forceLocale == "")
                defaultsystemToolStripMenuItem.Checked = true;


            if (ConfigurationManager.AppSettings["openGeocachingEmbedded"] == "False")
                openExternalURLInDefaultBrowserToolStripMenuItem.Checked = true;
            else
                openExternalURLInDefaultBrowserToolStripMenuItem.Checked = false;

            if (ConfigurationManager.AppSettings["openCacheEmbedded"] == "False")
                openCacheDetailInDefaultBrowerToolStripMenuItem.Checked = true;
            else
                openCacheDetailInDefaultBrowerToolStripMenuItem.Checked = false;
            
            if (ConfigurationManager.AppSettings["ignorefounds"] == "True")
                ignoreFoundCachesToolStripMenuItem.Checked = true;
            else
                ignoreFoundCachesToolStripMenuItem.Checked = false;

            if (ConfigurationManager.AppSettings["autocheckupdate"] == "True")
                automaticallyCheckForUpdatesToolStripMenuItem.Checked = true;
            else
                automaticallyCheckForUpdatesToolStripMenuItem.Checked = false;

            if (ConfigurationManager.AppSettings["enabletooltips"] == "True")
            {
            	fMenuDisableListTooltipToolStripMenuItem.Enabled = true;
                fMenuEnableTooltipsToolStripMenuItem.Checked = true;
            }
            else
            {
            	fMenuDisableListTooltipToolStripMenuItem.Enabled = false;
                fMenuEnableTooltipsToolStripMenuItem.Checked = false;
            }
            
            if (ConfigurationManager.AppSettings["disabletooltipsformainlist"] == "True")
                fMenuDisableListTooltipToolStripMenuItem.Checked = true;
            else
                fMenuDisableListTooltipToolStripMenuItem.Checked = false;
            
            if (_bUseKm)
                useMilToolStripMenuItem.Checked = true;
            else
                useMilToolStripMenuItem.Checked = false;

            if (ConfigurationManager.AppSettings["displaytabmap"] == "True")
                displayTabForQuickCacheMapToolStripMenuItem.Checked = true;
            else
                displayTabForQuickCacheMapToolStripMenuItem.Checked = false;

            if (_bUseGCPopularity)
                popularityGCToolStripMenuItem.Checked = true;
            else
                popularityGCToolStripMenuItem.Checked = false;

            if (ConfigurationManager.AppSettings["markmissingcaches"] == "True")
                markMissingCachesToolStripMenuItem.Checked = true;
            else
                markMissingCachesToolStripMenuItem.Checked = false;
        }

        private void EnableDisableSpecialFeatures()
        {
        	bool special = SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled();
        	foreach(ToolStripItem item in _menuEntriesRequiringSpecialFeatures)
        	{
        		item.Visible = special;
                CustomeStyleOnGoldItems(item);// BCR 20170825
            }
        }

        private void CustomeStyleOnGoldItems(ToolStripItem item) // BCR 20170825
        {
            item.Font = MyTools.ChangeFontStyle(item.Font, true, true);
        }

        private void AssignToolStripMenuItemIcon(ToolStripMenuItem item, bool bUseDefaultIcon, String keyOfNonDefaultIconImage)
        {
        	if (bUseDefaultIcon)
        	{
        		// On rétablit l'icone d'origine
				if ((_imgMenus != null) && (_imgMenus.ContainsKey(item.Name)))
                	item.Image = _imgMenus[item.Name];
				else
					item.Image = null;
        	}
        	else
        	{
        		// On applique l'icone de remplacement
        		item.Image = GetImageSized(keyOfNonDefaultIconImage);
        	}
        }
        
        private bool IsParentMenuDisableOrInvisible(ToolStripMenuItem item)
        {
        	if (item == null)
        	{
        		return false;
        	}
        	else
        	{
        		// si on est soit invisible, soit désactivé, on retourne true
        		if (!item.Enabled)// || !item.Visible)
        			return true;
        		
        		// On vérifie le père
        		ToolStripMenuItem dad = item.OwnerItem as ToolStripMenuItem;
        		return IsParentMenuDisableOrInvisible(dad);
        	}
        }
        
        private void EnableDisableToolbar()
        {
        	// si on est enable et visible, alors on enable le toolbar button
        	// MAIS !!! Si un de nos parent est disabled ou invisible, alors on se disable aussi
        	// cas du menu sélection qui est masqué mais pas ses fils
        	if (_shortcutToolstrip != null)
        	{
        		foreach(var o in _shortcutToolstrip.Items)
        		{
        			ToolStripButton titem = o as ToolStripButton;
        			if (titem != null)
        			{
        				String tag = titem.Tag as String;
        				if (!String.IsNullOrEmpty(tag))
        				{
        					// On cherche l'item dans le menu
        					ToolStripMenuItem menu = MyTools.FindControl(this, tag) as ToolStripMenuItem;
				        	if (menu != null)
				        	{
				        		// Ok, on a le menu
				        		bool disable = IsParentMenuDisableOrInvisible(menu);
				        		titem.Enabled = !disable;
				        	}
        				}
        			}
        		}
        	}
        }
        
        
        private void EnableDisableMenuEntries(List<Tuple<ToolStripMenuItem, bool>> items, bool enable, String keyOfDisabledIconImage)
        {
        	if (items != null)
        	{
	        	foreach(Tuple<ToolStripMenuItem, bool> tple in items)
	        	{
	        		if (tple.Item2 && (!GetInternetStatus())) // On a besoin d'internet et c'est pas dispo
	        		{
	        			tple.Item1.Enabled = false; // on force à désactivé
	        			AssignToolStripMenuItemIcon(tple.Item1, false, "InternetNo");
	        		}
	        		else
	        		{
	        			// Soit on n'a pas besoin d'internet, soit on en a besoin mais c'est dispo
	        			// bref, on fait ce qu'on nous dit, pas de contre indication
	        			tple.Item1.Enabled = enable;
	        			AssignToolStripMenuItemIcon(tple.Item1, enable, keyOfDisabledIconImage);
	        		}
	        		
	        	}
        	}
        }
        
        private void EnableDisableMenuEntriesBasedOnInternet(ToolStripMenuItem[] items, bool enable)
        {
        	if (items != null)
        	{
        		bool special = SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled();
        		
	        	foreach(ToolStripMenuItem item in items)
	        	{
	        		if (_menuEntriesRequiringSpecialFeatures.Contains(item))
	        		{
	        			// Attention, spécial feature
	        			if (special)
	        			{
	        				item.Visible = true;
                            CustomeStyleOnGoldItems(item);// BCR 20170825
                            item.Enabled = enable;
	        				
	        				// On rétablit l'icone d'origine
	        				AssignToolStripMenuItemIcon(item, enable, "InternetNo");
	        			}
	        			else
	        			{
	        				item.Visible = false;
	        			}
	        		}
	        		else // une fonction simple
	        		{
	        			item.Enabled = enable;
	        			AssignToolStripMenuItemIcon(item, enable, "InternetNo");
	        		}
	        		
	        	}
        	}
        }
        
        private void TranslateForm()
        {
            
        	SelectedCachesToolStripMenuItem.Text = GetTranslator().GetString("SelectedCachesToolStripMenuItem");
			SCDisplayToolStripMenuItem.Text = GetTranslator().GetString("SCDisplayToolStripMenuItem");
			SCDisplayOnMapToolStripMenuItem.Text = GetTranslator().GetString("SCDisplayOnMapToolStripMenuItem");
			SCDisplayDetailsToolStripMenuItem.Text = GetTranslator().GetString("SCDisplayDetailsToolStripMenuItem");
			SCDisplayOnGCToolStripMenuItem.Text = GetTranslator().GetString("SCDisplayOnGCToolStripMenuItem");
			SCDisplayModifToolStripMenuItem.Text = GetTranslator().GetString("SCDisplayModifToolStripMenuItem");
			OfflineCachesToolStripMenuItem.Text = GetTranslator().GetString("OfflineCachesToolStripMenuItem");
			OCWriteNoteToolStripMenuItem.Text = GetTranslator().GetString("OCWriteNoteToolStripMenuItem");
			OCViewCacheToolStripMenuItem.Text = GetTranslator().GetString("OCViewCacheToolStripMenuItem");
			OCDownloadToolStripMenuItem.Text = GetTranslator().GetString("OCDownloadToolStripMenuItem");
			geoFranceToolStripMenuItem.Text = GetTranslator().GetString("geoFranceToolStripMenuItem");
			OCRemoveAllToolStripMenuItem.Text = GetTranslator().GetString("OCRemoveAllToolStripMenuItem");
			OCDisplayAllToolStripMenuItem.Text = GetTranslator().GetString("OCDisplayAllToolStripMenuItem");
			FilterNearToolStripMenuItem.Text = GetTranslator().GetString("FilterNearToolStripMenuItem");
			FLNUseAsCenterToolStripMenuItem.Text = GetTranslator().GetString("FLNUseAsCenterToolStripMenuItem");
			FLNDisplayLessThanToolStripMenuItem.Text = GetTranslator().GetString("FLNDisplayLessThanToolStripMenuItem");
			String unit = "";
            if (_bUseKm)
            {
                unit = GetTranslator().GetString("LVKm");
            }
            else
            {
                unit = GetTranslator().GetString("LVMi");
            }
            FLNDisplayLessThanToolStripMenuItem5.Text = "5 " + unit;
            FLNDisplayLessThanToolStripMenuItem10.Text = "10 " + unit;
            FLNDisplayLessThanToolStripMenuItem20.Text = "20 " + unit;
            FLNDisplayLessThanToolStripMenuItem50.Text = "50 " + unit;
            FLNDisplayLessThanToolStripMenuItem100.Text = "100 " + unit;
            
				
			WaypointsToolStripMenuItem.Text = GetTranslator().GetString("WaypointsToolStripMenuItem");
			WPAddToolStripMenuItem.Text = GetTranslator().GetString("WPAddToolStripMenuItem");
			WPManageToolStripMenuItem.Text = GetTranslator().GetString("WPManageToolStripMenuItem");
			MarkSelectionToolStripMenuItem.Text = GetTranslator().GetString("MarkSelectionToolStripMenuItem");
			MSAsFoundToolStripMenuItem.Text = GetTranslator().GetString("MSAsFoundToolStripMenuItem");
			MSToggleToolStripMenuItem.Text = GetTranslator().GetString("MSToggleToolStripMenuItem");
			MSMarkAllToolStripMenuItem.Text = GetTranslator().GetString("MSMarkAllToolStripMenuItem");
			MSUnmarkAllToolStripMenuItem.Text = GetTranslator().GetString("MSUnmarkAllToolStripMenuItem");
			FavSelToolStripMenuItem.Text = GetTranslator().GetString("FavSelToolStripMenuItem");
			FSAddToolStripMenuItem.Text = GetTranslator().GetString("FSAddToolStripMenuItem");
			FsDelToolStripMenuItem.Text = GetTranslator().GetString("FsDelToolStripMenuItem");
			AddMLabelSelToolStripMenuItem.Text = GetTranslator().GetString("AddMLabelSelToolStripMenuItem");
			RemoveSelCachesToolStripMenuItem.Text = GetTranslator().GetString("RemoveSelCachesToolStripMenuItem");
			IgnoreSelCachesToolStripMenuItem.Text = GetTranslator().GetString("IgnoreSelCachesToolStripMenuItem");


        	
        	
        	filteronsqldbToolStripMenuItem.Text = GetTranslator().GetString("filteronsqldbToolStripMenuItem");
        	filterOnCacheCacheToolStripMenuItem.Text = GetTranslator().GetString("filterOnCacheCacheToolStripMenuItem");
        	kmlMaintenanceToolStripMenuItem.Text = GetTranslator().GetString("kmlMaintenanceToolStripMenuItem");
        	completeFromCacheCacheFullToolStripMenuItem.Text = GetTranslator().GetString("completeFromCacheCacheFullToolStripMenuItem");
        	filterOnCountryToolStripMenuItem.Text = GetTranslator().GetString("filterOnCountryToolStripMenuItem");
        	filterOnFrenchRegionToolStripMenuItem.Text = GetTranslator().GetString("filterOnFrenchRegionToolStripMenuItem");
        	filterOnFrenchDepartmentToolStripMenuItem.Text = GetTranslator().GetString("filterOnFrenchDepartmentToolStripMenuItem");
        	filterOnFrenchCityToolStripMenuItem.Text = GetTranslator().GetString("filterOnFrenchCityToolStripMenuItem");
        	
        	profilesToolStripMenuItem.Text = GetTranslator().GetString("FMenuProfiles");
        	createModifyProfileToolStripMenuItem.Text = GetTranslator().GetString("FMenuCreateNewProfile");
        	
            createMyFindsToolStripMenuItem.Text = GetTranslator().GetString("FMenuMyFinds");
            generateSQLdb.Text = GetTranslator().GetString("generateSQLdb");
            manageIgnoreListToolStripMenuItem.Text = GetTranslator().GetString("IgnoreCacheManager");

            // Maintenance
            diagnosticsToolStripMenuItem.Text = GetTranslator().GetString("FMenuDiagnostics");
            modifyconfigurationToolStripMenuItem.Text = GetTranslator().GetString("FMenuChangeConfigFile");

            lblTipEnlargeAreaWebBrowser.Text = GetTranslator().GetString("LblEnlargeAreFoCacheDetail");
            EmptywbFastCachePreview();

            checkGCaccountToolStripMenuItem.Text = GetTranslator().GetString("FMenuCheckGCAccount");
            configGCaccountinfoToolStripMenuItem.Text = GetTranslator().GetString("FMenuConfigureGC");
            
            pluginscodeToolStripMenuItem.Text = GetTranslator().GetString("FMenuDisplayPluginsElements");

            computeAltiToolStripMenuItem.Text = GetTranslator().GetString("FMenuToolsAlti");
            
            donateToolStripMenuItem.Text = GetTranslator().GetString("FMenuDonate");
            downloadbetaToolStripMenuItem.Text = GetTranslator().GetString("FMenuDownloadBeta");
            downloadPluginsToolStripMenuItem.Text = GetTranslator().GetString("FMenuDownloadPlugins");
            
            testpluginToolStripMenuItem.Text = GetTranslator().GetString("FMenuTestPlugin");

            markMissingCachesToolStripMenuItem.Text = GetTranslator().GetString("FMenuConfigMarkMissingCaches");
            numberLastLogsSymbolsToolStripMenuItem.Text = GetTranslator().GetString("FMenuLastLogSymbols");
            importOV2ToolStripMenuItem.Text = GetTranslator().GetString("FMenuImportOV2SuperPP");
            popularityGCToolStripMenuItem.Text = GetTranslator().GetString("FMenuPopularityGCInsteadOfWilson");
            importExportStatsToolStripMenuItem.Text = GetTranslator().GetString("FMenuIOCacheStats");
            exportCacheStatsToolStripMenuItem.Text = GetTranslator().GetString("FMenuOCacheStats");
            importCacheStatsToolStripMenuItem.Text = GetTranslator().GetString("FMenuICacheStats");
            forceUpdateToolStripMenuItem.Text = GetTranslator().GetString("FMenuForceUpdate");
            
            
            manualUpdateToolStripMenuItem.Text = GetTranslator().GetString("FMenuManualUpdate");
            radioButton1txtfilter.Text = GetTranslator().GetString("LblName");
            radioButton2txtfilter.Text = GetTranslator().GetString("LblOwner");
            radioButton3txtfilter.Text = GetTranslator().GetString("LblCode");
            rbFilterOnTagtxtfilter.Text = GetTranslator().GetString("LblTag");
            rdBtnAnd.Text = GetTranslator().GetString("LblAND");
            rdBtnOR.Text = GetTranslator().GetString("LblOR");
            radioButton8AttsIn.Text = GetTranslator().GetString("LblAllOfThem");
            radioButton9AttsIn.Text = GetTranslator().GetString("LblAtLeastOne");
            radioButton11AttsOut.Text = GetTranslator().GetString("LblAllOfThemOut");
            radioButton10AttsOut.Text = GetTranslator().GetString("LblAtLeastOneOut");

            groupBox6.Text = GetTranslator().GetString("GrpAttsPermissions");
            groupBox7.Text = GetTranslator().GetString("GrpAttsEquipments");
            groupBox8.Text = GetTranslator().GetString("GrpAttsConditions");
            groupBox10.Text = GetTranslator().GetString("GrpAttsHazards");
            groupBox9.Text = GetTranslator().GetString("GrpAttsFacilities");
            groupBox11.Text = GetTranslator().GetString("GrpAttsSpecials");
            groupBox17.Text = GetTranslator().GetString("GrpAttsPermissions");
            groupBox16.Text = GetTranslator().GetString("GrpAttsEquipments");
            groupBox13.Text = GetTranslator().GetString("GrpAttsConditions");
            groupBox14.Text = GetTranslator().GetString("GrpAttsHazards");
            groupBox15.Text = GetTranslator().GetString("GrpAttsFacilities");
            groupBox12.Text = GetTranslator().GetString("GrpAttsSpecials");

            existingDBToolStripMenuItem.Text = GetTranslator().GetString("FMenuExistingDB");
            convertCoordToolStripMenuItem.Text = GetTranslator().GetString("FMenuToolsConverter");
            displayNonTranslatedCachesToolStripMenuItem.Text = GetTranslator().GetString("FMenuDispNonTranslated");
            translationToolStripMenuItem.Text = GetTranslator().GetString("FMenuTranslation");
            

            
            // GeoInt menu
            itineraryToolStripMenuItem.Text = GetTranslator().GetString("FMenuGeoInt");
            displayAPathConnectingAllDisplayedCachesToolStripMenuItem.Text = GetTranslator().GetString("FMenuDisplayTVPFunction");
            displayDirectPathToolStripMenuItem.Text = GetTranslator().GetString("FMenuDisplayDirectFunction");
            identifyClustersToolStripMenuItem.Text = GetTranslator().GetString("FMenuCluster");
            clearmarkersToolStripMenuItem.Text = GetTranslator().GetString("FMenuGeoIntClean");


            completeCachesToolStripMenuItem.Text = GetTranslator().GetString("FMenuToolsModCaches");
            modifyNameToolStripMenuItem.Text = GetTranslator().GetString("FMenuToolsModCachesName");
            modifyNameReplaceToolStripMenuItem.Text = GetTranslator().GetString("FMenuToolsModCachesNameReplace");
            modifyNameWithTagToolStripMenuItem.Text = GetTranslator().GetString("FMenuToolsModCachesTag");
            modifyCoordToolStripMenuItem.Text = GetTranslator().GetString("FMenuModifyManualCoord");

            displayCachesWithoutStatsToolStripMenuItem.Text = GetTranslator().GetString("FMenuCacheWithoutStats");
            specificFiltersToolStripMenuItem.Text = GetTranslator().GetString("FMenuSpecific");

            // Geocaching.com
            liveToolStripMenuItem.Text = GetTranslator().GetString("FMenuLive");
            usersInformationToolStripMenuItem.Text = GetTranslator().GetString("FMenuLiveUInfo");
            logselcachesToolStripMenuItem.Text = GetTranslator().GetString("FMenuLiveLogCache");
            loadfieldnotesToolStripMenuItem.Text = GetTranslator().GetString("FMenuLiveLoadFieldNotes");
            getUpdatedCoordToolStripMenuItem.Text = GetTranslator().GetString("FMenuLiveUpdateCoord");
            setUpdatedCoordToolStripMenuItem.Text = GetTranslator().GetString("setUpdatedCoordToolStripMenuItem");
            completeCacheDescToolStripMenuItem.Text = GetTranslator().GetString("FMenuCompleteDesc");
            updateCachesItemsToolStripMenuItem.Text = GetTranslator().GetString("FMenuCompleteItemsFromCache");
            getFoundinfoToolStripMenuItem.Text = GetTranslator().GetString("FMenuRetrieveFoundInformations");
			displayOurDNFToolStripMenuItem.Text = GetTranslator().GetString("FMenuDisplayOnlyOurDNF");
            updateStatsToolStripMenuItem.Text = GetTranslator().GetString("FMenuLiveDownloadCachesStats");
            DownloadNotesFromGCToolStripMenuItem.Text = GetTranslator().GetString("DownloadNotesFromGCToolStripMenuItem");
            UploadNotesToGCToolStripMenuItem.Text = GetTranslator().GetString("UploadNotesToGCToolStripMenuItem");
            PQToolStripMenuItem.Text = GetTranslator().GetString("PQToolStripMenuItem");
            createPQToolStripMenuItem.Text = GetTranslator().GetString("createPQToolStripMenuItem");
            createPQDateToolStripMenuItem.Text = GetTranslator().GetString("createPQDateToolStripMenuItem");
            checkPQToolStripMenuItem.Text = GetTranslator().GetString("checkPQToolStripMenuItem");
            autodownloadPQToolStripMenuItem.Text = GetTranslator().GetString("autodownloadPQToolStripMenuItem");
            downloadPQToolStripMenuItem.Text = GetTranslator().GetString("FMenuLiveDownloadPQ");
            createpublishnotifications.Text = GetTranslator().GetString("createpublishnotifications");
            createpublishnotificationsext.Text = GetTranslator().GetString("createpublishnotificationsext");
            managepublishnotifications.Text = GetTranslator().GetString("managepublishnotifications");
            menupublishnotifications.Text = GetTranslator().GetString("menupublishnotifications");
            fmenugeocachingbasic.Text = GetTranslator().GetString("fmenugeocachingbasic");
            fmenugeocachingpremium.Text = GetTranslator().GetString("fmenugeocachingpremium");
            fmenugeocachingbasicpremium.Text = GetTranslator().GetString("fmenugeocachingbasicpremium");
            downloadSpoilersAuthenticatedToolStripMenuItem.Text = GetTranslator().GetString("downloadSpoilersAuthenticatedToolStripMenuItem");
            
            configureProxyToolStripMenuItem.Text = GetTranslator().GetString("FMenuConfigProxy");
            configureStartupLoadToolStripMenuItem.Text = GetTranslator().GetString("FMenuConfigurationLoadStartup");
            displayDTMatrixToolStripMenuItem.Text = GetTranslator().GetString("FMenuToolsDTMatrix");
            if (_pluginToolStripMenuItem != null)
                _pluginToolStripMenuItem.Text = GetTranslator().GetString("FMenuPlugins");
            displayGPXTrackOnMapToolStripMenuItem.Text = GetTranslator().GetString("FMenuDisplayGPXTrack");
            
            exportSelectedToolStripMenuItem.Text = GetTranslator().GetString("FMenuExportSel");
            exportImagesFromSelectedToolStripMenuItem.Text = GetTranslator().GetString("FMenuExportImagesSel");

            dispCachesToolStripMenuItem.Text = GetTranslator().GetString("FMenuDisplayedCaches");
            selCachesToolStripMenuItem.Text = GetTranslator().GetString("FMenuSelectedCaches");

            configureBookmarksToolStripMenuItem.Text = GetTranslator().GetString("FMenuConfigBookmarks");
            configureSpoilersDownloadToolStripMenuItem.Text = GetTranslator().GetString("DlgSpoilerKeywords");
            toolsToolStripMenuItem.Text = GetTranslator().GetString("FMenuTools");
            
            hideAllColumnsToolStripMenuItem.Text = GetTranslator().GetString("FMenuHideAllCol");
            displayAllColumnsToolStripMenuItem.Text = GetTranslator().GetString("FMenuShowAllCol");

            fillAccountKeyTranslationToolStripMenuItem.Text = GetTranslator().GetString("FMenuConfigurationTranslate");
            translateSelectedCachesToolStripMenuItem.Text = GetTranslator().GetString("FMenuTranslate");

            deleOfflineDataForMissingToolStripMenuItem.Text = GetTranslator().GetString("MNUDelOfflineDataForMissingCaches");
            performOfflineDataMaintenanceToolStripMenuItem.Text = GetTranslator().GetString("MNUPerformOfflineMaintenance");
            maintenanceToolStripMenuItem.Text = GetTranslator().GetString("FMenuMaintenance");
            
            // Filtres spécifiques
            displayCachesWithoutOfflineDataToolStripMenuItem.Text = GetTranslator().GetString("FMenuCacheWithoutOffdata");
            displayCachesWithoutADescriptionToolStripMenuItem.Text = GetTranslator().GetString("FMenuCacheWithoutDesc");
            displayCachesWithModificationsToolStripMenuItem.Text = GetTranslator().GetString("FMenuCacheWithModifications");
            displayCachesWithSpoilersToolStripMenuItem.Text = GetTranslator().GetString("FMenuCachesWithSpoilers");
            displayCachesWithGPSSpoilersToolStripMenuItem.Text = GetTranslator().GetString("displayCachesWithGPSSpoilersToolStripMenuItem");
            displayFranceCoverageToolStripMenuItem.Text = GetTranslator().GetString("displayFranceCoverageToolStripMenuItem");
            generateFranceCoverageToolStripMenuItem.Text = GetTranslator().GetString("generateFranceCoverageToolStripMenuItem");
            displayWorldCoverageToolStripMenuItem.Text = GetTranslator().GetString("displayWorldCoverageToolStripMenuItem");
            AnimateFindsToolStripMenuItem.Text = GetTranslator().GetString("AnimateFindsToolStripMenuItem");
            excludeMissingCachesToolStripMenuItem.Text = GetTranslator().GetString("FMenuToolsExcludeMissingCaches");
            filterAltitudeToolStripMenuItem.Text = GetTranslator().GetString("FMenuFltAltitude");
            excludeSelectedCachesToolStripMenuItem.Text = GetTranslator().GetString("FMenuExcludeSelectedCaches");

            displayTabForQuickCacheMapToolStripMenuItem.Text = GetTranslator().GetString("FMenuConfigTabMap");
			configureToolbarToolStripMenuItem.Text = GetTranslator().GetString("configureToolbarToolStripMenuItem");

            displayToolStripMenuItem.Text = GetTranslator().GetString("FMenuDisplay");
            loadGPXFilesToolStripMenuItem.Text = GetTranslator().GetString("FMenuLoadGPX");
            configurationToolStripMenuItem.Text = GetTranslator().GetString("FMenuconfiguration");
            languageToolStripMenuItem.Text = GetTranslator().GetString("FMenuLanguage");
            defaultsystemToolStripMenuItem.Text = GetTranslator().GetString("FMenuDefault");
            openExternalURLInDefaultBrowserToolStripMenuItem.Text = GetTranslator().GetString("FMenuOpenExternalBrowser");
            openCacheDetailInDefaultBrowerToolStripMenuItem.Text = GetTranslator().GetString("FMenuOpenDetailsExternalBrowser");
            ignoreFoundCachesToolStripMenuItem.Text = GetTranslator().GetString("FMenuIgnoreFound");
            changeUserNameToolStripMenuItem.Text = GetTranslator().GetString("FMenuChangeUser");
            changeHomeLocationToolStripMenuItem.Text = GetTranslator().GetString("FMenuchangeHome");
            changeAgeForNewCacheToolStripMenuItem.Text = GetTranslator().GetString("FMenuAgeForNew");
            useMilToolStripMenuItem.Text = GetTranslator().GetString("FMenuUseKm");
            UpdateMenuChecks();
            aboutToolStripMenuItem.Text = GetTranslator().GetString("FMenuAbout");
            aboutToolStripMenuItem1.Text = GetTranslator().GetString("FMenuAbout");
            seechangelogToolStripMenuItem.Text = GetTranslator().GetString("FMenuSeeHistory");
            checkUpdatesToolStripMenuItem.Text = GetTranslator().GetString("CheckUpdate");
            automaticallyCheckForUpdatesToolStripMenuItem.Text = GetTranslator().GetString("AutoCheckUpdate");
            fMenuEnableTooltipsToolStripMenuItem.Text = GetTranslator().GetString("FMenuEnableTooltips");
            fMenuDisableListTooltipToolStripMenuItem.Text = GetTranslator().GetString("FMenuDisableTooltipsForMainList");
            	
            menuStrip1.Items[0].Text = GetTranslator().GetString("FMenuFile");
            exportCurrentDisplayToolStripMenuItem.Text = GetTranslator().GetString("FMenuExport");
            exportImagesToolStripMenuItem.Text = GetTranslator().GetString("FMenuExportImages");
            closeToolStripMenuItem.Text = GetTranslator().GetString("FMenuClose");
            groupBox1.Text = GetTranslator().GetString("FFilterGroup");
            label1.Text = GetTranslator().GetString("FNameContains");
            btnSearch.Text = GetTranslator().GetString("FFilter");
            
            radioButton4displayspec.Text = GetTranslator().GetString("FDisplayAll");
            radioButton5displayspec.Text = GetTranslator().GetString("FDisplaySel");
            radioButton7displayspec.Text = GetTranslator().GetString("FDisplayFav");
            button4displayspec.Text = GetTranslator().GetString("BtnDisplay");

            btnCheckSize.Text = GetTranslator().GetString("BtnCheckAll");
            btnCheckType.Text = GetTranslator().GetString("BtnCheckAll");
            btnUncheckSize.Text = GetTranslator().GetString("BtnUncheckAll");
            btnUncheckType.Text = GetTranslator().GetString("BtnUncheckAll");
            
            // Area
            btnDefineArea.Text = GetTranslator().GetString("BtnDefineArea");
            cbFilterArea.Text = GetTranslator().GetString("FCheckFilterArea");
            groupBoxArea.Text = GetTranslator().GetString("FGBArea");
                        
			// Fav / Pop
			gbFavorites.Text = GetTranslator().GetString("gbFavorites");
			gbPopularity.Text = GetTranslator().GetString("gbPopularity");
			cbFilterFavorites.Text = GetTranslator().GetString("cbFilterFavorites");
			cbFilterPopularity.Text = GetTranslator().GetString("cbFilterPopularity");
			 
            cbFilterSize.Text = GetTranslator().GetString("FCheckFilterSize");
            cbFilterType.Text = GetTranslator().GetString("FCheckFilterType");
            cbFilterDistance.Text = GetTranslator().GetString("FCheckFilterDistance");
            cbFilterStatus.Text = GetTranslator().GetString("FCheckFilterStatus");
            cbFilterDifficulty.Text = GetTranslator().GetString("FCheckFilterDifficulty");
            cbFilterTerrain.Text = GetTranslator().GetString("FCheckFilterTerrain");
            cbFilterOwner.Text = GetTranslator().GetString("FCheckFilterUser");
            cbFilterAttributeIn.Text = GetTranslator().GetString("FCheckFilterAttributeIn");
            cbFilterAttributeOut.Text = GetTranslator().GetString("FCheckFilterAttributeOut");

            cbTBGC.Text = GetTranslator().GetString("FCheckFilterTBGC");
            gbOwner.Text = GetTranslator().GetString("GBDisplayIf");
            gbSize.Text = GetTranslator().GetString("FGBSize");
            gbType.Text = GetTranslator().GetString("FGBType");
            gbDistance.Text = GetTranslator().GetString("FGBDistance");
            gbStatus.Text = GetTranslator().GetString("FGBStatus");
            gbDiff.Text = GetTranslator().GetString("FGBDifficulty");
            gbTerr.Text = GetTranslator().GetString("FGBTerrain");

            label2.Text = GetTranslator().GetString("FGBBetween1");
            label3.Text = GetTranslator().GetString("FGBBetween2");
            label9.Text = GetTranslator().GetString("FGBBetween1");
            label8.Text = GetTranslator().GetString("FGBBetween2");
            label7.Text = GetTranslator().GetString("FGBBetween1");
            label6.Text = GetTranslator().GetString("FGBBetween2");
            label11.Text = GetTranslator().GetString("FFilterGroup");
            
            cbAvailable.Text = GetTranslator().GetString("FGBAvailable");
            cbArchived.Text = GetTranslator().GetString("FGBNotArchived");
            cbOwned.Text = GetTranslator().GetString("FGBOwned");
            cbFound.Text = GetTranslator().GetString("FGBFound");

            // Les tabpage
            tabPage1Size.Text = GetTranslator().GetString("TPSize");
            tabPage2Type.Text = GetTranslator().GetString("TPType");
            tabPage3Dist.Text = GetTranslator().GetString("TPHome");
            tabPage4Status.Text = GetTranslator().GetString("TPStatus");
            tabPage5DT.Text = GetTranslator().GetString("TPDT");
            tabPage7User.Text = GetTranslator().GetString("GBDisplayIf");
            tabPage13AttIn.Text = GetTranslator().GetString("TPAttP");
            tabPage14AttOut.Text = GetTranslator().GetString("TPAttM");
            tabPage9TB.Text = GetTranslator().GetString("TPTB");
            tabPage10Near.Text = GetTranslator().GetString("TPNear");
            tabPage16Area.Text = GetTranslator().GetString("TPArea");
            tabPageFavPop.Text = GetTranslator().GetString("TPStats");
           	tabPage12Region.Text = GetTranslator().GetString("TPRegion");
           	tabPage18Date.Text = GetTranslator().GetString("TPDate");
            tabPage11Multi.Text = GetTranslator().GetString("TPFltCombi");
            tabPage15_cachesPreviewMap.Text = GetTranslator().GetString("TPMap");
			            
            UpdateTextExplainMultipleFilter();

            btnMultipleFilters.Text = GetTranslator().GetString("BtnExecuteMultipleFilters");

            label20.Text = GetTranslator().GetString("LblCountry");
            label19.Text = GetTranslator().GetString("LblState");
            cbFilterRegionState.Text = GetTranslator().GetString("FCheckFilterRegion");
            gbRegionState.Text = GetTranslator().GetString("FGBRegion");

            
            cbFilterNear.Text = GetTranslator().GetString("FCheckFilterNear");
            textBox4.Text = GetTranslator().GetString("LblUseCacheCoords");
            groupBox3.Text = GetTranslator().GetString("GroupLocation");
            groupBox2.Text = GetTranslator().GetString("GroupDistLocation");
            label16.Text = GetTranslator().GetString("LblLessThan");
            label18.Text = GetTranslator().GetString("LblLatLon");

            
            cbFilterCreation.Text = GetTranslator().GetString("FCheckFilterDateCreation");
            cbFilterLastLog.Text = GetTranslator().GetString("FCheckFilterDateLog");
            gbDateCreation.Text = GetTranslator().GetString("GrpDateCreation");
            gbLastLog.Text = GetTranslator().GetString("GrpDateLog");
            lblDays1.Text = GetTranslator().GetString("LblDays");
            lblDays2.Text = GetTranslator().GetString("LblDays");

            // Prevent the filter from being checked in the beginning
            cbFilterAttributeIn.Checked = false;
            cbFilterAttributeOut.Checked = false;
            cbFilterNear.Checked = false;
            cbFilterRegionState.Checked = false;
            cbFilterArea.Checked = false;
            cbFilterCreation.Checked = false;
            cbFilterLastLog.Checked = false;

            TranslateKmMi();
            UpdateCheckBoxStatus(false);
            UpdateCheckBoxOwner(false);
            TranslateTooltips(this, _toolTipForMGM);
            
            // On restaure la toolbar ?
            ToolbarConfiguration.CreateToolbar(this);
            
            // Update menus
            RefreshMenuEnableStatus();
            
            // On vire le tooltip de lvGeocache si demandé
            if (ConfigurationManager.AppSettings["disabletooltipsformainlist"] == "True")
            {
	            if (_toolTipForMGM != null)
	            {
	            	_toolTipForMGM.SetToolTip(lvGeocaches, null);
	            }
            }
            
            TranslateListView();

        }

        /// <summary>
        /// 
        /// </summary>
        public void RefreshMenuEnableStatus()
        {
        	// Internet status update !
            bool bActif = GetInternetStatus();
            // Les entrées pure internet doivent être désactivées
            EnableDisableMenuEntriesBasedOnInternet(_menuEntriesRequiringInternet, bActif);
            // Les entrées dépendant d'internet doivent aussi être mise à jour si besoin
           	EnableDisableMenuEntries(_menuEntriesRequiringDisplayedCaches, (_iNbCachesDisplayedInListview != 0), "DisplayedNo");
            EnableDisableMenuEntries(_menuEntriesRequiringSelectedCaches, (_iNbCachesSelectedInListview !=0), "SelectedNo");
            EnableDisableMenuEntries(_menuEntriesRequiringOnlyOneSelectedCaches, (_iNbCachesSelectedInListview == 1), "SelectedOneNo");
            // Les fonctions spéciales
            EnableDisableSpecialFeatures();
            // La toolbar
            EnableDisableToolbar();
        }
        
        private void EmptywbFastCachePreview()
        {
            wbFastCachePreview.DocumentText = GetTranslator().GetString("HtmlDefaultNoCacheSelected");
        }


        private void UpdateTextExplainMultipleFilter()
        {
            if (rdBtnOR.Checked)
                textBoxMultipleFilters.Text = GetTranslator().GetString("LblMultipleFiltersExplainOR");
            else
                textBoxMultipleFilters.Text = GetTranslator().GetString("LblMultipleFiltersExplainAND");
        }

        void TranslateKmMi()
        {
            if (_bUseKm)
            {
                //lstv.Columns[7].Text = GetTranslator().GetString("LVKm");
                label4.Text = GetTranslator().GetString("LVKm");
                label5.Text = GetTranslator().GetString("LVKm");
                label14.Text = GetTranslator().GetString("LVKm");
                TranslateFilterNearKmMi();
            }
            else
            {
                //lstv.Columns[7].Text = GetTranslator().GetString("LVMi");
                label4.Text = GetTranslator().GetString("LVMi");
                label5.Text = GetTranslator().GetString("LVMi");
                label14.Text = GetTranslator().GetString("LVMi");
                TranslateFilterNearKmMi();
            }
        }

        private void SetManualSelectionToggle(object sender, EventArgs e)
        {
            foreach (Object obj in lvGeocaches.SelectedItems)
            {
                EXListViewItem lstvItem = obj as EXListViewItem;
                // The bool column
                EXBoolListViewSubItem subool = (EXBoolListViewSubItem)(lstvItem.SubItems[_ID_LVSel]);
                ToggleSelection(lstvItem, subool);
            }
        }

        private void SetManualSelectionAll(object sender, EventArgs e)
        {
            foreach (Object obj in lvGeocaches.SelectedItems)
            {
                EXListViewItem lstvItem = obj as EXListViewItem;
                // The bool column
                EXBoolListViewSubItem subool = (EXBoolListViewSubItem)(lstvItem.SubItems[_ID_LVSel]);
                subool.BoolValue = false; // so toggle with set it to true :-)
                ToggleSelection(lstvItem, subool);
            }
        }

        private void SetManualDeselectionAll(object sender, EventArgs e)
        {
            foreach (Object obj in lvGeocaches.SelectedItems)
            {
                EXListViewItem lstvItem = obj as EXListViewItem;
                // The bool column
                EXBoolListViewSubItem subool = (EXBoolListViewSubItem)(lstvItem.SubItems[_ID_LVSel]);
                subool.BoolValue = true; // so toggle with set it to false :-)
                ToggleSelection(lstvItem, subool);
            }
        }

        private void ToggleSelection(EXListViewItem lstvItem, EXBoolListViewSubItem subool)
        {
            subool.BoolValue = !(subool.BoolValue);
            lvGeocaches.Invalidate(subool.Bounds);
            String code = lstvItem.Text;
            try
            {
                Geocache geo = _caches[code];
                geo._bManualSelection = subool.BoolValue;
                if (geo._bManualSelection)
                    lstvItem._bHighlighted = true;
                else
                    lstvItem._bHighlighted = false;
            }
            catch (Exception)
            {
                MsgActionError(this, GetTranslator().GetString("ErrorCode") + ": " + code);
            }
        }

        private void downloadSpoilersAuthenticatedToolStripMenuItem_Click(object sender, EventArgs e)
        {
        	MakeOfflineDataAvailable(true);
        }
        
        private void CreateOfflineData(object sender, EventArgs e)
        {

        	MakeOfflineDataAvailable(false);
        }

        private void MakeOfflineDataAvailable(bool UseGCAccount)
        {
        	if (bOfflineDownloadInProgress)
                return;

            // Paramètres de récupération
            if (!ConfigureSpoilerDownload())
            {
            	MsgActionCanceled(this);
            	return;
            }
            
            // Utilise-t'on la galerie complète ? 
            UpdateHttpDefaultWebProxy();
            CookieContainer cookieJar = null;
            if (UseGCAccount)
            {
            	// On checke que les L/MDP soient corrects
                // Et on récupère les cookies au passage
                cookieJar = CheckGCAccount(true, false);
                if (cookieJar == null)
                    return;
            }
            
            
            bool bUseGallery = (ConfigurationManager.AppSettings["getimagesfromgallery"] == "True");
            
            List<OfflineCacheData> ocds = new List<OfflineCacheData>();
            String offdatapath = GetUserDataPath() + Path.DirectorySeparatorChar + "Offline";
            
            foreach (Object obj in lvGeocaches.SelectedItems)
            {
                EXListViewItem lstvItem = obj as EXListViewItem;
                String code = lstvItem.Text;
                
                try
                {
                    Geocache cache = _caches[code];
                    OfflineCacheData ocd1 = null;

                    //  Check if html description exists
                    String descHTML = "";
                    if (cache._ShortDescHTML.ToLower() == "true")
                        descHTML += cache._ShortDescriptionInHTML;
                    if (cache._LongDescHTML.ToLower() == "true")
                        descHTML += cache._LongDescriptionInHTML;

                    // Check if ocd exists for this cache
                    if (_od._OfflineData.ContainsKey(cache._Code))
                    {
                        // Get existing OCD
                        ocd1 = _od._OfflineData[cache._Code];
                    }
                    else
                    {
                        // Create new OCD
                        ocd1 = new OfflineCacheData();
                        ocd1._Code = cache._Code;
                        AssociateOcdCache(cache._Code, ocd1, cache);
                    }

                    // final attributes setting and add OCD to the list
                    ocd1._descHTML = descHTML;
                    ocd1._dateExport = DateTime.Now;
                    ocds.Add(ocd1);
                }
                catch (Exception excm)
                {
                	ShowException("", GetTranslator().GetString("ErrorCode") + ": " + code, excm);
                }
            }

            // Start download
            ThreadProgress threadprogress = new ThreadProgress();
            if (ocds.Count != 0)
            {
                threadprogress.Font = this.Font;
                threadprogress.Icon = this.Icon;
                threadprogress.Text = GetTranslator().GetString("LblDownloadInProgress");
                threadprogress.btnAbort.Text = GetTranslator().GetString("BtnAbort");
                threadprogress.Show();

                DownloadWorker worker = new DownloadWorker(this);
                worker.keywordsspoiler = GetSpoilerKeyWords();
                worker.delay = Int32.Parse(ConfigurationManager.AppSettings["spoilerdelaydownload"]);
                worker.threadprogress = threadprogress;
                worker.ocds = ocds;
                worker.bGetFromGallery = bUseGallery;
                worker.cookieJar = cookieJar;
                
                Thread workerThread = new Thread(worker.DoWork);

                // Start the worker thread.
                workerThread.Start();
                Console.WriteLine("main thread: Starting worker thread...");

                // Loop until worker thread activates.
                while (!workerThread.IsAlive) ;
            }
        }
        
        /// <summary>
        /// Remove an OCD from OfflineData
        /// </summary>
        /// <param name="code">cache code</param>
        public void RemoveAssociationOcdCache(String code)
        {
            Geocache cache = _caches[code];
            RemoveAssociationOcdCache(code, cache);
        }

        /// <summary>
        /// Remove an OCD from OfflineData and cross reference in Geocache object
        /// </summary>
        /// <param name="code">cache code</param>
        /// <param name="cache">geocache</param>
        public void RemoveAssociationOcdCache(String code, Geocache cache)
        {
            _od._OfflineData.Remove(code);
            if (cache != null)
                cache._Ocd = null;
        }

        /// <summary>
        /// Associate an OCD object to a cache
        /// </summary>
        /// <param name="code">cache code</param>
        /// <param name="ocd">OCD object</param>
        /// <param name="cache">cache</param>
        public void AssociateOcdCache(String code, OfflineCacheData ocd, Geocache cache)
        {
            if (_od._OfflineData.ContainsKey(code))
            {
                // On remplace
                _od._OfflineData[code] = ocd;
            }
            else
                _od._OfflineData.Add(code, ocd);

            // Assign ocd element
            ocd._Code = code;
            if (cache != null)
                cache._Ocd = ocd;
        }

        private void DeleteOfflineData(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(GetTranslator().GetString("LblConfirmDelOffline"),
                        GetTranslator().GetString("WarTitle"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                String offdatapath = GetUserDataPath() + Path.DirectorySeparatorChar + "Offline";
                List<OfflineCacheData> toPurge = new List<OfflineCacheData>();
                foreach (Object obj in lvGeocaches.SelectedItems)
                {
                    EXListViewItem lstvItem = obj as EXListViewItem;
                    String code = lstvItem.Text;
                    try
                    {
                        if (_od._OfflineData.ContainsKey(code))
                        {
                            // Remove old file from OCD
                            OfflineCacheData ocd1 = _od._OfflineData[code];
                            toPurge.Add(ocd1);
                            ocd1.PurgeFiles(offdatapath);
                            if (ocd1.IsEmpty())
                            {
                                RemoveAssociationOcdCache(code);
                            }
                        }
                    }
                    catch (Exception excm)
                    {
                    	ShowException("", GetTranslator().GetString("ErrorCode") + ": " + code, excm);
                    }
                }
                _od.Serialize(_odfile);
                UpdateListViewOfflineIcons(toPurge);
            }
        }

        private void WriteNoteOnCacheEvent(object sender, EventArgs e)
        {
            WriteNoteOnCache();
        }

        private void WriteNoteOnCache()
        {
            EXListViewItem lstvItem = lvGeocaches.Items[lvGeocaches.SelectedIndices[0]] as EXListViewItem;
            String code = lstvItem.Text;
            OfflineCacheData ocd1 = null;
            Geocache cache = _caches[code];
            if (_od._OfflineData.ContainsKey(code))
            {
                ocd1 = _od._OfflineData[code];
            }
            else
            {
                ocd1 = new OfflineCacheData();
                //ocd1._dateExport = DateTime.Now;
                //ocd1._Comment = "";
                ocd1._Code = code;
                //ocd1._bBookmarked = false;
                AssociateOcdCache(code, ocd1, cache);
            }

            NoteForm param = new NoteForm();
            TranslateTooltips(param, null);
            param.Icon = this.Icon;
            param.Font = this.Font;
            param.Text = GetTranslator().GetString("MNUNote");
            param.button1.Text = GetTranslator().GetString("BtnOk");
            param.button2.Text = GetTranslator().GetString("BtnCancel");
            param.label1.Text = GetTranslator().GetString("LblNote");
            param.textBox1noteform.Text = ocd1._Comment;

            if (param.ShowDialog() == DialogResult.OK)
            {
                ocd1._Comment = param.textBox1noteform.Text;

                if (ocd1.IsEmpty())
                {
                    RemoveAssociationOcdCache(code, cache);
                }

                _od.Serialize(_odfile);

                UpdateListViewOfflineIcons(ocd1);
            }
            else
            {
                if (ocd1.IsEmpty())
                {
                    RemoveAssociationOcdCache(code, cache);
                    _od.Serialize(_odfile);
                    UpdateListViewOfflineIcons(ocd1);
                }
            }
        }

        private void ViewOfflineData(object sender, EventArgs e)
        {
            foreach (Object obj in lvGeocaches.SelectedItems)
            {
                EXListViewItem lstvItem = obj as EXListViewItem;
                DisplayDetailFromSelection(lstvItem, true, true);
            }
        }

        private void SetDisplayDetail(object sender, EventArgs e)
        {
            foreach (Object obj in lvGeocaches.SelectedItems)
            {
                EXListViewItem lstvItem = obj as EXListViewItem;
                DisplayDetailFromSelection(lstvItem, false, true);
            }
        }

        private void SetDisplayGeocaching(object sender, EventArgs e)
        {
            foreach (Object obj in lvGeocaches.SelectedItems)
            {
                EXListViewItem lstvItem = obj as EXListViewItem;
                String code = lstvItem.Text;
                try
                {
                    Geocache geo = _caches[code];
                    if (ConfigurationManager.AppSettings["openGeocachingEmbedded"] == "True")
                        _cacheDetail.LoadPage("(GEO) " + geo._Name, geo._Url);
                    else
                        MyTools.StartInNewThread(geo._Url);
                }
                catch (Exception)
                {
                    MsgActionError(this, GetTranslator().GetString("ErrorCode") + ": " + code);
                }
            }
        }

        /// <summary>
        /// Returns image from index (from _listImagesSized)
        /// </summary>
        /// <param name="index">image index</param>
        /// <returns>image</returns>
        public Image getImageFromIndex(int index)
        {
            return _listImagesSized[index];
        }

        /// <summary>
        /// Returns image index from its key (from _listImagesSized)
        /// If key is not found, return [Fail] image index
        /// </summary>
        /// <param name="key">image key</param>
        /// <returns>image index</returns>
        public int getIndexImages(String key)
        {
            //Log("Get image for " + key);
            if (_indexImages.ContainsKey(key))
                return _indexImages[key];
            else
            {
                // Try to get with lower key ?
                if (_indexImagesLowerKey.ContainsKey(key.ToLower()))
                {
                    Log("!!!! Key " + key + " not found in images");
                    Log("!!!! Using lower case key instead");
                    return _indexImagesLowerKey[key.ToLower()];
                }
                else
                {
                    Log("!!!! Key " + key + " not found in images");
                    Log("!!!! Using *False* key instead");
                    return _indexImages["Fail"];
                }
            }
        }

        /// <summary>
        /// Returns image index from its key (from _listImagesSized)
        /// </summary>
        /// <param name="key">image key</param>
        /// <returns>image</returns>
        public Image GetImageSized(string key)
        {
            return _listImagesSized[getIndexImages(key)];
        }

        private void InitFilterImages()
        {
        	foreach(KeyValuePair<String, CheckBox> pair in _geocachingConstants.GetDicoSizeCheckbox())
            {
            	pair.Value.Image = _listImagesSized[getIndexImages(pair.Key)];
            }
            

            int iSize = 32;
            foreach(KeyValuePair<String, CheckBox> pair in _geocachingConstants.GetDicoTypeCheckbox())
            {
            	pair.Value.Image = MyTools.ResizeImage(_listImagesSized[getIndexImages(pair.Key)],iSize,iSize);
            }
            
            
            pbAvail.Image = _listImagesSized[getIndexImages("Enable Listing")];
            pbArchive.Image = _listImagesSized[getIndexImages("Temporarily Disable Listing")];
            pbFound.Image = _listImagesSized[getIndexImages("Found it")];
            pbOwned.Image = _listImagesSized[getIndexImages("Owned")];
            pbTB.Image = _listImagesSized[getIndexImages("TB")];
            pbGC.Image = _listImagesSized[getIndexImages("GC")];

            btnMatrixFilterDT.Image = _listImagesSized[getIndexImages("Matrix")];
            btnCartoDisplay.Image = _listImagesSized[getIndexImages("Earth")];
            btnResetFilters.Image = _listImagesSized[getIndexImages("Close")];
            btnCenterOnCarto.Image = _listImagesSized[getIndexImages("Earth")];

            comboDMin.SelectedIndex = 0;
            comboDMax.SelectedIndex = 8;
            comboTMin.SelectedIndex = 0;
            comboTMax.SelectedIndex = 8;
            comboCreation.SelectedIndex = 0;
            comboLastlog.SelectedIndex = 0;
            comboFavorites.SelectedIndex = 0;
            comboPopularity.SelectedIndex = 0;
            

            // To prevent the filter from being already actived, we uncheck them
            cbFilterTerrain.Checked = false;
            cbFilterDifficulty.Checked = false;
            cbFilterCreation.Checked = false;
            cbFilterLastLog.Checked = false;
            cbFilterAttributeIn.Checked = false;
            cbFilterAttributeOut.Checked = false;
            cbFilterPopularity.Checked = false;
            cbFilterFavorites.Checked = false;
        }

        private int GetVisualEltIndexFromCacheCode(String code)
        {
            foreach (EXImageListViewItem item in lvGeocaches.Items)
            {
                if (item.Text == code)
                {
                    return lvGeocaches.Items.IndexOf(item);
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns listviewitem from Geocache code
        /// </summary>
        /// <param name="code">geocache code</param>
        /// <returns>listviewitem from Geocache code, can be null</returns>
        public EXImageListViewItem GetVisualEltFromCacheCode(String code)
        {
            foreach (EXImageListViewItem item in lvGeocaches.Items)
            {
                if (item.Text == code)
                {
                    return item;
                }
            }
            return null;
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void RemoveVisualElt(EXImageListViewItem item)
        {
        	lvGeocaches.Items.Remove(item);
        }

        
        /// <summary>
        /// Create all list view cache elements and populate the list
        /// </summary>
        public void BuildListViewCache()
        {
			_listViewCaches = new List<EXListViewItem>();
            int iNbDaysForNew = Int32.Parse(ConfigurationManager.AppSettings["daysfornew"]);
            String kmmi = (_bUseKm) ? GetTranslator().GetString("LVKm") : GetTranslator().GetString("LVMi");
            bool bMarkMissingCache = (ConfigurationManager.AppSettings["markmissingcaches"] == "True");
            int numberlastlogssymbols = Int32.Parse(ConfigurationManager.AppSettings["numberlastlogssymbols"]);

            foreach (KeyValuePair<String, Geocache> elt in _caches)
            {
                Geocache cache = elt.Value;
                EXImageListViewItem item = CreateVisualEltFromGeocache(iNbDaysForNew, kmmi, bMarkMissingCache, cache, numberlastlogssymbols);
                _listViewCaches.Add(item);
            }
        }

        /// <summary>
        /// Create all list view cache elements and populate the list
        /// </summary>
        /// <param name="caches">caches</param>
        /// <returns>list of created items</returns>
        public List<EXImageListViewItem> BuildListViewCache(List<Geocache> caches)
        {
            int iNbDaysForNew = Int32.Parse(ConfigurationManager.AppSettings["daysfornew"]);
            String kmmi = (_bUseKm) ? GetTranslator().GetString("LVKm") : GetTranslator().GetString("LVMi");
            bool bMarkMissingCache = (ConfigurationManager.AppSettings["markmissingcaches"] == "True");
            int numberlastlogssymbols = Int32.Parse(ConfigurationManager.AppSettings["numberlastlogssymbols"]);

            List<EXImageListViewItem> ltvi = new List<EXImageListViewItem>();
            foreach (Geocache cache in caches)
            {
                EXImageListViewItem item = CreateVisualEltFromGeocache(iNbDaysForNew, kmmi, bMarkMissingCache, cache, numberlastlogssymbols);
                _listViewCaches.Add(item);
                ltvi.Add(item);
            }

            return ltvi;
        }

        private EXImageListViewItem CreateVisualEltFromGeocache(int iNbDaysForNew, String kmmi, bool bMarkMissingCache, Geocache cache, int numberlastlogssymbols)
        {
            //Log(cache._Code);
            int index;
            //EXListViewItem item = new EXListViewItem(cache._Code); 
            EXImageListViewItem item = new EXImageListViewItem(cache._Code);
            item.Tag = cache._Code;
            item.MyValue = cache._Code;
            item.Name = "LVCode";
            // Selection
            item.SubItems.Add(new EXBoolListViewSubItem(false));

            // Update offline icons
            // We will reuse the OCD object for Tag display
            OfflineCacheData ocd = UpdateOfflineIcons(item, cache._Code);

            EXMultipleImagesListViewSubItem subItem;
            //Type
            index = getIndexImages(cache._Type);
            item.SubItems.Add(new EXMultipleImagesListViewSubItem(new ArrayList(new object[] { _listImagesSized[index] }), cache._Type));
            // Name
            //String text = cache._Name + " (" + cache._PlacedBy + ")";
            String text = cache._Name + " (" + cache._Owner + ")";

            ArrayList iconList = new ArrayList();
            if (cache._bOwned)
            {
                index = getIndexImages("Owned");
                iconList.Add(_listImagesSized[index]);
            }
            else if (cache.IsFound())
            {
                index = getIndexImages("Found");
                iconList.Add(_listImagesSized[index]);
            }
            // Always in second position if exists
            if (isBookmarked(item, cache._Code))
            {
                int ifav = getIndexImages("Bookmark");
                iconList.Add(_listImagesSized[ifav]);
            }

            if (iconList.Count == 0)
            {
                subItem = new EXMultipleImagesListViewSubItem(text, iconList, text);
                //subItem = new EXMultipleImagesListViewSubItem(text);
                //subItem.MyValue = text;
            }
            else
            {
                subItem = new EXMultipleImagesListViewSubItem(text, iconList, text);
            }

            if (cache._Available.ToLower() != "true")
                subItem.Font = new Font(subItem.Font, subItem.Font.Style | FontStyle.Strikeout);

            if (cache.HasBeenModified)
                subItem.ForeColor = Color.Red;

            item.SubItems.Add(subItem);
            
            // Standard Geocache
            //Container
            index = getIndexImages(cache._Container);
            item.SubItems.Add(new EXMultipleImagesListViewSubItem(new ArrayList(new object[] { _listImagesSized[index] }), _geocachingConstants.ConvertContainerToInt(cache._Container).ToString() + "#"));

            // D
            index = getIndexImages(cache._D);
            item.SubItems.Add(new EXMultipleImagesListViewSubItem(new ArrayList(new object[] { _listImagesSized[index] }), cache._D + "#"));
            //subItem = new EXMultipleImagesListViewSubItem(cache._D);
            //subItem.MyValue = subItem.Text;
            //item.SubItems.Add(subItem);
            // T
            index = getIndexImages(cache._T);
            item.SubItems.Add(new EXMultipleImagesListViewSubItem(new ArrayList(new object[] { _listImagesSized[index] }), cache._T + "#"));
            //subItem = new EXMultipleImagesListViewSubItem(cache._T);
            //subItem.MyValue = subItem.Text;
            //item.SubItems.Add(subItem);


            // Km
            double d = 0;
            if (_bUseKm)
                d = cache.DistanceToHome();
            else
                d = cache.DistanceToHomeMi();

            String sd = String.Format("{0:0.#}", d);
            subItem = new EXMultipleImagesListViewSubItem(sd + " " + kmmi);
            subItem.MyValue = sd;
            item.SubItems.Add(subItem);
            // Placed
            String dateCreation = MyTools.CleanDate(cache._DateCreation);
            DateTime placed = MyTools.ParseDate(dateCreation); //DateTime.Parse(dateCreation);
            if ((DateTime.Now - placed).TotalDays < iNbDaysForNew)
            {
                index = getIndexImages("New");
                subItem = new EXMultipleImagesListViewSubItem(dateCreation, new ArrayList(new object[] { _listImagesSized[index] }), dateCreation);
                subItem.MyValue = dateCreation;
            }
            else
            {
                subItem = new EXMultipleImagesListViewSubItem(dateCreation);
                subItem.MyValue = dateCreation;
            }
            // pour un meilleur tri ! en premier la date de creation de la cache puis le chrono de la cache
            // Pour des dates identiques, c'est le chrono qui permettra de différencier
            {
                int tailleidmax = 12; // longeur de l'id
                string cacheid = cache._CacheId;
                if (cacheid.Length < tailleidmax)
                    cacheid = cacheid.PadLeft(tailleidmax - cacheid.Length);
                subItem.MyValue = dateCreation + cacheid;
            }

            item.SubItems.Add(subItem);

            // Last log
            if (cache._Logs.Count != 0)
            {
                // prendre le premier log, c'est le plus récent, la liste est déjà triée
                // l'utiliser pour l'affichage !
                CacheLog log = cache._Logs[0];
                index = getIndexImages(log._Type);
                String lastLog = MyTools.CleanDate(log._Date);
                
                // Optimisation !
                int maxnblogs = numberlastlogssymbols;
                if (maxnblogs == 1)
                {
                    // Fastdisplay !!!
                    if (!bMarkMissingCache)
                    {
                        subItem = (EXMultipleImagesListViewSubItem)(item.SubItems.Add(new EXMultipleImagesListViewSubItem(lastLog, new ArrayList(new object[] { _listImagesSized[index] }), lastLog)));
                    }
                    else
                    {
                        // Test d'identification des caches disparues
                        // **************
                        if (CustomFilterExcludeMissingCaches.ToBeDisplayed(new bool[] { true, true, false }, new bool[] { true, true, true, true, true }, 3, cache))
                            subItem = (EXMultipleImagesListViewSubItem)(item.SubItems.Add(new EXMultipleImagesListViewSubItem(lastLog, new ArrayList(new object[] { _listImagesSized[index] }), lastLog)));
                        else
                            subItem = (EXMultipleImagesListViewSubItem)(item.SubItems.Add(new EXMultipleImagesListViewSubItem(lastLog, new ArrayList(new object[] { _listImagesSized[getIndexImages("Error")], _listImagesSized[index] }), lastLog)));
                        // **************
                    }
                }
                else
                {
                    // Potentiellement plus long
                    if (!bMarkMissingCache)
                    {
                        int nblogs = Math.Min(maxnblogs, cache._Logs.Count);
                        object[] images = new object[nblogs];
                        for (int i = 0; i < nblogs; i++)
                        {
                            images[i] = _listImagesSized[getIndexImages(cache._Logs[i]._Type)];
                        }

                        //object[] images = new object[] { _listImagesSized[index] };
                        subItem = (EXMultipleImagesListViewSubItem)(item.SubItems.Add(new EXMultipleImagesListViewSubItem(lastLog, new ArrayList(images), lastLog)));
                    }
                    else
                    {
                        // Test d'identification des caches disparues
                        // **************
                        int ioffset = 0;
                        int nblogs = Math.Min(maxnblogs, cache._Logs.Count);
                        if (CustomFilterExcludeMissingCaches.ToBeDisplayed(new bool[] { true, true, false }, new bool[] { true, true, true, true, true }, 3, cache))
                        {
                            // présente
                        }
                        else
                        {
                            // potentiellement disparue
                            ioffset = 1;
                            nblogs += 1;
                        }
                        object[] images = new object[nblogs];
                        if (ioffset == 1)
                            images[0] = _listImagesSized[getIndexImages("Error")];
                        for (int i = ioffset; i < nblogs; i++)
                            images[i] = _listImagesSized[getIndexImages(cache._Logs[i - ioffset]._Type)];
                        subItem = (EXMultipleImagesListViewSubItem)(item.SubItems.Add(new EXMultipleImagesListViewSubItem(lastLog, new ArrayList(images), lastLog)));
                        // **************
                    }
                }

                // pour un meilleur tri ! en premier la date du log puis le chrono du log
                // Pour des dates identiques, c'est le chrono qui permettra de différencier
                subItem.MyValue = log._SortingKey;

            }
            else
            {
                subItem = new EXMultipleImagesListViewSubItem("");
                subItem.MyValue = "";
                item.SubItems.Add(subItem);
            }

            // TB/GC
            int iTBGC = cache._listTB.Count;
            if (iTBGC == 0)
            {
                subItem = new EXMultipleImagesListViewSubItem(" ");
                subItem.MyValue = iTBGC.ToString();
            }
            else
            {
                index = getIndexImages("TBORGC");
                subItem = new EXMultipleImagesListViewSubItem(" x " + iTBGC.ToString(), new ArrayList(new object[] { _listImagesSized[index] }), iTBGC.ToString());
            }
            item.SubItems.Add(subItem);

            // Available & Archive
            String val = "";
            if ((cache._Available.ToLower() == "true") && (cache._Archived.ToLower() == "false"))
            {
                // Published and activated
                val = "Enable Listing";
                index = getIndexImages(val);
                item.SubItems.Add(new EXMultipleImagesListViewSubItem(new ArrayList(new object[] { _listImagesSized[index] }), "0"));
            }
            else if ((cache._Available.ToLower() == "false") && (cache._Archived.ToLower() == "false"))
            {
                // Temporary deactivated
                val = "Temporarily Disable Listing";
                index = getIndexImages(val);
                item.SubItems.Add(new EXMultipleImagesListViewSubItem(new ArrayList(new object[] { _listImagesSized[index] }), "1"));
            }
            else // Archived == true
            {
                // Archived
                val = "Archive";
                index = getIndexImages(val);
                item.SubItems.Add(new EXMultipleImagesListViewSubItem(new ArrayList(new object[] { _listImagesSized[index] }), "2"));

            }
            /*
            // Available
            index = getIndexImages(cache._Available);
            item.SubItems.Add(new EXMultipleImagesListViewSubItem(new ArrayList(new object[] { _listImagesSized[index] }), cache._Available));
            // Archived
            String notarch = (cache._Archived == "True") ? "False" : "True";
            index = getIndexImages(notarch);
            item.SubItems.Add(new EXMultipleImagesListViewSubItem(new ArrayList(new object[] { _listImagesSized[index] }), cache._Archived));
            */

            //Attributes
            if (cache._Attributes.Count != 0)
            {
                ArrayList imgitem = new ArrayList();
                String atttxt = "";
                String atttxtv = "";
                foreach (String att in cache._Attributes)
                {
                    atttxt += GetTranslator().GetString(CreateAttributeTranslationKey(att)) + "; ";
                    atttxtv += att + "; ";
                    // WARNING : these frecking attributes are stored in lower case since it's hell to handle
                    String att2 = att.Replace("/", "");
                    index = getIndexImages(att2.ToLower());
                    imgitem.Add(_listImagesSized[index]);
                }
                item.SubItems.Add(new EXMultipleImagesListViewSubItem(imgitem, atttxtv));

                subItem = new EXMultipleImagesListViewSubItem(atttxt);
                subItem.MyValue = atttxtv;
                item.SubItems.Add(subItem);
            }
            else
            {
                subItem = new EXMultipleImagesListViewSubItem(" ");
                subItem.MyValue = "";
                item.SubItems.Add(subItem);
                subItem = new EXMultipleImagesListViewSubItem(" ");
                subItem.MyValue = "";
                item.SubItems.Add(subItem);
            }

            // Owner
            subItem = new EXMultipleImagesListViewSubItem(cache._Owner);
            subItem.MyValue = cache._Owner;
            item.SubItems.Add(subItem);

            // State
            subItem = new EXMultipleImagesListViewSubItem(cache._State);
            subItem.MyValue = cache._State;
            item.SubItems.Add(subItem);

            // country
            subItem = new EXMultipleImagesListViewSubItem(cache._Country);
            subItem.MyValue = cache._Country;
            item.SubItems.Add(subItem);

            // hint
            String hint = (cache._Hint == "") ? " " : cache._Hint;
            hint = hint.Replace("\r\n", " ");
            hint = hint.Replace("\n", " ");
            subItem = new EXMultipleImagesListViewSubItem(ROT13.Transform(hint));
            subItem.MyValue = hint;
            subItem.Name = "LVHint";
            item.SubItems.Add(subItem);

            // LVTag
            //if (true)
            {
                ArrayList imgitem = GetListImageTags(ocd);
                if ((imgitem != null) && (imgitem.Count != 0))
                {
                    subItem = new EXMultipleImagesListViewSubItem(imgitem, ocd.GetTags());
                    //subItem.MyValue = ocd.GetTags();
                    item.SubItems.Add(subItem);
                }
                else
                {
                    // Nothing to show
                    subItem = new EXMultipleImagesListViewSubItem(" ");
                    subItem.MyValue = "";
                    item.SubItems.Add(subItem);
                }
            }/*
            else
            {
                // Old fashion with text
                String tags = "";
                if (ocd != null)
                    tags = ocd.GetTags();
                subItem = new EXMultipleImagesListViewSubItem(tags);
                subItem.MyValue = tags;
                item.SubItems.Add(subItem);
            }*/

            // Récupération des statistiques
            String fv = "";
            double rating = -1.0;
            double alti = Double.MaxValue;
            int nbfound = -1;
            int nbdnf = -1;
            if (ocd != null)
            {
                fv = (ocd._iNbFavs == -1) ? "" : ocd._iNbFavs.ToString();
                nbfound = ocd._iNbFounds;
                nbdnf = ocd._iNbNotFounds;

                if (_bUseGCPopularity)
                    rating = ocd._dRatingSimple;
                else
                    rating = ocd._dRating;

                alti = ocd._dAltiMeters;
            }

            // LVFavs
            subItem = new EXMultipleImagesListViewSubItem(fv);
            if (fv != "")
            {
                index = getIndexImages("Fav");
                subItem.MyImages = new ArrayList(new object[] { _listImagesSized[index] });
            }
            subItem.MyValue = fv;
            item.SubItems.Add(subItem);

            // LVRating
            if (rating != -1)
            {
                //String iRating = RatingToImageStar(rating);
                String iRating = "ratio_" + ((int)(rating*100.0)).ToString();
                index = getIndexImages(iRating);
                String srating = rating.ToString("0.0%");
                subItem = new EXMultipleImagesListViewSubItem(new ArrayList(new object[] { _listImagesSized[index] }), rating.ToString("000.00%"));
                subItem.Text = srating;
                item.SubItems.Add(subItem);
            }
            else
            {
                subItem = new EXMultipleImagesListViewSubItem("");
                subItem.MyValue = "#";
                item.SubItems.Add(subItem);
            }

            // LVAlti
            if (alti != Double.MaxValue)
            {
                String salti = "";
                if (_bUseKm)
                {
                    salti = String.Format("{0:0.#}", alti) + " m";
                }
                else
                {
                    salti = String.Format("{0:0.#}", alti * 3.2808399) + " ft";
                }
                subItem = new EXMultipleImagesListViewSubItem(salti);

                // On shifte de 5000m pour éliminer les valeurs négatives et on arrondi après la virgule !
                subItem.MyValue = String.Format("{0:0.00}", alti + 5000.0);
                //subItem.MyValue = alti.ToString(); // On stocke toujours la valeur en mètres !

                item.SubItems.Add(subItem);
            }
            else
            {
                subItem = new EXMultipleImagesListViewSubItem("");
                subItem.MyValue = "#";
                item.SubItems.Add(subItem);
            }

            // LVFoundDNF
            if ((nbfound != -1) && (nbdnf != -1))
            {
                if (nbdnf == -1)
                    nbdnf = 0;
                if (nbfound == -1)
                    nbfound = 0;
                int total = (nbfound + nbdnf);
                String lbl = total.ToString() + " (" + nbfound.ToString() + "/" + nbdnf.ToString() + ")";
                subItem = new EXMultipleImagesListViewSubItem(lbl);
                subItem.MyValue = total.ToString();
                //int tailleidmax = 6; // longeur de l'id
                //if (subItem.MyValue.Length < tailleidmax)
                //    subItem.MyValue = subItem.MyValue.PadLeft(tailleidmax - subItem.MyValue.Length);
                item.SubItems.Add(subItem);
            }
            else
            {
                subItem = new EXMultipleImagesListViewSubItem("");
                subItem.MyValue = "#";
                item.SubItems.Add(subItem);
            }

            return item;
        }

        /// <summary>
        /// Converts rating in percentage to image
        /// Used to display popularity
        /// </summary>
        /// <param name="r">rating in percentage (from 0.0 to 1.0)</param>
        /// <returns>star image key</returns>
        public String RatingToImageStar(double r)
        {
            double rating = r * 100.0; // *2; // Well, > 50% will give a 5 stars rating
            String img = "0";
            if (rating < 10.0)
                img = "0";
            else if (rating < 20.0)
                img = "0.5";
            else if (rating < 30.0)
                img = "1";
            else if (rating < 40.0)
                img = "1.5";
            else if (rating < 50.0)
                img = "2";
            else if (rating < 60.0)
                img = "2.5";
            else if (rating < 70.0)
                img = "3";
            else if (rating < 80.0)
                img = "3.5";
            else if (rating < 90.0)
                img = "4";
            else if (rating < 100.0)
                img = "4.5";
            else
                img = "5";
            
            return img;
        }

        static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        /// <summary>
        /// Get all tags from an OCD
        /// </summary>
        /// <param name="ocd">OCD</param>
        /// <returns>list of tags</returns>
        public ArrayList GetListImageTags(OfflineCacheData ocd)
        {
            ArrayList imgitem = null;
            if ((ocd != null) && (ocd._Tags.Count != 0))
            {
                imgitem = new ArrayList();
                Image img = null;
                foreach (String tag in ocd._Tags)
                {
                    img = GetImageTag(tag);
                    imgitem.Add(img);
                }
            }
            return imgitem;
        }

        /// <summary>
        /// Convert a tag into an image
        /// </summary>
        /// <param name="tag">tag</param>
        /// <returns>image</returns>
        public Image GetImageTag(String tag)
        {
            Image img = null;
            String t = UppercaseFirst(tag);
            if (_imgTags.ContainsKey(t))
                img = _imgTags[t];
            else
            {
                Bitmap bmp = MyTools.CreateBitmapImage(t);
                img = bmp;
                _imgTags.Add(t, bmp);
                _listExistingTags.Add(tag);
            }
            return img;
        }

        /// <summary>
        /// Converts OpenCaching double value to fixed decimale values (Geocaching compatibility)
        /// </summary>
        /// <param name="soxv">Opencaching double value</param>
        /// <returns>Geocaching compliant value</returns>
        public String ConvertOXValueToDecimal(string soxv)
        {
            
            double r = 0;
            double oxv = 0;
            if (!Double.TryParse(soxv, NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out oxv))
                return "0";

            if (oxv < 0.5)
                r = 0;
            else if (oxv < 1.0)
                r = 0.5;
            else if (oxv < 1.5)
                r = 1.0;
            else if (oxv < 2.0)
                r = 1.5;
            else if (oxv < 2.5)
                r = 2.0;
            else if (oxv < 3.0)
                r = 2.5;
            else if (oxv < 3.5)
                r = 3.0;
            else if (oxv < 4.0)
                r = 3.5;
            else if (oxv < 4.5)
                r = 4.0;
            else if (oxv < 5.0)
                r = 4.5;
            else 
                r = 5.0;

            return r.ToString().Replace(",",".");
        }

        private void UpdateListViewKmMi()
        {
            String kmmi = (_bUseKm) ? GetTranslator().GetString("LVKm") : GetTranslator().GetString("LVMi");
            lvGeocaches.BeginUpdate();
            foreach (EXListViewItem item in _listViewCaches)
            {
                // On convertit les distances en km ou miles
                double d = 0;
                Geocache cache = _caches[item.Text];
                if (_bUseKm)
                    d = cache.DistanceToHome();
                else
                    d = cache.DistanceToHomeMi();

                String sd = String.Format("{0:0.#}", d);
                EXMultipleImagesListViewSubItem sub = item.SubItems[_ID_LVDistance] as EXMultipleImagesListViewSubItem;
                sub.Text = sd + " " + kmmi;
                sub.MyValue = sd;

                // on convertit les altitudes en metres ou pieds
                if ((cache._Ocd != null) && (cache._Ocd._dAltiMeters != Double.MaxValue))
                {
                    // On a une altitude valide et on va donc changer quelque chose
                    String salti = "";
                    if (_bUseKm)
                    {
                        salti = String.Format("{0:0.#}", cache._Ocd._dAltiMeters) + " m";
                    }
                    else
                    {
                        salti = String.Format("{0:0.#}", cache._Ocd._dAltiMeters * 3.2808399) + " ft";
                    }
                    // L'item altitude
                    sub = item.SubItems[_ID_LVAlti] as EXMultipleImagesListViewSubItem;
                    sub.Text = salti;
                }
            }
            lvGeocaches.EndUpdate();
        }

        private bool isBookmarked(EXListViewItem item, String code)
        {
            EXImageListViewItem lvitem = item as EXImageListViewItem;
            if (lvitem != null)
            {
                if (_od._OfflineData.ContainsKey(code))
                {
                    OfflineCacheData ocd = _od._OfflineData[code];
                    if (ocd._bBookmarked)
                        return true;
                }
            }
            return false;
        }

        private void UpdateBookmarkIcons(EXListViewItem item, String code, bool bBookmark)
        {
            ListViewItem.ListViewSubItem it = item.SubItems[_ID_LVName];
            EXMultipleImagesListViewSubItem lvitem = it as EXMultipleImagesListViewSubItem;
            int ifav = getIndexImages("Bookmark");
            Image imgBook = _listImagesSized[ifav];

            if (lvitem != null)
            {
               
                if ((lvitem.MyImages != null) && (lvitem.MyImages.Count != 0))
                {
                    // Something is there
                    if (bBookmark)
                    {
                        // We need to add bookmark if necessary
                        bool bFound = false;
                        for (int i = 0; i < lvitem.MyImages.Count; i++)
                        {
                            if (lvitem.MyImages[i] == imgBook)
                            {
                                // We found an image that is the bookmark icon
                                bFound = true;
                                break;
                            }
                        }
                        if (!bFound)
                        {
                            lvitem.MyImages.Add(imgBook);
                        }
                    }
                    else
                    {
                        // We need to remove it if found
                        for (int i = 0; i < lvitem.MyImages.Count; i++)
                        {
                            if (lvitem.MyImages[i] == imgBook)
                            {
                                // We found an image that is the bookmark icon
                                lvitem.MyImages.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // No image existing
                    // Create if necessary
                    if (bBookmark)
                    {
                        ArrayList iconList = new ArrayList();
                        iconList.Add(imgBook);
                        lvitem.MyImages = iconList;
                    }
                }
                
            }

        }

        private OfflineCacheData UpdateOfflineIcons(EXListViewItem item, String code)
        {
            OfflineCacheData ocd = null;
            EXImageListViewItem lvitem = item as EXImageListViewItem;
            if (lvitem != null)
            {
                Image img = _imgNothing;
                if (_od._OfflineData.ContainsKey(code))
                {
                    ocd = _od._OfflineData[code];
                    if (String.IsNullOrEmpty(ocd._Comment))
                    {
                    	// Pas de note
                    	if (ocd._NotDownloaded)
                    	{
                    		// jamais cherché à téléchargé
                    		// aucune image donc
                    	}
                    	else
                    	{
                    		if (/*(ocd._ImageFilesFromDescription.Count != 0) ||*/ (ocd._ImageFilesSpoilers.Count != 0))
                    		{
                    			// On a des spoilers
                    			img = _imgSpoiler;
                    		}
                    		else
                    		{
                    			// Pas de spoiler
                    			img = _imgNoSpoiler;
                    		}
                    	}
                    }
                    else
                    {
                    	// On a une note
                    	if (ocd._NotDownloaded)
                    	{
                    		// jamais cherché à téléchargé
                    		img = _imgNote;
                    	}
                    	else
                    	{
                    		if (/*(ocd._ImageFilesFromDescription.Count != 0) ||*/ (ocd._ImageFilesSpoilers.Count != 0))
                    		{
                    			// On a des spoilers
                    			img = _imgNoteSpoiler;
                    		}
                    		else
                    		{
                    			// Pas de spoiler
                    			img = _imgNoteNoSpoiler;
                    		}
                    	}
                    }
                }
                lvitem.MyImage = img;
            }
            return ocd;
        }

        /// <summary>
        /// Redraw list view of caches
        /// </summary>
        /// <param name="list">List of objects to be added to the list</param>
        public void PopulateListViewCacheAddList(List<EXImageListViewItem> list)
        {
            lvGeocaches.BeginUpdate();

            // Deactivate item sorter
            lvGeocaches.ListViewItemSorter = null;

            CacheFilter otherFilterToUse = null;
            if (_filterOverride != null)
            {
            	CacheFilter[] flts = { Filter, _filterOverride};
            	otherFilterToUse = new ChainedFiltersAND(flts);
            }
            
            // Populate
            foreach (EXListViewItem item in list)
            {
                Geocache cache = _caches[item.Text];
                if  ((!_bUseFilter) || (_bUseFilter && FilterOrOverride.ToBeDisplayed(cache))) // *** à voir selon les perfos ***
                {
                    lvGeocaches.Items.Add(item);
                }
            }
           
            lvGeocaches.EndUpdate();
        }

        /// <summary>
        /// Redraw list view of caches
        /// </summary>
        /// <param name="forcedList">If provided, force display of these objects, otherwise (if null), recreates all objects from loaded caches</param>
        public void PopulateListViewCache(List<EXListViewItem> forcedList)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            List<Geocache> theCaches = new List<Geocache>();

            lvGeocaches.BeginUpdate();
            lvGeocaches.Items.Clear();
            EmptywbFastCachePreview();

            // Deactivate item sorter
            IComparer sorter = lvGeocaches.ListViewItemSorter;
            lvGeocaches.ListViewItemSorter = null;

            CacheFilter otherFilterToUse = null;
            if (_filterOverride != null)
            {
            	CacheFilter[] flts = { Filter, _filterOverride};
            	otherFilterToUse = new ChainedFiltersAND(flts);
            }
            
            if (forcedList == null)
            {
                foreach (EXListViewItem item in _listViewCaches)
                {
                    Geocache cache = _caches[item.Text];
                    if ((!_bUseFilter) || (_bUseFilter && FilterOrOverride.ToBeDisplayed(cache)))
                    {
                        theCaches.Add(cache);
                        lvGeocaches.Items.Add(item);
                    }
                }
            }
            else
            {
                foreach (EXListViewItem item in forcedList)
                {
                    Geocache cache = _caches[item.Text];
                    theCaches.Add(cache);
                    lvGeocaches.Items.Add(item);
                }
            }

            // Reactivate item sorter
            if (sorter != null)
                lvGeocaches.ListViewItemSorter = sorter;
            else
            {
                int iCol = 7;
                lvGeocaches._sortcol = iCol;
                lvGeocaches.Sorting = SortOrder.Ascending;
                lvGeocaches.Columns[iCol].ImageKey = "up";
                lvGeocaches.ListViewItemSorter = new EXListView.ListViewSubItemComparerValue(iCol, lvGeocaches.Sorting);
            }

            lvGeocaches.EndUpdate();

            // *************************************************************
            // On peuple aussi le cache google maps...
            // Add caches to the list
            BuildCacheMapNew(theCaches);
            // *************************************************************


            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value. 
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds);
            String ss = GetTranslator().GetString("SSDisplayElts");

            int newNb = lvGeocaches.Items.Count;
            
            if ((_iNbCachesDisplayedInListview == 0) && (newNb != 0))
            {
            	// On dégrise
            	EnableDisableMenuEntries(_menuEntriesRequiringDisplayedCaches, true, "DisplayedNo");
            	// La toolbar
            	EnableDisableToolbar();
            }
            else if ((_iNbCachesDisplayedInListview != 0) && (newNb == 0))
            {
            	// On grise
            	EnableDisableMenuEntries(_menuEntriesRequiringDisplayedCaches, false, "DisplayedNo");
            	// La toolbar
            	EnableDisableToolbar();
            }
            
            // sinon c'est juste un changement de nombre et on ne fait rien
            _iNbCachesDisplayedInListview = newNb;
            
            toolStripStatusLabel1.Text = String.Format(ss, _iNbCachesDisplayedInListview.ToString(), elapsedTime);

        }

        private void CheckMarkerValidity(GMapMarker marker)
        {
            if (marker == null)
            {
                // Shit, this shouldn't be !!!
                if (!_bForceClose)
                {
                    UpdateConfFile("MapProvider", "");
                    String msg = String.Format(GetTranslator().GetString("ErrMajorProviderFailure"),
                        _cacheDetail._gmap.MapProvider.Name,
                        _cacheDetail._gmap.MapProvider.DbId.ToString());
                    msg = msg.Replace("#", "\r\n");
                    MsgActionError(this, msg, false);
                    this._bForceClose = true;
                }
                this.Close();
            }
            else
                return;
        }

       /// <summary>
       /// Display a cache on all the maps
       /// </summary>
       /// <param name="cache">cache to display</param>
       /// <param name="overlay">overlay for markers on map preview</param>
       /// <param name="overlaybigview">overlay for markers on map display</param>
       /// <param name="overlaywpts">overlay for waypoints on map preview</param>
       /// <param name="onlypureicons">if true display only pure icons</param>
       /// <returns>List of the two create markers if available, can be null</returns>
       public GMapMarkerImages[] DisplayCacheOnMaps(Geocache cache, GMapOverlay overlay, GMapOverlay overlaybigview, GMapOverlay overlaywpts, bool onlypureicons = false)
        {
        	Waypoint w = null;
            GMapMarkerImages marker = null;
            int indexZoomHigh;
            double wlat, wlon;
        	int z2, z3, z4;
            GMapMarkerImages[] createdmarkers = { null, null };

        	// On crée et affiche les markers des caches
            if (!onlypureicons && cache.IsFound())
            {
                indexZoomHigh = _lstIndexForOptimizedMap["Found"];
            }
            else if (!onlypureicons && cache._bOwned)
            {
                indexZoomHigh = _lstIndexForOptimizedMap["Owned"];
            }
            else
            {
                try
                {
                    indexZoomHigh = _lstIndexForOptimizedMap[cache._Type];
                }
                catch (Exception)
                {
                    indexZoomHigh = _lstIndexForOptimizedMap["UnsupportedCacheType"];
                }
            }

            // Le marker cache d'aperçu
            z4 = indexZoomHigh + 3 * _iOffsetIndexForOptimizedMap; // zoom micro
            z3 = indexZoomHigh + 2 * _iOffsetIndexForOptimizedMap; // zoom min
            z2 = indexZoomHigh + _iOffsetIndexForOptimizedMap; // zoom med
            if (overlay != null)
            {
	            marker = GMapWrapper.gmapMarkerWithImages(
	                overlay,
	                _lstImagesForOptimizedMap,
	                z4, // zoom micro
	                z3, // zoom min
	                z2, // zoom med
	                indexZoomHigh, // zoom high
	                cache._dLatitude,
	                cache._dLongitude,
	                cache,
	                this);
                createdmarkers[0] = marker;
            }

            // le marker cache de grande vue
            if (overlaybigview != null)
            {
	            marker = GMapWrapper.gmapMarkerWithImages(
	                overlaybigview,
	                _lstImagesForOptimizedMap,
	                z4, // zoom micro
	                z3, // zoom min
	                z2, // zoom med
	                indexZoomHigh, // zoom high
	                cache._dLatitude,
	                cache._dLongitude,
	                cache,
	                this);
                createdmarkers[1] = marker;
            }
            
            // et maintenant les waypoints
            if (overlaywpts != null)
            {
	            Dictionary<String, Waypoint> dicowpts = cache.GetListOfWaypoints();
	            foreach (KeyValuePair<String, Waypoint> paire in dicowpts)
	            {
	                // On simplifie, ce sera un bête marqueur avec une seule image
	                w = paire.Value;
	                //indexZoomHigh = _lstIndexForOptimizedMap[w._type];
	                
	                wlat = MyTools.ConvertToDouble(w._lat);
	                wlon = MyTools.ConvertToDouble(w._lon);
	
	                //z4 = indexZoomHigh + 3 * _iOffsetIndexForOptimizedMap; // zoom micro
	                //z3 = indexZoomHigh + 2 * _iOffsetIndexForOptimizedMap; // zoom min
	                //z2 = indexZoomHigh + _iOffsetIndexForOptimizedMap; // zoom med
	                // Le waypoint d'aperçu
	
	                GMapMarker mwpt = GMapWrapper.gmapMarkerWithImage(
	                    overlaywpts,
	                    _listImagesSized[getIndexImages(w._type)],
	                    wlat,
	                    wlon,
	                    CreateWaypointTooltip(cache, w));
	                if (mwpt != null)
	                    mwpt.Tag = cache;
	            }
            }

            return createdmarkers;
        }
        
       /// <summary>
       /// Redraw list of caches on maps
       /// </summary>
       /// <param name="theCaches">list of caches to redraw</param>
        public void BuildCacheMapNew(List<Geocache> theCaches)
        {
            try
            {
                // On suspend l'affichage
                _cachesPreviewMap.HoldInvalidation = true;
                _cacheDetail._gmap.HoldInvalidation = true;

                // On efface tous les markers précédents dans la vue preview (attention, les waypoints sont dans la vue preview par défaut
                // *********************
                GMapOverlay overlay = _cachesPreviewMap.Overlays[GMapWrapper.MARKERS];
                overlay.Markers.Clear();
                GMapOverlay overlaywpts = _cachesPreviewMap.Overlays[GMapWrapper.WAYPOINTS];
                overlaywpts.Markers.Clear();

                // idem dans cachedetails (là que les markers par défaut)
                // *********************
                GMapOverlay overlaybigview = _cacheDetail._gmap.Overlays[GMapWrapper.MARKERS];
                overlaybigview.Markers.Clear();
                // Et pareil avec les cercles et D/T de cachedetail &co
                _cacheDetail.EmptyAllDecorationOverlays();

                
                int iNb = theCaches.Count;
                if (lvGeocaches.Items.Count != 0)
                {
                    EXListViewItem lstvItem = lvGeocaches.Items[0] as EXListViewItem;
                    String code = lstvItem.Text;
                    Geocache geo = _caches[code];
                    _cachesPreviewMap.Position = new PointLatLng(geo._dLatitude, geo._dLongitude);
                    _cacheDetail._gmap.Position = new PointLatLng(geo._dLatitude, geo._dLongitude);
                }
                else
                {
                    // On se centre, sur la première cache de la liste ou sur la maison si la liste est vide
                    _cachesPreviewMap.Position = new PointLatLng(_dHomeLat, _dHomeLon);
                    _cacheDetail._gmap.Position = new PointLatLng(_dHomeLat, _dHomeLon);
                }

                if (iNb != 0)
                {
                    foreach (Geocache cache in theCaches)
                    {
                    	DisplayCacheOnMaps(cache, overlay, overlaybigview, overlaywpts);
                    }

                    // On restaure l'affichage
                    _cachesPreviewMap.Refresh();
                    _cacheDetail._gmap.Refresh();

                    // On affiche la bonne image
                    cachesPreviewMap_OnMapZoomChanged();
                    _cacheDetail.gmap_OnMapZoomChanged();
                }
            }
            catch (Exception ex)
            {
                // On restaure l'affichage
                _cachesPreviewMap.Refresh();
                _cacheDetail._gmap.Refresh();
                throw ex;
            }
        }

        private void ReBuildCacheMapPreview(List<Geocache> theCaches)
        {
            Geocache c = null;
            Waypoint w = null;
            try
            {
                // On suspend l'affichage
                _cachesPreviewMap.HoldInvalidation = true;
                
                // On efface tous les markers précédents dans la vue preview (attention, les waypoints sont dans la vue preview par défaut)
                // *********************
                GMapOverlay overlaywpts = _cachesPreviewMap.Overlays[GMapWrapper.WAYPOINTS];
                overlaywpts.Markers.Clear();

                int iNb = theCaches.Count;

                if (iNb != 0)
                {
                    double wlat, wlon;
                    foreach (Geocache cache in theCaches)
                    {
                        c = cache;
                        // et maintenant les waypoints
                        Dictionary<String, Waypoint> dicowpts = c.GetListOfWaypoints();
                        foreach (KeyValuePair<String, Waypoint> paire in dicowpts)
                        {
                            // On simplifie, ce sera un bête marqueur avec une seule image
                            w = paire.Value;
                            
                            wlat = MyTools.ConvertToDouble(w._lat);
                            wlon = MyTools.ConvertToDouble(w._lon);

                            // Le waypoint d'aperçu
                            GMapMarker mwpt = GMapWrapper.gmapMarkerWithImage(
                                overlaywpts,
                                _listImagesSized[getIndexImages(w._type)],
                                wlat,
                                wlon,
                                CreateWaypointTooltip(cache, w));
                            mwpt.Tag = cache;
                        }
                    }

                    // On restaure l'affichage
                    _cachesPreviewMap.Refresh();
                    
                    // On affiche la bonne image
                    cachesPreviewMap_OnMapZoomChanged();
                }
            }
            catch (Exception ex)
            {
                // On restaure l'affichage
                _cachesPreviewMap.Refresh();
                throw ex;
            }
        }

        private int AddColumn(int icol, string code, int width)
        {
            _dicoColumns.Add(icol, code);
            EXColumnHeader col = new EXColumnHeader(GetTranslator().GetString(code), width);
            col.Tag = width;
            lvGeocaches.Columns.Add(col);
            return icol;
        }

        private int AddColumnBool(int icol, string code, int width)
        {
            EXBoolColumnHeader boolcol = new EXBoolColumnHeader(GetTranslator().GetString(code), width);
            boolcol.TrueImage = _listImagesSized[getIndexImages("Selected")];
            boolcol.FalseImage = _listImagesSized[getIndexImages("NotSelected")];
            boolcol.Tag = width;
            _dicoColumns.Add(icol, code);
            lvGeocaches.Columns.Add(boolcol);

            return icol;
        }


        private void LoadAndApplyDisplayOrder()
        {
            String orderColumns = ConfigurationManager.AppSettings["ordercolumns"];
            if (orderColumns == "")
                return;
            try
            {
                List<string> cols = orderColumns.Split(';').ToList<string>();
                if (cols != null)
                {
                    foreach (String col in cols)
                    {
                        if (col != "")
                        {
                            string[] order = col.Split('|');
                            if ((order != null) && (order.Length == 3))
                            {
                                int ipos = GetColumnIndex(order[0]);
                                if (ipos != -1)
                                {
                                    // Display index
                                    lvGeocaches.Columns[ipos].DisplayIndex = Int32.Parse(order[1]);

                                    // With
                                    int w = Int32.Parse(order[2]);
                                    if (w != 0)
                                    {
                                        lvGeocaches.Columns[ipos].Width = w;
                                        lvGeocaches.Columns[ipos].Tag = w;
                                    }
                                    // else do nothing
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
               // RAZ !!!
                UpdateConfFile("ordercolumns", "");
            }
            lvGeocaches.Invalidate();
        }

        private void SaveColumnsDisplayOrder(ColumnReorderedEventArgs eo)
        {
            String conf = "";
            foreach (KeyValuePair<int, string> paire in _dicoColumns)
            {
                int i = lvGeocaches.Columns[paire.Key].DisplayIndex;
                int w = lvGeocaches.Columns[paire.Key].Width;
                if (w == 0)
                    w = Int32.Parse(lvGeocaches.Columns[paire.Key].Tag.ToString());
                else
                    lvGeocaches.Columns[paire.Key].Tag = w;
                if ((eo != null) && (eo.Header.Index == paire.Key))
                    i = eo.NewDisplayIndex;
                /*
                if ((ew != null) && (ew.ColumnIndex == paire.Key))
                    i = ew.NewWidth;
                */
                conf += paire.Value + "|" + i.ToString() + "|" + w.ToString() + ";";
            }
            UpdateConfFile("ordercolumns", conf);
        }

        private void lstv_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            SaveColumnsDisplayOrder(null);
        }

        private void lstv_ColumnReordered(object sender, ColumnReorderedEventArgs e)
        {
            if (e.OldDisplayIndex == 0)
                e.Cancel = true;
            else
            {
                // save the new order
                SaveColumnsDisplayOrder(e);
            }
        }

        int _ID_LVCode = 0;
        int _ID_LVSel = 0;
        int _ID_LVType = 0;
        int _ID_LVName = 0;
        int _ID_LVContainer = 0;
        int _ID_LVDifficulty = 0;
        int _ID_LVTerrain = 0;
        int _ID_LVDistance = 0;
        /// <summary>
        /// 
        /// </summary>
        public int _ID_LVPlaced = 0;
        int _ID_LVLastlog = 0;
        int _ID_LVTBGC = 0;
        int _ID_LVAvailable = 0;
        int _ID_LVAttributes = 0;
        int _ID_LVAttributesText = 0;
        int _ID_LVOwner = 0;
        int _ID_LVState = 0;
        int _ID_LVCountry = 0;
        int _ID_LVHint = 0;
        /// <summary>
        /// 
        /// </summary>
        public int _ID_LVTag = 0;
        int _ID_LVFavs = 0;
        int _ID_LVRating = 0;
        int _ID_LVAlti = 0;
        int _ID_LVFoundDNF = 0;
        
        private void InitListViewCache()
        {
            lvGeocaches.MySortBrush = SystemBrushes.ControlLight;
            lvGeocaches.MyHighlightBrush = Brushes.Goldenrod;
            lvGeocaches.MySelectBrush = Brushes.PaleGreen;
            lvGeocaches.GridLines = true;
            lvGeocaches.ControlPadding = 4;
            //lstv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom))));
            lvGeocaches.SmallImageList = _listImagesFoo;
            

            // Allow the user to rearrange columns EXCEPT BLOODY COLUMN #0 !!!(GCCode).
            // Because for now the column #0 owner draw is fucked up !!!
            lvGeocaches.ColumnReordered += new System.Windows.Forms.ColumnReorderedEventHandler(lstv_ColumnReordered);
            lvGeocaches.ColumnWidthChanged += new System.Windows.Forms.ColumnWidthChangedEventHandler(lstv_ColumnWidthChanged);
            lvGeocaches.AllowColumnReorder = true;
            lvGeocaches.SelectedIndexChanged += lstv_SelectionChanged;

            //add columns
            _ID_LVCode = AddColumn(0, "LVCode", 100); // #0
            _ID_LVSel = AddColumnBool(1, "LVSel", 25); // #1
            _ID_LVType = AddColumn(2, "LVType", 45); // #2
            _ID_LVName = AddColumn(3, "LVName", 250); // #3
            _ID_LVContainer = AddColumn(4, "LVContainer", 60); // #4
            _ID_LVDifficulty = AddColumn(5, "LVDifficulty", 65); // #5
            _ID_LVTerrain = AddColumn(6, "LVTerrain", 65); // #6
            _ID_LVDistance = AddColumn(7, "LVDistance", 50); // #7
            _ID_LVPlaced = AddColumn(8, "LVPlaced", 100); // #8
            _ID_LVLastlog = AddColumn(9, "LVLastlog", 100); // #9
            _ID_LVTBGC = AddColumn(10, "LVTBGC", 50); // #10
            _ID_LVAvailable = AddColumn(11, "LVAvailable", 50); // #11
            _ID_LVAttributes = AddColumn(12, "LVAttributes", 400); // #12
            _ID_LVAttributesText = AddColumn(13, "LVAttributesText", 800); // #13
            _ID_LVOwner = AddColumn(14, "LVOwner", 100); // #14
            _ID_LVState = AddColumn(15, "LVState", 100); // #15
            _ID_LVCountry = AddColumn(16, "LVCountry", 100); // #16
            _ID_LVHint = AddColumn(17, "LVHint", 300); // #17
            _ID_LVTag = AddColumn(18, "LVTag", 100); // #18
            _ID_LVFavs = AddColumn(19, "LVFavs", 65); // #19
            _ID_LVRating = AddColumn(20, "LVRating", 105); // #20
            _ID_LVAlti = AddColumn(21, "LVAlti", 105); // #21
            _ID_LVFoundDNF = AddColumn(22, "LVFoundDNF", 105); // #22
            
            LoadAndApplyDisplayOrder();
            
            // Now create menu to hide / show column
            // Retrieve list of hiden columns
            String hidenColumns = ConfigurationManager.AppSettings["hidencolumns"];
            foreach (EXColumnHeader col in lvGeocaches.Columns)
            {
            	String coli = col.Index.ToString();
                if (col.Index < 10)
                    coli = "0" + coli;
                
                ToolStripMenuItem item = new ToolStripMenuItem(col.Text);
                item.Click += new System.EventHandler(DisplaySubMenuItem_Click);
                item.Tag = col;
                item.Name = "displayToolStripMenuItem" + coli;
                displayToolStripMenuItem.DropDownItems.Add(item);

                if (hidenColumns.Contains(coli))
                {
                    item.Checked = false;
                    col.Width = 0;
                    // #Fix WinXP
                    col._bHidden = true;
                }
                else
                {
                    item.Checked = true;
                    // #Fix WinXP
                    col._bHidden = false;
                }   
            }

            lvGeocaches.MouseDoubleClick += new MouseEventHandler(lstv_MouseDoubleClick);
            lvGeocaches.MouseClick += new MouseEventHandler(lstv_MouseClick);
        }

        private void DisplaySubMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            EXColumnHeader col = item.Tag as EXColumnHeader;
            String hidenColumns = ConfigurationManager.AppSettings["hidencolumns"];
            String coli = col.Index.ToString();
            if (col.Index < 10)
                coli = "0" + coli;

            if (item.Checked)
            {
                item.Checked = false;
                col.Width = 0;
                // #Fix WinXP
                col._bHidden = true;
                hidenColumns += coli + ";";
            }
            else
            {
                item.Checked = true;
                col.Width = (int)(col.Tag);
                // #Fix WinXP
                col._bHidden = false;
                hidenColumns = hidenColumns.Replace(coli + ";", "");
            }
            UpdateConfFile("hidencolumns", hidenColumns);
            // #Fix WinXP
            if (e != null)
                PopulateListViewCache(null);
        }

        /// <summary>
        /// Create an image with a white background (optional) and image will be resized
        /// </summary>
        /// <param name="bEnable">If true, white background will be created, otherwise image will simply be resized</param>
        /// <param name="type">image key</param>
        /// <param name="resize1">new squared size of image</param>
        /// <param name="resize2">new squared size of white background</param>
        /// <returns>new image</returns>
        public Image GetImageWithWhiteBackground(bool bEnable, String type, int resize1, int resize2)
        {
            if (bEnable)
            {
                Image i1 = _listImagesSized[getIndexImages(type)];
                if (resize1 != -1)
                    i1 = MyTools.ResizeImage(i1, resize1, resize1);

                Image i2 = _listImagesSized[getIndexImages("White")];
                if (resize2 != -1)
                    i2 = MyTools.ResizeImage(i2, resize2, resize2);

                return MyTools.MergeTwoImages(i1, i2);
            }
            else
            {
                Image i1 = _listImagesSized[getIndexImages(type)];
                if (resize1 != -1)
                    i1 = MyTools.ResizeImage(i1, resize1, resize1);

                return i1;
            }
        }

        private void InitImageList()
        {
            _imgAttributes = new Dictionary<string, Image>();
            _imgAttributesGreyed = new Dictionary<string, Image>();
            _listImagesSized = new List<Image>();
            _indexImages = new Dictionary<string, int>();
            _indexImagesLowerKey = new Dictionary<string, int>();
            
            ParseImageFolder("", "*.gif", false);
            ParseImageFolder("", "*.png", false);
            ParseImageFolder("Wpts", "*.gif", false);
            ParseImageFolder("Type", "*.gif", false);
            ParseImageFolder("Size", "*.gif", false);
            ParseImageFolder("Log", "*.gif", false);
            ParseImageFolder("Star", "*.gif", false);
            ParseImageFolder("Carto", "*.png", false);
            ParseImageFolder("Reflection", "*.png", false);
            ParseImageFolder("Attribute", "*.gif", true);
            ParseImageFolder("AttributeGreyed", "*.gif", true);
            ParseImageFolder("Ratios", "*.png", false);
            ParseImageFolder("TypeCat", "*.png", false);
            ParseImageFolder("Tab", "*.png", false);
            

            _listImagesFoo = new ImageList();
            _listImagesFoo.ColorDepth = ColorDepth.Depth32Bit;
            _listImagesFoo.ImageSize = new Size(32, 32); // this will affect the row height
            _listImagesFoo.Images.Add("down", _listImagesSized[getIndexImages("Down")]);
            _listImagesFoo.Images.Add("up", _listImagesSized[getIndexImages("Up")]);
            _listImagesFoo.Images.Add("Owned", _listImagesSized[getIndexImages("Owned")]);

            // Construction de la liste des icones pour l'affichage optimisé de Gmaps
            // **********************************************************************
            // Plein taille 32x32
            // Taille moyenne 16x16
            // Taille mini 12x12
            int index = 0;
            List<String> supportedTypes = GeocachingConstants.GetSupportedCacheTypes();
            List<String> supportedWptTypes = GeocachingConstants.GetSupportedWaypointsType();
            foreach(String s in supportedTypes)
            {
            	_lstIndexForOptimizedMap.Add(s, index++);
            }
            _lstIndexForOptimizedMap.Add("Found", index++);
            _lstIndexForOptimizedMap.Add("Owned", index++);
            foreach(String s in supportedWptTypes)
            {
            	_lstIndexForOptimizedMap.Add(s, index++);
            }
            _lstIndexForOptimizedMap.Add("UnsupportedCacheType", index++);
            
            int iSize;
            iSize = 20;
            bool forcewhitebackground = false; // au lieu de true
            foreach(String s in supportedTypes)
            {
            	_lstImagesForOptimizedMap.Add(GetImageWithWhiteBackground(forcewhitebackground,s,iSize,iSize));
            }
            _lstImagesForOptimizedMap.Add(GetImageWithWhiteBackground(forcewhitebackground, "Found", iSize, iSize));
            _lstImagesForOptimizedMap.Add(GetImageWithWhiteBackground(forcewhitebackground, "Owned", iSize, iSize));
            foreach(String s in supportedWptTypes)
            {
            	_lstImagesForOptimizedMap.Add(_listImagesSized[getIndexImages(s)]);
            }
            _lstImagesForOptimizedMap.Add(GetImageWithWhiteBackground(true, "UnsupportedCacheType", iSize, iSize));
            // ATTENTION : si ajouter une ligne, màj de _lstIndexForOptimizedMap AUSSI !!!!!

            // les autres tailles
            // ******************
            _iOffsetIndexForOptimizedMap = _lstImagesForOptimizedMap.Count();
            Dictionary<String, String> _dicoTypeCategorySmall = _geocachingConstants.GetDicoTypeSmallIcon();
            
            iSize = 16; // Au lieu de 20
            foreach(String s in supportedTypes)
            {
            	_lstImagesForOptimizedMap.Add(_listImagesSized[getIndexImages(_dicoTypeCategorySmall[s])]);
            }
            _lstImagesForOptimizedMap.Add(GetImageWithWhiteBackground(_bEnableWhiteBackground, "Found", iSize, -1));
            _lstImagesForOptimizedMap.Add(GetImageWithWhiteBackground(_bEnableWhiteBackground, "Owned", iSize, -1)); 
            foreach(String s in supportedWptTypes)
            {
            	_lstImagesForOptimizedMap.Add(_listImagesSized[getIndexImages(s)]);
            }
            _lstImagesForOptimizedMap.Add(_listImagesSized[getIndexImages("hctx")]); // UnsupportedCacheType
            /*
 			iSize = 20;
            foreach(String s in supportedTypes)
            {
            	_lstImagesForOptimizedMap.Add(GetImageWithWhiteBackground(_bEnableWhiteBackground, s, iSize, -1));
            }
            _lstImagesForOptimizedMap.Add(GetImageWithWhiteBackground(_bEnableWhiteBackground, "Found", iSize, -1));
            _lstImagesForOptimizedMap.Add(GetImageWithWhiteBackground(_bEnableWhiteBackground, "Owned", iSize, -1)); 
            foreach(String s in supportedWptTypes)
            {
            	_lstImagesForOptimizedMap.Add(_listImagesSized[getIndexImages(s)]);
            }
            _lstImagesForOptimizedMap.Add(GetImageWithWhiteBackground(_bEnableWhiteBackground, "UnsupportedCacheType", iSize, -1));
            */
            

            // Les groupes moyens
            // ******************
            iSize = 13; // au lieu de 8
            // Les cat font 13x13 en moyenne
            Dictionary<String, String> _dicoTypeCategory = _geocachingConstants.GetDicoTypeCategory();
            foreach(String s in supportedTypes)
            {
            	_lstImagesForOptimizedMap.Add(_listImagesSized[getIndexImages(_dicoTypeCategory[s])]);
            }
            _lstImagesForOptimizedMap.Add(MyTools.ResizeImage(_listImagesSized[getIndexImages("Found")], iSize, iSize));  
            _lstImagesForOptimizedMap.Add(MyTools.ResizeImage(_listImagesSized[getIndexImages("Owned")], iSize, iSize));   
			foreach(String s in supportedWptTypes)
            {
            	_lstImagesForOptimizedMap.Add(null); // waypoint
            }    
            _lstImagesForOptimizedMap.Add(_listImagesSized[getIndexImages("cat8")]); // UnsupportedCacheType
            

            // Les petits groupes
            // ******************
            iSize = 8; // au lieu de 4
            // les mcat font 8x8
            foreach(String s in supportedTypes)
            {
            	_lstImagesForOptimizedMap.Add(_listImagesSized[getIndexImages("m" + _dicoTypeCategory[s])]);
            }
            _lstImagesForOptimizedMap.Add(MyTools.ResizeImage(_listImagesSized[getIndexImages("Found")], iSize, iSize));
            _lstImagesForOptimizedMap.Add(MyTools.ResizeImage(_listImagesSized[getIndexImages("Owned")], iSize, iSize));
            foreach(String s in supportedWptTypes)
            {
            	_lstImagesForOptimizedMap.Add(null); // waypoint
            }    
            _lstImagesForOptimizedMap.Add(_listImagesSized[getIndexImages("mcat8")]); // UnsupportedCacheType
            

            // Les combinaisons DT
            _dicoDT = CreateListImageDTRed();

            CreateAttributeIconsHMINew(ref _listPbIn, tabPage13AttIn, true);
            CreateAttributeIconsHMINew(ref _listPbOut, tabPage14AttOut, false);
        }

        /// <summary>
        /// Create static list of D/T images combinations
        /// </summary>
        /// <returns>list of D/T images combinations</returns>
        private Dictionary<String,Image> CreateListImageDTRed()
        {
            Dictionary<String, Image> dico = new Dictionary<string, Image>();
            
            
        	// Des jolis disques de couleur
        
            Dictionary<int, Color> dicot = new Dictionary<int, Color>();
            int alpha = 125;
            String stype = ConfigurationManager.AppSettings["dtonmapgradient"];
            
            // RGB gradient
            if (stype == "1")
            {
	            dicot.Add(10, Color.FromArgb(alpha, 0, 255, 0));
	            dicot.Add(15, Color.FromArgb(alpha, 31, 223, 0));
	            dicot.Add(20, Color.FromArgb(alpha, 63, 191, 0));
	            dicot.Add(25, Color.FromArgb(alpha, 95, 159, 0));
	            dicot.Add(30, Color.FromArgb(alpha, 127, 127, 0));
	            dicot.Add(35, Color.FromArgb(alpha, 159, 95, 0));
	            dicot.Add(40, Color.FromArgb(alpha, 191, 63, 0));
	            dicot.Add(45, Color.FromArgb(alpha, 223, 31, 0));
	            dicot.Add(50, Color.FromArgb(alpha, 255, 0, 0));
            }
            else if (stype == "2")
            {
	           	// HSV gradient 
	            dicot.Add(10, Color.FromArgb(alpha, 0, 255, 0));
	            dicot.Add(15, Color.FromArgb(alpha, 0, 255, 127));
	            dicot.Add(20, Color.FromArgb(alpha, 0, 255, 255));
	            dicot.Add(25, Color.FromArgb(alpha, 0, 127, 255));
	            dicot.Add(30, Color.FromArgb(alpha, 0, 0, 255));
	            dicot.Add(35, Color.FromArgb(alpha, 127, 0, 255));
	            dicot.Add(40, Color.FromArgb(alpha, 255, 0, 255));
	            dicot.Add(45, Color.FromArgb(alpha, 255, 0, 127));
	            dicot.Add(50, Color.FromArgb(alpha, 255, 0, 0));
            }
            else // 3
            {
	            // HSV gradient inverse
	            dicot.Add(10, Color.FromArgb(alpha, 0, 255, 0));
	            dicot.Add(15, Color.FromArgb(alpha, 63, 255, 0));
	            dicot.Add(20, Color.FromArgb(alpha, 127, 255, 0));
	            dicot.Add(25, Color.FromArgb(alpha, 191, 255, 0));
	            dicot.Add(30, Color.FromArgb(alpha, 255, 255, 0));
	            dicot.Add(35, Color.FromArgb(alpha, 255, 191, 0));
	            dicot.Add(40, Color.FromArgb(alpha, 255, 127, 0));
	            dicot.Add(45, Color.FromArgb(alpha, 255, 63, 0));
	            dicot.Add(50, Color.FromArgb(alpha, 255, 0, 0));
            }

            // Avec les disques,
            // D = rayon = 30 + (d-1)*10
           // String msg = "";
            for (int indexd = 10; indexd <= 50; indexd += 5)
            {
                String d = "d" + indexd.ToString();
                String keyd = indexd.ToString().Replace("0", "");
                if (keyd.Length == 2)
                    keyd = keyd.ToString().Insert(1, ".");
                
                int r = 20 + indexd;
                for (int indext = 10; indext <= 50; indext += 5)
                {
                    String t = "t" + indext.ToString();
                    String keyt = indext.ToString().Replace("0", "");
                    if (keyt.Length == 2)
                       keyt= keyt.ToString().Insert(1, ".");

                    Bitmap dti = new Bitmap(r, r);
                    Graphics canvas = Graphics.FromImage(dti);
                    SolidBrush brush = new SolidBrush(dicot[indext]);
                    canvas.FillEllipse(brush, 0, 0, r, r);
                    canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
            		canvas.Save();
                    
                    // On recréé les index
                    String key = keyd + keyt;
                    dico.Add(key, dti);
                    //dti.Save("DTC" + key.Replace('.','0') + ".png", ImageFormat.Png);
                    //msg += "_dicoDTPics.Add(\"" + key + "\", Resource.Drawable.DTC" + key.Replace('.', '0') + ");\r\n";
                }
            }
           // MSG(msg);

            return dico;
        }

        /// <summary>
        /// Parse a folder, looking for images and add them to list of usable images for MENUS
        /// Image filename without extension will be the key to retrieve it
        /// </summary>
        /// <param name="folder">folder path</param>
        /// <param name="filter">image filter (e.g. "*.jpg")</param>
        /// <param name="bLower">if true, key will be lower case </param>
        public void ParseImageFolderForMenus(String folder, String filter, bool bLower)
        {
            String imgpath = GetResourcesDataPath() + Path.DirectorySeparatorChar + "Img";
            String searchpath = imgpath;

            if (folder != "")
            {
                searchpath += Path.DirectorySeparatorChar + folder;
            }
            string[] filePaths = Directory.GetFiles(searchpath, filter);
            foreach (string f in filePaths)
            {
                String key = Path.GetFileNameWithoutExtension(f);
                if (bLower)
                    key = key.ToLower();
                
                try
                {
                	Image img = Image.FromFile(f);
                	_imgMenus.Add(key, img);
                	//Log("Adding " + key);
                }
                catch (Exception exc)
                {
                    Log("!!!! " + GetException("Build attributes image list MENUS", exc));
                    Log("!!!! Error inserting image : " + key);
                }
                //Log(index.ToString() + " : " + key);
            }
        }
        
        /// <summary>
        /// Parse a folder, looking for images and add them to list of usable images
        /// Image filename without extension will be the key to retrieve it
        /// </summary>
        /// <param name="folder">folder path</param>
        /// <param name="filter">image filter (e.g. "*.jpg")</param>
        /// <param name="bLower">if true, key will be lower case </param>
        public void ParseImageFolder(String folder, String filter, bool bLower)
        {
            String imgpath = GetResourcesDataPath() + Path.DirectorySeparatorChar + "Img";
            String searchpath = imgpath;

            bool bAttributes = false;
            bool bAttributesGreyed = false;
            if (folder == "Attribute")
                bAttributes = true;
            if (folder == "AttributeGreyed")
                bAttributesGreyed = true;

            if (folder != "")
            {
                searchpath += Path.DirectorySeparatorChar + folder;
            }
            string[] filePaths = Directory.GetFiles(searchpath, filter);
            foreach (string f in filePaths)
            {
                String key = Path.GetFileNameWithoutExtension(f);
                if (bLower)
                    key = key.ToLower();
                // Insert image
                //_listImagesFoo.Images.Add(Image.FromFile(f));
                try
                {
                    Image img = Image.FromFile(f);
                    
                    _listImagesSized.Add(img);
                    int index = _listImagesSized.Count - 1;
                    _indexImages.Add(key, index);
                    if (!bLower)
                    {
                        _indexImagesLowerKey.Add(key.ToLower(), index);
                    }

                    if (bAttributes)
                    {
                        _imgAttributes.Add(key, img);
                    }
                    else if (bAttributesGreyed)
                    {
                        _imgAttributesGreyed.Add(key, img);
                    }
                }
                catch (Exception exc)
                {
                    Log("!!!! " + GetException("Build attributes image list", exc));
                    Log("!!!! Error inserting image : " + key);
                }
                //Log(index.ToString() + " : " + key);
            }
        }

        /// <summary>
        /// Log an object into the log file (if activated)
        /// </summary>
        /// <param name="o">object to log</param>
        public void Log(Object o)
        {
            if (o == null)
                Log("<Null>");
            else
                Log(o.ToString());
        }

        /// <summary>
        /// Log a string into the log file (if activated)
        /// </summary>
        /// <param name="txt">text to log</param>
        public void Log(String txt)
        {
            if (_log != null)
            {
            	/*
            	StackFrame[] frames = new System.Diagnostics.StackTrace().GetFrames();
            	if (frames.Count() >= 2)
            		_log.WriteLine(DateTime.Now.ToString("MM-dd HH:mm:ss.fff ") + frames[1].GetMethod().ToString() + " " + txt);
            	else*/
            		_log.WriteLine(DateTime.Now.ToString("MM-dd HH:mm:ss.fff ") + txt);
                _log.Flush();
            }
        }

        private void OpenLog()
        {
            if (ConfigurationManager.AppSettings["logfile"] != "")
            {
                String exePath = Path.GetDirectoryName(Application.ExecutablePath);
                _log = new System.IO.StreamWriter(exePath + Path.DirectorySeparatorChar + ConfigurationManager.AppSettings["logfile"], true);
                Log("*********************** START NEW INSTANCE **************************");
            }
            else
            {
                _log = null;
            }
        }

        private void CloseLog()
        {
            if (_log != null)
            {
                Log("$$$$$$$$$$$$$$$$$$$$$$$ TERMINATE INSTANCE $$$$$$$$$$$$$$$$$$$$$$$$$$");
                _log.Close();
                _log = null;
            }
        }

        /// <summary>
        /// Assign loaded waypoints to loaded geocaches
        /// </summary>
        public void JoinWptsGC()
        {
            try
            {
                Log("JoinWptsGC");
                Log("--- _waypointsLoaded ---");
                // Jointure des vrais waypoints d'abord, issus de GPX
                foreach (KeyValuePair<String, Waypoint> paire in _waypointsLoaded)
                {
                    String wptname = paire.Key;
                    Waypoint wpt = paire.Value;
                    String gcparent = wpt._GCparent;

                    // Do we have the parent ?
                    if (_caches.ContainsKey(gcparent))
                    {
                        // Get daddy
                        Geocache cache = _caches[gcparent];
                        if (cache._waypoints.ContainsKey(wptname))
                        {
                            // Already existing. Newer ?
                            Waypoint oldwpt = cache._waypoints[wptname];
                            //if (oldwpt._DateExport < wpt._DateExport)
                            // On compare les dates des waypoints, c'est plus sain !
                            DateTime existingdate = MyTools.ParseDate(oldwpt._time);
                			DateTime newtime = MyTools.ParseDate(wpt._time);
                			if (existingdate < newtime)
                            {
                                // We replace it
                                cache._waypoints[wptname] = wpt;
                                wpt._eOrigin = Waypoint.WaypointOrigin.GPX;
                            }
                        }
                        else
                        {
                            // We add it
                            cache._waypoints.Add(wptname, wpt);
                            wpt._eOrigin = Waypoint.WaypointOrigin.GPX;
                        }
                    }
                }

                Log("--- _waypointsMGM ---");
                // Jointure des waypoints issus de MGM
                foreach (KeyValuePair<String, Waypoint> paire in _waypointsMGM)
                {
                    String wptname = paire.Key;
                    Waypoint wpt = paire.Value;
                    String gcparent = wpt._GCparent;

                    // Do we have the parent ?
                    Log(wptname + " Looking for parent cache " + gcparent);
                    if (_caches.ContainsKey(gcparent))
                    {
                    	Log(" Cache found");
                        // Get daddy
                        Geocache cache = _caches[gcparent];

                        // Est-ce qu'un waypoint du meme code existe déjà dans la cache
                        if (cache._waypoints.ContainsKey(wptname))
                        {
                        	Log(" found existing waypoint");
                            // Already existing. Newer ?
                            Waypoint oldwpt = cache._waypoints[wptname];
                            Log(" oldwpt._time " + oldwpt._time);
                            Log(" wpt._time " + wpt._time);
                            
                            //if (oldwpt._DateExport < wpt._DateExport)
                            DateTime existingdate = MyTools.ParseDate(oldwpt._time);
                			DateTime newtime = MyTools.ParseDate(wpt._time);
                			if (existingdate < newtime)
                            {
                            	Log(" waypoint is newer");
                            	Log(" cache._waypoints[wptname]._eOrigin : " + cache._waypoints[wptname]._eOrigin);
                                // We replace it
                                // On a donc déjà un waypoint dans la cache qui est plus vieux
                                if (cache._waypoints[wptname]._eOrigin == Waypoint.WaypointOrigin.GPX)
                                {
                                    // On remplace un waypoint déjà existant issu d'un GPX !
                                    // Le nouveau wpt est donc MODIFIED
                                    Log(" waypoint origin modified to MODIFIED");
                                    wpt._eOrigin = Waypoint.WaypointOrigin.MODIFIED;
                                }
                                // On l'ajoute dans les waypoints MGM !!!
                                Log(" replaced waypoint");
                                cache._waypointsFromMGM[wptname] = wpt;

                                // Custom ! On marque la cache
                                _iNbModifiedCaches += cache.InsertModification("WPT_" + wptname);
                            }
                        }
                        else
                        {
                        	Log(" new waypoint added");
                            // We add it
                            // Il n'existait pas dans la liste des caches, c'est donc une creation utilisateur
                            wpt._eOrigin = Waypoint.WaypointOrigin.CUSTOM;
                            cache._waypointsFromMGM.Add(wptname, wpt);
                            // Custom ! On marque la cache comme modifiée
                            _iNbModifiedCaches += cache.InsertModification("WPT_" + wptname);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log(GetException("Joining Waypoints", ex));
                throw;
            }
        }

        private void LoadDB()
        {
            // Init list of caches
            _caches = new Dictionary<string, Geocache>();
            _waypointsLoaded = new Dictionary<String, Waypoint>();

            // On va charger no waypoints MGM
            LoadMGMWayPoints();

            // check if command lines parameters where provided
            string[] args = Environment.GetCommandLineArgs();
            if (args.Count() > 1) // 1st arg is executable name !
            {
                for (int i = 1; i < args.Count(); i++)
                {
                    String f = args[i];
                    // do stuff
                    try
                    {
                        if (f.ToLower().EndsWith(".gpx"))
                        {
                            LoadFile(f);
                            _LoadedOriginalFiles.Add(f);
                        }
                        else if (f.ToLower().EndsWith(".zip"))
                        {
                            LoadZip(f);
                        }
                        else if (f.ToLower().EndsWith(".ggz"))
                        {
                            LoadGgz(f);
                        }

                    }
                    catch (Exception exc)
                    {
                        Log("!!!! " + GetException("Loading gpx/zip/ggz", exc));
                        Log("!!!! Failed loading " + f);
                        _errorMessageLoad += GetTranslator().GetString("ErrorLoad") + " " + f + "\r\n";
                    }
                    Log("Number of cache in DB: " + _caches.Count.ToString());
                }
            }
            else
            {
                // Get path for database
                String pathdb = GetUserDataPath() + Path.DirectorySeparatorChar + "GPX";

                List<String> ignoredfiles = StartupFilesConfig.GetListOfExcludedFiles();

                // Parse all GPX files
                string[] filePaths = Directory.GetFiles(pathdb, "*.gpx", SearchOption.AllDirectories);
                foreach (string f in filePaths)
                {
                    try
                    {
                        if (ignoredfiles.Contains(StartupFilesConfig.GetKeyFromFilename(pathdb, f)))
                            Log("*** " + f + " is ignored");
                        else
                        {
                            LoadFile(f);
                            _LoadedOriginalFiles.Add(f);
                        }
                    }
                    catch (Exception exc)
                    {
                        Log("!!!! " + GetException("Loading gpx", exc));
                        Log("!!!! Failed loading GPX " + f);
                        _errorMessageLoad += GetTranslator().GetString("ErrorLoad") + " " + f + "\r\n";
                    }
                    Log("Number of cache in DB: " + _caches.Count.ToString());
                }

                // Parse all ZIP files
                filePaths = Directory.GetFiles(pathdb, "*.zip", SearchOption.AllDirectories);
                foreach (string f in filePaths)
                {
                    try
                    {
                        if (ignoredfiles.Contains(StartupFilesConfig.GetKeyFromFilename(pathdb, f)))
                            Log("*** " + f + " is ignored");
                        else
                            LoadZip(f);
                    }
                    catch (Exception exc)
                    {
                        Log("!!!! " + GetException("Loading zip", exc));
                        Log("!!!! Failed loading ZIP " + f);
                        _errorMessageLoad += GetTranslator().GetString("ErrorLoad") + " " + f + "\r\n";
                    }
                    Log("Number of cache in DB: " + _caches.Count.ToString());
                }

                // Parse all GGZ files
                filePaths = Directory.GetFiles(pathdb, "*.ggz", SearchOption.AllDirectories);
                foreach (string f in filePaths)
                {
                    try
                    {
                        if (ignoredfiles.Contains(StartupFilesConfig.GetKeyFromFilename(pathdb, f)))
                            Log("*** " + f + " is ignored");
                        else
                            LoadGgz(f);
                    }
                    catch (Exception exc)
                    {
                        Log("!!!! " + GetException("Loading ggz", exc));
                        Log("!!!! Failed loading GGZ " + f);
                        _errorMessageLoad += GetTranslator().GetString("ErrorLoad") + " " + f + "\r\n";
                    }
                    Log("Number of cache in DB: " + _caches.Count.ToString());
                }
            }

            // Now we join Wpts & Caches
            JoinWptsGC();
            ChangeCacheStatusBasedonMGM();
            PostTreatmentLoadCache();
        }

        /// <summary>
        /// Check single cache status from MGM and set it to found if needed
        /// </summary>
        public void ChangeCacheStatusBasedonMGM(Geocache singlecache)
        {
            String owner = ConfigurationManager.AppSettings["owner"];
            List<String> caches = _cacheStatus.GetCacheFoundFromUser(owner);
            if ((caches != null) && (caches.Contains(singlecache._Code)))
            {
                // On a une cache trouvée.
                // Est-ce qu'elle l'était déjà ?
                _iNbModifiedCaches += singlecache.ManualFound();
            }
            
            // SURTOUT NE PAS AJOUTER MANUELLEMENT LES CACHES CHARGEES !!!
        }

        /// <summary>
        /// Check cache status from MGM and set it to found if needed
        /// </summary>
        private void ChangeCacheStatusBasedonMGM()
        {
        	String owner = ConfigurationManager.AppSettings["owner"];
        	List<String> caches = _cacheStatus.GetCacheFoundFromUser(owner);
        	if (caches != null)
        	{
        		foreach(String c in caches)
        		{
        			if (_caches.ContainsKey(c))
                    {
        				// On a une cache trouvée.
        				// Est-ce qu'elle l'était déjà ?
        				_iNbModifiedCaches += _caches[c].ManualFound();
        			}
        		}
        	}
        	
        	// SURTOUT NE PAS AJOUTER MANUELLEMENT LES CACHES CHARGEES !!!
        }
        
        private void LoadMGMWayPoints()
        {
            try
            {
            	Log("--- LoadMGMWayPoints ---");
            	_waypointsMGM.Clear();
                // _waypoints est forcément vide quand on arrive ici !
                String wptMGM = GetUserDataPath() + Path.DirectorySeparatorChar + "WaypointsMGM.dat";
                if (File.Exists(wptMGM))
                {
                    LoadFile(wptMGM);
                    _LoadedOriginalFiles.Add(wptMGM);
                }

                // _waypointsLoaded contient donc les waypoints lus dans le fichier.
                // On va les sauver dans _waypointsMGM
                foreach (KeyValuePair<String, Waypoint> paire in _waypointsLoaded)
                {
                    paire.Value._eOrigin = Waypoint.WaypointOrigin.CUSTOM;
                    _waypointsMGM.Add(paire.Key,paire.Value);
                }
                Log("Loaded _waypointsMGM : " + _waypointsMGM.Count.ToString());
                // Et on les vire de _waypointsLoaded !
                _waypointsLoaded.Clear();
            }
            catch (Exception ex)
            {
            	Log(GetException("Error loading custom MGM waypoints", ex));
            }
        }

        

        private void LoadWaypoint(DateTime dateExport, XmlNode node)
        {
            // Is it really a waypoint ?
            if (MyTools.getNodeValue(node, "type").Contains("Waypoint") == false)
            {
                return;
            }

            Waypoint wpt = new Waypoint();
            wpt._DateExport = dateExport;
            wpt._name = MyTools.getNodeValue(node, "name");
            wpt._time = MyTools.getNodeValue(node, "time");
            
            // Check if cache will be discarded or not
            bool bDiscard = false;
            if (_waypointsLoaded.ContainsKey(wpt._name))
            {
                // Existing element
                // Newer ?
                Waypoint existingwpt = _waypointsLoaded[wpt._name];

                DateTime existingdate = MyTools.ParseDate(existingwpt._time);
                DateTime newtime = MyTools.ParseDate(wpt._time);
                if (existingdate < newtime)
                //if (existingwpt._DateExport < wpt._DateExport)
                {
                    // Newer element, we keep it
                    _waypointsLoaded[wpt._name] = wpt;
                    bDiscard = false;
                }
                else
                {
                    // Older than already present in the list, ignored
                    bDiscard = true;
                }
            }
            else
            {
                // New element
                _waypointsLoaded.Add(wpt._name, wpt);
                bDiscard = false;
            }
            if (bDiscard)
                return;

            // Ok, we fill the data now
            wpt._lat = MyTools.getAttributeValue(node, "lat");
            wpt._lon = MyTools.getAttributeValue(node, "lon");
            //Log(wpt._lat + "|" + wpt._lon);

            wpt._cmt = MyTools.getNodeValue(node, "cmt");
            wpt._desc = MyTools.getNodeValue(node, "desc");
            wpt._url = MyTools.getNodeValue(node, "url");
            wpt._urlname = MyTools.getNodeValue(node, "urlname");
            wpt._sym = MyTools.getNodeValue(node, "sym");
            String type = MyTools.getNodeValue(node, "type");
            type = type.Replace("Waypoint|", "");
            wpt._type = type;

            // Post treatment
            wpt.PostTreatmentData(_tableWptsTypeTranslated);
        }


        private Waypoint LoadGCWaypoint(DateTime dateExport, XmlNode node)
        {
            // Is it really a waypoint ?
            if (MyTools.getNodeValue(node, "type").Contains("Waypoint") == false)
            {
                return null;
            }

            Waypoint wpt = new Waypoint();
            wpt._DateExport = dateExport;
            wpt._name = MyTools.getNodeValue(node, "name");
            wpt._time = MyTools.getNodeValue(node, "time");
            
            // Ok, we fill the data now
            wpt._lat = MyTools.getAttributeValue(node, "lat");
            wpt._lon = MyTools.getAttributeValue(node, "lon");
           
            wpt._cmt = MyTools.getNodeValue(node, "cmt");
            wpt._desc = MyTools.getNodeValue(node, "desc");
            wpt._url = MyTools.getNodeValue(node, "url");
            wpt._urlname = MyTools.getNodeValue(node, "urlname");
            wpt._sym = MyTools.getNodeValue(node, "sym");
            String type = MyTools.getNodeValue(node, "type");
            type = type.Replace("Waypoint|", "");
            wpt._type = type;

            // Post treatment
            wpt.PostTreatmentData(_tableWptsTypeTranslated);
            return wpt;
        }
        
        /// <summary>
        /// This is the heart of MGM
        /// Load a **GEOCACHING*** GPX file and returns a cache list
        /// Opencaching is NOT supported by this function
        /// Waypoints are DISCARDED by this function
        /// Loaded caches are not added to the list view by this function!
        /// </summary>
        /// <param name="f">GPX file (.gpx)</param>
        /// <param name="localCaches">dictionnary will be populated with caches present in the GPX file (index is cache Code). WARNING: waypoints will not be associated to geocache!</param>
        /// <param name="localWaypoints">list will be populated with waypoints present in the GPX file</param>
        public void LoadGCFile(string f, ref Dictionary<String, Geocache> localCaches, ref List<Waypoint> localWaypoints)
        {
        	String owner = ConfigurationManager.AppSettings["owner"].ToLower();
            bool bIgnoreFound = (ConfigurationManager.AppSettings["ignorefounds"] == "True") ? true : false;

            // Display file name
            Log("Current directory " + Directory.GetCurrentDirectory());
            Log("Loading file (LoadGCFile) " + f);
            String ori = f;

            // Log GPX file
            XmlDocument xmldoc;
            XmlNodeList xmlnode;
            xmldoc = new XmlDocument();
            String sentry = "";
            Geocache geo = null; ;
            try
            {

                xmldoc.Load(f);
                XmlNamespaceManager ns = new XmlNamespaceManager(xmldoc.NameTable);
                ns.AddNamespace("ns1", GROUNDSPEAK_NAMESPACE[0]);

                xmlnode = xmldoc.SelectNodes("/ns1:gpx/ns1:wpt", ns);
                
                // Count number of elements
                Log("Reading " + xmlnode.Count.ToString() + " caches");

                // Read file author to detect GC Tour
                bool bIsGCTour = false;
                try
                {
                    String author = xmldoc.SelectNodes("/ns1:gpx/ns1:author", ns).Item(0).InnerText.Trim();
                    if (author.ToLower().StartsWith("gctour"))
                    {
                        bIsGCTour = true;
                    }
                }
                catch (Exception)
                {
                }

                // Read export date
                DateTime dateExport = DateTime.Now;
                try
                {
	                String de = xmldoc.SelectNodes("/ns1:gpx/ns1:time", ns).Item(0).InnerText.Trim();
	                Log("Export date: " + de);
	                dateExport = MyTools.ParseDate(de);
                }
                catch(Exception)
                {}

                // Parse each node
                foreach (XmlNode node in xmlnode)
                {
                    geo = null;
                    if (MyTools.getNodeValue(node, "type").Contains("Geocache") == false)
                    {
                    	Waypoint wpt = LoadGCWaypoint(dateExport, node);
                    	if (wpt != null)
                    		localWaypoints.Add(wpt);
                        continue;
                    }
                    geo = new Geocache(this);
                    geo._DateExport = dateExport;
                    geo._Code = MyTools.getNodeValue(node, "name");
                    sentry = geo._Code;

                    // Check if cache will be discarded or not
                    bool bDiscard = false;
                    if (localCaches.ContainsKey(geo._Code))
                    {
                        // Existing element
                        // Newer ?
                        Geocache existingCache = localCaches[geo._Code];
                        if (existingCache._DateExport < geo._DateExport)
                        {
                            // Newer element, we keep it
                            localCaches[geo._Code] = geo;
                            bDiscard = false;

                            // complete origin
                            // new element, we keep it but append old origins at the end
                            geo._origin.Add(ori);
                            geo._origin.AddRange(existingCache._origin);
                        }
                        else
                        {
                            // Older than already present in the list, ignored
                            bDiscard = true;

                            // complete origin
                            // old element, we keep it but append new origins at the end
                            existingCache._origin.Add(ori);
                        }
                    }
                    else
                    {
                        // New element
                        localCaches.Add(geo._Code, geo);
                        // complete origin
                        geo._origin.Add(ori);

                        bDiscard = false;
                    }
                    if (bDiscard)
                    {
                    	geo = null;
                        continue;
                    }

                    if (MyTools.getNodeValue(node, "sym") == "Geocache Found")
                        geo._bFound = true;
                    else
                        geo._bFound = false;
                    if (bIgnoreFound)
                        geo._bFound = false;

                    geo._Name = MyTools.getNodeValue(node, "urlname");
                    geo._Url = MyTools.getNodeValue(node, "url");
                    if (geo._Url == "")
                    	geo._Url = "http://coord.info/" + geo._Code;
                    XmlNode extra = null;
                    extra = node["groundspeak:cache"];
                    
                    geo._Owner = MyTools.getNodeValue(extra, "groundspeak:owner");
                    if (owner == geo._Owner.ToLower())
                        geo._bOwned = true;
                    else
                        geo._bOwned = false;

                    geo._DateCreation = MyTools.getNodeValue(node, "time");
                    // Robustness with GeoPrinter that puts a bloody "Hidden: " in front of the date. tsssss
                    geo._DateCreation = geo._DateCreation.Replace("Hidden: ", "");
                    geo._DateCreation = geo._DateCreation.Replace("Cachée le: ", "");

                    geo._ShortDescription = MyTools.getNodeValue(extra, "groundspeak:short_description");
                    geo._LongDescription = MyTools.getNodeValue(extra, "groundspeak:long_description");
                    geo._D = MyTools.getNodeValue(extra, "groundspeak:difficulty");
                    geo._T = MyTools.getNodeValue(extra, "groundspeak:terrain");
                    // Robustness : C:Geo ajoute des ".0" à la fin des valeurs entières !
                    geo._D = geo._D.Replace(".0", "");
                    geo._T = geo._T.Replace(".0", "");

                    geo._Latitude = MyTools.getAttributeValue(node, "lat");
                    geo._Longitude = MyTools.getAttributeValue(node, "lon");
                    geo._Type = MyTools.getNodeValue(extra, "groundspeak:type");
                    // Patch GC !
                    if (geo._Type == "Mystery Cache")
                        geo._Type = "Unknown Cache";
                    geo._Available = MyTools.getAttributeValue(extra, "available");
                    geo._Archived = MyTools.getAttributeValue(extra, "archived");

                    // Fix GC Tour with missing uppercase
                    geo._Container = MyTools.FirstCharToUpper(MyTools.getNodeValue(extra, "groundspeak:container"));
                    // Fix opencaching with virtual caches whose container is undefined
                    if (geo._Container == "")
                        geo._Container = "Not chosen";

                    geo._Hint = MyTools.getNodeValue(extra, "groundspeak:encoded_hints");
                    geo.UpdateDistanceToHome(_dHomeLat, _dHomeLon);

                    // Attributes
                    XmlNode atts = extra["groundspeak:attributes"];
                    if ((atts != null) && (atts.ChildNodes.Count != 0))
                    {
                        geo._Attributes = new List<string>();
                        String satt;
                        foreach (XmlNode atn in atts.ChildNodes)
                        {

                            bool bNo = false;
                            if (MyTools.getAttributeValue(atn, "inc") == "1")
                                satt = atn.InnerText.Trim();
                            else
                            {
                                bNo = true;
                                satt = atn.InnerText.Trim() + "-no";
                            }

                            // Read data used only in GPX export
                            string iatt = MyTools.getAttributeValue(atn, "id");

                            // Check if an image for this attribute is found in the image list
                            // If not try to get the text from the correspondance table
                            //1) find image index ?
                            String key = satt.Replace("/", "");
                            if (_indexImages.ContainsKey(key.ToLower()))
                            {
                                // Good, we found it, do nothing
                            }
                            else
                            {
                                // Crap, try to get it from the dico
                                Log("!!!! Unknown attribute text: " + satt + " # id= " + iatt);
                                String keya = iatt;
                                if (bNo)
                                    keya += "-no";
                                if (_tableAttributes.ContainsKey(keya))
                                    satt = _tableAttributes[keya];
                                Log("New text for attribute: " + satt);
                            }

                            geo._Attributes.Add(satt);
                            geo._listAttributesId.Add(iatt);
                        }
                    }

                    // Logs
                    XmlNode logs = extra["groundspeak:logs"];
                    if ((logs != null) && (logs.ChildNodes.Count != 0))
                    {
                        // On a plusieurs logs et on va directement les trier du
                        // plus récent au plus ancien
                        Dictionary<String, CacheLog> newLogs = new Dictionary<string, CacheLog>();

                        geo._Logs = new List<CacheLog>();
                        foreach (XmlNode log in logs.ChildNodes)
                        {
                            CacheLog one_log = new CacheLog(this);
                            one_log._Date = MyTools.getNodeValue(log, "groundspeak:date");
                            one_log._Type = MyTools.getNodeValue(log, "groundspeak:type");
                            one_log._User = MyTools.getNodeValue(log, "groundspeak:finder");
                            one_log._Text = MyTools.getNodeValue(log, "groundspeak:text");

                            // Read data used only in GPX export
                            one_log._LogId = MyTools.getAttributeValue(log, "id");

                            // Robustness : this attribute might not be filled by 3rd parties applications
                            one_log._FinderId = MyTools.getAttributeValue(log, "groundspeak:finder", "id");

                            /*
                             * Even if encoded, the text is READABLE LOL !!!*/
                            one_log._Encoded = MyTools.getAttributeValue(log, "groundspeak:text", "encoded");

                            // Key for sorting
                            one_log._SortingKey = CreateKeyForLogSorting(one_log);

                            // On l'ajoute au dico
                            newLogs.Add(one_log._SortingKey, one_log);
                        }

                        // Maintenant on tri ces logs et on les stockes dans la cache
                        // On remplace les logs par la liste triée selon la key
                        var list = newLogs.Keys.ToList();
                        list.Sort();
                        list.Reverse();
                        foreach (var key in list)
                        {
                            geo._Logs.Add(newLogs[key]);
                        }
                    }

                    // TB/GC
                    // Robustness again
                    logs = extra["groundspeak:travelbugs"];
                    if ((logs != null) && (logs.ChildNodes.Count != 0))
                    {
                        geo._listTB = new Dictionary<string, string>();

                        String key = "", id = "", value = "";
                        foreach (XmlNode gctb in logs.ChildNodes)
                        {
                            key = MyTools.getAttributeValue(gctb, "ref");
                            // robustness au cas où on ait ref vide
                            if (key == "")
                                key = "TB" + Guid.NewGuid().ToString().Substring(0, 5);
                            value = MyTools.getNodeValue(gctb, "groundspeak:name");
                            geo._listTB.Add(key, value);

                            // Read data used only in GPX export
                            id = MyTools.getAttributeValue(gctb, "id");
                            // robustness au cas où on ait id vide
                            if (id == "")
                                id = key;
                            geo._listTBId.Add(id);
                        }
                    }
                    // else no TB info

                    // Read data used only in GPX export
                    geo._ShortDescHTML = MyTools.getAttributeValue(extra, "groundspeak:short_description", "html");
                    geo._LongDescHTML = MyTools.getAttributeValue(extra, "groundspeak:long_description", "html");
                    geo._CacheId = MyTools.getAttributeValue(extra, "id");
                    geo._PlacedBy = MyTools.getNodeValue(extra, "groundspeak:placed_by");
                    // Robustness : this attribute might not be filled by 3rd parties applications
                    geo._OwnerId = MyTools.getAttributeValue(extra, "groundspeak:owner", "id");
                    geo._Country = MyTools.getNodeValue(extra, "groundspeak:country");
                    geo._State = MyTools.getNodeValue(extra, "groundspeak:state");

                    // Final ops if GC Tour
                    if (bIsGCTour)
                    {
                        geo._Type = geo._Type.Replace("  ", " "); // to avoid "Virtual  Cache"
                        geo._Container = geo._Container.Replace("_", " "); // to avoid "Not_chosen"
                    }

                    // Update cache object
                    geo.UpdatePrivateData(owner);
                }
                xmlnode = null;
                
                xmldoc = null;
            }
            catch (Exception)
            {
            	xmlnode = null;
                xmldoc = null;
            	Log("!!!! Crashed during load of " + sentry);
                if (geo != null)
                {
                    Log(geo.ToString());

                    // Et surtout on vire cette cache moisie !
                    if (localCaches.ContainsKey(geo._Code))
                        localCaches.Remove(geo._Code);
                }
                throw;
            }
        }

        /// <summary>
        /// This is the heart of MGM
        /// Load a GPX file and populate cache internal database
        /// Loaded caches are not displayed by this function!
        /// </summary>
        /// <param name="f">GPX file (.gpx)</param>
        public void LoadFile(string f)
        {
        	String owner = ConfigurationManager.AppSettings["owner"].ToLower();
            bool bIgnoreFound = (ConfigurationManager.AppSettings["ignorefounds"] == "True") ? true : false;

            // Display file name
            Log("Current directory " + Directory.GetCurrentDirectory());
            Log("Loading file " + f);

            // Log GPX file
            XmlDocument _xmldoc;
            XmlNodeList _xmlnode;
            _xmldoc = new XmlDocument();
            String sentry = "";
            Geocache geo = null; ;
            try
            {

                _xmldoc.Load(f);
                XmlNamespaceManager ns = new XmlNamespaceManager(_xmldoc.NameTable);
                ns.AddNamespace("ns1", GROUNDSPEAK_NAMESPACE[0]);
                
                _xmlnode = _xmldoc.SelectNodes("/ns1:gpx/ns1:wpt", ns);

                // Count number of elements
                Log("Reading " + _xmlnode.Count.ToString() + " caches");

                // Read file author to detect GC Tour
                bool bIsGCTour = false;
                //bool bIsGsak = false;
                try
                {
                    String author = _xmldoc.SelectNodes("/ns1:gpx/ns1:author", ns).Item(0).InnerText.Trim();
                    if (author.ToLower().StartsWith("gctour"))
                    {
                        bIsGCTour = true;
                    }
                    //else if (author.ToLower().StartsWith("gsak"))
                    //{
                    //    bIsGsak = true;
                    //}
                }
                catch (Exception)
                {
                }

                // Read export date
                DateTime dateExport = DateTime.Now;
                try
                {
	                String de = _xmldoc.SelectNodes("/ns1:gpx/ns1:time", ns).Item(0).InnerText.Trim();
	                Log("Export date: " + de);
	                dateExport = MyTools.ParseDate(de);
                }
                catch(Exception)
                {}

                // Ok, we fill the origin now
                String ori = f;
                if (_zipFileCurrentlyLoaded != "")
                    ori += " (" + _zipFileCurrentlyLoaded + ")";
                _LoadedFiles.Add(ori);

                // Parse each node
                foreach (XmlNode node in _xmlnode)
                {
                    geo = null;
                    if (MyTools.getNodeValue(node, "type").Contains("Geocache") == false)
                    {
                        LoadWaypoint(dateExport, node);
                        continue;
                    }
                    geo = new Geocache(this);
                    geo._DateExport = dateExport;
                    geo._Code = MyTools.getNodeValue(node, "name");
                    sentry = geo._Code;

                    // Check if cache will be discarded or not
                    bool bDiscard = false;
                    if (_caches.ContainsKey(geo._Code))
                    {
                        // Existing element
                        // Newer ?
                        Geocache existingCache = _caches[geo._Code];
                        if (existingCache._DateExport < geo._DateExport)
                        {
                            // Newer element, we keep it
                            _caches[geo._Code] = geo;
                            bDiscard = false;

                            // complete origin
                            // new element, we keep it but append old origins at the end
                            geo._origin.Add(ori);
                            geo._origin.AddRange(existingCache._origin);
                        }
                        else
                        {
                            // Older than already present in the list, ignored
                            bDiscard = true;

                            // complete origin
                            // old element, we keep it but append new origins at the end
                            existingCache._origin.Add(ori);
                        }
                    }
                    else
                    {
                        // New element
                        // On ne l'ajoute que s'il n'est pas à ignorer !
                        if (_ignoreList.ContainsKey(geo._Code) == false)
                        {
                            _caches.Add(geo._Code, geo);
                            // complete origin
                            geo._origin.Add(ori);

                            bDiscard = false;
                        }
                        else
                            bDiscard = true;
                    }
                    if (bDiscard)
                        continue;

                    if (MyTools.getNodeValue(node, "sym") == "Geocache Found")
                        geo._bFound = true;
                    else
                        geo._bFound = false;
                    if (bIgnoreFound)
                        geo._bFound = false;

                    geo._Name = MyTools.getNodeValue(node, "urlname");
                    geo._Url = MyTools.getNodeValue(node, "url");
                    if (geo._Url == "")
                    	geo._Url = "http://coord.info/" + geo._Code;
                    XmlNode extra = null;
                    extra = node["groundspeak:cache"];
                    
                    geo._Owner = MyTools.getNodeValue(extra, "groundspeak:owner");
                    if (owner == geo._Owner.ToLower())
                        geo._bOwned = true;
                    else
                        geo._bOwned = false;

                    geo._DateCreation = MyTools.getNodeValue(node, "time");
                    // Robustness with GeoPrinter that puits a bloody "Hidden: " in front of the date. tsssss
                    geo._DateCreation = geo._DateCreation.Replace("Hidden: ", "");
                    geo._DateCreation = geo._DateCreation.Replace("Cachée le: ", "");

                    geo._ShortDescription = MyTools.getNodeValue(extra, "groundspeak:short_description");
                    geo._LongDescription = MyTools.getNodeValue(extra, "groundspeak:long_description");
                    geo._D = MyTools.getNodeValue(extra, "groundspeak:difficulty");
                    geo._T = MyTools.getNodeValue(extra, "groundspeak:terrain");
                    // Robustness : C:Geo ajoute des ".0" à la fin des valeurs entières !
                    geo._D = geo._D.Replace(".0", "");
                    geo._T = geo._T.Replace(".0", "");

                    geo._Latitude = MyTools.getAttributeValue(node, "lat");
                    geo._Longitude = MyTools.getAttributeValue(node, "lon");
                    geo._Type = MyTools.getNodeValue(extra, "groundspeak:type");
                    // Patch GC !
                    if (geo._Type == "Mystery Cache")
                        geo._Type = "Unknown Cache";
                    geo._Available = MyTools.getAttributeValue(extra, "available");
                    geo._Archived = MyTools.getAttributeValue(extra, "archived");
                    
                    // Fix GC Tour with missing uppercase
                    geo._Container = MyTools.FirstCharToUpper(MyTools.getNodeValue(extra, "groundspeak:container"));
                    // Fix opencaching with virtual caches whose container is undefined
                    if (geo._Container == "")
                        geo._Container = "Not chosen";

                    geo._Hint = MyTools.getNodeValue(extra, "groundspeak:encoded_hints");
                    geo.UpdateDistanceToHome(_dHomeLat, _dHomeLon);

                    // Attributes
                    XmlNode atts = extra["groundspeak:attributes"];
                    if ((atts != null) &&(atts.ChildNodes.Count != 0))
                    {
                        geo._Attributes = new List<string>();
                        String satt;
                        foreach (XmlNode atn in atts.ChildNodes)
                        {

                            bool bNo = false;
                            if (MyTools.getAttributeValue(atn, "inc") == "1")
                                satt = atn.InnerText.Trim();
                            else
                            {
                                bNo = true;
                                satt = atn.InnerText.Trim() + "-no";
                            }

                            // Read data used only in GPX export
                            string iatt = MyTools.getAttributeValue(atn, "id");

                            // Check if an image for this attribute is found in the image list
                            // If not try to get the text from the correspondance table
                            //1) find image index ?
                            String key = satt.Replace("/", "");
                            if (_indexImages.ContainsKey(key.ToLower()))
                            {
                                // Good, we found it, do nothing
                            }
                            else
                            {
                                // Crap, try to get it from the dico
                                Log("!!!! Unknown attribute text: " + satt + " # id= " + iatt);
                                String keya = iatt;
                                if (bNo)
                                    keya += "-no";
                                if (_tableAttributes.ContainsKey(keya))
                                    satt = _tableAttributes[keya];
                                Log("New text for attribute: " + satt);
                            }

                            geo._Attributes.Add(satt);
                            geo._listAttributesId.Add(iatt);                            
                        }
                    }

                    // Logs
                    XmlNode logs = extra["groundspeak:logs"];
                    if ((logs != null) && (logs.ChildNodes.Count != 0))
                    {
                        // On a plusieurs logs et on va directement les trier du
                        // plus récent au plus ancien
                        Dictionary<String, CacheLog> newLogs = new Dictionary<string, CacheLog>();

                        geo._Logs = new List<CacheLog>();
                        foreach (XmlNode log in logs.ChildNodes)
                        {
                            CacheLog one_log = new CacheLog(this);
                            one_log._Date = MyTools.getNodeValue(log, "groundspeak:date");
                            one_log._Type = MyTools.getNodeValue(log, "groundspeak:type");
                            one_log._User = MyTools.getNodeValue(log, "groundspeak:finder");
                            one_log._Text = MyTools.getNodeValue(log, "groundspeak:text");

                            // Read data used only in GPX export
                            one_log._LogId = MyTools.getAttributeValue(log, "id");

                            // Robustness : this attribute might not be filled by 3rd parties applications
                            one_log._FinderId = MyTools.getAttributeValue(log, "groundspeak:finder", "id");

                            /*
                             * Even if encoded, the text is READABLE LOL !!!*/
                            one_log._Encoded = MyTools.getAttributeValue(log, "groundspeak:text", "encoded");
                           
                            // Key for sorting
                            one_log._SortingKey = CreateKeyForLogSorting(one_log);

                            // On l'ajoute au dico
                            newLogs.Add(one_log._SortingKey, one_log);
                        }

                        // Maintenant on tri ces logs et on les stockes dans la cache
                        // On remplace les logs par la liste triée selon la key
                        var list = newLogs.Keys.ToList();
                        list.Sort();
                        list.Reverse();
                        foreach (var key in list)
                        {
                            geo._Logs.Add(newLogs[key]);
                        }
                    }

                    // TB/GC
                    // Robustness again
                    logs = extra["groundspeak:travelbugs"];
                    if ((logs != null) &&(logs.ChildNodes.Count != 0))
                    {
                        geo._listTB = new Dictionary<string, string>();

                        String key = "", id = "", value = "";
                        foreach (XmlNode gctb in logs.ChildNodes)
                        {
                            key = MyTools.getAttributeValue(gctb, "ref");
                            // robustness au cas où on ait ref vide
                            if (key == "")
                                key = "TB" + Guid.NewGuid().ToString().Substring(0, 5);
                            value = MyTools.getNodeValue(gctb, "groundspeak:name");
                            geo._listTB.Add(key, value);

                            // Read data used only in GPX export
                            id = MyTools.getAttributeValue(gctb, "id");
                            // robustness au cas où on ait id vide
                            if (id == "")
                                id = key;
                            geo._listTBId.Add(id);
                        }
                    }
                    // else no TB info

                    // Read data used only in GPX export
                    geo._ShortDescHTML = MyTools.getAttributeValue(extra, "groundspeak:short_description", "html");
                    geo._LongDescHTML = MyTools.getAttributeValue(extra, "groundspeak:long_description", "html");
                    geo._CacheId = MyTools.getAttributeValue(extra, "id");
                    geo._PlacedBy = MyTools.getNodeValue(extra, "groundspeak:placed_by");
                    // Robustness : this attribute might not be filled by 3rd parties applications
                    geo._OwnerId = MyTools.getAttributeValue(extra, "groundspeak:owner", "id");
                    geo._Country = MyTools.getNodeValue(extra, "groundspeak:country");
                    geo._State = MyTools.getNodeValue(extra, "groundspeak:state");

                    // Final ops if GC Tour
                    if (bIsGCTour)
                    {
                        geo._Type = geo._Type.Replace("  ", " "); // to avoid "Virtual  Cache"
                        geo._Container = geo._Container.Replace("_", " "); // to avoid "Not_chosen"
                    }

                    // Update cache object
                    geo.UpdatePrivateData(owner);

                }
            }
            catch (Exception ex)
            {
                Log("!!!! Crashed during load of " + sentry);
                if (geo != null)
                {
                    Log(geo.ToString());

                    // Et surtout on vire cette cache moisie !
                    if (_caches.ContainsKey(geo._Code))
                        _caches.Remove(geo._Code);
                }
                throw;
            }
        }

        
        private void Form1_FormClosing_1(object sender, FormClosingEventArgs e)
        {
        	// Cleanup...
        	if (_bForceClose)
            {
                CloseLog();
            }
            else
            {
                String msg = "";
                String title = GetTranslator().GetString("OKTitle");
                if (_iNbModifiedCaches <= 0)
                {
                    msg = GetTranslator().GetString("AskQuitconfirm");
                }
                else
                {
                	title = GetTranslator().GetString("WarTitle");
                	msg = GetTranslator().GetString("AskQuitconfirmModifications").Replace("#","\r\n");
                	int inbmodif = 0;
		        	String msg2 = DisplayModificationsImpl(null, ref inbmodif);
		        	if (msg2 != "")
		            {
		            	msg += "\r\n\r\n" + GetTranslator().GetString("CacheModifGeneral").Replace("#","\r\n") + "\r\n\r\n";
		            	msg += inbmodif.ToString() + GetTranslator().GetString("LblModifiedCaches") + "\r\n" + msg2;
		        	}
                }

                DialogResult dialogResult = MyMessageBox.Show(msg, title, MessageBoxIcon.Question, GetTranslator(), null, null, GetImageSized("dontleave"));
                if (dialogResult == DialogResult.Yes)
                {
                	// Sauvegarde de la base (si besoin)
                	UpdateMGMInternalDB();
                	
                	// Fermeture des logs
                    CloseLog();
                }
                else
                {
                    e.Cancel = true;
                }
            }
            
            // On ferme les cartes qui sont éventuellement en train de cacher
            if ( _cachesPreviewMap != null)
            	_cachesPreviewMap.Manager.CancelTileCaching();
            if (_cacheDetail._gmap != null)
            	_cacheDetail._gmap.Manager.CancelTileCaching();
        }

        private void OpenAllOfflineImage(object sender, EventArgs e)
        {
            List<Geocache> geos = GetSelectedCaches();
            foreach(Geocache geo in geos)
            {
	            String html = "<html><body>\r\n";
	            html += "<H1><a href=MGMGEO:" + geo._Code + ">" + geo._Code + "&nbsp;" + geo._Name + "</a></H1><br>\r\n";
	            String offdatapath = GetUserDataPath() + Path.DirectorySeparatorChar + "Offline" + Path.DirectorySeparatorChar;
	            foreach (KeyValuePair<String, OfflineImageWeb> paire2 in geo._Ocd._ImageFilesSpoilers)
	            {
	        		OfflineImageWeb oiw = paire2.Value;
	                if (oiw != null)
	                {
	                    String imgpath = "<img src=\"file:\\\\" + offdatapath + oiw._localfile + "\">";
	                    html += "<H3>" + HtmlAgilityPack.HtmlEntity.DeEntitize(oiw._name) + "</H3>\r\n";
	                    html += imgpath + "<br>\r\n";
	                }
	            }
	            html += "</body></html>";
	            _cacheDetail.LoadPageText(GetTranslator().GetString("LblOfflineImagesFor") + " " + geo._Name, html, true);
            }
        }
        
        private void OpenOfflineImage(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item != null)
            {
            	            
                OfflineImageWeb oiw = item.Tag as OfflineImageWeb;
                if (oiw != null)
                {
                	String offdatapath = GetUserDataPath() + Path.DirectorySeparatorChar + "Offline" + Path.DirectorySeparatorChar;
                    _cacheDetail.LoadPage(oiw._name, offdatapath + oiw._localfile);
                }
            }
        }

        private void CreateImagecontextMenu(bool bSingleSelection)
        {
            _cacheimages.DropDownItems.Clear();
            if (bSingleSelection)
            {
                _cacheimages.Enabled = true;
                EXListViewItem lstvItem = lvGeocaches.Items[lvGeocaches.SelectedIndices[0]] as EXListViewItem;
                String code = lstvItem.Text;
                if (_od._OfflineData.ContainsKey(code))
                {
                    OfflineCacheData ocd = _od._OfflineData[code];
                    if ((ocd._ImageFilesSpoilers.Count != 0))// || (ocd._ImageFilesFromDescription.Count != 0))
                    {
                    	// Pour tout afficher
                    	ToolStripMenuItem it = CreateTSMI("MNUImageDisplayAll", OpenAllOfflineImage);
                        _cacheimages.DropDownItems.Add(it);
                        it.Font = MyTools.ChangeFontStyle(it.Font, true, false);
                        _cacheimages.DropDownItems.Add(new ToolStripSeparator());
                        
                        // Les autres images
                        // Tous les 10 on crée un nouveau menu !
                        ToolStripMenuItem tsi = _cacheimages;
                        int index = 0;
                        foreach (KeyValuePair<String, OfflineImageWeb> paire2 in ocd._ImageFilesSpoilers)
                        {
                        	if (index == 9)
                        	{
                        		it = CreateTSMI("MNUImageDisplayNext");
                        		index = 0;
                        		tsi.DropDownItems.Add(it);
                        		tsi = it;
                        	}
                        	
                            String lbl = paire2.Value._name;
                            lbl = HtmlAgilityPack.HtmlEntity.DeEntitize(lbl);
                            ToolStripMenuItem item = new ToolStripMenuItem(lbl, null, new EventHandler(OpenOfflineImage));
                            item.Tag = paire2.Value;
                            tsi.DropDownItems.Add(item);
                            index++;
                        }
                    }
                    else
                    {
                        // No spoiler founds, but did we try to download images before ?
                        if (ocd._NotDownloaded)
                        {
                            _cacheimages.DropDownItems.Add(CreateTSMI("MNUCheckForImage", CreateOfflineData));
                        }
                        else
                        {
                            ToolStripMenuItem it = CreateTSMI("MNUNoImage");
                            _cacheimages.DropDownItems.Add(it);
                            it.Enabled = false;
                        }
                    }
                }
                else
                {
                    _cacheimages.DropDownItems.Add(CreateTSMI("MNUCheckForImage", CreateOfflineData));
                }
            }
            else
            {
                _cacheimages.Enabled = false;
                    
            }
        }

        private void lstv_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (lvGeocaches.SelectedItems.Count == 1)
                {
                    _offline.DropDownItems[0].Enabled = true;
                    _waypointsmenu.Enabled = true;
                    _filtersmenu.DropDownItems[0].Enabled = true;
                    _filtersmenu.DropDownItems[1].Enabled = true;
                    CreateImagecontextMenu(true);
                    
                    EXListViewItem lstvItem = lvGeocaches.SelectedItems[0] as EXListViewItem;
                    String code = lstvItem.Text;
                    if (isBookmarked(lstvItem, code))
                    {
                        _favmenu.DropDownItems[0].Enabled = false;
                        _favmenu.DropDownItems[1].Enabled = true;
                    }
                    else
                    {
                        _favmenu.DropDownItems[0].Enabled = true;
                        _favmenu.DropDownItems[1].Enabled = false;
                    }
                }
                else
                {
                    _offline.DropDownItems[0].Enabled = false;
                    _waypointsmenu.Enabled = false;
                    _filtersmenu.DropDownItems[0].Enabled = false;
                    _filtersmenu.DropDownItems[1].Enabled = false;
                    CreateImagecontextMenu(false);
                    
                    _favmenu.DropDownItems[0].Enabled = true;
                    _favmenu.DropDownItems[1].Enabled = true;
                }
                
                Log("CONTEXT MENU ON LISTVIEW INVOKED");
                TranslateTooltips(_mnuContextMenu, _toolTipForMGM);
                _mnuContextMenu.Show(lvGeocaches, e.Location);
            }
            else if (e.Button == MouseButtons.Left)
            {
                // Check if click on the select column item
                ListViewHitTestInfo info = lvGeocaches.HitTest(e.X, e.Y);
                ListViewItem.ListViewSubItem subitem = info.SubItem;
                if (subitem is EXBoolListViewSubItem)
                {
                    // Ok user clicked on selection column
                    EXBoolListViewSubItem subool = subitem as EXBoolListViewSubItem;
                    EXListViewItem lstvItem = lvGeocaches.GetItemAt(e.X, e.Y) as EXListViewItem;
                    if ((Control.ModifierKeys & Keys.Control) > 0)
                    {
                        // Ok we toggle selection for everything selected based on value from clicked item
                        lstvItem.Selected = true;
                        bool bval = subool.BoolValue;
                        foreach (Object obj in lvGeocaches.SelectedItems)
                        {
                            EXListViewItem lstvItemi = obj as EXListViewItem;
                            EXBoolListViewSubItem subooli = (EXBoolListViewSubItem)(lstvItemi.SubItems[_ID_LVSel]);
                            subooli.BoolValue = bval;
                            ToggleSelection(lstvItemi, subooli);
                        }
                    }
                    else
                    {
                        // Just toggle one single element
                        ToggleSelection(lstvItem, subool);
                    }
                }
                else if (subitem.Name == "LVCode")
                {
                    // 1st column
                    WriteNoteOnCache();
                }
                else if (subitem.Name == "LVHint")
                {
                    subitem.Text = ROT13.Transform(subitem.Text);
                    EXMultipleImagesListViewSubItem si = subitem as EXMultipleImagesListViewSubItem;
                    if ((si != null) && (si.MyValue != null) && (si.Text == si.MyValue.ToString()))
                    {
                        subitem.Font = MyTools.ChangeFontStyle(subitem.Font, true, true);
                    }
                    else
                        subitem.Font = MyTools.ChangeFontStyle(subitem.Font, false, false);

                    lvGeocaches.Invalidate(subitem.Bounds);
                }

                // display item on map
                Geocache geo = null;
                try
                {
                    EXListViewItem lstvItem = lvGeocaches.Items[lvGeocaches.SelectedIndices[0]] as EXListViewItem;
                    String code = lstvItem.Text;
                    geo = _caches[code];
                    displayCacheonMapTab(geo);
                }
                catch (Exception)
                {
                }

                // display item wbFastCachePreview
                DisplayFastCacheDetail(geo);
            }
        }

        /// <summary>
        /// Display cache detail in the fast cache WebBrowser in main form
        /// </summary>
        /// <param name="geo">Geocache to display</param>
        public void DisplayFastCacheDetail(Geocache geo)
        {
            try
            {
                if (geo != null)
                {
                    // On fait ça uniquement si le controle est suffisament visible
                    if (wbFastCachePreview.Visible)
                    {
                        // on affiche
                        bool bUseOfflineData = !_bInternetAvailable;
                        OfflineCacheData ocd = null;
                        if (_od._OfflineData.ContainsKey(geo._Code))
                        {
                            ocd = _od._OfflineData[geo._Code];
                        }
                        wbFastCachePreview.DocumentText = geo.ToHTML(_bUseKm, ocd, bUseOfflineData);

                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void lstv_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            EXListViewItem lstvItem = lvGeocaches.GetItemAt(e.X, e.Y) as EXListViewItem;
            
            ListViewHitTestInfo info = lvGeocaches.HitTest(e.X, e.Y);
            ListViewItem.ListViewSubItem subitem = info.SubItem;
            if (subitem is EXBoolListViewSubItem)
            {
                // already handled by single click
            }
            else
            {
                // We simply display
                DisplayDetailFromSelection(lstvItem, false, false);
            }
            
            // We simply display
            //DisplayDetailFromSelection(lstvItem);
        }

        private void DisplayDetailFromSelection(EXListViewItem lstvItem, bool bUseOfflineData, bool bForceDisplayFromGPX)
        {
            if (lstvItem == null) return;

            String code = lstvItem.Text;
            try
            {
                Geocache geo = _caches[code];
                OfflineCacheData ocd = null;
                if (_od._OfflineData.ContainsKey(geo._Code))
                {
                    ocd = _od._OfflineData[geo._Code];
                }
                
                
            	// Truc par défaut
            	_cacheDetail.LoadPageCache(geo, _bUseKm, ocd, bUseOfflineData, bForceDisplayFromGPX);
            }
            catch (Exception ex)
            {
            	ShowException("", "Cache description", ex);
            }
        }

        /// <summary>
        /// Display cache details from a cache
        /// </summary>
        /// <param name="geo">cache to display</param>
        /// <param name="bUseOfflineData">If true, will use offline saved data</param>
        public void DisplayDetailFromSelection(Geocache geo, bool bUseOfflineData)
        {
            if (geo == null) return;
            try
            {
                OfflineCacheData ocd = null;
                if (_od._OfflineData.ContainsKey(geo._Code))
                {
                    ocd = _od._OfflineData[geo._Code];
                }
                _cacheDetail.LoadPageCache(geo, _bUseKm, ocd, bUseOfflineData, false);
            }
            catch (Exception)
            {
                MsgActionError(this, GetTranslator().GetString("ErrorCode") + ": " + geo._Code);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            DoFilter();
        }

        /// <summary>
        /// Execute filter
        /// </summary>
        public void DoFilter()
        {
            Cursor old = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            _bUseFilter = true;
            if (UpdateFilter())
            {
                PopulateListViewCache(null);
            }
            Cursor.Current = old;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DisplayAll();
        }

        private void DisplayAll()
        {
            Cursor old = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            _bUseFilter = false;
            PopulateListViewCache(null);
            Cursor.Current = old;
        }

        /// <summary>
        /// Update current filter from HMI
        /// </summary>
        /// <returns>true if successful update</returns>
        public bool UpdateFilter()
        {
            try
            {
                Filter._bOnlyManualSelection = false;
                Filter._name = textBoxName.Text;

                Filter._bFilterSize = cbFilterSize.Checked;
                Filter._bFilterType = cbFilterType.Checked;
                Filter._bFilterDistance = cbFilterDistance.Checked;
                Filter._bFilterStatus = cbFilterStatus.Checked;
                Filter._bFilterDifficulty = cbFilterDifficulty.Checked;
                Filter._bFilterTerrain = cbFilterTerrain.Checked;
                Filter._bFilterOwner = cbFilterOwner.Checked;
                Filter._bFilterOwnerDisplay = cbOwnerDisplay.Checked;
                Filter._bFilterAttribute = cbFilterAttributeIn.Checked;
                Filter._bFilterAttributeOut = cbFilterAttributeOut.Checked;
                Filter._bFilterAttributeAllOfThem = radioButton8AttsIn.Checked;
                Filter._bFilterAttributeOutAllOfThem = radioButton11AttsOut.Checked;

                Filter._bContainsTBGC = cbTBGC.Checked;

                if (radioButton1txtfilter.Checked)
                    Filter._iNameType = 1;
                else if (radioButton2txtfilter.Checked)
                    Filter._iNameType = 2;
                else if (radioButton3txtfilter.Checked)
                    Filter._iNameType = 3;
                else if (rbFilterOnTagtxtfilter.Checked)
                    Filter._iNameType = 4;
                else
                    Filter._iNameType = 1;

                Filter._containers.Clear();
                foreach(KeyValuePair<String, CheckBox> pair in _geocachingConstants.GetDicoSizeCheckbox())
                {
                	if (pair.Value.Checked)
                		Filter._containers.Add(pair.Key);
                }

                Filter._types.Clear();
                foreach(KeyValuePair<String, CheckBox> pair in _geocachingConstants.GetDicoTypeCheckbox())
                {
                	if (pair.Value.Checked)
                		Filter._types.Add(pair.Key);
                }
                
                // We check always in Km, piss off Miles ;-)
                if (_bUseKm)
                {
                    Filter._distMin = MyTools.ConvertToDouble(txtDistMin.Text);
                    Filter._distMax = MyTools.ConvertToDouble(txtDistMax.Text);
                }
                else
                {
                    // Convert user input from miles to Km
                    double distMin = MyTools.ConvertToDouble(txtDistMin.Text) / _dConvKmToMi;
                    double distMax = MyTools.ConvertToDouble(txtDistMax.Text) / _dConvKmToMi;
                    Filter._distMin = distMin;
                    Filter._distMax = distMax;
                }

                Filter._avail = cbAvailable.Checked;
                Filter._archi = !(cbArchived.Checked);

                Filter._bOwned = cbOwned.Checked;
                Filter._bFound = cbFound.Checked;


                Filter._attributes = GetSelecteditemsAtts(true);
                Filter._attributesexcl = GetSelecteditemsAtts(false);
                
                
                Filter._dMin = (Double)(comboDMin.SelectedIndex) / 2.0 + 1.0;
                Filter._dMax = (Double)(comboDMax.SelectedIndex) / 2.0 + 1.0;
                Filter._tMin = (Double)(comboTMin.SelectedIndex) / 2.0 + 1.0;
                Filter._tMax = (Double)(comboTMax.SelectedIndex) / 2.0 + 1.0;
                
                // filter near
                Filter._bFilterNear = cbFilterNear.Checked;
                String sLat = "";
                String sLon = "";
                bool bOK = ParameterObject.TryToConvertCoordinates(textBox3latlonnear.Text, ref sLat, ref sLon);
                if (bOK)
                {
	                if (sLat != CoordConvHMI._sErrorValue)
	                    Filter._dLatNear = MyTools.ConvertToDouble(sLat);
	                if (sLon != CoordConvHMI._sErrorValue)
	                    Filter._dLonNear = MyTools.ConvertToDouble(sLon);
                }
                else
                {
                	// Exception	
                	String err = String.Format(GetTranslator().GetString("ErrWrongParameter").Replace("#","\r\n"),
                	                           GetTranslator().GetString("LblFilterDescriptionTxt") + ": " + GetTranslator().GetString("FCheckFilterNear") + " - " + GetTranslator().GetString("LblLatLon"),
                	                           ParameterObject.ParameterType.Coordinates/*just an error msg*/, 
                	                           textBox3latlonnear.Text,
                	                           "");
                	throw new Exception(err);
                		
                }
                
                if (_bUseKm)
                {
                    Filter._distMaxNear = MyTools.ConvertToDouble(textBox2neardist.Text);
                }
                else
                {
                    // Convert user input from miles to Km
                    double distMaxNear = MyTools.ConvertToDouble(textBox2neardist.Text) / _dConvKmToMi;
                    Filter._distMaxNear = distMaxNear;
                }

                // Filter Area
                // Ce qui suit met à jour la zone, le zoom du filtre
                if (_cacheDetail.area_PointsClicked.Count() > 3)
                    DefineFilterArea(this, _cacheDetail.area_PointsClicked, (int)_cacheDetail._gmap.Zoom, false, false);
                // On force la nouvelle mise à jour car les options d'IHM sont peut être différentes
                Filter._bFilterArea = cbFilterArea.Checked;

                // Filter country / State
                Filter._bFilterCountryState = cbFilterRegionState.Checked;
                if (comboBoxCountry.SelectedIndex != 0)
                    Filter._sCountry = comboBoxCountry.Text;
                else
                    Filter._sCountry = "";
                if (comboBoxState.SelectedIndex != 0)
                    Filter._sState = comboBoxState.Text;
                else
                    Filter._sState = "";

                // filter date
                Filter._bFilterCreationDate = cbFilterCreation.Checked;
                Filter._bFilterLastLogDate = cbFilterLastLog.Checked;
                Filter._iNbDaysCreation = Int32.Parse(txtDaysCreation.Text);
                Filter._iNbDaysLastLog = Int32.Parse(txtDaysLastLog.Text);
                if (comboCreation.SelectedIndex == 0)
                    Filter._bIsCreationInferiorOrEqual = true;
                else
                    Filter._bIsCreationInferiorOrEqual = false;
                if (comboLastlog.SelectedIndex == 0)
                    Filter._bIsLastLogInferiorOrEqual = true;
                else
                    Filter._bIsLastLogInferiorOrEqual = false;

                // filter popularity / favorites
                Filter._bFilterFavorites = cbFilterFavorites.Checked;
                Filter._bFilterPopularity = cbFilterPopularity.Checked;
	             if (ConfigurationManager.AppSettings["useGCPopularityFormula"] == "True")
	                    Filter._ratingSimplePopularity = true;
	                else
	                    Filter._ratingSimplePopularity = false;   
                Filter._iNbFavorites = Int32.Parse(txtFavoritesValue.Text);
                Filter._dPopularity = MyTools.ConvertToDouble(txtPopularityValue.Text);
                if (comboFavorites.SelectedIndex == 0)
                    Filter._bIsFavoritesSuperiorOrEqual = true;
                else
                    Filter._bIsFavoritesSuperiorOrEqual = false;
                if (comboPopularity.SelectedIndex == 0)
                    Filter._bIsPopularitySuperiorOrEqual = true;
                else
                    Filter._bIsPopularitySuperiorOrEqual = false;
                
                return true;
            }
            catch (Exception exc)
            {
            	ShowException("", GetTranslator().GetString("ErrFilter"), exc);
                return false;
            }
        }

        private void UpdateFromFilter()
        {
            textBoxName.Text = Filter._name;

            if (Filter._iNameType == 1)
                radioButton1txtfilter.Checked = true;
            else if (Filter._iNameType == 2)
                radioButton2txtfilter.Checked = true;
            else if (Filter._iNameType == 3)
                radioButton3txtfilter.Checked = true;
            else if (Filter._iNameType == 4)
                rbFilterOnTagtxtfilter.Checked = true;
            else
                radioButton1txtfilter.Checked = true;

			foreach(KeyValuePair<String, CheckBox> pair in _geocachingConstants.GetDicoSizeCheckbox())
            {
				pair.Value.Checked = Filter._containers.Contains(pair.Key);
            }

            foreach(KeyValuePair<String, CheckBox> pair in _geocachingConstants.GetDicoTypeCheckbox())
            {
				pair.Value.Checked = Filter._types.Contains(pair.Key);
            }
            
            // Filters are always stored in Km
            if (_bUseKm)
            {
                txtDistMin.Text = Filter._distMin.ToString().Replace(",",".");
                txtDistMax.Text = Filter._distMax.ToString().Replace(",", ".");
            }
            else
            {
                // Convert to user input from Km to miles
                double distMin = Filter._distMin * _dConvKmToMi;
                double distMax = Filter._distMax * _dConvKmToMi;
                txtDistMin.Text = distMin.ToString().Replace(",", ".");
                txtDistMax.Text = distMax.ToString().Replace(",", ".");
            }

            cbAvailable.Checked = Filter._avail;
            cbArchived.Checked = !(Filter._archi);

            cbOwned.Checked = Filter._bOwned;
            cbFound.Checked = Filter._bFound;

            SetSelecteditemsAtts(true, Filter._attributes);
            SetSelecteditemsAtts(false, Filter._attributesexcl);

            
            comboDMin.SelectedIndex = (int)((Filter._dMin - 1.0) * 2.0);
            comboDMax.SelectedIndex = (int)((Filter._dMax - 1.0) * 2.0);
            comboTMin.SelectedIndex = (int)((Filter._tMin - 1.0) * 2.0);
            comboTMax.SelectedIndex = (int)((Filter._tMax - 1.0) * 2.0);
            
            // filter near
            textBox3latlonnear.Text = Filter._dLatNear.ToString().Replace(",", ".") + " " + Filter._dLonNear.ToString().Replace(",", ".");
            if (_bUseKm)
            {
                textBox2neardist.Text = Filter._distMaxNear.ToString().Replace(",",".");
            }
            else
            {
                // Convert to user input from Km to miles
                double distMaxNear = Filter._distMaxNear * _dConvKmToMi;
                textBox2neardist.Text = distMaxNear.ToString().Replace(",", ".");
            }

            // filter country / state
            if (Filter._sCountry != "")
            {
                if (comboBoxCountry.Items.Contains(Filter._sCountry))
                    comboBoxCountry.SelectedItem = Filter._sCountry;
                else
                {
                    // Add this entry
                    _dicoCountryState.Add(Filter._sCountry, new List<string>());
                    comboBoxCountry.Items.Add(Filter._sCountry);
                    comboBoxCountry.SelectedItem = Filter._sCountry;
                }
            }
            else
                comboBoxCountry.SelectedIndex = 0;

            if (Filter._sState != "")
            {
                if (comboBoxState.Items.Contains(Filter._sState))
                    comboBoxState.SelectedItem = Filter._sState;
                else
                {
                    // Add this entry
                    List<String> regionfromcountry = null;
                    if (_dicoCountryState.ContainsKey(Filter._sCountry))
                        regionfromcountry = _dicoCountryState[Filter._sCountry];
                    else
                    {
                        regionfromcountry = new List<string>();
                        _dicoCountryState.Add(Filter._sCountry, regionfromcountry);
                    }
                    if (regionfromcountry.Contains(Filter._sState) == false)
                        regionfromcountry.Add(Filter._sState);

                    comboBoxState.Items.Add(Filter._sState);
                    comboBoxState.SelectedItem = Filter._sState;
                }
            }
            else
                comboBoxState.SelectedIndex = 0;

            cbFilterSize.Checked = Filter._bFilterSize;
            cbFilterType.Checked = Filter._bFilterType;
            cbFilterDistance.Checked = Filter._bFilterDistance;
            cbFilterStatus.Checked = Filter._bFilterStatus;
            cbFilterDifficulty.Checked = Filter._bFilterDifficulty;
            cbFilterTerrain.Checked = Filter._bFilterTerrain;
            cbFilterOwner.Checked = Filter._bFilterOwner;
            cbOwnerDisplay.Checked = Filter._bFilterOwnerDisplay;
            radioButton8AttsIn.Checked = Filter._bFilterAttributeAllOfThem;
            radioButton11AttsOut.Checked = Filter._bFilterAttributeOutAllOfThem;
            cbFilterAttributeIn.Checked = Filter._bFilterAttribute;
            cbFilterAttributeOut.Checked = Filter._bFilterAttributeOut;
            
            cbTBGC.Checked = Filter._bContainsTBGC;
            cbFilterNear.Checked = Filter._bFilterNear;
            cbFilterArea.Checked = Filter._bFilterArea;
            
            // Mise à jour de la zone de filtre
            if (Filter.IsAreaDefined())
            {
                List<PointLatLng> pts = Filter.GetAreaArrayGMapNET();
                _cacheDetail.DefineAreaFromPtsList(pts, Int32.Parse(Filter.GetAreaZoom())); // ça efface ce qu'il y avait avant si besoin
            }
            else
            {
                // On efface ce qu'il y avait avant de toute façon, qu'il y ait un truc ou pas maintenant
                _cacheDetail.EmptyAreaMarkers();
            }
            cbFilterRegionState.Checked = Filter._bFilterCountryState;

            // date
            txtDaysCreation.Text = Filter._iNbDaysCreation.ToString();
            txtDaysLastLog.Text = Filter._iNbDaysLastLog.ToString();
            if (Filter._bIsCreationInferiorOrEqual) 
                comboCreation.SelectedItem = "<=";
            else
                comboCreation.SelectedItem = ">";
            if (Filter._bIsLastLogInferiorOrEqual)
                comboLastlog.SelectedItem = "<=";
            else
                comboLastlog.SelectedItem = ">";
            cbFilterCreation.Checked = Filter._bFilterCreationDate;
            cbFilterLastLog.Checked = Filter._bFilterLastLogDate;
            
            // Favorites / Popularity
            // LUI ON LE RESTAURE PAS, CONFIG MGM !!!
			//	        _filter._ratingSimplePopularity;
	        txtFavoritesValue.Text = Filter._iNbFavorites.ToString();
	        txtPopularityValue.Text = Filter._dPopularity.ToString();     
	        if (Filter._bIsFavoritesSuperiorOrEqual) 
                comboFavorites.SelectedItem = ">=";
            else
                comboFavorites.SelectedItem = "<";
            if (Filter._bIsPopularitySuperiorOrEqual)
                comboPopularity.SelectedItem = ">=";
            else
                comboPopularity.SelectedItem = "<";
	        // A faire en dernier sinon la mise à jour du texte checke automatiquement la combo
	        cbFilterFavorites.Checked = Filter._bFilterFavorites;
	        cbFilterPopularity.Checked = Filter._bFilterPopularity;
	        
        }

        private void cbOwnerDisplay_CheckedChanged(object sender, EventArgs e)
        {
            if (cbOwnerDisplay.Checked)
            {
                gbOwner.Text = GetTranslator().GetString("GBDisplayIf");
                tabPage7User.Text = GetTranslator().GetString("GBDisplayIf");
                UpdatePageIfChecked(cbFilterOwner, tabPage7User);
            }
            else
            {
                gbOwner.Text = GetTranslator().GetString("GBExcludeIfNot");
                tabPage7User.Text = GetTranslator().GetString("GBExcludeIfNot");
                UpdatePageIfChecked(cbFilterOwner, tabPage7User);
            }
            UpdateCheckBoxOwner(true);
        }

        

        private void button2_Click(object sender, EventArgs e)
        {
            DisplaySel();
        }

        private void DisplaySel()
        {
            Cursor old = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            _bUseFilter = true;
            if (UpdateFilter())
            {
                Filter._bOnlyManualSelection = true;
                PopulateListViewCache(null);
            }
            Cursor.Current = old;
        }


        /// <summary>
        /// Compile a plugin
        /// </summary>
        /// <param name="sourceName">plugin source file (C# or VB#)</param>
        /// <param name="cr">compilation result</param>
        /// <returns>true if compilation succeeded</returns>
        public bool CompilePlugin(String sourceName, out CompilerResults cr)
        {
            FileInfo sourceFile = new FileInfo(sourceName);
            CodeDomProvider provider = null;
            bool compileOk = false;
            cr = null;

            // Select the code provider based on the input file extension.
            if (sourceFile.Extension.ToUpper(CultureInfo.InvariantCulture) == ".CS")
            {
                //provider = CodeDomProvider.CreateProvider("CSharp");
                
                Dictionary<string, string> compilerInfo = new Dictionary<string, string>();
                compilerInfo.Add("CompilerVersion", "v4.0");
                provider = new CSharpCodeProvider(compilerInfo);
            }
            else if (sourceFile.Extension.ToUpper(CultureInfo.InvariantCulture) == ".VB")
            {
                provider = CodeDomProvider.CreateProvider("VisualBasic");
            }
            else
            {
                // Source file must have a .cs or .vb extension
            }

            if (provider != null)
            {
                CompilerParameters cp = new CompilerParameters();

                // Generate an executable instead of 
                // a class library.
                cp.GenerateExecutable = false;

                // Save the assembly as a physical file.
                cp.GenerateInMemory = true;

                // Set whether to treat all warnings as errors.
                cp.TreatWarningsAsErrors = false;

                // Add assemplies
                string extAssembly = Path.Combine(
                 Path.GetDirectoryName(Application.ExecutablePath),
                  "MyGeocachingManager.exe");
                cp.ReferencedAssemblies.Add(extAssembly);
                cp.ReferencedAssemblies.Add("System.dll");
                cp.ReferencedAssemblies.Add("System.Core.dll");
                cp.ReferencedAssemblies.Add("System.configuration.dll");
                cp.ReferencedAssemblies.Add("System.Data.dll");
                cp.ReferencedAssemblies.Add("System.Drawing.dll");
                cp.ReferencedAssemblies.Add("System.Windows.Forms.dll");
                cp.ReferencedAssemblies.Add("System.IO.Compression.dll");
                cp.ReferencedAssemblies.Add("System.IO.Compression.FileSystem.dll");
                cp.ReferencedAssemblies.Add(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "GMap.NET.Core.dll"));
                cp.ReferencedAssemblies.Add(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "GMap.NET.WindowsForms.dll"));
                cp.ReferencedAssemblies.Add(Path.Combine(GetInternalDataPath(), @"COTS\System.Data.SQLite.dll"));
                cp.ReferencedAssemblies.Add(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "HtmlAgilityPack.dll"));

                // Invoke compilation of the source file.
                cr = provider.CompileAssemblyFromFile(cp,
                    sourceName);

                // Return the results of the compilation.
                if (cr.Errors.Count > 0)
                {
                    compileOk = false;
                }
                else
                {
                    compileOk = true;
                }
            }
            return compileOk;
        }

        private String GetCompileErrors(CompilerResults cr, String sourceName)
        {
            String sResult = "";

            if (cr.Errors.Count > 0)
            {
                // Display compilation errors.
                sResult += String.Format("****************************\r\nErrors building {0} into {1}\r\n",
                    sourceName, cr.PathToAssembly);
                foreach (CompilerError ce in cr.Errors)
                {
                    sResult += String.Format("  {0}\r\n", ce.ToString());
                }
            }
            else
            {
                // Display a successful compilation message.
                sResult += String.Format("****************************\r\nSource {0} built into {1} successfully.\r\n",
                    sourceName, cr.PathToAssembly);
            }

            return sResult;
        }

        /// <summary>
        /// Extract a plugin from the compiled assembly and add it to plugin internal list
        /// </summary>
        /// <param name="assembly">Compiled assembly</param>
        /// <param name="bRemoveDuplicates">if true remove duplicated plugin</param>
        public void GetPlugins(Assembly assembly, bool bRemoveDuplicates)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (!type.IsClass || type.IsNotPublic) continue;
                Type[] interfaces = type.GetInterfaces();
                if (((IList<Type>)interfaces).Contains(typeof(IScriptV2)))
                {
                    IScriptV2 iScript = (IScriptV2)Activator.CreateInstance(type);
                    iScript.Initialize(this);

                    // add the script details to a collection 
                    //ScriptDetails.Add(string.Format("{0} ({1})\r\n", iScript.Name, iScript.Description));
                    if (bRemoveDuplicates)
                    {
                        // On ne duplicate pas un plugin avec le même type !
                        // On supprime l'ancien
                        Type ty = iScript.GetType();
                        IScriptV2 previous = null;
                        foreach (IScriptV2 sc in _iListScriptV2)
                        {
                            if (sc.GetType().ToString() == ty.ToString())
                            {
                                previous = sc;
                            }
                        }
                        if (previous != null)
                            _iListScriptV2.Remove(previous);
                    }
                    // On ajoute
                    _iListScriptV2.Add(iScript);
                }
                else if (((IList<Type>)interfaces).Contains(typeof(IScript)))
                {
                    IScript iScript = (IScript)Activator.CreateInstance(type);
                    iScript.Initialize(this);

                    // add the script details to a collection 
                    //ScriptDetails.Add(string.Format("{0} ({1})\r\n", iScript.Name, iScript.Description));
                    if (bRemoveDuplicates)
                    {
                        // On ne duplicate pas un plugin avec le même type !
                        // On supprime l'ancien
                        Type ty = iScript.GetType();
                        IScript previous = null;
                        foreach (IScript sc in _iListScript)
                        {
                            if (sc.GetType().ToString() == ty.ToString())
                            {
                                previous = sc;
                            }
                        }
                        if (previous != null)
                            _iListScript.Remove(previous);
                    }

                    _iListScript.Add(iScript);
                }
            }
        }

        private string CompilePlugins()
        {
            CompilerResults cr;
            String plugPath = GetUserDataPath() + Path.DirectorySeparatorChar + "Plugins";
            String sResult = "";

            if (Directory.Exists(plugPath))
            {
                string[] filePaths = Directory.GetFiles(plugPath, "*.*");
                foreach (string f in filePaths)
                {
                	LogToSplash("Loading plugin " + Path.GetFileName(f), false);
                    if (CompilePlugin(f, out cr))
                    {
                        GetPlugins(cr.CompiledAssembly, false);
                    }
                    else
                    {
                        if (cr != null)
                        {
                            sResult += GetCompileErrors(cr, f);
                        }
                    }
                }
                
                // Create menu entry
                CreatePluginMenu();
            }
            return sResult;
        }

        /// <summary>
        /// (Re)Create the plugin menu
        /// </summary>
        public void CreatePluginMenu()
        {
            // On efface le menu précédent si besoin
            if (_pluginToolStripMenuItem != null)
            {
                menuStrip1.Items.Remove(_pluginToolStripMenuItem);
                _pluginToolStripMenuItem.Dispose();
            }

            _pluginToolStripMenuItem = null;
            
            // On ajoute les entrées de base propres à MGM
            if (_pluginToolStripMenuItem == null)
				_pluginToolStripMenuItem = CreateTSMI("FMenuPlugins");
            _pluginToolStripMenuItem.DropDownItems.Add(downloadPluginsToolStripMenuItem);
            _pluginToolStripMenuItem.DropDownItems.Add(pluginscodeToolStripMenuItem);
            _pluginToolStripMenuItem.DropDownItems.Add(testpluginToolStripMenuItem);
            TranslateTooltips(_pluginToolStripMenuItem, _toolTipForMGM);
            
            bool bSeparator = false;
            	
            // On ajoute le vieux scripts
            if (_iListScript.Count != 0)
            {
            	if (!bSeparator)
	            {
	            	_pluginToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
	            	bSeparator = true;
	            }
				
                foreach (IScript script in _iListScript)
                {
                    ToolStripMenuItem aPlugin = new ToolStripMenuItem(script.Name);
                    aPlugin.ToolTipText = script.Description;
                    aPlugin.Tag = script;
                    aPlugin.Name = "_pluginToolStripMenuItem" + script.Name;
                    aPlugin.Click += new System.EventHandler(aPluginToolStripMenuItem_Click);
                    _pluginToolStripMenuItem.DropDownItems.Add(aPlugin);
                }
            }
            
            // On ajoute les scripts nouveau format
            if (_iListScriptV2.Count != 0)
            {
            	if (!bSeparator)
	            {
	            	_pluginToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
	            	bSeparator = true;
	            }
            	
                foreach (IScriptV2 script in _iListScriptV2)
                {
                    ToolStripMenuItem aPlugin = new ToolStripMenuItem(script.Name);
                    aPlugin.ToolTipText = script.Description;
                    aPlugin.Name = "_pluginToolStripMenuItem" + script.Name;
                    aPlugin.Tag = script;

                    // Now fetch all the functions
                    // If we have only one function, do not create submenu
                    if (script.Functions.Count == 1)
                    {
                        IScriptV2Function sfct = new IScriptV2Function();
                        sfct.fctName = script.Functions.First().Value;
                        sfct.script = script;
                        aPlugin.Tag = sfct;
                        aPlugin.Click += new System.EventHandler(aPluginFctToolStripMenuItem_Click);
                        _pluginToolStripMenuItem.DropDownItems.Add(aPlugin);
                    }
                    else
                    {
                        _pluginToolStripMenuItem.DropDownItems.Add(aPlugin);
                        foreach (KeyValuePair<String, String> paire in script.Functions)
                        {
                            ToolStripMenuItem aPluginFct = new ToolStripMenuItem(paire.Key);
                            IScriptV2Function sfct = new IScriptV2Function();
                            sfct.fctName = paire.Value;
                            sfct.script = script;
                            aPluginFct.Tag = sfct;
                            aPluginFct.Name = "_pluginToolStripMenuItem" + script.Name + paire.Key;
                            aPlugin.DropDownItems.Add(aPluginFct);
                            aPluginFct.Click += new System.EventHandler(aPluginFctToolStripMenuItem_Click);
                        }
                    }
                }
            }
            
            // On ajoute tout le temps ce menu
            int i = menuStrip1.Items.IndexOf(aboutToolStripMenuItem);
            menuStrip1.Items.Insert(i, _pluginToolStripMenuItem);
        }

        private void aPluginFctToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem aPlugin = sender as ToolStripMenuItem;
            if ((aPlugin != null) && (aPlugin.Tag != null))
            {
                IScriptV2Function sfct = aPlugin.Tag as IScriptV2Function;
                IScriptV2 script = sfct.script;
                if (script != null)
                {
                    try
                    {
                        // Get the desired method by name: DisplayName
                        MethodInfo methodInfo = script.GetType().GetMethod(sfct.fctName);

                        // Use the instance to call the method without arguments
                        methodInfo.Invoke(script, null);
                    }
                    catch (Exception exc)
                    {
                        String msg = String.Format("Error executing:\r\nName: {0}\r\nDescription: {1}",
                            script.Name, script.Description);
                    	ShowException("", msg, exc);
                    }
                }
            }
        }

        private void aPluginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem aPlugin = sender as ToolStripMenuItem;
            if ((aPlugin != null)&&(aPlugin.Tag != null))
            {
                IScript script = aPlugin.Tag as IScript;
                if (script != null)
                {
                    try
                    {
                        script.DoIt();
                    }
                    catch (Exception exc)
                    {
                        String msg = String.Format("Error executing:\r\nName: {0}\r\nDescription: {1}",
                            script.Name, script.Description);
                    	ShowException("", msg, exc);
                    }
                }
            }
        }

        private void Atts_pictureBox_MouseHover(object sender, EventArgs e, PictureBox pb, Label tb)
        {
            PictureBox p = sender as PictureBox;
            if (p != null)
            {
                AttributeImage at = p.Tag as AttributeImage;
                if (at != null)
                {
                    String text;
                    if (at._Index == 2)
                        text = GetTranslator().GetString(CreateAttributeTranslationKey(at._Key + "-no"));
                    else
                        text = GetTranslator().GetString(CreateAttributeTranslationKey(at._Key));

                    ToolTip tt = new ToolTip();
                    tt.SetToolTip(p, text);

                    pb.Image = at._Images[(at._Index == 2) ? 2 : 1];
                    tb.Text = text;
                }
            }
        }

        private void Atts_pictureBox_MouseHoverIn(object sender, EventArgs e)
        {
            Atts_pictureBox_MouseHover(sender, e, pictureBoxAttIn, textBoxAttIn);
        }

        private void Atts_pictureBox_MouseHoverOut(object sender, EventArgs e)
        {
            Atts_pictureBox_MouseHover(sender, e, pictureBoxAttOut, textBoxAttOut);
        }

        private void Atts_pictureBox_MouseLeaveIn(object sender, EventArgs e)
        {
            pictureBoxAttIn.Image = null;
            textBoxAttIn.Text = "";
        }

        private void Atts_pictureBox_MouseLeaveOut(object sender, EventArgs e)
        {
            pictureBoxAttOut.Image = null;
            textBoxAttOut.Text = "";
        }

        private void Atts_pictureBoxIn_Click(object sender, EventArgs e)
        {
            AttsPictureboxClick(sender, true);
        }

        private void Atts_pictureBoxOut_Click(object sender, EventArgs e)
        {
            AttsPictureboxClick(sender, false);
        }

        private void AttsPictureboxClick(object sender, bool bIn)
        {
            PictureBox p = sender as PictureBox;
            if (p != null)
            {
                AttributeImage at = p.Tag as AttributeImage;
                if (at != null)
                {
                    int i = at._Index + 1;
                    if ((i == 3) || (at._Images[i] == null))
                        i = 0;
                    at._Index = i;

                    if (i != 0)
                    {
                        if (bIn)
                        {
                            if (!cbFilterAttributeIn.Checked)
                                cbFilterAttributeIn.Checked = true;
                        }
                        else
                        {
                            if (!cbFilterAttributeOut.Checked)
                                cbFilterAttributeOut.Checked = true;
                        }
                    }

                    p.Image = at._Images[i];
                    p.Invalidate();
                }
            }
        }

        /// <summary>
        /// Get list of selected attributes (Geocaching), to be used with CacheFilter._attributes / _attributesexcl
        /// </summary>
        /// <param name="bIn">If true, retrieve attributes that shall be present in the cache, otherwise attributes that shall NOT be present in the cache</param>
        /// <returns>attribute list</returns>
        public List<String> GetSelecteditemsAtts(bool bIn)
        {
            List<AttributeImage> lst = _listPbIn;
            if (!bIn)
                lst = _listPbOut;

            List<String> selection = new List<string>();
            foreach (AttributeImage at in lst)
            {
                if (at._Index != 0)
                {
                    String key = at._Key;
                    if (at._Index == 2)
                        key += "-no";
                    selection.Add(key.Replace("/", ""));
                }
            }
            return selection;
        }

        /// <summary>
        /// Select in HIM a list of attributes (Geocaching)
        /// </summary>
        /// <param name="bIn">If true, retrieve attributes that shall be present in the cache, otherwise attributes that shall NOT be present in the cache</param>
        /// <param name="atts">attributes list</param>
        public void SetSelecteditemsAtts(bool bIn, List<String> atts)
        {
            List<AttributeImage> lst = _listPbIn;
            if (!bIn)
                lst = _listPbOut;

            SuspendLayout();
            foreach (AttributeImage at in lst)
            {
                // Raz
                at._Index = 0;
                at._PictureBox.Image = at._Images[at._Index];

                foreach (string s in atts)
                {
                    // We go through the list of items and select them if found
                    bool bNo = false;
                    String key = s;
                    if (s.EndsWith("-no"))
                    {
                        key = s.Replace("-no", "");
                        bNo = true;
                    }

                    if (at._Key.Replace("/", "") == key)
                    {
                        if (bNo)
                            at._Index = 2;
                        else
                            at._Index = 1;
                        at._PictureBox.Image = at._Images[at._Index];
                    }
                }
                at._PictureBox.Invalidate();
            }
            ResumeLayout(false);
        }
        
        /// <summary>
        /// Complete informations of selected caches.
        /// If parameters is null, all attributes will be completed, otherwise, indicates true for attributes
        /// that shall be completed in the array:
        /// 00 : All
        /// 01 : DateCreation
        /// 02 : Owner
        /// 03 : Status
        /// 04 : Difficulty
        /// 05 : Terrain
        /// 06 : Description
        /// 07 : Container
        /// 08 : Hint
        /// 09 : Attributes
        /// 10 : Logs
        /// 11 : Contry
        /// 12 : State
        /// 13 : Statistics
        /// 14 : Basic info (name, url, coordinates)
        /// 15 : Personal notes
        /// </summary>
        /// <param name="parameters">
        /// If parameters is null, all attributes will be completed, otherwise, indicates true for attributes
        /// that shall be completed in the array:
        /// 00 : All
        /// 01 : DateCreation
        /// 02 : Owner
        /// 03 : Status
        /// 04 : Difficulty
        /// 05 : Terrain
        /// 06 : Description
        /// 07 : Container
        /// 08 : Hint
        /// 09 : Attributes
        /// 10 : Logs
        /// 11 : Contry
        /// 12 : State
        /// 13 : Statistics
        /// 14 : Basic info (name, url, coordinates)
        /// 15 : Personal notes
        /// </param>
        public void CompleteSelectedCaches(bool[] parameters)
        {
            // Si parameters est nul, alors on ne complete que les caches vides
            // Sinon on complète tout mais uniquement sur ce qui est choisi
            List<Geocache> caches = null;
            try
            {
                _ThreadProgressBarTitle = GetTranslator().GetString("FMenuCompleteDesc");
                CreateThreadProgressBarEnh();

                caches = GetSelectedCaches();
                if (caches.Count == 0)
                {
                    KillThreadProgressBarEnh();
                    return;
                }

                // Wait for the creation of the bar
                while (_ThreadProgressBar == null)
                {
                    Thread.Sleep(10);
                    Application.DoEvents();
                }
                _ThreadProgressBar.progressBar1.Maximum = caches.Count();

                UpdateHttpDefaultWebProxy();
                // On checke que les L/MDP soient corrects
                // Et on récupère les cookies au passage
                CookieContainer cookieJar = CheckGCAccount(true, false);
                if (cookieJar == null)
                    return;

                DataForStatsRetrieval dataforstats = new DataForStatsRetrieval();
                dataforstats.cookieJar = cookieJar;
                dataforstats.firstQuestion = true;
                dataforstats.inbmissed = 0;
                dataforstats.stopScoreRetrieval = false;

                foreach (Geocache geo in caches)
                {
                    if ((parameters == null) && (geo.ShouldBeCompleted() == false))
                    {
                        _ThreadProgressBar.Step();
                        continue;
                    }

                    string result = GetCacheHTMLFromClientImpl(geo._Url, cookieJar);
                    
                    // 2 - Parse values
                    bool bOk = CompleteCacheFromHTML(geo, result, parameters, dataforstats, false);
                    _cacheStatus.SaveCacheStatus();
                    
                    _ThreadProgressBar.Step();
                    if (_ThreadProgressBar._bAbort)
                        break;
                }

                PostTreatmentLoadCache();
                
                // Si on a mis à jour les stats
                if ((parameters == null) || parameters[0] || parameters[13])
                {
                    // Final wrapup
                    _od.Serialize(_odfile);
                    // Better way to do that : only recreate for modified caches
                    _cacheDetail.EmptyStatsMarkers();
                }

                // Better way to do that : only recreate for modified caches
                RecreateVisualElements(caches);

                // On redessine la carte
                BuildCacheMapNew(GetDisplayedCaches());
				
                KillThreadProgressBarEnh();

                String msg = GetTranslator().GetString("LblActionDone");
                MessageBoxIcon icon = MessageBoxIcon.Information;
                if ((parameters == null) || parameters[0] || parameters[13])
                {
                    if (dataforstats.inbmissed != 0)
                    {
                        msg += "\r\n" + String.Format(GetTranslator().GetString("LblErrorRetrieveStats"), dataforstats.inbmissed);
                        icon = MessageBoxIcon.Error;
                    }
                }

                if (icon == MessageBoxIcon.Information)
                	MsgActionDone(this);
                else
                	MsgActionError(this, msg);
            }
            catch (Exception ex)
            {
                PostTreatmentLoadCache();

                // Si on a mis à jour les stats
                if ((parameters == null) || parameters[0] || parameters[13])
                {
                    // Final wrapup
                    _od.Serialize(_odfile);
                    // Better way to do that : only recreate for modified caches
                    _cacheDetail.EmptyStatsMarkers();
                }

                // Better way to do that : only recreate for modified caches
                RecreateVisualElements(caches);

                KillThreadProgressBarEnh();

                ShowException("", GetTranslator().GetString("FMenuCompleteDesc"), ex);
            }
        }

        /// <summary>
        /// Complete informations of selected caches. Impacts only the list, not MGM inner information.
        /// If parameters is null, all attributes will be completed, otherwise, indicates true for attributes
        /// that shall be completed in the array:
        /// 00 : All
        /// 01 : DateCreation
        /// 02 : Owner
        /// 03 : Status
        /// 04 : Difficulty
        /// 05 : Terrain
        /// 06 : Description
        /// 07 : Container
        /// 08 : Hint
        /// 09 : Attributes
        /// 10 : Logs
        /// 11 : Contry
        /// 12 : State
        /// 13 : Statistics
        /// 14 : Basic info (name, url, coordinates)
        /// 15 : Personal notes
        /// </summary>
        /// <param name="caches">list of caches to complete</param>
        /// <param name="parameters">
        /// If parameters is null, all attributes will be completed, otherwise, indicates true for attributes
        /// that shall be completed in the array:
        /// 00 : All
        /// 01 : DateCreation
        /// 02 : Owner
        /// 03 : Status
        /// 04 : Difficulty
        /// 05 : Terrain
        /// 06 : Description
        /// 07 : Container
        /// 08 : Hint
        /// 09 : Attributes
        /// 10 : Logs
        /// 11 : Contry
        /// 12 : State
        /// 13 : Statistics
        /// 14 : Basic info (name, url, coordinates)
        /// 15 : Personal notes
        /// /// <param name="bSilent">if true, only error messages will be displayed</param>
        /// </param>
        /// <returns>true if not aborted</returns>
        public bool CompleteSelectedCaches(ref List<Geocache> caches, bool[] parameters, bool bSilent)
        {
            // Si parameters est nul, alors on ne complete que les caches vides
            // Sinon on complète tout mais uniquement sur ce qui est choisi
            bool bGood = true;
            try
            {
                if (caches.Count == 0)
                {
                    return true;
                }

                _ThreadProgressBarTitle = GetTranslator().GetString("FMenuCompleteDesc");
                CreateThreadProgressBarEnh();

                // Wait for the creation of the bar
                while (_ThreadProgressBar == null)
                {
                    Thread.Sleep(10);
                    Application.DoEvents();
                }
                _ThreadProgressBar.progressBar1.Maximum = caches.Count();

                UpdateHttpDefaultWebProxy();
                // On checke que les L/MDP soient corrects
                // Et on récupère les cookies au passage
                CookieContainer cookieJar = CheckGCAccount(true, false);
                if (cookieJar == null)
                    return false;

                DataForStatsRetrieval dataforstats = new DataForStatsRetrieval();
                dataforstats.cookieJar = cookieJar;
                dataforstats.firstQuestion = true;
                dataforstats.inbmissed = 0;
                dataforstats.stopScoreRetrieval = false;

                foreach (Geocache geo in caches)
                {
                    if ((parameters == null) && (geo.ShouldBeCompleted() == false))
                    {
                        _ThreadProgressBar.Step();
                        continue;
                    }

                    string result = GetCacheHTMLFromClientImpl(geo._Url, cookieJar);
                    // 2 - Parse values
                    bool bOk = CompleteCacheFromHTML(geo, result, parameters, dataforstats, false);
                    _cacheStatus.SaveCacheStatus();
                    
                    _ThreadProgressBar.Step();
                    if (_ThreadProgressBar._bAbort)
                    {
                        bGood = false;
                        break;
                    }
                }

                KillThreadProgressBarEnh();

                String msg = GetTranslator().GetString("LblActionDone");
                MessageBoxIcon icon = MessageBoxIcon.Information;
                if ((parameters == null) || parameters[0] || parameters[13])
                {
                    if (dataforstats.inbmissed != 0)
                    {
                        bSilent = false;
                        msg += "\r\n" + String.Format(GetTranslator().GetString("LblErrorRetrieveStats"), dataforstats.inbmissed);
                        icon = MessageBoxIcon.Error;
                    }
                }
                if (!bSilent)
                {
                	if (icon == MessageBoxIcon.Information)
	                	MsgActionDone(this);
	                else
                 	   MsgActionError(this, msg);
                }
            }
            catch (Exception ex)
            {
                bGood = false;
                KillThreadProgressBarEnh();

                ShowException("", GetTranslator().GetString("FMenuCompleteDesc"), ex);
            }
            return bGood;
        }

        /// <summary>
        /// Get HTML source of an URL, using a GC.com authentication cookie
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="cookieJar">GC.com authentication cookie</param>
        /// <returns>HTML source</returns>
        public string GetCacheHTMLFromClientImpl(String url, CookieContainer cookieJar)
        {
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
            objRequest.Proxy = GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
            objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
            String response = "";
            using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
            {
                response = responseStream.ReadToEnd();
                responseStream.Close();
            }
            return response;
        }

        /// <summary>
        /// Return true if cache is premium and we are basic
        /// </summary>
        /// <param name="html">cache html page</param>
        /// <returns>true if cache is premium and we are basic</returns>
        public bool IsPremiumCacheAndWeAreBasic(String html)
        {
        	//Si on est tombé sur une cache premium et que nous ne sommes pas premium
            // alors on trouve ce bloc :
            // ctl00_ContentBody_memberComparePanel
            // Dans ce cas là on sort direct
            return html.Contains("ctl00_ContentBody_memberComparePanel");
        }
        
        /// <summary>
        /// Complete a geocache by parsing the HTML source of its GC.com page
        /// If parameters is null, all attributes will be completed, otherwise, indicates true for attributes
        /// that shall be completed in the array:
        /// 00 : All
        /// 01 : DateCreation
        /// 02 : Owner
        /// 03 : Status
        /// 04 : Difficulty
        /// 05 : Terrain
        /// 06 : Description
        /// 07 : Container
        /// 08 : Hint
        /// 09 : Attributes
        /// 10 : Logs
        /// 11 : Contry
        /// 12 : State
        /// 13 : Statistics
        /// 14 : Basic info (name, url, coordinates)
        /// 15 : Personal notes
        /// </summary>
        /// <param name="geo">Geocache to complete. At least _Code SHALL be indicated</param>
        /// <param name="result">HTML source</param>
        /// <param name="parameters">
        /// If parameters is null, all attributes will be completed, otherwise, indicates true for attributes
        /// that shall be completed in the array:
        /// 00 : All
        /// 01 : DateCreation
        /// 02 : Owner
        /// 03 : Status
        /// 04 : Difficulty
        /// 05 : Terrain
        /// 06 : Description
        /// 07 : Container
        /// 08 : Hint
        /// 09 : Attributes
        /// 10 : Logs
        /// 11 : Contry
        /// 12 : State
        /// 13 : Statistics
        /// 14 : Basic info (name, url, coordinates)
        /// 15 : Personal notes
        /// </param>
        /// <param name="dataforstats">Class holding current status of completion (previously raised warning, etc.)</param>
        /// <param name="bDiagnostic">If true, indicates that a partial processing for diagnostic purposes will be performed</param>
        /// <returns>True if cache is completed successfully</returns>
        public bool CompleteCacheFromHTML(Geocache geo, string result, bool[] parameters, DataForStatsRetrieval dataforstats, bool bDiagnostic)
        {
        	String owner = ConfigurationManager.AppSettings["owner"].ToLower();
        	Log("*** CompleteCacheFromHTML (start) *** ");
        	// 00 : All
            // 01 : DateCreation
            // 02 : Owner
            // 03 : Status
            // 04 : Difficulty
            // 05 : Terrain
            // 06 : Description
            // 07 : Container
            // 08 : Hint
            // 09 : Attributes
            // 10 : Logs
            // 11 : Pays
            // 12 : Etat
            // 13 : Stats
            // 14 : Basic info (name, url, coordinates)
            // 15 : Personal notes
            // Est-ce qu'on demande de modifier UNIQUEMENT les stats (donc juste un impact sur l'OCD et par la cache en elle même ?)
            
            // Quoi qu'il advienne, si le code n'est pas renseigné, on le récupère
            if (String.IsNullOrEmpty(geo._Code))
            {
                geo._Code = MyTools.GetSnippetFromText("cache_details.aspx?wp=", "&amp", result);
                _iNbModifiedCaches += geo.InsertModification("CODE");
            }

            //Si on est tombé sur une cache premium et que nous ne sommes pas premium
            // Dans ce cas là on sort direct
            if (IsPremiumCacheAndWeAreBasic(result))
            {
            	Log("This cache is premium and we are not... " + geo._Code);
            	return false;
            }
            
            String PATTERN_OWNER_DISPLAYNAME = "<div id=\"ctl00_ContentBody_mcd1\">[^<]+<a href=\"[^\"]+\">([^<]+)</a>";
            
            String tmp = "";
            DateTime dateLog = DateTime.Now;
            // _DateCreation
            if ((parameters == null) || (parameters[0]) || (parameters[1]))
            {
                tmp = MyTools.GetSnippetFromText("ctl00_ContentBody_mcd2", "ctl00_ContentBody_diffTerr", result);
                tmp = MyTools.GetSnippetFromText(":", "</div>", tmp);
                tmp = MyTools.CleanString(tmp);
                if (DateTime.TryParse(tmp, out dateLog))
                {
                    if ((String.IsNullOrEmpty(geo._DateCreation)) ||
                        (geo._DateCreation.Length < 10) ||
                        (geo._DateCreation.Substring(0, 10) != dateLog.ToString(GeocachingConstants._FalseDatePattern).Substring(0, 10))
                    )
                    {
                        geo._DateCreation = dateLog.ToString(GeocachingConstants._FalseDatePattern);
                        _iNbModifiedCaches += geo.InsertModification("DATE");
                    }
                }
            }

            // _Owner / _OwnerId
            // _PlacedBy
            if ((parameters == null) || (parameters[0]) || (parameters[2]))
            {
                //tmp = GetSnippetFromText("ctl00_ContentBody_mcd1", "ctl00_ContentBody_diffTerr", result);
                //tmp = GetSnippetFromText("A cache by ", "</a>", result);
                //tmp = GetSnippetFromText(">", "", tmp);
                tmp = MyTools.DoRegex(result, PATTERN_OWNER_DISPLAYNAME);
                tmp = HtmlAgilityPack.HtmlEntity.DeEntitize(tmp);
                if (geo._Owner != tmp)
                {
                    geo._Owner = tmp;
                    _iNbModifiedCaches += geo.InsertModification("OWNER");
                }
                if (geo._PlacedBy != tmp)
                {
                    geo._PlacedBy = tmp;
                    _iNbModifiedCaches += geo.InsertModification("PLACEDBY");
                }

                // Est-ce la notre ?
                bool bold = geo._bOwned;
                if (owner.ToLower() == geo._Owner.ToLower())
                    geo._bOwned = true;
                else
                    geo._bOwned = false;
                if (bold != geo._bOwned)
                    _iNbModifiedCaches += geo.InsertModification("OWNED");

                // Est-elle trouvée par nous ?
                tmp = MyTools.GetSnippetFromText("ctl00_ContentBody_GeoNav_foundStatus", "ctl00_ContentBody_GeoNav_logTypeImage", result);
                bool bModif = false;
                if (tmp == "")
                {
                    // Pas de bloc "trouvé" présent
                    if (geo._bFound)
                    {
                    	geo._bFound = false;
                    	bModif = true;
                    }
                }
                else
                {
                    // On cherche <img src="/images/logtypes/48/2.png" id="ctl00_ContentBody_GeoNav_logTypeImage" /> qui correspond à Found It
                    tmp = MyTools.GetSnippetFromText("/images/logtypes/48/", ".png\" id", tmp);
                    if (tmp == "2")
                    {
                        // On va considérer ça comme un marquage trouvé par l'utilisateur
                        // si cette information n'était pas déjà présente dans la cache
                        if (geo.IsFound() == false)
	                    {
                        	if (_cacheStatus.DeclareFoundCache(owner, geo._Code))
		            		{
		            			geo._bFoundInMGM = true;
		            			bModif = true;
		            		}
                        }
                    }
                    else
                    {
                        if (geo._bFound)
	                    {
	                    	geo._bFound = false;
	                    	bModif = true;
	                    }
                    }
                }
                if (bModif)
                	_iNbModifiedCaches += geo.InsertModification("FOUND");
            }

            // Archived / available
            if ((parameters == null) || (parameters[0]) || (parameters[3]))
            {
                tmp = MyTools.GetSnippetFromText("<ul class=\"OldWarning\">", "</ul>", result);
                if (tmp != "")
                {
                    // soit archivée, soit désactivée
                    // Si on trouve "archiv" dans le texte, c'est archivé, sinon c'est désactivé
                    String s1 = geo._Available;
                    String s2 = geo._Archived;

                    if (tmp.Contains("archiv"))
                    {
                        geo._Available = "False";
                        geo._Archived = "True";
                    }
                    else
                    {
                        geo._Available = "False";
                        geo._Archived = "False";
                    }
                    if (s1 != geo._Available)
                        _iNbModifiedCaches += geo.InsertModification("AVAILABLE");
                    if (s2 != geo._Archived)
                        _iNbModifiedCaches += geo.InsertModification("ARCHIVE");
                }
            }

            // Difficulty
            String sold = geo._D;
            if ((parameters == null) || (parameters[0]) || (parameters[4]))
            {
                tmp = MyTools.GetSnippetFromText("<div id=\"ctl00_ContentBody_diffTerr", "</div>", result);
                tmp = MyTools.GetSnippetFromText("ctl00_ContentBody_uxLegendScale", "</span>", tmp);
                tmp = MyTools.GetSnippetFromText("/images/stars/stars", ".gif", tmp);
                geo._D = tmp.Replace("_", ".");
                geo._D = geo._D.Replace(".0", "");
                if (sold != geo._D)
                    _iNbModifiedCaches += geo.InsertModification("D");
                
            }

            // Terrain
            sold = geo._T;
            if ((parameters == null) || (parameters[0]) || (parameters[5]))
            {
                tmp = MyTools.GetSnippetFromText("<div id=\"ctl00_ContentBody_diffTerr", "</div>", result);
                tmp = MyTools.GetSnippetFromText("ctl00_ContentBody_Localize12", "</span>", tmp);
                tmp = MyTools.GetSnippetFromText("/images/stars/stars", ".gif", tmp);
                geo._T = tmp.Replace("_", ".");
                geo._T = geo._T.Replace(".0", "");
                if (sold != geo._T)
                    _iNbModifiedCaches += geo.InsertModification("T");
            }

            // Les descriptions
            if ((parameters == null) || (parameters[0]) || (parameters[6]))
            {
                // _ShortDescription / _ShortDescHTML
                tmp = MyTools.GetSnippetFromText("<span id=\"ctl00_ContentBody_ShortDescription\">", "</span>", result);
                geo._ShortDescHTML = "True";
                if (geo._ShortDescription != tmp)
                {
                    geo._ShortDescription = tmp;
                    _iNbModifiedCaches += geo.InsertModification("SHORTDESC");
                }
                
                // _LongDescription / _LongDescHTML
                tmp = MyTools.GetSnippetFromText("<span id=\"ctl00_ContentBody_LongDescription\">", "</span>", result);
                geo._LongDescHTML = "True";
                if (geo._LongDescription != tmp)
                {
                    geo._LongDescription = tmp;
                    _iNbModifiedCaches += geo.InsertModification("LONGDESC");
                }
            }

            // _Container
            if ((parameters == null) || (parameters[0]) || (parameters[7]))
            {
                tmp = MyTools.GetSnippetFromText("ctl00_ContentBody_size", "</div>", result);
                tmp = MyTools.GetSnippetFromText("/images/icons/container/", ".", tmp);
                if (geo._Container != UppercaseFirst(tmp))
                {
                    geo._Container = UppercaseFirst(tmp);
                    _iNbModifiedCaches += geo.InsertModification("CONTAINER");
                }
            }

            // _Hint
            if ((parameters == null) || (parameters[0]) || (parameters[8]))
            {
                tmp = MyTools.GetSnippetFromText("<div id=\"div_hint\" class=\"span-8 WrapFix\">", "</div>", result);
                if (tmp != "")
                    tmp = ROT13.Transform(tmp); // décrypte tout ça
                if (geo._Hint != tmp)
                {
                    geo._Hint = tmp;
                    _iNbModifiedCaches += geo.InsertModification("HINT");
                }
            }

            // _Attributes / _listAttributesId
            if ((parameters == null) || (parameters[0]) || (parameters[9]))
            {
                tmp = MyTools.GetSnippetFromText("ctl00_ContentBody_detailWidget", "ctl00_ContentBody_uxBanManWidget", result);
                tmp = MyTools.GetSnippetFromText("<div class=\"WidgetBody\">", "</div>", tmp);
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(tmp);
                var imageNodes = doc.DocumentNode.SelectNodes("//img");

                // Les anciens attributs pour comparer
                List<String> lold = new List<string>(geo._Attributes);
                geo._Attributes.Clear();
                geo._listAttributesId.Clear();
                
                if (imageNodes != null)
                {
                    foreach (HtmlAgilityPack.HtmlNode node in imageNodes)
                    {
                        if (node.Attributes.Contains("src"))
                        {
                            String key = node.Attributes["src"].Value.Replace("/images/attributes/", "").Replace(".gif", "").ToLower();
                            if (_tableAttributesCompleted.ContainsKey(key))
                            {
                                String id = _tableAttributesCompleted[key];
                                key = _tableAttributes[id];
                                id = id.Replace("-no", "");
                                geo._Attributes.Add(key);
                                geo._listAttributesId.Add(id);
                            }
                            else
                            {
                                if (key != "attribute-blank")
                                {
                                    geo._Attributes.Add(key);
                                    geo._listAttributesId.Add("-1");
                                }
                            }
                        }
                    }
                    if (MyTools.ScrambledEquals<String>(lold, geo._Attributes) == false)
                        _iNbModifiedCaches += geo.InsertModification("ATTRIBUTES");
                }
            }

            
            // _Logs
            if ((parameters == null) || (parameters[0]) || (parameters[10]))
            {
                // initalLogs = {"status"
                tmp = MyTools.GetSnippetFromText("initalLogs = {\"status\"", "var gaToken", result);
                List<String> logs = MyTools.GetSnippetsFromText("{\"LogID\"", "\"Images\":", tmp);
                
                // Si des logs existent, il ne faut pas les dupliquer et surtout il faut tout mettre dans le bon ordre
                // On crée une liste d'anciens logs s'ils existent
                Dictionary<String, CacheLog> newLogs = new Dictionary<string, CacheLog>();
                foreach (CacheLog log in geo._Logs)
                {
                    newLogs.Add(log._SortingKey, log);
                }

                foreach (String sl in logs)
                {
                    CacheLog log = new CacheLog(this);
                    log._Date = MyTools.GetSnippetFromText("Visited\":\"", "\"", sl);
                    // stupid date is inverted: month/days/year
                    dateLog = DateTime.Now;
                    //if (DateTime.TryParseExact(log._Date, "MM/dd/yyyy", null, DateTimeStyles.None, out dateLog) == true)
                    if (DateTime.TryParse(log._Date, out dateLog))
                    {
                        //Log(log._Date + " => " + dateLog.ToString(GeocachingConstants._FalseDatePattern));
                        log._Date = dateLog.ToString(GeocachingConstants._FalseDatePattern);
                    }

                    log._Type = MyTools.GetSnippetFromText("LogType\":\"", "\"", sl);
                    log._User = MyTools.GetSnippetFromText("UserName\":\"", "\"", sl);
                    log._Text = MyTools.GetSnippetFromText("LogText\":\"", "\",\"Created", sl);
                    // BUG GC.com
                    log._Text = log._Text.Replace("\\n","");
                    
                    log._Encoded = UppercaseFirst(MyTools.GetSnippetFromText("IsEncoded\":", ",\"creator", sl));
                    log._LogId = MyTools.GetSnippetFromText(":", ",\"CacheID\"", sl); // LogID en tout début
                    log._FinderId = MyTools.GetSnippetFromText("AccountID\":", ",\"AccountGuid", sl);
                    log._SortingKey = CreateKeyForLogSorting(log);

                    // On ajoute le log uniquement s'il n'est pas déjà présent (test sur _LogId)
                    // Les logs parsés sont par odre chronologique croissant
                    // s'il y a déjà des logs Il faut ajouter ce paquet en début de la liste déjà présente
                    
                    
                    // On n'ajoute que si la key n'est pas trouvée !
                    // ET si le logid n'existe pas non plus :-(
                    String sKeyTrouvee = "";
                    foreach (KeyValuePair<String, CacheLog> paire in newLogs)
                    {
                        if ((paire.Key == log._SortingKey) || (paire.Value._LogId == log._LogId))
                            sKeyTrouvee = paire.Key;
                    }
                    if (sKeyTrouvee == "")
                        newLogs.Add(log._SortingKey, log);
                    else
                        newLogs[sKeyTrouvee] = log;
                    
                }
                // On remplace les logs par la liste triée selon la key
                var list = newLogs.Keys.ToList();
	            list.Sort();
                list.Reverse();
                geo._Logs = new List<CacheLog>();
                foreach (var key in list)
	            {
                    geo._Logs.Add(newLogs[key]);
                }
                _iNbModifiedCaches += geo.InsertModification("LOGS");
            }

            // _Country
            if ((parameters == null) || (parameters[0]) || (parameters[11]))
            {
                tmp = MyTools.GetSnippetFromText("<span id=\"ctl00_ContentBody_Location\">", "</span>", result);
                String tmp2 = MyTools.GetSnippetFromText("<a href=\"/map/default.aspx", "</a>", tmp);
                if (tmp2 != "") // Parfois le bloc map est présent...
                    tmp = tmp2;
                tmp = MyTools.GetSnippetFromText(", ", "", tmp);
                if (geo._Country != tmp)
                {
                    geo._Country = tmp;
                    _iNbModifiedCaches += geo.InsertModification("COUNTRY");
                }
            }

            // _State
            if ((parameters == null) || (parameters[0]) || (parameters[12]))
            {
                tmp = MyTools.GetSnippetFromText("<span id=\"ctl00_ContentBody_Location\">", "</span>", result);
                String tmp2 = MyTools.GetSnippetFromText("<a href=\"/map/default.aspx", "</a>", tmp);
                if (tmp2 != "") // Parfois le bloc map est présent...
                {
                    tmp = tmp2;
                    tmp = MyTools.GetSnippetFromText("\">", ",", tmp);
                }
                else
                    tmp = MyTools.GetSnippetFromText(" ", ",", tmp);
                if (geo._State != tmp)
                {
                    geo._State = tmp;
                    _iNbModifiedCaches += geo.InsertModification("STATE");
                }
            }

            // les stats
            // Attention, on le fait uniquement on a défini les paramètres
            // Sinon on le fait si on n'a juste demandé la complétion des caches vides
            if ((parameters == null) || (parameters[0]) || (parameters[13]))
            {
                // Pas de modification de bImpactOCDOnly
                // On va récupérer les stats
                // *************************
                // Retrieve all logs, only if favorites succeeded, otherwise it's pointless
                int ifav = -1;
                int ifound = -1;
                int inotfound = -1;
                int ifoundpremium = -1;
                int inotfoundpremium = -1;
                String usertoken = GetBasicStatsFromHTML(result, ref ifav, ref ifound, ref inotfound);

                // On demande les stats (score de favoris)
                // ***************************************
                String sScoreFavori = "";
                if (_bPopulariteBasique)
                {
                    if ((ifound > 0) && (ifav > 0))
                    {
                        sScoreFavori = ((int)((double)ifav / (double)ifound * 100.0)).ToString();
                    }
                    else
                    {
                        sScoreFavori = "0";
                    }
                }
                else
                    GetAdvancedStatsFromHTML(result, ref dataforstats.inbmissed, dataforstats.cookieJar, ref dataforstats.stopScoreRetrieval, ref dataforstats.firstQuestion, ifav, usertoken, ref sScoreFavori);

                // On renseigne les stats avancées
                // ###############################
                ComputeAdvancedStats(ifav, ref ifoundpremium, ref inotfoundpremium, sScoreFavori);

                // on met à jour la cache avec ses stats
                UpdateCacheWithStats(geo, ifav, ifound, inotfound, ifoundpremium, inotfoundpremium, bDiagnostic);
            }

            // Basic cache info _Latitude, _Longitude, _Name, _Url
            if ((parameters == null) || (parameters[0]) || (parameters[14]))
            {

                // Coordonnnées
                String latlon = MyTools.DoRegex(result, "<span id=\"uxLatLon\"[^>]*>(.*?)</span>");
                String sLat = "";
                String sLon = "";
                if (CoordConvHMI.DDMMMtoDDD(latlon, ref sLat, ref sLon))
                {
                    if ((geo._Latitude != sLat) || (geo._Longitude != sLon))
                    {
                        geo._Latitude = sLat;
                        geo._Longitude = sLon;
                        geo._dLatitude = MyTools.ConvertToDouble(geo._Latitude);
                        geo._dLongitude = MyTools.ConvertToDouble(geo._Longitude);
                        geo.UpdateDistanceToHome(_dHomeLat, _dHomeLon);
                        _iNbModifiedCaches += geo.InsertModification("COORD");
                    }
                }

                // url
                if (geo._Url == "")//!= ("http://coord.info/" + geo._Code))
                {
                    geo._Url = "http://coord.info/" + geo._Code;
                    _iNbModifiedCaches += geo.InsertModification("URL");
                }

                // Le nom
                tmp = MyTools.GetSnippetFromText("<span id=\"ctl00_ContentBody_CacheName\">", "</span>", result);
                if (geo._Name != tmp)
                {
                    geo._Name = tmp;
                    _iNbModifiedCaches += geo.InsertModification("NAME");
                }

                // Le type
                // On le trouve dans le Header, en clair, 
                // <head id="ctl00_Head1"><meta charset="utf-8" /><meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1" /><title>
	            // GC1DZYK Montfort 1 (Wherigo Cache) in Île-de-France, France created by allec
                // </title>
                // En gros il faut récupérer ce qui est entre les ( ) en ayant viré [CODE] [NOM] juste avant
                String codename = geo._Code + " " + geo._Name + " (";
                tmp = MyTools.GetSnippetFromText("<title>", "</title>", result);
                tmp = MyTools.GetSnippetFromText(codename, ")", tmp);
                if (geo._Type != tmp)
                {
                    geo._Type = tmp;
                    _iNbModifiedCaches += geo.InsertModification("TYPE");
                }
            }

            // Personal notes
            if ((parameters == null) || (parameters[0]) || (parameters[15]))
            {
            	// *******************
            	tmp = MyTools.GetSnippetFromText("<span id=\"cache_note\">", "</span>", result);
            	if (tmp != "")
            	{
            		tmp = tmp.Replace("\n","\r\n");
            		tmp = tmp.Replace("\r\r\n","\r\n");
            		// On a récupéré une note.
            		// Do we need to create a new OCD object ?
		            if (geo._Ocd == null)
		            {
		                // yes we create it
		                OfflineCacheData ocd1 = new OfflineCacheData();
		                ocd1._Code = geo._Code;
		                if (!bDiagnostic)
		                    AssociateOcdCache(geo._Code, ocd1, geo);
		                else
		                {
		                    geo._Ocd = ocd1;
		                }
		            }
		            
		            // Maintenant on a un OCD, on regarde si le texte était déjà présent
		            if (String.IsNullOrEmpty(geo._Ocd._Comment))
		            {
		            	geo._Ocd._Comment = tmp;
		            	_od.Serialize(_odfile);
		            	//_iNbModifiedCaches += geo.InsertModification("NOTE");
                    	UpdateListViewOfflineIcons(geo._Ocd);
		            }
		            else
		            {
		            	if (geo._Ocd._Comment.Contains(tmp) == false)
		            	{
		            		geo._Ocd._Comment += "\r\n" + tmp;
		            		_od.Serialize(_odfile);
		            		//_iNbModifiedCaches += geo.InsertModification("NOTE");
		            		
		            	}
		            	// Sinon on ne fait rien
		            }
            	}
            }
            //Log(geo.ToString());
            
            // Mise à jour uiniquement en mode nominal
            geo.UpdatePrivateData(owner);

			Log("*** CompleteCacheFromHTML (end) *** ");
            return true;
        }

        /// <summary>
        /// Create log key for sorting purposes
        /// </summary>
        /// <param name="log">Cache log</param>
        /// <returns>key for sorting</returns>
        public String CreateKeyForLogSorting(CacheLog log)
        {
            // pour un meilleur tri ! en premier la date du log puis le chrono du log
            // Pour des dates identiques, c'est le chrono qui permettra de différencier
            String dateLog = MyTools.CleanDate(log._Date);
            int tailleidmax = 12; // longeur de l'id
            string logid = log._LogId;
            if (logid.Length < tailleidmax)
                logid = logid.PadLeft(tailleidmax - logid.Length);
            return dateLog + logid;
        }

        /// <summary>
        /// Recreate all listview cache items
        /// </summary>
        /// <param name="caches">list of caches to recreate</param>
        public void RecreateVisualElements(List<Geocache> caches)
        {
            RecreateVisualElements(caches, false);
        }

        /// <summary>
        /// Recreate all listview cache items
        /// </summary>
        /// <param name="caches">list of caches to recreate</param>
        /// <param name="bForceEvenIfNotDisplayed">Item will be recreated even if not currently displayed on list</param>
        public void RecreateVisualElements(List<Geocache> caches, bool bForceEvenIfNotDisplayed)
        {
            if ((caches == null) || (caches.Count == 0))
                return;

            int iNbDaysForNew = Int32.Parse(ConfigurationManager.AppSettings["daysfornew"]);
            String kmmi = (_bUseKm) ? GetTranslator().GetString("LVKm") : GetTranslator().GetString("LVMi");
            bool bMarkMissingCache = (ConfigurationManager.AppSettings["markmissingcaches"] == "True");
            int numberlastlogssymbols = Int32.Parse(ConfigurationManager.AppSettings["numberlastlogssymbols"]);

            lvGeocaches.BeginUpdate();
            foreach (Geocache cache in caches)
            {
                // Replace existing element in the listview
                int iitem = GetVisualEltIndexFromCacheCode(cache._Code);
                if (iitem != -1)
                {
                    // Create new item for listview
                    EXImageListViewItem item = CreateVisualEltFromGeocache(iNbDaysForNew, kmmi, bMarkMissingCache, cache, numberlastlogssymbols);

                    // remove old item from the list view
                    _listViewCaches.Remove((EXImageListViewItem)(lvGeocaches.Items[iitem]));

                    // Replace old item in listview
                    lvGeocaches.Items[iitem] = item;
                    
                    // don't forget to add this to the cache list
                	_listViewCaches.Add(item);

                    lvGeocaches.Invalidate(item.Bounds);
                }
                else
                {
                    // Item wasn't in the list
                    if (bForceEvenIfNotDisplayed)
                    {
                        // Create new item for listview
                        EXImageListViewItem item = CreateVisualEltFromGeocache(iNbDaysForNew, kmmi, bMarkMissingCache, cache, numberlastlogssymbols);

                        // remove old item from the list view
                        // On va devoir le trouver à partir de son code !!!
                        foreach (EXImageListViewItem ei in _listViewCaches)
                        {
                            String code = ei.Text;
                            if (code == cache._Code)
                            {
                                _listViewCaches.Remove(ei);
                                break;
                            }
                        }
                        
                        // don't forget to add this to the cache list
                        _listViewCaches.Add(item);
                    }
                }
            }
            lvGeocaches.EndUpdate();
        }

        /// <summary>
        /// Recreate listview cache item
        /// </summary>
        /// <param name="cache">cache to recreate</param>
        public void RecreateVisualElements(Geocache cache)
        {
            RecreateVisualElements(cache, false);
        }

        /// <summary>
        /// Recreate listview cache item
        /// </summary>
        /// <param name="cache">cache to recreate</param>
        /// <param name="bForceEvenIfNotDisplayed">Item will be recreated even if not currently displayed on list</param>
        public void RecreateVisualElements(Geocache cache, bool bForceEvenIfNotDisplayed)
        {
            if (cache == null)
                return;

            int iNbDaysForNew = Int32.Parse(ConfigurationManager.AppSettings["daysfornew"]);
            String kmmi = (_bUseKm) ? GetTranslator().GetString("LVKm") : GetTranslator().GetString("LVMi");
            bool bMarkMissingCache = (ConfigurationManager.AppSettings["markmissingcaches"] == "True");
            int numberlastlogssymbols = Int32.Parse(ConfigurationManager.AppSettings["numberlastlogssymbols"]);

            lvGeocaches.BeginUpdate();
            
            // Replace existing element in the listview
            int iitem = GetVisualEltIndexFromCacheCode(cache._Code);
            if (iitem != -1)
            {
                // Create new item for listview
                EXImageListViewItem item = CreateVisualEltFromGeocache(iNbDaysForNew, kmmi, bMarkMissingCache, cache, numberlastlogssymbols);

                // remove old item from the list view
                _listViewCaches.Remove((EXImageListViewItem)(lvGeocaches.Items[iitem]));

                // Replace old item in listview
                lvGeocaches.Items[iitem] = item;
                
                // don't forget to add this to the cache list
            	_listViewCaches.Add(item);

                lvGeocaches.Invalidate(item.Bounds);
            }
            else
            {
                // this wasn't in the list
                if (bForceEvenIfNotDisplayed)
                {
                    // Create new item for listview
                    EXImageListViewItem item = CreateVisualEltFromGeocache(iNbDaysForNew, kmmi, bMarkMissingCache, cache, numberlastlogssymbols);

                    // remove old item from the list view
                    // On va devoir le trouver à partir de son code !!!
                    foreach (EXImageListViewItem ei in _listViewCaches)
                    {
                        String code = ei.Text;
                        if (code == cache._Code)
                        {
                            _listViewCaches.Remove(ei);
                            break;
                        }
                    }
                    
                    // don't forget to add this to the cache list
                    _listViewCaches.Add(item);
                }
            }
            lvGeocaches.EndUpdate();
        }
        
        /// <summary>
        /// Modify a cache name of selected caches based on user input
        /// </summary>
        public void ModifyCacheName()
        {
            List<Geocache> caches = null;
            String owner = ConfigurationManager.AppSettings["owner"].ToLower();
            try
            {
            	caches = GetSelectedCaches();
            	if (caches.Count == 0)
            	{
            		MsgActionWarning(this, "LblErrorNoSelection");
            		return;
            	}
            	
                List<ParameterObject> lst = new List<ParameterObject>();
                lst.Add(new ParameterObject(ParameterObject.ParameterType.String, "", "nme", GetTranslator().GetString("LblTextAppend")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "wher", GetTranslator().GetString("LblAppendFirst")));

                ParametersChanger changer = new ParametersChanger();
                changer.Title = GetTranslator().GetString("FMenuToolsModCachesName");
                changer.BtnCancel = GetTranslator().GetString("BtnCancel");
                changer.BtnOK = GetTranslator().GetString("BtnOk");
                changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
                changer.ErrorTitle = GetTranslator().GetString("Error");
                changer.Parameters = lst;
                changer.Font = this.Font;
                changer.Icon = this.Icon;

                if (changer.ShowDialog() == DialogResult.OK)
                {
                    _ThreadProgressBarTitle = GetTranslator().GetString("FMenuToolsModCachesName");
                    CreateThreadProgressBarEnh();

                    String t = lst[0].Value;
                    bool appendfirst =  (lst[1].Value == "True");
                    // Wait for the creation of the bar
                    while (_ThreadProgressBar == null)
                    {
                        Thread.Sleep(10);
                        Application.DoEvents();
                    }
                    _ThreadProgressBar.progressBar1.Maximum = caches.Count();

                    foreach (Geocache geo in caches)
                    {
                        if (appendfirst)
                            geo._Name = t + " " + geo._Name;
                        else
                            geo._Name += " " + t;

                        geo.UpdatePrivateData(owner);
                        _iNbModifiedCaches += geo.InsertModification("NAME");
                        

                        _ThreadProgressBar.Step();
                        if (_ThreadProgressBar._bAbort)
                            break;
                    }

                    PostTreatmentLoadCache();

                    // Better way to do that : only recreate for modified caches
                    RecreateVisualElements(caches);

                    KillThreadProgressBarEnh();

                    MsgActionDone(this);
                }
            }
            catch (Exception ex)
            {
                PostTreatmentLoadCache();

                // Better way to do that : only recreate for modified caches
                RecreateVisualElements(caches);

                KillThreadProgressBarEnh();

                ShowException("", GetTranslator().GetString("FMenuToolsModCachesName"), ex);
            }
        }

        /// <summary>
        /// Add all cache tags before the cache name, for selected caches 
        /// </summary>
        public void AddTagsToTitle()
        {
            List<Geocache> caches = null;
            String owner = ConfigurationManager.AppSettings["owner"].ToLower();
            try
            {
				caches = GetSelectedCaches();
				if (caches.Count == 0)
            	{
            		MsgActionWarning(this, "LblErrorNoSelection");
					return;
            	}
				
                List<ParameterObject> lst = new List<ParameterObject>();
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "wher", GetTranslator().GetString("LblAppendFirst")));

                ParametersChanger changer = new ParametersChanger();
                changer.Title = GetTranslator().GetString("FMenuToolsModCachesTag");
                changer.BtnCancel = GetTranslator().GetString("BtnCancel");
                changer.BtnOK = GetTranslator().GetString("BtnOk");
                changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
                changer.ErrorTitle = GetTranslator().GetString("Error");
                changer.Parameters = lst;
                changer.Font = this.Font;
                changer.Icon = this.Icon;

                if (changer.ShowDialog() == DialogResult.OK)
                {
                    bool appendfirst = (lst[0].Value == "True");

                    _ThreadProgressBarTitle = GetTranslator().GetString("FMenuToolsModCachesTag");
                    CreateThreadProgressBarEnh();

                    // Wait for the creation of the bar
                    while (_ThreadProgressBar == null)
                    {
                        Thread.Sleep(10);
                        Application.DoEvents();
                    }
                    _ThreadProgressBar.progressBar1.Maximum = caches.Count();

                    foreach (Geocache geo in caches)
                    {
                        OfflineCacheData ocd = geo._Ocd;
                        if (ocd != null)
                        {
                            List<String> tags = ocd._Tags;
                            if ((tags != null) && (tags.Count != 0))
                            {
                                if (appendfirst)
                                {
                                    foreach (String t in tags)
                                    {
                                        geo._Name = "[" + t + "] " + geo._Name;
                                    }
                                }
                                else
                                {
                                    foreach (String t in tags)
                                    {
                                        geo._Name += " [" + t + "]";
                                    }
                                }

                                geo.UpdatePrivateData(owner);
                                _iNbModifiedCaches += geo.InsertModification("NAME");
                            }
                        }

                        _ThreadProgressBar.Step();
                        if (_ThreadProgressBar._bAbort)
                            break;
                    }

                    PostTreatmentLoadCache();

                    // Better way to do that : only recreate for modified caches
                    RecreateVisualElements(caches);

                    KillThreadProgressBarEnh();

                    MsgActionDone(this);
                }
            }
            catch (Exception ex)
            {
                PostTreatmentLoadCache();

                // Better way to do that : only recreate for modified caches
                RecreateVisualElements(caches);

                KillThreadProgressBarEnh();

                ShowException("", GetTranslator().GetString("FMenuToolsModCachesTag"), ex);
            }
        }

        /// <summary>
        /// Create distance circles for caches on map display
        /// </summary>
        /// <param name="gmap">map control</param>
        public void CreateOverlayCircles(GMapControl gmap)
        {
            Color color1 = Color.FromArgb(60, 255, 0, 0);
            Color color2 = Color.FromArgb(60, 255, 255, 0);
            Color color3 = Color.FromArgb(60, 0, 255, 255);
            Brush brush1 = new SolidBrush(color1);
            Brush brush2 = new SolidBrush(color2);
            Brush brush3 = new SolidBrush(color3);
            Pen pen1 = new Pen(color1, 2);
            Pen pen2 = new Pen(color2, 2);
            Pen pen3 = new Pen(color3, 2);
            Pen p;
            Brush b;

            GMapOverlay overlay = gmap.Overlays[GMapWrapper.CIRCLES];
            overlay.Markers.Clear();

            gmap.HoldInvalidation = true;
            // Les caches
            List<GMapMarker> markers = gmap.Overlays[GMapWrapper.MARKERS].Markers.ToList();
            foreach(GMapMarker markergen in markers)
            {
                GMapMarkerImages markerspec = markergen as GMapMarkerImages;
                Geocache geo = null;
                if ((markerspec == null) || (markerspec.IsVisible == false))
                    continue;
                else
                    geo = markerspec.GetGeocache();

                // Le marker pour le cercle
                if (geo._Type == "Traditional Cache")
                {
                    p = pen1;
                    b = brush1;
                }
                else
                {
                    p = pen2;
                    b = brush2;
                }

                // Le cercle
                GMapMarkerCircle circle = new GMapMarkerCircle(
                    gmap,
                    new PointLatLng(geo._dLatitude, geo._dLongitude),
                    161,
                    p,
                    b,
                    true);
                overlay.Markers.Add(circle);
            }
            
            // Les bookmarks
            List<GMapMarker> markersBookmarks = gmap.Overlays[GMapWrapper.BOOKMARKS].Markers.ToList();
            foreach(GMapMarker markergen in markersBookmarks)
            {
                GMapMarkerImages markerspec = markergen as GMapMarkerImages;
                
                p = pen3;
                b = brush3;

                // Le cercle
                GMapMarkerCircle circle = new GMapMarkerCircle(
                    gmap,
                    markergen.Position,
                    161,
                    p,
                    b,
                    true);
                overlay.Markers.Add(circle);
            }
            gmap.Refresh();
        }

        /// <summary>
        /// Create difficulty / terrain overlay for caches on map display
        /// </summary>
        /// <param name="gmap">map control</param>
        public void CreateOverlayDT(GMapControl gmap)
        {
            GMapOverlay overlay = gmap.Overlays[GMapWrapper.DT]; 
            overlay.Markers.Clear();
            gmap.HoldInvalidation = true;
            List<GMapMarker> markers = _cacheDetail._gmap.Overlays[GMapWrapper.MARKERS].Markers.ToList();
            foreach (GMapMarker markergen in markers)
            {
                GMapMarkerImages markerspec = markergen as GMapMarkerImages;
                Geocache geo = null;
                if ((markerspec == null) || (markerspec.IsVisible == false))
                    continue;
                else
                    geo = markerspec.GetGeocache();

                // Le marker D/T
                Image dti = _dicoDT[geo._D + geo._T];
                GMapWrapper.gmapMarkerWithImage(
                        overlay,
                        dti,
                        geo._dLatitude,
                        geo._dLongitude,
                        "");
            }
            gmap.Refresh();
        }

        /// <summary>
        /// Create statistics overlay for caches on map display
        /// </summary>
        /// <param name="gmap">map control</param>
        public void CreateOverlayStats(GMapControl gmap)
        {
            GMapOverlay overlay = gmap.Overlays[GMapWrapper.STATS];
            overlay.Markers.Clear();
            Image fav = _listImagesSized[getIndexImages("Fav")];
            double rating;
            gmap.HoldInvalidation = true;
            List<GMapMarker> markers = _cacheDetail._gmap.Overlays[GMapWrapper.MARKERS].Markers.ToList();
            String lblfav = GetTranslator().GetString("LblMapStatsFav");
            String lblpop = GetTranslator().GetString("LblMapStatsPop");

            foreach (GMapMarker markergen in markers)
            {
                GMapMarkerImages markerspec = markergen as GMapMarkerImages;
                Geocache geo = null;
                if ((markerspec == null) || (markerspec.IsVisible == false))
                    continue;
                else
                    geo = markerspec.GetGeocache();

                OfflineCacheData ocd = geo._Ocd;
                if ((ocd != null) && (ocd.HasStats()) && (ocd._iNbFavs > 0)) // uniquement pour des favoris > 0, ça nettoye un peu la zone
                {
                    // Le marker des stats
                    rating = (_bUseGCPopularity) ? ocd._dRatingSimple : ocd._dRating;
                    int irating = (int)(rating * 100.0);
                    if (rating < 0.0)
                        irating = 0;
                    String iRating = "ratio_" + irating.ToString();
                    int index = getIndexImages(iRating);
 
                    // Le marker des stats
                    GMapMarkerGCStatsPie markerstats = new GMapMarkerGCStatsPie(
                        new PointLatLng(geo._dLatitude, geo._dLongitude),
                        _listImagesSized[index],
                        ocd._iNbFavs,
                        irating,
                        Color.White);
                    /*
                    rating = (_bUseGCPopularity) ? ocd._dRatingSimple : ocd._dRating;
                    int irating = (int)(rating * 100.0);
                    double red = (irating <= 50) ? 255 : 256 - (irating - 50) * 5.12;
                    double green = (irating >= 50) ? 255 : irating * 5.12;
                    Color color = Color.FromArgb(200, (byte)red, (byte)green, 0);
                    String texte = "";
                    
                    // Si le rating est négatif (on n'a pas d'info, on affiche juste le nb de favoris)
                    GMapMarkerText markerstats = null;
                    if (irating >= 0)
                    {
                        texte = lblfav + ocd._iNbFavs.ToString() + " - " + lblpop + irating.ToString() + "%";
                        markerstats = new GMapMarkerText(
                            new PointLatLng(geo._dLatitude, geo._dLongitude),
                            texte,
                            Color.Black,
                            color,
                            0, -20,
                            false);
                    }
                    else
                    {
                        texte = lblfav + ocd._iNbFavs.ToString();
                        markerstats = new GMapMarkerText(
                            new PointLatLng(geo._dLatitude, geo._dLongitude),
                            texte,
                            Color.Black,
                            Color.White,
                            0, -20,
                            false);
                    }
*/
                    overlay.Markers.Add(markerstats);
                }
            }
            gmap.Refresh();
        }

        /// <summary>
        /// Create custom tooltip of a waypoint
        /// </summary>
        /// <param name="geo">associated geocache</param>
        /// <param name="w">waypoint</param>
        /// <returns>tooltip string</returns>
        public String CreateWaypointTooltip(Geocache geo, Waypoint w)
        {
            String ttxt = "[" + geo._Code + " " + geo._Name + "]" + Environment.NewLine +
                        w._name + " " + w._desc;
            if (w._cmt != "")
                ttxt += Environment.NewLine + w._cmt;
            return ttxt;
        }

        /// <summary>
        /// Create waypoints on map display
        /// </summary>
        /// <param name="gmap">map control</param>
        public void CreateOverlayWpts(GMapControl gmap)
        {
            GMapOverlay overlay = gmap.Overlays[GMapWrapper.WAYPOINTS];
            overlay.Markers.Clear();
            Waypoint w;
            Double wlat, wlon;
            gmap.HoldInvalidation = true;
            List<GMapMarker> markers = _cacheDetail._gmap.Overlays[GMapWrapper.MARKERS].Markers.ToList();
            foreach (GMapMarker markergen in markers)
            {
                GMapMarkerImages markerspec = markergen as GMapMarkerImages;
                Geocache geo = null;
                if ((markerspec == null) || (markerspec.IsVisible == false))
                    continue;
                else
                    geo = markerspec.GetGeocache();

                Dictionary<String, Waypoint> dicowpts = geo.GetListOfWaypoints();
                foreach (KeyValuePair<String, Waypoint> paire in dicowpts)
                {
                    // On simplifie, ce sera un bête marqueur avec une seule image
                    w = paire.Value;
                    wlat = MyTools.ConvertToDouble(w._lat);
                    wlon = MyTools.ConvertToDouble(w._lon);

                    GMapMarkerImage marker = GMapWrapper.gmapMarkerWithImage(
                        overlay,
                        _listImagesSized[getIndexImages(w._type)],
                        wlat,
                        wlon,
                        CreateWaypointTooltip(geo,w));
                    // On ajoute la Géocache associée au marker
                    marker.Tag = geo;
                }
            }
            gmap.Refresh();
        }
        
        /// <summary>
        /// Create cache codes overlay on map display
        /// </summary>
        /// <param name="gmap">map control</param>
        /// <param name="bBig">if true, code will be bigger on screen</param>
        public void CreateOverlayCode(GMapControl gmap, bool bBig)
        {
            GMapOverlay overlay = gmap.Overlays[GMapWrapper.CODES];
            overlay.Markers.Clear();
            gmap.HoldInvalidation = true;
            List<GMapMarker> markers = _cacheDetail._gmap.Overlays[GMapWrapper.MARKERS].Markers.ToList();
            foreach (GMapMarker markergen in markers)
            {
                GMapMarkerImages markerspec = markergen as GMapMarkerImages;
                Geocache geo = null;
                if ((markerspec == null) || (markerspec.IsVisible == false))
                    continue;
                else
                    geo = markerspec.GetGeocache();

                CreateOverlayText(geo._Code, geo._dLatitude, geo._dLongitude, overlay, bBig);
            }
            gmap.Refresh();
        }

        /// <summary>
        /// Create cache name overlay on map display
        /// </summary>
        /// <param name="gmap">map control</param>
        /// <param name="bBig">if true, name will be bigger on screen</param>
        public void CreateOverlayName(GMapControl gmap, bool bBig)
        {
            GMapOverlay overlay = gmap.Overlays[GMapWrapper.NAMES]; 
            overlay.Markers.Clear();
            gmap.HoldInvalidation = true;
            List<GMapMarker> markers = _cacheDetail._gmap.Overlays[GMapWrapper.MARKERS].Markers.ToList();
            foreach (GMapMarker markergen in markers)
            {
                GMapMarkerImages markerspec = markergen as GMapMarkerImages;
                Geocache geo = null;
                if ((markerspec == null) || (markerspec.IsVisible == false))
                    continue;
                else
                    geo = markerspec.GetGeocache();

                CreateOverlayText(geo._Name, geo._dLatitude, geo._dLongitude, overlay, bBig);
            }
            gmap.Refresh();
        }

        /// <summary>
        /// Create an overlay text on map display
        /// </summary>
        /// <param name="text">text</param>
        /// <param name="lat">latitude</param>
        /// <param name="lon">longitude</param>
        /// <param name="overlay">gmap overlay</param>
        /// <param name="bBig">if true, text will be bigger on screen</param>
        public void CreateOverlayText(String text, double lat, double lon, GMapOverlay overlay, bool bBig)
        {
            GMapMarkerText marker = new GMapMarkerText(new PointLatLng(lat, lon), text, Color.Black, Color.White, 0, (bBig)?22:20, bBig);
            overlay.Markers.Add(marker);
        }

        /// <summary>
        /// Keep only caches where a found it is found for the provided user (HMI)
        /// </summary>
        public void CreateMyFindGPX()
        {
            try
            {
                List<ParameterObject> lst = new List<ParameterObject>();
                lst.Add(new ParameterObject(ParameterObject.ParameterType.String, ConfigurationManager.AppSettings["owner"], "userlogin", GetTranslator().GetString("LblUserName")));

                ParametersChanger changer = new ParametersChanger();
                changer.Title = GetTranslator().GetString("FMenuMyFinds");
                changer.BtnCancel = GetTranslator().GetString("BtnCancel");
                changer.BtnOK = GetTranslator().GetString("BtnOk");
                changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
                changer.ErrorTitle = GetTranslator().GetString("Error");
                changer.Parameters = lst;
                changer.Font = this.Font;
                changer.Icon = this.Icon;

                if (changer.ShowDialog() == DialogResult.OK)
                {
                    String user = lst[0].Value.ToLower();
                    Log("Looking for logs from: " + user);
                    List<Geocache> caches = GetDisplayedCaches();
                    Log("Looking in " + caches.Count.ToString() + " cache(s)");

                    // Cleanup
                    _caches = new Dictionary<string, Geocache>();
                    _listViewCaches = new List<EXListViewItem>();
                    lvGeocaches.Items.Clear();
                    EmptywbFastCachePreview();

                    foreach (Geocache geo in caches)
                    {
                        // On va parcourir les logs à la recherche d'un "Found it" pour l'utilisateur demandé
                        CacheLog lg = null;
                        if (geo._Logs != null)
                        {
                            foreach (CacheLog log in geo._Logs)
                            {
                                // bon utilisateur et "Found it"
                                if ((lg == null) && (log._User.ToLower() == user) && (log._Type == "Found it"))
                                {
                                    lg = log;
                                    //Log(geo._Code + ": Found a log");
                                }
                            }
                        }
                        if (lg != null)
                        {
                            // On conserve cette cache
                            geo._Logs.Clear();
                            geo._Logs.Add(lg);
                            _caches.Add(geo._Code, geo);
                        }
                    }
                    
                    // Now we join Wpts & caches
                    PostTreatmentLoadCache();

                    BuildListViewCache();
                    _bUseFilter = false; // DON'T FORGET TO RESET THE FILTER !
                    PopulateListViewCache(null);
                    
                }

            }
            catch (Exception ex)
            {
                ShowException("", GetTranslator().GetString("FMenuMyFinds"), ex);
            }
        }

        /// <summary>
        /// Assign and translate all tooltips associated to a Control,
        /// based on tooltips files (.tips) found in MGM
        /// </summary>
        /// <param name="ctl">Control to which assign tooltips (Control, MenuItem, ToolstripMenuItem)</param>
        /// <param name="tip">tooltip. If null a new one will be created</param>
        public void TranslateTooltips(Object ctl, ToolTip tip)
        {
            if (ConfigurationManager.AppSettings["enabletooltips"] == "True")
            {
                if (tip == null)
                {
                    tip = new ToolTip();
                    // Set up the delays for the ToolTip.
                    tip.AutoPopDelay = 3000;
                    tip.InitialDelay = 1000;
                    tip.ReshowDelay = 500;
                    // Force the ToolTip text to be displayed whether or not the form is active.
                    tip.ShowAlways = true;
                }
                
                TranslationManager tipsmgr = GetTooltipsTranslator();
                // On va lister tous les controls du control et leur chercher un tooltip
                List<Object> os = new List<object>();
            	MyTools.ListControls(ctl, ref os);
            	String txt = null;
            	foreach(Object o in os)
            	{
            		// Recursively search the parent's children.
            		{
			            ToolStripItem tsparent = o as ToolStripItem;
			            if (tsparent != null)
			            {
			            	String name = tsparent.Name;
			            	//Log("ToolStripItem: " + name);
			            	txt = tipsmgr.GetStringMV(name);
			            	if (!String.IsNullOrEmpty(txt))
			            	{
			            		//Log(" ===> " + txt);
			            		tsparent.ToolTipText = txt;
		                        // Et on ajoute une image si trouvée ?
	            
	    	                    if ((_imgMenus != null) && (_imgMenus.ContainsKey(name)))
	    	                		tsparent.Image = _imgMenus[name];
			            	}
			            }
			            else
			            {
			            	TabPage tp = o as TabPage;
			            	if (tp != null)
			            	{
			            		//Log("TabPage: " + tp.Name);
			            		txt = tipsmgr.GetStringMV(tp.Name);
			            		if (!String.IsNullOrEmpty(txt))
			            		{
			            			//Log(" ===> " + txt);
			            			tp.ToolTipText = txt;
			            		}
			            	}
			            	else
			            	{
				            	Control ctrl = o as Control;
				            	if (ctrl != null)
				            	{
				            		//Log("Control: " + ctrl.Name);
				            		txt = tipsmgr.GetStringMV(ctrl.Name);
			            			if (!String.IsNullOrEmpty(txt))
			            			{
			            				//Log(" ===> " + txt);
				            			tip.SetToolTip(ctrl, txt);
			            			}
				            	}
			            	}
			            }
            		}
            	}
            }
        }
		
        /// <summary>
        /// Look for an unused waypoint code
        /// </summary>
        /// <param name="geo">Attached geocache</param>
        /// <param name="sCacheSuffix">Cache code without leading GC</param>
        /// <param name="sWptPrefix">Propose waypoint prefix</param>
        /// <param name="bIncludeWptsFromMGM">If true, _waypoints will be included in forbiden list</param>
        /// <returns>Valid waypoint code</returns>
        private String LookForAWptCode(Geocache geo, String sCacheSuffix, String sWptPrefix, bool bIncludeWptsFromMGM)
        {
            String sWptCode = sWptPrefix + sCacheSuffix;
            List<String> forbidencode = new List<string>();
            // Des valeurs interdites par construction
            forbidencode.Add("GC" + sCacheSuffix);
            forbidencode.Add("TB" + sCacheSuffix);
            forbidencode.Add("WP" + sCacheSuffix);
            forbidencode.Add("WM" + sCacheSuffix);
            if (bIncludeWptsFromMGM)
            {
                foreach (KeyValuePair<String, Waypoint> pair in _waypointsLoaded)
                {
                    // On ajoute le code du waypoint déjà existant
                    Log("_waypointsLoaded Forbiden code: " + pair.Value._name);
                    forbidencode.Add(pair.Value._name);
                }
                foreach (KeyValuePair<String, Waypoint> pair in _waypointsMGM)
                {
                    // On ajoute le code du waypoint déjà existant
                    Log("_waypointsMGM Forbiden code: " + pair.Value._name);
                    if (forbidencode.Contains(pair.Value._name) == false)
                        forbidencode.Add(pair.Value._name);
                }
            }
            Dictionary<String, Waypoint> dicowpts = geo.GetListOfWaypoints();
            foreach (KeyValuePair<String, Waypoint> pair in dicowpts)
            {
                // On ajoute le code du waypoint déjà existant
                Log("geo Forbiden code: " + pair.Value._name); 
                if (forbidencode.Contains(pair.Value._name) == false)
					forbidencode.Add(pair.Value._name);
            }
            
            // Est-ce que le code est interdit ?
            // On utilise cet alphabet :
            List<char> alphabet = new List<char>(new char[] { 
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K',
            'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T' ,'U', 'V',
            'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5', '6',
            '7', '8', '9'
            });

            bool bSecondRound = false;
            while (forbidencode.Contains(sWptCode))
            {
                Log("Bad sWptCode: " + sWptCode);
                // On va incrémenter la seconde lettre
                String sCurrentWptPrefix = sWptCode.Substring(0, 2);

                if ((bSecondRound) && (sCurrentWptPrefix == sWptPrefix)) // On est revenu à la première possibilité
                {
                    // On n'a plus aucune possibilité !
                    return "";
                }
                else if (sCurrentWptPrefix == "99") // Dernier préfixe possible
                {
                    // On démarre en AA, premier préfixe possible
                    bSecondRound = true;
                    sWptCode = "AA" + sCacheSuffix;
                }
                else if (alphabet.IndexOf(sCurrentWptPrefix[1]) == (alphabet.Count -1)) // on est arrivé en bout pour la seconde lettre du prefiex
                {
                    // On va incrémenter la lettre de devant s'il reste une possibilité
                    // Normalement la dernière possibilité a été catchée avant
                    char c = alphabet[alphabet.IndexOf(sCurrentWptPrefix[0]) + 1];
                    sCurrentWptPrefix = new String(c, 1) + new String(alphabet[0], 1);
                    sWptCode = sCurrentWptPrefix + sCacheSuffix;
                }
                else
                {
                    // On va incrémenter la lettre de derrière
                    char c = alphabet[alphabet.IndexOf(sCurrentWptPrefix[1]) + 1];
                    sCurrentWptPrefix = new String(sCurrentWptPrefix[0], 1) + new String(c, 1);
                    sWptCode = sCurrentWptPrefix + sCacheSuffix;
                }
                Log(" New code to test: " + sWptCode);
            }

            // On a trouvé un candidat
            return sWptCode;
        }

        /// <summary>
        /// Remove an existing waypoint
        /// </summary>
        /// <param name="wpt">Waypoint to remove</param>
        /// <returns>True if modification performed</returns>
        public bool RemoveWaypoint(Waypoint wpt)
        {
            try
            {
                // On supprimer le waypoint de la liste _waypointsMGM,
                // Et si applicable de la cache associée, avec un éventuel rollout de l'ancien waypoint
                // Puis on rafraichis les cartes
                // Et on sauve la liste de waypoints
                
                // La cache
                Geocache cache = _caches[wpt._GCparent];
                Log("--- RemoveWaypoint --- " + wpt._name);
                if (cache._waypointsFromMGM.ContainsKey(wpt._name))
                {
                	// C'était un waypoint modifié (éventuellement surchargeant un waypoint original)
                	// La cache perd cette modification
                	Log("deleted from cache._waypointsFromMGM");
                    // On le vire de la liste des waypoints MGM
                    _iNbModifiedCaches -= cache.RemoveModification("WPT_" + wpt._name);

                    // Et on lui vire le waypoint
                    cache._waypointsFromMGM.Remove(wpt._name);
                }
                else if (cache._waypoints.ContainsKey(wpt._name))
                {
                	Log("deleted from cache._waypoints");
                    // C'était un waypoint original
                    // La cache est maintenant modifiée !
                    _iNbModifiedCaches += cache.InsertModification("WPT_" + wpt._name);

                    // Et on lui vire le waypoint
                    cache._waypoints.Remove(wpt._name);
                }

                // on recrée cette cache dans MGM
                List<Geocache> caches = new List<Geocache>();
                caches.Add(cache);
                RecreateVisualElements(caches);

                // La liste MGM
                if (_waypointsMGM.ContainsKey(wpt._name))
                {
                	Log("deleted from this._waypointsMGM");
                    _waypointsMGM.Remove(wpt._name);
                }

                // On reconstruit ce maudit calque !
                List<Geocache> theCaches = GetDisplayedCaches();
                ReBuildCacheMapPreview(theCaches);

                // On reconstruit ce maudit calque !
                CreateOverlayWpts(_cacheDetail._gmap);

                // On le sauve dans MGM
                SaveWaypointsMGM();

                return true;
            }
            catch (Exception ex)
            {
                _cacheDetail._gmap.ControlTextLatLon = null;
                ShowException("", GetTranslator().GetString("WaypointMgrDel"), ex);
                return false;
            }
        }

        /// <summary>
        /// Edit an existing waypoint
        /// </summary>
        /// <param name="w">Waypoint to edit</param>
        /// <returns>Edited waypoint if modified, null if unchanged</returns>
        public Waypoint EditWaypoint(Waypoint w)
        {
            try
            {
                Log("--- EditWaypoint ---");           	
                // on ferme la carto
                CloseCacheDetail();

                // Est-ce un waypoint custom ou un pur issu d'un GPX ?
                // Si pur, il faut absolument le cloner pour stocker son clone édité dans la liste cache._waypointsfromMGM
                Geocache cache = _caches[w._GCparent];
                Waypoint wpt = w;
                Waypoint.WaypointOrigin wOrigin = cache.GetWaypointOrigin(w);
                if (wOrigin == Waypoint.WaypointOrigin.GPX)
                {
                	// On clone !
                	Log("cloning");
                	wpt = w.Clone();
                }
               	// sinon on laisse son type
               	
                List<ParameterObject> lst = new List<ParameterObject>();
                // 0 : Le type
                List<String> lsttype = new List<string>();
                _geocachingConstants.CreateListOfWaypointTypes(lsttype);
                ParameterObject pobj = new ParameterObject(ParameterObject.ParameterType.List, lsttype, "type", GetTranslator().GetString("WaypointType"));
                pobj.DefaultListValue = GetTranslator().GetString("WptType" + wpt._type.Replace(" ", ""));
                lst.Add(pobj);

                // 1 : Le nom
                lst.Add(new ParameterObject(ParameterObject.ParameterType.String, wpt._desc, "name", GetTranslator().GetString("WaypointName"), null, new List<object>(new string[] { "" })));

                // 2 : Coordonnées
                // Par défaut celles de la cache
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Coordinates/*good*/, wpt._lat.ToString().Replace(",", ".") + " " + wpt._lon.ToString().Replace(",", "."), "latlon", GetTranslator().GetString("WaypointCoord"), GetTranslator().GetStringM("TooltipParamLatLon")));

                // 3 : La description
                lst.Add(new ParameterObject(ParameterObject.ParameterType.TextBox, wpt._cmt, "desc", GetTranslator().GetString("WaypointDesc")));


                ParametersChanger changer = new ParametersChanger();
                changer.HandlerDisplayCoord = HandlerToDisplayCoordinates;
                changer.DisplayCoordImage = _listImagesSized[getIndexImages("Earth")];
                changer.Title = GetTranslator().GetString("WaypointMgrEdit");
                changer.BtnCancel = GetTranslator().GetString("BtnCancel");
                changer.BtnOK = GetTranslator().GetString("BtnOk");
                changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
                changer.ErrorTitle = GetTranslator().GetString("Error");
                changer.Parameters = lst;
                changer.Font = this.Font;
                changer.Icon = this.Icon;

                // Force creation du get handler on control
                changer.CreateControls();
                _cacheDetail._gmap.ControlTextLatLon = changer.CtrlCallbackCoordinates;

                if (changer.ShowDialog() == DialogResult.OK)
                {
                    _cacheDetail._gmap.ControlTextLatLon = null;

                    // Le type
                    String sType = lst[0].Value;
                    String sWptType = "";
                    String sWptTypePrefix = "";
                     _geocachingConstants.RetrieveWaypointTypeAndPrefix(sType, ref sWptType, ref sWptTypePrefix);

                    // On garde le code du waypoint existant !!!
                    
                    String sWptCode = wpt._name;
                    
                    // Le nom
                    String sName = lst[1].Value;

                    // Les coordonnées
                    Double dlon = Double.MaxValue;
                    Double dlat = Double.MaxValue;
                    ParameterObject.SplitLongitudeLatitude(lst[2].Value, ref dlon, ref dlat);

                    // La description
                    String sDesc = lst[3].Value;

                    // On va créer le waypoint
                    wpt._DateExport = DateTime.Now;
                    // Pas de modification de l'origine c'est fait plus haut
                    wpt._name = sWptCode;
                    wpt._lat = dlat.ToString().Replace(",", ".");
                    wpt._lon = dlon.ToString().Replace(",", ".");
                    wpt._time = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
                    wpt._cmt = sDesc;
                    wpt._desc = sName;
                    wpt._urlname = wpt._desc;
                    wpt._type = sWptType;
                    wpt._sym = sWptType;
                    

                    _iNbModifiedCaches += cache.InsertModification("WPT_" + wpt._name);
                    if (cache._Modifications.Count == 1)
                    {
                        // on recrée cette cache dans MGM
                        List<Geocache> caches = new List<Geocache>();
                        caches.Add(cache);
                        RecreateVisualElements(caches);
                    }

                    // Post treatment
                    wpt.PostTreatmentData(_tableWptsTypeTranslated);

                    // On le re-crée dans la map preview
                    // On le cherche
                    GMapMarker mkTrouve = null;
                    foreach (GMapMarker mk in _cachesPreviewMap.Overlays[GMapWrapper.WAYPOINTS].Markers)
                    {
                        if (mk.Tag == cache)
                        {
                            // on l'a trouvé, on le modifie !
                            mkTrouve = mk;
                            break;
                        }
                    }
                    if (mkTrouve != null)
                    {
                        // On reconstruit ce maudit calque !
                        List<Geocache> theCaches = GetDisplayedCaches();
                        ReBuildCacheMapPreview(theCaches);
                    }

                    // on le modifie dans la map globale si le calque waypoint est visible
                    mkTrouve = null;
                    foreach (GMapMarker mk in _cacheDetail._gmap.Overlays[GMapWrapper.WAYPOINTS].Markers)
                    {
                        if (mk.Tag == cache)
                        {
                            // on l'a trouvé, on le modifie !
                            mkTrouve = mk;
                            break;
                        }
                    }
                    if (mkTrouve != null)
                    {
                        // On reconstruit ce maudit calque !
                        CreateOverlayWpts(_cacheDetail._gmap);
                    }
                    

                    // On le sauve dans MGM
                    // On a pu éditer un waypoint officiel d'un GPX !
                    if (cache._waypointsFromMGM.ContainsKey(wpt._name))
                    {
                        // C'était déjà un waypoint MGM, il a forcément le bon type
                    }
                    else
                    {
                        // On a édité un waypoint d'origine
                        // On l'ajoute à la cache
                        wpt._eOrigin = Waypoint.WaypointOrigin.MODIFIED;
                        cache._waypointsFromMGM.Add(wpt._name, wpt);
                    }
                    
                    // On l'ajoute à MGM si besoin (y'a pas de raison qu'il n'y soit pas)
                    if (_waypointsMGM.ContainsKey(wpt._name) == false)
                        _waypointsMGM.Add(wpt._name, wpt); // Il faut l'ajouter !

                    SaveWaypointsMGM();

                    return wpt;
                }
                else
                {
                    _cacheDetail._gmap.ControlTextLatLon = null;
                    return null;
                }
            }
            catch (Exception ex)
            {
                _cacheDetail._gmap.ControlTextLatLon = null;
                ShowException("", GetTranslator().GetString("WaypointMgrEdit"), ex);
                return null;
            }
        }

        /// <summary>
        /// Add a waypoint to a cache
        /// </summary>
        /// <param name="cache">Cache to hold waypoint</param>
        /// <returns>Created waypoint if success</returns>
        public Waypoint AddWaypointToCache(Geocache cache)
        {
            try
            {
                Log("--- AddWaypointToCache ---");
                // on ferme la carto
                CloseCacheDetail();

                List<ParameterObject> lst = new List<ParameterObject>();
                // 0 : Le type
                List<String> lsttype = new List<string>();
                 _geocachingConstants.CreateListOfWaypointTypes(lsttype);
                lst.Add(new ParameterObject(ParameterObject.ParameterType.List, lsttype, "type", GetTranslator().GetString("WaypointType")));

                // 1 : Le nom
                lst.Add(new ParameterObject(ParameterObject.ParameterType.String, "", "name", GetTranslator().GetString("WaypointName"), null, new List<object>(new string[] { "" })));

                // 2 : Coordonnées
                // Par défaut celles de la cache
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Coordinates/*good*/, cache._Latitude + " " + cache._Longitude, "latlon", GetTranslator().GetString("WaypointCoord"), GetTranslator().GetStringM("TooltipParamLatLon")));

                // 3 : La description
                lst.Add(new ParameterObject(ParameterObject.ParameterType.TextBox, "", "desc", GetTranslator().GetString("WaypointDesc")));


                ParametersChanger changer = new ParametersChanger();
                changer.HandlerDisplayCoord = HandlerToDisplayCoordinates;
                changer.DisplayCoordImage = _listImagesSized[getIndexImages("Earth")];
                changer.Title = GetTranslator().GetString("WaypointCreation");
                changer.BtnCancel = GetTranslator().GetString("BtnCancel");
                changer.BtnOK = GetTranslator().GetString("BtnOk");
                changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
                changer.ErrorTitle = GetTranslator().GetString("Error");
                changer.Parameters = lst;
                changer.Font = this.Font;
                changer.Icon = this.Icon;

                // Force creation du get handler on control
                changer.CreateControls();
                _cacheDetail._gmap.ControlTextLatLon = changer.CtrlCallbackCoordinates;

                if (changer.ShowDialog() == DialogResult.OK)
                {
                    _cacheDetail._gmap.ControlTextLatLon = null;

                    // Le type
                    String sType = lst[0].Value;
                    String sWptType = "";
                    String sWptTypePrefix = "";
                     _geocachingConstants.RetrieveWaypointTypeAndPrefix(sType, ref sWptType, ref sWptTypePrefix);

                    // On cherche un code de waypoint libre
                    String cache_suffix = cache._Code.Substring(2);
                    String sWptCode = LookForAWptCode(cache, cache_suffix, sWptTypePrefix, false);
                    // On vérifie tout de même que par le plus grand des hasards il n'est pas déjà existant dans la liste des waypoints de MGM
                    // Il n'y a pas de raison !
                    if (_waypointsLoaded.ContainsKey(sWptCode) || _waypointsMGM.ContainsKey(sWptCode))
                    {
                        // WTF ?!
                        // On relance une recherche bourrine
                        sWptCode = LookForAWptCode(cache, cache_suffix, sWptTypePrefix, true);
                    }

                    if (sWptCode == "")
                    {
                        MsgActionError(this, GetTranslator().GetString("WptNoName"));
                        return null;
                    }
                    

                    // Le nom
                    String sName = lst[1].Value;

                    // Les coordonnées
                    Double dlon = Double.MaxValue;
                    Double dlat = Double.MaxValue;
                    ParameterObject.SplitLongitudeLatitude(lst[2].Value, ref dlon, ref dlat);

                    // La description
                    String sDesc = lst[3].Value;

                    // On va créer le waypoint et on l'ajoute à _waypoints
                    Waypoint wpt = new Waypoint();
                    wpt._eOrigin = Waypoint.WaypointOrigin.CUSTOM;
                    wpt._DateExport = DateTime.Now;
                    wpt._name = sWptCode;
                    _waypointsMGM.Add(wpt._name, wpt);
                    wpt._lat = dlat.ToString().Replace(",", ".");
                    wpt._lon = dlon.ToString().Replace(",", ".");
                    wpt._time = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
                    wpt._cmt = sDesc;
                    wpt._desc = sName;
                    wpt._url = cache._Url;
                    wpt._urlname = wpt._desc;
                    wpt._type = sWptType;
                    wpt._sym = sWptType;
                    // Post treatment
                    wpt.PostTreatmentData(_tableWptsTypeTranslated);

                    // On le crée dans la map preview
                    GMapMarker mwpt = GMapWrapper.gmapMarkerWithImage(
                        _cachesPreviewMap.Overlays[GMapWrapper.WAYPOINTS],
                        _listImagesSized[getIndexImages(wpt._type)],
                        dlat,
                        dlon,
                        CreateWaypointTooltip(cache, wpt));
                    mwpt.Tag = cache;
                    _cachesPreviewMap.Refresh();
                    cachesPreviewMap_OnMapZoomChanged();

                    // on le crée dans la map globale si le calque waypoint est visible
                    if (_cacheDetail._gmap.Overlays[GMapWrapper.WAYPOINTS].IsVisibile)
                    {
                        mwpt = GMapWrapper.gmapMarkerWithImage(
                            _cacheDetail._gmap.Overlays[GMapWrapper.WAYPOINTS],
                            _listImagesSized[getIndexImages(wpt._type)],
                            dlat,
                            dlon,
                            CreateWaypointTooltip(cache, wpt));
                        mwpt.Tag = cache;
                    }
                    _cacheDetail._gmap.Refresh();
                    _cacheDetail.gmap_OnMapZoomChanged();

                    // on l'ajoute à la cache
                    cache._waypointsFromMGM.Add(wpt._name, wpt);
                    _iNbModifiedCaches += cache.InsertModification("WPT_" + wpt._name);
                    if (cache._Modifications.Count == 1)
                    {
                        // on recrée cette cache dans MGM
                        List<Geocache> caches = new List<Geocache>();
                        caches.Add(cache);
                        RecreateVisualElements(caches);
                    }
                                        
                    // On le sauve dans MGM
                    SaveWaypointsMGM();

                    return wpt;
                }
                else
                {
                    _cacheDetail._gmap.ControlTextLatLon = null;
                    return null;
                }
            }
            catch (Exception ex)
            {
                _cacheDetail._gmap.ControlTextLatLon = null;
                ShowException("", GetTranslator().GetString("WaypointCreation"), ex);
                return null;
            }
        }

        

        private void SaveWaypointsMGM()
        {
            try
            {
                // on sauve les waypoints
                String fileRadix = GetUserDataPath() + Path.DirectorySeparatorChar + "WaypointsMGM.dat";
                System.IO.StreamWriter file = new System.IO.StreamWriter(fileRadix, false);

                file.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                file.WriteLine("<gpx xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:ox=\"http://www.opencaching.com/xmlschemas/opencaching/1/0\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" version=\"1.0\" creator=\"Groundspeak, Inc. All Rights Reserved. http://www.groundspeak.com\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/0 http://www.topografix.com/GPX/1/0/gpx.xsd http://www.groundspeak.com/cache/1/0/1 http://www.groundspeak.com/cache/1/0/1/cache.xsd\" xmlns=\"http://www.topografix.com/GPX/1/0\">");
                file.WriteLine("  <name>Cache Listing Generated from Geocaching.com</name>");
                file.WriteLine("  <desc>This is an individual cache generated from Geocaching.com</desc>");
                file.WriteLine("  <author>Account \"" + ConfigurationManager.AppSettings["owner"] + "\" From Geocaching.com</author>");
                file.WriteLine("  <email>contact@geocaching.com</email>");
                file.WriteLine("  <url>http://www.geocaching.com</url>");
                file.WriteLine("  <urlname>Geocaching - My Geocaching Manager Export " + DateTime.Now.ToLongDateString() + "</urlname>");
                String date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
                file.WriteLine("  <time>" + date + "</time>");
                file.WriteLine("  <keywords>cache, geocache</keywords>");
                String chunk = "";
                String wptstring = "";
                foreach (KeyValuePair<String, Waypoint> paire in _waypointsMGM)
                {
                    wptstring = paire.Value.ToGPXChunk();
                    chunk += wptstring;
                }

                file.Write(chunk);
                file.WriteLine("</gpx>");
                file.Close();
            }
            catch (Exception ex)
            {
                Log(GetException("Saving waypoints MGM", ex));
            }
        }
        

        private String GetCacheModificationText(Geocache geo)
        {
        	String m = "";
        	if (geo.HasBeenModified)
        	{
        		List<String> wpts = new List<string>();
        		foreach(String key in geo._Modifications)
        		{
        			// On ne traite pas les waypoints pour l'instant
        			if (key.StartsWith("WPT_"))
    			    {
        				wpts.Add(key.Substring(4));
    			    }
    			    else
    			    {
    			    	m += "* " + GetTranslator().GetString("CacheModif" + key).Replace("#","\r\n") + "\r\n";
    			    }
        		}
        	
        		// Les waypoints
        		if (wpts.Count != 0)
        		{
        			m += "* " + GetTranslator().GetString("CacheModifWpt_").Replace("#","\r\n");
        		}
        		foreach(String w in wpts)
        		{
        			m +="   " + w + "\r\n";
        		}
        	}
        	return m;
        }
        
        private String DisplayModificationsImpl(List<Geocache> cs, ref int inbmodif)
        {
        	String msg = "";
        	inbmodif = 0;
            if (cs == null)
            {
            	inbmodif = _iNbModifiedCaches;
	            foreach(KeyValuePair<String, Geocache> paire in _caches)
	            {
	            	Geocache geo = paire.Value;
	            	String m = GetCacheModificationText(geo);
	            	if (m != "")
            			msg += geo._Code + " " + geo._Name + "\r\n" + m;
	            }
            }
	        else
	        {
	        	foreach(Geocache geo in cs)
	            {
	            	String m = GetCacheModificationText(geo);
	            	if (m != "")
	            	{
            			msg += geo._Code + " " + geo._Name + "\r\n" + m;
            			inbmodif++;
	            	}
	            }
	        }
        
	        return msg;
        }
        
        private void DisplayModifications(List<Geocache> cs)
        {
        	int inbmodif = 0;
        	String msg = DisplayModificationsImpl(cs, ref inbmodif);
        	if (msg != "")
            {
            	msg = inbmodif.ToString() + GetTranslator().GetString("LblModifiedCaches") + "\r\n" + msg;
            	msg = GetTranslator().GetString("CacheModifGeneral").Replace("#","\r\n") + "\r\n\r\n" + msg;
        	}
        	else
        		msg = GetTranslator().GetString("LblNoModification");
        	MsgActionOk(this, msg, false);        	
        }
        
        private void SetDisplayModifications(object sender, EventArgs e)
        {
            List<Geocache> caches = GetSelectedCaches();
            
            DisplayModifications(caches);
        }
        
        /// <summary>
        /// nicely show an exception
        /// </summary>
        /// <param name="comment">optionnal comment (can be "")</param>
        /// <param name="ex">exception to display</param>
        /// <returns>formatted exception</returns>
        static public String GetException(String comment, Exception ex)
        {
        	String msg = "";
        	
        	// Le commentaire utilisateur
        	if (!String.IsNullOrEmpty(comment))
        		msg += "Comments: " + comment + "\r\n\r\n";
        	
        	// L'exception
        	String s = "Exception: " + ex.Message;
            if (ex.InnerException != null)
                s += "\r\n    " + ex.InnerException.Message;
            if (String.IsNullOrEmpty(ex.StackTrace) == false)
                s += "\r\n    " + ex.StackTrace;
            msg += s + "\r\n\r\n";
            
            // La stacktrace
            msg += "Stack:\r\n" + MyTools.StackTraceToString(1, "   ");
            
            return msg;
        }
        
        /// <summary>
        /// nicely show an exception
        /// </summary>
        /// <param name="title">windows title</param>
        /// <param name="comment">optionnal comment (can be "")</param>
        /// <param name="ex">exception to display</param>
        public void ShowException(String title, String comment, Exception ex)
        {
        	String msg = "";
        	if (title == "")
        		title = GetTranslator().GetString("ErrTitle");
        	
        	// Le commentaire utilisateur
        	if (!String.IsNullOrEmpty(comment))
        		msg += GetTranslator().GetString("lblcomments") + ": " + comment + "\r\n\r\n";
        	
        	// L'exception
        	String s = GetTranslator().GetString("LblException") + ": " + ex.Message;
            if (ex.InnerException != null)
                s += "\r\n    " + ex.InnerException.Message;
            if (String.IsNullOrEmpty(ex.StackTrace) == false)
                s += "\r\n    " + ex.StackTrace;
            msg += s + "\r\n\r\n";
            
            
            // La stacktrace
            msg += GetTranslator().GetString("LblStackTrace") + ":\r\n" + MyTools.StackTraceToString(1, "   ");
            
            Log("!! EXCEPTION !!" + title);
            Log(msg);
        	MsgActionError(this, msg, false);
        
        }

        private void ShowNotification(String title, String comment)
        {
            try
            {
                PopupNotifier popupNotifier1 = new PopupNotifier();
                popupNotifier1.BodyColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
                popupNotifier1.ContentFont = this.Font;// new System.Drawing.Font("Tahoma", 8F);
                popupNotifier1.GradientPower = 300;
                popupNotifier1.HeaderHeight = 20;
                popupNotifier1.Image = this.Icon.ToBitmap();
                popupNotifier1.OptionsMenu = null;
                popupNotifier1.Size = new System.Drawing.Size(400, 100);
                popupNotifier1.TitleFont = this.Font;// new System.Drawing.Font("Segoe UI", 9F);

                popupNotifier1.TitleText = title;
                popupNotifier1.ContentText = comment;
                popupNotifier1.ShowCloseButton = true;
                popupNotifier1.ShowOptionsButton = false;
                popupNotifier1.ImgGrip = GetImageSized("Grip");
                popupNotifier1.ShowGrip = true;
                popupNotifier1.Delay = 5000;
                popupNotifier1.AnimationInterval = 10;
                popupNotifier1.AnimationDuration = 1000;
                popupNotifier1.TitlePadding = new Padding(0);
                popupNotifier1.ContentPadding = new Padding(0);
                popupNotifier1.ImagePadding = new Padding(0);
                popupNotifier1.Scroll = true;

                popupNotifier1.Popup();
            }
            catch (Exception ex)
            {
                Log(GetException("!! Error showing update notification window ", ex));
            }
        }

        private String DecryptCookie(String cookie)
        {
        	String res = "";
            StringCipher.CustomDecryptNoPadding(cookie, ref res);
            int i = res.IndexOf('|');
            if (i != -1)
            {
            	res = res.Substring(i+1);
            }
            return res;
        }
        
        private void AskForCookieDecryption()
        {
        	List<ParameterObject> lst = new List<ParameterObject>();
        	lst.Add(new ParameterObject(ParameterObject.ParameterType.String, "", "cookie", "Cookie"));
            
            ParametersChanger changer = new ParametersChanger();
            changer.Title = "Cookie ?";
            changer.BtnCancel = GetTranslator().GetString("BtnCancel");
            changer.BtnOK = GetTranslator().GetString("BtnOk");
            changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
            changer.ErrorTitle = GetTranslator().GetString("Error");
            changer.Parameters = lst;
            changer.Font = this.Font;
            changer.Icon = this.Icon;

            if (changer.ShowDialog() == DialogResult.OK)
            {
                String cookie = lst[0].Value;
                
                MSG(DecryptCookie(cookie));
            }
        }

        /// <summary>
        /// Permet de zouiller les attributs d'une cache
        /// </summary>
        /// <param name="wpt">id de la cache</param>
        /// <param name="ids">id des attributs</param>
        /// <param name="yesnona">valeurs des attributs (yes, no, na)</param>
        public void ZouilleurdAttributsImpl(String wpt, String[] ids, String[] yesnona)
        {
            try
            {
                    UpdateHttpDefaultWebProxy();
                    String post_response = "";
                    // On checke que les L/MDP soient corrects
                    // Et on récupère les cookies au passage
                    CookieContainer cookieJar = CheckGCAccount(true, false);
                    if (cookieJar == null)
                        return;

                    // L'url pour modifier les attributs
                    String url = "https://www.geocaching.com/hide/attributes.aspx?WptID=" + wpt;
                    HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
                    objRequest.Proxy = GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
                    objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
                    HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
                    using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
                    {
                        post_response = responseStream.ReadToEnd();
                        responseStream.Close();
                    }

                    // On récupère les viewstates
                    String __VIEWSTATEFIELDCOUNT = "";
                    String[] __VIEWSTATE = null;
                    String __VIEWSTATEGENERATOR = "";
                    GetViewState(post_response, ref __VIEWSTATEFIELDCOUNT, ref __VIEWSTATE, ref __VIEWSTATEGENERATOR);

                    // Préparation des données du POST
                    Dictionary<String, String> post_values = new Dictionary<String, String>();
                    post_values.Add("__EVENTTARGET", "");
                    post_values.Add("__EVENTARGUMENT", "");
                    for(int i=0;i<ids.Length; i++)
                    {
                        if (ids[i] != "")
                            post_values.Add("ctl00$ContentBody$Attributes$" + ids[i], ids[i] + "-" + yesnona[i]);
                    }
                    

                    // tous les attributs impossibles hi hi hi
                    //int[] ids = new int[] { 19, 20, 21, 22, 23, 39, 18, 43, 11, 12, 26, 2, 3, 4, 5, 44, 48, 49, 50, 51, 60, 64 };
                    //foreach(int i in ids)
                    //{
                    //	post_values.Add("ctl00$ContentBody$Attributes$" + i.ToString(), i.ToString() + "-no");
                    //}

                    post_values.Add("ctl00$ContentBody$Attributes$btnUpdate", "Update Attributes");


                    // Les viewstate
                    post_values.Add("__VIEWSTATE", __VIEWSTATE[0]);
                    if (__VIEWSTATE.Length > 1)
                    {
                        for (int i = 1; i < __VIEWSTATE.Length; i++)
                        {
                            post_values.Add("__VIEWSTATE" + i.ToString(), __VIEWSTATE[i]);
                        }
                        post_values.Add("__VIEWSTATEFIELDCOUNT", __VIEWSTATE.Length.ToString());
                    }
                    if (__VIEWSTATEGENERATOR != "")
                        post_values.Add("__VIEWSTATEGENERATOR", __VIEWSTATEGENERATOR);

                    // Encodage des données du POST
                    String post_string = "";
                    foreach (KeyValuePair<String, String> post_value in post_values)
                    {
                        post_string += post_value.Key + "=" + HttpUtility.UrlEncode(post_value.Value) + "&";
                    }
                    post_string = post_string.TrimEnd('&');

                    // Création de la requête pour s'authentifier
                    objRequest = (HttpWebRequest)WebRequest.Create(url);
                    objRequest.Method = "POST";
                    objRequest.ContentLength = post_string.Length;
                    objRequest.ContentType = "application/x-www-form-urlencoded";
                    objRequest.Proxy = GetProxy(); // Créer votre proxy ici si besoin, sinon mettre NULL
                    objRequest.CookieContainer = cookieJar;
                    //objRequest.KeepAlive = false; // PATCH SARCE ?

                    // on envoit les POST data dans un stream (écriture)
                    StreamWriter myWriter = null;
                    myWriter = new StreamWriter(objRequest.GetRequestStream());
                    myWriter.Write(post_string);
                    myWriter.Close();

                    // lecture du stream de réponse et conversion en chaine
                    objResponse = (HttpWebResponse)objRequest.GetResponse();
                    using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
                    {
                        post_response = responseStream.ReadToEnd();
                        responseStream.Close();
                    }

                    _cacheDetail.LoadPageText("dbg", post_response, true);
                
            }
            catch (Exception ex)
            {
                KillThreadProgressBar();
                ShowException("Debug error", "An error in debug", ex);
            }
        }

        /// <summary>
        /// Permet de zouiller les attributs d'une cache
        /// </summary>
        public void ZouilleurdAttributs()
        {
        	try
            {
            	List<ParameterObject> lst = new List<ParameterObject>();
	            lst.Add(new ParameterObject(ParameterObject.ParameterType.Int, 5744353, "wptid","Identifiant Cache"));
                List<String> lstval = new List<string>();
                lstval.Add("yes");
                lstval.Add("no");
                lstval.Add("na");
                for (int i = 0; i < 15; i++)
                {
                    lst.Add(new ParameterObject(ParameterObject.ParameterType.String, "", "attid" + i.ToString(), "Attribut #" + i.ToString()));
                    lst.Add(new ParameterObject(ParameterObject.ParameterType.List, lstval, "attst" + i.ToString(), "Status #" + i.ToString()));
                }

                lst.Add(new ParameterObject(ParameterObject.ParameterType.Label, "", "no", "No : 11 19 25 34"));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Label, "", "yes", "Yes : 26 13 32 39"));

                ParametersChanger changer = new ParametersChanger();
	            changer.Title = "Attribut id";
	            changer.BtnCancel = GetTranslator().GetString("BtnCancel");
	            changer.BtnOK = GetTranslator().GetString("BtnOk");
	            changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
	            changer.ErrorTitle = GetTranslator().GetString("Error");
	            changer.Parameters = lst;
	            changer.Font = this.Font;
	            changer.Icon = this.Icon;
	
	            if (changer.ShowDialog() == DialogResult.OK)
	            {
                    String wpt = lst[0].Value;
                    String[] ids = new String[15];
                    String[] stats = new String[15];
                    for (int i = 0; i < 15; i++)
                    {
                        ids[i] = lst[1 + 2*i].Value;
                        stats[i] = lst[2 + 2 * i].Value;
                    }

                    ZouilleurdAttributsImpl(wpt, ids, stats);
	            }
                
            }
            catch (Exception ex)
            {
                KillThreadProgressBar();
                ShowException("Debug error", "An error in debug", ex);
            }
        }
        
        private void RecuperateurDeCaches()
        {
            try
            {
            	/*
            	 * Bonne méthode inspirée de C:Geo
case R.id.menu_map_live:
	mapOptions.isLiveEnabled = !mapOptions.isLiveEnabled;
	if (mapOptions.mapMode == MapMode.LIVE) {
		Settings.setLiveMap(mapOptions.isLiveEnabled);
	}
	caches.handleLiveLayers(mapOptions.isLiveEnabled);
	ActivityMixin.invalidateOptionsMenu(this);
+++++
liveOverlay
LiveCachesOverlay
.download
tokens = GCLogin.getInstance().getMapTokens();
final SearchResult searchResult = ConnectorFactory.searchByViewport(getViewport().resize(1.2), tokens);
final Set<Geocache> result = searchResult.getCachesFromSearchResult(LoadFlags.LOAD_CACHE_OR_DB);
https://github.com/cgeo/cgeo/blob/12c81899e1f45a4175484cc407600a78af52a236/main/src/cgeo/geocaching/connector/gc/GCMap.java
            	 */
            	
            	 
            	String msg = "";
            	
            	UpdateHttpDefaultWebProxy();
                // On checke que les L/MDP soient corrects
                // Et on récupère les cookies au passage
                CookieContainer cookieJar = CheckGCAccount(true, false);
                if (cookieJar == null)
                    return;

                String url = "https://www.geocaching.com/seek/nearest.aspx?lat=48.769383&lng=1.967517&ex=0&children=n";
                // On effectue une requête
                // on parse toutes les pages tant que la distance au centre du viewport est <= au rayon du viewport (max de la diagonale)
                // ensuite on récupère le fichier .loc pour chaque cache avec les infos basiques
                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
                objRequest.Proxy = GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
                objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
                HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
                String response;
                using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
                {
                    response = responseStream.ReadToEnd();
                    responseStream.Close();
                }
                
                // on parse toutes les caches
                List<String> caches = new List<String>();
                List<String> founds = MyTools.GetSnippetsFromText("https://www.geocaching.com/geocache/", "\" class=", response);
                // On supprime les doublons
                foreach(String s in founds)
                {
                	if (!caches.Contains(s))
                	{
                		caches.Add(s);
                	}
                }
                
                foreach(String cache in caches)
                {
	                // Pour chaque cache
	                url = "https://www.geocaching.com/geocache/" + cache;
	                objRequest = (HttpWebRequest)WebRequest.Create(url);
	                objRequest.Proxy = GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
	                objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
	                objResponse = (HttpWebResponse)objRequest.GetResponse();
	                using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
	                {
	                    response = responseStream.ReadToEnd();
	                    responseStream.Close();
	                }
	                
	                // On récupère les viewstates
	                String __VIEWSTATEFIELDCOUNT = "";
	                String[] __VIEWSTATE = null;
	                String __VIEWSTATEGENERATOR = "";
	                GetViewState(response, ref __VIEWSTATEFIELDCOUNT, ref __VIEWSTATE, ref __VIEWSTATEGENERATOR);
	
	                // Préparation des données du POST
	                Dictionary<String, String> post_values = new Dictionary<String, String>();
	                post_values.Add("__EVENTTARGET", "");
	                post_values.Add("__EVENTARGUMENT", "");
	                
	                post_values.Add("ctl00$ContentBody$btnLocDL", "LOC waypoint file");
	
	
	                // Les viewstate
	                post_values.Add("__VIEWSTATE", __VIEWSTATE[0]);
	                if (__VIEWSTATE.Length > 1)
	                {
	                    for (int i = 1; i < __VIEWSTATE.Length; i++)
	                    {
	                        post_values.Add("__VIEWSTATE" + i.ToString(), __VIEWSTATE[i]);
	                    }
	                    post_values.Add("__VIEWSTATEFIELDCOUNT", __VIEWSTATE.Length.ToString());
	                }
	                if (__VIEWSTATEGENERATOR != "")
	                    post_values.Add("__VIEWSTATEGENERATOR", __VIEWSTATEGENERATOR);
	
	                // Encodage des données du POST
	                String post_string = "";
	                foreach (KeyValuePair<String, String> post_value in post_values)
	                {
	                    post_string += post_value.Key + "=" + HttpUtility.UrlEncode(post_value.Value) + "&";
	                }
	                post_string = post_string.TrimEnd('&');
	
	                // Création de la requête pour s'authentifier
	                // Pour l'url de la cache : https://www.geocaching.com/geocache/GC5BVBB_marrrr-et-hummmaaas-box
	                // et le cacheid est là : <a href="javascript:s2gps('4559524');"
	                // C'est bon pour récupérer le loc, mais il faut les viewstates associés à la page :-(
	                objRequest = (HttpWebRequest)WebRequest.Create(url);
	                objRequest.Method = "POST";
	                objRequest.ContentLength = post_string.Length;
	                objRequest.ContentType = "application/x-www-form-urlencoded";
	                objRequest.Proxy = GetProxy(); // Créer votre proxy ici si besoin, sinon mettre NULL
	                objRequest.CookieContainer = cookieJar;
	                
	                // on envoit les POST data dans un stream (écriture)
	                StreamWriter myWriter = null;
	                myWriter = new StreamWriter(objRequest.GetRequestStream());
	                myWriter.Write(post_string);
	                myWriter.Close();
	
	                // lecture du stream de réponse et conversion en chaine
	                objResponse = (HttpWebResponse)objRequest.GetResponse();
	                using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
	                {
	                    response = responseStream.ReadToEnd();
	                    responseStream.Close();
	                }
	                
	                msg += cache + "\r\n" + response + "\r\n****************************************\r\n";
                }
                MSG(msg);
                
            }
            catch (Exception ex)
            {
                KillThreadProgressBar();
                ShowException("Debug error", "An error in debug", ex);
            }
        }
        
        /// <summary>
        /// Return build time
        /// </summary>
        /// <returns>MMddHHmmss format</returns>
        public String GetBuildTime()
        {
        	try
        	{
        		DateTime buildTime = MyTools.GetBuildDateTime(Assembly.GetExecutingAssembly());
            	return buildTime.ToString("MMddHHmm") ; // MMddHHmmss
        	}
        	catch(Exception)
        	{
        		return "?";
        	}
        }
           
        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="nb"></param>
        /// <returns></returns>
        public int FastChoice(String title, int nb)
        {
        	List<ParameterObject> lst = new List<ParameterObject>();
            List<String> lstypes = new List<string>();
            for(int i=0;i<=nb;i++)
            {
            	lstypes.Add(i.ToString());
            }
            lst.Add(new ParameterObject(ParameterObject.ParameterType.List, lstypes, "choix", title));
            
            ParametersChanger changer = new ParametersChanger();
            changer.Title = "Choisir une valeur pour " + title;
            changer.BtnCancel = GetTranslator().GetString("BtnCancel");
            changer.BtnOK = GetTranslator().GetString("BtnOk");
            changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
            changer.ErrorTitle = GetTranslator().GetString("Error");
            changer.Parameters = lst;
            changer.Font = this.Font;
            changer.Icon = this.Icon;
            
            if (changer.ShowDialog() == DialogResult.OK)
            {
            	return Int32.Parse(lst[0].Value);
            }
            else
            	return -1;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="vals"></param>
        /// <returns></returns>
        public int FastChoice(String title, String[] vals)
        {
        	List<ParameterObject> lst = new List<ParameterObject>();
            List<String> lstypes = new List<string>(vals);
            
            lst.Add(new ParameterObject(ParameterObject.ParameterType.List, lstypes, "choix", title));
            
            ParametersChanger changer = new ParametersChanger();
            changer.Title = "Choisir une valeur pour " + title;
            changer.BtnCancel = GetTranslator().GetString("BtnCancel");
            changer.BtnOK = GetTranslator().GetString("BtnOk");
            changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
            changer.ErrorTitle = GetTranslator().GetString("Error");
            changer.Parameters = lst;
            changer.Font = this.Font;
            changer.Icon = this.Icon;
            
            if (changer.ShowDialog() == DialogResult.OK)
            {
            	return lstypes.IndexOf(lst[0].Value);
            }
            else
            	return -1;
        }
        
        private void kmlMaintenanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
            	KmlManager kml = new KmlManager(this);
            	KmlManager.TypeTest en = (KmlManager.TypeTest)(FastChoice("le test", new String[]{ "Réduction de zone", "Génération DB Area", "Lecture DB Area", "Affichage couverture France"}));
            	kml.DoTest(en);
            	KillThreadProgressBarEnh();
            }
            catch (Exception ex)
            {
                KillThreadProgressBarEnh();
                ShowException("Debug error", "An error in debug", ex);
                
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="msg"></param>
        /// <param name="newstyle"></param>
        public void MsgActionOk(Form parent, String msg, bool newstyle = true)
        {
        	MsgActionImpl(parent, msg, GetTranslator().GetString("OkTitle"),
        	              MessageBoxIcon.Information, newstyle, Color.Green, NotificationManager.ToastDelay.Short, 20.0f, false);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="msg"></param>
        /// <param name="newstyle"></param>
        public void MsgActionDone(Form parent, String msg = "", bool newstyle = true)
        {
        	MsgActionImpl(parent, this.GetTranslator().GetStringM("LblActionDone") + msg, GetTranslator().GetString("OkTitle"),
        	              MessageBoxIcon.Information, newstyle, Color.Green, NotificationManager.ToastDelay.Short, 20.0f, false);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="msg"></param>
        /// <param name="newstyle"></param>
        public void MsgActionWarning(Form parent, String msg, bool newstyle = true)
        {
        	String lbl = this.GetTranslator().GetStringM("WarTitle");
        	MsgActionImpl(parent, msg, lbl, MessageBoxIcon.Exclamation, newstyle, Color.Blue, NotificationManager.ToastDelay.Medium, 20.0f, false);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="msg"></param>
        /// <param name="newstyle"></param>
        public void MsgActionError(Form parent, String msg, bool newstyle = true)
        {
        	String lbl = this.GetTranslator().GetStringM("ErrTitle");
        	if (msg != "")
        		msg = " - " + msg;
        	MsgActionImpl(parent, lbl + msg, lbl, MessageBoxIcon.Error, newstyle, Color.Red, NotificationManager.ToastDelay.Long, 20.0f, true);
        }
        
	  	/// <summary>
	  	/// 
	  	/// </summary>
	  	/// <param name="parent"></param>
	  	/// <param name="msg"></param>
	  	/// <param name="newstyle"></param>
        public void MsgActionCanceled(Form parent, String msg = "", bool newstyle = true)
        {
        	
        	String lbl = this.GetTranslator().GetStringM("LblOperationCancelled");
        	if (msg != "")
        		msg = " - " + msg;
        	MsgActionImpl(parent, lbl + msg, lbl, MessageBoxIcon.Exclamation, newstyle, Color.Orange, NotificationManager.ToastDelay.Short, 20.0f, false);
        }
        
        private void MsgActionImpl(Form parent, String msg, String title, MessageBoxIcon icon, bool newstyle, Color color, NotificationManager.ToastDelay delay, float fontsize, bool redbackground)
        {
        	if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate()
                {
		        	if (newstyle)
		        	{
			        	NotificationManager.Show(parent,
			                     msg,
			                     color,
			                     delay,
			                     NotificationManager.CloneFont(parent.Font, fontsize),
			                     redbackground);
		        	}
		        	else
		        	{
		        		MyMessageBox.Show(msg,
		                    title,
		                    icon,
		                    GetTranslator());
		        	}
        		 });
        	}
        	else
        	{
        		if (newstyle)
	        	{
		        	NotificationManager.Show(parent,
		                     msg,
		                     color,
		                     delay,
		                     NotificationManager.CloneFont(parent.Font, fontsize),
		                     redbackground);
	        	}
	        	else
	        	{
	        		MyMessageBox.Show(msg,
	                    title,
	                    icon,
	                    GetTranslator());
	        	}
        	}
        }
        
        /// <summary>
        /// Returns center of map if valid or home coordinates
        /// </summary>
        /// <returns></returns>
        public String GetInitialCoordinates()
        {
        	String msg = HomeLat.ToString() + " " + HomeLon.ToString();
        	if ((_cacheDetail != null) && (_cacheDetail._gmap != null))
        	{
        		var pos = _cacheDetail._gmap.Position;
        		if ((pos.Lat != 0.0) && (pos.Lng != 0.0))
        		{
        			msg = pos.Lat.ToString() + " " + pos.Lng.ToString();
        		}
        	}
        	
        	return msg;
        }
        
        /// <summary>
        /// Returns center of map if valid or home coordinates
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        public void GetInitialCoordinates(out double lat, out double lon)
        {
        	lat = HomeLat;
        	lon = HomeLon;
        	if ((_cacheDetail != null) && (_cacheDetail._gmap != null))
        	{
        		var pos = _cacheDetail._gmap.Position;
        		if ((pos.Lat != 0.0) && (pos.Lng != 0.0))
        		{
        			lat = pos.Lat;
        			lon = pos.Lng;
        		}
        	}
        }

        private void btnDEBUG_Click(object sender, EventArgs e)
        {
            try
            {
                String msg = "";
                String apikey = ConfigurationManager.AppSettings["yandextrnsl"];
                List<String> lst;
                var dico = GetSupportedYandexLng(apikey, out lst);
                
                foreach (var name in lst)
                    msg += name + "\r\n";

                MSG(msg);
            	//CustomFilterDateCreation fltr = new CustomFilterDateCreation(new string[]{ "01-08", "01-21",  "02-24", "12-17", "12-24"});
            	//ExecuteCustomFilter(fltr);
            	/*
            	var caches = GetSelectedCaches();
                if (caches.Count == 0)
                    return;
                foreach(var geo in caches)
                {
                	// <font size="+1"># ZAM'S #</font>
                	String nom = MyTools.GetSnippetFromText("<font size=\"+1\"># ", " #</font>", geo._LongDescriptionInHTML);
                	geo._Name += " - " + nom;
                	_iNbModifiedCaches += geo.InsertModification("NAME");
                }
                
                PostTreatmentLoadCache();

                // Better way to do that : only recreate for modified caches
                RecreateVisualElements(caches);

                KillThreadProgressBarEnh();

                MsgActionDone(this);*/
            }
            catch (Exception ex)
            {
                KillThreadProgressBarEnh();
                ShowException("Debug error", "An error in debug", ex);
                
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ToolStripButtonGenericClick(object sender, EventArgs e)
		{
        	String id = ((ToolStripButton)sender).Tag as String;
        	Object menu = MyTools.FindControl(this, id);
        	if (menu != null)
        	{
        		ToolStripMenuItem tsi = menu as ToolStripMenuItem;
        		if (tsi != null)
        		{
        			if (tsi.DropDownItems.Count == 0)
        				tsi.PerformClick();
        			else
        				tsi.ShowDropDown();
        			
        			return;
        		}
        	}
        	
        	MSG(id + " non trouvé");
		}
        

        /// <summary>
        /// Create and display a route on map display
        /// </summary>
        /// <param name="map">map control</param>
        /// <param name="overlay">gmap overlay</param>
        /// <param name="originalRoute">route to display</param>
        /// <param name="avoidHighways">if true, route will avoid highways</param>
        /// <param name="walkingMode">if true, route is computed for a pedestrian</param>
        /// <param name="bCreateMarkers">if true, intermediate markers will be created</param>
        public void CreateRoutableShunks(GMapControl map, GMapOverlay overlay, GMapRoute originalRoute, bool avoidHighways, bool walkingMode, bool bCreateMarkers)
        {
            int nbpts = originalRoute.Points.Count();
            if (nbpts >= 2) // Sinon ça n'a pas de sens
            {
                // On test le routing provider - est-il routable ?
                bool bRoutable = false, bDurationAvailable = false;
                // On checke les propriétés du routing provider
                GMap.NET.MapProviders.GMapProvider provider = map.MapProvider;
                CacheDetail.IsRoutableProvider(provider, ref bRoutable, ref bDurationAvailable);
                // Et on test le cast
                RoutingProvider rp = map.MapProvider as RoutingProvider;

                for (int i = 0; i < (nbpts - 1); i++)
                {
                    PointLatLng start = originalRoute.Points[i];
                    PointLatLng end = originalRoute.Points[i + 1];
                    MapRoute route = (rp == null)?null:rp.GetRoute(start, end, avoidHighways, walkingMode, (int)map.Zoom);
                    if (route == null)
                    {
                        // On tente par la direction en utilisant le provider GoogleMaps
                        GDirections ss;
                        var xx = GMap.NET.MapProviders.GMapProviders.GoogleMap.GetDirections(out ss, start, end, avoidHighways, false, walkingMode, false, false);
                        Log("DirectionsStatusCode: " + xx.ToString());
                        if ((xx == DirectionsStatusCode.OK) && (ss !=  null) && (ss.Route != null))
                        	route = new GMapRoute(ss.Route, "My route");
                    }

                    if (route != null)
                    {
                        // On change le nom de la carte en fonction de la dispo de la duration
                        if (bDurationAvailable)
                        {
                            // Si on a un mapprovider qui supporte la durée, on ne touche pas au nom de la route
                        }
                        else
                        {
                            String kmmi = (_bUseKm) ? GetTranslator().GetString("LVKm") : GetTranslator().GetString("LVMi");
                            double dist = (_bUseKm) ? route.Distance : route.Distance * _dConvKmToMi;
                            String tooltiptext = dist.ToString("0.0") + " " + kmmi;
                            route.Name = tooltiptext;
                        }

                        // add route
                        GMapRoute r = new GMapRoute(route.Points, route.Name);
                        r.IsHitTestVisible = true;
                        r.Tag = map;
                        overlay.Routes.Add(r);

                        // add route start/end marks
                        if (bCreateMarkers)
                        {
                            GMapMarkerImage marker = new GMapMarkerImage(GetImageSized("PolygonePoint"), start);
                            marker.ToolTipText = route.Name;
                            marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                            overlay.Markers.Add(marker);
                        }
                    }
                }
            }
        }

        private String GetErrorMessageFromYandex(int i)
        {
            switch (i)
            {
                case 200:
                    return "Operation completed successfully";
                case 401:
                    return GetTranslator().GetString("TranslateErr") + "\r\nInvalid API key";
                case 402:
                    return GetTranslator().GetString("TranslateErr") + "\r\nBlocked API key";
                case 404:
                    return GetTranslator().GetString("TranslateErr") + "\r\nExceeded the daily limit on the amount of translated text";
                case 413:
                    return GetTranslator().GetString("TranslateErr") + "\r\nExceeded the maximum text size";
                case 422:
                    return GetTranslator().GetString("TranslateErrSrcLng") + "\r\nThe text cannot be translated";
                case 501:
                    return GetTranslator().GetString("TranslateErrSrcLng") + "\r\nThe specified translation direction is not supported";
                default:
                    return GetTranslator().GetString("TranslateErr") + "\r\n???";
            }
        }

        private IEnumerable<string> SplitByLength(string str, int maxLength)
        {
            for (int index = 0; index < str.Length; index += maxLength)
            {
                yield return str.Substring(index, Math.Min(maxLength, str.Length - index));
            }
        }

        private int TranslateString(ref String inputString, String keyBing, String keyBingEnd, String apikey, String sourceLng, String targetLng, String failBing, String bonusText, String bonusLineFeed)
        {
            if (inputString != "")
            {

                String outputString = "";
                // On vire les traductions précédentes
                int ipos = inputString.IndexOf(keyBing);
                if (ipos != -1)
                    inputString = inputString.Substring(0, ipos);
                // On vire les traductions foirées
                ipos = inputString.IndexOf(failBing);
                if (ipos != -1)
                    inputString = inputString.Substring(0, ipos);

                int r = 200;
                // Maximum characters limit per call is 10000
                // If superior, we need to split
                int iMaxSize = 4500; // add a little bit of margin
                List<String> toTranslate = SplitByLength((bonusText == "") ? inputString : (bonusText + bonusLineFeed + inputString), iMaxSize).ToList();
                try
                {
                    foreach (String s in toTranslate)
                    {
                        String lngs = "";
                        if (sourceLng == "")
                            lngs = targetLng;
                        else
                            lngs = sourceLng + "-" + targetLng;

                        String text = s;
                        //text = MyTools.StripHtmlTags(text);
                        text = System.Uri.EscapeDataString(text).Replace(" ", "%20").Replace("&", "%26");

                        String strJson = MyTools.GetRequest(
                            new Uri(
                                "https://translate.yandex.net/api/v1.5/tr.json/translate?key=" + apikey + "&lang=" + lngs + "&format=plain&text=" + text), GetProxy(), -1);
                        System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
                        YandexRes yr = js.Deserialize<YandexRes>(strJson);
                        r = yr.code;
                        if (r != 200) // Error, no need to continue
                        {
                            break;
                        }
                        else
                        {
                            foreach (var t in yr.text)
                                outputString += t;
                        }
                    }

                    if (outputString != "")
                    {
                        if (r == 200)
                            inputString += keyBing + " [" + targetLng.ToUpper() + "] " + outputString + keyBingEnd;
                        else
                            inputString += keyBing + " [" + targetLng.ToUpper() + "] " + GetErrorMessageFromYandex(r) + keyBingEnd;
                    }
                }
                catch (Exception ex)
                {
                    // On a planté ici !
                    if (failBing == "")
                    {
                        // On n'a rien prévu de particulier, on forwarde///
                        throw;
                    }
                    else
                    {
                        // On bouffe l'exception
                        inputString += failBing;
                    }
                }

                return r;
            }
            else
                return -10;
        }

        /// <summary>
        /// 
        /// </summary>
        public class YandexLng
        {
        	/// <summary>
        	/// 
        	/// </summary>
            public List<string> dirs { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class YandexRes
        {
        	/// <summary>
        	/// 
        	/// </summary>
            public int code { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string lang { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<string> text { get; set; }
        }

        Dictionary<String, String> GetSupportedYandexLng(String apikey, out List<String> sortedNames)
        {
            String strJson = MyTools.GetRequest(new Uri("https://translate.yandex.net/api/v1.5/tr.json/getLangs?key=" + apikey), GetProxy(), 2000);
            System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
            YandexLng yl = js.Deserialize<YandexLng>(strJson);
            Dictionary<String, String> dicoNameCode = new Dictionary<string, string>();
            foreach (var lng in yl.dirs)
            {
                var codes = lng.Split('-');
                String code = codes[0];
                var name = new CultureInfo(code).DisplayName;
                if (!dicoNameCode.ContainsKey(name))
                    dicoNameCode.Add(name, code);
                code = codes[1];
                name = new CultureInfo(code).DisplayName;
                if (!dicoNameCode.ContainsKey(name))
                    dicoNameCode.Add(name, code);
            }
            sortedNames = dicoNameCode.Keys.ToList();
            sortedNames.Sort();
            return dicoNameCode;
        }

        private void TranslateCachesDescription()
        {
            List<Geocache> caches = null;
            String owner = ConfigurationManager.AppSettings["owner"].ToLower();
            try
            {
                UpdateHttpDefaultWebProxy();
                caches = GetSelectedCaches();
                if (caches.Count == 0)
                    return;

                // yandextrnsl
                String apikey = ConfigurationManager.AppSettings["yandextrnsl"];
                if (apikey == "")
                {
                    MsgActionError(this, GetTranslator().GetStringM("LblMissingTranslateAccountKey"));
                    return;
                }

                List<String> names;
                var dicoNameCode = GetSupportedYandexLng(apikey, out names);


                List<ParameterObject> lst = new List<ParameterObject>();
                List<String> lstvs = new List<string>();
                List<String> lstvd = new List<string>();
                String targetLng = GetTargetLngAccordingToLocale(dicoNameCode);
                String sourceLng = "";
                foreach (var n in names)
                {
                    lstvs.Add(n);
                    lstvd.Add(n);
                }
                String sAutoDetect = "* Auto Detect *";
                lstvs.Insert(0, sAutoDetect);
                lst.Add(new ParameterObject(ParameterObject.ParameterType.List, lstvs, "lngs", GetTranslator().GetString("LblTranslationLngSrc")));
                var po = new ParameterObject(ParameterObject.ParameterType.List, lstvd, "lngd", GetTranslator().GetString("LblTranslationLngDest"));
                lst.Add(po);
                po.DefaultListValue = targetLng;

                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "desc", GetTranslator().GetString("ChkTranslateDescription")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "hint", GetTranslator().GetString("ChkTranslateHint")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "logs", GetTranslator().GetString("ChkTranslateLogs")));

                ParametersChanger changer = new ParametersChanger();
                changer.Title = GetTranslator().GetString("LblTranslationLngDest");
                changer.BtnCancel = GetTranslator().GetString("BtnCancel");
                changer.BtnOK = GetTranslator().GetString("BtnOk");
                changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
                changer.ErrorTitle = GetTranslator().GetString("Error");
                changer.Parameters = lst;
                changer.Font = this.Font;
                changer.Icon = this.Icon;

                bool bDesc = true;
                bool bHint = true;
                bool bLogs = false;
                if (changer.ShowDialog() == DialogResult.OK)
                {
                    String nicelngsrc = changer.Parameters[0].Value;
                    
                    if (nicelngsrc == sAutoDetect)
                        nicelngsrc = "";

                    String nicelng = changer.Parameters[1].Value;
                    foreach(var p in dicoNameCode)
                    {
                        if (p.Key == nicelng)
                            targetLng = p.Value;

                        if ((nicelngsrc != "")&&(p.Key == nicelngsrc))
                            sourceLng = p.Value;
                    }

                    bDesc = (changer.Parameters[2].Value == "True");
                    bHint = (changer.Parameters[3].Value == "True");
                    bLogs = (changer.Parameters[4].Value == "True");
                }
                else
                    return;
                
                _ThreadProgressBarTitle = GetTranslator().GetString("FMenuTranslation");
                CreateThreadProgressBarEnh();

                // Wait for the creation of the bar
                while (_ThreadProgressBar == null)
                {
                    Thread.Sleep(10);
                    Application.DoEvents();
                }
                _ThreadProgressBar.progressBar1.Maximum = caches.Count();
                _ThreadProgressBar.lblWait.Text = "";

                String errorMsg = "";
                foreach (Geocache geo in caches)
                {
                    String keyBing = "<br><b><font color=#FF0000>[TRANSLATION - START]</font></b>";
                    String keyBingEnd = "<b><font color=#FF0000>[TRANSLATION - END]</font></b><br>";
                    String keyBing2= "[TRANSLATION - START]";
                    String keyBingEnd2 = "[TRANSLATION - END]";
                    String failBing = "<br><b><font color=#0000FF>[TRANSLATION - FAILED!]</font></b>";
                    String failBing2 = "[TRANSLATION - FAILED!]";
                    try
                    {
                        // Description
                        if (bDesc)
                        {
                            // On va éventuellement détecter Additional waypoints & co, les ignorer et les rajouter ensuite
                            int pos1 = geo._LongDescription.IndexOf("Additional Hidden Waypoints");
                            int pos2 = geo._LongDescription.IndexOf("Additional Waypoints");
                            if (pos1 == -1)
                                pos1 = Int32.MaxValue;
                            if (pos2 == -1)
                                pos2 = Int32.MaxValue;
                            int pos = Math.Min(pos1, pos2);

                            String debdesc = geo._LongDescription;
                            String enddesc = "";
                            if (pos != Int32.MaxValue)
                            {
                                // On découpe
                                debdesc = geo._LongDescription.Substring(0, pos);
                                enddesc = geo._LongDescription.Substring(pos);
                            }

                            TranslateString(ref debdesc, keyBing, keyBingEnd, apikey, sourceLng, targetLng, failBing, "", "");
                            geo._LongDescription = debdesc + enddesc;

                            // Pour Tof, on rajoute la traduction du titre
                            TranslateString(ref geo._ShortDescription, keyBing, keyBingEnd, apikey, sourceLng, targetLng, failBing, geo._Name, ((geo._ShortDescHTML == "True") ? "<br>" : "\r\n"));

                            _iNbModifiedCaches += geo.InsertModification("LONGDESC");
                            _iNbModifiedCaches += geo.InsertModification("SHORTDESC");
                        }

                        // Hint
                        if (bHint)
                        {
                            TranslateString(ref geo._Hint, keyBing2, keyBingEnd2, apikey, sourceLng, targetLng, failBing2, "", "");
                            _iNbModifiedCaches += geo.InsertModification("HINT");
                        }

                        // Logs
                        if (bLogs)
                        {
                            foreach (CacheLog log in geo._Logs)
                            {
                                TranslateString(ref log._Text, keyBing, keyBingEnd, apikey, sourceLng, targetLng, failBing, "", "");
                                _iNbModifiedCaches += geo.InsertModification("LOGS");
                            }
                        }

                        geo.UpdatePrivateData(owner);
                    }
                    catch (Exception ex2)
                    {
                        String t1 = GetTranslator().GetString("TranslateErr") + ": " + geo._Code + " - " + geo._Name;
                        String msg = GetException(t1, ex2);
                        Log(msg);
                        errorMsg += msg + Environment.NewLine;
                        errorMsg += "**************************" + Environment.NewLine;
                    }

                    _ThreadProgressBar.Step();
                    if (_ThreadProgressBar._bAbort)
                        break;
                }

                PostTreatmentLoadCache();

                // Better way to do that : only recreate for modified caches
                RecreateVisualElements(caches);

                KillThreadProgressBarEnh();

                if (errorMsg == "")
                {
                    // GC43X6P
                    MsgActionDone(this);
                }
                else
                {
                    MsgActionError(this, GetTranslator().GetString("FMenuTranslate"));    
                }
            }
            catch (Exception ex)
            {
                PostTreatmentLoadCache();
                RecreateVisualElements(caches);

                KillThreadProgressBarEnh();
                ShowException("", "Translate cache description", ex);
            }
        }

        private String GetTargetLngAccordingToLocale(Dictionary<String, String> dicoNameCode)
        {
            String keyLng = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            foreach(var pair in dicoNameCode)
            {
                if (pair.Value == keyLng)
                    return pair.Key;
            }
            return new CultureInfo("en").DisplayName; ;
        }

        private void DisplayDTMatrix()
        {
            List<Geocache> caches = GetDisplayedCaches();
            string key = "";
            List<Geocache> lst = null;
            Dictionary<String, List<Geocache>> dtmatrix = new Dictionary<string, List<Geocache>>();
            foreach (Geocache geo in caches)
            {
                // only for real GC
                key = geo._D + geo._T;
                if (dtmatrix.ContainsKey(key))
                {
                    // found
                    lst = dtmatrix[key];
                }
                else
                {
                    // not found, create it
                    lst = new List<Geocache>();
                    dtmatrix.Add(key, lst);
                }

                // Add current cache to lst
                lst.Add(geo);
            }

            String earthpath = "<img src=\"file:\\\\" + GetResourcesDataPath() + Path.DirectorySeparatorChar + "Img" + Path.DirectorySeparatorChar + "Earth.gif\">";
            String html = "<html><body>";
            String lhtml = "";
            String thtml = "<table border=1>";
            thtml += "<tr><td>D/T</td><td bgcolor=#CCCCFF><b>T1</b></td><td bgcolor=#CCCCFF><b>T1.5</b></td><td bgcolor=#CCCCFF><b>T2</b></td><td bgcolor=#CCCCFF><b>T2.5</b></td><td bgcolor=#CCCCFF><b>T3</b></td><td bgcolor=#CCCCFF><b>T3.5</b></td><td bgcolor=#CCCCFF><b>T4</td><td bgcolor=#CCCCFF><b>T4.5</td><td bgcolor=#CCCCFF><b>T5</td></tr>";
            for (int d = 2; d <= 10; d++)
            {
                double dd = (double)d / 2.0;
                String sd = dd.ToString().Replace(",", ".");

                thtml += "<tr><td bgcolor=#CCCCFF><b>D" + sd + "</b></td>";
                for (int t = 2; t <= 10; t++)
                {
                    double dt = (double)t / 2.0;
                    String st = dt.ToString().Replace(",", ".");
                    key = sd + st;
                    String lblkey = "D" + sd + " / T" + st;
                    String sval = "&nbsp;";
                    if (dtmatrix.ContainsKey(key))
                    {
                        // Create link
                        sval = "<a name=v" + key + "></a><a href=#l" + key + ">" + dtmatrix[key].Count.ToString() + "</a>";

                        // Create target of link
                        lhtml += "<b><a name=l" + key + "></a><a href=#v" + key + ">" + lblkey + "</a></b><br>\r\n";
                        foreach (Geocache g in dtmatrix[key])
                        {
                            lhtml += "<li><a href=MGMGEOM:" + g._Code + ">" + earthpath + "</a>&nbsp;<a href=MGMGEO:" + g._Code + ">" + g._Code + "</a>&nbsp;" + g._Name + "(" + g._Type + ") - ";
                            if (_bUseKm)
                                lhtml += String.Format("{0:0.#}", g.DistanceToHome()).ToString().Replace(",", ".") + " " + GetTranslator().GetString("LVKm");
                            else
                                lhtml += String.Format("{0:0.#}", g.DistanceToHomeMi()).ToString().Replace(",", ".") + " " + GetTranslator().GetString("LVMi");
                            lhtml += "</li>\r\n";
                        }
                        lhtml += "<br><br>";
                    }
                    thtml += "<td>" + sval + "</td>";

                }
                thtml += "</tr>\r\n";
            }

            thtml += "</table>";

            html += thtml;
            html += "<hr>";
            html += lhtml;
            html += "</body></html>";
            _cacheDetail.LoadPageText("DT Matrix", html, true);
        }

        private void displayCacheonMapTab(Geocache cache)
        {
            if (ConfigurationManager.AppSettings["displaytabmap"] == "True")
            {
                _cachesPreviewMap.Position = new PointLatLng(cache._dLatitude, cache._dLongitude);

                if ((tabPage15_cachesPreviewMap.Tag != null) && (((bool)(tabPage15_cachesPreviewMap.Tag)) == true))
                {
                    // It's maximized, so we display it in the cache detail
                    _cacheDetail.DisplayCacheMap(cache._dLatitude, cache._dLongitude);
                }

            }
        }

        private static bool IsPathOnRemovableDrive(String path)
        {
            bool bRemovable = false;
            try
            {
                String root = Path.GetPathRoot(path);
                DriveInfo[] drives = DriveInfo.GetDrives();
                foreach (DriveInfo di in drives)
                {
                    if (di.Name == root)
                    {
                        if (di.DriveType == DriveType.Removable)
                            bRemovable = true;
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }
            return bRemovable;
        }

        private void CreateAttributeIconsHMINew(ref List<AttributeImage> _listPb, TabPage page, bool bIn)
        {
            if ((_imgAttributesGreyed == null) || (_imgAttributesGreyed.Count == 0))
                return;

            if (_listPb != null)
            {
                foreach (AttributeImage p in _listPb)
                {
                    if (p._PictureBox != null)
                    {
                        Controls.Remove(p._PictureBox);
                        p._PictureBox.Dispose();
                    }
                }
                _listPb.Clear();
            }
            else
                _listPb = new List<AttributeImage>();

            // Deal with the groups
            GroupBox[] groupsPlus = new GroupBox[] { groupBox6, groupBox7, groupBox8, groupBox10, groupBox9, groupBox11 };
            GroupBox[] groupsMinus = new GroupBox[] { groupBox17, groupBox16, groupBox13, groupBox14, groupBox15, groupBox12 };
            GroupBox[] groups = (bIn) ? groupsPlus : groupsMinus;

            AddRightAttributesToGroup(_listPb, bIn, 3, groups[0], 1);
            AddRightAttributesToGroup(_listPb, bIn, 4, groups[1], 2);
            AddRightAttributesToGroup(_listPb, bIn, 8, groups[2], 3);
            AddRightAttributesToGroup(_listPb, bIn, 3, groups[3], 4);
            AddRightAttributesToGroup(_listPb, bIn, 4, groups[4], 5);
            AddRightAttributesToGroup(_listPb, bIn, 1, groups[5], 6);

        }

        private void AddRightAttributesToGroup(List<AttributeImage> _listPb, bool bIn, int imax, GroupBox grp, int iGroup)
        {
            // some values
            double factor = 0.8;
            // 0;1 or 5;0
            int i = 0;  // to dodge (checkbox)
            int j = 1;  // to dodge (checkbox)
            int w = (int)(_imgAttributesGreyed.First().Value.Size.Width * factor);
            int h = (int)(_imgAttributesGreyed.First().Value.Size.Height * factor);
            const int marge1 = 5; // instead of 8
            const int marge2 = -10; // instead of 8
            const int marge = 2;

            foreach (KeyValuePair<String, String> paire in _tableAttributes)
            {
                if (paire.Key.Contains("-no"))
                    continue;

                String att_name = paire.Value;
                String att_name_clean = paire.Value.ToLower().Replace("/", "");
                
                String index = GetIndexOfAttribute(att_name);
                int cat = -1;
                if (index != null)
                    cat = _tableAttributesCategory[index];
                try
                {
                    if (cat == iGroup)
                    {
                        // Get image
                        AttributeImage at = new AttributeImage();

                        PictureBox pb = new PictureBox();
                        pb.Location = new Point(marge1 + i * (w + marge), marge2 + j * (h + marge));
                        pb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                        pb.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
                        pb.Name = "pb_" + att_name;
                        pb.Size = new Size(w, h);
                        pb.Image = _imgAttributesGreyed[att_name_clean + "-grey"];
                        pb.SizeMode = PictureBoxSizeMode.StretchImage;
                        if (bIn)
                        {
                            pb.MouseEnter += new System.EventHandler(this.Atts_pictureBox_MouseHoverIn);
                            pb.MouseLeave += new System.EventHandler(this.Atts_pictureBox_MouseLeaveIn);
                            pb.Click += new System.EventHandler(this.Atts_pictureBoxIn_Click);
                        }
                        else
                        {
                            pb.MouseEnter += new System.EventHandler(this.Atts_pictureBox_MouseHoverOut);
                            pb.MouseLeave += new System.EventHandler(this.Atts_pictureBox_MouseLeaveOut);
                            pb.Click += new System.EventHandler(this.Atts_pictureBoxOut_Click);
                        }

                        grp.Controls.Add(pb);

                        ((System.ComponentModel.ISupportInitialize)(pb)).EndInit();
                        at._Index = 0;
                        at._Key = att_name_clean;
                        at._PictureBox = pb;
                        at._Images[0] = _imgAttributesGreyed[at._Key + "-grey"];
                        if (_imgAttributes.ContainsKey(at._Key))
                            at._Images[1] = _imgAttributes[at._Key];
                        if (_imgAttributes.ContainsKey(at._Key + "-no"))
                            at._Images[2] = _imgAttributes[at._Key + "-no"];
                        pb.Tag = at;

                        _listPb.Add(at);
                        i++;
                        if (i == imax)
                        {
                            j++;
                            i = 0;
                        }
                    }
                }
                catch (Exception exc)
                {
                	Log(GetException("Building attributes HIM filters",exc));
                    Log(att_name + " => " + index + " => " + cat.ToString());
                }
            }
        }
        

        private void exportCurrentDisplayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportCaches(false);
        }

        /// <summary>
        /// Export caches in a supported format
        /// </summary>
        /// <param name="selection">if true, only selection will be exported</param>
        public void ExportCaches(bool selection)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "GPX (*.gpx)|*.gpx|HTML (*.html)|*.html|GGZ (*.ggz)|*.ggz|CSV (*.csv)|*.csv|TomTom OV2 (*.ov2)|*.ov2|SQLite (*.db)|*.db";

            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Cursor old = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                String fileRadix = "";

                // Si on exporte en GPX ou en GGZ des caches modifiées, on considère qu'on a sauvé les modifications
                // du coup il faut rafraichir la liste
                bool bCheckRefreshForModifications = false;
                int old_iNbModifiedCaches = _iNbModifiedCaches;
                
                switch (saveFileDialog1.FilterIndex)
                {
                    case 1: // Simple GPX
                		{
                			bCheckRefreshForModifications = true;
                        	fileRadix = ExportGPX(saveFileDialog1, selection);
                        	break;
                		}
                    case 2: // Simple HTML
                        fileRadix = ExportHtmlWrap(saveFileDialog1, selection);
                        break;
                    case 3: // GGZ file
                        {
                        	bCheckRefreshForModifications = true;
	                        fileRadix = ExportGGZ(saveFileDialog1, selection);
	                        break;
                        }
                    case 4: // CSV
                        fileRadix = ExportCSV(saveFileDialog1, selection);
                        break;
                    case 5: // OV2
                        fileRadix = ExportOV2(saveFileDialog1, selection);
                        break;
                    case 6: // SQLite
                        fileRadix = ExportSQL(saveFileDialog1, selection);
                        break;
                    default: // WTF ?
                        break;
                }

                if (bCheckRefreshForModifications && (_iNbModifiedCaches != old_iNbModifiedCaches))
                {
                    // On a sauvé des caches modifiées du coup il faut rafraichir. 
                    // Better way to do that : only recreate for modified caches
                    List<Geocache> cachestoprocess = null;
                    if (selection)
                        cachestoprocess = GetSelectedCaches();
                    else
                        cachestoprocess = GetDisplayedCaches();
                    RecreateVisualElements(cachestoprocess);
                }

                Cursor.Current = old;
            }
        }

        /// <summary>
        /// Export caches in HTML
        /// </summary>
        /// <param name="saveFileDialog1">save dialog for export destination</param>
        /// <param name="selection">if true, only selection will be exported</param>
        /// <returns>export file name</returns>
        public String ExportHtmlWrap(SaveFileDialog saveFileDialog1, bool selection)
        {
            HtmlExportDialog htmlexport = new HtmlExportDialog(this);
            htmlexport.Font = this.Font;
            htmlexport.Icon = this.Icon;

            if (htmlexport.ShowDialog() == DialogResult.OK)
            {
                bool bWithDetails = htmlexport.radioButton1htmlexport.Checked;
                bool bWithMap = htmlexport.radioButton4htmlexport.Checked;
                return ExportHTML(selection, bWithDetails, bWithMap, saveFileDialog1);
            }
            else
                return "";

        }

        private String ExportGGZ(SaveFileDialog saveFileDialog1, bool selection)
        {
            try
            {
                String radix = ExportGGZ(saveFileDialog1.FileName, selection);
                MsgActionOk(this, GetTranslator().GetString("MBExportDone"));
                return radix;
            }
            catch (Exception exc2)
            {
                ShowException("", "Exporting GGZ", exc2);
            }
            return "";
        }

        /// <summary>
        /// Export caches in a GGZ file
        /// </summary>
        /// <param name="theFileName">target filename</param>
        /// <param name="selection">if true, only selected caches will be exported</param>
        /// <returns>export file name</returns>
        public String ExportGGZ(String theFileName, bool selection)
        {
            List<Geocache> cachestoprocess = null;
            if (selection)
                cachestoprocess = GetSelectedCaches();
            else
                cachestoprocess = GetDisplayedCaches();
            return ExportGGZ(theFileName, cachestoprocess);
        }

        /// <summary>
        /// Export caches in a GGZ file
        /// </summary>
        /// <param name="theFileName">target filename</param>
        /// <param name="cachestoprocess">list of caches to export</param>
        /// <returns>export file name</returns>
        public String ExportGGZ(String theFileName, List<Geocache> cachestoprocess)
        {
            return ExportGGZ(theFileName, cachestoprocess, DateTime.Now);
        }

        /// <summary>
        /// Export caches in a GGZ file
        /// </summary>
        /// <param name="theFileName">target filename</param>
        /// <param name="cachestoprocess">list of caches to export</param>
        /// <param name="exportDate">export date that will be indicated in the GGZ</param>
        /// <returns>export file name</returns>
        public String ExportGGZ(String theFileName, List<Geocache> cachestoprocess, DateTime exportDate)
        {
            try
            {
                _ThreadProgressBarTitle = "GGZ Export";
                CreateThreadProgressBar();

                if (File.Exists(theFileName))
                	File.Delete(theFileName);
                
                FileInfo fi = new FileInfo(theFileName);
                Directory.SetCurrentDirectory(fi.Directory.ToString());
                String fileRadix = fi.Name.ToString();
                String fileRadixNoExtension = fileRadix.Substring(0, fileRadix.Length - fi.Extension.Length);
                int iNbCachesPerGPXFile = 200;

                // 1- create a random directory
                string tmpDirectory = Guid.NewGuid().ToString();
                if (Directory.Exists(tmpDirectory))
                {
                    Log("Delete old dir: " + tmpDirectory);
                    try
                    {
                        MyTools.DeleteDirectory(tmpDirectory, true);
                    }
                    catch (Exception exc1)
                    {
                    	Log("!!!! " + GetException("Deleting old directory", exc1));
                        Log("!!!! Failed deleting " + tmpDirectory);
                    }
                }
                else
                {
                    Directory.CreateDirectory(tmpDirectory);
                }
                
                // 1b - create data structure
                char sep = Path.DirectorySeparatorChar;
                Directory.CreateDirectory(tmpDirectory + sep + "data");
                Directory.CreateDirectory(tmpDirectory + sep + "index");
                Directory.CreateDirectory(tmpDirectory + sep + "index" + sep + "com");
                Directory.CreateDirectory(tmpDirectory + sep + "index" + sep + "com" + sep + "garmin");
                Directory.CreateDirectory(tmpDirectory + sep + "index" + sep + "com" + sep + "garmin" + sep + "geocaches");
                Directory.CreateDirectory(tmpDirectory + sep + "index" + sep + "com" + sep + "garmin" + sep + "geocaches" + sep + "v0");
                String fileIndex = tmpDirectory + sep + "index" + sep + "com" + sep + "garmin" + sep + "geocaches" + sep + "v0" + sep + "index.xml";

                // 1c- Create index file (need to be completed on the fly)
                System.IO.StreamWriter file = new System.IO.StreamWriter(fileIndex, false);
                file.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
                file.WriteLine("<ggz xmlns=\"http://www.opencaching.com/xmlschemas/ggz/1/0\">");
                String date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
                file.WriteLine("    <time>" + date + "</time>");

                // 2- create /data/geocache(i).gpx
                List<Geocache> caches = new List<Geocache>();
                int inum = 0;
                int iiter = 1;
                String internalGpxFile = fileRadixNoExtension + "_{0}.gpx";
                String gpxfile = tmpDirectory + sep + "data" + sep + internalGpxFile;
                
                foreach (Geocache geo in cachestoprocess)
                {
                    caches.Add(geo);
                    inum++;

                    if (inum == iNbCachesPerGPXFile)
                    {
                        file.WriteLine("    <file>");
                        file.WriteLine(String.Format("        <name>" + internalGpxFile + "</name>",iiter));
                        String fgpx = String.Format(gpxfile,iiter);
                        ExportGPXStreamed(fgpx, caches, file, exportDate);
                        file.WriteLine("    </file>");
                        inum = 0;
                        iiter++;
                        caches.Clear();
                    }
                }
                if (caches.Count != 0)
                {
                    file.WriteLine("    <file>");
                    file.WriteLine(String.Format("        <name>" + internalGpxFile + "</name>", iiter));
                    String fgpx = String.Format(gpxfile, iiter);
                    ExportGPXStreamed(fgpx, caches, file, exportDate);
                    file.WriteLine("    </file>");
                    caches.Clear();
                }

                // 3- create /index/com/garmin/geoçcaches/v0/index.xml
                file.WriteLine("</ggz>");
                file.Close();

                // 4- zip this directory
                // 5- rename it with the right filename
                // Nouvelle lib interne
                ZipFile.CreateFromDirectory((tmpDirectory + Path.DirectorySeparatorChar).Replace('\\','/'), fileRadix.Replace('\\','/'), CompressionLevel.Optimal, false, new MyEncoder());
                
                // Avec Ionic
                /*
                using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
                {
                    // add this map file into the "images" directory in the zip archive
                    zip.AddDirectory(tmpDirectory + Path.DirectorySeparatorChar + "data", "data");
                    zip.AddDirectory(tmpDirectory + Path.DirectorySeparatorChar + "index", "index");
                    zip.Save(fileRadix);
                }
                */
               
                // 6- Cleanup
                MyTools.DeleteDirectory(tmpDirectory, true);
                KillThreadProgressBar();

                return fileRadix;
            }
            catch (Exception exc2)
            {
                KillThreadProgressBar();
                throw exc2;
            }
        }

        /// <summary>
        /// Utilisé pour corriger le problème de ZipFile qui ne respecte pas la norme en mettrant des \\ au lieu de / dans les fichiers !
        /// </summary>
        class MyEncoder : UTF8Encoding
		{
		    public MyEncoder() : base(true)
		    {
		
		    }
		    public override byte[] GetBytes(string s)
		    {
		        s = s.Replace("\\", "/");
		        return base.GetBytes(s);
		   }
		}
        
        private String ExportCSV(SaveFileDialog saveFileDialog1, bool selection)
        {
            try
            {
                String radix = ExportCSV(saveFileDialog1.FileName, selection);
                MsgActionOk(this, GetTranslator().GetString("MBExportDone"));
                return radix;
            }
            catch (Exception exc2)
            {
                ShowException("", "Exporting CSV", exc2);
            }
            return "";
        }

        /// <summary>
        /// Export caches into a CSV file
        /// CSV columns are the one currently displayed in MGM
        /// </summary>
        /// <param name="thefilename">target file name</param>
        /// <param name="selection">if true, only selected caches are exported</param>
        /// <returns>export file name</returns>
        public String ExportCSV(String thefilename, bool selection)
        {
            try
            {
                _ThreadProgressBarTitle = "CSV Export";
                CreateThreadProgressBar();

                FileInfo fi = new FileInfo(thefilename);
                Directory.SetCurrentDirectory(fi.Directory.ToString());
                String fileRadix = fi.Name.ToString();

                System.IO.StreamWriter file = new System.IO.StreamWriter(fileRadix, false, Encoding.Default);
                
                // All the columns in the list and their displayed index
                Dictionary<int, string> listColumns = new Dictionary<int, string>();
                // All the displayed index : we will export in this order
                // Only non hiden columns will be added to the list
                AddColumnToListExport(listColumns, "LVCode");
                AddColumnToListExport(listColumns, "LVType");
                AddColumnToListExport(listColumns, "LVName");
                AddColumnToListExport(listColumns, "LVContainer");
                AddColumnToListExport(listColumns, "LVDifficulty");
                AddColumnToListExport(listColumns, "LVTerrain");
                AddColumnToListExport(listColumns, "LVDistance");
                AddColumnToListExport(listColumns, "LVPlaced");
                AddColumnToListExport(listColumns, "LVLastlog");
                AddColumnToListExport(listColumns, "LVTBGC");
                AddColumnToListExport(listColumns, "LVAvailable");
                AddColumnToListExport(listColumns, "LVAttributes");
                AddColumnToListExport(listColumns, "LVHint");
                AddColumnToListExport(listColumns, "LVTag");
                AddColumnToListExport(listColumns, "LVFavs");
                AddColumnToListExport(listColumns, "LVRating");
                AddColumnToListExport(listColumns, "LVOwner");
                AddColumnToListExport(listColumns, "LVAlti");
                AddColumnToListExport(listColumns, "LVFoundDNF");
                
                int nbcols = listColumns.Count;
                int[] columnsindex = new int[nbcols];
                string[] columnscode = new string[nbcols];

                List<int> li = listColumns.Keys.ToList<int>();
                li.Sort();
                int j = 0;
                foreach (int k in li)
                {
                    columnsindex[j] = k;
                    columnscode[j] = listColumns[k];
                    j++;
                }

                // Write headers
                String s = "";
                for (j = 0; j < nbcols; j++)
                {
                    s += "\"" + (GetTranslator().GetString(columnscode[j])).Replace("\"","\"\"") + "\";";
                    if (columnscode[j] == "LVName")
                    {
                        s += "\"" + (GetTranslator().GetString("LblLatitude")).Replace("\"", "\"\"") + "\";"; // DD.DDDD
                        s += "\"" + (GetTranslator().GetString("LblLongitude")).Replace("\"", "\"\"") + "\";";// DD.DDDD
                        s += "\"" + (GetTranslator().GetString("LblLatitude")).Replace("\"", "\"\"") + "\";"; // DD° MM.MMM
                        s += "\"" + (GetTranslator().GetString("LblLongitude")).Replace("\"", "\"\"") + "\";";// DD° MM.MMM
                    }
                }
                file.WriteLine(s);

                // Now write values
                int index = 1;
                
                ICollection collection = null;
                if (selection)
                    collection = lvGeocaches.SelectedItems;
                else
                    collection = lvGeocaches.Items;
                foreach (Object elt in collection)
                {
                    EXListViewItem lstvItem = elt as EXListViewItem;
                    String code = ReturnListEltValue(lstvItem, 0);
                    Geocache cache = _caches[code];
                    s = "";

                    // Go through all the columns
                    for (j = 0; j < nbcols; j++)
                    {
                        s += GetExportLineCSVValue(lstvItem, cache, columnscode[j]);
                    }
                    file.WriteLine(s);
                  
                    index++;
                }
                file.Close();

                KillThreadProgressBar();

                return fileRadix;
            }
            catch (Exception exc)
            {
                KillThreadProgressBar();
                throw exc;
            }
        }

        private String ExportSQL(SaveFileDialog saveFileDialog1, bool selection)
        {
            try
            {
            	bool striphtml = false;
            	DialogResult dialogResult = MessageBox.Show(GetTranslator().GetStringM("AskDeHTMLSQL"),
                        GetTranslator().GetString("AskDeHTMLSQLTitle"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            	if (dialogResult == DialogResult.Yes)
                {
                    striphtml = true;
                }
                String radix = ExportSQL(saveFileDialog1.FileName, selection, striphtml);
                MsgActionOk(this, GetTranslator().GetString("MBExportDone"));
                return radix;
            }
            catch (Exception exc2)
            {
                ShowException("", "Exporting SQL", exc2);
            }
            return "";
        }
        
        private String ExportOV2(SaveFileDialog saveFileDialog1, bool selection)
        {
            try
            {
                String radix = ExportOV2(saveFileDialog1.FileName, selection);
                MsgActionOk(this, GetTranslator().GetString("MBExportDone"));
                return radix;
            }
            catch (Exception exc2)
            {
                ShowException("", "Exporting OV2", exc2);
            }
            return "";
        }

        /// <summary>
        /// Export caches in OV2 (TomTom) format
        /// </summary>
        /// <param name="thefilename">export file name</param>
        /// <param name="selection">if true, only selected caches are exported</param>
        /// <returns>export file name</returns>
        public String ExportOV2(String thefilename, bool selection)
        {
            try
            {
                _ThreadProgressBarTitle = "OV2 Export";
                CreateThreadProgressBar();

                FileInfo fi = new FileInfo(thefilename);
                Directory.SetCurrentDirectory(fi.Directory.ToString());
                String fileRadix = fi.Name.ToString();
                BinaryWriter ov2File = new BinaryWriter(File.Open(fileRadix, FileMode.Create)); 
                
                // Récupération des caches
                ICollection collection = null;
                if (selection)
                    collection = lvGeocaches.SelectedItems;
                else
                    collection = lvGeocaches.Items;
                foreach (Object elt in collection)
                {
                    EXListViewItem lstvItem = elt as EXListViewItem;
                    String code = ReturnListEltValue(lstvItem, 0);
                    Geocache geo = _caches[code];
                    String name = "[" + geo._Code + "] " + geo._Name + " (by " + geo._Owner + ") (" + geo._D + "/" + geo._T + ") [" + geo._Container + "]";

                    int length = name.Length + 14;
                    byte[] ov2Data = new byte[length];
                    byte[] buffer = BitConverter.GetBytes('2');
                    //ov2Data[0] = buffer[0]; 
                    ov2Data[0] = 2;
                    buffer = BitConverter.GetBytes((long)length);
                    Array.Copy(buffer, 0, ov2Data, 1, 4);
                    buffer = BitConverter.GetBytes((long)(geo._dLongitude * 100000));
                    Array.Copy(buffer, 0, ov2Data, 5, 4);
                    buffer = BitConverter.GetBytes((long)(geo._dLatitude) * 100000);
                    Array.Copy(buffer, 0, ov2Data, 9, 4);
                    System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                    buffer = encoding.GetBytes(name);
                    Array.Copy(buffer, 0, ov2Data, 13, name.Length);
                    ov2File.Write(ov2Data);
                    
                }
                ov2File.Close();

                KillThreadProgressBar();

                return fileRadix;
            }
            catch (Exception exc)
            {
                KillThreadProgressBar();
                throw (exc);
            }
        }

        /// <summary>
        /// Export caches in SQLite format
        /// </summary>
        /// <param name="thefilename">export file name</param>
        /// <param name="selection">if true, only selected caches are exported</param>
        /// <param name="striphtml">if true, remove HTML tags (smaller DB, longer process)</param>
        /// <returns>export file name</returns>
        public String ExportSQL(String thefilename, bool selection, bool striphtml)
        {
            try
            {
                _ThreadProgressBarTitle = "SQL Export";
                CreateThreadProgressBar();

                 // Récupération des caches
                List<Geocache> collection = null;
                if (selection)
                	collection = GetSelectedCaches();
                else
                	collection = GetDisplayedCaches();
                
                if (File.Exists(thefilename))
                	File.Delete(thefilename);
                
                MGMDataBase dbmgm = new MGMDataBase(this, thefilename, true);
                
                HtmlAgilityPack.HtmlDocument doc = null;
				if (striphtml)
					doc = new HtmlAgilityPack.HtmlDocument();
			
				MGMDataBase.DBinfo dbi = new MGMDataBase.DBinfo();
                dbmgm.InsertGeocaches(collection, striphtml, doc, ref dbi);

                KillThreadProgressBar();

                FileInfo fi = new FileInfo(thefilename);
                String fileRadix = fi.Name.ToString();
                return fileRadix;
            }
            catch (Exception exc)
            {
                KillThreadProgressBar();
                throw (exc);
            }
        }
        
        /// <summary>
        /// Export caches into a GPX
        /// </summary>
        /// <param name="filename">export file name</param>
        /// <param name="cachestoprocess">list of caches to export</param>
        /// <param name="bSilent">if true, silent mode</param>
        /// <returns>export file name</returns>
        public String ExportGPXFromList(String filename, List<Geocache> cachestoprocess, bool bSilent)
        {
            FileInfo fi = new FileInfo(filename);
            Directory.SetCurrentDirectory(fi.Directory.ToString());
            String fileRadix = fi.Name.ToString();

            try
            {
                bool bLimit = false;
                int iNbCachesPerGPXFile = 1000;
                List<ParameterObject> lst = new List<ParameterObject>();
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Label, "", "totalnb", GetTranslator().GetString("LblTotalNbCaches") + " = " + cachestoprocess.Count.ToString()));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "limit", GetTranslator().GetString("LblLimitCachesPerGPX")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Int, iNbCachesPerGPXFile, "limitnum", GetTranslator().GetString("LblMaximumNumberCachesPerGPX")));

                ParametersChanger changer = new ParametersChanger();
                changer.Title = GetTranslator().GetString("LblLimitCachesPerGPX");
                changer.BtnCancel = GetTranslator().GetString("BtnCancel");
                changer.BtnOK = GetTranslator().GetString("BtnOk");
                changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
                changer.ErrorTitle = GetTranslator().GetString("Error");
                changer.Parameters = lst;
                changer.Font = this.Font;
                changer.Icon = this.Icon;

                if (changer.ShowDialog() == DialogResult.OK)
                {
                    bLimit = (lst[1].Value == "True");
                    iNbCachesPerGPXFile = Int16.Parse(lst[2].Value);
                }

                _ThreadProgressBarTitle = "GPX Export";
                CreateThreadProgressBar();

                if (bLimit)
                {
                    // Split files
                    String fileRadixNoExtension = fileRadix.Substring(0, fileRadix.Length - fi.Extension.Length);

                    List<Geocache> caches = new List<Geocache>();
                    int inum = 0;
                    int iiter = 1;
                    String gpxfile = fileRadixNoExtension + "_{0}.gpx";
                    foreach (Geocache geo in cachestoprocess)
                    {
                        caches.Add(geo);
                        inum++;

                        if (inum == iNbCachesPerGPXFile)
                        {
                            String fgpx = String.Format(gpxfile, iiter);
                            ExportGPXStreamed(fgpx, caches, null);
                            inum = 0;
                            iiter++;
                            caches.Clear();
                        }
                    }
                    if (caches.Count != 0)
                    {
                        String fgpx = String.Format(gpxfile, iiter);
                        ExportGPXStreamed(fgpx, caches, null);
                        caches.Clear();
                    }

                    fileRadix = gpxfile;
                }
                else
                {
                    // No split, nothing, just plain export...
                    ExportGPXStreamed(fileRadix, cachestoprocess, null);
                }
                KillThreadProgressBar();

                if (!bSilent)
                {
                    MsgActionOk(this, GetTranslator().GetString("MBExportDone") + " : " + fi.Directory.ToString() + "\\" + fileRadix);
                }
            }
            catch (Exception exc)
            {
                KillThreadProgressBar();

                ShowException("", "Exporting GPX from a list", exc);
            }


            return fileRadix;
        }

        /// <summary>
        /// Export caches into a GPX
        /// </summary>
        /// <param name="saveFileDialog1">target save file dialog</param>
        /// <param name="selection">if true, only selected caches are exported</param>
        /// <returns>export file name</returns>
        public String ExportGPX(SaveFileDialog saveFileDialog1, bool selection)
        {
            FileInfo fi = new FileInfo(saveFileDialog1.FileName);
            Directory.SetCurrentDirectory(fi.Directory.ToString());
            String fileRadix = fi.Name.ToString();

            try
            {
                bool bLimit = false;
                int iNbCachesPerGPXFile = 1000;
                List<ParameterObject> lst = new List<ParameterObject>();
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "limit", GetTranslator().GetString("LblLimitCachesPerGPX")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Int, iNbCachesPerGPXFile, "limitnum", GetTranslator().GetString("LblMaximumNumberCachesPerGPX")));

                ParametersChanger changer = new ParametersChanger();
                changer.Title = GetTranslator().GetString("LblLimitCachesPerGPX");
                changer.BtnCancel = GetTranslator().GetString("BtnCancel");
                changer.BtnOK = GetTranslator().GetString("BtnOk");
                changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
                changer.ErrorTitle = GetTranslator().GetString("Error");
                changer.Parameters = lst;
                changer.Font = this.Font;
                changer.Icon = this.Icon;

                if (changer.ShowDialog() == DialogResult.OK)
                {
                    bLimit = (lst[0].Value == "True");
                    iNbCachesPerGPXFile = Int16.Parse(lst[1].Value);
                }

                _ThreadProgressBarTitle = "GPX Export";
                CreateThreadProgressBar();

                if (bLimit)
                {
                    // Split files
                    String fileRadixNoExtension = fileRadix.Substring(0, fileRadix.Length - fi.Extension.Length);
                    
                    List<Geocache> caches = new List<Geocache>();
                    int inum = 0;
                    int iiter = 1;
                    String gpxfile = fileRadixNoExtension + "_{0}.gpx";
                    List<Geocache> cachestoprocess = null;
                    if (selection)
                        cachestoprocess = GetSelectedCaches();
                    else
                        cachestoprocess = GetDisplayedCaches();
                    foreach (Geocache geo in cachestoprocess)
                    {
                        caches.Add(geo);
                        inum++;

                        if (inum == iNbCachesPerGPXFile)
                        {
                            String fgpx = String.Format(gpxfile, iiter);
                            ExportGPXStreamed(fgpx, caches, null);
                            inum = 0;
                            iiter++;
                            caches.Clear();
                        }
                    }
                    if (caches.Count != 0)
                    {
                        String fgpx = String.Format(gpxfile, iiter);
                        ExportGPXStreamed(fgpx, caches, null);
                        caches.Clear();
                    }
                }
                else
                {
                    // No split, nothing, just plain export...
                    ExportGPXBrutal(selection, fileRadix, null, false, false);
                }
                KillThreadProgressBar();

                MsgActionOk(this, GetTranslator().GetString("MBExportDone"));
            }
            catch (Exception exc)
            {
                KillThreadProgressBar();

                ShowException("", "Exporting GPX", exc);
            }
            

            return fileRadix;
        }

        private String ExportGPXInternalProcessing(Geocache geo, String chunk, bool bOnlySpoilers, bool bUseSpoilerKeywords)
        {
            String output = chunk;
            List<String> keywordsspoiler = null;
            if (bUseSpoilerKeywords)
                keywordsspoiler = GetSpoilerKeyWordsAlways();
            if (_od._OfflineData.ContainsKey(geo._Code))
            {
                OfflineCacheData ocd = _od._OfflineData[geo._Code];
                if ((ocd._ImageFilesSpoilers.Count != 0) || (ocd._ImageFilesFromDescription.Count != 0))
                {
                    string offdatapath = GetUserDataPath() + Path.DirectorySeparatorChar + "Offline" + Path.DirectorySeparatorChar;
                    int ipic = 0;
                    String fjpeg = "";
                    if (ocd._ImageFilesSpoilers.Count != 0)
                    {
                        String spoilersImg = "";
                        foreach (KeyValuePair<string, OfflineImageWeb> paire in ocd._ImageFilesSpoilers)
                        {
                            try
                            {
                                bool bKeep = true;
                                if (bOnlySpoilers && bUseSpoilerKeywords)
                                    bKeep = IsAValidSpoiler(keywordsspoiler, paire.Value._name);

                                // Load image
                                if (bKeep)
                                {
                                    fjpeg = geo._Code + "_" + String.Format("{0:000}", ipic) + ".jpg";
                                    Image img = Image.FromFile(offdatapath + paire.Value._localfile);
                                    img.Save(fjpeg, System.Drawing.Imaging.ImageFormat.Jpeg);
                                    img.Dispose();

                                    String imgsrc = "&lt;br&gt;&lt;li&gt;" + MyTools.HtmlToXml(paire.Value._name) + "&lt;br&gt;&lt;img src=\"" + fjpeg + "\"&gt;&lt;/li&gt;";
                                    spoilersImg += imgsrc;

                                    ipic++;
                                }
                            }
                            catch (Exception)
                            {
                                // Do nothing
                            }
                        }

                        output = output.Replace("</groundspeak:long_description>", spoilersImg + "</groundspeak:long_description>");
                    }

                    if ((!bOnlySpoilers) && (ocd._ImageFilesFromDescription.Count != 0))
                    {
                        foreach (KeyValuePair<string, string> paire in ocd._ImageFilesFromDescription)
                        {
                            try
                            {
                                // Load image
                                fjpeg = geo._Code + "_" + String.Format("{0:000}", ipic) + ".jpg";
                                Image img = Image.FromFile(offdatapath + paire.Value);
                                img.Save(fjpeg, System.Drawing.Imaging.ImageFormat.Jpeg);
                                img.Dispose();

                                // Save to Jpeg
                                output = output.Replace(paire.Key, fjpeg);

                                ipic++;
                            }
                            catch (Exception)
                            {
                                // Do nothing
                            }
                        }
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Export all caches into a GPX
        /// </summary>
        /// <param name="fileRadix">target export file</param>
        public void ExportGPXBrutal(String fileRadix)
        {
        	ExportGPXBrutal(false, fileRadix, null, false, false);
        }

        /// <summary>
        /// Export caches into a GPX
        /// </summary>
        /// <param name="selection">if true, only selected caches are exported</param>
        /// <param name="fileRadix">target file for export</param>
        /// <param name="fGPXInternalProcessing">function for internal processing</param>
        /// <param name="bOnlySpoilers">if true, only spoilers images are exported</param>
        /// <param name="bUseSpoilerKeywords">if true, spoiler keywords are used</param>
        public void ExportGPXBrutal(bool selection, String fileRadix, Func<Geocache, String, bool, bool, String> fGPXInternalProcessing, bool bOnlySpoilers, bool bUseSpoilerKeywords)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(fileRadix, false);

            file.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            file.WriteLine("<gpx xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:ox=\"http://www.opencaching.com/xmlschemas/opencaching/1/0\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" version=\"1.0\" creator=\"Groundspeak, Inc. All Rights Reserved. http://www.groundspeak.com\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/0 http://www.topografix.com/GPX/1/0/gpx.xsd http://www.groundspeak.com/cache/1/0/1 http://www.groundspeak.com/cache/1/0/1/cache.xsd\" xmlns=\"http://www.topografix.com/GPX/1/0\">");
            file.WriteLine("  <name>Cache Listing Generated from Geocaching.com</name>");
            file.WriteLine("  <desc>This is an individual cache generated from Geocaching.com</desc>");
            file.WriteLine("  <author>Account \"" + ConfigurationManager.AppSettings["owner"] + "\" From Geocaching.com</author>");
            file.WriteLine("  <email>contact@geocaching.com</email>");
            file.WriteLine("  <url>http://www.geocaching.com</url>");
            file.WriteLine("  <urlname>Geocaching - My Geocaching Manager Export " + DateTime.Now.ToLongDateString() + "</urlname>");
            // 2011-09-18T07:00:00Z
            String date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
            file.WriteLine("  <time>" + date + "</time>");
            file.WriteLine("  <keywords>cache, geocache</keywords>");

            // +MODIFICATION OPTIMIZATION
            // Pas d'écriture des bounds cela permet d'écrire par bloc : plus rapide, moins de mémoire
            /*double minlat = 90.0;
            //double minlon = 180.0;
            //double maxlat = -90.0;
            double maxlon = -180.0;*/
            // -MODIFICATION OPTIMIZATION

            // +MODIFICATION OPTIMIZATION
            //String bigtext = "";
            // -MODIFICATION OPTIMIZATION
            
            List<Geocache> caches = null;
            if (selection)
                caches = GetSelectedCaches();
            else
                caches = GetDisplayedCaches();
            foreach (Geocache geo in caches)
            {
                // +MODIFICATION OPTIMIZATION
                /*if (geo._dLatitude < minlat)
                    minlat = geo._dLatitude;
                if (geo._dLongitude < minlon)
                    minlon = geo._dLongitude;
                if (geo._dLatitude > maxlat)
                    maxlat = geo._dLatitude;
                if (geo._dLongitude > maxlon)
                    maxlon = geo._dLongitude;*/
                // -MODIFICATION OPTIMIZATION

                int iLengthWaypointBloc = 0;
                String chunk = geo.ToGPXChunk(true, ref iLengthWaypointBloc/*, null*/);

                if (fGPXInternalProcessing != null)
                {
                    chunk = fGPXInternalProcessing(geo, chunk, bOnlySpoilers, bUseSpoilerKeywords);
                }

                // +MODIFICATION OPTIMIZATION
                //bigtext += chunk;
                file.Write(chunk);
                // -MODIFICATION OPTIMIZATION
            
                // remove hasbeenmodified flag
                if (geo.HasBeenModified)
                {
                    geo._Modifications.Clear();
                    _iNbModifiedCaches--;
                }
            }
            //   <bounds minlat="-12.03455" minlon="-77.017917" maxlat="-12.03455" maxlon="-77.017917" />
            // +MODIFICATION OPTIMIZATION
            /*String bounds = "";
            bounds = "  <bounds minlat=\"" + minlat
                + "\" minlon=\"" + minlon
                + "\" maxlat=\"" + maxlat
                + "\" maxlon=\"" + maxlon + "\" />";
            bounds = bounds.Replace(",", ".");
            file.WriteLine(bounds);*/
            // -MODIFICATION OPTIMIZATION

            // +MODIFICATION OPTIMIZATION
            //file.Write(bigtext);
            // -MODIFICATION OPTIMIZATION

            file.WriteLine("</gpx>");

            file.Close();
        }

        /// <summary>
        /// Export caches to GPX (streamed method)
        /// </summary>
        /// <param name="fileRadix">target file</param>
        /// <param name="caches">list of caches to export</param>
        /// <param name="fileIndex">file index (used for GGZ export)</param>
        public void ExportGPXStreamed(String fileRadix, List<Geocache> caches, System.IO.StreamWriter fileIndex)
        {
            ExportGPXStreamed(fileRadix, caches, fileIndex, DateTime.Now);
        }

        /// <summary>
        /// Export caches to GPX (streamed method)
        /// </summary>
        /// <param name="fileRadix">target file</param>
        /// <param name="caches">list of caches to export</param>
        /// <param name="fileIndex">file index (used for GGZ export)</param>
        /// <param name="dateExport">export date</param>
        public void ExportGPXStreamed(String fileRadix, List<Geocache> caches, System.IO.StreamWriter fileIndex, DateTime dateExport)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(fileRadix, false);
            String ss = "";
            ss += "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine;
            ss += "<gpx xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:ox=\"http://www.opencaching.com/xmlschemas/opencaching/1/0\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" version=\"1.0\" creator=\"Groundspeak, Inc. All Rights Reserved. http://www.groundspeak.com\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/0 http://www.topografix.com/GPX/1/0/gpx.xsd http://www.groundspeak.com/cache/1/0/1 http://www.groundspeak.com/cache/1/0/1/cache.xsd\" xmlns=\"http://www.topografix.com/GPX/1/0\">" + Environment.NewLine;
            ss += "  <name>Cache Listing Generated from Geocaching.com</name>" + Environment.NewLine;
            ss += "  <desc>This is an individual cache generated from Geocaching.com</desc>" + Environment.NewLine;
            ss += "  <author>Account \"" + ConfigurationManager.AppSettings["owner"] + "\" From Geocaching.com</author>" + Environment.NewLine;
            ss += "  <email>contact@geocaching.com</email>" + Environment.NewLine;
            ss += "  <url>http://www.geocaching.com</url>" + Environment.NewLine;
            ss += "  <urlname>Geocaching - My Geocaching Manager Export " + DateTime.Now.ToLongDateString() + "</urlname>" + Environment.NewLine;
            // 2011-09-18T07:00:00Z
            String date = dateExport.ToString("yyyy-MM-ddTHH:mm:ssZ");
            ss += "  <time>" + date + "</time>" + Environment.NewLine;
            ss += "  <keywords>cache, geocache</keywords>" + Environment.NewLine;
            file.Write(ss);

            // Current length (i.e. start of first wpt)
            long ideb = Encoding.UTF8.GetByteCount(ss);
            String twospaces = "  ";
            String linefeed = "\r\n";
            long to_remove = Encoding.UTF8.GetByteCount(linefeed + twospaces);
            ideb += Encoding.UTF8.GetByteCount(twospaces); // Add 2 spaces on the left of the first "<wpt" tag

            // +MODIFICATION OPTIMIZATION
            //String bigtext = "";
            // -MODIFICATION OPTIMIZATION

            String indexString = "";
            foreach (Geocache geo in caches)
            {
               
                /*
                List<long> lstOffsetLengthWaypoints = null;
                if (fileIndex != null)
                    lstOffsetLengthWaypoints = new List<long>();*/
                int iLengthWaypointBloc = 0;
                String chunk = geo.ToGPXChunk(true, ref iLengthWaypointBloc/*, lstOffsetLengthWaypoints*/); // false avant ! Attention, utilisé par les exports GPX par paquet aussi !!!!
                
                // Ok complete index file
                long ilen = Encoding.UTF8.GetByteCount(chunk);

                // Create indexchunk
                String indexChunk = "";
                if (fileIndex != null)
                {
                    indexChunk += "        <gch>" + Environment.NewLine;
                    indexChunk += "            <code>" + geo._Code + "</code>" + Environment.NewLine;
                    indexChunk += "            <name>" + MyTools.HtmlToXml(geo._Name) + "</name>" + Environment.NewLine;
                    indexChunk += "            <type>" + geo._Type + "</type>" + Environment.NewLine;
                    indexChunk += "            <lat>" + geo._Latitude + "</lat>" + Environment.NewLine;
                    indexChunk += "            <lon>" + geo._Longitude + "</lon>" + Environment.NewLine;
                    indexChunk += "            <file_pos>" + ideb.ToString() + "</file_pos>" + Environment.NewLine;

                    // ATTENTION : la longueur du bloc ne doit être que celle de la cache, sans tenir compte du bloc de waypoints qui
                    // se trouve juste après :
                    // (ilen - to_remove - iLengthWaypointBloc)
                    indexChunk += "            <file_len>" + (ilen - to_remove - iLengthWaypointBloc).ToString() + "</file_len>" + Environment.NewLine;
                    indexChunk += "            <ratings>" + Environment.NewLine;

                    String ratingforindex = "";
                
                    // convert what we can, this is a pure geocache
                    // Atlas Cached: no need to fill this one for GC
                    //indexChunk += "                <awesomeness>0.0</awesomeness>" + Environment.NewLine;

                    ratingforindex += "                <difficulty>" + geo._D + "</difficulty>" + Environment.NewLine;
                    String asize = _geocachingConstants.ConvertContainerToOpenCachingValues(geo._Container);
                    if (asize != "")
                        ratingforindex += "                <size>" + asize + "</size>" + Environment.NewLine;
                    ratingforindex += "                <terrain>" + geo._T + "</terrain>" + Environment.NewLine;
                    ratingforindex += "            </ratings>" + Environment.NewLine;
                    ratingforindex += "        </gch>" + Environment.NewLine;
                
                    // On ajoute les ratings
                    indexChunk += ratingforindex;

                    // Les waypoints
                    // Finalement on ne les stocke pas dans l'index
                    // on fait comme GSAK, keep it simple
                    // Append indexchunk
                    // +MODIFICATION OPTIMIZATION
                    indexString += indexChunk;
                    // -MODIFICATION OPTIMIZATION
                }
            
                // Add chunk
                // +MODIFICATION OPTIMIZATION
                //bigtext += chunk;
                file.Write(chunk);
                // -MODIFICATION OPTIMIZATION

                // set new ideb
                ideb += ilen;

                // remove hasbeenmodified flag
                if (geo.HasBeenModified)
                {
                    geo._Modifications.Clear();
                    _iNbModifiedCaches--;
                }
            }

            // +MODIFICATION OPTIMIZATION
            //file.Write(bigtext);
            // -MODIFICATION OPTIMIZATION
            
            file.WriteLine("</gpx>");

            file.Close();

            // +MODIFICATION OPTIMIZATION
            if (fileIndex != null)
            {
                // Compute CRC
                fileIndex.WriteLine("        <crc>" + CRC32.GetCRC(fileRadix).ToUpper() + "</crc>"); // Cette saleté de CRC a besoin du fichier final :-(

                //Write date
                date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
                fileIndex.WriteLine("        <time>" + date + "</time>");

                // Write remaining index file
                fileIndex.Write(indexString);
            }
            // -MODIFICATION OPTIMIZATION
        }

        private string ExportHTML(bool selection, bool bWithDetails, bool bWithMap, SaveFileDialog saveFileDialog1)
        {
            try
            {
                if (IsPathOnRemovableDrive(saveFileDialog1.FileName))
                {
                    DialogResult dialogResult = MessageBox.Show(GetTranslator().GetString("MsgRemovableDrive"),
                            GetTranslator().GetString("AskConfirm"),
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dialogResult != DialogResult.Yes)
                    {
                        return "";
                    }
                }

                _ThreadProgressBarTitle = "HTML Export";
                CreateThreadProgressBar();

                FileInfo fi = new FileInfo(saveFileDialog1.FileName);
                Directory.SetCurrentDirectory(fi.Directory.ToString());
                String fileRadix = fi.Name.ToString();
                String imgPath = Path.GetFileNameWithoutExtension(fi.Name);

                // First the images...
                System.IO.Directory.CreateDirectory(imgPath);

                // Write all images in this folder
                foreach (KeyValuePair<String, int> paire in _indexImages)
                {
                    _listImagesSized[paire.Value].Save(imgPath + Path.DirectorySeparatorChar + paire.Key + ".png", System.Drawing.Imaging.ImageFormat.Png);
                }

                System.IO.StreamWriter file = new System.IO.StreamWriter(fileRadix, false, Encoding.Default);
                file.Write("<html>\r\n");
                file.Write("<head>\r\n");
                file.Write("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=iso-8859-1\" />\r\n");
                if (bWithDetails)
                {
                    String script = @"
<script>
function visibilite(thingId, iForce)
{
	var targetElement;
	targetElement = document.getElementById('id_div_' + thingId) ;
	var oriLink;
	oriLink = document.getElementById('id_a_' + thingId) ;

	if (iForce == 0)
	{
		if (targetElement.style.display == 'none')
		{
			targetElement.style.display = '' ;
			if (oriLink != null)
				oriLink.innerHTML = '{0}';
		} 
		else if (targetElement.style.display == '')
		{
			targetElement.style.display = 'none' ;
			if (oriLink != null)
				oriLink.innerHTML = '{1}';
		}
	}
	else
	{
		if (iForce == 1)
		{
			targetElement.style.display = '' ;
			if (oriLink != null)
				oriLink.innerHTML = '{0}';
		} 
		else if (iForce == 2)
		{
			targetElement.style.display = 'none' ;
			if (oriLink != null)
				oriLink.innerHTML = '{1}';
		}
	}
}

function visibilite_all(iForce)
{";

                    script = script.Replace("{0}", GetTranslator().GetString("HtmlHideDesc"));
                    script = script.Replace("{1}", GetTranslator().GetString("HtmlShowDesc"));
                    file.Write(script);

                    List<Geocache> caches = GetDisplayedCaches();
                    // visibilite('GC3MBM2', iForce);
                    foreach (Geocache cache in caches)
                    {
                        file.Write("visibilite('" + cache._Code + "', iForce);\r\n");
                    }
                    file.Write(@"
}
</script>
");
                }

                file.Write("</head>\r\n");
                file.Write("<body>\r\n");
                file.Write(GetTranslator().GetString("HTMLFile") + ": " + fileRadix + "<br>\r\n");
                String date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
                file.Write((GetTranslator().GetString("HTMLCreationDate")) + ": " + date + "<br>\r\n");
                file.Write((GetTranslator().GetString("HTMLUser")) + ": " + ConfigurationManager.AppSettings["owner"] + "<br>\r\n");
                file.Write((GetTranslator().GetString("HTMLLatHome")) + ": " + _sHomeLat + "<br>\r\n");
                file.Write((GetTranslator().GetString("HTMLLonHome")) + ": " + _sHomeLon + "<br>\r\n");
                String url = "https://maps.google.com/maps?q=" + _sHomeLat + "+" + _sHomeLon;
                file.Write("<a href=" + url + ">Home location</a><br><br>\r\n");
                if (bWithDetails)
                {
                    file.Write("<a href=\"\" onclick=\"javascript:visibilite_all(1); return false;\" >" +
                        GetTranslator().GetString("HtmlShowAllDesc") + "</a>&nbsp;/&nbsp;\r\n");
                    file.Write("<a href=\"\" onclick=\"javascript:visibilite_all(2); return false;\" >" +
                        GetTranslator().GetString("HtmlHideAllDesc") + "</a><br>\r\n");
                }
                file.Write("<style type=\"text/css\">\r\n");
                file.Write(@"
tr.dh td {
	background-color: #66FF66; color: black;
}
tr.d0 td {
	background-color: #FFFFCC; color: black;
}
tr.d1 td {
	background-color: #F8F8F8; color: black;
}
.boldtable, .boldtable TD, .boldtable TH
{
font-family:sans-serif;
font-size:10pt;
}
</style>" + "\r\n");
                file.Write(@"<style>
@media print and (color) {
* { -webkit-print-color-adjust: exact; print-color-adjust: exact; }
     /* presentation rules for the page on color printers */
}
</style>" + "\r\n");

                file.Write("<table border=1 class=\"boldtable\" rules=COLS frame=BOX>\r\n");
                file.Write("  <tr class=\"dh\" align=center>\r\n");
                file.Write("    <td>#</td>\r\n");

                // All the columns in the list and their displayed index
                Dictionary<int, string> listColumns = new Dictionary<int, string>();
                // All the displayed index : we will export in this order
                // Only non hiden columns will be added to the list
                AddColumnToListExport(listColumns, "LVCode");
                AddColumnToListExport(listColumns, "LVType");
                AddColumnToListExport(listColumns, "LVName");
                AddColumnToListExport(listColumns, "LVContainer");
                AddColumnToListExport(listColumns, "LVDifficulty");
                AddColumnToListExport(listColumns, "LVTerrain");
                AddColumnToListExport(listColumns, "LVDistance");
                AddColumnToListExport(listColumns, "LVPlaced");
                AddColumnToListExport(listColumns, "LVLastlog");
                AddColumnToListExport(listColumns, "LVTBGC");
                AddColumnToListExport(listColumns, "LVAvailable");
                AddColumnToListExport(listColumns, "LVAttributes");
                AddColumnToListExport(listColumns, "LVHint");
                AddColumnToListExport(listColumns, "LVTag");
                AddColumnToListExport(listColumns, "LVFavs");
                AddColumnToListExport(listColumns, "LVRating");
                AddColumnToListExport(listColumns, "LVOwner");
                AddColumnToListExport(listColumns, "LVAlti");
                AddColumnToListExport(listColumns, "LVFoundDNF");

                int nbcols = listColumns.Count;
                int[] columnsindex = new int[nbcols];
                string[] columnscode = new string[nbcols];

                List<int> li = listColumns.Keys.ToList<int>();
                li.Sort();
                int j = 0;
                foreach (int k in li)
                {
                    columnsindex[j] = k;
                    columnscode[j] = listColumns[k];
                    j++;
                }

                // Write headers
                String s = "";
                for (j = 0; j < nbcols; j++)
                {
                    s += "    <td>" + (GetTranslator().GetString(columnscode[j])) + "</td>\r\n";
                    if (columnscode[j] == "LVName")
                        s += "    <td>" + (GetTranslator().GetString("HtmlCoord")) + "</td>\r\n";
                }
                file.Write(s);
                file.Write("  </tr>\r\n");

                // Now write values
                int index = 1;
                bool bcol0 = true;
                List<Geocache> theCaches = new List<Geocache>();


                ICollection collection = null;
                if (selection)
                    collection = lvGeocaches.SelectedItems;
                else
                    collection = lvGeocaches.Items;
                foreach (Object elt in collection)
                {
                    String tr = "  <tr class=\"d0\">\r\n";
                    if (!bcol0)
                        tr = "  <tr class=\"d1\">\r\n";
                    file.Write(tr);
                    bcol0 = !bcol0;

                    EXListViewItem lstvItem = elt as EXListViewItem;

                    String code = ReturnListEltValue(lstvItem, 0);
                    Geocache cache = _caches[code];
                    theCaches.Add(cache);
                    s = "    <td>" + index.ToString() + "</td>\r\n";

                    // Go through all the columns
                    for (j = 0; j < nbcols; j++)
                    {
                        //s += "    <td>?!?!?!</td>\r\n";
                        s += GetExportLineValue(imgPath, lstvItem, cache, columnscode[j]);
                    }
                    file.Write(s);
                    file.Write("  </tr>\r\n");

                    if (bWithDetails)
                    {
                        file.Write(tr);
                        file.Write("	<td colspan=\"15\">\r\n");
                        file.Write("<a href=\"\" id=\"id_a_" + cache._Code + "\" onclick=\"javascript:visibilite('" + cache._Code +
                            "', 0); return false;\" >" + GetTranslator().GetString("HtmlShowDesc") + "</a>\r\n");
                        file.Write("<div id=\"id_div_" + cache._Code + "\" style=\"display:none;\">\r\n");
                        s = "<br>" + cache._ShortDescriptionInHTML + "\r\n";
                        s += cache.ReturnLongDescriptionInHTMLWithWpts() + "<br></div>\r\n";
                        file.Write(s);
                        file.Write("    </td>\r\n");
                        file.Write("  </tr>\r\n");
                    }
                    index++;
                }
                file.Write("</table>\r\n");
                file.Write("</body>\r\n");
                file.Write("</html>\r\n");
                file.Close();

                KillThreadProgressBar();

                // Export Google Maps => N'EXISTE PLUS, on peut faire une capture d'écran c'est bien mieux !
                if (bWithMap)
                {
                    //fmap = saveFileDialog1.FileName.Replace(".html", ".png");
                    if (theCaches.Count != 0)
                    {
                        // On créé des marqueurs numérotés dans RESERVED2 par exemple
                        // On fait un screenshot
                        // On efface ce bordel
                        int i = 1;
                        ClearOverlay_RESERVED2();
                        //_cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2].Markers.Clear();
                        _cacheDetail._gmap.HoldInvalidation = true;
                        foreach (Geocache geo in theCaches)
                        {
                            // Le marker de numéro ?
                            GMapMarkerText mktxt = new GMapMarkerText(
                                new PointLatLng(geo._dLatitude, geo._dLongitude),
                                i.ToString(),
                                Color.White,
                                Color.Green,
                                0,
                                20,
                                false);
                            i++;
                            _cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2].Markers.Add(mktxt);
                        }
                        ShowCacheMapInCacheDetail();
                        _cacheDetail._gmap.Refresh();
                        _cacheDetail._gmap.ZoomAndCenterMarkers(_cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2].Id);
                    }
                }

                url = "file:///" + saveFileDialog1.FileName.Replace("\\", "/");
                if (ConfigurationManager.AppSettings["openGeocachingEmbedded"] == "True")
                    _cacheDetail.LoadPage("HTML Export (" + imgPath + ")", url);
                else
                {
                    //MyTools.StartInNewThread(url);
                    MyTools.StartInNewThread(saveFileDialog1.FileName);
                }
                
                return fileRadix;
            }
            catch (Exception exc)
            {
                KillThreadProgressBar();
                ShowException("", "Exporting HTML", exc);
            }
            return "";
        }
        
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private String ReturnListEltValue(EXListViewItem lstvItem, int iSubItem)
        {
            if (iSubItem == 0)
                return lstvItem.Text;
            else
            {
                EXMultipleImagesListViewSubItem subItem = (EXMultipleImagesListViewSubItem)(lstvItem.SubItems[iSubItem]);
                return subItem.MyValue;
            }
        }

        private string getImgSrcHTML(String path, String prefix)
        {
            return "<img src=\"" + path + Path.DirectorySeparatorChar + prefix + ".png\">";
        }
       
        private string getImgSrcHTML(String path, String prefix, int w, int h)
        {
            return "<img height=\"" + h + "\" width=\"" + w + "\" src=\"" + path + Path.DirectorySeparatorChar + prefix + ".png\">";
        }
        
        private bool IsColumnHiden(String code)
        {
            foreach (KeyValuePair<int, String> paire in _dicoColumns)
            {
                if (paire.Value == code)
                {
                    return (lvGeocaches.Columns[paire.Key] as EXColumnHeader)._bHidden ? true : false;
                }
            }

            // ????
            return false;
        }

        private int GetColumnIndex(String code)
        {
            foreach (KeyValuePair<int, String> paire in _dicoColumns)
            {
                if (paire.Value == code)
                {
                    return paire.Key;
                }
            }

            // ????
            return -1;
        }

        private int GetColumnDisplayIndex(String code)
        {
            foreach (KeyValuePair<int, String> paire in _dicoColumns)
            {
                if (paire.Value == code)
                {
                    return lvGeocaches.Columns[paire.Key].DisplayIndex;
                }
            }

            // ????
            return -1;
        }


        private string GetExportLineValue(String imgPath, EXListViewItem lstvItem, Geocache cache, String lbl)
        {
            String prefix = "";
            switch (lbl)
            {
                case "LVCode":
                    {
                        return "    <td><a href=" + cache._Url + ">" + cache._Code + "</a></td>\r\n";
                        
                    }
                case "LVOwner":
                    {
                        return "    <td>" + cache._Owner + "</td>\r\n";

                    }
                case "LVType":
                    {
                        return "    <td>" + getImgSrcHTML(imgPath, ReturnListEltValue(lstvItem, 2)) + "</td>\r\n";
                        
                    }
                case "LVHint":
                    {
                        if (cache._Hint != "")
                            return "    <td>" + cache._Hint + "</td>\r\n";
                        else
                            return "    <td>&nbsp;</td>\r\n";
                    }
                case "LVName":
                    {
            			if (cache.IsFound())
                            prefix = getImgSrcHTML(imgPath, "Found");
                        else if (cache._bOwned)
                            prefix = getImgSrcHTML(imgPath, "Owned");
                        else
                            prefix = "";
                        
                        // A note ?
                        string note = "";
                        EXImageListViewItem lvi = lstvItem as EXImageListViewItem;
                        if ((lvi != null) && ((lvi.MyImage == _imgNoteSpoiler) || (lvi.MyImage == _imgNoteNoSpoiler) || (lvi.MyImage == _imgNote)))
                        {
                            OfflineCacheData ocd = _od._OfflineData[cache._Code];
                            if (ocd._Comment != "")
                            {
                                note = "<br><u>" + GetTranslator().GetString("LblNote") + "</u>: " + ocd._Comment + "\r\n";
                            }
                        }

                        String s;
                        if (cache._Available.ToLower() == "true")
                            s = "    <td>" + prefix + (ReturnListEltValue(lstvItem, 3)) + note + "</td>\r\n";
                        else
                            s = "    <td>" + prefix + "<span style=\"text-decoration:line-through;\">" +
                                (ReturnListEltValue(lstvItem, 3)) + "</span>" + note + "</td>\r\n";

                        // Column coordinnates
                        // Suffix: coordinates
                        String coord = "<br>";
                        coord += CoordConvHMI.ConvertDegreesToDDMM(cache._dLatitude, true);
                        //coord += "<br>";
                        coord += "&nbsp;";
                        coord += CoordConvHMI.ConvertDegreesToDDMM(cache._dLongitude, false);
                        coord = coord.Replace("°", "&deg;");
                        s += "    <td>" + coord + "</td>\r\n";
                        return s;
                    }
                case "LVContainer":
                    {
                        String s = ReturnListEltValue(lstvItem, 4);
                        if (s.Contains("#"))
                            return "    <td>" + getImgSrcHTML(imgPath, _geocachingConstants.ConvertContainerToString(s.Replace("#",""))) + "</td>\r\n";
                        else
                            return "    <td>" + s.Replace("*","") + "</td>\r\n";
                        
                    }
                case "LVDifficulty":
                    {
                        String s = ReturnListEltValue(lstvItem, 5);
                        if (s.Contains("#"))
                            return "    <td>" + getImgSrcHTML(imgPath, s.Replace("#", "")) + "</td>\r\n";
                        else
                            return "    <td>" + s.Replace("*", "") + "</td>\r\n";
                        
                    }
                case "LVTerrain":
                    {
                        String s = ReturnListEltValue(lstvItem, 6);
                        if (s.Contains("#"))
                            return "    <td>" + getImgSrcHTML(imgPath, s.Replace("#", "")) + "</td>\r\n";
                        else
                            return "    <td>" + s.Replace("*", "") + "</td>\r\n";
                        
                    }
                case "LVFavs":
                    {
                        String s = ReturnListEltValue(lstvItem, 19);
                        if (s != "")
                        {
                            return "    <td>" + getImgSrcHTML(imgPath, "Fav") + s + "</td>\r\n";
                        }
                        else
                            return "    <td>&nbsp;</td>\r\n";

                    }
                case "LVRating":
                    {
                        String s = ReturnListEltValue(lstvItem, 20);
                        if ((s != "") && (s != "#"))
                        {
                            return "    <td>" + s + "</td>\r\n";
                        }
                        else
                            return "    <td>&nbsp;</td>\r\n";

                    }
                case "LVAlti":
                    {
                        String s = ReturnListEltValue(lstvItem, 21);
                        if (s != "")
                        {
                            return "    <td>" + s + "</td>\r\n";
                        }
                        else
                            return "    <td>&nbsp;</td>\r\n";

                    }
                case "LVFoundDNF":
                    {
                        String s = ReturnListEltValue(lstvItem, 22);
                        if (s != "")
                        {
                            return "    <td>" + s + "</td>\r\n";
                        }
                        else
                            return "    <td>&nbsp;</td>\r\n";

                    }
                case "LVTag":
                    {
                        String s = ReturnListEltValue(lstvItem, 18);
                        if (s != "")
                        {
                            return "    <td>" + s + "</td>\r\n";
                        }
                        else
                            return "    <td>&nbsp;</td>\r\n";
                    }
                case "LVDistance":
                    {
                        if (_bUseKm)
                            return "    <td>" + ReturnListEltValue(lstvItem, 7) + " " + GetTranslator().GetString("LVKm") + "</td>\r\n";
                        else
                            return "    <td>" + ReturnListEltValue(lstvItem, 7) + " " + GetTranslator().GetString("LVMi") + "</td>\r\n";
                        
                    }
                case "LVPlaced":
                    {
                        return "    <td>" + ReturnListEltValue(lstvItem, 8) + "</td>\r\n";
                        
                    }
                case "LVLastlog":
                    {
                        if (cache._Logs.Count != 0)
                            prefix = getImgSrcHTML(imgPath, cache._Logs[0]._Type);
                        else
                            prefix = "";
                        return "    <td>" + prefix + ReturnListEltValue(lstvItem, 9) + "</td>\r\n";
                        
                    }
                case "LVTBGC":
                    {
                        if (cache._listTB.Count != 0)
                            prefix = getImgSrcHTML(imgPath, "TBORGC") + " x" + ReturnListEltValue(lstvItem, 10);
                        else
                            prefix = "&nbsp;";
                        return "    <td>" + prefix + "</td>\r\n";
                        
                    }
                case "LVAvailable":
                    {
                        String val = "Enable Listing";
                        String myval = ReturnListEltValue(lstvItem, 11);
                        if (myval == "1")
                            val = "Temporarily Disable Listing";
                        else if (myval == "2")
                            val = "Archive";

                        return "    <td>" + getImgSrcHTML(imgPath, val) + "</td>\r\n";
                        
                    }
                case "LVAttributes":
                    {
                        String s = "    <td>\r\n";
                        String atttxt = "";
                        int n = 1;
                        if (cache._Attributes.Count != 0)
                        {
                            foreach (String att in cache._Attributes)
                            {
                                atttxt += GetTranslator().GetString(CreateAttributeTranslationKey(att)) + "; ";
                                s += "        " + getImgSrcHTML(imgPath, att.Replace("/", ""), 15, 15) + "\r\n";
                                if (n == 5)
                                {
                                    s += "<br>\r\n";
                                    n = 0;
                                }
                                n++;
                            }
                        }

                        if (atttxt == "")
                            s += "&nbsp;";
                        s += "    </td>\r\n";
                        return s;
                        
                    }
                default:
                    return "    <td>?!?!?!</td>\r\n";
                    
            }

        }

        private string GetExportLineCSVValue(EXListViewItem lstvItem, Geocache cache, String lbl)
        {
            String val = "";
            switch (lbl)
            {
                case "LVCode":
                    {
                        val = "\"" + cache._Code + "\";";
                        break;

                    }
                case "LVOwner":
                    {
                        val = "\"" + cache._Owner.Replace("\"", "\"\"") + "\";";
                        break;

                    }
                case "LVType":
                    {
                        val = "\"" + cache._Type + "\";";
                        break;

                    }
                case "LVHint":
                    {
                        val = "\"" + cache._Hint.Replace("\"","\"\"") + "\";";
                        break;
                    }
                case "LVName":
                    {
                        val = "\"" + cache._Name.Replace("\"", "\"\"") + "\";";
                        val += "\"" + cache._Latitude + "\";\"" + cache._Longitude + "\";";
                        val += "\"" + CoordConvHMI.ConvertDegreesToDDMM(cache._dLatitude, true) + "\";\"" + CoordConvHMI.ConvertDegreesToDDMM(cache._dLongitude, false) + "\";";
                        break;
                    }
                case "LVContainer":
                    {
                        val = "\"" + cache._Container + "\";";
                        break;
                    }
                case "LVDifficulty":
                    {
                        val = "\"" + cache._D + "\";";
                        break;

                    }
                case "LVTerrain":
                    {
                        val = "\"" + cache._T + "\";";
                        break;
                    }
                case "LVFavs":
                    {
                        OfflineCacheData ocd = cache._Ocd;
                        if ((ocd != null) && (ocd._iNbFavs != -1))
                        {
                            val = "\"" + ocd._iNbFavs + "\";";
                        }
                        else
                            val = "\"\";";
                        break;
                    }
                case "LVRating":
                    {
                        OfflineCacheData ocd = cache._Ocd;
                        if ((ocd != null) && (ocd._dRating != -1.0))
                        {
                            val = "\"" + ocd._dRating.ToString("0.0%") + "\";";
                        }
                        else
                            val = "\"\";";
                        break;
                    }
                case "LVFoundDNF":
                    {
                        OfflineCacheData ocd = cache._Ocd;
                        if ((ocd != null) && ((ocd._iNbFounds != -1.0) || (ocd._iNbNotFounds != -1.0)))
                        {
                            int nbfound = ocd._iNbFounds;
                            int nbdnf = ocd._iNbNotFounds;
                            if (nbdnf == -1)
                                nbdnf = 0;
                            if (nbfound == -1)
                                nbfound = 0;
                            int total = (nbfound + nbdnf);
                            String lbl2 = total.ToString() + " (" + nbfound.ToString() + "/" + nbdnf.ToString() + ")";
                            val = "\"" + lbl2 + "\";";
                        }
                        else
                            val = "\"\";";
                        break;
                    }
                case "LVAlti":
                    {
                        OfflineCacheData ocd = cache._Ocd;
                        if ((ocd != null) && (ocd._dAltiMeters != Double.MaxValue))
                        {
                            val = "\"" + String.Format("{0:0.#}", ocd._dAltiMeters).Replace(",", ".") + "\";";
                        }
                        else
                            val = "\"\";";
                        break;
                    }
                case "LVTag":
                    {
                        OfflineCacheData ocd = cache._Ocd;
                        if (ocd != null)
                        {
                            val = "\"" + ocd.GetTags() + "\";";
                        }
                        else
                            val = "\"\";";
                        break;
                    }
                case "LVDistance":
                    {
                        if (_bUseKm)
                            val = "\"" + cache.DistanceToHome().ToString().Replace(",",".") + " " + GetTranslator().GetString("LVKm") + "\";";
                        else
                            val = "\"" + cache.DistanceToHomeMi().ToString().Replace(",", ".") + " " + GetTranslator().GetString("LVMi") + "\";";
                        break;
                    }
                case "LVPlaced":
                    {
                        val = "\"" + cache._DateCreation + "\";";
                        break;
                    }
                case "LVLastlog":
                    {
                        if (cache._Logs.Count != 0)
                            val = "\"" +  cache._Logs[0]._Type + " " +  cache._Logs[0]._Date + "\";";
                        else
                            val = "\"\";";
                        break;
                    }
                case "LVTBGC":
                    {
                        val = "\"" + cache._listTB.Count.ToString() + "\";";
                        break;
                    }
                case "LVAvailable":
                    {
                        val = "\"" + cache._Available + "\";";
                        break;

                    }
                case "LVAttributes":
                    {
                        String atttxt = "";
                        if (cache._Attributes.Count != 0)
                        {
                            foreach (String att in cache._Attributes)
                            {
                                atttxt += GetTranslator().GetString(CreateAttributeTranslationKey(att)) + "; ";
                            }
                        }
                        
                        val = "\"" + atttxt.Replace("\"", "\"\"") + "\";";
                        break;

                    }
                default:
                    {
                        val = "???";
                        break;
                    }
            }
            return val;
        }

        private void AddColumnToListExport(Dictionary<int, string> listColumns, String code)
        {
            if (!IsColumnHiden(code))
                listColumns.Add(GetColumnDisplayIndex(code), code);
        }

        
        private void defaultsystemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateConfFile("forceLocale","");
            UpdateMenuChecks();
            ChangeLanguage();
        }

        private void langToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mnu = sender as ToolStripMenuItem;
            if (mnu != null)
            {
                String key = (String)(mnu.Tag);
                UpdateConfFile("forceLocale",key);
                UpdateMenuChecks();
                ChangeLanguage();
            }
        }

        private void openExternalURLInDefaultBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openExternalURLInDefaultBrowserToolStripMenuItem.Checked)
                UpdateConfFile("openGeocachingEmbedded", "True");
            else
                UpdateConfFile("openGeocachingEmbedded", "False");

            UpdateMenuChecks();
            
        }

        private void ignoreFoundCachesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ignoreFoundCachesToolStripMenuItem.Checked)
                UpdateConfFile("ignorefounds", "False");
            else
                UpdateConfFile("ignorefounds", "True");
            UpdateMenuChecks();

            _profileMgr.UpdateBasedOnMGMCurrentProfile(this);
            
            ReloadPreviouslyLoadedFiles();
        }

        /// <summary>
        /// Reload previously loaded GPX files
        /// </summary>
        public void ReloadPreviouslyLoadedFiles()
        {
            LoadBatchOfFilesImpl(_LoadedOriginalFiles.ToArray(), true, true, false); // On utilise le filtre
        }

        private string GetDecryptedOwnerPassword()
        {
            // On déchiffre le password s'il existe
            String password = ConfigurationManager.AppSettings["ownerpassword"];
            // Est-il déjà chiffré ?
            String password_decyph = "";
            if (password != "")
            {
                StringCipher.CustomDecrypt(password, ref password_decyph);
                // On a pu déchiffrer le password ça veut dire qu'il n'était pas déjà chiffré
            }
            return password_decyph;
        }

        private string GetDecryptedProxyPassword()
        {
            // On déchiffre le password s'il existe
            String password = ConfigurationManager.AppSettings["proxypassword"];
            // Est-il déjà chiffré ?
            String password_decyph = "";
            if (password != "")
            {
                StringCipher.CustomDecrypt(password, ref password_decyph);
                // On a pu déchiffrer le password ça veut dire qu'il n'était pas déjà chiffré
            }
            return password_decyph;
        }

        private void ChangeUserName()
        {
        	String oldlogin = ConfigurationManager.AppSettings["owner"];

            List<ParameterObject> lst = new List<ParameterObject>();
            lst.Add(new ParameterObject(ParameterObject.ParameterType.String, oldlogin, "login", GetTranslator().GetString("User")));
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Password, GetDecryptedOwnerPassword(), "passwd", GetTranslator().GetString("LblPassword")));

            ParametersChanger changer = new ParametersChanger();
            changer.Title = GetTranslator().GetString("FMenuChangeUser").Replace("&", "");
            changer.BtnCancel = GetTranslator().GetString("BtnCancel");
            changer.BtnOK = GetTranslator().GetString("BtnOk");
            changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
            changer.ErrorTitle = GetTranslator().GetString("Error");
            changer.Parameters = lst;
            changer.Font = this.Font;
            changer.Icon = this.Icon;

            if (changer.ShowDialog() == DialogResult.OK)
            {
                UpdateConfFile("owner", lst[0].Value);

                // password : on le chiffre et on l'écrit
                String password_cyph = "";
                if (lst[1].Value != "")
                    password_cyph = StringCipher.CustomEncrypt(lst[1].Value);
                UpdateConfFile("ownerpassword", password_cyph);

                UpdateHMIForGC();
				
                _profileMgr.UpdateBasedOnMGMCurrentProfile(this);
                
				// Verification du login / password s'ils sont ofurnis
				if ((lst[0].Value != "") && (lst[1].Value != ""))
					CheckGCAccount(false, true);
				
				// si le login a changé, on demande de redémarrer MGM... ou pas ;-)
				if (oldlogin != lst[0].Value)
                {
                    // On change le titre de la fenêtre
                    this.Text = "MGM - " + GetTranslator().GetString("User") + ": " + lst[0].Value;

                    // On va recharger les derniers fichiers chargés !
                    ReloadPreviouslyLoadedFiles();
                }
            }
        }
        
        private void changeUserNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
        	ChangeUserName();
        }

        private void ChangeHomeLocation()
        {
        	CloseCacheDetail();
        	List<ParameterObject> lst = new List<ParameterObject>();
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Coordinates/*good*/,
                _dHomeLat.ToString() + " " + _dHomeLon.ToString(),
                "latlon",
                GetTranslator().GetString("ParamHomeLatLon"),
                GetTranslator().GetStringM("TooltipParamLatLon")));
            ParametersChanger changer = new ParametersChanger();
            changer.HandlerDisplayCoord = HandlerToDisplayCoordinates;
            changer.DisplayCoordImage = _listImagesSized[getIndexImages("Earth")];
            changer.Title = GetTranslator().GetString("FMenuchangeHome").Replace("&", "");
            changer.BtnCancel = GetTranslator().GetString("BtnCancel");
            changer.BtnOK = GetTranslator().GetString("BtnOk");
            changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
            changer.ErrorTitle = GetTranslator().GetString("Error");
            changer.Parameters = lst;
            changer.Font = this.Font;
            changer.Icon = this.Icon;
 
            // Force creation du get handler on control
            changer.CreateControls();
            _cacheDetail._gmap.ControlTextLatLon = changer.CtrlCallbackCoordinates;

            if (changer.ShowDialog() == DialogResult.OK)
            {
                _cacheDetail._gmap.ControlTextLatLon = null;
                Double dlon = Double.MaxValue;
                Double dlat = Double.MaxValue;
                if (ParameterObject.SplitLongitudeLatitude(lst[0].Value, ref dlon, ref dlat))
                {
                    UpdateConfFile("mylocationlat", dlat.ToString().Replace(",", "."));
                    UpdateConfFile("mylocationlon", dlon.ToString().Replace(",", "."));

                    // Mise à jour des valeurs associées
                   	UpdateHomeInternalInformation();

                    _profileMgr.UpdateBasedOnMGMCurrentProfile(this);
                    
                    // On va recharger les derniers fichiers chargés !
                    ReloadPreviouslyLoadedFiles();

                    // Et on retracer les bookmarks (qui contiennent la maison)
                    UpdateBookmarkOverlay(GetBookmarks());
                }
                else
                {
                    // On ne devrait jamais rentrer ici
                    MsgActionError(this, GetTranslator().GetString("Error"));
                }
            }
            else
            	_cacheDetail._gmap.ControlTextLatLon = null;
        }
        
        private void changeHomeLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
        	ChangeHomeLocation();
        }

        /// <summary>
        /// Update internal information related to home position based on configuration file
        /// </summary>
        public void UpdateHomeInternalInformation()
        {
        	// Mise à jour des valeurs associées
            _dHomeLat = MyTools.ConvertToDouble(ConfigurationManager.AppSettings["mylocationlat"]);
            _dHomeLon = MyTools.ConvertToDouble(ConfigurationManager.AppSettings["mylocationlon"]);
            _sHomeLat = ConfigurationManager.AppSettings["mylocationlat"];
            _sHomeLon = ConfigurationManager.AppSettings["mylocationlon"];
        }
        
        private void changeAgeForNewCacheToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<ParameterObject> lst = new List<ParameterObject>();
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Int, 
                Int32.Parse(ConfigurationManager.AppSettings["daysfornew"]),
                "daysfornew",
                GetTranslator().GetString("FMenuAgeForNew")));

            ParametersChanger changer = new ParametersChanger();
            changer.Title = GetTranslator().GetString("FMenuAgeForNew");
            changer.BtnCancel = GetTranslator().GetString("BtnCancel");
            changer.BtnOK = GetTranslator().GetString("BtnOk");
            changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
            changer.ErrorTitle = GetTranslator().GetString("Error");
            changer.Parameters = lst;
            changer.Font = this.Font;
            changer.Icon = this.Icon;

            if (changer.ShowDialog() == DialogResult.OK)
            {
                UpdateConfFile("daysfornew", lst[0].Value);

                // On va recharger les derniers fichiers chargés !
                ReloadPreviouslyLoadedFiles();
            }
        }

        private void checkUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckUpdate(false, false);
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //progressBar.Value = e.ProgressPercentage;
        }

        private void UpdateMGMFromZipFile()
        {
        	try
            {
                Log("Unzipping udate");
                Directory.SetCurrentDirectory(GetInternalDataPath());
                string unpackDirectory = "Update";
                if (Directory.Exists(unpackDirectory))
                {
                    Log("Delete old dir: " + unpackDirectory);
                    MyTools.DeleteDirectory(unpackDirectory, true);
                }
                
                Log("Unzipping " + sDownloadedUpdateFile);
                try
	            {
	                ZipFile.ExtractToDirectory(sDownloadedUpdateFile, unpackDirectory);
	            }
	            catch (Exception exc1)
	            {
	            	Log("!!!! " + GetException("Unzipping", exc1));
	            	Log("!!!! Failed unzipping " + sDownloadedUpdateFile);
                    Log("Removing old dir: " + unpackDirectory);
                    MyTools.DeleteDirectory(unpackDirectory, true);
	            }
                // Delete zip file
                File.Delete(sDownloadedUpdateFile);

                DialogResult dialogResult = MyMessageBox.Show(GetTranslator().GetString("UpdateReadyToInstall"),
                    GetTranslator().GetString("WarTitle"),
                    MessageBoxIcon.Question, GetTranslator());
                if (dialogResult == DialogResult.Yes)
                {
                    FileInfo fi = new FileInfo(sDownloadedUpdateFile);
                    String shortname = fi.Name.Replace(fi.Extension, "");

                    // Check if we need to replace Updater.exe with the new version from the downloaded file
                    try
                    {
                        String of = "Updater.exe";
                        String nf = "Update" + Path.DirectorySeparatorChar + shortname + Path.DirectorySeparatorChar + "Updater.exe";
                        String old = MyTools.GetMD5(of);
                        Log("Current updater: " + old);
                        String niu = "";
                        if (File.Exists(nf))
                        {
                        	Log("Testing " + nf);
                        	niu = MyTools.GetMD5(nf);
                        }
                        else
                        {
                        	// Robustesse, on est sûrement une RC
	                    	// Check si on est une RC
		                    // 4.0.0.0.MyGeocachingManager
		                    // ou
		                    // 4.0.0.0.RC01.MyGeocachingManager
		                    Log("Non existing " + nf);
		                    String[] vers = shortname.Split('.');
		                    if (vers.Count() == 6)
		                    {
		                    	// On vire le RC, c'est peut être ça qui bloque
		                    	String s = vers[0] + "." + vers[1] + "." + vers[2] + "." + vers[3] + "." + vers[5];
		                    	nf = "Update" + Path.DirectorySeparatorChar + s + Path.DirectorySeparatorChar + "Updater.exe";
		                    	Log("Testing " + nf);
		                    	niu = MyTools.GetMD5(nf);
		                    }
                        }
                        Log("Downloaded updater: " + niu);
                        if ((niu != "") && (niu != old))
                        {
                            Log("Replacing Updater with the new one");
                            File.Copy(nf, of, true);
                        }
                        else
                        {
                            Log("Updater not changed");
                        }
                    }
                    catch (Exception e1)
                    {
                    	Log("!!!! " + GetException("Error checking version for Updater.exe", e1));
                    }

                    System.Diagnostics.ProcessStartInfo myInfo = new System.Diagnostics.ProcessStartInfo();
                    myInfo.FileName = "Updater.exe";
                    myInfo.WorkingDirectory = GetInternalDataPath();
                    myInfo.Arguments = shortname + " MyGeocachingManager.exe MyGeocachingManager.exe.Config readme.html";
                    Process.Start(myInfo);
                    this._bForceClose = true;
                    this.Close();
                }
                else
                {
                    // Delete downloaded installation
                    MyTools.DeleteDirectory(unpackDirectory, true);
                }
            }
            catch (Exception exc)
            {
            	ShowException("", GetTranslator().GetString("UpdateExtractError"), exc);
            }
        }
        
        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            checkUpdatesToolStripMenuItem.Enabled = true;
            if ((!e.Cancelled) && (e.Error == null))
            {
                toolStripStatusLabel3.Text = GetTranslator().GetString("UpdateDownloadDone");
                UpdateMGMFromZipFile();
            }
            else
            {
                toolStripStatusLabel3.Text = GetTranslator().GetString("ErrUpdate");
                String msg = GetTranslator().GetString("ErrUpdate");
                if (e.Error != null)
                {
                	msg = GetException(msg, e.Error);
                }
                MsgActionError(this, msg);
            }
            sDownloadedUpdateFile = "";
        }

        private void DisplayConfValues(String[] keys)
        {
            String s = "";
            foreach(String key in keys)
            {
                s += key + ": " + ConfigurationManager.AppSettings[key] + "\r\n";
            }
            MSG(s);
        }

        /// <summary>
        /// Return web proxy (null if not defined)
        /// </summary>
        /// <returns>web proxy (null if not defined)</returns>
        public WebProxy GetProxy()
        {
            WebProxy proxy = null;
            try
            {
                if (ConfigurationManager.AppSettings["proxyused"] == "True")
                {
                    if (ConfigurationManager.AppSettings["proxydomain"] != "")
                    {
                        String proxypassword = ConfigurationManager.AppSettings["proxypassword"];
                        //String proxypassword = GetDecryptedProxyPassword();
                        proxy = new WebProxy(ConfigurationManager.AppSettings["proxydomain"], true);
                        NetworkCredential credentials = new NetworkCredential(
                            ConfigurationManager.AppSettings["proxylogin"],
                            proxypassword);
                        proxy.Credentials = credentials;
                        //Log("Proxy used");
                    }
                }
                else
                {
                    //Log("No proxy used");
                }
            }
            catch (Exception)
            {
                proxy = null;
            }
            return proxy;
        }
        
        private void DoPing()
        {
            try
            {
                if (!_bInternetAvailable)
                    return;

                String urlupdate = ConfigurationManager.AppSettings["urlupdate"];
                // Il se peut qu'on ait 2 URL (le site principal et le site de backup)
                List<string> urls = urlupdate.Split(';').ToList<string>();
                if (urls.Count >= 1)
                {
                    // On ne ping que la première URL qui est le site officiel
                    String url = urls[0] + "/ping.php"; ;
                    Log("Ping " + url);
                    
                    WebClient client = new WebClient();
                    WebProxy proxy = GetProxy();
                    if (proxy != null)
                        client.Proxy = proxy;

                    String rep = MyTools.GetRequest(new Uri(url), proxy, 500);
                    
                    Log("Response: " + rep);
                    /*
                    if (rep.StartsWith("NO"))
                    {
                    	// On créé le cookie en mode debug
                    	url += "?cookie=" + CreateCookie();
                    	MyTools.GetRequest(new Uri(url), proxy, 200);
                    }
                    */
                   if (rep.StartsWith("KILL"))
                    {
                    	// Kill switch !
                    	System.Diagnostics.ProcessStartInfo myInfo = new System.Diagnostics.ProcessStartInfo();
	                    myInfo.FileName = "Updater.exe";
	                    myInfo.WorkingDirectory = GetInternalDataPath();
	                    myInfo.Arguments = "KILL";
	                    myInfo.WindowStyle = ProcessWindowStyle.Hidden;
	                    Process.Start(myInfo);
	                    this._bForceClose = true;
                    }
                }
            }
            catch (Exception exc)
            {
                // Surtout ne pas planter là dessus
                Log(GetException("Error during ping", exc));
            }
        
        }
        
        private void CheckUpdate(bool bSilent, bool bForceUpdate)
        {
            String urlupdate = ConfigurationManager.AppSettings["urlupdate"];
            // Il se peut qu'on ait 2 URL (le site principal et le site de backup)
            List<string> urls = urlupdate.Split(';').ToList<string>();
            Log("Check update with " + urlupdate);
            bool bCheckSuccess = false;
            foreach (String url in urls)
            {
                if (url != "")
                {
                    Log("Trying url " + url);
                    bCheckSuccess = CheckUpdateImpl(url, bSilent, bForceUpdate);
                    if (bCheckSuccess)
                    {
                        Log("Check update success");
                        return;
                    }
                    else
                    {
                        Log("Check update fail");
                    }
                }
            }
        }


        private bool CheckUpdateImpl(String urlupdate, bool bSilent, bool bForceUpdate)
        {
            String urlver = urlupdate + "/version.ver";
            bool bCheckSuccess = false;

            try
            {
                checkUpdatesToolStripMenuItem.Enabled = false;
                toolStripStatusLabel3.Text = GetTranslator().GetString("UpdateLookInProgress");

                WebClient client = new WebClient();
                WebProxy proxy = GetProxy();
                if (proxy != null)
                    client.Proxy = proxy;
                
                //string result = client.DownloadString(urlver);
                string result = MyTools.GetRequest(new Uri(urlver), proxy, 2000);

                String currentVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
                Log("currentVersion : " + currentVersion);

                Log("Result from update check : " + result.Replace("\r\n",""));
                bCheckSuccess = true;
                List<String> results = result.Split('#').ToList<string>();
                if (results.Count >= 1)
                {
                    result = results[0];
                    _bMajorUpdateDetected = false;
                    if ((results.Count >=2))
                    {
                        // Deux possibilités : soit on utilise "FORCE" pour forcer quoi qu'il advienne
                        // ou alors on indique la version minimale qui est requise
                        // si on est inférieur à cette version, alors on force l'update
                        if (results[1].StartsWith("FORCE"))
                        {
                            Log("Force update");
                            _bMajorUpdateDetected = true;
                        }
                        else if (results[1].StartsWith("MINVER_"))
                        {
                            string minver = results[1].Replace("MINVER_","");
                            minver = minver.Replace("\r\n", "");
                            minver = minver.Replace("\n", "");
                            Log("minver = " + minver);
                            int compo = currentVersion.CompareTo(minver);
                            if (compo < 0)
                            {
                                _bMajorUpdateDetected = true;
                            }
                            else if ((compo == 0) && (AssemblySubVersion != ""))
                            {
                                // On a la même version majeure, mais c'est une béta
                                _bMajorUpdateDetected = true;
                            }
                        }
                        
                    }
                }
                Log("MajorUpdateDetected = " + _bMajorUpdateDetected.ToString());
                Log("Version to check : " + result);

                int comp = currentVersion.CompareTo(result);
                Log("Comparison result : " + comp);
                // Traitement des versions RC, beta, etc...
                // si on a AssemblySubVersion != "", alors la version exécutée est une version de test qu'il
                // faut mettre à jour même si la version est identique
                KeyValuePair<String, String> kv = new KeyValuePair<string, string>("", "");
                if ((comp <= 0) && (AssemblySubVersion != "")) //  même version ou supérieure, tout est mieux qu'une beta !
                {
                    // On a la même version majeure, mais c'est une béta
                    comp = -1;
                    _bMajorUpdateDetected = true;
                }
                else if (AssemblySubVersion != "") // On regarde si on a une beta plus récente
                {
                    // Si on est là, c'est qu'on est une béta, et que rien de mieux en officiel existe
                    // donc on regarde si une beta mieux existe
                    kv = BetaDownload.GetMoreRecentBetaAvailable(urlupdate, this);
                    if (kv.Key != "")
                    {
                        // Yeah on a trouvé une beta plus récente !!!
                        comp = -1;
                        _bMajorUpdateDetected = true;
                        result = kv.Key;
                    }
                }
                
                checkUpdatesToolStripMenuItem.Enabled = true;
                bool forceLocale = ConfigurationManager.AppSettings["forceupdate"] == "True";
                if ((comp < 0) || forceLocale || bForceUpdate)
                {
                    String msg = String.Format(GetTranslator().GetString("CheckUpdateNewDownloadAsk"), currentVersion + AssemblySubVersion, result);
                    String msgnotif = String.Format(GetTranslator().GetString("CheckUpdateNewDownloadNotif"), currentVersion + AssemblySubVersion, result);
                    msgnotif = msgnotif.Replace("#", "\r\n");
                    if (_bMajorUpdateDetected)
                    {
                        msg += Environment.NewLine + GetTranslator().GetString("LblMandatoryUpdate");
                        msgnotif += Environment.NewLine + GetTranslator().GetString("LblMandatoryUpdate");
                    }

                    aboutToolStripMenuItem.Image = _listImagesSized[getIndexImages("True")];
                    checkUpdatesToolStripMenuItem.Image = _listImagesSized[getIndexImages("True")];
                    checkUpdatesToolStripMenuItem.ToolTipText = msg;
                    toolStripStatusLabel3.Text = GetTranslator().GetString("UpdateAvailable");
                    

                    if (!bSilent)
                    {
                        DialogResult dialogResult = MyMessageBox.Show(msg,
                            GetTranslator().GetString("WarTitle"),
                            MessageBoxIcon.Question, GetTranslator());
                        if (dialogResult == DialogResult.Yes)
                        {
                            // download the file
                            // http://XXX/hitcounter.php?file=2.0.4738.29943
                            // Patch en attendant de résoudre mes problèmes PHP sur mon serveur perso
                            String fupdate = urlupdate + "/hitcounter.php?file=" + result;

                            // Est-ce une beta ?
                            if (kv.Key != "")
                            {
                                fupdate = kv.Value;

                                // En local on vire toute référence à une beta
                                int pos = kv.Key.LastIndexOf('.');
                                result = kv.Key.Substring(0, pos);
                            }

                            String localfile = GetInternalDataPath() + Path.DirectorySeparatorChar + result + ".MyGeocachingManager.zip";

                            DownloadAndInstallMGMVersion(client, fupdate, localfile);

                            // toolStripStatusLabel3
                            // checkUpdatesToolStripMenuItem
                            // UpdateLookInProgress
                            // UpdateDownloadInProgress
                            // UpdateDownloadDone

                        }
                        else if (dialogResult == DialogResult.No)
                        {
                            if (kv.Key == "")
                            {
                                // Open only for real versions, not beta
                                // Just open the web site
                                MyTools.StartInNewThread(urlupdate);
                            }
                        }
                    }
                    else
                    {
                        // nothing in silent mode
                        ShowNotification(GetTranslator().GetString("UpdateAvailable"), msgnotif);
                    }
                }
                else
                {
                    // No update required
                    toolStripStatusLabel3.Text = GetTranslator().GetString("CheckUpdateNothing");
                    aboutToolStripMenuItem.Image = null;
                    checkUpdatesToolStripMenuItem.Image = null;
                    checkUpdatesToolStripMenuItem.ToolTipText = null;
                    if (!bSilent)
                        MsgActionOk(this, GetTranslator().GetString("CheckUpdateNothing"));
                }
            }
            catch (Exception exc)
            {
                checkUpdatesToolStripMenuItem.Enabled = true;
                toolStripStatusLabel3.Text = GetTranslator().GetString("ErrUpdate");
                Log(GetException("Error during check update", exc));
                String msg = GetTranslator().GetString("ErrUpdate") + ": " + urlupdate;
                /*
                if (!bSilent)
                {
                	ShowException("", msg, exc);
                }*/
                aboutToolStripMenuItem.Image = _listImagesSized[getIndexImages("False")];
                checkUpdatesToolStripMenuItem.Image = _listImagesSized[getIndexImages("False")];
                checkUpdatesToolStripMenuItem.ToolTipText = msg;
                //throw;
                bCheckSuccess = false;
            }

            return bCheckSuccess;
        }

        /// <summary>
        /// Download latest MGM version and install it
        /// </summary>
        /// <param name="client">webclient used for download</param>
        /// <param name="fupdate">update url</param>
        /// <param name="localfile">local file for download</param>
        public void DownloadAndInstallMGMVersion(WebClient client, String fupdate, String localfile)
        {
            // Async mode
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            sDownloadedUpdateFile = localfile;
            if (File.Exists(localfile))
                File.Delete(localfile);
            toolStripStatusLabel3.Text = GetTranslator().GetString("UpdateDownloadInProgress");
            checkUpdatesToolStripMenuItem.Enabled = false;
            client.DownloadFileAsync(new Uri(fupdate), localfile);
        }
    
        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox(this);
            String desc = GetTranslator().GetString("LblAboutDescription").Replace("#", Environment.NewLine);
            about.TextBoxDescription = desc;
            about.ShowDialog();
        }

        private void automaticallyCheckForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (automaticallyCheckForUpdatesToolStripMenuItem.Checked)
                UpdateConfFile("autocheckupdate", "False");
            else
                UpdateConfFile("autocheckupdate", "True");
            UpdateMenuChecks();
        }

        private void updateTextBoxKmMi(TextBox txtBox)
        {
            if (_bUseKm == false)
            {
                double d = MyTools.ConvertToDouble(txtBox.Text) * _dConvKmToMi;
                txtBox.Text = String.Format("{0:0.#}", d);
            }
            else
            {
                double d = MyTools.ConvertToDouble(txtBox.Text) / _dConvKmToMi;
                txtBox.Text = String.Format("{0:0.#}", d);
            }
        }

        private void useMilToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (useMilToolStripMenuItem.Checked)
            {
                UpdateConfFile("usekm", "False");
                _bUseKm = false;
                updateTextBoxKmMi(txtDistMin);
                updateTextBoxKmMi(txtDistMax);
                updateTextBoxKmMi(textBox2neardist);
            }
            else
            {
                UpdateConfFile("usekm", "True");
                _bUseKm = true;
                updateTextBoxKmMi(txtDistMin);
                updateTextBoxKmMi(txtDistMax);
                updateTextBoxKmMi(textBox2neardist);
            }
            UpdateMenuChecks();
            TranslateKmMi();
            UpdateListViewKmMi();
            PopulateListViewCache(null);
        }

        /// <summary>
        /// Load a GGZ file
        /// </summary>
        /// <param name="ggzFile">path to a GGZ file</param>
        public void LoadGgz(String ggzFile)
        {
            Directory.SetCurrentDirectory(GetInternalDataPath());
            string unpackDirectory = Guid.NewGuid().ToString();

            if (Directory.Exists(unpackDirectory))
            {
                Log("Delete old dir: " + unpackDirectory);
                try
                {
                    MyTools.DeleteDirectory(unpackDirectory, true);
                }
                catch (Exception exc1)
                {
                	Log("!!!! " + GetException("Deleting unpack directory",exc1));
                    Log("!!!! Failed deleting " + unpackDirectory);
                }
            }
            
            Log("Unzipping " + ggzFile);
            try
            {
                ZipFile.ExtractToDirectory(ggzFile, unpackDirectory);
            }
            catch (Exception exc1)
            {
            	Log("!!!! " + GetException("Unzipping", exc1));
            }
            
            // Now we parse this directory located in "data" directory
            String pathdb = GetInternalDataPath() + Path.DirectorySeparatorChar + unpackDirectory + Path.DirectorySeparatorChar + "data";
            Log("Parsing " + pathdb);

            // Parse all GPX files
            _zipFileCurrentlyLoaded = ggzFile;
            try
            {
                string[] filePaths = Directory.GetFiles(pathdb, "*.gpx", SearchOption.AllDirectories);
                foreach (string f in filePaths)
                {
                    try
                    {
                        Log("Loading file " + f);
                        LoadFile(f);
                    }
                    catch (Exception exc)
                    {
                    	Log("!!!! " + GetException("Loading a file", exc));
                        Log("!!!! Failed loading " + f);
                        _errorMessageLoad += GetTranslator().GetString("ErrorLoad") + " " + f + "\r\n";
                    }
                    Log("Number of cache in DB: " + _caches.Count.ToString());
                }
                _zipFileCurrentlyLoaded = "";
            }
            catch (Exception exc2)
            {
            	Log("!!!! " + GetException("Loading a GPX",exc2));
                Log("!!!! Failed loading GPX in " + pathdb);
                _errorMessageLoad += GetTranslator().GetString("ErrorLoad") + " " + pathdb + "\r\n";
            }

            // and some cleanup
            Log("Removing old dir: " + unpackDirectory);
            MyTools.DeleteDirectory(unpackDirectory, true);
            Log("Done with zip :)");
            _LoadedOriginalFiles.Add(ggzFile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zipToUnpack"></param>
        /// <param name="caches"></param>
        public void LoadGCZip(String zipToUnpack, ref Dictionary<String, Geocache> caches)
        {
        	Directory.SetCurrentDirectory(GetInternalDataPath());
            string unpackDirectory = Guid.NewGuid().ToString();

            if (Directory.Exists(unpackDirectory))
            {
                Log("Delete old dir: " + unpackDirectory);
                try
                {
                    MyTools.DeleteDirectory(unpackDirectory, true);
                }
                catch (Exception exc1)
                {
                	Log("!!!! " + GetException("Deleting unpack directory", exc1));
                    Log("!!!! Failed deleting " + unpackDirectory);
                }
            }
            Log("Unzipping " + zipToUnpack);
            try
            {
                ZipFile.ExtractToDirectory(zipToUnpack, unpackDirectory);
            }
            catch (Exception exc1)
            {
            	Log("!!!! " + GetException("Unzipping", exc1));
            }

            // Now we parse this directory
            String pathdb = GetInternalDataPath() + Path.DirectorySeparatorChar + Path.DirectorySeparatorChar + unpackDirectory;
            Log("Parsing " + pathdb);

            // Parse all GPX files
            string[] filePaths = Directory.GetFiles(pathdb, "*.gpx", SearchOption.AllDirectories);
            // Les waypoints/ On considère qu'ils sont liés uniquement aux caches trouvées dans ce zip ou dans la liste précédente
            // on les traite à la fin
            List<Waypoint> waypoints = new List<Waypoint>();
            foreach (string f in filePaths)
            {
                try
                {
                    Log("Loading file " + f);
                    LoadGCFile(f, ref  caches, ref waypoints);
                }
                catch (Exception exc)
                {
                	Log("!!!! " + GetException("Loading file", exc));
                    Log("!!!! Failed loading " + f);
                }
            }

            // Traiter les waypoints
            AssociateWaypointsToGeocaches(ref caches, waypoints);
            waypoints.Clear();
            waypoints = null;
            
            // and some cleanup
            Log("Removing old dir: " + unpackDirectory);
            MyTools.DeleteDirectory(unpackDirectory, true);
            Log("Done with zip :)");
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="caches"></param>
        /// <param name="waypoints"></param>
        public void AssociateWaypointsToGeocaches(ref Dictionary<String, Geocache> caches, List<Waypoint> waypoints)
        {
        	if ((caches != null) && (waypoints != null))
        	{
        		foreach(Waypoint wpt in waypoints)
        		{
        			if (caches.ContainsKey(wpt._GCparent))
        			{
        				var lst = caches[wpt._GCparent]._waypoints;
        				if (!lst.ContainsKey(wpt._name))
        					lst.Add(wpt._name, wpt);
        			}
        		}
        	}
        	
        }
        
        /// <summary>
        /// Load a zip file
        /// </summary>
        /// <param name="zipToUnpack">Path to a zip file</param>
        public void LoadZip(String zipToUnpack)
        {
            Directory.SetCurrentDirectory(GetInternalDataPath());
            string unpackDirectory = Guid.NewGuid().ToString();

            if (Directory.Exists(unpackDirectory))
            {
                Log("Delete old dir: " + unpackDirectory);
                try
                {
                    MyTools.DeleteDirectory(unpackDirectory, true);
                }
                catch (Exception exc1)
                {
                	Log("!!!! " + GetException("Deleting unpack directory", exc1));
                    Log("!!!! Failed deleting " + unpackDirectory);
                }
            }
            Log("Unzipping " + zipToUnpack);
            try
            {
                ZipFile.ExtractToDirectory(zipToUnpack, unpackDirectory);
            }
            catch (Exception exc1)
            {
            	Log("!!!! " + GetException("Unzipping", exc1));
            }

            // Now we parse this directory
            String pathdb = GetInternalDataPath() + Path.DirectorySeparatorChar + Path.DirectorySeparatorChar + unpackDirectory;
            Log("Parsing " + pathdb);

            // Parse all GPX files
            _zipFileCurrentlyLoaded = zipToUnpack;
            string[] filePaths = Directory.GetFiles(pathdb, "*.gpx", SearchOption.AllDirectories);
            foreach (string f in filePaths)
            {
                try
                {
                    Log("Loading file " + f);
                    LoadFile(f);
                }
                catch (Exception exc)
                {
                	Log("!!!! " + GetException("Loading file", exc));
                    Log("!!!! Failed loading " + f);
                    _errorMessageLoad += GetTranslator().GetString("ErrorLoad") + " " + f + "\r\n";
                }
                Log("Number of cache in DB: " + _caches.Count.ToString());
            }
            _zipFileCurrentlyLoaded = "";

            // and some cleanup
            Log("Removing old dir: " + unpackDirectory);
            MyTools.DeleteDirectory(unpackDirectory, true);
            Log("Done with zip :)");
            _LoadedOriginalFiles.Add(zipToUnpack);
        }

        private void loadGPXFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {

            // Set the file dialog to filter for graphics files.
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //openFileDialog1.Filter = "GPX & ZIP (*.gpx, *.zip)|*.gpx;*.zip|GPX (*.gpx)|*.gpx|ZIP (*.zip)|*.zip";
            openFileDialog1.Filter = "GPX & ZIP & GGZ (*.gpx, *.zip, *.ggz)|*.gpx;*.zip;*.ggz|GPX (*.gpx)|*.gpx|ZIP (*.zip)|*.zip|GGZ (*.ggz)|*.ggz";
            //  Allow the user to select multiple images.
            openFileDialog1.Multiselect = true;
            openFileDialog1.Title = GetTranslator().GetString("DlgChoseGPX");

            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string[] filenames = openFileDialog1.FileNames;
                LoadBatchOfFiles(filenames, false, true);
            }
        }

        private void LoadBatchOfFiles(string[] filenames, bool bForceReplace, bool bOfferToCopyFileintoDB)
        {
            LoadBatchOfFilesImpl(filenames, bForceReplace, false, bOfferToCopyFileintoDB); // RESET THE FILTER !!!
        }

        /// <summary>
        /// 
        /// </summary>
        public void CleanMGMInternals()
        {
        	// Cleanup
            _caches = new Dictionary<string, Geocache>();
            _waypointsLoaded = new Dictionary<String, Waypoint>();
            _listViewCaches = new List<EXListViewItem>();
            _LoadedFiles = new List<string>();
            _LoadedOriginalFiles = new List<string>();
            LoadMGMWayPoints();
            lvGeocaches.Items.Clear();
            EmptywbFastCachePreview();
        }
        
        /// <summary>
        /// Load a batch of GPX files (GPX, GGZ, ZIP)
        /// </summary>
        /// <param name="filenames">List of files to load</param>
        /// <param name="bForceReplace">If true, previously loaded caches will be erased</param>
        /// <param name="bUseFilter">If true, any existing activated filter will be applied at the end of the loading</param>
        /// <param name="bOfferToCopyFileintoDB">If true, it will be proposed to copy the files into the DB if they do not exists</param>
        public void LoadBatchOfFilesImpl(string[] filenames, bool bForceReplace, bool bUseFilter, bool bOfferToCopyFileintoDB)
        {
        	Log("--- LoadBatchOfFilesImpl ---");
            bool bReplace = false;

            if (bForceReplace == false)
            {
                MyMessageBox box = new MyMessageBox(GetTranslator().GetString("AskReplaceGPX").Replace("#", Environment.NewLine),
                    GetTranslator().GetString("AskReplaceGPXCaption"), MessageBoxIcon.Question, null, null, GetTranslator());
                box.TopMost = true;
                box.button1.Text = GetTranslator().GetString("BtnReplace");
                box.button2.Text = GetTranslator().GetString("BtnCombine");

                DialogResult dialogResult = box.ShowDialog();
                if (dialogResult == DialogResult.Yes)
                {
                    bReplace = true;
                }
                else if (dialogResult == DialogResult.No)
                {
                    bReplace = false;
                }
                else
                {
                	MsgActionCanceled(this);
                    return;
                }
            }
            else
            {
                bReplace = bForceReplace;
            }

            _ThreadProgressBarTitle = "";
            CreateThreadProgressBar();
            if (bReplace)
            {
                // Cleanup
                CleanMGMInternals();
            }

            // Read the files
            if (filenames != null)
            {
                foreach (String f in filenames)
                {
                    try
                    {
                        FileInfo fi = new FileInfo(f);
                        if (fi.Extension.ToLower() == ".gpx")
                        {
                            LoadFile(f);
                            _LoadedOriginalFiles.Add(f);
                        }
                        else if (fi.Extension.ToLower() == ".zip")
                        {
                            LoadZip(f);
                        }
                        else if (fi.Extension.ToLower() == ".ggz")
                        {
                            LoadGgz(f);
                        }
                    }
                    catch (Exception exc)
                    {
                    	Log("!!!! " + GetException("Loading gpx/zip/ggz", exc));
                        Log("!!!! Failed loading " + f);
                        _errorMessageLoad += GetTranslator().GetString("ErrorLoad") + " " + f + "\r\n";
                    }
                    Log("Number of cache in DB: " + _caches.Count.ToString());
                }
            }

            // Now we join Wpts & caches
            JoinWptsGC();
            ChangeCacheStatusBasedonMGM();
            PostTreatmentLoadCache();

            Log("BuildListViewCache");
            BuildListViewCache();
            Log("PopulateListViewCache");
            _bUseFilter = bUseFilter; // DON'T FORGET TO RESET THE FILTER !
            PopulateListViewCache(null);
            Log("Add control");
            KillThreadProgressBar();
            
            if (bOfferToCopyFileintoDB)
            {
            	// on ne va parler que des fichiers n'étant pas déjà présents dans la DB
            	// i.e. les fichiers dont le chemin ne commence pas par datapath
            	List<String> files = new List<string>();
            	String gpxdatapath = GetUserDataPath() + Path.DirectorySeparatorChar + "GPX" + Path.DirectorySeparatorChar;
            	String filelist = "\r\n";
            	foreach (String f in filenames)
	            {
            		if (f.StartsWith(gpxdatapath) == false)
            		{
            			filelist += f + "\r\n";
            			Log(f + " shall be copied...");
            			files.Add(f);
            		}
            	}
            	
            	if (files.Count != 0)
            	{
		            ParameterObject po = new ParameterObject(ParameterObject.ParameterType.Bool, true, "checkbox", GetTranslator().GetString("LblReplaceExistingOlderFile"));
		        	DialogResult dr = MyMessageBox.Show(
		                GetTranslator().GetString("LblIncludeFileInDB") + filelist,
		                GetTranslator().GetString("LblTitleIncludeFileInDB"),
		                MessageBoxIcon.Question,
		                po,
		                GetTranslator());
		            if (dr == DialogResult.Yes)
		            {
		        		bool bOverwrite = po.GetControlValueString() == "True";
		        		// on va copier le fichier dans le répertoire GPX de MGM
		        		foreach (String f in files)
		                {
		                    try
		                    {
		                    	String dst =  gpxdatapath + Path.GetFileName(f);
		                    	if (File.Exists(dst))
		                    	{
		                    		if (bOverwrite)
		                    		{
			                    		// La dst est plus ancienne ?
										System.IO.FileInfo fsrc = new System.IO.FileInfo(f);
										System.IO.FileInfo fdst = new System.IO.FileInfo(dst);
										if(fdst.LastWriteTime < fsrc.LastWriteTime)
										{
											Log("Overwrite existing file");
											File.Copy(f, dst, true);
										}
										else
										{
											Log("WE DO NOT overwrite existing file");
										}
										// sinon on ne fait rien
		                    		}
		                    		else
		                    		{
		                    			Log("File exist but overwrite NOT requested");
		                    		}
		                    	}
		                    	else
		                    	{
		                    		// on copie sans se poser de question
		                    		Log("File did not exist, copy it");
		                    		File.Copy(f, dst, true);
		                    	}
		                    }
		                    catch (Exception exc)
		                    {
		                    	Log("!!!! " + GetException("Copying file in GPX directory", exc));
		                        Log("!!!! Failed copying " + f);
		                    }
		                }
		            }
		            else
		            {
		            	Log("Copy in DB not requested");
		            }
            	}
            	else
            	{
            		Log("All files present in GPX folder");
            	}
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            LoadBatchOfFiles(files, false, true);
        }

        private void lstv_SelectionChanged(object sender, System.EventArgs e)
        {
        	// Si avant on avait des caches sélectionnées et maintenant aucune, on grise certaines parties des menus
        	// Si avant on avait aucune cache sélectionnée et maintenant au moins une, on dégrise certaines parties des menus
        	int newNb = 0;
            if (lvGeocaches.SelectedItems != null)
                newNb = lvGeocaches.SelectedItems.Count;
            else
            	newNb = 0;
            
            if ((_iNbCachesSelectedInListview == 0) && (newNb != 0))
            {
            	// On dégrise
            	EnableDisableMenuEntries(_menuEntriesRequiringSelectedCaches, true, "SelectedNo");
            	// La toolbar
            	EnableDisableToolbar();
            }
            else if ((_iNbCachesSelectedInListview != 0) && (newNb == 0))
            {
            	// On grise
            	EnableDisableMenuEntries(_menuEntriesRequiringSelectedCaches, false, "SelectedNo");
            	// La toolbar
            	EnableDisableToolbar();
            }
            // sinon c'est juste un changement de nombre et on ne fait rien
            
            // le nombre de 1 exact
            if ((_iNbCachesSelectedInListview == 1) && (newNb != 1))
            {
            	EnableDisableMenuEntries(_menuEntriesRequiringOnlyOneSelectedCaches, false, "SelectedOneNo");
            	// La toolbar
            	EnableDisableToolbar();
            }
            else if ((_iNbCachesSelectedInListview != 1) && (newNb == 1))
            {
            	// On grise
            	EnableDisableMenuEntries(_menuEntriesRequiringOnlyOneSelectedCaches, true, "SelectedOneNo");
            	// La toolbar
            	EnableDisableToolbar();
            }
            // sinon c'est juste un changement de nombre et on ne fait rien
            
            // On met à jour le status bar
            _iNbCachesSelectedInListview = newNb;
            toolStripStatusLabel3.Text = String.Format(GetTranslator().GetString("LblSelCachesNb"), _iNbCachesSelectedInListview);
        }

        /// <summary>
        /// Return number of selected caches (fast)
        /// </summary>
        /// <returns>Number of selected caches</returns>
        public int GetNumberOfSelectedCaches()
        {
            return lvGeocaches.SelectedItems.Count;
        }

        /// <summary>
        /// Return selected caches
        /// </summary>
        /// <returns>selected caches</returns>
        public List<Geocache> GetSelectedCaches()
        {
            List<Geocache> r = new List<Geocache>();
            foreach (Object obj in lvGeocaches.SelectedItems)
            {
                EXListViewItem lstvItem = obj as EXListViewItem;
                String code = lstvItem.Text;
                r.Add(_caches[code]);
            }
            return r;
        }

        /// <summary>
        /// Return selected listview items
        /// </summary>
        /// <returns>selected listview items</returns>
        public ListView.SelectedListViewItemCollection GetSelectedListViewItems()
        {
            return lvGeocaches.SelectedItems;
        }

        /// <summary>
        /// Return displayed caches
        /// </summary>
        /// <returns>displayed caches</returns>
        public List<Geocache> GetDisplayedCaches()
        {
            List<Geocache> r = new List<Geocache>();
            foreach (Object obj in lvGeocaches.Items)
            {
                EXListViewItem lstvItem = obj as EXListViewItem;
                String code = lstvItem.Text;
                r.Add(_caches[code]);
            }
            return r;
        }

        private void SolverAndDisplayTSP()
        {
            List<Geocache> r = GetDisplayedCaches();
            if (r.Count == 0)
            {
                return;
            }
            else if (tsp != null)
            {
                // we are already running, so tell the tsp thread to halt.
                tsp.Halt = true;
                MsgActionError(this, GetTranslator().GetString("ErrPathComputing"));
            }
            else if (r.Count < 50)
            {
                // let's go !
            }
            else
            {
                DialogResult dialogResult = MessageBox.Show(
                    String.Format(GetTranslator().GetString("LblMightBeLong"),r.Count.ToString()),
                    GetTranslator().GetString("WarTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult != DialogResult.Yes)
                    return;
            }


            // TVPIncludeHome
            DialogResult dr = MessageBox.Show(
                    GetTranslator().GetString("TVPIncludeHome"),
                    GetTranslator().GetString("WarTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                Geocache fakegeo = new Geocache(this);
                fakegeo._Longitude = _dHomeLon.ToString().Replace(",",".");
                fakegeo._Latitude = _dHomeLat.ToString().Replace(",", ".");
                fakegeo._dLongitude = _dHomeLon;
                fakegeo._dLatitude = _dHomeLat;
                fakegeo._Code = "HOME";
                fakegeo._Name = "Home";
                fakegeo._Type = "HomeG";
                fakegeo._bOwned = false;
                fakegeo._Available = "True";
                fakegeo._PlacedBy = ConfigurationManager.AppSettings["owner"];
                if (fakegeo._PlacedBy == "")
                    fakegeo._PlacedBy = "Myself";
                fakegeo._DateCreation = DateTime.Now.ToString(GeocachingConstants._FalseDatePattern);
                fakegeo._D = "1";
                fakegeo._T = "1";
                fakegeo._Container = "Not chosen";
                r.Insert(0, fakegeo);

            }

            cityList = new Tsp.Cities();
            cityList.OpenCityList(r);

            tspprogress = new ThreadProgress();
            tspprogress.Font = this.Font;
            tspprogress.Icon = this.Icon;
            tspprogress.Text = GetTranslator().GetString("LblComputeInProgress");
            tspprogress.btnAbort.Text = GetTranslator().GetString("BtnAbort");
            tspprogress.lblWait.Text = GetTranslator().GetString("LblWaitingNoTime");
            tspprogress.progressBar1.Visible = false;
            tspprogress.pictureBox1.Visible = true;
            tspprogress.label1.Visible = false;
            tspprogress.Show();

            ThreadPool.QueueUserWorkItem(new WaitCallback(BeginTsp));
        }

        /// <summary>
        /// Starts up the TSP class.
        /// This function executes on a thread pool thread.
        /// </summary>
        /// <param name="stateInfo">Not used"daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        private void BeginTsp(Object stateInfo)
        {
            // Assume the StartButton_Click did all the error checking
            int populationSize = 10000;
            int maxGenerations = 300000;
            int mutation = 3;
            int groupSize = 5;
            int numberOfCloseCities = 5;
            int chanceUseCloseCity = 90;
            int seed = 0;

            if (cityList.Count > 50)
                maxGenerations = 1000000;
            cityList.CalculateCityDistances(numberOfCloseCities);

            tspprogress.progressBar1.Maximum = maxGenerations;
            tsp = new Tsp.Tsp();
            tsp.foundNewBestTour += new Tsp.Tsp.NewBestTourEventHandler(tsp_foundNewBestTour);
            tsp.Begin(populationSize, maxGenerations, groupSize, mutation, seed, chanceUseCloseCity, cityList);
            tsp.foundNewBestTour -= new Tsp.Tsp.NewBestTourEventHandler(tsp_foundNewBestTour);
            tsp = null;
        }

        /// <summary>
        /// Delegate for the thread that runs the TSP algorithm.
        /// We use a separate thread so the GUI can redraw as the algorithm runs.
        /// </summary>
        /// <param name="sender">Object that generated this event."daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        /// <param name="e">Event arguments</param>
        public delegate void DrawEventHandler(Object sender, Tsp.TspEventArgs e);

        /// <summary>
        /// TSP algorithm raised an event that a new best tour was found.
        /// We need to do an invoke on the GUI thread before doing any draw code.
        /// </summary>
        /// <param name="sender">Object that generated this event</param>
        /// <param name="e">Event arguments.</param>
        private void tsp_foundNewBestTour(object sender, Tsp.TspEventArgs e)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new DrawEventHandler(DrawTour), new object[] { sender, e });
                    return;
                }
                catch (Exception)
                {
                    // This will fail when run as a control in IE due to a security exception.
                }
            }

            DrawTour(sender, e);
        }

        /// <summary>
        /// A new "best" tour from the TSP algorithm has been received.
        /// Draw the tour on the form, and update a couple of status labels.
        /// </summary>
        /// <param name="sender">Object that generated this event."daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        /// <param name="e">Event arguments."daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        public void DrawTour(object sender, Tsp.TspEventArgs e)
        {
            int lastCity = 0;
            int nextCity = e.BestTour[0].Connection1;
            List<Tsp.City> newBestTour = new List<Tsp.City>();

            foreach (Tsp.City city in e.CityList)
            {
                newBestTour.Add(cityList[lastCity]);
                
                // figure out if the next city in the list is [0] or [1]
                if (lastCity != e.BestTour[nextCity].Connection1)
                {
                    lastCity = nextCity;
                    nextCity = e.BestTour[nextCity].Connection1;
                }
                else
                {
                    lastCity = nextCity;
                    nextCity = e.BestTour[nextCity].Connection2;
                }
            }
            newBestTour.Add(cityList[lastCity]);

            if (e.Complete)
            {
                if (tspprogress != null)
                {
                    tspprogress.Hide();
                    tspprogress.Dispose();
                    tspprogress = null;
                }
                // DisplayNameAttribute the best tour
                DisplayTVPResult(newBestTour);
                
            }
            else
            {
                if (tspprogress._bAbort)
                {
                    tspprogress.Hide();
                    tspprogress.Dispose();
                    tspprogress = null;
                    tsp.Halt = true;
                }
                else
                {
                    //tspprogress.Step();
                }
            }
        }

        private void DisplayTVPResult(List<Tsp.City> newBestTour)
        {
            if (newBestTour.Count != 0)
            {
                // On a fini de calculer l'itinéraire
                // On l'affiche 
                // On efface l'itinéraire précédent
                _cacheDetail.EmptyRouteMarkers();
                ClearOverlay_RESERVED2();

                // On crée la route
                List<PointLatLng> pts = new List<PointLatLng>();

                foreach (Tsp.City c in newBestTour)
                {
                    pts.Add(new PointLatLng(c.Cache._dLatitude, c.Cache._dLongitude));
                }

                if ((pts != null) && (pts.Count != 0))
                {
                    // La route en ligne droite
                    GMapRoute route = new GMapRoute(pts, "Route");
                    route.IsHitTestVisible = true;
                    route.Tag = _cacheDetail._gmap; // très important pour le tooltip
                    Pen pen = new Pen(Color.Red);
                    pen.Width = 2; // route.Stroke.Width; 
                    route.Stroke = pen; 
                    // On change le nom de cette route pour le tooltip
                    String kmmi = (_bUseKm) ? GetTranslator().GetString("LVKm") : GetTranslator().GetString("LVMi");
                    double dist = (_bUseKm) ? route.Distance : route.Distance * _dConvKmToMi;
                    String tooltiptext = dist.ToString("0.0") + " " + kmmi;
                    route.Name = tooltiptext;
                    
                    _cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2].Routes.Add(route);

                    // On affiche
                    ShowCacheMapInCacheDetail();
                }
            }

        }

        private void UpdateListViewOfflineIcons(List<OfflineCacheData> data)
        {
            // Create a dictionnary
            Dictionary<String, OfflineCacheData> dico = new Dictionary<string, OfflineCacheData>();
            foreach (OfflineCacheData ocd in data)
            {
                dico.Add(ocd._Code, ocd);
            }

            // Udate icons
            foreach (EXListViewItem item in lvGeocaches.Items)
            {
                String code = item.Text;
                if (dico.ContainsKey(code))
                    UpdateOfflineIcons(item, code);
            }
            lvGeocaches.Invalidate();
        }

        private void UpdateListViewOfflineIcons(OfflineCacheData data)
        {
            // Udate icons
            foreach (EXListViewItem item in lvGeocaches.Items)
            {
                String code = item.Text;
                if (data._Code == code)
                    UpdateOfflineIcons(item, code);
            }
            lvGeocaches.Invalidate();
        }

        private void UpdateListViewBookmarkIcons(OfflineCacheData data, bool bBookmark)
        {
            // Udate icons
            foreach (EXListViewItem item in lvGeocaches.Items)
            {
                String code = item.Text;
                if (data._Code == code)
                    UpdateBookmarkIcons(item, code, bBookmark);
            }
            lvGeocaches.Invalidate();
        }

        /// <summary>
        /// Notification of DownloadWorker end of work
        /// finalizes images download
        /// </summary>
        /// <param name="worker">download worker</param>
        public void NotifyEndOfThread(DownloadWorker worker)
        {
            // Purge aborted ?
            List<OfflineCacheData> toPurge = new List<OfflineCacheData>();
            String offdatapath = GetUserDataPath() + Path.DirectorySeparatorChar + "Offline";
            foreach (OfflineCacheData ocd1 in worker.ocds)
            {
                // _bAborted:  delete them because they are in an inconsistent state
                // _NotDownloaded: they are empty with JUST the filenames but NOT the local files
                // so they don't really have offline data
                if (ocd1._bAborted)
                {
                    toPurge.Add(ocd1);
                }
                else if (ocd1._NotDownloaded)
                {
                    if (ocd1._Comment != "")
                    {
                        // Ok, just removed the files references
                        ocd1.PurgeFiles(offdatapath);
                    }
                    else
                    {
                        // No comment and not downloaded ? die !
                        toPurge.Add(ocd1);
                    }
                }
            }

            if (toPurge.Count != 0)
            {
                foreach (OfflineCacheData ocd in toPurge)
                {
                    ocd.PurgeFiles(offdatapath);
                    if (ocd.IsEmpty())
                    {
                        RemoveAssociationOcdCache(ocd._Code);
                    }
                }
            }
            _od.Serialize(_odfile);

            // Update icons
            UpdateListViewOfflineIcons(worker.ocds);
            MsgActionOk(this, GetTranslator().GetString("LblOfflineDownloadOver"));
        }

        private void LoadFilters()
        {
            // Insert at least one filter
            cbFilterList.Items.Clear();
            cbFilterList.Items.Add(CreateFooFilter());
            clbMltipleFilters.Items.Clear();
            clbMltipleFilters.Items.Add(CreateFooFilter("CBCurrentFilter"));

            String pathfilter = GetUserDataPath() + Path.DirectorySeparatorChar + "Filters";
            string[] filePaths = Directory.GetFiles(pathfilter, "*.osl", SearchOption.AllDirectories);
            foreach (string f in filePaths)
            {
                try
                {
                    CacheFilter aFilter = CacheFilter.Deserialize(f);
                    aFilter._filename = f;
                    cbFilterList.Items.Add(aFilter);
                    clbMltipleFilters.Items.Add(aFilter);
                }
                catch (Exception exc)
                {
                	Log("!!!! " + GetException("Loading a filter", exc));
                    Log("!!!! Failed loading Filter " + f);
                    _errorMessageLoad += GetTranslator().GetString("ErrorLoad") + " " + f + "\r\n";
                }
            }

            cbFilterList.SelectedIndex = 0;
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                CacheFilter sel = (CacheFilter)(cbFilterList.SelectedItem);
                if (sel._bToIgnore == false) // That's the place holder
                {
                    Filter = sel.CreateClone();
                    UpdateFromFilter();
                }
                else
                {
                    // Load empty filter
                    // WarNewFilter
                    DialogResult dialogResult = MessageBox.Show(GetTranslator().GetString("WarNewFilter"),
                        GetTranslator().GetString("WarTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dialogResult == DialogResult.Yes)
                    {
                        Filter = sel.CreateClone();
                        UpdateFromFilter();
                    }
                    else
                        return;
                }

                // Post treatment, sometimes filters can be activated even if nothing is necessary (type, size, attributes)
                PostTreatmentFilter();
                DoFilter();
            }
            catch (Exception exc)
            {
                ShowException("", GetTranslator().GetString("ErrLoadFilter"), exc);
            }
        }

        private void PostTreatmentFilter()
        {
            
            if (Filter._attributes.Count == 0)
                cbFilterAttributeIn.Checked = false;
            if (Filter._attributesexcl.Count == 0)
                cbFilterAttributeOut.Checked = false;
           
            bool ckd = true;
            ckd = ckd & cbType1.Checked;
            ckd = ckd & cbType2.Checked;
            ckd = ckd & cbType3.Checked;
            ckd = ckd & cbType4.Checked;
            ckd = ckd & cbType5.Checked;
            ckd = ckd & cbType6.Checked;
            ckd = ckd & cbType7.Checked;
            ckd = ckd & cbType8.Checked;
            ckd = ckd & cbType9.Checked;
            ckd = ckd & cbType10.Checked;
            ckd = ckd & cbType11.Checked;
            ckd = ckd & cbType12.Checked;
            ckd = ckd & cbType13.Checked;
            ckd = ckd & cbType14.Checked;
            ckd = ckd & cbType15.Checked;
            if (ckd)
                cbFilterType.Checked = false;

            ckd = true;
            ckd = ckd & cbSizeL.Checked;
            ckd = ckd & cbSizeM.Checked;
            ckd = ckd & cbSizeO.Checked;
            ckd = ckd & cbSizeR.Checked;
            ckd = ckd & cbSizeS.Checked;
            ckd = ckd & cbSizeU.Checked;
            ckd = ckd & cbSizeV.Checked;
            if (ckd)
                cbFilterSize.Checked = false;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Update otherwise save bullshit :-(
                if (!UpdateFilter())
                    return;

                CacheFilter sel = (CacheFilter)(cbFilterList.SelectedItem);
                if (sel._bToIgnore == false)
                {
                    // Overwrite
                    DialogResult dialogResult = MessageBox.Show(
                        String.Format(GetTranslator().GetString("DlgSaveFilterTxt"), sel._description),
                        GetTranslator().GetString("DlgSaveFilterTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dialogResult == DialogResult.Yes)
                    {
                        // Delete old filter
                        File.Delete(sel._filename);

                        // Create a clone of the current filter otherwise we might mess up a previously
                        // loaded filter that will thus share the same memory handler !!!
                        // USE THE PUREFILTER, IN CASE FILTER IS COMBINED WITH SOMETHING !!!
                        CacheFilter fifi = Filter.CreateClone();
                        

                        // Copy old description
                        fifi._description = sel._description;
                        fifi._descriptionDetails = sel._descriptionDetails;

                        // save new filter
                        String pathfilter = GetUserDataPath() + Path.DirectorySeparatorChar + "Filters";
                        fifi._filename = pathfilter + Path.DirectorySeparatorChar + String.Format(@"{0}.osl", Guid.NewGuid());
                        fifi.Serialize(fifi._filename);
                        
                        // Mise à jour
                        Filter = fifi;
                        cbFilterList.Items[cbFilterList.SelectedIndex] = Filter;
                        clbMltipleFilters.Items[cbFilterList.SelectedIndex] = Filter;
                    }
                }
                else
                {
                    // New filter
                    List<ParameterObject> lst = new List<ParameterObject>();
                    lst.Add(new ParameterObject(ParameterObject.ParameterType.String, "", "desc", GetTranslator().GetString("LblFilterDescriptionTxt")));
                    lst.Add(new ParameterObject(ParameterObject.ParameterType.String, "", "details", GetTranslator().GetString("LblFilterDescriptionTxt2")));
                    
                    ParametersChanger changer = new ParametersChanger();
                    changer.Title = GetTranslator().GetString("LblFilterDescriptionTitle");
                    changer.BtnCancel = GetTranslator().GetString("BtnCancel");
                    changer.BtnOK = GetTranslator().GetString("BtnOk");
                    changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
                    changer.ErrorTitle = GetTranslator().GetString("Error");
                    changer.Parameters = lst;
                    changer.Font = this.Font;
                    changer.Icon = this.Icon;

                    if (changer.ShowDialog() == DialogResult.OK)
                    {
                        // Create a clone of the current filter otherwise we might mess up a previously
                        // loaded filter that will thus share the same memory handler !!!
                        CacheFilter fifi = Filter.CreateClone();
                        
                        fifi._bToIgnore = false;
                        fifi._description = lst[0].Value;
                        fifi._descriptionDetails = lst[1].Value;
                        String pathfilter = GetUserDataPath() + Path.DirectorySeparatorChar + "Filters";
                        fifi._filename = pathfilter + Path.DirectorySeparatorChar + String.Format(@"{0}.osl", Guid.NewGuid());
                        fifi.Serialize(fifi._filename);
                        
                        // Mise à jour
                        Filter = fifi;
                        int i = cbFilterList.Items.Add(Filter);
                        clbMltipleFilters.Items.Add(Filter);
                        cbFilterList.SelectedIndex = i; // cbFilterList.Items.Count - 1;
                    }
                }
            }
            catch (Exception exc)
            {
                ShowException("", GetTranslator().GetString("ErrSaveFilter"), exc);
            }

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                CacheFilter sel = (CacheFilter)(cbFilterList.SelectedItem);
                if (sel._bToIgnore == false) // That's the place holder
                {
                    DialogResult dialogResult = MessageBox.Show(
                        String.Format(GetTranslator().GetString("DlgDelFilterTxt"), sel._description),
                        GetTranslator().GetString("DlgDelFilterTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dialogResult == DialogResult.Yes)
                    {
                        //do something
                        File.Delete(sel._filename);
                        
                        // D'abord la combo multiple
                        clbMltipleFilters.Items.Remove(sel);

                        // Puis la liste normale
                        int i = cbFilterList.SelectedIndex - 1;
                        if (i < 0)
                            i = 0;
                        cbFilterList.Items.Remove(sel);
                        cbFilterList.SelectedIndex = i;
                    }
                }
            }
            catch (Exception exc)
            {
                ShowException("", GetTranslator().GetString("ErrDelFilter"), exc);
            }

        }

        private void AddTabPageIconInfo(TabPage tab, String key)
        {
        	tab.ImageKey = key + "0";
        	tab.Tag = key;
        }
        
        private void AddIconToTabControlList(ImageList imgForTab, String key)
        {
        	imgForTab.Images.Add(key, _listImagesSized[getIndexImages(key)]); 
        	imgForTab.Images.Add(key + "0", _listImagesSized[getIndexImages(key + "0")]); 
        }
        
        private bool AddExtraIconToTab(bool chk, TabPage tab)
        {
        	// On regarde si on a un index particulier
            if (tab.Tag != null)
            {
                if (tab.Tag is String)
                {
                    String key = ((String)(tab.Tag));
                    //On a la clé pour le checked
                    
                    if (chk)
						tab.ImageKey = key;
                    else
                    	tab.ImageKey = key + "0";
                    return true;
                }
            }
            
            // Sinon par défaut
            if (chk)
            	tab.ImageIndex = 0; // coche verte
            else
            	tab.ImageIndex = -1; // rien du tout
            return false;
            
        }
        
        
        private void UpdatePageIfChecked(bool chkd, TabPage tab)
        {
            AddExtraIconToTab(chkd, tab);            
        }

        private void UpdatePageIfChecked(object sender, TabPage tab)
        {
            UpdatePageIfChecked(((CheckBox)(sender)).Checked, tab);
        }

        private void UpdatePageIfChecked(object[] senders, TabPage tab)
        {
        	bool chkd = false;
            foreach(object sender in senders)
            {
                if (((CheckBox)(sender)).Checked)
                {
                	chkd = true;
                }
            }
            
            AddExtraIconToTab(chkd, tab);
        }

        private void cbFilterSize_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePageIfChecked(sender, tabPage1Size);
        }

        private void cbFilterType_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePageIfChecked(sender, tabPage2Type);
        }

        private void cbFilterDistance_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePageIfChecked(sender, tabPage3Dist);
        }

        private void cbFilterStatus_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePageIfChecked(sender, tabPage4Status);
        }

        private void cbFilterDifficulty_CheckedChanged(object sender, EventArgs e)
        {
            //UpdatePageIfChecked(sender, tabPage5);
            UpdatePageIfChecked(new object[2] { cbFilterDifficulty, cbFilterTerrain }, tabPage5DT);
        }

        private void cbFilterTerrain_CheckedChanged(object sender, EventArgs e)
        {
            //UpdatePageIfChecked(sender, tabPage5);
            UpdatePageIfChecked(new object[2] { cbFilterDifficulty, cbFilterTerrain }, tabPage5DT);
        }

        private void cbFilterOwner_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePageIfChecked(sender, tabPage7User);
        }

        private void cbTBGC_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePageIfChecked(sender, tabPage9TB);
        }

        void UpdateCheckBoxStatus(bool bFilterModified)
        {
            // textBoxStatus FGBNotAvailable
            String avail = (cbAvailable.Checked) ? GetTranslator().GetString("FGBAvailable") : GetTranslator().GetString("FGBNotAvailable");
            String archi = (cbArchived.Checked) ? GetTranslator().GetString("FGBNotArchived") : GetTranslator().GetString("FGBArchived");
            String msg = String.Format(GetTranslator().GetString("LblExplanationStatus"), avail, archi);
            textBoxStatus.Text = msg;
            if (bFilterModified)
                CheckIfNotAlready(cbFilterStatus);

        }

        private void cbAvailable_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCheckBoxStatus(true);
        }

        private void cbArchived_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCheckBoxStatus(true);
        }

        void UpdateCheckBoxOwner(bool bFilterModified)
        {
           	String msg = "";
            
            if (cbOwnerDisplay.Checked)
            {
            	// On affiche toutes les caches dont...
            	if (cbOwned.Checked && cbFound.Checked)
            	{
            		// On affiche tout
            		msg = GetTranslator().GetString("FGDisplayCachesStatusAll");
            	}
            	else if (!cbOwned.Checked && cbFound.Checked)
            	{
            		msg = GetTranslator().GetString("FGDisplayCachesStatusNotowned");
            	}
            	else if (cbOwned.Checked && !cbFound.Checked)
            	{
            		msg = GetTranslator().GetString("FGDisplayCachesStatusNotFound");
            	}
            	else if (!cbOwned.Checked && !cbFound.Checked)
            	{
            		msg = GetTranslator().GetString("FGDisplayCachesStatusNotOwnedNotFound");
            	}
            }
            else
            {
            	// On affiche uniquement les caches dont...
            	if (cbOwned.Checked && cbFound.Checked)
            	{
            		// On affiche tout
            		msg = GetTranslator().GetString("FGDisplayCachesStatusOnlyAll");
            	}
            	else if (!cbOwned.Checked && cbFound.Checked)
            	{
            		msg = GetTranslator().GetString("FGDisplayCachesStatusOnlyFound");
            	}
            	else if (cbOwned.Checked && !cbFound.Checked)
            	{
            		msg = GetTranslator().GetString("FGDisplayCachesStatusOnlyOwned");
            	}
            	else if (!cbOwned.Checked && !cbFound.Checked)
            	{
            		msg = GetTranslator().GetString("FGDisplayCachesStatusNotOwnedNotFound");
            	}
            }
            textBoxUser.Text = msg;
            if (bFilterModified)
                CheckIfNotAlready(cbFilterOwner);

        }

        private void cbOwned_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCheckBoxOwner(true);
        }

        private void cbFound_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCheckBoxOwner(true);
        }

        private void CheckIfNotAlready(CheckBox cb)
        {
            if (cb.Checked == false)
                cb.Checked = true;
        }

        private void cbSize_CheckedChanged(object sender, EventArgs e)
        {
            CheckIfNotAlready(cbFilterSize);
        }

        private void cbType_CheckedChanged(object sender, EventArgs e)
        {
            CheckIfNotAlready(cbFilterType);
        }

        private void txtDist_TextChanged(object sender, EventArgs e)
        {
            CheckIfNotAlready(cbFilterDistance);
        }

        private void comboD_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckIfNotAlready(cbFilterDifficulty);
            ComboBox cb = (ComboBox)sender;
            String key = ((Double)(cb.SelectedIndex) / 2.0 + 1.0).ToString().Replace(",", ".");
            Image img = _listImagesSized[getIndexImages(key)];
            if (cb == comboDMin)
                pbDMin.Image = img;
            else
                pbDMax.Image = img;
        }

        private void comboT_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckIfNotAlready(cbFilterTerrain);
            ComboBox cb = (ComboBox)sender;
            String key = ((Double)(cb.SelectedIndex) / 2.0 + 1.0).ToString().Replace(",",".");
            Image img = _listImagesSized[getIndexImages(key)];
            if (cb == comboTMin)
                pbTMin.Image = img;
            else
                pbTMax.Image = img;
        }

        private void hideAllColumnsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lvGeocaches.BeginUpdate();
            foreach(ToolStripMenuItem item in displayToolStripMenuItem.DropDownItems)
            {
                if (item.Tag == null)
                    continue;
                item.Checked = true;
                DisplaySubMenuItem_Click(item,null);
            }
            lvGeocaches.EndUpdate();
        }

        private void displayAllColumnsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lvGeocaches.BeginUpdate();
            foreach (ToolStripMenuItem item in displayToolStripMenuItem.DropDownItems)
            {
                if (item.Tag == null)
                    continue;
                item.Checked = false;
                DisplaySubMenuItem_Click(item, null);
            }
            lvGeocaches.EndUpdate();
        }

        private void btnCheckSize_Click(object sender, EventArgs e)
        {
            cbSizeU.Checked = true;
            cbSizeL.Checked = true;
            cbSizeM.Checked = true;
            cbSizeS.Checked = true;
            cbSizeO.Checked = true;
            cbSizeR.Checked = true;
            cbSizeV.Checked = true;
        }

        private void btnUncheckSize_Click(object sender, EventArgs e)
        {
            cbSizeU.Checked = false;
            cbSizeL.Checked = false;
            cbSizeM.Checked = false;
            cbSizeS.Checked = false;
            cbSizeO.Checked = false;
            cbSizeR.Checked = false;
            cbSizeV.Checked = false;
        }

        private void btnCheckType_Click(object sender, EventArgs e)
        {
            cbType1.Checked = true;
            cbType2.Checked = true;
            cbType3.Checked = true;
            cbType4.Checked = true;
            cbType5.Checked = true;
            cbType6.Checked = true;
            cbType7.Checked = true;
            cbType8.Checked = true;
            cbType9.Checked = true;
            cbType10.Checked = true;
            cbType11.Checked = true;
            cbType12.Checked = true;
            cbType13.Checked = true;
            cbType14.Checked = true;
            cbType15.Checked = true;
        }

        private void btnUncheckType_Click(object sender, EventArgs e)
        {
            cbType1.Checked = false;
            cbType2.Checked = false;
            cbType3.Checked = false;
            cbType4.Checked = false;
            cbType5.Checked = false;
            cbType6.Checked = false;
            cbType7.Checked = false;
            cbType8.Checked = false;
            cbType9.Checked = false;
            cbType10.Checked = false;
            cbType11.Checked = false;
            cbType12.Checked = false;
            cbType13.Checked = false;
            cbType14.Checked = false;
            cbType15.Checked = false;
        }

        private void btnResetFilters_Click(object sender, EventArgs e)
        {
            // Load empty filter
            // WarNewFilter
            DialogResult dialogResult = MessageBox.Show(GetTranslator().GetString("WarNewFilter"),
                GetTranslator().GetString("WarTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                Filter = CreateFooFilter();
                UpdateFromFilter();
                PostTreatmentFilter();
            }
        }

        private CacheFilter CreateFooFilter()
        {
            return CreateFooFilter("CBNewFilter");
        }

        private CacheFilter CreateFooFilter(String keylabel)
        {
            CacheFilter foo = new CacheFilter();
            foo.DefaultFilter();
            foo._bToIgnore = true;
            foo._description = GetTranslator().GetString(keylabel);
            return foo;
        }
        
        /// <summary>
        /// Center map on coordinates
        /// </summary>
        /// <param name="lat">center latitude</param>
        /// <param name="lon">center longitude</param>
        public void CenterOnCarto(String lat, String lon)
        {
            try
            {
                double dlat = MyTools.ConvertToDouble(lat);
                double dlon = MyTools.ConvertToDouble(lon);
                ShowCacheMapInCacheDetail();
                _cacheDetail._gmap.Position = new PointLatLng(dlat, dlon);
            }
            catch (Exception)
            {

            }
        }

        private void btnCenterOnCarto_Click(object sender, EventArgs e)
        {
            String lat = "";
            String lon = "";
            
            if ((textBox3latlonnear.Text == "") || (ParameterObject.TryToConvertCoordinates(textBox3latlonnear.Text, ref lat, ref lon)))
            {
                if (lat == "") lat = "0";
	            if (lon == "") lon = "0";
	            CenterOnCarto(lat, lon);
            }
            else
            {
            	// Exception	
            	String err = String.Format(GetTranslator().GetString("ErrWrongParameter").Replace("#","\r\n"),
            	                           GetTranslator().GetString("LblLatLon"),
            	                           ParameterObject.ParameterType.Coordinates/*just an error message*/, 
            	                           textBox3latlonnear.Text,
            	                           "");
            	MsgActionError(this, err);
            }

        }
        

        /// <summary>
        /// Post treatment to call after all caches loading
        /// </summary>
        public void PostTreatmentLoadCache()
        {
            // Populate filters for Region & Country
            _dicoCountryState.Clear();
            
            cbGCCodeList.Items.Clear();

            // Populate with bookrmarks if any
            ListTextCoord bmarks = GetBookmarks();
            if (bmarks != null)
            {
                foreach (TextCoord coord in bmarks._TextCoords)
                {
                    ComboItem item = new ComboItem();
                    item.Text = "*** " + coord._Name + " ***";
                    item.Value = coord;
                    cbGCCodeList.Items.Add(item);
                }
            }

            // Populate with cache list
            foreach (KeyValuePair<String, Geocache> paire in _caches)
            {
                // GCCode
                cbGCCodeList.Items.Add(paire.Key + " " + paire.Value._Name);

                // Country
                if (_dicoCountryState.ContainsKey(paire.Value._Country) == false)
                {
                    _dicoCountryState.Add(paire.Value._Country, new List<string>());
                }

                // State
                List<String> regions = _dicoCountryState[paire.Value._Country];
                if (regions.Contains(paire.Value._State) == false)
                {
                    regions.Add(paire.Value._State);
                }

                // does it have an associated _ocd element ?
                if (_od._OfflineData.ContainsKey(paire.Value._Code))
                {
                    paire.Value._Ocd = _od._OfflineData[paire.Value._Code];
                }
            }

            if (cbGCCodeList.Items.Count != 0)
                cbGCCodeList.SelectedIndex = 0;
            cbFilterNear.Checked = false;
            
            PopulateComboCountryState(0,0);
        }

        void PopulateComboCountryState(int ico, int ist)
        {
            // Clear all
            comboBoxCountry.Items.Clear();
            comboBoxCountry.Items.Add(GetTranslator().GetString("LblAnyCountry"));
            comboBoxState.Items.Clear();
            comboBoxState.Items.Add(GetTranslator().GetString("LblAnyState"));

            foreach (KeyValuePair<String, List<String>> paire in _dicoCountryState)
            {
                if (paire.Key != "")
                    comboBoxCountry.Items.Add(paire.Key);
            }

            // Select elements
            if (comboBoxCountry.Items.Count > ico)
                comboBoxCountry.SelectedIndex = ico;

            if (comboBoxState.Items.Count > ist)
                comboBoxState.SelectedIndex = ist;
        }

        TextCoord GetTextCoordFromName(String name)
        {
            // For sure a bookmark
            ListTextCoord bmarks = GetBookmarks();
            foreach (TextCoord crd in bmarks._TextCoords)
            {
                if (name == crd._Name)
                {
                    return crd;
                }
            }
            return null;
        }

        private void cbGCCodeList_SelectedValueChanged(object sender, EventArgs e)
        {
            Object obj = cbGCCodeList.Items[cbGCCodeList.SelectedIndex];
            if (obj.GetType() == typeof(ComboItem))
            {
                // it's a bookmark
                ComboItem item = obj as ComboItem;
                if (item != null)
                {
                    // For sure a bookmark
                    TextCoord crd = item.Value as TextCoord;
                    if (crd != null)
                    {
                        textBox3latlonnear.Text = crd._Lat + " " + crd._Lon;
                    }
                }
            }
            else
            {
                // it's a regular geocache
                String lbl = obj.ToString();
                int pos = lbl.IndexOf(" ");
                String code = lbl.Substring(0, pos);
                try
                {
                    // Is it a cache code or a bookmark ?
                    if (_caches.ContainsKey(code))
                    {
                        Geocache geo = _caches[code];
                        textBox3latlonnear.Text = geo._Latitude + " " + geo._Longitude;
                    }
                }
                catch (Exception)
                {
                    MsgActionError(this, GetTranslator().GetString("ErrorCode") + ": " + code);
                }
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            CheckIfNotAlready(cbFilterNear);
            
            // Et on lance les conversions si ok
            textBoxlatlonnearconv.Text = ParametersChanger.ConvertCoordinates(textBox3latlonnear.Text);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            CheckIfNotAlready(cbFilterNear);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            CheckIfNotAlready(cbFilterNear);
        }

        private void cbFilterNear_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePageIfChecked(sender, tabPage10Near);
        }

        private void SetLimitCacheCloserThan(object sender, EventArgs e)
        {
            DefineOneSelAsNearOrigin();
            MenuItem mnu = sender as MenuItem;
            if (mnu != null)
            {
                int d = (int)(mnu.Tag);
                textBox2neardist.Text = d.ToString();
                DoFilter();
            }
            else
            {
	            ToolStripMenuItem tsi = sender as ToolStripMenuItem;
	            if (tsi != null)
	            {
	                int d = (int)(tsi.Tag);
	                textBox2neardist.Text = d.ToString();
	                DoFilter();
	            }
            }
        }

        private void DefineAsLocationForNearFilter(object sender, EventArgs e)
        {
            DefineOneSelAsNearOrigin();
        }

        private void DefineOneSelAsNearOrigin()
        {
            if (lvGeocaches.SelectedItems.Count == 1)
            {
                String code = "???";

                EXListViewItem lstvItem2 = lvGeocaches.Items[lvGeocaches.SelectedIndices[0]] as EXListViewItem;
                code = lstvItem2.Text;
                int i = cbGCCodeList.FindString(code);

                if (i == -1)
                    MsgActionError(this, GetTranslator().GetString("ErrorCode"));
                else
                {
                    cbGCCodeList.SelectedIndex = i;
                }
            }
        }

        private void displayAPathConnectingAllDisplayedCachesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SolverAndDisplayTSP();
        }

        private void btnMultipleFilters_Click(object sender, EventArgs e)
        {
            Cursor old = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            List<EXListViewItem> forcedList = new List<EXListViewItem>();

            // Attention, il peut y avoir le filtre courant dans cette liste, dans ce cas là, il convient de le remplacer
            // On regarde s'il est dans la liste
            bool bPlaceHolderPresent = false;
            CacheFilter filterForPlaceholder = null;
            foreach (object obj in clbMltipleFilters.CheckedItems)
            {   
                CacheFilter flt = obj as CacheFilter;
                if (flt._bToIgnore)
                	bPlaceHolderPresent = true;
            }
            if (bPlaceHolderPresent && UpdateFilter())
            {
            	filterForPlaceholder = Filter;
            }
            
            if (rdBtnOR.Checked)
            {
                ChainedFiltersOR chnf = new ChainedFiltersOR(clbMltipleFilters.CheckedItems, filterForPlaceholder);
                // Build list of caches
                foreach (EXListViewItem item in _listViewCaches)
                {
                    Geocache cache = _caches[item.Text];
                    if (chnf.ToBeDisplayed(cache))
                    {
                        forcedList.Add(item);
                    }
                }
            }
            else
            {
                ChainedFiltersAND chnf = new ChainedFiltersAND(clbMltipleFilters.CheckedItems, filterForPlaceholder);
                // Build list of caches
                foreach (EXListViewItem item in _listViewCaches)
                {
                    Geocache cache = _caches[item.Text];
                    if (chnf.ToBeDisplayed(cache))
                    {
                        forcedList.Add(item);
                    }
                }
            }

            // Display this list
            PopulateListViewCache(forcedList);

            Cursor.Current = old;
        }

        private void comboBoxCountry_SelectedValueChanged(object sender, EventArgs e)
        {
            // By the way, let's populate the state based on the dico
            comboBoxState.Items.Clear();
            comboBoxState.Items.Add(GetTranslator().GetString("LblAnyState"));
            if (comboBoxCountry.SelectedIndex != 0)
            {
                if (_dicoCountryState.ContainsKey(comboBoxCountry.Text))
                {
                    List<String> regions = _dicoCountryState[comboBoxCountry.Text];
                    foreach (String r in regions)
                    {
                        if (r != "")
                            comboBoxState.Items.Add(r);
                    }
                }
            }
            comboBoxState.SelectedIndex = 0;

            if (comboBoxCountry.SelectedIndex != 0)
                CheckIfNotAlready(cbFilterRegionState);
        }

        private void comboBoxState_SelectedValueChanged(object sender, EventArgs e)
        {
            if (comboBoxState.SelectedIndex != 0)
                CheckIfNotAlready(cbFilterRegionState);
        }

        private void cbFilterRegionState_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePageIfChecked(sender, tabPage12Region);
        }

        private void exportImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportImagesFromCaches(false);
        }

        /// <summary>
        /// Export previously downloaded images from caches
        /// </summary>
        /// <param name="selection">if true, only selected caches are used</param>
        public void ExportImagesFromCaches(bool selection)
        {
            ImageExportDialog imgexport = new ImageExportDialog(this);
            imgexport.Font = this.Font;
            imgexport.Icon = this.Icon;

            bool bLinux = false;
            if (MainWindow.IsLinux)
            {
                imgexport.radioButton2imgexport.Enabled = false;
                bLinux = true;
            }

            if (imgexport.ShowDialog() == DialogResult.OK)
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.SelectedPath = GetInternalDataPath();
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    if (imgexport.radioButton1imgexport.Checked)
                        ExportImagesGarmin(fbd, bLinux, imgexport.radioButton6imgexport.Checked, imgexport.radioButton7imgexport.Checked, imgexport.cbSpoilerGarminNormalimgexport.Checked, selection);
                    else if (imgexport.radioButton2imgexport.Checked)
                        ExportImagesGeocoded(fbd, imgexport.radioButton6imgexport.Checked, imgexport.radioButton7imgexport.Checked, selection);
                    else if (imgexport.radioButton3imgexport.Checked)
                        ExportImagesExploristGC(fbd, imgexport.radioButton6imgexport.Checked, imgexport.radioButton7imgexport.Checked, selection);
                    else
                        MSG("WTF ?!");
                }
            }
        }

        /// <summary>
        /// Export previously downloaded images from caches (eXplorist format) 
        /// </summary>
        /// <param name="fbd">folder browser dialog</param>
        /// <param name="bOnlySpoilers">if true, only spoilers are exported</param>
        /// <param name="bUseSpoilerKeywords">if true, keywords for spoilers are used</param>
        /// <param name="selection">if true, only selection if exported</param>
        public void ExportImagesExploristGC(FolderBrowserDialog fbd, bool bOnlySpoilers, bool bUseSpoilerKeywords, bool selection)
        {
            String path = fbd.SelectedPath;
            if (IsPathOnRemovableDrive(fbd.SelectedPath))
            {
                DialogResult dialogResult = MessageBox.Show(GetTranslator().GetString("MsgRemovableDrive"),
                        GetTranslator().GetString("AskConfirm"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult != DialogResult.Yes)
                {
                    return;
                }
            }

            _ThreadProgressBarTitle = GetTranslator().GetString("DlgImgExportExploristGC");
            CreateThreadProgressBar();

            Directory.SetCurrentDirectory(path);
            String gpxmagellan = "Geocaches";
            Directory.CreateDirectory(gpxmagellan);
            String imgMagellan = "Images" + Path.DirectorySeparatorChar + "Geocaches";
            Directory.CreateDirectory(imgMagellan);
            Directory.SetCurrentDirectory(imgMagellan);

            String date = DateTime.Now.ToString("yyyyMMddhhmmss");
            String fileRadix = path + Path.DirectorySeparatorChar + gpxmagellan + Path.DirectorySeparatorChar + date + "GC.gpx";
            
            ExportGPXBrutal(selection, fileRadix, this.ExportGPXInternalProcessing, bOnlySpoilers, bUseSpoilerKeywords);

            KillThreadProgressBar();

            String pexport = path;
            MsgActionOk(this, GetTranslator().GetString("MBExportDone") + "\r\n" + pexport);
            MyTools.StartInNewThread(pexport);
            
        }

        private bool IsAValidSpoiler(List<String> keywordsspoiler, String name)
        {
            // Get image name
            String n = name.ToLower();
            // Do we have a match with a keyword?
            foreach (String s in keywordsspoiler)
            {
                if (n.Contains(s))
                {
                    // We have a match
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Export previously downloaded images from caches (Garmin format)
        /// </summary>
        /// <param name="fbd">folder browser dialog</param>
        /// <param name="bLinux">if true, platform is Linux</param>
        /// <param name="bOnlySpoilers">if true, only spoilers are exported</param>
        /// <param name="bUseSpoilerKeywords">if true, keywords are used for spoiler detection</param>
        /// <param name="bSpoilerGarminNormal">if true, spoilers are exported as standard images and not spoilers</param>
        /// <param name="selection">if true, only selected caches are exported</param>
        public void ExportImagesGarmin(FolderBrowserDialog fbd, bool bLinux, bool bOnlySpoilers, bool bUseSpoilerKeywords, bool bSpoilerGarminNormal, bool selection)
        {
            /*
                http://garmin.blogs.com/softwareupdates/2012/01/geocaching-with-photos.html

                Works With All Geocaches
                You can also take advantage of geocache photos on your Garmin handheld for geocaches obtained from a source 
                other than OpenCaching.com, it just takes some work. A geocache’s photos, JPEG only, need to be placed on the handheld’s
                mass storage in the following manner

                Photos
                    \Garmin\GeocachePhotos\Last Character\Second To Last Character\Full Code\

                Spoiler Photos
                    <Photos Path>\Spoilers\
                For example, photos for a geocache with code OXZTXGC would be placed under the path

                    \Garmin\GeocachePhotos\C\G\OXZTXGC\

                And spoilers would be placed under

                    \Garmin\GeocachePhotos\C\G\OXZTXGC\Spoilers

                If the geocache has only three characters total, a 0 (zero) is used for the second to last character.
                For example, photos for a geocache with code OXR would be placed under the path

                    \Garmin\GeocachePhotos\R\0\OXR\
            */
            if (IsPathOnRemovableDrive(fbd.SelectedPath))
            {
                DialogResult dialogResult = MessageBox.Show(GetTranslator().GetString("MsgRemovableDrive"),
                        GetTranslator().GetString("AskConfirm"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult != DialogResult.Yes)
                {
                    return;
                }
            }

            _ThreadProgressBarTitle = GetTranslator().GetString("DlgImgExportGarmin");
            CreateThreadProgressBar();
            Directory.SetCurrentDirectory(fbd.SelectedPath);

            String imgGarmin = "GeocachePhotos";
            Directory.CreateDirectory(imgGarmin);
            Directory.SetCurrentDirectory(imgGarmin);

            // Now export
            List<Geocache> lst = null;
            if (selection)
                lst = GetSelectedCaches();
            else
                lst = GetDisplayedCaches();

            String offdatapath = GetUserDataPath() + Path.DirectorySeparatorChar + "Offline" + Path.DirectorySeparatorChar;
            List<String> keywordsspoiler = GetSpoilerKeyWordsAlways();
            String imgPath;
            foreach (Geocache geo in lst)
            {
                // Save images & spoilers and geotag them
                if (_od._OfflineData.ContainsKey(geo._Code))
                {
                    // Do we have images to load and save?
                    OfflineCacheData ocd = _od._OfflineData[geo._Code];
                    String lastS = geo._Code.Substring(geo._Code.Length - 1, 1);
                    String prelastS = geo._Code.Substring(geo._Code.Length - 2, 1);

                    if ((ocd._ImageFilesSpoilers.Count != 0) || (ocd._ImageFilesFromDescription.Count != 0))
                    {
                        int ipic = 0;
                        // Spoilers
                        if (ocd._ImageFilesSpoilers.Count != 0)
                        {
                            imgPath = lastS + Path.DirectorySeparatorChar + prelastS + Path.DirectorySeparatorChar + geo._Code;
                            if (bSpoilerGarminNormal == false)
                                imgPath += Path.DirectorySeparatorChar + "Spoilers";
                            Directory.CreateDirectory(imgPath);

                            foreach (KeyValuePair<string, OfflineImageWeb> paire in ocd._ImageFilesSpoilers)
                            {
                                bool bKeep = true;
                                if (bOnlySpoilers && bUseSpoilerKeywords)
                                    bKeep = IsAValidSpoiler(keywordsspoiler, paire.Value._name);
                                if (bKeep)
                                {
                                    String radix =
                                            //geo._Name + "_" + // PATCH
                                            geo._Code + "_" +
                                            String.Format("{0:000}", ipic) + "_" +
                                            HtmlAgilityPack.HtmlEntity.DeEntitize(paire.Value._name) + ".jpg";
                                    radix = MyTools.SanitizeFilename(radix);

                                    String fjpeg = imgPath + Path.DirectorySeparatorChar + radix;
                                    ipic++;
                                    try
                                    {
                                        // Load image
                                        Image img = Image.FromFile(offdatapath + paire.Value._localfile);

                                        // Save to Jpeg
                                        try
                                        {
                                            // Test with this filename
                                            img.Save(fjpeg, System.Drawing.Imaging.ImageFormat.Jpeg);
                                        }
                                        catch (Exception)
                                        {
                                            // Ok standard filename, something wrong happened
                                            fjpeg = imgPath + Path.DirectorySeparatorChar + geo._Code + "_" + String.Format("{0:000}", ipic) + ".jpg";
                                            img.Save(fjpeg, System.Drawing.Imaging.ImageFormat.Jpeg);
                                        }
                                        img.Dispose();

                                        if (!bLinux)
                                            MyTools.WriteCoordinatesToImage(fjpeg, geo._dLatitude, geo._dLongitude);
                                    }
                                    catch (Exception)
                                    {
                                        // Do nothing
                                    }
                                }
                            }
                        }

                        // Images from description
                        if ((!bOnlySpoilers) && (ocd._ImageFilesFromDescription.Count != 0))
                        {
                            imgPath = lastS + Path.DirectorySeparatorChar + prelastS + Path.DirectorySeparatorChar + geo._Code;
                            Directory.CreateDirectory(imgPath);
                            foreach (KeyValuePair<string, string> paire in ocd._ImageFilesFromDescription)
                            {
                                String fjpeg = imgPath + Path.DirectorySeparatorChar + geo._Code + "_" + String.Format("{0:000}", ipic) + ".jpg";
                                ipic++;
                                try
                                {
                                    // Load image
                                    Image img = Image.FromFile(offdatapath + paire.Value);

                                    // Save to Jpeg
                                    img.Save(fjpeg, System.Drawing.Imaging.ImageFormat.Jpeg);
                                    img.Dispose();
                                }
                                catch (Exception)
                                {
                                    // Do nothing
                                }
                            }
                        }
                    }
                }
            }
            // Cleanup
            imgPath = fbd.SelectedPath + Path.DirectorySeparatorChar + imgGarmin;
            CleanContent(imgPath, false);

            KillThreadProgressBar();

            String pexport = fbd.SelectedPath + Path.DirectorySeparatorChar + imgGarmin;
            MsgActionOk(this, GetTranslator().GetString("MBExportDone") + "\r\n" + pexport);
            MyTools.StartInNewThread(pexport);
            
        }

        private static bool CleanContent(String path, bool bDelete)
        {
            // Check current files
            String[] fics = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
            if (fics.Length != 0)
                return false; // not empty !!!

            // Ok, no file in the current folder, any other folder ?
            // Check current directories
            String[] dirs = Directory.GetDirectories(path);
            if (dirs.Length != 0)
            {
                bool bEmpty = true;
                foreach (String p in dirs)
                {
                    bEmpty = bEmpty & CleanContent(p, true);
                }
                if (bEmpty && bDelete)
                {
                    // Kill myself !
                    Directory.Delete(path);
                    return true;
                }
                else
                    return false;
            }
            else if (bDelete)
            {
                // Kill myself !
                Directory.Delete(path);
                return true;
            }

            return false;
            //String[] fics = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// Export previously downloaded images from caches(Geocoded)
        /// </summary>
        /// <param name="fbd">folder browser dialog</param>
        /// <param name="bOnlySpoilers">if true, only spoilers are exported</param>
        /// <param name="bUseSpoilerKeywords">if true, keywords are used for spoiler detection</param>
        /// <param name="selection">if true, only selected caches are exported</param>
        public void ExportImagesGeocoded(FolderBrowserDialog fbd, bool bOnlySpoilers, bool bUseSpoilerKeywords, bool selection)
        {
            if (IsPathOnRemovableDrive(fbd.SelectedPath))
            {
                DialogResult dialogResult = MessageBox.Show(GetTranslator().GetString("MsgRemovableDrive"),
                        GetTranslator().GetString("AskConfirm"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult != DialogResult.Yes)
                {
                    return;
                }
            }

            _ThreadProgressBarTitle = GetTranslator().GetString("DlgImgExportGeocoded");
            CreateThreadProgressBar();
            Directory.SetCurrentDirectory(fbd.SelectedPath);
            String imgPath = "GeocodedImages";
            Directory.CreateDirectory(imgPath);

            // Now export
            List<Geocache> lst = null;
            if (selection)
                lst = GetSelectedCaches();
            else
                lst = GetDisplayedCaches();
            String offdatapath = GetUserDataPath() + Path.DirectorySeparatorChar + "Offline" + Path.DirectorySeparatorChar;
            List<String> keywordsspoiler = GetSpoilerKeyWordsAlways();
            foreach (Geocache geo in lst)
            {
                // Save images & spoilers and geotag them
                if (_od._OfflineData.ContainsKey(geo._Code))
                {
                    // Do we have images to load and save?
                    OfflineCacheData ocd = _od._OfflineData[geo._Code];
                    if (ocd._ImageFilesSpoilers.Count != 0)
                    {
                        int ipic = 0;
                        foreach (KeyValuePair<string, OfflineImageWeb> paire in ocd._ImageFilesSpoilers)
                        {
                            bool bKeep = true;
                            if (bOnlySpoilers && bUseSpoilerKeywords)
                                bKeep = IsAValidSpoiler(keywordsspoiler, paire.Value._name);
                            if (bKeep)
                            {
                                String radix = geo._Code + "_" + String.Format("{0:000}", ipic) + "_" +
                                        HtmlAgilityPack.HtmlEntity.DeEntitize(paire.Value._name) + ".jpg";
                                radix = MyTools.SanitizeFilename(radix);

                                WriteGeocodedImage(imgPath, offdatapath, geo, ref ipic, paire.Value._localfile, radix);
                            }
                        }
                    }

                    if ((!bOnlySpoilers) && (ocd._ImageFilesFromDescription.Count != 0))
                    {
                        int ipic = 0;

                        foreach (KeyValuePair<string, string> paire in ocd._ImageFilesFromDescription)
                        {
                            String radix = geo._Code + "_DESC_" + String.Format("{0:000}", ipic) + ".jpg";
                            radix = MyTools.SanitizeFilename(radix);

                            WriteGeocodedImage(imgPath, offdatapath, geo, ref ipic, paire.Value, radix);
                        }
                    }
                }
            }
            KillThreadProgressBar();
            String pexport = fbd.SelectedPath + Path.DirectorySeparatorChar + "GeocodedImages";
            MsgActionOk(this, GetTranslator().GetString("MBExportDone") + "\r\n" + pexport);
            MyTools.StartInNewThread(pexport);
            
        }

        private void WriteGeocodedImage(String imgPath, String offdatapath, Geocache geo, ref int ipic, String localfile, String radix)
        {
            String fjpeg = imgPath + Path.DirectorySeparatorChar + radix;
            ipic++;
            try
            {
                // Load image
                Image img = Image.FromFile(offdatapath + localfile);

                // Save to Jpeg
                try
                {
                    // Test with this filename
                    img.Save(fjpeg, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                catch (Exception)
                {
                    // Ok standard filename, something wrong happened
                    fjpeg = imgPath + Path.DirectorySeparatorChar + geo._Code + "_" + String.Format("{0:000}", ipic) + ".jpg";
                    img.Save(fjpeg, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                img.Dispose();

                MyTools.WriteCoordinatesToImage(fjpeg, geo._dLatitude, geo._dLongitude);
            }
            catch (Exception)
            {
                // Do nothing
            }
        }

        private void cbFilterAttributeIn_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePageIfChecked(sender, tabPage13AttIn);
        }

        private void cbFilterAttributeOut_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePageIfChecked(sender, tabPage14AttOut);
        }

        private void cbFilterList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 0;
            toolTip1.InitialDelay = 0;
            toolTip1.ReshowDelay = 0;
            toolTip1.ShowAlways = true;
            CacheFilter fil = (CacheFilter)(cbFilterList.Items[cbFilterList.SelectedIndex]);
            if ((fil != null) && (fil._descriptionDetails != ""))
                toolTip1.SetToolTip(cbFilterList, fil._descriptionDetails);
            else
                toolTip1.SetToolTip(cbFilterList, cbFilterList.Items[cbFilterList.SelectedIndex].ToString());
        }

        private void clbMltipleFilters_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i = clbMltipleFilters.SelectedIndex;
            if (i != -1)
            {
                CacheFilter fil = (CacheFilter)(clbMltipleFilters.Items[i]);
                if (fil != null)
                    textBoxMultifilterExplain.Text = fil._descriptionDetails;
            }
        }

        private List<String> GetSpoilerKeyWords()
        {
            if (ConfigurationManager.AppSettings["usespoilerskeywords"] != "True")
                return new List<string>();

            return GetSpoilerKeyWordsAlways();
        }

        private List<string> GetSpoilerKeyWordsAlways()
        {
            List<String> lst = new List<string>();
            String keystxt = ConfigurationManager.AppSettings["spoilerskeywords"];
            if (keystxt == "")
                return lst;
            try
            {
                List<string> keys = keystxt.Split('¤').ToList<string>();
                if (keys != null)
                {
                    foreach (String key in keys)
                    {
                        if (key != "")
                        {
                            lst.Add(key);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return lst;
        }

        private void configureSpoilersDownloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigureSpoilerDownload();
        }

        /// <summary>
        /// Configure options for spoiler download
        /// </summary>
        /// <returns>false if operation canceled</returns>
        public bool ConfigureSpoilerDownload()
        {
            SpoilerKeywords dlg = new SpoilerKeywords(this);
            dlg._sSpoilerDefaultKeywords = _sSpoilerDefaultKeywords;
            dlg.Icon = this.Icon;
            dlg.Font = this.Font;
            if (ConfigurationManager.AppSettings["usespoilerskeywords"] == "True")
            {
                dlg.checkBox1spoilerkw.Checked = true;
            }
            else
            {
                dlg.checkBox1spoilerkw.Checked = false;
            }
            dlg.ConsiderSpoilerKeywords();
			if (ConfigurationManager.AppSettings["getimagesfromgallery"] == "True")
            {
                dlg.checkBoxGallery.Checked = true;
            }
            else
            {
                dlg.checkBoxGallery.Checked = false;
            }

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (dlg.checkBox1spoilerkw.Checked)
                {
                    UpdateConfFile("usespoilerskeywords", "True");
                }
                else
                {
                    UpdateConfFile("usespoilerskeywords", "False");
                }
                if (dlg.checkBoxGallery.Checked)
                {
                    UpdateConfFile("getimagesfromgallery", "True");
                }
                else
                {
                    UpdateConfFile("getimagesfromgallery", "False");
                }
                switch (dlg.comboBox1spoilerkw.SelectedIndex)
                {
                    case 0:
                        UpdateConfFile("spoilerdelaydownload", "0");
                        break;
                    case 1:
                        UpdateConfFile("spoilerdelaydownload", "1");
                        break;
                    case 2:
                        UpdateConfFile("spoilerdelaydownload", "3");
                        break;
                    case 3:
                        UpdateConfFile("spoilerdelaydownload", "5");
                        break;
                    case 4:
                        UpdateConfFile("spoilerdelaydownload", "10");
                        break;
                    default:
                        UpdateConfFile("spoilerdelaydownload", "0");
                        break;
                }
                UpdateConfFile("spoilerskeywords", dlg.GetKeywords());
                return true;
            }
            else
            	return false;
        }

        private void performOfflineDataMaintenanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PerformMaintenanceOnOfflineData();
        }

        private void deleOfflineDataForMissingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String offdatapath = GetUserDataPath() + Path.DirectorySeparatorChar + "Offline";
            String sMissingCache = GetTranslator().GetString("LblMissingCacheForOfflineData") + "\r\n\r\n";
            // Check if each cache in _od is present in _caches
            List<OfflineCacheData> toPurge = new List<OfflineCacheData>();
            foreach (KeyValuePair<String, OfflineCacheData> paire in _od._OfflineData)
            {
                if (_caches.ContainsKey(paire.Key) == false)
                {
                    OfflineCacheData ocd = paire.Value;

                    toPurge.Add(ocd);

                    string name = "";
                    if (_caches.ContainsKey(ocd._Code))
                        name = _caches[ocd._Code]._Name;
                    else
                        name = GetTranslator().GetString("LblCacheNotLoaded");
                    sMissingCache += ocd._Code + " - " + name + "\r\n";
                }
            }

            if (toPurge.Count != 0)
            {
                DialogResult dialogResult = MyMessageBox.Show(
                    sMissingCache,
                    GetTranslator().GetString("LblConfirmDelMissing"),
                    MessageBoxIcon.Question, GetTranslator());
                if (dialogResult == DialogResult.Yes)
                {
                    foreach (OfflineCacheData ocd in toPurge)
                    {
                        ocd.PurgeFiles(offdatapath);
                        RemoveAssociationOcdCache(ocd._Code, null);
                    }
                    _od.Serialize(_odfile);

                    UpdateListViewOfflineIcons(toPurge);

                    MsgActionDone(this);
                }
            }
            else
            {
                MsgActionOk(this, GetTranslator().GetString("LblNothingToDo"));
            }

            
        }

        private void PerformMaintenanceOnOfflineData()
        {
            List<String> missingFiles = new List<string>();
            String smissingFiles = GetTranslator().GetString("LblDelOrphanFiles") + "\r\n\r\n";
            List<OfflineCacheData> incompleteOcd = new List<OfflineCacheData>();
            String sincompleteOcd = GetTranslator().GetString("LblDelInconsistentOCD") + "\r\n\r\n";
            string offdatapath = GetUserDataPath() + Path.DirectorySeparatorChar + "Offline" + Path.DirectorySeparatorChar;

            // Parse ocd list and store all pictures in a list
            List<String> ocdFiles = new List<string>();
            foreach (KeyValuePair<String, OfflineCacheData> paire in _od._OfflineData)
            {
                OfflineCacheData ocd = paire.Value;
                if (ocd.HasMissingFiles(offdatapath))
                {
                    incompleteOcd.Add(ocd);
                    string name = "";
                    if (_caches.ContainsKey(ocd._Code))
                        name = _caches[ocd._Code]._Name;
                    else
                        name = GetTranslator().GetString("LblCacheNotLoaded");
                    sincompleteOcd += ocd._Code + " - " + name + "\r\n";
                }

                foreach (KeyValuePair<String, String> p1 in ocd._ImageFilesFromDescription)
                {
                    ocdFiles.Add(p1.Value);
                }
                foreach (KeyValuePair<String, OfflineImageWeb> p2 in ocd._ImageFilesSpoilers)
                {
                    ocdFiles.Add(p2.Value._localfile);
                }
            }

            string[] filePaths = Directory.GetFiles(offdatapath, "*_*", SearchOption.TopDirectoryOnly);
            foreach (string f in filePaths)
            {
                FileAttributes attr = File.GetAttributes(f);
                //detect whether its a directory or file
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    // We pass
                }
                else
                {
                    FileInfo fi = new FileInfo(f);
                    String radix = fi.Name;
                    if (!ocdFiles.Contains(radix))
                    {
                        missingFiles.Add(f);
                        smissingFiles += radix + "\r\n";
                    }
                }
            }

            bool todo = false;
            if (incompleteOcd.Count != 0)
            {
                todo = true;
                DialogResult dialogResult = MyMessageBox.Show(sincompleteOcd,
                        GetTranslator().GetString("AskConfirm"), MessageBoxIcon.Question, GetTranslator());
                if (dialogResult == DialogResult.Yes)
                {
                    foreach (OfflineCacheData ocd in incompleteOcd)
                    {
                        ocd.PurgeFiles(offdatapath);
                        if (ocd.IsEmpty()) // si l'OCD est vide, autant le virer !
                        {
                            RemoveAssociationOcdCache(ocd._Code, null);
                        }
                    }
                    _od.Serialize(_odfile);

                    // Update icons
                    UpdateListViewOfflineIcons(incompleteOcd);

                    MsgActionDone(this);
                }
            }

            if (missingFiles.Count != 0)
            {
                todo = true;
                DialogResult dialogResult = MyMessageBox.Show(smissingFiles,
                        GetTranslator().GetString("AskConfirm"), MessageBoxIcon.Question, GetTranslator());
                if (dialogResult == DialogResult.Yes)
                {
                    foreach (String f in missingFiles)
                    {
                        try
                        {
                            File.Delete(f);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    MsgActionDone(this);
                }
            }

            if (!todo)
            {
                MsgActionOk(this, GetTranslator().GetString("LblNothingToDo"));
            }
        }

        private void displayCachesWithoutOfflineDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CustomFilterNoOfflineData fltr = new CustomFilterNoOfflineData();
            fltr._od = _od;
            ExecuteCustomFilter(fltr);
        }

        /// <summary>
        /// Return bookmarks
        /// </summary>
        /// <returns>list of bookmarks</returns>
        public ListTextCoord GetBookmarks()
        {
            ListTextCoord bmarks = null;
            try
            {
                String bmarksfile = GetUserDataPath() + Path.DirectorySeparatorChar + "Bookmarks.dat";

                if (File.Exists(bmarksfile))
                    bmarks = ListTextCoord.Deserialize(bmarksfile);
                else
                    bmarks = new ListTextCoord();
            }
            catch (Exception exc2)
            {
                bmarks = new ListTextCoord();
                ShowException("", "Loading bookmarks", exc2);
            }
            return bmarks;
        }

        /// <summary>
        /// Update map overlay with bookmarks
        /// </summary>
        /// <param name="bmarks">list of bookmarks</param>
        public void UpdateBookmarkOverlay(ListTextCoord bmarks)
        {
            if (bmarks == null)
                return;
            Image img = GetImageWithWhiteBackground(_bEnableWhiteBackground, "HomeG", -1, 20);
            _cachesPreviewMap.Overlays[GMapWrapper.BOOKMARKS].Markers.Clear();
            _cacheDetail._gmap.Overlays[GMapWrapper.BOOKMARKS].Markers.Clear();

            // La maison
            CreateSimpleMarkerOnOverlay(GMapWrapper.BOOKMARKS, img, _dHomeLat, _dHomeLon, "Home");
            // Les autres
            foreach (TextCoord bm in bmarks._TextCoords)
            {
                double lat, lon;
                lat = MyTools.ConvertToDouble(bm._Lat);
                lon = MyTools.ConvertToDouble(bm._Lon);
                String tooltip = bm._Name;
                CreateSimpleMarkerOnOverlay(GMapWrapper.BOOKMARKS, img, lat, lon, tooltip);
            }
        }

        private GMapMarkerImage CreateSimpleMarkerOnOverlay(int iOverlay, Image img, double lat, double lon, String tooltip)
        {
            try
            {
                GMapWrapper.gmapMarkerWithImage(
                                 _cachesPreviewMap.Overlays[iOverlay],
                                 img,
                                 lat,
                                 lon,
                                 tooltip);

                GMapMarkerImage mk = GMapWrapper.gmapMarkerWithImage(
                            _cacheDetail._gmap.Overlays[iOverlay],
                            img,
                            lat,
                            lon,
                            tooltip);
                CheckMarkerValidity(mk);

                return mk;
            }
            catch (Exception exc)
            {
            	Log("!!! " + GetException("Creating a marker simple on overlay", exc));
                // Ca a chié !
                // On provoque un crash
                CheckMarkerValidity(null);
                return null;
            }
        }

        /// <summary>
        /// Save bookmarks
        /// </summary>
        /// <param name="bmarks">list of bookmarks</param>
        public void SaveBookmarks(ListTextCoord bmarks)
        {
            try
            {
                String bmarksfile = GetUserDataPath() + Path.DirectorySeparatorChar + "Bookmarks.dat";
                bmarks.Serialize(bmarksfile);
            }
            catch (Exception exc2)
            {
                ShowException("", "Saving bookmarks", exc2);
            }
        }

        private void configureBookmarksToolStripMenuItem_Click(object sender, EventArgs e)
        {
        	CloseCacheDetail();
            FavManager fmgr = new FavManager(this);

            fmgr.Icon = this.Icon;
            fmgr.Font = this.Font;

            fmgr.Show();
        }

        private void DisplayBookmarks()
        {
            CacheFilter oldFilter = Filter;
            bool old_bUseFilter = _bUseFilter;

            CustomFilterBookmarked fltr = new CustomFilterBookmarked();
            fltr._od = _od;
            Filter = fltr;
            _bUseFilter = true;
            PopulateListViewCache(null);

            _bUseFilter = old_bUseFilter;
            Filter = oldFilter;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (radioButton4displayspec.Checked)
                DisplayAll();
            else if (radioButton5displayspec.Checked)
                DisplaySel();
            else
                DisplayBookmarks();
        }

        private void SetAddDelTag(object sender, EventArgs e)
        {
            TagManager tm = new TagManager(this);
            if (tm.ShowDialog() == DialogResult.OK)
            {
                // Save od
                _od.Serialize(_odfile);

                // update display
                //lstv.Invalidate();
            }
        }

        private void SetAddFav(object sender, EventArgs e)
        {
            foreach (Object obj in lvGeocaches.SelectedItems)
            {
                EXListViewItem lstvItem = obj as EXListViewItem;
                SetCacheBookmarkStatus(lstvItem, true);
                
            }
        }

        private void SetDelFav(object sender, EventArgs e)
        {
            foreach (Object obj in lvGeocaches.SelectedItems)
            {
                EXListViewItem lstvItem = obj as EXListViewItem;
                SetCacheBookmarkStatus(lstvItem, false);

            }
        }

        private void SetCacheBookmarkStatus(EXListViewItem lstvItem, bool bBookmark)
        {
            String code = lstvItem.Text;
            Geocache cache = _caches[code];
            OfflineCacheData ocd1 = null;
            if (_od._OfflineData.ContainsKey(code))
            {
                ocd1 = _od._OfflineData[code];
            }
            else
            {
                // non existing
                if (bBookmark)
                {
                    ocd1 = new OfflineCacheData();
                    //ocd1._dateExport = DateTime.Now;
                    //ocd1._Comment = "";
                    ocd1._Code = code;
                    //ocd1._NotDownloaded = true;
                    AssociateOcdCache(code, ocd1, cache);
                }
            }

            if (ocd1 != null)
            {
                ocd1._bBookmarked = bBookmark;
                // UPDATE BEFORE REMOVING THE OCD OBJECT !!!!
                UpdateListViewBookmarkIcons(ocd1, bBookmark);

                if (ocd1.IsEmpty())
                {
                    RemoveAssociationOcdCache(code, cache);
                }

                _od.Serialize(_odfile);

                UpdateListViewOfflineIcons(ocd1);
            }
        }

	
		private void configureToolbarToolStripMenuItem_Click(object sender, EventArgs e)
        {
			ToolbarConfiguration tbc = new ToolbarConfiguration(this);
			tbc.ShowDialog();
		}
		
        private void displayTabForQuickCacheMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (displayTabForQuickCacheMapToolStripMenuItem.Checked)
                UpdateConfFile("displaytabmap", "False");
            else
                UpdateConfFile("displaytabmap", "True");

            UpdateMenuChecks();
            DisplayHideTabMap();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            // Définition de la zone filtre
            // On a juste à afficher la carto et à sélectionner le mode draw area
            ShowCacheMapInCacheDetail();
            _cacheDetail.SelectAreaDrawingMode();
        }

        private void cbFilterArea_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePageIfChecked(sender, tabPage16Area);
        }

        private void rdBtnOR_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTextExplainMultipleFilter();
        }

        private void rdBtnAnd_CheckedChanged(object sender, EventArgs e)
        {
            UpdateTextExplainMultipleFilter();
        }

        private void cbFilterCreation_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePageIfChecked(new object[2] {cbFilterCreation, cbFilterLastLog}, tabPage18Date);
        }

        private void cbFilterLastLog_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePageIfChecked(new object[2] { cbFilterCreation, cbFilterLastLog }, tabPage18Date);
        }

        private void cbCreation_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckIfNotAlready(cbFilterCreation);
        }

        private void cbLastlog_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckIfNotAlready(cbFilterLastLog);
        }

        private void txtDaysCreation_TextChanged(object sender, EventArgs e)
        {
            CheckIfNotAlready(cbFilterCreation);
        }

        private void txtDaysLastLog_TextChanged(object sender, EventArgs e)
        {
            CheckIfNotAlready(cbFilterLastLog);
        }

        private static void SelectStarFromClick(object sender, ComboBox cb)
        {
            PictureBox pb = sender as PictureBox;
            Point pt = pb.PointToClient(Cursor.Position);
            // 5 stars on this picture
            int percent = (int)((double)pt.X / (double)pb.Width * 100.0);

            int i = 8;
            if (percent <= 20)
                i = 0;
            else if (percent <= 30)
                i = 1;
            else if (percent <= 40)
                i = 2;
            else if (percent <= 50)
                i = 3;
            else if (percent <= 60)
                i = 4;
            else if (percent <= 70)
                i = 5;
            else if (percent <= 80)
                i = 6;
            else if (percent <= 90)
                i = 7;
            cb.SelectedIndex = i;
        }

        private void pbDMin_Click(object sender, EventArgs e)
        {
            SelectStarFromClick(sender, comboDMin);
        }

        private void pbDMax_Click(object sender, EventArgs e)
        {
            SelectStarFromClick(sender, comboDMax);
        }

        private void pbTMin_Click(object sender, EventArgs e)
        {
            SelectStarFromClick(sender, comboTMin);
        }

        private void pbTMax_Click(object sender, EventArgs e)
        {
            SelectStarFromClick(sender, comboTMax);
        }

        private void displayDTMatrixToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisplayDTMatrix();
        }

        private void configureStartupLoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartupFilesConfig dlg = new StartupFilesConfig(this);
            dlg.Icon = this.Icon;
            dlg.Font = this.Font;
            DialogResult res = dlg.ShowDialog();
            if (res == DialogResult.OK)
            {
                // Uncheck DB
                UncheckDBMenu();

                // force reload. Not optimized, but fock it...
                LoadBatchOfFiles(dlg._FilesToload.ToArray(), true, false);
            }
            else if (res == DialogResult.Cancel)
            {
                if (dlg._FilesToload.Count != 0)
                {
                    LoadBatchOfFiles(dlg._FilesToload.ToArray(), false, false);
                }
            }
        }

        /// <summary>
        /// Get OCD from a geocache code
        /// </summary>
        /// <param name="code">geocache code</param>
        /// <returns>OCD object (null if not existing)</returns>
        public OfflineCacheData GetOfflineCacheData(String code)
        {
            OfflineCacheData ocd = null;
            if (_od._OfflineData.ContainsKey(code))
            {
                // Do we have images to load and save?
                ocd = _od._OfflineData[code];
            }
            return ocd;
        }

        private void radioButton12_MouseClick(object sender, MouseEventArgs e)
        {
            if (_listExistingTags.Count != 0)
            {
                ContextMenu mnuContextMenu = new ContextMenu();
                MenuItem item = null;
                int i = 0;
                _listExistingTags.Sort();
                foreach (String t in _listExistingTags)
                {
                    if (item == null)
                        mnuContextMenu.MenuItems.Add(UppercaseFirst(t), new EventHandler(ChangeTagInTextBox));
                    else
                        item.MenuItems.Add(UppercaseFirst(t), new EventHandler(ChangeTagInTextBox));

                    i++;
                    if (i == 10)
                    {
                        i = 0;
                        MenuItem item2 = new MenuItem(GetTranslator().GetString("MnuMore"));
                        if (item == null)
                        {
                            mnuContextMenu.MenuItems.Add(item2);
                            item = item2;
                        }
                        else
                        {
                            item.MenuItems.Add(item2);
                            item = item2;
                        }

                    }
                }
                mnuContextMenu.Show(rbFilterOnTagtxtfilter,e.Location);
            }
        }

        private void ChangeTagInTextBox(object sender, EventArgs e)
        {
            String t = ((MenuItem)sender).Text;
            textBoxName.Text = t;
        }

        /// <summary>
        /// Close map display
        /// </summary>
        public void CloseTabMap()
        {
            // Minimize now
            tabPage15_cachesPreviewMap.Tag = false;
            tabPage15_cachesPreviewMap.ImageKey = "max";

            // Set cachedetail not topmost
            _cacheDetail.TopMost = false;
        }

        private void tabControl1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                // did we click on a page button ?
                Point p = e.Location;
                for (int i = 0; i < tabControl1.TabCount; i++)
                {
                    Rectangle r = tabControl1.GetTabRect(i);
                    r.Offset(2, 2);
                    r.Width = 16;
                    r.Height = 16;
                    if (r.Contains(p))
                    {
                        TabPage page = tabControl1.TabPages[i];
                        if (page == tabPage15_cachesPreviewMap) // tabmap
                        {
                            if ((tabPage15_cachesPreviewMap.Tag == null) || (((bool)(tabPage15_cachesPreviewMap.Tag)) == false))
                            {
                                // Maximize now !
                                ShowCacheMapInCacheDetail();
                            }
                            else
                            {
                                _cacheDetail.CloseCacheMap();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Show map display, centered on coordinates
        /// </summary>
        /// <param name="origin">center coordinates</param>
        /// <returns>Tabpage of maps</returns>
        public TabPage ShowCacheMapInCacheDetailImpl(PointLatLng origin)
        {
            tabPage15_cachesPreviewMap.Tag = true;
            tabPage15_cachesPreviewMap.ImageKey = "min";
            TabPage map = null;
            // Display either homelocation or a cache
            if ((origin.Lat == Double.MaxValue) || (origin.Lng == Double.MaxValue))
            {
                if (lvGeocaches.SelectedItems.Count == 0)
                {
                    if (lvGeocaches.Items.Count != 0)
                    {
                        EXListViewItem lstvItem = lvGeocaches.Items[0] as EXListViewItem;
                        String code = lstvItem.Text;
                        Geocache geo = _caches[code];
                        map = _cacheDetail.DisplayCacheMap(geo._dLatitude, geo._dLongitude);
                    }
                    else
                        map = _cacheDetail.DisplayCacheMap(_dHomeLat, _dHomeLon);
                }
                else
                {
                    EXListViewItem lstvItem = lvGeocaches.SelectedItems[0] as EXListViewItem;
                    String code = lstvItem.Text;
                    Geocache geo = _caches[code];
                    map = _cacheDetail.DisplayCacheMap(geo._dLatitude, geo._dLongitude);
                }
            }
            else
            {
                // On a un point pour le centrage
                map = _cacheDetail.DisplayCacheMap(origin.Lat, origin.Lng);
            }

            // Set cachedetail topmost NON !!!!
            _cacheDetail.TopMost = false;
            return map;
        }

        /// <summary>
        /// Show map display
        /// </summary>
        /// <returns>tabppage of maps</returns>
        public TabPage ShowCacheMapInCacheDetail()
        {
            PointLatLng pt = new PointLatLng(Double.MaxValue, Double.MaxValue);
            return ShowCacheMapInCacheDetailImpl(pt);
        }

        private void configureProxyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String proxypassword = ConfigurationManager.AppSettings["proxypassword"];
            //StringCipher.CustomDecrypt(ConfigurationManager.AppSettings["proxypassword"], ref proxypassword);
            List<ParameterObject> lst = new List<ParameterObject>();
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, (ConfigurationManager.AppSettings["proxyused"] == "True"), "proxyused", GetTranslator().GetString("LblProxyEnable")));
            lst.Add(new ParameterObject(ParameterObject.ParameterType.String, ConfigurationManager.AppSettings["proxydomain"], "proxydomain", GetTranslator().GetString("LblProxyDomain")));
            lst.Add(new ParameterObject(ParameterObject.ParameterType.String, ConfigurationManager.AppSettings["proxylogin"], "proxylogin", GetTranslator().GetString("LblProxyLogin")));
            lst.Add(new ParameterObject(ParameterObject.ParameterType.String, proxypassword, "proxypassword", GetTranslator().GetString("LblProxyPassword")));

            ParametersChanger changer = new ParametersChanger();
            changer.Title = GetTranslator().GetString("FMenuConfigProxy");
            changer.BtnCancel = GetTranslator().GetString("BtnCancel");
            changer.BtnOK = GetTranslator().GetString("BtnOk");
            changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
            changer.ErrorTitle = GetTranslator().GetString("Error");
            changer.Parameters = lst;
            changer.Font = this.Font;
            changer.Icon = this.Icon;

            if (changer.ShowDialog() == DialogResult.OK)
            {
                UpdateConfFile("proxyused", lst[0].Value);
                UpdateConfFile("proxydomain", lst[1].Value);
                UpdateConfFile("proxylogin", lst[2].Value);
                UpdateConfFile("proxypassword", /*StringCipher.CustomEncrypt*/(lst[3].Value));

                UpdateHttpDefaultWebProxy();
                GMap.NET.MapProviders.GMapProvider.WebProxy = GetProxy();
                
                CheckInternetAccess();
                UpdateInternetAccessStatus();
                TranslateForm();
                InitContextMenu();
            }            
        }

        private void displayCachesWithoutADescriptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CustomFilterNoDescription fltr = new CustomFilterNoDescription();
            ExecuteCustomFilter(fltr);
        }

        /// <summary>
        /// Execute a custom filter
        /// </summary>
        /// <param name="fltr">filter to execute</param>
        public void ExecuteCustomFilter(CacheFilter fltr)
        {
            bool old_bUseFilter = _bUseFilter;
            CacheFilter oldFilter = Filter; // Attention à ne pas considérer l'éventuel filtre combiné

            DialogResult dialogResult = MessageBox.Show(GetTranslator().GetString("MsgExecuteFilterFirst"),
                        GetTranslator().GetString("WarTitle"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                List<EXListViewItem> forcedList = new List<EXListViewItem>();
                List<CacheFilter> lst = new List<CacheFilter>();
                UpdateFilter();
                lst.Add(Filter);
                lst.Add(fltr);
                ChainedFiltersAND chnf = new ChainedFiltersAND(lst);
                // Build list of caches
                foreach (EXListViewItem item in _listViewCaches)
                {
                    Geocache cache = _caches[item.Text];
                    if (chnf.ToBeDisplayed(cache))
                    {
                        forcedList.Add(item);
                    }
                }
                PopulateListViewCache(forcedList);
            }
            else
            {
                Filter = fltr;
                _bUseFilter = true;
                PopulateListViewCache(null);
            }

            _bUseFilter = old_bUseFilter;
            Filter = oldFilter;
        }

        /// <summary>
        /// Execute a custom filter (silent)
        /// </summary>
        /// <param name="fltr">filter to execute</param>
        /// <param name="executefirst">if true, existing filter is executed first</param>
        public void ExecuteCustomFilterSilent(CacheFilter fltr, bool executefirst)
        {
            bool old_bUseFilter = _bUseFilter;
            CacheFilter oldFilter = Filter; // Attention à ne pas considérer l'éventuel filtre combiné

            if (executefirst)
            {
                List<EXListViewItem> forcedList = new List<EXListViewItem>();
                List<CacheFilter> lst = new List<CacheFilter>();
                UpdateFilter();
                lst.Add(Filter);
                lst.Add(fltr);
                ChainedFiltersAND chnf = new ChainedFiltersAND(lst);
                // Build list of caches
                foreach (EXListViewItem item in _listViewCaches)
                {
                    Geocache cache = _caches[item.Text];
                    if (chnf.ToBeDisplayed(cache))
                    {
                        forcedList.Add(item);
                    }
                }
                PopulateListViewCache(forcedList);
            }
            else
            {
                Filter = fltr;
                _bUseFilter = true;
                PopulateListViewCache(null);
            }

            _bUseFilter = old_bUseFilter;
            Filter = oldFilter;
        }

        private void displayCachesWithModificatiosnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CustomFilterModified fltr = new CustomFilterModified();
            ExecuteCustomFilter(fltr);
        }

        private void fillAccountKeyTranslationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            List<ParameterObject> lst = new List<ParameterObject>();
            lst.Add(new ParameterObject(ParameterObject.ParameterType.String, ConfigurationManager.AppSettings["yandextrnsl"], "apikey", GetTranslator().GetString("LblTranslationAccountKey")));

            ParametersChanger changer = new ParametersChanger();
            changer.Title = GetTranslator().GetString("FMenuConfigurationTranslate");
            changer.BtnCancel = GetTranslator().GetString("BtnCancel");
            changer.BtnOK = GetTranslator().GetString("BtnOk");
            changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
            changer.ErrorTitle = GetTranslator().GetString("Error");
            changer.Parameters = lst;
            changer.Font = this.Font;
            changer.Icon = this.Icon;

            if (changer.ShowDialog() == DialogResult.OK)
            {
                UpdateConfFile("yandextrnsl", lst[0].Value);
                if (lst[0].Value == "")
                    MsgActionError(this, GetTranslator().GetStringM("LblMissingTranslateAccountKey"));
            }
        }

        private void translateSelectedCachesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TranslateCachesDescription();
        }

       
        private void usersInformationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateHttpDefaultWebProxy();
                // On checke que les L/MDP soient corrects
                // Et on récupère les cookies au passage
                CookieContainer cookieJar = CheckGCAccount(true, false);
                if (cookieJar == null)
                    return;

                String url = "http://www.geocaching.com/my/default.aspx";
                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
                objRequest.Proxy = GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
                objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
                HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
                String response;
                using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
                {
                    response = responseStream.ReadToEnd();
                    responseStream.Close();
                }
                
                // On parse la page
                // ****************
                String user = MyTools.GetSnippetFromText("<span class=\"ProfileUsername\" title=\"", "\"", response);
                Log(user);
                String avatarurl = MyTools.DoRegex(response, "<img id=\"ctl00_uxLoginStatus_hlHeaderAvatar\" src=\"(.*?)\" style=\"border-width:0px;\" />"); ;
                Log(avatarurl);
                String membership = MyTools.CleanString(MyTools.GetSnippetFromText("<p class=\"NoBottomSpacing\" id=\"memberStatus\">", "<br />", response));
				Log(membership);
                String tmp = MyTools.GetSnippetFromText("<p class=\"NoBottomSpacing\" id=\"memberStatus\">", "</div>", response);
                String membersince = MyTools.GetSnippetFromText("</strong>", "<br />", tmp);
                Log(membersince);
                tmp = MyTools.GetSnippetFromText("<div id=\"uxCacheFind\" class=\"statbox\">", "</div>", response);
                String find = MyTools.CleanString(MyTools.GetSnippetFromText("<span class=\"statcount\">", "</span>", tmp));
                Log(find);
                tmp = MyTools.GetSnippetFromText("<div id=\"uxCacheHide\" class=\"statbox\">", "</div>", response);
                String hide = MyTools.CleanString(MyTools.GetSnippetFromText("<span class=\"statcount\">", "</span>", tmp));
                Log(hide);
                String fav = MyTools.CleanString(MyTools.GetSnippetFromText("<span class=\"favorite-rank\">", "</span>", response));
                Log(fav);
                String tb = "-";
                
                UserInfo ui = new UserInfo(user, avatarurl, membership, membersince, find, hide, tb, fav);
                ui.ShowDialog();
            }
            catch (Exception ex)
            {
                ShowException("", "Retrieving user information", ex);
            }
        }

        private void exportSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportCaches(true);
        }

        private void exportImagesFromSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportImagesFromCaches(true);
        }

        private void createpublishnotifications_Click(object sender, EventArgs e)
        {
            try
            {
            	CloseCacheDetail();
            	NotificationsManager.CreateNotifications(this);
            }
            catch (Exception ex)
            {
                ShowException("", "Create notifications", ex);
            }
        }
        
        /// <summary>
        /// Display coordinates on carto
        /// </summary>
        /// <param name="lat">latitude</param>
        /// <param name="lon">longitude</param>
        public void HandlerToDisplayCoordinates(double lat, double lon)
        {
        	CenterOnCarto(lat.ToString(), lon.ToString());
        }
        
        /// <summary>
        /// 
        /// </summary>
        public void CloseCacheDetail()
        {
        	if ((_cacheDetail != null) && (_cacheDetail.Visible))
        	{
        		_cacheDetail.Close();
        	}
        }
        
        private void createpublishnotificationsext_Click(object sender, EventArgs e)
        {
            try
            {
            	CloseCacheDetail();
            	NotificationCreation nc = new NotificationCreation(this);
            	nc.ShowDialog();
            }
            catch (Exception ex)
            {
                ShowException("", "Create notifications (full)", ex);
            }
        }
        
        private void managepublishnotifications_Click(object sender, EventArgs e)
        {
            try
            {
            	CloseCacheDetail();
                NotificationsManager.ListNotifsGroup(this);
            }
            catch (Exception ex)
            {
                ShowException("", "Delete notifications", ex);
            }
        }
        
        private void LaunchPQDownloadHMI(bool bAutoDwnloadUpdatedPQsOnStart = false)
        {	
        	UpdateHttpDefaultWebProxy();
            // On checke que les L/MDP soient corrects
            // Et on récupère les cookies au passage
            CookieContainer cookieJar = CheckGCAccount(true, false);
            if (cookieJar == null)
                return;

            String pqdatapath = GetUserDataPath() + Path.DirectorySeparatorChar + "GPX" + Path.DirectorySeparatorChar + "PQ";


            String response = GetPQDownloadHTML(cookieJar);

            PQDownloadHMI hmi = new PQDownloadHMI(this);
            hmi.Icon = this.Icon;
            hmi.Font = this.Font;
            hmi.Populate(response, false);
            hmi._bAutoDwnloadUpdatedPQsOnStart = bAutoDwnloadUpdatedPQsOnStart;
            hmi.ShowDialog();
        }
        
		private void autodownloadPQToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
            	
            	LaunchPQDownloadHMI(true);
            }
            catch (Exception ex)
            {
                ShowException("", "Autodownloading updated pocket queries", ex);
            }
        }
        
        private void downloadPQToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
            	LaunchPQDownloadHMI(false);
            }
            catch (Exception ex)
            {
                ShowException("", "Downloading a pocket query", ex);
            }
        }

        private void createPQToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
            	CloseCacheDetail();
            	PQDownloadHMI.CreatePQ(this);
            }
            catch (Exception ex)
            {
            	ShowException("", GetTranslator().GetString("createPQToolStripMenuItem"), ex);
            }
        }
        
        private void createPQDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
            	PQDownloadHMI.CreatePQDate(this);
            }
            catch (Exception ex)
            {
            	ShowException("", GetTranslator().GetString("createPQDateToolStripMenuItem"), ex);
            }
        }
        
        private void checkPQToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
            	PQCheckHMI hmi = new PQCheckHMI(this);
            	hmi.ShowDialog();
            }
            catch (Exception ex)
            {
            	ShowException("", GetTranslator().GetString("checkPQToolStripMenuItem"), ex);
            }
        }
        
        /// <summary>
        /// Retrieve HTML source code of PQ download page on GC.com
        /// </summary>
        /// <param name="cookieJar">Authentication cookie</param>
        /// <returns>HTML of PQ download page</returns>
        public String GetPQDownloadHTML(CookieContainer cookieJar)
        {
            String url = "http://www.geocaching.com/pocket/default.aspx";
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
            objRequest.Proxy = GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
            objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
            String response;
            using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
            {
                response = responseStream.ReadToEnd();
                responseStream.Close();
            }
            return response;
        }

        /// <summary>
        /// Update default proxy used for HttpWebRequest
        /// </summary>
        public void UpdateHttpDefaultWebProxy()
        {
            if (GetProxy() != null)
                HttpWebRequest.DefaultWebProxy.Credentials = GetProxy().Credentials; 
        }       

        private void DownloadNotesFromGCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Geocache> caches = null;
            try
            {
                caches = GetSelectedCaches();
                if (caches.Count != 0)
                {
                    UpdateHttpDefaultWebProxy();
                    // On checke que les L/MDP soient corrects
                    // Et on récupère les cookies au passage
                    CookieContainer cookieJar = CheckGCAccount(true, false);
                    if (cookieJar == null)
                        return;

                    _ThreadProgressBarTitle = GetTranslator().GetString("LblOperationInProgress");
                    CreateThreadProgressBarEnh();

                    // Wait for the creation of the bar
                    while (_ThreadProgressBar == null)
                    {
                        Thread.Sleep(10);
                        Application.DoEvents();
                    }
                    _ThreadProgressBar.progressBar1.Maximum = caches.Count();
                    _ThreadProgressBar.lblWait.Text = "";
                    _ThreadProgressBar.StartEstimate();

                    foreach (Geocache geo in caches)
                    {
                        _ThreadProgressBar.lblWait.Text = "";
                        
                        // Recupère les infos de la cache
                        // Récupération d'une page de geocache
                        // ***********************************
                        String url = geo._Url;
                        HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
                        objRequest.Proxy = GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
                        objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
                        HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
                        String response;
                        using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
                        {
                            response = responseStream.ReadToEnd();
                            responseStream.Close();
                        }

                        //Si on est tombé sur une cache premium et que nous ne sommes pas premium
			            // Dans ce cas là on sort direct
			           	if (IsPremiumCacheAndWeAreBasic(response))
			            {
			            	Log("This cache is premium and we are not... " + geo._Code);
			            }
			            else
			            {
	                        // On va récupérer les notes
	                        // *************************
	                        String tmp = MyTools.GetSnippetFromText("<span id=\"cache_note\">", "</span>", response);
			            	if (tmp != "")
			            	{
			            		tmp = tmp.Replace("\n","\r\n");
			            		tmp = tmp.Replace("\r\r\n","\r\n");
			            		// On a récupéré une note.
			            		// Do we need to create a new OCD object ?
					            if (geo._Ocd == null)
					            {
					                // yes we create it
					                OfflineCacheData ocd1 = new OfflineCacheData();
					                ocd1._Code = geo._Code;
					                AssociateOcdCache(geo._Code, ocd1, geo);
					            }
					            
					            // Maintenant on a un OCD, on regarde si le texte était déjà présent
					            if (String.IsNullOrEmpty(geo._Ocd._Comment))
					            {
					            	geo._Ocd._Comment = tmp;
					            	_od.Serialize(_odfile);
					            	UpdateListViewOfflineIcons(geo._Ocd);
					            }
					            else
					            {
					            	if (geo._Ocd._Comment.Contains(tmp) == false)
					            	{
					            		geo._Ocd._Comment += "\r\n" + tmp;
					            		_od.Serialize(_odfile);
					            	}
					            	// Sinon on ne fait rien
					            }
			            	}
			            }
			            
                        Application.DoEvents();
                        
                        // step
                        _ThreadProgressBar.Step();

                        
                        if (_ThreadProgressBar._bAbort)
                            break;
                    }

                    // Final wrapup
                    _od.Serialize(_odfile);


                    // No need to do that I think
                    //PostTreatmentLoadCache();
                    // Better way to do that : only recreate for modified caches
                    _cacheDetail.EmptyStatsMarkers();
                    RecreateVisualElements(caches);

                    KillThreadProgressBarEnh();
                    
                    MsgActionDone(this);
                }
                
            }
            catch (Exception ex)
            {
                // Final wrapup
                _od.Serialize(_odfile);

                // No need to do that I think
                //PostTreatmentLoadCache();
                // Better way to do that : only recreate for modified caches
                RecreateVisualElements(caches);

                KillThreadProgressBarEnh();
                ShowException("", "Download personal cache notes", ex);
            }
        }
       
        private void UploadNotesToGCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Geocache> caches = null;
            try
            {
                caches = GetSelectedCaches();
                if (caches.Count != 0)
                {
                    UpdateHttpDefaultWebProxy();
                    
                    // On checke que les L/MDP soient corrects
                    // Et on récupère les cookies au passage
                    CookieContainer cookieJar = CheckGCAccount(true, false);
                    if (cookieJar == null)
                        return;

                    _ThreadProgressBarTitle = GetTranslator().GetString("LblOperationInProgress");
                    CreateThreadProgressBarEnh();

                    // Wait for the creation of the bar
                    while (_ThreadProgressBar == null)
                    {
                        Thread.Sleep(10);
                        Application.DoEvents();
                    }
                    _ThreadProgressBar.progressBar1.Maximum = caches.Count();
                    _ThreadProgressBar.lblWait.Text = "";
                    _ThreadProgressBar.StartEstimate();

                    foreach (Geocache geo in caches)
                    {
                        _ThreadProgressBar.lblWait.Text = "";
                        
                        if ((geo._Ocd != null) && (!String.IsNullOrEmpty(geo._Ocd._Comment)))
                        {
	                        // Recupère les infos de la cache
	                        // Récupération d'une page de geocache
	                        // ***********************************
	                        String url = geo._Url;
	                        HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
	                        objRequest.Proxy = GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
	                        objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
	                        HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
	                        String response;
	                        using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
	                        {
	                            response = responseStream.ReadToEnd();
	                            responseStream.Close();
	                        }
													
	                        //Si on est tombé sur une cache premium et que nous ne sommes pas premium
				            // Dans ce cas là on sort direct
				           	if (IsPremiumCacheAndWeAreBasic(response))
				            {
				            	Log("This cache is premium and we are not... " + geo._Code);
				            }
				            else
				            {
		                        // On va uploader les notes
		                        // *************************
		                        String usertoken = MyTools.DoRegex(response, "userToken\\s*=\\s*'([^']+)'");
		                        // { dto: { et: et, ut: userToken} }
		                        String jsonText = "{dto:{\"et\":\"" + geo._Ocd._Comment + "\",\"ut\":\"" + usertoken + "\"}}";
		                       
		                        objRequest = (HttpWebRequest)WebRequest.Create("https://www.geocaching.com/seek/cache_details.aspx/SetUserCacheNote");
							    objRequest.ContentType = @"application/json";
								objRequest.Method = "POST";
							    objRequest.Proxy = GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
					            objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
							    System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
							    Byte[] byteArray = encoding.GetBytes(jsonText);
							    objRequest.ContentLength = byteArray.Length;
							    
							    using (Stream dataStream = objRequest.GetRequestStream())
							    {
							        dataStream.Write(byteArray, 0, byteArray.Length);
							        dataStream.Flush();
								    dataStream.Close();
							    }
							    
							    objResponse = (HttpWebResponse)objRequest.GetResponse();
							    using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
					            {
					                response = responseStream.ReadToEnd();
					                responseStream.Close();
					            }
							    //_cacheDetail.LoadPageText("dbg1", response, true);
				            }
                        }
                        // else nothing to do
                        Application.DoEvents();
                        
                        // step
                        _ThreadProgressBar.Step();
                        
                        if (_ThreadProgressBar._bAbort)
                            break;
                    }

                    // Final wrapup
                    _od.Serialize(_odfile);


                    // No need to do that I think
                    //PostTreatmentLoadCache();
                    // Better way to do that : only recreate for modified caches
                    _cacheDetail.EmptyStatsMarkers();
                    RecreateVisualElements(caches);

                    KillThreadProgressBarEnh();
                    
                    MsgActionDone(this);
                    
                }
                
            }
            catch (Exception ex)
            {
                // Final wrapup
                _od.Serialize(_odfile);

                // No need to do that I think
                //PostTreatmentLoadCache();
                // Better way to do that : only recreate for modified caches
                RecreateVisualElements(caches);

                KillThreadProgressBarEnh();
                ShowException("", "Upload personal cache notes", ex);
            }
        }
        
        private void updateStatsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Geocache> caches = null;
            try
            {
                caches = GetSelectedCaches();
                int inbmissed = 0;
                if (caches.Count != 0)
                {
                    UpdateHttpDefaultWebProxy();
                    // On checke que les L/MDP soient corrects
                    // Et on récupère les cookies au passage
                    CookieContainer cookieJar = CheckGCAccount(true, false);
                    if (cookieJar == null)
                        return;

                    _ThreadProgressBarTitle = GetTranslator().GetString("LblOperationInProgress");
                    CreateThreadProgressBarEnh();

                    // Wait for the creation of the bar
                    while (_ThreadProgressBar == null)
                    {
                        Thread.Sleep(10);
                        Application.DoEvents();
                    }
                    _ThreadProgressBar.progressBar1.Maximum = caches.Count();
                    _ThreadProgressBar.lblWait.Text = "";
                    _ThreadProgressBar.StartEstimate();

                    bool stopScoreRetrieval = false;
                    bool firstQuestion = true;
                    foreach (Geocache geo in caches)
                    {
                        _ThreadProgressBar.lblWait.Text = "";
                        
                        // Recupère les infos de la cache
                        // Récupération d'une page de geocache
                        // ***********************************
                        String url = geo._Url;
                        HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
                        objRequest.Proxy = GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
                        objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
                        HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
                        String response;
                        using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
                        {
                            response = responseStream.ReadToEnd();
                            responseStream.Close();
                        }

                        //Si on est tombé sur une cache premium et que nous ne sommes pas premium
			            // Dans ce cas là on sort direct
			           	if (IsPremiumCacheAndWeAreBasic(response))
			            {
			            	Log("This cache is premium and we are not... " + geo._Code);
			            }
			            else
			            {
	                        // On va récupérer les stats
	                        // *************************
	                        // Retrieve all logs, only if favorites succeeded, otherwise it's pointless
	                        int ifav = -1;
	                        int ifound = -1;
	                        int inotfound = -1;
	                        int ifoundpremium = -1;
	                        int inotfoundpremium = -1;
	                        String usertoken = GetBasicStatsFromHTML(response, ref ifav, ref ifound, ref inotfound);
	
	                        // On demande les stats (score de favoris)
	                        // ***************************************
	                        String sScoreFavori = "";
	                        if (_bPopulariteBasique)
	                        {
	                            Log("ifoundpremium " + ifound.ToString());
	                            Log("ifav " + ifav.ToString());
	                            if ((ifound > 0) && (ifav > 0))
	                            {
	                                sScoreFavori = ((int)((double)ifav / (double)ifound * 100.0)).ToString();
	                                Log("sScoreFavori " + sScoreFavori);
	                            }
	                            else
	                            {
	                                sScoreFavori = "0";
	                            }
	                        }
	                        else
	                            GetAdvancedStatsFromHTML(response, ref inbmissed, cookieJar, ref stopScoreRetrieval, ref firstQuestion, ifav, usertoken, ref sScoreFavori);
	
	                        // On renseigne les stats avancées
	                        // ###############################
	                        ComputeAdvancedStats(ifav, ref ifoundpremium, ref inotfoundpremium, sScoreFavori);
	                        
	                        // on met à jour la cache avec ses stats
	                        UpdateCacheWithStats(geo, ifav, ifound, inotfound, ifoundpremium, inotfoundpremium, false);
			            }
			            
                        Application.DoEvents();
                        
                        // step
                        _ThreadProgressBar.Step();

                        
                        if (_ThreadProgressBar._bAbort)
                            break;
                    }

                    // Final wrapup
                    _od.Serialize(_odfile);


                    // No need to do that I think
                    //PostTreatmentLoadCache();
                    // Better way to do that : only recreate for modified caches
                    _cacheDetail.EmptyStatsMarkers();
                    RecreateVisualElements(caches);

                    KillThreadProgressBarEnh();
                    
                    if (inbmissed == 0)
                    	MsgActionDone(this);
                    else
                    {
                    	MsgActionError(this, String.Format(GetTranslator().GetString("LblErrorRetrieveStats"), inbmissed));
                    }
                }
                
            }
            catch (Exception ex)
            {
                // Final wrapup
                _od.Serialize(_odfile);

                // No need to do that I think
                //PostTreatmentLoadCache();
                // Better way to do that : only recreate for modified caches
                RecreateVisualElements(caches);

                KillThreadProgressBarEnh();
                ShowException("", "Updating statistics", ex);
            }
        }

        private void UpdateCacheWithStats(Geocache geo, int ifav, int ifound, int inotfound, int ifoundpremium, int inotfoundpremium, bool bDiagnostic)
        {
            // Do we need to create a new OCD object ?
            if (geo._Ocd == null)
            {
                // yes we create it
                OfflineCacheData ocd1 = new OfflineCacheData();
                //ocd1._dateExport = DateTime.Now;
                ocd1._Code = geo._Code;
                if (!bDiagnostic)
                    AssociateOcdCache(geo._Code, ocd1, geo);
                else
                {
                    geo._Ocd = ocd1;
                }
            }

            // Now update stats for favs
            geo._Ocd._iNbFavs = ifav;
            geo._Ocd._iNbFounds = ifound;
            geo._Ocd._iNbNotFounds = inotfound;
            geo._Ocd._iNbFoundsPremium = ifoundpremium;
            geo._Ocd._iNbNotFoundsPremium = inotfoundpremium;
            if (geo._Ocd._iNbFavs == 0)
            {
                // Le calcul est simple, la popularité est nulle
                geo._Ocd._dRating = 0; // ou alors le résultat est tellement petit (1e-17) qu'on considère qu'il est nul !
                geo._Ocd._dRatingSimple = 0;
            }
            else if (geo._Ocd._iNbFoundsPremium != -1)
            {
                // On a des logs premium et un nombre de favoris *non nuls*, on calcule la popularité
                geo._Ocd._dRating = MyTools.Rating(geo._Ocd._iNbFavs, geo._Ocd._iNbFoundsPremium - geo._Ocd._iNbFavs);
                geo._Ocd._dRatingSimple = MyTools.RatingSimple(geo._Ocd._iNbFavs, geo._Ocd._iNbFoundsPremium - geo._Ocd._iNbFavs);
            }
            else
            {
                // Sinon on ne sait rien calculer il nous faut le nombre de log premium... :-(
                geo._Ocd._dRating = -1;
                geo._Ocd._dRatingSimple = -1;
            }
            Log(geo._Ocd.ToString()); // DEBUG !!!!
        }

        private void ComputeAdvancedStats(int ifav, ref int ifoundpremium, ref int inotfoundpremium, String sScoreFavori)
        {
            try
            {
                if (sScoreFavori != "")
                {
                    int iScoreFavori = Int32.Parse(sScoreFavori);
                    if (iScoreFavori != 0)
                    	ifoundpremium = (int)Math.Round((double)ifav * 100.0 / (double)iScoreFavori);
                }
                else
                {
                    ifoundpremium = -1;
                }
            }
            catch (Exception ex2)
            {
            	Log(GetException("Computing advanced statistics", ex2));
                ifoundpremium = -1;
                inotfoundpremium = -1;
            }
        }

        /// <summary>
        /// Retrieve VIEWSTATE information from a Geocaching.com webpage
        /// </summary>
        /// <param name="response">webpage to parse</param>
        /// <param name="__VIEWSTATEFIELDCOUNT">retrieved __VIEWSTATEFIELDCOUNT value</param>
        /// <param name="__VIEWSTATE">retrieved __VIEWSTATE list</param>
        /// <param name="__VIEWSTATEGENERATOR">retrieved __VIEWSTATEGENERATOR value</param>
        public static void GetViewState(String response, ref String __VIEWSTATEFIELDCOUNT, ref String[] __VIEWSTATE, ref String __VIEWSTATEGENERATOR)
        {
            // __VIEWSTATEFIELDCOUNT" value="2" />
            // id="__VIEWSTATE" value="
            // id="__VIEWSTATE1" value="
            // <input type="hidden" name="__VIEWSTATEGENERATOR" id="__VIEWSTATEGENERATOR" value="25748CED" />
            
            __VIEWSTATEFIELDCOUNT = MyTools.GetSnippetFromText("__VIEWSTATEFIELDCOUNT\" value=\"", "\"", response);
            if (__VIEWSTATEFIELDCOUNT == "")
            {
            	__VIEWSTATEFIELDCOUNT = "1";
            }
            int ivscount = Int32.Parse(__VIEWSTATEFIELDCOUNT);
            __VIEWSTATE = new string[ivscount];

            for (int i = 0; i < ivscount; i++)
            {
                if (i == 0)
                {
                    __VIEWSTATE[i] = MyTools.GetSnippetFromText("id=\"__VIEWSTATE\" value=\"", "\"", response);
                }
                else
                {
                    __VIEWSTATE[i] = MyTools.GetSnippetFromText("id=\"__VIEWSTATE" + i.ToString() + "\" value=\"", "\"", response);
                }
            }
            
            __VIEWSTATEGENERATOR = MyTools.GetSnippetFromText("__VIEWSTATEGENERATOR\" value=\"", "\"", response);
        }

        private void GetAdvancedStatsFromHTML(string result, ref int inbmissed, CookieContainer cookieJar, ref bool stopScoreRetrieval, ref bool firstQuestion, int ifav, String usertoken, ref String sScoreFavori)
        {
            try
            {
                if (ifav != 0) // sinon pas besoin de calculer le score de favoris
                {
                    if (!stopScoreRetrieval)
                    {
                        // On récupère les viewstates
                        String __VIEWSTATEFIELDCOUNT = "";
                        String[] __VIEWSTATE = null;
                        String __VIEWSTATEGENERATOR = "";
                        GetViewState(result, ref __VIEWSTATEFIELDCOUNT, ref __VIEWSTATE, ref __VIEWSTATEGENERATOR);
                        
                        // On va faire un post data tout simple
                        // Préparation des données du POST
                        Dictionary<String, String> post_values = new Dictionary<String, String>();
                        post_values.Add("__EVENTTARGET", "");
                        post_values.Add("__EVENTARGUMENT", "");
                        post_values.Add("__LASTFOCUS", "");
                        post_values.Add("__VIEWSTATE", __VIEWSTATE[0]);
                        if (__VIEWSTATE.Length > 1)
                        {
                            for (int i = 1; i < __VIEWSTATE.Length; i++)
                            {
                                post_values.Add("__VIEWSTATE" + i.ToString(), __VIEWSTATE[i]);
                            }
                            post_values.Add("__VIEWSTATEFIELDCOUNT", __VIEWSTATE.Length.ToString());
                        }
						if (__VIEWSTATEGENERATOR != "")
                    		post_values.Add("__VIEWSTATEGENERATOR", __VIEWSTATEGENERATOR);
                        
                        // Encodage des données du POST
                        String post_string = "";
                        foreach (KeyValuePair<String, String> post_value in post_values)
                        {
                            post_string += post_value.Key + "=" + System.Web.HttpUtility.UrlEncode(post_value.Value) + "&";
                        }
                        post_string = post_string.TrimEnd('&');


                        String url2 = "https://www.geocaching.com/datastore/favorites.svc/score?u=" + usertoken;// +"&f=false";
                        HttpWebRequest objRequest2 = (HttpWebRequest)WebRequest.Create(url2);
                        objRequest2.Method = "POST";
                        objRequest2.ContentLength = post_string.Length; //0;
                        objRequest2.ContentType = "application/x-www-form-urlencoded";
                        objRequest2.Proxy = GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
                        objRequest2.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification

                        // on envoit les POST data dans un stream (écriture)
                        StreamWriter myWriter = new StreamWriter(objRequest2.GetRequestStream());
                        myWriter.Write(post_string); //myWriter.Write("");
                        myWriter.Close();

                        HttpWebResponse objResponse2 = (HttpWebResponse)objRequest2.GetResponse();
                        using (StreamReader responseStream = new StreamReader(objResponse2.GetResponseStream()))
                        {
                            sScoreFavori = responseStream.ReadToEnd();
                            responseStream.Close();
                            Log("sScoreFavori = " + sScoreFavori);
                        }
                    }
                    else
                    {
                        inbmissed++;
                    }
                }
            }
            catch (Exception ex)
            {
                inbmissed++;
                sScoreFavori = "";
                Log(GetException("GetAdvancedStatsFromHTML", ex));
                // On a vraisemblablement explosé les limites GC.com
                if (firstQuestion)
                {
                    DialogResult dialogResult = MessageBox.Show(GetTranslator().GetString("MsgFavScoreLimit"),
                        GetTranslator().GetString("WarTitle"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dialogResult == DialogResult.Yes)
                    {
                        stopScoreRetrieval = true;
                    }
                    firstQuestion = false;
                }
            }
        }

        private String GetBasicStatsFromHTML(String response, ref int ifav, ref int ifound, ref int inotfound)
        {
            // On parse la page
            // ****************
            String found_box = MyTools.DoRegex(response, "<span id=\"ctl00_ContentBody_lblFindCounts\"><p(.+?)</p></span>");
            String sNbFav = MyTools.DoRegex(response, "<span class=\"favorite-value\">\\D*([0-9]+?)\\D*</span>");
            String usertoken = MyTools.DoRegex(response, "userToken\\s*=\\s*'([^']+)'");
            //String sFoundIt = MyTools.DoRegex(found_box, "title=\"Found it\" />\\D*([0-9]+?)\\D*&nbsp");
            String sFoundIt = MyTools.GetSnippetFromText("title=\"Found it\" /> ", "&nbsp;", found_box);
            // Pour les milliers !!!
            sFoundIt = sFoundIt.Replace(" ", "");
            sFoundIt = sFoundIt.Replace(".", "");
            sFoundIt = sFoundIt.Replace(",", "");
            String sDidntFoundIt = MyTools.GetSnippetFromText("title=\"Didn't find it\" /> ", "&nbsp;", found_box);
            // Pour les milliers !!!
            sDidntFoundIt = sDidntFoundIt.Replace(" ", "");
            sDidntFoundIt = sDidntFoundIt.Replace(".", "");
            sDidntFoundIt = sDidntFoundIt.Replace(",", "");

            Log("sNbFav = " + sNbFav);
            Log("sFoundIt = " + sFoundIt);
            Log("sDidntFoundIt = " + sDidntFoundIt);

            // On renseigne les stats de base
            // ##############################
            try
            {
                if (sNbFav != "")
                    ifav = Int32.Parse(sNbFav);
                else
                    ifav = 0;

                if (sFoundIt != "")
                    ifound = Int32.Parse(sFoundIt);
                else
                    ifound = 0;

                if (sDidntFoundIt != "")
                    inotfound = Int32.Parse(sDidntFoundIt);
                else
                    inotfound = 0;
            }
            catch (Exception ex2)
            {
            	Log(GetException("Computing simple statistics", ex2));
                ifav = -1;
                ifound = -1;
                inotfound = -1;
            }
            return usertoken;
        }       

        private void displayCachesWithoutStatsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CustomFilterNoStats fltr = new CustomFilterNoStats();
            ExecuteCustomFilter(fltr);
        }

        private void completeCacheDescToolStripMenuItem_Click(object sender, EventArgs e)
        {
        	if (!SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled()) return;
            CompleteSelectedCaches(null);
        }

        private void displayNonTranslatedCachesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CustomFilterNonTranslated fltr = new CustomFilterNonTranslated();
            ExecuteCustomFilter(fltr);
        }

        private void convertCoordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CoordConvHMI converter = new CoordConvHMI();
            converter.Icon = this.Icon;
            converter.Text = GetTranslator().GetString("FMenuToolsConverter");
            converter.LoadCoordDDD(_dHomeLat, _dHomeLon);
            converter.TopMost = true;
            TranslateTooltips(converter, null);
            converter.Show();
        }

        private void displayCachesWithSpoilersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CustomFilterWithSpoiler fltr = new CustomFilterWithSpoiler();
            ExecuteCustomFilter(fltr);
        }
        
        private void displayCachesWithGPSSpoilersToolStripMenuItem_Click(object sender, EventArgs e)
        {
        	if (!SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled()) return;

        	List<Geocache> caches = GetSelectedCaches();
        	String offdatapath = GetUserDataPath() + Path.DirectorySeparatorChar + "Offline" + Path.DirectorySeparatorChar;
        	GMapOverlay overlay = _cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2];
                
        	String html = "<html><body>\r\n";
        	bool trouve = false;
        	String earthpath = "<img src=\"file:\\\\" + GetResourcesDataPath() + Path.DirectorySeparatorChar + "Img" + Path.DirectorySeparatorChar + "Earth.gif\">";
            foreach(Geocache geo in caches)
        	{
            	String cachehtml = "<H1><a href=MGMGEO:" + geo._Code + ">" + geo._Code + "&nbsp;" + geo._Name + "</a></H1><br>\r\n";
				cachehtml += "<a href=MGMGEOM:" + geo._Code + ">" + earthpath + "</a>&nbsp;" + CoordConvHMI.ConvertDegreesToDDMM(geo._dLatitude, true) + "&nbsp;" + CoordConvHMI.ConvertDegreesToDDMM(geo._dLongitude, false) + "<br>\r\n";
        		if (geo._Ocd != null)
        		{
        			String blocimages = "";
        			foreach (KeyValuePair<String, OfflineImageWeb> paire in geo._Ocd._ImageFilesSpoilers)
                	{
        				OfflineImageWeb oiw = paire.Value;
		                if (oiw != null)
		                {
		                	String img = offdatapath + oiw._localfile;
		                    
		                    double? lat = null;
                            double? lon = null;
                            MyTools.getExifCoords(img, ref lat, ref lon);
                            if ((lat != null) && (lon != null))
                            {
                            	trouve = true;
                            	blocimages += "<li><a href=\"MGMGEOMXY:" + lat.ToString() + "#" + lon.ToString() + "\">" + earthpath + "</a>&nbsp;" + HtmlAgilityPack.HtmlEntity.DeEntitize(oiw._name) + "&nbsp;:&nbsp;";
                            	blocimages += CoordConvHMI.ConvertDegreesToDDMM((double)lat, true) + "&nbsp;";
                            	blocimages += CoordConvHMI.ConvertDegreesToDDMM((double)lon, false) + "&nbsp;";
                            	double km = MyTools.DistanceBetweenPoints(geo._dLatitude, geo._dLongitude, (double)lat, (double)lon);
                            	km *= 1000.0;
                            	blocimages += "&nbsp;[" + ((int)km).ToString() + "&nbsp;m]</li><br>";
                            	blocimages += "<img src=\"file:\\\\" + img + "\"><br>\r\n";
                            		
                            	// We create the marker
                            	Image dti = _listImagesSized[getIndexImages("OpenChest")];
				                GMapWrapper.gmapMarkerWithImageAndImageTooltip(
			                        overlay,
			                        dti,
			                        (double)lat,
			                        (double)lon,
			                       	img,
			                        HtmlAgilityPack.HtmlEntity.DeEntitize(oiw._name));//geo._Code + " " + geo._Name + " [" + ((int)km).ToString() + " m]");
                            }
		                }
        			}
        			if (blocimages != "")
        			{
        				html += cachehtml + blocimages + "<br>\r\n";
        			}
        		}
        	}
        	html += "</body></html>";
        	
        	if (trouve)
        	{
        		_cacheDetail.LoadPageText(GetTranslator().GetString("LblPicsWithGPS"), html, true);
        		ShowCacheMapInCacheDetail();
        	}
        	else
        	{
        		MsgActionWarning(this, GetTranslator().GetString("LblWarningNoResult"));
        	}
        }
        
        private void setUpdatedCoordToolStripMenuItem_Click(object sender, EventArgs e)
        {
        	List<Geocache> caches = null;
            try
            {
                caches = GetSelectedCaches();
                if (caches.Count != 0)
                {
                	
                	DialogResult dialogResult = MessageBox.Show(GetTranslator().GetString("AskConfirm"), GetTranslator().GetString("setUpdatedCoordToolStripMenuItem"),
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
	                if (dialogResult != DialogResult.Yes)
	                {
	                	return;
	                }
	                
                    UpdateHttpDefaultWebProxy();
                    
                    // On checke que les L/MDP soient corrects
                    // Et on récupère les cookies au passage
                    CookieContainer cookieJar = CheckGCAccount(true, false);
                    if (cookieJar == null)
                        return;

                    _ThreadProgressBarTitle = GetTranslator().GetString("LblOperationInProgress");
                    CreateThreadProgressBarEnh();

                    // Wait for the creation of the bar
                    while (_ThreadProgressBar == null)
                    {
                        Thread.Sleep(10);
                        Application.DoEvents();
                    }
                    _ThreadProgressBar.progressBar1.Maximum = caches.Count();
                    _ThreadProgressBar.lblWait.Text = "";
                    _ThreadProgressBar.StartEstimate();

                    foreach (Geocache geo in caches)
                    {
                        _ThreadProgressBar.lblWait.Text = "";
                        
                        if (GeocachingConstants.CheckIfCoordinatesCanBemodifiedOnGC(geo._Type))
                        {
                        	double dLat = geo._dLatitude;
	                        double dLon = geo._dLongitude;
	                        String sLat2 = CoordConvHMI.ConvertDegreesToDDMM(dLat, true);
			                String sLon2 = CoordConvHMI.ConvertDegreesToDDMM(dLon, false);
			                // N 48° 48.091' E 001° 55.180'
			                String coords = /*"DD° MM.MMM: " + */sLat2 + "' " + sLon2 + "'";
			                
	                        // Recupère les infos de la cache
	                        // Récupération d'une page de geocache
	                        // ***********************************
	                        String url = geo._Url;
	                        HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
	                        objRequest.Proxy = GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
	                        objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
	                        HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
	                        String response;
	                        using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
	                        {
	                            response = responseStream.ReadToEnd();
	                            responseStream.Close();
	                        }
													
	                        //Si on est tombé sur une cache premium et que nous ne sommes pas premium
				            // Dans ce cas là on sort direct
				           	if (IsPremiumCacheAndWeAreBasic(response))
				            {
				            	Log("This cache is premium and we are not... " + geo._Code);
				            }
				            else
				            {
		                        // On va modifier les coordonnées
		                        // ******************************
		                        
		                        String usertoken = MyTools.DoRegex(response, "userToken\\s*=\\s*'([^']+)'");
		                        // { dto: { data: {lat: $this.data("lat"), lng: $this.data("lng") }, ut: userToken } }
		                        String jsonText = "{dto: { data: {lat:\"" + dLat.ToString().Replace(',','.') + "\",lng:\"" + dLon.ToString().Replace(',','.') + "\"}, ut:\"" + usertoken + "\"}}";
		                        
		                        objRequest = (HttpWebRequest)WebRequest.Create("https://www.geocaching.com/seek/cache_details.aspx/SetUserCoordinate");
							    objRequest.ContentType = @"application/json";
								objRequest.Method = "POST";
							    objRequest.Proxy = GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
					            objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
							    System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
							    Byte[] byteArray = encoding.GetBytes(jsonText);
							    objRequest.ContentLength = byteArray.Length;
							    
							    using (Stream dataStream = objRequest.GetRequestStream())
							    {
							        dataStream.Write(byteArray, 0, byteArray.Length);
							        dataStream.Flush();
								    dataStream.Close();
							    }
							    
							    objResponse = (HttpWebResponse)objRequest.GetResponse();
							    using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
					            {
					                response = responseStream.ReadToEnd();
					                responseStream.Close();
					            }
							    //_cacheDetail.LoadPageText("dbg1", response, true);
				            }
                        }
                        // else nothing to do
                        Application.DoEvents();
                        
                        // step
                        _ThreadProgressBar.Step();
                        
                        if (_ThreadProgressBar._bAbort)
                            break;
                    }

                    KillThreadProgressBarEnh();
                    
                    MsgActionDone(this);
                    
                }
                
            }
            catch (Exception ex)
            {

                KillThreadProgressBarEnh();
                ShowException("", "Modify coordinates on GC.com", ex);
            }
        
        }
        
        private void getUpdatedCoordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Geocache> caches = null;
            try
            {
                caches = GetSelectedCaches();
                int nbmodif = 0;
                if (caches.Count != 0)
                {
                    UpdateHttpDefaultWebProxy();

                    // On checke que les L/MDP soient corrects
                    // Et on récupère les cookies au passage
                    CookieContainer cookieJar = CheckGCAccount(true, false);
                    if (cookieJar == null)
                        return;

                    _ThreadProgressBarTitle = GetTranslator().GetString("LblOperationInProgress");
                    CreateThreadProgressBarEnh();

                    // Wait for the creation of the bar
                    while (_ThreadProgressBar == null)
                    {
                        Thread.Sleep(10);
                        Application.DoEvents();
                    }
                    _ThreadProgressBar.progressBar1.Maximum = caches.Count();
                    _ThreadProgressBar.lblWait.Text = "";

                    // Init this shit
                    _ThreadProgressBar.StartEstimate();
                    
                    foreach (Geocache geo in caches)
                    {

                        // Recupère les infos de la cache
                        // Récupération d'une page de geocache
                        // ***********************************
                        String url = geo._Url;
                        HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
                        objRequest.Proxy = GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
                        objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
                        HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
                        String response;
                        using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
                        {
                            response = responseStream.ReadToEnd();
                            responseStream.Close();
                        }

                        // On parse la page
                        // ****************
                        //Si on est tombé sur une cache premium et que nous ne sommes pas premium
			            // Dans ce cas là on sort direct
			            if (IsPremiumCacheAndWeAreBasic(response))
			            {
			            	Log("This cache is premium and we are not... " + geo._Code);
			            }
			            else
			            {
	                        String latlon = MyTools.DoRegex(response, "<span id=\"uxLatLon\"[^>]*>(.*?)</span>");
	                        String sLat = "";
	                        String sLon = "";
	                        if (CoordConvHMI.DDMMMtoDDD(latlon, ref sLat, ref sLon))
	                        {
	                            Log(geo._Latitude + " " + geo._Longitude + " || " + sLat + " " + sLon);
	                            if ((geo._Latitude != sLat) || (geo._Longitude != sLon))
	                            {
	                                Log(" ==> modification effectue !");
	                                geo._Latitude = sLat;
	                                geo._Longitude = sLon;
	                                geo._dLatitude = MyTools.ConvertToDouble(geo._Latitude);
	                                geo._dLongitude = MyTools.ConvertToDouble(geo._Longitude);
	                                geo.UpdateDistanceToHome(_dHomeLat, _dHomeLon);
	                                _iNbModifiedCaches += geo.InsertModification("COORD");
	                                nbmodif++;
	                            }
	                            else
	                            {
	                                // pas de modification
	                                Log(" ==> Pas de modification");
	                            }
	                        }
	                        else
	                        {
	                            Log("Error retrieving latlon " + latlon + " for " + geo._Code);
	                        }
			            }
                        _ThreadProgressBar.Step();
                        if (_ThreadProgressBar._bAbort)
                            break;
                    }

                    // Better way to do that : only recreate for modified caches
                    RecreateVisualElements(caches);

                    // On redessine les caches sur la carte !
                    if (nbmodif != 0)
                    {
	                	// On redessine la carte
                		BuildCacheMapNew(GetDisplayedCaches());
                    }
                    
                    KillThreadProgressBarEnh();
                    MsgActionDone(this, "\r\n" + nbmodif.ToString() + GetTranslator().GetString("LblModifiedCaches"));
                }

            }
            catch (Exception ex)
            {
                // Better way to do that : only recreate for modified caches
                RecreateVisualElements(caches);

                KillThreadProgressBarEnh();
                ShowException("", "Updating cache coordinates", ex);
            }
        }

        private void forceUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckUpdate(false, true);
        }

        private void exportCacheStatsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.IO.StreamWriter file = null;
            try
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "OCDS (*.ocds)|*.ocds";
                saveFileDialog1.RestoreDirectory = true;
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    file = new System.IO.StreamWriter(saveFileDialog1.FileName, false);
                    OfflineCacheData.WriteStatsHeader(file);
                    foreach (KeyValuePair<String, OfflineCacheData> paire in _od._OfflineData)
                    {
                        OfflineCacheData ocd = paire.Value;
                        ocd.WriteStats(file);
                    }
                    file.Close();

                    MsgActionDone(this);
                }
            }
            catch (Exception ex)
            {
                if (file != null)
                    file.Close();
                ShowException("", "Exporting cache statistics", ex);
            }            
        }

        private void importCacheStatsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.IO.StreamReader file = null;
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "OCDS (*.ocds)|*.ocds";
                openFileDialog1.Multiselect = false;
                openFileDialog1.RestoreDirectory = true;
                DialogResult dr = openFileDialog1.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    string[] filenames = openFileDialog1.FileNames;
                    file = new System.IO.StreamReader(filenames[0]);
                    String header = file.ReadLine(); // ignored
                    String cols = file.ReadLine(); // ignored
                    String line;
                    String res = "";
                    int iMerged = 0;
                    int iIgnored = 0;
                    int iAdded = 0;
                    List<Geocache> caches = new List<Geocache>();
                    while ((line = file.ReadLine()) != null)
                    {
                        OfflineCacheData ocd = OfflineCacheData.ReadStats(line);
                        if (ocd != null)
                        {
                            Geocache cache = null;
                            if (_caches.ContainsKey(ocd._Code))
                                cache = _caches[ocd._Code];

                            // If already exists in _od, try to merge it
                            if (_od._OfflineData.ContainsKey(ocd._Code))
                            {
                                // It exists, we merge it if needed
                                if (_od._OfflineData[ocd._Code].MergeStats(ocd))
                                {
                                    // for further refresh
                                    if (cache != null)
                                        caches.Add(cache);
                                    iMerged++;
                                }
                                else
                                    iIgnored++;
                            }
                            else
                            {
                                // for further refresh
                                if (cache != null)
                                    caches.Add(cache);

                                // We had these stats to _od
                                AssociateOcdCache(ocd._Code, ocd, cache);
                                iAdded++;
                            }
                        }
                    }
                    res = String.Format(GetTranslator().GetString("MsgICacheStatsResult"), iMerged, iIgnored, iAdded);
                    file.Close();

                    // save _od file
                    _od.Serialize(_odfile);

                    // Better way to do that : only recreate for modified caches
                    RecreateVisualElements(caches);
                    //BuildListViewCache();
                    //PopulateListViewCache(null);

                    MsgActionDone(this, "\r\n" + res);
                }
            }
            catch (Exception ex)
            {
                if (file != null)
                    file.Close();
                ShowException("", "Importing cache statistics", ex);
            } 
        }

        private void UpdateListViewPopularity()
        {
            lvGeocaches.BeginUpdate();
            foreach (EXListViewItem item in _listViewCaches)
            {
                Geocache cache = _caches[item.Text];
                OfflineCacheData ocd = null;
                if (_od._OfflineData.ContainsKey(cache._Code))
                {
                    ocd = _od._OfflineData[cache._Code];
                }
                double rating = -1.0;
                if (ocd != null)
                {
                    if (_bUseGCPopularity)
                        rating = ocd._dRatingSimple;
                    else
                        rating = ocd._dRating;
                }

                EXMultipleImagesListViewSubItem subItem = null;
                if (rating != -1)
                {
                    //String iRating = RatingToImageStar(rating);
                    String iRating = "ratio_" + ((int)(rating * 100.0)).ToString();
                    int index = getIndexImages(iRating);
                    String srating = rating.ToString("0.0%");
                    subItem = new EXMultipleImagesListViewSubItem(new ArrayList(new object[] { _listImagesSized[index] }), rating.ToString("000.00%"));
                    subItem.Text = srating;
                    item.SubItems[_ID_LVRating] = subItem;
                }
                else
                {
                    subItem = new EXMultipleImagesListViewSubItem("");
                    subItem.MyValue = "#";
                    item.SubItems[_ID_LVRating] = subItem;
                }
            }
            lvGeocaches.EndUpdate();
        }

        private void popularityGCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (popularityGCToolStripMenuItem.Checked)
            {
                UpdateConfFile("useGCPopularityFormula", "False");
                popularityGCToolStripMenuItem.Checked = false;
                _bUseGCPopularity = false;
                _cacheDetail.EmptyStatsMarkers();
            }
            else
            {
                UpdateConfFile("popularityGCToolStripMenuItem", "True");
                popularityGCToolStripMenuItem.Checked = true;
                _bUseGCPopularity = true;
                _cacheDetail.EmptyStatsMarkers();
            }

            UpdateMenuChecks();
            UpdateListViewPopularity();
            PopulateListViewCache(null);
        }

        
        private void importOV2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
            	if (!SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled()) return;
            	
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "OV2 POI (*.ov2)|*.ov2";
                openFileDialog1.Multiselect = false;
                openFileDialog1.RestoreDirectory = true;
                DialogResult dr = openFileDialog1.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    string[] filenames = openFileDialog1.FileNames;
                    String fln = filenames[0];

                    String sType = "";
                    List<ParameterObject> lst = new List<ParameterObject>();
                    List<String> lstypes = GeocachingConstants.GetSupportedCacheTypes();
                    lst.Add(new ParameterObject(ParameterObject.ParameterType.List, lstypes, "stype", GetTranslator().GetString("LblTypeCache")));
                    lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "wher", GetTranslator().GetString("CheckIsSuperPPOV2")));

                    ParametersChanger changer = new ParametersChanger();
                    changer.Title = GetTranslator().GetString("LblTypeCache");
                    changer.BtnCancel = GetTranslator().GetString("BtnCancel");
                    changer.BtnOK = GetTranslator().GetString("BtnOk");
                    changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
                    changer.ErrorTitle = GetTranslator().GetString("Error");
                    changer.Parameters = lst;
                    changer.Font = this.Font;
                    changer.Icon = this.Icon;
                    
                    if (changer.ShowDialog() == DialogResult.OK)
                    {
                        sType = lst[0].Value;
                        bool bFromSuperPP = (lst[1].Value == "True");
                        _ThreadProgressBarTitle = "";
                        CreateThreadProgressBar();
                        OV2Reader ov2 = new OV2Reader();
                        List<Geocache> caches = ov2.ProcessFile(this, fln, sType, bFromSuperPP);

                        KillThreadProgressBar();
                        ExportGPXFromList(fln + ".gpx", caches, false);
                    }
                }
            }
            catch (Exception ex)
            {
                KillThreadProgressBar();
                ShowException("", "Import OV2 file", ex);
            } 
        }

        private void modifyNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ModifyCacheName();
        }

        private void modifyNameWithTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddTagsToTitle();
        }

        /// <summary>
        /// Handler for map display zoom changed
        /// </summary>
        public void cachesPreviewMap_OnMapZoomChanged()
        {
            GMapWrapper.OnMapZoomChanged(_cachesPreviewMap);
        }

        private void anymap_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            anymap_OnMarkerClickImpl(item, e, _cachesPreviewMap);
        }

        /// <summary>
        /// Handler called when a marker is clicked on map display
        /// </summary>
        /// <param name="item">clicked marker</param>
        /// <param name="e">mouse event</param>
        /// <param name="parentMap">parent map control</param>
        public void anymap_OnMarkerClickImpl(GMapMarker item, MouseEventArgs e, GMapControlCustom parentMap)
        {
            // Est-on sur une géocache ?
            // Si oui, on l'ouvre AVEC UN CLIC DROIT !
            if (e.Button == MouseButtons.Right)
            {
                GMapMarkerImages marker = null;
                if ((item != null) && (item.GetType() == typeof(GMapMarkerImages)))
                {
                    marker = (GMapMarkerImages)item;
                    Geocache geo = marker.GetGeocache();
                    if (geo != null)
                    {
                        DisplayDetailFromSelection(geo, false);
                    }
                }// Les waypoints ouvrent leur cache associée
                else if ((item != null) && (item.Tag != null) && (item.Tag.GetType() == typeof(Geocache)))
                {
                    // C'est surement un waypoint, en tout cas un objet avec une geocache associée, on ouvre donc la géocache
                    Geocache geo = (Geocache)(item.Tag);
                    if (geo != null)
                    {
                        DisplayDetailFromSelection(geo, false);
                    }
                }               
            }
            else if ((e.Button == MouseButtons.Left))// && (parentMap == _cachesPreviewMap)) // Quel que soit le père finalement...
            {
                try
                {
                    // Si on est dans _cachesPreviewMap, on sélectionne dans lvGeocaches
                    // la cache ainsi cliquée (ou la cache associée au waypoint)
                    int i = -1;
                    Geocache geo = null;
                    if ((item != null) && (item.GetType() == typeof(GMapMarkerImages)))
                    {
                        GMapMarkerImages marker = (GMapMarkerImages)item;
                        geo = marker.GetGeocache();
                        if (geo != null)
                        {
                            i = GetVisualEltIndexFromCacheCode(geo._Code);
                        }
                    }// Les waypoints ouvrent leur cache associée
                    else if ((item != null) && (item.Tag != null) && (item.Tag.GetType() == typeof(Geocache)))
                    {
                        // C'est surement un waypoint, en tout cas un objet avec une geocache associée, on ouvre donc la géocache
                        geo = (Geocache)(item.Tag);
                        if (geo != null)
                        {
                            i = GetVisualEltIndexFromCacheCode(geo._Code);
                        }
                    }
                    if (i != -1)
                    {
                        // On sélectionne cet item
                        EXListViewItem lstvItem = lvGeocaches.Items[i] as EXListViewItem;
                        SelectOnlyThisItemInlvGeocaches(lstvItem);
                        lvGeocaches.EnsureVisible(i);
                        if (parentMap == _cachesPreviewMap) // Car si on est dans la fenêtre flottante, on n'a pas envie de perdre son focus...
                            lvGeocaches.Focus();
                        DisplayFastCacheDetail(geo);
                    }
                }
                catch (Exception exc)
                {
                	Log("!!! " + GetException("anymap_OnMarkerClickImpl", exc));
                }
            }
        }

        /// <summary>
        /// Select a listviewitem in listview
        /// </summary>
        /// <param name="lstvItem">item to select</param>
        public void SelectOnlyThisItemInlvGeocaches(EXListViewItem lstvItem)
        {
            foreach (ListViewItem item in lvGeocaches.Items)
            {
                if (lstvItem == item)
                    item.Selected = true;
                else
                {
                    if (item.Selected)
                        item.Selected = false;
                }
            }
        }

        /// <summary>
        /// Called when a polygon is clicked
        /// </summary>
        /// <param name="item">clicked polygon</param>
        /// <param name="e">mouse event</param>
        public void cachedetail_OnPolygonClick(GMapPolygon item, MouseEventArgs e)
        {
        	// To make it activable, each Polygon shall have IsHitTestVisible = true !
        	//if (e.Button == MouseButtons.Right)
        	//	MSG("Polygon");
        }
        
        
        private void ExportRouteToGPX(object sender, EventArgs e)
        {
        	try
        	{
				ToolStripMenuItem item = sender as ToolStripMenuItem;
				if ((item != null) && (item.Tag != null)) 
				{
					GMapRoute route = item.Tag as GMapRoute;
					if (route != null) 
					{
						SaveFileDialog saveFileDialog1 = new SaveFileDialog();
			
						saveFileDialog1.Filter = "GPX (*.gpx)|*.gpx";
			
						saveFileDialog1.RestoreDirectory = true;
						if (saveFileDialog1.ShowDialog() == DialogResult.OK) 
						{
							System.IO.StreamWriter file = new System.IO.StreamWriter(saveFileDialog1.FileName, false, Encoding.Default);
	
							/*
<?xml version="1.0" encoding="UTF-8"?>
<gpx version="1.0">
	<name>Example gpx</name>
	<wpt lat="46.57638889" lon="8.89263889">
		<ele>2372</ele>
		<name>LAGORETICO</name>
	</wpt>
	<trk><name>Example gpx</name><number>1</number><trkseg>
		<trkpt lat="46.57608333" lon="8.89241667"><ele>2376</ele><time>2007-10-14T10:09:57Z</time></trkpt>
		<trkpt lat="46.57619444" lon="8.89252778"><ele>2375</ele><time>2007-10-14T10:10:52Z</time></trkpt>
		<trkpt lat="46.57641667" lon="8.89266667"><ele>2372</ele><time>2007-10-14T10:12:39Z</time></trkpt>
		<trkpt lat="46.57650000" lon="8.89280556"><ele>2373</ele><time>2007-10-14T10:13:12Z</time></trkpt>
		<trkpt lat="46.57638889" lon="8.89302778"><ele>2374</ele><time>2007-10-14T10:13:20Z</time></trkpt>
		<trkpt lat="46.57652778" lon="8.89322222"><ele>2375</ele><time>2007-10-14T10:13:48Z</time></trkpt>
		<trkpt lat="46.57661111" lon="8.89344444"><ele>2376</ele><time>2007-10-14T10:14:08Z</time></trkpt>
	</trkseg></trk>
</gpx>
							 */
							
							file.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
							file.WriteLine("<gpx version=\"1.0\">");
							file.WriteLine("	<name>MGM exported route</name>");
							file.WriteLine("	<trk><name>MGM route</name><number>1</number>");
							
							// on stocke les points avec une date farfelue
							DateTime date = DateTime.Now;
							
							// ATTENTION !
							// On va exporter toutes les routes de l'overlay associé à la route passée
							foreach(GMapRoute r in route.Overlay.Routes)
							{
								if ((r.Points != null) && (r.Points.Count != 0))
								{
									// On sauve cette route
									file.WriteLine("	<trkseg>");
									foreach (PointLatLng pt in r.Points) 
									{
										String sdate = date.ToString("yyyy-MM-ddTHH:mm:ssZ");
										String lineformat = "		<trkpt lat=\"{0}\" lon=\"{1}\"><ele>{2}</ele><time>{3}</time></trkpt>";
										file.WriteLine(String.Format(lineformat, pt.Lat.ToString().Replace(",","."), pt.Lng.ToString().Replace(",","."), 0, sdate));
										
										// Each points are separated of 1 minute
										date = date.AddMinutes(1.0);
									}
									file.WriteLine("	</trkseg>");
								}
							}
							file.WriteLine("	</trk>");
							file.WriteLine("</gpx>");
							file.Close();
							
							MsgActionDone(this);
						}
					}
				}
        	}
        	catch(Exception ex)
        	{
        		ShowException("", GetTranslator().GetString("FMenuExportTrack"), ex);
        	}
        }
        
        private void ExportRouteToGPXBasecamp(object sender, EventArgs e)
        {
        	try
        	{
				ToolStripMenuItem item = sender as ToolStripMenuItem;
				if ((item != null) && (item.Tag != null)) 
				{
					GMapRoute route = item.Tag as GMapRoute;
					if (route != null) 
					{
						SaveFileDialog saveFileDialog1 = new SaveFileDialog();
			
						saveFileDialog1.Filter = "GPX (*.gpx)|*.gpx";
			
						saveFileDialog1.RestoreDirectory = true;
						if (saveFileDialog1.ShowDialog() == DialogResult.OK) 
						{
							System.IO.StreamWriter file = new System.IO.StreamWriter(saveFileDialog1.FileName, false, Encoding.Default);
	
							/*
<?xml version="1.0" encoding="utf-8"?><gpx creator="Garmin Desktop App" version="1.1" xsi:schemaLocation="http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd http://www.garmin.com/xmlschemas/WaypointExtension/v1 http://www8.garmin.com/xmlschemas/WaypointExtensionv1.xsd http://www.garmin.com/xmlschemas/TrackPointExtension/v1 http://www.garmin.com/xmlschemas/TrackPointExtensionv1.xsd http://www.garmin.com/xmlschemas/GpxExtensions/v3 http://www8.garmin.com/xmlschemas/GpxExtensionsv3.xsd http://www.garmin.com/xmlschemas/ActivityExtension/v1 http://www8.garmin.com/xmlschemas/ActivityExtensionv1.xsd http://www.garmin.com/xmlschemas/AdventuresExtensions/v1 http://www8.garmin.com/xmlschemas/AdventuresExtensionv1.xsd http://www.garmin.com/xmlschemas/PressureExtension/v1 http://www.garmin.com/xmlschemas/PressureExtensionv1.xsd http://www.garmin.com/xmlschemas/TripExtensions/v1 http://www.garmin.com/xmlschemas/TripExtensionsv1.xsd http://www.garmin.com/xmlschemas/TripMetaDataExtensions/v1 http://www.garmin.com/xmlschemas/TripMetaDataExtensionsv1.xsd http://www.garmin.com/xmlschemas/ViaPointTransportationModeExtensions/v1 http://www.garmin.com/xmlschemas/ViaPointTransportationModeExtensionsv1.xsd http://www.garmin.com/xmlschemas/CreationTimeExtension/v1 http://www.garmin.com/xmlschemas/CreationTimeExtensionsv1.xsd http://www.garmin.com/xmlschemas/AccelerationExtension/v1 http://www.garmin.com/xmlschemas/AccelerationExtensionv1.xsd http://www.garmin.com/xmlschemas/PowerExtension/v1 http://www.garmin.com/xmlschemas/PowerExtensionv1.xsd http://www.garmin.com/xmlschemas/VideoExtension/v1 http://www.garmin.com/xmlschemas/VideoExtensionv1.xsd" xmlns="http://www.topografix.com/GPX/1/1" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:wptx1="http://www.garmin.com/xmlschemas/WaypointExtension/v1" xmlns:gpxtrx="http://www.garmin.com/xmlschemas/GpxExtensions/v3" xmlns:gpxtpx="http://www.garmin.com/xmlschemas/TrackPointExtension/v1" xmlns:gpxx="http://www.garmin.com/xmlschemas/GpxExtensions/v3" xmlns:trp="http://www.garmin.com/xmlschemas/TripExtensions/v1" xmlns:adv="http://www.garmin.com/xmlschemas/AdventuresExtensions/v1" xmlns:prs="http://www.garmin.com/xmlschemas/PressureExtension/v1" xmlns:tmd="http://www.garmin.com/xmlschemas/TripMetaDataExtensions/v1" xmlns:vptm="http://www.garmin.com/xmlschemas/ViaPointTransportationModeExtensions/v1" xmlns:ctx="http://www.garmin.com/xmlschemas/CreationTimeExtension/v1" xmlns:gpxacc="http://www.garmin.com/xmlschemas/AccelerationExtension/v1" xmlns:gpxpx="http://www.garmin.com/xmlschemas/PowerExtension/v1" xmlns:vidx1="http://www.garmin.com/xmlschemas/VideoExtension/v1">
  <metadata>
    <link href="http://www.garmin.com">
      <text>Garmin International</text>
    </link>
    <time>2016-01-06T11:04:26Z</time>
    <bounds maxlat="43.617936233058572" maxlon="4.011503085494041" minlat="43.615627521649003" minlon="4.006393728777766" />
  </metadata>

  <trk>
    <name>basecamp-ok</name>
    <extensions>
      <gpxx:TrackExtension>
        <gpxx:DisplayColor>DarkGray</gpxx:DisplayColor>
      </gpxx:TrackExtension>
    </extensions>
    <trkseg>
      <trkpt lat="43.616687580943108" lon="4.006547704339027" />
      <trkpt lat="43.617451172322035" lon="4.008242357522249" />
      <trkpt lat="43.617505067959428" lon="4.008499095216394" />
      <trkpt lat="43.617936233058572" lon="4.009924018755555" />
      <trkpt lat="43.616570821031928" lon="4.011413231492043" />
      <trkpt lat="43.616445008665323" lon="4.011503085494041" />
      <trkpt lat="43.616292290389538" lon="4.00956460274756" />
      <trkpt lat="43.615627521649003" lon="4.006958585232496" />
      <trkpt lat="43.616570821031928" lon="4.006393728777766" />
    </trkseg>
  </trk>

</gpx>
							 */
							file.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><gpx creator=\"Garmin Desktop App\" version=\"1.1\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd http://www.garmin.com/xmlschemas/WaypointExtension/v1 http://www8.garmin.com/xmlschemas/WaypointExtensionv1.xsd http://www.garmin.com/xmlschemas/TrackPointExtension/v1 http://www.garmin.com/xmlschemas/TrackPointExtensionv1.xsd http://www.garmin.com/xmlschemas/GpxExtensions/v3 http://www8.garmin.com/xmlschemas/GpxExtensionsv3.xsd http://www.garmin.com/xmlschemas/ActivityExtension/v1 http://www8.garmin.com/xmlschemas/ActivityExtensionv1.xsd http://www.garmin.com/xmlschemas/AdventuresExtensions/v1 http://www8.garmin.com/xmlschemas/AdventuresExtensionv1.xsd http://www.garmin.com/xmlschemas/PressureExtension/v1 http://www.garmin.com/xmlschemas/PressureExtensionv1.xsd http://www.garmin.com/xmlschemas/TripExtensions/v1 http://www.garmin.com/xmlschemas/TripExtensionsv1.xsd http://www.garmin.com/xmlschemas/TripMetaDataExtensions/v1 http://www.garmin.com/xmlschemas/TripMetaDataExtensionsv1.xsd http://www.garmin.com/xmlschemas/ViaPointTransportationModeExtensions/v1 http://www.garmin.com/xmlschemas/ViaPointTransportationModeExtensionsv1.xsd http://www.garmin.com/xmlschemas/CreationTimeExtension/v1 http://www.garmin.com/xmlschemas/CreationTimeExtensionsv1.xsd http://www.garmin.com/xmlschemas/AccelerationExtension/v1 http://www.garmin.com/xmlschemas/AccelerationExtensionv1.xsd http://www.garmin.com/xmlschemas/PowerExtension/v1 http://www.garmin.com/xmlschemas/PowerExtensionv1.xsd http://www.garmin.com/xmlschemas/VideoExtension/v1 http://www.garmin.com/xmlschemas/VideoExtensionv1.xsd\" xmlns=\"http://www.topografix.com/GPX/1/1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:wptx1=\"http://www.garmin.com/xmlschemas/WaypointExtension/v1\" xmlns:gpxtrx=\"http://www.garmin.com/xmlschemas/GpxExtensions/v3\" xmlns:gpxtpx=\"http://www.garmin.com/xmlschemas/TrackPointExtension/v1\" xmlns:gpxx=\"http://www.garmin.com/xmlschemas/GpxExtensions/v3\" xmlns:trp=\"http://www.garmin.com/xmlschemas/TripExtensions/v1\" xmlns:adv=\"http://www.garmin.com/xmlschemas/AdventuresExtensions/v1\" xmlns:prs=\"http://www.garmin.com/xmlschemas/PressureExtension/v1\" xmlns:tmd=\"http://www.garmin.com/xmlschemas/TripMetaDataExtensions/v1\" xmlns:vptm=\"http://www.garmin.com/xmlschemas/ViaPointTransportationModeExtensions/v1\" xmlns:ctx=\"http://www.garmin.com/xmlschemas/CreationTimeExtension/v1\" xmlns:gpxacc=\"http://www.garmin.com/xmlschemas/AccelerationExtension/v1\" xmlns:gpxpx=\"http://www.garmin.com/xmlschemas/PowerExtension/v1\" xmlns:vidx1=\"http://www.garmin.com/xmlschemas/VideoExtension/v1\">");
							file.WriteLine("<metadata>");
						    file.WriteLine("<link href=\"http://www.garmin.com\">");
						    file.WriteLine("  <text>Garmin International</text>");
						    file.WriteLine("</link>");
							DateTime date = DateTime.Now;
							String sdate = date.ToString("yyyy-MM-ddTHH:mm:ssZ");
							file.WriteLine("<time>"+sdate+"</time>");
							
							double minlat = 90.0;
				            double minlon = 180.0;
				            double maxlat = -90.0;
				            double maxlon = -180.0;
				            foreach(GMapRoute r in route.Overlay.Routes)
							{
								if ((r.Points != null) && (r.Points.Count != 0))
								{
									// On sauve cette route
									foreach (PointLatLng pt in r.Points) 
									{
										if (pt.Lat < minlat)
						                    minlat = pt.Lat;
						                if (pt.Lng < minlon)
						                    minlon = pt.Lng;
						                if (pt.Lat > maxlat)
						                    maxlat = pt.Lat;
						                if (pt.Lng > maxlon)
						                    maxlon = pt.Lng;
									}
								}
				            }
				            String bounds = "";
				            bounds = "  <bounds " 
				            	+    "maxlat=\"" + maxlat
				                + "\" maxlon=\"" + maxlon 
				            	+ "\" minlat=\"" + minlat
				                + "\" minlon=\"" + minlon
				                + "\" />";
				            bounds = bounds.Replace(",", ".");
				            file.WriteLine(bounds);
							// <bounds maxlat="43.617936233058572" maxlon="4.011503085494041" minlat="43.615627521649003" minlon="4.006393728777766" />
							
							file.WriteLine("</metadata>");
						    file.WriteLine("	<trk>");
							file.WriteLine("    <name>basecamp-ok</name>");
							file.WriteLine("    <extensions>");
							file.WriteLine("      <gpxx:TrackExtension>");
							file.WriteLine("        <gpxx:DisplayColor>DarkGray</gpxx:DisplayColor>");
							file.WriteLine("      </gpxx:TrackExtension>");
 							file.WriteLine("   </extensions>");
							// ATTENTION !
							// On va exporter toutes les routes de l'overlay associé à la route passée
							foreach(GMapRoute r in route.Overlay.Routes)
							{
								if ((r.Points != null) && (r.Points.Count != 0))
								{
									// On sauve cette route
									file.WriteLine("	<trkseg>");
									foreach (PointLatLng pt in r.Points) 
									{
										sdate = date.ToString("yyyy-MM-ddTHH:mm:ssZ");
										String lineformat = "		<trkpt lat=\"{0}\" lon=\"{1}\"><ele>{2}</ele><time>{3}</time></trkpt>";
										file.WriteLine(String.Format(lineformat, pt.Lat.ToString().Replace(",","."), pt.Lng.ToString().Replace(",","."), 0, sdate));
										
										// Each points are separated of 1 minute
										date = date.AddMinutes(1.0);
									}
									file.WriteLine("	</trkseg>");
								}
							}
							file.WriteLine("	</trk>");
							file.WriteLine("</gpx>");
							file.Close();
							
							MsgActionDone(this);
						}
					}
				}
        	}
        	catch(Exception ex)
        	{
        		ShowException("", GetTranslator().GetString("FMenuExportTrack"), ex);
        	}
        }
        
        /// <summary>
        /// Called when a route is clicked
        /// </summary>
        /// <param name="item">clicked route</param>
        /// <param name="e">mouse event</param>
        public void cachedetail_OnRouteClick(GMapRoute item, MouseEventArgs e)
        {
        	if (e.Button == MouseButtons.Right)
        	{
        		// Export de la route dans un GPX	
        		Point pt = this.PointToClient(Cursor.Position);
        		ContextMenuStrip mnu = new ContextMenuStrip();
            	ToolStripMenuItem selMenu = CreateTSMI("FMenuExportTrack", ExportRouteToGPX);
            	selMenu.Tag = item;
            	mnu.Items.Add(selMenu);
            	selMenu = CreateTSMI("FMenuExportTrackBasecamp", ExportRouteToGPXBasecamp);
            	selMenu.Tag = item;
            	mnu.Items.Add(selMenu);
            	mnu.Show(this, pt);
        	}
        	else
        	{
	            if ((item.Tag != null) && ((item.Tag.GetType() == typeof(GMapControl)) || (item.Tag.GetType() == typeof(GMapControlCustom))))
	            {
	                if (_cacheDetail._bDoingMeasure)
	                {
	                    // On ne fait rien !!!
	                }
	                else
	                {
	                    GMapControl map = item.Tag as GMapControl;
	                    // On va stocker ce marker dans RESERVED1
	                    map.Overlays[GMapWrapper.RESERVED1].Markers.Clear();
	
	                    GMapMarkerImage marker = new GMapMarkerImage(_listImagesSized[getIndexImages("CenterView")], map.FromLocalToLatLng(e.X, e.Y));
	                    marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
	                    marker.ToolTipText = item.Name;
	                    map.Overlays[GMapWrapper.RESERVED1].Markers.Add(marker);
	                }
	            }
        	}
        }
        

        private void btnCartoDisplay_Click(object sender, EventArgs e)
        {
            ShowCacheMapInCacheDetail();
        }

        /// <summary>
        /// Reset filter area
        /// </summary>
        public void ResetAreaFilter()
        {
            Filter.ResetArea();
        }

        /// <summary>
        /// Define filter area
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="pts">area points</param>
        /// <param name="zoom">zoom level</param>
        /// <param name="bIHMImpact">if true, area filter will be checked</param>
        /// <param name="bSilent">if true, error messages will not be displayed</param>
        /// <returns>true if area successfuly defined</returns>
        public bool DefineFilterArea(Form caller, List<PointLatLng> pts, int zoom, bool bIHMImpact, bool bSilent)
        {
            if ((pts == null) || (pts.Count < 3))
            {
                if (!bSilent)
                    MsgActionError(caller, GetTranslator().GetString("ErrToSmallArea"));
                return false;
            }

            Filter.SetAreaZoom(zoom.ToString());
            String clip = "";
            foreach (PointLatLng pt in pts)
            {
                clip += pt.Lat.ToString() + ";" + pt.Lng.ToString() + ":";
            }
            clip.Replace(",", ".");

            if (!Filter.DefineArea(clip))
            {
                if (!bSilent)
                    MsgActionError(caller, GetTranslator().GetString("ErrToSmallArea"));
                return false;
            }

            // Else we update filter tab
            if (bIHMImpact)
                cbFilterArea.Checked = true;
            Filter._bFilterArea = true;
            return true;
        }

        private void SetDisplaySelectionOnCarto(object sender, EventArgs e)
        {
            // On affiche uniquement la sélection dans Google Maps.
            // de base, tous les marqueurs sont déjà créés
            // on va juste masquer ceux qui ne sont pas sélectionnés.
            List<Geocache> caches = GetSelectedCaches();
            _cacheDetail._gmap.HoldInvalidation = true;
            _cacheDetail.EmptyAllDecorationOverlays();
            
            foreach (GMapMarker marker in _cacheDetail._gmap.Overlays[GMapWrapper.MARKERS].Markers)
            {
                GMapMarkerImages m = marker as GMapMarkerImages;
                if (m != null)
                {
                    // L'objet Géocacache est-il sélectionné ?
                    Geocache geo = m.GetGeocache();
                    if (caches.Contains(geo))
                        m.IsVisible = true;
                    else
                        m.IsVisible = false;
                }
            }
            _cacheDetail._gmap.Refresh();
            ShowCacheMapInCacheDetail();
        }

        private void excludeMissingCachesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<ParameterObject> lst = new List<ParameterObject>();
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "disponlyexcl", GetTranslator().GetString("LblDisplayMissingCaches")));
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Label, "", "lbl1", GetTranslator().GetString("LblCacheInvalidityConditions")));
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Int, 3, "mindnf", GetTranslator().GetString("LblExcludeCacheMinEvents")));
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "up1", System.Web.HttpUtility.HtmlDecode(GetTranslator().GetString("LOG_Didnt_find_it"))));
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "up2", System.Web.HttpUtility.HtmlDecode(GetTranslator().GetString("LOG_Needs_Archived"))));
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "up3", System.Web.HttpUtility.HtmlDecode(GetTranslator().GetString("LOG_Needs_Maintenance"))));
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Label, "", "lbl1", GetTranslator().GetString("LblCacheValidityConditions")));
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "p1", System.Web.HttpUtility.HtmlDecode(GetTranslator().GetString("LOG_Found_it"))));
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "p2", System.Web.HttpUtility.HtmlDecode(GetTranslator().GetString("LOG_Enable_Listing"))));
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "p3", System.Web.HttpUtility.HtmlDecode(GetTranslator().GetString("LOG_Owner_Maintenance"))));
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "p4", System.Web.HttpUtility.HtmlDecode(GetTranslator().GetString("LOG_Update_Coordinates"))));
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "p5", System.Web.HttpUtility.HtmlDecode(GetTranslator().GetString("LOG_Unarchive"))));
            

            ParametersChanger changer = new ParametersChanger();
            changer.Title = GetTranslator().GetString("FMenuToolsExcludeMissingCaches");
            changer.BtnCancel = GetTranslator().GetString("BtnCancel");
            changer.BtnOK = GetTranslator().GetString("BtnOk");
            changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
            changer.ErrorTitle = GetTranslator().GetString("Error");
            changer.Parameters = lst;
            changer.Font = this.Font;
            changer.Icon = this.Icon;

            if (changer.ShowDialog() == DialogResult.OK)
            {
                bool bDisplayOnlyExcluded = (lst[0].Value == "True");
                int minNbDNF = Int32.Parse(lst[2].Value);
                bool[] tConditionsNegatives = new bool[3];
                bool[] tConditionsPositives = new bool[5];
                for(int i=0;i<3;i++)
                    tConditionsNegatives[i] = (lst[3 + i].Value == "True");
                for (int i = 0; i < 5; i++)
                    tConditionsPositives[i] = (lst[7 + i].Value == "True");

                CustomFilterExcludeMissingCaches fltr = new CustomFilterExcludeMissingCaches(tConditionsNegatives, tConditionsPositives, minNbDNF, bDisplayOnlyExcluded);
                ExecuteCustomFilter(fltr);
            }
        }

        private void markMissingCachesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (markMissingCachesToolStripMenuItem.Checked)
            {
                UpdateConfFile("markmissingcaches", "False");
            }
            else
            {
                UpdateConfFile("markmissingcaches", "True");
            }
            UpdateMenuChecks();
            
            // On reaffiche les caches
            BuildListViewCache();
            PopulateListViewCache(null);
        }

        private void donateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MyTools.StartInNewThread("https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=KEFCJVN5QL6R4&lc=FR&item_name=MyGeocachingManager&currency_code=EUR&bn=PP%2dDonationsBF%3abtn_donate_LG%2egif%3aNonHosted");
        }

        
        private void displayDirectPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            // On efface l'itinéraire précédent
            _cacheDetail.EmptyRouteMarkers();
            ClearOverlay_RESERVED2();

            // On crée la route
            List<PointLatLng> pts = new List<PointLatLng>();

            // On va parcourir toutes les caches affichées dans l'ordre de tri de la liste
            List<Geocache> caches = GetDisplayedCaches();
            Geocache previous = null;
            double distance = 0;
            foreach (Geocache geo in caches)
            {
                if (previous != null)
                {
                    distance += geo.DistanceToCoord(previous._dLatitude, previous._dLongitude);
                }
                pts.Add(new PointLatLng(geo._dLatitude, geo._dLongitude));
                previous = geo;
            }

            if ((pts != null) && (pts.Count != 0))
            {
                // La route en ligne droite
                GMapRoute route = new GMapRoute(pts, "Route");
                route.IsHitTestVisible = true;
                route.Tag = _cacheDetail._gmap; // très important pour le tooltip
                Pen pen = new Pen(Color.Red);
                pen.Width = 2;// route.Stroke.Width;
                route.Stroke = pen;

                // On change le nom de cette route pour le tooltip
                String kmmi = (_bUseKm) ? GetTranslator().GetString("LVKm") : GetTranslator().GetString("LVMi");
                double dist = (_bUseKm) ? distance : distance * _dConvKmToMi;
                String tooltiptext = "";
                tooltiptext = dist.ToString("0.0") + " " + kmmi;
                route.Name = tooltiptext;
                
                _cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2].Routes.Add(route);

                // On affiche
                ShowCacheMapInCacheDetail();
            }
            
        }

        /*
         Google API Elevation
https://developers.google.com/maps/documentation/elevation/
Users of the free API:
2500 requests per 24 hour period.
512 locations per request.
5 requests per second.
         */

        private void GoogleAPIRequestElevation(int nbresponses, string bloccaches, ref List<double> altitudes)
        {
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            WebProxy proxy = GetProxy();
            if (proxy != null)
                client.Proxy = proxy;
            string url = "https://maps.googleapis.com/maps/api/elevation/xml?locations=" + bloccaches;
            string result = client.DownloadString(url);
            // On parse le résultat
            try
            {
                Log("parsing altitudes");
                //Log(result);
                XmlDocument _xmldoc;
                XmlNodeList _xmlnode;
                _xmldoc = new XmlDocument();
                _xmldoc.LoadXml(result);
                
// <status>OK</status>
// <result>
//  <location>
//   <lat>48.7526670</lat>
//   <lng>1.9898170</lng>
//  </location>
//  <elevation>177.2228241</elevation>
//  <resolution>152.7032318</resolution>
// </result>
// <result>
// ...           
                // Le status
                String status = _xmldoc.SelectNodes("/ElevationResponse/status").Item(0).InnerText.Trim();
                Log("Status : " + status);
                if (status != "OK")
                    throw new Exception("Bad response from Google API Elevation: " + status);

                // On parcours les réponses
                _xmlnode = _xmldoc.SelectNodes("/ElevationResponse/result");
                Log("Reading " + _xmlnode.Count.ToString() + " results for " + nbresponses.ToString() + " expected");
                // Parse each node
                foreach (XmlNode node in _xmlnode)
                {
                    String alti = MyTools.getNodeValue(node, "elevation");
                    Log("Altitude : " + alti);
                    altitudes.Add(MyTools.ConvertToDouble(alti));
                }
            }
            catch (Exception exc)
            {
            	Log("!!!! " + GetException("Get elevation from Google API", exc));
                Log("!!!! Error parsing altitudes");
            }
        }


        /// <summary>
        /// Get cache altitudes using Google Maps
        /// </summary>
        /// <param name="cachestoprocess">caches to process</param>
        /// <returns>list of cache altitudes</returns>
        public List<double> GetAltitudes(List<Geocache> cachestoprocess)
        {
            try
            {
                string bloccaches = "";
                int inum = 0;
                int iiter = 1;
                int iNbCachesPerRequest = 50;// 512; // au dela de 50 ça semble merder...
                List<double> altitudes = new List<double>();

                _ThreadProgressBarTitle = GetTranslator().GetString("FMenuToolsAlti");
                CreateThreadProgressBarEnh();

                // Wait for the creation of the bar
                while (_ThreadProgressBar == null)
                {
                    Thread.Sleep(10);
                    Application.DoEvents();
                }
                _ThreadProgressBar.progressBar1.Maximum = (cachestoprocess.Count() / iNbCachesPerRequest) + 1;

                // On itère sur toutes les caches
                foreach (Geocache geo in cachestoprocess)
                {
                    // On ajoute chaque cache au bloc à traiter
                    if (bloccaches == "")
                        bloccaches = geo._Latitude + "," + geo._Longitude;
                    else
                        bloccaches += "|" + geo._Latitude + "," + geo._Longitude;

                    inum++;

                    // On est prêt à faire une requête !
                    if (inum == iNbCachesPerRequest)
                    {
                        // REQUETE !!!
                        // On parse le résultat
                        GoogleAPIRequestElevation(inum, bloccaches, ref altitudes);
                        Thread.Sleep(1000); // Pour ne pas saturer l'API Google
                        inum = 0;
                        iiter++;
                        bloccaches = "";

                        _ThreadProgressBar.Step();
                        if (_ThreadProgressBar._bAbort)
                        {
                            KillThreadProgressBarEnh();
                            return null;
                        }
                    }
                }
                if (bloccaches != "")
                {
                    // REQUETE !!!
                    // On parse le résultat
                    GoogleAPIRequestElevation(inum, bloccaches, ref altitudes);
                    bloccaches = "";

                    _ThreadProgressBar.Step();
                    if (_ThreadProgressBar._bAbort)
                    {
                        KillThreadProgressBarEnh();
                        return null;
                    }
                }
                KillThreadProgressBarEnh();
                return altitudes;
            }
            catch (Exception)
            {
                KillThreadProgressBarEnh();
                throw;
            }
        }

        private void computeAltiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Geocache> caches = null;
            try
            {
                if (!_bInternetAvailable)
                {
                    MsgActionError(this, GetTranslator().GetString("LblInternetNo"));
                    return;
                }

                caches = GetSelectedCaches();
            	if (caches.Count == 0)
            	{
            		MsgActionWarning(this, "LblErrorNoSelection");
            		return;
            	}

                // Calcul des altitudes
                List<double> altitudes = GetAltitudes(caches);

                // Modification des objets geocache
                OfflineCacheData ocd1 = null;
                int index = 0;
                foreach (Geocache cache in caches)
                {
                    double dalti = altitudes[index];
                    index++;

                    if (dalti != Double.MaxValue)
                    {
                        String code = cache._Code;
                        if (_od._OfflineData.ContainsKey(code))
                        {
                            ocd1 = _od._OfflineData[code];
                        }
                        else
                        {
                            ocd1 = new OfflineCacheData();
                            ocd1._Code = code;
                            AssociateOcdCache(code, ocd1, cache);
                        }

                        // Mise à jour de _dAltiMeter
                        ocd1._dAltiMeters = dalti;
                    }
                }

                // Final wrapup
                _od.Serialize(_odfile);

                PostTreatmentLoadCache();

                // Better way to do that : only recreate for modified caches
                RecreateVisualElements(caches);

                MsgActionDone(this);
            }
            catch (Exception ex)
            {
                PostTreatmentLoadCache();

                // Better way to do that : only recreate for modified caches
                RecreateVisualElements(caches);

                ShowException("", "Compute altitude", ex);
            }
        }

        /// <summary>
        /// Check geocaching account credentials
        /// </summary>
        /// <param name="silent">if true, no message box will be raised for SUCCESS</param>
        /// <param name="bForceRegeneration">if true, force cookie regeneration, otherwise use (if exists) a previously create cookie </param>
        /// <returns>authentication cookie</returns>
        public CookieContainer CheckGCAccount(bool silent, bool bForceRegeneration)
        {
            try
            {
                Log("Verification login/mdp");
                String username = ConfigurationManager.AppSettings["owner"];
                String password = GetDecryptedOwnerPassword();
                if ((username == "") || (password == ""))
                {
                    MsgActionError(this, GetTranslator().GetString("LblIncorrectGCAccountPwdEmpty"));
                    return null;
                }
                else
                {

                    // Si on a deja un cookie valide et qu'on ne force pas sa régénération, alors on le retourne
                    // Gain de temps énorme
                    if ((_cookieJar != null) && (!bForceRegeneration))
                        return _cookieJar;

                    /* Penser à inclure les dépendances suivantes : 
                     * using System.Net;
                     * using System.IO;
                     * using System.Web;
                     * 
                     * et les références :
                     * System.Web
                     * System.Net
                     * 
                     * Définir les variables suivantes :
                     * String username = "METTRE VOTRE LOGIN ICI";
                     * String password = "METTRE VOTRE MDP EN CLAIR ICI";
                     */

                    // Notre container de cookies
                    CookieContainer cookieJar = new CookieContainer();

                    // Authentification sur GC.com
                    // ***************************

                    // TSL 1.2
                    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                    // Récupération des VIEWSTATE pour s'authentifier
                    string VOID_URL = "https://www.geocaching.com/account/login";
                    HttpWebResponse objResponse = null;
                    HttpWebRequest objRequest = null;
                    //if (false)
                    //	objResponse = MyTools.GetHttpRequestWithEncoding(VOID_URL, GetProxy(), 3000);
                    //else
                    {
                    	objRequest = (HttpWebRequest)WebRequest.Create(VOID_URL);
                        objRequest.CookieContainer = cookieJar;
                        objRequest.Proxy = GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
		            	objResponse = (HttpWebResponse)objRequest.GetResponse();
                    }
                    int cookieCount = cookieJar.Count;

                    if (objResponse == null)
		            {
		            	if (!silent)
		            	{
			            	MsgActionError(this, GetTranslator().GetString("LblErrorGCAccess"));
		            	}
		            	return null;
		            }
		            
		            String post_response = "";
		            using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
		            {
		                post_response = responseStream.ReadToEnd();
		                responseStream.Close();
		            }

                    // On récupère le token de vérification
                    String token = MyTools.GetSnippetFromText("__RequestVerificationToken\" type=\"hidden\" value=\"", "\"", post_response);
                    
                    // Préparation des données du POST
                    Dictionary<String, String> post_values = new Dictionary<String, String>();
                    //post_values.Add("__EVENTTARGET", "");
                    //post_values.Add("__EVENTARGUMENT", "");
                    post_values.Add("Username", username);
                    post_values.Add("Password", password);
					post_values.Add("__RequestVerificationToken", token);

                    // Encodage des données du POST
                    String post_string = "";
                    foreach (KeyValuePair<String, String> post_value in post_values)
                    {
                        post_string += post_value.Key + "=" + HttpUtility.UrlEncode(post_value.Value) + "&";
                    }
                    post_string = post_string.TrimEnd('&');
                    
                    // Création de la requête pour s'authentifier
                    objRequest = (HttpWebRequest)WebRequest.Create(VOID_URL);
                    objRequest.Method = "POST";
                    objRequest.ContentLength = post_string.Length;
                    objRequest.ContentType = "application/x-www-form-urlencoded";
                    objRequest.Proxy = GetProxy(); // Créer votre proxy ici si besoin, sinon mettre NULL
                    objRequest.CookieContainer = cookieJar;
                    //objRequest.KeepAlive = false; // PATCH SARCE ?

                    // on envoit les POST data dans un stream (écriture)
                    StreamWriter myWriter = null;
                    myWriter = new StreamWriter(objRequest.GetRequestStream());
                    myWriter.Write(post_string);
                    myWriter.Close();

                    // lecture du stream de réponse et conversion en chaine
                    objResponse = (HttpWebResponse)objRequest.GetResponse();
                    using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
                    {
                        post_response = responseStream.ReadToEnd();
                        responseStream.Close();
                    }
                    
                    // Pour le debug, interrogation du nombre de cookies retournés et trace de cette valeur
                    // peut être supprimé
                    cookieCount = cookieJar.Count;
                    bool loggedin = true; // DEPRECATED !!!  post_response.Contains("isLoggedIn: true,");
                    String userloginfromgc = MyTools.GetSnippetFromText("currentUsername: \"", "\",", post_response);
                    if (loggedin && (userloginfromgc.ToLower() == username.ToLower()))
                    {
                        _cookieJar = cookieJar;
                        if (!silent)
                        {
                            MsgActionOk(this, GetTranslator().GetString("LblCorrectGCAccount"));
                        }
                        
                        // Un petit dernier pour la route, on vérifie si on est premium
                        // UserIsPremium
                        try
                        {
                            UserIsPremium = post_response.Contains("currentUserIsPremium: true");
                        }
                        catch(Exception)
                        {
                        	// On considère qu'on est premium...
                        	UserIsPremium = true;
                        }
                        
                        return cookieJar;
                    }
                    else
                    {
                        _cookieJar = null;
                        Log("userloginfromgc = " + userloginfromgc);
                        MsgActionError(this, GetTranslator().GetString("LblIncorrectGCAccount"));
                        return null;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                _cookieJar = null;
                ShowException("", "Checking Geocaching.com account", ex);
                return null;
            }
        }

        private void checkGCaccountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckGCAccount(false, true);
        }

        private void filterAltitudeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<ParameterObject> lst = new List<ParameterObject>();
            List<String> lstcompare = new List<string>();
            lstcompare.Add(">=");
            lstcompare.Add("=");
            lstcompare.Add("<=");
            lst.Add(new ParameterObject(ParameterObject.ParameterType.List, lstcompare, "lstcompare", GetTranslator().GetString("LblAltiIs")));
            String lbl = "";
            if (_bUseKm)
                lbl = GetTranslator().GetString("LblCompareTo") + " (m)";
            else
                lbl = GetTranslator().GetString("LblCompareTo") + " (ft)";
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Double, 1000.0, "nbval", lbl));

            ParametersChanger changer = new ParametersChanger();
            changer.Title = GetTranslator().GetString("FMenuFltAltitude");
            changer.BtnCancel = GetTranslator().GetString("BtnCancel");
            changer.BtnOK = GetTranslator().GetString("BtnOk");
            changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
            changer.ErrorTitle = GetTranslator().GetString("Error");
            changer.Parameters = lst;
            changer.Font = this.Font;
            changer.Icon = this.Icon;

            if (changer.ShowDialog() == DialogResult.OK)
            {
                String comparestring = changer.Parameters[0].Value;
                Double value = MyTools.ConvertToDouble(changer.Parameters[1].Value);
                if (!_bUseKm)
                {
                    // L'altitude a été entrée en feet, mais nous on compare en metres !
                    value = value / 3.2808399;
                }
                //MSG(comparestring + " " + value.ToString());

                CustomFilterAltitude fltr = new CustomFilterAltitude(value, comparestring);
                ExecuteCustomFilter(fltr);
            }
        }

        private void excludeSelectedCachesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Geocache> liste = GetSelectedCaches();
            if (liste.Count != 0)
            {
                HashSet<String> hs = new HashSet<String>();
                foreach (Geocache geo in liste)
                {
                    hs.Add(geo._Code);
                }
                CustomFilterExcludeSelection fltr = new CustomFilterExcludeSelection(hs);
                ExecuteCustomFilter(fltr);
            }
        }

        private void displayGPXTrackOnMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log("displayGPXTrackOnMapToolStripMenuItem_Click");
            // Set the file dialog to filter for graphics files.
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "GPX (*.gpx)|*.gpx";
            openFileDialog1.Multiselect = false;
            openFileDialog1.Title = GetTranslator().GetString("DlgChoseGPX");

            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string[] filenames = openFileDialog1.FileNames;
                try
                {
                    List<PointLatLng> pts = new List<PointLatLng>();
                    List<DateTime> times = new List<DateTime>();
                    List<Double> elevations = new List<Double>();
                    bool bGood = false;
                    XmlDocument _xmldoc;
                    XmlNodeList _xmlnodes;
                    _xmldoc = new XmlDocument();
                    _xmldoc.Load(filenames[0]);
                    
                    XmlNamespaceManager ns = new XmlNamespaceManager(_xmldoc.NameTable);
                	ns.AddNamespace("ns1", GROUNDSPEAK_NAMESPACE[1]);
					_xmlnodes = _xmldoc.SelectNodes("/ns1:gpx/ns1:trk", ns);
					
                    if ((_xmlnodes != null) && (_xmlnodes.Count != 0))
                    {
                        //Log("_xmlnodes : " + _xmlnodes.Count.ToString());
                        foreach (XmlNode _xmlnode in _xmlnodes)
                        {
                            if (_xmlnode.HasChildNodes)
                            {
                                foreach (XmlNode child in _xmlnode.ChildNodes)
                                {
                                    //Log(child.Name);
                                    if (child.Name == "trkseg")
                                    {
                                        // Chaque fils est comme suit
                                        // <trkpt lat="48.8414149918" lon="2.2767397296">
                                        //   <ele>39.07</ele>
                                        //   <time>2015-07-12T07:29:53Z</time>
                                        // </trkpt>
                                        foreach (XmlNode trkpt in child.ChildNodes)
                                        {
                                            if (trkpt.Name == "trkpt")
                                            {
                                                String log = "";
                                                // On crée le point de la route
                                                String lat = MyTools.getAttributeValue(trkpt, "lat");
                                                String lon = MyTools.getAttributeValue(trkpt, "lon");
                                                if ((lat != "") && (lon != ""))
                                                {
                                                    PointLatLng pt = new PointLatLng(MyTools.ConvertToDouble(lat), MyTools.ConvertToDouble(lon));
                                                    //log += pt.ToString();
                                                    pts.Add(pt);
                                                }

                                                // Est-ce qu'on a le temps ?
                                                String val = MyTools.getNodeValue(trkpt, "time");
                                                if (val != "")
                                                {
                                                    log += " " + val;
                                                    DateTime valtime = DateTime.Now;
                                                    if (DateTime.TryParse(val, out valtime))
                                                    {
                                                        times.Add(valtime);
                                                        log += " [" + valtime.ToShortDateString() + " " + valtime.ToShortTimeString() + "]";
                                                    }
                                                }

                                                // Et l'altitude ?
                                                val = MyTools.getNodeValue(trkpt, "ele");
                                                if (val != "")
                                                {
                                                    //log += " " + val;
                                                    Double d = MyTools.ConvertToDouble(val);
                                                    elevations.Add(d);
                                                    //log += " [" + d.ToString() + "]";
                                                }
                                                //Log(log);
                                            }
                                        }
                                        bGood = true;
                                    }
                                }
                            }
                        }
                    }

                    if (_trackselector != null)
                    {
                        _trackselector.EmptyAssociatedDecorations();
                        _trackselector.Close();
                        _trackselector.Dispose();
                        _trackselector = null;
                    }

                    if (!bGood)
                    {
                        MsgActionError(this, GetTranslator().GetString("LblErrorNoATrack"));
                    }
                    else
                    {
                        // On affiche la route
                        // On a fini de calculer l'itinéraire
                        // On l'affiche 
                        // On efface l'itinéraire précédent
                        if ((pts != null) && (pts.Count != 0))
                        {
                            // S'il y a un problème sur le temps, on jette tout
                            if (times.Count != pts.Count)
                            {
                            	// C'est peut être un fichier basecamp, en tout cas on n'a que des points...
                            	DisplaySimpleTrack(pts);
                                return;
                            }

                            bool bUseSpeed;
                            TrackSelector.ColorType speedcolor;
                            Color singlecolor;
                            Log("Request params");
                            TrackSelector.RequestParameters(this, out bUseSpeed, out speedcolor, out singlecolor);
                            Log("bUseSpeed: " + bUseSpeed.ToString());
                            Log("speedcolor: " + speedcolor.ToString());
                            Log("singlecolor: " + singlecolor.ToString());
                            Log("Ctor TrackSelector");
                            _trackselector = new TrackSelector(this, pts, times, elevations, bUseSpeed, speedcolor, singlecolor);
                            _trackselector.Icon = this.Icon;
                            Log("Show TrackSelector");
                            _trackselector.Show();
                        }
                        else
                        {
                        	MsgActionError(this, GetTranslator().GetString("LblErrorNoATrack"));
                            return;
                        }
                    }
                }
                catch (Exception exc)
                {
                    ShowException("", GetTranslator().GetString("FMenuDisplayGPXTrack"), exc);
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
        
        private void DisplaySimpleTrack(List<PointLatLng> newpts)
        {
        	// Effacement
            _cacheDetail._gmap.Overlays[GMapWrapper.RESERVED4].Routes.Clear();
            _cacheDetail._gmap.Overlays[GMapWrapper.RESERVED4].Markers.Clear();
        
            if (newpts.Count >= 2)
            {
                // On ajoute un marker début et fin
                _cacheDetail._gmap.Overlays[GMapWrapper.RESERVED4].Markers.Add(CreateMarker(newpts[0], GMarkerGoogleType.green, GetTranslator().GetString("LblStart")));
                _cacheDetail._gmap.Overlays[GMapWrapper.RESERVED4].Markers.Add(CreateMarker(newpts[newpts.Count - 1], GMarkerGoogleType.orange, GetTranslator().GetString("LblEnd")));

                GMapRoute route = null;
                route = new GMapRoute(newpts, "Route");
				route.IsHitTestVisible = true;
                route.Tag = _cacheDetail._gmap; // très important pour le tooltip
                Pen pen = new Pen(Color.Red);
                pen.Width = 2;// route.Stroke.Width;
                route.Stroke = pen;
                // On change le nom de cette route pour le tooltip
                String kmmi = (_bUseKm) ? GetTranslator().GetString("LVKm") : GetTranslator().GetString("LVMi"); 
                double dist = (_bUseKm) ? route.Distance : route.Distance * _dConvKmToMi;
                String tooltiptext = dist.ToString("0.0") + " " + kmmi;
                route.Name = tooltiptext;
                _cacheDetail._gmap.Overlays[GMapWrapper.RESERVED4].Routes.Add(route);
                ShowCacheMapInCacheDetailImpl(newpts[0]);
            }
        }
    
        private void seechangelogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String readme = "";
            try
            {
                readme = GetInternalDataPath() + Path.DirectorySeparatorChar + "readme.html";
                _cacheDetail.LoadPage(GetTranslator().GetString("FMenuSeeHistory"), readme);
                
                String premium = GetInternalDataPath() + Path.DirectorySeparatorChar + "gold.html";
                if (File.Exists(premium) && SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled())
                {
                	_cacheDetail.LoadPage(GetTranslator().GetString("FMenuHistoryPremium"), premium);
                }
            }
            catch (Exception exc)
            {
                String msg = GetTranslator().GetString("FMenuSeeHistory") + ": (" + readme + ") ";
                ShowException("", msg, exc);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool enableGoldMode()
        {
        	bool ok = SpecialFeatures.SpecialFeaturesMgt.EnterSpecialFeaturesKey(this);
        	EnableDisableSpecialFeatures();
        	ToolbarConfiguration.CreateToolbar(this);
        	_profileMgr.UpdateBasedOnMGMCurrentProfile(this);
            return ok;
        }

        private void downloadbetaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BetaDownload beta = new BetaDownload(this);
            beta.Icon = this.Icon;
            beta.ShowDialog();
        }

        private void downloadPluginsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PluginsDownload plugins = new PluginsDownload(this);
            plugins.Icon = this.Icon;
            plugins.ShowDialog();
        }

        private void pluginscodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NamespaceReflector reflector = new NamespaceReflector(GetTranslator().GetString("TitleReflector"), "MyGeocachingManager.xml");
            reflector.Icon = this.Icon;
            reflector.Font = this.Font;
            reflector.Reflection_Namespace = GetImageSized("Reflection_Namespace"); // 0

            reflector.AuthorizedNamespaces = new string[] {
            "MyGeocachingManager",
            "MyGeocachingManager.HMI",
            "MyGeocachingManager.Geocaching",
            "MyGeocachingManager.Geocaching.Filters",
            "SpaceEyeTools",
            "SpaceEyeTools.HMI",
            "SpaceEyeTools.Markdown",
            "SpaceEyeTools.EXControls",
            "Tsp",
            "GMap.NET.WindowsForms",
            "GMap.NET.WindowsForms.Markers"
            }; 
            reflector.Reflection_Class = GetImageSized("Reflection_Class"); // 1
            reflector.Reflection_Method = GetImageSized("Reflection_Method"); // 2
            reflector.Reflection_Constructor = GetImageSized("Reflection_Constructor"); // 3
            reflector.Reflection_Field = GetImageSized("Reflection_Field"); // 4
            reflector.Reflection_Property = GetImageSized("Reflection_Property"); // 5
            reflector.Reflection_Interface = GetImageSized("Reflection_Interface"); // 6
            reflector.Reflection_Enum = GetImageSized("Reflection_Enum"); // 7
            reflector.Reflection_EnumValue = GetImageSized("Reflection_EnumValue"); // 8
            
            reflector.Show();
        }

        private void numberLastLogsSymbolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int numberlastlogssymbols = Int32.Parse(ConfigurationManager.AppSettings["numberlastlogssymbols"]);

            List<ParameterObject> lst = new List<ParameterObject>();
            List<String> lstvals = new List<string>();
            lstvals.Add("1");
            lstvals.Add("2");
            lstvals.Add("3");
            lstvals.Add("4");
            lstvals.Add("5");
            ParameterObject pobj = new ParameterObject(ParameterObject.ParameterType.List, lstvals, "lstvals", GetTranslator().GetString("LblLastLogsNumbers"));
            pobj.DefaultListValue = numberlastlogssymbols.ToString();
            lst.Add(pobj);

            ParametersChanger changer = new ParametersChanger();
            changer.Title = GetTranslator().GetString("FMenuLastLogSymbols");
            changer.BtnCancel = GetTranslator().GetString("BtnCancel");
            changer.BtnOK = GetTranslator().GetString("BtnOk");
            changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
            changer.ErrorTitle = GetTranslator().GetString("Error");
            changer.Parameters = lst;
            changer.Font = this.Font;
            changer.Icon = this.Icon;

            if (changer.ShowDialog() == DialogResult.OK)
            {
                int newnbsymbols = Int32.Parse(changer.Parameters[0].Value);
                if (newnbsymbols != numberlastlogssymbols)
                {
                    UpdateConfFile("numberlastlogssymbols", newnbsymbols.ToString());

                    // On reaffiche les caches
                    BuildListViewCache();
                    PopulateListViewCache(null);
                }
            }
        }

        private void btnINFO_Click(object sender, EventArgs e)
        {
            try
            {
            	List<Geocache> caches = GetSelectedCaches();
            	if (caches.Count == 1)
                {
                	Geocache geo = caches[0];
                	String msg = geo.ToShortString() + "\r\n";
                	MSG(msg);
                }
                else if (lvGeocaches.SelectedItems.Count >= 1)
                {
                   
                }
            }
            catch (Exception ex)
            {
                KillThreadProgressBar();
                ShowException("", "Hiden info button", ex);
            }
        }

        /// <summary>
        /// Get user information
        /// Buggy and not working.... :-(
        /// </summary>
        /// <param name="userid">user geocaching identifier (not the name, not the GUID, just the identifier)</param>
        public void GetUserInformation(string userid)
        {

            try
            {

                UpdateHttpDefaultWebProxy();

                // On checke que les L/MDP soient corrects

                // Et on récupère les cookies au passage

                CookieContainer cookieJar = CheckGCAccount(true, false);

                if (cookieJar == null)

                    return;

                String url = "http://www.geocaching.com/profile/default.aspx?id=" + userid;

                //MSG(url);

                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);

                objRequest.Proxy = GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)

                objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification

                HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();

                String response;

                using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
                {

                    response = responseStream.ReadToEnd();

                    responseStream.Close();

                }

                // On parse la page

                // ****************

                String user = MyTools.GetSnippetFromText("ctl00_ContentBody_ProfilePanel1_lblMemberName\">", "</span>", response);

                String avatarurl = MyTools.GetSnippetFromText("ctl00_ContentBody_ProfilePanel1_uxProfilePhoto\" class=\"user-avatar\" src=\"", "\" style=", response);

                String membership = MyTools.GetSnippetFromText("ctl00_ContentBody_ProfilePanel1_lblStatusText\">", "</span>", response);

                String membersince = MyTools.GetSnippetFromText("ctl00_ContentBody_ProfilePanel1_lblMemberSinceDate\">", "</span>", response);

                String tmp = "";

                tmp = MyTools.GetSnippetFromText("ctl00_ContentBody_ProfilePanel1_Panel_CachesFound", "</div>", response);

                String find = MyTools.CleanString(MyTools.GetSnippetFromText("width=\"16\" />", "</div>", tmp));

                String hide = "-";

                tmp = MyTools.GetSnippetFromText("ctl00_ContentBody_ProfilePanel1_Panel_TrackableStats", "</div>", response);

                String tb = MyTools.CleanString(MyTools.GetSnippetFromText("width=\"16\" />", "</div>", tmp));

                String fav = "-";

                UserInfo ui = new UserInfo(user, avatarurl, membership, membersince, find, hide, tb, fav);

                ui.ShowDialog();

            }

            catch (Exception ex)
            {

            	ShowException("", "Get user information", ex);
            }

        }

        private void updateCachesItemsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (GetNumberOfSelectedCaches() == 0)
                    return;

                List<ParameterObject> lst = new List<ParameterObject>();
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "All", GetTranslator().GetString("FDisplayAll")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "DateCreation", GetTranslator().GetString("HTMLCreationDate")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "Owner", GetTranslator().GetString("LVOwner2")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "Status", GetTranslator().GetString("LVAvailable2")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "Difficulty", GetTranslator().GetString("LVDifficulty")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "Terrain", GetTranslator().GetString("LVTerrain")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "Description", GetTranslator().GetString("LblDescription")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "Container", GetTranslator().GetString("LVContainer")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "Hint", GetTranslator().GetString("LVHint")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "Attributes", GetTranslator().GetString("LVAttributes")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "Logs", GetTranslator().GetString("LVLastlog")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "Pays", GetTranslator().GetString("LVCountry")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "Etat", GetTranslator().GetString("LVState")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "Stats", GetTranslator().GetString("LblStatistiques2")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "Basic", GetTranslator().GetString("LblBasicCacheInfo")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "Notes", GetTranslator().GetString("LblCacheNote")));

                ParametersChanger changer = new ParametersChanger();
                changer.Title = GetTranslator().GetString("LblCompleterCacheSelectItem");
                changer.BtnCancel = GetTranslator().GetString("BtnCancel");
                changer.BtnOK = GetTranslator().GetString("BtnOk");
                changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
                changer.ErrorTitle = GetTranslator().GetString("Error");
                changer.Parameters = lst;
                changer.Font = this.Font;
                changer.Icon = this.Icon;

                if (changer.ShowDialog() == DialogResult.OK)
                {
                    bool anychange = false;
                    bool[] parameters = new bool[lst.Count];
                    for (int i = 0; i < lst.Count; i++)
                    {
                        parameters[i] = (lst[i].Value == "True");
                        anychange = anychange || parameters[i];
                    }
                    if (anychange)
                    {
                        // On fait le boulot
                        CompleteSelectedCaches(parameters);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowException("", GetTranslator().GetString("FMenuCompleteItemsFromCache"), ex);
            }
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (splitContainer1.SplitterDistance > 200)
            {
                if (wbFastCachePreview.Visible == false)
                {
                    EmptywbFastCachePreview();
                    wbFastCachePreview.Visible = true;
                    lblTipEnlargeAreaWebBrowser.Visible = false;
                    
                }
            }
            else
            {
                if (wbFastCachePreview.Visible == true)
                {
                    wbFastCachePreview.Visible = false;
                    EmptywbFastCachePreview();
                    lblTipEnlargeAreaWebBrowser.Visible = true;
                }
            }
        }

        private void diagnosticsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Diagnostics diag = new Diagnostics(this);
            diag.Icon = this.Icon;
            diag.ShowDialog();
        }

        private void identifyClustersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String unit = "";
            if (_bUseKm)
            {
                unit = GetTranslator().GetString("LVKm");
            }
            else
            {
                unit = GetTranslator().GetString("LVMi");
            }

            List<ParameterObject> lst = new List<ParameterObject>();
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Double, 1.0, "kmmi", GetTranslator().GetString("LblClusterDistance") + " " + unit));
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Int, 15, "lon", GetTranslator().GetString("LblMinNumberCluster")));
            //lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "area", GetTranslator().GetString("LblRectangulalAreaCluster")));

            ParametersChanger changer = new ParametersChanger();
            changer.Title = GetTranslator().GetString("FMenuCluster");
            changer.BtnCancel = GetTranslator().GetString("BtnCancel");
            changer.BtnOK = GetTranslator().GetString("BtnOk");
            changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
            changer.ErrorTitle = GetTranslator().GetString("Error");
            changer.Parameters = lst;
            changer.Font = this.Font;
            changer.Icon = this.Icon;

            if (changer.ShowDialog() == DialogResult.OK)
            {
                _ThreadProgressBarTitle = GetTranslator().GetString("FMenuCluster");
                CreateThreadProgressBar();

                //LOAD DATA
                List<SimpleCluster> clusterList = new List<SimpleCluster>();
                List<Geocache> caches = GetDisplayedCaches();
                if (caches.Count != 0)
                {
                    int i = 0;
                    foreach (Geocache geo in caches)
                    {
                        SimpleCluster cluster = new SimpleCluster(
                            i++,
                            geo._Code,
                            geo._dLatitude,
                            geo._dLongitude);
                        clusterList.Add(cluster);
                    }

                    double distance = MyTools.ConvertToDouble(changer.Parameters[0].Value);
                    if (!_bUseKm)
                    {
                        // distance always in Km
                        distance = distance / _dConvKmToMi;
                    }

                    // conversion in degrees
                    // Get latitude of first cache (hope its not too widely distributed)
                    double lat_first_cache = caches[0]._dLatitude;
                    Double latitudeSensitivity = MyTools.KilometerToDegree(distance, lat_first_cache);
                    Double longitutdeSensitivity = MyTools.KilometerToDegree(distance, lat_first_cache);
                   
                    Int16 minnb = Int16.Parse(changer.Parameters[1].Value);
                    bool bRectangular = true;// bool.Parse(changer.Parameters[2].Value);

                    //CLUSTER THE DATA
                    Dictionary<int, SimpleCluster> clusteredData = null;
                    if (bRectangular)
                        clusteredData = SimpleCluster.ClusterTheData(clusterList, latitudeSensitivity, longitutdeSensitivity);
                    else
                        clusteredData = SimpleCluster.ClusterTheData(clusterList, latitudeSensitivity);

                    String s = distance.ToString() + " Km = " + latitudeSensitivity.ToString() + "°\r\n";
                    s += "latitudeSensitivity: " + latitudeSensitivity.ToString() + "\r\n";
                    s += "longitutdeSensitivity: " + longitutdeSensitivity.ToString() + "\r\n";
                    s += "minnb: " + minnb.ToString() + "\r\n";
                    s += "-----------------------------------------\r\n";
                    s += "Initial number: " + clusterList.Count().ToString() + "\r\n";
                    s += "Final number: " + clusteredData.Count().ToString() + "\r\n";
                    List<PointDouble> centers = new List<PointDouble>();
                    List<SimpleCluster> clusters = new List<SimpleCluster>();
                    foreach (KeyValuePair<int, SimpleCluster> pair in clusteredData)
                    {
                        SimpleCluster c = pair.Value;
                        if (c.LAT_LON_LIST.Count() >= minnb)
                        {
                            List<PointDouble> clusterarea = c.GetClusterArea();
                            s += "******************************\r\n";
                            double dlat = clusterarea[0].X - clusterarea[3].X;
                            double dlon = clusterarea[2].Y - clusterarea[0].Y;
                            dlat = MyTools.DegreeToKilometer(dlat, lat_first_cache);
                            dlon = MyTools.DegreeToKilometer(dlon, lat_first_cache);
                            String tooltip = String.Format("{0:0.#}", ((_bUseKm) ? dlat : dlat * _dConvKmToMi)) +
                                ((_bUseKm) ? GetTranslator().GetString("LVKm") : GetTranslator().GetString("LVMi")) + " x " +
                                String.Format("{0:0.#}", ((_bUseKm) ? dlon : dlon * _dConvKmToMi)) +
                                ((_bUseKm) ? GetTranslator().GetString("LVKm") : GetTranslator().GetString("LVMi"));
                            s += tooltip + "\r\n"; 
                            s += dlat.ToString() + " / " + dlon.ToString() + "\r\n";
                            //if ((final_latitudeSensitivity <= 2.0 * latitudeSensitivity) && (final_longitutdeSensitivity <= 2.0 * longitutdeSensitivity))
                            {
                                s += pair.Key.ToString() + " -> " + c.ToString() + "\r\n";
                                centers.Add(c.LAT_LON_CENTER);
                                clusters.Add(c);
                            }
                            //else
                            //{
                            //    s += "   ===> REJECTED\r\n";
                            //}
                        }
                    }
                    KillThreadProgressBar();

                    // Uniquement en débug
                    if (ConfigurationManager.AppSettings["debugbtn"] == "True")
                        MSG(s);

                    // On cree les cercles
                    Color color1 = Color.FromArgb(60, 255, 0, 0);
                    Color color2 = Color.FromArgb(60, 0, 255, 0);
                    Brush brush1 = new SolidBrush(color1);
                    Brush brush2 = new SolidBrush(color2);
                    Pen pen1 = new Pen(color1, 2);
                    Pen pen2 = new Pen(color2, 2);

                    GMapOverlay overlay = _cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2];
                    overlay.IsVisibile = true;
                    ClearOverlay_RESERVED2();

                    if (centers.Count() != 0)
                    {
                        int index = 0;
                        _cacheDetail._gmap.HoldInvalidation = true;
                        GMapMarkerCircle circle;
                        foreach (PointDouble pt in centers)
                        {
                            // La zone
                            if (!bRectangular)
                            {
                                double radius = 0.0;
                                PointDouble center = new PointDouble();
                                clusters[index].GetClusterCircle(ref center, ref radius);

                                circle = new GMapMarkerCircle(
                                        _cacheDetail._gmap,
                                        new PointLatLng(center.X, center.Y),
                                        (int)(radius * 1000.0),
                                        pen2,
                                        brush2,
                                        true);
                                String tooltip = GetTranslator().GetString("LblRadius") +
                                    " " +
                                    String.Format("{0:0.#}", ((_bUseKm) ? radius : radius * _dConvKmToMi)) +
                                    ((_bUseKm) ? GetTranslator().GetString("LVKm") : GetTranslator().GetString("LVMi"));
                                GMapMarkerImage marker = new GMapMarkerImage(_listImagesSized[getIndexImages("CenterView")], new PointLatLng(clusters[index].LAT_LON_CENTER.X, clusters[index].LAT_LON_CENTER.Y));
                                marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                                marker.ToolTipText = tooltip;
                                overlay.Markers.Add(circle);
                                overlay.Markers.Add(marker);
                                
                            }
                            else
                            {
                                List<PointLatLng> area = new List<PointLatLng>();
                                List<PointDouble> clusterarea = clusters[index].GetClusterArea();
                                area.Add(new PointLatLng(clusterarea[0].X, clusterarea[0].Y));
                                area.Add(new PointLatLng(clusterarea[1].X, clusterarea[1].Y));
                                area.Add(new PointLatLng(clusterarea[2].X, clusterarea[2].Y));
                                area.Add(new PointLatLng(clusterarea[3].X, clusterarea[3].Y));
                                double dlat = clusterarea[0].X - clusterarea[3].X;
                                double dlon = clusterarea[2].Y - clusterarea[0].Y;
                                dlat = MyTools.DegreeToKilometer(dlat, lat_first_cache);
                                dlon = MyTools.DegreeToKilometer(dlon, lat_first_cache);
                                String tooltip = String.Format("{0:0.#}", ((_bUseKm) ? dlat : dlat * _dConvKmToMi)) +
                                    ((_bUseKm) ? GetTranslator().GetString("LVKm") : GetTranslator().GetString("LVMi")) + " x " +
                                    String.Format("{0:0.#}", ((_bUseKm) ? dlon : dlon * _dConvKmToMi)) +
                                    ((_bUseKm) ? GetTranslator().GetString("LVKm") : GetTranslator().GetString("LVMi"));

                                GMapPolygon maparea = new GMapPolygon(area, "mazone");
                                maparea.Tag = tooltip;
                                overlay.Polygons.Add(maparea);

                                GMapMarkerImage marker = new GMapMarkerImage(_listImagesSized[getIndexImages("CenterView")],
                                    new PointLatLng((clusterarea[0].X + clusterarea[3].X)/2.0,
                                        (clusterarea[2].Y + clusterarea[0].Y)/2.0));
                                marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                                marker.ToolTipText = tooltip;
                                overlay.Markers.Add(marker);
                            }

                            // Un cercle pour chaque element du cluster (pour le debug)
                            foreach (String code in clusters[index].NAMES)
                            {
                                Geocache cache = _caches[code];
                                circle = new GMapMarkerCircle(
                                    _cacheDetail._gmap,
                                    new PointLatLng(cache._dLatitude, cache._dLongitude),
                                    161,
                                    pen1,
                                    brush1,
                                    true);
                                overlay.Markers.Add(circle);
                            }

                            index++;
                        }
                        _cacheDetail._gmap.Refresh();
                        _cacheDetail._gmap.Position = new PointLatLng(centers[0].X, centers[0].Y);
                        ShowCacheMapInCacheDetail();
                    }
                }
            }
        }


        private void clearmarkersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearOverlay_RESERVED2();
        }
        
		/// <summary>
		/// Clear all markers, routes, polygons from overlay RESERVED2 in _cacheDetail._gmap
		/// </summary>
        public void ClearOverlay_RESERVED2()
        {
            GMapOverlay overlay = _cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2];
            overlay.Markers.Clear();
            overlay.Routes.Clear();
            overlay.Polygons.Clear();
        }

        private void modifyconfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigEditor dlg = new ConfigEditor();
            dlg.Font = this.Font;
            dlg.Icon = this.Icon;
            dlg.Text = GetTranslator().GetString("FMenuChangeConfigFile");
            TranslateTooltips(dlg, null);
            dlg.ShowDialog();
        }

        private void testpluginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Plugin (*.cs, *.vb)|*.cs;*.vb";
            openFileDialog1.Multiselect = false;
            openFileDialog1.Title = GetTranslator().GetString("LblPluginFile");

            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string f = openFileDialog1.FileNames[0];
                CompilerResults cr;
                if (CompilePlugin(f, out cr))
                {
                    GetPlugins(cr.CompiledAssembly, true);
                    CreatePluginMenu();
                    MsgActionDone(this);
                }
                else
                {
                    if (cr != null)
                    {
                        MsgActionError(this, GetCompileErrors(cr, f), true);
                    }
                }
            }
        }

        private void createMyFindsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            CreateMyFindGPX();
        }

        private void generateSQLdb_Click(object sender, EventArgs e)
        {
        	String msg = "";
	        try
            {
	        	SaveFileDialog saveFileDialog1 = new SaveFileDialog();
	            saveFileDialog1.Filter = "SQLite (*.db)|*.db";
	            saveFileDialog1.RestoreDirectory = true;
	            String thefilename = "";
	            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
	            {
	            	thefilename = saveFileDialog1.FileName;
	            }
	            else
	            {
	            	MsgActionCanceled(this);
                    return;
	            }
	            
        		bool striphtml = false;
            	DialogResult dialogResult = MessageBox.Show(GetTranslator().GetStringM("AskDeHTMLSQL"),
                        GetTranslator().GetString("AskDeHTMLSQLTitle"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            	if (dialogResult == DialogResult.Yes)
                {
                    striphtml = true;
                }
            	
        		
        		
	            // Set the file dialog to filter for graphics files.
	            OpenFileDialog openFileDialog1 = new OpenFileDialog();
	            openFileDialog1.Filter = "GPX & ZIP (*.gpx, *.zip)|*.gpx;*.zip";
	            //  Allow the user to select multiple images.
	            openFileDialog1.Multiselect = true;
	            openFileDialog1.Title = GetTranslator().GetString("DlgChoseGPX");
	
	            DialogResult dr = openFileDialog1.ShowDialog();
	            if (dr == System.Windows.Forms.DialogResult.OK)
	            {
	                string[] filenames = openFileDialog1.FileNames;
	                _ThreadProgressBarTitle = GetTranslator().GetString("generateSQLdb");
                    CreateThreadProgressBarEnh();
                    
                    MGMDataBase dbmgm = null;
                    if (File.Exists(thefilename))
                    	dbmgm = new MGMDataBase(this, thefilename);
                    else
                    	dbmgm = new MGMDataBase(this, thefilename, true);
                
	                HtmlAgilityPack.HtmlDocument doc = null;
					if (striphtml)
						doc = new HtmlAgilityPack.HtmlDocument();

                    // Wait for the creation of the bar
                    while (_ThreadProgressBar == null)
                    {
                        Thread.Sleep(10);
                        Application.DoEvents();
                    }
                    _ThreadProgressBar.progressBar1.Maximum = filenames.Count();

                    MGMDataBase.DBinfo dbi = new MGMDataBase.DBinfo ();
                    
	                foreach(var f in filenames)
	                {
	                	Dictionary<String, Geocache> caches = new Dictionary<string, Geocache> ();
	                	List<Waypoint> waypoints = new List<Waypoint>();
	                	if (f.ToLower().EndsWith(".zip"))
	                	{
	                		LoadGCZip(f, ref caches);
	                	}
	                	else if (f.ToLower().EndsWith(".gpx"))
	                	{
	                		
	                		LoadGCFile(f, ref caches, ref waypoints);
	                		// On associe les waypoints
	                		AssociateWaypointsToGeocaches(ref caches, waypoints);
	                	}
	                	
	                	if (caches.Count() != 0)
	                	{
	                		msg += f + " : " + caches.Count().ToString() + " caches\r\n";
	                		dbmgm.InsertGeocaches(caches.Values.ToList(), striphtml, doc, ref dbi);
	                	}
	                	else 
	                		msg += f + " ignoré !\r\n";
	                	
	                	waypoints.Clear();
	                	waypoints = null;
	                	caches.Clear();
	                	caches = null;
	                	
	                	_ThreadProgressBar.Step();
                        if (_ThreadProgressBar._bAbort)
                            break;
	                }
	                
	                msg = "Nombre total de Travel bugs / Geocoins : " + dbmgm.GetCount(MGMDataBase.DBType.TravelBug).ToString() + "\r\n" +  msg;
	                msg = "Nombre total de Logs : " + dbmgm.GetCount(MGMDataBase.DBType.Log).ToString() + "\r\n" +  msg;
	                msg = "Nombre total de Waypoints : " + dbmgm.GetCount(MGMDataBase.DBType.Waypoint).ToString() + "\r\n" +  msg;
	                msg = "Nombre total de Caches : " + dbmgm.GetCount(MGMDataBase.DBType.GeocacheFull).ToString() + "\r\n" +  msg;
	                KillThreadProgressBarEnh();
	                MsgActionDone(this, "\r\n" + msg, false);
	            }
	            else
	            {
	            	MsgActionCanceled(this);
	            	return;
	            }
        	}
        	catch(Exception ex)
        	{
        		KillThreadProgressBarEnh();
        		ShowException("", GetTranslator().GetString("FMenuManualUpdate") + "\r\n" + msg + "\r\n", ex);
        	}
        }

        
        
        /// <summary>
        /// Overwrite ignore list of caches on hard drive with the one in MGM's memory
        /// </summary>
        public void ReplaceIgnoreList()
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(_ignorefile, false, Encoding.Default);

            // on supprime les caches
            foreach (KeyValuePair<String, MiniGeocache> pair in _ignoreList)
            {
                file.WriteLine(pair.Value.ToString());
            }
            file.Close();
        }

        private void manageIgnoreListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IgnoreCacheManager ignoremgr = new IgnoreCacheManager(this);
            ignoremgr.Icon = this.Icon;
            ignoremgr.Font = this.Font;
            ignoremgr.ShowDialog();
        }

        private void btnPinUnpin_Click(object sender, EventArgs e)
        {
            int index;
            if (_bFilterMasked == false)
            {
                // On était pinned
                index = getIndexImages("Unpinned");
                btnPinUnpin.Image = _listImagesSized[index];
                _bFilterMasked = true;
                groupBox1.MinimumSize = new Size(groupBox1.MinimumSize.Width,0);
                splitContainer1.Panel1MinSize = 0;
                btnPinUnpin.Tag = splitContainer1.SplitterDistance;
                splitContainer1.SplitterDistance = 0;
            }
            else
            {
                // on était unpinned
                index = getIndexImages("Pinned");
                btnPinUnpin.Image = _listImagesSized[index];
                _bFilterMasked = false;
                splitContainer1.SplitterDistance = (int)(btnPinUnpin.Tag);
                groupBox1.MinimumSize = new Size(groupBox1.MinimumSize.Width, 168);
                splitContainer1.Panel1MinSize = 178;
            }
        }

       
        private void logselcachesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Geocache> caches = GetSelectedCaches();
            if (caches.Count != 0)
            {
            	FieldNotesHMI.LoadFieldNotesC(this, caches);
            }
        }

        
        private void loadfieldnotesToolStripMenuItem_Click(object sender, EventArgs e)
        {
        	FieldNotesHMI.LoadFieldNotes(this, null);
        }


        private void fMenuEnableTooltipsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fMenuEnableTooltipsToolStripMenuItem.Checked)
                UpdateConfFile("enabletooltips", "False");
            else
                UpdateConfFile("enabletooltips", "True");
            UpdateMenuChecks();
            MsgActionWarning(this, GetTranslator().GetString("MsgRestartApplcation"));
        }
        
		void FMenuDisableListTooltipToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (fMenuDisableListTooltipToolStripMenuItem.Checked)
			{
                UpdateConfFile("disabletooltipsformainlist", "False");
                TranslateTooltips(this, _toolTipForMGM);
			}
            else
            {
                UpdateConfFile("disabletooltipsformainlist", "True");
                // On vire le tooltip de lvGeocache
                if (_toolTipForMGM != null)
                {
                	_toolTipForMGM.SetToolTip(lvGeocaches, null);
                }
            }
            UpdateMenuChecks();
		}
		void ManualUpdateToolStripMenuItemClick(object sender, EventArgs e)
		{
			try
			{
				// Set the file dialog to filter for graphics files.
	            OpenFileDialog openFileDialog1 = new OpenFileDialog();
	            openFileDialog1.Filter = "MGM installation ZIP (*.zip)|*.zip";
	            //  Allow the user to select multiple images.
	            openFileDialog1.Multiselect = false;
	            openFileDialog1.Title = GetTranslator().GetString("FMenuManualUpdate");
	
	            DialogResult dr = openFileDialog1.ShowDialog();
	            if (dr == System.Windows.Forms.DialogResult.OK)
	            {
	            	string[] filenames = openFileDialog1.FileNames;
	            	string file = filenames[0];
	            	string datapath = GetInternalDataPath();
	            	if (Path.GetDirectoryName(file) != datapath)
	            	{
	            		// Il faut le copier en local pour faire proprement
	            		String localfile = datapath + Path.DirectorySeparatorChar + Path.GetFileName(file);
	            		
	            		// On supprime l'ancien
	            		if (File.Exists(localfile))
	            			File.Delete(localfile);
	            		
	            		// On copie le nouveau
	            		File.Copy(file, localfile);
	            		
	            		// On change le fichier
	            		file = localfile;
	            	}
	            	
	            	// Maintenant on lance la mise à jour
	            	sDownloadedUpdateFile = file;
	            	UpdateMGMFromZipFile();
	            	sDownloadedUpdateFile = "";
	            }
			}
			catch(Exception ex)
			{
				sDownloadedUpdateFile = "";
				ShowException("", GetTranslator().GetString("FMenuManualUpdate"), ex);
			}
		}
		void ModifyCoordToolStripMenuItemClick(object sender, EventArgs e)
		{
			List<Geocache> caches = null;
            try
            {
                caches = GetSelectedCaches();
                if (caches.Count != 1)
                {
                	MsgActionWarning(this, GetTranslator().GetString("LblErrOnlyOneCache"));
                	return;
                }
            
                Geocache geo = caches[0];
                CloseCacheDetail();
                
                // On demande les coordonnées
                List<ParameterObject> lst = new List<ParameterObject>();
	            lst.Add(new ParameterObject(ParameterObject.ParameterType.Coordinates/*good*/,
	                geo._Latitude + " " +geo._Longitude,
	                "latlon",
	                GetTranslator().GetString("ParamHomeLatLon"),
	                GetTranslator().GetStringM("TooltipParamLatLon")));
	            ParametersChanger changer = new ParametersChanger();
	            changer.HandlerDisplayCoord = HandlerToDisplayCoordinates;
	            changer.DisplayCoordImage = _listImagesSized[getIndexImages("Earth")];
	            changer.Title = GetTranslator().GetString("FMenuModifyManualCoord").Replace("&", "");
	            changer.BtnCancel = GetTranslator().GetString("BtnCancel");
	            changer.BtnOK = GetTranslator().GetString("BtnOk");
	            changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
	            changer.ErrorTitle = GetTranslator().GetString("Error");
	            changer.Parameters = lst;
	            changer.Font = this.Font;
	            changer.Icon = this.Icon;
	 
	            // Force creation du get handler on control
                changer.CreateControls();
                _cacheDetail._gmap.ControlTextLatLon = changer.CtrlCallbackCoordinates;

                if (changer.ShowDialog() == DialogResult.OK)
                {
                    _cacheDetail._gmap.ControlTextLatLon = null;
	                Double dlon = Double.MaxValue;
	                Double dlat = Double.MaxValue;
	                if (ParameterObject.SplitLongitudeLatitude(lst[0].Value, ref dlon, ref dlat))
	                {
	                    if ((geo._dLatitude != dlat) || (geo._dLongitude != dlon))
		                {
	                    	geo._Latitude = dlat.ToString().Replace(",",".");
		                    geo._Longitude = dlon.ToString().Replace(",",".");
		                    geo._dLatitude = dlat;
		                    geo._dLongitude = dlon;
		                    geo.UpdateDistanceToHome(_dHomeLat, _dHomeLon);
		                    _iNbModifiedCaches += geo.InsertModification("COORD");
		                }
		                
		                // Better way to do that : only recreate for modified caches
		                List<Geocache> c = new List<Geocache>();
		                c.Add(geo);
		                RecreateVisualElements(c);
		
		                // On redessine la carte
		                BuildCacheMapNew(GetDisplayedCaches());
	                }
	                else
	                {
	                    // On ne devrait jamais rentrer ici
	                    MsgActionError(this, GetTranslator().GetString("Error"));
	                }
	            }

            }
            catch (Exception ex)
            {
                PostTreatmentLoadCache();

                // Better way to do that : only recreate for modified caches
                RecreateVisualElements(caches);

                // On redessine la carte
                BuildCacheMapNew(GetDisplayedCaches());

                ShowException("", GetTranslator().GetString("FMenuModifyManualCoord"), ex);
            }
		}
		
		/// <summary>
		/// Declare found a geocache
		/// update _cachestatus if needed
		/// DO NOT save _cachestatus if needed
		/// DO NOT RECREATE VISUAL ELEMENTS !
		/// </summary>
		/// <param name="owner">owner</param>
		/// <param name="geo">geocache to declare found</param>
		public void DeclareFoundCache(String owner, Geocache geo)
		{
			if (geo.IsFound() == false)
        	{
        		if (_cacheStatus.DeclareFoundCache(owner, geo._Code))
        		{
        			geo._bFoundInMGM = true;
        			_iNbModifiedCaches += geo.InsertModification("FOUND");
        		}
        	}
		}

        /// <summary>
        /// Declare found a geocache and force it to appear found
        /// update _cachestatus if needed
        /// DO NOT save _cachestatus if needed
        /// DO NOT RECREATE VISUAL ELEMENTS !
        /// </summary>
        /// <param name="owner">owner</param>
        /// <param name="geo">geocache to declare found</param>
        public void ForceDeclareFoundCache(String owner, Geocache geo)
        {
            if (geo.IsFound() == false)
            {
                _cacheStatus.DeclareFoundCache(owner, geo._Code);
                geo._bFoundInMGM = true;
                _iNbModifiedCaches += geo.InsertModification("FOUND");
            }
        }

		private void SetManualFoundSelection(object sender, EventArgs e)
        {
        	List<Geocache> caches = GetSelectedCaches();
            if (caches.Count == 0)
                return;
            String owner = ConfigurationManager.AppSettings["owner"];
            foreach(Geocache geo in caches)
            {
            	DeclareFoundCache(owner, geo);
            }
            _cacheStatus.SaveCacheStatus();
            
            // Better way to do that : only recreate for modified caches
            RecreateVisualElements(caches);
        }
		
		private void ChangeProfileHandler(object sender, EventArgs e)
        {
            ToolStripMenuItem mnu = sender as ToolStripMenuItem;
            if (mnu != null)
            {
            	if (_profileMgr.UpdateMGMFromProfile(this, mnu.Text))
            	{
                    BuildProfileSubMenu();
                    // On met à jour les fonctions spéciales
                    EnableDisableSpecialFeatures();
                    ToolbarConfiguration.CreateToolbar(this);
            	}
            }
		}
		
		/// <summary>
		/// (re)build profile submenu
		/// </summary>
		public void BuildProfileSubMenu()
		{
			// configGCaccountinfoToolStripMenuItem
			// FMenuProfiles
			List<String> profs = _profileMgr.GetListOfProfiles();
            String owner = ConfigurationManager.AppSettings["owner"];

            // On va nettoyer le menu des trucs existant à partir de l'index n°2
            while (profilesToolStripMenuItem.DropDownItems.Count > 2)
				profilesToolStripMenuItem.DropDownItems.RemoveAt(2);
			
			if (profs.Count != 0)
			{
				// On créé le menu
				foreach(String s in profs)
				{
                    ToolStripMenuItem ts = new ToolStripMenuItem(s, null, new EventHandler(ChangeProfileHandler));
                    ts.Name = "profilesToolStripMenuItem" + s;
                    profilesToolStripMenuItem.DropDownItems.Add(ts);
                    if (s == owner)
                        ts.Checked = true;
				}
			}
		}
		
		private String CreateCookie()
        {
        	String root = "|" + ConfigurationManager.AppSettings["owner"] + "|" + GetDecryptedOwnerPassword();
        	const int imax = 128;
        	int nbrandom = imax - root.Count();
        	if (nbrandom > 0)
        	{
        		root = MyTools.RandomString(nbrandom) + root;
        	}
        	else
        	{
        		// La combinaison mdp / password est plus longue que imax
				// pour éviter toute analyse, on va balancer du pur random
				root = MyTools.RandomString(imax);
        	}
        	root = StringCipher.CustomEncryptNoPadding(root);
        	root = HttpUtility.UrlEncode(root);
        	return root;
        }
		
		void ModifyNameReplaceToolStripMenuItemClick(object sender, EventArgs e)
		{
			ModifyCacheNameReplace();
		}
		
		/// <summary>
        /// Modify a cache name of selected caches based on user input
        /// </summary>
        public void ModifyCacheNameReplace()
        {
            List<Geocache> caches = null;
            String owner = ConfigurationManager.AppSettings["owner"].ToLower();
            try
            {
            	caches = GetSelectedCaches();
            	if (caches.Count == 0)
            	{
            		MsgActionWarning(this, "LblErrorNoSelection");
            		return;
            	}
            	
                List<ParameterObject> lst = new List<ParameterObject>();
                lst.Add(new ParameterObject(ParameterObject.ParameterType.String, "", "search", GetTranslator().GetString("LblTxtToReplace")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.String, "", "replace", GetTranslator().GetString("LbltxtReplacement")));
				lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "case", GetTranslator().GetString("LblCaseSensitive")));

                ParametersChanger changer = new ParametersChanger();
                changer.Title = GetTranslator().GetString("FMenuToolsModCachesNameReplace");
                changer.BtnCancel = GetTranslator().GetString("BtnCancel");
                changer.BtnOK = GetTranslator().GetString("BtnOk");
                changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
                changer.ErrorTitle = GetTranslator().GetString("Error");
                changer.Parameters = lst;
                changer.Font = this.Font;
                changer.Icon = this.Icon;

                if (changer.ShowDialog() == DialogResult.OK)
                {
                    _ThreadProgressBarTitle = GetTranslator().GetString("FMenuToolsModCachesNameReplace");
                    CreateThreadProgressBarEnh();

                    
                    String search = lst[0].Value;
                    String replace = lst[1].Value;
                    bool bCaseSens = (lst[2].Value == "True");
                    
                    // Wait for the creation of the bar
                    while (_ThreadProgressBar == null)
                    {
                        Thread.Sleep(10);
                        Application.DoEvents();
                    }
                    _ThreadProgressBar.progressBar1.Maximum = caches.Count();

                    foreach (Geocache geo in caches)
                    {
                    	String old =geo._Name;
                    	
                    	if (bCaseSens)
                    		geo._Name = geo._Name.Replace(search, replace);
                    	else
                    	{
                    		geo._Name = Regex.Replace(geo._Name, search, replace, RegexOptions.IgnoreCase);
                    	}
                    	if (old != geo._Name)
                    	{
                    		// Uniquement si modifié
	                        geo.UpdatePrivateData(owner);
	                        _iNbModifiedCaches += geo.InsertModification("NAME");
                    	}
                        

                        _ThreadProgressBar.Step();
                        if (_ThreadProgressBar._bAbort)
                            break;
                    }

                    PostTreatmentLoadCache();

                    // Better way to do that : only recreate for modified caches
                    RecreateVisualElements(caches);

                    KillThreadProgressBarEnh();

                    MsgActionDone(this);
                }
            }
            catch (Exception ex)
            {
                PostTreatmentLoadCache();

                // Better way to do that : only recreate for modified caches
                RecreateVisualElements(caches);

                KillThreadProgressBarEnh();

                ShowException("", GetTranslator().GetString("FMenuToolsModCachesNameReplace"), ex);
            }
        }
		void CreateModifyProfileToolStripMenuItemClick(object sender, EventArgs e)
		{
			ChangeUserName();
			if (ConfigurationManager.AppSettings["owner"] != "")
				ChangeHomeLocation();
		}

        private void getFoundinfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateHttpDefaultWebProxy();
                String owner = ConfigurationManager.AppSettings["owner"];

                // On checke que les L/MDP soient corrects
                // Et on récupère les cookies au passage
                CookieContainer cookieJar = CheckGCAccount(true, false);
                if (cookieJar == null)
                    return;

                _ThreadProgressBarTitle = GetTranslator().GetString("LblFetchingProgressInfoMsg");
                CreateThreadProgressBar();
                string result = GetCacheHTMLFromClientImpl("https://www.geocaching.com/my/logs.aspx?s=1&lt=2", cookieJar);

                KillThreadProgressBar();

                // On extrait la table
                result = MyTools.GetSnippetFromText("<table", "</table>", result);

                // Chaque <tr est un found it
                List<String> founds = MyTools.GetSnippetsFromText("<tr", "</tr>", result);

                // on parcourt chaque foundit
                int nb = 0;
                List<KeyValuePair<DateTime, Geocache>> found_caches = new List<KeyValuePair<DateTime, Geocache>>();
                DateTime deb = DateTime.Now;
                DateTime fin = new DateTime(1990, 1, 1);
                foreach (String bloc in founds)
                {
                    // on splitte en <td
                    List<String> cols = MyTools.GetSnippetsFromText("<td>", "</td>", bloc);

                    // 0 : image et found it
                    // 1 : vide
                    // 2 : date
                    // 3 : liens
                    // 4 : région / pays
                    // 5 : lien log
                    String date = cols[2];
                    date = MyTools.CleanString(date);
                    DateTime ddate;
                    if (DateTime.TryParse(date, out ddate) == false)
                        ddate = new DateTime(1999, 1, 1);

                    if (ddate < deb)
                        deb = ddate;
                    if (ddate > fin)
                        fin = ddate;

                    String link = MyTools.GetSnippetFromText("<a href=\"", "\" ", cols[3]);
                    Geocache geo = new Geocache(this);
                    geo._Url = link;
                    geo._Code = "";
                    geo._CacheId = (_iCacheId++).ToString();
                    geo._LongDescription = "";
                    geo._ShortDescription = "";
                    found_caches.Add(new KeyValuePair<DateTime, Geocache>(ddate, geo));

                    nb++;
                }

                // On demande la date de début et de fin du run
                bool bContinue = false;
                bool bAbort = false;
                List<Geocache> caches_to_export = null;
                int nbvalid = 0;
                do
                {
                    List<ParameterObject> lst = new List<ParameterObject>();
                    lst.Add(new ParameterObject(ParameterObject.ParameterType.Label, "", "totalnb", GetTranslator().GetString("LblTotalNbCaches") + " = " + nb.ToString())); 
                    lst.Add(new ParameterObject(ParameterObject.ParameterType.Date, deb, "deb", GetTranslator().GetString("LblStart")));
                    lst.Add(new ParameterObject(ParameterObject.ParameterType.Date, fin, "fin", GetTranslator().GetString("LblEnd")));

                    ParametersChanger changer = new ParametersChanger();
                    changer.Title = GetTranslator().GetString("LblPeriod");
                    changer.BtnCancel = GetTranslator().GetString("BtnCancel");
                    changer.BtnOK = GetTranslator().GetString("BtnOk");
                    changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
                    changer.ErrorTitle = GetTranslator().GetString("Error");
                    changer.Parameters = lst;
                    changer.Font = Font;
                    changer.Icon = Icon;

                    if (changer.ShowDialog() == DialogResult.OK)
                    {
                        deb = (DateTime)(lst[1].ValueO);
                        deb = new DateTime(deb.Year, deb.Month, deb.Day, 0, 0, 0);
                        fin = (DateTime)(lst[2].ValueO);
                        fin = new DateTime(fin.Year, fin.Month, fin.Day, 23, 59, 59);

                        if (deb > fin)
                        {
                            MsgActionError(this, GetTranslator().GetString("LblErrStartEnd"));
                            return;
                        }

                        // On calcule combien de caches sont concernées
                        nbvalid = 0;
                        caches_to_export = new List<Geocache>();
                        foreach (KeyValuePair<DateTime, Geocache> pair in found_caches)
                        {
                            if ((deb <= pair.Key) && (pair.Key <= fin))
                            {
                                nbvalid++;
                                caches_to_export.Add(pair.Value);
                            }
                        }
                        // AskRetrieveInfoForCaches
                        DialogResult dialogResult = MessageBox.Show(
                            String.Format(GetTranslator().GetString("AskRetrieveInfoForCaches"), nbvalid, nb),
                            GetTranslator().GetString("FMenuRetrieveFoundInformations"),
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dialogResult == DialogResult.Yes)
                        {
                            bContinue = true;
                        }
                    }
                    else
                        bAbort = true;
                }
                while (!bContinue && !bAbort);

                if (bAbort)
				{
					MsgActionCanceled(this);
				}
                
                // On complete les caches uniquement sur la période choisie
                if (bContinue && !bAbort)
                {
                    // 00 : All
                    // 01 : DateCreation
                    // 02 : Owner
                    // 03 : Status
                    // 04 : Difficulty
                    // 05 : Terrain
                    // 06 : Description
                    // 07 : Container
                    // 08 : Hint
                    // 09 : Attributes
                    // 10 : Logs
                    // 11 : Contry
                    // 12 : State
                    // 13 : Statistics
                    // 14 : Basic info (name, url, coordinates)
                    // 15 : Personal notes
                    bool[] parameters = new bool[16];
                    parameters[ 0] = false;
                    parameters[ 1] = true;
                    parameters[ 2] = true;
                    parameters[ 3] = true;
                    parameters[ 4] = true;
                    parameters[ 5] = true;
                    parameters[ 6] = true;
                    parameters[ 7] = true;
                    parameters[ 8] = true;
                    parameters[ 9] = true;
                    parameters[10] = false;
                    parameters[11] = true;
                    parameters[12] = true;
                    parameters[13] = false;
                    parameters[14] = true;
                    parameters[15] = false;
                    if (CompleteSelectedCaches(ref caches_to_export, parameters, true) == false)
                    {
                        MsgActionCanceled(this);
                        return;
                    }

                    // On va créer les faux logs
                    foreach (KeyValuePair<DateTime, Geocache> pair in found_caches)
                    {
                        Geocache geo = pair.Value;
                        if (String.IsNullOrEmpty(geo._Code) == false)
                        {
                            // On a téléchargé les infos
                            // On créé un faux log
                            CacheLog log = new CacheLog(this);
                            log._Date = pair.Key.ToString(GeocachingConstants._FalseDatePattern);
                            log._Encoded = "False";
                            log._FinderId = "666";
                            log._LogId = "666";
                            log._SortingKey = "";
                            log._Text = "MGM generated log";
                            log._Type = "Found it";
                            log._User = owner;
                            geo._Logs.Add(log);
                        }
                    }

                    // On exporte tout ce beau monde
                    bContinue = false;
                    bAbort = false;
                    FolderBrowserDialog fbd = null;
                    do
                    {
                        fbd = new FolderBrowserDialog();
                        fbd.Description = GetTranslator().GetString("LblSaveInformation");
                        DialogResult dialogResult = fbd.ShowDialog();
                        if (dialogResult == DialogResult.OK)
                        {
                            bContinue = true;
                        }
                        else
                        {
                            DialogResult dialogResult3 = MessageBox.Show(
                            String.Format(GetTranslator().GetString("AskCancelSave"), nbvalid, nb),
                            GetTranslator().GetString("LblSaveInformation"),
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (dialogResult3 == DialogResult.Yes)
                            {
                                bAbort = true;
                            }
                        }
                    }
                    while (!bContinue && !bAbort);

                    if (bContinue && !bAbort)
                    {
                        // On sauve le GPX
                        String file = fbd.SelectedPath + Path.DirectorySeparatorChar + "MyFinds.gpx";
                        ExportGPXFromList(file, caches_to_export, true);

                        // On sauve le geocache_visit.txt
                        String file2 = fbd.SelectedPath + Path.DirectorySeparatorChar + "geocache_visits.txt";
                        System.IO.StreamWriter f = new System.IO.StreamWriter(file2, false, System.Text.Encoding.GetEncoding("iso-8859-8"));
                        foreach (Geocache geo in caches_to_export)
                        {
                            f.WriteLine(geo._Code + "," + geo._Logs[0]._Date + "," + "Found it," + "\"\"");
                        }
                        f.Close();

                        MsgActionOk(this, GetTranslator().GetString("LblCreateFiles") + fbd.SelectedPath + "\r\n    " + file + "\r\n    " + file2, false);
                    }

                    // Et mise à jour de MGM found ?
                    // Déjà fait automatiquement dans la complétion des caches...
                    /*
                    DialogResult dialogResult2 = MessageBox.Show(
                            String.Format(GetTranslator().GetString("AskUpdateFoundMGM"), nbvalid, nb),
                            GetTranslator().GetString("AskUpdateFoundMGM"),
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dialogResult2 == DialogResult.Yes)
                    {
                        foreach (Geocache geo in caches_to_export)
                        {
                            _cacheStatus.DeclareFoundCache(owner, geo._Code);
                        }

                        // On met à jour le status des caches
                        _cacheStatus.SaveCacheStatus();
                    }
                    */

                    // Et on recréé les élèments visuels si nécessaire
                    List<Geocache> caches_to_recreate = new List<Geocache>();
                    foreach (Geocache geo in caches_to_export)
                    {
                        // Si la cache est présente dans MGM et dans la liste, on l'ajoute à la liste à recréer
                        if (_caches.ContainsKey(geo._Code))
                        {
                            Geocache geomgm = _caches[geo._Code];
                            ForceDeclareFoundCache(owner, geomgm);
                            caches_to_recreate.Add(geomgm);
                        }
                    }
                    if (caches_to_recreate.Count != 0)
                        RecreateVisualElements(caches_to_recreate, true);

                    MsgActionDone(this);
                    
                }

            }
            catch (Exception ex)
            {
                KillThreadProgressBar();
                ShowException("", GetTranslator().GetString("FMenuRetrieveFoundInformations"), ex);
            }
        }
        
		void OpenCacheDetailInDefaultBrowerToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (openCacheDetailInDefaultBrowerToolStripMenuItem.Checked)
                UpdateConfFile("openCacheEmbedded", "True");
            else
                UpdateConfFile("openCacheEmbedded", "False");

            UpdateMenuChecks();
		}
		
		void DisplayOurDNFToolStripMenuItemClick(object sender, EventArgs e)
		{
			try
            {
				// On peut faire super rapide si on a déjà un fichier de nos DNFs
				List<Geocache> caches_to_export = null;
				bool imported = false;
				if (MessageBox.Show(
                            GetTranslator().GetString("AskLoadGPXofDNF"),
                            GetTranslator().GetString("FMenuDisplayOnlyOurDNF"),
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					OpenFileDialog openFileDialog1 = new OpenFileDialog();
		            openFileDialog1.Filter = "GPX (*.gpx)|*.gpx";
		            openFileDialog1.Multiselect = false;
		            openFileDialog1.Title = GetTranslator().GetString("AskLoadGPXofDNF");
		
		            DialogResult dr = openFileDialog1.ShowDialog();
		            if (dr == System.Windows.Forms.DialogResult.OK)
		            {
		                string[] filenames = openFileDialog1.FileNames;
		                Dictionary<String, Geocache> dico =  new Dictionary<string, Geocache> ();
		                List<Waypoint> waypoints = new List<Waypoint>();
		               	LoadGCFile(filenames[0], ref dico, ref waypoints); // On se mode des waypoints ici
		                caches_to_export = new List<Geocache>();
		                foreach(KeyValuePair<String, Geocache> pair in dico)
		                {
		                	caches_to_export.Add(pair.Value);
		                }
		                imported = true;
		            }
				}
				
				bool bAbort = false;
				if (!imported)
				{
	                UpdateHttpDefaultWebProxy();
	                String owner = ConfigurationManager.AppSettings["owner"];
	
	                // On checke que les L/MDP soient corrects
	                // Et on récupère les cookies au passage
	                CookieContainer cookieJar = CheckGCAccount(true, false);
	                if (cookieJar == null)
	                    return;
	
	                _ThreadProgressBarTitle = GetTranslator().GetString("LblFetchingProgressInfoMsg");
	                CreateThreadProgressBar();
	                string result = GetCacheHTMLFromClientImpl("https://www.geocaching.com/my/logs.aspx?s=1&lt=3", cookieJar);
	
	                KillThreadProgressBar();
	
	                // On extrait la table
	                result = MyTools.GetSnippetFromText("<table", "</table>", result);
	
	                // Chaque <tr est un found it
	                List<String> founds = MyTools.GetSnippetsFromText("<tr", "</tr>", result);
	
	                // on parcourt chaque foundit
	                int nb = 0;
	                List<KeyValuePair<DateTime, Geocache>> found_caches = new List<KeyValuePair<DateTime, Geocache>>();
	                DateTime deb = DateTime.Now;
	                DateTime fin = new DateTime(1990, 1, 1);
	                foreach (String bloc in founds)
	                {
	                    // on splitte en <td
	                    List<String> cols = MyTools.GetSnippetsFromText("<td>", "</td>", bloc);
	
	                    // 0 : image et found it
	                    // 1 : vide
	                    // 2 : date
	                    // 3 : liens
	                    // 4 : région / pays
	                    // 5 : lien log
	                    String date = cols[2];
	                    date = MyTools.CleanString(date);
	                    DateTime ddate;
	                    if (DateTime.TryParse(date, out ddate) == false)
	                        ddate = new DateTime(1999, 1, 1);
	
	                    if (ddate < deb)
	                        deb = ddate;
	                    if (ddate > fin)
	                        fin = ddate;
	
	                    String link = MyTools.GetSnippetFromText("<a href=\"", "\" ", cols[3]);
	                    Geocache geo = new Geocache(this);
	                    geo._Url = link;
	                    geo._Code = "";
	                    geo._CacheId = (_iCacheId++).ToString();
	                    geo._LongDescription = "";
	                    geo._ShortDescription = "";
	                    found_caches.Add(new KeyValuePair<DateTime, Geocache>(ddate, geo));
	
	                    nb++;
	                }
	
	                // On demande la date de début et de fin du run
	                bool bContinue = false;
	                int nbvalid = 0;
	                do
	                {
	                    List<ParameterObject> lst = new List<ParameterObject>();
	                    lst.Add(new ParameterObject(ParameterObject.ParameterType.Label, "", "totalnb", GetTranslator().GetString("LblTotalNbCaches") + " = " + nb.ToString())); 
	                    lst.Add(new ParameterObject(ParameterObject.ParameterType.Date, deb, "deb", GetTranslator().GetString("LblStart")));
	                    lst.Add(new ParameterObject(ParameterObject.ParameterType.Date, fin, "fin", GetTranslator().GetString("LblEnd")));
	
	                    ParametersChanger changer = new ParametersChanger();
	                    changer.Title = GetTranslator().GetString("LblPeriod");
	                    changer.BtnCancel = GetTranslator().GetString("BtnCancel");
	                    changer.BtnOK = GetTranslator().GetString("BtnOk");
	                    changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
	                    changer.ErrorTitle = GetTranslator().GetString("Error");
	                    changer.Parameters = lst;
	                    changer.Font = Font;
	                    changer.Icon = Icon;
	
	                    if (changer.ShowDialog() == DialogResult.OK)
	                    {
	                        deb = (DateTime)(lst[1].ValueO);
	                        deb = new DateTime(deb.Year, deb.Month, deb.Day, 0, 0, 0);
	                        fin = (DateTime)(lst[2].ValueO);
	                        fin = new DateTime(fin.Year, fin.Month, fin.Day, 23, 59, 59);
	
	                        if (deb > fin)
	                        {
	                            MsgActionError(this, GetTranslator().GetString("LblErrStartEnd"));
	                            return;
	                        }
	
	                        // On calcule combien de caches sont concernées
	                        nbvalid = 0;
	                        caches_to_export = new List<Geocache>();
	                        foreach (KeyValuePair<DateTime, Geocache> pair in found_caches)
	                        {
	                            if ((deb <= pair.Key) && (pair.Key <= fin))
	                            {
	                                nbvalid++;
	                                caches_to_export.Add(pair.Value);
	                            }
	                        }
	                        // AskRetrieveInfoForCaches
	                        DialogResult dialogResult = MessageBox.Show(
	                            String.Format(GetTranslator().GetString("AskRetrieveInfoForCaches"), nbvalid, nb),
	                            GetTranslator().GetString("FMenuRetrieveFoundInformations"),
	                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
	                        if (dialogResult == DialogResult.Yes)
	                        {
	                            bContinue = true;
	                        }
	                    }
	                    else
	                        bAbort = true;
	                }
	                while (!bContinue && !bAbort);
	
	                // On complete les caches uniquement sur la période choisie
	                if (bContinue && !bAbort)
	                {
	                    // 00 : All
	                    // 01 : DateCreation
	                    // 02 : Owner
	                    // 03 : Status
	                    // 04 : Difficulty
	                    // 05 : Terrain
	                    // 06 : Description
	                    // 07 : Container
	                    // 08 : Hint
	                    // 09 : Attributes
	                    // 10 : Logs
	                    // 11 : Contry
	                    // 12 : State
	                    // 13 : Statistics
	                    // 14 : Basic info (name, url, coordinates)
	                    // 15 : Personal notes
	                    bool[] parameters = new bool[16];
	                    parameters[ 0] = false;
	                    parameters[ 1] = true;
	                    parameters[ 2] = true;
	                    parameters[ 3] = true;
	                    parameters[ 4] = true;
	                    parameters[ 5] = true;
	                    parameters[ 6] = true;
	                    parameters[ 7] = true;
	                    parameters[ 8] = true;
	                    parameters[ 9] = true;
	                    parameters[10] = false;
	                    parameters[11] = true;
	                    parameters[12] = true;
	                    parameters[13] = false;
	                    parameters[14] = true;
	                    parameters[15] = false;
	                    if (CompleteSelectedCaches(ref caches_to_export, parameters, true) == false)
	                    {
	                        MsgActionCanceled(this);
	                        return;
	                    }
	
	                    // On va créer les faux logs
	                    // Uniquement pour les caches que nous n'avons JAMAIS trouvé
	                    List<Geocache> a_virer = new List<Geocache>();
	                    foreach (KeyValuePair<DateTime, Geocache> pair in found_caches)
	                    {
	                        Geocache geo = pair.Value;
	                        if (String.IsNullOrEmpty(geo._Code) == false)
	                        {
	                        	if (geo.IsFound())
	                        		a_virer.Add(geo);
	                        	else
	                        	{
		                            // On a téléchargé les infos
		                            // On créé un faux log
		                            CacheLog log = new CacheLog(this);
		                            log._Date = pair.Key.ToString(GeocachingConstants._FalseDatePattern);
		                            log._Encoded = "False";
		                            log._FinderId = "666";
		                            log._LogId = "666";
		                            log._SortingKey = "";
		                            log._Text = "MGM generated log";
		                            log._Type = "Didn't find it";
		                            log._User = owner;
		                            geo._Logs.Add(log);
	                        	}
	                        }
	                    }
	
	                    // On nettoie
	                    foreach(Geocache geo in a_virer)
	                    {
	                    	caches_to_export.Remove(geo);
	                    }
	                    
	                    // On exporte tout ce beau monde
	                    bContinue = false;
	                    bAbort = false;
	                    FolderBrowserDialog fbd = null;
	                    do
	                    {
	                        fbd = new FolderBrowserDialog();
	                        fbd.Description = GetTranslator().GetString("LblSaveInformation");
	                        DialogResult dialogResult = fbd.ShowDialog();
	                        if (dialogResult == DialogResult.OK)
	                        {
	                            bContinue = true;
	                        }
	                        else
	                        {
	                            DialogResult dialogResult3 = MessageBox.Show(
	                            String.Format(GetTranslator().GetString("AskCancelSave"), nbvalid, nb),
	                            GetTranslator().GetString("LblSaveInformation"),
	                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
	                            if (dialogResult3 == DialogResult.Yes)
	                            {
	                                bAbort = true;
	                            }
	                        }
	                    }
	                    while (!bContinue && !bAbort);
	
	                    if (bContinue && !bAbort)
	                    {
	                        // On sauve le GPX
	                        String file = fbd.SelectedPath + Path.DirectorySeparatorChar + "MyDNFs.gpx";
	                        ExportGPXFromList(file, caches_to_export, true);
	
	                        MsgActionOk(this, GetTranslator().GetString("LblCreateFiles") + fbd.SelectedPath + "\r\n    " + file, false);
	                    }
	                }
				}
				
				if (bAbort)
				{
					MsgActionCanceled(this);
				}
				else
				{
					if (caches_to_export == null)
					{
						MsgActionError(this, GetTranslator().GetString("Error"));
					}
					else
					{
						// Le filtre
		                List<String> les_dnf = new List<string>();
		                foreach(Geocache geo in caches_to_export)
		                {
		                	les_dnf.Add(geo._Code);
		                }
		                
		                CustomFilterCode fltr = new CustomFilterCode(les_dnf);
		            	ExecuteCustomFilter(fltr);
					}
				}
                
            }
            catch (Exception ex)
            {
                KillThreadProgressBar();
                ShowException("", GetTranslator().GetString("FMenuDisplayOnlyOurDNF"), ex);
            }
		}
		void CbFilterFavoritesCheckedChanged(object sender, EventArgs e)
		{
			UpdatePageIfChecked(new object[2] { cbFilterFavorites, cbFilterPopularity }, tabPageFavPop);
		}
		void CbFilterPopularityCheckedChanged(object sender, EventArgs e)
		{
			UpdatePageIfChecked(new object[2] { cbFilterFavorites, cbFilterPopularity }, tabPageFavPop);
		}
		void ComboFavoritesSelectedIndexChanged(object sender, EventArgs e)
		{
			CheckIfNotAlready(cbFilterFavorites);
		}
		void TxtFavoritesValueTextChanged(object sender, EventArgs e)
		{
			CheckIfNotAlready(cbFilterFavorites);
		}
		void ComboPopularitySelectedIndexChanged(object sender, EventArgs e)
		{
			CheckIfNotAlready(cbFilterPopularity);
		}
		void TxtPopularityValueTextChanged(object sender, EventArgs e)
		{
			CheckIfNotAlready(cbFilterPopularity);
		}
		void ClbMltipleFiltersItemCheck(object sender, ItemCheckEventArgs e)
		{
			// Nb item checked
			int nb = clbMltipleFilters.CheckedItems.Count;
			if (e.NewValue == CheckState.Unchecked)
				nb--;
			else
				nb++;
			UpdatePageIfChecked((nb > 0), tabPage11Multi);
		
		}
		
		/// <summary>
		/// Return an empty cache
		/// </summary>
		/// <param name="code">Code of cache</param>
		/// <returns>Empty cache</returns>
		public Geocache GetEmptyCache(string code)
        {
        	Geocache geo = null;
        	geo = new Geocache(this);
            geo._Code = code;
            geo._CacheId = (_iCacheId++).ToString();
            geo._Url = "http://coord.info/" + geo._Code;
            return geo;
        }
		
		void BtnMatrixFilterDTClick(object sender, EventArgs e)
		{
			MatrixDT dtMatrix = new MatrixDT(this);
			dtMatrix.Show();
			btnMatrixFilterDT.Enabled = false;
		}
		

		 
		void completeFromCacheCacheFullToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (!SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled()) return;
			if (GetNumberOfSelectedCaches() == 0) return;
			
			String dbfullpath = HMIToSelectDB("completeFromCacheCacheFullToolStripMenuItem");
        	if (dbfullpath == "")
        		return;
			
			List<Geocache> caches = null;
			try
            {
                _ThreadProgressBarTitle = GetTranslator().GetString("completeFromCacheCacheFullToolStripMenuItem");
                CreateThreadProgressBar();

                caches = GetSelectedCaches();
                
                var codeid = "";
                foreach (Geocache geo in caches)
                {
                    if (geo.ShouldBeCompleted())
                    {
                    	codeid += "'" + geo._Code + "',";
                    }
                }

	       		codeid = codeid.Substring(0, codeid.Length - 1);
	       		String sq = "SELECT * from GeocacheFull where Code in (" + codeid + ")";
	       		MGMDataBase db = new MGMDataBase(this, dbfullpath);
	       		List<Geocache> result = db.PerformSelect(sq, _lastuseddbindbfilter, false);
	       		foreach(var g in result)
	       		{
	       			Geocache geoold = _caches[g._Code];
	       			geoold.Update(g);
	       			_iNbModifiedCaches += geoold.InsertModification("ALL");
	       		}
                
                PostTreatmentLoadCache();
                
                // Better way to do that : only recreate for modified caches
                RecreateVisualElements(caches);

                // On redessine la carte
                BuildCacheMapNew(GetDisplayedCaches());
				
                KillThreadProgressBar();

                MsgActionDone(this);
            }
            catch (Exception ex)
            {
                PostTreatmentLoadCache();

                // Better way to do that : only recreate for modified caches
                RecreateVisualElements(caches);

                KillThreadProgressBar();

                ShowException("", GetTranslator().GetString("completeFromCacheCacheFullToolStripMenuItem"), ex);
            }
		}
		
		/// <summary>
		/// 
		/// </summary>
		public String _lastuseddbindbfilter = "";
		/// <summary>
		/// 
		/// </summary>
		public int _lastlimitusedindbfilter = 1000;
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="keylabel"></param>
		/// <param name="msg"></param>
		/// <param name="verbose"></param>
		/// <param name="proposeCacheCache"></param>
		/// <returns></returns>
		public String HMIToSelectDB(String keylabel, String msg = "", bool verbose = true, bool proposeCacheCache = false)
		{
			// Input parameters
        	List<ParameterObject> lst = new List<ParameterObject>();
        	// On va chercher toutes les .db dans DataPath/DB
            List<String> listDBNames = new List<string>();
            List<String> listDBPath = new List<string>();
            
            bool hasdb = CacheCache.GetAvailableDB(this, out listDBNames, out listDBPath, verbose);
            if (!hasdb && !proposeCacheCache)
            	return "";
            if (proposeCacheCache)
            {
            	// On ajoute CacheCache.db
	            listDBNames.Insert(0, "CacheCache");
	            listDBPath.Insert(0, _cachecache._dbCacheCachePath);
            }
            
            var po = new ParameterObject(ParameterObject.ParameterType.List, listDBNames, "lstdbnames", GetTranslator().GetString("DBFullPath"));
            lst.Add(po);
            if (listDBNames.Contains(_lastuseddbindbfilter))
            	po.DefaultListValue = _lastuseddbindbfilter;
            else
            	_lastuseddbindbfilter = "";
            
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Int, _lastlimitusedindbfilter, "limitnum", GetTranslator().GetString("CacheCacheFilterMaxResults")));
            if (msg != "")
            {
            	po = new ParameterObject(ParameterObject.ParameterType.TextBox, msg, "info", "");
            	po.ReadOnly = true;
				lst.Add(po);
            }

            ParametersChanger changer = new ParametersChanger();
            changer.Title = GetTranslator().GetString(keylabel);
            changer.BtnCancel = GetTranslator().GetString("BtnCancel");
            changer.BtnOK = GetTranslator().GetString("BtnOk");
            changer.ErrorFormater = GetTranslator().GetString("ErrWrongParameter");
            changer.ErrorTitle = GetTranslator().GetString("Error");
            changer.OpenfileTitle = GetTranslator().GetString("LblOpenFiletitle");
            changer.OpenfileBtn = GetTranslator().GetString("LblOpenFileBtn");
            
            changer.Parameters = lst;
            changer.Font = this.Font;
            changer.Icon = this.Icon;

            if (changer.ShowDialog() == DialogResult.OK)
            {
                _lastuseddbindbfilter = lst[0].Value;
        		_lastlimitusedindbfilter = Int32.Parse(lst[1].Value);
                
                int pos = listDBNames.IndexOf(_lastuseddbindbfilter);
        		return listDBPath[pos];
            }
            else
            {
            	MsgActionCanceled(this);
            	return "";
            }
		}
		
		void filteronsqldbToolStripMenuItemClick(object sender, EventArgs e)
		{
			try
            {
				// SQL request
				UpdateFilter();
            	String warning = "";
            	if (
            		((_filter._name != "") && (_filter._iNameType == 4)) ||
            		_filter._bFilterNear ||
            		_filter._bFilterDistance ||
            		_filter._bFilterLastLogDate ||
            		_filter._bFilterFavorites ||
            		_filter._bFilterPopularity ||
            		(_filter._bFilterArea && (_filter.GetAreaArray() != null))
            	)
            	{
            		warning = GetTranslator().GetStringM("filteronsqldbwarning") + "\r\n";
            	}
            	String stm = _filter.ToSQLSelect(_lastlimitusedindbfilter);
            	String msg = String.Format(GetTranslator().GetStringM("AskPerformfilteronsqldb"), warning, stm);
            	
            	String dbfullpath = HMIToSelectDB("filteronsqldbToolStripMenuItem", msg);
            	if (dbfullpath == "")
            		return;
                
	            _ThreadProgressBarTitle = GetTranslator().GetString("filteronsqldbToolStripMenuItem");
                CreateThreadProgressBar();

                // Les caches retournées sont déjà ajoutées à MGM, c'est cool
            	MGMDataBase db = new MGMDataBase(this,dbfullpath);
            	List<Geocache> caches = db.PerformSelect(stm, _lastuseddbindbfilter);
            	
                // On récupère la liste des listview items du résultat
                List<EXListViewItem> forcedList = new List<EXListViewItem>();
                List<Geocache> cachestocreate = new List<Geocache>();
                LvGeocaches.BeginUpdate();
                foreach(Geocache geo in caches)
                {
                	// Soit on est déjà dans MGM et on a un listviewitem (forcedList)
                	// soit il faut créer l'élément graphique (cachestocreate)
                	EXImageListViewItem item = GetVisualEltFromCacheCode(geo._Code);
                	if (item != null)
                	{
                		// on va vérifier : si l'élément graphique existe MAIS qu'il est associé à une cache issue seulement de CACHECACHE *ET* que là on est en train
                		// d'utiliser CacheCacheFull, alors on va recréer cet élément graphique, car il sera plus détaillé
            			// on regarde la cache qui existe dans MGM
            			String placed = item.SubItems[_ID_LVPlaced].Text;
            			if (placed == "")
            			{
            				// ok la seule occurence de cette cache vient de CACHECACHE,
            				// Et là on a une jolie cache détaillée, on va donc récréer l'élement graphique
            				RemoveVisualElt(item);
            				cachestocreate.Add(geo);
            			}
            			else
            			{
            				// Bon on laisse
            				forcedList.Add(item);
            			}
                	}
                	else
                		cachestocreate.Add(geo);
                }
                LvGeocaches.EndUpdate();
                
                if (cachestocreate.Count != 0)
                {
                	// soit on doit en créer un élément graphique
                	List<EXImageListViewItem> listvi = BuildListViewCache(cachestocreate);
                	forcedList.AddRange(listvi);
                }
                
                // On affiche en executant le filtre sur MGM tout simplement
                DoFilter();
                
                KillThreadProgressBar();
                
                MsgActionDone(this);       	
	            	
            }
            catch (Exception ex)
            {
                KillThreadProgressBar();
                ShowException("", GetTranslator().GetString("filteronsqldbToolStripMenuItem"), ex);
            }
		}
		
		void FilterOnCacheCacheToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (!SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled()) return;
			_cachecache.FilterDatabase();
		}
		
		void filterOnFrenchRegionToolStripMenuItemClick(object sender, EventArgs e)
		{
			KmlManager kml = new KmlManager(this);
			kml.FilterOnFrenchArea(1);
		}
		
		void filterOnFrenchDepartmentToolStripMenuItemClick(object sender, EventArgs e)
		{
			KmlManager kml = new KmlManager(this);
			kml.FilterOnFrenchArea(2);
		}
		
		void filterOnFrenchCityToolStripMenuItemClick(object sender, EventArgs e)
		{
			KmlManager kml = new KmlManager(this);
			kml.FilterOnFrenchArea(3);
		}
		   
		void displayFranceCoverageToolStripMenuItemClick(object sender, EventArgs e)
		{
			KmlManager kml = new KmlManager(this);
			kml.DisplayFranceCoverage();
		}	
		
		void generateFranceCoverageToolStripMenuItemClick(object sender, EventArgs e)
		{
			KmlManager kml = new KmlManager(this);
			kml.GenerateFranceCoverage();
		}	
		
		void filterOnCountryToolStripMenuItemClick(object sender, EventArgs e)
		{
			KmlManager kml = new KmlManager(this);
			kml.FilterOnCountry();
		}	
		
		void displayWorldCoverageToolStripMenuItemClick(object sender, EventArgs e)
		{
			KmlManager kml = new KmlManager(this);
			kml.DisplayWorldCoverage();
		}
		
		void AnimateFindsToolStripMenuItemClick(object sender, EventArgs e)
		{
			KmlManager kml = new KmlManager(this);
			kml.AnimateMyFinds();
		}
		
		void geoFranceToolStripMenuItemClick(object sender, EventArgs e)
		{
			MyTools.StartInNewThread("https://play.google.com/store/apps/details?id=com.spaceeye.geofrance2");
		}
		
		void SCDisplayOnMapToolStripMenuItem_Click(object sender, EventArgs e) { SetDisplaySelectionOnCarto(null,null); }
		void SCDisplayDetailsToolStripMenuItem_Click(object sender, EventArgs e) { SetDisplayDetail(null,null); }
		void SCDisplayOnGCToolStripMenuItem_Click(object sender, EventArgs e) { SetDisplayGeocaching(null,null); }
		void SCDisplayModifToolStripMenuItem_Click(object sender, EventArgs e) { SetDisplayModifications(null,null); }
		
		void OCWriteNoteToolStripMenuItem_Click(object sender, EventArgs e) { WriteNoteOnCacheEvent(null,null); }
		void OCViewCacheToolStripMenuItem_Click(object sender, EventArgs e) { ViewOfflineData(null,null); }
		void OCDownloadToolStripMenuItem_Click(object sender, EventArgs e) { CreateOfflineData(null,null); }
		void OCRemoveAllToolStripMenuItem_Click(object sender, EventArgs e) { DeleteOfflineData(null,null); }
		void OCDisplayAllToolStripMenuItem_Click(object sender, EventArgs e) { OpenAllOfflineImage(null,null); }
		
		void FLNUseAsCenterToolStripMenuItem_Click(object sender, EventArgs e) { DefineAsLocationForNearFilter(null,null); }
		
		void WPAddToolStripMenuItem_Click(object sender, EventArgs e) { WaypointContextAdd(null,null); }
		void WPManageToolStripMenuItem_Click(object sender, EventArgs e) { WaypointContextManage(null,null); }
		
		
		void MSAsFoundToolStripMenuItem_Click(object sender, EventArgs e) { SetManualFoundSelection(null,null); }
		void MSToggleToolStripMenuItem_Click(object sender, EventArgs e) { SetManualSelectionToggle(null,null); }
		void MSMarkAllToolStripMenuItem_Click(object sender, EventArgs e) { SetManualSelectionAll(null,null); }
		void MSUnmarkAllToolStripMenuItem_Click(object sender, EventArgs e) { SetManualDeselectionAll(null,null); }
		
		void FSAddToolStripMenuItem_Click(object sender, EventArgs e) { SetAddFav(null,null); }
		void FsDelToolStripMenuItem_Click(object sender, EventArgs e) { SetDelFav(null,null); }
		void AddMLabelSelToolStripMenuItem_Click(object sender, EventArgs e) { SetAddDelTag(null,null); }
		
		void RemoveSelCachesToolStripMenuItem_Click(object sender, EventArgs e) { SetRemoveSelectionFromMGM(null,null); }
		void IgnoreSelCachesToolStripMenuItem_Click(object sender, EventArgs e) { SetIgnoreSelectionFromMGM(null,null); }

    }
}
