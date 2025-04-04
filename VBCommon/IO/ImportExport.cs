﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.IO;
using VBCommon.IO;
using VBCommon.Controls;
using System.Text.RegularExpressions;

namespace VBCommon.IO
{
    //This exception class is used to abort an import that includes improperly-formatted column names
    public class InvalidColumnNameException : Exception { }

    public class ImportExport
    {
        //class for import/export from/to xls, xlsx or csv files - note that xlsx has not been tested
        //at all nor have csv files been tested to any large degree.  
        //also note that the IO.OLEDB class has modifications specific to VB2 data expectations and that
        //has implications in the getTableInfo method (see below) used for export.
        
        private DataTable _dt = null;
        private const string FILE_FILTER = @"Spreadsheet Files (*.xls;*.xlsx;*.csv)|*.xls;*.xlsx;*.csv|All Files(*.*)|*.*";
        private string _importFileName = string.Empty;


        //constructor for import
        public ImportExport ()
        {
        }


        //constructor for export - pass it a datatable
        public ImportExport(DataTable dt)
        {
            _dt = dt;
        }


        //return a datatable on import (or datatable == null iff failed)
                                //"There was an error while trying to import the data. " +
                        //"One possible reason is that the file is currently open in Excel. " +
                        //"If the trouble persists, try converting your data to a comma-separated file and then importing it. " +
                        //"\n\nAlternatively, you can try installing the provider from http://www.microsoft.com/en-us/download/details.aspx?id=23734" +
                        //"\n\nError message: \n\n{0}", ex.Message));
        public DataTable Input
        {
            get 
            {
                try
                {
                    Import();
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Data import error. Make sure the data file is not currently open. " +
                    "If the error persists, try converting the data to a comma-separated file or, save your file as an .xls formatted file. " +
                    "\n\nYou might also update the Excel OLEDB drivers.  Search the web for Microsoft Access Database Engine and obtain the " +
                    "correct installer for your version of Excel.");                    
                    _dt = null;
                }

                return _dt; 
            }
        }


        //return a bool signifying export success/failure
        public bool Output
        {
            get
            {
                bool retval = Export();
                return retval;
            }
        }


        public string getFileImportedName
        {
            get { return _importFileName; }
        }


