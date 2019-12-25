using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimesOld
{
    public static class ConstantValues
    {
        public const string ApiKey = "40c5877a-7b0f-4045-9544-046d4f348bc2";
        public const string BaseUrl = "http://au-journeyplanner.silverrailtech.com";
        public const string TripsTemplate = "/journeyplannerservice/v2/REST/DataSets/PerthRestricted/StopTimetable?ApiKey={0}&format=json&StopUid=PerthRestricted%3A{1}&Time={2}&ReturnNotes=false&IsRealTimeChecked=false";
        public const string TripDetailTemplate = "/journeyplannerservice/v2/REST/DataSets/PerthRestricted/Trip?ApiKey={0}&format=json&TripUid=PerthRestricted%3A{1}&TripDate={2}&IsMappingDataReturned=false";
    }
}
