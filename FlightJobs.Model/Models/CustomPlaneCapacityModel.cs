﻿namespace FlightJobs.Model.Models
{
    public class CustomPlaneCapacityModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int CustomPassengerCapacity { get; set; }
        public int CustomCargoCapacityWeight { get; set; }
        public string CustomNameCapacity { get; set; }
        public long CustomPaxWeight { get; set; }
        public string ImagePath { get; set; }
    }
}
