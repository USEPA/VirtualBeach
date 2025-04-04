﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using VBCommon;
using VBCommon.IO;
using VBCommon.Statistics;
using VBCommon.Controls;
using VBCommon.Interfaces;
using VBCommon.Transforms;
using Ciloci.Flee;
using VBProjectManager;
using DotSpatial.Controls;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Net;


namespace Prediction
{
    //Prediction class.
    [JsonObject]
    public partial class frmPrediction : UserControl, IFormState
    {
        private ContextMenu cmForResponseVar = new ContextMenu();
        private ContextMenu cmForStats = new ContextMenu();
        private Dictionary<string, string> dictMainEffects;

        private IModel model;
        public List<Lazy<IModel, IDictionary<string, object>>> models;
        public event EventHandler RequestModelPluginList;
        public event EventHandler ControlChangeEvent;

        private string[] strArrReferencedVars;
        private DataTable corrDT; 
        private List<ListItem> lstIndVars;
        private string strOutputVariable;

        InputMapper IvMap;
        InputMapper ObsMap;
        private DataTable dtVariables;
        private DataTable dtObs;
        private DataTable dtStats;

        private int intCheckedRVTransform;

        //The transform of the model's original data:
        private DependentVariableTransforms xfrmImported;
        private double dblImportedPowerTransformExp;

        //The transform of the modeling threshold:
        private DependentVariableTransforms xfrmModeled;
        private double dblModeledPowerTransformExp;

        //The transform for entries in the Observations data table:
        private DependentVariableTransforms xfrmObs;
        private double dblObsPowerTransformExp;

        //The transform applied to the regulatory threshold:
        private DependentVariableTransforms xfrmThreshold;
        private double dblThresholdPowerTransformExp;

        //The transform for entries in the Observations data table:
        private DependentVariableTransforms xfrmDisplay;
        private double dblDisplayPowerTransformExp;
        
        List<string> listModels = new List<string>();
        IDictionary<string, object> dictPredictionElements = new Dictionary<string, object>();
        private int intSelectedModel;
        private string strMethod;
        private string strEnddatURL;
        private string strEnddatTimezone;
        private string strEnddatTimestamp;
        private bool bUseTimestamp;
        
        private string strModelTabClean;
        public event EventHandler ModelTabStateRequested;

        private Boolean boolAllowNotification = true;
        public event EventHandler NotifiableChangeEvent;
        public event EventHandler<ButtonStatusEventArgs> ButtonStatusEvent;


        //constructor
        public frmPrediction()
        {
            InitializeComponent();
            InitializeInterface();
        }


        private void InitializeInterface()
        {
            model = null;
            strArrReferencedVars = null;
            lstIndVars = null;
            strOutputVariable = null;

            IvMap = null;
            ObsMap = null;
            dtVariables = null;
            dtObs = null;
            dtStats = null;

            dblImportedPowerTransformExp = double.NaN;
            dblModeledPowerTransformExp = double.NaN;
            dblObsPowerTransformExp = double.NaN;
            dblThresholdPowerTransformExp = double.NaN;
            dblDisplayPowerTransformExp = double.NaN;

            intSelectedModel = -1;
            //dictPredictionElements = new Dictionary<string, object>();
            dictMainEffects = null;

            strEnddatURL = "";
            strEnddatTimezone = "";
            strEnddatTimestamp = "";
            bUseTimestamp = false;
        }

        public int NumberOfModels { get { return lbAvailableModels.Items.Count; } }

        //returns datatable for correlation data
        [JsonProperty]
        public DataTable CorrDT
        {
            get { return this.corrDT; }
        }


        //selected model's index from listbox
        [JsonProperty]
        public int SelectedModel
        {
            get { return intSelectedModel; }
            set { intSelectedModel = value; }
        }


        //Reconstruct the saved prediction state
        public void UnpackState(IDictionary<string, object> dictPackedState)
        {
            if (dictPackedState.Count == 0) return;

            //Temporarily disable notification of changes that occur during unpacking.
            boolAllowNotification = false;

            if (dictPackedState.ContainsKey("PredictionElements"))
            {
                dictPredictionElements = (IDictionary<string, object>)dictPackedState["PredictionElements"];

                //Convert the plugin's JSON into .NET objects and compile a dictionary of the deserialized objects.
                Dictionary<string, object> dictTemp = new Dictionary<string, object>();
                foreach (var pair in dictPredictionElements)
                {
                    Type objType = typeof(Dictionary<string, object>);
                    string jsonRep = pair.Value.ToString();

                    object objDeserialized = JsonConvert.DeserializeObject(jsonRep, objType);

                    if (((IDictionary<string, object>)objDeserialized).ContainsKey("VariableMapping"))
                    {
                        objType = typeof(Dictionary<string, object>);
                        jsonRep = ((IDictionary<string, object>)objDeserialized)["VariableMapping"].ToString();

                        object objVariableMapping = JsonConvert.DeserializeObject(jsonRep, objType);
                        ((IDictionary<string, object>)objDeserialized)["VariableMapping"] = (IDictionary<string, object>)objVariableMapping;
                    }

                    if (((IDictionary<string, object>)objDeserialized).ContainsKey("ObsVariableMapping"))
                    {
                        objType = typeof(Dictionary<string, object>);
                        jsonRep = ((IDictionary<string, object>)objDeserialized)["ObsVariableMapping"].ToString();

                        object objVariableMapping = JsonConvert.DeserializeObject(jsonRep, objType);
                        ((IDictionary<string, object>)objDeserialized)["ObsVariableMapping"] = (IDictionary<string, object>)objVariableMapping;
                    }

                    if (((IDictionary<string, object>)objDeserialized).ContainsKey("VariableDisplayOrder"))
                    {
                        objType = typeof(Dictionary<string, object>);
                        jsonRep = ((IDictionary<string, object>)objDeserialized)["VariableDisplayOrder"].ToString();

                        object objVariableMapping = JsonConvert.DeserializeObject(jsonRep, objType);
                        ((IDictionary<string, object>)objDeserialized)["VariableDisplayOrder"] = (IDictionary<string, object>)objVariableMapping;
                    }

                    dictTemp.Add(pair.Key, objDeserialized);
                }
                dictPredictionElements = dictTemp;
            }

            if (dictPackedState.ContainsKey("AvailableModels"))
            {
                List<string> listTemp = (List<string>)dictPackedState["AvailableModels"];

                foreach (string method in listTemp)
                    AddModel(method, false);
                
                if (dictPackedState.ContainsKey("AvailableModelsIndex")) { lbAvailableModels.SelectedIndex = (int)dictPackedState["AvailableModelsIndex"]; }                
            }

            if (!dictPackedState.ContainsKey("Model")) { return; }
            Dictionary<string, object> dictModel = (Dictionary<string, object>)dictPackedState["Model"];

            //Unpack the transforms
            xfrmModeled = (DependentVariableTransforms)dictPackedState["xfrmModeled"];
            dblModeledPowerTransformExp = Convert.ToDouble(dictPackedState["ModeledPowerTransformExponent"]);

            xfrmImported = (DependentVariableTransforms)dictPackedState["xfrmImported"];
            dblImportedPowerTransformExp = Convert.ToDouble(dictPackedState["ImportedPowerTransformExponent"]);

            xfrmThreshold = (DependentVariableTransforms)dictPackedState["xfrmThreshold"];
            dblThresholdPowerTransformExp = Convert.ToDouble(dictPackedState["ThresholdPowerTransformExponent"]);

            if (xfrmThreshold == DependentVariableTransforms.none)
                rbNone.Checked = true;
            else if (xfrmThreshold == DependentVariableTransforms.Log10)
                rbLog10.Checked = true;
            else if (xfrmThreshold == DependentVariableTransforms.Ln)
                rbLn.Checked = true;
            else if (xfrmThreshold == DependentVariableTransforms.Power)
                rbPower.Checked = true;

            xfrmObs = (DependentVariableTransforms)dictPackedState["xfrmObs"];
            dblObsPowerTransformExp = Convert.ToDouble(dictPackedState["ObsPowerTransformExponent"]);
            SetObsTransformCheckmarks(Item: (int)xfrmObs);

            xfrmDisplay = (DependentVariableTransforms)dictPackedState["xfrmDisplay"];
            dblDisplayPowerTransformExp = Convert.ToDouble(dictPackedState["DisplayPowerTransformExponent"]);
            SetStatsTransformCheckmarks(Item: (int)xfrmDisplay);

            txtPower.Text = dblThresholdPowerTransformExp.ToString();
            txtRegStd.Text = Convert.ToDouble(dictModel["RegulatoryThreshold"]).ToString();
            txtDecCrit.Text = Convert.ToDouble(dictModel["DecisionThreshold"]).ToString();
            txtProbabilityThreshold.Text = Convert.ToDouble(dictModel["ProbabilityThreshold"]).ToString();

            if (dictModel.ContainsKey("EnddatURL")) { strEnddatURL = dictModel["EnddatURL"].ToString(); }
            else { strEnddatURL = ""; }

            if (dictModel.ContainsKey("EnddatTimezone")) { strEnddatTimezone = dictModel["EnddatTimezone"].ToString(); }
            else { strEnddatTimezone = ""; }

            if (dictModel.ContainsKey("EnddatTimestamp")) { strEnddatTimestamp = dictModel["EnddatTimestamp"].ToString(); }
            else { strEnddatTimestamp = ""; }

            if (dictModel.ContainsKey("UseEnddatTimestamp")) { bUseTimestamp = (bool)dictModel["UseEnddatTimestamp"]; }
            else { bUseTimestamp = false; }

            if ((bool)dictModel["UseRawPredictions"]) { rbRaw.Checked = true; }
            else { rbProbability.Checked = true; }

            if (dictPackedState.ContainsKey("PredictionButtonEnabled") && dictPackedState.ContainsKey("ValidationButtonEnabled"))
            {
                if (ButtonStatusEvent != null)
                {
                    IDictionary<string, bool> dictButtonStates = new Dictionary<string, bool>();
                    dictButtonStates["ImportIVsEnabled"] = (bool)dictPackedState["ImportIVsEnabled"];
                    dictButtonStates["ImportObsEnabled"] = (bool)dictPackedState["ImportObsEnabled"];
                    dictButtonStates["ImportCombinedEnabled"] = (bool)dictPackedState["ImportCombinedEnabled"];
                    dictButtonStates["ViewColumnMapping"] = (bool)dictPackedState["ViewColumnMapping"];
                    
                    

                    dictButtonStates["ValidationButtonEnabled"] = (bool)dictPackedState["ValidationButtonEnabled"];
                    dictButtonStates["PredictionButtonEnabled"] = (bool)dictPackedState["PredictionButtonEnabled"];

                    dictButtonStates["PlotButtonEnabled"] = (bool)dictPackedState["PlotButtonEnabled"];
                    dictButtonStates["ClearButtonEnabled"] = (bool)dictPackedState["ClearButtonEnabled"];
                    dictButtonStates["ExportButtonEnabled"] = (bool)dictPackedState["ExportButtonEnabled"];

                    dictButtonStates["SetEnddatURLButtonEnabled"] = (bool)dictPackedState["SetEnddatURLButtonEnabled"];
                    dictButtonStates["EnddatImportButtonEnabled"] = (bool)dictPackedState["EnddatImportButtonEnabled"];

                    ButtonStatusEventArgs args = new ButtonStatusEventArgs(dictButtonStates, Set: true);
                    ButtonStatusEvent(this, args);
                }
            }

            //Unpack the current DataGridViews
            dtVariables = VBCommon.SerializationUtilities.DeserializeDataTable(Container: dictPackedState, Slot: "IVData", Title: "Variables");
            //if (dtVariables != null)
            {
                dgvVariables.DataSource = dtVariables;
                SetViewOnGrid(dgvVariables);
            }

            dtObs = VBCommon.SerializationUtilities.DeserializeDataTable(Container: dictPackedState, Slot: "ObsData", Title: "Observations");
            //if (dtObs != null)
            {
                dgvObs.DataSource = dtObs;
                SetViewOnGrid(dgvObs);
            }

            dtStats = VBCommon.SerializationUtilities.DeserializeDataTable(Container: dictPackedState, Slot: "StatData", Title: "Stats");
            //if (dtStats != null)
            {
                dgvStats.DataSource = dtStats;
                SetViewOnGrid(dgvStats);
            }

            if (model != null)
            {
                txtModel.Text = strOutputVariable + " = " + model.ModelString();                
            }
            else
            {
                txtModel.Text = "";
            }

            //Now re-enable notification events.
            boolAllowNotification = true;
        }


