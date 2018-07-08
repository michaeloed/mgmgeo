using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using MyGeocachingManager.Geocaching;
using SpaceEyeTools.HMI;
using System.Net;
using SpaceEyeTools;
using System.IO;
using System.Threading;
using System.Configuration;
using System.Diagnostics;
using SpaceEyeTools.Markdown;

namespace MyGeocachingManager
{
    /// <summary>
    /// Form to log a geocache
    /// </summary>
    public partial class GCLogHMI : Form
    {
    	/// <summary>
    	/// Template type for the log :
    	/// None: standard log
    	/// Simple: only text is allowed
    	/// Complex: can change text, log type
    	/// </summary>
    	public enum TemplateType 
    	{
    		/// <summary>
    		/// None: standard log
    		/// </summary>
    		None,
            /// <summary>
            /// Simple: can change text, TBs, favorite
            /// </summary>
            Simple,
			/// <summary>
			/// Complex: can change text, date, log type, TBs, favorite
			/// </summary>
			Complex,
				/// <summary>
			/// TB: only TB / Geocoins
			/// </summary>
			TB,
			/// <summary>
			/// TextOnly : can change text only
			/// </summary>
			TextOnly
    	}
    	
    	/// <summary>
    	/// Result of SubmitLog function
    	/// </summary>
    	public enum SubmitResult
    	{
    		/// <summary>
    		/// Submit succeded
    		/// </summary>
    		Success,
    		/// <summary>
    		/// Submit failed
    		/// </summary>
    		Error,
    		/// <summary>
    		/// User decided to retry (log modification)
    		/// </summary>
    		Retry
    	}
    	
        MainWindow _daddy = null;
        List<Geocache> _caches = null;

        // Pour la navigation entre les logs
        private Action<GCLogHMI, int> _handlerPrevious = null;
        private Action<GCLogHMI, int> _handlerNext = null;
        private Action<GCLogHMI, int> _handlerSave = null;
        
        /// <summary>
        /// Index of item within list
        /// </summary>
        public int _iItemPosition = -1;
        
        
        // Les infos persistantes
        static int _iFound = 0;
        static List<TBInfo> _lTBInfo = null;

        bool _bSilent = false;
        TemplateType _templateType = TemplateType.None;
        int _iCurrentNumbering = 1;
        Markdown _Markdown = new Markdown();
        Dictionary<String, String> _dicoSmileys = new Dictionary<string, string>();
        
        /// <summary>
        /// Template to use for each cache log if specified
        /// </summary>
        private String _template = "";
        
        /// <summary>
        /// Type used for last log
        /// </summary>
        public int _iTypeReallyLogged = 0;
        
        /// <summary>
        /// Action list for selected TBs
        /// </summary>
        public List<KeyValuePair<string, string>> _TBActionList = null;

        private Dictionary<KeyValuePair<string, string>, int> _importedTBs = null;

        /// <summary>
        /// If different of -1, will override runcount computed value
        /// </summary>
        private int _overrideRunCount = -1;
        
        /// <summary>
        /// If different of -1, will override runtotal computed value
        /// </summary>
        private int _overrideRunTotal = -1;
        
