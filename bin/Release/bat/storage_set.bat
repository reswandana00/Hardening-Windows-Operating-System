@echo off
echo Converting C: drive to NTFS if needed...
convert C: /FS:NTFS /NoSecurity
echo Conversion complete. Please restart your system.
