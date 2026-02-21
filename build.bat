@echo off
:: Setup Paths
SET G_DATA="C:\Program Files (x86)\Steam\steamapps\common\REPO\REPO_Data\Managed"
SET G_BEPIN="C:\Users\gil\AppData\Roaming\com.kesomannen.gale\repo\profiles\Default\BepInEx\core"
SET CSC="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

echo Compiling VoiceFix.dll...

:: /noconfig and /nostdlib+ ensure total isolation from Windows DLLs
%CSC% /noconfig /nostdlib+ /target:library /out:PhotonVoiceFix.dll ^
/r:%G_DATA%\mscorlib.dll ^
/r:%G_DATA%\netstandard.dll ^
/r:%G_DATA%\System.dll ^
/r:%G_DATA%\System.Core.dll ^
/r:%G_DATA%\UnityEngine.dll ^
/r:%G_DATA%\UnityEngine.CoreModule.dll ^
/r:%G_DATA%\PhotonRealtime.dll ^
/r:%G_DATA%\PhotonUnityNetworking.dll ^
/r:%G_DATA%\Photon3Unity3D.dll ^
/r:%G_DATA%\PhotonVoice.dll ^
/r:%G_DATA%\PhotonVoice.API.dll ^
/r:%G_BEPIN%\BepInEx.dll ^
/r:%G_BEPIN%\0Harmony.dll ^
VoiceFix.cs

if %errorlevel% equ 0 (
    echo.
    echo BUILD SUCCESSFUL!
    echo Copying to Gale plugins...
    copy /y PhotonVoiceFix.dll "C:\Users\gil\AppData\Roaming\com.kesomannen.gale\repo\profiles\Default\BepInEx\plugins\"
) else (
    echo.
    echo BUILD FAILED. Check errors above.
)
pause