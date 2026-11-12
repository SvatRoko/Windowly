# Windowly

Windowly is an open-source Windows 11 utility that remembers window size and position per application. It runs in the system tray and restores each new eligible window once, leaving you free to move it afterward.

## Use

Extract the entire self-contained win-x64 ZIP into a permanent folder and run `Windowly.exe`; no separate .NET installation is required. Open Notepad, position and resize it, then right-click the Windowly tray icon and select **Remember current window**. Close and reopen Notepad to try restoration. Capture again to update its rule.

**Settings** opens a customization interface with **Window layouts**, **Taskbar**, and **Notification panel** navigation. Window layouts lists saved applications with geometry and monitor details, supports remembering the previous application window and deleting rules, and offers **Start Windowly at sign-in**. Taskbar and Notification panel are preview pages only; they do not change Windows behavior. Closing Settings keeps Windowly running; **Exit** stops it.

## Build and package

Requires Windows 11 x64 and the .NET 10 SDK.

```powershell
dotnet build src/Windowly/Windowly.csproj
dotnet test tests/Windowly.Tests/Windowly.Tests.csproj
dotnet run --project src/Windowly/Windowly.csproj
powershell -ExecutionPolicy Bypass -File scripts/release.ps1
```

The release script publishes a self-contained Release build, includes README and LICENSE, and checks required ZIP entries. Output: `artifacts/Windowly-win-x64.zip`.

## Storage and sign-in

Rules are stored in `%LOCALAPPDATA%\Windowly\settings.json`. Writes replace the previous file; invalid JSON is preserved in a `.corrupt-*` recovery copy.

Sign-in startup is disabled by default. Enabling it creates only the `Windowly` value in `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`, pointing to the quoted executable path. After moving the portable folder, enable the option again from its new location.

## Limitations

- Elevated or protected windows may deny access. Shared host processes and Explorer are skipped; standard owned windows, dialogs, menus, desktop and taskbar are excluded. Custom dialogs may be indistinguishable from main windows.
- Apps that enforce their own placement can override restoration. Initialization retries are bounded; there is no continuous polling.
- Disconnected monitors fall back to the primary display. Geometry is scaled and clamped to its work area.
- No service, administrator privileges, telemetry, network calls, installer, or automatic updates.

## Verification

The initial automated build, test, and publish attempts were blocked by the development environment's SDK/access restrictions. The user subsequently reported running and testing the application; no detailed results have been recorded. CI builds and tests on Windows.

Manual checks to record: capture Notepad through the tray; reopen it; move it after restoration and switch focus; test maximized restore bounds, negative monitor coordinates, DPI changes and disconnected displays; delete a rule; verify Exit; deliberately enable startup and verify after sign-in.

## Remove

Disable **Start at sign-in**, choose **Exit**, and delete the extracted folder and `%LOCALAPPDATA%\Windowly`. If the executable has already been deleted, remove only the `Windowly` Run registry value manually.

## Contribute and license

Bug reports and focused pull requests are welcome. Include Windows version, reproduction steps, and expected versus actual behavior. Licensed under [MIT](LICENSE), copyright Windowly contributors.
