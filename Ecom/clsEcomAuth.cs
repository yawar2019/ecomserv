using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ecomserv.Ecom
{
    public class clsEcomAuth
    {
        public string ApiKey = "ujujuaaZSW23WDEE2yhyhdujujfftr54";
        private string additional_key = "vintest";
        public string GetNewToken(string device_id)
        {
            string res = GetDateStr(DateTime.Now) + "," + device_id+","+ additional_key;
            clsEcomEncryptor enc = new clsEcomEncryptor();
            return enc.Encrypt(res);
        }
        public bool AuthToken(string token, string device_id)
        {
            clsEcomEncryptor enc = new clsEcomEncryptor();
            clsEcom clsecom = new clsEcom();
            string res = clsecom.get_value(token);
            if (res.Trim().Length == 0)
                return false;
            res = enc.Encrypt(res);
            if (res.Trim().Length == 0)
                return false;
            string[] splt = res.Split(',');
            if (splt.Length != 3)
                return false;
            if (splt[2] != additional_key)
                return false;
            string device_id_token = splt[1];
            if (device_id != device_id_token)
                return false;
            DateTime date1 = DateTime.Now;
            if (!DateTime.TryParse(splt[0], out date1))
                return false;
            TimeSpan ts = (TimeSpan)(DateTime.Now - date1);
            if (ts.TotalDays > 30)
                return false;
            return true;
        }
        public string GetDateStr(DateTime date)
        {
            return date.ToString("dd-MMM-yyyy HH:mm:ss");
        }
        public bool IsContainsSpecialChars(string val)
        {
            bool res = false;
            try
            {
                if (val.ToLower().Trim().Contains("'"))
                    return true;
                if (val.ToLower().Trim().Contains('"'))
                    return true;
                if (val.ToLower().Trim().Contains("&"))
                    return true;
                if (val.ToLower().Trim().Contains("--"))
                    return true;
            }
            catch (Exception exp)
            {
                clsEcom clsecom = new clsEcom();
                clsecom.Log("IsContainsSpecialChars", exp.Message + "(val=" + val + ")", true, exp, false);
            }
            return res;
        }
    }
}