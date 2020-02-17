using System;
using System.Threading.Tasks;
using WooCommerceAccess.Configuration;
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

		public async Task< string > GetStoreVersion()
		{
			return await base.SendRequestAsync< string >( this._serviceUrl, ( url, mark ) => 
			{ 
				return base.WCObject.GetStoreVersion();
			} ).ConfigureAwait( false );
		}
	}
}
