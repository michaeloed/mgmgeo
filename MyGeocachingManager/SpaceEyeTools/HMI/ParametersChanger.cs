using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;
using System.IO;

namespace SpaceEyeTools.HMI
{
    /// <summary>
    /// Custom form to display as many input controls with associated labels for various value types
    /// </summary>
    public partial class ParametersChanger : Form
    {
        private bool bControlsCreated = false;
        private System.Windows.Forms.ToolTip _aToolTip = new System.Windows.Forms.ToolTip();
        private bool _bError = false;
        private String _sOpenfileTitle = "Choisir un fichier";
        private String _sOpenfileBtn = "Choisir";
        private String _sErrFormat = "";
        private String _sErrTitle = "";
        List<ParameterObject> _lParameters = null;
        private Control _ctrlCallbackCoordinates = null;
		private Control _ctrlCallbackRadius = null;
		
        Action<double, double> _handlerDisplayCoord = null;
        Image _displayCoordImage = null;

        /// <summary>
        /// 
        /// </summary>
        public String OpenfileTitle
        {
            set
            {
                this._sOpenfileTitle = value;
            }
        }

         /// <summary>
        /// 
        /// </summary>
        public String OpenfileBtn
        {
            set
            {
                this._sOpenfileBtn = value;
            }
        }
        
        /// <summary>
        /// Get the first control that handle coordinates (if applicable)
        /// Can be used to be automatically modified during a callback
        /// </summary>
        public Control CtrlCallbackCoordinates
        {
            get
            {
                return this._ctrlCallbackCoordinates;
            }
        }

        /// <summary>
        /// Get the first control that handle radius (if applicable)
        /// Can be used to be automatically modified during a callback
        /// </summary>
        public Control CtrlCallbackRadius
        {
            get
            {
                return this._ctrlCallbackRadius;
            }
        }
        
        /// <summary>
        /// Set handler to display coordinates (lat, lon)
        /// Also set an icon
        /// </summary>
        public Action<double, double> HandlerDisplayCoord
        {
            set
            {
                this._handlerDisplayCoord = value;
            }
        }

        /// <summary>
        /// Set an icon for button to display coordinates
        /// Also set an handler to display coordinates (lat, lon)
        /// </summary>
        public Image DisplayCoordImage
        {
            set
            {
                this._displayCoordImage = value;
            }
        }

        /// <summary>
        /// Get / Set list of parameters to modify
        /// </summary>
        public List<ParameterObject> Parameters
        {
            set
            {
                this._lParameters = value;
            }
            get
            {
                return this._lParameters;
            }
        }

        /// <summary>
        /// Set form title
        /// </summary>
        public string Title
        {
            set
            {
                this.Text = value;
            }
        }

        /// <summary>
        /// Set message string format for error display
        /// </summary>
        public string ErrorFormater
        {
            set
            {
                this._sErrFormat = value;
            }
        }

        /// <summary>
        /// Set error title label
        /// </summary>
        public string ErrorTitle
        {
            set
            {
                this._sErrTitle = value;
            }
        }

        /// <summary>
        /// Set ok button label
        /// </summary>
        public string BtnOK
        {
            set
            {
                btnOk.Text = value;
            }
        }

        /// <summary>
        /// Set cancel button label
        /// </summary>
        public string BtnCancel
        {
            set
            {
                btnCancel.Text = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ParametersChanger()
        {
            InitializeComponent();

            // Set up the delays for the ToolTip.
            _aToolTip.AutoPopDelay = 5000;
            _aToolTip.InitialDelay = 1000;
            _aToolTip.ReshowDelay = 500;
            // Force the ToolTip text to be displayed whether or not the form is active.
            _aToolTip.ShowAlways = true;
        }

        private void openfile_Click(object sender, EventArgs e)
        {
            Button ctrl = sender as Button;
            TextBox textBox1 = ctrl.Tag as TextBox;
        	OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "SQL (*.db)|*.db";
            openFileDialog1.Multiselect = false;
            openFileDialog1.Title = _sOpenfileTitle;
            
            String f = textBox1.Text;
            if (File.Exists(f))
            {
            	openFileDialog1.InitialDirectory = Path.GetDirectoryName(f);
            }

            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileNames[0];
            }
        }
        
        
        private void color_Click(object sender, EventArgs e)
        {
            Button ctrl = sender as Button;
            ColorDialog cd = new ColorDialog();
            cd.Color = ctrl.BackColor;
            if (cd.ShowDialog() == DialogResult.OK)
            {
                ctrl.BackColor = cd.Color;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            _bError = false;
            String err = "";

            foreach (ParameterObject p in _lParameters)
            {
                if (!p.UpdateValueFromControl())
                {
                    if (_sErrFormat == "")
                    {
                        err += p.ToString() + " => " + p.GetControlValueString() + " !";
                    }
                    else
                    {
                        String sinput = p.GetControlValueString();

                        // Les valeurs interdites si applicable
                        String sforbideninfo = "";
                        if ((p.eType == ParameterObject.ParameterType.Color) ||
                            (p.eType == ParameterObject.ParameterType.Password) ||
                            (p.eType == ParameterObject.ParameterType.TextBox) ||
                            (p.eType == ParameterObject.ParameterType.String))
                        {
                            if (p.ListForbidenValues != null)
                            {
                                foreach (Object o in p.ListForbidenValues)
                                {
                                    sforbideninfo += o.ToString() + "\r\n";
                                }
                                if (sforbideninfo != "")
                                    sforbideninfo = "\r\n" + sforbideninfo;
                            }
                        }
                        err += String.Format(_sErrFormat.Replace("#","\r\n"), p.InternalKey + " (" + p.DisplayName + ")" , p.Type, p.GetControlValueString(), sforbideninfo) + "\r\n";
                    }
                }
            }


            if (err != "")
            {
                String t = _sErrTitle;
                if (t == "")
                    t = "Error";

                MessageBox.Show(err,t,MessageBoxButtons.OK,MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                _bError = true;
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                _bError = false;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _bError = false;
            this.DialogResult = DialogResult.Cancel;
        }

        private void ParametersChanger_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_bError)
                e.Cancel = true;
        }

        private void ParametersChanger_Load(object sender, EventArgs e)
        {
            CreateControls();
        }

        /// <summary>
        /// Convert coordinnates from any supported format to three formats
        /// </summary>
        /// <param name="coords">Coordinates to convert</param>
        /// <returns>Converted coordinates (with #ERR if error)</returns>
        static public String ConvertCoordinates(String coords)
        {
            String info = "";

            try
            {
                String sLat = "";
                String sLon = "";
                String sLat2 = CoordConvHMI._sErrorValue;
                String sLon2 = CoordConvHMI._sErrorValue;
                double dLat = Double.MaxValue;
                double dLon = Double.MaxValue;

                bool bOK = ParameterObject.TryToConvertCoordinates(coords, ref sLat, ref sLon);
                if (sLat != CoordConvHMI._sErrorValue)
                    dLat = MyTools.ConvertToDouble(sLat);
                if (sLon != CoordConvHMI._sErrorValue)
                    dLon = MyTools.ConvertToDouble(sLon);
                info += /*"DD.DDDDDD: " + */sLat + " " + sLon + "\r\n";

                sLat2 = CoordConvHMI.ConvertDegreesToDDMM(dLat, true);
                sLon2 = CoordConvHMI.ConvertDegreesToDDMM(dLon, false);
                info += /*"DD° MM.MMM: " + */sLat2 + " " + sLon2 + "\r\n";

                sLat2 = CoordConvHMI.ConvertDegreesToDDMMSSTT(dLat, true);
                sLon2 = CoordConvHMI.ConvertDegreesToDDMMSSTT(dLon, false);
                info += /*"DD° MM' SS.SSS: " + */sLat2 + " " + sLon2;
            }
            catch (Exception)
            {
            }

            return info;
        }

        private void txtCoord_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                // Ok le sender est valide
                if (tb.Tag != null)
                {
                    TextBox tbconv = tb.Tag as TextBox;
                    if (tbconv != null)
                    {
                        tbconv.Text = ConvertCoordinates(tb.Text);
                    }
                }
            }
        }

