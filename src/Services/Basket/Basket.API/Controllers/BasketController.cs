using AutoMapper;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Basket.Core.Abstractions;
using Basket.Core.Contracts.Messages;
using Basket.Core.Entities;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Basket.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository _basketRepository;
        private readonly ILogger<BasketController> _logger;
        private readonly DiscountGrpcService _discountGrpcService;
        private readonly IRabbitMessagePublisher _publishEndpoint;
        private readonly IMapper _mapper;

        public BasketController(
            IBasketRepository basketRepository, 
            ILogger<BasketController> logger, 
            DiscountGrpcService discountGrpcService,
            IRabbitMessagePublisher publishEndpoint,
            IMapper mapper)
        {
            _basketRepository = basketRepository ?? throw new ArgumentNullException(nameof(basketRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _discountGrpcService = discountGrpcService ?? throw new ArgumentNullException(nameof(discountGrpcService));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet("{userName}", Name = "GetBasket")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> GetBasket(string userName)
        {
            var basket = await _basketRepository.GetBasket(userName);
            return Ok(basket ?? new ShoppingCart(userName));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody] ShoppingCart basket)
        {
            // Мы получаем корзину товаров от клиента, стоит пройтись по каждому товару и приминить к нему скидку, если такая есть
            foreach (var basketItem in basket.Items)
            {
                var coupon = await _discountGrpcService.GetCouponAsync(basketItem.ProductName);
                basketItem.Price -= coupon.Amount;
            }

            return Ok(await _basketRepository.UpdateBasket(basket));
        }

        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
        {
            var existingBasket = await _basketRepository.GetBasket(basketCheckout.UserName);

            if (existingBasket == null)
                return BadRequest();

            var basketCheckoutEvent = _mapper.Map<BasketCheckoutIntegrationEvent>(basketCheckout);
            basketCheckoutEvent.TotalPrice = existingBasket.TotalPrice;
            await _publishEndpoint.PublishWithRoutingKeyAsync(basketCheckoutEvent, "basket-checkout");

            await _basketRepository.DeleteBasket(basketCheckout.UserName);

            return Accepted();
        }



        [HttpDelete("{userName}", Name = "DeleteBasket")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteBasket(string userName)
        {
            await _basketRepository.DeleteBasket(userName);
            return Ok();
        }
    }
}
