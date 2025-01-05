using AutoMapper;
using HotelListing.API.Core.Models.Country;
using HotelListing.API.Core.Models.Hotel;
using HotelListing.API.Core.Models.Users;
using HotelListing.API.Data;

namespace HotelListing.API.Core.Configurations;

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
