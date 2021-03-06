﻿using System;
using System.Threading.Tasks;
using E247.Fun;
using E247.Fun.Exceptions;
using Ploeh.AutoFixture.Xunit2;
using Xunit;
using Xunit.Extensions;

namespace E247.Fun.UnitTest
{
    public class MaybeTests
    {
        [Fact]
        public void EmptyMaybeHasNoValue()
        {
            var actual = Maybe<string>.Empty();

            Assert.False(actual.HasValue);
            Assert.False(actual.Any());
            Assert.Throws<EmptyMaybeException>(() => actual.Value);
        }

        [Fact]
        public void MaybeWithNullHasNoValue()
        {
            var actual = new Maybe<string>(null);

            Assert.False(actual.HasValue);
            Assert.False(actual.Any());
            Assert.Throws<EmptyMaybeException>(() => actual.Value);
        }

        [Theory, AutoData]
        public void MaybeWithValueHasValue(string input)
        {
            var actual = new Maybe<string>(input);

            Assert.True(actual.HasValue);
            Assert.True(actual.Any());
            Assert.Equal(input, actual.Value);
        }

        [Theory, AutoData]
        public void CanImplicitlyReturnMaybe(string input)
        {
            Func<Maybe<string>> test = () => input;

            Assert.IsType<Maybe<string>>(test());
        }

        [Theory, AutoData]
        public void ToMaybeReturnsMaybe(string input)
        {
            var actual = input.ToMaybe();

            Assert.IsType<Maybe<string>>(actual);
        }

        [Theory, AutoData]
        public void MatchCallsSomeForMaybeWithValue(string input)
        {
            var maybeInput = input.ToMaybe();

            var result = maybeInput.Match<string, int>(
                Some: _ => 1,
                None: () => 0);

            Assert.Equal(1, result);
        }

