using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceEyeTools
{
    /// <summary>
    /// Stupid class to describe a point with "Double" coordinates
    /// </summary>
    public class PointDouble
    {

        /// <summary>
        /// X coordinate
        /// </summary>
        public double X;

        /// <summary>
        /// Y coordinate
        /// </summary>
        public double Y;

        /// <summary>
        /// Constructor
        /// Initialise point to (0.0, 0.0)
        /// </summary>
        public PointDouble()
        {
            X = 0.0;
            Y = 0.0;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public PointDouble(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>string value</returns>
        public override string ToString()
        {
            String s = "[" + X.ToString() + " ; " + Y.ToString() + "]";
            return s;
        }
    }
}
