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
    private const string IntLowerBound = "19";
    private const string IntInTheRange = "20";
    private const string IntUpperBound = "21";

    private const string DoubleLowerBound = "1.9";
    private const string DoubleInTheRange = "2.0";
    private const string DoubleUpperBound = "2.1";

    private const string IsoDateTimeLowerBound = "2000-11-22T09:48:17";
    private const string IsoDateTimeInTheRange = "2000-11-22T12:34:56";
    private const string IsoDateTimeUpperBound = "2000-11-22T18:47:32";

    private const string InvariantDateTimeLowerBound = "11/22/2000 9:48:17";
    private const string InvariantDateTimeInTheRange = "11/22/2000 12:34:56";
    private const string InvariantDateTimeUpperBound = "11/22/2000 18:47:32";

    private const string TimeSpanLowerBound = "2:15:28:54.997";
    private const string TimeSpanInTheRange = "2:15:51:42.397";
    private const string TimeSpanUpperBound = "2:16:22:41.736";

    private const string IsoDateOnlyLowerBound = "2000-10-22";
    private const string IsoDateOnlyInTheRange = "2000-11-22";
    private const string IsoDateOnlyUpperBound = "2000-12-22";

    private const string InvariantDateOnlyLowerBound = "10/22/2000";
    private const string InvariantDateOnlyInTheRange = "11/22/2000";
    private const string InvariantDateOnlyUpperBound = "12/22/2000";

    private const string TimeOnlyLowerBound = "15:28:54.997";
    private const string TimeOnlyInTheRange = "15:51:42.397";
    private const string TimeOnlyUpperBound = "16:22:41.736";

    private readonly IntegrationTestContext<TestableStartup, FilterDbContext> _testContext;

    public FilterOperatorTests(IntegrationTestContext<TestableStartup, FilterDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseResourceTypesInNamespace(typeof(FilterableResource).Namespace);

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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("someString").WhoseValue.Should().Be(resource.SomeString);

        responseDocument.Links.Should().NotBeNull();
        responseDocument.Links.Self.Should().Be("http://localhost/filterableResources?filter=equals(someString,'This%2c+that+%26+more+%2b+some')");
        responseDocument.Links.First.Should().Be("http://localhost/filterableResources?filter=equals(someString,%27This,%20that%20%26%20more%20%2B%20some%27)");
    }

    [Fact]
    public async Task Cannot_filter_equality_on_two_attributes()
    {
        // Arrange
        const string route = "/filterableResources?filter=equals(someInt32,otherInt32)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Comparing attributes against each other is not supported when using MongoDB.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
    }

    [Theory]
    [InlineData(IntLowerBound, IntUpperBound, ComparisonOperator.LessThan, IntInTheRange)]
    [InlineData(IntLowerBound, IntUpperBound, ComparisonOperator.LessThan, IntUpperBound)]
    [InlineData(IntLowerBound, IntUpperBound, ComparisonOperator.LessOrEqual, IntInTheRange)]
    [InlineData(IntLowerBound, IntUpperBound, ComparisonOperator.LessOrEqual, IntLowerBound)]
    [InlineData(IntUpperBound, IntLowerBound, ComparisonOperator.GreaterThan, IntInTheRange)]
    [InlineData(IntUpperBound, IntLowerBound, ComparisonOperator.GreaterThan, IntLowerBound)]
    [InlineData(IntUpperBound, IntLowerBound, ComparisonOperator.GreaterOrEqual, IntInTheRange)]
    [InlineData(IntUpperBound, IntLowerBound, ComparisonOperator.GreaterOrEqual, IntUpperBound)]
    public async Task Can_filter_comparison_on_whole_number(string matchingValue, string nonMatchingValue, ComparisonOperator filterOperator,
        string filterValue)
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeInt32 = int.Parse(matchingValue, CultureInfo.InvariantCulture)
        };

        var otherResource = new FilterableResource
        {
            SomeInt32 = int.Parse(nonMatchingValue, CultureInfo.InvariantCulture)
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("someInt32").WhoseValue.Should().Be(resource.SomeInt32);
    }

    [Theory]
    [InlineData(DoubleLowerBound, DoubleUpperBound, ComparisonOperator.LessThan, DoubleInTheRange)]
    [InlineData(DoubleLowerBound, DoubleUpperBound, ComparisonOperator.LessThan, DoubleUpperBound)]
    [InlineData(DoubleLowerBound, DoubleUpperBound, ComparisonOperator.LessOrEqual, DoubleInTheRange)]
    [InlineData(DoubleLowerBound, DoubleUpperBound, ComparisonOperator.LessOrEqual, DoubleLowerBound)]
    [InlineData(DoubleUpperBound, DoubleLowerBound, ComparisonOperator.GreaterThan, DoubleInTheRange)]
    [InlineData(DoubleUpperBound, DoubleLowerBound, ComparisonOperator.GreaterThan, DoubleLowerBound)]
    [InlineData(DoubleUpperBound, DoubleLowerBound, ComparisonOperator.GreaterOrEqual, DoubleInTheRange)]
    [InlineData(DoubleUpperBound, DoubleLowerBound, ComparisonOperator.GreaterOrEqual, DoubleUpperBound)]
    public async Task Can_filter_comparison_on_fractional_number(string matchingValue, string nonMatchingValue, ComparisonOperator filterOperator,
        string filterValue)
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeDouble = double.Parse(matchingValue, CultureInfo.InvariantCulture)
        };

        var otherResource = new FilterableResource
        {
            SomeDouble = double.Parse(nonMatchingValue, CultureInfo.InvariantCulture)
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("someDouble").WhoseValue.Should().Be(resource.SomeDouble);
    }

    [Theory]
    [InlineData(IsoDateTimeLowerBound, IsoDateTimeUpperBound, ComparisonOperator.LessThan, IsoDateTimeInTheRange)]
    [InlineData(IsoDateTimeLowerBound, IsoDateTimeUpperBound, ComparisonOperator.LessThan, IsoDateTimeUpperBound)]
    [InlineData(IsoDateTimeLowerBound, IsoDateTimeUpperBound, ComparisonOperator.LessOrEqual, IsoDateTimeInTheRange)]
    [InlineData(IsoDateTimeLowerBound, IsoDateTimeUpperBound, ComparisonOperator.LessOrEqual, IsoDateTimeLowerBound)]
    [InlineData(IsoDateTimeUpperBound, IsoDateTimeLowerBound, ComparisonOperator.GreaterThan, IsoDateTimeInTheRange)]
    [InlineData(IsoDateTimeUpperBound, IsoDateTimeLowerBound, ComparisonOperator.GreaterThan, IsoDateTimeLowerBound)]
    [InlineData(IsoDateTimeUpperBound, IsoDateTimeLowerBound, ComparisonOperator.GreaterOrEqual, IsoDateTimeInTheRange)]
    [InlineData(IsoDateTimeUpperBound, IsoDateTimeLowerBound, ComparisonOperator.GreaterOrEqual, IsoDateTimeUpperBound)]
    [InlineData(InvariantDateTimeLowerBound, InvariantDateTimeUpperBound, ComparisonOperator.LessThan, InvariantDateTimeInTheRange)]
    [InlineData(InvariantDateTimeLowerBound, InvariantDateTimeUpperBound, ComparisonOperator.LessThan, InvariantDateTimeUpperBound)]
    [InlineData(InvariantDateTimeLowerBound, InvariantDateTimeUpperBound, ComparisonOperator.LessOrEqual, InvariantDateTimeInTheRange)]
    [InlineData(InvariantDateTimeLowerBound, InvariantDateTimeUpperBound, ComparisonOperator.LessOrEqual, InvariantDateTimeLowerBound)]
    [InlineData(InvariantDateTimeUpperBound, InvariantDateTimeLowerBound, ComparisonOperator.GreaterThan, InvariantDateTimeInTheRange)]
    [InlineData(InvariantDateTimeUpperBound, InvariantDateTimeLowerBound, ComparisonOperator.GreaterThan, InvariantDateTimeLowerBound)]
    [InlineData(InvariantDateTimeUpperBound, InvariantDateTimeLowerBound, ComparisonOperator.GreaterOrEqual, InvariantDateTimeInTheRange)]
    [InlineData(InvariantDateTimeUpperBound, InvariantDateTimeLowerBound, ComparisonOperator.GreaterOrEqual, InvariantDateTimeUpperBound)]
    public async Task Can_filter_comparison_on_DateTime_in_UTC_time_zone(string matchingValue, string nonMatchingValue, ComparisonOperator filterOperator,
        string filterValue)
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeDateTimeInUtcZone = DateTime.Parse(matchingValue, CultureInfo.InvariantCulture).AsUtc()
        };

        var otherResource = new FilterableResource
        {
            SomeDateTimeInUtcZone = DateTime.Parse(nonMatchingValue, CultureInfo.InvariantCulture).AsUtc()
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter={filterOperator.ToString().Camelize()}(someDateTimeInUtcZone,'{filterValue}Z')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("someDateTimeInUtcZone").WhoseValue.Should().Be(resource.SomeDateTimeInUtcZone);
    }

    [Theory]
    [InlineData(IsoDateTimeLowerBound, IsoDateTimeUpperBound, ComparisonOperator.LessThan, IsoDateTimeInTheRange)]
    [InlineData(IsoDateTimeLowerBound, IsoDateTimeUpperBound, ComparisonOperator.LessThan, IsoDateTimeUpperBound)]
    [InlineData(IsoDateTimeLowerBound, IsoDateTimeUpperBound, ComparisonOperator.LessOrEqual, IsoDateTimeInTheRange)]
    [InlineData(IsoDateTimeLowerBound, IsoDateTimeUpperBound, ComparisonOperator.LessOrEqual, IsoDateTimeLowerBound)]
    [InlineData(IsoDateTimeUpperBound, IsoDateTimeLowerBound, ComparisonOperator.GreaterThan, IsoDateTimeInTheRange)]
    [InlineData(IsoDateTimeUpperBound, IsoDateTimeLowerBound, ComparisonOperator.GreaterThan, IsoDateTimeLowerBound)]
    [InlineData(IsoDateTimeUpperBound, IsoDateTimeLowerBound, ComparisonOperator.GreaterOrEqual, IsoDateTimeInTheRange)]
    [InlineData(IsoDateTimeUpperBound, IsoDateTimeLowerBound, ComparisonOperator.GreaterOrEqual, IsoDateTimeUpperBound)]
    [InlineData(InvariantDateTimeLowerBound, InvariantDateTimeUpperBound, ComparisonOperator.LessThan, InvariantDateTimeInTheRange)]
    [InlineData(InvariantDateTimeLowerBound, InvariantDateTimeUpperBound, ComparisonOperator.LessThan, InvariantDateTimeUpperBound)]
    [InlineData(InvariantDateTimeLowerBound, InvariantDateTimeUpperBound, ComparisonOperator.LessOrEqual, InvariantDateTimeInTheRange)]
    [InlineData(InvariantDateTimeLowerBound, InvariantDateTimeUpperBound, ComparisonOperator.LessOrEqual, InvariantDateTimeLowerBound)]
    [InlineData(InvariantDateTimeUpperBound, InvariantDateTimeLowerBound, ComparisonOperator.GreaterThan, InvariantDateTimeInTheRange)]
    [InlineData(InvariantDateTimeUpperBound, InvariantDateTimeLowerBound, ComparisonOperator.GreaterThan, InvariantDateTimeLowerBound)]
    [InlineData(InvariantDateTimeUpperBound, InvariantDateTimeLowerBound, ComparisonOperator.GreaterOrEqual, InvariantDateTimeInTheRange)]
    [InlineData(InvariantDateTimeUpperBound, InvariantDateTimeLowerBound, ComparisonOperator.GreaterOrEqual, InvariantDateTimeUpperBound)]
    public async Task Can_filter_comparison_on_DateTimeOffset(string matchingValue, string nonMatchingValue, ComparisonOperator filterOperator,
        string filterValue)
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeDateTimeOffset = DateTime.Parse(matchingValue, CultureInfo.InvariantCulture).AsUtc()
        };

        var otherResource = new FilterableResource
        {
            SomeDateTimeOffset = DateTime.Parse(nonMatchingValue, CultureInfo.InvariantCulture).AsUtc()
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter={filterOperator.ToString().Camelize()}(someDateTimeOffset,'{filterValue}Z')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);

        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("someDateTimeOffset").WhoseValue.Should().Be(resource.SomeDateTimeOffset);
    }

    [Theory]
    [InlineData(TimeSpanLowerBound, TimeSpanUpperBound, ComparisonOperator.LessThan, TimeSpanInTheRange)]
    [InlineData(TimeSpanLowerBound, TimeSpanUpperBound, ComparisonOperator.LessThan, TimeSpanUpperBound)]
    [InlineData(TimeSpanLowerBound, TimeSpanUpperBound, ComparisonOperator.LessOrEqual, TimeSpanInTheRange)]
    [InlineData(TimeSpanLowerBound, TimeSpanUpperBound, ComparisonOperator.LessOrEqual, TimeSpanLowerBound)]
    [InlineData(TimeSpanUpperBound, TimeSpanLowerBound, ComparisonOperator.GreaterThan, TimeSpanInTheRange)]
    [InlineData(TimeSpanUpperBound, TimeSpanLowerBound, ComparisonOperator.GreaterThan, TimeSpanLowerBound)]
    [InlineData(TimeSpanUpperBound, TimeSpanLowerBound, ComparisonOperator.GreaterOrEqual, TimeSpanInTheRange)]
    [InlineData(TimeSpanUpperBound, TimeSpanLowerBound, ComparisonOperator.GreaterOrEqual, TimeSpanUpperBound)]
    public async Task Can_filter_comparison_on_TimeSpan(string matchingValue, string nonMatchingValue, ComparisonOperator filterOperator, string filterValue)
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeTimeSpan = TimeSpan.Parse(matchingValue, CultureInfo.InvariantCulture)
        };

        var otherResource = new FilterableResource
        {
            SomeTimeSpan = TimeSpan.Parse(nonMatchingValue, CultureInfo.InvariantCulture)
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter={filterOperator.ToString().Camelize()}(someTimeSpan,'{filterValue}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("someTimeSpan").WhoseValue.Should().Be(resource.SomeTimeSpan);
    }

    [Theory]
    [InlineData(IsoDateOnlyLowerBound, IsoDateOnlyUpperBound, ComparisonOperator.LessThan, IsoDateOnlyInTheRange)]
    [InlineData(IsoDateOnlyLowerBound, IsoDateOnlyUpperBound, ComparisonOperator.LessThan, IsoDateOnlyUpperBound)]
    [InlineData(IsoDateOnlyLowerBound, IsoDateOnlyUpperBound, ComparisonOperator.LessOrEqual, IsoDateOnlyInTheRange)]
    [InlineData(IsoDateOnlyLowerBound, IsoDateOnlyUpperBound, ComparisonOperator.LessOrEqual, IsoDateOnlyLowerBound)]
    [InlineData(IsoDateOnlyUpperBound, IsoDateOnlyLowerBound, ComparisonOperator.GreaterThan, IsoDateOnlyInTheRange)]
    [InlineData(IsoDateOnlyUpperBound, IsoDateOnlyLowerBound, ComparisonOperator.GreaterThan, IsoDateOnlyLowerBound)]
    [InlineData(IsoDateOnlyUpperBound, IsoDateOnlyLowerBound, ComparisonOperator.GreaterOrEqual, IsoDateOnlyInTheRange)]
    [InlineData(IsoDateOnlyUpperBound, IsoDateOnlyLowerBound, ComparisonOperator.GreaterOrEqual, IsoDateOnlyUpperBound)]
    [InlineData(InvariantDateOnlyLowerBound, InvariantDateOnlyUpperBound, ComparisonOperator.LessThan, InvariantDateOnlyInTheRange)]
    [InlineData(InvariantDateOnlyLowerBound, InvariantDateOnlyUpperBound, ComparisonOperator.LessThan, InvariantDateOnlyUpperBound)]
    [InlineData(InvariantDateOnlyLowerBound, InvariantDateOnlyUpperBound, ComparisonOperator.LessOrEqual, InvariantDateOnlyInTheRange)]
    [InlineData(InvariantDateOnlyLowerBound, InvariantDateOnlyUpperBound, ComparisonOperator.LessOrEqual, InvariantDateOnlyLowerBound)]
    [InlineData(InvariantDateOnlyUpperBound, InvariantDateOnlyLowerBound, ComparisonOperator.GreaterThan, InvariantDateOnlyInTheRange)]
    [InlineData(InvariantDateOnlyUpperBound, InvariantDateOnlyLowerBound, ComparisonOperator.GreaterThan, InvariantDateOnlyLowerBound)]
    [InlineData(InvariantDateOnlyUpperBound, InvariantDateOnlyLowerBound, ComparisonOperator.GreaterOrEqual, InvariantDateOnlyInTheRange)]
    [InlineData(InvariantDateOnlyUpperBound, InvariantDateOnlyLowerBound, ComparisonOperator.GreaterOrEqual, InvariantDateOnlyUpperBound)]
    public async Task Can_filter_comparison_on_DateOnly(string matchingValue, string nonMatchingValue, ComparisonOperator filterOperator, string filterValue)
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeDateOnly = DateOnly.Parse(matchingValue, CultureInfo.InvariantCulture)
        };

        var otherResource = new FilterableResource
        {
            SomeDateOnly = DateOnly.Parse(nonMatchingValue, CultureInfo.InvariantCulture)
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter={filterOperator.ToString().Camelize()}(someDateOnly,'{filterValue}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("someDateOnly").WhoseValue.Should().Be(resource.SomeDateOnly);
    }

    [Theory]
    [InlineData(TimeOnlyLowerBound, TimeOnlyUpperBound, ComparisonOperator.LessThan, TimeOnlyInTheRange)]
    [InlineData(TimeOnlyLowerBound, TimeOnlyUpperBound, ComparisonOperator.LessThan, TimeOnlyUpperBound)]
    [InlineData(TimeOnlyLowerBound, TimeOnlyUpperBound, ComparisonOperator.LessOrEqual, TimeOnlyInTheRange)]
    [InlineData(TimeOnlyLowerBound, TimeOnlyUpperBound, ComparisonOperator.LessOrEqual, TimeOnlyLowerBound)]
    [InlineData(TimeOnlyUpperBound, TimeOnlyLowerBound, ComparisonOperator.GreaterThan, TimeOnlyInTheRange)]
    [InlineData(TimeOnlyUpperBound, TimeOnlyLowerBound, ComparisonOperator.GreaterThan, TimeOnlyLowerBound)]
    [InlineData(TimeOnlyUpperBound, TimeOnlyLowerBound, ComparisonOperator.GreaterOrEqual, TimeOnlyInTheRange)]
    [InlineData(TimeOnlyUpperBound, TimeOnlyLowerBound, ComparisonOperator.GreaterOrEqual, TimeOnlyUpperBound)]
    public async Task Can_filter_comparison_on_TimeOnly(string matchingValue, string nonMatchingValue, ComparisonOperator filterOperator, string filterValue)
    {
        // Arrange
        var resource = new FilterableResource
        {
            SomeTimeOnly = TimeOnly.Parse(matchingValue, CultureInfo.InvariantCulture)
        };

        var otherResource = new FilterableResource
        {
            SomeTimeOnly = TimeOnly.Parse(nonMatchingValue, CultureInfo.InvariantCulture)
        };

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<FilterableResource>();
            dbContext.FilterableResources.AddRange(resource, otherResource);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/filterableResources?filter={filterOperator.ToString().Camelize()}(someTimeOnly,'{filterValue}')";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("someTimeOnly").WhoseValue.Should().Be(resource.SomeTimeOnly);
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("someString").WhoseValue.Should().Be(resource.SomeString);
    }

    [Theory]
    [InlineData("yes", "no", "'yes'")]
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Attributes.Should().ContainKey("someString").WhoseValue.Should().Be(resource.SomeString);
    }

    [Fact]
    public async Task Cannot_filter_on_has()
    {
        // Arrange
        const string route = "/filterableResources?filter=has(children)";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(resource1.StringId);
    }
}
