@echo off
rem Shows which server plugins have a newer release on GitHub (list: build\tools\plugins.json).
node "%~dp0build\tools\check-updates.mjs"
pause
