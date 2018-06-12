using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using IdentityModel;

namespace IntrospectTokenCoreConsoleApp
{
    public static class Validation
    {
        /// <summary>
        /// Validates the token with custom logic.
        /// </summary>
        public static void Manual(IntrospectionTokenModel itm)
        {
            var jwt = new JwtSecurityToken(itm.AccessToken);
            IList<string> errors = new List<string>();

            if (!VerifyIssuer(jwt.Issuer,itm.ServerAuthorityAddress))
            {
                errors.Add($"Mismatch of issuer. Expected: {itm.ServerAuthorityAddress} but found : {jwt.Issuer}");
            }

            var authTime = int.Parse(jwt.Claims.First(x => x.Type == "auth_time").Value).ToDateTimeFromEpoch();

            if (jwt.ValidTo < DateTime.Now)
                errors.Add($"The token is expired.  exp : {jwt.ValidTo}");

            if (jwt.ValidTo.Subtract(authTime) != jwt.ValidTo.Subtract(jwt.ValidFrom))
                errors.Add($"The auth time of the token ({authTime} is not the same as nbf value : {jwt.ValidFrom}");

            if (jwt.ValidTo > DateTime.Now && jwt.ValidTo - jwt.ValidFrom != new TimeSpan(0, 0, itm.ExpireIn))
                errors.Add(
                    $"The token should expire in : {itm.ExpireIn} seconds but it expires in {(jwt.ValidTo - jwt.ValidFrom).Seconds}");

            if (!jwt.Claims.First(x => x.Type == "sub").Value.Equals(itm.UserId))
                errors.Add(
                    $"Expected value of the sub : {itm.UserId} but the token contained : {jwt.Claims.First(x => x.Type == "sub").Value}");

            if (jwt.Claims.Where(x => x.Type == "scope").All(y => y.Value != itm.ScopeName))
                errors.Add($"The token does not contain the expected scope: {itm.ScopeName}");

            if (!jwt.Claims.First(x => x.Type == "client_id").Value.Equals(itm.ClientId))
                errors.Add($"The token does not contain the expected client_id: {itm.ClientId}");

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
        ///     Verify if the issuer of the token is the same  as the desired authority.
        /// </summary>
        /// <param name="issuer"></param>
        /// <param name="authorityAddress"></param>
        private static bool VerifyIssuer(string issuer, string authorityAddress)
        {
            return issuer.Equals(authorityAddress);
        }
    }
}
