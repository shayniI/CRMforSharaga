using Supabase.Gotrue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public static class UserData
    {
        private static Session? Session {  get;  set; }

        public static void SetSession (Session? s) => Session = s;
        public static Session GetSession() => Session!;
        public static void LogOut() => Session = null;
    }
}
