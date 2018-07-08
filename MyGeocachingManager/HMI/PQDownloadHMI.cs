using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Net;
using SpaceEyeTools;
using SpaceEyeTools.HMI;
using System.Diagnostics;
using System.IO.Compression;
using System.Web;

namespace MyGeocachingManager
{
    /// <summary>
    /// form to download Pocket Queries GPX from official GC.com website
    /// GC.com account and password shall be provided in MGM
    /// </summary>
    public partial class PQDownloadHMI : Form
    {
        MainWindow _daddy = null;
        ImageList _imageList = new ImageList();

        /// <summary>
        /// 
        /// </summary>
        public bool _bAutoDwnloadUpdatedPQsOnStart = false;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="daddy">reference to MainForm, for callbacks</param>
        public PQDownloadHMI(MainWindow daddy)
        {
            _daddy = daddy;
            InitializeComponent();

            this.Text = _daddy.GetTranslator().GetString("LblDownloadPQ");
            fMenuLiveDownloadPQToolStripMenuItem.Text = "&" + _daddy.GetTranslator().GetString("FMenuLiveDownloadPQ");
            downloadAllToolStripMenuItempqmgr.Text = _daddy.GetTranslator().GetString("LblDownloadPQChecked");
            checkAllToolStripMenuItempqmgr.Text = _daddy.GetTranslator().GetString("BtnCheckAll");
            uncheckAllToolStripMenuItempqmgr.Text = _daddy.GetTranslator().GetString("BtnUncheckAll");
            downloadUpdatedToolStripMenuItempqmgr.Text = _daddy.GetTranslator().GetString("LblAutoDownloadUpdatedPQs");
            openPQFolderToolStripMenuItempqmgr.Text = _daddy.GetTranslator().GetString("LblOpenPQFolder");

            listView1pqdownloader.Columns.Add(_daddy.GetTranslator().GetString("LblName"), 200);
            listView1pqdownloader.Columns.Add(_daddy.GetTranslator().GetString("ColSizeMo"), 80);
            listView1pqdownloader.Columns.Add(_daddy.GetTranslator().GetString("HtmlWpts"), 50);
            listView1pqdownloader.Columns.Add(_daddy.GetTranslator().GetString("LblLastGen"), 200);
            listView1pqdownloader.Columns.Add(_daddy.GetTranslator().GetString("LblExistingFile"), 200);

            // create image list and fill it 
            int index = _daddy.getIndexImages("New");
            Image image = _daddy.getImageFromIndex(index);
            _imageList.Images.Add("new", image);
            listView1pqdownloader.SmallImageList = _imageList;

            _daddy.TranslateTooltips(this, null);
        }

        

        /// <summary>
        /// Populate list from main window, using html retrieven from GC.com
        /// </summary>
        /// <param name="htmlcontent">source code of PQ download page</param>
        /// <param name="diagnostic">If true, will only perform a diagnostic of the function</param>
        /// <returns>Number of available PQs</returns>
        public int Populate(String htmlcontent, bool diagnostic)
        {
            String pqdatapath = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "GPX" + Path.DirectorySeparatorChar + "PQ" + Path.DirectorySeparatorChar;
            if (!diagnostic)
                listView1pqdownloader.Items.Clear();

            // Le bloc de PQ à télécharger
            //String tmp = MyTools.GetSnippetFromText("<table id=\"uxOfflinePQTable\"", "<tr class=\"TableFooter\">", htmlcontent);
            // On cherche toutes les PQ
            int success = 0;
            for(int i=1;i<100;i++)
            {
                String pqpattern = "ctl00_ContentBody_PQDownloadList_uxDownloadPQList_ctl" + i.ToString("00") + "_trPQDownloadRow";
                String pq = MyTools.GetSnippetFromText(pqpattern, "</tr>", htmlcontent);
                if (pq == "")
                    break;
                // Le nom de la PQ
                /*
	<td>
                
                    <input type="checkbox" onclick="checkTopCBDL();" value="11355109" id="chk11355109" />
            </td>
	<td>
                1.
            </td>
	<td>
                <img src="/images/icons/16/pocket_query.png" alt="Pocket Query" />
                <a href="/pocket/downloadpq.ashx?g=6ad42890-0908-415b-88a6-b8f5bafc8f60&src=web">
                    Speciale - France - T4.5-5</a>
            </td>
	<td class="AlignRight">
                2.36 MB
            </td>
	<td class="AlignCenter">
                1000
            </td>
	<td>
                30/Jun/2015 (last day available)
            </td>
                 */
                // Le nom
                String name = MyTools.GetSnippetFromText("&src=web\">", "</a>", pq);
                name = MyTools.CleanString(name);
                ListViewItem item = null;
                if (!diagnostic)
                    item = listView1pqdownloader.Items.Add(name);

                // Le lien
                String url = "http://www.geocaching.com" + MyTools.GetSnippetFromText("<a href=\"", "\">", pq);
                if (!diagnostic)
                    item.Tag = url;
                if (url != "")
                    success++;

                // La taille
                String FileSizeInBytes = MyTools.GetSnippetFromText("<td class=\"AlignRight\">", "</td>", pq);
                FileSizeInBytes = MyTools.CleanString(FileSizeInBytes);
                if (!diagnostic)
                    item.SubItems.Add(FileSizeInBytes);

                // Le nombre de points
                String count = MyTools.GetSnippetFromText("<td class=\"AlignCenter\">", "</td>", pq);
                count = MyTools.CleanString(count);
                if (!diagnostic)
                    item.SubItems.Add(count);
                
                // Date de génération
                DateTime dt = DateTime.Now;
                try
                {
                    String dates = MyTools.GetSnippetFromText("<td class=\"AlignCenter\">", ")", pq);
                    if (dates != "")
                    {
                        dates = MyTools.GetSnippetFromText("<td>", "(", dates);
                        dates = MyTools.CleanString(dates);
                    }
                    else
                    {
                        // C'est surement un foutu bookmark !
                        dates = MyTools.GetSnippetFromText("<td class=\"AlignCenter\">", "", pq);
                        dates = MyTools.GetSnippetFromText("<td>", "</td>", dates);
                        dates = MyTools.CleanString(dates);
                    }
                    dt = Convert.ToDateTime(dates);
                    if (!diagnostic)
                        item.SubItems.Add(dt.ToLongDateString());
                }
                catch (Exception)
                {
                    if (!diagnostic)
                        item.SubItems.Add("");
                }

                // Le nom du fichier
                String file_name_pq = MyTools.SanitizeFilename(name);
                String f = pqdatapath + file_name_pq + ".zip";
                if (!diagnostic)
                {
                    if (File.Exists(f))
                    {
                        item.SubItems.Add(File.GetLastWriteTime(f).ToLongDateString());
                        if (dt < File.GetLastWriteTime(f))
                        {
                            item.BackColor = Color.PaleGreen;
                        }
                        else
                            item.ImageIndex = 0;
                    }
                    else
                    {
                        item.SubItems.Add("?");
                        item.ImageIndex = 0;
                    }
                }
            }
            return success;
        }

        private bool CheckDownloadedFile(String pqdatapath, String zipFile)
        {
            bool bOK = true;
            String unpackDirectory = pqdatapath + Guid.NewGuid().ToString();
            if (Directory.Exists(unpackDirectory))
                MyTools.DeleteDirectory(unpackDirectory, true);

            // On va dezipper le fichiers pour verifier que tout va bien
            try
            {
                ZipFile.ExtractToDirectory(zipFile, unpackDirectory);
            }
            catch (Exception)
            {
            	// Ca a rate, le zip est corrompu !
                bOK = false;
            }
           
            // Delete zip file
            MyTools.DeleteDirectory(unpackDirectory, true);

            return bOK;
        }


