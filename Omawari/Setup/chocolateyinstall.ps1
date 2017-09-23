$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$packageName = "Omawari"
$target = Join-Path $toolsDir "$($packageName).exe"
$shortcut = [Environment]::GetFolderPath("Desktop")
$shortcut = Join-Path $shortcut "Omawari.lnk"
Install-ChocolateyShortcut -ShortcutFilePath $shortcut -TargetPath $target