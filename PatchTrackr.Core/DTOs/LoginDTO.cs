
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PatchTrackr.Core.DTOs;

public class LoginDTO
{
    [Required(ErrorMessage = "{0} is required.")]
    [DisplayName("User ID")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "{0} is required.")]
    [DisplayName("Password")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    public string? ReturnUrl { get; set; }
}
