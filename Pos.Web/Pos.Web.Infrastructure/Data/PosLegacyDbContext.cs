using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pos.Web.Infrastructure.Entities.Legacy;

namespace Pos.Web.Infrastructure.Data;

public partial class PosLegacyDbContext : DbContext
{
    public PosLegacyDbContext(DbContextOptions<PosLegacyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Announcement> Announcements { get; set; }

    public virtual DbSet<ApiAuditLog> ApiAuditLogs { get; set; }

    public virtual DbSet<CategoriesToCategoriesExtra> CategoriesToCategoriesExtras { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CategoryItem> CategoryItems { get; set; }

    public virtual DbSet<CategoryItemsCostChangeHistory> CategoryItemsCostChangeHistories { get; set; }

    public virtual DbSet<CategoryItemsShortcutToolbar> CategoryItemsShortcutToolbars { get; set; }

    public virtual DbSet<CategoryItemsToFlavorGroup> CategoryItemsToFlavorGroups { get; set; }

    public virtual DbSet<CategoryItemsToShopifyProduct> CategoryItemsToShopifyProducts { get; set; }

    public virtual DbSet<CategoryOperationDepartment> CategoryOperationDepartments { get; set; }

    public virtual DbSet<CategorySectionType> CategorySectionTypes { get; set; }

    public virtual DbSet<ColorsType> ColorsTypes { get; set; }

    public virtual DbSet<Config> Configs { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerAddress> CustomerAddresses { get; set; }

    public virtual DbSet<CustomerCompany> CustomerCompanies { get; set; }

    public virtual DbSet<CustomerDiscountHistory> CustomerDiscountHistories { get; set; }

    public virtual DbSet<DataSourceType> DataSourceTypes { get; set; }

    public virtual DbSet<DiscountType> DiscountTypes { get; set; }

    public virtual DbSet<DocumentActionType> DocumentActionTypes { get; set; }

    public virtual DbSet<DocumentType> DocumentTypes { get; set; }

    public virtual DbSet<DocumentsHistory> DocumentsHistories { get; set; }

    public virtual DbSet<ErpCategoryItemRecipe> ErpCategoryItemRecipes { get; set; }

    public virtual DbSet<ErpCategoryItemRecipeIngredient> ErpCategoryItemRecipeIngredients { get; set; }

    public virtual DbSet<ExtraGroupsToExtrasCategory> ExtraGroupsToExtrasCategories { get; set; }

    public virtual DbSet<ExtrasGroup> ExtrasGroups { get; set; }

    public virtual DbSet<ExtrasSpecialFeaturesType> ExtrasSpecialFeaturesTypes { get; set; }

    public virtual DbSet<ExtrasToGroup> ExtrasToGroups { get; set; }

    public virtual DbSet<FeatureFlag> FeatureFlags { get; set; }

    public virtual DbSet<Flavor> Flavors { get; set; }

    public virtual DbSet<FlavorGroup> FlavorGroups { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceDeletionHistory> InvoiceDeletionHistories { get; set; }

    public virtual DbSet<InvoiceDiscountHistory> InvoiceDiscountHistories { get; set; }

    public virtual DbSet<InvoiceItem> InvoiceItems { get; set; }

    public virtual DbSet<InvoiceItemExtra> InvoiceItemExtras { get; set; }

    public virtual DbSet<InvoiceItemFlavor> InvoiceItemFlavors { get; set; }

    public virtual DbSet<InvoiceOrderProcessingHistory> InvoiceOrderProcessingHistories { get; set; }

    public virtual DbSet<InvoiceSpecialNoteType> InvoiceSpecialNoteTypes { get; set; }

    public virtual DbSet<InvoiceVatanalysis> InvoiceVatanalyses { get; set; }

    public virtual DbSet<InvoicesPaymentActionHistory> InvoicesPaymentActionHistories { get; set; }

    public virtual DbSet<InvoicesPaymentType> InvoicesPaymentTypes { get; set; }

    public virtual DbSet<InvoicesSpecialNote> InvoicesSpecialNotes { get; set; }

    public virtual DbSet<InvoicesStockProcessingHistory> InvoicesStockProcessingHistories { get; set; }

    public virtual DbSet<JccpluginType> JccpluginTypes { get; set; }

    public virtual DbSet<JccpluginsStatusCode> JccpluginsStatusCodes { get; set; }

    public virtual DbSet<JcctransactionsHistory> JcctransactionsHistories { get; set; }

    public virtual DbSet<LabelHeader> LabelHeaders { get; set; }

    public virtual DbSet<LogType> LogTypes { get; set; }

    public virtual DbSet<OmsUnprocessedInvoice> OmsUnprocessedInvoices { get; set; }

    public virtual DbSet<OnlineOrdersCustomerDatum> OnlineOrdersCustomerData { get; set; }

    public virtual DbSet<OnlineOrdersHistory> OnlineOrdersHistories { get; set; }

    public virtual DbSet<OpenDrawerHistory> OpenDrawerHistories { get; set; }

    public virtual DbSet<OrderLock> OrderLocks { get; set; }

    public virtual DbSet<PaymentType> PaymentTypes { get; set; }

    public virtual DbSet<PendingInvoice> PendingInvoices { get; set; }

    public virtual DbSet<PendingInvoiceItem> PendingInvoiceItems { get; set; }

    public virtual DbSet<PendingInvoiceItemsFlavor> PendingInvoiceItemsFlavors { get; set; }

    public virtual DbSet<Posdevice> Posdevices { get; set; }

    public virtual DbSet<PositionType> PositionTypes { get; set; }

    public virtual DbSet<PromotionalOffer> PromotionalOffers { get; set; }

    public virtual DbSet<RegulationsTemplate> RegulationsTemplates { get; set; }

    public virtual DbSet<ServerCommandsHistory> ServerCommandsHistories { get; set; }

    public virtual DbSet<ServerCommandsType> ServerCommandsTypes { get; set; }

    public virtual DbSet<ServiceTypeOperator> ServiceTypeOperators { get; set; }

    public virtual DbSet<ServingType> ServingTypes { get; set; }

    public virtual DbSet<ServingTypesToVat> ServingTypesToVats { get; set; }

    public virtual DbSet<ShopMap> ShopMaps { get; set; }

    public virtual DbSet<ShopsInfo> ShopsInfos { get; set; }

    public virtual DbSet<SyncQueue> SyncQueues { get; set; }

    public virtual DbSet<SystemLog> SystemLogs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserSession> UserSessions { get; set; }

    public virtual DbSet<Vat> Vats { get; set; }

    public virtual DbSet<VouchersRelease> VouchersReleases { get; set; }

    public virtual DbSet<VouchersUsageHistory> VouchersUsageHistories { get; set; }

    public virtual DbSet<ZreportType> ZreportTypes { get; set; }

    public virtual DbSet<ZreportsExport> ZreportsExports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Greek_CI_AS");

        modelBuilder.Entity<Address>(entity =>
        {
            entity.Property(e => e.DataSourceTypeId).HasDefaultValue((byte)1);

            entity.HasOne(d => d.Country).WithMany(p => p.Addresses).HasConstraintName("FK_Addresses_Countries");

            entity.HasOne(d => d.DataSourceType).WithMany(p => p.Addresses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Addresses_DataSourceTypes");
        });

        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.Property(e => e.IsShopHeaderVisible).HasDefaultValue(true);
        });

        modelBuilder.Entity<ApiAuditLog>(entity =>
        {
            entity.Property(e => e.Timestamp).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.User).WithMany(p => p.ApiAuditLogs).HasConstraintName("FK_ApiAuditLog_Users");
        });

        modelBuilder.Entity<CategoriesToCategoriesExtra>(entity =>
        {
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.CategoryExtras).WithMany(p => p.CategoriesToCategoriesExtraCategoryExtras)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CategoriesToCategoriesExtras_Categories1");

            entity.HasOne(d => d.Category).WithMany(p => p.CategoriesToCategoriesExtraCategories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CategoriesToCategoriesExtras_Categories");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.CategoryOperationDepartmentId).HasDefaultValue((byte)1);
            entity.Property(e => e.CategorySectionTypeId).HasDefaultValue((byte)1);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(1);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TimeStamp).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.CategoryOperationDepartment).WithMany(p => p.Categories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Categories_CategoryOperationDepartments");

            entity.HasOne(d => d.CategorySectionType).WithMany(p => p.Categories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Categories_CategorySectionType");

            entity.HasOne(d => d.ColorType).WithMany(p => p.Categories).HasConstraintName("FK_Categories_ColorsTypes");
        });

        modelBuilder.Entity<CategoryItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Table_1");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsFreeDrinkApplied).HasDefaultValue(true);
            entity.Property(e => e.IsStandAloneItem).HasDefaultValue(true);
            entity.Property(e => e.TimeStamp).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Category).WithMany(p => p.CategoryItemCategories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CategoryItems_Categories");

            entity.HasOne(d => d.ColorType).WithMany(p => p.CategoryItems).HasConstraintName("FK_CategoryItems_ColorsTypes");

            entity.HasOne(d => d.ExtrasCategory).WithMany(p => p.CategoryItemExtrasCategories).HasConstraintName("FK_CategoryItems_Categories1");

            entity.HasOne(d => d.ExtrasSpecialFeaturesType).WithMany(p => p.CategoryItems).HasConstraintName("FK_CategoryItems_ExtrasSpecialFeaturesType");

            entity.HasOne(d => d.LabelHeader).WithMany(p => p.CategoryItems).HasConstraintName("FK_CategoryItems_LabelHeaders");

            entity.HasOne(d => d.Vat).WithMany(p => p.CategoryItems).HasConstraintName("FK_CategoryItems_VATs");
        });

