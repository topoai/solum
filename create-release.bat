@echo off
REM -- Get the current date time in the format YYYY-MM-DD
set current_date=%DATE:~10,4%-%DATE:~4,2%-%DATE:~7,2%

REM -- Update with contents from bin folder and copy to release directory
mkdir releases\%current_date%\bin\
REM xcopy /F /D /S /Y bin\* releases\%current_date%\bin\
robocopy /MIR bin releases\%current_date%\bin

REM -- Create lib
mkdir releases\%current_date%\lib\
lib\ILMerge\ILMerge.exe /v4 /out:releases\%current_date%\lib\solumlib.dll /wildcards bin\solum.*.dll bin\solum.exe bin\RaptorDB*.dll bin\Handlebars.dll bin\Newtonsoft.Json.dll bin\NLog.dll bin\System.Threading.Tasks.Dataflow.dll

REM -- Prompt user to commit and push to git
echo.
set git_commit=y
set /p git_commit="Git add, commit, and push? [y]/n "

if "%git_commit:~0,1%" equ "y" (
	git add -Af releases\%current_date%\*
	git commit -am "Creating release %current_date%"
	git push
)