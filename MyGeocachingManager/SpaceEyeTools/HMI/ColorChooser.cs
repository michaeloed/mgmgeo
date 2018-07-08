using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SpaceEyeTools.HMI
{
    /// <summary>
    /// Form to select a color among the KnownColor enumeration.
    /// Not used in MGM ;-)
    /// </summary>
    public partial class ColorChooser : Form
    {
        KnownColor[] allColors;

        /// <summary>
        /// Constructor
        /// </summary>
        public ColorChooser()
        {
            InitializeComponent();

            // Get all the values from the KnownColor enumeration.
            System.Array colorsArray = Enum.GetValues(typeof(KnownColor));
            allColors = new KnownColor[colorsArray.Length];

            Array.Copy(colorsArray, allColors, colorsArray.Length);

            for (int i = 0; i < allColors.Length; i++)
            {
                ListViewItem item = new ListViewItem(allColors[i].ToString());
                item.BackColor = Color.FromName(allColors[i].ToString());
                item.Tag = i;
                listView1.Items.Add(item);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 0)
            {
                ListViewItem item = listView1.SelectedItems[0];
                String text = allColors[(int)(item.Tag)].ToString();
                Color c = Color.FromName(text);
                pictureBox1.Image = MyTools.CreateBitmapImage(text, Color.Black, c);
                pictureBox2.Image = MyTools.CreateBitmapImage(text, Color.White, c);
            }
        }

        
    }
}
