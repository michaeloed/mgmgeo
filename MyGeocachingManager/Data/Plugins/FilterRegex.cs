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
    public class CustomFilterRegex : CacheFilter
    {
        String _filter = "";
        bool _title = false;
		bool _desc = false;
		bool _logs = false;
		bool _simplesearch = false;
		
        public CustomFilterRegex(String filter, bool title, bool desc, bool logs, bool simplesearch)
        {
            _filter = filter;
			_title = title;
			_desc = desc;
			_logs = logs;
			_simplesearch = simplesearch;
        }

        public override bool ToBeDisplayed(Geocache cache)
        {
			if (!_simplesearch)
			{
				if (_title)
				{
					Match match = Regex.Match(cache._Name, _filter, RegexOptions.IgnoreCase);
					// Here we check the Match instance.
					if (match.Success)
						return true;
				}
				
				if (_desc)
				{
					Match match = Regex.Match(cache._ShortDescription + cache._LongDescription, _filter, RegexOptions.IgnoreCase);
					// Here we check the Match instance.
					if (match.Success)
						return true;
				}
				
				if (_logs)
				{
					foreach(var log in cache._Logs)
					{
						Match match = Regex.Match(log._Text, _filter, RegexOptions.IgnoreCase);
						// Here we check the Match instance.
						if (match.Success)
							return true;
					}
				}
							
				return false;
			}
			else
			{
				if (_title)
				{
					if (cache._Name.Contains(_filter))
						return true;
				}
				
				if (_desc)
				{
					if ((cache._ShortDescription + cache._LongDescription).Contains(_filter))
						return true;
				}
				
				if (_logs)
				{
					foreach(var log in cache._Logs)
					{
						if (log._Text.Contains(_filter))
							return true;
					}
				}
							
				return false;
			}
			
		}
    }

	public class FilterRegex : IScriptV2
	{
		private MainWindow _Daddy = null;
        public string Name { get { return "Recherche évoluée d'un texte"; } }
		public string Description { get {return "Permet de rechercher un texte dans les caches selon une expression régulière";}}
        public string Version { get { return "1.4"; } }
        public string MinVersionMGM { get { return "4.0.3.0.RC09"; } }
		
		public void Initialize(MainWindow daddy)
		{
			_Daddy = daddy;
		}

        public Dictionary<String, String> Functions
        {
            get
            {
                Dictionary<String, String> dico = new Dictionary<string, string>();
                dico.Add("Rechercher un texte (contient, simple et rapide...)", "DoItQuick");
                dico.Add("Rechercher un texte (wildcards)", "DoItWild");
                dico.Add("Rechercher un texte (regex)", "DoItRegex");
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

		public bool DoItQuick()
		{
			if (_Daddy != null)
			{
                List<ParameterObject> lst = new List<ParameterObject>();
                lst.Add(new ParameterObject(ParameterObject.ParameterType.String, "", "expression", "Filtre de recherche"));
				lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "titre", "Rechercher dans le nom"));
				lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "desc", "Rechercher dans la description"));
				lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "logs", "Rechercher dans les logs"));
				
                ParametersChanger changer = new ParametersChanger();
                changer.Title = "Entrez le filtre de recherche (simple)";
                changer.BtnCancel = "Annuler";
                changer.BtnOK = "Valider";
                changer.ErrorFormater = "Mauvais format de paramètre";
                changer.ErrorTitle = "Erreur";
                changer.Parameters = lst;
                changer.Font = _Daddy.Font;
                changer.Icon = _Daddy.Icon;

                if (changer.ShowDialog() == DialogResult.OK)
                {
					bool title = (changer.Parameters[1].Value == "True");
                    bool desc = (changer.Parameters[2].Value == "True");
                    bool logs = (changer.Parameters[3].Value == "True");
                    CustomFilterRegex fltr = new CustomFilterRegex(lst[0].Value, title, desc, logs, true);
                    try
					{
						_Daddy._ThreadProgressBarTitle = "";
						_Daddy.CreateThreadProgressBar();
						_Daddy.ExecuteCustomFilter(fltr);
						_Daddy.KillThreadProgressBar();
					}
					catch(Exception ex)
					{
						_Daddy.KillThreadProgressBar();
						_Daddy.ShowException("Boom", "Ca a planté...", ex);
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
		
		public bool DoItRegex()
		{
			if (_Daddy != null)
			{
                List<ParameterObject> lst = new List<ParameterObject>();
                lst.Add(new ParameterObject(ParameterObject.ParameterType.String, "", "expression", "Filtre de recherche"));
				lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "titre", "Rechercher dans le nom"));
				lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "desc", "Rechercher dans la description"));
				lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "logs", "Rechercher dans les logs"));
				
                ParametersChanger changer = new ParametersChanger();
                changer.Title = "Entrez le filtre de recherche (regex)";
                changer.BtnCancel = "Annuler";
                changer.BtnOK = "Valider";
                changer.ErrorFormater = "Mauvais format de paramètre";
                changer.ErrorTitle = "Erreur";
                changer.Parameters = lst;
                changer.Font = _Daddy.Font;
                changer.Icon = _Daddy.Icon;

                if (changer.ShowDialog() == DialogResult.OK)
                {
					bool title = (changer.Parameters[1].Value == "True");
                    bool desc = (changer.Parameters[2].Value == "True");
                    bool logs = (changer.Parameters[3].Value == "True");
                    CustomFilterRegex fltr = new CustomFilterRegex(lst[0].Value, title, desc, logs, false);
                    try
					{
						_Daddy._ThreadProgressBarTitle = "";
						_Daddy.CreateThreadProgressBar();
						_Daddy.ExecuteCustomFilter(fltr);
						_Daddy.KillThreadProgressBar();
					}
					catch(Exception ex)
					{
						_Daddy.KillThreadProgressBar();
						_Daddy.ShowException("Boom", "Ca a planté...", ex);
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

        public static string WildcardToRegex(string pattern)
        {
            pattern = pattern.Replace(".", @"\.");
            pattern = pattern.Replace("?", ".");
            pattern = pattern.Replace("*", ".*?");
            pattern = pattern.Replace(@"\", @"\\");
            pattern = pattern.Replace(" ", @"\s");
            return pattern;
            /*
            return "^" + Regex.Escape(pattern)
                              .Replace(@"\*", ".*")
                              .Replace(@"\?", ".")
                       + "$";*/
        }

        public bool DoItWild()
        {
            if (_Daddy != null)
            {
                List<ParameterObject> lst = new List<ParameterObject>();
                lst.Add(new ParameterObject(ParameterObject.ParameterType.String, "", "expression", "Filtre de recherche"));
				lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "titre", "Rechercher dans le titre"));
				lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, false, "desc", "Rechercher dans la description"));
				lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "logs", "Rechercher dans les logs"));
				
                ParametersChanger changer = new ParametersChanger();
                changer.Title = "Entrez le filtre de recherche (wildcards)";
                changer.BtnCancel = "Annuler";
                changer.BtnOK = "Valider";
                changer.ErrorFormater = "Mauvais format de paramètre";
                changer.ErrorTitle = "Erreur";
                changer.Parameters = lst;
                changer.Font = _Daddy.Font;
                changer.Icon = _Daddy.Icon;

                if (changer.ShowDialog() == DialogResult.OK)
                {
					bool title = (changer.Parameters[1].Value == "True");
                    bool desc = (changer.Parameters[2].Value == "True");
                    bool logs = (changer.Parameters[3].Value == "True");
                    CustomFilterRegex fltr = new CustomFilterRegex(FilterRegex.WildcardToRegex(lst[0].Value), title, desc, logs, false);
					try
					{
						_Daddy._ThreadProgressBarTitle = "";
						_Daddy.CreateThreadProgressBar();
						_Daddy.ExecuteCustomFilter(fltr);
						_Daddy.KillThreadProgressBar();
					}
					catch(Exception ex)
					{
						_Daddy.KillThreadProgressBar();
						_Daddy.ShowException("Boom", "Ca a planté...", ex);
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