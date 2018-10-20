#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Moq;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Jwt;
using SimpleIdServer.Core.WebSite.User.Actions;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.User
{
    public class GetUserOperationFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private IGetUserOperation _getUserOperation;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _getUserOperation.Execute(null));
        }

        [Fact]
        public async Task When_User_Is_Not_Authenticated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var emptyClaimsPrincipal = new ClaimsPrincipal();

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _getUserOperation.Execute(emptyClaimsPrincipal));

            // ASSERT
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.UnhandledExceptionCode);
            Assert.True(exception.Message == ErrorDescriptions.TheUserNeedsToBeAuthenticated);
        }

        [Fact]
        public async Task When_Subject_Is_Not_Passed_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var claimsIdentity = new ClaimsIdentity("test");
            claimsIdentity.AddClaim(new Claim("test", "test"));
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // ACT
            var exception = await  Assert.ThrowsAsync<IdentityServerException>(() => _getUserOperation.Execute(claimsPrincipal));

            // ASSERT
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.UnhandledExceptionCode);
            Assert.True(exception.Message == ErrorDescriptions.TheSubjectCannotBeRetrieved);
        }
        
        [Fact]
        public async Task When_Correct_Subject_Is_Passed_Then_ResourceOwner_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var claimsIdentity = new ClaimsIdentity("test");
            claimsIdentity.AddClaim(new Claim(Constants.StandardResourceOwnerClaimNames.Subject, "subject"));
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceOwner()));

            // ACT
            var result = _getUserOperation.Execute(claimsPrincipal);

            // ASSERT
            Assert.NotNull(result);
        }
        
        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _getUserOperation = new GetUserOperation(_resourceOwnerRepositoryStub.Object);
        }
    }
}
