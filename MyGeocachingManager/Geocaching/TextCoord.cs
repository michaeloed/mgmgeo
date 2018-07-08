using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MyGeocachingManager
{
    /// <summary>
    /// Bookmark definition.
    /// </summary>
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class TextCoord : ISerializable //derive your class from ISerializable
    {

        /// <summary>
        /// Bookmark longitude
        /// </summary>
        public String _Lon;

        /// <summary>
        /// Bookmark latitude
        /// </summary>
        public String _Lat;

        /// <summary>
        /// Bookmark name
        /// </summary>
        public String _Name;

        /// <summary>
        /// Bookmark unique identifier
        /// </summary>
        public String _Uid;

        /// <summary>
        /// Constructor
        /// </summary>
        public TextCoord()
        {
            _Lon = "";
            _Lat = "";
            _Name = "";
            _Uid = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">info</param>
        /// <param name="ctxt">context</param>
        public TextCoord(SerializationInfo info, StreamingContext ctxt)
        {
            //Get the values from info and assign them to the appropriate properties
            _Name = (string)info.GetValue("_Name", typeof(string));
            _Lon = (string)info.GetValue("_Lon", typeof(string));
            _Lat = (string)info.GetValue("_Lat", typeof(string));
            _Uid = (string)info.GetValue("_Uid", typeof(string));
        }

        /// <summary>
        /// Serialization function
        /// </summary>
        /// <param name="info">info</param>
        /// <param name="ctxt">context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            //You can use any custom name for your name-value pair. But make sure you
            // read the values with the same name. For ex:- If you write EmpId as "EmployeeId"
            // then you should read the same with "EmployeeId"
            info.AddValue("_Name", _Name);
            info.AddValue("_Lon", _Lon);
            info.AddValue("_Lat", _Lat);
            info.AddValue("_Uid", _Uid);
        }
    }
}
