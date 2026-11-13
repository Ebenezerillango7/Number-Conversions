using System;
using System.Collections.Generic;
using NumberConversions.Plugin.UI;
using Paratext.PluginInterfaces;

namespace NumberConversions.Plugin
{
    public class NumberConversionsPlugin : IParatextStandalonePlugin
    {
        public const string PluginName = "Number Conversions";

        public string Name => PluginName;
        public Version Version => new Version(1, 0, 0);
        public string VersionString => "1.0.0";
        public string Publisher => "Ebenezer Illango";

        public string GetDescription(string locale)
        {
            return "Normalizes numbers in Tamil scripture text: " +
                   "0-199 digits to Tamil text words (cardinals & ordinals), " +
                   "200+ Tamil text words to digits with South Asian / Indian grouping (1,23,456,789).";
        }

        public IDataFileMerger? GetMerger(IPluginHost host, string dataIdentifier) => null;

        public IEnumerable<PluginMenuEntry> PluginMenuEntries
        {
            get
            {
                yield return new PluginMenuEntry(
                    "Tamil Number Conversions...",
                    RunPlugin,
                    PluginMenuLocation.ScrTextTools,
                    imagePath: null,
                    customLocation: null);

                yield return new PluginMenuEntry(
                    "Uninstall Tamil Number Conversions...",
                    RunUninstaller,
                    PluginMenuLocation.ScrTextTools,
                    imagePath: null,
                    customLocation: null);
            }
        }

        private void RunPlugin(IPluginHost host, IParatextChildState? childState)
        {
            var window = new ConversionMainWindow(host, childState, this);
            window.Show();
        }

        private void RunUninstaller(IPluginHost host, IParatextChildState? childState)
        {
            PluginUninstaller.InitiateUninstall();
        }
    }
}
