using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Configuration;
using EXControls;
using System.Collections;
using System.Net;

namespace MyGeocachingManager
{
	public class WherigoDownloader : IScript
	{
		private MainWindow _Daddy = null;
		public string Name {get {return "Export GPX + Images for Explorist GC";}}
		public string Description { get {return "Create a GPX export of selected caches and export images visible on Magellan Explorist GC";}}
		
		public void Initialize(MainWindow daddy)
		{
			_Daddy = daddy;
		}
		
		private string ToBase64(Image image, ImageFormat format)
        {
          using (MemoryStream ms = new MemoryStream())
          {
            // Convert Image to byte[]
            image.Save(ms, format);
            byte[] imageBytes = ms.ToArray();

            // Convert byte[] to Base64 String
            string base64String = Convert.ToBase64String(imageBytes);
            return base64String;
          }
        }
		
		private String ExportExploristGC(String path)
        {
			Directory.SetCurrentDirectory(path);
			String imgMagellan = "MAGELLAN";
			if (Directory.Exists(imgMagellan))
			{
				try
				{
					Directory.Delete(imgMagellan, true);
				}
				catch(Exception) {}
			}
			String gpxmagellan = "MAGELLAN\\Geocaches";
			Directory.CreateDirectory(gpxmagellan);
			imgMagellan = "MAGELLAN\\Images\\Geocaches";
			Directory.CreateDirectory(imgMagellan);
			Directory.SetCurrentDirectory(imgMagellan);
			
			String fileRadix = path + "\\" + gpxmagellan + "\\GPX_ExploristGC.gpx";
            String offdatapath = ConfigurationManager.AppSettings["datapath"] + "\\Offline\\";
            System.IO.StreamWriter file = new System.IO.StreamWriter(fileRadix, false);
            
            file.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            file.WriteLine("<gpx xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" version=\"1.0\" creator=\"Groundspeak, Inc. All Rights Reserved. http://www.groundspeak.com\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/0 http://www.topografix.com/GPX/1/0/gpx.xsd http://www.groundspeak.com/cache/1/0/1 http://www.groundspeak.com/cache/1/0/1/cache.xsd\" xmlns=\"http://www.topografix.com/GPX/1/0\">");
            file.WriteLine("  <name>Cache Listing Generated from Geocaching.com</name>");
            file.WriteLine("  <desc>This is an individual cache generated from Geocaching.com</desc>");
            file.WriteLine("  <author>Account \"" + ConfigurationManager.AppSettings["owner"] + "\" From Geocaching.com</author>");
            file.WriteLine("  <email>contact@geocaching.com</email>");
            file.WriteLine("  <url>http://www.geocaching.com</url>");
            file.WriteLine("  <urlname>Geocaching - My Geocaching Manager Export " + DateTime.Now.ToLongDateString() + "</urlname>");
            // 2011-09-18T07:00:00Z
            String date = DateTime.Now.ToString("yyyy-MM-ddThh:mm:ssZ");
            file.WriteLine("  <time>" + date + "</time>");
            file.WriteLine("  <keywords>cache, geocache</keywords>");

            double minlat = 90.0;
            double minlon = 180.0;
            double maxlat = -90.0;
            double maxlon = -180.0;

            String bigtext = "";
			List<Geocache> lst = _Daddy.GetSelectedCaches();
            foreach (Geocache geo in lst)
            {
                if (geo._dLatitude < minlat)
                    minlat = geo._dLatitude;
                if (geo._dLongitude < minlon)
                    minlon = geo._dLongitude;
                if (geo._dLatitude > maxlat)
                    maxlat = geo._dLatitude;
                if (geo._dLongitude > maxlon)
                    maxlon = geo._dLongitude;
				
				String chunk = geo.ToGPXChunk();
				// Save images & spoilers in base 64?
				if (_Daddy._od._OfflineData.ContainsKey(geo._Code))
				{
					OfflineCacheData ocd = _Daddy._od._OfflineData[geo._Code];
					if ((ocd._ImageFilesSpoilers.Count != 0)||(ocd._ImageFilesFromDescription.Count != 0))
                    {
						int ipic = 0;
						String fjpeg = "";
						if (ocd._ImageFilesSpoilers.Count != 0)
						{
							String spoilersImg = "";
							foreach (KeyValuePair<string, OfflineImageWeb> paire in ocd._ImageFilesSpoilers)
							{
								try
								{
									// Load image
									fjpeg = geo._Code + "_" + String.Format("{0:000}", ipic) + ".jpg";
									Image img = Image.FromFile(offdatapath + paire.Value._localfile);
									img.Save(fjpeg, System.Drawing.Imaging.ImageFormat.Jpeg);
									img.Dispose();
									
									String imgsrc = "&lt;br&gt;&lt;li&gt;" + paire.Value._name + "&lt;br&gt;&lt;img src=\"" + fjpeg + "\"&gt;&lt;/li&gt;";
									spoilersImg += imgsrc;
									
									ipic++;
								}
								catch (Exception e1)
								{
									// Do nothing
									_Daddy.Log("!!!!!! Spoiler " + e1.Message);
								}
							}
							
							chunk = chunk.Replace("</groundspeak:long_description>", spoilersImg + "</groundspeak:long_description>");
						}
						
						if (ocd._ImageFilesFromDescription.Count != 0)
						{
							foreach (KeyValuePair<string, string> paire in ocd._ImageFilesFromDescription)
							{
								try
								{
									// Load image
									fjpeg = geo._Code + "_" + String.Format("{0:000}", ipic) + ".jpg";
									Image img = Image.FromFile(offdatapath + paire.Value);
									img.Save(fjpeg, System.Drawing.Imaging.ImageFormat.Jpeg);
									img.Dispose();
									
									// Save to Jpeg
									chunk = chunk.Replace(paire.Key, fjpeg);
									
									ipic++;
								}
								catch (Exception e1)
								{
									// Do nothing
									_Daddy.Log("!!!!!! Description " + e1.Message);
								}
							}
						}
					}
				}
				
                bigtext += chunk;
            }
            //   <bounds minlat="-12.03455" minlon="-77.017917" maxlat="-12.03455" maxlon="-77.017917" />
            String bounds = "";
            bounds = "  <bounds minlat=\"" + minlat
                + "\" minlon=\"" + minlon
                + "\" maxlat=\"" + maxlat
                + "\" maxlon=\"" + maxlon + "\" />";
            bounds = bounds.Replace(",", ".");
            file.WriteLine(bounds);
            file.Write(bigtext);
            file.WriteLine("</gpx>");

            file.Close();

			System.Diagnostics.Process.Start(path + "\\MAGELLAN");
            MessageBox.Show("Export done in " + fileRadix,
                "Attention!",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return fileRadix;
        }
		
		public bool DoIt()
		{
			if (_Daddy != null)
			{
				// http://ocm.dafb-o.de/index.php?topic=89.0
				FolderBrowserDialog fbd = new FolderBrowserDialog();
                String exePath = Path.GetDirectoryName(Application.ExecutablePath);
                fbd.SelectedPath = exePath;
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    ExportExploristGC(fbd.SelectedPath);
                }
				
				return true;
			}
			else
			{
				MessageBox.Show("Nothing to do");
				return false;
			}
		}
		
		public void Close()
		{
		}
	}
}