        Dictionary<int, Image> _imgForLogTypes = new Dictionary<int, Image>();
        private void cbLogTypeloghmi_DrawItem(object sender, DrawItemEventArgs e)
		{
        	//Draw background of the item e.DrawBackground(); 
        	e.DrawBackground();
			if (e.Index != -1)
			{ 
				int ItemHeight = 16;
				ComboBox cb = sender as ComboBox;
				
				// selected item 
				ComboItem ci = cbLogTypeloghmi.Items[e.Index] as ComboItem;
				int val = (Int32)(ci.Value);
				
				//Draw the image in combo box using its bound and ItemHeight 
				if (_imgForLogTypes.ContainsKey(val))
					e.Graphics.DrawImage(_imgForLogTypes[val], e.Bounds.X, e.Bounds.Y, ItemHeight, ItemHeight);
				
				//we need to draw the item as string because we made drawmode to ownervariable
				e.Graphics.DrawString(ci.Text, cb.Font, Brushes.Black, new RectangleF(e.Bounds.X + ItemHeight, e.Bounds.Y, cb.DropDownWidth, ItemHeight));
			}
			//draw rectangle over the item selected 
			e.DrawFocusRectangle(); 
        }
        
        
        /// <summary>
        /// Default constructor block for ANY constructor !
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        /// <param name="translate">If true, TranslateForm is called</param>
        private void BaseGCLogHMI(MainWindow daddy, bool translate)
        {
        	_Markdown.AutoNewLines = true;
			_daddy = daddy;
			InitializeComponent();

			// On masque au lancement
			_daddy.CloseCacheDetail();
			
			// Pour afficher les icones dans la combo
			_imgForLogTypes.Add(9, _daddy.GetImageSized("Will Attend"));
        	_imgForLogTypes.Add(4, _daddy.GetImageSized("Write note"));
        	_imgForLogTypes.Add(10, _daddy.GetImageSized("Attended"));
        	_imgForLogTypes.Add(2, _daddy.GetImageSized("Found it"));
        	_imgForLogTypes.Add(3, _daddy.GetImageSized("Didn't find it"));
        	_imgForLogTypes.Add(7, _daddy.GetImageSized("Needs Archived"));
        	_imgForLogTypes.Add(45, _daddy.GetImageSized("Needs Maintenance"));
        	_imgForLogTypes.Add(46, _daddy.GetImageSized("Owner Maintenance"));
        	_imgForLogTypes.Add(11, _daddy.GetImageSized("Webcam Photo Taken"));
			cbLogTypeloghmi.DrawMode = DrawMode.OwnerDrawFixed;
    		cbLogTypeloghmi.DrawItem += cbLogTypeloghmi_DrawItem;
			
			// On créé les handlers des boutons smileys
            this.button1.Click += new System.EventHandler(this.buttonSmileys_Click);
            this.button2.Click += new System.EventHandler(this.buttonSmileys_Click);
            this.button3.Click += new System.EventHandler(this.buttonSmileys_Click);
            this.button4.Click += new System.EventHandler(this.buttonSmileys_Click);
            this.button5.Click += new System.EventHandler(this.buttonSmileys_Click);
            this.button6.Click += new System.EventHandler(this.buttonSmileys_Click);
            this.button7.Click += new System.EventHandler(this.buttonSmileys_Click);
            this.button8.Click += new System.EventHandler(this.buttonSmileys_Click);
            this.button9.Click += new System.EventHandler(this.buttonSmileys_Click);
            this.button10.Click += new System.EventHandler(this.buttonSmileys_Click);
            this.button11.Click += new System.EventHandler(this.buttonSmileys_Click);
            this.button12.Click += new System.EventHandler(this.buttonSmileys_Click);
            this.button13.Click += new System.EventHandler(this.buttonSmileys_Click);
            this.button14.Click += new System.EventHandler(this.buttonSmileys_Click);
            this.button15.Click += new System.EventHandler(this.buttonSmileys_Click);
            this.button16.Click += new System.EventHandler(this.buttonSmileys_Click);
            this.button17.Click += new System.EventHandler(this.buttonSmileys_Click);
            this.button18.Click += new System.EventHandler(this.buttonSmileys_Click);
            this.button19.Click += new System.EventHandler(this.buttonSmileys_Click);
            this.button20.Click += new System.EventHandler(this.buttonSmileys_Click);

            this.btnmark01.Click += new System.EventHandler(this.buttonMarkdown_Click);
            this.btnmark02.Click += new System.EventHandler(this.buttonMarkdown_Click);
            this.btnmark03.Click += new System.EventHandler(this.buttonMarkdown_Click);
            this.btnmark04.Click += new System.EventHandler(this.buttonMarkdown_Click);
            this.btnmark05.Click += new System.EventHandler(this.buttonMarkdown_Click);
            this.btnmark06.Click += new System.EventHandler(this.buttonMarkdown_Click);
            this.btnmark07.Click += new System.EventHandler(this.buttonMarkdown_Click);
            this.btnmark08.Click += new System.EventHandler(this.buttonMarkdown_Click);
            this.btnmark09.Click += new System.EventHandler(this.buttonMarkdown_Click);
            this.btnmark10.Click += new System.EventHandler(this.buttonMarkdown_Click);
            
			if (translate)
				TranslateForm();
			this.Font = _daddy.Font;
			this.Icon = _daddy.Icon;
			lblloglength.Text = "(0/4000)";
			
			// Le dico des smileys
			_dicoSmileys = GetDicoSmileys(_daddy);
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="daddy"></param>
        /// <returns></returns>
        public static Dictionary<String, String> GetDicoSmileys(MainWindow daddy)
        {
        	Dictionary<String, String> dicoSmileys = new Dictionary<string, string> ();
        	// Le dico des smileys
			String imgpath = daddy.GetResourcesDataPath() + Path.DirectorySeparatorChar + "Img"+ Path.DirectorySeparatorChar + "Smileys" + Path.DirectorySeparatorChar;
			
			dicoSmileys.Add("[:)]", imgpath + "icon_smile.gif");
			dicoSmileys.Add("[8]", imgpath + "icon_smile_8ball.gif");
			dicoSmileys.Add("[:(!]", imgpath + "icon_smile_angry.gif");
			dicoSmileys.Add("[^]", imgpath + "icon_smile_approve.gif");
			dicoSmileys.Add("[:D]", imgpath + "icon_smile_big.gif");
			dicoSmileys.Add("[B)]", imgpath + "icon_smile_blackeye.gif");
			dicoSmileys.Add("[:I]", imgpath + "icon_smile_blush.gif");
			dicoSmileys.Add("[:o)]", imgpath + "icon_smile_clown.gif");
			dicoSmileys.Add("[8D]", imgpath + "icon_smile_cool.gif");
			dicoSmileys.Add("[xx(]", imgpath + "icon_smile_dead.gif");
			dicoSmileys.Add("[V]", imgpath + "icon_smile_dissapprove.gif");
			dicoSmileys.Add("[}:)]", imgpath + "icon_smile_evil.gif");
			dicoSmileys.Add("[:X]", imgpath + "icon_smile_kisses.gif");
			dicoSmileys.Add("[?]", imgpath + "icon_smile_question.gif");
			dicoSmileys.Add("[:(]", imgpath + "icon_smile_sad.gif");
			dicoSmileys.Add("[:O]", imgpath + "icon_smile_shock.gif");
			dicoSmileys.Add("[8)]", imgpath + "icon_smile_shy.gif");
			dicoSmileys.Add("[|)]", imgpath + "icon_smile_sleepy.gif");
			dicoSmileys.Add("[:P]", imgpath + "icon_smile_tongue.gif");
			dicoSmileys.Add("[;)]", imgpath + "icon_smile_wink.gif");
			
			return dicoSmileys;
        }
        
        /// <summary>
        /// Constructor standard
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        /// <param name="caches">caches to log</param>
        /// <param name="templateType">use this form for template</param>
        /// <param name="bSilent">Display only error message</param>
        /// <param name="template">Default text log (template)</param>
        /// <param name="overrideruncount">If different of -1, override runcount value</param>
        /// <param name="overrideruntotal">If different of -1, override runttoal value</param>
        public GCLogHMI(MainWindow daddy, List<Geocache> caches, TemplateType templateType, bool bSilent, String template, int overrideruncount = -1, int overrideruntotal = -1)
        {
        	_caches = caches;
            _template = template;
            _templateType = templateType;         
            _bSilent = bSilent;
            _overrideRunCount = overrideruncount;
            _overrideRunTotal = overrideruntotal;
        	BaseGCLogHMI(daddy, true);
            
            // On va désactiver certaines choses :
            UpdateHMIAccordingToTemplateType();
            txtCommentsloghmi.Text = LoadDefaultLogHeader();
            
        }

        private void UpdateHMICacheTextAndHint()
        {
        	if (_caches != null)
        	{
	        	if (_caches.Count == 1)
	            {
	        		Geocache geo = _caches[0];
	        		lblCacheInfo.Text = GetCacheTextInfo(geo);
	        		if (geo._Name != "")
	        		{
		        		// Petit bonus, si on a un type, on l'ajoute sur le bouton
		        		int it = _daddy.getIndexImages(geo._Type);
	            		Image image = _daddy.getImageFromIndex(it);
	            		Bitmap bmp = MyTools.ResizeImage(image, 16, 16);
	            		btnCacheInfologhmi.Image = bmp;
	            		btnCacheInfologhmi.Text = "";
	        		}
	        		else
	        		{
	        			btnCacheInfologhmi.Image = null;
	        			btnCacheInfologhmi.Text = "+";
	        		}
	                lblhintvalue.Text = geo._Hint;
	            }
	            else
	            {
	            	lblhint.Visible = false;
	            	lblhintvalue.Visible = false;
	            	lblfieldnote.Visible = false;
	            	lblfieldnotvalue.Visible = false;
	                lblCacheInfo.Text = _caches.Count.ToString() + " " + _daddy.GetTranslator().GetString("lblcacheswillbelogged");
	            }
        	}
        	else
        	{
        		lblhint.Visible = false;
            	lblhintvalue.Visible = false;
            	lblfieldnote.Visible = false;
	            lblfieldnotvalue.Visible = false;
                lblCacheInfo.Visible = false;
        	}
        }
        
        private void UpdateHMIAccordingToTemplateType()
        {
        	// On va désactiver certaines choses :
        	if (_templateType == GCLogHMI.TemplateType.TextOnly)
        	{
        		cbFavsloghmi.Visible = false;
        		panelCaches.Visible = false;
	            panelTxt.Visible = true;
	            panelImgs.Visible = false;
	            panelTB.Visible = false;
	            panelTxt.Location = panelCaches.Location;
	            panelBtn.Location = new Point(panelTxt.Location.X, panelTxt.Location.Y + panelTxt.Height + 4);
	            this.Height = panelBtn.Location.Y + panelBtn.Height + 50;
            
        		// Et surtout on se casse !!!
        		return;
        	}
        	if (_templateType == GCLogHMI.TemplateType.None)
            {
            	// Rien de particulier
            }
            else if (_templateType == GCLogHMI.TemplateType.Simple)
            {
            	panelCaches.Visible = false;
                panelTxt.Location = panelCaches.Location;
                panelTB.Location = new Point(panelTxt.Location.X, panelTxt.Location.Y + panelTxt.Height + 4);
                panelImgs.Location = new Point(panelTB.Location.X, panelTB.Location.Y + panelTB.Height + 4);
                panelBtn.Location = new Point(panelImgs.Location.X, panelImgs.Location.Y + panelImgs.Height + 4);
                this.Height = panelBtn.Location.Y + panelBtn.Height + 50;
            }
            else if (_templateType == GCLogHMI.TemplateType.Complex)
            {
                // La encore, rien de nouveau
            }
            UpdateHMICacheTextAndHint();
            PopulateListTBs(false);
        }
        
        private void UpdateHMIForTBOnly()
        {
        	// On va désactiver certaines choses :
        	panelCaches.Visible = false;
            panelTxt.Visible = false;
            panelImgs.Visible = false;
            panelTB.Location = panelCaches.Location;
            panelBtn.Location = new Point(panelTB.Location.X, panelTB.Location.Y + panelTB.Height + 4);
            this.Height = panelBtn.Location.Y + panelBtn.Height + 50;
           
            PopulateListTBs(true);
        }
        
        /// <summary>
        /// Constructor only for TB template !
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        public GCLogHMI(MainWindow daddy)
        {
        	_templateType = GCLogHMI.TemplateType.TB;
        	BaseGCLogHMI(daddy, true);
        	
            UpdateHMIForTBOnly();
        }
        
        /// <summary>
        /// Constructor only for template appliance !
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        /// <param name="templateType">use this form for template</param>
        public GCLogHMI(MainWindow daddy, TemplateType templateType)
        {
        	_templateType = templateType;
        	BaseGCLogHMI(daddy, true);
        	
            if (_templateType == GCLogHMI.TemplateType.TB)
            	UpdateHMIForTBOnly();
            else
            {
            	lblloglength.Text = "(0/4000)";
            	UpdateHMIAccordingToTemplateType();
            	txtCommentsloghmi.Text = LoadDefaultLogHeader();
            }
            
        }
        
        /// <summary>
        /// Load/Reload a simple cache with preexisting log info
        /// </summary>
        /// <param name="cache">cache to log</param>
        /// <param name="date">Log date</param>
        /// <param name="fieldnote">Field note if applicable</param>
        /// <param name="iLogType">Log type</param>
        /// <param name="templateType">use this form for template type</param>
        /// <param name="bSilent">Display only error message</param>
        /// <param name="template">Default text log (template)</param>
        /// <param name="infos">Log extra infos</param>
        /// <param name="overrideruncount">If different of -1, override runcount value</param>
        /// <param name="overrideruntotal">If different of -1, override runttoal value</param>
        /// <param name="bAutologBasedOnFieldNotes">Form is not displayed, log is automatically done based on provided information</param>
        public void Reload(Geocache cache, DateTime date, String fieldnote, int iLogType, TemplateType templateType, bool bSilent,
                        bool bAutologBasedOnFieldNotes, String template, LogExtraInfo infos, int overrideruncount, int overrideruntotal)
        {
        	_bSilent = bSilent;
            _caches = new List<Geocache>();
            _caches.Add(cache);
            _template = template;
            _templateType = templateType;
            _overrideRunCount = overrideruncount;
            _overrideRunTotal = overrideruntotal;
            TranslateForm();
            lblloglength.Text = "(0/4000)";
            dtPickerloghmi.Value = date;
            foreach (Object o in cbLogTypeloghmi.Items)
            {
                ComboItem ci = (ComboItem)o;
                if ((int)ci.Value == iLogType)
                {
                    cbLogTypeloghmi.SelectedItem = o;
                    break;
                }
            }
            lblCacheInfo.Text = GetCacheTextInfo(_caches[0]);
            lblhintvalue.Text = _caches[0]._Hint;
            
			txtCommentsloghmi.Text = LoadDefaultLogHeader();
			lblfieldnotvalue.Text = fieldnote;
			
			if (infos != null)
			{
				cbFavsloghmi.Checked = infos._bFavorited;
            	ImportTBs(infos._dicoTBs);
            	ImportImages(infos._dicoImages);
            	if (infos._bUseDateAndType)
            	{
            		SelectLogType(infos._iLogType);
            		dtPickerloghmi.Value = infos._logDate;
            	}
			}
			
            if (_templateType == GCLogHMI.TemplateType.None)
            {
            	PopulateListTBs(false);
	            if (bAutologBasedOnFieldNotes)
	            {
	            	// Petite vérification sur le log lui même
	            	if (txtCommentsloghmi.Text.Length <= 5) // MPLC, etc...
	            	{
	            		txtCommentsloghmi.Text = "# This log has been automatically generated using MyGeocachingManager (MGM) since the Geocacher forgot to provide a decent log text...#\r\n\r\n";
	            	}
	            	
	            	// On soumet le log
	            	// Et on ferme même si une erreur
	            	if (SubmitLog(false) != SubmitResult.Success)
	            	{
	            		this.DialogResult = DialogResult.Cancel;
	            	}
	            }
            }
            else
            {
            	// On va désactiver certaines choses :
            	UpdateHMIAccordingToTemplateType();
            }
        }
        
        /// <summary>
        /// Constructor to log a simple cache with preexisting log info
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        /// <param name="cache">cache to log</param>
        /// <param name="date">Log date</param>
        /// <param name="fieldnote">Field note if applicable</param>
        /// <param name="iLogType">Log type</param>
        /// <param name="templateType">use this form for template type</param>
        /// <param name="bSilent">Display only error message</param>
        /// <param name="template">Default text log (template)</param>
        /// <param name="infos">Log extra infos</param>
        /// <param name="overrideruncount">If different of -1, override runcount value</param>
        /// <param name="overrideruntotal">If different of -1, override runttoal value</param>
        /// <param name="bAutologBasedOnFieldNotes">Form is not displayed, log is automatically done based on provided information</param>
        public GCLogHMI(MainWindow daddy, Geocache cache, DateTime date, String fieldnote, int iLogType, TemplateType templateType, bool bSilent,
                        bool bAutologBasedOnFieldNotes, String template, LogExtraInfo infos, int overrideruncount, int overrideruntotal)
        {
        	BaseGCLogHMI(daddy, false);
        	Reload(cache, date, fieldnote, iLogType, templateType, bSilent, bAutologBasedOnFieldNotes, template, infos, overrideruncount, overrideruntotal);
        }

        /// <summary>
        /// Select the right log type in HMI
        /// </summary>
        /// <param name="iLogType">log type</param>
        public void SelectLogType(int iLogType)
        {
        	foreach (Object o in cbLogTypeloghmi.Items)
            {
                ComboItem ci = (ComboItem)o;
                if ((int)ci.Value == iLogType)
                {
                    cbLogTypeloghmi.SelectedItem = o;
                    break;
                }
            }
        }
        
        /// <summary>
        /// Return name of default log text file (template_logheader.txt)
        /// </summary>
        /// <returns>Return name of default log text file</returns>
        static public String GetDefaultLogTextFileName()
        {
        	return "template_logheader.txt";
        	
        }
        
        /// <summary>
        /// Load a default file, if present in the exe directory (template_logheader.txt)
        /// and insert its content in front of each log
        /// </summary>
        /// <returns>log header</returns>
        private String LoadDefaultLogHeader()
        {
        	if (_template != "")
        		return _template + "\r\n";
        	try
        	{
        		String headerfile = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + GetDefaultLogTextFileName();
        		if (File.Exists(headerfile))
        		{
	        		String header = "";
		            using (StreamReader responseStream = new StreamReader(headerfile, System.Text.Encoding.GetEncoding("iso-8859-1")))
		            {
		                header = responseStream.ReadToEnd();
		                responseStream.Close();
		            }
		            return header;
        		}
        		else
        			return "";
        	}
        	catch(Exception)
        	{
        		return "";
        	}
        }
        
        /// <summary>
        /// Automatically translate form labels
        /// </summary>
        public void TranslateForm()
        {
           
            btnLogHMISaveCurrent.Text = _daddy.GetTranslator().GetString("btnLogHMISaveCurrent");
            lblCacheName.Text = _daddy.GetTranslator().GetString("lblcachename");
            lblTypeOfLog.Text = _daddy.GetTranslator().GetString("lbltypeoflog");
            lblDateLogged.Text = _daddy.GetTranslator().GetString("lbldatelogged");
            lblComments.Text = _daddy.GetTranslator().GetString("lblcomments");
            lblPreview.Text = _daddy.GetTranslator().GetString("lblpreview");
            lblhint.Text = _daddy.GetTranslator().GetString("LVHint");
            lblfieldnote.Text = _daddy.GetTranslator().GetString("LblFieldNote");
            cbFavsloghmi.Text = _daddy.GetTranslator().GetString("lbladdfavorite");
            lblTBs.Text = _daddy.GetTranslator().GetString("lbltbs");
            btnCancelloghmi.Text = _daddy.GetTranslator().GetString("BtnCancel");
            btnDroppedAllloghmi.Text = _daddy.GetTranslator().GetString("LblTBAllDroppedOff");
            btnVisitAllloghmi.Text = _daddy.GetTranslator().GetString("LblTBAllVisited");
            btnClearAllloghmi.Text = _daddy.GetTranslator().GetString("LblTBAllClear");
            btnReloadTBloghmi.Text = _daddy.GetTranslator().GetString("LblReloadTBs");
            if (!_daddy.GetInternetStatus())
            {
            	btnReloadTBloghmi.Enabled = false;
            }
            lvTBsloghmi.Columns.Clear();
            lvTBsloghmi.Columns.Add(_daddy.GetTranslator().GetString("LVCode"), 60);
            lvTBsloghmi.Columns.Add(_daddy.GetTranslator().GetString("LVName"), 250);
            lvTBsloghmi.Columns.Add(_daddy.GetTranslator().GetString("LblTBDroppedOff"), 50);
            lvTBsloghmi.Columns.Add(_daddy.GetTranslator().GetString("LblTBVisited"), 50);
            
            lblImgs.Text = _daddy.GetTranslator().GetString("lblImgs");
            btnFnAddImg.Text = _daddy.GetTranslator().GetString("btnFnAddImg");
            btnFnRemoveImg.Text = _daddy.GetTranslator().GetString("btnFnRemoveImg");
            lvImgsLogHMI.Columns.Clear();
            lvImgsLogHMI.Columns.Add(_daddy.GetTranslator().GetString("FnColImgName"), 60);
            lvImgsLogHMI.Columns.Add(_daddy.GetTranslator().GetString("FnColImgDesc"), 100);
            lvImgsLogHMI.Columns.Add(_daddy.GetTranslator().GetString("FnColImgPath"), 250);
          
            this.Text = GetMainHMITextFromTemplateType();
            if (_templateType == GCLogHMI.TemplateType.TB)
            {
            	btnSubmitloghmi.Text = _daddy.GetTranslator().GetString("LblLogTemplateTB");
            }
            else if (_templateType == GCLogHMI.TemplateType.TextOnly)
            {
            	btnSubmitloghmi.Text = _daddy.GetTranslator().GetString("LblLogTextOnly");
            }
            else
            {
	            if (_templateType != GCLogHMI.TemplateType.Simple) // donc none ou complexe
	            {
	            	cbLogTypeloghmi.Items.Clear();
	            	if (_caches[0].IsEventType())
		            {
		                PopulateCombo("LOG_Will_Attend", 9);
		                PopulateCombo("LOG_Write_note", 4);
		                PopulateCombo("LOG_Attended", 10);
		            }
	            	else if (_caches[0].IsWebcamType()) // BCR 20170822
		            {
		                PopulateCombo("LOG_Webcam_Photo_Taken", 11);
		                PopulateCombo("LG_Didnt_find_it", 3);
		                PopulateCombo("LOG_Write_note", 4);
		                //PopulateCombo("LOG_Needs_Archived", 7);
		                PopulateCombo("LOG_Needs_Maintenance", 45);
		                PopulateCombo("LG_Owner_Maintenance", 46);
		            }
	            	else if (_caches[0]._Type == "") // BCR 20170822
	            	{
	            		// Ben on sait pas ! Donc on propose tout !
		                PopulateCombo("LG_Found_it", 2);
		                PopulateCombo("LG_Didnt_find_it", 3);
		                PopulateCombo("LOG_Will_Attend", 9);
		                PopulateCombo("LOG_Attended", 10);
		                PopulateCombo("LOG_Webcam_Photo_Taken", 11);
		                PopulateCombo("LG_Didnt_find_it", 3);
		                PopulateCombo("LOG_Write_note", 4);
		                //PopulateCombo("LOG_Needs_Archived", 7);
		                PopulateCombo("LOG_Needs_Maintenance", 45);
		                PopulateCombo("LG_Owner_Maintenance", 46);
		            }
		            else
		            {
		                PopulateCombo("LG_Found_it", 2);
		                PopulateCombo("LG_Didnt_find_it", 3);
		                PopulateCombo("LOG_Write_note", 4);
		                //PopulateCombo("LOG_Needs_Archived", 7);
		                PopulateCombo("LOG_Needs_Maintenance", 45);
		                PopulateCombo("LG_Owner_Maintenance", 46);
		            }
		            cbLogTypeloghmi.SelectedIndex = 0;
		            
	            }
	            
	            if (_templateType == GCLogHMI.TemplateType.None)
	            {
		            btnSubmitloghmi.Text = _daddy.GetTranslator().GetString("btnSubmitLog");
	            }
	            else if (_templateType == GCLogHMI.TemplateType.Simple)
	            {
	            	btnSubmitloghmi.Text = _daddy.GetTranslator().GetString("LblLogTemplateSimple");
	            }
	            else if (_templateType == GCLogHMI.TemplateType.Complex)
	            {
	            	btnSubmitloghmi.Text = _daddy.GetTranslator().GetString("LblLogTemplateComplex");
	            }
            }
            _daddy.TranslateTooltips(this, null);

        }

        private String GetMainHMITextFromTemplateType()
        {
        	String addon = "";
        	if ((_overrideRunCount != -1) && (_overrideRunTotal != -1))
        	{
        		addon = " (" + _overrideRunCount.ToString() + "/" + _overrideRunTotal.ToString() + ")";
        	}
        	if (_templateType == GCLogHMI.TemplateType.TB)
            {
            	return _daddy.GetTranslator().GetString("LblLogTemplateTB");
            }
           	if (_templateType == GCLogHMI.TemplateType.TextOnly)
            {
            	return _daddy.GetTranslator().GetString("LblLogTextOnly");
            }
           	else if (_templateType == GCLogHMI.TemplateType.None)
            {
	            return _daddy.GetTranslator().GetString("GCLogHMI") + addon;
            }
            else if (_templateType == GCLogHMI.TemplateType.Simple)
            {
            	return _daddy.GetTranslator().GetString("LblLogTemplateSimple") + addon;
            }
            else if (_templateType == GCLogHMI.TemplateType.Complex)
            {
            	return _daddy.GetTranslator().GetString("LblLogTemplateComplex") + addon;
            }
            return "";
        }
        
        private void PopulateListTBs(bool force)
        {
            //_daddy._ThreadProgressBarTitle = _daddy.GetTranslator().GetString("lbltbsGet");
            //_daddy.CreateThreadProgressBar();

            if (!_daddy.GetInternetStatus())
            	return;
            
            try
            {
                if (force || (_lTBInfo == null))
                {
                    // On a rien donc on peuple
                    CookieContainer cookieJar = _daddy.CheckGCAccount(true, false);

                    // Recupère les infos de la cache
                    // Récupération d'une page de geocache
                    // ***********************************
                    String url;
                    HttpWebRequest objRequest;
                    HttpWebResponse objResponse;
                    String response;
                    String backupcacheid = "";
                    if ((_caches != null) && (_caches.Count != 0))
                    {
                        url = _caches[0]._Url;
                        backupcacheid = _caches[0]._CacheId;
                        int minid =  _daddy._iMinCacheIdMGM;
                        if (String.Compare(backupcacheid,minid.ToString()) >= 0)
                        	backupcacheid = ""; // C'est une cache générée, il faut appeler le web
                    }
                    else
                    {
                        url = "http://coord.info/GC5552H";
                        backupcacheid = "255256"; // une basic : https://coord.info/GCPBD0
                    }

                    // Si on a déjà un cacheid, pas besoin de s'embêter à appeler la cache
                    String cacheid = "";
                    if (backupcacheid == "")
                    {
                    	
                    	// Bon ben là il faut appeler :-(
                    	objRequest = (HttpWebRequest)WebRequest.Create(url);
                    	objRequest.Proxy = _daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
                    	objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
                    	objResponse = (HttpWebResponse)objRequest.GetResponse();
                    	using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
                    	{
	                        response = responseStream.ReadToEnd();
                        	responseStream.Close();
                    	}
                    	cacheid = MyTools.GetSnippetFromText("/seek/log.aspx?ID=", "&lcn=1", response);
                    }
                    else
                    	cacheid = backupcacheid;

                    // Récupération des TBs
                    // **************************
                    //url = "https://www.geocaching.com/seek/log.aspx?ID=" + cacheid;
                    url = "https://www.geocaching.com/play/geocache/optout/" + cacheid;
                    objRequest = (HttpWebRequest)WebRequest.Create(url);
                    objRequest.Proxy = _daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
                    objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
                    objResponse = (HttpWebResponse)objRequest.GetResponse();
                    using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
                    {
                        response = responseStream.ReadToEnd();
                        responseStream.Close();
                    }

                    // Petit bonus : le nombre de caches déjà trouvées
                    String found = MyTools.GetSnippetFromText("<span class=\"cache-count\">", " ", response);
                    found = found.Replace(",", "");
                    found = found.Replace(".", "");
                    _iFound = 0;
                    Int32.TryParse(found, out _iFound);
                    
                    // Les TBs
                    response = MyTools.GetSnippetFromText("LogTrackablesTable Table", "</table>", response);
                    List<String> lTBBlocs = MyTools.GetSnippetsFromText("<tr id=\"ctl00_ContentBody_LogBookPanel1_uxTrackables_repTravelBugs_ctl", "</tr>", response);
                    _lTBInfo = new List<TBInfo>();
                    foreach (String tb in lTBBlocs)
                    {
                        String code = MyTools.GetSnippetFromText("<a href=\"/track/details.aspx?tracker=", "\">", tb);
                        String name = MyTools.GetSnippetFromText("</a></td>", "</td>", tb);
                        name = MyTools.GetSnippetFromText("<td>", "", name);
                        String id = MyTools.GetSnippetFromText("<option value=\"", "\">", tb);
                        
                        TBInfo tinfo = new TBInfo();
                        tinfo._code = code;
                        tinfo._id = id;
                        tinfo._name = name;
                        _lTBInfo.Add(tinfo);
                    }
                }

                // Quoi qu'il advienne, on a une liste de TB statique, on recharge juste
                this.Text = GetMainHMITextFromTemplateType() + " - " + ConfigurationManager.AppSettings["owner"] + ": " + _iFound.ToString() + _daddy.GetTranslator().GetString("LblCacheFound");

                lvTBsloghmi.Items.Clear();
                foreach (TBInfo tinfo in _lTBInfo)
                {
                    ListViewItem item = lvTBsloghmi.Items.Add(tinfo._code);
                    item.Tag = tinfo._id;
                    item.SubItems.Add(tinfo._name);
                    ListViewItem.ListViewSubItem sitem = item.SubItems.Add(_daddy.GetTranslator().GetString("BtnNo"));
                    sitem.Tag = 0;
                    sitem = item.SubItems.Add(_daddy.GetTranslator().GetString("BtnNo"));
                    sitem.Tag = 0;
                    ColorItemTB(item);
                }

                // On applique la liste des TBs importés si elle est là
                ApplyImportedTBsOnList();

                //_daddy.KillThreadProgressBar();
            }
            catch (Exception)
            {
                //_daddy.KillThreadProgressBar();
            }
        }

        private void PopulateCombo(String untranslated_label, int value)
        {
            ComboItem item = new ComboItem();
            item.Text = _daddy.GetTranslator().GetString(untranslated_label);
            item.Value = value;
            cbLogTypeloghmi.Items.Add(item);
        }

        
        
		private static String CreateDebugInfo(MainWindow daddy, Geocache cache, DateTime date, String logtext, bool bFav, int iLogType,
                                              List<KeyValuePair<string, string>> tbs, Dictionary<String, KeyValuePair<String, String>> images,
                                              bool bDiscardAlreadyDroppedTBs, int runcount, int runtotal)
		{
			String debug = "";
			debug += "Name: " + cache._Name + "\r\n";
			debug += "Date: " + date.ToString() + "\r\n";
			debug += "bFav: " + bFav.ToString() + "\r\n";
			debug += "iLogType: " + iLogType.ToString() +"\r\n";
			String tb = "TB: ";
			if (tbs != null)
			{
				foreach(KeyValuePair<String, String> pair in tbs)
					tb += pair.Key + " " + pair.Value + ";";
			}
			debug += tb + "\r\n";
			debug += "bDiscardAlreadyDroppedTBs: " + bDiscardAlreadyDroppedTBs.ToString() + "\r\n";
			debug += "runcount: " + runcount.ToString() + "\r\n";
			debug += "runtotal: " + runtotal.ToString() + "\r\n";
			
			String imgs = "IMGS: ";
			if (images != null)
			{
				foreach(KeyValuePair<String, KeyValuePair<String, String>> pair in images)
				{
					imgs += "\r\n     " + pair.Value.Key + ": " + pair.Value.Value + " -> " + pair.Key;
				}
			}
			debug += imgs + "\r\n";
			
			debug += "logtext: " + logtext;
			return debug;
		}
		
		/// <summary>
		/// Updload an image to an existing log
		/// </summary>
		/// <param name="url">url for upload</param>
		/// <param name="paramName">parameter name for file</param>
		/// <param name="file">filename (complete path)</param>
		/// <param name="contentType">file content type</param>
		/// <param name="nvc">list of post params</param>
		/// <param name="proxy">proxy (can be null)</param>
		/// <param name="cookieJar">cookies for authentication</param>
		/// <returns>true if successful</returns>
		static public bool HttpUploadFile(string url, string paramName, string file, string contentType, NameValueCollection nvc, WebProxy proxy, CookieContainer cookieJar) 
	    {
			
        	string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
        	byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

        	HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
        	wr.ContentType = "multipart/form-data; boundary=" + boundary;
        	wr.Method = "POST";
        	wr.KeepAlive = true;
        	wr.Proxy = proxy; // Là encore, on peut virer le proxy si non utilisé (NULL)
        	wr.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
        	wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

        	Stream rs = wr.GetRequestStream();

        	string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
	        foreach (string key in nvc.Keys)
	        {
	            rs.Write(boundarybytes, 0, boundarybytes.Length);
	            string formitem = string.Format(formdataTemplate, key, nvc[key]);
	            byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
	            rs.Write(formitembytes, 0, formitembytes.Length);
	        }
	        rs.Write(boundarybytes, 0, boundarybytes.Length);

	        string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
	        string header = string.Format(headerTemplate, paramName, file, contentType);
	        byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
	        rs.Write(headerbytes, 0, headerbytes.Length);
	
	        FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
	        byte[] buffer = new byte[4096];
	        int bytesRead = 0;
	        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0) {
	            rs.Write(buffer, 0, bytesRead);
	        }
	        fileStream.Close();
	
	        byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
	        rs.Write(trailer, 0, trailer.Length);
	        rs.Close();

	        WebResponse wresp = null;
	        bool ok = true;
	        try 
	        {
	            wresp = wr.GetResponse();
	            Stream stream2 = wresp.GetResponseStream();
	            StreamReader reader2 = new StreamReader(stream2);
	            String rep = reader2.ReadToEnd();
	        } 
	        catch(Exception) 
	        {
	            if(wresp != null) 
	            {
	                wresp.Close();
	                wresp = null;
	                ok = false;
	            }
	        } 
	        finally
	        {
	            wr = null;
	        }
	        return ok;
    	}
		
