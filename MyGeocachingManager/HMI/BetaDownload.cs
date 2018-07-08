using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MyGeocachingManager;
using System.Configuration;
using System.Net;
using System.Reflection;
using System.IO;
using SpaceEyeTools;
using SpaceEyeTools.HMI;
using System.Diagnostics;

namespace MyGeocachingManager.HMI
{
    /// <summary>
    /// Form to download beta version of MGM from any official MGM website
    /// - Official website
    /// - Failover website
    /// - Any website manually entered in configuration ;-)
    /// </summary>
    public partial class BetaDownload : Form
    {
        MainWindow _daddy = null;
        ImageList _imageList = new ImageList();

        /// <summary>
        /// Construction
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        public BetaDownload(MainWindow daddy)
        {
            _daddy = daddy;
            InitializeComponent();

            this.Text = _daddy.GetTranslator().GetString("FMenuDownloadBeta");
            listViewBeta.Columns.Add(_daddy.GetTranslator().GetString("Version"),100);
            listViewBeta.Columns.Add(_daddy.GetTranslator().GetString("LblWebsite"),200);

            _daddy.TranslateTooltips(this, null);

            // create image list and fill it 
            int index = _daddy.getIndexImages("New");
            Image image = _daddy.getImageFromIndex(index);
            _imageList.Images.Add("new", image);
            index = _daddy.getIndexImages("Owned");
            image = _daddy.getImageFromIndex(index);
            _imageList.Images.Add("owned", image);
            listViewBeta.SmallImageList = _imageList;
        }

