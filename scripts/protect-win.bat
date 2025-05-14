@echo off
set PROTECTION_STUDIO="C:\Program Files (x86)\Guardant\Software Licensing Kit\bin\protection_studio.exe"
%PROTECTION_STUDIO% --project ./guardant_config.pprx --output %1\Protected