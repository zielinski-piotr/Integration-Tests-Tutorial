using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimeZone = Contract.Responses.TimeZone;

namespace Service;

public class TimeZoneService : ITimeZoneService
{
    public TimeZone.Response CalculateDateTimeInTimeZone(
        Contract.Requests.TimeZone.Request timeZoneRequest)
    {
        try
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneRequest.TimeZoneId);
            var utcDateTime = DateTime.SpecifyKind(timeZoneRequest.DateTimeInUtc, DateTimeKind.Utc);
            var dateTimeInTimeZone = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timeZoneInfo);
            return new TimeZone.Response()
            {
                DateTimeInTimeZone = dateTimeInTimeZone
            };
        }
        catch (TimeZoneNotFoundException)
        {
            throw new KeyNotFoundException($"Unable to find the {timeZoneRequest.TimeZoneId} zone in the registry.");
        }
    }
}