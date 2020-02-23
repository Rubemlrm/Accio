﻿using System;
using System.Collections.Generic;
using System.Text;

namespace HpTcgCardBrowser.Business.Models.CardModels
{
    public class CardRarityModel
    {
        public Guid CardRarityId { get; set; }
        public string Name { get; set; }
        public Guid CreatedById { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid UpdatedById { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool Deleted { get; set; }
    }
}