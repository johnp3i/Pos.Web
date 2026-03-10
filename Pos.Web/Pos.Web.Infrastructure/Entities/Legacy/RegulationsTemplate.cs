using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class RegulationsTemplate
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime Timestamp { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    public string RegulationsJson { get; set; } = null!;

    [InverseProperty("RegulationsTemplate")]
    public virtual ICollection<PromotionalOffer> PromotionalOffers { get; set; } = new List<PromotionalOffer>();
}
