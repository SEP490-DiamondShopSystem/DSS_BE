﻿using DiamondShop.Application.Dtos.Responses.Diamonds;
using DiamondShop.Application.Usecases.DiamondCriterias.Commands.Create;
using DiamondShop.Application.Usecases.DiamondCriterias.Commands.Delete;
using DiamondShop.Application.Usecases.DiamondCriterias.Queries.GetAll;
using DiamondShop.Application.Usecases.DiamondPrices.Commands.Create;
using DiamondShop.Application.Usecases.DiamondPrices.Commands.Delete;
using DiamondShop.Application.Usecases.DiamondPrices.Commands.UpdateMany;
using DiamondShop.Application.Usecases.DiamondPrices.Queries.GetAllByShape;
using DiamondShop.Application.Usecases.DiamondPrices.Queries.GetPaging;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DiamondShop.Api.Controllers.Diamonds
{
    [Route("api/Diamond/Price")]
    [ApiController]
    public class DiamondPriceController : ApiControllerBase
    {
        private readonly ISender _sender;
        private readonly IMapper _mapper;

        public DiamondPriceController(ISender sender, IMapper mapper)
        {
            _sender = sender;
            _mapper = mapper;
        }
        [HttpGet]
        [Produces(typeof(DiamondPriceDto))]
        public async Task<ActionResult> GetPaging([FromQuery] GetDiamondPricePagingQuery command)
        {
            var result = await _sender.Send(command);
            var mappedResult = _mapper.Map<List<DiamondPriceDto>>(result.Values);
            return Ok(mappedResult);
        }
        [HttpGet("Shape")]
        [Produces(typeof(DiamondPriceDto))]
        public async Task<ActionResult> GetPaging([FromQuery] GetAllDiamondPriceByShapeQuery query)
        {
            var result = await _sender.Send(query);
            var mappedResult = _mapper.Map<List<DiamondPriceDto>>(result);
            return Ok(mappedResult);
        }
        [HttpPost]
        [Produces(typeof(DiamondPriceDto))]
        public async Task<ActionResult> Create([FromBody] CreateDiamondPricesCommand command)
        {
            var result = await _sender.Send(command);
            if (result.IsSuccess)
            {
                var mappedResult = _mapper.Map<DiamondPriceDto>(result);
                return Ok(mappedResult);
            }
            return MatchError(result.Errors, ModelState);
        }
        [HttpDelete("{shapeId}/{criteriaId}")]
        public async Task<ActionResult> Delete(string criteriaId, string shapeId)
        {
            var result = await _sender.Send(new DeleteDiamondPriceCommand(shapeId, criteriaId));
            if (result.IsSuccess)
                return Ok();
            return MatchError(result.Errors, ModelState);
        }
        [HttpPut]
        public async Task<ActionResult> Update([FromBody] UpdateManyDiamondPricesCommand updateManyDiamondPricesCommand)
        {
            var result = await _sender.Send(updateManyDiamondPricesCommand);
            if (result.IsSuccess)
            {
                var mappedResult = _mapper.Map<List<DiamondPriceDto>>(result.Value); 
                return Ok(mappedResult);
            }
            return MatchError(result.Errors, ModelState);
        }
    }
}
