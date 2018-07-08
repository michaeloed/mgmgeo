using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Configuration;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using SpaceEyeTools;
using SpaceEyeTools.HMI;
using MyGeocachingManager.Geocaching;
using System.Web;

namespace MyGeocachingManager
{
    /// <summary>
    /// Worker used to pilot caches images / spoilers download
    /// </summary>
    public class DownloadWorker
    {

    	private MainWindow _daddy = null;
    	
    	/// <summary>
    	/// If true, image will ge grabed from complete caches gallery
    	/// </summary>
    	public bool bGetFromGallery = false;
    	
        /// <summary>
        /// Associated ThreadProgress object for notification and abort
        /// </summary>
        public ThreadProgress threadprogress = null;

        /// <summary>
        /// List of OfflineCacheData to download
        /// </summary>
        public List<OfflineCacheData> ocds = null;

        /// <summary>
        /// List of spoilers keywords to use to detect spoiler images
        /// </summary>
        public List<String> keywordsspoiler = null;

        /// <summary>
        /// Delay in secondes between two downloads
        /// </summary>
        public int delay = 0;
        
        /// <summary>
        /// GC account cookie, can be null (anonymous download)
        /// </summary>
        public CookieContainer cookieJar = null;
        
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="daddy">Mainwindow</param>
        public DownloadWorker(MainWindow daddy)
        {
        	_daddy = daddy;
        }
        
        private void AssignProxy(WebClient client)
        {
            if (_daddy != null)
            {
                WebProxy proxy = _daddy.GetProxy();
                if (proxy != null)
                    client.Proxy = proxy;
            }
        }

        
        private void UpdateListImagesWithNewOnesFromHtml(ref List<Tuple<String, String, String>> toutesLesImages, string html)
        {
        	String bloc = MyTools.GetSnippetFromText("ctl00_ContentBody_GalleryItems_DataListGallery", "/table", html);
            List<String> images = MyTools.GetSnippetsFromText("<a href=", "data-title", bloc);
            List<String> noms = MyTools.GetSnippetsFromText("<span>", "</span>", bloc);
            List<String> dates = MyTools.GetSnippetsFromText("<span class=\"date-stamp\">", "</span>", bloc);
            int index = 0;
            foreach(String s in images)
            {
            	String link = s.Replace("'","").Replace(" ","");
            	// Le fichier local
            	int pos = link.LastIndexOf('/');
            	String local = Guid.NewGuid().ToString();
            	String nom = noms[index];
            	if (nom == "")
            		nom = "?";
            	toutesLesImages.Add(new Tuple<string, string, string>(link, dates[index].Replace('/','-') + " " + nom,local));
            	index++;
            }
        }
        
