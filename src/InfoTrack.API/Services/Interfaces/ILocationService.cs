using InfoTrack.API.DTOs;

namespace InfoTrack.API.Services.Interfaces;

public interface ILocationService
{
    Task<List<LocationDto>> GetAllAsync();
    Task<LocationDto?> GetByIdAsync(int id);
    Task<LocationDto> CreateAsync(CreateLocationRequest request);
    Task<LocationDto?> UpdateAsync(int id, UpdateLocationRequest request);
    Task<bool> DeleteAsync(int id);
    Task<LocationDto?> ToggleActiveAsync(int id);
    Task<List<string>> GetActiveNamesAsync();
}
