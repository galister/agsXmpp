using System;

namespace agsXMPP.Util
{
    static class Runtime
    {
        public static bool IsMono()
        {
            Type t = Type.GetType ("Mono.Runtime");
            if (t != null)
                 return true;
            
            return false;
        }
    }
}