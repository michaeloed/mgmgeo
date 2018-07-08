using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceEyeTools
{
    /// <summary>
    /// Stupid class to handle coordinates
    /// </summary>
    public class Coordinate
    {
        /// <summary>
        /// Latitude
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Coordinate() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="latitude">latitude</param>
        /// <param name="longitude">longitude</param>
        public Coordinate(double latitude, double longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
        }
    }
}
