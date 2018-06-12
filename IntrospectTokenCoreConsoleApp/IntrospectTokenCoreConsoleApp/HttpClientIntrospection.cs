using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace IntrospectTokenCoreConsoleApp
{
    public static class HttpClientIntrospection
    {
        /// <summary>
        ///     Introspection of accesstoken using httpClient to connect to the introspection endpoint.
        /// </summary>
        public static async Task<IntrospectionResponse> OfAccessToken(IntrospectionTokenModel itm)
        {


            using (var httpClient = new HttpClient())
            {
                httpClient.SetToken("Basic", GetAuthenticationString(itm.ScopeName, itm.ScopePassword));

                var form = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("token", itm.AccessToken)
                });


                var response = await httpClient.PostAsync(itm.IntrospectionPath, form);
                var result = await response.Content.ReadAsStringAsync();
                return new IntrospectionResponse(result);
            }
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
