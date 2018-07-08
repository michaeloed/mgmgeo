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
    /// Wrapper for bookmarks
    /// </summary>
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class ListTextCoord : ISerializable //derive your class from ISerializable
    {

        /// <summary>
        /// List of bookmarks
        /// </summary>
        public List<TextCoord> _TextCoords;

        /// <summary>
        /// List of associated bookmark files
        /// </summary>
        public List<DatabaseOfFiles> _Databases;

        /// <summary>
        /// Constructor
        /// </summary>
        public ListTextCoord()
        {
            _TextCoords = new List<TextCoord>();
            _Databases = new List<DatabaseOfFiles>();
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">info</param>
        /// <param name="ctxt">context</param>
        public ListTextCoord(SerializationInfo info, StreamingContext ctxt)
        {
            //Get the values from info and assign them to the appropriate properties
            _TextCoords = (List<TextCoord>)info.GetValue("_TextCoords", typeof(List<TextCoord>));

            try
            {
                _Databases = (List<DatabaseOfFiles>)info.GetValue("_Databases", typeof(List<DatabaseOfFiles>));
            }
            catch (Exception)
            {
                _Databases = new List<DatabaseOfFiles>();
            }
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
            info.AddValue("_TextCoords", _TextCoords);
            info.AddValue("_Databases", _Databases);
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
        static public ListTextCoord Deserialize(String file)
        {
            //Clear mp for further usage.
            ListTextCoord ltc = null;

            //Open the file written above and read values from it.
            Stream stream = File.Open(file, FileMode.Open);
            BinaryFormatter bformatter = new BinaryFormatter();

            ltc = (ListTextCoord)bformatter.Deserialize(stream);
            stream.Close();

            return ltc;
        }

    }
}
