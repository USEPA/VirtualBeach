﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using DotSpatial.Controls;
using System.ComponentModel.Composition;
using DotSpatial.Controls.Header;
using DotSpatial.Controls.Docking;
using VBCommon;
using VBCommon.PluginSupport;
using VBCommon.Interfaces;


namespace Prediction
{
    public class PredictionPlugin : Extension, IPartImportsSatisfiedNotification, IPlugin
    {                      
        [Import("Shell")]
        private ContainerControl Shell { get; set; }

        //instance of  class
        private frmPrediction _frmPred;
        private Globals.PluginType pluginType = VBCommon.Globals.PluginType.Prediction;
        
        private VBCommon.Signaller signaller;
        private const string strPanelKey = "Prediction";
        private const string strPanelCaption = "Prediction";
        
        //ribbon buttons
        private SimpleActionItem btnImportIV;
        private SimpleActionItem btnIVDataVal;
        private SimpleActionItem btnMakePred;
        private SimpleActionItem btnImportOB;
        private SimpleActionItem btnImportCombined;        
        private SimpleActionItem btnColumnMapper;
        private SimpleActionItem btnPlot;
        private SimpleActionItem btnClear;
        private SimpleActionItem btnExportCSV;
        private SimpleActionItem btnSetEnddatURL;
        private SimpleActionItem btnImportFromEnddat;
        private SimpleActionItem btnEnddatImportDate;

        private RootItem rootIPyPredictionTab;
        
        //complete and visible flags
        public Boolean boolComplete = false;
        public Boolean boolVisible = true;
        public Boolean boolChanged = false;

        //this plugin was clicked
        private string strTopPlugin = string.Empty;

        private Stack<string> UndoKeys;
        private Stack<string> RedoKeys;
        private string strUndoRedoKey;

        //Raise a message
        public delegate void MessageHandler<TArgs>(object sender, TArgs args) where TArgs : EventArgs;
        public event MessageHandler<VBCommon.PluginSupport.MessageArgs> MessageSent;

        
        //deactivate this plugin
        public override void Deactivate()
        {
            App.HeaderControl.RemoveAll();
            App.DockManager.Remove(strPanelKey);
            _frmPred = null;
            base.Deactivate();
        }


        public void Hide()
        {
            if (boolVisible)
            {
                boolChanged = true;

                App.HeaderControl.RemoveAll();
                App.DockManager.HidePanel(strPanelKey);
            }
            boolVisible = false;
        }


        public void Show()
        {
            if (!boolVisible)
            {
                boolChanged = true;

                AddRibbon("Show");
                App.DockManager.SelectPanel(strPanelKey);
                App.HeaderControl.SelectRoot(strPanelKey);
            }
            boolVisible = true;
        }


        public void MakeActive()
        {
            App.DockManager.SelectPanel(strPanelKey);
            App.HeaderControl.SelectRoot(strPanelKey);
        }


        //initial activation
        public override void Activate()
        {
            _frmPred = new frmPrediction();

            UndoKeys = new Stack<string>();
            RedoKeys = new Stack<string>();
            
            AddPanel();
            AddRibbon("Activate");
            
            //when panel is selected activate seriesview and ribbon tab
            App.DockManager.ActivePanelChanged += new EventHandler<DotSpatial.Controls.Docking.DockablePanelEventArgs>(DockManager_ActivePanelChanged);
            App.HeaderControl.RootItemSelected += new EventHandler<RootItemEventArgs>(HeaderControl_RootItemSelected);
            _frmPred.ButtonStatusEvent += new EventHandler<ButtonStatusEventArgs>(_frmPred_ButtonStatusEvent);
            _frmPred.ControlChangeEvent += new EventHandler(ControlChangeEventHandler);
            _frmPred.RequestModelPluginList += new EventHandler(PassModelPluginList);
            _frmPred.NotifiableChangeEvent += new EventHandler(NotifiableChangeHandler);
            
            boolVisible = true;
            boolComplete = false;

            base.Activate();
            Hide();
        }


        //a root item (plugin) has been selected
        void HeaderControl_RootItemSelected(object sender, RootItemEventArgs e)
        {
            if (e.SelectedRootKey == strPanelKey)
            {
                App.DockManager.SelectPanel(strPanelKey);
            }
        }


