﻿using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Features.Orders.Commands.CheckoutOrder;
using Ordering.Domain.Contracts.Messages;
using System;
using System.Threading.Tasks;

namespace Ordering.RabbitMQ.Consumers
{
    public class BasketCheckoutConsumer : ConsumerBase<BasketCheckoutIntegrationEvent>
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<BasketCheckoutConsumer> _logger;

        public BasketCheckoutConsumer(IMediator mediator, IMapper mapper, ILogger<BasketCheckoutConsumer> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public override async Task ConsumeAsync(MessageContextBase message)
        {
            var command = _mapper.Map<CheckoutOrderCommand>(message.Message);
            var result = await _mediator.Send(command);

            _logger.LogInformation("Заказ успешно создан с Id: {0}", result);
        }
    }
}
