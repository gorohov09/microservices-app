using Basket.Core.Entities;
using System;
using System.Threading.Tasks;

namespace Basket.API.Repositories
{
    public interface IBasketRepository
    {
        Task<ShoppingCart> GetBasket(string userName);
        Task<ShoppingCart> UpdateBasket(ShoppingCart basket);
        Task<bool> DeleteBasket(string userName);
        Task<bool> UpdatePrices(string productId, decimal newPrice);
    }
}
