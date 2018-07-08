using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyGeocachingManager
{
    /// <summary>
    /// Legacy plugin interface.
    /// Provides low functionnalities.
    /// Deprecated.
    /// </summary>
    public interface IScript
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
        /// Called when the plugin is initialized
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        void Initialize(MainWindow daddy);
        
        /// <summary>
        /// Method that is called when the plugin is click in MGM
        /// </summary>
        /// <returns>Returns true if command executed without any exception (exceptions are caught)</returns>
        bool DoIt();
        
        /// <summary>
        /// Called when the plugin is closed (disposed)
        /// </summary>
        void Close();
    }
}
