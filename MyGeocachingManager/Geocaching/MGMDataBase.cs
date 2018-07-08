/*
 * Created by SharpDevelop.
 * User: bcristiano
 * Date: 06/07/2016
 * Time: 11:30
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System.Web;
using SpaceEyeTools;
using System.Configuration;

namespace MyGeocachingManager.Geocaching
{
	/// <summary>
	/// All operations related to MGM database
	/// </summary>
	public class MGMDataBase
	{
		MainWindow _daddy = null;
		private String _dbpath = "";
		
		/// <summary>
		/// 
		/// </summary>
		public class DBinfo
		{
			/// <summary>
			/// 
			/// </summary>
			public double latmin = Double.MaxValue;
			/// <summary>
			/// 
			/// </summary>
			public double lonmin = Double.MaxValue;
			/// <summary>
			/// 
			/// </summary>
			public double latmax = Double.MinValue;
			/// <summary>
			/// 
			/// </summary>
			public double lonmax = Double.MinValue;
		}
		
		/// <summary>
		/// 
		/// </summary>
		public enum DBType
		{
			/// <summary>
			/// 
			/// </summary>
			GeocacheFull,
			/// <summary>
			/// 
			/// </summary>
			Log,
			/// <summary>
			/// 
			/// </summary>
			Waypoint,
			/// <summary>
			/// 
			/// </summary>
			TravelBug,
			/// <summary>
			/// 
			/// </summary>
			Area
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="daddy"></param>
		/// <param name="file"></param>
		/// <param name="create"></param>
		public MGMDataBase(MainWindow daddy, String file, bool create = false)
		{
			_daddy = daddy;
			_dbpath = "URI=file:" + file;// + ";Compress=True;";
			
			if (create)
			{
				using ( SQLiteConnection con = new SQLiteConnection(_dbpath))
				{
					con.Open();
					using (SQLiteCommand cmd = new SQLiteCommand(con))
					{
						// Drop table
						cmd.CommandText = "DROP TABLE IF EXISTS GeocacheFull";
						cmd.ExecuteNonQuery();
						cmd.CommandText = "DROP TABLE IF EXISTS Log";
						cmd.ExecuteNonQuery();
						cmd.CommandText = "DROP TABLE IF EXISTS Waypoint";
						cmd.ExecuteNonQuery();
						cmd.CommandText = "DROP TABLE IF EXISTS TravelBug";
						cmd.ExecuteNonQuery();
						cmd.CommandText = "DROP TABLE IF EXISTS Area";
						cmd.ExecuteNonQuery();
						
						// Create table GeocacheFull
						// Date format YYYY-MM-DD HH:MM:SS.SSS
						cmd.CommandText = @"CREATE TABLE IF NOT EXISTS GeocacheFull(
								Code TEXT PRIMARY KEY,
								CacheID TEXT,
								Name TEXT,
								Type TEXT,
								Latitude DOUBLE,
								Longitude DOUBLE,
								Difficulty TEXT,
								Terrain TEXT,
								Owner TEXT,
								OwnerId TEXT,
								PlacedBy Text,
								DateCreation TEXT,
								ShortDescription TEXT,
								LongDescription TEXT,
								ShortDescriptionAsHTML BOOL,
								LongDescriptionAsHTML BOOL,
								Available BOOL,
								Archived BOOL,
								Container TEXT,
								Hint TEXT,
								Found BOOL,
								AttributesIds TEXT,
								AttributesStatus TEXT,
								LogIds TEXT,
								WaypointsIds TEXT,
								TBIds Text,
								Country Text,
								State Text,
								DateExport TEXT,
								Owned BOOL
								)"; // Garder Owned à la fin pour garantir la compatibilité
						cmd.ExecuteNonQuery();
					
						// Create table Log
						cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Log(
								LogId TEXT PRIMARY KEY,
								Date TEXT,
								Type TEXT,
								Finder TEXT,
								FinderId TEXT,
								Comment TEXT,
								CommentIsEncoded BOOL,
								DateExport TEXT
								)";
						cmd.ExecuteNonQuery();
						
						// Create table Waypoint
						cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Waypoint(
								Code TEXT PRIMARY KEY,
								Origin INT,
								Latitude TEXT,
								Longitude TEXT,
								Date TEXT,
								Comment TEXT,
								Description TEXT,
								Url TEXT,
								UrlName TEXT,
								Symbol TEXT,
								Type TEXT,
								DateExport TEXT
								)";
						cmd.ExecuteNonQuery();
						
						// Create table TravelBug
						cmd.CommandText = @"CREATE TABLE IF NOT EXISTS TravelBug(
								Id TEXT PRIMARY KEY,
								Ref TEXT,
								Name TEXT,
								DateExport TEXT
								)";
						cmd.ExecuteNonQuery();
						
						// Create table Area
						// Id : latmin, latmax, lonmin, lonmax
						// Coord : coordinate in WGS84 decimal degrees
						cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Area(
								Id TEXT PRIMARY KEY,
								Coord DOUBLE
								)";
						cmd.ExecuteNonQuery();
					}
					con.Close();
				}
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="topleft"></param>
		/// <param name="bottomright"></param>
		/// <returns></returns>
		public bool GetArea(out PointLatLng topleft, out PointLatLng bottomright)
		{
			topleft = new PointLatLng();
			bottomright = new PointLatLng();
			
			try
			{
				// On requête depuis la DB
                string cs = _dbpath;
                double latmin = Double.MaxValue;
                double latmax = Double.MinValue;
                double lonmin = Double.MaxValue;
                double lonmax = Double.MinValue;
                using (SQLiteConnection con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(con))
                    {
                        // retrieve values
                        String stm = "SELECT * from Area";
                        using (SQLiteCommand cmd2 = new SQLiteCommand(stm, con))
                        {
                            using (SQLiteDataReader rdr = cmd2.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                	String id = (String)(rdr["Id"]);
                                	double coord = (double)(rdr["Coord"]);
                                	switch(id)
                                	{
                                		case "latmin":
                                			latmin = coord;
                                			break;
                                		case "latmax":
                                			latmax = coord;
                                			break;
										case "lonmin":
                                			lonmin = coord;
                                			break;
										case "lonmax":
                                			lonmax = coord;
                                			break;
                                		default:
                                			break;
                                	}
                                }
                            }
                        }
                    }
                    con.Close();
                }
				
                topleft = new PointLatLng(latmax, lonmin);
                bottomright = new PointLatLng(latmin, lonmax);
                
				return true;
			}
			catch(Exception)
			{
				return false;
			}
			
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public String GetFullCount()
		{
			String msg = "";
			msg += "Base : " + _dbpath + "\r\n";
			msg += "Nombre total de Caches : " + GetCount(MGMDataBase.DBType.GeocacheFull).ToString() + "\r\n";
            msg += "Nombre total de Waypoints : " + GetCount(MGMDataBase.DBType.Waypoint).ToString() + "\r\n";
            msg += "Nombre total de Logs : " + GetCount(MGMDataBase.DBType.Log).ToString() + "\r\n";
            msg += "Nombre total de Travel bugs / Geocoins : " + GetCount(MGMDataBase.DBType.TravelBug).ToString() + "\r\n";
			return msg;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public int GetCount(DBType type = DBType.GeocacheFull)
		{
			
			try
			{
				// On requête depuis la DB
                string cs = _dbpath;
                using (SQLiteConnection con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(con))
                    {
                        // retrieve values
                        String stm = "SELECT COUNT(*) from ";
                        switch(type)
                        {
                        	default:
                        	case DBType.GeocacheFull:
                        		stm += "GeocacheFull";
                        		break;
                    		case DBType.Area:
                    			stm += "Area";
                    			break;
                    		case DBType.Log:
                    			stm += "Log";
	                    		break;
	                    	case DBType.TravelBug:
                    			stm += "TravelBug";
                    			break;
                    		case DBType.Waypoint:
                    			stm += "Waypoint";
                    			break;
                        }
                        using (SQLiteCommand cmd2 = new SQLiteCommand(stm, con))
                        {
                            using (SQLiteDataReader rdr = cmd2.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                	int nb = rdr.GetInt32(0);
                                	return nb;
                                }
                            }
                        }
                    }
                    con.Close();
                }
				
                
				return -1;
			}
			catch(Exception)
			{
				return -1;
			}
			
		}
		
		private static Geocache FillGeocache(MainWindow daddy, String owner, String dbname, String code, SQLiteDataReader rdr, SQLiteConnection con)
		{
           var geo = new Geocache(daddy);
           geo._Code = code;
           geo._Url = "http://coord.info/" + geo._Code;
           geo._CacheId = rdr["CacheID"] as String;
           geo._Name = rdr["Name"] as String;
           geo._Type = rdr["Type"] as String;
           geo._dLatitude = (Double)(rdr["Latitude"]);
           geo._dLongitude = (Double)(rdr["Longitude"]);
           geo._Latitude = geo._dLatitude.ToString().Replace(",", ".");
           geo._Longitude = geo._dLongitude.ToString().Replace(",", ".");
           geo._D = rdr["Difficulty"] as String;
           geo._T = rdr["Terrain"] as String;
           geo._Owner = rdr["Owner"] as String;
           geo._OwnerId = rdr["OwnerId"] as String;
           geo._PlacedBy = rdr["PlacedBy"] as String;
           geo._DateCreation = rdr["DateCreation"] as String;
           geo._ShortDescription = rdr["ShortDescription"] as String;
           geo._LongDescription = rdr["LongDescription"] as String;
           geo._ShortDescriptionInHTML = geo._ShortDescription;
           geo._LongDescriptionInHTML = geo._LongDescription;
           geo._ShortDescHTML = ((bool)(rdr["ShortDescriptionAsHTML"]))?"True":"False";
           geo._LongDescHTML = ((bool)(rdr["LongDescriptionAsHTML"]))?"True":"False";
           geo._Available = ((bool)(rdr["Available"]))?"True":"False";
           geo._Archived = ((bool)(rdr["Archived"]))?"True":"False";
           geo._Container = rdr["Container"] as String;
           geo._Hint = rdr["Hint"] as String;
           geo._bFound = (bool)(rdr["Found"]);
           geo._listAttributesId = new List<String>((rdr["AttributesIds"] as String).Split(';'));
           if (geo._listAttributesId.Count != 0)
           {
           	geo._listAttributesId.RemoveAt(geo._listAttributesId.Count - 1);
           }
           geo._Attributes = new List<String>((rdr["AttributesStatus"] as String).Split(';'));
           if (geo._Attributes.Count != 0)
           {
           	geo._Attributes.RemoveAt(geo._Attributes.Count - 1);
           }
           
           // LogIds
           String logsid = rdr["LogIds"] as String;
           if (logsid != "")
           	logsid = logsid.Substring(0, logsid.Length - 1);
           
           	if (logsid != "")
           	{
           		// Retrieve log from DB
           		String sq = "SELECT * from Log where LogId in (" + logsid + ")";
           		// retrieve values
                using (SQLiteCommand cmd3 = new SQLiteCommand(sq, con))
                {
                    using (SQLiteDataReader rdr3 = cmd3.ExecuteReader())
                    {
                        while (rdr3.Read())
                        {
                        	CacheLog log  = new CacheLog(daddy);
                        	log._LogId = rdr3["LogId"] as String;
                        	log._Date = rdr3["Date"] as String;
                        	log._Type = rdr3["Type"] as String;
                        	log._User = rdr3["Finder"] as String;
                        	log._FinderId = rdr3["FinderId"] as String;
                        	log._Text = rdr3["Comment"] as String;
                        	log._Encoded = ((bool)(rdr3["CommentIsEncoded"]))?"True":"False";
                        	geo._Logs.Insert(0,log);
                        }
                    }
                }
           	}
           
           // WaypointsIds
           String wptsid = rdr["WaypointsIds"] as String;
           if (wptsid != "")
           	wptsid = wptsid.Substring(0, wptsid.Length - 1);
           if (wptsid != "")
           	{
           		// Retrieve wpts from DB
           		String[] ids = wptsid.Split(',');
           		wptsid = "";
           		foreach(String id in ids)
           		{
           			wptsid += "'" + id + "',";
           		}
           		wptsid = wptsid.Substring(0, wptsid.Length - 1);
           		String sq = "SELECT * from Waypoint where Code in (" + wptsid + ")";
           		// retrieve values
                using (SQLiteCommand cmd3 = new SQLiteCommand(sq, con))
                {
                    using (SQLiteDataReader rdr3 = cmd3.ExecuteReader())
                    {
                        while (rdr3.Read())
                        {
                        	Waypoint wpt = new Waypoint();
                        	String wcode = rdr3["Code"] as String;
                        	wpt._name = wcode;
                        	wpt._GCparent = geo._Code;
                        	wpt._eOrigin = (Waypoint.WaypointOrigin)((int)(rdr3["Origin"]));
                        	wpt._lat = rdr3["Latitude"] as String;
                        	wpt._lon = rdr3["Longitude"] as String;
                        	wpt._time = rdr3["Date"] as String;
                        	wpt._cmt = rdr3["Comment"] as String;
                        	wpt._desc = rdr3["Description"] as String;
                        	wpt._url = rdr3["Url"] as String;
                        	wpt._urlname = rdr3["UrlName"] as String;
                        	wpt._sym = rdr3["Symbol"] as String;
                        	wpt._type = rdr3["Type"] as String;
                        	wpt._DateExport = DateTime.Parse(rdr3["DateExport"] as String);
                        	
                        	
                        	geo._waypoints.Add(wcode, wpt);
                        }
                    }
                }
           	}
           
           // TBIds
           String tbsid = rdr["TBIds"] as String;
           if (tbsid != "")
           	tbsid = tbsid.Substring(0, tbsid.Length - 1);
           if (tbsid != "")
           	{
           		// Retrieve tbs from DB
           		String sq = "SELECT * from TravelBug where Id in (" + tbsid + ")";
           		// retrieve values
                using (SQLiteCommand cmd3 = new SQLiteCommand(sq, con))
                {
                    using (SQLiteDataReader rdr3 = cmd3.ExecuteReader())
                    {
                        while (rdr3.Read())
                        {
                        	geo._listTBId.Add(rdr3["Id"] as String);
                        	geo._listTB.Add(rdr3["Ref"] as String, rdr3["Name"] as String);
                        }
                    }
                }
           	}
           
           geo._Country = rdr["Country"] as String;
           geo._State = rdr["State"] as String;
           geo._DateExport = DateTime.Parse(rdr["DateExport"] as String);
           
           // Owned ?

           geo.UpdateDistanceToHome(daddy.HomeLat, daddy.HomeLon);

           // Update donnée privées
           geo.UpdatePrivateData(owner); // fait netre autre les coords
           
           // complete origin
           geo._origin.Add(dbname);

           return geo;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="dr"></param>
		/// <param name="columnName"></param>
		/// <returns></returns>
		public static bool HasColumn(SQLiteDataReader dr, string columnName)
	    {
	        for (int i=0; i < dr.FieldCount; i++)
	        {
	            if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
	                return true;
	        }
	        return false;
	    }
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="stm"></param>
		/// <param name="dbname"></param>
		/// <param name="updatedaddy"></param>
		/// <returns></returns>
        public List<Geocache> PerformSelect(String stm, String dbname, bool updatedaddy = true)
        {
        	//stm = "SELECT * FROM GeocacheFull";// WHERE Code LIKE 'GC51R62'";
        	GMapOverlay overlaybigview = _daddy._cacheDetail._gmap.Overlays[GMapWrapper.MARKERS];
        	String owner = ConfigurationManager.AppSettings["owner"].ToLower();
        	
        	List<Geocache> caches = new List<Geocache>();
        	// On fait le boulot dans le worker
            try
            {
                // On requête depuis la DB
                string cs = _dbpath;
                using (SQLiteConnection con = new SQLiteConnection(cs))
                {
                    con.Open();
                    
                    using (SQLiteCommand cmd = new SQLiteCommand(con))
                    {
                        // retrieve values
                        using (SQLiteCommand cmd2 = new SQLiteCommand(stm, con))
                        {
                            using (SQLiteDataReader rdr = cmd2.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                	Geocache geo = null;
                                	String code = rdr["Code"] as String;
	                                if (!updatedaddy || ((_daddy._caches.ContainsKey(code) == false) ||((_daddy._caches[code]._origin.Count == 1) && (_daddy._caches[code]._origin.Contains("CACHECACHE")))))
                                    {
	                                	geo = FillGeocache(_daddy, owner, dbname, code, rdr, con);
	                                	
		                               // On le rajoute aux données de MGM s'il n'est pas à ignorer
		                               if (updatedaddy)
		                               {
	                                        if (_daddy._ignoreList.ContainsKey(geo._Code) == false)
	                                        {
	                                            // Ajoute à la liste générale
	                                            if (_daddy._caches.ContainsKey(code))
	                                            	_daddy._caches[code] = geo;
	                                            else
	                                            	_daddy._caches.Add(geo._Code, geo);
	
	                                            
	                                            // On met à jour le status de la cache
	                							_daddy.ChangeCacheStatusBasedonMGM(geo);
	                							
	                							// On affiche sur la cache
	                							GMapMarkerImages[] createdmarkers = _daddy.DisplayCacheOnMaps(geo, null, overlaybigview, null);
	                							
								                // On adapte le niveau de zoom au markere créé
								                int iz = GMapMarkerImages.ReturnImageLevelFromZoom(_daddy._cacheDetail._gmap.Zoom);
								                GMapMarkerImages.ChangeImageAccordingToZoom((GMapMarkerImages)(createdmarkers[1]), iz);
	                                        }
		                                	else
		                                	{
		                                		// La cache était dans Daddy, c'est sûrement mieux
		                                		geo = _daddy._caches[code];
		                                	}
	                                	}
                                   // on ajoute à la liste à afficher
                                   caches.Add(geo);                             
                                }
                              }
                            }
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception)
            {
            	throw;
            }
            
            return caches;
        }
        
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="daddy"></param>
        /// <param name="cs"></param>
        /// <param name="stm"></param>
        /// <param name="dicoCacheCache"></param>
        /// <param name="dbname"></param>
        /// <returns></returns>
		public static List<Geocache> PerformSelectNoMapsUpdate(MainWindow daddy, String cs, String stm, ref Dictionary<String, Geocache> dicoCacheCache, String dbname)
        {
        	//stm = "SELECT * FROM GeocacheFull";// WHERE Code LIKE 'GC51R62'";
        	GMapOverlay overlaybigview = daddy._cacheDetail._gmap.Overlays[GMapWrapper.MARKERS];
        	String owner = ConfigurationManager.AppSettings["owner"].ToLower();
        	
        	List<Geocache> caches = new List<Geocache>();
        	// On fait le boulot dans le worker
            try
            {
                // On requête depuis la DB
                using (SQLiteConnection con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(con))
                    {
                        // retrieve values
                        using (SQLiteCommand cmd2 = new SQLiteCommand(stm, con))
                        {
                            using (SQLiteDataReader rdr = cmd2.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                	Geocache geo = null;
	                                String code = rdr["Code"] as String;
	                                if ((daddy._caches.ContainsKey(code) == false) && (dicoCacheCache.ContainsKey(code) == false))
	                                {
	                                	
	                                   geo = FillGeocache(daddy, owner, dbname, code, rdr, con);
	                                   
		                               // On le rajoute aux données de MGM s'il n'est pas à ignorer
                                        if (daddy._ignoreList.ContainsKey(geo._Code) == false)
                                        {
                                            // Ajoute à la liste générale
                                            if (daddy._caches.ContainsKey(code))
                                            	daddy._caches[code] = geo;
                                            else
                                            	daddy._caches.Add(geo._Code, geo);

                                            // complete origin
                                            geo._origin.Add(dbname);

                                            // On met à jour le status de la cache
                							daddy.ChangeCacheStatusBasedonMGM(geo);
                							
                							if (dicoCacheCache.ContainsKey(code))
                                            	dicoCacheCache[code] = geo;
                                            else
                                            	dicoCacheCache.Add(geo._Code, geo);
                                        }
                                	}
                                	else
                                	{
                                		// La cache était dans Daddy, c'est sûrement mieux
                                		geo = daddy._caches[code];
                                	}
                                   // on ajoute à la liste à afficher
                                   caches.Add(geo);                             
                                }
                            }
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception)
            {
            	throw;
            }
            
            return caches;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="caches"></param>
        /// <param name="striphtml"></param>
        /// <param name="doc"></param>
        /// <param name="dbi"></param>
        public void InsertGeocaches(List<Geocache> caches, bool striphtml, HtmlAgilityPack.HtmlDocument doc, ref DBinfo dbi)
		{
			List<Waypoint> wpts = new List<Waypoint>();
			List<CacheLog> logs = new List<CacheLog>();
			Dictionary<String, Tuple<String, String>> tbs = new Dictionary<string, Tuple<string, string>>();
						
			using (SQLiteConnection con = new SQLiteConnection(_dbpath))
			{
				con.Open();
				using (SQLiteCommand cmd = new SQLiteCommand(con))
				{
					// Prepare transaction
            	SQLiteTransaction transaction = con.BeginTransaction();
            	
	            	cmd.CommandText = 
@"INSERT OR REPLACE INTO GeocacheFull(
Code,
CacheID,
Name,
Type,
Latitude,
Longitude,
Difficulty,
Terrain,
Owner,
OwnerId,
PlacedBy,
DateCreation,
ShortDescription,
LongDescription,
ShortDescriptionAsHTML,
LongDescriptionAsHTML,
Available,
Archived,
Container,
Hint,
Found,
AttributesIds,
AttributesStatus,
LogIds,
WaypointsIds,
TBIds,
Country,
State,
DateExport,
Owned
) VALUES(
@Code,
@CacheID,
@Name,
@Type,
@Latitude,
@Longitude,
@Difficulty,
@Terrain,
@Owner,
@OwnerId,
@PlacedBy,
@DateCreation,
@ShortDescription,
@LongDescription,
@ShortDescriptionAsHTML,
@LongDescriptionAsHTML,
@Available,
@Archived,
@Container,
@Hint,
@Found,
@AttributesIds,
@AttributesStatus,
@LogIds,
@WaypointsIds,
@TBIds,
@Country,
@State,
@DateExport,
@Owned
)";
cmd.Parameters.AddWithValue("@Code", "");	
cmd.Parameters.AddWithValue("@CacheID", "");	
cmd.Parameters.AddWithValue("@Name", "");	
cmd.Parameters.AddWithValue("@Type", "");	
cmd.Parameters.AddWithValue("@Latitude", 0.0);	
cmd.Parameters.AddWithValue("@Longitude", 0.0);	
cmd.Parameters.AddWithValue("@Difficulty", "");	
cmd.Parameters.AddWithValue("@Terrain", "");	
cmd.Parameters.AddWithValue("@Owner", "");	
cmd.Parameters.AddWithValue("@OwnerId", "");	
cmd.Parameters.AddWithValue("@PlacedBy", "");	
cmd.Parameters.AddWithValue("@DateCreation", "");	
cmd.Parameters.AddWithValue("@ShortDescription", "");	
cmd.Parameters.AddWithValue("@LongDescription", "");	
cmd.Parameters.AddWithValue("@ShortDescriptionAsHTML", false);	
cmd.Parameters.AddWithValue("@LongDescriptionAsHTML", false);	
cmd.Parameters.AddWithValue("@Available", false);	
cmd.Parameters.AddWithValue("@Archived", false);	
cmd.Parameters.AddWithValue("@Container", "");	
cmd.Parameters.AddWithValue("@Hint", "");	
cmd.Parameters.AddWithValue("@Found", false);	
cmd.Parameters.AddWithValue("@AttributesIds", "");	
cmd.Parameters.AddWithValue("@AttributesStatus", "");	
cmd.Parameters.AddWithValue("@LogIds", "");	
cmd.Parameters.AddWithValue("@WaypointsIds", "");	
cmd.Parameters.AddWithValue("@TBIds", "");	
cmd.Parameters.AddWithValue("@Country", "");	
cmd.Parameters.AddWithValue("@State", "");	
cmd.Parameters.AddWithValue("@DateExport", "");	
cmd.Parameters.AddWithValue("@Owned", false);	

	                
					foreach(Geocache geo in caches)
					{
						// Area
						if (geo._dLatitude < dbi.latmin)
							dbi.latmin = geo._dLatitude;
						if (geo._dLatitude > dbi.latmax)
							dbi.latmax = geo._dLatitude;
						if (geo._dLongitude < dbi.lonmin)
							dbi.lonmin = geo._dLongitude;
						if (geo._dLongitude > dbi.lonmax)
							dbi.lonmax = geo._dLongitude;
						
						// Insert values using preparedcommands
cmd.Parameters["@Code"].Value = geo._Code;
cmd.Parameters["@CacheID"].Value = geo._CacheId;
cmd.Parameters["@Name"].Value = (striphtml)?MyTools.StripHtmlTags(geo._Name, doc):geo._Name;
cmd.Parameters["@Type"].Value = geo._Type;
cmd.Parameters["@Latitude"].Value = geo._dLatitude;
cmd.Parameters["@Longitude"].Value = geo._dLongitude;
cmd.Parameters["@Difficulty"].Value = geo._D;
cmd.Parameters["@Terrain"].Value = geo._T;
cmd.Parameters["@Owner"].Value = geo._Owner;
cmd.Parameters["@OwnerId"].Value = geo._OwnerId;
cmd.Parameters["@PlacedBy"].Value = geo._PlacedBy;
cmd.Parameters["@DateCreation"].Value = geo._DateCreation;

if (striphtml)
{
	cmd.Parameters["@ShortDescription"].Value = (striphtml)?MyTools.StripHtmlTags(geo._ShortDescription, doc):geo._ShortDescription;
	cmd.Parameters["@LongDescription"].Value = (striphtml)?MyTools.StripHtmlTags(geo._LongDescription, doc):geo._LongDescription;
	cmd.Parameters["@ShortDescriptionAsHTML"].Value = "False";
	cmd.Parameters["@LongDescriptionAsHTML"].Value = "False";
}
else
{
	cmd.Parameters["@ShortDescription"].Value = geo._ShortDescription;
	cmd.Parameters["@LongDescription"].Value = geo._LongDescription;
	cmd.Parameters["@ShortDescriptionAsHTML"].Value = (geo._ShortDescHTML == "True");
	cmd.Parameters["@LongDescriptionAsHTML"].Value = (geo._LongDescHTML == "True");
}

cmd.Parameters["@Available"].Value = (geo._Available == "True");
cmd.Parameters["@Archived"].Value = (geo._Archived == "True");;
cmd.Parameters["@Container"].Value = geo._Container;
cmd.Parameters["@Hint"].Value = (striphtml)?MyTools.StripHtmlTags(geo._Hint, doc):geo._Hint;
cmd.Parameters["@Found"].Value = geo.IsFound();
cmd.Parameters["@AttributesIds"].Value = "";
foreach(String s in geo._listAttributesId)
	cmd.Parameters["@AttributesIds"].Value += s + ";";
cmd.Parameters["@AttributesStatus"].Value = "";
foreach(String s in geo._Attributes)
	cmd.Parameters["@AttributesStatus"].Value += s + ";";
cmd.Parameters["@LogIds"].Value = "";
foreach(CacheLog log in geo._Logs)
{
	cmd.Parameters["@LogIds"].Value += log._LogId + ",";
	logs.Add(log);
}
cmd.Parameters["@WaypointsIds"].Value = "";
// Standard waypoints
foreach(KeyValuePair<String, Waypoint> pair in geo._waypoints)
{
	cmd.Parameters["@WaypointsIds"].Value += pair.Value._name + ",";
	wpts.Add(pair.Value);
}
// MGM waypoints
foreach(KeyValuePair<String, Waypoint> pair in geo._waypointsFromMGM)
{
	cmd.Parameters["@WaypointsIds"].Value += pair.Value._name + ",";
	wpts.Add(pair.Value);
}

cmd.Parameters["@TBIds"].Value = "";
int i=0;
String sid = "";
foreach (KeyValuePair<String, String> att in geo._listTB)
{
	sid = geo._listTBId[i];
	cmd.Parameters["@TBIds"].Value += sid + ",";
	if (!tbs.ContainsKey(sid))
	{
	   tbs.Add(sid,new Tuple<String, String>(att.Key, att.Value));
	}
    i++;
}

cmd.Parameters["@Country"].Value = geo._Country;
cmd.Parameters["@State"].Value = geo._State;
cmd.Parameters["@DateExport"].Value = geo._DateExport;
cmd.Parameters["@Owned"].Value = geo._bOwned;
		                cmd.ExecuteNonQuery();
					}
					
					// en transaction
                	transaction.Commit();
				}
				con.Close();
			}
			
			InsertLogs(logs, striphtml, doc);
			InsertWaypoints(wpts, striphtml, doc);
			InsertTBs(tbs);
			InsertArea(dbi.latmin, dbi.latmax, dbi.lonmin, dbi.lonmax);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="logs"></param>
		/// <param name="striphtml"></param>
		/// <param name="doc"></param>
		private void InsertLogs(List<CacheLog> logs, bool striphtml, HtmlAgilityPack.HtmlDocument doc)
		{
			using (SQLiteConnection con = new SQLiteConnection(_dbpath))
			{
				con.Open();
				using (SQLiteCommand cmd = new SQLiteCommand(con))
				{
					// Prepare transaction
            	SQLiteTransaction transaction = con.BeginTransaction();
            	
	            	cmd.CommandText = 
@"INSERT  OR REPLACE INTO Log(
LogId,
Date,
Type,
Finder,
FinderId,
Comment,
CommentIsEncoded,
DateExport
) VALUES(
@LogId,
@Date,
@Type,
@Finder,
@FinderId,
@Comment,
@CommentIsEncoded,
@DateExport
)";
cmd.Parameters.AddWithValue("@LogId", "");
cmd.Parameters.AddWithValue("@Date", "");
cmd.Parameters.AddWithValue("@Type", "");
cmd.Parameters.AddWithValue("@Finder", "");
cmd.Parameters.AddWithValue("@FinderId", "");
cmd.Parameters.AddWithValue("@Comment", "");
cmd.Parameters.AddWithValue("@CommentIsEncoded", false);
cmd.Parameters.AddWithValue("@DateExport", "");


					String now = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
					foreach(CacheLog log in logs)
					{
						
						// Insert values using preparedcommands
cmd.Parameters["@LogId"].Value = log._LogId;
cmd.Parameters["@Date"].Value = log._Date;
cmd.Parameters["@Type"].Value = log._Type;
cmd.Parameters["@Finder"].Value = log._User;
cmd.Parameters["@FinderId"].Value = log._FinderId;
cmd.Parameters["@Comment"].Value = (striphtml)?MyTools.StripHtmlTags(log._Text, doc):log._Text;
cmd.Parameters["@CommentIsEncoded"].Value = (log._Encoded == "True");
cmd.Parameters["@DateExport"].Value = now;

		                cmd.ExecuteNonQuery();
					}
					
					// en transaction
                	transaction.Commit();
				}
				con.Close();
			}
		}
		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="wpts"></param>
		/// <param name="striphtml"></param>
		/// <param name="doc"></param>
		private void InsertWaypoints(List<Waypoint> wpts, bool striphtml, HtmlAgilityPack.HtmlDocument doc)
		{
			using (SQLiteConnection con = new SQLiteConnection(_dbpath))
			{
				con.Open();
				using (SQLiteCommand cmd = new SQLiteCommand(con))
				{
					// Prepare transaction
            	SQLiteTransaction transaction = con.BeginTransaction();
            	
	            	cmd.CommandText = 
@"INSERT  OR REPLACE INTO Waypoint(
Code,
Origin,
Latitude,
Longitude,
Date,
Comment,
Description,
Url,
UrlName,
Symbol,
Type,
DateExport
) VALUES(
@Code,
@Origin,
@Latitude,
@Longitude,
@Date,
@Comment,
@Description,
@Url,
@UrlName,
@Symbol,
@Type,
@DateExport
)";
cmd.Parameters.AddWithValue("@Code", "");
cmd.Parameters.AddWithValue("@Origin", 0);
cmd.Parameters.AddWithValue("@Latitude", "");
cmd.Parameters.AddWithValue("@Longitude", "");
cmd.Parameters.AddWithValue("@Date", "");
cmd.Parameters.AddWithValue("@Comment", "");
cmd.Parameters.AddWithValue("@Description", "");
cmd.Parameters.AddWithValue("@Url", "");
cmd.Parameters.AddWithValue("@UrlName", "");
cmd.Parameters.AddWithValue("@Symbol", "");
cmd.Parameters.AddWithValue("@Type", "");
cmd.Parameters.AddWithValue("@DateExport", "");

					foreach(Waypoint wpt in wpts)
					{
						
						// Insert values using preparedcommands
cmd.Parameters["@Code"].Value = wpt._name;
cmd.Parameters["@Origin"].Value = (int)(wpt._eOrigin);
cmd.Parameters["@Latitude"].Value = wpt._lat;
cmd.Parameters["@Longitude"].Value = wpt._lon;
cmd.Parameters["@Date"].Value = wpt._time;
cmd.Parameters["@Comment"].Value = wpt._cmt;
cmd.Parameters["@Description"].Value = (striphtml)?MyTools.StripHtmlTags(wpt._desc, doc):wpt._desc;
cmd.Parameters["@Url"].Value = wpt._url;
cmd.Parameters["@UrlName"].Value = wpt._urlname;
cmd.Parameters["@Symbol"].Value = wpt._sym;
cmd.Parameters["@Type"].Value = wpt._type;
cmd.Parameters["@DateExport"].Value = wpt._DateExport;

		                cmd.ExecuteNonQuery();
					}
					
					// en transaction
                	transaction.Commit();
				}
				con.Close();
			}
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="tbs"></param>
		private void InsertTBs(Dictionary<String, Tuple<String, String>> tbs)
		{
			using (SQLiteConnection con = new SQLiteConnection(_dbpath))
			{
				con.Open();
				using (SQLiteCommand cmd = new SQLiteCommand(con))
				{
					// Prepare transaction
            	SQLiteTransaction transaction = con.BeginTransaction();
            	
	            	cmd.CommandText = 
@"INSERT  OR REPLACE INTO TravelBug(
Id,
Ref,
Name,
DateExport
) VALUES(
@Id,
@Ref,
@Name,
@DateExport
)";
cmd.Parameters.AddWithValue("@Id", "");
cmd.Parameters.AddWithValue("@Ref", "");
cmd.Parameters.AddWithValue("@Name", "");
cmd.Parameters.AddWithValue("@DateExport", "");

					String now = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
					foreach(KeyValuePair<String, Tuple<String, String>> pair in tbs)
					{
						
						// Insert values using preparedcommands
cmd.Parameters["@Id"].Value = pair.Key;
cmd.Parameters["@Ref"].Value = pair.Value.Item1;
cmd.Parameters["@Name"].Value = pair.Value.Item2;
cmd.Parameters["@DateExport"].Value = now;

		                cmd.ExecuteNonQuery();
					}
					
					// en transaction
                	transaction.Commit();
				}
				con.Close();
			}
		}
		
		private void InsertArea(double latmin, double latmax, double lonmin, double lonmax)
		{
			using (SQLiteConnection con = new SQLiteConnection(_dbpath))
			{
				con.Open();
				using (SQLiteCommand cmd = new SQLiteCommand(con))
				{
					// Prepare transaction
            	SQLiteTransaction transaction = con.BeginTransaction();
            	
	            	cmd.CommandText = 
@"INSERT  OR REPLACE INTO Area(
Id,
Coord
) VALUES(
@Id,
@Coord
)";
cmd.Parameters.AddWithValue("@Id", "");
cmd.Parameters.AddWithValue("@Coord", 0.0);
						
					// Insert values using preparedcommands
					cmd.Parameters["@Id"].Value = "latmin";
					cmd.Parameters["@Coord"].Value = latmin;
					cmd.ExecuteNonQuery();
					cmd.Parameters["@Id"].Value = "latmax";
					cmd.Parameters["@Coord"].Value = latmax;
					cmd.ExecuteNonQuery();
					cmd.Parameters["@Id"].Value = "lonmin";
					cmd.Parameters["@Coord"].Value = lonmin;
					cmd.ExecuteNonQuery();
					cmd.Parameters["@Id"].Value = "lonmax";
					cmd.Parameters["@Coord"].Value = lonmax;
					cmd.ExecuteNonQuery();
					
					// en transaction
                	transaction.Commit();
				}
				con.Close();
			}
		}
	}
}
