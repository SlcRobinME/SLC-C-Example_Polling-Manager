namespace Skyline.PollingManager.Providers
{
	using Skyline.DataMiner.Scripting;

	public class SLProtocolProvider
	{
		public SLProtocolProvider(SLProtocol protocol)
		{
			Protocol = protocol;
		}

		public static SLProtocol Protocol { get; set; }
	}
}
