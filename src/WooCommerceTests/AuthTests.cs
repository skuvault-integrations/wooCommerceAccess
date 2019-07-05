using FluentAssertions;
using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using WooCommerceAccess;
using WooCommerceAccess.Services.Authentication;

namespace WooCommerceTests
{
	[ TestFixture ]
	public class AuthTests : BaseTest
	{
		protected WooCommerceAuthenticationService AuthenticationService { get; private set; }
		protected OAuthAppCredentials AppCredentials { get; private set; }

		[ SetUp ]
		public void Init()
		{
			this.AppCredentials = this.LoadApplicationCredentials();
			this.AuthenticationService = new WooCommerceAuthenticationService( this.AppCredentials, base.Config );
		}

		[ Test ]
		public async Task GetAuthenticationHtmlForm()
		{
			var htmlForm = await this.AuthenticationService.GetAuthenticationHtmlForm();

			htmlForm.Should().NotBeNullOrWhiteSpace();
			htmlForm.Should().Contain( String.Format( "{0} would like to connect to your store", this.AppCredentials.AppName ) );
		}

		protected OAuthAppCredentials LoadApplicationCredentials()
		{
			string path = new Uri( Path.GetDirectoryName( Assembly.GetExecutingAssembly().CodeBase ) ).LocalPath;

			using( var reader = new StreamReader( path + @"\..\..\app-credentials.csv" ) )
			{
				string appName = reader.ReadLine();
				string scope = reader.ReadLine();
				string tenantId = reader.ReadLine();
				string returnUrl = reader.ReadLine();
				string callbackUrl = reader.ReadLine();

				return new OAuthAppCredentials( appName, scope, tenantId, returnUrl, callbackUrl );
			}
		}
	}
}
