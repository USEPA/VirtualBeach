using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;

namespace VBCommon.Statistics
{
    public class Statistics
    {
        private static double[,] _XpI = null;

        /// <summary>
        ///ProbExceedFits = NormalDistribution(prediction, threshold, fittedSE) where
        ///fittedSE = modelRMSE * SQR(x' * (X' * X)inv * x) where
        /// x is vector of variable values used in making a mlr fit (drVars)
        /// X is matrix of variable values in model datatable (dtCorr)
        /// (x' and X' are transposes of x and X respectively, and (X'X)inv is the inverse of the product of these matricies)
        /// </summary>
        /// <param name="drVars">values for fit vector</param>
        /// <param name="dtCorr">table of model variable values</param>
        /// <param name="prediction">value of the fit</param>
        /// <param name="threshold">value of the threshold</param>
        /// <param name="modelRMSE">model RMSE value</param>
        /// <param name="newModel">passed bool indicates if recompute of X'X matrix is required (yes if another model selected)</param>
        /// <returns>double, probability of exceedance for fitted values</returns>
        public static double[] PExceedFits(DataTable dtCorr, double[] predictions, double threshold, double modelRMSE)
        {
            try
            {
                //Create the design matrix (augmented by the intercept column)
                int p = dtCorr.Columns.Count + 1;
                int n = dtCorr.Rows.Count;
                double[][] X1 = new double[n][];
                Normal distStandardNormal = new Normal(0, 1);
                for (int i = 0; i < n; i++)
                {
                    DataRow row = dtCorr.Rows[i];
                    double[] drow = ((IList<object>)row.ItemArray).Cast<double>().ToArray();
                    double[] xrow = new double[p];
                    xrow[0] = 1;
                    for (int j = 1; j < p; j++)
                    {
                        xrow[j] = drow[j - 1];
                    }
                    X1[i] = xrow;
                }

                List<int> indx = Enumerable.Range(0, n).ToList<int>();
                Matrix<double> X = Matrix<double>.Build.DenseOfRowArrays(X1);
                QR<double> qr = X.QR();
                //double[,] H = X.Multiply((X.Transpose().Multiply(X)).Inverse().Multiply(X.Transpose()));
                List<double> diag = indx.Select(i => qr.Q.Row(i) * qr.Q.Row(i)).ToList<double>();
                List<double> std = diag.Select(y => modelRMSE * (Math.Sqrt(y))).ToList<double>();
                double[] prob = indx.Select(k => distStandardNormal.CumulativeDistribution((predictions[k] - threshold) / std[k]) * 100.0d).ToArray<double>();
                return prob;
            }
            catch (Exception e)
            {
                Console.WriteLine("Fitted P(Excced) calculation error via Statistics class: " + e.Message.ToString());
                return Enumerable.Repeat(double.NaN, dtCorr.Rows.Count).ToArray<double>();
            }
        }


