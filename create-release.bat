@echo off
REM -- Get the current date time in the format YYYY-MM-DD
set current_date=%DATE:~10,4%-%DATE:~4,2%-%DATE:~7,2%

REM -- Update with contents from bin folder and copy to release directory
mkdir releases\%current_date%\
xcopy /F /D /S /Y bin\* releases\%current_date%\

REM -- Prompt user to 
echo.
set git_commit=y
set /p git_commit="Git add, commit, and push? [y]/n "

if "%git_commit:~0,1%" equ "y" (
	git add -Af releases\%current_date%\*
	git commit -am "Creating release %current_date%"
	git push	
)