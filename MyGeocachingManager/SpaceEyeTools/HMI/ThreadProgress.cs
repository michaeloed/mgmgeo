using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace SpaceEyeTools.HMI
{
    /// <summary>
    /// Custom progress form, using a progress bar with accurate remaining time,
    /// abort button, or an animated GIF
    /// </summary>
    public partial class ThreadProgress : Form
    {
        private Stopwatch _chrono = null;
        private String _msgFormat = "";
        private int _howlong = 0;
        private System.Timers.Timer _MyTimer = null;
        
        /// <summary>
        /// If true, progress shall be aborted
        /// </summary>
        public bool _bAbort = false;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ThreadProgress()
        {
            InitializeComponent();
            ThreadProgress.CheckForIllegalCrossThreadCalls = false;
        }

        private const int CP_NOCLOSE_BUTTON = 0x200;
        
        /// <summary>
        /// Get creation parameters 
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            _bAbort = true;
            btnAbort.Enabled = false;
        }

        /// <summary>
        /// Perform a step of n values and update % label
        /// </summary>
        /// <param name="n">number of steps to pass</param>
        public void Step(int n)
        {
        	int oldsteps = progressBar1.Step;
        	progressBar1.Step = n;
            Step();
            progressBar1.Step = oldsteps;
        }
        
        /// <summary>
        /// Perform a step and update % label
        /// </summary>
        public void Step()
        {
            progressBar1.PerformStep();
            int percent = progressBar1.Value * 100 / progressBar1.Maximum;
            label1.Text = percent.ToString() + "%";
        }

        /// <summary>
        /// Display automatic progressbar, used when it is not possible
        /// to compute steps
        /// </summary>
        public void ShowAutomaticProgressBar()
        {
            progressBar1.Visible = false;
            label1.Visible = false;
            pictureBox1.Visible = true;
        }

        /// <summary>
        /// Start time estimate
        /// </summary>
        public void StartEstimate()
        {
            _chrono = new Stopwatch();
            _chrono.Start();
        }

        /// <summary>
        /// Compute and display estimate to complete using provided string format in title
        /// </summary>
        /// <param name="msgFormat">string format (only one parameter)</param>
        public void ComputeEstimateTitle(String msgFormat)
        {
            TimeSpan ts = _chrono.Elapsed;
            int percent = progressBar1.Value * 100 / progressBar1.Maximum;
            if (percent != 0)
            {
                int remain = 100 * ((int)ts.TotalSeconds) / percent - (int)ts.TotalSeconds;
                TimeSpan rts = new TimeSpan(0, 0, remain);
                this.Text = String.Format(msgFormat, rts.ToString());
            }
        }

        /// <summary>
        /// Compute and display estimate to complete using provided string format in lblWait
        /// </summary>
        /// <param name="msgFormat">string format (only one parameter)</param>
        public void ComputeEstimatelblWait(String msgFormat)
        {
            TimeSpan ts = _chrono.Elapsed;
            int percent = progressBar1.Value * 100 / progressBar1.Maximum;
            if (percent != 0)
            {
                int remain = 100 * ((int)ts.TotalSeconds) / percent - (int)ts.TotalSeconds;
                TimeSpan rts = new TimeSpan(0, 0, remain);
                lblWait.Text = String.Format(msgFormat, rts.ToString());
            }
        }
        
        /// <summary>
        /// Start time
        /// </summary>
        /// <param name="msgFormat">message format to display remaining time</param>
        /// <param name="howlong">number of iterations before the timer kills itself</param>
        public void StartTimer(String msgFormat, int howlong)
        {
            _msgFormat = msgFormat;
            _howlong = howlong;
            _MyTimer = new System.Timers.Timer();
            _MyTimer.Interval = 1000; // tick every second
            _MyTimer.Elapsed += new System.Timers.ElapsedEventHandler(MyTimer_Tick);
            _MyTimer.Start();
        }

        /// <summary>
        /// Stop the timer
        /// </summary>
        public void StopTimer()
        {
            // Just in case

            try
            {
                if (_MyTimer != null)
                {
                    lblWait.Text = "";
                    _MyTimer.Stop();
                    _MyTimer = null;
                }
            }
            catch (Exception)
            {
                // Bloody thing is not thread safe, sometimes _MyTimer becomes NULL
                // INSIDE the If statement....
            }
        }

        private void MyTimer_Tick(object source, System.Timers.ElapsedEventArgs e)
        {
            _howlong--;
            if (_howlong <= 0)
            {
                StopTimer();
            }
            else
            {
                //Application.DoEvents();
                lblWait.Text = String.Format(_msgFormat, _howlong);
            }
        }
    }
}
