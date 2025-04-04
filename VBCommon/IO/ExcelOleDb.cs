﻿//********************************************************************************************************
//
// File VBTools.IO.ExcelOleDb.cs
// Author: Kurt Wolfe
// Created: 06/02/2009
// Provides programmatic access to Excel files.  Not required to have Excel installed.
// This code is a modified version of the project found at:
// http://www.codeproject.com/KB/miscctrl/Excel_data_access.aspx
// Accessed on 06/02/2009
//
//modified several times by mgalvin late 2009 and early 2010 to get export working with vb2 data
//neither oledb create table nor insert row was working for any data of any sort
//changes are noted in code comments
//********************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text.RegularExpressions;

namespace VBCommon.IO
{
    /// <summary>
    /// 
    /// </summary>
    public class ExcelOleDb
    {
        public delegate void ProgressWork(float percentage);

        private string _fileName = "";
        private bool _firstRowHeaders = true;        
        private OleDbConnection _conn = null;
        private string _connString = "";


        public ExcelOleDb(string fileName, bool firstRowHeaders)
        {
            _fileName = fileName;
            _firstRowHeaders = firstRowHeaders;

            SetConnection();
        }

        

        private bool Validate()
        {
            if ((_fileName == null) || (_fileName == ""))
            {
                throw new Exception("File has not been specified.");
            }

            if (!File.Exists(_fileName))
                throw new Exception("Could not find file: " + _fileName);

            FileInfo fi = new FileInfo(_fileName);
            if ((!fi.Extension.Equals(".xls")) && (!fi.Extension.Equals(".xlsx")))
                throw new Exception("File must be of type .xls or .xlsx: " + _fileName);

            return true;
        }


        private void SetConnectionString()
        {
            //modified connection string to put quotes around data source and added mode=read/write
            string excelConnString = "Provider=Microsoft.{0}.OLEDB.{1};Data Source='{2}';Mode=ReadWrite;Extended Properties=\'Excel {3};HDR={4}\'";
           
            string headers = "";
            if (_firstRowHeaders)
                headers = "Yes";
            else
                headers = "No";

            //Check for File Format
            FileInfo fi = new FileInfo(_fileName);
            if (fi.Extension.Equals(".xls"))
            {
                // For Excel Below 2007 Format
                _connString =  string.Format(excelConnString, "Jet", "4.0", _fileName, "8.0",headers);
            }
            else
            {
                // For Excel 2007 or later file  format
                //Figure out what version of Excel OleDb provider is installed:
                string strExcelInterop = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("TypeLib\\{4AC9E1DA-5BAD-4AC7-86E3-24F4CDCECA28}\\c.0").GetValue("PrimaryInteropAssemblyName").ToString();
                if (strExcelInterop == null)
                    //If the provider wasn't found, let them know:
                    System.Windows.Forms.MessageBox.Show("You may not have the OleDb provider necessary to read Excel files. " +
                        "We'll attempt the import anyway - if it doesn't work consider converting the data to a comma-separated file and trying again." +
                        "\n\nAlternatively, you can try installing the provider from http://www.microsoft.com/en-us/download/details.aspx?id=23734");
                _connString = string.Format(excelConnString, "Ace", "12.0", _fileName, "12.0", headers);
            }
        }


        private void SetConnection()
        {
            if (_conn == null)
            {
                SetConnectionString();
                _conn = new OleDbConnection(_connString);
            }
        }


        /// <summary>
        /// Read an Excel worksheet in workbook
        /// </summary>
        /// <param name="worksheet">The name of the worksheet to read</param>
        /// <returns></returns>
        public DataTable Read(string worksheet)
        {
            return Read(worksheet, "");
        }


        /// <summary>
        /// Read an Excel worksheet in workbook
        /// </summary>
        /// <param name="worksheet">The name of the worksheet to read</param>
        /// <param name="criteria">A 'where' clause as part of the query</param>
        /// <returns>DataTable with </returns>
        public DataTable Read(string worksheet, string criteria)
        {
            try
            {
                if (_conn.State != ConnectionState.Open)
                {
                    _conn.Open();
                }
                string cmdText = "Select * from [{0}]";
                if (!string.IsNullOrEmpty(criteria))
                {
                    cmdText += " Where " + criteria;
                }

                string query = string.Format(cmdText, worksheet);
                OleDbCommand cmd = new OleDbCommand(query);
                cmd.Connection = _conn;
                OleDbDataAdapter adpt = new OleDbDataAdapter(cmd);

                DataTable dt = new DataTable();
                adpt.Fill(dt);

                if (dt != null)
                {
                    //Create the new datatable, datacolumn:
                    string sColName = dt.Columns[0].ColumnName;
                    DataTable dt2 = new DataTable();
                    dt2.Columns.Add(new DataColumn(columnName:sColName, dataType:typeof(String)));

                    //Populate the column:
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        DataRow dr = dt2.NewRow();
                        dr[0] = dt.Rows[j][0].ToString();
                        dt2.Rows.Add(dr);
                    }

                    //Replace the index column in dt with the new column:
                    dt.Columns.RemoveAt(0);
                    dt.Columns.Add(new DataColumn(columnName: sColName, dataType: typeof(String)));

                    for(int j=0; j<dt2.Rows.Count; j++)
                    {
                        dt.Rows[j][sColName] = dt2.Rows[j][sColName];
                    }
                    DataColumn dc = dt.Columns[sColName];
                    dc.SetOrdinal(0);

                    //Return the result
                    return dt;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }


        /// <summary>
        /// Retrieves the worksheets in a an Excel file
        /// </summary>
        /// <returns>Array of strings containing worksheet names</returns>
        public string[] GetWorksheetNames()
        {
            DataTable dt = GetSchema();
            List<string> names = new List<string>();

            if (dt != null)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                    names.Add(dt.Rows[i]["TABLE_NAME"].ToString());

                return names.ToArray();
            }
            else
                return null;
        }

