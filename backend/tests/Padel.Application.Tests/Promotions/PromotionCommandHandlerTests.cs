using FluentAssertions;
using Padel.Application.Common.Exceptions;
using Padel.Application.Promotions;
using Padel.Application.Tests.Common;
using Padel.Domain.Enums;

namespace Padel.Application.Tests.Promotions;

public sealed class PromotionCommandHandlerTests
{
    [Fact]
    public async Task Create_ThenGet_ReturnsPromotionWithRules()
    {
        await using var context = TestDbContextFactory.Create();
        var createHandler = new CreatePromotionCommandHandler(context);

        var id = await createHandler.Handle(
            new CreatePromotionCommand(
                "Weekend offer", true, null, null,
                [new PricingRuleInput(2, DiscountType.FixedRate, 12m)]),
            CancellationToken.None);

        var promotions = await new GetPromotionsQueryHandler(context)
            .Handle(new GetPromotionsQuery(), CancellationToken.None);

        var promotion = promotions.Should().ContainSingle(p => p.Id == id).Subject;
        promotion.Name.Should().Be("Weekend offer");
        promotion.IsActive.Should().BeTrue();
        promotion.Rules.Should().ContainSingle(r => r.MinimumHours == 2 && r.DiscountValue == 12m);
    }

    [Fact]
    public async Task Update_ReplacesNameRulesAndActiveState()
    {
        await using var context = TestDbContextFactory.Create();
        var id = await new CreatePromotionCommandHandler(context).Handle(
            new CreatePromotionCommand(
                "Original", true, null, null,
                [new PricingRuleInput(1, DiscountType.FixedRate, 10m)]),
            CancellationToken.None);

        var updateHandler = new UpdatePromotionCommandHandler(context);
        var updated = await updateHandler.Handle(
            new UpdatePromotionCommand(
                id, "Renamed", false, null, null,
                [new PricingRuleInput(3, DiscountType.Percentage, 20m)]),
            CancellationToken.None);

        updated.Name.Should().Be("Renamed");
        updated.IsActive.Should().BeFalse();
        updated.Rules.Should().ContainSingle(r => r.MinimumHours == 3 && r.DiscountType == "Percentage" && r.DiscountValue == 20m);
    }

    [Fact]
    public async Task Update_ThrowsNotFoundException_WhenPromotionDoesNotExist()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new UpdatePromotionCommandHandler(context);

        var act = () => handler.Handle(
            new UpdatePromotionCommand(999, "X", true, null, null, []),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
