﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Go1Bet.Core.DTO_s.User
{
    public class ConfirmationEmailDTO
    {
        public string userId { get; set; }
        public string token { get; set; }
    }
}
