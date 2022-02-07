using FluentAssertions;
using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using WooCommerceAccess.Services.Authentication;

namespace WooCommerceTests
{
	[ TestFixture( "credentials_cloud_sandbox.csv" ) ]
	public class AuthTests : BaseTest
	{
		protected WooCommerceAuthenticationService AuthenticationService { get; private set; }
		protected WooCommerceOAuthAppCredentials AppCredentials { get; private set; }
		private long tenantId = 12345;

		public AuthTests( string credentialsFileName ) : base ( credentialsFileName ) { }

		[ SetUp ]
		public void Init()
		{
			this.AppCredentials = this.LoadApplicationCredentials();
			this.AuthenticationService = new WooCommerceAuthenticationService( this.AppCredentials, base.Config );
		}

		[ Test ]
		public async Task GetAuthenticationHtmlForm()
		{
			string requestId = string.Format("{0}:{1}", this.tenantId, Guid.NewGuid().ToString() );
			var htmlForm = await this.AuthenticationService.GetAuthenticationHtmlForm( requestId, this.Mark );

			htmlForm.Should().NotBeNullOrWhiteSpace();
			htmlForm.Should().Contain( String.Format( "{0} would like to connect to your store", this.AppCredentials.AppName ) );
		}

		protected WooCommerceOAuthAppCredentials LoadApplicationCredentials()
		{
			string path = new Uri( Path.GetDirectoryName( Assembly.GetExecutingAssembly().CodeBase ) ).LocalPath;

			using( var reader = new StreamReader( path + @"\..\..\app-credentials.csv" ) )
			{
				string appName = reader.ReadLine();
				string scope = reader.ReadLine();
				string returnUrl = reader.ReadLine();
				string callbackUrl = reader.ReadLine();

				return new WooCommerceOAuthAppCredentials( appName, scope, returnUrl, callbackUrl );
			}
		}
	}
}
