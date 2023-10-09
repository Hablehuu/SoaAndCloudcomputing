using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyRestAPI.Models
{
    public class Developer
    {
        [Required]
        public string Name { get; set; }
        [Key]
        public int DeveloperID { get; set; }
        public int Established { get; set; }
        public bool PrivateOwned { get; set; }
        

    }
}
