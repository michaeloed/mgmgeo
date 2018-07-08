using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using MyGeocachingManager.Geocaching;

/// <summary>
/// Delegate used to indicate if a TabPage can be closed
/// Called right before potential closure of a TabPage
/// </summary>
/// <param name="indx">TabPage index</param>
/// <returns>True if TabPage can be closed</returns>
public delegate bool PreRemoveTab(int indx);

namespace MyGeocachingManager
{
    /// <summary>
    /// Custom tab control use un CacheDetail
    /// </summary>
    public class TabControlEx : TabControl
    {
    	MainWindow _daddy = null;
    	
        /// <summary>
        /// Constructor
        /// </summary>
        public TabControlEx(MainWindow daddy)
            : base()
        {
        	_daddy = daddy;
            PreRemoveTabPage = null;
            this.DrawMode = TabDrawMode.OwnerDrawFixed;
        }

        /// <summary>
        /// Reference to an instance of a PreRemoveTab delegate
        /// Can be null
        /// </summary>
        public PreRemoveTab PreRemoveTabPage;

        /// <summary>
        /// Image for close button displayed on each TabPage
        /// </summary>
        public Image _CloseButton = null;

        /// <summary>
        /// Image for gear / configuration button that can be displayed on specific TabPage
        /// </summary>
        public Image _GearButton = null;

        /// <summary>
        /// Image for earth button that can be displayed on specific TabPage
        /// </summary>
        public Image _EarthButton = null;

        /// <summary>
        /// Reference to a function that will be called if a gear button is pushed
        /// Cann be null
        /// </summary>
        public Func<Int32, Int32, bool> _fGearProcessing = null;

        private int _iClicWidth = 16;
        private int _iClicHeight = 16;

        /// <summary>
        /// Context menu of a TabPage
        /// </summary>
        public ContextMenuStrip _mnuContextMenu = null;

        /// <summary>
        /// -1 : not a special page
        /// 1 : do not close, tab page map
        /// 2 : action tab
        /// Checks if a TabPage is special
        /// </summary>
        /// <param name="page">Page to check</param>
        /// <returns>
        /// -1 : not a special page
        /// 1 : do not close, tab page map
        /// 2 : action tab
        /// </returns>
        public int IsSpecialPage(TabPage page)
        {
            return IsSpecialPageFromTag(page.Tag);
        }

        /// <summary>
        /// -1 : not a special page
        /// 1 : do not close, tab page map
        /// 2 : action tab
        /// Checks if a TabPage is special
        /// </summary>
        /// <param name="tag">tag associated to a page to check</param>
        /// <returns>
        /// -1 : not a special page
        /// 1 : do not close, tab page map
        /// 2 : action tab
        /// </returns>
        public int IsSpecialPageFromTag(Object tag)
        {
            if (tag != null)
            {
                if (tag is Int32)
                {
                    return ((Int32)(tag));
                }
            }
            return -1;
        }

        /// <summary>
        /// Custom OnDrawItem
        /// </summary>
        /// <param name="e">mouse events</param>
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            Rectangle r = e.Bounds;
            Rectangle or = r;
            r = GetTabRect(e.Index);
            r.Offset(2, 2);
            r.Width = _iClicWidth;
            r.Height = _iClicHeight;
            Brush b = new SolidBrush(Color.Black);

            // We display close button only if tabpage a tag != true
            TabPage page = this.TabPages[e.Index];
            Geocache geo = page.Tag as Geocache;
            Image geotype = null;
            int iExtraIconWidth = 0;
            if (geo != null)
            {
            	// C'est une géocache, donc on affiche l'icone en plus à côté
            	try
            	{
            		geotype = _daddy.getImageFromIndex(_daddy.getIndexImages(geo._Type));
            	}
            	catch(Exception)
            	{
            	}
            }
            
