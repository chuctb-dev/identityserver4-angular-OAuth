using IdentityServer4.Models;

namespace AuthenticationServer.Core.Configs
{
    public static class AuthenticationConfigs
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return
            [
                new IdentityResources.OpenId(),
                new IdentityResources.Email(),
                new IdentityResources.Profile(),
            ];
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return
            [
                new ApiResource("sampleapi", "Sample Api Resource")
                {
                    Scopes = ["api.read"]
                }
            ];
        }

        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return
            [
                new ApiScope("api.read", "Sample Api Scope")
            ];
        }

        public static IEnumerable<Client> GetClients()
        {
            return
            [
                new Client
                {
                    RequireConsent = false,
                    ClientId = "sample_api_swagger",
                    ClientName = "Sample Swagger Client",
                    ClientSecrets = {new Secret("secret".Sha256())},
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    RedirectUris = {"http://localhost:5001/swagger/oauth2-redirect.html"},
                    AllowedCorsOrigins = {"http://localhost:5001"},
                    AllowedScopes = { "api.read" }
                },
                new Client {
                    RequireConsent = false,
                    ClientId = "angular_spa",
                    ClientName = "Angular SPA",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowedScopes = { "openid", "profile", "email", "api.read" },
                    RedirectUris = {"http://localhost:4200/auth-callback"},
                    PostLogoutRedirectUris = {"http://localhost:4200/"},
                    AllowedCorsOrigins = {"http://localhost:4200"},
                    AllowAccessTokensViaBrowser = true,
                    AccessTokenLifetime = 3600
                }
            ];
        }
    }
}
