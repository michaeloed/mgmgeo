using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.IO.Compression;

namespace InstallationDirCreator
{
    class Program
    {
        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
	            Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it’s new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
	            Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
	            fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
	            DirectoryInfo nextTargetSubDir =
		            target.CreateSubdirectory(diSourceSubDir.Name);
	            CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        /// <summary>
        /// Delete (really) a directory, even if not empty
        /// </summary>
        /// <param name="path">path to delete</param>
        /// <param name="recursive">// Delete all files from the folder 'path', but keep all sub-folders and its files</param>
        public static void DeleteDirectory(string path, bool recursive)
		{
		    // Delete all files and sub-folders?
		    if (recursive)
		    {
		        // Yep... Let's do this
		        var subfolders = Directory.GetDirectories(path);
		        foreach (var s in subfolders)
		        {
		            DeleteDirectory(s, recursive);
		        }
		    }
		 
		    // Get all files of the folder
		    var files = Directory.GetFiles(path);
		    foreach (var f in files)
		    {
		        // Get the attributes of the file
		        var attr = File.GetAttributes(f);
		 
		        // Is this file marked as 'read-only'?
		        if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
		        {
		            // Yes... Remove the 'read-only' attribute, then
		            File.SetAttributes(f, attr ^ FileAttributes.ReadOnly);
		        }
		 
		        // Delete the file
		        File.Delete(f);
		    }
		 
		    // When we get here, all the files of the folder were
		    // already deleted, so we just delete the empty folder
		    Directory.Delete(path);
		}
        
