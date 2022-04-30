﻿using Microsoft.EntityFrameworkCore;
using WebApplication1;

namespace WebApi.Helpers
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        protected DataContext()
        {
        }

        public DbSet<SchedueledTask> SchedueledTasks { get; set; }
    }
}