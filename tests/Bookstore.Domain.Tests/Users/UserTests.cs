using Bookstore.Domain.Users;
using Bookstore.SharedKernel.Results;
using Shouldly;
using Xunit;

namespace Bookstore.Domain.Tests.Users;

public class UserTests
{
    [Fact]
    public void Create_ShouldReturnUserWithCorrectProperties()
    {
        // Arrange
        const string email = "john@example.com";
        const string passwordHash = "hashed-password-value";
        IReadOnlyCollection<Role> roles = [Role.User];

        // Act
        var result = User.Create(email, passwordHash, roles);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.Value.ShouldNotBe(Guid.Empty);
        result.Value.Email.ShouldBe(email);
        result.Value.PasswordHash.ShouldBe(passwordHash);
        result.Value.Roles.ShouldBe(roles);
    }

    [Fact]
    public void Create_ShouldAssignMultipleRoles()
    {
        // Arrange
        IReadOnlyCollection<Role> roles = [Role.User, Role.Admin];

        // Act
        var result = User.Create("admin@example.com", "hashed-password", roles);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Roles.Count.ShouldBe(2);
        result.Value.Roles.ShouldContain(Role.User);
        result.Value.Roles.ShouldContain(Role.Admin);
    }

    [Theory]
    [InlineData("", "hashed", "Email is required.")]
    [InlineData("   ", "hashed", "Email is required.")]
    public void Create_ShouldReturnValidationError_WhenEmailIsInvalid(
        string email, string passwordHash, string expectedMessage)
    {
        // Act
        var result = User.Create(email, passwordHash, [Role.User]);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe(expectedMessage);
    }

    [Theory]
    [InlineData("john@example.com", "", "Password hash is required.")]
    [InlineData("john@example.com", "   ", "Password hash is required.")]
    public void Create_ShouldReturnValidationError_WhenPasswordHashIsInvalid(
        string email, string passwordHash, string expectedMessage)
    {
        // Act
        var result = User.Create(email, passwordHash, [Role.User]);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe(expectedMessage);
    }

    [Fact]
    public void Create_ShouldReturnValidationError_WhenRolesAreEmpty()
    {
        // Act
        var result = User.Create("john@example.com", "hashed", Array.Empty<Role>());

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe("At least one role is required.");
    }

    [Fact]
    public void Update_ShouldModifyEmailAndRoles()
    {
        // Arrange
        var user = User.Create("old@example.com", "hashed", [Role.User]).Value;

        // Act
        var result = user.Update("new@example.com", [Role.Admin]);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        user.Email.ShouldBe("new@example.com");
        user.Roles.ShouldHaveSingleItem().ShouldBe(Role.Admin);
    }

    [Theory]
    [InlineData("", "Email is required.")]
    [InlineData("   ", "Email is required.")]
    public void Update_ShouldReturnValidationError_WhenEmailIsInvalid(
        string email, string expectedMessage)
    {
        // Arrange
        var user = User.Create("old@example.com", "hashed", [Role.User]).Value;

        // Act
        var result = user.Update(email, [Role.User]);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe(expectedMessage);
    }

    [Fact]
    public void Update_ShouldReturnValidationError_WhenRolesAreEmpty()
    {
        // Arrange
        var user = User.Create("old@example.com", "hashed", [Role.User]).Value;

        // Act
        var result = user.Update("new@example.com", Array.Empty<Role>());

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe("At least one role is required.");
    }

    [Fact]
    public void UpdatePasswordHash_ShouldModifyPasswordHash()
    {
        // Arrange
        var user = User.Create("john@example.com", "old-hash", [Role.User]).Value;

        // Act
        var result = user.UpdatePasswordHash("new-hash");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        user.PasswordHash.ShouldBe("new-hash");
    }

    [Theory]
    [InlineData("", "Password hash is required.")]
    [InlineData("   ", "Password hash is required.")]
    public void UpdatePasswordHash_ShouldReturnValidationError_WhenHashIsInvalid(
        string passwordHash, string expectedMessage)
    {
        // Arrange
        var user = User.Create("john@example.com", "old-hash", [Role.User]).Value;

        // Act
        var result = user.UpdatePasswordHash(passwordHash);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ValidationError>()
            .Description.ShouldBe(expectedMessage);
    }
}
