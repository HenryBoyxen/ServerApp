﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Der Code wurde von einer Vorlage generiert.
//
//     Manuelle Änderungen an dieser Datei führen möglicherweise zu unerwartetem Verhalten der Anwendung.
//     Manuelle Änderungen an dieser Datei werden überschrieben, wenn der Code neu generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ExampleGUI.SQL
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class OrgaSANItionEntities : DbContext
    {
        public OrgaSANItionEntities()
            : base("name=OrgaSANItionEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Benutzer> Benutzer { get; set; }
        public virtual DbSet<Sanitage> Sanitage { get; set; }
        public virtual DbSet<Schulen> Schulen { get; set; }
        public virtual DbSet<Springertage> Springertage { get; set; }
        public virtual DbSet<Tage> Tage { get; set; }
    }
}
