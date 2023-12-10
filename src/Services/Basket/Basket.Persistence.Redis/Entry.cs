using Basket.API.Repositories;
using Basket.Persistence.Redis.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Basket.Persistence.Redis
{
    /// <summary>
	/// Класс - входная точка проекта, регистрирующий реализованные зависимости текущим проектом
	/// </summary>
	public static class Entry
    {
        /// <summary>
        /// Добавить службы по работе с Redis
        /// </summary>
        /// <param name="services">Коллекция служб</param>
        /// <param name="configuration">Конфигурация приложения</param>
        /// <returns>Обновленная коллекция служб</returns>
        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
                 ConnectionMultiplexer.Connect(new ConfigurationOptions
                 {
                     EndPoints = { configuration["CacheSettings:ConnectionString"] },
                     AbortOnConnectFail = false,
                 }));

            services.AddScoped<IBasketRepository, BasketRepository>();


            return services;
        }
    }
}
