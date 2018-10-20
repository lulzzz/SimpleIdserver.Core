set ASPNETCORE_ENVIRONMENT=
set DATA_MIGRATED=true
START cmd /k "cd src/Apis/SimpleIdServer/SimpleIdServer.Startup && dotnet run -f net461"
START cmd /k "cd src/Apis/Uma/SimpleIdServer.Uma.Startup && dotnet run -f net461"
START cmd /k "cd src/Apis/Scim/SimpleIdServer.Scim.Startup && dotnet run -f net461"
echo Applications are running ...