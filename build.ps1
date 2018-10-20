param
(
    $config = 'Release'
)

$hostSln = Resolve-Path .\SimpleIdServer\SimpleIdServer.Host.sln
$umaSln = Resolve-Path .\SimpleIdServer\SimpleIdServer.Uma.sln
$scimSln = Resolve-Path .\SimpleIdServer\SimpleIdServer.Scim.sln

dotnet build $hostSln -c $config
dotnet build $umaSln -c $config
dotnet build $scimSln -c $config