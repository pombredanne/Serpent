@echo Building new version...
@call build-dotnet-microsoft.cmd
@echo.
echo "Running tests"

L:\tools\nunit2.6\nunit-console-x86 /framework:net-4.0 /nothread /noshadow .\Serpent.Test\bin\Release\Razorvine.Serpent.Test.dll
