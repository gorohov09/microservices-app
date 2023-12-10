using Microsoft.AspNetCore.Mvc;
using Shopping.Aggregator.Services;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using System;
using Shopping.Aggregator.Models;

namespace Shopping.Aggregator.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ShoppingController : ControllerBase
    {
        private readonly IBasketService _basketService;

        public ShoppingController(IBasketService basketService)
        {
            _basketService = basketService ?? throw new ArgumentNullException(nameof(basketService));
        }

        [HttpGet("{userName}", Name = "GetShopping")]
        [ProducesResponseType(typeof(BasketModel), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<BasketModel>> GetShopping(string userName)
        {
            var basket = await _basketService.GetBasket(userName);

            return Ok(basket);
        }
    }
}
