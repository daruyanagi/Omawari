using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;

namespace System.IO
{
    public static class PathHelper
    {
        public static FileSystemPath ToFileSystemPath(this DirectoryInfo dir) => new FileSystemPath(dir.FullName);
        public static FileSystemPath ToFileSystemPath(this FileInfo file) => new FileSystemPath(file.FullName);
        public static FileSystemPath ToFileSystemPath(this string path) => new FileSystemPath(path);
    }

    public class FileSystemPath
    {
        public FileSystemPath(string path)
        {
            Internal = Path.GetFullPath(path);
        }

        public override string ToString() => Internal;
        public override int GetHashCode() => Internal.GetHashCode();
        public override bool Equals(object obj)
        {
            var target = obj as FileSystemPath;
            if (target == null) return false;
            return target.Equals(target.Internal == Internal);
        }

        public string Internal { get; private set; } = default(string);

        public bool HasExtension => Path.HasExtension(Resolve().ToString());
        public bool IsDirectory => Directory.Exists(Resolve().ToString());
        public bool IsFile => File.Exists(Resolve().ToString());
        public bool Exists => IsDirectory || IsFile;
        public bool NotExists => !Exists;
        public FileSystemPath Root => Path.GetPathRoot(Resolve().ToString()).ToFileSystemPath();
        public FileSystemPath Parent => Path.GetDirectoryName(Resolve().ToString()).ToFileSystemPath();
        public string Name => GetFileName();
        public string NameWithoutExtension => GetFileNameWithoutExtension();
        public string Extension => GetExtension();

        public FileSystemPath Resolve() => Path.GetFullPath(Internal).ToFileSystemPath();

        public IEnumerable<FileSystemPath> GetSiblings()
        {
            var parent = Parent;
            if (IsFile) return Parent.EnumerateFiles().Where(_ => _.Name != Name);
            else if (IsDirectory) return Parent.EnumerateDirectories().Where(_ => _.Name != Name);
            else throw new FileNotFoundException("Not found File or Directory", Internal);
        }

        public void AppendAllLines(IEnumerable<string> contents) => File.AppendAllLines(Internal, contents);
        public void AppendAllLines(IEnumerable<string> contents, Encoding encoding) => File.AppendAllLines(Internal, contents, encoding);
        public void AppendAllText(string contents) => File.AppendAllText(Internal, contents);
        public void AppendAllText(string contents, Encoding encoding) => File.AppendAllText(Internal, contents, encoding);
        public StreamWriter AppendText(string contents) => File.AppendText(Internal);
        public void Copy(string destFileName, bool overwrite = false) => File.Copy(Internal, destFileName, overwrite);
        public FileStream CreateFile(string destFileName) => File.Create(Internal);
        public FileStream CreateFile(int bufferSize) => File.Create(Internal, bufferSize);
        public FileStream CreateFile(int bufferSize, FileOptions options) => File.Create(Internal, bufferSize, options);
        public StreamWriter CreateText(int bufferSize, FileOptions options) => File.CreateText(Internal);
        public void DecryptFile() => File.Decrypt(Internal);
        public void EncryptFile() => File.Encrypt(Internal);
        public FileAttributes GetAttributes() => File.GetAttributes(Internal);
        public FileStream OpenFile(FileMode mode) => File.Open(Internal, mode);
        public FileStream OpenFile(FileMode mode, FileAccess access) => File.Open(Internal, mode, access);
        public FileStream OpenFile(FileMode mode, FileAccess access, FileShare share) => File.Open(Internal, mode, access, share);
        public FileStream OpenFileAsReadOnly() => File.OpenRead(Internal);
        public StreamReader OpenTextFile() => File.OpenText(Internal);
        public FileStream OpenFileToWrite() => File.OpenWrite(Internal);
        public byte[] ReadAllBytes() => File.ReadAllBytes(Internal);
        public string[] ReadAllLines() => File.ReadAllLines(Internal);
        public string[] ReadAllLines(Encoding encoding) => File.ReadAllLines(Internal, encoding);
        public string ReadAllText() => File.ReadAllText(Internal);
        public string ReadAllText(Encoding encoding) => File.ReadAllText(Internal, encoding);
        public void SetAttributes(FileAttributes fileAttributes) => File.SetAttributes(Internal, fileAttributes);
        public void WriteAllBytes(byte[] bytes) => File.WriteAllBytes(Internal, bytes);
        public void WriteAllLines(string[] contents) => File.WriteAllLines(Internal, contents);
        public void WriteAllText(string contents) => File.WriteAllText(Internal, contents);

