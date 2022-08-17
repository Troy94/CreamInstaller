﻿using CreamInstaller.Components;
using CreamInstaller.Utility;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreamInstaller.Resources;

internal static class SmokeAPI
{
    internal static void GetSmokeApiComponents(
            this string directory,
            out string sdk32, out string sdk32_o,
            out string sdk64, out string sdk64_o,
            out string config,
            out string cache)
    {
        sdk32 = directory + @"\steam_api.dll";
        sdk32_o = directory + @"\steam_api_o.dll";
        sdk64 = directory + @"\steam_api64.dll";
        sdk64_o = directory + @"\steam_api64_o.dll";
        config = directory + @"\SmokeAPI.json";
        cache = directory + @"\SmokeAPI.cache.json";
    }

    internal static void WriteConfig(StreamWriter writer, SortedList<string, (DlcType type, string name, string icon)> overrideDlc, SortedList<string, (DlcType type, string name, string icon)> injectDlc, InstallForm installForm = null)
    {
        writer.WriteLine("{");
        writer.WriteLine("  \"$version\": 1,");
        writer.WriteLine("  \"logging\": false,");
        writer.WriteLine("  \"hook_steamclient\": true,");
        writer.WriteLine("  \"unlock_all\": true,");
        if (overrideDlc.Count > 0)
        {
            writer.WriteLine("  \"override\": [");
            KeyValuePair<string, (DlcType type, string name, string icon)> lastOverrideDlc = overrideDlc.Last();
            foreach (KeyValuePair<string, (DlcType type, string name, string icon)> pair in overrideDlc)
            {
                string dlcId = pair.Key;
                (_, string dlcName, _) = pair.Value;
                writer.WriteLine($"    {dlcId}{(pair.Equals(lastOverrideDlc) ? "" : ",")}");
                if (installForm is not null)
                    installForm.UpdateUser($"Added override DLC to SmokeAPI.json with appid {dlcId} ({dlcName})", InstallationLog.Action, info: false);
            }
            writer.WriteLine("  ],");
        }
        else
            writer.WriteLine("  \"override\": [],");
        if (injectDlc.Count > 0)
        {
            writer.WriteLine("  \"dlc_ids\": [");
            KeyValuePair<string, (DlcType type, string name, string icon)> lastInjectDlc = injectDlc.Last();
            foreach (KeyValuePair<string, (DlcType type, string name, string icon)> pair in injectDlc)
            {
                string dlcId = pair.Key;
                (_, string dlcName, _) = pair.Value;
                writer.WriteLine($"    {dlcId}{(pair.Equals(lastInjectDlc) ? "" : ",")}");
                if (installForm is not null)
                    installForm.UpdateUser($"Added inject DLC to SmokeAPI.json with appid {dlcId} ({dlcName})", InstallationLog.Action, info: false);
            }
            writer.WriteLine("  ],");
        }
        else
            writer.WriteLine("  \"dlc_ids\": [],");
        writer.WriteLine("  \"auto_inject_inventory\": true,");
        writer.WriteLine("  \"inventory_items\": []");
        writer.WriteLine("}");
    }

    internal static async Task Uninstall(string directory, InstallForm installForm = null, bool deleteConfig = true) => await Task.Run(() =>
    {
        directory.GetSmokeApiComponents(out string sdk32, out string sdk32_o, out string sdk64, out string sdk64_o, out string config, out string cache);
        if (File.Exists(sdk32_o))
        {
            if (File.Exists(sdk32))
            {
                File.Delete(sdk32);
                if (installForm is not null)
                    installForm.UpdateUser($"Deleted SmokeAPI: {Path.GetFileName(sdk32)}", InstallationLog.Action, info: false);
            }
            File.Move(sdk32_o, sdk32);
            if (installForm is not null)
                installForm.UpdateUser($"Restored Steamworks: {Path.GetFileName(sdk32_o)} -> {Path.GetFileName(sdk32)}", InstallationLog.Action, info: false);
        }
        if (File.Exists(sdk64_o))
        {
            if (File.Exists(sdk64))
            {
                File.Delete(sdk64);
                if (installForm is not null)
                    installForm.UpdateUser($"Deleted SmokeAPI: {Path.GetFileName(sdk64)}", InstallationLog.Action, info: false);
            }
            File.Move(sdk64_o, sdk64);
            if (installForm is not null)
                installForm.UpdateUser($"Restored Steamworks: {Path.GetFileName(sdk64_o)} -> {Path.GetFileName(sdk64)}", InstallationLog.Action, info: false);
        }
        if (deleteConfig && File.Exists(config))
        {
            File.Delete(config);
            if (installForm is not null)
                installForm.UpdateUser($"Deleted configuration: {Path.GetFileName(config)}", InstallationLog.Action, info: false);
        }
        if (deleteConfig && File.Exists(cache))
        {
            File.Delete(cache);
            if (installForm is not null)
                installForm.UpdateUser($"Deleted cache: {Path.GetFileName(cache)}", InstallationLog.Action, info: false);
        }
    });

