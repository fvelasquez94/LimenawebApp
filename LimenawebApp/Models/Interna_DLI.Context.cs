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
    
    public partial class Interna_DLIEntities : DbContext
    {
        public Interna_DLIEntities()
            : base("name=Interna_DLIEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Tb_MaestroPorcentajes> Tb_MaestroPorcentajes { get; set; }
        public virtual DbSet<Tb_ConfigBolsa> Tb_ConfigBolsa { get; set; }
        public virtual DbSet<Tb_Autorizaciones> Tb_Autorizaciones { get; set; }
        public virtual DbSet<Tb_Bonificaciones> Tb_Bonificaciones { get; set; }
        public virtual DbSet<Tb_customerscontacts> Tb_customerscontacts { get; set; }
        public virtual DbSet<Tb_customerstatus> Tb_customerstatus { get; set; }
        public virtual DbSet<Tb_registroBolsa> Tb_registroBolsa { get; set; }
    }
}
