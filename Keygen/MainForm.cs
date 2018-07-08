/*
 * Created by SharpDevelop.
 * User: bcristiano
 * Date: 19/07/2016
 * Time: 16:17
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SpaceEyeTools;
using System.Text;

namespace Keygen
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		/// <summary>
        /// Random generator
        /// </summary>
        public static readonly Random random = new System.Random((int)DateTime.Now.Ticks);
        
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		void Button1Click(object sender, EventArgs e)
		{
			textBox2.Text = GenerateKey(textBox1.Text);
		}
		
		private static String _password = "*d£^$^ùd¨#{|¤-(6#5`+°'";
		private static String _prefix = "§/:;?";
		private static String _suffix = "~µ%¨!";
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="owner"></param>
		/// <returns></returns>
		static public String GenerateKey(String owner)
		{
			// On travaille avec le login en minuscule
			owner = _prefix + owner.ToLower() + _suffix;
		
			// on veut une clé de 256 avec le login planqué au milieu
			int random = Math.Max(0, 256 - owner.Length);
			String fin = "";
			String deb = "";
			if (random != 0)
			{
				deb = RandomString(random/2);
				fin = RandomString(random - random/2);
			}
			String toencrypt = deb + owner + fin;
			return StringCipher.Encrypt(toencrypt, _password);
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
	}
}
