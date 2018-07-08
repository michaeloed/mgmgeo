using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Globalization;
using GMap.NET;
using SpaceEyeTools;
using MyGeocachingManager.Geocaching;
using System.Reflection;
using System.Configuration;

namespace MyGeocachingManager
{
    /// <summary>
    /// Standard filter, used in MGM. Contains all filterable Geocache elements.
    /// </summary>
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CacheFilter : ISerializable //derive your class from ISerializable
    {
        /// <summary>
        /// Actual version of filter. Not used in any processing
        /// </summary>
        public const string VERSION_FILTER    = "3.0";

        /// <summary>
        /// Version of the instanced object (can be different from VERSION_FILTER for older filter deserialized)
        /// </summary>
        public String _version;
        
        /// <summary>
        /// Name of the filter
        /// </summary>
        public String _name;
        
        /// <summary>
        /// List of containers we want to keep
        /// </summary>
        public List<String> _containers = new List<string>();
        
        /// <summary>
        /// List of sizes we want to keep
        /// </summary>
        public List<String> _types = new List<string>();
        
        /// <summary>
        /// Minimum distance from home for a cache
        /// </summary>
        public double _distMin;
        
        /// <summary>
        /// Maximum distance from home for a cache
        /// </summary>
        public double _distMax;
        
        /// <summary>
        /// Availability of a cache
        /// </summary>
        public bool _avail;
        
        /// <summary>
        /// Archived status of a cache
        /// </summary>
        public bool _archi;
        
        /// <summary>
        /// Owned by user
        /// </summary>
        public bool _bOwned = true;
        
        /// <summary>
        /// Found by user
        /// </summary>
        public bool _bFound = true;
        
        /// <summary>
        /// Minimum difficulty for a cache
        /// </summary>
        public double _dMin;
        
        /// <summary>
        /// Maximum difficulty for a cache
        /// </summary>
        public double _dMax;
        
        /// <summary>
        /// Minimum terrain for a cache
        /// </summary>
        public double _tMin;
        
        /// <summary>
        /// Maximum terrain for a cache
        /// </summary>
        public double _tMax;
        
        /// <summary>
        /// List of attributes we want to keep
        /// </summary>
        public List<String> _attributes;
        
        /// <summary>
        /// List of attributes we want to exclude
        /// </summary>
        public List<String> _attributesexcl;

        /// <summary>
        /// Indicates if we shall perform filtering on Size
        /// </summary>
        public bool _bFilterSize;
        
        /// <summary>
        /// Indicates if we shall perform filtering on Type of cache
        /// </summary>
        public bool _bFilterType;

        /// <summary>
        /// Indicates if we shall perform filtering on distance from home
        /// </summary>
        public bool _bFilterDistance;

        /// <summary>
        /// Indicates if we shall perform filtering on cache status
        /// </summary>
        public bool _bFilterStatus;

        /// <summary>
        /// Indicates if we shall perform filtering on difficulty
        /// </summary>
        public bool _bFilterDifficulty;

        /// <summary>
        /// Indicates if we shall perform filtering on terrain
        /// </summary>
        public bool _bFilterTerrain;

        /// <summary>
        /// Indicates if we shall perform filtering on caches owned
        /// </summary>
        public bool _bFilterOwner;

        /// <summary>
        /// If true, display with existing caches ALL caches satisfying status and availability filter
        /// If false, excludes ONLY caches satisfying status and availability filter
        /// </summary>
        public bool _bFilterOwnerDisplay;

        /// <summary>
        /// Indicates if we shall perform filtering on attributes present
        /// </summary>
        public bool _bFilterAttribute;

        /// <summary>
        /// If true, all provided attributes shall be present
        /// If false, at least one attribute shall be present
        /// </summary>
        public bool _bFilterAttributeAllOfThem;

        /// <summary>
        /// Indicates if we shall perform filtering on attributes not present
        /// </summary>
        public bool _bFilterAttributeOut;

        /// <summary>
        /// If true, all provided attributes shall be not present
        /// If false, at least one attribute shall be not present
        /// </summary>
        public bool _bFilterAttributeOutAllOfThem;

        /// <summary>
        /// If true, only manually selected caches will be displayed (filter is inactive)
        /// </summary>
        public bool _bOnlyManualSelection;

        /// <summary>
        /// Indicates if we shall perform filtering on TB/Geocoins presence
        /// </summary>
        public bool _bContainsTBGC;

        /// <summary>
        /// Indicates if we shall perform filtering on caches contained within an area
        /// </summary>
        public bool _bFilterArea;

        /// <summary>
        /// Indicates if we shall perform filtering on Country and state of a cache
        /// </summary>
        public bool _bFilterCountryState;

        /// <summary>
        /// Country value of a cache to be filtered 
        /// </summary>
        public String _sCountry;

        /// <summary>
        /// State (from Country) value of a cache to be filtered
        /// </summary>
        public String _sState;

        /// <summary>
        /// Type of filter to use for the provided text name in _name
        /// 1: Name of cache
        /// 2: Owner
        /// 3: GC Code
        /// 4: Tag 
        /// </summary>
        public int _iNameType;

        /// <summary>
        /// Name of the filter
        /// </summary>
        public string _description = "<>";
        
        /// <summary>
        /// Details on the filter (free field)
        /// </summary>
        public string _descriptionDetails = "";

        /// <summary>
        /// true if this filter shall be ignored
        /// </summary>
        public bool _bToIgnore;

        /// <summary>
        /// filename associated to the serialized filter
        /// </summary>
        public string _filename = "";

        /// <summary>
        /// Indicates if we shall perform filtering on caches based on proximity with coordinates
        /// </summary>
        public bool _bFilterNear;

        /// <summary>
        /// Maximum distance between cache and reference point for Near filter (_bFilterNear)
        /// </summary>
        public double _distMaxNear; // In Km only

        /// <summary>
        /// Latitude of reference point for Near filter (_bFilterNear) 
        /// </summary>
        public double _dLatNear;

        /// <summary>
        /// Longitude of reference point for Near filter (_bFilterNear)
        /// </summary>
        public double _dLonNear;

        // Area filter
        private PointDouble[] _areaarray;
        private String _areatxt;
        private String _areazoom;

        // Date filter
        /// <summary>
        /// Indicates if we shall perform filtering on creation date
        /// </summary>
        public bool _bFilterCreationDate;

        /// <summary>
        /// Indicates if we shall perform filtering on last log date
        /// </summary>
        public bool _bFilterLastLogDate;

        /// <summary>
        /// Number days since creation of a cache (used by _bFilterCreationDate)
        /// </summary>
        public int _iNbDaysCreation;

        /// <summary>
        /// Number days since last log of a cache (used by _bFilterLastLogDate)
        /// </summary>
        public int _iNbDaysLastLog;

        /// <summary>
        /// If true, date of creation shall be inferior or equal to _iNbDaysCreation
        /// </summary>
        public bool _bIsCreationInferiorOrEqual;
        
        /// <summary>
        /// If true, date of last log shall be inferior or equal to _iNbDaysLastLog
        /// </summary>
        public bool _bIsLastLogInferiorOrEqual;

        
        // Favorites
        /// <summary>
        /// Indicates if we shall perform filtering on favorites
        /// </summary>
        public bool _bFilterFavorites;
		/// <summary>
        /// Number favorites of a cache (used by _bFilterFavorites)
        /// </summary>
        public int _iNbFavorites;
		/// <summary>
        /// If true, favorites shall be superior or equal to _iNbFavorites
        /// </summary>
        public bool _bIsFavoritesSuperiorOrEqual;
        
        // Popularity
        /// <summary>
        /// Indicates if we shall perform filtering on Popularity
        /// </summary>
        public bool _bFilterPopularity;
		/// <summary>
        /// Number popularity of a cache (used by _bFilterPopularity)
        /// </summary>
        public double _dPopularity;
		/// <summary>
        /// If true, favorites shall be superior or equal to _dPopularity
        /// </summary>
        public bool _bIsPopularitySuperiorOrEqual;
        /// <summary>
        /// If true, comparison will be made against rating simple of a cache (Geocaching value)
        /// If false, comparison will be made against complexe rating of a cache (Lower bound of Wilson score confidence interval for a Bernoulli parameter, Project-GC value)
        /// </summary>
        public bool _ratingSimplePopularity;
        
        
        //Default constructor
        /// <summary>
        /// Default constructor (initializes attributes)
        /// </summary>
        public CacheFilter()
        {
            SetDefaultValues();
        }

        private void SetDefaultValues()
        {
            // Serialized
            _version = VERSION_FILTER;
            _name = "";
            _description = "<>";
            _descriptionDetails = "";
            _sCountry = "";
            _sState = "";

            _iNameType = 1;

            _avail = true;
            _archi = false;
            _bOwned = true;
            _bFound = true;
            _bFilterSize = false;
            _bFilterType = false;
            _bFilterDistance = false;
            _bFilterStatus = false;
            _bFilterDifficulty = false;
            _bFilterArea = false;
            _bFilterTerrain = false;
            _bFilterOwner = false;
            _bFilterOwnerDisplay = true;
            _bFilterAttribute = false;
            _bFilterAttributeOut = false;
            _bOnlyManualSelection = false;
            _bContainsTBGC = false;
            _bFilterCountryState = false;
            _bFilterAttributeAllOfThem = true;
            _bFilterAttributeOutAllOfThem = true;
            
            _dMin = 1.0;
            _dMax = 5.0;
            _tMin = 1.0;
            _tMax = 5.0;
            _distMin = 0.0;
            _distMax = 100.0;

            _containers = new List<string>();
            _types = new List<string>();
            _attributes = new List<string>();
            _attributesexcl = new List<string>();
            _areatxt = "";
            _areazoom = "12";
            _areaarray = null;

            // not serialized
            _bToIgnore = false;
            _filename = "";

            _bFilterNear = false;
            _distMaxNear = 20;
            _dLatNear = 0;
            _dLonNear = 0;

            // Date filter
            _bFilterCreationDate = false;
            _bFilterLastLogDate = false;
            _iNbDaysCreation = 7;
            _iNbDaysLastLog = 7;
            _bIsCreationInferiorOrEqual = true;
            _bIsLastLogInferiorOrEqual = true;
            
			// Fav / Pop
            _bFilterFavorites = false;
            _bFilterPopularity = false;
            _bIsFavoritesSuperiorOrEqual= true;
            _bIsPopularitySuperiorOrEqual = true;
            _ratingSimplePopularity = false;
            _iNbFavorites = 0;
            _dPopularity = 0.0;
        }

        /// <summary>
        /// Return an SQL select request using for supported part of the filter
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public String ToSQLSelect(int limit)
        {
        	String owner = ConfigurationManager.AppSettings["owner"];
        	String stm  = "SELECT * FROM GeocacheFull";
        	List<String> sqlchunks = new List<String>();
        	
        	// containe
        	if ( _bFilterSize && (_containers.Count != 0))
        	{
        		String s = "Container in (";
        		foreach(var c in _containers)
        		{
        			s += "'" + c + "',";
        		}
        		s = s.Substring(0, s.Length-1);
        		s += ")";
        		sqlchunks.Add(s);
        	}
        	
        	// cache type
        	if (_bFilterType && (_types.Count != 0))
        	{
        		String s = "Type in (";
        		foreach(var c in _types)
        		{
        			s += "'" + c + "',";
        		}
        		s = s.Substring(0, s.Length-1);
        		s += ")";
        		sqlchunks.Add(s);
        	}
                
        	// difficulty
        	if (_bFilterDifficulty)
        	{
        		String s = "Difficulty >= '" + _dMin.ToString().Replace(',','.') + "' AND Difficulty <= '" + _dMax.ToString().Replace(',','.') + "'";
        		sqlchunks.Add(s);
        	}
        	
        	// terrain
        	if (_bFilterTerrain)
        	{
        		String s = "Terrain >= '" + _tMin.ToString().Replace(',','.') + "' AND Terrain <= '" + _tMax.ToString().Replace(',','.') + "'";
        		sqlchunks.Add(s);
        	}
        	
        	// status
        	if (_bFilterStatus)
        	{
        		if (_archi && _avail)
        			sqlchunks.Add("Available AND Archived");	
        		else if (!_archi && _avail)
        			sqlchunks.Add("Available AND not Archived");	
        		else if (_archi && !_avail)
        			sqlchunks.Add("not Available AND Archived");	
        		else if (!_archi && !_avail)
        			sqlchunks.Add("not Available AND not Archived");	
        		
        	}
        	
        	// owned or found
        	if (_bFilterOwner)
            {
                if (_bFilterOwnerDisplay)
                {
                    if (_bOwned && _bFound)
                    {
                    	// Affiche toutes les caches
                    	// Rien à faire
                    }
                    else if (!_bOwned && _bFound)
                    {
                    	// Exclure les caches possédées
                    	sqlchunks.Add("LOWER(Owner) not like LOWER('%" + owner + "%')");
                    }
                    else if (_bOwned && !_bFound)
                    {
                    	// Exclure les caches trouvées
                    	sqlchunks.Add("not Found");	
                    }
                    else if (!_bOwned && !_bFound)
                    {
                    	// Exclure toutes les caches trouvées ou possédées.
                    	sqlchunks.Add("LOWER(Owner) not like LOWER('%" + owner + "%') AND not Found");
                    }
                }
                else
                {
                    if (_bOwned && _bFound)
                    {
                    	// Afficher uniquement les caches trouvées ou possédées
                    	sqlchunks.Add("LOWER(Owner) like LOWER('%" + owner + "%') OR Found");
                    }
                    else if (!_bOwned && _bFound)
                    {
                    	// Afficher uniquement les caches trouvées
                    	sqlchunks.Add("Found");	
                    }
                    else if (_bOwned && !_bFound)
                    {
                    	// Afficher uniquement les caches possédées
                    	sqlchunks.Add("LOWER(Owner) like LOWER('%" + owner + "%')");
                    }
                    else if (!_bOwned && !_bFound)
                    {
                    	// Afficher uniquement les caches non trouvées et non possédées
                    	sqlchunks.Add("LOWER(Owner) not like LOWER('%" + owner + "%') AND not Found");
                    }
                }
            }
        	
        	// has tb
        	if (_bContainsTBGC)
        	{
        		sqlchunks.Add("TBIds <> ''");	
        	}
        	
        	// date (creation & last log)
            if (_bFilterCreationDate)
            {
                DateTime bound = DateTime.Now.AddDays(-_iNbDaysCreation);
                String sbound = bound.ToString(GeocachingConstants._FalseDatePattern);
                if (_bIsCreationInferiorOrEqual)
                {
                    sqlchunks.Add("DateCreation >= '" + sbound + "'");
                }
                else
                {
                    sqlchunks.Add("DateCreation < '" + sbound + "'");
                }
            }
            
            // Attributes INCLUDED
            if (_bFilterAttribute)
            {
                // Filter caches containing specific attributes
                String s = "";
                if (_bFilterAttributeAllOfThem)
                {
                	s = "(";
                	if (_attributes.Count != 0)
                	{
                		int i = 0;
	                    foreach (String oneatt in _attributes)
	                    {
	                    	s += "LOWER(AttributesStatus) like '%" + oneatt.ToLower() + "%;'";
	                    	if (i != (_attributes.Count -1))
        						s += " AND ";
	                    	i++;
	                    }
                	}
                	s += ")";
                }
                else
                {
                    s = "(";
                	if (_attributes.Count != 0)
                	{
                		int i = 0;
	                    foreach (String oneatt in _attributes)
	                    {
	                    	s += "LOWER(AttributesStatus) like '%" + oneatt.ToLower() + "%;'";
	                    	if (i != (_attributes.Count -1))
        						s += " OR ";
	                    	i++;
	                    }
                	}
                	s += ")";
                    
                }
                sqlchunks.Add(s);
            }
            
            // Attributes EXCLUDED
            if (_bFilterAttributeOut)
            {
            	String s = "";
                if (_bFilterAttributeOutAllOfThem)
                {
                    // Filter caches NOT containing ALL specific attributes
                    // it means that as long as we find an attribute to exclude, the cache will be excluded.
                    // if any is not found, we keep the cache
                    s = "(";
                	if (_attributesexcl.Count != 0)
                	{
                		int i = 0;
	                    foreach (String oneatt in _attributesexcl)
	                    {
	                    	s += "LOWER(AttributesStatus) not like '%" + oneatt.ToLower() + "%;'";
	                    	if (i != (_attributesexcl.Count -1))
        						s += " AND ";
	                    	i++;
	                    }
                	}
                	s += ")";
                }
                else
                {
                    // Filter caches NOT containing ANY specific attributes
                    // it means that if we find any of the selected attribute in the
                    // cache, we discard it
                    s = "(";
                	if (_attributesexcl.Count != 0)
                	{
                		int i = 0;
	                    foreach (String oneatt in _attributesexcl)
	                    {
	                    	s += "LOWER(AttributesStatus) not like '%" + oneatt.ToLower() + "%;'";
	                    	if (i != (_attributesexcl.Count -1))
        						s += " OR ";
	                    	i++;
	                    }
                	}
                	s += ")";
                }
                sqlchunks.Add(s);
            }
            
            // Name contains
            if (_name != "")
            {
            	switch (_iNameType)
                {
                    case 1: // Name
                        {
                			sqlchunks.Add("LOWER(Name) like LOWER('%" + _name + "%')");
                            break;
                        }
                    case 2: // Owner
                        {
                            sqlchunks.Add("LOWER(Owner) like LOWER('%" + _name + "%')");
                            break;
                        }
                    case 3: // GC Code
                        {
                            sqlchunks.Add("LOWER(Code) like LOWER('%" + _name + "%')");
                            break;
                        }
                    default:
                        break;
                }
            }
            
            // country / State
            if (_bFilterCountryState)
            {
                if (_sState != "")
                	sqlchunks.Add("LOWER(State) like LOWER('%" + _sState + "%')");
                
                if (_sCountry != "")
                	sqlchunks.Add("LOWER(Country) like LOWER('%" + _sCountry + "%')");
            }
            
        	if (sqlchunks.Count != 0)
        	{
        		stm += " WHERE ";
				for(int i=0;i<sqlchunks.Count;i++)
        		{
					stm += sqlchunks[i];
        			if (i != (sqlchunks.Count -1))
        				stm += " AND ";
        		}
        	}
        	if (limit != 0)
				stm += " LIMIT " + limit.ToString();
        	
        	return stm;
        }
        
        /// <summary>
        /// Indicates if area for filter is defined
        /// </summary>
        /// <returns>true if area used for filter is defined</returns>
        public bool IsAreaDefined()
        {
            return (_areatxt != "");
        }

        /// <summary>
        /// return area used for filter
        /// </summary>
        /// <returns>all the points coordinates composing the area</returns>
        public String GetArea()
        {
            return _areatxt;
        }

        /// <summary>
        /// zoom level at which area was created (legacy) 
        /// </summary>
        /// <returns>zoom level (integer as an string)</returns>
        public String GetAreaZoom()
        {
            return _areazoom;
        }

        /// <summary>
        /// defines zoom level at which area was created
        /// </summary>
        /// <param name="zoom">zoom level (integer as an string)</param>
        public void SetAreaZoom(String zoom)
        {
            _areazoom = zoom;
        }

        /// <summary>
        /// return area used for filter
        /// </summary>
        /// <returns>all the points coordinates composing the area</returns>
        public PointDouble[] GetAreaArray()
        {
            return _areaarray;
        }

        /// <summary>
        /// return area used for filter, format used by GMap.Net
        /// </summary>
        /// <returns>all the points coordinates composing the area</returns>
        public List<PointLatLng> GetAreaArrayGMapNET()
        {
            List<PointLatLng> pts = new List<PointLatLng>();
            if (_areaarray != null)
            {
                foreach (PointDouble pt in _areaarray)
                {
                    pts.Add(new PointLatLng(pt.Y,pt.X));
                }
            }
            return pts;
        }

        /// <summary>
        /// Reset (empty) area used for filter
        /// </summary>
        public void ResetArea()
        {
            _areaarray = null;
            _areatxt = "";
        }

        /// <summary>
        /// Defines an area for filter
        /// </summary>
        /// <param name="atext">all the points coordinates composing the new area</param>
        /// <returns>true if area is valid (at least 3 points)</returns>
        public bool DefineArea(String atext)
        {
            //_areaarray = null;
            //_areatxt = "";
            if (atext == "")
                return false;

            List<string> lpts = atext.Split(':').ToList<string>();
            if ((lpts != null) && (lpts.Count >= 3))
            {
                List<PointDouble> pts = new List<PointDouble>();
                foreach (String ps in lpts)
                {
                    if (ps != "")
                    {
                        List<string> apt = ps.Split(';').ToList<string>();
                        if (apt.Count == 2)
                        {
                            pts.Add(new PointDouble(MyTools.ConvertToDouble(apt[1]), MyTools.ConvertToDouble(apt[0])));
                        }
                    }
                }
                if (pts.Count >= 3)
                {
                    _areaarray = pts.ToArray();
                    _areatxt = atext;
                    return true;
                }
                else
                    return false;
            }
            return false;
        }

        /// <summary>
        /// Deserialization constructor
        /// Performs version control
        /// </summary>
        /// <param name="info">info</param>
        /// <param name="ctxt">context</param>
        public CacheFilter(SerializationInfo info, StreamingContext ctxt)
        {
            //Get the values from info and assign them to the appropriate properties
            _name = (string)info.GetValue("_name", typeof(string));
            _description = (string)info.GetValue("_description", typeof(string));

            _avail = (bool)info.GetValue("_avail", typeof(bool));
            _archi = (bool)info.GetValue("_archi", typeof(bool));
            _bOwned = (bool)info.GetValue("_bOwned", typeof(bool));
            _bFound = (bool)info.GetValue("_bFound", typeof(bool));
            _bFilterSize = (bool)info.GetValue("_bFilterSize", typeof(bool));
            _bFilterType = (bool)info.GetValue("_bFilterType", typeof(bool));
            _bFilterDistance = (bool)info.GetValue("_bFilterDistance", typeof(bool));
            _bFilterStatus = (bool)info.GetValue("_bFilterStatus", typeof(bool));
            _bFilterDifficulty = (bool)info.GetValue("_bFilterDifficulty", typeof(bool));
            _bFilterTerrain = (bool)info.GetValue("_bFilterTerrain", typeof(bool));
            _bFilterOwner = (bool)info.GetValue("_bFilterOwner", typeof(bool));
            _bFilterOwnerDisplay = (bool)info.GetValue("_bFilterOwnerDisplay", typeof(bool));
            _bFilterAttribute = (bool)info.GetValue("_bFilterAttribute", typeof(bool));
            _bOnlyManualSelection = (bool)info.GetValue("_bOnlyManualSelection", typeof(bool));
            _bContainsTBGC = (bool)info.GetValue("_bContainsTBGC", typeof(bool));

            _dMin = (double)info.GetValue("_dMin", typeof(double));
            _dMax = (double)info.GetValue("_dMax", typeof(double));
            _tMin = (double)info.GetValue("_tMin", typeof(double));
            _tMax = (double)info.GetValue("_tMax", typeof(double));
            _distMin = (double)info.GetValue("_distMin", typeof(double));
            _distMax = (double)info.GetValue("_distMax", typeof(double));

            _containers = (List<string>)info.GetValue("_containers", typeof(List<string>));
            _types = (List<string>)info.GetValue("_types", typeof(List<string>));
            _attributes = (List<string>)info.GetValue("_attributes", typeof(List<string>));

            // New version of the filter, might not be available
            try
            {
                _attributesexcl = (List<string>)info.GetValue("_attributesexcl", typeof(List<string>));
            }
            catch (Exception)
            {
                _attributesexcl = new List<string>();
            }

            try
            {
                _iNameType = (int)info.GetValue("_iNameType", typeof(int));
            }
            catch (Exception)
            {
                _iNameType = 1;
            }
            
            try
            {
                _version = (string)info.GetValue("_version", typeof(string));
            }
            catch (Exception)
            {
                _version = VERSION_FILTER;
            }

            try
            {
                _bFilterNear = (bool)info.GetValue("_bFilterNear", typeof(bool));
                _distMaxNear = (double)info.GetValue("_distMaxNear", typeof(double));
                _dLatNear = (double)info.GetValue("_dLatNear", typeof(double));
                _dLonNear = (double)info.GetValue("_dLonNear", typeof(double));
            }
            catch (Exception)
            {
                _bFilterNear = false;
                _distMaxNear = 20;
                _dLatNear = 0;
                _dLonNear = 0;
            }

            try
            {
                _bFilterCountryState = (bool)info.GetValue("_bFilterCountryState", typeof(bool));
                _sCountry = (string)info.GetValue("_sCountry", typeof(string));
                _sState = (string)info.GetValue("_sState", typeof(string));
            }
            catch (Exception)
            {
                _bFilterCountryState = false;
                _sCountry = "";
                _sState = "";
            }

            try
            {
                _bFilterAttributeOut = (bool)info.GetValue("_bFilterAttributeOut", typeof(bool));
            }
            catch (Exception)
            {
                _bFilterAttribute = (bool)info.GetValue("_bFilterAttribute", typeof(bool));
                if (_bFilterAttribute && (_attributesexcl.Count != 0))
                    _bFilterAttributeOut = true;
                else
                    _bFilterAttributeOut = false;
            }

            try
            {
                _descriptionDetails = (string)info.GetValue("_descriptionDetails", typeof(string));
            }
            catch (Exception)
            {
                _descriptionDetails = "";
            }

            try
            {
                _areatxt = (String)info.GetValue("_areatxt", typeof(String));
                DefineArea(_areatxt);
            }
            catch (Exception)
            {
                _areaarray = null;
                _areatxt = "";
            }

            try
            {
                _areazoom = (String)info.GetValue("_areazoom", typeof(String));
            }
            catch (Exception)
            {
                _areazoom = "12";
            }

            try
            {
                _bFilterArea = (bool)info.GetValue("_bFilterArea", typeof(bool));
            }
            catch (Exception)
            {
                _bFilterArea = false;
            }

            try
            {
                _bFilterCreationDate = (bool)info.GetValue("_bFilterCreationDate", typeof(bool));
                _bFilterLastLogDate = (bool)info.GetValue("_bFilterLastLogDate", typeof(bool));
                _bIsCreationInferiorOrEqual = (bool)info.GetValue("_bIsCreationInferiorOrEqual", typeof(bool));
                _bIsLastLogInferiorOrEqual = (bool)info.GetValue("_bIsLastLogInferiorOrEqual", typeof(bool));
                _iNbDaysCreation = (int)info.GetValue("_iNbDaysCreation", typeof(int));
                _iNbDaysLastLog = (int)info.GetValue("_iNbDaysLastLog", typeof(int));

            }
            catch (Exception)
            {
                _bFilterCreationDate = false;
                _bFilterLastLogDate = false;
                _iNbDaysCreation = 7;
                _iNbDaysLastLog = 7;
                _bIsCreationInferiorOrEqual = true;
                _bIsLastLogInferiorOrEqual = true;
            }

            try
            {
                _bFilterAttributeAllOfThem = (bool)info.GetValue("_bFilterAttributeAllOfThem", typeof(bool));
                _bFilterAttributeOutAllOfThem = (bool)info.GetValue("_bFilterAttributeOutAllOfThem", typeof(bool));
            }
            catch (Exception)
            {
                _bFilterAttributeAllOfThem = true;
                _bFilterAttributeOutAllOfThem = true;
            }
            
            try
            {
            	_bFilterFavorites = (bool)info.GetValue("_bFilterFavorites", typeof(bool));;
	            _bFilterPopularity = (bool)info.GetValue("_bFilterPopularity", typeof(bool));;
	            _bIsFavoritesSuperiorOrEqual= (bool)info.GetValue("_bIsFavoritesSuperiorOrEqual", typeof(bool));;
	            _bIsPopularitySuperiorOrEqual = (bool)info.GetValue("_bIsPopularitySuperiorOrEqual", typeof(bool));;
	            _ratingSimplePopularity = (bool)info.GetValue("_ratingSimplePopularity", typeof(bool));;
	            _iNbFavorites = (int)info.GetValue("_iNbFavorites", typeof(int));;
	            _dPopularity = (double)info.GetValue("_dPopularity", typeof(double));;
            }
            catch(Exception)
            {
            	// Fav / Pop
	            _bFilterFavorites = false;
	            _bFilterPopularity = false;
	            _bIsFavoritesSuperiorOrEqual= true;
	            _bIsPopularitySuperiorOrEqual = true;
	            _ratingSimplePopularity = false;
	            _iNbFavorites = 0;
	            _dPopularity = 0.0;
            }
        }
                
        /// <summary>
        /// Serialization function
        /// </summary>
        /// <param name="info">info</param>
        /// <param name="ctxt">context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            //You can use any custom name for your name-value pair. But make sure you
            // read the values with the same name. For ex:- If you write EmpId as "EmployeeId"
            // then you should read the same with "EmployeeId"
            info.AddValue("_version", _version);
            info.AddValue("_name", _name);
            info.AddValue("_description", _description);
            info.AddValue("_descriptionDetails", _descriptionDetails);
            
            info.AddValue("_iNameType", _iNameType);
            
            info.AddValue("_avail", _avail);
            info.AddValue("_archi", _archi);
            info.AddValue("_bOwned", _bOwned);
            info.AddValue("_bFound", _bFound);
            info.AddValue("_bFilterSize", _bFilterSize);
            info.AddValue("_bFilterType", _bFilterType);
            info.AddValue("_bFilterDistance", _bFilterDistance);
            info.AddValue("_bFilterStatus", _bFilterStatus);
            info.AddValue("_bFilterDifficulty", _bFilterDifficulty);
            info.AddValue("_bFilterTerrain", _bFilterTerrain);
            info.AddValue("_bFilterOwner", _bFilterOwner);
            info.AddValue("_bFilterOwnerDisplay", _bFilterOwnerDisplay);
            info.AddValue("_bFilterAttribute", _bFilterAttribute);
            info.AddValue("_bFilterAttributeOut", _bFilterAttributeOut);
            info.AddValue("_bOnlyManualSelection", _bOnlyManualSelection);
            info.AddValue("_bContainsTBGC", _bContainsTBGC);

            info.AddValue("_dMin", _dMin);
            info.AddValue("_dMax", _dMax);
            info.AddValue("_tMin", _tMin);
            info.AddValue("_tMax", _tMax);
            info.AddValue("_distMin", _distMin);
            info.AddValue("_distMax", _distMax);

            info.AddValue("_containers", _containers);
            info.AddValue("_types", _types);
            info.AddValue("_attributes", _attributes);
            info.AddValue("_attributesexcl", _attributesexcl);

            info.AddValue("_bFilterNear", _bFilterNear);
            info.AddValue("_distMaxNear", _distMaxNear);
            info.AddValue("_dLatNear", _dLatNear);
            info.AddValue("_dLonNear", _dLonNear);

            info.AddValue("_bFilterCountryState", _bFilterCountryState);
            info.AddValue("_sCountry", _sCountry);
            info.AddValue("_sState", _sState);

            info.AddValue("_areatxt", _areatxt);
            info.AddValue("_areazoom", _areazoom);
            info.AddValue("_bFilterArea", _bFilterArea);

            info.AddValue("_bFilterCreationDate", _bFilterCreationDate);
            info.AddValue("_bFilterLastLogDate", _bFilterLastLogDate);
            info.AddValue("_iNbDaysCreation", _iNbDaysCreation);
            info.AddValue("_iNbDaysLastLog", _iNbDaysLastLog);
            info.AddValue("_bIsCreationInferiorOrEqual", _bIsCreationInferiorOrEqual);
            info.AddValue("_bIsLastLogInferiorOrEqual", _bIsLastLogInferiorOrEqual);
            info.AddValue("_bFilterAttributeAllOfThem", _bFilterAttributeAllOfThem);
            info.AddValue("_bFilterAttributeOutAllOfThem", _bFilterAttributeOutAllOfThem);
            
            info.AddValue("_bFilterFavorites", _bFilterFavorites);
            info.AddValue("_bFilterPopularity", _bFilterPopularity);
            info.AddValue("_bIsFavoritesSuperiorOrEqual", _bIsFavoritesSuperiorOrEqual);
            info.AddValue("_bIsPopularitySuperiorOrEqual", _bIsPopularitySuperiorOrEqual);
            info.AddValue("_ratingSimplePopularity", _ratingSimplePopularity);
            info.AddValue("_iNbFavorites", _iNbFavorites);
            info.AddValue("_dPopularity", _dPopularity);
        }

