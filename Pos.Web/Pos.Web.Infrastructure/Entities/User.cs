using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pos.Web.Infrastructure.Entities;

/// <summary>
/// User entity mapped to legacy dbo.Users table
/// </summary>
[Table("Users", Schema = "dbo")]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }

    public int? Code { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Surname { get; set; }

    [Required]
    public byte PositionTypeID { get; set; }

    [Required]
    [MaxLength(20)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public bool IsActive { get; set; }

    [Required]
    public DateTime RegistrationDate { get; set; }

    [Required]
    public byte ColorTypeID { get; set; }

    [Required]
    public byte DisplayOrder { get; set; }

    public byte? DataSourceTypeID { get; set; }

    [Required]
    public bool IsAllowToDeleteInvoices { get; set; }

    /// <summary>
    /// Full name for display purposes
    /// </summary>
    [NotMapped]
    public string FullName => string.IsNullOrEmpty(Surname) 
        ? Name 
        : $"{Name} {Surname}";

    /// <summary>
    /// Role name based on PositionTypeID
    /// 1 = Cashier, 2 = Admin, 3 = Manager, 4 = Waiter
    /// </summary>
    [NotMapped]
    public string Role => PositionTypeID switch
    {
        1 => "Cashier",
        2 => "Admin",
        3 => "Manager",
        4 => "Waiter",
        _ => "Unknown"
    };
}
