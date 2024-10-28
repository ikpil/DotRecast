namespace DotRecast.Tool.PublishToUniRecast;

public class CsProj
{
    public readonly string RootPath;
    public readonly string Name;
    public readonly string TargetPath;

    public CsProj(string rootPath, string name, string targetPath)
    {
        RootPath = rootPath;
        Name = name;
        TargetPath = targetPath;
    }
}