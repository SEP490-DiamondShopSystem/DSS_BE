﻿using DiamondShop.Application.Usecases.Orders.Files.Commands;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenQA.Selenium.DevTools;

namespace DiamondShop.Api.Controllers.Orders
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderFilesController : ApiControllerBase
    {
        private readonly ISender _sender;
        private readonly IMapper _mapper;

        public OrderFilesController(ISender sender, IMapper mapper)
        {
            _sender = sender;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult> CreateOrGetInvoice(string? orderId)
        {
            var command = new GetOrCreateOrderInvoiceCommand(orderId);
            var result = await _sender.Send(command);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            return MatchError(result.Errors, ModelState);
        }
    }
}