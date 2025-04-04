﻿using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using DotSpatial.Controls;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.Globalization;
using VBCommon;
using System.Threading;
using System.Collections.Specialized;
using Newtonsoft.Json;
//using VBDatasheet;
using VBProjectManager;
using VBCommon.Transforms;
using System.Windows.Forms.DataVisualization.Charting;

using RDotNet;
using RDotNet.NativeLibrary;

namespace RCommon
{
    public abstract partial class RModelingGui : UserControl
    {
        //Get access to the R interface:
        private REngine R = RInterface.R;
        protected RControllerInterface Controller;
        protected RModelInterface model = null;
        private static double dblProgressRangeLow, dblProgressRangeHigh;

        protected bool bAnticipatingModel = false;
        protected bool bAllowUpdateData = true;

        //Class member definitions:
        //Events:
        public delegate void EventHandler<TArgs>(object sender, TArgs args) where TArgs : EventArgs;
        public event EventHandler ModelUpdated;
        public event EventHandler ModelingCanceledEvent;
        public event EventHandler ModelingCompleteEvent;
        public event EventHandler ControlChangeEvent;
        public event EventHandler<RunButtonStatusArgs> RunButtonStatusChanged;

        public event EventHandler ManipulateDataTab;
        public event EventHandler ModelTab;
        public event EventHandler VariableTab;
        
        //Delegates to get data from Virtual Beach
        public delegate void RequestData(object sender, EventArgs args);
        public event EventHandler IronPythonInterfaceRequested;
        protected DataTable tblModelData;
        
        //Related to control of graphical interface elements:
        protected int intThresholdIndex;
        protected List<double> listCandidateSpecificity;
        protected List<double> listCandidateThreshold;
        protected List<double> listValidationSpecificity = new List<double>();
        protected List<double> listValidationThreshold = new List<double>();
        protected List<double> listTruePos = new List<double>();
        protected List<double> listTrueNeg = new List<double>();
        protected List<double> listFalsePos = new List<double>();
        protected List<double> listFalseNeg = new List<double>();
        protected bool boolControlStatus = true;
        protected bool boolInitialControlStatus = true;
        private List<ListItem> listPredictors = new List<ListItem>();

        //A flag to indicate whether this modeling tab has been used.
        private bool boolComplete = false;
        protected bool boolVirgin = true;
        private bool boolClean = true;
        private bool boolRunning = false;
        protected double dblMandateThreshold;
        protected string strMethod;

        //RunChanged = true:running, false:canceled
        public event EventHandler ControlStatusUpdated;
        private IDictionary<string, object> dictPackedState = null;
        private string modelstring;

        protected DependentVariableTransforms xfrmImported;
        protected DependentVariableTransforms xfrmThreshold;
        protected double dblImportedPowerTransformExponent;
        protected double dblThresholdPowerTransformExponent;

        private DataTable correlationData = null;
        private DataTable _dtFull = null;
        DataTable dt = new DataTable();
        
        //Number of observations in the dataset
        int intNumObs;

        IDictionary<string, object> dictPackedPlugin = new Dictionary<string, object>();




        public IDictionary<string, object> PackedState
        {
            get { return dictPackedState; }
        }

        public RModelingGui()
        {
            InitializeComponent();
            boolVirgin = true;
        
            //initialize the regulatory threshold
            EventArgs e = new EventArgs();
            rbValue_CheckedChanged(this, e);
            rbLogeValue_CheckedChanged(this, e);
            rbLog10Value_CheckedChanged(this, e);
            rbPower_CheckedChanged(this, e);

            //Request access to the IronPython interface.
            RequestIronPythonInterface();

            //ModelProgressDelegate mped = new ModelProgressDelegate(ReportProgress);
            //ipyInterface.ProgressEvent.Handle(mped);

            //Create the delegate that will raise the event that requests model data
            ModelingTabControl.TabPages[2].Paint += new PaintEventHandler(ModelTabEntered);
            ModelingTabControl.TabPages[3].Paint += new PaintEventHandler(DiagnosticTabEntered);
            this.dsControl1.NotifiableChangeEvent += new EventHandler(this.UpdateData);
        }


        public void Clear()
        {
            dsControl1.Clear();

            if (correlationData != null) { correlationData.Clear(); }
            if (_dtFull != null) { _dtFull.Clear(); }
            if (dt != null) { dt.Clear(); }

            lbAvailableVariables.Items.Clear();
            lbIndVariables.Items.Clear();

            ClearModelingTab();
        }


        public bool AnticipatingModel
        {
            get { return bAnticipatingModel; }
        }


        //Return the IronPython model object
        [JsonProperty]
        public RModelInterface Model
        {
            get { return this.model; }
        }


        //Gets or Sets the method to use to make the model.
        [JsonProperty]
        public string Method
        {
            get { return strMethod; }
            set { strMethod = value; }
        }


        //Get the list of the number of true positives
        [JsonProperty]
        public List<double> TruePositives
        {
            get { return this.listTruePos; }
        }


        //Get the list of the number of true negatives
        [JsonProperty]
        public List<double> TrueNegatives
        {
            get { return this.listTrueNeg; }
        }


        //Get the list of the number of false positives
        [JsonProperty]
        public List<double> FalsePositives
        {
            get { return this.listFalsePos; }
        }


        //Get the list of the number of false negatives
        [JsonProperty]
        public List<double> FalseNegatives
        {
            get { return this.listFalseNeg; }
        }
        

        //Get the list of observed specificities in cross-validation
        [JsonProperty]
        public List<double> ValidationSpecificities
        {
            get { return this.listValidationSpecificity; }
        }


        //Get the list of unique specificities for setting the model's decision threshold 
        [JsonProperty]
        public List<double> CandidateSpecificities
        {
            get { return this.listCandidateSpecificity; }
        }


        //Get the list of possible threshold values
        [JsonProperty]
        public List<double> Thresholds
        {
            get { return this.listCandidateThreshold; }
        }


        //Get the list of possible threshold values
        [JsonProperty]
        public int ThresholdingIndex
        {
            get { return this.intThresholdIndex; }
        }


        //Get the regulatory threshold
        [JsonProperty]
        public string RegulatoryThreshold
        {
            get { return this.tbThreshold.Text; }
        }


        //Get the exponent for a power transform 
        [JsonProperty]
        public string Exponent
        {
            get { return this.tbExponent.Text; }
        }


        //Get the regulatory threshold
        [JsonProperty]
        public string DecisionThreshold
        {
            get { return this.lblDecisionThreshold.Text; }
        }

        //get list of predictors for prediction tab
        [JsonProperty]
        public List<ListItem> ListPredictors
        {
            get { return this.listPredictors; }
        }


        //Return a flag indicating whether this modeling tab has been touched.
        public bool VirginState
        {
            get { return this.boolVirgin; }
        }

        
        //Return a flag indicating whether this has been modified since the model was last exported. (false : modified)
        public bool Clean
        {
            get { return this.boolClean; }
        }


        //Return a flag indicating whether the plugin is ready to export a completed model
        public bool Complete
        {
            get { return this.boolComplete; }
        }


        //Return a flag indicating whether this modeling tab has been touched.
        public bool ThresholdingButtonsVisible
        {
            get { return this.pnlThresholdingButtons.Visible; }
        }


        /*//Get the transform of the dependent variable ..commented out when changing Transform to Dictionary
        public Dictionary<string,object> DependentVariableTransform    
        {
            get { return dictTransform; }
        }*/


        //correlation datatable
        [JsonProperty]
        public DataTable CorrelationDataTable
        {
            get { return correlationData; }
            set { correlationData = value; }
        }


        public TabControl ModelingTabControl
        {
            get { return tabControl1; }
        }


