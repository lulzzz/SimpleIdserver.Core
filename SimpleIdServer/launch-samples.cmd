set ASPNETCORE_ENVIRONMENT=
set DATA_MIGRATED=true
START cmd /k "cd samples/SimpleIdServer.Openid.Server && dotnet run -f net461"
START cmd /k "cd samples/SimpleIdServer.Protected.Api && dotnet run -f net461"
echo Applications are running ...