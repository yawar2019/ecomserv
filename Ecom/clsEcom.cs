using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.Text;
using System.Web;

namespace ecomserv.Ecom
{
    public class clsEcom
    {
        public static string err = "";
        public EcomStaffHomeItem get_staff_home(string store_id, out string res_msg)
        {
            res_msg = "";
            EcomStaffHomeItem res = new EcomStaffHomeItem();
            clsConnectionSQL conn = new clsConnectionSQL();
            string query = @"SELECT  isnull(count(o.[order_id]),0) 
  FROM  [dbo].[ecom_order] o 
  inner join [dbo].[ecom_customer] c
  on c.store_id='"+ store_id + @"' and
  c.cust_id=o.cust_id 
where isnull(o.is_active,'')='Y' and o.[current_status_key]='processing'";
            res.total_processing = conn.ExecuteScalar(query);

            query = @"SELECT  isnull(count(o.[order_id]),0) 
  FROM  [dbo].[ecom_order] o 
  inner join [dbo].[ecom_customer] c 
  on c.store_id='" + store_id + @"' and 
  c.cust_id=o.cust_id 
where isnull(o.is_active,'')='Y' and 
o.[current_status_key]='ready_to_deliver'";
            res.total_ready_to_deliver = conn.ExecuteScalar(query);

            query = @"SELECT  isnull(count(o.[order_id]),0) 
  FROM  [dbo].[ecom_order] o 
  inner join [dbo].[ecom_customer] c
  on c.store_id='" + store_id + @"' and
  c.cust_id=o.cust_id 
where isnull(o.is_active,'')='Y' and 
(o.[current_status_key]='processing' or o.[current_status_key]='ready_to_deliver')";
            res.total_pending = conn.ExecuteScalar(query);
            return res;
        }

        public string convert_client_time_to_utc(string client_time_zone_offset, string client_time)
        {
            int client_time_zone_offset_int = 0;
            DateTime client_time_dt = DateTime.UtcNow;
            if (int.TryParse(client_time_zone_offset, out client_time_zone_offset_int))
            {
                if (DateTime.TryParse(client_time, out client_time_dt))
                {
                    client_time_dt = client_time_dt.AddMinutes(client_time_zone_offset_int);
                    return client_time_dt.ToString("dd-MMM-yyyy HH:mm:ss");
                }
                else
                {
                    //date conversion wrong
                }
            }
            else
            {
                //offset wrong
            }
            return client_time;
        }
        public string convert_utc_to_client_time(string client_time_zone_offset, string utc_time)
        {
            int client_time_zone_offset_int = 0;
            DateTime utc_time_dt = DateTime.UtcNow;
            if (int.TryParse(client_time_zone_offset, out client_time_zone_offset_int))
            {
                if (DateTime.TryParse(utc_time, out utc_time_dt))
                {
                    utc_time_dt = utc_time_dt.AddMinutes(-client_time_zone_offset_int);
                    return utc_time_dt.ToString("dd-MMM-yyyy HH:mm:ss");
                }
                else
                {
                    //date conversion wrong
                }
            }
            else
            {
                //offset wrong
            }
            return utc_time;
        }

