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
    /// Form to enter a note on a cache
    /// </summary>
    public partial class NoteForm : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NoteForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}
