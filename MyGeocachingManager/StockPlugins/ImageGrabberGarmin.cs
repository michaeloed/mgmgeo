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
		public string Name {get {return "Export all mages to Garmin (modern ones)";}}
		public string Description { get {return "Export all images from displayed caches - compatible with Garmin units (Montana, Oregon x50, Dakota, GPSMAP 62/78, eTrex 20/30";}}
		
		public void Initialize(MainWindow daddy)
		{
			_Daddy = daddy;
		}
		
		public bool DoIt()
		{
		/*
http://garmin.blogs.com/softwareupdates/2012/01/geocaching-with-photos.html

Works With All Geocaches
You can also take advantage of geocache photos on your Garmin handheld for geocaches obtained from a source other than OpenCaching.com, it just takes some work. A geocache’s photos, JPEG only, need to be placed on the handheld’s mass storage in the following manner

Photos
	\Garmin\GeocachePhotos\Last Character\Second To Last Character\Full Code\

Spoiler Photos
	<Photos Path>\Spoilers\
For example, photos for a geocache with code OXZTXGC would be placed under the path

	\Garmin\GeocachePhotos\C\G\OXZTXGC\

And spoilers would be placed under

	\Garmin\GeocachePhotos\C\G\OXZTXGC\Spoilers

If the geocache has only three characters total, a 0 (zero) is used for the second to last character. For example, photos for a geocache with code OXR would be placed under the path

	\Garmin\GeocachePhotos\R\0\OXR\
		*/
			if (_Daddy != null)
			{
				String exePath = Path.GetDirectoryName(Application.ExecutablePath);
                Directory.SetCurrentDirectory(exePath);
				String imgGarmin = "Garmin";
				imgGarmin = "Garmin\\GeocachePhotos";
				Directory.CreateDirectory(imgGarmin);
				Directory.SetCurrentDirectory(imgGarmin);
				
				// Now export
				List<Geocache> lst = _Daddy.GetDisplayedCaches();
				String offdatapath = ConfigurationManager.AppSettings["datapath"] + "\\Offline\\";
				int nb = 0;
				foreach(Geocache geo in lst)
				{
					// Save images & spoilers and geotag them
                    if (_Daddy._od._OfflineData.ContainsKey(geo._Code))
                    {
						_Daddy.Log(">>>>>>> Exporting " + geo._Code);
						
						// Do we have images to load and save?
                        OfflineCacheData ocd = _Daddy._od._OfflineData[geo._Code];
						String lastS = geo._Code.Substring(geo._Code.Length-1,1);
						String prelastS = geo._Code.Substring(geo._Code.Length-2,1);
							
                        if ((ocd._ImageFilesSpoilers.Count != 0)||(ocd._ImageFilesFromDescription.Count != 0))
                        {
							int ipic = 0;
							_Daddy.Log("Images found !");
							// Spoilers
							_Daddy.Log("Spoilers ?");
							if (ocd._ImageFilesSpoilers.Count != 0)
							{
								_Daddy.Log("Go with spoilers");
								String imgPath = lastS + "\\" + prelastS + "\\" + geo._Code + "\\Spoilers";
								Directory.CreateDirectory(imgPath);
								
								foreach (KeyValuePair<string, OfflineImageWeb> paire in ocd._ImageFilesSpoilers)
								{
									String radix = geo._Code + "_" + String.Format("{0:000}", ipic) + "_" +
											HtmlAgilityPack.HtmlEntity.DeEntitize(paire.Value._name) + ".jpg";
									radix = _Daddy.SanitizeFilename(radix);
									
									String fjpeg = imgPath + "\\" + radix;
									ipic++;
									try
									{
										// Load image
										Image img = Image.FromFile(offdatapath + paire.Value._localfile);

										// Save to Jpeg
										try
										{
											// Test with this filename
											img.Save(fjpeg, System.Drawing.Imaging.ImageFormat.Jpeg);
										}
										catch(Exception)
										{
											// Ok standard filename, something wrong happened
											fjpeg = imgPath + "\\" + geo._Code + "_" + String.Format("{0:000}", ipic) + ".jpg";
											img.Save(fjpeg, System.Drawing.Imaging.ImageFormat.Jpeg);
										}
										img.Dispose();
										
										_Daddy.WriteCoordinatesToImage(fjpeg, geo._dLatitude, geo._dLongitude);
										nb++;
									}
									catch (Exception e1)
									{
										// Do nothing
										_Daddy.Log(e1.Message);
									}
								}
							}
							
							// Images from description
							_Daddy.Log("Description ?");
							if (ocd._ImageFilesFromDescription.Count != 0)
							{
								_Daddy.Log("Go with description");
								String imgPath = lastS + "\\" + prelastS + "\\" + geo._Code;
								Directory.CreateDirectory(imgPath);
								
								foreach (KeyValuePair<string, string> paire in ocd._ImageFilesFromDescription)
								{
									String fjpeg = imgPath + "\\" + geo._Code + "_" + String.Format("{0:000}", ipic) + ".jpg";
									ipic++;
									try
									{
										// Load image
										Image img = Image.FromFile(offdatapath + paire.Value);

										// Save to Jpeg
										img.Save(fjpeg, System.Drawing.Imaging.ImageFormat.Jpeg);
										img.Dispose();
										
										nb++;
									}
									catch (Exception e1)
									{
										// Do nothing
										_Daddy.Log(e1.Message);
									}
								}
							}
						}
					}
				}
				_Daddy.MSG(nb.ToString() + " Image(s) exported");
				return true;
			}
			else
			{
				MessageBox.Show("Nothing exported");
				return false;
			}
		}
		
		public void Close()
		{
		}
	}
}