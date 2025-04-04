﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VBCommon;
using VBCommon.Statistics;
using VBCommon.Transforms;
using ZedGraph;

namespace VBControls
{
    public partial class MLRPlots : UserControl
    {
        private List<double[]> _XYPlotdata;

        //Threshold value used for sensitiviy, specificity, accuracy
        double _decisionThreshold;
        double _mandateThreshold;
        double _probabilityThreshold = 50;
        double _powerExp = double.NaN;
        double _rmse = double.NaN;
        DependentVariableTransforms _depVarXFrm = DependentVariableTransforms.none;
        DependentVariableTransforms _thresholdXfrm = DependentVariableTransforms.none;
        double _pwrXFrmExponent = double.NaN;

        string _sdecisionThreshold;
        string _smandateThreshold;
        string _sprobabilityThreshold;
        string _sxfrmThresholdExponent;
        
        public EventHandler CallForProbs;

        private string[] _rptlist = new string[]{
                "Plot: Pred vs Obs",
                "Error Table: CFU as DC",
                "Plot: % Exc vs Obs",
                "Error Table: % Exc as DC"
                };
        double[] _obs;
        double[] _pred;
        double[] _prob;
        string[] _tags;

        string _sdc = "Decision Criterion: ";

        public enum Exceedance { model, prediction};
        public Exceedance _exceedance = Exceedance.model;
        

        public MLRPlots()
        {
            InitializeComponent();

            //cboxPlotList.DataSource = _rptlist;
            _decisionThreshold = Convert.ToDouble(tbThresholdDec.Text);
            _mandateThreshold = Convert.ToDouble(tbThresholdReg .Text);
            _sprobabilityThreshold = "50";
            _sdecisionThreshold = tbThresholdDec.Text;
            _smandateThreshold = tbThresholdReg.Text;
            cboxPlotList.DataSource = _rptlist;

            InitResultsGraph();
        }

        public double[] Exceedances
        {
            set { _prob = value; }
            get { return _prob; }
        }


        public int ChartType
        {
            get { return cboxPlotList.SelectedIndex; }
            set
            {
                cboxPlotList.SelectedIndex = value;
            }
        }


        public double ThresholdHoriz
        {
            get { return _decisionThreshold; }
            set
            {
                _sdecisionThreshold = value.ToString();
                _decisionThreshold = value;
            }
        }

        public double ProbabilityThreshold
        {
            get { return _probabilityThreshold; }
            set
            {
                _sprobabilityThreshold = value.ToString();
                _probabilityThreshold = value;
            }
        }

        public double ThresholdVert
        {
            get { return _mandateThreshold; }
            set
            {
                tbThresholdReg.Text = _smandateThreshold = value.ToString();
                _mandateThreshold = value;
            }
        }

        public ZedGraphControl ZGC
        {
            get { return this.zgc; }
        }

        public ListView LISTVIEW
        {
            get { return this.listView1; }
        }

        public DependentVariableTransforms DependentVarXFrm
        {            
            get { return _depVarXFrm; }
            set { _depVarXFrm = value; }
        }

        public DependentVariableTransforms ThresholdTransform
        {
            get { return _thresholdXfrm; }
            set
            {
                _thresholdXfrm = value;
                switch (_thresholdXfrm)
                {
                    case DependentVariableTransforms.none:
                        rbValue.Checked = true;
                        break;
                    case DependentVariableTransforms.Ln:
                        rbLogeValue.Checked = true;
                        break;
                    case DependentVariableTransforms.Log10:
                        rbLog10Value.Checked = true;
                        break;
                    case DependentVariableTransforms.Power:
                        rbPwrValue.Checked = true;
                        break;
                }                
            }
        }

        public double PowerTransformExponent
        {
            get { return _pwrXFrmExponent; }
            set
            {
                txtPwrValue.Text = _sxfrmThresholdExponent = value.ToString();
                _pwrXFrmExponent = value;               
            }
        }

        public void SetThresholds(double decisionThreshold, double mandateThreshold)
        {
            _decisionThreshold = decisionThreshold;
            _mandateThreshold = mandateThreshold;
            _sdecisionThreshold = _decisionThreshold.ToString();
            tbThresholdReg.Text = _smandateThreshold = _mandateThreshold.ToString();

            cboxPlotList_SelectedIndexChanged(null, null);
            btnXYPlot_Click(null, null);
        }


        public void SetThresholds(string decisionThreshold, string mandateThreshold)
        {
            _sdecisionThreshold  = decisionThreshold;
            tbThresholdReg.Text = _smandateThreshold = mandateThreshold;

            cboxPlotList_SelectedIndexChanged(null, null);
            btnXYPlot_Click(null, null);
        }


