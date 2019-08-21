using Microsoft.EntityFrameworkCore;
using System;

namespace ModerationData {
    public class ModerationContext : DbContext {

        public ModerationContext(DbContextOptions options) : base(options) { }



    }
}
