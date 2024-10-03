﻿using DiamondShop.Application.Dtos.Responses.Promotions;
using DiamondShop.Application.Usecases.PromotionGifts.Commands.CreateMany;
using DiamondShop.Application.Usecases.PromotionGifts.Commands.Delete;
using DiamondShop.Application.Usecases.PromotionGifts.Queries.GetAll;
using DiamondShop.Application.Usecases.PromotionRequirements.Commands.CreateMany;
using DiamondShop.Application.Usecases.PromotionRequirements.Commands.Delete;
using DiamondShop.Application.Usecases.PromotionRequirements.Queries.GetAll;
using DiamondShop.Application.Usecases.Promotions.Commands.Create;
using DiamondShop.Application.Usecases.Promotions.Commands.Delete;
using DiamondShop.Application.Usecases.Promotions.Commands.UpdateGifts;
using DiamondShop.Application.Usecases.Promotions.Commands.UpdateRequirements;
using DiamondShop.Application.Usecases.Promotions.Queries.GetAll;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DiamondShop.Api.Controllers.Promotions
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionController : ApiControllerBase
    {
        private readonly ISender _sender;
        private readonly IMapper _mapper;

        public PromotionController(ISender sender, IMapper mapper)
        {
            _sender = sender;
            _mapper = mapper;
        }
       
     
        [HttpGet()]
        [ProducesResponseType(typeof(List<PromotionDto>),200)]
        public async Task<ActionResult> GetAllPromotion()
        {
            var result = await _sender.Send(new GetAllPromotionQuery());
            var mappedResult = _mapper.Map<List<PromotionDto>>(result);
            return Ok(mappedResult);
        }
        [HttpPost()]
        [ProducesResponseType(typeof(PromotionDto), 200)]
        public async Task<ActionResult> CreatePromotion(CreatePromotionCommand createPromotionCommand)
        {
            var result = await _sender.Send(createPromotionCommand);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            return MatchError(result.Errors, ModelState);
        }
        [HttpPut("{promotionId}/Requirement")]
        public async Task<ActionResult> UpdatePromotionRequirement([FromRoute]string promotionId, [FromBody] string[] requirementIds, [FromQuery] bool isAdd)
        {
            var result = await _sender.Send(new UpdatePromotionRequirementCommand (promotionId, requirementIds, isAdd));
            if (result.IsSuccess)
            {
                return Ok();
            }
            return MatchError(result.Errors, ModelState);
        }
        [HttpPut("{promotionId}/Gift")]
        public async Task<ActionResult> UpdateGiftRequirement([FromRoute]string promotionId, [FromBody] string[] giftIds, [FromQuery]bool isAdd)
        {
            var result = await _sender.Send(new UpdatePromotionGiftsCommand(promotionId, giftIds, isAdd));
            if (result.IsSuccess)
            {
                return Ok();
            }
            return MatchError(result.Errors, ModelState);
        }
        [HttpDelete("{promotionId}")]
        [ProducesResponseType(typeof(PromotionDto), 200)]
        public async Task<ActionResult> DeletePromotion(string promotionId)
        {
            var result = await _sender.Send(new DeletePromotionCommand(promotionId));
            if (result.IsSuccess)
            {
                var mappedResult = _mapper.Map<PromotionDto>(result.Value);
                return Ok(mappedResult);
            }
            return MatchError(result.Errors, ModelState);
        }
    }
}
