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
    
    public partial class dbLimenaEntities : DbContext
    {
        public dbLimenaEntities()
            : base("name=dbLimenaEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Sys_Companies> Sys_Companies { get; set; }
        public virtual DbSet<Sys_Departments> Sys_Departments { get; set; }
        public virtual DbSet<Sys_Roles> Sys_Roles { get; set; }
        public virtual DbSet<Sys_Users> Sys_Users { get; set; }
        public virtual DbSet<Tb_Resources> Tb_Resources { get; set; }
        public virtual DbSet<Tb_Alerts> Tb_Alerts { get; set; }
        public virtual DbSet<Sys_LogCon> Sys_LogCon { get; set; }
        public virtual DbSet<Tb_OrdersDetailsDSD> Tb_OrdersDetailsDSD { get; set; }
        public virtual DbSet<Tb_OrdersDSD> Tb_OrdersDSD { get; set; }
        public virtual DbSet<Tb_PreOrdersDetailsDSD> Tb_PreOrdersDetailsDSD { get; set; }
        public virtual DbSet<Tb_PreOrdersDSD> Tb_PreOrdersDSD { get; set; }
        public virtual DbSet<Tb_InventoryDetailsTRDSD> Tb_InventoryDetailsTRDSD { get; set; }
        public virtual DbSet<Tb_InventoryTRDSD> Tb_InventoryTRDSD { get; set; }
    }
}
