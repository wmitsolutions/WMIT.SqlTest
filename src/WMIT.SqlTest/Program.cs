using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using WMIT.SqlTest.Models;
using WMIT.SqlTest.Services;

namespace WMIT.SqlTest
{
    class Program
    {
        const string HELP_PATTERN = "-?|-h|--help";

        static int Main(string[] args)
        {
            var loggerConfiguration = new LoggerConfiguration();
            loggerConfiguration.WriteTo.Console(Serilog.Events.LogEventLevel.Debug,
                theme: AnsiConsoleTheme.Literate,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

            var logger = (ILogger)loggerConfiguration.CreateLogger();

            var serializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };

            var app = new CommandLineApplication
            {
                Name = "sqltest",
                Description = "A test runner for sql databases."
            };

            app.HelpOption(HELP_PATTERN);

            app.Command("run", runConfig =>
            {
                runConfig.HelpOption(HELP_PATTERN);

                var testFileArgument = runConfig.Argument("test file", "One or more files containing test cases", false);
                var jsonOption = runConfig.Option("-j|--json", "Outputs test results as json", CommandOptionType.NoValue);

                runConfig.OnExecute(async () =>
                {
                    if (jsonOption.HasValue())
                    {
                        logger = Serilog.Core.Logger.None;
                    }

                    var testFileContents = File.ReadAllText(testFileArgument.Value);
                    var testFile = JsonConvert.DeserializeObject<SqlTestFile>(testFileContents, serializerSettings);

                    try
                    {
                        var testRunner = new SqlTestRunner(logger);
                        var testResults = await testRunner.Run(testFile);

                        if (!jsonOption.HasValue())
                        {
                            PrintTestResults(testResults, logger);
                        }
                        else
                        {
                            Console.WriteLine(JsonConvert.SerializeObject(testResults, serializerSettings));
                        }

                        var isSuccess = testResults.TrueForAll(r => r.IsSuccess);
                        return isSuccess ? 0 : 1;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "An error occured while executing the test case");
                        return 1;
                    }

                });
            });

            if(args.Length == 0)
            {
                app.ShowHelp();
            }

            return app.Execute(args);
        }

        private static void PrintTestResults(List<SqlTestResult> testResults, ILogger logger)
        {
            if (testResults.TrueForAll(r => r.IsSuccess))
            {
                logger.Information("=> All tests executed successfully.");
            }
            else
            {
                logger.Error("=> Some tests failed.");
            }
        }
    }
}
