@echo off
reg add "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Network" /v MinPwdLen /t REG_DWORD /d 8 /f
cscript //nologo reg_minPasswdLength.vbs
cscript //nologo blank_password.vbs
python vnc-nopasswd.py
pause
