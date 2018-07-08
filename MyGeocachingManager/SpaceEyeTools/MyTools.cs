using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net;
using System.Globalization;
using System.Threading;
using System.Security.Cryptography;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Text.RegularExpressions;
using SpaceEyeTools.HMI;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Web;

namespace SpaceEyeTools
{
    /// <summary>
    /// A lovely "fourre-tout" class that contains various and unrelated functions thet serve my own goals ;-)
    /// </summary>
    static public class MyTools
    {
        static CultureInfo cEN = new CultureInfo("en-GB");
        static Font objFont = new Font("Arial", 10, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
        static Font objFontBig = new Font("Arial", 16, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
        static Pen blackPen = new Pen(Color.Black, 1);
        static double _toRad = 3.1415926538 / 180.0;
        static double _earthRadiusKm = 6371.0;
        static double _toRadearthRadiusKm = 3.1415926538 * 6371.0 / 180.0;
        
        /// <summary>
        /// Random generator
        /// </summary>
        public static readonly Random random = new System.Random((int)DateTime.Now.Ticks);

        
        /// <summary>
        /// Build stacktrace
        /// </summary>
        /// <param name="stack">Stacktrace, can be null</param>
        /// <param name="offset">number of stack frames to ignore</param>
        /// <param name="prefix">prefix to append in front of each stack line (can be "")</param>
        /// <returns>stacktrace</returns>
        static public string StackTraceToString(System.Diagnostics.StackTrace stack, int offset, String prefix)
		{
		    StringBuilder sb = new StringBuilder(256);
		    StackFrame[] frames = null;
		    if (stack != null)
		    	frames = stack.GetFrames();
		    else
		    	frames = new System.Diagnostics.StackTrace().GetFrames();
		    
		    for (int i = offset; i < frames.Length; i++) /* Ignore current StackTraceToString method...*/
		    {
		        var currFrame = frames[i];

		        var method = currFrame.GetMethod();
		        if (String.IsNullOrEmpty(currFrame.GetFileName()))
		        {
		        	sb.AppendLine(prefix + string.Format("{0}: {1}",                    
			            method.ReflectedType != null ? method.ReflectedType.Name : string.Empty,
			            method.ToString()
			           )); // method.Name
		        }
		        else
		        {
			        sb.AppendLine(prefix + string.Format("{0}: {1} [{2}:(C{3},L{4})]",                    
			            method.ReflectedType != null ? method.ReflectedType.Name : string.Empty,
			            method.ToString(),
			            currFrame.GetFileName(),
			            currFrame.GetFileColumnNumber(),
			            currFrame.GetFileLineNumber()
			           )); // method.Name
		        }
		    }
		    return sb.ToString();
		}
        
        /// <summary>
        /// Build stacktrace
        /// </summary>
        /// <param name="offset">number of stack frames to ignore</param>
        /// <param name="prefix">prefix to append in front of each stack line (can be "")</param>
        /// <returns>stacktrace</returns>
        static public string StackTraceToString(int offset, String prefix)
		{
        	return StackTraceToString(new System.Diagnostics.StackTrace(), offset, prefix);
		}
        
        /// <summary>
        /// Shuffle a list
        /// </summary>
        /// <typeparam name="T">type of the list</typeparam>
        /// <param name="deck">list to shuffle</param>
        public static void Shuffle<T>(List<T> deck)
        {
            int N = deck.Count;

            for (int i = 0; i < N; ++i)
            {
                int r = i + (int)(random.Next(N - i));
                T t = deck[r];
                deck[r] = deck[i];
                deck[i] = t;
            }
        }

        /// <summary>
        /// Create a random string
        /// </summary>
        /// <param name="size">string lentgh</param>
        /// <returns>random string</returns>
        public static string RandomString(int size)
	    {
	        StringBuilder builder = new StringBuilder();
	        char ch;
	        for (int i = 0; i < size; i++)
	        {
	            ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));                 
	            builder.Append(ch);
	        }
	
	        return builder.ToString();
	    }
	
        /// <summary>
        /// Create a random number
        /// </summary>
        /// <param name="max">maximum value (included)</param>
        /// <returns>random number between 0 and max value both included</returns>
        public static int RandomNumber(int max)
	    {
        	return random.Next(0, max);
	    }

        /// <summary>
        /// Remove unwanted characters from a string:
        /// \r\n
        /// \n
        /// :
        ///  (space)
        /// </summary>
        /// <param name="s">string to clean</param>
        /// <returns>cleanedc string</returns>
        /*
        public static String CleanString(String s)
        {
            String r = s;
            if (r != "")
            {
                r = r.Replace("\r\n", "");
                r = r.Replace("\n", "");
                r = r.Replace(":", "");
                r = r.Replace(" ", "");
            }
            return r;
        }*/
        public static String CleanString(String s)
	    {
        	if (String.IsNullOrEmpty(s))
        		return s;
        	
			int len = s.Length;
			char[] s2 = new char[len];
			int i2 = 0;
			for (int i = 0; i < len; i++)
			{
			    char c = s[i];
			    if (c != ':' && c != '\r' && c != '\n' && c != ' ')
			        s2[i2++] = c;
			}
			return new String(s2, 0, i2);
	    }

        /// <summary>
        /// Convert UTF8 text to something readable with lovely accents (ISO-8859-1)
        /// </summary>
        /// <param name="s">string to convers</param>
        /// <returns>converted string</returns>
        public static String ConvertUTF8ToNice(String s)
        {
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding utf8 = Encoding.UTF8;
            byte[] utfBytes = utf8.GetBytes(s);
            byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            string msg = iso.GetString(isoBytes);
            return msg;
        }

        /// <summary>
        /// Convert a string to double, regardless the current culture string format
        /// Will handle . BUT NOT "," as a decimal separator
        /// </summary>
        /// <param name="s">string to convert</param>
        /// <returns>double value</returns>
        static public Double ConvertToDoubleFast(String s)
        {
        	double d = Double.MaxValue;
        	Double.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out d);
        	return d;
        	
        }
        
        /// <summary>
        /// Convert a string to double, regardless the current culture string format
        /// Will handle . or , as a decimal separator
        /// </summary>
        /// <param name="s">string to convert</param>
        /// <returns>double value</returns>
        static public Double ConvertToDouble(String s)
        {
        	return Double.Parse(s.Replace(',', '.'), CultureInfo.InvariantCulture);
        	/*
            Double d;
            CultureInfo c1 = Thread.CurrentThread.CurrentCulture, c2 = Thread.CurrentThread.CurrentUICulture;

            // Sets the culture to En
            Thread.CurrentThread.CurrentCulture = MyTools.cEN;
            // Sets the UI culture to En
            Thread.CurrentThread.CurrentUICulture = MyTools.cEN;

            try
            {
                d = Convert.ToDouble(s.Replace(",", "."));

                // Sets the culture to French (France)
                Thread.CurrentThread.CurrentCulture = c1;
                // Sets the UI culture to French (France)
                Thread.CurrentThread.CurrentUICulture = c2;
            }
            catch (Exception)
            {
                // Sets the culture to French (France)
                Thread.CurrentThread.CurrentCulture = c1;
                // Sets the UI culture to French (France)
                Thread.CurrentThread.CurrentUICulture = c2;
                throw;
            }

            return d;*/
        }

        /// <summary>
        /// Get MD5 value of a file
        /// </summary>
        /// <param name="file">file (read only)</param>
        /// <returns>MD5 value</returns>
        public static String GetMD5(String file)
        {
        	try
        	{
	            using (var md5 = MD5.Create())
	            {
	                using (var stream = File.OpenRead(file))
	                {
	                    byte[] data = md5.ComputeHash(stream);
	
	                    // Create a new Stringbuilder to collect the bytes
	                    // and create a string.
	                    StringBuilder sBuilder = new StringBuilder();
	
	                    // Loop through each byte of the hashed data 
	                    // and format each one as a hexadecimal string.
	                    for (int i = 0; i < data.Length; i++)
	                    {
	                        sBuilder.Append(data[i].ToString("x2"));
	                    }
	
	                    // Return the hexadecimal string.
	                    return sBuilder.ToString();
	                }
	            }
        	}
        	catch(Exception)
        	{
        		return "";
        	}
        }