        //add a root item
        public void AddRibbon(string sender)
        {
            rootIPyPredictionTab = new RootItem(strPanelKey, strPanelCaption);
            rootIPyPredictionTab.SortOrder = (short)pluginType;
            App.HeaderControl.Add(rootIPyPredictionTab);
            App.HeaderControl.SelectRoot(strPanelKey);

            //add sub-ribbons
            //Import Data
            const string impGroupCaption = "Import Data";            
            btnImportIV = new SimpleActionItem(strPanelKey, "Import IV Data", btnImportIV_Click);
            btnImportIV.LargeImage = Properties.Resources.ImportIV;
            btnImportIV.GroupCaption = impGroupCaption;
            btnImportIV.Enabled = false;
            App.HeaderControl.Add(btnImportIV);

            btnImportOB = new SimpleActionItem(strPanelKey, "Import Observations", btnImportOB_Click);
            btnImportOB.LargeImage = Properties.Resources.ImportOB;
            btnImportOB.GroupCaption = impGroupCaption;
            btnImportOB.Enabled = false;
            App.HeaderControl.Add(btnImportOB);

            btnImportCombined = new SimpleActionItem(strPanelKey, "Import Combined", btnImportCombined_Click);
            btnImportCombined.LargeImage = Properties.Resources.ImportOB;
            btnImportCombined.GroupCaption = impGroupCaption;
            btnImportCombined.Enabled = false;
            App.HeaderControl.Add(btnImportCombined);

            btnSetEnddatURL = new SimpleActionItem(strPanelKey, "Set EnDDaT Data Source", btnSetEnddatURL_Click);
            btnSetEnddatURL.LargeImage = Properties.Resources.URL;
            btnSetEnddatURL.GroupCaption = impGroupCaption;
            btnSetEnddatURL.Enabled = false;
            App.HeaderControl.Add(btnSetEnddatURL);

            btnImportFromEnddat = new SimpleActionItem(strPanelKey, "Import From EnDDaT", btnImportFromEnddat_Click);
            //btnImportFromEnddat.LargeImage = Properties.Resources.ImportIV;
            btnImportFromEnddat.LargeImage = Properties.Resources.globe2;
            btnImportFromEnddat.GroupCaption = impGroupCaption;
            btnImportFromEnddat.Enabled = false;
            App.HeaderControl.Add(btnImportFromEnddat);

            btnEnddatImportDate = new SimpleActionItem(strPanelKey, "Import EnDDaT by Date", btnEnddatImportDate_Click);
            btnEnddatImportDate.LargeImage = Properties.Resources.calendar;
            btnEnddatImportDate.GroupCaption = impGroupCaption;
            btnEnddatImportDate.Enabled = false;
            App.HeaderControl.Add(btnEnddatImportDate);

            btnColumnMapper = new SimpleActionItem(strPanelKey, "View Column Mapping", btnColumnMapper_Click);
            btnColumnMapper.LargeImage = Properties.Resources.mapping;
            btnColumnMapper.GroupCaption = impGroupCaption;
            btnColumnMapper.Enabled = false;
            App.HeaderControl.Add(btnColumnMapper);



            //Predict
            const string predGroupCaption = "Predict";
            btnIVDataVal = new SimpleActionItem(strPanelKey, "Scan IV Data (Optional)", btnIVDataVal_Click);
            btnIVDataVal.LargeImage = Properties.Resources.IVDataVal;
            btnIVDataVal.GroupCaption = predGroupCaption;
            btnIVDataVal.Enabled = false;
            App.HeaderControl.Add(btnIVDataVal);

            btnMakePred = new SimpleActionItem(strPanelKey, "Make Predictons", btnMakePrediction_Click);
            //btnMakePred.LargeImage = Properties.Resources.MakePrediction;
            btnMakePred.LargeImage = Properties.Resources.gears;
            btnMakePred.GroupCaption = predGroupCaption;
            btnMakePred.Enabled = false;
            App.HeaderControl.Add(btnMakePred);




            //Evaluate
            const string evalGroupCaption = "Evaluate";
            btnPlot = new SimpleActionItem(strPanelKey, "Plot", btnPlot_Ck);
            btnPlot.LargeImage = Properties.Resources.Plot;
            btnPlot.GroupCaption = evalGroupCaption;
            btnPlot.Enabled = false;
            App.HeaderControl.Add(btnPlot);

            btnClear = new SimpleActionItem(strPanelKey, "Clear", btnClear_Click);
            btnClear.LargeImage = Properties.Resources.Clear;
            btnClear.GroupCaption = evalGroupCaption;
            btnClear.Enabled = false;
            App.HeaderControl.Add(btnClear);

            btnExportCSV = new SimpleActionItem(strPanelKey, "Export As CSV", btnExportTable_Ck);
            btnExportCSV.LargeImage = Properties.Resources.ExportAsCSV;
            btnExportCSV.GroupCaption = evalGroupCaption;
            btnExportCSV.Enabled = false;
            App.HeaderControl.Add(btnExportCSV);
            
        }


