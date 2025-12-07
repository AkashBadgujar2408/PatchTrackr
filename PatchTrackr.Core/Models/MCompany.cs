using System;
using System.Collections.Generic;

namespace PatchTrackr.Core.Models;

public partial class MCompany
{
    public Guid CompanyId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string CompanyEmail { get; set; } = null!;

    public string CompanyWebsite { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string AddressLine1 { get; set; } = null!;

    public string AddressLine2 { get; set; } = null!;

    public string AddressLine3 { get; set; } = null!;

    public bool IsActive { get; set; }
}
