using System;
using Bogus;
using MongoDB.Bson;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite
{
    internal sealed class ReadWriteFakers : FakerContainer
    {
        private readonly Lazy<Faker<WorkItem>> _lazyWorkItemFaker = new Lazy<Faker<WorkItem>>(() =>
            new Faker<WorkItem>()
                .UseSeed(GetFakerSeed())
                .RuleFor(workItem => workItem.Description, f => f.Lorem.Sentence())
                .RuleFor(workItem => workItem.DueAt, f => f.Date.Future())
                .RuleFor(workItem => workItem.Priority, f => f.PickRandom<WorkItemPriority>()));

        private readonly Lazy<Faker<WorkTag>> _lazyWorkTagFaker = new Lazy<Faker<WorkTag>>(() =>
            new Faker<WorkTag>()
                .UseSeed(GetFakerSeed())
                .RuleFor(workTag => workTag.Text, f => f.Lorem.Word())
                .RuleFor(workTag => workTag.IsBuiltIn, f => f.Random.Bool()));

        private readonly Lazy<Faker<UserAccount>> _lazyUserAccountFaker = new Lazy<Faker<UserAccount>>(() =>
            new Faker<UserAccount>()
                .UseSeed(GetFakerSeed())
                .RuleFor(userAccount => userAccount.FirstName, f => f.Name.FirstName())
                .RuleFor(userAccount => userAccount.LastName, f => f.Name.LastName()));

        private readonly Lazy<Faker<WorkItemGroup>> _lazyWorkItemGroupFaker = new Lazy<Faker<WorkItemGroup>>(() =>
            new Faker<WorkItemGroup>()
                .UseSeed(GetFakerSeed())
                .RuleFor(group => group.Name, f => f.Lorem.Word())
                .RuleFor(group => group.IsPublic, f => f.Random.Bool()));

        private readonly Lazy<Faker<RgbColor>> _lazyRgbColorFaker = new Lazy<Faker<RgbColor>>(() =>
            new Faker<RgbColor>()
                .UseSeed(GetFakerSeed())
                .RuleFor(color => color.DisplayName, f => f.Lorem.Word()));

        public Faker<WorkItem> WorkItem => _lazyWorkItemFaker.Value;
        public Faker<WorkTag> WorkTag => _lazyWorkTagFaker.Value;
        public Faker<UserAccount> UserAccount => _lazyUserAccountFaker.Value;
        public Faker<WorkItemGroup> WorkItemGroup => _lazyWorkItemGroupFaker.Value;
        public Faker<RgbColor> RgbColor => _lazyRgbColorFaker.Value;
    }
}
