using System;
using FluentAssertions;
using FluentAssertions.Numeric;
using FluentAssertions.Primitives;
using JetBrains.Annotations;

namespace JsonApiDotNetCoreMongoDbExampleTests.TestBuildingBlocks
{
    [PublicAPI]
    public static class ObjectAssertionsExtensions
    {
        private const decimal NumericPrecision = 0.00000000001M;

        /// <summary>
        /// Used to assert on a (nullable) <see cref="DateTime" /> or <see cref="DateTimeOffset" /> property, whose value is returned as <see cref="string" /> in
        /// JSON:API response body because of <see cref="IntegrationTestConfiguration.DeserializationSettings" />.
        /// </summary>
        [CustomAssertion]
        public static void BeCloseTo(this ObjectAssertions source, DateTimeOffset? expected, string because = "", params object[] becauseArgs)
        {
            if (expected == null)
            {
                source.Subject.Should().BeNull(because, becauseArgs);
            }
            else
            {
                if (!DateTimeOffset.TryParse((string)source.Subject, out DateTimeOffset value))
                {
                    source.Subject.Should().Be(expected, because, becauseArgs);
                }

                // We lose a little bit of precision (milliseconds) on roundtrip through MongoDB database.
                value.Should().BeCloseTo(expected.Value, because: because, becauseArgs: becauseArgs);
            }
        }

        /// <summary>
        /// Same as <see cref="NumericAssertionsExtensions.BeApproximately(NullableNumericAssertions{decimal}, decimal?, decimal, string, object[])" />, but with
        /// default precision.
        /// </summary>
        [CustomAssertion]
        public static AndConstraint<NullableNumericAssertions<decimal>> BeApproximately(this NullableNumericAssertions<decimal> parent, decimal? expectedValue,
            string because = "", params object[] becauseArgs)
        {
            return parent.BeApproximately(expectedValue, NumericPrecision, because, becauseArgs);
        }
    }
}
