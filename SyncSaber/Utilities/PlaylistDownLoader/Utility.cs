using BeatSaberMarkupLanguage.FloatingScreen;
using IPA.Loader;
//using PlaylistDownLoader.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

namespace SyncSaber.Utilities.PlaylistDownLoader
{
    public class Utility
    {
        private static readonly object s_lockObject = new object();
        public static bool IsModInstalled(string modName)
        {
            return PluginManager.GetPlugin(modName) != null;
        }
        public static bool IsPlaylistDownLoaderInstalled()
        {
            return IsModInstalled("PlaylistDownLoader");
        }

        public static void WriteStringListSafe(string path, List<string> data, bool sort = true)
        {
            lock (s_lockObject) {
                if (File.Exists(path)) {
                    File.Copy(path, path + ".bak", true);
                }

                if (sort) {
                    data.Sort();
                }

                File.WriteAllLines(path, data);
                File.Delete(path + ".bak");
            }
        }

        public static void AddEventHandler(DiContainer container,Action<string> action)
        {
            var instance = GetPlaylistDownloader(container);
            if (instance == null) {
                return;
            }
            var eventType = instance.GetType().GetEvent("ChangeNotificationText", (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
            if (eventType == null) {
                return;
            }
            eventType.AddEventHandler(instance, action);
        }

        public static void RemoveEventHandler(DiContainer container, Action<string> action)
        {
            var instance = GetPlaylistDownloader(container);
            if (instance == null) {
                return;
            }
            var eventType = instance.GetType().GetEvent("ChangeNotificationText", (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
            if (eventType == null) {
                return;
            }
            eventType.RemoveEventHandler(instance, action);
        }

        private static object GetPlaylistDownloader(DiContainer container)
        {
            if (!Utility.IsPlaylistDownLoaderInstalled()) {
                return null;
            }
            var interfaceType = Type.GetType("PlaylistDownLoader.Interfaces.IPlaylistDownloader, PlaylistDownLoader");
            if (interfaceType == null) {
                return null;
            }
            return container.TryResolve(interfaceType);
        }

        public static Task CheckPlaylistsSong(DiContainer container)
        {
            var instance = GetPlaylistDownloader(container);
            if (instance == null) {
                return Task.CompletedTask;
            }
            var methodType = instance.GetType().GetMethod("ChangeNotificationText", (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
            if (methodType == null) {
                return Task.CompletedTask;
            }
            return (Task)methodType.Invoke(instance, new object[0]);
        }
    }
}
