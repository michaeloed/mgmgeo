/*
 * Created by SharpDevelop.
 * User: bcristiano
 * Date: 30/06/2016
 * Time: 10:52
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SpaceEyeTools;

namespace MyGeocachingManager.Geocaching
{
	/// <summary>
	/// Holds Geocaching constants.
	/// </summary>
	public class GeocachingConstants
	{
		Dictionary<String, CheckBox> _dicoCheckBoxTypes = null;
		Dictionary<String, CheckBox> _dicoCheckBoxSize = null;
		Dictionary<String, String> _dicoCategoryType = null;
		Dictionary<String, String> _dicoCategoryTypeSmall = null;
		Dictionary<String, KeyValuePair<int, String>> _dicoTypeValueValueO = null;
		Dictionary<String, Tuple<String, String, bool>> _dicoWaypointTranskeyTypePrefixOk = null;

		MainWindow _daddy = null;

        /// <summary>
        /// 
        /// </summary>
        public static String _FalseDatePattern = "yyyy-MM-ddT00:00:01Z";

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        public GeocachingConstants(MainWindow daddy)
		{
			_daddy = daddy;
			
			// On initialise ce bordel
			_dicoTypeValueValueO = new Dictionary<string, KeyValuePair<int, string>>();
			int[] values = { 9, 3, 0, 1, 7, 2, 8, 10};
			String[] valueso = { "", "5.0", "2.0", "3.0", "", "4.0", "", ""};
        	
        	int i = 0;
    		foreach(String s in GetSupportedCacheSize())
    		{
    			_dicoTypeValueValueO.Add(s, new KeyValuePair<int, string>(values[i], valueso[i]));
    			i++;
    		}
    		// Pour cette saleté d'Opencaching
    		_dicoTypeValueValueO.Add("Unknown", new KeyValuePair<int, string>(values[i], valueso[i]));
    		
    		
    		_dicoWaypointTranskeyTypePrefixOk = new Dictionary<String, Tuple<String, String, bool>>();
    		i = 0;
    		String[] valuespf = { "FL", "IN" , "PK", "PS", "QA", "RP", "SM", "TH", "VS", "VI" };
    		bool[]   valuesok = { true, false, true, true, true, true, true, true, true, false};
    		foreach(String s in GetSupportedWaypointsType())
    		{
				String keytranslng = "WptType" + s.Replace(" ",""); // La clé WptType[typewaypointwithoutspace]
				_dicoWaypointTranskeyTypePrefixOk.Add(keytranslng, new Tuple<String, String, bool>(s, valuespf[i], valuesok[i]));
				i++;
    		}
		}
		
		/// <summary>
        /// Return list of supported cache types
        /// </summary>
        /// <returns>list of supported cache types</returns>
        static public List<String> GetSupportedCacheTypes()
        {
            List<String> lstypes = new List<string>();
            lstypes.Add("Traditional Cache");
            lstypes.Add("Multi-cache");
            lstypes.Add("Unknown Cache");
            lstypes.Add("Virtual Cache");
            lstypes.Add("Earthcache");
            lstypes.Add("Wherigo Cache");
            lstypes.Add("Webcam Cache");
            lstypes.Add("Event Cache");
            lstypes.Add("Mega-Event Cache");
            lstypes.Add("Giga-Event Cache");
            lstypes.Add("Letterbox Hybrid");
            lstypes.Add("Cache In Trash Out Event");
            lstypes.Add("Project APE Cache");
            lstypes.Add("Lab Cache");
            lstypes.Add("GPS Adventures Exhibit");
            return lstypes;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static public bool CheckIfCoordinatesCanBemodifiedOnGC(String type)
        {
        	return true;// Maintenant tout type de cache autorisé // ((type == "Multi-cache")||(type == "Unknown Cache"));
        }
        
        
        /// <summary>
        /// Return list of supported cache size
        /// </summary>
        /// <returns>list of supported cache size</returns>
        static public List<String> GetSupportedCacheSize()
        {
            List<String> lscontainers = new List<string>();
            lscontainers.Add("Not chosen"); // 9 
            lscontainers.Add("Large"); // 3 5.0
            lscontainers.Add("Micro"); // 0 2.0
            lscontainers.Add("Small"); // 1 3.0
            lscontainers.Add("Other"); // 7  
            lscontainers.Add("Regular"); // 2 4.0
            lscontainers.Add("Virtual"); // 8 
            // Unknown 10 
            return lscontainers;
        }

        
        /// <summary>
        /// Return dictionary Cache Type / Associated Checkbox
        /// </summary>
        /// <returns>Return dictionary Cache Type / Associated Checkbox</returns>
        public Dictionary<String, CheckBox> GetDicoTypeCheckbox()
        {
        	if (_dicoCheckBoxTypes == null)
        	{
        		_dicoCheckBoxTypes = new Dictionary<String, CheckBox>();
        		CheckBox[] cbs = { 
        			_daddy.cbType5, _daddy.cbType4, _daddy.cbType6, _daddy.cbType7, _daddy.cbType1,
        			_daddy.cbType9, _daddy.cbType8, _daddy.cbType2, _daddy.cbType10, _daddy.cbType13,
        			_daddy.cbType3, _daddy.cbType11, _daddy.cbType12, _daddy.cbType14, _daddy.cbType15 };
        	
	        	int i = 0;
        		foreach(String s in GetSupportedCacheTypes())
        		{
	        		_dicoCheckBoxTypes.Add(s, cbs[i]);
        			i++;
        		}
        	}
        	return _dicoCheckBoxTypes;
        }
        
       	/// <summary>
        /// Return dictionary Cache Type / Associated Checkbox
        /// </summary>
        /// <returns>Return dictionary Cache Type / Associated Checkbox</returns>
        public Dictionary<String, CheckBox> GetDicoSizeCheckbox()
        {
        	if (_dicoCheckBoxSize == null)
        	{
        		_dicoCheckBoxSize = new Dictionary<String, CheckBox>();
        		CheckBox[] cbs = { 
        			_daddy.cbSizeU, _daddy.cbSizeL, _daddy.cbSizeM, _daddy.cbSizeS, _daddy.cbSizeO,
        			_daddy.cbSizeR, _daddy.cbSizeV };
        	
	        	int i = 0;
        		foreach(String s in GetSupportedCacheSize())
        		{
	        		_dicoCheckBoxSize.Add(s, cbs[i]);
        			i++;
        		}
        	}
        	return _dicoCheckBoxSize;
        }
           
        /// <summary>
        /// Return dictionary Cache Type / Associated category
        /// </summary>
        /// <returns>Return dictionary Cache Type / Associated category</returns>
        public Dictionary<String, String> GetDicoTypeCategory()
        {
        	if (_dicoCategoryType == null)
        	{
        		_dicoCategoryType = new Dictionary<String, String>();
        		String[] cats = { 
        			"cat2", "cat3", "cat8", "cat4", "cat4",
					"cat8", "cat4", "cat6", "cat6", "cat6",
					"cat2", "cat6", "cat2", "cat6", "cat6",
        		};
        	
	        	int i = 0;
        		foreach(String s in GetSupportedCacheTypes())
        		{
	        		_dicoCategoryType.Add(s, cats[i]);
        			i++;
        		}
        	}
        	return _dicoCategoryType;
        }
        
        /// <summary>
        /// Return dictionary Cache Type / Associated small icon ~13x13
        /// </summary>
        /// <returns>Return dictionary Cache Type / Associated small icon</returns>
        public Dictionary<String, String> GetDicoTypeSmallIcon()
        {
        	if (_dicoCategoryTypeSmall == null)
        	{
        		_dicoCategoryTypeSmall = new Dictionary<String, String>();
        		String[] cats = { 
        			"hct2", "hct3", "hct8", "hct4", "hct13",
					"hct16", "hct11", "hct6", "hct14", "hct15",
					"hct10", "hct12", "hct2", "hct6", "hct6",
        		};
        	
	        	int i = 0;
        		foreach(String s in GetSupportedCacheTypes())
        		{
	        		_dicoCategoryTypeSmall.Add(s, cats[i]);
        			i++;
        		}
        	}
        	return _dicoCategoryTypeSmall;
        }
        
        /// <summary>
        /// Convert container to int
        /// </summary>
        /// <param name="container">container name</param>
        /// <returns>int</returns>
        public int ConvertContainerToInt(String container)
        {
        	if (_dicoTypeValueValueO.ContainsKey(container))
        		return _dicoTypeValueValueO[container].Key;
        	else
        		return 0;
        }

        /// <summary>
        /// Perform cache container conversion from Geocaching to Opencaching
        /// </summary>
        /// <param name="container">container geocaching size</param>
        /// <returns>Opencaching size</returns>
        public String ConvertContainerToOpenCachingValues(String container)
        {
        	if (_dicoTypeValueValueO.ContainsKey(container))
        		return _dicoTypeValueValueO[container].Value;
        	else
        		return "0.0";
        }

        /// <summary>
        /// Convert container int (string format) to string name
        /// </summary>
        /// <param name="container">int value</param>
        /// <returns>container name</returns>
        public String ConvertContainerToString(string container)
        {
        	int i = -1;
        	if (Int32.TryParse(container, out i))
        	{
        		foreach(KeyValuePair<String, KeyValuePair<int, String>> pair in _dicoTypeValueValueO)
        		{
        			if (pair.Value.Key == i)
        				return pair.Key;
        		}
        	}
        	return "Not chosen";
        }
        
        /// <summary>
        /// Return list of supported waypoints types
        /// </summary>
        /// <returns>list of supported waypoints types</returns>
        static public List<String> GetSupportedWaypointsType()
        {
            List<String> lswpttypes = new List<string>();
            lswpttypes.Add("Final Location");
			lswpttypes.Add("Invisible");
			lswpttypes.Add("Parking Area");
			lswpttypes.Add("Physical Stage");
			lswpttypes.Add("Question to Answer");
			lswpttypes.Add("Reference Point");
			lswpttypes.Add("Stages of a Multicache");
			lswpttypes.Add("Trailhead");
			lswpttypes.Add("Virtual Stage");
			lswpttypes.Add("Visible");
             
            return lswpttypes;
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lsttype"></param>
        public void CreateListOfWaypointTypes(List<String> lsttype)
        {
        	foreach(KeyValuePair<String, Tuple<String, String, bool>> pair in _dicoWaypointTranskeyTypePrefixOk)
        	{
        		if (pair.Value.Item3)
        			lsttype.Add(_daddy.GetTranslator().GetString(pair.Key));
        	}
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sType"></param>
        /// <param name="sWptType"></param>
        /// <param name="sWptTypePrefix"></param>
        public void RetrieveWaypointTypeAndPrefix(String sType, ref String sWptType, ref String sWptTypePrefix)
        {
        	foreach(KeyValuePair<String, Tuple<String, String, bool>> pair in _dicoWaypointTranskeyTypePrefixOk)
        	{
        		if ((sType ==  _daddy.GetTranslator().GetString(pair.Key)) && pair.Value.Item3)
        		{
        			sWptType = pair.Value.Item1;
                	sWptTypePrefix = pair.Value.Item2;
                	return;
        		}
        	}

        }
	}
}
