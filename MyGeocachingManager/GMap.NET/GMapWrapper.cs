using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;
using MyGeocachingManager.Geocaching;

namespace MyGeocachingManager
{
    /// <summary>
    /// GMap.NET wrapper with commonly used methods
    /// </summary>
    public class GMapWrapper
    {
        /// <summary>
        /// Create a GMapMarkerImage
        /// This marker can render only one image)
        /// </summary>
        /// <param name="markersOverlay">Associated overlay</param>
        /// <param name="markerImage">Associated image</param>
        /// <param name="lat">Marker latitude</param>
        /// <param name="lon">Marker longitude</param>
        /// <param name="tooltip">Marker tooltip</param>
        /// <returns>Created GMapMarkerImage</returns>
        static public GMapMarkerImage gmapMarkerWithImage(GMapOverlay markersOverlay, Image markerImage, double lat, double lon, String tooltip)
        {
            GMapMarkerImage marker = new GMapMarkerImage(markerImage, new PointLatLng(lat, lon));
            if (tooltip != "")
            {
                marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                marker.ToolTipText = tooltip;
            }
            try
            {
                markersOverlay.Markers.Add(marker);
            }
            catch (Exception)
            {
                // Peut être un problème de provider ?
                marker = null;
            }
            return marker;
        }
        
        /// <summary>
        /// Create a GMapMarkerImage
        /// This marker can render only one image)
        /// </summary>
        /// <param name="markersOverlay">Associated overlay</param>
        /// <param name="markerImage">Associated image</param>
        /// <param name="lat">Marker latitude</param>
        /// <param name="lon">Marker longitude</param>
        /// <param name="tooltipImagePath">Marker image that will be displayed as a tooltip tooltip (full image path)</param>
        /// <param name="label">label to display on top of the image</param>
        /// <returns>Created GMapMarkerImage</returns>
        static public GMapMarkerImage gmapMarkerWithImageAndImageTooltip(GMapOverlay markersOverlay, Image markerImage, double lat, double lon, String tooltipImagePath, String label)
        {
            GMapMarkerImage marker = new GMapMarkerImage(markerImage, new PointLatLng(lat, lon));
            if (tooltipImagePath != "")
            {
            	GMapToolTipPicture tooltip = new GMapToolTipPicture(label, tooltipImagePath, marker);
	            marker.ToolTip = tooltip;
	            marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
	            marker.ToolTipText = "empty";
            }
            try
            {
                markersOverlay.Markers.Add(marker);
            }
            catch (Exception)
            {
                // Peut être un problème de provider ?
                marker = null;
            }
            return marker;
        }
        
        /// <summary>
        /// Create a GMapMarkerImages (marker with several possible images) 
        /// This marker will render a different image for each predefined zoom level:
        /// </summary>
        /// <param name="markersOverlay">Associated overlay</param>
        /// <param name="liste">Image list</param>
        /// <param name="indexZoomMicro">Value for Micro zoom</param>
        /// <param name="indexZoomMin">Value for Minimum zoom</param>
        /// <param name="indexZoomMed">Value for Medium zoom</param>
        /// <param name="indexZoomHigh">Value for High zoom</param>
        /// <param name="lat">Marker latitude</param>
        /// <param name="lon">Marker longitude</param>
        /// <param name="tooltip">Marker tooltip</param>
        /// <returns>Created GMapMarkerImage</returns>
        static public GMapMarkerImages gmapMarkerWithImages(GMapOverlay markersOverlay, List<Image> liste, int indexZoomMicro, int indexZoomMin, int indexZoomMed, int indexZoomHigh, double lat, double lon, String tooltip)
        {
            GMapMarkerImages marker = new GMapMarkerImages(liste, indexZoomMicro, indexZoomMin, indexZoomMed, indexZoomHigh, new PointLatLng(lat, lon), null);
            if (tooltip != "")
            {
                marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                marker.ToolTipText = tooltip;
            }
            try
            {
                markersOverlay.Markers.Add(marker);
            }
            catch (Exception)
            {
                // Peut être un problème de provider ?
                marker = null;
            }
            return marker;
        }

