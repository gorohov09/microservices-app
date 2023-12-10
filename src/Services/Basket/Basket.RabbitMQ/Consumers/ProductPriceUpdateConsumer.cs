using Basket.API.Repositories;
using Basket.Core.Contracts.Messages;
using System;
using System.Threading.Tasks;

namespace Basket.RabbitMQ.Consumers
{
    public class ProductPriceUpdateConsumer : ConsumerBase<ProductPriceUpdatedIntegrationEvent>
    {
        private readonly IBasketRepository _basketRepository;

        public ProductPriceUpdateConsumer(IBasketRepository basketRepository)
        {
           _basketRepository = basketRepository;
        }

        public async override Task ConsumeAsync(MessageContextBase message)
        {
            var @event = message.Message;

            if (@event != null)
            {
                await _basketRepository.UpdatePrices(@event.ProductId, @event.NewPrice);
            }
        }
    }
}
