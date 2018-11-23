using System;
using System.Collections.Generic;
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using WMIT.SqlTest.Models;
using WMIT.SqlTest.Services;

namespace WMIT.SqlTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "SqlTest test runner";
            app.Description = "A test runner for sql databases.";
            app.HelpOption("-?|-h|--help");

            app.Command("run", runConfig =>
            {
                var testFileArgument = runConfig.Argument("test file", "One or more files containing test cases", false);

                runConfig.OnExecute(() =>
                {
                    var testFile = JsonConvert.DeserializeObject<SqlTestFile>(testFileArgument.Value);

                    try
                    {
                        var testRunner = new SqlTestRunner();
                        var testResults = testRunner.Run(testFile);
                        PrintTestResults(testResults);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return 1;
                    }

                    return 0;
                });
            });
        }

        private static void PrintTestResults(List<SqlTestResult> testResults)
        {
            if (testResults.TrueForAll(r => r.IsSuccess))
            {
                Console.WriteLine("Tests succeeded.");
            }
            else
            {
                Console.WriteLine("Some tests failed.");
            }
        }
    }
}
