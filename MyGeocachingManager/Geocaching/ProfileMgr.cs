/*
 * Created by SharpDevelop.
 * User: bcristiano
 * Date: 12/01/2016
 * Time: 11:19
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using SpaceEyeTools;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Forms;

namespace MyGeocachingManager.Geocaching
{
	/// <summary>
	/// Class to handle MGM profiles (user specific information)
	/// </summary>
	public class ProfileMgr
	{
		const string _dataFile = "Profiles.dat";
		MainWindow _daddy = null;
		
		Dictionary<String, UserInfo> _dicoProfile = new Dictionary<string, UserInfo>();
		
	
		/// <summary>
		/// Default
		/// </summary>
		/// <param name="daddy">Mainwindow</param>
		public ProfileMgr(MainWindow daddy)
		{
			_daddy = daddy;
		}
		
		/// <summary>
		/// Return list of profiles name
		/// </summary>
		/// <returns></returns>
		public List<String> GetListOfProfiles()
		{
			List<String> p = new List<string>();
			foreach(KeyValuePair<String, UserInfo> paire in _dicoProfile)
        	{
				p.Add(paire.Key);
			}
			return p;
		}
		
		/// <summary>
		/// Update MGM conf based on profile name
		/// </summary>
		/// <param name="name">profile name</param>
		/// <param name="daddy">main windows</param>
		/// <returns>true if profile found</returns>
		public bool UpdateMGMFromProfile(MainWindow daddy, String name)
		{
			UserInfo ui = null;
			if (_dicoProfile.ContainsKey(name))
			{
				ui = _dicoProfile[name];
				daddy.UpdateConfFile("owner",ui._owner);
				daddy.UpdateConfFile("ownerpassword",ui._ownerpassword);
				daddy.UpdateConfFile("mylocationlat",ui._mylocationlat);
				daddy.UpdateConfFile("mylocationlon",ui._mylocationlon);
				daddy.UpdateConfFile("ignorefounds",ui._ignorefounds);
				daddy.UpdateConfFile("key",ui._key);
				daddy.Text = "MGM - " + daddy.GetTranslator().GetString("User") + ": " + name;
				daddy.UpdateHomeInternalInformation();
				daddy.CheckGCAccount(true, true);
				daddy.UpdateHMIForGC();
				
                // On va recharger les derniers fichiers chargés !
                daddy.ReloadPreviouslyLoadedFiles();
                
				return true;
			}
			else
				return false;
		}
		
		/// <summary>
		/// Add current profile from MGM into profile list
		/// <param name="daddy">main windows</param>
		/// </summary>
		public void UpdateBasedOnMGMCurrentProfile(MainWindow daddy)
		{
			String owner = ConfigurationManager.AppSettings["owner"];
			if (owner == "")
				return;
			UserInfo ui = null;
			if (_dicoProfile.ContainsKey(owner))
				ui = _dicoProfile[owner];
			else
			{
				ui = new UserInfo();
				_dicoProfile[owner] = ui;
			}
			
			ui._owner = owner;
			ui._ownerpassword = ConfigurationManager.AppSettings["ownerpassword"];
			ui._mylocationlat = ConfigurationManager.AppSettings["mylocationlat"];
			ui._mylocationlon = ConfigurationManager.AppSettings["mylocationlon"];
			ui._ignorefounds = ConfigurationManager.AppSettings["ignorefounds"];
			ui._key = ConfigurationManager.AppSettings["key"];
			
			this.SaveProfiles();
			daddy.BuildProfileSubMenu();
		}
		
		/// <summary>
		/// Load information from xml file
		/// </summary>
		public void LoadProfiles()
		{
			String filename = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + _dataFile;
			try
			{
				if (File.Exists(filename))
	        	{
					XmlDocument xmldoc = new XmlDocument();
	                XmlNodeList xmlnode;
	                xmldoc.Load(filename);
	                xmlnode = xmldoc.SelectNodes("/profiles/owner");
	                
	                foreach (XmlNode elt in xmlnode)
	                {
	                	String name = MyTools.getAttributeValue(elt, "name");
	                	UserInfo ui = null;
	                	if (_dicoProfile.ContainsKey(name))
	                	{
	                		ui = _dicoProfile[name];
	                	}
	                	else
	                	{
	                		ui = new UserInfo();
	                		_dicoProfile[name] = ui;
	                	}
	                	ui._owner = name;
	                	
	                	XmlNode prefs = elt["preferences"];
	                	if ((prefs != null) && (prefs.ChildNodes.Count != 0))
	                    {
	                		XmlNode node = null;
	                		
	                		node = prefs.ChildNodes[0];
	                		ui._ownerpassword = MyTools.getAttributeValue(node, "value");
	                        node = prefs.ChildNodes[1];
	                		ui._mylocationlat = MyTools.getAttributeValue(node, "value");
	                        node = prefs.ChildNodes[2];
	                		ui._mylocationlon = MyTools.getAttributeValue(node, "value");
	                        node = prefs.ChildNodes[3];
	                        ui._ignorefounds = MyTools.getAttributeValue(node, "value");
	                        try
	                        {
	                        	node = prefs.ChildNodes[4];
	                        	ui._key = MyTools.getAttributeValue(node, "value");
	                        }
	                        catch(Exception)
	                        {
	                        	ui._key = "";
	                        }
	                        
	                	}
	                }
	        	}
			}
			catch(Exception ex)
			{
				_daddy.ShowException("Loading profile", "File : " + filename, ex);
			}
		}
		
		/// <summary>
		/// Save information into xml file
		/// </summary>
		public void SaveProfiles()
		{
			String filename = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + _dataFile;
            System.IO.StreamWriter file = new System.IO.StreamWriter(filename, false);
            file.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            file.WriteLine("<profiles>");
            foreach(KeyValuePair<String, UserInfo> paire in _dicoProfile)
        	{
            	file.WriteLine("	<owner name=\"" + MyTools.HtmlToXml(paire.Value._owner) + "\">");
            	file.WriteLine("	   <preferences>");
            	file.WriteLine("    	    <ownerpassword value=\"" + paire.Value._ownerpassword + "\"/>");
        		file.WriteLine("    	    <mylocationlat value=\"" + paire.Value._mylocationlat + "\"/>");
        		file.WriteLine("    	    <mylocationlon value=\"" + paire.Value._mylocationlon + "\"/>");
        		file.WriteLine("    	    <ignorefounds value=\"" + paire.Value._ignorefounds + "\"/>");
        		file.WriteLine("    	    <key value=\"" + paire.Value._key + "\"/>");
        		file.WriteLine("	   </preferences>");
        		file.WriteLine("	</owner>");
        	}
            file.WriteLine("</profiles>");
            file.Close();
		}
		
		/// <summary>
        /// ToString override
        /// </summary>
        /// <returns>String version of the object</returns>
        public override string ToString()
        {
        	string s = "";
        	foreach(KeyValuePair<String, UserInfo> paire in _dicoProfile)
        	{
        		s += "Key: " + paire.Key + "\r\n";
        		s += paire.Value.ToString();
        	}
        	return s;
        }
	}
	
	/// <summary>
	/// User information
	/// </summary>
	public class UserInfo
	{
		/// <summary>
		/// 
		/// </summary>
		public String _owner = "";
		
		/// <summary>
		/// 
		/// </summary>
		public String _ownerpassword = "";
		
		/// <summary>
		/// 
		/// </summary>
		public String _mylocationlat = "";
		
		/// <summary>
		/// 
		/// </summary>
		public String _mylocationlon = "";
		
		/// <summary>
		/// 
		/// </summary>
		public String _ignorefounds = "";
		
		/// <summary>
		/// 
		/// </summary>
		public String _key = "";
		
		/// <summary>
        /// ToString override
        /// </summary>
        /// <returns>String version of the object</returns>
        public override string ToString()
        {
        	string s = "";
        	s += "_owner: " + _owner + "\r\n";
        	s += "_ownerpassword: " + _ownerpassword + "\r\n";
        	s += "_mylocationlat: " + _mylocationlat + "\r\n";
        	s += "_mylocationlon: " + _mylocationlon + "\r\n";
        	s += "_ignorefounds: " + _ignorefounds + "\r\n";
        	s += "_key: " + _key + "\r\n";
        	return s;
        }
	}
}
