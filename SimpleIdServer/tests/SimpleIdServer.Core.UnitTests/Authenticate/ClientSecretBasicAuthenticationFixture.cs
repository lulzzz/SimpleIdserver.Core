﻿using System;
using Xunit;
using System.Collections.Generic;
using SimpleIdServer.Core.Authenticate;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Services;
using Moq;

namespace SimpleIdentityServer.Core.UnitTests.Authenticate
{
    public sealed class ClientSecretBasicAuthenticationFixture
    {
        private Mock<IClientPasswordService> _clientPasswordServiceStub;
        private IClientSecretBasicAuthentication _clientSecretBasicAuthentication;

        [Fact]
        public void When_Trying_To_Authenticate_The_Client_And_OneParameter_Is_Null_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authenticateInstruction = new AuthenticateInstruction();

            // ACT & ASSERTS
            Assert.Throws<ArgumentNullException>(() => _clientSecretBasicAuthentication.AuthenticateClient(null, null));
            Assert.Throws<ArgumentNullException>(() => _clientSecretBasicAuthentication.AuthenticateClient(authenticateInstruction, null));
        }

        [Fact]
        public void When_Trying_To_Authenticate_The_Client_And_ThereIsNoSharedSecret_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authenticateInstruction = new AuthenticateInstruction
            {
                ClientSecretFromAuthorizationHeader = "notCorrectClientSecret"
            };
            var firstClient = new Client
            {
                Secrets = null
            };
            var secondClient = new Client
            {
                Secrets = new List<ClientSecret>
                {
                    new ClientSecret
                    {
                        Type = ClientSecretTypes.X509Thumbprint
                    }
                }
            };
            _clientPasswordServiceStub.Setup(c => c.Encrypt(It.IsAny<string>())).Returns(string.Empty);

            // ACTS & ASSERTS
            Assert.Null(_clientSecretBasicAuthentication.AuthenticateClient(authenticateInstruction, firstClient));
            Assert.Null(_clientSecretBasicAuthentication.AuthenticateClient(authenticateInstruction, secondClient));
        }

        [Fact]
        public void When_Trying_To_Authenticate_The_Client_And_Credentials_Are_Not_Correct_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authenticateInstruction = new AuthenticateInstruction
            {
                ClientSecretFromAuthorizationHeader= "notCorrectClientSecret"
            };
            var client = new Client
            {
                Secrets = new List<ClientSecret>
                {
                    new ClientSecret
                    {
                        Type = ClientSecretTypes.SharedSecret,
                        Value = "not_correct"
                    }
                }
            };
            _clientPasswordServiceStub.Setup(c => c.Encrypt(It.IsAny<string>())).Returns("notCorrectClientSecret");

            // ACT
            var result = _clientSecretBasicAuthentication.AuthenticateClient(authenticateInstruction, client);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_Trying_To_Authenticate_The_Client_And_Credentials_Are_Correct_Then_Client_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientSecret = "clientSecret";
            var authenticateInstruction = new AuthenticateInstruction
            {
                ClientSecretFromAuthorizationHeader = clientSecret
            };
            var client = new Client
            {
                Secrets = new List<ClientSecret>
                {
                    new ClientSecret
                    {
                        Type = ClientSecretTypes.SharedSecret,
                        Value = clientSecret
                    }
                }
            };
            _clientPasswordServiceStub.Setup(c => c.Encrypt(It.IsAny<string>())).Returns(clientSecret);

            // ACT
            var result = _clientSecretBasicAuthentication.AuthenticateClient(authenticateInstruction, client);

            // ASSERT
            Assert.NotNull(result);
        }

        [Fact]
        public void When_Requesting_ClientId_And_Instruction_Is_Null_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _clientSecretBasicAuthentication.GetClientId(null));
        }

        [Fact]
        public void When_Requesting_ClientId_Then_ClientId_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientId = "clientId";
            var instruction = new AuthenticateInstruction
            {
                ClientIdFromAuthorizationHeader = clientId
            };

            // ACT
            var result = _clientSecretBasicAuthentication.GetClientId(instruction);
            
            // ASSERT
            Assert.True(clientId == result);

        }

        private void InitializeFakeObjects()
        {
            _clientPasswordServiceStub = new Mock<IClientPasswordService>();
            _clientSecretBasicAuthentication = new ClientSecretBasicAuthentication(_clientPasswordServiceStub.Object);
        }
    }
}