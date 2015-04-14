@echo off
REM xcopy /F /D /S /Y bin\* releases\%current_date%\bin\

REM -- Get the current date time in the format YYYY-MM-DD
set current_date=%DATE:~10,4%-%DATE:~4,2%-%DATE:~7,2%
set release_dir=releases\%current_date%\

REM -- Update with contents from bin folder and copy to release directory
lib\utils\echoc 2 Creating release binaries... %release_dir%
mkdir %release_dir%bin\
lib\utils\echoc 2 Copying \bin...
robocopy bin\ %release_dir%bin\ /MIR /XD bin\data

REM -- Create lib
lib\utils\echoc 2 Creating release lib...
mkdir releases\%current_date%\lib\
lib\ILMerge\ILMerge.exe /v4 /out:releases\%current_date%\lib\solumlib.dll /wildcards bin\solum.*.dll bin\solum.exe bin\RaptorDB*.dll bin\Handlebars.dll bin\Newtonsoft.Json.dll bin\NLog.dll bin\System.Threading.Tasks.Dataflow.dll

REM -- Git Add
echo.
set git_add=y
set /p git_add="Git add release? [y]/n "

if "%git_add:~0,1%" equ "y" (
	lib\utils\echoc 2 Adding...
	git add -Af %release_dir%	
) else (
	goto COMPLETED
)

REM -- Git Commmit
echo.
set git_commit=y
set /p git_commit="Git commit release? [y]/n "

if "%git_commit:~0,1%" equ "y" (
	lib\utils\echoc 2 Commiting...
	git commit -am "Creating release %current_date%"
) else (
	goto COMPLETED
)

REM -- Git Push
echo.
set git_commit=y
set /p git_commit="Git push release? [y]/n "

if "%git_commit:~0,1%" equ "y" (
	lib\utils\echoc 2 Pushing...
	git push
) else (
	goto COMPLETED
)	
	
:COMPLETED
echo.
lib\utils\echoc 2 Sucessfully created release %current_date%!