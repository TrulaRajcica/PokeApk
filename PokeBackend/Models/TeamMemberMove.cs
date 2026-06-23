using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokeBackend.Models
{
    public class TeamMemberMove
    {
        public int Id { get; set; }
        public int TeamMemberId { get; set; }
        public TeamMember? TeamMember { get; set; }
        public int MoveId { get; set; }
        public Move? Move { get; set; }
    }
}