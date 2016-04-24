using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aries.Model
{
    [MetadataType(typeof(ServerConfig.Metadata))]
    public partial class ServerConfig
    {
        internal sealed class Metadata
        {
            [Required]
            [StringLength(50)]
            public string ServerName { get; set; }

            [Required]
            [StringLength(50)]
            public string Host { get; set; }

            [Required]
            [Range(1, 65535, ErrorMessage = "端口号范围必须介于1到65535之间")]
            public int LoginPort { get; set; }

            [Required]
            [Range(1, 65535, ErrorMessage = "端口号范围必须介于1到65535之间")]
            public int ShopPort { get; set; }

            [Required]
            [Range(1, 65535, ErrorMessage = "端口号范围必须介于1到65535之间")]
            public int ChannelStartPort { get; set; }

            [Required]
            [Range(1, 65535, ErrorMessage = "端口号范围必须介于1到65535之间")]
            public int ChannelEndPort { get; set; }

            [Required]
            [StringLength(255)]
            public string ExeLocation { get; set; }

            [Required]
            [StringLength(50)]
            public string Version { get; set; }
        }
    }
}
