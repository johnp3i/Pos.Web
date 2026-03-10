using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class ServingType
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Description { get; set; } = null!;

    public bool IsDisplayingOperatorsActive { get; set; }

    [StringLength(5)]
    [Unicode(false)]
    public string ShortName { get; set; } = null!;

    public bool IsCheckoutSavedAsPending { get; set; }

    public bool IsOmsForwardingActive { get; set; }

    public string? RequiredAddressPropertiesJson { get; set; }

    public bool IsAddressSelectionRequired { get; set; }

    public bool IsActive { get; set; }

    [Column("OMSParametersJson")]
    public string? OmsparametersJson { get; set; }

    [InverseProperty("ServingType")]
    public virtual ICollection<PendingInvoice> PendingInvoices { get; set; } = new List<PendingInvoice>();

    [InverseProperty("ServingType")]
    public virtual ICollection<ServiceTypeOperator> ServiceTypeOperators { get; set; } = new List<ServiceTypeOperator>();

    [InverseProperty("ServingType")]
    public virtual ICollection<ServingTypesToVat> ServingTypesToVats { get; set; } = new List<ServingTypesToVat>();
}
