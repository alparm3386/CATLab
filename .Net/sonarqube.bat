REM unit test
cd CAT-service.UnitTests
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./TestResults/
REM sonarscanner
cd CAT-service
dotnet sonarscanner begin /k:"CAT-service" /d:sonar.cs.opencover.reportsPaths="../CAT-service.UnitTests/TestResults/coverage.opencover.xml" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="sqp_7c78030a1cfa4b3a6dcd091f24551bf9a637e65c"
dotnet build 
dotnet sonarscanner end /d:sonar.token="sqp_7c78030a1cfa4b3a6dcd091f24551bf9a637e65c"
REM pause
pause
