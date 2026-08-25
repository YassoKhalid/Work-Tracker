namespace SessionTrackerApi.Domain.Entities;

public class UserGoogleToken{
    public int Id { get; set; }
    public int UserId{get;set;}
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? AccessTokenExpiry { get; set; }
    public DateTime? CreatedAt { get; set; }


}