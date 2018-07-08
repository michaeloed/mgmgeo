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
		public string Name {get {return "Wherigo downloader";}}
		public string Description { get {return "A dirty way to download Wherigo cartridge";}}
		
		public void Initialize(MainWindow daddy)
		{
			_Daddy = daddy;
		}
		
		public bool DoIt()
		{
			if (_Daddy != null)
			{
				_Daddy.DownloadWherigo();
				return true;
			}
			else
				return false;
		}
		
		public void Close()
		{
		}
	}
}