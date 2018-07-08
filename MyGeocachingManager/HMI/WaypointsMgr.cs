using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SpaceEyeTools;
using SpaceEyeTools.HMI;
using MyGeocachingManager.Geocaching;

namespace MyGeocachingManager.HMI
{
    /// <summary>
    /// Form to manage cache waypoints
    /// </summary>
    public partial class WaypointsMgr : Form
    {
        MainWindow _Daddy = null;
        Geocache _geo = null;
        ImageList _imageList = new ImageList();

        /// <summary>
        /// Construction
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        /// <param name="geo">Associated geocache</param>
        public WaypointsMgr(MainWindow daddy, Geocache geo)
        {
            _Daddy = daddy;
            _geo = geo;

            InitializeComponent();
            this.Icon = _Daddy.Icon;
            this.Font = _Daddy.Font;

            this.Text = _Daddy.GetTranslator().GetString("WaypointsMgr");
            btnWaypointMgrAdd.Text = _Daddy.GetTranslator().GetString("WaypointMgrAdd");
            btnWaypointMgrDel.Text = _Daddy.GetTranslator().GetString("WaypointMgrDel");
            btnWaypointMgrEdit.Text = _Daddy.GetTranslator().GetString("WaypointMgrEdit");
            btnCartoDisplay.Image = _Daddy.GetImageSized("Earth");

            lvWaypointsMgrView.Columns.Add(_Daddy.GetTranslator().GetString("WaypointCode"), 80);
            lvWaypointsMgrView.Columns.Add(_Daddy.GetTranslator().GetString("WaypointType"), 160);
            lvWaypointsMgrView.Columns.Add(_Daddy.GetTranslator().GetString("WaypointName"), 250);
            lvWaypointsMgrView.Columns.Add(_Daddy.GetTranslator().GetString("WaypointDate"), 100);
            lvWaypointsMgrView.Columns.Add(_Daddy.GetTranslator().GetString("LblWaypointFromUser"), 80);


            foreach(String s in GeocachingConstants.GetSupportedWaypointsType())
            {
            	_imageList.Images.Add(s, _Daddy.GetImageSized(s));
            }
            lvWaypointsMgrView.SmallImageList = _imageList;

            Dictionary<String, Waypoint> dicowpts = _geo.GetListOfWaypoints();
            foreach (KeyValuePair<String, Waypoint> paire in dicowpts)
            {
                Waypoint wpt = paire.Value;
                AddWptToList(wpt);
            }

            _Daddy.TranslateTooltips(this, null);
        }

        private void AddWptToList(Waypoint wpt)
        {
            // Le code
            ListViewItem item = lvWaypointsMgrView.Items.Add(wpt._name);
            item.Tag = wpt;

            // L'image
            item.ImageKey = wpt._type;

            // Le type
            item.SubItems.Add(_Daddy.GetTranslator().GetString("WptType" + wpt._type.Replace(" ", "")));

            // Le nom
            item.SubItems.Add(wpt._desc);

            // La date
            DateTime dt = MyTools.ParseDate(wpt._time);
            item.SubItems.Add(dt.ToString("yyyy/MM/dd hh:mm"));

            // By user ?
            if (wpt._eOrigin == Waypoint.WaypointOrigin.GPX)
                item.SubItems.Add(_Daddy.GetTranslator().GetString("WaypointOriginGPX"));
            else if (wpt._eOrigin == Waypoint.WaypointOrigin.CUSTOM)
                item.SubItems.Add(_Daddy.GetTranslator().GetString("WaypointOriginCrea"));
            else if (wpt._eOrigin == Waypoint.WaypointOrigin.MODIFIED)
                item.SubItems.Add(_Daddy.GetTranslator().GetString("WaypointOriginMod"));
        }

        private void btnWaypointMgrAdd_Click(object sender, EventArgs e)
        {
            Waypoint wpt = _Daddy.AddWaypointToCache(_geo);
            if (wpt != null)
            {
                AddWptToList(wpt);
            }
        }

        private void btnWaypointMgrDel_Click(object sender, EventArgs e)
        {
            if (lvWaypointsMgrView.SelectedItems.Count != 0)
            {
                ListViewItem item = lvWaypointsMgrView.SelectedItems[0];
                Waypoint wpt = item.Tag as Waypoint;
                String code = wpt._name;
                if (_Daddy.RemoveWaypoint(wpt))
                {
                    // On doit mettre à jour (rollback)
                    // ou supprimer
                    Geocache cache = _Daddy._caches[wpt._GCparent];
                    if (cache._waypoints.ContainsKey(code))
                    {
                        // On va faire un rollback !
                        UpdateWptItem(item, cache._waypoints[code]);
                    }
                    else
                        lvWaypointsMgrView.Items.Remove(item);
                }
            }
        }

        private void UpdateWptItem(ListViewItem item, Waypoint wpt)
        {
            // Le code
            item.Text = wpt._name;
            item.Tag = wpt;

            // L'image
            item.ImageKey = wpt._type;

            // Le type
            item.SubItems[1].Text = _Daddy.GetTranslator().GetString("WptType" + wpt._type.Replace(" ", ""));

            // Le nom
            item.SubItems[2].Text = wpt._desc;

            // La date
            DateTime dt = MyTools.ParseDate(wpt._time);
            item.SubItems[3].Text = dt.ToString("yyyy/MM/dd hh:mm");

            // By user ? forcément oui !
            if (wpt._eOrigin == Waypoint.WaypointOrigin.GPX)
                item.SubItems[4].Text = (_Daddy.GetTranslator().GetString("WaypointOriginGPX"));
            else if (wpt._eOrigin == Waypoint.WaypointOrigin.CUSTOM)
                item.SubItems[4].Text = (_Daddy.GetTranslator().GetString("WaypointOriginCrea"));
            else if (wpt._eOrigin == Waypoint.WaypointOrigin.MODIFIED)
                item.SubItems[4].Text = (_Daddy.GetTranslator().GetString("WaypointOriginMod"));
        }

        private void btnWaypointMgrEdit_Click(object sender, EventArgs e)
        {
            EditWpt();
        }

        private void EditWpt()
        {
            if (lvWaypointsMgrView.SelectedItems.Count != 0)
            {
                ListViewItem item = lvWaypointsMgrView.SelectedItems[0];
                Waypoint wpt = item.Tag as Waypoint;
                Waypoint wptedit = _Daddy.EditWaypoint(wpt);
                if (wptedit != null)
                {
                    // On doit mettre à jour
                    UpdateWptItem(item, wptedit);
                }
            }
        }

        private void lvWaypointsMgrView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            EditWpt();
        }

        private void btnCartoDisplay_Click(object sender, EventArgs e)
        {
            _Daddy.ShowCacheMapInCacheDetail();
        }
    }
}
