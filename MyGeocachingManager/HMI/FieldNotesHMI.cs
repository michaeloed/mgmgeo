using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SpaceEyeTools.HMI;
using System.Net;
using SpaceEyeTools;
using MyGeocachingManager.Geocaching;
using System.Threading;
using System.Diagnostics;
using System.Configuration;

namespace MyGeocachingManager
{
    /// <summary>
    /// Form to handle field notes
    /// </summary>
    public partial class FieldNotesHMI : Form
    {
        MainWindow _daddy = null;
        ListViewColumnSorter lvwColumnSorter = null;
        
        // Les traductions
        String sfoundit = "";
        String sdnf = "";
        String sneedmaint = "";
        String sownermaint = "";
        String swritenote = "";
        String swillattend = "";
        String sattended = "";
        // BCR 20170822
        String swebcamphototaken = "";
        
        // Les constantes venant des field notes
        const String sfnfoundit = "Found it";
        const String sfndnf = "Didn't find it";
        const String sfnneedmaint = "Needs Maintenance";
        const String sfnownermaint = "Owner Maintenance";
        const String sfnwritenote = "Write note";
        const String sfnattended = "Attended";
        const String sfnwillattend = "Will Attend";
        const String sfnwebcamphototaken = "Webcam Photo Taken";
        
        // Pour le modèle de log
        String _template = "";
        LogExtraInfo _logExtraInfo = null;

        /// <summary>
        /// Context menu of a list
        /// </summary>
        public ContextMenuStrip _mnuContextMenu = null;
        
        /// <summary>
        /// La liste d'images pour les types de caches
        /// </summary>
        ImageList _imageList = new ImageList();
        
        /// <summary>
        /// Display context menu of a tab page
        /// </summary>
        /// <param name="pt">Point to display the menu</param>
        public void DisplayContextMenu(Point pt)
        {
            if (_mnuContextMenu != null)
            {
                _mnuContextMenu.Show(this, pt);
            }
        }
        
        /// <summary>
        /// Load field notes and display HMI if not visible
        /// </summary>
        /// <param name="daddy">Mainframe</param>
        /// <param name="fn">reference to existing FieldNotesHMI, can be null</param>
        public static void LoadFieldNotes(MainWindow daddy, FieldNotesHMI fn)
        {
        	try
        	{
	        	String fns = daddy.GetTranslator().GetString("LblFieldNote");
	        	OpenFileDialog openFileDialog1 = new OpenFileDialog();
	            openFileDialog1.Filter = "MGM & Garmin " + fns + " (*.mfn, *.txt)|*.mfn;*.txt|MGM " + fns + " (*.mfn)|*.mfn|Garmin " + fns + " (*.txt)|*.txt";
	            openFileDialog1.Multiselect = false;
	            openFileDialog1.Title = daddy.GetTranslator().GetString("DlgChoseGPX");
	
	            string filename = null;
	            DialogResult dr = openFileDialog1.ShowDialog();
	            if (dr == System.Windows.Forms.DialogResult.OK)
	            {
	                filename = openFileDialog1.FileNames[0];
	            }
	            if (fn == null)
                {
                	FieldNotesHMI fnHMI = new FieldNotesHMI(daddy, filename);
                	fnHMI.Show();
                }
                else
                {
                	fn.LoadFieldNotes(filename);
                }
	           
	        }
			catch (Exception exc)
            {
                
            	daddy.ShowException("", daddy.GetTranslator().GetString("FMenuLoadFN"), exc);
            }	
        }
        
        /// <summary>
        /// Load field notes and display HMI if not visible
        /// </summary>
        /// <param name="daddy">Mainframe</param>
        /// <param name="caches">list of caches to load</param>
        public static void LoadFieldNotesC(MainWindow daddy, List<Geocache> caches)
        {
        	try
        	{
            	FieldNotesHMI fnHMI = new FieldNotesHMI(daddy, caches);
            	fnHMI.Show();          
	        }
			catch (Exception exc)
            {
                
            	daddy.ShowException("", daddy.GetTranslator().GetString("FMenuLoadFN"), exc);
            }	
        }
        
        /// <summary>
        /// Determines a text file's encoding by analyzing its byte order mark (BOM).
        /// Defaults to ASCII when detection of the text file's endianness fails.
        /// </summary>
        /// <param name="filename">The text file to analyze.</param>
        /// <returns>The detected encoding.</returns>
        public static Encoding GetEncoding(string filename)
        {
            // Read the BOM
            var bom = new byte[4];
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            return Encoding.ASCII;
        }

