using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TicketScan
{
    public class UnsecurityDll
    {
        //二位码解密
        [DllImport("BAR2unsecurity.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Winapi)]
        public static extern long uncompress(string Instr, StringBuilder outStr, long nowyear);
    }
}
