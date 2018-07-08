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
using MyGeocachingManager.Geocaching.Filters;
using SpaceEyeTools;
using SpaceEyeTools.HMI;
using System.Linq;
using System.IO.Compression;

namespace MyGeocachingManager
{
	public class MonkeyCaches : IScriptV2
	{
		private MainWindow _Daddy = null;
        public string Name { get { return "Analyseur de caches Monkeys"; } }
		public string Description { get {return "Télécharger automatiquement les PQs des Monkeys et les traite";}}
        public string Version { get { return "1.4"; } }
        public string MinVersionMGM { get { return "4.0.3.0.RC01"; } }
		
		public void Initialize(MainWindow daddy)
		{
			_Daddy = daddy;
		}

        public Dictionary<String, String> Functions
        {
            get
            {
                Dictionary<String, String> dico = new Dictionary<string, string>();
                dico.Add("Récupérer et traiter les PQs Monkeys", "DoIt");
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

                try
                {
                    
                    string[] fichiers_monkeys = new string[] {
                    "https://www.dropbox.com/sh/vbm2svx98swen20/AAC4gad3RA5e6Fko8eXf2LNVa/7646619.zip?dl=1", "Bruno.fr",
                    "https://www.dropbox.com/sh/vbm2svx98swen20/AADfaTi0WiPMYsDvfdTK0hpZa/MyFindsFab.zip?dl=1", "ElFIQue",
					"https://www.dropbox.com/sh/vbm2svx98swen20/AAAg7lraTEa01pEs6R45eFnZa/MyFinds_RGX78.zip?dl=1", "RGX78",
					"", "Kirdec",
					"", "CamCasimir"
                };

				List<ParameterObject> lst = new List<ParameterObject>();
				for(int i=1;i<fichiers_monkeys.Length; i += 2)
				{
					if (fichiers_monkeys[i-1] != "")
						lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "foo", fichiers_monkeys[i]));
				}

                ParametersChanger changer = new ParametersChanger();
                changer.Title = "Choix des PQs à traiter";
                changer.BtnCancel = _Daddy.GetTranslator().GetString("BtnCancel");
                changer.BtnOK = _Daddy.GetTranslator().GetString("BtnOk");
                changer.ErrorFormater = _Daddy.GetTranslator().GetString("ErrWrongParameter");
                changer.ErrorTitle = _Daddy.GetTranslator().GetString("Error");
                changer.Parameters = lst;
                changer.Font =_Daddy.Font;
                changer.Icon = _Daddy.Icon;

				int nb_monkeys = 5;
                if (changer.ShowDialog() == DialogResult.OK)
                {
                    for(int i=0;i<lst.Count; i++)
					{
						if (lst[i].Value == "False")
							fichiers_monkeys[2*i] = "";
					}
				}
				
					_Daddy._ThreadProgressBarTitle = "Traitement des PQs MyFinds des Monkeys";
                    _Daddy.CreateThreadProgressBar();

                    List<String> files = new List<string>();
                    for (int i=0;i<fichiers_monkeys.Length / 2; i++)
                    {
						String file = fichiers_monkeys[2*i];
						if (file == "")
							continue;
							
                        string localfilegpx = "";
                        if (file.Contains(".zip"))
                        {
                            string localfilezip = Guid.NewGuid().ToString() + ".zip";
                            MyTools.DownloadFile(file, _Daddy.GetProxy(), null, localfilezip);

                            // On dezippe
							try
							{
								ZipFile.ExtractToDirectory(localfilezip, ".");
								using (ZipArchive archive = ZipFile.OpenRead(localfilezip))
								{
									localfilegpx = archive.Entries[0].Name;
								}
							}
							catch (Exception exc2)
							{
								File.Delete(localfilezip);
								throw exc2;
							}
                            File.Delete(localfilezip);
                        }
                        else
                        {
                            localfilegpx = Guid.NewGuid().ToString() + ".gpx";
                            MyTools.DownloadFile(file, _Daddy.GetProxy(), null, localfilegpx);
                        }
                        files.Add(localfilegpx);
                    }

                    Dictionary<String, Geocache> caches_trouvees_ensemble = null;
                    Dictionary<String, Geocache> caches_communes = new Dictionary<string, Geocache>();
                    foreach (String file in files)
                    {
                        _Daddy.Log("Loading " + file);
                        // On lit les caches
                        Dictionary<String, Geocache> aBatch = new Dictionary<String, Geocache>();
						List<Waypoint> wpts = new List<Waypoint>();
						_Daddy.LoadGCFile(file, ref aBatch, ref wpts);
                        File.Delete(file);
                        _Daddy.Log("Loaded " + aBatch.Count.ToString() + " caches");

                        // On ne garde que les logs "Found it", "Attended", "Webcam Photo Taken" dans les caches!
                        foreach (KeyValuePair<String, Geocache> pair in aBatch)
                        {
                            Geocache geo = pair.Value;
                            CacheLog found_log = null;
                            foreach (CacheLog log in geo._Logs)
                            {
                                if ((log._Type == "Found it") ||
                                    (log._Type == "Attended") ||
                                    (log._Type == "Webcam Photo Taken"))
                                {
                                    found_log = log;
                                    break;
                                }
                            }
                            geo._Logs.Clear();
                            geo._Logs.Add(found_log);
                            _Daddy.Log(geo._Code + ": " + found_log._Type + " " + found_log._Date);
                        }

                        // On fusionne avec uniquement les caches trouvées le même jour !
                        if (caches_trouvees_ensemble == null)
                        {
                            // Les caches trouvées ensemble
                            caches_trouvees_ensemble = aBatch;

                            // Et les caches potentiellement communes
                            foreach (KeyValuePair<String, Geocache> pair in aBatch)
                            {
                                caches_communes.Add(pair.Key, pair.Value);
                            }
                        }
                        else
                        {
                            Dictionary<String, Geocache> newcaches = new Dictionary<string, Geocache>();
                            // On va stocker les caches qui ont été trouvées le même jour
                            foreach (KeyValuePair<String, Geocache> pair in aBatch)
                            {
                                Geocache geo = pair.Value;
                                // Est-ce que cette cache est déjà dans la liste commune ? 
                                // Si non, même pas la peine de se creuser la tête
                                Geocache cache_merged = null;
                                if (caches_trouvees_ensemble.ContainsKey(pair.Key))
                                {
                                    _Daddy.Log("Found a match: " + pair.Key);
                                    cache_merged = caches_trouvees_ensemble[pair.Key];
                                    // Est-ce que ça a été trouvé à la même date ?
                                    String date_log_batch = MyTools.CleanDate(geo._Logs[0]._Date);
                                    String date_log_merge = MyTools.CleanDate(cache_merged._Logs[0]._Date);
                                    if (date_log_batch == date_log_merge)
                                    {
                                        _Daddy.Log("Found a common date: " + date_log_batch);
                                        // Ok on a un truc commun !!!
                                        newcaches.Add(pair.Key, cache_merged);
                                    }
                                    else
                                    {
                                        cache_merged = null;
                                        _Daddy.Log("Didn't find a common date: " + date_log_batch + " vs " + date_log_merge);
                                    }
                                }
                                // Else poubelle

                                // On regarde si on n'a pas eu un match sur la même date
                                if (cache_merged == null)
                                {
                                    _Daddy.Log("Check if it is a common cache");
                                    // Alors on met à jour caches_communes si besoin
                                    if (caches_communes.ContainsKey(pair.Key))
                                    {
                                        _Daddy.Log("Common cache!");
                                        // On met à jour les fichiers et les logs
                                        Geocache cache_common = caches_communes[pair.Key];
                                        cache_common._origin.AddRange(pair.Value._origin);
                                        cache_common._Logs.Add(pair.Value._Logs[0]);
                                        _Daddy.Log("Updated!");
                                    }
                                    else
                                    {
                                        _Daddy.Log("Not common (so far)");
                                        // On ajoute juste la cache
                                        caches_communes.Add(pair.Key, pair.Value);
                                    }
                                }
                            }

                            // On remplace la liste mergée
                            caches_trouvees_ensemble.Clear();
                            aBatch.Clear();
                            caches_trouvees_ensemble = newcaches;
                        }
                    }

                    _Daddy.KillThreadProgressBar();

					SaveFileDialog saveFileDialog1 = new SaveFileDialog();
					saveFileDialog1.Filter = "CSV (*.csv)|*.csv";
					saveFileDialog1.RestoreDirectory = true;
					if (saveFileDialog1.ShowDialog() == DialogResult.OK)
					{
						String fichier_csv = saveFileDialog1.FileName;
						FileInfo fi = new FileInfo(fichier_csv);
						Directory.SetCurrentDirectory(fi.Directory.ToString());
						String fileRadix = fi.Name.ToString();
						
						int nb_max = 3 + 2*nb_monkeys;
						
						System.IO.StreamWriter file = new System.IO.StreamWriter(fileRadix, false, Encoding.Default);
						
						_Daddy.Log("Create common found msg");
						EcritLigne(file, new object[]{"LISTE DES CACHES COMMUNES TROUVEES LE MEME JOUR", caches_trouvees_ensemble.Count}, nb_max);
						foreach (KeyValuePair<String, Geocache> pair in caches_trouvees_ensemble)
						{
							// Longueur max arguments : 3
							Geocache geo = pair.Value;
							EcritLigne(file, new object[]{ geo._Code, geo._Name.Replace(";",","), geo._Logs[0]._Date}, nb_max);
						}
						
						_Daddy.Log("Create common msg");
						EcritLigne(file, new object[]{"LISTE DES CACHES COMMUNES MAIS NON TROUVEES LE MEME JOUR"}, nb_max);
						foreach (KeyValuePair<String, Geocache> pair in caches_communes)
						{
							Geocache geo = pair.Value;
							if (geo._Logs.Count >= 2)
							{
								// Longueur max arguments : 3 + 2*nb_monkeys
								List<object> os = new List<object>();
								os.Add(geo._Code);
								os.Add(geo._Name.Replace(";",","));
								
								List<String> dates = new List<String>();
								
								// On va parcourir les monkeys dans l'ordre et voir s'ils sont présents
								for (int i=0;i<fichiers_monkeys.Length / 2; i++)
								{
									String monkey = fichiers_monkeys[2*i+1];
									String date = "";
									foreach (CacheLog log in geo._Logs)
									{
										if (log._User.ToLower() == monkey.ToLower())
										{
											date = MyTools.CleanDate(log._Date);
											dates.Add(date);
										}
									}
									if (date != "")
									{
										os.Add(monkey);
										os.Add(date);
									}
									else
									{
										os.Add(monkey);
										os.Add(" ");
									}
								}
								// On recherche les doublons
								var duplicates = dates.GroupBy(a => a).SelectMany(ab => ab.Skip(1).Take(1)).ToList();
								if (duplicates.Count() != 0)
									os.Add("MonkeyFind");
								else
									os.Add("NotMonkey");
									
								EcritLigne(file, os.ToArray(), nb_max);
								
							}
						}
						file.Close();
					}
						
                    // Exclure des caches affichées les caches de caches_communes ET caches_trouvees_ensemble
                    // En gros toute cache qui a été trouvée par au moins un membre doit être exclue
                    HashSet<String> hs = new HashSet<String>();
                    foreach (KeyValuePair<String, Geocache> pair in caches_communes)
                    {
                        hs.Add(pair.Key);
                    }
                    foreach (KeyValuePair<String, Geocache> pair in caches_trouvees_ensemble)
                    {
                        hs.Add(pair.Key);
                    }

                    CustomFilterExcludeSelection fltr = new CustomFilterExcludeSelection(hs);
                    _Daddy.ExecuteCustomFilter(fltr);

                    return true;
                }
                catch (Exception exc)
                {
					_Daddy.KillThreadProgressBar();
                    String msg = _Daddy.GetTranslator().GetString("ErrFilter") + ": " + exc.Message;
                    MyMessageBox.Show(msg, _Daddy.GetTranslator().GetString("ErrTitle"), MessageBoxIcon.Exclamation, _Daddy.GetTranslator());
                    return false;
                }
			}
			else
			{
				MessageBox.Show("Daddy is missing :-(");
				return false;
			}
		}
		
		public void EcritLigne(System.IO.StreamWriter file, object[] os, int nbMax)
		{
			String msg = "";
			int nb = 0;
			foreach(object o in os)
			{
				msg += o.ToString() + ";";
				nb++;
			}
			for(int i=nb;i<nbMax;i++)
				msg += ";";
			file.WriteLine(msg);
		}
		
		public void Close()
		{
		}
	}
}