            int iSpecial = IsSpecialPage(page);
            bool bNoClose = (iSpecial == 1);
            bool bGear = (iSpecial == 2);
            string titel = page.Text;
            Font f = this.Font;
            if (!bNoClose)
            {
                if (!bGear)
                {
                    if (_CloseButton == null)
                    {
                        Pen p = new Pen(b);
                        e.Graphics.DrawLine(p, r.X, r.Y, r.X + r.Width, r.Y + r.Height);
                        e.Graphics.DrawLine(p, r.X + r.Width, r.Y, r.X, r.Y + r.Height);
                    }
                    else
                    {
                        e.Graphics.DrawImage(_CloseButton, new Rectangle(r.X, r.Y, r.Width, r.Height));
                    }
                    
                    // L'icone du type de la cache ?
                    if (geotype != null)
                    {
                    	e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    	e.Graphics.DrawImage(geotype, new Rectangle(r.X + _iClicWidth, r.Y, r.Width, r.Height));
                    	iExtraIconWidth = r.Width;
                    }
                }
                else
                {
                    e.Graphics.DrawImage(_GearButton, new Rectangle(r.X, r.Y, r.Width, r.Height));
                }

                e.Graphics.DrawString(titel, f, b, new PointF(r.X + _iClicWidth + iExtraIconWidth, r.Y));
            }
            else
            {
                // La page Earth ? 
                if ((IsSpecialPage(this.TabPages[e.Index]) == 1) && (_EarthButton != null))
                {
                    e.Graphics.DrawImage(_EarthButton, new Rectangle(r.X, r.Y, r.Width, r.Height));
                }

                e.Graphics.DrawString(titel, f, b, new PointF(r.X + _iClicWidth, r.Y));
            }
        }

        private void HideShowMenuRange(bool bVisible, int ideb, int ifin)
        {
            for (int i = ideb; i <= ifin; i++)
            {
                _mnuContextMenu.Items[i].Visible = bVisible;
            }
        }

        /// <summary>
        /// Handler for mouse click
        /// </summary>
        /// <param name="e">Mouse event</param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Point p = e.Location;
                for (int i = 0; i < TabCount; i++)
                {
                    Rectangle r = GetTabRect(i);
                    r.Offset(2, 2);
                    r.Width = _iClicWidth;
                    r.Height = _iClicHeight;
                    if (r.Contains(p))
                    {
                        int iSpecial = IsSpecialPage(TabPages[i]);
                        if (iSpecial == 1)  // Earth tab
                        {
                            // No close
                            if (_EarthButton != null)
                            {
                                // On affiche le menu contextuel
                                DisplayContextMenu(e);
                            }
                            return;
                        }
                        else if (iSpecial >= 2)  // Gear processing
                        {
                            // No close
                            if (_fGearProcessing != null)
                                _fGearProcessing(i, iSpecial);
                            return;
                        }
                        else
                            CloseTab(i);
                    }
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                DisplayContextMenu(e);
            }
        }

        private void DisplayContextMenu(MouseEventArgs e)
        {
            DisplayContextMenu(e.Location);
        }

        /// <summary>
        /// Display context menu of a tab page
        /// </summary>
        /// <param name="pt">Point to display the menu</param>
        public void DisplayContextMenu(Point pt)
        {
            if (_mnuContextMenu != null)
            {
                // On prépare le menu contextuel
                // Par défaut on masque tout ce qui est relatif au fond carto
                HideShowMenuRange(true, 0, 2);
                HideShowMenuRange(false, 3, CacheDetail.MENUMAPOUTILS);

                if (SelectedTab.Tag == null)
                {
                    // C'est un onglet non géocache, on laisse juste de quoi fermer les onglets
                    HideShowMenuRange(false, 0, 0);
                }
                else if (IsSpecialPage(SelectedTab) == 1)
                {
                    // un onglet de fond carto, menu spécial
                    HideShowMenuRange(false, 0, 2);
                    HideShowMenuRange(true, 3, CacheDetail.MENUMAPOUTILS);
                }
                else
                {
                    // Un onglet de géocache, pas de changement
                }

                _mnuContextMenu.Show(this, pt);
            }
        }

        /// <summary>
        /// Close a tab page
        /// If tab page is special, it will not be closed
        /// </summary>
        /// <param name="i">Tab page index</param>
        public void CloseTab(int i)
        {
            CloseTabForce(i, false);
        }

        /// <summary>
        /// Close a tab page
        /// If tab page is special, it will be closed ony if bForce is true
        /// </summary>
        /// <param name="i">Tab page index</param>
        /// <param name="bForce">If tab page is special, it will be closed ony if bForce is true</param>
        public void CloseTabForce(int i, bool bForce)
        {
            int iSpecial = IsSpecialPage(TabPages[i]);
            if (!bForce && (iSpecial == 1))
                return; // no close

            if (PreRemoveTabPage != null)
            {
                bool closeIt = PreRemoveTabPage(i);
                if (!closeIt)
                    return;
            }
            TabPages.Remove(TabPages[i]);
            if (TabPages.Count == 0)
            {
                //this.Parent.Hide();
            }
            else
            {
                // Select previous tab
                if (i != 0)
                    this.SelectedIndex = i - 1;
                else
                    this.SelectedIndex = 0;
            }
        }
    }
}