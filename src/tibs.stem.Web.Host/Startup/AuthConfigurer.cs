﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Runtime.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using tibs.stem.Web.Authentication.External;
using tibs.stem.Web.Authentication.External.Facebook;
using tibs.stem.Web.Authentication.External.Google;
using tibs.stem.Web.Authentication.External.Microsoft;
using tibs.stem.Web.Authentication.JwtBearer;

namespace tibs.stem.Web.Startup
{
    public static class AuthConfigurer
    {
        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="configuration">The configuration.</param>
        public static void Configure(IApplicationBuilder app, IConfiguration configuration)
        {
            app.UseIdentity();

            if (bool.Parse(configuration["IdentityServer:IsEnabled"]))
            {
                app.UseIdentityServer();
            }

            if (bool.Parse(configuration["Authentication:JwtBearer:IsEnabled"]))
            {
                app.UseJwtBearerAuthentication(CreateJwtBearerAuthenticationOptions(app));
            }

            var externalAuthConfiguration = app.ApplicationServices.GetRequiredService<ExternalAuthConfiguration>();

            if (bool.Parse(configuration["Authentication:Facebook:IsEnabled"]))
            {
                externalAuthConfiguration.Providers.Add(
                    new ExternalLoginProviderInfo(
                        FacebookAuthProviderApi.Name,
                        configuration["Authentication:Facebook:AppId"],
                        configuration["Authentication:Facebook:AppSecret"],
                        typeof(FacebookAuthProviderApi)
                    )
                );
            }

            if (bool.Parse(configuration["Authentication:Google:IsEnabled"]))
            {
                externalAuthConfiguration.Providers.Add(
                    new ExternalLoginProviderInfo(
                        GoogleAuthProviderApi.Name,
                        configuration["Authentication:Google:ClientId"],
                        configuration["Authentication:Google:ClientSecret"],
                        typeof(GoogleAuthProviderApi)
                    )
                );
            }

            //not implemented yet. Will be implemented with https://github.com/aspnetzero/aspnet-zero-angular/issues/5
            if (bool.Parse(configuration["Authentication:Microsoft:IsEnabled"]))
            {
                externalAuthConfiguration.Providers.Add(
                    new ExternalLoginProviderInfo(
                        MicrosoftAuthProviderApi.Name,
                        configuration["Authentication:Microsoft:ConsumerKey"],
                        configuration["Authentication:Microsoft:ConsumerSecret"],
                        typeof(MicrosoftAuthProviderApi)
                    )
                );
            }

            if (bool.Parse(configuration["IdentityServer:IsEnabled"]))
            {
                app.UseIdentityServerAuthentication(
                    new IdentityServerAuthenticationOptions
                    {
                        Authority = configuration["App:ServerRootAddress"],
                        RequireHttpsMetadata = false,
                        AutomaticAuthenticate = true,
                        AutomaticChallenge = true
                    });
            }
        }

        private static JwtBearerOptions CreateJwtBearerAuthenticationOptions(IApplicationBuilder app)
        {
            var tokenAuthConfig = app.ApplicationServices.GetRequiredService<TokenAuthConfiguration>();
            return new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = new TokenValidationParameters
                {
                    // The signing key must match!
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = tokenAuthConfig.SecurityKey,

                    // Validate the JWT Issuer (iss) claim
                    ValidateIssuer = true,
                    ValidIssuer = tokenAuthConfig.Issuer,

                    // Validate the JWT Audience (aud) claim
                    ValidateAudience = true,
                    ValidAudience = tokenAuthConfig.Audience,

                    // Validate the token expiry
                    ValidateLifetime = true,

                    // If you want to allow a certain amount of clock drift, set that here
                    ClockSkew = TimeSpan.Zero
                },

                Events = new JwtBearerEvents
                {
                    OnMessageReceived = QueryStringTokenResolver
                }
            };
        }

        /* This method is needed to authorize SignalR javascript client.
         * SignalR can not send authorization header. So, we are getting it from query string as an encrypted text. */
        private static Task QueryStringTokenResolver(MessageReceivedContext context)
        {
            if (!context.HttpContext.Request.Path.HasValue ||
                !context.HttpContext.Request.Path.Value.StartsWith("/signalr"))
            {
                //We are just looking for signalr clients
                return Task.CompletedTask;
            }

            var qsAuthToken = context.HttpContext.Request.Query["enc_auth_token"].FirstOrDefault();
            if (qsAuthToken == null)
            {
                //Cookie value does not matches to querystring value
                return Task.CompletedTask;
            }

            //Set auth token from cookie
            context.Token = SimpleStringCipher.Instance.Decrypt(qsAuthToken, AppConsts.DefaultPassPhrase);
            return Task.CompletedTask;
        }
    }
}
