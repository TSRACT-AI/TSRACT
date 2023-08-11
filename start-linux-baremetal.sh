#!/bin/bash

pip install -r ./python/requirements.txt
dotnet restore ./dotnet/TSRACT.csproj
dotnet run --project ./dotnet/TSRACT.csproj -- --listen-all-bindings=true
