using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Runtime.InteropServices;

namespace SpaceEyeTools.EXControls 
{
    /// <summary>
    /// Custom listview with many enhanced features
    /// This is the one used in MGM MainForm
    /// </summary>
    public class EXListView : ListView {


        /// <summary>
        /// If true, selected items will not be highlighted, only manually selected items will be highlighted
        /// If false, selected items and manually selected items will be highlighted
        /// </summary>
        public bool _bUseHighlightOnly = false;

        /// <summary>
        /// clicked ListViewSubItem
        /// </summary>
        public ListViewItem.ListViewSubItem _clickedsubitem; //clicked ListViewSubItem

        /// <summary>
        /// clicked ListViewItem
        /// </summary>
        public ListViewItem _clickeditem; //clicked ListViewItem

        private int _col; //index of doubleclicked ListViewSubItem
        private TextBox txtbx; //the default edit control

        /// <summary>
        /// Index of clicked columnheader, indicates which column is currently sorted
        /// </summary>
        public int _sortcol; //index of clicked ColumnHeader
        private Brush _sortcolbrush; //color of items in sorted column
        private Brush _highlightbrush; //color of highlighted items
        private Brush _selectbrush; //color of selected items
        private int _cpadding; //padding of the embedded controls
            
        private const UInt32 LVM_FIRST = 0x1000;
        private const UInt32 LVM_SCROLL = (LVM_FIRST + 20);
        private const int WM_HSCROLL = 0x114;
        private const int WM_VSCROLL = 0x115;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_PAINT = 0x000F;
            
        /// <summary>
        /// Used for embedded control inside EXListView
        /// </summary>
        public struct EmbeddedControl {

            /// <summary>
            /// Embedded control reference
            /// </summary>
            public Control MyControl;

            /// <summary>
            /// Reference to EXControlListViewSubItem
            /// </summary>
            public EXControlListViewSubItem MySubItem;
        }
            
        private ArrayList _controls;
            
        [DllImport("user32.dll")]
        private static extern bool SendMessage(IntPtr hWnd, UInt32 m, int wParam, int lParam);
        
        /// <summary>
        /// Horrible reference to this ol' fuck' WndProc handler
        /// </summary>
        /// <param name="m">message</param>
        protected override void WndProc(ref Message m) {
            if (m.Msg == WM_PAINT) {
                foreach (EmbeddedControl c in _controls) {
                    Rectangle r = c.MySubItem.Bounds;
                    if (r.Y > 0 && r.Y < this.ClientRectangle.Height) {
                        c.MyControl.Visible = true;
                        c.MyControl.Bounds = new Rectangle(r.X + _cpadding, r.Y + _cpadding, r.Width - (2 * _cpadding), r.Height - (2 * _cpadding));
                    } else {
                        c.MyControl.Visible = false;
                    }
                }
            }
            switch (m.Msg) {
                case WM_HSCROLL:
                case WM_VSCROLL:
                case WM_MOUSEWHEEL:
                    this.Focus();
                    break;
            }
            base.WndProc(ref m);
        }
        
        private void ScrollMe(int x, int y) {
            SendMessage((IntPtr) this.Handle, LVM_SCROLL, x, y);
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public EXListView() {
            _cpadding = 4;
            _controls = new ArrayList();
            _sortcol = -1;
            _sortcolbrush = SystemBrushes.ControlLight;
            _highlightbrush = SystemBrushes.Highlight;
            _selectbrush = SystemBrushes.Highlight;
            this.OwnerDraw = true;
            this.FullRowSelect = true;
            this.View = View.Details;
            this.MouseDown += new MouseEventHandler(this_MouseDown);
            this.MouseDoubleClick += new MouseEventHandler(this_MouseDoubleClick);
            this.DrawColumnHeader += new DrawListViewColumnHeaderEventHandler(this_DrawColumnHeader);
            this.DrawSubItem += new DrawListViewSubItemEventHandler(this_DrawSubItem);
            this.MouseMove += new MouseEventHandler(this_MouseMove);
            this.ColumnClick += new ColumnClickEventHandler(this_ColumnClick);
            // #Fix WinXP
            this.ColumnWidthChanging += new ColumnWidthChangingEventHandler(this_ColumnWidthChanging);
            txtbx = new TextBox();
            txtbx.Visible = false;
            this.Controls.Add(txtbx);
            txtbx.Leave += new EventHandler(c_Leave);
            txtbx.KeyPress += new KeyPressEventHandler(txtbx_KeyPress);

            // Remove flickering effect :-)
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.Opaque, true);
        }
        
        /// <summary>
        /// Add a control to a sub item
        /// </summary>
        /// <param name="control">control to add</param>
        /// <param name="subitem">subitem to host this control</param>
        public void AddControlToSubItem(Control control, EXControlListViewSubItem subitem) {
            this.Controls.Add(control);
            subitem.MyControl = control;
            EmbeddedControl ec;
            ec.MyControl = control;
            ec.MySubItem = subitem;
            this._controls.Add(ec);
        }
        
        /// <summary>
        /// Remove a control from a sub item
        /// </summary>
        /// <param name="subitem">reference to a subitem</param>
        public void RemoveControlFromSubItem(EXControlListViewSubItem subitem) {
            Control c = subitem.MyControl;
            for (int i = 0; i < this._controls.Count; i++) {
                if (((EmbeddedControl) this._controls[i]).MySubItem == subitem) {
                    this._controls.RemoveAt(i);
                    subitem.MyControl = null;
                    this.Controls.Remove(c);
                    c.Dispose();
                    return;
                }
            }
        }   

        /// <summary>
        /// Get / Set ControlPadding
        /// </summary>
        public int ControlPadding {
            get {return _cpadding;}
            set {_cpadding = value;}
        }
        
