namespace MyVote.Services.AppServer
{
	public static class Constants
	{
		public const string CorsPolicyName = "MyVotePolicy";
        public const string TokenIssuer = "MyVoteIssuer";
        public const string TokenAudience = "MyVoteAudience";

        //TODO: Move this to use Secret Manager
        public const string SecretKey = "KEY";
    }
}