        //set the model with incoming datatable information
        public void SetModelData()
        {
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;

            //Datasheet's packed state coming in
            DataTable dtCorr = dsControl1.FilteredDataTable;
            DataView dvCorr = dtCorr.DefaultView;

            List<string> list = new List<string>();

            list.Add(dtCorr.Columns[0].ColumnName);
            list.Add(dsControl1.ResponseVarColName);

            int intNumVars = lbIndVariables.Items.Count;
            for (int i = 0; i < intNumVars; i++)
                list.Add(lbIndVariables.Items[i].ToString());

            DataTable data = this.tblModelData = dvCorr.ToTable("ModelData", false, list.ToArray());

            int intI = 0;
            bool boolFinished = false;
            while (!boolFinished)
            {
                //look at extendedProperties to determine which tranform button to check
                if (data.Columns[intI].ExtendedProperties.ContainsKey("dependentvariable"))
                {
                    if (data.Columns[intI].ExtendedProperties.ContainsKey("responsevardefinedtransform"))
                    {
                        //Unpack the user's selected transformation of the dependent variable.
                        xfrmImported = (DependentVariableTransforms)Enum.Parse(typeof(DependentVariableTransforms), data.Columns[intI].ExtendedProperties["responsevardefinedtransform"].ToString());
                        if (xfrmImported == DependentVariableTransforms.Power)
                            dblImportedPowerTransformExponent = dsControl1.PowerTransformExponent;

                        this.dblMandateThreshold = VBCommon.Transforms.Apply.UntransformThreshold(Convert.ToDouble(tbThreshold.Text), xfrmThreshold, dblThresholdPowerTransformExponent);
                        this.dblMandateThreshold = VBCommon.Transforms.Apply.TransformThreshold(dblMandateThreshold, xfrmImported, dblImportedPowerTransformExponent);

                        boolFinished = true;
                    }
                }
                //Have we checked all of the columns?
                intI++;
                if (data.Columns.Count == intI)
                    boolFinished = true;
            }
        }


        //Clear the control
        public void ClearModelingTab(bool PreserveVariables=false)
        {
            model = null;
            modelstring = null;

            chartValidation.Series["True positives"].Points.Clear();
            chartValidation.Series["True negatives"].Points.Clear();
            chartValidation.Annotations.Clear();
            chartValidation.Update();

            listValidationSpecificity.Clear();
            listValidationThreshold.Clear();
            listTruePos.Clear();
            listTrueNeg.Clear();
            listFalsePos.Clear();
            listFalseNeg.Clear();

            listCandidateSpecificity = new List<double>();
            listCandidateThreshold = new List<double>();
                        
            lvValidation.Items.Clear();
            if (!PreserveVariables)
                lvModel.Items.Clear();

            //tbThreshold.Text = "235";
            lblDecisionThreshold.Text = "";
            lblSpec.Visible = false;
            lblValidationTitle.Visible = false;

            pnlThresholdingButtons.Visible = false;
            boolComplete = false;
        }


        //Raise a request for access to the IronPython interface - should be raised when the control is created.
        protected void RequestIronPythonInterface()
        {
            if (IronPythonInterfaceRequested != null)
            {
                EventArgs e = new EventArgs();
                IronPythonInterfaceRequested(this, e);
            }
        }


        //Enable or disable controls, then raise an event to do the same up the chain in the containing Form.
        protected void ChangeControlStatus(bool enable)
        {
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;

            boolControlStatus = enable;

            rbLog10.Invoke((MethodInvoker)delegate
            {
                rbLog10.Enabled = enable;
            });

            rbLoge.Invoke((MethodInvoker)delegate
            {
                rbLoge.Enabled = enable;
            });

            rbValue.Invoke((MethodInvoker)delegate
            {
                rbValue.Enabled = enable;
            });

            rbPower.Invoke((MethodInvoker)delegate
            {
                rbPower.Enabled = enable;
            });

            tbThreshold.Invoke((MethodInvoker)delegate
            {
                tbThreshold.Enabled = enable;
            });

            tbExponent.Invoke((MethodInvoker)delegate
            {
                tbExponent.Enabled = enable;
            });

            System.Windows.Forms.Cursor.Current = Cursors.Default;
        }


        //Set column header names in Variable Selection listbox
        public void SetData(IDictionary<string, object> packedState)
        {
            dsControl1.UnpackState((IDictionary<string, object>)packedState["PackedDatasheetState"]); //((IDictionary<string, object>)dictPackedPlugin["PackedDatasheetState"]);
            
            dt = dsControl1.DT;
            this.correlationData = dsControl1.DT;
            _dtFull = dt;
            if (_dtFull == null) return;
            
            tabControl1.SelectedIndex = 0;
            int intDepVarIndx = 1;

            List<string> lstFieldList = new List<string>();
            for (int i = 1; i < _dtFull.Columns.Count; i++)
            {
                bool bDependentVariableColumn = false;

                if (_dtFull.Columns[i].ExtendedProperties.ContainsKey(VBCommon.Globals.DEPENDENTVAR))
                {
                    if (_dtFull.Columns[i].ExtendedProperties[VBCommon.Globals.DEPENDENTVAR].ToString() == "True")
                    {
                        bDependentVariableColumn = true;
                        intDepVarIndx = i;
                    }
                }

                if (!bDependentVariableColumn && _dtFull.Columns[i].ExtendedProperties.ContainsKey(VBCommon.Globals.ENABLED))
                {
                    if (_dtFull.Columns[i].ExtendedProperties[VBCommon.Globals.ENABLED].ToString() == "True")
                        lstFieldList.Add(_dtFull.Columns[i].ColumnName);
                }
                else if (!bDependentVariableColumn)
                    lstFieldList.Add(_dtFull.Columns[i].ColumnName);
            }

            intNumObs = _dtFull.Rows.Count;
            lblNumObs.Text = "Number of Observations: " + intNumObs.ToString();

            lbAvailableVariables.Items.Clear();
            lbIndVariables.Items.Clear();

            for (int i = 0; i < lstFieldList.Count; i++)
            {
                ListItem li = new ListItem(lstFieldList[i], i.ToString());

                if (listPredictors.Any(l => l.DisplayItem == li.DisplayItem))
                    lbIndVariables.Items.Add(li);
                else
                    lbAvailableVariables.Items.Add(li);
            }

            lblAvailVars.Text = "(" + lbAvailableVariables.Items.Count.ToString() + ")";
            lblDepVars.Text = "(" + lbIndVariables.Items.Count.ToString() + ")";
            lbDepVarName.Text = dsControl1.ResponseVarColName;
        }