        /// <summary>
        /// this doc is crap - updated by w.brooks sometime is the past
        ///ProbExceedPrediction = NormalDistribution(prediction, threshold, sdPrediction) where
        ///sdPrediction = modelRMSE * SQR( 1 + (x' * (X' * X)inv * x)) where
        /// x is vector of variable values used in making a prediction (drVars)
        /// X is matrix of variable values in model datatable (dtCorr)
        /// (x' and X' are transposes of x and X respectively, and (X'X)inv is the inverse of the product of these matricies)
        /// </summary>
        /// <param name="drVars">values for prediction vector</param>
        /// <param name="dtCorr">table of model variable values</param>
        /// <param name="prediction">value of the prediction</param>
        /// <param name="threshold">value of the threshold</param>
        /// <param name="modelRMSE">model RMSE value</param>
        /// <param name="newModel">passed bool indicates if recompute of X'X matrix is required (yes if another model selected)</param>
        /// <returns>double, probability of exceedance of prediction</returns>
        public static List<double> PExceed(DataTable dtModelData, double[] predictions, double threshold, double modelRMSE, DataTable dtPredData=null)
        {
            try
            {
                //Create the design matrix (augmented by the intercept column)
                int p = dtModelData.Columns.Count + 1;
                int n = dtModelData.Rows.Count;
                double[][] X1 = new double[n][];
                Matrix<double> X, P, XR, PR, XRm, PRm;

                for (int i = 0; i < n; i++)
                {
                    DataRow row = dtModelData.Rows[i];
                    double[] drow = ((IList<object>)row.ItemArray).Cast<double>().ToArray();
                    double[] xrow = new double[p];
                    xrow[0] = 1;
                    for (int j = 1; j < p; j++)
                    {
                        xrow[j] = drow[j - 1];
                    }
                    X1[i] = xrow;
                }
                X = Matrix<double>.Build.DenseOfRowArrays(X1);
                XR = X.QR().R;
                XRm = XR.Inverse();

                //If we're predicting on different data than was used in model building, then get the prediction data in to the proper form:
                //if (dtPredData != null && (dtPredData.Columns.Count <= (dtPredData.Rows.Count - 1)))
                if (dtPredData != null)
                {
                        if (dtPredData.Columns.Count != dtModelData.Columns.Count)
                        {
                            Exception err = new Exception(message: "The number of columns for the prediction data does not match the number of columns for the model data, as it must.");
                            throw err;
                        }

                        n = dtPredData.Rows.Count;
                        double[][] P1 = new double[n][];
                        for (int i = 0; i < n; i++)
                        {
                            DataRow row = dtPredData.Rows[i];
                            double[] drow = ((IList<object>)row.ItemArray).Cast<double>().ToArray();
                            double[] xrow = new double[p];
                            xrow[0] = 1;
                            for (int j = 1; j < p; j++)
                            {
                                xrow[j] = drow[j - 1];
                            }
                            P1[i] = xrow;
                        }
                        P = Matrix<double>.Build.DenseOfRowArrays(P1);

                    // why are we even doing this? P.QR().R fails for small n (<10) but why solve at all?
                        //PR = P.QR().R;
                        //PRm = PR.Inverse();

                }
                else
                {
                    P = X;
                    PR = XR;
                    PRm = XRm;
                }

                Normal distStandardNormal = new Normal(0, 1);
                List<int> indx = Enumerable.Range(0, n).ToList<int>();
                Matrix<double> precision = XRm * XRm.Transpose(); // H = P * (XRm * XRm.Transpose()) * P.Transpose()));
                List<double> diag = indx.Select(i => P.Row(i) * precision * P.Row(i)).ToList<double>();
                List<double> std = diag.Select(y => modelRMSE * (Math.Sqrt(1+ y))).ToList<double>();
                List<double> prob = indx.Select(k => distStandardNormal.CumulativeDistribution((predictions[k] - threshold) / std[k]) * 100.0d).ToList<double>();

                return prob;
            }

            catch (Exception e)
            {
                Console.WriteLine("Prediction P(Exceed) calculation error via Statistics class: " + e.Message.ToString());
                return Enumerable.Repeat(double.NaN, dtModelData.Rows.Count).ToList<double>();
            }
        }

        public static double[] PExceedPrediction(DataTable dtModelData, double[] predictions, double threshold, double modelRMSE, DataTable dtPredData = null)
        {
            try
            {
                //Create the design matrix (augmented by the intercept column)
                int p = dtModelData.Columns.Count + 1;
                int n = dtModelData.Rows.Count;
                double[][] X1 = new double[n][];
                Matrix<double> X, P, XR, PR, XRm, PRm;

                for (int i = 0; i < n; i++)
                {
                    DataRow row = dtModelData.Rows[i];
                    double[] drow = ((IList<object>)row.ItemArray).Cast<double>().ToArray();
                    double[] xrow = new double[p];
                    xrow[0] = 1;
                    for (int j = 1; j < p; j++)
                    {
                        xrow[j] = drow[j - 1];
                    }
                    X1[i] = xrow;
                }
                X = Matrix<double>.Build.DenseOfRowArrays(X1);
                XR = X.QR().R;
                XRm = XR.Inverse();

                //If we're predicting on different data than was used in model building, then get the prediction data in to the proper form:
                //if (dtPredData != null && (dtPredData.Columns.Count <= (dtPredData.Rows.Count - 1)))
                if (dtPredData != null)
                {
                    if (dtPredData.Columns.Count != dtModelData.Columns.Count)
                    {
                        Exception err = new Exception(message: "The number of columns for the prediction data does not match the number of columns for the model data, as it must.");
                        throw err;
                    }

                    n = dtPredData.Rows.Count;
                    double[][] P1 = new double[n][];
                    for (int i = 0; i < n; i++)
                    {
                        DataRow row = dtPredData.Rows[i];
                        double[] drow = ((IList<object>)row.ItemArray).Cast<double>().ToArray();
                        double[] xrow = new double[p];
                        xrow[0] = 1;
                        for (int j = 1; j < p; j++)
                        {
                            xrow[j] = drow[j - 1];
                        }
                        P1[i] = xrow;
                    }
                    P = Matrix<double>.Build.DenseOfRowArrays(P1);

                    // why are we even doing this? P.QR().R fails for small n (<10) but why solve at all?
                    //PR = P.QR().R;
                    //PRm = PR.Inverse();

                }
                else
                {
                    P = X;
                    PR = XR;
                    PRm = XRm;
                }

                Normal distStandardNormal = new Normal(0, 1);
                List<int> indx = Enumerable.Range(0, n).ToList<int>();
                Matrix<double> precision = XRm * XRm.Transpose(); // H = P * (XRm * XRm.Transpose()) * P.Transpose()));
                List<double> diag = indx.Select(i => P.Row(i) * precision * P.Row(i)).ToList<double>();
                List<double> std = diag.Select(y => modelRMSE * (Math.Sqrt(1 + y))).ToList<double>();
                //List<double> prob = indx.Select(k => distStandardNormal.CumulativeDistribution((predictions[k] - threshold) / std[k]) * 100.0d).ToList<double>();
                double[] prob = indx.Select(k => distStandardNormal.CumulativeDistribution((predictions[k] - threshold) / std[k]) * 100.0d).ToArray<double>();

                return prob;
            }

            catch (Exception e)
            {
                Console.WriteLine("Prediction P(Exceed) calculation error via Statistics class: " + e.Message.ToString());
                //return Enumerable.Repeat(double.NaN, dtModelData.Rows.Count).ToList<double>();
                return Enumerable.Repeat(double.NaN, dtModelData.Rows.Count).ToArray<double>();
            }
        }


