using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskbar
{
    public class TryCatchHelper
    {
        public static void Try(Action action, Action finallyAction)
        {
            try
            {
                action();
            }
            catch
            {

            }
            finally
            {
                finallyAction();
            }
        }
    }
}
