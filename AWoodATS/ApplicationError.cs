using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace AWoodATS
{
    public class ApplicationError
    {
        public static string appPath = "";
        public static void logErrors(Exception ex)
        {
            try
            {
                string filePath = HostingEnvironment.MapPath("~/Temp\\err" + ".txt");


                string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
                message += Environment.NewLine;
                message += "-----------------------------------------------------------";
                message += Environment.NewLine;
                message += string.Format("Message: {0}", ex.Message);
                message += Environment.NewLine;
                message += string.Format("StackTrace: {0}", ex.StackTrace);
                message += Environment.NewLine;
                message += string.Format("Source: {0}", ex.Source);
                message += Environment.NewLine;
                message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
                message += Environment.NewLine;
                message += "-----------------------------------------------------------";
                message += Environment.NewLine;
                //string path = HttpContext.Current.Server.MapPath("~/Temp/err.txt");
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine(message);
                }
            }
            catch (Exception er)
            {
                er.ToString();
            }
        }

        public static void WriteMessage(string messageToWrite)
        {
            try
            {
                string filePath = HostingEnvironment.MapPath( "~/Temp\\Message" + ".txt");
                //if (!Directory.Exists(filePath))
                //    Directory.CreateDirectory(filePath);
                string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
                message += Environment.NewLine;
                message += "-----------------------------------------------------------";
                message += Environment.NewLine;
                message += string.Format("Message: {0}", messageToWrite);
                message += Environment.NewLine;                
                message += "-----------------------------------------------------------";
                message += Environment.NewLine;
                //string path = HttpContext.Current.Server.MapPath("~/Temp/err.txt");
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine(message);
                }
            }
            catch (Exception er)
            {
                er.ToString();
            }
        }

        public static void logErrors(Exception ex,string FunctionName)
        {
            try
            {
                string filePath = HostingEnvironment.MapPath("~/Temp\\err" + ".txt");
                
                string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
                message += Environment.NewLine;
                message += "-----------------------------------------------------------";
                message += Environment.NewLine;
                message += string.Format("Message: {0}", ex.Message);
                message += Environment.NewLine;
                message += string.Format("StackTrace: {0}", ex.StackTrace);
                message += Environment.NewLine;
                message += string.Format("Source: {0}", ex.Source);
                message += Environment.NewLine;
                message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
                message += Environment.NewLine;
                message += Environment.NewLine;
                message += string.Format("FunctionName: {0}", FunctionName.ToString());
                message += "-----------------------------------------------------------";
                message += Environment.NewLine;
                //string path = HttpContext.Current.Server.MapPath("~/Temp/err.txt");
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine(message);
                }
            }
            catch (Exception er)
            {
                er.ToString();
            }
        }
    }
}