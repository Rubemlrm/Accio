﻿using System;
using System.Collections.Generic;

namespace HpTcgCardBrowser.Data
{
    public partial class RulingType
    {
        public Guid RulingTypeId { get; set; }
        public string Name { get; set; }
        public Guid CreatedById { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid UpdatedById { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool Deleted { get; set; }
    }
}