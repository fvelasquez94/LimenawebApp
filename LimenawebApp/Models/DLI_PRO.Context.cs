﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LimenawebApp.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DLI_PROEntities : DbContext
    {
        public DLI_PROEntities()
            : base("name=DLI_PROEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<OCRD> OCRD { get; set; }
        public virtual DbSet<BI_Dim_Customer> BI_Dim_Customer { get; set; }
        public virtual DbSet<UFD1> UFD1 { get; set; }
        public virtual DbSet<BI_Dim_Products> BI_Dim_Products { get; set; }
        public virtual DbSet<ITM1> ITM1 { get; set; }
        public virtual DbSet<Dsd_Products> Dsd_Products { get; set; }
        public virtual DbSet<View_dsd_suggestedinventory> View_dsd_suggestedinventory { get; set; }
        public virtual DbSet<OpenSalesOrders> OpenSalesOrders { get; set; }
        public virtual DbSet<C_DROUTE> C_DROUTE { get; set; }
        public virtual DbSet<C_HELPERS> C_HELPERS { get; set; }
        public virtual DbSet<C_TRUCKS> C_TRUCKS { get; set; }
        public virtual DbSet<C_DRIVERS> C_DRIVERS { get; set; }
        public virtual DbSet<OpenSalesOrders_Details> OpenSalesOrders_Details { get; set; }
    }
}
