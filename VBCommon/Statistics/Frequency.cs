using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics;
using MathNet.Numerics.Statistics;

namespace VBCommon.Statistics
{
    public class Frequency
    {
        private double[] _freq = null;
        private double [] _lb = null;
        private double [] _ub = null;
        private double[] _center = null;

        public void getHist(double[] data, int bins)
        {
            Histogram hist = new Histogram(data:data, nbuckets:bins);

            _freq = new double[bins];
            _lb = new double[bins];
            _ub = new double[bins];
            _center = new double[bins];
            double inc = ( ( (data.Max() - data.Min()) / bins) / 2.0d);
            for (int bin = 0; bin < hist.BucketCount; bin++)
            {
                _freq[bin] = Convert.ToInt32(hist[bin].Count);
                _lb[bin] = hist[bin].LowerBound;
                _ub[bin] = hist[bin].UpperBound;
                _center[bin] = _lb[bin] + inc;
            }
        }

        public double[] Counts
        {
            get { return _freq; }
        }
        public double[] LowerBounds
        {
            get { return _lb; }
        }
        public double[] UpperBounds
        {
            get { return _ub; }
        }
        public double [] Center
        {
         get { return _center; }
        }
    }
}
