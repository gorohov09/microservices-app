using Catalog.API.Entities;
using Catalog.API.Repositories;
using Catalog.Core.Abstractions;
using Catalog.Core.Contracts.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IRabbitMessagePublisher _publishEndpoint;
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(IProductRepository productRepository, IRabbitMessagePublisher publishEndpoint, ILogger<CatalogController> logger)
        {
            _productRepository = productRepository ??  throw new ArgumentNullException(nameof(productRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Product>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _productRepository.GetProducts();
            return Ok(products);
        }

        [HttpGet("{id:length(24)}", Name = "GetProduct")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductById(string id)
        {
            var product = await _productRepository.GetProductById(id);
            if (product is null)
            {
                _logger.LogInformation($"Product with id: {id}, not found");
                return NotFound();
            }

            return Ok(product);
        }

        [Route("[action]/{category}", Name = "GetProductsByCategory")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Product>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByCategory(string category)
        {
            var products = await _productRepository.GetProductsByCategory(category);
            return Ok(products);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Product>>> CreateProduct([FromBody] Product product)
        {
            await _productRepository.CreateProduct(product);
            return CreatedAtRoute("GetProduct", new { id = product.Id }, product);
        }

        [HttpPut]
        [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Product>>> UpdateProduct([FromBody] Product product)
        {
            var oldProduct = await _productRepository.GetProductById(product.Id);

            #region TODO: Реализовать паттерн Outbox

            //1-ая операция(Сохранение данных в БД)
            var result = await _productRepository.UpdateProduct(product);

            if (result && oldProduct?.Price != product.Price)
            {
                //2-ая операция(Отправление данных в брокер)
                await _publishEndpoint.PublishAsync(new ProductPriceUpdatedIntegrationEvent
                {
                    NewPrice = product.Price,
                    ProductId = product.Id
                });
            }
            #endregion

            return Ok(result);
        }

        [HttpDelete("{id:length(24)}", Name = "DeleteProduct")]
        [ProducesResponseType(typeof(Product), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<Product>>> DeleteProduct(string id)
        {
            return Ok(await _productRepository.DeleteProduct(id));
        }

        //[Route("[action]/{name}", Name = "GetProductsByName")]
        //[HttpGet]
        //[ProducesResponseType(typeof(IEnumerable<Product>), (int)HttpStatusCode.OK)]
        //public async Task<ActionResult<IEnumerable<Product>>> GetProductsByName(string category)
        //{
        //    var products = await _productRepository.GetProductsByName(category);
        //    return Ok(products);
        //}
    }
}
