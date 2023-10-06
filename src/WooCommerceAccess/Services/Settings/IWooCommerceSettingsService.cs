using System.Threading.Tasks;
using WooCommerceAccess.Models;
using WooCommerceAccess.Shared;

namespace WooCommerceAccess.Services.SystemStatus
{
	public interface IWooCommerceSettingsService
	{
		/// <summary>
		/// Retrieve WooCommerce shop level settings
		/// </summary>
		/// <param name="mark"></param>
		/// <returns></returns>
		Task< WooCommerceSettings > GetSettingsAsync( Mark mark );
	}
}
