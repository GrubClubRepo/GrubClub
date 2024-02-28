@echo off
call _update.bat WINXP SupperClub
if "%ERRORLEVEL%" == "1" (echo There was an Error executing a script)
pause