        //add the panel
        public void AddPanel()
        {
            var dp = new DockablePanel(strPanelKey, strPanelCaption, _frmPred, DockStyle.Fill);
            dp.DefaultSortOrder = (short)pluginType;
            App.DockManager.Add(dp);
        }


        //event handler when a plugin is selected from tabs
        void DockManager_ActivePanelChanged(object sender, DotSpatial.Controls.Docking.DockablePanelEventArgs e)
        {
            if (e.ActivePanelKey == strPanelKey)
            {
                App.DockManager.SelectPanel(strPanelKey);
                App.HeaderControl.SelectRoot(strPanelKey);
            }
        }


        //return the plugin type (Prediction)
        public Globals.PluginType PluginType
        {
            get { return pluginType; }
        }


        //return the panel key name
        public string PanelKey
        {
            get { return strPanelKey; }
        }


        //return the complete flag
        public Boolean Complete
        {
            get { return boolComplete; }
        }


        //return the visible flag
        public Boolean Visible
        {
            get { return boolVisible; }
        }


        //This function imports the signaller from the VBProjectManager
        [System.ComponentModel.Composition.Import("Signalling.GetSignaller", AllowDefault = true)]
        public Func<VBCommon.Signaller> GetSignaller
        {
            get;
            set;
        }  


        public void OnImportsSatisfied()
        {
            //If we've successfully imported a Signaller, then connect its events to our handlers.
            signaller = GetSignaller();
            signaller.BroadcastState += new VBCommon.Signaller.BroadcastEventHandler<VBCommon.PluginSupport.BroadcastEventArgs>(BroadcastStateListener);
            signaller.ProjectSaved += new VBCommon.Signaller.SerializationEventHandler<VBCommon.PluginSupport.SerializationEventArgs>(ProjectSavedListener);
            signaller.ProjectOpened += new VBCommon.Signaller.SerializationEventHandler<VBCommon.PluginSupport.SerializationEventArgs>(ProjectOpenedListener);
            signaller.ActivatePrediction += new EventHandler<EventArgs>(ActivatePredictionListener);
            signaller.UndoEvent += new VBCommon.Signaller.SerializationEventHandler<VBCommon.PluginSupport.UndoRedoEventArgs>(Undo);
            signaller.RedoEvent += new VBCommon.Signaller.SerializationEventHandler<VBCommon.PluginSupport.UndoRedoEventArgs>(Redo);
            signaller.UndoStackEvent += new VBCommon.Signaller.SerializationEventHandler<VBCommon.PluginSupport.UndoRedoEventArgs>(PushToStack);
            
            this.MessageSent += new MessageHandler<VBCommon.PluginSupport.MessageArgs>(signaller.HandleMessage);            
        }


        //This function imports the signaller from the VBProjectManager
        [System.ComponentModel.Composition.ImportMany(typeof(VBCommon.Interfaces.IModel))]
        private List<Lazy<IModel, IDictionary<string, object>>> models = null;


        //Expose the list of models via a property so that the control form can pull it out.
        public List<Lazy<IModel, IDictionary<string, object>>> Models
        {
            get { return (models); }
        }


        private void PassModelPluginList(object sender, EventArgs e)
        {
            ((frmPrediction)sender).models = models;
        }