        private List<Tuple<String, String, String>> GetAllImageUrlsFromCacheGallery(String url, ref bool status)
        {
        	try
        	{
	        	// Page de la cache
	            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
	            objRequest.Proxy = _daddy.GetProxy();
	            if (cookieJar != null)
	            	objRequest.CookieContainer = cookieJar;
	            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
	            String response;
	            using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
	            {
	                response = responseStream.ReadToEnd();
	                responseStream.Close();
	            }
	            
	            // On construit l'url de la gallery
	            String gallery = MyTools.GetSnippetFromText("seek/gallery.aspx?", "\"", response);
	            url  = "https://www.geocaching.com/seek/gallery.aspx?" + gallery;
	            
	            // Page de la gallery
	            objRequest = (HttpWebRequest)WebRequest.Create(url);
	            objRequest.Proxy = _daddy.GetProxy();
	            if (cookieJar != null)
	            	objRequest.CookieContainer = cookieJar;
	            objResponse = (HttpWebResponse)objRequest.GetResponse();
	            using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
	            {
	                response = responseStream.ReadToEnd();
	                responseStream.Close();
	            }
	            
	            // On calcule combien de pages il y a
	            String PageBuilderWidget = MyTools.GetSnippetFromText("PageBuilderWidget", "PageBuilderWidget", response);
	            List<String> values = MyTools.GetSnippetsFromText("<b>","</b>",PageBuilderWidget);
	            int nbPages = 1;
	            if (values.Count >= 3)
	            {
	            	Int32.TryParse(values[2], out nbPages);
	            }
	            
	            // On est sur la page 1, on va récupérer déjà les images de cette page
	            List<Tuple<String, String, String>> toutesLesImages = new List<Tuple<String, String, String>>();
	            
	            // Block d'images
	            UpdateListImagesWithNewOnesFromHtml(ref toutesLesImages, response);
	            
	            // Maintenant on boucle sur les pages
	            for(int ipage=2; ipage<= nbPages; ipage++)
	            {
	                // On récupère les viewstates de la page précédente
	                String __VIEWSTATEFIELDCOUNT = "";
	                String[] __VIEWSTATE = null;
	                String __VIEWSTATEGENERATOR = "";
	                MainWindow.GetViewState(response, ref __VIEWSTATEFIELDCOUNT, ref __VIEWSTATE, ref __VIEWSTATEGENERATOR);
	
	                // Préparation des données du POST
	                Dictionary<String, String> post_values = new Dictionary<String, String>();
	                // tout marche par paquet de 10. 
	                // A chaque 10e page, la suivant doit être appelée avec la flêche, sinon ça merde
	                if (ipage%10 == 1)
	                {
	                	// On est sur une page multiple de 10, on appelle la suivante avec next
	                	post_values.Add("__EVENTTARGET", "ctl00$ContentBody$GalleryItems$ResultsPager$ctl06");
	                }
	                else
	                {
	                	// On appele avec le bon numéro de page
	                	post_values.Add("__EVENTTARGET", "ctl00$ContentBody$GalleryItems$ResultsPager$lbGoToPage_" + ipage.ToString());
	                }
	                
	                post_values.Add("__EVENTARGUMENT", "");
	                
	                // Les viewstate
	                post_values.Add("__VIEWSTATE", __VIEWSTATE[0]);
	                if (__VIEWSTATE.Length > 1)
	                {
	                    for (int i = 1; i < __VIEWSTATE.Length; i++)
	                    {
	                        post_values.Add("__VIEWSTATE" + i.ToString(), __VIEWSTATE[i]);
	                    }
	                    post_values.Add("__VIEWSTATEFIELDCOUNT", __VIEWSTATE.Length.ToString());
	                }
	                if (__VIEWSTATEGENERATOR != "")
	                    post_values.Add("__VIEWSTATEGENERATOR", __VIEWSTATEGENERATOR);
	
	                // Encodage des données du POST
	                String post_string = "";
	                foreach (KeyValuePair<String, String> post_value in post_values)
	                {
	                    post_string += post_value.Key + "=" + HttpUtility.UrlEncode(post_value.Value) + "&";
	                }
	                post_string = post_string.TrimEnd('&');
	
	                // Création de la requête pour s'authentifier
	                objRequest = (HttpWebRequest)WebRequest.Create(url);
	                objRequest.Method = "POST";
	                objRequest.ContentLength = post_string.Length;
	                objRequest.ContentType = "application/x-www-form-urlencoded";
	                objRequest.Proxy = _daddy.GetProxy();
	                if (cookieJar != null)
	            		objRequest.CookieContainer = cookieJar;
	                
	                // on envoit les POST data dans un stream (écriture)
	                StreamWriter myWriter = null;
	                myWriter = new StreamWriter(objRequest.GetRequestStream());
	                myWriter.Write(post_string);
	                myWriter.Close();
	
	                // lecture du stream de réponse et conversion en chaine
	                objResponse = (HttpWebResponse)objRequest.GetResponse();
	                using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
	                {
	                    response = responseStream.ReadToEnd();
	                    responseStream.Close();
	                }
	                
	                // Block d'images
	            	UpdateListImagesWithNewOnesFromHtml(ref toutesLesImages, response);
	            }
	            
	            status = true;
	            return toutesLesImages;
        	}
        	catch(Exception)
        	{
        		status = false;
        		return null;
        	}
        		
        }
        