        //Set column header names in Variable Selection listbox
        public void UpdateData(object source, EventArgs e)
        {
            if (!bAllowUpdateData)
                return;

            boolClean = false;
            boolComplete = false;
            boolVirgin = false;

            dt = dsControl1.DT;
            this.correlationData = dsControl1.DT;
            _dtFull = dt;
            if (_dtFull == null) return;

            tabControl1.SelectedIndex = 0;
            int intDepVarIndx = 1;

            List<string> lstFieldList = new List<string>();
            for (int i = 1; i < _dtFull.Columns.Count; i++)
            {
                bool bDependentVariableColumn = false;

                if (_dtFull.Columns[i].ExtendedProperties.ContainsKey(VBCommon.Globals.DEPENDENTVAR))
                {
                    if (_dtFull.Columns[i].ExtendedProperties[VBCommon.Globals.DEPENDENTVAR].ToString() == "True")
                    {
                        bDependentVariableColumn = true;
                        intDepVarIndx = i;
                    }
                }

                if (!bDependentVariableColumn && _dtFull.Columns[i].ExtendedProperties.ContainsKey(VBCommon.Globals.ENABLED))
                {
                    if (!_dtFull.Columns[i].ExtendedProperties.ContainsKey(VBCommon.Globals.HIDDEN))
                    {
                        if (_dtFull.Columns[i].ExtendedProperties[VBCommon.Globals.ENABLED].ToString() == "True")
                            lstFieldList.Add(_dtFull.Columns[i].ColumnName);
                    }
                    else
                    {
                        if (_dtFull.Columns[i].ExtendedProperties[VBCommon.Globals.HIDDEN].ToString() == "False")
                            lstFieldList.Add(_dtFull.Columns[i].ColumnName);
                    }
                }
                else { }
            }

            intNumObs = dsControl1.DTRI.DTRowInfo.Count(x => x.Value == true); //_dtFull.Rows.Count;
            lblNumObs.Text = "Number of Observations: " + intNumObs.ToString();

            lbAvailableVariables.Items.Clear();
            lbIndVariables.Items.Clear();

            for (int i = 0; i < lstFieldList.Count; i++)
            {
                ListItem li = new ListItem(lstFieldList[i], i.ToString());
                lbAvailableVariables.Items.Add(li);
            }

            lblAvailVars.Text = "(" + lbAvailableVariables.Items.Count.ToString() + ")";
            lblDepVars.Text = "(" + lbIndVariables.Items.Count.ToString() + ")";
            lbDepVarName.Text = dsControl1.ResponseVarColName;

            ClearModelingTab();
            RaiseUpdateNotification();            
        }


        private void btnAddInputVariable_Click(object sender, EventArgs e)
        {
            List<ListItem> items = new List<ListItem>();
            int intSelectedIndices = lbAvailableVariables.SelectedIndices.Count;
            
            //Make a list of the variables we're adding to the model.
            for (int i = 0; i < intSelectedIndices; i++)
            {
                ListItem li = (ListItem)lbAvailableVariables.Items[lbAvailableVariables.SelectedIndices[i]];
                items.Add(li);
            }
            
            //Move the variables from the "Available" box to the "Added" box
            foreach (ListItem li in items)
            {
                lbAvailableVariables.Items.Remove(li);
                lbIndVariables.Items.Add(li);
            }
            
            lblAvailVars.Text = "(" + lbAvailableVariables.Items.Count.ToString() + ")";
            lblDepVars.Text = "(" + lbIndVariables.Items.Count.ToString() + ")";

            ClearModelingTab();
            RaiseUpdateNotification();
        }


        private void btnRemoveInputVariable_Click(object sender, EventArgs e)
        {
            List<ListItem> items = new List<ListItem>();

            //Make a list of the variables we're removing from the model.
            for (int i = 0; i < lbIndVariables.SelectedIndices.Count; i++)
            {
                ListItem li = (ListItem)lbIndVariables.Items[lbIndVariables.SelectedIndices[i]];
                items.Add(li);
            }

            //Move the variables from the "Added" box to the "Available" box
            foreach (ListItem li in items)
            {
                lbIndVariables.Items.Remove(li);

                bool boolFoundIdx = false;
                int intJ = 0;
                for (intJ = 0; intJ < lbAvailableVariables.Items.Count; intJ++)
                {
                    ListItem li2 = (ListItem)lbAvailableVariables.Items[intJ];
                    if (Convert.ToInt32(li2.ValueItem) > Convert.ToInt32(li.ValueItem))
                    {
                        lbAvailableVariables.Items.Insert(intJ, li);
                        boolFoundIdx = true;
                        break;
                    }
                }

                if (boolFoundIdx == false)
                    lbAvailableVariables.Items.Insert(intJ, li);
            }

            lblAvailVars.Text = "(" + lbAvailableVariables.Items.Count.ToString() + ")";
            lblDepVars.Text = "(" + lbIndVariables.Items.Count.ToString() + ")";

            ClearModelingTab();
            RaiseUpdateNotification();
        }


        public void btnRemoveInputVariablesFromModelingTab_Click(object sender, EventArgs e)
        {
            List<ListViewItem> items = new List<ListViewItem>();
            List<ListItem> candidates;

            //Make a list of the variables we're removing from the model.
            for (int i = 0; i < lvModel.SelectedIndices.Count; i++)
            {
                ListViewItem lviDrop = (ListViewItem)lvModel.Items[lvModel.SelectedIndices[i]];
                items.Add(lviDrop);
            }

            //Move the variables from the "Added" box to the "Available" box
            foreach (ListViewItem lvi in items)
            {
                string search = lvi.Text;
                candidates = lbIndVariables.Items.Cast<ListItem>().ToList<ListItem>();
                int index = candidates.Select((item, i) => new { Item = item, Index = i }).Single(s => s.Item.DisplayItem == search).Index;

                ListItem li = (ListItem)lbIndVariables.Items[index];                    
                lbIndVariables.Items.Remove(li);

                bool boolFoundIdx = false;
                int intJ = 0;
                for (intJ = 0; intJ < lbAvailableVariables.Items.Count; intJ++)
                {
                    ListItem li2 = (ListItem)lbAvailableVariables.Items[intJ];
                    if (Convert.ToInt32(li2.ValueItem) > Convert.ToInt32(li.ValueItem))
                    {
                        lbAvailableVariables.Items.Insert(intJ, li);
                        boolFoundIdx = true;
                        break;
                    }
                }

                if (boolFoundIdx == false)
                    lbAvailableVariables.Items.Insert(intJ, li);

                lvModel.Items.Remove(lvi);                
            }
            lvModel.Update();

            lblAvailVars.Text = "(" + lbAvailableVariables.Items.Count.ToString() + ")";
            lblDepVars.Text = "(" + lbIndVariables.Items.Count.ToString() + ")";

            ClearModelingTab(PreserveVariables: true);
            RaiseUpdateNotification();
        }


        //Enable or disable the regulatory threshold control, then raise an event to do the same up the chain in the containing Form.
        protected void ChangeThresholdControlStatus(bool enable)
        {
            tbThreshold.Invoke((MethodInvoker)delegate {tbThreshold.Enabled = enable;});
        }


        //This button runs the modeling method associated with this pane.
        public bool btnRun_Click(object sender, EventArgs e)
        {
            //Make sure that there is at least one exceedance and one non-exceedance:
            List<double> lstOutput = new List<double>();
            foreach (DataRow row in tblModelData.Rows) { lstOutput.Add(Convert.ToDouble(row[dsControl1.ResponseVarColName])); }
            int intExceedances = lstOutput.Count(y => y > dblMandateThreshold);
            if (intExceedances == 0)
            {
                MessageBox.Show("There are no exceedances in the training data! Did you forget to tell me about a transformation?");
                return false;
            }

            int intNonExceedances = lstOutput.Count(y => y <= dblMandateThreshold);
            if (intNonExceedances == 0)
            {
                MessageBox.Show("There are no non-exceedances in the training data! Did you forget to tell me about a transformation?");
                return false;
            }

            ClearModelingTab();
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;

            VBLogger.GetLogger().LogEvent("5", Globals.messageIntent.UserOnly, Globals.targetSStrip.ProgressBar);

            //Now this modeling tab has been touched.
            boolVirgin = false;
            boolRunning = true;

            SetRunButtonStatus(to: false);
            //if (ipyInterface == null) RequestIronPythonInterface();

            VBLogger.GetLogger().LogEvent("10", Globals.messageIntent.UserOnly, Globals.targetSStrip.ProgressBar);

            boolInitialControlStatus = boolControlStatus;
            ChangeControlStatus(false);
            SetModelData();
            VBLogger.GetLogger().LogEvent("15", Globals.messageIntent.UserOnly, Globals.targetSStrip.ProgressBar);

            SetProgressRange(Low: 15, High: 90);
            MakeModel(tblModelData);

            return true;
        }


