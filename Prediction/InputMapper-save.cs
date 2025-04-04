using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using VBCommon;
using VBCommon.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.ComponentModel;
//using System.Net.Http;

namespace Prediction
{
    class InputMapper
    {
        private Dictionary<string, string> dictColMap;
        private DataTable tblMappedData, tblCurrent;
        private string[] strArrReferencedVars;
        private string strLeftCaption, strRightCaption;
        private BackgroundWorker bgwEnddatDataRetriever = new BackgroundWorker();

        frmWorking frmEnddatWorking;
        bool bRunning = false;
        private string strResult = null;

        public delegate void SetDataCallback(DataTable Data, bool NewMapping);
        public SetDataCallback EnddatCallbackFunction;

        public Dictionary<string, string> ColumnMap
        {
            get { return dictColMap; }
            set { dictColMap = new Dictionary<string,string>(value); }
        }

        public string[] ReferencedVariables
        {
            get { return strArrReferencedVars.ToArray(); }
            //set { strArrReferencedVars = value.ToArray(); }
        }

        public string LeftCaption
        {
            get { return strLeftCaption; }
            //set { strLeftCaption = value; }
        }

        public string RightCaption
        {
            get { return strRightCaption; }
            //set { strRightCaption = value; }
        }


        public InputMapper()
        {
            bgwEnddatDataRetriever.DoWork += new DoWorkEventHandler(bgwEnddatDataRetriever_DoWork);
            bgwEnddatDataRetriever.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgwEnddatDataRetriever_RunWorkerCompleted);
        }


