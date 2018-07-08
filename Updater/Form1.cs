using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Timers;
using System.Xml;
using System.Diagnostics;

namespace Updater
{
    public partial class Form1 : Form
    {
        List<String> fileToIgnore = new List<string>();
        String myName = "";
        private static System.Timers.Timer aTimer;
        System.IO.StreamWriter _log = null;
        
        private const int CP_NOCLOSE_BUTTON = 0x200;
        
        /// <summary>
        /// Set a text in textBox1 and scroll bottom
        /// </summary>
        public String textBox1Text
        {
            set
            {
            	if (_log != null)
            	{
            		_log.Write(value);
            		_log.Flush();
            	}
                textBox1.Text += value;
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.ScrollToCaret();
            }
            get
            {
                return textBox1.Text;
            }
        }
        
        protected override CreateParams CreateParams
        {
            get
            {
            	const int WS_EX_COMPOSITED = 0x02000000;
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                myCp.ExStyle |= WS_EX_COMPOSITED;
                return myCp;
            }
        }

        public void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            try
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
            catch (Exception ex)
            {
                textBox1Text =  Environment.NewLine + ex.Message;
            }
        }

        private bool CopyFolderContents(string SourcePath, string DestinationPath)
        {
            SourcePath = SourcePath.EndsWith(@"\") ? SourcePath : SourcePath + @"\";
            DestinationPath = DestinationPath.EndsWith(@"\") ? DestinationPath : DestinationPath + @"\";

            try
            {
                if (Directory.Exists(SourcePath))
                {
                    if (Directory.Exists(DestinationPath) == false)
                    {
                        Directory.CreateDirectory(DestinationPath);
                    }

                    foreach (string files in Directory.GetFiles(SourcePath))
                    {
                        FileInfo fileInfo = new FileInfo(files);
                        if ((fileToIgnore.Contains(fileInfo.Name) == false) && (fileInfo.Name != myName))
                        {
                            fileInfo.CopyTo(string.Format(@"{0}\{1}", DestinationPath, fileInfo.Name), true);
                            textBox1Text =  files + Environment.NewLine;
                        }
                        else
                        {
                            // Ignore
                            textBox1Text =  "*** IGNORED FILE: " + files + Environment.NewLine;
                        }
                    }

                    foreach (string drs in Directory.GetDirectories(SourcePath))
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(drs);
                        if (CopyFolderContents(drs, DestinationPath + directoryInfo.Name) == false)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                textBox1Text =  Environment.NewLine + ex.Message;
                return false;
            }
        }

        public Form1()
        {
            InitializeComponent();

            // Create a timer
            aTimer = new System.Timers.Timer(100);
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Enabled = true;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            aTimer.Enabled = false;
            DoTheJob();
        }

        private void ProcessUpdateXML(string updpath, string exePath)
        {
            String updatexml = updpath + "\\update.xml";
            if (File.Exists(updatexml))
            {
            	// Lecture du fichier d'ordre
                textBox1Text =  "*** " + updatexml + " found!" + Environment.NewLine;
                XmlDocument _xmldoc;
                XmlNodeList _xmlnode;
                _xmldoc = new XmlDocument();
                _xmldoc.Load(updatexml);
                _xmlnode = _xmldoc.GetElementsByTagName("Config");
                string destination = exePath;
				progressBar1.Value++;
				
                // DirToCopy
                // *********
                XmlNode dirs = _xmlnode[0].ChildNodes.Item(0);
                foreach (XmlNode elt in dirs.ChildNodes)
                {
                    String src = exePath + "/" + elt.Attributes[0].InnerText.Trim();
                    String dst = elt.Attributes[1].InnerText.Trim();
                    if (dst == "")
                        dst = destination;
                    else
                        dst = destination + "/" + dst;
                    textBox1Text =  "DirToCopy - src: " + src + ", dst: " + dst + Environment.NewLine;
                    CopyAll(new DirectoryInfo(src), new DirectoryInfo(dst));
                }
                progressBar1.Value++;

                // FileToCopy
                // **********
                XmlNode files = _xmlnode[0].ChildNodes.Item(1);
                foreach (XmlNode elt in files.ChildNodes)
                {
                    String src = exePath + "/" + elt.Attributes[0].InnerText.Trim();
                    String dst = elt.Attributes[1].InnerText.Trim();
                    if (dst == "")
                        dst = destination;
                    else
                        dst = destination + "/" + dst;
                    textBox1Text =  "FileToCopy - src: " + src + ", dst: " + dst + Environment.NewLine;
                    try
                    {
                        File.Copy(src, dst);
                    }
                    catch (Exception ex)
                    {
                        textBox1Text =  Environment.NewLine + ex.Message;
                    }
                }
				progressBar1.Value++;
				
                // DirToRemove
                // ***********
                dirs = _xmlnode[0].ChildNodes.Item(2);
                foreach (XmlNode elt in dirs.ChildNodes)
                {
                    String src = exePath + "/" + elt.Attributes[0].InnerText.Trim();
                    textBox1Text =  "DirToRemove - src: " + src + Environment.NewLine;
                    try
                    {
                        if (Directory.Exists(src))
                            DeleteDirectory(src, true);
                        else
                        {
                            textBox1Text =  "*** NOT FOUND" + Environment.NewLine;
                        }
                    }
                    catch (Exception ex)
                    {
                        textBox1Text =  Environment.NewLine + ex.Message;
                    }
                }
				progressBar1.Value++;
				
                // FileToRemove
                // ************
                files = _xmlnode[0].ChildNodes.Item(3);
                foreach (XmlNode elt in files.ChildNodes)
                {
                    String src = exePath + "/" + elt.Attributes[0].InnerText.Trim();
                    textBox1Text =  "FileToRemove - src: " + src + Environment.NewLine;
                    try
                    {
                        if (File.Exists(src))
                            File.Delete(src);
                        else
                        {
                            textBox1Text =  "*** NOT FOUND" + Environment.NewLine;
                        }
                    }
                    catch (Exception ex)
                    {
                        textBox1Text =  Environment.NewLine + ex.Message;
                    }
                }
				progressBar1.Value++;

                // FileToCopy32
                // ************
                files = _xmlnode[0].ChildNodes.Item(4);
                foreach (XmlNode elt in files.ChildNodes)
                {
                    String src = exePath + "/" + elt.Attributes[0].InnerText.Trim();
                    String dst = elt.Attributes[1].InnerText.Trim();
                    if (dst == "")
                        dst = destination;
                    else
                        dst = destination + "/" + dst;
                    textBox1Text = "FileToCopy (32 bits only) - src: " + src + ", dst: " + dst + Environment.NewLine;
                    try
                    {
                        File.Copy(src, dst);
                    }
                    catch (Exception ex)
                    {
                        textBox1Text = Environment.NewLine + ex.Message;
                    }
                }
                progressBar1.Value++;

                // FileToCopy64
                // ************
                files = _xmlnode[0].ChildNodes.Item(5);
                foreach (XmlNode elt in files.ChildNodes)
                {
                    String src = exePath + "/" + elt.Attributes[0].InnerText.Trim();
                    String dst = elt.Attributes[1].InnerText.Trim();
                    if (dst == "")
                        dst = destination;
                    else
                        dst = destination + "/" + dst;
                    textBox1Text = "FileToCopy (64 bits only) - src: " + src + ", dst: " + dst + Environment.NewLine;
                    try
                    {
                        File.Copy(src, dst);
                    }
                    catch (Exception ex)
                    {
                        textBox1Text = Environment.NewLine + ex.Message;
                    }
                }
                progressBar1.Value++;
                
                // FileToMove
                // ************
                files = _xmlnode[0].ChildNodes.Item(6);
                foreach (XmlNode elt in files.ChildNodes)
                {
                    String src = exePath + "/" + elt.Attributes[0].InnerText.Trim();
                    String dst = elt.Attributes[1].InnerText.Trim();
                    dst = destination + "/" + dst;
                    textBox1Text = "FileToMove - src: " + src + ", dst: " + dst + Environment.NewLine;
                    try
                    {
                    	if (File.Exists(src))
                    	{
                    		// Efface l'ancien
                    		if (File.Exists(dst))
                    			File.Delete(dst);
                    	
							// Déplace                    		
                        	File.Move(src, dst);
                    	}
                    }
                    catch (Exception ex)
                    {
                        textBox1Text = Environment.NewLine + ex.Message;
                    }
                }
                progressBar1.Value++;
                
                // DirToMove
                // ************
                files = _xmlnode[0].ChildNodes.Item(7);
                foreach (XmlNode elt in files.ChildNodes)
                {
                    String src = exePath + "/" + elt.Attributes[0].InnerText.Trim();
                    String dst = elt.Attributes[1].InnerText.Trim();
                    dst = destination + "/" + dst;
                    textBox1Text = "DirToMove - src: " + src + ", dst: " + dst + Environment.NewLine;
                    try
                    {
                    	if (Directory.Exists(src))
                    	{
                    		// Efface l'ancien
                    		if (Directory.Exists(dst))
                    			DeleteDirectory(dst, true);
                    	
							// Déplace                    		
                        	Directory.Move(src, dst);
                    	}
                    }
                    catch (Exception ex)
                    {
                        textBox1Text = Environment.NewLine + ex.Message;
                    }
                }
                progressBar1.Value++;
                
                File.Delete(updatexml);
            }
            else
            {
                textBox1Text =  "*** " + updatexml + " not found!" + Environment.NewLine;
                throw new Exception("*** ABORTING UPDATE ***");
            }
        }

        private bool IsExeAlive(String fileNameToFilter)
        {
        	foreach (Process p in Process.GetProcesses())
			{
        		// Selon nos droits, certains exe ne seront pas accessibles...
        		try
        		{
				   string fileName = Path.GetFullPath(p.MainModule.FileName);
				
				   //check for equality (case insensitive)
				   if (string.Compare(fileNameToFilter, fileName, true) == 0)
				   {
				      //matching...
				      return true;
				   }
        		}
        		catch(Exception)
        		{
        			
        		}
			}
        	return false;
        }
        
        private void WaitForDeath(String exe)
        {
        	string fileNameToFilter = Path.GetFullPath(exe);
        	textBox1Text =  "Waiting for death of " + fileNameToFilter + Environment.NewLine;
        	while (IsExeAlive(fileNameToFilter))
        	{
        		textBox1Text =  "  => " + exe + " is running, please wait or kill the process with Task Manager..." + Environment.NewLine;
        		System.Threading.Thread.Sleep(5000);
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
        
        private void DoTheJob()
        {
            try
            {
                _log = new System.IO.StreamWriter(Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + "update.log", true);
                string[] args = Environment.GetCommandLineArgs();
                if (args.Count() == 5) // 1st arg is executable name !
                {
                	textBox1Text =  "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~" + Environment.NewLine;
                    textBox1Text =  "~~~ UPDATE INITIALIZING - DO NOT CLOSE THIS PROGRAM  ~~~" + Environment.NewLine;
                    textBox1Text =  "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~" + Environment.NewLine;
                    
                    String update = args[1];
                    String exe = args[2];
                    String config = args[3];
                    String readme = args[4];
                    myName = System.AppDomain.CurrentDomain.FriendlyName;
                    textBox1Text =  "Updater executable: " + myName + Environment.NewLine;
                    textBox1Text =  "Updater version: 2.1" + Environment.NewLine;
                    textBox1Text =  "Update file: " + update + Environment.NewLine;
                    textBox1Text =  "Executable file: " + exe + Environment.NewLine;
                    textBox1Text =  "Config file: " + config + Environment.NewLine;
                    textBox1Text =  "Readme file: " + readme + Environment.NewLine;

                    
                    
                    // LE PROGRESS BAR !!!
                    // Il y aura ces étapes
                    //  l'attente de la mort de l'exe
                    //  la copie
                    //  le traitement du XML : 5 étapes
                    //  le nettoyage
					//  le readme
					//  le lancement de l'application
					progressBar1.Maximum = 14;
                    
                    // Copy directory
                    fileToIgnore.Add(config);
                    fileToIgnore.Add("update.xml");
                    String exePath = Path.GetDirectoryName(Application.ExecutablePath);
                    String updpath = exePath + "\\Update\\" + update;
                    if (!File.Exists(updpath + "\\update.xml"))
                    {
                    	textBox1Text = updpath + " does not exists" + Environment.NewLine;
                    	// Robustesse, on est sûrement une RC
                    	// Check si on est une RC
	                    // 4.0.0.0.MyGeocachingManager
	                    // ou
	                    // 4.0.0.0.RC01.MyGeocachingManager
	                    String[] vers = update.Split('.');
	                    if (vers.Count() == 6)
	                    {
	                    	// On vire le RC, c'est peut être ça qui bloque
	                    	update = vers[0] + "." + vers[1] + "." + vers[2] + "." + vers[3] + "." + vers[5];
	                    	updpath = exePath + "\\Update\\" + update;
	                    	textBox1Text = "Checking " + updpath + Environment.NewLine;
	                    }
                    }
                    
                    textBox1Text =  "Copy update" + Environment.NewLine + "  From: " + updpath + Environment.NewLine + "  To:   " + exePath + Environment.NewLine;
                    textBox1Text =  "*******************************************************" + Environment.NewLine;
                    textBox1Text =  "*** UPDATE IN PROGRESS - DO NOT CLOSE THIS PROGRAM  ***" + Environment.NewLine;
                    textBox1Text =  "*******************************************************" + Environment.NewLine;

                    // Wait for the death the previous exe
                    // *********************************************
                    WaitForDeath(exe);
                    progressBar1.Value++;
                    
                    // La copie des dossiers
                    // *********************
                    if (CopyFolderContents(updpath, exePath))
                    {
                    	progressBar1.Value++;
                    	
                    	// Le traitement du XML
                    	// ********************
                        ProcessUpdateXML(updpath, exePath);

                        // Delete update directory
                        // ***********************
                        DeleteDirectory(updpath, true);
						progressBar1.Value++;
						
                        // Open readme
                        // ***********
                        System.Diagnostics.Process.Start(readme);
						progressBar1.Value++;
						
						textBox1Text =  Environment.NewLine;
			            textBox1Text =  "*******************************************************" + Environment.NewLine;
			            textBox1Text =  "*** UPDATE SUCCESSFUL - LAUNCHING APPLICATION       ***" + Environment.NewLine;
			            textBox1Text =  "*******************************************************" + Environment.NewLine;
			            System.Threading.Thread.Sleep(5000);
			            
                        // Launch application
                        // ******************
                        System.Diagnostics.Process.Start(exe);
						progressBar1.Value++;
						
                        // Exit
                        this.Close();
                    }
                }
                else if (args.Count() == 2)
                {
                	String keyword = args[1];
                	if (keyword == "KILL")
                	{
                		textBox1Text =  "Kill switch starting...";
                		// On supprime les binaires
                		String exePath = Path.GetDirectoryName(Application.ExecutablePath);
                		DirectoryInfo dir = new DirectoryInfo(exePath);
                		List<String> files = new List<string>();
                		foreach(var f in dir.GetFiles("*.dll"))
                		{
                			files.Add(exePath + "\\" + f.Name);
                		}
                		foreach(var f in dir.GetFiles("*.exe"))
                		{
                			if (f.Name != "Updater.exe")
                				files.Add(exePath + "\\" + f.Name);
                		}
                		
                		// On attend si besoin
                		WaitForDeath("MyGeocachingManager.exe");
                		
                		// On supprime
                		foreach(var f in files)
                		{
                			try
                			{
                				textBox1Text = "Suppression de " + f;
                				File.Delete(f);
                			}
                			catch(Exception)
                			{
                				
                			}
                		}
                		
                		textBox1Text =  "R.I.P.";
                		System.Threading.Thread.Sleep(5000);
                		
                		// Exit
                        this.Close();
                	}
                	else
                	{
                		textBox1Text =  "Invalid arguments: " + Environment.NewLine;
	                    for (int i = 1; i < args.Count(); i++)
	                    {
	                        String f = args[i];
	                        textBox1Text =  f + Environment.NewLine;
	                    }
                	}
                }
                else
                {
                    textBox1Text =  "Invalid arguments: " + Environment.NewLine;
                    for (int i = 1; i < args.Count(); i++)
                    {
                        String f = args[i];
                        textBox1Text =  f + Environment.NewLine;
                    }
                }
                _log.Close();
            }
            catch (Exception exc)
            {

                textBox1Text =  Environment.NewLine + exc.Message;
            }

            textBox1Text =  Environment.NewLine;
            textBox1Text =  "*******************************************************" + Environment.NewLine;
            textBox1Text =  "*** UPDATE FAILED - CLOSING WINDOW IN 10 SECONDS    ***" + Environment.NewLine;
            textBox1Text =  "*******************************************************" + Environment.NewLine;
            System.Threading.Thread.Sleep(10000);
            this.Close();

        }
    }
}
