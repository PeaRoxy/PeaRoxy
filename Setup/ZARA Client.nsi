SetCompress off

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
  Name "ZARA for Windows v${VERSION}"
  OutFile "..\Binaries\ZARA-Win-v${VERSION}_StandAlone.exe"

  ;Default installation folder
  InstallDir "$TEMP\ZARA v${VERSION}"

  ;Request application privileges for Windows Vista
  RequestExecutionLevel admin ;Require admin rights on NT6+ (When UAC is turned on)

  !include LogicLib.nsh

  BrandingText "ZARA"

;--------------------------------
;Interface Configuration
  !define MUI_ICON "ZARA.ico"
  !define MUI_ABORTWARNING

;--------------------------------
;Pages


;--------------------------------
;Languages

  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Installer Sections

Section "Main Application" SecMain
  ${nsProcess::FindProcess} "ZARA.exe" $R0
  StrCmp $R0 0 0 +2
     ${nsProcess::KillProcess} "ZARA.exe" $R0
     
  ${nsProcess::Unload}

  SectionIn RO
  SetOutPath "$INSTDIR"
  File  /r /x "*.pdb" /x "ZipExcludeList.txt" /x "*.crt" /x "PeaRoxy.Windows.*.dll" /x "PeaRoxy.ClientLibrary.dll" /x "CircularProgressBar.dll" /x "PeaRoxy.CommonLibrary.dll" /x "PeaRoxy.CoreProtocol.dll" /x "VDialog.dll" /x "WinFormAnimation.dll" /x "*.xml" /x "*.vshost.exe" "..\bin\ZARA\*"

  ExecShell open "$INSTDIR\ZARA.exe"
  Quit
SectionEnd

Function .onInit
        SetSilent silent
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