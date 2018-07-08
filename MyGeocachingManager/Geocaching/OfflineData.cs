using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MyGeocachingManager
{
    /// <summary>
    /// Container for a list of OfflineCacheData of caches
    /// </summary>
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class OfflineData : ISerializable //derive your class from ISerializable
    {

        /// <summary>
        /// Version of deserialized instance
        /// </summary>
        public String _version;

        /// <summary>
        /// List of OfflineCacheData objects
        /// </summary>
        public Dictionary<String, OfflineCacheData> _OfflineData;

        /// <summary>
        /// Current implementation version
        /// </summary>
        public const string VERSION_DATA = "1.0";

        /// <summary>
        /// Constructor
        /// </summary>
        public OfflineData()
        {
            _version = VERSION_DATA; 
            _OfflineData = new Dictionary<string, OfflineCacheData>();
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">info</param>
        /// <param name="ctxt">context</param>
        public OfflineData(SerializationInfo info, StreamingContext ctxt)
        {
            //Get the values from info and assign them to the appropriate properties
            _OfflineData = (Dictionary<string, OfflineCacheData>)info.GetValue("_OfflineData", typeof(Dictionary<string, OfflineCacheData>));
            _version = (string)info.GetValue("_version", typeof(string));
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
            info.AddValue("_OfflineData", _OfflineData);
        }

        /// <summary>
        /// Open a file and serialize the object into it in binary format.
        /// EmployeeInfo.osl is the file that we are creating. 
        /// Note:- you can give any extension you want for your file
        /// If you use custom extensions, then the user will now 
        ///   that the file is associated with your program.
        /// </summary>
        /// <param name="file">file to serialize to</param>
        public void Serialize(String file)
        {
            Stream stream = File.Open(file, FileMode.Create);
            BinaryFormatter bformatter = new BinaryFormatter();

            bformatter.Serialize(stream, this);
            stream.Close();
        }

        /// <summary>
        /// Deserialize
        /// </summary>
        /// <param name="file">file to deserialize from</param>
        /// <returns>Deserialized instance</returns>
        static public OfflineData Deserialize(String file)
        {
            //Clear mp for further usage.
            OfflineData od = null;

            //Open the file written above and read values from it.
            Stream stream = File.Open(file, FileMode.Open);
            BinaryFormatter bformatter = new BinaryFormatter();

            od = (OfflineData)bformatter.Deserialize(stream);
            stream.Close();

            return od;
        }

    }
}
