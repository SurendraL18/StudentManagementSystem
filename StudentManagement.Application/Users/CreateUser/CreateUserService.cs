using FluentValidation;
using StudentManagement.Application.Common.Exceptions;
using StudentManagement.Application.Common.Interfaces;
using StudentManagement.Domain.Entities;

namespace StudentManagement.Application.Users.CreateUser
{
    public class CreateUserService
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUserStore _userStore;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateUserCommand> _validator;


        public CreateUserService(IPasswordHasher passwordHasher, IUserStore userStore, IUnitOfWork unitOfWork, IValidator<CreateUserCommand> validator)
        {
            _passwordHasher = passwordHasher;
            _userStore = userStore;
            _unitOfWork = unitOfWork;
            _validator = validator;
        }


        public async Task<CreateUserResponse> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
        {
            await _validator.ValidateAndThrowAsync(command, cancellationToken);
            var existingUser = await _userStore.GetByEmailAsync(command.Email, cancellationToken);
            if (existingUser != null)
            {
                throw new DuplicateUserEmailException(command.Email);
            }

            string securedPasswordHash = _passwordHasher.Hash(command.Password);

            var user = new User(
             command.Email,
             securedPasswordHash,
             command.Role);

            await _userStore.AddAsync(user, cancellationToken);


            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex) when (ex.InnerException?.Message.Contains("IX_Users_Email") == true ||
                                       ex.Message.Contains("IX_Users_Email") == true ||
                                       ex.InnerException?.Message.Contains("unique constraint") == true)
            {

                throw new DuplicateUserEmailException(command.Email);
            }

            return new CreateUserResponse(
              user.Id,
              user.Email,
              user.Role,
              user.CreatedAtUtc);
        }

    }
}
