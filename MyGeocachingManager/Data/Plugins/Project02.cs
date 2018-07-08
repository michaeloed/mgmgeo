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
	public class CustomFilterFoundRecently2 : CacheFilter
    {
		bool _bEasyTradiOnly = false;
		bool _bForgetAboutLogs = false;
        public CustomFilterFoundRecently2(bool bEasyTradiOnly, bool bForgetAboutLogs)
        {
			_bEasyTradiOnly = bEasyTradiOnly;
			_bForgetAboutLogs = bForgetAboutLogs;
        }

        public override bool ToBeDisplayed(Geocache cache)
        {
			// si _bEasyTradiOnly
			// On ne garde que les caches tradi dont les D et T sont <= 4 NON TROUVEES !!!
			// Found it
			if (cache.IsFound())
				return false;
				
			// Une tradi ?
			if (String.Compare(cache._Type, "Traditional Cache", true) != 0)
				return false;
			
			// D
			if (cache.getD() >= 4)
                return false;

			// T
            if (cache.getT() >= 4)
                return false;
				
			// Et les logs
			if (_bForgetAboutLogs)
			{
				// On se moque des logs pour l'instant
				return true;
			}
			else
			{
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
    }
	
	public class Project02 : IScriptV2
	{
		private MainWindow _Daddy = null;
        public string Name { get { return "Project02"; } }
		public string Description { get {return "Permet de générer des notes de terrain pour les caches sélectionnées";}}
        public string Version { get { return "1.4"; } }
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
                dico.Add("Pack 02", "DoItCompleteFoundIt");
				dico.Add("-*-*-*-*-*-*-*-*-*-*-", "GetInfos");
                dico.Add("Extra - Définir la zone de filtrage", "DefineArea");
                dico.Add("Extra - Garder uniquement les found it de la sélection", "DoItFoundIt");
                dico.Add("Extra - Générer les notes de terrain pour la sélection", "DoIt");
                dico.Add("Extra - Afficher le calendrier des trouvailles", "DoItCal");
                dico.Add("*-*-*-*-*-*-*-*-*-*-*", "GetInfos");
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
                CustomFilterFoundRecently2 fltr = new CustomFilterFoundRecently2(false, false);
                _Daddy.ExecuteCustomFilter(fltr);
                
				return true;
			}
			else
			{
				MessageBox.Show("Daddy is missing :-(");
				return false;
			}
		}
		
		public CheckBox GetcbFilterArea()
		{
			Object o = MyTools.FindControl(_Daddy, "cbFilterArea");
			if (o != null)
			{
				return (CheckBox)o;
			}
			return null;
		}
		
		public bool CheckIfAreaIsDefined(ref bool bFilterOk, ref bool bZoneOk)
		{
			bFilterOk = false;
			bZoneOk = false;
			// On cherche si le filtre est checké
			CheckBox cb = GetcbFilterArea();
			if (cb != null)
			{
				if (cb.Checked)
				{
					bFilterOk = true;
					
					if (_Daddy._cacheDetail.area_PointsClicked.Count > 3)
					{
						// On a une belle zone d'au moins 3 points
						bZoneOk = true;
						return true;
					}
					bZoneOk = true;
					return true;
				}
			}
			return false;
		}
		
		public bool DoItCompleteFoundIt()
		{
			if (_Daddy != null)
			{
				String msg = "";
				bool bFilterOk = false;
				bool bZoneOk = false;
				DialogResult dialogResult;
				if (CheckIfAreaIsDefined(ref bFilterOk, ref bZoneOk) == false)
				{
					if (!bZoneOk)
					{
						// La zone n'est pas définie, on informe !
						msg = "ATTENTION ! Aucune zone valide n'est définie ! Voulez-vous en définir une (oui) ou continuer sans filtre de zone (non) ?";
						dialogResult = MessageBox.Show(msg,
                            _Daddy.GetTranslator().GetString("AskConfirm"),
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
						if (dialogResult == DialogResult.Yes)
						{
							// On définit la zone et on s'arrête là.
							return DefineArea();
						}
						// Sinon on continue, roule ma poule
					}
					else if (!bFilterOk)
					{
						// La zone est définie mais le filtre n'est pas activé
						msg = "ATTENTION ! Une zone est définie mais le filtre n'est pas activé ! Voulez-vous l'activer (oui) ou continuer sans filtre de zone (non) ?";
						dialogResult = MessageBox.Show(msg,
                            _Daddy.GetTranslator().GetString("AskConfirm"),
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
						if (dialogResult == DialogResult.Yes)
						{
							// On active le filtre
							CheckBox cb = GetcbFilterArea();
							if (cb != null)
							{
								cb.Checked = true;
							}
							
							if ((cb == null) || (cb.Checked == false))
							{
								// WTF !!!
								msg = "ATTENTION ! Impossible d'activer le filtre ! Voulez-vous néanmoins continuer ?";
								dialogResult = MessageBox.Show(msg,
									_Daddy.GetTranslator().GetString("AskConfirm"),
									MessageBoxButtons.YesNo, MessageBoxIcon.Question);
								if (dialogResult != DialogResult.Yes)
								{
									return false;
								}
							}
						}
						// Sinon on continue, roule ma poule
					}
				}	
				
				// filtre sur zone / tradi / D4T4 -> complete les logs -> filtre les logs -> crée les notes -> logge)
				msg = "Les opération suivantes vont être réalisées séquentiellement :\r\n";
				msg += "- Filtrage des caches affichées pour ne garder que les Traditionnelles non trouvées avec une D < 4 et un T < 4. Possibilité d'exécuter préalablement le filtre défini dans MGM\r\n";
				msg += "- Affichage du calendrier des trouvailles\r\n";
				msg += "- Définition du type de notes de terrain : fichier temporaire ou choisi manuellement (il sera alors conservé)\r\n";
				msg += "- Demande de la date de début et de fin souhaitées pour la création des notes de terrain\r\n";
				msg += "- Récupération des derniers logs des caches résultantes du filtre précédent\r\n";
				msg += "- Filtrage pour garder uniquement les caches dont le dernier log est un Found It\r\n";
				msg += "- Confirmation pour continuer\r\n";
				msg += "- Création des notes de terrain\r\n";
				msg += "- Ouverture de la fenêtre de log sur ces notes de terrain\r\n";
				msg += "Êtes-vous d'accord pour continuer ?";
				dialogResult = MessageBox.Show(msg,
                            _Daddy.GetTranslator().GetString("AskConfirm"),
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (dialogResult != DialogResult.Yes)
				{
					return false;
				}
					
					
				// On fait un préfiltre pour garder uniquement les candidats
				// On se moque des logs pour l'instant
				CustomFilterFoundRecently2 fltr = new CustomFilterFoundRecently2(true, true);
                _Daddy.ExecuteCustomFilter(fltr);
				
				// Ok on a fait un peu le ménage mais il se peut qu'il y ait beaucoup de caches
				List<Geocache> caches = null;
				caches = _Daddy.GetDisplayedCaches();
				if (caches.Count == 0)
				{
					_Daddy.MSG("Le filtre n'a retourné aucune cache valide !");
                    return false;
				}
				
				// On affiche le calendrier des trouvailles
				DisplayFoundCalendar(true);
				
				bool bAutoFile = false;
				DateTime deb = DateTime.MinValue;
				DateTime fin = DateTime.MinValue;
				// On demande la date de début et de fin du run
				List<ParameterObject> lst = new List<ParameterObject>();
				lst.Add(new ParameterObject(ParameterObject.ParameterType.Label, "" , "nb", caches.Count.ToString() + " caches vont être mises à jour !"));
				lst.Add(new ParameterObject(ParameterObject.ParameterType.Bool, true, "autofile", "Notes de terrain temporaires"));
				lst.Add(new ParameterObject(ParameterObject.ParameterType.Date, DateTime.Now, "date", "Date de début"));
				lst.Add(new ParameterObject(ParameterObject.ParameterType.Date, DateTime.Now, "date", "Date de fin"));
			
				ParametersChanger changer = new ParametersChanger();
				changer.Title = "Utiliser des notes de terrain temporaires ?";
				changer.BtnCancel = _Daddy.GetTranslator().GetString("BtnCancel");
				changer.BtnOK = _Daddy.GetTranslator().GetString("BtnOk");
				changer.ErrorFormater = _Daddy.GetTranslator().GetString("ErrWrongParameter");
				changer.ErrorTitle = _Daddy.GetTranslator().GetString("Error");
				changer.Parameters = lst;
				changer.Font = _Daddy.Font;
				changer.Icon = _Daddy.Icon;

				if (changer.ShowDialog() == DialogResult.OK)
				{	
					bAutoFile =  (lst[1].Value == "True");
					deb = (DateTime)(lst[2].ValueO);
					fin = (DateTime)(lst[3].ValueO);
					
					// Nombre de jours du run
					if (deb > fin)
					{
						_Daddy.MSG("La date de début doit être antérieure à la date de fin !");
						return false;
					}
				}
				else
					return false;
									
				// On complète les caches avec les derniers logs
                // 00 : All
				// 01 : DateCreation
				// 02 : Owner
				// 03 : Status
				// 04 : Difficulty
				// 05 : Terrain
				// 06 : Description
				// 07 : Container
				// 08 : Hint
				// 09 : Attributes
				// 10 : Logs
				// 11 : Contry
				// 12 : State
				// 13 : Statistics
				// 14 : Basic info (name, url, coordinates)
				bool[] parameters = new bool[15];
				parameters[ 0] = false;
				parameters[ 1] = false;
				parameters[ 2] = true;
				parameters[ 3] = false;
				parameters[ 4] = false;
				parameters[ 5] = false;
				parameters[ 6] = false;
				parameters[ 7] = false;
				parameters[ 8] = false;
				parameters[ 9] = false;
				parameters[10] = true;
				parameters[11] = false;
				parameters[12] = false;
				parameters[13] = false;
				parameters[14] = false;
				_Daddy.CompleteSelectedCaches(ref caches, parameters, true);
				
				// On conserve seulement les caches avec des logs found it en dernier
				fltr = new CustomFilterFoundRecently2(true, false);
                _Daddy.ExecuteCustomFilter(fltr);
				
				// Ok on a fait un peu le ménage mais il se peut qu'il y ait beaucoup de caches
				caches = _Daddy.GetDisplayedCaches();
				if (caches.Count == 0)
				{
					_Daddy.MSG("Le filtre n'a retourné aucune cache valide !");
                    return false;
				}
				
				msg = caches.Count.ToString() + " caches vont être loggées !";
				dialogResult = MessageBox.Show(msg,
                            _Daddy.GetTranslator().GetString("AskConfirm"),
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (dialogResult != DialogResult.Yes)
				{
					return false;
				}
				
				// On génère les field notes sur toutes les caches affichées
				String field_notes = "";
                if (GenerateFieldNotes(false, bAutoFile, ref field_notes, deb, fin))
				{
					// On lance l'IHM des field notes
					FieldNotesHMI fnHMI = new FieldNotesHMI(_Daddy, field_notes);
					fnHMI.Show();
					
					if (bAutoFile && File.Exists(field_notes))
					{
						File.Delete(field_notes);
					}
					return true;
				}
				return false;
			}
			else
			{
				MessageBox.Show("Daddy is missing :-(");
				return false;
			}
		}
		
		public bool DisplayFoundCalendar(bool bModal)
		{
			_Daddy.UpdateHttpDefaultWebProxy();
			String owner = ConfigurationManager.AppSettings["owner"];

			// On checke que les L/MDP soient corrects
			// Et on récupère les cookies au passage
			CookieContainer cookieJar = _Daddy.CheckGCAccount(true, false);
			if (cookieJar == null)
				return false;

			_Daddy._ThreadProgressBarTitle = _Daddy.GetTranslator().GetString("LblFetchingProgressInfoMsg");
			_Daddy.CreateThreadProgressBar();
			string result = _Daddy.GetCacheHTMLFromClientImpl("https://www.geocaching.com/my/logs.aspx?s=1&lt=2", cookieJar);

			
			// On extrait la table
			result = MyTools.GetSnippetFromText("<table", "</table>", result);

			// Chaque <tr est un found it
			List<String> founds = MyTools.GetSnippetsFromText("<tr", "</tr>", result);

			// on parcourt chaque foundit
			Dictionary<DateTime, int> date_founds = new Dictionary<DateTime, int>();
			foreach (String bloc in founds)
			{
				// on splitte en <td
				List<String> cols = MyTools.GetSnippetsFromText("<td>", "</td>", bloc);

				// 2 : date
				String date = cols[2];
				date = MyTools.CleanString(date);
				DateTime ddate;
				if (DateTime.TryParse(date, out ddate) == true)
				{
					ddate = new DateTime(ddate.Year, ddate.Month, ddate.Day);
					if (date_founds.ContainsKey(ddate))
					{
						date_founds[ddate] = date_founds[ddate] + 1;
					}
					else
					{
						date_founds.Add(ddate, 1);
					}

				}
			}
			_Daddy.KillThreadProgressBar();

			FoundCalendar cal = new FoundCalendar(date_founds);
			cal.Icon = _Daddy.Icon;
			if (bModal)
				cal.ShowDialog();
			else
				cal.Show();
			return true;
		}
		
		public bool DoItCal()
		{
			if (_Daddy != null)
			{
				return DisplayFoundCalendar(false);
			}
			else
			{
				MessageBox.Show("Daddy is missing :-(");
				return false;
			}
		}
		
		public bool GenerateFieldNotes(bool bGetSelOnly, bool bAutoFile, ref String field_notes, DateTime deb, DateTime fin)
		{
		
			// Liste de commentaires par défaut
			List<String> l = new List<string>();
			l.Add("Jolie cache, merci pour la découverte du lieu ! Merci pour la cache !");
			l.Add("Trouvée, mais c'était pas évident. Merci pour la cache !");
			l.Add("Merci pour cette cache et la promenade. Merci pour la cache !");
			l.Add("Une belle journée de géocaching qui m'a permis de trouver de nouvelles caches, merci. Merci pour la cache !");
			l.Add("Cache trouvée rapidement, sympa comme endroit. Merci pour la cache !");
			l.Add("Une de plus au compteur, je crois que je suis devenu accro. Merci pour la cache !");
			l.Add("Merci pour la cache !");
			l.Add("En promenade dans le coin j'en profite pour faire des caches. Merci pour la cache !");
			l.Add("J'ai un peu cherché pour celle-ci je ne devais pas être trop inspiré. Merci pour la cache !");
			l.Add("Très bon emplacement pour la cache. Merci pour la cache !");
			l.Add("Merci pour m'avoir montré cet endroit. Merci pour la cache !");
			l.Add("Jolie découverte, c'est sympa. Merci pour la cache !");
			l.Add("Un coin cool, j'aime beaucoup. Merci pour la cache !");
			l.Add("Etrangement très calme, pas de problème. Merci pour la cache !");
			l.Add("Je sens que je suis sur une bonne série, pas de problème. Merci pour la cache !");
			l.Add("Relativement facile à trouver, cool. Merci pour la cache !");
			l.Add("Rapidement délogée. Merci pour la cache !");
			l.Add("Et voilà, encore une cache, cool. Merci pour la cache !");
			l.Add("Je sens que j'ai un bon fluide aujourd'hui. Merci pour la cache !");
			l.Add("Trouvée sans trop de difficulté. Merci pour la cache !");
			l.Add("Des gens au loin, sûrement des moldus. Merci pour la cache !");
			l.Add("Je devais dormir j'ai mis du temps à la voir celle-là. Merci pour la cache !");
			l.Add("Bonne coordonnées, super. Merci pour la cache !");
			l.Add("J'ai un peu tourné en rond à cause de mon GPS capricieux. Merci pour la cache !");
			l.Add("Et une boite de plus, cool. Merci pour la cache !");
			l.Add("Et hop main mise sur la boite et le logbook. Merci pour la cache !");
			l.Add("endroit sympa qui mérite le détour. Merci pour la cache !");
			l.Add("Merci au poseur pour cette cache. Merci pour la cache !");
			l.Add("En route vers la suivante, sympa. Merci pour la cache !");
			l.Add("Et on continue la promenade. Merci pour la cache !");
			
			// On le fait pour les caches affichées
			List<Geocache> caches = null;
			if (bGetSelOnly)
				caches = _Daddy.GetSelectedCaches();
			else
				caches = _Daddy.GetDisplayedCaches();
			if (caches.Count == 0)
				return false;
			
			// On demande la date de début et de fin du run
			List<ParameterObject> lst = new List<ParameterObject>();
			lst.Add(new ParameterObject(ParameterObject.ParameterType.Label, "" , "nb", caches.Count.ToString() + " caches vont être traitées !"));
			lst.Add(new ParameterObject(ParameterObject.ParameterType.Date, DateTime.Now, "date", "Date de début"));
			lst.Add(new ParameterObject(ParameterObject.ParameterType.Date, DateTime.Now, "date", "Date de fin"));
			
			ParametersChanger changer = new ParametersChanger();
			changer.Title = "Période du run ?";
			changer.BtnCancel = _Daddy.GetTranslator().GetString("BtnCancel");
			changer.BtnOK = _Daddy.GetTranslator().GetString("BtnOk");
			changer.ErrorFormater = _Daddy.GetTranslator().GetString("ErrWrongParameter");
			changer.ErrorTitle = _Daddy.GetTranslator().GetString("Error");
			changer.Parameters = lst;
			changer.Font = _Daddy.Font;
			changer.Icon = _Daddy.Icon;

			bool bDateDefined = false;
			if ((deb != DateTime.MinValue) && (fin != DateTime.MinValue))
				bDateDefined = true;
			else
			{
				// On affiche le calendrier des trouvailles
				DisplayFoundCalendar(true);
			}
			
			if (bDateDefined || (changer.ShowDialog() == DialogResult.OK))
			{
				if (bDateDefined == false)
				{
					deb = (DateTime)(lst[1].ValueO);
					fin = (DateTime)(lst[2].ValueO);
					//MSG(deb.ToString("yyyy-MM-ddT00:00:1Z"));
				}
				
				// Nombre de jours du run
				if (deb > fin)
				{
					_Daddy.MSG("La date de début doit être antérieure à la date de fin !");
					return false;
				}
				int nbJours = (int)((fin - deb).TotalDays) + 1;
				int index = 0;
				int cachesperday = (int)((double)(caches.Count) / (double)nbJours + 1.0);
				DateTime dateCache =  deb;
				SaveFileDialog saveFileDialog1 = new SaveFileDialog();
				saveFileDialog1.Filter = "Field note (*.txt)|*.txt";
				saveFileDialog1.FileName = "geocache_visits.txt";
				saveFileDialog1.RestoreDirectory = true;
				if (bAutoFile)
				{
					String exePath = Path.GetDirectoryName(Application.ExecutablePath);
					saveFileDialog1.FileName = exePath + Path.DirectorySeparatorChar + Guid.NewGuid().ToString();
				}
				
				if (bAutoFile || (saveFileDialog1.ShowDialog() == DialogResult.OK))
				{
					FileInfo fi = new FileInfo(saveFileDialog1.FileName);
					field_notes = saveFileDialog1.FileName;
					Directory.SetCurrentDirectory(fi.Directory.ToString());
					String fileRadix = fi.Name.ToString();
					System.IO.StreamWriter file = new System.IO.StreamWriter(fileRadix, false, System.Text.Encoding.GetEncoding("iso-8859-8"));
					foreach(Geocache geo in caches)
					{
						String comment = l[MyTools.RandomNumber(l.Count - 1)];
					
						file.WriteLine(geo._Code + "," + 
									   dateCache.ToString("yyyy-MM-ddT00:00Z") + "," +
									   "Found it," +
									   "\"" + comment + "\"");

						index++;
						if (index >= cachesperday)
						{
							// Jour suivant
							index = 0;
							dateCache = dateCache.AddDays(1.0);
						}
					}
					file.Close();
				}
			}
			return true;
		}
		
		public bool DefineArea()
		{
			if (_Daddy != null)
			{
				// Définition de la zone filtre
				// On a juste à afficher la carto et à sélectionner le mode draw area
				_Daddy.ShowCacheMapInCacheDetail();
				_Daddy._cacheDetail.SelectAreaDrawingMode();
				return true;
			}
			else
			{
				MessageBox.Show("Daddy is missing :-(");
				return false;
			}
		}
		
		public bool DoIt()
		{
			if (_Daddy != null)
			{
				String field_notes = "";
                return GenerateFieldNotes(true, false, ref field_notes, DateTime.MinValue, DateTime.MinValue);
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
	
	/// <summary>
    /// A class to display days when you found caches
    /// </summary>
    public partial class FoundCalendar : Form
    {
        Dictionary<DateTime, int> _date_founds = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="date_founds">list of founds per day</param>
        public FoundCalendar(Dictionary<DateTime, int> date_founds)
        {
            _date_founds = date_founds;
            InitializeComponent();

            foreach (KeyValuePair<DateTime, int> pair in _date_founds)
            {
                myCal.AddBoldedDate(pair.Key);
            }

            DateTime today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            if (_date_founds.ContainsKey(today))
            {
                lblFound.Text = today.ToString("dd MMM: ") + _date_founds[today].ToString() + " found(s)";
            }
        }

        private void myCal_DateSelected(object sender, DateRangeEventArgs e)
        {
            DateTime deb = new DateTime(e.Start.Year, e.Start.Month, e.Start.Day);
            DateTime fin = new DateTime(e.End.Year, e.End.Month, e.End.Day);

            if (deb == fin)
            {
                if (_date_founds.ContainsKey(deb))
                {
                    lblFound.Text = deb.ToString("dd MMM: ") + _date_founds[deb].ToString() + " found(s)";
                }
                else
                    lblFound.Text = "";
            }
            else
            {
                // On compte le nombre de found
                int nb = 0;
                DateTime cur = deb;
                do
                {
                    if (_date_founds.ContainsKey(cur))
                    {
                        nb += _date_founds[cur];
                    }
                    cur = cur.AddDays(1.0);
                }
                while (cur <= fin);

                lblFound.Text = deb.ToString("dd MMM-> ") + fin.ToString("dd MMM: ") + nb.ToString() + " found(s)";
            }
        }
		
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.myCal = new System.Windows.Forms.MonthCalendar();
            this.lblFound = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // myCal
            // 
            this.myCal.Location = new System.Drawing.Point(0, 0);
            this.myCal.Margin = new System.Windows.Forms.Padding(0);
            this.myCal.MaxSelectionCount = 31;
            this.myCal.Name = "myCal";
            this.myCal.TabIndex = 0;
            this.myCal.DateSelected += new System.Windows.Forms.DateRangeEventHandler(this.myCal_DateSelected);
            // 
            // lblFound
            // 
            this.lblFound.AutoSize = true;
            this.lblFound.Location = new System.Drawing.Point(6, 165);
            this.lblFound.Name = "lblFound";
            this.lblFound.Size = new System.Drawing.Size(0, 13);
            this.lblFound.TabIndex = 1;
            // 
            // FoundCalendar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(226, 184);
            this.Controls.Add(this.lblFound);
            this.Controls.Add(this.myCal);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FoundCalendar";
            this.Text = "Found calendar";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MonthCalendar myCal;
        private System.Windows.Forms.Label lblFound;
    }
}