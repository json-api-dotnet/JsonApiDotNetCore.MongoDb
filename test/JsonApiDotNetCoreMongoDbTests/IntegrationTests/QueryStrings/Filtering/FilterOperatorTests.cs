using System.Globalization;
using System.Net;
using System.Web;
using FluentAssertions;
using FluentAssertions.Extensions;
using Humanizer;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings.Filtering;

public sealed class FilterOperatorTests : IClassFixture<IntegrationTestContext<TestableStartup, FilterDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup, FilterDbContext> _testContext;

    public FilterOperatorTests(IntegrationTestContext<TestableStartup, FilterDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<FilterableResourcesController>();
    }

    [Fact]
    public async Task Can_filter_equality_on_special_characters()
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeString = "This, that & more + some"
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, new FilterableResource());
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter=equals(someString,'{HttpUtility.UrlEncode(resource.SomeString)}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someString").With(value => value.Should().Be(resource.SomeString));
    }

    [Fact]
    public async Task Cannot_filter_equality_on_two_attributes()
    {
        // Arrange
        const string route = "/filterableResources?filter=equals(someInt32,otherInt32)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Comparing attributes against each other is not supported when using MongoDB.");
        error.Detail.Should().BeNull();
    }

    [Theory]
    [InlineData(19, 21, ComparisonOperator.LessThan, 20)]
    [InlineData(19, 21, ComparisonOperator.LessThan, 21)]
    [InlineData(19, 21, ComparisonOperator.LessOrEqual, 20)]
    [InlineData(19, 21, ComparisonOperator.LessOrEqual, 19)]
    [InlineData(21, 19, ComparisonOperator.GreaterThan, 20)]
    [InlineData(21, 19, ComparisonOperator.GreaterThan, 19)]
    [InlineData(21, 19, ComparisonOperator.GreaterOrEqual, 20)]
    [InlineData(21, 19, ComparisonOperator.GreaterOrEqual, 21)]
    public async Task Can_filter_comparison_on_whole_number(int matchingValue, int nonMatchingValue, ComparisonOperator filterOperator, double filterValue)
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeInt32 = matchingValue
        };

        var otherResource = new FilterableResource
        {
            SomeInt32 = nonMatchingValue
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter={filterOperator.ToString().Camelize()}(someInt32,'{filterValue}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someInt32").With(value => value.Should().Be(resource.SomeInt32));
    }

    [Theory]
    [InlineData(1.9, 2.1, ComparisonOperator.LessThan, 2.0)]
    [InlineData(1.9, 2.1, ComparisonOperator.LessThan, 2.1)]
    [InlineData(1.9, 2.1, ComparisonOperator.LessOrEqual, 2.0)]
    [InlineData(1.9, 2.1, ComparisonOperator.LessOrEqual, 1.9)]
    [InlineData(2.1, 1.9, ComparisonOperator.GreaterThan, 2.0)]
    [InlineData(2.1, 1.9, ComparisonOperator.GreaterThan, 1.9)]
    [InlineData(2.1, 1.9, ComparisonOperator.GreaterOrEqual, 2.0)]
    [InlineData(2.1, 1.9, ComparisonOperator.GreaterOrEqual, 2.1)]
    public async Task Can_filter_comparison_on_fractional_number(double matchingValue, double nonMatchingValue, ComparisonOperator filterOperator,
        double filterValue)
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeDouble = matchingValue
        };

        var otherResource = new FilterableResource
        {
            SomeDouble = nonMatchingValue
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter={filterOperator.ToString().Camelize()}(someDouble,'{filterValue}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someDouble").With(value => value.Should().Be(resource.SomeDouble));
    }

    [Theory]
    [InlineData("2001-01-01", "2001-01-09", ComparisonOperator.LessThan, "2001-01-05Z")]
    [InlineData("2001-01-01", "2001-01-09", ComparisonOperator.LessThan, "2001-01-09Z")]
    [InlineData("2001-01-01", "2001-01-09", ComparisonOperator.LessOrEqual, "2001-01-05Z")]
    [InlineData("2001-01-01", "2001-01-09", ComparisonOperator.LessOrEqual, "2001-01-01Z")]
    [InlineData("2001-01-09", "2001-01-01", ComparisonOperator.GreaterThan, "2001-01-05Z")]
    [InlineData("2001-01-09", "2001-01-01", ComparisonOperator.GreaterThan, "2001-01-01Z")]
    [InlineData("2001-01-09", "2001-01-01", ComparisonOperator.GreaterOrEqual, "2001-01-05Z")]
    [InlineData("2001-01-09", "2001-01-01", ComparisonOperator.GreaterOrEqual, "2001-01-09Z")]
    public async Task Can_filter_comparison_on_DateTime_in_UTC_time_zone(string matchingDateTime, string nonMatchingDateTime, ComparisonOperator filterOperator,
        string filterDateTime)
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeDateTimeInUtcZone = DateTime.Parse(matchingDateTime, CultureInfo.InvariantCulture).AsUtc()
        };

        var otherResource = new FilterableResource
        {
            SomeDateTimeInUtcZone = DateTime.Parse(nonMatchingDateTime, CultureInfo.InvariantCulture).AsUtc()
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter={filterOperator.ToString().Camelize()}(someDateTimeInUtcZone,'{filterDateTime}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);

        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someDateTimeInUtcZone")
            .With(value => value.Should().Be(resource.SomeDateTimeInUtcZone));
    }

    [Theory]
    [InlineData("The fox jumped over the lazy dog", "Other", TextMatchKind.Contains, "jumped")]
    [InlineData("The fox jumped over the lazy dog", "the fox...", TextMatchKind.Contains, "The")]
    [InlineData("The fox jumped over the lazy dog", "The fox jumped", TextMatchKind.Contains, "dog")]
    [InlineData("The fox jumped over the lazy dog", "Yesterday The fox...", TextMatchKind.StartsWith, "The")]
    [InlineData("The fox jumped over the lazy dog", "over the lazy dog earlier", TextMatchKind.EndsWith, "dog")]
    public async Task Can_filter_text_match(string matchingText, string nonMatchingText, TextMatchKind matchKind, string filterText)
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeString = matchingText
        };

        var otherResource = new FilterableResource
        {
            SomeString = nonMatchingText
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter={matchKind.ToString().Camelize()}(someString,'{filterText}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someString").With(value => value.Should().Be(resource.SomeString));
    }

    [Theory]
    [InlineData("two", "one two", "'one','two','three'")]
    [InlineData("two", "nine", "'one','two','three','four','five'")]
    public async Task Can_filter_in_set(string matchingText, string nonMatchingText, string filterText)
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeString = matchingText
        };

        var otherResource = new FilterableResource
        {
            SomeString = nonMatchingText
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter=any(someString,{filterText})";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.ShouldContainKey("someString").With(value => value.Should().Be(resource.SomeString));
    }

    [Fact]
    public async Task Cannot_filter_on_has()
    {
        // Arrange
        const string route = "/filterableResources?filter=has(children)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Relationships are not supported when using MongoDB.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_filter_on_has_with_nested_condition()
    {
        // Arrange
        const string route = "/filterableResources?filter=has(children,equals(someBoolean,'true'))";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Relationships are not supported when using MongoDB.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Cannot_filter_on_count()
    {
        // Arrange
        const string route = "/filterableResources?filter=equals(count(children),'2')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.ShouldHaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Relationships are not supported when using MongoDB.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
    }

    [Theory]
    [InlineData("and(equals(someString,'ABC'),equals(someInt32,'11'))")]
    [InlineData("and(equals(someString,'ABC'),equals(someInt32,'11'),equals(someEnum,'Tuesday'))")]
    [InlineData("or(equals(someString,'---'),lessThan(someInt32,'33'))")]
    [InlineData("not(equals(someEnum,'Saturday'))")]
    public async Task Can_filter_on_logical_functions(string filterExpression)
    {
        // Arrange
        var resource1 = new FilterableResource
        {
            SomeString = "ABC",
            SomeInt32 = 11,
            SomeEnum = DayOfWeek.Tuesday
        };

        var resource2 = new FilterableResource
        {
            SomeString = "XYZ",
            SomeInt32 = 99,
            SomeEnum = DayOfWeek.Saturday
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource1, resource2);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter={filterExpression}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.ShouldHaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(resource1.StringId);
    }
}
