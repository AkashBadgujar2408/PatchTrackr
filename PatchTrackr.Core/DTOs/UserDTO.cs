using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace PatchTrackr.Core.DTOs;

public class UserDTO
{
    public Guid? UserId { get; set; }

    [Required(ErrorMessage = "{0} is required.")]
    [DisplayName("User Name")]
    [StringLength(15, MinimumLength = 5, ErrorMessage = "Length of {0} should be between {2} and {1} characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "{0} can only contain alpha-numeric characters (A-Z, a-z, 0-9) and underscores (_).")]
    [Remote(action: "ValidateUserName", controller: "RemoteValidations", AdditionalFields = nameof(UserId))]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "{0} is required.")]
    [DisplayName("Password")]
    [StringLength(15, MinimumLength = 5, ErrorMessage = "Length of {0} should be between {2} and {1} characters.")]
    //[DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "{0} is required.")]
    [DisplayName("Full Name")]
    [StringLength(50, MinimumLength = 5, ErrorMessage = "Length of {0} should be between {2} and {1} characters.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "{0} is required.")]
    [DisplayName("Email")]
    [StringLength(50, MinimumLength = 5, ErrorMessage = "Length of {0} should be between {2} and {1} characters.")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = null!;

    [DisplayName("Phone No.")]
    [StringLength(10, MinimumLength = 8, ErrorMessage = "Please enter valid {0}.")]
    [DataType(DataType.PhoneNumber)]
    public string? PhoneNo { get; set; }

    [DisplayName("Active")]
    public bool IsActive { get; set; }
}