        public void CreateDirectory() => Directory.CreateDirectory(Resolve().ToString());
        public void CreateDirectory(DirectorySecurity directorySecurity) => Directory.CreateDirectory(Resolve().ToString(), directorySecurity);
        // public string GetDirectoryRoot() => Directory.GetDirectoryRoot(path);

        public IEnumerable<FileSystemPath> EnumerateDirectories()
            => Directory.EnumerateDirectories(Internal).Select(_ => _.ToFileSystemPath());
        public IEnumerable<FileSystemPath> EnumerateDirectories(string searchPattern)
            => Directory.EnumerateDirectories(Internal, searchPattern).Select(_ => _.ToFileSystemPath());
        public IEnumerable<FileSystemPath> EnumerateDirectories(string searchPattern, SearchOption searchOption)
            => Directory.EnumerateDirectories(Internal, searchPattern, searchOption).Select(_ => _.ToFileSystemPath());

        public IEnumerable<FileSystemPath> EnumerateFiles()
            => Directory.EnumerateFiles(Internal).Select(_ => _.ToFileSystemPath());
        public IEnumerable<FileSystemPath> EnumerateFiles(string searchPattern)
            => Directory.EnumerateFiles(Internal, searchPattern).Select(_ => _.ToFileSystemPath());
        public IEnumerable<FileSystemPath> EnumerateFiles(string searchPattern, SearchOption searchOption)
            => Directory.EnumerateFiles(Internal, searchPattern, searchOption).Select(_ => _.ToFileSystemPath());

        public IEnumerable<FileSystemPath> EnumerateFileSystemEntries()
            => Directory.EnumerateFileSystemEntries(Internal).Select(_ => _.ToFileSystemPath());
        public IEnumerable<FileSystemPath> EnumerateFileSystemEntries(string searchPattern)
            => Directory.EnumerateFileSystemEntries(Internal, searchPattern).Select(_ => _.ToFileSystemPath());
        public IEnumerable<FileSystemPath> EnumerateFileSystemEntries(string searchPattern, SearchOption searchOption)
            => Directory.EnumerateFileSystemEntries(Internal, searchPattern, searchOption).Select(_ => _.ToFileSystemPath());

        public void Delete(bool recursive)
        {
            if (IsFile) File.Delete(Internal);
            else if (IsDirectory) Directory.Delete(Internal, recursive);
            else throw new FileNotFoundException("Not found File or Directory", Internal);
        }

        public void Move(FileSystemPath dest) => Move(dest.Internal);
        public void Move(string dest)
        {
            if (IsFile) File.Move(Internal, dest);
            else if (IsDirectory) Directory.Move(Internal, dest);
            else throw new FileNotFoundException("Not found File or Directory", Internal);
        }

        public void ReplaceFile(FileSystemPath dest, FileSystemPath backup) => ReplaceFile(dest.Internal, backup.Internal);
        public void ReplaceFile(string dest, string backup) => File.Replace(Internal, dest, backup);

        public string GetExtension() => Path.GetExtension(Resolve().ToString());
        public string GetFileName() => Path.GetFileName(Resolve().ToString());
        public string GetFileNameWithoutExtension() => Path.GetFileNameWithoutExtension(Resolve().ToString());

