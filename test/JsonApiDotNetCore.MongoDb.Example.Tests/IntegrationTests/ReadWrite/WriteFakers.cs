using System;
using Bogus;
using JsonApiDotNetCore.MongoDb.Example.Tests.Helpers.Models;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests.ReadWrite
{
    internal sealed class WriteFakers : FakerContainer
    {
        private readonly Lazy<Faker<WorkItem>> _lazyWorkItemFaker = new Lazy<Faker<WorkItem>>(() =>
            new Faker<WorkItem>()
                .UseSeed(GetFakerSeed())
                .RuleFor(workItem => workItem.Description, f => f.Lorem.Sentence())
                .RuleFor(workItem => workItem.DueAt, f => f.Date.Future())
                .RuleFor(workItem => workItem.Priority, f => f.PickRandom<WorkItemPriority>()));

        private readonly Lazy<Faker<UserAccount>> _lazyUserAccountFaker = new Lazy<Faker<UserAccount>>(() =>
            new Faker<UserAccount>()
                .UseSeed(GetFakerSeed())
                .RuleFor(userAccount => userAccount.FirstName, f => f.Name.FirstName())
                .RuleFor(userAccount => userAccount.LastName, f => f.Name.LastName()));

        private readonly Lazy<Faker<RgbColor>> _lazyRgbColorFaker = new Lazy<Faker<RgbColor>>(() =>
            new Faker<RgbColor>()
                .UseSeed(GetFakerSeed())
                .RuleFor(color => color.DisplayName, f => f.Lorem.Word()));

        private readonly Lazy<Faker<Movie>> _lazyMovieFaker = new Lazy<Faker<Movie>>(() =>
            new Faker<Movie>()
                .RuleFor(movie => movie.Name, f => f.Lorem.Sentence()));
        
        private readonly Lazy<Faker<Director>> _lazyDirectorFaker = new Lazy<Faker<Director>>(() =>
            new Faker<Director>()
                .RuleFor(director => director.Name, f => f.Name.FindName()));

        public Faker<WorkItem> WorkItem => _lazyWorkItemFaker.Value;
        public Faker<UserAccount> UserAccount => _lazyUserAccountFaker.Value;
        public Faker<RgbColor> RgbColor => _lazyRgbColorFaker.Value;
        public Faker<Movie> Movie => _lazyMovieFaker.Value;
        public Faker<Director> Director => _lazyDirectorFaker.Value;
    }
}
