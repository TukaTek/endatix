using Microsoft.Extensions.Logging;
using Endatix.Core.Events;
using Endatix.Core.Abstractions;
using Endatix.Core.Features.Email;
using MediatR;

namespace Endatix.Core.Handlers;

/// <summary>
/// Default event handler for UserRegisteredEvent.
/// </summary>
internal sealed class UserRegisteredEventHandler(
    IEmailVerificationService emailVerificationService,
    IEmailTemplateService emailTemplateService,
    IEmailSender emailSender,
    ILogger<UserRegisteredEventHandler> logger) : INotificationHandler<UserRegisteredEvent>
{
    public async Task Handle(UserRegisteredEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogTrace("Handling User Registered event for {@eventData}", domainEvent.User);

        try
        {
            // Create verification token
            var tokenResult = await emailVerificationService.CreateVerificationTokenAsync(domainEvent.User.Id, cancellationToken);

            if (tokenResult.IsSuccess && tokenResult.Value != null)
            {
                // Create and send verification email
                var emailModel = emailTemplateService.CreateVerificationEmail(
                    domainEvent.User.Email,
                    tokenResult.Value.Token);

                await emailSender.SendEmailAsync(emailModel, cancellationToken);

                logger.LogInformation("Verification email sent successfully to {Email} for user {UserId}",
                    domainEvent.User.Email, domainEvent.User.Id);
            }
            else
            {
                logger.LogError("Failed to create verification token for user {UserId}. Errors: {Errors}",
                    domainEvent.User.Id,
                    string.Join(", ", tokenResult.ValidationErrors.Select(e => e.ErrorMessage)));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send verification email for user {UserId} ({Email})",
                domainEvent.User.Id, domainEvent.User.Email);
        }
    }
}
