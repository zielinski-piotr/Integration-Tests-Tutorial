using System;
using System.Collections.Generic;

namespace Contract.Responses;

public static class House
{
    public class Response
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public decimal Area { get; set; }
        public Address.Response Address { get; set; }
        public IEnumerable<Room.ListItem> Rooms { get; set; }
    }

    public class ListItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
    }
}