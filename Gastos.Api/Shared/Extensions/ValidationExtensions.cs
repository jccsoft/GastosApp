using FluentValidation.Results;

namespace Gastos.Api.Shared.Extensions;

/// <summary>
/// Provides extension methods for configuring validation services and handling validation results.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Adds FluentValidation validators from the specified assemblies to the dependency injection container.
    /// </summary>
    /// <param name="builder">The web application builder to configure.</param>
    /// <returns>The configured web application builder for method chaining.</returns>
    /// <remarks>
    /// This method registers validators from two assemblies:
    /// - The assembly containing <see cref="Gastos.Shared.IApplicationMarker"/>
    /// - The assembly containing the <see cref="Program"/> class
    /// All validators are registered with scoped lifetime.
    /// </remarks>
    public static WebApplicationBuilder AddMyValidators(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddValidatorsFromAssemblyContaining<Gastos.Shared.IApplicationMarker>(ServiceLifetime.Scoped)
            .AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Scoped);
        return builder;
    }

    /// <summary>
    /// Converts a FluentValidation validation result to an HTTP validation problem result.
    /// </summary>
    /// <param name="validationResult">The validation result to convert.</param>
    /// <returns>An <see cref="IResult"/> representing a validation problem response with HTTP 400 status.</returns>
    /// <remarks>
    /// This method transforms validation errors into a standardized problem details format
    /// that can be returned from minimal API endpoints or controllers.
    /// </remarks>
    public static IResult ToResult(this ValidationResult validationResult)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }
}
