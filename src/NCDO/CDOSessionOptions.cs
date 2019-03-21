using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using NCDO.Definitions;

namespace NCDO
{
    public class CDOSessionOptions
    {
        /// <summary>
        /// Service Uri
        /// </summary>
        public Uri ServiceUri { get; set; }

        /// <summary>
        /// Authentication model to use
        /// </summary>
        public AuthenticationModel AuthenticationModel { get; set; } = AuthenticationModel.Anonymous;

        /// <summary>
        /// Gets or sets the Authority to use when making OpenIdConnect calls.
        /// </summary>
        public string Authority { get; set; }

        /// <summary>Gets or sets the 'client_id' ~ username (basic auth) .</summary>
        public string ClientId { get; set; }

        /// <summary>Gets or sets the 'client_secret' ~ password (basic auth).</summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the audience for any received OpenIdConnect token.
        /// </summary>
        /// <value>
        /// The expected audience for any received OpenIdConnect token.
        /// </value>
        public string Audience { get; set; }

        /// <summary>
        /// Gets or sets the challenge to put in the "WWW-Authenticate" header.
        /// </summary>
        public string Challenge
        {
            get
            {
                switch (AuthenticationModel)
                {
                    case AuthenticationModel.Bearer_OnBehalf:
                    case AuthenticationModel.Bearer_WIA:
                        return AuthenticationModel.Bearer.ToString();
                    default:
                        return AuthenticationModel.ToString();
                }
            }
        }


        #region Bearer_onbehalf

        /// <summary>
        /// Function call to retrieve the User bearertoken
        /// </summary>
        public Func<string> UserAccessToken { get; set; } = () => null;

        /// <summary>
        /// Function call to retrieve the current user
        /// </summary>
        public Func<string> UserName { get; set; } = () => null;

        #endregion

        /// <summary>
        /// Specify SslProtocols required
        /// Ex: Tls12 | Tls11 | Tls
        /// </summary>
        public SecurityProtocolType SecurityProtocol { get; set; }
    }
}