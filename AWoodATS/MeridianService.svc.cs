using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AWoodATS
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "MeridianService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select MeridianService.svc or MeridianService.svc.cs at the Solution Explorer and start debugging.
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
    public class MeridianService : IMeridianService
    {
        private void CompleteShippingScheduleResponseProcess(object state)
        {
            var meridianAsyncResult = state as MeridianAsyncResult;
            ProcessShippingScheduleResponse objProcess = new ProcessShippingScheduleResponse();
            meridianAsyncResult.Result = objProcess.ProcessShippingScheduleResponseRequest(meridianAsyncResult.CurrentOperationContext);
            meridianAsyncResult.Completed();
        }
        private void CompleteProcess(object state)
        {
            var meridianAsyncResult = state as MeridianAsyncResult;

            if (CommonProcess.IsShippingScheduleRequest(meridianAsyncResult.CurrentOperationContext))
            {
                ProcessShippingSchedule objProcessShippingSchedule = new ProcessShippingSchedule();
                meridianAsyncResult.Result = objProcessShippingSchedule.ProcessDocument(meridianAsyncResult.CurrentOperationContext);
            }            
            meridianAsyncResult.Completed();
        }
        //public IAsyncResult BeginRequisition(AsyncCallback callback, object asyncState)
        //{
        //    throw new NotImplementedException();
        //}
        private async Task<bool> SendFileToFTP(MeridianResult meridianResult)
        {
            if ((meridianResult != null) && !string.IsNullOrWhiteSpace(meridianResult.FileName))
            {
                try
                {
                    FtpWebRequest ftpRequest = (FtpWebRequest)FtpWebRequest.Create(meridianResult.FtpServerInFolderPath + meridianResult.FileName);
                    ftpRequest.Credentials = new NetworkCredential(meridianResult.FtpUserName, meridianResult.FtpPassword);
                    ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                    ftpRequest.UseBinary = true;
                    ftpRequest.KeepAlive = false;
                    ftpRequest.Timeout = Timeout.Infinite;

                    if (meridianResult.UploadFromLocalPath)
                    {
                        using (StreamReader sourceStream = new StreamReader(meridianResult.LocalFilePath + meridianResult.FileName))
                            meridianResult.Content = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                    }

                    using (Stream requestStream = await ftpRequest.GetRequestStreamAsync())
                    {
                        requestStream.Write(meridianResult.Content, 0, meridianResult.Content.Length);
                        requestStream.Flush();
                    }
                    using (FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse())
                        if (response.StatusCode == FtpStatusCode.ClosingData)
                        {
                            var prefixToTake = meridianResult.IsSchedule ? MeridianGlobalConstants.XCBL_AWC_FILE_PREFIX : MeridianGlobalConstants.XCBL_AWC_REQUISITION_FILE_PREFIX;
                           // MeridianSystemLibrary.LogTransaction(meridianResult.WebUserName, meridianResult.FtpUserName, (prefixToTake + "- Successfully completed request"), "01.06", string.Format("{0} - Successfully completed request for {1}", prefixToTake, meridianResult.UniqueID), string.Format("Uploaded CSV file: {0} on ftp server successfully for {1}", meridianResult.FileName, meridianResult.UniqueID), meridianResult.FileName, meridianResult.UniqueID, meridianResult.OrderNumber, meridianResult.XmlDocument, "Success");
                            if (meridianResult.IsSchedule && MeridianGlobalConstants.CONFIG_AWC_CALL_SSR_REQUEST.Equals(MeridianGlobalConstants.XCBL_YES_FLAG, StringComparison.OrdinalIgnoreCase))
                                CommonProcess.SendShippingScheduleResponse1(meridianResult);
                            return true;
                        }
                        else
                        {
                            //MeridianSystemLibrary.LogTransaction(meridianResult.WebUserName, meridianResult.FtpUserName, "UploadFileToFtp", "03.08", "Error - While CSV uploading file - Inside TRY block", string.Format("Error - While uploading CSV file: {0} with error - Inside TRY block - ", meridianResult.FileName), meridianResult.FileName, meridianResult.UniqueID, meridianResult.OrderNumber, null, "Error 03.08 - Upload CSV to PBS");
                            return false;
                        }
                }
                catch (Exception ex)
                {
                    //MeridianSystemLibrary.LogTransaction(meridianResult.WebUserName, meridianResult.FtpUserName, "UploadFileToFtp", "03.08", "Error - While CSV uploading file - Inside CATCH block", string.Format("Error - While uploading CSV file: {0} with error {1} - Inside CATCH block - ", meridianResult.FileName, ex.Message), meridianResult.FileName, meridianResult.UniqueID, meridianResult.OrderNumber, null, "Error 03.08 - Upload CSV to PBS");
                    return false;
                }
            }
            return false;
        }
       
        public IAsyncResult BeginSubmitDocument(AsyncCallback callback, object asyncState)
        {
            var meridianAsyncResult = new MeridianAsyncResult(OperationContext.Current, callback, asyncState);
            ThreadPool.QueueUserWorkItem(CompleteProcess, meridianAsyncResult);
            return meridianAsyncResult;
        }

        public XElement EndSubmitDocument(IAsyncResult asyncResult)
        {
            var meridianAsyncResult = asyncResult as MeridianAsyncResult;
            meridianAsyncResult.AsyncWait.WaitOne();
            return XElement.Parse(MeridianSystemLibrary.GetMeridian_Status(meridianAsyncResult.Result.Status, meridianAsyncResult.Result.UniqueID, meridianAsyncResult.Result.IsSchedule));
        }
        public void DoWork()
        {
        }

        //public XElement EndRequisition(IAsyncResult result)
        //{
        //    throw new NotImplementedException();
        //}
        //public IAsyncResult BeginShippingScheduleResponse(AsyncCallback callback, object asyncState)
        //{
        //    var meridianAsyncResult = new MeridianAsyncResult(OperationContext.Current, callback, asyncState);
        //    ThreadPool.QueueUserWorkItem(CompleteShippingScheduleResponseProcess, meridianAsyncResult);
        //    return meridianAsyncResult;
        //}

        //public XElement EndShippingScheduleResponse(IAsyncResult result)
        //{
        //    var meridianAsyncResult = result as MeridianAsyncResult;
        //    meridianAsyncResult.AsyncWait.WaitOne();
        //    if (!meridianAsyncResult.Result.Status.Equals(MeridianGlobalConstants.MESSAGE_ACKNOWLEDGEMENT_FAILURE, StringComparison.OrdinalIgnoreCase))
        //    {
        //        if (MeridianGlobalConstants.CONFIG_AWC_REQUISITION_TEST)
        //            meridianAsyncResult.Result.Status = MeridianGlobalConstants.MESSAGE_ACKNOWLEDGEMENT_SUCCESS;
        //        else
        //            meridianAsyncResult.Result.Status = SendFileToFTP(meridianAsyncResult.Result).GetAwaiter().GetResult() ? MeridianGlobalConstants.MESSAGE_ACKNOWLEDGEMENT_SUCCESS : MeridianGlobalConstants.MESSAGE_ACKNOWLEDGEMENT_FAILURE;
        //    }
        //    return XElement.Parse(MeridianSystemLibrary.GetMeridian_Status(meridianAsyncResult.Result.Status, meridianAsyncResult.Result.UniqueID, meridianAsyncResult.Result.IsSchedule));
        //}

        
    }
}
