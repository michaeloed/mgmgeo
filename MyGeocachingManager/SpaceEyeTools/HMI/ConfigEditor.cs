using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SpaceEyeTools.EXControls;
using System.Configuration;

namespace SpaceEyeTools.HMI
{
    /// <summary>
    /// A simple form to edit configuration file
    /// </summary>
    public partial class ConfigEditor : Form
    {
        TextBox txtbx = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public ConfigEditor()
        {
            InitializeComponent();
        }

        private void txtbx_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                txtbx.Visible = false;
                // c_leave sera automatiquement appelé
            }
        }

        /// <summary>
        /// Override escape sequence during TextBox editing
        /// </summary>
        /// <param name="msg">message</param>
        /// <param name="keyData">key data</param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape && txtbx.Focused)
            {
                if (elvConfigcfgeditor._clickedsubitem != null)
                {
                    // On annule tout !
                    txtbx.Text = elvConfigcfgeditor._clickedsubitem.Text;
                    txtbx.Visible = false;
                    elvConfigcfgeditor._clickeditem.Tag = null;
                    // c_leave sera automatiquement appelé
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void c_Leave(object sender, EventArgs e)
        {
            Control c = (Control)sender;
            // On met à jour la config
            ConfigChanged(elvConfigcfgeditor._clickedsubitem, elvConfigcfgeditor._clickeditem.Text, c);
            elvConfigcfgeditor._clickedsubitem.Text = c.Text;

            // Sinon on ne touche à rien
            c.Visible = false;
            elvConfigcfgeditor._clickeditem.Tag = null;
        }

        private bool ConfigChanged(ListViewItem.ListViewSubItem subItem, String key, Control cvalue)
        {
            if (subItem.Text == cvalue.Text)
            {
                //MessageBox.Show("Pas de modification !");
                return false;
            }
            else
            {
                try
                {
                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings.Remove(key);
                    // On stocke dans le fichier de config
                    config.AppSettings.Settings.Add(key, cvalue.Text);
                    ConfigurationManager.RefreshSection("appSettings");
                    config.Save(ConfigurationSaveMode.Modified);
                    return true;
                }
                catch (Exception)
                {
                    cvalue.Text = subItem.Text;
                    return false;
                }
            }
        }

        private void ConfigEditor_Load(object sender, EventArgs e)
        {
            // Load columns & co
            elvConfigcfgeditor.Columns.Add(new EXColumnHeader("Name", 200));

            // Control textbox for edition
            txtbx = new TextBox();
            txtbx.Visible = false;
            txtbx.ForeColor = Color.Blue;
            elvConfigcfgeditor.Controls.Add(txtbx);
            txtbx.Leave += new EventHandler(c_Leave);
            txtbx.KeyPress += new KeyPressEventHandler(txtbx_KeyPress);

            elvConfigcfgeditor.Columns.Add(new EXEditableColumnHeader("Value", txtbx, 400));

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            foreach(KeyValueConfigurationElement  keyValueElement  in config.AppSettings.Settings)
            {
                EXListViewItem item = new EXListViewItem(keyValueElement.Key);
                elvConfigcfgeditor.Items.Add(item);
                EXListViewSubItem subitem = new EXListViewSubItem(keyValueElement.Value);
                item.SubItems.Add(subitem);
            }
        }
    }
}
