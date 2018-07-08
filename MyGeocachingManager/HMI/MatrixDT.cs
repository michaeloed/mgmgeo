/*
 * Created by SharpDevelop.
 * User: bcristiano
 * Date: 06/07/2016
 * Time: 17:36
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Net;
using System.IO;
using SpaceEyeTools;
using SpaceEyeTools.HMI;
using MyGeocachingManager.Geocaching.Filters;

namespace MyGeocachingManager.HMI
{
	/// <summary>
	/// Description of MatrixDT.
	/// </summary>
	public partial class MatrixDT : Form
	{
		MainWindow _daddy = null;
		List<String> _missingDT = new List<string>();
		
		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="daddy">Daddy</param>
		public MatrixDT(MainWindow daddy)
		{
			_daddy = daddy;
			

			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			int w = 32;
			int h = 32;
			
			
			String response = "";
			try
			{
				if (_daddy.GetInternetStatus())
				{
					_daddy.UpdateHttpDefaultWebProxy();
	                // On checke que les L/MDP soient corrects
	                // Et on récupère les cookies au passage
	                CookieContainer cookieJar = _daddy.CheckGCAccount(true, false);
	                if (cookieJar != null)
	                {
						String url = "https://www.geocaching.com/my/statistics.aspx";
			            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
			            objRequest.Proxy = _daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
			            objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
			            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
			            using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
			            {
			                response = responseStream.ReadToEnd();
			                responseStream.Close();
			            }
			            response = MyTools.GetSnippetFromText("<div id=\"DifficultyTerrainCaches\" class=\"ProfileStats\">", "</table>", response);
			            
			            Button btn = new Button();
	                    btn.Location = new System.Drawing.Point(0, 0);
	                    btn.Name = "buttonDigDTMatrix";
	                    btn.Size = new System.Drawing.Size(w, h);
	                    btn.Text = "";
	                    btn.Image =  _daddy.GetImageSized("Dig");
	                    btn.UseVisualStyleBackColor = true;
	                    btn.Click += new System.EventHandler(this.btndig_Click);
	                    this.Controls.Add(btn);
	                }
				}
			}
            catch (Exception)
            {
            }
            
			
			System.Drawing.Font fl = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			System.Drawing.Font fb = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			
			for(int col = 0; col < 9; col++)
			{
				double t = (1.0 + ((double)col)/2.0);
				Label lbl = new Label();
				lbl.Location = new System.Drawing.Point((1+col)*w, 0);
                lbl.Name = "labelc" + col.ToString();
                lbl.Size = new System.Drawing.Size(w, h);
                lbl.Text = ((t == 1.0)?"T":"") + t.ToString();
                lbl.Font = fl;
                lbl.TextAlign = ContentAlignment.MiddleCenter;
                this.Controls.Add(lbl);
			}
			
			int x = w;
			int y = h;
			for(int row = 0; row < 9; row++)
			{
				double d = (1.0 + ((double)row)/2.0);
				Label lbl = new Label();
                lbl.Location = new System.Drawing.Point(0, y + row*h);
                lbl.Name = "labelr" + row.ToString();
                lbl.Size = new System.Drawing.Size(w, h);
                lbl.Text = ((d == 1.0)?"D":"") + d.ToString();
                lbl.Font = fl;
                lbl.TextAlign = ContentAlignment.MiddleCenter;
                this.Controls.Add(lbl);
                
				for(int col = 0; col < 9; col++)
				{
					double t = (1.0 + ((double)col)/2.0);
					Button btn = new Button();
                    btn.Location = new System.Drawing.Point(x + col*w, y + row*h);
                    btn.Name = "button" + col.ToString() + row.ToString();
                    btn.Size = new System.Drawing.Size(w, h);
                    btn.Text = "";
                    btn.UseVisualStyleBackColor = true;
                    btn.Font = fb;
                    if (response != "")
                    {
                    	String tag1 = "<td id=\"" + (row+1).ToString() + "_" + (col+1).ToString() + "\" class=\"";
                    	String tag2 = "</td>";
                    	String val = MyTools.GetSnippetFromText(tag1, tag2, response);
                    	val = MyTools.GetSnippetFromText("\">", "", val);
                    	btn.Text = val;
                    	if ((val != "") && (val != "0"))
                    	{
                    		btn.BackColor = Color.LightGreen;
                    	}
                    	else
                    	{
                    		String key = d.ToString() + t.ToString();
                    		key = key.Replace(",",".");
                    		_missingDT.Add(key);
                    	}
                    }
                    btn.Tag = d.ToString() + "|" + t.ToString();
                    btn.Click += new System.EventHandler(this.btn_Click);
                    this.Controls.Add(btn);
				}
			}
			this.Size = new Size(x + 10 * w, y + 11 * h);

			TranslateForm();
			_daddy.TranslateTooltips(this, null);
		}
		
		private void TranslateForm()
		{
			this.Text = _daddy.GetTranslator().GetString("LblMatrixDTForm");
		}
		
		/// <summary>
		/// Returns list of matrix holes
		/// </summary>
		/// <param name="daddy">Mainwindow</param>
		/// <returns>null of list of (D,T) missing in D/T Matrix</returns>
		static public List<Tuple<String, String>> GetMatrixHoles(MainWindow daddy)
		{
			String response = "";
			List<Tuple<String, String>> holes = null;
			try
			{
				if (daddy.GetInternetStatus())
				{
					daddy.UpdateHttpDefaultWebProxy();
	                // On checke que les L/MDP soient corrects
	                // Et on récupère les cookies au passage
	                CookieContainer cookieJar = daddy.CheckGCAccount(true, false);
	                if (cookieJar != null)
	                {
						String url = "https://www.geocaching.com/my/statistics.aspx";
			            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
			            objRequest.Proxy = daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
			            objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
			            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
			            using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
			            {
			                response = responseStream.ReadToEnd();
			                responseStream.Close();
			            }
			            response = MyTools.GetSnippetFromText("<div id=\"DifficultyTerrainCaches\" class=\"ProfileStats\">", "</table>", response);
			            
			            
			            if (response != "")
						{
			            	holes = new List<Tuple<string, string>>();
			            	String tag2 = "</td>";
				            for(int row = 0; row < 9; row++)
							{
								double d = (1.0 + ((double)row)/2.0);
											                
								for(int col = 0; col < 9; col++)
								{
									double t = (1.0 + ((double)col)/2.0);
				                    
			                    	String tag1 = "<td id=\"" + (row+1).ToString() + "_" + (col+1).ToString() + "\" class=\"";
			                    	String val = MyTools.GetSnippetFromText(tag1, tag2, response);
			                    	val = MyTools.GetSnippetFromText("\">", "", val);
			                    	if ((val != "") && (val != "0"))
			                    	{
			                    		// Ce n'est pas un trou
			                    	}
			                    	else
			                    	{
			                    		holes.Add(new Tuple<string, string>(d.ToString().Replace(",",".") , t.ToString().Replace(",",".")));
			                    	}
								}
							}
	             	   	}
	                }
				}
			}
            catch (Exception)
            {
            	holes = null;
            }
            
            return holes;
		}
		
		private void btndig_Click(object sender, EventArgs e)
        {
			if (_missingDT.Count != 0)
			{
				CustomFilterListOfMissingDT fltr = new CustomFilterListOfMissingDT(_missingDT);
				DialogResult dialogResult = MyMessageBox.Show(
					_daddy.GetTranslator().GetStringM("AskDigMatrixDT"),
					_daddy.GetTranslator().GetString("AskDigMatrixDTTitle"),
					MessageBoxIcon.Question, 
					_daddy.GetTranslator(),
					"DigMatrixDTBtnOneTime",
					"DigMatrixDTBtnActive");
        		if (dialogResult == DialogResult.Yes)
        		{
        			_daddy._filterOverride = null;
        			_daddy.ExecuteCustomFilter(fltr);
        		}
        		else
        		{
        			_daddy._filterOverride = fltr;
        			_daddy._bUseFilter = true;
            		_daddy.PopulateListViewCache(null);
        		}
			}
			else
			{
				_daddy.MsgActionWarning(this, "#nothing to filter");
			}
		}
		
		private void btn_Click(object sender, EventArgs e)
        {
			String tag = (((Button)sender).Tag as string).Replace(",",".");
			String[] dt = tag.Split('|');
			double d = double.Parse(dt[0], System.Globalization.CultureInfo.InvariantCulture);
			double t = double.Parse(dt[1], System.Globalization.CultureInfo.InvariantCulture);
			_daddy.comboDMin.SelectedIndex = (int)((d - 1.0) * 2.0);
            _daddy.comboDMax.SelectedIndex = (int)((d - 1.0) * 2.0);
            _daddy.comboTMin.SelectedIndex = (int)((t - 1.0) * 2.0);
            _daddy.comboTMax.SelectedIndex = (int)((t - 1.0) * 2.0);
            _daddy.DoFilter();
		}
		
		void MatrixDTFormClosing(object sender, FormClosingEventArgs e)
		{
			_daddy.btnMatrixFilterDT.Enabled = true;
			_daddy._filterOverride = null;
		}
	}
	
}