        private bool GetSpoilersFromDescription(OfflineCacheData ocd) // cf. Surfoo
        {
            List<KeyValuePair<String, String>> spoilers = new List<KeyValuePair<string, string>>();
            try
            {
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(ocd._descHTML);
                var commentNodes = doc.DocumentNode.SelectNodes("//comment()");
                if (commentNodes != null)
                {
                    foreach (HtmlAgilityPack.HtmlNode node in commentNodes)
                    {
                        String cmt = node.InnerHtml;
                        if (cmt.StartsWith("<!-- Spoiler4Gpx ["))
                        {
                            int ipos1, ipos2;
                            String sName = "", sLink = "";
                            ipos1 = cmt.IndexOf("[");
                            ipos2 = cmt.IndexOf("](http");
                            if ((ipos1 != -1) && (ipos2 != -1) && (ipos1 < ipos2))
                                sName = cmt.Substring(ipos1 + 1, ipos2 - ipos1 - 1);

                            ipos1 = cmt.IndexOf("](http");
                            ipos2 = cmt.IndexOf(") -->");
                            if ((ipos1 != -1) && (ipos2 != -1) && (ipos1 < ipos2))
                                sLink = cmt.Substring(ipos1 + 2, ipos2 - ipos1 - 2);

                            if ((sName != "") && (sLink != ""))
                            {
                                spoilers.Add(new KeyValuePair<string, string>(sName, sLink));
                            }
                        }
                    }
                }
                if (spoilers.Count == 0)
                    return false;
                else
                {
                    // Now, deal with these spoilers
                    bool bUseKeyWords = false;
                    if ((keywordsspoiler != null) && (keywordsspoiler.Count != 0))
                        bUseKeyWords = true;

                    foreach (KeyValuePair<String, String> paire in spoilers)
                    {
                        String name = paire.Key;
                        String url = paire.Value;

                        bool bKeep = true;
                        if (bUseKeyWords)
                        {
                            bKeep = false;
                            // Check if one keyword is contained in the name
                            String n = name.ToLower();
                            foreach (String s in keywordsspoiler)
                            {
                                if (n.Contains(s))
                                {
                                    bKeep = true;
                                    break;
                                }
                            }
                        }

                        if (bKeep)
                        {
                            OfflineImageWeb oiw = new OfflineImageWeb();
                            oiw._url = url;
                            oiw._localfile = ocd._Code + "_P_" + Guid.NewGuid();
                            oiw._name = name;
                            ocd._ImageFilesSpoilers.Add(oiw._url, oiw);
                        }
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        /// <summary>
        /// Fetch geocache base on ocd._Code and download HTML page to list images to grab
        /// Images will be stored in ocd._ImageFilesSpoilers
        /// </summary>
        /// <param name="ocd">OCD object to complete</param>
        private void GetImageFromParsing(OfflineCacheData ocd)
        {
            bool bUseKeyWords = false;
            if ((keywordsspoiler != null) && (keywordsspoiler.Count != 0))
                bUseKeyWords = true;

            try
            {
                Geocache geo = _daddy._caches[ocd._Code];
                String url = geo._Url;
                
                /*
                WebClient client = new WebClient();
                AssignProxy(client);
                string htmlCode = client.DownloadString(url);
				*/
				
				// nouvelle méthode utilisant le cookie si besoin
                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
	            objRequest.Proxy = _daddy.GetProxy();
	            if (cookieJar != null)
	            	objRequest.CookieContainer = cookieJar;
	            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
	            string htmlCode;
	            using (StreamReader responseStream = new StreamReader(objResponse.GetResponseStream()))
	            {
	                htmlCode = responseStream.ReadToEnd();
	                responseStream.Close();
	            }
	            
                GetImageFromParsingImpl(ocd, bUseKeyWords, htmlCode);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Fetch geocache base on ocd._Code and download HTML page to list images to grab
        /// Images will be stored in ocd._ImageFilesSpoilers
        /// </summary>
        /// <param name="ocd">OCD object to complete</param>
        /// <param name="bUseKeyWords">If true, spoilers will be downloaded based on keywords provided in keywordsspoiler</param>
        /// <param name="htmlCode">Cache html page</param>
        public void GetImageFromParsingImpl(OfflineCacheData ocd, bool bUseKeyWords, string htmlCode)
        {
            // Patch
            String chunk = MyTools.GetSnippetFromText("<ul class=\"CachePageImages NoPrint\">", "</ul>", htmlCode);
            chunk = "<html><body>" + chunk + "</body></html>";
            
            // Parse HTML to retrieve links
            // Load the Html into the agility pack
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(chunk);
            // Now, using LINQ to get all Images
            var linkNodes = doc.DocumentNode.SelectNodes("//a[@href]");
            if (linkNodes != null)
            {
                //String ahref = "";
                foreach (HtmlAgilityPack.HtmlNode node in linkNodes)
                {
                    if (node.Attributes.Contains("href"))
                    {
                        String name = HtmlAgilityPack.HtmlEntity.DeEntitize(node.InnerText);
                        bool bKeep = true;
                        if (bUseKeyWords)
                        {
                            bKeep = false;
                            // Check if one keyword is contained in the name
                            String n = name.ToLower();
                            foreach (String s in keywordsspoiler)
                            {
                                if (n.Contains(s))
                                {
                                    bKeep = true;
                                    break;
                                }
                            }
                        }

                        if (bKeep)
                        {
                            String url = node.Attributes["href"].Value;
                            OfflineImageWeb oiw = new OfflineImageWeb();
                            oiw._url = url;
                            oiw._localfile = ocd._Code + "_P_" + Guid.NewGuid();
                            oiw._name = name;
                            ocd._ImageFilesSpoilers.Add(oiw._url, oiw);
                        }
                    }
                }
            }
        }

        
        private void TryBase64Conversion(String url, String localfile)
        {
            try
            {
                // Base 64:
                // data:image/jpeg;base64,XXXXXXXXXXXXXXXXX
                String pattern = "data:image/jpeg;base64,";
                if (url.StartsWith(pattern))
                {
                    String data = url.Replace(pattern, "");
                    Image img = MyTools.Base64ToImage(data);
                    img.Save(localfile, System.Drawing.Imaging.ImageFormat.Jpeg);
                    img.Dispose();
                }
            }
            catch (Exception)
            {
              
            }
        }

        private void OldSchoolGrabbing(OfflineCacheData ocd2)
        {
        	// La vielle méthode un peu incimpréhensible qui marche
        	bool bSpoilersAlreadyInDescription = false;
        	// Build images to download from description
            if (ocd2._descHTML == "")
            {
                // Nothing to do for the description
            }
            else
            {
                // Description present, looking for img url's
                ocd2._descHTML = "<html><body>" + ocd2._descHTML + "</body></html>";
                // Parse HTML description  to retrieve images
                // Load the Html into the agility pack
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(ocd2._descHTML);
                // Now, using LINQ to get all Images
                var imageNodes = doc.DocumentNode.SelectNodes("//img");
                if (imageNodes != null)
                {
                    foreach (HtmlAgilityPack.HtmlNode node in imageNodes)
                    {
                        if (node.Attributes.Contains("src"))
                        {
                            String url = node.Attributes["src"].Value;
                            if (ocd2._ImageFilesFromDescription.ContainsKey(url) == false)
                            {
                                String file = ocd2._Code + "_" + Guid.NewGuid();
                                ocd2._ImageFilesFromDescription.Add(url, file);
                            }
                        }
                    }
                }

                // Surfoo style: try to get spoilers from the description
                bSpoilersAlreadyInDescription = GetSpoilersFromDescription(ocd2);

                // Free some memory :-)
                ocd2._descHTML = "";
            }

            _daddy.Log("Retrieving spoilers from cache " + ocd2._Code);
            if (!bSpoilersAlreadyInDescription)
            {
                _daddy.Log("Spoilers taken from HTML parsing");
                // Parse html page and detect image to download in addition to the ones in the description
                // Spoilers ?
                GetImageFromParsing(ocd2);
            }
            else
            {
                _daddy.Log("Spoilers taken from GPX description");
            }
        }
        
        /// <summary>
        /// This method will be called when the thread is started.
        /// Perform download
        /// </summary>
        public void DoWork()
        {
            if ((ocds != null) && (ocds.Count != 0))
            {
                _daddy.bOfflineDownloadInProgress = true;
                String offdatapath = _daddy.GetUserDataPath() + Path.DirectorySeparatorChar + "Offline";
                
                // On continue à télécharger les images en anonyme, pas besoin du cookie ici
                WebClient client = new WebClient();
                AssignProxy(client);

                // Compute how many files to download
                int iFiles = ocds.Count;

                // 3* : because 3 steps : parse html, download description, download galerie
                threadprogress.progressBar1.Maximum = 3*iFiles;
                threadprogress.lblWait.Text = "";
                bool bNice = true;

                // Download each file
                foreach (OfflineCacheData ocd2 in ocds)
                {
                    // Only now we delete local files of this single cache only
                    // Do it BEFORE changing the file list ;-)
                    ocd2.PurgeFiles(offdatapath);

                    // Get geocache
                    Geocache geo = null;
                    if (_daddy._caches.ContainsKey(ocd2._Code))
                        geo = _daddy._caches[ocd2._Code];
                    else
                    {
                        // ?????
                        continue; // skip this one
                    }

                	// This is a real geocache
                	// ***********************
                	if (!bGetFromGallery)
                	{
                		// La vielle méthode un peu incimpréhensible qui marche
                		OldSchoolGrabbing(ocd2);
                	}
                	else
                	{
                		// On récupère de la gallery
                		bool status = false;
                		String url = geo._Url;
                		if (url == "")
                		{
                			url = "https://coord.info/" + geo._Code; // mais ça risque de ne pas marcher en résolution de nom
                		}
                		
                		// On récupère les images data
                		List<Tuple<String, String, String>> imgdata = GetAllImageUrlsFromCacheGallery(url, ref status);
                		if (!status)
                		{
                			// ca a merdé, on utilise la méthode de base
                			OldSchoolGrabbing(ocd2);
                		}
                		else
                		{
                			// Libère la mémoire
                			ocd2._descHTML = "";
                			
                			// Ca a marché. Cool
                			// Tuple : link, name, locafile
                			// Now, deal with these spoilers
		                    bool bUseKeyWords = false;
		                    if ((keywordsspoiler != null) && (keywordsspoiler.Count != 0))
		                        bUseKeyWords = true;
		                    
                			foreach(Tuple<String, String, String> tpl in imgdata)
                			{
                				bool bKeep = true;
		                        if (bUseKeyWords)
		                        {
		                            bKeep = false;
		                            // Check if one keyword is contained in the name
		                            String n = tpl.Item2.ToLower();
		                            foreach (String s in keywordsspoiler)
		                            {
		                                if (n.Contains(s))
		                                {
		                                    bKeep = true;
		                                    break;
		                                }
		                            }
		                        }
		
		                        if (bKeep)
		                        {
		                            OfflineImageWeb oiw = new OfflineImageWeb();
		                            oiw._url = tpl.Item1;
		                            oiw._localfile = ocd2._Code + "_P_" + tpl.Item3;
		                            oiw._name = tpl.Item2;
		                            ocd2._ImageFilesSpoilers.Add(oiw._url, oiw);
		                        }
                			}
                		}
                	}
                    
                    threadprogress.Step();

                    // Grab each picture
                    if (threadprogress._bAbort || _shouldStop)
                    {
                        ocd2._bAborted = true;
                    }
                    else
                    {
                        // Images from description
                        foreach (KeyValuePair<String, String> paire in ocd2._ImageFilesFromDescription)
                        {
                            if (threadprogress._bAbort || _shouldStop)
                            {
                                ocd2._bAborted = true;
                                break; // Stop for the current files
                            }

                            String url = paire.Key;
                            string fileName = paire.Value;
                            String localfile = offdatapath + Path.DirectorySeparatorChar + String.Format("{0}", fileName);
                            try
                            {
                                client.DownloadFile(new Uri(url), localfile);
                            }
                            catch (Exception)
                            {
                                // Keep what we downloaded anyway
                                TryBase64Conversion(url, localfile);
                            }
                        }
                        threadprogress.Step();

                        // Images from parsing
                        foreach (KeyValuePair<String, OfflineImageWeb> paire2 in ocd2._ImageFilesSpoilers)
                        {
                            if (threadprogress._bAbort || _shouldStop)
                            {
                                ocd2._bAborted = true;
                                break; // Stop for the current files
                            }

                            String url = paire2.Key;
                            string fileName = paire2.Value._localfile;
                            String localfile = offdatapath + Path.DirectorySeparatorChar + String.Format("{0}", fileName);
                            try
                            {
                                client.DownloadFile(new Uri(url), localfile);
                            }
                            catch (Exception)
                            {
                                // Keep what we downloaded anyway
                                TryBase64Conversion(url, localfile);
                            }
                        }
                        threadprogress.Step();

                        // IMPORTANT
                        // If we reached this far, we assume the cache has been parsed
                        // properly (except if we aborted in between of course)
                        // So we set the attribute ocd1._bJustCreated to false
                        // Because in post processing, the "justcreated" caches will be removed
                        ocd2._NotDownloaded = false;

                        // Security from spider web : delay ?
                        if (delay != 0)
                        {
                            if (threadprogress._bAbort || _shouldStop)
                            {
                                // Do nothing
                            }
                            else
                            {
                                threadprogress.lblWait.Text =
                                    (bNice ? @"- " : @"_ ") + String.Format(_daddy.GetTranslator().GetString("LblWaiting"), delay);
                                System.Threading.Thread.Sleep(delay * 1000);
                                threadprogress.lblWait.Text = "";
                                bNice = !bNice;
                            }
                        }
                    }

                    if (threadprogress._bAbort || _shouldStop)
                    {
                        // Stop the loop
                        break;
                    }
                }

                threadprogress.Hide();
                threadprogress.Dispose();
            }
            _daddy.bOfflineDownloadInProgress = false;
            _daddy.NotifyEndOfThread(this);
        }

        /// <summary>
        /// Request stop / abort of the download worker.
        /// Worker will stop as soon as possible
        /// </summary>
        public void RequestStop()
        {
            _shouldStop = true;
        }

        // Volatile is used as hint to the compiler that this data
        // member will be accessed by multiple threads.
        private volatile bool _shouldStop;
    }

}