        public EcomSalesItem get_sales(string day_key, string date_from, string date_to, string store_id, out string res_msg)
        {
            res_msg = "";
            EcomSalesItem res = new EcomSalesItem();
            clsConnectionSQL conn = new clsConnectionSQL();
            string query = "";
            DateTime date_from_dt = DateTime.Now;
            DateTime date_to_dt = DateTime.Now;
            switch (day_key)
            {
                case "today":
                    {
                        date_to_dt = DateTime.Now;
                        date_from_dt = new DateTime(date_to_dt.Year, date_to_dt.Month, date_to_dt.Day, 0, 0, 0);
                        break;
                    }
                case "yesterday":
                    {
                        date_to_dt = DateTime.Now.AddDays(-1);
                        date_to_dt = new DateTime(date_to_dt.Year, date_to_dt.Month, date_to_dt.Day, 23, 59, 59);
                        date_from_dt = new DateTime(date_to_dt.Year, date_to_dt.Month, date_to_dt.Day, 0, 0, 0);
                        break;
                    }
                case "last_7_days":
                    {
                        date_to_dt = DateTime.Now;
                        date_from_dt = date_to_dt.AddDays(-7);
                        date_from_dt = new DateTime(date_from_dt.Year, date_from_dt.Month, date_from_dt.Day, 0, 0, 0);
                        break;
                    }
                case "this_month":
                    {
                        date_to_dt = DateTime.Now;
                        date_from_dt = new DateTime(date_to_dt.Year, date_to_dt.Month, 1, 0, 0, 0);
                        break;
                    }
                case "last_month":
                    {
                        date_to_dt = DateTime.Now.AddMonths(-1);
                        date_to_dt = new DateTime(date_to_dt.Year, date_to_dt.Month, DateTime.DaysInMonth(date_to_dt.Year, date_to_dt.Month), 23, 59, 59);
                        date_from_dt = new DateTime(date_to_dt.Year, date_to_dt.Month, 1, 0, 0, 0);
                        break;
                    }
                case "last_30_days_month":
                    {
                        date_to_dt = DateTime.Now;
                        date_from_dt = date_to_dt.AddDays(-30);
                        date_from_dt = new DateTime(date_from_dt.Year, date_from_dt.Month, date_from_dt.Day, 0, 0, 0);
                        break;
                    }
                case "this_year":
                    {
                        date_to_dt = DateTime.Now;
                        date_from_dt = new DateTime(date_from_dt.Year, 1, 1, 0, 0, 0);
                        break;
                    }
                case "last_year":
                    {
                        date_to_dt = DateTime.Now;
                        date_to_dt = date_to_dt.AddYears(-1);
                        date_to_dt = new DateTime(date_to_dt.Year, 12, DateTime.DaysInMonth(date_to_dt.Year, 12), 23, 59, 59);
                        date_from_dt = new DateTime(date_to_dt.Year, 1, 1, 0, 0, 0);
                        break;
                    }
                case "custom":
                    {
                        if (!DateTime.TryParse(date_from, out date_from_dt))
                        {
                            res_msg = "Invalid From Date";
                            return res;
                        }
                        if (!DateTime.TryParse(date_to, out date_to_dt))
                        {
                            res_msg = "Invalid To Date";
                            return res;
                        }
                        break;
                    }
            }
            res.from_date = date_from_dt.ToString("dd-MMM-yyyy hh:mm tt");
            res.to_date = date_to_dt.ToString("dd-MMM-yyyy hh:mm tt");
            string condition = "";
            if (day_key != "all")
            {
                condition = @" and  [order_date] between cast('" + res.from_date + @"' as datetime) 
and cast('" + res.to_date + @"' as datetime)";
            }
            query = @"select count(*) FROM  [dbo].[ecom_order] 
where isnull(is_active,'')='Y' " + condition;
            res.total_orders_count = conn.ExecuteScalar(query);

            query = @"SELECT isnull(sum([tot_price]),0)
  FROM  [dbo].[ecom_order_product]
  where isnull(is_active,'')='Y' and 
  [order_id] in (select [order_id]  FROM  [dbo].[ecom_order] 
where isnull(is_active,'')='Y' and isnull([is_paid],'')='Y' and isnull([current_status_key],'')!='cancelled' " + condition + @")";
            res.total_sales_items_amount = conn.ExecuteScalar(query);

            query = @"SELECT isnull(count([tot_price]),0)
  FROM  [dbo].[ecom_order_product]
  where isnull(is_active,'')='Y' and 
  [order_id] in (select [order_id]  FROM  [dbo].[ecom_order] 
where isnull(is_active,'')='Y' and isnull([is_paid],'')='Y' and isnull([current_status_key],'')!='cancelled' " + condition + @")";
            res.total_sales_items_count = conn.ExecuteScalar(query);

            query = @"SELECT isnull(count([order_id]),0)
  FROM  [dbo].[ecom_order]
  where isnull(is_active,'')='Y' and  isnull([current_status_key],'')='cancelled' " + condition + @"";
            res.total_canceled_orders_count = conn.ExecuteScalar(query);
            return res;
        }
        public bool is_fk_issue(string exp_msg)
        {
            if (IsEmpty(exp_msg))
                return false;
            if (exp_msg.ToString().ToLower().Contains("The DELETE statement conflicted with the REFERENCE constraint".ToLower()))
                return true;
            return false;
        }
        public List<EcomOrderItem> getOrders(string store_id, string last_idx, string max_in_a_call, string condition_type, string paid_type, string current_status_key, string prod_id, string cust_id, string order_id, out string res_status)
        {
            string log_key = "getOrders";
            res_status = "";
            List<EcomOrderItem> res = new List<EcomOrderItem>();
            string query = "";
            try
            {
                clsConnectionSQL conn = new clsConnectionSQL(); 
                string condition_fileds = "";
                if (condition_type == "new_to_old")
                    condition_fileds = " o.[order_date] DESC ";
                else if (condition_type == "old_to_new")
                    condition_fileds = " o.[order_date] ASC ";
                else if (condition_type == "none")
                    condition_fileds = " o.[order_date] ";
                else
                    condition_fileds = " o.[order_date] ";

                string extra_fr_paid = "";
                if (paid_type == "paid")
                    extra_fr_paid = " and isnull(o.[is_paid],'')='Y' ";
                else if (paid_type == "not_paid")
                    extra_fr_paid = " and isnull(o.[is_paid],'')!='Y' ";

                string extra_for_prod = "";
                if (!IsEmpty(prod_id))
                {
                    extra_for_prod = @" and o.prod_id in (select prod_id from [dbo].[ecom_order_product] where 
  cust_id='"+cust_id+@"' and isnull(is_active,'')='Y' ) ";
                }

                query = @"SELECT
a.[order_id]
      ,a.[cust_id]
      ,a.[txn_id]
      ,a.[staff_id_updated_by]
      ,a.[tot_price]
      ,convert(char(20),a.[order_date],113) as order_date
      ,a.[order_type]
      ,a.[is_req_for_cancel_by_customer]
      ,a.[current_status_key]
      ,a.[is_paid]
      ,a.[delivery_name] 
      ,a.[delivery_address]
      ,a.[delivery_state_region]
      ,a.[delivery_city]
      ,a.[delivery_zip]
      ,a.[delivery_country]
      ,a.[delivery_landmark]
      ,a.[delivery_mobile_num]
      ,a.[delivery_wa_num]
      ,a.[delivery_email]
      ,a.[delivery_loc_lat]
      ,a.[delivery_loc_lng]
      ,a.[delivery_instruction]
      ,a.[delivery_time_preference]
      ,a.[billing_name] 
      ,a.[billing_address]
      ,a.[billing_state_region]
      ,a.[billing_city]
      ,a.[billing_zip]
      ,a.[billing_country]
      ,a.[created_by]
      ,a.[created_on]
      ,a.[updated_by]
      ,a.[updated_on]
      ,a.[is_active] 
,RowNum
  FROM (SELECT ROW_NUMBER() OVER (ORDER BY " + condition_fileds + @") As RowNum, o.* 
   FROM [dbo].[ecom_order] o 
 inner join [dbo].[ecom_customer] c
  on c.store_id='"+store_id+@"' and
  c.cust_id=o.cust_id 
 where ISNULL(o.[is_active],'')='Y' 
and o.[cust_id]= case when '" + cust_id + @"'='' then o.[cust_id] else  '" + cust_id + @"' end
and o.[order_id]= case when '" + order_id + @"'='' then o.[order_id] else  '" + order_id + @"' end
and o.[current_status_key]= case when '" + current_status_key + @"'='' then o.[current_status_key] else  '" + current_status_key + @"' end
 
" + extra_fr_paid + @"  
" + extra_for_prod + @" 
) As a  WHERE RowNum 
BETWEEN 1+(" + last_idx + @") AND (" + last_idx + @"+" + max_in_a_call + @")";

                DataTable dtData = conn.getDataTable(query);
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    EcomOrderItem order = new EcomOrderItem();

                    order.order_id = dtData.Rows[i]["order_id"].ToString().Trim();
                    order.cust_id = dtData.Rows[i]["cust_id"].ToString().Trim();
                    order.txn_id = dtData.Rows[i]["txn_id"].ToString().Trim();
                    order.staff_id_updated_by = dtData.Rows[i]["staff_id_updated_by"].ToString().Trim();
                    order.tot_price = dtData.Rows[i]["tot_price"].ToString().Trim();
                    order.order_date = dtData.Rows[i]["order_date"].ToString().Trim();
                    order.order_type = dtData.Rows[i]["order_type"].ToString().Trim();
                    order.is_req_for_cancel_by_customer = dtData.Rows[i]["is_req_for_cancel_by_customer"].ToString().Trim();
                    order.current_status_key = dtData.Rows[i]["current_status_key"].ToString().Trim();
                    order.is_paid = dtData.Rows[i]["is_paid"].ToString().Trim();
                    order.staff_id = dtData.Rows[i]["staff_id"].ToString().Trim();
                    order.last_row_index = dtData.Rows[i]["RowNum"].ToString().Trim();
                      
                    order.delivery_address = new EcomAddressItem();
                    order.delivery_address.name = dtData.Rows[i]["delivery_name"].ToString().Trim(); 
                    order.delivery_address.address = dtData.Rows[i]["delivery_address"].ToString().Trim();
                    order.delivery_address.state_region = dtData.Rows[i]["delivery_state_region"].ToString().Trim();
                    order.delivery_address.city = dtData.Rows[i]["delivery_city"].ToString().Trim();
                    order.delivery_address.zip = dtData.Rows[i]["delivery_zip"].ToString().Trim();
                    order.delivery_address.country = dtData.Rows[i]["delivery_country"].ToString().Trim();
                    order.delivery_address.landmark = dtData.Rows[i]["delivery_landmark"].ToString().Trim();
                    order.delivery_address.mobile_num = dtData.Rows[i]["delivery_mobile_num"].ToString().Trim();
                    order.delivery_address.wa_num = dtData.Rows[i]["delivery_wa_num"].ToString().Trim();
                    order.delivery_address.email = dtData.Rows[i]["delivery_email"].ToString().Trim();
                    order.delivery_address.loc_lat = dtData.Rows[i]["delivery_loc_lat"].ToString().Trim();
                    order.delivery_address.loc_lng = dtData.Rows[i]["delivery_loc_lng"].ToString().Trim();
                    order.delivery_address.instruction = dtData.Rows[i]["delivery_instruction"].ToString().Trim();
                    order.delivery_address.time_preference = dtData.Rows[i]["delivery_time_preference"].ToString().Trim();
                    order.billing_address = new EcomAddressItem();
                    order.billing_address.name = dtData.Rows[i]["billing_name"].ToString(); 
                    order.billing_address.address = dtData.Rows[i]["billing_address"].ToString();
                    order.billing_address.state_region = dtData.Rows[i]["billing_state_region"].ToString();
                    order.billing_address.city = dtData.Rows[i]["billing_city"].ToString();
                    order.billing_address.zip = dtData.Rows[i]["billing_zip"].ToString();
                    order.billing_address.country = dtData.Rows[i]["billing_country"].ToString();
                    order.product_order = new List<EcomProductOrderItem>();
                    query = @"SELECT [prod_order_id]
      ,[order_id]
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
      ,[updated_by]
      ,[updated_on]
      ,[is_active]
  FROM [dbo].[ecom_order_product] where [order_id]='" + order.order_id + @"' and isnull([is_active],'')='Y'";
                    DataTable dtOrd = conn.getDataTable(query);
                    for (int j = 0; j < dtOrd.Rows.Count; j++)
                    {
                        EcomProductOrderItem prod_ord = new EcomProductOrderItem();

                        prod_ord.prod_order_id = dtOrd.Rows[j]["prod_order_id"].ToString();
                        prod_ord.order_id = dtOrd.Rows[j]["order_id"].ToString();
                        prod_ord.prod_id = dtOrd.Rows[j]["prod_id"].ToString();
                        prod_ord.unit_price = dtOrd.Rows[j]["unit_price"].ToString();
                        prod_ord.discount_perc = dtOrd.Rows[j]["discount_perc"].ToString();
                        prod_ord.custome_size_extra_price = dtOrd.Rows[j]["custome_size_extra_price"].ToString();
                        prod_ord.custome_size_extra_price_perc = dtOrd.Rows[j]["custome_size_extra_price_perc"].ToString();
                        prod_ord.gift_extra_price = dtOrd.Rows[j]["gift_extra_price"].ToString();
                        prod_ord.gift_extra_price_perc = dtOrd.Rows[j]["gift_extra_price_perc"].ToString();
                        prod_ord.surprise_gift_extra_price = dtOrd.Rows[j]["surprise_gift_extra_price"].ToString();
                        prod_ord.surprise_gift_extra_price_perc = dtOrd.Rows[j]["surprise_gift_extra_price_perc"].ToString();
                        prod_ord.wish_card_extra_price = dtOrd.Rows[j]["wish_card_extra_price"].ToString();
                        prod_ord.wish_card_extra_price_perc = dtOrd.Rows[j]["wish_card_extra_price_perc"].ToString();
                        prod_ord.quantity = dtOrd.Rows[j]["quantity"].ToString();
                        prod_ord.product_color_id = dtOrd.Rows[j]["product_color_id"].ToString();
                        prod_ord.product_size_id = dtOrd.Rows[j]["product_size_id"].ToString();
                        prod_ord.is_custom_size = dtOrd.Rows[j]["is_custom_size"].ToString();
                        prod_ord.custom_size_field1_value = dtOrd.Rows[j]["custom_size_field1_value"].ToString();
                        prod_ord.custom_size_field2_value = dtOrd.Rows[j]["custom_size_field2_value"].ToString();
                        prod_ord.custom_size_field3_value = dtOrd.Rows[j]["custom_size_field3_value"].ToString();
                        prod_ord.custom_size_field4_value = dtOrd.Rows[j]["custom_size_field4_value"].ToString();
                        prod_ord.custom_size_field5_value = dtOrd.Rows[j]["custom_size_field5_value"].ToString();
                        prod_ord.custom_size_field6_value = dtOrd.Rows[j]["custom_size_field6_value"].ToString();
                        prod_ord.custom_size_field7_value = dtOrd.Rows[j]["custom_size_field7_value"].ToString();
                        prod_ord.custom_size_field8_value = dtOrd.Rows[j]["custom_size_field8_value"].ToString();
                        prod_ord.custom_size_field9_value = dtOrd.Rows[j]["custom_size_field9_value"].ToString();
                        prod_ord.custom_size_field10_value = dtOrd.Rows[j]["custom_size_field10_value"].ToString();
                        prod_ord.custom_size_color_code = dtOrd.Rows[j]["custom_size_color_code"].ToString();
                        prod_ord.custom_size_message = dtOrd.Rows[j]["custom_size_message"].ToString();
                        prod_ord.is_gift = dtOrd.Rows[j]["is_gift"].ToString();
                        prod_ord.is_surprise_gift = dtOrd.Rows[j]["is_surprise_gift"].ToString();
                        prod_ord.is_need_gift_wish_card = dtOrd.Rows[j]["is_need_gift_wish_card"].ToString();
                        prod_ord.gift_wish_card_url = dtOrd.Rows[j]["gift_wish_card_url"].ToString();
                        prod_ord.created_by = dtOrd.Rows[j]["created_by"].ToString();
                        prod_ord.created_on = dtOrd.Rows[j]["created_on"].ToString();
                        prod_ord.updated_by = dtOrd.Rows[j]["updated_by"].ToString();
                        prod_ord.updated_on = dtOrd.Rows[j]["updated_on"].ToString();
                        prod_ord.is_active = dtOrd.Rows[j]["is_active"].ToString();
                        string stat_res = "";
                        prod_ord.product = getProductsSingle(prod_ord.prod_id, order.cust_id, out stat_res);
                        order.product_order.Add(prod_ord);
                    }
                    res.Add(order);
                }
            }
            catch (Exception exp)
            {
                Log(log_key, "Exception occured. " + exp.Message, true, exp, true);
            }
            return res;
        }
        public string EcomSavePicAndGetFileName(string base64data, out bool is_path)
        {
            string log_key = "EcomSavePicAndGetFileName";
            string pic_url = "";
            is_path = false;
            string file_name = "";
            try
            {
                if (base64data.Trim().Length == 0)
                {
                    return "";
                }
                string fname = get_file_name_if_path(base64data);
                if (fname.Trim().Length > 0)
                {
                    is_path = true;
                    return fname;
                }
                byte[] bytes = Convert.FromBase64String(get_base64_without_tail(base64data));
                Image img = null;
                MemoryStream ms = null;
                using (ms = new MemoryStream(bytes))
                {
                    img = Image.FromStream(ms);
                    file_name = GetUniqNo() + ".jpeg";
                    pic_url = get_asset_folder() + file_name;
                    img.Save(pic_url, ImageFormat.Jpeg);
                }
                kill_memmory_stream(ms);
                kill_image(img);
                return file_name;
            }
            catch (Exception exp)
            {
                Log(log_key, exp.Message + "(pic_url=" + pic_url + ",base64data=" + base64data + ")", true, exp);
            }
            return "";
        }
        public string EcomSavePicAndGetFileName(string base64data, bool is_gif, out bool is_path)
        {
            string log_key = "EcomSavePicAndGetFileName";
            string pic_url = "";
            is_path = false;
            string file_name = "";
            try
            {
                if (base64data.Trim().Length == 0)
                {
                    return "";
                }
                string fname = get_file_name_if_path(base64data);
                if (fname.Trim().Length > 0)
                {
                    is_path = true;
                    return fname;
                }
                string base64data_without_tail = get_base64_without_tail(base64data);
                byte[] bytes = null;
                if (is_gif)
                {
                    HashSet<char> whiteSpace = new HashSet<char> { '\t', '\n', '\r', ' ' };
                    int length = base64data_without_tail.Count(c => !whiteSpace.Contains(c));
                    if (length % 4 != 0)
                        base64data_without_tail += new string('=', 4 - length % 4); // Pad length to multiple of 4.
                    bytes = Convert.FromBase64String(base64data_without_tail);
                }
                else
                    bytes = Convert.FromBase64String(base64data_without_tail);
                Image img = null;
                MemoryStream ms = null;
                using (ms = new MemoryStream(bytes))
                {
                    img = Image.FromStream(ms);
                    if (is_gif)
                        file_name = GetUniqNo() + ".gif";
                    else
                        file_name = GetUniqNo() + ".jpeg";
                    pic_url = get_asset_folder() + file_name;
                    if (is_gif)
                        img.Save(pic_url, ImageFormat.Gif);
                    else
                        img.Save(pic_url, ImageFormat.Jpeg);
                }
                kill_memmory_stream(ms);
                kill_image(img);
                return file_name;
            }
            catch (Exception exp)
            {
                Log(log_key, exp.Message + "(pic_url=" + pic_url + ",base64data=" + base64data + ")", true, exp);
            }
            return "";
        }
        public string get_date_to_normal(string iso)
        {
            return get_date_to_normal(iso, "dd MMM yyyy HH:mm");
        }
        public string get_date_to_normal(string iso, string out_format)
        {
            string res = "";
            if (iso != null)
            {
                if (iso.ToString().Trim().Length > 0)
                {
                    if (iso.ToString().Trim().Contains("T"))
                    {
                        DateTime d = DateTime.Now;
                        if (DateTime.TryParseExact(iso, @"yyyy-MM-dd\THH:mm:ss.fff\Z", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out d))
                        {
                            return d.ToString(out_format);
                        }
                        DateTimeOffset dto = DateTimeOffset.Parse(iso);
                        DateTime dtObject = dto.DateTime;
                        return dtObject.ToString(out_format);
                    }
                }
            }
            if (res == "")
            {
                if (iso.Trim().Contains("-"))
                {
                    string yr = iso.Split('-')[0].ToString().Trim();
                    if (yr.Length == 4)
                    {
                        DateTime date_dt = DateTime.Now;
                        if (GetDate(iso, "yyyy-mm-dd", out date_dt))
                        {
                            return date_dt.ToString(out_format);
                        }
                    }
                }
            }
            if (res == "")
            {
                DateTime date_dt = DateTime.Now;
                if (DateTime.TryParse(iso, out date_dt))
                {
                    return date_dt.ToString(out_format);
                }
            }
            return res;
        }
        public bool GetDate(string date, string formate, out DateTime dateNew)
        {
            dateNew = DateTime.Now;
            if (DateTime.TryParseExact(date, formate, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateNew))
                return true;
            return false;
        }
        public void kill_image(Image img)
        {
            try
            {
                img.Dispose();
            }
            catch
            {

            }
            try
            {
                img = null;
            }
            catch
            {

            }
            clear_gc();
        }
        public void clear_gc()
        {
            try
            {
                GC.Collect();
            }
            catch
            {

            }
            try
            {
                GC.WaitForPendingFinalizers();
            }
            catch
            {

            }
        }
        public void kill_memmory_stream(MemoryStream mem)
        {
            try
            {
                mem.Close();
            }
            catch
            {

            }
            try
            {
                mem.Dispose();
            }
            catch
            {

            }
            try
            {
                mem = null;
            }
            catch
            {

            }
            clear_gc();
        }
        public string get_asset_folder()
        {
            string folder_path = "";
            string log_key = "get_asset_folder";
            try
            {
                clsCommon common = new clsCommon();
                folder_path = System.Web.Hosting.HostingEnvironment.MapPath(common.GetConfig("ecom_asset_path"));
                if (!Directory.Exists(folder_path))
                    Directory.CreateDirectory(folder_path); 
                if (folder_path[folder_path.Length - 1].ToString().Trim() != "/")
                    folder_path += "/";
            }
            catch (Exception exp)
            {
                Log(log_key, exp.Message, true, exp);
            }
            return folder_path;
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
                res += "" + _rdm.Next(_min, _max).ToString();
            }
            catch (Exception exp)
            {
                Log("GetUniqNo", exp.Message, true, exp);
            }
            return res;
        }
        private string get_base64_without_tail(string base64)
        {
            string res = "";
            if (base64.Contains(","))
                return base64.Split(',')[1];
            return res;
        }
        public string get_file_name_if_path(string data)
        {
            if (data == null)
                return "";
            if (data.Contains("ecomserv.aadhisoft.com"))
            {
                string[] splt = data.Split('/');
                return splt[splt.Length - 1];
            }
            if (data.ToLower().Contains(".jpeg") ||
                data.ToLower().Contains(".jpg") ||
                data.ToLower().Contains(".png") ||
                data.ToLower().Contains(".mov") ||
                data.ToLower().Contains(".mp4"))
                return data;
            return "";
        }
        public string EcomSaveVideoAndGetFileName(string base64data, out bool is_path)
        {
            string log_key = "EcomSaveVideoAndGetFileName";
            string video_url = "";
            string file_name = "";
            is_path = false;
            try
            {
                if (base64data.Trim().Length == 0)
                    return "";
                string fname = get_file_name_if_path(base64data);
                if (fname.Trim().Length > 0)
                {
                    is_path = true;
                    return fname;
                }

                byte[] bytes = Convert.FromBase64String(get_base64_without_tail(base64data));
                FileStream file = null;
                MemoryStream ms = null;
                using (ms = new MemoryStream(bytes))
                {
                    file_name = GetUniqNo() + ".mov";
                    video_url = get_asset_folder() + file_name;
                    file = new FileStream(video_url, FileMode.Create, FileAccess.Write);
                    ms.WriteTo(file);
                }
                kill_memmory_stream(ms);
                kill_file_stream(file);
                return file_name;
            }
            catch (Exception exp)
            {
                Log(log_key, exp.Message + "(base64data=" + base64data + ")", true, exp);
            }
            return "";
        }
        public void kill_file_stream(FileStream file)
        {
            try
            {
                file.Close();
            }
            catch
            {

            }
            try
            {
                file.Dispose();
            }
            catch
            {

            }
            try
            {
                file = null;
            }
            catch
            {

            }
            clear_gc();
        }
        public bool SchDeletePic(string photo_file_name)
        {
            string log_key = "SchDeletePic";
            try
            {
                if (photo_file_name.Trim().Length == 0)
                    return false;
                string pic_url = get_asset_folder() + photo_file_name;
                if (!File.Exists(pic_url))
                    return false;
                clear_gc();
                File.Delete(pic_url);
                clear_gc();
                return true;
            }
            catch (Exception exp)
            {
                Log(log_key, exp.Message, true, exp);
            }
            return false;
        }
        public EcomAuthLoginItem GetProfile(string cust_id, out string res_status)
        {
            string log_key = "GetProfile";
            res_status = "";
            EcomAuthLoginItem res = new EcomAuthLoginItem();
            try
            {
                clsConnectionSQL conn = new clsConnectionSQL(); 
                string query = @"SELECT [cust_id] 
      , [store_id]
      , [email]
      , [is_email_verified]
      , [photo_url]
      , [phone_number]
      , [contact_name]
      ,convert(char(20),[dob],113) as dob 
   , [gender] 
  FROM  [dbo].[ecom_customer]  
  where  cust_id='" + cust_id + @"'";
                DataTable dtData = conn.getDataTable(query);
                if (dtData.Rows.Count > 0)
                {
                    res.cust_id = dtData.Rows[0]["cust_id"].ToString().Trim();
                    res.store_id = dtData.Rows[0]["store_id"].ToString().Trim();
                    res.email = dtData.Rows[0]["email"].ToString().Trim();
                    res.is_email_verified = dtData.Rows[0]["is_email_verified"].ToString().Trim();
                    res.photo_url = dtData.Rows[0]["photo_url"].ToString().Trim();
                    res.phone_number = dtData.Rows[0]["phone_number"].ToString().Trim();
                    res.contact_name = dtData.Rows[0]["contact_name"].ToString().Trim();
                    res.dob = dtData.Rows[0]["dob"].ToString().Trim();
                    res.gender = dtData.Rows[0]["gender"].ToString().Trim();
                    res.is_ok = "Y";
                }
            }
            catch (Exception exp)
            {
                Log(log_key, "Exception occured. " + exp.Message, true, exp, true);
            }
            return res;
        }
        public EcomAuthLoginItem AuthLogin(dynamic inData, out string res_status)
        {
            string log_key = "AuthLogin";
            res_status = "";
            EcomAuthLoginItem res = new EcomAuthLoginItem();
            try
            {
                clsConnectionSQL conn = new clsConnectionSQL();
                string app_identity = get_value(inData.app_identity);
                string email = get_value(inData.email);
                string is_email_verified = get_value(inData.is_email_verified);
                string photo_url = get_value(inData.photo_url);
                string phone_number = get_value(inData.phone_number);
                string contact_name = get_value(inData.contact_name);
                string dob = get_value(inData.dob);

                string provider_id = get_value(inData.provider_id);
                string need_server_validation = get_value(inData.need_server_validation);
                string auth_key = get_value(inData.auth_key);
                string auth_token = get_value(inData.auth_token);
                string is_ok = "Y";
                if (need_server_validation.ToString().Trim().ToLower() == "y")
                {
                    if (!validate_customer(provider_id, auth_key, auth_token))
                        is_ok = "N";
                }
                if (is_ok.ToLower() == "n")
                {
                    res_status = "Unauthorized!";
                    return res;
                }
                string query = @"SELECT  c.[cust_id]
      ,c.[store_id]
      ,c.[email]
      ,c.[is_email_verified]
      ,c.[photo_url]
      ,c.[phone_number]
      ,c.[contact_name]
      ,convert(char(20),c.[dob],113) as dob 
   ,c.[gender]
      ,c.[is_active] c_is_active
 ,s.[is_active] s_is_active
  FROM  [dbo].[ecom_customer] c 
  inner join [dbo].[ecom_store] s on 
  s.store_id=c.store_id  
  where c.email='" + email + @"' and s.app_identity='" + app_identity + @"'";
                DataTable dtData = conn.getDataTable(query);
                if (dtData.Rows.Count > 0)
                {
                    if (dtData.Rows[0]["c_is_active"].ToString().Trim().ToLower() != "y")
                    {
                        res_status = "Account is not active. Please contact the Shop Administrator.";
                        return res;
                    }
                    if (dtData.Rows[0]["s_is_active"].ToString().Trim().ToLower() != "y")
                    {
                        res_status = "Shop is temporarily inactive. Please try later or contact the Shop Administrator.";
                        return res;
                    }
                    if (!IsEmpty(is_email_verified))
                    {
                        if (dtData.Rows[0]["is_email_verified"].ToString().Trim().ToLower() != is_email_verified.Trim().ToLower())
                        {
                            conn.ExecuteNonQuery(@"UPDATE [dbo].[ecom_customer]
   SET [is_email_verified]='" + is_email_verified + @"' WHERE 
cust_id='" + dtData.Rows[0]["cust_id"].ToString().Trim() + @"'");
                        }
                    }
                    res.cust_id = dtData.Rows[0]["cust_id"].ToString().Trim();
                    res.store_id = dtData.Rows[0]["store_id"].ToString().Trim();
                    res.email = dtData.Rows[0]["email"].ToString().Trim();
                    res.is_email_verified = dtData.Rows[0]["is_email_verified"].ToString().Trim();
                    res.photo_url = dtData.Rows[0]["photo_url"].ToString().Trim();
                    res.phone_number = dtData.Rows[0]["phone_number"].ToString().Trim();
                    res.contact_name = dtData.Rows[0]["contact_name"].ToString().Trim();
                    res.dob = dtData.Rows[0]["dob"].ToString().Trim();
                    res.gender = dtData.Rows[0]["gender"].ToString().Trim();
                    res.is_ok = "Y";
                }
                else
                {
                    string store_id = conn.ExecuteScalar(@"SELECT  [store_id]  FROM  [dbo].[ecom_store] where app_identity='" + app_identity + @"'");
                    if (IsEmpty(store_id))
                    {
                        res_status = "Error Code: 549. Please try later or contact the Shop Administrator.";
                        return res;
                    }
                    query = @"INSERT INTO [dbo].[ecom_customer]
           ([store_id]
           ,[email]
           ,[is_email_verified]
           ,[photo_url]
           ,[phone_number]
           ,[contact_name]
           ,[dob] 
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + store_id + @"'
           ,'" + email + @"'
           ,'" + is_email_verified + @"'
           ,'" + photo_url + @"'
           ,'" + phone_number + @"'
           ,'" + contact_name + @"'
               ,'" + dob + @"' 
           ,getdate() 
           ,'Y')  SELECT SCOPE_IDENTITY()";
                    string cust_id = conn.ExecuteScalar(query);
                    if (cust_id.Trim() != "")
                    {
                        res.cust_id = cust_id;
                        res.store_id = store_id;
                        res.contact_name = contact_name;
                        res.photo_url = photo_url;
                        res.email = email;
                        res.is_email_verified = is_email_verified;
                        res.photo_url = photo_url;
                        res.phone_number = phone_number;
                        res.contact_name = contact_name;
                        res.dob = dob;
                        res.gender = "";
                        res.is_ok = "Y";
                    }
                }
            }
            catch (Exception exp)
            {
                Log(log_key, "Exception occured. " + exp.Message, true, exp, true);
            }
            return res;
        }
        public bool validate_customer(string provider_id, string auth_key, string auth_token)
        { 
            //vintest todo
            return false;
        }
        public bool removeFromFav(string prod_id, string cust_id, out string res_status)
        {
            string log_key = "removeFromFav";
            res_status = "";
            bool res = false;
            string query = "";
            try
            {
                clsConnectionSQL conn = new clsConnectionSQL();
                query = @"DELETE FROM [dbo].[ecom_favorite]
      WHERE prod_id='" + prod_id + @"' and cust_id='" + cust_id + "'";
                return conn.ExecuteNonQuery(query);
            }
            catch (Exception exp)
            {
                Log(log_key, "Exception occured. " + exp.Message, true, exp, true);
            }
            return res;
        }
        public bool addToFav(string prod_id, string cust_id,  out string res_status)
        {
            string log_key = "addToFav";
            res_status = "";
            bool res = false;
            string query = "";
            try
            {
                clsConnectionSQL conn = new clsConnectionSQL(); 
                query = @"delete [dbo].[ecom_favorite] where [cust_id]='"+ cust_id + @"' and [prod_id]='" + prod_id + @"'";
                conn.ExecuteNonQuery(query);
                query = @"INSERT INTO [dbo].[ecom_favorite]
           ([cust_id]
           ,[prod_id]
           ,[created_by]
           ,[created_on] 
           ,[is_active])
     VALUES
           ('"+cust_id+ @"'
           ,'" + prod_id + @"'
           ,'" + cust_id + @"'
           ,getdate() 
           ,'Y')";
                return conn.ExecuteNonQuery(query);
            }
            catch (Exception exp)
            {
                Log(log_key, "Exception occured. " + exp.Message, true, exp, true);
            }
            return res;
        }
        public string getCartCount(string dev_id, string cust_id, string store_id, out string res_status)
        {
            string log_key = "getCartCount";
            res_status = "";
            string res = "";
            string query = "";
            try
            {
                clsConnectionSQL conn = new clsConnectionSQL();
                if (IsEmpty(dev_id))
                    return res;
                if (IsEmpty(store_id))
                    return res;
                query = @"SELECT count(c.[cart_id]) 
  FROM [dbo].[ecom_cart] c

  inner join [dbo].[ecom_product] p
  on p.prod_id=c.prod_id 
  and isnull(p.[is_active],'')='Y' 

  inner join [dbo].[ecom_device] d 
  on d.dev_id=c.[dev_id]
  and d.store_id= '" + store_id + @"' 
  and d.[dev_id]= '" + dev_id + @"' 
  and isnull(d.[is_active],'')='Y'
  
  where c.[dev_id]= '" + dev_id + @"'  and
isnull(c.cust_id,'') =case when '" + cust_id + @"'='' then isnull(c.cust_id,'') else '" + cust_id + @"' end 
  and isnull(c.[is_active],'')='Y'";
                DataTable dtData = conn.getDataTable(query);
                if (dtData.Rows.Count > 0)
                {
                    res = dtData.Rows[0].ItemArray[0].ToString().Trim();
                }
            }
            catch (Exception exp)
            {
                Log(log_key, "Exception occured. " + exp.Message, true, exp, true);
            }
            return res;
        }
        public List<EcomCartItem> getCart(string dev_id, string cust_id, string store_id, out string res_status)
        {
            string log_key = "getCart";
            res_status = "";
            List<EcomCartItem> res = new List<EcomCartItem>();
            string query = "";
            try
            {
                clsConnectionSQL conn = new clsConnectionSQL();
                if (IsEmpty(dev_id))
                    return res;
                if (IsEmpty(store_id))
                    return res;
                query = @"SELECT 
c.[cart_id]
      ,c.[prod_id]
      ,c.[cust_id]
      ,c.[dev_id]
      ,c.[quantity]
      ,convert(char(20), c.[add_date],113) as add_date 
      ,c.[offer_id_for_coupen]
      ,c.[product_color_id]
      ,c.[product_size_id]
      ,c.[is_custom_size]
      ,c.[is_gift]
      ,c.[is_surprise_gift]
      ,c.[is_need_gift_wish_card]
      ,c.[gift_wish_card_url]
      ,c.[custom_size_field1_value]
      ,c.[custom_size_field2_value]
      ,c.[custom_size_field3_value]
      ,c.[custom_size_field4_value]
      ,c.[custom_size_field5_value]
      ,c.[custom_size_field6_value]
      ,c.[custom_size_field7_value]
      ,c.[custom_size_field8_value]
      ,c.[custom_size_field9_value]
      ,c.[custom_size_field10_value]
      ,c.[custom_size_color_code]
      ,c.[custom_size_message]
      ,c.[created_by]
      ,c.[created_on]
      ,c.[updated_by]
      ,c.[updated_on]
      ,c.[is_active] 
  FROM [dbo].[ecom_cart] c

  inner join [dbo].[ecom_product] p
  on p.prod_id=c.prod_id 
  and isnull(p.[is_active],'')='Y' 

  inner join [dbo].[ecom_device] d 
  on d.dev_id=c.[dev_id]
  and d.store_id= '" + store_id + @"' 
  and d.[dev_id]= '" + dev_id + @"' 
  and isnull(d.[is_active],'')='Y'
  
  where c.[dev_id]= '" + dev_id + @"'  and
isnull(c.cust_id,'') =case when '" + cust_id + @"'='' then isnull(c.cust_id,'') else '" + cust_id + @"' end 
  and isnull(c.[is_active],'')='Y'";
                DataTable dtData = conn.getDataTable(query);
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    EcomCartItem item = new EcomCartItem();

                    item.cart_id = dtData.Rows[i]["cart_id"].ToString().Trim();
                    item.prod_id = dtData.Rows[i]["prod_id"].ToString().Trim();
                    item.cust_id = dtData.Rows[i]["cust_id"].ToString().Trim();
                    item.dev_id = dtData.Rows[i]["dev_id"].ToString().Trim();
                    item.quantity = dtData.Rows[i]["quantity"].ToString().Trim();
                    item.add_date = dtData.Rows[i]["add_date"].ToString().Trim();
                    item.offer_id_for_coupen = dtData.Rows[i]["offer_id_for_coupen"].ToString().Trim();
                    item.product_color_id = dtData.Rows[i]["product_color_id"].ToString().Trim();
                    item.product_size_id = dtData.Rows[i]["product_size_id"].ToString().Trim();
                    item.is_custom_size = dtData.Rows[i]["is_custom_size"].ToString().Trim();
                    item.is_gift = dtData.Rows[i]["is_gift"].ToString().Trim();
                    item.is_surprise_gift = dtData.Rows[i]["is_surprise_gift"].ToString().Trim();
                    item.is_need_gift_wish_card = dtData.Rows[i]["is_need_gift_wish_card"].ToString().Trim();
                    item.gift_wish_card_url = dtData.Rows[i]["gift_wish_card_url"].ToString().Trim();
                    item.custom_size_field1_value = dtData.Rows[i]["custom_size_field1_value"].ToString().Trim();
                    item.custom_size_field2_value = dtData.Rows[i]["custom_size_field2_value"].ToString().Trim();
                    item.custom_size_field3_value = dtData.Rows[i]["custom_size_field3_value"].ToString().Trim();
                    item.custom_size_field4_value = dtData.Rows[i]["custom_size_field4_value"].ToString().Trim();
                    item.custom_size_field5_value = dtData.Rows[i]["custom_size_field5_value"].ToString().Trim();
                    item.custom_size_field6_value = dtData.Rows[i]["custom_size_field6_value"].ToString().Trim();
                    item.custom_size_field7_value = dtData.Rows[i]["custom_size_field7_value"].ToString().Trim();
                    item.custom_size_field8_value = dtData.Rows[i]["custom_size_field8_value"].ToString().Trim();
                    item.custom_size_field9_value = dtData.Rows[i]["custom_size_field9_value"].ToString().Trim();
                    item.custom_size_field10_value = dtData.Rows[i]["custom_size_field10_value"].ToString().Trim();
                    item.custom_size_color_code = dtData.Rows[i]["custom_size_color_code"].ToString().Trim();
                    item.custom_size_message = dtData.Rows[i]["custom_size_message"].ToString().Trim();
                    item.created_by = dtData.Rows[i]["created_by"].ToString().Trim();
                    item.created_on = dtData.Rows[i]["created_on"].ToString().Trim();
                    item.updated_by = dtData.Rows[i]["updated_by"].ToString().Trim();
                    item.updated_on = dtData.Rows[i]["updated_on"].ToString().Trim();
                    item.is_active = dtData.Rows[i]["is_active"].ToString().Trim();
                    string out_stat = "";
                    item.product = getProductsSingle(item.prod_id, "", out out_stat);
                    res.Add(item);
                }
            }
            catch (Exception exp)
            {
                Log(log_key, "Exception occured. " + exp.Message, true, exp, true);
            }
            return res;
        }
        public bool removeFromcart(string cart_id, out string res_status)
        {
            string log_key = "removeFromcart";
            res_status = "";
            bool res = false;
            string query = "";
            try
            {
                clsConnectionSQL conn = new clsConnectionSQL();
                query = @"DELETE FROM [dbo].[ecom_cart]
      WHERE cart_id='" + cart_id + @"'";
                return conn.ExecuteNonQuery(query);
            }
            catch (Exception exp)
            {
                Log(log_key, "Exception occured. " + exp.Message, true, exp, true);
            }
            return res;
        }
        public bool Updatecart(string cart_id, string offer_id_for_coupen,
            string product_color_id,
            string product_size_id,
            string is_custom_size,
            string is_gift,
            string is_surprise_gift,
            string is_need_gift_wish_card,
            string gift_wish_card_url,
            string custom_size_field1_value,
            string custom_size_field2_value,
            string custom_size_field3_value,
            string custom_size_field4_value,
            string custom_size_field5_value,
            string custom_size_field6_value,
            string custom_size_field7_value,
            string custom_size_field8_value,
            string custom_size_field9_value,
            string custom_size_field10_value,
            string custom_size_color_code,
            string custom_size_message,
            string prod_id,
            string quantity,
            string cust_id,
            string dev_id, out string res_status)
        {
            string log_key = "Updatecart";
            res_status = "";
            bool res = false;
            string query = "";
            try
            {
                clsConnectionSQL conn = new clsConnectionSQL();
                query = @"UPDATE [dbo].[ecom_cart]
   SET [prod_id] =  '" + prod_id + @"'
      ,[cust_id] =" + get_value_with_quote_for_sql(cust_id) + @"
      ,[dev_id] =  '" + dev_id + @"'
      ,[quantity] ='" + quantity + @"'
      ,[add_date] =getdate() 
      ,[offer_id_for_coupen] = '" + offer_id_for_coupen + @"'
      ,[product_color_id] = '" + product_color_id + @"'
      ,[product_size_id] = '" + product_size_id + @"'
      ,[is_custom_size] = '" + is_custom_size + @"'
      ,[is_gift] = '" + is_gift + @"'
      ,[is_surprise_gift] = '" + is_surprise_gift + @"'
      ,[is_need_gift_wish_card] = '" + is_need_gift_wish_card + @"'
      ,[gift_wish_card_url] = '" + gift_wish_card_url + @"'
      ,[custom_size_field1_value] = '" + custom_size_field1_value + @"'
      ,[custom_size_field2_value] = '" + custom_size_field2_value + @"'
      ,[custom_size_field3_value] = '" + custom_size_field3_value + @"'
      ,[custom_size_field4_value] = '" + custom_size_field4_value + @"'
      ,[custom_size_field5_value] = '" + custom_size_field5_value + @"'
      ,[custom_size_field6_value] = '" + custom_size_field6_value + @"'
      ,[custom_size_field7_value] = '" + custom_size_field7_value + @"'
      ,[custom_size_field8_value] = '" + custom_size_field8_value + @"'
      ,[custom_size_field9_value] = '" + custom_size_field9_value + @"'
      ,[custom_size_field10_value] = '" + custom_size_field10_value + @"'
      ,[custom_size_color_code] = '" + custom_size_color_code + @"'
      ,[custom_size_message] = '" + custom_size_message + @"'
     ,[updated_by] = " + get_value_with_quote_for_sql(cust_id) + @"
      ,[updated_on]= getdate()   
 WHERE cart_id='" + cart_id + @"'";
                return conn.ExecuteNonQuery(query);
            }
            catch (Exception exp)
            {
                Log(log_key, "Exception occured. " + exp.Message, true, exp, true);
            }
            return res;
        }
        public bool addTocart(string offer_id_for_coupen,  
            string product_color_id, 
            string product_size_id,
            string is_custom_size,
            string is_gift,
            string is_surprise_gift,
            string is_need_gift_wish_card,
            string gift_wish_card_url,
            string custom_size_field1_value,
            string custom_size_field2_value,
            string custom_size_field3_value,
            string custom_size_field4_value,
            string custom_size_field5_value,
            string custom_size_field6_value,
            string custom_size_field7_value,
            string custom_size_field8_value,
            string custom_size_field9_value,
            string custom_size_field10_value,
            string custom_size_color_code,
            string custom_size_message,
            string prod_id,
            string quantity,
            string cust_id, 
            string dev_id,
            out string res_status)
        {
            string log_key = "addTocart";
            res_status = "";
            bool res = false;
            string query = "";
            try
            {
                clsConnectionSQL conn = new clsConnectionSQL();

                query = @"delete [dbo].[ecom_cart] where [dev_id]='" + dev_id + @"' and [prod_id]='" + prod_id + @"'";
                conn.ExecuteNonQuery(query);
                query = @"INSERT INTO [dbo].[ecom_cart]
           ([prod_id]
           ,[cust_id]
           ,[dev_id]
           ,[quantity]
           ,[add_date]
           ,[offer_id_for_coupen]
           ,[product_color_id]
           ,[product_size_id]
           ,[is_custom_size]
           ,[is_gift]
           ,[is_surprise_gift]
           ,[is_need_gift_wish_card]
           ,[gift_wish_card_url]
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
           ,[created_by]
           ,[created_on] 
           ,[is_active])
     VALUES
           ('" + prod_id + @"'
           ," + get_value_with_quote_for_sql(cust_id) + @"
           ,'" + dev_id + @"'
          ,'" + quantity + @"'
           ,getdate()
            ,'" + offer_id_for_coupen + @"'
           ,'" + product_color_id + @"'
           ,'" + product_size_id + @"'
           ,'" + is_custom_size + @"'
           ,'" + is_gift + @"'
           ,'" + is_surprise_gift + @"'
           ,'" + is_need_gift_wish_card + @"'
           ,'" + gift_wish_card_url + @"'
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
          ,'" + cust_id + @"'
             ,getdate()
           ,'Y')";
                return conn.ExecuteNonQuery(query);
            }
            catch (Exception exp)
            {
                Log(log_key, "Exception occured. " + exp.Message, true, exp, true);
            }
            return res;
        }
        public List<EcomSearchCatItem> getSearchCats(string store_id, out string res_status)
        {
            string log_key = "getSearchCats";
            res_status = "";
            List<EcomSearchCatItem> res = new List<EcomSearchCatItem>();
            string query = "";
            try
            {
                clsConnectionSQL conn = new clsConnectionSQL();
                query = @"(SELECT   [cat_id] as id 
      ,[name_en]
      ,[name_ar] 
	  ,'category' as typ
  FROM  [dbo].[ecom_category] where 
  [store_id]='"+ store_id + @"' and 
  isnull(is_active,'')='Y')  
   union
  (
SELECT   [tag_id] as id 
      ,[tag_name_en] as name_en
      ,[tag_name_ar] as name_ar
	   ,'tag' as typ
  FROM  [dbo].[ecom_tag] where 
  [store_id]='" + store_id + @"' and 
  isnull(is_active,'')='Y')

     union
  (

  SELECT top 10  [kw_id] as id
      ,[kw]  as name_en
	    ,[kw]  as name_ar
        ,'popular' as typ
  FROM [dbo].[ecom_customer_search] where 
[search_count]>10 and 
  [store_id]='" + store_id + @"' and  
  isnull([is_active],'')='Y' ) order by typ,name_en,name_ar";
                DataTable dtData = conn.getDataTable(query);
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    EcomSearchCatItem this_item = new EcomSearchCatItem();
                    this_item.id = dtData.Rows[i]["id"].ToString().Trim();
                    this_item.type = dtData.Rows[i]["typ"].ToString().Trim();
                    this_item.name_en = dtData.Rows[i]["name_en"].ToString().Trim();
                    this_item.name_ar = dtData.Rows[i]["name_ar"].ToString().Trim();
                    res.Add(this_item);
                }  
            }
            catch (Exception exp)
            {
                Log(log_key, "Exception occured. " + exp.Message, true, exp, true);
            }
            return res;
        }
        public bool is_double_true_for_null(string val)
        {
            if (IsEmpty(val))
                return true;
            return is_double(val);
        }
        public bool is_double(string val)
        { 
            double value;
            if (double.TryParse(val, out value))
                return true;
            return false; 
        }
        public List<EcomProductData> getProductsDyna(dynamic inData, out string res_status)
        {
            string log_key = "getProducts";
            res_status = "";
            List<EcomProductData> res = new List<EcomProductData>();
            try
            {
                clsConnectionSQL conn = new clsConnectionSQL();
                string ecom_max_products_scroll_load = GetConfig("ecom_max_products_scroll_load");
                if (ecom_max_products_scroll_load == "")
                    ecom_max_products_scroll_load = "10";
                string last_idx = get_value(inData.last_idx);
                if (last_idx == "")
                    last_idx = "0";
                string is_for_list_str= get_value(inData.is_for_list);
                string condition_type = get_value(inData.condition_type);
                string store_id = get_value(inData.store_id);
                string search_kw = get_value(inData.search_kw);
                string cat_id = get_value(inData.cat_id);
                string brand_id = get_value(inData.brand_id);
                string color_name = get_value(inData.color_name);
                string color_code = get_value(inData.color_code);
                string min_price = get_value(inData.min_price);
                string max_price = get_value(inData.max_price);
                string prod_id = get_value(inData.prod_id);
                string tag_id = get_value(inData.tag_id);
                string cust_id = get_value(inData.cust_id);
                string is_fav = get_value(inData.is_fav);
                string is_active = get_value(inData.is_active);
                string is_custom_size_available = get_value(inData.is_custom_size_available);
                string is_gift_available = get_value(inData.is_gift_available);
                string is_surprise_gift_available = get_value(inData.is_surprise_gift_available);
                string age_from = get_value(inData.age_from);
                string age_to = get_value(inData.age_to);
                string admin_approval_status = get_value(inData.admin_approval_status);
                string stock_below = get_value(inData.stock_below);
                string stock_above = get_value(inData.stock_above);

                if (IsEmpty(is_fav.Trim()))
                    is_fav = "N";
                clsEcomAuth auth = new clsEcomAuth();
                if (search_kw.Trim().Length > 0)
                {
                    if (auth.IsContainsSpecialChars(search_kw))
                    {
                        res_status = "Search key should not contain special characters";
                        return res;
                    }
                }
                return getProductsCore(is_for_list_str,ecom_max_products_scroll_load,
             last_idx,
             condition_type,
             store_id,
             search_kw,
             cat_id,
             brand_id,
             tag_id,
             color_code,
             min_price,
             max_price,
             prod_id,
             cust_id,
             is_fav,
             is_active,
             is_custom_size_available,
             is_gift_available,
             is_surprise_gift_available,
             age_from,
             age_to,
             admin_approval_status,
             stock_below,
             stock_above,
            out res_status);
            }
            catch (Exception exp)
            {
                Log(log_key, "Exception occured. " + exp.Message, true, exp, true);
            }
            return res;
        }
        
        public EcomProductData getProductsSingle(string prod_id, string cust_id, out string res_status)
        {
            List<EcomProductData> prods = getProductsCore("N", "1",
            "0",
            "none",
            "",
             "",
            "",
            "",
            "",
            "",
            "",
            "",
              prod_id,
              cust_id,
            "N",
            "Y",
           "",
            "",
            "",
           "",
            "",
            "",
             "",
             "",
            out res_status);
            if (prods.Count > 0)
                return prods[0];
            return new EcomProductData();
        }
        public List<EcomAdOlaData> getAdsOlaForHome(string store_id,out string res_status)
        {
            string log_key = "getAdsOlaForHome";
            res_status = "";
            List<EcomAdOlaData> res = new List<EcomAdOlaData>();
            string query = "";
            try
            {
                clsConnectionSQL conn = new clsConnectionSQL(); 
                query = @"SELECT  a.[ad_id]
      ,a.[brand_id]
      ,a.[txn_id]
      ,a.[is_animation]
      ,a.[img_url]
      ,a.[date_from]
      ,a.[date_to]
      ,a.[is_paid]
      ,a.[destination_type]
      ,a.[destination_value]
      ,a.[admin_approval_status]
      ,a.[admin_approval_remarks]
      ,a.[order_by]
      ,a.[created_by]
      ,a.[created_on]
      ,a.[updated_by]
      ,a.[updated_on]
      ,a.[is_active]
  FROM  [dbo].[ecom_ad] a
  inner join [dbo].[ecom_brand] b
  on b.brand_id=a.brand_id and
  b.store_id='"+ store_id + @"'
  where isnull(a.is_active,'N')='Y'
  and getdate() between a.[date_from] and a.[date_to] 
  and a.[admin_approval_status]='approved'
  order by order_by"; 

                DataTable dtData = conn.getDataTable(query); 
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    EcomAdOlaData this_item = new EcomAdOlaData();
                    this_item.ad_id = dtData.Rows[i]["ad_id"].ToString().Trim();
                    this_item.brand_id = dtData.Rows[i]["brand_id"].ToString().Trim(); 
                    this_item.is_animation = dtData.Rows[i]["is_animation"].ToString().Trim();
                    this_item.img_url = dtData.Rows[i]["img_url"].ToString().Trim();
                    this_item.date_from = dtData.Rows[i]["date_from"].ToString().Trim();
                    this_item.date_to = dtData.Rows[i]["date_to"].ToString().Trim(); 
                    this_item.destination_type = dtData.Rows[i]["destination_type"].ToString().Trim();
                    this_item.destination_value = dtData.Rows[i]["destination_value"].ToString().Trim(); 
                    this_item.order_by = dtData.Rows[i]["order_by"].ToString().Trim(); 
                    res.Add(this_item);
                }  
            }
            catch (Exception exp)
            {
                Log(log_key, "Exception occured. " + exp.Message, true, exp, true);
            }
            return res;
        }
        public List<EcomBusinessSectorData> getBusinessSectors(string store_id,string is_active, out string res_status)
        {
            string log_key = "getBusinessSectors";
            res_status = "";
            List<EcomBusinessSectorData> res = new List<EcomBusinessSectorData>();
            string query = "";
            try
            {
                clsConnectionSQL conn = new clsConnectionSQL();
                query = @"SELECT   [sector_id]
      ,[store_id]
      ,[sector_name_en]
      ,[sector_name_ar]
      ,[order_by]
      ,[created_by]
      ,[created_on]
      ,[updated_by]
      ,[updated_on]
      ,[is_active]
  FROM [dbbomo1].[dbo].[ecom_business_sector] where 
isnull([store_id],'')=case when '" + store_id + @"'='' then  isnull([store_id],'') else '" + store_id + @"' end and 
isnull([is_active],'')=case when '" + is_active + @"'='' then  isnull([is_active],'') else '" + is_active + @"' end order by order_by";

                DataTable dtData = conn.getDataTable(query);
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    EcomBusinessSectorData this_item = new EcomBusinessSectorData();
                    this_item.sector_id = dtData.Rows[i]["sector_id"].ToString().Trim();
                    this_item.store_id = dtData.Rows[i]["store_id"].ToString().Trim();
                    this_item.sector_name_en = dtData.Rows[i]["sector_name_en"].ToString().Trim();
                    this_item.sector_name_ar = dtData.Rows[i]["sector_name_ar"].ToString().Trim();
                    this_item.order_by = dtData.Rows[i]["order_by"].ToString().Trim();
                    this_item.created_by = dtData.Rows[i]["created_by"].ToString().Trim();
                    this_item.created_on = dtData.Rows[i]["created_on"].ToString().Trim();
                    this_item.updated_by = dtData.Rows[i]["updated_by"].ToString().Trim();
                    this_item.updated_on = dtData.Rows[i]["updated_on"].ToString().Trim();
                    this_item.is_active = dtData.Rows[i]["is_active"].ToString().Trim();
                    res.Add(this_item);
                }
            }
            catch (Exception exp)
            {
                Log(log_key, "Exception occured. " + exp.Message, true, exp, true);
            }
            return res;
        }
        public bool can_register_brand(dynamic inData, string store_id, out string res)
        {
            string name_owner_en = get_value(inData.name_owner_en);
            res = "";
            string stat = "";
            if (!IsEmptyAndContainsSpecial(name_owner_en, "Owner Name in English", out stat))
            {
                res = stat;
                return false;
            } 

            string name_owner_ar = get_value(inData.name_owner_ar);
            if (!IsEmptyAndContainsSpecial(name_owner_ar, "Owner Name in Arabic", out stat))
            {
                res = stat;
                return false;
            } 

            string email = get_value(inData.email);
            if (IsEmpty(email))
            {
                res = "Email should not be empty";
                return false;
            }
            clsConnectionSQL conn = new clsConnectionSQL();
            DataTable dtExist = conn.getDataTable(@"SELECT * FROM [dbo].[ecom_brand] where [email] = '" + email + @"' and store_id='" + store_id + "'");
            if (dtExist.Rows.Count > 0)
            {
                res = "Email already existing for another Brand";
                return false;
            }
            dtExist = conn.getDataTable(@"SELECT s.* FROM [dbo].[ecom_staff] s inner join [dbo].[ecom_brand] b 
on b.[brand_id]=s.[brand_id] and b.store_id='" + store_id + "' where s.[email] = '" + email + @"' ");
            if (dtExist.Rows.Count > 0)
            {
                res = "Email already using as a Staff User of another Brand";
                return false;
            } 
            return true;
        }
        public List<EcomProductData> getProductsCore(string is_for_list_str, string ecom_max_products_scroll_load, 
            string last_idx, 
            string condition_type, 
            string store_id, 
            string search_kw, 
            string cat_id, 
            string brand_id, 
            string tag_id,  
            string color_code, 
            string min_price, 
            string max_price, 
            string prod_id, 
            string cust_id,
            string is_fav,
            string is_active,
            string is_custom_size_available,
            string is_gift_available,
            string is_surprise_gift_available,
            string age_from, 
            string age_to,
            string admin_approval_status,
             string stock_below,
              string stock_above,
            out string res_status)
        {
            string log_key = "getProducts2";
            res_status = "";
            List<EcomProductData> res = new List<EcomProductData>();
            string query = "";
            try
            {
                clsConnectionSQL conn = new clsConnectionSQL();
                bool is_fav_bool = false;
                if (get_value(is_fav).ToLower().Trim() == "y")
                    is_fav_bool = true;
                if (is_fav_bool)
                {
                    if (get_value(cust_id).Trim().Length == 0)
                    {
                        res_status = "You are not logged in. Please login and try again.";
                        return res;
                    }
                }
                bool is_for_list = get_bool_from_yn(is_for_list_str);
                string condition_fileds = ""; 

                if (condition_type == "name_en")
                    condition_fileds = " [name_en] ";
                if (condition_type == "name_ar")
                    condition_fileds = " [name_ar] ";

                else if (condition_type == "new_to_old")
                    condition_fileds = " case when isnull([updated_on],'')='' then [created_on] else [updated_on] end DESC ";
                else if (condition_type == "old_to_new")
                    condition_fileds = " case when isnull([updated_on],'')='' then [created_on] else [updated_on] end ASC ";

                else if (condition_type == "min_to_max_price")
                    condition_fileds = " [unit_price] ASC ";
                else if (condition_type == "max_to_min_price")
                    condition_fileds = " [unit_price] DESC "; 
                else
                    condition_fileds = " [order_by] ";


                clsEcomAuth auth = new clsEcomAuth();
                if (search_kw.Trim().Length > 0)
                {
                    if (auth.IsContainsSpecialChars(search_kw))
                    {
                        res_status = "Search key should not contain special characters";
                        return res;
                    }
                }
                string extra_for_tag = "";
                if (!IsEmpty(tag_id))
                {
                    extra_for_tag = @" inner join [dbo].[ecom_product_tag] ptag on 
 ptag.prod_id= a.prod_id  and 
ptag.tag_id='" + tag_id + @"' and 
   ptag.is_active =case when '" + is_active + @"'='' then ptag.is_active else '" + is_active + @"' end  ";
                }

                string extra_for_store_id = "";
                if (!IsEmpty(store_id))
                {
                    extra_for_store_id = @" inner join [dbo].[ecom_brand] pbrand on 
 pbrand.brand_id= a.brand_id  and 
pbrand.store_id='" + store_id + @"' and 
   pbrand.is_active =case when '" + is_active + @"'='' then pbrand.is_active else '" + is_active + @"' end  ";
                }

                string extra_for_fav = "";
                if (is_fav_bool)
                {
                    extra_for_fav = @" and prod_id in (select prod_id from [dbo].[ecom_favorite] where 
  cust_id='"+cust_id+"' and  is_active=case when '" + is_active + @"'='' then  is_active else '" + is_active + @"' end ) ";
                }

                string extra_for_color_code= "";
                if (!IsEmpty(color_code))
                {
                    extra_for_color_code = @" and prod_id in (select prod_id from [dbo].[ecom_product_color] where 
  LOWER([color_code])='" + color_code.ToLower() + "')";
                }
                string extra_for_stock = "";
                if (!IsEmpty(stock_below))
                {
                    extra_for_stock = @" and prod_id in (select prod_id from [dbo].[ecom_product_stock] where 
  [stock_count]<=" + stock_below + ")";
                }
                string extra_for_stock_above = "";
                if (!IsEmpty(stock_above))
                {
                    extra_for_stock_above = @" and prod_id in (select prod_id from [dbo].[ecom_product_stock] where 
  [stock_count]>" + stock_above + ")";
                }
                query = @"SELECT
