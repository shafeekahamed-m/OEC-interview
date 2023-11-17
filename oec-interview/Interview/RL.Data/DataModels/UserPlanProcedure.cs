using RL.Data.DataModels.Common;
using System.ComponentModel.DataAnnotations;

namespace RL.Data.DataModels;

public class UserPlanProcedure : IChangeTrackable
{
    [Key]
    public int PlanProcedureId { get; set; }
    [Key]
    public int UserId { get; set; }
    public bool IsDelete { get; set; }
    public virtual PlanProcedure PlanProcedure { get; set; }
    public virtual User User { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
}
