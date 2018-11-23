using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace WMIT.SqlTest.Models
{
    public class SqlTestFile 
    {
        public string ConnectionString { get; set; }
        public List<SqlTestCase> Tests { get; set; } = new List<SqlTestCase>();
    }
}
