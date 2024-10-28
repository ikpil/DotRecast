using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace DotRecast.Tool.PublishToUniRecast;

public static class Program
{
    public static void Main(string[] args)
    {
        var source = SearchDirectory("DotRecast");
        var destination = SearchDirectory("UniRecast");

        if (!Directory.Exists(source))
        {
            throw new Exception("not found source directory");
        }

        if (!Directory.Exists(destination))
        {
            throw new Exception("not found destination directory");
        }

        var ignorePaths = ImmutableArray.Create("bin", "obj");
        var projs = ImmutableArray.Create(
            // src
            new CsProj("src", "DotRecast.Core", "Runtime"),
            new CsProj("src", "DotRecast.Recast", "Runtime"),
            new CsProj("src", "DotRecast.Detour", "Runtime"),
            new CsProj("src", "DotRecast.Detour.Crowd", "Runtime"),
            new CsProj("src", "DotRecast.Detour.Dynamic", "Runtime"),
            new CsProj("src", "DotRecast.Detour.Extras", "Runtime"),
            new CsProj("src", "DotRecast.Detour.TileCache", "Runtime"),
            new CsProj("src", "DotRecast.Recast.Toolset", "Runtime")
        );


        foreach (var proj in projs)
        {
            var sourcePath = Path.Combine(source, proj.RootPath, $"{proj.Name}");
            var destPath = Path.Combine(destination, $"{proj.TargetPath}", $"{proj.Name}");
            
            SyncFiles(sourcePath, destPath, ignorePaths, "*.cs");
        }

        // // 몇몇 필요한 리소스 복사 하기
        // string destResourcePath = destDotRecast + "/resources";
        // if (!Directory.Exists(destResourcePath))
        // {
        //     Directory.CreateDirectory(destResourcePath);
        // }

        // string sourceResourcePath = Path.Combine(dotRecastPath, "resources/nav_test.obj");
        // File.Copy(sourceResourcePath, destResourcePath + "/nav_test.obj", true);
    }

    public static string SearchPath(string searchPath, int depth, out bool isDir)
    {
        isDir = false;

        for (int i = 0; i < depth; ++i)
        {
            var relativePath = string.Join("", Enumerable.Range(0, i).Select(x => "../"));
            var searchingPath = Path.Combine(relativePath, searchPath);
            var fullSearchingPath = Path.GetFullPath(searchingPath);

            if (File.Exists(fullSearchingPath))
            {
                return fullSearchingPath;
            }

            if (Directory.Exists(fullSearchingPath))
            {
                isDir = true;
                return fullSearchingPath;
            }
        }

        return string.Empty;
    }

    // only directory
    public static string SearchDirectory(string dirname, int depth = 10)
    {
        var searchingPath = SearchPath(dirname, depth, out var isDir);
        if (isDir)
        {
            return searchingPath;
        }

        var path = Path.GetDirectoryName(searchingPath) ?? string.Empty;
        return path;
    }

    public static string SearchFile(string filename, int depth = 10)
    {
        var searchingPath = SearchPath(filename, depth, out var isDir);
        if (!isDir)
        {
            return searchingPath;
        }

        return string.Empty;
    }

    private static void SyncFiles(string srcRootPath, string dstRootPath, IList<string> ignoreFolders, string searchPattern = "*")
    {
        // 끝에서부터 이그노어 폴더일 경우 패스
        var destLastFolderName = Path.GetFileName(dstRootPath);
        if (ignoreFolders.Any(x => x == destLastFolderName))
            return;

        if (!Directory.Exists(dstRootPath))
            Directory.CreateDirectory(dstRootPath);

        // 소스파일 추출
        var sourceFiles = Directory.GetFiles(srcRootPath, searchPattern).ToList();
        var sourceFolders = Directory.GetDirectories(srcRootPath)
            .Select(x => new DirectoryInfo(x))
            .ToList();

        // 대상 파일 추출
        var destinationFiles = Directory.GetFiles(dstRootPath, searchPattern).ToList();
        var destinationFolders = Directory.GetDirectories(dstRootPath)
            .Select(x => new DirectoryInfo(x))
            .ToList();

        // 대상에 파일이 있는데, 소스에 없을 경우, 대상 파일을 삭제 한다.
        foreach (var destinationFile in destinationFiles)
        {
            var destName = Path.GetFileName(destinationFile);
            var found = sourceFiles.Any(x => Path.GetFileName(x) == destName);
            if (found)
                continue;

            File.Delete(destinationFile);
            Console.WriteLine($"delete file - {destinationFile}");
        }

        // 대상에 폴더가 있는데, 소스에 없을 경우, 대상 폴더를 삭제 한다.
        foreach (var destinationFolder in destinationFolders)
        {
            var found = sourceFolders.Any(sourceFolder => sourceFolder.Name == destinationFolder.Name);
            if (found)
                continue;

            Directory.Delete(destinationFolder.FullName, true);
            Console.WriteLine($"delete folder - {destinationFolder.FullName}");
        }

        // 소스 파일을 복사 한다.
        foreach (var sourceFile in sourceFiles)
        {
            var name = Path.GetFileName(sourceFile);
            var dest = Path.Combine(dstRootPath, name);
            File.Copy(sourceFile, dest, true);
            Console.WriteLine($"copy - {sourceFile} => {dest}");
        }

        // 대상 폴더를 복사 한다
        foreach (var sourceFolder in sourceFolders)
        {
            var dest = Path.Combine(dstRootPath, sourceFolder.Name);
            SyncFiles(sourceFolder.FullName, dest, ignoreFolders, searchPattern);
        }
    }
}