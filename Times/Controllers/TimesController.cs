using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace Times.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TimesController : ControllerBase
    {
        private readonly ILogger<TimesController> _logger;

        public TimesController(ILogger<TimesController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public TimeResponse Get()
        {
            var client = new RestClient("http://au-journeyplanner.silverrailtech.com");
            var now = DateTime.Now.ToString("yyyy-MM-ddTHH:mm");
            var request = new RestRequest($"/journeyplannerservice/v2/REST/DataSets/PerthRestricted/StopTimetable?ApiKey=40c5877a-7b0f-4045-9544-046d4f348bc2&format=json&StopUid=PerthRestricted%3A133&Time={HttpUtility.UrlEncode(now)}&ReturnNotes=true&IsRealTimeChecked=false", Method.GET);

            IRestResponse response = client.Execute(request);
            var content = response.Content; // raw content as string
            var stops = JsonConvert.DeserializeObject<TripResult>(content);

            var railTimes = stops.Trips.Where(x => x.Summary.Mode == "Rail");

            return new TimeResponse
            {
                Times = railTimes.Select(x => x.Summary.StartTimeOnly).ToList()
            };
        }
    }    public class Summary
    {
        public string Mode { get; set; }
        public string TripStartTime { get; set; }
        public string StartTimeOnly { get
            {
                if(string.IsNullOrEmpty(TripStartTime))
                    return "";

                return TripStartTime.Substring(11, 5);
            }
        }
    }

    public class Trip
    {
        public Summary Summary { get; set; }
    }

    public class TripResult
    {
        public List<Trip> Trips { get; set; }
    }

    public class TimeResponse
    {
        public List<string> Times { get; set; }
    }
}
