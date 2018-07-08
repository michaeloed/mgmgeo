using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Net;
using MyGeocachingManager.Geocaching;
using SpaceEyeTools.HMI;
using SpaceEyeTools;
using System.Threading;
using System.Diagnostics;

namespace MyGeocachingManager.HMI
{
    /// <summary>
    /// Form to perform and display MGM diagnostics
    /// </summary>
    public partial class Diagnostics : Form
    {
        private MainWindow _daddy = null;
        private ImageList _imageList = new ImageList();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        public Diagnostics(MainWindow daddy)
        {
            _daddy = daddy;
            InitializeComponent();

            this.Text = _daddy.GetTranslator().GetString("FMenuDiagnostics");
            lvDiagnostics.Columns.Add(_daddy.GetTranslator().GetString("FGBStatus"), 50);
            lvDiagnostics.Columns.Add(_daddy.GetTranslator().GetString("LblName"), 200);
            lvDiagnostics.Columns.Add(_daddy.GetTranslator().GetString("LblDescription"), 400);

            // create image list and fill it 
            int index = _daddy.getIndexImages("True");
            Image image = _daddy.getImageFromIndex(index);
            _imageList.Images.Add("OK", image);
            index = _daddy.getIndexImages("Close");
            image = _daddy.getImageFromIndex(index);
            _imageList.Images.Add("KO", image);
            lvDiagnostics.SmallImageList = _imageList;
        }

        private ListViewItem CreateEntry(String status, String label, String detail)
        {
            ListViewItem item = null;
            if ((status == "Yes") || (status == "OK"))
            {
                item = lvDiagnostics.Items.Add(status, 0);
                item.BackColor = Color.LightGreen;
            }
            else
            {
                item = lvDiagnostics.Items.Add(status, 1);
                item.BackColor = Color.Red;
                item.ForeColor = Color.White;
            }
            item.SubItems.Add(label);
            if (detail != "")
                item.SubItems.Add(detail);
            _daddy._ThreadProgressBar.Step();
            _daddy._ThreadProgressBar.lblWait.Text = status + ": " + label + " " + detail;
            return item;
        }

        private void Diagnostics_Load(object sender, EventArgs e)
        {
            try
            {
                _daddy._ThreadProgressBarTitle = _daddy.GetTranslator().GetString("LblDownloadInProgress");
                _daddy.CreateThreadProgressBarEnh();
                // Wait for the creation of the bar
                while (_daddy._ThreadProgressBar == null)
                {
                    Thread.Sleep(10);
                    Application.DoEvents();
                }
                _daddy._ThreadProgressBar.progressBar1.Maximum = 31;
                _daddy._ThreadProgressBar.lblWait.Text = "Performing diagnostics...";

                ListViewItem item = null;
                int nb = 0;

                // Proxy ?
                if (_daddy.GetProxy() != null)
                    CreateEntry("OK", "Proxy status", "Defined");
                else
                    CreateEntry("OK", "Proxy status", "Undefined");

                // Internet access ?
                if (_daddy.CheckInternetAccess())
                    CreateEntry("OK", "Internet access", "Available");
                else
                    CreateEntry("KO", "Internet access", "Not available");

                // GC account ?
                String cryptedpass = ConfigurationManager.AppSettings["ownerpassword"];
                String username = ConfigurationManager.AppSettings["owner"];
                if (username != "")
                    CreateEntry("OK", "Geocaching login", "Filled");
                else
                    CreateEntry("KO", "Geocaching login", "Empty");

                if (cryptedpass != "")
                    CreateEntry("OK", "Geocaching password", "Filled");
                else
                    CreateEntry("KO", "Geocaching password", "Empty");

                CookieContainer cookie = null;
                if ((username != "") && (cryptedpass != ""))
                {
                    cookie = _daddy.CheckGCAccount(true, true);
                    if (cookie != null)
                        CreateEntry("OK", "Geocaching authentication", "Success");
                    else
                        CreateEntry("KO", "Geocaching authentication", "Error during authentication");
                }
                else
                {
                    CreateEntry("KO", "Geocaching authentication", "Login and/or password empty");
                }

                // MGM websites
                String urlupdate = ConfigurationManager.AppSettings["urlupdate"];
                List<string> urls = urlupdate.Split(';').ToList<string>();
                foreach (String url in urls)
                {
                    try
                    {
                        String rep = MyTools.GetRequest(new Uri(url), _daddy.GetProxy(), 2000);
                        if (rep != "")
                            CreateEntry("OK", "MGM website", "Available: " + url);
                        else
                            CreateEntry("KO", "MGM website", "Not available: " + url);
                    }
                    catch
                    {
                        item = CreateEntry("KO", "MGM website availability", "Not available: " + url);
                    }
                }

                // PQ download ?
                if (cookie == null)
                {
                    CreateEntry("KO", "Pocket Queries download", "User not authenticated");
                }
                else
                {
                    String html = _daddy.GetPQDownloadHTML(cookie);
                    PQDownloadHMI pq = new PQDownloadHMI(_daddy);
                    nb = pq.Populate(html, true);
                    if (nb != 0)
                        CreateEntry("OK", "Pocket Queries download", "Available PQs: " + nb.ToString());
                    else
                        CreateEntry("KO", "Pocket Queries download", "Available PQs: " + nb.ToString());
                }
                

                // Images
                OfflineCacheData ocd = new OfflineCacheData();
                WebClient client = new WebClient();
                client.Proxy = _daddy.GetProxy();
                string htmlCode = client.DownloadString("http://coord.info/GC2MQGF");
                DownloadWorker dwnd = new DownloadWorker(_daddy);
                dwnd.GetImageFromParsingImpl(ocd, false, htmlCode);
                String s = "";
                nb = 0;
                List<string> images = new List<string>();
                foreach (KeyValuePair<String, OfflineImageWeb> paire in ocd._ImageFilesSpoilers)
                {
                    OfflineImageWeb oid = paire.Value;
                    s += oid._name + ";";
                    images.Add(oid._name);
                    nb++;
                }
                // ;;;
                if (nb == 4)
                {
                    if (
                        (images[0] == "[SPOILER] Cache") &&
                        //(images[1] == "Départ du sentier / Start of the trail") && // les accents peuvent faire chier
                        (images[2] == "Le Saule Pleureur / The Weeping Willow") &&
                        (images[3] == "Le Saule Pleureur / The Weeping Willow")
                        )
                        CreateEntry("OK", "Image/Spoilers download", "Good count and names");
                    else
                        CreateEntry("KO", "Image/Spoilers download", "Bad names");
                }
                else
                    CreateEntry("KO", "Image/Spoilers download", "Bad count");

                // Completion d'une cache
                if (cookie == null)
                {
                    CreateEntry("KO", "Cache update", "User not authenticated");
                }
                else
                {
                    htmlCode = _daddy.GetCacheHTMLFromClientImpl("http://coord.info/GCX076", cookie);
                    Geocache geo = new Geocache(_daddy);
                    DataForStatsRetrieval dataforstats = new DataForStatsRetrieval();
                    dataforstats.cookieJar = cookie;
                    dataforstats.firstQuestion = false;
                    dataforstats.inbmissed = 0;
                    dataforstats.stopScoreRetrieval = false;
                    nb = _daddy._iNbModifiedCaches;
                    _daddy.CompleteCacheFromHTML(geo, htmlCode, null, dataforstats, true);
                    _daddy._iNbModifiedCaches = nb;
                    if (CheckCacheContent(geo))
                        CreateEntry("OK", "Cache update", "General success");
                    else
                        CreateEntry("KO", "Cache update", "General failure");
                }

                _daddy.KillThreadProgressBarEnh();
            }
            catch (Exception exc)
            {
                _daddy.KillThreadProgressBarEnh();
                _daddy.ShowException("", _daddy.GetTranslator().GetString("FMenuDiagnostics"), exc);
            }
        }

