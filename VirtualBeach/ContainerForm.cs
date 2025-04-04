﻿using DotSpatial.Controls;
using DotSpatial.Controls.Docking;
using DotSpatial.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using VBProjectManager;
using VBCommon;
using System.Threading;


namespace VirtualBeach
{
    public partial class ContainerForm : Form
    {
        [Export("Shell", typeof(ContainerControl))]
        private static ContainerControl Shell;

        //frmVBSplashScreen _splshForm = null;
        VBCommon.Controls.frmVBSplashScreen _splshForm = null;

        /// <summary>
        /// Initializes a new instance of the MainForm class.
        /// </summary>
        public ContainerForm()
        {
            InitializeComponent();

            //var splashForm = new frmVBSplashScreen(false, 0);
            //var splashForm = new VBCommon.Controls.frmVBSplashScreen(false, 0);
            //splashForm.ControlBox = false;
            //splashForm.Show(this);

            Thread splashthread = new Thread(new ThreadStart(LoadSplashScreen));
            splashthread.IsBackground = true;
            splashthread.Start();

            //The AppManager is a piece of DotSpatial that we'll use only here and there.
            appManager = new AppManager();

            //splashForm.Close();
            //splashForm = null;

            //Set the main application window to be the "Shell" 
            Shell = this;
            this.statusBar.Dock = DockStyle.Bottom;
            appManager.LoadExtensions();
            this.FormClosed += new FormClosedEventHandler(Dispose);

            CloseSplashScreen();

            VBLogger.GetLogger().AddHandler(new VBLogger.MessageLoggedEventHandler(this.WriteMessage));           
        }

        public AppManager appManager { get; set; }


        public void Dispose(object sender, FormClosedEventArgs e)
        {
            appManager.Dispose();
        }


        public void OnFormClosing()
        {

        }


         ///<summary>
         ///method is UI message displayer from application components
         ///messages can go to any of 3 status strips or progress bar
         ///</summary>
         ///<param name="message"></param>
         ///<param name="target"></param>
        private void WriteMessage(string message, Globals.targetSStrip target)
        {
            switch (target)
            {
                case Globals.targetSStrip.StatusStrip1:
                    toolStripStatusLabel1.Text = message;
                    break;
                case Globals.targetSStrip.StatusStrip2:
                    toolStripStatusLabel2.Text = message;
                    break;
                case Globals.targetSStrip.StatusStrip3:
                    toolStripStatusLabel3.Text = message;
                    break;
                case Globals.targetSStrip.ProgressBar:
                    int value = (int)Convert.ToInt32(message);
                    //int value = (int) Convert.ToSingle(message);
                    toolStripProgressBar1.Value = value;
                    if (value == 100)
                        toolStripProgressBar1.Value = 0;
                    break;
                default:
                    break;
            }
        }


        private void LoadSplashScreen()
        {
            _splshForm = new VBCommon.Controls.frmVBSplashScreen(false, 0);
            _splshForm.ControlBox = false;
            _splshForm.ShowDialog();
        }


        private void CloseSplashScreen()
        {
            if (_splshForm != null)
            {
                _splshForm.Invoke(new MethodInvoker(delegate
                {
                    _splshForm.Close();
                    _splshForm.Dispose();
                }));
            }
        }
    }
}
