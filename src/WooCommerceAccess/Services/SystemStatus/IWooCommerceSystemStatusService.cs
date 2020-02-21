using System.Threading.Tasks;

namespace WooCommerceAccess.Services.SystemStatus
{
	public interface IWooCommerceSystemStatusService
	{
		Task< string > GetStoreVersion();
	}
}
