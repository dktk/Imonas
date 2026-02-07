// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Text;

using FluentAssertions;
using FluentAssertions.Primitives;

using SG.Common;

namespace Application.IntegrationTests
{
    public static class AssertionExtensions
    {
        public static AndConstraint<BooleanAssertions> ShouldBeTrue<T>(this bool assertion, Result<T> result)
        {
            var errorMessage = result.Message;

            if (result?.Errors?.Any() ?? false)
            {
                var builder = new StringBuilder();
                result.Errors.ToList().ForEach(e => builder.AppendLine(e));

                errorMessage = builder.ToString();
            }

            return result.Success.Should().BeTrue(errorMessage);
        }

        public static AndConstraint<BooleanAssertions> ShouldBeTrue<T>(this Result<T> actualValue)
        {
            var errorMessage = actualValue.Message;

            if (actualValue?.Errors?.Any() ?? false)
            {
                var builder = new StringBuilder();
                actualValue.Errors.ToList().ForEach(e => builder.AppendLine(e));

                errorMessage = builder.ToString();
            }

            return actualValue.Success.Should().BeTrue(errorMessage);
        }
    }
}
