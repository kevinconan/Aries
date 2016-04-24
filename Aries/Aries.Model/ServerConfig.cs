using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aries.Model
{
    public class ServerConfig : ICloneable
    {
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public string ServerName { get; set; }

        [Required]
        [StringLength(50)]
        public string Host { get; set; }

        [Required]
        [Range(1, 65535, ErrorMessage = "端口号范围必须介于1到65535之间")]
        public int LoginPort { get; set; } = 8484;

        [Required]
        [Range(1, 65535, ErrorMessage = "端口号范围必须介于1到65535之间")]
        public int ShopPort { get; set; } = 8600;

        [Required]
        [Range(1, 65535, ErrorMessage = "端口号范围必须介于1到65535之间")]
        public int ChannelStartPort { get; set; } = 7575;

        [Required]
        [Range(1, 65535, ErrorMessage = "端口号范围必须介于1到65535之间")]
        public int ChannelEndPort { get; set; } = 7580;

        [Required]
        [StringLength(255)]
        public string ExeLocation { get; set; } = "MapleStory.exe";

        public object Clone()
        {
            return new ServerConfig
            {
                ID = ID,
                ServerName = ServerName,
                Host = Host,
                LoginPort = LoginPort,
                ShopPort = ShopPort,
                ChannelStartPort = ChannelStartPort,
                ChannelEndPort = ChannelEndPort,
                ExeLocation = ExeLocation,
            };
        }
    }
}
