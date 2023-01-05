using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AS_Vranicich.AS_Models
{
    public class Session
    {
        public int SessionId { get; }
        public DateTime ValidUntil { get; }

        public User Owner { get; }
        public int UserId { get; }
    }
}