    internal static async Task Install(string directory, ProgramSelection selection, InstallForm installForm = null, bool generateConfig = true) => await Task.Run(() =>
    {
        directory.GetCreamApiComponents(out _, out _, out _, out _, out string oldConfig);
        if (File.Exists(oldConfig))
        {
            File.Delete(oldConfig);
            if (installForm is not null)
                installForm.UpdateUser($"Deleted old CreamAPI configuration: {Path.GetFileName(oldConfig)}", InstallationLog.Action, info: false);
        }
        directory.GetSmokeApiComponents(out string sdk32, out string sdk32_o, out string sdk64, out string sdk64_o, out string config, out _);
        if (File.Exists(sdk32) && !File.Exists(sdk32_o))
        {
            File.Move(sdk32, sdk32_o);
            if (installForm is not null)
                installForm.UpdateUser($"Renamed Steamworks: {Path.GetFileName(sdk32)} -> {Path.GetFileName(sdk32_o)}", InstallationLog.Action, info: false);
        }
        if (File.Exists(sdk32_o))
        {
            Properties.Resources.Steamworks32.Write(sdk32);
            if (installForm is not null)
                installForm.UpdateUser($"Wrote SmokeAPI: {Path.GetFileName(sdk32)}", InstallationLog.Action, info: false);
        }
        if (File.Exists(sdk64) && !File.Exists(sdk64_o))
        {
            File.Move(sdk64, sdk64_o);
            if (installForm is not null)
                installForm.UpdateUser($"Renamed Steamworks: {Path.GetFileName(sdk64)} -> {Path.GetFileName(sdk64_o)}", InstallationLog.Action, info: false);
        }
        if (File.Exists(sdk64_o))
        {
            Properties.Resources.Steamworks64.Write(sdk64);
            if (installForm is not null)
                installForm.UpdateUser($"Wrote SmokeAPI: {Path.GetFileName(sdk64)}", InstallationLog.Action, info: false);
        }
        if (generateConfig)
        {
            IEnumerable<KeyValuePair<string, (DlcType type, string name, string icon)>> overrideDlc = selection.AllDlc.Except(selection.SelectedDlc);
            foreach ((string id, string name, SortedList<string, (DlcType type, string name, string icon)> extraDlc) in selection.ExtraSelectedDlc)
                overrideDlc = overrideDlc.Except(extraDlc);
            IEnumerable<KeyValuePair<string, (DlcType type, string name, string icon)>> injectDlc = new List<KeyValuePair<string, (DlcType type, string name, string icon)>>();
            if (selection.AllDlc.Count > 64 || selection.ExtraDlc.Any(e => e.dlc.Count > 64))
            {
                injectDlc = injectDlc.Concat(selection.SelectedDlc.Where(pair => pair.Value.type is DlcType.SteamHidden));
                foreach ((string id, string name, SortedList<string, (DlcType type, string name, string icon)> extraDlc) in selection.ExtraSelectedDlc)
                    if (selection.ExtraDlc.Where(e => e.id == id).Single().dlc.Count > 64)
                        injectDlc = injectDlc.Concat(extraDlc.Where(pair => pair.Value.type is DlcType.SteamHidden));
            }
            if (overrideDlc.Any() || injectDlc.Any())
            {
                if (installForm is not null)
                    installForm.UpdateUser("Generating SmokeAPI configuration for " + selection.Name + $" in directory \"{directory}\" . . . ", InstallationLog.Operation);
                File.Create(config).Close();
                StreamWriter writer = new(config, true, Encoding.UTF8);
                WriteConfig(writer,
                    new(overrideDlc.ToDictionary(pair => pair.Key, pair => pair.Value), PlatformIdComparer.Strings),
                    new(injectDlc.ToDictionary(pair => pair.Key, pair => pair.Value), PlatformIdComparer.Strings),
                    installForm);
                writer.Flush();
                writer.Close();
            }
            else if (File.Exists(config))
            {
                File.Delete(config);
                if (installForm is not null)
                    installForm.UpdateUser($"Deleted unnecessary configuration: {Path.GetFileName(config)}", InstallationLog.Action, info: false);
            }
        }
    });
}
