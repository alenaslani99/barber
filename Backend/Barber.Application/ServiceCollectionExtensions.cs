using Barber.Application.UseCases;
using Barber.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Barber.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<LoginUserHandler>();
        services.AddScoped<RefreshSessionHandler>();
        services.AddScoped<LogoutUserHandler>();
        services.AddScoped<CreateStaffHandler>();
        services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();
        return services;
    }
}
