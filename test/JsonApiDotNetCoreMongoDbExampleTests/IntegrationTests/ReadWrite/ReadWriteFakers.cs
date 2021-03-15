using System;
using Bogus;
using JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always
// @formatter:keep_existing_linebreaks true

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite
{
    internal sealed class ReadWriteFakers : FakerContainer
    {
        private readonly Lazy<Faker<WorkItem>> _lazyWorkItemFaker = new Lazy<Faker<WorkItem>>(() =>
            new Faker<WorkItem>()
                .UseSeed(GetFakerSeed())
                .RuleFor(workItem => workItem.Description, faker => faker.Lorem.Sentence())
                .RuleFor(workItem => workItem.DueAt, faker => faker.Date.Future())
                .RuleFor(workItem => workItem.Priority, faker => faker.PickRandom<WorkItemPriority>()));

        private readonly Lazy<Faker<WorkTag>> _lazyWorkTagFaker = new Lazy<Faker<WorkTag>>(() =>
            new Faker<WorkTag>()
                .UseSeed(GetFakerSeed())
                .RuleFor(workTag => workTag.Text, faker => faker.Lorem.Word())
                .RuleFor(workTag => workTag.IsBuiltIn, faker => faker.Random.Bool()));

        private readonly Lazy<Faker<UserAccount>> _lazyUserAccountFaker = new Lazy<Faker<UserAccount>>(() =>
            new Faker<UserAccount>()
                .UseSeed(GetFakerSeed())
                .RuleFor(userAccount => userAccount.FirstName, faker => faker.Name.FirstName())
                .RuleFor(userAccount => userAccount.LastName, faker => faker.Name.LastName()));

        private readonly Lazy<Faker<WorkItemGroup>> _lazyWorkItemGroupFaker = new Lazy<Faker<WorkItemGroup>>(() =>
            new Faker<WorkItemGroup>()
                .UseSeed(GetFakerSeed())
                .RuleFor(group => group.Name, faker => faker.Lorem.Word())
                .RuleFor(group => group.IsPublic, faker => faker.Random.Bool()));

        private readonly Lazy<Faker<RgbColor>> _lazyRgbColorFaker = new Lazy<Faker<RgbColor>>(() =>
            new Faker<RgbColor>()
                .UseSeed(GetFakerSeed())
                .RuleFor(color => color.DisplayName, faker => faker.Lorem.Word()));

        public Faker<WorkItem> WorkItem => _lazyWorkItemFaker.Value;
        public Faker<WorkTag> WorkTag => _lazyWorkTagFaker.Value;
        public Faker<UserAccount> UserAccount => _lazyUserAccountFaker.Value;
        public Faker<WorkItemGroup> WorkItemGroup => _lazyWorkItemGroupFaker.Value;
        public Faker<RgbColor> RgbColor => _lazyRgbColorFaker.Value;
    }
}