        //Pack State for Serializing
        public IDictionary<string, object> PackState()
        {
            IDictionary<string, object> dictPluginState = new Dictionary<string, object>();

            if (listModels.Count > 0)
            {
                dictPluginState.Add("AvailableModels", listModels);
                if (intSelectedModel >= 0) { dictPluginState.Add("AvailableModelsIndex", intSelectedModel); }
            }

            if (model == null)
                return dictPluginState;

            //Serialize the model
            double dblRegulatoryThreshold;
            double dblDecisionThreshold;

            //Save the state of the power transform exponent textbox
            double dblTransformExponent = 1;
            try { dblTransformExponent = Convert.ToDouble(txtPower.Text); }
            catch { } //If the textbox can't be converted to a number, then leave the exponent as 1


            dictPluginState.Add("xfrmModeled", xfrmModeled);
            dictPluginState.Add("ModeledPowerTransformExponent", dblModeledPowerTransformExp);

            dictPluginState.Add("xfrmImported", xfrmImported);
            dictPluginState.Add("ImportedPowerTransformExponent", dblImportedPowerTransformExp);

            dictPluginState.Add("xfrmThreshold", xfrmThreshold);
            dictPluginState.Add("ThresholdPowerTransformExponent", dblThresholdPowerTransformExp);

            dictPluginState.Add("xfrmObs", xfrmObs);
            dictPluginState.Add("ObsPowerTransformExponent", dblObsPowerTransformExp);

            dictPluginState.Add("xfrmDisplay", xfrmDisplay);
            dictPluginState.Add("DisplayPowerTransformExponent", dblDisplayPowerTransformExp);

            try { dblRegulatoryThreshold = Convert.ToDouble(txtRegStd.Text); }
            catch (InvalidCastException) { dblRegulatoryThreshold = -1; }

            try { dblDecisionThreshold = Convert.ToDouble(txtDecCrit.Text); }
            catch { dblDecisionThreshold = dblRegulatoryThreshold; }

            //Pack model as string and as model for serializing. need to versions for Json.net serialization (which can't serialize IronPython objects)
            Dictionary<string, object> dictModelState = new Dictionary<string, object>();
            dictModelState.Add("Method", strMethod);
            dictModelState.Add("RegulatoryThreshold", dblRegulatoryThreshold);
            dictModelState.Add("DecisionThreshold", dblDecisionThreshold);
            dictModelState.Add("ProbabilityThreshold", Convert.ToDouble(txtProbabilityThreshold.Text));
            dictModelState.Add("UseRawPredictions", rbRaw.Checked);
            dictModelState.Add("EnddatURL", strEnddatURL);
            dictModelState.Add("EnddatTimezone", strEnddatTimezone);
            dictModelState.Add("EnddatTimestamp", strEnddatTimestamp);
            dictModelState.Add("UseEnddatTimestamp", bUseTimestamp);

            dictPluginState.Add("Model", dictModelState);

            if (ButtonStatusEvent != null)
            {
                IDictionary<string, bool> dictButtonStates = new Dictionary<string, bool>();
                ButtonStatusEventArgs args = new ButtonStatusEventArgs(dictButtonStates);
                ButtonStatusEvent(this, args);

                foreach (KeyValuePair<string, bool> kvp in dictButtonStates)
                    dictPluginState.Add(kvp.Key, kvp.Value);
            }

            //pack values
            dgvVariables.EndEdit();
            dtVariables = (DataTable)dgvVariables.DataSource;
            VBCommon.SerializationUtilities.SerializeDataTable(Data: dtVariables, Container: dictPluginState, Slot: "IVData", Title: "Variables");

            dgvObs.EndEdit();
            dtObs = (DataTable)dgvObs.DataSource;
            VBCommon.SerializationUtilities.SerializeDataTable(Data: dtObs, Container: dictPluginState, Slot: "ObsData", Title: "Observations");

            dgvStats.EndEdit();
            dtStats = (DataTable)dgvStats.DataSource;
            VBCommon.SerializationUtilities.SerializeDataTable(Data: dtStats, Container: dictPluginState, Slot: "StatData", Title: "Stats");

            dictPluginState.Add("PredictionElements", dictPredictionElements);

            return dictPluginState;
        }


        //store packed state and populate listbox
        public void AddModel(string Method, bool ReplacePredictionElements = true)
        {
            //Disconnect the selection-changed handlers for this process
            this.lbAvailableModels.SelectedIndexChanged -= new System.EventHandler(this.lbAvailableModels_SelectedIndexChanged);            
            
            //If there is already a model from this plugin in the listBox, then remove it.
            if (listModels.Contains(Method))
            {
                //Remove the Method from the lists
                listModels.Remove(Method);
                
                if (lbAvailableModels.SelectedItem != null)
                {
                    if (lbAvailableModels.SelectedItem.ToString() == Method && ReplacePredictionElements)
                    {
                        ClearDataGridViews(Method);
                        txtModel.Clear();
                        txtPower.Text = "1";
                        txtRegStd.Text = "235";
                        txtProbabilityThreshold.Text = "50";
                        txtDecCrit.Text = "235";
                        strEnddatURL = "";
                        rbNone.Select();
                        rbRaw.Select();
                        strMethod = null;
                        lbAvailableModels.SelectedIndex = -1;
                    }
                }
                lbAvailableModels.Items.Remove(Method);
            }

            if (ReplacePredictionElements)
            {
                if (dictPredictionElements.ContainsKey(Method))
                    dictPredictionElements.Remove(Method);
            }

            //Now add the model to the listBox
            lbAvailableModels.Items.Add(Method);
            listModels.Add(Method);

            //...And re-connect the selection-changed event handler
            this.lbAvailableModels.SelectedIndexChanged += new System.EventHandler(this.lbAvailableModels_SelectedIndexChanged);
        }


        //when user selects model to use, send it to SetModel()
        private void lbAvailableModels_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            object selectedItem = lbAvailableModels.SelectedItem;
            string strSelectedItem = "";

            if (selectedItem != null) { strSelectedItem = selectedItem.ToString(); }
            intSelectedModel = lbAvailableModels.SelectedIndex;

            //First, pack up the data/observations/predictions for the current plugin
            if (strMethod != null)
            {
                //Make an entry for this model in dictPredictionElements if none exists
                if (!dictPredictionElements.ContainsKey(strMethod))
                    dictPredictionElements.Add(strMethod, new Dictionary<string, object>());

                //Serialize the datatables (Variables, Obs, and Stats):
                dgvVariables.EndEdit();
                dtVariables = (DataTable)dgvVariables.DataSource;
                if (dtVariables != null)
                    VBCommon.SerializationUtilities.SerializeDataTable(Data: dtVariables, Container: (IDictionary<string, object>)dictPredictionElements[strMethod], Slot: "IVData", Title: "Variables");

                dgvObs.EndEdit();
                dtObs = (DataTable)dgvObs.DataSource;
                if (dtObs != null)
                    VBCommon.SerializationUtilities.SerializeDataTable(Data: dtObs, Container: (IDictionary<string, object>)dictPredictionElements[strMethod], Slot: "ObsData", Title: "Observations");

                dgvStats.EndEdit();
                dtStats = (DataTable)dgvStats.DataSource;
                if (dtStats != null)
                    VBCommon.SerializationUtilities.SerializeDataTable(Data: dtStats, Container: (IDictionary<string, object>)dictPredictionElements[strMethod], Slot: "StatData", Title: "Stats");

                //Pack up the data transformations
                PackTransformations(Container: (IDictionary<string, object>)dictPredictionElements[strMethod]);

                //Pack the EnDDaT URL if it exists.
                if (strEnddatURL != "")
                {
                    if (((IDictionary<string, object>)dictPredictionElements[strMethod]).ContainsKey("EnddatURL"))
                        ((IDictionary<string, object>)dictPredictionElements[strMethod]).Remove("EnddatURL");
                    ((IDictionary<string, object>)dictPredictionElements[strMethod]).Add("EnddatURL", strEnddatURL);
                }

                //Pack the EnDDaT Timezone if it exists.
                if (strEnddatTimezone != "")
                {
                    if (((IDictionary<string, object>)dictPredictionElements[strMethod]).ContainsKey("EnddatTimezone"))
                        ((IDictionary<string, object>)dictPredictionElements[strMethod]).Remove("EnddatTimezone");
                    ((IDictionary<string, object>)dictPredictionElements[strMethod]).Add("EnddatTimezone", strEnddatTimezone);
                }

                //Pack the EnDDaT Timestamp if it exists.
                if (strEnddatTimestamp != "")
                {
                    if (((IDictionary<string, object>)dictPredictionElements[strMethod]).ContainsKey("EnddatTimestamp"))
                        ((IDictionary<string, object>)dictPredictionElements[strMethod]).Remove("EnddatTimestamp");
                    ((IDictionary<string, object>)dictPredictionElements[strMethod]).Add("EnddatTimestamp", strEnddatTimestamp);
                }

                if (((IDictionary<string, object>)dictPredictionElements[strMethod]).ContainsKey("UseEnddatTimestamp"))
                    ((IDictionary<string, object>)dictPredictionElements[strMethod]).Remove("UseEnddatTimestamp");
                ((IDictionary<string, object>)dictPredictionElements[strMethod]).Add("UseEnddatTimestamp", bUseTimestamp);

                if (ButtonStatusEvent != null)
                {
                    IDictionary<string, bool> dictButtonStates = new Dictionary<string, bool>();
                    ButtonStatusEventArgs args = new ButtonStatusEventArgs(dictButtonStates);
                    ButtonStatusEvent(this, args);

                    foreach (KeyValuePair<string, bool> kvp in dictButtonStates)
                        ((IDictionary<string, object>)dictPredictionElements[strMethod])[kvp.Key] = kvp.Value;
                }

            //didn't select a model
            if (selectedItem == null)
            {
                txtModel.Clear();
                txtPower.Text = "1";
                txtRegStd.Text = "235";
                txtProbabilityThreshold.Text = "50";
                txtDecCrit.Text = "235";
                strEnddatURL = "";
                strEnddatTimezone ="";
                strEnddatTimestamp = "";
                bUseTimestamp = false;

                rbNone.Select();
                rbRaw.Select();
                strMethod = null;

                //Deactivate all the ribbon buttons
                if (ButtonStatusEvent != null)
                {
                    string[] strButtonStateKeys = {"ImportIVsEnabled", "ImportObsEnabled", "ValidationButtonEnabled",
                                                                  "PredictionButtonEnabled", "PlotButtonEnabled", "ClearButtonEnabled",
                                                                  "ExportButtonEnabled", "SetEnddatURLButtonEnabled",
                                                                  "EnddatImportButtonEnabled", "ImportCombinedEnabled",
                                                                  "ViewColumnMapping"};

                    IDictionary<string, bool> dictButtonStates = new Dictionary<string, bool>();
                    foreach (string key in strButtonStateKeys)
                        dictButtonStates[key] = false;

                    ButtonStatusEventArgs args = new ButtonStatusEventArgs(dictButtonStates, Set: true);
                    ButtonStatusEvent(this, args);
                }

                return;
            }

            
            }

            //Clear the column mappings
            IvMap = null;
            ObsMap = null;

            //clear the grids if another model's info has been used
            if (corrDT != null)
            {
                this.dgvStats.DataSource = null;
                this.dgvObs.DataSource = null;
                this.dgvVariables.DataSource = null;
            }

            SetModel(strSelectedItem);

            NotifyContainer();
        }


        private void PackTransformations(IDictionary<string, object> Container)
        {
            //Save the regulatory threshold as entered in the textbox
            if (Container.ContainsKey("RegulatoryThreshold"))
                Container.Remove("RegulatoryThreshold");
            Container.Add("RegulatoryThreshold", Convert.ToDouble(txtRegStd.Text));

            //Save the decision threshold as entered in the textbox
            if (txtDecCrit.Text != "")
            {
                if (Container.ContainsKey("DecisionThreshold"))
                    Container.Remove("DecisionThreshold");
                Container.Add("DecisionThreshold", Convert.ToDouble(txtDecCrit.Text));
            }
            else
            {
                if (Container.ContainsKey("DecisionThreshold"))
                    Container.Remove("DecisionThreshold");
                Container.Add("DecisionThreshold", Convert.ToDouble(txtRegStd.Text));
            }

            //Transform for dtObs
            if (Container.ContainsKey("xfrmObs"))
                Container.Remove("xfrmObs");
            Container.Add("xfrmObs", xfrmObs);

            //Exponent for dtObs Power transformation
            if (Container.ContainsKey("xfrmObsExponent"))
                Container.Remove("xfrmObsExponent");
            Container.Add("xfrmObsExponent", dblObsPowerTransformExp);

            //Transform for regulatory criterion
            if (Container.ContainsKey("xfrmThreshold"))
                Container.Remove("xfrmThreshold");
            Container.Add("xfrmThreshold", xfrmThreshold);

            //Transform for transform in prediction spreadsheet
            if (Container.ContainsKey("xfrmDisplay"))
                Container.Remove("xfrmDisplay");
            Container.Add("xfrmDisplay", xfrmDisplay);

            //Exponent for regulatory criterion Power transformation
            if (Container.ContainsKey("xfrmDisplayExponent"))
                Container.Remove("xfrmDisplayExponent");
            Container.Add("xfrmDisplayExponent", dblDisplayPowerTransformExp);

            //Exponent for regulatory criterion Power transformation
            if (Container.ContainsKey("xfrmThresholdExponent"))
                Container.Remove("xfrmThresholdExponent");
            Container.Add("xfrmThresholdExponent", dblThresholdPowerTransformExp);

            //Store the contents of the Probability Threshold textbox.
            if (Container.ContainsKey("ProbabilityThreshold"))
                Container.Remove("ProbabilityThreshold");
            Container.Add("ProbabilityThreshold", Convert.ToDouble(txtProbabilityThreshold.Text));

            //Status of the rbRaw radiobutton.
            if (Container.ContainsKey("RawThreshold"))
                Container.Remove("RawThreshold");
            Container.Add("RawThreshold", rbRaw.Checked);
        }


