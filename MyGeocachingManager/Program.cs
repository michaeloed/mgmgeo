using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;
using System.IO;
using SpaceEyeTools;
using SpaceEyeTools.HMI;
using System.Drawing;

namespace MyGeocachingManager
{
    /// <summary>
    /// Main entry point for the application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Exception exc = new Exception();
                if (!MyTools.CheckWriteAccess(Path.GetDirectoryName(Application.ExecutablePath), ref exc))
                {
                    String msg = "";
                    if (Thread.CurrentThread.CurrentUICulture.Name.StartsWith("fr"))
        			{
                    	msg += "nMyGeocachingManager (MGM) doit être installé dans un répertoire avec des droits d'accès en écriture afin de créer ses fichiers temporaires et sa base de données interne. Il semble que vous ayez installé MGM dans un répertoire où les droits d'accès en écriture ne sont pas autorisés. Merci d'installer MGM dans un autre endroit avec les droits requis ou exécutez MGM avec des droits administrateur (non recommandé car non nécessaire à son bon fonctionnement).\r\n";
	                    msg += "*******************************\r\n";
	                    msg += "[ERREUR] : \r\n" + exc.Message;
	                    MyMessageBox.Show(msg, "Erreur", MessageBoxIcon.Error, null);
                    }
                    else
                    {
                    	msg += "\r\nMyGeocachingManager (MGM) must be installed in a folder with write access in order to create temporary files and its internal database. It appears that you installed MGM in a location where write access is denied. Please install it on another location with requested rights or execute MGM with Administrative rights (not recommended since not needed to function properly).\r\n";
	                    msg += "*******************************\r\n";
	                    msg += "[ERREUR] : \r\n" + exc.Message;
	                    MyMessageBox.Show(msg, "Error / Erreur", MessageBoxIcon.Error, null);
                    }
                    return;
                }

                String dotnet = MyTools.GetHighestInstalledFramework();
                if (!MyTools.VerifyMinFrameworkVersion(dotnet))
	            {
	            	return;
	            }
			      
                // Ok tout va bien, on démarre
                // cela peut prendre des plombes !
                MainWindow wnd = new MainWindow(dotnet);
                
                // Si on force close, on s'arrête là
                if (wnd._bForceClose)
                	return;
                
                // Go !!!
                Application.Run(wnd);
            }
            catch (Exception ex)
            {
            	MyMessageBox.Show(MainWindow.GetException("FATAL ERROR! Program will close, exception caught in Program",ex), "FATAL ERROR", MessageBoxIcon.Error, null);
                
            }
        }
        
        
    }
}
