/*
 * Created by SharpDevelop.
 * User: bcristiano
 * Date: 15/09/2016
 * Time: 11:36
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using SpaceEyeTools;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;

namespace MyGeocachingManager.HMI
{
	/// <summary>
	/// Description of ToolbarConfiguration.
	/// </summary>
	public partial class ToolbarConfiguration : Form
	{
		MainWindow _daddy = null;
		ImageList _imageList = new ImageList();
		List<String> _addedindex = new List<string>();
		static String _sepTag = "-";
		static String _sepLbl = "-------------";
static List<Tuple<String,bool>> _colorandpen = new List<Tuple<string, bool>>(new Tuple<string, bool>[]{
new Tuple<String,bool>("YellowGreen", false),
new Tuple<String,bool>("Yellow", false),
new Tuple<String,bool>("Wheat", false),
new Tuple<String,bool>("Violet", false),
new Tuple<String,bool>("Turquoise", false),
new Tuple<String,bool>("Tomato", false),
new Tuple<String,bool>("Thistle", false),
new Tuple<String,bool>("Teal", true),
new Tuple<String,bool>("Tan", false),
new Tuple<String,bool>("SteelBlue", true),
new Tuple<String,bool>("SpringGreen", false),
new Tuple<String,bool>("Snow", false),
new Tuple<String,bool>("SlateGray", true),
new Tuple<String,bool>("SlateBlue", true),
new Tuple<String,bool>("SkyBlue", false),
new Tuple<String,bool>("Silver", false),
new Tuple<String,bool>("Sienna", true),
new Tuple<String,bool>("SeaShell", false),
new Tuple<String,bool>("SeaGreen", true),
new Tuple<String,bool>("SandyBrown", false),
new Tuple<String,bool>("Salmon", false),
new Tuple<String,bool>("SaddleBrown", true),
new Tuple<String,bool>("RoyalBlue", true),
new Tuple<String,bool>("RosyBrown", false),
new Tuple<String,bool>("Red", true),
new Tuple<String,bool>("Purple", true),
new Tuple<String,bool>("PowderBlue", false),
new Tuple<String,bool>("Plum", false),
new Tuple<String,bool>("Pink", false),
new Tuple<String,bool>("Peru", false),
new Tuple<String,bool>("PeachPuff", false),
new Tuple<String,bool>("PapayaWhip", false),
new Tuple<String,bool>("PaleVioletRed", false),
new Tuple<String,bool>("PaleTurquoise", false),
new Tuple<String,bool>("PaleGreen", false),
new Tuple<String,bool>("PaleGoldenrod", false),
new Tuple<String,bool>("Orchid", false),
new Tuple<String,bool>("OrangeRed", true),
new Tuple<String,bool>("Orange", false),
new Tuple<String,bool>("OliveDrab", true),
new Tuple<String,bool>("Olive", true),
new Tuple<String,bool>("OldLace", false),
new Tuple<String,bool>("Navy", true),
new Tuple<String,bool>("NavajoWhite", false),
new Tuple<String,bool>("Moccasin", false),
new Tuple<String,bool>("MistyRose", false),
new Tuple<String,bool>("MintCream", false),
new Tuple<String,bool>("MidnightBlue", true),
new Tuple<String,bool>("MediumVioletRed", true),
new Tuple<String,bool>("MediumTurquoise", false),
new Tuple<String,bool>("MediumSpringGreen", false),
new Tuple<String,bool>("MediumSlateBlue", true),
new Tuple<String,bool>("MediumSeaGreen", false),
new Tuple<String,bool>("MediumPurple", true),
new Tuple<String,bool>("MediumOrchid", true),
new Tuple<String,bool>("MediumBlue", true),
new Tuple<String,bool>("MediumAquamarine", false),
new Tuple<String,bool>("Maroon", true),
new Tuple<String,bool>("Linen", false),
new Tuple<String,bool>("LimeGreen", false),
new Tuple<String,bool>("Lime", false),
new Tuple<String,bool>("LightYellow", false),
new Tuple<String,bool>("LightSteelBlue", false),
new Tuple<String,bool>("LightSlateGray", true),
new Tuple<String,bool>("LightSkyBlue", false),
new Tuple<String,bool>("LightSeaGreen", false),
new Tuple<String,bool>("LightSalmon", false),
new Tuple<String,bool>("LightPink", false),
new Tuple<String,bool>("LightGreen", false),
new Tuple<String,bool>("LightGray", false),
new Tuple<String,bool>("LightGoldenrodYellow", false),
new Tuple<String,bool>("LightCyan", false),
new Tuple<String,bool>("LightCoral", false),
new Tuple<String,bool>("LightBlue", false),
new Tuple<String,bool>("LemonChiffon", false),
new Tuple<String,bool>("LawnGreen", false),
new Tuple<String,bool>("LavenderBlush", false),
new Tuple<String,bool>("Lavender", false),
new Tuple<String,bool>("Khaki", false),
new Tuple<String,bool>("Ivory", false),
new Tuple<String,bool>("Indigo", true),
new Tuple<String,bool>("IndianRed", true),
new Tuple<String,bool>("HotPink", false),
new Tuple<String,bool>("Honeydew", false),
new Tuple<String,bool>("GreenYellow", false),
new Tuple<String,bool>("Green", true),
new Tuple<String,bool>("Gray", true),
new Tuple<String,bool>("Goldenrod", false),
new Tuple<String,bool>("Gold", false),
new Tuple<String,bool>("GhostWhite", false),
new Tuple<String,bool>("Gainsboro", false),
new Tuple<String,bool>("Fuchsia", true),
new Tuple<String,bool>("ForestGreen", true),
new Tuple<String,bool>("FloralWhite", false),
new Tuple<String,bool>("Firebrick", true),
new Tuple<String,bool>("DodgerBlue", false),
new Tuple<String,bool>("DimGray", true),
new Tuple<String,bool>("DeepSkyBlue", false),
new Tuple<String,bool>("DeepPink", true),
new Tuple<String,bool>("DarkViolet", true),
new Tuple<String,bool>("DarkTurquoise", false),
new Tuple<String,bool>("DarkSlateGray", true),
new Tuple<String,bool>("DarkSlateBlue", true),
new Tuple<String,bool>("DarkSeaGreen", false),
new Tuple<String,bool>("DarkSalmon", false),
new Tuple<String,bool>("DarkRed", true),
new Tuple<String,bool>("DarkOrchid", true),
new Tuple<String,bool>("DarkOrange", false),
new Tuple<String,bool>("DarkOliveGreen", true),
new Tuple<String,bool>("DarkMagenta", true),
new Tuple<String,bool>("DarkKhaki", false),
new Tuple<String,bool>("DarkGreen", true),
new Tuple<String,bool>("DarkGray", false),
new Tuple<String,bool>("DarkGoldenrod", false),
new Tuple<String,bool>("DarkCyan", true),
new Tuple<String,bool>("DarkBlue", true),
new Tuple<String,bool>("Cyan", false),
new Tuple<String,bool>("Crimson", true),
new Tuple<String,bool>("Cornsilk", false),
new Tuple<String,bool>("CornflowerBlue", false),
new Tuple<String,bool>("Coral", false),
new Tuple<String,bool>("Chocolate", true),
new Tuple<String,bool>("Chartreuse", false),
new Tuple<String,bool>("CadetBlue", false),
new Tuple<String,bool>("BurlyWood", false),
new Tuple<String,bool>("Brown", true),
new Tuple<String,bool>("BlueViolet", true),
new Tuple<String,bool>("Blue", true),
new Tuple<String,bool>("BlanchedAlmond", false),
new Tuple<String,bool>("Black", true),
new Tuple<String,bool>("Bisque", false),
new Tuple<String,bool>("Beige", false),
new Tuple<String,bool>("Azure", false),
new Tuple<String,bool>("Aquamarine", false),
new Tuple<String,bool>("Aqua", false),
new Tuple<String,bool>("AntiqueWhite", false),
new Tuple<String,bool>("AliceBlue", false)
});
		int _indexColor = 0;
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="daddy"></param>
		public ToolbarConfiguration(MainWindow daddy)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			_daddy = daddy;
			
			_imageList.Images.Clear();
            _imageList.ColorDepth = ColorDepth.Depth32Bit;
            _imageList.ImageSize = new Size(16, 16); // this will affect the row height
            foreach(var pair in _daddy._imgMenus)
            {
            	_imageList.Images.Add(pair.Key, pair.Value);
            }
            
            tblvAvail.SmallImageList = _imageList;
            tblvAdded.SmallImageList = _imageList;
            
            tblvAvail.Columns.Add(_daddy.GetTranslator().GetString("tbAvailMenus"), 250);
            tblvAdded.Columns.Add(_daddy.GetTranslator().GetString("tbAddedMenus"), 250);
            _indexColor = 0;
            
            // On ajoute toutes les entrées de menu de MGM
            foreach(var o in _daddy.menuStrip1.Items)
            {
            	ToolStripMenuItem tsi = o as ToolStripMenuItem;
            	PopulateWithTSI(tblvAvail, tsi, true);	
            }
            
            // On peuple avec la barre existant
            String ids = ConfigurationManager.AppSettings["toolbarids"];
            String[] sids = ids.Split(';');
            foreach(String id in sids)
            {
            	if (id == _sepTag)
            	{
            		AddSeparator(tblvAdded);
            	}
            	else
            	{
            		// On recherche son équivalent dans tblvAvail
            		foreach(ListViewItem item in tblvAvail.Items)
            		{
            			String tag = item.Tag as String;
						if (!String.IsNullOrEmpty(tag))
						{
							if (tag == id)
							{
								AddItemToRight(item);
								break;
							}
						}
            		}
            	}
            }
            
            this.Text = _daddy.GetTranslator().GetString("configureToolbarToolStripMenuItem");
            this.Icon = _daddy.Icon;
            tbbtnSave.Text = _daddy.GetTranslator().GetString("tbbtnSave");
            tbbtnCancel.Text = _daddy.GetTranslator().GetString("tbbtnCancel");
            
            _daddy.TranslateTooltips(this, null);
            
		}
		
		private void AddItemToRight(ListViewItem item)
		{
			var newitem = tblvAdded.Items.Add(item.Text);
			newitem.Tag = item.Tag;
			newitem.ImageKey = item.ImageKey;
			newitem.BackColor = item.BackColor;
			newitem.Font = (Font)(item.Font.Clone());
			String tag = item.Tag as String;
			if (!String.IsNullOrEmpty(tag))
			{
				if (tag != _sepTag)
					_addedindex.Add(tag);
			}
		}
		private void PopulateWithTSI(ListView lv, ToolStripMenuItem tsi, bool labelonly = false)
		{
			if ((tsi != null) && (tsi.Name != "")) // Si on a un nom vide, de toute façon on ne sait rien faire
        	{
				// On regarde si on est dans les golds et que nous ne sommes pas gold
				bool special = SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled();
				if (!special)
				{
					foreach(var o in _daddy._menuEntriesRequiringSpecialFeatures)
	    			{
	    				if (o == tsi)
	    				{
	    					return; // Pas d'ajout de ce truc !
	    				}
	    			}
				}
				
				
				if ((tsi.DropDownItems.Count != 0) && (!labelonly))
				{
					AddSeparator(lv);
				}
				
				// Et lui même
				String lbl = tsi.Text.Replace("&","");
        		ListViewItem item = lv.Items.Add(lbl);
        		if (_daddy._imgMenus.ContainsKey(tsi.Name))
        			item.ImageKey = tsi.Name; // on a une icone
        		else
        		{
        			
        			_imageList.Images.Add(tsi.Name, MyTools.CreateToolbarImage(lbl, (_colorandpen[_indexColor].Item2)?Color.White:Color.Black, Color.FromName(_colorandpen[_indexColor].Item1)));
        			item.ImageKey = tsi.Name; // on a créé une icone "nomenuicon";
        			_indexColor = (_indexColor + 1)%(_colorandpen.Count);
        		}

        		if (!labelonly)
        		{
					item.Tag = tsi.Name;
        		}
        		else
        		{
        			// Une tête de menu
        			item.Font = MyTools.ChangeFontStyle(item.Font, true, true);
        			//item.BackColor = Color.LightGreen;
        		}
                	
        		if ((tsi.DropDownItems.Count != 0)&&(!labelonly))
				{
					item.Font = MyTools.ChangeFontStyle(item.Font, false, true);
					item.Tag = null; // Les menus ne sont pas ajoutables dans la toolbar
				}
        		
        		// Les entrées du menu
				foreach(var sub in tsi.DropDownItems)
				{
					ToolStripMenuItem tsis = sub as ToolStripMenuItem;
					if (tsis != null)
						PopulateWithTSI(lv, tsis);
					else
					{
						ToolStripSeparator tsp = sub as ToolStripSeparator;
						if (tsp != null)
							AddSeparator(lv);
						else
						{
							// Un label ?
							ToolStripLabel tsl = sub as ToolStripLabel;
							if (tsl != null)
							{
								var tsli = lv.Items.Add(tsl.Text);
								tsli.ImageKey = tsl.Name;
								tsli.Font = MyTools.ChangeFontStyle(tsli.Font, true, false);
							}
						}
					}
				}
				// On ajoute un séparateur après pour isoler le menu
				if (tsi.DropDownItems.Count != 0)
				{
					AddSeparator(lv);
				}
        	}
		}
		
		private void AddSeparator(ListView lv)
		{
			if ((lv.Items.Count != 0)&&(lv.Items[lv.Items.Count - 1].Text != _sepLbl))
			{
				var tsep = lv.Items.Add(_sepLbl);
				tsep.Tag = _sepTag;
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="daddy"></param>
		static public void CreateToolbar(MainWindow daddy)
		{
			try
			{
				// On efface la toolbar de daddy
				if (daddy._shortcutToolstrip != null)
				{
					daddy._shortcutToolstrip.Items.Clear();
					daddy.Controls.Remove(daddy._shortcutToolstrip);
					daddy._shortcutToolstrip = null;
				}
				
				// On recrée éventuellement la toolbar
				String ids = ConfigurationManager.AppSettings["toolbarids"];
	            String[] sids = ids.Split(';');
	            int indexcolor = 0;
	            bool special = SpecialFeatures.SpecialFeaturesMgt.AreSpecialFeaturesEnabled();
	            if (sids.Length != 0)
	            {           	
	            	List<ToolStripItem> tsi = new List<ToolStripItem>();
		            foreach(String id in sids)
		            {
		            	if (String.IsNullOrEmpty(id))
		            	{
		            		
		            	}
		            	else if (id == _sepTag)
		            	{
		            		// 
				        	// toolStripSeparator1
				        	// 
				        	var toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
				        	toolStripSeparator1.Name = "toolStripSeparator1";
				        	toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
				        	tsi.Add(toolStripSeparator1);
		            	}
		            	else
		            	{
		            		// 
				        	// toolStripButton1
				        	// 
				        	// _menuEntriesRequiringSpecialFeatures
				        	String lbl = "???";
				        	Object menu = MyTools.FindControl(daddy, id);
				        	ToolStripMenuItem tsimenu = null;
				        	bool docreate = true;
				        	if (menu != null)
				        	{
				        		tsimenu = menu as ToolStripMenuItem;
				        		if (tsimenu != null)
				        		{
				        			lbl = tsimenu.Text;
				        			
				        			if (!special)
				        			{
					        			foreach(var o in daddy._menuEntriesRequiringSpecialFeatures)
					        			{
					        				if (o == tsimenu)
					        				{
					        					docreate = false;
					        				}
					        			}
				        			}
				        		}
				        	}
				        	
				        	if (docreate)
				        	{
					        	var toolStripButton1 = new System.Windows.Forms.ToolStripButton();
					        	toolStripButton1.Text = lbl;
					        	toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
					        	if ((daddy._imgMenus != null) && (daddy._imgMenus.ContainsKey(id)))
				                	toolStripButton1.Image = daddy._imgMenus[id];
								else
								{
									//toolStripButton1.Image = daddy._imgMenus["nomenuicon"];
									toolStripButton1.Image = MyTools.CreateToolbarImage(toolStripButton1.Text, (_colorandpen[indexcolor].Item2)?Color.White:Color.Black, Color.FromName(_colorandpen[indexcolor].Item1));
									indexcolor = (indexcolor + 1)%(_colorandpen.Count);
								}
					        	toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
					        	toolStripButton1.Name = id;
					        	toolStripButton1.Tag = id;
					        	toolStripButton1.Size = new System.Drawing.Size(23, 22);
					        	
					        	toolStripButton1.Click += new System.EventHandler(daddy.ToolStripButtonGenericClick);
					        	tsi.Add(toolStripButton1);
				        	}
		            	}
		            }
		            
		            if (tsi.Count != 0)
		            {
			            daddy._shortcutToolstrip = new System.Windows.Forms.ToolStrip();
			            // 
			        	// toolStrip1
			        	// 
			        	daddy._shortcutToolstrip.Items.AddRange(tsi.ToArray());
			        	daddy._shortcutToolstrip.Location = new System.Drawing.Point(0, 0);
			        	daddy._shortcutToolstrip.Name = "toolStrip1";
			        	daddy._shortcutToolstrip.Size = new System.Drawing.Size(827, 25);
			        	daddy._shortcutToolstrip.TabIndex = 10;
			        	daddy._shortcutToolstrip.Text = "toolStrip1";
			        	
			        	// Il faut s'insérer avant MenuStrip1
			        	int pos = daddy.Controls.IndexOf(daddy.menuStrip1);
			        	daddy.Controls.Add(daddy._shortcutToolstrip);
			        	daddy.Controls.SetChildIndex(daddy._shortcutToolstrip, pos);
		            }
	            }
			}
			catch(Exception){}
		}
		
		void TbbtnSaveClick(object sender, EventArgs e)
		{
			String tbids = "";
			if (tblvAdded.Items.Count != 0)
			{
	    		foreach(ListViewItem item in tblvAdded.Items)
	    		{
	    			String tag = item.Tag as String;
					if (!String.IsNullOrEmpty(tag))
					{
		    			if (tag != _sepTag)
		    			{
		    				tbids += tag + ";";
		    			}
		    			else
		    			{
		    				tbids += _sepTag + ";";
		    			}
					}
	    		}		
			}

        	_daddy.UpdateConfFile("toolbarids", tbids);
        	
        	// On (re)crée la toolbar
        	CreateToolbar(_daddy);
        	_daddy.RefreshMenuEnableStatus();
        	
			this.Close();
		}
		
		
		void TbbtnCancelClick(object sender, EventArgs e)
		{
			this.Close();
		}
		
		void TbbtnRemoveClick(object sender, EventArgs e)
		{
			List<ListViewItem> itemstoremove = new List<ListViewItem>();
			foreach(int i in tblvAdded.SelectedIndices)
			{
				// on ajoute, en excluant les entrées sans tag
				var item = tblvAdded.Items[i];
				itemstoremove.Add(item);
				String tag = item.Tag as String;
				if (!String.IsNullOrEmpty(tag))
				{
					if (tag != _sepTag)
						_addedindex.Remove(tag);
				}
			}
			foreach(var it in itemstoremove)
			{
				tblvAdded.Items.Remove(it);
			}
		}
		void TbbtnAddClick(object sender, EventArgs e)
		{
			foreach(int i in tblvAvail.SelectedIndices)
			{
				// on ajoute, en excluant les entrées sans tag
				var item = tblvAvail.Items[i];
				String tag = item.Tag as String;
				if (!String.IsNullOrEmpty(tag))
				{
					if ((tag != _sepTag) && (_addedindex.Contains(tag)))
					{
						// Pas d'ajout en double de vraies entrées
					}
					else
					{
						AddItemToRight(item);
					}
				}
			}
		}
		
		private bool IsSelectionEmpty()
        {
        	ListView.SelectedListViewItemCollection collection = tblvAdded.SelectedItems;
            if ((collection != null) && (collection.Count != 0))
            {
            	return false;
            }
            else
            	return true;
        }
		
		void TbbtnMoveUpClick(object sender, EventArgs e)
		{
			if (IsSelectionEmpty())
				return;
	         ListView.SelectedListViewItemCollection collection = tblvAdded.SelectedItems;
	         
	         // On parcourt dans l'ordre chrono
	         // Ne fait rien si le premier item de la collection est en première position de la liste
	         if (collection[0].Index == 0)
	             return;
	
	         foreach (ListViewItem item in collection)
	         {
	             int currentIndex = item.Index;
	             if (currentIndex > 0)
	             {
	                 tblvAdded.Items.RemoveAt(currentIndex);
	                 tblvAdded.Items.Insert(currentIndex - 1, item);
	             }
	         }
		}
		void TbbtnMoveDownClick(object sender, EventArgs e)
		{
			if (IsSelectionEmpty())
				return;
        	
            ListView.SelectedListViewItemCollection collection = tblvAdded.SelectedItems;
            
            // On parcourt dans l'ordre chrono inverse
            // Ne fait rien si le dernier item de la collection est en dernière position de la liste
            if (collection[collection.Count - 1].Index == (tblvAdded.Items.Count - 1))
                return;

            for (int i = collection.Count - 1; i >= 0; i--)
            {
                ListViewItem item = collection[i];
                int currentIndex = item.Index;
                if (currentIndex < (tblvAdded.Items.Count - 1))
                {
                    tblvAdded.Items.RemoveAt(currentIndex);
                    tblvAdded.Items.Insert(currentIndex + 1, item);
                }
            }
		}
	}
}