        public VBCommon.Transforms.DependentVariableTransforms Transform
        {
            set 
            {
                if (value == VBCommon.Transforms.DependentVariableTransforms.none)
                {
                    rbValue.Checked = true;
                    _decisionThreshold = Convert.ToDouble(tbThresholdDec.Text);
                    _mandateThreshold = Convert.ToDouble(tbThresholdReg.Text);
                }
                else if (value == VBCommon.Transforms.DependentVariableTransforms.Ln)
                {
                    rbLogeValue.Checked = true;
                    _decisionThreshold = Math.Log(Convert.ToDouble(tbThresholdDec.Text));
                    _mandateThreshold = Math.Log(Convert.ToDouble(tbThresholdReg.Text));
                }
                else if (value == VBCommon.Transforms.DependentVariableTransforms.Log10)
                {
                    rbLog10Value.Checked = true;
                    _decisionThreshold = Math.Log10(Convert.ToDouble(tbThresholdDec.Text));
                    _mandateThreshold = Math.Log10(Convert.ToDouble(tbThresholdReg.Text));
                }
                else if (value == VBCommon.Transforms.DependentVariableTransforms.Power)
                {
                    rbPwrValue.Checked = true;
                    double pwr = Convert.ToDouble(txtPwrValue.Text);
                    _decisionThreshold = Math.Pow(Convert.ToDouble(tbThresholdDec.Text),pwr);
                    _mandateThreshold = Math.Pow(Convert.ToDouble(tbThresholdReg.Text),pwr);
                }
            }

            get
            {               
                if (rbLogeValue.Checked)
                    return VBCommon.Transforms.DependentVariableTransforms.Ln;
                else if (rbLog10Value.Checked)
                    return VBCommon.Transforms.DependentVariableTransforms.Log10;
                else if (rbPwrValue.Checked)
                    return VBCommon.Transforms.DependentVariableTransforms.Power;
                else //(rbValue.Checked)
                    return VBCommon.Transforms.DependentVariableTransforms.none;
            }                        
        }

        public double PowerExponent
        {
            get { return _powerExp; }
            set 
            {
                    _powerExp = value; 
                    txtPwrValue.Text = _powerExp.ToString(); 
            }
        }

        private void InitResultsGraph()
        {
            _XYPlotdata = new List<double[]>();

            GraphPane myPane = zgc.GraphPane;
            if (myPane.CurveList.Count > 0)
                myPane.CurveList.RemoveRange(0, myPane.CurveList.Count - 1);

            myPane.Title.Text = "Predicted vs Observed";
            myPane.XAxis.Title.Text = "Observed";
            myPane.YAxis.Title.Text = "Predicted";

            myPane.XAxis.MajorGrid.IsVisible = true;
            myPane.XAxis.MajorGrid.Color = Color.Gray;

            myPane.YAxis.MajorGrid.IsVisible = true;
            myPane.YAxis.MajorGrid.Color = Color.Gray;

            PointPairList list = new PointPairList();

            //LineItem curve = myPane.AddCurve("Y", list, Color.Red, SymbolType.Circle);
            LineItem curve = myPane.AddCurve("", list, Color.Red, SymbolType.Circle);
            curve.Line.IsVisible = false;
            curve.Symbol.Border.IsVisible = true;
            curve.Symbol.Fill = new Fill(Color.Firebrick);

            //Vertical and horizontal threshold lines
            PointPairList list2 = new PointPairList();
            LineItem curve2 = myPane.AddCurve("Decision Threshold", list2, Color.Blue, SymbolType.None);
            curve2.Line.IsVisible = false;

            PointPairList list3 = new PointPairList();
            LineItem curve3 = myPane.AddCurve("Regulatory Threshold", list3, Color.Green, SymbolType.None);
            curve3.Line.IsVisible = false;

            // Scale the axes
            zgc.AxisChange();
        }

        public void UpdateResults(List<double[]> data, double rmse, Exceedance exceedance, string[] tags=null)
        {
            string plotName = string.Empty;
            if (exceedance == Exceedance.prediction)
                plotName = "Pred vs Obs";
             else 
                plotName = "Fit vs Obs";

            _XYPlotdata = data;
            _rmse = rmse;
            _exceedance = exceedance;

            if (_XYPlotdata == null || _XYPlotdata.Count < 1)
            {
                return;
            }

            _obs = new double[data.Count];
            _pred = new double[data.Count];
            bool bTags = false;
            _tags = null;

            if (tags != null)
            {
                if (tags.Length == data.Count)
                {
                    bTags = true;
                    _tags = new string[data.Count];
                }
            }

            double dc = ThresholdHoriz;

            for (int i = 0; i < data.Count; i++)
            {
                _obs[i] = data[i][0];
                _pred[i] = data[i][1];
                if (bTags) { _tags[i] = tags[i]; }
            }

            if (cboxPlotList.SelectedIndex == 0 || cboxPlotList.SelectedIndex == 1)
            {
                GraphPane myPane = zgc.GraphPane;
                myPane.CurveList.Clear();
                myPane = addPlotXY(_obs, _pred, _tags, null);
                myPane = addThresholdCurve(myPane, null);

                zgc.AxisChange();
                zgc.Refresh();

                CreateMFailTable();
            }

            else if (cboxPlotList.SelectedIndex == 2 || cboxPlotList.SelectedIndex == 3)
            {
                GraphPane myPane = zgc.GraphPane;
                myPane.CurveList.Clear();
                myPane = addPlotPROB(_obs, _prob, _tags, null);
                myPane = addProbThresholdCurve(myPane);
                myPane.XAxis.Cross = 0.0;
                zgc.AxisChange();
                zgc.Refresh();
 
                CreatePExceedTable();
            }

            if (bTags) { zgc.IsShowPointValues = true; }
        }


        /// <summary>
        /// accessor to populate probabilities; used from prediction
        /// mlr model probabilities differ from predictions - set after call to UpdateResults()
        /// </summary>
        public double[] SetPredictionProbabilities
        {
            set { _prob = value; }
        }

