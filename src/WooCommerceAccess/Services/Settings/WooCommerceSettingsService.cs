using System.Threading.Tasks;
using WooCommerceAccess.Configuration;
using WooCommerceAccess.Models;
using WooCommerceAccess.Shared;
using WooCommerceAccess.Throttling;

namespace WooCommerceAccess.Services.SystemStatus
{
	public class WooCommerceSettingsService : BaseService, IWooCommerceSettingsService
	{
		private readonly string _serviceUrl;

		public WooCommerceSettingsService( WooCommerceConfig config, Throttler throttler )
			: base( config, throttler )
		{
			_serviceUrl = base.WCObject.SettingsApiUrl;
		}

		public async Task< WooCommerceSettings > GetSettingsAsync( Mark mark )
		{
			return await base.SendRequestAsync< WooCommerceSettings >( this._serviceUrl, mark, ( url, marker ) => 
			{ 
				return base.WCObject.GetSettingsAsync( url, marker );
			} ).ConfigureAwait( false );
		}
	}
}
