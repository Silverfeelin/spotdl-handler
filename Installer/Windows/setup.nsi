; SpotDL-Handler NSIS install script.
; Based on template by zmilojko.
; https://gist.github.com/zmilojko/3174804

!include "MUI.nsh"
!include StrRep.nsh

!define PRODUCT_NAME "SpotDL Handler"
!define PRODUCT_VERSION "1.1.2"
!define PRODUCT_PUBLISHER "Silverfeelin"
!define PRODUCT_WEB_SITE "https://www.github.com/Silverfeelin"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define PRODUCT_UNINST_ROOT_KEY "HKLM"

!define MUI_ABORTWARNING
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

!define BIN "..\..\SpotifyDownloader\bin\Release\netcoreapp2.1\publish"

; Wizard pages
!insertmacro MUI_PAGE_WELCOME

!define MUI_PAGE_HEADER_TEXT "Prerequisites"
!define MUI_PAGE_HEADER_SUBTEXT "This application will not run without a couple of prerequisites."
!define MUI_LICENSEPAGE_TEXT_TOP "Important information!"
!define MUI_LICENSEPAGE_TEXT_BOTTOM "$\n* If you have installed all the prerequisites, please continue."
!define MUI_LICENSEPAGE_BUTTON "Next >"
!insertmacro MUI_PAGE_LICENSE "License.rtf"

!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "English"

; Replace the constants bellow to hit suite your project
Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "SetupSpotdlHandler${PRODUCT_VERSION}.exe"
InstallDir "$PROGRAMFILES\SpotDL-Handler"
ShowInstDetails show
ShowUnInstDetails show

; Following lists the files you want to include, go through this list carefully!
Section "MainSection" SEC01
  ; SpotDL-Handler
  SetOutPath "$INSTDIR"
  SetOverwrite ifnewer
  File "${BIN}\*.dll"
  File "${BIN}\appsettings.json"
  File "${BIN}\SpotifyDownloader.runtimeconfig.json"
  File "${BIN}\configure.bat"
  
  IfFileExists "$INSTDIR\appsettings.user.json" skip_user save_user
  save_user:
  ; SpotDL-Handler user config
  nsJSON::Set /value `{}`
  ${StrReplaceV4} "$R0" "\" "\\" $MUSIC

  nsJSON::Set `output` `directory` /value `"$R0"`
  nsJSON::Set `output` `extension` /value `".m4a"`
  nsJSON::Serialize /format /file $INSTDIR\appsettings.user.json
  skip_user:
  
  ; Protocol
  WriteRegStr "HKCR" "spotdl" "" "spotdl protocol"
  WriteRegStr "HKCR" "spotdl" "URL Protocol" ""
  WriteRegStr "HKCR" "spotdl\DefaultIcon" "" "alert.exe,1"
  WriteRegStr "HKCR" "spotdl\shell\open\command" "" 'cmd.exe /C dotnet "$INSTDIR\SpotifyDownloader.dll" %1'
  
  ; Tampermonkey
SectionEnd

Section -Post
  ;Following lines will make uninstaller work - do not change anything, unless you really want to.
  WriteUninstaller "$INSTDIR\uninst.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
  
  ExecShell "open" "https://greasyfork.org/en/scripts/376669-spotify-web-download"
SectionEnd

; Replace the following strings to suite your needs
Function un.onUninstSuccess
  HideWindow
  MessageBox MB_ICONINFORMATION|MB_OK "Application was successfully removed from your computer. Please remember to uninstall the Tampermonkey script from your browser."
FunctionEnd

Function un.onInit
  MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "Are you sure you want to remove SpotDL Handler?" IDYES +2
  Abort
FunctionEnd

; Remove any file that you have added above - removing uninstallation and folders last.
; Note: if there is any file changed or added to these folders, they will not be removed. Also, parent folder (which in my example 
; is company name ZWare) will not be removed if there is any other application installed in it.
Section Uninstall
  Delete "$INSTDIR\*.dll"
  Delete "$INSTDIR\appsettings.json"
  Delete "$INSTDIR\appsettings.user.json"
  Delete "$INSTDIR\SpotifyDownloader.runtimeconfig.json"
  Delete "$INSTDIR\configure.bat"
  Delete "$INSTDIR\uninst.exe"

  RMDir "$INSTDIR"
  RMDir "$INSTDIR\.."

  DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
  
  ; Protocol
  DeleteRegKey "HKCR" "spotdl"
  DeleteRegKey "HKCR" "spotdl"
  DeleteRegKey "HKCR" "spotdl\DefaultIcon"
  DeleteRegKey "HKCR" "spotdl\shell\open\command"

  SetAutoClose true
SectionEnd