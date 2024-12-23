using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;

using System.Runtime.CompilerServices;

namespace WizBuz
{
    public class WizBuzService
    {
        // Constructor for a class that sets up a server that listens on port 6969
        public WizBuzService()
        {
            var createWeb = WebApplication.CreateBuilder(args)
                .ConfigureAppConfiguration((hostingConfig, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
            });

        }
    }  
}