a.[prod_id]
      ,a.[brand_id]

,br1.[name_brand_en] as brand_name_en
   ,br1.[name_brand_ar] as brand_name_ar

      ,a.[cat_id]
,ca1.[name_en] as cat_name_en
   ,ca1.[name_ar] as cat_name_ar

      ,a.[name_en]
      ,a.[name_ar]
      ,a.[name_desc_en]
      ,a.[name_desc_ar]
      ,a.[quantity]
      ,a.[unit_price]
      ,a.[is_custom_size_available]

      ,a.[custome_size_extra_price]
      ,a.[custome_size_extra_price_perc]
  ,a.[gift_extra_price]
      ,a.[gift_extra_price_perc]
  ,a.[surprise_gift_extra_price]
      ,a.[surprise_gift_extra_price_perc]
      ,a.[wish_card_extra_price]
      ,a.[wish_card_extra_price_perc]

      ,a.[custom_size_img_url]
      ,a.[custom_size_description]
      ,a.[custom_size_field1]
      ,a.[custom_size_field2]
      ,a.[custom_size_field3]
      ,a.[custom_size_field4]
      ,a.[custom_size_field5]
      ,a.[custom_size_field6]
      ,a.[custom_size_field7]
      ,a.[custom_size_field8]
      ,a.[custom_size_field9]
      ,a.[custom_size_field10]
      ,a.[is_gift_available]
      ,a.[is_surprise_gift_available]
      ,a.[size_chart_img_url]
      ,a.[age_from]
      ,a.[age_to]
      ,a.[admin_approval_status]
      ,a.[admin_approval_remarks]
      ,a.[order_by]
      ,a.[created_by]
         ,convert(char(20),a.[created_on],113) as created_on
      ,a.[updated_by]
	   ,convert(char(20),a.[updated_on],113) as updated_on 
      ,a.[is_active] 
 ,offer.[offer_id]
 ,offer.[offer_dec_en]
	  ,offer.[offer_dec_ar]
	   ,offer.[offer_perc] 
