using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PenggunaAPI.Models
{
    public class Pengguna
    {
        [Key]
        public int PenggunaId { get; set; } 
        public string Email { get; set; }   
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Created { get; set; }
        public bool isLocked { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Saldo> Saldo { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
    }
}