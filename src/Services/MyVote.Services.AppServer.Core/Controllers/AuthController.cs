using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MyVote.Services.AppServer.Models;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MyVote.Services.AppServer.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
		
        public AuthController(
            SignInManager<ApplicationUser> signInManager,
            ILoggerFactory loggerFactory)
        {
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger<AuthController>();
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string redirect_uri)
        {
            provider = (char.ToUpper(provider[0]) + provider.Substring(1));
            // Request a redirect to the external login provider.
            var properties = ConfigureExternalAuthenticationProperties(provider,
                Url.Action("ExternalLoginCallback", "Auth",
                new { ReturnUrl = redirect_uri }));
            return Challenge(properties, provider);
        }
        private const string LoginProviderKey = "LoginProvider";
        private const string XsrfKey = "KEY";
        private AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl, string userId = null)
        {
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            properties.Items[LoginProviderKey] = provider;
            if (userId != null)
            {
                properties.Items[XsrfKey] = userId;
            }
            return properties;
        }

        //
        // GET: /Auth/ExternalLoginCallback
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return BadRequest("Not successfully authenticated by external provider");
            }
            ExternalLoginInfo loginInfo = null;
            try
            {
                loginInfo = await _signInManager.GetExternalLoginInfoAsync();
                if (loginInfo == null)
                {
                    return BadRequest("Not successfully authenticated by external provider");
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                throw;
            }
            var userId = loginInfo.Principal.FindFirstValue(ClaimTypes.Email) ?? loginInfo.Principal.FindFirstValue(ClaimTypes.Name);

            var redirectUri = string.Format("{0}#provider={1}&haslocalaccount={2}&external_user_name={3}&user_id={4}",
                                        returnUrl,
                                        loginInfo.LoginProvider,
                                        "True",
                                        userId,
                                        loginInfo.LoginProvider + ":" + loginInfo.ProviderKey);
            return Redirect(redirectUri);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ObtainLocalAccessToken(string provider, string userName)
        {
            //TODO: Need to perform Token validation with the Provider
            //      as well as verifying within MyVote database the userid exists

            //IdentityUser user = await _repo.FindAsync(new UserLoginInfo(provider, verifiedAccessToken.user_id));
            //bool hasRegistered =  user != null;
            //if (!hasRegistered)
            //{
            //    return BadRequest("External user is not registered");
            //}

            //generate access token response
            return Ok(GenerateToken(userName));
        }

        #region Helpers
        private string GenerateToken(string username)
        {
            var now = DateTime.UtcNow;

            // Specifically add the jti (nonce), iat (issued timestamp), and sub (subject/user) claims.
            // You can add other claims here, if you want:
            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(now).ToString(), ClaimValueTypes.Integer64)
            };
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Constants.SecretKey));
            var expiration = TimeSpan.FromMinutes(60);
            // Create the JWT and write it to a string
            var jwt = new JwtSecurityToken(issuer: Constants.TokenIssuer,
                                            audience: Constants.TokenAudience,
                                            claims: claims,
                                            notBefore: now,
                                            expires: now.Add(expiration),
                                            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                userName = username,
                expires_in = (int)expiration.TotalSeconds
            };

            // Serialize and return the response
            return JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented });
        }


        /// <summary>
        /// Get this datetime as a Unix epoch timestamp (seconds since Jan 1, 1970, midnight UTC).
        /// </summary>
        /// <param name="date">The date to convert.</param>
        /// <returns>Seconds since Unix epoch.</returns>
        private static long ToUnixEpochDate(DateTime date)
            => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
        #endregion
    }
}
