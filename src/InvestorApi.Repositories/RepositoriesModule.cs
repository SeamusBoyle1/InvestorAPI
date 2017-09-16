﻿using InvestorApi.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace InvestorApi.Repositories
{
    public static class RepositoriesModule
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<ISettingRepository, SettingRepository>();
        }

        public static void ConfigureDbContext(DbContextOptionsBuilder options)
        {
            string url = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (string.IsNullOrEmpty(url))
            {
                url = "postgres://wmjepiakfzqvwv:28db1b434fb4e6b6b9f7710476623687496aebc4e4b8f82048a5668fcc8071fe@ec2-54-221-207-192.compute-1.amazonaws.com:5432/d48fqc1i7c9r2o";
                //throw new InvalidOperationException("Environment variable with database connection string not found.");
            }

            string[] urlSegments = url.Split(new[] { '/', ':', '@' }, StringSplitOptions.RemoveEmptyEntries);

            string username = urlSegments[1];
            string password = urlSegments[2];
            string host = urlSegments[3];
            string port = urlSegments[4];
            string database = urlSegments[5];

            string connection = $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=True";

            options.UseNpgsql(connection);
        }
    }
}
