SetCompressor /SOLID lzma

!include "nsProcess.nsh"
;--------------------------------
;Include Modern UI

  !include "MUI2.nsh"

   ; include for some of the windows messages defines
   !include "winmessages.nsh"

;--------------------------------
;General
  !include "x64.nsh"
  ;Name and file
  Name "PeaRoxy Client for Windows v0.9.5.0"
  OutFile "..\Packages\PeaRoxyClient-Win-v0.9.5.exe"

  ;Default installation folder
  InstallDir "$PROGRAMFILES\PeaRoxy"

  ;Get installation folder from registry if available
  InstallDirRegKey HKLM "Software\PeaRoxy" ""

  ;Request application privileges for Windows Vista
  RequestExecutionLevel admin ;Require admin rights on NT6+ (When UAC is turned on)

  !include LogicLib.nsh

  BrandingText "PeaRoxy"

;--------------------------------
;Interface Configuration
  !define MUI_ICON "Setup.ico"
  !define MUI_HEADERIMAGE
  !define MUI_HEADERIMAGE_BITMAP "Header.bmp" ; optional
  !define MUI_WELCOMEFINISHPAGE_BITMAP "Left.bmp"
  !define MUI_ABORTWARNING

;--------------------------------
;Pages

  ;!insertmacro MUI_PAGE_LICENSE "GPL.txt"
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
    !define MUI_FINISHPAGE_RUN "$INSTDIR\PeaRoxy.Windows.WPFClient.exe"
    !define MUI_FINISHPAGE_RUN_TEXT "Launch PeaRoxy Client"
  !insertmacro MUI_PAGE_FINISH

  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  !insertmacro MUI_UNPAGE_FINISH

;--------------------------------
;Languages

  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Installer Sections

Section "Main Application" SecMain
  ${nsProcess::FindProcess} "PeaRoxy.Windows.WPFClient.exe" $R0
  StrCmp $R0 0 0 +7
     ExecWait '"$INSTDIR\PeaRoxy.Windows.WPFClient.exe" /quit'
     Sleep 20000
     ${nsProcess::FindProcess} "PeaRoxy.Windows.WPFClient.exe" $R0
     StrCmp $R0 0 0 +3
        ${nsProcess::KillProcess} "PeaRoxy.Windows.WPFClient.exe" $R0
        Sleep 5000
  ${nsProcess::Unload}

  SectionIn RO
  SetOutPath "$INSTDIR"
  ;File "GPL.txt"
  File "..\bin\WPFClient\*"

  CreateDirectory "$INSTDIR\HTTPSCerts"
  SetOutPath "$INSTDIR\HTTPSCerts"
  File "..\bin\WPFClient\HTTPSCerts\*"

  CreateDirectory "$INSTDIR\TAPDriver"
  SetOutPath "$INSTDIR\TAPDriver"
  File "..\bin\WPFClient\TAPDriver\*"

  SetOutPath "$INSTDIR"

  ;Store installation folder
  WriteRegStr HKLM "Software\PeaRoxy" "InstallDir" $INSTDIR
  WriteRegStr HKLM "Software\PeaRoxy" "Version" "0.9.0"

  CreateShortCut "$SMPROGRAMS\PeaRoxy Client.lnk" "$INSTDIR\PeaRoxy.Windows.WPFClient.exe"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "PeaRoxy Client" '"$INSTDIR\PeaRoxy.Windows.WPFClient.exe" /autoRun'

  ;Create uninstaller
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\PeaRoxy Client 0.9.5 for Windows" "DisplayName" "PeaRoxy Client 0.9.5 for Windows (remove only)"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\PeaRoxy Client 0.9.5 for Windows" "UninstallString" "$INSTDIR\Uninstall PeaRoxy.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\PeaRoxy Client 0.9.5 for Windows" "Publisher" "PeaRoxy (pearoxy.com)"

  WriteUninstaller "$INSTDIR\Uninstall PeaRoxy.exe"


SectionEnd
Function .onInit
	UserInfo::GetAccountType
	pop $0
	${If} $0 != "admin" ;Require admin rights on NT4+
		MessageBox mb_iconstop "Administrator rights required!"
		SetErrorLevel 740 ;ERROR_ELEVATION_REQUIRED
		Quit
	${EndIf}
FunctionEnd
;--------------------------------
;Descriptions

  ;Language strings
  LangString DESC_SecMain ${LANG_ENGLISH} "PeaRoxy WPF Client for Windows"

  ;Assign language strings to sections
  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecMain} $(DESC_SecMain)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
;Uninstaller Section

Section "Uninstall"
  ExecWait '"$INSTDIR\PeaRoxy.Windows.WPFClient.exe" /quit'
  RMDir /r "$INSTDIR\*.*"
  RMDir "$INSTDIR"
  Delete "$SMPROGRAMS\PeaRoxy Client.lnk"

  DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "PeaRoxy Client"
  DeleteRegKey /ifempty HKLM "Software\PeaRoxy"
  DeleteRegKey HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PeaRoxy Client 0.9.5 for Windows"
SectionEnd
Function un.onInit
	SetRebootFlag true 
	UserInfo::GetAccountType
	pop $0
	${If} $0 != "admin" ;Require admin rights on NT4+
		MessageBox mb_iconstop "Administrator rights required!"
		SetErrorLevel 740 ;ERROR_ELEVATION_REQUIRED
		Quit
	${EndIf}
FunctionEnd