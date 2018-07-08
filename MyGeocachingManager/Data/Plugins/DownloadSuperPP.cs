using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Configuration;
using System.Collections;
using System.Net;
using MyGeocachingManager.Geocaching;
using System.Data.SQLite;
using SpaceEyeTools;
using SpaceEyeTools.HMI;
using System.IO.Compression;

namespace MyGeocachingManager
{
	public class DownloadSuperPP : IScriptV2
	{
		private MainWindow _Daddy = null;
        public string Name { get { return "Convertisseur SuperPP"; } }
		public string Description { get {return "Télécharger automatiquement les fichiers de SuperPP et crée des GPX vides";}}
        public string Version { get { return "1.6"; } }
        public string MinVersionMGM { get { return "4.0.0.0.RC1"; } }
		
		public void Initialize(MainWindow daddy)
		{
			_Daddy = daddy;
		}

        public Dictionary<String, String> Functions
        {
            get
            {
                Dictionary<String, String> dico = new Dictionary<string, string>();
                dico.Add("Récupérer et créer les fichiers", "DoIt");
                dico.Add("Informations", "GetInfos");
                return dico;
            }
        }

        public void GetInfos()
        {
            String s;
            s = "Version : " + this.Version + "\r\n";
            s += "Nom : " + this.Name + "\r\n";
            s += "Description : " + this.Description + "\r\n";
            s += "Version minimale de MGM : " + this.MinVersionMGM;

            _Daddy.MSG(s);
        }

