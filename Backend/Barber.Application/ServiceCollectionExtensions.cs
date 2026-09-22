using Barber.Application.Logging;
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
        services.AddScoped<GetMyProfileHandler>();
        services.AddScoped<GetMyBookingsHandler>();
        services.AddScoped<GetBookingsHandler>();
        services.AddScoped<GetAvailabilityHandler>();
        services.AddScoped<UpdateBookingStatusHandler>();
        services.AddScoped<GetDaysOffHandler>();
        services.AddScoped<CreateDayOffHandler>();
        services.AddScoped<DeleteDayOffHandler>();
        services.AddScoped<GetBarbersHandler>();
        services.AddScoped<GetServicesHandler>();
        services.AddScoped<GetShopHandler>();
        services.AddScoped<CreateBookingHandler>();
        services.AddScoped<ChangePasswordHandler>();
        services.AddScoped<AuditWriter>();
        services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();
        return services;
    }
}
