using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using bomoserv.Common;
using bomoserv.Data; 

namespace bomoserv
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ibomopublic" in both code and config file together.
    [ServiceContract]
    public interface ibomopublic
    {
        [OperationContract]
        MPDataList GetMPData(MPDataList data);
        [OperationContract]
        MPDataList GetMPLinks(MPDataList data);
        [OperationContract]
        PDFData ConvertFileData(PDFData data);

        [OperationContract]
        [WebInvoke(Method = "GET", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, UriTemplate = "ConvertFile/{ApiKey}/{is_paid}/{from_format}/{to_format}/{source}/{file_path}/{file_data}/{page_type}/{page_num}/{file_pw}/{file_email}")]

        PDFData ConvertFile(string ApiKey, string is_paid,string from_format,string to_format,string source,string file_path,string file_data,string page_type,string page_num,string file_pw,string file_email);
         
    }
}
