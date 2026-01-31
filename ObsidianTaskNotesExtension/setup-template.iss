; Inno Setup Script for ObsidianTaskNotesExtension
; This template is used by the BuildExeInstaller psake task
;
; CLSID from ObsidianTaskNotesExtension.cs: 25569750-3959-414b-891f-727b292ff830

#define AppVersion "0.0.1.0"
#define ExtensionName "ObsidianTaskNotesExtension"
#define DisplayName "Obsidian Task Notes"
#define DeveloperName "HeyItsGilbert"
#define CLSID "25569750-3959-414b-891f-727b292ff830"

[Setup]
AppId={{A8E5D7F3-9B2C-4E1A-8D6F-3C7B9E2A1D4F}}
AppName={#DisplayName}
AppVersion={#AppVersion}
AppPublisher={#DeveloperName}
AppPublisherURL=https://github.com/HeyItsGilbert/ObsidianTaskNotesExtension
AppSupportURL=https://github.com/HeyItsGilbert/ObsidianTaskNotesExtension/issues
AppUpdatesURL=https://github.com/HeyItsGilbert/ObsidianTaskNotesExtension/releases
DefaultDirName={autopf}\{#ExtensionName}
DefaultGroupName={#DisplayName}
DisableProgramGroupPage=yes
LicenseFile=..\..\LICENSE.txt
OutputDir=bin\Release\installer
OutputBaseFilename={#ExtensionName}-Setup-{#AppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
MinVersion=10.0.19041
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
; Source path is updated by BuildExeInstaller task for each platform
Source: "bin\Release\net9.0-windows10.0.26100.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#DisplayName}"; Filename: "{app}\{#ExtensionName}.exe"
Name: "{group}\Uninstall {#DisplayName}"; Filename: "{uninstallexe}"

[Registry]
; Register COM server for Command Palette discovery
Root: HKCU; Subkey: "SOFTWARE\Classes\CLSID\{{{#CLSID}}"; ValueType: string; ValueData: "{#ExtensionName}"; Flags: uninsdeletekey
Root: HKCU; Subkey: "SOFTWARE\Classes\CLSID\{{{#CLSID}}\LocalServer32"; ValueType: string; ValueData: """{app}\{#ExtensionName}.exe"" -RegisterProcessAsComServer"; Flags: uninsdeletekey

[Run]
; Optional: Register the extension after installation
Filename: "{app}\{#ExtensionName}.exe"; Parameters: "-RegisterProcessAsComServer"; Flags: nowait postinstall skipifsilent; Description: "Register extension with Command Palette"

[UninstallRun]
; Clean up COM registration on uninstall
Filename: "reg"; Parameters: "delete ""HKCU\SOFTWARE\Classes\CLSID\{{{#CLSID}}"" /f"; Flags: runhidden

[Code]
function InitializeSetup(): Boolean;
begin
  Result := True;
  // Check if PowerToys is installed (optional check)
  if not RegKeyExists(HKEY_CURRENT_USER, 'SOFTWARE\Classes\PowerToys') then
  begin
    if MsgBox('PowerToys may not be installed. This extension requires PowerToys Command Palette to function.' + #13#10 + #13#10 + 'Do you want to continue with the installation?', mbConfirmation, MB_YESNO) = IDNO then
    begin
      Result := False;
    end;
  end;
end;