        //event listener for plugin broadcasting changes
        private void BroadcastStateListener(object sender, VBCommon.PluginSupport.BroadcastEventArgs e)
        {
            if (((IPlugin)sender).PluginType == Globals.PluginType.Modeling)
            {
                if ((bool)e.PackedPluginState["Complete"])
                {                    
                    _frmPred.AddModel(e.PackedPluginState["Method"].ToString());
                    Show();
                }
                else
                {
                    int intValidModels = _frmPred.ClearMethod(e.PackedPluginState["Method"].ToString());
                    if (intValidModels == 0)
                        Hide();
                }

                _frmPred.ClearDataGridViews(e.PackedPluginState["Method"].ToString());
                boolComplete = false;
                boolChanged = true;
            }
            if (((IPlugin)sender).PluginType == Globals.PluginType.Datasheet)
            {
                if (!(bool)((IPlugin)sender).Complete)
                {
                    //Hide the prediction plugin if someone's monkeying around on the datasheet
                    Hide();
                    return;
                }
                else
                {
                    //If we got here, then someone has exported from the datasheet
                    //In this case, the user exported changed data from the datasheet, so we need to reset the prediction plugin.
                    if (!(bool)e.PackedPluginState["Clean"]) { Reset(); }

                    //Keep the prediction plugin hidden unless the user backed out of their changes and there are models available for prediction.
                    if (_frmPred.NumberOfModels > 0) { Show(); }
                    else { Hide(); }
                }
            }                
        }


        private void Reset()
        {
            _frmPred.Reset();
        }


        public void Broadcast()
        {
            boolChanged = true;

            IDictionary<string, object> dictPackedState = _frmPred.PackState();
            dictPackedState.Add("Complete", boolComplete);
            dictPackedState.Add("Visible", boolVisible);

            signaller.RaiseBroadcastRequest(this, dictPackedState);
            signaller.TriggerUndoStack();
        }


        private void NotifiableChangeHandler(object sender, EventArgs e)
        {
            Broadcast();
        }


        //event handler for saving project state
        private void ProjectSavedListener(object sender, VBCommon.PluginSupport.SerializationEventArgs e)
        {
            IDictionary<string, object> dictPackedState = _frmPred.PackState();
            dictPackedState.Add("Complete", boolComplete);
            dictPackedState.Add("Visible", boolVisible);

            e.PackedPluginStates.Add(strPanelKey, dictPackedState);
        }


        //event handler for opening project state
        private void ProjectOpenedListener(object sender, VBCommon.PluginSupport.SerializationEventArgs e)
        {
            if (e.PackedPluginStates.ContainsKey(strPanelKey))
            {
                Reset();
                IDictionary<string, object> dictPlugin = e.PackedPluginStates[strPanelKey];

                if ((bool)dictPlugin["Visible"]) { Show(); }
                else { Hide(); }
                boolVisible = (bool)dictPlugin["Visible"];                
                boolComplete = (bool)dictPlugin["Complete"];                

                _frmPred.UnpackState(e.PackedPluginStates[strPanelKey]);
            }
        }


        //event handler for opening project state
        private void ActivatePredictionListener(object sender, EventArgs e)
        {
            Show();
        }


        private void PushToStack(object sender, UndoRedoEventArgs args)
        {
            if (boolChanged)
            {
                IDictionary<string, object> dictPackedState = _frmPred.PackState();
                dictPackedState.Add("Complete", boolComplete);
                dictPackedState.Add("Visible", boolVisible);

                string strKey = PersistentStackUtilities.RandomString(10);
                args.Store.Add(strKey, dictPackedState);
                UndoKeys.Push(strKey);
                RedoKeys.Clear();
                boolChanged = false;
            }
            else
            {
                try
                {
                    string strKey = UndoKeys.Peek();
                    UndoKeys.Push(strKey);
                    RedoKeys.Clear();
                }
                catch { }
            }
        }


        private void Undo(object sender, UndoRedoEventArgs args)
        {
            try
            {
                string strCurrentKey = UndoKeys.Pop();
                string strPastKey = UndoKeys.Peek();
                RedoKeys.Push(strCurrentKey);

                if (strCurrentKey != strPastKey)
                {
                    IDictionary<string, object> dictPlugin = args.Store[strPastKey];
                    if ((bool)dictPlugin["Visible"]) { Show(); }
                    else { Hide(); }

                    boolVisible = (bool)dictPlugin["Visible"];
                    boolComplete = (bool)dictPlugin["Complete"];
                    _frmPred.UnpackState(dictPlugin);
                }
            }
            catch { }
        }


