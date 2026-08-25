namespace SessionTrackerApi.Domain.Entities;

public class Session
{
    public int Id { get; set; }
    public string? GoogleEventId { get; set; }
    public string? Title { get; set; }
    public DateTime StartTime { get; set; }
    public double DurationInHours { get; set; }
    public decimal HourlyRate { get; set; } = 140; 
    public string Status { get; set; } = "Pending";
    public string? CancelReason { get; set; }
}