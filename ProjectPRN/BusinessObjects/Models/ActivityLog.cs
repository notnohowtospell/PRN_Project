using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class ActivityLog
{
    public int LogId { get; set; }

    public int? UserId { get; set; }

    public string Action { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public virtual Student? User { get; set; }
}
