@echo off
:: server name must be enclosed in ""
call _update.bat "winsrv107.pleskdns.co.uk,51350" SupperClub supperclub_user 82LsN33j#123
if "%ERRORLEVEL%" == "1" (exit 1) else (exit 0)