        //Run the model-building process.
        //This is the callback function that Virtual Beach will use to run the modeling process.
        protected void MakeModel(DataTable Data)
        {
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;

            //Set up the local variables we'll need for model-building.
            DataTable tblData = Data;
            double dblSpecificity = 0.9;
            string strTarget = dsControl1.ResponseVarColName; //tblData.Columns[1].Caption;
            double dblThreshold = dblMandateThreshold;

            //Remove the ID field:
            tblData.Columns.Remove(tblData.Columns[0].Caption);

            //Run the IronPython model-building code, then call PopulateResults to display the coefficients and the decision threshold.
            ModelValidationCompleteDelegate callbackDelegate = new ModelValidationCompleteDelegate(ModelingComplete);
            ModelCancelledDelegate cancellationDelegate = new ModelCancelledDelegate(CancellationCallback);
            ModelProgressDelegate progressDelegate = new ModelProgressDelegate(ReportProgress);

            //ipyInterface.Validate(tblData, strTarget, dblThreshold, dblSpecificity, method: strMethod, callback: callbackDelegate, cancellationCallback:cancellationDelegate);
            bAnticipatingModel = true; 
            Controller.Validate(Data: tblData, Target: strTarget, Threshold: dblThreshold, Specificity: dblSpecificity, Folds: 5, CompletionCallback: callbackDelegate, CancellationCallback: cancellationDelegate, ProgressCallback: progressDelegate);

            //bAnticipatingModel = true;
        }


        protected void ModelingComplete(ValidationAndModel Result)
        {
            if (AnticipatingModel)
            {
                bAnticipatingModel = false;
                this.model = Result.Model;
                UpdateStoredModelString();
                PopulateResults(this.model);

                //Now extract the valid thresholds and the corresponding specificities
                model.SetThreshold(0.9);
                double dblSpecificity = model.Specificity;
                Dictionary<string, List<double>> thresholding = utils.GetPossibleSpecificities(model);
                this.listCandidateThreshold = thresholding["thresholds"];
                this.listCandidateSpecificity = thresholding["specificities"];

                //The following is a bit of python code to initialize the decision threshold at the default specificity.
                this.intThresholdIndex = thresholding["thresholds"].IndexOf(model.Threshold);

                //Extract the number of false positives, true positives, false negatives, and true negatives.
                Dictionary<string, List<double>> counts = utils.SpecificityChart(Result);
                listValidationSpecificity = counts["spec"];
                listValidationThreshold = counts["thresh"];
                listTruePos = counts["tpos"];
                listTrueNeg = counts["tneg"];
                listFalsePos = counts["fpos"];
                listFalseNeg = counts["fneg"];
                
                //Put the results on the GUI
                InitializeValidationChart();
                UpdateDiagnostics();
                AnnotateChart();

                //Enable the thresholding controls and the model-selection button.
                pnlThresholdingButtons.Visible = true;
                ChangeControlStatus(boolInitialControlStatus);
                ChangeThresholdControlStatus(true);

                //Work's done; let's go home.
                boolComplete = true;
                RaiseUpdateNotification();

                VBLogger.GetLogger().LogEvent("90", Globals.messageIntent.UserOnly, Globals.targetSStrip.ProgressBar);
                System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;
                Application.DoEvents();


                VBLogger.GetLogger().LogEvent("95", Globals.messageIntent.UserOnly, Globals.targetSStrip.ProgressBar);

                boolRunning = false;
                SetRunButtonStatus(to: true);
                VBLogger.GetLogger().LogEvent("Model building complete on the " + strMethod + " plugin.");
                VBLogger.GetLogger().LogEvent("100", Globals.messageIntent.UserOnly, Globals.targetSStrip.ProgressBar);
                System.Windows.Forms.Cursor.Current = Cursors.Default;

                if (ModelingCompleteEvent != null)
                {
                    EventArgs args = new EventArgs();
                    ModelingCompleteEvent(this, args);
                }
            }
            return;
        }


        private void UpdateStoredModelString()
        {
            //Pack up the model's state in two ways: one with the model represented by a dynamic object, the other with the model represented by a string
            modelstring = utils.SerializeRObject(model.RModelObject);
        }


        public void btnCancel_Click(object sender, EventArgs e)
        {
            bAnticipatingModel = false;
            boolComplete = false;

            if (ModelingCanceledEvent != null)
            {
                EventArgs args = new EventArgs();
                ModelingCanceledEvent(this, args);
            }
                        
            Controller.CancelModel();
            VBLogger.GetLogger().LogEvent("Model building was cancelled on the " + strMethod + " plugin.");
            VBLogger.GetLogger().LogEvent("100", Globals.messageIntent.UserOnly, Globals.targetSStrip.ProgressBar);
            ChangeControlStatus(true);
            System.Windows.Forms.Cursor.Current = Cursors.Default;
        }


        public void CancellationCallback(string Message)
        {
            System.Windows.Forms.MessageBox.Show(Message);
        }


        protected void InitializeValidationChart()
        {
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;

            //Get the limits of the thresholding chart:
            double dblMinX = listCandidateThreshold != null ? listCandidateThreshold.Min() : 0;
            dblMinX = listValidationThreshold != null ? Math.Min(dblMinX, listValidationThreshold.Min()) : dblMinX;

            double dblMaxX = listCandidateThreshold != null ? listCandidateThreshold.Max() : 0;
            dblMaxX = listValidationThreshold != null ? Math.Max(dblMinX, listValidationThreshold.Max()) : dblMaxX;

            //Set up the plotting area to show the validation results
            chartValidation.ChartAreas[0].AxisX.Minimum = dblMinX;
            chartValidation.ChartAreas[0].AxisX.Maximum = dblMaxX;

            //Make sure the chart is clear before drawing on it.
            chartValidation.Series["True positives"].Points.Clear();
            chartValidation.Series["True negatives"].Points.Clear();

            //Plot the validation results (true positives and true negatives versus specificity).
            for (int i = 0; i < listValidationSpecificity.Count; i++)
            {
                chartValidation.Series["True positives"].Points.AddXY(xValue: listValidationThreshold[i], yValue: listTruePos[i]);
                chartValidation.Series["True negatives"].Points.AddXY(xValue: listValidationThreshold[i], yValue: listTrueNeg[i]);
            }

            //Finish drawing the chart and then add the thresholding line.
            chartValidation.ChartAreas[0].AxisX.LabelStyle.Format = "0.00";
            chartValidation.Series["True positives"].YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;

            //Label the axes:
            chartValidation.Titles.Clear();
            chartValidation.Titles.Add(new Title("Decision threshold", Docking.Bottom, new Font("Arial", 11), Color.Black));
            chartValidation.Titles.Add(new Title("True positives", Docking.Right, new Font("Arial", 11), Color.Black));
            chartValidation.Titles.Add(new Title("True negatives", Docking.Left, new Font("Arial", 11), Color.Black));
            chartValidation.Update();
        }


        protected virtual void PopulateResults(RModelInterface model) { }


        public void ModelTabEntered(object sender, EventArgs e)
        {
            if (this.model != null)
            {
                //rebuild model                
                InitializeValidationChart();
                AnnotateChart();
            }
        }


        public void DiagnosticTabEntered(object sender, EventArgs e)
        {
            if (this.model != null)
            {
                //rebuild model                
                //UpdateDiagnostics();
            }
        }


