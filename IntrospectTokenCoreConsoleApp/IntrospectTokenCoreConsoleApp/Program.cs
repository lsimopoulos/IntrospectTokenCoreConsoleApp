using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.Extensions.CommandLineUtils;

namespace IntrospectTokenCoreConsoleApp
{
    public class Program
    {
        private static string IntrospectionPath { get; set; }
        private static string ServerAuthorityAddress { get; set; }
        private static string AccessToken { get; set; }
        private static string ScopeName { get; set; }
        private static string ScopePassword { get; set; }

        public static string ClientId { get; set; }
        public static string UserId { get; set; }

        public static int ExpireIn { get; set; }

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
                    ParseExtraArgsForCustomvalidation(config.Arguments.Select(x => x.Value).ToArray());
                    CustomValidation();

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
                    ParseArgs(config.Arguments.Select(x => x.Value).ToArray());
                    var result = ValidateViaIntrospectClient().GetAwaiter().GetResult();
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
                    ParseArgs(config.Arguments.Select(x => x.Value).ToArray());
                    var result = IntrospectAccessTokenViaHttpClient().GetAwaiter().GetResult();
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
        private static void ParseArgs(string[] args)
        {
            if (ValidateIntrospectionPath(args[0]) && !IsHttpsAddress(args[0]))
            {
                ServerAuthorityAddress = args[0];
                IntrospectionPath = args[0] + "/connect/introspect";
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

        /// <summary>
        ///     Parses the extra arguments that are needed for the custom validation.
        /// </summary>
        /// <param name="args"></param>
        private static void ParseExtraArgsForCustomvalidation(string[] args)
        {
            ParseArgs(args);
            UserId = args[4];
            ExpireIn = int.Parse(args[5]);
            ClientId = args[6];
        }

        /// <summary>
        ///     Validates the path string.
        /// </summary>
        /// <param name="path"></param>
        private static bool ValidateIntrospectionPath(string path)
        {
            return Uri.TryCreate(path, UriKind.Absolute, out Uri uri)
                   && uri.Scheme == "http";
        }

        /// <summary>
        ///     Checks if the address is https.
        /// </summary>
        /// <param name="path"></param>
        private static bool IsHttpsAddress(string path)
        {
            return path.Contains("https://");
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
        ///     Introspection of access token using Identity Server 4's Introspect Client to connect to the introspection endpoint.
        /// </summary>
        private static async Task<IntrospectionResponse> ValidateViaIntrospectClient()
        {
            var introspectionClient = new IntrospectionClient(
                IntrospectionPath,
                "api1",
                "secret");


            return await introspectionClient.SendAsync(new IntrospectionRequest
            {
                Token = AccessToken
            });
        }
        /// <summary>
        /// Validates the token with custom logic.
        /// </summary>
        private static void CustomValidation()
        {
            var jwt = new JwtSecurityToken(AccessToken);
            IList<string> errors = new List<string>();

            if (!VerifyIssuer(jwt.Issuer))
            {
                errors.Add($"Mismatch of issuer. Expected: {ServerAuthorityAddress} but found : {jwt.Issuer}");
            }

            var authTime = long.Parse(jwt.Claims.First(x => x.Type == "auth_time").Value).ToDateTimeFromEpoch();

            if (!jwt.Audiences.Contains(ScopeName))
                errors.Add($"The expected scope {ScopeName} was not found on the given token");

            if (jwt.ValidTo < DateTime.Now)
                errors.Add($"The token is expired.  exp : {jwt.ValidTo}");

            if (jwt.ValidTo.Subtract(authTime) != jwt.ValidTo.Subtract(jwt.ValidFrom))
                errors.Add($"The auth time of the token ({authTime} is not the same as nbf value : {jwt.ValidFrom}");

            if (jwt.ValidTo > DateTime.Now && jwt.ValidTo - jwt.ValidFrom != new TimeSpan(0, 0, ExpireIn))
                errors.Add(
                    $"The token should expire in : {ExpireIn} seconds but it expires in {(jwt.ValidTo - jwt.ValidFrom).Seconds}");

            if (!jwt.Claims.First(x => x.Type == "sub").Value.Equals(UserId))
                errors.Add(
                    $"Expected value of the sub : {UserId} but the token contained : {jwt.Claims.First(x => x.Type == "sub").Value}");

            if (jwt.Claims.Where(x => x.Type == "scope").All(y => y.Value != ScopeName))
                errors.Add($"The token does not contain the expected scope: {ScopeName}");

            if (!jwt.Claims.First(x => x.Type == "client_id").Value.Equals(ClientId))
                errors.Add($"The token does not contain the expected client_id: {ClientId}");

            if (errors.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("//////////////////////////////////////");
                foreach (var error in errors)
                    Console.WriteLine(error);
                Console.WriteLine("//////////////////////////////////////");
            }
            else
            {
                Console.WriteLine("The token for the given scope is valid.");
            }
        }

        /// <summary>
        ///     Verify if the issuer of the token is the same  as
        /// </summary>
        /// <param name="issuer"></param>
        private static bool VerifyIssuer(string issuer)
        {
            return issuer.Equals(ServerAuthorityAddress);
        }

        /// <summary>
        ///     Introspection of accesstoken using httpClient to connect to the introspection endpoint.
        /// </summary>
        private static async Task<IntrospectionResponse> IntrospectAccessTokenViaHttpClient()
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
                return new IntrospectionResponse(result);
            }
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
        ///     Returns a base 64 string that is containing the scope name and scope password.
        /// </summary>
        /// <param name="scopeName"></param>
        /// <param name="plainPassword"></param>
        private static string GetAuthenticationString(string scopeName, string plainPassword)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{scopeName}:{plainPassword}"));
        }
    }
}