using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using RCommon;

namespace PLSPlugin
{
    [Export(typeof(VBCommon.Interfaces.IModel))]
    [ExportMetadata("PluginKey", "PLS")]
    class PLSPlugin : RModelingPlugin, VBCommon.Interfaces.IModel
    {
        public PLSPlugin()
        {
            base.strPanelKey = "PLSPanel";
            base.strPanelCaption = "PLS";
        }

        public override void Activate()
        {
            modelingControl = new PLSControl.PLSGui();
            base.Activate();
        }


        public List<double> Predict(System.Data.DataSet dsPredictionData, double RegulatoryThreshold, double DecisionThreshold, VBCommon.Transforms.DependentVariableTransforms ThresholdTransform, double ThresholdPowerExponent) { return (modelingControl.Predict(dsPredictionData, RegulatoryThreshold, DecisionThreshold, ThresholdTransform, ThresholdPowerExponent)); }
        public List<double> PredictExceedanceProbability(System.Data.DataSet dsPredictionData, double RegulatoryThreshold, double DecisionThreshold, VBCommon.Transforms.DependentVariableTransforms ThresholdTransform, double ThresholdPowerExponent) { return (modelingControl.PredictExceedanceProbability(dsPredictionData, RegulatoryThreshold, DecisionThreshold, ThresholdTransform, ThresholdPowerExponent)); }
        public string ModelString() { return (modelingControl.ModelString()); }
    }
}
