cd "$PSScriptRoot" 

# tools �t�H���_�[����ɂ���
remove-item ".\tools" -recurse -force

new-item ".\tools" -ItemType "directory"

# Release �r���h�� tools �t�H���_�[�փR�s�[
# $source = "..\bin\Release" 
#robocopy $source .\tools /MIR

# ���s�t�@�C���̃V�~���O��h�~
#$files = get-childitem ".\tools" -include *.exe -recurse
#foreach ($file in $files) {
  #generate an ignore file
#  New-Item "$file.ignore" -type file -force | Out-Null
#}

# �C���X�g�[���X�N���v�g�Ȃǂ��R�s�[
copy .\chocolateyinstall.ps1 .\tools\chocolateyinstall.ps1
#copy .\LICENSE.txt .\tools\LICENSE.txt
#copy .\VERIFICATION.txt .\tools\VERIFICATION.txt

# .nuspec �̃o�[�W��������������
$version = (Get-ItemProperty ..\bin\Release\Omawari.exe).VersionInfo.FileVersion
$(Get-Content .\omawari.nuspec.sample) -replace "<version>.+?</version>", "<version>$version</version>" > .\omawari.nuspec

# .nupkg ���Đ���
del *.nupkg
choco pack

# .nupkg �̃e�X�g
choco uninstall "Omawari"
choco install "Omawari" -fdv -y  -s ".\" 