        public static DataTable getPearsonCorrCoeffs(DataTable data)
        {
            DataTable dtCoeff = new DataTable();
            dtCoeff.Columns.Add("Variable1");
            dtCoeff.Columns.Add("Variable2");
            dtCoeff.Columns.Add("PearsonCorrelation", Type.GetType("System.Decimal"));

            if ((data == null) || (data.Rows.Count == 0) || (data.Columns.Count < 3))
            {
                return dtCoeff; 
            }
            Hashtable htVarValues = new Hashtable();
            double[] arColumn;

            for (int intColumnIndex = 2; intColumnIndex < data.Columns.Count; intColumnIndex++)
            {
                arColumn = new double[data.Rows.Count];
                int count = 0;
                foreach (DataRow dr in data.Rows)
                {
                    arColumn.SetValue(Convert.ToDouble(dr[intColumnIndex].ToString()), count);
                    count++;
                }
                htVarValues.Add(intColumnIndex, arColumn);
            }
            string strCoeff = "";
            int intLength = 0;
            for (int intFirstColumnIndex = 2; intFirstColumnIndex < (data.Columns.Count - 1); intFirstColumnIndex++)
            {
                double[] var1Values = (double[])htVarValues[intFirstColumnIndex];
                for (int intSecondColumnIndex = (intFirstColumnIndex + 1); intSecondColumnIndex < data.Columns.Count; intSecondColumnIndex++)
                {
                    double[] var2Values = (double[])htVarValues[intSecondColumnIndex];
                    double coeff = Correlation(var1Values, var2Values);
                    coeff = Math.Abs(coeff);
                    if (coeff < 0.001)
                    {
                        coeff = 0;
                    }
                    strCoeff = coeff.ToString();
                    intLength = strCoeff.Length;
                    if (intLength > 8)
                    {
                        intLength = 8;
                    }
                    strCoeff = strCoeff.Substring(0, intLength);
                    coeff = Convert.ToDouble(strCoeff);
                    DataRow dr = dtCoeff.NewRow();
                    dr["Variable1"] = data.Columns[intFirstColumnIndex].ColumnName;
                    dr["Variable2"] = data.Columns[intSecondColumnIndex].ColumnName;
                    dr["PearsonCorrelation"] = coeff;
                    dtCoeff.Rows.Add(dr);
                }
            }
            dtCoeff = sortTable(dtCoeff, "PearsonCorrelation", "DESC");
            return dtCoeff;
        }


        public static DataTable sortTable(DataTable dtUnsorted, string sortColumn, string sortDirection)
        {
            string sortFormat = "{0} {1}";
            dtUnsorted.DefaultView.Sort = string.Format(sortFormat, sortColumn, sortDirection);
            return dtUnsorted.DefaultView.Table;
        }


        public static double Correlation(double[] deparray, double[] vararray)
        {
            double correlation = deparray.Covariance(vararray) / (deparray.StandardDeviation() * vararray.StandardDeviation());
            return correlation;
        }
        

        /// <summary>
        /// return a 2-tailed p-value for the t distribution of the pearson correlation score
        /// <param name="pcoeff">pearson score for the variable relative to the dependent variable</param>
        /// <param name="n">number of observations minus 2</param>
        /// <returns></returns>
        public static double Pvalue4Correlation(double pcoeff, int n)
        {
            double Pval = double.NaN;
            //changed 11/21/2014 by mog - pval blowsup for perfect correlations (pcoeff>=1.0), returns NaN
            if (pcoeff >= 1.0) return Pval;

            int degreeF = n - 2;
            double tscore = pcoeff / Math.Sqrt((1 - Math.Pow(pcoeff, 2.0d)) / degreeF);
            StudentT tDist = new StudentT(location:0, scale: 1, freedom: degreeF);

            if (tscore < 0)
            {
                Pval = 2 * tDist.CumulativeDistribution(tscore);
            }
            else
            {
                Pval = 2 * (1 - tDist.CumulativeDistribution(tscore));
            }

            return Pval;
        }
    }       
}