        public void SetModel(string strModelPlugin)
        {
            IDictionary<string, object> dictPackedState = null;
            strMethod = strModelPlugin;

            //Load the interface that links us to the selected modeling plugin:
            foreach (Lazy<IModel, IDictionary<string, object>> module in models)
            {
                if (module.Metadata["PluginKey"].ToString() == strModelPlugin)
                {                    
                    model = module.Value;
                    dictPackedState = model.GetPackedState();
                }
            }

            if (dictPackedState != null)
            {
                //make sure empty model doesnt run through this method
                if (dictPackedState.Count <= 2)
                    return;

                Dictionary<string, object> dictModel = (Dictionary<string, object>)dictPackedState["Model"];
                //dictTransform = (Dictionary<string, object>)dictPackedState["Transform"];

                if (dictModel != null)
                {
                    Dictionary<string, object> dictPackedDatasheet = (Dictionary<string, object>)dictPackedState["PackedDatasheetState"];
                    strOutputVariable = dictPackedDatasheet["DepVarColNameAsImported"].ToString();

                    //datatables serialized as xml string to maintain extendedProperty values
                    string strXmlDataTable = (string)dictPackedDatasheet["XmlDataTable"];
                    StringReader sr = new StringReader(strXmlDataTable);
                    DataSet ds = new DataSet();
                    ds.ReadXml(sr);
                    sr.Close();
                    corrDT = ds.Tables[0];

                    //unpack independent variables and text boxes
                    lstIndVars = (List<ListItem>)dictPackedState["Predictors"];

                    txtModel.Text = strOutputVariable + " = " +  model.ModelString();

                    List<string> list = new List<string>();
                    list.Add(corrDT.Columns[0].ColumnName);
                    list.Add(corrDT.Columns[1].ColumnName);

                    int intNumVars = lstIndVars.Count;
                    for (int i = 0; i < intNumVars; i++)
                        list.Add(lstIndVars[i].ToString());

                    //Lets get all the main effect variables
                    dictMainEffects = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
                    for (int i = 1; i < corrDT.Columns.Count; i++)
                    {
                        bool bIsResponseVariable = (corrDT.Columns[i].ColumnName == strOutputVariable);
                        bool bMainEffect = Support.IsMainEffect(corrDT.Columns[i].ColumnName, corrDT);
                        if (bMainEffect && !bIsResponseVariable)
                        {
                            string strColName = corrDT.Columns[i].ColumnName;
                            dictMainEffects.Add(strColName, strColName);
                        }
                    }

                    //determine which transform type box to check
                    xfrmModeled = (DependentVariableTransforms)dictPackedState["xfrmThreshold"];
                    dblModeledPowerTransformExp = Convert.ToDouble(dictPackedState["ThresholdPowerTransformExponent"]);

                    xfrmImported = (DependentVariableTransforms)dictPackedDatasheet["DepVarTransform"];
                    dblImportedPowerTransformExp = Convert.ToDouble(dictPackedDatasheet["DepVarExponent"]);
                                        
                    //format txtModel textbox
                    string strModelExpression = model.ModelString();
                    txtModel.Text = strOutputVariable + " = " +  strModelExpression;

                    //Lets get only the main effects in the model
                    string[] strArrRefvars = ExpressionEvaluator.GetReferencedVariables(strModelExpression, dictMainEffects.Keys.ToArray());
                    List<string> lstRefVar = new List<string>();
                    lstRefVar.Add("ID");
                    lstRefVar.AddRange(strArrRefvars);
                    strArrReferencedVars = lstRefVar.ToArray();

                    //We may need to restore some previously used elements
                    if (!dictPredictionElements.ContainsKey(strModelPlugin))
                    {
                        dictPredictionElements.Add(strModelPlugin, new Dictionary<string, object>());

                        //Use the thresholds from the packed model object
                        txtDecCrit.Text = ((double)dictModel["DecisionThreshold"]).ToString();
                        txtRegStd.Text = ((double)dictModel["RegulatoryThreshold"]).ToString();
                        txtProbabilityThreshold.Text = "50";
                        rbRaw.Checked = true;
                        strEnddatURL = "";
                        strEnddatTimezone = "";
                        strEnddatTimestamp = "";
                        bUseTimestamp = false;

                        //Grab default transforms from the model.
                        xfrmThreshold = xfrmModeled;
                        dblThresholdPowerTransformExp = dblModeledPowerTransformExp;

                        dgvVariables.DataSource = InitializeIVData();
                        SetViewOnGrid(dgvVariables);

                        dgvObs.DataSource = InitializeObsData();
                        SetViewOnGrid(dgvObs);

                        xfrmObs = DependentVariableTransforms.none;
                        dblObsPowerTransformExp = 1;
                        SetObsTransformCheckmarks(Item: (int)xfrmObs);


                        //We prefer to match the displayed data to the imported transform
                        xfrmDisplay = xfrmImported;
                        dblDisplayPowerTransformExp = dblImportedPowerTransformExp;
                        SetStatsTransformCheckmarks(Item: (int)xfrmDisplay);

                        //Use the transforms from the packed model object
                        txtPower.Text = dblThresholdPowerTransformExp.ToString();

                        //Initially, the validation and prediction buttons should be disabled.
                        if (ButtonStatusEvent != null)
                        {
                            IDictionary<string, bool> dictButtonStates = new Dictionary<string, bool>();
                            dictButtonStates["ImportIVsEnabled"] = true;
                            dictButtonStates["ImportObsEnabled"] = true;
                            dictButtonStates["ImportCombinedEnabled"] = true;
                            dictButtonStates["ViewColumnMapping"] = true;
                            
                            dictButtonStates["ValidationButtonEnabled"] = false;
                            dictButtonStates["PredictionButtonEnabled"] = false;

                            dictButtonStates["PlotButtonEnabled"] = false;
                            dictButtonStates["ClearButtonEnabled"] = true;
                            dictButtonStates["ExportButtonEnabled"] = false;
                            
                            dictButtonStates["SetEnddatURLButtonEnabled"] = true;
                            dictButtonStates["EnddatImportButtonEnabled"] = false;

                            ButtonStatusEventArgs args = new ButtonStatusEventArgs(dictButtonStates, Set: true);
                            ButtonStatusEvent(this, args);
                        }
                    }
                    else
                    {
                        IDictionary<string, object> dictNewModel = (IDictionary<string, object>)dictPredictionElements[strModelPlugin];
                        
                        if (dictNewModel.ContainsKey("EnddatURL"))
                        {
                            strEnddatURL = dictNewModel["EnddatURL"].ToString();
                        }
                        else { strEnddatURL = ""; }

                        if (dictNewModel.ContainsKey("EnddatTimezone"))
                        {
                            strEnddatTimezone = dictNewModel["EnddatTimezone"].ToString();
                        }
                        else { strEnddatTimezone = ""; }

                        if (dictNewModel.ContainsKey("EnddatTimestamp"))
                        {
                            strEnddatTimestamp = dictNewModel["EnddatTimestamp"].ToString();
                        }
                        else { strEnddatTimestamp = ""; }

                        if (dictNewModel.ContainsKey("UseEnddatTimestamp"))
                        {
                            bUseTimestamp = (bool)dictNewModel["UseEnddatTimestamp"];
                        }
                        else { bUseTimestamp = false; }

                        if (dictNewModel.ContainsKey("ObsVariableMapping"))
                        {
                            ObsMap = new InputMapper((IDictionary<string, object>)(dictNewModel["ObsVariableMapping"]));
                        }

                        if (dictNewModel.ContainsKey("VariableMapping"))
                        {
                            IvMap = new InputMapper((IDictionary<string, object>)(dictNewModel["VariableMapping"]));
                        }

                        if (dictNewModel.ContainsKey("PredictionButtonEnabled") && dictNewModel.ContainsKey("ValidationButtonEnabled"))
                        {
                            if (ButtonStatusEvent != null)
                            {
                                string[] strButtonStateKeys = {"ImportIVsEnabled", "ImportObsEnabled", "ValidationButtonEnabled",
                                                                  "PredictionButtonEnabled", "PlotButtonEnabled", "ClearButtonEnabled",
                                                                  "ExportButtonEnabled", "SetEnddatURLButtonEnabled",
                                                                  "EnddatImportButtonEnabled", "ImportCombinedEnabled",
                                                                  "ViewColumnMapping"};
                                IDictionary<string, bool> dictButtonStates = new Dictionary<string, bool>();
                                foreach (string key in strButtonStateKeys)
                                    dictButtonStates[key] = (bool)((IDictionary<string, object>)dictPredictionElements[strMethod])[key];

                                ButtonStatusEventArgs args = new ButtonStatusEventArgs(dictButtonStates, Set: true);
                                ButtonStatusEvent(this, args);
                            }
                        }

                        if (dictNewModel.ContainsKey("xfrmThreshold"))
                        {
                            //If these were saved in the prediction elements, great! Otherwise, grab defaults from the model.
                            xfrmThreshold = (DependentVariableTransforms)(Convert.ToInt32(dictNewModel["xfrmThreshold"]));
                            dblThresholdPowerTransformExp = Convert.ToDouble(dictNewModel["xfrmThresholdExponent"]);
                            txtRegStd.Text = ((double)dictNewModel["RegulatoryThreshold"]).ToString();
                            txtDecCrit.Text = ((double)dictNewModel["DecisionThreshold"]).ToString();
                            txtProbabilityThreshold.Text = ((double)dictNewModel["ProbabilityThreshold"]).ToString();
                            rbRaw.Checked = (bool)dictNewModel["RawThreshold"];
                        }
                        else
                        {
                            //Here we are grabbing defaults from the model.
                            xfrmThreshold = xfrmModeled;
                            dblThresholdPowerTransformExp = dblModeledPowerTransformExp;
                            txtDecCrit.Text = ((double)dictModel["DecisionThreshold"]).ToString();
                            txtRegStd.Text = ((double)dictModel["RegulatoryThreshold"]).ToString();
                            txtProbabilityThreshold.Text = "50";
                            rbRaw.Checked = true;
                        }

                        dtVariables = VBCommon.SerializationUtilities.DeserializeDataTable(Container: dictNewModel, Slot: "IVData", Title: "Variables");
                        if (dtVariables != null)
                        {
                            dgvVariables.DataSource = dtVariables;
                            SetViewOnGrid(dgvVariables);
                        }
                        else
                        {
                            dgvVariables.DataSource = InitializeIVData();
                            SetViewOnGrid(dgvVariables);
                            dgvVariables.AllowUserToAddRows = true;
                        }                        

                        //First, establish the default (will be overwritten if a version is found within the prediction elements)
                        xfrmObs = DependentVariableTransforms.none;
                        dblObsPowerTransformExp = 1;

                        dtObs = VBCommon.SerializationUtilities.DeserializeDataTable(Container: dictNewModel, Slot: "ObsData", Title: "Observations");
                        if (dtObs != null)
                        {
                            dgvObs.DataSource = dtObs;
                            SetViewOnGrid(dgvObs);

                            if (dictNewModel.ContainsKey("xfrmObs"))
                            {
                                //Use the thresholds and transforms from the prediction elements
                                xfrmObs = (DependentVariableTransforms)(Convert.ToInt32(dictNewModel["xfrmObs"]));
                                dblObsPowerTransformExp = Convert.ToDouble(dictNewModel["xfrmObsExponent"]);
                            }
                            else
                            {
                                //Here we are using the default (no transformation).
                                xfrmObs = DependentVariableTransforms.none;
                                dblObsPowerTransformExp = 1;
                            }
                        }
                        else
                        {
                            dgvObs.DataSource = InitializeObsData();
                            SetViewOnGrid(dgvObs);
                        }
                        SetObsTransformCheckmarks(Item: (int)xfrmObs);

                        dtStats = VBCommon.SerializationUtilities.DeserializeDataTable(Container: dictNewModel, Slot: "StatData", Title: "Stats");
                        if (dtStats != null)
                        {
                            dgvStats.DataSource = dtStats;
                            SetViewOnGrid(dgvStats);

                            if (dictNewModel.ContainsKey("xfrmDisplay"))
                            {
                                //Use the thresholds and transforms from the prediction elements
                                xfrmDisplay = (DependentVariableTransforms)(Convert.ToInt32(dictNewModel["xfrmDisplay"]));
                                dblDisplayPowerTransformExp = Convert.ToDouble(dictNewModel["xfrmDisplayExponent"]);
                            }
                            else
                            {
                                //Here we are using the default (match the imported transform).
                                xfrmDisplay = xfrmImported;
                                dblDisplayPowerTransformExp = dblImportedPowerTransformExp;
                            }
                        }
                        else
                        {
                            //Here we are using the default (match the imported transform).
                            xfrmDisplay = xfrmImported;
                            dblDisplayPowerTransformExp = dblImportedPowerTransformExp;
                        }
                        SetStatsTransformCheckmarks(Item: (int)xfrmDisplay);
                    }

                    //determine which transform type box to check
                    txtPower.Text = dblThresholdPowerTransformExp.ToString();
                    if (xfrmThreshold == DependentVariableTransforms.none)
                        rbNone.Checked = true;
                    else if (xfrmThreshold == DependentVariableTransforms.Log10)
                        rbLog10.Checked = true;
                    else if (xfrmThreshold == DependentVariableTransforms.Ln)
                        rbLn.Checked = true;
                    else if (xfrmThreshold == DependentVariableTransforms.Power)
                    {
                        rbPower.Checked = true;                        
                    }
                    else
                        rbNone.Checked = true;
                }
            }
        }


        private DataTable InitializeIVData()
        {
            if (strArrReferencedVars != null)
            {
                //Create the datatable and initialize it with the ID column
                DataTable dt = new DataTable();
                dt.Columns.Add("ID", typeof(string));

                //Add a column to the datatable for each predictor
                foreach (string meKey in strArrReferencedVars)
                {
                    if (String.Compare(meKey, "ID", true) != 0)
                        dt.Columns.Add(meKey, typeof(double));
                }

                return dt;
            }
            else
                return null;
        }


