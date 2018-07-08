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
    /// Class holding basic Geocaching information on a user
    /// </summary>
    public partial class UserInfo : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="user">User name</param>
        /// <param name="avatarurl">URL to avatar image</param>
        /// <param name="membership">Type of membership</param>
        /// <param name="membersince">Date of subscription to GC.com</param>
        /// <param name="find">Number of found it</param>
        /// <param name="hide">Number of caches hiden</param>
        /// <param name="tb">Number of owned travel bugs : geocacoins</param>
        /// <param name="fav">Number of favorites in hand</param>
        public UserInfo(String user, String avatarurl, String membership, String membersince, String find, String hide, String tb, String fav)
        {
            InitializeComponent();
            this.Text = user;
            try
            {
                pictureBox1.Load(avatarurl);
            }
            catch (Exception)
            { }
            label7.Text = membership;
            label8.Text = membersince;
            label10.Text = find;
            label11.Text = hide;
            label12.Text = tb;
            label9.Text = fav;
        }
    }
}