        private void DownloadPQs(bool onlychecked)
        {
            try
            {
                _daddy.UpdateHttpDefaultWebProxy();
                // On checke que les L/MDP soient corrects
                // Et on récupère les cookies au passage
                CookieContainer cookieJar = _daddy.CheckGCAccount(true, false);
                if (cookieJar == null)
                    return;

                String pqdatapath = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "GPX" + Path.DirectorySeparatorChar + "PQ" + Path.DirectorySeparatorChar;
                if (!Directory.Exists(pqdatapath))
                    Directory.CreateDirectory(pqdatapath);

                // LblDownloadInProgress
                _daddy._ThreadProgressBarTitle = _daddy.GetTranslator().GetString("LblDownloadInProgress");
                _daddy.CreateThreadProgressBarEnh();

                int nbchecked = 0;
                foreach (ListViewItem item in listView1pqdownloader.Items)
                {
                    if ((!onlychecked) || (item.Checked))
                    {
                        nbchecked++;
                    }
                }

                // Wait for the creation of the bar
                while (_daddy._ThreadProgressBar == null)
                {
                    Thread.Sleep(10);
                    Application.DoEvents();
                }
                _daddy._ThreadProgressBar.progressBar1.Maximum = nbchecked;
                _daddy._ThreadProgressBar.lblWait.Text = "";
                String errors = "";
                foreach (ListViewItem item in listView1pqdownloader.Items)
                {
                    if ((!onlychecked) || (item.Checked))
                    {
                        String url = item.Tag as String;
                        _daddy.Log(url);

                        String file_name_pq = MyTools.SanitizeFilename(item.SubItems[0].Text);
                        String ftmp = pqdatapath + Guid.NewGuid().ToString();
                        String f = pqdatapath + file_name_pq + ".zip";
                        if ((!onlychecked) && File.Exists(f))
                        {
                            DateTime dt = Convert.ToDateTime(item.SubItems[3].Text);
                            if (dt < File.GetLastWriteTime(f))
                            {
                                // already good
                                continue;
                            }
                        }

                        _daddy._ThreadProgressBar.lblWait.Text = item.SubItems[0].Text;
                        try
                        {
                            // On charge !!!
                            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
                            objRequest.Proxy = _daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
                            objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
                            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();

                            // on efface le fichier tmp s'il existe
                            if (File.Exists(ftmp))
                                File.Delete(ftmp);

                            using (Stream output = File.OpenWrite(ftmp)) // au lieu de f
                            using (Stream input = objResponse.GetResponseStream())
                            {
                                byte[] buffer = new byte[8192];
                                int bytesRead;
                                while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    output.Write(buffer, 0, bytesRead);
                                }
                                output.Close();
                                input.Close();
                            }

                            // On a telecharge sans probleme
                            // est ce que le fichier est bon ?
                            if (CheckDownloadedFile(pqdatapath, ftmp))
                            {
                                // Tout va bien on remplace le fichier precedent
                                if (File.Exists(f))
                                    File.Delete(f);
                                File.Move(ftmp, f);

                                item.SubItems[4].Text = File.GetLastWriteTime(f).ToLongDateString();
                                item.Checked = false;
                                item.BackColor = Color.PaleGreen;
                                item.ImageIndex = -1;
                            }
                            else
                            {
                                // on efface le fichier telecharge
                                File.Delete(ftmp);
                                // et on logge une erreur
                                _daddy.Log("!!!!!!!! Error downloading PQ (corruption) " + file_name_pq);
                                errors += _daddy.GetTranslator().GetString("ErrorLoad") + " " + file_name_pq + "\r\n";
                                item.SubItems[4].Text = _daddy.GetTranslator().GetString("ErrDownloadFailed");
                            }

                        }
                        catch (Exception ex1)
                        {
                            // Ca a plante on efface le fichier tmp
                            if (File.Exists(ftmp))
                                File.Delete(ftmp);
                            _daddy.Log("!!!!!!!! Error downloading PQ " + file_name_pq);
                            _daddy.Log(MainWindow.GetException("Downloading PQ", ex1));
                            errors += _daddy.GetTranslator().GetString("ErrorLoad") + " " + file_name_pq + "\r\n";
                            item.SubItems[4].Text = _daddy.GetTranslator().GetString("ErrDownloadFailed");
                        }
                        _daddy._ThreadProgressBar.Step();
                        
                    }
                    if (_daddy._ThreadProgressBar._bAbort)
                        break;
                }
                
                _daddy.KillThreadProgressBarEnh();
                _daddy.ReloadPreviouslyLoadedFiles();
                
                if (errors != "")
                {
                    _daddy.MsgActionError(this, errors);
                }
            }
            catch (Exception ex)
            {
                _daddy.KillThreadProgressBarEnh();
                _daddy.ShowException("", "Downloading pocket queries", ex);
            }
        }


