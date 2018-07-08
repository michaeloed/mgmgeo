using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyGeocachingManager
{
    /// <summary>
    /// New plugin interface providing much more finctionnalities
    /// With this one a plugin can declare several functions that will 
    /// create the same number of menu entries in MGM
    /// </summary>
    public interface IScriptV2
    {
        /// <summary>
        /// Get plugin name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Get plugin description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Get plugin version
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Get minimum version of MGM required for this plugin
        /// </summary>
        string MinVersionMGM { get; }
        
        /// <summary>
        /// Get list of plugin functions:
        /// Key: Function name (displayed in MGM menu entry)
        /// Value: Function name in source code
        /// </summary>
        Dictionary<String, String> Functions { get; }
        
        /// <summary>
        /// alled when the plugin is initialized
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        void Initialize(MainWindow daddy);
        
        /// <summary>
        /// alled when the plugin is closed (disposed)
        /// </summary>
        void Close();
    }

    /// <summary>
    /// Class associated to a specific IScriptV2 plugin function
    /// </summary>
    public class IScriptV2Function
    {

        /// <summary>
        /// Function name in source code
        /// </summary>
        public String fctName;

        /// <summary>
        /// Parent IScriptV2
        /// </summary>
        public IScriptV2 script;
    }
}
