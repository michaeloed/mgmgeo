/*
 * Created by SharpDevelop.
 * User: bcristiano
 * Date: 09/09/2016
 * Time: 14:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.IO;
using SpaceEyeTools.EXControls;
using SpaceEyeTools;
using SpaceEyeTools.HMI;
using System.Collections;
using System.Collections.Generic;

namespace MyGeocachingManager.HMI
{
	/// <summary>
	/// Description of PQCheckHMI.
	/// </summary>
	public partial class PQCheckHMI : Form
	{
		MainWindow _daddy = null;
		int[] nbjours = new int[]{ 0, 0, 0, 0, 0, 0, 0, 0 };
		TextBox[] totaljours = null;
		CookieContainer cookieJar = null;
		
		/// <summary>
		/// 
		/// </summary>
		public class PQData
		{
			/// <summary>
			/// 
			/// </summary>
			public String Url;
			/// <summary>
			/// 
			/// </summary>
			public bool Checked;
			/// <summary>
			/// 
			/// </summary>
			public int Index;
			
			/// <summary>
			/// 
			/// </summary>
			/// <param name="url"></param>
			/// <param name="check"></param>
			/// <param name="index"></param>
			public PQData(String url, bool check, int index)
			{
				Url = url;
				Checked = check;
				Index = index;
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="daddy"></param>
		public PQCheckHMI(MainWindow daddy)
		{
			_daddy = daddy;
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			this.Icon = _daddy.Icon;
			this.Text = daddy.GetTranslator().GetString("checkPQToolStripMenuItem");
			
			btnGCNCancel.Text = daddy.GetTranslator().GetString("PQBtnCancel");
			btnGCNCreate.Text = daddy.GetTranslator().GetString("PQBtnApply");
			button1.Text = daddy.GetTranslator().GetString("PQBtnRefresh");
			button2.Text = daddy.GetTranslator().GetString("PQBtnDelete");
			
			// init de la liste
			lvGCNGrid.HeaderStyle = ColumnHeaderStyle.Nonclickable;
			lvGCNGrid.MouseClick += new MouseEventHandler(lstv_MouseClick);
			lvGCNGrid.FullRowSelect = true;
			lvGCNGrid.MyHighlightBrush = new SolidBrush(Color.Transparent);
			lvGCNGrid.MySelectBrush = new SolidBrush(Color.Transparent);
			EXColumnHeader col = new EXColumnHeader("Nom", 150);
			lvGCNGrid.Columns.Add(col);
			
			EXBoolColumnHeader boolcol = new EXBoolColumnHeader(daddy.GetTranslator().GetString("PQDeleteColumn"),40);
            boolcol.TrueImage = _daddy.GetImageSized("Close");
            boolcol.FalseImage = _daddy.GetImageSized("NotSelected");
			lvGCNGrid.Columns.Add(boolcol);
			
			AddColumn(daddy.GetTranslator().GetString("PQSunday"), DayOfWeek.Sunday);
			AddColumn(daddy.GetTranslator().GetString("PQMonday"), DayOfWeek.Monday);
			AddColumn(daddy.GetTranslator().GetString("PQTuesday"), DayOfWeek.Tuesday);
			AddColumn(daddy.GetTranslator().GetString("PQWednesday"), DayOfWeek.Wednesday);
			AddColumn(daddy.GetTranslator().GetString("PQThursday"), DayOfWeek.Thursday);
			AddColumn(daddy.GetTranslator().GetString("PQFriday"), DayOfWeek.Friday);
			AddColumn(daddy.GetTranslator().GetString("PQSaturday"), DayOfWeek.Saturday);
			col = new EXColumnHeader(daddy.GetTranslator().GetString("PQCheckLastGen"), 200);
			lvGCNGrid.Columns.Add(col);
			totaljours = new TextBox[]{ textBox1, textBox2, textBox3, textBox4, textBox5, textBox6, textBox7};
			
			InitPQs();
			
		}
		
		private void AddColumn(String name, DayOfWeek dayofweek)
		{
			name = name.Substring(0, 2);
			EXBoolColumnHeader boolcol = new EXBoolColumnHeader(name,30);
            boolcol.TrueImage = _daddy.GetImageSized("Selected");
            boolcol.FalseImage = _daddy.GetImageSized("NotSelected");
			//col = new EXColumnHeader("", 50);
			var col = lvGCNGrid.Columns.Add(boolcol);

		}
		
		private void InitPQs()
		{
		
			try
			{
				_daddy._ThreadProgressBarTitle = _daddy.GetTranslator().GetString("LblOperationInProgress");
	            _daddy.CreateThreadProgressBar();
	                	
				nbjours = new int[]{ 0, 0, 0, 0, 0, 0, 0, 0 };
				lvGCNGrid.Items.Clear();
				
				// Premire ligne pour les noms de colonne
				EXListViewItem lvi = null;
				
				_daddy.UpdateHttpDefaultWebProxy();
	            String post_response = "";
	            // On checke que les L/MDP soient corrects
	            // Et on récupère les cookies au passage
	            cookieJar = _daddy.CheckGCAccount(true, false);
	            if (cookieJar == null)
	                return;
	            
	            // Pour récupérer les emails
	            String url = "https://www.geocaching.com/pocket/default.aspx";
	            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
	            objRequest.Proxy = _daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
	            objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
	            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
	            using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
	            {
	                post_response = responseStream.ReadToEnd();
	                responseStream.Close();
	            }
				
	            // L'heure du serveur
	            label8.Text = _daddy.GetTranslator().GetString("PQCheckServerTime");
	            String heure = MyTools.GetSnippetFromText("<div id=\"ActivePQs\">","</p>", post_response);
	            heure = MyTools.GetSnippetFromText("</strong>","</small>", heure);
	            heure = CleanBloodyHTML(heure);
	            DateTime servtime;// = new DateTime(1990, 1, 1);
	            DateTime.TryParse(heure, out servtime);
	            label8.Text += heure;
	            
	            int i = 1;
	            while(true)
	            {
	            	
	            	String pqdeb = "";
	            	
	            	if (i<100)
	            		pqdeb = String.Format("ctl00_ContentBody_PQListControl1_PQRepeater_ctl{0:00}_uxPQImageType", i);
	            	else if (i<1000)
	            		pqdeb = String.Format("ctl00_ContentBody_PQListControl1_PQRepeater_ctl{0:000}_uxPQImageType", i);
	            	else if (i<10000)
	            		pqdeb = String.Format("ctl00_ContentBody_PQListControl1_PQRepeater_ctl{0:0000}_uxPQImageType", i);
	            	
	            	String pqbloc = MyTools.GetSnippetFromText(pqdeb, "</tr>", post_response);
	            	if (pqbloc == "")
	            		break;
	            	else
	            	{
	            		// On a un bloc valide
	            		List<String> vals = MyTools.GetSnippetsFromText("<td class=", "</td>", pqbloc);
	            		String guid = MyTools.GetSnippetFromText("<a href=", "</a>", pqbloc);
	            		guid = MyTools.GetSnippetFromText("aspx?guid=", "\"", guid);
	            		String name = MyTools.GetSnippetFromText("<a href=", "</a>", pqbloc);
	            		name = MyTools.GetSnippetFromText(">", "", name);
	            		lvi = new EXListViewItem(name);
	            		lvi.MyValue = guid;
						lvGCNGrid.Items.Add(lvi);
						
						// La suppression
						EXBoolListViewSubItem subi = new EXBoolListViewSubItem(false);
						subi.Tag = null;
						lvi.SubItems.Add(subi);
						
						// Maintenant les jours
						for(int d=1;d<=7;d++)
						{
							String bloc = vals[d];
							String urlpq = MyTools.GetSnippetFromText("<a href=\"", "\">", bloc);
							if (bloc.Contains("checkbox_off.png"))
							{
								subi = new EXBoolListViewSubItem(false);
								subi.Tag = new PQData(urlpq,false, d-1);
								lvi.SubItems.Add(subi);
							}
							else
							{
								subi = new EXBoolListViewSubItem(true);
								subi.Tag = new PQData(urlpq,true, d-1);
								nbjours[d-1] = nbjours[d-1] + 1;
								lvi.SubItems.Add(subi);
							}
						}
						// Dernière génération
						String dategen = MyTools.GetSnippetFromText("<td>", "</td>", pqbloc);
						dategen = CleanBloodyHTML(dategen);
						var subi2 = lvi.SubItems.Add(new EXListViewSubItem(dategen));
						DateTime lastgen;
						if (DateTime.TryParse(dategen, out lastgen))
					    {
							if ((servtime.Year == lastgen.Year) && (servtime.Month == lastgen.Month) && (servtime.Day == lastgen.Day))
							{
								// On met en gras
								subi2.BackColor = Color.LightGreen;
							}
					    }
						
	            	}
	            	
	            	i++;
	            }
	            
	            UpdateNbJours();
	            _daddy.KillThreadProgressBar();
			}
			catch(Exception ex)
			{
				_daddy.ShowException("", _daddy.GetTranslator().GetString("checkPQToolStripMenuItem"), ex);
				_daddy.KillThreadProgressBar();
			}
		}
		
		private String CleanBloodyHTML(String html)
		{
			html = html.Replace("\r","");
			html = html.Replace("\n","");
			html = html.TrimStart();
			html = html.TrimEnd();
			html = html.Replace("<strong>","");
			html = html.Replace("</strong>","");
			html = MyTools.StripHtmlTags(html);
			return html;
		}
		
		private void UpdateNbJours()
		{
			// Le total
            for(int k=0;k<7;k++)
            {
            	int nb = nbjours[k];
            	totaljours[k].Text = nb.ToString();
            	if (nb < 10)
            	{
            		totaljours[k].BackColor = Color.LightGreen;
            	}
            	else if (nb == 10)
            	{
            		totaljours[k].BackColor = Color.LightBlue;
            	}
            	else
            	{
            		totaljours[k].BackColor = Color.Red;
            	}
            }
		}
		
		private void lstv_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Check if click on the select column item
                ListViewHitTestInfo info = lvGCNGrid.HitTest(e.X, e.Y);
                ListViewItem.ListViewSubItem subitem = info.SubItem;
                if (subitem is EXBoolListViewSubItem)
                {
                    // Ok user clicked on selection column
                    EXBoolListViewSubItem subool = subitem as EXBoolListViewSubItem;
                    subool.BoolValue = !(subool.BoolValue);
                    PQData pqd = subool.Tag as PQData;
                    if (pqd != null)
                    {
	                    if (subool.BoolValue)
	                    {
	                    	nbjours[pqd.Index] = nbjours[pqd.Index] + 1;
	                    }
	                    else
	                    {
	                    	nbjours[pqd.Index] = nbjours[pqd.Index] - 1;
	                    }
	                    lvGCNGrid.Invalidate(subool.Bounds);
	                    UpdateNbJours();
                    }
                }
            }
        }
		
		void BtnGCNCancelClick(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
            this.Close();
		}
		
		String PQCheck(PQData pqd, bool check)
		{
			String url = "https://www.geocaching.com" + pqd.Url;
			try
			{
	            url = url.Substring(0, url.Length-1);
	            if (check)
	            	url += "1";
	            else
	            	url += "0";
	            
	            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
	            objRequest.Proxy = _daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
	            objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
	            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
	            String post_response = "";
	            using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
	            {
	                post_response = responseStream.ReadToEnd();
	                responseStream.Close();
	            }
	            return post_response;
			}
			catch(Exception ex)
			{
				String msg = "<p class=\"Warning\">EXCEPTION : " + url + " "  + ex.Message + "</p>";
				return msg;
			}
		}
		
		bool CheckResult(String pqname, String res, ref String msg)
		{
			// <p class="Warning"> </p>
			var warning = MyTools.GetSnippetFromText("<p class=\"Warning\">", "</p>", res);
			warning = CleanBloodyHTML(warning);
			if (warning != "")
				msg += pqname + " : " + warning + "\r\n";
			
			// <p class="Success">La Pocket Query <strong>_038</strong> a été <strong>activée</strong> pour <strong>Sunday</strong></p>
			var success = MyTools.GetSnippetFromText("<p class=\"Success\">", "</p>", res);
			success = CleanBloodyHTML(success);
			if (success != "")
				msg += pqname + " : " + success + "\r\n";
			if (warning != "")
				return false;
			else
				return true;
		}
		
		void BtnGCNCreateClick(object sender, EventArgs e)
		{
			foreach(int nb in nbjours)
			{
				if (nb > 10)
				{
                	_daddy.MsgActionError(this, _daddy.GetTranslator().GetString("PQCheckErrNb"));
					return;
				}
			}
			String msg = "";
			
			_daddy._ThreadProgressBarTitle = _daddy.GetTranslator().GetString("LblOperationInProgress");
	        _daddy.CreateThreadProgressBar();
	            
			// On va commencer par décocher les PQs qui ne sont plus cochées
			// *************************************************************
			for(int i=0;i<lvGCNGrid.Items.Count;i++)
			{
				EXListViewItem lvi = (EXListViewItem)(lvGCNGrid.Items[i]);
				// On parcourt ses sous items
				for(int k=2; k< lvi.SubItems.Count; k++)
				{
					 EXBoolListViewSubItem svi = lvi.SubItems[k] as EXBoolListViewSubItem;
					 if (svi != null)
					 {
					 	PQData pqd = svi.Tag as PQData;
					 	if (pqd != null)
					 	{
					 		if ((svi.BoolValue == false) && (pqd.Checked))
					 		{
					 			// On a décoché, on traite
					 			//msg += lvi.Text + " décochée pour le jour " + k.ToString() + "\r\n";
					 			String res = PQCheck(pqd, svi.BoolValue);
					 			CheckResult(lvi.Text, res, ref msg);
					 		}
					 		
					 	}
					 }
				}
			}
			
			// Puis on coche les nouvelles PQs
			// *******************************
			for(int i=0;i<lvGCNGrid.Items.Count;i++)
			{
				EXListViewItem lvi = (EXListViewItem)(lvGCNGrid.Items[i]);
				// On parcourt ses sous items
				for(int k=2; k< lvi.SubItems.Count; k++)
				{
					 EXBoolListViewSubItem svi = lvi.SubItems[k] as EXBoolListViewSubItem;
					 if (svi != null)
					 {
					 	PQData pqd = svi.Tag as PQData;
					 	if (pqd != null)
					 	{
					 		if ((svi.BoolValue) && (!pqd.Checked))
					 		{
					 			// On a coché, on traite
					 			//msg += lvi.Text + " cochée pour le jour " + k.ToString() + "\r\n";
					 			String res = PQCheck(pqd, svi.BoolValue);
					 			CheckResult(lvi.Text, res, ref msg);
					 		}
					 		
					 	}
					 }
				}
			}
			
			if (msg != "")
				textBox8.Text = msg;
			else
				textBox8.Text = _daddy.GetTranslator().GetString("LblActionDone");
			
			// Recharge ?
			InitPQs();
			_daddy.KillThreadProgressBar();
		}
		
		void Button1Click(object sender, EventArgs e)
		{
			InitPQs();
		}
		void Button2Click(object sender, EventArgs e)
		{
			// Supprime la sélection
			try
			{
				_daddy._ThreadProgressBarTitle = _daddy.GetTranslator().GetString("PQBtnDelete");
	        	_daddy.CreateThreadProgressBar();
	            
	        	// On va parcourir chaque PQ, ouvrir sa page et poster la suppression
	        	for(int i=0;i<lvGCNGrid.Items.Count;i++)
				{
					EXListViewItem lvi = (EXListViewItem)(lvGCNGrid.Items[i]);
					
					 EXBoolListViewSubItem svi = lvi.SubItems[1] as EXBoolListViewSubItem;
					 if (svi != null)
					 {
					 	
				 		if (svi.BoolValue == true)
				 		{
				 			String guid = lvi.MyValue;
			                guid = "https://www.geocaching.com/pocket/gcquery.aspx?guid=" + guid;
			                PQDownloadHMI.DeletePQ(_daddy, guid);
				 		}				 	
					 }
				}
				
				// On rafraichit
				InitPQs();
				
				_daddy.KillThreadProgressBar();
			}
			catch(Exception ex)
			{
				_daddy.ShowException("", _daddy.GetTranslator().GetString("checkPQToolStripMenuItem"), ex);
				_daddy.KillThreadProgressBar();
			}
			
			
		}
	}
}
