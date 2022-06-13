using AutoMapper;
using Contract.Responses;
using DomainModelHouse = Domain.House;
using DomainModelAddress = Domain.Address;
using DomainModelRoom = Domain.Room;

namespace Service;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<DomainModelHouse, House.Response>();
        CreateMap<DomainModelHouse, House.ListItem>();
        CreateMap<DomainModelHouse, Contract.Requests.House.Patch>().ReverseMap();
        CreateMap<DomainModelAddress, Address.Response>();
        CreateMap<DomainModelAddress, Contract.Requests.Address.Patch>().ReverseMap();
        CreateMap<Contract.Requests.House.Update, DomainModelHouse>().ReverseMap();
        CreateMap<Contract.Requests.House.Request, DomainModelHouse>();
        CreateMap<DomainModelRoom, Room.ListItem>();
        CreateMap<DomainModelRoom, Contract.Requests.Room.Patch>().ReverseMap();
    }
}