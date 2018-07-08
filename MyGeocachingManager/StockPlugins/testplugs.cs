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

namespace MyGeocachingManager
{
	public class TestPlugin : IScript
	{
		private MainWindow _Daddy = null;
		public string Name {get {return "Test plugin";}}
		public string Description { get {return "A stupid plugin";}}
		
		public void Initialize(MainWindow daddy)
		{
			_Daddy = daddy;
		}
		
		public bool DoIt()
		{
			if (_Daddy != null)
			{
				_Daddy.MSG("je suis un plugin de test");
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