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
using System.IO;
using SpaceEyeTools;
using SpaceEyeTools.HMI;
using MyGeocachingManager.Geocaching;

namespace MyGeocachingManager
{
    /// <summary>
    /// Form to manage bookmarks
    /// </summary>
    public partial class FavManager : Form
    {
        MainWindow _daddy = null;
        TextCoord _coord = new TextCoord();
        ListTextCoord _bmarks = null;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        public FavManager(MainWindow daddy)
        {
            _daddy = daddy;
            InitializeComponent();

            this.Text = _daddy.GetTranslator().GetString("DlgFavManager");
            this.label1.Text = _daddy.GetTranslator().GetString("LblName");
            this.label2.Text = _daddy.GetTranslator().GetString("LblLatLon");
            this.favmgrbutton1.Text = _daddy.GetTranslator().GetString("BtnOk");
            this.favmgrbutton2.Text = _daddy.GetTranslator().GetString("BtnCancel");
            favmgrlistView1.Columns.Add(_daddy.GetTranslator().GetString("LblName"), 200);
            _daddy.TranslateTooltips(this, null);
            _bmarks = _daddy.GetBookmarks();

            foreach (TextCoord crd in _bmarks._TextCoords)
            {
                ListViewItem item = favmgrlistView1.Items.Add(crd._Name);
                item.Tag = crd;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ListTextCoord bmarks = new ListTextCoord();
            // copy database
            bmarks._Databases = _bmarks._Databases;

            // Now create new coords
            foreach (ListViewItem item in favmgrlistView1.Items)
            {
                TextCoord crd = item.Tag as TextCoord;
                bmarks._TextCoords.Add(crd);
            }

            // save 
            _daddy.SaveBookmarks(bmarks);

            // Maj du pere
            _daddy.PostTreatmentLoadCache();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }


        private void btnLoad_Click(object sender, EventArgs e)
        {
            _coord = new TextCoord();
            DisplayTextCoord(_coord);
        }

        private void DisplayTextCoord(TextCoord crd)
        {
            if (crd != null)
            {
                this.favmgrtextBox1.Text = crd._Name;
                this.favmgrtextBox2.Text = crd._Lat + " " + crd._Lon;
            }
        }

        private String performValidityCheckAndUpdateHMI()
        {
            // Check if valid
            String err = "";

            if (favmgrtextBox1.Text == "")
            {
                err += _daddy.GetTranslator().GetString("ErrEmptyName") + "\r\n";
            }


            Double dlon = Double.MaxValue;
            Double dlat = Double.MaxValue;
            if (ParameterObject.SplitLongitudeLatitude(favmgrtextBox2.Text, ref dlon, ref dlat))
            {
                // Ok tout va bien
            }
            else
            {
                String format = _daddy.GetTranslator().GetString("ErrWrongParameter");
                err += String.Format(format.Replace("#", "\r\n"), label2.Text, typeof(double).ToString(), favmgrtextBox2.Text, "") + "\r\n";
            }

            if (err == "")
            {
                // It's valid,
                _coord._Name = favmgrtextBox1.Text;
                _coord._Lat = dlat.ToString().Replace(",", ".");
                _coord._Lon = dlon.ToString().Replace(",", ".");
            }

            return err;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Check if valid
            String err = performValidityCheckAndUpdateHMI();

            if (err != "")
            {
                MessageBox.Show(err);
            }
            else
            {
                // either we overwrite existing or we create a new
                bool bFound = false;
                ListViewItem itemFound = null;
                foreach (ListViewItem item in favmgrlistView1.Items)
                {
                    TextCoord crd = item.Tag as TextCoord;
                    if (crd != null)
                    {
                        if (crd._Uid == _coord._Uid)
                        {
                            itemFound = item;
                            bFound = true;
                            break;
                        }
                    }
                }
                
                if (!bFound)
                {
                    // Add new
                    itemFound = favmgrlistView1.Items.Add(_coord._Name);
                    itemFound.Tag = _coord;
                }
                else
                {
                    // replace old
                    itemFound.Tag = _coord;
                    itemFound.Text = _coord._Name;
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            bool bFound = false;
            ListViewItem itemFound = null;
            foreach (ListViewItem item in favmgrlistView1.Items)
            {
                TextCoord crd = item.Tag as TextCoord;
                if (crd != null)
                {
                    if (crd._Uid == _coord._Uid)
                    {
                        itemFound = item;
                        bFound = true;
                        break;
                    }
                }
            }
            if (bFound)
                favmgrlistView1.Items.Remove(itemFound);
            _coord = new TextCoord();
            DisplayTextCoord(_coord);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            String latlon = favmgrtextBox2.Text;
            Double dlon = Double.MaxValue;
            Double dlat = Double.MaxValue;
            if (ParameterObject.SplitLongitudeLatitude(latlon, ref dlon, ref dlat))
            {
                _daddy.HandlerToDisplayCoordinates(dlat, dlon);
            }            
        }


        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem lstvItem = favmgrlistView1.GetItemAt(e.X, e.Y);
            if ((lstvItem != null) && (lstvItem.Tag != null))
            {
                _coord = lstvItem.Tag as TextCoord;
                DisplayTextCoord(_coord);
            }
        }

        private void FavManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            _daddy.UpdateBookmarkOverlay(_daddy.GetBookmarks());
        }

        private void favmgrbtnEdit_Click(object sender, EventArgs e)
        {
            Double dlon ,dlat;
            _daddy.GetInitialCoordinates(out dlat, out dlon);
            
            if (ParameterObject.SplitLongitudeLatitude(favmgrtextBox2.Text, ref dlon, ref dlat))
            {
                // Ok tout va bien
            }
            else
            {
                dlon = _daddy.HomeLon;
                dlat = _daddy.HomeLat;
            }
            
            List<ParameterObject> lst = new List<ParameterObject>();
            lst.Add(new ParameterObject(ParameterObject.ParameterType.String, favmgrtextBox1.Text, "nme", label1.Text));
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Coordinates/*good*/,
                dlat.ToString() + " " + dlon.ToString(),
                "latlon",
                label2.Text,
                _daddy.GetTranslator().GetString("TooltipParamLatLon").Replace("#","\r\n")));
            ParametersChanger changer = new ParametersChanger();
            changer.HandlerDisplayCoord = _daddy.HandlerToDisplayCoordinates;
            changer.DisplayCoordImage = _daddy.GetImageSized("Earth");
            changer.Title = _daddy.GetTranslator().GetString("DlgFavManager");
            changer.BtnCancel = _daddy.GetTranslator().GetString("BtnCancel");
            changer.BtnOK = _daddy.GetTranslator().GetString("BtnOk");
            changer.ErrorFormater = _daddy.GetTranslator().GetString("ErrWrongParameter");
            changer.ErrorTitle = _daddy.GetTranslator().GetString("Error");
            changer.Parameters = lst;
            changer.Font = this.Font;
            changer.Icon = this.Icon;
            
            if (changer.ShowDialog() == DialogResult.OK)
            {
                dlon = Double.MaxValue;
                dlat = Double.MaxValue;
                if (ParameterObject.SplitLongitudeLatitude(lst[1].Value, ref dlon, ref dlat))
                {
                    this.favmgrtextBox1.Text = lst[0].Value;
                    this.favmgrtextBox2.Text = dlat.ToString().Replace(",", ".") + " " + dlon.ToString().Replace(",", ".");
                    performValidityCheckAndUpdateHMI();
                }
            }
        }

    }
}
