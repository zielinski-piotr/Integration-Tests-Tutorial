using System.Threading.Tasks;

namespace Service;

public interface ITimeZoneService
{
    Contract.Responses.TimeZone.Response CalculateDateTimeInTimeZone(Contract.Requests.TimeZone.Request timeZoneRequest);
}