        private void Redo(object sender, UndoRedoEventArgs args)
        {
            try
            {
                string strCurrentKey = UndoKeys.Peek();
                string strNextKey = RedoKeys.Pop();
                UndoKeys.Push(strNextKey);

                if (strCurrentKey != strNextKey)
                {
                    IDictionary<string, object> dictPlugin = args.Store[strNextKey];
                    if ((bool)dictPlugin["Visible"]) { Show(); }
                    else { Hide(); }

                    boolVisible = (bool)dictPlugin["Visible"];
                    boolComplete = (bool)dictPlugin["Complete"];
                    _frmPred.UnpackState(dictPlugin);
                }
            }
            catch { }
        }


        private void SendMessage(string message)
        {
            if (MessageSent != null)
            {
                VBCommon.PluginSupport.MessageArgs e = new VBCommon.PluginSupport.MessageArgs(message);
                MessageSent(this, e);
            }
        }
                

        //import iv data, sends to form click event
        void btnImportIV_Click(object sender, EventArgs e)
        {
            bool validated = _frmPred.btnImportIVs_Click(sender, e);
            if (validated)
            {
                btnIVDataVal.Enabled = true;
                btnMakePred.Enabled = true;
                btnExportCSV.Enabled = true;
                Broadcast();
            }
        }


        //import iv data, sends to form click event
        void btnSetEnddatURL_Click(object sender, EventArgs e)
        {
            bool validated = _frmPred.btnSetEnddatURL_Click(sender, e);
            if (validated)
            {
                btnImportFromEnddat.Enabled = true;
                btnEnddatImportDate.Enabled = true;
                Broadcast();
            }
            else
            {
                btnImportFromEnddat.Enabled = false;
                btnEnddatImportDate.Enabled = false;
            }
        }


        //import iv data, sends to form click event
        void btnImportFromEnddat_Click(object sender, EventArgs e)
        {
            bool validated = _frmPred.btnImportFromEnddat_Click(sender, e);
            if (validated)
            {
                btnIVDataVal.Enabled = true;
                btnMakePred.Enabled = true;
                btnExportCSV.Enabled = true;
                Broadcast();
            }
        }


        //import iv data, sends to form click event
        void btnEnddatImportDate_Click(object sender, EventArgs e)
        {
            bool validated = _frmPred.btnEnddatImportDate_Click(sender, e);
            if (validated)
            {
                btnIVDataVal.Enabled = true;
                btnMakePred.Enabled = true;
                btnExportCSV.Enabled = true;
                Broadcast();
            }
        }
        


        //import ob data, sends to form click event
        void btnImportOB_Click(object sender, EventArgs e)
        {
            bool validated = _frmPred.btnImportObs_Click(sender, e);
            if (validated)
            {
                btnExportCSV.Enabled = true;
                Broadcast();
            }
        }

        //import combined iv and obs data, sends to form click event
        void btnImportCombined_Click(object sender, EventArgs e)
        {
            bool validated = _frmPred.btnImportCombined_Click(sender, e);            
            if (validated)
            {
                btnIVDataVal.Enabled = true;
                btnMakePred.Enabled = true;
                btnExportCSV.Enabled = true;
                Broadcast();
            }
        }

        //launch column mapper
        void btnColumnMapper_Click(object sender, EventArgs e)
        {
            bool validated = true;
            _frmPred.btnDisplayColumnMapping_Click(sender, e);
            if (validated)
            {
                btnIVDataVal.Enabled = true;
                btnMakePred.Enabled = true;
                btnExportCSV.Enabled = true;
                Broadcast();
            }
        }


        void ControlChangeEventHandler(object sender, EventArgs e)
        {
            this.boolChanged = true;
        }

        
        // validate data, sends to form click event and enables make prediction button
        void btnIVDataVal_Click(object sender, EventArgs e)
        {
            bool validated = _frmPred.btnIVDataValidation_Click(sender, e);
            if (validated)
            {
                btnMakePred.Enabled = true;
                Broadcast();
            }
            else
                btnMakePred.Enabled = false;
        }


        //make prediction, sends to form click event and sets complete to true
        void btnMakePrediction_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            bool validate = _frmPred.btnMakePredictions_Click(sender, e);
            if (validate)
            {
                btnPlot.Enabled = true;
                boolComplete = true;
                Broadcast();
            }
            Cursor.Current = Cursors.Default;
        }


        //plot, sends to form click event
        void btnPlot_Ck(object sender, EventArgs e)
        {
            _frmPred.btnPlot_Click(sender, e);
        }


