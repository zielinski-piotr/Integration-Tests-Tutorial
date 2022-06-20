using System;

namespace Contract.Requests;

public static class TimeZone
{
    public class Request
    {
        public DateTime DateTimeInUtc { get; set; }
        public string TimeZoneId { get; set; }
    }
}