using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Serilog;
using WMIT.SqlTest.Models;

namespace WMIT.SqlTest.Services
{
    public class SqlTestRunner
    {
        private readonly ILogger _logger;

        public SqlTestRunner(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<List<SqlTestResult>> Run(SqlTestFile testFile, string schema)
        {
            var connection = new SqlConnection(testFile.ConnectionString);
            await connection.OpenAsync();

            var testResults = new List<SqlTestResult>();
            var counter = 1;

            foreach (var test in testFile.Tests)
            {
                test.Proc = schema + test.Proc;

                _logger.Information($"Executing test {counter++}/{testFile.Tests.Count}: {test.Proc}...");
                var testResult = await RunTestCase(connection, test);

                if (testResult.IsSuccess)
                {
                    _logger.Information($"=> Success");
                }
                else
                {
                    _logger.Error($"=> Failed: {testResult.Exception.Message}");
                }

                testResults.Add(testResult);
            }

            connection.Close();
            return testResults;
        }

        private async Task<SqlTestResult> RunTestCase(SqlConnection connection, SqlTestCase test)
        {
            var testResult = new SqlTestResult()
            {
                Proc = test.Proc
            };

            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = test.Proc;
                    command.CommandType = CommandType.StoredProcedure;

                    SqlParameter retValue = command.Parameters.Add("return", SqlDbType.Int);
                    retValue.Direction = ParameterDirection.ReturnValue;
                    await command.ExecuteNonQueryAsync();

                    testResult.IsSuccess = ((int)retValue.Value == 0);
                }
            }
            catch (Exception ex)
            {
                testResult.IsSuccess = false;
                testResult.Exception = ex;
            }

            return testResult;
        }
    }
}
