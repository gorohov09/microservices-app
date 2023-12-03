using AutoMapper;
using Basket.API.Entities;
using Basket.Core.Contracts.Messages;

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
