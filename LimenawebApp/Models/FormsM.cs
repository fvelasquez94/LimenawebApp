//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class FormsM
    {
        public int ID_form { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int ID_activity { get; set; }
        public bool active { get; set; }
        public string query1 { get; set; }
        public string query2 { get; set; }
        public int ID_empresa { get; set; }
    }
}
