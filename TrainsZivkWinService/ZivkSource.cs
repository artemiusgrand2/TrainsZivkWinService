using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace TrainsZivkWinService
{
    class ZivkSource
    {

        public IList<TrainEvent> GetLastTrainEvents(DateTime startTime, DateTime endTime, string connectionString)
        {
            var result = new List<TrainEvent>();
            // название процедуры
            string sqlExpression = "GetTrainsEvents";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                // указываем, что команда представляет хранимую процедуру
                command.CommandType = System.Data.CommandType.StoredProcedure;
                // параметры
                var nameParam1 = new SqlParameter("@StartTime", startTime);
                var nameParam2 = new SqlParameter("@EndTime", endTime);
                // добавляем параметры
                command.Parameters.Add(nameParam1);
                command.Parameters.Add(nameParam2);
                //
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                var newEvent = new TrainEvent()
                                {
                                    TrainId = reader.GetGuid(0),
                                    TrainNumber = reader.IsDBNull(1) ? null : reader.GetString(1),
                                    EventTime = reader.GetDateTime(2),
                                    EventStation = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    DObjType = reader.IsDBNull(4) ? (short)0 : reader.GetInt16(4),
                                    DObjName = reader.IsDBNull(5) ? null : reader.GetString(5),
                                    EventType = reader.IsDBNull(6) ? (short)0 : reader.GetInt16(6),
                                    EventAxis = reader.IsDBNull(7) ? null : reader.GetString(7),
                                    TrainNumberPrefix = reader.IsDBNull(9) ? null : reader.GetString(9),
                                    TrainNumberSuffix = reader.IsDBNull(10) ? null : reader.GetString(10)
                                };
                                if (result.Where(x => x.EventStation == newEvent.EventStation && x.TrainNumber == newEvent.TrainNumber).Count() == 0)
                                    result.Add(newEvent);
                            }
                            catch { }
                        }
                    }
                    // reader.Close();
                }
                if (Program.Stations != null && Program.Stations.Count > 0)
                    result = result.Where(x => Program.Stations.Contains(x.EventStation)).ToList();
                if (Program.Trains != null && Program.Trains.Count > 0)
                    result = result.Where(x => Program.Trains.Contains(x.TrainNumber)).ToList();
                //удаляем разрывные записи
                var trainsRepeat = result.GroupBy(x => x.TrainNumber).Where(x => x.Count() > 1).ToList();
                var buffer = new List<TrainEvent>();
                if (trainsRepeat.Count > 0)
                {
                    var ids = trainsRepeat.SelectMany(x => x.Select(y => y.TrainId)).Distinct();
                    var stations = trainsRepeat.SelectMany(x => x.Select(y => y.EventStation)).Distinct();
                    //ищем повторяющиеся поезда
                    command.CommandText =
                     "SELECT E_SCBThreadID, E_StationOfEvent, E_Time FROM TEvents where E_SCBThreadID in (@IDS) and E_StationOfEvent in (@Stations)";
                    command.CommandType = System.Data.CommandType.Text;
                    command.AddArrayParameters("IDS", ids);
                    command.AddArrayParameters("Stations", stations);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                try
                                {
                                    var newEvent = new TrainEvent()
                                    {
                                        TrainId = reader.GetGuid(0),
                                        EventStation = reader.IsDBNull(1) ? null : reader.GetString(1),
                                        EventTime = reader.GetDateTime(2)
                                    };
                                    buffer.Add(newEvent);
                                }
                                catch { }
                            }
                        }
                    }
                    //
                    foreach (var trainEventsGroup in trainsRepeat)
                    {
                        int trainNumber;
                        var trainEvents = trainEventsGroup.ToList();
                        if (int.TryParse(trainEventsGroup.Key, out trainNumber) && Program.TrainsPass.Where(x=>x.Start>=trainNumber && trainNumber <=x.End).ToList().Count > 0)
                        {

                            trainEvents.Where(x => trainEvents.IndexOf(x) > 0).ToList().ForEach(x => {
                                trainEvents.Remove(x);
                                result.Remove(x);
                            });
                        }
                        else
                        {
                            for (var i = 0; i < trainEvents.Count; i++)
                            {
                                var chiefRope = buffer.Where(y => y.TrainId == trainEvents[i].TrainId).ToList();
                                trainEvents.Where(x => trainEvents.IndexOf(x) > i).ToList().ForEach(x =>
                                {
                                    var findEvent = chiefRope.Where(y => y.TrainId == trainEvents[i].TrainId && x.EventStation == y.EventStation && (Math.Abs((x.EventTime - y.EventTime).TotalMinutes) <= 5)).FirstOrDefault();
                                    if (findEvent != null)
                                    {
                                        trainEvents.Remove(x);
                                        result.Remove(x);
                                    }
                                });
                            }
                        }
                    }
                }
                //ищем  индексы поездов
                command.CommandText =
                    "SELECT E.E_SCBThreadID, A.TAsoup_MessageTime ,A.TAsoup_TrainIndex_FirstStation ,A.TAsoup_TrainIndex_StockNumber ,A.TAsoup_TrainIndex_LastStation FROM TEvents E inner join  TLastEvents LE on E.E_ID = LE.LE_GlobalEventID " +
                    "left outer join TSCBThreadsTrains TT on E.E_ID = TT.SCBTT_EventID left outer  join TTrainsASOUPInfo A on E.E_ID = A.TAsoup_EventID where " +
                    "E.E_SCBThreadID in (@IDS)  and A.TAsoup_TrainIndex_LastStation IS NOT NULL order by E.E_InsRecDateTime desc";
                command.CommandType = System.Data.CommandType.Text;
                command.Parameters.Clear();
                command.AddArrayParameters("IDS", result.Select(x => x.TrainId));
                buffer.Clear();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                var newEvent = new TrainEvent()
                                {
                                    TrainId = reader.GetGuid(0),
                                    MessTime = reader.GetDateTime(1),
                                    Index1 = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    Index2 = reader.IsDBNull(3) ? null : reader.GetString(3),
                                    Index3 = reader.IsDBNull(4) ? null : reader.GetString(4)
                                };
                                buffer.Add(newEvent);
                            }
                            catch { }
                        }
                    }
                }
                //
                result.ForEach(x =>
                {
                    var value = buffer.Where(y => y.TrainId == x.TrainId).FirstOrDefault();
                    int bufferInt;
                    if(value != null)
                    {
                        x.Index1 = value.Index1;
                        x.Index2 = value.Index2;
                        x.Index3 = (int.TryParse(value.Index3, out bufferInt)) ? HelpFunctions.GetFullESRCode(bufferInt).ToString() : value.Index3;
                        x.MessTime = value.MessTime;
                    }
                });
            }

            //
            return result;
        }

    }

    public static class Extensions
    {
        public static void AddArrayParameters<T>(this SqlCommand cmd, string name, IEnumerable<T> values)
        {
            name = name.StartsWith("@") ? name : "@" + name;
            var names = string.Join(", ", values.Select((value, i) => {
                var paramName = name + i;
                cmd.Parameters.AddWithValue(paramName, value);
                return paramName;
            }));
            cmd.CommandText = cmd.CommandText.Replace(name, names);
        }
    }
}
