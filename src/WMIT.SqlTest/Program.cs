using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.FileSystemGlobbing;
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
        const string JSON_PATTERN = "-j|--json";
        const string SCHEMA_PATTERN = "-s|--schema";
        const string USER_PATTERN = "-u|--user";
        const string PASSWORD_PATTERN = "-p|--password";

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
            app.Option("-j|--json", "Outputs test results as json", CommandOptionType.NoValue);
            app.Option("-s|--schema", "Specifies the used schema for the test-procedures", CommandOptionType.SingleValue);
            app.Option("-u|--user", "Specifies the used user for the test-procedures", CommandOptionType.SingleValue);
            app.Option("-p|--password", "Specifies the used schema for the test-procedures", CommandOptionType.SingleValue);

            app.Command("run", runConfig =>
            {
                runConfig.HelpOption(HELP_PATTERN);

                var testFileArgument = runConfig.Argument("test file", "One or more files containing test cases", false);
                var jsonOption = runConfig.Option("-j|--json", "Outputs test results as json", CommandOptionType.NoValue);
                var schemaOption = runConfig.Option("-s|--schema", "Specifies the used schema for the test-procedures", CommandOptionType.SingleValue);
                var userOption = runConfig.Option("-u|--user", "Specifies the used user for the test-procedures", CommandOptionType.SingleValue);
                var passwordOption = runConfig.Option("-p|--password", "Specifies the used schema for the test-procedures", CommandOptionType.SingleValue);

                runConfig.OnExecute(async () =>
                {
                    if (jsonOption.HasValue())
                    {
                        logger = Serilog.Core.Logger.None;
                    }

                    var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
                    matcher.AddInclude(testFileArgument.Value);
                    var files = matcher.GetResultsInFullPath(Environment.CurrentDirectory);

                    try
                    {
                        var testRunner = new SqlTestRunner(logger);
                        var allResults = new List<SqlTestResult>();

                        foreach (var file in files)
                        {
                            logger.Information("Executing test file {FileName}", file);
                            var testFileContents = File.ReadAllText(file);
                            var testFile = JsonConvert.DeserializeObject<SqlTestFile>(testFileContents, serializerSettings);

                            var schema = "";

                            if (schemaOption.HasValue())
                            {
                                schema = schemaOption.Value() + ".";
                            }

                            var user = "";

                            if (userOption.HasValue())
                            {
                                user = userOption.Value();
                            }

                            var password = "";

                            if (passwordOption.HasValue())
                            {
                                password = passwordOption.Value();
                            }

                            var testResults = await testRunner.Run(testFile, schema, user, password);
                            allResults.AddRange(testResults);
                        }

                        if (!jsonOption.HasValue())
                        {
                            PrintTestResults(allResults, logger);
                        }
                        else
                        {
                            Console.WriteLine(JsonConvert.SerializeObject(allResults, serializerSettings));
                        }

                        var isSuccess = allResults.TrueForAll(r => r.IsSuccess);
                        return isSuccess ? 0 : 1;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "An error occured while executing the test case");
                        return 1;
                    }

                });
            });

            if (args.Length == 0)
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
