using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Accord;
//using Accord.Statistics;
//using Accord.Math;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.Distributions;

namespace VBCommon.Statistics
{
    public class AndersonDarlingNormality
    {
        //private double _adtest = double.NaN;
        private double _adStat = double.NaN;
        private double _adStatPval = double.NaN;
        private bool _reject;

        public void getADstat(double[] data)
        {
            try
            {
                //Anderson-Darling p-value interpolator:
                //MathNet.Numerics.Interpolation.CubicSpline interp = MathNet.Numerics.Interpolation.CubicSpline.InterpolateNatural(new double[5] { 0.576, 0.656, 0.787, 0.918, 1.092 }, new double[5] { 0.25, 0.1, 0.05, 0.025, 0.01 });
                //change by mog 11/21/2014 via mc due ref http://en.wikipedia.org/wiki/Anderson%E2%80%93Darling_test
                MathNet.Numerics.Interpolation.CubicSpline interp = MathNet.Numerics.Interpolation.CubicSpline.InterpolateNatural(new double[5] { 0.576, 0.656, 0.787, 0.918, 1.092 }, new double[5] { 0.15, 0.1, 0.05, 0.025, 0.01 });

                Vector<double> X = Vector<double>.Build.DenseOfArray(data);
                Sorting.Sort(X);
                Normal adDist = new Normal(mean: X.Mean(), stddev: X.StandardDeviation());
                double adStat = 0;
                int n = X.Count;
                double nD = Convert.ToDouble(n);
                for (int i = 0; i < n; i++)
                {
                    adStat += Convert.ToDouble(2 * (i + 1) - 1) / nD * (Math.Log(adDist.CumulativeDistribution(X[i])) + Math.Log(1 - adDist.CumulativeDistribution(X[n - i - 1])));
                }

                _adStat = (-n - adStat) * (1.0 + 4.0/nD - 25.0/Math.Pow(n,2));
                _adStatPval = interp.Interpolate(_adStat) > 0 ? interp.Interpolate(_adStat) : 0;
                _reject = interp.Interpolate(_adStat) > 0.05 ? false : true;

                ////Anderson-Darling p-value interpolator:
                //MathNet.Numerics.Interpolation.CubicSpline interp = MathNet.Numerics.Interpolation.CubicSpline.InterpolateNatural(new double[5] { 0.576, 0.656, 0.787, 0.918, 1.092 }, new double[5] { 0.25, 0.1, 0.05, 0.025, 0.01 });

                //Vector<double> X = Vector<double>.Build.DenseOfArray(data);
                //Sorting.Sort(X);
                //Normal adDist = new Normal(mean: X.Mean(), stddev: X.StandardDeviation());
                //double adStat = 0;
                //int n = X.Count;
                //double nD = Convert.ToDouble(n);
                //for (int i = 0; i < n; i++)
                //{
                //    adStat += Convert.ToDouble(2 * (i + 1) - 1) / nD * (Math.Log(adDist.CumulativeDistribution(X[i])) + Math.Log(1 - adDist.CumulativeDistribution(X[n - i - 1])));
                //}

                //_adStat = (-n - adStat) * (1.0 + 4.0/nD - 25.0/Math.Pow(n,2));
                //_adStatPval = interp.Interpolate(_adStat) > 0 ? interp.Interpolate(_adStat) : 0;
                //_reject = interp.Interpolate(_adStat) > 0.05 ? false : true;
            }
            catch
            {
                _adStat = double.NaN;
                _adStatPval = double.NaN;
                _reject = false;
            }

             
        }

        public double ADStat
        {
            get { return _adStat; }
        }

        public double ADStatPval
        {
            get { return _adStatPval; }
        }

        public bool RejectNHyp
        {
            get { return _reject; }
        }

    }
}
