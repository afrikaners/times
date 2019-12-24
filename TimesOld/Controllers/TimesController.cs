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
    [Route("api/[controller]")]
    [ApiController]
    public class TimesController : ControllerBase
    {
        private readonly ILogger<TimesController> _logger;
        private const string _baseUrl = "http://au-journeyplanner.silverrailtech.com";
        private const string _apiKey = "40c5877a-7b0f-4045-9544-046d4f348bc2";
        private const string _parameterTemplate = "/journeyplannerservice/v2/REST/DataSets/PerthRestricted/StopTimetable?ApiKey={0}&format=json&StopUid=PerthRestricted%3A{1}&Time={2}&ReturnNotes=false&IsRealTimeChecked=false";

        private enum TripCode
        {
            BUTLER = 133,
            PERTH = 64
        }

        public TimesController(ILogger<TimesController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return BadRequest("request is incorrect");
            }
            var client = new RestClient(_baseUrl);
            
            Enum.TryParse(s, out TripCode station);

            var awst_now = TimeZoneInfo.ConvertTime(DateTime.Now,
                 TimeZoneInfo.FindSystemTimeZoneById("W. Australia Standard Time")).ToString("yyyy-MM-ddTHH:mm");
            var request = new RestRequest(string.Format(_parameterTemplate,_apiKey, (int)station, HttpUtility.UrlEncode(awst_now)));
            //var request = new RestRequest($"/journeyplannerservice/v2/REST/DataSets/PerthRestricted/StopTimetable?ApiKey=40c5877a-7b0f-4045-9544-046d4f348bc2&format=json&StopUid=PerthRestricted%3A133&Time={HttpUtility.UrlEncode(awst_now)}&ReturnNotes=false&IsRealTimeChecked=false", Method.GET);

            IRestResponse response = client.Execute(request);
            var content = response.Content; // raw content as string
            var stops = JsonConvert.DeserializeObject<TripResult>(content);

            
            var railTimes = stops.Trips.Where(x => x.Summary.Mode == "Rail" && x.Summary.RouteName == "Joondalup Line" && x.Destination.ParentName == getDestination(station));

            

            return Ok(new TimeResponse 
            {

                Times = railTimes.Select(x => $"{x.Summary.StartTimeOnly} ({x.Summary.MiutesToDepartLabel})").ToList()
            });
        }

        private string getDestination(TripCode tripCode)
        {
            switch (tripCode)
            {
                case TripCode.BUTLER:
                    return "Elizabeth Quay Stn";
                case TripCode.PERTH:
                    return "Butler Stn";
            }

            return "";
        }
    }    public class Summary
    {
        public string Mode { get; set; }
        public string TripStartTime { get; set; }
        public string RouteName { get; set; }
        public string StartTimeOnly { get
            {
                if(string.IsNullOrEmpty(TripStartTime))
                    return "";

                return TripStartTime.Substring(11, 5);
            }
        }
        public string MinutesToDepart
        {
            get
            {
                var parsedDate = DateTime.Parse(TripStartTime);
                var awst_now = TimeZoneInfo.ConvertTime(DateTime.Now,
                 TimeZoneInfo.FindSystemTimeZoneById("W. Australia Standard Time"));

                TimeSpan ts = parsedDate - awst_now;
                return ((int)Math.Floor(ts.TotalMinutes)).ToString();
            }
        }

        public string MiutesToDepartLabel
        {
            get
            {
                if(MinutesToDepart == "0" || MinutesToDepart == "-1")
                {
                    return "NOW";
                }

                return $"{MinutesToDepart} mins";
            }
        }
    }

    public class Trip
    {
        public Summary Summary { get; set; }
        public Destination Destination { get; set; }
    }

    public class TripResult
    {
        public List<Trip> Trips { get; set; }
    }

    public class TimeResponse
    {
        public List<string> Times { get; set; }
    }

    public class Destination
    {
        public string ParentName { get; set; }
    }
}