        private void tbThresholdReg_TextChanged(object sender, EventArgs e)
        {

            if (Double.TryParse(tbThresholdReg.Text, out _mandateThreshold) == false)
            {
                string msg = @"Regulatory standard must be a numeric value.";
                MessageBox.Show(msg);
                return;
            }
            _smandateThreshold = tbThresholdReg.Text;
        }


        private void tbThresholdDec_TextChanged(object sender, EventArgs e)
        {
            double threshold;
            if (Double.TryParse(tbThresholdDec.Text, out threshold) == false)
            {
                string msg = @"Decision criterion must be a numeric value.";
                MessageBox.Show(msg);
                return;
            }
            
            if (cboxPlotList.SelectedIndex == 0 || cboxPlotList.SelectedIndex == 1)
            {
                _decisionThreshold = threshold;
                _sdecisionThreshold = tbThresholdDec.Text;
            }
            else if (cboxPlotList.SelectedIndex == 2 || cboxPlotList.SelectedIndex == 3)
            {
                _probabilityThreshold = threshold;
                _sprobabilityThreshold = tbThresholdDec.Text;
            }
        }


        private void txtPwr_Leave(object sender, EventArgs e)
        {
            double power;
            TextBox txtBox = (TextBox)sender;

            if (!Double.TryParse(txtBox.Text, out power))
            {
                MessageBox.Show("Invalid number.");
                txtBox.Focus();
                return;
            }
            _sxfrmThresholdExponent = txtBox.Text.ToString();
            _powerExp = power;            
        }


        private void rbPwrValue_CheckedChanged(object sender, EventArgs e)
        {
            if (rbPwrValue.Checked)
            {
                txtPwrValue.Enabled = true;
            }
            else
            {
                txtPwrValue.Enabled = false;
            }
        }


        public void btnXYPlot_Click(object sender, EventArgs e)
        {
            //just validate and get the thresholds, and then transform
            tbThresholdDec_TextChanged(null, EventArgs.Empty);
            tbThresholdReg_TextChanged(null, EventArgs.Empty);
            if (rbValue.Checked)
            {
                rbValue_CheckedChanged(null, EventArgs.Empty);
            }
            else if (rbLog10Value.Checked)
            {
                rbLog10Value_CheckedChanged(null, EventArgs.Empty);
            }
            else if (rbLogeValue.Checked)
            {
                rbLogeValue_CheckedChanged(null, EventArgs.Empty);
            }
            else if (rbPwrValue.Checked)
            {
                rbPwrValue_Changed(null, EventArgs.Empty);
            }

            if (CallForProbs != null && _XYPlotdata != null)
            {
                CallForProbs(this, null);
            }
            UpdateResults(_XYPlotdata, _rmse, _exceedance, _tags);
        }


        private void rbValue_CheckedChanged(object sender, EventArgs e)
        {
            if (rbValue.Checked)
            {
                //Create three variables to hold working values of the thresholds.
                double tv = double.NaN;
                double th = double.NaN;
                double dblDecisionThreshold = double.NaN;
                _thresholdXfrm = DependentVariableTransforms.none;

                try
                {
                    //Initialize the working values.
                    tv = Convert.ToDouble(tbThresholdReg.Text.ToString());
                    th = Convert.ToDouble(tbThresholdDec.Text.ToString());
                    if (cboxPlotList.SelectedIndex == 0 || cboxPlotList.SelectedIndex == 1) { dblDecisionThreshold = th; }
                    else { dblDecisionThreshold = Convert.ToDouble(_sdecisionThreshold); }

                    //Apply the plotting transform to the thresholds:
                    if (_depVarXFrm == DependentVariableTransforms.Power)
                    {
                        tv = Apply.TransformThreshold(tv, DependentVariableTransforms.Power, _pwrXFrmExponent);
                        dblDecisionThreshold = Apply.TransformThreshold(dblDecisionThreshold, DependentVariableTransforms.Power, _pwrXFrmExponent);
                    }
                    else
                    {
                        tv = Apply.TransformThreshold(tv, DependentVarXFrm);
                        dblDecisionThreshold = Apply.TransformThreshold(dblDecisionThreshold, DependentVarXFrm);
                    }                    
                }
                catch
                {
                    string msg = @"Cannot convert thresholds. (" + tbThresholdDec.Text + ", " + tbThresholdReg.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }

                //Store the thresholds
                _mandateThreshold = tv;
                _decisionThreshold = dblDecisionThreshold;
                if (cboxPlotList.SelectedIndex == 2 || cboxPlotList.SelectedIndex == 3) { _probabilityThreshold = th; }

                _sdc = "Decision Criterion: " + _sdecisionThreshold;
            }
        }


