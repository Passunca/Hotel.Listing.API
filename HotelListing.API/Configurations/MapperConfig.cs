﻿using AutoMapper;
using HotelListing.API.Data;
using HotelListing.API.Models.Country;
using HotelListing.API.Models.Hotel;
using HotelListing.API.Models.Users;

namespace HotelListing.API.Configurations;

public class MapperConfig : Profile
{
    public MapperConfig()
    {
        CreateMap<Country, CreateCountryDto>().ReverseMap();
        CreateMap<Country, GetCountryDto>().ReverseMap();
        CreateMap<Country, GetCountryDetailsDto>().ReverseMap();
        CreateMap<Country, UpdateCountryDto>().ReverseMap();

        CreateMap<Hotel, BaseHotelDto>().ReverseMap();
        CreateMap<Hotel, CreateHotelDto>().ReverseMap();
        CreateMap<Hotel, HotelDto>().ReverseMap();
        CreateMap<Hotel, UpdateHotelDto>().ReverseMap();

        CreateMap<ApiUserDto, ApiUser>().ReverseMap();
    }
}
