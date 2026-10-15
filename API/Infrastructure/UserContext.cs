using Contracts;
using Contracts.Models;
using FluentResults;
using Providers.OrgSphere;

namespace API.Infrastructure;

public class UserContext(IOrgSphereProvider orgSphere) : IUserContext
{
    public required Guid? UserId { get; set; } = null;
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public UserCard? UserCard { get; set; } = null;

    public async Task<Result> GetCard()
    {
        if (UserCard is not null) return Result.Ok();
        var uc = await orgSphere.GetUserAsync(new() { userId = UserId.Value });
        if (uc.IsFailed) return Result.Fail(uc.Errors);
        UserCard = uc.Value;
        return Result.Ok();
    }
    public async Task<Result<UserCard>> GetUserCardAsync()
    {
        if (UserCard is null)
        {
            var card = await GetCard();
            if (card.IsFailed) return Result.Fail(card.Errors);
            if (UserCard is null)
                return Result.Fail("User card not found");
        }
        return Result.Ok(UserCard);
    }
}
