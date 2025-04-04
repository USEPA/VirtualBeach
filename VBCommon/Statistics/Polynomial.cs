﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using VBCommon;


using System.Collections.Specialized;
using System.Collections;
using System.ComponentModel;
using System.Drawing;


using MathNet.Numerics;
using MathNet.Numerics.Statistics;

namespace VBCommon.Statistics
{
    public class Polynomial
    {
        //run a regression on y, x and x**2 and compute x(new) = intercept + c1*x + c2*x*x and adjustedR**2 fit
        //save x var name, intercept, c1, c2 and adjustedR**2 in memory (_polyDT) (for prediction) and 
        //return new []x (_polytransform) and fit (_rsqrd)


        private DataTable _modelDT = null;
        private double[] _polytransform = null;
        public double _adjrsqrd, _intercept, _c1, _c2, _rsqrd, _pearsonR;
        private string _colname = string.Empty;
        public static DataTable _polyDT = null;

        //constructor for procedural processing in datasheet
        public Polynomial(DataTable dt, int colndx, int depVarColIndex)
        {
            createResultsTable();
            _colname = dt.Columns[colndx].ColumnName;
            _modelDT = createModelTable(dt, colndx, depVarColIndex);
            MultipleRegression model = new MultipleRegression(_modelDT, "Y", new [] { "X", "X**2"} );
            model.Compute();
            DataTable result = model.Parameters;
            computePoly(result);
            _adjrsqrd = model.AdjustedR2;
            _rsqrd = model.R2;

            //pearson r
            double[] y = Utility.GetColumnFromTable(dt, depVarColIndex);
            _pearsonR = y.Covariance(_polytransform) / y.StandardDeviation() / _polytransform.StandardDeviation();

            savePolyInfo();
        }

        //constructor for manual processing (plotting)
        public Polynomial(double[] y, double[] x, string colname)
        {
            createResultsTable();
            _colname = colname;
            _modelDT = createModelTable(y, x);
            MultipleRegression model = new MultipleRegression(_modelDT, "Y", new[] { "X", "X**2" });
            model.Compute();
            DataTable result = model.Parameters;
            computePoly(result);
            _adjrsqrd = model.AdjustedR2;
            _rsqrd = model.R2;

            _pearsonR = y.Covariance(_polytransform) / y.StandardDeviation() / _polytransform.StandardDeviation();

            savePolyInfo();
        }


        private void createResultsTable()
        {
            //throw new NotImplementedException();
            if (_polyDT == null)
            {
                _polyDT = new DataTable();
                _polyDT.Columns.Add("colname", typeof(string));
                _polyDT.Columns.Add("intercept", typeof(double));
                _polyDT.Columns.Add("coeffX", typeof(double));
                _polyDT.Columns.Add("coeffXX", typeof(double));
                _polyDT.Columns.Add("adjR**2", typeof(double));
                _polyDT.Columns.Add("pearsonR", typeof(double));
            }
        }


        private void savePolyInfo()
        {
            //throw new NotImplementedException();
            _polyDT.Rows.Add(new Object [] {_colname, _intercept, _c1, _c2, _adjrsqrd, _pearsonR});
        }

        public double[] getPolyT
        {
            get { return _polytransform; }
        }

        public double getAdjRsqrd
        {
            get { return _adjrsqrd; }
        }

        public double getRsqrd
        {
            get { return _rsqrd; }
        }

        public DataTable getPolyInfo
        {
            get { return _polyDT; }
        }

        public double getPearsonR
        {
            get { return _pearsonR; }
        }

        private void computePoly(DataTable result)
        {
            //throw new NotImplementedException();
            _polytransform = new double [_modelDT.Rows.Count];            
            _intercept = (double)result.Rows[0]["Value"];
            _c1 = (double)result.Rows[1]["Value"];
            _c2 = (double)result.Rows[2]["Value"];
            int n = 0;
            foreach (DataRow r in _modelDT.Rows)
            { 
                _polytransform[n] = _intercept + _c1*(double)r.ItemArray[1] + _c2*(double)r.ItemArray[2];
                n++;
            }
        }


        private DataTable createModelTable(DataTable dt, int colndx, int depVarColIndex)
        {
            //throw new NotImplementedException();
            addModelTableCols();
            double[] y = Utility.GetColumnFromTable(dt, depVarColIndex);
            double [] x = Utility.GetColumnFromTable(dt, colndx);
            VBCommon.Transforms.Transformer t = new VBCommon.Transforms.Transformer(dt, colndx);
            double[] x2 = t.SQUARE; // t.REALSQUARE;
            //DataRow r;
            for (int i = 0; i < y.Length; i++)
                _modelDT.Rows.Add(new Object[] {y[i], x[i], x2[i]});
            return _modelDT;
        }

        private DataTable createModelTable(double[] y, double[] x)
        {
            addModelTableCols();
            VBCommon.Transforms.Transformer t = new VBCommon.Transforms.Transformer(x);
            double[] x2 = t.SQUARE;
            for (int i = 0; i < y.Length; i++)
                _modelDT.Rows.Add(new Object[] { y[i], x[i], x2[i] });
            return _modelDT;
        }


        private void addModelTableCols()
        {
            //throw new NotImplementedException();
            _modelDT = new DataTable("PolynomialData");
            _modelDT.Columns.Add("Y", typeof(double));
            _modelDT.Columns.Add("X", typeof(double));
            _modelDT.Columns.Add("X**2", typeof(double));
        }
    }
}