        private bool CheckOneEntry(string label, string detail, bool ok)
        {
            if (ok)
                CreateEntry("OK", label, detail);
            else
                CreateEntry("KO", label, detail);
            return ok;
        }

        private bool CheckCacheContent(Geocache geo)
        {
            bool ok = true;

            ok = ok && CheckOneEntry("Cache update", "Owner", (geo._Owner == "T Diddy and Sea Mama"));
            ok = ok && CheckOneEntry("Cache update", "Type", (geo._Type == "Traditional Cache"));
            ok = ok && CheckOneEntry("Cache update", "Date of creation", (geo._DateCreation == "2006-07-05T00:00:1Z"));
            ok = ok && CheckOneEntry("Cache update", "Description (short)", (geo._ShortDescription != ""));
            ok = ok && CheckOneEntry("Cache update", "Description (long)", (geo._LongDescription != ""));
            ok = ok && CheckOneEntry("Cache update", "Difficulty", (geo._D == "3"));
            ok = ok && CheckOneEntry("Cache update", "Terrain", (geo._T == "1"));
            ok = ok && CheckOneEntry("Cache update", "Latitude", (geo._Latitude == "37.787767"));
            ok = ok && CheckOneEntry("Cache update", "Longitude", (geo._Longitude == "-122.489883"));
            ok = ok && CheckOneEntry("Cache update", "Container", (geo._Container == "Micro"));
            ok = ok && CheckOneEntry("Cache update", "Hint", (geo._Hint != ""));
            ok = ok && CheckOneEntry("Cache update", "Logs", (geo._Logs.Count != 0));
            String username = ConfigurationManager.AppSettings["owner"];
            ok = ok && CheckOneEntry("Cache update", "Found: " + geo.IsFound().ToString(), true);
            ok = ok && CheckOneEntry("Cache update", "Owned", (geo._bOwned == false));
            ok = ok && CheckOneEntry("Cache update", "Archived", (geo._Archived == "False"));
            ok = ok && CheckOneEntry("Cache update", "Available", (geo._Available == "True"));
            ok = ok && CheckOneEntry("Cache update", "Owner", (geo._PlacedBy == "T Diddy and Sea Mama"));
            ok = ok && CheckOneEntry("Cache update", "Country", (geo._Country == "United States"));
            ok = ok && CheckOneEntry("Cache update", "State", (geo._State == "California"));
            if (geo._Ocd != null)
            {
                ok = ok && CheckOneEntry("Cache update", "Statistics", (geo._Ocd._iNbFavs != 0));
                ok = ok && CheckOneEntry("Cache update", "Popularity", (geo._Ocd._dRatingSimple != -1.0));
            }
            else
            {
                CreateEntry("KO", "Cache update", "Statistics");
                ok = false;
            }
            return ok;
        }

        private void lvDiagnostics_DoubleClick(object sender, EventArgs e)
        {
            if (lvDiagnostics.SelectedItems.Count != 0)
            {
                ListViewItem item = lvDiagnostics.SelectedItems[0];
                if (item.SubItems.Count == 3)
                {
                    _daddy.MSG(item.SubItems[2].Text);
                }
            }
        }
    }
}