        modelBuilder.Entity<CategoryItemsCostChangeHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_CategoryItemsCostChangeHistory_1");

            entity.HasOne(d => d.CategoryItem).WithMany(p => p.CategoryItemsCostChangeHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CategoryItemsCostChangeHistory_CategoryItems");
        });

        modelBuilder.Entity<CategoryItemsShortcutToolbar>(entity =>
        {
            entity.HasOne(d => d.CategoryItem).WithMany(p => p.CategoryItemsShortcutToolbars)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CategoryItemsShortcutToolbar_CategoryItems");
        });

        modelBuilder.Entity<CategoryItemsToFlavorGroup>(entity =>
        {
            entity.HasKey(e => e.CategoryItemId).HasName("PK_CategoryItemsToFlavorGroups_1");

            entity.Property(e => e.CategoryItemId).ValueGeneratedNever();
            entity.Property(e => e.IsExtraEnabled).HasDefaultValue(true);

            entity.HasOne(d => d.CategoryItem).WithOne(p => p.CategoryItemsToFlavorGroup)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CategoryItemsToFlavorGroups_CategoryItems");

            entity.HasOne(d => d.FlavorGroups).WithMany(p => p.CategoryItemsToFlavorGroups)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CategoryItemsToFlavorGroups_CategoryItemsToFlavorGroups");
        });

        modelBuilder.Entity<CategoryItemsToShopifyProduct>(entity =>
        {
            entity.HasOne(d => d.CategoryItemExtra).WithMany(p => p.CategoryItemsToShopifyProductCategoryItemExtras).HasConstraintName("FK_CategoryItemsToShopifyProducts_CategoryItems1");

            entity.HasOne(d => d.CategoryItem).WithMany(p => p.CategoryItemsToShopifyProductCategoryItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CategoryItemsToShopifyProducts_CategoryItems");
        });

        modelBuilder.Entity<CategoryOperationDepartment>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsReceiptPrintEnable).HasDefaultValue(true);
        });

        modelBuilder.Entity<CategorySectionType>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<ColorsType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Colors");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Config>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_CountriesCodes");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Iso2).IsFixedLength();
            entity.Property(e => e.Iso3).IsFixedLength();
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(e => e.DataSourceTypeId).HasDefaultValue((byte)1);
            entity.Property(e => e.FreeDrinksFactor).HasDefaultValue((byte)8);
            entity.Property(e => e.RegistrationDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Company).WithMany(p => p.Customers).HasConstraintName("FK_Customers_CustomerCompanies");

            entity.HasOne(d => d.DataSourceType).WithMany(p => p.Customers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Customers_DataSourceTypes");

            entity.HasOne(d => d.DefaultAddress).WithMany(p => p.Customers).HasConstraintName("FK_Customers_Addresses");
        });

        modelBuilder.Entity<CustomerAddress>(entity =>
        {
            entity.HasOne(d => d.Address).WithMany(p => p.CustomerAddresses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerAddresses_Addresses");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerAddresses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerAddresses_Customers");
        });

        modelBuilder.Entity<CustomerCompany>(entity =>
        {
            entity.HasOne(d => d.Address).WithMany(p => p.CustomerCompanies).HasConstraintName("FK_CustomerCompanies_Addresses");
        });

        modelBuilder.Entity<CustomerDiscountHistory>(entity =>
        {
            entity.Property(e => e.Timestamp).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerDiscountHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerDiscountHistory_Customers");

            entity.HasOne(d => d.DiscountType).WithMany(p => p.CustomerDiscountHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerDiscountHistory_DiscountTypes");
        });

        modelBuilder.Entity<DataSourceType>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<DiscountType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_CustomerDiscountTypes");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<DocumentActionType>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<DocumentType>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasComment("This value determines whether the JDS service processes the specified document type.");
        });

        modelBuilder.Entity<DocumentsHistory>(entity =>
        {
            entity.HasOne(d => d.DocumentActionType).WithMany(p => p.DocumentsHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentsHistory_DocumentActionTypes");

            entity.HasOne(d => d.DocumentType).WithMany(p => p.DocumentsHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentsHistory_DocumentTypes");

            entity.HasOne(d => d.User).WithMany(p => p.DocumentsHistories).HasConstraintName("FK_DocumentsHistory_Users");
        });

        modelBuilder.Entity<ErpCategoryItemRecipe>(entity =>
        {
            entity.HasOne(d => d.CategoryItem).WithMany(p => p.ErpCategoryItemRecipes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Erp.CategoryItemRecipes_CategoryItems");

            entity.HasOne(d => d.User).WithMany(p => p.ErpCategoryItemRecipes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Erp.CategoryItemRecipes_Users");
        });

        modelBuilder.Entity<ErpCategoryItemRecipeIngredient>(entity =>
        {
            entity.HasOne(d => d.CategoryItemRecipe).WithMany(p => p.ErpCategoryItemRecipeIngredients)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Erp.CategoryItemRecipeIngredients_Erp.CategoryItemRecipes");
        });

        modelBuilder.Entity<ExtraGroupsToExtrasCategory>(entity =>
        {
            entity.HasOne(d => d.ColorType).WithMany(p => p.ExtraGroupsToExtrasCategories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExtraGroupsToExtrasCategories_ColorsTypes");

            entity.HasOne(d => d.ExtraCategory).WithMany(p => p.ExtraGroupsToExtrasCategories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExtraGroupsToExtrasCategories_Categories");

            entity.HasOne(d => d.ExtraGroup).WithMany(p => p.ExtraGroupsToExtrasCategories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExtraGroupsToExtrasCategories_ExtrasGroups");
        });

        modelBuilder.Entity<ExtrasSpecialFeaturesType>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<ExtrasToGroup>(entity =>
        {
            entity.HasOne(d => d.ExtraCategoryItem).WithMany(p => p.ExtrasToGroups)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExtrasToGroups_CategoryItems");

            entity.HasOne(d => d.ExtraGroup).WithMany(p => p.ExtrasToGroups)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExtrasToGroups_ExtrasGroups");
        });

        modelBuilder.Entity<FeatureFlag>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.FeatureFlags).HasConstraintName("FK_FeatureFlags_UpdatedBy");
        });

        modelBuilder.Entity<Flavor>(entity =>
        {
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);

            entity.HasOne(d => d.LabelHeader).WithMany(p => p.Flavors).HasConstraintName("FK_Flavors_LabelHeaders");
        });

        modelBuilder.Entity<FlavorGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_CategoryFlavors");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasMany(d => d.Flavors).WithMany(p => p.FlavorGroups)
                .UsingEntity<Dictionary<string, object>>(
                    "FlavorsToGroup",
                    r => r.HasOne<Flavor>().WithMany()
                        .HasForeignKey("FlavorId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_FlavorsToGroups_Flavors"),
                    l => l.HasOne<FlavorGroup>().WithMany()
                        .HasForeignKey("FlavorGroupId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_FlavorsToGroups_FlavorGroups"),
                    j =>
                    {
                        j.HasKey("FlavorGroupId", "FlavorId");
                        j.ToTable("FlavorsToGroups");
                        j.IndexerProperty<byte>("FlavorGroupId").HasColumnName("FlavorGroupID");
                        j.IndexerProperty<short>("FlavorId").HasColumnName("FlavorID");
                    });
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.Property(e => e.CustomerPaid).HasComment("If it is null then the customer has paid the exact invoice amount");
            entity.Property(e => e.DataSourceTypeId).HasDefaultValue((byte)1);
            entity.Property(e => e.TimeStamp).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TotalCost).HasComment("Price including VAT");

            entity.HasOne(d => d.Address).WithMany(p => p.Invoices).HasConstraintName("FK_Invoices_Addresses");

            entity.HasOne(d => d.Customer).WithMany(p => p.Invoices).HasConstraintName("FK_Invoices_Customers");

            entity.HasOne(d => d.DataSourceType).WithMany(p => p.Invoices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invoices_DataSourceTypes");

            entity.HasOne(d => d.PaymentType).WithMany(p => p.Invoices).HasConstraintName("FK_Invoices_PaymentTypes");

            entity.HasOne(d => d.PendingInvoice).WithMany(p => p.Invoices).HasConstraintName("FK_Invoices_PendingInvoices");

            entity.HasOne(d => d.Posdevice).WithMany(p => p.Invoices).HasConstraintName("FK_Invoices_POSDevices");

            entity.HasOne(d => d.ServingTypeToVat).WithMany(p => p.Invoices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invoices_ServingTypesToVAT");

            entity.HasOne(d => d.User).WithMany(p => p.Invoices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invoices_Users");
        });

        modelBuilder.Entity<InvoiceDeletionHistory>(entity =>
        {
            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceDeletionHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceDeletionHistory_InvoiceDeletionHistory");

            entity.HasOne(d => d.User).WithMany(p => p.InvoiceDeletionHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceDeletionHistory_Users");
        });

        modelBuilder.Entity<InvoiceDiscountHistory>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK_InvoiceDiscountHistory_1");

            entity.Property(e => e.InvoiceId).ValueGeneratedNever();
            entity.Property(e => e.IsPercentage).HasDefaultValue(true);

            entity.HasOne(d => d.Invoice).WithOne(p => p.InvoiceDiscountHistory)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceDiscountHistory_Invoices");
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_InvoiceItems_1");

            entity.Property(e => e.TotalSalePrice).HasComment("Price includes InvoicesItemsExtras sale price.");

            entity.HasOne(d => d.CategoryItem).WithMany(p => p.InvoiceItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceItems_CategoryItems");

            entity.HasOne(d => d.ErpCategoryItemRecipe).WithMany(p => p.InvoiceItems).HasConstraintName("FK_InvoiceItems_Erp.CategoryItemRecipes");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceItems_Invoices");
        });

        modelBuilder.Entity<InvoiceItemExtra>(entity =>
        {
            entity.HasOne(d => d.CategoryItemExtra).WithMany(p => p.InvoiceItemExtras)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceItemExtras_CategoryItems");

            entity.HasOne(d => d.InvoiceItem).WithMany(p => p.InvoiceItemExtras)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceItemExtras_InvoiceItems");
        });

        modelBuilder.Entity<InvoiceItemFlavor>(entity =>
        {
            entity.HasOne(d => d.Flavor).WithMany(p => p.InvoiceItemFlavors)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceItemFlavors_Flavors");

            entity.HasOne(d => d.InvoiceItemExtra).WithMany(p => p.InvoiceItemFlavors).HasConstraintName("FK_InvoiceItemFlavors_InvoiceItemFlavors");

            entity.HasOne(d => d.InvoiceItem).WithMany(p => p.InvoiceItemFlavors)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceItemFlavors_InvoiceItems");
        });

        modelBuilder.Entity<InvoiceOrderProcessingHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_InvoicesServiceCompletion");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceOrderProcessingHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceOrderProcessingHistory_Invoices");

            entity.HasOne(d => d.OrderServiceTypeOperator).WithMany(p => p.InvoiceOrderProcessingHistories).HasConstraintName("FK_InvoiceOrderProcessingHistory_ServiceTypeOperators");
        });

        modelBuilder.Entity<InvoiceSpecialNoteType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_InvoiceSpecialNotes");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<InvoiceVatanalysis>(entity =>
        {
            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceVatanalyses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceVATAnalysis_Invoices");

            entity.HasOne(d => d.Vat).WithMany(p => p.InvoiceVatanalyses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoiceVATAnalysis_VATs");
        });

        modelBuilder.Entity<InvoicesPaymentActionHistory>(entity =>
        {
            entity.Property(e => e.IsBatchAction).HasComment("Set to true when a batch insert is performed (eg. Voucher reconciliation)");

            entity.HasOne(d => d.DataSourceType).WithMany(p => p.InvoicesPaymentActionHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoicesPaymentActionHistory_DataSourceTypes");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoicesPaymentActionHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoicesPaymentActionHistory_Invoices");

            entity.HasOne(d => d.PaymentType).WithMany(p => p.InvoicesPaymentActionHistories).HasConstraintName("FK_InvoicesPaymentActionHistory_PaymentTypes");

            entity.HasOne(d => d.User).WithMany(p => p.InvoicesPaymentActionHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoicesPaymentActionHistory_Users");
        });

        modelBuilder.Entity<InvoicesPaymentType>(entity =>
        {
            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoicesPaymentTypes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoicesPaymentTypes_Invoices");
        });

        modelBuilder.Entity<InvoicesSpecialNote>(entity =>
        {
            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoicesSpecialNotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoicesSpecialNotes_Invoices");

            entity.HasOne(d => d.InvoiceSpecialNoteType).WithMany(p => p.InvoicesSpecialNotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoicesSpecialNotes_InvoiceSpecialNoteTypes");
        });

        modelBuilder.Entity<InvoicesStockProcessingHistory>(entity =>
        {
            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoicesStockProcessingHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvoicesStockProcessingHistory_Invoices");
        });

        modelBuilder.Entity<JccpluginType>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<JccpluginsStatusCode>(entity =>
        {
            entity.HasOne(d => d.Jccplugin).WithMany(p => p.JccpluginsStatusCodes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JCCPluginsStatusCodes_JCCPluginTypes");
        });

        modelBuilder.Entity<JcctransactionsHistory>(entity =>
        {
            entity.Property(e => e.Id).HasComment("This is the system id, that is sent to jcc client service.");

            entity.HasOne(d => d.Invoice).WithMany(p => p.JcctransactionsHistories).HasConstraintName("FK_JCCTransactionsHistory_Invoices");
        });

        modelBuilder.Entity<LabelHeader>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<LogType>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<OmsUnprocessedInvoice>(entity =>
        {
            entity.HasOne(d => d.Invoice).WithMany(p => p.OmsUnprocessedInvoices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OmsUnprocessedInvoices_Invoices");
        });

        modelBuilder.Entity<OnlineOrdersCustomerDatum>(entity =>
        {
            entity.Property(e => e.InvoiceId).ValueGeneratedNever();
        });

        modelBuilder.Entity<OnlineOrdersHistory>(entity =>
        {
            entity.HasOne(d => d.DataSourceType).WithMany(p => p.OnlineOrdersHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OnlineOrdersHistory_DataSourceTypes");
        });

        modelBuilder.Entity<OpenDrawerHistory>(entity =>
        {
            entity.HasOne(d => d.User).WithMany(p => p.OpenDrawerHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OpenDrawerHistory_Users");
        });

        modelBuilder.Entity<OrderLock>(entity =>
        {
            entity.HasIndex(e => e.LockExpiresAt, "IX_OrderLocks_LockExpiresAt").HasFilter("([IsActive]=(1))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LockAcquiredAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderLocks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderLocks_Orders");

            entity.HasOne(d => d.User).WithMany(p => p.OrderLocks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderLocks_Users");
        });

        modelBuilder.Entity<PaymentType>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<PendingInvoice>(entity =>
        {
            entity.Property(e => e.IsFromMap).HasDefaultValue(false);
            entity.Property(e => e.IsPercentage).HasDefaultValue(true);

            entity.HasOne(d => d.Address).WithMany(p => p.PendingInvoices).HasConstraintName("FK_PendingInvoices_Addresses");

            entity.HasOne(d => d.Customer).WithMany(p => p.PendingInvoices).HasConstraintName("FK_PendingInvoices_Customers");

            entity.HasOne(d => d.DataSourceType).WithMany(p => p.PendingInvoices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PendingInvoices_DataSourceTypes");

            entity.HasOne(d => d.ExportUser).WithMany(p => p.PendingInvoiceExportUsers).HasConstraintName("FK_PendingInvoices_Users");

            entity.HasOne(d => d.Posdevice).WithMany(p => p.PendingInvoices).HasConstraintName("FK_PendingInvoices_POSDevices");

            entity.HasOne(d => d.SavedByUser).WithMany(p => p.PendingInvoiceSavedByUsers).HasConstraintName("FK_PendingInvoices_Users1");

            entity.HasOne(d => d.ServingType).WithMany(p => p.PendingInvoices).HasConstraintName("FK_PendingInvoices_ServingTypes");
        });

        modelBuilder.Entity<PendingInvoiceItem>(entity =>
        {
            entity.Property(e => e.DbTimeStamp).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.CategoryItem).WithMany(p => p.PendingInvoiceItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PendingInvoiceItems_CategoryItems");

            entity.HasOne(d => d.PendingInvoice).WithMany(p => p.PendingInvoiceItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PendingInvoiceItems_PendingInvoices");
        });

        modelBuilder.Entity<PendingInvoiceItemsFlavor>(entity =>
        {
            entity.HasOne(d => d.Flavor).WithMany(p => p.PendingInvoiceItemsFlavors)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PendingInvoiceItemsFlavors_Flavors");

            entity.HasOne(d => d.PendingInvoiceItem).WithMany(p => p.PendingInvoiceItemsFlavors)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PendingInvoiceItemsFlavors_PendingInvoiceItems");
        });

        modelBuilder.Entity<Posdevice>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasOne(d => d.JccpluginType).WithMany(p => p.Posdevices).HasConstraintName("FK_POSDevices_JCCPluginTypes");
        });

        modelBuilder.Entity<PositionType>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<PromotionalOffer>(entity =>
        {
            entity.HasOne(d => d.RegulationsTemplate).WithMany(p => p.PromotionalOffers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PromotionalOffers_RegulationsTemplates");
        });

        modelBuilder.Entity<ServiceTypeOperator>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.ColorType).WithMany(p => p.ServiceTypeOperators)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceTypeOperators_ColorsTypes");

            entity.HasOne(d => d.ServingType).WithMany(p => p.ServiceTypeOperators)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceTypeOperators_ServingTypes");
        });

        modelBuilder.Entity<ServingType>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<ServingTypesToVat>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Timestamp).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ServingType).WithMany(p => p.ServingTypesToVats)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServingTypesToVAT_ServingTypes");

            entity.HasOne(d => d.Vat).WithMany(p => p.ServingTypesToVats)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServingTypesToVAT_VATs");
        });

        modelBuilder.Entity<ShopMap>(entity =>
        {
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<ShopsInfo>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<SyncQueue>(entity =>
        {
            entity.Property(e => e.ServerTimestamp).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status).HasDefaultValue("Pending");

            entity.HasOne(d => d.User).WithMany(p => p.SyncQueues)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SyncQueue_Users");
        });

        modelBuilder.Entity<SystemLog>(entity =>
        {
            entity.HasOne(d => d.LogType).WithMany(p => p.SystemLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SystemLogs_LogTypes");

            entity.HasOne(d => d.User).WithMany(p => p.SystemLogs).HasConstraintName("FK_SystemLogs_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.ColorTypeId).HasDefaultValue((byte)1);
            entity.Property(e => e.DataSourceTypeId).HasDefaultValue((byte)1);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RegistrationDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ColorType).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_ColorsTypes");

            entity.HasOne(d => d.DataSourceType).WithMany(p => p.Users).HasConstraintName("FK_Users_DataSourceTypes");

            entity.HasOne(d => d.PositionType).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_PositionTypes");
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasIndex(e => e.RefreshToken, "IX_UserSessions_RefreshToken").HasFilter("([IsActive]=(1))");

            entity.HasIndex(e => e.RefreshTokenExpiresAt, "IX_UserSessions_RefreshTokenExpiresAt").HasFilter("([IsActive]=(1))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastActivityAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.User).WithMany(p => p.UserSessions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserSessions_Users");
        });

        modelBuilder.Entity<Vat>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<VouchersRelease>(entity =>
        {
            entity.HasOne(d => d.Customer).WithMany(p => p.VouchersReleases).HasConstraintName("FK_VouchersReleases_Customers");

            entity.HasOne(d => d.Invoice).WithMany(p => p.VouchersReleases).HasConstraintName("FK_VouchersReleases_Invoices");

            entity.HasOne(d => d.PromotionalOffer).WithMany(p => p.VouchersReleases)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VouchersReleases_PromotionalOffers");
        });

        modelBuilder.Entity<VouchersUsageHistory>(entity =>
        {
            entity.HasOne(d => d.UsedByCustomer).WithMany(p => p.VouchersUsageHistories).HasConstraintName("FK_VouchersUsageHistory_Customers");

            entity.HasOne(d => d.UsedByInvoice).WithMany(p => p.VouchersUsageHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VouchersUsageHistory_Invoices");

            entity.HasOne(d => d.VouchersReleases).WithMany(p => p.VouchersUsageHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VouchersUsageHistory_VouchersReleases");
        });

        modelBuilder.Entity<ZreportType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ZReportType");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<ZreportsExport>(entity =>
        {
            entity.Property(e => e.Timesatmp).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ZreportType).WithMany(p => p.ZreportsExports)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ZReportsExports_ZReportTypes");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
