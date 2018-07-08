/*
 * Created by SharpDevelop.
 * User: bcristiano
 * Date: 10/08/2016
 * Time: 09:09
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using MyGeocachingManager.Geocaching;
using SpaceEyeTools.EXControls;
using SpaceEyeTools;
using SpaceEyeTools.HMI;
using System.Web;
using System.Net;
using System.IO;

namespace MyGeocachingManager.HMI
{
	/// <summary>
	/// Description of NotificationCreation.
	/// </summary>
	public partial class NotificationCreation : Form
	{
		MainWindow _daddy = null;
		String _sErrTitle = "";
		String _sErrFormat = "";
		List<Tuple<String, List<int>>> listOfAlloweKindPerCacheType = null;
		List<String> listOfCacheTypes = null;
		List<int> listOfCacheTypesId = null;
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="daddy"></param>
		public NotificationCreation(MainWindow daddy)
		{
			_daddy = daddy;
			_sErrTitle = _daddy.GetTranslator().GetString("Error");
			_sErrFormat = _daddy.GetTranslator().GetString("ErrWrongParameter");
			
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			if (!SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled()) return;
			
			btnGCNMap.Image = _daddy.GetImageSized("Earth");
			_daddy._cacheDetail._gmap.ControlTextLatLon = tbGCNCenter;
			tbGCNCenter.TextChanged += new System.EventHandler(this.txtCoord_TextChanged);
			tbGCNCenter.Text = _daddy.HomeLat.ToString() + " " + _daddy.HomeLon.ToString();
			this.Text = _daddy.GetTranslator().GetString("createpublishnotifications");
			lblGCNRadius.Text = _daddy.GetTranslator().GetString("FTFDistance");
			lblGCNCentre.Text = _daddy.GetTranslator().GetString("ParamCenterLatLon");
			lblGCNEmail.Text = _daddy.GetTranslator().GetString("FTFEmails");
			lblGCNGrid.Text = _daddy.GetTranslator().GetString("FTFCacheTypes");
			lblGCNNom.Text = _daddy.GetTranslator().GetString("FTFName");
			btnGCNCancel.Text = _daddy.GetTranslator().GetString("BtnCancel");
			btnGCNCreate.Text = _daddy.GetTranslator().GetString("BtnOK");
			
			daddy.UpdateHttpDefaultWebProxy();
            String post_response = "";
            // On checke que les L/MDP soient corrects
            // Et on récupère les cookies au passage
            CookieContainer cookieJar = daddy.CheckGCAccount(true, false);
            if (cookieJar == null)
                return;
            
            // Pour récupérer les emails
            String url = "https://www.geocaching.com/notify/edit.aspx";
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
            objRequest.Proxy = daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
            objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
            using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
            {
                post_response = responseStream.ReadToEnd();
                responseStream.Close();
            }
            if (NotificationsManager.CheckWarningMessage(_daddy, post_response))
    		{
    			// Shit
    		}
            
            List<String> lsemails = new List<string>();
            String email = "";
	        String mails = MyTools.GetSnippetFromText("ctl00$ContentBody$LogNotify$ddlAltEmails","select>",post_response);
	        lsemails = MyTools.GetSnippetsFromText("value=\"", "\">", mails);
	        if (lsemails.Count != 0)
	        	email = lsemails[0];
	        cbGCNEmails.Items.AddRange(lsemails.ToArray());
            if (cbGCNEmails.Items.Count != 0)
            {
                cbGCNEmails.SelectedIndex = 0;
            }
            else
            {
                cbGCNEmails.Visible = false;
                lblGCNEmail.Visible = false;
            }
	      
			// List of cache type (to associate with list of int)
			listOfCacheTypes = new List<string>(new String[] {"Earthcache", "Event Cache", "Cache In Trash Out Event", "Giga-Event Cache", "Mega-Event Cache", "Letterbox Hybrid", "Multi-cache", "Traditional Cache", "Unknown Cache", "Wherigo Cache", "Virtual Cache" }); // BCR 20170825
			listOfCacheTypesId = new List<int>(new int[] {137, 6, 13, 7005, 453, 5, 3, 2, 8, 1858, 4 }); // BCR 20170825

			//Matrix
			listOfAlloweKindPerCacheType = new List<Tuple<String, List<int>>>(); // BCR 20170825
			listOfAlloweKindPerCacheType.Add(new Tuple<String,List<int>>("Found it", new List<int>(new int[] { 0, -1, -1, -1, -1, 0, 0, 0, 0, 0, 0 })));
			listOfAlloweKindPerCacheType.Add(new Tuple<String,List<int>>("Retract Listing", new List<int>(new int[] { 7, 8, 8, 8, 8, 7, 7, 7, 7, 7, 7 })));
			listOfAlloweKindPerCacheType.Add(new Tuple<String,List<int>>("Didn't find it", new List<int>(new int[] { 1, -1, -1, -1, -1, 1, 1, 1, 1, 1, 1 })));
			listOfAlloweKindPerCacheType.Add(new Tuple<String,List<int>>("Temporarily Disable Listing", new List<int>(new int[] { 8, 10, 10, 10, 9, 8, 8, 8, 8, 8, 8 })));
			listOfAlloweKindPerCacheType.Add(new Tuple<String,List<int>>("Write note", new List<int>(new int[] { 2, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2 })));
			listOfAlloweKindPerCacheType.Add(new Tuple<String,List<int>>("Enable Listing", new List<int>(new int[] { 9, 11, 11, 11, 10, 9, 9, 9, 9, 9, 9 })));
			listOfAlloweKindPerCacheType.Add(new Tuple<String,List<int>>("Archive", new List<int>(new int[] { 3, 1, 1, 1, 1, 3, 3, 3, 3, 3, 3 })));
			listOfAlloweKindPerCacheType.Add(new Tuple<String,List<int>>("Update Coordinates", new List<int>(new int[] { 10, 9, 9, 9, 11, 10, 10, 10, 10, 10, 10 })));
			listOfAlloweKindPerCacheType.Add(new Tuple<String,List<int>>("Needs Archived", new List<int>(new int[] { 4, 2, 2, 2, 2, 4, 4, 4, 4, 4, 4 })));
			listOfAlloweKindPerCacheType.Add(new Tuple<String,List<int>>("Needs Maintenance", new List<int>(new int[] { 11, -1, -1, -1, 12, 11, 11, 11, 11, 11, 11 })));
			listOfAlloweKindPerCacheType.Add(new Tuple<String,List<int>>("Unarchive", new List<int>(new int[] { 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5 })));
			listOfAlloweKindPerCacheType.Add(new Tuple<String,List<int>>("Owner Maintenance", new List<int>(new int[] { 12, -1, -1, -1, -1, 12, 12, 12, 12, 12, 12 })));
			listOfAlloweKindPerCacheType.Add(new Tuple<String,List<int>>("Publish Listing", new List<int>(new int[] { 6, 7, 7, 7, 7, 6, 6, 6, 6, 6, 6 })));
			listOfAlloweKindPerCacheType.Add(new Tuple<String,List<int>>("Announcement", new List<int>(new int[] { -1, 6, 6, 6, 6, -1, -1, -1, -1, -1, -1 })));
			listOfAlloweKindPerCacheType.Add(new Tuple<String,List<int>>("Will Attend", new List<int>(new int[] { -1, 3, 3, 3, 3, -1, -1, -1, -1, -1, -1 })));
			listOfAlloweKindPerCacheType.Add(new Tuple<String,List<int>>("Attended", new List<int>(new int[] { -1, 4, 4, 4, 4, -1, -1, -1, -1, -1, -1 })));

			// Create listview
			lvGCNGrid.HeaderStyle = ColumnHeaderStyle.None;
			ImageList imglist = new ImageList();
			imglist.ColorDepth = ColorDepth.Depth32Bit;
            imglist.ImageSize = new Size(20, 20); // this will affect the row height
            imglist.Images.Add(_daddy.GetImageSized("Fail"));
			lvGCNGrid.SmallImageList = imglist;
			lvGCNGrid.MouseClick += new MouseEventHandler(lstv_MouseClick);
			lvGCNGrid.FullRowSelect = true;
			lvGCNGrid.MyHighlightBrush = new SolidBrush(Color.Transparent);
			lvGCNGrid.MySelectBrush = new SolidBrush(Color.Transparent);
			
			// Create columns
			// First column is for notification kind
			EXColumnHeader col = new EXColumnHeader("Type", 150);
			lvGCNGrid.Columns.Add(col);
			// And a column per cache type
			foreach(String key in listOfCacheTypes)
			{
				EXBoolColumnHeader boolcol = new EXBoolColumnHeader("",20);
	            boolcol.TrueImage = _daddy.GetImageSized("Selected");
	            boolcol.FalseImage = _daddy.GetImageSized("NotSelected");
				//col = new EXColumnHeader("", 50);
				lvGCNGrid.Columns.Add(boolcol);
			}
			
			// And a line with all cache type
			EXListViewItem lvi = new EXListViewItem("Cache types");
			lvGCNGrid.Items.Add(lvi);
			foreach(String key in listOfCacheTypes)
			{
				EXImageListViewSubItem si = new EXImageListViewSubItem(MyTools.ResizeImage(_daddy.GetImageSized(key), 16, 16));
				lvi.SubItems.Add(si);
			}
			
			// Add a line for each notification kind
			foreach(Tuple<String, List<int>> o in listOfAlloweKindPerCacheType)
			{
				lvi = new EXListViewItem(o.Item1);
				lvGCNGrid.Items.Add(lvi);
				foreach(int i in o.Item2)
				{
					if (i != -1)
					{
						EXBoolListViewSubItem subi = new EXBoolListViewSubItem(false);
						lvi.SubItems.Add(subi);
					}
					else
					{
						EXListViewSubItem subi = new EXListViewSubItem("");
						subi.BackColor = Color.DarkBlue;
						lvi.SubItems.Add(subi);
					}
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
                    lvGCNGrid.Invalidate(subool.Bounds);
                }
            }
        }
		
		void DisplayError(String name, String attendu, String saisie, String interdit)
		{
			// Valeur incorrecte pour le paramètre "{0}"#* Attendu : {1}#* Saisie : {2}#* Interdit : {3}
			String msg = String.Format(_sErrFormat.Replace("#","\r\n"), name, attendu, saisie, interdit) + "\r\n";
			
			MessageBox.Show(msg, _sErrTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
			
		}
		
		void BtnGCNCreateClick(object sender, EventArgs e)
		{
			// On vérifie
			// coordonnées
			Double dLat = Double.MaxValue;
            Double dLon = Double.MaxValue;
            String sLat = "";
            String sLon = "";
            bool bOK = ParameterObject.TryToConvertCoordinates(tbGCNCenter.Text, ref sLat, ref sLon);
            if (sLat != CoordConvHMI._sErrorValue)
                dLat = MyTools.ConvertToDouble(sLat);
            if (sLon != CoordConvHMI._sErrorValue)
                dLon = MyTools.ConvertToDouble(sLon);
            if (!bOK)
            {
            	DisplayError(lblGCNCentre.Text, _daddy.GetTranslator().GetString("WaypointCoord"), tbGCNCenter.Text, "");
            	return;
            }
					
			// nom
			if (tbGCNName.Text == "")
			{
            	DisplayError(lblGCNNom.Text, _daddy.GetTranslator().GetString("lblnotempty"), tbGCNName.Text, "");
            	return;
            }
			
			// radius
			Int32 radius = 0;
			if (!Int32.TryParse(tbGCNRadius.Text, out radius))
			{
            	DisplayError(lblGCNRadius.Text, _daddy.GetTranslator().GetString("lblnotnumber"), tbGCNRadius.Text, "");
            	return;
            }
			
			// email
			if ((cbGCNEmails.Visible) && (cbGCNEmails.SelectedIndex == -1))
			{
            	DisplayError(lblGCNEmail.Text, _daddy.GetTranslator().GetString("lblvalue"), "", "");
            	return;
            }

            String email = "";
            if (cbGCNEmails.Items.Count != 0)
            {
                int pos = cbGCNEmails.SelectedIndex;
                email = cbGCNEmails.Items[pos].ToString();
            }
            
			// type			
			// au moins une croix quelque part...
			// int associé au type de cache, nom du type de cache, liste des commandes POST pour kind of notif
			// Tuple<int, string, List<String>
			// on va créer un dico avec comme clé le type de cache
			Dictionary<String, Tuple<int, string, List<String>>> dicoCreation = new Dictionary<string, Tuple<int, string, List<string>>>();
			// On parcourt toutes les lignes, sauf la première qui correspond aux types
			for(int i=1; i < lvGCNGrid.Items.Count; i++)
			{
				// On a l'item
				EXListViewItem lvi = (EXListViewItem)(lvGCNGrid.Items[i]);
				
				// On parcourt ses sous items
				for(int k=1; k< lvi.SubItems.Count; k++)
				{
					 EXBoolListViewSubItem svi = lvi.SubItems[k] as EXBoolListViewSubItem;
					 if (svi != null)
					 {
					 	// On a une valeur checkable
					 	if (svi.BoolValue)
					 	{
					 		// Et elle est checkée !!
					 		// On construit ce qu'il nous faut maintenant
					 		//msg += listOfCacheTypes[k-1] + " " + listOfCacheTypesId[k-1] + " " + listOfAlloweKindPerCacheType[i-1].Item1 + " " + listOfAlloweKindPerCacheType[i-1].Item2[k-1] + "\r\n";
					 		
					 		
					 		String typeofcache = listOfCacheTypes[k-1];
					 		int typeofcacheid = listOfCacheTypesId[k-1];
					 		String kindofnotifreadable = listOfAlloweKindPerCacheType[i-1].Item1;
					 		String kindofnotifpost = "ctl00$ContentBody$LogNotify$cblLogTypeList$" + listOfAlloweKindPerCacheType[i-1].Item2[k-1];
					 		if (dicoCreation.ContainsKey(typeofcache))
					 		{
					 			// On met à jour la liste des kind of notif
					 			Tuple<int, string, List<String>> obj = dicoCreation[typeofcache];
					 			obj.Item3.Add(kindofnotifpost);
					 		}
					 		else
					 		{
					 			Tuple<int, string, List<String>> obj = new Tuple<int, string, List<string>>(typeofcacheid, typeofcache, new List<string>(new string[] { kindofnotifpost}));
					 			dicoCreation.Add(typeofcache, obj);
					 		}
					 	}
					 }
				}			
			}
			
			if (dicoCreation.Count == 0)
			{
				DisplayError(lblGCNGrid.Text, _daddy.GetTranslator().GetString("lblvalue"), "", "");
            	return;
			}
			
			/*
			foreach(KeyValuePair<String, Tuple<int, string, List<String>>> pair in dicoCreation)
			{
				Tuple<int, string, List<String>> obj = pair.Value;
				msg += obj.Item1.ToString() + " " + obj.Item2 + " -> ";
				foreach(String s in obj.Item3)
				{
					msg += s + " ";
				}
				msg += "\r\n";
			}
			_daddy.MSG(msg);
			return;
			*/
			
			// On est valide, on peut créer
			_daddy._cacheDetail._gmap.ControlTextLatLon = null;
			
			// Go création
			CookieContainer cookieJar = _daddy.CheckGCAccount(true, false);
			String url = "https://www.geocaching.com/notify/edit.aspx";
			bool error = false;
            foreach(KeyValuePair<String, Tuple<int, string, List<String>>> pair in dicoCreation)
			{
				Tuple<int, string, List<String>> obj = pair.Value;
				// On demande la page par défaut pour initialiser une nouvelle demande
        		HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
                objRequest.Proxy = _daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
                objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
                HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
                String post_response;
                using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
                {
                    post_response = responseStream.ReadToEnd();
                    responseStream.Close();
                }
                // On regarde si on a un message de warning
	    		if (NotificationsManager.CheckWarningMessage(_daddy, post_response))
	    		{
	    			// Shit
	    			error = true;
	    			break;
	    		}
	    		
        		// Une mise à jour pour définir le type de cache
        		String post_string = NotificationsManager.GeneratePostString(_daddy, post_response, dLat, dLon, radius, tbGCNName.Text, obj, email, true);
        		post_response = NotificationsManager.GeneratePostRequets(_daddy, url, post_string, cookieJar);
        		
        		// Une mise à jour pour définir le type de notif
        		post_string = NotificationsManager.GeneratePostString(_daddy, post_response, dLat, dLon, radius, tbGCNName.Text, obj, email, true);
        		post_response = NotificationsManager.GeneratePostRequets(_daddy, url, post_string, cookieJar);
        		if (NotificationsManager.CheckValidationMessage(_daddy, post_response))
        		{
        			// Shit
        			error = true;
        			break;
        		}
			}
            if (!error)
            {
				_daddy.MsgActionDone(this);
            }
            
			this.DialogResult = DialogResult.OK;
            this.Close();
		}
        
		void BtnGCNCancelClick(object sender, EventArgs e)
		{
			// Si valide
			_daddy._cacheDetail._gmap.ControlTextLatLon = null;
			
			this.DialogResult = DialogResult.Cancel;
            this.Close();
		}
		
		void BtnGCNMapClick(object sender, EventArgs e)
		{
			Double dLat = Double.MaxValue;
            Double dLon = Double.MaxValue;
            String sLat = "";
            String sLon = "";
            bool bOK = ParameterObject.TryToConvertCoordinates(tbGCNCenter.Text, ref sLat, ref sLon);
            if (sLat != CoordConvHMI._sErrorValue)
                dLat = MyTools.ConvertToDouble(sLat);
            if (sLon != CoordConvHMI._sErrorValue)
                dLon = MyTools.ConvertToDouble(sLon);
            if (bOK)
                _daddy.HandlerToDisplayCoordinates(dLat, dLon);
            else
                MessageBox.Show(_sErrTitle, _sErrTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
		
		private void txtCoord_TextChanged(object sender, EventArgs e)
        {
			tbGCNAllCoords.Text = ParametersChanger.ConvertCoordinates(tbGCNCenter.Text);
        }
	}
}
