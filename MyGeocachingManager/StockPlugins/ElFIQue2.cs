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
using SpaceEyeTools;
using SpaceEyeTools.HMI;
using MyGeocachingManager.Geocaching;
using SpaceEyeTools.EXControls;
using MyGeocachingManager.Geocaching.Filters;

namespace MyGeocachingManager
{
	public class ElFIQue2 : IScriptV2
	{
		private MainWindow _Daddy = null;
		public string Name {get {return "GGZ Generateur";}}
		public string Description { get {return "Regroupe dans un seul fichier un ensemble de GPX";}}
        public string Version { get { return "1.1"; } }
        public string MinVersionMGM { get { return "3.0.1.0.RC5"; } }
		
		public Dictionary<String, String> Functions
		{ 
			get 
			{
				Dictionary<String, String> dico = new Dictionary<String, String>();
				dico.Add("Dezip", "Batch");
				dico.Add("IdF", "IdFGenerator");
				dico.Add("Plaisir", "PlaisirGenerator");
				dico.Add("Informations", "GetInfos");
				return dico;
			}
		}
		
		public void Initialize(MainWindow daddy)
		{
			_Daddy = daddy;
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
		
		public bool Batch()
		{
			if (_Daddy != null)
			{
				// Lance le fichier bat  de dezippage des fichiers
				System.Diagnostics.ProcessStartInfo myInfo = new System.Diagnostics.ProcessStartInfo();
				myInfo.FileName = @"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\batch.bat";
				myInfo.WorkingDirectory = @"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\";
				myInfo.Arguments = ""; // La tu peux mettre des arguments si besoin en les guillemets
				System.Diagnostics.Process.Start(myInfo);

				return true;
			}
			else
			{
				MessageBox.Show("Daddy is missing :-(");
				return false;
			}
		}



		public bool IdFGenerator()
		{
			if (_Daddy != null)
			{
				// le code du plugin est ici :
				// *********************************
				
				// Efface les caches déjà existantes
				_Daddy._caches = new Dictionary<string, Geocache>();
				
				// Efface les waypoints déjà existants
				_Daddy._waypoints = new Dictionary<String, Waypoint>(); 
				
				// Charge un fichier (avec son chemin complet)
				_Daddy.LoadFile(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\12044886_IdFMystery1.gpx");
	            		_Daddy.LoadFile(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\13353760_IdFMystery2.gpx");
				_Daddy.JoinWptsGC();
	            _Daddy.PostTreatmentLoadCache();
	            _Daddy.BuildListViewCache();
	            _Daddy.PopulateListViewCache(null);
	            
				// On exécute des filtres déjà sauvegardés
				// Pas implémentable pour l'instant, objets private
				bool filtres_mode_OR = true; // On récupère le résultat de chaque filtre en mode "OU" (sinon c'est "ET")
				List<CacheFilter> les_filtres = new List<CacheFilter>();
				const String f1 = "bonus"; // Mettre le nom qu'on veut
				const String f2 = "final"; // Mettre le nom qu'on veut
				foreach(Object obj in _Daddy.cbFilterList.Items) 
				{
					
					CacheFilter fil = (CacheFilter)(obj);
					switch(fil._description)
					{
						case f1:
						case f2:
							{
								les_filtres.Add(fil);
								break;
							}
						default:
							break;
					}
				}
            
				if (les_filtres.Count != 0)
				{
					List<EXListViewItem> forcedList = new List<EXListViewItem>();
					if (filtres_mode_OR)
					{
		                		ChainedFiltersOR chnf = new ChainedFiltersOR(les_filtres);
		                		// Build list of caches
		                		foreach (EXListViewItem item in _Daddy._listViewCaches) // /!\ BCR : à rendre public !
		                		{
		                    			Geocache cache = _Daddy._caches[item.Text];
		                    			if (chnf.ToBeDisplayed(cache))
		                    			{
		                        			forcedList.Add(item);
		                    			}
		                		}
		            		}
		            		else
		            		{
		                		ChainedFiltersAND chnf = new ChainedFiltersAND(les_filtres);
		                		// Build list of caches
		                		foreach (EXListViewItem item in _Daddy._listViewCaches)
		                		{
		                    			Geocache cache = _Daddy._caches[item.Text];
		                    			if (chnf.ToBeDisplayed(cache))
		                    			{
		                        			forcedList.Add(item);
		                    			}
		                		}
		            		}
		            
		            		_Daddy.PopulateListViewCache(forcedList);
				}
				
				// On sauve le résultat
				_Daddy.ExportGPXBrutal(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\IdF_Bonus.gpx");



				
				// *********************************
				// Génération du Fichier GGZ IdF

				_Daddy.LoadFile(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\IdF_Bonus.gpx");
	            		_Daddy.LoadZip(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\IdFMulti+.zip");
	            		_Daddy.LoadZip(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\IdFNight.zip");
	            		_Daddy.LoadZip(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\IdF Mystery décodé.zip");
	            		_Daddy.LoadZip(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\IdF Mystery sur place.zip");
				_Daddy.LoadFile(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\12227826_IdFTradi1.gpx");
				_Daddy.LoadFile(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\12227994_IdFTradi2.gpx");
				_Daddy.LoadFile(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\12228003_IdFTradi3.gpx");
				_Daddy.LoadFile(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\12228015_IdFTradi4.gpx");
				_Daddy.LoadFile(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\13437328_IdFTradi5.gpx");
				_Daddy.JoinWptsGC();
	            		_Daddy.PostTreatmentLoadCache();
	            		_Daddy.BuildListViewCache();
	            		_Daddy.PopulateListViewCache(null);

				_Daddy.ExportGGZ(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\IdF.ggz", false);

		
				return true;
			}
			else
			{
				MessageBox.Show("Daddy is missing :-(");
				return false;
			}
		}
		


		public bool PlaisirGenerator()
		{
			if (_Daddy != null)
			{
			
				// Le vrai code du plugin sera ici :
				// *********************************


				// Efface les caches déjà existantes
				_Daddy._caches = new Dictionary<string, Geocache>();
				
				// Efface les waypoints déjà existants
				_Daddy._waypoints = new Dictionary<String, Waypoint>(); 
				
				// Charge un fichier (avec son chemin complet)
				_Daddy.LoadFile(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\12044886_IdFMystery1.gpx");
	            		_Daddy.LoadFile(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\13353760_IdFMystery2.gpx");
				_Daddy.JoinWptsGC();
	            		_Daddy.PostTreatmentLoadCache();
	            		_Daddy.BuildListViewCache();
	            		_Daddy.PopulateListViewCache(null);
	            
				// On exécute des filtres déjà sauvegardés
				bool filtres_mode_OR = true; // On récupère le résultat de chaque filtre en mode "OU" (sinon c'est "ET")
				List<CacheFilter> les_filtres = new List<CacheFilter>();
				const String f1 = "bonus"; // Mettre le nom qu'on veut
				const String f2 = "final"; // Mettre le nom qu'on veut
				foreach(Object obj in _Daddy.cbFilterList.Items) 
				{
					
					CacheFilter fil = (CacheFilter)(obj);
					switch(fil._description)
					{
						case f1:
						case f2:
							{
								les_filtres.Add(fil);
								break;
							}
						default:
							break;
					}
				}
            
				if (les_filtres.Count != 0)
				{
					List<EXListViewItem> forcedList = new List<EXListViewItem>();
					if (filtres_mode_OR)
					{
		                		ChainedFiltersOR chnf = new ChainedFiltersOR(les_filtres);
		                		// Build list of caches
		                		foreach (EXListViewItem item in _Daddy._listViewCaches) 
		                		{
		                    			Geocache cache = _Daddy._caches[item.Text];
		                    			if (chnf.ToBeDisplayed(cache))
		                    			{
		                        			forcedList.Add(item);
		                    			}
		                		}
		            		}
		            		else
		            		{
		                		ChainedFiltersAND chnf = new ChainedFiltersAND(les_filtres);
		                		// Build list of caches
		                		foreach (EXListViewItem item in _Daddy._listViewCaches)
		                		{
		                    			Geocache cache = _Daddy._caches[item.Text];
		                    			if (chnf.ToBeDisplayed(cache))
		                    			{
		                        			forcedList.Add(item);
		                    			}
		                		}
		            		}
		            
		            		_Daddy.PopulateListViewCache(forcedList);
				}
				
				// On sauve le résultat
				_Daddy.ExportGPXBrutal(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\IdF_Bonus.gpx");



				
				// *********************************
				// Génération du Fichier GGZ Plaisir

				// efface les caches existantes
				_Daddy._caches = new Dictionary<string, Geocache>();				

				_Daddy.LoadFile(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\IdF_Bonus.gpx");
	            		_Daddy.LoadZip(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\IdFMulti+.zip");
	            		_Daddy.LoadZip(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\IdFNight.zip");
	            		_Daddy.LoadZip(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\IdF Mystery décodé.zip");
	            		_Daddy.LoadZip(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\IdF Mystery sur place.zip");
				_Daddy.LoadFile(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\12471731_PlaisirTradi50km1.gpx");
				_Daddy.LoadFile(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\12471781_PlaisirTradi50km2.gpx");
				_Daddy.LoadFile(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\12471793_PlaisirTradi50km3.gpx");
				_Daddy.LoadFile(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\13414233_PlaisirTradi50km4.gpx");
				_Daddy.LoadFile(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\PQ\15387685_PlaisirTradi50km5.gpx");

				_Daddy.JoinWptsGC();
	            		_Daddy.PostTreatmentLoadCache();
	            		_Daddy.BuildListViewCache();
	            		_Daddy.PopulateListViewCache(null);

				_Daddy.ExportGGZ(@"F:\Documents\5- GeoCaching\MyGeocachingManager\GPX\Plaisir.ggz", false);

		
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