,convert(char(20),offer.[valid_from],113) valid_from
,convert(char(20),offer.[valid_to],113) valid_to
	,offer.[coupon_code] 
,asset.[img_url] 
,RowNum 
FROM (SELECT ROW_NUMBER() OVER (ORDER BY " + condition_fileds + @") As RowNum, * 
   FROM [dbo].[ecom_product] 
 where  

([name_en] like '%'+''+case when '" + search_kw + @"'='' then 
   [name_en] else '" + search_kw + @"' end +''+'%' or
   [name_ar] like '%'+''+case when '" + search_kw + @"'='' then 
   [name_ar] else '" + search_kw + @"' end +''+'%'
   or
   [name_desc_en] like '%'+''+case when '" + search_kw + @"'='' then 
   [name_desc_en] else '" + search_kw + @"' end +''+'%'
     or
   [name_desc_ar] like '%'+''+case when '" + search_kw + @"'='' then 
   [name_desc_ar] else '" + search_kw + @"' end +''+'%'
   ) and is_active=case when '" + is_active + @"'='' then  is_active else '" + is_active + @"' end

 and [cat_id]=case when '" + cat_id + @"'='' then [cat_id] else '" + cat_id + @"' end 

    and [brand_id]=case when '" + brand_id + @"'='' then [brand_id] else '" + brand_id + @"' end 
  

and 
	[unit_price] >= case when '" + min_price + @"'='' then 
   [unit_price] else '" + min_price + @"' end

and 
	[unit_price] <= case when '" + max_price + @"'='' then 
   [unit_price] else '" + max_price + @"' end 


and 
	[age_from] >= case when '" + age_from + @"'='' then 
   [age_from] else '" + age_from + @"' end

and 
	[age_to] <= case when '" + age_to + @"'='' then 
   [age_to] else '" + age_to + @"' end 


and [prod_id]= case when '" + prod_id + @"'='' then 
   [prod_id] else '" + prod_id + @"' end  

and [is_custom_size_available]= case when '" + is_custom_size_available + @"'='' then 
   [is_custom_size_available] else '" + is_custom_size_available + @"' end  

and [is_gift_available]= case when '" + is_gift_available + @"'='' then 
   [is_gift_available] else '" + is_gift_available + @"' end  

and [is_surprise_gift_available]= case when '" + is_surprise_gift_available + @"'='' then 
   [is_surprise_gift_available] else '" + is_surprise_gift_available + @"' end  

and [admin_approval_status]= case when '" + admin_approval_status + @"'='' then 
   [admin_approval_status] else '" + admin_approval_status + @"' end  

" + extra_for_fav + @"  
" + extra_for_color_code + @"
" + extra_for_stock + @"
" + extra_for_stock_above + @"
) As a  
inner join ecom_brand br1 on
br1.brand_id=a.[brand_id] and 
isnull(br1.is_active,'')='Y'

