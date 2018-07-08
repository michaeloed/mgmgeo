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
using SpaceEyeTools;
using SpaceEyeTools.HMI;
using SpaceEyeTools.EXControls;
using MyGeocachingManager.HMI;
using MyGeocachingManager.Geocaching;
using System.Collections;
using System.Net;

namespace MyGeocachingManager
{
	public class KmlExport : IScript
	{
		private MainWindow _Daddy = null;
		public string Name {get {return "Export displayed caches to Kml";}}
		public string Description { get {return "Create a KML export of displayed caches";}}
		
		public void Initialize(MainWindow daddy)
		{
			_Daddy = daddy;
		}
		
		private void exportKml()
		{
			SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "KML (*.kml)|*.kml";

            saveFileDialog1.RestoreDirectory = true ;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
				FileInfo fi = new FileInfo(saveFileDialog1.FileName);
				Directory.SetCurrentDirectory(fi.Directory.ToString());
				String fileRadix = fi.Name.ToString();
				System.IO.StreamWriter file = new System.IO.StreamWriter(fileRadix, false);
				List<Geocache> lst = _Daddy.GetDisplayedCaches();
				file.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
				file.WriteLine("<kml xmlns=\"http://www.opengis.net/kml/2.2\">");
				file.WriteLine("  <Document>");
				foreach (Geocache geo in lst)
				{
					String desc = geo._ShortDescriptionInHTML + "<br>" + geo._LongDescriptionInHTML;
				
					file.WriteLine("	<Placemark>");
					file.WriteLine("	  <name>" + geo._Code + " - " + geo._Name + "</name>");
					file.WriteLine("	  <description>");
					file.WriteLine("		<![CDATA[");
					file.WriteLine("		  <a href=" + geo._Url + ">link on Geocaching.com</a>");
					file.WriteLine("		]]>");
					file.WriteLine("	  </description>");
					file.WriteLine("	  <Point>");
					file.WriteLine("		<coordinates>" + geo._Longitude + "," + geo._Latitude + "</coordinates>");
					file.WriteLine("	  </Point>");
					file.WriteLine("	</Placemark>");
				}
				file.WriteLine("  </Document>");
				file.WriteLine("</kml>");
				
				file.Close();
				MyMessageBox.Show("Export done!","Done", MessageBoxIcon.Information, null);
			}
		}
		
		
		public bool DoIt()
		{
			if (_Daddy != null)
			{
				exportKml();
				
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