        protected void AnnotateChart()
        {
            System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;        
            
            //Set the threshold with the given specificity.
            double dblSpecificity = this.listCandidateSpecificity[this.intThresholdIndex];
            double dblThreshold = this.listCandidateThreshold[this.intThresholdIndex];
            model.SetThreshold(dblSpecificity);
            double untransformedDecisionCriterion = VBCommon.Transforms.Apply.UntransformThreshold(model.Threshold, xfrmImported, dblImportedPowerTransformExponent);
            lblDecisionThreshold.Text = String.Format("{0:F2}", untransformedDecisionCriterion);

            //Locate the specificity annotation
            chartValidation.Update();
            chartValidation.ChartAreas[0].RecalculateAxesScale();
            lblSpec.Text = "Decision Criterion: " + Convert.ToString(Math.Round(value: untransformedDecisionCriterion, digits: 2));
            int intXLoc = (int)chartValidation.ChartAreas[0].AxisX.ValueToPixelPosition(dblThreshold) + chartValidation.Location.X - (int)(lblSpec.Size.Width / 2);
            lblSpec.Location = new Point(x: intXLoc, y: 42);
            lblSpec.Visible = true;
            lblValidationTitle.Visible = true;

            //Format the threshold line and draw it on the chart.
            chartValidation.Annotations.Clear();
            System.Windows.Forms.DataVisualization.Charting.VerticalLineAnnotation myLine = new System.Windows.Forms.DataVisualization.Charting.VerticalLineAnnotation();
            myLine.X = chartValidation.ChartAreas[0].AxisX.ValueToPosition(dblThreshold);
            myLine.AxisY = chartValidation.ChartAreas[0].AxisY;
            double dblYMax = chartValidation.ChartAreas[0].AxisY.Maximum;
            double dblYMin = chartValidation.ChartAreas[0].AxisY.Minimum;
            myLine.Y = dblYMax;
            myLine.Height = chartValidation.ChartAreas[0].AxisY.ValueToPosition(dblYMin) - chartValidation.ChartAreas[0].AxisY.ValueToPosition(dblYMax);
            myLine.Visible = true;
            myLine.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dash;
            chartValidation.Annotations.Add(myLine);
            chartValidation.Update();

            //Summarize the model's performance in the validation ListView.
            List<double> lstDblCandidates = listValidationSpecificity.Where(arg => arg <= dblSpecificity).ToList();

            //If there are no predictions below the candidate threshold, then just use the lowest valid threshold:
            int intIndex = 0;
            if(lstDblCandidates.Count != 0)
                intIndex = listValidationSpecificity.FindIndex(arg => arg == lstDblCandidates.Max());
            string[] strArrValidation = new string[7] { listTruePos[intIndex].ToString(), listTrueNeg[intIndex].ToString(), listFalsePos[intIndex].ToString(), listFalseNeg[intIndex].ToString(), ((double)listTruePos[intIndex] / (double)(listTruePos[intIndex] + listFalseNeg[intIndex])).ToString("0.00"), ((double)listTrueNeg[intIndex] / (double)(listTrueNeg[intIndex] + listFalsePos[intIndex])).ToString("0.00"), ((double)(listTruePos[intIndex] + listTrueNeg[intIndex]) / (double)(listTruePos[intIndex] + listFalseNeg[intIndex] + listTrueNeg[intIndex] + listFalsePos[intIndex])).ToString("0.00") };

            //Add the row to the listview, coloring it red if this variable is considered to have minor influence.
            ListViewItem lvi = new ListViewItem(strArrValidation);
            lvValidation.Items.Clear();
            lvValidation.Items.Add(lvi);

            System.Windows.Forms.Cursor.Current = Cursors.Default;
            UpdateDiagnostics();
        }
               
        
        //Pack State for Serializing
        public IDictionary<string, object> PackState()
        {
            if (dsControl1.DT == null)
                return null;

            IDictionary<string, object> dictPluginState = new Dictionary<string, object>();

            //Needed by the prediction plugin: 
            //if changing a saved project need to keep the listPredictions the same, because lbIndVariables aren't saved            
            listPredictors = new List<ListItem>();
            if (lbIndVariables.Items.Count > 0)
            {
                foreach (ListItem item in lbIndVariables.Items)
                    listPredictors.Add(item);
            }

            dictPluginState.Add("PackedDatasheetState", dsControl1.PackState());
            dictPluginState.Add("Predictors", listPredictors);

            //Save the transforms.
            dictPluginState.Add("xfrmImported", xfrmImported);
            dictPluginState.Add("xfrmThreshold", xfrmThreshold);
            dictPluginState.Add("ImportedPowerTransformExponent", dblImportedPowerTransformExponent);
            dictPluginState.Add("ThresholdPowerTransformExponent", dblThresholdPowerTransformExponent);

            //Save the state of UI elements.
            dictPluginState.Add("VirginState", this.boolVirgin);
            dictPluginState.Add("ThresholdingButtonsVisible", true);
            dictPluginState.Add("ActiveTab", tabControl1.SelectedIndex);
            dictPluginState.Add("Method", strMethod);
            dictPluginState.Add("SeedCheckboxState", cbRandomSeed.Checked);
            dictPluginState.Add("RandomSeed", tbRandomSeed.Text);

            //Store the regulatory threshold
            double dblRegulatoryThreshold;
            try { dblRegulatoryThreshold = Convert.ToDouble(RegulatoryThreshold); }
            catch (InvalidCastException) { dblRegulatoryThreshold = -1; }

            //if just adding to stack, go to different pack to be used in setData()
            if (Model == null)
                return dictPluginState;

            //Store the decision threshold.
            double dblDecisionThreshold;
            try { dblDecisionThreshold = Convert.ToDouble(DecisionThreshold); }
            catch (InvalidCastException) { dblDecisionThreshold = -1; }

            Dictionary<string, object> dictModelState = new Dictionary<string, object>();
            string strScratchDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VirtualBeach");
            dictModelState.Add("ModelObject", model.PackState(modelstring));
            dictModelState.Add("ImportedTransform", xfrmImported);
            dictModelState.Add("ImportedTransformExponent", dblImportedPowerTransformExponent);
            dictModelState.Add("ThresholdTransform", xfrmThreshold);
            dictModelState.Add("ThresholdTransformExponent", dblThresholdPowerTransformExponent);
            dictModelState.Add("RegulatoryThreshold", dblRegulatoryThreshold);
            dictModelState.Add("DecisionThreshold", dblDecisionThreshold);
            dictModelState.Add("Method", strMethod);
            dictPluginState.Add("Model", dictModelState);
            
            //Save the lists that we use to make the validation chart
            Dictionary<string, List<double>> dictValidation = new SerializableDictionary<string, List<double>>();
            dictValidation.Add("tpos", this.TruePositives);
            dictValidation.Add("tneg", this.TrueNegatives);
            dictValidation.Add("fpos", this.FalsePositives);
            dictValidation.Add("fneg", this.FalseNegatives);
            dictValidation.Add("specificity", listValidationSpecificity);
            dictValidation.Add("thresholds", listValidationThreshold);

            dictPluginState.Add("ValidationDictionary", dictValidation);

            //Save the lists that we use to set the model's decision threshold.
            Dictionary<string, List<double>> dictThresholding = new Dictionary<string, List<double>>();
            dictThresholding.Add("specificity", this.listCandidateSpecificity);
            dictThresholding.Add("threshold", this.listCandidateThreshold);
            dictPluginState.Add("ThresholdingDictionary", dictThresholding);
            dictPluginState.Add("ThresholdingIndex", this.intThresholdIndex);

            dictPackedState = dictPluginState;
            return dictPluginState;
        }

        
        //Reconstruct the saved modeling state - without the IronPython Model
        public void UnpackState(IDictionary<string, object> dictProjectState)
        {
            if (dictProjectState.Count <= 3) return; //if only plugin props are here

            //unpack the model's datasheet
            listPredictors = (List<ListItem>)dictProjectState["Predictors"];
            bAllowUpdateData = false;
            this.SetData(dictProjectState);
            bAllowUpdateData = true;

            boolComplete = (bool)dictProjectState["Complete"];
            if (boolComplete)
            {
                //Unpack the lists that go into making the validation chart.
                Dictionary<string, List<double>> dictValidation = (Dictionary<string, List<double>>)dictProjectState["ValidationDictionary"];
                this.listValidationSpecificity = dictValidation["specificity"];
                this.listValidationThreshold = dictValidation["thresholds"];
                this.listTruePos = dictValidation["tpos"];
                this.listTrueNeg = dictValidation["tneg"];
                this.listFalsePos = dictValidation["fpos"];
                this.listFalseNeg = dictValidation["fneg"];

                //Unpack the lists that are used to set the model's decision threshold                
                Dictionary<string, List<double>> dictThresholding = (Dictionary<string, List<double>>)dictProjectState["ThresholdingDictionary"];
                this.listCandidateSpecificity = dictThresholding["specificity"];
                this.listCandidateThreshold = dictThresholding["threshold"];
                this.intThresholdIndex = (int)dictProjectState["ThresholdingIndex"];

                //Unpack the random seed
                if (dictProjectState.ContainsKey("RandomSeed") && dictProjectState.ContainsKey("SeedCheckboxState"))
                {
                    cbRandomSeed.Checked = (bool)(dictProjectState["SeedCheckboxState"]);
                    tbRandomSeed.Text = dictProjectState["RandomSeed"].ToString();
                    cbRandomSeed_CheckedChanged(null, new EventArgs());
                }
                else
                {
                    cbRandomSeed.Checked = false;
                    tbRandomSeed.Text = "";
                }

                //ModelState modelState = (ModelState)dictProjectState["Model"];
                Dictionary<string, object> dictModel = (Dictionary<string, object>)dictProjectState["Model"];
                if (dictModel.ContainsKey("ModelObject"))
                {
                    Type objType = typeof(Dictionary<string, object>);
                    string jsonRep = dictModel["ModelObject"].ToString();

                    object objModel = JsonConvert.DeserializeObject(jsonRep, objType);
                    dictModel["ModelObject"] = (IDictionary<string, object>)objModel;
                    this.model = Controller.Deserialize(dictModel["ModelObject"]);
                }               
                this.Method = (string)dictModel["Method"];

                //The chart's properties don't exist until it's drawn. It's not drawn until its tab comes to the front.
                //So we have to activate the modeling tab before creating the thresholding plot.
                //tabControl1.SelectedTab = ModelingTab;
                PopulateResults(this.model);
                UpdateDiagnostics();
                
                //Unpack the contents of the threshold and exponent text boxes
                xfrmImported = (DependentVariableTransforms)dictProjectState["xfrmImported"];
                xfrmThreshold = (DependentVariableTransforms)dictProjectState["xfrmThreshold"];
                dblImportedPowerTransformExponent = Convert.ToDouble(dictProjectState["ImportedPowerTransformExponent"]);
                dblThresholdPowerTransformExponent = Convert.ToDouble(dictProjectState["ThresholdPowerTransformExponent"]);

                this.tbExponent.Text = dblThresholdPowerTransformExponent.ToString();
                this.tbThreshold.Text = ((double)dictModel["RegulatoryThreshold"]).ToString();
                this.lblDecisionThreshold.Text = ((double)dictModel["DecisionThreshold"]).ToString();

                //Unpack the user's selected transformation of the dependent variable.
                if (xfrmThreshold == DependentVariableTransforms.none)
                    this.rbValue.Checked = true;
                else if (xfrmThreshold == DependentVariableTransforms.Ln)
                    this.rbLoge.Checked = true;
                else if (xfrmThreshold == DependentVariableTransforms.Log10)
                    this.rbLog10.Checked = true;
                else if (xfrmThreshold == DependentVariableTransforms.Power)
                    this.rbPower.Checked = true;
                else
                    this.rbValue.Checked = true;


                //Now make sure the selected transformation is reflected behind the scenes, too.
                EventArgs e = new EventArgs();
                rbValue_CheckedChanged(this, e);
                rbLogeValue_CheckedChanged(this, e);
                rbLog10Value_CheckedChanged(this, e);
                rbPower_CheckedChanged(this, e);

                //Now restore the elements of the user interface.
                this.pnlThresholdingButtons.Visible = (bool)dictProjectState["ThresholdingButtonsVisible"];
            }
            else { ClearModelingTab(); }

            //Now set the selected tab to whatever was on top when the project was saved.
            tabControl1.SelectedIndex = (int)(dictProjectState["ActiveTab"]); //model
        }


