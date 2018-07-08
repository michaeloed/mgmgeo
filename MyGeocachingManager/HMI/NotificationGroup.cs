/*
 * Created by SharpDevelop.
 * User: bcristiano
 * Date: 12/08/2016
 * Time: 10:10
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using SpaceEyeTools.EXControls;
using System.Net;
using MyGeocachingManager.Geocaching;
using System.Collections.Generic;
using SpaceEyeTools.HMI;
using SpaceEyeTools;

namespace MyGeocachingManager.HMI
{
	/// <summary>
	/// Description of NotificationGroup.
	/// </summary>
	public partial class NotificationGroup : Form
	{
		MainWindow _daddy = null;
		Dictionary<EXListViewItem, List<GCNotification>> _dicoItemGCs = new Dictionary<EXListViewItem, List<GCNotification>>();
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="daddy"></param>
		public NotificationGroup(MainWindow daddy)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			_daddy = daddy;
			this.Text = _daddy.GetTranslator().GetString("menupublishnotifications");;
			
			btnGCNGDelete.Text = _daddy.GetTranslator().GetString("deletepublishnotifications");
			btnGCNGDelete.Image = _daddy._imgMenus["deletepublishnotifications"];
			btnGCNGDelete.ImageAlign = ContentAlignment.MiddleLeft;
			btnGCNGDelete.TextAlign = ContentAlignment.MiddleCenter; 

            btnGCNGToggle.Text = _daddy.GetTranslator().GetString("togglepublishnotifications");
            btnGCNGToggle.Image = _daddy._imgMenus["togglepublishnotifications"];
            btnGCNGToggle.ImageAlign = ContentAlignment.MiddleLeft;
			btnGCNGToggle.TextAlign = ContentAlignment.MiddleCenter; 

            btnGCNGUpdate.Text = _daddy.GetTranslator().GetString("updatepublishnotifications");
            btnGCNGUpdate.Image = _daddy._imgMenus["updatepublishnotifications"];
            btnGCNGUpdate.ImageAlign = ContentAlignment.MiddleLeft;
			btnGCNGUpdate.TextAlign = ContentAlignment.MiddleCenter; 

            btnGCNGMap.Text = _daddy.GetTranslator().GetString("mappublishnotifications");
            btnGCNGMap.Image = _daddy._imgMenus["mappublishnotifications"];
            btnGCNGMap.ImageAlign = ContentAlignment.MiddleLeft;
			btnGCNGMap.TextAlign = ContentAlignment.MiddleCenter; 

			ImageList imglist = new ImageList();
			imglist.ColorDepth = ColorDepth.Depth32Bit;
            imglist.ImageSize = new Size(32, 32); // this will affect the row height
            imglist.Images.Add("GroupCaches", _daddy.GetImageSized("GroupCaches"));
            foreach(String s in Geocaching.GeocachingConstants.GetSupportedCacheTypes())
            {
            	imglist.Images.Add(s, _daddy.GetImageSized(s));
            }
			lvGCNListGroup.SmallImageList = imglist;
			lvGCNListGroup.MouseClick += new MouseEventHandler(lstv_MouseClick);
			lvGCNListGroup.FullRowSelect = true;
						
			EXColumnHeader col = null;
			col = new EXColumnHeader(_daddy.GetTranslator().GetString("FTFName"), 200);
			lvGCNListGroup.Columns.Add(col);
			col = new EXColumnHeader("", 40);
			lvGCNListGroup.Columns.Add(col);
			col = new EXColumnHeader("", 24);
			lvGCNListGroup.Columns.Add(col);
			col = new EXColumnHeader(_daddy.GetTranslator().GetString("ParamCenterLatLon"), 150);
			lvGCNListGroup.Columns.Add(col);
			col = new EXColumnHeader(_daddy.GetTranslator().GetString("FTFDistance"), 80);
			lvGCNListGroup.Columns.Add(col);
			col = new EXColumnHeader(_daddy.GetTranslator().GetString("FTFNotifTypes"), 150);
			lvGCNListGroup.Columns.Add(col);
			
			_daddy._ThreadProgressBarTitle = _daddy.GetTranslator().GetString("menupublishnotifications");
			_daddy.CreateThreadProgressBar();
			
			PopulateList();
			_daddy.KillThreadProgressBar();
		}
		
		private void PopulateList()
		{
			// **** BLOC TO RETRIEVE NOTIFICATIONS LIST ***
            if (!SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled()) return;
        	List<String> options;
        	ImageList imglist2;
        	List<Tuple<String, String, bool, String, String, String>> lsentries;
        	List<String> checkedvalues;
        	CookieContainer cookieJar = _daddy.CheckGCAccount(true, false);
            if (cookieJar == null)
                return;
        	NotificationsManager.RetrieveNotificationsList(_daddy, out options, out imglist2, out lsentries, out checkedvalues);
        	// **** END BLOC TO RETRIEVE NOTIFICATIONS LIST ***
        	
        	// Get details on notifications and group them
        	Dictionary<String, List<GCNotification>> diconotifs = new Dictionary<string, List<GCNotification>>();
        	String post_response = "";
        	foreach(Tuple<String, String, bool, String, String, String> tpl in lsentries)
        	{
        		GCNotification gcn = NotificationsManager.GetNotificationData(_daddy, tpl.Item2, ref post_response);
        		String key = gcn.dlat.ToString() + gcn.dlon.ToString() + gcn.distance.ToString();
    			if (diconotifs.ContainsKey(key))
    			{
    				diconotifs[key].Add(gcn);
    			}
    			else
    			{
    				diconotifs.Add(key, new List<GCNotification>(new GCNotification[] { gcn }));
    			}
        	}
        	
        	_dicoItemGCs.Clear();
        	lvGCNListGroup.Items.Clear();
        	foreach(KeyValuePair<String, List<GCNotification>> pair in diconotifs)
        	{
        		GCNotification gcn = pair.Value[0];
        		String sLat2 = CoordConvHMI.ConvertDegreesToDDMM(gcn.dlat, true);
                String sLon2 = CoordConvHMI.ConvertDegreesToDDMM(gcn.dlon, false);
                String coord = /*"DD° MM.MMM: " + */sLat2 + " " + sLon2 + "\r\n";
                String radius = gcn.distance.ToString() + " Km";
                
        		// Create line
        		if (pair.Value.Count == 1)
        		{
        			AddGCNToList(gcn, gcn.GetTypeKeyInEnglish(), pair.Value);
        		}
        		else
        		{
        			AddGCNToList(gcn, "GroupCaches", pair.Value);
        		}
        	}
		}
		
		private void AddGCNToList(GCNotification gcn, String keyIcon, List<GCNotification> lst, bool special = false, int index = -1)
		{
			String sLat2 = CoordConvHMI.ConvertDegreesToDDMM(gcn.dlat, true);
            String sLon2 = CoordConvHMI.ConvertDegreesToDDMM(gcn.dlon, false);
            String coord = /*"DD° MM.MMM: " + */sLat2 + " " + sLon2 + "\r\n";
            String radius = gcn.distance.ToString() + " Km";
            
            String lbl = gcn.name;
            if (special)
            	lbl = gcn.GetTypeKeyInEnglish();
            
            EXImageListViewItem lvi = null;
            if (special)
            {
            	lvi = new EXImageListViewItem(lbl, _daddy.GetImageSized("Nothing"));
            	EXImageListViewSubItem svi2 = new EXImageListViewSubItem(_daddy.GetImageSized(keyIcon));
    			lvi.SubItems.Add(svi2);
            }
            else
            {
            	lvi = new EXImageListViewItem(lbl, _daddy.GetImageSized(keyIcon));
            	EXImageListViewSubItem svi2 = new EXImageListViewSubItem("");
    			lvi.SubItems.Add(svi2);
            }
            if (lst.Count != 1)
            {
            	lvi.Font = MyTools.ChangeFontStyle(lvi.Font, true, false);
            	
            	EXImageListViewSubItem svi2 = new EXImageListViewSubItem("");
	    		lvi.SubItems.Add(svi2);
            }
            else
            {
	            if (gcn.checknotif)
	            {
	            	EXImageListViewSubItem svi2 = new EXImageListViewSubItem(_daddy.GetImageSized("bulletgreen"));
	    			lvi.SubItems.Add(svi2);
	            }
	            else
	            {
	            	EXImageListViewSubItem svi2 = new EXImageListViewSubItem(_daddy.GetImageSized("bulletred"));
	    			lvi.SubItems.Add(svi2);
	            }
            }
            
    		EXListViewSubItem svi = null;
    		if (special)
    		{
    			svi = new EXListViewSubItem("");
	    		lvi.SubItems.Add(svi);
	    		svi = new EXListViewSubItem("");
	    		lvi.SubItems.Add(svi);
    		}
    		else
    		{
	    		svi = new EXListViewSubItem(coord);
	    		lvi.SubItems.Add(svi);
	    		svi = new EXListViewSubItem(radius);
	    		lvi.SubItems.Add(svi);
    		}
    		String kind = "";
    		if (lst.Count == 1)
    		{
	    		foreach(String k in gcn.kindofnotifnames)
	    			kind += k + " - ";
    		}
    		svi = new EXListViewSubItem(kind);
    		lvi.SubItems.Add(svi);
    		
    		if (special)
    			lvGCNListGroup.Items.Insert(index+1, lvi);
    		else
    			lvGCNListGroup.Items.Add(lvi);
    		
    		_dicoItemGCs.Add(lvi, lst);
		}
		
		private void lstv_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Check if click on the select column item
                ListViewHitTestInfo info = lvGCNListGroup.HitTest(e.X, e.Y);
                var item = info.Item;
                if (item is EXImageListViewItem)
                {
                    // Ok user clicked on selection column
                    EXImageListViewItem lvi = item as EXImageListViewItem;
                    if (_dicoItemGCs.ContainsKey(lvi))
                    {
                    	List<GCNotification> gcns = _dicoItemGCs[lvi];
                    	int itemindex = lvGCNListGroup.Items.IndexOf(item);
                    	if (gcns.Count > 1)
                    	{
                    		if (gcns[0].Tag == false)
                    		{
	                    		// On crée les fils
                    			foreach(GCNotification gcn in gcns)
	                    		{
	                    			AddGCNToList(gcn, gcn.GetTypeKeyInEnglish(), new List<GCNotification>(new GCNotification[]{ gcn }), true, itemindex);
	                    		}
                    			gcns[0].Tag = true;
                    		}
                    		else
                    		{
                    			// on supprime les fils
                    			for(int i=1;i<=gcns.Count;i++)
                    			{
                    				EXImageListViewItem lvsub = (EXImageListViewItem)(lvGCNListGroup.Items[itemindex + 1]);
                    				_dicoItemGCs.Remove(lvsub);
                    				lvGCNListGroup.Items.Remove(lvsub);
                    			}
                    			gcns[0].Tag = false;
                    		}
                    	}
                    }
                    
                }
            }
        }
		
		private List<String> GetSelectedIds(out List<GCNotification> associatedGCN)
		{
			List<String> ids = new List<string>();
			associatedGCN = new List<GCNotification>();
			
			Dictionary<string, GCNotification> selectionDico = new Dictionary<string, GCNotification>();
			foreach(ListViewItem item in lvGCNListGroup.SelectedItems)
			{
				EXListViewItem lvi = item as EXListViewItem;
				foreach(GCNotification gcn in _dicoItemGCs[lvi])
				{
					if (!selectionDico.ContainsKey(gcn.id))
					{
						selectionDico.Add(gcn.id, gcn);
					}
				}
			}
			
			foreach(var pair in selectionDico)
			{
				ids.Add(pair.Key);
				associatedGCN.Add(pair.Value);
			}
			return ids;
		}
		
		void BtnGCNGDeleteClick(object sender, EventArgs e)
		{
			String fct = "deletepublishnotifications";
			try
			{
				if (!SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled()) return;
	        	CookieContainer cookieJar = _daddy.CheckGCAccount(true, false);
	            if (cookieJar == null)
	                return;
	            
	            List<GCNotification> associatedGCN;
				List<String> ids = GetSelectedIds(out associatedGCN);
				if (ids.Count == 0)
				{
            		_daddy.MsgActionWarning(this, _daddy.GetTranslator().GetString("LblErrorNoSelectionElt"));
            		return;
            	}
				
	        	DialogResult dialogResult = MessageBox.Show(_daddy.GetTranslator().GetString("AskConfirm"),
				                                            _daddy.GetTranslator().GetString(fct),
	                            							MessageBoxButtons.YesNo, MessageBoxIcon.Question);
	            if (dialogResult == DialogResult.Yes)
	            {	            
		            foreach(String id in ids)
		            	NotificationsManager.DeleteNotificationsImpl(_daddy, id, cookieJar);
		            
					PopulateList();
					_daddy.MsgActionDone(this);
	            }
			}
            catch (Exception ex)
            {
                _daddy.ShowException("", _daddy.GetTranslator().GetString(fct), ex);
            }
		}
		
		void BtnGCNGToggleClick(object sender, EventArgs e)
		{
			String fct = "togglepublishnotifications";
			try
			{
				if (!SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled()) return;
	        	CookieContainer cookieJar = _daddy.CheckGCAccount(true, false);
	            if (cookieJar == null)
	                return;
	            
	            List<GCNotification> associatedGCN;
				List<String> ids = GetSelectedIds(out associatedGCN);
				if (ids.Count == 0)
				{
            		_daddy.MsgActionWarning(this, _daddy.GetTranslator().GetString("LblErrorNoSelectionElt"));
            		return;
            	}
				
				
	        	DialogResult dialogResult = MessageBox.Show(_daddy.GetTranslator().GetString("AskConfirm"),
				                                            _daddy.GetTranslator().GetString(fct),
	                            							MessageBoxButtons.YesNo, MessageBoxIcon.Question);
	            if (dialogResult == DialogResult.Yes)
	            {
				
		            foreach(String id in ids)
		            	NotificationsManager.ToggleNotificationsImpl(_daddy, id, cookieJar);
		            
					PopulateList();
					_daddy.MsgActionDone(this);
	            }
			}
            catch (Exception ex)
            {
                _daddy.ShowException("", _daddy.GetTranslator().GetString(fct), ex);
            }
		}
		
		void BtnGCNGUpdateClick(object sender, EventArgs e)
		{
			String fct = "updatepublishnotifications";
			try
			{
				if (!SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled()) return;
	        	CookieContainer cookieJar = _daddy.CheckGCAccount(true, false);
	            if (cookieJar == null)
	                return;
	            
	            List<GCNotification> associatedGCN;
				List<String> ids = GetSelectedIds(out associatedGCN);
				if (ids.Count == 0)
				{
            		_daddy.MsgActionWarning(this, _daddy.GetTranslator().GetString("LblErrorNoSelectionElt"));
            		return;
            	}
				
				List<ParameterObject> lst = new List<ParameterObject>();
	            lst.Add(new ParameterObject(ParameterObject.ParameterType.Coordinates/*good*/, _daddy.GetInitialCoordinates(), "latlon",_daddy.GetTranslator().GetString("ParamCenterLatLon"),_daddy.GetTranslator().GetStringM("TooltipParamLatLon"))); 
	            
	            ParametersChanger changer = new ParametersChanger();
	            changer.HandlerDisplayCoord = _daddy.HandlerToDisplayCoordinates;
	        	changer.DisplayCoordImage = _daddy.GetImageSized("Earth");
	            changer.Title = _daddy.GetTranslator().GetString("updatepublishnotifications");
	            changer.BtnCancel = _daddy.GetTranslator().GetString("BtnCancel");
	            changer.BtnOK = _daddy.GetTranslator().GetString("BtnOk");
	            changer.ErrorFormater = _daddy.GetTranslator().GetString("ErrWrongParameter");
	            changer.ErrorTitle = _daddy.GetTranslator().GetString("Error");
	            changer.Parameters = lst;
	            changer.Font = _daddy.Font;
	            changer.Icon = _daddy.Icon;
	
	            // Force creation du get handler on control
	            changer.CreateControls();
	            _daddy._cacheDetail._gmap.ControlTextLatLon = changer.CtrlCallbackCoordinates;
	
	            if (changer.ShowDialog() == DialogResult.OK)
	            {
	                _daddy._cacheDetail._gmap.ControlTextLatLon = null;
	                Double dlon = Double.MaxValue;
	                Double dlat = Double.MaxValue;
	                if (ParameterObject.SplitLongitudeLatitude(lst[0].Value, ref dlon, ref dlat))
	                {
			            foreach(String id in ids)
		            		NotificationsManager.UpdateNotificationsImpl(_daddy, id, cookieJar,dlat, dlon);
			            
			            _daddy.MsgActionDone(this);
	                }
	            }
	            else
	            	_daddy._cacheDetail._gmap.ControlTextLatLon = null;
	            
			}
            catch (Exception ex)
            {
                _daddy.ShowException("", _daddy.GetTranslator().GetString(fct), ex);
            }
			
		}
		
		void BtnGCNGMapClick(object sender, EventArgs e)
		{
			String fct = "mappublishnotifications";
			try
			{
				if (!SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled()) return;
				
				List<GCNotification> associatedGCN;
				List<String> ids = GetSelectedIds(out associatedGCN);
				if (ids.Count == 0)
				{
					_daddy.MsgActionWarning(this, _daddy.GetTranslator().GetString("LblErrorNoSelectionElt"));
            		return;
            	}
				
				
	        	DialogResult dialogResult = MessageBox.Show(_daddy.GetTranslator().GetString("AskConfirm"),
				                                            _daddy.GetTranslator().GetString(fct),
	                            							MessageBoxButtons.YesNo, MessageBoxIcon.Question);
	            if (dialogResult == DialogResult.Yes)
	            {
				
		            NotificationsManager.MapNotificationsImpl(_daddy, ids);
	            }
			}
            catch (Exception ex)
            {
                _daddy.ShowException("", _daddy.GetTranslator().GetString(fct), ex);
            }
			
		}
	}
}
