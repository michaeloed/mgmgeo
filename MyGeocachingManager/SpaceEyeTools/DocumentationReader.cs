using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using SpaceEyeTools.HMI;
using System.Windows.Forms;

namespace SpaceEyeTools
{
    /// <summary>
    /// Parse a documentation file and create a dictionnary
    /// </summary>
    public class DocumentationReader
    {
        /// <summary>
        /// Read documentation file
        /// </summary>
        /// <param name="docfile">documentation file</param>
        /// <returns>dictionary containing all documentation entries</returns>
        static public Dictionary<String, DocumentationElement> ReadDocumentation(String docfile)
        {
            Dictionary<String, DocumentationElement> documentation = new Dictionary<String, DocumentationElement>();
            try
            {
                DocumentationElement elt = null;
                XmlReader reader = XmlReader.Create(docfile);
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name.ToString())
                        {
                            case "member":
                                // On stocke l'ancien
                                if (elt != null)
                                    documentation.Add(elt.Name, elt);

                                // On démarre un nouveau
                                elt = new DocumentationElement();
                                elt.Name = reader.GetAttribute("name").Trim();
                                break;

                            case "summary":
                                if (elt != null)
                                    elt.Summary = reader.ReadString().Trim();
                                // Sinon c'est la merde ?!
                                break;

                            case "returns":
                                if (elt != null)
                                    elt.Returns = reader.ReadString().Trim();
                                // Sinon c'est la merde ?!
                                break;

                            case "param":
                                // On crée un param
                                if (elt != null)
                                {
                                    DocumentationElement param = new DocumentationElement();
                                    param.Name = reader.GetAttribute("name");
                                    param.Summary = reader.ReadString();
                                    elt.Parameters.Add(param);
                                }
                                break;
                        }
                    }
                }

                // nettoyage
                reader.Close();

                // On stocke le dernier
                if (elt != null)
                    documentation.Add(elt.Name, elt);
            }
            catch (Exception)
            {
            }
            return documentation;
        }
    }

    /// <summary>
    /// A documentation element
    /// </summary>
    public class DocumentationElement
    {
        /// <summary>
        /// Name of this element
        /// </summary>
        public String Name = "";
        
        /// <summary>
        /// Associated summary
        /// </summary>
        public String Summary = "";

        /// <summary>
        /// Associated return documentation
        /// </summary>
        public String Returns = "";

        /// <summary>
        /// Associated DocumentationElement (parameters of method)
        /// </summary>
        public List<DocumentationElement> Parameters = new List<DocumentationElement>();

        /// <summary>
        /// constructor
        /// </summary>
        public DocumentationElement()
        {
        }

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>stirng value</returns>
        public override string ToString()
        {
            String s = "Name: " + Name;
            if (Summary != "")
                s += "\r\nSummary: " + Summary;
            if (Returns != "")
                s += "\r\nReturns: " + Returns;
            if (Parameters.Count != 0)
            {
                s += "\r\nParameter(s): " + Returns;
                foreach (DocumentationElement elt in Parameters)
                {
                    s += "\r\n    " + elt.Name;
                    if (elt.Summary != "")
                        s += ": " + elt.Summary;
                }
            }

            return s;
        }
    }
}
