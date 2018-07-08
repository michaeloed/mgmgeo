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
    /// Custom GMapToolTip marker
    /// Used to render Geocache details "Geocaching.com" style
    /// </summary>
    [Serializable]
    public class GMapToolTipCustom : GMapToolTip, ISerializable
    {
        private Geocache _geo;
        private MainWindow _daddy;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        /// <param name="geo">Associated geocache</param>
        /// <param name="marker">Associated geocache marker</param>
        public GMapToolTipCustom(MainWindow daddy, Geocache geo, GMapMarker marker)
            : base(marker)
        {
            _daddy = daddy;
            _geo = geo;
            Stroke = new Pen(Color.FromArgb(140, Color.Navy));
            Stroke.Width = 3;
            this.Stroke.LineJoin = LineJoin.Round;
            this.Stroke.StartCap = LineCap.RoundAnchor;

            Fill = Brushes.Yellow;
        }

       
        /// <summary>
        /// Rendering method
        /// </summary>
        /// <param name="g">Graphic to render</param>
        public override void OnRender(Graphics g)
        {
            if (_geo != null)
            {
                String imgpath = _daddy.GetResourcesDataPath() + Path.DirectorySeparatorChar + "Img";
                String imgpath_clean = imgpath.Replace("\\", "/");
                

                String nameHtml = "";              
                if (_geo._Available.ToLower() == "true")
                    nameHtml = _geo._Name.Replace("'", "&#146;");
                else
                    nameHtml = "<span style=\"text-decoration:line-through;\">" + _geo._Name.Replace("'", " ") + "</span>";

                String stats = "";
                OfflineCacheData ocd = _geo._Ocd;
                if ((ocd != null) && (ocd.HasStats()))
                {
                    // On a des stats
                    double rating;
                    rating = (_daddy._bUseGCPopularity) ? ocd._dRatingSimple : ocd._dRating;
                    if (rating >= 0)
                    {
                        stats = "	<tr><td><img width=16 height=16 src='" + imgpath_clean + "/Fav.png'>" + ocd._iNbFavs.ToString() +
                            ",&nbsp;" + _daddy.GetTranslator().GetString("LVRating") + ":&nbsp;" + "</td>" +
                            "		<td>" +
                        "<img src='" + imgpath_clean + "/Ratios/" + "ratio_" + ((int)(ocd._dRating * 100.0)).ToString() + ".png'>" + rating.ToString("0.0%").ToString() + "</td></tr>";
                    }
                    else if (ocd._iNbFavs > 0)
                    {
                        stats = "	<tr><td><img width=16 height=16 src='" + imgpath_clean + "/Fav.png'>" + ocd._iNbFavs.ToString() + "</td>" +
                            "		<td>&nbsp;</td></tr>";
                    }
                }

                //String theType = "		<img width=16 height=16 src='" + imgpath_clean + "/Type/" + _geo._Type + ".gif'>";
                String theType = "		<img src='" + imgpath_clean + "/TypeCat/" + _daddy._geocachingConstants.GetDicoTypeSmallIcon()[_geo._Type] + ".png'>";

                if (MyTools.InsensitiveContainsInStringList(GeocachingConstants.GetSupportedCacheTypes(), _geo._Type) == false)
                    theType = "		<img width=16 height=16 src='" + imgpath_clean + "/Fail.png'>";
                String infpopup =
                    "<body><table border=0>" +
                    "	<tr><td><b>"+
					theType+
                    "		" + nameHtml + "</b>" +
					"	</td>"+
					"	<td align='right'><h4>" + _geo._Code + "</h4></td></tr>"+
                    stats +
                    "		<tr><td>" + _daddy.GetTranslator().GetString("HTMLACacheBy") + ":&nbsp;" + MyTools.RemoveDiacritics(_geo._PlacedBy).Replace("'", " ") + "</td>" +
                    "		<td>" + _daddy.GetTranslator().GetString("HTMLHidden") + ":&nbsp;" + MyTools.CleanDate(_geo._DateCreation) + "</td></tr>" +
                    "		<tr><td>" + _daddy.GetTranslator().GetString("HTMLDifficulty") + ":&nbsp;<img src='" + imgpath_clean + "/Star/" + _geo._D + ".gif'></td>" +
                    "		<td>" + _daddy.GetTranslator().GetString("HTMLTerrain") + ":&nbsp;<img src='" + imgpath_clean + "/Star/" + _geo._T + ".gif'></td></tr>" +
                	"		<tr><td>" + _daddy.GetTranslator().GetString("HTMLSize") + ":&nbsp;<img src='" + imgpath_clean + "/Size/" + _geo._Container + ".gif'></td>";
                
				if ((_geo._Ocd != null) && (_geo._Ocd._dAltiMeters != Double.MaxValue))
				{
					
					String salti = "";
                    if (_daddy._bUseKm)
                    {
                        salti = String.Format("{0:0.#}", _geo._Ocd._dAltiMeters) + " m";
                    }
                    else
                    {
                        salti = String.Format("{0:0.#}", _geo._Ocd._dAltiMeters * 3.2808399) + " ft";
                    }
					infpopup += "<td>" + _daddy.GetTranslator().GetString("HTMLAltitude") + ":&nbsp;" + salti + "</td>";
				}
				else
				{
                	infpopup += "<td>&nbsp;</td>";
				}
				infpopup += "</tr></table></body>";

                // A VIRER !!!!!!!!!!
                //_daddy.Log(infpopup);


                Image img = null;
                CssData css = CssData.Parse("body { font:8pt Tahoma } h3 { color: navy; font-weight:normal; }", true);
                img = HtmlRender.RenderToImage(infpopup, 330, 150, Color.White, css, null, null);
                
                System.Drawing.Size st = img.Size;
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(Marker.ToolTipPosition.X, Marker.ToolTipPosition.Y - st.Height/2, st.Width + TextPadding.Width, st.Height + TextPadding.Height);
                rect.Offset(Offset.X, Offset.Y);

                g.DrawImage(img, new Point(rect.X, rect.Y));
            }
        }

       
    }
}
