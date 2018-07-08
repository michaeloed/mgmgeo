using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Globalization;
using System.Net;

namespace MyGeocachingManager
{
    /// <summary>
    /// Class for various extra geocache information stored within MGM database
    /// </summary>
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class OfflineCacheData : ISerializable //derive your class from ISerializable
    {

        /// <summary>
        /// Current implementation version
        /// </summary>
        public const string VERSION_DATA = "1.4";

        /// <summary>
        /// Version of deserialized instance
        /// </summary>
        public String _version;

        /// <summary>
        /// Export date of instance
        /// </summary>
        public DateTime _dateExport;

        /// <summary>
        /// Geocaching code of associated geocache
        /// </summary>
        public String _Code;

        /// <summary>
        /// Comment on geocache
        /// </summary>
        public String _Comment;

        /// <summary>
        /// List of images present in geocache description
        /// </summary>
        public Dictionary<String, String> _ImageFilesFromDescription;

        /// <summary>
        /// List of spoilers associated to geocache
        /// </summary>
        public Dictionary<String, OfflineImageWeb> _ImageFilesSpoilers;

        /// <summary>
        /// If true, download process of associated image has been aborted
        /// </summary>
        public bool _bAborted;

        /// <summary>
        /// True if geocache is bookmarked
        /// </summary>
        public bool _bBookmarked;

        /// <summary>
        /// List of user defined tags for associated cache
        /// </summary>
        public List<String> _Tags;

        /// <summary>
        /// Number of favorites
        /// </summary>
        public int _iNbFavs;

        /// <summary>
        /// Number of founds
        /// </summary>
        public int _iNbFounds;

        /// <summary>
        /// Number of DNFs
        /// </summary>
        public int _iNbNotFounds;

        /// <summary>
        /// Number of founds by premium members
        /// </summary>
        public int _iNbFoundsPremium;

        /// <summary>
        /// Number of DNF by premium members
        /// </summary>
        public int _iNbNotFoundsPremium;

        /// <summary>
        /// Popularity of geocache, based on Geocaching.com formula (_iNbFavs/_iNbFounds)
        /// </summary>
        public double _dRating;

        /// <summary>
        /// Popularity of geocache, based on Project-GC formula (Lower bound of Wilson score confidence interval for a Bernoulli parameter)
        /// </summary>
        public double _dRatingSimple;

        /// <summary>
        /// Altitude of associated geocache, in meters
        /// </summary>
        public double _dAltiMeters;

        // Not serialised
        /// <summary>
        /// HTML description of geocache
        /// Temporary information for image download, not serialized
        /// </summary>
        public String _descHTML;

        /// <summary>
        /// True if no image has ever been downloaded for this cache
        /// </summary>
        public bool _NotDownloaded;

        /// <summary>
        /// Constructor
        /// </summary>
        public OfflineCacheData()
        {
            _bBookmarked = false;
            _NotDownloaded = true;
            _descHTML = "";
            _bAborted = false;
            _version = VERSION_DATA; 
            _dateExport = DateTime.Now;
            _Code = "";
            _Comment = "";
            _ImageFilesFromDescription = new Dictionary<string, string>();
            _ImageFilesSpoilers = new Dictionary<string, OfflineImageWeb>();
            _Tags = new List<string>();
            _iNbNotFounds = -1;
            _iNbFounds = -1;
            _iNbNotFoundsPremium = -1;
            _iNbFoundsPremium = -1;
            _dRating = -1.0;
            _dRatingSimple = -1.0;
            _iNbFavs = -1;
            _dAltiMeters = Double.MaxValue;
        }

        /// <summary>
        /// Retrieve list of user defined tags
        /// </summary>
        /// <returns>User defined tags separated by a ;</returns>
        public String GetTags()
        {
            String vt = "";
            foreach (String t in _Tags)
            {
                vt += t + ";";
            }
            return vt;
        }

        /// <summary>
        /// Add a list of user defined tags
        /// </summary>
        /// <param name="tags">user defined list of tags</param>
        public void AddTags(List<String> tags)
        {
            foreach (String t in tags)
            {
                if (!(_Tags.Contains(t)))
                    _Tags.Add(t);
            }
        }

        /// <summary>
        /// Remove a list of user defined tags
        /// </summary>
        /// <param name="tags">user defined list of tags</param>
        public void RemoveTags(List<String> tags)
        {
            foreach (String t in tags)
            {
                if (_Tags.Contains(t))
                    _Tags.Remove(t);
            }
        }

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>String version of the object</returns>
        public override string ToString()
        {
            string s = "_NotDownloaded: " + _NotDownloaded.ToString() + "\r\n";
            s += "_bBookmarked: " + _bBookmarked.ToString() + "\r\n";
            s += "_bAborted: " + _bAborted.ToString() + "\r\n";
            s += "_version: " + _version + "\r\n";
            s += "_dateExport: " + _dateExport.ToShortDateString() + " " + _dateExport.ToShortTimeString() + "\r\n";
            s += "_Code: " + _Code + "\r\n";
            s += "_Comment: " + _Comment + "\r\n";
            s += "_iNbFavs: " + _iNbFavs.ToString() + "\r\n";
            s += "_iNbFounds: " + _iNbFounds.ToString() + "\r\n";
            s += "_iNbNotFounds: " + _iNbNotFounds.ToString() + "\r\n";
            s += "_iNbFoundsPremium: " + _iNbFoundsPremium.ToString() + "\r\n";
            s += "_iNbNotFoundsPremium: " + _iNbNotFoundsPremium.ToString() + "\r\n";
            s += "_dRating: " + _dRating.ToString() + "\r\n";
            s += "_dRatingSimple: " + _dRatingSimple.ToString() + "\r\n";
            s += "_dAltiMeters: " + _dAltiMeters.ToString() + "\r\n";
            s += "IsEmpty(): " + IsEmpty().ToString() + "\r\n";
            s += "Tags: " + GetTags() + "\r\n";
            s += "_ImageFilesFromDescription: " + _ImageFilesFromDescription.Count.ToString() + "\r\n";
            foreach (KeyValuePair<string, string> p1 in _ImageFilesFromDescription)
            {
                s += "   Distant: " + p1.Key + "\r\n";
                s += "   Local: " + p1.Value + "\r\n";
            }
            s += "_ImageFilesSpoilers: " + _ImageFilesSpoilers.Count.ToString() + "\r\n";
            foreach (KeyValuePair<string, OfflineImageWeb> p2 in _ImageFilesSpoilers)
            {
                s += "   Url: " + p2.Key + "\r\n";
                s += "   _name: " + p2.Value._name + "\r\n";
                s += "   _localfile: " + p2.Value._localfile + "\r\n";
                s += "   _dateExport: " + p2.Value._dateExport.ToString() + "\r\n";
                s += "   _url: " + p2.Value._url + "\r\n";

            }

            return s;
        }

        /// <summary>
        /// If true, the following values are defined:
        /// _iNbFavs OR
        /// _iNbFounds OR
        /// _iNbNotFounds OR
        /// _iNbFoundsPremium OR
        /// _iNbNotFoundsPremium OR
        /// _dRating OR
        /// _dRatingSimple
        /// OR _dAltiMeters
        /// </summary>
        /// <returns>Value of _iNbFavs OR
        /// _iNbFounds OR
        /// _iNbNotFounds OR
        /// _iNbFoundsPremium OR
        /// _iNbNotFoundsPremium OR
        /// _dRating OR
        /// _dRatingSimple OR
        /// _dAltiMeters</returns>
        public bool HasStats()
        {
            // Si tout est à -1, alors on n'A PAS de stat
            if ((_iNbFavs == -1) && (_iNbFounds == -1) && (_iNbNotFounds == -1) && (_iNbFoundsPremium == -1) && (_iNbNotFoundsPremium == -1) && (_dRating == -1.0) && (_dRatingSimple == -1.0) && (_dAltiMeters == Double.MaxValue))
                return false;
            else
                return true;

        }

        /// <summary>
        /// Indicates if instance is empty (no extra information contained)
        /// </summary>
        /// <returns>true if:
        /// HasStats is false AND
        /// _Comment is empty AND
        /// _bBookmarked is false AND
        /// _NotDownloaded is true AND
        /// _Tags count is 0
        /// </returns>
        public bool IsEmpty()
        {
            //return ((_Comment == "") && (_ImageFilesFromDescription.Count == 0) && (_ImageFilesSpoilers.Count == 0));

            // an OCD is considered empty if it has no comment and if it has never been downloaded.
            // We want to keep the info that it has been downloaded and that it has no picture, it might prevent
            // the user to downloaded it again
            return ((!HasStats()) && (_Comment == "") && (!_bBookmarked) && _NotDownloaded && (_Tags.Count == 0));
        }

        /// <summary>
        /// Check if some referenced files are missing
        /// </summary>
        /// <param name="pathdata">Path containing files database</param>
        /// <returns>true is files are missing</returns>
        public bool HasMissingFiles(String pathdata)
        {
            foreach (KeyValuePair<String, String> paire in _ImageFilesFromDescription)
            {
                try
                {
                    String f = pathdata + Path.DirectorySeparatorChar + paire.Value;
                    if (!File.Exists(f))
                        return true;
                }
                catch (Exception)
                {
                }
            }
          
            foreach (KeyValuePair<String, OfflineImageWeb> paire2 in _ImageFilesSpoilers)
            {
                try
                {
                    String f = pathdata + Path.DirectorySeparatorChar + paire2.Value._localfile;
                    if (!File.Exists(f))
                        return true;
                }
                catch (Exception)
                {
                }
            }

            return false;
        }

        /// <summary>
        /// Remove all associated files!
        /// </summary>
        /// <param name="pathdata">Path containing files database</param>
        public void PurgeFiles(String pathdata)
        {
            foreach (KeyValuePair<String, String> paire in _ImageFilesFromDescription)
            {
                try
                {
                    String f = pathdata + Path.DirectorySeparatorChar + paire.Value;
                    if (File.Exists(f))
                        File.Delete(f);
                }
                catch (Exception)
                {
                }
            }
            _ImageFilesFromDescription.Clear();

            foreach (KeyValuePair<String, OfflineImageWeb> paire2 in _ImageFilesSpoilers)
            {
                try
                {
                    String f = pathdata + Path.DirectorySeparatorChar + paire2.Value._localfile;
                    if (File.Exists(f))
                        File.Delete(f);
                }
                catch (Exception)
                {
                }
            }
            _ImageFilesSpoilers.Clear();

            _NotDownloaded = true;
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">info</param>
        /// <param name="ctxt">context</param>
        public OfflineCacheData(SerializationInfo info, StreamingContext ctxt)
        {
            //Get the values from info and assign them to the appropriate properties
            _version = (string)info.GetValue("_version", typeof(string));
            _dateExport = (DateTime)info.GetValue("_dateExport", typeof(DateTime));
            _Code = (string)info.GetValue("_Code", typeof(string));
            _Comment = (string)info.GetValue("_Comment", typeof(string));
            _ImageFilesFromDescription = (Dictionary<string, string>)info.GetValue("_ImageFiles", typeof(Dictionary<string, string>));

            try
            {
                _ImageFilesSpoilers = (Dictionary<string, OfflineImageWeb>)info.GetValue("_OffImageFiles", typeof(Dictionary<string, OfflineImageWeb>));
            }
            catch (Exception)
            {
                _ImageFilesSpoilers = new Dictionary<string, OfflineImageWeb>();
            }

            try
            {
                _NotDownloaded = (bool)info.GetValue("_NotDownloaded", typeof(bool));
            }
            catch (Exception)
            {
                // To bad for the past, we assume old caches were all downloaedd
                _NotDownloaded = false;
            }

            try
            {
                _bBookmarked = (bool)info.GetValue("_bBookmarked", typeof(bool));
            }
            catch (Exception)
            {
                // To bad for the past, we assume old caches were all downloaedd
                _bBookmarked = false;
            }

            try
            {
                _Tags = (List<string>)info.GetValue("_Tags", typeof(List<string>));
            }
            catch (Exception)
            {
                _Tags = new List<string>();
            }

            try
            {
                _iNbFavs = (int)info.GetValue("_iNbFavs", typeof(int));
                _iNbFounds = (int)info.GetValue("_iNbFounds", typeof(int));
                _iNbNotFounds = (int)info.GetValue("_iNbNotFounds", typeof(int));
            }
            catch (Exception)
            {
                // To bad for the past, we assume old caches were all downloaedd
                _iNbFavs = -1;
                _iNbFounds = -1;
                _iNbNotFounds = -1;
            }

            try
            {
                _dRating = (double)info.GetValue("_dRating", typeof(double));
                _iNbFoundsPremium = (int)info.GetValue("_iNbFoundsPremium", typeof(int));
                _iNbNotFoundsPremium = (int)info.GetValue("_iNbNotFoundsPremium", typeof(int));
            }
            catch (Exception)
            {
                // To bad for the past, we assume old caches were all downloaedd
                _dRating = -1.0;
                _iNbFoundsPremium = -1;
                _iNbNotFoundsPremium = -1;
            }

            try
            {
                _dRatingSimple = (double)info.GetValue("_dRatingSimple", typeof(double));
            }
            catch (Exception)
            {
                _dRatingSimple = _dRating;
            }

            try
            {
                _dAltiMeters = (double)info.GetValue("_dAltiMeters", typeof(double));
            }
            catch (Exception)
            {
                _dAltiMeters = Double.MaxValue;
            }
        }

        /// <summary>
        /// Serialization function.
        /// </summary>
        /// <param name="info">info</param>
        /// <param name="ctxt">context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            //You can use any custom name for your name-value pair. But make sure you
            // read the values with the same name. For ex:- If you write EmpId as "EmployeeId"
            // then you should read the same with "EmployeeId"
            info.AddValue("_version", _version);
            info.AddValue("_dateExport", _dateExport);
            info.AddValue("_Code", _Code);
            info.AddValue("_Comment", _Comment);
            info.AddValue("_ImageFiles", _ImageFilesFromDescription);
            info.AddValue("_OffImageFiles", _ImageFilesSpoilers);
            info.AddValue("_NotDownloaded", _NotDownloaded);
            info.AddValue("_bBookmarked", _bBookmarked);
            info.AddValue("_Tags", _Tags);
            info.AddValue("_iNbFavs", _iNbFavs);
            info.AddValue("_iNbFounds", _iNbFounds);
            info.AddValue("_iNbNotFounds", _iNbNotFounds);
            info.AddValue("_iNbFoundsPremium", _iNbFoundsPremium);
            info.AddValue("_iNbNotFoundsPremium", _iNbNotFoundsPremium);
            info.AddValue("_dRating", _dRating);
            info.AddValue("_dRatingSimple", _dRatingSimple);
            info.AddValue("_dAltiMeters", _dAltiMeters);
        }

        /// <summary>
        /// Write stats header in a file (for export)
        /// </summary>
        /// <param name="fi">StreamWriter of open file</param>
        public static void WriteStatsHeader(System.IO.StreamWriter fi)
        {
            String s =
                    "HEADER;1.0;" +
                    DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "\r\n" +
                    "CODE;DATE;FAVS;FOU;NFOU;FOUP;NFOUP;RAT;RATS;ALTI";
            fi.WriteLine(s);
        }

        /// <summary>
        /// Write stats in a file (for export)
        /// </summary>
        /// <param name="fi">StreamWriter of open file</param>
        public void WriteStats(System.IO.StreamWriter fi)
        {
            if (HasStats())
            {
                String s =
                    _Code + ";" +
                    String.Format("{0:yyyy/MM/dd HH:mm:ss}", _dateExport) + ";" +
                    _iNbFavs.ToString() + ";" +
                    _iNbFounds.ToString() + ";" +
                    _iNbNotFounds.ToString() + ";" +
                    _iNbFoundsPremium.ToString() + ";" +
                    _iNbNotFoundsPremium.ToString() + ";" +
                    _dRating.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + ";" +
                    _dRatingSimple.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + ";" +
                    _dAltiMeters.ToString(CultureInfo.CreateSpecificCulture("en-GB"));

                fi.WriteLine(s);
            }
        }

        /// <summary>
        /// Read stats from a file (for import)
        /// </summary>
        /// <param name="line">Stats for ONE OfflineCacheData</param>
        /// <returns>New OfflineCacheData with stats from file</returns>
        public static OfflineCacheData ReadStats(String line)
        {
            if (line != "")
            {
                List<string> myList = line.Split(';').ToList();
                if (myList.Count == 10)
                {
                    OfflineCacheData ocd = new OfflineCacheData();
                    ocd._Code = myList[0];
                    DateTime dt;
                    DateTime.TryParseExact(myList[1], "yyyy/MM/dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out dt);
                    ocd._dateExport = dt;
                    ocd._iNbFavs = Int32.Parse(myList[2]);
                    ocd._iNbFounds = Int32.Parse(myList[3]);
                    ocd._iNbNotFounds = Int32.Parse(myList[4]);
                    ocd._iNbFoundsPremium = Int32.Parse(myList[5]);
                    ocd._iNbNotFoundsPremium = Int32.Parse(myList[6]);
                    ocd._dRating = Double.Parse(myList[7], CultureInfo.CreateSpecificCulture("en-GB"));
                    ocd._dRatingSimple = Double.Parse(myList[8], CultureInfo.CreateSpecificCulture("en-GB"));
                    try
                    {
                        ocd._dAltiMeters = Double.Parse(myList[9], CultureInfo.CreateSpecificCulture("en-GB"));
                    }
                    catch (Exception) { }
                    return ocd;
                }
                else
                    return null;
            }
            else
                return null;
        }

        /// <summary>
        /// Merge stats from existing OfflineCacheData if they are better defined
        /// use new stats IF
        /// - more _iNbFounds
        /// - or more _iNbNotFounds
        /// - or more _iNbFavs
        /// - don't bother using the date, it's wrong anyway
        /// If altitude of ocd is valid, we will merge it anyway
        /// </summary>
        /// <param name="ocd">existing OfflineCacheData with stats</param>
        /// <returns>true if stats have been merged</returns>
        public bool MergeStats(OfflineCacheData ocd)
        {
            // Simple check
            if ((this == ocd) || (this._Code != ocd._Code))
                return false;


            bool merged = false;
            // use new stats IF
            // more _iNbFounds
            // or more _iNbNotFounds
            // or more _iNbFavs
            // don't bother using the date, it's wrong anyway
            if ((ocd._iNbFavs > this._iNbFavs) ||
                (ocd._iNbFounds > this._iNbFounds) ||
                (ocd._iNbNotFounds > this._iNbNotFounds))
            {
                // We merge it
                this._iNbFavs = ocd._iNbFavs;
                this._iNbFounds = ocd._iNbFounds;
                this._iNbFoundsPremium = ocd._iNbFoundsPremium;
                this._iNbNotFounds = ocd._iNbNotFounds;
                this._iNbNotFoundsPremium = ocd._iNbNotFoundsPremium;
                this._dRating = ocd._dRating;
                this._dRatingSimple = ocd._dRatingSimple;
                merged = true;
            }
            else
                merged = false;

            // Si l'altitude est valide, on l'utilise
            if (ocd._dAltiMeters != Double.MaxValue)
            {
                this._dAltiMeters = ocd._dAltiMeters;
                merged = true;
            }

            return merged;
        }
    }

    /// <summary>
    /// Structure used for stats retrieval
    /// </summary>
    public struct DataForStatsRetrieval
    {
        /// <summary>
        /// Number of previously missed stats due to an error
        /// </summary>
        public int inbmissed;

        /// <summary>
        /// If true, stats retrieval process will be stopped
        /// </summary>
        public bool stopScoreRetrieval;

        /// <summary>
        /// If true, question to stop retrieval has never been asked before in the current process
        /// </summary>
        public bool firstQuestion;

        /// <summary>
        /// Cookie containing user authentication for GC.com
        /// </summary>
        public CookieContainer cookieJar;
    }
}
