﻿using DotSpatial.Controls.Docking;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;


namespace VBDockManager
{
    /// <summary>
    /// The DockManager implementation for HydroDesktop
    /// </summary>
    public class VBDockManager : IDockManager, IPartImportsSatisfiedNotification
    {
        /// <summary>
        /// The main dock container panel
        /// </summary>
        public DockPanel MainDockPanel { get; set; }

        [Import("Shell")]
        public ContainerControl Shell { get; set; }

        /// <summary>
        /// The lookup list of dock panels (for keeping track of existing panels)
        /// </summary>
        private Dictionary<string, DockContent> dockContents = new Dictionary<string, DockContent>();
        private Dictionary<DockContent, int> sortOrderLookup = new Dictionary<DockContent, int>();
        private string ActivePanelKey { get; set; }


        /// <summary>
        /// Create the default docking manager
        /// </summary>
        public VBDockManager()
        {
        }


        /// <summary>
        /// setup the parent form. This 
        /// occurs when the main form becomes available
        /// </summary>
        public void OnImportsSatisfied()
        {
            MainDockPanel = new DockPanel();
            MainDockPanel.Parent = Shell; //using the static variable
            MainDockPanel.Dock = DockStyle.Fill;
            MainDockPanel.BringToFront();
            MainDockPanel.DocumentStyle = WeifenLuo.WinFormsUI.Docking.DocumentStyle.DockingSdi;

            //setup the events
            MainDockPanel.ActiveDocumentChanged += new EventHandler(MainDockPanel_ActiveDocumentChanged);
        }


        public void ResetLayout()
        {
            //check the map
            DockContent mapContent = dockContents["kMap"];
            if (mapContent.IsFloat)
            {
                mapContent.Dock = DockStyle.Fill;
                mapContent.DockState = DockState.Document;
                mapContent.PanelPane = MainDockPanel.ActiveDocumentPane;
                //mapContent.Show(MainDockPanel);
            }

            //first, check the list
            foreach (string key in dockContents.Keys)
            {
                if (key == "kHydroSeriesView")
                {
                    DockContent cnt = dockContents[key];

                    cnt.Dock = DockStyle.Left;
                    cnt.DockState = DockState.DockLeft;
                    cnt.PanelPane = dockContents["kLegend"].Pane;

                    if (cnt.IsHidden)
                    {
                        cnt.Show();
                    }
                }
                else if (key != "kMap" && key != "kLegend")
                {
                    DockContent cnt = dockContents[key];

                    cnt.Dock = DockStyle.Fill;
                    cnt.DockState = DockState.Document;
                    cnt.PanelPane = dockContents["kMap"].Pane;

                    if (cnt.IsHidden)
                    {
                        cnt.Show();
                    }
                }
            }
        }


        /// <summary>
        /// Add a dockable panel
        /// </summary>
        /// <param name="panel">The dockable panel</param>
        public void Add(DockablePanel panel)
        {
            string key = panel.Key;
            string caption = panel.Caption;
            Control innerControl = panel.InnerControl;
            DockStyle dockStyle = panel.Dock;
            short zOrder = panel.DefaultSortOrder;

            Image img = null;
            if (panel.SmallImage != null) img = panel.SmallImage;

            //set dock style of the inner control to Fill
            innerControl.Dock = DockStyle.Fill;

            // make an attempt to start the pane off at the right width.
            if (dockStyle == DockStyle.Right)
                MainDockPanel.DockRightPortion = (double)innerControl.Width / MainDockPanel.Width;

            //setting document tab strip location to 'bottom'
            if (dockStyle == DockStyle.Fill)
            {
                MainDockPanel.DocumentTabStripLocation = DocumentTabStripLocation.Bottom;
                MainDockPanel.DocumentStyle = DocumentStyle.DockingWindow;
                
            }

            //add the inner control of the panel
            DockContent content = new DockContent();
            content.ShowHint = ConvertToDockState(dockStyle);
            content.Controls.Add(innerControl);

            content.Text = caption;
            content.TabText = caption;
            content.Tag = key;
            innerControl.Tag = key;

            content.HideOnClose = true;

            if (img != null)
            {
                content.Icon = ImageToIcon(img);
            }

            content.Show(MainDockPanel);

            //event handler for closing
            content.FormClosing += new FormClosingEventHandler(content_FormClosing);
            content.FormClosed += new FormClosedEventHandler(content_FormClosed);

            //the tag is used by the ActivePanelChanged event
            content.Pane.Tag = key;

            //add panel to contents dictionary
            if (!dockContents.ContainsKey(key))
            {
                dockContents.Add(key, content);
            }
            if (!sortOrderLookup.ContainsKey(content))
            {
                sortOrderLookup.Add(content, zOrder);
            }

            //trigger the panel added event
            OnPanelAdded(key);

            //set the correct sort order
            if (content.Pane.Contents.Count > 1)
            {
                int sortingIndex = ConvertSortOrderToIndex(content, zOrder);
                content.Pane.SetContentIndex(content, sortingIndex);
            }
        }


