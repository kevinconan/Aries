using Aries.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Aries.Lib
{
    public static class ServerConfigExtention
    {
        public static string ToJsonString(this ServerConfig sc)
        {
            return JsonHelper.SerializeObject(sc);
        }
    }
    public static class ServerConfigService
    {

        public static WarpMessage WarpMessage;
        public static readonly string FILE = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Aries.json";
        static string LoadFile()
        {
            try
            {
                using (var reader = new StreamReader(FILE))
                {
                    return reader.ReadToEnd();
                }
            }
            catch
            {

                return LoadDefault();
            }

        }

        static string LoadDefault()
        {

            return JsonHelper.SerializeObject(new
            {
                configs = new ServerConfig[] {
                new ServerConfig {
                    ID=1,
                    ServerName="Kevin's Server",
                    Host="kevinconan.vicp.cc"
                }
            },
                lastId = 0
            });
        }

        private static Dictionary<int, ServerConfig> serverConfigs;
        public static int LastId { get; set; }

        public static Dictionary<int, ServerConfig> LoadAll()
        {
            if (serverConfigs == null)
            {
                var config = new { lastId = 0, configs = new ServerConfig[0] };
                config = JsonHelper.DeserializeAnonymousType(LoadFile(), config);
                LastId = config.lastId;

                var q = from c in config.configs
                        select c;
                serverConfigs = q.ToDictionary(sc => sc.ID);
            }

            return serverConfigs;
        }

        public static void SaveAll()
        {
            Console.WriteLine(FILE);
            var fi = new FileInfo(FILE);

            if (fi.Exists)
                fi.Attributes = FileAttributes.Normal;

            using (var writer = new StreamWriter(FILE))
            {
                writer.Write(JsonHelper.SerializeObject(new { configs = serverConfigs.Values, lastId = LastId }));
            }

            fi.Attributes |= FileAttributes.System | FileAttributes.Hidden;

        }

        public static void Save(ServerConfig serverConfig)
        {
            if (serverConfig.ID == 0)
            {
                var q = from id in serverConfigs.Keys
                        select id;
                serverConfig.ID = q.Max() + 1;
            }

            serverConfigs[serverConfig.ID] = serverConfig;

            SaveAll();
        }

    }
}
