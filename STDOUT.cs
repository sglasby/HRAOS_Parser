using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HRAOS_take2
{
    public class STDOUT
    {
        public RichTextBox stdout;  // not-public, once needed methods are added...

        public STDOUT(RichTextBox tt)
        {
            stdout = tt;
        }

        public void print(string format, params object[] args )
        {
            string ss = string.Format(format, args);
            stdout.AppendText(ss);
        } // print()

    } // class STDOUT

} // namespace Haxima_Runtime_Attributes_Object_System
