using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using WooCommerceAccess.Services;
using WooCommerceNET;
using LegacyModels = WooCommerceNET.WooCommerce.Legacy;
// ReSharper disable StringLiteralTypo

namespace WooCommerceTests.Mappers.ResponseDeserialization.Legacy
{
	public class OrderListDeserializationTests
	{
		private readonly Randomizer _randomizer = new Randomizer();
		private RestAPI _restApi;

		[SetUp]
		public void Init()
		{
			_restApi = new RestAPI(url: ApiBasePath.V3, key: _randomizer.GetString(), secret: _randomizer.GetString());
		}

		[Test]
		[Description("This happens when WooCommerce AVATAX plug-in is used")]
		public void DeserializeJSonOrderList_ShouldNotThrow_WhenOrderTaxLineContainsNonNumericRateId()
		{
			const string orderId = "5176";
			const string nonNumericRateId = "AVATAX-TX-STATE-TAX";
			var orderListRawJsonWithNonNumericTaxLineRateId = "[{\"id\":" + orderId + ",\"order_number\":\"5176\",\"order_key\":\"123\",\"created_at\":\"2022-10-19T19:33:05Z\",\"updated_at\":\"2022-10-19T19:33:12Z\",\"completed_at\":\"1970-01-01T00:00:00Z\",\"status\":\"processing\",\"currency\":\"USD\",\"total\":\"0.55\",\"subtotal\":\"0.50\",\"total_line_items_quantity\":1,\"total_tax\":\"0.05\",\"total_shipping\":\"0.00\",\"cart_tax\":\"0.05\",\"shipping_tax\":\"0.00\",\"total_discount\":\"0.00\",\"shipping_methods\":\"\",\"payment_details\":{\"method_id\":\"stripe\",\"method_title\":\"Credit card / debit card\",\"paid\":true},\"billing_address\":{\"first_name\":\"OmriTESTING SHIPSTATION SYNC\",\"last_name\":\"Tester\",\"company\":\"\",\"address_1\":\"123 Some St\",\"address_2\":\"\",\"city\":\"Austin\",\"state\":\"TX\",\"postcode\":\"78749\",\"country\":\"US\",\"email\":\"testFake@skuvault.com\",\"phone\":\"\"},\"shipping_address\":{\"first_name\":\"OmriTESTING SHIPSTATION SYNC\",\"last_name\":\"Tester\",\"company\":\"\",\"address_1\":\"123 Some St\",\"address_2\":\"\",\"city\":\"Austin\",\"state\":\"TX\",\"postcode\":\"78749\",\"country\":\"US\"},\"note\":\"\",\"customer_ip\":\"1.1.1.1\",\"customer_user_agent\":\"Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:105.0) Gecko/20100101 Firefox/105.0\",\"customer_id\":3,\"view_order_url\":\"https://www.somewhere.com\",\"line_items\":[{\"id\":89,\"subtotal\":\"0.50\",\"subtotal_tax\":\"0.05\",\"total\":\"0.50\",\"total_tax\":\"0.05\",\"price\":\"0.50\",\"quantity\":1,\"tax_class\":\"\",\"name\":\"The Paris Hours\",\"product_id\":4660,\"sku\":\"LQ127\",\"meta\":[]}],\"shipping_lines\":[],\"tax_lines\":[{\"id\":90,\"rate_id\":\"" + nonNumericRateId + "\",\"code\":\"AVATAX-TX-STATE-TAX\",\"title\":\"State Sales Tax\",\"total\":\"0.03\",\"compound\":false},{\"id\":91,\"rate_id\":\"AVATAX-TX-CITY-TAX\",\"code\":\"AVATAX-TX-CITY-TAX\",\"title\":\"City Sales Tax\",\"total\":\"0.01\",\"compound\":false},{\"id\":92,\"rate_id\":\"AVATAX-TX-SPECIAL-TAX\",\"code\":\"AVATAX-TX-SPECIAL-TAX\",\"title\":\"Special Sales Tax\",\"total\":\"0.01\",\"compound\":false}],\"fee_lines\":[],\"coupon_lines\":[],\"customer\":{\"id\":3,\"created_at\":\"2022-08-24T15:20:53Z\",\"last_update\":\"2022-10-19T19:33:05Z\",\"email\":\"testFake@skuvault.com\",\"first_name\":\"Omri\",\"last_name\":\"Tester\",\"username\":\"ob\",\"role\":\"administrator\",\"last_order_id\":5176,\"last_order_date\":\"2022-10-19T19:33:05Z\",\"orders_count\":2,\"total_spent\":\"1.10\",\"avatar_url\":\"https://secure.gravatar.com/avatar/1111\",\"billing_address\":{\"first_name\":\"OmriTESTING SHIPSTATION SYNC\",\"last_name\":\"Tester\",\"company\":\"\",\"address_1\":\"123 Some St\",\"address_2\":\"\",\"city\":\"Austin\",\"state\":\"TX\",\"postcode\":\"78749\",\"country\":\"US\",\"email\":\"testFake@skuvault.com\",\"phone\":\"\"},\"shipping_address\":{\"first_name\":\"OmriTESTING SHIPSTATION SYNC\",\"last_name\":\"Tester\",\"company\":\"\",\"address_1\":\"123 Some St\",\"address_2\":\"\",\"city\":\"Austin\",\"state\":\"TX\",\"postcode\":\"78749\",\"country\":\"US\"}}}]";

			var result = _restApi.DeserializeJSon<LegacyModels.OrderList>(orderListRawJsonWithNonNumericTaxLineRateId);

			var resultOrder = result.Single();
			Assert.That(resultOrder.id, Is.EqualTo(orderId));
		}
	}
}