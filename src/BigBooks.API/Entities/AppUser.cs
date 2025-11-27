using BigBooks.API.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BigBooks.API.Entities
{
    public class AppUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Key { get; set; }

        [Required]
        public Role Role { get; set; }

        /// <summary>
        /// UserEmail is user id for API
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public decimal Wallet { get; set; }

        public IList<AccountTransaction> Transactions { get; set; } = new List<AccountTransaction>();
    }
}
