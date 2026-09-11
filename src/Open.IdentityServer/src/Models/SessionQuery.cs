namespace Open.IdentityServer.Models;

public class SessionQuery
{
    public string ResultsToken { get; set; }

    public bool RequestPriorResults { get; set; }

    public int CountRequested { get; set; }

    public string SubjectId { get; init; }

    public string SessionId { get; init; }

    public string DisplayName { get; init; }
}