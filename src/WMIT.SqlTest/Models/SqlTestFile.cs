using System.Collections.Generic;

namespace WMIT.SqlTest.Models
{
    public class SqlTestFile 
    {
        public string ConnectionString { get; set; }
        public List<SqlTestCase> Tests { get; set; } = new List<SqlTestCase>();
    }
}
