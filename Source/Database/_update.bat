:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: Script update
:: Runs in the order by name all *.sql in the current directory
:: Parameters:
:: "%1" - service name[,port number] ("" required in case port number needs to be included)
:: %2 - database
:: %3 - user (optional)                                  :
:: %4 - password (optional)                                  :
::                                                                         
:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
@echo off
cls

::Check the parameters
if %1=="" goto ErrorWithParameters
if "%2"=="" goto ErrorWithParameters
if not "%3"=="" if "%4"=="" goto ErrorWithParameters 
if "%3"=="" if not "%4"=="" goto ErrorWithParameters

echo Now Updating Database %2 on Server %1

:: Main loop for each file (%%i) in the directory with *.sql
for /f "tokens=1" %%i in ('dir /b *.sql ^| find /c /i ".sql"') do set nFiles=%%i
set /a fileNo=1
for /f %%i in ('dir /b /on *.sql') do (
	call :ProgressMeter %%fileNo%% %nFiles%
	call _execute_script.bat %1 %2 %%i %3 %4
	if errorlevel 1 goto :Error
	set /a fileNo+=1
)

:: Reset counters
set nFiles=
set fileNo=
echo.
echo End of Processing: SUCCESS
echo.
goto :End

:ErrorWithParameters
echo Parameter error with Calling Program:
echo
echo %0 ^<SERVER/SERVICE^> ^<DB_ALIAS^> [^<DB_USER^> ^<PASSWORD^>]
goto :EOF

:: ******************************************************************
:ProgressMeter
setlocal enabledelayedexpansion
set /A ProgressPercent=%1*100/%2

set /A NumBars=%ProgressPercent%/2
set /A NumSpaces=50-%NumBars%

set Meter=

:: Build the meter image using vertical bars followed by trailing spaces
:: Note there is a trailing space at the end of the second line below
for /L %%A in (%NumBars%,-1,1) do set Meter=!Meter!I
for /L %%A in (%NumSpaces%,-1,1) do set Meter=!Meter! 

:: Display the progress meter in the title bar and return to the main program
TITLE Progress:  [%Meter%]  %ProgressPercent%%%
endlocal
goto :EOF
:: ******************************************************************

:: Exit with an Error Code 1
:Error
exit /B 1

:End
call :ProgressMeter 100 100
TITLE cmd.exe
exit /B 0