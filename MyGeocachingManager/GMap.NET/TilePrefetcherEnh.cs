
namespace GMap.NET
{
   using System.Collections.Generic;
   using System.ComponentModel;
   using System.Windows.Forms;
   using GMap.NET.Internals;
   using System;
   using GMap.NET.MapProviders;
   using System.Threading;
   using GMap.NET.WindowsForms;
   using GMap.NET.WindowsForms.Markers;
   using System.Drawing;
    using SpaceEyeTools;

   /// <summary>
   /// form helping to prefetch tiles on local db
    /// specialization of TilePrefetcher from GMap.NET
   /// </summary>
   public partial class TilePrefetcherEnh : Form
   {
      BackgroundWorker worker = new BackgroundWorker();
      List<GPoint> list;
      int zoom;
      GMapProvider provider;
      int sleep;
      int all;

       /// <summary>
       /// Message string format for progress
      /// Default template: "Fetching tile at zoom {0}/{1}: {2} of {3}, complete: {4} %"
       /// </summary>
      public String LblFetchingMessageFormat = "Fetching tile at zoom {0}/{1}: {2} of {3}, complete: {4} %";

       /// <summary>
      /// Default: "Please wait...";
       /// </summary>
      public String LblWaiting = "Please wait...";

       /// <summary>
      /// Default: "all tiles saved";
       /// </summary>
        public String LblAllTileSaved = "all tiles saved";

       /// <summary>
        /// Default: "saving tiles...";
       /// </summary>
        public String LblSavingTiles = "saving tiles...";

       /// <summary>
        /// Default: "tiles to save...";
       /// </summary>
        public String LblTileToSave = "tiles to save...";

       /// <summary>
        /// Default: "Prefetch Complete! => {0} of {1}";
       /// </summary>
        public String LblPrefetchComplete = "Prefetch Complete! => {0} of {1}";

       /// <summary>
        /// Default: "Prefetch Canceled! => {0} of {1}";
       /// </summary>
        public String LblPrefetchCancelled = "Prefetch Canceled! => {0} of {1}";

       /// <summary>
      /// When consecutive TilePrefetcherEnh will be called,
      /// indicate the final zoom level to achieve
      /// Only for label progress text
      /// If -1 will be ignored
       /// </summary>
      public int zoomfinal = -1;

       /// <summary>
       /// If true, a message box will be displayed et the end of the process
       /// </summary>
      public bool ShowCompleteMessage = false;
      RectLatLng area;
      GMap.NET.GSize maxOfTiles;

       /// <summary>
       /// Associated gmap overlay in which to draw downloaded tiles
       /// Can be null
       /// </summary>
      public GMapOverlay Overlay;
      int retry;

       /// <summary>
       /// If true, tiles will be shuffled for download
       /// </summary>
      public bool Shuffle = true;

       /// <summary>
       /// Constructor
       /// </summary>
      public TilePrefetcherEnh()
      {
         InitializeComponent();

         GMaps.Instance.OnTileCacheComplete += new TileCacheComplete(OnTileCacheComplete);
         GMaps.Instance.OnTileCacheStart += new TileCacheStart(OnTileCacheStart);
         GMaps.Instance.OnTileCacheProgress += new TileCacheProgress(OnTileCacheProgress);

         worker.WorkerReportsProgress = true;
         worker.WorkerSupportsCancellation = true;
         worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
         worker.DoWork += new DoWorkEventHandler(worker_DoWork);
         worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
      }

      readonly AutoResetEvent done = new AutoResetEvent(true);

      void OnTileCacheComplete()
      {
         if(!IsDisposed)
         {
            done.Set();

            MethodInvoker m = delegate
            {
                label2.Text = LblAllTileSaved;
            };
            Invoke(m);
         }
      }

      void OnTileCacheStart()
      {
         if(!IsDisposed)
         {
            done.Reset();

            MethodInvoker m = delegate
            {
                label2.Text = LblSavingTiles;
            };
            Invoke(m);
         }
      }

      void OnTileCacheProgress(int left)
      {
         if(!IsDisposed)
         {
            MethodInvoker m = delegate
            {
                label2.Text = left + " " + LblTileToSave;
            };
            Invoke(m);
         }
      }

       /// <summary>
       /// Start prefetch
       /// </summary>
       /// <param name="area">area to download</param>
       /// <param name="zoom">zoom level to download</param>
       /// <param name="provider">gmap provider</param>
       /// <param name="sleep">sleep</param>
       /// <param name="retry">retry</param>
      public void Start(RectLatLng area, int zoom, GMapProvider provider, int sleep, int retry)
      {
         if(!worker.IsBusy)
         {
            this.label1.Text = "...";
            this.label2.Text = LblWaiting;
            this.progressBarDownload.Value = 0;

            this.area = area;
            this.zoom = zoom;
            this.provider = provider;
            this.sleep = sleep;
            this.retry = retry;

            GMaps.Instance.UseMemoryCache = false;
            GMaps.Instance.CacheOnIdleRead = false;
            GMaps.Instance.BoostCacheEngine = true;

            if (Overlay != null)
            {
                Overlay.Markers.Clear();
            }

            worker.RunWorkerAsync();

            this.ShowDialog();
         }
      }

