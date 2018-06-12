using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace IntrospectTokenCoreConsoleApp
{
    public static class IdentityClientIntrospection
    {
        /// <summary>
        ///     Introspection of access token using Identity Server 4's Introspect Client to connect to the introspection endpoint.
        /// </summary>
        public static async Task<IntrospectionResponse> OfAccessToken(IntrospectionTokenModel itm)
        {
            var introspectionClient = new IntrospectionClient(
            itm.IntrospectionPath,
                itm.ScopeName,
                itm.ScopePassword
                );


            return await introspectionClient.SendAsync(new IntrospectionRequest
            {
                Token = itm.AccessToken
            });
        }
      
    }
}