        private void BaseConstructeur(MainWindow daddy)
        {
        	_daddy = daddy;
            InitializeComponent();
            
            lvwColumnSorter = new ListViewColumnSorter(this);
			lvCachesfn.ListViewItemSorter = lvwColumnSorter;
			lvCachesfn.ColumnClick += new ColumnClickEventHandler(this_ColumnClick);
			
            this.Font = _daddy.Font;
            this.Icon = _daddy.Icon;
            TranslateForm();

            sfoundit = _daddy.GetTranslator().GetString("LG_Found_it");
            sdnf = _daddy.GetTranslator().GetString("LG_Didnt_find_it");
            sneedmaint = _daddy.GetTranslator().GetString("LOG_Needs_Maintenance");
            sownermaint = _daddy.GetTranslator().GetString("LG_Owner_Maintenance");
            swritenote = _daddy.GetTranslator().GetString("LOG_Write_note");
            swillattend = _daddy.GetTranslator().GetString("LOG_Will_Attend");
            swebcamphototaken = _daddy.GetTranslator().GetString("LOG_Webcam_Photo_Taken");
            sattended = _daddy.GetTranslator().GetString("LOG_Attended");
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        /// <param name="filename">field notes file (only Garmin geocaches_visit.txt supported at the moment)</param>
        public FieldNotesHMI(MainWindow daddy, string filename)
        {
            
        	BaseConstructeur(daddy);
            LoadFieldNotes(filename);
            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        /// <param name="caches">selection of caches</param>
        public FieldNotesHMI(MainWindow daddy, List<Geocache> caches)
        {
            BaseConstructeur(daddy);
            LoadFieldNotes(caches);
            
        }
        
        private void this_ColumnClick(object sender, ColumnClickEventArgs e) 
        {
        	// Determine if clicked column is already the column that is being sorted.
			if ( e.Column == lvwColumnSorter.SortColumn )
			{
				// Reverse the current sort direction for this column.
				if (lvwColumnSorter.Order == SortOrder.Ascending)
				{
					lvwColumnSorter.Order = SortOrder.Descending;
				}
				else
				{
					lvwColumnSorter.Order = SortOrder.Ascending;
				}
			}
			else
			{
				// Set the column number that is to be sorted; default to ascending.
				lvwColumnSorter.SortColumn = e.Column;
				lvwColumnSorter.Order = SortOrder.Ascending;
			}
			
			// Perform the sort with these new sort options.
			lvCachesfn.Sort();
			
			//  Màj rcrt
			CalculateRCRT();
        }
        
        private void LoadFieldNotes(List<Geocache> caches)
        {
        	try
            {
        		lvCachesfn.Items.Clear();
        		lvCachesfn.SuspendLayout();
        		
        		if (caches == null)
        			return;
        		
        		int num = 1;
        		DateTime date = DateTime.Now;
        		foreach(Geocache geo in caches)
                {
                    // Num
                    ListViewItem item = lvCachesfn.Items.Add(num.ToString());
					num++;
                    item.UseItemStyleForSubItems = false;
                    
                    // Est-ce que la cache existe ?
                    item.Tag = geo; // La geocache est dans le tag de l'item de la listview
                    // on ajoute l'icone
                    item.ImageKey = geo._Type;
                   
                    // GCCode
                    ListViewItem.ListViewSubItem sitem = item.SubItems.Add(geo._Code);
                    
                    // Traité ?
                    sitem = item.SubItems.Add(_daddy.GetTranslator().GetString("BtnNo"));

                    // Cache name
                    sitem = item.SubItems.Add(geo._Name);

                    // Les D/T
                    String dt = "";
                    if (geo != null)
                    	dt = geo._D + "/" + geo._T;
                    sitem = item.SubItems.Add(dt);
                    
                    // RC/RT
                    sitem = item.SubItems.Add("?");
                    sitem.Tag = null;
                    
                    // date logged
                    sitem = item.SubItems.Add(date.ToString("yyyy-MM-dd@HH:mm"));//.ToShortDateString());
                    sitem.Tag = date;//.ToString("yyyy-MM-ddTHH:mm:ssZ");

                    // type of log
                    // BCR 20170822
                    if (geo != null)
                    {
                    	if (geo.IsEventType())
                    		sitem = item.SubItems.Add(sattended);
                    	else if (geo.IsWebcamType())
                    		sitem = item.SubItems.Add(swebcamphototaken);
                    	else
                    		sitem = item.SubItems.Add(sfoundit);
                    }
                    else
                    	sitem = item.SubItems.Add(sfoundit);

                    // texte du log
                    sitem = item.SubItems.Add("");
                    sitem.Tag = new LogExtraInfo();
                    
                    // note de terrain
                    sitem = item.SubItems.Add("");
                    sitem.Tag = false;

                    // Favorisé ?
                    sitem = item.SubItems.Add(_daddy.GetTranslator().GetString("BtnNo"));
                    
                    // Status TB
                    sitem = item.SubItems.Add(String.Format(_daddy.GetTranslator().GetString("LblTBStatusTemplate"), 0, 0));
                    
                    // Nb images
                    sitem = item.SubItems.Add("0");
                    
                    // log status
                    sitem = item.SubItems.Add("?");
                    sitem.Tag = null;
                }
        		ColorItems();
        		lvCachesfn.Refresh();
        		DisplaySelCount();
        		CalculateRCRT();
        		
            }
            catch (Exception exc)
            {
            	lvCachesfn.Refresh();
            	_daddy.ShowException("", _daddy.GetTranslator().GetString("FMenuLiveLoadFieldNotes"), exc);
            }	
        }
        
        private void LoadFieldNotes(string filename)
        {
        	try
            {
        		lvwColumnSorter.SortColumn = 0;
        		lvwColumnSorter.Order = SortOrder.None;
        		
        		lvCachesfn.Items.Clear();
        		lvCachesfn.SuspendLayout();
        		
        		if (String.IsNullOrEmpty(filename) || (!File.Exists(filename)))
        			return;
        		
        		bool bFnMGM = false;
        		if (filename.ToLower().EndsWith(".mfn"))
        			bFnMGM = true;
        		
        		int num = 1;
        		using (StreamReader r = new StreamReader(filename, System.Text.Encoding.GetEncoding("iso-8859-1")))//GetEncoding(filename)))
                {
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        List<String> items = line.Split(',').ToList();
                        if (items.Count < 4)
                            continue;
                        String code = items[0].Replace("\0","");
                        String datelogged = items[1].Replace("\0", "");
                        String logtype = items[2].Replace("\0", "");
                        DateTime date = MyTools.ParseDate(datelogged).ToUniversalTime();
                        // On supporte uniquement les 4 types suivant :
                        // Fount it
                        // Didn't find it
                        // Needs Maintenance
                        // Write note
                        if (IsSupportedFnLogType(logtype))
                        {
                            String comment = items[3].Replace("\0", "");
                            if (!bFnMGM && (items.Count > 4))
                            {
                                // Il y avait sûrement une virgule dans le commentaire de la cache
                                // On concatène tout ça
                                for (int i = 4; i < items.Count; i++)
                                {
                                    comment += "," + items[i].Replace("\0", "");
                                }
                            }

                            // Supprimer le " de debut et de fin du commentaire
                            if ((comment.Length >= 1) && (comment[0] == '"'))
                                comment = comment.Remove(0, 1);
                            if ((comment.Length >= 1) && (comment[comment.Length - 1] == '"'))
                                comment = comment.Remove(comment.Length - 1, 1);

                            String name = "";
                            Geocache geo = null;
                            
                            // Num
                            ListViewItem item = lvCachesfn.Items.Add(num.ToString());
							num++;
                            item.UseItemStyleForSubItems = false;
                            
                            // Est-ce que la cache existe ?
                            if (_daddy._caches.ContainsKey(code))
                            {
                                geo = _daddy._caches[code];
                                item.Tag = geo; // La geocache est dans le tag de l'item de la listview
                                name = geo._Name;
                                
                                // on ajoute l'icone
                                item.ImageKey = geo._Type;
                            }
                            else
                                item.Tag = null;

                            // GCCode
                            ListViewItem.ListViewSubItem sitem = item.SubItems.Add(code); // GC Code
                            
                            // Traité ?
                            sitem = item.SubItems.Add(_daddy.GetTranslator().GetString("BtnNo"));

                            // Cache name
                            sitem = item.SubItems.Add(name);

                            // Les D/T
                            String dt = "";
                            if (geo != null)
                            	dt = geo._D + "/" + geo._T;
                            sitem = item.SubItems.Add(dt);
                            
                            // RC/RT
                            sitem = item.SubItems.Add("?");
                            sitem.Tag = null;
                            
                            // date logged
                            sitem = item.SubItems.Add(date.ToString("yyyy-MM-dd@HH:mm"));//.ToShortDateString());
                            sitem.Tag = date; // datelogged;

                            // type of log
                            String slog = LogTypeGCToNice(logtype);
                            sitem = item.SubItems.Add(slog);

                            // texte du log
                            sitem = item.SubItems.Add("");
                            sitem.Tag = new LogExtraInfo();
                            
                            // note de terrain
                            comment = comment.Replace("&#44;",",");
                            sitem = item.SubItems.Add(comment);
                            sitem.Tag = false;

                            // Favorisé ?
                            sitem = item.SubItems.Add(_daddy.GetTranslator().GetString("BtnNo"));
                            
                            // Status TB
                            sitem = item.SubItems.Add(String.Format(_daddy.GetTranslator().GetString("LblTBStatusTemplate"), 0, 0));
                            
                            // Nb images
                            sitem = item.SubItems.Add("0");
                            
                            // log status
                            sitem = item.SubItems.Add("?");
                            sitem.Tag = null;
                            
                            // Les données extra
                            if (bFnMGM)
                            {
                            	// On doit avoir 3 champs de plus dans la liste (ou 4 si on a le status du log)
                            	if (items.Count >= 7)
                            	{
                            		// Le texte du log
                            		String logtext = items[4];
                            		logtext = logtext.Replace("&#44;", ",");
									logtext = logtext.Replace("&lf;", "\r\n");
									item.SubItems[_ID_LOGTEXT].Text = logtext;
                            		
									// Les infos extra
									LogExtraInfo infos = new LogExtraInfo();
									infos._dicoTBs = new Dictionary< KeyValuePair<string, string>, int>();
									infos._dicoImages = new Dictionary<string, KeyValuePair<string, string>>();
									item.SubItems[_ID_LOGTEXT].Tag = infos;
									
                            		// Les favoris
                            		String favorited = items[5];
                            		if (favorited == "yes")
                            		{
                            			item.SubItems[_ID_FAVORITED].Text = _daddy.GetTranslator().GetString("BtnYes");
                            			infos._bFavorited = true;
                            		}
                            		
                            		// Les images à charger
                            		String imgitems = items[6];
                            		{
                            			// chemin$titre$desc|chemin$titre$desc|...
                            			String msg  = "";
                            			int nb = 0;
                            			// On split
                            			List<String> images = imgitems.Split('|').ToList();
                            			foreach(String image in images)
                            			{
                            				List<String> data = image.Split('$').ToList();
                            				if (data.Count == 3)
                            				{
                            					// On vérifie que le fichier image existe
                            					String path = data[0];
                            					if (File.Exists(path))
                            					{
                            						if (infos._dicoImages.ContainsKey(path) == false)
                            						{
                            							KeyValuePair<string, string> kv = new KeyValuePair<string, string>(data[1], data[2]);
        												infos._dicoImages.Add(path, kv);
                            							nb++;
                            						}
                            					}
                            					else
                            					{
                            						msg += data[1] + ": " + data[2] + " -> " + data[0] + "\r\n";
                            					}
                            				}
                            			}
                            			if (msg != "")
                            			{
                            				_daddy.MsgActionWarning(this, _daddy.GetTranslator().GetString("LblErrMissingImage") + "\r\n" + msg);
                            			}
                            			
                            			item.SubItems[_ID_IMAGES].Text = nb.ToString();
                            		}
                            		
                            		// Le status du log
                            		if (items.Count >= 8)
                            		{
                            			item.SubItems[_ID_LOGSTATUS].Text = items[7];
                            		}
                            		else
                            		{
                            			item.SubItems[_ID_LOGSTATUS].Text = "?";
                            		}
                            	}
                            }
                            	
                        }
                        else
                        {
                            // log type non supporté (Unattempted ?)
                        }
                    }
                    ColorItems();
                }
        		lvCachesfn.Refresh();
        		DisplaySelCount();
        		CalculateRCRT();
        		
            }
            catch (Exception exc)
            {
            	lvCachesfn.Refresh();
            	_daddy.ShowException("", _daddy.GetTranslator().GetString("FMenuLiveLoadFieldNotes"), exc);
            }	
        }
        
        void CalculateRCRT()
        {
        	lvCachesfn.SuspendLayout();
        	
        	// Mise à jour du RC/RT
        	Dictionary<String, int> rtperday = new Dictionary<string, int>();
        	Dictionary<ListViewItem, int> rcperitem = new Dictionary<ListViewItem, int>();
        	
        	// on calcule le RT pour chaque jour
        	foreach (ListViewItem item in lvCachesfn.Items)
        	{
        		// récupère le jour
        		String date = ((DateTime)(item.SubItems[_ID_DATE].Tag)).ToString("yyyy-MM-dd");
        		
        		// récupère le type
        		int iType = LogTypeToInt(item.SubItems[_ID_TYPELOG].Text);
        		
        		if ((iType == 10) || (iType == 2))
        		{
        			// on fait évoluer le rt
        			// est-ce la première entrée du jour ?
        			if (rtperday.ContainsKey(date))
        			{
        				// Oui, on est déjà sur un jour existant
        				rtperday[date] = rtperday[date] + 1;
        			}
        			else
        			{
        				// C'est un nouveau jour
        				rtperday.Add(date, 1);
        			}
        			
        			// On fait évoluer le rc
        			rcperitem[item] = rtperday[date];
        		}
        		else
        		{
        			// rien n'est fait pour le rt
        			
        			// On discarde le rc pour ce jour là
        			rcperitem.Add(item, 0);
        		}
        	}
        	
        	// on met à jour l'affichage
        	foreach (ListViewItem item in lvCachesfn.Items)
        	{
        		// récupère le jour
        		String date = ((DateTime)(item.SubItems[_ID_DATE].Tag)).ToString("yyyy-MM-dd");
        		
        		var rc = 0;
        		int rt = 0;
        		if (rcperitem.ContainsKey(item))
        			rc = rcperitem[item];
        		if (rtperday.ContainsKey(date))
        			rt = rtperday[date];
        		
        		item.SubItems[_ID_RCRT].Text = ((rc == 0)?"-":rc.ToString()) + "/" + ((rt == 0)?"-":rt.ToString());
        		item.SubItems[_ID_RCRT].Tag = new Tuple<int, int>(rc,rt);
        	}
        	
        	lvCachesfn.Refresh();
        }
        
        int _ID_CODE = 0;
        int _ID_DONE = 1;
        int _ID_NAME = 2;
        int _ID_DT = 3;
        /// <summary>
        /// 
        /// </summary>
        public int _ID_DATE = 4;
        int _ID_TYPELOG = 5;
        int _ID_LOGTEXT = 6;
        int _ID_FIELDNOTE = 7;
        int _ID_FAVORITED = 8;
        int _ID_TBSTATUS = 9;
        int _ID_IMAGES = 10;
        int _ID_LOGSTATUS = 11;
        /// <summary>
        /// 
        /// </summary>
        public int _ID_NUM = 12;
        /// <summary>
        /// 
        /// </summary>
        public int _ID_RCRT = 13;
        
        private bool EnableOrNoteDefaultLogHeaderDeletion()
        {
        	String headerfile = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + GCLogHMI.GetDefaultLogTextFileName();
        	if (File.Exists(headerfile))
        		deleteDefaultLogTextToolStripMenuItem.Enabled = true;
        	else
        		deleteDefaultLogTextToolStripMenuItem.Enabled = false;
        	return deleteDefaultLogTextToolStripMenuItem.Enabled;
        }
        
        /// <summary>
        /// Automatically translate form labels
        /// </summary>
        public void TranslateForm()
        {
            this.Text = _daddy.GetTranslator().GetString("FieldNotesHMI");
            
            lvCachesfn.Columns.Add("#", 50);
            _ID_NUM = lvCachesfn.Columns.Count - 1;
            
            lvCachesfn.Columns.Add(_daddy.GetTranslator().GetString("LVCode"), 100);
            _ID_CODE = lvCachesfn.Columns.Count - 1;
            
            lvCachesfn.Columns.Add(_daddy.GetTranslator().GetString("LVDone"), 50);
            _ID_DONE = lvCachesfn.Columns.Count - 1;
            
            lvCachesfn.Columns.Add(_daddy.GetTranslator().GetString("LVName"), 100);
            _ID_NAME = lvCachesfn.Columns.Count - 1;
            
            lvCachesfn.Columns.Add(_daddy.GetTranslator().GetString("FGBDifficultyTerrain"), 50);
            _ID_DT = lvCachesfn.Columns.Count - 1;
            
            lvCachesfn.Columns.Add("RC/RT", 50);
            _ID_RCRT = lvCachesfn.Columns.Count - 1;
            
            lvCachesfn.Columns.Add(_daddy.GetTranslator().GetString("lbldatelogged"), 120);
            _ID_DATE = lvCachesfn.Columns.Count - 1;
            
            lvCachesfn.Columns.Add(_daddy.GetTranslator().GetString("lbltypeoflog"), 80);
            _ID_TYPELOG = lvCachesfn.Columns.Count - 1;
            
            lvCachesfn.Columns.Add(_daddy.GetTranslator().GetString("LblLogText"), 200);
            _ID_LOGTEXT = lvCachesfn.Columns.Count - 1;
                        
            lvCachesfn.Columns.Add(_daddy.GetTranslator().GetString("LblFieldNote"), 90);
            _ID_FIELDNOTE = lvCachesfn.Columns.Count - 1;
            
            lvCachesfn.Columns.Add(_daddy.GetTranslator().GetString("LblFavorited"), 60);
            _ID_FAVORITED = lvCachesfn.Columns.Count - 1;
            
            lvCachesfn.Columns.Add(_daddy.GetTranslator().GetString("LblTBStatus"), 90);
            _ID_TBSTATUS = lvCachesfn.Columns.Count - 1;
            
            lvCachesfn.Columns.Add(_daddy.GetTranslator().GetString("LblFnNbImages"), 60);
            _ID_IMAGES = lvCachesfn.Columns.Count - 1;
            
            lvCachesfn.Columns.Add(_daddy.GetTranslator().GetString("LblLogStatus"), 150);
            _ID_LOGSTATUS = lvCachesfn.Columns.Count - 1;
            
            // Et on ajoute les images à lvCachesfn
            List<String> ltypes = GeocachingConstants.GetSupportedCacheTypes();
            _imageList.Images.Clear();
            _imageList.ColorDepth = ColorDepth.Depth32Bit;
            _imageList.ImageSize = new Size(32, 32); // this will affect the row height
            foreach(String type in ltypes)
            {
            	int it = _daddy.getIndexImages(type);
	            Image image = _daddy.getImageFromIndex(it);
	            _imageList.Images.Add(type, image);
            }
            lvCachesfn.SmallImageList = _imageList;
            
            templateToolStripMenuItem.Text = _daddy.GetTranslator().GetString("gbTemplate");
            defineDefaultLogTextToolStripMenuItem.Text = _daddy.GetTranslator().GetString("defineDefaultLogTextToolStripMenuItem");
			deleteDefaultLogTextToolStripMenuItem.Text = _daddy.GetTranslator().GetString("deleteDefaultLogTextToolStripMenuItem");
			EnableOrNoteDefaultLogHeaderDeletion();
			
			defineTtemplateToolStripMenuItem.Text = _daddy.GetTranslator().GetString("defineTtemplateToolStripMenuItem");
			applyTemplateToolStripMenuItem.Text = _daddy.GetTranslator().GetString("applyTemplateToolStripMenuItem");
			
			informationToolStripMenuItem.Text = _daddy.GetTranslator().GetString("gbInformation");
			completeEmptyCachesToolStripMenuItem.Text = _daddy.GetTranslator().GetString("completeEmptyCachesToolStripMenuItem");
			displaySelectedToolStripMenuItem.Text = _daddy.GetTranslator().GetString("displaySelectedToolStripMenuItem");
			displayLogStatusToolStripMenuItem.Text = _daddy.GetTranslator().GetString("displayLogStatusToolStripMenuItem");
			
			selectionToolStripMenuItem.Text = _daddy.GetTranslator().GetString("LblLogManagement");
			modifyLogSelectedToolStripMenuItem.Text = _daddy.GetTranslator().GetString("modifyLogSelectedToolStripMenuItem");
			copyFieldNotesToolStripMenuItem.Text = _daddy.GetTranslator().GetString("copyFieldNotesToolStripMenuItem");
			delSelectedToolStripMenuItem.Text = _daddy.GetTranslator().GetString("delSelectedToolStripMenuItem");
			logSelectedToolStripMenuItem.Text = _daddy.GetTranslator().GetString("logSelectedToolStripMenuItem");

			importExportToolStripMenuItem.Text = _daddy.GetTranslator().GetString("FMenuImportExportFN");
			loadToolStripMenuItem.Text = _daddy.GetTranslator().GetString("FMenuLoadFN");
			saveToolStripMenuItem.Text = _daddy.GetTranslator().GetString("FMenuSaveFN");
				
			tBsToolStripMenuItem.Text = _daddy.GetTranslator().GetString("tBsToolStripMenuItem");
			travelTBOnSelectedCachesToolStripMenuItem.Text = _daddy.GetTranslator().GetString("travelTBOnSelectedCachesToolStripMenuItem");

			if (!_daddy.GetInternetStatus())
			{
				completeEmptyCachesToolStripMenuItem.Enabled = false;
				displayLogStatusToolStripMenuItem.Enabled = false;
				logSelectedToolStripMenuItem.Enabled = false;
				tBsToolStripMenuItem.Enabled = false;
				travelTBOnSelectedCachesToolStripMenuItem.Enabled = false;
			}
			
			_daddy.TranslateTooltips(this, null);
			
			// Le menu contextuel
			_mnuContextMenu = new ContextMenuStrip();
			
			_mnuContextMenu.Items.Add(_daddy.CreateTSMI("applyTemplateToolStripMenuItem", ApplyTemplateToolStripMenuItemClick));
			_mnuContextMenu.Items.Add(_daddy.CreateTSMI("completeEmptyCachesToolStripMenuItem", CompleteEmptyCachesToolStripMenuItemClick, _daddy.GetInternetStatus()));
			_mnuContextMenu.Items.Add(_daddy.CreateTSMI("displaySelectedToolStripMenuItem", DisplaySelectedToolStripMenuItemClick));
			_mnuContextMenu.Items.Add(_daddy.CreateTSMI("displayLogStatusToolStripMenuItem", DisplayLogStatusToolStripMenuItemClick, _daddy.GetInternetStatus()));
			
			_mnuContextMenu.Items.Add(_daddy.CreateTSMI("modifyLogSelectedToolStripMenuItem", ModifyLogSelectedToolStripMenuItemClick));
			_mnuContextMenu.Items.Add(_daddy.CreateTSMI("copyFieldNotesToolStripMenuItem", CopyFieldNotesToolStripMenuItemClick));
			_mnuContextMenu.Items.Add(_daddy.CreateTSMI("delSelectedToolStripMenuItem", DelSelectedToolStripMenuItemClick));
			int i = _mnuContextMenu.Items.Add(_daddy.CreateTSMI("logSelectedToolStripMenuItem", LogSelectedToolStripMenuItemClick, _daddy.GetInternetStatus()));
			_mnuContextMenu.Items[i].Image = logSelectedToolStripMenuItem.Image;

			 _daddy.TranslateTooltips(_mnuContextMenu, null);
            
			 DisplaySelCount();
            
        }

        private void ColorItems()
        {
        	if ((lvCachesfn.Items != null) && (lvCachesfn.Items.Count != 0))
        	{
	        	foreach (ListViewItem item in lvCachesfn.Items)
	        	{
	        		String txt = item.SubItems[_ID_LOGTEXT].Text;
	        		if (txt.Length <= 5)
	        		{
	        			item.SubItems[_ID_LOGTEXT].BackColor = Color.Red;
	        			item.SubItems[_ID_LOGTEXT].ForeColor = Color.White;
	        		}
	        		else
	        		{
	        			bool templated = (bool)(item.SubItems[_ID_FIELDNOTE].Tag);
	        			if (templated)
	        				item.SubItems[_ID_LOGTEXT].BackColor = Color.Orange;
	        			else
	        				item.SubItems[_ID_LOGTEXT].BackColor = Color.LightGreen;
	        			item.SubItems[_ID_LOGTEXT].ForeColor = Color.Black;
	        		}
	        	}
        	}
        }
        
        private void lvCaches_MouseDoubleClick(object sender, MouseEventArgs e)
        {
        	ModifyLogOfSelectedCaches();
        }

        
        private void CompleteCacheInfo(ListViewItem item)
        {
            if (item != null)
            {
                // On est obligé d'avoir un objet cache détaillé
                Geocache geo = null;
                if (item.Tag == null)
                {
                	geo = _daddy.GetEmptyCache(item.SubItems[_ID_CODE].Text);
                    List<Geocache> caches = new List<Geocache>();
                    caches.Add(geo);
                    /*
                    if (_daddy.GetInternetStatus())
                    	_daddy.CompleteSelectedCaches(ref caches, null, true);*/
                    // optimisation, pas nécessaire !!!
                    
                    item.Tag = geo;
                    item.SubItems[_ID_NAME].Text = geo._Name;
                }
            }
        }

        private String LogTypeToString(int iType)
        {
        	if (iType == 2)
                return sfoundit;
        	else if (iType == 3)
                return sdnf;
        	else if (iType == 4)
        		return swritenote;
        	else if (iType == 45)
                return sneedmaint;
            else if (iType == 46)
                return sownermaint;
            else if (iType == 9)
                return swillattend;
            else if (iType == 10)
                return sattended;
            else if (iType == 11) // BCR 20170822
            	return swebcamphototaken;
            else
            	return "Logged";
        }
        
        private int LogTypeToInt(string sType)
		{
			if (sType == sfoundit)
                return 2;
            else if (sType == sdnf)
                return 3;
            else if (sType == swritenote)
                return 4; 
			else if (sType == sneedmaint)
                return 45;
			else if (sType == sownermaint)
                return 46;
			else if (sType == swillattend)
                return 9;
			else if (sType == sattended)
                return 10;
			else if (sType == swebcamphototaken) // BCR 20170822
                return 11;
			return 0;
		}
        
        private string LogTypeToGC(string sType)
		{
			if (sType == sfoundit)
                return sfnfoundit;
            else if (sType == sdnf)
                return sfndnf;
            else if (sType == sneedmaint)
                return sfnneedmaint; 
			else if (sType == sownermaint)
                return sfnownermaint; 
			else if (sType == swillattend)
                return sfnwillattend; 
			else if (sType == sattended)
                return sfnattended; 
			else if (sType == swritenote)
                return sfnwritenote;
			else if (sType == swebcamphototaken) // BCR 20170822
				return sfnwebcamphototaken;
			return "";
		}
        
        private string LogTypeGCToNice(string sType)
		{
			if (sType == sfnfoundit)
                return sfoundit;
            else if (sType == sfndnf)
                return sdnf;
            else if (sType == sfnneedmaint)
                return sneedmaint; 
			else if (sType == sfnownermaint)
                return sownermaint; 
			else if (sType == sfnwillattend)
                return swillattend; 
			else if (sType == sfnattended)
                return sattended; 
			else if (sType == sfnwritenote)
                return swritenote;
			else if (sType == sfnwebcamphototaken)
				return swebcamphototaken; // BCR 20170822
			return "";
		}
        
        private bool IsSupportedFnLogType(string sType)
		{
			if ((sType == sfnfoundit) ||
        	    (sType == sfndnf) ||
        	    (sType == sfnneedmaint) ||
        	    (sType == sfnownermaint) ||
        	    (sType == sfnwillattend) ||
        	    (sType == sfnattended) ||
        	    (sType == sfnwritenote) || // BCR 20170822
        	    (sType == sfnwebcamphototaken)
        	   )
        		return true;
        	else
        		return false;
		}
        
        private DialogResult LogCache(ListViewItem item, bool bSilent, bool bAutologBasedOnFieldNotes, int runcount, int runtotal)
        {
            if (item != null)
            {
                // On est obligé d'avoir un objet cache détaillé
                Geocache geo = null;
                CompleteCacheInfo(item);
                geo = (Geocache)(item.Tag);
                
                int iType = LogTypeToInt(item.SubItems[_ID_TYPELOG].Text);
                
                String owner = ConfigurationManager.AppSettings["owner"];
                LogExtraInfo infos = item.SubItems[_ID_LOGTEXT].Tag as LogExtraInfo;
                GCLogHMI loghmi = new GCLogHMI(_daddy, geo, (DateTime)(item.SubItems[_ID_DATE].Tag),
                                               item.SubItems[_ID_FIELDNOTE].Text,
                                               iType, GCLogHMI.TemplateType.None, bSilent, bAutologBasedOnFieldNotes,
                                               item.SubItems[_ID_LOGTEXT].Text, infos, runcount, runtotal);
                                
                DialogResult result;
                if (bAutologBasedOnFieldNotes == false)
                	result = loghmi.ShowDialog();
                else
                	result = loghmi.DialogResult;
                
                if (result == DialogResult.OK)
                {
                    // Ca a marché !
                    // Attention au cas de log auto qui n'a pas modifié correctement l'IHM donc pas besoin de recréer l'item
                    if (!bAutologBasedOnFieldNotes)
                    {
                    	UpdateItemStatus(loghmi._iTypeReallyLogged, item);
                    	UpdateItemFromLogHMI(loghmi, item);
                    }
                    else
                    {
                    	UpdateItemStatus(iType, item);
                    }
                }
        		
                return result;
            }
            return DialogResult.Abort;
        }

        private void UpdateItemStatus(int type, ListViewItem item)
        {
        	item.SubItems[_ID_DONE].Text = _daddy.GetTranslator().GetString("BtnYes");
            item.SubItems[_ID_LOGSTATUS].Text = LogTypeToString(type);
            item.SubItems[_ID_DONE].BackColor = Color.LightGreen;
            item.SubItems[_ID_LOGSTATUS].BackColor = Color.LightGreen;
        }
        
        private void UpdateItemFromLogHMI(GCLogHMI loghmi, ListViewItem item)
        {
        	String text = loghmi.txtCommentsloghmi.Text;
        	String date = loghmi.dtPickerloghmi.Value.ToString("yyyy-MM-dd@HH:mm");//ToShortDateString();
            String type = loghmi.cbLogTypeloghmi.Text;
            
        	// La date
        	item.SubItems[_ID_DATE].Text = date;
        	item.SubItems[_ID_DATE].Tag = loghmi.dtPickerloghmi.Value;
        	
        	// Le type
        	item.SubItems[_ID_TYPELOG].Text = type;
        	
        	// Le commentaire
        	// Attention il peut y avoir seulement un "\r\n" ajouté à la fin par rapport au log précédent
        	// dans ce cas là on le jette
        	if ((item.SubItems[_ID_LOGTEXT].Text + "\r\n") == text)
        	{
        		// finalement aucune modification de texte, on ne touche à rien
        	}
        	else
        	{
        		// Une modification ?
	        	if (item.SubItems[_ID_LOGTEXT].Text != text)
	        	{
	        		// on n'est forcément plus templated
	        		item.SubItems[_ID_FIELDNOTE].Tag = false;
	        	}
	        	item.SubItems[_ID_LOGTEXT].Text = text;
        	}
        	
        	// Favorisé & TB & images
        	Dictionary< KeyValuePair<string, string>, int> tbs = loghmi.ExportTBs();
        	Dictionary<String, KeyValuePair<String, String>> imgs = loghmi.ExportImages();
        	UpdateItemForTBsAndFavorite(loghmi.cbFavsloghmi.Checked, tbs, imgs, item);
        	
            // On écrase toutes les infos extra
			LogExtraInfo infos = new LogExtraInfo();
			infos._bUseDateAndType = false;
			infos._bFavorited = loghmi.cbFavsloghmi.Checked;
			infos._dicoTBs = tbs;
			infos._dicoImages = imgs;
			item.SubItems[_ID_LOGTEXT].Tag = infos;
        }
        
        private void UpdateItemForTBsAndFavorite(bool bFavorited, Dictionary< KeyValuePair<string, string>, int> tbs, Dictionary<String, KeyValuePair<String, String>> imgs, ListViewItem item)
        {
        	// Favorisé ?
            if (bFavorited)
            	item.SubItems[_ID_FAVORITED].Text = _daddy.GetTranslator().GetString("BtnYes");
            else
            	item.SubItems[_ID_FAVORITED].Text = _daddy.GetTranslator().GetString("BtnNo");
            
        	// TB status
        	int nbdrop = 0;
            int nbvis = 0;
            if (tbs != null)
            {
	            foreach (KeyValuePair<KeyValuePair<string, string>, int> pair in tbs)
	    		{
	            	if (pair.Value == 1)
	            		nbdrop++;
	            	if (pair.Value == 2)
	            		nbvis++;
	            }
            }
            item.SubItems[_ID_TBSTATUS].Text = String.Format(_daddy.GetTranslator().GetString("LblTBStatusTemplate"), nbdrop, nbvis);
            
            // Nb images
            int nb = 0;
            if (imgs != null)
            	nb = imgs.Count;
            item.SubItems[_ID_IMAGES].Text = nb.ToString();
            
        }
        
        private void btnDelSelected_Click(object sender, EventArgs e)
        {
        	if (IsSelectionEmpty())
				return;
			
        	DialogResult dialogResult = MessageBox.Show(
	            _daddy.GetTranslator().GetString("AskDelLog"),
	            _daddy.GetTranslator().GetString("AskDelLogCaption"),
	            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
	        if (dialogResult == DialogResult.Yes)
	        {
	            ListView.SelectedListViewItemCollection collection = lvCachesfn.SelectedItems;
	            foreach (ListViewItem item in collection)
                {
                    lvCachesfn.Items.Remove(item);
                }
	            DisplaySelCount();
	        }
	        CalculateRCRT();
        }

        private bool IsSelectionEmpty()
        {
        	ListView.SelectedListViewItemCollection collection = lvCachesfn.SelectedItems;
            if ((collection != null) && (collection.Count != 0))
            {
            	return false;
            }
            else
            	return true;
        }
        
        /// <summary>
        /// Choice of log method selection : From a new template common for all, Automatically from existing info, Manually one by one
        /// </summary>
        /// <param name="titleKeyLabel">Translation key for title</param>
        /// <returns>1: From a new template common for all, 2: Automatically from existing info, 3: Manually one by one</returns>
        public int SelectLogTypeMethode(String titleKeyLabel)
        {
        	List<ParameterObject> lst = new List<ParameterObject>();
            List<String> lstv = new List<string>();
            lstv.Add(_daddy.GetTranslator().GetString("LblLogMethodNewModel"));
            lstv.Add(_daddy.GetTranslator().GetString("LblLogMethodAutoFromInfos"));
            lstv.Add(_daddy.GetTranslator().GetString("LblLogMethodManualOneByOne"));
            lst.Add(new ParameterObject(ParameterObject.ParameterType.List, lstv, "logmethod", 
                                        _daddy.GetTranslator().GetString("LogMethodTxt"),
                                        _daddy.GetTranslator().GetStringM("LblTooltipLogMethodFieldNote")));
            
            ParametersChanger changer = new ParametersChanger();
            changer.Title = _daddy.GetTranslator().GetString(titleKeyLabel);
            changer.BtnCancel = _daddy.GetTranslator().GetString("BtnCancel");
            changer.BtnOK = _daddy.GetTranslator().GetString("BtnOk");
            changer.ErrorFormater = _daddy.GetTranslator().GetString("ErrWrongParameter");
            changer.ErrorTitle = _daddy.GetTranslator().GetString("Error");
            changer.Parameters = lst;
            changer.Font = this.Font;
            changer.Icon = this.Icon;
            if (changer.ShowDialog() == DialogResult.OK)
            {
                String logmethod = changer.Parameters[0].Value;
                if (logmethod == _daddy.GetTranslator().GetString("LblLogMethodNewModel"))
                	return 1;
                else if (logmethod == _daddy.GetTranslator().GetString("LblLogMethodAutoFromInfos"))
                	return 2;
                else if (logmethod == _daddy.GetTranslator().GetString("LblLogMethodManualOneByOne"))
                	return 3;
            }
            return -1;
        }
        
        private bool AskReadyToLog(ListView.SelectedListViewItemCollection collection)
        {
        	// Vérification des logs vides ?
        	int nbVides = 0;
        	foreach (ListViewItem item in collection)
            {
        		if (item.SubItems[_ID_LOGTEXT].Text.Length <= 5)
        			nbVides++;
            }
        	
        	// Message de confirmation
        	String svide = "";
        	if (nbVides != 0)
        	{
        		svide = String.Format(_daddy.GetTranslator().GetString("LblErrEmptyLogs"), nbVides);
        	}
        	String smsg = String.Format(_daddy.GetTranslator().GetString("AskPerformLog"), collection.Count, svide);
        	smsg = smsg.Replace("#","\r\n");
        	
        	DialogResult dialogResult = MessageBox.Show(
	            smsg,
	            _daddy.GetTranslator().GetString("AskPerformLogCaption"),
	            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
	        if (dialogResult == DialogResult.Yes)
	        {
	        	return true;
	        }
	        else
	        	return false;
        }
        
        private void btnLogSelected_Click(object sender, EventArgs e)
        {
        	if (IsSelectionEmpty())
				return;
			
            ListView.SelectedListViewItemCollection collection = lvCachesfn.SelectedItems;
            
            if (collection.Count == 1)
            {
            	ListViewItem item = collection[0];
            	Tuple<int, int> rcrt = (Tuple<int, int>)(item.SubItems[_ID_RCRT].Tag);
            	
                LogCache(item, false, false, rcrt.Item1, rcrt.Item2);
            }
            else
            {
            	
            	int ilogmethod = SelectLogTypeMethode("FMenuLiveLogCache");
                if (ilogmethod != -1)
                {
                    if (ilogmethod == 1)
                    {
                    	// A partir d'un nouveau modèle commun pour toutes
						// On utilise un template commun, donc on oublie juste les comments
                        // ****************************************************************
                        // mais on garde la date
                        // On va d'abord mettre à jour toutes les infos des caches
                        GetCacheInfo(true, false);

                        // On construit la liste maintenant
                        List<Geocache> caches = new List<Geocache>();
                        foreach (ListViewItem item in collection)
                        {
                            Geocache geo = (Geocache)(item.Tag);
                            caches.Add(geo);
                        }

                        // Et on utilise GCLogHMI uniquement pour saisir le template et les TBs
                        GCLogHMI loghmi = new GCLogHMI(_daddy, caches, GCLogHMI.TemplateType.Complex, false, "");
                        DialogResult result = loghmi.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            String text = loghmi.txtCommentsloghmi.Text;
                            List<KeyValuePair<string, string>> tbs = loghmi._TBActionList;

                            // Et maintenant on lance les logs en boucle
                            _daddy._ThreadProgressBarTitle = _daddy.GetTranslator().GetString("GCLogHMI");
                            _daddy.CreateThreadProgressBarEnh();
                            
                            // Wait for the creation of the bar
                            while (_daddy._ThreadProgressBar == null)
                            {
                                Thread.Sleep(10);
                                Application.DoEvents();
                            }
                            _daddy._ThreadProgressBar.progressBar1.Maximum = caches.Count;

                            bool bOk = true;
                            String msg = "";
                            bool bDiscardAlreadyDroppedTBs = false;
                            foreach (ListViewItem item in collection)
                            {
                                Geocache cache = (Geocache)(item.Tag);
                                int iType = loghmi._iTypeReallyLogged;// LogTypeToInt(item.SubItems[_ID_TYPELOG].Text);
                                DateTime date = loghmi.dtPickerloghmi.Value; // (DateTime)(item.SubItems[_ID_DATE].Tag);
								Tuple<int, int> rcrt = (Tuple<int, int>)(item.SubItems[_ID_RCRT].Tag);
            	
                                _daddy._ThreadProgressBar.lblWait.Text = cache._Code + " " + cache._Name;

                                if (_daddy._ThreadProgressBar._bAbort)
                                {
                                    msg += cache._Code + " " + cache._Name + " -> " + _daddy.GetTranslator().GetString("AbortLoggingCache") + "\r\n";
                                    bOk = false;
                                }
                                else
                                {
                                	if (GCLogHMI.LogCache(_daddy, cache, date, text, loghmi.cbFavsloghmi.Checked, iType, tbs, bDiscardAlreadyDroppedTBs, rcrt.Item1, rcrt.Item2, loghmi.ExportImages()))
                                    {
                                        // At least one success, so no double drop of TBs !
                                        bDiscardAlreadyDroppedTBs = true;
                                        
                                        // Ca a marché !
                						UpdateItemStatus(iType, item);
                						
                						UpdateItemFromLogHMI(loghmi, item);
                                    }
                                    else
                                    {
                                        msg += cache._Code + " " + cache._Name + " -> " + _daddy.GetTranslator().GetString("ErrLoggingCache") + "\r\n";
                                        bOk = false;
                                    }
                                }
                                _daddy._ThreadProgressBar.Step();
                            }

                            _daddy.KillThreadProgressBarEnh();
			        		
                            if (bOk)
                            {
                                
                                _daddy.MsgActionOk(this, _daddy.GetTranslator().GetString("SuccessLoggingCache"));
                            }
                            else
                            {
                                _daddy.MsgActionError(this, msg);
                            }
                        }
                    }
                    else if (ilogmethod == 2)
                    {
                    	if (!AskReadyToLog(collection))
                    		return;
                    	PerformLogMethod2(collection);
                    	
                    }
                    else if (ilogmethod == 3)
                    {
                    	PerformLogMethod3(collection);
                    }
                }
            }
        }

        private void PerformLogMethod2(ListView.SelectedListViewItemCollection collection)
        {
        	
        	// Automatiquement à partir des infos existantes
			// On va d'abord compléter les caches
            GetCacheInfo(true, false);
            
            // Et maintenant on lance les logs en boucle
			_daddy._ThreadProgressBarTitle = _daddy.GetTranslator().GetString("GCLogHMI");
			_daddy.CreateThreadProgressBarEnh();
			
			// Wait for the creation of the bar
			while (_daddy._ThreadProgressBar == null)
			{
				Thread.Sleep(10);
				Application.DoEvents();
			}
			_daddy._ThreadProgressBar.progressBar1.Maximum = collection.Count;
			
            foreach (ListViewItem item in collection)
            {
            	Tuple<int, int> rcrt = (Tuple<int, int>)(item.SubItems[_ID_RCRT].Tag);
            	_daddy._ThreadProgressBar.lblWait.Text = item.SubItems[_ID_CODE].Text + " " + item.SubItems[_ID_NAME].Text;

				if (_daddy._ThreadProgressBar._bAbort)
				{
					_daddy.MsgActionCanceled(this, _daddy.GetTranslator().GetString("AbortLoggingCache"));
					_daddy.KillThreadProgressBarEnh();
                    return;
				}
				else
				{
	                DialogResult res = LogCache(item, true, true, rcrt.Item1, rcrt.Item2);
	                if (res != DialogResult.OK)
	                {
	                    _daddy.MsgActionError(this, _daddy.GetTranslator().GetString("ErrLoggingCache"));
	                	_daddy.KillThreadProgressBarEnh();
	                    return;
	                }
	                else
	                {
	                	_daddy._ThreadProgressBar.Step();
	                }
				}
            }
            ColorItems();
            _daddy.KillThreadProgressBarEnh();
            _daddy.MsgActionOk(this, _daddy.GetTranslator().GetString("SuccessLoggingCache"));
        }
        
        private void PerformLogMethod3(ListView.SelectedListViewItemCollection collection)
        {
        	
        	// Automatiquement à partir des infos existantes
			// On va d'abord compléter les caches
            GetCacheInfo(true, false);
            
            foreach (ListViewItem item in collection)
            {
            	Tuple<int, int> rcrt = (Tuple<int, int>)(item.SubItems[_ID_RCRT].Tag);
                DialogResult res = LogCache(item, true, false, rcrt.Item1, rcrt.Item2);
                if (res != DialogResult.OK)
                {
                    _daddy.MsgActionError(this, _daddy.GetTranslator().GetString("ErrLoggingCache"));
                    return;
                }
            }
            ColorItems();
            _daddy.MsgActionOk(this, _daddy.GetTranslator().GetString("SuccessLoggingCache"));
        }
        
        private void GetCacheInfo(bool bSilent, bool doTheJob)
        {
            ListView.SelectedListViewItemCollection collection = lvCachesfn.SelectedItems;
            if ((collection != null) && (collection.Count != 0))
            {
                List<ListViewItem> items = new List<ListViewItem>();
                List<Geocache> caches = new List<Geocache>();
                foreach (ListViewItem item in collection)
                {
                    if (item.Tag == null)
                    {
                        Geocache geo = _daddy.GetEmptyCache(item.SubItems[_ID_CODE].Text);
                        items.Add(item);
                        caches.Add(geo);
                    }
                    else
                    {
                    	// Est ce que la cache est vide ?
                    	Geocache geo = item.Tag as Geocache;
                    	if (geo._Name == "")
                    	{
                    		// On considère qu'elle est vide
                    		items.Add(item);
                        	caches.Add(geo);
                    	}
                    	
                    }
                }
                if (caches.Count != 0)
                {
                	if (_daddy.GetInternetStatus() && doTheJob)
                    	_daddy.CompleteSelectedCaches(ref caches, null, bSilent);
                	
                	// On se créée un dico
                	Dictionary<String, Geocache> dico = new Dictionary<string, Geocache>();
                	
                    for (int i = 0; i < caches.Count; i++)
                    {
                        ListViewItem item = items[i];
                        Geocache geo = caches[i];
                        if (dico.ContainsKey(geo._Code) == false)
                        	dico.Add(geo._Code, geo);
                        
                        item.Tag = geo;
                        item.SubItems[_ID_NAME].Text = geo._Name;
                        // on ajoute l'icone
                        item.ImageKey = geo._Type;
                        // Et le D/T
                        if (geo._Name != "")
                        	item.SubItems[_ID_DT].Text = geo._D + "/" + geo._T;
                    }
                    
                    // Petit bonus, on regarde si d'autres items non sélectionnés ne pourraient pas bénéficier de notre complétion 
                    // (caches dupliquées par exemple)
                    foreach(ListViewItem li in lvCachesfn.Items)
                    {
                    	if (dico.ContainsKey(li.Text))
                    	{
	                    	bool replace = false;
	                    	if (li.Tag == null)
	                    	{
	                    		replace = true;
	                    	}
	                    	else
	                    	{
	                    		Geocache gg = li.Tag as Geocache;
	                    		if (gg._Name == "")
	                    		{
	                    			// C'est une bouse vide
	                    			replace = true;
	                    		}
	                    	}
	                    	
	                    	if (replace)
	                    	{
	                    		Geocache gg = dico[li.Text];
	                    		li.Tag = gg;
	                    		li.SubItems[_ID_NAME].Text = gg._Name;
		                        // on ajoute l'icone
		                        li.ImageKey = gg._Type;
		                        // Et le D/T
		                        if (gg._Name != "")
                        			li.SubItems[_ID_DT].Text = gg._D + "/" + gg._T;
	                    	}
                    	}
                    }
                }
            }
        }


        private void fnbtnMoveUp_Click(object sender, EventArgs e)
        {
        	if (IsSelectionEmpty())
				return;
        	
        	// On désactive le sort
        	lvwColumnSorter.Order = SortOrder.None;
        	lvwColumnSorter.SortColumn = 0;
        	
	         ListView.SelectedListViewItemCollection collection = lvCachesfn.SelectedItems;
	         
	         // On parcourt dans l'ordre chrono
	         // Ne fait rien si le premier item de la collection est en première position de la liste
	         if (collection[0].Index == 0)
	             return;
	
	         foreach (ListViewItem item in collection)
	         {
	             int currentIndex = item.Index;
	             if (currentIndex > 0)
	             {
	                 lvCachesfn.Items.RemoveAt(currentIndex);
	                 lvCachesfn.Items.Insert(currentIndex - 1, item);
	             }
	         }
	         CalculateRCRT();
        }

        private void fnbtnMoveDown_Click(object sender, EventArgs e)
        {
        	if (IsSelectionEmpty())
				return;
        	
        	// On désactive le sort
        	lvwColumnSorter.Order = SortOrder.None;
        	lvwColumnSorter.SortColumn = 0;
        	
            ListView.SelectedListViewItemCollection collection = lvCachesfn.SelectedItems;
            
            // On parcourt dans l'ordre chrono inverse
            // Ne fait rien si le dernier item de la collection est en dernière position de la liste
            if (collection[collection.Count - 1].Index == (lvCachesfn.Items.Count - 1))
                return;

            for (int i = collection.Count - 1; i >= 0; i--)
            {
                ListViewItem item = collection[i];
                int currentIndex = item.Index;
                if (currentIndex < (lvCachesfn.Items.Count - 1))
                {
                    lvCachesfn.Items.RemoveAt(currentIndex);
                    lvCachesfn.Items.Insert(currentIndex + 1, item);
                }
            }
            CalculateRCRT();
          
        }
		void FnbtnModifyLogSelectedClick(object sender, EventArgs e)
		{
			if (IsSelectionEmpty())
				return;
			
			ModifyLogOfSelectedCaches();
		}
		
		private GCLogHMI ModifyLogOfOneCacheImpl(ListViewItem item, GCLogHMI loghmi)
		{
			Geocache geo = null;
            CompleteCacheInfo(item);
            geo = (Geocache)(item.Tag);
            
            int iType = LogTypeToInt(item.SubItems[_ID_TYPELOG].Text);
            LogExtraInfo infos = item.SubItems[_ID_LOGTEXT].Tag as LogExtraInfo;
            Tuple<int, int> rcrt = (Tuple<int, int>)(item.SubItems[_ID_RCRT].Tag);
            
            if (loghmi == null)
            {
            	
            	loghmi = new GCLogHMI(_daddy, geo, (DateTime)(item.SubItems[_ID_DATE].Tag),
	                                   item.SubItems[_ID_FIELDNOTE].Text, iType, GCLogHMI.TemplateType.Complex, true, false, 
	                                   item.SubItems[_ID_LOGTEXT].Text, infos, rcrt.Item1, rcrt.Item2);
            }
            else
            {
            	loghmi.Reload(geo, (DateTime)(item.SubItems[_ID_DATE].Tag),
                           item.SubItems[_ID_FIELDNOTE].Text, iType, GCLogHMI.TemplateType.Complex, true, false, 
                           item.SubItems[_ID_LOGTEXT].Text, infos,  rcrt.Item1, rcrt.Item2);
            	loghmi.UpdateLogPreviewMarkdown();
			}
            
                        
            // On crée les infos de log extra si elles n'existent pas
            if (infos == null)
            {
            	infos = new LogExtraInfo();
            	item.SubItems[_ID_LOGTEXT].Tag = infos;
            }
            
            // On ajoute les handlers de navigation
            bool previous = true;
            bool next = true;
            int pos = lvCachesfn.Items.IndexOf(item);
			if (pos == 0)
				previous = false;
			if (pos == (lvCachesfn.Items.Count - 1))
				next = false;
            loghmi.DefinePreviousAndNextButtons(previous, next, HandlerPrevious, HandlerNext, HandlerSave, pos);
            
            return loghmi;
		}
		
		private void ModifyLogOfOneCache(ListViewItem item)
		{
			GCLogHMI loghmi = ModifyLogOfOneCacheImpl(item, null);
			
            DialogResult result = loghmi.ShowDialog();
            if (result == DialogResult.OK)
            {   
            	UpdateItemFromLogHMI(loghmi, lvCachesfn.Items[loghmi._iItemPosition]);
            }
		}
		
		
		private void ModifyLogOfSelectedCaches()
		{
			
			ListView.SelectedListViewItemCollection collection = lvCachesfn.SelectedItems;
            if (collection != null)
            {
                if (collection.Count == 1)
                {
                	// On est obligé d'avoir un objet cache détaillé
                	ListViewItem item = collection[0];
                	ModifyLogOfOneCache(item);
                }
                else
                {
	                // On utilise un template commun, donc on oublie juste les comments
	                // ****************************************************************
	                DialogResult dialogResult = MessageBox.Show(
			            _daddy.GetTranslator().GetString("AskModifyBunchAndLoseComments"),
			            _daddy.GetTranslator().GetString("fnbtnModifyLogSelected"),
			            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			        if (dialogResult == DialogResult.Yes)
			        {
		                // mais on garde la date
		                // On va d'abord mettre à jour toutes les infos des caches
		                GetCacheInfo(true, false);
		
		                // On construit la liste maintenant
		                List<Geocache> caches = new List<Geocache>();
		                List<String> oldlogtext = new List<string>();
		                foreach (ListViewItem item in collection)
		                {
		                    Geocache geo = (Geocache)(item.Tag);
		                    caches.Add(geo);
		                    oldlogtext.Add(item.SubItems[_ID_LOGTEXT].Text);
		                }
		
		                // Et on utilise GCLogHMI uniquement pour saisir le template et les TBs
		                GCLogHMI loghmi = new GCLogHMI(_daddy, caches, GCLogHMI.TemplateType.Complex, false, "");
		                		                
		                DialogResult result = loghmi.ShowDialog();
		                if (result == DialogResult.OK)
		                {
		                    foreach (ListViewItem item in collection)
		                    {                    	
		                    	// On écrase toutes les infos extra
		                    	UpdateItemFromLogHMI(loghmi, item);
		                    }
		                }
			        }
                }
                ColorItems();
                CalculateRCRT();
            }
		}
		
		void BtnDefTemplateClick(object sender, EventArgs e)
		{
			GCLogHMI loghmi = new GCLogHMI(_daddy, null, GCLogHMI.TemplateType.Simple, false, _template);
			if (_logExtraInfo != null)
			{
				// On recharge
				loghmi.cbFavsloghmi.Checked = _logExtraInfo._bFavorited;
				loghmi.ImportTBs(_logExtraInfo._dicoTBs);
				loghmi.ImportImages(_logExtraInfo._dicoImages);
			}

            // Pour les field notes, on ne veut pas modifier la date et le type massivement
            if (loghmi.ShowDialog() == DialogResult.OK)
            {
            	// Le template a bien été saisi
            	_template = loghmi.txtCommentsloghmi.Text;
                _logExtraInfo = new LogExtraInfo();
                // Pas le type de log ni la date
                _logExtraInfo._bUseDateAndType = false;
                _logExtraInfo._bFavorited = loghmi.cbFavsloghmi.Checked;
                _logExtraInfo._dicoTBs = loghmi.ExportTBs();
                _logExtraInfo._dicoImages = loghmi.ExportImages();
            }
		}
		
		void BtnApplyTemplateClick(object sender, EventArgs e)
		{
			if (IsSelectionEmpty())
				return;
			
			DialogResult dialogResult = MessageBox.Show(
	            _daddy.GetTranslator().GetString("AskApplyTemplate"),
	            _daddy.GetTranslator().GetString("AskApplyTemplateCaption"),
	            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
	        if (dialogResult == DialogResult.Yes)
	        {
	        	ListView.SelectedListViewItemCollection collection = lvCachesfn.SelectedItems;
				foreach (ListViewItem item in collection)
	            {
	            	// Le commentaire
	            	item.SubItems[_ID_LOGTEXT].Text = _template;

                    if (_logExtraInfo != null)
                    {
                        LogExtraInfo infos = new LogExtraInfo();
                        item.SubItems[_ID_LOGTEXT].Tag = infos;
                        item.SubItems[_ID_FIELDNOTE].Tag = true; // templated
                        infos._bUseDateAndType = false;
                        infos._bFavorited = _logExtraInfo._bFavorited;
                        infos._dicoTBs = _logExtraInfo._dicoTBs;
                        infos._dicoImages = _logExtraInfo._dicoImages;

        				UpdateItemForTBsAndFavorite(infos._bFavorited, infos._dicoTBs, infos._dicoImages, item);
                    }
                }
				ColorItems();
	        }
		}
		void BtnCopyFieldNotesClick(object sender, EventArgs e)
		{
			if (IsSelectionEmpty())
				return;
			
			DialogResult dialogResult = MessageBox.Show(
	            _daddy.GetTranslator().GetString("AskCopyFieldNotes"),
	            _daddy.GetTranslator().GetString("AskCopyFieldNotesCaption"),
	            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
	        if (dialogResult == DialogResult.Yes)
	        {
	        	ListView.SelectedListViewItemCollection collection = lvCachesfn.SelectedItems;
				foreach (ListViewItem item in collection)
	            {
					// les notes de terrain
					String fn = item.SubItems[_ID_FIELDNOTE].Text;
						
	            	// Le commentaire
	            	String logtext = item.SubItems[_ID_LOGTEXT].Text;
	            	if (fn != "")
	            	{
	            		if (logtext != "")
	            			logtext += "\r\n" + fn;
	            		else
	            			logtext = fn;
	            		item.SubItems[_ID_LOGTEXT].Text = logtext;
	            		item.SubItems[_ID_FIELDNOTE].Tag = false; // templated NOT
	            	}
                }
				ColorItems();
	        }
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
                foreach (ListViewItem item in lvCachesfn.Items)
                {
                    item.Selected = true;
                }
                lvCachesfn.Focus();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        
		void DefineTtemplateToolStripMenuItemClick(object sender, EventArgs e)
		{
			BtnDefTemplateClick(sender, e);
		}
		void ApplyTemplateToolStripMenuItemClick(object sender, EventArgs e)
		{
			BtnApplyTemplateClick(sender, e);
		}
		void DisplaySelectedToolStripMenuItemClick(object sender, EventArgs e)
		{
			btnDisplaySelected_Click(sender, e);
		}
		void DisplayLogStatusToolStripMenuItemClick(object sender, EventArgs e)
		{
			fnbtnDisplayLogstatus_Click(sender, e);
		}
		void ModifyLogSelectedToolStripMenuItemClick(object sender, EventArgs e)
		{
			FnbtnModifyLogSelectedClick(sender, e);
		}
		void CopyFieldNotesToolStripMenuItemClick(object sender, EventArgs e)
		{
			BtnCopyFieldNotesClick(sender, e);
		}
		void DelSelectedToolStripMenuItemClick(object sender, EventArgs e)
		{
			btnDelSelected_Click(sender, e);
		}
		void LogSelectedToolStripMenuItemClick(object sender, EventArgs e)
		{
			btnLogSelected_Click(sender, e);
		}
		void LvCachesfnMouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
				DisplayContextMenu(new Point(e.X, e.Y));
            }
		}
		void LoadToolStripMenuItemClick(object sender, EventArgs e)
		{
			DialogResult dialogResult = MessageBox.Show(_daddy.GetTranslator().GetString("AskConfirm"),
			                                            _daddy.GetTranslator().GetString("FMenuLoadFN"),
                            							MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
				FieldNotesHMI.LoadFieldNotes(_daddy, this);
            }
		}
		void SaveToolStripMenuItemClick(object sender, EventArgs e)
		{
			try
			{
				
				String fns = _daddy.GetTranslator().GetString("LblFieldNote");
				SaveFileDialog saveFileDialog1 = new SaveFileDialog();
	            saveFileDialog1.Filter = "MGM " + fns + " (*.mfn)|*.mfn|Garmin " + fns + " (*.txt)|*.txt";
	            saveFileDialog1.RestoreDirectory = true;
	            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
	            {
	            	String filename = saveFileDialog1.FileName;
	            	bool bFnMGM = false;
	        		if (filename.ToLower().EndsWith(".mfn"))
	        			bFnMGM = true;
	        		else
	        		{
	        			DialogResult dialogResult = MessageBox.Show(_daddy.GetTranslator().GetStringM("LblWarningSaveFNGarmin"),
			                                            _daddy.GetTranslator().GetString("FMenuSaveFN"),
                            							MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			            if (dialogResult != DialogResult.Yes)
			            {
			            	return;
			            }
	        		}
	        		
	        		// On sauve
	        		System.IO.StreamWriter file = new System.IO.StreamWriter(filename, false, Encoding.Default);
	        		
	        		// Modèle 
	        		// GC5V10G,2015-09-14T15:37Z,Found it,"ABC"
	        		foreach (ListViewItem item in lvCachesfn.Items)
		            {
	        			DateTime date = (DateTime)(item.SubItems[_ID_DATE].Tag);
						String sdate = date.ToString("yyyy-MM-ddTHH:mmZ");
						String slogtype = LogTypeToGC(item.SubItems[_ID_TYPELOG].Text);
						
						// On sauve les infos de base
						String sfn = item.SubItems[_ID_FIELDNOTE].Text;
						sfn = sfn.Replace(",","&#44;");
						file.Write(item.SubItems[_ID_CODE].Text + "," + sdate + "," + slogtype + ",\"" + sfn + "\"");
						// Les infos supplémentaires
						// En l'état uniquement les favoris
						
						if (bFnMGM)
						{
							// Le texte du log
							// Il faut remplacer les , et les \r\n
							String logtext = item.SubItems[_ID_LOGTEXT].Text;
							logtext = logtext.Replace(",","&#44;");
							logtext = logtext.Replace("\r\n","&lf;");
							logtext = logtext.Replace("\n","&lf;");
							file.Write("," + logtext + ",");
							
							// favorisé ?
							LogExtraInfo infos = item.SubItems[_ID_LOGTEXT].Tag as LogExtraInfo;
							if (infos != null)
							{
								if (infos._bFavorited)
									file.Write("yes");
								else
									file.Write("no");
							}
							else
								file.Write("no");
							
							// Les dico image
							file.Write(",");
							if ((infos!= null) && (infos._dicoImages != null))
							{
								foreach(KeyValuePair<String, KeyValuePair<String, String>> pair in infos._dicoImages)
								{
									// Format :
									// chemin$titre$desc|chemin$titre$desc|...
									file.Write(pair.Key + "$" + pair.Value.Key + "$" + pair.Value.Value);
									file.Write("|");
								}
							}
							
							// Le status du log
							file.Write(",");
							file.Write(item.SubItems[_ID_LOGSTATUS].Text);
						}
						
						// nouvelle ligne
						file.Write(file.NewLine);
						
					}
	        		// on ferme
	        		file.Close();
	            }
			}
			catch (Exception exc)
            {
                
            	_daddy.ShowException("", _daddy.GetTranslator().GetString("FMenuSaveFN"), exc);
            }	
		}
		void TravelTBOnSelectedCachesToolStripMenuItemClick(object sender, EventArgs e)
		{
	
			if (IsSelectionEmpty())
				return;
			
			DialogResult dialogResult = MessageBox.Show(_daddy.GetTranslator().GetString("AskConfirm"),
			                                            _daddy.GetTranslator().GetString("travelTBOnSelectedCachesToolStripMenuItem"),
                            							MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
				GCLogHMI loghmi = new GCLogHMI(_daddy);
                DialogResult res = loghmi.ShowDialog();
                if (res == DialogResult.OK)
                {
                	
                	// On calcule la mise à jour de la colonne de TBs
                	// TB status
		        	int nbdrop = 0;
		            int nbvis = 0;
		            Dictionary< KeyValuePair<string, string>, int> tbs = loghmi.ExportTBs();
		            if (tbs != null)
		            {
			            foreach (KeyValuePair<KeyValuePair<string, string>, int> pair in tbs)
			    		{
			            	if (pair.Value == 1)
			            		nbdrop++;
			            	if (pair.Value == 2)
			            		nbvis++;
			            }
		            }
		            
                	// On applique
                	ListView.SelectedListViewItemCollection collection = lvCachesfn.SelectedItems;
					foreach (ListViewItem item in collection)
		            {
						LogExtraInfo infos = item.SubItems[_ID_LOGTEXT].Tag as LogExtraInfo;
						if (infos != null)
						{
							// On écrase seulement les TBs
							infos._dicoTBs = tbs;
							infos._dicoImages = loghmi.ExportImages();
						}
						else
						{
							// On crée un nouveau
							infos = new LogExtraInfo();
	                    	infos._bUseDateAndType = false;
	                    	infos._bFavorited = false;
	            			infos._dicoTBs = tbs;
	            			infos._dicoImages = loghmi.ExportImages();
			                item.SubItems[_ID_LOGTEXT].Tag = infos;
						}
						// Mise à jour item
						item.SubItems[_ID_TBSTATUS].Text = String.Format(_daddy.GetTranslator().GetString("LblTBStatusTemplate"), nbdrop, nbvis);
	                }
                }
            }
		}
		
		private void fnbtnDisplayLogstatus_Click(object sender, EventArgs e)
        {
        	if (IsSelectionEmpty())
				return;
			
            ListView.SelectedListViewItemCollection collection = lvCachesfn.SelectedItems;
            if ((collection != null) && (collection.Count != 0))
            {
                _daddy.UpdateHttpDefaultWebProxy();
                // On checke que les L/MDP soient corrects
                // Et on récupère les cookies au passage
                CookieContainer cookieJar = _daddy.CheckGCAccount(true, false);
                if (cookieJar == null)
                    return;

                // Et maintenant on lance les logs en boucle
                _daddy._ThreadProgressBarTitle = _daddy.GetTranslator().GetString("DisplayLogStatus");
                _daddy.CreateThreadProgressBarEnh();

                // Wait for the creation of the bar
                while (_daddy._ThreadProgressBar == null)
                {
                    Thread.Sleep(10);
                    Application.DoEvents();
                }
                _daddy._ThreadProgressBar.progressBar1.Maximum = collection.Count;

                List<ListViewItem> items = new List<ListViewItem>();
                List<Geocache> caches = new List<Geocache>();
                foreach (ListViewItem item in collection)
                {

                    try
                    {
                        _daddy._ThreadProgressBar.lblWait.Text = item.SubItems[_ID_CODE].Text + " " + item.SubItems[_ID_NAME].Text;

                        String code = item.SubItems[_ID_CODE].Text;
                        String url = "http://coord.info/" + item.SubItems[_ID_CODE].Text;
                        string result = _daddy.GetCacheHTMLFromClientImpl(url, cookieJar);

                        String logtype = MyTools.GetSnippetFromText("ctl00_ContentBody_GeoNav_logText\">", "<", result);
                        String logdate = MyTools.GetSnippetFromText("ctl00_ContentBody_GeoNav_logDate", "</small>", result);
                        logdate = MyTools.GetSnippetFromText(" title=", "</a>", logdate);
                        logdate = MyTools.GetSnippetFromText(":", "", logdate);
                        logdate = logdate.Trim();

                        // le status du log
                        if (logtype == "")
                            item.SubItems[_ID_LOGSTATUS].Text = _daddy.GetTranslator().GetString("LblLogNoInfo");
                        else
                        {
                            item.SubItems[_ID_LOGSTATUS].Text = logtype + ": " + logdate;
                        }
                        
                        // Plus de détail
                        // sur le log type
                        Geocache geo = null;
                        if (_daddy._caches.ContainsKey(code))
                        	geo = _daddy._caches[code];
						String tmp = MyTools.GetSnippetFromText("ctl00_ContentBody_GeoNav_foundStatus", "ctl00_ContentBody_GeoNav_logTypeImage", result);
						if (tmp != "")
						{
							tmp = MyTools.GetSnippetFromText("/images/logtypes/48/", ".png\" id", tmp);
		                    if (tmp == "2")
		                    {
		                        // On va considérer ça comme un marquage trouvé par l'utilisateur
		                        // si cette information n'était pas déjà présente dans la cache
		                        String owner = ConfigurationManager.AppSettings["owner"];
                        		if (geo != null)
		                        {
		                        	// On a un objet qu'on peut mettre à jour
			                        if (geo.IsFound() == false)
				                    {
			                        	_daddy.DeclareFoundCache(owner, geo);
                        	
			                        	// On met à jour le status des caches
					                    _daddy._cacheStatus.SaveCacheStatus();
					                    // On réaffiche tout proprement
					            		// Better way to do that : only recreate for modified caches
					            		_daddy.RecreateVisualElements(geo, true);
			                        }
		                        }
		                        else
		                        {
		                        	// On n'a pas d'objet, on se contente du cachestatus
		                        	_daddy._cacheStatus.DeclareFoundCache(owner, code);
                                    // On met à jour le status des caches
                                    _daddy._cacheStatus.SaveCacheStatus();
		                        }
		                    }
						}
                    }
                    catch (Exception)
                    {
                    }
                    _daddy._ThreadProgressBar.Step();
                }

                _daddy.KillThreadProgressBarEnh();
            }
        }
		
		private void btnDisplaySelected_Click(object sender, EventArgs e)
        {
            if (IsSelectionEmpty())
				return;
			
            GetCacheInfo(true, true);
            ListView.SelectedListViewItemCollection collection = lvCachesfn.SelectedItems;
            foreach (ListViewItem item in collection)
            {
                if (item.Tag != null)
                {
                    Geocache geo = (Geocache)(item.Tag);
                    _daddy._cacheDetail.LoadPageCache(geo, _daddy._bUseKm, null, false, false);
                }
            }
        }

		void CompleteEmptyCachesToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (IsSelectionEmpty())
				return;
			
            GetCacheInfo(true, true);
		}
		
		void DefineDefaultLogTextToolStripMenuItemClick(object sender, EventArgs e)
		{
			try
			{
				GCLogHMI loghmi = new GCLogHMI(_daddy, GCLogHMI.TemplateType.TextOnly);
				if (loghmi.ShowDialog() == DialogResult.OK)
				{
					// on récupère le texte
					String text = loghmi.txtCommentsloghmi.Text;
					
					// On sauve dans le fichier
					String headerfile = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + GCLogHMI.GetDefaultLogTextFileName();
					System.IO.StreamWriter file = new System.IO.StreamWriter(headerfile, false, Encoding.Default);
					file.Write(text);
					
					// on ferme
	        		file.Close();
	        		
					// on met à jour l'IHM
					EnableOrNoteDefaultLogHeaderDeletion();
				}
			}
			catch(Exception exc)
			{
				_daddy.ShowException("", _daddy.GetTranslator().GetString("defineDefaultLogTextToolStripMenuItem"), exc);
			}
			
		}
		
		void DeleteDefaultLogTextToolStripMenuItemClick(object sender, EventArgs e)
		{
			try
			{
				DialogResult dialogResult = MessageBox.Show(_daddy.GetTranslator().GetString("AskConfirm"),
			                                            _daddy.GetTranslator().GetString("deleteDefaultLogTextToolStripMenuItem"),
                            							MessageBoxButtons.YesNo, MessageBoxIcon.Question);
	            if (dialogResult == DialogResult.Yes)
	            {
					String headerfile = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + GCLogHMI.GetDefaultLogTextFileName();
		        	if (File.Exists(headerfile))
		        		File.Delete(headerfile);
					EnableOrNoteDefaultLogHeaderDeletion();
	            }
			}
			catch(Exception exc)
			{
				_daddy.ShowException("", _daddy.GetTranslator().GetString("deleteDefaultLogTextToolStripMenuItem"), exc);
			}
		}
		
		private void DisplaySelCount()
		{
			int isel = lvCachesfn.SelectedItems.Count;
			int inb = lvCachesfn.Items.Count;
			tsSelStatus.Text = String.Format(_daddy.GetTranslator().GetString("tsSelStatus"),isel, inb);
		}
		
		void LvCachesfnSelectedIndexChanged(object sender, EventArgs e)
		{
			DisplaySelCount();
		}

		void HandlerSave(GCLogHMI loghmi, int pos)
		{
			if (loghmi != null)
			{
				UpdateItemFromLogHMI(loghmi, lvCachesfn.Items[pos]);
			}
		}
			
		void HandlerNext(GCLogHMI loghmi, int pos)
		{
			if (loghmi != null)
			{
				ModifyLogOfOneCacheImpl(lvCachesfn.Items[pos + 1], loghmi);
			}
		}
		
		void HandlerPrevious(GCLogHMI loghmi, int pos)
		{
			if (loghmi != null)
			{
				ModifyLogOfOneCacheImpl(lvCachesfn.Items[pos - 1], loghmi);
			}
		}
    }
    
    /// <summary>
    /// Class to handle extra log info such as TBs visited/dropped, favorite points added
    /// </summary>
    public class LogExtraInfo
    {
    	/// <summary>
    	/// True if _iLogType and _logDate shall be used
    	/// </summary>
    	public bool _bUseDateAndType = false;
    	
    	/// <summary>
    	/// Log type
    	/// </summary>
    	public int _iLogType = -1;
    	
    	/// <summary>
    	/// Log date
    	/// </summary>
    	public DateTime _logDate = DateTime.Now;
    		
    	/// <summary>
    	/// key : TB code, id; value : 1 dropped, 2 visited 
    	/// </summary>
    	public Dictionary< KeyValuePair<string, string>, int> _dicoTBs = null;
    	
    	/// <summary>
    	/// images to attach to the log (key is complete filename, value is (title, description))
    	/// </summary>
    	public Dictionary<String, KeyValuePair<String, String>> _dicoImages = null;
    	
    	/// <summary>
    	/// True if favorite awarded
    	/// </summary>
    	public bool _bFavorited = false;
    }
    
    /// <summary>
	/// This class is an implementation of the 'IComparer' interface.
	/// </summary>
	public class ListViewColumnSorter : IComparer
	{
		/// <summary>
		/// Specifies the column to be sorted
		/// </summary>
		private int ColumnToSort;
		/// <summary>
		/// Specifies the order in which to sort (i.e. 'Ascending').
		/// </summary>
		private SortOrder OrderOfSort;
		/// <summary>
		/// Case insensitive comparer object
		/// </summary>
		private CaseInsensitiveComparer ObjectCompare;
	
		FieldNotesHMI _fnhmi = null;
		
		/// <summary>
		/// Class constructor.  Initializes various elements
		/// </summary>
		public ListViewColumnSorter(FieldNotesHMI fnhmi)
		{
			_fnhmi = fnhmi;
			
			// Initialize the column to '0'
			ColumnToSort = 0;
	
			// Initialize the sort order to 'none'
			OrderOfSort = SortOrder.None;
	
			// Initialize the CaseInsensitiveComparer object
			ObjectCompare = new CaseInsensitiveComparer();
		}
	
		/// <summary>
		/// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
		/// </summary>
		/// <param name="x">First object to be compared</param>
		/// <param name="y">Second object to be compared</param>
		/// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
		public int Compare(object x, object y)
		{
			int compareResult;
			ListViewItem listviewX, listviewY;
	
			// Cast the objects to be compared to ListViewItem objects
			listviewX = (ListViewItem)x;
			listviewY = (ListViewItem)y;
	
			// Compare the two items
			Object a = null;
			Object b = null;
			if (ColumnToSort == _fnhmi._ID_NUM)
			{
				a = Int32.Parse(listviewX.SubItems[ColumnToSort].Text);
				b = Int32.Parse(listviewY.SubItems[ColumnToSort].Text);
			}
			else if (ColumnToSort == _fnhmi._ID_RCRT)
			{
				// On ne trie pas !!!
				return 0;
			}
			else if (ColumnToSort == _fnhmi._ID_DATE)
			{
				a = listviewX.SubItems[ColumnToSort].Tag;
				b = listviewY.SubItems[ColumnToSort].Tag;
			}
			else
			{
				a = listviewX.SubItems[ColumnToSort].Text;
				b = listviewY.SubItems[ColumnToSort].Text;
			}
			
			compareResult = ObjectCompare.Compare(a,b);
				
			// Calculate correct return value based on object comparison
			if (OrderOfSort == SortOrder.Ascending)
			{
				// Ascending sort is selected, return normal result of compare operation
				return compareResult;
			}
			else if (OrderOfSort == SortOrder.Descending)
			{
				// Descending sort is selected, return negative result of compare operation
				return (-compareResult);
			}
			else
			{
				// Return '0' to indicate they are equal
				return 0;
			}
		}
	    
		/// <summary>
		/// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
		/// </summary>
		public int SortColumn
		{
			set
			{
				ColumnToSort = value;
			}
			get
			{
				return ColumnToSort;
			}
		}
	
		/// <summary>
		/// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
		/// </summary>
		public SortOrder Order
		{
			set
			{
				OrderOfSort = value;
			}
			get
			{
				return OrderOfSort;
			}
		}
	    
	}

}
