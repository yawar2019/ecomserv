using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using ecomserv.Common; 
using ecomserv.Ecom; 
namespace ecomserv
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "iecompublic" in both code and config file together.
    [ServiceContract]
    public interface iecompublic
    {
      

        [OperationContract]
        EcomData HandleEcom(EcomData data); 

  }
}
