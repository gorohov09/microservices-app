using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Domain.Contracts.Messages;
using Ordering.RabbitMQ;
using Ordering.RabbitMQ.Consumers;

namespace Ordering.RabbitMQ
{
    /// <summary>
	/// Класс - входная точка проекта, регистрирующий реализованные зависимости текущим проектом
	/// </summary>
	public static class Entry
    {
        /// <summary>
        /// Добавить службы проекта с очередью
        /// </summary>
        /// <param name="services">Коллекция служб</param>
        /// <param name="connectionString">Строка подключения к RMQ</param>
        /// <returns>Обновленная коллекция служб</returns>
        public static IServiceCollection AddRabbitMq(this IServiceCollection services, string connectionString)
            => services.AddMassTransit(connectionString, options => options
                .AddConsumer<BasketCheckoutIntegrationEvent, BasketCheckoutConsumer>(queueName: "test-queue", exchangeName: "test-exchange"));
    }
}
