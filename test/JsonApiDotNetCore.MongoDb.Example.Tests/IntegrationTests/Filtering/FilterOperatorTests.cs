using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using FluentAssertions;
using Humanizer;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;

namespace JsonApiDotNetCore.MongoDb.Example.Tests.IntegrationTests.Filtering
{
    public sealed class FilterOperatorTests : IClassFixture<IntegrationTestContext<TestableStartup>>
    {
        private readonly IntegrationTestContext<TestableStartup> _testContext;

        public FilterOperatorTests(IntegrationTestContext<TestableStartup> testContext)
        {
            _testContext = testContext;
            
            _testContext.RegisterResources(builder =>
            {
                builder.Add<FilterableResource, string>();
            });
            
            _testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddResourceRepository<MongoEntityRepository<FilterableResource, string>>();
            });

            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.EnableLegacyFilterNotation = false;
            options.SerializerSettings.DateFormatString = "yyyy-MM-dd";
        }

        [Fact]
        public async Task Can_filter_equality_on_special_characters()
        {
            // Arrange
            var resource = new FilterableResource
            {
                SomeString = "This, that & more"
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var collection = db.GetCollection<FilterableResource>(nameof(FilterableResource));
                await collection.DeleteManyAsync(Builders<FilterableResource>.Filter.Empty);
                await collection.InsertManyAsync(new[] {resource, new FilterableResource()});
            });

            var route = $"/filterableResources?filter=equals(someString,'{HttpUtility.UrlEncode(resource.SomeString)}')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes["someString"].Should().Be(resource.SomeString);
        }

        // [Fact]
        // public async Task Can_filter_equality_on_two_attributes_of_same_type()
        // {
        //     // Arrange
        //     var resource = new FilterableResource
        //     {
        //         SomeInt32 = 5,
        //         OtherInt32 = 5
        //     };
        //
        //     var otherResource = new FilterableResource
        //     {
        //         SomeInt32 = 5,
        //         OtherInt32 = 10
        //     };
        //
        //     await _testContext.RunOnDatabaseAsync(async db =>
        //     {
        //         var collection = db.GetCollection<FilterableResource>(nameof(FilterableResource));
        //         await collection.DeleteManyAsync(Builders<FilterableResource>.Filter.Empty);
        //         await collection.InsertManyAsync(new[] {resource, otherResource});
        //     });
        //
        //     var route = "/filterableResources?filter=equals(someInt32,otherInt32)";
        //
        //     // Act
        //     var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);
        //
        //     // Assert
        //     httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        //
        //     responseDocument.ManyData.Should().HaveCount(1);
        //     responseDocument.ManyData[0].Attributes["someInt32"].Should().Be(resource.SomeInt32);
        //     responseDocument.ManyData[0].Attributes["otherInt32"].Should().Be(resource.OtherInt32);
        // }

        // [Fact]
        // public async Task Can_filter_equality_on_two_attributes_of_same_nullable_type()
        // {
        //     // Arrange
        //     var resource = new FilterableResource
        //     {
        //         SomeNullableInt32 = 5,
        //         OtherNullableInt32 = 5
        //     };
        //
        //     var otherResource = new FilterableResource
        //     {
        //         SomeNullableInt32 = 5,
        //         OtherNullableInt32 = 10
        //     };
        //
        //     await _testContext.RunOnDatabaseAsync(async db =>
        //     {
        //         var collection = db.GetCollection<FilterableResource>(nameof(FilterableResource));
        //         await collection.DeleteManyAsync(Builders<FilterableResource>.Filter.Empty);
        //         await collection.InsertManyAsync(new[] {resource, otherResource});
        //     });
        //
        //     var route = "/filterableResources?filter=equals(someNullableInt32,otherNullableInt32)";
        //
        //     // Act
        //     var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);
        //
        //     // Assert
        //     httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        //
        //     responseDocument.ManyData.Should().HaveCount(1);
        //     responseDocument.ManyData[0].Attributes["someNullableInt32"].Should().Be(resource.SomeNullableInt32);
        //     responseDocument.ManyData[0].Attributes["otherNullableInt32"].Should().Be(resource.OtherNullableInt32);
        // }

        // [Fact]
        // public async Task Can_filter_equality_on_two_attributes_with_nullable_at_start()
        // {
        //     // Arrange
        //     var resource = new FilterableResource
        //     {
        //         SomeInt32 = 5,
        //         SomeNullableInt32 = 5
        //     };
        //
        //     var otherResource = new FilterableResource
        //     {
        //         SomeInt32 = 5,
        //         SomeNullableInt32 = 10
        //     };
        //
        //     await _testContext.RunOnDatabaseAsync(async db =>
        //     {
        //         var collection = db.GetCollection<FilterableResource>(nameof(FilterableResource));
        //         await collection.DeleteManyAsync(Builders<FilterableResource>.Filter.Empty);
        //         await collection.InsertManyAsync(new[] {resource, otherResource});
        //     });
        //
        //     var route = "/filterableResources?filter=equals(someNullableInt32,someInt32)";
        //
        //     // Act
        //     var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);
        //
        //     // Assert
        //     httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        //
        //     responseDocument.ManyData.Should().HaveCount(1);
        //     responseDocument.ManyData[0].Attributes["someInt32"].Should().Be(resource.SomeInt32);
        //     responseDocument.ManyData[0].Attributes["someNullableInt32"].Should().Be(resource.SomeNullableInt32);
        // }

        // [Fact]
        // public async Task Can_filter_equality_on_two_attributes_with_nullable_at_end()
        // {
        //     // Arrange
        //     var resource = new FilterableResource
        //     {
        //         SomeInt32 = 5,
        //         SomeNullableInt32 = 5
        //     };
        //
        //     var otherResource = new FilterableResource
        //     {
        //         SomeInt32 = 5,
        //         SomeNullableInt32 = 10
        //     };
        //
        //     await _testContext.RunOnDatabaseAsync(async db =>
        //     {
        //         var collection = db.GetCollection<FilterableResource>(nameof(FilterableResource));
        //         await collection.DeleteManyAsync(Builders<FilterableResource>.Filter.Empty);
        //         await collection.InsertManyAsync(new[] {resource, otherResource});
        //     });
        //
        //     var route = "/filterableResources?filter=equals(someInt32,someNullableInt32)";
        //
        //     // Act
        //     var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);
        //
        //     // Assert
        //     httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        //
        //     responseDocument.ManyData.Should().HaveCount(1);
        //     responseDocument.ManyData[0].Attributes["someInt32"].Should().Be(resource.SomeInt32);
        //     responseDocument.ManyData[0].Attributes["someNullableInt32"].Should().Be(resource.SomeNullableInt32);
        // }

        // [Fact]
        // public async Task Can_filter_equality_on_two_attributes_of_compatible_types()
        // {
        //     // Arrange
        //     var resource = new FilterableResource
        //     {
        //         SomeInt32 = 5,
        //         SomeUnsignedInt64 = 5
        //     };
        //
        //     var otherResource = new FilterableResource
        //     {
        //         SomeInt32 = 5,
        //         SomeUnsignedInt64 = 10
        //     };
        //
        //     await _testContext.RunOnDatabaseAsync(async db =>
        //     {
        //         var collection = db.GetCollection<FilterableResource>(nameof(FilterableResource));
        //         await collection.DeleteManyAsync(Builders<FilterableResource>.Filter.Empty);
        //         await collection.InsertManyAsync(new[] {resource, otherResource});
        //     });
        //
        //     var route = "/filterableResources?filter=equals(someInt32,someUnsignedInt64)";
        //
        //     // Act
        //     var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);
        //
        //     // Assert
        //     httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        //
        //     responseDocument.ManyData.Should().HaveCount(1);
        //     responseDocument.ManyData[0].Attributes["someInt32"].Should().Be(resource.SomeInt32);
        //     responseDocument.ManyData[0].Attributes["someUnsignedInt64"].Should().Be(resource.SomeUnsignedInt64);
        // }

        [Fact]
        public async Task Cannot_filter_equality_on_two_attributes_of_incompatible_types()
        {
            // Arrange
            var route = "/filterableResources?filter=equals(someDouble,someTimeSpan)";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Query creation failed due to incompatible types.");
            responseDocument.Errors[0].Detail.Should().Be("No coercion operator is defined between types 'System.TimeSpan' and 'System.Double'.");
            responseDocument.Errors[0].Source.Parameter.Should().BeNull();
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

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var collection = db.GetCollection<FilterableResource>(nameof(FilterableResource));
                await collection.DeleteManyAsync(Builders<FilterableResource>.Filter.Empty);
                await collection.InsertManyAsync(new[] {resource, otherResource});
            });

            var route = $"/filterableResources?filter={filterOperator.ToString().Camelize()}(someInt32,'{filterValue}')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes["someInt32"].Should().Be(resource.SomeInt32);
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
        public async Task Can_filter_comparison_on_fractional_number(double matchingValue, double nonMatchingValue, ComparisonOperator filterOperator, double filterValue)
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

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var collection = db.GetCollection<FilterableResource>(nameof(FilterableResource));
                await collection.DeleteManyAsync(Builders<FilterableResource>.Filter.Empty);
                await collection.InsertManyAsync(new[] {resource, otherResource});
            });

            var route = $"/filterableResources?filter={filterOperator.ToString().Camelize()}(someDouble,'{filterValue}')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes["someDouble"].Should().Be(resource.SomeDouble);
        }

        [Theory]
        [InlineData("2001-01-01", "2001-01-09", ComparisonOperator.LessThan, "2001-01-05")]
        [InlineData("2001-01-01", "2001-01-09", ComparisonOperator.LessThan, "2001-01-09")]
        [InlineData("2001-01-01", "2001-01-09", ComparisonOperator.LessOrEqual, "2001-01-05")]
        [InlineData("2001-01-01", "2001-01-09", ComparisonOperator.LessOrEqual, "2001-01-01")]
        [InlineData("2001-01-09", "2001-01-01", ComparisonOperator.GreaterThan, "2001-01-05")]
        [InlineData("2001-01-09", "2001-01-01", ComparisonOperator.GreaterThan, "2001-01-01")]
        [InlineData("2001-01-09", "2001-01-01", ComparisonOperator.GreaterOrEqual, "2001-01-05")]
        [InlineData("2001-01-09", "2001-01-01", ComparisonOperator.GreaterOrEqual, "2001-01-09")]
        public async Task Can_filter_comparison_on_DateTime(string matchingDateTime, string nonMatchingDateTime, ComparisonOperator filterOperator, string filterDateTime)
        {
            // Arrange
            var resource = new FilterableResource
            {
                SomeDateTime = DateTime.ParseExact(matchingDateTime, "yyyy-MM-dd", null)
            };

            var otherResource = new FilterableResource
            {
                SomeDateTime = DateTime.ParseExact(nonMatchingDateTime, "yyyy-MM-dd", null)
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var collection = db.GetCollection<FilterableResource>(nameof(FilterableResource));
                await collection.DeleteManyAsync(Builders<FilterableResource>.Filter.Empty);
                await collection.InsertManyAsync(new[] {resource, otherResource});
            });

            var route = $"/filterableResources?filter={filterOperator.ToString().Camelize()}(someDateTime,'{DateTime.ParseExact(filterDateTime, "yyyy-MM-dd", null)}')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes["someDateTime"].Should().Be(resource.SomeDateTime.ToString("yyyy-MM-dd"));
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

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var collection = db.GetCollection<FilterableResource>(nameof(FilterableResource));
                await collection.DeleteManyAsync(Builders<FilterableResource>.Filter.Empty);
                await collection.InsertManyAsync(new[] {resource, otherResource});
            });

            var route = $"/filterableResources?filter={matchKind.ToString().Camelize()}(someString,'{filterText}')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes["someString"].Should().Be(resource.SomeString);
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

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var collection = db.GetCollection<FilterableResource>(nameof(FilterableResource));
                await collection.DeleteManyAsync(Builders<FilterableResource>.Filter.Empty);
                await collection.InsertManyAsync(new[] {resource, otherResource});
            });

            var route = $"/filterableResources?filter=any(someString,{filterText})";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Attributes["someString"].Should().Be(resource.SomeString);
        }

        [Fact]
        public async Task Can_filter_on_has()
        {
            // Arrange
            var resource = new FilterableResource
            {
                Children = new List<FilterableResource>
                {
                    new FilterableResource()
                }
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var collection = db.GetCollection<FilterableResource>(nameof(FilterableResource));
                await collection.DeleteManyAsync(Builders<FilterableResource>.Filter.Empty);
                await collection.InsertManyAsync(new[] {resource, new FilterableResource()});
            });

            var route = "/filterableResources?filter=has(children)";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(resource.StringId);
        }

        [Fact]
        public async Task Can_filter_on_count()
        {
            // Arrange
            var resource = new FilterableResource
            {
                Children = new List<FilterableResource>
                {
                    new FilterableResource(),
                    new FilterableResource()
                }
            };

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var collection = db.GetCollection<FilterableResource>(nameof(FilterableResource));
                await collection.DeleteManyAsync(Builders<FilterableResource>.Filter.Empty);
                await collection.InsertManyAsync(new[] {resource, new FilterableResource()});
            });

            var route = "/filterableResources?filter=equals(count(children),'2')";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(resource.StringId);
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

            await _testContext.RunOnDatabaseAsync(async db =>
            {
                var collection = db.GetCollection<FilterableResource>(nameof(FilterableResource));
                await collection.DeleteManyAsync(Builders<FilterableResource>.Filter.Empty);
                await collection.InsertManyAsync(new[] {resource1, resource2});
            });

            var route = $"/filterableResources?filter={filterExpression}";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(resource1.StringId);
        }
    }
}
