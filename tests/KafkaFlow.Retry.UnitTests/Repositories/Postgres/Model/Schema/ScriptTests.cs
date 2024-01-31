using System;
using KafkaFlow.Retry.Postgres.Model.Schema;

namespace KafkaFlow.Retry.UnitTests.Repositories.Postgres.Model.Schema;

public class ScriptTests
{
    [Fact]
    public void Script_Ctor_Success()
    {
        // Arrange
        var query = "test";

        // Act
        var script = new Script(query);

        // Assert
        script.Should().NotBeNull();
        script.Value.Should().Be(query);
    }

    [Fact]
    public void Script_Ctor_WithoutValue_ThrowsException()
    {
        // Arrange
        string query = null;

        // Act
        Action act = () => new Script(query);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}