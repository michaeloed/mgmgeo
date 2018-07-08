using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace MyGeocachingManager.HMI
{
    /// <summary>
    /// Custom panel to draw graphs on it
    /// </summary>
    public class MyPanel : System.Windows.Forms.Panel
    {
        /// <summary>
        /// List of colors
        /// </summary>
        public List<Color> _Colors = null;


        /// <summary>
        /// List of points to draw
        /// </summary>
        public List<List<Double>> _Points = null;

        /// <summary>
        /// List of names for legend
        /// </summary>
        public List<String> _Names = null;

        /// <summary>
        /// List of Minimum value of points
        /// </summary>
        public List<Double> _Min = null;

        /// <summary>
        /// List of Maximum value of points
        /// </summary>
        public List<Double> _Max = null;

        /// <summary>
        /// List of Minimum value of points - string for legend
        /// </summary>
        public List<String> _MinL = null;

        /// <summary>
        /// List of Maximum value of points - string for legend
        /// </summary>
        public List<String> _MaxL = null;

        /// <summary>
        /// Index position of cursor (refers to list of points)
        /// </summary>
        public Int32 _Index = -1;

        /// <summary>
        /// Callback method called during OnMouseMove on this control
        /// </summary>
        public Func<int, bool> _CallbackMethodMouseMove = null;

        int _minx = 0;
        int _maxx = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public MyPanel()
        {
            /*
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            */
            this.SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer, true);
            this.MouseMove += new MouseEventHandler(OnMouseMove);
        }

        /// <summary>
        /// Draw a vertical bar under cursor and call _CallbackMethodMouseMove
        /// with index as parameter
        /// DOES NOT refresh or draw anything on the control !
        /// SIMPLY updates _Index
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public void OnMouseMove(object sender, MouseEventArgs e)
        {

            if ((_Points != null) && (_Points.Count > 0) && (_Points[0].Count != 0))
            {
                int w = _maxx - _minx;
                int x = e.Location.X;
                Double ratioW = (double)(w) / (double)(_Points[0].Count);
                _Index = (int)((double)(x - _minx) / ratioW);
                if (_Index < 0)
                    _Index = 0;
                if (_Index >= _Points[0].Count)
                    _Index = _Points[0].Count - 1;
                //this.Refresh();
                if (_CallbackMethodMouseMove != null)
                    _CallbackMethodMouseMove(_Index);
            }
        }

        /// <summary>
        /// OpPaint override
        /// </summary>
        /// <param name="pe">paint event</param>
        protected override void OnPaint(PaintEventArgs pe)
        {
            // Call the OnPaint method of the base class.
            base.OnPaint(pe);

            // Declare and instantiate a new pen.
            System.Drawing.Pen myPenBlack = new System.Drawing.Pen(Color.Black);
            System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White);
            
            try
            {
                if (_Points != null)
                {
                    // Legende
                    float fy = 1.0f;
                    float margef = 2.0f;
                    _minx = 0;
                    _maxx = this.Width - 1;

                    // Si on n'a plus de 2 courbes, on prend la pleine largeur du controle
                    pe.Graphics.FillRectangle(myBrush, new Rectangle(new Point(0, 0), this.Size));
                    if (_Names.Count > 2)
                    {
                        // Draw an aqua rectangle in the rectangle represented by the control.
                        pe.Graphics.DrawRectangle(myPenBlack, new Rectangle(new Point(0, 0), new Size(this.Width - 1, this.Height - 1)));
                    }

                    // Sinon on trace une belle légende
                    // Puis les axes ensuite
                    System.Drawing.StringFormat drawFormat = new System.Drawing.StringFormat(StringFormatFlags.DirectionVertical);
                    for (int i = 0; i < _Names.Count; i++)
                    {
                        String s = _Names[i];
                        Color c = _Colors[i];
                        
                        // Les min et max
                        // Uniquement si deux courbes !
                        if ((_Names.Count <= 2) && (i == 0))
                        {
                            // Le max
                            SizeF sf = pe.Graphics.MeasureString(_MaxL[i], this.Font);
                            pe.Graphics.DrawString(_MaxL[i], this.Font, new SolidBrush(c), margef, margef);
                            
                            // Le bord de la zone
                            _minx = (int)(margef + sf.Width + margef);

                            // Le min
                            sf = pe.Graphics.MeasureString(_MinL[i], this.Font);
                            pe.Graphics.DrawString(_MinL[i], this.Font, new SolidBrush(c), margef, (float)this.Height - sf.Height - margef);
                            _minx = Math.Max(_minx, (int)(margef + sf.Width + margef));

                            // La légende
                            SizeF sf2 = pe.Graphics.MeasureString(s, this.Font, 100, drawFormat);
                            pe.Graphics.DrawString(s, this.Font, new SolidBrush(c), _minx - margef - sf2.Width - margef, ((float)this.Height - sf2.Height) / 2.0f, drawFormat);

                        }
                        else if ((_Names.Count <= 2) && (i == 1))
                        {
                            // Le max
                            SizeF sf = pe.Graphics.MeasureString(_MaxL[i], this.Font);
                            SizeF sf3 = pe.Graphics.MeasureString(_MinL[i], this.Font);
                            float x = Math.Min((float)this.Width - sf.Width - margef, (float)this.Width - sf3.Width - margef);
                            pe.Graphics.DrawString(_MaxL[i], this.Font, new SolidBrush(c), x, margef);

                            SizeF sf2 = pe.Graphics.MeasureString(s, this.Font, 100, drawFormat);
                            pe.Graphics.DrawString(s, this.Font, new SolidBrush(c), x, ((float)this.Height - sf2.Height) / 2.0f, drawFormat);

                            // Le bord de la zone
                            _maxx = (int)(x - margef);

                            // Le min
                            pe.Graphics.DrawString(_MinL[i], this.Font, new SolidBrush(c), x, (float)this.Height - sf.Height - margef);
                        }
                        else
                        {
                            // La légende
                            pe.Graphics.DrawString(s, this.Font, new SolidBrush(c), margef, fy);
                            SizeF sf = pe.Graphics.MeasureString(s, this.Font);
                            fy += sf.Height + margef;
                        }
                    }

                    if (_Names.Count <= 2)
                    {
                        // Draw an aqua rectangle in the rectangle represented by the control.
                        pe.Graphics.DrawRectangle(myPenBlack, new Rectangle(new Point(_minx, 0), new Size(_maxx - _minx, this.Height - 1)));
                    }


                    int indexGraph = 0;
                    System.Drawing.Pen myPenBlue = new System.Drawing.Pen(Color.Blue, 2.0f);
                    int w = _maxx - _minx;
                    foreach (List<Double> pts in _Points)
                    {
                        System.Drawing.Pen myPenRed = new System.Drawing.Pen(_Colors[indexGraph]);
                        if ((pts != null) && (pts.Count > 2))
                        {
                            Double ratioH = (double)(this.Height) / (_Max[indexGraph] - _Min[indexGraph]);
                            Double ratioW = (double)(w) / (double)(pts.Count);
                            int i = 0;
                            Point? pt1 = null;
                            foreach (Double d in pts)
                            {
                                // Coordinates of point
                                int x = _minx + (int)((double)i * ratioW);
                                int y = this.Height - (int)((double)(d - _Min[indexGraph]) * ratioH);

                                if (pt1 == null)
                                    pt1 = new Point(x, y);
                                else
                                {
                                    Point pt2 = new Point(x, y);
                                    pe.Graphics.DrawLine(myPenRed, (Point)pt1, (Point)pt2);
                                    pt1 = pt2;
                                }

                                // Cross ?
                                if ((indexGraph == (_Points.Count - 1)) && (i == _Index))
                                {
                                    //int w = 3;
                                    //pe.Graphics.DrawLine(myPenBlue, new Point(x - w, y - w), new Point(x + w, y + w));
                                    //pe.Graphics.DrawLine(myPenBlue, new Point(x - w, y + w), new Point(x + w, y - w));
                                    pe.Graphics.DrawLine(myPenBlue, new Point(x, 0), new Point(x, this.Height));
                                }
                                i++;
                            }
                        }
                        indexGraph++;
                    }
                }
            }
            catch (Exception)
            {
                pe.Graphics.FillRectangle(myBrush, new Rectangle(new Point(0, 0), this.Size));
                pe.Graphics.DrawRectangle(myPenBlack, new Rectangle(new Point(0, 0), new Size(this.Width - 1, this.Height - 1)));
            }
        }
    }
}
