using System;
using System.Collections.Generic;
using MyGeocachingManager.Geocaching;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using GMap.NET.WindowsForms.Markers;
using System.Data.SQLite;
using GMap.NET.WindowsForms;
using SpaceEyeTools.EXControls;
using System.Net;
using SpaceEyeTools.HMI;
using System.Configuration;
using System.Linq;
using SpaceEyeTools;
using System.IO.Compression;
using System.Text;

namespace MyGeocachingManager.HMI
{
    /// <summary>
    /// Class dedicated to CacheCache.db management
    /// </summary>
    public class CacheCache
    {
        MainWindow _daddy = null;
        CacheDetail _cd = null;
        bool _UpdateChecked = false;

        /// <summary>
        /// path to DB
        /// </summary>
        public String _dbCacheCachePath = "";

           
        /// <summary>
        /// dico of feteched caches
        /// </summary>
        public Dictionary<String, Geocache> _dicoCacheCache = new Dictionary<string, Geocache>();

        /// <summary>
        /// True if cachecache enabled
        /// </summary>
        public bool _bEnableCacheCache = false;

        /// <summary>
        /// True if cachecachefull shall be used for PlayCacheCache
        /// </summary>
        public bool _bUseCacheCacheFull = false;
        private String _UseCacheCacheFullPath = "";
        
        /// <summary>
        /// Worker that fetch DB and update map and other stuffs
        /// </summary>
        public BackgroundWorker _workerCacheCache = new BackgroundWorker();

        // Les valeurs des derniers filtres
        String _defCode = "";
        String _defName = "";
        String _defType = "";
        String _defCompD = "=";
        String _defCompT = "=";
        String _defValD = "-";
        String _defValT = "-";
        String _defBasePath = "CacheCache";
        
        private Form _caller = null;
        
        /// <summary>
        /// CacheCache constructor
        /// </summary>
        /// <param name="daddy">reference to MainWindow</param>
        /// <param name="cd">reference to CacheDetail</param>
        public CacheCache(MainWindow daddy, CacheDetail cd)
        {
            _daddy = daddy;
            _cd = cd;
            _workerCacheCache.WorkerReportsProgress = true;
            _workerCacheCache.WorkerSupportsCancellation = false;
            _workerCacheCache.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
            _workerCacheCache.DoWork += new DoWorkEventHandler(worker_DoWork);
            _workerCacheCache.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            _dbCacheCachePath = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "CacheCache.db";
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Les données du worker
            WorkerCacheCacheData wd = e.UserState as WorkerCacheCacheData;

            if ((!_daddy._bUseFilter) || (_daddy._bUseFilter && _daddy.FilterOrOverride.ToBeDisplayed(wd.cache))) // *** à voir selon les perfos ***
            {
                // On met à jour le status de la cache
                _daddy.ChangeCacheStatusBasedonMGM(wd.cache);

                // On affiche sur la cache
                GMapMarkerImages[] createdmarkers = _daddy.DisplayCacheOnMaps(wd.cache, null, wd.overlay, null);

                // On adapte le niveau de zoom au markere créé
                int iz = GMapMarkerImages.ReturnImageLevelFromZoom(_cd._gmap.Zoom);
                GMapMarkerImages.ChangeImageAccordingToZoom((GMapMarkerImages)(createdmarkers[1]), iz);
            }
        }

        /// <summary>
        /// Create SQL SELECT syntax for specific filter values
        /// </summary>
        /// <param name="uselitedb">if true, used GeocacheLite table instead of GeocacheFull</param>
        /// <param name="code">if null or empty, ignored. Otherwise can use wildcard % or _</param>
        /// <param name="name">if null or empty, ignored. Otherwise can use wildcard % or _</param>
        /// <param name="type">if null or empty, ignored. Otherwise can use wildcard % or _</param>
        /// <param name="compD">comparaison string</param>
        /// <param name="valD">if null or empty or "_", ignored.</param>
        /// <param name="compT">comparaison string</param>
        /// <param name="valT">if null or empty or "_", ignored.</param>
        /// <param name="matrixHoles">List or (D,T) couples</param>
        /// <param name="maxresults">Max result for SQL</param>
        /// <returns>SQL SELECT syntax for specific filter values</returns>
        public String CreateSelectString(bool uselitedb, String code, String name, String type, String compD, String valD, String compT, String valT, List<Tuple<String, String>> matrixHoles, int maxresults)
        {
        	// GeocacheLite(Code, Name, Type, Latitude, Longitude, Difficulty, Terrain)
        	List<String> sqlchunks = new List<String>();
        	
        	if (!String.IsNullOrEmpty(code))
        	{
        		sqlchunks.Add("Code LIKE '" + code + "'");
        		
        	}
        	if (!String.IsNullOrEmpty(name))
        	{
        		sqlchunks.Add("Name LIKE '" + name + "'");
        		
        	}
        	if (!String.IsNullOrEmpty(type) && (type != "-"))
        	{
        		sqlchunks.Add("Type LIKE '" + type + "'");
        		
        	}
        	if (!String.IsNullOrEmpty(valD) && (valD != "-"))
        	{
        		sqlchunks.Add("Difficulty " + compD + " '" + valD + "'");
        	}
        	if (!String.IsNullOrEmpty(valT) && (valT != "-"))
        	{
        		sqlchunks.Add("Terrain " + compT + " '" + valT + "'");
        	}
        	if ((matrixHoles != null) && (matrixHoles.Count != 0))
        	{
        		String s = "(";
        		for(int i=0;i<matrixHoles.Count;i++)
        		{
        			s += "(Difficulty = '" + matrixHoles[i].Item1 + "' AND Terrain = '" + matrixHoles[i].Item2 + "')";
        			if (i != (matrixHoles.Count -1))
        				s += " OR ";
        		}
        		s += ")";
        		sqlchunks.Add(s);
        	}
        	
        	String stm = "SELECT * FROM ";
        	if (uselitedb)
        		stm += "GeocacheLite";
        	else
        		stm += "GeocacheFull";
        	if (sqlchunks.Count != 0)
        	{
        		stm += " WHERE ";
				for(int i=0;i<sqlchunks.Count;i++)
        		{
					stm += sqlchunks[i];
        			if (i != (sqlchunks.Count -1))
        				stm += " AND ";
        		}
        	}
        	if (maxresults != 0)
				stm += " LIMIT " + maxresults.ToString();
	        	
	        return stm;
        }
        
