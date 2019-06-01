using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aries.Model
{
    public class ServerConfig : ObservableObject, ICloneable
    {
        private int id;
        private string serverName;
        private string host;
        private int loginPort = 8484;
        private int shopPort = 8600;
        private int ahPort = 8700;
        public int chatPort = 8238;
        private int channelStartPort = 7575;
        private int channelEndPort = 7580;
        private string exeLocation = "MapleStory.exe";

        public int ID
        {
            get { return id; }
            set
            {
                id = value;
                RaisePropertyChanged("ID");
            }
        }

        [Required]
        [StringLength(50)]
        public string ServerName { get { return serverName; } set { serverName = value; RaisePropertyChanged("ServerName"); } }

        [Required]
        [StringLength(50)]
        public string Host { get { return host; } set { host = value; RaisePropertyChanged("Host"); } }

        [Required]
        [Range(1, 65535, ErrorMessage = "端口号范围必须介于1到65535之间")]
        public int LoginPort { get { return loginPort; } set { loginPort = value; RaisePropertyChanged("LoginPort"); } }

        [Required]
        [Range(1, 65535, ErrorMessage = "端口号范围必须介于1到65535之间")]
        public int ShopPort { get { return shopPort; } set { shopPort = value; RaisePropertyChanged("ShopPort"); } }

        [Required]
        [Range(1, 65535, ErrorMessage = "端口号范围必须介于1到65535之间")]
        public int AhPort { get { return ahPort; } set { ahPort = value; RaisePropertyChanged("AhPort"); } }

        [Required]
        [Range(1, 65535, ErrorMessage = "端口号范围必须介于1到65535之间")]
        public int ChatPort { get { return chatPort; } set { chatPort = value; RaisePropertyChanged("ChatPort"); } }

        [Required]
        [Range(1, 65535, ErrorMessage = "端口号范围必须介于1到65535之间")]
        public int ChannelStartPort { get { return channelStartPort; } set { channelStartPort = value; RaisePropertyChanged("ChannelStartPort"); } }

        [Required]
        [Range(1, 65535, ErrorMessage = "端口号范围必须介于1到65535之间")]
        public int ChannelEndPort { get { return channelEndPort; } set { channelEndPort = value; RaisePropertyChanged("ChannelEndPort"); } }

        [Required]
        [StringLength(255)]
        public string ExeLocation { get { return exeLocation; } set { exeLocation = value; RaisePropertyChanged("ExeLocation"); } }

        public object Clone()
        {
            return new ServerConfig
            {
                ID = ID,
                ServerName = ServerName,
                Host = Host,
                LoginPort = LoginPort,
                ShopPort = ShopPort,
                AhPort = AhPort,
                ChatPort = ChatPort,
                ChannelStartPort = ChannelStartPort,
                ChannelEndPort = ChannelEndPort,
                ExeLocation = ExeLocation,
            };
        }
    }
}
