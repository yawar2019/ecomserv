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
    public class DataCommon
    {
        public class Currency
        {
            //---SECURE
            [DataMember]
            public string STATUSAll { get; set; }
            [DataMember]
            public string TIME { get; set; }
            [DataMember]
            public string SI { get; set; }
            [DataMember]
            public string SES { get; set; }
            [DataMember]
            public string DEVID { get; set; }
            //----
            [DataMember]
            public string Return_Status { get; set; }
            [DataMember]
            public string Return_Key { get; set; }
            [DataMember]
            public string Flag { get; set; } 
            [DataMember]
            public string From { get; set; }
            [DataMember]
            public string To { get; set; }
            [DataMember]
            public  string Val { get; set; }
            [DataMember]
            public string ValOut { get; set; }
        }
        [DataContract]
        public class TextVal
        {
            [DataMember]
            public string text { get; set; }
            [DataMember]
            public string value { get; set; }

        }
    }
}