        /// <summary>
        /// Reads the Schema Information
        /// </summary>
        /// <returns>DataTable containing schema information</returns>
        private DataTable GetSchema()
        {
            DataTable dtSchema = null;
            try
            {
                if (_conn.State != ConnectionState.Open) _conn.Open();
                object[] args = new object[] { null, null, null, "TABLE" };
                dtSchema = _conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, args);
                return dtSchema;
            }
            catch (System.Data.OleDb.OleDbException ex)
            {
                System.Windows.Forms.MessageBox.Show(String.Format("An error occured while opening the file: {0}", ex.Message));
                return null;
            }
        }


        /// <summary>
        /// Creates Create Table Statement and runs it.
        /// </summary>
        /// <param name="tableName">Name of the worksheet</param>
        /// <param name="tableDefination">Key value pair of column names and types</param>
        /// <returns></returns>
        public bool WriteTable(string tableName, Dictionary<string, string> tableDefination)
        {
            try
            {
                OleDbCommand cmd = new OleDbCommand(
                GenerateCreateTable(tableName, tableDefination), _conn);
                {
                    if (_conn.State != ConnectionState.Open)
                        _conn.Open();

                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Damn OLEDB connection failed AGAIN!", e.Message.ToString());
                return false;
            }
        }

        // Generates Insert Statement and executes it
        public bool AddNewRow(DataRow dr)
        {
            //added error checking
            try
            {
                using (OleDbCommand cmd = new OleDbCommand(GenerateInsertStatement(dr), _conn))
                {
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (OleDbException e)
            {
                Console.WriteLine("OLEDB Insert failed:" + e.Message.ToString());
                //this was helpful - NOT
                //for (int i = 0; i < e.Errors.Count; i++)
                //{
                //    System.Windows.Forms.MessageBox.Show("Index #" + i + "\n" +
                //           "Message: " + e.Errors[i].Message + "\n" +
                //           "Native: " + e.Errors[i].NativeError.ToString() + "\n" +
                //           "Source: " + e.Errors[i].Source + "\n" +
                //           "SQL: " + e.Errors[i].SQLState + "\n");
                //}
                return false;
            }
            
        }

        // Create Table Generation based on Table Defination
        private string GenerateCreateTable(string tableName,Dictionary<string, string> tableDefination)
        {

            StringBuilder sb = new StringBuilder();
            bool firstcol = true;
            sb.AppendFormat("CREATE TABLE [{0}](", tableName);
            firstcol = true;
            foreach (KeyValuePair<string, string> keyvalue in tableDefination)
            {
                if (!firstcol)
                {
                    sb.Append(",");
                }
                firstcol = false;
                sb.AppendFormat("{0} {1}", keyvalue.Key, keyvalue.Value);
            }

            sb.Append(")");
            return sb.ToString();
        }


        //Generates InsertStatement from a DataRow.
        private string GenerateInsertStatement(DataRow dr)
        {
            //modified to put brackets around col name and removed quotes around values other than col 0
            //also replace all "[" and "]" with "(" and ")" respectively in col names
            StringBuilder sb = new StringBuilder();
            bool firstcol = true;
            sb.AppendFormat("INSERT INTO [{0}](", dr.Table.TableName);
            
            foreach (DataColumn dc in dr.Table.Columns)
            {
                if (!firstcol)
                {
                    sb.Append(",");
                }
                firstcol = false;
                string temp = dc.Caption;
                temp = temp.Replace("[", "(");
                temp = temp.Replace("]", ")");
                sb.Append("[" + temp + "]");
            }

            sb.Append(") VALUES(");
            firstcol = true;
            for (int i = 0; i <= dr.Table.Columns.Count - 1; i++)
            {
                if (!object.ReferenceEquals(dr.Table.Columns[i].DataType, typeof(int)))
                {
                    //string type for date/time column (col==0) and Model string column
                    //if (i == 0 || dr.Table.Columns[i].Caption == "Model")
                    if (object.ReferenceEquals(dr.Table.Columns[i].DataType, typeof(string)) ||
                        object.ReferenceEquals(dr.Table.Columns[i].DataType, typeof(DateTime)))
                    {
                        sb.Append("'");
                        sb.Append(dr[i].ToString().Replace("'", "''"));
                        sb.Append("'");
                    }
                    else
                    {
                        //sb.Append("'");
                        sb.Append(dr[i].ToString().Replace("'", "''"));
                        //sb.Append("'");
                    }
                }
                else
                {
                    sb.Append(dr[i].ToString().Replace("'", "''"));
                }
                if (i != dr.Table.Columns.Count - 1)
                {
                    sb.Append(",");
                }
            }

            sb.Append(")");
            return sb.ToString();
        }

        public void CloseConnection()
        {
            _conn.Close();
            _conn.Dispose();
        }
    }
}