        void content_FormClosed(object sender, FormClosedEventArgs e)
        {
            DockContent c = sender as DockContent;
            if (c != null)
            {
                OnPanelClosed(c.Tag.ToString());
            }
        }


        void content_FormClosing(object sender, FormClosingEventArgs e)
        {
            DockContent c = sender as DockContent;
            if (c != null)
            {
                OnPanelClosed(c.Tag.ToString());
            }
        }


        private Icon ImageToIcon(Image img)
        {
            Bitmap bm = img as Bitmap;
            if (bm != null)
            {
                return Icon.FromHandle(bm.GetHicon());
            }
            return null;
        }


        public void Remove(string key)
        {
            if (dockContents.ContainsKey(key))
            {
                DockContent content = dockContents[key];
                content.Close();

                //remove event handlers
                content.FormClosing -= content_FormClosing;
                content.FormClosed -= content_FormClosed;

                content.Dispose();
                dockContents.Remove(key);
                OnPanelRemoved(key);
            }
        }


        public void HidePanel(string key)
        {
            if (dockContents.ContainsKey(key))
            {
                dockContents[key].Hide();
            }
        }

        public void ShowPanel(string key)
        {
            if (dockContents.ContainsKey(key))
            {
                dockContents[key].Show();
            }
        }

        public static WeifenLuo.WinFormsUI.Docking.DockState ConvertToDockState(System.Windows.Forms.DockStyle dockStyle)
        {
            switch (dockStyle)
            {
                case System.Windows.Forms.DockStyle.Bottom:
                    return WeifenLuo.WinFormsUI.Docking.DockState.DockBottom;
                case System.Windows.Forms.DockStyle.Fill:
                    return WeifenLuo.WinFormsUI.Docking.DockState.Document;
                case System.Windows.Forms.DockStyle.Left:
                    return WeifenLuo.WinFormsUI.Docking.DockState.DockLeft;
                case System.Windows.Forms.DockStyle.None:
                    return WeifenLuo.WinFormsUI.Docking.DockState.Float;
                case System.Windows.Forms.DockStyle.Right:
                    return WeifenLuo.WinFormsUI.Docking.DockState.DockRight;
                case System.Windows.Forms.DockStyle.Top:
                    return WeifenLuo.WinFormsUI.Docking.DockState.DockTop;

                default:
                    throw new NotImplementedException();
            }
        }
                
        public event EventHandler<DockablePanelEventArgs> ActivePanelChanged;
        public event EventHandler<DockablePanelEventArgs> PanelAdded;
        public event EventHandler<DockablePanelEventArgs> PanelRemoved;
        public event EventHandler<DockablePanelEventArgs> PanelClosed;
        
        public void SelectPanel(string key)
        {
            if (dockContents.ContainsKey(key))
            {
                dockContents[key].Activate();
            }
        }


        /// <summary>
        /// Raises the ActivePanelChanged event
        /// </summary>
        void MainDockPanel_ActiveDocumentChanged(object sender, EventArgs e)
        {
            if (MainDockPanel.ActiveContent == null) return;
            if (MainDockPanel.ActiveContent.DockHandler == null) return;
            if (MainDockPanel.ActiveContent.DockHandler.Content == null) return;

            DockContent activeContent = MainDockPanel.ActiveContent.DockHandler.Content as DockContent;
            if (activeContent == null) return;
            if (activeContent.Tag == null) return;

            string activePanelKey = activeContent.Tag.ToString();
            OnActivePanelChanged(activePanelKey);
        }


        protected void OnPanelClosed(string panelKey)
        {
            if (PanelClosed != null)
            {
                PanelClosed(this, new DockablePanelEventArgs(panelKey));
            }
        }


        protected void OnPanelAdded(string panelKey)
        {
            if (PanelAdded != null)
            {
                PanelAdded(this, new DockablePanelEventArgs(panelKey));
            }
        }


        protected void OnPanelRemoved(string panelKey)
        {
            if (PanelRemoved != null)
            {
                PanelRemoved(this, new DockablePanelEventArgs(panelKey));
            }
        }


        protected void OnActivePanelChanged(string newActivePanelKey)
        {
            if (ActivePanelChanged != null)
            {
                ActivePanelChanged(this, new DockablePanelEventArgs(newActivePanelKey));
            }
        }


        int ConvertSortOrderToIndex(DockContent content, int sortOrder)
        {
            DockPane pane = content.Pane;
            int index = pane.Contents.Count - 1;
            List<int> sortOrderList = new List<int>();

            foreach (DockContent existingContent in pane.Contents)
            {
                sortOrderList.Add(sortOrderLookup[existingContent]);
            }
            sortOrderList.Sort();
            index = sortOrderList.IndexOf(sortOrder);
            return index;
        }
    }
}
