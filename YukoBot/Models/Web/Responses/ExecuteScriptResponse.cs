﻿using System.Collections.Generic;

namespace YukoBot.Models.Web.Responses
{
    public class ExecuteScriptResponse : Response
    {
        public bool Next { get; set; }
        public List<string> Urls { get; set; } = new List<string>();
    }
}
