using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimesOld
{
    public static class Helpers
    {
        public static DateTime NowAWST
        { get
            {
                return TimeZoneInfo.ConvertTime(DateTime.Now,
                    TimeZoneInfo.FindSystemTimeZoneById("W. Australia Standard Time"));
            }
        }

    }
}
