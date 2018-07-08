using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MyGeocachingManager.Geocaching;
using SpaceEyeTools.HMI;

namespace MyGeocachingManager
{
    /// <summary>
    /// Form to manage ignored caches
    /// </summary>
    public partial class IgnoreCacheManager : Form
    {
        MainWindow _daddy = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        public IgnoreCacheManager(MainWindow daddy)
        {
            _daddy = daddy;
            InitializeComponent();
            TranslateForm();

            lvIgnoredCaches.Columns.Add(_daddy.GetTranslator().GetString("LVCode"), 80);
            lvIgnoredCaches.Columns.Add(_daddy.GetTranslator().GetString("LVName"), 250);
            lvIgnoredCaches.Columns.Add(_daddy.GetTranslator().GetString("LVType"), 110);
            lvIgnoredCaches.Columns.Add(_daddy.GetTranslator().GetString("LVContainer"), 65);
            lvIgnoredCaches.Columns.Add(_daddy.GetTranslator().GetString("LVDifficulty"), 65);
            lvIgnoredCaches.Columns.Add(_daddy.GetTranslator().GetString("LVTerrain"), 65);

            foreach (KeyValuePair<String, MiniGeocache> pair in _daddy._ignoreList)
            {
                MiniGeocache geo = pair.Value;
                ListViewItem item = lvIgnoredCaches.Items.Add(geo._Code);
                item.SubItems.Add(geo._Name);
                item.SubItems.Add(geo._Type);
                item.SubItems.Add(geo._Container);
                item.SubItems.Add(geo._D);
                item.SubItems.Add(geo._T);
            }
        }

        /// <summary>
        /// Automatically translate form labels
        /// </summary>
        public void TranslateForm()
        {
            this.Text = _daddy.GetTranslator().GetString("IgnoreCacheManager");
            this.btnRemoveFromIgnoreList.Text = _daddy.GetTranslator().GetString("Lblremovefromignorelist");
            _daddy.TranslateTooltips(this, null);
            
        }


        private void IgnoreCacheManager_Load(object sender, EventArgs e)
        {

        }

        private void btnRemoveFromIgnoreList_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection selitems = lvIgnoredCaches.SelectedItems;
            foreach (ListViewItem item in selitems)
            {
                _daddy._ignoreList.Remove(item.Text);
                lvIgnoredCaches.Items.Remove(item);
            }

            // On sauve la liste
            _daddy.ReplaceIgnoreList();

            // Restart needed
            _daddy.MsgActionWarning(this, _daddy.GetTranslator().GetString("MsgRestartApplcation"));
        }
    }
}
