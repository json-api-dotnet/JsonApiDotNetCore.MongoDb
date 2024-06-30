using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.Meta;

internal sealed class MetaFakers
{
    private readonly Lazy<Faker<SupportTicket>> _lazySupportTicketFaker = new(() => new Faker<SupportTicket>()
        .MakeDeterministic()
        .RuleFor(supportTicket => supportTicket.Description, faker => faker.Lorem.Paragraph()));

    public Faker<SupportTicket> SupportTicket => _lazySupportTicketFaker.Value;
}
