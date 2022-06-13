using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Contract.Requests;
using Data;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using DomainModelHouse = Domain.House;

namespace Service;

public class HouseService : IHouseService
{
    private readonly IMapper _mapper;
    private readonly IRepository _repository;

    public HouseService(IRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task UpdateHouse(JsonPatchDocument<House.Patch> patch, Guid id)
    {
        _ = id == Guid.Empty ? throw new ArgumentException(nameof(id)) : id;
        _ = patch ?? throw new ArgumentNullException(nameof(patch));

        var domainModelHouse = await _repository.Get<DomainModelHouse>()
                                   .Include(x => x.Address)
                                   .Include(x=>x.Rooms)
                                   .FirstOrDefaultAsync(x => x.Id == id) ??
                               throw new KeyNotFoundException($"There is no {nameof(House)} with Id: {id}");

        var apiModelHouse = _mapper.Map<House.Patch>(domainModelHouse);

        patch.ApplyTo(apiModelHouse);

        _mapper.Map(apiModelHouse, domainModelHouse);

        _repository.Update(domainModelHouse);

        await _repository.SaveChangesAsync();
    }

    public async Task UpdateHouse(House.Update update, Guid id)
    {
        _ = id == Guid.Empty ? throw new ArgumentException(nameof(id)) : id;
        _ = update ?? throw new ArgumentNullException(nameof(update));

        var domainModelHouse = await _repository.Get<DomainModelHouse>()
                                   .Include(x => x.Address)
                                   .Include(x=>x.Rooms)
                                   .FirstOrDefaultAsync(x => x.Id == id) ??
                               throw new KeyNotFoundException($"There is no {nameof(House)} with Id: {id}");

        _mapper.Map(update, domainModelHouse);

        _repository.Update(domainModelHouse);

        await _repository.SaveChangesAsync();
    }

    public async Task<Contract.Responses.House.Response> GetHouseById(Guid id)
    {
        _ = id == Guid.Empty ? throw new ArgumentException(nameof(id)) : id;

        var domainModelHouse = await _repository.Get<DomainModelHouse>()
            .Include(x => x.Address)
            .Include(x => x.Rooms)
            .FirstOrDefaultAsync(x => x.Id == id);

        return domainModelHouse is null
            ? throw new KeyNotFoundException($"{nameof(DomainModelHouse)} with Id: {id} was not found")
            : _mapper.Map<Contract.Responses.House.Response>(domainModelHouse);
    }

    public async Task RemoveHouseById(Guid id)
    {
        _ = id == Guid.Empty ? throw new ArgumentException(nameof(id)) : id;

        var domainModelHouse = await _repository.Get<DomainModelHouse>()
            .Include(x => x.Address)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (domainModelHouse is null)
            throw new KeyNotFoundException($"{nameof(DomainModelHouse)} with Id: {id} was not found");

        _repository.Remove(domainModelHouse);

        await _repository.SaveChangesAsync();
    }

    public async Task<Contract.Responses.House.Response> CreateHouse(House.Request request)
    {
        _ = request ?? throw new ArgumentNullException(nameof(request));

        var house = _mapper.Map<DomainModelHouse>(request);

        await _repository.Add(house);

        await _repository.SaveChangesAsync();

        return _mapper.Map<Contract.Responses.House.Response>(house);
    }

    public async Task<IEnumerable<Contract.Responses.House.ListItem>> GetHouses()
    {
        var domainModelHouse = await _repository.Get<DomainModelHouse>().ToListAsync();

        return _mapper.Map<IEnumerable<Contract.Responses.House.ListItem>>(domainModelHouse);
    }
}