@echo off
javac DomainSettingsManager.java
if %errorlevel% equ 0 (
    java DomainSettingsManager
) else (
    echo Compilation failed!
    pause
) 