        private void checkAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1pqdownloader.Items)
            {
                item.Checked = true;
            }
        }

        private void uncheckAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1pqdownloader.Items)
            {
                item.Checked = false;
            }
        }

        private void downloadAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DownloadPQs(true);
        }

        private void downloadUpdatedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DownloadPQs(false);
        }

        private void openPQFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String pqdatapath = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "GPX" + Path.DirectorySeparatorChar + "PQ" + Path.DirectorySeparatorChar;
            MyTools.StartInNewThread(pqdatapath);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="daddy"></param>
        /// <returns></returns>
        static public bool CreatePQ(MainWindow daddy)
        {
        	try
            {
                daddy.UpdateHttpDefaultWebProxy();
                // On checke que les L/MDP soient corrects
                // Et on récupère les cookies au passage
                CookieContainer cookieJar = daddy.CheckGCAccount(true, false);
                if (cookieJar == null)
                    return false;

                // Valeurs sauvegardées
                String name = "";
                List<String> checkeddays = new List<string>();
                int runtype = 0;
                bool enabledonly = false;
                bool excludefound = false;
                bool excludeowned = false;
                String coord = "";
                int radius = 100;
                List<string> lstvalsdays = new List<string>(new string[] 
            	{
					daddy.GetTranslator().GetString("PQSunday"),
					daddy.GetTranslator().GetString("PQMonday"),
					daddy.GetTranslator().GetString("PQTuesday"),
					daddy.GetTranslator().GetString("PQWednesday"),
					daddy.GetTranslator().GetString("PQThursday"),
					daddy.GetTranslator().GetString("PQFriday"),
					daddy.GetTranslator().GetString("PQSaturday")
            	});
                List<string> lstvalstypes = new List<string>(new string[] 
            	{
						daddy.GetTranslator().GetString("PQOnceUncheck"),
						daddy.GetTranslator().GetString("PQWeekly"),
						daddy.GetTranslator().GetString("PQOnceDelete")
	            	});
            	
                while (true)
                {
                	// on attaque le post
	                String post_response;
	                String url = "https://www.geocaching.com/pocket/gcquery.aspx";
	                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
	                objRequest.Proxy = daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
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
	                MainWindow.GetViewState(post_response, ref __VIEWSTATEFIELDCOUNT, ref __VIEWSTATE, ref __VIEWSTATEGENERATOR);
	                
                	// Préparation des données du POST
	                Dictionary<String, String> post_values = new Dictionary<String, String>();
	                post_values.Add("__EVENTTARGET", "");
	                post_values.Add("__EVENTARGUMENT", "");
	                post_values.Add("__LASTFOCUS", "");
	                
	                List<ParameterObject> lst = new List<ParameterObject>();
	                ParameterObject pobj = null;
	                
	                // 0 Name
	            	lst.Add(new ParameterObject(ParameterObject.ParameterType.String, name, "lbl", daddy.GetTranslator().GetString("PQName")));
	            	
	            	// 1 Day, démarre à dimanche ctl00$ContentBody$cbDays$0
	            	pobj = new ParameterObject(ParameterObject.ParameterType.CheckList, lstvalsdays, "lstvals1", daddy.GetTranslator().GetString("PQDays"));
	            	if (checkeddays.Count != 0)
	            		pobj.ListCheckedValue = checkeddays;
	            	lst.Add(pobj);
	            
	            	// 2 Type de cache ctl00$ContentBody$rbRunOption (1 à 3)
	            	String tooltip = daddy.GetTranslator().GetString("PQOnceUncheck") + "\r\n" + daddy.GetTranslator().GetString("PQWeekly") + "\r\n" + daddy.GetTranslator().GetString("PQOnceDelete");
	            	pobj = new ParameterObject(ParameterObject.ParameterType.List, lstvalstypes, "lstvals2", daddy.GetTranslator().GetString("PQRunType"), tooltip);
	            	pobj.DefaultListValue = lstvalstypes[runtype];
		            lst.Add(pobj);
		            
		            // 3 Is enabled ctl00$ContentBody$cbOptions$13
		            lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, enabledonly, "enabled", daddy.GetTranslator().GetString("PQEnabled")));
		            
		            // 4 Que je n'ai pas trouvée ctl00$ContentBody$cbOptions$0
		            lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, excludefound, "excludefound", daddy.GetTranslator().GetString("PQExcludeFound")));
		            
		            // 5 Que je ne possède pas ctl00$ContentBody$cbOptions$2
		            lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, excludeowned, "excludeowned", daddy.GetTranslator().GetString("PQExcludeOwned")));
		            
		            
		            // 6 Coordinates
		            if (coord == "")
		            {
		            	coord = daddy.GetInitialCoordinates();
		            }
		            lst.Add(new ParameterObject(ParameterObject.ParameterType.Coordinates/*good*/, coord, "latlon",daddy.GetTranslator().GetString("ParamCenterLatLon"),daddy.GetTranslator().GetStringM("TooltipParamLatLon")));
	            
		            // 7 Radius
		            String unit = "";
		            if (daddy._bUseKm)
		            {
		                unit = daddy.GetTranslator().GetString("LVKm");
		            }
		            else
		            {
		                unit = daddy.GetTranslator().GetString("LVMi");
		            }
		            lst.Add(new ParameterObject(ParameterObject.ParameterType.Radius, radius, "dist", daddy.GetTranslator().GetString("PQRadius") + " " + unit));
		            
		            // On lance
		            ParametersChanger changer = new ParametersChanger();
		            changer.HandlerDisplayCoord = daddy.HandlerToDisplayCoordinates;
		            changer.DisplayCoordImage = daddy.GetImageSized("Earth");
		            changer.Title = daddy.GetTranslator().GetString("createPQToolStripMenuItem");
		            changer.BtnCancel = daddy.GetTranslator().GetString("BtnCancel");
		            changer.BtnOK = daddy.GetTranslator().GetString("BtnOk");
		            changer.ErrorFormater = daddy.GetTranslator().GetString("ErrWrongParameter");
		            changer.ErrorTitle = daddy.GetTranslator().GetString("Error");
		            changer.Parameters = lst;
		            changer.Font = daddy.Font;
		            changer.Icon = daddy.Icon;
		 
		            // Force creation du get handler on control
	            	changer.CreateControls();
	            	daddy._cacheDetail._gmap.ControlTextLatLon = changer.CtrlCallbackCoordinates;
	            	daddy._cacheDetail._gmap.ControlTextRadius = changer.CtrlCallbackRadius;
						
		            if (changer.ShowDialog() == DialogResult.OK)
		            {
		                daddy._cacheDetail._gmap.ControlTextLatLon = null;
		                daddy._cacheDetail._gmap.ControlTextRadius = null;
		                // Le submit
		                name = lst[0].Value;
		                post_values.Add("ctl00$ContentBody$tbName", name);
		                checkeddays.Clear();
		                foreach(var day in lst[1].ValueIndexes)
		                {
		                	checkeddays.Add(lstvalsdays[day]);
			                post_values.Add("ctl00$ContentBody$cbDays$" + day.ToString(), "checked");
		                }
		                runtype = lst[2].ValueIndex;
		                post_values.Add("ctl00$ContentBody$rbRunOption", (runtype + 1).ToString());
		                
		                if (lst[3].Value == "True")
		                {
		                	enabledonly = true;
		                	post_values.Add("ctl00$ContentBody$cbOptions$13", "checked");
		                }
		                else
		                	enabledonly = false;
		                
		                if (lst[4].Value == "True")
		                {
		                	excludefound = true;
		                	post_values.Add("ctl00$ContentBody$cbOptions$0", "checked");
		                }
		                else
		                	excludefound = false;
		                
		                if (lst[5].Value == "True")
		                {
		                	excludeowned = true;
		                	post_values.Add("ctl00$ContentBody$cbOptions$2", "checked");
		                }
		                else
		                	excludeowned = false;
		                
		                post_values.Add("ctl00$ContentBody$tbResults", "1000");
		                post_values.Add("ctl00$ContentBody$Type", "rbTypeAny");
		                post_values.Add("ctl00$ContentBody$Container", "rbContainerAny");
		                post_values.Add("ctl00$ContentBody$Origin", "rbOriginWpt");
		                post_values.Add("ctl00$ContentBody$LatLong", "0");
		                double dlon = 0.0;
		                double dlat = 0.0;
		                coord = lst[6].Value;
		                if (ParameterObject.SplitLongitudeLatitude(coord, ref dlon, ref dlat))
	                	{
		                	if (dlon < 0.0)
		                	{
		                		// West
		                		post_values.Add("ctl00$ContentBody$LatLong:_selectEastWest", "-1");
		                		post_values.Add("ctl00$ContentBody$LatLong$_inputLongDegs", Math.Abs(dlon).ToString().Replace(',','.'));
		                	}
		                	else
		                	{
		                		// East
		                		post_values.Add("ctl00$ContentBody$LatLong:_selectEastWest", "1");
		                		post_values.Add("ctl00$ContentBody$LatLong$_inputLongDegs", Math.Abs(dlon).ToString().Replace(',','.'));
		                		
		                	}
		                	if (dlat < 0.0)
		                	{
		                		// South 
		                		post_values.Add("ctl00$ContentBody$LatLong:_selectNorthSouth", "-1");
		                		post_values.Add("ctl00$ContentBody$LatLong$_inputLatDegs", Math.Abs(dlat).ToString().Replace(',','.'));
		                	}
		                	else
		                	{
		                		// North 
		                		post_values.Add("ctl00$ContentBody$LatLong:_selectNorthSouth", "1");
		                		post_values.Add("ctl00$ContentBody$LatLong$_inputLatDegs", Math.Abs(dlat).ToString().Replace(',','.'));
		                	}
		                }
		                if (!Int32.TryParse(lst[7].Value, out radius))
		                	radius = -1;
		                post_values.Add("ctl00$ContentBody$tbRadius", lst[7].Value);
		                if (daddy._bUseKm)
		                	post_values.Add("ctl00$ContentBody$rbUnitType", "km");
		                else
		                	post_values.Add("ctl00$ContentBody$rbUnitType", "mi");
		                
		                
		                post_values.Add("ctl00$ContentBody$ddFormats", "GPX");
		                post_values.Add("ctl00$ContentBody$cbIncludePQNameInFileName", "checked");
		                post_values.Add("ctl00$ContentBody$btnSubmit", "Submit Information");
						
		                
		
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
		                objRequest.Proxy = daddy.GetProxy(); // Créer votre proxy ici si besoin, sinon mettre NULL
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
		                    post_response = responseStream.ReadToEnd();
		                    responseStream.Close();
		                }
		                
		                // On récupère les viewstates pour un éventuel retry
		                __VIEWSTATEFIELDCOUNT = "";
		                __VIEWSTATE = null;
		                __VIEWSTATEGENERATOR = "";
		                MainWindow.GetViewState(post_response, ref __VIEWSTATEFIELDCOUNT, ref __VIEWSTATE, ref __VIEWSTATEGENERATOR);
	
		                //daddy._cacheDetail.LoadPageText("dbg1", post_response, true);
		                // On a soit un succés, soit un warning
		                // Dans tous les cas on demande soit de valider (rien à faire)
		                // Soit de modifier, dans ce cas là on supprime et on relance avec les bons paramètres
		                String resultat = "";
		                bool error = false;
		                GetFeedbackFromPostResponse(daddy, post_response, ref resultat, ref error, lst[1].ValueIndexes, lstvalsdays);
		                
		                /*
		                String error = MyTools.GetSnippetFromText("ctl00_ContentBody_ValidationSummary1", "</div>", post_response);
	    				error = MyTools.GetSnippetFromText("<ul>", "</ul>", error);
	    				if (error != "")
	    				{
	    					resultat = HtmlAgilityPack.HtmlEntity.DeEntitize(error);
	    				}
	    				else
	    				{
	    					// C'est un succés alors ?
	    					// On regarde tout de même si tous les jours demandés sont cochés
	    					String uncheckeddays = "";
	    					foreach(var day in lst[1].ValueIndexes)
			                {
	    						String key = "ctl00$ContentBody$cbDays$" + day.ToString();
	    						String val = MyTools.GetSnippetFromText(key, "/>", post_response);
	    						if (!val.Contains("checked=\"checked\""))
	    						{
	    							// Ca a raté
	    							uncheckeddays += lstvalsdays[day] + "\r\n";
	    						}
			                }
	    					if (uncheckeddays != "")
	    					{
	    						uncheckeddays = daddy.GetTranslator().GetString("PQDaysNotAccepted") + "\r\n" + uncheckeddays;
	    					}
	    					
	    					String warning = MyTools.GetSnippetFromText("<p class=\"Warning\">", ".", post_response);
	    					String success = MyTools.GetSnippetFromText("<p class=\"Success\">", ".", post_response);
	    					resultat = (warning != "")?warning+"\r\n":"";
	    					resultat += uncheckeddays;
	    					resultat += success;
	    					HtmlAgilityPack.HtmlEntity.DeEntitize(resultat);
	    				}*/
		                bool splitproposal = false;
		                if (resultat.Contains("1000"))
		                {
		                	resultat += "\r\n\r\n" + daddy.GetTranslator().GetStringM("LblSplitNotice");
		                	splitproposal = true;
		                }
	    				DialogResult dialogResult = MessageBox.Show(daddy.GetTranslator().GetStringM("AskPQRetry") + "\r\n" + resultat,
	                        daddy.GetTranslator().GetString("AskPQRetryTitle"),
	                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
	            		if (dialogResult == DialogResult.Yes)
	            		{
	            			// On va ressayer
	            			// On supprimer l'ancienne PQ
	            			post_response = DeletePQAndRetry(daddy, cookieJar, url, __VIEWSTATEFIELDCOUNT, __VIEWSTATE, __VIEWSTATEGENERATOR);
	            			
	            			// On repart pour un tour
	            		}
	            		else
	            		{
	            			// On supprime si on est en échec uniquement
	            			if (error)
	            			{
	            				// On supprime
	            				post_response = DeletePQAndRetry(daddy, cookieJar, url, __VIEWSTATEFIELDCOUNT, __VIEWSTATE, __VIEWSTATEGENERATOR);
	            				
	            				daddy.MsgActionError(daddy, daddy.GetTranslator().GetStringM("PQDeleted"));
	            				return false;
	            			}
	            			else
	            			{
	            				if (splitproposal)
	            				{
	            					dialogResult = MessageBox.Show(String.Format(daddy.GetTranslator().GetStringM("AskPQ1000"),name),
			                        daddy.GetTranslator().GetString("AskPQ1000Title"),
			                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				            		if (dialogResult == DialogResult.Yes)
				            		{
				            			// On supprime
				            			post_response = DeletePQAndRetry(daddy, cookieJar, url, __VIEWSTATEFIELDCOUNT, __VIEWSTATE, __VIEWSTATEGENERATOR);
				            			
				            			// on va tenter de découper en ajoutant le critère des jours
				            			return CreateSplitPQ(daddy, name, runtype, enabledonly, excludefound, excludeowned, coord, radius);
				            		}
				            		else
				            		{
				            			// tout va bien
										daddy.MsgActionDone(daddy);
										return true;
				            		}
	            				}
	            				else
	            				{
		            				// tout va bien
		            				daddy.MsgActionDone(daddy);
		            				return true;
	            				}
	            			}
	            		}
		            }
		            else
		            {
		            	daddy._cacheDetail._gmap.ControlTextLatLon = null;
		                daddy._cacheDetail._gmap.ControlTextRadius = null;
		                daddy.MsgActionCanceled(daddy);
		            	return false;
		            }
                }
            }
            catch(Exception ex)
            {
            	daddy._cacheDetail._gmap.ControlTextLatLon = null;
		        daddy._cacheDetail._gmap.ControlTextRadius = null;
            	daddy.ShowException("", daddy.GetTranslator().GetString("createPQToolStripMenuItem"), ex);
                return false;
            }
        }
        
        static void GetFeedbackFromPostResponse(MainWindow daddy, String post_response, ref String feedback, ref bool iserror, List<int> checkeddays, List<String> lstvalsdays)
        {
        	
        	feedback = "";
        	String error = MyTools.GetSnippetFromText("ctl00_ContentBody_ValidationSummary1", "</div>", post_response);
			error = MyTools.GetSnippetFromText("<ul>", "</ul>", error);
			if (error != "")
			{
				feedback = HtmlAgilityPack.HtmlEntity.DeEntitize(error);
				iserror = true;
			}
			else
			{
				iserror = false;
				// C'est un succés alors ?
				// On regarde tout de même si tous les jours demandés sont cochés
				String uncheckeddays = "";
				if (checkeddays != null)
				{
					foreach(var day in checkeddays)
	                {
						String key = "ctl00$ContentBody$cbDays$" + day.ToString();
						String val = MyTools.GetSnippetFromText(key, "/>", post_response);
						if (!val.Contains("checked=\"checked\""))
						{
							// Ca a raté
							uncheckeddays += lstvalsdays[day] + "\r\n";
						}
	                }
				}
				if (uncheckeddays != "")
				{
					uncheckeddays = daddy.GetTranslator().GetString("PQDaysNotAccepted") + "\r\n" + uncheckeddays;
				}
				
				String warning = MyTools.GetSnippetFromText("<p class=\"Warning\">", ".", post_response);
				String success = MyTools.GetSnippetFromText("<p class=\"Success\">", ".", post_response);
				feedback = (warning != "")?warning+"\r\n":"";
				feedback += uncheckeddays;
				feedback += success;
				HtmlAgilityPack.HtmlEntity.DeEntitize(feedback);
			}
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="daddy"></param>
        /// <param name="pqurl"></param>
        /// <returns></returns>
        static public bool DeletePQ(MainWindow daddy, String pqurl)
        {
        	try
        	{
        		// On checke que les L/MDP soient corrects
                // Et on récupère les cookies au passage
                CookieContainer cookieJar = daddy.CheckGCAccount(true, false);
                if (cookieJar == null)
                    return false;
        	
                // on attaque le post
                String post_response;
                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(pqurl);
                objRequest.Proxy = daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
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
                MainWindow.GetViewState(post_response, ref __VIEWSTATEFIELDCOUNT, ref __VIEWSTATE, ref __VIEWSTATEGENERATOR);
                
            	// Préparation des données du POST
                Dictionary<String, String> post_values = new Dictionary<String, String>();
                post_values.Add("__EVENTTARGET", "");
                post_values.Add("__EVENTARGUMENT", "");
                post_values.Add("__LASTFOCUS", "");
            	
                // La demande de suppression
                post_values.Add("ctl00$ContentBody$btnDelete", "Delete This Query");
                
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
                objRequest = (HttpWebRequest)WebRequest.Create(pqurl);
                objRequest.Method = "POST";
                objRequest.ContentLength = post_string.Length;
                objRequest.ContentType = "application/x-www-form-urlencoded";
                objRequest.Proxy = daddy.GetProxy(); // Créer votre proxy ici si besoin, sinon mettre NULL
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
                    post_response = responseStream.ReadToEnd();
                    responseStream.Close();
                }
                
        		return true;
        	}
        	catch(Exception)
        	{
        		return false;
        	}
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="daddy"></param>
        /// <param name="name"></param>
        /// <param name="runtype"></param>
        /// <param name="enabledonly"></param>
        /// <param name="excludefound"></param>
        /// <param name="excludeowned"></param>
        /// <param name="coord"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        static public bool CreateSplitPQ(MainWindow daddy, String name, int runtype, bool enabledonly, bool excludefound, bool excludeowned, String coord, int radius)
        {
        	try
        	{
        		int pbnumber = 1;
        		// On checke que les L/MDP soient corrects
                // Et on récupère les cookies au passage
                CookieContainer cookieJar = daddy.CheckGCAccount(true, false);
                if (cookieJar == null)
                    return false;

                bool goon = true;
                                
                // Les dates pour la recherche
                // Première cache en France
                DateTime deb = new DateTime(2000, 01, 01);//new DateTime(2001, 04, 22);
                DateTime now = DateTime.Now;
                DateTime bound = now;
                DateTime boundup = now;
                
                // Et maintenant on lance les logs en boucle
                daddy._ThreadProgressBarTitle = daddy.GetTranslator().GetString("LblSplitPQ");
	            daddy.CreateThreadProgressBar();
	            
	            // Wait for the creation of the bar
	            while (daddy._ThreadProgressBar == null)
	            {
	                Thread.Sleep(10);
	                Application.DoEvents();
	            }
	            daddy._ThreadProgressBar.btnAbort.Enabled = true;
	            
                // Dichotomie
                // On initialise à la moitié
                bound = MyTools.GetMiddleDate(deb, boundup);
                
                bool forcestop = false;
                List<int> pq_results = new List<int>();
                while (goon)
                {
                	if (daddy._ThreadProgressBar._bAbort)
                	{
                		daddy.KillThreadProgressBar();
                		daddy.MsgActionCanceled(daddy);
                		return false;
                	}
                	
                	daddy._ThreadProgressBar.lblWait.Text = deb.ToShortDateString() + " -> " + bound.ToShortDateString();
	            	
                	// on attaque le post
	                String post_response;
	                String url = "https://www.geocaching.com/pocket/gcquery.aspx";
	                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
	                objRequest.Proxy = daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
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
	                MainWindow.GetViewState(post_response, ref __VIEWSTATEFIELDCOUNT, ref __VIEWSTATE, ref __VIEWSTATEGENERATOR);
	                
                	// Préparation des données du POST
	                Dictionary<String, String> post_values = new Dictionary<String, String>();
	                post_values.Add("__EVENTTARGET", "");
	                post_values.Add("__EVENTARGUMENT", "");
	                post_values.Add("__LASTFOCUS", "");
	            	
	                // Le submit
	                post_values.Add("ctl00$ContentBody$tbName", name + String.Format("_{0:00}", pbnumber));
	                post_values.Add("ctl00$ContentBody$rbRunOption", (runtype + 1).ToString());
	                if (enabledonly)
	                	post_values.Add("ctl00$ContentBody$cbOptions$13", "checked");
	                if (excludefound)
		                post_values.Add("ctl00$ContentBody$cbOptions$0", "checked");
	                if (excludeowned)
	                	post_values.Add("ctl00$ContentBody$cbOptions$2", "checked");
	                post_values.Add("ctl00$ContentBody$tbResults", "1000");
	                post_values.Add("ctl00$ContentBody$Type", "rbTypeAny");
	                post_values.Add("ctl00$ContentBody$Container", "rbContainerAny");
	                post_values.Add("ctl00$ContentBody$Origin", "rbOriginWpt");
	                post_values.Add("ctl00$ContentBody$LatLong", "0");
	                double dlon = 0.0;
	                double dlat = 0.0;
	                if (ParameterObject.SplitLongitudeLatitude(coord, ref dlon, ref dlat))
                	{
	                	if (dlon < 0.0)
	                	{
	                		// West
	                		post_values.Add("ctl00$ContentBody$LatLong:_selectEastWest", "-1");
	                		post_values.Add("ctl00$ContentBody$LatLong$_inputLongDegs", Math.Abs(dlon).ToString().Replace(',','.'));
	                	}
	                	else
	                	{
	                		// East
	                		post_values.Add("ctl00$ContentBody$LatLong:_selectEastWest", "1");
	                		post_values.Add("ctl00$ContentBody$LatLong$_inputLongDegs", Math.Abs(dlon).ToString().Replace(',','.'));
	                		
	                	}
	                	if (dlat < 0.0)
	                	{
	                		// South 
	                		post_values.Add("ctl00$ContentBody$LatLong:_selectNorthSouth", "-1");
	                		post_values.Add("ctl00$ContentBody$LatLong$_inputLatDegs", Math.Abs(dlat).ToString().Replace(',','.'));
	                	}
	                	else
	                	{
	                		// North 
	                		post_values.Add("ctl00$ContentBody$LatLong:_selectNorthSouth", "1");
	                		post_values.Add("ctl00$ContentBody$LatLong$_inputLatDegs", Math.Abs(dlat).ToString().Replace(',','.'));
	                	}
	                }
	                post_values.Add("ctl00$ContentBody$tbRadius", radius.ToString());
	                if (daddy._bUseKm)
	                	post_values.Add("ctl00$ContentBody$rbUnitType", "km");
	                else
	                	post_values.Add("ctl00$ContentBody$rbUnitType", "mi");

	                // Date de début
	                post_values.Add("ctl00$ContentBody$Placed", "rbPlacedBetween");
	                post_values.Add("ctl00$ContentBody$DateTimeBegin", "true");
	                post_values.Add("ctl00$ContentBody$DateTimeBegin$Month", deb.Month.ToString());
	                post_values.Add("ctl00$ContentBody$DateTimeBegin$Day", deb.Day.ToString());
	                post_values.Add("ctl00$ContentBody$DateTimeBegin$Year", deb.Year.ToString());
	                post_values.Add("ctl00$ContentBody$DateTimeEnd", "true");
	                post_values.Add("ctl00$ContentBody$DateTimeEnd$Month", bound.Month.ToString());
	                post_values.Add("ctl00$ContentBody$DateTimeEnd$Day", bound.Day.ToString());
	                post_values.Add("ctl00$ContentBody$DateTimeEnd$Year", bound.Year.ToString());

	                // Date de fin
	                
	                post_values.Add("ctl00$ContentBody$ddFormats", "GPX");
	                post_values.Add("ctl00$ContentBody$cbIncludePQNameInFileName", "checked");
	                post_values.Add("ctl00$ContentBody$btnSubmit", "Submit Information");
					
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
	                objRequest.Proxy = daddy.GetProxy(); // Créer votre proxy ici si besoin, sinon mettre NULL
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
	                    post_response = responseStream.ReadToEnd();
	                    responseStream.Close();
	                }
	                
	                if (forcestop)
	                {
	                	// On arrête là, pas besoin d'analyser le résultat
	                	goon = false;
	                }
	                else
	                {
		                // On récupère les viewstates pour une éventuelle suite
		                __VIEWSTATEFIELDCOUNT = "";
		                __VIEWSTATE = null;
		                __VIEWSTATEGENERATOR = "";
		                MainWindow.GetViewState(post_response, ref __VIEWSTATEFIELDCOUNT, ref __VIEWSTATE, ref __VIEWSTATEGENERATOR);
		                
		                // Analyse du résultat
		                String resultat = "";
		                bool error = false;
		                GetFeedbackFromPostResponse(daddy, post_response, ref resultat, ref error, null, null);
		                
		                if (error)
		                {
			                // On supprime, y'a un truc qui a merdé
							post_response = DeletePQAndRetry(daddy, cookieJar, url, __VIEWSTATEFIELDCOUNT, __VIEWSTATE, __VIEWSTATEGENERATOR);
							
							daddy.MsgActionError(daddy, daddy.GetTranslator().GetStringM("PQDeleted") + "\r\n" + resultat);
							daddy.KillThreadProgressBar();
							return false;
		                }
		                else
		                {
		                	String nbs = MyTools.CleanStringOfNonDigits(resultat);
		                	int nb = 0;
		                	if (Int32.TryParse(nbs, out nb))
		                	{
		                		if (nb > 1000) // WTF ???
		                		{
		                			// On supprime, y'a un truc qui a merdé
									post_response = DeletePQAndRetry(daddy, cookieJar, url, __VIEWSTATEFIELDCOUNT, __VIEWSTATE, __VIEWSTATEGENERATOR);
									
									daddy.MsgActionError(daddy, daddy.GetTranslator().GetStringM("PQDeleted") + "\r\n" + resultat);
									daddy.KillThreadProgressBar();
									return false;
		                		}
		                		else if  (nb == 1000)
		                		{
		                			daddy._ThreadProgressBar.lblWait.Text = deb.ToShortDateString() + " -> " + bound.ToShortDateString() + " : " + nb.ToString();
		                			// On supprime c'est trop de résultat
									post_response = DeletePQAndRetry(daddy, cookieJar, url, __VIEWSTATEFIELDCOUNT, __VIEWSTATE, __VIEWSTATEGENERATOR);
									
									// Cas d'arrêt : si deb = bound, on a plus de 1000 caches sur une journée, j'arrête
									if (bound.ToShortDateString() == deb.ToShortDateString())
									{
										daddy.MsgActionError(daddy, String.Format(daddy.GetTranslator().GetStringM("LblMoreThan1000InaDay"), bound.ToShortDateString()));
										daddy.KillThreadProgressBar();
										return false;
									}
									
		                			// On doit diviser, on garde la borne basse
		                			boundup = bound; // Voici la nouvelle limite haute pour cette itération
		                			bound = MyTools.GetMiddleDate(deb, boundup);
		                			goon = true;
		                		}
		                		else if ((nb >= 800) && (nb <= 1000))
		                		{
		                			daddy._ThreadProgressBar.lblWait.Text = deb.ToShortDateString() + " -> " + bound.ToShortDateString() + " : " + nb.ToString();
		                			// Si la borne haute est aujourd'hui on stoppe enfin
		                			if ((now.Day <= bound.Day) && (now.Month <= bound.Month) && (now.Year <= bound.Year))
		                			{
		                				pq_results.Add(nb);
		                				// On arrête au prochain coup, mais là on va changer la date de fin à un truc très loin
		                				bound = new DateTime(now.Year + 1, now.Month, now.Day);
		                				forcestop = true;
		                				// On force la suppression de la dernière PQ que l'on va recréer
		                				post_response = DeletePQAndRetry(daddy, cookieJar, url, __VIEWSTATEFIELDCOUNT, __VIEWSTATE, __VIEWSTATEGENERATOR);
		                				
		                				//goon = false;
		                			}
		                			else
		                			{
		                				pq_results.Add(nb);
		                				// On  garde !!!
			                			pbnumber++;
			                			
		                				goon = true;
		                			
			                			// La nouvelle borne basse
			                			deb = bound.AddDays(1); // On démarre après la borne haute
			                			// On rétablit la borne haute
			                			boundup = now;
			                			// on se fixe sur la borne haute
			                			bound = boundup;
		                			}
		                		}
		                		else if (nb < 800)
		                		{
		                			daddy._ThreadProgressBar.lblWait.Text = deb.ToShortDateString() + " -> " + bound.ToShortDateString() + " : " + nb.ToString();
		                			// On supprime c'est trop peu de résultat à moins que la borne de fin soit aujourd'hui !
		                			if ((now.Day <= bound.Day) && (now.Month <= bound.Month) && (now.Year <= bound.Year))
		                			{
		                				pq_results.Add(nb);
		                				// On arrête au prochain coup, mais là on va changer la date de fin à un truc très loin
		                				bound = new DateTime(now.Year + 1, now.Month, now.Day);
		                				forcestop = true;
		                				// On force la suppression de la dernière PQ que l'on va recréer
		                				post_response = DeletePQAndRetry(daddy, cookieJar, url, __VIEWSTATEFIELDCOUNT, __VIEWSTATE, __VIEWSTATEGENERATOR);
		                				
		                				//goon = false;
		                			}
		                			else
		                			{
										post_response = DeletePQAndRetry(daddy, cookieJar, url, __VIEWSTATEFIELDCOUNT, __VIEWSTATE, __VIEWSTATEGENERATOR);
										
			                			// On doit augmenter la borne haute mais sans dépasser boundup
			                			bound = MyTools.GetMiddleDate(bound, boundup);
			                			goon = true;
		                			}
		                		}
		                	}
		                }
	                }
                }
                daddy.KillThreadProgressBar();
                String msg = daddy.GetTranslator().GetStringM("LblSplitResult");
                int n = 1;
                foreach(var nb in pq_results)
                {
                	msg += name + String.Format("_{0:00} = {1} caches\r\n", n++, nb);
                }
                
                daddy.MsgActionDone(daddy, "\r\n" + msg);
        		return true;
        	}
        	catch(Exception ex)
        	{
        		daddy.KillThreadProgressBar();
        		daddy.ShowException("", daddy.GetTranslator().GetString("createPQToolStripMenuItem"), ex);
                return false;
        	}
        		
        }
        
        static private String DeletePQAndRetry(MainWindow daddy, CookieContainer cookieJar, String url, String __VIEWSTATEFIELDCOUNT, String[] __VIEWSTATE, String __VIEWSTATEGENERATOR)
        {
        	var post_values = new Dictionary<String, String>();
            post_values.Add("__EVENTTARGET", "");
            post_values.Add("__EVENTARGUMENT", "");
            post_values.Add("__LASTFOCUS", "");
            
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
            post_values.Add("ctl00$ContentBody$btnDelete", "Delete This Query");
            
            var post_string = "";
            foreach (KeyValuePair<String, String> post_value in post_values)
            {
                post_string += post_value.Key + "=" + HttpUtility.UrlEncode(post_value.Value) + "&";
            }
            post_string = post_string.TrimEnd('&');

            // Création de la requête pour s'authentifier
            var objRequest = (HttpWebRequest)WebRequest.Create(url);
            objRequest.Method = "POST";
            objRequest.ContentLength = post_string.Length;
            objRequest.ContentType = "application/x-www-form-urlencoded";
            objRequest.Proxy = daddy.GetProxy(); // Créer votre proxy ici si besoin, sinon mettre NULL
            objRequest.CookieContainer = cookieJar;

            // on envoit les POST data dans un stream (écriture)
            StreamWriter myWriter = null;
            myWriter = new StreamWriter(objRequest.GetRequestStream());
            myWriter.Write(post_string);
            myWriter.Close();

            // lecture du stream de réponse et conversion en chaine
            var objResponse = (HttpWebResponse)objRequest.GetResponse();
            var post_response = "";
            using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
            {
                post_response = responseStream.ReadToEnd();
                responseStream.Close();
            }
            return post_response;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="daddy"></param>
        /// <returns></returns>
        static public bool CreatePQDate(MainWindow daddy)
        {
        	try
            {
            	DateTime deb = DateTime.Now;
				DateTime fin = DateTime.Now;
            	List<ParameterObject> lst = new List<ParameterObject>();
            	lst.Add(new ParameterObject(ParameterObject.ParameterType.String, "", "search", daddy.GetTranslator().GetString("PQName")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Date, deb, "deb", daddy.GetTranslator().GetString("LblStart")));
                lst.Add(new ParameterObject(ParameterObject.ParameterType.Date, fin, "fin", daddy.GetTranslator().GetString("LblEnd")));

                ParametersChanger changer = new ParametersChanger();
                changer.Title = daddy.GetTranslator().GetString("createPQDateToolStripMenuItem");
                changer.BtnCancel = daddy.GetTranslator().GetString("BtnCancel");
                changer.BtnOK = daddy.GetTranslator().GetString("BtnOk");
                changer.ErrorFormater = daddy.GetTranslator().GetString("ErrWrongParameter");
                changer.ErrorTitle = daddy.GetTranslator().GetString("Error");
                changer.Parameters = lst;
                changer.Font = daddy.Font;
                changer.Icon = daddy.Icon;

                if (changer.ShowDialog() == DialogResult.OK)
                {
                	String name = lst[0].Value;
                    deb = (DateTime)(lst[1].ValueO);
                    deb = new DateTime(deb.Year, deb.Month, deb.Day, 0, 0, 0);
                    fin = (DateTime)(lst[2].ValueO);
                    fin = new DateTime(fin.Year, fin.Month, fin.Day, 23, 59, 59);

                    if (deb > fin)
                    {
                        daddy.MsgActionError(daddy, daddy.GetTranslator().GetString("LblErrStartEnd"));
                        return false;
                    }
                    
                    // On crée
                    if (CreatePQDate(daddy, name,
                                     deb.Month.ToString(), deb.Day.ToString(), deb.Year.ToString(),
                                     fin.Month.ToString(), fin.Day.ToString(), fin.Year.ToString()))
                    {
	                    daddy.MsgActionDone(daddy);
	                    return true;
                    }
                    else
                    {
                    	daddy.MsgActionError(daddy, daddy.GetTranslator().GetString("createPQDateToolStripMenuItem"));
	                    return false;
                    }
                }
                else
                {
                	daddy.MsgActionCanceled(daddy);
                	return false;
                }
           
            }
            catch (Exception ex)
            {
            	daddy.ShowException("", daddy.GetTranslator().GetString("createPQDateToolStripMenuItem"), ex);
            	return false;
            }
        }
        
        static private bool CreatePQDate(MainWindow daddy, String label, String mdeb, String ddeb, String ydeb, String mend, String dend, String yend)
        {
            try
            {
                daddy.UpdateHttpDefaultWebProxy();
                // On checke que les L/MDP soient corrects
                // Et on récupère les cookies au passage
                CookieContainer cookieJar = daddy.CheckGCAccount(true, false);
                if (cookieJar == null)
                    return false;

                // on attaque le post
                String post_response;
                String url = "https://www.geocaching.com/pocket/gcquery.aspx";
                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
                objRequest.Proxy = daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
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
                MainWindow.GetViewState(post_response, ref __VIEWSTATEFIELDCOUNT, ref __VIEWSTATE, ref __VIEWSTATEGENERATOR);

                // Préparation des données du POST
                Dictionary<String, String> post_values = new Dictionary<String, String>();
                post_values.Add("__EVENTTARGET", "");
                post_values.Add("__EVENTARGUMENT", "");
                post_values.Add("__LASTFOCUS", "");

                // Le submit
                post_values.Add("ctl00$ContentBody$tbName", label);
                post_values.Add("ctl00$ContentBody$rbRunOption", "1");
                post_values.Add("ctl00$ContentBody$tbResults", "1000");
                post_values.Add("ctl00$ContentBody$Type", "rbTypeAny");
                post_values.Add("ctl00$ContentBody$Container", "rbContainerAny");
                post_values.Add("ctl00$ContentBody$CountryState", "rbCountries");
                post_values.Add("ctl00$ContentBody$lbCountries", "73"); // France
                post_values.Add("ctl00$ContentBody$Origin", "rbOriginNone");
                post_values.Add("ctl00$ContentBody$ddFormats", "GPX");
                post_values.Add("ctl00$ContentBody$cbIncludePQNameInFileName", "checked");
                post_values.Add("ctl00$ContentBody$btnSubmit", "Submit Information");
                post_values.Add("ctl00$ContentBody$Placed", "rbPlacedBetween");
                post_values.Add("ctl00$ContentBody$DateTimeBegin", "true");
                post_values.Add("ctl00$ContentBody$DateTimeBegin$Month", mdeb);
                post_values.Add("ctl00$ContentBody$DateTimeBegin$Day", ddeb);
                post_values.Add("ctl00$ContentBody$DateTimeBegin$Year", ydeb);
                post_values.Add("ctl00$ContentBody$DateTimeEnd", "true");
                post_values.Add("ctl00$ContentBody$DateTimeEnd$Month", mend);
                post_values.Add("ctl00$ContentBody$DateTimeEnd$Day", dend);
                post_values.Add("ctl00$ContentBody$DateTimeEnd$Year", yend);

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
                objRequest.Proxy = daddy.GetProxy(); // Créer votre proxy ici si besoin, sinon mettre NULL
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
                    post_response = responseStream.ReadToEnd();
                    responseStream.Close();
                }
                //_cacheDetail.LoadPageText("dbg1", post_response, true);
                if (post_response.Contains("<p class=\"Success\">"))
                    return true;
                else
                    return false;
            }
            catch(Exception)
            {
                return false;
            }
        }

        static private int GetMonth(String mois)
        {
            switch(mois)
            {
                case "Janvier":
               	case "January":
                     return 1;
                case "February":
                case "Février":
                    return 2;
                case "March":
                case "Mars":
                    return 3;
                case "April":
                case "Avril":
                    return 4;
                case "May":
                case "Mai":
                    return 5;
                case "June":
                case "Juin":
                    return 6;
                case "July":
                case "Juillet":
                    return 7;
                case "August":
                case "Aout":
                    return 8;
                case "September":
                case "Septembre":
                    return 9;
                case "October":
                case "Octobre":
                    return 10;
                case "November":
                case "Novembre":
                    return 11;
                case "December":
                case "Décembre":
                    return 12;
                default:
                    return 1;
            }
        }

        static private void CreateFrancePQs(MainWindow daddy)
        {
            try
            {
#region dates_for_pq
                String[] dates = new String[]
                {
                    "Avril/22/2001;Mars/06/2007",
"Mars/07/2007;Novembre/01/2007",
"Novembre/02/2007;Mai/10/2008",
"Mai/11/2008;Octobre/09/2008",
"Octobre/10/2008;Janvier/30/2009",
"Janvier/31/2009;Mai/05/2009",
"Mai/06/2009;Juillet/27/2009",
"Juillet/28/2009;Octobre/02/2009",
"Octobre/03/2009;Décembre/29/2009",
"Décembre/30/2009;Avril/02/2010",
"Avril/03/2010;Mai/25/2010",
"Mai/26/2010;Juillet/25/2010",
"Juillet/26/2010;Septembre/05/2010",
"Septembre/06/2010;Octobre/25/2010",
"Octobre/26/2010;Décembre/28/2010",
"Décembre/29/2010;Février/23/2011",
"Février/24/2011;Avril/07/2011",
"Avril/08/2011;Mai/08/2011",
"Mai/09/2011;Juin/17/2011",
"Juin/18/2011;Juillet/25/2011",
"Juillet/26/2011;Aout/26/2011",
"Aout/27/2011;Septembre/26/2011",
"Septembre/27/2011;Octobre/28/2011",
"Octobre/29/2011;Novembre/26/2011",
"Novembre/27/2011;Janvier/05/2012",
"Janvier/06/2012;Février/11/2012",
"Février/12/2012;Mars/09/2012",
"Mars/10/2012;Mars/30/2012",
"Mars/31/2012;Avril/13/2012",
"Avril/14/2012;Mai/03/2012",
"Mai/04/2012;Mai/22/2012",
"Mai/23/2012;Juin/14/2012",
"Juin/15/2012;Juillet/08/2012",
"Juillet/09/2012;Aout/04/2012",
"Aout/05/2012;Aout/24/2012",
"Aout/25/2012;Septembre/13/2012",
"Septembre/14/2012;Octobre/04/2012",
"Octobre/05/2012;Octobre/28/2012",
"Octobre/29/2012;Novembre/17/2012",
"Novembre/18/2012;Décembre/20/2012",
"Décembre/21/2012;Janvier/11/2013",
"Janvier/12/2013;Février/10/2013",
"Février/11/2013;Mars/02/2013",
"Mars/03/2013;Mars/17/2013",
"Mars/18/2013;Avril/05/2013",
"Avril/06/2013;Avril/18/2013",
"Avril/19/2013;Avril/25/2013",
"Avril/26/2013;Mai/07/2013",
"Mai/08/2013;Mai/16/2013",
"Mai/17/2013;Mai/25/2013",
"Mai/26/2013;Juin/07/2013",
"Juin/08/2013;Juin/24/2013",
"Juin/25/2013;Juillet/13/2013",
"Juillet/14/2013;Juillet/28/2013",
"Juillet/29/2013;Aout/12/2013",
"Aout/13/2013;Aout/24/2013",
"Aout/25/2013;Septembre/05/2013",
"Septembre/06/2013;Septembre/17/2013",
"Septembre/18/2013;Octobre/01/2013",
"Octobre/02/2013;Octobre/19/2013",
"Octobre/20/2013;Octobre/31/2013",
"Novembre/01/2013;Novembre/15/2013",
"Novembre/16/2013;Décembre/06/2013",
"Décembre/07/2013;Décembre/27/2013",
"Décembre/28/2013;Janvier/09/2014",
"Janvier/10/2014;Janvier/14/2014",
"Janvier/15/2014;Janvier/29/2014",
"Janvier/30/2014;Février/13/2014",
"Février/14/2014;Février/25/2014",
"Février/26/2014;Mars/06/2014",
"Mars/07/2014;Mars/15/2014",
"Mars/16/2014;Mars/22/2014",
"Mars/23/2014;Mars/29/2014",
"Mars/30/2014;Avril/06/2014",
"Avril/07/2014;Avril/14/2014",
"Avril/15/2014;Avril/21/2014",
"Avril/22/2014;Avril/29/2014",
"Avril/30/2014;Mai/03/2014",
"Mai/04/2014;Mai/09/2014",
"Mai/10/2014;Mai/16/2014",
"Mai/17/2014;Mai/24/2014",
"Mai/25/2014;Juin/03/2014",
"Juin/04/2014;Juin/09/2014",
"Juin/10/2014;Juin/19/2014",
"Juin/20/2014;Juin/28/2014",
"Juin/29/2014;Juillet/05/2014",
"Juillet/06/2014;Juillet/14/2014",
"Juillet/15/2014;Juillet/22/2014",
"Juillet/23/2014;Juillet/30/2014",
"Juillet/31/2014;Aout/08/2014",
"Aout/09/2014;Aout/15/2014",
"Aout/16/2014;Aout/22/2014",
"Aout/23/2014;Aout/29/2014",
"Aout/30/2014;Septembre/05/2014",
"Septembre/06/2014;Septembre/11/2014",
"Septembre/12/2014;Septembre/19/2014",
"Septembre/20/2014;Septembre/28/2014",
"Septembre/29/2014;Octobre/07/2014",
"Octobre/08/2014;Octobre/18/2014",
"Octobre/19/2014;Octobre/27/2014",
"Octobre/28/2014;Novembre/06/2014",
"Novembre/07/2014;Novembre/14/2014",
"Novembre/15/2014;Novembre/24/2014",
"Novembre/25/2014;Décembre/07/2014",
"Décembre/08/2014;Décembre/19/2014",
"Décembre/20/2014;Décembre/30/2014",
"Décembre/31/2014;Janvier/09/2015",
"Janvier/10/2015;Janvier/18/2015",
"Janvier/19/2015;Janvier/30/2015",
"Janvier/31/2015;Février/10/2015",
"Février/11/2015;Février/19/2015",
"Février/20/2015;Février/27/2015",
"Février/28/2015;Mars/04/2015",
"Mars/05/2015;Mars/09/2015",
"Mars/10/2015;Mars/17/2015",
"Mars/18/2015;Mars/22/2015",
"Mars/23/2015;Mars/29/2015",
"Mars/30/2015;Avril/05/2015",
"Avril/06/2015;Avril/11/2015",
"Avril/12/2015;Avril/17/2015",
"Avril/18/2015;Avril/22/2015",
"Avril/23/2015;Avril/27/2015",
"Avril/28/2015;Mai/01/2015",
"Mai/02/2015;Mai/05/2015",
"Mai/06/2015;Mai/12/2015",
"Mai/13/2015;Mai/18/2015",
"Mai/19/2015;Mai/23/2015",
"Mai/24/2015;Mai/29/2015",
"Mai/30/2015;Juin/04/2015",
"Juin/05/2015;Juin/09/2015",
"Juin/10/2015;Juin/17/2015",
"Juin/18/2015;Juin/23/2015",
"Juin/24/2015;Juillet/01/2015",
"Juillet/02/2015;Juillet/11/2015",
"Juillet/12/2015;Juillet/17/2015",
"Juillet/18/2015;Juillet/24/2015",
"Juillet/25/2015;Juillet/30/2015",
"Juillet/31/2015;Aout/05/2015",
"Aout/06/2015;Aout/11/2015",
"Aout/12/2015;Aout/15/2015",
"Aout/16/2015;Aout/19/2015",
"Aout/20/2015;Aout/23/2015",
"Aout/24/2015;Aout/27/2015",
"Aout/28/2015;Septembre/02/2015",
"Septembre/03/2015;Septembre/07/2015",
"Septembre/08/2015;Septembre/14/2015",
"Septembre/15/2015;Septembre/22/2015",
"Septembre/23/2015;Septembre/28/2015",
"Septembre/29/2015;Octobre/04/2015",
"Octobre/05/2015;Octobre/10/2015",
"Octobre/11/2015;Octobre/16/2015",
"Octobre/17/2015;Octobre/20/2015",
"Octobre/21/2015;Octobre/24/2015",
"Octobre/25/2015;Octobre/28/2015",
"Octobre/29/2015;Octobre/31/2015",
"Novembre/01/2015;Novembre/07/2015",
"Novembre/08/2015;Novembre/13/2015",
"Novembre/14/2015;Novembre/20/2015",
"Novembre/21/2015;Novembre/27/2015",
"Novembre/28/2015;Décembre/05/2015",
"Décembre/06/2015;Décembre/13/2015",
"Décembre/14/2015;Décembre/21/2015",
"Décembre/22/2015;Décembre/27/2015",
"Décembre/28/2015;Décembre/31/2015",
"Janvier/01/2016;Janvier/06/2016",
"Janvier/07/2016;Janvier/14/2016",
"Janvier/15/2016;Janvier/20/2016",
"Janvier/21/2016;Janvier/25/2016",
"Janvier/26/2016;Janvier/28/2016",
"Janvier/29/2016;Janvier/31/2016",
"Février/01/2016;Février/05/2016",
"Février/06/2016;Février/08/2016",
"Février/09/2016;Février/14/2016",
"Février/15/2016;Février/18/2016",
"Février/19/2016;Février/22/2016",
"Février/23/2016;Février/28/2016",
"Février/29/2016;Mars/02/2016",
"Mars/03/2016;Mars/05/2016",
"Mars/06/2016;Mars/11/2016",
"Mars/12/2016;Mars/16/2016",
"Mars/17/2016;Mars/20/2016",
"Mars/21/2016;Mars/25/2016",
"Mars/26/2016;Mars/29/2016",
"Mars/30/2016;Avril/02/2016",
"Avril/03/2016;Avril/06/2016",
"Avril/07/2016;Avril/10/2016",
"Avril/11/2016;Avril/15/2016",
"Avril/16/2016;Avril/18/2016",
"Avril/19/2016;Avril/23/2016",
"Avril/24/2016;Avril/28/2016",
"Avril/29/2016;Mai/02/2016",
"Mai/03/2016;Mai/06/2016",
"Mai/07/2016;Mai/10/2016",
"Mai/11/2016;Mai/15/2016",
"Mai/16/2016;Mai/19/2016",
"Mai/20/2016;Mai/24/2016",
"Mai/25/2016;Mai/29/2016",
"Mai/30/2016;Juin/04/2016",
"Juin/05/2016;Juin/10/2016",
"Juin/11/2016;Juin/17/2016",
"Juin/18/2016;Juin/23/2016",
"Juin/24/2016;Juin/30/2016",
"Juillet/01/2016;Juillet/06/2016",
"Juillet/07/2016;Juillet/13/2016",
"Juillet/14/2016;Juillet/20/2016",
"Juillet/21/2016;Juillet/26/2016",
"Juillet/27/2016;Juillet/31/2016",
"Aout/01/2016;Aout/05/2016",
"Aout/06/2016;Aout/10/2016",
"Aout/11/2016;Aout/15/2016",
"Aout/16/2016;Aout/21/2016",
"Aout/22/2016;Aout/26/2016",
"Aout/27/2016;Septembre/02/2016"
                };
#endregion
				// Et maintenant on lance les logs en boucle
	            daddy._ThreadProgressBarTitle = "Creation des PQs pour couvrir la France";
	            daddy.CreateThreadProgressBarEnh();
	            
	            // Wait for the creation of the bar
	            while (daddy._ThreadProgressBar == null)
	            {
	                Thread.Sleep(10);
	                Application.DoEvents();
	            }
	            daddy._ThreadProgressBar.progressBar1.Maximum = dates.Count();
	            
                int index = 1;
                foreach (String date in dates)
                {
                    String label = "_" + index.ToString("000");

                    if (daddy._ThreadProgressBar._bAbort)
                    {
                    	daddy.MsgActionError(daddy, "Création annulée, " + label + " non créée");
                    	break;
                    }
                    
                    daddy._ThreadProgressBar.lblWait.Text = "Creation de la PQ " + label;
                    
                    String[] vals = date.Split(';');
                    String[] valsd = vals[0].Split('/');
                    String[] valse = vals[1].Split('/');

                    String mdeb = GetMonth(valsd[0]).ToString();
                    String ddeb = valsd[1];
                    String ydeb = valsd[2];
                    String mend = GetMonth(valse[0]).ToString();
                    String dend = valse[1];
                    String yend = valse[2];
                    bool b = CreatePQDate(daddy, label, mdeb, ddeb, ydeb, mend, dend, yend);
                    index++;

                    if (!b)
                    {
                       	daddy.MsgActionError(daddy, "KO " + label + " " + mdeb + "/" + ddeb + "/" + ydeb + " " + mend + "/" + dend + "/" + yend);
                        break;
                    }
                    daddy._ThreadProgressBar.Step();
                    
                    
                }

                daddy.KillThreadProgressBarEnh();
                daddy.MsgActionDone(daddy);
                //daddy._cacheDetail.LoadPageText("dbg1", post_response, true);
                
            }
            catch (Exception ex)
            {
                daddy.KillThreadProgressBarEnh();
                daddy.ShowException("PQ error", "An error in PQ", ex);
            }
        }
        
		void PQDownloadHMILoad(object sender, EventArgs e)
		{
			if (_bAutoDwnloadUpdatedPQsOnStart)
			{
				 DownloadPQs(false);
			}
		}

    }
}
