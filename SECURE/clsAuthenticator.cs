using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Data;
using System.Data.SqlClient;

using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;
namespace ecomserv.SECURE
{
    public class clsAuthenticator
    {
        public string ApiKey = "iN8uyhtgrRETFRDE2432fvRGp09fXqaa";
      
        public bool IsValidString(string val)
        {
            bool res = false;
            try
            {
                if (val.ToLower().Trim().Contains("'"))
                    return false;
                if (val.ToLower().Trim().Contains('"'))
                    return false;
                if (val.ToLower().Trim().Contains("&"))
                    return false;
                return true;
            }
            catch (Exception exp)
            {
                clsCommon common = new clsCommon();
                common.Log("IsValidString", exp.Message + "(val=" + val + ")", true, exp);
            }
            return res;
        }
    }
}