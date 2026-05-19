namespace InfoTrack.API.DTOs;

public record LocationDto(int Id, string Name, bool IsActive, DateTime CreatedAt);

public record CreateLocationRequest(string Name);

public record UpdateLocationRequest(string Name, bool IsActive);
