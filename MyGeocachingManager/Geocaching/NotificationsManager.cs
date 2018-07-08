/*
 * Created by SharpDevelop.
 * User: bcristiano
 * Date: 22/07/2016
 * Time: 11:31
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Threading;
using GMap.NET;
using MyGeocachingManager;
using System.Collections.Generic;
using SpaceEyeTools.HMI;
using System.Web;
using System.Net;
using System.IO;
using SpaceEyeTools;
using System.Windows.Forms;
using System.Drawing;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms;
using MyGeocachingManager.HMI;

namespace MyGeocachingManager.Geocaching
{
	/// <summary>
	/// Description of NotificationsManager.
	/// </summary>
	public static class NotificationsManager
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="daddy"></param>
		/// <param name="post_response"></param>
		/// <param name="dlat"></param>
		/// <param name="dlon"></param>
		/// <param name="distance"></param>
		/// <param name="name"></param>
		/// <param name="tyo"></param>
		/// <param name="email"></param>
		/// <param name="checknotif"></param>
		/// <returns></returns>
		static public String GeneratePostString(MainWindow daddy, String post_response, double dlat, double dlon, int distance, String name, Tuple<int, String, List<String>> tyo, String email, bool checknotif)
        {
        	// On récupère les viewstates
            String __VIEWSTATEFIELDCOUNT = "";
            String[] __VIEWSTATE = null;
            String __VIEWSTATEGENERATOR = "";
            MainWindow.GetViewState(post_response, ref __VIEWSTATEFIELDCOUNT, ref __VIEWSTATE, ref __VIEWSTATEGENERATOR);
            	
            // Préparation des données du POST
            Dictionary<String, String> post_values = new Dictionary<String, String>();
            post_values.Add("__EVENTTARGET", "");
            post_values.Add("__EVENTARGUMENT", "");
            post_values.Add("__LASTFOCUS", "");
            
			// Le nom                 
            post_values.Add("ctl00$ContentBody$LogNotify$tbName", name);

            // Le type
            post_values.Add("ctl00$ContentBody$LogNotify$ddTypeList", tyo.Item1.ToString());

            // IL FAUT PUBLIER POUR ACTIVER LA SUITE !!!
            // *****************************************
            
			// la notif
			foreach(String n in tyo.Item3)
				post_values.Add(n, "checked");
			
			// Les coordonnées en degrées
			post_values.Add("ctl00$ContentBody$LogNotify$LatLong", "4");
			
			// Les valeurs des coordonnées
			// Le nord 
			String slat = CoordConvHMI.ConvertDegreesToDDMM(dlat, true);
			if (slat.Contains("N"))
				post_values.Add("ctl00$ContentBody$LogNotify$LatLong:_selectNorthSouth", "1");
			else
				post_values.Add("ctl00$ContentBody$LogNotify$LatLong:_selectNorthSouth", "-1");
			
			// l'est
			String slon = CoordConvHMI.ConvertDegreesToDDMM(dlon, false);
			if (slon.Contains("E"))
				post_values.Add("ctl00$ContentBody$LogNotify$LatLong:_selectEastWest", "1");
			else
				post_values.Add("ctl00$ContentBody$LogNotify$LatLong:_selectEastWest", "-1");
			
			// enfin les valeurs numériques des coordonnées
			post_values.Add("ctl00$ContentBody$LogNotify$LatLong$_inputLatDegs", (Math.Abs(dlat)).ToString().Replace(',','.'));
			post_values.Add("ctl00$ContentBody$LogNotify$LatLong$_inputLongDegs", (Math.Abs(dlon)).ToString().Replace(',','.'));
			
			// La distance en km
			post_values.Add("ctl00$ContentBody$LogNotify$tbDistance", distance.ToString());
			
			// l'email
            if (email != "")
			    post_values.Add("ctl00$ContentBody$LogNotify$ddlAltEmails", email);
			
			// activer les notifs
			if (checknotif)
				post_values.Add("ctl00$ContentBody$LogNotify$cbEnable", "checked");
			
			// Le submit
			post_values.Add("ctl00$ContentBody$LogNotify$btnGo", "submit");
			
            // Les viewstate
            post_values.Add("__VIEWSTATE", __VIEWSTATE[0]);
            if (__VIEWSTATE.Length > 1)
            {
                for (int i = 1; i < __VIEWSTATE.Length; i++)
                {
                    post_values.Add("__VIEWSTATE" + i.ToString(), __VIEWSTATE[i]);
                }
                post_values.Add("__VIEWSTATEFIELDCOUNT", __VIEWSTATE.Length.ToString());
            }
            if (__VIEWSTATEGENERATOR != "")
                post_values.Add("__VIEWSTATEGENERATOR", __VIEWSTATEGENERATOR);

            // Encodage des données du POST
            String post_string = "";
            foreach (KeyValuePair<String, String> post_value in post_values)
            {
                post_string += post_value.Key + "=" + HttpUtility.UrlEncode(post_value.Value) + "&";
            }
            post_string = post_string.TrimEnd('&');
            return post_string;
        }
        
		/// <summary>
		/// 
		/// </summary>
		/// <param name="daddy"></param>
		/// <param name="url"></param>
		/// <param name="post_string"></param>
		/// <param name="cookieJar"></param>
		/// <returns></returns>
        static public String GeneratePostRequets(MainWindow daddy, String url, String post_string, CookieContainer cookieJar)
        {
        	String post_response = "";
        	
        	// Création de la requête pour s'authentifier
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
            objRequest.Method = "POST";
            objRequest.ContentLength = post_string.Length;
            objRequest.ContentType = "application/x-www-form-urlencoded";
            objRequest.Proxy = daddy.GetProxy(); // Créer votre proxy ici si besoin, sinon mettre NULL
            objRequest.CookieContainer = cookieJar;
            
            // on envoit les POST data dans un stream (écriture)
            StreamWriter myWriter = null;
            myWriter = new StreamWriter(objRequest.GetRequestStream());
            myWriter.Write(post_string);
            myWriter.Close();

            // lecture du stream de réponse et conversion en chaine
            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
            using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
            {
                post_response = responseStream.ReadToEnd();
                responseStream.Close();
            }
            return post_response;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="daddy"></param>
        /// <param name="options"></param>
        /// <param name="imglist"></param>
        /// <param name="lsentries"></param>
        /// <param name="checkedvalues"></param>
        static public void RetrieveNotificationsList(MainWindow daddy, out List<String> options, out ImageList imglist, out List<Tuple<String, String, bool, String, String, String>> lsentries, out List<String> checkedvalues)
        {
        	options = null;
        	imglist = null;
        	lsentries = null;
        	checkedvalues = null;
        	
            daddy.UpdateHttpDefaultWebProxy();
            String post_response = "";
            // On checke que les L/MDP soient corrects
            // Et on récupère les cookies au passage
            CookieContainer cookieJar = daddy.CheckGCAccount(true, false);
            if (cookieJar == null)
                return;
            
            // Pour récupérer les notifications
            String url = "https://www.geocaching.com/notify/default.aspx";
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
            objRequest.Proxy = daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
            objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
            using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
            {
                post_response = responseStream.ReadToEnd();
                responseStream.Close();
            }
            
            imglist = new ImageList();
            imglist.ColorDepth = ColorDepth.Depth32Bit;
            imglist.ImageSize = new Size(32, 32); // this will affect the row height
            
            // On parse
            // On récupère la tables des notifications
            // Tuple :
            // Label, id, check, Name, Type, notifs
            lsentries = new List<Tuple<String, String, bool, String, String, String>>();
            checkedvalues = new List<string>();
            String table = MyTools.GetSnippetFromText("<table class=\"Table\">", "</table>", post_response);
            List<String> entries = MyTools.GetSnippetsFromText("<tr", "</tr>", table);
            options = new List<string>();
            foreach(String entry in entries)
            {
            	String name = MyTools.GetSnippetFromText("<strong>", "</strong>", entry);
            	String id = MyTools.GetSnippetFromText("edit.aspx?NID=", "\"", entry);
            	String notif = MyTools.GetSnippetFromText("<br />","\n", entry);
            	int pos = notif.IndexOf(":");
            	if (pos != -1)
            		notif = notif.Substring(pos + 2);
            	String type = MyTools.GetSnippetFromText(".gif\" alt=\"","\"", entry);
            	
            	String lbl = "[" + name + "] " + type + ", " + notif;
            	
            	// Checked ?
            	bool check = false;
            	if (entry.Contains("checkbox_on.png\" alt=\"Checked\""))
            	{
            		check = true;
            		checkedvalues.Add(lbl);
            	}
            	
            	lsentries.Add(new Tuple<string, string, bool, String, String, String>(lbl, id, check, name, type, notif));
            	
            	options.Add(lbl);
            	if (imglist.Images.ContainsKey(lbl) == false) // Au cas où on ait des doublons...
            		imglist.Images.Add(lbl, daddy.GetImageSized(type));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="daddy"></param>
        static public void DeleteNotifications(MainWindow daddy)
        {
            // **** BLOC TO RETRIEVE NOTIFICATIONS LIST ***
            if (!SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled()) return;
        	List<String> options;
        	ImageList imglist;
        	List<Tuple<String, String, bool, String, String, String>> lsentries;
        	List<String> checkedvalues;
        	CookieContainer cookieJar = daddy.CheckGCAccount(true, false);
            if (cookieJar == null)
                return;
        	NotificationsManager.RetrieveNotificationsList(daddy, out options, out imglist, out lsentries, out checkedvalues);
        	// **** END BLOC TO RETRIEVE NOTIFICATIONS LIST ***
        	
            List<ParameterObject> lst = new List<ParameterObject>();
                        
            ParameterObject po = new ParameterObject(ParameterObject.ParameterType.CheckList, options, "notifs", daddy.GetTranslator().GetString("FTFName"));
            po.ImagesForCheckList = imglist;
			lst.Add(po);
			
            ParametersChanger changer = new ParametersChanger();
            changer.HandlerDisplayCoord = daddy.HandlerToDisplayCoordinates;
        	changer.DisplayCoordImage = daddy.GetImageSized("Earth");
            changer.Title = daddy.GetTranslator().GetString("deletepublishnotifications");
            changer.BtnCancel = daddy.GetTranslator().GetString("BtnCancel");
            changer.BtnOK = daddy.GetTranslator().GetString("BtnOk");
            changer.ErrorFormater = daddy.GetTranslator().GetString("ErrWrongParameter");
            changer.ErrorTitle = daddy.GetTranslator().GetString("Error");
            changer.Parameters = lst;
            changer.Font = daddy.Font;
            changer.Icon = daddy.Icon;

            if (changer.ShowDialog() == DialogResult.OK)
            {
                String stypes = changer.Parameters[0].Value;
                String[] ltypes = stypes.Split('\n');
            	
            	foreach(String atype in ltypes)
            	{
            		if (atype == "")
            			continue;
            		
            		// on trouve les bonnes infos
            		Tuple<String, String, bool, String, String, String> tyo = null;
            		foreach(Tuple<String, String, bool, String, String, String> o in lsentries)
            		{
            			if (o.Item1 == atype)
            			{
            				tyo = o;
            			}
            		}
            		
            		DeleteNotificationsImpl(daddy, tyo.Item2, cookieJar);
            		
            	}
            	
            	daddy.MsgActionDone(daddy);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="daddy"></param>
        /// <param name="id"></param>
        /// <param name="cookieJar"></param>
        static public void DeleteNotificationsImpl(MainWindow daddy, String id, CookieContainer cookieJar)
        {
        	// on attaque la suppression
        	String post_response;
    		String url = "https://www.geocaching.com/notify/edit.aspx?NID=" +id;
    		HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
            objRequest.Proxy = daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
            objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
            using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
            {
                post_response = responseStream.ReadToEnd();
                responseStream.Close();
            }
            
            // On récupère les viewstates
            String __VIEWSTATEFIELDCOUNT = "";
            String[] __VIEWSTATE = null;
            String __VIEWSTATEGENERATOR = "";
            MainWindow.GetViewState(post_response, ref __VIEWSTATEFIELDCOUNT, ref __VIEWSTATE, ref __VIEWSTATEGENERATOR);
            	
            // Préparation des données du POST
            Dictionary<String, String> post_values = new Dictionary<String, String>();
            post_values.Add("__EVENTTARGET", "");
            post_values.Add("__EVENTARGUMENT", "");
            post_values.Add("__LASTFOCUS", "");
           
			// Le submit
			post_values.Add("ctl00$ContentBody$LogNotify$btnArchive", "Delete Notification");
			
            // Les viewstate
            post_values.Add("__VIEWSTATE", __VIEWSTATE[0]);
            if (__VIEWSTATE.Length > 1)
            {
                for (int i = 1; i < __VIEWSTATE.Length; i++)
                {
                    post_values.Add("__VIEWSTATE" + i.ToString(), __VIEWSTATE[i]);
                }
                post_values.Add("__VIEWSTATEFIELDCOUNT", __VIEWSTATE.Length.ToString());
            }
            if (__VIEWSTATEGENERATOR != "")
                post_values.Add("__VIEWSTATEGENERATOR", __VIEWSTATEGENERATOR);

            // Encodage des données du POST
            String post_string = "";
            foreach (KeyValuePair<String, String> post_value in post_values)
            {
                post_string += post_value.Key + "=" + HttpUtility.UrlEncode(post_value.Value) + "&";
            }
            post_string = post_string.TrimEnd('&');
            
            // Création de la requête pour s'authentifier
            objRequest = (HttpWebRequest)WebRequest.Create(url);
            objRequest.Method = "POST";
            objRequest.ContentLength = post_string.Length;
            objRequest.ContentType = "application/x-www-form-urlencoded";
            objRequest.Proxy = daddy.GetProxy(); // Créer votre proxy ici si besoin, sinon mettre NULL
            objRequest.CookieContainer = cookieJar;
            
            // on envoit les POST data dans un stream (écriture)
            StreamWriter myWriter = null;
            myWriter = new StreamWriter(objRequest.GetRequestStream());
            myWriter.Write(post_string);
            myWriter.Close();

            // lecture du stream de réponse et conversion en chaine
            objResponse = (HttpWebResponse)objRequest.GetResponse();
            using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
            {
                post_response = responseStream.ReadToEnd();
                responseStream.Close();
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="daddy"></param>
        static public void ToggleNotifications(MainWindow daddy)
        {
            // **** BLOC TO RETRIEVE NOTIFICATIONS LIST ***
            if (!SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled()) return;
        	List<String> options;
        	ImageList imglist;
        	List<Tuple<String, String, bool, String, String, String>> lsentries;
        	List<String> checkedvalues;
        	CookieContainer cookieJar = daddy.CheckGCAccount(true, false);
            if (cookieJar == null)
                return;
        	NotificationsManager.RetrieveNotificationsList(daddy, out options, out imglist, out lsentries, out checkedvalues);
        	// **** END BLOC TO RETRIEVE NOTIFICATIONS LIST ***
        	
            List<ParameterObject> lst = new List<ParameterObject>();
                        
            ParameterObject po = new ParameterObject(ParameterObject.ParameterType.CheckList, options, "notifs", daddy.GetTranslator().GetString("FTFName"));
            po.ImagesForCheckList = imglist;
            po.ListCheckedValue = checkedvalues;
			lst.Add(po);
			
            ParametersChanger changer = new ParametersChanger();
            changer.HandlerDisplayCoord = daddy.HandlerToDisplayCoordinates;
        	changer.DisplayCoordImage = daddy.GetImageSized("Earth");
            changer.Title = daddy.GetTranslator().GetString("togglepublishnotifications");
            changer.BtnCancel = daddy.GetTranslator().GetString("BtnCancel");
            changer.BtnOK = daddy.GetTranslator().GetString("BtnOk");
            changer.ErrorFormater = daddy.GetTranslator().GetString("ErrWrongParameter");
            changer.ErrorTitle = daddy.GetTranslator().GetString("Error");
            changer.Parameters = lst;
            changer.Font = daddy.Font;
            changer.Icon = daddy.Icon;

            if (changer.ShowDialog() == DialogResult.OK)
            {
                String stypes = changer.Parameters[0].Value;
                String[] ltypes = stypes.Split('\n');
                List<String> checkactuels = new List<string>();
                foreach(String c in ltypes)
                	checkactuels.Add(c);
                
                // Là on n'a que les types sélectionnés : ltypes
                // Donc on va parcourir ce qu'on avait avant
                // si on a un changement sur la valeur de check, on toggle
                // On parcourt la liste initiale
                foreach(Tuple<String, String, bool, String, String, String> o in lsentries)
                {
                	bool oldCheck = o.Item3;
                	bool newCheck = checkactuels.Contains(o.Item1);
                	if (oldCheck != newCheck)
                	{
                		// On doit toggle
                		ToggleNotificationsImpl(daddy, o.Item2, cookieJar);
                	}
                }
                daddy.MsgActionDone(daddy);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="daddy"></param>
        /// <param name="id"></param>
        /// <param name="cookieJar"></param>
        static public void ToggleNotificationsImpl(MainWindow daddy, string id, CookieContainer cookieJar)
        {
        	// Pour basculer le toggle sur une notification, il suffit
			// d'appeler https://www.geocaching.com/notify/default.aspx?did=<ID>
			// ça bascule l'état de notification tout simplement
			String url = "https://www.geocaching.com/notify/default.aspx?did=" + id;
			HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
            objRequest.Proxy = daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
            objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
            using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
            {
                String post_response = responseStream.ReadToEnd();
                responseStream.Close();
            }
        }
               
        /// <summary>
        /// 
        /// </summary>
        /// <param name="daddy"></param>
        /// <param name="id"></param>
        /// <param name="cookieJar"></param>
        /// <param name="dlat"></param>
        /// <param name="dlon"></param>
        static public void UpdateNotificationsImpl(MainWindow daddy, String id, CookieContainer cookieJar, double dlat, double dlon)
        {
        	// Récupérer ses infos et mettre à jour
        	String post_response = "";
    		GCNotification gcn = GetNotificationData(daddy, id, ref post_response);
    		if (gcn != null)
    		{
    			// On va poster la mise à jour			                
        		// Une mise à jour pour définir les modifications de coordonnées
        		String post_string = GeneratePostString(daddy, post_response, dlat, dlon, gcn.distance, gcn.name, gcn.data, gcn.email, gcn.checknotif);
        		String url = "https://www.geocaching.com/notify/edit.aspx?NID=" + id;
        		
        		// Et on poste tout ça
    			post_response = GeneratePostRequets(daddy, url, post_string, cookieJar);
    		}
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="daddy"></param>
        /// <param name="id"></param>
        /// <param name="post_response"></param>
        /// <returns></returns>
        static public GCNotification GetNotificationData(MainWindow daddy, String id, ref String post_response)
        {
        	GCNotification gcn = null;
        	
        	try
        	{
        		gcn = new GCNotification();
        		gcn.id = id;
        		
        		daddy.UpdateHttpDefaultWebProxy();
	            // On checke que les L/MDP soient corrects
	            // Et on récupère les cookies au passage
	            CookieContainer cookieJar = daddy.CheckGCAccount(true, false);
	            if (cookieJar == null)
	                return null;
	            
				// Pour récupérer les notifications
				String url = "https://www.geocaching.com/notify/edit.aspx?NID=" + id;
	            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
	            objRequest.Proxy = daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
	            objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
	            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
	            using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
	            {
	                post_response = responseStream.ReadToEnd();
	                responseStream.Close();
	            }
	            
	            // On va parser les informations
	            // Le nom
	            String txt = MyTools.GetSnippetFromText("ctl00$ContentBody$LogNotify$tbName\" type=\"text\" value=\"", "\" id=\"", post_response);
	            gcn.name = txt;
	            
	            // Le type id + nom
	            txt = MyTools.GetSnippetFromText("ctl00$ContentBody$LogNotify$ddTypeList", "/select", post_response);
	            txt = MyTools.GetSnippetFromText("<option selected=\"selected\"", "/option", txt); // ==>  value="2">Traditional Cache<
	            String sid = MyTools.GetSnippetFromText(" value=\"","\">", txt);
	            int typeid = Int32.Parse(sid);
	            String type = MyTools.GetSnippetFromText(">","<", txt);
	            
	            // Les types de notifications
	            List<String> typepublishchecked = new List<string>();
	            List<String> typepublishcheckedname = new List<string>();
	            txt = MyTools.GetSnippetFromText("ctl00_ContentBody_LogNotify_cblLogTypeList","/table", post_response);
	            List<String> entries = MyTools.GetSnippetsFromText("<tr>","</tr>", txt);
	            foreach(String entry in entries)
	            {
	            	if (entry.Contains("checked=\"checked\""))
	            	{
	            		// Elle est checked
	            		String kindofnotifpost = MyTools.GetSnippetFromText("name=\"","\"",entry);
	            		typepublishchecked.Add(kindofnotifpost);
	            		
	            		// On cherche le vrai nom
	            		// L'id du kind of notif ?
	            		String kindofnotifid = kindofnotifpost.Replace("ctl00$ContentBody$LogNotify$cblLogTypeList$","");
	            		String kindofnotifname = MyTools.GetSnippetFromText("ctl00_ContentBody_LogNotify_cblLogTypeList_" + kindofnotifid + "\">", "</label>",entry);
	            		typepublishcheckedname.Add(kindofnotifname);
	            	}
	            }
	            
	            // on construit les datas
	            // typeid, type
	            gcn.data = new Tuple<int, string, List<string>> (typeid, type, typepublishchecked);
	            gcn.kindofnotifnames = typepublishcheckedname;
	            
	            // la distance
	            txt = MyTools.GetSnippetFromText("ctl00$ContentBody$LogNotify$tbDistance\" type=\"text\" value=\"", "\" id=", post_response);
	            gcn.distance = Int32.Parse(txt);
	            
	            // L'email
	            txt = MyTools.GetSnippetFromText("ctl00$ContentBody$LogNotify$ddlAltEmails", "/select", post_response);
	            txt = MyTools.GetSnippetFromText("selected=\"selected\" value=\"", "\">", txt);
	            gcn.email = txt;
	            
	            // Notification checkée ?
	            gcn.checknotif = post_response.Contains("name=\"ctl00$ContentBody$LogNotify$cbEnable\" checked=\"checked\"");
	            
	            
	            // Et les coordonnées
	            // Nord ou sud ?
	            txt = MyTools.GetSnippetFromText("ctl00$ContentBody$LogNotify$LatLong:_selectNorthSouth", "/select", post_response);
	            txt = MyTools.GetSnippetFromText("selected=\"selected\" value=\"", "\">", txt);
	            String nordsud = "";
	            if (txt == "1")
	            	nordsud = "N";
	            else
	            	nordsud = "S";
	            
	            // degres ns
	            String degns = MyTools.GetSnippetFromText("ctl00$ContentBody$LogNotify$LatLong$_inputLatDegs\" type=\"text\" value=\"", "\" maxlength", post_response);
	            
	            // min ns
	            String minns = MyTools.GetSnippetFromText("ctl00$ContentBody$LogNotify$LatLong$_inputLatMins\" type=\"text\" value=\"", "\" maxlength", post_response);
	            
	            // Est ou ouest ?
	            txt = MyTools.GetSnippetFromText("ctl00$ContentBody$LogNotify$LatLong:_selectEastWest", "/select", post_response);
	            txt = MyTools.GetSnippetFromText("selected=\"selected\" value=\"", "\">", txt);
	            String estouest = "";
	            if (txt == "1")
	            	estouest = "E";
	            else
	            	estouest = "W";
	            
	            // degres ew
	            String degew = MyTools.GetSnippetFromText("ctl00$ContentBody$LogNotify$LatLong$_inputLongDegs\" type=\"text\" value=\"", "\" maxlength", post_response);
	            
	            // min ew
	            String minew = MyTools.GetSnippetFromText("ctl00$ContentBody$LogNotify$LatLong$_inputLongMins\" type=\"text\" value=\"", "\" maxlength", post_response);
	            
	            // Coordinates format :
	            // N 48° 46.164 E 01° 58.048
	            String coords = nordsud + " " + degns + "° " + minns + " " + estouest + " " + degew + "° " + minew;
	            String sLat = "";
	            String sLon= "";
	            bool bOK = ParameterObject.TryToConvertCoordinates(coords, ref sLat, ref sLon);
                if (sLat != CoordConvHMI._sErrorValue)
                    gcn.dlat = MyTools.ConvertToDouble(sLat);
                else
                	throw new Exception();
                if (sLon != CoordConvHMI._sErrorValue)
                    gcn.dlon = MyTools.ConvertToDouble(sLon);
                else
                	throw new Exception();
                
        	}
        	catch(Exception)
        	{
        		gcn = null;
        	}
        	
        	return gcn;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="daddy"></param>
        static public void MapNotifications(MainWindow daddy)
        {
        	// **** BLOC TO RETRIEVE NOTIFICATIONS LIST ***
            if (!SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled()) return;
        	List<String> options;
        	ImageList imglist;
        	List<Tuple<String, String, bool, String, String, String>> lsentries;
        	List<String> checkedvalues;
        	CookieContainer cookieJar = daddy.CheckGCAccount(true, false);
            if (cookieJar == null)
                return;
        	NotificationsManager.RetrieveNotificationsList(daddy, out options, out imglist, out lsentries, out checkedvalues);
        	// **** END BLOC TO RETRIEVE NOTIFICATIONS LIST ***
        	
            List<ParameterObject> lst = new List<ParameterObject>();
            
            ParameterObject po = new ParameterObject(ParameterObject.ParameterType.CheckList, options, "notifs", daddy.GetTranslator().GetString("FTFName"));
            po.ImagesForCheckList = imglist;
			lst.Add(po);
			
            ParametersChanger changer = new ParametersChanger();
            changer.HandlerDisplayCoord = daddy.HandlerToDisplayCoordinates;
        	changer.DisplayCoordImage = daddy.GetImageSized("Earth");
            changer.Title = daddy.GetTranslator().GetString("mappublishnotifications");
            changer.BtnCancel = daddy.GetTranslator().GetString("BtnCancel");
            changer.BtnOK = daddy.GetTranslator().GetString("BtnOk");
            changer.ErrorFormater = daddy.GetTranslator().GetString("ErrWrongParameter");
            changer.ErrorTitle = daddy.GetTranslator().GetString("Error");
            changer.Parameters = lst;
            changer.Font = daddy.Font;
            changer.Icon = daddy.Icon;

            
            // Force creation du get handler on control
            changer.CreateControls();
            daddy._cacheDetail._gmap.ControlTextLatLon = changer.CtrlCallbackCoordinates;

            if (changer.ShowDialog() == DialogResult.OK)
            {
                daddy._cacheDetail._gmap.ControlTextLatLon = null;
                
            	String stypes = changer.Parameters[0].Value;
            	String[] ltypes = stypes.Split('\n');
                
            	List<String> ids = new List<string>();
                foreach(String c in ltypes)
                {
               	
	                // Là on n'a que les types sélectionnés : c
	                foreach(Tuple<String, String, bool, String, String, String> o in lsentries)
	                {
	                	if (o.Item1 == c)
	                	{
	                		// Récupérer ses infos
	                		ids.Add(o.Item2);
	                	}
	                }
                }
                
                MapNotificationsImpl(daddy, ids);
                
            }
            else
            	daddy._cacheDetail._gmap.ControlTextLatLon = null;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="daddy"></param>
        /// <param name="ids"></param>
        static public void MapNotificationsImpl(MainWindow daddy, List<String> ids)
        {
        	// Le dico
        	String post_response = "";
            Dictionary<String, List<GCNotification>> diconotifs = new Dictionary<string, List<GCNotification>>();
	         foreach(String id in ids)
	         {    
        		// Récupérer ses infos
        		GCNotification gcn = GetNotificationData(daddy, id, ref post_response);
        		if (gcn != null)
        		{
        			// On empile les notifications ayant les même coordonnées
        			// La clé du dico : lat+lon
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
	         }
	            
	            // on affiche
	            daddy.ClearOverlay_RESERVED2();
	            foreach(KeyValuePair<String, List<GCNotification>> pair in diconotifs)
	            {
	            	// on va créer les markers + circulaires
	            	// si la liste a une seule notif, on prend son type et sa couleur
	            	// sinon on fout tout en pink
	            	GMarkerGoogleType gtype = GMarkerGoogleType.pink_pushpin;
	            	Color c = Color.Pink;
	            	GCNotification first = pair.Value[0];
	            	String tip = "";
	            	PointLatLng pt = new PointLatLng(first.dlat,first.dlon);
	            	if (pair.Value.Count == 1)
	            	{
	            		first.GetIcon(ref gtype, ref c);
	            	}
	            	foreach(GCNotification gn in pair.Value)
	            	{
	            		tip += gn.name + " " + gn.GetTypeKeyInEnglish() + "\r\n";
	            	}
	            	
	            	// on crée le marker
	            	GMapMarker marker = new GMarkerGoogle(pt, gtype);
	            	marker.ToolTipText = tip;
	            	marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
	            	daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2].Markers.Add(marker);
	            	
	            	// Le disque
	            	c = Color.FromArgb(60, c.R, c.G, c.B);
	            	Brush brush = new SolidBrush(c);
	        		Pen pen = new Pen(c, 2);
	        		GMapMarkerCircle circle = new GMapMarkerCircle( daddy._cacheDetail._gmap, pt, first.distance * 1000, pen, brush, true);
	        		daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2].Markers.Add(circle);
	        		
	            }
			            
				daddy.ShowCacheMapInCacheDetail();
				daddy._cacheDetail._gmap.ZoomAndCenterCircles(daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2].Id);         
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="daddy"></param>
        static public void CreateNotifications(MainWindow daddy)
        {
            if (!SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled()) return;

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
            // On regarde si on a un message de warning
    		if (CheckWarningMessage(daddy, post_response))
    		{
    			// Shit
    			return;
    		}
    		
            List<String> lsemails = new List<string>();
            String email = "";
	        String mails = MyTools.GetSnippetFromText("ctl00$ContentBody$LogNotify$ddlAltEmails","select>",post_response);
	        lsemails = MyTools.GetSnippetsFromText("value=\"", "\">", mails);
	        if (lsemails.Count != 0)
	        	email = lsemails[0];
	                    	
        	// Les types autorisés
        	List<Tuple<int, string, List<String>>> types = new List<Tuple<int, string, List<String>>>
        	{
        		Tuple.Create(2, "Traditional Cache", new List<string>(new string[] { "ctl00$ContentBody$LogNotify$cblLogTypeList$6"})),
				Tuple.Create(3, "Multi-cache", new List<string>(new string[] { "ctl00$ContentBody$LogNotify$cblLogTypeList$6"})),
				Tuple.Create(8, "Unknown Cache", new List<string>(new string[] { "ctl00$ContentBody$LogNotify$cblLogTypeList$6"})),
				Tuple.Create(137, "Earthcache", new List<string>(new string[] { "ctl00$ContentBody$LogNotify$cblLogTypeList$6"})),
				Tuple.Create(5, "Letterbox Hybrid", new List<string>(new string[] { "ctl00$ContentBody$LogNotify$cblLogTypeList$6"})),
				Tuple.Create(1858, "Wherigo Cache", new List<string>(new string[] { "ctl00$ContentBody$LogNotify$cblLogTypeList$6"})),
				Tuple.Create(4, "Virtual Cache", new List<string>(new string[] { "ctl00$ContentBody$LogNotify$cblLogTypeList$6"})), // BCR 20170825
				
				Tuple.Create(6, "Event Cache", new List<string>(new string[] { "ctl00$ContentBody$LogNotify$cblLogTypeList$7"})),
				Tuple.Create(13, "Cache In Trash Out Event", new List<string>(new string[] { "ctl00$ContentBody$LogNotify$cblLogTypeList$7"})),
            	Tuple.Create(7005, "Giga-Event Cache", new List<string>(new string[] { "ctl00$ContentBody$LogNotify$cblLogTypeList$7"})),
            	Tuple.Create(453, "Mega-Event Cache", new List<string>(new string[] { "ctl00$ContentBody$LogNotify$cblLogTypeList$7"}))
            		
				//Tuple.Create(4738, "Groundspeak Block Party"),
				//Tuple.Create(3774, "Groundspeak Lost and Found Celebration"),
				//Tuple.Create(3653, "Lost and Found Event Cache"),
				//Tuple.Create(1304, "GPS Adventures Exhibit"),
				//Tuple.Create(12, "Locationless (Reverse) Cache"),
				//Tuple.Create(9, "Project APE Cache"),
				//Tuple.Create(3773, "Groundspeak HQ"),
				//Tuple.Create(4, "Virtual Cache"),
				//Tuple.Create(11, "Webcam Cache"),
				
        	};
        	List<ParameterObject> lst = new List<ParameterObject>();
            List<String> lsttypes = new List<string>();
            ImageList imglist = new ImageList();
            imglist.ColorDepth = ColorDepth.Depth32Bit;
            imglist.ImageSize = new Size(32, 32); // this will affect the row height
            foreach(var o in types)
            {
            	lsttypes.Add(o.Item2);
            	imglist.Images.Add(o.Item2, daddy.GetImageSized(o.Item2));
            }
                
            String defname = "MGMN";
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Coordinates/*good*/, daddy.GetInitialCoordinates(), "latlon",daddy.GetTranslator().GetString("ParamCenterLatLon"),daddy.GetTranslator().GetStringM("TooltipParamLatLon")));
            ParameterObject po = new ParameterObject(ParameterObject.ParameterType.CheckList, lsttypes, "types", daddy.GetTranslator().GetString("FTFCacheTypes"));
            po.ImagesForCheckList = imglist;
			lst.Add(po);
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Int, 20, "dist", daddy.GetTranslator().GetString("FTFDistance")));
            lst.Add(new ParameterObject(ParameterObject.ParameterType.String, defname, "notif", daddy.GetTranslator().GetString("FTFName")));
            if (email != "")
                lst.Add(new ParameterObject(ParameterObject.ParameterType.List, lsemails, "emails", daddy.GetTranslator().GetString("FTFEmails")));
            
            ParametersChanger changer = new ParametersChanger();
            changer.HandlerDisplayCoord = daddy.HandlerToDisplayCoordinates;
        	changer.DisplayCoordImage = daddy.GetImageSized("Earth");
            changer.Title = daddy.GetTranslator().GetString("createpublishnotifications");
            changer.BtnCancel = daddy.GetTranslator().GetString("BtnCancel");
            changer.BtnOK = daddy.GetTranslator().GetString("BtnOk");
            changer.ErrorFormater = daddy.GetTranslator().GetString("ErrWrongParameter");
            changer.ErrorTitle = daddy.GetTranslator().GetString("Error");
            changer.Parameters = lst;
            changer.Font = daddy.Font;
            changer.Icon = daddy.Icon;

            
            // Force creation du get handler on control
            changer.CreateControls();
            daddy._cacheDetail._gmap.ControlTextLatLon = changer.CtrlCallbackCoordinates;

            if (changer.ShowDialog() == DialogResult.OK)
            {
                daddy._cacheDetail._gmap.ControlTextLatLon = null;
                Double dlon = Double.MaxValue;
                Double dlat = Double.MaxValue;
                if (ParameterObject.SplitLongitudeLatitude(lst[0].Value, ref dlon, ref dlat))
                {
                	String stypes = changer.Parameters[1].Value;
                	String[] ltypes = stypes.Split('\n');
                	int distance = Int32.Parse(changer.Parameters[2].Value);
                	String name = changer.Parameters[3].Value;
                    if (email != "")
                        email = changer.Parameters[4].Value;
                	if (name == "")
                		name = defname;
                	String post_string = "";
                	
                	bool error = false;
                	foreach(String atype in ltypes)
                	{
                		if (atype == "")
                			continue;

                		// on trouve les bonnes infos
                		Tuple<int, String, List<String>> tyo = null;
                		foreach(Tuple<int, String, List<String>> o in types)
                		{
                			if (o.Item2 == atype)
                			{
                				tyo = o;
                			}
                		}
                    	
                		// On demande la page par défaut pour initialiser une nouvelle demande
                		objRequest = (HttpWebRequest)WebRequest.Create(url);
		                objRequest.Proxy = daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
		                objRequest.CookieContainer = cookieJar; // surtout récupérer le container de cookie qui est maintenant renseigné avec le cookie d'authentification
		                objResponse = (HttpWebResponse)objRequest.GetResponse();
		                using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
		                {
		                    post_response = responseStream.ReadToEnd();
		                    responseStream.Close();
		                }
		                // On regarde si on a un message de warning
                		if (CheckWarningMessage(daddy, post_response))
                		{
                			// Shit
                			error = true;
                			break;
                		}
                		
                		// Une mise à jour pour définir le type de cache
                		post_string = GeneratePostString(daddy, post_response, dlat, dlon, distance, name, tyo, email, true);
                		post_response = GeneratePostRequets(daddy, url, post_string, cookieJar);                		
                		
                		// Une mise à jour pour définir le type de notif
                		post_string = GeneratePostString(daddy, post_response, dlat, dlon, distance, name, tyo, email, true);
                		post_response = GeneratePostRequets(daddy, url, post_string, cookieJar);
                		if (CheckValidationMessage(daddy, post_response))
                		{
                			// Shit
                			error = true;
                			break;
                		}
                		
	                    //_cacheDetail.LoadPageText("dbg1", post_response, true);
                	}
                	if (!error)
                	{
                		daddy.MsgActionDone(daddy);
                	}
                }                   
            }
            else
            	daddy._cacheDetail._gmap.ControlTextLatLon = null;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="daddy"></param>
        /// <param name="html"></param>
        /// <returns></returns>
        static public bool CheckValidationMessage(MainWindow daddy, String html)
        {
        	// On regarde si on a un message de warning
    		String warning = MyTools.GetSnippetFromText("ctl00_ContentBody_LogNotify_ValidationSummary1", "</div>", html);
    		warning = MyTools.GetSnippetFromText("<ul>", "</ul>", warning);
    		if (warning != "")
    		{
    			// Shit
    			warning = MyTools.StripHtmlTags(warning);
    			daddy.MsgActionError(daddy, warning);
    			return true;
    		}
    		else
    			return false;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="daddy"></param>
        /// <param name="html"></param>
        /// <returns></returns>
        static public bool CheckWarningMessage(MainWindow daddy, String html)
        {
        	// On regarde si on a un message de warning
    		String warning = MyTools.GetSnippetFromText("<p class=\"Warning\">", "</p>", html);
    		if (warning != "")
    		{
    			// Shit
    			daddy.MsgActionError(daddy, warning);
    			return true;
    		}
    		else
    			return false;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="daddy"></param>
        static public void ListNotifsGroup(MainWindow daddy)
        {
        	 if (!SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled()) return;
        	 
        	NotificationGroup ng = new NotificationGroup(daddy);
        	ng.ShowDialog();
        }
	}
	
	/// <summary>
	/// 
	/// </summary>
	public class GCNotification
	{
		/// <summary>
		/// 
		/// </summary>
		public string id = "";
		/// <summary>
		/// 
		/// </summary>
		public double dlat = Double.MaxValue;
		/// <summary>
		/// 
		/// </summary>
		public double dlon = Double.MaxValue;
		/// <summary>
		/// 
		/// </summary>
		public int distance = int.MaxValue;
		/// <summary>
		/// 
		/// </summary>
		public String name = "";
		/// <summary>
		/// 
		/// </summary>
		public String email = "";
		/// <summary>
		/// 
		/// </summary>
		public Tuple<int, String, List<String>> data = null;
		/// <summary>
		/// 
		/// </summary>
		public List<String> kindofnotifnames = null;
		/// <summary>
		/// 
		/// </summary>
		public bool checknotif = false;
		/// <summary>
		/// 
		/// </summary>
		public bool Tag = false;
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("[GCNotification Dlat={0}, Dlon={1}, Distance={2}, Name={3}, Email={4}, Checknotif={5}, Data={6}]", dlat, dlon, distance, name, email, checknotif, data);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public String GetTypeKeyInEnglish()
		{
			if (data.Item1 == 137) return "Earthcache";
			if (data.Item1 == 6) return "Event Cache";
			if (data.Item1 == 13) return "Cache In Trash Out Event";
			if (data.Item1 == 7005) return "Giga-Event Cache";
			if (data.Item1 == 4738) return "Groundspeak Block Party";
			if (data.Item1 == 3774) return "Groundspeak Lost and Found Celebration";
			if (data.Item1 == 3653) return "Lost and Found Event Cache";
			if (data.Item1 == 453) return "Mega-Event Cache";
			if (data.Item1 == 1304) return "GPS Adventures Exhibit";
			if (data.Item1 == 5) return "Letterbox Hybrid";
			if (data.Item1 == 12) return "Locationless (Reverse) Cache";
			if (data.Item1 == 3) return "Multi-cache";
			if (data.Item1 == 9) return "Project APE Cache";
			if (data.Item1 == 2) return "Traditional Cache";
			if (data.Item1 == 8) return "Unknown Cache";
			if (data.Item1 == 3773) return "Groundspeak HQ";
			if (data.Item1 == 4) return "Virtual Cache";
			if (data.Item1 == 11) return "Webcam Cache";
			if (data.Item1 == 1858) return "Wherigo Cache";	
			return "";
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="gtype"></param>
		/// <param name="c"></param>
		public void GetIcon(ref GMarkerGoogleType gtype, ref Color c)
        {
			// Se baser sur item1 !
			/*
		<option value="137">Earthcache</option>
		<option value="6">Event Cache</option>
		<option value="13">Cache In Trash Out Event</option>
		<option value="7005">Giga-Event Cache</option>
		<option value="4738">Groundspeak Block Party</option>
		<option value="3774">Groundspeak Lost and Found Celebration</option>
		<option value="3653">Lost and Found Event Cache</option>
		<option value="453">Mega-Event Cache</option>
		<option value="1304">GPS Adventures Exhibit</option>
		<option value="5">Letterbox Hybrid</option>
		<option value="12">Locationless (Reverse) Cache</option>
		<option value="3">Multi-cache</option>
		<option value="9">Project APE Cache</option>
		<option value="2">Traditional Cache</option>
		<option value="8">Unknown Cache</option>
		<option value="3773">Groundspeak HQ</option>
		<option value="4">Virtual Cache</option>
		<option value="11">Webcam Cache</option>
		<option value="1858">Wherigo Cache</option>			
			*/
            if (data.Item1 == 137)
            {
            	gtype = GMarkerGoogleType.purple_pushpin;
            	c = Color.Purple;
            }
            else if (data.Item1 == 5)
            {
            	gtype = GMarkerGoogleType.green_pushpin;
            	c = Color.Green;
            }
            else if (data.Item1 == 3)
            {
            	gtype = GMarkerGoogleType.yellow_pushpin;
            	c = Color.Yellow;
            }
            else if (data.Item1 == 8)
            {
            	gtype = GMarkerGoogleType.blue_pushpin;
            	c = Color.Blue;
            }
            else if (data.Item1 == 2)
            {
            	gtype = GMarkerGoogleType.green_pushpin;
            	c = Color.Green;
            }
            else if (data.Item1 == 4)
            {
            	gtype = GMarkerGoogleType.purple_pushpin;
            	c = Color.Purple;
            }
            else if (data.Item1 == 11)
            {
            	gtype = GMarkerGoogleType.purple_pushpin;
            	c = Color.Purple;
            }
            else if (data.Item1 == 1858)
            {
            	gtype = GMarkerGoogleType.blue_pushpin;
            	c = Color.Blue;
            }
            else
            {
            	// Tout le reste c'est du type event
            	gtype = GMarkerGoogleType.red_pushpin;
            	c = Color.Red;
            }
        }
	}
			
}
