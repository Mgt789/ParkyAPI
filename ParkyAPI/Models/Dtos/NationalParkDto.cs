using System;
using System.ComponentModel.DataAnnotations;

namespace ParkyAPI.Models.Dtos
{
    public class NationalParkDto
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string State { get; set; }
        public DateTime Created { get; set; }
        public DateTime Established { get; set; }
    }
}
