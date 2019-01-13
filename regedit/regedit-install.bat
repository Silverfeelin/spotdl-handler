@ECHO OFF

ECHO Enter path to SpotifyDownloader.dll
set /p path=

%SystemRoot%\System32\reg.exe ADD HKEY_CLASSES_ROOT\spotdl /ve /d "spotdl protocol" /f >nul
%SystemRoot%\System32\reg.exe ADD HKEY_CLASSES_ROOT\spotdl /v "URL Protocol" /d "\"\"" /f >nul
%SystemRoot%\System32\reg.exe ADD HKEY_CLASSES_ROOT\spotdl\DefaultIcon /ve /d "alert.exe,1" /f >nul
%SystemRoot%\System32\reg.exe ADD HKEY_CLASSES_ROOT\spotdl\shell\open\command /ve /d "cmd.exe /C dotnet \"%path%\" %%1" /f >nul

PAUSE