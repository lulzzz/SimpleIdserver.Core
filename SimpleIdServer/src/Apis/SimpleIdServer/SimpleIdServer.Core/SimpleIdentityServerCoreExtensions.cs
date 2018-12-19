using Microsoft.Extensions.DependencyInjection;
using SimpleIdServer.AccountFilter;
using SimpleIdServer.Common.Client.Factories;
using SimpleIdServer.Core.Api.Authorization;
using SimpleIdServer.Core.Api.Authorization.Actions;
using SimpleIdServer.Core.Api.Authorization.Common;
using SimpleIdServer.Core.Api.Discovery;
using SimpleIdServer.Core.Api.Discovery.Actions;
using SimpleIdServer.Core.Api.Introspection;
using SimpleIdServer.Core.Api.Introspection.Actions;
using SimpleIdServer.Core.Api.Jwks;
using SimpleIdServer.Core.Api.Jwks.Actions;
using SimpleIdServer.Core.Api.Profile;
using SimpleIdServer.Core.Api.Profile.Actions;
using SimpleIdServer.Core.Api.Registration;
using SimpleIdServer.Core.Api.Registration.Actions;
using SimpleIdServer.Core.Api.Token;
using SimpleIdServer.Core.Api.Token.Actions;
using SimpleIdServer.Core.Api.UserInfo;
using SimpleIdServer.Core.Api.UserInfo.Actions;
using SimpleIdServer.Core.Authenticate;
using SimpleIdServer.Core.Common;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Factories;
using SimpleIdServer.Core.Helpers;
using SimpleIdServer.Core.Jwt.Converter;
using SimpleIdServer.Core.JwtToken;
using SimpleIdServer.Core.Protector;
using SimpleIdServer.Core.Repositories;
using SimpleIdServer.Core.Services;
using SimpleIdServer.Core.Translation;
using SimpleIdServer.Core.Validators;
using SimpleIdServer.Core.WebSite.Authenticate;
using SimpleIdServer.Core.WebSite.Authenticate.Actions;
using SimpleIdServer.Core.WebSite.Authenticate.Common;
using SimpleIdServer.Core.WebSite.Consent;
using SimpleIdServer.Core.WebSite.Consent.Actions;
using SimpleIdServer.Core.WebSite.User;
using SimpleIdServer.Core.WebSite.User.Actions;
using System;
using System.Collections.Generic;

namespace SimpleIdServer.Core
{
    public static class SimpleIdentityServerCoreExtensions
    {
        public static IServiceCollection AddSimpleIdentityServerCore(this IServiceCollection serviceCollection, OAuthConfigurationOptions configurationOptions = null, List<ClaimAggregate> claims = null, List<Common.Models.Client> clients = null, List<Consent> consents = null, List<JsonWebKey> jsonWebKeys = null,
            List<ResourceOwnerProfile> profiles = null, List<ResourceOwner> resourceOwners = null, List<Scope> scopes = null, List<Common.Models.Translation> translations = null, PasswordSettings passwordSettings = null)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }
            
