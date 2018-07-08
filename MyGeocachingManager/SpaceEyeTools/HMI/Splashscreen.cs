using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Configuration;

namespace SpaceEyeTools.HMI
{
    /// <summary>
    /// Lovely splashscreen window used during MGM startup
    /// </summary>
    public partial class Splashscreen : Form
    {
        private String _AssemblySubVersion = "";
		private String _ExtraInfo = "";

		/// <summary>
        /// Get / Set extra info
        /// </summary>
        public string ExtraInfo
        {
            get
            {
                return _ExtraInfo;
            }
            set
            {
                _ExtraInfo = value;
            }
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public Splashscreen()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            InitializeComponent();

            //this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            this.FormBorderStyle = FormBorderStyle.None;
            
            progressBar.ForeColor = Color.LimeGreen;
            ThreadProgress.CheckForIllegalCrossThreadCalls = false;
           
        }

        /// <summary>
        /// Get executing assembly version 
        /// </summary>
        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        /// <summary>
        /// Get / Set assembly subversion
        /// </summary>
        public string AssemblySubVersion
        {
            get
            {
                return _AssemblySubVersion;
            }
            set
            {
                _AssemblySubVersion = value;
            }
        }

        /// <summary>
        /// Get assembly product name
        /// </summary>
        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        private void SetControlOnPicture(Control lbl)
        {
            var pos = this.PointToScreen(lbl.Location);
            pos = pBox.PointToClient(pos);
            lbl.Parent = pBox;
            lbl.Location = pos;
            lbl.BackColor = Color.Transparent;
        }

        /// <summary>
        /// Set maximum number of steps for progress bar
        /// </summary>
        /// <param name="iMax">number of steps</param>
        public void SetMaximumSteps(int iMax)
        {
            progressBar.Maximum = iMax;
        }

        /// <summary>
        /// Update one information and performs a step
        /// </summary>
        /// <param name="info">information to display</param>
        /// <param name="dostep">if true, perform step on progress bar</param>
        /// <returns>always true</returns>
        public bool UpdateInfo(String info, bool dostep)
        {
            lblInfo.Text = info;
            if (dostep)
            	progressBar.PerformStep();

            return true;
        }

        /// <summary>
        /// Return full path to splashscreen image
        /// </summary>
        /// <param name="bExtra">return true if extra image used</param>
        /// <returns>full path to splashscreen image</returns>
        public static string GetSplashScreen(out bool bExtra)
        {
        	String exePath = Path.GetDirectoryName(Application.ExecutablePath);
            String splash = "";

            // Extra : existence d'une image bonus en fonction de la date ou de la période ?
            // OnActivated va parcourir tous les fichiers .jpg dans exePath + \Img\Extra
            String extradir = exePath + @"\Resources\Img\Extra\";
            bExtra = false;
            try
            {
                if (Directory.Exists(extradir))
                {
                    DateTime now = DateTime.Now;
                    string[] filePaths = Directory.GetFiles(extradir, "*.jpg", SearchOption.TopDirectoryOnly);
                    foreach (string f in filePaths)
                    {
                        String file = f.Replace(extradir, ""); 
                        if (file.StartsWith("XTRA") && file.EndsWith(".jpg") && (file.Length == 16))
                        {
                            // Candidate potentiel
                            file = file.Replace("XTRA", "");
                            file = file.Replace(".jpg", "");
                            
                            
                            //Maintenant le format est MMddMMdd avec date de début, date de fin
                            Int32 iMdeb = Int32.Parse(file.Substring(0, 2));
                            Int32 iDdeb = Int32.Parse(file.Substring(2, 2));
                            Int32 iMfin = Int32.Parse(file.Substring(4, 2));
                            Int32 iDfin = Int32.Parse(file.Substring(6, 2));
                            DateTime deb = new DateTime(now.Year, iMdeb, iDdeb, 0, 0, 1);
                            DateTime fin = new DateTime(now.Year, iMfin, iDfin, 23, 59, 59);

                            if ((deb <= now) && (now <= fin))
                            {
                                splash = f;
                                bExtra = true;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            if (!bExtra)
                splash = exePath + @"\splash.png";
            
            return splash;
        }
        
        private void Splashscreen_Load(object sender, EventArgs e)
        {
        	bool bExtra = false;
        	String splash = GetSplashScreen(out bExtra);
        	
            if (File.Exists(splash))
            {
                try
                {
                    pBox.Image = Image.FromFile(splash);
                }
                catch (Exception)
                {
                }
            }

            lblName.Text = AssemblyProduct;
            String ver = AssemblyVersion;
            if (AssemblySubVersion != "")
                ver += "." + AssemblySubVersion;
            else
            {
                String prefix = ".0";
                while (ver.EndsWith(prefix))
                {
                    ver = ver.Substring(0, ver.LastIndexOf(prefix));
                }
            }

            lblVersion.Text = String.Format("Version {0}", ver);
            if (bExtra)
            {
                lblInfo.ForeColor = Color.White;
                lblExtraInfo.ForeColor = Color.White;
                lblName.ForeColor = Color.White;
                lblName.Visible = true;
                lblVersion.ForeColor = Color.White;
            }
            else
            {
                lblInfo.ForeColor = Color.Black;
                lblExtraInfo.ForeColor = Color.Black;
                lblName.ForeColor = Color.Black;// White;
                lblVersion.ForeColor = Color.Black;// White;
            }
            SetControlOnPicture(lblName);
            SetControlOnPicture(lblVersion);
            SetControlOnPicture(lblInfo);
            SetControlOnPicture(lblExtraInfo);
            lblInfo.Text = "Warming up...";
            lblExtraInfo.Text = _ExtraInfo;

            //this.pBox.BackColor = Color.Transparent;
            //this.pBox.BorderStyle = BorderStyle.None;
            //((Bitmap)pBox.Image).MakeTransparent(((Bitmap)pBox.Image).GetPixel(0, 0));
            
            this.Activate();
            this.BringToFront();
        }
    }
}
