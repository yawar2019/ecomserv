using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;

namespace bomoserv.Data
{
    [DataContract]
    public class MPDataList
    {
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public string videoId { get; set; }
        [DataMember]
        public string Keyword { get; set; }
        [DataMember]
        public string ApiKey { get; set; }
        [DataMember]
        public string is_paid { get; set; }
        [DataMember]
        public List<MPData> data { get; set; } 

    }
    [DataContract]
    public class MPData
    {
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public string Keyword { get; set; }
        [DataMember]
        public string ApiKey { get; set; }
        [DataMember]
        public string is_paid { get; set; }
        [DataMember]
        public string videoId { get; set; }
        [DataMember]
        public string thumbnail { get; set; }
        [DataMember]
        public string title_lengthy { get; set; }
        [DataMember]
        public string simpleText { get; set; }
        [DataMember]
        public string lengthText { get; set; }
        [DataMember]
        public string viewCountText { get; set; }
        [DataMember]
        public string publishedTimeText { get; set; }

    }
    [DataContract]
    public class PDFData
    {
        [DataMember]
        public string Status { get; set; } 
        [DataMember]
       // public string data { get; set; }
       // [DataMember]
        public byte[] buffer { get; set; }
        [DataMember]
        public string ApiKey { get; set; }
        [DataMember]
        public string is_paid { get; set; }
        [DataMember]
        public string from_format { get; set; }
        [DataMember]
        public string to_format { get; set; }
        [DataMember]
        public string source { get; set; }
        [DataMember]
        public string file_path { get; set; }
        [DataMember]
        public string file_data { get; set; }
        [DataMember]
        public string page_type { get; set; }
        [DataMember]
        public string page_num { get; set; }
        [DataMember]
        public string file_pw { get; set; }
        [DataMember]
        public string file_email { get; set; }

    }

}