			serviceCollection.AddTransient<IGrantedTokenGeneratorHelper, GrantedTokenGeneratorHelper>();
            serviceCollection.AddTransient<IConsentHelper, ConsentHelper>();
            serviceCollection.AddTransient<IClientHelper, ClientHelper>();
            serviceCollection.AddTransient<IAuthorizationFlowHelper, AuthorizationFlowHelper>();
            serviceCollection.AddTransient<IClientCredentialsGrantTypeParameterValidator, ClientCredentialsGrantTypeParameterValidator>();
            serviceCollection.AddTransient<IClientValidator, ClientValidator>();
            serviceCollection.AddTransient<IScopeValidator, ScopeValidator>();
            serviceCollection.AddTransient<IGrantedTokenValidator, GrantedTokenValidator>();
            serviceCollection.AddTransient<IAuthorizationCodeGrantTypeParameterAuthEdpValidator, AuthorizationCodeGrantTypeParameterAuthEdpValidator>();
            serviceCollection.AddTransient<IResourceOwnerGrantTypeParameterValidator, ResourceOwnerGrantTypeParameterValidator>();
            serviceCollection.AddTransient<IAuthorizationCodeGrantTypeParameterTokenEdpValidator, AuthorizationCodeGrantTypeParameterTokenEdpValidator>();
            serviceCollection.AddTransient<IRegistrationParameterValidator, RegistrationParameterValidator>();
            serviceCollection.AddTransient<ICompressor, Compressor>();
            serviceCollection.AddTransient<IEncoder, Encoder>();
            serviceCollection.AddTransient<IParameterParserHelper, ParameterParserHelper>();
            serviceCollection.AddTransient<IActionResultFactory, ActionResultFactory>();
            serviceCollection.AddTransient<IAuthorizationActions, AuthorizationActions>();
            serviceCollection.AddTransient<IGetAuthorizationCodeOperation, GetAuthorizationCodeOperation>();
            serviceCollection.AddTransient<IGetTokenViaImplicitWorkflowOperation, GetTokenViaImplicitWorkflowOperation>();
            serviceCollection.AddTransient<IUserInfoActions, UserInfoActions>();
            serviceCollection.AddTransient<IGetJwsPayload, GetJwsPayload>();
            serviceCollection.AddTransient<ITokenActions, TokenActions>();
            serviceCollection.AddTransient<IGetTokenByResourceOwnerCredentialsGrantTypeAction, GetTokenByResourceOwnerCredentialsGrantTypeAction>();
            serviceCollection.AddTransient<IGetTokenByAuthorizationCodeGrantTypeAction, GetTokenByAuthorizationCodeGrantTypeAction>();
            serviceCollection.AddTransient<IGetAuthorizationCodeAndTokenViaHybridWorkflowOperation, GetAuthorizationCodeAndTokenViaHybridWorkflowOperation>();
            serviceCollection.AddTransient<IGetTokenByClientCredentialsGrantTypeAction, GetTokenByClientCredentialsGrantTypeAction>();
            serviceCollection.AddTransient<IConsentActions, ConsentActions>();
            serviceCollection.AddTransient<IConfirmConsentAction, ConfirmConsentAction>();
            serviceCollection.AddTransient<IDisplayConsentAction, DisplayConsentAction>();
            serviceCollection.AddTransient<IRegistrationActions, RegistrationActions>();
            serviceCollection.AddTransient<IJwksActions, JwksActions>();
            serviceCollection.AddTransient<IRotateJsonWebKeysOperation, RotateJsonWebKeysOperation>();
            serviceCollection.AddTransient<IGetSetOfPublicKeysUsedToValidateJwsAction, GetSetOfPublicKeysUsedToValidateJwsAction>();
            serviceCollection.AddTransient<IJsonWebKeyEnricher, JsonWebKeyEnricher>();
            serviceCollection.AddTransient<IGetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction, GetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction>();
            serviceCollection.AddTransient<IAuthenticateActions, AuthenticateActions>();
            serviceCollection.AddTransient<IAuthenticateResourceOwnerOpenIdAction, AuthenticateResourceOwnerOpenIdAction>();
            serviceCollection.AddTransient<ILocalOpenIdUserAuthenticationAction, LocalOpenIdUserAuthenticationAction>();
            serviceCollection.AddTransient<IAuthenticateHelper, AuthenticateHelper>();
            serviceCollection.AddTransient<IDiscoveryActions, DiscoveryActions>();
            serviceCollection.AddTransient<ICreateDiscoveryDocumentationAction, CreateDiscoveryDocumentationAction>();
            serviceCollection.AddTransient<IProcessAuthorizationRequest, ProcessAuthorizationRequest>();
            serviceCollection.AddTransient<IJwtGenerator, JwtGenerator>();
            serviceCollection.AddTransient<IJwtParser, JwtParser>();
            serviceCollection.AddTransient<IGenerateAuthorizationResponse, GenerateAuthorizationResponse>();
            serviceCollection.AddTransient<IAuthenticateClient, AuthenticateClient>();
            serviceCollection.AddTransient<IAuthenticateInstructionGenerator, AuthenticateInstructionGenerator>();
            serviceCollection.AddTransient<IClientSecretBasicAuthentication, ClientSecretBasicAuthentication>();
            serviceCollection.AddTransient<IClientSecretPostAuthentication, ClientSecretPostAuthentication>();
            serviceCollection.AddTransient<IClientAssertionAuthentication, ClientAssertionAuthentication>();
            serviceCollection.AddTransient<IClientTlsAuthentication, ClientTlsAuthentication>();
            serviceCollection.AddTransient<IGetTokenByRefreshTokenGrantTypeAction, GetTokenByRefreshTokenGrantTypeAction>();
            serviceCollection.AddTransient<IRefreshTokenGrantTypeParameterValidator, RefreshTokenGrantTypeParameterValidator>();
            serviceCollection.AddTransient<ITranslationManager, TranslationManager>();
            serviceCollection.AddTransient<IGrantedTokenHelper, GrantedTokenHelper>();
            serviceCollection.AddTransient<IHttpClientFactory, HttpClientFactory>();
            serviceCollection.AddTransient<IIntrospectionActions, IntrospectionActions>();
            serviceCollection.AddTransient<IPostIntrospectionAction, PostIntrospectionAction>();
            serviceCollection.AddTransient<IIntrospectionParameterValidator, IntrospectionParameterValidator>();
            serviceCollection.AddTransient<IRegisterClientAction, RegisterClientAction>();
            serviceCollection.AddTransient<IJsonWebKeyConverter, JsonWebKeyConverter>();
            serviceCollection.AddTransient<IGenerateClientFromRegistrationRequest, GenerateClientFromRegistrationRequest>();
            serviceCollection.AddTransient<IGetConsentsOperation, GetConsentsOperation>();
            serviceCollection.AddTransient<IUserActions, UserActions>();
            serviceCollection.AddTransient<IRemoveConsentOperation, RemoveConsentOperation>();
            serviceCollection.AddTransient<IRevokeTokenAction, RevokeTokenAction>();
            serviceCollection.AddTransient<IGetUserOperation, GetUserOperation>();
            serviceCollection.AddTransient<IUpdateUserCredentialsOperation, UpdateUserCredentialsOperation>();
            serviceCollection.AddTransient<IUpdateUserClaimsOperation, UpdateUserClaimsOperation>();
            serviceCollection.AddTransient<IAddUserOperation, AddUserOperation>();
            serviceCollection.AddTransient<IGenerateAndSendCodeAction, GenerateAndSendCodeAction>();
            serviceCollection.AddTransient<IValidateConfirmationCodeAction, ValidateConfirmationCodeAction>();
            serviceCollection.AddTransient<IRemoveConfirmationCodeAction, RemoveConfirmationCodeAction>();
            serviceCollection.AddTransient<ITwoFactorAuthenticationHandler, TwoFactorAuthenticationHandler>();
            serviceCollection.AddTransient<IPayloadSerializer, PayloadSerializer>();
            serviceCollection.AddTransient<IProfileActions, ProfileActions>();
            serviceCollection.AddTransient<ILinkProfileAction, LinkProfileAction>();
            serviceCollection.AddTransient<IUnlinkProfileAction, UnlinkProfileAction>();
            serviceCollection.AddTransient<IGetUserProfilesAction, GetUserProfilesAction>();
            serviceCollection.AddTransient<IGetResourceOwnerClaimsAction, GetResourceOwnerClaimsAction>();
            serviceCollection.AddTransient<IUpdateUserTwoFactorAuthenticatorOperation, UpdateUserTwoFactorAuthenticatorOperation>();
            serviceCollection.AddTransient<IResourceOwnerAuthenticateHelper, ResourceOwnerAuthenticateHelper>();
            serviceCollection.AddTransient<IAmrHelper, AmrHelper>();
            serviceCollection.AddTransient<IRevokeTokenParameterValidator, RevokeTokenParameterValidator>();
            serviceCollection.AddSingleton<IClientPasswordService, DefaultClientPasswordService>();
            serviceCollection.AddSingleton<IConfigurationService>(new DefaultConfigurationService(configurationOptions));
            serviceCollection.AddSingleton<IClaimRepository>(new DefaultClaimRepository(claims));
            serviceCollection.AddSingleton<IClientRepository>(new DefaultClientRepository(clients));
            serviceCollection.AddSingleton<IConsentRepository>(new DefaultConsentRepository(consents));
            serviceCollection.AddSingleton<IJsonWebKeyRepository>(new DefaultJsonWebKeyRepository(jsonWebKeys));
            serviceCollection.AddSingleton<IProfileRepository>(new DefaultProfileRepository(profiles));
            serviceCollection.AddSingleton<IResourceOwnerRepository>(new DefaultResourceOwnerRepository(resourceOwners));
            serviceCollection.AddSingleton<IScopeRepository>(new DefaultScopeRepository(scopes));
            serviceCollection.AddSingleton<ITranslationRepository>(new DefaultTranslationRepository(translations));
            serviceCollection.AddSingleton<ISubjectBuilder>(new DefaultSubjectBuilder());
            serviceCollection.AddSingleton<IAccountFilter>(new DefaultAccountFilter());
            serviceCollection.AddSingleton<IUserClaimsEnricher>(new DefaultUserClaimsEnricher());
            serviceCollection.AddSingleton<IClientInfoService>(new DefaultClientInfoService());
            serviceCollection.AddSingleton<IPasswordSettingsRepository>(new DefaultPasswordSettingsRepository(passwordSettings));
            return serviceCollection;
        }
    }
}
