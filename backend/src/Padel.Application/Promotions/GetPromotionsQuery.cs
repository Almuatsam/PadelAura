using MediatR;

namespace Padel.Application.Promotions;

public sealed record GetPromotionsQuery : IRequest<List<PromotionDto>>;
