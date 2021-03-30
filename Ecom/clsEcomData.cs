using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ecomserv.Ecom
{
    public class clsEcomData
    {
    }
    [DataContract]
    public class EcomData
    {
        [DataMember]
        public string api_key { get; set; }
        [DataMember]
        public string app_identity { get; set; }
        [DataMember]
        public string token { get; set; }
        [DataMember]
        public string token_new { get; set; }
        [DataMember]
        public string cust_id { get; set; }
        [DataMember]
        public string staff_id { get; set; }
        [DataMember]
        public string role_type { get; set; }
        [DataMember]
        public string store_id { get; set; }

        [DataMember]
        public string dev_id { get; set; }
        [DataMember]
        public string dev_type { get; set; }
        [DataMember]
        public string reg_id { get; set; }
        [DataMember]
        public string lang_code { get; set; }
        [DataMember]
        public string status { get; set; }

        [DataMember]
        public string status_msg { get; set; }

        [DataMember]
        public string flag { get; set; }

        [DataMember]
        public string data { get; set; }
        [DataMember]
        public string prod_id { get; set; }
        [DataMember]
        public string time_offset { get; set; }
    }
    public class EcomUserData
    {
        public string user_type { get; set; }
        public EcomCustomerData customer { get; set; }
        public EcomBrandData brand { get; set; }
        public EcomStaffData staff { get; set; }
    }
    public class EcomCustomerData
    {
        public string cust_id { get; set; }
        public string store_id { get; set; }
        public string email { get; set; }
        public string is_email_verified { get; set; }
        public string photo_url { get; set; }
        public string phone_number { get; set; }
        public string contact_name { get; set; }
        public string dob { get; set; }
        public string gender { get; set; }
        public string created_by { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }

    }
    public class EcomBrandData
    {
        public string brand_id { get; set; }
        public string store_id { get; set; }
        public string email { get; set; }
        public string name_owner_en { get; set; }
        public string name_owner_ar { get; set; }
        public string name_brand_en { get; set; }
        public string name_brand_ar { get; set; }
        public string brand_desc_en { get; set; }
        public string brand_desc_ar { get; set; }
        public string business_sector_id { get; set; }
        public string phone { get; set; }
        public string mobile_whatsapp { get; set; }
        public string reg_message_to_admin { get; set; }
        public string icon_url { get; set; }
        public string loc_lat { get; set; }
        public string loc_lng { get; set; }
        public string admin_approval_status { get; set; }
        public string admin_approval_remarks { get; set; }
        public string need_approval_to_add_prod { get; set; }
        public string order_by { get; set; }
        public string allow_delivery_within_km { get; set; }
        public string created_by { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }
        public string f_name_owner { get; set; }
        public string _name_owner { get; set; }
        public string usiness_address_line_1 { get; set; }
        public string usiness_address_line_2 { get; set; }
        public string usiness_address_city { get; set; }
        public string usiness_address_po_box_no { get; set; }
        public string usiness_address_postcode { get; set; }
        public string usiness_address_country { get; set; }

        public string l_name_owner { get; set; }
        public string business_address_line_1 { get; set; }
        public string business_address_line_2 { get; set; }
        public string business_address_city { get; set; }
        public string business_address_po_box_no { get; set; }
        public string business_address_country { get; set; }
        public string business_address_postcode { get; set; }
        public string website { get; set; }
        public string social_media_url { get; set; }

    }
    public class EcomStaffData
    {
        public string staff_id { get; set; }
        public string brand_id { get; set; }
        public string store_id { get; set; }
        public string role_type { get; set; }
        public string email { get; set; }
        public string is_email_verified { get; set; }
        public string name_en { get; set; }
        public string name_ar { get; set; }
        public string mobile_num { get; set; }
        public string wa_num { get; set; }
        public string address_en { get; set; }
        public string address_ar { get; set; }
        public string loc_lat { get; set; }
        public string loc_lng { get; set; }
        public string created_by { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }
        public string f_name { get; set; }
        public string l_name { get; set; }
        public string is_firebase_account_created { get; set; }
        public string firebase_initial_pw { get; set; }



    }
    public class EcomAppData
    {
        public string store_id { get; set; }
        public string name_en { get; set; }
        public string name_ar { get; set; }
        public string name_short_en { get; set; }
        public string name_short_ar { get; set; }
        public string address_en { get; set; }
        public string address_ar { get; set; }
        public string phone { get; set; }
        public string wa_num { get; set; }
        public string email { get; set; }
        public string email_tech_support { get; set; }
        public string tax_default_perc { get; set; }
        public string about_us_en { get; set; }
        public string about_us_ar { get; set; }
        public string vision_en { get; set; }
        public string vision_ar { get; set; }
        public string mission_en { get; set; }
        public string mission_ar { get; set; }
        public string need_epayment { get; set; }
        public string current_app_version_for_update { get; set; }
        public string privacy_policy_en_url { get; set; }
        public string privacy_policy_ar_url { get; set; }
        public string refund_policy_en_url { get; set; }
        public string refund_policy_ar_url { get; set; }
        public string terms_and_conditions_en_url { get; set; }
        public string terms_and_conditions_ar_url { get; set; }
        public string loc_lat { get; set; }
        public string loc_lng { get; set; }
        public string currency_code { get; set; }
        public string price_ad_per_day_animation { get; set; }
        public string price_ad_per_day_static { get; set; }
        public string icon_url { get; set; }
        public string web_url { get; set; }
        public string fb_url { get; set; }
        public string instagram_url { get; set; }
        public string twitter_url { get; set; }
        public string youtube_url { get; set; }
        public string linkedin_url { get; set; }
        public string app_identity { get; set; }
        public string need_all_logs { get; set; }
        public string created_by { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }

    }

    public class EcomSearchCatItem
    {
        public string id { get; set; }
        public string type { get; set; }
        public string name_en { get; set; }
        public string name_ar { get; set; }
    }
    public class EcomAdOlaData
    {
        public string ad_id { get; set; }
        public string brand_id { get; set; }
        public string txn_id { get; set; }
        public string is_animation { get; set; }
        public string img_url { get; set; }
        public string date_from { get; set; }
        public string date_to { get; set; }
        public string is_paid { get; set; }
        public string destination_type { get; set; }
        public string destination_value { get; set; }
        public string admin_approval_status { get; set; }
        public string admin_approval_remarks { get; set; }
        public string order_by { get; set; }
        public string created_by { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }
    }
    public class EcomBusinessSectorData
    {
        public string sector_id { get; set; }
        public string store_id { get; set; }
        public string sector_name_en { get; set; }
        public string sector_name_ar { get; set; }
        public string order_by { get; set; }
        public string created_by { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }
    }
    public class EcomProductData
    {
        public string prod_id { get; set; }
        public string brand_id { get; set; }
        public string brand_name_en { get; set; }
        public string brand_name_ar { get; set; }
        public string cat_id { get; set; }
        public string cat_name_en { get; set; }
        public string cat_name_ar { get; set; }
        public string name_en { get; set; }
        public string name_ar { get; set; }
        public string name_desc_en { get; set; }
        public string name_desc_ar { get; set; }
        public string quantity { get; set; }


        public string unit_price_old { get; set; }
        public string unit_price { get; set; }
        public string is_custom_size_available { get; set; }
        public string custom_size_img_url { get; set; }
        public string custom_size_description { get; set; }
        public string custom_size_field1 { get; set; }
        public string custom_size_field2 { get; set; }
        public string custom_size_field3 { get; set; }
        public string custom_size_field4 { get; set; }
        public string custom_size_field5 { get; set; }
        public string custom_size_field6 { get; set; }
        public string custom_size_field7 { get; set; }
        public string custom_size_field8 { get; set; }
        public string custom_size_field9 { get; set; }
        public string custom_size_field10 { get; set; }
        public string is_gift_available { get; set; }
        public string is_surprise_gift_available { get; set; }
        public string size_chart_img_url { get; set; }
        public string age_from { get; set; }
        public string age_to { get; set; }
        public string admin_approval_status { get; set; }
        public string admin_approval_remarks { get; set; }
        public string order_by { get; set; }
        public string created_by { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }

        public string created_by_name { get; set; }
        public string updated_by_name { get; set; }
        public string last_row_index { get; set; }
        public string is_fav { get; set; }
        public string img_url { get; set; }


        public string custome_size_extra_price { get; set; }
        public string custome_size_extra_price_perc { get; set; }
        public string gift_extra_price { get; set; }
        public string gift_extra_price_perc { get; set; }
        public string surprise_gift_extra_price { get; set; }
        public string surprise_gift_extra_price_perc { get; set; }
        public string wish_card_extra_price { get; set; }
        public string wish_card_extra_price_perc { get; set; }


        public EcomOfferData offer { get; set; }
        public List<EcomOfferData> offers { get; set; }
        public List<EcomProductAssetsData> asset { get; set; }

        public List<EcomProductColorData> color { get; set; }

        public List<EcomProductWishCardData> wishcard { get; set; }

        public List<EcomProductSizeData> size { get; set; }
        public List<EcomProductStockData> stock { get; set; }
        public List<EcomProductTagData> tag { get; set; }
    }
    public class EcomProductTagData
    {
        public string product_tag_id { get; set; }
        public string prod_id { get; set; }
        public string tag_id { get; set; }

        public string created_by { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }

        public string created_by_name { get; set; }
        public string updated_by_name { get; set; }
        public string last_row_index { get; set; }
    }
    public class EcomProductStockData
    {
        public string product_stock_id { get; set; }
        public string prod_id { get; set; }
        public string product_size_id { get; set; }
        public string product_color_id { get; set; }
        public string stock_count { get; set; }


        public string order_by { get; set; }
        public string created_by { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }

        public string created_by_name { get; set; }
        public string updated_by_name { get; set; }
        public string last_row_index { get; set; }
    }
    public class EcomProductSizeData
    {
        public string product_size_id { get; set; }
        public string prod_id { get; set; }
        public string size_name_en { get; set; }
        public string size_name_ar { get; set; }


        public string order_by { get; set; }
        public string created_by { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }

        public string created_by_name { get; set; }
        public string updated_by_name { get; set; }
        public string last_row_index { get; set; }
    }
    public class EcomProductWishCardData
    {
        public string product_wish_card_id { get; set; }
        public string prod_id { get; set; }
        public string img_url { get; set; }
        public string order_by { get; set; }
        public string created_by { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }

        public string created_by_name { get; set; }
        public string updated_by_name { get; set; }
        public string last_row_index { get; set; }
    }
    public class EcomProductColorData
    {
        public string product_color_id { get; set; }
        public string prod_id { get; set; }
        public string color_name_en { get; set; }
        public string color_name_ar { get; set; }
        public string color_code { get; set; }
        public string color_image_url { get; set; }
        public string order_by { get; set; }
        public string created_by { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }

        public string created_by_name { get; set; }
        public string updated_by_name { get; set; }
        public string last_row_index { get; set; }
    }
    public class EcomProductAssetsData
    {
        public string asset_id { get; set; }
        public string prod_id { get; set; }
        public string product_color_id { get; set; }
        public string type { get; set; }
        public string img_url { get; set; }
        public string is_default { get; set; }
        public string order_by { get; set; }
        public string created_by { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }

        public string created_by_name { get; set; }
        public string updated_by_name { get; set; }
        public string last_row_index { get; set; }
    }
    public class EcomOfferData
    {
        public string offer_id { get; set; }
        public string prod_id { get; set; }
        public string cat_id { get; set; }
        public string brand_id { get; set; }
        public string offer_dec_en { get; set; }
        public string offer_dec_ar { get; set; }
        public string offer_perc { get; set; }
        public string valid_from { get; set; }
        public string valid_to { get; set; }
        public string coupon_code { get; set; }
        public string created_by { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }

        public string created_by_name { get; set; }
        public string updated_by_name { get; set; }
        public string last_row_index { get; set; }
    }


    public class EcomCartItem
    {
        public string cart_id { get; set; }
        public string prod_id { get; set; }
        public string cust_id { get; set; }
        public string dev_id { get; set; }
        public string quantity { get; set; }
        public string add_date { get; set; }
        public string offer_id_for_coupen { get; set; }
        public string product_color_id { get; set; }
        public string product_size_id { get; set; }
        public string is_custom_size { get; set; }
        public string is_gift { get; set; }
        public string is_surprise_gift { get; set; }
        public string is_need_gift_wish_card { get; set; }
        public string gift_wish_card_url { get; set; }
        public string custom_size_field1_value { get; set; }
        public string custom_size_field2_value { get; set; }
        public string custom_size_field3_value { get; set; }
        public string custom_size_field4_value { get; set; }
        public string custom_size_field5_value { get; set; }
        public string custom_size_field6_value { get; set; }
        public string custom_size_field7_value { get; set; }
        public string custom_size_field8_value { get; set; }
        public string custom_size_field9_value { get; set; }
        public string custom_size_field10_value { get; set; }
        public string custom_size_color_code { get; set; }
        public string custom_size_message { get; set; }
        public string created_by { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }
        public EcomProductData product { get; set; }
    }
    public class EcomAuthLoginItem
    {
        public string cust_id { get; set; }
        public string store_id { get; set; }
        public string email { get; set; }
        public string is_email_verified { get; set; }
        public string photo_url { get; set; }
        public string phone_number { get; set; }
        public string contact_name { get; set; }
        public string dob { get; set; }
        public string gender { get; set; }
        public string is_ok { get; set; }
    }
    public class EcomAddressItem
    {
        public string address_id { get; set; }
        public string cust_id { get; set; }
        public string address_type { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string state_region { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public string country { get; set; }
        public string landmark { get; set; }
        public string mobile_num { get; set; }
        public string wa_num { get; set; }
        public string email { get; set; }
        public string is_email_verified { get; set; }
        public string is_mobile_verified { get; set; }
        public string loc_lat { get; set; }
        public string loc_lng { get; set; }
        public string instruction { get; set; }
        public string time_preference { get; set; }
        public string is_default { get; set; }
    }
    public class EcomOrderItem
    {

        public string order_id { get; set; }
        public string cust_id { get; set; }
        public string txn_id { get; set; }
        public string staff_id_updated_by { get; set; }
        public string tot_price { get; set; }
        public string order_date { get; set; }
        public string order_type { get; set; }
        public string is_req_for_cancel_by_customer { get; set; }
        public string current_status_key { get; set; }
        public string is_paid { get; set; }
        public string staff_id { get; set; }
        public string last_row_index { get; set; }
        public EcomAddressItem delivery_address { get; set; }
        public EcomAddressItem billing_address { get; set; }

        public List<EcomProductOrderItem> product_order { get; set; }
    }


    public class EcomProductOrderItem
    {
        public string prod_order_id { get; set; }
        public string order_id { get; set; }
        public string prod_id { get; set; }
        public string unit_price { get; set; }
        public string discount_perc { get; set; }
        public string custome_size_extra_price { get; set; }
        public string custome_size_extra_price_perc { get; set; }
        public string gift_extra_price { get; set; }
        public string gift_extra_price_perc { get; set; }
        public string surprise_gift_extra_price { get; set; }
        public string surprise_gift_extra_price_perc { get; set; }
        public string wish_card_extra_price { get; set; }
        public string wish_card_extra_price_perc { get; set; }
        public string quantity { get; set; }
        public string product_color_id { get; set; }
        public string product_size_id { get; set; }
        public string is_custom_size { get; set; }
        public string custom_size_field1_value { get; set; }
        public string custom_size_field2_value { get; set; }
        public string custom_size_field3_value { get; set; }
        public string custom_size_field4_value { get; set; }
        public string custom_size_field5_value { get; set; }
        public string custom_size_field6_value { get; set; }
        public string custom_size_field7_value { get; set; }
        public string custom_size_field8_value { get; set; }
        public string custom_size_field9_value { get; set; }
        public string custom_size_field10_value { get; set; }
        public string custom_size_color_code { get; set; }
        public string custom_size_message { get; set; }
        public string is_gift { get; set; }
        public string is_surprise_gift { get; set; }
        public string is_need_gift_wish_card { get; set; }
        public string gift_wish_card_url { get; set; }
        public string created_by { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }

        public EcomProductData product { get; set; }
    }
    public class EcomRoleItem
    {
        public string role_id { get; set; }
        public string store_id { get; set; }
        public string role_name { get; set; }
        public string is_admin { get; set; }
        public string created_by { get; set; }
        public string created_by_name { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_by_name { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }
        public List<EcomRoleFormItem> role_forms = new List<EcomRoleFormItem>();
    }
    public class EcomRoleFormItem
    {
        public string role_form_id { get; set; }
        public string role_id { get; set; }
        public string form_key { get; set; }
        public string can_edit { get; set; }
        public string can_delete { get; set; }
        public string created_by { get; set; }

        public string created_by_name { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_by_name { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }
    }

    public class EcomStaffItem
    {
        public string staff_id { get; set; }
        public string brand_id { get; set; }
        public string store_id { get; set; }
        public string role_type { get; set; }
        public string email { get; set; }
        public string name_en { get; set; }
        public string name_ar { get; set; }
        public string mobile_num { get; set; }
        public string wa_num { get; set; }
        public string address_en { get; set; }
        public string address_ar { get; set; }
        public string loc_lat { get; set; }
        public string loc_lng { get; set; }
        public string created_by { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }

        public string created_by_name { get; set; }
        public string updated_by_name { get; set; }
    }

    public class EcomGiftItem
    {
        public string gift_id { get; set; }
        public string store_id { get; set; }
        public string min_price { get; set; }
        public string max_price { get; set; }
        public string icon_url { get; set; }
        public string order_by { get; set; }
        public string created_by { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }
        public string created_by_name { get; set; }
        public string updated_by_name { get; set; }
        public string last_row_index { get; set; }
    }
    public class EcomBrandItem
    {
        public string brand_id { get; set; }
        public string store_id { get; set; }
        public string email { get; set; }
        public string name_owner_en { get; set; }
        public string name_owner_ar { get; set; }
        public string name_brand_en { get; set; }
        public string name_brand_ar { get; set; }
        public string brand_desc_en { get; set; }
        public string brand_desc_ar { get; set; }
        public string business_sector_id { get; set; }
        public string phone { get; set; }
        public string mobile_whatsapp { get; set; }
        public string reg_message_to_admin { get; set; }
        public string icon_url { get; set; }
        public string loc_lat { get; set; }
        public string loc_lng { get; set; }
        public string admin_approval_status { get; set; }
        public string admin_approval_remarks { get; set; }
        public string need_approval_to_add_prod { get; set; }
        public string order_by { get; set; }
        public string allow_delivery_within_km { get; set; }
        public string created_by { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }
        public string created_by_name { get; set; }
        public string updated_by_name { get; set; }
        public string last_row_index { get; set; }
    }
    public class EcomcategoryItem
    {
        public string cat_id { get; set; }
        public string store_id { get; set; }
        public string name_en { get; set; }
        public string name_ar { get; set; }
        public string icon_url { get; set; }
        public string order_by { get; set; }
        public string created_by { get; set; }

        public string created_by_name { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }

        public string updated_by_name { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }
    }
    public class EcomTagItem
    {
        public string tag_id { get; set; }
        public string store_id { get; set; }
        public string tag_name_en { get; set; }
        public string tag_name_ar { get; set; }

        public string created_by { get; set; }

        public string created_by_name { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }

        public string updated_by_name { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }
    }

    public class EcomOfferItem
    {
        public string offer_id { get; set; }
        public string prod_id { get; set; }
        public string cat_id { get; set; }
        public string brand_id { get; set; }
        public string offer_by_brand_id { get; set; }
        public string tag_id { get; set; }
        public string offer_dec_en { get; set; }
        public string offer_dec_ar { get; set; }
        public string offer_perc { get; set; }
        public string valid_from { get; set; }
        public string valid_to { get; set; }
        public string coupon_code { get; set; }
        public string offer_img_url { get; set; }

        public string created_by { get; set; }

        public string created_by_name { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_by_name { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }
    }
    public class EcomTemplateItem
    {
        public string template_id { get; set; }
        public string store_id { get; set; }
        public string template_type { get; set; }
        public string destination_type { get; set; }
        public string col_count_for_static { get; set; }
        public string title_en { get; set; }
        public string title_ar { get; set; }
        public string description_en { get; set; }
        public string description_ar { get; set; }
        public string created_by { get; set; }
        public string created_by_name { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_by_name { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }
    }
    public class EcomTemplateItemItem
    {
        public string template_items_id { get; set; }
        public string template_id { get; set; }
        public string item_id { get; set; }
        public string img_url { get; set; }
        public string title_en { get; set; }
        public string title_ar { get; set; }
        public string description_en { get; set; }
        public string description_ar { get; set; }
        public string order_by { get; set; }
        public string created_by { get; set; }
        public string created_by_name { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_by_name { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }
    }
    public class EcomTemplateHomeItem
    {
        public string template_home_id { get; set; }
        public string template_id { get; set; }
        public string brand_id { get; set; }
        public string store_id { get; set; }
        public string order_by { get; set; }
        public string created_by { get; set; }
        public string created_by_name { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_by_name { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }
    }
    public class EcomStaffRoleAppItem
    {
        public string role_id { get; set; }
        public string store_id { get; set; }
        public string role_name { get; set; }
        public string is_admin { get; set; }
        public string created_by { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }
        public List<EcomStaffRoleFormAppItem> role_forms { get; set; }
    }
    public class EcomStaffRoleFormAppItem
    {
        public string role_form_id { get; set; }
        public string role_id { get; set; }
        public string form_key { get; set; }
        public string can_edit { get; set; }
        public string can_delete { get; set; }
        public string created_by { get; set; }
        public string created_on { get; set; }
        public string updated_by { get; set; }
        public string updated_on { get; set; }
        public string is_active { get; set; }
    }
    public class EcomSalesItem
    {
        public string from_date { get; set; }
        public string to_date { get; set; }
        public string total_orders_count { get; set; }
        public string total_sales_items_amount { get; set; }
        public string total_sales_items_count { get; set; }
        public string total_canceled_orders_count { get; set; }
    }
    public class EcomStaffHomeItem
    {
        public string total_processing { get; set; }
        public string total_ready_to_deliver { get; set; }
        public string total_pending { get; set; }
    }
}