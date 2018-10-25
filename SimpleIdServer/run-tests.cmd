REM SIMPLEIDSERVER
dotnet test tests\SimpleIdServer.Core.Jwt.UnitTests
dotnet test tests\SimpleIdServer.Core.UnitTests
dotnet test tests\SimpleIdServer.Host.Tests
dotnet test tests\SimpleIdServer.AccessToken.Store.Tests
dotnet test tests\SimpleIdServer.Storage.Tests
dotnet test tests\SimpleIdServer.AccountFilter.Basic.Tests
dotnet test tests\SimpleIdServer.Authenticate.SMS.Tests
dotnet test tests\SimpleIdServer.Store.Tests

REM UMA
dotnet test tests\SimpleIdServer.Uma.Core.UnitTests
dotnet test tests\SimpleIdServer.Uma.Host.Tests

REM SCIM
dotnet test tests\SimpleIdServer.Scim.Core.Tests
dotnet test tests\SimpleIdServer.Scim.Client.Tests