        /// <summary>
        /// Get node value of an XmlNode
        /// </summary>
        /// <param name="node">node</param>
        /// <param name="key">element key</param>
        /// <returns>node value</returns>
        public static String getNodeValue(XmlNode node, String key)
        {
            XmlElement elt = node[key];
            if (elt == null)
                return "";
            else
                return elt.InnerText.Trim();
        }

        /// <summary>
        /// Get node attribute value of an XmlNode
        /// </summary>
        /// <param name="node">node</param>
        /// <param name="key">attribute key</param>
        /// <returns>attribute value</returns>
        public static String getAttributeValue(XmlNode node, String key)
        {
            XmlAttribute att = node.Attributes[key];
            if (att == null)
                return "";
            else
                return att.InnerText.Trim();
        }

        /// <summary>
        /// Get node attribute value of an XmlNode
        /// </summary>
        /// <param name="node">node</param>
        /// <param name="nodeKey">element key</param>
        /// <param name="attkey">attribute key</param>
        /// <returns>attribute value</returns>
        public static String getAttributeValue(XmlNode node, String nodeKey, String attkey)
        {
            XmlElement elt = node[nodeKey];
            if (elt == null)
                return "";

            XmlAttribute att = elt.Attributes[attkey];
            if (att == null)
                return "";
            else
                return att.InnerText.Trim();
        }

        /// <summary>
        /// Draw first image on top of second image
        /// </summary>
        /// <param name="firstImage">first image</param>
        /// <param name="secondImage">second image</param>
        /// <returns>new merged image</returns>
        public static Image MergeTwoImages(Image firstImage, Image secondImage)
        {
            Bitmap bitmap = new Bitmap(secondImage.Width, secondImage.Height);
            Graphics canvas = Graphics.FromImage(bitmap);
            canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
            canvas.DrawImage(secondImage, new Rectangle(0, 0, secondImage.Width, secondImage.Height), new Rectangle(0, 0, secondImage.Width, secondImage.Height), GraphicsUnit.Pixel);
            float ix = 0f, iy = 0f;
            if (firstImage.Width < secondImage.Width)
                ix = (float)(secondImage.Width - firstImage.Width) / 2f;
            if (firstImage.Height < secondImage.Height)
                iy = (float)(secondImage.Height - firstImage.Height) / 2f;
            canvas.DrawImage(firstImage, ix, ix);
            canvas.Save();
            return bitmap;
        }

        /// <summary>
        /// Resize an image, preserving aspect ratio and center it on the frame
        /// </summary>
        /// <param name="imgPhoto"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <returns></returns>
        public static Image FixedSize(Image imgPhoto, int Width, int Height)
	    {
	        int sourceWidth = imgPhoto.Width;
	        int sourceHeight = imgPhoto.Height;
	        int sourceX = 0;
	        int sourceY = 0;
	        int destX = 0;
	        int destY = 0;
	
	        float nPercent = 0;
	        float nPercentW = 0;
	        float nPercentH = 0;
	
	        nPercentW = ((float)Width / (float)sourceWidth);
	        nPercentH = ((float)Height / (float)sourceHeight);
	        if (nPercentH < nPercentW)
	        {
	            nPercent = nPercentH;
	            destX = System.Convert.ToInt16((Width -
	                          (sourceWidth * nPercent)) / 2);
	        }
	        else
	        {
	            nPercent = nPercentW;
	            destY = System.Convert.ToInt16((Height -
	                          (sourceHeight * nPercent)) / 2);
	        }
	
	        int destWidth = (int)(sourceWidth * nPercent);
	        int destHeight = (int)(sourceHeight * nPercent);
	
	        Bitmap bmPhoto = new Bitmap(Width, Height,
	                          PixelFormat.Format24bppRgb);
	        bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
	                         imgPhoto.VerticalResolution);
	
	        Graphics grPhoto = Graphics.FromImage(bmPhoto);
	        grPhoto.Clear(Color.White);
	        grPhoto.InterpolationMode =
	                InterpolationMode.HighQualityBicubic;
	
	        grPhoto.DrawImage(imgPhoto,
	            new Rectangle(destX, destY, destWidth, destHeight),
	            new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
	            GraphicsUnit.Pixel);
	
