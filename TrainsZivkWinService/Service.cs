using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Configuration;
using System.Data.SqlClient;

using SpacialToGIDClasses;

namespace TrainsZivkWinService
{
    public class Service : IService
    {

        private DateTime lastGetEventFromAGDP;
        private IList<TrainEvent> eventsAGDP;
        private readonly int countHour = 4;
        private readonly int countMinForHideTrains = 10;
        private readonly CorrectStations correctStations;

        public Service()
        {
            int buffer;
            if (ConfigurationManager.AppSettings.AllKeys.Contains("countHour") && int.TryParse(ConfigurationManager.AppSettings["countHour"], out buffer) && buffer > 0)
                countHour = buffer;
            //
            if (ConfigurationManager.AppSettings.AllKeys.Contains("countMinForHideTrains") && int.TryParse(ConfigurationManager.AppSettings["countMinForHideTrains"], out buffer) && buffer > 0)
                countMinForHideTrains = buffer;
            //
            if (ConfigurationManager.ConnectionStrings["fileConvertESRCode"] != null && System.IO.File.Exists(ConfigurationManager.ConnectionStrings["fileConvertESRCode"].ConnectionString))
                correctStations = SpacialToGIDClasses.StationCodeToGIDTransformer.GetStationCodesNew(ConfigurationManager.ConnectionStrings["fileConvertESRCode"].ConnectionString);
            GetEventsAGDP();
        }

        public IList<TrainEvent> GetLastTrainEvents()
        {
            var end = DateTime.Now;
            var start = end.AddHours(-countHour);
            var eventsZivk = (new ZivkSource()).GetLastTrainEvents(start, end, ConfigurationManager.ConnectionStrings["zivk"].ConnectionString);
            var eventsIasPurGp = (new IasPurGpSource()).GetLastTrainEvents(start, end, ConfigurationManager.ConnectionStrings["iasPurGP"].ConnectionString);
            if (DateTime.Now.DayOfYear != lastGetEventFromAGDP.DayOfYear)
                GetEventsAGDP();
            //работаем с выборкой IasPurGp
            var result = eventsZivk.Where(x =>
            {
                var finder = eventsIasPurGp.Where(y => x.Index1 != null && y.Index1.IndexOf(x.Index1) == 0 && int.Parse(y.Index2) == int.Parse(x.Index2) && y.Index3 == x.Index3
                &&(y.OperationCode == "P0004" || y.OperationCode == "P0022" || y.OperationCode == "P0023" || y.OperationCode == "C0064" || y.OperationCode == "C1020" /*|| x.EventStation != y.EventStation*/)).FirstOrDefault();
                if (finder != null)
                    return false;
                else
                    return true;
            }).ToList();
            //работаем с выборкой AGDP
            result = result.Where(x =>
            {
                var finder = eventsAGDP.Where(y => y.TrainNumber == x.TrainNumber && y.EventStation == x.EventStation).FirstOrDefault();
                if (finder != null && (DateTime.Now - x.EventTime).TotalMinutes > countMinForHideTrains)
                    return false;
                else
                    return true;
            }).ToList();
            //
            return result;
        }


        private void GetEventsAGDP()
        {
            eventsAGDP = (new AGDPSource()).GetLastTrainEvents(ConfigurationManager.ConnectionStrings["agdp"].ConnectionString);
            if (correctStations != null && correctStations.StationsList != null)
            {
                eventsAGDP.ToList().ForEach(x =>
                {
                    var findStation = correctStations.StationsList.Where(y => y.RealCode == x.EventStation).FirstOrDefault();
                    if (findStation != null)
                        x.EventStation = findStation.GidCode;
                });

            }
            lastGetEventFromAGDP = DateTime.Now;
        }
    }
}
