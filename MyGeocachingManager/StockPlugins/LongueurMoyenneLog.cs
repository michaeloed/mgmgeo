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
using System.Text.RegularExpressions;
using MyGeocachingManager.Geocaching;

namespace MyGeocachingManager
{
	public class MoyenneLog : IScriptV2
	{
		private MainWindow _Daddy = null;
        public string Name { get { return "Longueur Moyenne d'un Log"; } }
		public string Description { get {return "Affiche la longueur moyenne des log des caches sélectionnées";}}
        public string Version { get { return "1.3"; } }
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
                dico.Add("Longueur moyenne des logs de la sélection", "DoIt");
				dico.Add("Longueur moyenne des logs par cache", "DoItDetail");
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
                List<Geocache> caches = null;
				caches = _Daddy.GetSelectedCaches();
				if (caches.Count == 0)
				{
					_Daddy.MSG("Vous devez sélectionner des caches avant d'exécuter cette commande");
                    return false;
				}
				
				double iLongueurTotale = 0.0;
				double iNbLogs = 0.0;
				foreach(Geocache geo in caches)
				{
					foreach(CacheLog log in geo._Logs)
					{
						if (log._Type.ToLower() == "found it")
						{
							iLongueurTotale += log._Text.Length;
							iNbLogs += 1.0;
						}
					}
				}
				
				if (iNbLogs == 0)
				{
					_Daddy.MSG("Aucun log 'Found it' trouvé pour les caches sélectionnées");
                    return false;
				}
				
				double moy = iLongueurTotale / iNbLogs;
				String msg = String.Format("La longueur moyenne des {0} logs 'Found it' analysés est de {1} caracteres", iNbLogs, (int)moy);
				_Daddy.MSG(msg);
				return true;
			}
			else
			{
				MessageBox.Show("Daddy is missing :-(");
				return false;
			}
		}
       
	   public bool DoItDetail()
		{
			if (_Daddy != null)
			{
                List<Geocache> caches = null;
				caches = _Daddy.GetSelectedCaches();
				if (caches.Count == 0)
				{
					_Daddy.MSG("Vous devez sélectionner des caches avant d'exécuter cette commande");
                    return false;
				}
				
				String msg = "";
				foreach(Geocache geo in caches)
				{
					double iLongueurTotale = 0.0;
					double iNbLogs = 0.0;
					msg += geo._Code + " " + geo._Name + " : ";
					foreach(CacheLog log in geo._Logs)
					{
						if (log._Type.ToLower() == "found it")
						{
							iLongueurTotale += log._Text.Length;
							iNbLogs += 1.0;
						}
					}
					
					if (iNbLogs == 0)
					{
						msg += "Aucun log 'Found it' trouvé\r\n";
					}
					else
					{
						double moy = iLongueurTotale / iNbLogs;
						msg += String.Format("Moyenne de {0} caracteres pour {1} logs 'Found it' analysés\r\n", (int)moy, iNbLogs);
					}
				}
				_Daddy.MSG(msg);
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