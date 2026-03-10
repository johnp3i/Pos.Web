using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("CategorySectionType")]
public partial class CategorySectionType
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [StringLength(50)]
    public string Section { get; set; } = null!;

    [InverseProperty("CategorySectionType")]
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