		/// <summary>
		/// Post an image to an existing log
		/// </summary>
		/// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
		/// <param name="cookieJar">Authentication cookies</param>
		/// <param name="logpage">content of log page</param>
		/// <param name="imagefile">complete filename to image (JPG only !)</param>
		/// <param name="txtname">image title</param>
		/// <param name="txtdesc">image description</param>
		/// <returns>true if successful</returns>
		static public bool PostImage(MainWindow daddy, CookieContainer cookieJar, String logpage, String imagefile, String txtname, String txtdesc)
		{
			// On récupère le logid
			String logid = MyTools.GetSnippetFromText("ctl00_ContentBody_LogBookPanel1_lnkUpload", "</a>", logpage);
			logid = MyTools.GetSnippetFromText("upload.aspx?LID=", "\">", logpage);
			String url = "https://www.geocaching.com/seek/upload.aspx?LID=" + logid;
			
			// On charge la nouvelle page avec les nouveaux viewstate
			HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
            objRequest.Proxy = daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
            objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
            using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
            {
                logpage = responseStream.ReadToEnd();
                responseStream.Close();
            }
            
			// On récupère le VIEWSTATE
            String __VIEWSTATEFIELDCOUNT = "";
            String[] __VIEWSTATE = null;
            String __VIEWSTATEGENERATOR = "";
            MainWindow.GetViewState(logpage, ref __VIEWSTATEFIELDCOUNT, ref __VIEWSTATE, ref __VIEWSTATEGENERATOR);
               
			// Préparation des données du POST
            NameValueCollection post_values = new NameValueCollection();
		    post_values.Add("__EVENTTARGET", "");
            post_values.Add("__EVENTARGUMENT", "");
            post_values.Add("__LASTFOCUS", "");
            post_values.Add("ctl00$ContentBody$ImageUploadControl1$uxFileCaption", txtname);
            post_values.Add("ctl00$ContentBody$ImageUploadControl1$uxFileDesc", txtdesc);
            post_values.Add("ctl00$ContentBody$ImageUploadControl1$uxUpload", "Upload");
            // Les VIEWSTATE
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
                
            return HttpUploadFile(url, "ctl00$ContentBody$ImageUploadControl1$uxFileUpload", imagefile, "image/jpeg", post_values, daddy.GetProxy(), cookieJar);
		}
			
