@echo off
cd /d "%~dp0"

call "mlagents-env\Scripts\activate"
if errorlevel 1 goto :error

start "" http://localhost:6006
if errorlevel 1 goto :error

echo.
echo Reload once tensorboard is running to view (if it doesn't reload automatically).
echo.

tensorboard --logdir results
if errorlevel 1 goto :error

exit /b 0

:error
echo See error above
pause
exit /b 1

:: this file was 100% HAND WRITTEN!