using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Collections;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using bomoserv.Data;
/// <summary>
/// Summary description for clsFiles
/// </summary>
public class clsFiles
{

    private clsCommon common = new clsCommon();

    public clsFiles()
    {
        try
        {

        }
        catch (Exception exp)
        {
            common.Log("clsFiles", exp.Message, true, exp);
        }
    }


    public bool convert(PDFData data,   out string res_msg, out byte[] byte_array)
    {
        bool res = false;
       // out_file_byte_str = "";
        res_msg = "";
        byte_array = null;
        try
        {
            byte[] file_bytes = null;
            //string file_name = "";
            int trial_page_count = 1;
            if (data.source.Trim().ToLower() == "link")
            {
                try
                {
                    bool isValid = false;
                    var request = (HttpWebRequest)WebRequest.Create(data.file_path.Trim());
                    request.Method = "HEAD";
                    var response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK && response.ContentType == "application/pdf")
                    {
                        WebClient myClient = new WebClient();
                        file_bytes = myClient.DownloadData(data.file_path.Trim());
                        isValid = true;
                    }
                    else
                    {
                        res_msg = "Failed: Could not get the PDF file from the URL. Please verify that the file Url is correct";
                        return res;
                    }
                    try
                    {
                        response.Close();
                    }
                    catch
                    {

                    }
                    if (!isValid)
                    {
                        res_msg = "Failed: The Url could not be reached. Please verify that the file Url is correct";
                        return res;
                    }
                }
                catch (Exception exp)
                {
                    res_msg = "Failed: The Url could not be reached. Please verify that the file Url is correct";
                    return res;
                }
            }
            else
            {
              //  common.Log("FromBase64String", data.file_data.Substring(0, Math.Min(data.file_data.Length, 250)), true, null);
                string convert = data.file_data.Replace("data:application/pdf;base64,", String.Empty); 
                file_bytes = Convert.FromBase64String(convert); 
               // common.Log("FromBase64String2", "in next line", true, null);
            }
            clsConvert conve = new clsConvert();
            SautinSoft.PdfFocus obj = conve.get_obj();
            try
            {
                if (data.file_pw.Trim().Length > 0&& data.is_paid.ToLower() == "y")
                    obj.Password = data.file_pw.Trim();
                obj.OpenPdf(file_bytes);
                if (obj.PageCount == 0)
                {
                    res_msg = "Failed: " + "PDF File is protected or corrupted";
                    return res;
                }
                string RenderPagesString = "";
                if (data.is_paid.ToLower() != "y")
                    RenderPagesString = "1-" + trial_page_count.ToString();
                else
                {
                    if (data.page_type.ToLower().Trim() == "custom")
                        RenderPagesString = data.page_num;
                    else if (data.page_type.ToLower().Trim() == "odd")
                    { 
                        for (int i = 1; i <= obj.PageCount; i++)
                        {
                            if (i % 2 != 0)
                            {
                                if (RenderPagesString.Trim().Length > 0)
                                    RenderPagesString += ",";
                                RenderPagesString += i.ToString();
                            }
                        }
                    }
                    else if (data.page_type.ToLower().Trim() == "even")
                    {
                        for (int i = 1; i <= obj.PageCount; i++)
                        {
                            if (i % 2 == 0)
                            {
                                if (RenderPagesString.Trim().Length > 0)
                                    RenderPagesString += ",";
                                RenderPagesString += i.ToString();
                            }
                        }
                    }
                }
                if (RenderPagesString.Trim().Length > 0)
                    obj.RenderPagesString = RenderPagesString;  

                switch (data.to_format)
                {
                    case "word":
                        {
                            string res_data = obj.ToWord();
                            if (res_data.Trim().Length > 0)
                            {
                                string out_name = Path.GetFileNameWithoutExtension(common.GetUniqNo()) + ".doc";
                                string file_detail = "attachment;filename=" + out_name;
                                if (data.file_email.Trim().Length == 0)
                                {
                                    byte_array =get_array(res_data);
                                    res = true;
                                }
                                else
                                {
                                    res = send_file_as_email(res_data, file_detail, data.file_email);
                                }
                               // common.alert_me("file", res_data);
                            }
                            break;
                        }
                    case "rtf":
                        {
                            string res_data = obj.ToWord();
                            if (res_data.Trim().Length > 0)
                            {
                                string out_name = Path.GetFileNameWithoutExtension(common.GetUniqNo()) + ".rtf";
                                string file_detail = "attachment;filename=" + out_name;
                                if (data.file_email.Trim().Length == 0)
                                {
                                    byte_array = get_array(res_data);
                                    res = true;
                                }
                                else
                                {
                                    res = send_file_as_email(res_data, file_detail, data.file_email);
                                }
                            }
                            break;

                        }
                    case "xls":
                        {
                            byte[] res_data_byte = obj.ToExcel();
                            if (res_data_byte != null)
                            {
                                if (res_data_byte.Length > 0)
                                {
                                    string out_name = Path.GetFileNameWithoutExtension(common.GetUniqNo()) + ".xls";
                                    string file_detail = "attachment; filename=" + out_name;
                                    if (data.file_email.Trim().Length == 0)
                                    {
                                        byte_array = res_data_byte;
                                        res = true;
                                    }
                                    else
                                    {
                                        res = send_file_as_email(res_data_byte, file_detail, data.file_email);
                                    }

                                }
                            }
                            break;
                        }
                    case "xml":
                        {
                            string res_data = obj.ToXml();
                            if (res_data.Trim().Length > 0)
                            {
                                string out_name = Path.GetFileNameWithoutExtension(common.GetUniqNo()) + ".xml";
                                string file_detail = "attachment;filename=" + out_name;
                                if (data.file_email.Trim().Length == 0)
                                {
                                    byte_array = get_array(res_data);
                                    res = true;
                                }
                                else
                                {
                                    res = send_file_as_email(res_data, file_detail, data.file_email);
                                }

                            }
                            break;
                        }
                    case "text":
                        {
                            string res_data = obj.ToXml();
                            if (res_data.Trim().Length > 0)
                            {
                                string out_name = Path.GetFileNameWithoutExtension(common.GetUniqNo()) + ".txt";
                                string file_detail = "attachment;filename=" + out_name;
                                if (data.file_email.Trim().Length == 0)
                                {
                                    byte_array = get_array(res_data);
                                    res = true;
                                }
                                else
                                {
                                    res = send_file_as_email(res_data, file_detail, data.file_email);
                                }

                            }
                            break;
                        }
                    case "tiff":
                        {
                            byte[] res_data_byte = obj.ToMultipageTiff();
                            if (res_data_byte != null)
                            {
                                string out_name = Path.GetFileNameWithoutExtension(common.GetUniqNo()) + ".tiff";
                                string file_detail = "attachment; filename=" + out_name;
                                if (res_data_byte.Length > 0)
                                {
                                    if (data.file_email.Trim().Length == 0)
                                    {
                                        byte_array = res_data_byte;
                                        res = true;
                                    }
                                    else
                                    {
                                        res = send_file_as_email(res_data_byte, file_detail, data.file_email);
                                    }
                                }
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            { 
                common.Log("Convert", ex.Message, true, ex);
                return res;
            }
            finally
            {
                conve.kill_obj(obj);
            }
        }
        catch (Exception exp)
        {
            common.Log("Convert", exp.Message, true, exp);
        }
        return res;
    }
    private bool send_file_as_email(byte[] data, string file_details, string to_email)
    {
        bool res = false;
        try
        {
            Attachment att = new Attachment(new MemoryStream(data), file_details);
            string prd_name = "Aadhi PDF Converter";
            string sub = "Converted document using " + prd_name;
            string content = "Hi," + Environment.NewLine;
            content += "Please find the attachment that you converted using " + prd_name + "." + Environment.NewLine;
            content += "Thanks for using " + prd_name + "!" + Environment.NewLine + Environment.NewLine;

            content += "---------------" + Environment.NewLine;
            content += "Convert PDF files in PC : http://www.aadhisoft.com/pdfconverter " + Environment.NewLine;
            content += "Convert PDF files online : http://www.aadhisoft.com/pdftoword " + Environment.NewLine;
            content += "Visit Our Website : https://www.bomosi.com/ " + Environment.NewLine;
            content += "Our more iOS Apps : https://itunes.apple.com/us/developer/vinod-m/id825514195 " + Environment.NewLine;
            content += "Our more Android Apps : https://play.google.com/store/apps/developer?id=Quotes " + Environment.NewLine;
            content += "---------------" + Environment.NewLine;
            content += "Note, this is an auto generated email. Please do not reply." + Environment.NewLine;
            return common.send_email(false, to_email, sub, content, att, false);
        }
        catch (Exception exp)
        {
            common.Log("send_as_email", exp.Message, true, exp);
        }
        return res;
    }
    private bool send_file_as_email(string data, string file_details, string to_email)
    {
        bool res = false;
        try
        {
          //  byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(data);
            return send_file_as_email(get_array(data), file_details, to_email);
        }
        catch (Exception exp)
        {
            common.Log("send_as_email", exp.Message, true, exp);
        }
        return res;
    }
    private byte[] get_array(string data)
    {
        return System.Text.Encoding.UTF8.GetBytes(data);
    }

    private string get_config(string key)
    {
        string res = "";
        try
        {
            ConnectionStringSettings mySetting = ConfigurationManager.ConnectionStrings[key];
            if (mySetting == null || string.IsNullOrEmpty(mySetting.ConnectionString))
                return "";
            return mySetting.ConnectionString;
        }
        catch (Exception exp)
        {
            common.Log("get_config", exp.Message, true, exp);
        }
        return res;
    }
}