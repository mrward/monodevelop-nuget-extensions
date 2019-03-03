./powershell-host-publish.sh
cp -r bin/PowerShellConsoleHost/publish/* bin/PowerShellConsoleHost/
mono /Applications/Visual\ Studio.app/Contents/Resources/lib/monodevelop/bin/vstool.exe setup pack bin/MonoDevelop.PackageManagement.Extensions.dll