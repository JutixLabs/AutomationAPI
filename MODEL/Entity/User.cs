namespace AutomationAPI.MODEL.Entity
{
    public class User
    {
        public string ID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        //public bool IsEmailVerified { get; set; } = false;
        //public string EmailVerificationToken { get; set; }
    }
}
