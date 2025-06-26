using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations;
using WeatherGuard.Core.DTOs.Common;

namespace WeatherGuard.Api.Filters;

/// <summary>
/// Global model validation filter
/// </summary>
public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );

            var response = new ApiResponseDto<object>
            {
                Success = false,
                Message = "Validation failed",
                Data = errors
            };

            context.Result = new BadRequestObjectResult(response);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Nothing to do here
    }
}

/// <summary>
/// Custom validation attributes
/// </summary>
public class GuidNotEmptyAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is Guid guid)
        {
            return guid != Guid.Empty;
        }
        return false;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"The {name} field must be a valid non-empty GUID.";
    }
}

public class DateRangeAttribute : ValidationAttribute
{
    private readonly string _startDateProperty;
    private readonly string _endDateProperty;

    public DateRangeAttribute(string startDateProperty, string endDateProperty)
    {
        _startDateProperty = startDateProperty;
        _endDateProperty = endDateProperty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var startDateProperty = validationContext.ObjectType.GetProperty(_startDateProperty);
        var endDateProperty = validationContext.ObjectType.GetProperty(_endDateProperty);

        if (startDateProperty == null || endDateProperty == null)
        {
            return new ValidationResult("Invalid property names for date range validation.");
        }

        var startDate = startDateProperty.GetValue(validationContext.ObjectInstance) as DateTime?;
        var endDate = endDateProperty.GetValue(validationContext.ObjectInstance) as DateTime?;

        if (startDate.HasValue && endDate.HasValue && startDate.Value >= endDate.Value)
        {
            return new ValidationResult("Start date must be before end date.");
        }

        return ValidationResult.Success;
    }
}

public class CoordinateRangeAttribute : ValidationAttribute
{
    private readonly double _min;
    private readonly double _max;

    public CoordinateRangeAttribute(double min, double max)
    {
        _min = min;
        _max = max;
    }

    public override bool IsValid(object? value)
    {
        if (value is decimal decimalValue)
        {
            var doubleValue = (double)decimalValue;
            return doubleValue >= _min && doubleValue <= _max;
        }
        return true; // Let other validators handle null/type issues
    }

    public override string FormatErrorMessage(string name)
    {
        return $"The {name} field must be between {_min} and {_max}.";
    }
}