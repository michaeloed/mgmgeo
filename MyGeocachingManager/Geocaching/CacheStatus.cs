/*
 * Created by SharpDevelop.
 * User: bcristiano
 * Date: 11/01/2016
 * Time: 12:24
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
	/// Class to handle status of caches in MGM (found)
	/// </summary>
	public class CacheStatus
	{
		const string _dataFile = "CacheStatus.dat";
		MainWindow _daddy = null;
		
		/// <summary>
		/// Key: username
		/// Value: list of found caches codes
		/// </summary>
		Dictionary<String, List<String>> _dicoCacheFound = new Dictionary<string, List<string>>();
		
		/// <summary>
		/// Default
		/// </summary>
		/// <param name="daddy">MainWindows</param>
		public CacheStatus(MainWindow daddy)
		{
			_daddy = daddy;
		}
		
		/// <summary>
		/// Save information into xml file
		/// </summary>
		public void SaveCacheStatus()
		{
			String filename = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + _dataFile;
            System.IO.StreamWriter file = new System.IO.StreamWriter(filename, false);
            file.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            file.WriteLine("<cachestatus>");
            
            foreach(KeyValuePair<String, List<String>> paire in _dicoCacheFound)
        	{
            	file.WriteLine("	<user name=\"" + MyTools.HtmlToXml(paire.Key) + "\">");
            	file.WriteLine("    	<found>");
        		foreach(String c in paire.Value)
        		{
        			file.WriteLine("    	    <cache code=\"" + c + "\"/>");
        		}
        		file.WriteLine("    	</found>");
        		file.WriteLine("	</user>");
        	}
            file.WriteLine("</cachestatus>");
            file.Close();
		}
		
		/// <summary>
		/// Load information from xml file
		/// </summary>
		public void LoadCacheStatus()
		{
			String filename = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + _dataFile;
			try
			{
				if (File.Exists(filename))
	        	{
	        		XmlDocument xmldoc;
	                XmlNodeList xmlnode;
	                xmldoc = new XmlDocument();
	                xmldoc.Load(filename);
	                xmlnode = xmldoc.SelectNodes("/cachestatus/user");
	                
	                foreach (XmlNode elt in xmlnode)
	                {
	                	String name = MyTools.getAttributeValue(elt, "name");
	                	List<String> caches = null;
	                	if (_dicoCacheFound.ContainsKey(name))
	                	{
	                		caches = _dicoCacheFound[name];
	                	}
	                	else
	                	{
	                		caches = new List<string>();
	                		_dicoCacheFound[name] = caches;
	                	}
	                	
	                	XmlNode foundit = elt["found"];
	                	if ((foundit != null) && (foundit.ChildNodes.Count != 0))
	                    {
	                        foreach (XmlNode c in foundit.ChildNodes)
	                        {
		                		String code = MyTools.getAttributeValue(c, "code");
		                		if (caches.Contains(code) == false)
		                			caches.Add(code);
		                	}
	                	}
	                }
	                
	        	}
			}
			catch(Exception ex)
			{
				_daddy.ShowException("Loading cache status", "File : " + filename, ex);
			}
		}
		
		/// <summary>
        /// ToString override
        /// </summary>
        /// <returns>String version of the object</returns>
        public override string ToString()
        {
        	string s = "";
        	foreach(KeyValuePair<String, List<String>> paire in _dicoCacheFound)
        	{
        		s += paire.Key + "\r\n";
        		foreach(String c in paire.Value)
        			s += "   found: " + c + "\r\n";
        	}
           
            return s;
        }
        
        /// <summary>
        /// Declare a cache found for a specific user
        /// </summary>
        /// <param name="name">username</param>
        /// <param name="code">cache code</param>
        /// <returns>true if success</returns>
        public bool DeclareFoundCache(String name, String code)
        {
        	List<String> caches = null;
        	if (_dicoCacheFound.ContainsKey(name))
        	{
        		caches = _dicoCacheFound[name];
        	}
        	else
        	{
        		caches = new List<string>();
        		_dicoCacheFound[name] = caches;
        	}
        	
        	if (caches.Contains(code) == false)
        	{
        		caches.Add(code);
        		return true;
        	}
        	else
        		return false;
        }
        
        /// <summary>
        /// Check if cache is found for a specific user
        /// </summary>
        /// <param name="name">username</param>
        /// <param name="code">cache code</param>
        /// <returns>true if success</returns>
        public bool IsFoundCache(String name, String code)
        {
        	if (_dicoCacheFound.ContainsKey(name))
        	{
        		List<String> caches = _dicoCacheFound[name];
        		return caches.Contains(code);
        	}
        	else
        	{
        		return false;
        	}
        }
        
        
        /// <summary>
        /// Return list of cache found for a user
        /// </summary>
        /// <param name="name">username</param>
        /// <returns>null if no cache found</returns>
        public List<String> GetCacheFoundFromUser(String name)
        {
        	if (_dicoCacheFound.ContainsKey(name))
        	{
        		return _dicoCacheFound[name];
        	}
        	else
        	{
        		return null;
        	}
        }
	}
}
