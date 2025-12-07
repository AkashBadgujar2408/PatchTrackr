using System;
using System.Collections.Generic;

namespace PatchTrackr.Core.Models;

public partial class MProject
{
    public Guid ProjectId { get; set; }

    public string ProjectCode { get; set; } = null!;

    public string ProjectName { get; set; } = null!;

    public string? ProjectDesc { get; set; }

    public bool IsActive { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? CreatedIp { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public string? UpdatedIp { get; set; }
}