		public bool DoIt()
		{
			if (_Daddy != null)
			{
                
                // Create a random directory (and delete existing one)
                String exePath = Path.GetDirectoryName(Application.ExecutablePath);
                string tmpDirectory = exePath + Path.DirectorySeparatorChar + "Data\\_EmptyOV2ForFrance";
                if (Directory.Exists(tmpDirectory))
                {
                    try
                    {
                        Directory.Delete(tmpDirectory, true);
                    }
                    catch (Exception)
                    {
                    }
                }
                Directory.CreateDirectory(tmpDirectory);

                // Create SuperPP directory (and delete existing one)
                string superPPDirectory = exePath + Path.DirectorySeparatorChar + "Data\\_EmptyGPXForFrance";
                if (Directory.Exists(superPPDirectory))
                {
                    try
                    {
                        Directory.Delete(superPPDirectory, true);
                    }
                    catch (Exception)
                    {
                    }
                }
                Directory.CreateDirectory(superPPDirectory);


				List<ParameterObject> lst = new List<ParameterObject>();
				lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "cachecache", "Créer CacheCache.db"));
				lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "gpx", "Créer un GPX par type"));
				lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "ggz", "Créer un GGZ global"));
				

                ParametersChanger changer = new ParametersChanger();
                changer.Title = "Choix des opérations à effectuer";
                changer.BtnCancel = _Daddy.GetTranslator().GetString("BtnCancel");
                changer.BtnOK = _Daddy.GetTranslator().GetString("BtnOk");
                changer.ErrorFormater = _Daddy.GetTranslator().GetString("ErrWrongParameter");
                changer.ErrorTitle = _Daddy.GetTranslator().GetString("Error");
                changer.Parameters = lst;
                changer.Font =_Daddy.Font;
                changer.Icon = _Daddy.Icon;
				
				if (changer.ShowDialog() == DialogResult.OK)
                {
					try
					{
						bool bcache = (lst[0].Value == "True");
						bool bgpx = (lst[1].Value == "True");
						bool bggz = (lst[2].Value == "True");
						
						// Download SuperPP zip (in tmpdirectory
						String url = "http://sd-2.archive-host.com/membres/up/81061952863402030/GeoCaches.zip";
						string f = tmpDirectory + Path.DirectorySeparatorChar + "GeoCaches.zip";
						HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
						objRequest.Proxy = _Daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
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
						}

						// Unzip zip file
						ZipFile.ExtractToDirectory(f, tmpDirectory);
						
						// Initialise DB if not existing
						String fln = "";
						String cachecacheprefix = "CacheCache";
						int totalcachecache = 0;
						fln = superPPDirectory + Path.DirectorySeparatorChar + cachecacheprefix;
						Directory.CreateDirectory(fln);
						fln = Path.DirectorySeparatorChar + cachecacheprefix + Path.DirectorySeparatorChar + cachecacheprefix + ".db";
						string cs = "URI=file:" + superPPDirectory + fln;					
						using ( SQLiteConnection con = new SQLiteConnection(cs))
						{
							con.Open();
							using (SQLiteCommand cmd = new SQLiteCommand(con))
							{
								// Drop table
								if (bcache)
								{
									cmd.CommandText = "DROP TABLE IF EXISTS GeocacheLite";
									cmd.ExecuteNonQuery();
								
									// Create table
									cmd.CommandText = @"CREATE TABLE IF NOT EXISTS GeocacheLite(
											Code TEXT PRIMARY KEY, Name TEXT, Type TEXT,
											Latitude DOUBLE, Longitude DOUBLE,
											Difficulty TEXT, Terrain TEXT)";
									cmd.ExecuteNonQuery();
								}
								
								// Convert OV2
								_Daddy._ThreadProgressBarTitle = "";
								_Daddy.CreateThreadProgressBar();
								OV2Reader ov2 = new OV2Reader();
								List<Geocache> c = new List<Geocache>();
								List<Geocache> cfull = new List<Geocache>();

								fln = Path.DirectorySeparatorChar + "GC CitoEvent.ov2";
								if (bcache)
									totalcachecache += ov2.ProcessFileSuperPP(con, cmd, tmpDirectory + fln, "Cache In Trash Out Event");
								if (bgpx || bggz)
									c = ov2.ProcessFile(_Daddy, tmpDirectory + fln, "Cache In Trash Out Event", true);
								if (bgpx)
									_Daddy.ExportGPXStreamed(superPPDirectory + fln.Replace(".ov2", ".gpx"), c, null, new DateTime(2000, 1, 1, 0, 0, 1));
								cfull.AddRange(c);

								fln = Path.DirectorySeparatorChar + "GC Earthcache.ov2";
								if (bcache)
									totalcachecache += ov2.ProcessFileSuperPP(con, cmd, tmpDirectory + fln, "Earthcache");
								if (bgpx || bggz)
									c = ov2.ProcessFile(_Daddy, tmpDirectory + fln, "Earthcache", true);
								if (bgpx)
									_Daddy.ExportGPXStreamed(superPPDirectory + fln.Replace(".ov2", ".gpx"), c, null, new DateTime(2000, 1, 1, 0, 0, 1));
								cfull.AddRange(c);

								fln = Path.DirectorySeparatorChar + "GC LetterBox.ov2";
								if (bcache)
									totalcachecache += ov2.ProcessFileSuperPP(con, cmd, tmpDirectory + fln, "Letterbox Hybrid");
								if (bgpx || bggz)
									c = ov2.ProcessFile(_Daddy, tmpDirectory + fln, "Letterbox Hybrid", true);
								if (bgpx)
									_Daddy.ExportGPXStreamed(superPPDirectory + fln.Replace(".ov2", ".gpx"), c, null, new DateTime(2000, 1, 1, 0, 0, 1));
								cfull.AddRange(c);

								fln = Path.DirectorySeparatorChar + "GC Multi.ov2";
								if (bcache)
									totalcachecache += ov2.ProcessFileSuperPP(con, cmd, tmpDirectory + fln, "Multi-cache");
								if (bgpx || bggz)
									c = ov2.ProcessFile(_Daddy, tmpDirectory + fln, "Multi-cache", true);
								if (bgpx)
									_Daddy.ExportGPXStreamed(superPPDirectory + fln.Replace(".ov2", ".gpx"), c, null, new DateTime(2000, 1, 1, 0, 0, 1));
								cfull.AddRange(c);

								fln = Path.DirectorySeparatorChar + "GC Mystery.ov2";
								if (bcache)
									totalcachecache += ov2.ProcessFileSuperPP(con, cmd, tmpDirectory + fln, "Unknown Cache");
								if (bgpx || bggz)
									c = ov2.ProcessFile(_Daddy, tmpDirectory + fln, "Unknown Cache", true);
								if (bgpx)
									_Daddy.ExportGPXStreamed(superPPDirectory + fln.Replace(".ov2", ".gpx"), c, null, new DateTime(2000, 1, 1, 0, 0, 1));
								cfull.AddRange(c);

								fln = Path.DirectorySeparatorChar + "GC Traditional.ov2";
								if (bcache)
									totalcachecache += ov2.ProcessFileSuperPP(con, cmd, tmpDirectory + fln, "Traditional Cache");
								if (bgpx || bggz)
									c = ov2.ProcessFile(_Daddy, tmpDirectory + fln, "Traditional Cache", true);
								if (bgpx)
									_Daddy.ExportGPXStreamed(superPPDirectory + fln.Replace(".ov2", ".gpx"), c, null, new DateTime(2000, 1, 1, 0, 0, 1));
								cfull.AddRange(c);

								fln = Path.DirectorySeparatorChar + "GC Virtual.ov2";
								if (bcache)
									totalcachecache += ov2.ProcessFileSuperPP(con, cmd, tmpDirectory + fln, "Virtual Cache");
								if (bgpx || bggz)
									c = ov2.ProcessFile(_Daddy, tmpDirectory + fln, "Virtual Cache", true);
								if (bgpx)
									_Daddy.ExportGPXStreamed(superPPDirectory + fln.Replace(".ov2", ".gpx"), c, null, new DateTime(2000, 1, 1, 0, 0, 1));
								cfull.AddRange(c);

								fln = Path.DirectorySeparatorChar + "GC Webcam.ov2";
								if (bcache)
									totalcachecache += ov2.ProcessFileSuperPP(con, cmd, tmpDirectory + fln, "Webcam Cache");
								if (bgpx || bggz)
									c = ov2.ProcessFile(_Daddy, tmpDirectory + fln, "Webcam Cache", true);
								if (bgpx)
									_Daddy.ExportGPXStreamed(superPPDirectory + fln.Replace(".ov2", ".gpx"), c, null, new DateTime(2000, 1, 1, 0, 0, 1));
								cfull.AddRange(c);

								fln = Path.DirectorySeparatorChar + "GC Wherigo.ov2";
								if (bcache)
									totalcachecache += ov2.ProcessFileSuperPP(con, cmd, tmpDirectory + fln, "Wherigo Cache");
								if (bgpx || bggz)
									c = ov2.ProcessFile(_Daddy, tmpDirectory + fln, "Wherigo Cache", true);
								if (bgpx)
									_Daddy.ExportGPXStreamed(superPPDirectory + fln.Replace(".ov2", ".gpx"), c, null, new DateTime(2000, 1, 1, 0, 0, 1));
								cfull.AddRange(c);

								// Save GGZ
								fln = Path.DirectorySeparatorChar + "GC France.ggz";
								if (bggz)
									_Daddy.ExportGGZ(superPPDirectory + fln, cfull, new DateTime(2000, 1, 1, 0, 0, 1));

								// Delete temporary directory
								//Directory.Delete(tmpDirectory, true);
							}
							con.Close();
						}
						
						if (bcache)
						{
							fln = superPPDirectory + Path.DirectorySeparatorChar + cachecacheprefix + Path.DirectorySeparatorChar + cachecacheprefix;

							// Le fichier id
							System.IO.StreamWriter file = new System.IO.StreamWriter(fln + ".id", false, Encoding.Default);
							String sdate = DateTime.Now.ToString("yyyy-MM-ddTHH:mmZ");
							file.WriteLine(sdate);
							file.WriteLine(totalcachecache.ToString());
							file.WriteLine("1.0");
							file.Close();

							// On zippe la cache et on crée le fichier CacheCache.id
							ZipFile.CreateFromDirectory(
								superPPDirectory + Path.DirectorySeparatorChar + cachecacheprefix,
								superPPDirectory + Path.DirectorySeparatorChar + cachecacheprefix  +".zip",
								CompressionLevel.Optimal, false);
						}
						
						System.Diagnostics.Process.Start(superPPDirectory);

						_Daddy.KillThreadProgressBar();
					}
					catch(Exception)
					{
						_Daddy.KillThreadProgressBar();
						throw;
					}
				}
				return true;
			}
			else
			{
				MessageBox.Show("Daddy is missing :-(");
				return false;
			}
		}
		
		public void Close()
		{
		}
	}
}