using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Common.Services;
using Matrix.Audio.Models;
using Matrix.Audio.Server.Common;
using Matrix.Audio.Server.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;


namespace Matrix.Audio.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(
    IOptions<JwtConfig> jwtConfig,
    IEntityRepository entityRepository,
    IPasswordEncrypter passwordEncrypter,
    ITokenService tokenService,
    IEmailService emailService,
    IEmailTemplateService emailTemplateService,
    INewUserProcessor newUserProcessor,
    ILogger<AuthController> logger
    ) : ControllerBase
{
    private readonly JwtConfig _jwtConfig = jwtConfig.Value;
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly IPasswordEncrypter _passwordEncrypter = passwordEncrypter;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IEmailService _emailService = emailService;
    private readonly IEmailTemplateService _emailTemplateService = emailTemplateService;
    private readonly INewUserProcessor _newUserProcessor = newUserProcessor;
    private readonly ILogger<AuthController> _logger = logger;

    [HttpPost("login")]
    public async Task<LoginResult> Login([FromBody] LoginViewModel request)
    {
        return await LoginUser(request, UserRoles.User);
    }

    [HttpPost("login/tower")]
    public async Task<LoginResult> LoginTower([FromBody] LoginViewModel request)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrEmpty(_jwtConfig.TowerToken))
        {
            return new LoginResult { Success = false };
        }

        if (!_passwordEncrypter.Verify(_jwtConfig.TowerToken, request.Token))
        {
            return new LoginResult { Success = false };
        }

        return await LoginUser(request, UserRoles.Admin);
    }

    [HttpPost("refresh")]
    public async Task<LoginResult> Refresh([FromBody] RefreshTokenViewModel viewModel)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(viewModel.Token);
        if (principal == null)
            return LoginResult.Fail;

        var userEmail = principal.Identity?.Name;
        var userLogin = await _entityRepository.GetUserLoginByEmail(userEmail!);
        if (userLogin == null || userLogin.RefreshToken != viewModel.RefreshToken)
        {
            return LoginResult.Fail;
        }

        var user = await _entityRepository.GetUserByEmailAsync(userEmail!);
        var token = _tokenService.GenerateToken(user, UserRoles.User);
        var refreshToken = GenerateRefreshToken();

        userLogin.Token = token;
        userLogin.RefreshToken = refreshToken;
        userLogin.DateCreated = DateTime.UtcNow;
        await _entityRepository.UpdateAsync(userLogin);

        return new LoginResult
        {
            Success = true,
            Token = token,
            RefreshToken = refreshToken,
            User = user.ToViewModel()
        };
    }

    [HttpPost("register")]
    public async Task<RegisterResult> Register([FromBody] RegisterViewModel request)
    {
        var user = await _entityRepository.GetUserByEmailAsync(request.Email!);
        if (user != null)
        {
            return new RegisterResult { Success = false };
        }

        var number = new Random().Next(1, 17);
        var imagePrefix = number % 2 == 0 ? "girl-" : "boy-";
        var defaultLevel = await _entityRepository.GetAsync<AppSetting>(AppSettingKeys.AppDefaultUserLevel);

        user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            Password = _passwordEncrypter.Encrypt(request.Password!),
            Name = request.Name,
            DateCreated = DateTime.UtcNow,
            DateUpdated = DateTime.UtcNow,
            ImageId = $"{imagePrefix}{number}.png",
            Level = defaultLevel != null ? int.Parse(defaultLevel.Value) : 1000
        };

        var result = await _entityRepository.UpdateAsync(user);
        if (result)
        {
            await _newUserProcessor.Process(user.Id);
        }
        return new RegisterResult { Success = result };
    }

    [HttpPost("password/reset")]
    public async Task<ResultBase> ResetPassword([FromBody] ResetPasswordViewModel request)
    {
        var user = await _entityRepository.GetUserByEmailAsync(request.Email);
        if (user == null)
        {
            return new ResultBase { Success = false, Message = "User doesn't exist" };
        }

        try
        {
            var template = _emailTemplateService.GetTemplate(EmailTemplateNames.ResetPassword);
            if (template == null)
            {
                _logger.LogWarning("Email template {TemplateName} not found", EmailTemplateNames.ResetPassword);
                return new ResultBase { Success = false, Message = "Email template not found" };
            }

            var subject = _emailTemplateService.ProcessPlaceholders(template.Subject!, new Dictionary<string, string>
            {
                { "username", user.Name! }
            });
            var body = _emailTemplateService.ProcessPlaceholders(template.Body!, new Dictionary<string, string>
            {
                { "username", user.Name! },
                { "resetLink", "http://localhost:3000/reset-password" }
            });

            // send reset link to email

            var emailOption = new EmailOptions
            {
                Subject = subject,
                Body = body,
                To = [request.Email]
            };
            await _emailService.SendEmailAsync(emailOption);

            var result = await _entityRepository.UpdateAsync(user);
            return new ResultBase { Success = result, Message = "Reset email link has sent to your email." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email");
            return new ResultBase { Success = false, Message = $"Failed to send email to {request.Email}" };
        }
    }

    [HttpPost("password/update")]
    [Authorize]
    public async Task<ResultBase> UpdatePassword(UpdatePasswordViewModel request)
    {
        var user = await _entityRepository.GetUserByEmailAsync(request.Email);
        if (user == null)
        {
            return new ResultBase { Success = false, Message = "User doesn't exist" };
        }

        if (!_passwordEncrypter.Verify(user.Password!, request.OldPassword))
        {
            return new ResultBase { Success = false, Message = "Old password is incorrect." };
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            return new ResultBase { Success = false, Message = "New password and confirmation do not match." };
        }

        user.Password = _passwordEncrypter.Encrypt(request.NewPassword);
        var result = await _entityRepository.UpdateAsync(user);
        return new ResultBase { Success = result, Message = "Password updated successfully." };
    }

    [HttpGet("user/{id}")]
    [Authorize]
    public async Task<UserViewModel> GetUserById(string id)
    {
        var user = await _entityRepository.GetAsync<User>(id);
        var profile = await _entityRepository.GetProfileForUserAsync(id);
        var readBooks = await _entityRepository.GetUserPlayAlbumCountAsyn(id);
        var userLogin = await _entityRepository.GetUserLogin(id);

        if (user == null)
        {
            return new UserViewModel();
        }

        var viewModel = user.ToViewModel(profile);
        viewModel.ReadBooks = readBooks;
        viewModel.DateLastLogin = userLogin == null ? DateTime.Now : userLogin.DateCreated;

        return viewModel;
    }

    [HttpPost("user/profile")]
    [Authorize]
    public async Task<ResultBase> UpdateProfile([FromBody] UserViewModel viewModel)
    {
        var user = await _entityRepository.GetAsync<User>(viewModel.Id!);
        if (user == null)
        {
            return new ResultBase { Success = false, Message = "User doesn't exist" };
        }

        user.Name = viewModel.Name;
        user.DateUpdated = DateTime.UtcNow;
        await _entityRepository.UpdateAsync(user);

        var profile = await _entityRepository.GetProfileForUserAsync(viewModel.Id!);
        profile ??= new UserProfile
        {
            Id = Guid.NewGuid().ToString(),
            UserId = viewModel.Id!,
            DateCreated = DateTime.UtcNow,
            DateUpdated = DateTime.UtcNow
        };
        profile.FirstName = viewModel.FirstName;
        profile.LastName = viewModel.LastName;
        profile.Bio = viewModel.Bio;
        profile.DateUpdated = DateTime.UtcNow;

        var result = await _entityRepository.UpdateAsync(profile);
        return new ResultBase { Success = result };
    }

    [HttpPost("encrypt{password}")]
    public string Encrypt(string password)
    {
        return _passwordEncrypter.Encrypt(password);
    }

    private async Task<LoginResult> LoginUser(LoginViewModel request, string role)
    {
        try
        {
            var user = await _entityRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                return new LoginResult { Success = false };
            }

            if (!_passwordEncrypter.Verify(user.Password!, request.Password))
            {
                return new LoginResult { Success = false };
            }


            var defaultLevel = await _entityRepository.GetAsync<AppSetting>(AppSettingKeys.AppDefaultUserLevel);
            if (defaultLevel != null)
            {
                user.Level = int.Parse(defaultLevel.Value);
                await _entityRepository.UpdateAsync(user);
            }

            var token = _tokenService.GenerateToken(user, role);
            var refreshToken = GenerateRefreshToken();

            var userLogin = await _entityRepository.GetUserLogin(user.Id);
            if (userLogin == null)
            {
                userLogin = new UserLogin
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = user.Id,
                    UserEmail = user.Email!,
                    Token = token,
                    RefreshToken = refreshToken,
                    DateCreated = DateTime.UtcNow
                };
            }
            else
            {
                userLogin.Token = token;
                userLogin.RefreshToken = refreshToken;
                userLogin.DateCreated = DateTime.UtcNow;
            }
            await _entityRepository.UpdateAsync(userLogin);

            return new LoginResult
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                User = user.ToViewModel()
            };
        }
        catch (Exception e)
        {
            string message = $"{e.Message}\n {e.StackTrace}";
            return LoginResult.Error(message);
        }
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}
