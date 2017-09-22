# 起点となる bin フォルダー
$root = Join-Path $PSScriptRoot "Omawari\bin"

# 入力フォルダー
$src = Join-Path $root "Release"

# 出力フォルダー
$version = (Get-ItemProperty (Join-Path $src "Omawari.exe")).VersionInfo.FileVersion
$dest = Join-Path $root Omawari-$version.zip

# ZIP で圧縮
Compress-Archive -Path $src/* -DestinationPath $dest

# フォルダーを開く
Invoke-Item $root