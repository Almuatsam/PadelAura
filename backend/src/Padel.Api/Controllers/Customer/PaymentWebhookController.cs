using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Padel.Application.Bookings.ProcessPaymentWebhook;

namespace Padel.Api.Controllers.Customer;

[ApiController]
[Route("api/payment")]
public sealed class PaymentWebhookController(ISender sender) : ControllerBase
{
    // Always returns 200 — even an unrecognized session_id is acknowledged rather than rejected,
    // so Thawani doesn't retry-storm us. See ProcessPaymentWebhookCommandHandler for why the body
    // is never trusted for the actual payment status, only used to look up which booking to
    // re-verify.
    [HttpPost("webhook")]
    public async Task<ActionResult> Webhook([FromBody] JsonElement body, CancellationToken cancellationToken)
    {
        await sender.Send(new ProcessPaymentWebhookCommand(ExtractSessionId(body)), cancellationToken);
        return Ok();
    }

    private static string? ExtractSessionId(JsonElement body)
    {
        if (body.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (body.TryGetProperty("session_id", out var direct) && direct.ValueKind == JsonValueKind.String)
        {
            return direct.GetString();
        }

        if (body.TryGetProperty("data", out var data)
            && data.ValueKind == JsonValueKind.Object
            && data.TryGetProperty("session_id", out var nested)
            && nested.ValueKind == JsonValueKind.String)
        {
            return nested.GetString();
        }

        return null;
    }
}
