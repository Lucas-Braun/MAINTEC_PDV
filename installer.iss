; MAINTEC PDV - Inno Setup Script
; Para compilar: abra este arquivo no Inno Setup e clique em Build > Compile

#define MyAppName "MAINTEC PDV"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "MAINTEC"
#define MyAppExeName "PDV.exe"
#define MyAppURL "https://github.com/Lucas-Braun/MAINTEC_PDV"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
; Icone do instalador (descomente e ajuste se tiver um .ico)
; SetupIconFile=src\PDV.App\Assets\icon.ico
OutputDir=installer_output
OutputBaseFilename=MAINTEC_PDV_Setup_{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes
; Tamanho minimo de tela
MinVersion=10.0

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na &Area de Trabalho"; GroupDescription: "Atalhos:"
Name: "startupicon"; Description: "Iniciar com o &Windows"; GroupDescription: "Atalhos:"

[Files]
; Copia tudo da pasta publish
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Desinstalar {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: startupicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Abrir {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Limpa dados locais na desinstalacao (opcional - descomente se quiser)
; Type: filesandordirs; Name: "{localappdata}\PDV"
