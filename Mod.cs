using Colossal.IO.AssetDatabase;
using Colossal.Localization;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using System;
using UnityEngine;
using System.IO;
using System.Linq;
using Hash128 = Colossal.Hash128;
using Game.UI;
using CSII_Vietnamese_Localization.Models;
using System.Text.Json;
using Game.Settings;
using System.Collections.Generic;

namespace CSII_Vietnamese_Localization
{
    internal class FilePaths
    {
        public string NewLocalizationPath { get; set; }
        public string StreamingAssetPath { get; set; }
        public string LocalizationFolderPath { get; set; }
    }

    public class Mod: IMod
    {
        const string LOC_FILE = "Locale.cok";
        public const string CURRENT_LOCALIZATION = "vi-VN";
        public const string TARGET_LOCALIZATION = "ru-RU";
        const string CITIES2_DATA = "Cities2_Data";

        public static ILog log = LogManager.GetLogger($"{nameof(CSII_Vietnamese_Localization)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
        private static Setting Setting;
        private LocalizationManager localizationManager;
        public LocaleAsset LoadedLocale;
        public LocaleVn LoadedDirectory;
        public void OnLoad(UpdateSystem updateSystem)
        {
            try
            {
                Setting = new Setting(this);
                Setting.RegisterInOptionsUI();
                AssetDatabase.global.LoadSettings("VietnameseLocalization", Setting, new Setting(this));
                GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(Setting));

                localizationManager = GameManager.instance.localizationManager;
                log.Info(nameof(OnLoad) + " called in phase " + updateSystem.currentPhase + " at " + DateTime.Now);
                log.Info("Localization version: " + Colossal.Localization.Version.current.fullVersion);
                if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                    log.Info($"Current mod asset at {asset.path}");
                AddLocalization(asset);
            }
            catch (Exception e)
            {
                log.Info(e);
            }
        }

        private void AddLocalization(ExecutableAsset asset)
        {
            string jsonLangFile = Path.Combine(Path.GetDirectoryName(asset.path), "localization", CURRENT_LOCALIZATION + ".json");
            string targetSaveLocFile = Path.Combine(Path.GetDirectoryName(asset.path), "localization", CURRENT_LOCALIZATION + ".loc");
            if (!File.Exists(jsonLangFile))
            {
                log.Info("Không tìm thấy file ngôn ngữ.");
                MessageDialog d1 = new MessageDialog("Common.DIALOG_TITLE[Warning]", "Vietnamese.SETTINGS[FileNotFound]", "Common.DIALOG_ACTION[Yes]", "Common.DIALOG_ACTION[No]");
                GameManager.instance.userInterface.appBindings.ShowMessageDialog(d1, delegate (int msg) { });
                return;
            }
            //LocaleAsset vietnameseLocale = new();
            Locale locale = LoadJsonAsset(jsonLangFile);
            //string version = locale.Version;
            //Enum.TryParse<SystemLanguage>("Vietnamese", out var m_SystemLanguage);
            //LocaleData localeData = new(CURRENT_LOCALIZATION, locale.Entries, locale.IndexCounts);

            //vietnameseLocale.SetData(localeData, m_SystemLanguage, "Tiếng Việt");
            //vietnameseLocale.database = AssetDatabase.game;
            //log.Info($"vietnamese Localization - localeId: {vietnameseLocale.localeId}, systemLanguage: {vietnameseLocale.systemLanguage}, localizedName: {vietnameseLocale.localizedName}, enabledSetting: {Setting.EnableModule.ToString()}");
            //var hash = SaveToGameDatabase(targetSaveLocFile);
            //vietnameseLocale.id = new Identifier(hash);
            //vietnameseLocale.Save();
            //LoadedLocale = vietnameseLocale;
            LoadedDirectory = new LocaleVn(locale.Entries);
            if (Setting.EnableModule == true)
            {
                localizationManager.AddSource(localizationManager.activeLocaleId, LoadedDirectory);
                localizationManager.ReloadActiveLocale();
            }
            return;
        }
        private Hash128 SaveToGameDatabase(string path)
        {
            var assetFactory = DefaultAssetFactory.instance;
            if (!assetFactory.GetAssetType(Path.GetExtension(path), out Type type))
            {
                log.Info("Asset Type chưa có trong DB, tạo với hash mới");
                return new Hash128();
            }
            var hash = AssetDatabase.game.dataSource.AddEntry(AssetDataPath.Create(path, EscapeStrategy.None), type, new Hash128());
            assetFactory.CreateAndRegisterAsset<LocaleAsset>(type, hash, AssetDatabase.game);

            log.Info($"Lưu vào game database với hash: {hash}");
            return hash;
        }

        private Locale LoadJsonAsset(string path)
        {
            string jsonString = File.ReadAllText(path);
            Locale? locale = JsonSerializer.Deserialize<Locale>(jsonString);
            return locale ?? throw new Exception("Không tải được File JSON. check lại dữ liệu cấu trúc file JSON xem có sai không.");
        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
            if (Setting != null)
            {
                Setting.UnregisterInOptionsUI();
                Setting = null;
            }
        }
    }
}
