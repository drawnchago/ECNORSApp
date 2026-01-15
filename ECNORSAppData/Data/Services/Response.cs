using System;
using System.Collections.Generic;
using System.Text;

namespace ECNORSAppData.Data.Services
{
    public sealed class Response<T>
    {
        public bool succes { get; set; }
        public string? message { get; set; }
        public T? data { get; set; }
    }
}
