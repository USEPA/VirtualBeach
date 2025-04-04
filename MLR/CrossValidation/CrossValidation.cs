﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using GALib;
//using VBStatistics;
using VBCommon.Statistics;
using MLRCore;

namespace CrossValidation
{
    
    public class CrossValidation
    {
        private List<IIndividual> _models = null;
        //private List<MSEP> _validatedModels = null;
        private MSEP[] _validatedModels = null;
        
        //Called R by Cyterski
        private int _sampleSize = 0;
        //Called N by Cyterski
        private int _numIterations = 0;

        private int _seed = 0;

        private int _numRecs = 0;

        private ProgressBar _progBar = null;

        public ProgressBar ProgressBar
        {
            set { _progBar = value; }
        }

        public int SampleSize
        {
            get { return _sampleSize; }
            set { _sampleSize = value; }
        }

        public int Iterations
        {
            get { return _numIterations; }
            set { _numIterations = value; }
        }

        public int Seed
        {
            get { return _seed; }
            set { _seed = value; }
        }

        public MSEP[] MSEPList
        {
            get { return _validatedModels; }
        }

        public CrossValidation(List<IIndividual> models)
        {
            _models = models;            

        }

        public void Run()
        {
            if (_models == null)
                return;

            CrossValidate(_models);
        }

        private void CrossValidate(List<IIndividual> models)
        {
            if (_progBar != null)
            {
                _progBar.Minimum = 0;
                _progBar.Maximum = _numIterations;
                _progBar.Step = 1;
            }

            MLRDataManager projMgr = MLRDataManager.GetDataManager();            
            DataTable dt = projMgr.ModelDataTable;

            if (dt == null)
                throw new Exception("Model data table is null.");

            _validatedModels = new MSEP[_models.Count];

            for (int i = 0; i < _models.Count; i++)
            {                
                string[] indepVars = _models[i].Chromosome.Where(x => x != "").ToArray();
                _validatedModels[i] = new MSEP(_models[i], 0, indepVars);
            }

            DataTable dtCopy = dt.Copy();

            _numRecs = dtCopy.Rows.Count;

            for (int i = 0; i < _numIterations; i++)
            {
                if (_progBar != null)                
                    _progBar.Value = i + 1;

                List<int> indexList = RandList(_sampleSize, _numRecs);
                indexList.Sort();

                DataTable dtTraining = dt.Copy();
                DataTable dtTesting = dt.Clone();

                for (int j = indexList.Count - 1; j >= 0; j--)
                {
                    int idx = indexList[j];
                    dtTesting.ImportRow(dtTraining.Rows[idx]);
                    dtTraining.Rows[idx].Delete();
                }
                //added this to fix projectopen xvalidation bug - don't know why we dont hit this otherwise... mog 1/6/2014
                dtTraining.AcceptChanges();

                for (int j = 0; j < _validatedModels.Length; j++)
                    CrossValidateSingleModel(_validatedModels[j], dtTraining, dtTesting, indexList);
            }

            for (int i = 0; i < _validatedModels.Length; i++)
                _validatedModels[i].msep = _validatedModels[i].msep / (double)_numIterations;
        }


        private void CrossValidateSingleModel(MSEP msepObjIn, DataTable dtTraining, DataTable dtTesting, List<int> indexList)
        {
            IIndividual model = msepObjIn.Model;
            string[] indepVars = msepObjIn.IndependentVariables;
            //bad assumptions here; dependent variable may not be in col 1 - mog 1/7/2014
            //string depVar = dtTraining.Columns[1].ColumnName; 

            MLRDataManager projMgr = MLRDataManager.GetDataManager();
            string depVar = projMgr.ModelDependentVariable;
            int depVarColNdx = dtTesting.Columns[depVar].Ordinal;
            
            
            MultipleRegression mr = null;

            try
            {
                mr = new MultipleRegression(dtTraining, depVar, indepVars);
                mr.Compute();
            }
            catch (Exception e)
            {
                //continue
            }

            double msep = 0;
            int numTestingRows = dtTesting.Rows.Count;
            foreach (DataRow dr in dtTesting.Rows)
            {
                double predVal = mr.Predict(dr);

                //bad assumptions here; dependent variable may not be in col 1 - mog 1/7/2014
                //double obsVal = Convert.ToDouble(dr[1]);
                double obsVal = Convert.ToDouble(dr[depVarColNdx]);
                msep += Math.Pow((obsVal - predVal), 2.0);
            }

            msep = msep / (double)numTestingRows;
            msepObjIn.msep += msep;
        }


        private List<int> RandList(int sampleSize, int numObservations)
        {
            int[] numList = new int[numObservations];
            List<int> sampledValues = new List<int>(sampleSize);

            for (int i = 0; i < numObservations; ++i) 
                 numList[i] = i;


            int nLeft = numObservations;
            //Random listRand = new Random(_seed);
            Random listRand = new Random();

            for (int i = 0; i < sampleSize; i++)
            {
                int x = listRand.Next(nLeft);
                int randVal = numList[x];
                sampledValues.Add(randVal);
                numList[x] = numList[--nLeft];
            }

            sampledValues.Sort();
            return sampledValues;
        }
    }
}
