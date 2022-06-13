using System;

namespace Domain;

public class Room
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Color { get; set; }
    public decimal Area { get; set; }
    public Guid HouseId { get; set; }
}