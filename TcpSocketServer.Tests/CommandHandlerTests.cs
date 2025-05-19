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
        Assert.Equal("ERROR EmptyCommand", result);
    }

    [Fact]
    public async Task HandleCommandAsync_UnknownCommand_ReturnsError()
    {
        // Arrange
        string command = "UNKNOWN";

        // Act
        var result = await _commandHandler.HandleCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal("ERROR UnknownCommand", result);
    }

    [Fact]
    public async Task HandleCommandAsync_LogoutCommand_ReturnsGoodbye()
    {
        // Arrange
        string command = "LOGOUT";

        // Act
        var result = await _commandHandler.HandleCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal("OK Goodbye", result);
    }

    [Fact]
    public async Task HandleCommandAsync_PingCommand_ReturnsPong()
    {
        // Arrange
        string command = "PING";

        // Act
        var result = await _commandHandler.HandleCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal("PONG", result);
    }

    [Fact]
    public async Task HandleCommandAsync_CarCommandWithoutBrand_ReturnsError()
    {
        // Arrange
        string command = "CAR";

        // Act
        var result = await _commandHandler.HandleCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal("ERROR MissingCarParameter", result);
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
        Assert.Equal(expected, result);
        _carApiServiceMock.Verify(s => s.GetCarInfoAsync("BMW", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleCommandAsync_CarCommandWithMultipleArgs_UseFirstArgAsBrand()
    {
        // Arrange
        string command = "CAR BMW Additional Arguments";
        string expected = "CAR INFO: BMW - Founded: 1916, Country: Germany";
        
        _carApiServiceMock
            .Setup(s => s.GetCarInfoAsync("BMW", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _commandHandler.HandleCommandAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(expected, result);
        _carApiServiceMock.Verify(s => s.GetCarInfoAsync("BMW", It.IsAny<CancellationToken>()), Times.Once);
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
        Assert.Equal(expected, result);
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
        Assert.Equal(expected, result);
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
        Assert.Equal("ERROR InternalError", result);
    }
}