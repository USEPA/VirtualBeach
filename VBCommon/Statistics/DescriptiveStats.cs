using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using MathNet.Numerics;
using MathNet.Numerics.Statistics;

namespace VBCommon.Statistics
{
    public class DescriptiveStats
    {
        //private NumericalVariable _stats = null;
        private double _max;
        private double _min;
        private double _mean;
        private double _stddev;
        private double _kurtosis;
        private double _skewness;
        private double _range;
        private double _variance;
        private double _sum;
        private int _count;
        private double _median;

        public void getStats(double[] data) 
        {
            DescriptiveStatistics descriptiveStats = new DescriptiveStatistics(data);
            
            _max = descriptiveStats.Maximum;
            _min = descriptiveStats.Minimum;
            _mean = descriptiveStats.Mean;
            _stddev = descriptiveStats.StandardDeviation;
            _kurtosis = descriptiveStats.Kurtosis;
            _skewness = descriptiveStats.Skewness;
            _range = _max - _min;
            _variance = descriptiveStats.Variance;
            _sum = data.Sum();
            _count = data.Count();
            _median = data.Median();

        }


        public double Max
        {
            get { return _max; }
        }
        public double Min
        {
            get { return _min; }
        }
        public double Mean
        {
            get { return _mean; }
        }
        public double StdDev
        {
            get { return _stddev; }
        }
        public double Kurtosis
        {
            get { return _kurtosis; }
        }
        public double Range
        {
            get { return _range; }
        }
        public double Skewness
        {
            get { return _skewness; }
        }
        public double Variance
        {
            get { return _variance; }
        }
        public double Sum
        {
            get { return _sum; }
        }
        public int Count
        {
            get { return _count; }
        }
        public double Median
        {
            get { return _median; }
        }
    }
}
