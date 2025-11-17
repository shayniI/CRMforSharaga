using Supabase.Gotrue;
using WpfApp1.Models;

namespace WpfApp1
{
    public static class UserData
    {
        private static Session? Session { get; set; }
        private static Profile? Profile { get; set; }

        public static void SetSession(Session? s) => Session = s;
        public static Session GetSession() => Session!;
        public static void SetProfile(Profile? p) => Profile = p;
        public static Profile? GetProfile() => Profile;
        public static void LogOut()
        {
            Session = null;
            Profile = null;
        }

        // Возвращаем прикладную роль из profiles; fallback — роль из auth (authenticated / anon)
        public static string? GetRole()
        {
            if (!string.IsNullOrEmpty(Profile?.Role))
                return Profile!.Role;
            return Session?.User?.Role; // это будет "authenticated" по умолчанию у вошедшего
        }
    }
}
