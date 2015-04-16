@echo off

REM -- Pull the YikYak Library from github
utils\echoc 2 Initializing git submodules...
git submodule init
utils\echoc 2 Updating git submodules...
git submodule update

utils\echoc 2 SUCCESS: All steps completed successfully!