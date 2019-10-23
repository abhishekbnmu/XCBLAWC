using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Timers;
using System.Web;

namespace AWoodATS
{
    public class ProcessPBSQueryResult
    {
        private static readonly object padLock = new object();

        private static ProcessPBSQueryResult instance = null;

        public Dictionary<string, PBSData> AllPBSOrder = new Dictionary<string, PBSData>();

        private static System.Timers.Timer pbsFrequencyTimer = new System.Timers.Timer();

        private static bool IsProcessing = false;

        ProcessPBSQueryResult()
        {

        }

        //It will give instance of ProcessPBSQueryResult and if Processing 
        //is going on(to fetch data from service and fill to 'AllPBSOrder' variable)
        //then it will make caller wait for process to be finished.
        public static ProcessPBSQueryResult Instance
        {
            get
            {
                lock (padLock)
                {
                    if (instance == null)
                        instance = new ProcessPBSQueryResult();

                    while (IsProcessing)
                    {
                        Thread.Sleep(500);
                    }

                    return instance;
                }
            }
        }

        public void InitiateFrequencyTimer()
        {
            pbsFrequencyTimer.AutoReset = true;
            pbsFrequencyTimer.Enabled = false;
            pbsFrequencyTimer.Interval = TimeSpan.FromMinutes(MeridianGlobalConstants.DEFAULT_PBS_FREQUENCY_TIMER_INTERVAL_IN_MINUTES).TotalMilliseconds;
            pbsFrequencyTimer.Elapsed += PbsFrequencyTimer_Elapsed;
            pbsFrequencyTimer.Start();
        }

        private void PbsFrequencyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
           // MeridianSystemLibrary.LogTransaction(null, null, "PbsFrequencyTimer_Elapsed", "01.09", "Success - inside PbsFrequencyTimer_Elapsed", "Success - inside PbsFrequencyTimer_Elapsed", null, null, null, null, "Success - inside PbsFrequencyTimer_Elapsed");
            var dateNow = DateTime.Now;
            var startTime = MeridianGlobalConstants.PBS_QUERY_START_TIME;
            var startTimeParts = startTime.Split(new char[1] { ':' });
            var startDateTime = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, int.Parse(startTimeParts[0]), int.Parse(startTimeParts[1]), 00);

