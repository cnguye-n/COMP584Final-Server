namespace COMP584Server.Data.DTO
{
    public class TeamMemberDto
    {
        public int TeamMemberId { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; } = "";
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string RoleInTeam { get; set; } = "";
    }
}