        /// <summary>
        /// Return list of all beta version more recent than the current one
        /// </summary>
        /// <param name="url">Url of serveur to check</param>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        /// <returns>the highest available beta (key = version, value = full url)</returns>
        public static KeyValuePair<String, String> GetMoreRecentBetaAvailable(String url, MainWindow daddy)
        {
            Dictionary<String, String> betas = new Dictionary<String, String>();
            KeyValuePair<String, String> kv = new KeyValuePair<string, string>("","");

            if (MainWindow.AssemblySubVersion == "")
            {
                // La version courante n'est pas une beta, donc pas de raison de retourner des betas à mettre à jour
                return kv;
            }

            // On stocke uniquement la version
            String currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (MainWindow.AssemblySubVersion != "")
                currentVersion += "." + MainWindow.AssemblySubVersion;

            // On peuple a liste
            String urlupdate = ConfigurationManager.AppSettings["urlupdate"];
            // Il se peut qu'on ait 2 URL (le site principal et le site de backup)
            
            try
            {
                String u = url + "/alpha/view.php";
                // On récupère les versions disponibles
                String response = MyTools.GetRequest(new Uri(u), daddy.GetProxy(), 2000);

                // On parse la réponse
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(response);
                var linkNodes = doc.DocumentNode.SelectNodes("//a[@href]");
                if (linkNodes != null)
                {
                    foreach (HtmlAgilityPack.HtmlNode node in linkNodes)
                    {
                        if (node.Attributes.Contains("href"))
                        {
                            // Est-ce une version ?
                            String name = HtmlAgilityPack.HtmlEntity.DeEntitize(node.InnerText);
                            if (name.EndsWith(".MyGeocachingManager.zip"))
                            {
                              
                                String n = name.Replace(".MyGeocachingManager.zip", "");
                                List<string> vers = n.Split('.').ToList<string>();
                                String betaver = vers[0] + "." + vers[1] + "." + vers[2] + "." + vers[3];

                                // Est-ce plus récent ?
                                // Si la version courante a la même version majeure qu'une beta (sans le AssemblySubVersion)
                                // elle est forcément plus récente car les béta ne sont crées qu'avec un numéro de version futur
                                //  C'est toujours x.y.z.a ou x.y.z.a.b
                                int comp = currentVersion.CompareTo(n);
                                
                                if (comp < 0)
                                {
                                    // C'est une beta plus récente
                                    betas.Add(n, url + "/alpha/" + n + ".MyGeocachingManager.zip");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                daddy.Log(MainWindow.GetException("Error with URL " + url, ex));
            }

            // On regarde maintenant quelle est la version la plus à jour
            foreach (KeyValuePair<String, String> pair in betas)
            {

                int i = kv.Key.CompareTo(pair.Key);
                if (i < 0) // cool on a une version supérieure
                {
                    kv = new KeyValuePair<string, string>(pair.Key, pair.Value);
                }
            }

            return kv;
        }

        private void BetaDownload_Load(object sender, EventArgs e)
        {
            // On stocke uniquement la version
            String currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (MainWindow.AssemblySubVersion != "")
                currentVersion += "." + MainWindow.AssemblySubVersion;

            // On peuple a liste
            String urlupdate = ConfigurationManager.AppSettings["urlupdate"];
            // Il se peut qu'on ait 2 URL (le site principal et le site de backup)
            List<string> urls = urlupdate.Split(';').ToList<string>();
            foreach (String url in urls)
            {
                try
                {
                    String u = url + "/alpha/view.php";
                    // On récupère les versions disponibles
                    String response = MyTools.GetRequest(new Uri(u), _daddy.GetProxy(), 2000);

                    // On parse la réponse
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(response);
                    var linkNodes = doc.DocumentNode.SelectNodes("//a[@href]");
                    if (linkNodes != null)
                    {
                        foreach (HtmlAgilityPack.HtmlNode node in linkNodes)
                        {
                            if (node.Attributes.Contains("href"))
                            {
                                // Est-ce une version ?
                                String name = HtmlAgilityPack.HtmlEntity.DeEntitize(node.InnerText);
                                if (name.EndsWith(".MyGeocachingManager.zip"))
                                {
                                    
                                    String n = name.Replace(".MyGeocachingManager.zip","");
                                    List<string> vers = n.Split('.').ToList<string>();
                                    String betaver = vers[0] + "." + vers[1] + "." + vers[2] + "." + vers[3];
                                    
                                    ListViewItem item = listViewBeta.Items.Add(n);

                                    // Est-ce plus récent ?
                                    // Si la version courante a la même version majeure qu'une beta (sans le AssemblySubVersion)
                                    // elle est forcément plus récente car les béta ne sont crées qu'avec un numéro de version futur
                                    //  C'est toujours x.y.z.a ou x.y.z.a.b
                                    int comp = currentVersion.CompareTo(n);
                                    if (MainWindow.AssemblySubVersion == "")
                                    {
                                        // La version courante n'est pas beta
                                        // si elle a la meme version majeure que la beta, on ne fait rien
                                        if (currentVersion == betaver)
                                            comp = 0;
                                    }

                                    if (comp < 0)
                                        item.ImageIndex = 0;
                                    else if (currentVersion == n)
                                        item.ImageIndex = 1;

                                    item.SubItems.Add(url);
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                	_daddy.Log(MainWindow.GetException("Error with URL " + url, ex));
                }
            }

            if (listViewBeta.Items.Count == 0)
            {
                _daddy.MsgActionWarning(this, _daddy.GetTranslator().GetString("LblNoBetaVersion"));
                this.Close();
            }
        }

        private void listViewBeta_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listViewBeta.SelectedItems.Count != 0)
            {
                ListViewItem item = listViewBeta.SelectedItems[0];

                DialogResult dialogResult = MyMessageBox.Show(
                        String.Format(_daddy.GetTranslator().GetString("LblAreYouSureBeta"), item.Text),
                        _daddy.GetTranslator().GetString("WarTitle"),
                        MessageBoxIcon.Question, _daddy.GetTranslator());

                if (dialogResult == DialogResult.Yes)
                {
                    WebClient client = new WebClient();
                    WebProxy proxy = _daddy.GetProxy();
                    if (proxy != null)
                        client.Proxy = proxy;

                    // Le chemin local
                    // Attention, le zip contient la version majeure, pas la référence à la beta
                    List<string> vers = item.Text.Split('.').ToList<string>();
                    String betaver = vers[0] + "." + vers[1] + "." + vers[2] + "." + vers[3];
                    String localfile = _daddy.GetInternalDataPath() + Path.DirectorySeparatorChar + @"\" + betaver + ".MyGeocachingManager.zip";

                    // L'url
                    String fupdate = item.SubItems[1].Text + "/alpha/" + item.Text + ".MyGeocachingManager.zip";

                    _daddy.DownloadAndInstallMGMVersion(client, fupdate, localfile);
                }
            }
        }
    }
}
