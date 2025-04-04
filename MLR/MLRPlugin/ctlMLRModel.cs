﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using GALibForm;
using VBCommon;
using VBCommon.Controls;

namespace MLRPlugin
{
    public partial class ctlMLRModel : UserControl
    {
        public static ctlMLRModel MLRControl = null;
        private bool bAllowNotifiableDataEvent = true;

        public ctlMLRModel()
        {
            InitializeComponent();

            MLRControl = this;

            dsControl1.NotifiableChangeEvent +=new EventHandler(dsControl1_NotifiableChangeEvent);
        }


        public TabControl ModelingTabControl
        {
            get { return tabControl1; }
        }


        //Return a flag indicating whether the plugin is ready to export a completed model
        public bool Complete
        {
            get { return frmModel1.ModelingComplete; }
        }


        public bool AllowNotifiableDataEvent
        {
            get { return bAllowNotifiableDataEvent; }
            set { bAllowNotifiableDataEvent = value; }
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


        //Handle changes in the local datasheet control
        private void dsControl1_NotifiableChangeEvent(object source, EventArgs e)
        {
            if (!bAllowNotifiableDataEvent)
                return;
            
            MLRCore.MLRDataManager _dataMgr = MLRCore.MLRDataManager.GetDataManager();
            _dataMgr.ModelDataTable = dsControl1.FilteredDataTable;
            frmModel1.InitControls();
            frmModel1.SetData();
        }


        public GALibForm.frmModel ModelForm
        {
            get { return this.frmModel1; }
        }


        public DatasheetControl LocalDatasheet
        {
            get { return this.dsControl1; }
        }


        /// <summary>
        /// Pack the plugin state
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, object> PackProjectState()
        {
            Dictionary<string, object> pluginState = new Dictionary<string, object>();
            DataTable dt = dsControl1.DT;

            if (dt != null)
            {
                StringWriter sw = null;
                sw = new StringWriter();
                dt.WriteXml(sw, XmlWriteMode.WriteSchema, false);
                string xmlDataTable = sw.ToString();
                sw.Close();
                sw = null;
                pluginState.Add("CorrelationDataTable", xmlDataTable);
            }

            pluginState.Add("PackedDatasheetState", dsControl1.PackState());
            //pluginState.Add("VarSelectionState", ctlVariableSelection1.PackState());
            pluginState.Add("ActiveTab", tabControl1.SelectedIndex);
            pluginState.Add("Model", frmModel1.PackProjectState());

            if (frmModel1.ModelInfo != null)
            {
                Dictionary<string, object> dictTransform = new Dictionary<string, object>();
                dictTransform.Add("Type", frmModel1.ModelInfo.DependentVariableTransform);
                dictTransform.Add("Exponent", frmModel1.ModelInfo.DependentVariablePowerTransformExponent);
                pluginState.Add("Transform", dictTransform);

                pluginState.Add("Predictors", frmModel1.ModelInfo.IndependentVariables.ToList());

                pluginState.Add("xfrmThreshold", frmModel1.ModelInfo.ThresholdTransform);
                pluginState.Add("ThresholdPowerTransformExponent", frmModel1.ModelInfo.ThresholdPowerTransformExponent);

                pluginState.Add("xfrmImported", frmModel1.ModelInfo.DependentVariableTransform);
                pluginState.Add("ImportedPowerTransformExponent", frmModel1.ModelInfo.DependentVariablePowerTransformExponent);
            }

            pluginState.Add("Method", "MLR");

            return pluginState;
        }


        public void UnpackProjectState(IDictionary<string, object> dictProjectState)
        {
            //Something went wrong
            if (dictProjectState == null || dictProjectState.Count < 4)
                return;

            if (dictProjectState.ContainsKey("PackedDatasheetState"))            
                dsControl1.UnpackState(dictProjectState["PackedDatasheetState"] as IDictionary<string, object>);
            
            MLRCore.MLRDataManager _dataMgr = MLRCore.MLRDataManager.GetDataManager();
            _dataMgr.ModelDataTable = dsControl1.FilteredDataTable;

            frmModel1.InitControls();
            frmModel1.SetData();
            
            if (dictProjectState.ContainsKey("Model"))
                frmModel1.UnpackProjectState(dictProjectState["Model"] as IDictionary<string, object>);

            if (dictProjectState.ContainsKey("ActiveTab"))
                tabControl1.SelectedIndex = (int)(dictProjectState["ActiveTab"]); //model
    
            this.Show();
        }
    }
}
