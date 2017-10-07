cd "$PSScriptRoot" 

# tools フォルダーを空にする
remove-item ".\tools" -recurse -force

new-item ".\tools" -ItemType "directory"

# Release ビルドを tools フォルダーへコピー
# $source = "..\bin\Release" 
#robocopy $source .\tools /MIR

# 実行ファイルのシミングを防止
#$files = get-childitem ".\tools" -include *.exe -recurse
#foreach ($file in $files) {
  #generate an ignore file
#  New-Item "$file.ignore" -type file -force | Out-Null
#}

# インストールスクリプトなどをコピー
copy .\chocolateyinstall.ps1 .\tools\chocolateyinstall.ps1
#copy .\LICENSE.txt .\tools\LICENSE.txt
#copy .\VERIFICATION.txt .\tools\VERIFICATION.txt

# .nuspec のバージョンを書き換え
$version = (Get-ItemProperty ..\bin\Release\Omawari.exe).VersionInfo.FileVersion
$(Get-Content .\omawari.nuspec.sample) -replace "<version>.+?</version>", "<version>$version</version>" > .\omawari.nuspec

# .nupkg を再生成
del *.nupkg
choco pack

# .nupkg のテスト
choco uninstall "Omawari"
choco install "Omawari" -fdv -y  -s ".\" 