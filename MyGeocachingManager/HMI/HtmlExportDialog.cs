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
    /// Form to select options for HTML export
    /// </summary>
    public partial class HtmlExportDialog : Form
    {
        MainWindow _daddy = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        public HtmlExportDialog(MainWindow daddy)
        {
            _daddy = daddy;
            InitializeComponent();
            this.Text = _daddy.GetTranslator().GetString("DlgHtmlExportDialog");
            this.groupBox1.Text = _daddy.GetTranslator().GetString("GrpHtmlShowCacheDescription");
            this.radioButton1htmlexport.Text = _daddy.GetTranslator().GetString("BtnYes");
            this.radioButton2htmlexport.Text = _daddy.GetTranslator().GetString("BtnNo");
            this.groupBox2.Text = _daddy.GetTranslator().GetString("GrpHtmlCreateCacheMap");
            this.radioButton4htmlexport.Text = _daddy.GetTranslator().GetString("BtnYes");
            this.radioButton3htmlexport.Text = _daddy.GetTranslator().GetString("BtnNo");
            this.button1.Text = _daddy.GetTranslator().GetString("BtnOk");
            this.button2.Text = _daddy.GetTranslator().GetString("BtnCancel");
            _daddy.TranslateTooltips(this, null);
        }
    }
}
