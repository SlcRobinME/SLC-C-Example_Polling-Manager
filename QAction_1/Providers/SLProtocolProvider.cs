namespace Skyline.PollingManager.Providers
{
	using Skyline.DataMiner.Scripting;

	public class SLProtocolProvider
	{
		public SLProtocolProvider(SLProtocolExt protocol)
		{
			Protocol = protocol;
		}

		public static SLProtocolExt Protocol { get; set; }
	}
}
