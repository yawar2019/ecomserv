using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Drawing;
using System.Data.SqlClient;
using Oracle.DataAccess.Client;
using System.Net.Mail;
using System.Net;
using System.Drawing.Drawing2D;
using System.Configuration;
using System.Web.Configuration;
using BarcodeLib; 
public class clsCommon
{ 
    public  string version = "3.6"; 
    
    public  bool HasSpecialChars(string val)
    {
        try
        {
            var regexItem = new System.Text.RegularExpressions.Regex("^[a-zA-Z0-9]*$");
            if (!regexItem.IsMatch(val)) { return true; }
        }
        catch (Exception exp)
        {
            Log("CheckForSpecialChars", exp.Message, true, exp);
        }
        return false;
    }
   
    public  string GetDateInFormat(string date, string format)
    {
        if (date.Trim().Length == 0)
            return date;
        DateTime dateNew = DateTime.Now;
        if (DateTime.TryParse(date, out dateNew))
        {
            return dateNew.ToString(format);
        }
        else
        {
            Log("GetDateInFormat", "failed to convert date(" + date + ")", true, null);
            return date;
        }
    }

    public  string ImageToBase64(string jpg_filepath)
    {
        string res = "";
        try
        {
            if (!File.Exists(jpg_filepath))
            {
                Log("ImageToBase64", "File does not exist(" + jpg_filepath + ")", true, null);
                return "";
            }
            return ImageToBase64(Image.FromFile(jpg_filepath));
        }
        catch (Exception exp)
        {
            Log("ImageToBase64", exp.Message, true, exp);
        }
        return res;
    }
    public  string ImageToBase64(Image img)
    {
        string res = "";
        try
        {
            return "data:image/jpg;base64," + Convert.ToBase64String(ImageToByteArraybyImageConverter(img));
        }
        catch (Exception exp)
        {
            Log("ImageToBase643", exp.Message, true, exp);
        }
        return res;
    }
    private  byte[] ImageToByteArraybyImageConverter(System.Drawing.Image image)
    {
        ImageConverter imageConverter = new ImageConverter();
        byte[] imageByte = (byte[])imageConverter.ConvertTo(image, typeof(byte[]));
        return imageByte;
    }
    public  bool IsEmpty(string val)
    {
        bool res = true;
        try
        {
            if (val == null)
                return res;
            if (val.ToString().Trim().Length == 0)
                return res;
            return false;
        }
        catch (Exception exp)
        {
            Log("IsEmpty", exp.Message, true, exp);
        }
        return res;
    }
    public string GetUniqNo()
    {
        string res = "";
        try
        {
            res = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            int _min = 10000;
            int _max = 99999;
            Random _rdm = new Random();
            res += "_" + _rdm.Next(_min, _max).ToString();
        }
        catch (Exception exp)
        {
            Log("GetUniqNo", exp.Message, true, exp);
        }
        return res;
    }
    public  string GetConfig(string key)
    {
        string res = "";
        try
        {
            res = System.Configuration.ConfigurationManager.AppSettings.Get(key);
        }
        catch (Exception exp)
        {
            Log("GetConfig", exp.Message, true, exp);
        }
        return res;
    } 
    public  void Log(string logDesc)
    {
        Log("LogInfo", logDesc, false, null);
    }
    public  void Log(string action, string logDesc, bool isError, Exception expp)
    {
        bomoserv.Common.clsLog clslog = new bomoserv.Common.clsLog();
        clslog.Log(action, logDesc, isError, expp);
    }
    public  string GetTodayToSave()
    {
        return DateTime.Now.ToString("dd MMM yyyy HH:mm:ss");
    }
    public  string GetTimeStamp()
    {
        return DateTime.Now.ToString("yyyyMMddHHmmssfff");
    }
    public  bool GetDate(string date, string formate, out DateTime dateNew)
    {
        dateNew = DateTime.Now;
        if (DateTime.TryParseExact(date, formate, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateNew))
            return true;
        return false;
    }
    public  string GetDate(string date_str_input, string formate)
    { 
        DateTime dateNew = DateTime.Now;
        if (DateTime.TryParse(date_str_input, out dateNew))
            return dateNew.ToString(formate);
        return date_str_input;
    } 
      
    public  string GetForSQL(string val)
    {
        string res = "";
        try
        {
            res = val.Replace("'", "");
            res = res.Replace("&", "and");
            res = res.Replace("\"", "");
        }
        catch (Exception exp)
        {
            Log("GetForSQL", exp.Message, true, exp);
        }
        return res;
    }
    public bool alert_me(string sub, string MailBody)
    {
        try
        {
            return send_email(false, GetConfig("email_alert_to"), sub, MailBody, null, false);
        }
        catch (Exception exp)
        {
            Log("alert_me", exp.Message + "(body:" + MailBody + ")", true, exp);
        }
        return false;
    }
    public bool send_email(bool is_html, string to, string sub, string MailBody, Attachment att, bool need_bcc)
    {
        try
        { 
            string Email_From = GetConfig("email_from");
            string smtp = GetConfig("smtp_from");
            string Email_FromPass = GetConfig("email_pass_from");
            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(smtp.Trim()); 
            client.Timeout = 100000000;
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            message.From = new System.Net.Mail.MailAddress(Email_From, GetConfig("email_from_name"));
            message.To.Add(to);
            if (need_bcc)
                message.Bcc.Add(GetConfig("email_sales_bcc"));
            message.Subject = sub;
            message.Body = MailBody;
            message.IsBodyHtml = is_html;
            if (att != null)
                message.Attachments.Add(att);
            System.Net.NetworkCredential myCreds = new System.Net.NetworkCredential(Email_From, Email_FromPass, "");
            client.Credentials = myCreds;
            client.Send(message);
            return true;
        }
        catch (Exception exp)
        {
            Log("send_email", exp.Message + "(body:" + MailBody + ")", true, exp); 
        }
        return false;
    }
}