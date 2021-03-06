//  Copyright (C) 2020 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactoryGUI.Controls;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Helpers
{
    internal static class FileHelper
    {
        public static async Task<string> ReadAllTextAsync(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                                              4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
            var buffer = new byte[(int)stream.Length];
            await stream.ReadAsync(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }

        public static async Task WriteAllTextAsync(string path, string contents)
        {
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None,
                                              4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
            var buffer = Encoding.UTF8.GetBytes(contents);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        public static bool IsOnSameVolume(string path1, string path2)
        {
            string? root1 = Path.GetPathRoot(path1);
            string? root2 = Path.GetPathRoot(path2);

            if (string.IsNullOrEmpty(root1) || string.IsNullOrEmpty(root2)) return false;

            var info1 = new DriveInfo(root1);
            var info2 = new DriveInfo(root2);

            return string.Equals(info1.Name, info2.Name, StringComparison.OrdinalIgnoreCase);
        }

        public static async ValueTask<DirectoryInfo> MoveDirectoryWithStatusAsync(DirectoryInfo directory, string destination)
        {
            var mainWindow = App.Current?.MainWindow;
            if (!(mainWindow is null))
            {
                // UI is loaded, display status window
                if (IsOnSameVolume(directory.FullName, destination))
                {
                    // On same volume, showing a dialog is not necessary
                    directory.MoveTo(destination);
                    return directory;
                }
                else
                {
                    var op = directory.MoveToByCopyAsync(destination);

                    // Get localized strings
                    string title = (string)App.Current!.Locales.GetResource("MovingLocation_Title");
                    string description = (string)App.Current!.Locales.GetResource("MovingFiles_Message");

                    // We don't need to evaluate this since we don't allow cancellation
                    await ProgressDialog.Show(title, description, op, mainWindow);
                    return op.Result;
                }
            }
            else
            {
                // No UI, proceed silently
                return await directory.MoveToAsync(destination);
            }
        }

        // Helper function to prompt the user about deletion if the directory is not empty
        // If the directory does not exist or is empty we silently return true
        public static async ValueTask<bool> AssureDirectorySafeForMoveAsync(DirectoryInfo directory)
        {
            if (!directory.Exists) return true;
            if (directory.IsEmpty()) return true;

            var result = await Messages.ConfirmLocationMove.Show();
            if (result == DialogResult.Yes)
            {
                directory.Delete(true);
                return true;
            }
            else
            {
                return false;
            }
        }

        private static string SanitizePath(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows is case insensitive and likes to mix capitalization
                path = path.Trim().ToLower();
            }
            else
            {
                // On other OS we can be sure capitalization will be consistent
                path = path.Trim();
            }

            path = path.Replace('\\', '/');
            if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
            return path;
        }

        public static bool PathsEqual(string first, string second)
        {
            first = SanitizePath(first);
            second = SanitizePath(second);
            return first == second;
        }

        public static bool DirectoriesEqual(DirectoryInfo first, DirectoryInfo second)
            => PathsEqual(first.FullName, second.FullName);

        public static bool FilesEqual(FileInfo first, FileInfo second)
            => PathsEqual(first.FullName, second.FullName);

        public static bool PathExists(string path)
            => File.Exists(path) || Directory.Exists(path);
    }
}
