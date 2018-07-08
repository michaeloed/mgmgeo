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
using System.Collections;
using System.Net;
using SpaceEyeTools;
using SpaceEyeTools.HMI;
using System.Text.RegularExpressions;
using MyGeocachingManager.Geocaching;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace MyGeocachingManager
{
	public class CustomFilterCata : CacheFilter
    {
		double _dkm = 0.0;
		PointLatLng _center;
        public CustomFilterCata(double dkm, PointLatLng center)
        {
			_dkm = dkm;
			_center = center;
        }

        public override bool ToBeDisplayed(Geocache cache)
        {
		
			Double distance = MyTools.DistanceBetweenPoints(cache._dLatitude, cache._dLongitude, _center.Lat, _center.Lng);
			if (distance > _dkm)
				return true;
			else
				return false;
        }
    }
	
	public class Catastrophe : IScriptV2
	{
		private MainWindow _Daddy = null;
        public string Name { get { return "Simulateur de catastrophe"; } }
		public string Description { get {return "Permet de simuler des catastrophes";}}
        public string Version { get { return "1.3"; } }
        public string MinVersionMGM { get { return "3.1.6.0"; } }
		
		public String apophis = "iVBORw0KGgoAAAANSUhEUgAAABkAAAAZCAYAAADE6YVjAAAABHNCSVQICAgIfAhkiAAAAAFzUkdCAK7OHOkAAAAEZ0FNQQAAsY8L/GEFAAAACXBIWXMAAA3WAAAN1gGQb3mcAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAAiVJREFUSEu91ltLVFEYh/H5HPPxCqkMyqIDRESIQd0YdROdoJsmzDSK6iLLcKibIgoKUiK8kSm2OZFkqeWsnqdmYO3TOEPmgp+wda/373r3u5mphBAqYUeoooYGwhawjvWqXFU6AQmKbv5X1q0aYmLRDVulZki+RQNtneud2BVd96dhSPqXFt+NoxiEAV4fwRCy9/cgHRIHHMAZHIYBh3AaJ7EH8b5N5EM6ARY8hhqG29cncAUXsB/x3i7y7bJFnsCAiziFaZzFJfi327gFT5jdXyAf4jOwRZ7AgJsYxRtcxjjOo44nOI5sjYx0SPyQbZFFDJiCLVrADXgy2/YWr+C9cZ2MdIhjGj9kW+QJDHiJa/iKu3iNe1jELLw/rhXJt8sxdcMI/G9tkScw4APG8AM/0VktrOAqsvWQD5FjalE3+ZBtkScwYBllawPn+JGpVxzie2CLHuAOfMi2yBN0WRu/VsN6shTCXo4W1SsOke+BY/ocz+BDjluUWQasrSTh29JcWBtLUrXKQ+QQOGHv20pWHNBceBqWZ5iKqE73EPkeOKZfULCyAZ/mxsNi/WGqxuYh8j1wTJ2iaBUFzL8YDc2JmdT+3kLkWDum7VUWMDt9MKwON1J7ew+RI82ElgW8e7QvfJxkzjP7+gsR78F60swHPB4KjcnroTVQ/J70/+VhsPVnTJ2ipH4/fJ6YCt9H5ovvbX8ybstn/DZ8W/nbsv/4vStUfgNlEXHNKwcHhwAAAABJRU5ErkJggg==";
		
		public String fukushima = "iVBORw0KGgoAAAANSUhEUgAAABkAAAAZCAYAAADE6YVjAAAABHNCSVQICAgIfAhkiAAAAAFzUkdCAK7OHOkAAAAEZ0FNQQAAsY8L/GEFAAAACXBIWXMAAA3WAAAN1gGQb3mcAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAAYZJREFUSEu9ls0rBVEYxueP8JWiLOyUj3JtiJTF9VXsWFhcGxulWLASiyvdjYgSJSWl/Ae2bJRkY2NDMunaSPkobj/z3ntoZu4796u596mnpjPv8zxnznnPzFiAVdVOfU2E4+pObIeEQFv8xFf80wHOYNJXFBaT4m+ZJ9AKQqH4W85FWEsURFtCtBuhsuSQhm5oH4HWIWiJQmOPXicsOqRjFLaP4PXN6RsX3j/g4BSa+7M1RYWsbkEqZVwDcH0LTb1eXcEhm4fGpQDIZNzagkISe0at4NGG9V0Ym4HZFTg7h8sbrz5vSN8kfP8YRx8enqBt2FvvnAsm5rxjOUPqujJrHITBaV3nZ86Q5Q3jpuD5JTNrTednYEhkHD6/jKOCu3tdp1ENkRleXBm3AEgr5zqAbqohSwnjlAfxnWytxqyQ+Xj+A/cH6bpozKvX+B8ir4P9E6MuArJv0iDSiW5jN9MhtU5BbBEW1krnwJQeIFT3JGxWLKT8X8aKfOMr8rfidGGZ/7uwfgEd/FJMNm4YKAAAAABJRU5ErkJggg==";
		
		public String haiti = "iVBORw0KGgoAAAANSUhEUgAAABkAAAAZCAYAAADE6YVjAAAABHNCSVQICAgIfAhkiAAAAAFzUkdCAK7OHOkAAAAEZ0FNQQAAsY8L/GEFAAAACXBIWXMAAA3WAAAN1gGQb3mcAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAAltJREFUSEuVlk9IVFEUxl8ZtCqLdlERtAqCgoKi9gXRqiKCCKOCWgQR0iJa1qJNEbUocBMthohyyikd0dGCNK0xMwsqyzCdaRxQA9P+qH2d0+Hy3vOd++6bgR9z3znnnu+9c8/lXg+A99DDynoPKaJIQKNtM5BeqPsUOE+K83J+I1AOBKj0HAUaaYrmi6HM+T0a8BdoASEGLgPtW3WfgxSLWEsUpJQFuvfrPgdFFtEcEaa/Am/O6D4XiUQaltLy0Y9LpvldJBJ5sk1ERu76tuzacEwciUR6jgGTH4Gx577twyUgUx2Os5FIZOAK8Okarcuwbys1AfnD4Tgb3uBNwEbLegnizurcA/ydoQ1ZJbapQeBbxk/06rie4915Euk9CWgM36G3vyoJ+At4I/4aBZpWAw8WA7NTwMyklCyzDPgzDrw+Fc3DbW8tV/M6SloCHq2gBBNim8hTE2wHWjcA3/voSx5TyWqA/rPA0O1oDkPsmox1AO8vyj8/F9P0ZgeArr3SafkjwGiz7KHcpuh8Q6xI7wkqyw/gS5088+L31wJvz5H4BSnT3G+gnIvODRIrklkuScxO57LwOg3dAl4eEhuXrHN3eN58nC1cqAee7ZTxi4P0fE/2S/sWsT3dQf8L/HgNp0jXPuqoVTLmhOPd0kkNS6KxNpwi3K5mnF0jrfuzEI5x4RQJkl5EG3KOFrpN99uoSISZHqGdfEP32ahYhBe977Tus8EiiU5GQ+E+0LFL91n4fzImOuMNn69XdpYQqcS3FQOXqoKrkdxW6MBLdO8y5Dbq9nkE7l3w/gG1P9yaFcEVkwAAAABJRU5ErkJggg==";
		
		public String hiroshima = "iVBORw0KGgoAAAANSUhEUgAAABkAAAAZCAYAAADE6YVjAAAABHNCSVQICAgIfAhkiAAAAAFzUkdCAK7OHOkAAAAEZ0FNQQAAsY8L/GEFAAAACXBIWXMAAA3WAAAN1gGQb3mcAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAAhNJREFUSEtj+P//P8NXES6pX2Kcy36KcT4D0v8pxVBzloHMBZkPs+AVsiIq4lcg8xmAjGVoEnB8Wlb4/zVpQaxyJOBlDPiCaI+82H8nQ4P/VymwCGQ+yCcYEjsUxP8/l+QFW2JsbPzfEWjRFRnyLcKw5Ka0wH9ToMFBejr/NylKgC0B4RhdLRR1pGCsPpmjIgs2GGRRrYYKmB2hp42hjlgMt+QkMJJXKkmB8SognqEiB7eoRV2JOpa8kuD5Hw40CBY8qToa/2ci+QhkMbJGkHpgpKKI4cIowfUaqBHkYphFWdrq/2chWQRKDMdlRf7nAMVB8eanrwsO2m/iXCiGomOMOHkDtKhaU+V/oZYaGK9RlITHkYeB/n8zYyO4I0A4OTn5/+Gli/9/k+RBMQcZY414bLgGmgDQLfj69ev/+/fv/6+1NPn/XQy7j4iy5BIwj5gbYfoAZoGbmxtYbKKqPFb9RFlSBQw+QhaAMCieLsgIYegnaAkoUi2RfIHLAhgOBiaQr2gJgSifgFIZMRbAMCjPIesnypK9wDKMWAtAeIGyDIp+oiwB4XMzphJlAQhXAOMQWS/RloDwHCNERsWHU3Q0UfSRZAkIb1aQABf92AyHYfSkTLIlIPwCWLyUaKlitQBU1LyT4EZRj7dmJISfAS3bB0wUU1XlwEXRfGCEfxRHtQBWM+Ks46mEl9GntUL7dtd/BgBAnu4VsCoOJAAAAABJRU5ErkJggg==";
		
		public String katrina = "iVBORw0KGgoAAAANSUhEUgAAABkAAAAZCAYAAADE6YVjAAAABHNCSVQICAgIfAhkiAAAAAFzUkdCAK7OHOkAAAAEZ0FNQQAAsY8L/GEFAAAACXBIWXMAAA3WAAAN1gGQb3mcAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAAXxJREFUSEu9lrtKA0EUhvc5BJv0thaCVloIIuIT+Bb6CEkTEFEh4AULsfYC2ojESlTURBDEwkvcIohIzMyZ7jgHXZnNnhN213GLD5Zl5v/Pv2dmdgJEDAZVZaDUrWyXVDm0oAdC0iNd0v82UOV2zyBftEk/+EnADfCD1Q/sg69PJBGSSezluKrhpF7LzYSuxfSImEkVTrBj1J+Z1/uyyZzewTN4xAt4wga0sGleM3EJz3gKDzijN2WTiLyJ9uA2oUWwJlGiK3hhK3a5sYkp+bkdv6APEloEa0KMqVV8N59sxS4fpotTep3ViBBNptUG1u335aq/tgmpeuIQ7nBYLbEaEbl68mY6OKpWEvMkxJ5QtVKKXWjikKom5klkTpI1BSEmodXF7ZVjuMdZvYUjajkxT0JsvM++iCbu7udOgCO7qtL2RTRx4VJ5SeJCqWhHRykappVphaUyIeiIcZP0nrT9SG2yCPXf/nAnbT/IpIA/YxH/+EJuK/9/78LgC2oVDYMV7MfKAAAAAElFTkSuQmCC";
		
		public String tchernobyl = "iVBORw0KGgoAAAANSUhEUgAAABkAAAAZCAYAAADE6YVjAAAABHNCSVQICAgIfAhkiAAAAAFzUkdCAK7OHOkAAAAEZ0FNQQAAsY8L/GEFAAAACXBIWXMAAA3WAAAN1gGQb3mcAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAAgVJREFUSEu9lj1LQmEUx2/iklZkDaVQU9ELFT6aQ36ChD5BIBH0GRwEl5x0dGpoaEghQoS2Pkq0ODUIbU41nM7/kefe5z7PMYLK4XfuPf/zJtdzXwIiYoIc02PetPt70Af9uC8FrOgBI8ZN/AvQN8dneqKU8Ff02H5/iT4+5qjVWqazsw0ql7eoUNjTR/jQEZfqLN7YemLI/f0C5fNHVCwWp4I48qR6A1tfNLy/J7jJodjcgDjypHoDW1+0aTRWxOYGxKU6G7a+6HJ8vCsOgC7lu7D1RZeHh3TYWKno8kGX8l3Y+qLEzc0ivbwk9TmO8N2cabD1xWlgXR8f0z9Z2xhsfVHi7m4hvFQ4wpfyJNj6osTJyXb4XwD4Up4E28lJvb7Kd/O+x/PzvI4rdRAbAh864lId+pne4ZBmMxNrYri8XNfxi4v1mA4fOuK2bri+zvhDBoOUmIxfNRol6PMzoE5niSqVTX2EDx1xqa7fT/lDUGTfAzal0o5uaHIBfOhSPh419gaGQ0C1mhWLlMrT6+vkHjHAhy7ln59nY7lsI2c4TIoPxKurtViRAbqbi6ey+4PYRg7odtN8nQthEbZoPJZvPuj21qHu9tZ/ErCNCwCboZTShe32she3QXwyQFGtFq2tDVv5zYj9xyZJMZfT0016eoq2yUG/GWfyjp/F1wq8//zuouALdVJX7uQjIGMAAAAASUVORK5CYII=";
		
		public String vesuve = "iVBORw0KGgoAAAANSUhEUgAAABkAAAAZCAYAAADE6YVjAAAABHNCSVQICAgIfAhkiAAAAAFzUkdCAK7OHOkAAAAEZ0FNQQAAsY8L/GEFAAAACXBIWXMAAA3WAAAN1gGQb3mcAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAAgpJREFUSEu9lk1rE1EUhvM7+gck32OShQspburKRf9FXdQvBG27CySl6EKwIFjaRavWjxRDUSxCBVcKpYtgFyVroWLrpNGmSZNM8nrPzJ0kMzl3Jq2pBx5yc3PP+95z7pA7AQABrF8YQT64inxoT4AhIHSEHukKfWkQ3mcWDgGhK/QDsgJmQZfW6/Cp5p0EV4WJf4uqj2Jo55yCJ0+iaCxFHHMK9siE+8FB7XEMlVmt8729FoJ+K4nmykAm4tCZSaK5HDHFaNxYjOLgeqojWpmL42AihdZLqzrjRRjtN+rWKU2M5xFzt1QFiZBoeSoB41nENDRNxJkcP4yjPHOR1bDxbNeftGaKle4kzU9Cv9kdl25bY6qUy7fxNKFq7F2roOq43F5YE+r9UVbDUUaDfqO7cw4yoXXUNvsM3SgrqT+N4tekt4FNeTqB1qszHDxBT8/h/QQrbCJaefwgDrzl8208TUyEQPNzFsZOrp8vGT7Hhb8Jsf9V/M8xQfPceheDmegFqeoKmufWuxjMpPpDqrqi9pNf78LfZPOaVFTEp3E+rwd/k915qaaI4gKf14O3yccxwKhJNUW0Gla1XL5EbbIxCvwuSiWfqHwXG7rK6wj6Td6lgG9z1qGeJuplq7XvLzn1BGTSvRk/XAa27gKF9NnZvie6cKXXhG5G/zv+36A7/r+8rZz7excCfwGPpivu9GSA6wAAAABJRU5ErkJggg==";
		
		public Image Base64ToImage(string base64String)
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

		public void Initialize(MainWindow daddy)
		{
			try
			{
				_Daddy = daddy;
				
				ContextMenuStrip cms = _Daddy._cacheDetail.tabControlCD._mnuContextMenu;
				if (cms != null)
				{
					String tools = _Daddy.GetTranslator().GetString("FMenuTools");
					ToolStripMenuItem tso = null;
					foreach(ToolStripMenuItem ts in cms.Items)
					{
						if (ts.Text == tools)
						{
							tso = ts;
							break;
						}
					}
					if (tso != null)
					{
						ToolStripItem mnu;
						tso.DropDownItems.Add(new ToolStripSeparator());
						mnu = tso.DropDownItems.Add("Le bombardement atomique d'Hiroshima", Base64ToImage(hiroshima),DoCata);
						mnu.Tag = new ExploData(5, Color.FromArgb(60, 250, 22, 9));
						mnu = tso.DropDownItems.Add("Le tremblement de terre en Haïti et ses répliques", Base64ToImage(haiti),DoCata);
						mnu.Tag = new ExploData(17, Color.FromArgb(60, 174, 0, 255));
						mnu = tso.DropDownItems.Add("La fuite de la centrale nucléaire de Fukushima", Base64ToImage(fukushima),DoCata);
						mnu.Tag = new ExploData(20, Color.FromArgb(60, 21, 53, 255));
						mnu = tso.DropDownItems.Add("L'explosion de la centrale nucléaire de Tchernobyl", Base64ToImage(tchernobyl),DoCata);
						mnu.Tag = new ExploData(30, Color.FromArgb(60, 255, 255, 0));
						mnu = tso.DropDownItems.Add("L'éruption du Vésuve et ses conséquences", Base64ToImage(vesuve),DoCata);
						mnu.Tag = new ExploData(200, Color.FromArgb(60, 255, 175, 42));
						mnu = tso.DropDownItems.Add("La collision avec l'astéroïde 99942 Apophis (prévisions)", Base64ToImage(apophis),DoCata);
						mnu.Tag = new ExploData(325, Color.FromArgb(60, 255, 98, 255));
						mnu = tso.DropDownItems.Add("Le passage de l'ouragan Katrina aux Etats-Unis", Base64ToImage(katrina),DoCata);
						mnu.Tag = new ExploData(650, Color.FromArgb(60, 34, 245, 136));
					}
				}
			}
			catch(Exception)
			{
			}
		}

		public void DoCata(object sender, EventArgs e)
        {
			ToolStripItem mnu = sender as ToolStripItem;
            if (mnu != null)
            {
				ExploData explo = mnu.Tag as ExploData;
				Brush brush1 = new SolidBrush(explo.color);
				Pen pen1 = new Pen(explo.color, 2);
				
				GMapOverlay overlay = _Daddy._cacheDetail._gmap.Overlays[GMapWrapper.RESERVED2];
				overlay.IsVisibile = true;
				_Daddy.ClearOverlay_RESERVED2();
				
				GMapMarkerCircle circle;
				PointLatLng center = _Daddy._cacheDetail._gmap.Position;
				circle = new GMapMarkerCircle(
                                        _Daddy._cacheDetail._gmap,
                                        center,
                                        (int)(explo.radius * 1000.0),
                                        pen1,
                                        brush1,
                                        true);
				overlay.Markers.Add(circle);
				_Daddy._cacheDetail._gmap.Refresh();
				
				
				CustomFilterCata fltr = new CustomFilterCata(explo.radius, center);
                _Daddy.ExecuteCustomFilterSilent(fltr,true);
				_Daddy._cacheDetail._gmap.Position = center;
			}
			
		}
		
        public Dictionary<String, String> Functions
        {
            get
            {
                Dictionary<String, String> dico = new Dictionary<string, string>();
                dico.Add("Informations", "GetInfos");
                return dico;
            }
        }

        public void GetInfos()
        {
            String s;
            s = "Version : " + this.Version + "\r\n";
            s += "Nom : " + this.Name + "\r\n";
            s += "Description : " + this.Description + "\r\n";
            s += "Version minimale de MGM : " + this.MinVersionMGM;

            _Daddy.MSG(s);
        }
		
		public void Close()
		{
		}
	}

	public class ExploData
	{
		public int radius = 0;
		public Color color = Color.Black;
		
		public ExploData(int r, Color c)
		{
			radius = r;
			color = c;
		}
	}
}