        static void Main(string[] args)
        {
        	try
        	{
	            String vernum = "";
	            XmlDocument _xmldoc;
	            XmlNodeList _xmlnode;
	            _xmldoc = new XmlDocument();
	            _xmldoc.Load("Installation.xml");
	            _xmlnode = _xmldoc.GetElementsByTagName("Config");
	
	            String subversion = "";
	            if ((args != null) && (args.Length > 0))
	            {
	            	subversion = args[0] + ".";
	            	Console.WriteLine("Subversion: " + subversion);
	            }
	            
	            // Name
	            String name = _xmlnode[0].ChildNodes.Item(0).InnerText.Trim();
	            Console.WriteLine("Name: " + name);
	
	            // ExeVersion
	            String exeversion = _xmlnode[0].ChildNodes.Item(1).InnerText.Trim();
	            Console.WriteLine("ExeVersion: " + exeversion);
	            if (exeversion != "")
	            {
	                vernum = System.Reflection.AssemblyName.GetAssemblyName(exeversion).Version.ToString();
	                Console.WriteLine("==> Version number: " + vernum);
	            }
	
	            // Version
	            String version = _xmlnode[0].ChildNodes.Item(2).InnerText.Trim();
	            Console.WriteLine("Version: " + version);
	            if (version != "")
	            {
	                vernum = version;
	                Console.WriteLine("==> Version number: " + vernum);
	            }
	
	            // Create destination dir
	            String destination = "";
	            //if (subversion == "")
	            	destination = vernum + "." + name ;
	            //else
	            //	destination = vernum + "." + subversion + name ;
	            
	            if (Directory.Exists(destination))
	            {
	                Console.WriteLine("Destination directory " + destination + " exists, remove old one");
	                DeleteDirectory(destination, true);
	            }
	            Console.WriteLine("Create destination directory " + destination);
	            Directory.CreateDirectory(destination);
	
	
	            // DirToCopy
	            XmlNode dirs = _xmlnode[0].ChildNodes.Item(3);
	            foreach (XmlNode elt in dirs.ChildNodes)
	            {
	                String src = elt.Attributes[0].InnerText.Trim();
	                String dst = elt.Attributes[1].InnerText.Trim();
	                if (dst == "")
	                    dst = destination;
	                else
	                    dst = destination + "/" + dst;
	                Console.WriteLine("DirToCopy - src: " + src + ", dst: " + dst);
	                CopyAll(new DirectoryInfo(src), new DirectoryInfo(dst));
	            }
	
	            // FileToCopy
	            XmlNode files = _xmlnode[0].ChildNodes.Item(4);
	            foreach (XmlNode elt in files.ChildNodes)
	            {
	                String src = elt.Attributes[0].InnerText.Trim();
	                String dst = elt.Attributes[1].InnerText.Trim();
	                if (dst == "")
	                    dst = destination;
	                else
	                    dst = destination + "/" + dst;
	                Console.WriteLine("FileToCopy - src: " + src + ", dst: " + dst);
	                /*
	                try
	                {
	                    if (File.Exists(dst))
	                    {
	                        Console.WriteLine("*** delete " + dst);
	                        File.Delete(dst);
	                        Console.WriteLine("*** done!");
	                    }
	                    else
	                        Console.WriteLine("*** not existing " + dst);
	                }
	                catch (Exception ex)
	                {
	                    Console.WriteLine(ex.Message);
	                }
	                */
	                File.Copy(src, dst, true);
	            }
	
	            // Une commande avant la fin ?
	
	            Console.WriteLine("One last command?");
	
	            XmlNode eltexe = _xmlnode[0].ChildNodes.Item(5);
	
	            String execmd = eltexe.Attributes[0].InnerText;
	
	            String exeparams = eltexe.Attributes[1].InnerText;
	
	            Console.WriteLine("Last command before the end: " + execmd + " " + exeparams);
	
	            if (execmd != "")
	            {
	
	                String[] ps = exeparams.Split(';');
	
	                foreach (String p in ps)
	                {
	
	                    if ((p != null) && (p != ""))
	                    {
	
	                        // On se place dans le répertoire de destination
	
	                        //String before = Path.GetDirectoryName(Application.ExecutablePath);
	
	                        String before = Directory.GetCurrentDirectory();
	
	                        Directory.SetCurrentDirectory(destination);
	
	                        Console.WriteLine("--> Last command before the end: " + execmd + " " + p);
	
	                        var process = System.Diagnostics.Process.Start(execmd, p);
	
	                        process.WaitForExit();
	
	                        Directory.SetCurrentDirectory(before);
	
	                    }
	
	                }
	
	            }
	
	            // Version minimale pour forcer l'update
	            String versionup = _xmlnode[0].ChildNodes.Item(6).InnerText.Trim();
	            Console.WriteLine("VersionMinForUpdate: " + versionup);
	            if (versionup != "")
	            {
	                Console.WriteLine("==> Version number for update: " + versionup);
	            }
	
	            // Zip it?
	
	            try
	            {
	
	                String szip = _xmlnode[0].ChildNodes.Item(7).InnerText.Trim();
	
	                Console.WriteLine("Zip: " + szip);
	
	                if (szip == "True")
	                {
						String zipname = vernum + "." + subversion + name;
						String zipfile = zipname + ".zip";
						if (File.Exists(zipfile))
							File.Delete(zipfile);
						ZipFile.CreateFromDirectory(destination, zipfile, CompressionLevel.Optimal, true);
	
	                    // La même chose avec un mot de passe ?
	                    /*
	                    if (_xmlnode[0].ChildNodes.Count >= 9)
	                    {
	                        String szipassword = _xmlnode[0].ChildNodes.Item(8).InnerText.Trim();
	                        Console.WriteLine("Zip password: " + szipassword);
	                        using (ZipFile zip = new ZipFile())
	                        {
	                            // Le zip avec mon mot de passe
	                            zip.Encryption = EncryptionAlgorithm.WinZipAes256;
	                            zip.Password = szipassword;
	                            // add this map file into the "images" directory in the zip archive
	                            zip.AddDirectory(destination, destination);
	
	                            // Le zip password
	                            zip.Save(zipname + ".password.zip");
	                        }
	                    }
						*/
	                }
	
	            }
	
	            catch (Exception ex)
	            {
	
	                Console.WriteLine(ex.Message);
	
	            }
	
	
	            // Write version.ver
	            System.IO.StreamWriter ver = new System.IO.StreamWriter("version.ver", false);
	            if (versionup == "")
	                ver.WriteLine(vernum + "#");
	            else
	                ver.WriteLine(vernum + "#MINVER_" + versionup);
	            ver.Close();
        	}
        	catch(Exception excp)
			{
        		Console.WriteLine(excp.Message);
			}
        }
    }
}