        private void rbLog10Value_CheckedChanged(object sender, EventArgs e)
        {
            if (rbLog10Value.Checked)
            {
                //Create three variables to hold working values of the thresholds.
                double tv = double.NaN;
                double th = double.NaN;
                double dblDecisionThreshold = double.NaN;
                _thresholdXfrm = DependentVariableTransforms.Log10;

                //ms has no fp error checking... check for all conditions.
                //log10(x) when x == 0 results in NaN and when x < 0 results in -Infinity
                
                try
                {
                    //Initialize the working values.
                    tv = Convert.ToDouble(tbThresholdReg.Text.ToString());
                    th = Convert.ToDouble(tbThresholdDec.Text.ToString());
                    if (cboxPlotList.SelectedIndex == 0 || cboxPlotList.SelectedIndex == 1) { dblDecisionThreshold = th; }
                    else { dblDecisionThreshold = Convert.ToDouble(_sdecisionThreshold); }

                    //Remove the input transform from both thresholds
                    tv = Apply.UntransformThreshold(tv, DependentVariableTransforms.Log10);
                    dblDecisionThreshold = Apply.UntransformThreshold(dblDecisionThreshold, DependentVariableTransforms.Log10);

                    //Apply the plotting transform to both thresholds
                    if (DependentVarXFrm == DependentVariableTransforms.Power)
                    {
                        tv = Apply.TransformThreshold(tv, DependentVariableTransforms.Power, _pwrXFrmExponent);
                        dblDecisionThreshold = Apply.TransformThreshold(dblDecisionThreshold, DependentVariableTransforms.Power, _pwrXFrmExponent);
                    }
                    else
                    {
                        tv = Apply.TransformThreshold(tv, DependentVarXFrm);
                        dblDecisionThreshold = Apply.TransformThreshold(dblDecisionThreshold, DependentVarXFrm);
                    }
                }
                catch
                {
                    string msg = @"Cannot Exponentiate transform thresholds. (" + tbThresholdDec.Text + ", " + tbThresholdReg.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }
                if (tv.Equals(double.NaN) || th.Equals(double.NaN))
                {
                    string msg = @"Entered values must be greater than 0. (" + tbThresholdDec.Text + ", " + tbThresholdReg.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }
                if (tv < 0 || th < 0)
                {
                    string msg = @"Entered values must be greater than 0. (" + tbThresholdDec.Text + ", " + tbThresholdReg.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }

                //Store the thresholds
                _mandateThreshold = tv;
                _decisionThreshold = dblDecisionThreshold;
                if (cboxPlotList.SelectedIndex == 2 || cboxPlotList.SelectedIndex == 3) { _probabilityThreshold = th; }

                _sdc = "Decision Criterion: " + "10^(" + _sdecisionThreshold + ")";
            }
        }


        private void rbLogeValue_CheckedChanged(object sender, EventArgs e)
        {
            if (rbLogeValue.Checked)
            {
                //Create three variables to hold the working values of the thresholds.
                double tv = double.NaN;
                double th = double.NaN;
                double dblDecisionThreshold = double.NaN;
                _thresholdXfrm = DependentVariableTransforms.Ln;

                //ms has no fp error checking... check for all conditions.
                //loge(x) when x == 0 results in NaN and when x < 0 results in -Infinity
                
                try
                {
                    //Initialize the working values. 
                    tv = Convert.ToDouble(tbThresholdReg.Text.ToString());
                    th = Convert.ToDouble(tbThresholdDec.Text.ToString());
                    if (cboxPlotList.SelectedIndex == 0 || cboxPlotList.SelectedIndex == 1) { dblDecisionThreshold = th; }
                    else { dblDecisionThreshold = Convert.ToDouble(_sdecisionThreshold); }
                    
                    //Remove the input trransform from both thresholds
                    dblDecisionThreshold = Apply.UntransformThreshold(dblDecisionThreshold, DependentVariableTransforms.Ln);
                    tv = Apply.UntransformThreshold(tv, DependentVariableTransforms.Ln);

                    //Apply the plotting transform to both thresholds
                    if (DependentVarXFrm == DependentVariableTransforms.Power)
                    {
                        dblDecisionThreshold = Apply.TransformThreshold(dblDecisionThreshold, DependentVariableTransforms.Power, _pwrXFrmExponent);
                        tv = Apply.TransformThreshold(tv, DependentVariableTransforms.Power, _pwrXFrmExponent);
                    }
                    else
                    {
                        dblDecisionThreshold = Apply.TransformThreshold(dblDecisionThreshold, DependentVarXFrm);
                        tv = Apply.TransformThreshold(tv, DependentVarXFrm);
                    }
                }
                catch
                {
                    string msg = @"Cannot exponentiate transform thresholds. (" + tbThresholdDec.Text + ", " + tbThresholdReg.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }
                if (tv.Equals(double.NaN) || th.Equals(double.NaN))
                {
                    string msg = @"Entered values must be greater than 0. (" + tbThresholdDec.Text + ", " + tbThresholdReg.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }
                if (tv < 0 || th < 0)
                {
                    string msg = @"Entered values must be greater than 0. (" + tbThresholdDec.Text + ", " + tbThresholdReg.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }

                //Store the thresholds
                _mandateThreshold = tv;
                _decisionThreshold = dblDecisionThreshold;
                if (cboxPlotList.SelectedIndex == 2 || cboxPlotList.SelectedIndex == 3) { _probabilityThreshold = th; }

                _sdc = "Decision Criterion: " + "Exp(" + _sdecisionThreshold + ")";
            }
        }


