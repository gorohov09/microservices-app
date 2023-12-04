using System;

namespace Basket.Core.Contracts.Messages
{
    public class BasketCheckoutIntegrationEvent
    {
        public Guid Id { get; private set; }

        public DateTime CreationDate { get; private set; }

        public BasketCheckoutIntegrationEvent(Guid id, DateTime creationDate)
        {
            Id = id;
            CreationDate = creationDate;
        }

        public BasketCheckoutIntegrationEvent() :
            this(Guid.NewGuid(), DateTime.Now)
        {

        }

        public string UserName { get; set; }
        public decimal TotalPrice { get; set; }

        // Адрес доставки
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string AddressLine { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }

        // Оплата
        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public string Expiration { get; set; }
        public string CVV { get; set; }
        public int PaymentMethod { get; set; }
    }
}
