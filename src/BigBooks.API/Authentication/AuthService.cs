using BigBooks.API.Authentication;
using BigBooks.API.Entities;
using BigBooks.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BigBooks.API.Services
{
    public class AuthService(IConfiguration config,
        IDbContextFactory<BigBookDbContext> dbContextFactory,
        ILogger<AuthService> logger) : IAuthService
    {
        /// <summary>
        /// return token for user
        /// return null if invalid/rejected
        /// </summary>
        /// <param name="request">authorization request</param>
        /// <returns></returns>
        public async Task<AuthResponse> GenerateToken(AuthRequest request)
        {
            logger.LogDebug("GenerateToken, {0}, {1}",
                request.UserId,
                request.Password);

            try
            {
                var cfgRoot = config as IConfigurationRoot;
                var keySecret = cfgRoot.GetValue<string>("Authentication:SecretForKey");
                var issuer = cfgRoot.GetValue<string>("Authentication:Issuer");
                var audience = cfgRoot.GetValue<string>("Authentication:Audience");

                AppUser matchedUser = null;
                using (var ctx = dbContextFactory.CreateDbContext())
                {
                    matchedUser = await ctx.AppUsers
                        .AsNoTracking()
                        .SingleOrDefaultAsync(u => (u.UserEmail == request.UserId));
                }

                if (matchedUser == null)
                {
                    return new AuthResponse
                    {
                        Token = null,
                        Expiration = null,
                        Error = $"User not found: {request.UserId}"
                    };
                }

                if (!matchedUser.Password.Equals(request.Password))
                {
                    return new AuthResponse
                    {
                        Token = null,
                        Expiration = null,
                        Error = "Invalid password"
                    };
                }

                var securityKey = new SymmetricSecurityKey(
                    Convert.FromBase64String(keySecret));
                var signingCredentials = new SigningCredentials(
                    securityKey, SecurityAlgorithms.HmacSha256);

                var claimsForToken = new List<Claim>();
                claimsForToken.Add(new Claim(JwtRegisteredClaimNames.Sub, matchedUser.UserEmail));
                claimsForToken.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                claimsForToken.Add(new Claim(ClaimTypes.Name, matchedUser.UserName));
                claimsForToken.Add(new Claim(ClaimTypes.Role, matchedUser.Role.ToString()));

                var tokenExpiration = DateTime.Now.AddHours(0.5);

                var jwtSecurityToken = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claimsForToken,
                    notBefore: DateTime.Now,
                    expires: tokenExpiration,
                    signingCredentials: signingCredentials);

                var userToken = new JwtSecurityTokenHandler()
                   .WriteToken(jwtSecurityToken);
                return new AuthResponse
                {
                    Token = userToken,
                    Expiration = tokenExpiration,
                    Error = string.Empty
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GenerateToken Fail, {0}, {1}",
                    request.UserId,
                    request.Password);
                return new AuthResponse
                {
                    Token = null,
                    Expiration = null,
                    Error = ex.Message
                };
            }
        }

    }
}
