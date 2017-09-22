# �N�_�ƂȂ� bin �t�H���_�[
$root = Join-Path $PSScriptRoot "Omawari\bin"

# ���̓t�H���_�[
$src = Join-Path $root "Release"

# �o�̓t�H���_�[
$version = (Get-ItemProperty (Join-Path $src "Omawari.exe")).VersionInfo.FileVersion
$dest = Join-Path $root Omawari-$version.zip

# ZIP �ň��k
Compress-Archive -Path $src/* -DestinationPath $dest

# �t�H���_�[���J��
Invoke-Item $root