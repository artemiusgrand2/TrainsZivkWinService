
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace TrainsZivkWinService
{
    public class IasPurGpSource
    {
        public IList<TrainEvent> GetLastTrainEvents(DateTime startTime, DateTime endTime, string connectionString)
        {
            var result = new List<TrainEvent>();
            // название процедуры
            string sqlExpression = "GetLastMessages5676FromTime";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(sqlExpression, connection);
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
                                    result.Add(new TrainEvent()
                                    {
                                        TrainNumber = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                        OperationCode = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                        EventStation = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                        EventTime = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4),
                                        Index1 = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                        Index2 = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                                        Index3 = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                                    });
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
            catch(Exception error) { }
            //
            return result;
        }
    }
}

