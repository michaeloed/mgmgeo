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
    /// Form to download a plugin from any official MGM website
    /// - Official website
    /// - Failover website
    /// - Any website manually entered in configuration ;-)
    /// This forms displays also local plugins not present on website
    /// and allows deletion of local / downloaded plugin
    /// </summary>
    public partial class PluginsDownload : Form
    {
        MainWindow _daddy = null;
        ImageList _imageList = new ImageList();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        public PluginsDownload(MainWindow daddy)
        {
            _daddy = daddy;
            InitializeComponent();

            this.Text = _daddy.GetTranslator().GetString("FMenuDownloadPlugins");
            listViewPluginsDownload.Columns.Add(_daddy.GetTranslator().GetString("LblFile"),160);
            listViewPluginsDownload.Columns.Add(_daddy.GetTranslator().GetString("LblWebsite"),120);
            listViewPluginsDownload.Columns.Add(_daddy.GetTranslator().GetString("LblName"), 100);
            listViewPluginsDownload.Columns.Add(_daddy.GetTranslator().GetString("LblDescription"), 250);
            listViewPluginsDownload.Columns.Add(_daddy.GetTranslator().GetString("Version"), 100);
            listViewPluginsDownload.Columns.Add(_daddy.GetTranslator().GetString("LblMinVerMGM"), 100);
            
            // create image list and fill it 
            int index = _daddy.getIndexImages("Owned");
            Image image = _daddy.getImageFromIndex(index);
            _imageList.Images.Add("owned", image);
            index = _daddy.getIndexImages("New");
            image = _daddy.getImageFromIndex(index);
            _imageList.Images.Add("new", image);
            listViewPluginsDownload.SmallImageList = _imageList;

            fMenuPluginsIHMToolStripMenuItem.Text = _daddy.GetTranslator().GetString("FMenuPluginsDownloadMenu");
            openLocalPluginToolStripMenuItem.Text = _daddy.GetTranslator().GetString("FMenuPlugOpenDir");
            deleteLocalPluginsToolStripMenuItem.Text = _daddy.GetTranslator().GetString("FMenuPlugDelete");

            _daddy.TranslateTooltips(this, null);
        }

        private void PluginsDownload_Load(object sender, EventArgs e)
        {
            // On peuple a liste
            String urlupdate = ConfigurationManager.AppSettings["urlupdate"];
            // Il se peut qu'on ait 2 URL (le site principal et le site de backup)
            List<string> urls = urlupdate.Split(';').ToList<string>();
            String plugpath = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + @"\Plugins\";
            List<string> pluginstrouves = new List<string>();
            foreach (String url in urls)
            {
                try
                {
                    String u = url + "/plugins/view.php";
                    // On récupère les plugins disponibles
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
                                // Est-ce un plugin ?
                                String name = HtmlAgilityPack.HtmlEntity.DeEntitize(node.InnerText);
                                if (name.EndsWith(".cs") || name.EndsWith(".vb"))
                                {           
                                    // C'est un plugin
                                    // On l'ajoute
                                    ListViewItem item = listViewPluginsDownload.Items.Add(name);
                                    pluginstrouves.Add(name);

                                    String localfile = plugpath + name;
                                    if (File.Exists(localfile))
                                        item.ImageIndex = 0;

                                    // son url source
                                    item.SubItems.Add(url);

                                    // Pour la suite on a besoin de télécharger le source et de vérifier s'il contient les tags d'identification
                                    bool downloadok = false;
                                    string plugcontent = "";
                                    try
                                    {
                                        plugcontent = MyTools.GetRequestWithEncoding(new Uri(url + "/plugins/" + name), _daddy.GetProxy(), 500);
                                        if (plugcontent != "")
                                            downloadok = true;
                                    }
                                    catch (Exception)
                                    {
                                        downloadok = false;
                                    }

                                    if (downloadok)
                                    {
                                        CompleteListViewInfo(item, plugcontent);
                                    }
                                    else
                                    {
                                        // son nom (optionnel)
                                        item.SubItems.Add("?");

                                        // sa description (optionnel)
                                        item.SubItems.Add("?");

                                        // sa version (optionnel)                                  
                                        item.SubItems.Add("?");

                                        // la version minimum MGM (optionnel)
                                        item.SubItems.Add("?");
                                    }
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                	_daddy.Log(MainWindow.GetException("Error with URL " + url,ex));

                }

                

            }

            // On complete la liste avec les plugins locaux SANS inclure ceux qu'on a déjà trouvé sur le web
            string[] filePaths = Directory.GetFiles(plugpath, "*.cs", SearchOption.AllDirectories);
            foreach (string p in filePaths)
            {
                String f = Path.GetFileName(p);
                // Est-il déjà contenu dans la liste ?
                if (pluginstrouves.Contains(f) == false)
                {
                    ListViewItem item = listViewPluginsDownload.Items.Add(f);
                    item.ImageIndex = 0;

                    // son url source
                    item.SubItems.Add("-");

                    // Le contenu
                    System.IO.StreamReader myFile = new System.IO.StreamReader(p, Encoding.Default, true);
                    string plugcontent = myFile.ReadToEnd();
                    myFile.Close();

                    // Le reste
                    CompleteListViewInfo(item, plugcontent);

                    // On le force ) ne pas être téléchargeable
                    item.Tag = false;
                }
            }

            if (listViewPluginsDownload.Items.Count == 0)
            {
                _daddy.MsgActionWarning(this, _daddy.GetTranslator().GetString("LblNoPlugins"));
                this.Close();
            }
        }

        private void CompleteListViewInfo(ListViewItem item, string plugcontent)
        {
            // son nom (optionnel)
            String nom = "";
            string value = MyTools.GetSnippetFromText("string Name", ";", plugcontent);
            value = MyTools.GetSnippetFromText("\"", "\"", value);
            if (value != "")
            {
                item.SubItems.Add(value);
                nom = value;
            }
            else
                item.SubItems.Add("-");

            // sa description (optionnel)
            value = MyTools.GetSnippetFromText("string Description", ";", plugcontent);
            value = MyTools.GetSnippetFromText("\"", "\"", value);
            if (value != "")
                item.SubItems.Add(value);
            else
                item.SubItems.Add("-");

            // sa version (optionnel)                                  
            value = MyTools.GetSnippetFromText("string Version", ";", plugcontent);
            value = MyTools.GetSnippetFromText("\"", "\"", value);
            if (value != "")
            {
                item.SubItems.Add(value);
                // On regarde si on trouve ce script, uniquement dans les IScriptV2 qui implémentent cette version
                // et si la version actuellement chargée est plus vieille
                // pffuuu
                if (nom != "") // pas de meilleure clé pour l'instant
                {
                    foreach (IScriptV2 script in _daddy._iListScriptV2)
                    {
                        if (script.Name == nom)
                        {
                            int comp = value.CompareTo(script.Version);
                            if (comp > 0)
                            {
                                item.ImageIndex = 1;
                            }
                        }
                    }
                }
            }
            else
                item.SubItems.Add("-");

            // la version minimum MGM (optionnel)
            value = MyTools.GetSnippetFromText("string MinVersionMGM", ";", plugcontent);
            value = MyTools.GetSnippetFromText("\"", "\"", value);
            if (value != "")
            {
                item.SubItems.Add(value);

                // a-t'on une version supérieure à la notre ?
                List<string> vers = value.Split('.').ToList<string>();
                String betaver = vers[0] + "." + vers[1] + "." + vers[2] + "." + vers[3];
                String currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                if (MainWindow.AssemblySubVersion != "")
                    currentVersion += "." + MainWindow.AssemblySubVersion;

                // Est-ce plus récent ?
                // Si la version courante a la même version majeure qu'une beta (sans le AssemblySubVersion)
                // elle est forcément plus récente car les béta ne sont crées qu'avec un numéro de version futur
                //  C'est toujours x.y.z.a ou x.y.z.a.b
                int comp = currentVersion.CompareTo(value);
                if (MainWindow.AssemblySubVersion == "")
                {
                    // La version courante n'est pas beta
                    // si elle a la meme version majeure que la beta, on ne fait rien
                    if (currentVersion == betaver)
                        comp = 0;
                }

                if (comp < 0)
                {
                    item.Font = new Font(item.Font, item.Font.Style | FontStyle.Strikeout);
                    item.Tag = false;
                }
            }
            else
                item.SubItems.Add("-");
        }

        private void listViewBeta_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listViewPluginsDownload.SelectedItems.Count != 0)
            {
                ListViewItem item = listViewPluginsDownload.SelectedItems[0];

                // Autorisé au téléchargement ?
                if (item.Tag != null)
                {
                    bool allow = (bool)(item.Tag);
                    if (allow == false)
                        return;
                }

                DialogResult dialogResult = MyMessageBox.Show(
                        String.Format(_daddy.GetTranslator().GetString("LblAreYouSurePlugins"), item.Text),
                        _daddy.GetTranslator().GetString("WarTitle"),
                        MessageBoxIcon.Question, _daddy.GetTranslator());

                if (dialogResult == DialogResult.Yes)
                {
                    // L'url
                    String url = item.SubItems[1].Text + "/plugins/" + item.Text;

                    // Le chemin local
                    String plugPath = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar;
                    if (Directory.Exists(plugPath) == false)
                        Directory.CreateDirectory(plugPath);
                    String localfile = plugPath + item.Text;

                    HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
                    objRequest.Proxy = _daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
                    HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
                    using (Stream output = File.OpenWrite(localfile))
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

                    item.ImageIndex = 0;

                    _daddy.MsgActionWarning(this, _daddy.GetTranslator().GetString("MsgRestartApplcation"));
                }
            }
        }

        private void deleteLocalPluginsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewPluginsDownload.SelectedItems.Count != 0)
            {
                String plugPath = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar;
                foreach (ListViewItem item in listViewPluginsDownload.SelectedItems)
                {
                    String localfile = plugPath + item.Text;
                    if (File.Exists(localfile))
                    {
                        File.Delete(localfile);
                        item.ImageIndex = -1;
                    }
                }
                _daddy.MsgActionWarning(this, _daddy.GetTranslator().GetString("MsgRestartApplcation"));
            }
        }

        private void openLocalPluginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String pqdatapath = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "Plugins" + Path.DirectorySeparatorChar;
            MyTools.StartInNewThread(pqdatapath);
        }
    }
}
