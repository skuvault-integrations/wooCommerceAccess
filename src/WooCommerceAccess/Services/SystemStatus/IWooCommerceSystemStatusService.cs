using System.Threading.Tasks;
using WooCommerceAccess.Shared;

namespace WooCommerceAccess.Services.SystemStatus
{
	public interface IWooCommerceSystemStatusService
	{
		Task< string > GetStoreVersionAsync( Mark mark );
	}
}
