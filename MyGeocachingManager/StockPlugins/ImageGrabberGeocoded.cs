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
		public string Name {get {return "Export Images and Spoilers (geocoded)";}}
		public string Description { get {return "Export images and spoilers from displayed caches - Geocoded and stored in a single directory";}}
		
		public void Initialize(MainWindow daddy)
		{
			_Daddy = daddy;
		}
		
		public bool DoIt()
		{
			if (_Daddy != null)
			{
				String exePath = Path.GetDirectoryName(Application.ExecutablePath);
                Directory.SetCurrentDirectory(exePath);
				String imgPath = "ExportedImages";
				Directory.CreateDirectory(imgPath);
				
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
							
                        if (ocd._ImageFilesSpoilers.Count != 0)
                        {
							_Daddy.Log("Go with spoilers");
							
							int ipic = 0;
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