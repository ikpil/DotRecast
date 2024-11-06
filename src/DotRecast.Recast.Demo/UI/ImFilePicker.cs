using System;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.IO;
using System.Numerics;
using ImGuiNET;
using Serilog;

namespace DotRecast.Recast.Demo.UI;

// original code : https://gist.github.com/prime31/91d1582624eb2635395417393018016e
public class ImFilePicker
{
    private static readonly ILogger Logger = Log.ForContext<RecastDemo>();

    private static readonly Dictionary<string, ImFilePicker> _filePickers = new Dictionary<string, ImFilePicker>();

    public string RootFolder;
    public string CurrentFolder;
    public string SelectedFile;
    public List<string> AllowedExtensions;
    public bool OnlyAllowFolders;

    public static ImFilePicker GetFolderPicker(string pickerName, string startingPath)
        => GetFilePicker(pickerName, startingPath, null, true);

    public static ImFilePicker GetFilePicker(string pickerName, string startingPath, string searchFilter = null, bool onlyAllowFolders = false)
    {
        if (File.Exists(startingPath))
        {
            startingPath = new FileInfo(startingPath).DirectoryName;
        }
        else if (string.IsNullOrEmpty(startingPath) || !Directory.Exists(startingPath))
        {
            startingPath = Environment.CurrentDirectory;
            if (string.IsNullOrEmpty(startingPath))
                startingPath = AppContext.BaseDirectory;
        }

        if (!_filePickers.TryGetValue(pickerName, out ImFilePicker fp))
        {
            fp = new ImFilePicker();
            fp.RootFolder = startingPath;
            fp.CurrentFolder = startingPath;
            fp.OnlyAllowFolders = onlyAllowFolders;

            if (searchFilter != null)
            {
                if (fp.AllowedExtensions != null)
                    fp.AllowedExtensions.Clear();
                else
                    fp.AllowedExtensions = new List<string>();

                fp.AllowedExtensions.AddRange(searchFilter.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
            }

            _filePickers.Add(pickerName, fp);
        }

        return fp;
    }

    public static void RemoveFilePicker(string pickerName) => _filePickers.Remove(pickerName);

    public bool Draw()
    {
        ImGui.Text("Current Folder: " + CurrentFolder);
        bool result = false;

        if (ImGui.BeginChild(1, new Vector2(1024, 400)))
        {
            var di = new DirectoryInfo(CurrentFolder);
            if (di.Exists)
            {
                if (di.Parent != null)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, (uint)Color.Yellow.ToArgb());
                    if (ImGui.Selectable("../", false, ImGuiSelectableFlags.DontClosePopups))
                        CurrentFolder = di.Parent.FullName;

                    ImGui.PopStyleColor();
                }

                var fileSystemEntries = GetFileSystemEntries(di.FullName);
                foreach (var fse in fileSystemEntries)
                {
                    if (Directory.Exists(fse))
                    {
                        var name = Path.GetFileName(fse);
                        ImGui.PushStyleColor(ImGuiCol.Text, (uint)Color.Yellow.ToArgb());
                        if (ImGui.Selectable(name + "/", false, ImGuiSelectableFlags.DontClosePopups))
                            CurrentFolder = fse;
                        ImGui.PopStyleColor();
                    }
                    else
                    {
                        var name = Path.GetFileName(fse);
                        bool isSelected = SelectedFile == fse;
                        if (ImGui.Selectable(name, isSelected, ImGuiSelectableFlags.DontClosePopups))
                            SelectedFile = fse;

                        if (ImGui.IsMouseDoubleClicked(0))
                        {
                            result = true;
                            ImGui.CloseCurrentPopup();
                        }
                    }
                }
            }
        }

        ImGui.EndChild();


        if (ImGui.Button("Cancel"))
        {
            result = false;
            ImGui.CloseCurrentPopup();
        }

        if (OnlyAllowFolders)
        {
            ImGui.SameLine();
            if (ImGui.Button("Open"))
            {
                result = true;
                SelectedFile = CurrentFolder;
                ImGui.CloseCurrentPopup();
            }
        }
        else if (SelectedFile != null)
        {
            ImGui.SameLine();
            if (ImGui.Button("Open"))
            {
                result = true;
                ImGui.CloseCurrentPopup();
            }
        }

        return result;
    }

    bool TryGetFileInfo(string fileName, out FileInfo realFile)
    {
        try
        {
            realFile = new FileInfo(fileName);
            return true;
        }
        catch
        {
            realFile = null;
            return false;
        }
    }

    List<string> GetFileSystemEntries(string fullName)
    {
        var files = new List<string>();
        var dirs = new List<string>();

        ImmutableArray<string> fileEntries;
        try
        {
            fileEntries = Directory
                .GetFileSystemEntries(fullName, "")
                .ToImmutableArray();
        }
        catch (Exception e)
        {
            Logger.Error(e, "");
            return files;
        }

        foreach (var fse in fileEntries)
        {
            if (Directory.Exists(fse))
            {
                dirs.Add(fse);
            }
            else if (!OnlyAllowFolders)
            {
                if (AllowedExtensions != null)
                {
                    var ext = Path.GetExtension(fse);
                    if (AllowedExtensions.Contains(ext))
                        files.Add(fse);
                }
                else
                {
                    files.Add(fse);
                }
            }
        }

        var ret = new List<string>(dirs);
        ret.AddRange(files);

        return ret;
    }
}