        private void Import()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = FILE_FILTER;
            openFileDialog1.Multiselect = false;
            openFileDialog1.FileName = "";
            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.Cancel)
                return;

            string fileName = openFileDialog1.FileName;
            if ((fileName == null) || (fileName == ""))
                return;


            FileInfo fi = new FileInfo(fileName);
            string extension = fi.Extension;
            string selectedWorksheet = "";

            //Dealing with an Excel file
            if ((String.Compare(extension, ".xls", true) == 0) || (String.Compare(extension, ".xlsx", true) == 0))
            {
                ExcelOleDb excelDb = new ExcelOleDb(fileName, true);
                string[] names = excelDb.GetWorksheetNames();
                
                //If there is only 1 worksheet in a workbook, lets not make the user select it.  Just open it.
                if (names != null)
                {
                    if (names.Length == 1)
                        selectedWorksheet = names[0];
                    else
                    {
                        frmChooseWorksheet chooseWorkSheet = new frmChooseWorksheet(fi.Name, names);
                        dr = chooseWorkSheet.ShowDialog();

                        if (dr == DialogResult.Cancel || chooseWorkSheet.SelectedWorksheetName == null ||
                            chooseWorkSheet.SelectedWorksheetName == "")
                            return;

                        selectedWorksheet = chooseWorkSheet.SelectedWorksheetName;
                    }
                    _dt = excelDb.Read(selectedWorksheet);
                }
                else
                {
                    _dt = null;
                    selectedWorksheet = "";
                }
                
                if ((_dt == null) || (_dt.Rows.Count < 1))
                    throw new Exception("The following Excel file and worksheet could not be imported: " + fileName + " : " + selectedWorksheet);

            }
            else if (String.Compare(extension, ".csv", true) == 0)
            {
                DelimitedFile delimFile = new DelimitedFile(fileName, ',', true);
                _dt = delimFile.Read();

                if ((_dt == null) || (_dt.Rows.Count < 1))
                    throw new Exception("The following CSV file could not be imported: " + fileName);
            }
            else
            {
                throw new Exception("Unknown file type: " + fileName);
            }

            //Check that column names are valid:
            try
            {
                if (_dt.Columns.Cast<DataColumn>().Where(x => Regex.IsMatch(x.ColumnName, @"^[^a-zA-Z]")).Count() > 0)
                {
                    throw new InvalidColumnNameException();
                }                
            }
            catch (InvalidColumnNameException ex)
            {
                _dt = null;
                System.Windows.Forms.MessageBox.Show("Invalid column name! Please make sure that all columns begin with a letter character.");
                throw new Exception(ex.Message);
            }
            _importFileName = fileName;
        }


        private bool Export()
        {
            //TODO: get overwrite existing file working, release locks on exported file when done
            //and get this and IO.OLEDB in sync to work on general data types instead of VB2 data

            Dictionary<string, string> tableInfo = new Dictionary<string, string>();
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = FILE_FILTER;
            saveFileDialog1.FileName = "";
            saveFileDialog1.OverwritePrompt = false;
            DialogResult dr = saveFileDialog1.ShowDialog();
            if (dr == DialogResult.Cancel)
                return false;

            string fileName = saveFileDialog1.FileName;
            if ((fileName == null) || (fileName == ""))
                return false;

            FileInfo fi = new FileInfo(fileName);

            if (File.Exists(fileName))
            {
                MessageBox.Show("Cannot overwrite existing file; must provide unique name or save elsewhere.",
                    "Export failure", MessageBoxButtons.OK);
                return false;
            }

            string extension = fi.Extension;

            //Dealing with an Excel file
            if ((String.Compare(".xls", extension, true) == 0) || (String.Compare(".xlsx", extension, true) == 0))
            {
                ExcelOleDb excelDb = new ExcelOleDb(fileName, true);
                //excelDb.WriteTable("Sheet1", _rawdataDT);
                if (_dt.TableName == "" || _dt.TableName == null) _dt.TableName = "VB2ExportedData";
                bool writeSuccess = excelDb.WriteTable(_dt.TableName, getTableInfo(_dt));
                //Console.WriteLine("export write success: " + writeSuccess);
                if (writeSuccess)
                {
                    for (int r = 0; r < _dt.Rows.Count; r++)
                    {
                        DataRow mydr = _dt.Rows[r];
                        bool insertSuccess = excelDb.AddNewRow(mydr);
                        //Console.WriteLine("export insert success: " + insertSuccess);
                        if (!insertSuccess)
                        {
                            MessageBox.Show("Issue with InsertRow at row = " + r.ToString() + ".", "OLEDB Export failure at insert row", MessageBoxButtons.OK);
                            return false; // break;
                        }
                    }
                    //something here to release lock on file... ???
                    
                }
                    //well this is stupid - REVISIT!!!!!!!!!!!!!!!!!!!!!!!!!
                else
                {
                    MessageBox.Show("Issue with CreatTable.", "OLEDB Export failure at createTable", MessageBoxButtons.OK);
                    return false;
                }
            }
            else if (String.Compare(".csv", extension, true) == 0)
            {
                DelimitedFile delimFile = new DelimitedFile(fileName, ',', true);
                delimFile.Write(_dt);
            }
            return true;
        }
        
        
        private Dictionary<string, string> getTableInfo(DataTable dt)
        {
            // utility method used for export of data - return table info (colnames/types)

            //TODO - get vbtools IO exceloledb POS code working

            Dictionary<string, string> tableInfo = new Dictionary<string, string>();
            string colname = string.Empty;
            string coltype = string.Empty;

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                //replace sq brackets with parentheses in column name
                string temp = dt.Columns[i].ColumnName;
                temp = temp.Replace("[", "(");
                temp = temp.Replace("]", ")");
                //delimit column name for oledb class
                colname = "[" + temp + "]";
                //date/time stamp - character data
                if (i == 0 || i == 1) coltype = System.Data.OleDb.OleDbType.VarChar.ToString();
                //if exported from prediction tab, model expression - character data
                //else if (dt.Columns[i].ColumnName.Contains("Model")) coltype = System.Data.OleDb.OleDbType.VarChar.ToString();// System.Data.OleDb.OleDbType.LongVarChar.ToString();
                //everything else - doubles
                else coltype = System.Data.OleDb.OleDbType.Double.ToString();
                tableInfo.Add(colname, coltype);
            }
            return tableInfo;
        }


        /// <summary>
        /// method gathers bad cells (blanks, nulls, text, search criterion) by datatable row
        /// </summary>
        /// <returns>list of int arrays containg column index and row index of bad cells</returns>
        public static List<int[]> GetBadCellsByRow(DataTable RawData, string Find)
        {
            DataTable tblRaw = RawData;
            string strFindstring = Find;
            int[] cellNdxs = new int[2];
            List<int[]> badCells = new List<int[]>();
            
            foreach (DataRow dr in tblRaw.Rows)
            {
                int rndx = tblRaw.Rows.IndexOf(dr);
                foreach (DataColumn dc in tblRaw.Columns)
                {
                    int cndx = tblRaw.Columns.IndexOf(dc);
                    //gather blanks...
                    if (string.IsNullOrEmpty(dr[dc].ToString()))
                    {
                        cellNdxs[0] = cndx;
                        cellNdxs[1] = rndx;
                        badCells.Add(cellNdxs);
                        cellNdxs = new int[2];
                    }
                    // ...AND alpha cells only (blanks captured above)...
                    else if (!Information.IsNumeric(dr[dc].ToString()) && tblRaw.Columns.IndexOf(dc) != 0)
                    {
                        cellNdxs[0] = cndx;
                        cellNdxs[1] = rndx;
                        badCells.Add(cellNdxs);
                        cellNdxs = new int[2];
                    }
                    //...AND user input
                    if (dr[dc].ToString() == strFindstring && !string.IsNullOrWhiteSpace(strFindstring))
                    {
                        cellNdxs[0] = cndx;
                        cellNdxs[1] = rndx;
                        badCells.Add(cellNdxs);
                        cellNdxs = new int[2];
                    }
                }
            }
            return badCells;
        }


        public static List<int[]> GetBadColumns(DataTable RawData)
        {
            List<int[]>  listBadColumns = new List<int[]>();

            //Count the distinct values in each column.
            var results = RawData.Columns
            .Cast<DataColumn>()
            .Select(dc => new
            {
                Name = dc.ColumnName,
                Index = RawData.Columns.IndexOf(dc),
                Values = RawData.Rows
                    .Cast<DataRow>()
                    .Select(row => row[dc])
                    .Distinct()
                    .Count()
            })
            .OrderBy(item => item.Values);//.ToList<int>();

            //Remove any columns that have fewer than two distinct values.
            foreach (var entry in results)
            {                
                if (entry.Values < 2)
                {
                    int[] col = {entry.Index, 0};
                    listBadColumns.Add(col);
                }
            }

            return listBadColumns;
        }
    }
}
