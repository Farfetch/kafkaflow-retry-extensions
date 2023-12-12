﻿using System;
using FluentAssertions;
using KafkaFlow.Retry.Durable.Definitions.Polling;
using Xunit;

namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Definitions.Polling;

public class CleanupPollingDefinitionTests
{
    [Theory]
    [InlineData("x", 30, 300, typeof(ArgumentException))]
    [InlineData("0 0/1 * 1/1 * ? *", 0, 300, typeof(ArgumentOutOfRangeException))]
    [InlineData("0 0/1 * 1/1 * ? *", 30, -10, typeof(ArgumentOutOfRangeException))]
    [InlineData("0 0/1 * 1/1 * ? *", 30, 0, typeof(ArgumentOutOfRangeException))]
    public void CleanupPollingDefinition_Ctor_Enabled_ThrowsExpectedException(
        string cronExpression,
        int timeToLiveInDays,
        int rowsPerRequest,
        Type expectedExceptionType)
    {
        // Act
        Action act = () => new CleanupPollingDefinition(
            enabled: true,
            cronExpression,
            timeToLiveInDays,
            rowsPerRequest);

        // Assert
        Assert.Throws(expectedExceptionType, act);
    }

    [Theory]
    [InlineData("x", 30, 300)]
    [InlineData("0 0/1 * 1/1 * ? *", 0, 300)]
    [InlineData("0 0/1 * 1/1 * ? *", 30, -10)]
    [InlineData("0 0/1 * 1/1 * ? *", 30, 0)]
    public void CleanupPollingDefinition_Ctor_NotEnabled_DoesNotThrowsExceptionWithInvalidParams(
        string cronExpression,
        int timeToLiveInDays,
        int rowsPerRequest)
    {
        // Act
        var actualPollingDefinition = new CleanupPollingDefinition(
            enabled: false,
            cronExpression,
            timeToLiveInDays,
            rowsPerRequest);

        // Assert
        actualPollingDefinition.Should().NotBeNull();
    }
}