        public FileSystemPath ChangeExtension(string extension) => Path.ChangeExtension(Internal, extension).ToFileSystemPath();
        public FileSystemPath Combine(string path2) => Path.Combine(Internal, path2).ToFileSystemPath();
        public FileSystemPath Combine(FileSystemPath path2) => Path.Combine(Internal, path2.Internal).ToFileSystemPath();
        public FileSystemPath GetRandom() => Path.GetRandomFileName().ToFileSystemPath();
        public FileSystemPath GetTemp() => Path.GetTempFileName().ToFileSystemPath();
        public FileSystemPath GetTempFolder() => Path.GetTempPath().ToFileSystemPath();
        public FileSystemPath GetAdminToolsFolder() => Environment.GetFolderPath(Environment.SpecialFolder.AdminTools).ToFileSystemPath();
        public FileSystemPath GetApplicationDataFolder() => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToFileSystemPath();
        public FileSystemPath GetCDBurningFolder() => Environment.GetFolderPath(Environment.SpecialFolder.CDBurning).ToFileSystemPath();
        public FileSystemPath GetCommonAdminToolsFolder() => Environment.GetFolderPath(Environment.SpecialFolder.CommonAdminTools).ToFileSystemPath();
        public FileSystemPath GetCommonApplicationDataFolder() => Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData).ToFileSystemPath();
        public FileSystemPath GetCommonDesktopFolder() => Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory).ToFileSystemPath();
        public FileSystemPath GetCommonDocumentsFolder() => Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments).ToFileSystemPath();
        public FileSystemPath GetCommonMusicFolder() => Environment.GetFolderPath(Environment.SpecialFolder.CommonMusic).ToFileSystemPath();
        public FileSystemPath GetCommonOemLinksFolder() => Environment.GetFolderPath(Environment.SpecialFolder.CommonOemLinks).ToFileSystemPath();
        public FileSystemPath GetCommonPicturesFolder() => Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures).ToFileSystemPath();
        public FileSystemPath GetCommonProgramFilesFolder() => Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles).ToFileSystemPath();
        public FileSystemPath GetCommonProgramFilesX86Folder() => Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86).ToFileSystemPath();
        public FileSystemPath GetCommonProgramsFolder() => Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms).ToFileSystemPath();
        public FileSystemPath GetCommonStartMenuFolder() => Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu).ToFileSystemPath();
        public FileSystemPath GetCommonStartupFolder() => Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup).ToFileSystemPath();
        public FileSystemPath GetCommonTemplatesFolder() => Environment.GetFolderPath(Environment.SpecialFolder.CommonTemplates).ToFileSystemPath();
        public FileSystemPath GetCommonVideosFolder() => Environment.GetFolderPath(Environment.SpecialFolder.CommonVideos).ToFileSystemPath();
        public FileSystemPath GetCookiesFolder() => Environment.GetFolderPath(Environment.SpecialFolder.Cookies).ToFileSystemPath();
        public FileSystemPath GetLogocalDesktopFolder() => Environment.GetFolderPath(Environment.SpecialFolder.Desktop).ToFileSystemPath();
        public FileSystemPath GetDesktopFolder() => Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory).ToFileSystemPath();
        public FileSystemPath GetFavoritesFolder() => Environment.GetFolderPath(Environment.SpecialFolder.Favorites).ToFileSystemPath();
        public FileSystemPath GetFontsFolder() => Environment.GetFolderPath(Environment.SpecialFolder.Fonts).ToFileSystemPath();
        public FileSystemPath GetHistoryFolder() => Environment.GetFolderPath(Environment.SpecialFolder.History).ToFileSystemPath();
        public FileSystemPath GetInternetCacheFolder() => Environment.GetFolderPath(Environment.SpecialFolder.InternetCache).ToFileSystemPath();
        public FileSystemPath GetLocalApplicationDataFolder() => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).ToFileSystemPath();
        public FileSystemPath GetLocalizedResourcesFolder() => Environment.GetFolderPath(Environment.SpecialFolder.LocalizedResources).ToFileSystemPath();
        public FileSystemPath GetMyComputerFolder() => Environment.GetFolderPath(Environment.SpecialFolder.MyComputer).ToFileSystemPath();
        public FileSystemPath GetMyDocumentsFolder() => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToFileSystemPath();
        public FileSystemPath GetMyMusicFolder() => Environment.GetFolderPath(Environment.SpecialFolder.MyMusic).ToFileSystemPath();
        public FileSystemPath GetMyPicturesFolder() => Environment.GetFolderPath(Environment.SpecialFolder.MyPictures).ToFileSystemPath();
        public FileSystemPath GetMyVideosFolder() => Environment.GetFolderPath(Environment.SpecialFolder.MyVideos).ToFileSystemPath();
        public FileSystemPath GetNetworkShortcutsFolder() => Environment.GetFolderPath(Environment.SpecialFolder.NetworkShortcuts).ToFileSystemPath();
        public FileSystemPath GetPersonalFolder() => Environment.GetFolderPath(Environment.SpecialFolder.Personal).ToFileSystemPath();
        public FileSystemPath GetPrinterShortcutsFolder() => Environment.GetFolderPath(Environment.SpecialFolder.PrinterShortcuts).ToFileSystemPath();
        public FileSystemPath GetProgramFilesFolder() => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).ToFileSystemPath();
        public FileSystemPath GetProgramFilesX86Folder() => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).ToFileSystemPath();
        public FileSystemPath GetProgramsFolder() => Environment.GetFolderPath(Environment.SpecialFolder.Programs).ToFileSystemPath();
        public FileSystemPath GetRecentFolder() => Environment.GetFolderPath(Environment.SpecialFolder.Recent).ToFileSystemPath();
        public FileSystemPath GetResourcesFolder() => Environment.GetFolderPath(Environment.SpecialFolder.Resources).ToFileSystemPath();
        public FileSystemPath GetSendToFolder() => Environment.GetFolderPath(Environment.SpecialFolder.SendTo).ToFileSystemPath();
        public FileSystemPath GetStartMenuFolder() => Environment.GetFolderPath(Environment.SpecialFolder.StartMenu).ToFileSystemPath();
        public FileSystemPath GetStartupFolder() => Environment.GetFolderPath(Environment.SpecialFolder.Startup).ToFileSystemPath();
        public FileSystemPath GetSystemFolder() => Environment.GetFolderPath(Environment.SpecialFolder.System).ToFileSystemPath();
        public FileSystemPath GetSystemX86Folder() => Environment.GetFolderPath(Environment.SpecialFolder.SystemX86).ToFileSystemPath();
        public FileSystemPath GetTemplatesFolder() => Environment.GetFolderPath(Environment.SpecialFolder.Templates).ToFileSystemPath();
        public FileSystemPath GetUserProfileFolder() => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).ToFileSystemPath();
        public FileSystemPath GetWindowsFolder() => Environment.GetFolderPath(Environment.SpecialFolder.Windows).ToFileSystemPath();

        public FileInfo AsFileInfo() { try { return new FileInfo(Internal); } catch { return null; } }
        public DirectoryInfo AsDirectoryInfo() { try { return new DirectoryInfo(Internal); } catch { return null; } }

        // ToDo: File/Directory 両対応、とりあえず面倒などで後でやる
        // public DateTime GetCreationTime() => File.GetCreationTime(FullPath);
        // public DateTime GetCreationTimeUtc() => File.GetCreationTimeUtc(FullPath);
        // public DateTime GetLastAccessTime() => File.GetLastAccessTime(FullPath);
        // public DateTime GetLastAccessTimeUtc() => File.GetLastAccessTimeUtc(FullPath);
        // public DateTime GetLastWriteTime() => File.GetLastWriteTime(FullPath);
        // public DateTime GetLastWriteTimeUtc() => File.GetLastWriteTimeUtc(FullPath);
        // public void SetAccessControl(FileSecurity fileSecurity) => File.SetAccessControl(FullPath, fileSecurity);
        // public void SetCreationTime(DateTime creationTime) => File.SetCreationTime(FullPath, creationTime);
        // public void SetCreationTimeUtc(DateTime creationTime) => File.SetCreationTimeUtc(FullPath, creationTime);
        // public void SetLastAccessTime(DateTime lastAccessTime) => File.SetLastAccessTime(FullPath, lastAccessTime);
        // public void SetLastAccessTimeUtc(DateTime lastAccessTime) => File.SetLastAccessTimeUtc(FullPath, lastAccessTime);
        // public void SetLastWriteTime(DateTime lastWriteTime) => File.SetLastWriteTime(FullPath, lastWriteTime);
        // public void SetLastWriteTimeUtc(DateTime lastWriteTime) => File.SetLastWriteTimeUtc(FullPath, lastWriteTime);
    }
}
