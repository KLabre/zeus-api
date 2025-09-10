namespace Zeus.Api.Models
{
    public static class AppConstants
    {
        public const string ZeusSpecificOrigins = "Origins";

        public static class AppCache
        {
            public const string BlacklistedTokenKey = "BlackListedTokenList-{0}";
            public const int BlackListedTokenExpiryInMinutes = 60;
            public const int NumberOfRequestLimit = 1000;
        }
    }
}
