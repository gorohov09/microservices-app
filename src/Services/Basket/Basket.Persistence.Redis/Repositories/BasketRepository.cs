using Basket.API.Repositories;
using Basket.Core.Entities;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.Persistence.Redis.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly IDatabase _database;

        public BasketRepository(IConnectionMultiplexer connectionMultiplexer)
        {
            _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
            _database = _connectionMultiplexer.GetDatabase();
        }

        public async Task<bool> DeleteBasket(string userName)
        {
            return await _database.KeyDeleteAsync(userName);
        }

        public async Task<ShoppingCart> GetBasket(string userName)
        {
            var basketString = await _database.StringGetAsync(userName);

            if (basketString.HasValue)
            {
                return JsonConvert.DeserializeObject<ShoppingCart>(basketString.ToString());
            }

            return new ShoppingCart(userName);
        }

        public async Task<ShoppingCart> UpdateBasket(ShoppingCart basket)
        {
            var result = await _database.StringSetAsync(basket.UserName, JsonConvert.SerializeObject(basket));
            if (result)
                return await GetBasket(basket.UserName);

            return basket;
        }

        public async Task<bool> UpdatePrices(string productId, decimal newPrice)
        {
            var server = GetServer();
            var keysData = server.Keys();

            foreach (var key in keysData)
            {
                var basket = await GetBasket(key);
                if (basket is not null)
                {
                    var products = basket.Items.Where(x => x.ProductId == productId);
                    foreach (var product in products)
                    {
                        product.Price = newPrice;
                    }

                    await UpdateBasket(basket);
                }
            }

            return true;
        }

        private IServer GetServer()
        {
            var endpoint = _connectionMultiplexer.GetEndPoints();
            return _connectionMultiplexer.GetServer(endpoint.First());
        }
    }
}
