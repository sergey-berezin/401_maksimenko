using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DataManagerSpace
{
    public class QandA
    {
        public class TabText
        {
            public int Id { get; set; }
            public string Text { get; set; }
        }

        public class AnswerHistory
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }
            public int TabTextId { get; set; }
            public string QueryText { get; set; }
            public string AnswerText { get; set; }

        }

    }
}
