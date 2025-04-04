using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using RCommon;

namespace GBMPlugin
{
    [Export(typeof(VBCommon.Interfaces.IModel))]
    [ExportMetadata("PluginKey", "GBM")]
    class GBMPlugin : RModelingPlugin, VBCommon.Interfaces.IModel
    {
        public GBMPlugin()
        {
            base.strPanelKey = "GBMPanel";
            base.strPanelCaption = "GBM";
        }

        public override void Activate()
        {
            modelingControl = new GBMControl.GBMGui();
            base.Activate();
        }

        public List<double> Predict(System.Data.DataSet dsPredictionData, double RegulatoryThreshold, double DecisionThreshold, VBCommon.Transforms.DependentVariableTransforms ThresholdTransform, double ThresholdPowerExponent) { return (modelingControl.Predict(dsPredictionData, RegulatoryThreshold, DecisionThreshold, ThresholdTransform, ThresholdPowerExponent)); }
        public List<double> PredictExceedanceProbability(System.Data.DataSet dsPredictionData, double RegulatoryThreshold, double DecisionThreshold, VBCommon.Transforms.DependentVariableTransforms ThresholdTransform, double ThresholdPowerExponent) { return (modelingControl.PredictExceedanceProbability(dsPredictionData, RegulatoryThreshold, DecisionThreshold, ThresholdTransform, ThresholdPowerExponent)); }
        public string ModelString() { return (modelingControl.ModelString()); }
    }
}
