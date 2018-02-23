using System;
using System.Collections.Generic;
using System.Text;
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
                    case AuthenticationModel.Basic:
                        return "Basic";
                    case AuthenticationModel.Form:
                        break;
                    case AuthenticationModel.SingleSignOn:
                        break;
                    case AuthenticationModel.Form_SingleSignOn:
                        break;
                }

                return null;
            }
        }

        /// <summary>
        /// Returns the token to pass in the headers for the given challenge
        /// </summary>
        public string ChallengeToken
        {
            get
            {
                switch (AuthenticationModel)
                {
                    case AuthenticationModel.Basic:
                        return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}"));
                    case AuthenticationModel.Form:
                        break;
                    case AuthenticationModel.SingleSignOn:
                        break;
                    case AuthenticationModel.Form_SingleSignOn:
                        break;
                }

                return null;
            }
        }
    }
}
