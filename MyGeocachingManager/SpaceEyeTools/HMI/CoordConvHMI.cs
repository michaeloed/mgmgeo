using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace SpaceEyeTools.HMI
{
    /// <summary>
    /// Form that perform coordinates conversion
    /// Also holds statis methods to perform conversions
    /// DD.DDDDDD : 48.769408 1.967473
    /// DD° MM.MMM : N 48° 46.164 E 01° 58.048
    /// DD° MM' SS.SSS : N 48° 46' 9.9 E 01° 58' 2.9
    /// </summary>
    public partial class CoordConvHMI : Form
    {
        /// <summary>
        /// Error value during a conversion
        /// </summary>
        static public String _sErrorValue = "#ERR";

        /// <summary>
        /// Constructor
        /// </summary>
        public CoordConvHMI()
        {
            InitializeComponent();

        }
        
        /// <summary>
        /// Load coordinates in decimal degrees
        /// E.g. N 48° 46.409 E 001° 58.029
        /// LoadCoordDDD(48.77348, 1.96715)
        /// </summary>
        /// <param name="lat">latitude</param>
        /// <param name="lon">longitude</param>
        public void LoadCoordDDD(double lat, double lon)
        {
            tbLatLonDDD.Text = lat.ToString().Replace(",", ".") + " " + lon.ToString().Replace(",", ".");
            DDDtoDDMMM();
            DDDtoDDMMSSS();
        }

        /// <summary>
        /// Load coordinates in this format:
        /// N 48° 46.164 E 01° 58.048
        /// </summary>
        /// <param name="lat">latitude</param>
        /// <param name="lon">longitude</param>
        public void LoadCoordDDMMM(string lat, string lon)
        {
            tbLatLonDDMMM.Text = lat.Replace(",", ".") + " " + lon.Replace(",", ".");
            DDMMMtoDDD();
            DDDtoDDMMSSS();
        }

        private void DDDtoDDMMSSS()
        {
            String c = tbLatLonDDD.Text;
            c = c.TrimStart(null);
            c = c.TrimEnd(null);
            int ipos = c.IndexOf(" ");
            String c1 = "";
            String c2 = "";
            if (ipos != -1)
            {
                c1 = c.Substring(0, ipos);
                c2 = c.Substring(ipos + 1);
            }

            tbLatLonDDMMSSS.Text = OneConvertDDD2DDMMSSS(c1, "S", "N", true) + " " + OneConvertDDD2DDMMSSS(c2, "W", "E", false);
        }

        static private String OneConvertDDD2DDMMSSS(String coord, String negative, String positive, bool bLat)
        {
            double lat;
            if (coord != "")
                coord = coord.Replace(",", ".");
            if (Double.TryParse(coord, NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out lat))
            {
                if (!CheckLonLatValidity(lat, bLat))
                    return CoordConvHMI._sErrorValue;

                int d = (int)lat;
                String NSEW = (d < 0) ? negative + " " : positive + " ";
                d = Math.Abs(d);

                double m = (Math.Abs(lat) - (double)d) * 60.0;
                double s = (Math.Abs(m) - (double)(int)m) * 60.0;
                return NSEW + String.Format(CultureInfo.InvariantCulture, "{0:00}", d) + "° " + String.Format(CultureInfo.InvariantCulture, "{0:00}", (int)m) + "' " +  String.Format(CultureInfo.InvariantCulture, "{0:0.0}", s);
            }
            else
            {
                return CoordConvHMI._sErrorValue;
            }
        }

        private void DDDtoDDMMM()
        {
            String c = tbLatLonDDD.Text;
            c = c.TrimStart(null);
            c = c.TrimEnd(null);
            int ipos = c.IndexOf(" ");
            String c1 = "";
            String c2 = "";
            if (ipos != -1)
            {
                c1 = c.Substring(0, ipos);
                c2 = c.Substring(ipos + 1);
            }
            tbLatLonDDMMM.Text = OneConvertDDD2DDMMM(c1, "S", "N", true) + " " + OneConvertDDD2DDMMM(c2, "W", "E", false);
        }

        private String OneConvertDDD2DDMMM(String coord, String negative, String positive, bool bLat)
        {
            double lat;
            if (coord != "")
                coord = coord.Replace(",", ".");
            if (Double.TryParse(coord, NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out lat))
            {
                if (!CheckLonLatValidity(lat, bLat))
                    return CoordConvHMI._sErrorValue;

                int d = (int)lat;
                String NSEW = (d < 0) ? negative + " " : positive + " ";
                d = Math.Abs(d);
                double m = (Math.Abs(lat) - (double)d) * 60.0;
                // String.Format("{0:0.00}", 123.4); 
                return NSEW + String.Format(CultureInfo.InvariantCulture, "{0:00}", d) + "° " + String.Format(CultureInfo.InvariantCulture, "{0:0.000}", m);
            }
            else
            {
                return CoordConvHMI._sErrorValue;
            }
        }

        /// <summary>
        /// converts coordinates from DDMMM to DDD
        /// From DD.DDDDDD : 48.769408 1.967473
        /// To DD° MM' SS.SSS : N 48° 46' 9.9 E 01° 58' 2.9
        /// </summary>
        /// <param name="LatLonDDMMSSS">coordinates to convert</param>
        /// <param name="sLat">output, N or S</param>
        /// <param name="sLon">output, E or W</param>
        /// <returns>true if coordinates are valid</returns>
        static public bool DDMMSSStoDDD(String LatLonDDMMSSS, ref String sLat, ref String sLon)
        {
            // Now it's tricky...
            String c = LatLonDDMMSSS;
            c = c.TrimStart(null);
            c = c.TrimEnd(null);

            int iNS = Math.Max(c.IndexOf("N"), c.IndexOf("S"));
            int iEW = Math.Max(c.IndexOf("E"), c.IndexOf("W"));
            if ((iNS == -1) || (iEW == -1))
                return false;

            String c1 = "";
            String c2 = "";
            if (iNS < iEW)
            {
                c1 = c.Substring(0, iEW);
                c2 = c.Substring(iEW);
            }
            else
            {
                c2 = c.Substring(0, iNS);
                c1 = c.Substring(iNS);
            }

            sLat = CoordConvHMI.OneConvertDDMMSSS2DDD(c1, "S", "N", true);
            sLon = CoordConvHMI.OneConvertDDMMSSS2DDD(c2, "W", "E", false);
            if ((sLat != CoordConvHMI._sErrorValue) && (sLon != CoordConvHMI._sErrorValue))
                return true;
            else
                return false;
        }


        /// <summary>
        /// converts coordinates from DDMMM to DDD
        /// From DD° MM.MMM : N 48° 46.164 E 01° 58.048
        /// To DD.DDDDDD : 48.769408 1.967473
        /// </summary>
        /// <param name="LatLonDDMMM">coordinates to convert</param>
        /// <param name="sLat">output, N or S</param>
        /// <param name="sLon">output, E or W</param>
        /// <returns>true if coordinates are valid</returns>
        static public bool DDMMMtoDDD(String LatLonDDMMM, ref String sLat, ref String sLon)
        {
            // Now it's tricky...
            String c = LatLonDDMMM;
            c = c.TrimStart(null);
            c = c.TrimEnd(null);

            int iNS = Math.Max(c.IndexOf("N"), c.IndexOf("S"));
            int iEW = Math.Max(c.IndexOf("E"), c.IndexOf("W"));
            if ((iNS == -1) || (iEW == -1))
                return false;

            String c1 = "";
            String c2 = "";
            if (iNS < iEW)
            {
                c1 = c.Substring(0, iEW);
                c2 = c.Substring(iEW);
            }
            else
            {
                c2 = c.Substring(0, iNS);
                c1 = c.Substring(iNS);
            }

            sLat = CoordConvHMI.OneConvertDDMMM2DDD(c1, "S", "N", true);
            sLon = CoordConvHMI.OneConvertDDMMM2DDD(c2, "W", "E", false);
            if ((sLat != CoordConvHMI._sErrorValue) && (sLon != CoordConvHMI._sErrorValue))
                return true;
            else
                return false;
        }

        private void DDMMMtoDDD()
        {
            // Now it's tricky...
            String c = tbLatLonDDMMM.Text;
            c = c.TrimStart(null);
            c = c.TrimEnd(null);
            
            int iNS = Math.Max(c.IndexOf("N"), c.IndexOf("S"));
            int iEW = Math.Max(c.IndexOf("E"), c.IndexOf("W"));
            if ((iNS == -1) || (iEW == -1))
                tbLatLonDDD.Text = CoordConvHMI._sErrorValue;
            
            String c1 = "";
            String c2 = "";
            if (iNS < iEW)
            {
                c1 = c.Substring(0, iEW);
                c2 = c.Substring(iEW);
            }
            else
            {
                c2 = c.Substring(0, iNS);
                c1 = c.Substring(iNS);
            }

            tbLatLonDDD.Text = CoordConvHMI.OneConvertDDMMM2DDD(c1, "S", "N", true) + " " + CoordConvHMI.OneConvertDDMMM2DDD(c2, "W", "E", false);
        }

        static private String OneConvertDDMMM2DDD(string coord, string negative, string positive, bool bLat)
        {
            try
            {
                // Lat
                String slat = coord;
                // north of south ?
                double sign = 1.0;
                if (slat.Contains(negative))
                    sign = -1.0;
                slat = slat.Replace(" ", "");
                slat = slat.Replace(positive, "");
                slat = slat.Replace(negative, "");
                int ipos = slat.IndexOf("°");
                if (ipos != -1)
                {
                    double d, m;
                    String sd = slat.Substring(0, ipos).Replace(",", ".");
                    if (Double.TryParse(sd, NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out d))
                    {
                        if (!CheckLonLatValidity(d, bLat))
                            return CoordConvHMI._sErrorValue;

                        // ok we have the degress
                        // No try to get the minutes
                        String sm = slat.Substring(ipos + 1).Replace(",", ".");
                        sm = sm.Replace("'", "");
                        if (Double.TryParse(sm, NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out m))
                        {
                        	d += m / 60.0;
                            d = d * sign;
                            
                           	// ATTENTION ON PEUT AVOIR
                        	// N 48° 456 par exemple
                        	// Il faut donc rechecker la lon/lat pour vérifier sa validité
                            if (!CheckLonLatValidity(d, bLat))
                            return CoordConvHMI._sErrorValue;
                            
                            return String.Format("{0:0.######}", d).Replace(",", ".");
                        }
                        else
                            return CoordConvHMI._sErrorValue;
                    }
                    else
                        return CoordConvHMI._sErrorValue;
                }
                else
                    return CoordConvHMI._sErrorValue;
            }
            catch (Exception)
            {
                return CoordConvHMI._sErrorValue;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DDDtoDDMMM();
            DDDtoDDMMSSS();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DDMMMtoDDD();
            DDDtoDDMMSSS();
        }

        private void DDMMSSStoDDD()
        {
            // Now it's tricky...
            String c = tbLatLonDDMMSSS.Text;
            c = c.TrimStart(null);
            c = c.TrimEnd(null);

            int iNS = Math.Max(c.IndexOf("N"), c.IndexOf("S"));
            int iEW = Math.Max(c.IndexOf("E"), c.IndexOf("W"));
            if ((iNS == -1) || (iEW == -1))
                tbLatLonDDD.Text = CoordConvHMI._sErrorValue;

            String c1 = "";
            String c2 = "";
            if (iNS < iEW)
            {
                c1 = c.Substring(0, iEW);
                c2 = c.Substring(iEW);
            }
            else
            {
                c2 = c.Substring(0, iNS);
                c1 = c.Substring(iNS);
            }

            tbLatLonDDD.Text = OneConvertDDMMSSS2DDD(c1, "S", "N", true) + " " + OneConvertDDMMSSS2DDD(c2, "W", "E", false);
        }

        static private String OneConvertDDMMSSS2DDD(string coord, string negative, string positive, bool bLat)
        {
            try
            {
                // Lat
                String slat = coord;
                // north of south ?
                double sign = 1.0;
                if (slat.Contains(negative))
                    sign = -1.0;
                slat = slat.Replace(" ", "");
                slat = slat.Replace(positive, "");
                slat = slat.Replace(negative, "");
                int ipos = slat.IndexOf("°");
                if (ipos != -1)
                {
                    double d, m, s;
                    String sd = slat.Substring(0, ipos).Replace(",", ".");
                    if (Double.TryParse(sd, NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out d))
                    {
                        if (!CheckLonLatValidity(d, bLat))
                            return CoordConvHMI._sErrorValue;

                        // ok we have the degress
                        // No try to get the minutes
                        int ipos2 = slat.IndexOf("'");
                        if (ipos2 != -1)
                        {
                            String sm = slat.Substring(ipos + 1, ipos2 - ipos).Replace(",", ".");
                            sm = sm.Replace("'", "");
                            if (Double.TryParse(sm, NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out m))
                            {
                                d += m / 60.0;

                                // Now the seconds
                                String ss = slat.Substring(ipos2+1).Replace(",", ".");
                                ss = ss.Replace("'", "");
                                ss = ss.Replace("\"", "");
                                
                                // Check again lat/lon
                                if (!CheckLonLatValidity(d, bLat))
                            		return CoordConvHMI._sErrorValue;
                                
                                if (Double.TryParse(ss, NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out s))
                                {
                                    d += s / (60.0 * 60.0);
                                    d = d * sign;
                                    
                                     // Check again lat/lon
                                	if (!CheckLonLatValidity(d, bLat))
                            			return CoordConvHMI._sErrorValue;
                                
                                    return String.Format("{0:0.######}", d).Replace(",", ".");
                                }
                                else
                                    return CoordConvHMI._sErrorValue;
                            }
                            else
                                return CoordConvHMI._sErrorValue;
                        }
                        else
                            return CoordConvHMI._sErrorValue;
                    }
                    else
                        return CoordConvHMI._sErrorValue;
                }
                else
                    return CoordConvHMI._sErrorValue;
            }
            catch (Exception)
            {
                return CoordConvHMI._sErrorValue;
            }
        }

        /// <summary>
        /// Convert decimal degrees to minutes, seconds, tenths of seconds
        /// </summary>
        /// <param name="decimal_degrees">value to convert</param>
        /// <param name="bLat">true if value is a latitude</param>
        /// <returns>converted coordinate</returns>
        static public String ConvertDegreesToDDMMSSTT(double decimal_degrees, bool bLat)
        {
            if (!CheckLonLatValidity(decimal_degrees, bLat))
                return CoordConvHMI._sErrorValue;

            
            // set decimal_degrees value here
            if (bLat)
                return OneConvertDDD2DDMMSSS(decimal_degrees.ToString().Replace(",", "."), "S", "N", true);
            else
                return OneConvertDDD2DDMMSSS(decimal_degrees.ToString().Replace(",", "."), "W", "E", false);
        }

        /// <summary>
        /// Convert one degree value in DDMM
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <param name="bLat">true if value is a latitude</param>
        /// <returns>converted coordinate</returns>
        static public String ConvertDegreesToDDMM(double value, bool bLat)
        {
            if (!CheckLonLatValidity(value, bLat))
                return CoordConvHMI._sErrorValue;

            double ov = value;
            int deg = (int)value;
            value = Math.Abs(value - deg);
            double min = value * 60;

            String s;
            if (bLat)
            {
                if (ov < 0d)
                    s = "S ";
                else
                    s = "N ";
                deg = Math.Abs(deg);
                s += deg.ToString() + "° " + String.Format("{0:0.###}", min);
            }
            else
            {
                if (ov < 0d)
                    s = "W ";
                else
                    s = "E ";
                deg = Math.Abs(deg);
                s += deg.ToString() + "° " + String.Format("{0:0.###}", min);
            }
            // Maudits français !
            s = s.Replace(",", ".");
            return s;
        }

        /// <summary>
        /// Convert decimal degrees to minutes, seconds, tenths of seconds
        /// NO SAFETY CHECK HERE !!!
        /// </summary>
        /// <param name="decimal_degrees">decimal degrees</param>
        /// <param name="minutes">minutes</param>
        /// <param name="seconds">seconds</param>
        /// <param name="tenths">tenths of seconds</param>
        static public void ConvertDegreesToDDMMSSTT(double decimal_degrees, out double minutes, out double seconds, out double tenths)
        {
            // set decimal_degrees value here
            minutes = (decimal_degrees - Math.Floor(decimal_degrees)) * 60.0;
            seconds = (minutes - Math.Floor(minutes)) * 60.0;
            tenths = (seconds - Math.Floor(seconds)) * 10.0;
            // get rid of fractional part
            minutes = Math.Floor(minutes);
            seconds = Math.Floor(seconds);
            tenths = Math.Floor(tenths);
        }

        /// <summary>
        /// Check if longitude and latitude in decimal degrees are within valid range
        /// </summary>
        /// <param name="decimal_degrees">longitude or latitude</param>
        /// <param name="bLat">True if latitude</param>
        /// <returns>true if value is correct</returns>
        static public bool CheckLonLatValidity(String decimal_degrees, bool bLat)
        {
            try
            {
                double coord = MyTools.ConvertToDouble(decimal_degrees);
                return CoordConvHMI.CheckLonLatValidity(coord, bLat);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Check if longitude and latitude in decimal degrees are within valid range
        /// </summary>
        /// <param name="decimal_degrees_lon">longitude in decimal degrees (-180 / +180)</param>
        /// <param name="decimal_degrees_lat">latitude in decimal degrees (-90 / +90)</param>
        /// <returns>true if both values are correct</returns>
        static public bool CheckLonLatValidity(String decimal_degrees_lon, String decimal_degrees_lat)
        {
            try
            {
                double lon = MyTools.ConvertToDouble(decimal_degrees_lon);
                double lat = MyTools.ConvertToDouble(decimal_degrees_lat);
                return CoordConvHMI.CheckLonLatValidity(lon, lat);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Check if longitude and latitude in decimal degrees are within valid range
        /// </summary>
        /// <param name="decimal_degrees">longitude or latitude</param>
        /// <param name="bLat">True if latitude</param>
        /// <returns>true if value is correct</returns>
        static public bool CheckLonLatValidity(double decimal_degrees, bool bLat)
        {
            if (bLat)
            {
                if ((decimal_degrees < -90.0) ||
                    (decimal_degrees > 90.0))
                    return false;
                else
                    return true;
            }
            else
            {
                if ((decimal_degrees < -180.0) ||
                    (decimal_degrees > 180.0))
                    return false;
                else
                    return true;
            }
        }



        /// <summary>
        /// Check if longitude and latitude in decimal degrees are within valid range
        /// </summary>
        /// <param name="decimal_degrees_lon">longitude in decimal degrees (-180 / +180)</param>
        /// <param name="decimal_degrees_lat">latitude in decimal degrees (-90 / +90)</param>
        /// <returns>true if both values are correct</returns>
        static public bool CheckLonLatValidity(double decimal_degrees_lon, double decimal_degrees_lat)
        {
            if ((decimal_degrees_lon < -180.0) ||
                (decimal_degrees_lon > 180.0) ||
                (decimal_degrees_lat < -90.0) ||
                (decimal_degrees_lat > 90.0))
                return false;
            else
                return true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DDMMSSStoDDD();
            DDDtoDDMMM();
        }
    }
}
