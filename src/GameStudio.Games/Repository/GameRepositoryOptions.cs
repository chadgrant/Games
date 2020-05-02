namespace GameStudio.Games.Repository
{
	public class GameRepositoryOptions
	{
		public int Retries { get; set; } = 3;
		public int TimeoutStepMilliseconds { get; set; } = 500;
		public int ExceptionsBeforeBreaking { get; set; } = 3;
		public int DurationOfBreakMilliseconds { get; set; } = 5000;
		public int MemoryCacheDurationMinutes { get; set; } = 5;
	}
}
