using Moq;
using SimpleIdServer.Core.Common.Models;
using SimpleIdServer.Core.Common.Repositories;
using SimpleIdServer.Core.Errors;
using SimpleIdServer.Core.Exceptions;
using SimpleIdServer.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Helpers
{
    public class AmrHelperFixture
    {
        private Mock<IAuthenticationContextclassReferenceRepository> _authenticationContextclassReferenceRepositoryStub;
        private IAmrHelper _amrHelper;

        [Fact]
        public async Task When_Pass_Null_Parameters_To_GetNextAmr_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _amrHelper.GetNextAmr(null, null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _amrHelper.GetNextAmr("acr", null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Pass_Unknown_Acr_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _authenticationContextclassReferenceRepositoryStub.Setup(a => a.Get(It.IsAny<string>())).Returns(Task.FromResult((AuthenticationContextclassReference)null));

            // ACT
            var result = await _amrHelper.GetNextAmr("acr", new[] { "amr" }).ConfigureAwait(false);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task When_Pass_First_Amr_Then_Second_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _authenticationContextclassReferenceRepositoryStub.Setup(a => a.Get(It.IsAny<string>())).Returns(Task.FromResult(new AuthenticationContextclassReference
            {
                Name = "acr",
                AmrLst = new List<string>
                {
                    "amr",
                    "amr1",
                    "amr2"
                }
            }));

            // ACT
            var result = await _amrHelper.GetNextAmr("acr", new[] { "amr", "amr1" }).ConfigureAwait(false);

            // ASSERT

            Assert.Equal("amr2", result);
        }

        [Fact]
        public async Task When_Pass_Last_Amr_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _authenticationContextclassReferenceRepositoryStub.Setup(a => a.Get(It.IsAny<string>())).Returns(Task.FromResult(new AuthenticationContextclassReference
            {
                Name = "acr",
                AmrLst = new List<string>
                {
                    "amr",
                    "amr1",
                    "amr2"
                }
            }));

            // ACT
            var result = await _amrHelper.GetNextAmr("acr", new[] { "amr", "amr1", "amr2" }).ConfigureAwait(false);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_No_Amr_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() => _amrHelper.GetAmr(new List<string>(), new[] { "pwd" }));
            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InternalError, exception.Code);
            Assert.Equal(ErrorDescriptions.NoActiveAmr, exception.Message);
        }

        [Fact]
        public void When_Amr_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERTS
            var exception = Assert.Throws<IdentityServerException>(() => _amrHelper.GetAmr(new List<string> { "invalid" }, new[] { "pwd" }));
            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InternalError, exception.Code);
            Assert.Equal(string.Format(ErrorDescriptions.TheAmrDoesntExist, "pwd"), exception.Message);
        }

        [Fact]
        public void When_Amr_Doesnt_Exist_Then_Default_One_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var amr = _amrHelper.GetAmr(new List<string> { "pwd" }, new[] { "invalid" });

            // ASSERTS
            Assert.Equal("pwd", amr);
        }

        [Fact]
        public void When_Amr_Exists_Then_Same_Amr_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var amr = _amrHelper.GetAmr(new List<string> { "amr" }, new[] { "amr" });

            // ASSERTS
            Assert.Equal("amr", amr);
        }

        private void InitializeFakeObjects()
        {
            _authenticationContextclassReferenceRepositoryStub = new Mock<IAuthenticationContextclassReferenceRepository>();
            _amrHelper = new AmrHelper(_authenticationContextclassReferenceRepositoryStub.Object);
        }
    }
}
