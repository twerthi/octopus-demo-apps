using System.Collections.Concurrent;
using Shoalbridge.Payments.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<PaymentLedger>();

var app = builder.Build();

app.MapHealthChecks("/health");

app.MapGet("/api/info", (IConfiguration config) => new
{
    Deployment = DeploymentInfo.From(config, "Shoalbridge Financial", "Shoalbridge.Payments.Api"),
    DailyLimit = config.GetValue("Payments:DailyLimit", 10_000m),
    AuditMode = config.GetValue("Compliance:AuditMode", false)
});

app.MapGet("/api/accounts", () => PaymentLedger.Accounts);

app.MapGet("/api/payments", (PaymentLedger ledger) =>
    ledger.Payments.OrderByDescending(p => p.CreatedAt).Take(10));

app.MapPost("/api/payments", (PaymentRequest request, PaymentLedger ledger, IConfiguration config, ILogger<Program> log) =>
{
    var limit = config.GetValue("Payments:DailyLimit", 10_000m);
    var audit = config.GetValue("Compliance:AuditMode", false);

    if (request.Amount <= 0)
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["amount"] = ["Amount must be greater than zero."] });

    if (PaymentLedger.Accounts.All(a => a.Id != request.FromAccountId))
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["fromAccountId"] = ["Unknown account."] });

    var sentToday = ledger.SentToday(request.FromAccountId);
    if (sentToday + request.Amount > limit)
        return Results.Problem(
            title: "Daily limit exceeded",
            detail: $"This payment would take today's total to ${sentToday + request.Amount:N2}. The limit is ${limit:N2}.",
            statusCode: StatusCodes.Status422UnprocessableEntity);

    var payment = ledger.Record(request, audit);
    if (audit)
        log.LogInformation("AUDIT payment {PaymentId} {Amount} from {Account} to {Payee}",
            payment.Id, payment.Amount, payment.FromAccountId, payment.Payee);

    return Results.Created($"/api/payments/{payment.Id}", payment);
});

app.Run();

public record Account(string Id, string Name, string Number, decimal Balance);
public record PaymentRequest(string FromAccountId, string Payee, decimal Amount);
public record Payment(Guid Id, string FromAccountId, string Payee, decimal Amount, DateTimeOffset CreatedAt, bool Audited);

public class PaymentLedger
{
    public static readonly Account[] Accounts =
    [
        new("chk-001", "Everyday Checking", "••••4821", 4_250.18m),
        new("sav-002", "High-Yield Savings", "••••7730", 18_902.55m),
        new("bus-003", "Business Operating", "••••1094", 62_410.00m),
    ];

    private readonly ConcurrentBag<Payment> _payments = new(
    [
        new(Guid.NewGuid(), "chk-001", "Harbor Utilities", 142.60m, DateTimeOffset.UtcNow.AddDays(-1), false),
        new(Guid.NewGuid(), "bus-003", "Pier 9 Office Supply", 1_280.00m, DateTimeOffset.UtcNow.AddDays(-3), false),
    ]);

    public IEnumerable<Payment> Payments => _payments;

    public decimal SentToday(string accountId) => _payments
        .Where(p => p.FromAccountId == accountId && p.CreatedAt.UtcDateTime.Date == DateTime.UtcNow.Date)
        .Sum(p => p.Amount);

    public Payment Record(PaymentRequest request, bool audited)
    {
        var payment = new Payment(Guid.NewGuid(), request.FromAccountId, request.Payee, request.Amount, DateTimeOffset.UtcNow, audited);
        _payments.Add(payment);
        return payment;
    }
}
