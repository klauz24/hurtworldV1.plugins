using System;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("Raid Time Limit (Hurtworld Legacy)", "klauz24", "1.0.1")]
    internal class RaidTimeLimit : HurtworldPlugin
    {
        private Configuration _config;

        private class Configuration
        {
            [JsonProperty("From")]
            public int From = 23;

            [JsonProperty("To")]
            public int To = 10;

            [JsonProperty("Tag")]
            public string Tag = "<color=#ff0000>[Raid Time Limit]</color>";

            [JsonProperty("Message")]
            public string Message = "It is not allowed to raid between {from} and {to}.";

            public string ToJson() => JsonConvert.SerializeObject(this);

            public Dictionary<string, object> ToDictionary() => JsonConvert.DeserializeObject<Dictionary<string, object>>(ToJson());
        }

        protected override void LoadDefaultConfig() => _config = new Configuration();

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                _config = Config.ReadObject<Configuration>();
                if (_config == null)
                {
                    throw new JsonException();
                }

                if (!_config.ToDictionary().Keys.SequenceEqual(Config.ToDictionary(x => x.Key, x => x.Value).Keys))
                {
                    Puts("Configuration appears to be outdated; updating and saving.");
                    SaveConfig();
                }
            }
            catch
            {
                Puts($"Configuration file {Name}.json is invalid; using defaults.");
                LoadDefaultConfig();
            }
        }

        protected override void SaveConfig()
        {
            Puts($"Configuration changes saved to {Name}.json");
            Config.WriteObject(_config, true);
        }

        private void OnEntitySpawned(NetworkViewData data)
        {
            if (data.PrefabId == 20 || data.PrefabId == 65)
            {
                Execute(data, false);
            }
            if (data.PrefabId == 85)
            {
                Execute(data, true);
            }
        }

        private void Execute(NetworkViewData data, bool direct)
        {
            var timeNow = DateTime.Now;
            if (timeNow.Hour >= _config.From || timeNow.Hour < _config.To)
            {
                if (direct)
                {
                    var session = GameManager.Instance.GetSession(data.NetworkView.owner);
                    var message = _config.Message.Replace("{from}", _config.From.ToString()).Replace("{to}", _config.To.ToString());
                    hurt.SendChatMessage(session, _config.Tag, message);
                    Destroy(data.NetworkView);
                }
                else
                {
                    Destroy(data.NetworkView);
                }
            }
        }

        private void Destroy(uLink.NetworkView view) => HNetworkManager.Instance.NetDestroy(view);
    }
}