        protected void btnLeft25_Click(object sender, EventArgs e)
        { 
            if (this.intThresholdIndex > 26)
                this.intThresholdIndex -= 25;
            else
                this.intThresholdIndex = 1;

            AnnotateChart();
            //UpdateDiagnostics();
            RaiseUpdateNotification();
        }
        

        protected void btnLeft1_Click(object sender, EventArgs e)
        {
            if (this.intThresholdIndex > 1)
                this.intThresholdIndex -= 1;
            else
                this.intThresholdIndex = 1;

            AnnotateChart();
            //UpdateDiagnostics();
            RaiseUpdateNotification();
        }


        protected void btnRight1_Click(object sender, EventArgs e)
        {
            if (this.intThresholdIndex < this.listCandidateThreshold.Count - 1)
                this.intThresholdIndex += 1;
            else
                this.intThresholdIndex = this.listCandidateThreshold.Count - 1;

            AnnotateChart();
            //UpdateDiagnostics();
            RaiseUpdateNotification();
        }


        protected void btnRight25_Click(object sender, EventArgs e)
        {
            if (this.intThresholdIndex < this.listCandidateThreshold.Count - 25)
                this.intThresholdIndex += 25;
            else
                this.intThresholdIndex = this.listCandidateThreshold.Count - 1;

            AnnotateChart();
            //UpdateDiagnostics();
            RaiseUpdateNotification();
        }


        protected void rbValue_CheckedChanged(object sender, EventArgs e)
        {
            if (rbValue.Checked)
            {
                double tv = double.NaN;

                try
                {
                    tv = Convert.ToDouble(tbThreshold.Text.ToString());
                }
                catch
                {
                    string msg = @"Cannot convert threshold. (" + tbThreshold.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }

                dblMandateThreshold = Apply.TransformThreshold(tv, xfrmImported, dblImportedPowerTransformExponent);
                xfrmThreshold = DependentVariableTransforms.none;
            }
            boolClean = false;
        }


        protected void rbLog10Value_CheckedChanged(object sender, EventArgs e)
        {
            if (rbLog10.Checked)
            {
                double tv = double.NaN;

                try
                {
                    tv = Apply.UntransformThreshold(Convert.ToDouble(tbThreshold.Text.ToString()), DependentVariableTransforms.Log10);
                }
                catch
                {
                    string msg = @"Cannot invert Log 10 transform for threshold. (" + tbThreshold.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }

                if (tv.Equals(double.NaN))
                {
                    string msg = @"Error while untransforming the threshold. (" + tbThreshold.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }

                /*if (tv < 0)
                {
                    string msg = @"Entered value must be greater than 0. (" + tbThreshold.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }*/

                dblMandateThreshold = Apply.TransformThreshold(tv, xfrmImported, dblImportedPowerTransformExponent);
                xfrmThreshold = DependentVariableTransforms.Log10;
            }
            boolClean = false;
        }


