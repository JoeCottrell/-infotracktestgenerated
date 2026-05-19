using InfoTrack.API.Data;
using InfoTrack.API.DTOs;
using InfoTrack.API.Models;
using InfoTrack.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InfoTrack.API.Services;

public class LocationService : ILocationService
{
    private readonly AppDbContext _db;

    public LocationService(AppDbContext db) => _db = db;

    public async Task<List<LocationDto>> GetAllAsync() =>
        await _db.Locations
            .OrderBy(l => l.Name)
            .Select(l => new LocationDto(l.Id, l.Name, l.IsActive, l.CreatedAt))
            .ToListAsync();

    public async Task<LocationDto?> GetByIdAsync(int id)
    {
        var l = await _db.Locations.FindAsync(id);
        return l is null ? null : new LocationDto(l.Id, l.Name, l.IsActive, l.CreatedAt);
    }

    public async Task<LocationDto> CreateAsync(CreateLocationRequest request)
    {
        var location = new Location { Name = request.Name.Trim(), IsActive = true };
        _db.Locations.Add(location);
        await _db.SaveChangesAsync();
        return new LocationDto(location.Id, location.Name, location.IsActive, location.CreatedAt);
    }

    public async Task<LocationDto?> UpdateAsync(int id, UpdateLocationRequest request)
    {
        var location = await _db.Locations.FindAsync(id);
        if (location is null) return null;

        location.Name = request.Name.Trim();
        location.IsActive = request.IsActive;
        await _db.SaveChangesAsync();
        return new LocationDto(location.Id, location.Name, location.IsActive, location.CreatedAt);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var location = await _db.Locations.FindAsync(id);
        if (location is null) return false;

        _db.Locations.Remove(location);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<LocationDto?> ToggleActiveAsync(int id)
    {
        var location = await _db.Locations.FindAsync(id);
        if (location is null) return null;

        location.IsActive = !location.IsActive;
        await _db.SaveChangesAsync();
        return new LocationDto(location.Id, location.Name, location.IsActive, location.CreatedAt);
    }

    public async Task<List<string>> GetActiveNamesAsync() =>
        await _db.Locations
            .Where(l => l.IsActive)
            .OrderBy(l => l.Name)
            .Select(l => l.Name)
            .ToListAsync();
}
