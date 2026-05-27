using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncSaber.Utilities
{
    internal class IO_Util
    {
        public static void MoveFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (var dir in source.GetDirectories()) {
                MoveFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            }

            foreach (var file in source.GetFiles()) {
                var newFilePath = Path.Combine(target.FullName, file.Name);
                if (File.Exists(newFilePath)) {
                    try {
                        File.Delete(newFilePath);
                    }
                    catch (Exception e) {
                        Logger.Error(e);
                        var filesToDelete = Path.Combine(Environment.CurrentDirectory, "FilesToDelete");
                        if (!Directory.Exists(filesToDelete)) {
                            Directory.CreateDirectory(filesToDelete);
                        }

                        File.Move(newFilePath, Path.Combine(filesToDelete, file.Name));
                    }
                }
                file.MoveTo(newFilePath);
            }
        }

        public static void ExtractZip(Stream stream, string extractPath)
        {
            if (stream == null || string.IsNullOrWhiteSpace(extractPath)) {
                return;
            }
            if (File.Exists(extractPath)) {
                return;
            }
            try {
                using (var archaive = new ZipArchive(stream, ZipArchiveMode.Read)) {
                    archaive.ExtractToDirectory(extractPath);
                }
            }
            catch (Exception e) {
                Logger.Error(e);
                return;
            }
        }
    }
}