        [Fact]
        public void MatchCallsNoneForEmptyMaybe()
        {
            var maybe = Maybe<string>.Empty();

            var result = maybe.Match(
                Some: _ => 1,
                None: () => 0);

            Assert.Equal(0, result);
        }
        [Theory, AutoData]
        public async Task MatchAsyncCallsSomeForMaybeWithValue(string input)
        {
            var maybeInput = input.ToMaybe();

            var result = await maybeInput.MatchAsync(
                Some: _ => Task.FromResult(1),
                None: () => Task.FromResult(0));

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task MatchAsyncCallsNoneForEmptyMaybe()
        {
            var maybe = Maybe<string>.Empty();

            var result = await maybe.MatchAsync(
                Some: _ => Task.FromResult(1),
                None: () => Task.FromResult(0));

            Assert.Equal(0, result);
        }

        [Theory, AutoData]
        public async Task MatchAsyncWithSynchronousNoneCallsSomeForMaybeWithValue(
            string input)
        {
            var maybeInput = input.ToMaybe();

            var result = await maybeInput.MatchAsync(
                Some: _ => Task.FromResult(1),
                None: () => 0);

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task MatchAsyncWithSynchronousNoneCallsNoneForEmptyMaybe()
        {
            var maybe = Maybe<string>.Empty();

            var result = await maybe.MatchAsync(
                Some: _ => Task.FromResult(1),
                None: () => 0);

            Assert.Equal(0, result);
        }

        [Theory, AutoData]
        public void MatchCallsActionSomeForMaybeWithValue(
            string input,
            string noneSideEffect)
        {
            var maybeInput = input.ToMaybe();
            var target = "42";

            var result = maybeInput.Match(
                Some: value => { target = value; },
                None: () => { target = noneSideEffect; });

            Assert.Equal(input, target);
        }


        [Theory, AutoData]
        public void MatchCallsActionNoneForMaybeWithoutValue(
            string input,
            string noneSideEffect)
        {
            var maybeInput = Maybe<string>.Empty();
            var target = "42";

            var result = maybeInput.Match(
                Some: value => { target = value; },
                None: () => { target = noneSideEffect; });

            Assert.Equal(noneSideEffect, target);
        }

        [Theory, AutoData]
        public async Task MatchAsyncCallsActionSomeForMaybeWithValue(
            string input,
            string noneSideEffect)
        {
            var maybeInput = input.ToMaybe();
            var target = "42";

            var result = await maybeInput.MatchAsync(
                Some: async value => { target = await Task.FromResult(value); },
                None: async () => { target = await Task.FromResult(noneSideEffect); });

            Assert.Equal(input, target);
        }


        [Theory, AutoData]
        public async Task MatchAsyncCallsActionNoneForMaybeWithoutValue(
            string input,
            string noneSideEffect)
        {
            var maybeInput = Maybe<string>.Empty();
            var target = "42";

            var result = await maybeInput.MatchAsync(
                Some: async value => { target = await Task.FromResult(value); },
                None: async () => { target = await Task.FromResult(noneSideEffect); });

            Assert.Equal(noneSideEffect, target);
        }

        [Theory, AutoData]
        public void MaybeWithValueIsEqualToValue(string input)
        {
            var maybe = input.ToMaybe();

            Assert.Equal(input, maybe);
        }

        [Theory, AutoData]
        public void EmptyMaybeIsNotEqualToValue(string input)
        {
            var maybe = Maybe<string>.Empty();

            Assert.NotEqual(input, maybe);
        }

        [Theory, AutoData]
        public void MaybeWithValueIsNotEqualToDifferentValue(
            string input,
            string other)
        {
            var maybe = input.ToMaybe();

            Assert.NotEqual(input, other);
            Assert.NotEqual(other, maybe);
        }

        [Theory, AutoData]
        public void MapMapsAFunction(string input)
        {
            var maybe = input.ToMaybe();

            var actual = maybe
                .Map(v => v.Length);

            Assert.Equal(input.Length, actual.Value);
        }

        [Fact]
        public void MapMapsEmptyToEmpty()
        {
            var maybe = Maybe<string>.Empty();

            var actual = maybe
                .Map(v => v.Length);

            Assert.False(actual.HasValue);
        }

        [Theory, AutoData]
        public async Task MapAsyncMapsAFunction(string input)
        {
            var maybe = input.ToMaybe();

            var actual = await maybe
                .MapAsync(v => Task.FromResult(v.Length));

            Assert.Equal(input.Length, actual.Value);
        }

        [Fact]
        public async Task MapAsyncMapsEmptyToEmpty()
        {
            var maybe = Maybe<string>.Empty();

            var actual = await maybe
                .MapAsync(v => Task.FromResult(v.Length));

            Assert.False(actual.HasValue);
        }

        [Theory, AutoData]
        public void TeeMapWithNonEmptyMaybe(object input)
        {
            var actionCalled = false;
            Action<object> someAdhocAction = (object str) => {
                actionCalled = true;
            };

            input
                .ToMaybe()
                .TeeMap(someAdhocAction);

            Assert.True(actionCalled);
        }

        [Fact]
        public void TeeMapWithEmptyMaybe()
        {
            var actionCalled = false;
            Action<object> someAdhocAction = (object str) => {
                actionCalled = true;
            };

            object nullValue = null;

            nullValue.ToMaybe()
                .TeeMap(someAdhocAction);

            Assert.False(actionCalled);
        }

        [Theory, AutoData]
        public void TeeMapDoesNotChangeMaybe(object input)
        {
            var maybeValue = input.ToMaybe();
            Action<object> potentiallyHarmfullAction = (object param) => {
                param = new object();
            };

            var actual = maybeValue.TeeMap(potentiallyHarmfullAction);

            Assert.Equal(maybeValue, actual);
            Assert.Equal(maybeValue.Value, actual.Value);
        }

        [Theory, AutoData]
        public void SelectManyLetsUsUseWeirdSyntax(string input1, string input2)
        {
            var actual = from a in input1.ToMaybe()
                         from b in input2.ToMaybe()
                         select $"Actual values: {a} {b}";

            Assert.True(actual.HasValue);
            Assert.Contains(input1, actual.Value);
            Assert.Contains(input2, actual.Value);
        }

        [Theory, AutoData]
        public void SelectManyLetsUsUseWeirdSyntaxWithEmptyMaybes(
            string input1, 
            string input2)
        {
            var actual = from a in input1.ToMaybe()
                         from b in input2.ToMaybe()
                         from c in Maybe<string>.Empty()
                         select $"Actual values: {a} {b} {c}";

            Assert.False(actual.HasValue);
        }
    }
}
