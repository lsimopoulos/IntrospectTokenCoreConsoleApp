using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.CommandLineUtils;

namespace AccessTokenValidationConsole
{
    public class Program
    {
        private static string IntrospectionPath { get; set; }
        private static string AccessToken { get; set; }
        private static string ScopeName { get; set; }

        private static string ScopePassword { get; set; }

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

            var identity = app.Command("identity", config =>
            {
                config.Description = "Validate token using IdentityServer 4's IntrospectionClient";
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
                    ParseArgs(config.Arguments.Select(x => x.Value).ToArray());
                    ValidateViaIntrospectClient().GetAwaiter().GetResult();

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
                config.Description = "Validate token using httpClient";
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
                    ParseArgs(config.Arguments.Select(x => x.Value).ToArray());
                    ValidateViaHttpClient().GetAwaiter().GetResult();

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

        private static void ParseArgs(string[] args)
        {
            if (ValidateIntrospectionPath(args[0]) && !IsHttpsAddress(args[0]))
            {
                IntrospectionPath = args[0];
            }
            else
            {
                Console.WriteLine("Make sure that the serverAuthorityAddress is valid and an HTTP address");
                Environment.Exit(1);
            }

            ScopeName = args[1];
            ScopePassword = args[2];
            AccessToken = args[3];
        }

        private static bool ValidateIntrospectionPath(string path)
        {
            return Uri.TryCreate(path, UriKind.Absolute, out Uri uri)
                   && uri.Scheme == "http";
        }

        private static bool IsHttpsAddress(string path)
        {
            return path.Contains("https://");
        }

        private static void ShowUsage()
        {
            Console.WriteLine("///////////////////////////////////////////////////////////////////////////////////");
            Console.WriteLine(" Usage 1 : identity  serverAuthorityAddress scopeName scopePassword accessToken");
            Console.WriteLine(" Usage 2 : httpclient serverAuthorityAddress scopeName scopePassword accessToken");
            Console.WriteLine("///////////////////////////////////////////////////////////////////////////////////");
        }


        private static async Task ValidateViaIntrospectClient()
        {
            var introspectionClient = new IntrospectionClient(
                IntrospectionPath,
                "api1",
                "secret");


            var response = await introspectionClient.SendAsync(new IntrospectionRequest
            {
                Token = AccessToken
            });
            PrintResultToConsole(response.IsError ? response.Error : response.Json.ToString());
        }

        private static async Task ValidateViaHttpClient()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.SetToken("Basic", GetAuthenticationString(ScopeName, ScopePassword));

                var form = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("token", AccessToken)
                });


                var response = await httpClient.PostAsync(IntrospectionPath, form);
                var result = await response.Content.ReadAsStringAsync();
                var introspectionResponse = new IntrospectionResponse(result);
                PrintResultToConsole(introspectionResponse.Json.ToString());
            }
        }

        private static void PrintResultToConsole(string result)
        {
            Console.WriteLine();
            Console.WriteLine("Result:");
            Console.WriteLine();
            Console.WriteLine(result);
        }

        private static string GetAuthenticationString(string scopeName, string plainPassword)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{scopeName}:{plainPassword}"));
        }
    }
}