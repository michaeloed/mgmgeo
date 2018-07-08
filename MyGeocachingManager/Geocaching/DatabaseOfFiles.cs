using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MyGeocachingManager
{
    /// <summary>
    /// Database of bookmark files
    /// </summary>
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class DatabaseOfFiles : ISerializable //derive your class from ISerializable
    {

        /// <summary>
        /// Database name
        /// </summary>
        public String _Name;

        /// <summary>
        /// List of associated files
        /// </summary>
        public List<String> _Files;

        /// <summary>
        /// DB identifier
        /// </summary>
        public String _Id;

        /// <summary>
        /// Constructor
        /// </summary>
        public DatabaseOfFiles()
        {
            _Id = Guid.NewGuid().ToString();
            _Name = "";
            _Files = new List<string>();
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">info</param>
        /// <param name="ctxt">context</param>
        public DatabaseOfFiles(SerializationInfo info, StreamingContext ctxt)
        {
            //Get the values from info and assign them to the appropriate properties
            _Name = (string)info.GetValue("_Name", typeof(string));
            _Id = (string)info.GetValue("_Id", typeof(string));
            _Files = (List<string>)info.GetValue("_Files", typeof(List<string>));
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
            info.AddValue("_Name", _Name);
            info.AddValue("_Id", _Name);
            info.AddValue("_Files", _Files);
        }

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>String version of the object</returns>
        public override string ToString()
        {
            return _Name;
        }
    }
}