        public InputMapper(IDictionary<string, object> PackedState)
        {
            this.UnpackState(PackedState);
            bgwEnddatDataRetriever.DoWork += new DoWorkEventHandler(bgwEnddatDataRetriever_DoWork);
            bgwEnddatDataRetriever.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgwEnddatDataRetriever_RunWorkerCompleted);
        }


        public InputMapper(Dictionary<string, string> MainEffects, string LeftCaption, string[] VariableNames, string RightCaption)
        {
            strArrReferencedVars = VariableNames.ToArray();
            //dictMainEffects = MainEffects;
            strLeftCaption = LeftCaption;
            strRightCaption = RightCaption;
            bgwEnddatDataRetriever.DoWork += new DoWorkEventHandler(bgwEnddatDataRetriever_DoWork);
            bgwEnddatDataRetriever.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgwEnddatDataRetriever_RunWorkerCompleted);
        }


        public InputMapper(string LeftCaption, string[] VariableNames, string RightCaption)
        {
            strArrReferencedVars = VariableNames;
            //dictMainEffects = MainEffects;
            strLeftCaption = LeftCaption;
            strRightCaption = RightCaption;
            bgwEnddatDataRetriever.DoWork += new DoWorkEventHandler(bgwEnddatDataRetriever_DoWork);
            bgwEnddatDataRetriever.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgwEnddatDataRetriever_RunWorkerCompleted);
        }


        public DataTable ImportFile(DataTable tblCurrent)
        {           
            VBCommon.IO.ImportExport import = new ImportExport();
            DataTable dt = import.Input;            
            if (dt == null)
                return(null);

            if (dictColMap == null)
            {
                string[] strArrHeaderCaptions = { strLeftCaption, strRightCaption };

                //Dictionary<string, string> dictFields = new Dictionary<string, string>(dictMainEffects);
                frmColumnMapper colMapper = new frmColumnMapper(strArrReferencedVars, dt, strArrHeaderCaptions, true, false);
                DialogResult dr = colMapper.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    dt = colMapper.MappedTable;
                    dictColMap = colMapper.ColumnMapping;
                    dt = MapInputColumns(dt);

                    if (tblCurrent != null)
                    {
                        tblCurrent.Merge(dt);
                        dt = tblCurrent;
                    }

                    if (!CheckUniqueIDs(dt)) 
                        return null;
                    else 
                        return dt;
                }
                else
                    return null;
            }
            else
            {
                //We shouldn't have existing data unless we have an existing mapping, so assume that tblCurrent!=null lands us here.
                dt = MapInputColumns(dt);

                if (tblCurrent != null)
                {
                    tblCurrent.Merge(dt);

                    foreach (DataColumn col in dt.Columns)
                    {
                        string colname = col.Caption;
                        tblCurrent.Columns[colname].SetOrdinal(dt.Columns.IndexOf(colname));
                    }

                    dt = tblCurrent;
                }

                if (!CheckUniqueIDs(dt))
                    return null;
                else
                    return dt;
            }
        }


        private void bgwEnddatDataRetriever_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            using (WebDownload client = new WebDownload())
            {
                /*try
                {
                    e.Result = client.DownloadString(e.Argument.ToString()); 
                }
                catch (Exception ex)
                {
                    e.Result = null;
                    e.Cancel = true;
                }*/
                client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DataArrived);

                strResult = null;
                Uri EnddatUri = new Uri(e.Argument.ToString());
                client.DownloadStringAsync(EnddatUri);

                while (strResult == null)
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }
                }

                e.Result = strResult;
            }
        }


        private void DataArrived(object sender, DownloadStringCompletedEventArgs e)
        {
            strResult = e.Result.ToString();
        }


        private void bgwEnddatDataRetriever_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            frmEnddatWorking.Hide();
            bRunning = false;

            if (e.Error != null)
            {
                System.Windows.Forms.MessageBox.Show("There was an error loading data from the EnDDaT web service:\n" + e.Error.Message.ToString());
                return;// null;
            }
            else if (e.Cancelled)
            {
                //resultLabel.Text = "Canceled";
            }
            else
            {
                string strData = e.Result.ToString();
                
                strData = strData.Trim();
                string[] strRows = strData.Split('\n');

                //Deal with illegal characters in the column headers
                Regex re0 = new Regex("(,)(?!(?:[^\"]|\"[^\"]*\")*$)");
                Regex re1 = new Regex("(,)(?!(?:[^\']|\'[^\']*\')*$)");
                Regex re2 = new Regex("(,)(?!(?:[^\\[\\(]|[\\[\\(][^\\]\\)]*[\\]\\)])*$)");
                Regex re3 = new Regex("[^0-9a-zA-Z.,\\\"]");

                strRows[0] = re0.Replace(strRows[0], ".");
                strRows[0] = re1.Replace(strRows[0], ".");
                strRows[0] = re2.Replace(strRows[0], ".");
                strRows[0] = re3.Replace(strRows[0], ".");

                DataTable data = new DataTable();
                foreach (string head in strRows[0].Split(','))
                {
                    string trimmed = head.Trim('"');
                    data.Columns.Add(trimmed);
                }

                for (int i = 1; i < strRows.Length; i++)
                {
                    string row = strRows[i];
                    data.Rows.Add(row.Split(','));
                }

                if (dictColMap == null)
                {
                    string[] strArrHeaderCaptions = { strLeftCaption, strRightCaption };

                    //Dictionary<string, string> dictFields = new Dictionary<string, string>(dictMainEffects);
                    frmColumnMapper colMapper = new frmColumnMapper(strArrReferencedVars, data, strArrHeaderCaptions, true, false);
                    DialogResult dr = colMapper.ShowDialog();

                    if (dr == DialogResult.OK)
                    {
                        DataTable dt = colMapper.MappedTable;
                        dictColMap = colMapper.ColumnMapping;
                        dt = MapInputColumns(dt);

                        if (tblCurrent != null)
                        {
                            tblCurrent.Merge(dt);
                            dt = tblCurrent;
                        }

                        if (!CheckUniqueIDs(dt))
                            return; //null;
                        else
                            EnddatCallbackFunction(Data: dt, NewMapping: true);
                    }
                    else
                        return; //null;
                }
                else
                {
                    //We shouldn't have existing data unless we have an existing mapping, so assume that tblCurrent!=null lands us here.
                    DataTable dt = MapInputColumns(data);

                    if (tblCurrent != null)
                    {
                        tblCurrent.Merge(dt);

                        foreach (DataColumn col in dt.Columns)
                        {
                            string colname = col.Caption;
                            tblCurrent.Columns[colname].SetOrdinal(dt.Columns.IndexOf(colname));
                        }

                        dt = tblCurrent;
                    }

                    if (!CheckUniqueIDs(dt))
                        return; //null;
                    else
                        EnddatCallbackFunction(Data: dt, NewMapping: false);
                }
            }
        }


        public void InitiateEnddatImport(string URL, DataTable Current, SetDataCallback Callback)
        {
            if (!bRunning)
            {
                tblCurrent = Current;
                EnddatCallbackFunction = Callback;
                bRunning = true;
                bgwEnddatDataRetriever.WorkerSupportsCancellation = true;
                bgwEnddatDataRetriever.RunWorkerAsync(URL);

                frmEnddatWorking = new frmWorking("The EnDDaT web service is accessing remote data. Depending on your Internet\nconnection and the amount of data you've requested, this may take up to two minutes.", CancelEnddatRead);
                frmEnddatWorking.Show();
            }
        }


        public void CancelEnddatRead(object sender, EventArgs args)
        {
            bgwEnddatDataRetriever.CancelAsync();
        }


        private DataTable MapInputColumns(DataTable tblRawData)
        {
            DataTable dt = new DataTable();

            if (dictColMap.ContainsKey("ID"))
                dt.Columns.Add("ID", typeof(string));     

            foreach (string meKey in dictColMap.Keys)
            {
                if (String.Compare(meKey,"ID",true) != 0)                
                    dt.Columns.Add(meKey, typeof(double));                                                                      
            }

            //Populate the new data table with data from the old.
            for (int i = 0; i < tblRawData.Rows.Count; i++)
            {
                DataRow dr = dt.NewRow();
                foreach (string meKey in dictColMap.Keys)
                {
                    if (String.Compare(meKey, "ID", true) == 0)
                        dr[meKey] = tblRawData.Rows[i][dictColMap[meKey]].ToString();
                    else
                    {
                        if (dictColMap[meKey] == "none")
                            dr[meKey] = DBNull.Value;
                        else
                            try { dr[meKey] = tblRawData.Rows[i][dictColMap[meKey]]; }
                            catch (ArgumentException e) { dr[meKey] = DBNull.Value; }                                    
                    }
                }
                dt.Rows.Add(dr);
            }
            
            if (dt.Columns.Contains("ID"))
                dt.Columns["ID"].SetOrdinal(0);

            tblMappedData = dt;
            return tblMappedData;
        }


        public bool CheckUniqueIDs(DataTable dt)
        {
            int errndx = 0;
            if (!RecordIndexUnique(dt, out errndx))
            {
                MessageBox.Show("Unable to import datasets with non-unique record identifiers.\n" +
                                "Fix your datatable by assuring unique record identifier values\n" +
                                "in the ID column and try importing again.\n\n" +
                                "Record Identifier values cannot be blank or duplicated;\nencountered " +
                                "error near row " + errndx.ToString(), "Import Data Error - Cannot Import This Dataset", MessageBoxButtons.OK);
                return false;
            }
            return true;
        }
        

        /// <summary>
        /// test all cells in the ID column for uniqueness
        /// could do this with linq but then how does one find where?
        /// Code copied from Mike's VBDatasheet.frmDatasheet.
        /// </summary>
        /// <param name="dt">table to search</param>
        /// <param name="where">record number of offending timestamp</param>
        /// <returns>true iff all unique, false otherwise</returns>
        public static bool RecordIndexUnique(DataTable dt, out int where)
        {
            Dictionary<string, int> dictTemp = new Dictionary<string, int>();
            int intNdx = -1;
            try
            {
                foreach (DataRow dr in dt.Rows)
                {
                    string strTempval = dr["ID"].ToString();
                    dictTemp.Add(dr["ID"].ToString(), ++intNdx);
                    if (string.IsNullOrWhiteSpace(dr["ID"].ToString()))
                    {
                        where = intNdx++;
                        //MessageBox.Show("Record Identifier values cannot be blank - encountered blank in row " + ndx++.ToString() + ".\n",
                        //    "Import data error", MessageBoxButtons.OK);
                        return false;
                    }
                }
            }
            catch (ArgumentException)
            {
                where = intNdx++;
                //MessageBox.Show("Record Identifier values cannot be duplicated - encountered existing record in row " + ndx++.ToString() + ".\n",
                //    "Import data error", MessageBoxButtons.OK);
                return false;
            }
            where = intNdx;
            return true;
        }


        public IDictionary<string, object> PackState()
        {
            IDictionary<string, object> dictPackedState = new Dictionary<string, object>();

            //dictPackedState.Add("MainEffects", dictMainEffects);
            dictPackedState.Add("ColumnMap", JsonConvert.SerializeObject(dictColMap));
            dictPackedState.Add("ReferencedVariables", strArrReferencedVars);
            dictPackedState.Add("LeftCaption", strLeftCaption);
            dictPackedState.Add("RightCaption", strRightCaption);

            return dictPackedState;
        }


        public void UnpackState(IDictionary<string, object> PackedState)
        {
            IDictionary<string, object> dictPackedState = PackedState;

            if (dictPackedState.ContainsKey("ColumnMap"))
            {
                if (dictPackedState["ColumnMap"].GetType() == typeof(string))
                {
                    Type objType = typeof(Dictionary<string, string>);
                    string jsonRep = dictPackedState["ColumnMap"].ToString();

                    object objVariableMapping = JsonConvert.DeserializeObject(jsonRep, objType);
                    dictColMap = (Dictionary<string, string>)objVariableMapping;
                }
            }

            if (dictPackedState.ContainsKey("ReferencedVariables"))
            {
                if (dictPackedState["ReferencedVariables"].GetType() == typeof(Newtonsoft.Json.Linq.JArray))
                {
                    Type objType = typeof(string[]);
                    string jsonRep = dictPackedState["ReferencedVariables"].ToString();

                    object objVariableMapping = JsonConvert.DeserializeObject(jsonRep, objType);
                    strArrReferencedVars = (string[])objVariableMapping;
                }
            }

            if (dictPackedState.ContainsKey("LeftCaption"))
            {
                if (dictPackedState["LeftCaption"].GetType() == typeof(Newtonsoft.Json.Linq.JObject))
                {
                    Type objType = typeof(string);
                    string jsonRep = dictPackedState["LeftCaption"].ToString();

                    object objVariableMapping = JsonConvert.DeserializeObject(jsonRep, objType);
                    strLeftCaption = objVariableMapping.ToString();
                }
            }

            if (dictPackedState.ContainsKey("RightCaption"))
            {
                if (dictPackedState["RightCaption"].GetType() == typeof(Newtonsoft.Json.Linq.JObject))
                {
                    Type objType = typeof(string);
                    string jsonRep = dictPackedState["RightCaption"].ToString();

                    object objVariableMapping = JsonConvert.DeserializeObject(jsonRep, objType);
                    strRightCaption = objVariableMapping.ToString();
                }
            }
        }
    }
}
