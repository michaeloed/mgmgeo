using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SpaceEyeTools;
using System.IO;

namespace SpaceEyeTools.HMI
{
    /// <summary>
    /// Custom form to display a message MessageBox replacement) with support
    /// of icons and type of answer
    /// </summary>
    public partial class MyMessageBox : Form
    {
        private bool _bYesNo = false;

        private ParameterObject _extraCheckBox = null;
        
        /// <summary>
        /// Maximum width for the window
        /// If larger, scrollbars will be used
        /// </summary>
        public static int _iMaxWidth = 640;
        
        /// <summary>
        /// Maximum height for the window
        /// If longer, scrollbars will be used
        /// </summary>
        public static int _iMaxHeight = 480;

        /// <summary>
        /// Create a message box
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="title">Title</param>
        /// <param name="icon">Icon</param>
        /// <param name="translator">reference to a translator to translate OK and Cancel buttons</param>
        /// <param name="lblYes">If not null and not empty, label for the Yes button (of key forthis label if translator provided)</param>
        /// <param name="lblNo">If not null and not empty, label for the No button (of key forthis label if translator provided)</param>
        /// <param name="img">can replace icon</param>
        /// <returns>result of user validation</returns>
        public static DialogResult Show(String msg, String title, MessageBoxIcon icon, TranslationManager translator, String lblYes = null, String lblNo = null, Image img = null)
        {
            MyMessageBox box = new MyMessageBox(msg, title, icon, img, null, translator, lblYes, lblNo);
            box.TopMost = true;
            DialogResult dr = box.ShowDialog();
            return dr;
        }
        
        /// <summary>
        /// Create a message box
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="title">Title</param>
        /// <param name="icon">Icon</param>
        /// <param name="extraCheckBox">Title for an extra checkbox at the bottom of the message box. If NULL or type different of BOOL or label empty, no extra checkbox</param>
        /// <param name="translator">reference to a translator to translate OK and Cancel buttons</param>
        /// <param name="lblYes">If not null and not empty, label for the Yes button (of key forthis label if translator provided)</param>
        /// <param name="lblNo">If not null and not empty, label for the No button (of key forthis label if translator provided)</param>
        /// <returns>result of user validation</returns>
        public static DialogResult Show(String msg, String title, MessageBoxIcon icon, ParameterObject extraCheckBox, TranslationManager translator, String lblYes = null, String lblNo = null)
        {
            MyMessageBox box = new MyMessageBox(msg, title, icon, null, extraCheckBox, translator, lblYes, lblNo);
            box.TopMost = true;
            DialogResult dr = box.ShowDialog();
            return dr;
        }

        /// <summary>
        /// Create a message box
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="title">Title</param>
        /// <param name="img">custom image to display</param>
        /// <param name="translator">reference to a translator to translate OK and Cancel buttons</param>
        /// <param name="lblYes">If not null and not empty, label for the Yes button (of key forthis label if translator provided)</param>
        /// <param name="lblNo">If not null and not empty, label for the No button (of key forthis label if translator provided)</param>
        /// <returns>result of user validation</returns>
        public static DialogResult Show(String msg, String title, Image img, TranslationManager translator, String lblYes = null, String lblNo = null)
        {
            MyMessageBox box = new MyMessageBox(msg, title, MessageBoxIcon.Information, img, null, translator, lblYes, lblNo);
            box.TopMost = true;
            DialogResult dr = box.ShowDialog();
            return dr;
        }

