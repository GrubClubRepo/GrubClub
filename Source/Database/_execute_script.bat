:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: Run SQL Script      
:: Parameters:
:: "%1" - server name[,port number] ("" required in case port number needs to be included)
:: %2 - database
:: %3 - script file
:: %4 - username (optional)                                  
:: %5 - password (optional)                                                                                                          
::
::		    	SQLCMDCURRDIR - current directory
::		    	SQLCMDSCRIPTPATH - path to the script files
::		    	SQLCMDSCRIPTFILENAME - script name with no extension
:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
@echo off

:Display the file parameters
:for %%j IN (%*) DO ECHO %%j

if %1=="" goto Error
if "%2"=="" goto Error
if not exist %3 goto Break
if not "%4"=="" if "%5"=="" goto Error 
if "%4"=="" if not "%5"=="" goto Error

echo Processing Script: %3

:: echo Check script file name matches script code: %3 = %~n3
find /C "SET @ScriptCode='%~n3'" %3 | find ": 1" > NUL

if errorlevel 1 (
	echo Variable @ScriptCode in the script '%~n3' does not match the file name '%3' [or @ScriptCode has extension .sql]
) else (
	if "%4"=="" (
		:: echo Check script not already executed
        sqlcmd -S %1 -d %2 -Q "IF EXISTS(SELECT * FROM 2kmm.ScriptUpdate WHERE ScriptCode='$(SQLCMDSCRIPTFILENAME)') PRINT N'Script $(SQLCMDSCRIPTFILENAME) already run'" -v SQLCMDSCRIPTFILENAME="%~n3" | find "Script %~n3 already run" > NUL
		if errorlevel 1 (
			:: echo Executing script
			sqlcmd -S %1 -d %2 -i %3 -b -v SQLCMDCURRDIR="%~dp0" SQLCMDSCRIPTPATH="%~dp3" SQLCMDSCRIPTFILENAME="%~n3" 
		) 
	) else (
		:: echo Check script not already executed (with Username and Password parameters)
		sqlcmd -S %1 -d %2 -U %4 -P %5 -Q "IF EXISTS(SELECT * FROM 2kmm.ScriptUpdate WHERE ScriptCode='$(SQLCMDSCRIPTFILENAME)') PRINT N'Script $(SQLCMDSCRIPTFILENAME) already run'" -v SQLCMDSCRIPTFILENAME="%~n3" | find "Script %~n3 already run" > NUL
		if errorlevel 1 (
			:: echo Executing Script
			sqlcmd -S %1 -d %2 -i %3 -b -U %4 -P %5 -v SQLCMDCURRDIR="%~dp0" SQLCMDSCRIPTPATH="%~dp3" SQLCMDSCRIPTFILENAME="%~n3"
		)
	)
)

if errorlevel 1 (
	echo Updating Interrupted.
	echo Failed to execute script: %3
)

goto :EOF

:Break
echo Script file '%3' not found
goto :EOF

:Error
echo Paramters missing calling program:
echo.
echo %0 ^<SERVER/SERVICE^> ^<DB_ALIAS^> ^<FILE_SCRIPT^> [^<DB_USER^> ^<PASSWORD^>]
goto :EOF