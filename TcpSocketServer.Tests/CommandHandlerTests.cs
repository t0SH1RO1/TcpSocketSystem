using Microsoft.Extensions.Logging;
using Moq;
using TcpSocketServer.Services;
using Xunit;

namespace TcpSocketServer.Tests;

public class CommandHandlerTests
{
    private readonly Mock<ILogger<CommandHandler>> _loggerMock;
    private readonly Mock<ICarApiService> _carApiServiceMock;
    private readonly CommandHandler _commandHandler;

    public CommandHandlerTests()
    {
        _loggerMock = new Mock<ILogger<CommandHandler>>();
        _carApiServiceMock = new Mock<ICarApiService>();
        _commandHandler = new CommandHandler(_loggerMock.Object, _carApiServiceMock.Object);
    }

    [Fact]
    public async Task HandleCommandAsync_EmptyCommand_ReturnsError()
    {
        // Arrange
        string command = "";

        // Act
        var result = await _commandHandler.HandleCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal("ERROR EmptyCommand", result.Response);
        Assert.False(result.IsLogout);
    }

    [Fact]
    public async Task HandleCommandAsync_UnknownCommand_ReturnsError()
    {
        // Arrange
        string command = "UNKNOWN";

        // Act
        var result = await _commandHandler.HandleCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.StartsWith("ERROR UnknownCommand", result.Response);
        Assert.False(result.IsLogout);
    }

    [Fact]
    public async Task HandleCommandAsync_LogoutCommand_ReturnsGoodbyeAndSetsFlag()
    {
        // Arrange
        string command = "LOGOUT";

        // Act
        var result = await _commandHandler.HandleCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal("OK Goodbye", result.Response);
        Assert.True(result.IsLogout);
    }

    [Fact]
    public async Task HandleCommandAsync_PingCommand_ReturnsPong()
    {
        // Arrange
        string command = "PING";

        // Act
        var result = await _commandHandler.HandleCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal("PONG", result.Response);
        Assert.False(result.IsLogout);
    }

    [Fact]
    public async Task HandleCommandAsync_CarCommandWithoutBrand_ReturnsError()
    {
        // Arrange
        string command = "CAR";

        // Act
        var result = await _commandHandler.HandleCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal("ERROR MissingBrand", result.Response);
        Assert.False(result.IsLogout);
    }

    [Fact]
    public async Task HandleCommandAsync_CarCommandWithBrand_CallsApiAndReturnsInfo()
    {
        // Arrange
        string command = "CAR BMW";
        string expected = "CAR INFO: BMW - Founded: 1916, Country: Germany";

        _carApiServiceMock
            .Setup(s => s.GetCarInfoAsync("BMW", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _commandHandler.HandleCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(expected, result.Response);
        Assert.False(result.IsLogout);
        _carApiServiceMock.Verify(s => s.GetCarInfoAsync("BMW", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleCommandAsync_CarCommandWithMultipleArgs_UsesAllTextAfterCommandAsBrand()
    {
        // Arrange
        string command = "CAR BMW Additional Arguments";
        string expected = "CAR INFO: BMW Additional Arguments - Founded: 1916, Country: Germany";

        _carApiServiceMock
            .Setup(s => s.GetCarInfoAsync("BMW Additional Arguments", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _commandHandler.HandleCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(expected, result.Response);
        Assert.False(result.IsLogout);
        _carApiServiceMock.Verify(s => s.GetCarInfoAsync("BMW Additional Arguments", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleCommandAsync_CaseInsensitiveCommands_WorksCorrectly()
    {
        // Arrange
        string command = "car BMW";
        string expected = "CAR INFO: BMW - Founded: 1916, Country: Germany";

        _carApiServiceMock
            .Setup(s => s.GetCarInfoAsync("BMW", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _commandHandler.HandleCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(expected, result.Response);
        Assert.False(result.IsLogout);
    }

    [Fact]
    public async Task HandleCommandAsync_CommandWithExtraSpaces_ParsedCorrectly()
    {
        // Arrange
        string command = "  CAR   BMW  ";
        string expected = "CAR INFO: BMW - Founded: 1916, Country: Germany";

        _carApiServiceMock
            .Setup(s => s.GetCarInfoAsync("BMW", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _commandHandler.HandleCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(expected, result.Response);
        Assert.False(result.IsLogout);
    }

    [Fact]
    public async Task HandleCommandAsync_ExceptionInProcessing_ReturnsError()
    {
        // Arrange
        string command = "CAR BMW";

        _carApiServiceMock
            .Setup(s => s.GetCarInfoAsync("BMW", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _commandHandler.HandleCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal("ERROR InternalError", result.Response);
        Assert.False(result.IsLogout);
    }
}
