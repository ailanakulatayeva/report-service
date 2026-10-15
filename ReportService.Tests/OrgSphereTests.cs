using Contracts.Messages;
using Contracts.Models;
using FluentResults;
using Moq;
using Providers.OrgSphere;
using Xunit;

namespace ReportService.Tests;

public class OrgSphereTests
{
	private readonly Mock<IOrgSphereProvider> _orgSphereMock;

	public OrgSphereTests()
	{
		_orgSphereMock = new Mock<IOrgSphereProvider>();
	}

	[Fact]
	public async Task CheckUserRoleAsync_UserHasRole()
	{
		_orgSphereMock
			.Setup(x => x.CheckUserRoleAsync(It.IsAny<string>()))
			.ReturnsAsync(Result.Ok());

		var result = await _orgSphereMock.Object.CheckUserRoleAsync("hse_dev");

		Assert.True(result.IsSuccess);
	}

	[Fact]
	public async Task CheckUserRoleAsync_UserHasNoRole()
	{
		_orgSphereMock
			.Setup(x => x.CheckUserRoleAsync(It.IsAny<string>()))
			.ReturnsAsync(Result.Fail("No role"));

		var result = await _orgSphereMock.Object.CheckUserRoleAsync("hse_dev");

		Assert.True(result.IsFailed);
	}

	[Fact]
	public async Task GetUserAsync_ExistingUserCard()
	{
		var userId = Guid.NewGuid();
		var userCard = new UserCard { UserId = userId };

		_orgSphereMock
			.Setup(x => x.GetUserAsync(It.IsAny<GetUserRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(Result.Ok(userCard));

		var result = await _orgSphereMock.Object.GetUserAsync(new GetUserRequest { userId = userId });

		Assert.True(result.IsSuccess);
		Assert.Equal(userId, result.Value.UserId);
	}

	[Fact]
	public async Task GetUserAsync_NonExistingUser()
	{
		_orgSphereMock
			.Setup(x => x.GetUserAsync(It.IsAny<GetUserRequest>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(Result.Fail("User not found"));

		var result = await _orgSphereMock.Object.GetUserAsync(new GetUserRequest { userId = Guid.NewGuid() });

		Assert.True(result.IsFailed);
	}

	[Fact]
	public async Task GetUsersByIdsAsync_ReturnUsers()
	{
		var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
		var users = ids.Select(id => new UserCard { UserId = id }).ToList();

		_orgSphereMock
			.Setup(x => x.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(Result.Ok<IEnumerable<UserCard>>(users));

		var result = await _orgSphereMock.Object.GetUsersByIdsAsync(ids);

		Assert.True(result.IsSuccess);
		Assert.Equal(2, result.Value.Count());
	}
}