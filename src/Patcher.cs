using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
namespace HearthstoneAccessPatcher;
static class Patcher
{

    public static bool IsHsDirectory(string? directory)
    {
        if (string.IsNullOrWhiteSpace(directory)) return false;
        directory = Path.GetFullPath(directory);
        if (Directory.Exists(directory) && Path.GetFileName(directory) == "Hearthstone" && File.Exists(Path.Combine(directory, Constants.HearthstoneAssemblyPath)))
        {
            return true;
        }
        return false;
    }

    public static string? LocateHearthstone()
    {
        string? path = Environment.GetEnvironmentVariable("HEARTHSTONE_HOME");
        if (path != null && IsHsDirectory(path))
        {
            return Path.GetFullPath(path);
        }
        string programFiles;
        if (Environment.Is64BitOperatingSystem)
        {
            programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        }
        else
        {
            programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        }

        path = Path.Combine(programFiles, "Hearthstone");
        if (IsHsDirectory(path))
        {
            return path;
        }
        return null;
    }

    /// <summary>Reads the game version declared by a patch archive manifest.</summary>
    public static string GetHearthstoneVersion(Stream patchArchive)
    {
        if (!patchArchive.CanSeek) throw new InvalidDataException("Patch archive cannot be read.");

        patchArchive.Position = 0;
        try
        {
            using ZipArchive archive = new(patchArchive, ZipArchiveMode.Read, leaveOpen: true);
            ZipArchiveEntry? manifest = archive.GetEntry("patch/Accessibility/hsa_manifest.json")
                ?? archive.GetEntry("Accessibility/hsa_manifest.json");
            if (manifest == null) throw new InvalidDataException("Patch archive does not contain hsa_manifest.json.");

            using Stream manifestStream = manifest.Open();
            using JsonDocument document = JsonDocument.Parse(manifestStream);
            if (!document.RootElement.TryGetProperty("hearthstone_version", out JsonElement version))
                throw new InvalidDataException("Patch manifest does not declare a Hearthstone version.");

            return version.GetString() ?? throw new InvalidDataException("Patch manifest has an empty Hearthstone version.");
        }
        finally
        {
            patchArchive.Position = 0;
        }
    }

    /// <summary>
    /// Applies an archive to the Hearthstone directory. Official archives use a
    /// <c>patch/</c> root; custom overlays may contain game files at their root.
    /// </summary>
    public static void UnpackAndPatch(Stream downloaded, string directory, bool placeChangelogOnDesktop)
    {
        string targetDirectory = Path.GetFullPath(directory);
        string targetPrefix = targetDirectory.EndsWith(Path.DirectorySeparatorChar)
            ? targetDirectory
            : targetDirectory + Path.DirectorySeparatorChar;

        using ZipArchive archive = new(downloaded, ZipArchiveMode.Read, leaveOpen: true);
        bool hasPatchRoot = archive.Entries.Any(entry =>
            entry.FullName.StartsWith(Constants.PatchDirectory, StringComparison.OrdinalIgnoreCase));

        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            string entryName = entry.FullName;
            if (entryName.Equals("changelog.md", StringComparison.OrdinalIgnoreCase))
            {
                if (placeChangelogOnDesktop)
                {
                    string changelogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "HearthstoneAccess Changelog.md");
                    using Stream changelogStream = entry.Open();
                    using FileStream changelogFile = new(changelogPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    changelogStream.CopyTo(changelogFile);
                }
                continue;
            }

            if (string.IsNullOrWhiteSpace(entryName) || entryName.EndsWith('/')) continue;
            if (hasPatchRoot)
            {
                if (!entryName.StartsWith(Constants.PatchDirectory, StringComparison.OrdinalIgnoreCase)) continue;
                entryName = entryName.Substring(Constants.PatchDirectory.Length);
            }

            string destination = Path.GetFullPath(Path.Combine(targetDirectory, Path.Join(entryName.Split('/'))));
            if (!destination.StartsWith(targetPrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException($"Archive entry attempts to write outside target directory: {entry.FullName}");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            using Stream entryStream = entry.Open();
            using FileStream fileStream = new(destination, FileMode.Create, FileAccess.Write, FileShare.None);
            entryStream.CopyTo(fileStream);
        }
    }

}