        private void rbPwrValue_Changed(object sender, EventArgs e)
        {
            if (rbPwrValue.Checked)
            {
                //Create four variables to hold working values of the thresholds.
                double tv = double.NaN;
                double th = double.NaN;
                double power = double.NaN;
                double dblDecisionThreshold = double.NaN;
                _thresholdXfrm = DependentVariableTransforms.Power;

                //ms has no fp error checking... check for all conditions.
                //loge(x) when x == 0 results in NaN and when x < 0 results in -Infinity

                try
                {
                    //Initialize the working values
                    tv = Convert.ToDouble(tbThresholdReg.Text.ToString());
                    th = Convert.ToDouble(tbThresholdDec.Text.ToString());
                    power = Convert.ToDouble(txtPwrValue.Text);
                    if (cboxPlotList.SelectedIndex == 0 || cboxPlotList.SelectedIndex == 1) { dblDecisionThreshold = th; }
                    else { dblDecisionThreshold = Convert.ToDouble(_sdecisionThreshold); }

                    //Remove the input transform from both thresholds
                    tv = Apply.UntransformThreshold(tv, DependentVariableTransforms.Power, power);
                    dblDecisionThreshold = Apply.UntransformThreshold(dblDecisionThreshold, DependentVariableTransforms.Power, power);

                    //Apply the plotting transform to both thresholds
                    if (DependentVarXFrm == DependentVariableTransforms.Power)
                    {
                        tv = Apply.TransformThreshold(tv, DependentVariableTransforms.Power, _pwrXFrmExponent);
                        dblDecisionThreshold = Apply.TransformThreshold(dblDecisionThreshold, DependentVariableTransforms.Power, _pwrXFrmExponent);   
                    }
                    else
                    {
                        tv = Apply.TransformThreshold(tv, DependentVarXFrm);
                        dblDecisionThreshold = Apply.TransformThreshold(dblDecisionThreshold, DependentVarXFrm);
                    }
                }
                catch
                {
                    string msg = @"Cannot power transform thresholds. (" + tbThresholdDec.Text + ", " + tbThresholdReg.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }
                if (tv.Equals(double.NaN) || th.Equals(double.NaN))
                {
                    string msg = @"Entered values must be greater than 0. (" + tbThresholdDec.Text + ", " + tbThresholdReg.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }
                if (tv < 0 || th < 0)
                {
                    string msg = @"Entered values must be greater than 0. (" + tbThresholdDec.Text + ", " + tbThresholdReg.Text + ") ";
                    MessageBox.Show(msg);
                    return;
                }

                //Store the thresholds
                _mandateThreshold = tv;
                _decisionThreshold = th;
                if (cboxPlotList.SelectedIndex == 2 || cboxPlotList.SelectedIndex == 3) { _probabilityThreshold = th; }

                _sdc = "Decision Criterion: " + "(" + _sdecisionThreshold + ")" + "** (1/" + txtPwrValue.Text + ")"; 
            }
        }


        public void cboxPlotList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboxPlotList.SelectedIndex == 0)
            {
                zgc.Visible = true;
                listView1.Visible = false;
                label11.Text = "Decision Criterion (Horizontal)";
                tbThresholdDec.Text =_sdecisionThreshold;
                btnXYPlot_Click(null, null);
                groupBox2.Visible = true;
                groupBox3.Visible = true;
                gbxModelvsPred.Visible = false;
                rbViewFits.Visible = false;
                rbViewPred.Visible = false;
            }
            else if (cboxPlotList.SelectedIndex == 1)
            {
                label11.Text = "Decision Criterion (Horizontal)";
                tbThresholdDec.Text = _sdecisionThreshold;
                zgc.Visible = false;
                listView1.Visible = true;
                groupBox2.Visible = false;
                groupBox3.Visible = false;
                gbxModelvsPred.Visible = false;
                rbViewFits.Visible = false;
                rbViewPred.Visible = false;

            }
            else if (cboxPlotList.SelectedIndex == 2)
            {
                zgc.Visible = true;
                listView1.Visible = false;
                label11.Text = "Percent Probability (0-100)";
                tbThresholdDec.Text = _sprobabilityThreshold;
                groupBox2.Visible = true;
                groupBox3.Visible = true;
                gbxModelvsPred.Visible = true;
                rbViewFits.Visible = true;
                rbViewPred.Visible = true;
            }
            else if (cboxPlotList.SelectedIndex == 3)
            {
                label11.Text = "Percent Probability (0-100)";
                tbThresholdDec.Text = _sprobabilityThreshold;
                zgc.Visible = false;
                listView1.Visible = true;
                groupBox2.Visible = false;
                groupBox3.Visible = false;
                gbxModelvsPred.Visible = false;
                rbViewFits.Visible = false;
                rbViewPred.Visible = false;
            }

