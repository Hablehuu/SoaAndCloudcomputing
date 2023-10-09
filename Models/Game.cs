using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MyRestAPI.Models
{
    public class Game
    {
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        public string? Genres { get; set; }
        public int ReleaseYear { get; set; }
        [ForeignKey("DeveloperID")]
        public int DeveloperID { get; set; }
    }
}
