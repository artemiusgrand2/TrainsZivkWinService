using System;
using System.Net;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace TrainsZivkWinService
{
    public class AGDPSource
    {
        public IList<TrainEvent> GetLastTrainEvents(string url)
        {
            var result = new StringBuilder();
            var request = WebRequest.Create(url);
            request.Timeout = 30000;
            lock (request)
            {
                using (var response = request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            string line = "";
                            while ((line = reader.ReadLine()) != null)
                                result.Append(line);
                        }
                    }
                }
            }
            //
            return (new JavaScriptSerializer()).Deserialize<IList<TrainEvent>>(result.ToString());
        }
    }
}
