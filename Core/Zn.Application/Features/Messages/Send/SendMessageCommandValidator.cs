using FluentValidation;
using Zn.Domain.Entity;

namespace Zn.Application.Features.Messages.Send
{
    /// <summary>
    /// <see cref="SendMessageCommand"/> için FluentValidation kuralları. Uzunluk sınırları
    /// Message entity'sindeki sabitlerle (ve dolayısıyla MessageConfiguration kolon limitleriyle)
    /// senkrondur. Bu sınırlar aynı zamanda bot/spam yükünü kısmak için bilinçli üst sınırlardır.
    /// Validator yalnızca komuta uygulanır; entity invariant'ları Domain'dedir.
    /// </summary>
    public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
    {
        public SendMessageCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(Message.NameMaxLength)
                .WithMessage($"Name must not exceed {Message.NameMaxLength} characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.")
                .MaximumLength(Message.EmailMaxLength)
                .WithMessage($"Email must not exceed {Message.EmailMaxLength} characters.");

            RuleFor(x => x.Subject)
                .NotEmpty().WithMessage("Subject is required.")
                .MaximumLength(Message.SubjectMaxLength)
                .WithMessage($"Subject must not exceed {Message.SubjectMaxLength} characters.");

            RuleFor(x => x.MessageBody)
                .NotEmpty().WithMessage("Message body is required.")
                .MaximumLength(Message.MessageBodyMaxLength)
                .WithMessage($"Message body must not exceed {Message.MessageBodyMaxLength} characters.");
        }
    }
}
