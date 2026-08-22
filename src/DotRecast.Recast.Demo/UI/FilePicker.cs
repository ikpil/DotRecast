// SPDX-FileCopyrightText: 2026 Ikpil Choi(ikpil@naver.com)
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using ImGuiNET;

namespace DotRecast.Recast.Demo.UI;

// Reusable ImGui file picker for the sample application. The caller supplies the title and filters,
// so it stays independent of any particular sample or file format.
internal sealed class FilePicker
{
    // Bright ANSI blue, the conventional `ls` color for directories on a dark terminal.
    private static readonly Vector4 DirectoryColor = new Vector4(0.33f, 0.48f, 1.0f, 1.0f);

    internal readonly struct Filter
    {
        public readonly string Name;
        private readonly string[] m_extensions;

        public Filter(string name, params string[] extensions)
        {
            Name = name;
            m_extensions = extensions ?? Array.Empty<string>();
        }

        public bool Matches(string path)
        {
            if (m_extensions.Length == 0)
            {
                return true;
            }

            string extension = Path.GetExtension(path);
            foreach (string expected in m_extensions)
            {
                string normalized = expected.StartsWith('.') ? expected : "." + expected;
                if (string.Equals(extension, normalized, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public string DefaultExtension
        {
            get
            {
                if (m_extensions.Length == 0)
                {
                    return string.Empty;
                }
                string extension = m_extensions[0];
                return extension.StartsWith('.') ? extension : "." + extension;
            }
        }
    }

    private enum PickerMode
    {
        Open,
        Save,
    }

    private readonly List<Entry> m_entries = new List<Entry>();
    private readonly string m_popupName;
    private readonly string m_fileNamePopupName;
    private readonly string m_overwritePopupName;
    private readonly Filter[] m_filters;
    private readonly string[] m_filterNames;
    private string m_directory = Directory.GetCurrentDirectory();
    private string m_address = Directory.GetCurrentDirectory();
    private string m_fileName = string.Empty;
    private string m_message = string.Empty;
    private string m_pendingOverwritePath = string.Empty;
    private int m_filterIndex;
    private PickerMode m_mode;
    private bool m_focusAddress;
    private bool m_focusFileName;
    private bool m_openRequested;
    private bool m_overwriteRequested;

    private readonly struct Entry
    {
        public readonly string Name;
        public readonly string FullPath;
        public readonly bool IsDirectory;
        public readonly bool OpenOnSingleClick;
        public readonly long Length;
        public readonly DateTime Modified;

        public Entry(string name, string fullPath, bool isDirectory, long length, DateTime modified,
            bool openOnSingleClick = false)
        {
            Name = name;
            FullPath = fullPath;
            IsDirectory = isDirectory;
            OpenOnSingleClick = openOnSingleClick;
            Length = length;
            Modified = modified;
        }
    }

    public FilePicker(string title, string id, params Filter[] filters)
    {
        m_popupName = $"{title}##{id}";
        m_fileNamePopupName = $"##FileNameChoices{id}";
        m_overwritePopupName = $"Replace existing file?##Overwrite{id}";
        m_filters = filters is { Length: > 0 }
            ? filters
            : new[] { new Filter("All files (*.*)") };
        m_filterNames = new string[m_filters.Length];
        for (int i = 0; i < m_filters.Length; ++i)
        {
            m_filterNames[i] = m_filters[i].Name;
        }
    }

    public void ShowOpen(string path)
    {
        Show(path, PickerMode.Open);
    }

    public void ShowSave(string path)
    {
        Show(path, PickerMode.Save);
    }

    private void Show(string path, PickerMode mode)
    {
        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(string.IsNullOrWhiteSpace(path) ? "." : path);
        }
        catch
        {
            fullPath = Directory.GetCurrentDirectory();
        }

        string directory = Directory.Exists(fullPath) ? fullPath : Path.GetDirectoryName(fullPath);
        if (string.IsNullOrEmpty(directory) || Directory.Exists(directory) == false)
        {
            directory = Directory.GetCurrentDirectory();
        }

        SetDirectory(directory);
        m_fileName = Directory.Exists(fullPath) ? string.Empty : Path.GetFileName(fullPath);
        m_mode = mode;
        m_focusAddress = false;
        m_focusFileName = mode == PickerMode.Save;
        m_pendingOverwritePath = string.Empty;
        m_overwriteRequested = false;
        m_openRequested = true;
    }

    public bool Draw(out string selectedPath)
    {
        selectedPath = string.Empty;
        if (m_openRequested)
        {
            ImGui.OpenPopup(m_popupName);
            m_openRequested = false;
        }

        float fontSize = ImGui.GetFontSize();
        ImGui.SetNextWindowSize(new Vector2(52.0f * fontSize, 32.0f * fontSize), ImGuiCond.Appearing);
        ImGui.SetNextWindowSizeConstraints(new Vector2(40.0f * fontSize, 22.0f * fontSize),
            new Vector2(float.MaxValue, float.MaxValue));

        bool accepted = false;
        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoScrollbar |
                                       ImGuiWindowFlags.NoScrollWithMouse;
        // The sample theme makes resize grips nearly transparent. Keep this dialog's grip obvious so
        // users can discover that the file browser is resizable.
        ImGui.PushStyleColor(ImGuiCol.ResizeGrip, new Vector4(0.32f, 0.50f, 0.78f, 0.70f));
        ImGui.PushStyleColor(ImGuiCol.ResizeGripHovered, new Vector4(0.38f, 0.62f, 1.0f, 0.90f));
        ImGui.PushStyleColor(ImGuiCol.ResizeGripActive, new Vector4(0.25f, 0.55f, 1.0f, 1.0f));
        // No close-button pointer: this modal is dismissed only by Open or Cancel. This also keeps
        // navigation clicks from ever being interpreted as a request to close the picker.
        if (ImGui.BeginPopupModal(m_popupName, windowFlags))
        {
            ImGuiIOPtr io = ImGui.GetIO();
            if (io.KeyCtrl && ImGui.IsKeyPressed(ImGuiKey.L, false))
            {
                m_focusAddress = true;
            }
            DrawNavigation(fontSize);
            ImGui.Separator();

            // Reserve the footer explicitly. The modal itself never scrolls; only the entries table does.
            ImGuiStylePtr style = ImGui.GetStyle();
            float footerHeight = 2.0f * ImGui.GetFrameHeight() + ImGui.GetTextLineHeight() +
                                 5.0f * style.ItemSpacing.Y + 2.0f * style.FramePadding.Y;
            float browserHeight = Math.Max(10.0f * fontSize, ImGui.GetContentRegionAvail().Y - footerHeight);
            float placesWidth = 10.0f * fontSize;
            ImGuiWindowFlags paneFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
            if (ImGui.BeginChild("##places", new Vector2(placesWidth, browserHeight),
                    ImGuiChildFlags.Border, paneFlags))
            {
                DrawPlaces();
            }
            ImGui.EndChild();

            ImGui.SameLine();
            if (ImGui.BeginChild("##files", new Vector2(0.0f, browserHeight),
                    ImGuiChildFlags.Border, paneFlags))
            {
                accepted = DrawEntries(fontSize, out selectedPath);
            }
            ImGui.EndChild();

            ImGui.Separator();
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted("File name:");
            ImGui.SameLine();
            float fileTypeWidth = 17.0f * fontSize;
            float fileNameWidth = Math.Max(10.0f * fontSize,
                ImGui.GetContentRegionAvail().X - fileTypeWidth - style.ItemSpacing.X);
            bool enter = DrawFileNameCombo(fileNameWidth, fontSize);
            ImGui.SameLine();
            ImGui.PushItemWidth(fileTypeWidth);
            if (ImGui.Combo("##fileType", ref m_filterIndex, m_filterNames, m_filterNames.Length))
            {
                Refresh();
            }
            ImGui.PopItemWidth();

            if (m_message.Length > 0)
            {
                ImGui.TextColored(new Vector4(0.9f, 0.45f, 0.35f, 1.0f), m_message);
            }
            else
            {
                ImGui.Dummy(new Vector2(0.0f, ImGui.GetTextLineHeight()));
            }

            float buttonsWidth = 13.0f * fontSize;
            ImGui.SetCursorPosX(Math.Max(ImGui.GetCursorPosX(), ImGui.GetWindowWidth() - buttonsWidth));
            string acceptLabel = m_mode == PickerMode.Save ? "Save" : "Open";
            if ((ImGui.Button(acceptLabel, new Vector2(5.5f * fontSize, 0.0f)) || enter) &&
                TryAccept(out selectedPath))
            {
                accepted = true;
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(5.5f * fontSize, 0.0f)))
            {
                ImGui.CloseCurrentPopup();
            }

            if (m_overwriteRequested)
            {
                ImGui.OpenPopup(m_overwritePopupName);
                m_overwriteRequested = false;
            }
            ImGui.SetNextWindowSize(new Vector2(24.0f * fontSize, 0.0f), ImGuiCond.Appearing);
            if (ImGui.BeginPopupModal(m_overwritePopupName, ImGuiWindowFlags.AlwaysAutoResize |
                    ImGuiWindowFlags.NoSavedSettings))
            {
                ImGui.TextWrapped($"'{Path.GetFileName(m_pendingOverwritePath)}' already exists. Replace it?");
                ImGui.Separator();
                if (ImGui.Button("Replace", new Vector2(7.0f * fontSize, 0.0f)))
                {
                    selectedPath = m_pendingOverwritePath;
                    accepted = true;
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
                if (ImGui.Button("Cancel", new Vector2(7.0f * fontSize, 0.0f)))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }

            if (accepted)
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
        ImGui.PopStyleColor(3);

        return accepted;
    }

    // Windows-style editable combo: typing remains available, while the arrow lists the files in
    // the current filtered view. ImGui's regular Combo is selection-only, so compose the two parts.
    private bool DrawFileNameCombo(float width, float fontSize)
    {
        float buttonWidth = ImGui.GetFrameHeight();
        float inputWidth = Math.Max(4.0f * fontSize, width - buttonWidth);
        Vector2 popupPosition = ImGui.GetCursorScreenPos();

        if (m_focusFileName)
        {
            ImGui.SetKeyboardFocusHere();
            m_focusFileName = false;
        }
        ImGui.PushItemWidth(inputWidth);
        string hint = m_mode == PickerMode.Save ? "Enter a new or existing file name" : "Enter or select a file name";
        bool enter = ImGui.InputTextWithHint("##fileName", hint, ref m_fileName, 512,
            ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll);
        ImGui.PopItemWidth();
        ImGui.SameLine(0.0f, 0.0f);
        if (ImGui.ArrowButton("##fileNameArrow", ImGuiDir.Down))
        {
            ImGui.OpenPopup(m_fileNamePopupName);
        }

        ImGui.SetNextWindowPos(new Vector2(popupPosition.X, popupPosition.Y + ImGui.GetFrameHeight()));
        ImGui.SetNextWindowSizeConstraints(new Vector2(width, 0.0f),
            new Vector2(width, 12.0f * fontSize));
        if (ImGui.BeginPopup(m_fileNamePopupName, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings))
        {
            bool hasFiles = false;
            foreach (Entry entry in m_entries)
            {
                if (entry.IsDirectory)
                {
                    continue;
                }

                hasFiles = true;
                bool selected = string.Equals(m_fileName, entry.Name, StringComparison.OrdinalIgnoreCase);
                if (ImGui.Selectable(entry.Name + "##FileNameChoice" + entry.FullPath, selected))
                {
                    m_fileName = entry.Name;
                    m_message = string.Empty;
                    ImGui.CloseCurrentPopup();
                }
            }
            if (hasFiles == false)
            {
                ImGui.TextDisabled("No matching files");
            }
            ImGui.EndPopup();
        }

        return enter;
    }

    private void DrawNavigation(float fontSize)
    {
        if (ImGui.Button("Refresh"))
        {
            Refresh();
        }
        ImGui.SameLine();
        ImGui.TextDisabled("Ctrl+L to edit the path");

        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted("Path:");
        ImGui.SameLine();
        float goWidth = ImGui.CalcTextSize("Go").X + 2.0f * ImGui.GetStyle().FramePadding.X;
        float addressWidth = Math.Max(8.0f * fontSize,
            ImGui.GetContentRegionAvail().X - goWidth - ImGui.GetStyle().ItemSpacing.X);
        if (m_focusAddress)
        {
            ImGui.SetKeyboardFocusHere();
            m_focusAddress = false;
        }
        ImGui.PushItemWidth(addressWidth);
        // This is deliberately a real InputText, not a read-only breadcrumb: paths can be copied,
        // pasted, edited, and committed with Enter or the Go button.
        bool go = ImGui.InputText("##address", ref m_address, 2048,
            ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll);
        ImGui.PopItemWidth();
        ImGui.SameLine();
        if (ImGui.Button("Go"))
        {
            go = true;
        }
        if (go)
        {
            string path = m_address.Trim();
            // Windows' "Copy as path" includes quotes. Accept that text without making the user
            // remove them before pressing Go.
            if (path.Length >= 2 && path[0] == '"' && path[^1] == '"')
            {
                path = path.Substring(1, path.Length - 2);
            }
            SetDirectory(path);
        }
    }

    private void DrawPlaces()
    {
        DrawPlace("Working folder", Directory.GetCurrentDirectory());
        DrawPlace("Desktop", Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
        DrawPlace("Documents", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

        ImGui.Separator();
        string[] roots;
        try
        {
            roots = Directory.GetLogicalDrives();
        }
        catch
        {
            roots = Array.Empty<string>();
        }

        foreach (string root in roots)
        {
            DrawPlace(root, root);
        }
    }

    private void DrawPlace(string label, string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        bool selected = string.Equals(m_directory, path, StringComparison.OrdinalIgnoreCase);
        if (ImGui.Selectable(label, selected))
        {
            SetDirectory(path);
        }
    }

    private bool DrawEntries(float fontSize, out string selectedPath)
    {
        selectedPath = string.Empty;
        bool accepted = false;
        if (ImGui.BeginTable("##entries", 3,
                ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.SizingFixedFit,
                ImGui.GetContentRegionAvail()))
        {
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Date modified", ImGuiTableColumnFlags.WidthFixed, 11.0f * fontSize);
            ImGui.TableSetupColumn("Size", ImGuiTableColumnFlags.WidthFixed, 6.0f * fontSize);
            ImGui.TableHeadersRow();

            foreach (Entry entry in m_entries)
            {
                string directoryToOpen = null;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                bool selected = entry.IsDirectory == false &&
                                string.Equals(m_fileName, entry.Name, StringComparison.OrdinalIgnoreCase);
                if (entry.IsDirectory)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, DirectoryColor);
                }
                if (ImGui.Selectable(entry.Name + "##" + entry.FullPath, selected,
                        ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowDoubleClick))
                {
                    if (entry.IsDirectory)
                    {
                        if (entry.OpenOnSingleClick || ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        {
                            directoryToOpen = entry.FullPath;
                        }
                    }
                    else
                    {
                        m_fileName = entry.Name;
                        m_message = string.Empty;
                        if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        {
                            accepted = TryAccept(out selectedPath);
                        }
                    }
                }

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(entry.Modified.ToString("yyyy-MM-dd HH:mm"));
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(entry.IsDirectory ? string.Empty : FormatSize(entry.Length));
                if (entry.IsDirectory)
                {
                    ImGui.PopStyleColor();
                }
                if (directoryToOpen != null)
                {
                    SetDirectory(directoryToOpen);
                    break;
                }
            }
            ImGui.EndTable();
        }
        return accepted;
    }

    private bool TryAccept(out string path)
    {
        path = string.Empty;
        string candidate;
        try
        {
            candidate = Path.IsPathRooted(m_fileName)
                ? Path.GetFullPath(m_fileName)
                : Path.GetFullPath(Path.Combine(m_directory, m_fileName));

            if (m_mode == PickerMode.Save && Path.HasExtension(candidate) == false)
            {
                candidate += m_filters[m_filterIndex].DefaultExtension;
            }
        }
        catch (Exception exception)
        {
            m_message = exception.Message;
            return false;
        }

        if (Directory.Exists(candidate))
        {
            SetDirectory(candidate);
            m_fileName = string.Empty;
            return false;
        }
        if (m_filters[m_filterIndex].Matches(candidate) == false)
        {
            m_message = $"Select a file matching {m_filters[m_filterIndex].Name}.";
            return false;
        }

        if (m_mode == PickerMode.Open && File.Exists(candidate) == false)
        {
            m_message = "The selected file does not exist.";
            return false;
        }
        if (m_mode == PickerMode.Save && File.Exists(candidate))
        {
            m_pendingOverwritePath = candidate;
            m_overwriteRequested = true;
            return false;
        }

        path = candidate;
        m_message = string.Empty;
        return true;
    }

    private void SetDirectory(string path)
    {
        try
        {
            string fullPath = Path.GetFullPath(path);
            if (Directory.Exists(fullPath) == false)
            {
                m_message = "Folder not found.";
                return;
            }
            m_directory = fullPath;
            m_address = fullPath;
            m_fileName = string.Empty;
            Refresh();
        }
        catch (Exception exception)
        {
            m_message = exception.Message;
        }
    }

    private void Refresh()
    {
        m_entries.Clear();
        try
        {
            DirectoryInfo current = new DirectoryInfo(m_directory);
            m_entries.Add(new Entry(".", current.FullName, true, 0, current.LastWriteTime, true));

            DirectoryInfo parent = Directory.GetParent(m_directory);
            if (parent != null)
            {
                m_entries.Add(new Entry("..", parent.FullName, true, 0, parent.LastWriteTime, true));
            }

            foreach (string directory in Directory.EnumerateDirectories(m_directory))
            {
                DirectoryInfo info = new DirectoryInfo(directory);
                m_entries.Add(new Entry(info.Name, info.FullName, true, 0, info.LastWriteTime, true));
            }
            foreach (string file in Directory.EnumerateFiles(m_directory))
            {
                if (m_filters[m_filterIndex].Matches(file) == false)
                {
                    continue;
                }
                FileInfo info = new FileInfo(file);
                m_entries.Add(new Entry(info.Name, info.FullName, false, info.Length, info.LastWriteTime));
            }

            m_entries.Sort((left, right) =>
            {
                if (left.IsDirectory != right.IsDirectory)
                {
                    return left.IsDirectory ? -1 : 1;
                }
                return StringComparer.OrdinalIgnoreCase.Compare(left.Name, right.Name);
            });
            m_message = string.Empty;
        }
        catch (Exception exception)
        {
            m_message = exception.Message;
        }
    }

    private static string FormatSize(long size)
    {
        if (size < 1024)
        {
            return $"{size} B";
        }
        if (size < 1024 * 1024)
        {
            return $"{size / 1024.0:F1} KB";
        }
        return $"{size / (1024.0 * 1024.0):F1} MB";
    }
}