        private void btnDisplayCoordClick(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if ((btn != null) && (btn.Tag != null))
            {
                TextBox tb = btn.Tag as TextBox;
                if (tb == null)
                {
                    MessageBox.Show(_sErrTitle, _sErrTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (_handlerDisplayCoord != null)
                    {
                        Double dLat = Double.MaxValue;
                        Double dLon = Double.MaxValue;
                        String sLat = "";
                        String sLon = "";
                        bool bOK = ParameterObject.TryToConvertCoordinates(tb.Text, ref sLat, ref sLon);
                        if (sLat != CoordConvHMI._sErrorValue)
                            dLat = MyTools.ConvertToDouble(sLat);
                        if (sLon != CoordConvHMI._sErrorValue)
                            dLon = MyTools.ConvertToDouble(sLon);
                        if (bOK)
                        	_handlerDisplayCoord(dLat, dLon);
                        else
                            MessageBox.Show(_sErrTitle, _sErrTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show(_sErrTitle, _sErrTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBox1_DropDownClosed(object sender, EventArgs e)
		{
        	if (_aToolTip != null)
        	{
        		ComboBox cb = sender as ComboBox;
        		if(cb != null)
		    		_aToolTip.Hide(cb);
        	}
		}
		
		private void comboBox1_DrawItem(object sender, DrawItemEventArgs e)
		{
			ComboBox cb = sender as ComboBox;
    		if(cb != null)
    		{
    			if (e.Index < 0) 
    			{ 
    				return; 
    			} // added this line thanks to Andrew's comment
			    
    			string text = cb.GetItemText(cb.Items[e.Index]);
			    e.DrawBackground();
			    using (SolidBrush br = new SolidBrush(e.ForeColor))
			    { 
			    	e.Graphics.DrawString(text, e.Font, br, e.Bounds); 
			    }
			    if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
			    { 
			    	if (_aToolTip != null)
    				{
			    		_aToolTip.Show(text, cb, e.Bounds.Right, e.Bounds.Bottom);
			    	}
			    }
			    e.DrawFocusRectangle();
    		}
		}
		
        /// <summary>
        /// Force controls creation before OnLoad
        /// </summary>
        public void CreateControls()
        {
            if (bControlsCreated)
                return;
            else
                bControlsCreated = true;

            //this.SuspendLayout();
            int i = 12;
            int iHeight = 20;
            int iWidth = 150;
            int iMarge = 5;
            int iMaxWidth = 250;

            // Compute max label width
            Graphics g = this.CreateGraphics();
            foreach (ParameterObject p in _lParameters)
            {
                switch (p.eType)
                {
                    case ParameterObject.ParameterType.Bool:
                        {
                            // Nothing to do !
                            break;
                        }
                    default:
                        {
                            // Compute text length
                            SizeF sz = g.MeasureString(p.DisplayName, this.Font);
                            int w = (int)sz.Width;
                            if (w > iWidth)
                                iWidth = w;
                            break;
                        }
                }
            }

            // Resize window if necessary
            if (iWidth != 150)
            {
                this.Size = new Size(this.Width + iWidth - 150, this.Height);
            }
            foreach (ParameterObject p in _lParameters)
            {
                switch (p.eType)
                {
                    case ParameterObject.ParameterType.Bool:
                        {
                            // Create checbox
                            
                            CheckBox checkBox1 = new CheckBox();
                            checkBox1.AutoSize = true;
                            checkBox1.Location = new System.Drawing.Point(12, i);
                            checkBox1.Name = "checkBox" + i.ToString() ;
                            checkBox1.Size = new System.Drawing.Size(80, iHeight);
                            checkBox1.Text = p.DisplayName;
                            checkBox1.UseVisualStyleBackColor = true;
                            if (p.Value == "True")
                                checkBox1.Checked = true;
                            checkBox1.Enabled = !p.Disabled;
                            this.Controls.Add(checkBox1);
                            p.Ctrl = checkBox1;
                            if ((p.TooltipText != null) && (p.TooltipText != ""))
                                _aToolTip.SetToolTip(checkBox1, p.TooltipText);

                            i += iHeight + iMarge;
                            break;
                        }
                    case ParameterObject.ParameterType.Date:
                        {
                			Label label1 = new Label();
                            label1.AutoSize = true;
                            label1.Location = new System.Drawing.Point(12, i);
                            label1.Name = "label" + i.ToString();
                            label1.Size = new System.Drawing.Size(iWidth, iHeight);
                            label1.Text = p.DisplayName;
                            this.Controls.Add(label1);

                            int x = label1.Left + iWidth + 10;
                            
                            // Create DateTimePicker
                            
                            DateTimePicker dtPickerloghmi = new DateTimePicker();
                            dtPickerloghmi.Location = new System.Drawing.Point(x, i);
				            dtPickerloghmi.Name = "datepicker" + i.ToString() ;
				            dtPickerloghmi.Size = new System.Drawing.Size(177, iHeight);
				            dtPickerloghmi.Value = (DateTime)(p.ValueO);
                            this.Controls.Add(dtPickerloghmi);
                            dtPickerloghmi.Enabled = !p.Disabled;
                            p.Ctrl = dtPickerloghmi;
                            if ((p.TooltipText != null) && (p.TooltipText != ""))
                                _aToolTip.SetToolTip(dtPickerloghmi, p.TooltipText);

                            i += iHeight + iMarge;
                            break;
                        }
                	case ParameterObject.ParameterType.OpenFile:
                        {
                            Label label1 = new Label();
                            label1.AutoSize = true;
                            label1.Location = new System.Drawing.Point(12, i);
                            label1.Name = "label" + i.ToString();
                            label1.Size = new System.Drawing.Size(iWidth, iHeight);
                            label1.Text = p.DisplayName;
                            this.Controls.Add(label1);
                            
                            int x = label1.Left + iWidth + 10;
                            TextBox textBox1 = new TextBox();
                            textBox1.Location = new System.Drawing.Point(x, i);
                            textBox1.Name = "textBox1" + i.ToString();
                            textBox1.Size = new System.Drawing.Size(200, iHeight);
                            textBox1.Text = p.Value;
                            textBox1.ReadOnly = true;
                            textBox1.Enabled = !p.Disabled;
                            this.Controls.Add(textBox1);
                            p.Ctrl = textBox1;
                            if ((p.TooltipText != null) && (p.TooltipText != ""))
                                _aToolTip.SetToolTip(textBox1, p.TooltipText);

                            i += textBox1.Size.Height + iMarge;
                            
                            // Create button
                            Button button1 = new Button();
                            button1.Location = new System.Drawing.Point(x, i);
                            button1.Name = "button" + i.ToString();
                            button1.Size = new System.Drawing.Size(80, iHeight);
                            button1.Text = _sOpenfileBtn;
                            button1.Click += new System.EventHandler(this.openfile_Click);
							button1.Enabled = !p.Disabled;
							button1.Tag = textBox1;
                            if ((p.TooltipText != null) && (p.TooltipText != ""))
                                _aToolTip.SetToolTip(button1, p.TooltipText);
                            this.Controls.Add(button1);

                            i += iHeight + iMarge;
                            
                            break;
                        }
                    case ParameterObject.ParameterType.Color:
                        {
                            Label label1 = new Label();
                            label1.AutoSize = true;
                            label1.Location = new System.Drawing.Point(12, i);
                            label1.Name = "label" + i.ToString();
                            label1.Size = new System.Drawing.Size(iWidth, iHeight);
                            label1.Text = p.DisplayName;
                            this.Controls.Add(label1);

                            int x = label1.Left + iWidth + 10;

                            // Create button
                            Button button1 = new Button();
                            button1.Location = new System.Drawing.Point(x, i);
                            button1.Name = "button" + i.ToString();
                            button1.Size = new System.Drawing.Size(iHeight, iHeight);
                            button1.Text = "";
                            button1.UseVisualStyleBackColor = true;
                            button1.BackColor = (Color)(p.ValueO);
                            button1.Click += new System.EventHandler(this.color_Click);
							button1.Enabled = !p.Disabled;
							
                            if ((p.TooltipText != null) && (p.TooltipText != ""))
                                _aToolTip.SetToolTip(button1, p.TooltipText);

                            this.Controls.Add(button1);
                            p.Ctrl = button1;

                            i += iHeight + iMarge;
                            break;
                        }
                    case ParameterObject.ParameterType.List:
                        {
                            Label label1 = new Label();
                            label1.AutoSize = true;
                            label1.Location = new System.Drawing.Point(12, i);
                            label1.Name = "label" + i.ToString();
                            label1.Size = new System.Drawing.Size(iWidth, iHeight);
                            label1.Text = p.DisplayName;
                            this.Controls.Add(label1);

                            int x = label1.Left + iWidth + 10;
                            // Create combobox
                            ComboBox comboBox1 = new ComboBox();
                            comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                            comboBox1.FormattingEnabled = true;
                            comboBox1.Location = new System.Drawing.Point(x, i);
                            comboBox1.Name = "comboBox" + i.ToString(); 
                            comboBox1.Size = new System.Drawing.Size(200, iHeight);
                            
                            //comboBox1.DrawMode = DrawMode.OwnerDrawFixed;
    						//comboBox1.DrawItem += comboBox1_DrawItem;
    						//comboBox1.DropDownClosed += comboBox1_DropDownClosed;
                            
                            List<String> lst = p.ValueO as List<String>;
                            foreach (object o in lst)
                            {
                                comboBox1.Items.Add(o);
                            }
                            // On va sélectionner par défaut 
                            if (p.DefaultListValue == null)
                            {
                                comboBox1.SelectedIndex = 0;
                            }
                            else
                            {
                                // Si la valeur existe, on la sélectionne
                                int index = comboBox1.Items.IndexOf(p.DefaultListValue);
                                if (index != -1)
                                    comboBox1.SelectedIndex = index;
                                else
                                    comboBox1.SelectedIndex = 0;
                            }
                            comboBox1.Enabled = !p.Disabled;
                            this.Controls.Add(comboBox1);
                            p.Ctrl = comboBox1;
                            if ((p.TooltipText != null) && (p.TooltipText != ""))
                                _aToolTip.SetToolTip(comboBox1, p.TooltipText);

                            i += iHeight + iMarge;
                            break;
                        }
                    case ParameterObject.ParameterType.CheckList:
                        {
                			int iHeightSpecial = 200;
                            Label label1 = new Label();
                            label1.AutoSize = true;
                            label1.Location = new System.Drawing.Point(12, i);
                            label1.Name = "label" + i.ToString();
                            label1.Size = new System.Drawing.Size(iWidth, iHeightSpecial);
                            label1.Text = p.DisplayName;
                            this.Controls.Add(label1);

                            int x = label1.Left + iWidth + 10;
                            // Create combobox
                            ListView listView1 = new ListView();
                            listView1.CheckBoxes = true;
				        	listView1.FullRowSelect = true;
				        	listView1.GridLines = true;
				        	listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Clickable;
				        	listView1.Location = new System.Drawing.Point(x, i);
				        	listView1.MultiSelect = false;
				        	listView1.Name = "listView" + i.ToString(); 
				        	listView1.ShowItemToolTips = true;
				        	listView1.Size = new System.Drawing.Size(300, iHeightSpecial);
				        	listView1.UseCompatibleStateImageBehavior = false;
				        	listView1.View = System.Windows.Forms.View.Details;
				        	listView1.Columns.Add("",280);
				        	if (p.ImagesForCheckList != null)
				        		listView1.SmallImageList = p.ImagesForCheckList;
				        	iMaxWidth = Math.Max(iMaxWidth, listView1.Size.Width + 50);
            				
                            List<String> lst = p.ValueO as List<String>;
                            foreach (String o in lst)
                            {
                                ListViewItem lvi = listView1.Items.Add(o);
                                lvi.ToolTipText = o;
                                lvi.ImageKey = o;
                                // On va sélectionner par défaut 
                                if ((p.ListCheckedValue != null) && (p.ListCheckedValue.Contains(o)))
                                	lvi.Checked = true;
                            }
                            listView1.Enabled = !p.Disabled;
                            this.Controls.Add(listView1);
                            p.Ctrl = listView1;
                            if ((p.TooltipText != null) && (p.TooltipText != ""))
                                _aToolTip.SetToolTip(listView1, p.TooltipText);

                            i += iHeightSpecial + iMarge;
                            break;
                        }
                    case ParameterObject.ParameterType.Coordinates:
                        {
                            Label label1 = new Label();
                            label1.AutoSize = true;
                            label1.Location = new System.Drawing.Point(12, i);
                            label1.Name = "label" + i.ToString();
                            label1.Size = new System.Drawing.Size(iWidth, iHeight);
                            label1.Text = p.DisplayName;
                            this.Controls.Add(label1);

                            int x = label1.Left + iWidth + 10;
                            TextBox textBox1 = new TextBox();
                            textBox1.Location = new System.Drawing.Point(x, i);
                            textBox1.Name = "textBox1Coord" + i.ToString();
                            textBox1.Size = new System.Drawing.Size(200, iHeight);
                            textBox1.Text = p.Value;
                            this.Controls.Add(textBox1);
                            p.Ctrl = textBox1;
                            i += iHeight + iMarge;
                            if ((p.TooltipText != null) && (p.TooltipText != ""))
                                _aToolTip.SetToolTip(textBox1, p.TooltipText);

                            // Le control pour le callback
                            if (_ctrlCallbackCoordinates == null)
                                _ctrlCallbackCoordinates = textBox1;

                            // Handler pour màj des conversion auto
                            textBox1.TextChanged += new System.EventHandler(this.txtCoord_TextChanged);

                            // Et on ajoute un TextBox pour afficher les conversions diverses
                            TextBox textBox2 = new TextBox();
                            textBox2.Location = new System.Drawing.Point(x, i);
                            textBox2.Name = "textBox2CoordConv" + i.ToString();
                            textBox2.Multiline = true; 
                            textBox2.Size = new System.Drawing.Size(200, 45); // Ou 250
                            textBox2.Text = ConvertCoordinates(textBox1.Text); ;
                            textBox2.ReadOnly = true;
                            textBox2.Enabled = !p.Disabled;
                            this.Controls.Add(textBox2);

                            // On affiche le bouton pour le handler d'affichage des coordonnées
                            if (_handlerDisplayCoord != null)
                            {
                                // On crée un bouton
                                Button btn = new System.Windows.Forms.Button();
                                btn.Location = new System.Drawing.Point(x + 200 + iMarge, i - iHeight - iMarge);
                                btn.Margin = new System.Windows.Forms.Padding(0);
                                btn.Name = "btnDisplayCoord" + i.ToString();
                                btn.Text = "";
                                btn.Size = new System.Drawing.Size(iHeight, iHeight);
                                if (_displayCoordImage != null)
                                    btn.Image = _displayCoordImage;
                                else
                                    btn.Text = "G";
                                btn.UseVisualStyleBackColor = true;
                                btn.Click += new System.EventHandler(this.btnDisplayCoordClick);
                                btn.Tag = textBox1;
                                btn.Enabled = !p.Disabled;
                                this.Controls.Add(btn);
                                if ((p.TooltipText != null) && (p.TooltipText != ""))
                                    _aToolTip.SetToolTip(btn, p.TooltipText);

                                // On aggrandit la textbox
                                textBox2.Size = new System.Drawing.Size(textBox2.Size.Width + iMarge + iHeight, 45);
                            }

                            // On indique au textBox1 où afficher sa conversion
                            textBox1.Tag = textBox2;
                            iMaxWidth = Math.Max(iMaxWidth, textBox2.Size.Width + 50);
                            i += textBox2.Size.Height + iMarge;

                            break;
                        }
                	case ParameterObject.ParameterType.Radius:
                        {
                            Label label1 = new Label();
                            label1.AutoSize = true;
                            label1.Location = new System.Drawing.Point(12, i);
                            label1.Name = "label" + i.ToString();
                            label1.Size = new System.Drawing.Size(iWidth, iHeight);
                            label1.Text = p.DisplayName;
                            this.Controls.Add(label1);
                            
                            int x = label1.Left + iWidth + 10;
                            TextBox textBox1 = new TextBox();
                            textBox1.Location = new System.Drawing.Point(x, i);
                            textBox1.Name = "textBox1" + i.ToString();
                            textBox1.Size = new System.Drawing.Size(200, iHeight);
                            textBox1.Text = p.Value;
							textBox1.Enabled = !p.Disabled;
                            if (_ctrlCallbackRadius == null)
                            	_ctrlCallbackRadius = textBox1;
                            
                            this.Controls.Add(textBox1);
                            p.Ctrl = textBox1;
                            if ((p.TooltipText != null) && (p.TooltipText != ""))
                                _aToolTip.SetToolTip(textBox1, p.TooltipText);

                            i += textBox1.Size.Height + iMarge;
                            break;
                        }
                    case ParameterObject.ParameterType.Double:
                    case ParameterObject.ParameterType.Int:
                    case ParameterObject.ParameterType.Password:
                    case ParameterObject.ParameterType.TextBox:
                    case ParameterObject.ParameterType.String:
                        {
                            Label label1 = new Label();
                            label1.AutoSize = true;
                            label1.Location = new System.Drawing.Point(12, i);
                            label1.Name = "label" + i.ToString();
                            label1.Size = new System.Drawing.Size(iWidth, iHeight);
                            label1.Text = p.DisplayName;
                            this.Controls.Add(label1);
                            
                            int x = label1.Left + iWidth + 10;
                            int txtwidth = 200;
                            if (p.DisplayName == "")
                            {
                            	// On affiche à gauche, pas de label
                            	x = 12;
                            	txtwidth += iWidth + 10;
                            }
                            TextBox textBox1 = new TextBox();
                            textBox1.Location = new System.Drawing.Point(x, i);
                            textBox1.Name = "textBox1" + i.ToString();
                            textBox1.Size = new System.Drawing.Size(txtwidth, iHeight);
                            textBox1.Text = p.Value;
                            textBox1.ReadOnly = p.ReadOnly;

                            // Cas spécifique du password
                            if (p.eType == ParameterObject.ParameterType.Password)
                            {
                                textBox1.UseSystemPasswordChar = true;
                            }
                            else if (p.eType == ParameterObject.ParameterType.TextBox)
                            {
                                // Cas spécifique de la textbox multiline
                                textBox1.Multiline = true;
                                textBox1.ScrollBars = ScrollBars.Both;
                                textBox1.Size = new System.Drawing.Size(txtwidth, 3 * iHeight);
                                btnOk.TabStop = false;
                                btnCancel.TabStop = false;
                                this.AcceptButton = null;
                                this.CancelButton = null;
                            }
                            textBox1.Enabled = !p.Disabled;
                            this.Controls.Add(textBox1);
                            p.Ctrl = textBox1;
                            if ((p.TooltipText != null) && (p.TooltipText != ""))
                                _aToolTip.SetToolTip(textBox1, p.TooltipText);

                            i += textBox1.Size.Height + iMarge;
                            break;
                        }
                    case ParameterObject.ParameterType.Label:
                        {
                            Label label1 = new Label();
                            label1.AutoSize = true;
                            label1.Location = new System.Drawing.Point(12, i);
                            label1.Name = "label" + i.ToString();
                            label1.Size = new System.Drawing.Size(iWidth, iHeight);
                            label1.Text = p.DisplayName;
                            this.Controls.Add(label1);

                            int x = label1.Left + iWidth + 10;
                            TextBox textBox1 = new TextBox();
                            textBox1.Location = new System.Drawing.Point(x, i);
                            textBox1.Name = "textBox1" + i.ToString();
                            textBox1.Size = new System.Drawing.Size(200, iHeight);
                            textBox1.Text = "";// p.Value; // force value to nothing
                            textBox1.Visible = false;
                            this.Controls.Add(textBox1);
                            p.Ctrl = textBox1;
                            if ((p.TooltipText != null) && (p.TooltipText != ""))
                                _aToolTip.SetToolTip(textBox1, p.TooltipText);
                            
                            i += iHeight + iMarge;
                            break;
                        }
                }
            }

            int iTotalWidth = 12 + iWidth + 10 + iMaxWidth + 12;
            this.Size = new Size(iTotalWidth, i + 4 * 12 + 50);
            //this.ResumeLayout(false);
            //this.PerformLayout();
        }
    }

    /// <summary>
    /// Parameter class used with ParametersChanger
    /// </summary>
    public class ParameterObject
    {
        /// <summary>
        /// Enumeration defining which type of parameter shall be created
        /// </summary>
        public enum ParameterType { 
            /// <summary>
            /// Default type, shall not be used except for initialisation
            /// </summary>
            None = 1, 
            /// <summary>
            /// Parameter is a String
            /// </summary>
            String,
            /// <summary>
            /// Parameter is a multiline String
            /// </summary>
            TextBox,
            /// <summary>
            /// Parameter is a String but password style (only dots are visible)
            /// </summary>
            Password,
            /// <summary>
            /// Parameter is a Boolean
            /// </summary>
            Bool, 
            /// <summary>
            /// Parameter is an Interger 32 bits
            /// </summary>
            Int, 
            /// <summary>
            /// Parameter is a Double 32 bits
            /// </summary>
            Double,
            /// <summary>
            /// Parameter is a coordinate un decimal degrees, DD MM.MMM or DD MM SSS
            /// First logitude, then latitude
            /// </summary>
            Coordinates,
            /// <summary>
            /// Parameter is a List
            /// </summary>
            List, 
            /// <summary>
            /// Parameter is a text Label
            /// </summary>
            Label,
            /// <summary>
            /// Parameter is a color dialog picker
            /// </summary>
            Color,
            /// <summary>
            /// Parameter is a date picker
            /// </summary>
            Date,
            /// <summary>
            /// Parameter is a multi check item list
            /// </summary>
            CheckList,
            /// <summary>
            /// Parameter is a positive integer for radius (used with Coordinates)
            /// </summary>
            Radius,
            /// <summary>
            /// Parameter is an existing file
            /// </summary>
            OpenFile
        };
        private ParameterType _Type = ParameterType.None;
        private bool _Disabled = false;
        private bool _ReadOnly = false;
        private object _Value = null;
        private String _sTooltipText = "";
        private String _sInternalKey = "";
        private String _sDisplayName = "";
        private Control _Control = null;
        private static CultureInfo cEN = new CultureInfo("en-GB");
        private String _sDefaultListValue = null;
        private List<String> _sListCheckedValue = null;
        private int _ValueIndex = -1; // Only for List parameter !
        private List<int> _ValueIndexes = null;
        private List<Object> _lListForbidenValues = new List<Object>();
        private ImageList _lImagesForCheckList = null;

        /// <summary>
        /// Get / Set list of images for a CheckList only
        /// </summary>
        public ImageList ImagesForCheckList
        {
            get
            {
                return _lImagesForCheckList;
            }
            set
            {
                _lImagesForCheckList = value;
            }
        }
        
        /// <summary>
        /// Set Readonly status
        /// </summary>
        public bool ReadOnly
        {
        	get
            {
                return _ReadOnly;
            }
            set
            {
            	_ReadOnly = value;
            }
        }
        
        /// <summary>
        /// Get disabled status
        /// </summary>
        public bool Disabled
        {
            get
            {
                return _Disabled;
            }
        }
        
        
        /// <summary>
        /// Get list of forbiden values if defined
        /// </summary>
        public List<Object> ListForbidenValues
        {
            get
            {
                return _lListForbidenValues;
            }
        }

        /// <summary>
        /// Set a specific control for this parameter
        /// Control shall ba aligned with parameter type
        /// </summary>
        public Control Ctrl
        {
            set
            {
                this._Control = value;
            }
        }


        /// <summary>
        /// Get parameter type (enum)
        /// </summary>
        public ParameterType eType
        {
            get
            {
                return _Type;
            }
        }

        /// <summary>
        /// Get parameter type (string)
        /// </summary>
        public string Type
        {
            get
            {
                switch (_Type)
                {
                    case ParameterType.Bool:
                        {
                            return "Bool";
                        }
                    case ParameterType.Double:
                        {
                            return "Double";
                        }
                    case ParameterType.Int:
                        {
                            return "Int";
                        }
                    case ParameterType.Password:
                        {
                            return "Password";
                        }
                    case ParameterType.TextBox:
                        {
                            return "TextBox";
                        }
                    case ParameterType.Coordinates:
                        {
                            return "Coordinates";
                        }
                    case ParameterType.Radius:
                        {
                            return "Radius";
                        }
                    case ParameterType.String:
                        {
                            return "String";
                        }
                    case ParameterType.List:
                        {
                            return "List";
                        }
                    case ParameterType.CheckList:
                        {
                            return "CheckList";
                        }
                    case ParameterType.Label:
                        {
                            return "Label";
                        }
                    case ParameterType.Color:
                        {
                            return "Color";
                        }
                    case ParameterType.OpenFile:
                        {
                            return "OpenFile";
                        }
                    case ParameterType.Date:
                        {
                            return "Date";
                        }
                    default:
                        {
                            return "Unknown";
                        }
                }
            }
        }

        /// <summary>
        /// Get name to display for this parameter
        /// </summary>
        public string DisplayName
        {
            get
            {
                return _sDisplayName;
            }
        }

        /// <summary>
        /// Get tooltiptext
        /// </summary>
        public string TooltipText
        {
            get
            {
                return _sTooltipText;
            }
        }

        /// <summary>
        /// Get / Set default list value for List parameter
        /// </summary>
        public string DefaultListValue
        {
            get
            {
                return _sDefaultListValue;
            }
            set
            {
                _sDefaultListValue = value;
            }
        }

        /// <summary>
        /// Get / Set default checked values for CheckList parameter
        /// </summary>
        public List<String> ListCheckedValue
        {
            get
            {
                return _sListCheckedValue;
            }
            set
            {
                _sListCheckedValue = value;
            }
        }

        
        
        /// <summary>
        /// Get internal key for this parameter
        /// Not really used
        /// </summary>
        public string InternalKey
        {
            get
            {
                return _sInternalKey;
            }
        }

        /// <summary>
        /// Get parameter value as a string
        /// </summary>
        public string Value
        {
            get
            {
                if (_Value == null)
                    return "";
                else
                    return _Value.ToString();
            }
        }

        /// <summary>
        /// Get index of selected value
        /// Only representative for ParameterType.List
        /// </summary>
        public int ValueIndex
        {
            get
            {
                return _ValueIndex;
            }
        }

        /// <summary>
        /// Get index of selected valuees
        /// Only representative for ParameterType.CheckList
        /// </summary>
        public List<int> ValueIndexes
        {
        	get
        	{
        		return _ValueIndexes;
        	}
        }
        
        /// <summary>
        /// Get parameter value as an object
        /// </summary>
        public object ValueO
        {
            get
            {
                return _Value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">parameter type</param>
        /// <param name="value">parameter initial value</param>
        /// <param name="sKey">parameter key</param>
        /// <param name="sName">parameter display name</param>
        /// <param name="disabled">if true parameter is disabled</param>
        public ParameterObject(ParameterType type, object value, String sKey, String sName, bool disabled = false)
        {
            _Type = type;
            _Value = value;
            _sInternalKey = sKey;
            _sDisplayName = sName;
            _sDefaultListValue = null;
            _sListCheckedValue = null;
            _sTooltipText = null;
            _Disabled = disabled;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">parameter type</param>
        /// <param name="value">parameter initial value</param>
        /// <param name="sKey">parameter key</param>
        /// <param name="sName">parameter display name</param>
        /// <param name="sTooltipText">tooltip display value (can be null)</param>
        public ParameterObject(ParameterType type, object value, String sKey, String sName, String sTooltipText)
            : this(type, value, sKey, sName)
        {
            _sTooltipText = sTooltipText;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">parameter type</param>
        /// <param name="value">parameter initial value</param>
        /// <param name="sKey">parameter key</param>
        /// <param name="sName">parameter display name</param>
        /// <param name="sTooltipText">tooltip display value (can be null)</param>
        /// <param name="lListForbidenValues">list of forbiden values, only valy for these types: Color, Password, String</param>
        public ParameterObject(ParameterType type, object value, String sKey, String sName, String sTooltipText, List<Object> lListForbidenValues)
            : this(type, value, sKey, sName, sTooltipText)
        {
            if (lListForbidenValues != null)
                _lListForbidenValues = lListForbidenValues;
            else
                _lListForbidenValues = new List<Object>();
        }

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>string value</returns>
        public override string ToString()
        {
            String s = "";
            s = _sInternalKey + " (" + _sDisplayName + ") [" + _Type + "] = ";
            if (_Value == null)
                s += "(null)";
            else
                s += _Value.ToString();

            return s;
        }

        /// <summary>
        /// Get parameter value from control
        /// </summary>
        /// <returns>Parameter value as a string</returns>
        public String GetControlValueString()
        {
            if (_Control != null)
            {
                switch (_Type)
                {
                    case ParameterType.Bool:
                        {
                            if (_Control is CheckBox)
                            {
                                CheckBox ctrl = _Control as CheckBox;
                                return ctrl.Checked.ToString();
                            }
                            break;
                        }
                    case ParameterType.List:
                        {
                            if (_Control is ComboBox)
                            {
                                ComboBox ctrl = _Control as ComboBox;
                                int i = ctrl.SelectedIndex;
                                _ValueIndex = i;
                                return ctrl.Items[i].ToString();
                            }
                            break;
                        }
                    case ParameterType.CheckList:
                        {
                            if (_Control is ListView)
                            {
                                ListView ctrl = _Control as ListView;
                                _ValueIndexes = new List<int>();
                                String vals = "";
                                foreach(ListViewItem o in ctrl.CheckedItems)
                                {
                                	vals += o.Text + "\n";
                                }
                                foreach(int i in ctrl.CheckedIndices)
								{
                                	_ValueIndexes.Add(i);
								}
                                return vals;
                            }
                            break;
                        }
                    case ParameterType.Color:
                        {
                            if (_Control is Button)
                            {
                                Button ctrl = _Control as Button;
                                return ctrl.BackColor.ToString();
                            }
                            break;
                        }
                    case ParameterType.Date:
                        {
                            if (_Control is DateTimePicker)
                            {
                                DateTimePicker ctrl = _Control as DateTimePicker;
                                return ctrl.Value.ToString("yyyy-MM-ddT00:00:01Z");
                            }
                            break;
                        }
                    case ParameterType.OpenFile:
                    case ParameterType.Double:
                    case ParameterType.Int:
                    case ParameterType.Password:
                    case ParameterType.TextBox:
                    case ParameterType.String:
                    case ParameterType.Coordinates:
                    case ParameterType.Radius:
                    case ParameterType.Label:
                        {
                            if (_Control is TextBox)
                            {
                                TextBox ctrl = _Control as TextBox;
                                return ctrl.Text;
                            }
                            break;
                        }
                }               
            }
            return "";
        }

        /// <summary>
        /// Text is latitude then longitude in decimal degrees
        /// </summary>
        /// <param name="text">Text is latitude then longitude in decimal degrees</param>
        /// <param name="dlon">longitude</param>
        /// <param name="dlat">latitude</param>
        /// <returns>true is split succeeded</returns>
        static public bool SplitLongitudeLatitude(String text, ref double dlon, ref double dlat)
        {
            dlon = Double.MaxValue;
            dlat = Double.MaxValue;
            String lat = "";
            String lon = "";
            try 
	        {
		        // Expect "longitude latitude"
                String lonlat = text;
                lonlat = text.TrimStart(' ');
                lonlat = text.TrimEnd(' ');
                if ((lonlat != "") && (lonlat.Contains(" ")))
                {
                    // On découpe
                    int pos = lonlat.IndexOf(' ');
                    if (pos <= 0)
                        return false;
                    lat = lonlat.Substring(0, pos);
                    lon = lonlat.Substring(pos + 1);
                    lon = lon.Trim();
                    lat = lat.Trim();
                    bool result = true;
                    if (CoordConvHMI.CheckLonLatValidity(lon, false))
                        dlon = MyTools.ConvertToDouble(lon);
                    else
                        result = false;

                    if (CoordConvHMI.CheckLonLatValidity(lat, true))
                        dlat = MyTools.ConvertToDouble(lat);
                    else
                        result = false;

                    return result;
                }
                else
                {
                    // On tente tout de même de convertir la première valeur, la latitude
                    dlat = MyTools.ConvertToDouble(lonlat);
                    return false;
                }
	        }
	        catch (Exception)
	        {
                return false;
	        }
        }

        /// <summary>
        /// Try to convert coordinates from any format to decimal degrees
        /// </summary>
        /// <param name="ctrltxt">latitude and longitude</param>
        /// <param name="sLat">valid latitude or #ERR</param>
        /// <param name="sLon">valid longitude or #ERR</param>
        /// <returns>True if both coordinates are valid</returns>
        static public bool TryToConvertCoordinates(String ctrltxt, ref String sLat, ref String sLon)
        {
            sLat = CoordConvHMI._sErrorValue;
            sLon = CoordConvHMI._sErrorValue;
            try
            {
                // On essaie de convertir les valeurs
                // ctrl.Text doit contenir la latitude puis la longitude 
                // - en degrés décimaux séparés par un espace
                // - en DD MM.MMM séparés par un espace
                // - en DD MM SSS séparés par un espace
                if (ctrltxt.Contains("N") ||
                    ctrltxt.Contains("S") ||
                    ctrltxt.Contains("E") ||
                    ctrltxt.Contains("W"))
                {
                    // on est en DD MM.MMM ou DD MM SSS
                    // DD° MM.MMM : N 48° 46.164 E 01° 58.048
                    // DD° MM' SS.SSS : N 48° 46' 9.9 E 01° 58' 2.9
                    
                    // Si on a des ' ou '' ou " alors on tente en DDMMSSS
                    if (ctrltxt.Contains("'") ||
                        ctrltxt.Contains("''") ||
                        ctrltxt.Contains("\""))
                    {
                        // Peut être en DD MM SSS ?
                        if (CoordConvHMI.DDMMSSStoDDD(ctrltxt, ref sLat, ref sLon))
                        {
                            // On a converti
                        }
                        else if (CoordConvHMI.DDMMMtoDDD(ctrltxt, ref sLat, ref sLon)) // au cas ou on ait mis des ' derrière les minutes décimales
                        {
                            // On a converti
                        }
                        else
                            return false;
                    }
                    else
                    {
                        // On va tenter de convertir en degrés décimaux
                        // Test en DDMMM
                        if (CoordConvHMI.DDMMMtoDDD(ctrltxt, ref sLat, ref sLon))
                        {
                            // On a converti
                        }
                        else
                            return false;
                    }
                }
                else
                {
                    // On est en degrés décimaux
                    // On isole la latitude et la longitude
                    double dlon = double.MaxValue;
                    double dlat = double.MaxValue;
                    if (ParameterObject.SplitLongitudeLatitude(ctrltxt.Replace(",", "."), ref dlon, ref dlat))
                    {
                        // C'est top, tout va bien
                        sLon = dlon.ToString().Replace(",", ".");
                        sLat = dlat.ToString().Replace(",", ".");
                    }
                    else
                    {
                        // On essaie tout de même de traduire ce qu'on peut
                        if (dlon != Double.MaxValue)
                            sLon = dlon.ToString().Replace(",", ".");
                        if (dlat != Double.MaxValue)
                            sLat = dlat.ToString().Replace(",", ".");
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Update value of parameter from the one in the control
        /// </summary>
        /// <returns>true is control value is valid</returns>
        public bool UpdateValueFromControl()
        {
            if (_Control == null)
                return false;
            else
            {
                switch (_Type)
                {
                    case ParameterType.Bool:
                        {
                            if (_Control is CheckBox)
                            {
                                CheckBox ctrl = _Control as CheckBox;
                                // Pas de vérification de valeur interdite ici sur un booléen
                                _Value = ctrl.Checked;         
                            }
                            else
                                return false;
                            break;
                        }
                    case ParameterType.Date:
                        {
                            if (_Control is DateTimePicker)
                            {
                                DateTimePicker ctrl = _Control as DateTimePicker;
                                // Pas de vérification de valeur interdite ici sur un booléen
                                _Value = ctrl.Value;         
                            }
                            else
                                return false;
                            break;
                        }
                    case ParameterType.Color:
                        {
                            if (_Control is Button)
                            {
                                Button ctrl = _Control as Button;
                                // Vérification des valeurs interdite
                                if (_lListForbidenValues != null)
                                {
                                    foreach (Object o in _lListForbidenValues)
                                    {
                                        Color fv = (Color)o;
                                        if (ctrl.BackColor == fv)
                                            return false;
                                    }
                                }
                                // Mise à jour de la valeur
                                _Value = ctrl.BackColor;
                            }
                            else
                                return false;
                            break;
                        }
                    case ParameterType.OpenFile:
                        {
                            if (_Control is TextBox)
                            {
                                TextBox ctrl = _Control as TextBox;
                                if (ctrl.Text == "")
                                    return false;
                                else
                                	_Value = ctrl.Text;
                            }
                            else
                                return false;
                            break;
                        }
                    case ParameterType.List:
                        {
                            if (_Control is ComboBox)
                            {
                                ComboBox ctrl = _Control as ComboBox;
                                int i = ctrl.SelectedIndex;
                                _ValueIndex = i;
                                _Value = ctrl.Items[i].ToString();
                                // Pas de vérification de validité ! 
                                // on suppose qu'on n'est pas suffisament con pour avoir donné des valeurs interdites
                            }
                            else
                                return false;
                            break;
                        }
                    case ParameterType.CheckList:
                        {
                            if (_Control is ListView)
                            {
                                ListView ctrl = _Control as ListView;
                                _ValueIndexes = new List<int>();
                                _Value = "";
                                foreach(ListViewItem o in ctrl.CheckedItems)
                                {
                                	_Value += o.Text + "\n";
                                }
                                foreach(int i in ctrl.CheckedIndices)
								{
                                	_ValueIndexes.Add(i);
								}
                            }
                            break;
                        }
                    case ParameterType.Coordinates:
                        {
                            if (_Control is TextBox)
                            {
                                TextBox ctrl = _Control as TextBox;
                                String sLat = "";
                                String sLon = "";
                                if (TryToConvertCoordinates(ctrl.Text, ref sLat, ref sLon))
                                {
                                    // Pas de vérification de valeurs interdites sur ce type
                                    _Value = sLat + " " + sLon;
                                }
                                else
                                    return false;
                            }
                            else
                                return false;
                            break;
                        }
                    case ParameterType.Double:
                        {
                            if (_Control is TextBox)
                            {
                                TextBox ctrl = _Control as TextBox;
                                try
                                {
                                    // Pas de vérification de valeur interdite sur ce type
                                    _Value = MyTools.ConvertToDouble(ctrl.Text);
                                }
                                catch (Exception)
                                {
                                    return false;
                                }
                            }
                            else
                                return false;
                            break;
                        }
                    case ParameterType.Int:
                        {
                            if (_Control is TextBox)
                            {
                                TextBox ctrl = _Control as TextBox;
                                try
                                {
                                    // Pas de vérification de valeur interdite
                                    _Value = Convert.ToInt32(ctrl.Text);
                                }
                                catch (Exception)
                                {
                                    return false;
                                }
                            }
                            else
                                return false;
                            break;
                        }
                	case ParameterType.Radius:
                        {
                            if (_Control is TextBox)
                            {
                                TextBox ctrl = _Control as TextBox;
                                try
                                {
                                    // Pas de vérification de valeur interdite
                                    _Value = Convert.ToInt32(ctrl.Text);
                                    if ((int)(_Value) <= 0)
                                    	return false;
                                }
                                catch (Exception)
                                {
                                    return false;
                                }
                            }
                            else
                                return false;
                            break;
                        }
                    case ParameterType.Password:
                    case ParameterType.TextBox:
                    case ParameterType.String:
                        {
                            if (_Control is TextBox)
                            {
                                TextBox ctrl = _Control as TextBox;
                                // Vérification des valeurs interdite
                                if (_lListForbidenValues != null)
                                {
                                    foreach (Object o in _lListForbidenValues)
                                    {
                                        String fv = (String)o;
                                        if (ctrl.Text == fv)
                                            return false;
                                    }
                                }
                                // Mise à jour de la valeur
                                _Value = ctrl.Text;
                            }
                            else
                                return false;
                            break;
                        }
                    case ParameterType.Label:
                        {
                            if (_Control is TextBox)
                            {
                                TextBox ctrl = _Control as TextBox;
                                _Value = ctrl.Text;
                            }
                            else
                                return false;
                            break;
                        }
                    default:
                        {
                            return false;
                        }
                }
                return true;
            }
        }
    }
}
