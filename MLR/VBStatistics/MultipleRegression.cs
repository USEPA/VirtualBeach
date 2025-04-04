using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
//using Extreme.Statistics;
//using Extreme.Mathematics.LinearAlgebra;
//using Extreme.Statistics.Tests;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;

namespace VBCommon.Statistics
{
    public class MultipleRegression
    {
        Vector<double> _coefs;

        private DataTable _dataTable = null;
        private string _dependentVar = "";
        private string[] _independentVars = null;
        private double _adjR2;
        private double _R2;
        private double _AIC;
        private double _AICC;
        private double _BIC;
        private double _Press;
        private double _RMSE;

        private double[] _studentizedResiduals = null;
        private double[] _dffits = null;
        private double[] _cooks = null;
        private DataTable _parameters = null;
        private double[] _predictedValues = null;
        private double[] _observedValues = null;

        private double[] arrOutputData = null;
        private double[][] arrInputData = null;
        private string strOutputName;
        private string strInputName;

        private Dictionary<string, double> _VIF = null;
        private double _maxVIF = 0;
        private string _maxVIFParameter = "";
        private double _eigenvalueRatio = 1;

        private double _ADresidPvalue = double.NaN;
        private double _ADresidNormStatVal = double.NaN;

        private double _WSresidPvalue = double.NaN;
        private double _WSresidNormStatVal = double.NaN;


        public MultipleRegression() { }


        public MultipleRegression(DataTable dataTable, string dependentVariable, string[] independentVariables)
        {
            Populate(dataTable, dependentVariable, independentVariables);
        }


        public MultipleRegression(double[] OutputData, double[][] InputData, string OutputName = "", string InputName = "")
        {
            arrOutputData = OutputData;
            arrInputData = InputData;

            strOutputName = OutputName;
            strInputName = InputName;

            _independentVars = new string[1] { InputName };
        }


        public MultipleRegression(double[] OutputData, double[] InputData, string OutputName = "", string InputName = "")
        {
            //Set up the DataTable with two columns:
            _dataTable = new DataTable();
            _dataTable.Columns.Add(OutputName, typeof(double));
            try {  _dataTable.Columns.Add(InputName, typeof(double)); }
            catch (DuplicateNameException) { _dataTable.Columns.Add(InputName + ".", typeof(double)); }

            arrOutputData = OutputData;
            arrInputData = new double[InputData.Length][];
            for (int i = 0; i < InputData.Length; i++)
            {
                arrInputData[i] = new double[] { InputData[i] };
                _dataTable.LoadDataRow(new object[] { OutputData[i], InputData[i] }, true);
            }

            strOutputName = OutputName;
            strInputName = InputName;

            _independentVars = new string[1] { InputName };
            strOutputName = _dependentVar = OutputName;
        }


        public Dictionary<string, object> PackState()
        {
            Dictionary<string, object> dictPackedState = new Dictionary<string,object>();
            VBCommon.SerializationUtilities.SerializeDataTable(Data: _dataTable, Container: dictPackedState, Slot: "Data");
            dictPackedState.Add("OutputName", strOutputName);
            dictPackedState.Add("InputNames", _independentVars.ToList<string>());

            return dictPackedState;
        }


        public void UnpackState(Dictionary<string, object> PackedState)
        {
            if (PackedState.ContainsKey("Data") && PackedState.ContainsKey("InputNames") && PackedState.ContainsKey("OutputName"))
            {
                DataTable dt = VBCommon.SerializationUtilities.DeserializeDataTable(PackedState, "Data"); //(dt, PackedState, "Data");
                
                string output = PackedState["OutputName"].ToString();
                string[] inputs = (PackedState["InputNames"] as Newtonsoft.Json.Linq.JArray).ToObject<List<string>>().ToArray();
                Populate(dt, output, inputs);
                Compute();
            }
        }