	        grPhoto.Dispose();
	        return bmPhoto;
	    }
        
        /// <summary>
        /// Resize an image
        /// </summary>
        /// <param name="img">image</param>
        /// <param name="nWidth">new width</param>
        /// <param name="nHeight">new height</param>
        /// <returns>resized image</returns>
        public static Bitmap ResizeImage(Image img, int nWidth, int nHeight)
        {
            Bitmap result = new Bitmap(nWidth, nHeight);
            using (Graphics g = Graphics.FromImage((Image)result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, nWidth, nHeight);
            }
            return result;
        }

        /// <summary>
        /// Decode base64 string into an image
        /// </summary>
        /// <param name="base64String">base64 string</param>
        /// <returns>decoded image</returns>
        public static Image Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0,
              imageBytes.Length);

            // Convert byte[] to Image
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
        }


        /// <summary>
        /// Case insensitive contain on a string
        /// </summary>
        /// <param name="text">text</param>
        /// <param name="value">value to search</param>
        /// <returns>true is value is present in text</returns>
        public static bool CaseInsensitiveContains(string text, string value)
        {
            return text.IndexOf(value, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        /// <summary>
        /// Encode an image in Base64
        /// </summary>
        /// <param name="image">image to encode</param>
        /// <param name="format">image format</param>
        /// <returns>encoded image in Base64</returns>
        public static string ImageToBase64(Image image, System.Drawing.Imaging.ImageFormat format)
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

        /// <summary>
        /// Create an image from a label
        /// </summary>
        /// <param name="sImageText">text label</param>
        /// <returns>created image</returns>
        public static Bitmap CreateBitmapImage(string sImageText)
        {
            return CreateBitmapImage(sImageText, Color.Black, Color.Gold);
        }

        /// <summary>
        /// Create an image from a label
        /// </summary>
        /// <param name="sImageText">text label</param>
        /// <param name="penColor">color for pen (forecolor)</param>
        /// <param name="backColor">background color</param>
        /// <returns>created image</returns>
        public static Bitmap CreateToolbarImage(string sImageText, Color penColor, Color backColor)
        {
            Bitmap objBmpImage = new Bitmap(1, 1);

            int w = 23;
            int h = 22;
            
            int intWidth = 0;
            int intHeight = 0;

            // Create the Font object for the image text drawing.
            Font objFont = new Font("Smallest Pixel-7", 9f);//"Arial", 10, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);

            // Create a graphics object to measure the text's width and height.
            Graphics objGraphics = Graphics.FromImage(objBmpImage);

            // This is where the bitmap size is determined.
            String text = sImageText;
            do
            {
            	intWidth = (int)objGraphics.MeasureString(text, objFont).Width;
            	intHeight = (int)objGraphics.MeasureString(text, objFont).Height;
            	if (intWidth >= w)
            	{
            		text = text.Substring(0, text.Length - 1);
            	}
            }
            while(intWidth >= w);

            // Create the bmpImage again with the correct size for the text and font.
            objBmpImage = new Bitmap(objBmpImage, new Size(w, h));

            // Add the colors to the new bitmap.
            objGraphics = Graphics.FromImage(objBmpImage);

            // Set Background color
            //objGraphics.Clear(backColor);
            objGraphics.Clear(Color.Transparent);

            // Options
            objGraphics.SmoothingMode = SmoothingMode.AntiAlias;
            objGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            // Rectangle (rounded or not)
            Pen blackPen = new Pen(Color.Black, 1);
            Rectangle borders = new Rectangle(0, 0, w - 1, h - 1);
            SolidBrush brush = new SolidBrush(backColor);
            DrawRoundedRectangle(objGraphics, blackPen, 0, 0, w - 1, h - 1, 3, 3, brush);

            // Text
            float fx = (float)(w - intWidth)/2.0f;
            float fy = (float)(h - intHeight)/2.0f;
            objGraphics.DrawString(text, objFont, new SolidBrush(penColor), fx , fy);

            objGraphics.Flush();

            return (objBmpImage);
        }
        
        /// <summary>
        /// Create an image from a label
        /// </summary>
        /// <param name="sImageText">text label</param>
        /// <param name="penColor">color for pen (forecolor)</param>
        /// <param name="backColor">background color</param>
        /// <returns>created image</returns>
        public static Bitmap CreateBitmapImage(string sImageText, Color penColor, Color backColor)
        {
            Bitmap objBmpImage = new Bitmap(1, 1);

            int intWidth = 0;
            int intHeight = 0;

            // Create the Font object for the image text drawing.
            Font objFont = new Font("Corbel", 14, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);

            // Create a graphics object to measure the text's width and height.
            Graphics objGraphics = Graphics.FromImage(objBmpImage);

            // This is where the bitmap size is determined.
            intWidth = (int)objGraphics.MeasureString(sImageText, objFont).Width + 3; // +2 for borders
            intHeight = (int)objGraphics.MeasureString(sImageText, objFont).Height + 3; // +2 for borders

            // Create the bmpImage again with the correct size for the text and font.
            objBmpImage = new Bitmap(objBmpImage, new Size(intWidth, intHeight));

            // Add the colors to the new bitmap.
            objGraphics = Graphics.FromImage(objBmpImage);

            // Set Background color
            //objGraphics.Clear(backColor);
            objGraphics.Clear(Color.Transparent);

            // Options
            objGraphics.SmoothingMode = SmoothingMode.AntiAlias;
            objGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            // Rectangle (rounded or not)
            Pen blackPen = new Pen(Color.Black, 1);
            Rectangle borders = new Rectangle(0, 0, intWidth - 1, intHeight - 1);
            //objGraphics.DrawRectangle(blackPen, borders);
            SolidBrush brush = new SolidBrush(backColor);
            DrawRoundedRectangle(objGraphics, blackPen, 0, 0, intWidth - 1, intHeight - 1, 3, 3, brush);

            // Text
            objGraphics.DrawString(sImageText, objFont, new SolidBrush(penColor), 1, 1);

            objGraphics.Flush();

            return (objBmpImage);
        }

        /// <summary>
        /// Draw a rounded rectangle on a Graphic
        /// </summary>
        /// <param name="g">graphic</param>
        /// <param name="p">pen</param>
        /// <param name="x">x origin in pixels</param>
        /// <param name="y">y origin in pixels</param>
        /// <param name="w">width in pixels</param>
        /// <param name="h">height in pixels</param>
        /// <param name="rx">round x radius in pixels</param>
        /// <param name="ry">round y radius in pixels</param>
        /// <param name="b">brush</param>
        public static void DrawRoundedRectangle(Graphics g, Pen p, int x, int y, int w, int h, int rx, int ry, Brush b)
        {
            System.Drawing.Drawing2D.GraphicsPath path
              = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(x, y, rx + rx, ry + ry, 180, 90);
            path.AddLine(x + rx, y, x + w - rx, y);
            path.AddArc(x + w - 2 * rx, y, 2 * rx, 2 * ry, 270, 90);
            path.AddLine(x + w, y + ry, x + w, y + h - ry);
            path.AddArc(x + w - 2 * rx, y + h - 2 * ry, rx + rx, ry + ry, 0, 91);
            path.AddLine(x + rx, y + h, x + w - rx, y + h);
            path.AddArc(x, y + h - 2 * ry, 2 * rx, 2 * ry, 90, 91);
            path.CloseFigure();
            if (b != null)
                g.FillPath(b, path);
            g.DrawPath(p, path);
        }

        /// <summary>
        /// Find duplicate words in a list
        /// </summary>
        /// <param name="v">list of words, separated by a ' ', ',', ';', '.', '\''</param>
        /// <param name="iMinLength">minimum word length</param>
        /// <returns>list of duplicate words, separated by '\r\n'</returns>
        static public string FindDuplicateWords(string v, int iMinLength)
        {
            // 1
            // Keep track of words found in this Dictionary.
            var d = new Dictionary<string, bool>();

            // 2
            // Build up string into this StringBuilder.
            StringBuilder b = new StringBuilder();

            // 3
            // Split the input and handle spaces and punctuation.
            string[] a = v.Split(new char[] { ' ', ',', ';', '.', '\'' },
                StringSplitOptions.RemoveEmptyEntries);

            // 4
            // Loop over each word
            foreach (string current in a)
            {
                // 5
                // Lowercase each word
                string lower = current.ToLower();

                // 6
                // If we haven't already encountered the word,
                // append it to the result.
                if ((lower.Length >= iMinLength) && (!d.ContainsKey(lower)))
                {
                    b.Append(current).Append("\r\n");
                    d.Add(lower, true);
                }
            }
            // 7
            // Return the duplicate words removed
            return b.ToString().Trim();
        }

        /// <summary>
        /// Get a subset from a string, between 2 tags
        /// exemple:
        /// "toto is a weird guy"
        /// tag1 = "is a"
        /// tag2 = " guy"
        /// result = " weird"
        /// If the subset is not found, return ""
        /// </summary>
        /// <param name="tag1">beginning tag</param>
        /// <param name="tag2">end tag (if "", search will only be performed on tag1)</param>
        /// <param name="text">text to look into</param>
        /// <returns>If the subset is not found, return "", otherwise a matching subset</returns>
        public static String GetSnippetFromText(String tag1, String tag2, String text)
        {
            // Recherche la première occurence d'une chaine contenue entre tag1 et tag2 dans text
            String r = "";
            if (text != "")
            {

                int i1 = text.IndexOf(tag1);
                if (i1 != -1)
                {
                	int l1 = i1 + tag1.Length;
                    if (tag2 != "")
                    {
                        int i2 = text.IndexOf(tag2, l1);
                        if (i2 != -1)
                        {
                            r = text.Substring(l1, i2 - l1);
                        }
                    }
                    else
                    {
                        r = text.Substring(l1);
                    }
                }
            }
            return r;
        }

        /// <summary>
        /// Get all subsets from a string, between 2 tags
        /// exemple:
        /// "toto is a weird guy, really is a stupid guy, I think it is a dummy guy"
        /// tag1 = "is a"
        /// tag2 = " guy"
        /// result = " weird", " stupid", " dummy"
        /// If the subset is not found, return ""
        /// </summary>
        /// <param name="tag1">beginning tag</param>
        /// <param name="tag2">end tag (if "", search will only be performed on tag1)</param>
        /// <param name="text">text to look into</param>
        /// <returns>If the subset is not found, return "", otherwise the list of matching subsets</returns>
        public static List<String> GetSnippetsFromText(String tag1, String tag2, String text)
        {
            // Recherche toutes les chaines contenues entre tag1 et tag2 dans text
            List<String> r = new List<string>();
            int istart = 0;
            int i1 = -1;
            if (text != "")
            {
                do
                {
                    i1 = text.IndexOf(tag1, istart);
                    if (i1 != -1)
                    {
                    	int l1 = i1 + tag1.Length;
                        if (tag2 != "")
                        {
                            // présence d'un tag de fin
                            int i2 = text.IndexOf(tag2, i1 + 1);
                            if (i2 != -1)
                            {
                                r.Add(text.Substring(l1, i2 - l1));
                                istart = i2 + tag2.Length; // on repart pour un tour
                            }
                            else
                            {
                                // pas de tag de fin trouvé, on arrête là
                                i1 = -1;
                            }
                        }
                        else
                        {
                            // pas de tag de fin fourni
                            r.Add(text.Substring(l1));
                            i1 = -1; // on stoppe là
                        }
                    }
                }
                while (i1 != -1);

            }
            return r;
        }

        /// <summary>
        /// Execute a regular expression
        /// </summary>
        /// <param name="input">input text</param>
        /// <param name="param">regular expression</param>
        /// <returns>result</returns>
        public static String DoRegex(String input, String param)
        {
            Match match = Regex.Match(input, param, RegexOptions.IgnoreCase);

            // Here we check the Match instance.
            if (match.Success)
            {
                // Finally, we get the Group value and display it.
                return match.Groups[1].Value;
            }
            else
                return "s";
        }

        /// <summary>
        /// Check if a point is inside a polygon
        /// </summary>
        /// <param name="p">point to check</param>
        /// <param name="poly">polygon</param>
        /// <returns>true if point is present inside the polygon</returns>
        public static bool PointInPolygon(PointDouble p, PointDouble[] poly)
        {
            PointDouble p1, p2;

            bool inside = false;

            if (poly.Length < 3)
            {
                return inside;
            }

            PointDouble oldPoint = new PointDouble(poly[poly.Length - 1].X, poly[poly.Length - 1].Y);

            for (int i = 0; i < poly.Length; i++)
            {
                PointDouble newPoint = new PointDouble(poly[i].X, poly[i].Y);

                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if ((newPoint.X < p.X) == (p.X <= oldPoint.X)
                && ((double)p.Y - (double)p1.Y) * (double)(p2.X - p1.X)
                 < ((double)p2.Y - (double)p1.Y) * (double)(p.X - p1.X))
                {
                    inside = !inside;
                }

                oldPoint = newPoint;
            }

            return inside;
        }
        
        /// <summary>
        /// Check write access on a path (not a file, a path!)
        /// </summary>
        /// <param name="path">path to check</param>
        /// <param name="exc">return exception</param>
        /// <returns>true if write access is granted</returns>
        public static bool CheckWriteAccess(String path, ref Exception exc)
        {
            try
            {
                String touch = path + Path.DirectorySeparatorChar + Guid.NewGuid().ToString();
                StreamWriter f = new System.IO.StreamWriter(touch);
                f.Write("@");
                f.Close();
                File.Delete(touch);
            }
            catch (Exception ex)
            {
                exc = ex;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Clean a date and remove what's after 'T'
        /// E.g.: yyyy-MM-ddT00:00:1Z ==> yyyy-MM-dd
        /// </summary>
        /// <param name="date">date to clean</param>
        /// <returns>cleaned date</returns>
        public static String CleanDate(String date)
        {
            int pos = date.IndexOf('T');
            if (pos != -1)
                return date.Substring(0, pos);
            else
                return date;
        }

        /// <summary>
        /// Create a bitmap image
        /// </summary>
        /// <param name="sImageText">Text of the image</param>
        /// <param name="penColor">pen color (foreground color)</param>
        /// <param name="backColor">bckground color</param>
        /// <param name="bBig">if true, text will use a big font</param>
        /// <returns>create bitmap</returns>
        static public Bitmap CreateBitmapImage(string sImageText, Color penColor, Color backColor, bool bBig)
        {
            Bitmap objBmpImage = new Bitmap(1, 1);

            int intWidth = 0;
            int intHeight = 0;

            // Create the Font object for the image text drawing.
            Font font = (bBig) ? objFontBig : objFont;

            // Create a graphics object to measure the text's width and height.
            Graphics objGraphics = Graphics.FromImage(objBmpImage);

            // This is where the bitmap size is determined.
            intWidth = (int)objGraphics.MeasureString(sImageText, font).Width + 3; // +2 for borders
            intHeight = (int)objGraphics.MeasureString(sImageText, font).Height + 3; // +2 for borders

            // Create the bmpImage again with the correct size for the text and font.
            objBmpImage = new Bitmap(objBmpImage, new Size(intWidth, intHeight));

            // Add the colors to the new bitmap.
            objGraphics = Graphics.FromImage(objBmpImage);

            // Set Background color
            //objGraphics.Clear(backColor);
            objGraphics.Clear(Color.Transparent);

            // Options
            //objGraphics.SmoothingMode = SmoothingMode.AntiAlias;
            //objGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            // Rectangle (rounded or not)

            Rectangle borders = new Rectangle(0, 0, intWidth - 1, intHeight - 1);
            //objGraphics.DrawRectangle(blackPen, borders);
            SolidBrush brush = new SolidBrush(backColor);
            DrawRoundedRectangle(objGraphics, blackPen, 0, 0, intWidth - 1, intHeight - 1, 3, 3, brush);

            // Text
            objGraphics.DrawString(sImageText, font, new SolidBrush(penColor), 1, 1);

            objGraphics.Flush();

            return (objBmpImage);
        }

        static void MergeArray(ref byte[] array1, byte[] array2)
        {
            int array1OriginalLength = array1.Length;
            int nl = array1OriginalLength + array2.Length;
            Array.Resize<byte>(ref array1, nl);
            Array.Copy(array2, 0, array1, array1OriginalLength, array2.Length);
        }

        static byte[] GetArrayDegrees(double dd)
        {
            double mm, ss, tt;
            CoordConvHMI.ConvertDegreesToDDMMSSTT(dd, out mm, out ss, out tt);
            byte[] one = BitConverter.GetBytes(1);

            byte[] bdd = BitConverter.GetBytes((int)dd);
            byte[] bmm = BitConverter.GetBytes((int)mm);
            byte[] bss = BitConverter.GetBytes((int)ss);
            MergeArray(ref bdd, one);
            MergeArray(ref bdd, bmm);
            MergeArray(ref bdd, one);
            MergeArray(ref bdd, bss);
            MergeArray(ref bdd, one);
            return bdd;
        }

        /// <summary>
        /// Wrtie coordinates into an image exif information
        /// </summary>
        /// <param name="Filename">image file</param>
        /// <param name="dLat">latitude</param>
        /// <param name="dLong">longitude</param>
        static public void WriteCoordinatesToImage(string Filename, double dLat, double dLong)
        {
            byte[] bLat = GetArrayDegrees(dLat);
            byte[] bLon = GetArrayDegrees(dLong);
            byte[] bLaNS = (dLat > 0) ? BitConverter.GetBytes('N') : BitConverter.GetBytes('S');
            byte[] bLoEW = (dLong > 0) ? BitConverter.GetBytes('E') : BitConverter.GetBytes('W');

            using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(Filename)))
            {
                File.Delete(Filename);
                using (Image Pic = Image.FromStream(ms))
                {
                    PropertyItem pi = Pic.PropertyItems[0];
                    pi.Id = 0x0004;
                    pi.Type = 5;
                    pi.Len = bLon.Length;
                    pi.Value = bLon;
                    Pic.SetPropertyItem(pi);

                    pi.Id = 0x0002;
                    pi.Value = bLat;
                    Pic.SetPropertyItem(pi);

                    pi.Id = 0x0003;
                    pi.Type = 2;
                    pi.Len = bLoEW.Length;
                    pi.Value = bLoEW;
                    Pic.SetPropertyItem(pi);

                    pi.Id = 0x0001;
                    pi.Value = bLaNS;
                    Pic.SetPropertyItem(pi);

                    Pic.Save(Filename);
                }
            }
        }

        /// <summary>
        /// Load exif information from an image
        /// </summary>
        /// <param name="FileName">image file</param>
        /// <returns>raw exif data</returns>
        static public String LoadExif(string FileName)
        {
            Image Pic = Image.FromFile(FileName);
            PropertyItem[] PropertyItems;
            PropertyItems = Pic.PropertyItems;
            String s = "";
            foreach (PropertyItem p in PropertyItems)
            {
                /*
                PropertyItems[0].Id = 0x0002;
                PropertyItems[0].Type = 5;
                PropertyItems[0].Len = bLong.Length;
                PropertyItems[0].Value = bLong;
                */
                s += String.Format("Id:{0} Type:{1} Len:{2}\r\n", p.Id, p.Type, p.Len);
            }
            Pic.Dispose();
            return s;
        }

        /// <summary>
        /// Remove diacritics (accents) from a string
        /// </summary>
        /// <param name="stIn">string to clean</param>
        /// <returns>cleaned strig</returns>
        static public string RemoveDiacritics(string stIn)
        {
            string stFormD = stIn.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }

        /// <summary>
        /// Change font style
        /// </summary>
        /// <param name="org">font to change</param>
        /// <param name="bold">true for bold</param>
        /// <param name="italic">true for italic</param>
        /// <returns>new font</returns>
        public static Font ChangeFontStyle(Font org, bool bold, bool italic)
        {
            FontStyle style = org.Style;
            if (bold) style |= FontStyle.Bold;
            else style &= ~FontStyle.Bold;
            if (italic) style |= FontStyle.Italic;
            else style &= ~FontStyle.Italic;
            return new Font(org, style);
        }
        
        /// <summary>
        /// Get coordinate of a city, using http://maps.google.com/maps/api/geocode/json?address={0}
        /// </summary>
        /// <param name="proxy">web proxy (can be null)</param>
        /// <param name="cityName">city name</param>
        /// <param name="coord"></param>
        /// <param name="cityFound"></param>
        /// <returns>true if found</returns>
        public static bool GetCoordinate(WebProxy proxy, string cityName, out Coordinate coord, out string cityFound)
        {
            string apiUrl, jsonString;
            cityFound = "";
            coord = null;
            
            try
            {
	            apiUrl = string.Format(@"http://maps.google.com/maps/api/geocode/json?address={0}&sensor=false", cityName);
	            WebClient client = new WebClient();
	            client.Proxy = proxy;
	            jsonString = client.DownloadString(apiUrl);
	
	            // Les coordonnées
	            string value = MyTools.GetSnippetFromText("\"location\" : {", "}", jsonString);
	            String lat = MyTools.GetSnippetFromText("\"lat\" : ", ",", value);
	            String lon = MyTools.GetSnippetFromText("\"lng\" : ", "", value);
	
	            // La ville
	            value = GetSnippetFromText("\"address_components\"", "],", jsonString);
	            List<String> cityvals = GetSnippetsFromText("\"long_name\" : ", "\",", value);
	            for(int i=0;i<cityvals.Count;i++)
	            {
	                cityFound += cityvals[i].Replace("\"", "");
	                if (i != (cityvals.Count - 1))
	                    cityFound += ", ";
	            }
	            coord = new Coordinate(ConvertToDouble(lat), ConvertToDouble(lon));
	            return true;
            }
            catch(Exception)
            {
            	return false;
            }
        }

        /// <summary>
        /// Convert first character of a string to uppercase
        /// </summary>
        /// <param name="s">string</param>
        /// <returns>converted string</returns>
        public static string FirstCharToUpper(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        /// <summary>
        /// Convert a date into a string
        /// Date format shall be yyyy-dd-MMTHH:mm:ss
        /// </summary>
        /// <param name="de">date (format yyyy-dd-MMTHH:mm:ss)</param>
        /// <returns>converted date</returns>
        public static DateTime ParseDate(String de)
        {
            DateTime dateExport = DateTime.Now;
            if (DateTime.TryParse(de, out dateExport) == false)
            {
                // Shit, a stupid date. Inverted month & days ?
                if (DateTime.TryParseExact(de, "yyyy-dd-MMTHH:mm:ss", null, DateTimeStyles.None, out dateExport) == false)
                {
                    dateExport = DateTime.Now;
                }
            }
            return dateExport;
        }

        /// <summary>
        /// Returns approximate distance in kilometers between two points
        /// </summary>
        /// <param name="lat1">first point latitude</param>
        /// <param name="lon1">first point longitude</param>
        /// <param name="lat2">second point latitude</param>
        /// <param name="lon2">second point longitude</param>
        /// <returns>distance in kilometers</returns>
        static public double DistanceBetweenPoints(double lat1, double lon1, double lat2, double lon2)
        {
            double e = _toRad * lat1;
            double f = _toRad * lon1;
            double g = _toRad * lat2;
            double h = _toRad * lon2;
            double i = (Math.Cos(e) * Math.Cos(g) * Math.Cos(f) * Math.Cos(h) + Math.Cos(e) * Math.Sin(f) * Math.Cos(g) * Math.Sin(h) + Math.Sin(e) * Math.Sin(g));
            double j = (Math.Acos(i));
            double k = (_earthRadiusKm * j);
            return k;
        }

        /// <summary>
        /// Convert a distance (longitude_distance) expressed in degrees, expressed on a longitude direction,
        /// in kilometers, assuming your are at latitude latitude_position
        /// </summary>
        /// <param name="longitude_distance">distance in degreees to convert</param>
        /// <param name="latitude_position">position in latitude (degrees) at which computation shall be done</param>
        /// <returns>distance in kilometers</returns>
        static public double DegreeToKilometer(double longitude_distance, double latitude_position)
        {
            return _toRadearthRadiusKm * Math.Cos(_toRad * latitude_position) * longitude_distance;
        }

        /// <summary>
        /// Convert a distance (kilometer_distance) expressed in kilometers, expressed on a longitude direction,
        /// in degrees, assuming your are at latitude latitude_position
        /// </summary>
        /// <param name="kilometer_distance">distance in kilometers to convert</param>
        /// <param name="latitude_position">position in latitude (degrees) at which computation shall be done</param>
        /// <returns>distance in degrees</returns>
        static public double KilometerToDegree(double kilometer_distance, double latitude_position)
        {
            return kilometer_distance / (_toRadearthRadiusKm * Math.Cos(_toRad * latitude_position));
        }

        /// <summary>
        /// Replace invalid characters in a filename with ' '
        /// </summary>
        /// <param name="filename">filename to clean</param>
        /// <returns>clean filename</returns>
        static public String SanitizeFilename(String filename)
        {
            String f = filename;

            // Get a list of invalid file characters. 
            char[] invalidFileChars = Path.GetInvalidFileNameChars();
            foreach (char someChar in invalidFileChars)
            {
                f = f.Replace(someChar, ' ');
            }

            return f;
        }

        /// <summary>
        /// Get url content (ISO-8859-1 format)
        /// </summary>
        /// <param name="uri">url</param>
        /// <param name="proxy">web proxy (can be null)</param>
        /// <param name="timeoutMilliseconds">timout delay in milliseconds</param>
        /// <returns>url content</returns>
        static public string GetRequestWithEncoding(Uri uri, WebProxy proxy, int timeoutMilliseconds)
        {
            var request = System.Net.WebRequest.Create(uri);
            if (proxy != null)
                request.Proxy = proxy;
            request.Timeout = timeoutMilliseconds;
            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new System.IO.StreamReader(stream, Encoding.GetEncoding("ISO-8859-1")))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Get url content (ISO-8859-1 format)
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="proxy">web proxy (can be null)</param>
        /// <param name="timeoutMilliseconds">timout delay in milliseconds</param>
        /// <returns>HttpWebResponse</returns>
        static public HttpWebResponse GetHttpRequestWithEncoding(String url, WebProxy proxy, int timeoutMilliseconds)
        {
        	try
        	{
        		var uri = new Uri("url");
	            var request = System.Net.WebRequest.Create(uri);
	            if (proxy != null)
	                request.Proxy = proxy;
	            request.Timeout = timeoutMilliseconds;
	            return (HttpWebResponse)request.GetResponse();
        	}
        	catch(Exception)
        	{
        		return null;
        	}
        }
        
        /// <summary>
        /// Get url content (standard encoding)
        /// </summary>
        /// <param name="uri">url</param>
        /// <param name="proxy">web proxy (can be null)</param>
        /// <param name="timeoutMilliseconds">timout delay in milliseconds</param>
        /// <returns>url content</returns>
        static public string GetRequest(Uri uri, WebProxy proxy, int timeoutMilliseconds)
        {
            var request = System.Net.WebRequest.Create(uri);
            if (proxy != null)
                request.Proxy = proxy;
            if (timeoutMilliseconds != -1)
                request.Timeout = timeoutMilliseconds;
            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new System.IO.StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary >
        /// Ratings (Lower bound of Wilson score confidence interval for a Bernoulli parameter)
        /// </summary>    
        /// <param name="positive">Positive ratings</param>
        /// <param name="negative">Negative ratings</param>
        /// <returns>rating</returns>    
        static public double Rating(double positive, double negative)
        {
        	// Pas d'optimisation, le compilo s'en charge
            return (((positive + 1.9208) / (positive + negative) - 1.96 * Math.Sqrt(((positive * negative) / (positive + negative)) + 0.9604) / (positive + negative)) / (1.0 + 3.8416 / (positive + negative)));
        }

        /// <summary>
        /// Rating (simple formula)
        /// rating = positive / (positive + negative)
        /// </summary>
        /// <param name="positive">Positive ratings</param>
        /// <param name="negative">Negative ratings</param>
        /// <returns>rating</returns>
        static public double RatingSimple(double positive, double negative)
        {
            return (positive / (positive + negative));
        }

        /// <summary>
        /// Convert HTML to XML (replace ampersand, inferior and superior)
        /// </summary>
        /// <param name="s">HTML</param>
        /// <returns>XML</returns>
        public static String HtmlToXml(String s)
        {
            String val = s;
            val = val.Replace("&", "&amp;");
            val = val.Replace("<", "&lt;");
            val = val.Replace(">", "&gt;");
            return val;
        }

        /// <summary>
        /// Insensitive search in string list
        /// </summary>
        /// <param name="liste">list of string</param>
        /// <param name="keyword">string to search</param>
        /// <returns>true if string is present in list</returns>
        public static bool InsensitiveContainsInStringList(List<String> liste, String keyword)
        {
            if (liste.FindIndex(x => x.Equals(keyword, StringComparison.OrdinalIgnoreCase)) != -1)
                return true;
            else
                return false;
        }

        /// <summary>
        /// check if a point is inside a circle
        /// </summary>
        /// <param name="center_x">Center X</param>
        /// <param name="center_y">Center Y</param>
        /// <param name="radius">Radius</param>
        /// <param name="x">Point X</param>
        /// <param name="y">Point Y</param>
        /// <returns>true if point is inside a circle or center Center and radius Radius</returns>
        public static bool PointInCircle(double center_x, double center_y, double radius, double x, double y)
        {
            double square_dist = (center_x - x)*(center_x - x) + (center_y - y)*(center_y - y);
            return (square_dist <= radius * radius);
        }


        /// <summary>
        /// Download a binary file
        /// </summary>
        /// <param name="url">Url of file</param>
        /// <param name="proxy">Proxy (can be null)</param>
        /// <param name="cookie">Cookie (can be null)</param>
        /// <param name="localFile">Local file to download url in</param>
        public static void DownloadFile(String url, WebProxy proxy, CookieContainer cookie, String localFile )
        {
            
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
            objRequest.Proxy = proxy;
            objRequest.CookieContainer = cookie;
            HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();

            // on efface le fichier tmp s'il existe
            if (File.Exists(localFile))
                File.Delete(localFile);

            using (Stream output = File.OpenWrite(localFile))
            using (Stream input = objResponse.GetResponseStream())
            {
                byte[] buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    output.Write(buffer, 0, bytesRead);
                }
                output.Close();
                input.Close();
            }            
        }


        /// <summary>
        /// Recursively find the named control within a ContextMenuStrip
        /// </summary>
        /// <param name="parent">Parent ContextMenuStrip to look for within</param>
        /// <param name="name">Name of the control to look for</param>
        /// <returns>Found ToolStripMenuItem (or null)</returns>
        public static ToolStripMenuItem FindControl(ContextMenuStrip parent, string name)
        {
            // Recursively search the parent's children.
            foreach (object mnu in parent.Items)
            {
                if (mnu.GetType() == typeof(ToolStripMenuItem))
                {
                    ToolStripMenuItem found = MyTools.FindControl((ToolStripMenuItem)mnu, name);
                    if (found != null)
                        return found;
                }
            }

            // If we still haven't found it, it's not here.
            return null;
        }

        /// <summary>
        /// Recursively find the named control within a ToolStripMenuItem
        /// </summary>
        /// <param name="parent">Parent ToolStripMenuItem to look for within</param>
        /// <param name="name">Name of the control to look for</param>
        /// <returns>Found ToolStripMenuItem (or null)</returns>
        public static ToolStripMenuItem FindControl(ToolStripMenuItem parent, string name)
        {
            // Check the parent.
            if ((parent.Name != null) && (parent.Name == name)) return parent;

            // Recursively search the parent's children.
            foreach (object ctl in parent.DropDownItems)
            {
                if (ctl.GetType() == typeof(ToolStripMenuItem))
                {
                    ToolStripMenuItem found = MyTools.FindControl((ToolStripMenuItem)ctl, name);
                    if (found != null)
                        return found;
                }
            }

            // If we still haven't found it, it's not here.
            return null;
        }

        /// <summary>
        /// Recursively find the named control. Including within MenuStrip
        /// </summary>
        /// <param name="parent">Parent to look for within</param>
        /// <param name="name">Name of the control to look for</param>
        /// <returns>Found object (or null)</returns>
        public static Object FindControl(Control parent, string name)
        {
            // Check the parent.
            if ((parent.Name != null) && (parent.Name == name)) return parent;

            // Recursively search the parent's children.
            foreach (Control ctl in parent.Controls)
            {
                
                Object found = MyTools.FindControl(ctl, name);
                if (found != null)
                    return found;
                else
                {
                    // Is the control a MenuStrip ?
                    if (ctl.GetType() == typeof(MenuStrip))
                    {
                        MenuStrip mnu = (MenuStrip)ctl;
                        foreach (ToolStripMenuItem item in mnu.Items)
                        {
                            found = MyTools.FindControl(item, name);
                            if (found != null)
                                return found;
                        }
                    }
                }
                
            }

            // If we still haven't found it, it's not here.
            return null;
        }
        
        /// <summary>
        /// Recursively list controls. Including within MenuStrip
        /// </summary>
        /// <param name="parent">Parent to look for within. Can be a control, menustrip or toolstripmenuitem</param>
        /// <param name="lobjs">List of objects to complete. CANNOT BE NULL!</param>
        public static void ListControls(Object parent, ref List<Object> lobjs)
        {
            // Check the parent.
            if (parent == null)
            	return;

            // Recursively search the parent's children.
            ContextMenuStrip cmparent = parent as ContextMenuStrip;
            if (cmparent != null)
            {
            	// A ContextMenuStrip
            	// We add the control
				lobjs.Add(cmparent);
				
				// Look for sons						
        		foreach (Object item in cmparent.Items)
                {
        			MyTools.ListControls(item, ref lobjs);
                }
            }
            else
            {
	            ToolStripMenuItem tsparent = parent as ToolStripMenuItem;
	            if (tsparent != null)
	            {
	            	// A ToolStripMenuItem
	            	// We add the control
					lobjs.Add(tsparent);
					
					// Look for sons						
	        		foreach (Object item in tsparent.DropDownItems)
	                {
	        			MyTools.ListControls(item, ref lobjs);
	                }
	            }
	            else
	            {
                    ToolStripItem tsiparent = parent as ToolStripItem;
                    if (tsiparent != null)
                    {
                        // it can be a separator or a tslabel
                        // We add the control
                        lobjs.Add(tsiparent);
                    }
                    else
                    {
                        MenuStrip mnuparent = parent as MenuStrip;
                        if (mnuparent != null)
                        {
                            // A MenuStrip
                            // We add the control
                            lobjs.Add(mnuparent);

                            // Look for sons						
                            foreach (Object item in mnuparent.Items)
                            {
                                MyTools.ListControls(item, ref lobjs);
                            }
                        }
                        else
                        {
                            Control ctrlparent = parent as Control;
                            if (ctrlparent != null)
                            {
                                // A Control
                                // We add the control
                                lobjs.Add(ctrlparent);

                                // Look for sons
                                foreach (Control ctl in ctrlparent.Controls)
                                {
                                    MyTools.ListControls(ctl, ref lobjs);
                                }
                            }
                        }
                    }
	            }
            }
        }
        
        /// <summary>
        /// Check if lists are equal
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="list1">First list</param>
        /// <param name="list2">Second list</param>
        /// <returns>True if list equals</returns>
        public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }
        
        /// <summary>
        /// Check if a type is generic
        /// </summary>
        /// <param name="genericType">Type to check against</param>
        /// <param name="instance">instance</param>
        /// <returns>true if genetic type</returns>
        static public bool IsInstanceOfGenericType(Type genericType, object instance)
	    {
	        Type type = instance.GetType();
	        while (type != null)
	        {
	            if (type.IsGenericType &&
	                type.GetGenericTypeDefinition() == genericType)
	            {
	                return true;
	            }
	            type = type.BaseType;
	        }
	        return false;
	    }
        
        /// <summary>
        /// Full details of this object
        /// </summary>
        /// <param name="o">object to inspect</param>
        /// <param name="prefix">added in front of each line</param>
        /// <returns>Return full details on object</returns>;
        public static String ObjectDetails(Object o, string prefix)
        {
        	String s = "";
        	try
        	{
        		Type type = o.GetType(); // Get type pointer
				FieldInfo[] fields = type.GetFields(); // Obtain all fields
				foreach (var field in fields) // Loop through fields
				{
				    string name = field.Name; // Get string name
				    object temp = field.GetValue(o); // Get value
				    s += prefix + name + " : ";
				    if (temp != null)
				    {
				    	if (MyTools.IsInstanceOfGenericType(typeof(List<>),temp))
				    	{
				    		//s += prefix + "it is a list\r\n";
				    		IEnumerable ie = temp as IEnumerable;
				    		s += "\r\n";
				    		foreach(object oie in ie)
				    		{
				    			s += prefix + "     " + oie.ToString() + "\r\n";//Details(oie, prefix + "   ");
				    		}
				    	}
						else
					    	s += prefix + temp.ToString() + "\r\n";
				    }
				    else
				    	s += prefix + "null\r\n";
				}
        	}
        	catch(Exception e)
        	{
        		s += prefix + e.Message + "\r\n";
        	}
        	return s;
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
        
        #pragma warning disable 169
		#pragma warning disable 649
		struct _IMAGE_FILE_HEADER
		{
		    public ushort Machine;
		    public ushort NumberOfSections;
		    public uint TimeDateStamp;
		    public uint PointerToSymbolTable;
		    public uint NumberOfSymbols;
		    public ushort SizeOfOptionalHeader;
		    public ushort Characteristics;
		};
		
		/// <summary>
		/// Usage var buildTime = BuildDate.GetBuildDateTime(Assembly.GetExecutingAssembly());
		/// </summary>
		/// <param name="assembly">assembly to inspect</param>
		/// <returns></returns>
		public static DateTime GetBuildDateTime(Assembly assembly)
		{
		    var path = new Uri(assembly.GetName().CodeBase).LocalPath;
		
		    if (!File.Exists(path)) return new DateTime(1990,1,1);
		
		    var headerDefinition = typeof (_IMAGE_FILE_HEADER);
		
		    var buffer = new byte[Math.Max(Marshal.SizeOf(headerDefinition), 4)];
		    using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
		    {
		        fileStream.Position = 0x3C;
		        fileStream.Read(buffer, 0, 4);
		        fileStream.Position = BitConverter.ToUInt32(buffer, 0); // COFF header offset
		        fileStream.Read(buffer, 0, 4); // "PE\0\0"
		        fileStream.Read(buffer, 0, buffer.Length);
		    }
		    var pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
		    try
		    {
		        var addr = pinnedBuffer.AddrOfPinnedObject();
		        var coffHeader = (_IMAGE_FILE_HEADER)Marshal.PtrToStructure(addr, headerDefinition);
		
		        var epoch = new DateTime(1970, 1, 1);
		        var sinceEpoch = new TimeSpan(coffHeader.TimeDateStamp * TimeSpan.TicksPerSecond);
		        var buildDate = epoch + sinceEpoch;
		        var localTime = TimeZone.CurrentTimeZone.ToLocalTime(buildDate);
		
		        return localTime;
		    }
		    finally
		    {
		        pinnedBuffer.Free();
		    }
		}
		
		/// <summary>
		/// Get value of a regedit key
		/// </summary>
		/// <param name="path">path in HKEY_LOCAL_MACHINE</param>
		/// <param name="key">key</param>
		/// <returns>value or null if not found</returns>
		public static String GetRegValue(String path, String key)
		{
			RegistryKey registryKey = null;
	        registryKey = Registry.LocalMachine.OpenSubKey(path);
	        if (registryKey != null)
	        {
	        	Object val = registryKey.GetValue(key);
	        	return ((val == null)?null:val.ToString());
	        }
	        else
	        	return null;
		}
		
		/// <summary>
		/// Returns highest installed framework
		/// </summary>
		/// <returns>highest installed framework (such as 4.5.2)</returns>
		static public String GetHighestInstalledFramework()
		{
			try
			{
				// On teste le Fwk 4
				String val = GetRegValue(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full", "Release");
				if (String.IsNullOrEmpty(val))
				{
					// On teste le 3.5
					val = GetRegValue(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5", "Install");
					if (String.IsNullOrEmpty(val))
						return "";
					else
					{
						if (val == "1")
							return "3.5";
						else
							return "";
					}
				}
				else
				{
					String fwk = DisplayExactFrameworkVersion(val);
					if ((fwk != "") && (fwk != "unknown"))
					{
						int pos = fwk.IndexOf(' ');
						if (pos != -1)
							fwk = fwk.Substring(0, pos);
						return fwk;
					}
					else
					{
						// On tente de se baser sur la version du coup
                        val = GetRegValue(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full", "Version");
                        return val;
					}
				}
			}
			catch(Exception)
			{
				return "";
			}
		}
		
		/// <summary>
		/// Return various informations on OS and installed frameworks
		/// </summary>
		/// <returns>info</returns>
		public static String GetOSAssembliesFrameworkInfo()
		{
			String s = "";
			try
			{
				s += "Operating System Details" + "\r\n";
	            OperatingSystem os = Environment.OSVersion;
	            s += "OS Version: " + os.Version.ToString() + "\r\n";
	            s += "OS Platform: " + os.Platform.ToString() + "\r\n";
	            s += "OS SP: " + os.ServicePack.ToString() + "\r\n";
	            s += "OS Version String: " + os.VersionString.ToString() + "\r\n";
	           
	            // Get Version details
	            Version ver = os.Version;
	            s += "Major version: " + ver.Major + "\r\n";
	            s += "Major Revision: " + ver.MajorRevision + "\r\n";
	            s += "Minor version: " + ver.Minor + "\r\n";
	            s += "Minor Revision: " + ver.MinorRevision + "\r\n";
	            s += "Build: " + ver.Build + "\r\n";
	            String vers = ver.Major + "." + ver.MajorRevision;
	            s += "Windows ";
	            switch(vers)
	            {
	            	case "10.0":
	            		s += "10";
	            		break;
	            	case "6.3":
	            		s += "8.1";
	            		break;
	            	case "6.2":
	            		s += "8";
	            		break;
	            	case "6.1":
	            		s += "7";
	            		break;
	            	case "6.0":
	            		s += "Vista";
	            		break;
	            	case "5.2":
	            		s += "XP Pro 64";
	            		break;
	            	case "5.1":
	            		s += "XP";
	            		break;
	            	default:
	            		s += " ? or to old to consider";
	            		break;
	            }
	            s += "\r\n";
	            s += "PROCESSOR_ARCHITECTURE: " + System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE").ToString() + "\r\n";

	            
	            String val;
	            s += "\r\n.NET 3.5\r\n";
	            DisplayValue(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5", "Install", ref s);            
	            DisplayValue(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5", "Version", ref s);            
	            DisplayValue(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5", "SP", ref s);            
	           	
	            s += "\r\n.NET 4.0 FULL\r\n";
	            DisplayValue(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full", "Install", ref s);            
	            DisplayValue(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full", "Version", ref s);            
	            DisplayValue(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full", "SP", ref s);
	            DisplayValue(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full", "Release", ref s);
	            val = GetRegValue(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full", "Release");
	            String fwk = DisplayExactFrameworkVersion(val);
	            if (fwk != "")
	            	s += ".NET Framework " + fwk + "\r\n";
	            
	            s += "\r\nASSEMBLIES\r\n";
	            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
	            foreach(Assembly asm in asms)
	            {
	            	if (String.IsNullOrEmpty(asm.Location) == false)
	            	{
	            		s += asm.FullName + "\r\n";
	            	}
	            }
			}
			catch(Exception ex)
			{
				s += "\r\nEXCEPTION + " + ex.Message + "\r\n";
			}
            return s;
		}
		
		static String DisplayExactFrameworkVersion(String val)
		{
			if (val == null)
			{
				return "";
			}
			
			String s = "";
			switch(val)
			{
				case "378389":
					s += "4.5";
					break;
				case "378675":
					s += "4.5.1 installed with Windows 8.1 or Windows Server 2012 R2";
					break;
				case "378758":
					s += "4.5.1 installed on Windows 8, Windows 7 SP1, or Windows Vista SP2";
					break;
				case "379893":
					s += "4.5.2";
					break;
				case "393295":
					s += "4.6 on Windows 10";
					break;
				case "393297":
					s += "4.6 on NON Windows 10";
					break;
				case "394254":
					s += "4.6.1 on Windows 10 November Update";
					break;
				case "394271":
					s += "4.6.1 on NON Windows 10 November Update";
					break;
				case "394747":
					s += "4.6.2 on Windows 10 Insider Preview Build 14295";
					break;
				case "394748":
					s += "4.6.2 on NON Windows 10 Insider Preview Build 14295";
					break;
				case "394802":
                    s += "4.6.2 on Windows 10 Anniversarye Update ?";
                    break;
				default:
					s += "unknown";
					break;
			}
			return s;
		}
		
		static void DisplayValue(String path, String key, ref String s)
		{
			String val;
            val = GetRegValue(path, key);
            s += key + ": " + (String.IsNullOrEmpty(val)?"(null)":val.ToString()) + "\r\n";
		}
		
		/// <summary>
		/// Check if provided framework version is >= 4.5
		/// </summary>
		/// <param name="dotnet">version to check</param>
		/// <returns>true if dotnet >= 4.5</returns>
		static public bool VerifyMinFrameworkVersion(String dotnet)
        {
        	String mindotnet = "4.5";
        	if (String.Compare(dotnet,mindotnet) < 0)
        	{
        		// Zut...
        		String url = "https://www.microsoft.com/en-US/download/details.aspx?id=42642";
        		String paramtxt = "Open install webpage for .Net " + mindotnet;
        		String title = "Missing .Net Framework " + mindotnet;
        		String txt = String.Format(@".Net Framework required: {0}
.Net Framework installed : {1} !
You need to install at least .Net {2} in order MGM to work properly, otherwise unexpected results might occur.
Proceed anyway with MGM startup?", mindotnet, dotnet, mindotnet);
        		String btn1 = "Yes";
        		String btn2 = "No";
        		if (Thread.CurrentThread.CurrentUICulture.Name.StartsWith("fr"))
        		{
        			url = "https://www.microsoft.com/fr-FR/download/details.aspx?id=42642";
        			paramtxt = "Ouvrir la page d'installation de .Net " + mindotnet;
	        		title = "Framework .Net " + mindotnet + " manquant" ;
	        		txt = String.Format(@".Net Framework requis : {0}
.Net Framework installé : {1} !
Le Framework {2} doit être installé pour que MGM puisse fonctionner correctement, sinon des comportements innatendus peuvent se produire.
Poursuivre le lancement de MGM ?", mindotnet, dotnet, mindotnet);
	        		btn1 = "Oui";
	        		btn2 = "Non";
        		}
        		ParameterObject po = new ParameterObject(ParameterObject.ParameterType.Bool, "false", "checkbox", paramtxt);
        		DialogResult dialogResult = MyMessageBox.Show(
					txt,
					title,
					MessageBoxIcon.Question, 
					po,
					null,
					btn1,
					btn2);
        		
        		if (po.GetControlValueString() == "True")
        			MyTools.StartInNewThread(url);
        			
        		if (dialogResult == DialogResult.Yes)
        		{
        			return true;
        		}
        		else
        		{
        			// On quitte
        			return false;
        		}
        	}
        	return true;
        }
		
		/// <summary>
		/// Start a command with Process.Start
		/// </summary>
		/// <param name="cmd">command</param>
		public static void StartCmd(String cmd) 
		{
			try
			{
        		Process.Start(cmd);
			}
			catch(Exception ex)
			{
				MyMessageBox.Show(ex.Message, "StartCmd", MessageBoxIcon.Error, null);
			}
	    }
	
		/// <summary>
		/// Start a command with Process.Start in a new thread
		/// </summary>
		/// <param name="cmd">command</param>
	    public static void StartInNewThread(String cmd) 
	    {
	        var t = new Thread(() => StartCmd(cmd));
	        t.Start();
	    }
	    
	    private static double ExifGpsToDouble(PropertyItem propItemRef, PropertyItem propItem)
        {
            double degreesNumerator = BitConverter.ToUInt32(propItem.Value, 0);
            double degreesDenominator = BitConverter.ToUInt32(propItem.Value, 4);
            double degrees = degreesNumerator / (double)degreesDenominator;

            double minutesNumerator = BitConverter.ToUInt32(propItem.Value, 8);
            double minutesDenominator = BitConverter.ToUInt32(propItem.Value, 12);
            double minutes = minutesNumerator / (double)minutesDenominator;

            double secondsNumerator = BitConverter.ToUInt32(propItem.Value, 16);
            double secondsDenominator = BitConverter.ToUInt32(propItem.Value, 20);
            double seconds = secondsNumerator / (double)secondsDenominator;


            double coorditate = degrees + (minutes / 60d) + (seconds / 3600d);
            string gpsRef = System.Text.Encoding.ASCII.GetString(new byte[1] { propItemRef.Value[0] }); //N, S, E, or W
            if (gpsRef == "S" || gpsRef == "W")
                coorditate = coorditate * -1;
            return coorditate;
        }
        
        private static double? GetLatitude(Image targetImg)
        {
            try
            {
                //Property Item 0x0001 - PropertyTagGpsLatitudeRef
                PropertyItem propItemRef = targetImg.GetPropertyItem(1);
                //Property Item 0x0002 - PropertyTagGpsLatitude
                PropertyItem propItemLat = targetImg.GetPropertyItem(2);
                return ExifGpsToDouble(propItemRef, propItemLat);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
        
        private static double? GetLongitude(Image targetImg)
        {
            try
            {
                //Property Item 0x0003 - PropertyTagGpsLongitudeRef
                PropertyItem propItemRef = targetImg.GetPropertyItem(3);
                //Property Item 0x0004 - PropertyTagGpsLongitude
                PropertyItem propItemLong = targetImg.GetPropertyItem(4);
                return ExifGpsToDouble(propItemRef, propItemLong);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public static void getExifCoords(string filename, ref double? latitude, ref double? longitude)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            using (Image img = Image.FromStream(fs, true, false))
            {
                latitude = GetLatitude(img);
                if (latitude != null)
                    longitude = GetLongitude(img);
                else
                    longitude = null;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="html"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static string StripHtmlTags(string html, HtmlAgilityPack.HtmlDocument doc = null)
		{
        	String s = html;
        	try
        	{
			    if (String.IsNullOrEmpty(html)) return "";
			    if (doc == null)
			    	doc = new HtmlAgilityPack.HtmlDocument();
			    doc.LoadHtml(html);
			    s = HttpUtility.HtmlDecode(doc.DocumentNode.InnerText);
			    s = HtmlAgilityPack.HtmlEntity.DeEntitize(s);
			    return s;
        	}
        	catch(Exception)
        	{
        		return s;
        	}
		}
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string CleanStringOfNonDigits(string s)
		{
		  if (string.IsNullOrEmpty(s)) return s;
		  StringBuilder sb = new StringBuilder(s);
		  int j = 0 ;
		  int i = 0 ;
		  while ( i < sb.Length )
		  {
		    bool isDigit = char.IsDigit( sb[i] ) ;
		    if ( isDigit )
		    {
		      sb[j++] = sb[i++];
		    }
		    else
		    {
		      ++i ;
		    }
		  }
		  sb.Length = j;
		  string cleaned = sb.ToString();
		  return cleaned;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="deb"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static DateTime GetMiddleDate(DateTime deb, DateTime end)
        {
        	var delta = end - deb;
        	int daystoaddtodeb = (int)(delta.TotalDays / 2.0);
        	return deb.AddDays(daystoaddtodeb);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="decimation"></param>
        /// <returns></returns>
        public static List<T> DecimateList<T>(List<T> list, int decimation)
        {
			if (decimation <= 1)
        		return list;
			
        	List<T> newlist = new List<T>();
        	int nb = list.Count;
        	if (nb < decimation*3)
        		return list;
        	else
        	{
        		// On on crée une nouvelle liste
        		int index = 0;
        		while (index < nb)
        		{
        			newlist.Add(list[index]);
        			index += decimation;
        		}
        		
        		// Si le dernier élement n'est pas le dernier de list, on ajoute le dernier de list
        		if ((index - decimation) != (nb -1))
        		{
        			// On ajoute le dernier élement
        			newlist.Add(list[nb-1]);
        		}
        	}
        	return newlist;
        }
        
       /// <summary>
       /// 
       /// </summary>
       /// <param name="command"></param>
       /// <returns></returns>
		static public string ExecuteCommandSync(object command)
		{
		     try
		     {
		        // create the ProcessStartInfo using "cmd" as the program to be run,
		        // and "/c " as the parameters.
		        // Incidentally, /c tells cmd that we want it to execute the command that follows,
		        // and then exit.
		    	System.Diagnostics.ProcessStartInfo procStartInfo =
		        	new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);
		
		    	// The following commands are needed to redirect the standard output.
		    	// This means that it will be redirected to the Process.StandardOutput StreamReader.
		    	procStartInfo.RedirectStandardOutput = true;
		    	procStartInfo.UseShellExecute = false;
		    
		    	// Do not create the black window.
		    	procStartInfo.CreateNoWindow = true;
		    
		    	// Now we create a process, assign its ProcessStartInfo and start it
		    	System.Diagnostics.Process proc = new System.Diagnostics.Process();
		    	proc.StartInfo = procStartInfo;
		    	proc.Start();

		    	// Get the output into a string
		    	string result = proc.StandardOutput.ReadToEnd();
		    
		    	// Display the command output.
			    Console.WriteLine(result);
			    return result;
		      }
		      catch (Exception ex)
		      {
		      	// Log the exception
		      	return ex.Message;
		      }
		}
    }
}
