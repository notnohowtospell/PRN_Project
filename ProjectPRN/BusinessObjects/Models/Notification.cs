using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public int? StudentId { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Student? Student { get; set; }
}