       /// <summary>
       /// Stop prefetch
       /// </summary>
      public void Stop()
      {
         GMaps.Instance.OnTileCacheComplete -= new TileCacheComplete(OnTileCacheComplete);
         GMaps.Instance.OnTileCacheStart -= new TileCacheStart(OnTileCacheStart);
         GMaps.Instance.OnTileCacheProgress -= new TileCacheProgress(OnTileCacheProgress);

         done.Set();

         if(worker.IsBusy)
         {
            worker.CancelAsync();
         }

         GMaps.Instance.CancelTileCaching();

         done.Close();
      }

      void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
      {
         if(ShowCompleteMessage)
         {
            if(!e.Cancelled)
            {
               MessageBox.Show(this, String.Format(LblPrefetchComplete, ((int)e.Result).ToString(), all));
            }
            else
            {
                MessageBox.Show(this, String.Format(LblPrefetchCancelled, ((int)e.Result).ToString(), all));
            }
         }

         if (e.Cancelled)
             this.DialogResult = DialogResult.Abort;
         else
             this.DialogResult = DialogResult.OK;

         list.Clear();

         GMaps.Instance.UseMemoryCache = true;
         GMaps.Instance.CacheOnIdleRead = true;
         GMaps.Instance.BoostCacheEngine = false;

         worker.Dispose();

         this.Close();
      }

      bool CacheTiles(int zoom, GPoint p)
      {
         foreach(var pr in provider.Overlays)
         {
            Exception ex;
            PureImage img;

            // tile number inversion(BottomLeft -> TopLeft)
            if(pr.InvertedAxisY)
            {
               img = GMaps.Instance.GetImageFrom(pr, new GPoint(p.X, maxOfTiles.Height - p.Y), zoom, out ex);
            }
            else // ok
            {
               img = GMaps.Instance.GetImageFrom(pr, p, zoom, out ex);
            }

            if(img != null)
            {
               img.Dispose();
               img = null;
            }
            else
            {
               return false;
            }
         }
         return true;
      }

       /// <summary>
       /// List of cached tiles
       /// </summary>
      public readonly Queue<GPoint> CachedTiles = new Queue<GPoint>();

      void worker_DoWork(object sender, DoWorkEventArgs e)
      {
         if(list != null)
         {
            list.Clear();
            list = null;
         }
         list = provider.Projection.GetAreaTileList(area, zoom, 0);
         maxOfTiles = provider.Projection.GetTileMatrixMaxXY(zoom);
         all = list.Count;

         int countOk = 0;
         int retryCount = 0;

         if(Shuffle)
         {
            MyTools.Shuffle<GPoint>(list);
         }

         lock(this)
         {
            CachedTiles.Clear();
         }

         for(int i = 0; i < all; i++)
         {
            if(worker.CancellationPending)
               break;

            GPoint p = list[i];
            {
               if(CacheTiles(zoom, p))
               {
                   if (Overlay != null)
                   {
                       lock (this)
                       {
                           CachedTiles.Enqueue(p);
                       }
                   }
                  countOk++;
                  retryCount = 0;
               }
               else
               {
                  if(++retryCount <= retry) // retry only one
                  {
                     i--;
                     System.Threading.Thread.Sleep(1111);
                     continue;
                  }
                  else
                  {
                     retryCount = 0;
                  }
               }
            }

            worker.ReportProgress((int)((i + 1) * 100 / all), i + 1);

            if (sleep > 0)
            {
                System.Threading.Thread.Sleep(sleep);
            }
         }

         e.Result = countOk;

         if(!IsDisposed)
         {
            done.WaitOne();
         }
      }

      void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
      {
         this.label1.Text = String.Format(LblFetchingMessageFormat, zoom, (zoomfinal == -1)? zoom: zoomfinal, ((int)e.UserState).ToString(), all,  e.ProgressPercentage.ToString());
         this.progressBarDownload.Value = e.ProgressPercentage;

         if (Overlay != null)
         {
             GPoint? l = null;

             lock (this)
             {
                 if (CachedTiles.Count > 0)
                 {
                     l = CachedTiles.Dequeue();
                 }
             }

             if (l.HasValue)
             {
                 var px = Overlay.Control.MapProvider.Projection.FromTileXYToPixel(l.Value);
                 var p = Overlay.Control.MapProvider.Projection.FromPixelToLatLng(px, zoom);

                 var r1 = Overlay.Control.MapProvider.Projection.GetGroundResolution(zoom, p.Lat);
                 var r2 = Overlay.Control.MapProvider.Projection.GetGroundResolution((int)Overlay.Control.Zoom, p.Lat);
                 var sizeDiff = r2 / r1;

                 GMapMarkerTile m = new GMapMarkerTile(p, (int)(Overlay.Control.MapProvider.Projection.TileSize.Width / sizeDiff));
                 Overlay.Markers.Add(m);
             }
         }
      }

      private void Prefetch_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
      {
         if(e.KeyCode == Keys.Escape)
         {
             this.DialogResult = DialogResult.Abort;
            this.Close();
         }
      }

      private void Prefetch_FormClosed(object sender, FormClosedEventArgs e)
      {
         this.Stop();
      }
   }

   class GMapMarkerTile : GMapMarker
   {
      static  Brush Fill = new SolidBrush(Color.FromArgb(155, Color.Blue));

      public GMapMarkerTile(PointLatLng p, int size) : base(p)
      {
         Size = new System.Drawing.Size(size, size);
      }

      public override void OnRender(Graphics g)
      {
         g.FillRectangle(Fill, new System.Drawing.Rectangle(LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height));
      }
   }
}
