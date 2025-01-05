using HotelListing.API.Core.Models.Hotel;

namespace HotelListing.API.Core.Models.Country;

public class GetCountryDetailsDto : GetCountryDto
{
    public List<BaseHotelDto> Hotels { get; set; }
}
