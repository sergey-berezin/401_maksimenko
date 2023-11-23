using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.EntityFrameworkCore;
using static DataManagerSpace.QandA;
using System.Windows.Input;
using WpfAnswerBert;

namespace DataManagerSpace
{
    public class DataManager : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<TabText> TabTexts { get; set; }
        public DbSet<AnswerHistory> AnswerHistories { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source=\"D:\\Березин1\\Task3\\WpfAnswerBert\\WpfAnswerBert\\database.db\"");
        }
    }

}

