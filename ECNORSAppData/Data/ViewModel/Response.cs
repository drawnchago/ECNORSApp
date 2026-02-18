using System;
using System.Collections.Generic;
using System.Text;

namespace ECNORSApp.Data.DTO;

    public sealed class Response<T>
    {
        public bool succes { get; set; }
        public string? message { get; set; }
        public T? data { get; set; }
    }
