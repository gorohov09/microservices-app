using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Ordering.Application;
using Ordering.Infrastructure;
using System.Reflection;
using Ordering.RabbitMQ;

namespace Ordering.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddApplicationServices()
                .AddInfrastructureServices(Configuration);

            //services.AddMassTransit(config => {

            //    config.AddConsumer<BasketCheckoutConsumer>();

            //    config.UsingRabbitMq((ctx, cfg) => {
            //        var host = Configuration["EventBusSettings:HostAddress"];

            //        cfg.Host(host);

            //        cfg.ReceiveEndpoint(EventBusConstants.BasketCheckoutQueue, c =>
            //        {
            //            c.ConfigureConsumer<BasketCheckoutConsumer>(ctx);
            //        });
            //    });
            //});
            //services.AddMassTransitHostedService();

            //services.AddScoped<BasketCheckoutConsumer>();

            //Добавление кролика
            services.AddRabbitMq(Configuration["EventBusSettings:HostAddress"]);

            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ordering.API", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ordering.API v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
