REM unit test
cd CAT-onlineEditor.UnitTests
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput=./TestResults/
REM sonarscanner
cd ../CAT-OnlineEditor
dotnet sonarscanner begin /k:"CAT-online-editor" /d:sonar.host.url="http://localhost:9000"  /d:sonar.token="sqp_2fc155f2489e53c0c503a40e0662e24059ab47ec"
dotnet build 
dotnet sonarscanner end /d:sonar.token="sqp_2fc155f2489e53c0c503a40e0662e24059ab47ec"
REM pause
pause
