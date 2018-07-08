using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using GMap.NET.WindowsForms;
using System.Drawing.Text;
using System.Drawing.Drawing2D;

namespace GMap.NET.WindowsForms.Markers
{
    /// <summary>
    /// Custom overlay:  this one has the capability to be hiden
    /// </summary>
    public class GMapOverlayCustom : GMap.NET.WindowsForms.GMapOverlay
    {
        private bool _bForceHide; // If true, always hiden regardless zoom level

        /// <summary>
        /// Get/Set for ForceHide attribute
        /// If true, this overlay will be automatically hiden
        /// </summary>
        public bool ForceHide
        {
            get { return _bForceHide; }
            set { _bForceHide = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Overlay identifier</param>
        /// <param name="bForceHide">If true, this overlay will be automatically hiden</param>
        public GMapOverlayCustom(String id, bool bForceHide)
            : base(id)
        {
            _bForceHide = bForceHide;
        }
    }
}
