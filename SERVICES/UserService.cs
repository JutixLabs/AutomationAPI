using AutomationAPI.DATA;
using AutomationAPI.MODEL.DTO;
using AutomationAPI.MODEL.Entity;
using AutomationAPI.MODEL.Interface;
using AutomationAPI.SERVICES.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AutomationAPI.SERVICES
{
    public class UserService : BaseEntityService<User>, IUserService
    {
        private readonly AppDbContext _dbContext;
        private readonly ICustomIdGenerator _idGen;
        private readonly IHttpContextAccessor _httpContextAccessor;
        //private readonly IEmailService _emailService;
        private readonly ILogger<UserService> _logger;
        public UserService(AppDbContext dbContext, ICustomIdGenerator idGen, IHttpContextAccessor httpContextAccessor,
            /*IEmailService emailService,*/ ILogger<UserService> logger) : base(dbContext)
        {
            _dbContext = dbContext;
            _idGen = idGen;
            _httpContextAccessor = httpContextAccessor;
            //_emailService = emailService;
            _logger = logger;
        }
        public async Task<User> AddUserAsync(User model)
        {
            try
            {
                var email = model.Email.ToLower().Trim();
                var validateUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
                if (validateUser != null)
                    throw new Exception("User with email already exists.");

                var password = model.PasswordHash;
                if (string.IsNullOrEmpty(password) || password.Length < 8 || !Regex.IsMatch(password, @"[!@#$%^&*(),.?""{}|<>]"))
                    throw new Exception("Password must be at least 8 characters long and contain at least one special character.");
                
                //var token = Guid.NewGuid().ToString();

                var user = new User
                {
                    ID = await GenerateUniqueIdAsync(() => _idGen.TimeStamped("USER", 8, "yyyy-MM")),
                    FullName = model.FullName,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    //EmailVerificationToken = token
                };

                await _dbContext.Users.AddAsync(user);
                await _dbContext.SaveChangesAsync();

                //var link = $"https://jutix-automation-api.vercel.app/verify-email?token={token}";

                //await _emailService.SendEmailAsync(user.Email, "Verify your email",
                //    $"Click to verify: {link}");

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Error]: {ex.Message}");
                throw;
            }
        }

        public async Task ChangePasswordAsync(ChangePasswordRequest model)
        {
            try
            {
                var user = await GetCurrentUserAsync();

                if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.PasswordHash))
                    throw new Exception("Incorrect Password.");

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

                await _dbContext.SaveChangesAsync();

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Error]: {ex.Message}");
                throw;
            }
        }

        public async Task<string> DeleteAccountAsync()
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();

                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.ID == userId);

                _dbContext.Users.Remove(user);

                _logger.LogInformation($"User with ID {userId} deleted their account. Details: {user.FullName} ({user.Email})");

                await _dbContext.SaveChangesAsync();
                return "Account deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Error]: {ex.Message}");
                throw;
            }
        }

        public async Task<User> GetCurrentUserAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetLoggedInUserId();

            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.ID == userId);
                 
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Error]: {ex.Message}");
                throw;
            }
        }

        public async Task<User> UpdateProfileAsync(UpdateProfileRequest model)
        {
            try
            {
                var user = await GetCurrentUserAsync();

                user.FullName = model.FullName;

                await _dbContext.SaveChangesAsync();

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Error]: {ex.Message}");
                throw;
            }
        }

        //public async Task VerifyEmailAsync(string token)
        //{
        //    var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);
        //    if (user == null)
        //        throw new Exception("Invalid token");

        //    user.IsEmailVerified = true;
        //    user.EmailVerificationToken = string.Empty;

        //    await _dbContext.SaveChangesAsync();
        //    return;
        //}
    }
}