        private DataTable InitializeObsData()
        {
            if (strOutputVariable != null)
            {
                //Create the datatable and initialize it with the ID column
                DataTable dt = new DataTable();
                dt.Columns.Add("ID", typeof(string));
                dt.Columns.Add(strOutputVariable, typeof(double));
                return dt;
            }
            else
                return null;
        }


        private void dgvVariables_RowsAdded(object sender, EventArgs e)
        {
            if (dgvVariables.Rows.Count != 2)
                return;

            if (ButtonStatusEvent != null)
            {                
                Dictionary<string, bool> dictButtonState = new Dictionary<string, bool>();
                dictButtonState.Add("ValidationButtonEnabled", true);
                dictButtonState.Add("PredictionButtonEnabled", true);

                ButtonStatusEventArgs args = new ButtonStatusEventArgs(StatusDictionary:dictButtonState, Set:true);
                ButtonStatusEvent(this, args);
            }
        }


        private void dgvVariables_Scroll(object sender, ScrollEventArgs e)
        {
            int intFirst = dgvVariables.FirstDisplayedScrollingRowIndex;
            if (dgvObs.Rows.Count > 0)
            {                
                if (intFirst >= dgvObs.Rows.Count)
                    dgvObs.FirstDisplayedScrollingRowIndex = dgvObs.Rows.Count - 1;
                else
                    dgvObs.FirstDisplayedScrollingRowIndex = dgvVariables.FirstDisplayedScrollingRowIndex;
            }
            if (dgvStats.Rows.Count > 0)
            {
                if (intFirst >= dgvStats.Rows.Count)
                    dgvStats.FirstDisplayedScrollingRowIndex = dgvStats.Rows.Count - 1;
                else
                    dgvStats.FirstDisplayedScrollingRowIndex = dgvVariables.FirstDisplayedScrollingRowIndex;
            }
        }


        private void dgvObs_Scroll(object sender, ScrollEventArgs e)
        {
            int intFirst = dgvObs.FirstDisplayedScrollingRowIndex;
            if (dgvVariables.Rows.Count > 0)
            {
                if (intFirst >= dgvVariables.Rows.Count)
                    dgvVariables.FirstDisplayedScrollingRowIndex = dgvVariables.Rows.Count - 1;
                else
                    dgvVariables.FirstDisplayedScrollingRowIndex = dgvObs.FirstDisplayedScrollingRowIndex;            
            }

            if (dgvStats.Rows.Count > 0)
            {
                if (intFirst >= dgvStats.Rows.Count)
                    dgvStats.FirstDisplayedScrollingRowIndex = dgvStats.Rows.Count - 1;
                else
                    dgvStats.FirstDisplayedScrollingRowIndex = dgvObs.FirstDisplayedScrollingRowIndex;
            }
        }


        private void dgvStats_Scroll(object sender, ScrollEventArgs e)
        {
            int intFirst = dgvStats.FirstDisplayedScrollingRowIndex;
            if (dgvVariables.Rows.Count > 0)
            {
                if (intFirst >= dgvVariables.Rows.Count)
                    dgvVariables.FirstDisplayedScrollingRowIndex = dgvVariables.Rows.Count - 1;
                else
                    dgvVariables.FirstDisplayedScrollingRowIndex = dgvStats.FirstDisplayedScrollingRowIndex;
            }

            if (dgvObs.Rows.Count > 0)
            {
                if (intFirst >= dgvObs.Rows.Count)
                    dgvObs.FirstDisplayedScrollingRowIndex = dgvObs.Rows.Count - 1;
                else
                    dgvObs.FirstDisplayedScrollingRowIndex = dgvStats.FirstDisplayedScrollingRowIndex;
            }
        }


        //import IV datatable
        public bool btnImportIVs_Click(object sender, EventArgs e)
        {
            bool boolNewMapping = false;

            //Get the currently existing data, if there is any.
            DataTable tblRaw = null;
            dtVariables = (DataTable)dgvVariables.DataSource;
            if (dtVariables != null)
            {
                if (dtVariables.Rows.Count >= 1)
                {
                    dgvVariables.EndEdit();
                    dtVariables.AcceptChanges();
                    tblRaw = dtVariables.AsDataView().ToTable();
                }
            }

            //check to ensure user chose a model first
            if (dictMainEffects == null)
            {
                MessageBox.Show("You must first pick a model from the Available Models");
                return(false);
            }

            DataTable dt;
            try
            {
                DataImportResult import = IvMap.ImportFile(tblRaw);
                dt = import.Data;
                boolNewMapping = import.NewMapping;
            }
            catch
            {
                IvMap = new InputMapper(dictMainEffects, "Model Variables", strArrReferencedVars, "Imported Variables");
                DataImportResult import = IvMap.ImportFile(tblRaw);
                dt = import.Data;
                boolNewMapping = true;
            }

            if (dt == null || dt.Rows.Count == 0)
                return (false);

            dgvVariables.DataSource = dt;

            foreach (DataGridViewColumn dvgCol in dgvVariables.Columns)
            {
                dvgCol.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            
            SetViewOnGrid(dgvVariables);

            //Store the imported data in case we want to move to another modeling method
            dgvVariables.EndEdit();
            dtVariables = (DataTable)dgvVariables.DataSource;
            
            //Store prediction elements in case we want to navigate away from this model and then come back.
            VBCommon.SerializationUtilities.SerializeDataTable(Data: dtVariables, Container: (IDictionary<string, object>)dictPredictionElements[strMethod], Slot: "IVData", Title: "Variables");
            if (boolNewMapping)
            {
                if (((IDictionary<string, object>)(dictPredictionElements[strMethod])).ContainsKey("VariableMapping"))
                    ((IDictionary<string, object>)(dictPredictionElements[strMethod])).Remove("VariableMapping");
                ((IDictionary<string, object>)(dictPredictionElements[strMethod])).Add("VariableMapping", IvMap.PackState());
            }

            return (true);
        }


        public bool btnImportFromEnddat_Click(object sender, EventArgs e)
        {   
            string strEnddatCopy = String.Copy(strEnddatURL);

            //check to ensure user chose a model first
            if (dictMainEffects == null)
            {
                MessageBox.Show("You must first pick a model from the Available Models");
                return (false);
            }

            string strEnddatQuery = EnddatCommon.FormatEnddatQuery(strEnddatCopy, strEnddatTimestamp, strEnddatTimezone, bUseTimestamp);
            return ImportFromEnddat(strEnddatQuery);
        }


        public bool btnEnddatImportDate_Click(object sender, EventArgs e)
        {
            string strEnddatCopy = String.Copy(strEnddatURL);

            //check to ensure user chose a model first
            if (dictMainEffects == null)
            {
                MessageBox.Show("You must first pick a model from the Available Models");
                return (false);
            }

            frmEnddatImportDate EnddatImportDateForm = new frmEnddatImportDate();
            DialogResult dr = EnddatImportDateForm.ShowDialog();
            if (dr == DialogResult.OK)
            {
                DateTime date = EnddatImportDateForm.Date;
                string strEnddatQuery = EnddatCommon.FormatEnddatQuery(strEnddatCopy, date, strEnddatTimestamp, strEnddatTimezone, bUseTimestamp);
                string strImportDate = date.Year.ToString("0000") + "-" + date.Month.ToString("00") + "-" + date.Day.ToString("00");
                string strReplacement = "&datetime=" + strImportDate + "${timestamp}";

                Regex reLatest = new Regex("&?latest=[^&]*");
                Regex reDatetime = new Regex("&?datetime=(?<timestamp>[^&]*)");

                //Modify the EnDDaT query to insert the requested date
                if (reDatetime.Match(strEnddatQuery).Success)
                {
                    strEnddatQuery = reDatetime.Replace(strEnddatQuery, strReplacement);
                }
                else
                {
                    string strTimezone;
                    if (strEnddatTimezone == "Local time") { strTimezone = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).ToString().Split(':')[0].Trim(); }
                    else { strTimezone = strEnddatTimezone.Split(':')[0].Trim(); }
                    strTimezone = strTimezone + "00";

                    //Prepend a zero if necessary:
                    if (strTimezone[0] == '-' && strTimezone.Length == 4) { strTimezone = "-0" + strTimezone[1]; }
                    else if (strTimezone[0] == '+' && strTimezone.Length == 4) { strTimezone = "+0" + strTimezone[1]; }
                    
                    DateTime now = DateTime.Now;
                    string timestamp = now.Hour.ToString("00") + ":" + now.Minute.ToString("00") + ":" + now.Second.ToString("00");
                    strEnddatQuery = strEnddatQuery + "&datetime=" + strImportDate + "T" + timestamp + strTimezone;
                }

                //Remove any possible match with '&latest':
                if (reLatest.Match(strEnddatQuery).Success)
                {
                    strEnddatQuery = reLatest.Replace(strEnddatQuery, "");
                }

                return ImportFromEnddat(strEnddatQuery);
            }
            else { return false; }
        }


        //import IV datatable
        public bool ImportFromEnddat(string EnddatQuery)
        {
            DataTable tblRaw = null;
            dtVariables = (DataTable)dgvVariables.DataSource;
            if (dtVariables != null)
            {
                if (dtVariables.Rows.Count >= 1)
                {
                    dgvVariables.EndEdit();
                    dtVariables.AcceptChanges();
                    tblRaw = dtVariables.AsDataView().ToTable();
                }
            }

            try
            {
                IvMap.InitiateEnddatImport(EnddatQuery, tblRaw, SetEnddatData);
            }
            catch (NullReferenceException)
            {
                IvMap = new InputMapper(dictMainEffects, "Model Variables", strArrReferencedVars, "Imported Variables");
                IvMap.InitiateEnddatImport(EnddatQuery, tblRaw, SetEnddatData);
            }
            catch
            {
                return false;
            }

            return true;
        }


        public void SetEnddatData(DataTable Data, bool NewMapping)
        {
            if (Data == null || Data.Rows.Count == 0)
                return; //false;

            dgvVariables.DataSource = Data;

            foreach (DataGridViewColumn dvgCol in dgvVariables.Columns)
            {
                dvgCol.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
                        
            SetViewOnGrid(dgvVariables);

            //Store the imported data in case we want to move to another modeling method
            dgvVariables.EndEdit();
            dtVariables = (DataTable)dgvVariables.DataSource;
            
            //Store prediction elements in case we want to navigate away from this model and then come back.
            VBCommon.SerializationUtilities.SerializeDataTable(Data: dtVariables, Container: (IDictionary<string, object>)dictPredictionElements[strMethod], Slot: "IVData", Title: "Variables");
            if (NewMapping)
            {
                if (((IDictionary<string, object>)(dictPredictionElements[strMethod])).ContainsKey("VariableMapping"))
                    ((IDictionary<string, object>)(dictPredictionElements[strMethod])).Remove("VariableMapping");
                ((IDictionary<string, object>)(dictPredictionElements[strMethod])).Add("VariableMapping", IvMap.PackState());
            }

            return;
        }
        

        //import IV datatable
        public bool btnSetEnddatURL_Click(object sender, EventArgs e)
        {
            frmEnddatURL enddat_dialog = new frmEnddatURL(strEnddatURL, strEnddatTimestamp, strEnddatTimezone, bUseTimestamp);
            DialogResult dr = enddat_dialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                strEnddatURL = enddat_dialog.URL;
                bUseTimestamp = enddat_dialog.UseTimestamp;
                strEnddatTimestamp = enddat_dialog.Timestamp;
                strEnddatTimezone = enddat_dialog.Timezone;
                
                Regex reBegin = new Regex("&?beginPosition=[^&]*");
                Regex reEnd = new Regex("&?endPosition=[^&]*");
                Regex reInterval = new Regex("&?timeInt=[^&]*");
                Regex reLatest = new Regex("&?latest=[^&]*");
                Regex reStyle = new Regex("&?style=[^&]*");
                Regex reTimezone = new Regex("&?TZ=[^&]*");
                Regex reDatetime = new Regex("&?datetime=[^&]*");                
                
                strEnddatURL = reBegin.Replace(strEnddatURL, "");
                strEnddatURL = reEnd.Replace(strEnddatURL, "");
                strEnddatURL = reInterval.Replace(strEnddatURL, "");
                strEnddatURL = reLatest.Replace(strEnddatURL, "");
                strEnddatURL = reStyle.Replace(strEnddatURL, "");
                strEnddatURL = reTimezone.Replace(strEnddatURL, "");
                strEnddatURL = reDatetime.Replace(strEnddatURL, "");

                //Make sure the timestamp looks like HH:MM:SS
                string[] timestamps = strEnddatTimestamp.Split(':');
                List<int> segments = new List<int>();
                for (int i = 0; i < timestamps.Length; i++)
                {
                    int a;
                    bool success = Int32.TryParse(timestamps[i], out a);
                    if (success) { segments.Add(a); }
                }                
                if (segments.Count == 1 && segments[0]<24)
                    strEnddatTimestamp = strEnddatTimestamp + ":00:00";
                else if (strEnddatTimestamp.Split(':').Length == 2 && segments[0]<24 && segments[1]<60)
                    strEnddatTimestamp = strEnddatTimestamp + ":00";

                return (true);
            }
            else
            { 
                return false;
            }
        }


