using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Domain.Entities;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Infrastructure.Services;

namespace fundoo_notes.Application.Features.Users.Commands.RegisterUser
{
    /// <summary>
    /// Handler for RegisterUserCommand
    /// </summary>
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ILogger<RegisterUserCommandHandler> _logger;

        public RegisterUserCommandHandler(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IJwtTokenService jwtTokenService,
            IEmailService emailService,
            IMapper mapper,
            ILogger<RegisterUserCommandHandler> logger)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _jwtTokenService = jwtTokenService;
            _emailService = emailService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AuthResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing user registration for email: {Email}", request.Email);

            // Check if user already exists
            if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            // Create new user
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = _passwordService.HashPassword(request.Password),
                EmailVerificationToken = _passwordService.GenerateRandomToken(),
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
                IsEmailVerified = false,
                IsActive = true
            };

            // Save user to database
            await _userRepository.AddAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User registered successfully with ID: {UserId}", user.Id);

            // Send email verification
            try
            {
                await _emailService.SendEmailVerificationAsync(user.Email, user.EmailVerificationToken!);
                _logger.LogInformation("Email verification sent to: {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send email verification to: {Email}", user.Email);
                // Don't fail registration if email sending fails
            }

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(user);
            var tokenExpiration = _jwtTokenService.GetTokenExpiration(token);

            var userDto = _mapper.Map<UserDto>(user);

            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = tokenExpiration,
                User = userDto
            };
        }
    }
}