        protected void rbLogeValue_CheckedChanged(object sender, EventArgs e)
        {
            if (rbLoge.Checked)
            {
                double tv = double.NaN;

                try
                {
                    tv = Apply.UntransformThreshold(Convert.ToDouble(tbThreshold.Text.ToString()), DependentVariableTransforms.Ln);
                }
                catch
                {
                    string msg = @"Cannot invert Log e transform of threshold. (" + tbThreshold.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }

                if (tv.Equals(double.NaN))
                {
                    string msg = @"Error untransforming the threshold. (" + tbThreshold.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }

                /*if (tv < 0)
                {
                    string msg = @"Entered value must be greater than 0. (" + tbThreshold.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }*/

                dblMandateThreshold = Apply.TransformThreshold(tv, xfrmImported, dblImportedPowerTransformExponent);
                xfrmThreshold = DependentVariableTransforms.Ln;
            }
            boolClean = false;
        }


        protected void rbPower_CheckedChanged(object sender, EventArgs e)
        {
            if (rbPower.Checked)
                tbExponent.Enabled = true;
            else
                tbExponent.Enabled = false;

            if (rbPower.Checked)
            {
                double tv = double.NaN;

                try
                {
                    tv = Apply.UntransformThreshold(Convert.ToDouble(tbThreshold.Text.ToString()), DependentVariableTransforms.Power, Convert.ToDouble(tbExponent.Text.ToString()));
                }
                catch
                {
                    string msg = @"Cannot exponentiate threshold. (threshold: " + tbThreshold.Text + ", exponent: " + tbExponent.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }

                if (tv.Equals(double.NaN))
                {
                    string msg = @"Entered value must not be zero. (" + tbThreshold.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }

                if (Convert.ToDouble(tbExponent.Text.ToString()) == 0)
                {
                    string msg = @"Exponent cannot be zero.";
                    MessageBox.Show(msg);
                    return;
                }

                xfrmThreshold = DependentVariableTransforms.Power;
                dblThresholdPowerTransformExponent = Convert.ToDouble(tbExponent.Text);
                dblMandateThreshold = Apply.TransformThreshold(tv, xfrmImported, dblImportedPowerTransformExponent);
            }
            boolClean = false;
        }


        protected void tbExponent_Leave(object sender, EventArgs e)
        {
            double dblExponent;

            if (Double.TryParse(tbExponent.Text, out dblExponent) == false)
            {
                string msg = @"Exponent must be a numeric value.";
                MessageBox.Show(msg);
                tbExponent.Focus();
            }
            else
            {
                dblMandateThreshold = Math.Pow(Convert.ToDouble(tbThreshold.Text.ToString()), 1/dblExponent);
                dblThresholdPowerTransformExponent = dblExponent;
            }
            boolClean = false;
        }


        protected void tbThresholdReg_TextChanged(object sender, EventArgs e)
        {
            if (Double.TryParse(tbThreshold.Text, out dblMandateThreshold) == false)
            {
                string msg = @"Regulatory standard must be a numeric value.";
                MessageBox.Show(msg);
                return;
            }
            boolClean = false;
        }


        //This method is called whenever there is a change to the model or the threshold. It updates the prediction and diagnostic tabs.
        protected void RaiseUpdateNotification()
        {
            if (ModelUpdated != null)
            {
                EventArgs e = new EventArgs();
                ModelUpdated(this, e);
            }
        }


        private void tbThreshold_TextChanged(object sender, EventArgs e)
        {
            double tv = double.NaN;
            if (rbValue.Checked)
            {
                try
                {
                    tv = Convert.ToDouble(tbThreshold.Text.ToString());
                }
                catch
                {
                    string msg = @"Cannot set threshold. (" + tbThreshold.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }
            }
            else if (rbLog10.Checked)
            {
                try
                {
                    tv = Apply.UntransformThreshold(Convert.ToDouble(tbThreshold.Text.ToString()), DependentVariableTransforms.Log10);
                }
                catch
                {
                    string msg = @"Cannot exponentiate threshold. (" + tbThreshold.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }

                if (tv.Equals(double.NaN))
                {
                    string msg = @"Error while untransforming the threshold. (" + tbThreshold.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }

                /*if (tv < 0)
                {
                    string msg = @"Entered value must be greater than 0. (" + tbThreshold.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }*/
            }
            else if (rbLoge.Checked)
            {
                try
                {
                    tv = Apply.UntransformThreshold(Convert.ToDouble(tbThreshold.Text.ToString()), DependentVariableTransforms.Ln);
                }
                catch
                {
                    string msg = @"Cannot exponentiate threshold. (" + tbThreshold.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }

                if (tv.Equals(double.NaN))
                {
                    string msg = @"Error while untransforming the threshold. (" + tbThreshold.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }

                /*if (tv < 0)
                {
                    string msg = @"Entered value must be greater than 0. (" + tbThreshold.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }*/
            }
            else if (rbPower.Checked)
            {
                try
                {
                    tv = Apply.UntransformThreshold(Convert.ToDouble(tbThreshold.Text.ToString()), DependentVariableTransforms.Power, Convert.ToDouble(tbExponent.Text.ToString()));
                }
                catch
                {
                    string msg = @"Cannot take root of threshold. (threshold: " + tbThreshold.Text + ", root: " + tbExponent.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }

                if (tv.Equals(double.NaN))
                {
                    string msg = @"Entered value must be greater than 0. (" + tbThreshold.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }

                if (Convert.ToDouble(tbExponent.Text.ToString()) == 0)
                {
                    string msg = @"Exponent cannot be zero.";
                    MessageBox.Show(msg);
                    return;
                }
            }

            dblMandateThreshold = Apply.TransformThreshold(tv, xfrmImported, dblImportedPowerTransformExponent);
            boolClean = false;
        }


        public List<double> Predict(DataSet dsPredictionData, double RegulatoryThreshold, double DecisionThreshold, VBCommon.Transforms.DependentVariableTransforms ThresholdTransform, double ThresholdPowerExponent)
        {
            try
            {                
                DataTable tblForPrediction = dsPredictionData.Tables["Prediction"];
                return (model.Predict(tblForPrediction));
            }
            catch
            {
                return new List<double>();
            }
        }


        public List<double> PredictExceedanceProbability(DataSet dsPredictionData, double RegulatoryThreshold, double DecisionThreshold, VBCommon.Transforms.DependentVariableTransforms ThresholdTransform, double ThresholdPowerExponent)
        {
            try
            {
                //Set up the received threshold to match the modeled transform.
                double dblRegulatoryThreshold = VBCommon.Transforms.Apply.UntransformThreshold(RegulatoryThreshold, ThresholdTransform, ThresholdPowerExponent);
                dblRegulatoryThreshold = VBCommon.Transforms.Apply.TransformThreshold(dblRegulatoryThreshold, xfrmImported, dblImportedPowerTransformExponent);

                double dblDecisionThreshold = VBCommon.Transforms.Apply.UntransformThreshold(DecisionThreshold, ThresholdTransform, ThresholdPowerExponent);
                dblDecisionThreshold = VBCommon.Transforms.Apply.TransformThreshold(dblDecisionThreshold, xfrmImported, dblImportedPowerTransformExponent);

                //Now make predictions
                DataTable tblForPrediction = dsPredictionData.Tables["Prediction"];
                return (model.PredictExceedanceProbability(tblForPrediction, dblDecisionThreshold));
            }
            catch
            {
                return new List<double>();
            }
        }


        public string ModelString()
        {
            try { return (model.GetModelExpression().Replace("[", "(").Replace("]", ")")); }
            catch { return "Error! There's no actual model for this method."; }
        }


        private void SetRunButtonStatus(bool to)
        {
            if (RunButtonStatusChanged != null)
            {
                RunButtonStatusArgs StatusArgs = new RunButtonStatusArgs(status:to);
                RunButtonStatusChanged(this, StatusArgs);
            }
        }


        public void btnComputeAO_Click(object sender, EventArgs e)
        {
            dsControl1.btnComputeAO_Click(sender, e);
        }


