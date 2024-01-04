using SABLANCE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SABLANCE.ViewModel
{
    public class CustomerModel
    {
        public decimal ID { get; set; }
        public string Code { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public int MemberID { get; set; }
        public string AccountType { get; set; }
        public string Password { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}