using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using SportsStoreWebApp.Models;
using SportsStoreWebApp.Models.Abstract;
using SportsStoreWebApp.Models.Concrete;
using SportsStoreWebApp.Models.Services;

namespace SportsStoreWebApp
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }
    public IConfiguration Configuration { get; private set; }
    public void ConfigureServices(IServiceCollection services)
    {
      services.Configure<StorageUtility>(cfg =>
      {
        if (string.IsNullOrEmpty(Configuration["StorageAccountInformation"]))
        {
          cfg.StorageAccountName = Configuration["StorageAccountInformation:StorageAccountName"];
          cfg.StorageAccountAccessKey = Configuration["StorageAccountInformation:StorageAccountAccessKey"];
        }
      });

      services.AddMvc();

      services.AddDbContext<SportsStoreDbContext>(cfg =>
      {
        cfg.UseSqlServer(Configuration["ConnectionStrings:SportsStoreConnection"], sqlServerOptionsAction: sqlOption =>
        {
          sqlOption.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
        });
      });

      if (Configuration["EnableRedisCaching"] == "true")
      {
        services.AddDistributedRedisCache(cfg => {
          cfg.Configuration = Configuration["ConnectionStrings:RedisConnection"];
          cfg.InstanceName = "master";
        });
      }

      services.AddScoped<IProductRepository, EfProductRepository>();
      services.AddScoped<IPhotoService, PhotoService>();

      services.AddApplicationInsightsTelemetry(cfg =>
      {
        cfg.InstrumentationKey = Configuration["ApplicationInsights:InstrumentationKey"];
      });

      services.AddLogging(cfg =>
      {
        cfg.AddApplicationInsights(Configuration["ApplicationInsights:InstrumentationKey"]);
        cfg.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Information);
      });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {

      var appInsightsFlag = app.ApplicationServices.GetService<Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration>();
      if (Configuration["EnableAppInsightsTelemetry"] == "false")
      {
        appInsightsFlag.DisableTelemetry = true;
      }
      else
      {
        appInsightsFlag.DisableTelemetry = false;
      }

      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseFileServer();

      using (var scope = app.ApplicationServices.CreateScope())
      {
        var context = scope.ServiceProvider.GetRequiredService<SportsStoreDbContext>();
        context.Database.Migrate();
      }

      app.UseRouting();

      app.UseEndpoints(ConfigureEndpoints);
    }

    private void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
    {
      endpoints.MapControllerRoute(name: "Default", pattern: "{controller=Product}/{action=List}/{id?}");
      //endpoints.MapControllerRoute(name: "Default", pattern: "{controller=Home}/{action=Index}/{id?}");

      //endpoints.MapGet("/", async context =>
      //{
      //  await context.Response.WriteAsync("Hello World!");
      //});
    }
  }
}
