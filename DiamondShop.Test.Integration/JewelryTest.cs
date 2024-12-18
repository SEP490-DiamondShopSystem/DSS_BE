﻿using DiamondShop.Application.Dtos.Requests.Jewelries;
using DiamondShop.Application.Usecases.Jewelries.Commands.Create;
using DiamondShop.Application.Usecases.Jewelries.Commands.Delete;
using DiamondShop.Application.Usecases.JewelryModels.Commands.Delete;
using DiamondShop.Domain.Common.Enums;
using DiamondShop.Domain.Models.Diamonds;
using DiamondShop.Domain.Models.Jewelries;
using DiamondShop.Domain.Models.JewelryModels;
using DiamondShop.Domain.Models.JewelryModels.ErrorMessages;
using DiamondShop.Domain.Repositories.JewelryModelRepo;
using DiamondShop.Domain.Repositories.JewelryRepo;
using DiamondShop.Test.Integration.Data;
using FluentResults;
using HtmlAgilityPack.CssSelectors.NetCore;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium.BiDi.Modules.BrowsingContext;
using Xunit.Abstractions;

namespace DiamondShop.Test.Integration
{
    public class JewelryTest : BaseIntegrationTest
    {
        protected readonly ITestOutputHelper _output;
        public JewelryTest(IntegrationWAF factory, ITestOutputHelper output) : base(factory)
        {
            _output = output;
        }
        [Trait("ReturnTrue", "DeleteJewelry")]
        [Fact]
        public async Task Delete_Jewelry_Should_Detach_Diamond()
        {
            var jewelry = await TestData.SeedDefaultJewelry(_context);
            var diamond = await TestData.SeedDefaultDiamond(_context, jewelry.Id);
            Assert.NotNull(diamond.JewelryId);
            _context.Set<Jewelry>().Remove(jewelry);
            await _context.SaveChangesAsync();
            var jewelries = _context.Set<Jewelry>().ToList();
            Assert.Equal(0, jewelries.Count);
            Assert.Null(diamond.JewelryId);
        }

