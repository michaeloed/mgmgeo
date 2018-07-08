using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using SpaceEyeTools;
using SpaceEyeTools.HMI;
using System.Configuration;
using System.CodeDom.Compiler;
using Microsoft.Win32;

namespace MyGeocachingManager
{
    /// <summary>
    /// "About" class Form, including an easter-egg ;-)
    /// A Konami sequence?
    /// A double click on something?
    /// </summary>
    public partial class AboutBox : Form
    {
        //private int iClick = 0;
        MainWindow _daddy = null;
        private KonamiSequence sequence = new KonamiSequence();
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        public AboutBox(MainWindow daddy)
        {
            _daddy = daddy;

            
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            InitializeComponent();

            bool bExtra = false;
        	String splash = Splashscreen.GetSplashScreen(out bExtra);
            if (File.Exists(splash))
            {
                try
                {
                    pictureBox1.Image = Image.FromFile(splash);
                }
                catch (Exception)
                {
                }
            }
            
            this.Text = String.Format("About {0}", AssemblyTitle);
            lblExtraInfo.Text = "";
            
            String ver = AssemblyVersion;
            if (MainWindow.AssemblySubVersion != "")
            {
                ver += "." + MainWindow.AssemblySubVersion;
                lblExtraInfo.Text = "Build " + _daddy.GetBuildTime() + " ";
            }
            else
            {
                String prefix = ".0";
                while (ver.EndsWith(prefix))
                {
                    ver = ver.Substring(0, ver.LastIndexOf(prefix));
                }
            }
			String dotnet = MyTools.GetHighestInstalledFramework();
            if (dotnet != "")
            	lblExtraInfo.Text += ".Net " + MyTools.GetHighestInstalledFramework();
            
            this.labelVersion.Text = String.Format("Version {0}", ver);
            this.labelCopyright.Text = AssemblyCopyright;
            this.label5.Text = AssemblyDescription;
            
            try
            {
            	txtAssemblies.Text = MyTools.GetOSAssembliesFrameworkInfo();
            }
            catch(Exception ex)
            {
            	txtAssemblies.Text = ex.Message;
            }
            
            MakeTransparentControl(labelVersion, bExtra);
            MakeTransparentControl(labelCopyright, bExtra);
            MakeTransparentControl(label5, bExtra);
            MakeTransparentControl(linkLabel1, bExtra);
            MakeTransparentControl(linkLabel2, bExtra);
            MakeTransparentControl(linkLabel3, bExtra);
            MakeTransparentControl(linkLabel4, bExtra);
            MakeTransparentControl(linkLabel5, bExtra);
            MakeTransparentControl(label1, bExtra);
            MakeTransparentControl(label2, bExtra);
            MakeTransparentControl(label3, bExtra);
            MakeTransparentControl(label4, bExtra);
            MakeTransparentControl(lblExtraInfo, bExtra);
            MakeTransparentControl(lockPictureBox, bExtra);
            //MakeTransparentControl(txtAssemblies, bExtra);
            
            // Le lock / unlock ?
            if (SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled())
            {
            	// unlock
            	lockPictureBox.Image = _daddy.GetImageSized("Lock-Unlock-icon");
            }
            else
            {
            	// lock
            	lockPictureBox.Image = _daddy.GetImageSized("Lock-Lock-icon");
            }
                       
        }

        void MakeTransparentControl(Control ctrl, bool bWhite)
        {
        	var pos = this.PointToScreen(ctrl.Location);
	        pos = pictureBox1.PointToClient(pos);
	        ctrl.Parent = pictureBox1;
	        ctrl.Location = pos;
	        ctrl.BackColor = Color.Transparent;
	        if (bWhite)
	        	ctrl.ForeColor = Color.White;
        }
        
        #region Assembly Attribute Accessors

        /// <summary>
        /// Set general description
        /// </summary>
        public string TextBoxDescription
        {
            set
            {
                this.label5.Text = value;
            }
        }


        /// <summary>
        /// Get assembly title
        /// </summary>
        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        /// <summary>
        /// Assembly version of MGM
        /// </summary>
        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        /// <summary>
        /// Get assembly version
        /// </summary>
        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        /// <summary>
        /// Get assembly product
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

        /// <summary>
        /// Get assembly copyright
        /// </summary>
        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        /// <summary>
        /// Get assembly company
        /// </summary>
        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion


        private void logoPictureBox_DoubleClick(object sender, EventArgs e)
        {
            _daddy.DisplayHideDebugBtn();
        }
        
		void LinkLabel5LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			MyTools.StartInNewThread("https://greatmaps.codeplex.com/");
		}
		
		void LinkLabel1LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			MyTools.StartInNewThread(linkLabel1.Text);
		}
		
		void LinkLabel2LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			MyTools.StartInNewThread(linkLabel2.Text);
		}
		
		void LinkLabel3LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			MyTools.StartInNewThread(linkLabel3.Text);
		}
		
		void LinkLabel4LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			MyTools.StartInNewThread("mailto:" + linkLabel4.Text);
		}
		void AboutBoxKeyUp(object sender, KeyEventArgs e)
		{
			if (sequence.IsCompletedBy(e.KeyCode))
            {
				String f = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "Catastrophe.cs";
				if (File.Exists(f))
				{
					CompilerResults cr;
					if (_daddy.CompilePlugin(f, out cr))
	                {
	                    _daddy.GetPlugins(cr.CompiledAssembly, true);
	                    _daddy.CreatePluginMenu();
	                    _daddy.ShowCacheMapInCacheDetail();
						_daddy.MsgActionWarning(this, _daddy.GetTranslator().GetStringM("EasterEggCataEnabled"));
					}
				}
            }
		}

        private void lockPictureBox_DoubleClick(object sender, EventArgs e)
        {
            if (!SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled())
            {
                if (_daddy.enableGoldMode())
                {
                    lockPictureBox.Image = _daddy.GetImageSized("Lock-Unlock-icon");
                }
            }
        }
    }
}
