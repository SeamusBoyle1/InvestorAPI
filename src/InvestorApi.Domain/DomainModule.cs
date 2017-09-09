using InvestorApi.Contracts;
using InvestorApi.Domain.Providers;
using InvestorApi.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InvestorApi.Domain
{
    public static class DomainModule
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();

            services.AddSingleton<PasswordHashingProvider>();
        }
    }
}