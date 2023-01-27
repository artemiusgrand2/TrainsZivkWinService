using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

using TrainsZivkWinService.Enums;

namespace TrainsZivkWinService
{
    public class HelpFunctions
    {

        public static int GetFullESRCode(int shortESRCode)
        {
            var result = shortESRCode;
            //узнаем сколько цифр в еср коде
            var fourCode = (int)shortESRCode / 1000;
            var fiveCode = (int)shortESRCode / 10000;
            var countDischarges = (fourCode <= 9 && fourCode >= 1) ? 4 : (fiveCode <= 9 && fiveCode >= 1) ? 5 : 0;
            if (countDischarges != 0)
            {
                var listDischarges = new List<int>();
                var balance = shortESRCode;
                for (var i = 1; i < countDischarges; i++)
                {
                    listDischarges.Add((int)(balance / (int)(Math.Pow(10, countDischarges - i))));
                    balance = (int)(balance % (int)(Math.Pow(10, countDischarges - i)));
                    if (i == countDischarges - 1)
                        listDischarges.Add(balance);
                }
                //
                if (listDischarges.Count == 4)
                {
                    listDischarges.Add(0);
                    result = result * 10;
                }
                //
                result = GetDischargeESRCode(listDischarges, result);
            }
            //
            return result;
        }

        public static int GetDischargeESRCode(IList<int> listDischarges, int code)
        {
            var result = code;
            int summ = 0;
            for (var i = 1; i <= listDischarges.Count; i++)
                summ += (listDischarges[i - 1] * i);
            //
            var balance = summ % 11;
            if (summ % 11 != 10)
            {
                result = result * 10 + balance;
                if (listDischarges.Count == 4)
                {
                    listDischarges.Add(balance);
                    return GetDischargeESRCode(listDischarges, result);
                }
            }
            else
            {
                for (var i = 3; i <= listDischarges.Count + 2; i++)
                    summ += (listDischarges[i - 3] * i);
                //
                balance = summ % 11;
                if (balance == 10)
                    balance = 0;
                result = result * 10 + balance;
                if (listDischarges.Count == 4)
                    return GetDischargeESRCode(listDischarges, result);
            }
            //
            return result;
        }

        public static void ReadConfig(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    var currentObject = ViewObject.none;
                    foreach (var str in File.ReadAllText(file).ToUpper().Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var cells = str.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                        if (cells.Length == 2)
                        {
                            if (cells[0].IndexOf("TRAINS") != -1)
                                currentObject = ViewObject.trains;
                            else if (cells[0].IndexOf("STATIONS") != -1)
                                currentObject = ViewObject.stations;
                            else
                                currentObject = ViewObject.none;
                            //
                            if (currentObject == ViewObject.stations || currentObject == ViewObject.trains)
                            {
                                var cell = cells[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => HelpFunctions.ParseStation(x)).ToList();
                                switch (currentObject)
                                {
                                    case ViewObject.stations:
                                        {
                                            Program.Stations.AddRange(cell);
                                        }
                                        break;
                                    case ViewObject.trains:
                                        {
                                            cell.ForEach(x =>
                                            {
                                                var interval = x.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                                                if (interval.Length == 2)
                                                {
                                                    int start, end;
                                                    if (int.TryParse(interval[0], out start) && int.TryParse(interval[1], out end))
                                                    {
                                                        if (start <= end)
                                                        {
                                                            for (var i = start; i <= end; i++)
                                                            {
                                                                if (!Program.Trains.Contains(i.ToString()))
                                                                    Program.Trains.Add(i.ToString());
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (interval.Length == 1)
                                                {
                                                    var trainNumber = HelpFunctions.ParseStation(interval[0]);
                                                    if (!Program.Trains.Contains(trainNumber))
                                                        Program.Trains.Add(trainNumber);
                                                }
                                            });
                                        }
                                        break;
                                }
                            }

                        }
                    }
                }
            }
            catch { }
        }

        public static string ParseStation(string station)
        {
            int buffer;
            if (int.TryParse(station, out buffer))
                return buffer.ToString();
            else
                return station;
        }

    }
}
