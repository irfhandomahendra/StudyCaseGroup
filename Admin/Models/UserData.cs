﻿namespace Admin.Models
{
    public class UserData
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public bool isLocked { get; set; }
    }
}
