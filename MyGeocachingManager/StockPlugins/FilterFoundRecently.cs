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
    public class CustomFilterFoundRecently : CacheFilter
    {
        public CustomFilterFoundRecently()
        {
		
        }

        public override bool ToBeDisplayed(Geocache cache)
        {
			// Found it
			if (cache._Logs.Count != 0)
			{
				if (cache._Logs[0]._Type == "Found it")
					return true;
				else
					return false;
					
			}
			return false;
        }
    }

	public class FoundRecently : IScriptV2
	{
		private MainWindow _Daddy = null;
        public string Name { get { return "Filtre sur les caches trouvées"; } }
		public string Description { get {return "Permet de filtrer les caches dont le dernier log est un found it";}}
        public string Version { get { return "1.1"; } }
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
                dico.Add("Garder uniquement les found it", "DoItFoundIt");
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

		public bool DoItFoundIt()
		{
			if (_Daddy != null)
			{
                CustomFilterFoundRecently fltr = new CustomFilterFoundRecently();
                _Daddy.ExecuteCustomFilter(fltr);
                
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