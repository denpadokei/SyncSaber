﻿using BeatSaberMarkupLanguage.FloatingScreen;
using IPA.Loader;
using PlaylistDownLoader.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using TMPro;
using UnityEngine;
using Zenject;

namespace SyncSaber.Utilities
{
    public class Utility
    {
        private static readonly object _lockObject = new object();

        public static TextMeshProUGUI CreateNotificationText(string text, FloatingScreen screen)
        {
            //var gameObject = new GameObject();
            //GameObject.DontDestroyOnLoad(gameObject);
            //gameObject.transform.position = new Vector3(0, 0f, 2.5f);
            //gameObject.transform.eulerAngles = new Vector3(0, 0, 0);
            //gameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            //var canvas = gameObject.AddComponent<Canvas>();
            //canvas.renderMode = RenderMode.WorldSpace;
            //var rectTransform = canvas.transform as RectTransform;
            //rectTransform.sizeDelta = new Vector2(200, 50);

            var notificationText = BeatSaberUI.CreateText(screen.transform as RectTransform, text, new Vector2(0f, 0f), new Vector2(0f, 0f));

            notificationText.text = text;
            notificationText.fontSize = 10f;
            notificationText.alignment = TextAlignmentOptions.Center;
            return notificationText;
        }

        //public static async Task DownloadFile(string url, string path)
        //{
        //    var res = await WebClient.DownloadSong(url, new System.Threading.CancellationToken());
        //    try {
        //        if (!Directory.Exists(Path.GetDirectoryName(path)))
        //            Directory.CreateDirectory(Path.GetDirectoryName(path));
        //        File.WriteAllBytes(path, res);
        //    }
        //    catch (Exception e) {
        //        Logger.Info($"Exception when writing file! {e}");
        //    }
        //}

        //public static void EmptyDirectory(string directory, bool delete = true)
        //{
        //    if (Directory.Exists(directory))
        //    {
        //        var directoryInfo = new DirectoryInfo(directory);
        //        foreach (System.IO.FileInfo file in directoryInfo.GetFiles()) file.Delete();
        //        foreach (System.IO.DirectoryInfo subDirectory in directoryInfo.GetDirectories()) subDirectory.Delete(true);

        //        if (delete) Directory.Delete(directory, true);
        //    }
        //}

        public static void MoveFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (var dir in source.GetDirectories())
                MoveFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (var file in source.GetFiles()) {
                var newFilePath = Path.Combine(target.FullName, file.Name);
                if (File.Exists(newFilePath)) {
                    try {
                        File.Delete(newFilePath);
                    }
                    catch (Exception e) {
                        Logger.Error(e);
                        //Logger.Info($"Failed to delete file {Path.GetFileName(newFilePath)}! File is in use!");
                        var filesToDelete = Path.Combine(Environment.CurrentDirectory, "FilesToDelete");
                        if (!Directory.Exists(filesToDelete))
                            Directory.CreateDirectory(filesToDelete);
                        File.Move(newFilePath, Path.Combine(filesToDelete, file.Name));
                        //Logger.Info("Moved file into FilesToDelete directory!");
                    }
                }
                file.MoveTo(newFilePath);
            }
        }

        //public static async Task ExtractZip(string zipPath, string extractPath)
        //{
        //    if (File.Exists(zipPath))
        //    {
        //        bool extracted = false;
        //        try
        //        {
        //            if (Directory.Exists(".syncsabertemp"))
        //                Directory.CreateDirectory(".syncsabertemp");

        //            ZipFile.ExtractToDirectory(zipPath, ".syncsabertemp");
        //            extracted = true;
        //        }
        //        catch (Exception)
        //        {
        //            Logger.Info($"An error occured while trying to extract \"{zipPath}\"!");
        //            return;
        //        }
        //        await Task.Delay(500);
        //        File.Delete(zipPath);

        //        try
        //        {
        //            if (extracted)
        //            {
        //                if (!Directory.Exists(extractPath)) {
        //                    Logger.Debug($"temp directory : {extractPath}");
        //                    Directory.CreateDirectory(extractPath);
        //                }
        //                MoveFilesRecursively(new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, ".syncsabertemp")), new DirectoryInfo(extractPath));
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Logger.Info($"An exception occured while trying to move files into their final directory! {e}");
        //        }
        //        EmptyDirectory(".syncsabertemp");
        //    }
        //}

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
                Logger.Info($"{e}");
                return;
            }
        }

        public static bool IsModInstalled(string modName) => PluginManager.GetPlugin(modName) != null;

        public static void WriteStringListSafe(string path, List<string> data, bool sort = true)
        {
            lock (_lockObject) {
                if (File.Exists(path))
                    File.Copy(path, path + ".bak", true);

                if (sort)
                    data.Sort();

                File.WriteAllLines(path, data);
                File.Delete(path + ".bak");
            }
        }

        public static IPlaylistDownloader GetPlaylistDownloader(DiContainer container)
        {
            if (!Plugin.Instance.IsPlaylistDownlaoderInstalled) {
                return null;
            }
            return container.Resolve<IPlaylistDownloader>();
        }
    }
}
