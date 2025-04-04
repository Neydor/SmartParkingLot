using Moq;
using SmartParkingLot.Application.DTOs.Request;
using SmartParkingLot.Application.Exceptions;
using SmartParkingLot.Application.Interfaces;
using SmartParkingLot.Application.Services;
using SmartParkingLot.Domain.Entities;
using SmartParkingLot.Domain.Enums;
using SmartParkingLot.Domain.Interfaces;

namespace SmartParkingLot.Tests;

public class ParkingServiceTests
{
    private readonly Mock<IParkingSpotRepository> _mockSpotRepo;
    private readonly Mock<IDeviceRepository> _mockDeviceRepo;
    private readonly Mock<IRateLimiterService> _mockRateLimiter;
    private readonly ParkingService _parkingService;

    private readonly Guid _testSpotId = Guid.NewGuid();
    private readonly Guid _testSpotId2 = Guid.NewGuid();
    private readonly Guid _testSpotIdWithDevice = Guid.NewGuid();
    private readonly Guid _testDeviceId = Guid.NewGuid();
    private readonly Guid _testDeviceId2 = Guid.NewGuid();
    private readonly Guid _testDeviceId3 = Guid.NewGuid();
    private readonly Guid _unregisteredDeviceId = Guid.NewGuid();
    private readonly ParkingSpot _testSpot;
    private readonly ParkingSpot _testSpot2;
    private readonly ParkingSpot _testSpot3;

