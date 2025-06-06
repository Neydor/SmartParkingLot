﻿namespace SmartParkingLot.API.DTOs.Responses
{
    public class ApiPaginatedResponse<T>
    {
        public IEnumerable<T> Data { get; set; } = [];
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}
