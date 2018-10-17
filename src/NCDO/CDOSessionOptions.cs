using System;
using System.Security.Authentication;
using System.Text;
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
                    case AuthenticationModel.Bearer_WIA:
                        return AuthenticationModel.Bearer.ToString();
                    default:
                        return AuthenticationModel.ToString();
                }
            }
        }

        /// <summary>
        /// The token passed can be overruled by strictly setting the token in the options
        /// </summary>
        private string _token;
        private TokenCache _tokenCache = new TokenCache();
        /// <summary>
        /// Returns the token to pass in the headers for the given challenge
        /// </summary>
        public string ChallengeToken
        {
            get
            {
                if (_token == null)
                {
                    switch (AuthenticationModel)
                    {
                        case AuthenticationModel.Basic:
                            if (ClientId == null) throw new ArgumentNullException(nameof(ClientId));
                            if (ClientSecret == null) throw new ArgumentNullException(nameof(ClientSecret));
                            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}"));
                        case AuthenticationModel.Bearer:
                            if (ClientId == null) throw new ArgumentNullException(nameof(ClientId));
                            if (ClientSecret == null) throw new ArgumentNullException(nameof(ClientSecret));
                            if (Authority == null) throw new ArgumentNullException(nameof(Authority));
                            if (Audience == null) throw new ArgumentNullException(nameof(Audience));
                            var authContext = new AuthenticationContext(Authority, _tokenCache);
                            var clientCredential = new ClientCredential(ClientId, ClientSecret);
                            var tokenResult = authContext.AcquireTokenAsync(Audience, clientCredential).Result;
                            return tokenResult.AccessToken;
                        case AuthenticationModel.Bearer_WIA:
                            if (ClientId == null) throw new ArgumentNullException(nameof(ClientId));
                            if (Authority == null) throw new ArgumentNullException(nameof(Authority));
                            if (Audience == null) throw new ArgumentNullException(nameof(Audience));
                            var authContext2 = new AuthenticationContext(Authority, _tokenCache);
                            var userCredential = new UserCredential();
                            var tokenResult2 = authContext2.AcquireTokenAsync(Audience, ClientId, userCredential).Result;
                            return tokenResult2.AccessToken;
                    }
                }

                return _token;
            }
            protected set => _token = value;
        }

        /// <summary>
        /// Specify SslProtocols required
        /// Ex: SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls
        /// </summary>
        public SslProtocols SslProtocols { get; set; } = SslProtocols.Default;
    }
}
