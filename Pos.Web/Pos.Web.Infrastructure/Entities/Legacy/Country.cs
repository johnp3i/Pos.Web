using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class Country
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [Column("Country")]
    [StringLength(100)]
    public string Country1 { get; set; } = null!;

    [StringLength(10)]
    public string CountryCode { get; set; } = null!;

    [Column("ISO2")]
    [StringLength(2)]
    [Unicode(false)]
    public string Iso2 { get; set; } = null!;

    [Column("ISO3")]
    [StringLength(3)]
    [Unicode(false)]
    public string Iso3 { get; set; } = null!;

    [Column("CurrencyID")]
    public byte? CurrencyId { get; set; }

    [InverseProperty("Country")]
    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
}
