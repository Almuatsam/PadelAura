using MediatR;

namespace Padel.Application.Bookings.ProcessPaymentWebhook;

public sealed record ProcessPaymentWebhookCommand(string? SessionId) : IRequest;
