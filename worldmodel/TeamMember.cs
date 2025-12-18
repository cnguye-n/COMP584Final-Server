using System.ComponentModel.DataAnnotations;

namespace worldmodel
{
    public class TeamMember
    {
        public int TeamMemberId { get; set; }

        // FK -> Team
        public int TeamId { get; set; }
        public Team? Team { get; set; }

        // FK -> AspNetUsers (Identity)
        [Required]
        public string UserId { get; set; } = "";
        public WorldModelUser? User { get; set; }

        // "Owner" / "Admin" / "Member"
        [Required]
        public string RoleInTeam { get; set; } = "Member";
    }
}
