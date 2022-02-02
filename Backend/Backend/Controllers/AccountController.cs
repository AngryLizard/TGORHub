using Backend.Context;
using Backend.Models;
using Backend.Models.Requests;
using Backend.Models.Assets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// Inspired by https://www.c-sharpcorner.com/article/authentication-authorization-using-net-core-web-api-using-jwt-token-and/
namespace Backend.Controllers
{
    /// <summary>
    /// Controller for all user account related queries:
    /// Register, login, group management
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : BackendController
    {
        /// <summary>
        /// Claim key used for permissions
        /// </summary>
        public static readonly string ScopeClaim = "scope";

        /// <summary>
        /// Claim key used for user identification
        /// </summary>
        public static readonly string UserClaim = "user";

        private readonly IConfiguration _config;
        private readonly ApplicationContext _context;

        ///
        public AccountController(IConfiguration config, ApplicationContext context)
        {
            _context = context;
            _config = config;
        }

        /// <summary>
        /// Register new user. Users are identified via email and the password is hashed immediately.
        /// Default with no special access and user-level permissions.
        /// </summary>
        [AllowAnonymous]
        [HttpPut("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                UserModel? user = _context.Users.SingleOrDefault(u => u.Email == request.Email);
                if (user == null)
                {
                    string hashed = _context.Hash(request.Password);
                    await _context.Users.AddAsync(new CredentialsModel()
                    {
                        Name = request.Username,
                        Email = request.Email,
                        Password = hashed,
                        Permissions = PermissionType.User,
                        Date = DateTime.UtcNow,
                        AssetAccess = new List<AssetModel>(),
                        GroupAccess = new List<GroupModel>(),
                        ProjectAccess = new List<ProjectModel>()
                    });

                    await _context.SaveChangesAsync();
                    return Ok();
                }
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retreive access token from user login.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("Login")]
        public ActionResult<TokenModel> Login([FromBody] LoginRequest request)
        {
            string hashed = _context.Hash(request.Password);
            try
            {
                UserModel? user = _context.Users.SingleOrDefault(u => u.Email == request.Email && u.Password == hashed);
                if (user != null)
                {
                    string userToken = generateToken(user);
                    return new TokenModel() { Token = userToken };
                }
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Validate whether user has valid token, return user information.
        /// </summary>
        [Authorize]
        [HttpGet("Validate")]
        public async Task<ActionResult<UserModel>> Validate()
        {
            UserModel? user = await _context.Users.FindIncludeAsync(GetUserId());
            if (user != null)
            {
                // In case claims changed we might want to regenerate token here in the far future
                //string userToken = generateToken(user);

                // Need to regenerate output so we don't send password/email
                return new UserModel()
                {
                    Date = user.Date,
                    AssetAccess = user.AssetAccess,
                    GroupAccess = user.GroupAccess,
                    ProjectAccess = user.ProjectAccess,
                    Permissions = user.Permissions,
                    Id = user.Id,
                    Name = user.Name
                };
            }
            return NotFound();
        }

        /// <summary>
        /// Get a list of all registered users.
        /// </summary>
        [Permissions(PermissionType.Accounts)]
        [HttpGet("List")]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetUsers()
        {
            return await _context.Users.Select<CredentialsModel, UserModel>(u => u).IncludeAll().ToListAsync();
        }

        /// <summary>
        /// Add access rights to a user.
        /// </summary>
        [Permissions(PermissionType.Accounts)]
        [HttpPut("Permission/{userId}")]
        public async Task<IActionResult> AddPermission(long userId, [FromBody] PermissionType access)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Add access flag
            user.Permissions |= access;
            return Ok();
        }

        /// <summary>
        /// Remove access rights from a user.
        /// </summary>
        [Permissions(PermissionType.Accounts)]
        [HttpDelete("Permission/{userId}")]
        public async Task<IActionResult> RemovePermission(long userId, [FromBody] PermissionType access)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Remove access flag
            user.Permissions &= ~access;
            return Ok();
        }

        /// <summary>
        /// Add a user into a group.
        /// </summary>
        [Permissions(PermissionType.Accounts)]
        [HttpPut("Group/{userId}")]
        public async Task<IActionResult> AddUserToGroup(long userId, [FromBody] long groupId)
        {
            UserModel? user = await _context.Users.FindIncludeAsync(userId);
            GroupModel? group = await _context.Groups.FindAsync(groupId);
            if (user != null && group != null)
            {
                if (!user.GroupAccess.Includes(group))
                {
                    user.GroupAccess.Append(group);
                    _context.Entry(user).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                return Ok();
            }
            return NotFound();
        }

        /// <summary>
        /// Remove a user from a group.
        /// </summary>
        [Permissions(PermissionType.Accounts)]
        [HttpDelete("Group/{userId}")]
        public async Task<IActionResult> RemoveUserFromGroup(long userId, [FromBody] long groupId)
        {
            UserModel? user = await _context.Users.FindIncludeAsync(userId);
            GroupModel? group = await _context.Groups.FindAsync(groupId);
            if (user != null && group != null)
            {
                if (user.GroupAccess.Includes(group))
                {
                    user.GroupAccess.Append(group);
                    _context.Entry(user).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                return Ok();
            }
            return NotFound();
        }

        private string generateToken(UserModel user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new Claim[] {
                        new Claim(UserClaim, user.Id.ToString()),
                        new Claim(ScopeClaim, user.Permissions.ToString())
                    };

            var token = new JwtSecurityToken(
                  _config["Jwt:Issuer"],
                  _config["Jwt:Audience"],
                  claims,
                  expires: DateTime.Now.AddMinutes(120),
                  signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