        [Trait("ReturnTrue", "DefaultRing")]
        [Fact]
        public async Task Create_DefaultRing_Should_AddToDb()
        {
            var model = await TestData.SeedDefaultRingModel(_context);
            var diamond = await TestData.SeedDefaultDiamond(_context);

            var jewelryReq = new JewelryRequestDto(model.Id.Value, TestData.SizeIds[0].Value, TestData.MetalIds[0].Value, null, Domain.Common.Enums.ProductStatus.Active);
            var attachedDiamonds = new List<string>() { diamond.Id.Value };
            var command = new CreateJewelryCommand(jewelryReq, model.SideDiamonds.FirstOrDefault().Id.Value, attachedDiamonds);
            var result = await _sender.Send(command);
            if (result.IsFailed)
            {
                _output.WriteLine(result.Errors[0].Message);
            }
            Assert.True(result.IsSuccess);
        }
        [Trait("ReturnTrue", "MultiMainDiamondRing")]
        [Fact]
        public async Task Create_MultiMainDiamondRing_Should_AddToDb()
        {
            var model = await TestData.SeedMultiMainDiamondRingModel(_context);
            var diamonds = await TestData.SeedDefaultDiamonds(_context, 3, "1");

            var jewelryReq = new JewelryRequestDto(model.Id.Value, TestData.SizeIds[0].Value, TestData.MetalIds[0].Value, null, Domain.Common.Enums.ProductStatus.Active);
            var attachedDiamonds = diamonds.Select(p => p.Id.Value).ToList();
            var command = new CreateJewelryCommand(jewelryReq, model.SideDiamonds.FirstOrDefault().Id.Value, attachedDiamonds);
            var result = await _sender.Send(command);
            if (result.IsFailed)
            {
                _output.WriteLine(result.Errors[0].Message);
            }
            Assert.True(result.IsSuccess);
        }
        [Trait("ReturnTrue", "NoDiamondRing")]
        [Fact]
        public async Task Create_NoDiamondRing_Should_AddToDb()
        {
            var model = await TestData.SeedNoDiamondRingModel(_context);

            var jewelryReq = new JewelryRequestDto(model.Id.Value, TestData.SizeIds[0].Value, TestData.MetalIds[0].Value, null, Domain.Common.Enums.ProductStatus.Active);
            var command = new CreateJewelryCommand(jewelryReq, null, null);
            var result = await _sender.Send(command);
            if (result.IsFailed)
            {
                _output.WriteLine(result.Errors[0].Message);
            }
            Assert.True(result.IsSuccess);
        }
        [Trait("ReturnTrue", "DefaultNecklace")]
        [Fact]
        public async Task Create_DefaultNecklace_Should_AddToDb()
        {
            var model = await TestData.SeedDefaultNecklaceModel(_context);
            var diamond = await TestData.SeedDefaultDiamond(_context);

            var jewelryReq = new JewelryRequestDto(model.Id.Value, TestData.SizeIds[0].Value, TestData.MetalIds[0].Value, null, Domain.Common.Enums.ProductStatus.Active);
            var attachedDiamonds = new List<string>() { diamond.Id.Value };
            var command = new CreateJewelryCommand(jewelryReq, model.SideDiamonds.FirstOrDefault().Id.Value, attachedDiamonds);
            var result = await _sender.Send(command);
            if (result.IsFailed)
            {
                _output.WriteLine(result.Errors[0].Message);
            }
            Assert.True(result.IsSuccess);
        }
        [Trait("ReturnFalse", "UnmatchingMainDiamondCount")]
        [Fact]
        public async Task Create_DefaultRing_WithDifferentDiamondCount_ShouldNot_AddToDb()
        {
            var model = await TestData.SeedMultiMainDiamondRingModel(_context);
            var jewelryReq = new JewelryRequestDto(model.Id.Value, TestData.SizeIds[0].Value, TestData.MetalIds[0].Value, null, Domain.Common.Enums.ProductStatus.Active);
            var command = new CreateJewelryCommand(jewelryReq, model.SideDiamonds.FirstOrDefault().Id.Value, null);
            var result = await _sender.Send(command);
            if (result.IsFailed)
            {
                _output.WriteLine(result.Errors[0].Message);
            }
            Assert.True(result.IsFailed);
        }
        [Trait("ReturnFalse", "UnmatchingMainDiamondShape")]
        [Fact]
        public async Task Create_DefaultRing_WithDifferentDiamondShape_ShouldNot_AddToDb()
        {
            var model = await TestData.SeedMultiMainDiamondRingModel(_context);
            var diamonds = await TestData.SeedDefaultDiamonds(_context, 3, "4");

            var jewelryReq = new JewelryRequestDto(model.Id.Value, TestData.SizeIds[0].Value, TestData.MetalIds[0].Value, null, Domain.Common.Enums.ProductStatus.Active);
            var attachedDiamonds = diamonds.Select(p => p.Id.Value).ToList();
            var command = new CreateJewelryCommand(jewelryReq, model.SideDiamonds.FirstOrDefault().Id.Value, attachedDiamonds);
            var result = await _sender.Send(command);
            if (result.IsFailed)
            {
                _output.WriteLine(result.Errors[0].Message);
            }
            Assert.True(result.IsFailed);
        }
        [Trait("ReturnTrue", "NoConflict")]
        [Fact]
        public async Task No_Conflicting_Code()
        {
            var model = await TestData.SeedNoDiamondRingModel(_context);
            var jewelryReq = new JewelryRequestDto(model.Id.Value, TestData.SizeIds[0].Value, TestData.MetalIds[0].Value, "ABC", Domain.Common.Enums.ProductStatus.Active);
            var createResult = await _sender.Send(new CreateJewelryCommand(jewelryReq, null, null));
            if (createResult.IsFailed)
            {
                _output.WriteLine(createResult.Errors[0].Message);
            }
            Assert.True(createResult.IsSuccess);
            var createSecondResult = await _sender.Send(new CreateJewelryCommand(jewelryReq, null, null));
            if (createSecondResult.IsFailed)
            {
                _output.WriteLine(createSecondResult.Errors[0].Message);
            }
            Assert.True(createSecondResult.IsFailed);
        }
        [Trait("ReturnTrue", "DeleteJewelryStillCreateNewOne")]
        [Fact]
        public async Task Delete_Jewelry_Should_StillAbleTo_Create_New_One_AddToDb()
        {
            var model = await TestData.SeedNoDiamondRingModel(_context);
            var jewelryReq = new JewelryRequestDto(model.Id.Value, TestData.SizeIds[0].Value, TestData.MetalIds[0].Value, null, Domain.Common.Enums.ProductStatus.Active);
            for (int i = 0; i < 3; i++)
            {
                var createResult = await _sender.Send(new CreateJewelryCommand(jewelryReq, null, null));
                if (createResult.IsFailed)
                {
                    _output.WriteLine(createResult.Errors[0].Message);
                }
                Assert.True(createResult.IsSuccess);
            }
            var jewelryBefore = await _context.Set<Jewelry>().ToListAsync();
            foreach (var jewelry in jewelryBefore)
                _output.WriteLine(jewelry.SerialCode);
            var deleteResult = await _sender.Send(new DeleteJewelryCommand(jewelryBefore[1].Id.Value));
            if (deleteResult.IsFailed)
            {
                _output.WriteLine(deleteResult.Errors[0].Message);
            }
            Assert.True(deleteResult.IsSuccess);
            var jewelryWhile = await _context.Set<Jewelry>().ToListAsync();
            _output.WriteLine("");
            foreach (var jewelry in jewelryWhile)
                _output.WriteLine(jewelry.SerialCode);
            var command = new CreateJewelryCommand(jewelryReq, null, null);
            var result = await _sender.Send(command);
            if (result.IsFailed)
            {
                _output.WriteLine(result.Errors[0].Message);
            }
            Assert.True(result.IsSuccess);
            _output.WriteLine("");
            var jewelryAfter = await _context.Set<Jewelry>().ToListAsync();
            foreach (var jewelry in jewelryAfter)
                _output.WriteLine(jewelry.SerialCode);
        }
        [Trait("ReturnTrue", "DeleteJewelryStillCreateNewOne")]
        [Fact]
        public async Task Delete_Jewelry_Model()
        {
            var model = await TestData.SeedNoDiamondRingModel(_context);
            _output.WriteLine(model.Id.Value);
            var jewelryReq = new JewelryRequestDto(model.Id.Value, TestData.SizeIds[0].Value, TestData.MetalIds[0].Value, null, Domain.Common.Enums.ProductStatus.Sold);
            for (int i = 0; i < 3; i++)
            {
                var createResult = await _sender.Send(new CreateJewelryCommand(jewelryReq, null, null));
                if (createResult.IsFailed)
                {
                    _output.WriteLine(createResult.Errors[0].Message);
                }
                Assert.True(createResult.IsSuccess);
            }
            var isExistingFlag = await _context.Set<Jewelry>().Where(p => p.Status != ProductStatus.Sold && p.ModelId == model.Id).AnyAsync();
            Assert.False(isExistingFlag);
            _context.Set<JewelryModel>().Remove(model);
            await _context.SaveChangesAsync();
            var models = await _context.Set<JewelryModel>().ToListAsync();
            _output.WriteLine(models.Count().ToString());
            foreach (var modelOne in models)
            {
                _output.WriteLine($"{modelOne.Id.Value}");
            }
            var jewelries = await _context.Set<Jewelry>().ToListAsync();
            foreach(var jewelry in jewelries)
            {
                _output.WriteLine($"{jewelry.Id.Value} {jewelry.SerialCode} {jewelry.ModelId}");
            }
            Assert.NotNull(jewelries);
        }
    }
}
