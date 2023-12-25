REM unit test
cd ../CAT-main.UnitTests
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./TestResults/
REM sonarscanner
cd ../CAT-main
dotnet sonarscanner begin /k:"CAT-main" /d:sonar.host.url="http://localhost:9000"  /d:sonar.token="sqp_26299e0970cb343f2a81f1d10bbdeb6305cfe60d"
dotnet build 
dotnet sonarscanner end /d:sonar.token="sqp_26299e0970cb343f2a81f1d10bbdeb6305cfe60d"
REM pause
pause
