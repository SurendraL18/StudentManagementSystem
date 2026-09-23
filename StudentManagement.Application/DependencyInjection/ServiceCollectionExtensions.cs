using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StudentManagement.Application.Users.CreateUser;

namespace StudentManagement.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {

            services.AddScoped<IValidator<CreateUserCommand>, CreateUserCommandValidator>();


            services.AddScoped<CreateUserService>();

            return services;
        }
    }
}
