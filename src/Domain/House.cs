using System;
using System.Collections.Generic;

namespace Domain;

public class House
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Color { get; set; }
    public decimal Area { get; set; }
    public Guid? AddressId { get; set; }
    public Address Address { get; set; }

    public virtual ICollection<Room> Rooms { get; set; }

    public static House Create(Guid id, string name, string color, decimal area, Address address,
        List<Room> rooms = null)
    {
        return new House
        {
            Id = id == Guid.Empty ? throw new ArgumentException(nameof(id)) : id,
            Name = name ?? throw new ArgumentNullException(nameof(name)),
            Color = color ?? throw new ArgumentNullException(nameof(color)),
            Area = area,
            AddressId = address?.Id,
            Address = address,
            Rooms = rooms ?? new List<Room>()
        };
    }
}