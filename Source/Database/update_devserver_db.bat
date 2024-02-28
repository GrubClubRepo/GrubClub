@echo off
call _update.bat .\JEAN_LAPTOP SupperClub sa sql
if "%ERRORLEVEL%" == "1" (exit 1) else (exit 0)