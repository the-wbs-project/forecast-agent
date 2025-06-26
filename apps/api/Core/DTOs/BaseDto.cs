namespace WeatherGuard.Core.DTOs;

public abstract class BaseDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public abstract class BaseCreateDto
{
    // No common properties for create DTOs
}

public abstract class BaseUpdateDto
{
    public Guid Id { get; set; }
}