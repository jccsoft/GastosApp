using FluentValidation.Results;

namespace Gastos.Api.Shared.Extensions;

public static class ValidationExtensions
{
    public static IResult ToResult(this ValidationResult validationResult)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }
}