        /// <summary>
        /// Get / Set brush used for sorting
        /// </summary>
        public Brush MySortBrush {
            get {return _sortcolbrush;}
            set {_sortcolbrush = value;}
        }
        
        /// <summary>
        /// Get / Set brush used for highlight
        /// </summary>
        public Brush MyHighlightBrush {
            get {return _highlightbrush;}
            set {_highlightbrush = value;}
        }

        /// <summary>
        /// Get / Set brush used for selection
        /// </summary>
        public Brush MySelectBrush
        {
            get { return _selectbrush; }
            set { _selectbrush = value; }
        }
        
        private void txtbx_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char) Keys.Return) {
                _clickedsubitem.Text = txtbx.Text;
                txtbx.Visible = false;
                _clickeditem.Tag = null;
            }
        }
        
        private void c_Leave(object sender, EventArgs e) {
            Control c = (Control) sender;
            _clickedsubitem.Text = c.Text;
            c.Visible = false;
            _clickeditem.Tag = null;
        }

        // #Fix WinXP
        private void this_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            EXColumnHeader col = this.Columns[e.ColumnIndex] as EXColumnHeader;
            if (col._bHidden)
            {
                e.Cancel = true;
                e.NewWidth = 0;
            }
            else
            {
                if (e.NewWidth < 10)
                    e.NewWidth = 10;
            }
        }

        private void this_MouseDown(object sender, MouseEventArgs e) {
            ListViewHitTestInfo lstvinfo = this.HitTest(e.X, e.Y);
            ListViewItem.ListViewSubItem subitem = lstvinfo.SubItem;
            if (subitem == null) return;
            int subx = subitem.Bounds.Left;
            if (subx < 0) {
                this.ScrollMe(subx, 0);
            }
        }
        
        private void this_MouseDoubleClick(object sender, MouseEventArgs e) {
            EXListViewItem lstvItem = this.GetItemAt(e.X, e.Y) as EXListViewItem;
            if (lstvItem == null) return;
            _clickeditem = lstvItem;
            int x = lstvItem.Bounds.Left;
            int i;
            for (i = 0; i < this.Columns.Count; i++) {
                x = x + this.Columns[i].Width;
                if (x > e.X) {
                    x = x - this.Columns[i].Width;
                    _clickedsubitem = lstvItem.SubItems[i];
                    _col = i;
                    break;
                }
            }
            if (!(this.Columns[i] is EXColumnHeader)) return;
            EXColumnHeader col = (EXColumnHeader) this.Columns[i];
            if (col.GetType() == typeof(EXEditableColumnHeader)) {
                EXEditableColumnHeader editcol = (EXEditableColumnHeader) col;
                if (editcol.MyControl != null) {
                    Control c = editcol.MyControl;
                    if (c.Tag != null) {
                        this.Controls.Add(c);
                        c.Tag = null;
                        if (c is ComboBox) {
                            ((ComboBox) c).SelectedValueChanged += new EventHandler(cmbx_SelectedValueChanged);
                        }
                        c.Leave += new EventHandler(c_Leave);
                    }
                    c.Location = new Point(x, this.GetItemRect(this.Items.IndexOf(lstvItem)).Y);
                    c.Width = this.Columns[i].Width;
                    if (c.Width > this.Width) c.Width = this.ClientRectangle.Width;
                    c.Text = _clickedsubitem.Text;
                    c.Visible = true;
                    c.BringToFront();
                    c.Focus();
                } else {
                    txtbx.Location = new Point(x, this.GetItemRect(this.Items.IndexOf(lstvItem)).Y);
                    txtbx.Width = this.Columns[i].Width;
                    if (txtbx.Width > this.Width) txtbx.Width = this.ClientRectangle.Width;
                    txtbx.Text = _clickedsubitem.Text;
                    txtbx.Visible = true;
                    txtbx.BringToFront();
                    txtbx.Focus();
                }
            } else if (col.GetType() == typeof(EXBoolColumnHeader)) {
                EXBoolColumnHeader boolcol = (EXBoolColumnHeader) col;
                if (boolcol.Editable) {
                    EXBoolListViewSubItem boolsubitem = (EXBoolListViewSubItem) _clickedsubitem;
                    if (boolsubitem.BoolValue == true) {
                        boolsubitem.BoolValue = false;
                    } else {
                        boolsubitem.BoolValue = true;
                    }
                    this.Invalidate(boolsubitem.Bounds);
                }
            }
        }
        
        private void cmbx_SelectedValueChanged(object sender, EventArgs e) {
            if (((Control) sender).Visible == false || _clickedsubitem == null) return;
            if (sender.GetType() == typeof(EXComboBox)) {
                EXComboBox excmbx = (EXComboBox) sender;
                object item = excmbx.SelectedItem;
                //Is this an combobox item with one image?
                if (item.GetType() == typeof(EXComboBox.EXImageItem)) {
                    EXComboBox.EXImageItem imgitem = (EXComboBox.EXImageItem) item;
                    //Is the first column clicked -- in that case it's a ListViewItem
                    if (_col == 0) {
                        if (_clickeditem.GetType() == typeof(EXImageListViewItem)) {
                            ((EXImageListViewItem) _clickeditem).MyImage = imgitem.MyImage;
                        } else if (_clickeditem.GetType() == typeof(EXMultipleImagesListViewItem)) {
                            EXMultipleImagesListViewItem imglstvitem = (EXMultipleImagesListViewItem) _clickeditem;
			                imglstvitem.MyImages.Clear();
			                imglstvitem.MyImages.AddRange(new object[] {imgitem.MyImage});
                        }
                    //another column than the first one is clicked, so we have a ListViewSubItem
                    } else {
                        if (_clickedsubitem.GetType() == typeof(EXImageListViewSubItem)) {
                            EXImageListViewSubItem imgsub = (EXImageListViewSubItem) _clickedsubitem;
                            imgsub.MyImage = imgitem.MyImage;
                        } else if (_clickedsubitem.GetType() == typeof(EXMultipleImagesListViewSubItem)) {
                            EXMultipleImagesListViewSubItem imgsub = (EXMultipleImagesListViewSubItem) _clickedsubitem;
                            imgsub.MyImages.Clear();
                            imgsub.MyImages.Add(imgitem.MyImage);
			                imgsub.MyValue = imgitem.MyValue;
                        }
                    }
                    //or is this a combobox item with multiple images?
                } else if (item.GetType() == typeof(EXComboBox.EXMultipleImagesItem)) {
                    EXComboBox.EXMultipleImagesItem imgitem = (EXComboBox.EXMultipleImagesItem) item;
                    if (_col == 0) {
                        if (_clickeditem.GetType() == typeof(EXImageListViewItem)) {
                            ((EXImageListViewItem) _clickeditem).MyImage = (Image) imgitem.MyImages[0];
                        } else if (_clickeditem.GetType() == typeof(EXMultipleImagesListViewItem)) {
                            EXMultipleImagesListViewItem imglstvitem = (EXMultipleImagesListViewItem) _clickeditem;
			                imglstvitem.MyImages.Clear();
			                imglstvitem.MyImages.AddRange(imgitem.MyImages);
                        }
                    } else {
                        if (_clickedsubitem.GetType() == typeof(EXImageListViewSubItem)) {
                            EXImageListViewSubItem imgsub = (EXImageListViewSubItem) _clickedsubitem;
                            if (imgitem.MyImages != null) {
                                imgsub.MyImage = (Image) imgitem.MyImages[0];
                            }
                        } else if (_clickedsubitem.GetType() == typeof(EXMultipleImagesListViewSubItem)) {
                            EXMultipleImagesListViewSubItem imgsub = (EXMultipleImagesListViewSubItem) _clickedsubitem;
                            imgsub.MyImages.Clear();
			                imgsub.MyImages.AddRange(imgitem.MyImages);
			                imgsub.MyValue = imgitem.MyValue;
                        }
                    }
                }
            }
            ComboBox c = (ComboBox) sender;
            _clickedsubitem.Text = c.Text;
            c.Visible = false;
            _clickeditem.Tag = null;
        }
        
        private void this_MouseMove(object sender, MouseEventArgs e) 
        {
            
            ListViewItem item = this.GetItemAt(e.X, e.Y);
            if (item != null /*&& item.Tag == null*/) {
                this.Invalidate(item.Bounds);
                item.Tag = "t";
            }
        }
        
        private void this_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e) {
            e.DrawDefault = true;
        }

        private void this_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawBackground();
            
            // #Fix WinXP
            EXColumnHeader col = this.Columns[e.ColumnIndex] as EXColumnHeader;
            if (col._bHidden)
                return;

            if (e.ColumnIndex == _sortcol)
            {
                e.Graphics.FillRectangle(_sortcolbrush, e.Bounds);
            }
            // hightlight
            {
                EXListViewItem item = (EXListViewItem)e.Item;
                if (item._bHighlighted)
                {
                    e.Graphics.FillRectangle(_selectbrush, e.Bounds);
                }
            }

            if (!_bUseHighlightOnly)
            {
                if ((e.ItemState & ListViewItemStates.Selected) != 0)
                {
                    e.Graphics.FillRectangle(_highlightbrush, e.Bounds);
                }
            }

            int fonty = e.Bounds.Y + ((int)(e.Bounds.Height / 2)) - ((int)(e.SubItem.Font.Height / 2));
            int x = e.Bounds.X + 2;
            if (e.ColumnIndex == 0)
            {
                EXListViewItem item = (EXListViewItem)e.Item;

                if (item.GetType() == typeof(EXImageListViewItem))
                {
                    EXImageListViewItem imageitem = (EXImageListViewItem)item;
                    if (imageitem.MyImage != null)
                    {
                        Image img = imageitem.MyImage;
                        int imgy = e.Bounds.Y + ((int)(e.Bounds.Height / 2)) - ((int)(img.Height / 2));
                        e.Graphics.DrawImage(img, x, imgy, img.Width, img.Height);
                        x += img.Width + 2;
                    }
                }

                e.Graphics.DrawString(e.SubItem.Text, e.SubItem.Font, new SolidBrush(e.SubItem.ForeColor), x, fonty);
                //System.Media.SystemSounds.Beep.Play();
                return;
            }
            EXListViewSubItemAB subitem = e.SubItem as EXListViewSubItemAB;
            if (subitem == null)
            {
                e.DrawDefault = true;
            }
            else
            {
                x = subitem.DoDraw(e, x, this.Columns[e.ColumnIndex] as EXColumnHeader);
                //e.Graphics.DrawString(e.SubItem.Text, e.SubItem.Font, new SolidBrush(e.SubItem.ForeColor), x, fonty);

                // #Fix2
                e.Graphics.DrawString(e.SubItem.Text, e.SubItem.Font,
                    new SolidBrush(e.SubItem.ForeColor),
                    new Rectangle(x, fonty, col.Width, e.SubItem.Font.Height));
            }
        }

        private ListViewComparer GetColumnComparer(int iCol)
        {
            ListViewComparer cmp = this.ListViewItemSorter as ListViewComparer;
            if (cmp == null)
                return null;
            else
            {
                while (cmp != null)
                {
                    if (cmp._col == iCol)
                    {
                        // found it ;-)
                        return cmp;
                    }
                    else
                    {
                        cmp = cmp._nextComparer;
                    }
                }
                return null;
            }
        }

        private void CreateMonoSorter(object sender, ColumnClickEventArgs e)
        {
            // Ok, we erase all arrow marks
            for (int i = 0; i < this.Columns.Count; i++)
            {
                this.Columns[i].ImageKey = null;
            }

            // single column sorting, easy
            ListViewComparer foundcmp = GetColumnComparer(e.Column);
            if (foundcmp == null)
            {
                // Click on a new column
                // Nominal process
            }
            else
            {
                // Existing column, we invert the sort order
                // AND set the good sortcolumn
                _sortcol = foundcmp._col;
            }

            if (e.Column != _sortcol)
            {
                _sortcol = e.Column;
                this.Sorting = SortOrder.Ascending;
                this.Columns[e.Column].ImageKey = "up";
            }
            else
            {
                if (this.Sorting == SortOrder.Ascending)
                {
                    this.Sorting = SortOrder.Descending;
                    this.Columns[e.Column].ImageKey = "down";
                }
                else
                {
                    this.Sorting = SortOrder.Ascending;
                    this.Columns[e.Column].ImageKey = "up";
                }
            }

            // We have to create a new sorter
            // Create comparer
            ListViewComparer comparer = null;
            if (_sortcol == 0)
            {
                //ListViewItem
                if (this.Items[0].GetType() == typeof(EXListViewItem))
                {
                    //sorting on text
                    comparer = new ListViewItemComparerText(e.Column, this.Sorting);
                }
                else
                {
                    //sorting on value
                    comparer = new ListViewItemComparerValue(e.Column, this.Sorting);
                }
            }
            else
            {
                //ListViewSubItem
                if (this.Items[0].SubItems[_sortcol].GetType() == typeof(EXListViewSubItemAB))
                {
                    //sorting on text
                    comparer = new ListViewSubItemComparerText(e.Column, this.Sorting);
                }
                else
                {
                    //sorting on value
                    comparer = new ListViewSubItemComparerValue(e.Column, this.Sorting);
                }
            }

            // Apply the new comparer (to do that, we need to remove it and reapply it)
            //this.ListViewItemSorter = null;
            this.ListViewItemSorter = comparer;
        }

        private void CreateMultiSorter(object sender, ColumnClickEventArgs e)
        {
            // Multi column sorting
            // Is it a new column ?
            ListViewComparer foundcmp = GetColumnComparer(e.Column);
            _sortcol = e.Column;
            if (foundcmp == null)
            {
                // Click on a new column
                this.Sorting = SortOrder.Ascending;
                this.Columns[e.Column].ImageKey = "up";
            }
            else
            {
                // Existing column, we invert the sort order
                if (foundcmp._order == SortOrder.Ascending)
                {
                    foundcmp._order = SortOrder.Descending;
                    this.Sorting = SortOrder.Descending;
                    this.Columns[e.Column].ImageKey = "down";
                }
                else
                {
                    foundcmp._order = SortOrder.Ascending;
                    this.Sorting = SortOrder.Ascending;
                    this.Columns[e.Column].ImageKey = "up";
                }

                // nothing else to do, we go out
                return;
            }

            // We have to create a new sorter
            // Create comparer
            ListViewComparer comparer = null;
            if (_sortcol == 0)
            {
                //ListViewItem
                if (this.Items[0].GetType() == typeof(EXListViewItem))
                {
                    //sorting on text
                    comparer = new ListViewItemComparerText(e.Column, this.Sorting);
                }
                else
                {
                    //sorting on value
                    comparer = new ListViewItemComparerValue(e.Column, this.Sorting);
                }
            }
            else
            {
                //ListViewSubItem
                if (this.Items[0].SubItems[_sortcol].GetType() == typeof(EXListViewSubItemAB))
                {
                    //sorting on text
                    comparer = new ListViewSubItemComparerText(e.Column, this.Sorting);
                }
                else
                {
                    //sorting on value
                    comparer = new ListViewSubItemComparerValue(e.Column, this.Sorting);
                }
            }

            // Get current comparer : CANNOT be null, already checked in the regular columnclick handler
            ListViewComparer firstcmp = this.ListViewItemSorter as ListViewComparer;
            if (foundcmp == null)
            {
                // We just have to add this comparer at the end
                firstcmp.GetLastComparer()._nextComparer = comparer;
            }
            else
            {
                // It was an existing comparer, and we already inverted its sort order
            }

            // Apply the new comparer (to do that, we need to remove it and reapply it)
            ListViewComparer cmp = this.ListViewItemSorter as ListViewComparer;
            this.ListViewItemSorter = null;
            this.ListViewItemSorter = cmp;
        }


        private void this_ColumnClick(object sender, ColumnClickEventArgs e) 
        {
            if (this.Items.Count == 0) return;

            // Prepare sort by emptying tag (fucking stupid idea IMHO)
            for (int i = 0; i < this.Items.Count; i++)
            {
                this.Items[i].Tag = null;
            }

            if ((Control.ModifierKeys & Keys.Control) > 0)
            {
                // If Ctrl key is down, add this column to the sequence
                if (this.ListViewItemSorter == null)
                    CreateMonoSorter(sender, e);
                else
                    CreateMultiSorter(sender, e);
            }
            else
            {
                CreateMonoSorter(sender, e);
            }

        }

        /// <summary>
        /// Custom IComparer used to sort EXListView
        /// </summary>
        public class ListViewComparer : System.Collections.IComparer
        {

            /// <summary>
            /// Index of sorted column
            /// </summary>
            public int _col;

            /// <summary>
            /// Sort order
            /// </summary>
            public SortOrder _order;

            /// <summary>
            /// Comparers are chained.
            /// Reference to the next comparer in the chain
            /// </summary>
            public ListViewComparer _nextComparer = null;

            /// <summary>
            /// Constructor
            /// </summary>
            public ListViewComparer()
            {
                _col = 0;
                _order = SortOrder.Ascending;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="col">column index</param>
            /// <param name="order">sort order</param>
            public ListViewComparer(int col, SortOrder order)
            {
                _col = col;
                _order = order;
            }

            /// <summary>
            /// Compare method.
            /// shall not be used, always returns -1
            /// </summary>
            /// <param name="x">first object</param>
            /// <param name="y">second object</param>
            /// <returns>always -1</returns>
            public virtual  int Compare(object x, object y)
            {
                return -1;
            }

            private String ToStringPrivate()
            {
                String s = this.GetType().ToString() + "[" + _col.ToString() + "] - ";
                if (_order == SortOrder.Ascending)
                    s += "Ascending";
                else
                    s += "Descending";

                return s;
            }

            /// <summary>
            /// ToString override
            /// </summary>
            /// <returns>string value</returns>
            public override string ToString()
            {
                String s = ToStringPrivate();
                if (_nextComparer != null)
                    s += "\r\n" + _nextComparer.ToString();

                return s;
            }

            /// <summary>
            /// Returns last comparer of the chained list of comparers
            /// </summary>
            /// <returns>last comparer of the chained list of comparers</returns>
            public ListViewComparer GetLastComparer()
            {
                if (_nextComparer == null)
                    return this;
                else
                    return _nextComparer.GetLastComparer();
            }
        }

        /// <summary>
        /// Daughter class of ListViewComparer
        /// Specialized for subitem text comparison
        /// </summary>
        public class ListViewSubItemComparerText : ListViewComparer {
            
            
            /// <summary>
            /// Constructor
            /// </summary>
            public ListViewSubItemComparerText() {
                _col = 0;
                _order = SortOrder.Ascending;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="col">column index</param>
            /// <param name="order">sort order</param>
            public ListViewSubItemComparerText(int col, SortOrder order) {
                _col = col;
                _order = order;
            }

            /// <summary>
            /// Compare method
            /// </summary>
            /// <param name="x">first object ListViewItem</param>
            /// <param name="y">second object ListViewItem</param>
            /// <returns>-1 if x inferior to Y, 0 if equals, +1 if superior to y</returns>
            public override  int Compare(object x, object y)
            {
                int returnVal = -1;
                
                string xstr = ((ListViewItem) x).SubItems[_col].Text;
                string ystr = ((ListViewItem) y).SubItems[_col].Text;
                
                decimal dec_x;
                decimal dec_y;
                DateTime dat_x;
                DateTime dat_y;
                
                if (Decimal.TryParse(xstr, out dec_x) && Decimal.TryParse(ystr, out dec_y)) {
                    returnVal = Decimal.Compare(dec_x, dec_y);
                } else if (DateTime.TryParse(xstr, out dat_x) && DateTime.TryParse(ystr, out dat_y)) {
                    returnVal = DateTime.Compare(dat_x, dat_y);
                } else {
                    returnVal = String.Compare(xstr, ystr);
                }
                if (_order == SortOrder.Descending) returnVal *= -1;

                if ((_nextComparer != null) && (returnVal == 0))
                    return _nextComparer.Compare(x, y);

                return returnVal;
            }
        
        }

        /// <summary>
        /// Daughter class of ListViewComparer
        /// Specialized for sub item value comparison
        /// </summary>
        public class ListViewSubItemComparerValue : ListViewComparer
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public ListViewSubItemComparerValue() {
                _col = 0;
                _order = SortOrder.Ascending;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="col">column index</param>
            /// <param name="order">sort order</param>
            public ListViewSubItemComparerValue(int col, SortOrder order) {
                _col = col;
                _order = order;
            }
            
            /// <summary>
            /// Compare method
            /// </summary>
            /// <param name="x">first object EXListViewSubItemAB</param>
            /// <param name="y">second object EXListViewSubItemAB</param>
            /// <returns>-1 if x inferior to Y, 0 if equals, +1 if superior to y</returns>
            public override int Compare(object x, object y) {
                int returnVal = -1;
                
                string xstr = ((EXListViewSubItemAB) ((ListViewItem) x).SubItems[_col]).MyValue;
                string ystr = ((EXListViewSubItemAB) ((ListViewItem) y).SubItems[_col]).MyValue;
                
                decimal dec_x;
                decimal dec_y;
                DateTime dat_x;
                DateTime dat_y;
                
                if (Decimal.TryParse(xstr, out dec_x) && Decimal.TryParse(ystr, out dec_y)) {
                    returnVal = Decimal.Compare(dec_x, dec_y);
                } else if (DateTime.TryParse(xstr, out dat_x) && DateTime.TryParse(ystr, out dat_y)) {
                    returnVal = DateTime.Compare(dat_x, dat_y);
                } else {
                    returnVal = String.Compare(xstr, ystr);
                }
                if (_order == SortOrder.Descending) returnVal *= -1;

                if ((_nextComparer != null) && (returnVal == 0))
                    return _nextComparer.Compare(x, y);

                return returnVal;
            }
        
        }
	
        /// <summary>
        /// Specialized comparer for item text
        /// </summary>
	    public class ListViewItemComparerText : ListViewComparer 
        {
            
            /// <summary>
            /// Constructor
            /// </summary>
            public ListViewItemComparerText() {
                _col = 0;
                _order = SortOrder.Ascending;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="col">column index</param>
            /// <param name="order">sort order</param>
            public ListViewItemComparerText(int col, SortOrder order) {
                _col = col;
                _order = order;
            }
            
            /// <summary>
            /// Compare method
            /// </summary>
            /// <param name="x">first object ListViewItem</param>
            /// <param name="y">second object ListViewItem</param>
            /// <returns>-1 if x inferior to Y, 0 if equals, +1 if superior to y</returns>
            public override int Compare(object x, object y) {
                int returnVal = -1;
                
                string xstr = ((ListViewItem) x).Text;
                string ystr = ((ListViewItem) y).Text;
                
                decimal dec_x;
                decimal dec_y;
                DateTime dat_x;
                DateTime dat_y;
                
                if (Decimal.TryParse(xstr, out dec_x) && Decimal.TryParse(ystr, out dec_y)) {
                    returnVal = Decimal.Compare(dec_x, dec_y);
                } else if (DateTime.TryParse(xstr, out dat_x) && DateTime.TryParse(ystr, out dat_y)) {
                    returnVal = DateTime.Compare(dat_x, dat_y);
                } else {
                    returnVal = String.Compare(xstr, ystr);
                }
                if (_order == SortOrder.Descending) returnVal *= -1;

                if ((_nextComparer != null) && (returnVal == 0))
                    return _nextComparer.Compare(x, y);

                return returnVal;
            }
        
        }
	
        /// <summary>
        /// Comparer sepcialized for item value
        /// </summary>
	    public class ListViewItemComparerValue : ListViewComparer
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public ListViewItemComparerValue() {
                _col = 0;
                _order = SortOrder.Ascending;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="col">column index</param>
            /// <param name="order">sort order</param>
            public ListViewItemComparerValue(int col, SortOrder order) {
                _col = col;
                _order = order;
            }
            
            /// <summary>
            /// Compare method
            /// </summary>
            /// <param name="x">first object EXListViewItem</param>
            /// <param name="y">second object EXListViewItem</param>
            /// <returns>-1 if x inferior to Y, 0 if equals, +1 if superior to y</returns>
            public override int Compare(object x, object y) {
                int returnVal = -1;
                
                string xstr = ((EXListViewItem) x).MyValue;
                string ystr = ((EXListViewItem) y).MyValue;
                
                decimal dec_x;
                decimal dec_y;
                DateTime dat_x;
                DateTime dat_y;
                
                if (Decimal.TryParse(xstr, out dec_x) && Decimal.TryParse(ystr, out dec_y)) {
                    returnVal = Decimal.Compare(dec_x, dec_y);
                } else if (DateTime.TryParse(xstr, out dat_x) && DateTime.TryParse(ystr, out dat_y)) {
                    returnVal = DateTime.Compare(dat_x, dat_y);
                } else {
                    returnVal = String.Compare(xstr, ystr);
                }
                if (_order == SortOrder.Descending) returnVal *= -1;

                if ((_nextComparer != null) && (returnVal == 0))
                    return _nextComparer.Compare(x, y);

                return returnVal;
            }
        
        }
        
    }
    
    /// <summary>
    /// Specialization of ColumnHeader
    /// </summary>
    public class EXColumnHeader : ColumnHeader {

        // #Fix WinXP

        /// <summary>
        /// True if column is hiden (not displayed)
        /// </summary>
        public bool _bHidden = false;


        /// <summary>
        /// Constructor
        /// </summary>
        public EXColumnHeader()
        {
            
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">column label</param>
        public EXColumnHeader(string text) {
            this.Text = text;
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">column label</param>
        /// <param name="width">column width</param>
        public EXColumnHeader(string text, int width) {
            this.Text = text;
            this.Width = width;
        }
        
    }
    
    /// <summary>
    /// Specialized EXColumnHeader : editable column
    /// </summary>
    public class EXEditableColumnHeader : EXColumnHeader {
        
        private Control _control;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public EXEditableColumnHeader() {
            
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">column label</param>
        public EXEditableColumnHeader(string text) {
            this.Text = text;
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">column label</param>
        /// <param name="width">column width</param>
        public EXEditableColumnHeader(string text, int width) {
            this.Text = text;
            this.Width = width;
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">column label</param>
        /// <param name="control">column specific control for edition</param>
        public EXEditableColumnHeader(string text, Control control) {
            this.Text = text;
            this.MyControl = control;
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">colum label</param>
        /// <param name="control">column specific control for edition</param>
        /// <param name="width">column width</param>
        public EXEditableColumnHeader(string text, Control control, int width) {
            this.Text = text;
            this.MyControl = control;
            this.Width = width;
        }
        
        /// <summary>
        /// Get / Set control for edition
        /// </summary>
        public Control MyControl {
            get {return _control;}
            set {
                _control = value;
                _control.Visible = false;
                _control.Tag = "not_init";
            }
        }
        
    }
    
    /// <summary>
    /// specialized column for handling boolean values only
    /// </summary>
    public class EXBoolColumnHeader : EXColumnHeader {
        
        private Image _trueimage;
        private Image _falseimage;
        private bool _editable;
            
        /// <summary>
        /// Constructor
        /// </summary>
        public EXBoolColumnHeader() {
            init();
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">column label</param>
        public EXBoolColumnHeader(string text) {
            init();
            this.Text = text;
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">column label</param>
        /// <param name="width">column width</param>
        public EXBoolColumnHeader(string text, int width) {
            init();
            this.Text = text;
            this.Width = width;
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">column label</param>
        /// <param name="trueimage">column image for TRUE value</param>
        /// <param name="falseimage">column image for FALSE value</param>
        public EXBoolColumnHeader(string text, Image trueimage, Image falseimage) {
            init();
            this.Text = text;
            _trueimage = trueimage;
            _falseimage = falseimage;
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">column label</param>
        /// <param name="trueimage">column image for TRUE value</param>
        /// <param name="falseimage">column image for FALSE value</param>
        /// <param name="width">column width</param>
        public EXBoolColumnHeader(string text, Image trueimage, Image falseimage, int width) {
            init();
            this.Text = text;
            _trueimage = trueimage;
            _falseimage = falseimage;
            this.Width = width;
        }
        
        private void init() {
            _editable = false;
        }
        
        /// <summary>
        /// Get / Set image for TRUE value
        /// </summary>
        public Image TrueImage {
            get {return _trueimage;}
            set {_trueimage = value;}
        }
        
        /// <summary>
        /// Get / Set image for FALSE value
        /// </summary>
        public Image FalseImage {
            get {return _falseimage;}
            set {_falseimage = value;}
        }
        
        /// <summary>
        /// Get / Set scolumn status
        /// If true, column can be edited
        /// </summary>
        public bool Editable {
            get {return _editable;}
            set {_editable = value;}
        }
        
    }
    
    /// <summary>
    /// Abstract class for owner drawn sub item
    /// </summary>
    public abstract class EXListViewSubItemAB : ListViewItem.ListViewSubItem {
        
        private string _value = "";
        
        /// <summary>
        /// Constructor
        /// </summary>
        public EXListViewSubItemAB() {
            
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">sub item label</param>
        public EXListViewSubItemAB(string text) {
            this.Text = text;
        }
        
        /// <summary>
        /// Get / Set sub item value
        /// </summary>
        public string MyValue {
            get {return _value;}
            set {_value = value;}
        }
        
        /// <summary>
        /// Owner drawn method
        /// </summary>
        /// <param name="e">event</param>
        /// <param name="x">x coordinate</param>
        /// <param name="ch">column header</param>
        /// <returns>return the new x coordinate</returns>
        public abstract int DoDraw(DrawListViewSubItemEventArgs e, int x, EXControls.EXColumnHeader ch);

    }
    
    /// <summary>
    /// Custom sub item
    /// </summary>
    public class EXListViewSubItem : EXListViewSubItemAB {
        
        /// <summary>
        /// Constructor
        /// </summary>
        public EXListViewSubItem() {
            
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">sub item label</param>
        public EXListViewSubItem(string text) {
            this.Text = text;
        }

        /// <summary>
        /// Owner drawn method
        /// </summary>
        /// <param name="e">event</param>
        /// <param name="x">x coordinate</param>
        /// <param name="ch">column header</param>
        /// <returns>return the new x coordinate</returns>
        public override int DoDraw(DrawListViewSubItemEventArgs e, int x, EXControls.EXColumnHeader ch) {
            return x;
        }

    }
    
    /// <summary>
    /// Custom sub item holding a control
    /// </summary>
    public class EXControlListViewSubItem : EXListViewSubItemAB {
        
        private Control _control;
            
        /// <summary>
        /// Constructor
        /// </summary>
        public EXControlListViewSubItem() {
            
        }
        
        /// <summary>
        /// Set / Get control
        /// </summary>
        public Control MyControl {
            get {return _control;}
            set {_control = value;}
        }

        /// <summary>
        /// Owner drawn method
        /// </summary>
        /// <param name="e">event</param>
        /// <param name="x">x coordinate</param>
        /// <param name="ch">column header</param>
        /// <returns>return the new x coordinate</returns>
        public override int DoDraw(DrawListViewSubItemEventArgs e, int x, EXColumnHeader ch) {
            return x;
        }
        
    }
    
    /// <summary>
    /// Custom sub item with one image
    /// </summary>
    public class EXImageListViewSubItem : EXListViewSubItemAB {
        
        private Image _image;

        /// <summary>
        /// Constructor
        /// </summary>
        public EXImageListViewSubItem() {
            
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">sub item text</param>
        public EXImageListViewSubItem(string text) {
            this.Text = text;
        }
            
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="image">sub item image</param>
        public EXImageListViewSubItem(Image image) {
            _image = image;
        }
	
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="image">sub item image</param>
        /// <param name="value">sub item value</param>
        public EXImageListViewSubItem(Image image, string value) {
            _image = image;
            this.MyValue = value;
        }
	
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">sub item label</param>
        /// <param name="image">sub item image</param>
        /// <param name="value">sub item value</param>
        public EXImageListViewSubItem(string text, Image image, string value) {
            this.Text = text;
            _image = image;
            this.MyValue = value;
        }
        
        /// <summary>
        /// Get / Set sub item image
        /// </summary>
        public Image MyImage {
            get {return _image;}
            set {_image = value;}
        }

        /// <summary>
        /// Owner drawn method
        /// </summary>
        /// <param name="e">event</param>
        /// <param name="x">x coordinate</param>
        /// <param name="ch">column header</param>
        /// <returns>return the new x coordinate</returns>
        public override int DoDraw(DrawListViewSubItemEventArgs e, int x, EXControls.EXColumnHeader ch)
        {
            if (this.MyImage != null) {
                Image img = this.MyImage;
                int imgy = e.Bounds.Y + ((int) (e.Bounds.Height / 2)) - ((int) (img.Height / 2));
                e.Graphics.DrawImage(img, x, imgy, img.Width, img.Height);
                x += img.Width + 2;
            }
            return x;
        }
        
    }
    
    /// <summary>
    /// Custom sub item with multiple images
    /// </summary>
    public class EXMultipleImagesListViewSubItem : EXListViewSubItemAB {
        
        private ArrayList _images;
            
        /// <summary>
        /// Constructor
        /// </summary>
        public EXMultipleImagesListViewSubItem() {
            
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">sub item label</param>
        public EXMultipleImagesListViewSubItem(string text) {
            this.Text = text;
        }
            
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="images">sub item images</param>
        public EXMultipleImagesListViewSubItem(ArrayList images) {
            _images = images;
        }
	
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="images">sub item images</param>
        /// <param name="value">sub item value</param>
        public EXMultipleImagesListViewSubItem(ArrayList images, string value) {
            _images = images;
            this.MyValue = value;
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">sub item label</param>
        /// <param name="images">sub item images</param>
        /// <param name="value">sub item value</param>
        public EXMultipleImagesListViewSubItem(string text, ArrayList images, string value) {
            this.Text = text;
            _images = images;
            this.MyValue = value;
        }
        
        /// <summary>
        /// Get / Set sub item images
        /// </summary>
        public ArrayList MyImages {
            get {return _images;}
            set {_images = value;}
        }

        /// <summary>
        /// Owner drawn method
        /// </summary>
        /// <param name="e">event</param>
        /// <param name="x">x coordinate</param>
        /// <param name="ch">column header</param>
        /// <returns>return the new x coordinate</returns>
        public override int DoDraw(DrawListViewSubItemEventArgs e, int x, EXColumnHeader ch)
        {    
            if (this.MyImages != null && this.MyImages.Count > 0) {
                // #Fix2
                int xorg = x;

                for (int i = 0; i < this.MyImages.Count; i++) {
                    Image img = (Image) this.MyImages[i];

                    // #Fix2
                    if ((x + img.Width) > (xorg + ch.Width))
                    {
                        // Clip image
                        int wmax = Math.Max(1, (xorg + ch.Width) - x);
                        int imgy = e.Bounds.Y + ((int)(e.Bounds.Height / 2)) - ((int)(img.Height / 2));
                        e.Graphics.DrawImage(img,
                            new Rectangle(x, imgy, wmax, img.Height),
                            new Rectangle(0, 0, img.Width, img.Height),
                            GraphicsUnit.Pixel);
                        break;
                    }
                    else
                    {
                        int imgy = e.Bounds.Y + ((int)(e.Bounds.Height / 2)) - ((int)(img.Height / 2));
                        e.Graphics.DrawImage(img, x, imgy, img.Width, img.Height);
                    }
                    // #End Fix2
                    /*
                    int imgy = e.Bounds.Y + ((int) (e.Bounds.Height / 2)) - ((int) (img.Height / 2));
                    e.Graphics.DrawImage(img, x, imgy, img.Width, img.Height);
                     */
                    x += img.Width + 2;
                }
            }
            return x;
        }
        
    }

    /// <summary>
    /// Custom sub item for boolean display
    /// </summary>
    public class EXBoolListViewSubItem : EXListViewSubItemAB {
        
        private bool _value;
            
        /// <summary>
        /// Constructor
        /// </summary>
        public EXBoolListViewSubItem() {
            
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="val">sub item value</param>
        public EXBoolListViewSubItem(bool val) {
            _value = val;
	        this.MyValue = val.ToString();
        }
        
        /// <summary>
        /// Get / Set sub item value
        /// </summary>
        public bool BoolValue {
            get {return _value;}
            set {
		        _value = value;
		        this.MyValue = value.ToString();
	        }
        }

        /// <summary>
        /// Owner drawn method
        /// </summary>
        /// <param name="e">event</param>
        /// <param name="x">x coordinate</param>
        /// <param name="ch">column header</param>
        /// <returns>return the new x coordinate</returns>
        public override int DoDraw(DrawListViewSubItemEventArgs e, int x, EXColumnHeader ch)
        {    
            EXBoolColumnHeader boolcol = (EXBoolColumnHeader) ch;
            Image boolimg;
            if (this.BoolValue == true) {
                boolimg = boolcol.TrueImage;
            } else {
                boolimg = boolcol.FalseImage;
            }
            int imgy = e.Bounds.Y + ((int) (e.Bounds.Height / 2)) - ((int) (boolimg.Height / 2));

            e.Graphics.DrawImage(boolimg, x, imgy, boolimg.Width, boolimg.Height);
            x += boolimg.Width + 2;
            return x;
        }
        
    }
    
    /// <summary>
    /// Custom standard listviewitem
    /// </summary>
    public class EXListViewItem : ListViewItem {
	
	    private string _value;
        
        /// <summary>
        /// True if item shall be highlighted
        /// </summary>
        public bool _bHighlighted = false;

        /// <summary>
        /// Thread safe tag attribute to store miscellaneaous values
        /// </summary>
        public Object _safeTag = null;
    
        /// <summary>
        /// Constructor
        /// </summary>
        public EXListViewItem() {
            
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">item label</param>
        public EXListViewItem(string text) {
            this.Text = text;
        }
	
        /// <summary>
        /// Get / Set item value
        /// </summary>
        public string MyValue {
            get {return _value;}
            set {_value = value;}
        }
        
    }
    
    /// <summary>
    /// Custom item with an image
    /// </summary>
    public class EXImageListViewItem : EXListViewItem {
        
        private Image _image;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public EXImageListViewItem() {
            
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">item label</param>
        public EXImageListViewItem(string text) {
            this.Text = text;
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="image">item image</param>
        public EXImageListViewItem(Image image) {
            _image = image;
        }
	
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">item label</param>
        /// <param name="image">item image</param>
        public EXImageListViewItem(string text, Image image) {
            _image = image;
            this.Text = text;
        }
	
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">item label</param>
        /// <param name="image">item image</param>
        /// <param name="value">item value</param>
	    public EXImageListViewItem(string text, Image image, string value) {
            this.Text = text;
            _image = image;
	        this.MyValue = value;
        }

        /// <summary>
        /// Get / Set item image
        /// </summary>
        public Image MyImage {
            get {return _image;}
            set {_image = value;}
        }
        
    }
    
    /// <summary>
    /// Custom item with multiple images
    /// </summary>
    public class EXMultipleImagesListViewItem : EXListViewItem {
        
        private ArrayList _images;
            
        /// <summary>
        /// Constructor
        /// </summary>
        public EXMultipleImagesListViewItem() {
            
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">item label</param>
        public EXMultipleImagesListViewItem(string text) {
            this.Text = text;
        }
            
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="images">item images</param>
        public EXMultipleImagesListViewItem(ArrayList images) {
            _images = images;
        }
	
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">item label</param>
        /// <param name="images">item images</param>
	    public EXMultipleImagesListViewItem(string text, ArrayList images) {
            this.Text = text;
            _images = images;
        }
	
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">item label</param>
        /// <param name="images">item images</param>
        /// <param name="value">item value</param>
	    public EXMultipleImagesListViewItem(string text, ArrayList images, string value) {
            this.Text = text;
            _images = images;
	        this.MyValue = value;
        }
        
        /// <summary>
        /// Get / Set item images
        /// </summary>
        public ArrayList MyImages {
            get {return _images;}
            set {_images = value;}
        }
        
    }    

}