        //clear, sends to form click event
        void btnClear_Click(object sender, EventArgs e)
        {
            DialogResult dgr = MessageBox.Show("This will clear the prediction plugin of all imported data for all models.", "Proceed?", MessageBoxButtons.OKCancel);
            if (dgr == DialogResult.OK)
            {
                _frmPred.btnClear_Click(sender, e);

                btnImportIV.Enabled = false;
                btnImportOB.Enabled = false;
                btnImportCombined.Enabled = false;
                btnIVDataVal.Enabled = false;
                btnMakePred.Enabled = false;
                btnColumnMapper.Enabled = false;

                btnExportCSV.Enabled = false;
                btnClear.Enabled = false;
                btnPlot.Enabled = false;

                btnSetEnddatURL.Enabled = false;
                btnImportFromEnddat.Enabled = false;
                btnEnddatImportDate.Enabled = false;

                Broadcast();
            }
        }


        //export table, sends to form click event
        void btnExportTable_Ck(object sender, EventArgs e)
        {
            _frmPred.btnExportTable_Click(sender, e);
        }


        void _frmPred_ButtonStatusEvent(object sender, ButtonStatusEventArgs args)
        {
            if (args.Set == false)
            {
                args.ButtonStatus.Add("ImportIVsEnabled", btnImportIV.Enabled);
                args.ButtonStatus.Add("ImportObsEnabled", btnImportOB.Enabled);
                args.ButtonStatus.Add("ImportCombinedEnabled", btnImportCombined.Enabled);

                args.ButtonStatus.Add("ViewColumnMapping", btnColumnMapper.Enabled);

                args.ButtonStatus.Add("ValidationButtonEnabled", btnIVDataVal.Enabled);
                args.ButtonStatus.Add("PredictionButtonEnabled", btnMakePred.Enabled);

                args.ButtonStatus.Add("PlotButtonEnabled", btnPlot.Enabled);
                args.ButtonStatus.Add("ClearButtonEnabled", btnClear.Enabled);
                args.ButtonStatus.Add("ExportButtonEnabled", btnExportCSV.Enabled);
                
                args.ButtonStatus.Add("SetEnddatURLButtonEnabled", btnSetEnddatURL.Enabled);
                args.ButtonStatus.Add("EnddatImportButtonEnabled", btnImportFromEnddat.Enabled);
            }
            else
            {
                if (args.ButtonStatus.ContainsKey("ImportIVsEnabled")) { btnImportIV.Enabled = args.ButtonStatus["ImportIVsEnabled"]; }
                if (args.ButtonStatus.ContainsKey("ImportObsEnabled")) { btnImportOB.Enabled = args.ButtonStatus["ImportObsEnabled"]; }
                if (args.ButtonStatus.ContainsKey("ImportCombinedEnabled")) { btnImportCombined.Enabled = args.ButtonStatus["ImportCombinedEnabled"]; }
                if (args.ButtonStatus.ContainsKey("ViewColumnMapping")) { btnColumnMapper.Enabled = args.ButtonStatus["ViewColumnMapping"]; }
                
                if (args.ButtonStatus.ContainsKey("ValidationButtonEnabled")) { btnIVDataVal.Enabled = args.ButtonStatus["ValidationButtonEnabled"]; }
                if (args.ButtonStatus.ContainsKey("PredictionButtonEnabled")) { btnMakePred.Enabled = args.ButtonStatus["PredictionButtonEnabled"]; }

                if (args.ButtonStatus.ContainsKey("PlotButtonEnabled")) { btnPlot.Enabled = args.ButtonStatus["PlotButtonEnabled"]; }
                if (args.ButtonStatus.ContainsKey("ClearButtonEnabled")) { btnClear.Enabled = args.ButtonStatus["ClearButtonEnabled"]; }
                if (args.ButtonStatus.ContainsKey("ExportButtonEnabled")) { btnExportCSV.Enabled = args.ButtonStatus["ExportButtonEnabled"]; }

                if (args.ButtonStatus.ContainsKey("SetEnddatURLButtonEnabled")) { btnSetEnddatURL.Enabled = args.ButtonStatus["SetEnddatURLButtonEnabled"]; }
                if (args.ButtonStatus.ContainsKey("EnddatImportButtonEnabled"))
                {
                    btnImportFromEnddat.Enabled = args.ButtonStatus["EnddatImportButtonEnabled"];
                    btnEnddatImportDate.Enabled = args.ButtonStatus["EnddatImportButtonEnabled"];
                }
            }
        }
    }
}
