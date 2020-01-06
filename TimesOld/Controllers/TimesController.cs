using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using TimesOld;

namespace Times.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimesController : ControllerBase
    {
        private readonly ILogger<TimesController> _logger;
        
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

            if(s == "keepalive")
            {
                return Ok();
            }

            var client = new RestClient(ConstantValues.BaseUrl);
            
            Enum.TryParse(s, out TripCode station);
            //var aa = new DateTime(2020, 01, 06, 15, 00, 00);
            var request = new RestRequest(string.Format(ConstantValues.TripsTemplate, ConstantValues.ApiKey, (int)station, HttpUtility.UrlEncode(Helpers.NowAWST.ToString("yyyy-MM-ddTHH:mm"))));
            //var request = new RestRequest(string.Format(ConstantValues.TripsTemplate, ConstantValues.ApiKey, (int)station, HttpUtility.UrlEncode(Helpers.ToAWST(aa).ToString("yyyy-MM-ddTHH:mm"))));

            IRestResponse response = client.Execute(request);
            var content = response.Content; // raw content as string
            var stops = JsonConvert.DeserializeObject<TripResult>(content);

            //var test = stops.Trips.Where(x => x.DepartTime == "2020-01-06T15:13");


            var railTimes = stops.Trips.Where(x => x.Summary.Mode == "Rail" && x.Summary.RouteName == "Joondalup Line" && x.Destination.ParentName == getDestination(station));

            return Ok(new TimeResponse 
            {

                Trips = railTimes.Select(x => new ResponseTrip { 
                    TimeLabel = $"{x.StartTimeOnly} ({x.MinutesToDepartLabel})",
                    TripSourceId = x.Summary.TripSourceId
                }).ToList()
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
    }    public class ResponseTrip
    {
        public string TimeLabel { get; set; }
        public string TripSourceId { get; set; }
    }    public class Summary
    {
        public string Mode { get; set; }
        public string TripStartTime { get; set; }
        public string TripSourceId { get; set; }
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
                try
                {
                    var timePart = TripStartTime.Split('T')[1];
                    var datePart = TripStartTime.Split('T')[0];
                    if (timePart.StartsWith("24"))
                    {
                        var date = DateTime.Parse(datePart).AddDays(1);
                        TripStartTime = $"{date.ToString("yyyy-MM-dd")}T{timePart.Replace("24:", "00:")}";
                    }
                    var parsedDate = DateTime.Parse(TripStartTime);

                    TimeSpan ts = parsedDate - Helpers.NowAWST;
                    return ((int)Math.Floor(ts.TotalMinutes)).ToString();
                }
                catch
                {
                    return "-999";
                }

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
        public String DepartTime { get; set; }
        public string StartTimeOnly
        {
            get
            {
                if (string.IsNullOrEmpty(DepartTime))
                    return "";

                return DepartTime.Substring(11, 5);
            }
        }
        public string MinutesToDepart
        {
            get
            {
                try
                {
                    var timePart = DepartTime.Split('T')[1];
                    var datePart = DepartTime.Split('T')[0];
                    if (timePart.StartsWith("24"))
                    {
                        var date = DateTime.Parse(datePart).AddDays(1);
                        DepartTime = $"{date.ToString("yyyy-MM-dd")}T{timePart.Replace("24:", "00:")}";
                    }
                    var parsedDate = DateTime.Parse(DepartTime);

                    TimeSpan ts = parsedDate - Helpers.NowAWST;
                    return ((int)Math.Floor(ts.TotalMinutes)).ToString();
                }
                catch
                {
                    return "-999";
                }

            }
        }

        public string MinutesToDepartLabel
        {
            get
            {
                if (MinutesToDepart == "0" || MinutesToDepart == "-1")
                {
                    return "NOW";
                }

                return $"{MinutesToDepart} mins";
            }
        }
    }

    public class TripResult
    {
        public List<Trip> Trips { get; set; }
    }

    public class TimeResponse
    {
        public List<ResponseTrip> Trips { get; set; }
    }

    public class Destination
    {
        public string ParentName { get; set; }
    }
}
