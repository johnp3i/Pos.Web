using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class CategoriesToCategoriesExtra
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("CategoryID")]
    public int CategoryId { get; set; }

    [Column("CategoryExtrasID")]
    public int CategoryExtrasId { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("CategoriesToCategoriesExtraCategories")]
    public virtual Category Category { get; set; } = null!;

    [ForeignKey("CategoryExtrasId")]
    [InverseProperty("CategoriesToCategoriesExtraCategoryExtras")]
    public virtual Category CategoryExtras { get; set; } = null!;
}
