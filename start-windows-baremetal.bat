cd /D "%~dp0"

call pip install -r ./python/requirements.txt
call dotnet restore ./dotnet/TSRACT.csproj
start dotnet run --project ./dotnet/TSRACT.csproj
start http://localhost:5000
pause
