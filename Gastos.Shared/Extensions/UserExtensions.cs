using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Gastos.Shared.Extensions;

public static class UserExtensions
{
    private const string DemoUserId = "Demo";
    private const string DemoUserName = "Demo User";

    public static bool IsAuthenticated(this ClaimsPrincipal user)
    {
        return user?.Identity?.IsAuthenticated ?? false;
    }

    public static string GetUserId(this HttpContext httpContext)
    {
        return httpContext.User.GetUserId() ?? DemoUserId;
    }

    public static string GetUserId(this IHttpContextAccessor contextAccessor)
    {
        return contextAccessor?.HttpContext?.User.GetUserId() ?? DemoUserId;
    }

    public static string GetUserId(this ClaimsPrincipal user)
    {
        if (user.HasNoClaims()) return DemoUserId;

        var claim = user.GetClaim_NameIdentifier();
        if (claim is not null) return claim.Value;

        claim = user.GetClaim_ObjectIdentifier();
        if (claim is not null) return claim.Value;

        return DemoUserId;
    }

    public static string GetUserName(this ClaimsPrincipal user)
    {
        if (user.HasNoClaims()) return DemoUserName;

        var claim = user.GetClaim_Name();
        claim ??= user.GetClaim_PreferredName();

        return claim?.Value ?? DemoUserName;
    }

    public static string GetUserMail(this ClaimsPrincipal user)
    {
        if (user.HasNoClaims()) return "";

        var claim = user.GetClaim_EMail();
        if (claim is not null && claim.Value.IsValidEmail()) return claim.Value;

        claim = user.GetClaim_PreferredName();
        if (claim is not null && claim.Value.IsValidEmail()) return claim.Value;

        claim = user.GetClaim_Name();
        if (claim is not null && claim.Value.IsValidEmail()) return claim.Value;


        return "";
    }

    public static List<string> GetUserGroups(this ClaimsPrincipal user) => [.. user.Claims.Where(c => c.Type == "groups").Select(c => c.Value)];


    private static bool HasNoClaims(this ClaimsPrincipal user) => !user.Claims.Any();

    private static Claim? GetClaim_NameIdentifier(this ClaimsPrincipal user) => user.Claims.FirstOrDefault(c => c.Type.Contains("nameidentifier"));
    private static Claim? GetClaim_ObjectIdentifier(this ClaimsPrincipal user) => user.Claims.FirstOrDefault(c => c.Type.Contains("objectidentifier") || c.Type == "oid");
    private static Claim? GetClaim_Name(this ClaimsPrincipal user) => user.Claims.FirstOrDefault(c => c.Type == "name");
    private static Claim? GetClaim_PreferredName(this ClaimsPrincipal user) => user.Claims.FirstOrDefault(c => c.Type.Contains("preferred_username"));
    private static Claim? GetClaim_EMail(this ClaimsPrincipal user) => user.Claims.FirstOrDefault(c => c.Type.Contains("mail"));


    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Normalize the domain
            email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                  RegexOptions.None, TimeSpan.FromMilliseconds(200));

            // Examines the domain part of the email and normalizes it.
            static string DomainMapper(Match match)
            {
                // Use IdnMapping class to convert Unicode domain names.
                var idn = new IdnMapping();

                // Pull out and process domain name (throws ArgumentException on invalid)
                string domainName = idn.GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }

        try
        {
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
}


