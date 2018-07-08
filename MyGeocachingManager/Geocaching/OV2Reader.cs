using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Threading;
using SpaceEyeTools;
using System.Data.SQLite;

namespace MyGeocachingManager.Geocaching
{
    /// <summary>
    /// Parser of OV2 format (TomTom POI)
    /// </summary>
    public class OV2Reader
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public OV2Reader()
        {
        }

        /// <summary>
        /// Create a list of empty geocaches from an OV2 file from SuperPP.
        /// SuperPP is a geocacher producing OV2 exports for France,
        /// see http://www.geocaching.com/profile/default.aspx?guid=05b50a82-2ae6-404b-958f-f184100acf25
        /// DB will be populated with the following valid attributes:
        /// - Code
        /// - Name
        /// - Type
        /// - Latitude
        /// - Longitude
        /// - Difficulty
        /// - Terrain
        /// </summary>
        /// <param name="con">SQLiteConnection</param>
        /// <param name="cmd">SQLiteCommand</param>
        /// <param name="filename">OV2 file</param>
        /// <param name="sType">Type of Geocache to be created (Traditional, etc...)</param>
        /// <returns>Number of inserted row into DB</returns>
        public int ProcessFileSuperPP(SQLiteConnection con, SQLiteCommand cmd, string filename, string sType)
        {
        	int nb = 0;
            string[] data = null;
            try
            {
                if (!File.Exists(filename))
                    return 0;

            	// Prepare transaction
            	SQLiteTransaction transaction = con.BeginTransaction();
            	
            	cmd.CommandText = "INSERT INTO GeocacheLite(Code, Name, Type, Latitude, Longitude, Difficulty, Terrain) VALUES(@Code, @Name, @Type, @Latitude, @Longitude, @Difficulty, @Terrain)";
				cmd.Parameters.AddWithValue("@Code", "");
		        cmd.Parameters.AddWithValue("@Name", "");
		        cmd.Parameters.AddWithValue("@Type", "");
                cmd.Parameters.AddWithValue("@Latitude", 0.0);
                cmd.Parameters.AddWithValue("@Longitude", 0.0);
                cmd.Parameters.AddWithValue("@Difficulty", "");
                cmd.Parameters.AddWithValue("@Terrain", "");
                        
                FileStream infile = new FileStream(filename, FileMode.Open, FileAccess.Read);
                while (infile.Length != infile.Position)
                {
                    data = readOV2Record(infile);
                    if (data != null)
                    {
                        //for (int i = 0; i < data.Length; i++)
                        // 0 : nom
                        // 1 : latitude
                        // 2 : longitude
                        // data[i]
                        double lon = MyTools.ConvertToDouble(data[2]);
						double lat = MyTools.ConvertToDouble(data[1]);
                        
                        // Le bon vieux format de SuperPP
                        String code = data[0].Substring(0, data[0].IndexOf(' '));
                        String name = data[0].Substring(data[0].IndexOf(" - ") + 3);
                        int iend = name.LastIndexOf(" (");
                        if (iend != -1)
                            name = name.Substring(0, iend);
                        
                        String D = "1";
                        String T = "1";
                        // On cherche à convertir la D/T
                        try
                        {
                            iend = data[0].LastIndexOf("(");
                            if (iend != -1)
                            {
                                string dt = data[0].Substring(iend);
                                dt = dt.Substring(0, dt.Length - 1);
                                D = dt.Substring(1, dt.IndexOf('/') - 1).Replace(",", ".");
                                T = dt.Substring(dt.IndexOf('/') + 1).Replace(",", ".");
                            }
                        }
                        catch (Exception)
                        {
                        }
                        
		                // Insert values using preparedcommands
		                cmd.Parameters["@Code"].Value = code;
		                cmd.Parameters["@Name"].Value = name;
		                cmd.Parameters["@Type"].Value = sType;
		                cmd.Parameters["@Latitude"].Value = lat;
		                cmd.Parameters["@Longitude"].Value = lon;
		                cmd.Parameters["@Difficulty"].Value = D;
		                cmd.Parameters["@Terrain"].Value = T;
		                cmd.ExecuteNonQuery();
		                
		                nb++;
                    }
                }
                infile.Close();

                // en transaction
                transaction.Commit();
                
             
                return nb;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        /// <summary>
        /// Create a list of empty geocaches from an OV2 file.
        /// Empty geocache will have the following valid attributes:
        /// - Longitude
        /// - Latitude
        /// - Type
        /// - Difficulty (only for SuperPP format)
        /// - Terrain (only for SuperPP format)
        /// All other attributes will have default values.
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        /// <param name="filename">OV2 file</param>
        /// <param name="sType">Type of Geocache to be created (Traditional, etc...)</param>
        /// <param name="bFromSuperPP">true if OV2 is generated by SuperPP
        /// SuperPP is a geocacher producing OV2 exports for France,
        /// see http://www.geocaching.com/profile/default.aspx?guid=05b50a82-2ae6-404b-958f-f184100acf25</param>
        /// <returns>List of created geocaches</returns>
        public List<Geocache> ProcessFile(MainWindow daddy, string filename, string sType, bool bFromSuperPP)
        {
            string[] data = null;
            try
            {
                List<Geocache> caches = new List<Geocache>();
                FileStream infile = new FileStream(filename, FileMode.Open, FileAccess.Read);
                int iIndexCache = 0;
                while (infile.Length != infile.Position)
                {
                    data = readOV2Record(infile);
                    if (data != null)
                    {
                        //for (int i = 0; i < data.Length; i++)
                        // 0 : nom
                        // 1 : latitude
                        // 2 : longitude
                        // data[i]
                        Geocache geo = new Geocache(daddy);
                        geo._Longitude = data[2].ToString().Replace(",", ".");
                        geo._Latitude = data[1].ToString().Replace(",", ".");
                        geo._dLongitude = MyTools.ConvertToDouble(geo._Longitude);
                        geo._dLatitude = MyTools.ConvertToDouble(geo._Latitude);
                        if (bFromSuperPP)
                        {
                            // Le bon vieux format de SuperPP
                            geo._Code = data[0].Substring(0, data[0].IndexOf(' '));
                            String n = data[0].Substring(data[0].IndexOf(" - ") + 3);
                            int iend = n.LastIndexOf(" (");
                            if (iend != -1)
                                n = n.Substring(0, iend);
                            geo._Name = n;
                            geo._CacheId = (daddy._iCacheId++).ToString();
                            geo._OwnerId = geo._CacheId;
                            geo._Country = "France";
                            geo._State = "Unknown";
                            geo._Type = sType;
                            geo._Available = "True";
                            geo._Archived = "False";
                            geo._Url = "http://coord.info/" + geo._Code;
                            geo._Owner = "Unknown";
                            geo._PlacedBy = "Unknown";
                            geo._ShortDescHTML = "none";
                            geo._ShortDescription = "none";
                            geo._LongDescHTML = "none";
                            geo._LongDescription = "none";
                            geo._Hint = "";
                            geo._DateCreation = DateTime.Now.ToString(GeocachingConstants._FalseDatePattern);
                            geo._Container = "Not chosen";

                            // On cherche à convertir la D/T
                            try
                            {
                                iend = data[0].LastIndexOf("(");
                                if (iend != -1)
                                {
                                    string dt = data[0].Substring(iend);
                                    dt = dt.Substring(0, dt.Length - 1);
                                    geo._D = dt.Substring(1, dt.IndexOf('/') - 1).Replace(",", ".");
                                    geo._T = dt.Substring(dt.IndexOf('/') + 1).Replace(",", ".");
                                }
                                else
                                {
                                    geo._D = "1";
                                    geo._T = "1";
                                }
                            }
                            catch (Exception)
                            {
                                geo._D = "1";
                                geo._T = "1";
                            }
                        }
                        else
                        {
                            // Un format inconnu...
                            String code = "GC";
                            String subcode = iIndexCache.ToString("X");
                            int izero = 5 - subcode.Length;
                            if (izero > 0)
                            {
                                String str = new String('0', izero);
                                code += str;
                            }
                            code += subcode;

                            geo._Code = code;
                            String n = data[0];
                            /*int iend = n.LastIndexOf(" (");
                            if (iend != -1)
                                n = n.Substring(0, iend);*/
                            geo._Name = n;
                            geo._CacheId = (daddy._iCacheId++).ToString();
                            geo._OwnerId = geo._CacheId;
                            geo._Country = "Unknown";
                            geo._State = "Unknown";
                            geo._Type = sType;
                            geo._Available = "True";
                            geo._Archived = "False";
                            geo._Url = "http://coord.info/FAKEGEOCACHE";
                            geo._Owner = "Unknown";
                            geo._PlacedBy = "Unknown";
                            geo._ShortDescHTML = "none";
                            geo._ShortDescription = "none";
                            geo._LongDescHTML = "none";
                            geo._LongDescription = "none";
                            geo._Hint = "";
                            geo._DateCreation = DateTime.Now.ToString(GeocachingConstants._FalseDatePattern);
                            geo._Container = "Not chosen";

                            // On cherche à convertir la D/T
                            /*try
                            {
                                iend = data[0].LastIndexOf("(");
                                if (iend != -1)
                                {
                                    string dt = data[0].Substring(iend);
                                    dt = dt.Substring(0, dt.Length - 1);
                                    geo._D = dt.Substring(1, dt.IndexOf('/') - 1).Replace(",", ".");
                                    geo._T = dt.Substring(dt.IndexOf('/') + 1).Replace(",", ".");
                                    if (geo._D == "")
                                        geo._D = "1";
                                    if (geo._T == "")
                                        geo._T = "1";
                                }
                                else
                                {
                                    geo._D = "1";
                                    geo._T = "1";
                                }
                            }
                            catch (Exception)*/
                            {
                                geo._D = "1";
                                geo._T = "1";
                            }
                        }
                       
                        iIndexCache++;
                        caches.Add(geo);
                    }
                }
                return caches;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private String ConvertUTF8ToNice(byte[] utfBytes)
        {
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding utf8 = Encoding.ASCII; //Encoding.UTF8;
            byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            string msg = iso.GetString(isoBytes);
            return msg;
        }

        private string[] readOV2Record(FileStream inputStream)
        {
            String[] record = null;
            int b = -1;

            if ((b = inputStream.ReadByte()) > -1)
            {
                // if it is a simple POI record
                if (b == 2)
                {

                    long total = readLong(inputStream);

                    double longitude = (double)readLong(inputStream) / 100000.0;
                    double latitude = (double)readLong(inputStream) / 100000.0;

                    byte[] r = new byte[(int)total - 13];
                    inputStream.Read(r, 0, (int)total - 13);

                    record = new String[3];
                    //string s = System.Text.ASCIIEncoding.ASCII.GetString(r);
                    String s = Encoding.GetEncoding("ISO-8859-1").GetString(r); // Correction du problème d'encodage
                    record[0] = s;
                    record[0] = record[0].Substring(0, record[0].Length - 1);
                    record[1] = latitude.ToString();
                    record[2] = longitude.ToString();
                }
                //if it is a deleted record
                else if (b == 0)
                {
                    byte[] r = new byte[9];
                    inputStream.Read(r, 0, 9);
                }
                //if it is a skipper record
                else if (b == 1)
                {
                    byte[] r = new byte[20];
                    inputStream.Read(r, 0, 20);
                }
                else
                {
                    //
                    return null;
                }
            }
            else
            {
                return null;
            }

            return record;
        }

        private long readLong(FileStream fis)
        {
            long res = 0;

            res = fis.ReadByte();
            res += fis.ReadByte() << 8;
            res += fis.ReadByte() << 16;
            res += fis.ReadByte() << 24;
            return res;
        }
    }
}