        public void Reset()
        {
            List<string> models = lbAvailableModels.Items.Cast<string>().ToList();
            foreach (string model in models)
            {
                ClearDataGridViews(model);
                ClearMethod(model);
            }
            
            InitializeInterface();            
        }

        
        public void ClearDataGridViews(string Method)
        {
            if (((IDictionary<string, object>)dictPredictionElements).ContainsKey(Method))
            {
                VBCommon.SerializationUtilities.SerializeDataTable(Data: null, Container: (IDictionary<string, object>)dictPredictionElements[Method], Slot: "IVData");
                VBCommon.SerializationUtilities.SerializeDataTable(Data: null, Container: (IDictionary<string, object>)dictPredictionElements[Method], Slot: "ObsData");
                VBCommon.SerializationUtilities.SerializeDataTable(Data: null, Container: (IDictionary<string, object>)dictPredictionElements[Method], Slot: "StatData");
            }

            //when changes made to modeling, clear the prediction tables (reset)
            if (Method == strMethod)
            {
                this.dgvStats.DataSource = null;
                this.dgvObs.DataSource = null;
                this.dgvVariables.DataSource = null;
                this.IvMap = null;
                this.ObsMap = null;
                txtModel.Text = "";
                txtDecCrit.Text = "";
            }            
        }


        public int ClearMethod(string Method)
        {
            //If there isn't a model from this plugin in the listBox, then we've got nothing to do.
            if (listModels.Contains(Method))
            {
                //Disconnect the selection-changed event handler while we work
                this.lbAvailableModels.SelectedIndexChanged -= new System.EventHandler(this.lbAvailableModels_SelectedIndexChanged);
                
                //Remove the Method from the lists
                listModels.Remove(Method);
                lbAvailableModels.Items.Remove(Method);
                if (dictPredictionElements.ContainsKey(Method))
                    dictPredictionElements.Remove(Method);

                //...Now reconnect the seleciton-changed event handler.
                this.lbAvailableModels.SelectedIndexChanged += new System.EventHandler(this.lbAvailableModels_SelectedIndexChanged);
            }
            return listModels.Count;
        }
        

        //Import OB datatable
        public bool btnImportObs_Click(object sender, EventArgs e)
        {
            bool boolNewMapping = false;

            //Get the currently existing data, if there is any.
            DataTable tblRaw = null;
            dtObs = (DataTable)dgvObs.DataSource;
            if (dtObs != null)
            {
                if (dtObs.Rows.Count >= 1)
                {
                    dgvObs.EndEdit();
                    dtObs.AcceptChanges();
                    tblRaw = dtObs.AsDataView().ToTable();
                }
            }

            //check to ensure user chose a model first
            if (dictMainEffects == null)
            {
                MessageBox.Show("You must first pick a model from the Available Models");
                return false;
            }

            DataTable dt;
            try
            {
                DataImportResult import = ObsMap.ImportFile(tblRaw);
                dt = import.Data;
                boolNewMapping = import.NewMapping;
            }
            catch
            {
                string[] strArrObsColumns = { "ID", strOutputVariable };
                ObsMap = new InputMapper("ID", strArrObsColumns, "Obs");
                DataImportResult import = ObsMap.ImportFile(tblRaw);
                dt = import.Data;
                boolNewMapping = true;
            }

            
            if (dt == null)
                return false;

            dgvObs.DataSource = dt;

            foreach (DataGridViewColumn dvgCol in dgvObs.Columns)
            {
                dvgCol.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            SetViewOnGrid(dgvObs);

            //Store the imported data in case we want to move to another modeling method
            dgvObs.EndEdit();
            dtObs = (DataTable)dgvObs.DataSource;
            VBCommon.SerializationUtilities.SerializeDataTable(Data: dtObs, Container: (IDictionary<string, object>)dictPredictionElements[strMethod], Slot: "ObsData", Title: "Observations");
            if (boolNewMapping)
            {
                if (((IDictionary<string, object>)(dictPredictionElements[strMethod])).ContainsKey("ObsVariableMapping"))
                    ((IDictionary<string, object>)(dictPredictionElements[strMethod])).Remove("ObsVariableMapping");
                ((IDictionary<string, object>)(dictPredictionElements[strMethod])).Add("ObsVariableMapping", ObsMap.PackState());
            }

            return true;
        }

        //Import IV and OB datatable
        public bool btnImportCombined_Click(object sender, EventArgs e)
        {           
            //Beware the hack...
            bool boolNewMapping = false;

            //check to ensure user chose a model first
            if (dictMainEffects == null)
            {
                MessageBox.Show("You must first pick a model from the Available Models");
                return (false);
            }

            DataTable dt;

            List<string> lstRefVars = new List<string>(strArrReferencedVars);
            //lstRefVars.Add("Obs");
            lstRefVars.Add(strOutputVariable);
            InputMapper combinedMap = new InputMapper(dictMainEffects, "Variables", lstRefVars.ToArray(), "Imported Variables");
            DataImportResult import = combinedMap.ImportFile(null);
            dt = import.Data;
            if (dt == null || dt.Rows.Count == 0)
                return (false);

            boolNewMapping = true;

            //Now we have to separate out the combined IM to a separate IV mapper and OBs mapper.

            //Separate out the combined IM to a IV mapper.
            IvMap = new InputMapper("Model Variables", strArrReferencedVars, "Imported Variables");
            Dictionary<string, string> dctIVSColMap = new Dictionary<string, string>(combinedMap.ColumnMap);
            //if (dctIVSColMap.ContainsKey("Obs"))
            //    dctIVSColMap.Remove("Obs");
            if (dctIVSColMap.ContainsKey(strOutputVariable))
                dctIVSColMap.Remove(strOutputVariable);
            IvMap.ColumnMap = dctIVSColMap;

            List<string> lstColNames = new List<string>();
            foreach (DataColumn dc in dt.Columns)
            {
                if (string.Compare(dc.ColumnName, strOutputVariable, true) != 0)
                    lstColNames.Add(dc.ColumnName);

            }

            DataTable dtIVs = dt.DefaultView.ToTable(false, lstColNames.ToArray());
            
            dtVariables = (DataTable)dgvVariables.DataSource;
            if (dtVariables != null)
            {
                if (dtVariables.Rows.Count >= 1)
                {
                    dgvVariables.EndEdit();
                    dtVariables.AcceptChanges();
                    DataTable dtTemp = dtVariables.AsDataView().ToTable();
                    dtTemp.Merge(dtIVs);

                    if (!IvMap.CheckUniqueIDs(dtTemp))
                        return false;

                    dtIVs = dtTemp;
                }
            } 
            dgvVariables.DataSource = dtIVs;


            //separate out the combined IM to a OBs mapper.            
            Dictionary<string, string> dctObsColMap = new Dictionary<string,string>();
            dctObsColMap.Add("ID",combinedMap.ColumnMap["ID"]);
            dctObsColMap.Add(strOutputVariable, combinedMap.ColumnMap[strOutputVariable]);
            //dctObsColMap.Add("Obs",combinedMap.ColumnMap["Obs"]);

            string[] strArrObsColumns = { "ID", strOutputVariable };
            //ObsMap = new InputMapper("Obs IDs", strArrObsColumns, "Obs");
            ObsMap = new InputMapper("ID", strArrObsColumns, strOutputVariable);
            ObsMap.ColumnMap = dctObsColMap;
            //string[] obsColNames = {"ID","Obs"};
            string[] obsColNames = { "ID", strOutputVariable };
            DataTable dtObsNew = dt.DefaultView.ToTable(false, obsColNames);
            dtObs = (DataTable)dgvObs.DataSource;
            if (dtObs != null)
            {
                if (dtObs.Rows.Count >= 1)
                {
                    dgvObs.EndEdit();
                    dtObs.AcceptChanges();
                    DataTable dtTemp = dtObs.AsDataView().ToTable();
                    dtTemp.Merge(dtObsNew);
                    if (!ObsMap.CheckUniqueIDs(dtTemp))
                        return false;
                    dtObsNew = dtTemp;
                }
            }
            dgvObs.DataSource = dtObsNew;

            //if (boolNewMapping)
            //Must be a new mapping if we made it this far.
            if (true)
            {
                if (((IDictionary<string, object>)(dictPredictionElements[strMethod])).ContainsKey("VariableMapping"))
                    ((IDictionary<string, object>)(dictPredictionElements[strMethod])).Remove("VariableMapping");
                ((IDictionary<string, object>)(dictPredictionElements[strMethod])).Add("VariableMapping", IvMap.PackState());

                if (((IDictionary<string, object>)(dictPredictionElements[strMethod])).ContainsKey("ObsVariableMapping"))
                    ((IDictionary<string, object>)(dictPredictionElements[strMethod])).Remove("ObsVariableMapping");
                ((IDictionary<string, object>)(dictPredictionElements[strMethod])).Add("ObsVariableMapping", ObsMap.PackState());
            }

            return boolNewMapping;
        }

        public void btnDisplayColumnMapping_Click(object sender, EventArgs e)
        {
            bool retVal = true;
            bool boolNewMapping = false;

            if (IvMap == null || IvMap.ColumnMap == null || IvMap.ColumnMap.Count == 0)
            {
                MessageBox.Show("No column mapping has been set.");
                return;
            }

            frmShowColumnMapping frmShowColMap = new frmShowColumnMapping(IvMap.LeftCaption, IvMap.RightCaption, IvMap.ColumnMap);
            DialogResult dr = frmShowColMap.ShowDialog();

            //Use abort to indicate 
            if (dr == DialogResult.Abort)
            {
                IDictionary<string, object> dictModel;
                if (dictPredictionElements.ContainsKey(strMethod))
                {
                    dictModel = (IDictionary<string, object>)dictPredictionElements[strMethod];
                    if (dictModel.ContainsKey("VariableMapping"))
                        dictModel.Remove("VariableMapping");

                }                

                if (IvMap != null)
                    IvMap.ColumnMap.Clear();
                IvMap = null;
            }

        }


        //make predictions based on imported ob and iv datatables
        public bool btnMakePredictions_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            VBLogger.GetLogger().LogEvent("0", Globals.messageIntent.UserOnly, Globals.targetSStrip.ProgressBar);
            
            dtVariables = (DataTable)dgvVariables.DataSource;
            if (dtVariables == null)
                return false;

            if (dtVariables.Rows.Count < 1)
                return false;

            dgvVariables.EndEdit();
            dtVariables.AcceptChanges();
            dgvObs.EndEdit();      

            VBLogger.GetLogger().LogEvent("10", Globals.messageIntent.UserOnly, Globals.targetSStrip.ProgressBar);      
            dtObs = (DataTable)dgvObs.DataSource;
            if (dtObs != null)
                dtObs.AcceptChanges();

            DataTable tblRaw = dtVariables.AsDataView().ToTable();
            tblRaw.Columns.Remove("ID");
            List<int[]> lstBadCells = VBCommon.IO.ImportExport.GetBadCellsByRow(tblRaw, "");
            if (lstBadCells.Count > 0)
            {
                MessageBox.Show("There are errors in the data. Run data validation to find and correct them.");
                return false;
            }

            VBLogger.GetLogger().LogEvent("20", Globals.messageIntent.UserOnly, Globals.targetSStrip.ProgressBar);

            DataTable tblForPrediction = BuildPredictionTable(tblRaw, model.ModelString());  

            VBLogger.GetLogger().LogEvent("30", Globals.messageIntent.UserOnly, Globals.targetSStrip.ProgressBar);

            DataSet dsTables = new DataSet();
            tblRaw.TableName = "Raw";
            tblForPrediction.TableName = "Prediction";
            dsTables.Tables.Add(tblRaw);
            dsTables.Tables.Add(tblForPrediction);
            Cursor.Current = Cursors.WaitCursor;

            VBLogger.GetLogger().LogEvent("40", Globals.messageIntent.UserOnly, Globals.targetSStrip.ProgressBar);


            VBLogger.GetLogger().LogEvent("50", Globals.messageIntent.UserOnly, Globals.targetSStrip.ProgressBar);

            //make prediction
            List<double> lstPredictions = model.Predict(dsTables, Convert.ToDouble(txtRegStd.Text), Convert.ToDouble(txtDecCrit.Text), xfrmThreshold, dblThresholdPowerTransformExp);

            VBLogger.GetLogger().LogEvent("60", Globals.messageIntent.UserOnly, Globals.targetSStrip.ProgressBar);
            Cursor.Current = Cursors.WaitCursor;

            //create prediction table to show prediction
            DataTable dtPredictions = new DataTable();
            dtPredictions.Columns.Add("ID", typeof(string));
            dtPredictions.Columns.Add("CalcValue", typeof(double));

            VBLogger.GetLogger().LogEvent("70", Globals.messageIntent.UserOnly, Globals.targetSStrip.ProgressBar);
            
            for (int i = 0; i < lstPredictions.Count; i++)
            {
                DataRow dr = dtPredictions.NewRow();
                dr["ID"] = dtVariables.Rows[i]["ID"].ToString();
                dr["CalcValue"] = lstPredictions[i];
                dtPredictions.Rows.Add(dr);
            }

            VBLogger.GetLogger().LogEvent("80", Globals.messageIntent.UserOnly, Globals.targetSStrip.ProgressBar);

            dtStats = GeneratePredStats(dtPredictions, dtObs, dsTables);

            VBLogger.GetLogger().LogEvent("90", Globals.messageIntent.UserOnly, Globals.targetSStrip.ProgressBar);
            Cursor.Current = Cursors.WaitCursor;

             if (dtStats == null)
                 return false;

            dgvStats.DataSource = dtStats;
            foreach (DataGridViewColumn dvgCol in dgvStats.Columns)
                dvgCol.SortMode = DataGridViewColumnSortMode.NotSortable;

            SetViewOnGrid(dgvStats);

            //Store the predictions in case we want to move to another modeling method
            dgvStats.EndEdit();
            dtStats = (DataTable)dgvStats.DataSource;
            VBCommon.SerializationUtilities.SerializeDataTable(Data: dtStats, Container: (IDictionary<string, object>)dictPredictionElements[strMethod], Slot: "StatData", Title: "Stats");

            VBLogger.GetLogger().LogEvent("100", Globals.messageIntent.UserOnly, Globals.targetSStrip.ProgressBar);
            return true;
        }


