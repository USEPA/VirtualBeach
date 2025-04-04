using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VBCommon;
using VBCommon.Controls;
using VBCommon.Transforms;

namespace Prediction
{
    public partial class frmPredictionScatterPlot : Form
    {
        //observations and stats datatables
        private DataTable dtObs = null;
        private DataTable dtStats = null;
        private string strObservationColumn;
        private string strPredictionColumn;
        private string strProbabilityColumn;

        private DependentVariableTransforms xfrmObs;
        private DependentVariableTransforms xfrmPred;
        private DependentVariableTransforms xfrmThresh;
        private DependentVariableTransforms xfrmThreshOriginal;

        private double dblObsExponent;
        private double dblPredExponent;
        private double dblThreshExponent;
        private double dblThreshExponentOriginal;
        private double dblOriginalDecisionThreshold;
        private double dblOriginalRegulatoryThreshold;

        //constructor
        public frmPredictionScatterPlot(DataTable dtObs, DataTable dtStats, string ObservationColumn = "Observation", string PredictionColumn = "Model_Prediction", string ProbabilityColumn = "Exceedance_Probability", bool RawPredictions = true)
        {
            InitializeComponent();
            this.dtObs = dtObs;
            this.dtStats = dtStats;
            this.strObservationColumn = scatterPlot.response = ObservationColumn;
            this.strPredictionColumn = PredictionColumn;
            this.strProbabilityColumn = ProbabilityColumn;
            
            scatterPlot.ReplotRequested += new EventHandler(scatterPlot_ReplotRequested);
            scatterPlot.PlotModeToggled +=new EventHandler(scatterPlot_PlotModeToggled);
            scatterPlot.UpdateResults(GetObsPredData());            
        }


        //configure display for plot
        public void ConfigureDisplay(double DecisionThreshold, double RegulatoryThreshold, double ProbabilityThreshold, DependentVariableTransforms ObservationTransform, double ObservationExponent, DependentVariableTransforms PredictionTransform, double PredictionExponent, DependentVariableTransforms ThresholdTransform, double ThresholdExponent, bool RawPredictions=false, bool EnableProbabilityThreshold=false)
        {
            scatterPlot.SetThresholds(DecisionThreshold, RegulatoryThreshold, ProbabilityThreshold);
            scatterPlot.PowerExponent = ThresholdExponent;            
            scatterPlot.Transform = ThresholdTransform.ToString();
            scatterPlot.RawPredictions = RawPredictions;
            scatterPlot.DisplayTransform = PredictionTransform.ToString();
            scatterPlot.DisplayTransformExponent = PredictionExponent;

            xfrmObs = ObservationTransform;
            xfrmPred = PredictionTransform;
            xfrmThresh = ThresholdTransform;

            //Store these in case we need to go back to the original thresholds
            xfrmThreshOriginal = ThresholdTransform;
            dblOriginalDecisionThreshold = DecisionThreshold;
            dblOriginalRegulatoryThreshold = RegulatoryThreshold;
            dblThreshExponentOriginal = ThresholdExponent;

            dblObsExponent = ObservationExponent;
            dblPredExponent = PredictionExponent;
            dblThreshExponent = ThresholdExponent;

            scatterPlot.UpdateResults(GetObsPredData());
            scatterPlot.Refresh();      
        }


        private void scatterPlot_ReplotRequested(object sender, EventArgs args)
        {
            scatterPlot.UpdateResults(GetObsPredData());
        }


        //When toggling between raw predictions and probabilities, we must reset the raw prediction thresholds
        //to the values they took when this window was opened. This is bedcause the probabilities are based on
        //the chance of exceeding those original thresholds.
        private void scatterPlot_PlotModeToggled(object sender, EventArgs args)
        {
            scatterPlot.SetThresholds(dblOriginalDecisionThreshold, dblOriginalRegulatoryThreshold, scatterPlot.ProbabilityThreshold);
            scatterPlot.PowerExponent = dblThreshExponentOriginal;
            scatterPlot.Transform = xfrmThreshOriginal.ToString();

            xfrmThresh = xfrmThreshOriginal;
            dblThreshExponent = dblThreshExponentOriginal;

            scatterPlot.UpdateResults(GetObsPredData());
            scatterPlot.Refresh();
        }


        //return a list of pred data
        private DataTable GetObsPredData()
        {
            DataTable plotdata = new DataTable();
            plotdata.Columns.Add("key", typeof(string));
            plotdata.Columns.Add("observed", typeof(double));
            plotdata.Columns.Add("predicted", typeof(double));

            if ((dtObs == null) || (dtStats == null))
                return null;
                                   
            List<string> lstStatsKeys = new List<string>();
            foreach (DataRow row in dtStats.Rows) {
                try
                {
                    lstStatsKeys.Add(row[0].ToString());
                }
                catch {}
            }

            List<string> lstObsKeys = new List<string>();
            foreach (DataRow row in dtObs.Rows) {
                try
                {
                    lstObsKeys.Add(row[0].ToString());
                }
                catch {}
            }

            if (lstStatsKeys.Count < 1)
                return null;

            List<double[]> lstData = new List<double[]>();
            DataRow plotrow = null;
            for (int i = 0; i < lstStatsKeys.Count; i++)
            {
                if (lstObsKeys.Contains(lstStatsKeys[i]))
                {
                    if (!Convert.IsDBNull(dtObs.Rows[lstObsKeys.IndexOf(lstStatsKeys[i])][strObservationColumn]))
                    {
                        int j = lstObsKeys.IndexOf(lstStatsKeys[i]);
                        string key = lstStatsKeys[i].ToString();

                        plotrow = plotdata.NewRow();
                        plotrow[0] = key;
                        plotrow[1] = VBCommon.Transforms.Apply.TransformThreshold(VBCommon.Transforms.Apply.UntransformThreshold(Convert.ToDouble(dtObs.Rows[j][strObservationColumn]), xfrmObs, dblObsExponent), xfrmPred, dblPredExponent);
                        if (scatterPlot.RawPredictions) { plotrow[2] = Convert.ToDouble(dtStats.Rows[i][strPredictionColumn]); }
                        else { plotrow[2] = Convert.ToDouble(dtStats.Rows[i][strProbabilityColumn]); }
                        plotdata.Rows.Add(plotrow);
                    }
                }
            }
            return plotdata;
        }

        
        //close the plot form
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        //update plot
        private void frmMLRPredObs_Load(object sender, EventArgs e)
        {
            scatterPlot.UpdateResults(GetObsPredData());
        }
    }
}
