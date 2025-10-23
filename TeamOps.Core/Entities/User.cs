using TeamOps.Core.Common;

namespace TeamOps.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;       // usado para login
        public string? CodigoFJ { get; set; }                   // opcional
        public string Name { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public AccessLevel AccessLevel { get; set; } = AccessLevel.Basic;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}