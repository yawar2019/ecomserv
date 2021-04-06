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
using System.Security.Cryptography;
using ecomserv.SECURE;
using ecomserv.Common; 
using ecomserv.Ecom;
using System.Web.Mail; 

namespace ecomserv
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class ecompublic : iecompublic
    {


        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "/HandleEcom")]
        public EcomData HandleEcom(EcomData input)
        {
            string log_key = "HandleEcom";
            EcomData result = new EcomData();
            clsEcom clsecom = new clsEcom();
            result.data = "";
            result.status = "Failed. Please try later";
            result.status_msg = "";
            try
            {
                clsEcomAuth auth = new clsEcomAuth();
                if (input.api_key == null)
                {
                    clsecom.Log(log_key, "Received request without ApiKey(still safe)", true, null);
                    return result;
                }
                if (auth.IsContainsSpecialChars(input.api_key))
                {
                    clsecom.Log(log_key, "Received request with ApiKey with invalid values(still safe). Value:" + input.api_key, true, null);
                    return result;
                }
                if (input.api_key != auth.ApiKey)
                {
                    clsecom.Log(log_key, "Received request with wrong ApiKey(still safe, ApiKey Received :" + input.api_key + ")", true, null);
                    return result;
                }
                string token = clsecom.get_value(input.token);
                string app_identity = clsecom.get_value(input.app_identity);
                string cust_id = clsecom.get_value(input.cust_id);
                string staff_id = clsecom.get_value(input.staff_id);
                string role_type = clsecom.get_value(input.role_type);
                string store_id = clsecom.get_value(input.store_id);
                string dev_id = clsecom.get_value(input.dev_id);
                string dev_type = clsecom.get_value(input.dev_type);
                string reg_id = clsecom.get_value(input.reg_id);
                string lang_code = clsecom.get_value(input.lang_code);
                string status = clsecom.get_value(input.status);
                string status_msg = clsecom.get_value(input.status_msg);
                string flag = clsecom.get_value(input.flag);
                string data = clsecom.get_value(input.data);
                string time_offset = clsecom.get_value(input.time_offset);
                string res_msg = "";
                if (dev_id.Trim().Length == 0)
                {
                    clsecom.Log(log_key, "Received request with no device id(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                    return result;
                }
                if (dev_type.Trim().Length == 0)
                {
                    clsecom.Log(log_key, "Received request with no device type(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                    return result;
                }
                if (flag.Trim().Length == 0)
                {
                    clsecom.Log(log_key, "Received request with no flag(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                    return result;
                }
                if (flag != "init_app")
                {
                    if (clsecom.IsEmpty(store_id))
                    {
                        clsecom.Log(log_key, "Received request with no store_id(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                        return result;
                    }
                }
                dynamic inData = clsecom.getJsonStringToObject(data);
                if (inData == null)
                {
                    clsecom.Log(dev_id, store_id, log_key, "Json dynamic object null(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                    return result;
                }
                string query = "";
                clsConnectionSQL conn = new clsConnectionSQL();
                clsEcomAuth clsecomauth = new clsEcomAuth();
                switch (flag)
                {
                    case "get_staff_home":
                        {
                            string day_key = clsecom.get_value(inData.day_key);
                            string date_from = clsecom.get_value(inData.date_from);
                            string date_to = clsecom.get_value(inData.date_to);
                            res_msg = "";
                            EcomStaffHomeItem item = clsecom.get_staff_home(store_id, out res_msg);
                            result.status_msg = res_msg;
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "get_sales":
                        {
                            string day_key = clsecom.get_value(inData.day_key);
                            string date_from = clsecom.get_value(inData.date_from);
                            string date_to = clsecom.get_value(inData.date_to);
                            res_msg = "";
                            EcomSalesItem item = clsecom.get_sales(day_key, date_from, date_to, store_id, out res_msg);
                            result.status_msg = res_msg;
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "is_staff":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string email = clsecom.get_value(inData.email);
                            string app_identity_this = clsecom.get_value(inData.app_identity);
                            if (auth.IsContainsSpecialChars(email))
                            {
                                result.status_msg = "Email should not contain special characters";
                                return result;
                            }
                            query = @"SELECT  s.*
  FROM  [dbo].[ecom_staff] s
  inner join [dbo].[ecom_store] st on
  st.store_id=s.store_id and
  st.app_identity='" + app_identity + @"' 
  where s.[email]='" + email + @"'";
                            DataTable dtStaff = conn.getDataTable(query);
                            string is_staff = "N";
                            if (dtStaff.Rows.Count > 0)
                                is_staff = "Y";
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(is_staff);
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }

                    //===
                    case "get_offer_home":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string offer_id = clsecom.get_value(inData.offer_id);
                            string offer_by_brand_id = clsecom.get_value(inData.offer_by_brand_id);
                            query = @"SELECT r.[offer_id]
      ,r.[prod_id]
   ,r.[tag_id]
      ,r.[cat_id]
      ,r.[brand_id] 
,r.[offer_by_brand_id] 
  ,r.[offer_dec_en] 
  ,r.[offer_dec_ar] 
  ,r.[offer_perc]  
  ,convert(char(20),r.[valid_from],113) as valid_from
  ,convert(char(20),r.[valid_to],113) as valid_to 
  ,r.[coupon_code]   
      ,r.[created_by] 
      ,convert(char(20),r.[created_on],113) as created_on 
      ,r.[updated_by] 
 ,convert(char(20),r.[updated_on],113) as updated_on 
      ,r.[is_active] 
,r.offer_img_url 
  FROM [dbo].[ecom_offer] r  
left join ecom_staff esc on 
esc.staff_id=r.[created_by]

left join ecom_staff esu on 
esu.staff_id=r.[updated_by]

WHERE 
r.offer_by_brand_id=case when '" + offer_by_brand_id + @"'='' then r.offer_by_brand_id else '" + offer_by_brand_id + @"' end and 
r.offer_id=case when '" + offer_id + @"'='' then r.offer_id else '" + offer_id + @"' end 
and getdate() between r.[valid_from] and r.[valid_to] 
and isnull(r.[is_active],'N')='Y' 
order by r.valid_from,r.valid_to";
                            DataTable dtData = conn.getDataTable(query);
                            List<EcomOfferItem> items = new List<EcomOfferItem>();
                            for (int i = 0; i < dtData.Rows.Count; i++)
                            {
                                EcomOfferItem item = new EcomOfferItem();
                                item.offer_id = dtData.Rows[i]["offer_id"].ToString().Trim();
                                item.prod_id = dtData.Rows[i]["prod_id"].ToString().Trim();
                                item.cat_id = dtData.Rows[i]["cat_id"].ToString().Trim();
                                item.tag_id = dtData.Rows[i]["tag_id"].ToString().Trim();
                                item.offer_img_url = dtData.Rows[i]["offer_img_url"].ToString().Trim();
                                item.brand_id = dtData.Rows[i]["brand_id"].ToString().Trim();
                                item.offer_by_brand_id = dtData.Rows[i]["offer_by_brand_id"].ToString().Trim();
                                item.offer_dec_en = dtData.Rows[i]["offer_dec_en"].ToString().Trim();
                                item.offer_dec_ar = dtData.Rows[i]["offer_dec_ar"].ToString().Trim();
                                item.offer_perc = dtData.Rows[i]["offer_perc"].ToString().Trim();
                                item.valid_from = dtData.Rows[i]["valid_from"].ToString().Trim();
                                item.valid_to = dtData.Rows[i]["valid_to"].ToString().Trim();
                                item.coupon_code = dtData.Rows[i]["coupon_code"].ToString().Trim();
                                // item.created_by = dtData.Rows[i]["created_by"].ToString().Trim();

                                /// item.created_by_name = dtData.Rows[i]["created_by_name"].ToString().Trim();
                                // item.created_on = dtData.Rows[i]["created_on"].ToString().Trim();
                                //  item.updated_by = dtData.Rows[i]["updated_by"].ToString().Trim();
                                //  item.updated_by_name = dtData.Rows[i]["updated_by_name"].ToString().Trim();
                                //   item.updated_on = dtData.Rows[i]["updated_on"].ToString().Trim();
                                // item.is_active = dtData.Rows[i]["is_active"].ToString().Trim();

                                items.Add(item);
                            }
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(items);
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "get_offer":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string offer_id = clsecom.get_value(inData.offer_id);
                            string offer_by_brand_id = clsecom.get_value(inData.offer_by_brand_id);
                            query = @"SELECT r.[offer_id]
      ,r.[prod_id]
   ,r.[tag_id]
      ,r.[cat_id]
      ,r.[brand_id] 
,r.[offer_by_brand_id] 
  ,r.[offer_dec_en] 
  ,r.[offer_dec_ar] 
  ,r.[offer_perc]  
  ,convert(char(20),r.[valid_from],113) as valid_from
  ,convert(char(20),r.[valid_to],113) as valid_to 
  ,r.[coupon_code]  
      ,r.[created_by]
      ,convert(char(20),r.[created_on],113) as created_on
      ,r.[updated_by]
 ,convert(char(20),r.[updated_on],113) as updated_on 
      ,r.[is_active]
,r.offer_img_url
  FROM [dbo].[ecom_offer] r  
left join ecom_staff esc on 
esc.staff_id=r.[created_by]

left join ecom_staff esu on 
esu.staff_id=r.[updated_by]

WHERE 
r.offer_by_brand_id=case when '" + offer_by_brand_id + @"'='' then r.offer_by_brand_id else '" + offer_by_brand_id + @"' end and 
r.offer_id=case when '" + offer_id + @"'='' then r.offer_id else '" + offer_id + @"' end order by r.valid_from,r.valid_to";
                            DataTable dtData = conn.getDataTable(query);
                            List<EcomOfferItem> items = new List<EcomOfferItem>();
                            for (int i = 0; i < dtData.Rows.Count; i++)
                            {
                                EcomOfferItem item = new EcomOfferItem();
                                item.offer_id = dtData.Rows[i]["offer_id"].ToString().Trim();
                                item.prod_id = dtData.Rows[i]["prod_id"].ToString().Trim();
                                item.cat_id = dtData.Rows[i]["cat_id"].ToString().Trim();
                                item.tag_id = dtData.Rows[i]["tag_id"].ToString().Trim();
                                item.offer_img_url = dtData.Rows[i]["offer_img_url"].ToString().Trim();
                                item.brand_id = dtData.Rows[i]["brand_id"].ToString().Trim();
                                item.offer_by_brand_id = dtData.Rows[i]["offer_by_brand_id"].ToString().Trim();
                                item.offer_dec_en = dtData.Rows[i]["offer_dec_en"].ToString().Trim();
                                item.offer_dec_ar = dtData.Rows[i]["offer_dec_ar"].ToString().Trim();
                                item.offer_perc = dtData.Rows[i]["offer_perc"].ToString().Trim();
                                item.valid_from = dtData.Rows[i]["valid_from"].ToString().Trim();
                                item.valid_to = dtData.Rows[i]["valid_to"].ToString().Trim();
                                item.coupon_code = dtData.Rows[i]["coupon_code"].ToString().Trim();
                                item.created_by = dtData.Rows[i]["created_by"].ToString().Trim();

                               // item.created_by_name = dtData.Rows[i]["created_by_name"].ToString().Trim();
                                item.created_on = dtData.Rows[i]["created_on"].ToString().Trim();
                                item.updated_by = dtData.Rows[i]["updated_by"].ToString().Trim();
                               // item.updated_by_name = dtData.Rows[i]["updated_by_name"].ToString().Trim();
                                item.updated_on = dtData.Rows[i]["updated_on"].ToString().Trim();
                                item.is_active = dtData.Rows[i]["is_active"].ToString().Trim();

                                items.Add(item);
                            }
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(items);
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "delete_offer":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string offer_id = clsecom.get_value(inData.offer_id);
                            string photo_url_to_delete = conn.ExecuteScalar("SELECT  [offer_img_url]  FROM [dbo].[ecom_offer] where [offer_id]='" + offer_id + "'");

                            query = @"DELETE FROM [dbo].[ecom_offer] WHERE  offer_id='" + offer_id + "'";
                            string ret_msg = "";
                            if (!conn.ExecuteNonQuery(query, "", false, out ret_msg))
                            {
                                if (clsecom.is_fk_issue(ret_msg))
                                    result.status_msg = "Cannot delete. Data is using in another form.";
                                return result;
                            }
                            clsecom.SchDeletePic(photo_url_to_delete);
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "update_offer":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string offer_id = clsecom.get_value(inData.offer_id);
                            string tag_id = clsecom.get_value(inData.tag_id);
                            string offer_img_data = clsecom.get_value(inData.offer_img_data);
                            bool is_exist = false;
                            string offer_img_url = clsecom.EcomSavePicAndGetFileName(offer_img_data, out is_exist);
                            string photo_url_to_delete = "";
                            if (!is_exist)
                            {
                                photo_url_to_delete = conn.ExecuteScalar("SELECT  [offer_img_data]  FROM [dbo].[ecom_offer] where [offer_id]='" + offer_id + "'");
                            }
                            string prod_id = clsecom.get_value(inData.prod_id);
                            string cat_id = clsecom.get_value(inData.cat_id);
                            string brand_id = clsecom.get_value(inData.brand_id);
                            string offer_by_brand_id = clsecom.get_value(inData.offer_by_brand_id);
                            string offer_dec_en = clsecom.get_value(inData.offer_dec_en);
                            if (auth.IsContainsSpecialChars(offer_dec_en))
                            {
                                result.status_msg = "Offer Description in English should not contain special characters";
                                return result;
                            }
                            string offer_dec_ar = clsecom.get_value(inData.offer_dec_ar);
                            if (auth.IsContainsSpecialChars(offer_dec_ar))
                            {
                                result.status_msg = "Offer Description in Arabic should not contain special characters";
                                return result;
                            }
                            string offer_perc = clsecom.get_value(inData.offer_perc);
                            if (auth.IsContainsSpecialChars(offer_perc))
                            {
                                result.status_msg = "Offer Percentage in Arabic should not contain special characters";
                                return result;
                            }

                            string valid_from = clsecom.get_value(inData.valid_from);
                            if (auth.IsContainsSpecialChars(valid_from))
                            {
                                result.status_msg = "Valid From should not contain special characters";
                                return result;
                            }

                            string valid_to = clsecom.get_value(inData.valid_to);
                            if (auth.IsContainsSpecialChars(valid_to))
                            {
                                result.status_msg = "Valid To should not contain special characters";
                                return result;
                            }

                            string coupon_code = clsecom.get_value(inData.coupon_code);
                            if (auth.IsContainsSpecialChars(coupon_code))
                            {
                                result.status_msg = "Coupen Code should not contain special characters";
                                return result;
                            }

                            string is_active = clsecom.get_value(inData.is_active);

                            query = @"UPDATE [dbo].[ecom_offer]
   SET [prod_id] ='" + prod_id + @"' 
      ,[cat_id] ='" + cat_id + @"'  
      ,[brand_id] ='" + brand_id + @"'  
  ,[offer_by_brand_id] ='" + offer_by_brand_id + @"'
 ,[tag_id] ='" + tag_id + @"'  
      ,[offer_dec_en] ='" + offer_dec_en + @"'  
      ,[offer_dec_ar] =N'" + offer_dec_ar + @"' 
      ,[offer_perc] ='" + offer_perc + @"'  
      ,[valid_from] ='" + valid_from + @"' 
      ,[valid_to] ='" + valid_to + @"' 
      ,[coupon_code] ='" + coupon_code + @"' 
      ,[updated_by] ='" + staff_id + @"' 
      ,[updated_on] =getdate()
      ,[is_active] ='" + is_active + @"' 
,[offer_img_url]='" + offer_img_url + @"'
 WHERE offer_id='" + offer_id + @"'";
                            if (!conn.ExecuteNonQuery(query))
                                return result;
                            clsecom.SchDeletePic(photo_url_to_delete);
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "add_offer":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string tag_id = clsecom.get_value(inData.tag_id);
                            string offer_img_url = clsecom.get_value(inData.offer_img_url);
                           // string offer_img_data = clsecom.get_value(inData.offer_img_data);
                            bool is_exist = false;
                           // offer_img_url = clsecom.EcomSavePicAndGetFileName(offer_img_data, out is_exist);
                            string offer_id = clsecom.get_value(inData.offer_id);
                            string prod_id = clsecom.get_value(inData.prod_id);
                            string cat_id = clsecom.get_value(inData.cat_id);
                            string brand_id = clsecom.get_value(inData.brand_id);
                            string offer_by_brand_id = clsecom.get_value(inData.offer_by_brand_id);
                            string offer_dec_en = clsecom.get_value(inData.offer_dec_en);
                           
                            if (auth.IsContainsSpecialChars(offer_dec_en))
                            {
                                result.status_msg = "Offer Description in English should not contain special characters";
                                return result;
                            }
                            string offer_dec_ar = clsecom.get_value(inData.offer_dec_ar);
                            if (auth.IsContainsSpecialChars(offer_dec_ar))
                            {
                                result.status_msg = "Offer Description in Arabic should not contain special characters";
                                return result;
                            }
                            string offer_perc = clsecom.get_value(inData.offer_perc);
                            if (auth.IsContainsSpecialChars(offer_perc))
                            {
                                result.status_msg = "Offer Percentage in Arabic should not contain special characters";
                                return result;
                            }

                            string valid_from = clsecom.get_value(inData.valid_from);
                            if (auth.IsContainsSpecialChars(valid_from))
                            {
                                result.status_msg = "Valid From should not contain special characters";
                                return result;
                            }

                            string valid_to = clsecom.get_value(inData.valid_to);
                            if (auth.IsContainsSpecialChars(valid_to))
                            {
                                result.status_msg = "Valid To should not contain special characters";
                                return result;
                            }

                            string coupon_code = clsecom.get_value(inData.coupon_code);
                            if (auth.IsContainsSpecialChars(coupon_code))
                            {
                                result.status_msg = "Coupen Code should not contain special characters";
                                return result;
                            }

                            string is_active = clsecom.get_value(inData.is_active);

                            query = @"INSERT INTO [dbo].[ecom_offer]
           ([prod_id]
           ,[cat_id]
           ,[brand_id]
           ,[offer_by_brand_id]
            ,[tag_id]
           ,[offer_dec_en]
           ,[offer_dec_ar]
           ,[offer_perc] 
           ,[valid_from]
           ,[valid_to]
           ,[coupon_code]
           ,[created_by]
           ,[created_on] 
           ,[is_active]
           ,[offer_img_url])
     VALUES
           ('" + prod_id + @"' 
           ,'" + cat_id + @"'  
           ,'" + brand_id + @"' 
           ,'" + offer_by_brand_id + @"' 
            ,'" + tag_id + @"' 
           ,'" + offer_dec_en + @"'  
           ,'" + offer_dec_ar + @"' 
           ,'" + offer_perc + @"'  
           ,'" + valid_from + @"' 
           ,'" + valid_to + @"' 
           ,'" + coupon_code + @"' 
           ,'" + staff_id + @"' 
           ,getdate() 
           ,'" + is_active + @"'
           ,'" + offer_img_url + "')";
                            if (!conn.ExecuteNonQuery(query))
                                return result;
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    //===
                    case "get_products_staff":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            res_msg = "";
                            List<EcomProductData> prods = clsecom.getProductsDyna(inData, out res_msg);
                            if (!clsecom.IsEmpty(res_msg))
                            {
                                result.status_msg = res_msg;
                                return result;
                            }
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(prods);
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "delete_product":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string prod_id = clsecom.get_value(inData.prod_id);
                            SqlCommand comm = conn.GetCommandWithTransaction();

                            DataTable dtAssetsToDelete = conn.getDataTable(@"SELECT  [img_url]
        FROM [dbo].[ecom_product_asset] where [prod_id] = '" + prod_id + @"'", comm);
                            string ret_msg = "";
                            if (!conn.ExecuteNonQuery("delete [dbo].[ecom_product_asset] where [prod_id]='" + prod_id + "'", comm, out ret_msg))
                            {
                                if (clsecom.is_fk_issue(ret_msg))
                                {
                                    result.status_msg = "Cannot delete. Asset Data is using in another form.";
                                    conn.RollBack(comm);
                                    return result;
                                }
                            }

                            if (!conn.ExecuteNonQuery("delete [dbo].[ecom_product_color] where [prod_id]='" + prod_id + "'", comm, out ret_msg))
                            {
                                if (clsecom.is_fk_issue(ret_msg))
                                {
                                    result.status_msg = "Cannot delete. Color Data is using in another form.";
                                    conn.RollBack(comm);
                                    return result;
                                }
                            }

                            DataTable dtAssetsToDeleteWishCard = conn.getDataTable(@"SELECT  [img_url]
        FROM [dbo].[ecom_product_gift_wish_card] where [prod_id] = '" + prod_id + @"'", comm);
                            if (!conn.ExecuteNonQuery("delete [dbo].[ecom_product_gift_wish_card] where [prod_id]='" + prod_id + "'", comm, out ret_msg))
                            {
                                if (clsecom.is_fk_issue(ret_msg))
                                {
                                    result.status_msg = "Cannot delete. Wish card Data is using in another form.";
                                    conn.RollBack(comm);
                                    return result;
                                }
                            }

                            if (!conn.ExecuteNonQuery("delete [dbo].[ecom_product_review] where [prod_id]='" + prod_id + "'", comm, out ret_msg))
                            {
                                if (clsecom.is_fk_issue(ret_msg))
                                {
                                    result.status_msg = "Cannot delete. Review Data is using in another form.";
                                    conn.RollBack(comm);
                                    return result;
                                }
                            }

                            if (!conn.ExecuteNonQuery("delete [dbo].[ecom_product_size] where [prod_id]='" + prod_id + "'", comm, out ret_msg))
                            {
                                if (clsecom.is_fk_issue(ret_msg))
                                {
                                    result.status_msg = "Cannot delete. Size Data is using in another form.";
                                    conn.RollBack(comm);
                                    return result;
                                }
                            }

                            if (!conn.ExecuteNonQuery("delete [dbo].[ecom_product_stock] where [prod_id]='" + prod_id + "'", comm, out ret_msg))
                            {
                                if (clsecom.is_fk_issue(ret_msg))
                                {
                                    result.status_msg = "Cannot delete. Stock Data is using in another form.";
                                    conn.RollBack(comm);
                                    return result;
                                }
                            }

                            if (!conn.ExecuteNonQuery("delete [dbo].[ecom_product_tag] where [prod_id]='" + prod_id + "'", comm, out ret_msg))
                            {
                                if (clsecom.is_fk_issue(ret_msg))
                                {
                                    result.status_msg = "Cannot delete. Tag Data is using in another form.";
                                    conn.RollBack(comm);
                                    return result;
                                }
                            }

                            string custom_size_img_url_to_delete = conn.ExecuteScalar("SELECT  [custom_size_img_url]  FROM [dbo].[ecom_product] where [prod_id]='" + prod_id + "'");

                            if (!conn.ExecuteNonQuery("delete [dbo].[ecom_product] where [prod_id]='" + prod_id + "'", comm, out ret_msg))
                            {
                                if (clsecom.is_fk_issue(ret_msg))
                                {
                                    result.status_msg = "Cannot delete. Product Data is using in another form.";
                                }
                                conn.RollBack(comm);
                                return result;
                            }
                            conn.Commit(comm);
                            for (int i = 0; i < dtAssetsToDelete.Rows.Count; i++)
                            {
                                clsecom.SchDeletePic(dtAssetsToDelete.Rows[i]["img_url"].ToString().ToLower());
                            }
                            for (int i = 0; i < dtAssetsToDeleteWishCard.Rows.Count; i++)
                            {
                                clsecom.SchDeletePic(dtAssetsToDeleteWishCard.Rows[i]["img_url"].ToString().ToLower());
                            }
                            clsecom.SchDeletePic(custom_size_img_url_to_delete.ToString().ToLower());
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "update_product":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string prod_id = clsecom.get_value(inData.prod_id);
                            string brand_id = clsecom.get_value(inData.brand_id);
                            string stat = "";
                            if (!clsecom.IsEmptyAndContainsSpecial(brand_id, "Brand", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }
                            string cat_id = clsecom.get_value(inData.cat_id);
                            if (!clsecom.IsEmptyAndContainsSpecial(cat_id, "Category", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }
                            string name_en = clsecom.get_value(inData.name_en);
                            if (!clsecom.IsEmptyAndContainsSpecial(name_en, "Name in English", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }
                            string name_ar = clsecom.get_value(inData.name_ar);
                            if (!clsecom.IsEmptyAndContainsSpecial(name_ar, "Name in Arabic", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }
                            string name_desc_en = clsecom.get_value(inData.name_desc_en);
                            if (!clsecom.IsContainsSpecial(name_desc_en, "Description in English", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }
                            string name_desc_ar = clsecom.get_value(inData.name_desc_ar);
                            if (!clsecom.IsContainsSpecial(name_desc_ar, "Description in Arabic", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }
                            string quantity = clsecom.get_value(inData.quantity);
                            if (clsecom.IsEmpty(quantity))
                                quantity = "1";
                            if (!clsecom.IsContainsSpecial(quantity, "Quantity", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }
                            string unit_price = clsecom.get_value(inData.unit_price);
                            if (!clsecom.IsEmptyAndContainsSpecial(unit_price, "Unit Price", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }

                            string is_custom_size_available = clsecom.get_value(inData.is_custom_size_available);
                            if (clsecom.IsEmpty(is_custom_size_available))
                                is_custom_size_available = "N";
                            string custom_size_img_data = "";
                            string custom_size_img_url = "";
                            string custom_size_description = "";
                            string custom_size_field1 = "";
                            string custom_size_field2 = "";
                            string custom_size_field3 = "";
                            string custom_size_field4 = "";
                            string custom_size_field5 = "";
                            string custom_size_field6 = "";
                            string custom_size_field7 = "";
                            string custom_size_field8 = "";
                            string custom_size_field9 = "";
                            string custom_size_field10 = "";

                            string custom_size_img_url_to_delete = "";

                            if (clsecom.get_bool_from_yn(is_custom_size_available))
                            {
                                custom_size_img_data = clsecom.get_value(inData.custom_size_img_data);
                                if (!clsecom.IsEmpty(custom_size_img_data, "Image for Custom Size", out stat))
                                {
                                    if (!clsecom.IsEmpty(stat))
                                        result.status_msg = stat;
                                    return result;
                                }

                                bool is_exist = false;
                                custom_size_img_url = clsecom.EcomSavePicAndGetFileName(custom_size_img_data, out is_exist);

                                if (!is_exist)
                                {
                                    custom_size_img_url_to_delete = conn.ExecuteScalar("SELECT  [custom_size_img_url]  FROM [dbo].[ecom_product] where [prod_id]='" + prod_id + "'");
                                }

                                custom_size_description = clsecom.get_value(inData.custom_size_description);
                                custom_size_field1 = clsecom.get_value(inData.custom_size_field1);
                                if (clsecom.IsEmpty(custom_size_field1))
                                {
                                    result.status_msg = "Atleast one field is needed for Custom Size";
                                    return result;
                                }
                                custom_size_field2 = clsecom.get_value(inData.custom_size_field2);
                                custom_size_field3 = clsecom.get_value(inData.custom_size_field3);
                                custom_size_field4 = clsecom.get_value(inData.custom_size_field4);
                                custom_size_field5 = clsecom.get_value(inData.custom_size_field5);
                                custom_size_field6 = clsecom.get_value(inData.custom_size_field6);
                                custom_size_field7 = clsecom.get_value(inData.custom_size_field7);
                                custom_size_field8 = clsecom.get_value(inData.custom_size_field8);
                                custom_size_field9 = clsecom.get_value(inData.custom_size_field9);
                                custom_size_field10 = clsecom.get_value(inData.custom_size_field10);
                            }
                            string is_gift_available = clsecom.get_value(inData.is_gift_available);
                            string is_surprise_gift_available = clsecom.get_value(inData.is_surprise_gift_available);
                            string size_chart_img_url = clsecom.get_value(inData.size_chart_img_url);
                            string age_from = clsecom.get_value(inData.age_from);
                            string age_to = clsecom.get_value(inData.age_to);
                            string admin_approval_status = clsecom.get_value(inData.admin_approval_status);
                            if (clsecom.IsEmpty(admin_approval_status))
                                admin_approval_status = "pending_approval";
                            string admin_approval_remarks = clsecom.get_value(inData.admin_approval_remarks);
                            string order_by = clsecom.get_value(inData.order_by);
                            string is_active = clsecom.get_value(inData.is_active);

                            DataTable dtExist = conn.getDataTable(@"SELECT * FROM [dbo].[ecom_product] where [name_en] = '" + name_en + @"' and store_id='" + store_id + "' and prod_id!='" + prod_id + @"'");
                            if (dtExist.Rows.Count > 0)
                            {
                                result.status_msg = "Name in English already existing";
                                return result;
                            }
                            dtExist = conn.getDataTable(@"SELECT * FROM [dbo].[ecom_product] where [name_ar] = '" + name_ar + @"' and store_id='" + store_id + "' and prod_id!='" + prod_id + @"'");
                            if (dtExist.Rows.Count > 0)
                            {
                                result.status_msg = "Name in Arabic already existing";
                                return result;
                            }
                            if (auth.IsContainsSpecialChars(order_by))
                            {
                                result.status_msg = "Order By should not contain special characters";
                                return result;
                            }

                            SqlCommand comm = conn.GetCommandWithTransaction();
                            query = @"UPDATE [dbo].[ecom_product]
   SET [brand_id] = '" + brand_id + @"'
      ,[cat_id] = '" + cat_id + @"'
      ,[name_en] = '" + name_en + @"'
      ,[name_ar] = N'" + name_ar + @"'
      ,[name_desc_en] = '" + name_desc_en + @"'
      ,[name_desc_ar] = N'" + name_desc_ar + @"'
      ,[quantity] = '" + quantity + @"'
      ,[unit_price] = '" + unit_price + @"'
      ,[is_custom_size_available] = '" + is_custom_size_available + @"'
      ,[custom_size_img_url] = '" + custom_size_img_url + @"'
      ,[custom_size_description] = '" + custom_size_description + @"'
      ,[custom_size_field1] = '" + custom_size_field1 + @"'
      ,[custom_size_field2] = '" + custom_size_field2 + @"'
      ,[custom_size_field3] = '" + custom_size_field3 + @"'
      ,[custom_size_field4] = '" + custom_size_field4 + @"'
      ,[custom_size_field5] = '" + custom_size_field5 + @"'
      ,[custom_size_field6] = '" + custom_size_field6 + @"'
      ,[custom_size_field7] = '" + custom_size_field7 + @"'
      ,[custom_size_field8] = '" + custom_size_field8 + @"'
      ,[custom_size_field9] = '" + custom_size_field9 + @"'
      ,[custom_size_field10] = '" + custom_size_field10 + @"'
      ,[is_gift_available] = '" + is_gift_available + @"'
      ,[is_surprise_gift_available] = '" + is_surprise_gift_available + @"'
      ,[size_chart_img_url] = '" + size_chart_img_url + @"'
      ,[age_from] = '" + age_from + @"'
      ,[age_to] = '" + age_to + @"'
      ,[admin_approval_status] = '" + admin_approval_status + @"'
      ,[admin_approval_remarks] = '" + admin_approval_remarks + @"'
      ,[order_by] = '" + order_by + @"'
      ,[updated_by] = '" + staff_id + @"'
      ,[updated_on] =getdate()
      ,[is_active] ='" + is_active + @"'
 WHERE prod_id='" + prod_id + "'";
                            if (!conn.ExecuteNonQuery(query, comm))
                            {
                                conn.RollBack(comm);
                                return result;
                            }
                            if (inData.asset == null)
                            {
                                conn.RollBack(comm);
                                result.status_msg = "Please add atleast one photo of product";
                                return result;
                            }
                            if (inData.asset.Count == 0)
                            {
                                conn.RollBack(comm);
                                result.status_msg = "Please add atleast one photo of product";
                                return result;
                            }

                            DataTable dtAssetsToDelete = conn.getDataTable(@"SELECT  [img_url]
        FROM [dbo].[ecom_product_asset] where [prod_id] = '" + prod_id + @"'", comm);

                            conn.ExecuteNonQuery("delete [dbo].[ecom_product_asset] where [prod_id]='" + prod_id + "'", comm);
                            List<string> images_to_keep = new List<string>();
                            if (inData.asset == null)
                            {
                                conn.RollBack(comm);
                                result.status_msg = "Please add atleast one photo of product";
                                return result;
                            }
                            if (inData.asset.Count == 0)
                            {
                                conn.RollBack(comm);
                                result.status_msg = "Please add atleast one photo of product";
                                return result;
                            }
                            for (int i = 0; i < inData.asset.Count; i++)
                            {
                                string asset_id = clsecom.get_value(inData.asset[i].asset_id);
                                string type = clsecom.get_value(inData.asset[i].type);
                                string asset_data = clsecom.get_value(inData.asset[i].img_data);
                                bool is_exist = false;
                                string img_url = "";
                                if (type.ToLower() == "video")
                                {
                                    img_url = clsecom.EcomSaveVideoAndGetFileName(asset_data, out is_exist);
                                }
                                else if (type.ToLower() == "image")
                                {
                                    img_url = clsecom.EcomSavePicAndGetFileName(asset_data, out is_exist);
                                }
                                else
                                {
                                    conn.RollBack(comm);
                                    result.status_msg = "One of photos are not correct(Error Code: 361)";
                                    return result;
                                }
                                if (clsecom.IsEmpty(img_url))
                                {
                                    conn.RollBack(comm);
                                    result.status_msg = "One or more of the assets not correct(Error Code: 366)";
                                    return result;
                                }
                                string is_default = clsecom.get_value(inData.asset[i].is_default);
                                string order_by_asset = clsecom.get_value(inData.asset[i].order_by);
                                string is_active_asset = clsecom.get_value(inData.asset[i].is_active);
                                query = @"INSERT INTO [dbo].[ecom_product_asset]
           ([prod_id]
           ,[type]
           ,[img_url]
           ,[is_default]
           ,[order_by]
           ,[created_by]
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + prod_id + @"' 
           ,'" + type + @"' 
           ,'" + img_url + @"' 
           ,'" + is_default + @"' 
           ,'" + order_by_asset + @"' 
           ,'" + staff_id + @"' 
           ,getdate()
           ,'" + is_active_asset + @"')";
                                if (!conn.ExecuteNonQuery(query, comm))
                                {
                                    result.status_msg = "Failed to save asset(s)";
                                    conn.RollBack(comm);
                                    return result;
                                }
                                images_to_keep.Add(img_url);
                            }
                            conn.ExecuteNonQuery("delete [dbo].[ecom_product_color] where [prod_id]='" + prod_id + "'", comm);
                            if (inData.color != null)
                            {
                                for (int i = 0; i < inData.color.Count; i++)
                                {

                                    query = @"INSERT INTO [dbo].[ecom_product_color]
           ([prod_id]
           ,[color_name_en]
           ,[color_name_ar]
           ,[color_code] 
           ,[order_by]
           ,[created_by]
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + prod_id + @"'
           ,'" + clsecom.get_value(inData.color[i].color_name_en) + @"'
           ,N'" + clsecom.get_value(inData.color[i].color_name_ar) + @"'
           ,'" + clsecom.get_value(inData.color[i].color_code) + @"'
           ,'" + clsecom.get_value(inData.color[i].order_by) + @"'
           ,'" + staff_id + @"'
           ,getdate()
           ,'Y')";
                                    if (!conn.ExecuteNonQuery(query, comm))
                                    {
                                        result.status_msg = "Failed to save color(s)";
                                        conn.RollBack(comm);
                                        return result;
                                    }
                                }
                            }

                            DataTable dtAssetsToDeleteWishCard = conn.getDataTable(@"SELECT  [img_url]
        FROM [dbo].[ecom_product_gift_wish_card] where [prod_id] = '" + prod_id + @"'", comm);
                            conn.ExecuteNonQuery("delete [dbo].[ecom_product_gift_wish_card] where [prod_id]='" + prod_id + "'", comm);
                            if (inData.wishcard != null)
                            {
                                for (int i = 0; i < inData.wishcard.Count; i++)
                                {
                                    string img_data = inData.wishcard[i].img_data;
                                    bool is_exist = false;
                                    string img_url = clsecom.EcomSavePicAndGetFileName(img_data, out is_exist);
                                    query = @"INSERT INTO [dbo].[ecom_product_gift_wish_card]
           ([prod_id]
           ,[img_url]
           ,[order_by]
           ,[created_by]
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + prod_id + @"'
           ,'" + img_url + @"'
          ,'" + clsecom.get_value(inData.wishcard[i].order_by) + @"'
            ,'" + staff_id + @"'
           ,getdate()
           ,'Y')";
                                    if (!conn.ExecuteNonQuery(query, comm))
                                    {
                                        result.status_msg = "Failed to save wish card(s)";
                                        conn.RollBack(comm);
                                        return result;
                                    }
                                    images_to_keep.Add(img_url);
                                }
                            }

                            conn.ExecuteNonQuery("delete [dbo].[ecom_product_size] where [prod_id]='" + prod_id + "'", comm);
                            if (inData.size != null)
                            {
                                for (int i = 0; i < inData.size.Count; i++)
                                {
                                    query = @"INSERT INTO [dbo].[ecom_product_size]
           ([prod_id]
           ,[size_name_en]
           ,[size_name_ar]
           ,[order_by]
           ,[created_by]
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + prod_id + @"'
           ,'" + clsecom.get_value(inData.size[i].size_name_en) + @"'
         ,N'" + clsecom.get_value(inData.size[i].size_name_ar) + @"'
          ,'" + clsecom.get_value(inData.size[i].order_by) + @"'
           ,'" + prod_id + @"'
           ,getdate()
           ,'Y')";
                                    if (!conn.ExecuteNonQuery(query, comm))
                                    {
                                        result.status_msg = "Failed to save size(s)";
                                        conn.RollBack(comm);
                                        return result;
                                    }
                                }
                            }

                            conn.ExecuteNonQuery("delete [dbo].[ecom_product_stock] where [prod_id]='" + prod_id + "'", comm);
                            if (inData.stock != null)
                            {
                                for (int i = 0; i < inData.stock.Count; i++)
                                {
                                    query = @"INSERT INTO [dbo].[ecom_product_stock]
           ([prod_id]
           ,[product_size_id]
           ,[product_color_id]
           ,[stock_count]
           ,[order_by]
           ,[created_by]
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + prod_id + @"'
           ,'" + clsecom.get_value(inData.stock[i].product_size_id) + @"'
           ,'" + clsecom.get_value(inData.stock[i].product_color_id) + @"'
           ,'" + clsecom.get_value(inData.stock[i].stock_count) + @"'
           ,'" + clsecom.get_value(inData.stock[i].order_by) + @"'
           ,'" + staff_id + @"'
           ,getdate()
           ,'Y')";
                                    if (!conn.ExecuteNonQuery(query, comm))
                                    {
                                        result.status_msg = "Failed to save stock(s)";
                                        conn.RollBack(comm);
                                        return result;
                                    }
                                }
                            }

                            conn.ExecuteNonQuery("delete [dbo].[ecom_product_tag] where [prod_id]='" + prod_id + "'", comm);
                            if (inData.tag != null)
                            {
                                for (int i = 0; i < inData.tag.Count; i++)
                                {
                                    query = @"INSERT INTO [dbo].[ecom_product_tag]
           ([prod_id]
           ,[tag_id]
           ,[created_by]
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + prod_id + @"'
           ,'" + clsecom.get_value(inData.tag[i].tag_id) + @"'
           ,'" + staff_id + @"'
           ,getdate()
           ,'Y')";
                                    if (!conn.ExecuteNonQuery(query, comm))
                                    {
                                        result.status_msg = "Failed to save tag(s)";
                                        conn.RollBack(comm);
                                        return result;
                                    }
                                }
                            }
                            conn.Commit(comm);
                            for (int i = 0; i < dtAssetsToDelete.Rows.Count; i++)
                            {
                                bool got = false;
                                for (int j = 0; j < images_to_keep.Count; j++)
                                {
                                    if (dtAssetsToDelete.Rows[i]["img_url"].ToString().ToLower().Contains(images_to_keep[j].ToLower()))
                                    {
                                        got = true;
                                        break;
                                    }
                                }
                                if (got)
                                    continue;
                                clsecom.SchDeletePic(dtAssetsToDelete.Rows[i]["img_url"].ToString().ToLower());
                            }
                            for (int i = 0; i < dtAssetsToDeleteWishCard.Rows.Count; i++)
                            {
                                bool got = false;
                                for (int j = 0; j < images_to_keep.Count; j++)
                                {
                                    if (dtAssetsToDeleteWishCard.Rows[i]["img_url"].ToString().ToLower().Contains(images_to_keep[j].ToLower()))
                                    {
                                        got = true;
                                        break;
                                    }
                                }
                                if (got)
                                    continue;
                                clsecom.SchDeletePic(dtAssetsToDeleteWishCard.Rows[i]["img_url"].ToString().ToLower());
                            }
                            clsecom.SchDeletePic(custom_size_img_url_to_delete.ToString().ToLower());
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "add_product":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string brand_id = clsecom.get_value(inData.brand_id);
                            string stat = "";
                            if (!clsecom.IsEmptyAndContainsSpecial(brand_id, "Brand", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }
                            string cat_id = clsecom.get_value(inData.cat_id);
                            if (!clsecom.IsEmptyAndContainsSpecial(cat_id, "Category", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }
                            string name_en = clsecom.get_value(inData.name_en);
                            if (!clsecom.IsEmptyAndContainsSpecial(name_en, "Name in English", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }
                            string name_ar = clsecom.get_value(inData.name_ar);
                            if (!clsecom.IsEmptyAndContainsSpecial(name_ar, "Name in Arabic", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }
                            string name_desc_en = clsecom.get_value(inData.name_desc_en);
                            if (!clsecom.IsContainsSpecial(name_desc_en, "Description in English", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }
                            string name_desc_ar = clsecom.get_value(inData.name_desc_ar);
                            if (!clsecom.IsContainsSpecial(name_desc_ar, "Description in Arabic", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }
                            string quantity = clsecom.get_value(inData.quantity);
                            if (clsecom.IsEmpty(quantity))
                                quantity = "1";
                            if (!clsecom.IsContainsSpecial(quantity, "Quantity", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }
                            string unit_price = clsecom.get_value(inData.unit_price);
                            if (!clsecom.IsEmptyAndContainsSpecial(unit_price, "Unit Price", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }

                            string is_custom_size_available = clsecom.get_value(inData.is_custom_size_available);
                            if (clsecom.IsEmpty(is_custom_size_available))
                                is_custom_size_available = "N";
                            string custom_size_img_data = "";
                            string custom_size_img_url = "";
                            string custom_size_description = "";
                            string custom_size_field1 = "";
                            string custom_size_field2 = "";
                            string custom_size_field3 = "";
                            string custom_size_field4 = "";
                            string custom_size_field5 = "";
                            string custom_size_field6 = "";
                            string custom_size_field7 = "";
                            string custom_size_field8 = "";
                            string custom_size_field9 = "";
                            string custom_size_field10 = "";

                            //if (clsecom.get_bool_from_yn(is_custom_size_available))
                            //{
                            //    custom_size_img_data = clsecom.get_value(inData.custom_size_img_data);
                            //    if (!clsecom.IsEmpty(custom_size_img_data, "Image for Custom Size", out stat))
                            //    {
                            //        if (!clsecom.IsEmpty(stat))
                            //            result.status_msg = stat;
                            //        return result;
                            //    }

                            //    bool is_exist = false;
                            //    custom_size_img_url = clsecom.EcomSavePicAndGetFileName(custom_size_img_data, out is_exist);

                            //    custom_size_description = clsecom.get_value(inData.custom_size_description);
                            //    custom_size_field1 = clsecom.get_value(inData.custom_size_field1);
                            //    if (clsecom.IsEmpty(custom_size_field1))
                            //    {
                            //        result.status_msg = "Atleast one field is needed for Custom Size";
                            //        return result;
                            //    }
                            //    custom_size_field2 = clsecom.get_value(inData.custom_size_field2);
                            //    custom_size_field3 = clsecom.get_value(inData.custom_size_field3);
                            //    custom_size_field4 = clsecom.get_value(inData.custom_size_field4);
                            //    custom_size_field5 = clsecom.get_value(inData.custom_size_field5);
                            //    custom_size_field6 = clsecom.get_value(inData.custom_size_field6);
                            //    custom_size_field7 = clsecom.get_value(inData.custom_size_field7);
                            //    custom_size_field8 = clsecom.get_value(inData.custom_size_field8);
                            //    custom_size_field9 = clsecom.get_value(inData.custom_size_field9);
                            //    custom_size_field10 = clsecom.get_value(inData.custom_size_field10);
                            //}
                            string is_gift_available = clsecom.get_value(inData.is_gift_available);
                            string is_surprise_gift_available = clsecom.get_value(inData.is_surprise_gift_available);
                            string size_chart_img_url = clsecom.get_value(inData.size_chart_img_url);
                            string age_from = clsecom.get_value(inData.age_from);
                            string age_to = clsecom.get_value(inData.age_to);
                            string admin_approval_status = clsecom.get_value(inData.admin_approval_status);
                            if (clsecom.IsEmpty(admin_approval_status))
                                admin_approval_status = "pending_approval";
                            string admin_approval_remarks = clsecom.get_value(inData.admin_approval_remarks);
                            string order_by = clsecom.get_value(inData.order_by);

                            DataTable dtExist = conn.getDataTable(@"SELECT * FROM [dbo].[ecom_product] where [name_en] = '" + name_en + @"' and store_id='" + store_id + "'");
                            if (dtExist.Rows.Count > 0)
                            {
                                result.status_msg = "Name in English already existing";
                                return result;
                            }
                            dtExist = conn.getDataTable(@"SELECT * FROM [dbo].[ecom_product] where [name_ar] = '" + name_ar + @"' and store_id='" + store_id + "'");
                            if (dtExist.Rows.Count > 0)
                            {
                                result.status_msg = "Name in Arabic already existing";
                                return result;
                            }
                            if (auth.IsContainsSpecialChars(order_by))
                            {
                                result.status_msg = "Order By should not contain special characters";
                                return result;
                            }
                            SqlCommand comm = conn.GetCommandWithTransaction();
                            try
                            {

                                query = @"INSERT INTO [dbo].[ecom_product]
            ([brand_id]
           ,[cat_id]
           ,[name_en]
           ,[name_ar]
           ,[name_desc_en]
           ,[name_desc_ar]
           ,[quantity]
           ,[unit_price]
           ,[is_custom_size_available]
           ,[custom_size_img_url]
           ,[custom_size_description]
           ,[custom_size_field1]
           ,[custom_size_field2]
           ,[custom_size_field3]
           ,[custom_size_field4]
           ,[custom_size_field5]
           ,[custom_size_field6]
           ,[custom_size_field7]
           ,[custom_size_field8]
           ,[custom_size_field9]
           ,[custom_size_field10]
           ,[is_gift_available]
           ,[is_surprise_gift_available]
           ,[size_chart_img_url]
           ,[age_from]
           ,[age_to]
           ,[admin_approval_status]
           ,[admin_approval_remarks]
           ,[order_by]
           ,[created_by]
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + brand_id + @"'
           ,'" + cat_id + @"'
           ,'" + name_en + @"'
           ,'" + name_ar + @"'
           ,'" + name_desc_en + @"'
           ,'" + name_desc_ar + @"'
           ,'" + quantity + @"'
           ,'" + unit_price + @"'
           ,'" + is_custom_size_available + @"'
           ,'" + custom_size_img_url + @"'
           ,'" + custom_size_description + @"'
           ,'" + custom_size_field1 + @"'
           ,'" + custom_size_field2 + @"'
           ,'" + custom_size_field3 + @"'
           ,'" + custom_size_field4 + @"'
           ,'" + custom_size_field5 + @"'
           ,'" + custom_size_field6 + @"'
           ,'" + custom_size_field7 + @"'
           ,'" + custom_size_field8 + @"'
           ,'" + custom_size_field9 + @"'
           ,'" + custom_size_field10 + @"'
           ,'" + is_gift_available + @"'
           ,'" + is_surprise_gift_available + @"'
           ,'" + size_chart_img_url + @"'
           ,'" + age_from + @"'
           ,'" + age_to + @"'
           ,'" + admin_approval_status + @"'
           ,'" + admin_approval_remarks + @"'
           ,'" + order_by + @"'
           ,'" + staff_id + @"'
           ,getdate()
           ,'Y')  SELECT SCOPE_IDENTITY()";
                                string prod_id = conn.ExecuteScalar(query, comm);
                                if (clsecom.IsEmpty(prod_id))
                                {
                                    conn.RollBack(comm);
                                    return result;
                                }
                                if (inData.asset == null)
                                {
                                    conn.RollBack(comm);
                                    result.status_msg = "Please add atleast one photo of product";
                                    return result;
                                }
                                if (inData.asset.Count == 0)
                                {
                                    conn.RollBack(comm);
                                    result.status_msg = "Please add atleast one photo of product";
                                    return result;
                                }
                                for (int i = 0; i < inData.asset.Count; i++)
                                {
                                    string asset_id = clsecom.get_value(inData.asset[i].asset_id);
                                    string type = clsecom.get_value(inData.asset[i].type);
                                    string asset_data = clsecom.get_value(inData.asset[i].img_data);
                                    bool is_exist = false;
                                    string img_url = "";
                                    if (type.ToLower() == "video")
                                    {
                                        img_url = clsecom.EcomSaveVideoAndGetFileName(asset_data, out is_exist);
                                    }
                                    else if (type.ToLower() == "image")
                                    {
                                        img_url = clsecom.EcomSavePicAndGetFileName(asset_data, out is_exist);
                                    }
                                    else
                                    {
                                        conn.RollBack(comm);
                                        result.status_msg = "One of photos are not correct(Error Code: 361)";
                                        return result;
                                    }
                                    if (clsecom.IsEmpty(img_url))
                                    {
                                        conn.RollBack(comm);
                                        result.status_msg = "One or more of the assets not correct(Error Code: 366)";
                                        return result;
                                    }
                                    string is_default = clsecom.get_value(inData.asset[i].is_default);
                                    string order_by_asset = clsecom.get_value(inData.asset[i].order_by);
                                    string is_active_asset = clsecom.get_value(inData.asset[i].is_active);
                                    query = @"INSERT INTO [dbo].[ecom_product_asset]
           ([prod_id]
           ,[type]
           ,[img_url]
           ,[is_default]
           ,[order_by]
           ,[created_by]
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + prod_id + @"' 
           ,'" + type + @"' 
           ,'" + img_url + @"' 
           ,'" + is_default + @"' 
           ,'" + order_by_asset + @"' 
           ,'" + staff_id + @"' 
           ,getdate()
           ,'" + is_active_asset + @"')";
                                    if (!conn.ExecuteNonQuery(query, comm))
                                    {
                                        result.status_msg = "Failed to save asset(s)";
                                        conn.RollBack(comm);
                                        return result;
                                    }
                                }

                                if (inData.color != null)
                                {
                                    for (int i = 0; i < inData.color.Count; i++)
                                    {

                                        query = @"INSERT INTO [dbo].[ecom_product_color]
           ([prod_id]
           ,[color_name_en]
           ,[color_name_ar]
           ,[color_code] 
           ,[order_by]
           ,[created_by]
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + prod_id + @"'
           ,'" + clsecom.get_value(inData.color[i].color_name_en) + @"'
           ,N'" + clsecom.get_value(inData.color[i].color_name_ar) + @"'
           ,'" + clsecom.get_value(inData.color[i].color_code) + @"'
           ,'" + clsecom.get_value(inData.color[i].order_by) + @"'
           ,'" + staff_id + @"'
           ,getdate()
           ,'Y')";
                                        if (!conn.ExecuteNonQuery(query, comm))
                                        {
                                            result.status_msg = "Failed to save color(s)";
                                            conn.RollBack(comm);
                                            return result;
                                        }
                                    }
                                }

                                if (inData.wishcard != null)
                                {
                                    for (int i = 0; i < inData.wishcard.Count; i++)
                                    {
                                        string img_data = inData.wishcard[i].img_data;
                                        bool is_exist = false;
                                        string img_url = clsecom.EcomSavePicAndGetFileName(img_data, out is_exist);
                                        query = @"INSERT INTO [dbo].[ecom_product_gift_wish_card]
           ([prod_id]
           ,[img_url]
           ,[order_by]
           ,[created_by]
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + prod_id + @"'
           ,'" + img_url + @"'
          ,'" + clsecom.get_value(inData.wishcard[i].order_by) + @"'
            ,'" + staff_id + @"'
           ,getdate()
           ,'Y')";
                                        if (!conn.ExecuteNonQuery(query, comm))
                                        {
                                            result.status_msg = "Failed to save wish card(s)";
                                            conn.RollBack(comm);
                                            return result;
                                        }
                                    }
                                }


                                if (inData.size != null)
                                {
                                    for (int i = 0; i < inData.size.Count; i++)
                                    {
                                        query = @"INSERT INTO [dbo].[ecom_product_size]
           ([prod_id]
           ,[size_name_en]
           ,[size_name_ar]
           ,[order_by]
           ,[created_by]
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + prod_id + @"'
           ,'" + clsecom.get_value(inData.size[i].size_name_en) + @"'
         ,N'" + clsecom.get_value(inData.size[i].size_name_ar) + @"'
          ,'" + clsecom.get_value(inData.size[i].order_by) + @"'
           ,'" + prod_id + @"'
           ,getdate()
           ,'Y')";
                                        if (!conn.ExecuteNonQuery(query, comm))
                                        {
                                            result.status_msg = "Failed to save size(s)";
                                            conn.RollBack(comm);
                                            return result;
                                        }
                                    }
                                }


                                if (inData.stock != null)
                                {
                                    for (int i = 0; i < inData.stock.Count; i++)
                                    {
                                        query = @"INSERT INTO [dbo].[ecom_product_stock]
           ([prod_id]
           ,[product_size_id]
           ,[product_color_id]
           ,[stock_count]
           ,[order_by]
           ,[created_by]
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + prod_id + @"'
           ,'" + clsecom.get_value(inData.stock[i].product_size_id) + @"'
           ,'" + clsecom.get_value(inData.stock[i].product_color_id) + @"'
           ,'" + clsecom.get_value(inData.stock[i].stock_count) + @"'
           ,'" + clsecom.get_value(inData.stock[i].order_by) + @"'
           ,'" + staff_id + @"'
           ,getdate()
           ,'Y')";
                                        if (!conn.ExecuteNonQuery(query, comm))
                                        {
                                            result.status_msg = "Failed to save stock(s)";
                                            conn.RollBack(comm);
                                            return result;
                                        }
                                    }
                                }


                                if (inData.tag != null)
                                {
                                    for (int i = 0; i < inData.tag.Count; i++)
                                    {
                                        query = @"INSERT INTO [dbo].[ecom_product_tag]
           ([prod_id]
           ,[tag_id]
           ,[created_by]
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + prod_id + @"'
           ,'" + clsecom.get_value(inData.tag[i].tag_id) + @"'
           ,'" + staff_id + @"'
           ,getdate()
           ,'Y')";
                                        if (!conn.ExecuteNonQuery(query, comm))
                                        {
                                            result.status_msg = "Failed to save tag(s)";
                                            conn.RollBack(comm);
                                            return result;
                                        }
                                    }
                                }
                            }
                            catch (Exception expp)
                            {
                                conn.RollBack(comm);
                                clsecom.Log(log_key, "Exception occured. " + expp.Message + "(Input:" + clsecom.getJsonObjectToString(input) + ")", true, expp, true);
                                return result;
                            }
                            conn.Commit(comm);
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    //==
                    case "get_tag":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string tag_id = clsecom.get_value(inData.tag_id);
                            query = @"SELECT r.[tag_id]
      ,r.[store_id]
      ,r.[tag_name_en]
      ,r.[tag_name_ar] 
      ,r.[created_by]
      ,convert(char(20),r.[created_on],113) as created_on
      ,r.[updated_by]
 ,convert(char(20),r.[updated_on],113) as updated_on 
      ,r.[is_active]

  FROM [dbo].[ecom_tag] r  
 

WHERE  r.[store_id]='" + store_id + @"' and 
r.tag_id=case when '" + tag_id + "'='' then r.tag_id else '" + tag_id + "' end order by r.tag_name_en,r.tag_name_ar";
                            DataTable dtData = conn.getDataTable(query);
                            List<EcomTagItem> items = new List<EcomTagItem>();
                            for (int i = 0; i < dtData.Rows.Count; i++)
                            {
                                EcomTagItem item = new EcomTagItem();
                                item.tag_id = dtData.Rows[i]["tag_id"].ToString().Trim();
                                item.store_id = dtData.Rows[i]["store_id"].ToString().Trim();
                                item.tag_name_en = dtData.Rows[i]["tag_name_en"].ToString().Trim();
                                item.tag_name_ar = dtData.Rows[i]["tag_name_ar"].ToString().Trim();
                                item.created_by = dtData.Rows[i]["created_by"].ToString().Trim();
                                //item.created_by_name = dtData.Rows[i]["created_by_name"].ToString().Trim();
                                item.created_on = dtData.Rows[i]["created_on"].ToString().Trim();
                                item.updated_by = dtData.Rows[i]["updated_by"].ToString().Trim();
                                //item.updated_by_name = dtData.Rows[i]["updated_by_name"].ToString().Trim();
                                item.updated_on = dtData.Rows[i]["updated_on"].ToString().Trim();
                                item.is_active = dtData.Rows[i]["is_active"].ToString().Trim();
                                items.Add(item);
                            }
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(items);
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "delete_tag":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string tag_id = clsecom.get_value(inData.tag_id);
                            query = @"DELETE FROM [dbo].[ecom_tag] 
 WHERE  tag_id='" + tag_id + "'";
                            string ret_msg = "";
                            if (!conn.ExecuteNonQuery(query, "", false, out ret_msg))
                            {
                                if (clsecom.is_fk_issue(ret_msg))
                                    result.status_msg = "Cannot delete. Data is using in another form.";
                                return result;
                            }
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "update_tag":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string tag_id = clsecom.get_value(inData.tag_id);
                            string tag_name_en = clsecom.get_value(inData.tag_name_en);
                            if (auth.IsContainsSpecialChars(tag_name_en))
                            {
                                result.status_msg = "Name in English should not contain special characters";
                                return result;
                            }
                            DataTable dtExist = conn.getDataTable(@"SELECT * FROM [dbo].[ecom_tag] where [tag_name_en] = '" + tag_name_en + @"' and store_id='" + store_id + "' and tag_id!='" + tag_id + @"'");
                            if (dtExist.Rows.Count > 0)
                            {
                                result.status_msg = "Name in English already existing";
                                return result;
                            }

                            string tag_name_ar = clsecom.get_value(inData.tag_name_ar);
                            if (auth.IsContainsSpecialChars(tag_name_ar))
                            {
                                result.status_msg = "Name in Arabic should not contain special characters";
                                return result;
                            }
                            dtExist = conn.getDataTable(@"SELECT * FROM [dbo].[ecom_tag] where [tag_name_ar] = '" + tag_name_ar + @"' and store_id='" + store_id + "' and tag_id!='" + tag_id + @"'");
                            if (dtExist.Rows.Count > 0)
                            {
                                result.status_msg = "Name in Arabic already existing";
                                return result;
                            }
                            string is_active = clsecom.get_value(inData.is_active);

                            query = @"UPDATE [dbo].[ecom_tag]
   SET  [tag_name_en] ='" + tag_name_en + @"'
      ,[tag_name_ar] = N'" + tag_name_ar + @"' 
      ,[updated_by] = '" + staff_id + @"'
      ,[updated_on] =getdate()
      ,[is_active] = '" + is_active + @"'
 WHERE tag_id='" + tag_id + @"'";
                            if (!conn.ExecuteNonQuery(query))
                                return result;
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "add_tag":
                        {


                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string tag_id = clsecom.get_value(inData.tag_id);
                            string tag_name_en = clsecom.get_value(inData.tag_name_en);
                            if (auth.IsContainsSpecialChars(tag_name_en))
                            {
                                result.status_msg = "Name in English should not contain special characters";
                                return result;
                            }
                            DataTable dtExist = conn.getDataTable(@"SELECT * FROM [dbo].[ecom_tag] where [tag_name_en] = '" + tag_name_en + @"' and store_id='" + store_id + "'");
                            if (dtExist.Rows.Count > 0)
                            {
                                result.status_msg = "Name in English already existing";
                                return result;
                            }

                            string tag_name_ar = clsecom.get_value(inData.tag_name_ar);
                            if (auth.IsContainsSpecialChars(tag_name_ar))
                            {
                                result.status_msg = "Name in Arabic should not contain special characters";
                                return result;
                            }
                            dtExist = conn.getDataTable(@"SELECT * FROM [dbo].[ecom_tag] where [tag_name_ar] = '" + tag_name_ar + @"' and store_id='" + store_id + "'");
                            if (dtExist.Rows.Count > 0)
                            {
                                result.status_msg = "Name in Arabic already existing";
                                return result;
                            }
                            string is_active = clsecom.get_value(inData.is_active);

                            query = @"INSERT INTO [dbo].[ecom_tag]
           ([store_id]
           ,[tag_name_en]
           ,[tag_name_ar]  
           ,[created_by]
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + store_id + @"' 
           ,'" + tag_name_en + @"'  
           ,N'" + tag_name_ar + @"'   
           ,'" + staff_id + @"' 
           ,getdate()
           ,'" + is_active + @"')";
                            if (!conn.ExecuteNonQuery(query))
                                return result;
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    //==
                    case "get_category":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string cat_id = clsecom.get_value(inData.cat_id);
                            query = @"SELECT r.[cat_id]
      ,r.[store_id]
       ,r.[name_en]
      ,r.[name_ar]
,r.[icon_url]
,r.[order_by]
      ,r.[created_by]
      ,convert(char(20),r.[created_on],113) as created_on
      ,r.[updated_by]
 ,convert(char(20),r.[updated_on],113) as updated_on 
      ,r.[is_active]
  
  FROM [dbo].[ecom_category] r  
left join ecom_staff esc on 
esc.staff_id=r.[created_by]

left join ecom_staff esu on 
esu.staff_id=r.[updated_by]

WHERE  r.[store_id]='" + store_id + @"' and 
r.cat_id=case when '" + cat_id + "'='' then r.cat_id else '" + cat_id + "' end order by r.name_ar";
                            DataTable dtData = conn.getDataTable(query);
                            List<EcomcategoryItem> items = new List<EcomcategoryItem>();
                            for (int i = 0; i < dtData.Rows.Count; i++)
                            {
                                EcomcategoryItem item = new EcomcategoryItem();
                                item.cat_id = dtData.Rows[i]["cat_id"].ToString().Trim();
                                item.store_id = dtData.Rows[i]["store_id"].ToString().Trim();
                                item.name_en = dtData.Rows[i]["name_en"].ToString().Trim();
                                item.name_ar = dtData.Rows[i]["name_ar"].ToString().Trim();
                                item.icon_url = dtData.Rows[i]["icon_url"].ToString().Trim();
                                item.order_by = dtData.Rows[i]["order_by"].ToString().Trim();
                                item.created_by = dtData.Rows[i]["created_by"].ToString().Trim();
                               // item.created_by_name = dtData.Rows[i]["created_by_name"].ToString().Trim();
                               //// item.created_on = dtData.Rows[i]["created_on"].ToString().Trim();
                               // item.updated_by = dtData.Rows[i]["updated_by"].ToString().Trim();
                               // item.updated_by_name = dtData.Rows[i]["updated_by_name"].ToString().Trim();
                                //item.updated_on = dtData.Rows[i]["updated_on"].ToString().Trim();
                                item.is_active = dtData.Rows[i]["is_active"].ToString().Trim();
                                items.Add(item);
                            }
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(items);
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "delete_category":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string cat_id = clsecom.get_value(inData.cat_id);
                            string photo_url_to_delete = conn.ExecuteScalar("SELECT  [icon_url]  FROM [dbo].[ecom_category] where [cat_id]='" + cat_id + "'");
                            query = @"DELETE FROM [dbo].[ecom_category] 
 WHERE  cat_id='" + cat_id + "'";
                            string ret_msg = "";
                            if (!conn.ExecuteNonQuery(query, "", false, out ret_msg))
                            {
                                if (clsecom.is_fk_issue(ret_msg))
                                    result.status_msg = "Cannot delete. Data is using in another form.";
                                return result;
                            }
                            clsecom.SchDeletePic(photo_url_to_delete);
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "update_category":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string cat_id = clsecom.get_value(inData.cat_id);
                            string name_en = clsecom.get_value(inData.name_en);
                            if (auth.IsContainsSpecialChars(name_en))
                            {
                                result.status_msg = "Name in English should not contain special characters";
                                return result;
                            }
                            DataTable dtExist = conn.getDataTable(@"SELECT * FROM [dbo].[ecom_category] where [name_en] = '" + name_en + @"' and store_id='" + store_id + "' and cat_id!='" + cat_id + "'");
                            if (dtExist.Rows.Count > 0)
                            {
                                result.status_msg = "Name in English already existing";
                                return result;
                            }

                            string name_ar = clsecom.get_value(inData.name_ar);
                            if (auth.IsContainsSpecialChars(name_ar))
                            {
                                result.status_msg = "Name in Arabic should not contain special characters";
                                return result;
                            }
                            dtExist = conn.getDataTable(@"SELECT * FROM [dbo].[ecom_category] where [name_ar] = '" + name_ar + @"' and store_id='" + store_id + "' and cat_id!='" + cat_id + "'");
                            if (dtExist.Rows.Count > 0)
                            {
                                result.status_msg = "Name in Arabic already existing";
                                return result;
                            }
                            string order_by = clsecom.get_value(inData.order_by);
                            string is_active = clsecom.get_value(inData.is_active);

                            string icon_url = clsecom.get_value(inData.icon_url);
                            string icon_data = clsecom.get_value(inData.icon_data);

                            bool is_exist = false;
                            icon_url = clsecom.EcomSavePicAndGetFileName(icon_data, out is_exist);
                            string photo_url_to_delete = "";
                            if (!is_exist)
                            {
                                photo_url_to_delete = conn.ExecuteScalar("SELECT  [icon_url]  FROM [dbo].[ecom_category] where [cat_id]='" + cat_id + "'");
                            }

                            query = @"UPDATE [dbo].[ecom_category]
   SET [store_id] ='" + store_id + @"' 
      ,[name_en] = '" + name_en + @"' 
      ,[name_ar] = N'" + name_ar + @"' 
      ,[icon_url] ='" + icon_url + @"' 
      ,[order_by] ='" + order_by + @"'  
      ,[updated_by] ='" + staff_id + @"' 
      ,[updated_on] =getdate()
      ,[is_active] = '" + is_active + @"' 
 WHERE cat_id='" + cat_id + @"'";
                            if (!conn.ExecuteNonQuery(query))
                                return result;
                            clsecom.SchDeletePic(photo_url_to_delete);
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "add_category":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string cat_id = clsecom.get_value(inData.cat_id);
                            string name_en = clsecom.get_value(inData.name_en);
                            if (auth.IsContainsSpecialChars(name_en))
                            {
                                result.status_msg = "Name in English should not contain special characters";
                                return result;
                            }
                            DataTable dtExist = conn.getDataTable(@"SELECT * FROM [dbo].[ecom_category] where [name_en] = '" + name_en + @"' and store_id='" + store_id + "'");
                            if (dtExist.Rows.Count > 0)
                            {
                                result.status_msg = "Name in English already existing";
                                return result;
                            }

                            string name_ar = clsecom.get_value(inData.name_ar);
                            if (auth.IsContainsSpecialChars(name_ar))
                            {
                                result.status_msg = "Name in Arabic should not contain special characters";
                                return result;
                            }
                            dtExist = conn.getDataTable(@"SELECT * FROM [dbo].[ecom_category] where [name_ar] = '" + name_ar + @"' and store_id='" + store_id + "'");
                            if (dtExist.Rows.Count > 0)
                            {
                                result.status_msg = "Name in Arabic already existing";
                                return result;
                            }
                            string order_by = clsecom.get_value(inData.order_by);
                            string is_active = clsecom.get_value(inData.is_active);
                            string icon_url = clsecom.get_value(inData.icon_url);
                            string icon_data = clsecom.get_value(inData.icon_data);
                            bool is_exist = false;
                            icon_url = clsecom.EcomSavePicAndGetFileName(icon_data, out is_exist);

                            query = @"INSERT INTO [dbo].[ecom_category]
           ([store_id]
           ,[name_en]
           ,[name_ar]
           ,[icon_url]
           ,[order_by]
           ,[created_by]
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + store_id + @"' 
           ,'" + name_en + @"'  
           ,N'" + name_ar + @"' 
           ,'" + icon_url + @"' 
           ,'" + order_by + @"'  
           ,'" + staff_id + @"' 
           ,getdate()
           ,'" + is_active + @"')";
                            if (!conn.ExecuteNonQuery(query))
                                return result;
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "get_staff":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string staff_id_this = clsecom.get_value(inData.staff_id);
                            query = @"SELECT ,r.[staff_id]
      ,r.[brand_id]
      ,r.[store_id]
      ,r.[role_type]
      ,r.[email]
      ,r.[name_en]
      ,r.[name_ar]
      ,r.[mobile_num]
      ,r.[wa_num]
      ,r.[address_en]
      ,r.[address_ar]
      ,r.[loc_lat]
      ,r.[loc_lng]
      ,r.[created_by]
      ,convert(char(20),r.[created_on],113) as created_on
      ,r.[updated_by]
 ,convert(char(20),r.[updated_on],113) as updated_on 
      ,r.[is_active]
,esc.name_en+' '+esc.name_en as created_by_name
,esu.name_en+' '+esu.name_en as updated_by_name  
  FROM [dbo].[ecom_staff] r  
left join ecom_staff esc on 
esc.staff_id=r.[created_by]

left join ecom_staff esu on 
esu.staff_id=r.[updated_by]

WHERE  r.store_id='" + store_id + @"'
and r.staff_id=case when '" + staff_id_this + "'='' then r.staff_id else '" + staff_id_this + "' end order by r.name_en,r.name_ar,r.name_en,r.name_ar";
                            DataTable dtData = conn.getDataTable(query);
                            List<EcomStaffItem> items = new List<EcomStaffItem>();
                            for (int i = 0; i < dtData.Rows.Count; i++)
                            {
                                EcomStaffItem item = new EcomStaffItem();
                                item.staff_id = dtData.Rows[i]["staff_id"].ToString().Trim();
                                item.brand_id = dtData.Rows[i]["brand_id"].ToString().Trim();
                                item.store_id = dtData.Rows[i]["store_id"].ToString().Trim();
                                item.role_type = dtData.Rows[i]["role_type"].ToString().Trim();
                                item.email = dtData.Rows[i]["email"].ToString().Trim();
                                item.name_en = dtData.Rows[i]["name_en"].ToString().Trim();
                                item.name_ar = dtData.Rows[i]["name_ar"].ToString().Trim();
                                item.mobile_num = dtData.Rows[i]["mobile_num"].ToString().Trim();
                                item.wa_num = dtData.Rows[i]["wa_num"].ToString().Trim();
                                item.address_en = dtData.Rows[i]["address_en"].ToString().Trim();
                                item.address_ar = dtData.Rows[i]["address_ar"].ToString().Trim();
                                item.loc_lat = dtData.Rows[i]["loc_lat"].ToString().Trim();
                                item.loc_lng = dtData.Rows[i]["loc_lng"].ToString().Trim();
                                item.created_by = dtData.Rows[i]["created_by"].ToString().Trim();
                                item.created_on = dtData.Rows[i]["created_on"].ToString().Trim();
                                item.updated_by = dtData.Rows[i]["updated_by"].ToString().Trim();
                                item.updated_on = dtData.Rows[i]["updated_on"].ToString().Trim();
                                item.is_active = dtData.Rows[i]["is_active"].ToString().Trim();
                                item.created_by_name = dtData.Rows[i]["created_by_name"].ToString().Trim();
                                item.updated_by_name = dtData.Rows[i]["updated_by_name"].ToString().Trim();
                                items.Add(item);
                            }
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(items);
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "delete_staff":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string staff_id_this = clsecom.get_value(inData.staff_id);

                            string ret_msg = "";
                            SqlCommand comm = conn.GetCommandWithTransaction();
                            query = @"DELETE FROM [dbo].[ecom_staff] 
 WHERE  staff_id='" + staff_id_this + "'";
                            if (!conn.ExecuteNonQuery(query, "", comm, out ret_msg))
                            {
                                conn.RollBack(comm);
                                if (clsecom.is_fk_issue(ret_msg))
                                    result.status_msg = "Cannot delete. Data is using in another form.";
                                return result;
                            }

                            conn.Commit(comm);
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "update_staff":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string staff_id_this = clsecom.get_value(inData.staff_id);
                            string email = clsecom.get_value(inData.email);
                            DataTable dtExist = conn.getDataTable(@"SELECT * FROM [dbo].[ecom_staff] where [email] = '" + email + @"' and staff_id!='" + staff_id_this + @"'");
                            if (dtExist.Rows.Count > 0)
                            {
                                result.status_msg = "Email already existing";
                                return result;
                            }
                            role_type = clsecom.get_value(inData.role_type);
                            string brand_id = clsecom.get_value(inData.brand_id);
                            if (role_type == "admin")
                            {
                                brand_id = "0";
                            }
                            else if (clsecom.IsEmpty(brand_id))
                            {
                                result.status_msg = "No Brand available";
                                return result;
                            }
                            string name_en = clsecom.get_value(inData.name_en);
                            string stat = "";
                            if (!clsecom.IsEmptyAndContainsSpecial(name_en, "Name in English", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }

                            string name_ar = clsecom.get_value(inData.name_ar);
                            if (!clsecom.IsEmptyAndContainsSpecial(name_ar, "Name in Arabic", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }

                            string mobile_num = clsecom.get_value(inData.mobile_num);
                            if (auth.IsContainsSpecialChars(mobile_num))
                            {
                                result.status_msg = "Mobile NUmber should not contain special characters";
                                return result;
                            }

                            string wa_num = clsecom.get_value(inData.wa_num);
                            if (auth.IsContainsSpecialChars(wa_num))
                            {
                                result.status_msg = "WhatsApp Number should not contain special characters";
                                return result;
                            }

                            string address_en = clsecom.get_value(inData.address_en);
                            if (auth.IsContainsSpecialChars(address_en))
                            {
                                result.status_msg = "Address in English should not contain special characters";
                                return result;
                            }

                            string address_ar = clsecom.get_value(inData.address_ar);
                            if (auth.IsContainsSpecialChars(address_ar))
                            {
                                result.status_msg = "Address in Arabic should not contain special characters";
                                return result;
                            }

                            string is_active = clsecom.get_value(inData.is_active);
                            query = @"UPDATE [dbo].[ecom_staff]
   SET [brand_id] = '" + brand_id + @"'
      ,[store_id] = '" + store_id + @"'
      ,[role_type] = '" + role_type + @"'
      ,[email] = '" + email + @"'
      ,[name_en] = '" + name_en + @"'
      ,[name_ar] = N'" + name_ar + @"'
      ,[mobile_num] = '" + mobile_num + @"'
      ,[wa_num] = '" + wa_num + @"'
      ,[address_en] = '" + address_en + @"'
      ,[address_ar] = N'" + address_ar + @"' 
      ,[updated_by] = '" + staff_id + @"'
      ,[updated_on] = getdate()
      ,[is_active] = '" + is_active + @"'
 WHERE  staff_id='" + staff_id_this + @"'";
                            SqlCommand comm = conn.GetCommandWithTransaction();
                            if (!conn.ExecuteNonQuery(query, comm))
                            {
                                conn.RollBack(comm);
                                return result;
                            }
                            conn.Commit(comm);
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "add_staff":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string email = clsecom.get_value(inData.email);
                            DataTable dtExist = conn.getDataTable(@"SELECT * FROM [dbo].[ecom_staff] where [email] = '" + email + @"' and store_id='" + store_id + "'");
                            if (dtExist.Rows.Count > 0)
                            {
                                result.status_msg = "Email already existing";
                                return result;
                            }
                            role_type = clsecom.get_value(inData.role_type);
                            string brand_id = clsecom.get_value(inData.brand_id);
                            if (role_type == "admin")
                            {
                                brand_id = "0";
                            }
                            else if (clsecom.IsEmpty(brand_id))
                            {
                                result.status_msg = "No Brand available";
                                return result;
                            }
                            string name_en = clsecom.get_value(inData.name_en);
                            string stat = "";
                            if (!clsecom.IsEmptyAndContainsSpecial(name_en, "Name in English", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }

                            string name_ar = clsecom.get_value(inData.name_ar);
                            if (!clsecom.IsEmptyAndContainsSpecial(name_ar, "Name in Arabic", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }

                            string mobile_num = clsecom.get_value(inData.mobile_num);
                            if (auth.IsContainsSpecialChars(mobile_num))
                            {
                                result.status_msg = "Mobile Number should not contain special characters";
                                return result;
                            }

                            string wa_num = clsecom.get_value(inData.wa_num);
                            if (auth.IsContainsSpecialChars(wa_num))
                            {
                                result.status_msg = "WhatsApp Number should not contain special characters";
                                return result;
                            }

                            string address_en = clsecom.get_value(inData.address_en);
                            if (auth.IsContainsSpecialChars(address_en))
                            {
                                result.status_msg = "Address in English should not contain special characters";
                                return result;
                            }

                            string address_ar = clsecom.get_value(inData.address_ar);
                            if (auth.IsContainsSpecialChars(address_ar))
                            {
                                result.status_msg = "Address in Arabic should not contain special characters";
                                return result;
                            }

                            string is_active = clsecom.get_value(inData.is_active);
                            query = @"INSERT INTO [dbo].[ecom_staff]
           ([brand_id]
           ,[store_id]
           ,[role_type]
           ,[email]
           ,[name_en]
           ,[name_ar]
           ,[mobile_num]
           ,[wa_num]
           ,[address_en]
           ,[address_ar] 
           ,[created_by]
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + brand_id + @"'
           ,'" + store_id + @"'
           ,'" + role_type + @"'
           ,'" + email + @"'
           ,'" + name_en + @"'
           ,N'" + name_ar + @"'
           ,'" + mobile_num + @"'
           ,'" + wa_num + @"'
           ,'" + address_en + @"'
           ,N'" + address_ar + @"' 
           ,'" + staff_id + @"'
          ,getdate()
          , 'Y') SELECT SCOPE_IDENTITY()";
                            SqlCommand comm = conn.GetCommandWithTransaction();
                            string staff_id_this = conn.ExecuteScalar(query, comm);
                            if (clsecom.IsEmpty(staff_id_this))
                            {
                                conn.RollBack(comm);
                                return result;
                            }
                            conn.Commit(comm);
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    //==

                    //==
                    case "delete_ad":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string ad_id = clsecom.get_value(inData.ad_id);
                            string photo_url_to_delete = conn.ExecuteScalar("SELECT  [img_url]  FROM [dbo].[ecom_ad] where [ad_id]='" + ad_id + "'");
                            query = @"DELETE FROM [dbo].[ecom_ad] 
 WHERE  ad_id='" + ad_id + "'";
                            string ret_msg = "";
                            if (!conn.ExecuteNonQuery(query, "", false, out ret_msg))
                            {
                                if (clsecom.is_fk_issue(ret_msg))
                                    result.status_msg = "Cannot delete. Data is using in another form.";
                                return result;
                            }
                            clsecom.SchDeletePic(photo_url_to_delete);
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "update_ad":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string ad_id = clsecom.get_value(inData.ad_id);
                            string brand_id = clsecom.get_value(inData.brand_id);
                            string txn_id = clsecom.get_value(inData.txn_id);
                            if (clsecom.IsEmpty(txn_id))
                                txn_id = "0";
                            string is_animation = clsecom.get_value(inData.is_animation);
                            string date_from = clsecom.convert_client_time_to_utc(time_offset, clsecom.get_value(inData.date_from));
                            string date_to = clsecom.convert_client_time_to_utc(time_offset, clsecom.get_value(inData.date_to));
                            DateTime date_from_dt = DateTime.UtcNow;
                            DateTime date_to_dt = DateTime.UtcNow;
                            if (!DateTime.TryParse(date_from, out date_from_dt))
                            {
                                result.status_msg = "Date format of From Date is not correct";
                                return result;
                            }
                            if (!DateTime.TryParse(date_to, out date_to_dt))
                            {
                                result.status_msg = "Date format of To Date is not correct";
                                return result;
                            }
                            if (date_from_dt > date_to_dt)
                            {
                                result.status_msg = "From date should be less than To Date";
                                return result;
                            }
                            string is_paid = clsecom.get_value(inData.is_paid);
                            if (clsecom.IsEmpty(is_paid))
                                is_paid = "N";

                            string destination_type = clsecom.get_value(inData.destination_type);
                            string destination_value = clsecom.get_value(inData.destination_value);
                            string order_by = clsecom.get_value(inData.order_by);
                            string is_active = clsecom.get_value(inData.is_active);

                            string img_url = clsecom.get_value(inData.img_url);
                            string img_data = clsecom.get_value(inData.img_data);

                            string admin_approval_status = "pending_approval";
                            string admin_approval_remarks = "";

                            // string icon_url = clsecom.get_value(inData.icon_url);
                            //  string icon_data = clsecom.get_value(inData.icon_data);

                            bool is_exist = false;
                            if (clsecom.get_bool_from_yn(is_animation))
                                img_url = clsecom.EcomSavePicAndGetFileName(img_data, true, out is_exist);
                            else
                                img_url = clsecom.EcomSavePicAndGetFileName(img_data, out is_exist);
                            if (clsecom.IsEmpty(img_url))
                            {
                                result.status_msg = "Image failed to save";
                                return result;
                            }
                            string photo_url_to_delete = "";
                            if (!is_exist)
                            {
                                photo_url_to_delete = conn.ExecuteScalar("SELECT  [img_url]  FROM [dbo].[ecom_ad] where [ad_id]='" + ad_id + "'");
                            }

                            query = @"UPDATE [dbo].[ecom_ad]
   SET [brand_id] = '" + brand_id + @"'
      ,[txn_id] = '" + txn_id + @"'
      ,[is_animation] = '" + is_animation + @"'
      ,[img_url] = '" + img_url + @"'
      ,[date_from] = '" + date_from + @"'
      ,[date_to] = '" + date_to + @"'
      ,[is_paid] = '" + is_paid + @"'
      ,[destination_type] = '" + destination_type + @"'
      ,[destination_value] = '" + destination_value + @"'
      ,[admin_approval_status] = '" + admin_approval_status + @"'
      ,[admin_approval_remarks] = '" + admin_approval_remarks + @"'
      ,[order_by] = '" + order_by + @"' 
      ,[updated_by] = '" + staff_id + @"'
      ,[updated_on] = getutcdate()
      ,[is_active] = '" + is_active + @"'
 WHERE  ad_id='" + ad_id + "'";
                            if (!conn.ExecuteNonQuery(query))
                                return result;
                            clsecom.SchDeletePic(photo_url_to_delete);
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "add_ad":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string brand_id = clsecom.get_value(inData.brand_id);
                            string txn_id = clsecom.get_value(inData.txn_id);
                            if (clsecom.IsEmpty(txn_id))
                                txn_id = "3";
                            string is_animation = clsecom.get_value(inData.is_animation);
                            string date_from = clsecom.convert_client_time_to_utc(time_offset, clsecom.get_value(inData.date_from));
                            string date_to = clsecom.convert_client_time_to_utc(time_offset, clsecom.get_value(inData.date_to));
                            DateTime date_from_dt = DateTime.UtcNow;
                            DateTime date_to_dt = DateTime.UtcNow;
                            if (!DateTime.TryParse(date_from, out date_from_dt))
                            {
                                result.status_msg = "Date format of From Date is not correct";
                                return result;
                            }
                            if (!DateTime.TryParse(date_to, out date_to_dt))
                            {
                                result.status_msg = "Date format of To Date is not correct";
                                return result;
                            }
                            if (date_from_dt > date_to_dt)
                            {
                                result.status_msg = "From date should be less than To Date";
                                return result;
                            }
                            string is_paid = clsecom.get_value(inData.is_paid);
                            if (clsecom.IsEmpty(is_paid))
                                is_paid = "N";

                            string destination_type = clsecom.get_value(inData.destination_type);
                            string destination_value = clsecom.get_value(inData.destination_value);
                            string order_by = clsecom.get_value(inData.order_by);
                            string is_active = clsecom.get_value(inData.is_active);


                            string img_url = clsecom.get_value(inData.img_url);
                            string img_data = clsecom.get_value(inData.img_url);
                            bool is_exist = false;
                            if (clsecom.get_bool_from_yn(is_animation))
                                img_url = clsecom.EcomSavePicAndGetFileName(img_data, true, out is_exist);
                            else
                                img_url = clsecom.EcomSavePicAndGetFileName(img_data, out is_exist);
                            if (clsecom.IsEmpty(img_url))
                            {
                                result.status_msg = "Image failed to save";
                                return result;
                            }
                            string admin_approval_status = "pending_approval";
                            string admin_approval_remarks = "";

                            query = @"INSERT INTO [dbo].[ecom_ad]
           ([brand_id]
    
           ,[is_animation]
           ,[img_url]
           ,[date_from]
           ,[date_to]
           ,[is_paid]
           ,[destination_type]
           ,[destination_value]
           ,[admin_approval_status]
           ,[admin_approval_remarks]
           ,[order_by]
           ,[created_by]
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + brand_id + @"'
      
           ,'" + is_animation + @"' 
           ,'" + img_url + @"'   
           ,'" + date_from + @"'  
           ,'" + date_to + @"'   
           ,'" + is_paid + @"' 
           ,'" + destination_type + @"' 
           ,'" + destination_value + @"' 
           ,'" + admin_approval_status + @"' 
           ,'" + admin_approval_remarks + @"' 
           ,'" + order_by + @"' 
           ,'" + staff_id + @"' 
           ,getutcdate() 
          ,'" + is_active + @"')  SELECT SCOPE_IDENTITY()";
                            string ad_id = conn.ExecuteScalar(query);
                            if (clsecom.IsEmpty(ad_id))
                                return result;
                            string pn_title = "New Request for Ad";
                            string brand_name = conn.ExecuteScalar("SELECT  [name_brand_en]  FROM  [dbo].[ecom_brand] where brand_id='" + brand_id + "'");
                            string pn_msg = "Brand " + brand_name + " is submited an Ad for Approval of Administrator";
                            clsecom.sendPnToAllAdmins("register_new_ad", store_id, pn_title, pn_title, pn_msg, pn_msg, ad_id);

                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "update_store":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string store_id_this = clsecom.get_value(inData.store_id);
                            string name_en = clsecom.get_value(inData.name_en);
                            if (auth.IsContainsSpecialChars(name_en))
                            {
                                result.status_msg = "Name in English should not contain special characters";
                                return result;
                            }
                            string name_ar = clsecom.get_value(inData.name_ar);
                            if (auth.IsContainsSpecialChars(name_ar))
                            {
                                result.status_msg = "Name in Arabic should not contain special characters";
                                return result;
                            }

                            string name_short_en = clsecom.get_value(inData.name_short_en);
                            if (auth.IsContainsSpecialChars(name_short_en))
                            {
                                result.status_msg = "Short Name in English should not contain special characters";
                                return result;
                            }
                            string name_short_ar = clsecom.get_value(inData.name_short_ar);
                            if (auth.IsContainsSpecialChars(name_short_ar))
                            {
                                result.status_msg = "Short Name in Arabic should not contain special characters";
                                return result;
                            }

                            string address_en = clsecom.get_value(inData.address_en);
                            if (auth.IsContainsSpecialChars(address_en))
                            {
                                result.status_msg = "Address in English should not contain special characters";
                                return result;
                            }
                            string address_ar = clsecom.get_value(inData.address_ar);
                            if (auth.IsContainsSpecialChars(address_ar))
                            {
                                result.status_msg = "Address in Arabic should not contain special characters";
                                return result;
                            }
                            string phone = clsecom.get_value(inData.phone);
                            if (auth.IsContainsSpecialChars(phone))
                            {
                                result.status_msg = "Phone Number should not contain special characters";
                                return result;
                            }

                            string wa_num = clsecom.get_value(inData.wa_num);
                            if (auth.IsContainsSpecialChars(wa_num))
                            {
                                result.status_msg = "WhatsApp Number should not contain special characters";
                                return result;
                            }
                            string email_contact_us = clsecom.get_value(inData.email_contact_us);
                            if (auth.IsContainsSpecialChars(email_contact_us))
                            {
                                result.status_msg = "Contact Us Email should not contain special characters";
                                return result;
                            }

                            string emails_order_alert = clsecom.get_value(inData.emails_order_alert);
                            if (auth.IsContainsSpecialChars(emails_order_alert))
                            {
                                result.status_msg = "Order Alert Email should not contain special characters";
                                return result;
                            }

                            string emails_cust_msgs = clsecom.get_value(inData.emails_cust_msgs);
                            if (auth.IsContainsSpecialChars(emails_cust_msgs))
                            {
                                result.status_msg = "Customer Messages Email should not contain special characters";
                                return result;
                            }

                            string working_time_open = clsecom.get_value(inData.working_time_open);
                            if (auth.IsContainsSpecialChars(working_time_open))
                            {
                                result.status_msg = "Working Time Open should not contain special characters";
                                return result;
                            }

                            string working_time_close = clsecom.get_value(inData.working_time_close);
                            if (auth.IsContainsSpecialChars(working_time_close))
                            {
                                result.status_msg = "Working Time Close should not contain special characters";
                                return result;
                            }

                            string working_week_days_en = clsecom.get_value(inData.working_week_days_en);
                            if (auth.IsContainsSpecialChars(working_week_days_en))
                            {
                                result.status_msg = "Working Week Days in English should not contain special characters";
                                return result;
                            }

                            string working_week_days_ar = clsecom.get_value(inData.working_week_days_ar);
                            if (auth.IsContainsSpecialChars(working_week_days_ar))
                            {
                                result.status_msg = "Working Week Days in Arabic should not contain special characters";
                                return result;
                            }

                            string tax_default_perc = clsecom.get_value(inData.tax_default_perc);
                            if (auth.IsContainsSpecialChars(tax_default_perc))
                            {
                                result.status_msg = "Tax Default Percentage should not contain special characters";
                                return result;
                            }

                            string about_us_en = clsecom.get_value(inData.about_us_en);
                            if (auth.IsContainsSpecialChars(about_us_en))
                            {
                                result.status_msg = "About Us in English should not contain special characters";
                                return result;
                            }

                            string about_us_ar = clsecom.get_value(inData.about_us_ar);
                            if (auth.IsContainsSpecialChars(about_us_ar))
                            {
                                result.status_msg = "About Us in Arabic should not contain special characters";
                                return result;
                            }

                            string mission_en = clsecom.get_value(inData.mission_en);
                            if (auth.IsContainsSpecialChars(mission_en))
                            {
                                result.status_msg = "Mission in English should not contain special characters";
                                return result;
                            }

                            string mission_ar = clsecom.get_value(inData.mission_ar);
                            if (auth.IsContainsSpecialChars(mission_ar))
                            {
                                result.status_msg = "Mission in Arabic should not contain special characters";
                                return result;
                            }

                            string vision_en = clsecom.get_value(inData.vision_en);
                            if (auth.IsContainsSpecialChars(vision_en))
                            {
                                result.status_msg = "Vision in English should not contain special characters";
                                return result;
                            }

                            string vision_ar = clsecom.get_value(inData.vision_ar);
                            if (auth.IsContainsSpecialChars(vision_ar))
                            {
                                result.status_msg = "Vision in Arabic should not contain special characters";
                                return result;
                            }

                            string privacy_policy_en_url = clsecom.get_value(inData.privacy_policy_en_url);
                            if (auth.IsContainsSpecialChars(privacy_policy_en_url))
                            {
                                result.status_msg = "Privacy Policy in English URL should not contain special characters";
                                return result;
                            }

                            string privacy_policy_ar_url = clsecom.get_value(inData.privacy_policy_ar_url);
                            if (auth.IsContainsSpecialChars(privacy_policy_ar_url))
                            {
                                result.status_msg = "Privacy Policy in Arabic URL should not contain special characters";
                                return result;
                            }

                            string refund_policy_en_url = clsecom.get_value(inData.refund_policy_en_url);
                            if (auth.IsContainsSpecialChars(refund_policy_en_url))
                            {
                                result.status_msg = "Refund Policy in English should not contain special characters";
                                return result;
                            }

                            string refund_policy_ar_url = clsecom.get_value(inData.refund_policy_ar_url);
                            if (auth.IsContainsSpecialChars(refund_policy_ar_url))
                            {
                                result.status_msg = "Refund Policy in Arabic should not contain special characters";
                                return result;
                            }

                            string terms_and_conditions_en_url = clsecom.get_value(inData.terms_and_conditions_en_url);
                            if (auth.IsContainsSpecialChars(terms_and_conditions_en_url))
                            {
                                result.status_msg = "Terms and Conditions in English should not contain special characters";
                                return result;
                            }

                            string terms_and_conditions_ar_url = clsecom.get_value(inData.terms_and_conditions_ar_url);
                            if (auth.IsContainsSpecialChars(terms_and_conditions_ar_url))
                            {
                                result.status_msg = "Terms and Conditions in Arabic should not contain special characters";
                                return result;
                            }

                            string loc_lat = clsecom.get_value(inData.loc_lat);
                            string loc_lng = clsecom.get_value(inData.loc_lng);
                            string currency_code = clsecom.get_value(inData.currency_code);
                            if (auth.IsContainsSpecialChars(currency_code))
                            {
                                result.status_msg = "Currency Code should not contain special characters";
                                return result;
                            }

                            string web_url = clsecom.get_value(inData.web_url);
                            if (auth.IsContainsSpecialChars(web_url))
                            {
                                result.status_msg = "Web URL should not contain special characters";
                                return result;
                            }

                            string fb_url = clsecom.get_value(inData.fb_url);
                            if (auth.IsContainsSpecialChars(fb_url))
                            {
                                result.status_msg = "Facebook URL should not contain special characters";
                                return result;
                            }

                            string instagram_url = clsecom.get_value(inData.instagram_url);
                            if (auth.IsContainsSpecialChars(instagram_url))
                            {
                                result.status_msg = "Instagram URL should not contain special characters";
                                return result;
                            }

                            string twitter_url = clsecom.get_value(inData.twitter_url);
                            if (auth.IsContainsSpecialChars(twitter_url))
                            {
                                result.status_msg = "Twitter URL should not contain special characters";
                                return result;
                            }

                            string youtube_url = clsecom.get_value(inData.youtube_url);
                            if (auth.IsContainsSpecialChars(youtube_url))
                            {
                                result.status_msg = "YouTube URL should not contain special characters";
                                return result;
                            }

                            string linkedin_url = clsecom.get_value(inData.linkedin_url);
                            if (auth.IsContainsSpecialChars(linkedin_url))
                            {
                                result.status_msg = "LinkedIn URL should not contain special characters";
                                return result;
                            }

                            string estimated_hrs_to_deliver = clsecom.get_value(inData.estimated_hrs_to_deliver);
                            if (auth.IsContainsSpecialChars(estimated_hrs_to_deliver))
                            {
                                result.status_msg = "Estimated Hours to Deliver should not contain special characters";
                                return result;
                            }

                            string max_delivery_dist_km = clsecom.get_value(inData.max_delivery_dist_km);
                            if (auth.IsContainsSpecialChars(max_delivery_dist_km))
                            {
                                result.status_msg = "Max Delivery Distance should not contain special characters";
                                return result;
                            }

                            string need_all_logs = clsecom.get_value(inData.need_all_logs);
                            string imp_msg_to_customer = clsecom.get_value(inData.imp_msg_to_customer);
                            if (auth.IsContainsSpecialChars(imp_msg_to_customer))
                            {
                                result.status_msg = "Imp Message to Customer should not contain special characters";
                                return result;
                            }

                            string holiday_from = clsecom.get_value(inData.holiday_from);
                            if (auth.IsContainsSpecialChars(holiday_from))
                            {
                                result.status_msg = "Holiday From should not contain special characters";
                                return result;
                            }

                            string holiday_to = clsecom.get_value(inData.holiday_to);
                            if (auth.IsContainsSpecialChars(holiday_to))
                            {
                                result.status_msg = "Holiday To should not contain special characters";
                                return result;
                            }

                            string show_rate_on_product = clsecom.get_value(inData.show_rate_on_product);

                            string icon_url = clsecom.get_value(inData.icon_url);
                            string icon_data = clsecom.get_value(inData.icon_data);
                            bool is_exist = false;
                            icon_url = clsecom.EcomSavePicAndGetFileName(icon_data, out is_exist);
                            string photo_url_to_delete = "";
                            if (!is_exist)
                            {
                                photo_url_to_delete = conn.ExecuteScalar("SELECT  [icon_url]  FROM [dbo].[ecom_store] where [store_id]='" + store_id + "'");

                            }
                            query = @"UPDATE [dbo].[ecom_store]
   SET [name_en] ='" + name_en + @"'
      ,[name_ar] = '" + name_ar + @"'
      ,[name_short_en] = '" + name_short_en + @"'
      ,[name_short_ar] = N'" + name_short_ar + @"'
      ,[address_en] ='" + address_en + @"'
      ,[address_ar] = N'" + address_ar + @"'
      ,[phone] = '" + phone + @"'
      ,[wa_num] = '" + wa_num + @"'
      ,[email_contact_us] = '" + email_contact_us + @"'
      ,[emails_order_alert] = '" + emails_order_alert + @"'
      ,[emails_cust_msgs] = '" + emails_cust_msgs + @"'
      ,[working_time_open] ='" + working_time_open + @"'
      ,[working_time_close] = '" + working_time_close + @"'
      ,[working_week_days_en] = '" + working_week_days_en + @"'
      ,[working_week_days_ar] = N'" + working_week_days_ar + @"'
      ,[tax_default_perc] = '" + tax_default_perc + @"'
      ,[about_us_en] = '" + about_us_en + @"'
      ,[about_us_ar] = N'" + about_us_ar + @"'
      ,[mission_en] = '" + mission_en + @"'
      ,[mission_ar] = N'" + mission_ar + @"'
      ,[vision_en] = '" + vision_en + @"'
      ,[vision_ar] = N'" + vision_ar + @"'  
      ,[privacy_policy_en_url] = '" + privacy_policy_en_url + @"'
      ,[privacy_policy_ar_url] = '" + privacy_policy_ar_url + @"'
      ,[refund_policy_en_url] = '" + refund_policy_en_url + @"'
      ,[refund_policy_ar_url] = '" + refund_policy_ar_url + @"'
      ,[terms_and_conditions_en_url] ='" + terms_and_conditions_en_url + @"'
      ,[terms_and_conditions_ar_url] = '" + terms_and_conditions_ar_url + @"'
      ,[loc_lat] ='" + loc_lat + @"'
      ,[loc_lng] ='" + loc_lng + @"'
      ,[currency_code] ='" + currency_code + @"'
      ,[icon_url] = '" + icon_url + @"'
      ,[web_url] = '" + web_url + @"'
      ,[fb_url] = '" + fb_url + @"'
      ,[instagram_url] = '" + instagram_url + @"'
      ,[twitter_url] ='" + twitter_url + @"'
      ,[youtube_url] = '" + youtube_url + @"'
      ,[linkedin_url] = '" + linkedin_url + @"'
      ,[estimated_hrs_to_deliver] ='" + estimated_hrs_to_deliver + @"'
      ,[max_delivery_dist_km] = '" + max_delivery_dist_km + @"'
      ,[app_identity] = '" + app_identity + @"'
      ,[need_all_logs] = '" + need_all_logs + @"'
      ,[imp_msg_to_customer] ='" + imp_msg_to_customer + @"'
      ,[holiday_from] = '" + holiday_from + @"'
      ,[holiday_to] ='" + holiday_to + @"'
      ,[show_rate_on_product] ='" + show_rate_on_product + @"' 
 WHERE  store_id='" + store_id + @"'";
                            if (!conn.ExecuteNonQuery(query))
                            {
                                return result;
                            }
                            clsecom.SchDeletePic(photo_url_to_delete);
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "update_order":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string current_status_key = clsecom.get_value(inData.current_status_key);
                            string is_paid = clsecom.get_value(inData.is_paid);
                            string order_id = clsecom.get_value(inData.order_id);
                            query = @"UPDATE [dbo].[ecom_order]
   SET  [staff_id_updated_by] = '" + staff_id + @"' 
      ,[current_status_key] = '" + current_status_key + @"' 
      ,[is_paid] = '" + is_paid + @"'  
      ,[updated_on] = getdate() 
 WHERE order_id='" + order_id + @"'";
                            if (!conn.ExecuteNonQuery(query))
                            {
                                return result;
                            }
                            string cust_id_order = conn.ExecuteScalar("SELECT [cust_id] FROM  [dbo].[ecom_order] where [order_id]='" + order_id + "'");
                            string title_en = "New Order Status:" + clsecom.getStatusFromKey(current_status_key);
                            string title_ar = "حالة الطلب الجديد: " + clsecom.getStatusFromKey(current_status_key);
                            string msg_en = "Status of your order(Id: " + order_id + ") is changed to " + clsecom.getStatusFromKey(current_status_key);
                            string msg_ar = "تم تغيير حالة طلبك (المعرف: " + order_id + ") إلى " + clsecom.getStatusFromKey(current_status_key);
                            clsecom.sendPnToCustomer(store_id, cust_id_order, title_en, title_ar, msg_en, msg_ar, "order_status_change", order_id);
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "add_order":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string txn_id = clsecom.get_value(inData.txn_id);
                            string tot_price = clsecom.get_value(inData.tot_price);
                            string order_type = clsecom.get_value(inData.order_type);
                            string current_status_key = "processing";
                            string is_paid = clsecom.get_value(inData.is_paid);
                            string delivery_name = clsecom.get_value(inData.delivery_name);
                            if (auth.IsContainsSpecialChars(delivery_name))
                            {
                                result.status = "Name should not contain invalid characters.";
                                return result;
                            }

                            string delivery_address = clsecom.get_value(inData.delivery_address);
                            if (auth.IsContainsSpecialChars(delivery_address))
                            {
                                result.status = "Delivery address should not contain invalid characters.";
                                return result;
                            }
                            string delivery_state_region = clsecom.get_value(inData.delivery_state_region);
                            if (auth.IsContainsSpecialChars(delivery_state_region))
                            {
                                result.status = "State/Region should not contain invalid characters.";
                                return result;
                            }
                            string delivery_city = clsecom.get_value(inData.delivery_city);
                            if (auth.IsContainsSpecialChars(delivery_city))
                            {
                                result.status = "Delivery city should not contain invalid characters.";
                                return result;
                            }
                            string delivery_zip = clsecom.get_value(inData.delivery_zip);
                            if (auth.IsContainsSpecialChars(delivery_zip))
                            {
                                result.status = "Delivery zip should not contain invalid characters.";
                                return result;
                            }
                            string delivery_country = clsecom.get_value(inData.delivery_country);
                            if (auth.IsContainsSpecialChars(delivery_country))
                            {
                                result.status = "Delivery country should not contain invalid characters.";
                                return result;
                            }
                            string delivery_landmark = clsecom.get_value(inData.delivery_landmark);
                            if (auth.IsContainsSpecialChars(delivery_landmark))
                            {
                                result.status = "Landmark should not contain invalid characters.";
                                return result;
                            }
                            string delivery_mobile_num = clsecom.get_value(inData.delivery_mobile_num);
                            if (auth.IsContainsSpecialChars(delivery_mobile_num))
                            {
                                result.status = "Mobile number should not contain invalid characters.";
                                return result;
                            }
                            string delivery_wa_num = clsecom.get_value(inData.delivery_wa_num);
                            if (auth.IsContainsSpecialChars(delivery_wa_num))
                            {
                                result.status = "WhatsApp number should not contain invalid characters.";
                                return result;
                            }
                            string delivery_email = clsecom.get_value(inData.delivery_email);
                            if (auth.IsContainsSpecialChars(delivery_email))
                            {
                                result.status = "Email should not contain invalid characters.";
                                return result;
                            }
                            string delivery_loc_lat = clsecom.get_value(inData.delivery_loc_lat);
                            string delivery_loc_lng = clsecom.get_value(inData.delivery_loc_lng);
                            string delivery_instruction = clsecom.get_value(inData.delivery_instruction);
                            if (auth.IsContainsSpecialChars(delivery_instruction))
                            {
                                result.status = "Delivery instruction should not contain invalid characters.";
                                return result;
                            }
                            string delivery_time_preference = clsecom.get_value(inData.delivery_time_preference);
                            if (auth.IsContainsSpecialChars(delivery_time_preference))
                            {
                                result.status = "Delivery time preference should not contain invalid characters.";
                                return result;
                            }
                            string billing_name = clsecom.get_value(inData.billing_name);
                            if (auth.IsContainsSpecialChars(billing_name))
                            {
                                result.status = "Billing name should not contain invalid characters.";
                                return result;
                            }
                            string billing_address = clsecom.get_value(inData.billing_address);
                            if (auth.IsContainsSpecialChars(billing_address))
                            {
                                result.status = "Billing address should not contain invalid characters.";
                                return result;
                            }
                            string billing_state_region = clsecom.get_value(inData.billing_state_region);
                            if (auth.IsContainsSpecialChars(billing_state_region))
                            {
                                result.status = "Billing state/region should not contain invalid characters.";
                                return result;
                            }
                            string billing_city = clsecom.get_value(inData.billing_city);
                            if (auth.IsContainsSpecialChars(billing_city))
                            {
                                result.status = "Billing city should not contain invalid characters.";
                                return result;
                            }
                            string billing_zip = clsecom.get_value(inData.billing_zip);
                            if (auth.IsContainsSpecialChars(billing_zip))
                            {
                                result.status = "Billing zip should not contain invalid characters.";
                                return result;
                            }
                            string billing_country = clsecom.get_value(inData.billing_country);
                            if (auth.IsContainsSpecialChars(billing_country))
                            {
                                result.status = "Billing country should not contain invalid characters.";
                                return result;
                            }
                            SqlCommand comm = conn.GetCommandWithTransaction();
                            query = @"INSERT INTO [dbo].[ecom_order]
           ([cust_id]
           ,[txn_id] 
,[tot_price]
           ,[order_date]
           ,[order_type]
,[is_req_for_cancel_by_customer]
,[current_status_key] 
           ,[is_paid] 
           ,[delivery_name] 
           ,[delivery_address]
           ,[delivery_state_region]
           ,[delivery_city]
           ,[delivery_zip]
           ,[delivery_country]
           ,[delivery_landmark]
           ,[delivery_mobile_num]
           ,[delivery_wa_num]
           ,[delivery_email]
           ,[delivery_loc_lat]
           ,[delivery_loc_lng]
           ,[delivery_instruction]
           ,[delivery_time_preference]
           ,[billing_name] 
           ,[billing_address]
           ,[billing_state_region]
           ,[billing_city]
           ,[billing_zip]
           ,[billing_country]
           ,[created_by]
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + cust_id + @"' 
           ," + clsecom.get_value_with_quote_for_sql(txn_id) + @" 
,'" + tot_price + @"' 
           ,getdate() 
          ,'" + order_type + @"' 
,'N' 
,'" + current_status_key + @"'  
           ,'" + is_paid + @"'  
           ,'" + delivery_name + @"'  
    ,'" + delivery_address + @"' 
          ,'" + delivery_state_region + @"' 
            ,'" + delivery_city + @"' 
           ,'" + delivery_zip + @"' 
            ,'" + delivery_country + @"' 
              ,'" + delivery_landmark + @"' 
             ,'" + delivery_mobile_num + @"' 
           ,'" + delivery_wa_num + @"' 
           ,'" + delivery_email + @"' 
             ,'" + delivery_loc_lat + @"' 
           ,'" + delivery_loc_lng + @"' 
           ,'" + delivery_instruction + @"' 
          ,'" + delivery_time_preference + @"' 
          ,'" + billing_name + @"'  
             ,'" + billing_address + @"' 
          ,'" + billing_state_region + @"' 
            ,'" + billing_city + @"' 
           ,'" + billing_zip + @"' 
            ,'" + billing_country + @"' 
           ,'" + cust_id + @"' 
           ,getdate() 
           ,'Y') SELECT SCOPE_IDENTITY()";
                            string order_id = conn.ExecuteScalar(query, comm);
                            if (clsecom.IsEmpty(order_id))
                            {
                                conn.RollBack(comm);
                                return result;
                            }
                            var prods = inData.items;
                            if (prods == null)
                            {
                                result.status = "No products avalable";
                                conn.RollBack(comm);
                                return result;
                            }
                            List<string> brand_ids = new List<string>();
                            for (int i = 0; i < prods.Count; i++)
                            {
                                string brand_id = clsecom.get_value(prods[i].brand_id);
                                if (!brand_ids.Contains(brand_id))
                                    brand_ids.Add(brand_id);

                                string cart_id = clsecom.get_value(prods[i].cart_id);
                                string prod_id = clsecom.get_value(prods[i].prod_id);
                                string unit_price = clsecom.get_value(prods[i].unit_price);
                                string discount_perc = clsecom.get_value(prods[i].discount_perc);
                                string quantity = clsecom.get_value(prods[i].quantity);
                                int qua = 0;
                                int.TryParse(quantity, out qua);
                                if (qua == 0)
                                    continue;
                                string product_color_id = clsecom.get_value(prods[i].product_color_id);
                                string product_size_id = clsecom.get_value(prods[i].product_size_id);
                                string is_custom_size = clsecom.get_value(prods[i].is_custom_size);
                                string custom_size_field1_value = clsecom.get_value(prods[i].custom_size_field1_value);
                                string custom_size_field2_value = clsecom.get_value(prods[i].custom_size_field2_value);
                                string custom_size_field3_value = clsecom.get_value(prods[i].custom_size_field3_value);
                                string custom_size_field4_value = clsecom.get_value(prods[i].custom_size_field4_value);
                                string custom_size_field5_value = clsecom.get_value(prods[i].custom_size_field5_value);
                                string custom_size_field6_value = clsecom.get_value(prods[i].custom_size_field6_value);
                                string custom_size_field7_value = clsecom.get_value(prods[i].custom_size_field7_value);
                                string custom_size_field8_value = clsecom.get_value(prods[i].custom_size_field8_value);
                                string custom_size_field9_value = clsecom.get_value(prods[i].custom_size_field9_value);
                                string custom_size_field10_value = clsecom.get_value(prods[i].custom_size_field10_value);
                                string custom_size_color_code = clsecom.get_value(prods[i].custom_size_color_code);
                                string custom_size_message = clsecom.get_value(prods[i].custom_size_message);
                                string is_gift = clsecom.get_value(prods[i].is_gift);
                                string is_surprise_gift = clsecom.get_value(prods[i].is_surprise_gift);
                                string is_need_gift_wish_card = clsecom.get_value(prods[i].is_need_gift_wish_card);
                                string gift_wish_card_url = clsecom.get_value(prods[i].gift_wish_card_url);

                                string custome_size_extra_price = clsecom.get_value(prods[i].custome_size_extra_price);
                                string custome_size_extra_price_perc = clsecom.get_value(prods[i].custome_size_extra_price_perc);
                                string gift_extra_price = clsecom.get_value(prods[i].gift_extra_price);
                                string gift_extra_price_perc = clsecom.get_value(prods[i].gift_extra_price_perc);
                                string surprise_gift_extra_price = clsecom.get_value(prods[i].surprise_gift_extra_price);
                                string surprise_gift_extra_price_perc = clsecom.get_value(prods[i].surprise_gift_extra_price_perc);
                                string wish_card_extra_price = clsecom.get_value(prods[i].wish_card_extra_price);
                                string wish_card_extra_price_perc = clsecom.get_value(prods[i].wish_card_extra_price_perc);

                                query = @"INSERT INTO [dbo].[ecom_order_product]
           ([order_id]
           ,[prod_id]
           ,[unit_price]
           ,[discount_perc]
           ,[custome_size_extra_price]
           ,[custome_size_extra_price_perc]
           ,[gift_extra_price]
           ,[gift_extra_price_perc]
           ,[surprise_gift_extra_price]
           ,[surprise_gift_extra_price_perc]
           ,[wish_card_extra_price]
           ,[wish_card_extra_price_perc]
           ,[quantity]
           ,[product_color_id]
           ,[product_size_id]
           ,[is_custom_size]
           ,[custom_size_field1_value]
           ,[custom_size_field2_value]
           ,[custom_size_field3_value]
           ,[custom_size_field4_value]
           ,[custom_size_field5_value]
           ,[custom_size_field6_value]
           ,[custom_size_field7_value]
           ,[custom_size_field8_value]
           ,[custom_size_field9_value]
           ,[custom_size_field10_value]
           ,[custom_size_color_code]
           ,[custom_size_message]
           ,[is_gift]
           ,[is_surprise_gift]
           ,[is_need_gift_wish_card]
           ,[gift_wish_card_url]
           ,[created_by]
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + order_id + @"'
           ,'" + prod_id + @"'
           ,'" + unit_price + @"'
           ,'" + discount_perc + @"'
           ,'" + custome_size_extra_price + @"'
           ,'" + custome_size_extra_price_perc + @"'
           ,'" + gift_extra_price + @"'
           ,'" + gift_extra_price_perc + @"'
           ,'" + surprise_gift_extra_price + @"'
           ,'" + surprise_gift_extra_price_perc + @"'
           ,'" + wish_card_extra_price + @"'
           ,'" + wish_card_extra_price_perc + @"'
           ,'" + quantity + @"'
           ,'" + product_color_id + @"'
           ,'" + product_size_id + @"'
           ,'" + is_custom_size + @"'
           ,'" + custom_size_field1_value + @"'
           ,'" + custom_size_field2_value + @"'
           ,'" + custom_size_field3_value + @"'
           ,'" + custom_size_field4_value + @"'
           ,'" + custom_size_field5_value + @"'
           ,'" + custom_size_field6_value + @"'
           ,'" + custom_size_field7_value + @"'
           ,'" + custom_size_field8_value + @"'
           ,'" + custom_size_field9_value + @"'
           ,'" + custom_size_field10_value + @"'
           ,'" + custom_size_color_code + @"'
           ,'" + custom_size_message + @"'
           ,'" + is_gift + @"'
           ,'" + is_surprise_gift + @"'
           ,'" + is_need_gift_wish_card + @"'
           ,'" + gift_wish_card_url + @"'
           ,'" + cust_id + @"'
           ,getdate()
           ,'Y')";
                                if (conn.ExecuteNonQuery(query, comm))
                                {
                                    query = @"DELETE FROM [dbo].[ecom_cart]
      WHERE cart_id='" + cart_id + @"'";
                                    if (!conn.ExecuteNonQuery(query, comm))
                                    {
                                        conn.RollBack(comm);
                                        return result;
                                    }
                                }
                                else
                                {
                                    conn.RollBack(comm);
                                    return result;
                                }
                            }
                            conn.Commit(comm);
                            for (int i = 0; i < brand_ids.Count; i++)
                            {
                                string title_en = "";
                                string title_ar = "";
                                string msg_en = "";
                                string msg_ar = "";
                                int prod_count = 0;
                                int quantity = 0;
                                for (int j = 0; j < prods.Count; j++)
                                {
                                    string brand_id = clsecom.get_value(prods[j].brand_id);
                                    if (brand_id != brand_ids[i])
                                        continue;
                                    prod_count++;
                                    string q1 = clsecom.get_value(prods[j].quantity);
                                    int qua = 0;
                                    int.TryParse(q1, out qua);
                                    if (qua == 0)
                                        quantity += qua;
                                }
                                if (quantity > 0 && prod_count > 0)
                                {
                                    title_en = "You have " + prod_count + " New Orders.";
                                    title_ar = "لديك " + prod_count + " أوامر جديدة.";
                                    msg_en = "You have " + prod_count + " new orders. Quantity: " + quantity + ". Order Id: " + order_id;
                                    msg_ar = "لديك" + prod_count + "طلبات جديدة. الكمية:" + quantity + ". معرف الطلب:" + order_id;
                                    clsecom.sendPnToBrand(store_id, brand_ids[i], title_en, title_ar, msg_en, msg_ar, "new_order", order_id);
                                }
                            }
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";

                            break;
                        }
                    case "get_orders":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string last_idx = clsecom.get_value(inData.last_idx);
                            string max_in_a_call = clsecom.get_value(inData.max_in_a_call);
                            string condition_type = clsecom.get_value(inData.condition_type);
                            string paid_type = clsecom.get_value(inData.paid_type);
                            string current_status_key = clsecom.get_value(inData.current_status_key);
                            string prod_id = clsecom.get_value(inData.prod_id);
                            string order_id = clsecom.get_value(inData.order_id);
                            string cust_id_this = clsecom.get_value(inData.cust_id);
                            string ret_stat = "";
                            List<EcomOrderItem> orders = clsecom.getOrders(store_id, last_idx, max_in_a_call, condition_type, paid_type, current_status_key, prod_id, cust_id_this, order_id, out ret_stat);
                            if (!clsecom.IsEmpty(ret_stat))
                            {
                                result.status_msg = ret_stat;
                            }
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(orders);
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "get_my_orders":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string ret_stat = "";
                            List<EcomOrderItem> orders = clsecom.getOrders(store_id, "0", "1000", "", "", "", "", cust_id, "", out ret_stat);
                            if (!clsecom.IsEmpty(ret_stat))
                            {
                                result.status_msg = ret_stat;
                            }
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(orders);
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "get_cust_address":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string address_id = clsecom.get_value(inData.address_id);
                            query = @"SELECT  [address_id]
      ,[cust_id]
      ,[address_type]
      ,[name] 
      ,[address]
      ,[state_region]
      ,[city]
      ,[zip]
      ,[country]
      ,[landmark]
      ,[mobile_num]
      ,[wa_num]
      ,[email]
      ,[is_email_verified]
      ,[is_mobile_verified]
      ,[loc_lat]
      ,[loc_lng]
      ,[instruction]
      ,[time_preference]
      ,[is_default]
      ,[created_by]
      ,[created_on]
      ,[updated_by]
      ,[updated_on]
      ,[is_active]
  FROM  [dbo].[ecom_address] where isnull([is_active],'')='Y' and [cust_id]='" + cust_id + @"' and 
[address_id]=case when '" + address_id + @"'='' then [address_id] else '" + address_id + @"' end";
                            DataTable dtData = conn.getDataTable(query);
                            List<EcomAddressItem> addresses = new List<EcomAddressItem>();
                            for (int i = 0; i < dtData.Rows.Count; i++)
                            {
                                EcomAddressItem add = new EcomAddressItem();
                                add.address_id = dtData.Rows[i]["address_id"].ToString().Trim();
                                add.cust_id = dtData.Rows[i]["cust_id"].ToString().Trim();
                                add.address_type = dtData.Rows[i]["address_type"].ToString().Trim();
                                add.name = dtData.Rows[i]["name"].ToString().Trim();
                                add.address = dtData.Rows[i]["address"].ToString().Trim();
                                add.state_region = dtData.Rows[i]["state_region"].ToString().Trim();
                                add.city = dtData.Rows[i]["city"].ToString().Trim();
                                add.zip = dtData.Rows[i]["zip"].ToString().Trim();
                                add.country = dtData.Rows[i]["country"].ToString().Trim();
                                add.landmark = dtData.Rows[i]["landmark"].ToString().Trim();
                                add.mobile_num = dtData.Rows[i]["mobile_num"].ToString().Trim();
                                add.wa_num = dtData.Rows[i]["wa_num"].ToString().Trim();
                                add.email = dtData.Rows[i]["email"].ToString().Trim();
                                add.is_email_verified = dtData.Rows[i]["is_email_verified"].ToString().Trim();
                                add.is_mobile_verified = dtData.Rows[i]["is_mobile_verified"].ToString().Trim();
                                add.loc_lat = dtData.Rows[i]["loc_lat"].ToString().Trim();
                                add.loc_lng = dtData.Rows[i]["loc_lng"].ToString().Trim();
                                add.instruction = dtData.Rows[i]["instruction"].ToString().Trim();
                                add.time_preference = dtData.Rows[i]["time_preference"].ToString().Trim();
                                add.is_default = dtData.Rows[i]["is_default"].ToString().Trim();
                                addresses.Add(add);
                            }
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(addresses);
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "set_def_cust_address":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string address_id = clsecom.get_value(inData.address_id);
                            query = @"UPDATE [dbo].[ecom_address]
   SET  [is_default] ='Y' 
      ,[updated_by] ='" + cust_id + @"'
      ,[updated_on] =getdate() 
 WHERE  address_id='" + address_id + @"' ";
                            if (!conn.ExecuteNonQuery(query))
                            {
                                return result;
                            }
                            query = @"UPDATE [dbo].[ecom_address]
   SET  [is_default] ='N' 
      ,[updated_by] ='" + cust_id + @"'
      ,[updated_on] =getdate() 
 WHERE [cust_id] = '" + cust_id + @"' and address_id!='" + address_id + @"' ";
                            if (!conn.ExecuteNonQuery(query))
                            {
                                return result;
                            }
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "delete_cust_address":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string address_id = clsecom.get_value(inData.address_id);
                            query = @"DELETE FROM [dbo].[ecom_address]
      WHERE address_id='" + address_id + @"'";
                            if (!conn.ExecuteNonQuery(query))
                            {
                                return result;
                            }
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "update_cust_address":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string address_id = clsecom.get_value(inData.address_id);
                            string address_type = clsecom.get_value(inData.address_type);
                            string name = clsecom.get_value(inData.name);
                            if (auth.IsContainsSpecialChars(name))
                            {
                                result.status = "Name should not contain special characters";
                                return result;
                            }

                            string address = clsecom.get_value(inData.address);
                            if (auth.IsContainsSpecialChars(address))
                            {
                                result.status = "Address should not contain special characters";
                                return result;
                            }
                            string state_region = clsecom.get_value(inData.state_region);
                            if (auth.IsContainsSpecialChars(state_region))
                            {
                                result.status = "State/Region should not contain special characters";
                                return result;
                            }
                            string city = clsecom.get_value(inData.city);
                            if (auth.IsContainsSpecialChars(city))
                            {
                                result.status = "City should not contain special characters";
                                return result;
                            }
                            string zip = clsecom.get_value(inData.zip);
                            if (auth.IsContainsSpecialChars(zip))
                            {
                                result.status = "Zip should not contain special characters";
                                return result;
                            }
                            string country = clsecom.get_value(inData.country);
                            if (auth.IsContainsSpecialChars(country))
                            {
                                result.status = "Country should not contain special characters";
                                return result;
                            }
                            string landmark = clsecom.get_value(inData.landmark);
                            if (auth.IsContainsSpecialChars(landmark))
                            {
                                result.status = "Landmark should not contain special characters";
                                return result;
                            }
                            string mobile_num = clsecom.get_value(inData.mobile_num);
                            if (auth.IsContainsSpecialChars(mobile_num))
                            {
                                result.status = "Mobile Number should not contain special characters";
                                return result;
                            }
                            string wa_num = clsecom.get_value(inData.wa_num);
                            if (auth.IsContainsSpecialChars(wa_num))
                            {
                                result.status = "WhatsApp Number should not contain special characters";
                                return result;
                            }
                            string email = clsecom.get_value(inData.email);
                            if (auth.IsContainsSpecialChars(email))
                            {
                                result.status = "Email should not contain special characters";
                                return result;
                            }
                            string is_email_verified = clsecom.get_value(inData.is_email_verified);
                            string is_mobile_verified = clsecom.get_value(inData.is_mobile_verified);
                            string loc_lat = clsecom.get_value(inData.loc_lat);
                            string loc_lng = clsecom.get_value(inData.loc_lng);
                            string instruction = clsecom.get_value(inData.instruction);
                            if (auth.IsContainsSpecialChars(instruction))
                            {
                                result.status = "Instruction should not contain special characters";
                                return result;
                            }
                            string time_preference = clsecom.get_value(inData.time_preference);
                            if (auth.IsContainsSpecialChars(time_preference))
                            {
                                result.status = "Time preference should not contain special characters";
                                return result;
                            }
                            string is_default = clsecom.get_value(inData.is_default);
                            if (is_default.ToString().Trim().ToLower() == "y")
                            {
                                query = @"UPDATE [dbo].[ecom_address] SET [is_default]='N' 
where [cust_id]='" + cust_id + @"'";
                                conn.ExecuteNonQuery(query);
                            }
                            query = @"UPDATE [dbo].[ecom_address] 
   SET [cust_id] = '" + cust_id + @"' 
      ,[address_type] = '" + address_type + @"'
      ,[name] ='" + name + @"' 
      ,[address] ='" + address + @"'
      ,[state_region] = '" + state_region + @"'
      ,[city] ='" + city + @"'
      ,[zip] = '" + zip + @"'
      ,[country] = '" + country + @"'
      ,[landmark] ='" + landmark + @"'
      ,[mobile_num] = '" + mobile_num + @"'
      ,[wa_num] ='" + wa_num + @"'
      ,[email] ='" + email + @"'
      ,[is_email_verified] = '" + is_email_verified + @"'
      ,[is_mobile_verified] ='" + is_mobile_verified + @"'
      ,[loc_lat] = '" + loc_lat + @"'
      ,[loc_lng] ='" + loc_lng + @"'
      ,[instruction] = '" + instruction + @"'
      ,[time_preference] = '" + time_preference + @"'
      ,[is_default] ='" + is_default + @"' 
      ,[updated_by] ='" + cust_id + @"'
      ,[updated_on] = getdate() 
 WHERE address_id='" + address_id + @"'";
                            if (!conn.ExecuteNonQuery(query))
                            {
                                return result;
                            }
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "add_cust_address":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string address_type = clsecom.get_value(inData.address_type);
                            string name = clsecom.get_value(inData.name);
                            if (auth.IsContainsSpecialChars(name))
                            {
                                result.status = "Name should not contain special characters";
                                return result;
                            }

                            string address = clsecom.get_value(inData.address);
                            if (auth.IsContainsSpecialChars(address))
                            {
                                result.status = "Address should not contain special characters";
                                return result;
                            }
                            string state_region = clsecom.get_value(inData.state_region);
                            if (auth.IsContainsSpecialChars(state_region))
                            {
                                result.status = "State/Region should not contain special characters";
                                return result;
                            }
                            string city = clsecom.get_value(inData.city);
                            if (auth.IsContainsSpecialChars(city))
                            {
                                result.status = "City should not contain special characters";
                                return result;
                            }
                            string zip = clsecom.get_value(inData.zip);
                            if (auth.IsContainsSpecialChars(zip))
                            {
                                result.status = "Zip should not contain special characters";
                                return result;
                            }
                            string country = clsecom.get_value(inData.country);
                            if (auth.IsContainsSpecialChars(country))
                            {
                                result.status = "Country should not contain special characters";
                                return result;
                            }
                            string landmark = clsecom.get_value(inData.landmark);
                            if (auth.IsContainsSpecialChars(landmark))
                            {
                                result.status = "Landmark should not contain special characters";
                                return result;
                            }
                            string mobile_num = clsecom.get_value(inData.mobile_num);
                            if (auth.IsContainsSpecialChars(mobile_num))
                            {
                                result.status = "Mobile Number should not contain special characters";
                                return result;
                            }
                            string wa_num = clsecom.get_value(inData.wa_num);
                            if (auth.IsContainsSpecialChars(wa_num))
                            {
                                result.status = "WhatsApp Number should not contain special characters";
                                return result;
                            }
                            string email = clsecom.get_value(inData.email);
                            if (auth.IsContainsSpecialChars(email))
                            {
                                result.status = "Email should not contain special characters";
                                return result;
                            }
                            string is_email_verified = clsecom.get_value(inData.is_email_verified);
                            string is_mobile_verified = clsecom.get_value(inData.is_mobile_verified);
                            string loc_lat = clsecom.get_value(inData.loc_lat);
                            string loc_lng = clsecom.get_value(inData.loc_lng);
                            string instruction = clsecom.get_value(inData.instruction);
                            if (auth.IsContainsSpecialChars(instruction))
                            {
                                result.status = "Instruction should not contain special characters";
                                return result;
                            }
                            string time_preference = clsecom.get_value(inData.time_preference);
                            if (auth.IsContainsSpecialChars(time_preference))
                            {
                                result.status = "Time preference should not contain special characters";
                                return result;
                            }
                            string is_default = clsecom.get_value(inData.is_default);
                            if (is_default.ToString().Trim().ToLower() == "y")
                            {
                                query = @"UPDATE [dbo].[ecom_address] SET [is_default]='N' 
where [cust_id]='" + cust_id + @"'";
                                conn.ExecuteNonQuery(query);
                            }
                            query = @"INSERT INTO [dbo].[ecom_address] 
           ([cust_id]
           ,[address_type] 
           ,[name]
           ,[address]
           ,[state_region]
           ,[city]
           ,[zip]
           ,[country]
           ,[landmark]
           ,[mobile_num]
           ,[wa_num]
           ,[email]
           ,[is_email_verified]
           ,[is_mobile_verified]
           ,[loc_lat]
           ,[loc_lng]
           ,[instruction]
           ,[time_preference]
           ,[is_default]
           ,[created_by]
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + cust_id + @"'
           ,'" + address_type + @"'
           ,'" + name + @"' 
           ,'" + address + @"'
           ,'" + state_region + @"'
           ,'" + city + @"'
           ,'" + zip + @"'
           ,'" + country + @"'
           ,'" + landmark + @"'
           ,'" + mobile_num + @"'
           ,'" + wa_num + @"'
           ,'" + email + @"'
           ,'" + is_email_verified + @"'
           ,'" + is_mobile_verified + @"'
           ,'" + loc_lat + @"'
           ,'" + loc_lng + @"'
           ,'" + instruction + @"'
           ,'" + time_preference + @"'
           ,'" + is_default + @"'
           ,'" + cust_id + @"'
           ,getdate()
           ,'Y')";
                            if (!conn.ExecuteNonQuery(query))
                            {
                                return result;
                            }
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "update_cust_profile":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string is_email_verified = clsecom.get_value(inData.is_email_verified);
                            string photo_data = clsecom.get_value(inData.photo_data);
                            bool is_exist = false;
                            string photo_url = clsecom.EcomSavePicAndGetFileName(photo_data, out is_exist);
                            string photo_url_to_delete = "";
                            if (!is_exist)
                            {
                                photo_url_to_delete = conn.ExecuteScalar("SELECT  [photo_url]   FROM [dbo].[ecom_customer] where [cust_id]='" + cust_id + "'");
                            }
                            string phone_number = clsecom.get_value(inData.phone_number);
                            string contact_name = clsecom.get_value(inData.contact_name);
                            string dob = clsecom.get_value(inData.dob);
                            dob = clsecom.get_date_to_normal(dob);
                            string gender = clsecom.get_value(inData.gender);
                            if (auth.IsContainsSpecialChars(contact_name))
                            {
                                result.status = "Contact Name should not contain special characters.";
                                return result;
                            }
                            res_msg = "";
                            query = @"UPDATE [dbo].[ecom_customer] 
   SET [store_id] = '" + store_id + @"' 
      ,[is_email_verified] =  '" + is_email_verified + @"' 
      ,[photo_url] =  '" + photo_url + @"' 
      ,[phone_number] =  '" + phone_number + @"' 
      ,[contact_name] =  '" + contact_name + @"' 
      ,[dob] = '" + dob + @"' 
      ,[gender] = '" + gender + @"'  
      ,[updated_by] =  '" + cust_id + @"'  
      ,[updated_on] = getdate() 
 WHERE cust_id='" + cust_id + @"'";
                            if (!conn.ExecuteNonQuery(query))
                            {
                                return result;
                            }
                            clsecom.SchDeletePic(photo_url_to_delete);
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "get_cust_profile":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            EcomAuthLoginItem login_item = clsecom.GetProfile(cust_id, out res_msg);
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(login_item);
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "val_cust":
                        {
                            string new_token = clsecomauth.GetNewToken(dev_id);
                            res_msg = "";
                            EcomAuthLoginItem login_item = clsecom.AuthLogin(inData, out res_msg);
                            if (!clsecom.IsEmpty(res_msg))
                            {
                                result.status_msg = res_msg;
                                result.data = Newtonsoft.Json.JsonConvert.SerializeObject(login_item);
                                return result;
                            }
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(login_item);
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "remove_from_fav":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string prod_id = clsecom.get_value(inData.prod_id);
                            res_msg = "";
                            if (!clsecom.removeFromFav(prod_id, cust_id, out res_msg))
                            {
                                if (!clsecom.IsEmpty(res_msg))
                                {
                                    result.status_msg = res_msg;
                                }
                                return result;
                            }
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "add_to_fav":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string prod_id = clsecom.get_value(inData.prod_id);
                            res_msg = "";
                            if (!clsecom.addToFav(prod_id, cust_id, out res_msg))
                            {
                                if (!clsecom.IsEmpty(res_msg))
                                {
                                    result.status_msg = res_msg;
                                }
                                return result;
                            }
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "get_cart_count":
                        {
                            res_msg = ""; 
                            string cart_count = clsecom.getCartCount(dev_id, cust_id, store_id, out res_msg);
                            if (!clsecom.IsEmpty(res_msg))
                                result.status_msg = res_msg;
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(cart_count);
                            result.token_new = "";
                            result.status = "SUCCESS";
                            break;
                        }
                    case "get_cart":
                        {
                            res_msg = "";
                            // string cust_id = clsecom.get_value(inData.cust_id);
                            List<EcomCartItem> cart_list = clsecom.getCart(dev_id, cust_id, store_id, out res_msg);
                            if (!clsecom.IsEmpty(res_msg))
                                result.status_msg = res_msg;
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(cart_list);
                            result.token_new = "";
                            result.status = "SUCCESS";
                            break;
                        }
                    case "remove_from_cart":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string cart_id = clsecom.get_value(inData.cart_id);
                            res_msg = "";
                            if (!clsecom.removeFromcart(cart_id, out res_msg))
                            {
                                if (!clsecom.IsEmpty(res_msg))
                                {
                                    result.status_msg = res_msg;
                                }
                                return result;
                            }
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "update_cart":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string cart_id = clsecom.get_value(inData.cart_id);
                            string offer_id_for_coupen = clsecom.get_value(inData.offer_id_for_coupen);
                            string product_color_id = clsecom.get_value(inData.product_color_id);
                            string product_size_id = clsecom.get_value(inData.product_size_id);
                            string is_custom_size = clsecom.get_value(inData.is_custom_size);
                            string is_gift = clsecom.get_value(inData.is_gift);
                            string is_surprise_gift = clsecom.get_value(inData.is_surprise_gift);
                            string is_need_gift_wish_card = clsecom.get_value(inData.is_need_gift_wish_card);
                            string gift_wish_card_url = clsecom.get_value(inData.gift_wish_card_url);
                            string custom_size_field1_value = clsecom.get_value(inData.custom_size_field1_value);
                            string custom_size_field2_value = clsecom.get_value(inData.custom_size_field2_value);
                            string custom_size_field3_value = clsecom.get_value(inData.custom_size_field3_value);
                            string custom_size_field4_value = clsecom.get_value(inData.custom_size_field4_value);
                            string custom_size_field5_value = clsecom.get_value(inData.custom_size_field5_value);
                            string custom_size_field6_value = clsecom.get_value(inData.custom_size_field6_value);
                            string custom_size_field7_value = clsecom.get_value(inData.custom_size_field7_value);
                            string custom_size_field8_value = clsecom.get_value(inData.custom_size_field8_value);
                            string custom_size_field9_value = clsecom.get_value(inData.custom_size_field9_value);
                            string custom_size_field10_value = clsecom.get_value(inData.custom_size_field10_value);
                            string custom_size_color_code = clsecom.get_value(inData.custom_size_color_code);
                            string custom_size_message = clsecom.get_value(inData.custom_size_message);
                            if (!clsecom.IsEmpty(custom_size_message))
                            {
                                if (auth.IsContainsSpecialChars(custom_size_message))
                                {
                                    result.status_msg = "Message at Custom Size & Color should not contain special characters";
                                    return result;
                                }
                            }

                            if (!clsecom.IsEmpty(custom_size_field1_value))
                            {
                                if (auth.IsContainsSpecialChars(custom_size_field1_value))
                                {
                                    result.status_msg = "Value at field 1 in Custom Size & Color should not contain special characters";
                                    return result;
                                }
                            }
                            if (!clsecom.IsEmpty(custom_size_field2_value))
                            {
                                if (auth.IsContainsSpecialChars(custom_size_field2_value))
                                {
                                    result.status_msg = "Value at field 2 in Custom Size & Color should not contain special characters";
                                    return result;
                                }
                            }
                            if (!clsecom.IsEmpty(custom_size_field3_value))
                            {
                                if (auth.IsContainsSpecialChars(custom_size_field3_value))
                                {
                                    result.status_msg = "Value at field 3 in Custom Size & Color should not contain special characters";
                                    return result;
                                }
                            }
                            if (!clsecom.IsEmpty(custom_size_field4_value))
                            {
                                if (auth.IsContainsSpecialChars(custom_size_field4_value))
                                {
                                    result.status_msg = "Value at field 4 in Custom Size & Color should not contain special characters";
                                    return result;
                                }
                            }
                            if (!clsecom.IsEmpty(custom_size_field5_value))
                            {
                                if (auth.IsContainsSpecialChars(custom_size_field5_value))
                                {
                                    result.status_msg = "Value at field 5 in Custom Size & Color should not contain special characters";
                                    return result;
                                }
                            }
                            if (!clsecom.IsEmpty(custom_size_field6_value))
                            {
                                if (auth.IsContainsSpecialChars(custom_size_field6_value))
                                {
                                    result.status_msg = "Value at field 6 in Custom Size & Color should not contain special characters";
                                    return result;
                                }
                            }
                            if (!clsecom.IsEmpty(custom_size_field7_value))
                            {
                                if (auth.IsContainsSpecialChars(custom_size_field7_value))
                                {
                                    result.status_msg = "Value at field 7 in Custom Size & Color should not contain special characters";
                                    return result;
                                }
                            }
                            if (!clsecom.IsEmpty(custom_size_field8_value))
                            {
                                if (auth.IsContainsSpecialChars(custom_size_field8_value))
                                {
                                    result.status_msg = "Value at field 8 in Custom Size & Color should not contain special characters";
                                    return result;
                                }
                            }
                            if (!clsecom.IsEmpty(custom_size_field9_value))
                            {
                                if (auth.IsContainsSpecialChars(custom_size_field9_value))
                                {
                                    result.status_msg = "Value at field 9 in Custom Size & Color should not contain special characters";
                                    return result;
                                }
                            }
                            if (!clsecom.IsEmpty(custom_size_field10_value))
                            {
                                if (auth.IsContainsSpecialChars(custom_size_field10_value))
                                {
                                    result.status_msg = "Value at field 10 in Custom Size & Color should not contain special characters";
                                    return result;
                                }
                            }
                            string prod_id = clsecom.get_value(inData.prod_id);
                            string quantity = clsecom.get_value(inData.quantity);
                            res_msg = "";
                            if (!clsecom.Updatecart(cart_id, offer_id_for_coupen,
             product_color_id,
             product_size_id,
             is_custom_size,
             is_gift,
             is_surprise_gift,
             is_need_gift_wish_card,
             gift_wish_card_url,
             custom_size_field1_value,
             custom_size_field2_value,
             custom_size_field3_value,
             custom_size_field4_value,
             custom_size_field5_value,
             custom_size_field6_value,
             custom_size_field7_value,
             custom_size_field8_value,
             custom_size_field9_value,
             custom_size_field10_value,
             custom_size_color_code,
             custom_size_message,
             prod_id,
             quantity,
             cust_id,
             dev_id, out res_msg))
                            {
                                if (!clsecom.IsEmpty(res_msg))
                                {
                                    result.status_msg = res_msg;
                                }
                                return result;
                            }
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "add_to_cart":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            //  string prod_id = clsecom.get_value(inData.prod_id);
                            //  string quantity = clsecom.get_value(inData.quantity);


                            string offer_id_for_coupen = clsecom.get_value(inData.offer_id_for_coupen);
                            string product_color_id = clsecom.get_value(inData.product_color_id);
                            string product_size_id = clsecom.get_value(inData.product_size_id);
                            string is_custom_size = clsecom.get_value(inData.is_custom_size);
                            string is_gift = clsecom.get_value(inData.is_gift);
                            string is_surprise_gift = clsecom.get_value(inData.is_surprise_gift);
                            string is_need_gift_wish_card = clsecom.get_value(inData.is_need_gift_wish_card);
                            string gift_wish_card_url = clsecom.get_value(inData.gift_wish_card_url);
                            string custom_size_field1_value = clsecom.get_value(inData.custom_size_field1_value);
                            string custom_size_field2_value = clsecom.get_value(inData.custom_size_field2_value);
                            string custom_size_field3_value = clsecom.get_value(inData.custom_size_field3_value);
                            string custom_size_field4_value = clsecom.get_value(inData.custom_size_field4_value);
                            string custom_size_field5_value = clsecom.get_value(inData.custom_size_field5_value);
                            string custom_size_field6_value = clsecom.get_value(inData.custom_size_field6_value);
                            string custom_size_field7_value = clsecom.get_value(inData.custom_size_field7_value);
                            string custom_size_field8_value = clsecom.get_value(inData.custom_size_field8_value);
                            string custom_size_field9_value = clsecom.get_value(inData.custom_size_field9_value);
                            string custom_size_field10_value = clsecom.get_value(inData.custom_size_field10_value);
                            string custom_size_color_code = clsecom.get_value(inData.custom_size_color_code);
                            string custom_size_message = clsecom.get_value(inData.custom_size_message);
                            if (!clsecom.IsEmpty(custom_size_message))
                            {
                                if (auth.IsContainsSpecialChars(custom_size_message))
                                {
                                    result.status_msg = "Message at Custom Size & Color should not contain special characters";
                                    return result;
                                }
                            }

                            if (!clsecom.IsEmpty(custom_size_field1_value))
                            {
                                if (auth.IsContainsSpecialChars(custom_size_field1_value))
                                {
                                    result.status_msg = "Value at field 1 in Custom Size & Color should not contain special characters";
                                    return result;
                                }
                            }
                            if (!clsecom.IsEmpty(custom_size_field2_value))
                            {
                                if (auth.IsContainsSpecialChars(custom_size_field2_value))
                                {
                                    result.status_msg = "Value at field 2 in Custom Size & Color should not contain special characters";
                                    return result;
                                }
                            }
                            if (!clsecom.IsEmpty(custom_size_field3_value))
                            {
                                if (auth.IsContainsSpecialChars(custom_size_field3_value))
                                {
                                    result.status_msg = "Value at field 3 in Custom Size & Color should not contain special characters";
                                    return result;
                                }
                            }
                            if (!clsecom.IsEmpty(custom_size_field4_value))
                            {
                                if (auth.IsContainsSpecialChars(custom_size_field4_value))
                                {
                                    result.status_msg = "Value at field 4 in Custom Size & Color should not contain special characters";
                                    return result;
                                }
                            }
                            if (!clsecom.IsEmpty(custom_size_field5_value))
                            {
                                if (auth.IsContainsSpecialChars(custom_size_field5_value))
                                {
                                    result.status_msg = "Value at field 5 in Custom Size & Color should not contain special characters";
                                    return result;
                                }
                            }
                            if (!clsecom.IsEmpty(custom_size_field6_value))
                            {
                                if (auth.IsContainsSpecialChars(custom_size_field6_value))
                                {
                                    result.status_msg = "Value at field 6 in Custom Size & Color should not contain special characters";
                                    return result;
                                }
                            }
                            if (!clsecom.IsEmpty(custom_size_field7_value))
                            {
                                if (auth.IsContainsSpecialChars(custom_size_field7_value))
                                {
                                    result.status_msg = "Value at field 7 in Custom Size & Color should not contain special characters";
                                    return result;
                                }
                            }
                            if (!clsecom.IsEmpty(custom_size_field8_value))
                            {
                                if (auth.IsContainsSpecialChars(custom_size_field8_value))
                                {
                                    result.status_msg = "Value at field 8 in Custom Size & Color should not contain special characters";
                                    return result;
                                }
                            }
                            if (!clsecom.IsEmpty(custom_size_field9_value))
                            {
                                if (auth.IsContainsSpecialChars(custom_size_field9_value))
                                {
                                    result.status_msg = "Value at field 9 in Custom Size & Color should not contain special characters";
                                    return result;
                                }
                            }
                            if (!clsecom.IsEmpty(custom_size_field10_value))
                            {
                                if (auth.IsContainsSpecialChars(custom_size_field10_value))
                                {
                                    result.status_msg = "Value at field 10 in Custom Size & Color should not contain special characters";
                                    return result;
                                }
                            }
                            string prod_id = clsecom.get_value(inData.prod_id);
                            string quantity = clsecom.get_value(inData.quantity);
                            res_msg = "";
                            if (!clsecom.addTocart(offer_id_for_coupen,
             product_color_id,
             product_size_id,
             is_custom_size,
             is_gift,
             is_surprise_gift,
             is_need_gift_wish_card,
             gift_wish_card_url,
             custom_size_field1_value,
             custom_size_field2_value,
             custom_size_field3_value,
             custom_size_field4_value,
             custom_size_field5_value,
             custom_size_field6_value,
             custom_size_field7_value,
             custom_size_field8_value,
             custom_size_field9_value,
             custom_size_field10_value,
             custom_size_color_code,
             custom_size_message,
             prod_id,
             quantity,
             cust_id,
             dev_id, out res_msg))
                            {
                                if (!clsecom.IsEmpty(res_msg))
                                {
                                    result.status_msg = res_msg;
                                }
                                return result;
                            }
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "get_search_cats":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            res_msg = "";
                            List<EcomSearchCatItem> items = clsecom.getSearchCats(store_id, out res_msg);
                            if (!clsecom.IsEmpty(res_msg))
                            {
                                result.status_msg = res_msg;
                                return result;
                            }
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(items);
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "update_requests_for_approval":
                        {
                            string type = clsecom.get_value(inData.type);
                            string id = clsecom.get_value(inData.id);
                            string admin_approval_status = clsecom.get_value(inData.admin_approval_status);
                            string admin_approval_remarks = clsecom.get_value(inData.admin_approval_remarks);
                            string title = "";
                            string msg = "";
                            if (clsecom.IsEmpty(admin_approval_status))
                                return result;
                            res_msg = "";
                            //if (clsecom.IsContainsSpecial(admin_approval_remarks, "Remarks", out res_msg))
                            //{
                            //    result.status_msg = res_msg;
                            //    return result;
                            //}
                            if (type == "ad")
                            {
                                query = @"UPDATE [dbo].[ecom_ad] 
   SET [admin_approval_status] = '" + admin_approval_status + @"'
,[admin_approval_remarks] = '" + admin_approval_remarks + @"'
      ,[updated_by] ='" + staff_id + @"'
,[updated_on] =getutcdate()
 WHERE  ad_id='" + id + @"'";
                                if (!conn.ExecuteNonQuery(query))
                                {
                                    result.status_msg = "Failed to process. Please try later(Error Code: 2001)";
                                    return result;
                                }
                                title = "New Ad Status";
                                msg = "Status of your Ad is changed to: " + clsecom.getStatusFromKey(admin_approval_status) + ". ";
                                if (!clsecom.IsEmpty(admin_approval_remarks))
                                    msg += "Remarks: " + admin_approval_remarks;
                                DataTable dtBrnd = conn.getDataTable("SELECT  [brand_id],name_brand_en  FROM  [dbo].[ecom_ad] where ad_id='" + id + "'");
                                string brand_id = dtBrnd.Rows[0]["brand_id"].ToString().Trim();
                                clsecom.sendPnToBrand("status_change_ad", store_id, brand_id, title, title, msg, msg, id);

                                title = "Ad Status Changed to: " + clsecom.getStatusFromKey(admin_approval_status);
                                msg = "Status of your Ad submitted for approval of Administrator is changed to " + clsecom.getStatusFromKey(admin_approval_status) + ". ";
                                if (!clsecom.IsEmpty(admin_approval_remarks))
                                    msg += "Remarks by Administrator: " + admin_approval_remarks + ".";
                                msg = clsecom.get_email_content(msg);
                                clsecom.sendEmailToBrand(brand_id, title, msg, false);

                                if (admin_approval_status.ToLower().Trim() == "approved")
                                {
                                    string name_brand_en = dtBrnd.Rows[0]["name_brand_en"].ToString().Trim();
                                    title = "Ad  $('#categoryDropDownList').html('');";
                                    msg = "One Ad of Brand " + name_brand_en + " is available now!";
                                    clsecom.sendPnToAllCustomers("new_ad_available", store_id, title, title, msg, msg, id);
                                }
                            }
                            else if (type == "brand")
                            {
                                DataTable dtBrand = conn.getDataTable(@"SELECT   [name_brand_en] ,[admin_approval_status],[admin_approval_remarks],email,f_name_owner,l_name_owner,icon_url  FROM  [dbo].[ecom_brand] where brand_id='" + id + "'");
                                string prev_status = dtBrand.Rows[0]["admin_approval_status"].ToString().Trim().ToLower();
                                bool is_firebase_account_created = clsecom.get_bool_from_yn(conn.ExecuteScalar("select isnull(is_firebase_account_created,'N') from [dbo].[ecom_staff] where [brand_id]='" + id + "'"));
                                if (admin_approval_status.ToLower().Trim() == "rejected")
                                {
                                    if (!is_firebase_account_created)
                                    {
                                        string brand_name = dtBrand.Rows[0]["name_brand_en"].ToString().Trim();
                                        title = "New Business Status";
                                        msg = "Status of your Business(" + brand_name + ") is changed to: " + clsecom.getStatusFromKey(admin_approval_status) + ". ";
                                        if (!clsecom.IsEmpty(admin_approval_remarks))
                                            msg += "Remarks: " + admin_approval_remarks + ". ";
                                        msg += "You can submit again using Sign Up form.";
                                        clsecom.sendPnToBrand("status_change_business_acc", store_id, id, title, title, msg, msg, "");

                                        title = "Business Status Changed to: " + clsecom.getStatusFromKey(admin_approval_status);
                                        msg = "Status of your Business submitted for approval of Administrator is changed to " + clsecom.getStatusFromKey(admin_approval_status) + ". ";
                                        if (!clsecom.IsEmpty(admin_approval_remarks))
                                        {
                                            msg += Environment.NewLine;
                                            msg += "Remarks by Administrator: " + admin_approval_remarks + ". ";
                                        }
                                        msg += Environment.NewLine;
                                        msg += "Thank you for interesting in Olaala. You can submit again for Approval using the Sign Up form.";
                                        msg = clsecom.get_email_content(msg);
                                        if (clsecom.sendEmailToBrand(id, title, msg, false))
                                        {
                                            conn.ExecuteNonQuery("DELETE FROM [dbo].[ecom_staff] where brand_id='" + id + "'");
                                            conn.ExecuteNonQuery("DELETE FROM [dbo].[ecom_brand] where brand_id='" + id + "'");
                                            clsecom.SchDeletePic(dtBrand.Rows[0]["icon_url"].ToString().Trim());
                                        }
                                    }
                                    else
                                    {
                                        string brand_name = dtBrand.Rows[0]["name_brand_en"].ToString().Trim();
                                        title = "New Business Status";
                                        msg = "Status of your Business(" + brand_name + ") is changed to: " + clsecom.getStatusFromKey(admin_approval_status) + ". ";
                                        if (!clsecom.IsEmpty(admin_approval_remarks))
                                            msg += "Remarks: " + admin_approval_remarks + ". ";
                                        msg += "You can submit again for approval by login and using Business Profile form in the App.";
                                        clsecom.sendPnToBrand("status_change_business_acc", store_id, id, title, title, msg, msg, "");

                                        title = "Business Status Changed to: " + clsecom.getStatusFromKey(admin_approval_status);
                                        msg = "Status of your Business submitted for approval of Administrator is changed to " + clsecom.getStatusFromKey(admin_approval_status) + ". ";
                                        if (!clsecom.IsEmpty(admin_approval_remarks))
                                            msg += "Remarks by Administrator: " + admin_approval_remarks + ". ";
                                        msg += "You can submit again for approval by login and using Business Profile form in the App.";
                                        msg = clsecom.get_email_content(msg);
                                        if (clsecom.sendEmailToBrand(id, title, msg, false))
                                        {
                                            query = @"UPDATE [dbo].[ecom_brand] 
   SET [admin_approval_status] = '" + admin_approval_status + @"'
,[admin_approval_remarks] = '" + admin_approval_remarks + @"'
      ,[updated_by] ='" + staff_id + @"'
,[updated_on] =getutcdate()
 WHERE  brand_id='" + id + @"'";
                                            if (!conn.ExecuteNonQuery(query))
                                            {
                                                result.status_msg = "Failed to process. Please try later(Error Code: 2002)";
                                                return result;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    string brand_name = dtBrand.Rows[0]["name_brand_en"].ToString().Trim();
                                    title = "New Business Status";
                                    msg = "Status of your Business(" + brand_name + ") is changed to: " + clsecom.getStatusFromKey(admin_approval_status) + ". ";
                                    if (!clsecom.IsEmpty(admin_approval_remarks))
                                        msg += "Remarks: " + admin_approval_remarks + ". ";

                                    if (admin_approval_status.ToLower().Trim() == "approved")
                                    {
                                        if (!is_firebase_account_created)
                                        {
                                            msg += "Please Check your email for login details.";
                                        }
                                    }
                                    clsecom.sendPnToBrand("status_change_business_acc", store_id, id, title, title, msg, msg, id);
                                    title = "Business Status Changed to: " + clsecom.getStatusFromKey(admin_approval_status);
                                    msg = "Status of your Business submitted for the approval of Administrator is changed to " + clsecom.getStatusFromKey(admin_approval_status) + ". ";
                                    if (!clsecom.IsEmpty(admin_approval_remarks))
                                        msg += "Remarks by Administrator: " + admin_approval_remarks + ". ";
                                    string login_id = "";
                                    string pw = "";
                                    if (admin_approval_status.ToLower().Trim() == "approved")
                                    {
                                        if (!is_firebase_account_created)
                                        {
                                            login_id = dtBrand.Rows[0]["email"].ToString().Trim();
                                            pw = clsecom.generate_password();
                                            msg += Environment.NewLine + Environment.NewLine + "Below is the Login Details for login into the Olaala App;" + Environment.NewLine;
                                            msg += "Login Email: " + login_id + Environment.NewLine + "";
                                            msg += "Password: " + pw + Environment.NewLine + "";

                                        }
                                    }
                                    msg = clsecom.get_email_content(msg);
                                    if (clsecom.sendEmailToBrand(id, title, msg, false))
                                    {
                                        query = @"UPDATE [dbo].[ecom_brand] 
   SET [admin_approval_status] = '" + admin_approval_status + @"'
,[admin_approval_remarks] = '" + admin_approval_remarks + @"'
      ,[updated_by] ='" + staff_id + @"'
,[updated_on] =getutcdate()
 WHERE  brand_id='" + id + @"'";
                                        if (admin_approval_status.ToLower().Trim() == "approved")
                                        {
                                            if (!is_firebase_account_created)
                                            {
                                                DataTable dtDataExist = conn.getDataTable(@"SELECT distinct [email]  FROM  [dbo].[ecom_staff] where [brand_id]='" + id + "' and email='" + dtBrand.Rows[0]["email"].ToString().Trim() + @"'");
                                                if (dtDataExist.Rows.Count == 0)
                                                {
                                                    query = @"INSERT INTO [dbo].[ecom_staff]
           ([brand_id]
,[store_id]
           ,[role_type]
           ,[email] 
           ,[f_name]
           ,[l_name] 
           ,[created_on] 
           ,[is_active]
,[firebase_initial_pw])
     VALUES
           ('" + id + @"'
,'" + store_id + @"'
           ,'business_owner'
           ,'" + dtBrand.Rows[0]["email"].ToString().Trim() + @"' 
          ,'" + dtBrand.Rows[0]["f_name_owner"].ToString().Trim() + @"'
           ,'" + dtBrand.Rows[0]["l_name_owner"].ToString().Trim() + @"' 
           ,getutcdate() 
           ,'Y'
,'" + pw + @"')";
                                                    if (!conn.ExecuteNonQuery(query))
                                                    {
                                                        result.status_msg = "Failed to process. Please try later(Error Code: 2005)";
                                                        return result;
                                                    }
                                                }
                                                else
                                                {
                                                    query = @"UPDATE [dbo].[ecom_staff]  SET  [firebase_initial_pw] = '" + pw + @"' WHERE brand_id='" + id + "'";
                                                    if (!conn.ExecuteNonQuery(query))
                                                    {
                                                        result.status_msg = "Failed to process. Please try later(Error Code: 2006)";
                                                        return result;
                                                    }
                                                }
                                                query = @"UPDATE [dbo].[ecom_brand] 
   SET [admin_approval_status] = '" + admin_approval_status + @"'
,[admin_approval_remarks] = '" + admin_approval_remarks + @"'
      ,[updated_by] ='" + staff_id + @"'
,[updated_on] =getutcdate() 
 WHERE  brand_id='" + id + @"'";
                                            }
                                        }
                                        if (!conn.ExecuteNonQuery(query))
                                        {
                                            result.status_msg = "Failed to process. Please try later(Error Code: 2002)";
                                            return result;
                                        }


                                        if (admin_approval_status.ToLower().Trim() == "approved")
                                        {
                                            title = "Brand Available";
                                            msg = "Brand " + brand_name + " is available now in the App!";
                                            clsecom.sendPnToAllCustomers("new_brand_available", store_id, title, title, msg, msg, id);
                                        }
                                    }
                                }
                            }
                            else if (type == "product")
                            {
                                title = "New Product Status";
                                DataTable dtProd = conn.getDataTable("SELECT  [name_en],is_gift_available,is_surprise_gift_available  FROM  [dbo].[ecom_product] where [prod_id]='" + id + @"'");
                                string product_name = dtProd.Rows[0]["name_en"].ToString().Trim();
                                msg = "Status of your Product(" + product_name + ") is changed to: " + clsecom.getStatusFromKey(admin_approval_status) + ". ";
                                if (!clsecom.IsEmpty(admin_approval_remarks))
                                    msg += "Remarks: " + admin_approval_remarks + ". ";
                                clsecom.sendPnToBrand("status_change_product", store_id, id, title, title, msg, msg, id);

                                title = "Product Status Changed to: " + clsecom.getStatusFromKey(admin_approval_status);
                                msg = "Status of your Product(" + product_name + ") submitted for approval of Administrator is changed to " + clsecom.getStatusFromKey(admin_approval_status) + ". ";
                                if (!clsecom.IsEmpty(admin_approval_remarks))
                                    msg += "Remarks by Administrator: " + admin_approval_remarks + ". ";
                                msg = clsecom.get_email_content(msg);
                                if (clsecom.sendEmailToBrand(id, title, msg, false))
                                {
                                    query = @"UPDATE [dbo].[ecom_product] 
   SET [admin_approval_status] = '" + admin_approval_status + @"'
,[admin_approval_remarks] = '" + admin_approval_remarks + @"'
      ,[updated_by] ='" + staff_id + @"'
,[updated_on] =getutcdate()
 WHERE  prod_id='" + id + @"'";
                                    if (!conn.ExecuteNonQuery(query))
                                    {
                                        result.status_msg = "Failed to process. Please try later(Error Code: 2002)";
                                        return result;
                                    }
                                    if (admin_approval_status.ToLower().Trim() == "approved")
                                    {
                                        title = "Product Available";
                                        msg = "Product " + product_name + " is available now!";
                                        clsecom.sendPnToAllCustomers("new_product_available", store_id, title, title, msg, msg, id);

                                        string is_gift_available = dtProd.Rows[0]["is_gift_available"].ToString().Trim();
                                        string is_surprise_gift_available = dtProd.Rows[0]["is_surprise_gift_available"].ToString().Trim();
                                        if (clsecom.get_bool_from_yn(is_gift_available))
                                        {
                                            title = "New Gift Product";
                                            msg = "New Gift Product " + product_name + " is available now!";
                                            clsecom.sendPnToAllCustomers("new_gift_available", store_id, title, title, msg, msg, id);
                                        }
                                        if (clsecom.get_bool_from_yn(is_surprise_gift_available))
                                        {
                                            title = "New Surprise Gift Product";
                                            msg = "New Surprise Gift Product " + product_name + " is available now!";
                                            clsecom.sendPnToAllCustomers("new_surprise_gift_available", store_id, title, title, msg, msg, id);
                                        }
                                    }
                                }
                            }
                            else if (type == "order_cancel")
                            {
                                title = "Order Cancel Status";
                                msg = "Status of your request to Cancel Order is changed to: " + clsecom.getStatusFromKey(admin_approval_status) + ". ";
                                if (!clsecom.IsEmpty(admin_approval_remarks))
                                    msg += "Remarks: " + admin_approval_remarks + ". ";
                                clsecom.sendPnToBrand("status_change_order_cancel_request", store_id, id, title, title, msg, msg, id);

                                title = "Order Cancel Status Changed to: " + clsecom.getStatusFromKey(admin_approval_status);
                                msg = "Status of your request to cancel the Order is changed to " + clsecom.getStatusFromKey(admin_approval_status) + ". ";
                                if (!clsecom.IsEmpty(admin_approval_remarks))
                                    msg += "Remarks by Administrator: " + admin_approval_remarks + ". ";
                                msg = clsecom.get_email_content(msg);
                                if (clsecom.sendEmailToCustomer(id, title, msg, false))
                                {
                                    query = @"UPDATE [dbo].[ecom_order_cancel] 
   SET [admin_approval_status] = '" + admin_approval_status + @"'
,[admin_approval_remarks] = '" + admin_approval_remarks + @"'
      ,[updated_by] ='" + staff_id + @"'
,[updated_on] =getutcdate()
 WHERE  order_cancel_id='" + id + @"'";
                                    if (!conn.ExecuteNonQuery(query))
                                    {
                                        result.status_msg = "Failed to process. Please try later(Error Code: 2002)";
                                        return result;
                                    }
                                }
                            }
                            result.status_msg = "";
                            result.data = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "get_requests_for_approval":
                        {
                            res_msg = "";
                            List<EcomreqForApprovalItem> items = clsecom.get_req_for_approval(time_offset, store_id, out res_msg);
                            result.status_msg = res_msg;
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(items);
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }

                    case "get_products":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            res_msg = "";
                            List<EcomProductData> prods = clsecom.getProductsDyna(inData, out res_msg);
                            if (!clsecom.IsEmpty(res_msg))
                            {
                                result.status_msg = res_msg;
                                return result;
                            }
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(prods);
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    //==
                    case "get_gift":
                        {
                            string gift_id = clsecom.get_value(inData.gift_id);
                            string cond_field = clsecom.get_value(inData.cond_field);
                            if (clsecom.IsEmpty(cond_field))
                                cond_field = "order_by";
                            string ecom_max_products_scroll_load = clsecom.GetConfig("ecom_max_products_scroll_load");
                            if (clsecom.IsEmpty(ecom_max_products_scroll_load))
                                ecom_max_products_scroll_load = "10";
                            string last_idx = clsecom.get_value(inData.last_idx);
                            if (last_idx == "")
                                last_idx = "0";
                            string is_active = clsecom.get_value(inData.is_active);
                            query = @"SELECT  b.[gift_id]
  ,b.[store_id]
      ,b.[min_price]
      ,b.[max_price]
      ,b.[icon_url]
      ,b.[order_by] 
      ,b.[created_by]
      ,convert(char(20),b.[created_on],113) as created_on
      ,b.[updated_by]
	   ,convert(char(20),b.[updated_on],113) as updated_on 
      ,b.[is_active]
,RowNum

from ( 
SELECT ROW_NUMBER() OVER (ORDER BY  " + cond_field + @") As RowNum, *  
FROM [dbo].[ecom_gift] 
where   
[is_active]=case when '" + is_active + @"'='' then [is_active] else '" + is_active + @"' end and 
[store_id]='" + store_id + @"') as b
 
left join ecom_staff esc on 
esc.staff_id=b.[created_by]

left join ecom_staff esu on 
esu.staff_id=b.[updated_by]

WHERE RowNum BETWEEN 1+(" + last_idx + @") AND (" + last_idx + @"+" + ecom_max_products_scroll_load + @")";
                            DataTable dtData = conn.getDataTable(query);
                            List<EcomGiftItem> items = new List<EcomGiftItem>();
                            for (int i = 0; i < dtData.Rows.Count; i++)
                            {
                                EcomGiftItem item = new EcomGiftItem();
                                item.gift_id = dtData.Rows[i]["gift_id"].ToString().Trim();
                                item.store_id = dtData.Rows[i]["store_id"].ToString().Trim();
                                item.min_price = dtData.Rows[i]["min_price"].ToString().Trim();
                                item.max_price = dtData.Rows[i]["max_price"].ToString().Trim();
                                item.icon_url = dtData.Rows[i]["icon_url"].ToString().Trim();
                                item.order_by = dtData.Rows[i]["order_by"].ToString().Trim();
                                item.created_by = dtData.Rows[i]["created_by"].ToString().Trim();
                                item.created_on = dtData.Rows[i]["created_on"].ToString().Trim();
                                item.updated_by = dtData.Rows[i]["updated_by"].ToString().Trim();
                                item.updated_on = dtData.Rows[i]["updated_on"].ToString().Trim();
                                item.is_active = dtData.Rows[i]["is_active"].ToString().Trim();
                                //item.created_by_name = dtData.Rows[i]["created_by_name"].ToString().Trim();
                                //item.updated_by_name = dtData.Rows[i]["updated_by_name"].ToString().Trim();
                                item.last_row_index = dtData.Rows[i]["RowNum"].ToString().Trim();
                                items.Add(item);
                            }
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(items);
                            //result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.token_new = "";
                            result.status = "SUCCESS";
                            break;
                        }
                   
                    case "get_brand":
                        {
                            string brand_id = clsecom.get_value(inData.brand_id);
                            string cond_field = clsecom.get_value(inData.cond_field);
                            if (clsecom.IsEmpty(cond_field))
                                cond_field = "order_by";

                            string ecom_max_products_scroll_load = clsecom.get_value(inData.ecom_max_products_scroll_load);
                            if (clsecom.IsEmpty(ecom_max_products_scroll_load))
                            {
                                ecom_max_products_scroll_load = clsecom.GetConfig("ecom_max_products_scroll_load");
                                if (clsecom.IsEmpty(ecom_max_products_scroll_load))
                                    ecom_max_products_scroll_load = "20";
                            }
                            string last_idx = clsecom.get_value(inData.last_idx);
                            if (last_idx == "")
                                last_idx = "0";
                            string is_active = clsecom.get_value(inData.is_active);
                            string admin_approval_status = clsecom.get_value(inData.admin_approval_status);
                            query = @"SELECT  b.[brand_id]
      ,b.[store_id]
      ,b.[email]
      ,b.[name_brand_en]
      ,b.[name_brand_ar]
      ,b.[brand_desc_en]
      ,b.[brand_desc_ar]
      ,b.[business_sector_id]
      ,b.[phone]
      ,b.[mobile_whatsapp]
      ,b.[reg_message_to_admin]
      ,b.[icon_url]
      ,b.[loc_lat]
      ,b.[loc_lng]
      ,b.[admin_approval_status]
      ,b.[admin_approval_remarks]
      ,b.[need_approval_to_add_prod]
      ,b.[order_by]
      ,b.[allow_delivery_within_km]
      ,b.[created_by]
      ,convert(char(20),b.[created_on],113) as created_on
      ,b.[updated_by]
	   ,convert(char(20),b.[updated_on],113) as updated_on 
      ,b.[is_active]
 
,RowNum

from ( 
SELECT ROW_NUMBER() OVER (ORDER BY  " + cond_field + @") As RowNum, *  
FROM [dbo].[ecom_brand] 
where  
[brand_id]=case when '" + brand_id + @"'='' then [brand_id] else '" + brand_id + @"' end and 
[is_active]=case when '" + is_active + @"'='' then [is_active] else '" + is_active + @"' end and 
[admin_approval_status]=case when '" + admin_approval_status + @"'='' then [admin_approval_status] else '" + admin_approval_status + @"' end

and  [store_id]='" + store_id + @"') as b
 
left join ecom_staff esc on 
esc.staff_id=b.[created_by]

left join ecom_staff esu on 
esu.staff_id=b.[updated_by]

WHERE RowNum BETWEEN 1+(" + last_idx + @") AND (" + last_idx + @"+" + ecom_max_products_scroll_load + @")
";
                            DataTable dtData = conn.getDataTable(query);
                            List<EcomBrandItem> items = new List<EcomBrandItem>();
                            for (int i = 0; i < dtData.Rows.Count; i++)
                            {
                                EcomBrandItem item = new EcomBrandItem();
                                item.brand_id = dtData.Rows[i]["brand_id"].ToString().Trim();
                                item.store_id = dtData.Rows[i]["store_id"].ToString().Trim();
                                item.email = dtData.Rows[i]["email"].ToString().Trim();
                                //item.name_owner_en = dtData.Rows[i]["name_owner_en"].ToString().Trim();
                                //item.name_owner_ar = dtData.Rows[i]["name_owner_ar"].ToString().Trim();
                                item.name_brand_en = dtData.Rows[i]["name_brand_en"].ToString().Trim();
                                item.name_brand_ar = dtData.Rows[i]["name_brand_ar"].ToString().Trim();
                                item.brand_desc_en = dtData.Rows[i]["brand_desc_en"].ToString().Trim();
                                item.brand_desc_ar = dtData.Rows[i]["brand_desc_ar"].ToString().Trim();
                                item.business_sector_id = dtData.Rows[i]["business_sector_id"].ToString().Trim();
                                item.phone = dtData.Rows[i]["phone"].ToString().Trim();
                                item.mobile_whatsapp = dtData.Rows[i]["mobile_whatsapp"].ToString().Trim();
                                item.reg_message_to_admin = dtData.Rows[i]["reg_message_to_admin"].ToString().Trim();
                                item.icon_url = dtData.Rows[i]["icon_url"].ToString().Trim();
                                item.loc_lat = dtData.Rows[i]["loc_lat"].ToString().Trim();
                                item.loc_lng = dtData.Rows[i]["loc_lng"].ToString().Trim();
                                item.admin_approval_status = dtData.Rows[i]["admin_approval_status"].ToString().Trim();
                                item.admin_approval_remarks = dtData.Rows[i]["admin_approval_remarks"].ToString().Trim();
                                item.need_approval_to_add_prod = dtData.Rows[i]["need_approval_to_add_prod"].ToString().Trim();
                                item.order_by = dtData.Rows[i]["order_by"].ToString().Trim();
                                item.allow_delivery_within_km = dtData.Rows[i]["allow_delivery_within_km"].ToString().Trim();
                                item.created_by = dtData.Rows[i]["created_by"].ToString().Trim();
                                item.created_on = dtData.Rows[i]["created_on"].ToString().Trim();
                                item.updated_by = dtData.Rows[i]["updated_by"].ToString().Trim();
                                item.updated_on = dtData.Rows[i]["updated_on"].ToString().Trim();
                                item.is_active = dtData.Rows[i]["is_active"].ToString().Trim();
                                //item.created_by_name = dtData.Rows[i]["created_by_name"].ToString().Trim();
                                //item.updated_by_name = dtData.Rows[i]["updated_by_name"].ToString().Trim();
                                item.last_row_index = dtData.Rows[i]["RowNum"].ToString().Trim();
                                items.Add(item);
                            }
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(items);
                            //result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.token_new = "";
                            result.status = "SUCCESS";
                            break;
                        }
                    case "get_ads_home":
                        {
                            res_msg = "";
                            string brand_id = clsecom.get_value(inData.brand_id);
                            List<EcomAdOlaData> ads = clsecom.getAdsOlaForHome(store_id, out res_msg);
                            if (!clsecom.IsEmpty(res_msg))
                            {
                                result.status_msg = res_msg;
                                return result;
                            }
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(ads);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "get_business_sector":
                        {
                            res_msg = "";
                            string is_active = clsecom.get_value(inData.is_active);
                            List<EcomBusinessSectorData> res = clsecom.getBusinessSectors(store_id, is_active, out res_msg);
                            if (!clsecom.IsEmpty(res_msg))
                            {
                                result.status_msg = res_msg;
                                return result;
                            }
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(res);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "update_brand_and_request":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string brand_id = clsecom.get_value(inData.brand_id);
                            if (clsecom.IsEmpty(brand_id))
                            {
                                clsecom.Log(log_key, "Received without brand_id.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string email = clsecom.get_value(inData.email);
                            string stat = "";
                            if (!clsecom.IsEmptyAndContainsSpecial(email, "Email", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }
                            string name_owner_en = clsecom.get_value(inData.name_owner_en);
                            if (!clsecom.IsEmptyAndContainsSpecial(name_owner_en, "Business Owner Name in English", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }
                            string name_owner_ar = clsecom.get_value(inData.name_owner_ar);
                            if (!clsecom.IsEmptyAndContainsSpecial(name_owner_ar, "Business Owner Name in Arabic", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }
                            string name_brand_en = clsecom.get_value(inData.name_brand_en);
                            if (!clsecom.IsEmptyAndContainsSpecial(name_brand_en, "Brand Name in English", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }

                            DataTable dtEx = conn.getDataTable(@"SELECT *  FROM  [dbo].[ecom_brand] where [name_brand_en]='" + name_brand_en + @"' and [store_id]='" + store_id + @"' and brand_id!='" + brand_id + "'");
                            if (dtEx.Rows.Count > 0)
                            {
                                result.status_msg = "Brand Name in English already using by another Brand. Please choose another name. If you are the authorized person for using this name please contact administrator(" + clsecom.GetConfig("ecom_email_support") + ")";
                                return result;
                            }
                            string name_brand_ar = clsecom.get_value(inData.name_brand_ar);
                            if (!clsecom.IsEmptyAndContainsSpecial(name_brand_ar, "Brand Name in Arabic", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }
                            dtEx = conn.getDataTable(@"SELECT *  FROM  [dbo].[ecom_brand] where [name_brand_ar]='" + name_brand_ar + @"' and [store_id]='" + store_id + @"' and brand_id!='" + brand_id + "'");
                            if (dtEx.Rows.Count > 0)
                            {
                                result.status_msg = "Brand Name in Arabic already using by another Brand. Please choose another name. If you are the authorized person for using this name please contact administrator(" + clsecom.GetConfig("ecom_email_support") + ")";
                                return result;
                            }
                            string brand_desc_en = clsecom.get_value(inData.brand_desc_en);
                            string brand_desc_ar = clsecom.get_value(inData.brand_desc_ar);
                            string business_sector_id = clsecom.get_value(inData.business_sector_id);
                            if (!clsecom.IsEmptyAndContainsSpecial(business_sector_id, "Business Sector", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }

                            string phone = clsecom.get_value(inData.phone);
                            if (!clsecom.IsEmptyAndContainsSpecial(phone, "Phone", out stat))
                            {
                                if (!clsecom.IsEmpty(stat))
                                    result.status_msg = stat;
                                return result;
                            }

                            string mobile_whatsapp = clsecom.get_value(inData.mobile_whatsapp);
                            string reg_message_to_admin = clsecom.get_value(inData.reg_message_to_admin);
                            string icon_data = clsecom.get_value(inData.icon_data);
                            bool is_exist = false;
                            string icon_url = clsecom.EcomSavePicAndGetFileName(icon_data, out is_exist);
                            string loc_lat = clsecom.get_value(inData.loc_lat);
                            string loc_lng = clsecom.get_value(inData.loc_lng);
                            string allow_delivery_within_km = clsecom.get_value(inData.allow_delivery_within_km);
                            if (clsecom.IsEmpty(allow_delivery_within_km))
                                allow_delivery_within_km = "-1";
                            string admin_approval_status = "pending_approval";
                            query = @"UPDATE [dbo].[ecom_brand]
   SET [store_id] = '" + store_id + @"'
      ,[email] ='" + email + @"'
      ,[name_owner_en] ='" + name_owner_en + @"'
      ,[name_owner_ar]  =N'" + name_owner_ar + @"'
      ,[name_brand_en]  ='" + name_brand_en + @"'
      ,[name_brand_ar]  =N'" + name_brand_ar + @"'
      ,[brand_desc_en]  ='" + brand_desc_en + @"'
      ,[brand_desc_ar]  =N'" + brand_desc_ar + @"'
      ,[business_sector_id]  ='" + business_sector_id + @"'
      ,[phone]  ='" + phone + @"'
      ,[mobile_whatsapp]  ='" + mobile_whatsapp + @"'
      ,[reg_message_to_admin]  ='" + reg_message_to_admin + @"'
      ,[icon_url]  ='" + icon_url + @"'
      ,[loc_lat]  ='" + loc_lat + @"'
      ,[loc_lng]  ='" + loc_lng + @"'
      ,[admin_approval_status]  ='" + admin_approval_status + @"' 
      ,[need_approval_to_add_prod]  ='N' 
      ,[allow_delivery_within_km]  ='" + allow_delivery_within_km + @"' 
      ,[updated_by]  ='" + staff_id + @"'
      ,[updated_on]  =getdate()
      ,[is_active]  ='Y'
 WHERE  brand_id='" + brand_id + @"'";
                            if (!conn.ExecuteNonQuery(query))
                                return result;
                            string title_en = "Request for Approval";
                            string title_ar = "طلب موافقة";
                            string msg_en = "Information of Brand(" + name_brand_en + ") is updated and is waiting for Approval of Administrator";
                            string msg_ar = "يتم تحديث معلومات العلامة التجارية (" + name_brand_ar + ") وتنتظر موافقة المسؤول";
                            string destination_type = "approve_brand";
                            string destination_value = brand_id;
                            clsecom.sendPnToAllAdmins(store_id, title_en, title_ar, msg_en, msg_ar, destination_type, destination_value);
                            result.data = "";
                            stat = admin_approval_status.ToLower().Replace("_", " ");
                            result.status_msg = "Your account Approval Status is:" + clsecom.ToTitleCase(stat) + ". Please wait for 48 business hours. You will get email notification after completing the Approval Process. If more delayed please contact administrator(" + clsecom.GetConfig("ecom_email_support") + ")";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
//                    case "login_staff":
//                        {
//                            if (clsecom.IsEmpty(token))
//                            {
//                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
//                                return result;
//                            }
//                            string email = clsecom.get_value(inData.email);
//                            if (clsecom.IsEmpty(email))
//                            {
//                                result.status_msg = "Email should not be empty";
//                                return result;
//                            }
//                            if (auth.IsContainsSpecialChars(email))
//                            {
//                                result.status_msg = "Email should not contain special characters";
//                                return result;
//                            }
//                            string is_email_verified = clsecom.get_value(inData.is_email_verified);
//                            DataTable dtExist = conn.getDataTable(@"SELECT [staff_id]
//      ,[brand_id]
//      ,[store_id]
//      ,[role_type]
//      ,[email]
//,[is_email_verified]
//      ,[name_en]
//      ,[name_ar]
//      ,[mobile_num]
//      ,[wa_num]
//      ,[address_en]
//      ,[address_ar]
//      ,[loc_lat]
//      ,[loc_lng]
//      ,[created_by]
//      ,[created_on]
//      ,[updated_by]
//      ,[updated_on]
//      ,[is_active]
//  FROM  [dbo].[ecom_staff] where email='" + email + @"' and [store_id]='" + store_id + @"'");
//                            if (dtExist.Rows.Count == 0)
//                            {
//                                result.status_msg = "Unauthorized";
//                                return result;
//                            }
//                            if (!clsecom.get_bool_from_yn(dtExist.Rows[0]["is_active"].ToString().ToLower()))
//                            {
//                                result.status_msg = "This user account is deactivated";
//                                return result;
//                            }
//                            if (!clsecom.IsEmpty(is_email_verified))
//                            {
//                                if (is_email_verified != dtExist.Rows[0]["is_email_verified"].ToString())
//                                {
//                                    conn.ExecuteNonQuery(@"UPDATE [dbo].[ecom_staff]
//   SET [is_email_verified]='" + is_email_verified + @"'
// WHERE staff_id='" + dtExist.Rows[0]["staff_id"].ToString() + "'");
//                                }
//                            }
//                            else
//                                is_email_verified = dtExist.Rows[0]["is_email_verified"].ToString();
//                            EcomUserData userData = new EcomUserData();
//                            userData.user_type = dtExist.Rows[0]["role_type"].ToString();
//                            userData.customer = new EcomCustomerData();
//                            userData.brand = new EcomBrandData();
//                            userData.staff = new EcomStaffData();
//                            userData.staff.staff_id = dtExist.Rows[0]["staff_id"].ToString();
//                            userData.staff.brand_id = dtExist.Rows[0]["brand_id"].ToString();
//                            userData.staff.store_id = dtExist.Rows[0]["store_id"].ToString();
//                            userData.staff.role_type = dtExist.Rows[0]["role_type"].ToString();
//                            userData.staff.email = dtExist.Rows[0]["email"].ToString();
//                            userData.staff.is_email_verified = is_email_verified;
//                            userData.staff.name_en = dtExist.Rows[0]["name_en"].ToString();
//                            userData.staff.name_ar = dtExist.Rows[0]["name_ar"].ToString();
//                            userData.staff.mobile_num = dtExist.Rows[0]["mobile_num"].ToString();
//                            userData.staff.wa_num = dtExist.Rows[0]["wa_num"].ToString();
//                            userData.staff.address_en = dtExist.Rows[0]["address_en"].ToString();
//                            userData.staff.address_ar = dtExist.Rows[0]["address_ar"].ToString();
//                            userData.staff.loc_lat = dtExist.Rows[0]["loc_lat"].ToString();
//                            userData.staff.loc_lng = dtExist.Rows[0]["loc_lng"].ToString();
//                            userData.staff.created_by = dtExist.Rows[0]["created_by"].ToString();
//                            userData.staff.created_on = dtExist.Rows[0]["created_on"].ToString();
//                            userData.staff.updated_by = dtExist.Rows[0]["updated_by"].ToString();
//                            userData.staff.updated_on = dtExist.Rows[0]["updated_on"].ToString();
//                            userData.staff.is_active = dtExist.Rows[0]["is_active"].ToString();

//                            if (dtExist.Rows[0]["role_type"].ToString().ToLower() == "admin")
//                            {
//                                result.data = Newtonsoft.Json.JsonConvert.SerializeObject(userData);
//                                result.status_msg = "";
//                                result.token_new = clsecomauth.GetNewToken(dev_id);
//                                result.status = "SUCCESS";
//                                return result;
//                            }
//                            string brand_id = dtExist.Rows[0]["brand_id"].ToString();
//                            string brand_email = dtExist.Rows[0]["email"].ToString();
//                            dtExist = conn.getDataTable(@"SELECT  [brand_id]
//      ,[store_id]
//      ,[email]
//      ,[name_owner_en]
//      ,[name_owner_ar]
//      ,[name_brand_en]
//      ,[name_brand_ar]
//      ,[brand_desc_en]
//      ,[brand_desc_ar]
//      ,[business_sector_id]
//      ,[phone]
//      ,[mobile_whatsapp]
//      ,[reg_message_to_admin]
//      ,[icon_url]
//      ,[loc_lat]
//      ,[loc_lng]
//      ,[admin_approval_status]
//      ,[admin_approval_remarks]
//      ,[need_approval_to_add_prod]
//      ,[order_by]
//      ,[allow_delivery_within_km]
//      ,[created_by]
//      ,[created_on]
//      ,[updated_by]
//      ,[updated_on]
//      ,[is_active]
//  FROM  [dbo].[ecom_brand] where [email]='" + email + "' and brand_id='" + brand_id + "' and store_id='" + store_id + "'");
//                            if (dtExist.Rows.Count == 0)
//                            {
//                                result.status_msg = "Unauthorized";
//                                return result;
//                            }

//                            if (!clsecom.get_bool_from_yn(dtExist.Rows[0]["is_active"].ToString().ToLower()))
//                            {
//                                result.status_msg = "Your business account is deactivated. Please contact administrator(" + clsecom.GetConfig("ecom_email_support") + ")";
//                                return result;
//                            }

//                            userData.brand = new EcomBrandData();
//                            userData.brand.brand_id = dtExist.Rows[0]["brand_id"].ToString();
//                            userData.brand.store_id = dtExist.Rows[0]["store_id"].ToString();
//                            userData.brand.email = dtExist.Rows[0]["email"].ToString();
//                            userData.brand.name_owner_en = dtExist.Rows[0]["name_owner_en"].ToString();
//                            userData.brand.name_owner_ar = dtExist.Rows[0]["name_owner_ar"].ToString();
//                            userData.brand.name_brand_en = dtExist.Rows[0]["name_brand_en"].ToString();
//                            userData.brand.name_brand_ar = dtExist.Rows[0]["name_brand_ar"].ToString();
//                            userData.brand.brand_desc_en = dtExist.Rows[0]["brand_desc_en"].ToString();
//                            userData.brand.brand_desc_ar = dtExist.Rows[0]["brand_desc_ar"].ToString();
//                            userData.brand.business_sector_id = dtExist.Rows[0]["business_sector_id"].ToString();
//                            userData.brand.phone = dtExist.Rows[0]["phone"].ToString();
//                            userData.brand.mobile_whatsapp = dtExist.Rows[0]["mobile_whatsapp"].ToString();
//                            userData.brand.reg_message_to_admin = dtExist.Rows[0]["reg_message_to_admin"].ToString();
//                            userData.brand.icon_url = dtExist.Rows[0]["icon_url"].ToString();
//                            userData.brand.loc_lat = dtExist.Rows[0]["loc_lat"].ToString();
//                            userData.brand.loc_lng = dtExist.Rows[0]["loc_lng"].ToString();
//                            userData.brand.admin_approval_status = dtExist.Rows[0]["admin_approval_status"].ToString();
//                            userData.brand.admin_approval_remarks = dtExist.Rows[0]["admin_approval_remarks"].ToString();
//                            userData.brand.need_approval_to_add_prod = dtExist.Rows[0]["need_approval_to_add_prod"].ToString();
//                            userData.brand.order_by = dtExist.Rows[0]["order_by"].ToString();
//                            userData.brand.allow_delivery_within_km = dtExist.Rows[0]["allow_delivery_within_km"].ToString();
//                            userData.brand.created_by = dtExist.Rows[0]["created_by"].ToString();
//                            userData.brand.created_on = dtExist.Rows[0]["created_on"].ToString();
//                            userData.brand.updated_by = dtExist.Rows[0]["updated_by"].ToString();
//                            userData.brand.updated_on = dtExist.Rows[0]["updated_on"].ToString();
//                            userData.brand.is_active = dtExist.Rows[0]["is_active"].ToString();


//                            if (dtExist.Rows[0]["admin_approval_status"].ToString().ToLower() == "rejected")
//                            {
//                                result.status_msg = "Your account Approval Status is:" + clsecom.ToTitleCase(dtExist.Rows[0]["admin_approval_status"].ToString().ToLower()) + ". Please contact administrator(" + clsecom.GetConfig("ecom_email_support") + " for further steps)";
//                                result.data = Newtonsoft.Json.JsonConvert.SerializeObject(userData);
//                                result.status = "SUCCESS";
//                                return result;
//                            }
//                            if (dtExist.Rows[0]["admin_approval_status"].ToString().ToLower() == "pending_approval" ||
//                                dtExist.Rows[0]["admin_approval_status"].ToString().ToLower() == "on_hold")
//                            {
//                                string stat = dtExist.Rows[0]["admin_approval_status"].ToString().ToLower().Replace("_", " ");
//                                result.status_msg = "Your account Approval Status is:" + clsecom.ToTitleCase(stat) + ". Please wait for 48 business hours. You will get email notification after completing the Approval Process. If more delayed please contact administrator(" + clsecom.GetConfig("ecom_email_support") + ")";
//                                result.data = Newtonsoft.Json.JsonConvert.SerializeObject(userData);
//                                result.status = "SUCCESS";
//                                return result;
//                            }

//                            if (dtExist.Rows[0]["admin_approval_status"].ToString().ToLower() == "draft")
//                            {
//                                result.status_msg = "Please submit Business Account Form for approval. Please use Business Profile form in the App";
//                                result.data = Newtonsoft.Json.JsonConvert.SerializeObject(userData);
//                                result.status = "SUCCESS";
//                                return result;
//                            }
//                            if (dtExist.Rows[0]["admin_approval_status"].ToString().ToLower() != "approved")
//                            {
//                                string stat = dtExist.Rows[0]["admin_approval_status"].ToString().ToLower().Replace("_", " ");
//                                result.status_msg = "Your account Approval Status is:" + clsecom.ToTitleCase(stat) + ". Please contact administrator(" + clsecom.GetConfig("ecom_email_support") + ")";
//                                result.data = Newtonsoft.Json.JsonConvert.SerializeObject(userData);
//                                result.status = "SUCCESS";
//                                return result;
//                            }
//                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(userData);
//                            result.status_msg = "";
//                            result.token_new = clsecomauth.GetNewToken(dev_id);
//                            result.status = "SUCCESS";
//                            break;
//                        }
                    case "register_brand":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string res_stat = "";
                            if (!clsecom.can_register_brand(inData, store_id, out res_stat))
                            {
                                if (!clsecom.IsEmpty(res_stat))
                                    result.status_msg = res_stat;
                                return result;
                            }
                            SqlCommand comm = conn.GetCommandWithTransaction();
                            query = @"INSERT INTO [dbo].[ecom_brand]
           ([store_id]
           ,[email]
           ,[name_owner_en]
           ,[name_owner_ar] 
           ,[admin_approval_status] 
           ,[need_approval_to_add_prod] 
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + store_id + @"'
           ,'" + clsecom.get_value(inData.email) + @"'
           ,'" + clsecom.get_value(inData.name_owner_en) + @"' 
           ,N'" + clsecom.get_value(inData.name_owner_ar) + @"'   
           ,'draft' 
           ,'Y'   
           ,getdate() 
           ,'Y') SELECT SCOPE_IDENTITY()";
                            string brand_id = conn.ExecuteScalar(query, comm);
                            if (clsecom.IsEmpty(brand_id))
                            {
                                conn.RollBack(comm);
                                return result;
                            }
                            query = @"INSERT INTO [dbo].[ecom_staff]
           ([brand_id]
,[store_id]
           ,[role_type]
           ,[email] 
           ,[name_en]
           ,[name_ar] 
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + brand_id + @"'
,'" + store_id + @"'
           ,'business_owner'
           ,'" + clsecom.get_value(inData.email) + @"' 
          ,'" + clsecom.get_value(inData.name_owner_en) + @"'
           ,N'" + clsecom.get_value(inData.name_owner_ar) + @"' 
           ,getdate() 
           ,'Y')";
                            if (!conn.ExecuteNonQuery(query, comm))
                            {
                                conn.RollBack(comm);
                                return result;
                            }
                            conn.Commit(comm);
                            result.data = "";
                            result.status_msg = "Your account created. Please login and submit the Business Account Form for approval of Admin.";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "can_register_brand":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string res_stat = "";
                            if (!clsecom.can_register_brand(inData, store_id, out res_stat))
                            {
                                if (!clsecom.IsEmpty(res_stat))
                                    result.status_msg = res_stat;
                                return result;
                            }
                            result.data = "OK";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    //== 

                    case "init_app":
                        {
                            EcomAppData appdata = clsecom.getAppData(app_identity);
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(appdata);
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "update_device":
                        {
                            string new_token = clsecomauth.GetNewToken(dev_id);
                            if (!clsecom.setDevice(store_id, dev_id, cust_id, staff_id, role_type, dev_type, lang_code, token, reg_id, out res_msg))
                            {
                                if (!clsecom.IsEmpty(res_msg))
                                    result.status_msg = res_msg;
                                return result;
                            }
                            result.token_new = new_token;
                            result.status = "SUCCESS";
                            break;
                        }
                    case "login_staff":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string email = clsecom.get_value(inData.email);
                            if (clsecom.IsEmpty(email))
                            {
                                result.status_msg = "Email should not be empty";
                                return result;
                            }
                            if (auth.IsContainsSpecialChars(email))
                            {
                                result.status_msg = "Email should not contain special characters";
                                return result;
                            }
                            string is_email_verified = clsecom.get_value(inData.is_email_verified);
                            DataTable dtExist = conn.getDataTable(@"SELECT [staff_id]
      ,[brand_id]
      ,[store_id]
      ,[role_type]
      ,[email]
,[is_email_verified]
      ,[f_name]
      ,[l_name]
      ,[mobile_num]
      ,[wa_num]
      ,[address_en]
      ,[address_ar]
      ,[loc_lat]
      ,[loc_lng]
      ,[created_by]
      ,[created_on]
      ,[updated_by]
      ,[updated_on]
      ,[is_active]
,[is_firebase_account_created]
,[firebase_initial_pw]
  FROM  [dbo].[ecom_staff] where email='" + email + @"' and [store_id]='" + store_id + @"'");
                            if (dtExist.Rows.Count == 0)
                            {
                                result.status_msg = "Unauthorized";
                                return result;
                            }
                            if (!clsecom.get_bool_from_yn(dtExist.Rows[0]["is_active"].ToString().ToLower()))
                            {
                                result.status_msg = "This user account is deactivated";
                                return result;
                            }
                            if (!clsecom.IsEmpty(is_email_verified))
                            {
                                if (is_email_verified != dtExist.Rows[0]["is_email_verified"].ToString())
                                {
                                    conn.ExecuteNonQuery(@"UPDATE [dbo].[ecom_staff]
   SET [is_email_verified]='" + is_email_verified + @"'
 WHERE staff_id='" + dtExist.Rows[0]["staff_id"].ToString() + "'");
                                }
                            }
                            else
                                is_email_verified = dtExist.Rows[0]["is_email_verified"].ToString();
                            EcomUserData userData = new EcomUserData();
                            userData.user_type = dtExist.Rows[0]["role_type"].ToString();
                            userData.customer = new EcomCustomerData();
                            userData.brand = new EcomBrandData();
                            userData.staff = new EcomStaffData();
                            userData.staff.staff_id = dtExist.Rows[0]["staff_id"].ToString();
                            userData.staff.brand_id = dtExist.Rows[0]["brand_id"].ToString();
                            userData.staff.store_id = dtExist.Rows[0]["store_id"].ToString();
                            userData.staff.role_type = dtExist.Rows[0]["role_type"].ToString();
                            userData.staff.email = dtExist.Rows[0]["email"].ToString();
                            userData.staff.is_email_verified = is_email_verified;
                            userData.staff.f_name = dtExist.Rows[0]["f_name"].ToString();
                            userData.staff.l_name = dtExist.Rows[0]["l_name"].ToString();
                            userData.staff.mobile_num = dtExist.Rows[0]["mobile_num"].ToString();
                            userData.staff.wa_num = dtExist.Rows[0]["wa_num"].ToString();
                            userData.staff.address_en = dtExist.Rows[0]["address_en"].ToString();
                            userData.staff.address_ar = dtExist.Rows[0]["address_ar"].ToString();
                            userData.staff.loc_lat = dtExist.Rows[0]["loc_lat"].ToString();
                            userData.staff.loc_lng = dtExist.Rows[0]["loc_lng"].ToString();
                            userData.staff.created_by = dtExist.Rows[0]["created_by"].ToString();
                            userData.staff.created_on = dtExist.Rows[0]["created_on"].ToString();
                            userData.staff.updated_by = dtExist.Rows[0]["updated_by"].ToString();
                            userData.staff.updated_on = dtExist.Rows[0]["updated_on"].ToString();
                            userData.staff.is_active = dtExist.Rows[0]["is_active"].ToString();

                            userData.staff.is_firebase_account_created = dtExist.Rows[0]["is_firebase_account_created"].ToString();
                            userData.staff.firebase_initial_pw = dtExist.Rows[0]["firebase_initial_pw"].ToString();

                            if (dtExist.Rows[0]["role_type"].ToString().ToLower() == "admin")
                            {
                                result.data = Newtonsoft.Json.JsonConvert.SerializeObject(userData);
                                result.status_msg = "";
                                result.token_new = clsecomauth.GetNewToken(dev_id);
                                result.status = "SUCCESS";
                                return result;
                            }
                            string brand_id = dtExist.Rows[0]["brand_id"].ToString();
                            string brand_email = dtExist.Rows[0]["email"].ToString();
                            dtExist = conn.getDataTable(@"SELECT  [brand_id]
      ,[store_id]
      ,[email] 
 ,[f_name_owner]
      ,[l_name_owner]
      ,[business_address_line_1]
      ,[business_address_line_2]
      ,[business_address_city]
      ,[business_address_po_box_no]
      ,[business_address_postcode]
      ,[business_address_country]
      ,[name_brand_en]
      ,[name_brand_ar]
      ,[brand_desc_en]
      ,[brand_desc_ar]
      ,[business_sector_id]
      ,[phone]
      ,[mobile_whatsapp]
      ,[reg_message_to_admin]
      ,[icon_url]
      ,[loc_lat]
      ,[loc_lng]
      ,[admin_approval_status]
      ,[admin_approval_remarks]
      ,[need_approval_to_add_prod]
      ,[order_by]
      ,[allow_delivery_within_km]
      ,[created_by]
      ,[created_on]
      ,[updated_by]
      ,[updated_on]
      ,[is_active]
 ,[website]
 ,[social_media_url]
  FROM  [dbo].[ecom_brand] where [email]='" + email + "' and brand_id='" + brand_id + "' and store_id='" + store_id + "'");
                            if (dtExist.Rows.Count == 0)
                            {
                                result.status_msg = "Unauthorized";
                                return result;
                            }

                            if (!clsecom.get_bool_from_yn(dtExist.Rows[0]["is_active"].ToString().ToLower()))
                            {
                                result.status_msg = "Your business account is deactivated. Please contact administrator(" + clsecom.GetConfig("ecom_email_support") + ")";
                                return result;
                            }

                            userData.brand = new EcomBrandData();
                            userData.brand.brand_id = dtExist.Rows[0]["brand_id"].ToString();
                            userData.brand.store_id = dtExist.Rows[0]["store_id"].ToString();
                            userData.brand.email = dtExist.Rows[0]["email"].ToString();

                            userData.brand.f_name_owner = dtExist.Rows[0]["f_name_owner"].ToString();
                            userData.brand.l_name_owner = dtExist.Rows[0]["l_name_owner"].ToString();
                            userData.brand.business_address_line_1 = dtExist.Rows[0]["business_address_line_1"].ToString();
                            userData.brand.business_address_line_2 = dtExist.Rows[0]["business_address_line_2"].ToString();
                            userData.brand.business_address_city = dtExist.Rows[0]["business_address_city"].ToString();
                            userData.brand.business_address_po_box_no = dtExist.Rows[0]["business_address_po_box_no"].ToString();
                            userData.brand.business_address_postcode = dtExist.Rows[0]["business_address_postcode"].ToString();
                            userData.brand.business_address_country = dtExist.Rows[0]["business_address_country"].ToString();

                            userData.brand.name_brand_en = dtExist.Rows[0]["name_brand_en"].ToString();
                            userData.brand.name_brand_ar = dtExist.Rows[0]["name_brand_ar"].ToString();
                            userData.brand.brand_desc_en = dtExist.Rows[0]["brand_desc_en"].ToString();
                            userData.brand.brand_desc_ar = dtExist.Rows[0]["brand_desc_ar"].ToString();
                            userData.brand.business_sector_id = dtExist.Rows[0]["business_sector_id"].ToString();
                            userData.brand.phone = dtExist.Rows[0]["phone"].ToString();
                            userData.brand.mobile_whatsapp = dtExist.Rows[0]["mobile_whatsapp"].ToString();
                            userData.brand.reg_message_to_admin = dtExist.Rows[0]["reg_message_to_admin"].ToString();
                            userData.brand.icon_url = dtExist.Rows[0]["icon_url"].ToString();
                            userData.brand.loc_lat = dtExist.Rows[0]["loc_lat"].ToString();
                            userData.brand.loc_lng = dtExist.Rows[0]["loc_lng"].ToString();
                            userData.brand.admin_approval_status = dtExist.Rows[0]["admin_approval_status"].ToString();
                            userData.brand.admin_approval_remarks = dtExist.Rows[0]["admin_approval_remarks"].ToString();
                            userData.brand.need_approval_to_add_prod = dtExist.Rows[0]["need_approval_to_add_prod"].ToString();
                            userData.brand.order_by = dtExist.Rows[0]["order_by"].ToString();
                            userData.brand.allow_delivery_within_km = dtExist.Rows[0]["allow_delivery_within_km"].ToString();
                            userData.brand.created_by = dtExist.Rows[0]["created_by"].ToString();
                            userData.brand.created_on = dtExist.Rows[0]["created_on"].ToString();
                            userData.brand.updated_by = dtExist.Rows[0]["updated_by"].ToString();
                            userData.brand.updated_on = dtExist.Rows[0]["updated_on"].ToString();
                            userData.brand.is_active = dtExist.Rows[0]["is_active"].ToString();
                            userData.brand.website = dtExist.Rows[0]["website"].ToString();
                            userData.brand.social_media_url = dtExist.Rows[0]["social_media_url"].ToString();

                            if (dtExist.Rows[0]["admin_approval_status"].ToString().ToLower() == "rejected")
                            {
                                //result.status_msg = "Your account Approval Status is:" + clsecom.ToTitleCase(dtExist.Rows[0]["admin_approval_status"].ToString().ToLower()) + ". Please contact administrator (" + clsecom.GetConfig("ecom_email_support").Trim() + " for further steps)";
                                result.status_msg = "Your account Approval Status is:" + clsecom.ToTitleCase(dtExist.Rows[0]["admin_approval_status"].ToString().ToLower()) + ". You can submit Business Account Form again for approval. Please use Business Profile form in the App";
                                result.data = Newtonsoft.Json.JsonConvert.SerializeObject(userData);
                                result.status = "SUCCESS";
                                return result;
                            }
                            if (dtExist.Rows[0]["admin_approval_status"].ToString().ToLower() == "pending_approval" ||
                                dtExist.Rows[0]["admin_approval_status"].ToString().ToLower() == "on_hold")
                            {
                                string stat = dtExist.Rows[0]["admin_approval_status"].ToString().ToLower().Replace("_", " ");
                                result.status_msg = "Your account Approval Status is:" + clsecom.ToTitleCase(stat) + ". Please wait for 48 business hours. You will get email notification after completing the Approval Process. If more delayed please contact administrator (" + clsecom.GetConfig("ecom_email_support").Trim() + ")";
                                result.data = Newtonsoft.Json.JsonConvert.SerializeObject(userData);
                                result.status = "SUCCESS";
                                return result;
                            }

                            if (dtExist.Rows[0]["admin_approval_status"].ToString().ToLower() == "draft")
                            {
                                result.status_msg = "Please submit Business Account Form for approval. Please use Business Profile form in the App";
                                result.data = Newtonsoft.Json.JsonConvert.SerializeObject(userData);
                                result.status = "SUCCESS";
                                return result;
                            }
                            if (dtExist.Rows[0]["admin_approval_status"].ToString().ToLower() != "approved")
                            {
                                string stat = dtExist.Rows[0]["admin_approval_status"].ToString().ToLower().Replace("_", " ");
                                result.status_msg = "Your account Approval Status is:" + clsecom.ToTitleCase(stat) + ". Please contact administrator (" + clsecom.GetConfig("ecom_email_support").Trim() + ")";
                                result.data = Newtonsoft.Json.JsonConvert.SerializeObject(userData);
                                result.status = "SUCCESS";
                                return result;
                            }
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(userData);
                            result.status_msg = "";
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                    case "get_user_type_for_exists":
                        {
                            if (clsecom.IsEmpty(token))
                            {
                                clsecom.Log(log_key, "Received without token.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            string email = clsecom.get_value(inData.email);
                            if (auth.IsContainsSpecialChars(email))
                            {
                                result.status_msg = "Email should not contain special characters";
                                return result;
                            }
                            if (clsecom.IsEmpty(store_id))
                            {
                                clsecom.Log(log_key, "Received without store_id.(Input:" + clsecom.getJsonObjectToString(input) + ")", true, null, true);
                                return result;
                            }
                            query = @"SELECT  role_type
  FROM  [dbo].[ecom_staff]  where store_id='" + store_id + @"' and [email]='" + email + @"'";
                            DataTable dtData = conn.getDataTable(query);
                            string user_type = "";
                            if (dtData.Rows.Count > 0)
                            {
                                if (dtData.Rows[0]["role_type"].ToString().Trim().Length > 0)
                                    user_type = dtData.Rows[0]["role_type"].ToString().Trim();
                                else
                                    user_type = "business_user";
                            }
                            else
                            {
                                query = @"SELECT   [cust_id] FROM [dbo].[ecom_customer]  where 
store_id='" + store_id + @"' and [email]='" + email + @"'";
                                dtData = conn.getDataTable(query);
                                if (dtData.Rows.Count > 0)
                                    user_type = "customer";
                            }
                            if (clsecom.IsEmpty(user_type))
                                user_type = "not_exists";
                            result.data = Newtonsoft.Json.JsonConvert.SerializeObject(user_type);
                            result.token_new = clsecomauth.GetNewToken(dev_id);
                            result.status = "SUCCESS";
                            break;
                        }
                }
            }
            catch (Exception exp)
            {
                clsecom.Log(log_key, "Exception occured. " + exp.Message + "(Input:" + clsecom.getJsonObjectToString(input) + ")", true, exp, true);
            }
            finally
            {
                if (clsecom.GetConfig("need_return_err").ToLower().Trim() == "y")
                {
                    if (clsEcom.err.Trim().Length > 0)
                        result.status_msg = clsEcom.err.Trim();
                }
            }
            return result;
        }

    }
}