        /// <summary>
        /// Clone current instance and properly copies filter information
        /// </summary>
        /// <returns>new cloned instance</returns>
        public CacheFilter CreateClone()
        {
            CacheFilter cache = null;

            // Serialize in memory
            MemoryStream stream = new MemoryStream();
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Serialize(stream, this);

            // seek to start
            stream.Seek(0, SeekOrigin.Begin);

            // Deserialize from memory into a new object :-)
            cache = (CacheFilter)bformatter.Deserialize(stream);
            stream.Close();

            return cache;
        }

        /// <summary>
        /// Open a file and serialize the object into it in binary format.
        /// EmployeeInfo.osl is the file that we are creating. 
        /// Note: you can give any extension you want for your file
        /// If you use custom extensions, then the user will now 
        ///   that the file is associated with your program.
        /// </summary>
        /// <param name="file">file to serialize to</param>
        public void Serialize(String file)
        {
            Stream stream = File.Open(file, FileMode.Create);
            BinaryFormatter bformatter = new BinaryFormatter();

            bformatter.Serialize(stream, this);
            stream.Close();
        }

        /// <summary>
        /// Deserialize a filter
        /// </summary>
        /// <param name="file">serialized filter file</param>
        /// <returns>Instance of the deserialized filter</returns>
        static public CacheFilter Deserialize(String file)
        {
            //Clear mp for further usage.
            CacheFilter cache = null;

            //Open the file written above and read values from it.
            Stream stream = File.Open(file, FileMode.Open);
            BinaryFormatter bformatter = new BinaryFormatter();

            cache = (CacheFilter)bformatter.Deserialize(stream);
            stream.Close();

            return cache;
        }

