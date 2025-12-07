using System;
using System.Collections.Generic;

namespace PatchTrackr.Core.Models;

public partial class MForm
{
    public int FormId { get; set; }

    public string? FormName { get; set; }

    public string? FormTitle { get; set; }

    public string? ControllerName { get; set; }

    public string? ActionName { get; set; }

    public int? ParentFormId { get; set; }

    public int? SortOrder { get; set; }

    public bool? IsViewApplicable { get; set; }

    public bool? IsAddApplicable { get; set; }

    public bool? IsUpdateApplicable { get; set; }

    public bool? IsDeleteApplicable { get; set; }

    public bool IsActive { get; set; }
}
