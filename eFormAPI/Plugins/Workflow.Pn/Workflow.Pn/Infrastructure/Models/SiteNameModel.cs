﻿using System;

namespace Workflow.Pn.Infrastructure.Models
{
    public class SiteNameModel
    {
        public int SiteUId { get; set; }
        public string SiteName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