        static Image GetMajorFailureImg()
        {
        	try
        	{
        		String exePath = Path.GetDirectoryName(Application.ExecutablePath);
        		String f = exePath + Path.DirectorySeparatorChar + "majorfailure.gif";
        		if (File.Exists(f))
        		{
        			Image img = Image.FromFile(f);
        			return img;
        		}
        		return null;
        	}
        	catch(Exception)
        	{
        		return null;
        	}
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="msg">Message</param>
        /// <param name="title">Title</param>
        /// <param name="icon">Icon</param>
        /// <param name="img">custom image to display</param>
        /// <param name="extraCheckBox">Title for an extra checkbox at the bottom of the message box. If NULL or type different of BOOL or label empty, no extra checkbox</param>
        /// <param name="translator">reference to a translator to translate OK and Cancel buttons</param>
        /// <param name="lblYes">If not null and not empty, label for the Yes button (of key forthis label if translator provided)</param>
        /// <param name="lblNo">If not null and not empty, label for the No button (of key forthis label if translator provided)</param>
        public MyMessageBox(String msg, String title, MessageBoxIcon icon, Image img, ParameterObject extraCheckBox, TranslationManager translator, String lblYes = null, String lblNo = null)
        {
            InitializeComponent();
            _extraCheckBox = extraCheckBox;
            _bYesNo = false;
            this.Text = title;
            textBox1.Text = msg;
            Icon ico = null;
            Image iImage = img;
            /*
		None,
		Hand, Stop, Error = 16,
		Question = 32,
		Exclamation, Warning = 48,
		Information, Asterisk = 64,
            */
            switch (icon)
            {
                case MessageBoxIcon.Exclamation: // also = Warning
                    ico = SystemIcons.Exclamation;
                    break;
                case MessageBoxIcon.Error: // also = Error, Stop
                    ico = SystemIcons.Error;
                    if (iImage == null) // si on a fourni une image, on ne l'écrase pas
                    	iImage = GetMajorFailureImg();
                    break;
                case MessageBoxIcon.Information: // also = Asterix
                    ico = SystemIcons.Information;
                    break;
                case MessageBoxIcon.None:
                    ico = null;
                    break;
                case MessageBoxIcon.Question:
                    ico = SystemIcons.Question;
                    _bYesNo = true;
                    break;
                default:
                    ico = SystemIcons.Information;
                    break;
            }

            if (ico != null)
            {
            	pictureBox1.Image = ico.ToBitmap();
                this.Icon = ico;
            }
            
            if (iImage != null)
            {
                /*
                int hDisp = pictureBox2.Top - textBox1.Top - 1;
                textBox1.Height = hDisp;
                */

                // Position on the left edge
                pictureBox2.Left = 3;

                // Center in the middle of the window
                int w = iImage.Width;
                int h = iImage.Height;

                if (h >= this.ClientSize.Height)
                    pictureBox2.Top = 0;
                else
                {
                    pictureBox2.Top = (this.ClientSize.Height - h) / 2;
                }
                pictureBox2.Image = iImage;

                textBox1.Left = pictureBox2.Left + pictureBox2.Width + 3;
                int tw = this.ClientSize.Width - textBox1.Left - 3;
                if (tw > 0)
                    textBox1.Width = tw;

                pictureBox1.Visible = false;
            }

            if (translator != null)
            {
                if (_bYesNo)
                {
                	if (String.IsNullOrEmpty(lblYes))
                    	button1.Text = translator.GetString("BtnYes");
                	else
                		button1.Text = translator.GetString(lblYes);
                    if (String.IsNullOrEmpty(lblNo))
                    	button2.Text = translator.GetString("BtnNo");
                    else
                    	button2.Text = translator.GetString(lblNo);
                }
                else
                {
                    button1.Text = translator.GetString("BtnTrueOK");
                    button2.Text = translator.GetString("BtnCancel");
                }
            }
            else
            {
                if (_bYesNo)
                {
                    if (String.IsNullOrEmpty(lblYes))
                    	button1.Text = "Yes";
                    else
                    	button1.Text = lblYes;
                    if (String.IsNullOrEmpty(lblNo))
                    	button2.Text = "No";
                    else
                    	button2.Text = lblNo;
                }
                else
                {
                    button1.Text = "OK";
                    button2.Text = "Cancel";
                }
            }

            // On essaie d'adapter la largeur de TextBox1 à son contenu
            // On va prendre la ligne la plus large
            if (msg != "")
            {
                List<String> lignes = msg.Split('\n').ToList<string>();
                String lignemax = "";
                foreach (String ligne in lignes)
                {
                    if (ligne.Length > lignemax.Length)
                        lignemax = ligne;
                }
                // Maintenant on calcule la largeur max de cette ligne
                try
                {
                    Size s = TextRenderer.MeasureText(lignemax, this.Font);
                    // On va limiter à _iMaxWidth x _iMaxHeight pour la fenêtre TextBox
                    s = new Size(Math.Min(_iMaxWidth, 50 + s.Width), Math.Min(_iMaxHeight, 50 + s.Height * lignes.Count()));
                    Size sold = textBox1.Size;
                    int deltaw = Math.Max(0,s.Width - sold.Width);
                    int deltah = Math.Max(0,s.Height - sold.Height);
                    this.Size = new Size(this.Size.Width + deltaw, this.Size.Height + deltah);
                }
                catch (Exception)
                {
                }
            }


            // Extra checkbox ?
            if ((_extraCheckBox != null) && (_extraCheckBox.eType == ParameterObject.ParameterType.Bool) && (String.IsNullOrEmpty(_extraCheckBox.DisplayName) == false))
            {
            	int iMarge = 2;
            	int iHeight = 20;
            	
            	this.Size = new Size(this.Size.Width, this.Size.Height + iHeight + 2 * iMarge);
            	
				CheckBox checkBox1 = new CheckBox();
                checkBox1.AutoSize = true;
                checkBox1.Location = new System.Drawing.Point(textBox1.Location.X, textBox1.Location.Y + textBox1.Size.Height + iMarge);
                checkBox1.Name = "checkBox";
                checkBox1.Size = new System.Drawing.Size(80, iHeight);
                checkBox1.Text = _extraCheckBox.DisplayName;
                checkBox1.UseVisualStyleBackColor = true;
                checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
                if (_extraCheckBox.Value == "True")
                    checkBox1.Checked = true;
                this.Controls.Add(checkBox1);
                _extraCheckBox.Ctrl = checkBox1;
                
            }
            
            
            CenterButtons();
        }

        private void CenterButtons()
        {
            // Center buttons
            if (_bYesNo)
            {
                int widthbtns = button2.Location.X + button2.Width - button1.Location.X;
                int space = button2.Location.X - (button1.Location.X + button1.Width);
                int left = (this.Width - widthbtns) / 2;
                button1.Location = new Point(left, button1.Location.Y);
                button2.Location = new Point(button1.Location.X + button1.Width + space, button2.Location.Y);
            }
            else
            {
                button2.Enabled = false;
                button2.Visible = false;
                int left = (this.Width - button1.Width) / 2;
                button1.Location = new Point(left, button1.Location.Y);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_bYesNo)
                this.DialogResult = DialogResult.Yes;
            else
                this.DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_bYesNo)
                this.DialogResult = DialogResult.No;
            else
                this.DialogResult = DialogResult.Cancel;
        }

        private void MyMessageBox_SizeChanged(object sender, EventArgs e)
        {
            CenterButtons();
        }

        /// <summary>
        /// Handler to process key pressed
        /// Deals with CTRL+A to select all text
        /// </summary>
        /// <param name="msg">message</param>
        /// <param name="keyData">key data</param>
        /// <returns>true</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.A))
            {
                textBox1.SelectAll();
                textBox1.Focus();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
