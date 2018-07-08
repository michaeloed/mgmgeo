using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace MyGeocachingManager
{
    /// <summary>
    /// Form to select Image export options
    /// </summary>
    public partial class ImageExportDialog : Form
    {
        MainWindow _daddy = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        public ImageExportDialog(MainWindow daddy)
        {
            _daddy = daddy;
            InitializeComponent();
            this.Text = _daddy.GetTranslator().GetString("DlgImgExportText");
            this.groupBox1.Text = _daddy.GetTranslator().GetString("DlgImgExportTypes");
            this.radioButton1imgexport.Text = _daddy.GetTranslator().GetString("DlgImgExportGarmin");
            this.radioButton2imgexport.Text = _daddy.GetTranslator().GetString("DlgImgExportGeocoded");
            this.radioButton3imgexport.Text = _daddy.GetTranslator().GetString("DlgImgExportExploristGC");
            this.button1.Text = _daddy.GetTranslator().GetString("BtnOk");
            this.button2.Text = _daddy.GetTranslator().GetString("BtnCancel");

            this.button3imgexport.Text = _daddy.GetTranslator().GetString("BtnConfigure");
            this.radioButton6imgexport.Text = _daddy.GetTranslator().GetString("BtnYes");
            this.radioButton7imgexport.Text = _daddy.GetTranslator().GetString("BtnYes");
            this.radioButton4imgexport.Text = _daddy.GetTranslator().GetString("BtnNo");
            this.radioButton5imgexport.Text = _daddy.GetTranslator().GetString("BtnNo");
            this.groupBox2.Text = _daddy.GetTranslator().GetString("GrpLimitToSpoilers");
            this.groupBox3.Text = _daddy.GetTranslator().GetString("GrpFilterKeyWords");

            this.cbSpoilerGarminNormalimgexport.Text = _daddy.GetTranslator().GetString("cbConsiderGarminSpoilerAsNormalPics");

            //  Default text for button1
            textBoxStatus.Text = _daddy.GetTranslator().GetString("DlgImgExportGarminExpl");
            _daddy.TranslateTooltips(this, null);
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1imgexport.Checked)
            {
                textBoxStatus.Text = _daddy.GetTranslator().GetString("DlgImgExportGarminExpl");
                cbSpoilerGarminNormalimgexport.Enabled = true;
            }
            else if (radioButton2imgexport.Checked)
            {
                textBoxStatus.Text = _daddy.GetTranslator().GetString("DlgImgExportGeocodedExpl");
                cbSpoilerGarminNormalimgexport.Enabled = false;
            }
            else if (radioButton3imgexport.Checked)
            {
                textBoxStatus.Text = _daddy.GetTranslator().GetString("DlgImgExportExploristGCExpl");
                cbSpoilerGarminNormalimgexport.Enabled = false;
            }
            else
            {
                textBoxStatus.Text = "";
                cbSpoilerGarminNormalimgexport.Enabled = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _daddy.ConfigureSpoilerDownload();
        }
    }
}
