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
    // display caches without stats (part of OCD)
    public class CustomFilterDatePose : CacheFilter
    {
        List<String> _dates = new List<String>();
		
        public CustomFilterDatePose()
        {
				_dates.Add("0801");
				_dates.Add("2101");
				_dates.Add("2402");
				_dates.Add("2412");
        }

        public override bool ToBeDisplayed(Geocache cache)
        {
			if (_dates.Contains(cache._dtDateCreation.ToString("ddMM")))
				return true;
			else
				return false;
        }
    }

	public class FilterDatePose : IScriptV2
	{
		private MainWindow _Daddy = null;
        public string Name { get { return "Filtre évolué sur la date de pose"; } }
		public string Description { get {return "Permet de filtrer le nom des caches selon leur date de pose";}}
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
                dico.Add("Filtrer la date de pose", "DoIt");
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
                
				CustomFilterDatePose fltr = new CustomFilterDatePose();
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