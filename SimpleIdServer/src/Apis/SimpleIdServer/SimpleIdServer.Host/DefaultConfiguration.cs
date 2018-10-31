﻿using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleIdServer.Host
{
    internal static class DefaultConfiguration
    {
        public static List<Translation> DEFAULT_TRANSLATIONS = new List<Translation>
        {
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.ApplicationWouldLikeToCode,
                Value = "the client {0} would like to access"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.IndividualClaimsCode,
                Value = "individual claims"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.NameCode,
                Value = "Name"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.LoginCode,
                Value = "Login"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.PasswordCode,
                Value = "Password"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.UserNameCode,
                Value = "Username"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.ConfirmCode,
                Value = "Confirm"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.CancelCode,
                Value = "Cancel"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.LoginLocalAccount,
                Value = "Login with your local account"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.LoginExternalAccount,
                Value = "Login with your external account"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.LinkToThePolicy,
                Value = "policy"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.Tos,
                Value = "Terms of Service"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.SendCode,
                Value = "Send code"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.Code,
                Value = "Code"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.EditResourceOwner,
                Value = "Edit resource owner"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.YourName,
                Value = "Your name"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.YourPassword,
                Value = "Your password"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.Email,
                Value = "Email"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.YourEmail,
                Value = "Your email"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.TwoAuthenticationFactor,
                Value = "Two authentication factor"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.UserIsUpdated,
                Value = "User has been updated"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.SendConfirmationCode,
                Value = "Send a confirmation code"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.Phone,
                Value = "Phone"
            },
            new Translation
            {
                LanguageTag = "en",
                Code = Core.Constants.StandardTranslationCodes.HashedPassword,
                Value = "Hashed password"
            }
        };

        public static List<Scope> DEFAULT_SCOPES = new List<Scope>
        {
            new Scope
            {
                Name = "openid",
                IsExposed = true,
                IsOpenIdScope = true,
                IsDisplayedInConsent = true,
                Description = "access to the openid scope",
                Type = ScopeType.ProtectedApi,
                Claims = new List<string> { },
                CreateDateTime = DateTime.UtcNow,
                UpdateDateTime = DateTime.UtcNow
            },
            new Scope
            {
                Name = "profile",
                IsExposed = true,
                IsOpenIdScope = true,
                Description = "Access to the profile",
                Claims = new List<string>
                {
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name,
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName,
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.GivenName,
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName,
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.NickName,
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName,
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.Profile,
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.Picture,
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.WebSite,
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.Gender,
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate,
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo,
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.Locale,
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt
                },
                Type = ScopeType.ResourceOwner,
                IsDisplayedInConsent = true,
                CreateDateTime = DateTime.UtcNow,
                UpdateDateTime = DateTime.UtcNow
            },
            new Scope
            {
                Name = "scim",
                IsExposed = true,
                IsOpenIdScope = true,
                Description = "Access to the scim",
                Claims = new List<string>
                {
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.ScimId,
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.ScimLocation
                },
                Type = ScopeType.ResourceOwner,
                IsDisplayedInConsent = true,
                CreateDateTime = DateTime.UtcNow,
                UpdateDateTime = DateTime.UtcNow
            },
            new Scope
            {
                Name = "email",
                IsExposed = true,
                IsOpenIdScope = true,
                IsDisplayedInConsent = true,
                Description = "Access to the email",
                Claims = new List<string>
                {
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email,
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified
                },
                Type = ScopeType.ResourceOwner,
                CreateDateTime = DateTime.UtcNow,
                UpdateDateTime = DateTime.UtcNow
            },
            new Scope
            {
                Name = "address",
                IsExposed = true,
                IsOpenIdScope = true,
                IsDisplayedInConsent = true,
                Description = "Access to the address",
                Claims = new List<string>
                {
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.Address
                },
                Type = ScopeType.ResourceOwner,
                CreateDateTime = DateTime.UtcNow,
                UpdateDateTime = DateTime.UtcNow
            },
            new Scope
            {
                Name = "phone",
                IsExposed = true,
                IsOpenIdScope = true,
                IsDisplayedInConsent = true,
                Description = "Access to the phone",
                Claims = new List<string>
                {
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber,
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified
                },
                Type = ScopeType.ResourceOwner,
                CreateDateTime = DateTime.UtcNow,
                UpdateDateTime = DateTime.UtcNow
            },
            new Scope
            {
                Name = "role",
                IsExposed = true,
                IsOpenIdScope = false,
                IsDisplayedInConsent = true,
                Description = "Access to your roles",
                Claims = new List<string>
                {
                    Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role
                },
                Type = ScopeType.ResourceOwner,
                CreateDateTime = DateTime.UtcNow,
                UpdateDateTime = DateTime.UtcNow
            },
            new Scope
            {
                Name = "register_client",
                IsExposed = false,
                IsOpenIdScope = false,
                IsDisplayedInConsent = true,
                Description = "Register a client",
                Type = ScopeType.ProtectedApi,
                CreateDateTime = DateTime.UtcNow,
                UpdateDateTime = DateTime.UtcNow
            },
            new Scope
            {
                Name = "manage_profile",
                IsExposed = false,
                IsOpenIdScope = false,
                IsDisplayedInConsent = true,
                Description = "Manage the user's profiles",
                Type = ScopeType.ProtectedApi,
                CreateDateTime = DateTime.UtcNow,
                UpdateDateTime = DateTime.UtcNow
            },
            new Scope
            {
                Name = "manage_account_filtering",
                IsExposed = false,
                IsOpenIdScope = false,
                IsDisplayedInConsent = true,
                Description = "Manage the account filtering",
                Type = ScopeType.ProtectedApi,
                CreateDateTime = DateTime.UtcNow,
                UpdateDateTime = DateTime.UtcNow
            }
        };

        public static List<ResourceOwner> DEFAULT_USERS = new List<ResourceOwner>
        {
            new ResourceOwner
            {
                Id = "administrator",
                CreateDateTime = DateTime.UtcNow,
                UpdateDateTime =  DateTime.UtcNow,
                Password = PasswordHelper.ComputeHash("password"),
                Claims = new List<Claim>
                {
                    new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, "administrator")
                }
            }
        };

        public static List<Client> DEFAULT_CLIENTS = new List<Client>
        {
            new Client
            {
                ClientId = "clientAdministrator",
                Secrets = new List<ClientSecret>
                {
                    new ClientSecret
                    {
                        Type = ClientSecretTypes.SharedSecret,
                        Value = "clientSecret"
                    }
                },
                ClientName = "Client administrator",
                TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                ApplicationType = ApplicationTypes.native,
                UpdateDateTime = DateTime.UtcNow,
                CreateDateTime = DateTime.UtcNow,
                AllowedScopes = new List<Scope>
                {
                    new Scope
                    {
                        Name = "register_client"
                    }
                },
                GrantTypes = new List<GrantType>
                {
                    GrantType.client_credentials
                },
                ResponseTypes = new List<ResponseType>
                {
                    ResponseType.token
                }
            }
        };

        public static List<ClaimAggregate> DEFAULT_CLAIMS = new List<ClaimAggregate>
        {
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, IsIdentifier = true },
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name },
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName },
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.GivenName },
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName },
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.NickName },
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName },
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Profile },
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Picture },
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.WebSite },
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Gender },
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate },
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo },
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Locale },
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt },
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email },
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified },
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Address },
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber },
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified },
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Role },
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.ScimId },
            new ClaimAggregate { Code = Core.Jwt.Constants.StandardResourceOwnerClaimNames.ScimLocation }
        };
    }
}