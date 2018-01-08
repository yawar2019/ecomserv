using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Data;

namespace bomoserv.Common
{
    class clsLog
    {
        private  List<string> LastErrors = new List<string>();
        private  string LogFilePath = "";
        public  void Log(string action, string logDesc, bool isError, Exception expp)
        {
            try
            {
                string log_msg = GetLogMessage(action, logDesc, isError, expp);
                string query = @"INSERT INTO [aadhisof_db1].[dbo].[tbl_log]
           ([log_name]
           ,[log_desc]
           ,[log_date]
           ,[is_send_email])
     VALUES
           ('" + action + @"'
           ,'" + log_msg + @"'
           ,getdate()
           ,'n')";
                clsConnectionSQL conn = new clsConnectionSQL();
                conn.ExecuteNonQuery(query);
                if (isError)
                {
                    DataTable dtData = new DataTable();
                    dtData = conn.getDataTable(@"SELECT [log_name]
      ,[log_desc]
      ,convert(char(20), [log_date],113) as log_date
      ,[is_send_email]
  FROM [aadhisof_db1].[dbo].[tbl_log] order by log_date desc");
                    if (dtData.Rows.Count > 0)
                    {
                        string last_date_str = dtData.Rows[dtData.Rows.Count - 1]["log_date"].ToString().Trim();
                        DateTime last_date = DateTime.Now;
                        if (DateTime.TryParse(last_date_str, out last_date))
                        {
                            TimeSpan ts = (TimeSpan)(DateTime.Now - last_date);
                            if (ts.TotalDays > 3)
                            {
                                clsCommon common = new clsCommon();
                                string sub = "Log Data: " + dtData.Rows.Count + " item(s), gap=" + ((int)ts.TotalDays).ToString() + " day(s)";
                                string msgbody = "";
                                for (int i = 0; i < dtData.Rows.Count; i++)
                                    msgbody += dtData.Rows[i]["log_date"].ToString().Trim() + " - " + dtData.Rows[i]["log_name"].ToString().Trim() + " - " + dtData.Rows[i]["log_desc"].ToString().Trim() + Environment.NewLine;
                                if (common.alert_me(sub, msgbody))
                                {
                                    conn.ExecuteNonQuery("delete [aadhisof_db1].[dbo].[tbl_log]");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exp)
            {

            }
        } 
       
        public  string GetTimeStamp()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }
        public  int GetLineNumber(Exception ex)
        {
            var lineNumber = 0;
            try
            {
                const string lineSearch = ":line ";
                var index = ex.StackTrace.LastIndexOf(lineSearch);
                if (index != -1)
                {
                    var lineNumberText = ex.StackTrace.Substring(index + lineSearch.Length);
                    if (int.TryParse(lineNumberText, out lineNumber))
                    {
                    }
                }
            }
            catch
            {

            }
            return lineNumber;
        }
        private  string GetLogMessage(string action, string logDesc, bool isError, Exception expp)
        {
            string currentLogData = "";
            string expDetail = "";
            clsCommon common = new clsCommon();
            if (isError)
            {
                if (expp != null)
                {
                    expDetail = "at line:" + GetLineNumber(expp);
                    expDetail += expp.ToString();

                    currentLogData += DateTime.Now.ToString() + " - version : " + common.version + " - Error : " + action + " - " + logDesc + " - (More: " + expDetail + ")";
                }
                else
                    currentLogData += DateTime.Now.ToString() + " - version : " + common.version + " - Error : " + action + " - " + logDesc + "";
            }
            else
                currentLogData += DateTime.Now.ToString() + " - version : " + common.version + " - Status : " + action + " - " + logDesc;
            return currentLogData;
        }
    }
}