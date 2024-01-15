REM unit test
cd ../UnitTests/CAT-main.UnitTests
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./TestResults/
REM sonarscanner
cd ../CAT-main
dotnet sonarscanner begin /k:"Cat-main" /d:sonar.host.url="http://localhost:9000"  /d:sonar.token="sqp_e9d7a1cc7cf38e35cb3308775dc695b38eac8d03"
dotnet build 
dotnet sonarscanner end /d:sonar.token="sqp_e9d7a1cc7cf38e35cb3308775dc695b38eac8d03"
REM pause
pause
