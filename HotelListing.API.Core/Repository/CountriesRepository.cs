using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.API.Core.Contracts;
using HotelListing.API.Core.Exceptions;
using HotelListing.API.Core.Models.Country;
using HotelListing.API.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Core.Repository;

public class CountriesRepository : GenericRepository<Country>, ICountriesRepository
{
    private readonly HotelListingDbContext _context;
    private readonly IMapper _mapper;

    public CountriesRepository(HotelListingDbContext context, IMapper mapper) : base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<GetCountryDto> GetCountryDetails(int id)
    {
        var country = await _context.Countries.Include(c => c.Hotels)
                     .ProjectTo<GetCountryDto>(_mapper.ConfigurationProvider)
                     .FirstOrDefaultAsync(c => c.Id == id);

        if (country == null) 
        {
            throw new NotFoundException(nameof(GetCountryDetails), id);
        }

        return country;
    }
}
