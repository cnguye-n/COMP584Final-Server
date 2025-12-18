using System.ComponentModel.DataAnnotations;

namespace worldmodel
{
    public class Team
    {
        public int TeamId { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
    }
}
