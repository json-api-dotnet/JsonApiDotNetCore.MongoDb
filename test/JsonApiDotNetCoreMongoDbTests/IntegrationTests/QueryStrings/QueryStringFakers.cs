using Bogus;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings;

internal sealed class QueryStringFakers
{
    private readonly Lazy<Faker<Blog>> _lazyBlogFaker = new(() => new Faker<Blog>()
        .MakeDeterministic()
        .RuleFor(blog => blog.Title, faker => faker.Lorem.Word())
        .RuleFor(blog => blog.PlatformName, faker => faker.Company.CompanyName()));

    private readonly Lazy<Faker<BlogPost>> _lazyBlogPostFaker = new(() => new Faker<BlogPost>()
        .MakeDeterministic()
        .RuleFor(blogPost => blogPost.Caption, faker => faker.Lorem.Sentence())
        .RuleFor(blogPost => blogPost.Url, faker => faker.Internet.Url()));

    private readonly Lazy<Faker<WebAccount>> _lazyWebAccountFaker = new(() => new Faker<WebAccount>()
        .MakeDeterministic()
        .RuleFor(webAccount => webAccount.UserName, faker => faker.Person.UserName)
        .RuleFor(webAccount => webAccount.Password, faker => faker.Internet.Password())
        .RuleFor(webAccount => webAccount.DisplayName, faker => faker.Person.FullName)
        .RuleFor(webAccount => webAccount.DateOfBirth, faker => faker.Person.DateOfBirth.TruncateToWholeMilliseconds())
        .RuleFor(webAccount => webAccount.EmailAddress, faker => faker.Internet.Email()));

    public Faker<Blog> Blog => _lazyBlogFaker.Value;
    public Faker<BlogPost> BlogPost => _lazyBlogPostFaker.Value;
    public Faker<WebAccount> WebAccount => _lazyWebAccountFaker.Value;
}