        private DataTable BuildPredictionTable(DataTable tblRaw, string strModelExpression)
        {
            DataTable tblForPrediction = new DataTable();

            //Match only +/- symbols that are surrounded by spaces, which is the only place we should see breaks between additive terms.
            string[] strExpressionChunks = Regex.Split(strModelExpression, " [+-] ");
            List<string> strExpressions = new List<string>();

            for (int k=0; k<strExpressionChunks.Count(); k++)
            {
                string var = strExpressionChunks[k];
                if (var != "")
                {
                    int intIndx;
                    string strVariable = var.Trim();
                    if ((intIndx = strVariable.IndexOf('(')) != -1)
                        if ((intIndx = strVariable.IndexOf(')', intIndx)) != -1)
                            intIndx = 0;

                    if ((intIndx = strVariable.IndexOf('*')) != -1)
                        strVariable = strVariable.Substring(intIndx + 1);

                    //If the column name can be cast to a double, then it is the intercept and should be ignored.
                    double intercept;
                    bool castable = double.TryParse(strVariable, out intercept);
                    if (!castable)
                    {
                        strExpressions.Add(strVariable);
                    }
                }
            }

            //Do any transformations/manipulations of the raw data.
            ExpressionEvaluator ee = new ExpressionEvaluator();
            tblForPrediction = ee.EvaluateTable(strExpressions.ToArray(), tblRaw);
            return tblForPrediction;
        }


