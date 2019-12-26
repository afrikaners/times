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
    public class TripDetailController : ControllerBase
    {
        private readonly ILogger<TimesController> _logger;
        private const string _baseUrl = ConstantValues.BaseUrl;
        private const string _apiKey = ConstantValues.ApiKey;
        private const string _parameterTemplate = ConstantValues.TripsTemplate;

        private enum TripCode
        {
            BUTLER = 133,
            PERTH = 64
        }

        public TripDetailController(ILogger<TimesController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("request is incorrect");
            }
            var client = new RestClient(ConstantValues.BaseUrl);
            var awst_now = TimeZoneInfo.ConvertTime(DateTime.Now,
                 TimeZoneInfo.FindSystemTimeZoneById("W. Australia Standard Time")).ToString("yyyy-MM-ddTHH:mm");
            var request = new RestRequest(string.Format(ConstantValues.TripDetailTemplate, ConstantValues.ApiKey, id, HttpUtility.UrlEncode(awst_now)));
            IRestResponse response = client.Execute(request);
            var content = response.Content; // raw content as string
            var data = JsonConvert.DeserializeObject<Trips>(content);
            data.TripStops = data.TripStops.Where(x => x.TransitStop.ParentName != "Elizabeth Quay Stn").ToList();

            return Ok(new RedactedTripResponse
            {
                Stops = data.TripStops.Select(x => new RedactedTripDetails
                {
                    ArriveTime = x.ArrivalTimeOnly,
                    DepartTime = string.IsNullOrEmpty(x.ArrivalTimeOnly) ? x.DepartureTimeOnly : "",
                    Station = x.TransitStop.ParentName
                }).ToList()
            });
        }

        public class TransitStop
        {
            public string ParentName { get; set; }
            public string Description { get; set; }

        }

        public class TripStop
        {
            public TransitStop TransitStop { get; set; }
            public int SequenceNumber { get; set; }
            public string DepartureTime { get; set; }
            public string DepartureTimeOnly { get
                {
                    if (String.IsNullOrEmpty(DepartureTime))
                    {
                        return String.Empty;
                    }

                    return DepartureTime.Split('T')[1];
                }
            }

            public string ArrivalTimeOnly
            {
                get
                {
                    if (String.IsNullOrEmpty(ArrivalTime))
                    {
                        return String.Empty;
                    }

                    return ArrivalTime.Split('T')[1];
                }
            }

            public string ArrivalTime { get; set; }
        }

        public class Trips
        {
            public List<TripStop> TripStops { get; set; }
        }


        public class RedactedTripDetails
        {
            public string Station { get; set; }
            public string DepartTime { get; set; }
            public string ArriveTime { get; set; }
        }

        public class RedactedTripResponse
        {
            public List<RedactedTripDetails> Stops { get; set; }
        }
    }

}