        /// <summary>
        /// Returns database date of generation and age
        /// </summary>
        /// <param name="dt">date of generation</param>
        /// <param name="age">age in days</param>
        /// <param name="count">count of entries</param>
        public void GetGeocacheDateAndAge(ref DateTime dt, ref String age, ref int count)
        {
        	try
        	{
        		dt = new DateTime(1990,1,1);
        		String date = "";
        		age = "";
        		count = -1;
        		String id = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "CacheCache.id";
                if (File.Exists(id))
                {
                    // On récupère la date de notre fichier
                    using (StreamReader stream = new StreamReader(id))
                    {
                        date = stream.ReadLine();
                        Int32.TryParse(stream.ReadLine(), out count);
                    }
                    dt = DateTime.Parse(date);
                    TimeSpan ts = DateTime.Now - dt;
                    age = ((int)(ts.TotalDays)).ToString();
                }
        	}
        	catch(Exception)
        	{
        	}
        }
        
        /// <summary>
        /// Return number of geocaches in database
        /// </summary>
        /// <returns>number of geocaches in database, -1 if error</returns>
        public int GetGeocacheCountInBase(String dbpath)
        {
        	int nb = -1;
        	try
        	{
        		string cs = "URI=file:" + dbpath;
                using (SQLiteConnection con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(con))
                    {
                        // retrieve values
                        String stm = "SELECT count(*) FROM GeocacheLite";
                        using (SQLiteCommand cmd2 = new SQLiteCommand(stm, con))
                        {
                            using (SQLiteDataReader rdr = cmd2.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                    nb = rdr.GetInt32(0);
                                }
                            }
                        }
                    }
                    con.Close();
                }
        	}
        	catch(Exception)
        	{
        	}
        	return nb;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbpath"></param>
        /// <param name="stm"></param>
        /// <returns></returns>
        public List<Geocache> PerformSelect(String dbpath, String stm)
        {
        	String owner = ConfigurationManager.AppSettings["owner"].ToLower();
        	List<Geocache> caches = new List<Geocache>();
        	// On fait le boulot dans le worker
            try
            {
                // On requête depuis la DB
                GMapOverlay overlaybigview = _cd._gmap.Overlays[GMapWrapper.MARKERS];
                string cs = "URI=file:" + dbpath;
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
                                    String code = rdr.GetString(0);
                                    String name = rdr.GetString(1);
                                    String type = rdr.GetString(2);
                                    Double lat = rdr.GetDouble(3);
                                    Double lon = rdr.GetDouble(4);
                                    String D = rdr.GetString(5);
                                    String T = rdr.GetString(6);

                                    Geocache geo = null;
                                    if (_daddy._caches.ContainsKey(code) == false)
                                    {
                                        geo = _daddy.GetEmptyCache(code);
                                        geo._Name = name;
                                        geo._Type = type;
                                        geo._Latitude = lat.ToString().Replace(",", ".");
                                        geo._dLatitude = lat;
                                        geo._Longitude = lon.ToString().Replace(",", ".");
                                        geo._dLongitude = lon;
                                        geo._D = D;
                                        geo._T = T;
                                        geo._Container = "Other";
                                        geo.UpdateDistanceToHome(_daddy.HomeLat, _daddy.HomeLon);

                                        // Update donnée privées
                                        geo.UpdatePrivateData(owner); // fait netre autre les coords

                                        // On le rajoute aux données de MGM s'il n'est pas à ignorer
                                        if (_daddy._ignoreList.ContainsKey(geo._Code) == false)
                                        {
                                            // Ajoute à la liste générale
                                            _daddy._caches.Add(geo._Code, geo);

                                            // complete origin
                                            geo._origin.Add("CACHECACHE");

                                            // Et on met à jour le dico
                                            _dicoCacheCache.Add(code, geo);
                                            
                                            // On met à jour le status de la cache
                							_daddy.ChangeCacheStatusBasedonMGM(geo);
                							
                							// On affiche sur la cache
                							GMapMarkerImages[] createdmarkers = _daddy.DisplayCacheOnMaps(geo, null, overlaybigview, null);
                							
							                // On adapte le niveau de zoom au markere créé
							                int iz = GMapMarkerImages.ReturnImageLevelFromZoom(_cd._gmap.Zoom);
							                GMapMarkerImages.ChangeImageAccordingToZoom((GMapMarkerImages)(createdmarkers[1]), iz);
                                        }
                                    }
                                    else
                                    {
                                    	geo = _daddy._caches[code];
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
 
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // On fait le boulot dans le worker
            try
            {
            	String owner = ConfigurationManager.AppSettings["owner"].ToLower();
            	
                // On récupère la zone
                String latmin = _cd._gmap.ViewArea.Bottom.ToString().Replace(",", ".");
                String latmax = _cd._gmap.ViewArea.Top.ToString().Replace(",", ".");
                String lonmin = _cd._gmap.ViewArea.Left.ToString().Replace(",", ".");
                String lonmax = _cd._gmap.ViewArea.Right.ToString().Replace(",", ".");

                // On requête depuis la DB

                List<Geocache> caches = new List<Geocache>();
                String dbpath = _dbCacheCachePath;
                String dbname = "GeocacheLite";
                int limit = 2000;
                if (_bUseCacheCacheFull)
                {
                	dbpath = _UseCacheCacheFullPath;
               		dbname = "GeocacheFull";
               		limit = _daddy._lastlimitusedindbfilter;
                }
                
                var stm = "SELECT * FROM " + dbname + " WHERE Latitude >= " + latmin + " AND Latitude <= " + latmax + " AND Longitude >= " + lonmin + " AND Longitude <= " + lonmax + " LIMIT " + limit.ToString();
                string cs = "URI=file:" + dbpath;
                if (_bUseCacheCacheFull)
                {
                	caches = MGMDataBase.PerformSelectNoMapsUpdate(_daddy, cs, stm, ref _dicoCacheCache, _daddy._lastuseddbindbfilter);
                }
                else
                {
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
	                                    String code = rdr.GetString(0);
	                                    String name = rdr.GetString(1);
	                                    String type = rdr.GetString(2);
	                                    Double lat = rdr.GetDouble(3);
	                                    Double lon = rdr.GetDouble(4);
	                                    String D = rdr.GetString(5);
	                                    String T = rdr.GetString(6);
	
	                                    if ((_daddy._caches.ContainsKey(code) == false) && (_dicoCacheCache.ContainsKey(code) == false))
	                                    {
	                                        Geocache geo = _daddy.GetEmptyCache(code);
	                                        geo._Name = name;
	                                        geo._Type = type;
	                                        geo._Latitude = lat.ToString().Replace(",", ".");
	                                        geo._dLatitude = lat;
	                                        geo._Longitude = lon.ToString().Replace(",", ".");
	                                        geo._dLongitude = lon;
	                                        geo._D = D;
	                                        geo._T = T;
	                                        geo._Container = "Other";
	                                        geo.UpdateDistanceToHome(_daddy.HomeLat, _daddy.HomeLon);
	
	                                        // Update donnée privées
	                                        geo.UpdatePrivateData(owner); // fait netre autre les coords
	
	                                        // On le rajoute aux données de MGM s'il n'est pas à ignorer
	                                        if (_daddy._ignoreList.ContainsKey(geo._Code) == false)
	                                        {
	                                            // Ajoute à la liste générale
	                                            _daddy._caches.Add(geo._Code, geo);
	
	                                            // complete origin
	                                            geo._origin.Add("CACHECACHE");
	
	                                            // On l'ajoute aux caches à afficher
	                                            caches.Add(geo);
	
	                                            // Et on met à jour le dico
	                                            _dicoCacheCache.Add(code, geo);
	                                        }
	                                    }
	                                }
	                            }
	                        }
	                    }
	                    con.Close();
	                }
                }
                
                // On affiche
                GMapOverlay overlaybigview = _cd._gmap.Overlays[GMapWrapper.MARKERS];
                int i = 0;
                int n = caches.Count;
                foreach (Geocache cache in caches)
                {
                    WorkerCacheCacheData wd = new WorkerCacheCacheData();
                    wd.overlay = overlaybigview;
                    wd.cache = cache;
                    _workerCacheCache.ReportProgress(100 * i++ / n, wd);
                }

                // On peut faire un peu d'affichage dans la ListView maintenant

                // Jointure des waypoints
                //JoinWptsGC();

                // Diverses mises à jour : combo country
                //PostTreatmentLoadCache();

                // On va virer les éventuels elements visuels deja existant
                /*
                _daddy.LvGeocaches.BeginUpdate();
                foreach(var g in caches)
                {
                	EXImageListViewItem item = _daddy.GetVisualEltFromCacheCode(g._Code);
                	if (item != null)
                		_daddy.RemoveVisualElt(item);
                }
                _daddy.LvGeocaches.EndUpdate();
                */
               
                // Mise à jour de la liste
                List<EXImageListViewItem> listvi = _daddy.BuildListViewCache(caches);

                // Population de la liste
                _daddy.PopulateListViewCacheAddList(listvi);

                _cd.Text = String.Format(_daddy.GetTranslator().GetString("lblCacheCacheStatus"),n , _dicoCacheCache.Count);
            }
            catch (Exception ex)
            {
            	_cd.Text = ex.Message;
            }
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // sortie du mutex            
        }

        /// <summary>
        /// Run background worker that will populate from cache if not busy
        /// </summary>
        /// <param name="launcher">identify who launched the game</param>
        public void PlayCacheCache(WorkerCacheCacheData.WhoLaunched launcher)
        {
            if (_bEnableCacheCache)
            {
                // Si on bosse déjà on ne fait rien
                if (_workerCacheCache.IsBusy == false)
                {
                    // On bulle, donc on lance le worker
                    _workerCacheCache.RunWorkerAsync(launcher);
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if there is an update downloading</returns>
        private bool CheckForUpdate(Form caller)
        {

            _UpdateChecked = true;
            try
            {
                String id = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "CacheCache.id";
                String urlwithupdate = "";
                String localversion = "";
                String newversion = "";
                if (File.Exists(id))
                {
                    // On récupère la date de notre fichier
                    using (StreamReader stream = new StreamReader(id))
                    {
                        newversion = stream.ReadLine();
                        newversion = newversion.Replace("\r\n", "");
                        newversion = newversion.Replace("\n", "");
                        localversion = newversion;
                        stream.Close();
                    }

                    if (_daddy.GetInternetStatus())
                    {
                        String urlupdate = ConfigurationManager.AppSettings["urlupdate"];
                        // Il se peut qu'on ait 2 URL (le site principal et le site de backup)
                        List<string> urls = urlupdate.Split(';').ToList<string>();
                        foreach (String u in urls)
                        {
                            // On ne ping que la première URL qui est le site officiel
                            String url = u + "/db2/CacheCache.id";
                            try
                            {
                                // On teste le téléchargement de CacheCache.id pour s'assurer que le serveur est vivant
                                WebProxy proxy = _daddy.GetProxy();
                                String rep = MyTools.GetRequest(new Uri(url), proxy, 200);
                                rep = rep.Replace("\r\n", "\n");
                                List<string> data = rep.Split('\n').ToList<string>();
                                if (data.Count >= 3)
                                {
                                    // Le fichier semble valide
                                    String date = data[0];
                                    if (String.Compare(date, newversion) > 0)
                                    {
                                        // On a plus récent
                                        newversion = date;
                                        urlwithupdate = u;
                                    }
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }

                        // A-t'on trouvé un truc plus récent ?
                        if (urlwithupdate != "")
                        {
                            // Oui !!!
                            DialogResult dialogResult = MyMessageBox.Show(
                                String.Format(_daddy.GetTranslator().GetStringM("AskDownloadNewerDB"),localversion , newversion),
                                          _daddy.GetTranslator().GetString("AskDownloadDBTitle"),
                                          MessageBoxIcon.Question, _daddy.GetTranslator());
                            if (dialogResult == DialogResult.Yes)
                            {
                                // Ok on télécharge
                                DownloadDBImpl(caller, urlwithupdate);
                                return true;
                            }
                        }
                        else
                        {
                        	// Est-ce que notre base est vieille... (> 1 mois)
                        	DateTime local = MyTools.ParseDate(localversion);
                        	TimeSpan ts = DateTime.Now - local;
                        	if (ts.TotalDays > 30)
                        	{
                        		// Elle est vieille !
                        		// on va proposer de la régénérer
                        		DialogResult dialogResult = MyMessageBox.Show(
                                String.Format(_daddy.GetTranslator().GetStringM("AskGenerateNewDB"),localversion , newversion),
                                          _daddy.GetTranslator().GetString("AskGenerateNewDBTitle"),
                                          MessageBoxIcon.Question, _daddy.GetTranslator());
	                            if (dialogResult == DialogResult.Yes)
	                            {
	                                // Ok on génère
	                                GenerateNewDatabase(caller);
	                                return true;
	                            }
                        	}
                        }
                    }
                }
                else
                {
                    // Il n'existe pas... Bizarre, on n'aurait jamais du arriver ici
                }
                return false;
            }
            catch(Exception ex)
            {
            	_daddy.Log(ex.Message);
            	return false;
            }
        }
        
        private void GenerateNewDatabase(Form caller)
        {
            String tmpDirectory = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "tmp";
            try
        	{
        		// On prépare le répertoire temporaire
        		if (Directory.Exists(tmpDirectory))
                {
        			try
                    {
                        MyTools.DeleteDirectory(tmpDirectory, true);
                    }
                    catch (Exception)
                    {
                    }
        		}
        		Directory.CreateDirectory(tmpDirectory);
        		
        		// on télécharge le fichier
        		String url = "http://sd-2.archive-host.com/membres/up/81061952863402030/GeoCaches.zip";
				string f = tmpDirectory + Path.DirectorySeparatorChar + "GeoCaches.zip";
				HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
				objRequest.Proxy = _daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
				HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
				using (Stream output = File.OpenWrite(f))
				using (Stream input = objResponse.GetResponseStream())
				{
					byte[] buffer = new byte[8192];
					int bytesRead;
					while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
					{
						output.Write(buffer, 0, bytesRead);
					}
					output.Close();
					input.Close();
					
					// Unzip zip file
					ZipFile.ExtractToDirectory(f, tmpDirectory);
                    int totalcachecache = 0;

                    String newbd = tmpDirectory + Path.DirectorySeparatorChar + "CacheCache.db";
					string cs = "URI=file:" + newbd;
                    using (SQLiteConnection con = new SQLiteConnection(cs))
                    {
                        con.Open();
                        using (SQLiteCommand cmd = new SQLiteCommand(con))
                        {
                            cmd.CommandText = "DROP TABLE IF EXISTS GeocacheLite";
                            cmd.ExecuteNonQuery();

                            // Create table
                            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS GeocacheLite(
									Code TEXT PRIMARY KEY, Name TEXT, Type TEXT,
									Latitude DOUBLE, Longitude DOUBLE,
									Difficulty TEXT, Terrain TEXT)";
                            cmd.ExecuteNonQuery();

                            
                            OV2Reader ov2 = new OV2Reader();
                            String fln = "";

                            fln = Path.DirectorySeparatorChar + "GC CitoEvent.ov2";
                            totalcachecache += ov2.ProcessFileSuperPP(con, cmd, tmpDirectory + fln, "Cache In Trash Out Event");
                            fln = Path.DirectorySeparatorChar + "GC Earthcache.ov2";
                            totalcachecache += ov2.ProcessFileSuperPP(con, cmd, tmpDirectory + fln, "Earthcache");
                            fln = Path.DirectorySeparatorChar + "GC LetterBox.ov2";
                            totalcachecache += ov2.ProcessFileSuperPP(con, cmd, tmpDirectory + fln, "Letterbox Hybrid");
                            fln = Path.DirectorySeparatorChar + "GC Multi.ov2";
                            totalcachecache += ov2.ProcessFileSuperPP(con, cmd, tmpDirectory + fln, "Multi-cache");
                            fln = Path.DirectorySeparatorChar + "GC Mystery.ov2";
                            totalcachecache += ov2.ProcessFileSuperPP(con, cmd, tmpDirectory + fln, "Unknown Cache");
                            fln = Path.DirectorySeparatorChar + "GC Traditional.ov2";
                            totalcachecache += ov2.ProcessFileSuperPP(con, cmd, tmpDirectory + fln, "Traditional Cache");
                            fln = Path.DirectorySeparatorChar + "GC Virtual.ov2";
                            totalcachecache += ov2.ProcessFileSuperPP(con, cmd, tmpDirectory + fln, "Virtual Cache");
                            fln = Path.DirectorySeparatorChar + "GC Webcam.ov2";
                            totalcachecache += ov2.ProcessFileSuperPP(con, cmd, tmpDirectory + fln, "Webcam Cache");
                            fln = Path.DirectorySeparatorChar + "GC Wherigo.ov2";
                            totalcachecache += ov2.ProcessFileSuperPP(con, cmd, tmpDirectory + fln, "Wherigo Cache");
                        }
                        con.Close();
                    }
                    // Le fichier id
                    System.IO.StreamWriter file = new System.IO.StreamWriter(tmpDirectory + Path.DirectorySeparatorChar + "CacheCache.id", false, Encoding.Default);
					String sdate = DateTime.Now.ToString("yyyy-MM-ddTHH:mmZ");
                    file.WriteLine(sdate);
					file.WriteLine(totalcachecache.ToString());
					file.WriteLine("1.0");
					file.Close();
							
					// Maintenant on déplace
					String tmpdb = _dbCacheCachePath + ".tmp";
					if (File.Exists(tmpdb))
						File.Delete(tmpdb);
					File.Move(newbd, tmpdb);
							
					String id = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "CacheCache.id";
					String tmpid = id + ".tmp";
					if (File.Exists(tmpid))
						File.Delete(tmpid);
					File.Move(tmpDirectory + Path.DirectorySeparatorChar + "CacheCache.id", tmpid);
							
					// On efface et on renomme
					if (File.Exists(_dbCacheCachePath))
						File.Delete(_dbCacheCachePath);
					File.Move(tmpdb, _dbCacheCachePath);
							
					if (File.Exists(id))
						File.Delete(id);
					File.Move(tmpid, id);
						
					
                    if (Directory.Exists(tmpDirectory))
                    {
                        try
                        {
                            MyTools.DeleteDirectory(tmpDirectory, true);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    // All right
                    _daddy.MsgActionOk(caller, _daddy.GetTranslator().GetStringM("LblDatabaseOK"));
                }
        	}
        	catch(Exception ex)
        	{
        		_daddy.ShowException("", _daddy.GetTranslator().GetStringM("ErrorCacheCacheGeneration"), ex);
                if (Directory.Exists(tmpDirectory))
                {
                    try
                    {
                        MyTools.DeleteDirectory(tmpDirectory, true);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private void DownloadDBImpl(Form caller, String url)
        {
        	if (_cd.btnCacheCacheConfigure != null)
            	_cd.btnCacheCacheConfigure.Enabled = false;
            // Et on télécharge la base
            WebClient client = new WebClient();
            client.Proxy = _daddy.GetProxy();
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            String localfile = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "CacheCache.zip";
            if (File.Exists(localfile))
                File.Delete(localfile);
            
            // On renseigne le caller
            _caller = caller;
            client.DownloadFileAsync(new Uri(url + "/db2/CacheCache.zip"), localfile);
        }

        private void DownloadDB(Form caller)
        {
            // Async mode
            String urlupdate = ConfigurationManager.AppSettings["urlupdate"];
            // Il se peut qu'on ait 2 URL (le site principal et le site de backup)
            List<string> urls = urlupdate.Split(';').ToList<string>();
            foreach(String u in urls)
            {
                // On ne ping que la première URL qui est le site officiel
                String url = u + "/db2/CacheCache.id";
                try
                {
                    // On teste le téléchargement de CacheCache.id pour s'assurer que le serveur est vivant
                    WebProxy proxy = _daddy.GetProxy();
                    String rep = MyTools.GetRequest(new Uri(url), proxy, 200);

                    _daddy.MsgActionWarning(caller, _daddy.GetTranslator().GetStringM("LblDBInfo") + rep);
                    DownloadDBImpl(caller, u);
                    return;
                }
                catch(Exception)
                {

                }
            }

            // Si on est là, on a tout merdé
            _daddy.MsgActionError(caller, _daddy.GetTranslator().GetStringM("LblErrNoDBAvailableOnline"));
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            if ((!e.Cancelled) && (e.Error == null))
            {
                // On dézippe le fichier
                String unpackDirectory = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar;
                String localfile = unpackDirectory + "CacheCache.zip";
                try
	            {
                    if (File.Exists(unpackDirectory + "CacheCache.db"))
                        File.Delete(unpackDirectory + "CacheCache.db");
                    if (File.Exists(unpackDirectory + "CacheCache.id"))
                        File.Delete(unpackDirectory + "CacheCache.id");

                    ZipFile.ExtractToDirectory(localfile, unpackDirectory);
	            }
	            catch (Exception ex)
	            {
	            	_daddy.MsgActionError(_caller??_daddy, _daddy.GetTranslator().GetStringM("LblErrUnzipingDB") + "\r\n" + ex.Message);
	            	_caller = null;
                    File.Delete(localfile);
                    if (_cd.btnCacheCacheConfigure != null)
                    	_cd.btnCacheCacheConfigure.Enabled = true;
                    return;
	            }
	            
                // Delete zip file
                File.Delete(localfile);

                // All right
                _daddy.MsgActionOk(_caller??_daddy, _daddy.GetTranslator().GetStringM("LblDatabaseOK"));
            }
            else
            {
                _daddy.MsgActionError(_caller??_daddy, _daddy.GetTranslator().GetStringM("LblErrDownloadingDB"));
            }
            _caller = null;
            if (_cd.btnCacheCacheConfigure != null)
            	_cd.btnCacheCacheConfigure.Enabled = true;
        }
        
        /// <summary>
        /// Return true if database is available
        /// Alos perofrms update check and propose to download if newer or missing DB
        /// </summary>
        /// <returns>True if DB available</returns>
        public bool CheckDatabaseAvailability(Form caller)
        {
        	if (File.Exists(_dbCacheCachePath))
            {
                // On regarde 1 fois par lancement si la base est à jour
                if (!_UpdateChecked)
                {
                	if (CheckForUpdate(caller)) // if true, we are downloading an update
                		return false;
                	
                }
				
            	// On peut activer cool
            	return true;
            }
            else
            {
                // Message d'erreur, on invite à télécharger la base
                if (_daddy.GetInternetStatus())
                {
                    DialogResult dialogResult = MyMessageBox.Show(_daddy.GetTranslator().GetStringM("AskDownloadDBNet"),
                                      _daddy.GetTranslator().GetString("AskDownloadDBTitle"),
                                      MessageBoxIcon.Question, _daddy.GetTranslator());
                    if (dialogResult == DialogResult.Yes)
                    {
                        // Ok on télécharge
                        DownloadDB(caller);
                    }
                }
                else
                {
                    _daddy.MsgActionError(caller, _daddy.GetTranslator().GetStringM("AskDownloadDBNoNet"));
                }
                return false;
            }
        }
        
        /// <summary>
        /// Toggle cachecache function
        /// </summary>
        public void ToggleCacheCache()
        {
            if (_bEnableCacheCache)
            {
                // On désactive tout simplement
                _bEnableCacheCache = false;
            }
            else
            {
            	// On propose de choisir une base détaillée
            	String dbfullpath = _daddy.HMIToSelectDB("DBFullPath", "", false, true);
            	if (dbfullpath != "")
            	{
	            	if (dbfullpath == _dbCacheCachePath)
	            	{
	            		_bEnableCacheCache = CheckDatabaseAvailability(_daddy._cacheDetail);
	            		_bUseCacheCacheFull = false;
	            	}
	            	else
	            	{
	            		_bEnableCacheCache = true;
	                    _bUseCacheCacheFull = true;
	                    _UseCacheCacheFullPath = dbfullpath;
	            	}
            	}
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="daddy"></param>
        /// <param name="listDBNames"></param>
        /// <param name="listDBPath"></param>
        /// <param name="verbose"></param>
        /// <returns></returns>
        public static bool GetAvailableDB(MainWindow daddy, out List<String> listDBNames, out List<String> listDBPath, bool verbose = true)
        {
        	// On va chercher toutes les .db dans DataPath/DB
            String dbdatapath = daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "DB" + Path.DirectorySeparatorChar;
            listDBNames = new List<string>();
            listDBPath = new List<string>();
            
            try
            {
            	if (!Directory.Exists(dbdatapath))
                    Directory.CreateDirectory(dbdatapath);
                
	            // On cherche d'autres bases
	            string[] filePaths = Directory.GetFiles(dbdatapath, "*.db", SearchOption.AllDirectories);
	            foreach (string filename in filePaths)
	        	{
	            	var name = Path.GetFileNameWithoutExtension(filename);
	            	if (name != "CacheCache")
	            	{
	            		listDBNames.Add(name);
	            		listDBPath.Add(filename);
	            	}
	            }
	            
	            if (listDBNames.Count != 0)
	            	return true;
	            else
	            {
	            	// LblNoAvailableDB
	            	if (verbose)
	            		daddy.MsgActionError(daddy, daddy.GetTranslator().GetString("LblNoAvailableDB"));
	            	return false;
	            }
            }
            catch(Exception)
            {
            	listDBNames = new List<string>();
            	listDBPath = new List<string>();
            	if (verbose)
	            	daddy.MsgActionError(daddy, daddy.GetTranslator().GetString("LblNoAvailableDB"));
	            return false;
            }
        }
        /// <summary>
        /// Execute a custom filter on database with user input information
        /// </summary>
        public void FilterDatabase()
        {
        	try
            {
				if (!CheckDatabaseAvailability(_daddy))
					return;
            	
            	List<ParameterObject> lst = new List<ParameterObject>();
	            List<String> lstcompare = new List<string>();
	            lstcompare.Add(">=");
	            lstcompare.Add("=");
	            lstcompare.Add("<=");
	            
	            List<String> lstvals = new List<string>();
	            lstvals.AddRange(new String[] {
				"-",
				"1",
				"1.5",
				"2",
				"2.5",
				"3",
				"3.5",
				"4",
				"4.5",
				"5"});
	            
	            List<String> types = GeocachingConstants.GetSupportedCacheTypes();
	            types.Insert(0, "-");
	            
	            ParameterObject po;
	            po = new ParameterObject(ParameterObject.ParameterType.String, _defCode, "code", _daddy.GetTranslator().GetString("CacheCacheFilterCode"));
	            lst.Add(po);
	            po = new ParameterObject(ParameterObject.ParameterType.String, _defName, "name", _daddy.GetTranslator().GetString("CacheCacheFilterName"));
	            lst.Add(po);
	            po = new ParameterObject(ParameterObject.ParameterType.List, types, "lsttype", _daddy.GetTranslator().GetString("CacheCacheFilterType"));
	            po.DefaultListValue = _defType;
            	lst.Add(po);
            	po = new ParameterObject(ParameterObject.ParameterType.List, lstcompare, "lstcompareD", _daddy.GetTranslator().GetString("CacheCacheFilterDComp"));
            	po.DefaultListValue = _defCompD;
	            lst.Add(po);
	            po = new ParameterObject(ParameterObject.ParameterType.List, lstvals, "lstvalsD", _daddy.GetTranslator().GetString("CacheCacheFilterDVals"));
	            po.DefaultListValue = _defValD;
	            lst.Add(po);
	            po = new ParameterObject(ParameterObject.ParameterType.List, lstcompare, "lstcompareT", _daddy.GetTranslator().GetString("CacheCacheFilterTComp"));
	            po.DefaultListValue = _defCompT;
	            lst.Add(po);
	            po = new ParameterObject(ParameterObject.ParameterType.List, lstvals, "lstvalsT", _daddy.GetTranslator().GetString("CacheCacheFilterTVals"));
	            po.DefaultListValue = _defValT;
	            lst.Add(po);
	            	            
	            // Uniquement si internet et gc.com configurés
	            bool bMatrixEnabled = false;
	            if (_daddy.GetInternetStatus())
				{
					_daddy.UpdateHttpDefaultWebProxy();
	                // On checke que les L/MDP soient corrects
	                // Et on récupère les cookies au passage
	                CookieContainer cookieJar = _daddy.CheckGCAccount(true, false);
	                if (cookieJar != null)
	                {
	            		bMatrixEnabled = true;
	                }
	            }
	            lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "holes", _daddy.GetTranslator().GetString("CacheCacheFilterMatrixHoles"), !bMatrixEnabled));
	            lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "otherfilter", _daddy.GetTranslator().GetString("CacheCacheFilterExecuteCurrentFilter")));
	            lst.Add(new ParameterObject(ParameterObject.ParameterType.Int, 10000, "maxresults", _daddy.GetTranslator().GetString("CacheCacheFilterMaxResults")));
	            
	            // On va chercher toutes les .db dans DataPath/DB
	            List<String> listDBNames = new List<string>();
	            List<String> listDBPath = new List<string>();
	            GetAvailableDB(_daddy, out listDBNames, out listDBPath, false);
	            
	            // On ajoute CacheCache.db
	            listDBNames.Insert(0, "CacheCache");
	            listDBPath.Insert(0, _dbCacheCachePath);
	          
	            po = new ParameterObject(ParameterObject.ParameterType.List, listDBNames, "lstdbnames", _daddy.GetTranslator().GetString("CacheCacheFilterBase"));
	            lst.Add(po);
	            if (!listDBNames.Contains(_defBasePath))
	            	_defBasePath = "CacheCache";
	            po.DefaultListValue = _defBasePath;
	            
	            String date = "";
	            String days = "";
	            int count = -1;
	            DateTime dt = DateTime.Now;
	            GetGeocacheDateAndAge(ref dt, ref days, ref count);
	            date = dt.ToString("dd MMMM yyyy HH:mm");
	            lst.Add(new ParameterObject(ParameterObject.ParameterType.Label, "", "infonb", String.Format(_daddy.GetTranslator().GetString("CacheCacheDatabaseInfoNb"), count)));
	            lst.Add(new ParameterObject(ParameterObject.ParameterType.Label, "", "infodate", String.Format(_daddy.GetTranslator().GetString("CacheCacheDatabaseInfoDate"), date, days)));
	            
	
	            ParametersChanger changer = new ParametersChanger();
	            changer.Title = _daddy.GetTranslator().GetString("filterOnCacheCacheToolStripMenuItem");
	            changer.BtnCancel = _daddy.GetTranslator().GetString("BtnCancel");
	            changer.BtnOK = _daddy.GetTranslator().GetString("BtnOk");
	            changer.ErrorFormater = _daddy.GetTranslator().GetString("ErrWrongParameter");
	            changer.ErrorTitle = _daddy.GetTranslator().GetString("Error");
	            changer.Parameters = lst;
	            changer.Font = _daddy.Font;
	            changer.Icon = _daddy.Icon;
	
	            if (changer.ShowDialog() == DialogResult.OK)
	            {
	            	String code = changer.Parameters[0].Value;
	            	_defCode = code;
	            	String name = changer.Parameters[1].Value;
	            	_defName = name;
	            	String type = changer.Parameters[2].Value;
	            	_defType = type;
	            	String comparestringD = changer.Parameters[3].Value;
	            	_defCompD = comparestringD;
	            	String valD = changer.Parameters[4].Value;
	            	_defValD = valD;
	                String comparestringT = changer.Parameters[5].Value;
	                _defCompT = comparestringT;
	            	String valT = changer.Parameters[6].Value;
	            	_defValT = valT;
	            	
	            	List<Tuple<String, String>> matrixHoles = null;
	            	bool bExecuteCurrentFilter = false;
	            	
	            	if (bMatrixEnabled)
	            	{
	                	bool matrixHole =  bool.Parse(changer.Parameters[7].Value);
	                	if (matrixHole)
	                		matrixHoles = MatrixDT.GetMatrixHoles(_daddy);
	                	bExecuteCurrentFilter =  bool.Parse(changer.Parameters[8].Value);
	            	}
	            	else
	            	{
	            		bExecuteCurrentFilter =  bool.Parse(changer.Parameters[7].Value);
	            	}
	            	int maxresults = Int32.Parse(changer.Parameters[9].Value);
	            	
	            	String dbpath = _dbCacheCachePath;
	            	bool uselitedb = true;
	            	String dbpathfull = "";
	            	String dbname = changer.Parameters[10].Value;
	            	if (dbname == "CacheCache")
	            	{
	            		uselitedb = true;
	            	}
	            	else
	            	{
	            		uselitedb = false;
	            		_defBasePath = dbname;
	            		// on récupère le chemin
	            		int pos = listDBNames.IndexOf(dbname);
	            		dbpathfull = listDBPath[pos];
	            	}
	            		            	
	                String sql = CreateSelectString(uselitedb, code, name, type, comparestringD, valD, comparestringT, valT, matrixHoles, maxresults);
	                 
	                _daddy._ThreadProgressBarTitle = _daddy.GetTranslator().GetString("CacheCacheFilterWorking");
                	_daddy.CreateThreadProgressBar();
                	
                	// Les caches retournées sont déjà ajoutées à MGM, c'est cool
                	List<Geocache> caches = null;
                	if (uselitedb)
                		caches = PerformSelect(dbpath, sql);
                	else
                	{
                		MGMDataBase db = new MGMDataBase(_daddy, dbpathfull);
                		caches = db.PerformSelect(sql, dbname);
                	}
	                
	                // On récupère la liste des listview items du résultat
	                List<EXListViewItem> forcedList = new List<EXListViewItem>();
	                List<Geocache> cachestocreate = new List<Geocache>();
	                _daddy.LvGeocaches.BeginUpdate();
	                foreach(Geocache geo in caches)
	                {
	                	// Soit on est déjà dans MGM et on a un listviewitem (forcedList)
	                	// soit il faut créer l'élément graphique (cachestocreate)
	                	EXImageListViewItem item = _daddy.GetVisualEltFromCacheCode(geo._Code);
	                	if (item != null)
	                	{
	                		// on va vérifier : si l'élément graphique existe MAIS qu'il est associé à une cache issue seulement de CACHECACHE *ET* que là on est en train
	                		// d'utiliser CacheCacheFull, alors on va recréer cet élément graphique, car il sera plus détaillé
	                		if (uselitedb)
	                		{
	                			// Ce qui existe ne peut pas être pire
	                			forcedList.Add(item);
	                		}
	                		else
	                		{
	                			// on regarde la cache qui existe dans MGM
	                			String placed = item.SubItems[_daddy._ID_LVPlaced].Text;
	                			if (placed == "")
	                			{
	                				// ok la seule occurence de cette cache vient de CACHECACHE,
	                				// Et là on a une jolie cache détaillée, on va donc récréer l'élement graphique
	                				_daddy.RemoveVisualElt(item);
	                				cachestocreate.Add(geo);
	                			}
	                			else
	                			{
	                				// Bon on laisse
	                				forcedList.Add(item);
	                			}
	                		}
	                	}
	                	else
	                		cachestocreate.Add(geo);
	                }
	                _daddy.LvGeocaches.EndUpdate();
	                
	                if (cachestocreate.Count != 0)
	                {
	                	// soit on doit en créer un élément graphique
	                	List<EXImageListViewItem> listvi = _daddy.BuildListViewCache(cachestocreate);
	                	forcedList.AddRange(listvi);
	                }
	                
	                // On affiche cette liste
	                if (!bExecuteCurrentFilter)
	                	_daddy.PopulateListViewCache(forcedList); // C'est simple, on affiche la liste résultat
	                else
	                {
	                	// On fait un filtre AND sur ce résultat et le filtre actuel
	                	_daddy.UpdateFilter();
	                	
	                	CacheFilter flt = _daddy.Filter;
	                	List<EXListViewItem> forcedListfinal = new List<EXListViewItem>();
	                	foreach (EXListViewItem item in forcedList)
		                {
		                    Geocache cache = _daddy._caches[item.Text];
		                    if (flt.ToBeDisplayed(cache))
		                    {
		                        forcedListfinal.Add(item);
		                    }
		                }
		                _daddy.PopulateListViewCache(forcedListfinal);
	                }
	                
	                _daddy.KillThreadProgressBar();
	            }
            	
            }
            catch (Exception ex)
            {
                _daddy.KillThreadProgressBar();
                _daddy.ShowException("", _daddy.GetTranslator().GetString("filterOnCacheCacheToolStripMenuItem"), ex);
            }
        }
    }

    /// <summary>
    /// Data for CacheCache worker
    /// </summary>
    public class WorkerCacheCacheData
    {
        /// <summary>
        /// Identify who launched the worker
        /// </summary>
        public enum WhoLaunched
        {
            /// <summary>
            /// Map dragged event
            /// </summary>
            MAPDRAG,
            /// <summary>
            /// Zoom changed event
            /// </summary>
            ZOOMCHANGE
        };

        /// <summary>
        /// Cache to display
        /// </summary>
        public Geocache cache = null;

        /// <summary>
        /// Overlay to display on
        /// </summary>
        public GMapOverlay overlay = null;
    }
}