    public ParkingServiceTests()
    {
        _mockSpotRepo = new Mock<IParkingSpotRepository>();
        _mockDeviceRepo = new Mock<IDeviceRepository>();
        _mockRateLimiter = new Mock<IRateLimiterService>();

        _parkingService = new ParkingService(
            _mockSpotRepo.Object,
            _mockDeviceRepo.Object,
            _mockRateLimiter.Object);

        _testSpot = new ParkingSpot(_testSpotId, "TestSpotA1");
        _testSpot2 = new ParkingSpot(_testSpotId2, "TestSpotA2"); 
        _testSpot3 = new ParkingSpot(_testSpotIdWithDevice, "TestWithDeviceA3"); 
        //_testSpot3.Occupy(_testDeviceId3); 

        // Default setups
        _mockDeviceRepo.Setup(r => r.IsDeviceRegisteredAsync(_testDeviceId)).ReturnsAsync(true);
        _mockDeviceRepo.Setup(r => r.IsDeviceRegisteredAsync(_testDeviceId2)).ReturnsAsync(true);
        _mockDeviceRepo.Setup(r => r.IsDeviceRegisteredAsync(_testDeviceId3)).ReturnsAsync(true);
        _mockDeviceRepo.Setup(r => r.IsDeviceRegisteredAsync(_unregisteredDeviceId)).ReturnsAsync(false);
        _mockSpotRepo.Setup(r => r.GetByIdAsync(_testSpotId)).ReturnsAsync(_testSpot);
        _mockSpotRepo.Setup(r => r.ExistsAsync(_testSpotId)).ReturnsAsync(true);
        _mockSpotRepo.Setup(r => r.GetByIdAsync(_testSpotId2)).ReturnsAsync(_testSpot2);
        _mockSpotRepo.Setup(r => r.ExistsAsync(_testSpotId2)).ReturnsAsync(true);
        _mockSpotRepo.Setup(r => r.GetByIdAsync(_testSpotIdWithDevice)).ReturnsAsync(_testSpot3);
        _mockSpotRepo.Setup(r => r.ExistsAsync(_testSpotIdWithDevice)).ReturnsAsync(true);
        _mockSpotRepo.Setup(r => r.ExistsOccupiedSpotByDeviceAsync(_testSpotIdWithDevice)).ReturnsAsync(true);
        _mockRateLimiter.Setup(r => r.IsActionAllowedAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(true); // Allow by default
    }

    [Fact]
    public async Task OccupySpotAsync_ValidRequest_ShouldOccupySpot()
    {
        // Arrange (Spot is initially Free in _testSpot setup)

        // Act
        await _parkingService.OccupySpotAsync(_testSpotId, _testDeviceId);

        // Assert
        Assert.Equal(ParkingSpotStatus.Occupied, _testSpot.Status);
        Assert.Equal(_testDeviceId, _testSpot.OccupyingDeviceId);
        _mockSpotRepo.Verify(r => r.UpdateAsync(It.Is<ParkingSpot>(s => s.Id == _testSpotId && s.Status == ParkingSpotStatus.Occupied)), Times.Once);
        _mockRateLimiter.Verify(r => r.IsActionAllowedAsync(_testDeviceId, It.IsAny<string>()), Times.Once); // Verify rate limit checked
    }

    [Fact]
    public async Task OccupySpotAsync_SpotNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockSpotRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ParkingSpot?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _parkingService.OccupySpotAsync(Guid.NewGuid(), _testDeviceId));
    }

    [Fact]
    public async Task OccupySpotAsync_UnregisteredDevice_ShouldThrowValidationException()
    {
        // Arrange
        _mockRateLimiter.Setup(r => r.IsActionAllowedAsync(_unregisteredDeviceId, It.IsAny<string>())).ReturnsAsync(true); // Still check rate limit first

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _parkingService.OccupySpotAsync(_testSpotId, _unregisteredDeviceId));
        _mockSpotRepo.Verify(r => r.UpdateAsync(It.IsAny<ParkingSpot>()), Times.Never); // Should not update
    }

    [Fact]
    public async Task OccupySpotAsync_SpotAlreadyOccupied_ShouldThrowConflictException()
    {
        // Arrange
        _testSpot.Occupy(_testDeviceId); // Pre-occupy the spot

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(() => _parkingService.OccupySpotAsync(_testSpotId, _testDeviceId));
        Assert.Contains("already occupied", exception.Message);
        _mockSpotRepo.Verify(r => r.UpdateAsync(It.IsAny<ParkingSpot>()), Times.Never); // Should not update again
    }

    [Fact]
    public async Task FreeSpotAsync_DeviceNotTheSame_ShouldThrowConflictException()
    {
        // Arrange
        _testSpot.Occupy(_testDeviceId); // Pre-occupy the spot with _testDeviceId

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _parkingService.FreeSpotAsync(_testSpotId, _testDeviceId2));
        Assert.Contains("as it is not occupying it", exception.Message);
        _mockSpotRepo.Verify(r => r.UpdateAsync(It.IsAny<ParkingSpot>()), Times.Never); // Should not update
    }

    [Fact]
    public async Task FreeSpotAsync_SpotAlreadyFree_ShouldThrowConflictException()
    {
        // Arrange
        _testSpot.Occupy(_testDeviceId); 
        _testSpot.Free(); 

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ConflictException>(() => _parkingService.FreeSpotAsync(_testSpotId, _testDeviceId));
        Assert.Contains("already free", exception.Message);
        _mockSpotRepo.Verify(r => r.UpdateAsync(It.IsAny<ParkingSpot>()), Times.Never); // Should not update
    }

    [Fact]
    public async Task OccupySpotAsync_RateLimitExceeded_ShouldThrowValidationException()
    {
        // Arrange
        _mockRateLimiter.Setup(r => r.IsActionAllowedAsync(_testDeviceId, It.IsAny<string>())).ReturnsAsync(false); // Simulate rate limit hit

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _parkingService.OccupySpotAsync(_testSpotId, _testDeviceId));
        Assert.Contains("rate limit exceeded", exception.Message); // Check for specific message
        _mockDeviceRepo.Verify(r => r.IsDeviceRegisteredAsync(It.IsAny<Guid>()), Times.Never); // Should not check device registration if rate limited
        _mockSpotRepo.Verify(r => r.UpdateAsync(It.IsAny<ParkingSpot>()), Times.Never); // Should not update
    }
    
    [Fact]
    public async Task OccupySpotAsync_DeviceInAnotherSpot_ShouldThrowValidationException()
    {
        // Arrange
        _testSpot3.Occupy(_testDeviceId3); // Pre-occupy another spot with the device
        _mockSpotRepo.Setup(r => r.ExistsOccupiedSpotByDeviceAsync(_testDeviceId3)).ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _parkingService.OccupySpotAsync(_testSpotId, _testDeviceId3));
        Assert.Contains("is occupied a parking spot", exception.Message);
        _mockSpotRepo.Verify(r => r.UpdateAsync(It.IsAny<ParkingSpot>()), Times.Never); // Should not update again
    }

    [Fact]
    public async Task AddSpotAsync_ValidData_ShouldAddSpotAndReturnDto()
    {
        // Arrange
        var createDto = new CreateParkingSpotDto { Name = "NewSpotB1" };
        ParkingSpot? capturedSpot = null;
        // Capture the added spot to verify its properties
        _mockSpotRepo.Setup(r => r.AddAsync(It.IsAny<ParkingSpot>()))
                     .Callback<ParkingSpot>(spot => capturedSpot = spot)
                     .Returns(Task.CompletedTask);

        // Act
        var resultDto = await _parkingService.AddSpotAsync(createDto);

        // Assert
        _mockSpotRepo.Verify(r => r.AddAsync(It.IsAny<ParkingSpot>()), Times.Once);
        Assert.NotNull(capturedSpot);
        Assert.Equal(createDto.Name, capturedSpot.Name);
        Assert.Equal(ParkingSpotStatus.Free, capturedSpot.Status);
        Assert.NotNull(resultDto);
        Assert.Equal(capturedSpot.Id, resultDto.Id);
        Assert.Equal(createDto.Name, resultDto.Name);
        Assert.Equal("Free", resultDto.Status);
    }

    [Fact]
    public async Task DeleteSpotAsync_ExistingSpot_ShouldCallRepositoryDelete()
    {
        // Arrange (ExistsAsync setup in constructor)

        // Act
        await _parkingService.DeleteSpotAsync(_testSpotId);

        // Assert
        _mockSpotRepo.Verify(r => r.DeleteAsync(_testSpotId), Times.Once);
    }

    [Fact]
    public async Task DeleteSpotAsync_NonExistingSpot_ShouldThrowNotFoundException()
    {
        // Arrange
        _mockSpotRepo.Setup(r => r.ExistsAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _parkingService.DeleteSpotAsync(Guid.NewGuid()));
        _mockSpotRepo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}