        private void Populate(DataTable dataTable, string dependentVariable, string[] independentVariables)
        {
            _dataTable = dataTable;
            strOutputName = _dependentVar = dependentVariable;
            _independentVars = independentVariables;

            List<double> listOutputData = new List<double>();
            for (int i=0; i<dataTable.Rows.Count; i++)
            {
                listOutputData.Add((double)(dataTable.Rows[i][dependentVariable]));
            }
            arrOutputData = listOutputData.ToArray();
            arrInputData = new double[dataTable.Rows.Count][];

            Dictionary<int, int> dictColMap = new Dictionary<int, int>();
            for (int k = 0; k < independentVariables.Length; k++)
            {
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    if (dataTable.Columns[j].Caption == independentVariables[k])
                        dictColMap.Add(k, j);
                }
            }

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                double[] temp = new double[independentVariables.Length];
                for (int k = 0; k < independentVariables.Length; k++)
                {
                    temp[k] = Convert.ToDouble(dataTable.Rows[i].ItemArray[dictColMap[k]]);
                }
                arrInputData[i] = temp;
            }
        }


        public DataTable Data
        {
            get { return _dataTable; }
        }


        public double R2
        {
            get { return _R2; }
        }

        public double AdjustedR2
        {
            get { return _adjR2; }
        }

        public double AIC
        {
            get { return _AIC; }
        }

        public double AICC
        {
            get { return _AICC; }
        }

        public double BIC
        {
            get { return _BIC; }
        }

        public double Press
        {
            get { return _Press; }
        }

        public double RMSE
        {
            get { return _RMSE; }
        }

        public double[] DFFITS
        {
            get { return _dffits; }
        }

        public double[] Cooks
        {
            get { return _cooks; }
        }

        public double[] StudentizedResiduals
        {
            get { return _studentizedResiduals; }
        }

        public DataTable Parameters
        {
            get { return _parameters; }
        }

        public double[] PredictedValues
        {
            get { return _predictedValues; }
        }

        public double[] ObservedValues
        {
            get { return _observedValues; }
        }

        public double ADResidPvalue 
        {
            get { return _ADresidPvalue; }
        }

        public double ADResidNormStatVal
        {
            get { return _ADresidNormStatVal; }
        }

        public double WSResidPvalue
        {
            get { return _WSresidPvalue; }
        }

        public double WSResidNormStatVal
        {
            get { return _WSresidNormStatVal; }
        }

        public double MaxVIF
        {
            get { return _maxVIF; }
        }

        public double EigenvalueRatio
        {
            get { return _eigenvalueRatio; }
        }

        public void Compute()
        {
            // Now create the regression model. Parameters are the name 
            // of the dependent variable, a string array containing 
            // the names of the independent variables, and the VariableCollection
            // containing all variables.
            int n = arrInputData.Length;
            int p = arrInputData[0].Length;

            double[] tmpIntercept = new double [n];
            for (int i=0; i<n; i++)
                tmpIntercept[i] = 1;
            Vector<double> intercept = Vector<double>.Build.DenseOfArray(tmpIntercept);

            Matrix<double> InputData = Matrix<double>.Build.DenseOfRowArrays(arrInputData);
            Matrix<double> X = InputData.InsertColumn(0, intercept);
            Vector<double> Y = Vector<double>.Build.DenseOfArray(arrOutputData);
            QR<double> qr = X.QR();
            Vector<double> coefs = _coefs = qr.Solve(Y);
            Vector<double> fits = X * coefs;
            Vector<double> Residuals = Y - fits;
            double sse = Residuals * Residuals;
            double meany = Y.Average();
            double sst = (Y - meany) * (Y - meany);
            
            Matrix<double> Rm = qr.R.Inverse();
            Matrix<double> RmT = Rm.Transpose();
            
            //Make sure the intercept appears first in the list of results:
            List<string> inputNames = new List<string>();
            inputNames.Add("(Intercept)");
            foreach (string varname in _independentVars)
            {
                inputNames.Add(varname);
            }

            //Compute some summary statistics
            double sigma2 = sse / (n - p);
            _R2 = (sst - sse) / sst;
            _adjR2 = _R2 - (1 - _R2) * 2 / (n - 3);      
            _RMSE = Math.Sqrt(sse / (n-p-1));
            
            //Calculate the selection criteria            
            double[] SquaredResiduals = new double[n];
            double[] Leverage = new double[n];
            double[] L2 = new double[n];
            for (int i=0; i<n; i++)
            {
                SquaredResiduals[i] = Math.Pow(Residuals[i], 2);
                Leverage[i] = qr.Q.Row(i) * qr.Q.Row(i);
            }

            double[] ExternallyStudentizedResiduals = new double[n];
            _dffits = new double[n];
            _cooks = new double[n];
            for (int i=0; i<n; i++)
            {
                ExternallyStudentizedResiduals[i] = Residuals[i] / Math.Sqrt((sse - SquaredResiduals[i]) / (n - p - 1) * (1 - Leverage[i]));
                _dffits[i] = ExternallyStudentizedResiduals[i] * Math.Sqrt(Leverage[i] / (1 - Leverage[i]));
                _cooks[i] = SquaredResiduals[i] / (p * sse / (n-p-2)) * Leverage[i] / Math.Pow((1 - Leverage[i]), 2);
            }

            _AIC = n * Math.Log(sse / n) + (2 * p) + n + 2;
            _AICC = _AIC + (2 * (p + 1) * (p + 2)) / (n - p - 2);
            _BIC = n * (Math.Log(sse / n)) + (p * Math.Log(n));
                        
            _Press = 0.0;
            double leverage = 0.0;
            for (int i = 0; i <n; i++)
            {
                leverage = Math.Min(Leverage[i], 0.99);                
                _Press += Math.Pow((Residuals[i]) / (1 - leverage),2);
            }
            
            _parameters = createParametersDataTable();
            DataRow dr = null;

            //T-test p-value:
            StudentT tDist = new StudentT(location: 0, scale: 1, freedom: n - p - 1);
            for (int i = 0; i <= p; i++)
            {
                //find the standard error of this coefficient
                double S2 = sigma2 * (Rm.Row(i) * Rm.Row(i));

                dr = _parameters.NewRow();
                dr["Name"] = inputNames[i];
                dr["Value"] = coefs[i];
                dr["StandardError"] = Math.Sqrt(S2);
                dr["TStatistic"] = coefs[i] / Math.Sqrt(S2);
                dr["PValue"] = 2 * (1 - tDist.CumulativeDistribution(Math.Abs(coefs[i] / Math.Sqrt(S2))));
                dr["StandardizedCoefficient"] = getStandardCoeff(inputNames[i], coefs[i]);
                _parameters.Rows.Add(dr);
            }
            
            _predictedValues = fits.ToArray();            
            _observedValues = Y.ToArray();            
            _studentizedResiduals = ExternallyStudentizedResiduals;

            Normal distribution = new Normal(mean:0, stddev:1);

            double RSE = Math.Sqrt(sse / (n-1));
            double[] standardizedResid = new double[n];
            for (int i = 0; i<n; i++)
            {
                standardizedResid[i] = Residuals[i] / RSE;
            }
            Array.Sort(standardizedResid);

            //Anderson-Darling normality test for residuals:
            double AD_stat = 0;
            for (int i = 0; i<n; i++)
            {
                AD_stat += Convert.ToDouble(2 * i + 1) * (Math.Log(distribution.CumulativeDistribution(standardizedResid[i])) + Math.Log(1-distribution.CumulativeDistribution(standardizedResid[n-1-i])));
            }

            AD_stat = -Convert.ToDouble(n) - AD_stat / Convert.ToDouble(n);                        
            _ADresidNormStatVal = AD_stat;
            _ADresidPvalue = 1 - adinf(AD_stat);
            
            _VIF = ComputeVIFs(InputData);
            _maxVIF = _VIF.Values.Max(x => Math.Abs(x));
            _maxVIFParameter = _VIF.First(x => Math.Abs(x.Value)==_maxVIF).Key;

            //Check the ratio between the largest, smallest Eigenvalues. Too large => design matrix is singular.
            _eigenvalueRatio = X.ConditionNumber();
        }


        private Dictionary<string, double> ComputeVIFs(Matrix<double> CovariateMatrix)
        {
            //Convert the DesignMatrix to a Math.NET object:
            Matrix<double> X = CovariateMatrix.Clone();
            int n = X.RowCount;
            int p = X.ColumnCount;

            double[] tmpIntercept = new double [n];
            for (int i=0; i<n; i++)
                tmpIntercept[i] = 1;
            Vector<double> intercept = Vector<double>.Build.DenseOfArray(tmpIntercept);

            //Initialize the dictionary that'll hold the VIFs
            Dictionary<string, double> VIFs = new Dictionary<string, double>();
            
            //Compute VIFs: start the loop at 1 to skip the intercept's VIF
            for (int i=0; i<p; i++) 
            {
                try
                {
                    //Get the matrices prepared to build a linear model
                    Vector<double> Y = X.Column(i);
                    //double[,] YY = new double[n, 1].SetColumn(0, Y);
                    X.SetColumn(i, intercept);
                    Vector<double> b = MathNet.Numerics.LinearRegression.MultipleRegression.NormalEquations<double>(x: X, y: Y);
                    Vector<double> fits = X * b;

                    //Replace the column we just cleared:
                    X.SetColumn(i, Y);

                    //Compute the sums of squares for the full, intercept-only models
                    double meany = Y.Average();
                    double sst = (Y - meany) * (Y - meany);
                    double ssr = (Y - fits) * (Y - fits);
                    double R2 = 1 - ssr / sst;

                    //Calculate the VIF for this variable and add it to the list.
                    VIFs.Add(_independentVars[i].ToString(), 1/(1-R2));
                }
                catch
                {
                    //If we can't calculate the VIF (e.g. because the design matrix is singular), then call it infinite
                    VIFs.Add(_independentVars[i-1].ToString(), Double.PositiveInfinity);
                }
            }
            return VIFs;
        }


        private double adinf(double z)
        {
            /* Short, practical version of full ADinf(z), z>0.   */
            if (z < 2)
                return Math.Exp(-1.2337141/z) / Math.Sqrt(z)*(2.00012+(.247105-(.0649821-(.0347962-(.011672-.00168691*z)*z)*z)*z)*z);
            else
                return Math.Exp(-Math.Exp(1.0776-(2.30695-(.43424-(.082433-(.008056 -.0003146*z)*z)*z)*z)*z));
        }


        private double getStandardCoeff(string paramName, double coeff)
        {
            //throw new NotImplementedException();
            if (paramName == "(Intercept)") return double.NaN;

            List<double> listY = new List<double>();
            for (int i = 0; i < _dataTable.Rows.Count; i++)
            {
                listY.Add((double)(_dataTable.Rows[i][_dependentVar]));
            }
            double stdevY = listY.StandardDeviation();

            List<double> listX = new List<double>();
            for (int i = 0; i < _dataTable.Rows.Count; i++)
            {
                listX.Add((double)(_dataTable.Rows[i][paramName]));
            }
            double stdevX = listX.StandardDeviation();

            return coeff * stdevX / stdevY;
        }


        public double Predict(DataRow independentValues)
        {
            Vector<double> X;
            double[] indVals = new double[_independentVars.Length + 1];
            
            //Intercept is the first entry, then the covariates:
            indVals[0] = 1;
            for (int i = 1; i < _independentVars.Length; i++)
            {
                indVals[i] = Convert.ToDouble(independentValues[_independentVars[i]]);
            }

            X = Vector<double>.Build.DenseOfArray(indVals);
            return Predict(X);
        }


        public double Predict(Vector<double> covariates)
        {
            return _coefs * covariates;
        }


        private DataTable createParametersDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Name",typeof(string));
            dt.Columns.Add("Value", typeof(double));
            dt.Columns.Add("StandardError", typeof(double));
            dt.Columns.Add("TStatistic", typeof(double));
            dt.Columns.Add("PValue", typeof(double));
            dt.Columns.Add("StandardizedCoefficient", typeof(double));

            return dt;
        }


        public Dictionary<string, double> Model
        {
            get
            {
                Dictionary<string, double> parameters = new Dictionary<string, double>();
                for (int i = 0; i < _parameters.Rows.Count; i++)
                {
                    parameters.Add(_parameters.Rows[i][0].ToString(), Convert.ToDouble(_parameters.Rows[i][1]));
                }

                return parameters;
            }
        }        
    }
}
