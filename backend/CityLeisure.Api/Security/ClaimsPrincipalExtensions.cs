using System.Security.Claims;

namespace CityLeisure.Api.Security;

public static class ClaimsPrincipalExtensions
{
    public static int GetRequiredUserId(this ClaimsPrincipal user)
    {
        var rawUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(rawUserId, out var userId) ? userId : 0;
    }

    public static int? GetOptionalUserId(this ClaimsPrincipal user)
    {
        var rawUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(rawUserId, out var userId) && userId > 0 ? userId : null;
    }
}
