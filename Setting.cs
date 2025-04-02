using Colossal.IO.AssetDatabase;
using Colossal.Localization;
using Game.Modding;
using Game.Settings;
using Game.UI.Widgets;
using Game.SceneFlow;
using Game.UI;
using Colossal;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CSII_Vietnamese_Localization
{
    public class LocaleVn : IDictionarySource
    {
        private readonly Dictionary<string, string> Entries;

        public LocaleVn(Dictionary<string, string> Entries)
        {
            this.Entries = Entries;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return Entries;
        }

        public void Unload()
        {

        }
    }

    public class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Setting;
        public LocaleEN(Setting setting)
        {
            m_Setting = setting;
        }
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "Tiếng Việt" },
                { m_Setting.GetOptionTabLocaleID(Setting.K_SECTION), "Main" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ButtonExportLocale)), "Xuất File Text" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ButtonExportLocale)), $"Xuất file text của ngôn ngữ được chọn." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.GameLanguageDropdown)), "Ngôn ngữ" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.GameLanguageDropdown)), $"Chọn ngôn ngữ để xuất file text." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.EnableModule)), "Bật Tiếng Việt" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.EnableModule)), $"Bật Tiếng Việt ghi đè lên ngôn ngữ đang chọn của game" },
                { "Vietnamese.SETTINGS[ExportFolderNotFound]", $"Không tìm thấy folder export" },
                { "Vietnamese.SETTINGS[ExportCompletedTo]", $"Đã xuất ra folder export của mod." },
                { "Vietnamese.SETTINGS[FileNotFound]", $"Không tìm thấy file việt hoá." },
            };
        }
        public void Unload()
        {

        }
    }

    [FileLocation("VietnameseLocalization")]
    public class Setting : ModSetting
    {
        public const string K_SECTION = "Main";
        private readonly LocalizationManager localizationManager = GameManager.instance.localizationManager;
        private readonly Mod Module;
        public Setting(Mod mod) : base(mod)
        {
            Module = mod;
        }

        public override void SetDefaults()
        {
            throw new System.NotImplementedException();
        }

        [SettingsUISection(K_SECTION)]
        [SettingsUISetter(typeof(Setting), nameof(SetEnableModule))]
        public bool EnableModule { get; set; }

        public void SetEnableModule(bool value)
        {
            if (value == true)
            {
                localizationManager.AddSource(localizationManager.activeLocaleId, Module.LoadedDirectory);
                localizationManager.ReloadActiveLocale();
            }
            else
            {
                localizationManager.RemoveSource(localizationManager.activeLocaleId, Module.LoadedDirectory);
            }
        }

        [SettingsUIDropdown(typeof(Setting), nameof(GetStringDropdownItems))]
        [SettingsUISection(K_SECTION)]
        public string GameLanguageDropdown { get; set; } = "en-US";

        public DropdownItem<string>[] GetStringDropdownItems()
        {
            string[] supportedLocales = localizationManager.GetSupportedLocales();
            // array map from supportedLocales
            var dropdownItems = new DropdownItem<string>[supportedLocales.Length];
            for (int i = 0; i < supportedLocales.Length; i++)
            {
                dropdownItems[i] = new DropdownItem<string>
                {
                    displayName = localizationManager.GetLocalizedName(supportedLocales[i]),
                    value = supportedLocales[i]
                };
            }
            return dropdownItems;
        }

        [SettingsUIButton]
        [SettingsUISection(K_SECTION)]
        public bool ButtonExportLocale
        {
            set
            {
                GameManager.instance.modManager.TryGetExecutableAsset(Module, out var ModuleDll);
                var targetPath = Path.GetDirectoryName(ModuleDll.path);
                if (targetPath == null)
                {
                    MessageDialog d1 = new MessageDialog("Common.DIALOG_TITLE[Warning]", "Vietnamese.SETTINGS[ExportFolderNotFound]", "Common.DIALOG_ACTION[Yes]", "Common.DIALOG_ACTION[No]");
                    GameManager.instance.userInterface.appBindings.ShowMessageDialog(d1, delegate (int msg) {});
                    return;
                }
                int count = 0;
                foreach (LocaleAsset localeAsset in AssetDatabase.global.GetAssets<LocaleAsset>())
                {
                    if (localeAsset.localeId == GameLanguageDropdown)
                    {
                        try
                        {
                            string outputFilePath = Path.Combine(targetPath, "export", $"{localeAsset.localeId}_{count++}.json");
                            Directory.CreateDirectory(Path.Combine(targetPath, "export"));
                            var localeData = new Dictionary<string, object>
                            {
                                ["IndexCounts"] = localeAsset.data.indexCounts,
                                ["Entries"] = localeAsset.data.entries,
                                ["LocaleId"] = localeAsset.localeId,
                                ["LocaleName"] = localeAsset.localizedName,
                                ["Version"] = Game.Version.current.shortVersion
                            };
                            var options = new JsonSerializerOptions
                            {
                                WriteIndented = true
                            };
                            string jsonString = JsonSerializer.Serialize(localeData, options);
                            File.WriteAllText(outputFilePath, jsonString);
                        }
                        catch (System.Exception e)
                        {
                            MessageDialog dialog = new MessageDialog("Common.DIALOG_TITLE[Warning]", e.Message, "Common.DIALOG_ACTION[Yes]");
                            GameManager.instance.userInterface.appBindings.ShowMessageDialog(dialog, delegate (int msg) { });
                        }
                    }
                }
                MessageDialog dialog2 = new MessageDialog("Common.DIALOG_TITLE[Warning]", $"Complete", "Common.DIALOG_ACTION[Yes]");
                GameManager.instance.userInterface.appBindings.ShowMessageDialog(dialog2, delegate (int msg) { });
            }
        }
    }
}
