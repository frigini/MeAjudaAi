using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Common;

namespace MeAjudaAi.Modules.Users.Application.Commands;

public class UpdateUserProfileCommand : ICommand<Result>
{
    public Guid CorrelationId { get; } = Guid.NewGuid();
    //public string Name { get; set; }
    //public string Email { get; set; }
    //public string PhoneNumber { get; set; }
    //public string Address { get; set; }
    //public string ProfilePictureUrl { get; set; }
    // Add any other properties needed for updating the user profile
}