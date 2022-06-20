using System;

namespace Contract.Responses;

public static class TimeZone
{
    public class Response
    {
        public DateTime DateTimeInTimeZone { get; set; }
    }
}