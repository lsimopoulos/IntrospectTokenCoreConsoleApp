namespace IntrospectTokenCoreConsoleApp
{
    public  class IntrospectionTokenModel
    {
        public  string IntrospectionPath { get; set; }
        public  string ServerAuthorityAddress { get; set; }
        public  string AccessToken { get; set; }
        public  string ScopeName { get; set; }
        public  string ScopePassword { get; set; }

        public  string ClientId { get; set; }
        public  string UserId { get; set; }

        public  int ExpireIn { get; set; }
    }
}