        /// <summary>
        /// Create a GMapMarkerImages (marker with several possible images) 
        /// This marker will render a different image for each predefined zoom level:
        /// </summary>
        /// <param name="markersOverlay">Associated overlay</param>
        /// <param name="liste">Image list</param>
        /// <param name="indexZoomMicro">Value for Micro zoom</param>
        /// <param name="indexZoomMin">Value for Minimum zoom</param>
        /// <param name="indexZoomMed">Value for Medium zoom</param>
        /// <param name="indexZoomHigh">Value for High zoom</param>
        /// <param name="lat">Marker latitude</param>
        /// <param name="lon">Marker longitude</param>
        /// <param name="geo">Associated geocache</param>
        /// <param name="daddy">Reference to main window (MainWindow instance), used for callback purposes</param>
        /// <returns>Created GMapMarkerImages</returns>
        static public GMapMarkerImages gmapMarkerWithImages(GMapOverlay markersOverlay, List<Image> liste, int indexZoomMicro, int indexZoomMin, int indexZoomMed, int indexZoomHigh, double lat, double lon, Geocache geo, MainWindow daddy)
        {
            GMapMarkerImages marker = new GMapMarkerImages(liste, indexZoomMicro, indexZoomMin, indexZoomMed, indexZoomHigh, new PointLatLng(lat, lon), geo);
            GMapToolTipCustom tooltip = new GMapToolTipCustom(daddy, geo,marker);
            marker.ToolTip = tooltip;
            marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
            marker.ToolTipText = "empty";

            try
            {
                markersOverlay.Markers.Add(marker);
            }
            catch (Exception)
            {
                // Peut être un problème de provider ?
                marker = null;
            }
            return marker;
        }

        // ******************************************************************************
        // ******************************************************************************

        /// <summary>
        /// Value of overlay containing circles
        /// </summary>
        public static int CIRCLES = 0;

        /// <summary>
        /// Value of overlay containing difficulty and terrain information
        /// </summary>
        public static int DT = 0;

        /// <summary>
        /// Value of overlay containing bookmarks
        /// </summary>
        public static int BOOKMARKS = 0;

        /// <summary>
        /// Value of overlay containing geocache codes
        /// </summary>
        public static int CODES = 0;

        /// <summary>
        /// Value of overlay containing geocache names
        /// </summary>
        public static int NAMES = 0;

        /// <summary>
        /// Value of overlay containing geocache statistics
        /// </summary>
        public static int STATS = 0;

        /// <summary>
        /// Value of overlay containing areas
        /// </summary>
        public static int AREA = 0;

        /// <summary>
        /// Value of overlay containing itineraries
        /// </summary>
        public static int ITINERARY = 0;

        /// <summary>
        /// Reserved value of overlay
        /// </summary>
        public static int RESERVED1 = 0;

        /// <summary>
        /// Reserved value of overlay
        /// </summary>
        public static int RESERVED2 = 0;

        /// <summary>
        /// Reserved value of overlay
        /// </summary>
        public static int RESERVED3 = 0;

        /// <summary>
        /// Reserved value of overlay
        /// </summary>
        public static int RESERVED4 = 0;

        /// <summary>
        /// Value of overlay containing waypoints
        /// </summary>
        public static int WAYPOINTS = 0;

        /// <summary>
        /// Value of overlay containing markers (geocaches)
        /// </summary>
        public static int MARKERS = 0;

        /// <summary>
        /// Value of overlay containing geocaches from cache
        /// </summary>
        public static int CACHE = 0;

        /// <summary>
        /// Create an overlay and associate it to the map control
        /// </summary>
        /// <param name="map">Associated map control</param>
        /// <param name="label">Overlay name</param>
        /// <param name="bCustom">If true, instanciate a GMapOverlayCustom,
        /// If false, instanciates a GMapOverlay</param>
        /// <param name="bForceHide">If true, forces this overlay to be hiden</param>
        /// <param name="index">Overlay z-index</param>
        static public void CreateOverlay(GMapControl map, String label, bool bCustom, bool bForceHide, ref int index)
        {
            GMapOverlay overlay;
            if (!bCustom)
                overlay = new GMapOverlay(label);
            else
                overlay = new GMapOverlayCustom(label, bForceHide);
            map.Overlays.Add(overlay);
            index = map.Overlays.Count - 1;
        }

