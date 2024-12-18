﻿using DiamondShop.Application.Commons.Responses;
using DiamondShop.Application.Dtos.Responses.Jewelries;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Application.Usecases.JewelryReviews.Commands.ChangeVisibility;
using DiamondShop.Application.Usecases.JewelryReviews.Commands.Create;
using DiamondShop.Application.Usecases.JewelryReviews.Commands.Delete;
using DiamondShop.Application.Usecases.JewelryReviews.Queries.GetAll;
using DiamondShop.Domain.Models.RoleAggregate;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiamondShop.Api.Controllers.Jewelries
{
    [Route("api/JewelryReview")]
    [ApiController]
    public class JewelryReviewController : ApiControllerBase
    {
        private readonly ISender _sender;
        private readonly IMapper _mapper;

        public JewelryReviewController(ISender sender, IMapper mapper)
        {
            _sender = sender;
            _mapper = mapper;
        }

        [HttpGet("All")]
        public async Task<ActionResult> GetAll([FromQuery] GetAllJewelryReviewQuery getAllJewelryReviewQuery)
        {
            var result = await _sender.Send(getAllJewelryReviewQuery);
            var mappedResult = _mapper.Map<PagingResponseDto<JewelryReviewDto>>(result);
            return Ok(mappedResult);
        }

        [HttpPost("Create")]
        [Authorize(Roles = AccountRole.CustomerId)]
        public async Task<ActionResult> CreateJewelryReview([FromForm] JewelryReviewRequestDto jewelryReviewRequestDto)
        {
            var userId = User.FindFirst(IJwtTokenProvider.USER_ID_CLAIM_NAME);
            if (userId != null)
            {
                var result = await _sender.Send(new CreateJewelryReviewCommand(userId.Value, jewelryReviewRequestDto));
                if (result.IsSuccess)
                {
                    var mappedResult = _mapper.Map<JewelryReviewDto>(result.Value);
                    return Ok(mappedResult);
                }
                else
                    return MatchError(result.Errors, ModelState);
            }
            else
                return Unauthorized();
        }
        [HttpPut("ChangeVisibility")]
        [Authorize(Roles = AccountRole.StaffId)]
        public async Task<ActionResult> ChangeVisibilityJewelryReview([FromQuery] string JewelryId)
        {
            var result = await _sender.Send(new ChangeVisibilityJewelryReviewCommand(JewelryId));
            if (result.IsSuccess)
            {
                var mappedResult = _mapper.Map<JewelryReviewDto>(result.Value);
                return Ok(mappedResult);
            }
            else
                return MatchError(result.Errors, ModelState);
        }
        [HttpDelete("Delete")]
        [Authorize(Roles = AccountRole.CustomerId)]
        public async Task<ActionResult> HideJewelryReview([FromQuery] string JewelryId)
        {
            var userId = User.FindFirst(IJwtTokenProvider.USER_ID_CLAIM_NAME);
            if (userId != null)
            {
                var result = await _sender.Send(new DeleteJewelryReviewCommand(userId.Value, JewelryId));
                if (result.IsSuccess)
                {
                    return Ok("Đã xóa nhận xét");
                }
                else
                    return MatchError(result.Errors, ModelState);
            }
            else
                return Unauthorized();
        }
    }
}
