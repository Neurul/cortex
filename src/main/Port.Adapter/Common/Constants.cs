using System;

namespace org.neurul.Cortex.Port.Adapter.Common
{
    public struct EnvironmentVariableKeys
    {
        public const string DatabasePath = "DATABASE_PATH";
        public const string IdentityAccessDatabasePath = "IDENTITY_ACCESS_DATABASE_PATH";
        public const string RequireAuthentication = "REQUIRE_AUTHENTICATION";
        public const string TokenIssuerAddress = "TOKEN_ISSUER_ADDRESS";
    }
}
