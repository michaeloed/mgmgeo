using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;
using SpaceEyeTools.EXControls;
using System.Collections;
using SpaceEyeTools;
using SpaceEyeTools.HMI;
using MyGeocachingManager.Geocaching;

namespace MyGeocachingManager
{
    /// <summary>
    /// Form to manage tags (labels) on caches
    /// </summary>
    public partial class TagManager : Form
    {
        MainWindow _daddy = null;
        List<String> _TagsInit = new List<string>();
        List<String> _TagsToAdd = new List<string>();
        List<String> _TagsToDel = new List<string>();
        List<Geocache> _selCaches = null;
        ListView.SelectedListViewItemCollection _selLVItems = null;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        public TagManager(MainWindow daddy)
        {
            _daddy = daddy;
            InitializeComponent();

            this.Text = _daddy.GetTranslator().GetString("MNUAddDelTag");
            label1.Text = _daddy.GetTranslator().GetString("LblTag");
            button1tagmgr.Text = _daddy.GetTranslator().GetString("BtnOk");
            button2tagmgr.Text = _daddy.GetTranslator().GetString("BtnCancel");
            _daddy.TranslateTooltips(this, null);

            // Populate combo list with existing tags
            foreach (String t in _daddy._listExistingTags)
            {
                if (comboBox1tagmgr.Items.Contains(t) == false)
                    comboBox1tagmgr.Items.Add(t);
            }

            // Load the set of tags common to the selection
            _selCaches = _daddy.GetSelectedCaches();
            _selLVItems = _daddy.GetSelectedListViewItems();

            // Create the common list of tags for this selection. Tricky...
            if (_selCaches.Count != 0)
            {
                // Init list with list from first cache
                Geocache geo = _selCaches[0];
                if (geo._Ocd == null)
                {
                    // This one has no tag, no need to look for an intersection
                    // there will be nothing !
                    return;
                }
                else
                {
                    // Populate list with this one
                    foreach (String t in geo._Ocd._Tags)
                    {
                        _TagsInit.Add(t);
                    }
                }

                // Ok, now we loop through all the caches and compute the intersection
                foreach (Geocache geo2 in _selCaches)
                {
                    // Nothing to intersect with ?
                    if ((geo2._Ocd == null) || (geo2._Ocd._Tags.Count == 0))
                    {
                        // This one has no tag, no need to look for an intersection
                        // there will be nothing !
                        _TagsInit.Clear();
                        return;
                    }

                    // Intersect
                    _TagsInit = _TagsInit.Intersect(geo2._Ocd._Tags).ToList();

                    // Intersection is empty ? If so, bye bye
                    if (_TagsInit.Count == 0)
                        return;
                }
            }

            foreach (String blo in _TagsInit)
            {
                ListViewItem item = new ListViewItem(blo);
                listView1tagmgr.Items.Add(item);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Look for tags to add
            foreach (ListViewItem item in listView1tagmgr.Items)
            {
                // To be added if not already present in the initial list
                if (_TagsInit.Contains(item.Text) == false)
                {
                    _TagsToAdd.Add(item.Text);
                }
            }

            // Look for tag to remove
            foreach (String t in _TagsInit)
            {
                // to be removed if not present in the listview
                if (listView1tagmgr.FindItemWithText(t) == null)
                {
                    _TagsToDel.Add(t);
                }
            }

            // Ok, now parse all the caches and update their tags
            foreach (Geocache cache in _selCaches)
            {
                // Create ocd is not existing
                if (cache._Ocd == null)
                {
                    OfflineCacheData ocd1 = new OfflineCacheData();
                    ocd1._dateExport = DateTime.Now;
                    ocd1._Comment = "";
                    ocd1._Code = cache._Code;
                    ocd1._bBookmarked = false;
                    _daddy.AssociateOcdCache(cache._Code, ocd1, cache);
                }

                // Add tags
                cache._Ocd.AddTags(_TagsToAdd);

                // Remove Tags
                cache._Ocd.RemoveTags(_TagsToDel);

                // Check if need to remove ocd data?
                if (cache._Ocd.IsEmpty())
                {
                    _daddy.RemoveAssociationOcdCache(cache._Code);
                }
            }
           
            // update tag list
            _daddy.CreateListStringTags();

            // Update listview 
            foreach (ListViewItem item in _selLVItems)
            {
                String code = item.Text;
                Geocache geo = _daddy._caches[code];
                EXMultipleImagesListViewSubItem subitem = ((EXMultipleImagesListViewSubItem)(item.SubItems[_daddy._ID_LVTag]));
                //if (true)
                {
                    ArrayList imgitem = _daddy.GetListImageTags(geo._Ocd);
                    if ((imgitem != null) && (imgitem.Count != 0))
                    {
                        subitem.MyImages = imgitem;
                        subitem.MyValue = geo._Ocd.GetTags();
                    }
                    else
                    {
                        subitem.MyImages = null;
                        subitem.MyValue = "";
                    }
                }/*
                else
                {
                    String t = "";
                    if (geo._Ocd != null)
                        t = geo._Ocd.GetTags();
                    subitem.Text = t;
                    subitem.MyValue = t;
                }*/
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            String tag = comboBox1tagmgr.Text;
            if (tag == "")
                return;
            tag = tag.ToLower();

            if (tag.Contains(";"))
            {
            	_daddy.MsgActionWarning(this, _daddy.GetTranslator().GetString("LblNoSemiColumnPlease"));
            }
            else
            {
                if (listView1tagmgr.FindItemWithText(tag) == null)
                {
                    // New tag
                    listView1tagmgr.Items.Add(tag);
                    if (comboBox1tagmgr.Items.Contains(tag) == false)
                        comboBox1tagmgr.Items.Add(tag);
                }
                comboBox1tagmgr.Text = "";
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem listViewItem in listView1tagmgr.SelectedItems)
            {
                listViewItem.Remove();
            }
        }
    }
}
