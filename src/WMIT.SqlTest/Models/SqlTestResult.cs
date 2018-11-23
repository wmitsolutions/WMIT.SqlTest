using System;

namespace WMIT.SqlTest.Models
{
    public class SqlTestResult
    {
        public bool IsSuccess { get; set; }
        public string Proc { get; set; }
        public Exception Exception { get; set; }
    }
}
