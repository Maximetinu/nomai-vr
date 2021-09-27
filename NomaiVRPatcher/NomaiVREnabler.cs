﻿using AssetsTools.NET;
using AssetsTools.NET.Extra;
using BepInEx;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NomaiVRPatcher
{
    /// <summary>
    /// Patches the game to use both old and new input system
    /// Moves VR Plugin files to the appropriate folders
    /// </summary>
    public static class NomaiVREnabler
    {
        // List of assemblies to patch
        public static IEnumerable<string> TargetDLLs { get; } = new string[0];

        // Patches the assemblies
        public static void Patch(AssemblyDefinition assembly) { }

        // Called by BepInEx
        public static void Initialize()
        {
            var gameManagersPath = Path.Combine(Paths.ManagedPath, "..", "globalgamemanagers");
            var backupPath = BackupFile(gameManagersPath);

            //TODO: Copy relevant files?

            PatchGlobalGameManagers(gameManagersPath, backupPath);
        }

        private static void PatchGlobalGameManagers(string gameManagersPath, string gameManagersBackup)
        {
            AssetsManager assetsManager = new AssetsManager();
            assetsManager.LoadClassPackage(Path.Combine(Paths.PatcherPluginPath, "NomaiVR", "classdata.tpk"));
            AssetsFileInstance assetsFileInstance = assetsManager.LoadAssetsFile(gameManagersBackup, false);
            AssetsFile assetsFile = assetsFileInstance.file;
            AssetsFileTable assetsFileTable = assetsFileInstance.table;
            assetsManager.LoadClassDatabaseFromPackage(assetsFile.typeTree.unityVersion);

            List<AssetsReplacer> replacers = new List<AssetsReplacer>();

            AssetFileInfoEx playerSettings = assetsFileTable.GetAssetInfo(1);
            AssetTypeValueField playerSettingsBase = assetsManager.GetTypeInstance(assetsFile, playerSettings).GetBaseField();
            AssetTypeValueField disableOldInputManagerSupport = playerSettingsBase.Get("disableOldInputManagerSupport");
            disableOldInputManagerSupport.value = new AssetTypeValue(EnumValueTypes.ValueType_Bool, false);
            replacers.Add(new AssetsReplacerFromMemory(0, playerSettings.index, (int)playerSettings.curFileType, 0xffff, playerSettingsBase.WriteToByteArray()));

            using (AssetsFileWriter writer = new AssetsFileWriter(File.OpenWrite(gameManagersPath)))
            {
                assetsFile.Write(writer, 0, replacers, 0);
            }
        }

        private static string BackupFile(string fileName)
        {
            var backupName = fileName + ".bak";
            if (!File.Exists(backupName))
                File.Copy(fileName, backupName);
            return backupName;
        }
    }
}
