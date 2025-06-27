using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using TravelWeb.Controllers;
using TravelWeb.Models;

namespace TravelWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Tour> Tours { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Guides> Guides { get; set; }
        public DbSet<TourAssignment> TourAssignment { get; set; }
        public DbSet<GroupBooking> GroupBookings { get; set; }
        public DbSet<TourDeposit> TourDeposits { get; set; }

    }

}