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
  Name "PeaRoxy Client for Windows v${VERSION}"
  OutFile "..\Binaries\PeaRoxyClient-Win-v${VERSION}.exe"

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
  !define MUI_ICON "PeaRoxy Setup.ico"
  !define MUI_HEADERIMAGE
  !define MUI_HEADERIMAGE_BITMAP "PeaRoxy Header.bmp" ; optional
  !define MUI_WELCOMEFINISHPAGE_BITMAP "PeaRoxy Left.bmp"
  !define MUI_ABORTWARNING

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_LICENSE "PeaRoxy LICENSE.txt"
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
  
  CreateDirectory "$INSTDIR\TAPDriver\x64"
  SetOutPath "$INSTDIR\TAPDriver\x64"
  File "..\bin\WPFClient\TAPDriver\x64\*"
  
  CreateDirectory "$INSTDIR\TAPDriver\x86"
  SetOutPath "$INSTDIR\TAPDriver\x86"
  File "..\bin\WPFClient\TAPDriver\x86\*"

  SetOutPath "$INSTDIR"

  ;Store installation folder
  WriteRegStr HKLM "Software\PeaRoxy" "InstallDir" $INSTDIR
  WriteRegStr HKLM "Software\PeaRoxy" "Version" ${VERSION}

  CreateShortCut "$SMPROGRAMS\PeaRoxy Client.lnk" "$INSTDIR\PeaRoxy.Windows.WPFClient.exe"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "PeaRoxy Client" '"$INSTDIR\PeaRoxy.Windows.WPFClient.exe" /autoRun'

  ;Create uninstaller
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\PeaRoxy Client for Windows" "DisplayName" "PeaRoxy Client ${VERSION} for Windows (remove only)"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\PeaRoxy Client for Windows" "UninstallString" "$INSTDIR\Uninstall PeaRoxy.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\PeaRoxy Client for Windows" "Publisher" "PeaRoxy (pearoxy.com)"

  WriteUninstaller "$INSTDIR\Uninstall PeaRoxy.exe"


SectionEnd
Function .onInit
	UserInfo::GetAccountType
	pop $0
	${If} $0 != "admin" ;Require admin rights on NT4+
		MessageBox MB_ICONSTOP "Administrator rights required! Try running this file as Admin."
		SetErrorLevel 740 ;ERROR_ELEVATION_REQUIRED
		Quit
	${EndIf}
	CALL CheckVC2010Redist
	CALL CheckNetFramework4Client
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
  DeleteRegKey HKLM "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PeaRoxy Client for Windows"
SectionEnd
Function un.onInit
	SetRebootFlag true 
	UserInfo::GetAccountType
	pop $0
	${If} $0 != "admin" ;Require admin rights on NT4+
		MessageBox MB_ICONSTOP "Administrator rights required! Try running this file as Admin."
		SetErrorLevel 740 ;ERROR_ELEVATION_REQUIRED
		Quit
	${EndIf}
FunctionEnd

Function CheckVC2010Redist
    ClearErrors
    ${If} ${RunningX64}
          ReadRegDWORD $0 HKLM "SOFTWARE\Wow6432Node\Microsoft\VisualStudio\10.0\VC\VCRedist\x86" "Installed"
    ${Else}
          ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\VisualStudio\10.0\VC\VCRedist\x86" "Installed"
    ${EndIf}
    IfErrors NotDetected
    ${If} $0 == 1
        Return
    ${Else}
    NotDetected:
        MessageBox MB_ICONSTOP "Microsoft Visual C++ 2010 Redistributable Package is not installed, we will now redirect you to the Microsoft Website to download it."
        ExecShell open "http://www.microsoft.com/en-us/download/details.aspx?id=5555"
        Quit
    ${EndIf}
FunctionEnd

Function CheckNetFramework4Client
    ClearErrors
    ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client" "Install"
    IfErrors NotDetected
    ${If} $0 == 1
        Return
    ${Else}
        ClearErrors
        ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" "Install"
        IfErrors NotDetected
        ${If} $0 == 1
              Return
        ${Else}
        NotDetected:
            MessageBox MB_ICONSTOP "Microsoft .NET Framework 4 is not installed, we will now redirect you to the Microsoft Website to download it."
            ExecShell open "http://www.microsoft.com/en-us/download/details.aspx?id=24872"
            Quit
        ${EndIf}
    ${EndIf}
FunctionEnd