inner join ecom_category ca1 on
ca1.cat_id=a.[cat_id] and 
isnull(ca1.is_active,'')='Y'

" + extra_for_tag + @"   
" + extra_for_store_id + @"   
      left join [dbo].[ecom_offer] offer on 
   (offer.prod_id=a.prod_id or
   offer.brand_id=a.brand_id or
   offer.cat_id=a.cat_id) and
    offer.is_active=case when '" + is_active + @"'='' then  offer.is_active else '" + is_active + @"' end
   and  getdate() between valid_from and valid_to 

  left join [dbo].[ecom_product_asset] asset on 
asset.[prod_id]=a.[prod_id] and 
isnull(asset.[is_default],'')='Y' and 
 asset.[is_active]=case when '" + is_active + @"'='' then  asset.is_active else '" + is_active + @"' end and 
asset.[type]='image' 

left join ecom_staff esc on 
esc.staff_id=a.[created_by]

left join ecom_staff esu on 
esu.staff_id=a.[updated_by]

WHERE RowNum BETWEEN 1+(" + last_idx + @") AND (" + last_idx + @"+" + ecom_max_products_scroll_load + @")";
                //send_error("query", query);
                DataTable dtData = conn.getDataTable(query);
                List<string> prod_exist = new List<string>();
                string prod_ids = "";
                for (int i = 0; i < dtData.Rows.Count; i++)
                {
                    EcomProductData this_item = new EcomProductData();
                    this_item.prod_id = dtData.Rows[i]["prod_id"].ToString().Trim();
                    if (prod_exist.Contains(this_item.prod_id))
                        continue;
                    prod_exist.Add(this_item.prod_id); 

                    this_item.brand_name_en = dtData.Rows[i]["brand_name_en"].ToString().Trim();
                    this_item.brand_name_ar = dtData.Rows[i]["brand_name_ar"].ToString().Trim();
                    this_item.cat_name_en = dtData.Rows[i]["cat_name_en"].ToString().Trim();
                    this_item.cat_name_ar = dtData.Rows[i]["cat_name_ar"].ToString().Trim();
                    this_item.custome_size_extra_price = dtData.Rows[i]["custome_size_extra_price"].ToString().Trim();
                    this_item.custome_size_extra_price_perc = dtData.Rows[i]["custome_size_extra_price_perc"].ToString().Trim();
                    this_item.gift_extra_price = dtData.Rows[i]["gift_extra_price"].ToString().Trim();
                    this_item.gift_extra_price_perc = dtData.Rows[i]["gift_extra_price_perc"].ToString().Trim();
                    this_item.surprise_gift_extra_price = dtData.Rows[i]["surprise_gift_extra_price"].ToString().Trim();
                    this_item.surprise_gift_extra_price_perc = dtData.Rows[i]["surprise_gift_extra_price_perc"].ToString().Trim();
                    this_item.wish_card_extra_price = dtData.Rows[i]["wish_card_extra_price"].ToString().Trim();
                    this_item.wish_card_extra_price_perc = dtData.Rows[i]["wish_card_extra_price_perc"].ToString().Trim();

                    this_item.brand_id = dtData.Rows[i]["brand_id"].ToString().Trim();
                    this_item.cat_id = dtData.Rows[i]["cat_id"].ToString().Trim();
                    if (!is_for_list)
                    { 
                        this_item.quantity = dtData.Rows[i]["quantity"].ToString().Trim();  
                        this_item.is_custom_size_available = dtData.Rows[i]["is_custom_size_available"].ToString().Trim();
                        this_item.custom_size_img_url = dtData.Rows[i]["custom_size_img_url"].ToString().Trim();
                        this_item.custom_size_description = dtData.Rows[i]["custom_size_description"].ToString().Trim();
                        this_item.custom_size_field1 = dtData.Rows[i]["custom_size_field1"].ToString().Trim();
                        this_item.custom_size_field2 = dtData.Rows[i]["custom_size_field2"].ToString().Trim();
                        this_item.custom_size_field3 = dtData.Rows[i]["custom_size_field3"].ToString().Trim();
                        this_item.custom_size_field4 = dtData.Rows[i]["custom_size_field4"].ToString().Trim();
                        this_item.custom_size_field5 = dtData.Rows[i]["custom_size_field5"].ToString().Trim();
                        this_item.custom_size_field6 = dtData.Rows[i]["custom_size_field6"].ToString().Trim();
                        this_item.custom_size_field7 = dtData.Rows[i]["custom_size_field7"].ToString().Trim();
                        this_item.custom_size_field8 = dtData.Rows[i]["custom_size_field8"].ToString().Trim();
                        this_item.custom_size_field9 = dtData.Rows[i]["custom_size_field9"].ToString().Trim();
                        this_item.custom_size_field10 = dtData.Rows[i]["custom_size_field10"].ToString().Trim();
                        this_item.is_gift_available = dtData.Rows[i]["is_gift_available"].ToString().Trim();
                        this_item.is_surprise_gift_available = dtData.Rows[i]["is_surprise_gift_available"].ToString().Trim();
                        this_item.size_chart_img_url = dtData.Rows[i]["size_chart_img_url"].ToString().Trim();
                        this_item.age_from = dtData.Rows[i]["age_from"].ToString().Trim();
                        this_item.age_to = dtData.Rows[i]["age_to"].ToString().Trim();
                        this_item.admin_approval_status = dtData.Rows[i]["admin_approval_status"].ToString().Trim();
                        this_item.admin_approval_remarks = dtData.Rows[i]["admin_approval_remarks"].ToString().Trim();
                        this_item.order_by = dtData.Rows[i]["order_by"].ToString().Trim();
                        this_item.created_by = dtData.Rows[i]["created_by"].ToString().Trim();
                        this_item.created_on = dtData.Rows[i]["created_on"].ToString().Trim();
                        this_item.updated_by = dtData.Rows[i]["updated_by"].ToString().Trim();
                        this_item.updated_on = dtData.Rows[i]["updated_on"].ToString().Trim();
                        this_item.is_active = dtData.Rows[i]["is_active"].ToString().Trim();

                       // this_item.created_by_name = dtData.Rows[i]["created_by_name"].ToString().Trim();
                        //this_item.updated_by_name = dtData.Rows[i]["updated_by_name"].ToString().Trim();
                    }
                    this_item.name_en = dtData.Rows[i]["name_en"].ToString().Trim();
                    this_item.name_ar = dtData.Rows[i]["name_ar"].ToString().Trim();
                    this_item.name_desc_en = dtData.Rows[i]["name_desc_en"].ToString().Trim();
                    this_item.name_desc_ar = dtData.Rows[i]["name_desc_ar"].ToString().Trim();
                    this_item.unit_price = dtData.Rows[i]["unit_price"].ToString().Trim();  
                    this_item.last_row_index = dtData.Rows[i]["RowNum"].ToString().Trim();
                    this_item.img_url = dtData.Rows[i]["img_url"].ToString().Trim();

                    this_item.offer = new EcomOfferData();
                    this_item.offers = new List<EcomOfferData>();

                    this_item.asset = new List<EcomProductAssetsData>();
                    this_item.color = new List<EcomProductColorData>();
                    this_item.wishcard = new List<EcomProductWishCardData>();
                    this_item.size = new List<EcomProductSizeData>();
                    this_item.stock = new List<EcomProductStockData>();
                    this_item.tag = new List<EcomProductTagData>();



                    this_item.offer = new EcomOfferData();
                    this_item.offer.offer_id = dtData.Rows[i]["offer_id"].ToString().Trim();
                    this_item.offer.offer_dec_en = dtData.Rows[i]["offer_dec_en"].ToString().Trim();
                    this_item.offer.offer_dec_ar = dtData.Rows[i]["offer_dec_ar"].ToString().Trim();
                    this_item.offer.offer_perc = dtData.Rows[i]["offer_perc"].ToString().Trim();
                    this_item.offer.valid_from = dtData.Rows[i]["valid_from"].ToString().Trim();
                    this_item.offer.valid_to = dtData.Rows[i]["valid_to"].ToString().Trim();
                    this_item.offer.coupon_code = dtData.Rows[i]["coupon_code"].ToString().Trim();
                    string new_price = get_new_offer_price(this_item.quantity, this_item.unit_price, this_item.offer.offer_perc);
                    if (!IsEmpty(new_price))
                    {
                      //  this_item.unit_price_old = this_item.unit_price;
                      //  this_item.unit_price = new_price;
                    }
                    if (!IsEmpty(prod_ids))
                        prod_ids += ",";
                    prod_ids += "'" + this_item.prod_id + "'";
                    this_item.is_fav = "N";
                    this_item.is_active = dtData.Rows[i]["is_active"].ToString().Trim();
                    res.Add(this_item);
                }

                //====
                if (!IsEmpty(prod_ids))
                {
                    DataTable dtDataFavs = new DataTable();
                    if (!IsEmpty(cust_id))
                    {
                        query = @"SELECT  [fav_id]
      ,[cust_id]
      ,[prod_id] 
  FROM  [dbo].[ecom_favorite] where [cust_id]='" + cust_id + "' and  [is_active]=case when '" + is_active + @"'='' then  is_active else '" + is_active + @"' end 
and [prod_id] in (" + prod_ids + @")";
                        dtDataFavs = conn.getDataTable(query);
                        if (dtDataFavs.Rows.Count > 0)
                        {
                            for (int j = 0; j < res.Count; j++)
                            {
                                for (int k = 0; k < dtDataFavs.Rows.Count; k++)
                                {
                                    if (res[j].prod_id == dtDataFavs.Rows[k]["prod_id"].ToString().Trim())
                                    {
                                        res[j].is_fav = "Y";
                                        break;
                                    }
                                }
                            }
                        }
                    }


                    //assets
                    query = @"SELECT  a.[asset_id]
      ,a.[prod_id]
      ,a.[product_color_id]
      ,a.[type]
      ,a.[img_url]
      ,a.[is_default]

      ,a.[order_by] 
   ,a.[created_by]
      ,convert(char(20),a.[created_on],113) as created_on
      ,a.[updated_by]
	   ,convert(char(20),a.[updated_on],113) as updated_on 
      ,a.[is_active]
	  ,esc.name_en as created_by_name
,esu.name_en as updated_by_name

  FROM  [dbo].[ecom_product_asset] a
left join ecom_staff esc on 
esc.staff_id=a.[created_by]

left join ecom_staff esu on 
esu.staff_id=a.[updated_by]
where 
   a.[is_active]=case when '" + is_active + @"'='' then  a.is_active else '" + is_active + @"' end and 
  a.[prod_id] in (" + prod_ids + @") order by a.[prod_id],a.[order_by]";
                    DataTable dtAssets = conn.getDataTable(query);


                    //offers
                    DataTable dtOffers = new DataTable();

                    query = @"SELECT distinct [offer_id]
      ,[prod_id]
      ,[cat_id]
      ,[tag_id]
      ,[brand_id]
      ,[offer_by_brand_id]
      ,[offer_img_url]
      ,[offer_dec_en]
      ,[offer_dec_ar]
      ,[offer_perc]
      ,[valid_from]
      ,[valid_to]
      ,[coupon_code]
      ,[created_by]
      ,[created_on]
      ,[updated_by]
      ,[updated_on]
      ,[is_active]
  FROM  [dbo].[ecom_offer] where 
  (
prod_id in  (" + prod_ids + @") or
brand_id  in (select distinct brand_id from ecom_product where prod_id in (" + prod_ids + @") and  is_active=case when '" + is_active + @"'='' then  is_active else '" + is_active + @"' end) or
 cat_id in (select distinct cat_id from ecom_product where prod_id in (" + prod_ids + @") and  is_active=case when '" + is_active + @"'='' then  is_active else '" + is_active + @"' end) or
 [tag_id] in (select distinct tag_id from ecom_product_tag where prod_id in (" + prod_ids + @") and  is_active=case when '" + is_active + @"'='' then  is_active else '" + is_active + @"' end)) and 
 is_active=case when '" + is_active + @"'='' then  is_active else '" + is_active + @"' end
   and  getdate() between valid_from and valid_to";
                    dtOffers = conn.getDataTable(query);

                    //color
                    DataTable dtColors = new DataTable();
                    if (!is_for_list)
                    {
                        query = @"SELECT   a.[product_color_id]
      ,a.[prod_id]
      ,a.[color_name_en]
      ,a.[color_name_ar]
      ,a.[color_code]
      ,a.[color_image_url]

      ,a.[order_by] 
   ,a.[created_by]
      ,convert(char(20),a.[created_on],113) as created_on
      ,a.[updated_by]
	   ,convert(char(20),a.[updated_on],113) as updated_on 
      ,a.[is_active]
	  ,esc.name_en as created_by_name
,esu.name_en as updated_by_name
  FROM  [dbo].[ecom_product_color] a 

  left join ecom_staff esc on 
esc.staff_id=a.[created_by]

left join ecom_staff esu on 
esu.staff_id=a.[updated_by]
where 
   a.[is_active]=case when '" + is_active + @"'='' then  a.is_active else '" + is_active + @"' end and 
  a.[prod_id] in (" + prod_ids + @") order by a.[prod_id],a.[order_by]";
                        dtColors = conn.getDataTable(query);
                    }

                    //wishcard
                    DataTable dtWishCards = new DataTable();
                    if (!is_for_list)
                    {
                        query = @"SELECT   a.[product_wish_card_id]
      ,a.[prod_id]
      ,a.[img_url]

      ,a.[order_by] 
   ,a.[created_by]
      ,convert(char(20),a.[created_on],113) as created_on
      ,a.[updated_by]
	   ,convert(char(20),a.[updated_on],113) as updated_on 
      ,a.[is_active]
	  ,esc.name_en as created_by_name
,esu.name_en as updated_by_name 
  FROM  [dbo].[ecom_product_gift_wish_card] a 

  left join ecom_staff esc on 
esc.staff_id=a.[created_by]

left join ecom_staff esu on 
esu.staff_id=a.[updated_by]
where 
   a.[is_active]=case when '" + is_active + @"'='' then  a.is_active else '" + is_active + @"' end and 
  a.[prod_id] in (" + prod_ids + @") order by a.[prod_id],a.[order_by]";
                        dtWishCards = conn.getDataTable(query);
                    }
                    //size
                    DataTable dtSize = new DataTable();
                    if (!is_for_list)
                    {
                        query = @" SELECT  a.[product_size_id]
      ,a.[prod_id]
      ,a.[size_name_en]
      ,a.[size_name_ar]

      ,a.[order_by] 
   ,a.[created_by]
      ,convert(char(20),a.[created_on],113) as created_on
      ,a.[updated_by]
	   ,convert(char(20),a.[updated_on],113) as updated_on 
      ,a.[is_active]
	  ,esc.name_en as created_by_name
,esu.name_en as updated_by_name 
  FROM  [dbo].[ecom_product_size] a 

  left join ecom_staff esc on 
esc.staff_id=a.[created_by]

left join ecom_staff esu on 
esu.staff_id=a.[updated_by]
where 
   a.[is_active]=case when '" + is_active + @"'='' then  a.is_active else '" + is_active + @"' end and 
  a.[prod_id] in (" + prod_ids + @") order by a.[prod_id],a.[order_by]";
                        dtSize = conn.getDataTable(query);
                    }


                    //stock
                    DataTable dtStock = new DataTable();
                    if (!is_for_list)
                    {
                        query = @"SELECT  a.[product_stock_id]
      ,a.[prod_id]
      ,a.[product_size_id]
      ,a.[product_color_id]
      ,a.[stock_count]

      ,a.[order_by] 
   ,a.[created_by]
      ,convert(char(20),a.[created_on],113) as created_on
      ,a.[updated_by]
	   ,convert(char(20),a.[updated_on],113) as updated_on 
      ,a.[is_active]
	  ,esc.name_en as created_by_name
,esu.name_en as updated_by_name 
  FROM  [dbo].[ecom_product_stock] a 

  left join ecom_staff esc on 
esc.staff_id=a.[created_by]

left join ecom_staff esu on 
esu.staff_id=a.[updated_by]
where 
   a.[is_active]=case when '" + is_active + @"'='' then  a.is_active else '" + is_active + @"' end and 
  a.[prod_id] in (" + prod_ids + @") order by a.[prod_id],a.[order_by]";
                        dtStock = conn.getDataTable(query);
                    }

                    //tag
                    DataTable dtTag = new DataTable();

                    if (!is_for_list)
                    {
                        query = @" SELECT  a.[product_tag_id]
      ,a.[prod_id]
      ,a.[tag_id]
   ,a.[created_by]
      ,convert(char(20),a.[created_on],113) as created_on
      ,a.[updated_by]
	   ,convert(char(20),a.[updated_on],113) as updated_on 
      ,a.[is_active]
	  ,esc.name_en as created_by_name
,esu.name_en as updated_by_name 
  FROM  [dbo].[ecom_product_tag] a 

  left join ecom_staff esc on 
esc.staff_id=a.[created_by]

left join ecom_staff esu on 
esu.staff_id=a.[updated_by]
where 
   a.[is_active]=case when '" + is_active + @"'='' then  a.is_active else '" + is_active + @"' end and 
  a.[prod_id] in (" + prod_ids + @") order by a.[prod_id]";
                        dtTag = conn.getDataTable(query);
                    }
                    //-----
                    for (int j = 0; j < res.Count; j++)
                    {
                        //fav
                        /*
                        for (int k = 0; k < dtDataFavs.Rows.Count; k++)
                        {
                            if (res[j].prod_id == dtDataFavs.Rows[k]["prod_id"].ToString().Trim())
                            {
                                res[j].is_fav = "Y";
                                break;
                            }
                        }
                        */

                        //assets
                        res[j].asset = new List<EcomProductAssetsData>();
                        string img_url = "";
                        for (int k = 0; k < dtAssets.Rows.Count; k++)
                        {
                            if (res[j].prod_id == dtAssets.Rows[k]["prod_id"].ToString().Trim())
                            {
                                EcomProductAssetsData this_asset = new EcomProductAssetsData();

                                this_asset.asset_id = dtAssets.Rows[k]["asset_id"].ToString().Trim();
                                this_asset.prod_id = dtAssets.Rows[k]["prod_id"].ToString().Trim();
                                this_asset.product_color_id = dtAssets.Rows[k]["product_color_id"].ToString().Trim();
                                this_asset.type = dtAssets.Rows[k]["type"].ToString().Trim();
                                this_asset.img_url = dtAssets.Rows[k]["img_url"].ToString().Trim();
                                this_asset.is_default = dtAssets.Rows[k]["is_default"].ToString().Trim();
                                this_asset.order_by = dtAssets.Rows[k]["order_by"].ToString().Trim();
                                this_asset.created_by = dtAssets.Rows[k]["created_by"].ToString().Trim();
                                this_asset.created_on = dtAssets.Rows[k]["created_on"].ToString().Trim();
                                this_asset.updated_by = dtAssets.Rows[k]["updated_by"].ToString().Trim();
                                this_asset.updated_on = dtAssets.Rows[k]["updated_on"].ToString().Trim();
                                this_asset.is_active = dtAssets.Rows[k]["is_active"].ToString().Trim();

                                this_asset.created_by_name = dtAssets.Rows[k]["created_by_name"].ToString().Trim();
                                this_asset.updated_by_name = dtAssets.Rows[k]["updated_by_name"].ToString().Trim();

                                if (IsEmpty(img_url))
                                {
                                    if (get_bool_from_yn(this_asset.is_default))
                                    {
                                        if (get_bool_from_yn(this_asset.is_active))
                                        {
                                            if (this_asset.type == "image")
                                                img_url = this_asset.img_url;
                                        }
                                    }
                                }
                                res[j].asset.Add(this_asset);
                            }
                        }
                        if (IsEmpty(img_url))
                        {
                            for (int k = 0; k < res[j].asset.Count; k++)
                            {
                                if (get_bool_from_yn(is_active))
                                {
                                    if (get_bool_from_yn(res[j].asset[k].is_active))
                                    {
                                        if (res[j].asset[k].type == "image")
                                        {
                                            img_url = res[j].asset[k].img_url;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    if (res[j].asset[k].type == "image")
                                    {
                                        img_url = res[j].asset[k].img_url;
                                        break;
                                    }
                                }
                            }
                        }
                        if (IsEmpty(res[j].img_url))
                            res[j].img_url = img_url;



                        //color
                        res[j].color = new List<EcomProductColorData>();
                        if (!is_for_list)
                        {
                            for (int k = 0; k < dtColors.Rows.Count; k++)
                            {
                                if (res[j].prod_id == dtColors.Rows[k]["prod_id"].ToString().Trim())
                                {
                                    EcomProductColorData this_color = new EcomProductColorData();
                                    this_color.product_color_id = dtColors.Rows[k]["product_color_id"].ToString().Trim();
                                    this_color.prod_id = dtColors.Rows[k]["prod_id"].ToString().Trim();
                                    this_color.color_name_en = dtColors.Rows[k]["color_name_en"].ToString().Trim();
                                    this_color.color_name_ar = dtColors.Rows[k]["color_name_ar"].ToString().Trim();
                                    this_color.color_code = dtColors.Rows[k]["color_code"].ToString().Trim();
                                    this_color.color_image_url = dtColors.Rows[k]["color_image_url"].ToString().Trim();
                                    this_color.order_by = dtColors.Rows[k]["order_by"].ToString().Trim();
                                    this_color.created_by = dtColors.Rows[k]["created_by"].ToString().Trim();
                                    this_color.created_on = dtColors.Rows[k]["created_on"].ToString().Trim();
                                    this_color.updated_by = dtColors.Rows[k]["updated_by"].ToString().Trim();
                                    this_color.updated_on = dtColors.Rows[k]["updated_on"].ToString().Trim();
                                    this_color.is_active = dtColors.Rows[k]["is_active"].ToString().Trim();

                                    this_color.created_by_name = dtColors.Rows[k]["created_by_name"].ToString().Trim();
                                    this_color.updated_by_name = dtColors.Rows[k]["updated_by_name"].ToString().Trim();
                                    res[j].color.Add(this_color);
                                }
                            }
                        }

                        //wishcard
                        res[j].wishcard = new List<EcomProductWishCardData>();
                        if (!is_for_list)
                        {
                            for (int k = 0; k < dtWishCards.Rows.Count; k++)
                            {
                                if (res[j].prod_id == dtWishCards.Rows[k]["prod_id"].ToString().Trim())
                                {
                                    EcomProductWishCardData this_item = new EcomProductWishCardData();
                                    this_item.product_wish_card_id = dtWishCards.Rows[k]["product_wish_card_id"].ToString().Trim();
                                    this_item.prod_id = dtWishCards.Rows[k]["prod_id"].ToString().Trim();
                                    this_item.img_url = dtWishCards.Rows[k]["img_url"].ToString().Trim();
                                    this_item.order_by = dtWishCards.Rows[k]["order_by"].ToString().Trim();
                                    this_item.created_by = dtWishCards.Rows[k]["created_by"].ToString().Trim();
                                    this_item.created_on = dtWishCards.Rows[k]["created_on"].ToString().Trim();
                                    this_item.updated_by = dtWishCards.Rows[k]["updated_by"].ToString().Trim();
                                    this_item.updated_on = dtWishCards.Rows[k]["updated_on"].ToString().Trim();
                                    this_item.is_active = dtWishCards.Rows[k]["is_active"].ToString().Trim();
                                    this_item.created_by_name = dtWishCards.Rows[k]["created_by_name"].ToString().Trim();
                                    this_item.updated_by_name = dtWishCards.Rows[k]["updated_by_name"].ToString().Trim();
                                    res[j].wishcard.Add(this_item);
                                }
                            }
                        }

                        //size
                        res[j].size = new List<EcomProductSizeData>();
                        if (!is_for_list)
                        {
                            for (int k = 0; k < dtSize.Rows.Count; k++)
                            {
                                if (res[j].prod_id == dtSize.Rows[k]["prod_id"].ToString().Trim())
                                {
                                    EcomProductSizeData this_item = new EcomProductSizeData();
                                    this_item.product_size_id = dtSize.Rows[k]["product_size_id"].ToString().Trim();
                                    this_item.prod_id = dtSize.Rows[k]["prod_id"].ToString().Trim();
                                    this_item.size_name_en = dtSize.Rows[k]["size_name_en"].ToString().Trim();
                                    this_item.size_name_ar = dtSize.Rows[k]["size_name_ar"].ToString().Trim();


                                    this_item.order_by = dtSize.Rows[k]["order_by"].ToString().Trim();
                                    this_item.created_by = dtSize.Rows[k]["created_by"].ToString().Trim();
                                    this_item.created_on = dtSize.Rows[k]["created_on"].ToString().Trim();
                                    this_item.updated_by = dtSize.Rows[k]["updated_by"].ToString().Trim();
                                    this_item.updated_on = dtSize.Rows[k]["updated_on"].ToString().Trim();
                                    this_item.is_active = dtSize.Rows[k]["is_active"].ToString().Trim();

                                    this_item.created_by_name = dtSize.Rows[k]["created_by_name"].ToString().Trim();
                                    this_item.updated_by_name = dtSize.Rows[k]["updated_by_name"].ToString().Trim();

                                    res[j].size.Add(this_item);
                                }
                            }
                        }
                        //stock
                        res[j].stock = new List<EcomProductStockData>();
                        if (!is_for_list)
                        {
                            for (int k = 0; k < dtStock.Rows.Count; k++)
                            {
                                if (res[j].prod_id == dtStock.Rows[k]["prod_id"].ToString().Trim())
                                {
                                    EcomProductStockData this_item = new EcomProductStockData();

                                    this_item.product_stock_id = dtStock.Rows[k]["product_stock_id"].ToString().Trim();
                                    this_item.prod_id = dtStock.Rows[k]["prod_id"].ToString().Trim();
                                    this_item.product_size_id = dtStock.Rows[k]["product_size_id"].ToString().Trim();
                                    this_item.product_color_id = dtStock.Rows[k]["product_color_id"].ToString().Trim();
                                    this_item.stock_count = dtStock.Rows[k]["stock_count"].ToString().Trim();


                                    this_item.order_by = dtStock.Rows[k]["order_by"].ToString().Trim();
                                    this_item.created_by = dtStock.Rows[k]["created_by"].ToString().Trim();
                                    this_item.created_on = dtStock.Rows[k]["created_on"].ToString().Trim();
                                    this_item.updated_by = dtStock.Rows[k]["updated_by"].ToString().Trim();
                                    this_item.updated_on = dtStock.Rows[k]["updated_on"].ToString().Trim();
                                    this_item.is_active = dtStock.Rows[k]["is_active"].ToString().Trim();

                                    this_item.created_by_name = dtStock.Rows[k]["created_by_name"].ToString().Trim();
                                    this_item.updated_by_name = dtStock.Rows[k]["updated_by_name"].ToString().Trim();

                                    res[j].stock.Add(this_item);
                                }
                            }
                        }

                        //tag
                        res[j].tag = new List<EcomProductTagData>();
                        if (!is_for_list)
                        {
                            for (int k = 0; k < dtTag.Rows.Count; k++)
                            {
                                if (res[j].prod_id == dtTag.Rows[k]["prod_id"].ToString().Trim())
                                {
                                    EcomProductTagData this_item = new EcomProductTagData();
                                    this_item.product_tag_id = dtTag.Rows[k]["product_tag_id"].ToString().Trim();
                                    this_item.prod_id = dtTag.Rows[k]["prod_id"].ToString().Trim();
                                    this_item.tag_id = dtTag.Rows[k]["tag_id"].ToString().Trim();

                                    this_item.created_by = dtTag.Rows[k]["created_by"].ToString().Trim();
                                    this_item.created_on = dtTag.Rows[k]["created_on"].ToString().Trim();
                                    this_item.updated_by = dtTag.Rows[k]["updated_by"].ToString().Trim();
                                    this_item.updated_on = dtTag.Rows[k]["updated_on"].ToString().Trim();
                                    this_item.is_active = dtTag.Rows[k]["is_active"].ToString().Trim();

                                    //this_item.created_by_name = dtTag.Rows[k]["created_by_name"].ToString().Trim();
                                    //this_item.updated_by_name = dtTag.Rows[k]["updated_by_name"].ToString().Trim();
                                    res[j].tag.Add(this_item);
                                }
                            }
                        }

                        //offers
                        res[j].offers = new List<EcomOfferData>();
                        List<string> offer_ids = new List<string>();
                        for (int k = 0; k < dtOffers.Rows.Count; k++)
                        {
                            if (res[j].prod_id == dtOffers.Rows[k]["prod_id"].ToString().Trim() ||
                                res[j].cat_id == dtOffers.Rows[k]["cat_id"].ToString().Trim() ||
                                res[j].brand_id == dtOffers.Rows[k]["brand_id"].ToString().Trim()
                                )
                            {
                                if (offer_ids.Contains(dtOffers.Rows[k]["offer_id"].ToString().Trim()))
                                    continue;
                                offer_ids.Add(dtOffers.Rows[k]["offer_id"].ToString().Trim());
                                EcomOfferData this_offer = new EcomOfferData();
                                this_offer.offer_id = dtOffers.Rows[k]["offer_id"].ToString().Trim();
                                this_offer.prod_id = dtOffers.Rows[k]["prod_id"].ToString().Trim();
                                this_offer.cat_id = dtOffers.Rows[k]["cat_id"].ToString().Trim();
                                this_offer.brand_id = dtOffers.Rows[k]["brand_id"].ToString().Trim();
                                this_offer.offer_dec_en = dtOffers.Rows[k]["offer_dec_en"].ToString().Trim();
                                this_offer.offer_dec_ar = dtOffers.Rows[k]["offer_dec_ar"].ToString().Trim();
                                this_offer.offer_perc = dtOffers.Rows[k]["offer_perc"].ToString().Trim();
                                this_offer.valid_from = dtOffers.Rows[k]["valid_from"].ToString().Trim();
                                this_offer.valid_to = dtOffers.Rows[k]["valid_to"].ToString().Trim();
                                this_offer.coupon_code = dtOffers.Rows[k]["coupon_code"].ToString().Trim();
                                this_offer.is_active = dtOffers.Rows[k]["is_active"].ToString().Trim();
                                res[j].offers.Add(this_offer);
                            }
                            else
                            {
                                for (int a = 0; a < res[j].tag.Count; a++)
                                {
                                    if (res[j].tag[a].tag_id == dtOffers.Rows[k]["tag_id"].ToString().Trim())
                                    {
                                        if (offer_ids.Contains(dtOffers.Rows[k]["offer_id"].ToString().Trim()))
                                            continue;
                                        offer_ids.Add(dtOffers.Rows[k]["offer_id"].ToString().Trim());
                                        EcomOfferData this_offer = new EcomOfferData();
                                        this_offer.offer_id = dtOffers.Rows[k]["offer_id"].ToString().Trim();
                                        this_offer.prod_id = dtOffers.Rows[k]["prod_id"].ToString().Trim();
                                        this_offer.cat_id = dtOffers.Rows[k]["cat_id"].ToString().Trim();
                                        this_offer.brand_id = dtOffers.Rows[k]["brand_id"].ToString().Trim();
                                        this_offer.offer_dec_en = dtOffers.Rows[k]["offer_dec_en"].ToString().Trim();
                                        this_offer.offer_dec_ar = dtOffers.Rows[k]["offer_dec_ar"].ToString().Trim();
                                        this_offer.offer_perc = dtOffers.Rows[k]["offer_perc"].ToString().Trim();
                                        this_offer.valid_from = dtOffers.Rows[k]["valid_from"].ToString().Trim();
                                        this_offer.valid_to = dtOffers.Rows[k]["valid_to"].ToString().Trim();
                                        this_offer.coupon_code = dtOffers.Rows[k]["coupon_code"].ToString().Trim();
                                        this_offer.is_active = dtOffers.Rows[k]["is_active"].ToString().Trim();
                                        res[j].offers.Add(this_offer);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                //=====
                 
            }
            catch (Exception exp)
            {
                Log(log_key, "Exception occured. " + exp.Message, true, exp, true);
            }
            return res;
        }
        public bool get_bool_from_yn(string val)
        {
            if (IsEmpty(val))
                return false;
            if (val.ToString().Trim().ToLower() == "y")
                return true;
            return false;
        }
        public string get_new_offer_price(string prod_quantity, string prod_unit_price, string offer_perc)
        {
            int prod_quantity_int = 0;
            double prod_unit_price_double = 0;
            int offer_perc_int = 0;
            double offer_min_price_double = 0;
            int offer_min_quantity_int = 0;
            if (int.TryParse(prod_quantity, out prod_quantity_int))
            {
                if (double.TryParse(prod_unit_price, out prod_unit_price_double))
                {
                    if (int.TryParse(offer_perc, out offer_perc_int))
                    { 
                        if (prod_quantity_int >= offer_min_quantity_int)
                        {
                            if (prod_unit_price_double >= offer_min_price_double)
                            {
                                double new_price = prod_unit_price_double * (offer_min_price_double / 100);
                                return new_price.ToString("0.###");
                            }
                        }
                    }
                }
            }
            return "";
        }

        public EcomAppData getAppData(string app_identity)
        {
            clsConnectionSQL conn = new clsConnectionSQL();
            EcomAppData res = new EcomAppData();
            string logdata = "getAppData";
            try
            {
                string query = @"SELECT [store_id]
      ,[name_en]
      ,[name_ar]
      ,[name_short_en]
      ,[name_short_ar]
      ,[address_en]
      ,[address_ar]
      ,[phone]
      ,[wa_num]
      ,[email]
      ,[email_tech_support]
      ,[tax_default_perc]
      ,[about_us_en]
      ,[about_us_ar]
      ,[vision_en]
      ,[vision_ar]
      ,[mission_en]
      ,[mission_ar]
      ,[need_epayment]
      ,[current_app_version_for_update]
      ,[privacy_policy_en_url]
      ,[privacy_policy_ar_url]
      ,[refund_policy_en_url]
      ,[refund_policy_ar_url]
      ,[terms_and_conditions_en_url]
      ,[terms_and_conditions_ar_url]
      ,[loc_lat]
      ,[loc_lng]
      ,[currency_code]
      ,[price_ad_per_day_animation]
      ,[price_ad_per_day_static]
      ,[icon_url]
      ,[web_url]
      ,[fb_url]
      ,[instagram_url]
      ,[twitter_url]
      ,[youtube_url]
      ,[linkedin_url]
      ,[app_identity]
      ,[need_all_logs]
      ,[created_by]
      ,[created_on]
      ,[updated_by]
      ,[updated_on]
      ,[is_active]
  FROM  [dbo].[ecom_store] where isnull([is_active],'')='Y' and [app_identity]='" + app_identity + @"'";
                DataTable dtData = conn.getDataTable(query);
                if (dtData.Rows.Count > 0)
                {
                    res.store_id = dtData.Rows[0]["store_id"].ToString().Trim();  
                    res.name_en = dtData.Rows[0]["name_en"].ToString().Trim();
                    res.name_ar = dtData.Rows[0]["name_ar"].ToString().Trim();
                    res.name_short_en = dtData.Rows[0]["name_short_en"].ToString().Trim();
                    res.name_short_ar = dtData.Rows[0]["name_short_ar"].ToString().Trim();
                    res.address_en = dtData.Rows[0]["address_en"].ToString().Trim();
                    res.address_ar = dtData.Rows[0]["address_ar"].ToString().Trim();
                    res.phone = dtData.Rows[0]["phone"].ToString().Trim();
                    res.wa_num = dtData.Rows[0]["wa_num"].ToString().Trim(); 
                    res.tax_default_perc = dtData.Rows[0]["tax_default_perc"].ToString().Trim();
                    res.about_us_en = dtData.Rows[0]["about_us_en"].ToString().Trim();
                    res.about_us_ar = dtData.Rows[0]["about_us_ar"].ToString().Trim();
                    res.vision_en = dtData.Rows[0]["vision_en"].ToString().Trim();
                    res.vision_ar = dtData.Rows[0]["vision_ar"].ToString().Trim();
                    res.need_epayment = dtData.Rows[0]["need_epayment"].ToString().Trim();
                    res.current_app_version_for_update = dtData.Rows[0]["current_app_version_for_update"].ToString().Trim();
                    res.privacy_policy_en_url = dtData.Rows[0]["privacy_policy_en_url"].ToString().Trim();
                    res.privacy_policy_ar_url = dtData.Rows[0]["privacy_policy_ar_url"].ToString().Trim();
                    res.refund_policy_en_url = dtData.Rows[0]["refund_policy_en_url"].ToString().Trim();
                    res.refund_policy_ar_url = dtData.Rows[0]["refund_policy_ar_url"].ToString().Trim();
                    res.terms_and_conditions_en_url = dtData.Rows[0]["terms_and_conditions_en_url"].ToString().Trim();
                    res.terms_and_conditions_ar_url = dtData.Rows[0]["terms_and_conditions_ar_url"].ToString().Trim();
                    res.loc_lat = dtData.Rows[0]["loc_lat"].ToString().Trim();
                    res.loc_lng = dtData.Rows[0]["loc_lng"].ToString().Trim();
                    res.currency_code = dtData.Rows[0]["currency_code"].ToString().Trim();
                    res.icon_url = dtData.Rows[0]["icon_url"].ToString().Trim();
                    res.web_url = dtData.Rows[0]["web_url"].ToString().Trim();
                    res.fb_url = dtData.Rows[0]["fb_url"].ToString().Trim();
                    res.instagram_url = dtData.Rows[0]["instagram_url"].ToString().Trim();
                    res.twitter_url = dtData.Rows[0]["twitter_url"].ToString().Trim();
                    res.youtube_url = dtData.Rows[0]["youtube_url"].ToString().Trim();
                    res.linkedin_url = dtData.Rows[0]["linkedin_url"].ToString().Trim(); 
                    res.need_all_logs = dtData.Rows[0]["need_all_logs"].ToString().Trim(); 
                }
            }
            catch (Exception exp)
            {
                Log(logdata, exp.Message, true, exp, true);
            }
            return res;
        }
        public bool setDeviceStaff1(string store_id, string dev_id, string staff_id, string dev_type, string lang_code, string token, string reg_id, out string res_msg)
        {
            res_msg = "";
            clsConnectionSQL conn = new clsConnectionSQL();
            string query = @"SELECT * FROM [dbo].[ecom_device_staff] where [dev_id]='" + dev_id + "' and [dev_type]='" + dev_type + "' and [store_id]='" + store_id + "'";
            DataTable dtData = conn.getDataTable(query);
            if (dtData.Rows.Count > 0)
            {
                if (!checkIsTrue(dtData.Rows[0]["is_active"].ToString()))
                {
                    res_msg = "Your access is restricted. Please contact app administrator(" + GetConfig("ecom_email_support") + ")";
                    return false;
                }
                query = @"UPDATE [dbo].[ecom_device_staff]
   SET  [staff_id] =" + get_value_with_quote_for_sql(staff_id) + @"
      ,[dev_type] = '" + dev_type + @"'
      ,[lang_code] = '" + lang_code + @"'
      ,[token] ='" + token + @"'
      ,[reg_id] = '" + reg_id + @"'
      ,[last_login_time] =getdate()
      ,[is_online] = 'Y' 
      ,[updated_by] =" + get_value_with_quote_for_sql(staff_id) + @"
      ,[updated_on] =getdate() 
,[store_id] ='" + store_id + @"' 
 WHERE  [dev_id] = '" + dev_id + @"'";
            }
            else
            {
                query = @"INSERT INTO [dbo].[ecom_device_staff]
           ([dev_id]
           ,[staff_id]
           ,[dev_type]
           ,[lang_code]
           ,[token]
           ,[reg_id]
           ,[last_login_time]
           ,[is_online]
           ,[created_by]
           ,[created_on] 
           ,[is_active]
,[store_id])
     VALUES
           ('" + dev_id + @"'
           ," + get_value_with_quote_for_sql(staff_id) + @"
              ,'" + dev_type + @"' 
           ,'" + lang_code + @"'  
            ,'" + token + @"'  
            ,'" + reg_id + @"' 
           ,getdate()
           ,'Y'
           ," + get_value_with_quote_for_sql(staff_id) + @"
           ,getdate()
           ,'Y'
,'" + store_id + @"')";
            }
            return conn.ExecuteNonQuery(query);
        }
        public bool setDevice(string store_id, string dev_id, string cust_id, string staff_id, string role_type, string dev_type, string lang_code, string token, string reg_id, out string res_msg)
        {
            res_msg = "";
            clsConnectionSQL conn = new clsConnectionSQL();
            string query = @"SELECT * FROM [dbo].[ecom_device] where [dev_id]='" + dev_id + "'  and [dev_type]='" + dev_type + "' and [store_id]='" + store_id + "'";
            DataTable dtData = conn.getDataTable(query);
            if (dtData.Rows.Count > 0)
            {
                if (!checkIsTrue(dtData.Rows[0]["is_active"].ToString()))
                {
                    res_msg = "Your access is restricted. Please contact app administrator(" + GetConfig("ecom_email_support") + ")";
                    return false;
                }
                query = @"UPDATE [dbo].[ecom_device]
   SET  [cust_id] ="+ get_value_with_quote_for_sql (cust_id) + @"
,[staff_id] =" + get_value_with_quote_for_sql(staff_id) + @"
      ,[dev_type] = '" + dev_type + @"'
  ,[role_type] = '" + role_type + @"'
      ,[lang_code] = '" + lang_code + @"'
      ,[token] ='" + token + @"'
      ,[reg_id] = '" + reg_id + @"'
      ,[last_login_time] =getdate()
      ,[is_online] = 'Y' 
      ,[updated_by] =" + get_value_with_quote_for_sql(cust_id) + @"
      ,[updated_on] =getdate() 
,[store_id] ='" + store_id + @"' 
 WHERE  [dev_id] = '" + dev_id + @"'";
            }
            else
            {
                query = @"INSERT INTO [dbo].[ecom_device]
           ([dev_id]
           ,[cust_id]
 ,[staff_id]
,[role_type]
           ,[dev_type]
           ,[lang_code]
           ,[token]
           ,[reg_id]
           ,[last_login_time]
           ,[is_online]
           ,[created_by]
           ,[created_on] 
           ,[is_active]
,[store_id])
     VALUES
           ('" + dev_id + @"'
           ," + get_value_with_quote_for_sql(cust_id) + @"
   ," + get_value_with_quote_for_sql(staff_id) + @"
  ,'" + role_type + @"' 
              ,'" + dev_type + @"' 
           ,'" + lang_code + @"'  
            ,'" + token + @"'  
            ,'" + reg_id + @"' 
           ,getdate()
           ,'Y'
           ," + get_value_with_quote_for_sql(cust_id) + @"
           ,getdate()
           ,'Y'
,'" + store_id + @"')";
            }
            return conn.ExecuteNonQuery(query);
        }
        public string get_value_with_quote_for_sql(string value)
        {
            if (IsEmpty(value))
                return "null";
            else
                return "'" + value + "'";
        }
        public bool IsEmpty(string value)
        {
            if (get_value(value).Trim().Length == 0)
                return true;
            return false;
        }
        public bool checkIsTrue(string value)
        {
            if (value == null)
                return false;
            if (value.ToString().Trim().ToLower() == "y")
                return true;
            return false;
        }
        public string getJsonObjectToString(object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            return FormatJson(json);
        }
        public dynamic getJsonStringToObject(string json_str)
        {
            dynamic obj = null;
            try
            {
                if (json_str == null)
                    return obj;
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer()
                {
                    MaxJsonLength = Int32.MaxValue // specify length as per your business requirement
                };
                serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
                obj = serializer.Deserialize(json_str, typeof(object));
            }
            catch (Exception exp)
            {
                Log("getJsonObject", exp.Message, true, exp);
            }
            return obj;
        }
        private string FormatJson(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Newtonsoft.Json.Formatting.Indented);
        }
        public string get_value(string val)
        {
            if (val == null)
                return "";
            return val.ToString().Trim();
        }
        public void Log(string action, string logDesc, bool isError, Exception expp)
        {
            Log("NA", "0", action, logDesc, isError, expp, isError);
        }
        public void Log(string action, string logDesc, bool isError, Exception expp, bool need_email)
        {
            Log("NA", "0", action, logDesc, isError, expp, need_email);
        }
        public void Log(string dev_id, string store_id, string action, string logDesc, bool isError, Exception expp, bool need_email)
        {
            try
            {
                if (logDesc == "Thread was being aborted.")
                    return;
                string log_msg = GetLogMessage(dev_id, store_id, action, logDesc, isError, expp);
                if(GetConfig("need_return_err").ToLower().Trim()=="y")
                {
                    clsEcom.err +=", "+ action + "-" + log_msg;
                }

                if (need_email)
                {
                    if (send_error(action, log_msg))
                    {

                    }
                }
                clsConnectionSQL conn = new clsConnectionSQL();
                string is_error = "N";
                if (isError)
                    is_error = "Y";
                string query = "";
                log_msg = log_msg.Substring(0, 2000 - 1);
                log_msg = log_msg.Replace("'", "");
                log_msg = log_msg.Replace("\"", "");
                log_msg = log_msg.Replace("&", "");
                log_msg = log_msg.Replace("%", "");
                if (log_msg.Length < 2000)
                {
                    query = @"INSERT INTO [dbo].[ecom_log] 
           ([dev_id]
           ,[is_error]
           ,[action]
           ,[description]
           ,[store_id]
           ,[log_time])
     VALUES
           ('" + dev_id + @"'
           ,'" + is_error + @"'
           ,'" + action + @"'
          ,'" + log_msg + @"'
           ," + get_value_with_quote_for_sql(store_id) + @"
           ,getdate())";
                    if (!conn.ExecuteNonQuery(query))
                    {
                        if (send_error("LogSave", "Save log in Database failed(query:" + query + ")"))
                        {

                        }
                    }
                }
                if (isError)
                {
                    int limit = 5;
                    if (!int.TryParse(GetConfig("ecom_log_table_days_limit"), out limit))
                        limit = 5;
                    query = @"delete [dbo].[ecom_log] where log_time<cast('" + DateTime.Now.AddDays(-limit).ToString("dd MMM yyyy") + "' as datetime)";
                    conn.ExecuteNonQuery(query);
                }
            }
            catch (Exception exp)
            {

            }
        }
        public bool send_error(string action, string details)
        {
            try
            {
                string ecom_email_to_error_sep_by_comma = GetConfig("ecom_email_to_error_sep_by_comma");
                string ecom_app_name = GetConfig("ecom_app_name");
                string sub = "TECH_ALERT|" + action;
                string msg = "Hi," + Environment.NewLine;
                msg += "Got error in the server of app " + ecom_app_name + ". Below the details;" + Environment.NewLine;
                msg += "----------" + Environment.NewLine;
                msg += details + Environment.NewLine;
                msg += "----------" + Environment.NewLine;
                msg += "This is an auto-generated email. Please do not reply." + Environment.NewLine;
                return send_email_gen(ecom_email_to_error_sep_by_comma, sub, msg);

            }
            catch (Exception exp)
            {
                //  Log("send_email", exp.Message + "(body:" + MailBody + ")", true, exp);
            }
            return false;
        }
        public bool send_email_gen(string to, string sub, string mail_body)
        {
            try
            {
                List<string> tos = new List<string>();
                string[] tos_arr = to.Split(',');
                tos = new List<string>(tos_arr);
                return send_email_gen(tos, sub, mail_body);
            }
            catch (Exception exp)
            {
                //  Log("send_email", exp.Message + "(body:" + MailBody + ")", true, exp);
            }
            return false;
        }
        public bool send_email_gen(List<string> tos, string sub, string mail_body)
        {
            try
            {
                string ecom_email_no_reply_smtp = GetConfig("ecom_email_no_reply_smtp");
                string ecom_email_no_reply_smtp_port = GetConfig("ecom_email_no_reply_smtp_port");
                string ecom_email_no_reply_from = GetConfig("ecom_email_no_reply_from");
                string ecom_email_no_reply_from_pw = GetConfig("ecom_email_no_reply_from_pw");
                string ecom_app_name = GetConfig("ecom_app_name");
                AttachmentCollection atts = new MailMessage().Attachments;
                clsCommon common = new clsCommon();
                return common.send_email(false, tos, sub, mail_body, atts, ecom_email_no_reply_from, ecom_email_no_reply_from_pw, ecom_app_name, ecom_email_no_reply_smtp, Convert.ToInt32(ecom_email_no_reply_smtp_port));
            }
            catch (Exception exp)
            {
                //  Log("send_email", exp.Message + "(body:" + MailBody + ")", true, exp);
            }
            return false;
        }
        
       
        public string GetConfig(string key)
        {
            string res = "";
            try
            {
                res = System.Configuration.ConfigurationManager.AppSettings.Get(key);
            }
            catch (Exception exp)
            {
                Log("GetConfig", exp.Message, true, exp, false);
            }
            return res;
        }
        private string GetLogMessage(string device_id, string store_id, string action, string logDesc, bool isError, Exception expp)
        {
            string currentLogData = "";
            string expDetail = "";
            string app_name = "";
            if (isError)
            {
                try
                {
                    app_name = System.AppDomain.CurrentDomain.FriendlyName;
                }
                catch (Exception exp)
                {

                }

                if (expp != null)
                {
                    expDetail = "at line:" + GetLineNumber(expp);
                    expDetail += expp.ToString();
                    currentLogData += DateTime.Now.ToString() + " - DevId: " + device_id + " - store_id: " + store_id + " - " + app_name + " - Error : " + action + " - " + logDesc + " - (More: " + expDetail + ")";
                }
                else
                    currentLogData += DateTime.Now.ToString() + " - DevId: " + device_id + " - store_id: " + store_id + " - " + app_name + " - Error : " + action + " - " + logDesc + "";
            }
            else
                currentLogData += DateTime.Now.ToString() + " - DevId: " + device_id + " - store_id: " + store_id + " - " + app_name + " - Status : " + action + " - " + logDesc;
            return currentLogData.Replace("'", " ").Replace("\"", " ").Replace("--", " ").Replace("-", " ").Replace("&", " ");
        }
        public int GetLineNumber(Exception ex)
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
        public bool sendPnToBrand(string store_id, string brand_id, string title_en, string title_ar, string msg_en, string msg_ar, string destination_type, string destination_value)
        {
            return sendPn(store_id, false, false, false, false, brand_id, "", "", title_en, title_ar, msg_en, msg_ar, destination_type, destination_value);
        }
        public bool sendPnToCustomer(string store_id, string cust_id, string title_en, string title_ar, string msg_en, string msg_ar, string destination_type, string destination_value)
        {
            return sendPn(store_id, false, false, false, false,"", "", cust_id, title_en, title_ar, msg_en, msg_ar, destination_type, destination_value);
        }
        public bool sendPnToStaff(string store_id, string staff_id, string title_en, string title_ar, string msg_en, string msg_ar, string destination_type, string destination_value)
        {
            return sendPn(store_id, false, false, false, false,"",  staff_id,"",  title_en,  title_ar,  msg_en,  msg_ar,  destination_type,  destination_value);
        }
        public bool sendPnToAllAdmins(string store_id, string title_en, string title_ar, string msg_en, string msg_ar, string destination_type, string destination_value)
        {
            return sendPn(store_id, false, false, false, true,"", "", "", title_en, title_ar, msg_en, msg_ar, destination_type, destination_value);
        }
        public bool sendPn(string store_id, bool is_to_all_cust, bool is_to_all_brands, bool is_to_all_admin, bool is_to_all_delivery_agents, string brand_id, string staff_id, string cust_id, string title_en, string title_ar, string msg_en, string msg_ar, string destination_type, string destination_value)
        {
            //vintest todo
            return true;
        }
        public bool send_pn_firebase(string reg_id, string title, string body, string key1_value, string key2_value)
        {
            try
            {
                if (IsEmpty(body))
                    return false;
                string SERVER_API_KEY = GetConfig("firebase_api_key");
                var SENDER_ID = GetConfig("firebase_sender_id");
                WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.Method = "post";
                tRequest.Headers.Add(string.Format("Authorization: key={0}", SERVER_API_KEY));
                tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));
                tRequest.ContentType = "application/json";
                var payload = new
                {
                    to = reg_id,
                    priority = "high",
                    content_available = true,
                    notification = new
                    {
                        body = body,
                        title = title,
                        badge = 1
                    },
                    data = new
                    {
                        key1 = key1_value,
                        key2 = key2_value
                    }

                };

                string postbody = Newtonsoft.Json.JsonConvert.SerializeObject(payload).ToString();
                Byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
                tRequest.ContentLength = byteArray.Length;
                using (Stream dataStream = tRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    using (WebResponse tResponse = tRequest.GetResponse())
                    {
                        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                        {
                            if (dataStreamResponse != null) using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                {
                                    String sResponseFromServer = tReader.ReadToEnd();
                                    return true;
                                }
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                Log("send_pn_firebase", exp.Message + "(body:" + body + ", reg_id:" + reg_id + ")", true, exp);
            }
            return false;
        }
        public string getStatusFromKey(string key)
        {
            string stat = key.ToLower().Replace("_", " ");
            return ToTitleCase(stat);
        }
        public string ToTitleCase(string txt)
        {
            CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;
            return textInfo.ToTitleCase(txt);
        }
        public bool IsEmptyAndContainsSpecial(string val, string key_name, out string res)
        {
            res = "";
            if (IsEmpty(val))
            {
                res = key_name + " should not be empty";
                return false;
            }
            clsEcomAuth auth = new clsEcomAuth();
            if (auth.IsContainsSpecialChars(val))
            {
                res = key_name + " should not contain special characters";
                return false;
            }
            return true;
        }
        public bool IsEmpty(string val, string key_name, out string res)
        {
            res = "";
            if (IsEmpty(val))
            {
                res = key_name + " should not be empty";
                return false;
            } 
            return true;
        }
        public bool IsContainsSpecial(string val, string key_name, out string res)
        {
            res = "";
            if (IsEmpty(val))
            { 
                return true;
            }
            clsEcomAuth auth = new clsEcomAuth();
            if (auth.IsContainsSpecialChars(val))
            {
                res = key_name + " should not contain special characters";
                return false;
            }
            return true;
        }
    } 
}