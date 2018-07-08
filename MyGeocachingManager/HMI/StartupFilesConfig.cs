using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using System.Configuration;
using SpaceEyeTools;
using SpaceEyeTools.HMI;
using MyGeocachingManager.Geocaching;
using System.Diagnostics;

namespace MyGeocachingManager
{
    /// <summary>
    /// Form to configure list of files that will be loaded on MGM startup
    /// It can also be used to load a subpart of all available GPX files
    /// </summary>
    public partial class StartupFilesConfig : Form
    {
        MainWindow _daddy = null;
        /// <summary>
        /// List of files to load, separated by ;
        /// </summary>
        public List<String> _FilesToload = new List<string>();

        /// <summary>
        /// Return key associated to a file
        /// </summary>
        /// <param name="pathdb">Path where GPX are stored</param>
        /// <param name="filename">File</param>
        /// <returns>key associated to a file</returns>
        public static String GetKeyFromFilename(String pathdb, String filename)
        {
            return filename.Replace(pathdb, "");
        }

        private ListViewItem SetFileStats(List<String> ignoredf, String f, String pathdb)
        {
            FileInfo fi = new FileInfo(f);
            ListViewItem item = new ListViewItem(fi.Name);
            double l = fi.Length / (1024.0*1024.0);
            item.SubItems.Add(l.ToString("F1") + " " + _daddy.GetTranslator().GetString("LblMO"));
            item.SubItems.Add(fi.LastWriteTime.ToLongDateString());//String.Format("{0:d/M/yyyy HH:mm:ss}", fi.LastWriteTime));
            String key = StartupFilesConfig.GetKeyFromFilename(pathdb, f);
            item.SubItems.Add(key);
            item.Tag = key;
            if (ignoredf.Contains(key))
            {
                item.Checked = false; // ignored = unchecked
            }
            else
            {
                item.Checked = true; // keep it : check
            }

            return item;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        public StartupFilesConfig(MainWindow daddy)
        {
            _daddy = daddy;
            InitializeComponent();

            this.Text = _daddy.GetTranslator().GetString("DlgStartupFilecfg");
            label1.Text = _daddy.GetTranslator().GetString("LblCheckFileToLoad");
            button1startupconf.Text = _daddy.GetTranslator().GetString("BtnOk");
            button2startupconf.Text = _daddy.GetTranslator().GetString("BtnCancel");
            button3startupconf.Text = _daddy.GetTranslator().GetString("BtnCheckAll");
            button4startupconf.Text = _daddy.GetTranslator().GetString("BtnUncheckAll");
            button5startupconf.Text = _daddy.GetTranslator().GetString("BtnLoadSelectedFiles");
            label11.Text = _daddy.GetTranslator().GetString("LblDatabase");

            // Init listview
            listView1startupconf.Columns.Add(_daddy.GetTranslator().GetString("ColFileName"), 250);
            listView1startupconf.Columns.Add(_daddy.GetTranslator().GetString("ColSizeMo") + " (" + _daddy.GetTranslator().GetString("LblMO") + ")", 50);
            listView1startupconf.Columns.Add(_daddy.GetTranslator().GetString("ColCreationDate"), 150);
            listView1startupconf.Columns.Add(_daddy.GetTranslator().GetString("ColFilePath"), 150);

            // Build list from configuration
            List<String> ignoredf;
            ignoredf = StartupFilesConfig.GetListOfExcludedFiles();

            // And finally add all the files from the saved DB if they are missing
            // DB names
            ListTextCoord b = _daddy.GetBookmarks();
            cbDBListstartupconf.Items.Add(_daddy.GetTranslator().GetString("CBNewDB"));
            cbDBListstartupconf.SelectedIndex = 0;
            foreach (DatabaseOfFiles db in b._Databases)
            {
                cbDBListstartupconf.Items.Add(db);
            }

            // Load files
            String pathdb = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "GPX";
            string[] filePaths = Directory.GetFiles(pathdb, "*.gpx", SearchOption.AllDirectories);
            foreach (string f in filePaths)
            {
                ListViewItem item = SetFileStats(ignoredf, f, pathdb);
                listView1startupconf.Items.Add(item);
            }
            filePaths = Directory.GetFiles(pathdb, "*.zip", SearchOption.AllDirectories);
            foreach (string f in filePaths)
            {
                ListViewItem item = SetFileStats(ignoredf, f, pathdb); 
                listView1startupconf.Items.Add(item);
            }
            filePaths = Directory.GetFiles(pathdb, "*.ggz", SearchOption.AllDirectories);
            foreach (string f in filePaths)
            {
                ListViewItem item = SetFileStats(ignoredf, f, pathdb); 
                listView1startupconf.Items.Add(item);
            }

            // now complete with non existing files if any
            foreach (String s in ignoredf)
            {
                
                bool bfound = false;
                foreach (ListViewItem item in listView1startupconf.Items)
                {
                    if (item.Tag.ToString() == s)
                    {
                        bfound = true;
                        break;
                    }
                }
                if (!bfound)
                {
                    // On ne l'ajoute pas !!!!
                    /*
                    String cleaned = s;
                    int ipos = s.LastIndexOf('\\');
                    if (ipos != -1)
                        cleaned = s.Substring(ipos + 1);

                    ListViewItem item = new ListViewItem(cleaned);
                    item.SubItems.Add("?");
                    item.SubItems.Add("?");
                    item.SubItems.Add(s);
                    item.Tag = s;
                    item.Checked = false; // ignored = unchecked
                    listView1.Items.Add(item);
                    */
                }
            }
            
            // On resauve tout ça après le nettoyage, ça évite de se trainer les fichiers qui ont disparu
            SaveListOfExcludedFiles();
            _FilesToload.Clear();
            _daddy.TranslateTooltips(this, null);
        }

        private DatabaseOfFiles GetDBFromName(String name)
        {
            ListTextCoord b = _daddy.GetBookmarks();
            foreach (DatabaseOfFiles db in b._Databases)
            {
                if (db._Name == name)
                    return db;
            }
            return null;
        }


        /// <summary>
        /// Return a list of excluded files (i.e. that will not be loaded)
        /// </summary>
        /// <returns>List of excluded files (i.e. that will not be loaded)</returns>
        public static List<String> GetListOfExcludedFiles()
        {
            List<String> ignoredf;
            ignoredf = new List<string>();
            String excf = ConfigurationManager.AppSettings["excludedfiles"];
            ignoredf = excf.Split(':').ToList<string>();
            ignoredf.Remove("");
            return ignoredf;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveListOfExcludedFiles();
        }

        private void SaveListOfExcludedFiles()
        {
            // update configuration
            _FilesToload.Clear();
            String pathdb = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "GPX";
            String v = "";
            foreach (ListViewItem item in listView1startupconf.Items)
            {
                if (item.Checked == false) // to remove
                {
                    // These are the files to remove
                    v += item.Tag.ToString() + ":";
                }
                else
                {
                    // These are the files to load - IF THEY EXIST
                    String fload = pathdb + item.Tag.ToString();
                    if (File.Exists(fload))
                        _FilesToload.Add(fload);
                }
            }
            _daddy.UpdateConfFile("excludedfiles", v);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _FilesToload.Clear();
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.SubItems[1].Text == "?")
                e.Item.Font = new Font(listView1startupconf.Font, FontStyle.Bold);
            else
                e.Item.Font = listView1startupconf.Font;
            // e.Item.Font = new Font(listView1.Font, /*FontStyle.Strikeout |*/ FontStyle.Bold);

            if (e.Item.Checked == false) // unchecked = we ignore it
            {
                e.Item.ForeColor = listView1startupconf.ForeColor;
            }
            else
            {
                e.Item.ForeColor = Color.Blue;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1startupconf.Items)
            {
                item.Checked = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1startupconf.Items)
            {
                item.Checked = false;
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                Object sel = cbDBListstartupconf.SelectedItem;
                if (sel is DatabaseOfFiles) // That's the place holder
                {
                    DatabaseOfFiles db = (DatabaseOfFiles)sel;
                    // exclude everyone
                    foreach (ListViewItem item in listView1startupconf.Items)
                    {
                        item.Checked = false; // not loaded
                    }

                    // uncheck only files from the DB
                    foreach (string f in db._Files)
                    {
                        foreach (ListViewItem item in listView1startupconf.Items)
                        {
                            if (item.Tag.ToString() == f)
                            {
                                item.Checked = true; // to load
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
            	_daddy.ShowException("", "Loading files", exc);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                Object sel = cbDBListstartupconf.SelectedItem;
                if (sel is DatabaseOfFiles) // we overwrite existing DB
                {
                    DatabaseOfFiles db = (DatabaseOfFiles)sel;
                    foreach (ListViewItem item in listView1startupconf.Items)
                    {
                        if (item.Checked) // check = keep it
                        {
                            db._Files.Add(item.Tag.ToString());
                        }
                    }
                    // we look for existing DB
                    ListTextCoord b = _daddy.GetBookmarks();
                    foreach (DatabaseOfFiles edb in b._Databases)
                    {
                        if (edb._Id == db._Id)
                        {
                            edb._Files = db._Files;
                            break;
                        }
                    }
                    _daddy.SaveBookmarks(b);
                }
                else // we create a new db
                {
                    DatabaseOfFiles db = new DatabaseOfFiles();
                    foreach (ListViewItem item in listView1startupconf.Items)
                    {
                        if (item.Checked) // check = keep
                        {
                            db._Files.Add(item.Tag.ToString());
                        }
                    }

                    // and now the name
                    List<ParameterObject> lst = new List<ParameterObject>();
                    lst.Add(new ParameterObject(ParameterObject.ParameterType.String, "", "name", _daddy.GetTranslator().GetString("LblName")));

                    ParametersChanger changer = new ParametersChanger();
                    changer.Title = _daddy.GetTranslator().GetString("LblDatabase");
                    changer.BtnCancel = _daddy.GetTranslator().GetString("BtnCancel");
                    changer.BtnOK = _daddy.GetTranslator().GetString("BtnOk");
                    changer.ErrorFormater = _daddy.GetTranslator().GetString("ErrWrongParameter");
                    changer.ErrorTitle = _daddy.GetTranslator().GetString("Error");
                    changer.Parameters = lst;
                    changer.Font = this.Font;
                    changer.Icon = this.Icon;

                    if (changer.ShowDialog() == DialogResult.OK)
                    {
                        db._Name = lst[0].Value;
                        ListTextCoord b = _daddy.GetBookmarks();
                        b._Databases.Add(db);
                        _daddy.SaveBookmarks(b);

                        int i = cbDBListstartupconf.Items.Add(db);
                        cbDBListstartupconf.SelectedIndex = i;
                        _daddy.UpdateMenuWithDB();
                    }
                }
            }
            catch (Exception exc)
            {
            	_daddy.ShowException("", "Saving files", exc);
               
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                Object sel = cbDBListstartupconf.SelectedItem;
                if (sel is DatabaseOfFiles) // That's the place holder
                {
                    DatabaseOfFiles db = (DatabaseOfFiles)sel;
                    DialogResult dialogResult = MessageBox.Show(
                        String.Format(_daddy.GetTranslator().GetString("DlgDelDBTxt"), db._Name),
                        _daddy.GetTranslator().GetString("DlgDelDBTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dialogResult == DialogResult.Yes)
                    {
                        //do something
                        // we look for existing DB
                        ListTextCoord b = _daddy.GetBookmarks();
                        foreach (DatabaseOfFiles edb in b._Databases)
                        {
                            if (edb._Id == db._Id)
                            {
                                b._Databases.Remove(edb);
                                break;
                            }
                        }
                        _daddy.SaveBookmarks(b);

                        // update cbbox
                        int i = cbDBListstartupconf.SelectedIndex - 1;
                        if (i < 0)
                            i = 0;
                        cbDBListstartupconf.Items.Remove(sel);
                        cbDBListstartupconf.SelectedIndex = i;
                        _daddy.UpdateMenuWithDB();
                    }
                }
            }
            catch (Exception exc)
            {
                _daddy.ShowException("", "Deleting files", exc);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            _FilesToload.Clear();
            String pathdb = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "GPX";
            foreach (ListViewItem item in listView1startupconf.Items)
            {
                if (item.Checked)
                {
                    // These are the files to load - IF THEY EXIST
                    String fload = pathdb + item.Tag.ToString();
                    if (File.Exists(fload))
                        _FilesToload.Add(fload);
                }
            }
        }
    }
}
