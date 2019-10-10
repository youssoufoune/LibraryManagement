using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryData.Models
{
    public class Patron
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public String LastName { get; set; }
        public string Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string TelephoneNumber { get; set; }
        public virtual LibraryCard LibraryCard { get; set; } // Virtyal keyword when y ou want to lazy load that property's data. 
                                                             // Lasy loading loads a collection from the database the first time it is accesed
        public virtual LibraryBranch HomeLibraryBranch { get; set; }
    }
}
