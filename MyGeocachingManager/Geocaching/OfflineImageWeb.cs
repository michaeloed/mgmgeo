using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace MyGeocachingManager
{
    /// <summary>
    /// Class for downloaded cache images
    /// </summary>
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class OfflineImageWeb : ISerializable //derive your class from ISerializable
    {

        /// <summary>
        /// Current version of implementation
        /// </summary>
        public const string VERSION_DATA = "1.0";

        /// <summary>
        /// Version of deserialized instance
        /// </summary>
        public String _version;

        /// <summary>
        /// Export date of instance
        /// </summary>
        public DateTime _dateExport;

        /// <summary>
        /// Image name
        /// </summary>
        public String _name;

        /// <summary>
        /// Image url
        /// </summary>
        public String _url;

        /// <summary>
        /// Image local file on hard drive
        /// </summary>
        public String _localfile;

        /// <summary>
        /// Constructor
        /// </summary>
        public OfflineImageWeb()
        {
            _version = VERSION_DATA;
            _dateExport = DateTime.Now;
            _name = "";
            _url = "";
            _localfile = "";
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">info</param>
        /// <param name="ctxt">context</param>
        public OfflineImageWeb(SerializationInfo info, StreamingContext ctxt)
        {
            //Get the values from info and assign them to the appropriate properties
            _version = (string)info.GetValue("_version", typeof(string));
            _dateExport = (DateTime)info.GetValue("_dateExport", typeof(DateTime));
            _name = (string)info.GetValue("_name", typeof(string));
            _url = (string)info.GetValue("_url", typeof(string));
            _localfile = (string)info.GetValue("_localfile", typeof(string));
        }

        /// <summary>
        /// Serialization function.
        /// </summary>
        /// <param name="info">info</param>
        /// <param name="ctxt">context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            //You can use any custom name for your name-value pair. But make sure you
            // read the values with the same name. For ex:- If you write EmpId as "EmployeeId"
            // then you should read the same with "EmployeeId"
            info.AddValue("_version", _version);
            info.AddValue("_dateExport", _dateExport);
            info.AddValue("_name", _name);
            info.AddValue("_url", _url);
            info.AddValue("_localfile", _localfile);
        }
    }
}
