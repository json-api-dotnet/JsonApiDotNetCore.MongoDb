using System;
using Bogus;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.Meta
{
    internal sealed class SupportFakers : FakerContainer
    {
        private readonly Lazy<Faker<SupportTicket>> _lazySupportTicketFaker = new Lazy<Faker<SupportTicket>>(() =>
            new Faker<SupportTicket>()
                .UseSeed(GetFakerSeed())
                .RuleFor(supportTicket => supportTicket.Description, faker => faker.Lorem.Paragraph()));

        public Faker<SupportTicket> SupportTicket => _lazySupportTicketFaker.Value;
    }
}