        /// <summary>
        /// Create all needed overlays and associate them to the map control:
        /// CACHE (GMapOverlayCustom),
        /// RESERVED3 (GMapOverlayCustom),
        /// CIRCLES (GMapOverlayCustom),
        /// DT (GMapOverlayCustom),
        /// BOOKMARKS,
        /// CODES (GMapOverlayCustom),
        /// NAMES (GMapOverlayCustom),
        /// STATS (GMapOverlayCustom),
        /// AREA,
        /// WAYPOINTS (GMapOverlayCustom),
        /// RESERVED4 (GMapOverlayCustom),
        /// MARKERS,
        /// ITINERARY
        /// RESERVED1 (GMapOverlayCustom),
        /// RESERVED2 (GMapOverlayCustom)
        /// </summary>
        /// <param name="map">map control</param>
        static public void CreateOverlays(GMapControl map)
        {
        	CreateOverlay(map, "CacheCache", true, false, ref GMapWrapper.CACHE);
            CreateOverlay(map, "Reserved3", true, false, ref GMapWrapper.RESERVED3);
            CreateOverlay(map, "Circles", true, true, ref GMapWrapper.CIRCLES);
            CreateOverlay(map, "DT", true, true, ref GMapWrapper.DT);
            CreateOverlay(map, "Bookmarks", false, false, ref GMapWrapper.BOOKMARKS); // NO CUSTOM, visible par défaut
            CreateOverlay(map, "Codes", true, true, ref GMapWrapper.CODES);
            CreateOverlay(map, "Names", true, true, ref GMapWrapper.NAMES);
            CreateOverlay(map, "Stats", true, true, ref GMapWrapper.STATS);
            CreateOverlay(map, "Area", false, false, ref GMapWrapper.AREA); // NO CUSTOM
            CreateOverlay(map, "Waypoints", true, true, ref GMapWrapper.WAYPOINTS);
            CreateOverlay(map, "Reserved4", true, false, ref GMapWrapper.RESERVED4);
            CreateOverlay(map, "Markers", false, false, ref GMapWrapper.MARKERS); // NO CUSTOM
            CreateOverlay(map, "Itinerary", false, false, ref GMapWrapper.ITINERARY); // NO CUSTOM
            CreateOverlay(map, "Reserved1", true, false, ref GMapWrapper.RESERVED1);
            CreateOverlay(map, "Reserved2", true, false, ref GMapWrapper.RESERVED2);
        }

