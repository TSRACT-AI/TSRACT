cd /D "%~dp0"

@rem config
set INSTALL_DIR=%cd%\python
set CONDA_ROOT_PREFIX=%cd%\python\conda
set INSTALL_ENV_DIR=%cd%\python\env
set conda_exists=F

@rem figure out whether conda is installed
call "%CONDA_ROOT_PREFIX%\_conda.exe" --version >nul 2>&1
if "%ERRORLEVEL%" EQU "0" set conda_exists=T

if "%conda_exists%" == "F" (
	echo Miniconda is not installed. Please run the install script first.
	goto end
)

@rem environment isolation
set PYTHONNOUSERSITE=1
set PYTHONPATH=
set PYTHONHOME=
set "CUDA_PATH=%INSTALL_ENV_DIR%"
set "CUDA_HOME=%CUDA_PATH%"

@rem activate installer env
call "%CONDA_ROOT_PREFIX%\condabin\conda.bat" activate "%INSTALL_ENV_DIR%" || ( echo. && echo Miniconda hook not found. && goto end )

@rem setup installer env
echo Restoring dotnet project dependencies...
call dotnet restore ./dotnet/TSRACT.csproj

echo Running project...
start dotnet run --project ./dotnet/TSRACT.csproj -- --bypass-conda=false

start http://localhost:5000

:end
pause
