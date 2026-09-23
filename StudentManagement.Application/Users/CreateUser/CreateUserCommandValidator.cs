using FluentValidation;

namespace StudentManagement.Application.Users.CreateUser
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email Address is Required")
                .EmailAddress().WithMessage("A valid email address format is required");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is Required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("Role must be a valid, defined system user role.");


        }
    }
}
