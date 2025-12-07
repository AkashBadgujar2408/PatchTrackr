using System;
using System.Collections.Generic;

namespace PatchTrackr.Core.Models;

public partial class MUserRight
{
    public int UserRightId { get; set; }

    public string UserName { get; set; } = null!;

    public int FormId { get; set; }

    public bool? ViewRight { get; set; }

    public bool? AddRight { get; set; }

    public bool? UpdateRight { get; set; }

    public bool? DeleteRight { get; set; }

    public bool IsActive { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? CreatedIp { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public string? UpdatedIp { get; set; }
}
