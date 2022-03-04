using System.Threading.Tasks;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Shared;
using WooCommerceAccess.Throttling;

namespace WooCommerceAccess.Services.SystemStatus
{
	public class WooCommerceSystemStatusService : BaseService, IWooCommerceSystemStatusService
	{
		private readonly string _serviceUrl;

		public WooCommerceSystemStatusService( WooCommerceConfig config, Throttler throttler )
			: base( config, throttler )
		{
			_serviceUrl = base.WCObject.SystemStatusApiUrl;
		}

		public async Task< string > GetStoreVersionAsync( Mark mark )
		{
			return await base.SendRequestAsync< string >( this._serviceUrl, mark, ( url, marker ) => 
			{ 
				return base.WCObject.GetStoreVersionAsync( url, marker );
			} ).ConfigureAwait( false );
		}
	}
}
