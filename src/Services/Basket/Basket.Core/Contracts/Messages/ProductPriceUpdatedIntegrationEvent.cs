using System;

namespace Basket.Core.Contracts.Messages
{
    public class ProductPriceUpdatedIntegrationEvent
    {
        public Guid Id { get; private set; }

        public DateTime CreationDate { get; private set; }

        public ProductPriceUpdatedIntegrationEvent(Guid id, DateTime creationDate)
        {
            Id = id;
            CreationDate = creationDate;
        }

        public ProductPriceUpdatedIntegrationEvent() :
            this(Guid.NewGuid(), DateTime.Now)
        {

        }

        public string ProductId { get; set; }

        public decimal NewPrice { get; set; }
    }
}
