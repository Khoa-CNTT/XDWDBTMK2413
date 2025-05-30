//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RestaurantTableSystem.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Table
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Table()
        {
            this.Bookings = new HashSet<Booking>();
        }
    
        public int table_id { get; set; }
        public Nullable<int> restaurant_id { get; set; }
        public string table_number { get; set; }
        public int capacity { get; set; }
        public string type { get; set; }
        public Nullable<bool> is_available { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual Restaurant Restaurant { get; set; }
    }
}
