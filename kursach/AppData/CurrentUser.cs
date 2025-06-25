namespace kursach.AppData
{
    public static class CurrentUser
    {
        public static int Id { get; set; }
        public static int RoleId { get; set; }
        public static string FullName { get; set; }
        public static string Email { get; set; }

        public static void Clear()
        {
            Id = 0;
            RoleId = 0;
            FullName = string.Empty;
            Email = string.Empty;
        }
    }
}