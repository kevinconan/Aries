using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aries.Injection
{
    public class TestInjection
    {
        public static int Start(string argument)
        {
            Application.Run(new Form1());
            return 0;
        }
    }
}