        //finish editing variables table
        private void dgvVariables_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            dgvVariables.EndEdit();
            dtVariables = (DataTable)dgvVariables.DataSource;
            dtVariables.AcceptChanges();
        }


        //finish editing ob table
        private void dgvObs_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            dgvObs.EndEdit();
            dtObs = (DataTable)dgvVariables.DataSource;
            dtObs.AcceptChanges();
        }


        //generate prediction data for table
        private DataTable GeneratePredStats(DataTable dtPredictions, DataTable dtObs, DataSet dsForPrediction)
        {
            //VBCommon.Transforms.DependentVariableTransforms dvt = GetTransformType();
            if (xfrmThreshold == VBCommon.Transforms.DependentVariableTransforms.Power)
            {
                if (ValidateNumericTextBox(txtPower) == false)
                    return null;
            }

            double dblCrit = Convert.ToDouble(txtDecCrit.Text);
            dblCrit = VBCommon.Transforms.Apply.UntransformThreshold(dblCrit, xfrmThreshold, dblThresholdPowerTransformExp);
            dblCrit = VBCommon.Transforms.Apply.TransformThreshold(dblCrit, xfrmDisplay, dblDisplayPowerTransformExp);

            double dblRegStd = Convert.ToDouble(txtRegStd.Text);
            dblRegStd = VBCommon.Transforms.Apply.UntransformThreshold(dblRegStd, xfrmThreshold, dblThresholdPowerTransformExp);
            dblRegStd = VBCommon.Transforms.Apply.TransformThreshold(dblRegStd, xfrmDisplay, dblDisplayPowerTransformExp);

            double dblProbThreshold = Convert.ToDouble(txtProbabilityThreshold.Text);

            DataTable dt = new DataTable();
            dt.Columns.Add("ID", typeof(string));
            dt.Columns.Add("Model_Prediction", typeof(double));
            dt.Columns.Add("Decision_Criterion", typeof(double));
            dt.Columns.Add("Exceedance_Probability", typeof(double));
            dt.Columns.Add("Regulatory_Standard", typeof(double));
            dt.Columns.Add("Error_Type", typeof(string));
            //dt.Columns.Add("Untransformed", typeof(double));

            double dblPredValue = 0.0;
            string strId = "";

            List<double> lstExceedanceProbability = model.PredictExceedanceProbability(dsForPrediction, Convert.ToDouble(txtRegStd.Text), Convert.ToDouble(txtDecCrit.Text), xfrmThreshold, dblThresholdPowerTransformExp);
            for (int i = 0; i < dtPredictions.Rows.Count; i++)
            {
                try
                {
                    dblPredValue = (double)dtPredictions.Rows[i]["CalcValue"];
                    dblPredValue = VBCommon.Transforms.Apply.UntransformThreshold(dblPredValue, xfrmImported, dblImportedPowerTransformExp);
                    dblPredValue = VBCommon.Transforms.Apply.TransformThreshold(dblPredValue, xfrmDisplay, dblDisplayPowerTransformExp);
                    DataRow dr = dt.NewRow();
                    strId = (string)dtPredictions.Rows[i]["ID"];
                    dr["ID"] = strId;
                    dr["Model_Prediction"] = dblPredValue;
                    dr["Decision_Criterion"] = dblCrit;
                    dr["Exceedance_Probability"] = lstExceedanceProbability[i];
                    dr["Regulatory_Standard"] = dblRegStd;

                    //determine if we have an error and its type
                    //No guarentee we have same num of obs as we do predictions or that we have any obs at all
                    if ((dtObs != null) && (dtObs.Rows.Count > 0))
                    {
                        string strErrType = "";
                        DataRow[] rows = dtObs.Select("ID = '" + strId + "'");

                        if ((rows != null) && (rows.Length > 0))
                        {                  
                            if (rows[0][1] is Double)
                            {
                                double dblObs = VBCommon.Transforms.Apply.UntransformThreshold((double)rows[0][1], xfrmObs, dblObsPowerTransformExp);
                                dblObs = VBCommon.Transforms.Apply.TransformThreshold(dblObs, xfrmDisplay, dblDisplayPowerTransformExp);
                                if (rbRaw.Checked)
                                {
                                    if ((dblPredValue > dblCrit) && (dblObs < dblRegStd))
                                        strErrType = "False Positive";
                                    else if ((dblObs > dblRegStd) && (dblPredValue < dblCrit))
                                        strErrType = "False Negative";
                                }
                                else
                                {
                                    if ((lstExceedanceProbability[i] > dblProbThreshold) && (dblObs < dblRegStd))
                                        strErrType = "False Positive";
                                    else if ((dblObs > dblRegStd) && (lstExceedanceProbability[i] < dblProbThreshold))
                                        strErrType = "False Negative";
                                }
                            }
                            else {strErrType = "No Obs Value"; }
                        }
                        dr["Error_Type"] = strErrType;
                    }
                    dt.Rows.Add(dr);
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(String.Format("i = {0}", i));
                    System.Windows.Forms.MessageBox.Show(e.Message);
                    System.Windows.Forms.MessageBox.Show(lstExceedanceProbability[i].ToString());
                    
                }
            }
            return dt;            
        }


        private double GetTransformPower(string pwrTransform)
        {
            if (String.IsNullOrWhiteSpace(pwrTransform))
                return double.NaN;

            char[] chrArrDelim = ",".ToCharArray();
            string[] strArrSvals = pwrTransform.Split(chrArrDelim);

            double dblPower = 1.0;
            if (strArrSvals.Length != 2)
                 return double.NaN;

            if (!Double.TryParse(strArrSvals[1], out dblPower))
                return double.NaN;

            return dblPower;
        }
        

        private void splitContainer2_SplitterMoved(object sender, SplitterEventArgs e)
        {
            int left1 = splitContainer1.Panel2.Left;
            int left2 = splitContainer2.Panel2.Left;
        }

        
        //load the prediction form
        private void frmIPyPrediction_Load(object sender, EventArgs e)
        {
            cmForResponseVar.MenuItems.Add("Define Transform:");
            cmForResponseVar.MenuItems[0].MenuItems.Add("none", new EventHandler(DefineTransformForRV));
            cmForResponseVar.MenuItems[0].MenuItems.Add("Log10", new EventHandler(DefineTransformForRV));
            cmForResponseVar.MenuItems[0].MenuItems.Add("Ln", new EventHandler(DefineTransformForRV));
            cmForResponseVar.MenuItems[0].MenuItems.Add("Power", new EventHandler(DefineTransformForRV));
            //cmforResponseVar.MenuItems.Add("Untransform", new EventHandler(Untransform));

            cmForStats.MenuItems.Add("Display Transform:");
            cmForStats.MenuItems[0].MenuItems.Add("none", new EventHandler(ChangeDisplayTransform));
            cmForStats.MenuItems[0].MenuItems.Add("Log10", new EventHandler(ChangeDisplayTransform));
            cmForStats.MenuItems[0].MenuItems.Add("Ln", new EventHandler(ChangeDisplayTransform));
            cmForStats.MenuItems[0].MenuItems.Add("Power", new EventHandler(ChangeDisplayTransform));

            //Request that the prediction plugin pass along its list of modeling plugins.
            if (RequestModelPluginList != null)
            {
                EventArgs args = new EventArgs();
                RequestModelPluginList(this, args);
            }
        }


        public void ChangeDisplayTransform(object o, EventArgs e)
        {
            DependentVariableTransforms xfrmLast = xfrmDisplay;
            double dblExponentLast = dblDisplayPowerTransformExp;

            //menu response from right click, determine which transform was selected
            MenuItem mi = (MenuItem)o;
            string transform = mi.Text;
            if (transform == VBCommon.Transforms.DependentVariableTransforms.Power.ToString())
            {
                frmPowerExponent frmExp = new frmPowerExponent(dtStats, dtStats.Columns.IndexOf(dtStats.Columns["Model_Prediction"]));
                DialogResult dlgr = frmExp.ShowDialog();
                if (dlgr != DialogResult.Cancel)
                {
                    string sexp = frmExp.Exponent.ToString("n2");
                    transform += "," + sexp;
                    xfrmDisplay = VBCommon.Transforms.DependentVariableTransforms.Power;
                    dblDisplayPowerTransformExp = Convert.ToDouble(sexp);
                    dtStats.Columns["Model_Prediction"].ExtendedProperties[VBCommon.Globals.DEPENDENTVARIBLEDEFINEDTRANSFORM] = transform;
                    SetStatsTransformCheckmarks(Item: 3);
                }
            }
            else
            {
                if (String.Compare(transform, "Log10", true) == 0)
                {
                    xfrmDisplay = VBCommon.Transforms.DependentVariableTransforms.Log10;
                    SetStatsTransformCheckmarks(Item: 1);
                }
                else if (String.Compare(transform, "Ln", true) == 0)
                {
                    xfrmDisplay = VBCommon.Transforms.DependentVariableTransforms.Ln;
                    SetStatsTransformCheckmarks(Item: 2);
                }
                else if (String.Compare(transform, "none", true) == 0)
                {
                    xfrmDisplay = VBCommon.Transforms.DependentVariableTransforms.none;
                    SetStatsTransformCheckmarks(Item: 0);
                }
                
                dtStats.Columns["Model_Prediction"].ExtendedProperties[VBCommon.Globals.DEPENDENTVARIBLEDEFINEDTRANSFORM] = transform;
            }

            for (int i = 0; i < dgvStats.Rows.Count; i++)
            {
                //Get the current values
                DataGridViewRow row = dgvStats.Rows[i];
                double dblPrediction = Convert.ToDouble(row.Cells["Model_Prediction"].Value);
                double dblRegulatory = Convert.ToDouble(row.Cells["Regulatory_Standard"].Value);
                double dblDecision = Convert.ToDouble(row.Cells["Decision_Criterion"].Value);

                //Untransform the current values
                dblPrediction = VBCommon.Transforms.Apply.UntransformThreshold(dblPrediction, xfrmLast, dblExponentLast);
                dblRegulatory = VBCommon.Transforms.Apply.UntransformThreshold(dblRegulatory, xfrmLast, dblExponentLast);
                dblDecision = VBCommon.Transforms.Apply.UntransformThreshold(dblDecision, xfrmLast, dblExponentLast);

                //Apply the new transform
                dblPrediction = VBCommon.Transforms.Apply.TransformThreshold(dblPrediction, xfrmDisplay, dblDisplayPowerTransformExp);
                dblRegulatory = VBCommon.Transforms.Apply.TransformThreshold(dblRegulatory, xfrmDisplay, dblDisplayPowerTransformExp);
                dblDecision = VBCommon.Transforms.Apply.TransformThreshold(dblDecision, xfrmDisplay, dblDisplayPowerTransformExp);

                //Write the transformed values back to the datagridview:
                row.Cells["Model_Prediction"].Value = dblPrediction;
                row.Cells["Regulatory_Standard"].Value = dblRegulatory;
                row.Cells["Decision_Criterion"].Value = dblDecision;
            }

            NotifyContainer();
        }


        public void DefineTransformForRV(object o, EventArgs e)
        {
            //menu response from right click, determine which transform was selected
            MenuItem mi = (MenuItem)o;
            string transform = mi.Text;
            if (transform == VBCommon.Transforms.DependentVariableTransforms.Power.ToString())
            {
                frmPowerExponent frmExp = new frmPowerExponent(dtObs, 1);
                DialogResult dlgr = frmExp.ShowDialog();
                if (dlgr != DialogResult.Cancel)
                {
                    string sexp = frmExp.Exponent.ToString("n2");
                    transform += "," + sexp;
                    xfrmObs = VBCommon.Transforms.DependentVariableTransforms.Power;
                    dblObsPowerTransformExp = Convert.ToDouble(sexp);
                    dtObs.Columns[1].ExtendedProperties[VBCommon.Globals.DEPENDENTVARIBLEDEFINEDTRANSFORM] = transform;
                    SetObsTransformCheckmarks(Item: 3);
                }
            }
            else
            {
                if (String.Compare(transform, "Log10", true) == 0)
                {
                    xfrmObs = VBCommon.Transforms.DependentVariableTransforms.Log10;
                    SetObsTransformCheckmarks(Item: 1);
                }
                else if (String.Compare(transform, "Ln", true) == 0)
                {
                    xfrmObs = VBCommon.Transforms.DependentVariableTransforms.Ln;
                    SetObsTransformCheckmarks(Item: 2);
                }
                else if (String.Compare(transform, "none", true) == 0)
                {
                    xfrmObs = VBCommon.Transforms.DependentVariableTransforms.none;
                    SetObsTransformCheckmarks(Item: 0);
                }

                dtObs.Columns[1].ExtendedProperties[VBCommon.Globals.DEPENDENTVARIBLEDEFINEDTRANSFORM] = transform;
            }

            NotifyContainer();
        }


        private void SetObsTransformCheckmarks(int Item)
        {
            int i;
            intCheckedRVTransform = Item;

            //Handle the defined transforms' menu:
            for (i = 0; i < 4; i++)
            {
                if (Item == i)
                    cmForResponseVar.MenuItems[0].MenuItems[i].Checked = true;
                else
                    cmForResponseVar.MenuItems[0].MenuItems[i].Checked = false;
            }
        }


        private void SetStatsTransformCheckmarks(int Item)
        {
            int i;
            intCheckedRVTransform = Item;

            //Handle the defined transforms' menu:
            for (i = 0; i < 4; i++)
            {
                if (Item == i)
                    cmForStats.MenuItems[0].MenuItems[i].Checked = true;
                else
                    cmForStats.MenuItems[0].MenuItems[i].Checked = false;
            }
        }


        public void btnExportTable_Click(object sender, EventArgs e)
        {
            //end and save edits on all 3 tables
            dgvVariables.EndEdit();
            dtVariables = (DataTable)dgvVariables.DataSource;
            if (dtVariables != null)
                dtVariables.AcceptChanges();
            else
                return;

            dgvObs.EndEdit();
            dtObs = (DataTable)dgvObs.DataSource;
            if (dtObs != null)
                dtObs.AcceptChanges();

            dgvStats.EndEdit();
            dtStats = (DataTable)dgvStats.DataSource;
            if (dtStats != null)
                dtStats.AcceptChanges();

            if ((dtVariables == null) && (dtObs == null) && (dtStats == null))
                return;
            //save exported as
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Export Prediction Data";
            sfd.Filter = @"CSV Files|*.csv|All Files|*.*";

            DialogResult dr = sfd.ShowDialog();
            if (dr == DialogResult.Cancel)
                return;

            int intMaxRowsVars = dtVariables.Rows.Count;
            int intMaxRowsObs = 0;
            int intMaxRowsStats = 0;
            
            if (dtObs != null)
                intMaxRowsObs = dtObs.Rows.Count;
            if (dtStats != null)
                intMaxRowsStats = dtStats.Rows.Count;

            int intMaxRows = Math.Max(intMaxRowsVars, Math.Max(intMaxRowsObs, intMaxRowsStats));

            StringBuilder sb = new StringBuilder();

            //Write out the column headers
            if (dtVariables != null)
            {
                for (int i = 0; i < dtVariables.Columns.Count; i++)
                {
                    if (i > 0)
                        sb.Append(",");

                    sb.Append(dtVariables.Columns[i].ColumnName);
                }
            }

            if (dtObs != null)
            {
                for (int i = 0; i < dtObs.Columns.Count; i++)
                {
                    sb.Append(",");
                    sb.Append(dtObs.Columns[i].ColumnName);
                }
            }

            if (dtStats != null)
            {
                for (int i = 0; i < dtStats.Columns.Count; i++)
                {
                    sb.Append(",");
                    sb.Append(dtStats.Columns[i].ColumnName);
                }
            } //Finished writing out column headers
            

            sb.Append(Environment.NewLine);

            //write out the data
            for (int i = 0; i < intMaxRows; i++)
            {
                for (int j = 0; j < dtVariables.Columns.Count; j++)
                {
                    if (j > 0)
                        sb.Append(",");

                    if (i < dtVariables.Rows.Count)
                        sb.Append(dtVariables.Rows[i][j].ToString());
                    else
                        sb.Append("");
                }
               

                if (dtObs != null)
                {
                    for (int j = 0; j < dtObs.Columns.Count; j++)
                    {                        
                        sb.Append(",");

                        if (i < dtObs.Rows.Count)
                            sb.Append(dtObs.Rows[i][j].ToString());
                        else
                            sb.Append("");
                    }    
                }

                if (dtStats != null)
                {
                    for (int j = 0; j < dtStats.Columns.Count; j++)
                    {
                        sb.Append(",");

                        if (i < dtStats.Rows.Count)
                            sb.Append(dtStats.Rows[i][j].ToString());
                        else
                            sb.Append("");
                    }
                }

                sb.Append(Environment.NewLine);
            } //End writing out data            

            string fileName = sfd.FileName;

            StreamWriter sw = new StreamWriter(fileName);
            sw.Write(sb.ToString());
            sw.Close();
        }


        //add comma separators to the column names
        private StringBuilder AddCommaSeparatedColumns(DataTable dt, StringBuilder sb)
        {
            if ((dt == null) || (dt.Columns.Count < 1))
                return sb;

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (i > 0)
                    sb.Append(",");

                sb.Append(dt.Columns[i].ColumnName);
            }
            return sb;
        }


        //add a row with commas added
        private StringBuilder AddRow(DataTable dt, StringBuilder sb)
        {
            if ((dt == null) || (dt.Columns.Count < 1))
                return sb;

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (i > 0)
                    sb.Append(",");

                sb.Append(dt.Columns[i].ColumnName);
            }
            return sb;
        }


        //Import table
        public void btnImportTable_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Open Prediction Data";
            ofd.Filter = @"VB2 Prediction Files|*.vbpred|All Files|*.*";
            DialogResult dr = ofd.ShowDialog();
            if (dr == DialogResult.Cancel)
                return;
            //save filename
            string fileName = ofd.FileName;
            //read the incoming table
            DataSet ds = new DataSet();
            ds.ReadXml(fileName, XmlReadMode.ReadSchema);

            if (ds.Tables.Contains("Variables") == false)
            {
                MessageBox.Show("Invalid Prediction Dataset.  Does not contain variable information.");
                return;
            }
            //save values
            dtVariables = ds.Tables["Variables"];
            dtObs = ds.Tables["Observations"];
            dtStats = ds.Tables["Stats"];

            dgvVariables.DataSource = dtVariables;
            dgvObs.DataSource = dtObs;
            dgvStats.DataSource = dtStats;
        }


        //clear tables. set all to null
        public void btnClear_Click(object sender, EventArgs e)
        {
            dgvVariables.DataSource = null;
            dgvObs.DataSource = null;
            dgvStats.DataSource = null;

            if (dtVariables != null)
                dtVariables.Clear();            
            dtVariables = null;

            if (dtObs != null)
                dtObs.Clear();            
            dtObs = null;

            if (dtStats != null)
                dtStats.Clear();            
            dtStats = null;

            IvMap = null;
            ObsMap = null;
            //dictPredictionElements = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> kvpPlugin in dictPredictionElements)
            {
                Dictionary<string, object> dictPlugin = kvpPlugin.Value as Dictionary<string, object>;
                if (dictPlugin.ContainsKey("IVData")) { dictPlugin.Remove("IVData");}
                if (dictPlugin.ContainsKey("ObsData")) { dictPlugin.Remove("ObsData");}
                if (dictPlugin.ContainsKey("StatData")) { dictPlugin.Remove("StatData");}
            }
            lbAvailableModels.ClearSelected();

            InitializeInterface();

            dgvVariables.DataSource = InitializeIVData();
            dgvObs.DataSource = InitializeObsData();

            SetViewOnGrid(dgvVariables);
            SetViewOnGrid(dgvObs);
        }


        //plot predictions
        public void btnPlot_Click(object sender, EventArgs e)
        {
            dgvObs.EndEdit();
            dgvStats.EndEdit();

            //ensure there is observation and prediction data
            DataTable dtObs = dgvObs.DataSource as DataTable;
            DataTable dtStats = dgvStats.DataSource as DataTable;

            if ((dtObs == null) || (dtObs.Rows.Count < 1))
            {
                MessageBox.Show("Plotting requires Observation data.");
                return;
            }
            
            if ((dtStats == null) || (dtStats.Rows.Count < 1))
            {
                MessageBox.Show("Plotting requires Prediction data.");
                return;
            }

            //start plotting
            frmPredictionScatterPlot frmPlot = new frmPredictionScatterPlot(dtObs, dtStats, ObservationColumn:strOutputVariable, PredictionColumn:"Model_Prediction", ProbabilityColumn:"Exceedance_Probability");
            frmPlot.Show();

            //configure the plot display
            frmPlot.ConfigureDisplay(DecisionThreshold:Convert.ToDouble(txtDecCrit.Text), RegulatoryThreshold:Convert.ToDouble(txtRegStd.Text), ProbabilityThreshold:Convert.ToDouble(txtProbabilityThreshold.Text), ObservationTransform:xfrmObs, ObservationExponent:dblObsPowerTransformExp, PredictionTransform:xfrmDisplay, PredictionExponent:dblDisplayPowerTransformExp, ThresholdTransform:xfrmThreshold, ThresholdExponent:dblThresholdPowerTransformExp, RawPredictions:rbRaw.Checked);
        }


        //selected variable change 
        private void dgvVariables_SelectionChanged(object sender, EventArgs e)
        {
            //unsubscribe
            dgvObs.SelectionChanged -= new EventHandler(dgvObs_SelectionChanged);
            dgvStats.SelectionChanged -= new EventHandler(dgvStats_SelectionChanged);
            //clear data
            DataGridViewSelectedRowCollection selRowCol = dgvVariables.SelectedRows;
          
            List<string> lstObsKeys = new List<string>();
            if (dgvObs.DataSource != null)
            {
                dgvObs.ClearSelection();
                foreach (DataGridViewRow row in dgvObs.Rows)
                {
                    try { lstObsKeys.Add(row.Cells[0].Value.ToString()); }
                    catch { }
                }
            }

            List<string> lstStatsKeys = new List<string>();
            if (dgvStats.DataSource != null)
            {
                dgvStats.ClearSelection();
                foreach (DataGridViewRow row in dgvStats.Rows)
                {
                    try { lstStatsKeys.Add(row.Cells[0].Value.ToString()); }
                    catch { }
                }
            }

            for (int i=0;i<selRowCol.Count;i++)
            {
                if (dgvObs.DataSource != null)
                {                    
                    if (lstObsKeys.Contains(selRowCol[i].Cells[0].Value))
                        dgvObs.Rows[lstObsKeys.IndexOf(selRowCol[i].Cells[0].Value.ToString())].Selected = true;
                }

                if (dgvStats.DataSource != null)
                {
                    if (lstStatsKeys.Contains(selRowCol[i].Cells[0].Value))
                        dgvStats.Rows[lstStatsKeys.IndexOf(selRowCol[i].Cells[0].Value.ToString())].Selected = true;

                    //if (selRowCol[i].Index < dgvStats.Rows.Count)
                    //    dgvStats.Rows[selRowCol[i].Index].Selected = true;
                }
            }
            //resubscribe
            dgvObs.SelectionChanged += new EventHandler(dgvObs_SelectionChanged);
            dgvStats.SelectionChanged += new EventHandler(dgvStats_SelectionChanged);
        }


        //selected observation change
        private void dgvObs_SelectionChanged(object sender, EventArgs e)
        {
            //unsubscribe
            dgvVariables.SelectionChanged -= new EventHandler(dgvVariables_SelectionChanged);
            dgvStats.SelectionChanged -= new EventHandler(dgvStats_SelectionChanged);

            DataGridViewSelectedRowCollection selRowCol = dgvObs.SelectedRows;

            //clear all
            List<string> lstVariablesKeys = new List<string>();
            if (dgvVariables.DataSource != null)
            {
                dgvVariables.ClearSelection();
                foreach (DataGridViewRow row in dgvVariables.Rows)
                {
                    try { lstVariablesKeys.Add(row.Cells[0].Value.ToString()); }
                    catch { }
                }
            }

            List<string> lstStatsKeys = new List<string>();
            if (dgvStats.DataSource != null)
            {
                dgvStats.ClearSelection();
                foreach (DataGridViewRow row in dgvStats.Rows)
                {
                    try { lstStatsKeys.Add(row.Cells[0].Value.ToString()); }
                    catch { }
                }
            }

            for (int i = 0; i < selRowCol.Count; i++)
            {
                if (dgvVariables.DataSource != null)
                {                    
                    if (lstVariablesKeys.Contains(selRowCol[i].Cells[0].Value))
                        dgvVariables.Rows[lstVariablesKeys.IndexOf(selRowCol[i].Cells[0].Value.ToString())].Selected = true;
                }

                if (dgvStats.DataSource != null)
                {
                    if (lstStatsKeys.Contains(selRowCol[i].Cells[0].Value))
                        dgvStats.Rows[lstStatsKeys.IndexOf(selRowCol[i].Cells[0].Value.ToString())].Selected = true;
                }
            }

            //resubscribe
            dgvVariables.SelectionChanged += new EventHandler(dgvVariables_SelectionChanged);
            dgvStats.SelectionChanged += new EventHandler(dgvStats_SelectionChanged);
        }


        //selected stats change
        private void dgvStats_SelectionChanged(object sender, EventArgs e)
        {
            //unsubscribe
            dgvVariables.SelectionChanged -= new EventHandler(dgvVariables_SelectionChanged);
            dgvObs.SelectionChanged -= new EventHandler(dgvObs_SelectionChanged);

            DataGridViewSelectedRowCollection selRowCol = dgvStats.SelectedRows;

            //clear all
            List<string> lstVariablesKeys = new List<string>();
            if (dgvVariables.DataSource != null)
            {
                dgvVariables.ClearSelection();
                foreach (DataGridViewRow row in dgvVariables.Rows)
                {
                    try { lstVariablesKeys.Add(row.Cells[0].Value.ToString()); }
                    catch { }
                }
            }

            List<string> lstObsKeys = new List<string>();
            if (dgvObs.DataSource != null)
            {
                dgvObs.ClearSelection();
                foreach (DataGridViewRow row in dgvObs.Rows)
                {
                    try { lstObsKeys.Add(row.Cells[0].Value.ToString()); }
                    catch { }
                }
            }

            for (int i = 0; i < selRowCol.Count; i++)
            {
                if (dgvVariables.DataSource != null)
                {
                    if (lstVariablesKeys.Contains(selRowCol[i].Cells[0].Value))
                        dgvVariables.Rows[lstVariablesKeys.IndexOf(selRowCol[i].Cells[0].Value.ToString())].Selected = true;
                }

                if (dgvObs.DataSource != null)
                {
                    if (lstObsKeys.Contains(selRowCol[i].Cells[0].Value))
                        dgvObs.Rows[lstObsKeys.IndexOf(selRowCol[i].Cells[0].Value.ToString())].Selected = true;
                }
            }

            //resubscribe
            dgvVariables.SelectionChanged += new EventHandler(dgvVariables_SelectionChanged);
            dgvObs.SelectionChanged += new EventHandler(dgvObs_SelectionChanged);
        }


        public void SetViewOnGrid(DataGridView dgv)
        {
            Cursor.Current = Cursors.WaitCursor;

            //utility method used to set numerical precision displayed in grid

            //seems to be the only way I can figure to get a string in col 1 that may
            //(or may not) be a date and numbers in all other columns.
            //in design mode set "no format" for the dgv defaultcellstyle
            if (dgv.Rows.Count <= 1) return;

            string testcellval = string.Empty;
            for (int col = 0; col < dgv.Columns.Count; col++)
            {
                testcellval = dgv[col, 0].Value.ToString();
                double result;
                bool isNum = Double.TryParse(testcellval, out result); //try a little visualbasic magic

                if (isNum)
                {
                    dgv.Columns[col].ValueType = typeof(System.Double);
                    dgv.Columns[col].DefaultCellStyle.Format = "g4";
                }
                else
                {
                    dgv.Columns[col].ValueType = typeof(System.String);
                }
            }
        }


        private List<string> getBadCells(DataTable dt, bool skipFirstColumn)
        {
            double dblResult;
            if (dt == null)
                return null;
            //look for blank and non numeric cell values
            List<string> lstCells = new List<string>();
            foreach (DataColumn dc in dt.Columns)
            {
                if (skipFirstColumn)
                {
                    if (dt.Columns.IndexOf(dc) == 0)
                        continue;
                }
                foreach (DataRow dr in dt.Rows)
                {
                    if (string.IsNullOrEmpty(dr[dc].ToString()))
                        lstCells.Add("Row " + dr[0].ToString() + " Column " + dc.Caption + " has blank cell.");
                    else if (!Double.TryParse(dr[dc].ToString(), out dblResult) && dt.Columns.IndexOf(dc) != 0)
                        lstCells.Add("Row " + dr[0].ToString() + " Column " + dc.Caption + " has non-numeric cell value: '" + dr[dc].ToString() + "'");
                }
            }
            return lstCells;
        }


        //event handler for error
        private void dgvVariables_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            string strErr = "Data value must be numeric.";
            dgvVariables.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected = true;
            MessageBox.Show(strErr);
        }


        //event handler for error
        private void dgvObs_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            string err = "Data value must be numeric.";
            dgvObs.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected = true;
            MessageBox.Show(err);
        }


        private void rbNone_CheckedChanged(object sender, EventArgs e)
        {
            if (rbNone.Checked)
            {
                xfrmThreshold = DependentVariableTransforms.none;
                dblThresholdPowerTransformExp = Double.NaN;
            }
        }


        private void rbLog10_CheckedChanged(object sender, EventArgs e)
        {
            if (rbLog10.Checked)
            {
                xfrmThreshold = DependentVariableTransforms.Log10;
                dblThresholdPowerTransformExp = Double.NaN;
            }
        }


        private void rbLn_CheckedChanged(object sender, EventArgs e)
        {
            if (rbLn.Checked)
            {
                xfrmThreshold = DependentVariableTransforms.Ln;
                dblThresholdPowerTransformExp = Double.NaN;
            }
        }


        private void rbPower_CheckedChanged(object sender, EventArgs e)
        {
            if (rbPower.Checked)
            {
                frmPowerExponent frmExp = new frmPowerExponent(dtObs, 1);
                DialogResult dlgr = frmExp.ShowDialog();
                if (dlgr != DialogResult.Cancel)
                {
                    string sexp = frmExp.Exponent.ToString("n2");
                    xfrmThreshold = DependentVariableTransforms.Power;
                    dblThresholdPowerTransformExp = Convert.ToDouble(sexp);
                    txtPower.Text = sexp.ToString();
                    //state = dtState.dirty;
                    //NotifyContainer();
                }
                //txtPower.Enabled = true;
            }
            //else
                //txtPower.Enabled = false;
        }


        //check what is entered in the power textbox
        private bool ValidateNumericTextBox(TextBox txtBox)
        {
            double dblVal = 1.0;
            if (!Double.TryParse(txtBox.Text, out dblVal))
            {
                MessageBox.Show(txtPower.Text + "Invalid number.");
                txtPower.Focus();
                return false;
            }
            return true;
        }


        //check value entered in decCrit textbox when leaving
        private void txtDecCrit_Leave(object sender, EventArgs e)
        {
            double dblResult;
            if (!Double.TryParse(txtDecCrit.Text, out dblResult))
            {
                MessageBox.Show("Invalid number.");
                txtDecCrit.Focus();
            }
        }


        //check value entered in regStd textbox when leaving
        private void txtRegStd_Leave(object sender, EventArgs e)
        {
            double dblResult;
            if (!Double.TryParse(txtRegStd.Text, out dblResult))
            {
                MessageBox.Show("Invalid number.");
                txtRegStd.Focus();
            }
        }


        //validate imported datatable
        public bool btnIVDataValidation_Click(object sender, EventArgs e)
        {
            //check for non unique records, blank records
            dgvVariables.EndEdit();
            DataTable dt = dgvVariables.DataSource as DataTable;
            if ((dt == null) ||(dt.Rows.Count < 1))
                return(false);

            DataTable dtCopy = dt.Copy();
            DataTable dtSaved = dt.Copy();
            frmMissingPredValues frmMissVal = new frmMissingPredValues(dgvVariables, dtCopy);
            frmMissVal.ShowDialog();
            if (frmMissVal.Status)
            {
                int errndx;
                if (!InputMapper.RecordIndexUnique(frmMissVal.ValidatedDT, out errndx))
                {
                    MessageBox.Show("Unable to process datasets with non-unique record identifiers.\n" +
                                    "Fix your datatable by assuring unique record identifier values\n" +
                                    "in the ID column and try validating again.\n\n" +
                                    "Record Identifier values cannot be blank or duplicated;\nencountered " +
                                    "error near row " + errndx.ToString(), "Data Validation Error - Cannot Process This Dataset", MessageBoxButtons.OK);
                    return(false);
                }
                dgvVariables.DataSource = frmMissVal.ValidatedDT;
                //btnMakePredictions.Enabled = true;
                return (true);
            }
            else
            {
                dgvVariables.DataSource = dtSaved;
                return (false);
            }
        }


        //mouseup after right-click on obs
        private void dgvObs_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            ShowContextMenus((DataGridView)sender, e);
        }


        //mouseup after right-click on obs
        private void dgvStats_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            ShowContextMenus((DataGridView)sender, e);
        }


        //show the contextmenu after mouseup to select transform type
        private void ShowContextMenus(DataGridView dgv, MouseEventArgs me)
        {
            DataGridView.HitTestInfo ht = dgv.HitTest(me.X, me.Y);
            int intColndx = ht.ColumnIndex;
            int intRowndx = ht.RowIndex;

            if (dgv == dgvObs)
            {
                DataTable _dt = (DataTable)dgvObs.DataSource;
                if (intRowndx > 0 && intColndx > 0) return; //cell hit, go away
                //get transform user selected
                if (intRowndx < 0 && intColndx >= 0)
                {
                    if (intColndx == 1)
                    {
                        cmForResponseVar.Show(dgv, new Point(me.X, me.Y));
                    }
                }
            }
            else if (dgv == dgvStats)
            {
                DataTable _dt = (DataTable)dgvStats.DataSource;
                if (intRowndx > 0 && intColndx > 0) return; //cell hit, go away

                //get transform user selected
                if (intRowndx < 0 && intColndx >= 0)
                {
                    if (intColndx == dgvStats.Columns.IndexOf(dgvStats.Columns["Model_Prediction"]) || intColndx == dgvStats.Columns.IndexOf(dgvStats.Columns["Decision_Criterion"]) || intColndx == dgvStats.Columns.IndexOf(dgvStats.Columns["Regulatory_Standard"]))
                    {
                        cmForStats.Show(dgv, new Point(me.X, me.Y));
                    }
                }
            }
        }


        private void dgvVariables_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            //If user has edited the ID column, make sure the IDs are still unique.
            //btnMakePredictions.Enabled = false;
        }


        //create the cells in the datagridview
        private void dgvVariables_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            StringFormat sf = new StringFormat();
            int intCount = dgvVariables.RowCount;
            sf.Alignment = StringAlignment.Center;
            if(( e.ColumnIndex < 0) && (e.RowIndex >= 0) && (e.RowIndex < intCount) )
            {
                e.PaintBackground(e.ClipBounds, true);
                e.Graphics.DrawString((e.RowIndex + 1).ToString(), this.Font, Brushes.Black, e.CellBounds, sf);
                e.Handled = true;
            }
        }

        //event handler 
        private string ModelTabStatus()
        {
            strModelTabClean = null;

            if(ModelTabStateRequested != null)
            {
                EventArgs e = new EventArgs();
                ModelTabStateRequested(this, e);

                while(strModelTabClean == null)
                { }
            }
            return strModelTabClean;
        }


        //set the modeltabstate clean flag
        public string ModelTabState
        {
            set { strModelTabClean = value; }
        }
        
        
        private void NotifyContainer()
        {
            if (NotifiableChangeEvent != null && boolAllowNotification)
            {
                EventArgs e = new EventArgs();
                NotifiableChangeEvent(this, e);
            }
        }

        private IDictionary<string, object> GetVariableDisplayOrder()
        {

            IDictionary<string, object> dctVariableColumnDisplayOrder = new Dictionary<string, object>();

            foreach (DataGridViewColumn col in dgvVariables.Columns)
                dctVariableColumnDisplayOrder.Add(col.Name, col.DisplayIndex);

            return dctVariableColumnDisplayOrder;

        }


        private void dgvVariables_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (!dictPredictionElements.ContainsKey(strMethod))
                return;

            Dictionary<string, object> dctMethod = dictPredictionElements[strMethod] as Dictionary<string, object>;
            IDictionary<string, object> dctVarDispOrder = null;

            if (!dctMethod.ContainsKey("VariableDisplayOrder"))
                return;

            string jsonRep = null;            
            foreach (KeyValuePair<string, object> pair in dctMethod)
            {
                if (string.Compare(pair.Key, "VariableDisplayOrder", true) == 0)
                {
                    dctVarDispOrder = pair.Value as IDictionary<string, object>;
                    //jsonRep = pair.Value.ToString();
                    break;
                }
            }

            //Type objType = typeof(Dictionary<string, object>);
            //object objDeserialized = JsonConvert.DeserializeObject(jsonRep, objType);

           // dctVarDispOrder = (IDictionary<string, object>)objDeserialized;

            if (dctVarDispOrder == null || dctVarDispOrder.Count < 1)
                return;

            foreach (DataGridViewColumn col in dgvVariables.Columns)
            {
                if (dctVarDispOrder.ContainsKey(col.Name))
                    col.DisplayIndex = Convert.ToInt32(dctVarDispOrder[col.Name].ToString());
            }

        }

        private void btnSaveColumnOrder_Click(object sender, EventArgs e)
        {

            if (dictPredictionElements.ContainsKey(strMethod))
            {

                IDictionary<string, object> dctMethod = dictPredictionElements[strMethod] as IDictionary<string, object>;
                if (dctMethod == null)
                    return;

                IDictionary<string, object> dct = GetVariableDisplayOrder();
                if (dctMethod.ContainsKey("VariableDisplayOrder"))
                    dctMethod.Remove("VariableDisplayOrder");

                dctMethod.Add("VariableDisplayOrder", dct);
            }

        }

        private void btnClearColumnOrder_Click(object sender, EventArgs e)
        {
            if (dictPredictionElements.ContainsKey(strMethod))
            {

                Dictionary<string, object> dctMethod = dictPredictionElements[strMethod] as Dictionary<string, object>;
                if (dctMethod == null)
                    return;

                if (dctMethod.ContainsKey("VariableDisplayOrder"))
                    dctMethod.Remove("VariableDisplayOrder");

            }

        }    
    }
}
