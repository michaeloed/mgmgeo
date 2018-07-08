using System;
using MyGeocachingManager;
using System.IO;
using System.Xml;
using SpaceEyeTools;
using SpaceEyeTools.HMI;
using System.Windows.Forms;
using System.Collections.Generic;
using GMap.NET;
using MyGeocachingManager.Geocaching.Filters;
using GMap.NET.WindowsForms;
using System.Configuration;
using System.Net;
using System.ComponentModel;
using System.IO.Compression;
using MyGeocachingManager.Geocaching;
using System.Text;
using System.Data.SQLite;
using System.Globalization;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace MyGeocachingManager.HMI
{
	/// <summary>
	/// Description of FranceKml.
	/// </summary>
	public class KmlManager
	{
		MainWindow _daddy = null;
		bool _downloadinprogress = false;
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="daddy"></param>
		public KmlManager(MainWindow daddy)
		{
			_daddy = daddy;
		}
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
		public void FilterOnFrenchArea(int type)
		{
            //if (!SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled()) return;

            if (type == 1)
				FilterOnFrenchAreaImpl(type, "LblRegionSelection");
			else if (type == 2)
				FilterOnFrenchAreaImpl(type, "LblDepartementSelection");
			else if (type == 3)
			{
				//Un petit filtre pour choisir la taille de la ville
	            List<ParameterObject> lst = new List<ParameterObject>();
	            List<String> lstypes = new List<string>();
	            lstypes.Add(_daddy.GetTranslator().GetString("LblVilleGrande"));
	            lstypes.Add(_daddy.GetTranslator().GetString("LblVilleMoyenne"));
	            lstypes.Add(_daddy.GetTranslator().GetString("LblVillePetite"));
	            
	            lst.Add(new ParameterObject(ParameterObject.ParameterType.List, lstypes, "choix", _daddy.GetTranslator().GetString("LblVilleTaille")));
	
	            ParametersChanger changer = new ParametersChanger();
	            changer.Title = _daddy.GetTranslator().GetString("LblVilleTaille");
	            changer.BtnCancel = _daddy.GetTranslator().GetString("BtnCancel");
	            changer.BtnOK = _daddy.GetTranslator().GetString("BtnOk");
	            changer.ErrorFormater = _daddy.GetTranslator().GetString("ErrWrongParameter");
	            changer.ErrorTitle = _daddy.GetTranslator().GetString("Error");
	            changer.Parameters = lst;
	            changer.Font = _daddy.Font;
	            changer.Icon = _daddy.Icon;
	            
	            if (changer.ShowDialog() == DialogResult.OK)
	            {
	                String city = lst[0].Value;
	                int i = type;
	                if (city == _daddy.GetTranslator().GetString("LblVilleGrande"))
	                	i = 3;
	                else if (city == _daddy.GetTranslator().GetString("LblVilleMoyenne"))
	                	i = 4;
	                else if (city == _daddy.GetTranslator().GetString("LblVillePetite"))
	                	i = 5;
	                FilterOnFrenchAreaImpl(i, "LblVilleSelection");
	            }
			}
			
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public List<Tuple<String,List<PointLatLng>>> GetFranceAreasDB(int type)
		{
			List<Tuple<String,List<PointLatLng>>> areas = new List<Tuple<string, List<PointLatLng>>>();
			try
			{
				var cs = "URI=file:" + _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "FrenchAreas.lay";
				using (SQLiteConnection con = new SQLiteConnection(cs))
                {
                    con.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(con))
                    {
                        // retrieve values
                        String stm = "SELECT Name, Points from FrenchAreas WHERE Layout = " + type.ToString();
                        using (SQLiteCommand cmd2 = new SQLiteCommand(stm, con))
                        {
                            using (SQLiteDataReader rdr = cmd2.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                	String name = rdr.GetString(0);
                                	String pts = rdr.GetString(1);
                                	
                                	// On construit la liste
                                	String[] elts = pts.Split(' ');
                                	List<PointLatLng> vector = new List<PointLatLng> ();
                                	for(int i=0;i<elts.Length;i+=2)
                                	{
                                		double lat, lng;
                                		lat = MyTools.ConvertToDoubleFast(elts[i]);
                                		lng = MyTools.ConvertToDoubleFast(elts[i+1]);
                                		
		                                vector.Add(new PointLatLng(lat, lng));
                                	}
                                	areas.Add(new Tuple<String,List<PointLatLng>>(name, vector));
                                }
                            }
                        }
                    }
                    con.Close();
                }
			}
			catch(Exception ex)
			{
				//_daddy.KillThreadProgressBar();
                _daddy.ShowException("", "An error in GetFranceAreasDB", ex);
			}
			return areas;
		}
		
		/// <summary>
		/// Uses the Douglas Peucker algorithm to reduce the number of points.
		/// http://www.codeproject.com/Articles/18936/A-Csharp-Implementation-of-Douglas-Peucker-Line-Ap
		/// </summary>
		/// <param name="Points">The points.</param>
		/// <param name="Tolerance">The tolerance.</param>
		/// <returns></returns>
		public static List<PointLatLng> DouglasPeuckerReduction
		    (List<PointLatLng> Points, Double Tolerance)
		{
		    if (Points == null || Points.Count < 3)
		    return Points;
		
		    if (Tolerance == 0.0)
		    	return Points;
		    
		    Int32 firstPoint = 0;
		    Int32 lastPoint = Points.Count - 1;
		    List<Int32> pointIndexsToKeep = new List<Int32>();
		
		    //The first and the last point cannot be the same
		    while (Points[firstPoint].Equals(Points[lastPoint]))
		    {
		        lastPoint--;
		    }
		
		    //Add the first and last index to the keepers
		    pointIndexsToKeep.Add(firstPoint);
		    pointIndexsToKeep.Add(lastPoint);
		
		    
		    DouglasPeuckerReduction(Points, firstPoint, lastPoint, 
		    Tolerance, ref pointIndexsToKeep);
		
		    List<PointLatLng> returnPoints = new List<PointLatLng>();
		    pointIndexsToKeep.Sort();
		    foreach (Int32 index in pointIndexsToKeep)
		    {
		        returnPoints.Add(Points[index]);
		    }
		
		    return returnPoints;
		}
		    
		/// <summary>
		/// Douglases the peucker reduction.
		/// </summary>
		/// <param name="points">The points.</param>
		/// <param name="firstPoint">The first point.</param>
		/// <param name="lastPoint">The last point.</param>
		/// <param name="tolerance">The tolerance.</param>
		/// <param name="pointIndexsToKeep">The point index to keep.</param>
		private static void DouglasPeuckerReduction(List<PointLatLng> 
		    points, Int32 firstPoint, Int32 lastPoint, Double tolerance, 
		    ref List<Int32> pointIndexsToKeep)
		{
		    Double maxDistance = 0;
		    Int32 indexFarthest = 0;
		    
		    for (Int32 index = firstPoint; index < lastPoint; index++)
		    {
		        Double distance = PerpendicularDistance
		            (points[firstPoint], points[lastPoint], points[index]);
		        if (distance > maxDistance)
		        {
		            maxDistance = distance;
		            indexFarthest = index;
		        }
		    }
		
		    if (maxDistance > tolerance && indexFarthest != 0)
		    {
		        //Add the largest point that exceeds the tolerance
		        pointIndexsToKeep.Add(indexFarthest);
		    
		        DouglasPeuckerReduction(points, firstPoint, 
		        indexFarthest, tolerance, ref pointIndexsToKeep);
		        DouglasPeuckerReduction(points, indexFarthest, 
		        lastPoint, tolerance, ref pointIndexsToKeep);
		    }
		}
		
		/// <summary>
		/// The distance of a point from a line made from point1 and point2.
		/// </summary>
		/// <param name="Point1">The PT1.</param>
		/// <param name="Point2">The PT2.</param>
		/// <param name="Point">The p.</param>
		/// <returns></returns>
		public static Double PerpendicularDistance
		    (PointLatLng Point1, PointLatLng Point2, PointLatLng Point)
		{

		    Double area = Math.Abs(.5 * (Point1.Lng * Point2.Lat + Point2.Lng * 
		    Point.Lat + Point.Lng * Point1.Lat - Point2.Lng * Point1.Lat - Point.Lng * 
		    Point2.Lat - Point1.Lng * Point.Lat));
		    Double bottom = Math.Sqrt(Math.Pow(Point1.Lng - Point2.Lng, 2) + 
		    Math.Pow(Point1.Lat - Point2.Lat, 2));
		    Double height = area / bottom * 2;
		
		    return height;
		    
		    
		}
		

		private enum ShrinkingType
		{
			Decimation,
			DouglasPeuckerReduction
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="usesmalldepartmentsifavail"></param>
		/// <param name="forcedecimation"></param>
		/// <param name="forcetolerance"></param>
		/// <param name="typedeb"></param>
		/// <param name="typeend"></param>
		/// <param name="silent"></param>
		public void GenerateFranceAreasDB(bool usesmalldepartmentsifavail = false, int forcedecimation = 0, double forcetolerance = 0, int typedeb = 1, int typeend = 3, bool silent = false)
		{
			try
			{
				_daddy.ClearOverlay_RESERVED2();
				// On a 3 layout maximum
				ShrinkingType[] eShrinkingTypes = new ShrinkingType[]{
					ShrinkingType.DouglasPeuckerReduction,
					ShrinkingType.DouglasPeuckerReduction,
					ShrinkingType.DouglasPeuckerReduction
				};
				double[] vShrinkingParameter = new double[]
				{
					0.03, // 0.01 0.003 pour 1000 pour les cantons en DouglasPeuckerReduction
					0.02, // 0.01 0.003 pour 1000 pour les départements en DouglasPeuckerReduction
					0.01 // 0.01 0.005 pour 1000 pour les cantons en DouglasPeuckerReduction
				};
				
				_daddy._ThreadProgressBarTitle = "Génération des calques";
				_daddy.CreateThreadProgressBarEnh();
				
				// Wait for the creation of the bar
                while (_daddy._ThreadProgressBar == null)
                {
                    Thread.Sleep(10);
                    Application.DoEvents();
                }
				
				String msg = "";
                String filename = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "FrenchAreas.lay";
                if (File.Exists(filename))
                	File.Delete(filename);
                
				int index = 0;
            	String dbpath = "URI=file:" + filename;
            	using ( SQLiteConnection con = new SQLiteConnection(dbpath))
				{
					con.Open();
					using (SQLiteCommand cmd = new SQLiteCommand(con))
					{
						// Drop table
						cmd.CommandText = "DROP TABLE IF EXISTS FrenchAreas";
						cmd.ExecuteNonQuery();
						
						// Layout : 
						// 1 Region
						// 2 Departement
						// 3 Canton
						// 4 Villes
						// 5 Communes
						cmd.CommandText = @"CREATE TABLE IF NOT EXISTS FrenchAreas(
											Id INTEGER PRIMARY KEY,
											Layout INTEGER,
											Points TEXT,
											Size INTEGER,
											Name TEXT,
											Number TEXT
											)";
						cmd.ExecuteNonQuery();
						
						// Les regions et les communes viennent de FRA_adm1 et FRA_adm3
						// Les départements sont déjà simplifiés
						TypeKML typekml = TypeKML.FRA_adm;
						for(int type=typedeb;type<=typeend;type++)
						{
							msg += "Type : " + type.ToString() + "\r\n";
							int nb  = 0;
							string[] filePaths;
							if (usesmalldepartmentsifavail && (type == 2))
							{
								// On utilise notre base bien plus détaillée si elle existe
								String kmlpath = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "KML" + Path.DirectorySeparatorChar + "Dpts"  + Path.DirectorySeparatorChar ;
								if (Directory.Exists(kmlpath))
								{
									// Les petits fichiers département
			            			filePaths = Directory.GetFiles(kmlpath, "*.kml", SearchOption.AllDirectories);
			            			typekml = TypeKML.Dpts;
								}
								else
								{
									// Le gros fichier FranceKML
									filePaths = new string[1];
			            			filePaths[0] = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "KML" + Path.DirectorySeparatorChar + "FRA_adm" + type.ToString() + ".kml";
			            			typekml = TypeKML.FRA_adm;
								}
							}
							else
							{
								// Le gros fichier FranceKML
			            		filePaths = new string[1];
			            		filePaths[0] = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "KML" + Path.DirectorySeparatorChar + "FRA_adm" + type.ToString() + ".kml";
			            		typekml = TypeKML.FRA_adm;
							}
							
			            	foreach (string f in filePaths)
			                {            				
			                	Dictionary<String,List<List<PointLatLng>>> dico;
			                	if (KmlManager.ReadKml(f, out dico, typekml))
			                	{
			                		foreach(KeyValuePair<String,List<List<PointLatLng>>> pair in dico)
			                		{
			                			// Prepare transaction
			            				SQLiteTransaction transaction = con.BeginTransaction();
			            				cmd.CommandText = 
											@"INSERT OR REPLACE INTO FrenchAreas(
											Id,
											Layout,
											Points,
											Size,
											Name,
											Number
											) VALUES(
											@Id,
											@Layout,
											@Points,
											@Size,
											@Name,
											@Number
											)";
			            				cmd.Parameters.AddWithValue("@Id", 0);	
			            				cmd.Parameters.AddWithValue("@Layout", 0);	
			            				cmd.Parameters.AddWithValue("@Points", "");	
			            				cmd.Parameters.AddWithValue("@Size", 0);	
			            				cmd.Parameters.AddWithValue("@Name", "");	
			            				cmd.Parameters.AddWithValue("@Number", "");
			            				
			                			var name = pair.Key;
			                			// On supprime après le # c'est styleUrl
			                			int pos = name.IndexOf('#');
			                			if (pos != -1)
			                				name = name.Substring(0, pos);
			                			
			                			// Découpage nom / numéro
				                		String number = "";
				                		if (typekml == TypeKML.Dpts)
				                		{
				                			pos = name.IndexOf('-');
				                			number = name.Substring(0, pos -1);
				                			name = name.Substring(pos + 2);
				                		}
							
				                		int nbpol = 0;
				                		foreach(var oldpoly in pair.Value)
				                		{
				                			nbpol++;
				                			_daddy._ThreadProgressBar.lblWait.Text = name + "(" + nbpol.ToString() + ")";
				                			var poly = oldpoly;
				                			
				                			// On regarde quelle reduction appliquer
				                			var reduc = eShrinkingTypes[type - 1];
				                			if (reduc == ShrinkingType.Decimation)
				                			{
				                				int dec = (forcedecimation == 0)? (int)(vShrinkingParameter[type -1]):forcedecimation;
				                				poly = MyTools.DecimateList<PointLatLng>(oldpoly, dec);
				                			}
				                			else if (reduc == ShrinkingType.DouglasPeuckerReduction)
				                			{
				                				double tol = (forcetolerance == 0.0)? vShrinkingParameter[type -1]:forcetolerance;
				                				poly = KmlManager.DouglasPeuckerReduction(oldpoly, tol);
				                			}
				                			
				                			GMapPolygon area = new GMapPolygon(poly, name);
				                			area.Tag = null;
				                			area.Fill = new SolidBrush(Color.FromArgb(100, 0, 255, 0));
				                			area.Stroke = new Pen(Color.Green);
				                			_daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2].Polygons.Add(area);
				                			
				                			msg += name + " " + poly.Count.ToString() + "\r\n";
				                			nb += poly.Count;
				                			
				                			String points = "";
				                			foreach(var pt in poly)
				                			{
				                				points += pt.Lat.ToString().Replace(',','.') + " " + pt.Lng.ToString().Replace(',','.') + " ";
				                			}
				                			points = points.Substring(0, points.Length - 1);
				                			
				                			
				                			cmd.Parameters["@Id"].Value = index++;
				                			cmd.Parameters["@Layout"].Value = type;
											cmd.Parameters["@Points"].Value = points;
											cmd.Parameters["@Size"].Value = poly.Count;
											cmd.Parameters["@Name"].Value = name;
											cmd.Parameters["@Number"].Value = number;
											cmd.ExecuteNonQuery();
				                		}
				                		
				                		// en transaction
	                					transaction.Commit();
			                		}
			                	}
			                	
			                	
			                }
							msg += "TOTAL : " + nb.ToString() + "\r\n************************************************************\r\n";			            	
						}
					}
					con.Close();
            	}
            	
            	
            	_daddy.KillThreadProgressBarEnh();
                _daddy.ShowCacheMapInCacheDetail();
                
                if (!silent)
                {
                	_daddy.MsgActionDone(_daddy, "Fichier généré : " + filename + "\r\n" + msg, false);
                }
			}
			catch (Exception ex)
            {
                _daddy.KillThreadProgressBarEnh();
                _daddy.ShowException("", "An error in GenerateFranceAreasDB", ex);
            }
		}
		
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool GenerateFranceCoverage()
		{
			try
            {
				//if (!SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled()) return;
				
				_daddy.ClearOverlay_RESERVED2();
				// On vérifie la présence des fichiers !
				// 1 : Régions
                // 2 : Départements
                // 3 : Villes
                bool dataavail = true;
                for(int i=1;i<=3;i++)
                {
                	String fi = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "KML" + Path.DirectorySeparatorChar + "FRA_adm" + i + ".kml";
                	dataavail = dataavail && File.Exists(fi);
                }
                if (!dataavail)
            	{
            		// Message d'erreur, on invite à télécharger la base
            		if (_downloadinprogress)
            			return false;
	                if (_daddy.GetInternetStatus())
	                {
	                    DialogResult dialogResult = MyMessageBox.Show(_daddy.GetTranslator().GetStringM("AskDownloadKMLNet"),
	                                      _daddy.GetTranslator().GetString("AskDownloadKMLTitle"),
	                                      MessageBoxIcon.Question, _daddy.GetTranslator());
	                    if (dialogResult == DialogResult.Yes)
	                    {
	                        // Ok on télécharge
	                        DownloadDB();
	                    }
	                }
	                else
	                {
	                    _daddy.MsgActionError(_daddy, _daddy.GetTranslator().GetStringM("AskDownloadKMLNoNet"));
	                }
                    return false;
            	}
				
				// On demande si on veut aussi générer le calque pour Geo France (si on n'a pas trouvé le fichier dans data)
				// *************************************************************
				String filenamelayer = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "FrenchAreas.lay";
                if (!File.Exists(filenamelayer))
                {
                	var dialogResult2 = MyMessageBox.Show(_daddy.GetTranslator().GetStringM("lblAskGenerateLayoutFrance"),
	                                      _daddy.GetTranslator().GetString("lblAskGenerateLayoutFranceTitle"),
	                                      MessageBoxIcon.Question, _daddy.GetTranslator());
			        if (dialogResult2 == DialogResult.Yes)
			        {
			        	GenerateFranceAreasDB(false, 0, 0.0, 1, 3, true);
			        	filenamelayer = " ( + " + filenamelayer + ")";
			        }
			        else 
			        	filenamelayer = "";
                }
                else
                	filenamelayer = "";
		        
                _daddy._ThreadProgressBarTitle = _daddy.GetTranslator().GetString("generateFranceCoverageToolStripMenuItem");
				_daddy.CreateThreadProgressBarEnh();
				// Wait for the creation of the bar
                while (_daddy._ThreadProgressBar == null)
                {
                    Thread.Sleep(10);
                    Application.DoEvents();
                }
                
				String filename = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "MyFindFrenchAreas.lay";
				if (File.Exists(filename))
                	File.Delete(filename);
                
				int index = 0;
				//System.IO.StreamWriter file = new System.IO.StreamWriter(filename, false);
            	String dbpath = "URI=file:" + filename;// + ";Compress=True;";
            	using ( SQLiteConnection con = new SQLiteConnection(dbpath))
				{
					con.Open();
					using (SQLiteCommand cmd = new SQLiteCommand(con))
					{
						// Drop table
						cmd.CommandText = "DROP TABLE IF EXISTS MyFindFrenchAreas";
						cmd.ExecuteNonQuery();
						
						cmd.CommandText = @"CREATE TABLE IF NOT EXISTS MyFindFrenchAreas(
											Id INTEGER PRIMARY KEY,
											Layout INTEGER,
											Value INTEGER,
											Name TEXT
											)";
						cmd.ExecuteNonQuery();
						
						List<Tuple<Int32, String, List<GMapPolygon>>> allZones;
						for(int i=1;i<=3;i++) // Max 3
						{
							if (!DisplayFranceCoverageImpl(i, out allZones, TrackSelector.ColorType.GreenRed, false, false))
							{
								con.Close();
								_daddy.KillThreadProgressBarEnh();
								return false;
							}
							
							// Prepare transaction
            				SQLiteTransaction transaction = con.BeginTransaction();
            				cmd.CommandText = 
								@"INSERT OR REPLACE INTO MyFindFrenchAreas(
								Id,
								Layout,
								Value,
								Name
								) VALUES(
								@Id,
								@Layout,
								@Value,
								@Name
								)";
            				cmd.Parameters.AddWithValue("@Id", 0);	
            				cmd.Parameters.AddWithValue("@Layout", 0);	
            				cmd.Parameters.AddWithValue("@Value", 0);	
            				cmd.Parameters.AddWithValue("@Name", "");	
            				
							foreach(Tuple<Int32, String, List<GMapPolygon>> tpl in allZones)
							{
								_daddy._ThreadProgressBar.lblWait.Text = tpl.Item2;
							
								//file.WriteLine(i.ToString() + "¤" + tpl.Item2 + "¤" + tpl.Item1.ToString());
								cmd.Parameters["@Id"].Value = index++;
								cmd.Parameters["@Layout"].Value = i;
								cmd.Parameters["@Value"].Value = tpl.Item1;
								cmd.Parameters["@Name"].Value = tpl.Item2;
								cmd.ExecuteNonQuery();
							}
							
							// en transaction
                			transaction.Commit();
						}
						//file.Close();
					}
					con.Close();
            	}
            	
            	//_daddy.ClearOverlay_RESERVED2();
            	 _daddy.KillThreadProgressBarEnh();
				_daddy.MsgActionDone(_daddy, String.Format(_daddy.GetTranslator().GetStringM("lblFranceCoverageLayout"), filename + filenamelayer), false);
            	 return true;
			}
			catch (Exception ex)
            {
                 _daddy.KillThreadProgressBarEnh();
                _daddy.ShowException("", "An error in GenerateFranceCoverage", ex);
                return false;
            }
		}
		
		/// <summary>
		/// 
		/// </summary>
		public void DisplayFranceCoverage()
		{
			try
            {
            	
                //Un petit filtre pour choisir la taille de la ville
                List<ParameterObject> lst = new List<ParameterObject>();
                List<String> lstypes = new List<string>();
                lstypes.Add(_daddy.GetTranslator().GetString("lblDisplayRegions"));
                lstypes.Add(_daddy.GetTranslator().GetString("lblDisplayDepartements"));
                lstypes.Add(_daddy.GetTranslator().GetString("lblDisplayCantons"));
                lstypes.Add(_daddy.GetTranslator().GetString("lblDisplayVilles"));
                lstypes.Add(_daddy.GetTranslator().GetString("lblDisplayCommunes"));
                

                lst.Add(new ParameterObject(ParameterObject.ParameterType.List, lstypes, "choix",  _daddy.GetTranslator().GetString("displayFranceCoverageToolStripMenuItem")));
				List<String> lstcompare = new List<string>();
            	lstcompare.Add(_daddy.GetTranslator().GetString("LblSpeedGreenRed"));
	            lstcompare.Add(_daddy.GetTranslator().GetString("LblSpeedHSL"));
	            lstcompare.Add(_daddy.GetTranslator().GetString("LblSpeedUnicolor"));
	            ParameterObject po = new ParameterObject(ParameterObject.ParameterType.List, lstcompare, "speedcolor", _daddy.GetTranslator().GetString("LblColorType"));
	            po.DefaultListValue = _daddy.GetTranslator().GetString("LblSpeedUnicolor");
	            lst.Add(po);
            
                ParametersChanger changer = new ParametersChanger();
                changer.Title = _daddy.GetTranslator().GetString("displayFranceCoverageToolStripMenuItem");
                changer.BtnCancel = _daddy.GetTranslator().GetString("BtnCancel");
                changer.BtnOK = _daddy.GetTranslator().GetString("BtnOk");
                changer.ErrorFormater = _daddy.GetTranslator().GetString("ErrWrongParameter");
                changer.ErrorTitle = _daddy.GetTranslator().GetString("Error");
                changer.Parameters = lst;
                changer.Font =  _daddy.Font;
                changer.Icon =  _daddy.Icon;
                int type = 2;
                TrackSelector.ColorType colorType = TrackSelector.ColorType.GreenRed;
                if (changer.ShowDialog() == DialogResult.OK)
                {
                    String s = lst[0].Value;
                    colorType = (TrackSelector.ColorType)(changer.Parameters[1].ValueIndex);
                    if (s == _daddy.GetTranslator().GetString("lblDisplayRegions"))
                    	type = 1;
                    else if (s == _daddy.GetTranslator().GetString("lblDisplayDepartements"))
                    	type = 2;
                    else if (s == _daddy.GetTranslator().GetString("lblDisplayCantons"))
                    	type = 3;
                	else if (s == _daddy.GetTranslator().GetString("lblDisplayVilles"))
                		type = 4;
                	else if (s == _daddy.GetTranslator().GetString("lblDisplayCommunes"))
                		type = 5;
                }
                else
                    return;

                List<Tuple<Int32, String, List<GMapPolygon>>> allZones;
                DisplayFranceCoverageImpl(type, out allZones, colorType);
            }
            catch (Exception ex)
            {
                _daddy.KillThreadProgressBar();
                _daddy.ShowException("", "An error in DisplayFranceCoverage", ex);
            }
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool DisplayWorldCoverage()
		{
			//if (!SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled()) return false;
			
			// Le travail sur le monde très résolu fait sauter la mémoire, c'est trop gros, dommage
			// ==> ce n'est plus le cas avec le nouveau chargement des XML			
			// A-t'on une base monde très résolue ?
			String index = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "KML" + Path.DirectorySeparatorChar + "World\\_index.dat";
			if (File.Exists(index))
			{
				return DisplayWorldCoverageHighResSimple(); // DisplayWorldCoverageHighResFull est trop complexe et fait sauter la mémoire
			}
			else
			{
				return DisplayWorldCoverageLowRes();
			}
			
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private bool DisplayWorldCoverageHighResSimple()
		{
			try
            {
            	_daddy.ClearOverlay_RESERVED2();
                
                // Si on est là, les fichiers KML résolus sont censés exister !
            	String kmlpath = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "KML" + Path.DirectorySeparatorChar + "World"  + Path.DirectorySeparatorChar;
            	
                    
                _daddy._ThreadProgressBarTitle =  _daddy.GetTranslator().GetString("LblOperationInProgress");
                _daddy.CreateThreadProgressBar();

                // On va parcourir tous les fichiers et créer les zone.
                // C'est du lourd, du très lourd
                string[] filePaths = Directory.GetFiles(kmlpath, "*.kml", SearchOption.AllDirectories);
                 
                // On récupère les caches affichées
                List<Geocache> caches =  _daddy.GetDisplayedCaches();
                 
                // Les zones
                _daddy._cacheDetail._gmap.HoldInvalidation = true;
            	foreach (string filename in filePaths)
            	{  
            		XmlDocument _xmldoc = new XmlDocument();
            		
        			// On charge le pays
	                _xmldoc.Load(filename);
	
	               	// Hop, on va lire le pays
	               	// On se trouve au niveau d'une zone que l'on veut allumer
            		// On va les stocker avant de les dessiner
                	List<GMapPolygon> areas = new List<GMapPolygon>();
                	bool trouve = false;
                	XmlNamespaceManager ns = new XmlNamespaceManager(_xmldoc.NameTable);
					ns.AddNamespace("ns1", "http://earth.google.com/kml/2.2");
					var nodes = _xmldoc.SelectNodes("/ns1:kml/ns1:Document/ns1:Placemark", ns);
            		foreach (XmlNode placemarknode in nodes)
            		{
                		XmlNode _xmlnode = placemarknode["MultiGeometry"];
                		String s = "";
                
                		// On parcourt tous les polygones de MultiGeometry
	                    foreach (XmlNode elt in _xmlnode.ChildNodes)
	                    {
	                    	// On est dans Polygon
	                        // On va lire les coordonnées
	                        // Polygon -> outerBoundaryIs -> LinearRing -> coordinates
	                        XmlNode coords = elt["outerBoundaryIs"];
		                	if (coords == null)
		                		continue;
		                	coords = coords["LinearRing"];
		                	coords = coords["coordinates"];
		                	s = coords.InnerText;
		                	s = s.Replace("\r\n","\n");
		                	s = s.Replace("\n","#");
		                	s = s.Replace("\t","");
		                	s = s.Replace(" ","#");
		                	String[] latlng = s.Split('#'); // lng, lat, alt?
	                        List<PointDouble> pts = new List<PointDouble>();
	                        List<PointLatLng> pts2 = new List<PointLatLng>();
	
	                        // On construit le polygone
	                        foreach (String ll in latlng)
	                        {
	                            String[] lalo = ll.Split(',');
	                            if (lalo.Length >= 2)
	                            {
	                            	double lat, lng;
	                            	lat = MyTools.ConvertToDoubleFast(lalo[1]);
	                            	lng = MyTools.ConvertToDoubleFast(lalo[0]);
	                            	
	                                pts.Add(new PointDouble(lng, lat));
	                                pts2.Add(new PointLatLng(lat, lng));
	                            }
	                        }
	
	                        // On affiche chaque zone
	                        for(int i=0;(!trouve)&&(i<caches.Count); i++)
	                        {
	                            Geocache geo = caches[i];
	                            PointDouble ptcache = new PointDouble(geo._dLongitude, geo._dLatitude);
	                            if (MyTools.PointInPolygon(ptcache, pts.ToArray()))
	                            {
	                            	trouve = true;
	                            }
	                        }
	                        
	                        // On crée la zone
	                        GMapPolygon area = new GMapPolygon(pts2, "mazone");
	                        // On ajoute pour dessiner plus tard
	                        areas.Add(area);
	                    }
            		}
                
                	// On ajoute ces zones au machin final
                	if (trouve)
                	{
                    	// On dessine
                    	foreach(var area in areas)
                    	{
		                	area.Tag = null;
		                	area.Fill = new SolidBrush(Color.FromArgb(100, 0, 255, 0));
				            area.Stroke = new Pen(Color.Green);
				            
				            
				            
		                    _daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2].Polygons.Add(area);
                    	}
                	}
                	else
                	{
                	
                		// Non trouvé
				        //area.Fill = new SolidBrush(Color.FromArgb(100, 255, 0, 0));
				        //area.Stroke = new Pen(Color.Red);
				        
                		// Si non affichable
                		areas.Clear();
                	}
                	
                	// Nettoyage
                	_xmldoc.RemoveAll();
                }
                
                _daddy._cacheDetail._gmap.Refresh();
                _daddy.KillThreadProgressBar();
                
                // On affiche proprement 
                _daddy.ShowCacheMapInCacheDetail();
                _daddy._cacheDetail._gmap.Zoom = 2.5;
            	_daddy._cacheDetail.Location = new Point(0,0);
        		_daddy._cacheDetail.Size = new Size(1200,1200);
        		_daddy._cacheDetail._gmap.Position = new PointLatLng(0, 0);
                
                return true;
            }
            catch (Exception ex)
            {
                _daddy.KillThreadProgressBar();
                _daddy.ShowException("", "An error in DisplayWorldCoverageHighRes", ex);
                return false;
            }
		}
		
		/// <summary>
		/// 
		/// </summary>
		public enum TypeKML
		{
			/// <summary>
			/// 
			/// </summary>
			World_ISO2 = 0,
			/// <summary>
			/// 
			/// </summary>
			FRA_adm = 1,
			/// <summary>
			/// 
			/// </summary>
			Dpts = 3
		};
			
		/// <summary>
		/// 
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="dico"></param>
		/// <param name="typekml"></param>
		/// <returns></returns>
		public static bool ReadKml(String filename, out Dictionary<String,List<List<PointLatLng>>> dico, TypeKML typekml = TypeKML.FRA_adm)
		{
			dico = null;
			String name = "";
			try
			{
				XmlDocument _xmldoc = new XmlDocument();
    			// On charge le kml
                _xmldoc.Load(filename);
                int i =0;
        		XmlNamespaceManager ns = new XmlNamespaceManager(_xmldoc.NameTable);
        		if (typekml == TypeKML.World_ISO2)
					ns.AddNamespace("ns1", "http://www.opengis.net/kml/2.2");
        		else
        			ns.AddNamespace("ns1", "http://earth.google.com/kml/2.2");
        		
				var nodes = _xmldoc.SelectNodes("/ns1:kml/ns1:Document/ns1:Placemark", ns);
				dico = new Dictionary<String,List<List<PointLatLng>>>();
            	foreach (XmlNode placemarknode in nodes)
        		{
            		// Le nom
            		if (typekml == TypeKML.World_ISO2)
            			name = placemarknode["name"].InnerText + "_" + (i++).ToString("0000");
            		else
            			name = placemarknode["name"].InnerText + placemarknode["styleUrl"].InnerText; // styleUrl pour éviter les doublons dans les clés
            		
            		var pts = new List<List<PointLatLng>>();
            		dico.Add(name, pts);
            		
            		XmlNode _xmlnode = placemarknode["MultiGeometry"];
            		String s = "";
            
            		// On parcourt tous les polygones de MultiGeometry
                    foreach (XmlNode elt in _xmlnode.ChildNodes)
                    {
                    	if (typekml != TypeKML.Dpts)
                    	{
                    		// On est dans Polygon
	                        // On va lire les coordonnées
	                        // Polygon -> outerBoundaryIs -> LinearRing -> coordinates
	                        XmlNode coords = elt["outerBoundaryIs"];
	                        if (coords == null)
		                		continue;
		                	coords = coords["LinearRing"]["coordinates"];
	                        s = coords.InnerText;
	                        
	                        //s = s.Replace(" ", "");
	                        //s = s.Replace("\r\n", "\n");
	                        //String[] latlng = s.Split('\n');
	                        
	                        s = s.Replace("\r\n","\n");
		                	s = s.Replace('\n','#');
		                	s = s.Replace("\t","");
		                	s = s.Replace(' ','#');
		                	String[] latlng = s.Split('#'); // lng, lat, alt?
		                	
	                        List<PointLatLng> pts2 = new List<PointLatLng>();
	
	                        // On construit le polygone
	                        foreach (String ll in latlng)
	                        {
	                            String[] lalo = ll.Split(',');
	                            if (lalo.Length >= 2)
	                            {
	                            	double lat, lng;
	                            	lat = MyTools.ConvertToDoubleFast(lalo[1]);
	                            	lng = MyTools.ConvertToDoubleFast(lalo[0]);
	                            		                            	
	                                pts2.Add(new PointLatLng(lat, lng));
	                            }
	                        }
	                        
	                        if (pts2.Count >= 3)
		                        pts.Add(pts2);
                    	}
                    	else
                    	{
                    		// Les fichiers kml de département
	                        // On va lire les coordonnées
	                        // LineString -> coordinates
	                        if (elt.Name == "LineString")
	                        {
		                        XmlNode coords = elt["coordinates"];
			                	if (coords == null)
			                		continue;
			                	s = coords.InnerText;
			                	s = s.Replace("\r\n","\n");
			                	s = s.Replace("\n","#");
			                	s = s.Replace("\t","");
			                	s = s.Replace(" ","#");
			                	
			                	String[] latlng = s.Split('#'); // lng, lat, alt?
		                        List<PointLatLng> pts2 = new List<PointLatLng>();
		
		                        // On construit le polygone
		                        foreach (String ll in latlng)
		                        {
		                            String[] lalo = ll.Split(',');
		                            if (lalo.Length >= 2)
		                            {
		                            	double lat, lng;
		                            	lat = MyTools.ConvertToDoubleFast(lalo[1]);
	                            		lng = MyTools.ConvertToDoubleFast(lalo[0]);
		                            	
		                                pts2.Add(new PointLatLng(lat, lng));
		                            }
		                        }
		                        if (pts2.Count >= 3)
		                        	pts.Add(pts2);
	                        }
                    	}
                    }
        		}
            
            	// Nettoyage
            	_xmldoc.RemoveAll();
            	return true;
			}
			catch(Exception)
			{
				return false;
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private bool DisplayWorldCoverageHighResFull()
		{
			try
            {
            	_daddy.ClearOverlay_RESERVED2();
                
                // Si on est là, les fichiers KML résolus sont censés exister !
            	String kmlpath = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "KML" + Path.DirectorySeparatorChar + "World"  + Path.DirectorySeparatorChar;
            	
                //Un petit filtre pour choisir le gradient
                List<ParameterObject> lst = new List<ParameterObject>();               
                List<String> lstcompare = new List<string>();
            	lstcompare.Add(_daddy.GetTranslator().GetString("LblSpeedGreenRed"));
	            lstcompare.Add(_daddy.GetTranslator().GetString("LblSpeedHSL"));
	            lstcompare.Add(_daddy.GetTranslator().GetString("LblSpeedUnicolor"));
	            ParameterObject po = new ParameterObject(ParameterObject.ParameterType.List, lstcompare, "speedcolor", _daddy.GetTranslator().GetString("LblColorType"));
	            po.DefaultListValue = _daddy.GetTranslator().GetString("LblSpeedUnicolor");
	            lst.Add(po);
                ParametersChanger changer = new ParametersChanger();
                changer.Title = _daddy.GetTranslator().GetString("displayFranceCoverageToolStripMenuItem");
                changer.BtnCancel = _daddy.GetTranslator().GetString("BtnCancel");
                changer.BtnOK = _daddy.GetTranslator().GetString("BtnOk");
                changer.ErrorFormater = _daddy.GetTranslator().GetString("ErrWrongParameter");
                changer.ErrorTitle = _daddy.GetTranslator().GetString("Error");
                changer.Parameters = lst;
                changer.Font =  _daddy.Font;
                changer.Icon =  _daddy.Icon;
                if (changer.ShowDialog() == DialogResult.OK)
                {
                	TrackSelector.ColorType colorType = (TrackSelector.ColorType)(changer.Parameters[0].ValueIndex);
                    
	                _daddy._ThreadProgressBarTitle =  _daddy.GetTranslator().GetString("LblOperationInProgress");
	                _daddy.CreateThreadProgressBar();
	
	                // On va parcourir tous les fichiers et créer les zone.
	                // C'est du lourd, du très lourd
	                string[] filePaths = Directory.GetFiles(kmlpath, "*.kml", SearchOption.AllDirectories);
	                 
	                // On récupère les caches affichées
	                List<Geocache> caches =  _daddy.GetDisplayedCaches();
	                 
	                // Les zones
	                List<Tuple<Int32, String, List<GMapPolygon>>> allZones = new List<Tuple<Int32, String, List<GMapPolygon>>>();
	                _daddy._cacheDetail._gmap.HoldInvalidation = true;
	            	foreach (string filename in filePaths)
	            	{  
	            		XmlDocument _xmldoc = new XmlDocument();
	            		
	            		
            			// On charge le pays
    	                _xmldoc.Load(filename);
    
						// Le nombre maximum de caches trouvées dans ce pays	    	                
		               	int max = 0;
		               	
		               	// Hop, on va lire le pays
		               	// On se trouve au niveau d'une zone que l'on veut allumer
                		// On va les stocker avant de les dessiner
                    	List<GMapPolygon> areas = new List<GMapPolygon>();
                		int nbInMetaZone = 0;
                		XmlNamespaceManager ns = new XmlNamespaceManager(_xmldoc.NameTable);
						ns.AddNamespace("ns1", "http://earth.google.com/kml/2.2");
						var nodes = _xmldoc.SelectNodes("/ns1:kml/ns1:Document/ns1:Placemark", ns);
                    	foreach (XmlNode placemarknode in nodes)
                		{
                    		XmlNode _xmlnode = placemarknode["MultiGeometry"];
                    		String s = "";
                    
                    		// On parcourt tous les polygones de MultiGeometry
		                    foreach (XmlNode elt in _xmlnode.ChildNodes)
		                    {
		                    	// On est dans Polygon
		                        // On va lire les coordonnées
		                        // Polygon -> outerBoundaryIs -> LinearRing -> coordinates
		                        XmlNode coords = elt["outerBoundaryIs"];
			                	if (coords == null)
			                		continue;
			                	coords = coords["LinearRing"];
			                	coords = coords["coordinates"];
			                	s = coords.InnerText;
			                	s = s.Replace("\r\n","\n");
			                	s = s.Replace("\n","#");
			                	s = s.Replace("\t","");
			                	s = s.Replace(" ","#");
			                	String[] latlng = s.Split('#'); // lng, lat, alt?
		                        List<PointDouble> pts = new List<PointDouble>();
		                        List<PointLatLng> pts2 = new List<PointLatLng>();
		
		                        // On construit le polygone
		                        foreach (String ll in latlng)
		                        {
		                            String[] lalo = ll.Split(',');
		                            if (lalo.Length >= 2)
		                            {
		                            	double lat, lng;
		                            	lat = MyTools.ConvertToDoubleFast(lalo[1]);
	                            		lng = MyTools.ConvertToDoubleFast(lalo[0]);
		                            			                            	
		                                pts.Add(new PointDouble(lng, lat));
		                                pts2.Add(new PointLatLng(lat, lng));
		                            }
		                        }
		
		                        // On affiche chaque zone
		                        for(int i=0;i<caches.Count; i++)
		                        {
		                            Geocache geo = caches[i];
		                            PointDouble ptcache = new PointDouble(geo._dLongitude, geo._dLatitude);
		                            if (MyTools.PointInPolygon(ptcache, pts.ToArray()))
		                            {
		                            	nbInMetaZone++;
		                            }
		                        }
		                        
		                        // On crée la zone
		                        GMapPolygon area = new GMapPolygon(pts2, "mazone");
		                        // On ajoute pour dessiner plus tard
		                        areas.Add(area);
		                    }
                		}
                    
                    	// On ajoute ces zones au machin final
                    	if (nbInMetaZone > max)
	                    	max = nbInMetaZone;
                    	allZones.Add(new Tuple<Int32, String, List<GMapPolygon>>(nbInMetaZone, filename, areas));
                    	
                    	// Nettoyage
                    	_xmldoc.RemoveAll();
	                }
	                
	                // On trace les polygones
	                DisplayAreas(allZones, colorType);
                    
	                _daddy._cacheDetail._gmap.Refresh();
	                _daddy.KillThreadProgressBar();
	                
	                // On affiche proprement 
	                _daddy.ShowCacheMapInCacheDetail();
	                _daddy._cacheDetail._gmap.Zoom = 2.5;
	            	_daddy._cacheDetail.Location = new Point(0,0);
	        		_daddy._cacheDetail.Size = new Size(1200,1200);
	        		_daddy._cacheDetail._gmap.Position = new PointLatLng(0, 0);
	                
	                return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _daddy.KillThreadProgressBar();
                _daddy.ShowException("", "An error in DisplayWorldCoverageHighRes", ex);
                return false;
            }
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private bool DisplayWorldCoverageLowRes()
		{
			try
            {
               
            	_daddy.ClearOverlay_RESERVED2();
                
                // 1 : Régions
                // 2 : Départements
                // 3 : Villes
                String filename = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "KML" + Path.DirectorySeparatorChar + "World_ISO2.kml";
                if (File.Exists(filename) == false)
            	{
            		// Message d'erreur, on invite à télécharger la base
            		if (_downloadinprogress)
            			return false;
	                if (_daddy.GetInternetStatus())
	                {
	                    DialogResult dialogResult = MyMessageBox.Show(_daddy.GetTranslator().GetStringM("AskDownloadKMLNet"),
	                                      _daddy.GetTranslator().GetString("AskDownloadKMLTitle"),
	                                      MessageBoxIcon.Question, _daddy.GetTranslator());
	                    if (dialogResult == DialogResult.Yes)
	                    {
	                        // Ok on télécharge
	                        DownloadDB();
	                    }
	                }
	                else
	                {
	                    _daddy.MsgActionError(_daddy, _daddy.GetTranslator().GetStringM("AskDownloadKMLNoNet"));
	                }
                    return false;
            	}
	               
                //Un petit filtre pour choisir le gradient
                List<ParameterObject> lst = new List<ParameterObject>();               
                List<String> lstcompare = new List<string>();
            	lstcompare.Add(_daddy.GetTranslator().GetString("LblSpeedGreenRed"));
	            lstcompare.Add(_daddy.GetTranslator().GetString("LblSpeedHSL"));
	            lstcompare.Add(_daddy.GetTranslator().GetString("LblSpeedUnicolor"));
	            ParameterObject po = new ParameterObject(ParameterObject.ParameterType.List, lstcompare, "speedcolor", _daddy.GetTranslator().GetString("LblColorType"));
	            po.DefaultListValue = _daddy.GetTranslator().GetString("LblSpeedUnicolor");
	            lst.Add(po);
                ParametersChanger changer = new ParametersChanger();
                changer.Title = _daddy.GetTranslator().GetString("displayFranceCoverageToolStripMenuItem");
                changer.BtnCancel = _daddy.GetTranslator().GetString("BtnCancel");
                changer.BtnOK = _daddy.GetTranslator().GetString("BtnOk");
                changer.ErrorFormater = _daddy.GetTranslator().GetString("ErrWrongParameter");
                changer.ErrorTitle = _daddy.GetTranslator().GetString("Error");
                changer.Parameters = lst;
                changer.Font =  _daddy.Font;
                changer.Icon =  _daddy.Icon;
                if (changer.ShowDialog() == DialogResult.OK)
                {
                	TrackSelector.ColorType colorType = (TrackSelector.ColorType)(changer.Parameters[0].ValueIndex);
                    
	                 _daddy._ThreadProgressBarTitle =  _daddy.GetTranslator().GetString("LblOperationInProgress");
	                 _daddy.CreateThreadProgressBar();
	
	                XmlDocument _xmldoc;
	                
	                List<Geocache> caches =  _daddy.GetDisplayedCaches();
	                _xmldoc = new XmlDocument();
	                _xmldoc.Load(filename);
	                
	                
	               	List<Tuple<Int32, String, List<GMapPolygon>>> allZones = new List<Tuple<Int32, String, List<GMapPolygon>>>();
	               	int max = 0;
	               	
	               	XmlNamespaceManager ns = new XmlNamespaceManager(_xmldoc.NameTable);
	            	ns.AddNamespace("ns1", "http://www.opengis.net/kml/2.2");
					var nodes = _xmldoc.SelectNodes("/ns1:kml/ns1:Document/ns1:Placemark", ns);
	               	foreach (XmlNode placemarknode in nodes)
	                {
	                	String name = placemarknode.ChildNodes[0].InnerText;
	                	
	                	// On se trouve au niveau d'une zone que l'on veut allumer
	                	// elle est potentiellement composée de plusieurs morceaux, dès que 1 est bon, on allume tout
	                	// On va les stocker avant de les dessiner
	                    List<GMapPolygon> areas = new List<GMapPolygon>();
	                    
	                    XmlNode _xmlnode = placemarknode["MultiGeometry"];
	                    String s = "";
	                     _daddy._cacheDetail._gmap.HoldInvalidation = true;
	                    
	                    // On parcourt tous les polygones de MultiGeometry
	                    int nbInMetaZone = 0;
	                    foreach (XmlNode elt in _xmlnode.ChildNodes)
	                    {
	                    	// On est dans Polygon
	                        // On va lire les coordonnées
	                        // Polygon -> outerBoundaryIs -> LinearRing -> coordinates
	                        XmlNode coords = elt["outerBoundaryIs"];
		                	if (coords == null)
		                		continue;
		                	coords = coords["LinearRing"];
		                	coords = coords["coordinates"];
		                	s = coords.InnerText;
		                	s = s.Replace("\r\n","\n");
		                	s = s.Replace("\n","#");
		                	s = s.Replace("\t","");
		                	s = s.Replace(" ","#");
		                	String[] latlng = s.Split('#'); // lng, lat, alt?
	                        List<PointDouble> pts = new List<PointDouble>();
	                        List<PointLatLng> pts2 = new List<PointLatLng>();
	
	                        // On construit le polygone
	                        foreach (String ll in latlng)
	                        {
	                            String[] lalo = ll.Split(',');
	                            if (lalo.Length >= 2)
	                            {
	                            	double lat, lng;
	                            	lat = MyTools.ConvertToDoubleFast(lalo[1]);
	                            	lng = MyTools.ConvertToDoubleFast(lalo[0]);
	                            	
	                                pts.Add(new PointDouble(lng, lat));
	                                pts2.Add(new PointLatLng(lat, lng));
	                            }
	                        }
	
	                        // On affiche chaque zone
	                        for(int i=0;i<caches.Count; i++)
	                        {
	                            Geocache geo = caches[i];
	                            PointDouble ptcache = new PointDouble(geo._dLongitude, geo._dLatitude);
	                            if (MyTools.PointInPolygon(ptcache, pts.ToArray()))
	                            {
	                            	nbInMetaZone++;
	                            }
	                        }
	                        
	                        // On crée la zone
	                        GMapPolygon area = new GMapPolygon(pts2, "mazone");
	                        // On ajoute pour dessiner plus tard
	                        areas.Add(area);
	                    }
	                    
	                    // On ajoute ces zones au machin final
	                    if (nbInMetaZone > max)
	                    	max = nbInMetaZone;
	                    allZones.Add(new Tuple<Int32, String, List<GMapPolygon>>(nbInMetaZone, name, areas));
	                }
	                
	                // On trace les polygones
	                DisplayAreas(allZones, colorType);
                    
	                _daddy._cacheDetail._gmap.Refresh();
	                _daddy.KillThreadProgressBar();
	                
	                // On affiche proprement 
	                _daddy.ShowCacheMapInCacheDetail();
	                _daddy._cacheDetail._gmap.Zoom = 2.5;
	            	_daddy._cacheDetail.Location = new Point(0,0);
	        		_daddy._cacheDetail.Size = new Size(1200,1200);
	        		_daddy._cacheDetail._gmap.Position = new PointLatLng(0, 0);
	                
	                return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _daddy.KillThreadProgressBar();
                _daddy.ShowException("", "An error in DisplayWorldCoverageLowRes", ex);
                return false;
            }
		}
		
		private void DisplayAreas(List<Tuple<Int32, String, List<GMapPolygon>>> allZones, TrackSelector.ColorType colorType)
		{
			// On trace les polygones
            // Le pourcentage pour la définition des couleurs est moyen car si on a de trop grandes différences, on 
            // tous les faibles vont être identiques (ex. 0 - 100 caches versus 2000 caches)
            // Il vaut mieux calculer son rang dans la liste des non nuls et calculer son pourcentage comme cela
            List<int> valNonNulles = new List<int>();
            foreach(var tple in allZones)
            {
            	if (!valNonNulles.Contains(tple.Item1))
            		valNonNulles.Add(tple.Item1);
            }
            // On tri cette liste par ordre ascendant
            valNonNulles.Sort();
            
            foreach(var tple in allZones)
            {
            	int nb = tple.Item1;
            	//double percent = (max == 0)?0.0:(double)nb/(double)max;
            	double percent = 0.0;
            	int pos = valNonNulles.IndexOf(nb);
            	if (pos != -1)
            	{
            		percent = (double)(1+pos)/(double)valNonNulles.Count;
            	}
            	
                foreach(GMapPolygon area in tple.Item3)
                {
                	// On dessine
                	area.Tag = null;
                	if (nb != 0)
		            {
		            	if (colorType == TrackSelector.ColorType.SingleColor)
		            	{
		                	area.Fill = new SolidBrush(Color.FromArgb(100, 0, 255, 0));
		            	}
		            	else if (colorType == TrackSelector.ColorType.GreenRed)
		            	{
		            		area.Fill = new SolidBrush(Color.FromArgb(100, 0, (int)(100.0*percent + 155.5), 0));
		            	}
		            	else if (colorType == TrackSelector.ColorType.HSL)
		            	{
		            		ColorRGB c = ColorRGB.HSL2RGB(percent, 0.5, 0.5);
		            		area.Fill = new SolidBrush(Color.FromArgb(100, c.R, c.G, c.B));
		            	}
		                area.Stroke = new Pen(Color.Green);
		            }
		            else
		            {
		                area.Fill = new SolidBrush(Color.FromArgb(100, 255, 0, 0));
		                area.Stroke = new Pen(Color.Red);
		            }
                    _daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2].Polygons.Add(area);
                }
            }
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="allZones"></param>
		/// <param name="colorType"></param>
		/// <param name="clearOverlay"></param>
		/// <param name="createthreadwait"></param>
		/// <returns></returns>
		public bool DisplayFranceCoverageImpl(int type, out List<Tuple<Int32, String, List<GMapPolygon>>> allZones, TrackSelector.ColorType colorType = TrackSelector.ColorType.GreenRed, bool clearOverlay = true, bool createthreadwait = true)
		{
			allZones = new List<Tuple<Int32, String, List<GMapPolygon>>>();
			//if (!SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled()) return false;
			
			try
            {
				if (clearOverlay)
            		_daddy.ClearOverlay_RESERVED2();
                
                // 1 : Régions
                // 2 : Départements
                // 3 : Villes
                String filename = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "KML" + Path.DirectorySeparatorChar + "FRA_adm" + type.ToString() + ".kml";
                if (File.Exists(filename) == false)
            	{
            		// Message d'erreur, on invite à télécharger la base
            		if (_downloadinprogress)
            			return false;
	                if (_daddy.GetInternetStatus())
	                {
	                    DialogResult dialogResult = MyMessageBox.Show(_daddy.GetTranslator().GetStringM("AskDownloadKMLNet"),
	                                      _daddy.GetTranslator().GetString("AskDownloadKMLTitle"),
	                                      MessageBoxIcon.Question, _daddy.GetTranslator());
	                    if (dialogResult == DialogResult.Yes)
	                    {
	                        // Ok on télécharge
	                        DownloadDB();
	                    }
	                }
	                else
	                {
	                    _daddy.MsgActionError(_daddy, _daddy.GetTranslator().GetStringM("AskDownloadKMLNoNet"));
	                }
                    return false;
            	}
               
                 _daddy._ThreadProgressBarTitle =  _daddy.GetTranslator().GetString("LblOperationInProgress");
                 if (createthreadwait)
                 	_daddy.CreateThreadProgressBar();

                XmlDocument _xmldoc;
                
                List<Geocache> caches =  _daddy.GetDisplayedCaches();
                _xmldoc = new XmlDocument();
                _xmldoc.Load(filename);
                int max = 0;
                
                XmlNamespaceManager ns = new XmlNamespaceManager(_xmldoc.NameTable);
            	ns.AddNamespace("ns1", "http://earth.google.com/kml/2.2");
				var nodes = _xmldoc.SelectNodes("/ns1:kml/ns1:Document/ns1:Placemark", ns);
				
                foreach (XmlNode placemarknode in nodes)
                {
                	// On se trouve au niveau d'une zone que l'on veut allumer
                	// elle est potentiellement composée de plusieurs morceaux, dès que 1 est bon, on allume tout
                	// On va les stocker avant de les dessiner
                    List<GMapPolygon> areas = new List<GMapPolygon>();
                    
                    String textname = placemarknode["name"].InnerText;
                    
                    XmlNode _xmlnode = placemarknode["MultiGeometry"];
                    String s = "";
                     _daddy._cacheDetail._gmap.HoldInvalidation = true;
                    
                    // On parcourt tous les polygones de MultiGeometry
                    int nbInMetaZone = 0;
                    foreach (XmlNode elt in _xmlnode.ChildNodes)
                    {
                        // On est dans Polygon
                        // On va lire les coordonnées
                        // Polygon -> outerBoundaryIs -> LinearRing -> coordinates
                        XmlNode coords = elt["outerBoundaryIs"]["LinearRing"]["coordinates"];
                        s = coords.InnerText;
                        s = s.Replace(" ", "");
                        s = s.Replace("\r\n", "\n");
                        String[] latlng = s.Split('\n');
                        List<PointDouble> pts = new List<PointDouble>();
                        List<PointLatLng> pts2 = new List<PointLatLng>();

                        // On construit le polygone
                        foreach (String ll in latlng)
                        {
                            String[] lalo = ll.Split(',');
                            if (lalo.Length == 2)
                            {
                            	double lat, lng;
                            	lat = MyTools.ConvertToDoubleFast(lalo[1]);
	                            lng = MyTools.ConvertToDoubleFast(lalo[0]);
                            	
                                pts.Add(new PointDouble(lng, lat));
                                pts2.Add(new PointLatLng(lat, lng));
                            }
                        }

                        // On affiche chaque zone
                        for(int i=0;i<caches.Count; i++)
                        {
                            Geocache geo = caches[i];
                            PointDouble ptcache = new PointDouble(geo._dLongitude, geo._dLatitude);
                            if (MyTools.PointInPolygon(ptcache, pts.ToArray()))
                                nbInMetaZone++;
                        }
                        
                        // On crée la zone
                        GMapPolygon area = new GMapPolygon(pts2, "mazone");
                        // On ajoute pour dessiner plus tard
                        areas.Add(area);
                    }
                    
                    // On ajoute ces zones au machin final
                    if (nbInMetaZone > max)
                    	max = nbInMetaZone;
                    allZones.Add(new Tuple<Int32, String, List<GMapPolygon>>(nbInMetaZone, textname, areas));
                }
                
                // On trace les polygones
                DisplayAreas(allZones, colorType);
                
                _daddy._cacheDetail._gmap.Refresh();
                if (createthreadwait)
                	_daddy.KillThreadProgressBar();
                
                // On affiche proprement
                _daddy.ShowCacheMapInCacheDetail();  
                _daddy._cacheDetail._gmap.Zoom = 6.0;
            	_daddy._cacheDetail.Location = new Point(0,0);
        		_daddy._cacheDetail.Size = new Size(1200,1200);
        		_daddy._cacheDetail._gmap.Position = new PointLatLng(46.5437496027386, 2.9443359375);	                
        		
                return true;
            }
            catch (Exception ex)
            {
                if (createthreadwait)
                	_daddy.KillThreadProgressBar();
                _daddy.ShowException("", "An error in DisplayFranceCoverage", ex);
                return false;
            }
		}
		
		private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            if ((!e.Cancelled) && (e.Error == null))
            {
                // On dézippe le fichier
                String unpackDirectory = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "KML" + Path.DirectorySeparatorChar;
                String localfile = unpackDirectory + "FRA_adm.zip";
                try
	            {
                    if (File.Exists(unpackDirectory + "FRA_adm1.kml"))
                        File.Delete(unpackDirectory + "FRA_adm1.kml");
                    if (File.Exists(unpackDirectory + "FRA_adm2.kml"))
                        File.Delete(unpackDirectory + "FRA_adm2.kml");
                    if (File.Exists(unpackDirectory + "FRA_adm3.kml"))
                        File.Delete(unpackDirectory + "FRA_adm3.kml");
                    if (File.Exists(unpackDirectory + "FRA_adm4.kml"))
                        File.Delete(unpackDirectory + "FRA_adm4.kml");
                    if (File.Exists(unpackDirectory + "FRA_adm5.kml"))
                        File.Delete(unpackDirectory + "FRA_adm5.kml");
                    if (File.Exists(unpackDirectory + "World_ISO2.kml"))
                        File.Delete(unpackDirectory + "World_ISO2.kml");
                    
                    ZipFile.ExtractToDirectory(localfile, unpackDirectory);
	            }
	            catch (Exception ex)
	            {
	            	_downloadinprogress = false;
	            	_daddy.MsgActionError(_daddy, _daddy.GetTranslator().GetStringM("LblErrUnzipingKML") + "\r\n" + ex.Message);
                    File.Delete(localfile);
                    return;
	            }
	            
                // Delete zip file
                File.Delete(localfile);

                // All right
                _downloadinprogress = false;
                _daddy.MsgActionOk(_daddy, _daddy.GetTranslator().GetStringM("LblDatabaseOK"));
            }
            else
            {
            	_downloadinprogress = false;
                _daddy.MsgActionError(_daddy, _daddy.GetTranslator().GetStringM("LblErrDownloadingKML"));
            }
        }
		
		private void DownloadDBImpl(String url)
        {
            // on télécharge la base
            _downloadinprogress = true;
            try
            {
	            WebClient client = new WebClient();
	            client.Proxy = _daddy.GetProxy();
	            client.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
	            String kmlpath = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "KML";
	            if (Directory.Exists(kmlpath) == false)
	            	Directory.CreateDirectory(kmlpath);
	            
	            String localfile = kmlpath + Path.DirectorySeparatorChar + "FRA_adm.zip";
	            if (File.Exists(localfile))
	                File.Delete(localfile);
	            client.DownloadFileAsync(new Uri(url + "/db2/FRA_adm.zip"), localfile);
            }
            catch(Exception)
            {
            	_downloadinprogress = false;
            	throw;
            }
        }
		
		private void DownloadDB()
        {
            // Async mode
            String urlupdate = ConfigurationManager.AppSettings["urlupdate"];
            // Il se peut qu'on ait 2 URL (le site principal et le site de backup)
            String[] urls = urlupdate.Split(';');
            foreach(String u in urls)
            {
                // On ne ping que la première URL qui est le site officiel
                String url = u + "/db2/CacheCache.id";
                try
                {
                    // On teste le téléchargement de CacheCache.id pour s'assurer que le serveur est vivant
                    WebProxy proxy = _daddy.GetProxy();
                    MyTools.GetRequest(new Uri(url), proxy, 200);
                    
                    DownloadDBImpl(u);
                    return;
                }
                catch(Exception)
                {

                }
            }

            // Si on est là, on a tout merdé
            _daddy.MsgActionError(_daddy, _daddy.GetTranslator().GetStringM("LblErrNoKMLAvailableOnline"));
        }
		
		private void FilterOnFrenchAreaImpl(int type, String keylabel)
		{
            try
            {
            	// 1 : Régions
            	// 2 : Départements
            	// 3 : Villes
            	
            	String filename = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "KML" + Path.DirectorySeparatorChar + "FRA_adm" + type.ToString() + ".kml";
            	if (File.Exists(filename) == false)
            	{
            		// Message d'erreur, on invite à télécharger la base
            		if (_downloadinprogress)
            			return;
	                if (_daddy.GetInternetStatus())
	                {
	                    DialogResult dialogResult = MyMessageBox.Show(_daddy.GetTranslator().GetStringM("AskDownloadKMLNet"),
	                                      _daddy.GetTranslator().GetString("AskDownloadKMLTitle"),
	                                      MessageBoxIcon.Question, _daddy.GetTranslator());
	                    if (dialogResult == DialogResult.Yes)
	                    {
	                        // Ok on télécharge
	                        DownloadDB();
	                    }
	                }
	                else
	                {
	                    _daddy.MsgActionError(_daddy, _daddy.GetTranslator().GetStringM("AskDownloadKMLNoNet"));
	                }
                    return;
            	}
            	
            	_daddy._ThreadProgressBarTitle = _daddy.GetTranslator().GetString("LblOperationInProgress");
                _daddy.CreateThreadProgressBar();
                
            	XmlDocument _xmldoc;
                Dictionary<String, List<List<PointLatLng>>> zones = new Dictionary<string, List<List<PointLatLng>>>();
                
                _xmldoc = new XmlDocument();
                _xmldoc.Load(filename);
                
                XmlNamespaceManager ns = new XmlNamespaceManager(_xmldoc.NameTable);
            	ns.AddNamespace("ns1", "http://earth.google.com/kml/2.2");
				var nodes = _xmldoc.SelectNodes("/ns1:kml/ns1:Document/ns1:Placemark", ns);
				
                foreach(XmlNode placemarknode in nodes)
                {
                	String name = placemarknode["name"].InnerText;
                	if (type >= 4) // A partir du niveau 4 il peut y avoir des doublons...
                		name += " " + placemarknode["styleUrl"].InnerText;
                	XmlNode _xmlnode = placemarknode["MultiGeometry"];
	                String s = "";
	                _daddy._cacheDetail._gmap.HoldInvalidation = true;
	                int i = 0;
	                // On parcourt tous les polygones de MultiGeometry
	                List<List<PointLatLng>> zones_of_name = new List<List<PointLatLng>>();
	                foreach (XmlNode elt in _xmlnode.ChildNodes)
	                {
	                	// On est dans Polygon
	                	// On va lire les coordonnées
	                	// Polygon -> outerBoundaryIs -> LinearRing -> coordinates
	                	XmlNode coords = elt["outerBoundaryIs"]["LinearRing"]["coordinates"];
	                	s = coords.InnerText;
	                	s = s.Replace(" ","");
	                	s = s.Replace("\r\n","\n");
	                	String[] latlng = s.Split('\n');
	                	List<PointLatLng> pts = new List<PointLatLng>();
	                	
	                	// On construit le polygone
	                	foreach(String ll in latlng)
	                	{
	                		String[] lalo = ll.Split(',');
	                		if (lalo.Length == 2)
	                		{
	                			double lat, lng;
                            	lat = MyTools.ConvertToDoubleFast(lalo[1]);
	                            lng = MyTools.ConvertToDoubleFast(lalo[0]);
                            	
                                pts.Add(new PointLatLng(lat, lng));
	                		}
	                	}
	                	
	                	// On l'ajoute
	                	zones_of_name.Add(pts);
	                	
			           i++;
	                }
	                
	                zones.Add(name, zones_of_name);
                }
                _daddy._cacheDetail._gmap.Refresh();
                
                //Un petit filtre pour choisir le département
                List<ParameterObject> lst = new List<ParameterObject>();
                List<String> lstypes = new List<string>();
                foreach(String key in zones.Keys)
                	lstypes.Add(key);
                lstypes.Sort();
                lst.Add(new ParameterObject(ParameterObject.ParameterType.List, lstypes, "choix", _daddy.GetTranslator().GetString(keylabel)));

                ParametersChanger changer = new ParametersChanger();
                changer.Title = _daddy.GetTranslator().GetString(keylabel);
                changer.BtnCancel = _daddy.GetTranslator().GetString("BtnCancel");
                changer.BtnOK = _daddy.GetTranslator().GetString("BtnOk");
                changer.ErrorFormater = _daddy.GetTranslator().GetString("ErrWrongParameter");
                changer.ErrorTitle = _daddy.GetTranslator().GetString("Error");
                changer.Parameters = lst;
                changer.Font = _daddy.Font;
                changer.Icon = _daddy.Icon;
                
                _daddy.KillThreadProgressBar();
                
                if (changer.ShowDialog() == DialogResult.OK)
                {
                    String dpt = lst[0].Value;
                    CustomFilterArea fltr = new CustomFilterArea(zones[dpt]);
                    _daddy.ExecuteCustomFilter(fltr);
                    
                    _daddy.ClearOverlay_RESERVED2();
                    int i = 0;
                    foreach(List<PointLatLng> anarea in zones[dpt])
		        	{
			            if (anarea.Count >= 3)
			            {
			            	GMapPolygon area = new GMapPolygon(anarea, dpt + i.ToString());
		                	area.Tag = null;
		                	area.Fill = new SolidBrush(Color.Transparent);
		                	_daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2].Polygons.Add(area);
		                	i++;
			            }
		        	}
                    _daddy.ShowCacheMapInCacheDetail();
                    _daddy._cacheDetail._gmap.ZoomAndCenterPolygons(_daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2].Id);
                }
                
            }
            catch (Exception ex)
            {
                _daddy.KillThreadProgressBar();
                _daddy.ShowException("", "An error in FilterOnFrenchAreaImpl", ex);
            }
		}
		
		/// <summary>
		/// 
		/// </summary>
		private void FilterOnCountryImplLowRes()
		{
            try
            {
            	String keylabel = "LblCountrySelection";
            	String filename = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "KML" + Path.DirectorySeparatorChar + "World_ISO2.kml";
            	if (File.Exists(filename) == false)
            	{
            		// Message d'erreur, on invite à télécharger la base
            		if (_downloadinprogress)
            			return;
	                if (_daddy.GetInternetStatus())
	                {
	                    DialogResult dialogResult = MyMessageBox.Show(_daddy.GetTranslator().GetStringM("AskDownloadKMLNet"),
	                                      _daddy.GetTranslator().GetString("AskDownloadKMLTitle"),
	                                      MessageBoxIcon.Question, _daddy.GetTranslator());
	                    if (dialogResult == DialogResult.Yes)
	                    {
	                        // Ok on télécharge
	                        DownloadDB();
	                    }
	                }
	                else
	                {
	                    _daddy.MsgActionError(_daddy, _daddy.GetTranslator().GetStringM("AskDownloadKMLNoNet"));
	                }
                    return;
            	}
            	
            	_daddy._ThreadProgressBarTitle = _daddy.GetTranslator().GetString("LblOperationInProgress");
                _daddy.CreateThreadProgressBar();
                
            	XmlDocument _xmldoc;
                Dictionary<String, List<List<PointLatLng>>> zones = new Dictionary<string, List<List<PointLatLng>>>();
                
                _xmldoc = new XmlDocument();
                _xmldoc.Load(filename);
                XmlNamespaceManager ns = new XmlNamespaceManager(_xmldoc.NameTable);
            	ns.AddNamespace("ns1", "http://www.opengis.net/kml/2.2");
				var nodes = _xmldoc.SelectNodes("/ns1:kml/ns1:Document/ns1:Placemark", ns);
				
                foreach(XmlNode placemarknode in nodes)
                {
                	XmlNode childname = placemarknode.ChildNodes[0];
                	String name = childname.InnerText;
                	XmlNode _xmlnode = placemarknode["MultiGeometry"];
	                String s = "";
	                int i = 0;
	                // On parcourt tous les polygones de MultiGeometry
	                List<List<PointLatLng>> zones_of_name = new List<List<PointLatLng>>();
	                foreach (XmlNode elt in _xmlnode.ChildNodes)
	                {
	                	// On est dans Polygon
	                	// On va lire les coordonnées
	                	// Polygon -> outerBoundaryIs -> LinearRing -> coordinates
	                	XmlNode coords = elt["outerBoundaryIs"];
	                	if (coords == null)
	                		continue;
	                	coords = coords["LinearRing"];
	                	coords = coords["coordinates"];
	                	s = coords.InnerText;
	                	s = s.Replace("\r\n","\n");
	                	s = s.Replace("\n","#");
	                	s = s.Replace("\t","");
	                	s = s.Replace(" ","#");
	                	String[] latlng = s.Split('#'); // lng, lat, alt?
	                	List<PointLatLng> pts = new List<PointLatLng>();
	                	
	                	// On construit le polygone
	                	foreach(String ll in latlng)
	                	{
	                		String[] lalo = ll.Split(',');
	                		if (lalo.Length >= 2)
	                		{
	                			double lat, lng;
                            	lat = MyTools.ConvertToDoubleFast(lalo[1]);
	                            lng = MyTools.ConvertToDoubleFast(lalo[0]);
                            	
	                            pts.Add(new PointLatLng(lat, lng));
	                		}
	                	}
	                	
	                	// On l'ajoute
	                	zones_of_name.Add(pts);
	                	
			           i++;
	                }
	                
	                zones.Add(name, zones_of_name);
                }
                
                //Un petit filtre pour choisir le pays
                List<ParameterObject> lst = new List<ParameterObject>();
                List<String> lstypes = new List<string>();
                foreach(String key in zones.Keys)
                	lstypes.Add(key);
                lstypes.Sort();
                lst.Add(new ParameterObject(ParameterObject.ParameterType.List, lstypes, "choix", _daddy.GetTranslator().GetString(keylabel)));

                ParametersChanger changer = new ParametersChanger();
                changer.Title = _daddy.GetTranslator().GetString(keylabel);
                changer.BtnCancel = _daddy.GetTranslator().GetString("BtnCancel");
                changer.BtnOK = _daddy.GetTranslator().GetString("BtnOk");
                changer.ErrorFormater = _daddy.GetTranslator().GetString("ErrWrongParameter");
                changer.ErrorTitle = _daddy.GetTranslator().GetString("Error");
                changer.Parameters = lst;
                changer.Font = _daddy.Font;
                changer.Icon = _daddy.Icon;
                
                _daddy.KillThreadProgressBar();
                
                if (changer.ShowDialog() == DialogResult.OK)
                {
                    String dpt = lst[0].Value;
                    CustomFilterArea fltr = new CustomFilterArea(zones[dpt]);
                    _daddy.ExecuteCustomFilter(fltr);
                    
                    _daddy.ClearOverlay_RESERVED2();
                    int i = 0;
                    
                    Double LatMin = Double.MaxValue;
		            Double LatMax = Double.MinValue;
		            Double LonMin = Double.MaxValue;
		            Double LonMax = Double.MinValue;
		           
                    foreach(List<PointLatLng> anarea in zones[dpt])
		        	{
			            if (anarea.Count >= 3)
			            {
			            	foreach(var pt in anarea)
			            	{
			            		if (pt.Lat < LatMin)
				                    LatMin = pt.Lat;
				                if (pt.Lat > LatMax)
				                    LatMax = pt.Lat;
				                if (pt.Lng < LonMin)
				                    LonMin = pt.Lng;
				                if (pt.Lng > LonMax)
				                    LonMax = pt.Lng;
			            	}
			            	GMapPolygon area = new GMapPolygon(anarea, dpt + i.ToString());
		                	area.Tag = null;
		                	area.Fill = new SolidBrush(Color.Transparent);
		                	_daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2].Polygons.Add(area);
		                	i++;

			            }
		        	}
                    _daddy.ShowCacheMapInCacheDetail();
                    _daddy._cacheDetail._gmap.ZoomAndCenterPolygons(_daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2].Id);
                }
                
            }
            catch (Exception ex)
            {
                _daddy.KillThreadProgressBar();
                _daddy.ShowException("", "An error in FilterOnCountryImplLowRes", ex);
            }
		}
	
		/// <summary>
		/// 
		/// </summary>
		private void FilterOnCountryImplHighRes()
		{
            try
            {
            	String keylabel = "LblCountrySelection";
            	// Si on est là, le fichier est censé exister !
            	String kmlpath = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "KML" + Path.DirectorySeparatorChar + "World"  + Path.DirectorySeparatorChar;
            	String index = kmlpath + "_index.dat";
			
            	// On va lire toutes les entrées du fichier index
            	List<String[]> _pays = new List<String[]>();
            	List<String> lstypes = new List<string>();
                using (StreamReader r = new StreamReader(index, Encoding.GetEncoding("ISO-8859-1")))
                {
                    // Pays;fichier;nombre de zones
                    // Tajikistan;TJK_adm0.kml;3 
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                    	String[] vals = line.Split(';');
                    	_pays.Add(vals);
                    	lstypes.Add(vals[0]);
                    }
                }
                
            	//Un petit filtre pour choisir le pays
                List<ParameterObject> lst = new List<ParameterObject>();
                lstypes.Sort();
                lst.Add(new ParameterObject(ParameterObject.ParameterType.List, lstypes, "choix", _daddy.GetTranslator().GetString(keylabel)));

                ParametersChanger changer = new ParametersChanger();
                changer.Title = _daddy.GetTranslator().GetString(keylabel);
                changer.BtnCancel = _daddy.GetTranslator().GetString("BtnCancel");
                changer.BtnOK = _daddy.GetTranslator().GetString("BtnOk");
                changer.ErrorFormater = _daddy.GetTranslator().GetString("ErrWrongParameter");
                changer.ErrorTitle = _daddy.GetTranslator().GetString("Error");
                changer.Parameters = lst;
                changer.Font = _daddy.Font;
                changer.Icon = _daddy.Icon;
                
                
                
                if (changer.ShowDialog() == DialogResult.OK)
                {
                    String pays = lst[0].Value;
                    String fichier_pays = "";
                    foreach(String[] vals in _pays)
                    {
                    	if (vals[0] == pays)
                    	{
                    		fichier_pays = kmlpath + vals[1];
                    		break;
                    	}
                    }
                    
                    // On va lire le fichier pays pour en récupérer les zones
                    _daddy._ThreadProgressBarTitle = _daddy.GetTranslator().GetString("LblOperationInProgress");
	                _daddy.CreateThreadProgressBar();
	                
	            	XmlDocument _xmldoc;
	                List<List<PointLatLng>> zones = new List<List<PointLatLng>>();
	                _xmldoc = new XmlDocument();
	                _xmldoc.Load(fichier_pays);
	                XmlNamespaceManager ns = new XmlNamespaceManager(_xmldoc.NameTable);
					ns.AddNamespace("ns1", "http://earth.google.com/kml/2.2");
					var nodes = _xmldoc.SelectNodes("/ns1:kml/ns1:Document/ns1:Placemark", ns);
	                foreach(XmlNode placemarknode in nodes)
	                {
	                	XmlNode childname = placemarknode.ChildNodes[0];
	                	String name = childname.InnerText;
	                	XmlNode _xmlnode = placemarknode["MultiGeometry"];
		                String s = "";
		                // On parcourt tous les polygones de MultiGeometry
		                foreach (XmlNode elt in _xmlnode.ChildNodes)
		                {
		                	// On est dans Polygon
		                	// On va lire les coordonnées
		                	// Polygon -> outerBoundaryIs -> LinearRing -> coordinates
		                	XmlNode coords = elt["outerBoundaryIs"];
		                	if (coords == null)
		                		continue;
		                	coords = coords["LinearRing"];
		                	coords = coords["coordinates"];
		                	s = coords.InnerText;
		                	s = s.Replace("\r\n","\n");
		                	s = s.Replace("\n","#");
		                	s = s.Replace("\t","");
		                	s = s.Replace(" ","#");
		                	String[] latlng = s.Split('#'); // lng, lat, alt?
		                	List<PointLatLng> pts = new List<PointLatLng>();
		                	
		                	// On construit le polygone
		                	foreach(String ll in latlng)
		                	{
		                		String[] lalo = ll.Split(',');
		                		if (lalo.Length >= 2)
		                		{
		                			double lat, lng;
	                            	lat = MyTools.ConvertToDoubleFast(lalo[1]);
	                            	lng = MyTools.ConvertToDoubleFast(lalo[0]);
	                            	
	                                pts.Add(new PointLatLng(lat, lng));
		                		}
		                	}
		                	
		                	// On l'ajoute
		                	zones.Add(pts);
		                }
	                }
                    _daddy.KillThreadProgressBar();
                    
                    CustomFilterArea fltr = new CustomFilterArea(zones);
                    _daddy.ExecuteCustomFilter(fltr);
                    
                    _daddy.ClearOverlay_RESERVED2();
                    int i = 0;
                    foreach(List<PointLatLng> anarea in zones)
		        	{
			            if (anarea.Count >= 3)
			            {
			            	GMapPolygon area = new GMapPolygon(anarea, pays + i.ToString());
		                	area.Tag = null;
		                	area.Fill = new SolidBrush(Color.Transparent);
		                	_daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2].Polygons.Add(area);
		                	i++;
		                	
			            }
		        	}
                    _daddy.ShowCacheMapInCacheDetail();
                    _daddy._cacheDetail._gmap.ZoomAndCenterPolygons(_daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2].Id);
                }
                else
                	return;
            }
            catch (Exception ex)
            {
                _daddy.KillThreadProgressBar();
                _daddy.ShowException("", "An error in FilterOnCountryImplHighRes", ex);
            }
		}
	
		/// <summary>
		/// 
		/// </summary>
		public void FilterOnCountry()
		{
			//if (!SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled()) return;
            	
			// A-t'on une base monde très résolue ?
			String index = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "KML" + Path.DirectorySeparatorChar + "World\\_index.dat";
			if (File.Exists(index))
			{
				FilterOnCountryImplHighRes();
			}
			else
			{
				FilterOnCountryImplLowRes();
			}
		}
		
		private void DownloadWorldKMLs()
        {
            try
            {
            	List<Tuple<String, String>> countries = new List<Tuple<string, string>>();
				countries.Add(new Tuple<String, String>("AFG","Afghanistan"));
				countries.Add(new Tuple<String, String>("XAD","Akrotiri and Dhekelia"));
				countries.Add(new Tuple<String, String>("ALA","Aland"));
				countries.Add(new Tuple<String, String>("ALB","Albania"));
				countries.Add(new Tuple<String, String>("DZA","Algeria"));
				countries.Add(new Tuple<String, String>("ASM","American Samoa"));
				countries.Add(new Tuple<String, String>("AND","Andorra"));
				countries.Add(new Tuple<String, String>("AGO","Angola"));
				countries.Add(new Tuple<String, String>("AIA","Anguilla"));
				countries.Add(new Tuple<String, String>("ATA","Antarctica"));
				countries.Add(new Tuple<String, String>("ATG","Antigua and Barbuda"));
				countries.Add(new Tuple<String, String>("ARG","Argentina"));
				countries.Add(new Tuple<String, String>("ARM","Armenia"));
				countries.Add(new Tuple<String, String>("ABW","Aruba"));
				countries.Add(new Tuple<String, String>("AUS","Australia"));
				countries.Add(new Tuple<String, String>("AUT","Austria"));
				countries.Add(new Tuple<String, String>("AZE","Azerbaijan"));
				countries.Add(new Tuple<String, String>("BHS","Bahamas"));
				countries.Add(new Tuple<String, String>("BHR","Bahrain"));
				countries.Add(new Tuple<String, String>("BGD","Bangladesh"));
				countries.Add(new Tuple<String, String>("BRB","Barbados"));
				countries.Add(new Tuple<String, String>("BLR","Belarus"));
				countries.Add(new Tuple<String, String>("BEL","Belgium"));
				countries.Add(new Tuple<String, String>("BLZ","Belize"));
				countries.Add(new Tuple<String, String>("BEN","Benin"));
				countries.Add(new Tuple<String, String>("BMU","Bermuda"));
				countries.Add(new Tuple<String, String>("BTN","Bhutan"));
				countries.Add(new Tuple<String, String>("BOL","Bolivia"));
				countries.Add(new Tuple<String, String>("BES","Bonaire, Saint Eustatius and Saba"));
				countries.Add(new Tuple<String, String>("BIH","Bosnia and Herzegovina"));
				countries.Add(new Tuple<String, String>("BWA","Botswana"));
				countries.Add(new Tuple<String, String>("BVT","Bouvet Island"));
				countries.Add(new Tuple<String, String>("BRA","Brazil"));
				countries.Add(new Tuple<String, String>("IOT","British Indian Ocean Territory"));
				countries.Add(new Tuple<String, String>("VGB","British Virgin Islands"));
				countries.Add(new Tuple<String, String>("BRN","Brunei"));
				countries.Add(new Tuple<String, String>("BGR","Bulgaria"));
				countries.Add(new Tuple<String, String>("BFA","Burkina Faso"));
				countries.Add(new Tuple<String, String>("BDI","Burundi"));
				countries.Add(new Tuple<String, String>("KHM","Cambodia"));
				countries.Add(new Tuple<String, String>("CMR","Cameroon"));
				countries.Add(new Tuple<String, String>("CAN","Canada"));
				countries.Add(new Tuple<String, String>("CPV","Cape Verde"));
				countries.Add(new Tuple<String, String>("XCA","Caspian Sea"));
				countries.Add(new Tuple<String, String>("CYM","Cayman Islands"));
				countries.Add(new Tuple<String, String>("CAF","Central African Republic"));
				countries.Add(new Tuple<String, String>("TCD","Chad"));
				countries.Add(new Tuple<String, String>("CHL","Chile"));
				countries.Add(new Tuple<String, String>("CHN","China"));
				countries.Add(new Tuple<String, String>("CXR","Christmas Island"));
				countries.Add(new Tuple<String, String>("XCL","Clipperton Island"));
				countries.Add(new Tuple<String, String>("CCK","Cocos Islands"));
				countries.Add(new Tuple<String, String>("COL","Colombia"));
				countries.Add(new Tuple<String, String>("COM","Comoros"));
				countries.Add(new Tuple<String, String>("COK","Cook Islands"));
				countries.Add(new Tuple<String, String>("CRI","Costa Rica"));
				countries.Add(new Tuple<String, String>("CIV","Cote d'Ivoire"));
				countries.Add(new Tuple<String, String>("HRV","Croatia"));
				countries.Add(new Tuple<String, String>("CUB","Cuba"));
				countries.Add(new Tuple<String, String>("CUW","Curacao"));
				countries.Add(new Tuple<String, String>("CYP","Cyprus"));
				countries.Add(new Tuple<String, String>("CZE","Czech Republic"));
				countries.Add(new Tuple<String, String>("COD","Democratic Republic of the Congo"));
				countries.Add(new Tuple<String, String>("DNK","Denmark"));
				countries.Add(new Tuple<String, String>("DJI","Djibouti"));
				countries.Add(new Tuple<String, String>("DMA","Dominica"));
				countries.Add(new Tuple<String, String>("DOM","Dominican Republic"));
				countries.Add(new Tuple<String, String>("ECU","Ecuador"));
				countries.Add(new Tuple<String, String>("EGY","Egypt"));
				countries.Add(new Tuple<String, String>("SLV","El Salvador"));
				countries.Add(new Tuple<String, String>("GNQ","Equatorial Guinea"));
				countries.Add(new Tuple<String, String>("ERI","Eritrea"));
				countries.Add(new Tuple<String, String>("EST","Estonia"));
				countries.Add(new Tuple<String, String>("ETH","Ethiopia"));
				countries.Add(new Tuple<String, String>("FLK","Falkland Islands"));
				countries.Add(new Tuple<String, String>("FRO","Faroe Islands"));
				countries.Add(new Tuple<String, String>("FJI","Fiji"));
				countries.Add(new Tuple<String, String>("FIN","Finland"));
				countries.Add(new Tuple<String, String>("FRA","France"));
				countries.Add(new Tuple<String, String>("GUF","French Guiana"));
				countries.Add(new Tuple<String, String>("PYF","French Polynesia"));
				countries.Add(new Tuple<String, String>("ATF","French Southern Territories"));
				countries.Add(new Tuple<String, String>("GAB","Gabon"));
				countries.Add(new Tuple<String, String>("GMB","Gambia"));
				countries.Add(new Tuple<String, String>("GEO","Georgia"));
				countries.Add(new Tuple<String, String>("DEU","Germany"));
				countries.Add(new Tuple<String, String>("GHA","Ghana"));
				countries.Add(new Tuple<String, String>("GIB","Gibraltar"));
				countries.Add(new Tuple<String, String>("GRC","Greece"));
				countries.Add(new Tuple<String, String>("GRL","Greenland"));
				countries.Add(new Tuple<String, String>("GRD","Grenada"));
				countries.Add(new Tuple<String, String>("GLP","Guadeloupe"));
				countries.Add(new Tuple<String, String>("GUM","Guam"));
				countries.Add(new Tuple<String, String>("GTM","Guatemala"));
				countries.Add(new Tuple<String, String>("GGY","Guernsey"));
				countries.Add(new Tuple<String, String>("GIN","Guinea"));
				countries.Add(new Tuple<String, String>("GNB","Guinea-Bissau"));
				countries.Add(new Tuple<String, String>("GUY","Guyana"));
				countries.Add(new Tuple<String, String>("HTI","Haiti"));
				countries.Add(new Tuple<String, String>("HMD","Heard Island and McDonald Islands"));
				countries.Add(new Tuple<String, String>("HND","Honduras"));
				countries.Add(new Tuple<String, String>("HKG","Hong Kong"));
				countries.Add(new Tuple<String, String>("HUN","Hungary"));
				countries.Add(new Tuple<String, String>("ISL","Iceland"));
				countries.Add(new Tuple<String, String>("IND","India"));
				countries.Add(new Tuple<String, String>("IDN","Indonesia"));
				countries.Add(new Tuple<String, String>("IRN","Iran"));
				countries.Add(new Tuple<String, String>("IRQ","Iraq"));
				countries.Add(new Tuple<String, String>("IRL","Ireland"));
				countries.Add(new Tuple<String, String>("IMN","Isle of Man"));
				countries.Add(new Tuple<String, String>("ISR","Israel"));
				countries.Add(new Tuple<String, String>("ITA","Italy"));
				countries.Add(new Tuple<String, String>("JAM","Jamaica"));
				countries.Add(new Tuple<String, String>("JPN","Japan"));
				countries.Add(new Tuple<String, String>("JEY","Jersey"));
				countries.Add(new Tuple<String, String>("JOR","Jordan"));
				countries.Add(new Tuple<String, String>("KAZ","Kazakhstan"));
				countries.Add(new Tuple<String, String>("KEN","Kenya"));
				countries.Add(new Tuple<String, String>("KIR","Kiribati"));
				countries.Add(new Tuple<String, String>("XKO","Kosovo"));
				countries.Add(new Tuple<String, String>("KWT","Kuwait"));
				countries.Add(new Tuple<String, String>("KGZ","Kyrgyzstan"));
				countries.Add(new Tuple<String, String>("LAO","Laos"));
				countries.Add(new Tuple<String, String>("LVA","Latvia"));
				countries.Add(new Tuple<String, String>("LBN","Lebanon"));
				countries.Add(new Tuple<String, String>("LSO","Lesotho"));
				countries.Add(new Tuple<String, String>("LBR","Liberia"));
				countries.Add(new Tuple<String, String>("LBY","Libya"));
				countries.Add(new Tuple<String, String>("LIE","Liechtenstein"));
				countries.Add(new Tuple<String, String>("LTU","Lithuania"));
				countries.Add(new Tuple<String, String>("LUX","Luxembourg"));
				countries.Add(new Tuple<String, String>("MAC","Macao"));
				countries.Add(new Tuple<String, String>("MKD","Macedonia"));
				countries.Add(new Tuple<String, String>("MDG","Madagascar"));
				countries.Add(new Tuple<String, String>("MWI","Malawi"));
				countries.Add(new Tuple<String, String>("MYS","Malaysia"));
				countries.Add(new Tuple<String, String>("MDV","Maldives"));
				countries.Add(new Tuple<String, String>("MLI","Mali"));
				countries.Add(new Tuple<String, String>("MLT","Malta"));
				countries.Add(new Tuple<String, String>("MHL","Marshall Islands"));
				countries.Add(new Tuple<String, String>("MTQ","Martinique"));
				countries.Add(new Tuple<String, String>("MRT","Mauritania"));
				countries.Add(new Tuple<String, String>("MUS","Mauritius"));
				countries.Add(new Tuple<String, String>("MYT","Mayotte"));
				countries.Add(new Tuple<String, String>("MEX","Mexico"));
				countries.Add(new Tuple<String, String>("FSM","Micronesia"));
				countries.Add(new Tuple<String, String>("MDA","Moldova"));
				countries.Add(new Tuple<String, String>("MCO","Monaco"));
				countries.Add(new Tuple<String, String>("MNG","Mongolia"));
				countries.Add(new Tuple<String, String>("MNE","Montenegro"));
				countries.Add(new Tuple<String, String>("MSR","Montserrat"));
				countries.Add(new Tuple<String, String>("MAR","Morocco"));
				countries.Add(new Tuple<String, String>("MOZ","Mozambique"));
				countries.Add(new Tuple<String, String>("MMR","Myanmar"));
				countries.Add(new Tuple<String, String>("NAM","Namibia"));
				countries.Add(new Tuple<String, String>("NRU","Nauru"));
				countries.Add(new Tuple<String, String>("NPL","Nepal"));
				countries.Add(new Tuple<String, String>("NLD","Netherlands"));
				countries.Add(new Tuple<String, String>("NCL","New Caledonia"));
				countries.Add(new Tuple<String, String>("NZL","New Zealand"));
				countries.Add(new Tuple<String, String>("NIC","Nicaragua"));
				countries.Add(new Tuple<String, String>("NER","Niger"));
				countries.Add(new Tuple<String, String>("NGA","Nigeria"));
				countries.Add(new Tuple<String, String>("NIU","Niue"));
				countries.Add(new Tuple<String, String>("NFK","Norfolk Island"));
				countries.Add(new Tuple<String, String>("PRK","North Korea"));
				countries.Add(new Tuple<String, String>("XNC","Northern Cyprus"));
				countries.Add(new Tuple<String, String>("MNP","Northern Mariana Islands"));
				countries.Add(new Tuple<String, String>("NOR","Norway"));
				countries.Add(new Tuple<String, String>("OMN","Oman"));
				countries.Add(new Tuple<String, String>("PAK","Pakistan"));
				countries.Add(new Tuple<String, String>("PLW","Palau"));
				countries.Add(new Tuple<String, String>("PSE","Palestina"));
				countries.Add(new Tuple<String, String>("PAN","Panama"));
				countries.Add(new Tuple<String, String>("PNG","Papua New Guinea"));
				countries.Add(new Tuple<String, String>("PRY","Paraguay"));
				countries.Add(new Tuple<String, String>("PER","Peru"));
				countries.Add(new Tuple<String, String>("PHL","Philippines"));
				countries.Add(new Tuple<String, String>("PCN","Pitcairn Islands"));
				countries.Add(new Tuple<String, String>("POL","Poland"));
				countries.Add(new Tuple<String, String>("PRT","Portugal"));
				countries.Add(new Tuple<String, String>("PRI","Puerto Rico"));
				countries.Add(new Tuple<String, String>("QAT","Qatar"));
				countries.Add(new Tuple<String, String>("COG","Republic of Congo"));
				countries.Add(new Tuple<String, String>("REU","Reunion"));
				countries.Add(new Tuple<String, String>("ROU","Romania"));
				countries.Add(new Tuple<String, String>("RUS","Russia"));
				countries.Add(new Tuple<String, String>("RWA","Rwanda"));
				countries.Add(new Tuple<String, String>("BLM","Saint-Barthelemy"));
				countries.Add(new Tuple<String, String>("MAF","Saint-Martin"));
				countries.Add(new Tuple<String, String>("SHN","Saint Helena"));
				countries.Add(new Tuple<String, String>("KNA","Saint Kitts and Nevis"));
				countries.Add(new Tuple<String, String>("LCA","Saint Lucia"));
				countries.Add(new Tuple<String, String>("SPM","Saint Pierre and Miquelon"));
				countries.Add(new Tuple<String, String>("VCT","Saint Vincent and the Grenadines"));
				countries.Add(new Tuple<String, String>("WSM","Samoa"));
				countries.Add(new Tuple<String, String>("SMR","San Marino"));
				countries.Add(new Tuple<String, String>("STP","Sao Tome and Principe"));
				countries.Add(new Tuple<String, String>("SAU","Saudi Arabia"));
				countries.Add(new Tuple<String, String>("SEN","Senegal"));
				countries.Add(new Tuple<String, String>("SRB","Serbia"));
				countries.Add(new Tuple<String, String>("SYC","Seychelles"));
				countries.Add(new Tuple<String, String>("SLE","Sierra Leone"));
				countries.Add(new Tuple<String, String>("SGP","Singapore"));
				countries.Add(new Tuple<String, String>("SXM","Sint Maarten"));
				countries.Add(new Tuple<String, String>("SVK","Slovakia"));
				countries.Add(new Tuple<String, String>("SVN","Slovenia"));
				countries.Add(new Tuple<String, String>("SLB","Solomon Islands"));
				countries.Add(new Tuple<String, String>("SOM","Somalia"));
				countries.Add(new Tuple<String, String>("ZAF","South Africa"));
				countries.Add(new Tuple<String, String>("SGS","South Georgia and the South Sandwich Islands"));
				countries.Add(new Tuple<String, String>("KOR","South Korea"));
				countries.Add(new Tuple<String, String>("SSD","South Sudan"));
				countries.Add(new Tuple<String, String>("ESP","Spain"));
				countries.Add(new Tuple<String, String>("LKA","Sri Lanka"));
				countries.Add(new Tuple<String, String>("SDN","Sudan"));
				countries.Add(new Tuple<String, String>("SUR","Suriname"));
				countries.Add(new Tuple<String, String>("SJM","Svalbard and Jan Mayen"));
				countries.Add(new Tuple<String, String>("SWZ","Swaziland"));
				countries.Add(new Tuple<String, String>("SWE","Sweden"));
				countries.Add(new Tuple<String, String>("CHE","Switzerland"));
				countries.Add(new Tuple<String, String>("SYR","Syria"));
				countries.Add(new Tuple<String, String>("TWN","Taiwan"));
				countries.Add(new Tuple<String, String>("TJK","Tajikistan"));
				countries.Add(new Tuple<String, String>("TZA","Tanzania"));
				countries.Add(new Tuple<String, String>("THA","Thailand"));
				countries.Add(new Tuple<String, String>("TLS","Timor-Leste"));
				countries.Add(new Tuple<String, String>("TGO","Togo"));
				countries.Add(new Tuple<String, String>("TKL","Tokelau"));
				countries.Add(new Tuple<String, String>("TON","Tonga"));
				countries.Add(new Tuple<String, String>("TTO","Trinidad and Tobago"));
				countries.Add(new Tuple<String, String>("TUN","Tunisia"));
				countries.Add(new Tuple<String, String>("TUR","Turkey"));
				countries.Add(new Tuple<String, String>("TKM","Turkmenistan"));
				countries.Add(new Tuple<String, String>("TCA","Turks and Caicos Islands"));
				countries.Add(new Tuple<String, String>("TUV","Tuvalu"));
				countries.Add(new Tuple<String, String>("UGA","Uganda"));
				countries.Add(new Tuple<String, String>("UKR","Ukraine"));
				countries.Add(new Tuple<String, String>("ARE","United Arab Emirates"));
				countries.Add(new Tuple<String, String>("GBR","United Kingdom"));
				countries.Add(new Tuple<String, String>("USA","United States"));
				countries.Add(new Tuple<String, String>("UMI","United States Minor Outlying Islands"));
				countries.Add(new Tuple<String, String>("URY","Uruguay"));
				countries.Add(new Tuple<String, String>("UZB","Uzbekistan"));
				countries.Add(new Tuple<String, String>("VUT","Vanuatu"));
				countries.Add(new Tuple<String, String>("VAT","Vatican City"));
				countries.Add(new Tuple<String, String>("VEN","Venezuela"));
				countries.Add(new Tuple<String, String>("VNM","Vietnam"));
				countries.Add(new Tuple<String, String>("VIR","Virgin Islands, U.S."));
				countries.Add(new Tuple<String, String>("WLF","Wallis and Futuna"));
				countries.Add(new Tuple<String, String>("ESH","Western Sahara"));
				countries.Add(new Tuple<String, String>("YEM","Yemen"));
				countries.Add(new Tuple<String, String>("ZMB","Zambia"));
				countries.Add(new Tuple<String, String>("ZWE","Zimbabwe"));
				
				// http://biogeo.ucdavis.edu/data/gadm2.8/kmz/TWN_adm0.kmz

				foreach(var tpl in countries)
				{
					// On charge !!!
					String url = "http://biogeo.ucdavis.edu/data/gadm2.8/kmz/" + tpl.Item1 + "_adm0.kmz";
	                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
	                objRequest.Proxy = _daddy.GetProxy(); // Là encore, on peut virer le proxy si non utilisé (NULL)
	                HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
	                String ftmp = "World\\" + tpl.Item2 + ".zip";
	                
	                // on efface le fichier tmp s'il existe
	                if (File.Exists(ftmp))
	                    File.Delete(ftmp);
	
	                using (Stream output = File.OpenWrite(ftmp)) // au lieu de f
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
	                ZipFile.ExtractToDirectory(ftmp, "World");
	                File.Delete(ftmp);
				}
				_daddy.MSG("ok");
            }
            catch (Exception ex)
            {
                _daddy.ShowException("Debug error", "An error in debug", ex);
                
            }
        }

		/// <summary>
		/// 
		/// </summary>
		public void AnimateMyFinds()
        {
        	try
            {       
				String datapath = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar;
				String ffmpeg = datapath + "ffmpeg.exe";
				String myfinds = datapath + "GPX" + Path.DirectorySeparatorChar + "PQ" + Path.DirectorySeparatorChar + "MyFindsPocketQuery.zip";
                String animpath = datapath + "Animation";
                bool genvideo = true;
                bool highquality = true;

                String more = "";
				if (!File.Exists(myfinds))
            	{
					more += "\r\n\r\nATTENTION : Pour utiliser cette fonction, vous devez copier votre PQ MyFinds dans " + myfinds;	
				}
				if (!File.Exists(ffmpeg))
            	{
					more += "\r\n\r\nATTENTION : Si vous souhaitez générer une vidéo d'animation, il est nécessaire que ffmpeg.exe soit présent dans " + datapath;	
					more += "\r\nVous pouvez le télécharger ici : https://ffmpeg.zeranoe.com/builds/ (bouton 'Download FFmpeg') et extraire uniquement ffmpeg.exe";
					genvideo = false;
				}
				
            	DialogResult dialogResult = MyMessageBox.Show("L'exécution de cette fonction va supprimer l'ensemble des caches affichées dans MGM jusqu'au prochain redémarrage.\r\n*** NE TOUCHEZ PAS A MGM pendant toute la durée de l'exécution de la fonction ! ***" + more + "\r\n\r\nVous pouvez choisir au préalable le fond cartographie de votre choix en allant dans l'affichage cartographique de MGM.\r\n\r\nVoulez-vous continuer ?",
	                                      _daddy.GetTranslator().GetString("AnimateFindsToolStripMenuItem"),
	                                      MessageBoxIcon.Question, _daddy.GetTranslator());
            	if (dialogResult != DialogResult.Yes)
                {
            		_daddy.MsgActionCanceled(_daddy);
            		return;
                }
            	
            	// On charge le fichier MyFindsPocketQuery.zip
            	if (File.Exists(myfinds))
            	{
            		bool onlyFrance = true;
	            	bool saveimg = true;
	            	
	            	List<ParameterObject> lstp = new List<ParameterObject>();
					lstp.Add(new ParameterObject(ParameterObject.ParameterType.Bool, onlyFrance, "fr", "Générer uniquement la France (sinon monde entier)"));
					lstp.Add(new ParameterObject(ParameterObject.ParameterType.Bool, saveimg, "img", "Sauver les images intermédiaires"));
                    lstp.Add(new ParameterObject(ParameterObject.ParameterType.Label, "", "video", "Paramètres vidéo"));
                    lstp.Add(new ParameterObject(ParameterObject.ParameterType.Bool, genvideo, "img", "Générer la vidéo (ffmpeg.exe requis dans /Data)", !File.Exists(ffmpeg)));
                    lstp.Add(new ParameterObject(ParameterObject.ParameterType.Bool, highquality, "img", "Vidéo en haute qualité (fichier plus gros)", !File.Exists(ffmpeg)));

                    ParametersChanger changer = new ParametersChanger();
	                changer.Title = _daddy.GetTranslator().GetString("AnimateFindsToolStripMenuItem");
	                changer.BtnCancel = _daddy.GetTranslator().GetString("BtnCancel");
	                changer.BtnOK = _daddy.GetTranslator().GetString("BtnOk");
	                changer.ErrorFormater = _daddy.GetTranslator().GetString("ErrWrongParameter");
	                changer.ErrorTitle = _daddy.GetTranslator().GetString("Error");
	                changer.Parameters = lstp;
	                changer.Font = _daddy.Font;
	                changer.Icon = _daddy.Icon;
	
	                if (changer.ShowDialog() == DialogResult.OK)
	                {
	                   onlyFrance = (lstp[0].Value == "True");
	                   saveimg = (lstp[1].Value == "True");
	                   genvideo = (lstp[3].Value == "True");
                        highquality = (lstp[4].Value == "True");

                        if (!saveimg && genvideo)
	                   {
	                   	_daddy.MsgActionError(_daddy, "Vous devez sauver les images pour pouvoir générer la vidéo");
	                   	return;
	                   }
	                }
	                else
	                {
	                	_daddy.MsgActionCanceled(_daddy);
	                	return;
	                }
	                
	                // Vérification de la présence des KML
	                string f;
	            	KmlManager.TypeKML typekml = KmlManager.TypeKML.FRA_adm;
	            	if (!onlyFrance)
	            	{
	            		f = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "KML" + Path.DirectorySeparatorChar + "World_ISO2.kml";
	            		typekml = KmlManager.TypeKML.World_ISO2;
	            	}
	            	else
	            	{
	            		f = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "KML" + Path.DirectorySeparatorChar + "FRA_adm2.kml";
	            		typekml = KmlManager.TypeKML.FRA_adm;
	            	}
	            	if (File.Exists(f) == false)
	            	{
	            		// Message d'erreur, on invite à télécharger la base
	            		if (_downloadinprogress)
	            			return;
		                if (_daddy.GetInternetStatus())
		                {
		                    dialogResult = MyMessageBox.Show(_daddy.GetTranslator().GetStringM("AskDownloadKMLNet"),
		                                      _daddy.GetTranslator().GetString("AskDownloadKMLTitle"),
		                                      MessageBoxIcon.Question, _daddy.GetTranslator());
		                    if (dialogResult == DialogResult.Yes)
		                    {
		                        // Ok on télécharge
		                        DownloadDB();
		                    }
		                }
		                else
		                {
		                    _daddy.MsgActionError(_daddy, _daddy.GetTranslator().GetStringM("AskDownloadKMLNoNet"));
		                }
	                    return;
	            	}
	            	
	                // Le préfixe des images
	                String imgprefix = ((onlyFrance)?"france":"world");
	                
	                // On crée anim s'il n'existe pas
	                if (!Directory.Exists(animpath))
	                	Directory.CreateDirectory(animpath);
	                
	                // On nettoie le répertoire
	                var imgs = Directory.GetFiles(animpath, imgprefix + "*.jpg");
	                foreach(var fi in imgs)
	                {
	                	File.Delete(fi);
	                }
	                
            		// On nettoie tout
            		_daddy.ClearOverlay_RESERVED2();
            		_daddy.CleanMGMInternals();
                    _daddy.BuildListViewCache();
                    _daddy._bUseFilter = false; // DON'T FORGET TO RESET THE FILTER !
                    _daddy.PopulateListViewCache(null);
                    
            		// On charge dans une liste les caches du MyFinds,
            		Dictionary<String, Geocache> cachestmp = new Dictionary<string, Geocache>();
            		_daddy.LoadGCZip(myfinds, ref cachestmp);
            		
            		
            		// On ne garde que les Françaises et les Found It
            		// Key : jour
            		// List : les caches trouvées
            		Dictionary<String, List<Geocache>> caches = new Dictionary<string, List<Geocache>>();
            		foreach(var pair in cachestmp)
            		{
            			var geo = pair.Value;
            			
            			// En France ?
            			if (!onlyFrance || (geo._Country == "France"))
            			{
            				String key = "";
            			
            				// On regarde si on a un Found it
            				foreach(var log in geo._Logs)
            				{
            					if (log._Type == "Found it")
            					{
            						key = MyTools.ParseDate(log._Date).ToString("yyyy-MM-dd");
            						break;
            					}
            				}
            				
            				// on insère ?
            				if (key != "")
            				{
            					// Existe déjà ?
            					if (caches.ContainsKey(key))
            						caches[key].Add(geo);
            					else
            					{
            						// On ajoute
            						var lst = new List<Geocache>();
            						lst.Add(geo);
            						caches.Add(key, lst);
            					}
            				}
            			}
            		}
            		            		
            		// On trie par date de trouvaille (dans un dico avec un tri sur la clé ?)
            		var jourstries = new List<String>(caches.Keys);
            		jourstries.Sort();
            		
            		// On affiche la carte de France, centrée au bon zoom
            		GMapOverlay overlaybigview = _daddy._cacheDetail._gmap.Overlays[GMapWrapper.MARKERS];
            		_daddy.ShowCacheMapInCacheDetail();
            		if (onlyFrance)
            		{
            			// La France
            			_daddy._cacheDetail._gmap.Zoom = 6;
		        		_daddy._cacheDetail.Location = new Point(0,0);
		    			_daddy._cacheDetail.Size = new Size(800,800);
		    			_daddy._cacheDetail._gmap.Position = new PointLatLng(46.5361926748986, 1.966552734375);
            		}
            		else
            		{
            			// Le Monde
            			_daddy._cacheDetail._gmap.Zoom = 2.5;
		            	_daddy._cacheDetail.Location = new Point(0,0);
		        		_daddy._cacheDetail.Size = new Size(1200,1200);
		        		_daddy._cacheDetail._gmap.Position = new PointLatLng(0, 0);
            		}
	        		
            		// On calcule la date de début et la date de fin
            		DateTime deb = MyTools.ParseDate(jourstries[0]); 
            		DateTime end = MyTools.ParseDate(jourstries[jourstries.Count - 1]);
            		
            		lstp = new List<ParameterObject>();
	            	lstp.Add(new ParameterObject(ParameterObject.ParameterType.Date, deb, "deb", _daddy.GetTranslator().GetString("LblStart")));
	                lstp.Add(new ParameterObject(ParameterObject.ParameterType.Date, end, "fin", _daddy.GetTranslator().GetString("LblEnd")));
	
	                changer = new ParametersChanger();
	                changer.Title = "Période à traiter";
	                changer.BtnCancel = _daddy.GetTranslator().GetString("BtnCancel");
	                changer.BtnOK = _daddy.GetTranslator().GetString("BtnOk");
	                changer.ErrorFormater = _daddy.GetTranslator().GetString("ErrWrongParameter");
	                changer.ErrorTitle = _daddy.GetTranslator().GetString("Error");
	                changer.Parameters = lstp;
	                changer.Font = _daddy.Font;
	                changer.Icon = _daddy.Icon;
	
	                if (changer.ShowDialog() == DialogResult.OK)
	                {
	                	deb = (DateTime)(lstp[0].ValueO);
	                    end = (DateTime)(lstp[1].ValueO);
	                    
	                    if (deb > end)
	                    {
	                        _daddy.MsgActionError(_daddy._cacheDetail, _daddy.GetTranslator().GetString("LblErrStartEnd"));
	                        return;
	                    }
	                }
	                else
	                {
	                	_daddy.MsgActionCanceled(_daddy._cacheDetail);
	                	return;
	                }
            		int days = (int)((end - deb).TotalDays);
            		
                    /*
            		// Petite attente
            		_daddy._ThreadProgressBarTitle = _daddy.GetTranslator().GetString("AnimateFindsToolStripMenuItem");
            		_daddy.CreateThreadProgressBarEnh();
	            	// Wait for the creation of the bar
	                while (_daddy._ThreadProgressBar == null)
	                {
	                    Thread.Sleep(10);
	                    Application.DoEvents();
	                }
	                // Le compteur
	                _daddy._ThreadProgressBar.progressBar1.Maximum = days + ((genvideo)?1:0);
	                */

            		// On va lire les KML des zones
	            	Dictionary<String,List<List<PointLatLng>>> dico;
	            	Dictionary<String,List<List<PointDouble>>> dicoinclusion;
	            	List<String> zones_trouvees = new List<string> ();
	            	KmlManager.ReadKml(f, out dico, typekml);
	               	// On va créer les points pour tester les inclusions
	               	dicoinclusion = new Dictionary<string, List<List<PointDouble>>> ();
	               	foreach(var pair in dico)
            		{
	               		List<List<PointDouble>> lst = new List<List<PointDouble>> ();
	               		
	               		foreach(var poly in pair.Value)
	               		{
	               			List<PointDouble> newpoly = new List<PointDouble>();
	               			foreach(var pt in poly)
	               			{
	               				PointDouble newpt = new PointDouble(pt.Lng, pt.Lat);
	               				newpoly.Add(newpt);
	               			}
	               			lst.Add(newpoly);
	               		}
	               		
	               		// On ajoute
	               		dicoinclusion.Add(pair.Key, lst);
	               	}
	               	
            		// On ajoute jour par jour dans la carto sur cette période en prenant une photo pour chaque jour
            		for(int i=0; i <= days; i++)
            		{
            			// Préparation du dessin
            			_daddy._cacheDetail._gmap.HoldInvalidation = true;
            			
            			// On calcule le jour
            			var oneday = deb.AddDays(i);
            			String key = oneday.ToString("yyyy-MM-dd");
                        /*
            			_daddy._ThreadProgressBar.lblWait.Text = "Jour " + key;
            			if (_daddy._ThreadProgressBar._bAbort)
            				break;
            			*/
                        if (days != 0)
                            _daddy._cacheDetail.Text = "Jour " + key + " " + ((int)(((double)(i * 100)) / ((double)days))).ToString() + "%";

                        // On affiche si besoin les caches
                        if (caches.ContainsKey(key))
            			{
            				var lst = caches[key];
            				// On ajoute les caches sur la carte
            				foreach(var geo in lst)
            				{
            					_daddy.DisplayCacheOnMaps(geo, null, overlaybigview, null, true);
            					
            					// On regarde si on a une région que l'on peut allumer
	            				foreach(var pair in dicoinclusion)
	            				{
	            					// Est-elle déjà allumée ? Si oui, on l'ignore
	            					if (!zones_trouvees.Contains(pair.Key))
	            					{
	            						// On cherche une inclusion
	            						bool inclu = false;
	            						foreach(var poly in pair.Value)
			                			{
	            							// On teste l'inclusion
	            							if (MyTools.PointInPolygon(new PointDouble(geo._dLongitude, geo._dLatitude), poly.ToArray()))
	            							{
	            								inclu = true;
	            								break;
	            							}
	            						}
	            						
	            						// Si on est inclu, on l'indique dans zones_trouvees et on dessine le polygone
	            						if (inclu)
	            						{
	            							zones_trouvees.Add(pair.Key);
	            							
	            							// On crée la zone
	            							foreach(var poly in dico[pair.Key])
	            							{
	                        					GMapPolygon area = new GMapPolygon(poly, "mazone");
	                        					area.Tag = null;
		                						area.Fill = new SolidBrush(Color.FromArgb(100, 0, 255, 0));
				            					area.Stroke = new Pen(Color.Green);
				            	                _daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2].Polygons.Add(area);
	            							}
	            						}
	            					}
	            				}
            				}
            			}
            			
            			// On finalise l'affichage
            			_daddy._cacheDetail._gmap.Refresh();
                    	_daddy._cacheDetail.gmap_OnMapZoomChanged();
                    	
            			// On sauve l'image
            			if (saveimg)
            			{
            				f = animpath + Path.DirectorySeparatorChar + imgprefix + (i+1).ToString() + ".jpg";
	            			Image img = _daddy._cacheDetail._gmap.ToImage();
	            			// On écrit la date
	            			Graphics g = Graphics.FromImage(img);
							g.SmoothingMode = SmoothingMode.AntiAlias;
							g.InterpolationMode = InterpolationMode.HighQualityBicubic;
							g.PixelOffsetMode = PixelOffsetMode.HighQuality;
							g.DrawString(key, new Font("Tahoma",24), Brushes.Black, 300, 10);
							g.Flush();
	            			img.Save(f, ImageFormat.Jpeg);
	            			img.Dispose();
            			}
            			
            			// Step
            			//_daddy._ThreadProgressBar.Step();
            		}
            		
            		// On génère la vidéo
            		if (/*(!_daddy._ThreadProgressBar._bAbort) &&*/ genvideo)
            		{
                        //_daddy._ThreadProgressBar.lblWait.Text = "Génération de la vidéo... Veuillez patienter...";
                        _daddy._cacheDetail.Text = "Génération de la vidéo... Veuillez patienter...";

                        String name = imgprefix + "_" + deb.ToString("yyyyMMdd") + "_" + end.ToString("yyyyMMdd") + ".avi";
            			String namepath = animpath + Path.DirectorySeparatorChar + name;
            			if (File.Exists(name))
            				File.Delete(name);

                        String ffmpegparam = "";
                        if (highquality)
                        {
                            if (imgprefix == "france")
                                ffmpegparam = " -b:v 2000k -minrate 2000k -maxrate 3000k -bufsize 1835k ";
                            else
                                ffmpegparam = " -b:v 4000k -minrate 4000k -maxrate 5000k -bufsize 1835k ";
                        }
            			String cmd = ffmpeg + " -f image2 -i " + animpath + Path.DirectorySeparatorChar + imgprefix + "%d.jpg " + ffmpegparam + namepath;
                        MyTools.ExecuteCommandSync(cmd);
            			
            			// Step
            			//_daddy._ThreadProgressBar.Step();
            			
            			// Petite vérif
            			if (File.Exists(namepath))
            			{
            				_daddy.MsgActionOk(_daddy._cacheDetail, "Terminé, résultats dans /data/animation, vidéo : " + name);
            			}
            			else
            			{
            				_daddy.MsgActionWarning(_daddy._cacheDetail, "Terminé, résultats dans /data/animation, vidéo NON GENEREE !");
            			}
            		}
            		//else if (_daddy._ThreadProgressBar._bAbort)
            		//	_daddy.MsgActionCanceled(_daddy._cacheDetail, "Opération annulée, résultats partiels dans /data/animation");
            		else
            			_daddy.MsgActionOk(_daddy._cacheDetail, "Terminé, résultats dans /data/animation");

                    //On arrête l'attente
                    //_daddy.KillThreadProgressBarEnh();
                    _daddy._cacheDetail.Text = _daddy.GetTranslator().GetString("CacheDetailTitle");

                    // On ouvre le dossier
                    MyTools.StartInNewThread(animpath);
            	}
            	else
            	{
            		_daddy.MsgActionError(_daddy,"Veuillez copier votre fichier MyFinds ici : " + myfinds);
            	}
            	
            }
            catch (Exception ex)
            {
                //_daddy.KillThreadProgressBarEnh();
                _daddy.ShowException("Crap, an error", "An error in AnimateMyFinds", ex);
            }
        }
        
		/// <summary>
		/// 
		/// </summary>
		public enum TypeTest
		{
			/// <summary>
			/// 
			/// </summary>
			TESTDECIMATION = 0,
			/// <summary>
			/// 
			/// </summary>
			TESTGENDBAREA = 1,
			/// <summary>
			/// 
			/// </summary>
			TESTREADDBAREA = 2,
			/// <summary>
			/// 
			/// </summary>
			TESTDISPLAYCOVERAGE = 3
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="typetest"></param>
		public void DoTest(TypeTest typetest)
		{
			try
            {
            	int decimation = 0;
            	double tolerance = 0;
            	_daddy.ClearOverlay_RESERVED2();
	            	
            	if (typetest == TypeTest.TESTDECIMATION)
            	{
	            	
            		int type = _daddy.FastChoice("Calque", new String[]{"Régions", "Départements", "Cantons", "Villes", "Communes"}) + 1;
	            	decimation = _daddy.FastChoice("Tolérance ‰", 500);
	            	
	            	
	            	_daddy.CreateThreadProgressBarEnh();
	            	// Wait for the creation of the bar
	                while (_daddy._ThreadProgressBar == null)
	                {
	                    Thread.Sleep(10);
	                    Application.DoEvents();
	                }
	                
	                TypeKML typekml = TypeKML.FRA_adm;
	            	String msg = "";
	            	int nb  = 0;
	            	string[] filePaths;
	            	if (typekml == TypeKML.Dpts)
	            	{
	            		String kmlpath = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "KML" + Path.DirectorySeparatorChar + "Dpts"  + Path.DirectorySeparatorChar ;
	            		filePaths = Directory.GetFiles(kmlpath, "*.kml", SearchOption.AllDirectories);
	            	}
	            	else
	            	{
	            		filePaths = new string[1];
					    filePaths[0] = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "KML" + Path.DirectorySeparatorChar + "FRA_adm" + type.ToString() + ".kml";
	            	}
	            	foreach (string f in filePaths)
	                {
	                	Dictionary<String,List<List<PointLatLng>>> dico;
	                	if (KmlManager.ReadKml(f, out dico, typekml))
	                	{
	                		foreach(KeyValuePair<String,List<List<PointLatLng>>> pair in dico)
				            {
	                			// Découpage nom / numéro
		                		String name = pair.Key;
		                		// On supprime après le # c'est styleUrl
	                			int pos = name.IndexOf('#');
	                			if (pos != -1)
	                				name = name.Substring(0, pos);
	                			
		                		String number = "";
		                		if (typekml == TypeKML.Dpts)
		                		{
	                				pos = name.IndexOf('-');
		                			number = name.Substring(0, pos -1);
		                			name = name.Substring(pos + 2);
		                		}
		                		_daddy._ThreadProgressBar.lblWait.Text = name;
		                		
		                		foreach(var oldpoly in pair.Value)
		                		{
		                			var newpoly = oldpoly;
		                			if (decimation != 0)
		                				newpoly = KmlManager.DouglasPeuckerReduction(oldpoly, ((double)decimation)/1000.0);
		                			
		                			GMapPolygon area = new GMapPolygon(newpoly, name);
		                			area.Tag = null;
		                			area.Stroke = new Pen(Color.Green);
		                			area.Fill = new SolidBrush(Color.FromArgb(100, 0, 255, 0));
		                			_daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2].Polygons.Add(area);
		                			
		                			msg += name + " " + newpoly.Count.ToString() + "\r\n";
		                			nb += newpoly.Count;
		                			
		                			String points = "";
		                			foreach(var pt in newpoly)
		                			{
		                				points += pt.Lat.ToString().Replace(',','.') + " " + pt.Lng.ToString().Replace(',','.') + " ";
		                			}
		                			points = points.Substring(0, points.Length - 1);
		                			
		                			if (_daddy._ThreadProgressBar._bAbort)
		                				break;
		                		}
		                		if (_daddy._ThreadProgressBar._bAbort)
		                			break;
	                		}
	                	}
	            	}
	            	_daddy.KillThreadProgressBarEnh();
	            	_daddy.ShowCacheMapInCacheDetail();
	            	msg = "TOTAL : " + nb.ToString() + "\r\n" + msg;
	            	_daddy.MSG(msg);
            	}
            	else if (typetest == TypeTest.TESTGENDBAREA)
            	{
	            	int deb = _daddy.FastChoice("Calque de début", new String[]{"Régions", "Départements", "Cantons"}) + 1;
	            	int fin = _daddy.FastChoice("Calque de fin", new String[]{"Régions", "Départements", "Cantons"}) + 1;
					GenerateFranceAreasDB(false, decimation, tolerance, deb, fin);
            	}
            	else if (typetest == TypeTest.TESTREADDBAREA)
            	{								
					int type = _daddy.FastChoice("Lecture du calque", new String[]{"Régions", "Départements", "Cantons"}) + 1;
					if (type >= 1)
					{
						List<Tuple<String,List<PointLatLng>>> areas = GetFranceAreasDB(type);
						int nb = 0;
						foreach(var tpl in areas)
						{
							GMapPolygon area = new GMapPolygon(tpl.Item2, tpl.Item1);
							nb += tpl.Item2.Count;
		        			area.Tag = null;
		        			area.Fill = new SolidBrush(Color.FromArgb(100, 0, 255, 0));
		        			area.Stroke = new Pen(Color.Green);
		        			_daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2].Polygons.Add(area);
						}
						_daddy.ShowCacheMapInCacheDetail();
						var msg = "TOTAL : " + nb.ToString() ;
	            		_daddy.MSG(msg);
					}
					
            	}
            	else if (typetest == TypeTest.TESTDISPLAYCOVERAGE)
            	{
					int type = _daddy.FastChoice("Affichage couverture du calque", new String[]{"Régions", "Départements", "Cantons", "Villes", "Communes"}) + 1;
					List<Tuple<Int32, String, List<GMapPolygon>>> allZones;
					DisplayFranceCoverageImpl(type, out allZones, TrackSelector.ColorType.GreenRed, false);
					String msg = "";
					int nb = 0;
					foreach(var area in allZones)
					{
						foreach(var pol in area.Item3)
						{
							nb += pol.Points.Count;
							msg += area.Item2 + " " + pol.Points.Count.ToString() + "\r\n";
						}
					}
					msg += "TOTAL : " + nb.ToString();
					_daddy.MSG(msg);
				}
            }
            catch (Exception ex)
            {
                _daddy.KillThreadProgressBarEnh();
                _daddy.ShowException("Debug error", "An error in DoTest", ex);
                
            }
		}
	}
}
