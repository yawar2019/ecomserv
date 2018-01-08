using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.Serialization.Json;
using System.Data.SqlClient;
using System.ServiceModel.Activation;
using System.Globalization;
using Oracle.DataAccess.Client;
using System.Security.Cryptography;
using bomoserv.SECURE;
using bomoserv.Common;
using bomoserv.Data;
using bomoserv.ProcessFile;

namespace bomoserv
{
          
    public class bomopublic : ibomopublic
    {
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/GetMPData")]
        public MPDataList GetMPData(MPDataList data)
        {
            MPDataList result = new MPDataList();
            clsCommon common = new clsCommon();
            string log_key = "GetMPData";
            result.Status = "Failed. Please try later";
            try
            {
                clsAuthenticator auth = new clsAuthenticator();
                if (data.ApiKey == null)
                {
                    common.Log(log_key, "Received without ApiKey(still safe)", true, null);
                    return result;
                }
                if (!auth.IsValidString(data.ApiKey))
                {
                    common.Log(log_key, "Received with ApiKey with invalid values(still safe)", true, null);
                    return result;
                }
                if (data.ApiKey != auth.ApiKey)
                {
                    common.Log(log_key, "Received with wrong ApiKey(still safe, ApiKey Received :" + data.ApiKey + ")", true, null);
                    return result;
                }
                if (data.videoId == null)
                {
                    //  common.Log(log_key, "Received without ApiKey(still safe)", true, null);
                    return result;
                }
                if (data.videoId == "")
                {
                    return result;
                }

                result.data = new List<MPData>();
                clsMusic music = new clsMusic();
                MPData musicData = new MPData();
                if (!music.GetMusic(data.videoId, out musicData))
                {
                    result.Status = "No data. Please try to download another item";
                }
                else
                {
                    result.data.Add(musicData);
                    result.Status = "SUCCESS";
                }
            }
            catch (Exception exp)
            {
                common.Log(log_key, exp.Message, true, exp);
            }
            finally
            {

            }

            return result;
        }
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/GetMPLinks")]
        public MPDataList GetMPLinks(MPDataList data)
        {
            MPDataList result = new MPDataList();
            clsCommon common = new clsCommon();
            string log_key = "GetMPLinks";
            result.Status = "Failed. Please try later";
            try
            {
                clsAuthenticator auth = new clsAuthenticator();
                if (data.ApiKey == null)
                {
                    common.Log(log_key, "Received without ApiKey(still safe)", true, null);
                    return result;
                }
                if (!auth.IsValidString(data.ApiKey))
                {
                    common.Log(log_key, "Received with ApiKey with invalid values(still safe)", true, null);
                    return result;
                }
                if (data.ApiKey != auth.ApiKey)
                {
                    common.Log(log_key, "Received with wrong ApiKey(still safe, ApiKey Received :" + data.ApiKey + ")", true, null);
                    return result;
                }
                if (data.Keyword == null)
                {
                    //  common.Log(log_key, "Received without ApiKey(still safe)", true, null);
                    return result;
                }
                if (data.Keyword == "")
                {
                    //  common.Log(log_key, "Received without ApiKey(still safe)", true, null);
                    return result;
                }
                //  if (!auth.IsValidString(data.Keyword))
                //  {
                // common.Log(log_key, "Received with ApiKey with invalid values(still safe)", true, null);
                //  return result;
                // }

                result.data = new List<MPData>();
                clsMusic music = new clsMusic();
                List<MPData> listdata = new List<MPData>();
                if (!music.GetDataList(data.Keyword, out listdata))
                {
                    result.Status = "No data. Please try with another keyword";
                }
                else
                {
                    result.data = listdata;
                    result.Status = "SUCCESS";
                }


            }
            catch (Exception exp)
            {
                common.Log(log_key, exp.Message, true, exp);
            }
            finally
            {

            }

            return result;
        }

        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/ConvertFileData")]
        public PDFData ConvertFileData(PDFData data)
        {
            PDFData result = new PDFData();
            clsCommon common = new clsCommon();
            result.Status = "Failed. Please try later"; 
            try
            {
                clsAuthenticator auth = new clsAuthenticator(); 
                if (data.ApiKey == null)
                {
                    common.Log("ConvertFileData", "Received without ApiKey(still safe)", true, null);
                    return result;
                } 
                if (!auth.IsValidString(data.ApiKey))
                {
                    common.Log("ConvertFileData", "Received with ApiKey with invalid values(still safe)", true, null);
                    return result;
                } 
                if (data.ApiKey != auth.ApiKey)
                {
                    common.Log("ConvertFileData", "Received with wrong ApiKey(still safe, ApiKey Received :" + data.ApiKey + ")", true, null);
                    return result;
                } 
                clsFiles clsfile = new clsFiles();
               string out_file_out_file_byte_str = "";
                byte[] out_file_byte_array = null;
                string res_msg = ""; 
                if (data.from_format.Trim().ToLower() == "pdf")
                {
                    if (clsfile.convert(data, out res_msg, out out_file_byte_array))
                    {
                        //result.data = out_file_out_file_byte_str;
                        result.buffer = out_file_byte_array;
                        result.Status = "SUCCESS";
                    }
                    else
                    {
                        if (res_msg.Trim().Length > 0)
                            result.Status = res_msg;
                    }
                } 
            }
            catch (Exception exp)
            {
                common.Log("ConvertFileData", exp.Message, true, exp);
            }
            finally
            {
                
            }

            return result;
        }
        public PDFData ConvertFile(string ApiKey, string is_paid, string from_format, string to_format, string source, string file_path, string file_data, string page_type, string page_num, string file_pw, string file_email)
        {
            PDFData result = new PDFData();
            clsCommon common = new clsCommon();
            result.Status = "Failed. Please try later";
            try
            {
                result.Status = "SUCCESS!!";
            }
            catch (Exception exp)
            {
                common.Log("ConvertFile", exp.Message, true, exp);
            }
            return result;
        }
       
    }
}