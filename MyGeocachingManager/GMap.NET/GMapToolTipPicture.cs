using System.Drawing;
using System.Drawing.Drawing2D;
using System;
using System.Runtime.Serialization;
using HtmlRenderer;
using MyGeocachingManager;
using System.Text;
using System.Globalization;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using MyGeocachingManager.Geocaching;
using SpaceEyeTools;

namespace GMap.NET.WindowsForms
{
	/// <summary>
	/// Tooltip that will display an image
	/// </summary>
	public class GMapToolTipPicture : GMapToolTip, ISerializable
	{
		int _width = 0;
		int _height = 0;
		Image _img = null;
		static Font objFont = new Font("Arial", 12, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
        static Brush brushText = new SolidBrush(Color.Black);
        static Brush brushTextW = new SolidBrush(Color.White);
        String _label = "";
        
		/// <summary>
		/// 
		/// </summary>
		/// <param name="label"></param>
		/// <param name="imagePath"></param>
		/// <param name="marker"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public GMapToolTipPicture(String label, String imagePath, GMapMarker marker, int width = 256, int height = 256)
			: base(marker)
		{
			_width = width;
			_height = height;
			_label = label;
			try
			{
				
				_img = Image.FromFile(imagePath);
				_img = MyTools.FixedSize(_img, _width, _height);
			}
			catch(Exception)
			{
				_img = null;
			}
		}
		
		/// <summary>
        /// Rendering method
        /// </summary>
        /// <param name="g">Graphic to render</param>
        public override void OnRender(Graphics g)
        {
        	if (_img != null)
            {
        		try
        		{
        			int offset = 0;
        			if (_label != "")
        			{
        				System.Drawing.SizeF st = g.MeasureString(_label, objFont);
        				g.FillRectangle(brushTextW, Marker.ToolTipPosition.X, Marker.ToolTipPosition.Y, _img.Width, (int)(st.Height + 0.5f));
        				g.DrawString(_label, objFont, brushText, Marker.ToolTipPosition.X + (_img.Width - (int)(st.Width + 0.5f))/2, Marker.ToolTipPosition.Y);
        				offset = (int)(st.Height + 0.5f);
        			}
	                g.DrawImage(_img, new Point(Marker.ToolTipPosition.X, Marker.ToolTipPosition.Y + offset));
        		}
        		catch(Exception)
        		{
        			
        		}
            }
        }
	}
}
