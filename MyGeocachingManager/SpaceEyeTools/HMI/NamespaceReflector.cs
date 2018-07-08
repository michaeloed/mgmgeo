using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace SpaceEyeTools.HMI
{
    /// <summary>
    /// Mwahaha, a form that performs reflection on the loaded assembly
    /// Exposes all public elements of this assembly in a tree and display associated documentation
    /// as a tooltip
    /// A true masterpiece ;-)
    /// </summary>
    public partial class NamespaceReflector : Form
    {
        private Dictionary<String, DocumentationElement> _documentation = new Dictionary<string,DocumentationElement>();

        /// <summary>
        /// Image used for namespace
        /// </summary>
        public Image Reflection_Namespace = null;

        /// <summary>
        /// Image used for class
        /// </summary>
        public Image Reflection_Class = null;

        /// <summary>
        /// Image used for method
        /// </summary>
        public Image Reflection_Method = null;

        /// <summary>
        /// Image used for constructor
        /// </summary>
        public Image Reflection_Constructor = null;

        /// <summary>
        /// Image used for field
        /// </summary>
        public Image Reflection_Field = null;

        /// <summary>
        /// Image used for property
        /// </summary>
        public Image Reflection_Property = null;

        /// <summary>
        /// Image used for interface
        /// </summary>
        public Image Reflection_Interface = null;

        /// <summary>
        /// Image used for enumeration
        /// </summary>
        public Image Reflection_Enum = null;

        /// <summary>
        /// Image used for enumeration value
        /// </summary>
        public Image Reflection_EnumValue = null;

        /// <summary>
        /// List of namespaces authorized for parsing and reflection
        /// </summary>
        public string[] AuthorizedNamespaces = null;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="title">form title</param>
        /// <param name="docfile">path to document file in XML standard documentation format</param>
        public NamespaceReflector(String title, String docfile)
        {
            InitializeComponent();
            this.Text = title;
            _documentation = DocumentationReader.ReadDocumentation(docfile);
        }

        private void SetImage(TreeNode node, ReflectionImage_Type index)
        {
            node.ImageIndex = (int)index;
            node.SelectedImageIndex = (int)index;
        }

        private string GetDocumentation(String key)
        {
            if (_documentation.ContainsKey(key))
                return _documentation[key].ToString();
            else
                return key + "\r\n<No documentation>";
        }

        /// <summary>
        /// Enumeration defining which type of parameter shall be created
        /// </summary>
        public enum ReflectionImage_Type
        {
            /// <summary>
            /// Value of enum for namespace
            /// </summary>
            Reflection_Namespace = 0,
            /// <summary>
            /// Value of enum for class
            /// </summary>
            Reflection_Class = 1,
            /// <summary>
            /// Value of enum for method
            /// </summary>
            Reflection_Method = 2,
            /// <summary>
            /// Value of enum for constructor
            /// </summary>
            Reflection_Constructor = 3,
            /// <summary>
            /// Value of enum for field
            /// </summary>
            Reflection_Field = 4,
            /// <summary>
            /// Value of enum for property
            /// </summary>
            Reflection_Property = 5,
            /// <summary>
            /// Value of enum for interface
            /// </summary>
            Reflection_Interface = 6,
            /// <summary>
            /// Value of enum for enumeration
            /// </summary>
            Reflection_Enum = 7,
            /// <summary>
            /// Value of enum for enum value
            /// </summary>
            Reflection_EnumValue = 8
        };

        private void MGMReflector_Load(object sender, EventArgs e)
        {
            
            ImageList myImageList = new ImageList();
            
            myImageList.Images.Add(Reflection_Namespace); // 0
            myImageList.Images.Add(Reflection_Class); // 1
            myImageList.Images.Add(Reflection_Method); // 2
            myImageList.Images.Add(Reflection_Constructor); // 3
            myImageList.Images.Add(Reflection_Field); // 4
            myImageList.Images.Add(Reflection_Property); // 5
            myImageList.Images.Add(Reflection_Interface); // 6
            myImageList.Images.Add(Reflection_Enum); // 7
            myImageList.Images.Add(Reflection_EnumValue); // 8
            
            treeViewTypes.ImageList = myImageList;
            treeViewTypes.TreeViewNodeSorter = new NodeSorter();

            Dictionary<string, TreeNode> racines = new Dictionary<string, TreeNode>();
            foreach (string ns in AuthorizedNamespaces)
            {
                TreeNode root = treeViewTypes.Nodes.Add(ns);
                root.Tag = ns;
                SetImage(root, ReflectionImage_Type.Reflection_Namespace);
                racines.Add(ns, root);
            }

            Assembly ass = Assembly.GetExecutingAssembly();
            foreach (Type type in ass.GetTypes())
            {
                if (type.IsPublic)
                {
                    if ((type.Namespace != null) && (racines.ContainsKey(type.Namespace)))
                    {
                        TreeNode root = racines[type.Namespace];
                        String lbl = DisplayType(type); // type.Name;
                        if ((type.BaseType != null) && (type.BaseType != typeof(Object)))
                            lbl += " : " + DisplayType(type.BaseType);
                        else
                        {
                            Type[] infs = type.GetInterfaces();
                            if ((infs != null) && (infs.Count() != 0))
                            {
                                int count = infs.Count();
                                int i = 0;
                                lbl += " : ";
                                foreach (Type inf in type.GetInterfaces())
                                {
                                    lbl += inf.Name;
                                    i++;
                                    if (i != count)
                                        lbl += ", ";
                                }
                            }
                        }

                        TreeNode node = root.Nodes.Add(lbl);
                        node.ToolTipText = GetDocumentation("T:" + type.FullName);
                        if (type.IsPublic == false)
                            node.ForeColor = Color.Red;
                        node.Tag = lbl;
                        if (type.IsInterface)
                            SetImage(node, ReflectionImage_Type.Reflection_Interface);
                        else
                            SetImage(node, ReflectionImage_Type.Reflection_Class);
                        PopulateNode(node, type, false);
                    }
                }
            }
            treeViewTypes.Sort();
        }

        private String DisplayTypeRaw(Type t)
        {
            if (t.IsGenericType)
            {
                String nom = "";
                if (t.FullName != null)
                {
                    int pos = t.FullName.IndexOf("`");
                    nom = t.FullName;
                    if (pos != -1)
                        nom = t.FullName.Substring(0, pos);
                }
                else
                {
                    int pos = t.Name.IndexOf("`");
                    nom = t.Name;
                    if (pos != -1)
                        nom = t.Name.Substring(0, pos);
                }

                String lbl = nom + "{";
                // Get the generic type parameters or type arguments.
                Type[] typeParameters = t.GetGenericArguments();

                int count = typeParameters.Length;
                int i = 0;
                foreach (Type tParam in typeParameters)
                {
                    lbl += DisplayTypeRaw(tParam);
                    i++;
                    if (i != count)
                        lbl += ",";
                }

                lbl += "}";
                return lbl;
            }
            else
                return t.FullName;
        }

        private String DisplayType(Type t)
        {
            if (t.IsGenericType)
            {
                int pos = t.Name.IndexOf("`");
                String nom = t.Name;
                if (pos != -1)
                    nom = t.Name.Substring(0, pos);
                String lbl = nom + "<";
                // Get the generic type parameters or type arguments.
                Type[] typeParameters = t.GetGenericArguments();

                int count = typeParameters.Length;
                int i = 0;
                foreach (Type tParam in typeParameters)
                {
                    lbl += DisplayType(tParam);
                    i++;
                    if (i != count)
                        lbl += ", ";
                }

                lbl += ">";
                return lbl;
            }
            else
                return t.Name;
        }

        private string DisplayMethodRaw(ParameterInfo[] pi)
        {
            if (pi.Count() == 0)
                return "";

            String lbl = "(";
            int count = pi.Count();
            int i = 0;
            foreach (var param in pi)
            {
                lbl += DisplayTypeRaw(param.ParameterType);
                i++;
                if (i != count)
                    lbl += ",";
            }
            lbl += ")";
            lbl = lbl.Replace("&", "@");
            lbl = lbl.Replace("+", ".");
            return lbl;
        }

        private string DisplayMethod(ParameterInfo[] pi)
        {
            String lbl = "(";
            int count = pi.Count();
            int i = 0;
            foreach (var param in pi)
            {
                lbl += DisplayType(param.ParameterType) + " " + param.Name;
                i++;
                if (i != count)
                    lbl += ", ";
            }
            lbl += ")";
            return lbl;
        }

        private bool IsAccessor(MethodInfo mi)
        {
            try
            {
                if ((mi == null) || (mi.DeclaringType == null))
                    return false;
                PropertyInfo[] pif = mi.DeclaringType.GetProperties();
                if ((pif != null) && (pif.Count() != 0))
                {
                    foreach (PropertyInfo pi in pif)
                    {
                        foreach (MethodInfo mis in pi.GetAccessors())
                        {
                            if (mis.Name == mi.Name)
                                return true;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        private void PopulateNode(TreeNode node, Type type, bool bNestedType)
        {
            foreach (ConstructorInfo ctor in type.GetConstructors())
            {
                if (ctor.IsPublic)
                {
                    String lbl = type.Name + DisplayMethod(ctor.GetParameters());
                    TreeNode subnode = node.Nodes.Add(lbl);
                    subnode.Tag = ".ctor: " + lbl;

                    String sdoc = "M:" + type.FullName + ".#ctor" + DisplayMethodRaw(ctor.GetParameters());
                    if (bNestedType)
                        sdoc = sdoc.Replace("+", ".");
                    subnode.ToolTipText = GetDocumentation(sdoc);

                    SetImage(subnode, ReflectionImage_Type.Reflection_Constructor);
                    if (ctor.IsPublic == false)
                        subnode.ForeColor = Color.Red;
                }
            }
            foreach (MemberInfo mi in type.GetMembers())
            {
                // Method, Property, Field, Constructor
                switch (mi.MemberType)
                {
                    case MemberTypes.NestedType:
                        {
                            Type nestedType = (Type)mi;
                            String lbl = nestedType.Name;
                            TreeNode subnode = node.Nodes.Add(lbl);
                            subnode.Tag = lbl;
                            subnode.ToolTipText = GetDocumentation("T:" + type.FullName + "." + nestedType.Name);
                            SetImage(subnode, ReflectionImage_Type.Reflection_Enum);
                            PopulateNode(subnode, nestedType, true);
                            break;
                        }
                    case MemberTypes.Field:
                        {
                            FieldInfo fi = ((FieldInfo)mi);
                            if (fi.IsPublic && (fi.DeclaringType == type))
                            {
                                TreeNode subnode = null;
                                if (!bNestedType)
                                {
                                    String lbl = fi.Name + " : " + DisplayType(fi.FieldType);
                                    subnode = node.Nodes.Add(lbl);
                                    subnode.Tag = "Field: " + lbl;
                                    subnode.ToolTipText = GetDocumentation("F:" + type.FullName + "." + fi.Name);
                                    SetImage(subnode, ReflectionImage_Type.Reflection_Field);
                                }
                                else
                                {
                                    //  Un enumvalue
                                    String lbl = fi.Name;
                                    subnode = node.Nodes.Add(lbl);
                                    String sdoc = "F:" + type.FullName + "." + fi.Name;
                                    sdoc = sdoc.Replace("+", ".");
                                    subnode.ToolTipText = GetDocumentation(sdoc);
                                    SetImage(subnode, ReflectionImage_Type.Reflection_EnumValue);
                                }
                                
                                if (fi.IsPrivate)
                                    subnode.ForeColor = Color.Red;
                                
                            }
                            break;
                        }
                    case MemberTypes.Method:
                        {
                            MethodInfo obj = ((MethodInfo)mi);
                            if (obj.IsPublic && (obj.IsConstructor == false) && (obj.DeclaringType == type) && (!IsAccessor(obj)))
                            {
                                String lbl = obj.Name + DisplayMethod(obj.GetParameters()) + " : " + DisplayType(obj.ReturnType);
                                TreeNode subnode = node.Nodes.Add(lbl);
                                subnode.Tag = "Method: " + lbl;
                                String sdoc = "M:" + type.FullName + "." + obj.Name + DisplayMethodRaw(obj.GetParameters());
                                if (bNestedType)
                                    sdoc = sdoc.Replace("+",".");
                                subnode.ToolTipText = GetDocumentation(sdoc);
                                SetImage(subnode, ReflectionImage_Type.Reflection_Method);
                                if (obj.IsPublic == false)
                                    subnode.ForeColor = Color.Red;
                            }
                            break;
                        }
                    case MemberTypes.Property:
                        {
                            PropertyInfo obj = ((PropertyInfo)mi);
                            if (obj.DeclaringType == type)
                            {
                                String lbl = obj.Name;
                                TreeNode subnode = node.Nodes.Add(lbl);
                                subnode.Tag = "Property: " + lbl;
                                String sdoc = "P:" + type.FullName + "." + obj.Name;
                                if (bNestedType)
                                    sdoc = sdoc.Replace("+", ".");
                                subnode.ToolTipText = GetDocumentation(sdoc);
                                
                                SetImage(subnode, ReflectionImage_Type.Reflection_Property);
                                foreach (MethodInfo mai in obj.GetAccessors())
                                {
                                    if (mai.IsPublic)
                                    {
                                        TreeNode finalnode = subnode.Nodes.Add(mai.ToString());
                                        finalnode.Tag = mai.ToString();
                                        SetImage(finalnode, ReflectionImage_Type.Reflection_Method);
                                        if (mai.IsPublic == false)
                                            finalnode.ForeColor = Color.Red;
                                    }
                                }
                            }
                            break;
                        }
                    //default:
                    //    if (mi.Name.Contains("ParameterType"))
                    //        MessageBox.Show(mi.ToString());
                    //    break;
                }
            }
        }
    }

    /// <summary>
    /// Sorter for nodes
    /// Performs comparison on node Tag value
    /// </summary>
    public class NodeSorter : System.Collections.IComparer
    {
        /// <summary>
        /// Comparer methode
        /// </summary>
        /// <param name="x">first object</param>
        /// <param name="y">second object</param>
        /// <returns>-1 if x inferior to Y, 0 if equals, +1 if superior to y</returns>
        public int Compare(object x, object y)
        {
            try
            {
                TreeNode tx = x as TreeNode;
                TreeNode ty = y as TreeNode;
                return string.Compare(tx.Tag.ToString(), ty.Tag.ToString());
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }

}
