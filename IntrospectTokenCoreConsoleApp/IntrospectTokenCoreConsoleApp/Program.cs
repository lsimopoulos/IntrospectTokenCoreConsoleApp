using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;

namespace IntrospectTokenCoreConsoleApp
{
    public static class Program
    {


        public static void Main(string[] args)
        {
            var app = new CommandLineApplication();

            // ReSharper disable once UnusedVariable
            var help = app.Command("help", config =>
            {
                config.Description = "list of available commands and the syntax should be used";

                config.OnExecute(() =>
                {
                    ShowUsage();

                    return 1;
                });
            });

            var validate = app.Command("validate", config =>
            {
                config.Description = "Validate token";
                config.HelpOption("-? | -h | --help");
                // ReSharper disable once UnusedVariable
                var arguments = new List<CommandArgument>
                {
                    config.Argument("serverAuthorityAddress", "the address of the authority server"),
                    config.Argument("scopeName", "the name of the scope"),
                    config.Argument("scopePassword", "the password of the scope"),
                    config.Argument("accessToken", "the accesstoken to be validated"),
                    config.Argument("userId", "the  user id"),
                    config.Argument("expiresIn", "the expire period in s"),
                    config.Argument("clientId", "the client id")
                };
                config.OnExecute(() =>
                {
                    var itm = ParseExtraArgsForCustomvalidation(config.Arguments.Select(x => x.Value).ToArray());
                    Validation.Manual(itm);

                    return 1;
                });
                config.HelpOption("-? | -h | --help");
            });
            validate.Command("help", config =>
            {
                config.Description = "list the arguments";
                config.OnExecute(() =>
                {
                    validate.ShowHelp("identity");
                    return 1;
                });
            });

            var identity = app.Command("identity", config =>
            {
                config.Description =
                    "Show the result from the introspect endpoint using IdentityServer 4's IntrospectionClient";
                config.HelpOption("-? | -h | --help");
                // ReSharper disable once UnusedVariable
                var arguments = new List<CommandArgument>
                {
                    config.Argument("serverAuthorityAddress", "the address of the authority server"),
                    config.Argument("scopeName", "the name of the scope"),
                    config.Argument("scopePassword", "the password of the scope"),
                    config.Argument("accessToken", "the accesstoken to be validated")
                };
                config.OnExecute(() =>
                {
                    var itm = ParseArgs(config.Arguments.Select(x => x.Value).ToArray());
                    var result = IdentityClientIntrospection.OfAccessToken(itm).GetAwaiter().GetResult();
                    PrintResultToConsole(result.IsError ? result.Error : result.Json.ToString());
                    return 1;
                });
                config.HelpOption("-? | -h | --help");
            });
            identity.Command("help", config =>
            {
                config.Description = "list the arguments";
                config.OnExecute(() =>
                {
                    identity.ShowHelp("identity");
                    return 1;
                });
            });
            var http = app.Command("httpclient", config =>
            {
                config.Description = "Show the result from the introspect endpoint  using httpClient";
                config.HelpOption("-? | -h | --help");
                // ReSharper disable once UnusedVariable
                var arguments = new List<CommandArgument>
                {
                    config.Argument("serverAuthorityAddress", "the address of the authority server"),
                    config.Argument("scopeName", "the name of the scope"),
                    config.Argument("scopePassword", "the password of the scope"),
                    config.Argument("accessToken", "the accesstoken to be validated")
                };
                config.OnExecute(() =>
                {
                    var itm = ParseArgs(config.Arguments.Select(x => x.Value).ToArray());
                    var result = HttpClientIntrospection.OfAccessToken(itm).GetAwaiter().GetResult();
                    PrintResultToConsole(result.Json.ToString());
                    return 0;
                });


                config.HelpOption("-? | -h | --help");
            });

            http.Command("help", config =>
            {
                config.Description = "get help!";
                config.OnExecute(() =>
                {
                    http.ShowHelp("http");
                    return 1;
                });
            });

            app.HelpOption("-? | -h | --help");
            try
            {
                var result = app.Execute(args);
                Environment.Exit(result);
            }
            catch (Exception e)
            {
                Console.WriteLine("///////////////////////////////////////////////////////////");
                Console.WriteLine();
                Console.WriteLine($"Error : {e.Message}");
                Console.WriteLine();
                ShowUsage();
                Console.WriteLine();
                Console.WriteLine("///////////////////////////////////////////////////////////");
                Environment.Exit(1);
            }
        }

        /// <summary>
        ///     Parses the arguments.
        /// </summary>
        /// <param name="args"></param>
        private static IntrospectionTokenModel ParseArgs(string[] args)
        {
            //&& !IsHttpsAddress(args[0])
            var itm = new IntrospectionTokenModel();
            if (ValidateIntrospectionPath(args[0]) )
            {
                itm.ServerAuthorityAddress = args[0];
                itm.IntrospectionPath = args[0] + "/connect/introspect";
            }
            else
            {
                Console.WriteLine("Make sure that the serverAuthorityAddress is valid");
                Environment.Exit(1);
            }

            itm.ScopeName = args[1];
            itm.ScopePassword = args[2];
            itm.AccessToken = args[3];

            return itm;
        }

        /// <summary>
        ///     Parses the extra arguments that are needed for the custom validation.
        /// </summary>
        /// <param name="args"></param>
        private static IntrospectionTokenModel ParseExtraArgsForCustomvalidation(string[] args)
        {
            var itm = ParseArgs(args);
            itm.UserId = args[4];
            itm.ExpireIn = int.Parse(args[5]);
            itm.ClientId = args[6];
            return itm;
        }

      
        /// <summary>
        ///     Displays the usage.
        /// </summary>
        private static void ShowUsage()
        {
            Console.WriteLine("///////////////////////////////////////////////////////////////////////////////////");
            Console.WriteLine(" Usage 1 : identity  serverAuthorityAddress scopeName scopePassword accessToken");
            Console.WriteLine(" Usage 2 : httpclient serverAuthorityAddress scopeName scopePassword accessToken");
            Console.WriteLine(
                " Usage 3 : validate serverAuthorityAddress scopeName scopePassword accessToken userId expiresIn clientId");
            Console.WriteLine("///////////////////////////////////////////////////////////////////////////////////");
        }

        /// <summary>
        ///     Displays the result to the the console.
        /// </summary>
        /// <param name="result"></param>
        private static void PrintResultToConsole(string result)
        {
            Console.WriteLine();
            Console.WriteLine("Result:");
            Console.WriteLine();
            Console.WriteLine(result);
        }

        /// <summary>
        ///     Validates the path string.
        /// </summary>
        /// <param name="path"></param>
        private static bool ValidateIntrospectionPath(string path)
        {
            return Uri.TryCreate(path, UriKind.Absolute, out _);
        }

    }
}