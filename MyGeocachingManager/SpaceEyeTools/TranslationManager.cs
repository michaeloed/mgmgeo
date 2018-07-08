using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace SpaceEyeTools
{
    /// <summary>
    /// Class to manage available translations for MGM
    /// </summary>
    public class TranslationManager
    {
        Dictionary<String, String> _dicTranslations = null;
        String _locale = "";
        String _name = "";
        String _ename = "";

        /// <summary>
        /// Get all available keys for translations
        /// </summary>
        public List<String> Keys
        {
            get
            {
                return new List<string>(_dicTranslations.Keys);
            }
        }

        /// <summary>
        /// Get current language locale
        /// </summary>
        public string Locale
        {
            get
            {
                return _locale;
            }
        }

        /// <summary>
        /// Get current language name in native language
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Get current language name in english
        /// </summary>
        public string EnglishName
        {
            get
            {
                return _ename;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filename">translation language file</param>
        /// <param name="bToLower">if true, keys will be lowered</param>
        public TranslationManager(String filename, bool bToLower)
        {
            _dicTranslations = new Dictionary<string, string>();
            String key = "";
            String val = "";
            try
            {
                String line;

                // Read the file and display it line by line.
                System.IO.StreamReader file =
                   new System.IO.StreamReader(filename, System.Text.Encoding.Default, true);

                _name = file.ReadLine();
                _ename = file.ReadLine();
                _locale = file.ReadLine();
                while ((line = file.ReadLine()) != null)
                {
                    int pos = line.IndexOf("=");
                    if (pos != -1)
                    {
                        key = line.Substring(0, pos);
                        if (bToLower)
                            key = key.ToLower(); // Robustness : tout sera en minuscules
                        val = line.Substring(pos+1);
                        _dicTranslations.Add(key, val);
                    }
                }

                file.Close();
            }
            catch (Exception exc)
            {
                Exception e2 = new Exception(exc.Message + " [key,val] = [" + key + "," + val + "]");
                throw e2;
            }
        }

        /// <summary>
        /// Retrieve language information from a language file
        /// </summary>
        /// <param name="filename">Language file</param>
        /// <param name="name">Language name in target language (i.e. Français)</param>
        /// <param name="ename">Language name in English (i.e. French)</param>
        /// <param name="locale">Language locale (i.e. fr-FR)</param>
        public static void GetInfo(String filename, ref String name, ref String ename, ref String locale)
        {
            // Read the file and display it line by line.
            System.IO.StreamReader file =
               new System.IO.StreamReader(filename, System.Text.Encoding.Default, true);

            name = file.ReadLine();
            ename = file.ReadLine();
            locale = file.ReadLine();
            
            file.Close();
        }

        /// <summary>
        /// Get a translation from a key (key will be lowered)
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>associated translation for the key</returns>
        public String GetString(String key)
        {
            key = key.ToLower(); // Robustness : tout sera en minuscules
            if ((_dicTranslations != null) && (_dicTranslations.ContainsKey(key)))
                return _dicTranslations[key];
            else
                return "#" + key + "#"; // instead of "###"
        }

        /// <summary>
        /// Get a translation from a key (key will be lowered) and replace # with a return \r\n
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>associated translation for the key</returns>
        public String GetStringM(String key)
        {
            key = key.ToLower(); // Robustness : tout sera en minuscules
            if ((_dicTranslations != null) && (_dicTranslations.ContainsKey(key)))
            	return _dicTranslations[key].Replace("#","\r\n");
            else
                return "#" + key + "#"; // instead of "###"
        }
        
        /// <summary>
        /// Get a translation from a key (key will be lowered) and replace # with a return \r\n
        /// THIS IS CASE SENSITIVE!
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>associated translation for the key or null if not found!</returns>
        public String GetStringMV(String key)
        {
            if ((_dicTranslations != null) && (_dicTranslations.ContainsKey(key)))
            	return _dicTranslations[key].Replace("#","\r\n");
            else
            	return null;
        }
        
        /// <summary>
        /// Get a translation from a key (key will ***NOT*** be lowered)
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>associated translation for the key</returns>
        public String GetStringU(String key)
        {
            if ((_dicTranslations != null) && (_dicTranslations.ContainsKey(key)))
                return _dicTranslations[key];
            else
                return "#" + key + "#"; // instead of "###"
        }
    }
}
