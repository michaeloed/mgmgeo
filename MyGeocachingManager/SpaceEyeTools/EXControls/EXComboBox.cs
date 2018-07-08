using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;

namespace SpaceEyeTools.EXControls 
{

    /// <summary>
    /// Custom ComboBox control
    /// Used in EXListView
    /// </summary>
    public class EXComboBox : ComboBox {
        
        private Brush _highlightbrush; //color of highlighted items
        
        /// <summary>
        /// Constructor
        /// </summary>
        public EXComboBox() {
            _highlightbrush = SystemBrushes.Highlight;
            this.DrawMode = DrawMode.OwnerDrawFixed;
            this.DrawItem += new DrawItemEventHandler(this_DrawItem);
        }
        
        /// <summary>
        /// Get / Set custom Highlight brush
        /// </summary>
        public Brush MyHighlightBrush {
            get {return _highlightbrush;}
            set {_highlightbrush = value;}
        }
        
        private void this_DrawItem(object sender, DrawItemEventArgs e) {
            if (e.Index == -1) return;
            e.DrawBackground();
            if ((e.State & DrawItemState.Selected) != 0) {
                e.Graphics.FillRectangle(_highlightbrush, e.Bounds);
            }
            EXItem item = (EXItem) this.Items[e.Index];
            Rectangle bounds = e.Bounds;
            int x = bounds.X + 2;
            if (item.GetType() == typeof(EXImageItem)) {
                EXImageItem imgitem = (EXImageItem) item;
                if (imgitem.MyImage != null) {
                    Image img = imgitem.MyImage;
                    int y = bounds.Y + ((int) (bounds.Height / 2)) - ((int) (img.Height / 2)) + 1;
                    e.Graphics.DrawImage(img, x, y, img.Width, img.Height);
                    x += img.Width + 2;
                }
            } else if (item.GetType() == typeof(EXMultipleImagesItem)) {
                EXMultipleImagesItem imgitem = (EXMultipleImagesItem) item; 
                if (imgitem.MyImages != null) {
                    for (int i = 0; i < imgitem.MyImages.Count; i++) {
                        Image img = (Image) imgitem.MyImages[i];
                        int y = bounds.Y + ((int) (bounds.Height / 2)) - ((int) (img.Height / 2)) + 1;
                        e.Graphics.DrawImage(img, x, y, img.Width, img.Height);
                        x += img.Width + 2;
                    }
                }
            }
            int fonty = bounds.Y + ((int) (bounds.Height / 2)) - ((int) (e.Font.Height / 2));
            e.Graphics.DrawString(item.Text, e.Font, new SolidBrush(e.ForeColor), x, fonty);
            e.DrawFocusRectangle();
        }
        
        /// <summary>
        /// Custom ComboBox item
        /// </summary>
        public class EXItem {
            
            private string _text = "";
            private string _value = "";
            
            /// <summary>
            /// Constructor
            /// </summary>
            public EXItem() {
                
            }
            
            /// <summary>
            /// Constructor with label
            /// </summary>
            /// <param name="text">label</param>
            public EXItem(string text) {
                _text = text;
            }
            
            /// <summary>
            /// Get / Set label
            /// </summary>
            public string Text {
                get {return _text;}
                set {_text = value;}
            }
            
            /// <summary>
            /// Get / Set value
            /// </summary>
            public string MyValue {
                get {return _value;}
                set {_value = value;}
            }
            
            /// <summary>
            /// ToString override
            /// </summary>
            /// <returns>String version of object</returns>
            public override string ToString() {
                return _text;
            }
            
        }
        
        /// <summary>
        /// Custom item with an image
        /// </summary>
        public class EXImageItem : EXItem {
            
            private Image _image;
                
            /// <summary>
            /// Constructor
            /// </summary>
            public EXImageItem() {
                
            }
            
            /// <summary>
            /// Constructor with label
            /// </summary>
            /// <param name="text">label</param>
            public EXImageItem(string text) {
                this.Text = text;
            }
            
            /// <summary>
            /// Constructor with image
            /// </summary>
            /// <param name="image">image</param>
            public EXImageItem(Image image) {
                _image = image;
            }
            
            /// <summary>
            /// Constructor with label and image
            /// </summary>
            /// <param name="text">label</param>
            /// <param name="image">image</param>
            public EXImageItem(string text, Image image) {
                this.Text = text;
                _image = image;
            }
            
            /// <summary>
            /// Constructor with value and image
            /// </summary>
            /// <param name="image">image</param>
            /// <param name="value">valye</param>
            public EXImageItem(Image image, string value) {
                _image = image;
                this.MyValue = value;
            }
            
            /// <summary>
            /// Constructor with value, label and image
            /// </summary>
            /// <param name="text">label</param>
            /// <param name="image">image</param>
            /// <param name="value">value</param>
            public EXImageItem(string text, Image image, string value) {
                this.Text = text;
                _image = image;
                this.MyValue = value;
            }
            
            /// <summary>
            /// Get / Set image
            /// </summary>
            public Image MyImage {
                get {return _image;}
                set {_image = value;}
            }
            
        }
        
        /// <summary>
        /// Item with multiple images
        /// </summary>
        public class EXMultipleImagesItem : EXItem {
            
            private ArrayList _images;
                
            /// <summary>
            /// Constructor
            /// </summary>
            public EXMultipleImagesItem() {
                
            }
            
            /// <summary>
            /// Constructor with label
            /// </summary>
            /// <param name="text">label</param>
            public EXMultipleImagesItem(string text) {
                this.Text = text;
            }
            
            /// <summary>
            /// Constructor with images
            /// </summary>
            /// <param name="images">images</param>
            public EXMultipleImagesItem(ArrayList images) {
                _images = images;
            }
            
            /// <summary>
            /// Constructor with label and images
            /// </summary>
            /// <param name="text">label</param>
            /// <param name="images">images</param>
            public EXMultipleImagesItem(string text, ArrayList images) {
                this.Text = text;
                _images = images;
            }
            
            /// <summary>
            /// Constructor with value and images
            /// </summary>
            /// <param name="images">images</param>
            /// <param name="value">value</param>
            public EXMultipleImagesItem(ArrayList images, string value) {
                _images = images;
                this.MyValue = value;
            }
            
            /// <summary>
            /// Constructor with value, label and images
            /// </summary>
            /// <param name="text">label</param>
            /// <param name="images">images</param>
            /// <param name="value">value</param>
            public EXMultipleImagesItem(string text, ArrayList images, string value) {
                this.Text = text;
                _images = images;
                this.MyValue = value;
            }

            /// <summary>
            /// Get / Set images
            /// </summary>
            public ArrayList MyImages {
                get {return _images;}
                set {_images = value;}
            }
            
        }
    
    }

}