		/// <summary>
		/// Post images to an existing log
		/// </summary>
		/// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
		/// <param name="cookieJar">Authentication cookies</param>
		/// <param name="logpage">content of log page</param>
		/// <param name="images">images to attach to the log (key is complete filename, value is (title, description))</param>
		/// <returns>true if successful</returns>
		static public bool PostImages(MainWindow daddy, CookieContainer cookieJar, String logpage, Dictionary<String, KeyValuePair<String, String>> images)
		{
			if (images != null)
			{
				foreach(KeyValuePair<String, KeyValuePair<String, String>> pair in images)
				{
					String imagefile = pair.Key;
					String txtname = pair.Value.Key;
					String txtdesc = pair.Value.Value;
					if (File.Exists(imagefile))
					{
						if (!PostImage(daddy, cookieJar, logpage, imagefile, txtname, txtdesc))
							return false;
					}
				}
				return true;
			}
			else
				return false;
		}
		
        /// <summary>
        /// Log a cache on geocaching website
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        /// <param name="cache">Geocache to log</param>
        /// <param name="date">Log date</param>
        /// <param name="logtext">Log text</param>
        /// <param name="bFav">True if cache shall be favorited</param>
        /// <param name="iLogType">Log type</param>
        /// <param name="tbs">List of TBs to drop of visit</param>
        /// <param name="bDiscardAlreadyDroppedTBs">If true, TBs marked "Dropped Off will not be dropped off again</param>
        /// <param name="runcount">Cache number within a batch (used for %runcount)</param>
        /// <param name="runtotal">Number of caches that will be logged during a batch (used for %runtotal)</param>
        /// <param name="images">images to attach to the log (key is complete filename, value is (title, description))</param>
        /// <returns>True if log is successful</returns>
        public static bool LogCache(MainWindow daddy, Geocache cache, DateTime date, String logtext, bool bFav, int iLogType,
                                    List<KeyValuePair<string, string>> tbs, bool bDiscardAlreadyDroppedTBs,
                                    int runcount, int runtotal,
                                   	Dictionary<String, KeyValuePair<String, String>> images)
        {
        	
            
        	//String debug = CreateDebugInfo(daddy, cache, date, logtext, bFav, iLogType, tbs, images, bDiscardAlreadyDroppedTBs, runcount, runtotal);
        	//MessageBox.Show(debug);
        	//Thread.Sleep(2000);
        	//return true;
        	//#warning GCLogHMI.LogCache NOT IMPLEMENTED (DEBUG MODE)
            try
            {
            	
            	daddy.Log("==> LogCache " + cache._Code + "  " + cache._Name + "  " + date.ToString() + " " + iLogType.ToString());
            	String dbgmsg = "";
                CookieContainer cookieJar = daddy.CheckGCAccount(true, false);

                String url;
                HttpWebRequest objRequest;
                HttpWebResponse objResponse;
                String response;
                
                // Recupère les infos de la cache
                String cacheid = cache._CacheId;
                int minid =  daddy._iMinCacheIdMGM;
                if (String.Compare(cacheid, minid.ToString()) >= 0)
            		cacheid = ""; // C'est une cache générée, il faut appeler le web
                
                // Si cacheid est null, alors on doit parser la page une première fois...
                if (cacheid == "")
                {
                	// Récupération d'une page de geocache
	                // ***********************************
	                url = cache._Url;
	                objRequest = (HttpWebRequest)WebRequest.Create(url);
	                objRequest.Proxy = daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
	                objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
	                objResponse = (HttpWebResponse)objRequest.GetResponse();
	                using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
	                {
	                    response = responseStream.ReadToEnd();
	                    responseStream.Close();
	                }
	
	                // On cherche l'id de la cache
	                // ***************************
	                cacheid = MyTools.GetSnippetFromText("/seek/log.aspx?ID=", "&lcn=1", response);
                }
				
                // Récupération des VIEWSTATE
                // **************************
                url = "https://www.geocaching.com/seek/log.aspx?ID=" + cacheid;
                
                objRequest = (HttpWebRequest)WebRequest.Create(url);
                objRequest.Proxy = daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
                objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
                objResponse = (HttpWebResponse)objRequest.GetResponse();
                using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
                {
                    response = responseStream.ReadToEnd();
                    responseStream.Close();
                }

                String __VIEWSTATEFIELDCOUNT = "";
                String[] __VIEWSTATE = null;
                String __VIEWSTATEGENERATOR = "";
                MainWindow.GetViewState(response, ref __VIEWSTATEFIELDCOUNT, ref __VIEWSTATE, ref __VIEWSTATEGENERATOR);

                // Petit bonus : le nombre de caches déjà trouvées
                String found = MyTools.GetSnippetFromText("<span class=\"cache-count\">", " ", response);
                found = found.Replace(",", "");
                found = found.Replace(".", "");
                int iFound = 0;
                Int32.TryParse(found, out iFound);

                // ATTENTION !!!!
                // Le format de la date est spécifique à l'utilisateur
                // non mais quelle idée à la con !
                // ctl00_ContentBody_LogBookPanel1_uxDateFormatHint">(
                String date_format = MyTools.GetSnippetFromText("ctl00_ContentBody_LogBookPanel1_uxDateFormatHint\">(", ")", response);
                String dateforgc = "";
                if (date_format != "")
                	dateforgc = date.ToString(date_format,System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
                else
                	dateforgc = date.ToString("dd/MMM/yyyy",System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
                
                // Préparation des données du POST
                Dictionary<String, String> post_values = new Dictionary<String, String>();
                post_values.Add("__EVENTTARGET", "");
                post_values.Add("__EVENTARGUMENT", "");
                post_values.Add("__LASTFOCUS", "");
                post_values.Add("ctl00$ContentBody$LogBookPanel1$ddLogType", iLogType.ToString());
                /*
                post_values.Add("ctl00$ContentBody$LogBookPanel1$uxDateVisited", "02/01/2016");
                post_values.Add("ctl00$ContentBody$LogBookPanel1$uxDateVisited$Month", "01");
                post_values.Add("ctl00$ContentBody$LogBookPanel1$uxDateVisited$Day", "02");
                post_values.Add("ctl00$ContentBody$LogBookPanel1$uxDateVisited$Year", "2016");
                post_values.Add("ctl00$ContentBody$LogBookPanel1$DateTimeLogged", "02/01/2016");
                post_values.Add("ctl00$ContentBody$LogBookPanel1$DateTimeLogged$Month", "01");
                post_values.Add("ctl00$ContentBody$LogBookPanel1$DateTimeLogged$Day", "02");
                post_values.Add("ctl00$ContentBody$LogBookPanel1$DateTimeLogged$Year", "2016");
                 */
                post_values.Add("ctl00$ContentBody$LogBookPanel1$uxDateVisited", dateforgc);
                post_values.Add("ctl00$ContentBody$LogBookPanel1$uxDateVisited$Month", date.Month.ToString());
                post_values.Add("ctl00$ContentBody$LogBookPanel1$uxDateVisited$Day", date.Day.ToString());
                post_values.Add("ctl00$ContentBody$LogBookPanel1$uxDateVisited$Year", date.Year.ToString());
                post_values.Add("ctl00$ContentBody$LogBookPanel1$DateTimeLogged", dateforgc);
                post_values.Add("ctl00$ContentBody$LogBookPanel1$DateTimeLogged$Month", date.Month.ToString());
                post_values.Add("ctl00$ContentBody$LogBookPanel1$DateTimeLogged$Day", date.Day.ToString());
                post_values.Add("ctl00$ContentBody$LogBookPanel1$DateTimeLogged$Year", date.Year.ToString());
                post_values.Add("ctl00$ContentBody$LogBookPanel1$LogButton", "Submit Log Entry");
                dbgmsg += "Log type: " + iLogType.ToString() + "\r\n";
                dbgmsg += "Log date: " + date.ToShortDateString() + "\r\n";

                // Utilisation des mots clés
                // %runcount - The cache count for this run of publish logs. This differs to %count as it only refers to the number for current session and not the cumulative number of finds for the user.
                // %runtotal - The total number of finds in this session. You can use this tag in combination with %runcount to indicate something like "This is find number %runcount of %runtotal for the day"
                // %count - Total cache find number for the user. GSAK uses the api to initially fetch your found count (if you want to override this number use %count=nnn) Each time this tag is used on a found log, this number is incremented by 1
                String slog = logtext.Trim();
                if ((iLogType == 10) || // attended
                    (iLogType == 11) || // webcam photo
                    (iLogType == 2)) // Found it
                {
                    // on augmente de 1 le nombre de caches trouvées
                    // et le nombre de caches du run
                    iFound++;
                }
                slog = slog.Replace("%count", iFound.ToString());
                if (runcount > 0)
                	slog = slog.Replace("%runcount", runcount.ToString());
                else
                	slog = slog.Replace("%runcount", "");
                if (runtotal > 0)
                	slog = slog.Replace("%runtotal", runtotal.ToString());
                else
                	slog = slog.Replace("%runtotal", "");
                slog = slog.Replace("%owner", cache._Owner);
                //slog = slog.Replace("\r\n", "  ");
                //daddy.MSG(slog);
                post_values.Add("ctl00$ContentBody$LogBookPanel1$uxLogInfo", slog);
                dbgmsg += "Log comment: " + slog + "\r\n";

                post_values.Add("ctl00$ContentBody$LogBookPanel1$btnSubmitLog", "Submit Log Entry");
                post_values.Add("ctl00$ContentBody$LogBookPanel1$uxLogCreationSource", "Old");
                post_values.Add("ctl00$ContentBody$uxVistOtherListingGC", "");
                if (bFav)
                {
                    post_values.Add("ctl00$ContentBody$LogBookPanel1$chkAddToFavorites", "on");
                    dbgmsg += "Add to favorite: Yes\r\n";
                }

                // Les Tbs
                String sActionForTBs = "";
                foreach (KeyValuePair<String, String> pair in tbs)
                {
                    if (bDiscardAlreadyDroppedTBs && pair.Value.Contains("_DroppedOff"))
                    {
                        // No double drop !
                    }
                    else
                    {
                        sActionForTBs += pair.Value + ",";
                        dbgmsg += "TB action: " + pair.Value + "\r\n";
                    }
                }
                if (sActionForTBs != "")
                {
                    post_values.Add("ctl00$ContentBody$LogBookPanel1$uxTrackables$hdnSelectedActions", sActionForTBs);
                    post_values.Add("ctl00$ContentBody$LogBookPanel1$uxTrackables$hdnCurrentFilter", "");
                }

                // Les VIEWSTATE
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
                 
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentLength = post_string.Length;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Proxy = daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
                request.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification

                String post_response;
                // on envoit les POST data dans un stream (écriture)
                StreamWriter myWriter = null;
                myWriter = new StreamWriter(request.GetRequestStream());
                myWriter.Write(post_string);
                myWriter.Close();

                // lecture du stream de réponse et conversion en chaine
                objResponse = (HttpWebResponse)request.GetResponse();
                using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
                {
                    post_response = responseStream.ReadToEnd();
                    responseStream.Close();
                }

                // Pour Need maintenance, une confirmation est nécessaire
                // ******************************************************
                if (iLogType == 45)
                {
                    // On remet à jour les viewstates
                    __VIEWSTATEFIELDCOUNT = "";
                    __VIEWSTATE = null;
                    __VIEWSTATEGENERATOR = "";
                    MainWindow.GetViewState(post_response, ref __VIEWSTATEFIELDCOUNT, ref __VIEWSTATE, ref __VIEWSTATEGENERATOR);

                    // On créé un nouveau post
                    post_values = new Dictionary<String, String>();
                    post_values.Add("__EVENTTARGET", "");
                    post_values.Add("__EVENTARGUMENT", "");
                    post_values.Add("__LASTFOCUS", "");
                    post_values.Add("ctl00$ContentBody$LogBookPanel1$btnConfirm", "Yes");
                    post_values.Add("ctl00$ContentBody$LogBookPanel1$uxLogInfo", slog);

                    // Les TBs
                    if (sActionForTBs != "")
                    {
                        post_values.Add("ctl00$ContentBody$LogBookPanel1$uxTrackables$hdnSelectedActions", sActionForTBs);
                        post_values.Add("ctl00$ContentBody$LogBookPanel1$uxTrackables$hdnCurrentFilter", "");
                    }

                    // Les VIEWSTATE
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
                    post_string = "";
                    foreach (KeyValuePair<String, String> post_value in post_values)
                    {
                        post_string += post_value.Key + "=" + System.Web.HttpUtility.UrlEncode(post_value.Value) + "&";
                    }
                    post_string = post_string.TrimEnd('&');

                    // Emission de la requête du POST de confirmation
                    request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "POST";
                    request.ContentLength = post_string.Length;
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.Proxy = daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
                    request.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification

                    // on envoit les POST data dans un stream (écriture)
                    myWriter = null;
                    myWriter = new StreamWriter(request.GetRequestStream());
                    myWriter.Write(post_string);
                    myWriter.Close();

                    // lecture du stream de réponse et conversion en chaine
                    objResponse = (HttpWebResponse)request.GetResponse();
                    post_response = "";
                    using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
                    {
                        post_response = responseStream.ReadToEnd();
                        responseStream.Close();
                    }

                    // Et enfin on peut faire un test normal de la réponse
                    PostImages(daddy, cookieJar, post_response, images);
                    return true;
                    
                    // Si la réponse contient ctl00_ContentBody_LogBookPanel1_LogImage alors c'est réussi, sinon ça a raté
                    //#warning DESACTIVATION DE LA VERIFICATION DU LOG POSTE (MAINTENANCE)
                    /*
                    if (post_response.Contains("ctl00_ContentBody_LogBookPanel1_LogImage"))
                    {
                        return true;
                    }
                    else
                    {
                        //daddy._cacheDetail.LoadPageText("Log status", post_response, true);
                        return false;
                    }*/
                }
                else
                {
                    // Log standard sans besoin de confirmation
                    // ****************************************
                    // Bon je ne comprends rien, donc je retourne OK tout le temps
                    PostImages(daddy, cookieJar, post_response, images);
                    return true;
                    
                    // Si la réponse contient ctl00_ContentBody_LogBookPanel1_LogImage alors c'est réussi, sinon ça a raté
                    //#warning DESACTIVATION DE LA VERIFICATION DU LOG POSTE
                    /*
                    if (post_response.Contains("ctl00_ContentBody_LogBookPanel1_LogImage"))
                    {
                    	if (iLogType == 2)
                        {
                        	// C'était un found it
                        	String owner = ConfigurationManager.AppSettings["owner"];
                        	daddy.DeclareFoundCache(owner, cache);
                        	
                        	// On met à jour le status des caches
		                    daddy._cacheStatus.SaveCacheStatus();
		                    // On réaffiche tout proprement
		            		// Better way to do that : only recreate for modified caches
		            		daddy.RecreateVisualElements(cache, true);
                        }
	            		
	            		return true;
                    }
                    else
                    {
                        //daddy._cacheDetail.LoadPageText("Log status", post_response, true);
                        return false;
                    }*/
                }
            }
            catch (Exception ex)
            {
            	daddy.Log(MainWindow.GetException("LogCache: " + cache._Code, ex));
                return false;
            }
        }

        private List<KeyValuePair<string, string>> HandleTBs()
        {
            List<KeyValuePair<string, string>> tbs = new List<KeyValuePair<string, string>>();

            int i = 1;
            foreach (ListViewItem item in lvTBsloghmi.Items)
            {
                String id = (string)(item.Tag);
                String num = i.ToString();
                if (i < 10)
                    num = "0" + num;
                String postentry = "ctl00$ContentBody$LogBookPanel1$uxTrackables$repTravelBugs$ctl" + num + "$ddlAction";
                String action = "";
                if ((int)(item.SubItems[2].Tag) == 1)
                {
                    action = id + "_DroppedOff";
                    KeyValuePair<string, string> pair = new KeyValuePair<string, string>(postentry, action);
                    tbs.Add(pair);
                }
                else if ((int)(item.SubItems[3].Tag) == 1)
                {
                    action = id + "_Visited";
                    KeyValuePair<string, string> pair = new KeyValuePair<string, string>(postentry, action);
                    tbs.Add(pair);
                }
                i++;
            }
            return tbs;
        }

        /// <summary>
        /// Export list images for log
        /// </summary>
        /// <returns>images to attach to the log (key is complete filename, value is (title, description))</returns>
        public Dictionary<String, KeyValuePair<String, String>> ExportImages()
        {
        	Dictionary<String, KeyValuePair<String, String>> imgs = new Dictionary<string, KeyValuePair<string, string>>();
        	
        	foreach(ListViewItem item in lvImgsLogHMI.Items)
        	{
        		String title = item.SubItems[0].Text;
        		String desc = item.SubItems[1].Text;
        		String path = item.SubItems[2].Text;
        		if (imgs.ContainsKey(path) == false)
        		{
        			KeyValuePair<string, string> kv = new KeyValuePair<string, string>(title, desc);
        			imgs.Add(path, kv);
        		}
        	}
        	return imgs;
        }
        
        /// <summary>
        /// Import list images for log
        /// </summary>
        /// <param name="images">images to attach to the log (key is complete filename, value is (title, description))</param>
        public void ImportImages(Dictionary<String, KeyValuePair<String, String>> images)
        {
        	lvImgsLogHMI.Items.Clear();
        	if (images == null)
        		return;
        	
        	foreach(KeyValuePair<String, KeyValuePair<String, String>> pair in images)
        	{
        		ListViewItem item = lvImgsLogHMI.Items.Add(pair.Value.Key);
        		item.SubItems.Add(pair.Value.Value);
        		item.SubItems.Add(pair.Key);
        	}
        }
        
        /// <summary>
        /// Export list of visited / dropped off TBs
        /// </summary>
        /// <returns>key : TB code, id; value : 1 dropped, 2 visited </returns>
        public Dictionary< KeyValuePair<string, string>, int> ExportTBs()
        {
        	// key : TB name, id
        	// value : 1 dropped, 2 visited
            Dictionary< KeyValuePair<string, string>, int> tbs = new Dictionary< KeyValuePair<string, string>, int>();

            foreach (ListViewItem item in lvTBsloghmi.Items)
            {
                String id = (string)(item.Tag);
                if ((int)(item.SubItems[2].Tag) == 1)
                {
                	// Dropped
                    KeyValuePair<string, string> pair = new KeyValuePair<string, string>(item.SubItems[0].Text, id);
                    tbs.Add(pair, 1);
                }
                else if ((int)(item.SubItems[3].Tag) == 1)
                {
                	// visited
                    KeyValuePair<string, string> pair = new KeyValuePair<string, string>(item.SubItems[0].Text, id);
                    tbs.Add(pair, 2);
                }
            }
            return tbs;
        }
        

        /// <summary>
        /// Import list of visited / dropped off TBs and updated HMI
        /// </summary>
        /// <param name="tbs">key : TB code, id; value : 1 dropped, 2 visited </param>
        public void ImportTBs(Dictionary< KeyValuePair<string, string>, int> tbs)
        {
            _importedTBs = tbs;
            ApplyImportedTBsOnList();
        }
        
        private void ApplyImportedTBsOnList()
        {
            if (_importedTBs == null)
                return;
            foreach (KeyValuePair<KeyValuePair<string, string>, int> pair in _importedTBs)
            {
                String idfromlist = pair.Key.Value;
                foreach (ListViewItem item in lvTBsloghmi.Items)
                {
                    String id = (string)(item.Tag);
                    if (id == idfromlist)
                    {
                        // On a trouvé un TB à importer
                        if (pair.Value == 1)
                        {
                            // Dropped
                            item.SubItems[2].Tag = 1;
                            item.SubItems[3].Tag = 0;
                            item.SubItems[2].Text = _daddy.GetTranslator().GetString("LblTBDroppedOff");
                            item.SubItems[3].Text = _daddy.GetTranslator().GetString("BtnNo");
                            ColorItemTB(item);
                        }
                        else if (pair.Value == 2)
                        {
                            // Visited
                            item.SubItems[2].Tag = 0;
                            item.SubItems[3].Tag = 1;
                            item.SubItems[2].Text = _daddy.GetTranslator().GetString("BtnNo");
                            item.SubItems[3].Text = _daddy.GetTranslator().GetString("LblTBVisited");
                            ColorItemTB(item);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Submit a log
        /// </summary>
        /// <param name="offerRetryOnError">If true, retry will be offered on error on log lenght</param>
        /// <returns>SubmitResult status</returns>
        private SubmitResult SubmitLog(bool offerRetryOnError)
        {
        	if (txtCommentsloghmi.Text.Length <= 5) // MPLC, etc...
            {
        		if (!offerRetryOnError)
        		{
	                _daddy.MsgActionError(this, _daddy.GetTranslator().GetString("lblemptycomment"));
	        		return SubmitResult.Error;
        		}
        		else
        		{
        			DialogResult dialogResult = MessageBox.Show(
        				(_daddy.GetTranslator().GetString("lblemptycomment") + "#" + _daddy.GetTranslator().GetString("AskRetryIncorrectLogLength")).Replace("#","\r\n"),
	            		_daddy.GetTranslator().GetString("AskRetryIncorrectLogLengthTitle"),
	            		MessageBoxButtons.YesNo, MessageBoxIcon.Question);
	        		if (dialogResult == DialogResult.Yes)
						return  SubmitResult.Retry;
	        		else
	        			return SubmitResult.Error;
        		}
            }
            else if (txtCommentsloghmi.Text.Length >= 4000)
            {
                if (!offerRetryOnError)
        		{
                	_daddy.MsgActionError(this, _daddy.GetTranslator().GetString("ErrLogToLong"));
            		return SubmitResult.Error;
                }
                else
        		{
        			DialogResult dialogResult = MessageBox.Show(
        				(_daddy.GetTranslator().GetString("ErrLogToLong") + "#" + _daddy.GetTranslator().GetString("AskRetryIncorrectLogLength")).Replace("#","\r\n"),
	            		_daddy.GetTranslator().GetString("AskRetryIncorrectLogLengthTitle"),
	            		MessageBoxButtons.YesNo, MessageBoxIcon.Question);
	        		if (dialogResult == DialogResult.Yes)
						return  SubmitResult.Retry;
	        		else
	        			return SubmitResult.Error;
        		}
            }
            else
            {
                List<KeyValuePair<string, string>> tbs = HandleTBs();
                _TBActionList = tbs;

                if (_templateType == GCLogHMI.TemplateType.None)
                {
                    // Normal log
                    bool bMoreThanOneCache = (_caches.Count > 1);
                    bool bNotQuiet = (!_bSilent || bMoreThanOneCache);
                    //MessageBox.Show(bNotQuiet.ToString());
                    if (bNotQuiet)
                    {
                        _daddy._ThreadProgressBarTitle = GetMainHMITextFromTemplateType();
                        _daddy.CreateThreadProgressBarEnh();

                        // Wait for the creation of the bar
                        while (_daddy._ThreadProgressBar == null)
                        {
                            Thread.Sleep(10);
                            Application.DoEvents();
                        }
                        _daddy._ThreadProgressBar.progressBar1.Maximum = _caches.Count;
                    }

                    bool bOk = true;
                    String msg = "";
                    ComboItem item = (ComboItem)(cbLogTypeloghmi.SelectedItem);
                    //List<KeyValuePair<string, string>> tbs = HandleTBs();
                    //_TBActionList = tbs;
                    bool bDiscardAlreadyDroppedTBs = false;
                    int runcount = 1;
                    if (_overrideRunCount != -1)
                    	runcount = _overrideRunCount;
                    int runtotal = _caches.Count;
                    if (_overrideRunTotal != -1)
                    	runtotal = _overrideRunTotal;
                    String owner = ConfigurationManager.AppSettings["owner"];
                    foreach (Geocache cache in _caches)
                    {
                        if (bNotQuiet)
                            _daddy._ThreadProgressBar.lblWait.Text = cache._Code + " " + cache._Name;

                        if (bNotQuiet && _daddy._ThreadProgressBar._bAbort)
                        {
                            msg += cache._Code + " " + cache._Name + " -> " + _daddy.GetTranslator().GetString("AbortLoggingCache") + "\r\n";
                            bOk = false;
                        }
                        else
                        {
                        	_iTypeReallyLogged = (int)(item.Value);
                        	if (GCLogHMI.LogCache(_daddy, cache, dtPickerloghmi.Value, txtCommentsloghmi.Text, cbFavsloghmi.Checked, (int)(item.Value), tbs, bDiscardAlreadyDroppedTBs, runcount, runtotal, ExportImages()))
                            {
                                // At least one success, so no double drop of TBs !
                                bDiscardAlreadyDroppedTBs = true;
                            }
                            else
                            {
                            	msg += cache._Code + " " + cache._Name + " -> " + _daddy.GetTranslator().GetString("ErrLoggingCache") + "\r\n";
                                bOk = false;
                            }
                        }
                        runcount++;
                        if (_overrideRunCount != -1)
                        	_overrideRunCount = runcount;
                        
                        if (bNotQuiet)
                            _daddy._ThreadProgressBar.Step();
                    }
                    if (bNotQuiet)
                        _daddy.KillThreadProgressBarEnh();
            
                    if (bOk)
                    {
                    	if (!_bSilent)
                        {
                            _daddy.MsgActionOk(this, _daddy.GetTranslator().GetString("SuccessLoggingCache"));
                        }
                        this.DialogResult = DialogResult.OK;
                        CloseForm();
                        return SubmitResult.Success;
                    }
                    else
                    {
                    	_daddy.MsgActionError(this, msg);
                    	return SubmitResult.Error;
                    }
                }
                else
                {
                    // on est dans un log de template simplement.
                    // on va juste récupérer le texte du log
                    ComboItem item = (ComboItem)(cbLogTypeloghmi.SelectedItem);
                    if (item != null)
                    	_iTypeReallyLogged = (int)(item.Value);
                    
                    this.DialogResult = DialogResult.OK;
                    CloseForm();
                    return SubmitResult.Success;
                }
            }
        }
        
        private void CloseForm()
        {
        	this.Close();
        }
        
        private void btnSubmit_Click(object sender, EventArgs e)
        {
        	if (_templateType == GCLogHMI.TemplateType.TB)
        		this.DialogResult = DialogResult.OK;
        	else if (_templateType == GCLogHMI.TemplateType.TextOnly)
        		this.DialogResult = DialogResult.OK;
        	else
        	{
        		SubmitResult status = SubmitLog(true);
	        	if (status == SubmitResult.Success)
	        	{
	        		this.DialogResult = DialogResult.OK;
	        	}
	        	else if (status == SubmitResult.Error)
	        	{
	        		this.DialogResult = DialogResult.Cancel;
	        	}
	        	else if (status == SubmitResult.Retry)
	        	{
	        		// On laisse ouvert
	        	}
        	}
        }

        private void txtComments_TextChanged(object sender, EventArgs e)
        {
        	UpdateLogPreviewMarkdown();
        	
        	// Surtout redonner le focus
        	lblComments.Focus();
        	txtCommentsloghmi.Focus();
        }
      
        
        /// <summary>
        /// Update log preview in Markdown
        /// </summary>
        public void UpdateLogPreviewMarkdown()
        {
        	// Attention, les \r\n comptent pour 2 : <double espace> car je les remplace dans l'envoi
            // Pour faire ça on va splitter
            int offset_line_feed = (txtCommentsloghmi.Text.Split('\n').Count() - 1);
            if (offset_line_feed < 0)
                offset_line_feed = 0;
            lblloglength.Text = "(" + (offset_line_feed + txtCommentsloghmi.Text.Length).ToString() + "/4000)";
            
            try
            {
            	String slog = _Markdown.Transform(txtCommentsloghmi.Text);
            	if ((_templateType != GCLogHMI.TemplateType.Simple) && (_templateType != GCLogHMI.TemplateType.TextOnly))
            	{
	                ComboItem item = (ComboItem)(cbLogTypeloghmi.SelectedItem);
	                int iLogType = (int)(item.Value);
	                int iFound = _iFound;
	                if ((iLogType == 10) || // attended
                        (iLogType == 11) || // webcam photo
                        (iLogType == 2)) // Found it
	                {
	                    // on augmente de 1 le nombre de caches trouvées
	                    // et le nombre de caches du run
	                    iFound++;
	                }
	                slog = slog.Replace("%count", iFound.ToString());
	                
	                int runcount = 1;
	                if (_overrideRunCount != -1)
	                	runcount = _overrideRunCount;       
	                if (runcount > 0)
		            	slog = slog.Replace("%runcount", runcount.ToString());
	                else
	                	slog = slog.Replace("%runcount", "");
		            
		            
		            int runtotal = _caches.Count;
		            if (_overrideRunTotal != -1)
		            	runtotal = _overrideRunTotal;
		            if (runtotal > 0)
		            	slog = slog.Replace("%runtotal", runtotal.ToString());
		            else
		            	slog = slog.Replace("%runtotal", "");
		            
	                if (_caches.Count != 0)
	                    slog = slog.Replace("%owner", _caches[0]._Owner);
            	}
            	
            	// On remplace les smileys
                foreach(KeyValuePair<String, String> pair in _dicoSmileys)
                {
                	slog = slog.Replace(pair.Key, "<IMG SRC=\"" + pair.Value + "\" alt=\"" + pair.Key + "\">");
                }
                
                _logdisplayhtml.DocumentText = slog;
                
                
            }
            catch(Exception)
            {
            	_logdisplayhtml.DocumentText = "Error rendering log!";
            }
        }
        
        private String GetCacheTextInfo(Geocache geo)
        {
        	String s = geo._Code;
    		if (geo._Name != "")
    		{
        		s +=  " " +
                    geo._Name + " (" +
                    geo._Owner + ") - " +
                    geo._Type + " [" +
                    geo._Container + "] + " +
                    geo._D.ToString() + "/" +
                    geo._T.ToString();
    		}
    		return s;
        }
        
        private void btnCacheInfo_Click(object sender, EventArgs e)
        {
            if (_caches.Count != 1)
            {
                String msg = "";
                foreach (Geocache cache in _caches)
                {
                    msg += GetCacheTextInfo(cache) + "\r\n";
                }
                _daddy.MsgActionOk(this, msg);
            }
            else
            {
                // On affiche le détail de la cache
                _daddy._cacheDetail.LoadPageCache(_caches[0], _daddy._bUseKm, null, false, false);
            }
        }

        private void ColorItemTB(ListViewItem item)
        {
            int i = (int)(item.SubItems[2].Tag);
            int j = (int)(item.SubItems[3].Tag);
            if ((i == 1) || (j == 1))
                item.BackColor = Color.LightGreen;
            else
                item.BackColor = Color.White;
        }

        private void btnDroppedAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvTBsloghmi.Items)
            {
                ListViewItem.ListViewSubItem sitem = item.SubItems[2];
                sitem.Tag = 1;
                sitem.Text = _daddy.GetTranslator().GetString("LblTBDroppedOff");
                
                sitem = item.SubItems[3];
                sitem.Tag = 0;
                sitem.Text = _daddy.GetTranslator().GetString("BtnNo");

                ColorItemTB(item);
            }
        }

        private void btnVisitAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvTBsloghmi.Items)
            {
                ListViewItem.ListViewSubItem sitem = item.SubItems[2];
                sitem.Tag = 0;
                sitem.Text = _daddy.GetTranslator().GetString("BtnNo");

                sitem = item.SubItems[3];
                sitem.Tag = 1;
                sitem.Text = _daddy.GetTranslator().GetString("LblTBVisited");

                ColorItemTB(item);
            }
        }

        private void btnClearAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvTBsloghmi.Items)
            {
                ListViewItem.ListViewSubItem sitem = item.SubItems[2];
                sitem.Tag = 0;
                sitem.Text = _daddy.GetTranslator().GetString("BtnNo");

                sitem = item.SubItems[3];
                sitem.Tag = 0;
                sitem.Text = _daddy.GetTranslator().GetString("BtnNo");

                ColorItemTB(item);
            }
        }

        private void lvTBs_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem item = lvTBsloghmi.GetItemAt(e.X, e.Y);
            if (item != null)
            {
                ListViewHitTestInfo info = lvTBsloghmi.HitTest(e.X, e.Y);
                ListViewItem.ListViewSubItem sitem = info.SubItem;
                if (sitem != null)
                {
                    if (item.SubItems[2] == sitem)
                    {
                        // double click sur droppped off
                        if ((int)sitem.Tag == 0) // On était No avant
                        {
                            sitem.Tag = 1;
                            sitem.Text = _daddy.GetTranslator().GetString("LblTBDroppedOff");

                            // On pas le visited à No
                            sitem = item.SubItems[3];
                            sitem.Tag = 0;
                            sitem.Text = _daddy.GetTranslator().GetString("BtnNo");
                        }
                        else
                        {
                            // on passe simplement à No maintenant
                            // On ne change pas le visited
                            sitem.Tag = 0;
                            sitem.Text = _daddy.GetTranslator().GetString("BtnNo");
                        }
                    }
                    else if (item.SubItems[3] == sitem)
                    {
                        // double click sur visited off
                        if ((int)sitem.Tag == 0) // On était No avant
                        {
                            sitem.Tag = 1;
                            sitem.Text = _daddy.GetTranslator().GetString("LblTBVisited");

                            // On pas le dropped off à No
                            sitem = item.SubItems[2];
                            sitem.Tag = 0;
                            sitem.Text = _daddy.GetTranslator().GetString("BtnNo");
                        }
                        else
                        {
                            // on passe simplement à No maintenant
                            // On ne change pas le dropped off
                            sitem.Tag = 0;
                            sitem.Text = _daddy.GetTranslator().GetString("BtnNo");
                        }
                    }

                    ColorItemTB(item);
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            CloseForm();
        }

        private void buttonSmileys_Click(object sender, EventArgs e)
        {
            Button ctrl = sender as Button;
            var insertText = (String)(ctrl.Tag);
            var selectionIndex = txtCommentsloghmi.SelectionStart;
            txtCommentsloghmi.Text = txtCommentsloghmi.Text.Insert(selectionIndex, insertText);
            txtCommentsloghmi.SelectionStart = selectionIndex + insertText.Length;
            txtCommentsloghmi.Focus();
        }
		
        private void buttonMarkdown_Click(object sender, EventArgs e)
        {
            Button ctrl = sender as Button;
            var insertText = (String)(ctrl.Tag);
            if (insertText == "1. ")
            {
            	insertText = _iCurrentNumbering.ToString() + ". ";
            	_iCurrentNumbering++;
            }
            else
            	insertText = insertText.Replace("\\n","\r\n");
            
            var selectionIndex = txtCommentsloghmi.SelectionStart;
			var selectionText = txtCommentsloghmi.SelectedText;
			
			// Si le insertText contient text ou Heading, on va voir si une sélection existe
			// Si oui, alors on encadre le texte existant, sinon on insère simplement
			bool bSurround = false;
			if (insertText.Contains("text") || insertText.Contains("Heading"))
				bSurround = true;
			if (bSurround && (selectionText != ""))
			{
				// on encadre
				String newtext = selectionText;
				if (insertText.Contains("text"))
					newtext = insertText.Replace("text", selectionText);
				else if (insertText.Contains("Heading"))
					newtext = insertText.Replace("Heading", selectionText);
				
				txtCommentsloghmi.SelectedText = newtext;
				txtCommentsloghmi.SelectionStart = selectionIndex;
				txtCommentsloghmi.SelectionLength = newtext.Length;
			}
			else
			{
				// Sinon on insère simplement
            	txtCommentsloghmi.Text = txtCommentsloghmi.Text.Insert(selectionIndex, insertText);
            	txtCommentsloghmi.SelectionStart = selectionIndex + insertText.Length;
			}
            
            txtCommentsloghmi.Focus();
            
        }

        private void btnReloadTBloghmi_Click(object sender, EventArgs e)
        {
            PopulateListTBs(true);
        }
        
		void BtnFnAddImgClick(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Image (*.jpg)|*.jpg";
            openFileDialog1.Multiselect = false;
            openFileDialog1.Title = _daddy.GetTranslator().GetString("btnFnAddImg");

            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string[] filenames = openFileDialog1.FileNames;
                
                // on vérifie que l'image n'existe pas déjà
                bool found = false;
                foreach(ListViewItem item in lvImgsLogHMI.Items)
                {
                	if (item.SubItems[2].Text == filenames[0])
                		found = true;
                }
                
                if (found)
                {
                	_daddy.MsgActionError(this, _daddy.GetTranslator().GetString("LblErrExistingImage"));
                }
                else
                {
                	// On ajoute
                	// On demande une description et un titre
                	List<ParameterObject> lst = new List<ParameterObject>();
                    String nomdefault = "";
                    if ((_caches != null) && (_caches.Count == 1))
                        nomdefault = _caches[0]._Code;
                    lst.Add(new ParameterObject(ParameterObject.ParameterType.String, nomdefault, "title", _daddy.GetTranslator().GetString("FnColImgName")));
	                lst.Add(new ParameterObject(ParameterObject.ParameterType.String, "", "desc", _daddy.GetTranslator().GetString("FnColImgDesc")));
	
	                ParametersChanger changer = new ParametersChanger();
	                changer.Title = _daddy.GetTranslator().GetString("btnFnAddImg");
	                changer.BtnCancel = _daddy.GetTranslator().GetString("BtnCancel");
	                changer.BtnOK = _daddy.GetTranslator().GetString("BtnOk");
	                changer.ErrorFormater = _daddy.GetTranslator().GetString("ErrWrongParameter");
	                changer.ErrorTitle = _daddy.GetTranslator().GetString("Error");
	                changer.Parameters = lst;
	                changer.Font = this.Font;
	                changer.Icon = this.Icon;
	
	                if (changer.ShowDialog() == DialogResult.OK)
	                {
	                	String name = lst[0].Value;
                    	String desc = lst[1].Value;
                    	ListViewItem item = lvImgsLogHMI.Items.Add(name);
		        		item.SubItems.Add(desc);
		        		item.SubItems.Add(filenames[0]);
	                }
                }
            }
		}
		
		void BtnFnRemoveImgClick(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection collection = lvImgsLogHMI.SelectedItems;
            if ((collection != null) && (collection.Count != 0))
            {
                foreach (ListViewItem item in collection)
                {
                    lvImgsLogHMI.Items.Remove(item);
                }
            }
		}
		
		/// <summary>
		/// Define handlers called for next and previous caches
		/// </summary>
		/// <param name="previous">true if previous button available</param>
		/// <param name="next">true if next button available</param>
		/// <param name="handlerPrevious">handler to previous</param>
		/// <param name="handlerNext">handler to next</param>
		/// <param name="handlerSave">handler to save</param>
		/// <param name="pos">index of displayed item within list</param>
		public void DefinePreviousAndNextButtons(bool previous, bool next,  Action<GCLogHMI, int> handlerPrevious, Action<GCLogHMI, int> handlerNext, Action<GCLogHMI, int> handlerSave, int pos)
		{
			btnPrevloghmi.Visible = previous;
        	btnNextloghmi.Visible = next;
        	_iItemPosition = pos;
        	_handlerPrevious = handlerPrevious;
        	_handlerNext = handlerNext;
        	_handlerSave = handlerSave;
        	btnLogHMISaveCurrent.Visible = (_handlerSave != null);
        	
		}
		
		void BtnPrevloghmiClick(object sender, EventArgs e)
		{
			if ((_caches != null) && (_handlerPrevious != null))
        	{
	        	if (_caches.Count == 1)
	            {
	        		_handlerPrevious(this, _iItemPosition);
	        		txtCommentsloghmi.SelectionStart = 0;
	        		txtCommentsloghmi.SelectionLength = 0;
	        	}
			}
		}
		
		void BtnNextloghmiClick(object sender, EventArgs e)
		{
			if ((_caches != null) && (_handlerNext != null))
        	{
	        	if (_caches.Count == 1)
	            {
	        		_handlerNext(this, _iItemPosition);
	        		txtCommentsloghmi.SelectionStart = 0;
	        		txtCommentsloghmi.SelectionLength = 0;
	        	}
			}
		}
		void BtnLogHMISaveCurrentClick(object sender, EventArgs e)
		{
			if ((_caches != null) && (_handlerNext != null))
        	{
	        	if (_caches.Count == 1)
	            {
	        		_handlerSave(this, _iItemPosition);
	        	}
			}
		}
		
    }

    /// <summary>
    /// Stupid class to hold TB info
    /// </summary>
    public class TBInfo
    {
        /// <summary>
        /// TB code
        /// </summary>
        public String _code = "";

        /// <summary>
        /// TB id (from log page)
        /// </summary>
        public String _id = "";

        /// <summary>
        /// TB name
        /// </summary>
        public String _name = "";
    }
}
