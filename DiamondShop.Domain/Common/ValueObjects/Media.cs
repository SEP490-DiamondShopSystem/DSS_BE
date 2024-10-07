﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Domain.Common.ValueObjects
{
    public class Media
    {
        public string? MediaName { get; set; }
        public string MediaPath { get; set; }
        public string ContentType { get; set; } 
    }
}