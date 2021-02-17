using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Data;

namespace ecomserv.Common
{
    class clsLog
    {
        private  List<string> LastErrors = new List<string>(); 
        public  void Log(string action, string logDesc, bool isError, Exception expp)
        {
            try
            {
                if (logDesc == "Thread was being aborted.")
                    return;
                string log_msg = GetLogMessage(action, logDesc, isError, expp);
                if (!isError)
                    return;
                string sub1 = "Log Data: "+ action;
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
            string app_name = "";
            if (isError)
            { 
                try
                {
                    app_name = System.AppDomain.CurrentDomain.FriendlyName;
                }
                catch(Exception exp)
                {

                }
                
                if (expp != null)
                {
                    expDetail = "at line:" + GetLineNumber(expp);
                    expDetail += expp.ToString();

                    currentLogData += DateTime.Now.ToString() + " - "+ app_name + " - version : " + common.version + " - Error : " + action + " - " + logDesc + " - (More: " + expDetail + ")";
                }
                else
                    currentLogData += DateTime.Now.ToString() + " - " + app_name + " - version : " + common.version + " - Error : " + action + " - " + logDesc + "";
            }
            else
                currentLogData += DateTime.Now.ToString() + " - " + app_name + " - version : " + common.version + " - Status : " + action + " - " + logDesc;
            return currentLogData;
        }
    }
}