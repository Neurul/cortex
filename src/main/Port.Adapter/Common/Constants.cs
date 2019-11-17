using System;

namespace org.neurul.Cortex.Port.Adapter.Common
{
    public struct EnvironmentVariableKeys
    {
        public const string DatabasePath = "DATABASE_PATH";
        public const string UserDatabasePath = "USER_DATABASE_PATH";
        public const string RequireAuthentication = "REQUIRE_AUTHENTICATION";
        public const string TestUserSubjectId = "TEST_USER_SUBJECT_ID";
        public const string TokenIssuerAddress = "TOKEN_ISSUER_ADDRESS";
    }
}
