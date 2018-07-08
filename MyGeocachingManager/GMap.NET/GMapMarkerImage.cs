using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using GMap.NET.WindowsForms;
using MyGeocachingManager;
using MyGeocachingManager.Geocaching;

namespace GMap.NET.WindowsForms.Markers
{
    /// <summary>
    /// Custom marker: marker that displays a custom image
    /// </summary>
    public class GMapMarkerImage : GMapMarker
    {
        private Image _image;

        /// <summary>
        /// Get/Set marker image
        /// </summary>
        public Image CurrentImage
        {
            get { return _image; }
            set 
            { 
                _image = value; 
                Size = (_image == null) ? new Size(8, 8) : _image.Size; 
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Image">Marker image</param>
        /// <param name="p">Marker coordinates</param>
        public GMapMarkerImage(Image Image, PointLatLng p)
            : base(p)
        {
            _image = Image;
            Size = (_image == null)? new Size(8,8):_image.Size;
        }

        /// <summary>
        /// Rendering method
        /// </summary>
        /// <param name="g">Graphic for rendering</param>
        public override void OnRender(Graphics g)
        {
            g.DrawImage(_image, new Point(LocalPosition.X - _image.Width / 2, LocalPosition.Y - _image.Height / 2));
        }
    }

    /// <summary>
    /// Custom marker: 
    /// marker that displays a custom image among a list,
    /// depending on map control zoom level
    /// </summary>
    public class GMapMarkerImages : GMapMarker
    {

        /// <summary>
        /// Micro zoom value
        /// </summary>
        public static double VAL_ZOOM_MICRO = 11.0;//9.0;

        /// <summary>
        /// Minimum zoom value
        /// </summary>
        public static double VAL_ZOOM_MINIMUM = 12.0;//11.0;

        /// <summary>
        /// Medium zoom value
        /// </summary>
        public static double VAL_ZOOM_MEDIUM = 13.0;
        
        /*
         if (zoom <= GMapMarkerImages.VAL_ZOOM_MICRO)
            return 0;
        else if (zoom <= GMapMarkerImages.VAL_ZOOM_MINIMUM)
            return 1;
        else if (zoom <= GMapMarkerImages.VAL_ZOOM_MEDIUM)
            return 2;
        else
            return 3; 
        */
        private List<Image> _images;
        private Image _image;
        private int _indexZoomMicro;
        private int _indexZoomMin;
        private int _indexZoomMed;
        private int _indexZoomHigh;
        private Geocache _geo;

        /// <summary>
        /// Change marker image
        /// </summary>
        /// <param name="indexImage">Image index</param>
        public void SetImage(int indexImage)
        {
            _image = _images[indexImage];
            Size = (_image == null)? new Size(8,8):_image.Size;
        }

        /// <summary>
        /// Returns associated Geocache
        /// </summary>
        /// <returns>a Geocache</returns>
        public Geocache GetGeocache()
        {
            return _geo;
        }

        /// <summary>
        /// Automatically change marker image with the one associate to micro zoom level
        /// </summary>
        public void SetZoomImageMicro()
        {
            _image = _images[_indexZoomMicro];
            Size = (_image == null)? new Size(8,8):_image.Size;
        }

        /// <summary>
        /// Automatically change marker image with the one associate to minimum zoom level
        /// </summary>
        public void SetZoomImageMin()
        {
            _image = _images[_indexZoomMin];
            Size = (_image == null)? new Size(8,8):_image.Size;
        }

        /// <summary>
        /// Automatically change marker image with the one associate to medium zoom level
        /// </summary>
        public void SetZoomImageMed()
        {
            _image = _images[_indexZoomMed];
            Size = (_image == null)? new Size(8,8):_image.Size;
        }

        /// <summary>
        /// Automatically change marker image with the one associate to high zoom level
        /// </summary>
        public void SetZoomImageHigh()
        {
            _image = _images[_indexZoomHigh];
            Size = (_image == null)? new Size(8,8):_image.Size;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Images">Image list</param>
        /// <param name="indexZoomMicro">Value for Micro zoom</param>
        /// <param name="indexZoomMin">Value for Minimum zoom</param>
        /// <param name="indexZoomMed">Value for Medium zoom</param>
        /// <param name="indexZoomHigh">Value for High zoom</param>
        /// <param name="p">Marker coordinates</param>
        /// <param name="geo">Associated geocache</param>
        public GMapMarkerImages(List<Image> Images, int indexZoomMicro, int indexZoomMin, int indexZoomMed, int indexZoomHigh, PointLatLng p, Geocache geo)
            : base(p)
        {
            _geo = geo;
            _images = Images;
            _indexZoomMicro = indexZoomMicro; // zoom <= VAL_ZOOM_MICRO
            _indexZoomMin = indexZoomMin; // zoom <= VAL_ZOOM_MINIMUM
            _indexZoomMed = indexZoomMed; // Zoom <= VAL_ZOOM_MEDIUM
            _indexZoomHigh = indexZoomHigh; // zoom > VAL_ZOOM_MEDIUM
            _image = _images[indexZoomMed];
            Size = (_image == null)? new Size(8,8):_image.Size;
        }

        /// <summary>
        /// Rendering method
        /// </summary>
        /// <param name="g">Graphic for rendering</param>
        public override void OnRender(Graphics g)
        {
            if (_image != null)
                g.DrawImage(_image, new Point(LocalPosition.X - _image.Width / 2, LocalPosition.Y - _image.Height / 2));
        }

        /// <summary>
        /// Returns zoom level for GMapMarkerImages based on
        /// map control zoom level
        /// 0 if map control zoom level inferior or equal to GMapMarkerImages.VAL_ZOOM_MICRO
        /// 1 if map control zoom level inferior or equal to GMapMarkerImages.VAL_ZOOM_MINIMUM
        /// 2 if map control zoom level inferior or equal to GMapMarkerImages.VAL_ZOOM_MEDIUM
        /// 3 otherwise
        /// </summary>
        /// <param name="zoom">map control zoom level</param>
        /// <returns>
        /// 0 if map control zoom level inferior or equal to GMapMarkerImages.VAL_ZOOM_MICRO
        /// 1 if map control zoom level inferior or equal to GMapMarkerImages.VAL_ZOOM_MINIMUM
        /// 2 if map control zoom level inferior or equal to GMapMarkerImages.VAL_ZOOM_MEDIUM
        /// 3 otherwise
        /// </returns>
        public static int ReturnImageLevelFromZoom(double zoom)
        {
            if (zoom <= GMapMarkerImages.VAL_ZOOM_MICRO)
                return 0;
            else if (zoom <= GMapMarkerImages.VAL_ZOOM_MINIMUM)
                return 1;
            else if (zoom <= GMapMarkerImages.VAL_ZOOM_MEDIUM)
                return 2;
            else
                return 3;
        }

        /// <summary>
        /// Update GMapMarkerImages according to its zoom level
        /// </summary>
        /// <param name="marker">marker to be changed</param>
        /// <param name="iz">zoom level
        /// 0 if map control zoom level inferior or equal to GMapMarkerImages.VAL_ZOOM_MICRO
        /// 1 if map control zoom level inferior or equal to GMapMarkerImages.VAL_ZOOM_MINIMUM
        /// 2 if map control zoom level inferior or equal to GMapMarkerImages.VAL_ZOOM_MEDIUM
        /// 3 otherwise
        /// </param>
        public static void ChangeImageAccordingToZoom(GMapMarkerImages marker, int iz)
        {
            switch (iz)
            {
                case 0:
                    marker.SetZoomImageMicro();
                    break;
                case 1:
                    marker.SetZoomImageMin();
                    break;
                case 2:
                    marker.SetZoomImageMed();
                    break;
                default:
                    marker.SetZoomImageHigh();
                    break;
            }
        }
    }
}