            var endTime = MeridianGlobalConstants.PBS_QUERY_END_TIME;
            endTime = !string.IsNullOrWhiteSpace(endTime) ? endTime : MeridianGlobalConstants.DEFAULT_PBS_QUERY_END_TIME;
            var endTimeParts = endTime.Split(new char[1] { ':' });
            var endDateTime = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, int.Parse(endTimeParts[0]), int.Parse(endTimeParts[1]), 00);

            if ((DateTime.Now >= startDateTime) && (DateTime.Now <= endDateTime))
            {
                // GetAllOrder();
            }
            else
            {
                pbsFrequencyTimer.Stop();
                pbsFrequencyTimer.Interval = TimeSpan.FromMinutes(MeridianGlobalConstants.DEFAULT_PBS_FREQUENCY_TIMER_INTERVAL_IN_MINUTES).TotalMilliseconds;
                pbsFrequencyTimer.Start();
            }
        }

        private void GetAllOrder()
        {
            IsProcessing = true;
           // MeridianSystemLibrary.LogTransaction(null, null, "GetAllOrder", "01.10", "Success - inside GetAllOrder", "Success - inside GetAllOrder", null, null, null, null, "Success - inside GetAllOrder");
            var allLatestPBSOrders = new Dictionary<string, PBSData>();
            pbsFrequencyTimer.Stop();
            pbsFrequencyTimer.Interval = TimeSpan.FromMinutes(MeridianGlobalConstants.PBS_QUERY_FREQUENCY).TotalMilliseconds;
            pbsFrequencyTimer.Start();
            string fileNameFormat = DateTime.Now.ToString(MeridianGlobalConstants.XCBL_FILE_DATETIME_FORMAT);

            using (HttpClient client = new HttpClient())
            {
                var sqlQuery = string.Format(MeridianGlobalConstants.PBS_WEB_SERVICE, MeridianGlobalConstants.PBS_WEB_SERVICE_QUERY, MeridianGlobalConstants.CONFIG_PBS_WEB_SERVICE_USER_NAME, MeridianGlobalConstants.CONFIG_PBS_WEB_SERVICE_PASSWORD);
                var res = client.GetAsync(sqlQuery).Result;
                var resultString = client.GetStringAsync(MeridianGlobalConstants.PBS_OUTPUT_FILE).Result;

                if (MeridianGlobalConstants.PBS_ENABLE_CACHE_LOG == MeridianGlobalConstants.XCBL_YES_FLAG)
                    CommonProcess.CreateLogFile(string.Format("{0}\\PBS{1}voc.txt", MeridianGlobalConstants.PBS_CACHE_LOG_LOCATION, fileNameFormat), resultString);

                if (!string.IsNullOrWhiteSpace(resultString))
                {
                    var lines = resultString.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    if (lines.Count() > 1)
                    {
                        for (int i = 1; i < lines.Length; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(lines[i]))
                            {
                                var values = lines[i].Split(',');
                                if (values.Length > 29)
                                {
                                    DateTimeOffset orderDate;
                                    if (DateTimeOffset.TryParse(values[4], out orderDate))
                                    {
                                        PBSData pbsData = new PBSData();
                                        pbsData.DeliveryDate = values[1];
                                        pbsData.ShipmentDate = values[2];
                                        pbsData.IsScheduled = values[3];
                                        pbsData.OrderNumber = values[11].Trim();
                                        pbsData.DestinationName = values[25];
                                        pbsData.DestinationStreet = values[26];
                                        pbsData.DestinationStreetSupplyment1 = values[27];
                                        pbsData.DestinationCity = values[28];
                                        pbsData.DestinationRegionCoded = string.Format(MeridianGlobalConstants.XCBL_US_CODE + values[29]);
                                        pbsData.DestinationPostalCode = values[30];

                                        if (!allLatestPBSOrders.ContainsKey(pbsData.OrderNumber))
                                            allLatestPBSOrders.Add(pbsData.OrderNumber, pbsData);
                                    }
                                }
                                else
                                {
                                  ///  MeridianSystemLibrary.LogTransaction(null, null, "GetAllOrder", "02.25", "Warning - Values lenght less then 29", "Warning - Values lenght less then 29 from PBS WebService", null, null, null, null, "Warning 02.25 : Values lenght less then 29");
                                }
                            }
                        }
                    }
                    else
                    {
                       // MeridianSystemLibrary.LogTransaction(null, null, "GetAllOrder", "02.26", "Warning - PBS File Lines Count < 2", "Warning - PBS File Lines Count < 2 from PBS WebService", null, null, null, null, "Warning 02.26 : PBS File Lines Count < 2");
                    }
                }
                else
                {
                   // MeridianSystemLibrary.LogTransaction(null, null, "GetAllOrder", "02.27", "Warning - Empty PBS text file", "Warning - Empty PBS text file from PBS WebService", null, null, null, null, "Warning 02.27 : Empty Text File");
                }
            }

            if (MeridianGlobalConstants.PBS_ENABLE_CACHE_LOG == MeridianGlobalConstants.XCBL_YES_FLAG)
            {
                StringBuilder strBuilder = new StringBuilder(MeridianGlobalConstants.PBS_CSV_HEADERS);
                strBuilder.AppendLine();
                foreach (var item in allLatestPBSOrders)
                {
                    strBuilder.AppendLine(string.Format(MeridianGlobalConstants.PBS_CSV_HEADER_NAME_FORMAT,
                        item.Value.DeliveryDate, item.Value.ShipmentDate, item.Value.IsScheduled,
                        item.Value.OrderNumber, item.Value.DestinationName, item.Value.DestinationStreet,
                        item.Value.DestinationStreetSupplyment1, item.Value.DestinationCity, item.Value.DestinationRegionCoded,
                        item.Value.DestinationPostalCode));
                }
                CommonProcess.CreateLogFile(string.Format("{0}\\XCBL{1}PBSCachedOrders.csv", MeridianGlobalConstants.PBS_CACHE_LOG_LOCATION, fileNameFormat), strBuilder.ToString());
            }

            AllPBSOrder = allLatestPBSOrders;
            IsProcessing = false;
        }
    }
}
 