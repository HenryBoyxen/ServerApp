//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class Benutzer
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Benutzer()
        {
            this.Benutzer1 = new HashSet<Benutzer>();
            this.Tage = new HashSet<Tage>();
            this.Tage1 = new HashSet<Tage>();
            this.Tage2 = new HashSet<Tage>();
            this.Tage3 = new HashSet<Tage>();
        }
    
        public int B_ID { get; set; }
        public string Vorname { get; set; }
        public string Nachname { get; set; }
        public string E_Mail { get; set; }
        public int Schul_ID { get; set; }
        public string Benutzername { get; set; }
        public string Passwort { get; set; }
        public Nullable<int> Sanitage_ID { get; set; }
        public Nullable<int> Springertage_ID { get; set; }
        public Nullable<int> SaniScore { get; set; }
        public Nullable<int> SpringerScore { get; set; }
        public Nullable<int> AnzahlDienste { get; set; }
        public Nullable<int> FavPartner { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Benutzer> Benutzer1 { get; set; }
        public virtual Benutzer Benutzer2 { get; set; }
        public virtual Sanitage Sanitage { get; set; }
        public virtual Schulen Schulen { get; set; }
        public virtual Springertage Springertage { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tage> Tage { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tage> Tage1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tage> Tage2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tage> Tage3 { get; set; }
    }
}
