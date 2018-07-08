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
using MyGeocachingManager.HMI;
using GMap.NET;
using GMap.NET.WindowsForms;
using System.Threading;

namespace MyGeocachingManager
{
	public class CacheCoverage : IScriptV2
	{
		private MainWindow _Daddy = null;
        public string Name { get { return "Couverture géographique des caches"; } }
		public string Description { get {return "Génère les images de couverture géographique à partir des caches affichées";}}
        public string Version { get { return "1.2"; } }
        public string MinVersionMGM { get { return "4.0.2.0.RC04"; } }
		
		public void Initialize(MainWindow daddy)
		{
			_Daddy = daddy;
		}

        public Dictionary<String, String> Functions
        {
            get
            {
                Dictionary<String, String> dico = new Dictionary<string, string>();
                dico.Add("Couverture géographique", "DoIt");
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
                String pqdatapath = _Daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "GPX" + Path.DirectorySeparatorChar + "PQ" + Path.DirectorySeparatorChar;
            	String myfinds = pqdatapath + "MyFindsPocketQuery.zip";
            	if (File.Exists(myfinds) == false)
            	{
            		_Daddy.MSG("Fichier " + myfinds + " manquant");
            		return false;
            	}
            	string[] files = new string[1];
            	files[0] = myfinds;
            	_Daddy.LoadBatchOfFilesImpl(files, true, false, false);
            	
            	KmlManager kml = new KmlManager(_Daddy);
				
				// Le monde
				bool b = kml.DisplayWorldCoverage();
				if (!b)
				{
					_Daddy.MSG("Erreur");
					return false;
				}
				//_Daddy._cacheDetail._gmap.Zoom = 2.5;
            	//_Daddy._cacheDetail.Location = new Point(0,0);
        		//_Daddy._cacheDetail.Size = new Size(1200,1200);
        		//_Daddy._cacheDetail._gmap.Position = new PointLatLng(0, 0);
        		Thread.Sleep(2000);
        		
            	Image img = _Daddy._cacheDetail._gmap.ToImage();
            	img.Save(_Daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "World.png", ImageFormat.Png);
				
				// La France et ses zones
            	for(int i=1;i<=5;i++)
            	{
            		b = kml.DisplayFranceCoverageImpl(i);
            		if (!b)
            		{
            			_Daddy.MSG("Erreur");
            			return false;
            		}
            		
            		//_Daddy._cacheDetail._gmap.Zoom = 6.0;
	            	//_Daddy._cacheDetail.Location = new Point(0,0);
            		//_Daddy._cacheDetail.Size = new Size(1200,1200);
            		//_Daddy._cacheDetail._gmap.Position = new PointLatLng(46.5437496027386, 2.9443359375);
            		Thread.Sleep(2000);
            		
	            	img = _Daddy._cacheDetail._gmap.ToImage();
	            	img.Save(_Daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "France_" + i.ToString() + ".png", ImageFormat.Png);
            	}
            	
            	MyTools.StartCmd(_Daddy.GetUserDataPath());
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