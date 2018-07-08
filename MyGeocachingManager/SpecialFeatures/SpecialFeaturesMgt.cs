/*
 * Created by SharpDevelop.
 * User: bcristiano
 * Date: 19/07/2016
 * Time: 09:14
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Configuration;
using SpaceEyeTools;
using SpaceEyeTools.HMI;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MyGeocachingManager.SpecialFeatures
{
	/// <summary>
	/// Description of SpecialFeaturesMgt.
	/// </summary>
	static public class SpecialFeaturesMgt
	{
		private static String _password = "*d£^$^ùd¨#{|¤-(6#5`+°'";
		private static String _prefix = "§/:;?";
		private static String _suffix = "~µ%¨!";

		
		/// <summary>
		/// Handle special features
		/// </summary>
		/// <returns>true if they are enabled</returns>
		static public bool AreSpecialFeaturesEnabled()
		{
			String owner = ConfigurationManager.AppSettings["owner"];
			if (owner == "")
				return false;
			
			String key = ConfigurationManager.AppSettings["key"];
			bool bOk = false;
			try
			{
				owner = _prefix + owner.ToLower() + _suffix;
				String decrypted = StringCipher.Decrypt(key, _password);
				if (decrypted.Contains(owner))
					bOk = true;
			}
			catch(Exception)
			{
				bOk = false;
			}
			
			return bOk;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="owner"></param>
		/// <returns></returns>
		static private String GenerateKey(String owner)
		{
			// On travaille avec le login en minuscule
			owner = _prefix + owner.ToLower() + _suffix;
		
			// on veut une clé de 256 avec le login planqué au milieu
			int random = Math.Max(0, 256 - owner.Length);
			String fin = "";
			String deb = "";
			if (random != 0)
			{
				deb = MyTools.RandomString(random/2);
				fin = MyTools.RandomString(random - random/2);
			}
			String toencrypt = deb + owner + fin;
			return StringCipher.Encrypt(toencrypt, _password);
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="daddy"></param>
		/// <returns></returns>
		static public bool EnterSpecialFeaturesKey(MainWindow daddy)
		{
			String owner = ConfigurationManager.AppSettings["owner"];
			String key = ConfigurationManager.AppSettings["key"];
			
			List<ParameterObject> lst = new List<ParameterObject>();
            lst.Add(new ParameterObject(ParameterObject.ParameterType.Label, "", "owner", daddy.GetTranslator().GetString("LblSpecialFeaturesUser") + " " + owner));
            lst.Add(new ParameterObject(ParameterObject.ParameterType.TextBox, key, "key", daddy.GetTranslator().GetString("LblSpecialFeaturesKey")));

            ParametersChanger changer = new ParametersChanger();
            changer.Title = daddy.GetTranslator().GetString("enableAdvancedMGM");
            changer.BtnCancel = daddy.GetTranslator().GetString("BtnCancel");
            changer.BtnOK = daddy.GetTranslator().GetString("BtnOk");
            changer.ErrorFormater = daddy.GetTranslator().GetString("ErrWrongParameter");
            changer.ErrorTitle = daddy.GetTranslator().GetString("Error");
            changer.Parameters = lst;
            changer.Font = daddy.Font;
            changer.Icon = daddy.Icon;

            bool bOK = AreSpecialFeaturesEnabled();
            if (changer.ShowDialog() == DialogResult.OK)
            {
                daddy.UpdateConfFile("key", lst[1].Value);
                
                bOK = AreSpecialFeaturesEnabled();
	            if (bOK)
				{
					daddy.MsgActionOk(daddy, daddy.GetTranslator().GetString("LblGoodKey"));
				}
				else
				{
					daddy.MsgActionWarning(daddy, daddy.GetTranslator().GetString("LblBadKeyCheater"));
				}
            }
            
			return bOK;
		}
	}
}
