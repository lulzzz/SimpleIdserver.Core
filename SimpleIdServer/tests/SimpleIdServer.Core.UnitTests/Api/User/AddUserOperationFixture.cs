﻿using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using SimpleIdServer.AccountFilter;
using SimpleIdServer.Core.Api.Profile.Actions;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Parameters;
using SimpleIdServer.Core.Services;
using SimpleIdServer.OpenId.Logging;
using Xunit;
using SimpleIdServer.Core.Api.User.Actions;

namespace SimpleIdentityServer.Core.UnitTests.Api.User
{
    public class AddUserOperationFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<IClaimRepository> _claimsRepositoryStub;
        private Mock<ILinkProfileAction> _linkProfileActionStub;
        private Mock<IOpenIdEventSource> _openidEventSourceStub;
        private Mock<ISubjectBuilder> _subjectBuilderStub;
        private Mock<IAccountFilter> _accountFilterStub;
        private Mock<IUserClaimsEnricher> _userClaimsEnricherStub;
        private Mock<IAddUserCredentialsOperation> _addUserCredentialsOperationStub;
        private IAddUserOperation _addResourceOwnerAction;
        
        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addResourceOwnerAction.Execute(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _addResourceOwnerAction.Execute(new AddUserParameter(null), null));
        }

        [Fact]
        public async Task When_ResourceOwner_With_Same_Credentials_Exists_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new AddUserParameter(null);

            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceOwner()));

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _addResourceOwnerAction.Execute(parameter, null));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.UnhandledExceptionCode);
            Assert.True(exception.Message == ErrorDescriptions.TheRoWithCredentialsAlreadyExists);
        }

        [Fact]
        public async Task When_ResourceOwner_Cannot_Be_Added_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _subjectBuilderStub.Setup(s => s.BuildSubject()).Returns(Task.FromResult("sub")); _accountFilterStub.Setup(s => s.Check(It.IsAny<IEnumerable<Claim>>())).Returns(Task.FromResult(new AccountFilterResult
            {
                IsValid = true
            }));
            _resourceOwnerRepositoryStub.Setup(r => r.InsertAsync(It.IsAny<ResourceOwner>())).Returns(Task.FromResult(false));
            var parameter = new AddUserParameter(null);

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _addResourceOwnerAction.Execute(parameter, null));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.Equal("unhandled_exception", exception.Code);
            Assert.Equal("An error occured while trying to insert the resource owner", exception.Message);
        }
        
        [Fact]
        public async Task When_Add_ResourceOwner_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new AddUserParameter(new List<Claim>());
            _accountFilterStub.Setup(s => s.Check(It.IsAny<IEnumerable<Claim>>())).Returns(Task.FromResult(new AccountFilterResult
            {
                IsValid = true
            }));
            _subjectBuilderStub.Setup(s => s.BuildSubject()).Returns(Task.FromResult("sub"));
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult((ResourceOwner)null));
            _resourceOwnerRepositoryStub.Setup(r => r.InsertAsync(It.IsAny<ResourceOwner>())).Returns(Task.FromResult(true));

            // ACT
            await _addResourceOwnerAction.Execute(parameter, null);

            // ASSERT
            _resourceOwnerRepositoryStub.Verify(r => r.InsertAsync(It.IsAny<ResourceOwner>()));
            _openidEventSourceStub.Verify(o => o.AddResourceOwner("sub"));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _claimsRepositoryStub = new Mock<IClaimRepository>();
            _linkProfileActionStub = new Mock<ILinkProfileAction>();
            _openidEventSourceStub = new Mock<IOpenIdEventSource>();
            _subjectBuilderStub = new Mock<ISubjectBuilder>();
            _accountFilterStub = new Mock<IAccountFilter>();
            _userClaimsEnricherStub = new Mock<IUserClaimsEnricher>();
            _addUserCredentialsOperationStub = new Mock<IAddUserCredentialsOperation>();

            _addResourceOwnerAction = new AddUserOperation(
                _resourceOwnerRepositoryStub.Object,
                _claimsRepositoryStub.Object,
                _linkProfileActionStub.Object,
                _accountFilterStub.Object,
                _openidEventSourceStub.Object,
                new List<IUserClaimsEnricher> { _userClaimsEnricherStub.Object },
                _subjectBuilderStub.Object,
                _addUserCredentialsOperationStub.Object);
        }
    }
}
