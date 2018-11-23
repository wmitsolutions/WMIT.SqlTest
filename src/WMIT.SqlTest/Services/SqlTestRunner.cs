using System.Collections.Generic;
using WMIT.SqlTest.Models;

namespace WMIT.SqlTest.Services
{
    public class SqlTestRunner
    {
        public List<SqlTestResult> Run(SqlTestFile testFile)
        {
            return new List<SqlTestResult>()
            {
                new SqlTestResult() { IsSuccess = true }
            };
        }
    }
}
