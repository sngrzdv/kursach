namespace kursach.AppData
{
    public static class CurrentUser
    {
        public static int Id { get; set; }
        public static int RoleId { get; set; }
        public static string FullName { get; set; }
        public static string Email { get; set; }

        public static void SetUser(Users user)
        {
            if (user != null)
            {
                Id = user.Id;
                RoleId = user.RoleId;
                FullName = $"{user.LastName} {user.FirstName}";
                Email = user.Email;
            }
        }

        public static void Clear()
        {
            Id = 0;
            RoleId = 0;
            FullName = string.Empty;
            Email = string.Empty;
        }

        public static bool IsAuthenticated => Id > 0;
        public static bool IsEmployer => RoleId == 2;
        public static bool IsJobSeeker => RoleId == 3;
        public static bool IsAdmin => RoleId == 1;
    }
}