using System.Threading.Tasks;

namespace Service;

public interface ITimeZoneService
{
    Contract.Responses.TimeZone.Response GetDateTimeInTimeZone(Contract.Requests.TimeZone.Request timeZoneRequest);
}