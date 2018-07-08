using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;


namespace MyGeocachingManager
{
    /// <summary>
    /// Form to configure images and spoiler download:
    /// - Configure list of spoilers keywords
    /// - Configure delay between two images download
    /// - Configure usage of keywords for spoiler detection
    /// </summary>
    public partial class SpoilerKeywords : Form
    {
        MainWindow _daddy = null;
        /// <summary>
        /// List of spoiler keywords, separated by ¤
        /// </summary>
        public String _sSpoilerDefaultKeywords = "";

        /// <summary>
        /// Construction
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        public SpoilerKeywords(MainWindow daddy)
        {
            _daddy = daddy;
            InitializeComponent();
            button1.Text = _daddy.GetTranslator().GetString("BtnOk");
            button2.Text = _daddy.GetTranslator().GetString("BtnCancel");
            button3spoilerkw.Text = _daddy.GetTranslator().GetString("BtnReinitSpoilerKeywords");
            this.Text = _daddy.GetTranslator().GetString("DlgSpoilerKeywords");
            textBoxExpl.Text = _daddy.GetTranslator().GetString("LblSpoilerKeyWordExpl");
            checkBox1spoilerkw.Text = _daddy.GetTranslator().GetString("CbUseSpoilerKeywords");
            label1.Text = _daddy.GetTranslator().GetString("LblDelayBetweenCaches");
            checkBoxGallery.Text = _daddy.GetTranslator().GetStringM("checkBoxGallery");

            comboBox1spoilerkw.Items.Add(_daddy.GetTranslator().GetString("LblNone"));
            comboBox1spoilerkw.Items.Add("1s");
            comboBox1spoilerkw.Items.Add("3s");
            comboBox1spoilerkw.Items.Add("5s");
            comboBox1spoilerkw.Items.Add("10s");
            String delay = ConfigurationManager.AppSettings["spoilerdelaydownload"];
            switch (delay)
            {
                case "0":
                    comboBox1spoilerkw.SelectedIndex = 0;
                    break;
                case "1":
                    comboBox1spoilerkw.SelectedIndex = 1;
                    break;
                case "3":
                    comboBox1spoilerkw.SelectedIndex = 2;
                    break;
                case "5":
                    comboBox1spoilerkw.SelectedIndex = 3;
                    break;
                case "10":
                    comboBox1spoilerkw.SelectedIndex = 4;
                    break;
                default:
                    comboBox1spoilerkw.SelectedIndex = 0;
                    break;
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.AutoPopDelay = 0;
            toolTip1.InitialDelay = 0;
            toolTip1.ReshowDelay = 0;
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(comboBox1spoilerkw, _daddy.GetTranslator().GetString("TipDelayBetweenCaches"));
            
            String keystxt = ConfigurationManager.AppSettings["spoilerskeywords"];
            PopulateKeywordsList(keystxt);
            _daddy.TranslateTooltips(this, null);
        }

        private void PopulateKeywordsList(String keystxt)
        {
            textBox1spoilerkw.Text = "";
            if (keystxt == "")
                return;
            try
            {
                keystxt = keystxt.ToLower();
                List<string> keys = keystxt.Split('¤').ToList<string>();
                if (keys != null)
                {
                    foreach (String key in keys)
                    {
                        if (key != "")
                        {
                            textBox1spoilerkw.Text += key + "\r\n";
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Return list of spoiler keywords, separated by a ¤
        /// Case insensitive
        /// </summary>
        /// <returns>list of spoiler keywords, separated by a ¤</returns>
        public String GetKeywords()
        {
            String s = textBox1spoilerkw.Text.Replace("\r\n\r\n", "\r\n");
            s = s.Replace("\r\n", "¤");
            s = s.ToLower();
            return s;

        }

        void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            ConsiderSpoilerKeywords();
        }

        /// <summary>
        /// Check if keywords shall be considered for spoiler detection
        /// checked : spoiler keywords will be used
        /// </summary>
        public void ConsiderSpoilerKeywords()
        {
            if (checkBox1spoilerkw.Checked)
            {
                textBox1spoilerkw.Enabled = true;
            }
            else
            {
                textBox1spoilerkw.Enabled = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(_daddy.GetTranslator().GetString("AskConfirm"),
                _daddy.GetTranslator().GetString("WarTitle"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                PopulateKeywordsList(_sSpoilerDefaultKeywords);
            }
        }
    }
}