        /// <summary>
        /// Check if a cache shall be displayed based on filter definition
        /// </summary>
        /// <param name="cache">Cache to be checked</param>
        /// <returns>true if cache shall be displayed (passes the filter definition)</returns>
        public virtual bool ToBeDisplayed(Geocache cache)
        {
            // Manual selection ?
            if (cache._bManualSelection)
                return true;
            if (_bOnlyManualSelection)
                return false;

            // Name contains
            if (_name != "")
            {
                switch (_iNameType)
                {
                    case 1: // Name
                        {
                            if ((cache._Name.ToLower().Contains(_name.ToLower()) == false))
                                return false;
                            break;
                        }
                    case 2: // Owner
                        {
                            if ((cache._Owner.ToLower().Contains(_name.ToLower()) == false))
                                return false;
                            break;
                        }
                    case 3: // GC Code
                        {
                            if ((cache._Code.ToLower().Contains(_name.ToLower()) == false))
                                return false;
                            break;
                        }
                    case 4: // Tag
                        {
                            // Get OCD for this cache
                            // If no OCD and match with a tag requested, remove !
                            // OR if ocd and not contain a tag, remove !
                            if ((cache._Ocd == null) || (cache._Ocd._Tags.Contains(_name.ToLower()) == false))
                                return false;
                            break;
                        }
                    default:
                        break;
                }
            }

            if (_bFilterType && (_types.Count != 0) && (_types.Contains(cache._Type, StringComparer.OrdinalIgnoreCase) == false))
                return false;

            if ( _bFilterSize && (_containers.Count != 0) && (_containers.Contains(cache._Container, StringComparer.OrdinalIgnoreCase) == false))
                return false;

            if (_bFilterDistance)
            {
                double d = cache.DistanceToHome();
                if ((_distMin > d) || (d > _distMax))
                    return false;
            }

            if (_bFilterStatus)
            {
                if (_avail != cache.getAvailable())
                    return false;

                if (_archi != cache.getArchived())
                    return false;
            }

            if (_bFilterDifficulty && ((_dMin > cache.getD()) || (cache.getD() > _dMax)))
                return false;

            if (_bFilterTerrain && ((_tMin > cache.getT()) || (cache.getT() > _tMax)))
                return false;

            if (_bFilterOwner)
            {
                // if _bFilterOwnerDisplay : display i
                // else exclude if not
                if (_bFilterOwnerDisplay)
                {
                    if (!_bOwned && cache._bOwned)
                        return false;

                    if (!_bFound && cache.IsFound())
                        return false;
                }
                else
                {
                    // Exclude, more difficult to understand
                    if (_bOwned && _bFound)
                    {
                    	// only found or owned cache
                    	if (cache._bOwned || cache.IsFound())
                    	{
                    		
                    	}
                    	else
                    		return false;
                    }
                    else if (!_bOwned && _bFound)
                    {
                    	// only found and not owned
                    	if (!cache._bOwned && cache.IsFound())
                    	{
                    		
                    	}
                    	else
                    		return false;
                    }
                    else if (_bOwned && !_bFound)
                    {
                    	// only owned and not found
                    	if (cache._bOwned && !cache.IsFound())
                    	{
                    		
                    	}
                    	else
                    		return false;
                    }
                    else if (!_bOwned && !_bFound)
                    {
                    	// only not owned and not found
                    	if (cache._bOwned || cache.IsFound())
                    		return false;
                    }
                }
            }

            // Attributes INCLUDED
            if (_bFilterAttribute)
            {
                // Filter caches containing specific attributes
                if (_bFilterAttributeAllOfThem)
                {
                    bool bTrouve = true;
                    foreach (String oneatt in _attributes)
                    {
                        String atttofind = oneatt.ToLower() + ";";
                        if (cache._txtAttributes.Contains(atttofind))
                        {
                            // Contains attribute, that's good
                            continue;
                        }
                        else
                        {
                            // Not contains, filtered
                            bTrouve = false;
                            break;
                        }
                    }
                    if (!bTrouve)
                        return false;
                }
                else
                {
                    // if at least one attribute of the selection if found, keep the cache
                    bool bTrouve = false;
                    foreach (String oneatt in _attributes)
                    {
                        String atttofind = oneatt.ToLower() + ";";
                        if (cache._txtAttributes.Contains(atttofind))
                        {
                            // Contains attribute, that's good
                            bTrouve = true;
                            break;
                        }
                        else
                        {
                            // Not contains, continue
                            continue;
                        }
                    }
                    if (!bTrouve)
                        return false;
                }
            }

            // Attributes EXCLUDED
            if (_bFilterAttributeOut)
            {
                if (_bFilterAttributeOutAllOfThem)
                {
                    // Filter caches NOT containing ALL specific attributes
                    // it means that as long as we find an attribute to exclude, the cache will be excluded.
                    // if any is not found, we keep the cache
                    bool bTrouve = true;
                    foreach (String oneatt in _attributesexcl)
                    {
                        String atttofind = oneatt.ToLower() + ";";
                        if (cache._txtAttributes.Contains(atttofind))
                        {
                            // Contains attributs, we don't want that
                            continue;
                        }
                        else
                        {
                            // Not contains, good, continue with next attribute
                            bTrouve = false;
                            break;
                        }
                    }

                    if (bTrouve)
                        return false;
                }
                else
                {
                    // Filter caches NOT containing ANY specific attributes
                    // it means that if we find any of the selected attribute in the
                    // cache, we discard it
                    bool bTrouve = false;
                    foreach (String oneatt in _attributesexcl)
                    {
                        String atttofind = oneatt.ToLower() + ";";
                        if (cache._txtAttributes.Contains(atttofind))
                        {
                            // Contains attributs, we don't want that
                            bTrouve = true;
                            break;
                        }
                        else
                        {
                            // Not contains, good
                            continue; // at least one attribute is not contained, let's keep the cache
                        }
                    }

                    if (bTrouve)
                        return false;
                }
            }


            // TB/GC
            if (_bContainsTBGC && (cache._listTB.Count == 0))
                return false;

            // Near caches
            if (_bFilterNear)
            {
                double d = cache.DistanceToCoord(_dLatNear, _dLonNear);
                if (d > _distMaxNear)
                    return false;
            }

            // country / State
            if (_bFilterCountryState)
            {
                String geotxt = "";
                if (_sState == "")
                    geotxt = ";";
                else
                    geotxt = cache._State + ";";
                if (_sCountry == "")
                    geotxt += "";
                else
                    geotxt += cache._Country;

                String cmptxt = _sState + ";" + _sCountry;

                if (cmptxt != geotxt)
                    return false;
            }

            // date (creation & last log)
            if (_bFilterCreationDate)
            {
                if (cache._dtDateCreation == DateTime.MaxValue)
                    return false;
                DateTime today = DateTime.Now;
                TimeSpan span = today - cache._dtDateCreation;
                if (_bIsCreationInferiorOrEqual)
                {
                    // ok if nbdays <= valuefilter
                    if (span.TotalDays > _iNbDaysCreation)
                        return false;
                }
                else
                {
                    // ok if nbdays <= valuefilter
                    if (span.TotalDays <= _iNbDaysCreation)
                        return false;
                }
            }
            if (_bFilterLastLogDate)
            {
                if (cache._dtDateLastLog == DateTime.MaxValue)
                    return false;
                DateTime today = DateTime.Now;
                TimeSpan span = today - cache._dtDateLastLog;
                if (_bIsLastLogInferiorOrEqual)
                {
                    // ok if nbdays <= valuefilter
                    if (span.TotalDays > _iNbDaysLastLog)
                        return false;
                }
                else
                {
                    // ok if nbdays <= valuefilter
                    if (span.TotalDays <= _iNbDaysLastLog)
                        return false;
                }
            }


            // favorites and popularity
            if (_bFilterFavorites)
            {
            	if ((cache._Ocd != null) && (cache._Ocd._iNbFavs != -1))
            	{
            		// on a des stats
            		if (_bIsFavoritesSuperiorOrEqual && (cache._Ocd._iNbFavs >= _iNbFavorites))
            		{
            			// good !
            		}
            		else if (!_bIsFavoritesSuperiorOrEqual && (cache._Ocd._iNbFavs < _iNbFavorites))
            		{
            			// good !
            		}
            		else
            			return false;
            		
            	}
            	else
            	{
            		// Pas de stat, on rejette de base
            		return false;
            	}
            }
                
            if (_bFilterPopularity)
            {
            	if ((cache._Ocd != null) && (
                    (_ratingSimplePopularity && (cache._Ocd._dRatingSimple != -1)) ||
                    ((!_ratingSimplePopularity) && (cache._Ocd._dRating != -1))
                    ))
            	{
            		// on a des stats
            		double rating = (_ratingSimplePopularity) ? cache._Ocd._dRatingSimple : cache._Ocd._dRating;
            		rating *= 100.0; // ATTENTION !!!
            		
            		if (_bIsPopularitySuperiorOrEqual && (rating >= _dPopularity))
            		{
            			// Good !
            		}
            		else if (!_bIsPopularitySuperiorOrEqual && (rating < _dPopularity))
            		{
            			// Good !
            		}
            		else
            			return false;
            	}
            	else
            	{
            		// Pas de stat, on rejette de base
            		return false;
            	}
            }
            
            
            // area array
            if (_bFilterArea && (_areaarray != null))
            {
                PointDouble ptcache = new PointDouble(cache._dLongitude, cache._dLatitude);
                if (!MyTools.PointInPolygon(ptcache, _areaarray))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Set default values for filter
        /// This filter will authorize any cache
        /// </summary>
        public void DefaultFilter()
        {
            SetDefaultValues();
            _containers = GeocachingConstants.GetSupportedCacheSize();
            _types = GeocachingConstants.GetSupportedCacheTypes();
        }

        /// <summary>
        /// ToString  implementation for this class
        /// </summary>
        /// <returns>String version of the object</returns>
        public override string ToString()
        {
            return _description;
        }
        
    }
}
