using AutoMapper;
using Basket.Core.Contracts.Messages;
using Basket.Core.Entities;

namespace Basket.API.Mapper
{
    public class BasketProfile : Profile
    {
        public BasketProfile()
        {
            CreateMap<BasketCheckout, BasketCheckoutIntegrationEvent>().ReverseMap();
        }
    }
}
