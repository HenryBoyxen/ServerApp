//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ExampleGUI.SQL
{
    using System;
    using System.Collections.Generic;
    
    public partial class Tage
    {
        public int Tag_ID { get; set; }
        public System.DateTime Datum { get; set; }
        public Nullable<int> Sani1 { get; set; }
        public Nullable<int> Sani2 { get; set; }
        public Nullable<int> Springer1 { get; set; }
        public Nullable<int> Springer2 { get; set; }
    
        public virtual Benutzer Benutzer { get; set; }
        public virtual Benutzer Benutzer1 { get; set; }
        public virtual Benutzer Benutzer2 { get; set; }
        public virtual Benutzer Benutzer3 { get; set; }
    }
}
