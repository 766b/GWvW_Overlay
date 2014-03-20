using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWvW_Overlay.Resources.Lang
{
    public class LocalizedStrings
    {
        public LocalizedStrings()
        {
        }

        private static Strings locale = new Strings();
        public Strings Locale { get { return locale; } }
    }
}