        public void btnManipulate_Click(object sender, EventArgs e)
        {
            dsControl1.btnManipulate_Click(sender, e);
        }


        public void btnTransform_Click(object sender, EventArgs e)
        {
            dsControl1.btnTransform_Click(sender, e);
        }


        private ZedGraph.GraphPane TimeSeries(double[] Observed, double[] Fitted, string[] Tags)
        {
            DateTime date;
            bool dateAxis = false;
            if (IsDate(Tags[0], out date)) dateAxis = true;

            ZedGraph.PointPairList pplObserved = new ZedGraph.PointPairList();
            ZedGraph.PointPairList pplFitted = new ZedGraph.PointPairList();
            string tag = string.Empty;
            //might be a smarter way to add tags to points but I can't find it.
            for (int i = 0; i < Observed.Length; i++)
            {
                //tag = "(" + iv[i].ToString("n2") + " , " + tags[i] + ") ";
                tag = Tags[i];
                if (dateAxis)
                {
                    DateTime d = Convert.ToDateTime(Tags[i]);
                    pplObserved.Add((ZedGraph.XDate)d, Observed[i], tag);
                    pplFitted.Add((ZedGraph.XDate)d, Fitted[i], tag);
                }
                else
                {
                    //the following does not work for string IDs...
                    //double n = Convert.ToDouble(Tags[i]);
                    //pplObserved.Add(n, Observed[i], tag);
                    //pplFitted.Add(n, Fitted[i], tag);
                    pplObserved.Add(i, Observed[i], tag);
                    pplFitted.Add(i, Fitted[i], tag);
                }
            }
            ZedGraph.GraphPane gp = new ZedGraph.GraphPane();

            ZedGraph.LineItem curveObserved = gp.AddCurve("YObs", pplObserved, Color.Black, ZedGraph.SymbolType.Square);
            ZedGraph.LineItem curveFitted = gp.AddCurve("YPred", pplFitted, Color.Red, ZedGraph.SymbolType.Triangle);

            curveObserved.Line.IsVisible = true;
            curveFitted.Line.IsVisible = true;
            
            if (dateAxis) gp.XAxis.Type = ZedGraph.AxisType.Date;
            else gp.XAxis.Type = ZedGraph.AxisType.Linear;
            gp.XAxis.Title.Text = dsControl1.DT.Columns[0].ColumnName;
            gp.YAxis.Title.Text = dsControl1.ResponseVarColName;

            gp.Tag = "TSPlot";
            gp.Title.Text = "Time Series Plot";

            return gp;
        }


        private ZedGraph.GraphPane PairPlot(double[] X, double[] Y, string[] Tags, string XLabel, string YLabel, string Title, string Tag, ZedGraph.SymbolType Symbol = ZedGraph.SymbolType.Triangle, Color? SymbolColor = null)
        {
            ZedGraph.PointPairList ppl = new ZedGraph.PointPairList();
            string tag = string.Empty;
            Color color = SymbolColor != null ? (Color)SymbolColor : Color.Black;

            //might be a smarter way to add tags to points but I can't find it.
            for (int i = 0; i < X.Length; i++)
            {
                ppl.Add(x: X[i], y: Y[i], tag: Tags[i]);
            }
            ZedGraph.GraphPane gp = new ZedGraph.GraphPane();
            ZedGraph.LineItem curveResiduals = gp.AddCurve(YLabel, ppl, color, Symbol);
            curveResiduals.Line.IsVisible = false;

            gp.XAxis.Type = ZedGraph.AxisType.Linear;
            gp.XAxis.Title.Text = XLabel;
            gp.YAxis.Title.Text = YLabel;

            gp.Tag = Tag;
            gp.Title.Text = Title;

            return gp;
        }


        private bool IsDate(string anyString, out DateTime resultDate)
        {
            bool isDate = true;

            if (anyString == null)
            {
                anyString = "";
            }
            try
            {
                resultDate = DateTime.Parse(anyString);
            }
            catch
            {
                resultDate = DateTime.MinValue;
                isDate = false;
            }

            return isDate;
        }


        private void UpdateDiagnostics()
        {
            ZedGraph.MasterPane master = zgcDiagnostic.MasterPane;
            master.PaneList.Clear();

            List<double> lstFitted = Model.Fitted;
            List<double> lstObserved = Model.Actual;
            List<double> lstResiduals = Model.Residuals;

            DataRow[] rows = dsControl1.DT.Select();
            string[] tags = rows.Select(x => x[0].ToString()).ToArray();
            zgcDiagnostic.MasterPane.Add(TimeSeries(Observed: lstObserved.ToArray(), Fitted: lstFitted.ToArray(), Tags: tags));
            zgcDiagnostic.MasterPane.Add(PairPlot(X: lstFitted.ToArray(), Y:lstResiduals.ToArray(), Tags: tags, XLabel:"Fitted", YLabel:"Residuals", Title:"Residuals vs. Fitted", Tag:"Resid", Symbol:ZedGraph.SymbolType.Square, SymbolColor:Color.Black));
            zgcDiagnostic.MasterPane.Add(PairPlot(Y: lstResiduals.ToArray(), X: lstObserved.ToArray(), Tags: tags, XLabel: "Observed", YLabel: "Residuals", Title: "Residuals vs. Observed", Tag: "Resid", Symbol: ZedGraph.SymbolType.Triangle, SymbolColor: Color.Red));
            zgcDiagnostic.MasterPane.Add(PairPlot(Y: lstFitted.ToArray(), X: lstObserved.ToArray(), Tags: tags, XLabel: "Observed", YLabel: "Fitted", Title: "Fitted vs. Observed", Tag: "Fit", Symbol: ZedGraph.SymbolType.Circle, SymbolColor: Color.Blue));

            ZedGraph.GraphPane pane = zgcDiagnostic.MasterPane[3];
            ZedGraph.PointPairList regulatory = new ZedGraph.PointPairList();
            regulatory.Add(lstObserved.Min(), Model.Threshold);
            regulatory.Add(lstObserved.Max(), Model.Threshold);
            ZedGraph.LineItem regulatoryCurve = pane.AddCurve("threshold", regulatory, Color.Green, ZedGraph.SymbolType.None);
            regulatoryCurve.Line.IsVisible = true;
            
            ZedGraph.PointPairList threshold = new ZedGraph.PointPairList();
            threshold.Add(Model.Regulatory, lstFitted.Min());
            threshold.Add(Model.Regulatory, lstFitted.Max());
            ZedGraph.LineItem thresholdCurve = pane.AddCurve("threshold", threshold, Color.Blue, ZedGraph.SymbolType.None);
            thresholdCurve.Line.IsVisible = true;

            using (Graphics g = this.CreateGraphics())
            { master.SetLayout(g, ZedGraph.PaneLayout.SquareColPreferred); }

            zgcDiagnostic.IsShowPointValues = true;
            zgcDiagnostic.AxisChange();
            master.AxisChange();
            zgcDiagnostic.Refresh();
        }


        private void SetProgressRange(double Low, double High)
        {
            dblProgressRangeLow = Low;
            dblProgressRangeHigh = High;
        }


        public void ReportProgress(string message, double progress)
        {
            if (AnticipatingModel)
            {
                int modelingProgress;
                modelingProgress = Convert.ToInt32(progress * (dblProgressRangeHigh - dblProgressRangeLow) + dblProgressRangeLow);

                VBLogger.GetLogger().LogEvent(modelingProgress.ToString(), Globals.messageIntent.UserOnly, Globals.targetSStrip.ProgressBar);
            }
        }

        private void cbRandomSeed_CheckedChanged(object sender, EventArgs e)
        {
            if (cbRandomSeed.Checked)
                tbRandomSeed.Enabled = true;
            else tbRandomSeed.Enabled = false;
        }
    }
}
