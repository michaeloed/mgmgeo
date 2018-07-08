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
	public class CompleteCaches : IScript
	{
		private MainWindow _Daddy = null;
		public string Name {get {return "Complete selected caches";}}
		public string Description { get {return "Will parse HTML page of an incomplete geocache to fill missing information";}}
		
		public void Initialize(MainWindow daddy)
		{
			_Daddy = daddy;
		}
		
		public bool DoIt()
		{
			if (_Daddy != null)
			{
				_Daddy.CompleteSelectedCaches();
				return true;
			}
			else
			{
				MessageBox.Show("Daddy is missing :-(");
				return false;
			}
		}
		
		public void Close()
		{
		}
	}
}