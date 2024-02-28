@echo off
call _update.bat "winsrv105.pleskdns.co.uk,51350" SupperClub supperclub_user sN56j4oU1o
if "%ERRORLEVEL%" == "1" (exit 1) else (exit 0)