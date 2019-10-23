using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Web.Services.Protocols;
using System.Xml.Linq;

namespace AWoodATS
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IMeridianService" in both code and config file together.
    [ServiceContract(Namespace = "http://tempuri.org")]
    public interface IMeridianService
    {
        [OperationContract(AsyncPattern = true)]
        [SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Bare)]
        IAsyncResult BeginSubmitDocument(AsyncCallback callback, object asyncState);

        XElement EndSubmitDocument(IAsyncResult asyncResult);


        //[OperationContract(AsyncPattern = true)]
        //[SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Bare)]
        //IAsyncResult BeginRequisition(AsyncCallback callback, object asyncState);

        //XElement EndRequisition(IAsyncResult result);


        //[OperationContract(AsyncPattern = true)]
        //[SoapDocumentMethod(ParameterStyle = SoapParameterStyle.Bare)]
        //IAsyncResult BeginShippingScheduleResponse(AsyncCallback callback, object asyncState);

        //XElement EndShippingScheduleResponse(IAsyncResult result);
    }
}