        // ******************************************************************************
        // ******************************************************************************
        /// <summary>
        /// Hide overlays of a map control if needed.
        /// Some overlays will be hiden for low zoom levels
        /// </summary>
        /// <param name="gmap">Map control</param>
        /// <param name="iz">Zoom level, different from map control zoom level:
        /// 0 if map control zoom level inferior or equal to GMapMarkerImages.VAL_ZOOM_MICRO
        /// 1 if map control zoom level inferior or equal to GMapMarkerImages.VAL_ZOOM_MINIMUM
        /// 2 if map control zoom level inferior or equal to GMapMarkerImages.VAL_ZOOM_MEDIUM
        /// 3 otherwise
        /// <param name="zoom">Map zoom</param>
        /// </param>
        public static void HandleOverlaysVisibility(GMapControl gmap, int iz, double zoom)
        {
            if (zoom < (GMapMarkerImages.VAL_ZOOM_MICRO - 3.0)) //iz <= 1) // on désactive pour les zoom très éloignés certains overlay
            {
                gmap.Overlays[GMapWrapper.CIRCLES].IsVisibile = false;
                gmap.Overlays[GMapWrapper.DT].IsVisibile = false;
                gmap.Overlays[GMapWrapper.BOOKMARKS].IsVisibile = false;
                gmap.Overlays[GMapWrapper.CODES].IsVisibile = false;
                gmap.Overlays[GMapWrapper.NAMES].IsVisibile = false;
                gmap.Overlays[GMapWrapper.STATS].IsVisibile = false;
                gmap.Overlays[GMapWrapper.WAYPOINTS].IsVisibile = false;
            }
            else if (iz <= 1) // on désactive pour les zoom très éloignés certains overlay
            {
                gmap.Overlays[GMapWrapper.CIRCLES].IsVisibile = false;
                gmap.Overlays[GMapWrapper.DT].IsVisibile = ((gmap.Overlays[GMapWrapper.DT] as GMapOverlayCustom).ForceHide) ? false : true;
                gmap.Overlays[GMapWrapper.BOOKMARKS].IsVisibile = false;
                gmap.Overlays[GMapWrapper.CODES].IsVisibile = false;
                gmap.Overlays[GMapWrapper.NAMES].IsVisibile = false;
                gmap.Overlays[GMapWrapper.STATS].IsVisibile = ((gmap.Overlays[GMapWrapper.STATS] as GMapOverlayCustom).ForceHide) ? false : true;
                gmap.Overlays[GMapWrapper.WAYPOINTS].IsVisibile = false;
            }
            else
            {
                gmap.Overlays[GMapWrapper.CIRCLES].IsVisibile = ((gmap.Overlays[GMapWrapper.CIRCLES] as GMapOverlayCustom).ForceHide) ? false : true;
                gmap.Overlays[GMapWrapper.DT].IsVisibile = ((gmap.Overlays[GMapWrapper.DT] as GMapOverlayCustom).ForceHide) ? false : true;
                gmap.Overlays[GMapWrapper.BOOKMARKS].IsVisibile = true;
                gmap.Overlays[GMapWrapper.CODES].IsVisibile = ((gmap.Overlays[GMapWrapper.CODES] as GMapOverlayCustom).ForceHide) ? false : true;
                gmap.Overlays[GMapWrapper.NAMES].IsVisibile = ((gmap.Overlays[GMapWrapper.NAMES] as GMapOverlayCustom).ForceHide) ? false : true;
                gmap.Overlays[GMapWrapper.STATS].IsVisibile = ((gmap.Overlays[GMapWrapper.STATS] as GMapOverlayCustom).ForceHide) ? false : true;
                gmap.Overlays[GMapWrapper.RESERVED1].IsVisibile = ((gmap.Overlays[GMapWrapper.RESERVED1] as GMapOverlayCustom).ForceHide) ? false : true;
                gmap.Overlays[GMapWrapper.RESERVED2].IsVisibile = ((gmap.Overlays[GMapWrapper.RESERVED2] as GMapOverlayCustom).ForceHide) ? false : true;
                gmap.Overlays[GMapWrapper.RESERVED3].IsVisibile = ((gmap.Overlays[GMapWrapper.RESERVED3] as GMapOverlayCustom).ForceHide) ? false : true;
                gmap.Overlays[GMapWrapper.RESERVED4].IsVisibile = ((gmap.Overlays[GMapWrapper.RESERVED4] as GMapOverlayCustom).ForceHide) ? false : true;
                gmap.Overlays[GMapWrapper.WAYPOINTS].IsVisibile = ((gmap.Overlays[GMapWrapper.WAYPOINTS] as GMapOverlayCustom).ForceHide) ? false : true;
                gmap.Overlays[GMapWrapper.CACHE].IsVisibile = ((gmap.Overlays[GMapWrapper.CACHE] as GMapOverlayCustom).ForceHide) ? false : true;

            }
        }


        /// <summary>
        /// Hide overlays of a map control if needed.
        /// Some overlays will be hiden for low zoom levels
        /// Zoom level will be automatically retrieved from map control
        /// </summary>
        /// <param name="gmap">Map control</param>
        public static void HandleOverlaysVisibility(GMapControl gmap)
        {
            int iz = GMapMarkerImages.ReturnImageLevelFromZoom(gmap.Zoom);
            GMapWrapper.HandleOverlaysVisibility(gmap, iz, gmap.Zoom);
        }

        /// <summary>
        /// Handler for zoom level change.
        /// This handler will deal with overlays visibility, change MARKERS images according to zoom level.
        /// </summary>
        /// <param name="gmap">Map control</param>
        public static void OnMapZoomChanged(GMapControl gmap)
        {
            gmap.HoldInvalidation = true;
            int iz = GMapMarkerImages.ReturnImageLevelFromZoom(gmap.Zoom);
            GMapOverlay overlay = gmap.Overlays[GMapWrapper.MARKERS];
            foreach (GMapMarker m in overlay.Markers)
            {
                GMapMarkerImages.ChangeImageAccordingToZoom((GMapMarkerImages)m, iz);
            }
            gmap.Refresh();

            // on désactive pour les zoom très éloignés certains overlay 
            GMapWrapper.HandleOverlaysVisibility(gmap, iz, gmap.Zoom);
        }
    }
}