            if (CallForProbs != null && _XYPlotdata != null)
            {
                CallForProbs(this, null);
            }
            UpdateResults(_XYPlotdata, _rmse, _exceedance, _tags);
        }


        private void CreatePExceedTable()
        {
            listView1.Clear();
            listView1.View = View.Details;

            string[] cols = { "P(Exceedance)", "False Non-Exceed", "False Exceed", "Total", "Sensitivity", "Specificity", "Accuracy" };
            for (int i = 0; i < cols.Length; i++)
            {
                listView1.Columns.Add(new ColumnHeader());
                listView1.Columns[i].Text = cols[i];
                listView1.Columns[i].TextAlign = HorizontalAlignment.Left;
            }

            for (int prob = 5; prob < 101; prob = prob + 5)
            {
                string[] line = new string[7];
                line[0] = prob.ToString();

                ModelErrorCounts mec = new ModelErrorCounts();
                mec.getCounts(prob, _mandateThreshold, _prob, _obs);

                line[1] = mec.FNCount.ToString();
                line[2] = mec.FPCount.ToString();
                int tot = mec.FPCount + mec.FNCount;
                line[3] = tot.ToString();
                line[5] = mec.Specificity.ToString("f4");
                line[4] = mec.Sensitivity.ToString("f4");
                line[6] = mec.Accuracy.ToString("f4");

                ListViewItem lvi = new ListViewItem(line);
                listView1.Items.Add(lvi);
            }
            string[] newline = new string[] { "", "", "", "", "", "", "" };
            listView1.Items.Add(new ListViewItem(newline));
            newline[0] = _sdc;
            listView1.Items.Add(new ListViewItem(newline));
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }


        private void CreateMFailTable()
        {
            //just validate and get the thresholds, and then transform
            tbThresholdDec_TextChanged(null, EventArgs.Empty);
            tbThresholdReg_TextChanged(null, EventArgs.Empty);
            if (rbValue.Checked)
            {
                rbValue_CheckedChanged(null, EventArgs.Empty);
            }
            else if (rbLog10Value.Checked)
            {
                rbLog10Value_CheckedChanged(null, EventArgs.Empty);
            }
            else if (rbLogeValue.Checked)
            {
                rbLogeValue_CheckedChanged(null, EventArgs.Empty);
            }
            else if (rbPwrValue.Checked)
            {
                rbPwrValue_Changed(null, EventArgs.Empty);
            }

            const int interations = 25;

            listView1.Clear();
            listView1.View = View.Details;

            string[] cols = { "Decision Threshold", "False Non-Exceed", "False Exceed", "Total", "Sensitivity", "Specificity", "Accuracy" };
            for (int i = 0; i < cols.Length; i++)
            {
                listView1.Columns.Add(new ColumnHeader());
                listView1.Columns[i].Text = cols[i];
                listView1.Columns[i].TextAlign = HorizontalAlignment.Left;
            }

            double dcMax = _pred.Max();
            double dcMin = _pred.Min();
            double inc = (dcMax - dcMin) / (double)interations;
            double dc = dcMin - inc;

            ModelErrorCounts mec;
            int tot;
            ListViewItem lvi;
            bool lineInserted = false;
            while (dc < dcMax)
            {
                dc += inc;
                string[] line = new string[7];
                double tdc = 0.0d;
                if (Transform == VBCommon.Transforms.DependentVariableTransforms.Log10)
                {
                    tdc = Math.Pow(10.0d, dc);
                }
                else if (Transform == VBCommon.Transforms.DependentVariableTransforms.Ln)
                {
                    tdc = Math.Pow(Math.E, dc);
                }
                else if (Transform == VBCommon.Transforms.DependentVariableTransforms.Power)
                {
                    tdc = Math.Pow(dc, 1.0 / _powerExp);
                }
                else //(Transform == Globals.DependendVariableTransforms.none)
                {
                    tdc = dc;
                }

                line[0] = tdc.ToString("n4");

                mec = new ModelErrorCounts();
                mec.getCounts(dc, _mandateThreshold, _pred, _obs);
                line[1] = mec.FNCount.ToString();
                line[2] = mec.FPCount.ToString();
                tot = mec.FNCount + mec.FPCount;
                line[3] = tot.ToString();
                line[5] = mec.Specificity.ToString("f4");
                line[4] = mec.Sensitivity.ToString("f4");
                line[6] = mec.Accuracy.ToString("f4");

                lvi = new ListViewItem(line);
                listView1.Items.Add(lvi);
            }
            string[] newline = new string[] { "", "", "", "", "", "", "" };
            listView1.Items.Add(new ListViewItem(newline));
            newline[0] = _sdc;
            listView1.Items.Add(new ListViewItem(newline));
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }


        private GraphPane addPlotPROB(double[] obs, double[] pexceed, string[] tags, string plot)
        {
            PointPairList ppl1 = new PointPairList();

            string tag = string.Empty;
            bool bTags = false;
            if (tags != null)
            {
                if (tags.Length == obs.Length)
                    bTags = true;
            }

            int npts = obs.Length > pexceed.Length ? pexceed.Length : obs.Length;

            for (int i = 0; i < npts; i++)
            {
                if (bTags) { ppl1.Add(obs[i], pexceed[i], tags[i]); }
                else { ppl1.Add(obs[i], pexceed[i]); }  
            }

            GraphPane gp = zgc.GraphPane;
            LineItem curve1 = gp.AddCurve(_sdc, ppl1, Color.Red, SymbolType.Circle);
            curve1.Symbol.Border.IsVisible = true;
            curve1.Symbol.Fill = new Fill(Color.Firebrick);
            curve1.Line.IsVisible = false;

            gp.XAxis.Title.Text = "Observed";
            if (_exceedance == Exceedance.prediction)
                gp.YAxis.Title.Text = "Probability of Prediction Exceedance";
            else gp.YAxis.Title.Text = "Probability of FitValue Exceedance";

            gp.Tag = "PROBPlot";
            if (_exceedance == Exceedance.prediction)
                gp.Title.Text = "Prediction Probability Exceedance vs Observations";
            else gp.Title.Text = "FitValue Probability Exceedance vs Observations";
            gp.XAxis.Type = AxisType.Linear;

            ModelErrorCounts mec = new ModelErrorCounts();
            mec.getCounts(_probabilityThreshold, _mandateThreshold, pexceed, obs);

            tbFN.Text = mec.FNCount.ToString();
            tbFP.Text = mec.FPCount.ToString();
            txbSpecificity.Text = mec.Specificity.ToString();
            txbSensitivity.Text = mec.Sensitivity.ToString();
            txbAccuracy.Text = mec.Accuracy.ToString();

            gp.XAxis.Cross = 0;

            if (bTags) { zgc.IsShowPointValues = true; }

            return gp;
        }


        private GraphPane addProbThresholdCurve(GraphPane myPane)
        {
            double xMin, xMax, yMin, yMax;
            double xPlotMin, xPlotMax, yPlotMin, yPlotMax;
            myPane.CurveList[0].GetRange(out xMin, out xMax, out yMin, out yMax, false, false, myPane);

            if (xMax.GetType() == typeof(ZedGraph.XDate))
            {
                xPlotMin = xMin < 0.0 ? xMin : 0;
                xPlotMax = xMax > _probabilityThreshold ? xMax : _probabilityThreshold;
            }
            else
            {
                xPlotMin = xMin;
                xPlotMax = xMax;
            }
            yPlotMin = yMin < 0.0 ? yMin : 0;

            //mikec wants max yscale to be this so display is max 1.2
            yPlotMax = 101.0;

            //probability threshold
            PointPair pp1 = new PointPair(xPlotMin, _probabilityThreshold);
            PointPair pp2 = new PointPair(xPlotMax, _probabilityThreshold);
            PointPairList ppl1 = new PointPairList();
            ppl1.Add(pp1);
            ppl1.Add(pp2);

            //regulatory threshold
            pp1 = new PointPair(_mandateThreshold, yPlotMin);
            pp2 = new PointPair(_mandateThreshold, yPlotMax);
            PointPairList ppl2 = new PointPairList();
            ppl2.Add(pp1);
            ppl2.Add(pp2);

            LineItem curve1 = myPane.AddCurve("Exceedance Probability Threshold", ppl1, Color.Blue, SymbolType.None);
            LineItem curve2 = myPane.AddCurve("Regulatory Threshold", ppl2, Color.Green, SymbolType.None);
            curve1.Line.IsVisible = true;

            return myPane;
        }


        private GraphPane addPlotXY(double[] obs, double[] pred, string[] tags, string plot /*, double[] unbiased */)
        {
            PointPairList ppl1 = new PointPairList();

            string tag = string.Empty;
            bool bTags = false;
            if (tags != null)
            {
                if (tags.Length == obs.Length)
                    bTags = true;
            }

            int npts = obs.Length > pred.Length ? pred.Length : obs.Length;

            for (int i = 0; i < npts; i++)
            {
                if (bTags) { ppl1.Add(obs[i], pred[i], tags[i]); }
                else { ppl1.Add(obs[i], pred[i]); }                
            }

            GraphPane gp = zgc.GraphPane;
            LineItem curve1 = gp.AddCurve(null, ppl1, Color.Red, SymbolType.Circle);
            curve1.Symbol.Border.IsVisible = true;
            curve1.Symbol.Fill = new Fill(Color.Firebrick);
            curve1.Line.IsVisible = false;

            gp.XAxis.Title.Text = "Observed";
            if (_exceedance == Exceedance.prediction)
                gp.YAxis.Title.Text = "Predicted";
            else gp.YAxis.Title.Text = "Fitted";

            gp.Tag = "XYPlot";
            if (_exceedance == Exceedance.prediction)
                gp.Title.Text = "Predicted vs Observed Values";
            else gp.Title.Text = "Fitted vs Observed";
            gp.XAxis.Type = AxisType.Linear;

            ModelErrorCounts mec = new ModelErrorCounts();
            mec.getCounts(_decisionThreshold, _mandateThreshold, pred, obs);

            tbFN.Text = mec.FNCount.ToString();
            tbFP.Text = mec.FPCount.ToString();
            txbSpecificity.Text = mec.Specificity.ToString();
            txbSensitivity.Text = mec.Sensitivity.ToString();
            txbAccuracy.Text = mec.Accuracy.ToString();

            gp.XAxis.MajorGrid.IsVisible = true;
            gp.XAxis.MajorGrid.Color = Color.Gray;

            gp.YAxis.MajorGrid.IsVisible = true;
            gp.YAxis.MajorGrid.Color = Color.Gray;

            if (bTags) { zgc.IsShowPointValues = true; }

            return gp;
        }

        private GraphPane addThresholdCurve(GraphPane myPane, string plot)
        {
            double xMin, xMax, yMin, yMax;
            double xPlotMin, xPlotMax, yPlotMin, yPlotMax;
            myPane.CurveList[0].GetRange(out xMin, out xMax, out yMin, out yMax, false, false, myPane);
 
            xPlotMin = xMin;
            xPlotMax = xMax > _decisionThreshold ? xMax : _decisionThreshold;
  
            yPlotMin = yMin < 0.0 ? yMin : 0;
            yPlotMax = yMax > _mandateThreshold ? yMax : _mandateThreshold;

            //decision threshold
            PointPair pp1 = new PointPair(xPlotMin - 1, _decisionThreshold);
            PointPair pp2 = new PointPair(xPlotMax + 1, _decisionThreshold);
            PointPairList ppl1 = new PointPairList();
            ppl1.Add(pp1);
            ppl1.Add(pp2);

            //regulatory threshold
            pp1 = new PointPair(_mandateThreshold, yPlotMin - 1);
            pp2 = new PointPair(_mandateThreshold, yPlotMax + 1);
            PointPairList ppl2 = new PointPairList();
            ppl2.Add(pp1);
            ppl2.Add(pp2);

            LineItem curve1 = myPane.AddCurve("Decision Threshold", ppl1, Color.Blue, SymbolType.None);
            LineItem curve2 = myPane.AddCurve("Regulatory Threshold", ppl2, Color.Green, SymbolType.None);
            curve1.Line.IsVisible = true;
            curve2.Line.IsVisible = true;

            return myPane;
        }


        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Modifiers & Keys.Control) != 0)
            {
                if (e.KeyCode == Keys.C)
                {
                    CopyListViewToClipboard(listView1);
                }
            }
        }


        public void CopyListViewToClipboard(ListView lv)
        {
            StringBuilder buffer = new StringBuilder();

            for (int i = 0; i < lv.Columns.Count; i++)
            {
                buffer.Append(lv.Columns[i].Text);
                buffer.Append("\t");
            }

            buffer.Append(Environment.NewLine);

            for (int i = 0; i < lv.Items.Count; i++)
            {
                if (lv.Items[i].Selected)
                {
                    for (int j = 0; j < lv.Columns.Count; j++)
                    {
                        buffer.Append(lv.Items[i].SubItems[j].Text);
                        buffer.Append("\t");
                    }

                    buffer.Append(Environment.NewLine);
                }
            }

            Clipboard.SetText(buffer.ToString());
        }


        private void rbValue_CheckedChanged_1(object sender, EventArgs e)
        {
            if (rbValue.Checked)
            {
                if (cboxPlotList.SelectedIndex == 0 || cboxPlotList.SelectedIndex == 1)
                {
                    _sdc = "Decision Criterion: " + tbThresholdDec.Text;
                }
            }
        }


        private void rbLog10Value_CheckedChanged_1(object sender, EventArgs e)
        {
            if (rbLog10Value.Checked)
            {
                if (cboxPlotList.SelectedIndex == 0 || cboxPlotList.SelectedIndex == 1)
                {
                    _sdc = "Decision Criterion: " + "Log(" + tbThresholdDec.Text + ")";
                }
            }
        }


        private void rbLogeValue_CheckedChanged_1(object sender, EventArgs e)
        {
            if (rbLogeValue.Checked)
            {
                if (cboxPlotList.SelectedIndex == 0 || cboxPlotList.SelectedIndex == 1)
                {
                    _sdc = "Decision Criterion: " + "Ln(" + tbThresholdDec.Text + ")";
                }
            }  
        }


        public Dictionary<string, object> PackState()
        {
            Dictionary<string, object> dictPackedState = new Dictionary<string, object>();

            dictPackedState.Add("ChartType", cboxPlotList.SelectedIndex);
            dictPackedState.Add("ProbabilityThreshold", _sprobabilityThreshold);
            dictPackedState.Add("DecisionThreshold", _sdecisionThreshold);
            dictPackedState.Add("RegulatoryThreshold", _smandateThreshold);
            dictPackedState.Add("ThresholdTransformExponent", _sxfrmThresholdExponent);
            dictPackedState.Add("ThresholdTransformType", _thresholdXfrm);
            dictPackedState.Add("DepVarTransformType", _depVarXFrm);

            return dictPackedState;
        }


        public void UnpackState(Dictionary<string, object> PackedState)
        {
            if (PackedState.ContainsKey("ProbabilityThreshold"))
                ProbabilityThreshold = Convert.ToDouble(PackedState["ProbabilityThreshold"]);
            if (PackedState.ContainsKey("DecisionThreshold"))
                ThresholdHoriz = Convert.ToDouble(PackedState["DecisionThreshold"]);
            if (PackedState.ContainsKey("RegulatoryThreshold"))
                ThresholdVert = Convert.ToDouble(PackedState["RegulatoryThreshold"]);
            if (PackedState.ContainsKey("ThresholdTransformExponent"))
                PowerTransformExponent = Convert.ToDouble(PackedState["ThresholdTransformExponent"]);
            if (PackedState.ContainsKey("ThresholdTransformType"))
                ThresholdTransform = (DependentVariableTransforms)(Convert.ToInt32(PackedState["ThresholdTransformType"]));
            if (PackedState.ContainsKey("DepVarTransformType"))
                DependentVarXFrm = (DependentVariableTransforms)(Convert.ToInt32(PackedState["DepVarTransformType"]));
            if (PackedState.ContainsKey("ChartType"))
                ChartType = Convert.ToInt32(PackedState["ChartType"]);
            
            //Trigger the control to redraw itself with the correct display settings:
            cboxPlotList_SelectedIndexChanged(null, null);
        }

        private void rbViewFits_CheckedChanged(object sender, EventArgs e)
        {
            if (rbViewFits.Checked)
            {
                _exceedance = Exceedance.model;
                cboxPlotList_SelectedIndexChanged(null, null);
            }

        }

        private void rbViewPred_CheckedChanged(object sender, EventArgs e)
        {
            if (rbViewPred.Checked)
            {
                _exceedance = Exceedance.prediction;
                cboxPlotList_SelectedIndexChanged(null, null);
            }
        }
    }
}
