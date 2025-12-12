using Online_Language_School.Models;

public class RefundRequest
{
    public int Id { get; set; }
    public int EnrollmentId { get; set; }
    public string ReasonText { get; set; }
    public bool? IsApproved { get; set; }
    public string? RejectReason { get; set; }

    public Enrollment Enrollment { get; set; }
}
