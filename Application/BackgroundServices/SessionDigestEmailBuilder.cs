using SessionTrackerApi.Domain.Entities;

namespace SessionTrackerApi.Application.BackgroundServices;

/// <summary>
/// Builds the styled HTML body for the nightly session digest email.
/// </summary>
public static class SessionDigestEmailBuilder
{
    public static string Build(DateTime date, List<Session> sessions)
    {
        var completed = sessions.Where(s => s.Status == "Completed").ToList();
        var pending   = sessions.Where(s => s.Status == "Pending").ToList();
        var canceled  = sessions.Where(s => s.Status == "Canceled").ToList();

        var totalEarnings = completed.Sum(s =>
            s.DurationInHours * (double)(s.HourlyRate == 0 ? 140 : s.HourlyRate));

        var rows = BuildSessionRows(sessions);
        var pendingBanner = BuildPendingBanner(pending.Count);

        return $"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="UTF-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
            </head>
            <body style="margin:0;padding:0;background:#0b0f1a;font-family:system-ui,-apple-system,sans-serif;">
              <div style="max-width:640px;margin:40px auto;padding:0 16px;">

                {BuildHeader(date)}
                {BuildStatsRow(totalEarnings, completed.Count, pending.Count, canceled.Count)}
                {pendingBanner}
                {BuildSessionTable(rows)}
                {BuildFooter()}

              </div>
            </body>
            </html>
            """;
    }

    // ── Sections ────────────────────────────────────────────────────────────

    private static string BuildHeader(DateTime date) => $"""
        <div style="background:linear-gradient(135deg,#1a2236,#111827);border:1px solid #1e293b;border-radius:16px 16px 0 0;padding:32px 32px 24px;">
          <div style="font-size:12px;font-weight:600;letter-spacing:1px;color:#6366f1;text-transform:uppercase;margin-bottom:8px;">Daily Digest</div>
          <h1 style="margin:0;font-size:24px;font-weight:700;color:#e2e8f0;">{date:MMMM dd, yyyy}</h1>
          <p style="margin:8px 0 0;font-size:14px;color:#64748b;">Here's a summary of your sessions from yesterday.</p>
        </div>
        """;

    private static string BuildStatsRow(double earnings, int completed, int pending, int canceled) => $"""
        <div style="background:#111827;border-left:1px solid #1e293b;border-right:1px solid #1e293b;display:table;width:100%;box-sizing:border-box;">
          <div style="display:table-row;">
            {StatCell($"${earnings:F2}", "Total Earned", "#10b981")}
            {StatCell(completed.ToString(), "Completed",   "#6366f1")}
            {StatCell(pending.ToString(),   "Pending",     "#f59e0b")}
            {StatCell(canceled.ToString(),  "Canceled",    "#ef4444")}
          </div>
        </div>
        """;

    private static string StatCell(string value, string label, string color) => $"""
        <div style="display:table-cell;padding:20px 24px;text-align:center;border-right:1px solid #1e293b;border-bottom:1px solid #1e293b;">
          <div style="font-size:28px;font-weight:700;color:{color};">{value}</div>
          <div style="font-size:11px;color:#64748b;margin-top:4px;text-transform:uppercase;letter-spacing:0.5px;">{label}</div>
        </div>
        """;

    private static string BuildPendingBanner(int count)
    {
        if (count == 0) return string.Empty;
        var plural = count != 1 ? "s" : "";
        return $"""
            <div style="background:#1a1200;border-left:1px solid #1e293b;border-right:1px solid #1e293b;padding:14px 24px;">
              <div style="background:#f59e0b22;border:1px solid #f59e0b44;border-radius:10px;padding:12px 16px;">
                <span style="color:#f59e0b;font-weight:600;font-size:13px;">⚠️  {count} session{plural} still marked as Pending</span>
                <span style="color:#94a3b8;font-size:12px;display:block;margin-top:2px;">Please mark them as Completed or Canceled in your dashboard.</span>
              </div>
            </div>
            """;
    }

    private static string BuildSessionTable(string rows) => $"""
        <div style="background:#111827;border:1px solid #1e293b;border-top:none;">
          <div style="padding:14px 16px 8px;background:#1a2236;border-bottom:1px solid #1e293b;">
            <span style="font-size:11px;font-weight:600;letter-spacing:0.5px;color:#64748b;text-transform:uppercase;">Session Breakdown</span>
          </div>
          <table style="width:100%;border-collapse:collapse;">
            <thead>
              <tr style="background:#0f172a;">
                {TableHeader("Title")} {TableHeader("Time")} {TableHeader("Duration")} {TableHeader("Status")} {TableHeader("Earnings")}
              </tr>
            </thead>
            <tbody>{rows}</tbody>
          </table>
        </div>
        """;

    private static string TableHeader(string label) =>
        $"<th style='padding:10px 16px;text-align:left;font-size:11px;font-weight:600;color:#475569;text-transform:uppercase;letter-spacing:0.5px;'>{label}</th>";

    private static string BuildSessionRows(List<Session> sessions)
    {
        if (sessions.Count == 0)
            return "<tr><td colspan='5' style='padding:32px;text-align:center;color:#64748b;'>No sessions recorded for this day.</td></tr>";

        var sb = new System.Text.StringBuilder();
        foreach (var s in sessions)
        {
            var statusColor  = s.Status switch { "Completed" => "#10b981", "Canceled" => "#ef4444", _ => "#f59e0b" };
            var rate         = (double)(s.HourlyRate == 0 ? 140 : s.HourlyRate);
            var earningsText = s.Status == "Completed" ? $"${s.DurationInHours * rate:F2}" : "—";
            var title        = System.Net.WebUtility.HtmlEncode(s.Title ?? "Untitled");
            var reason       = s.Status == "Canceled" && !string.IsNullOrWhiteSpace(s.CancelReason)
                ? $"<div style='font-size:12px;color:#94a3b8;margin-top:2px;'>{System.Net.WebUtility.HtmlEncode(s.CancelReason)}</div>"
                : "";

            sb.Append($"""
                <tr>
                  <td style="padding:12px 16px;border-bottom:1px solid #1e293b;"><span style="font-weight:600;color:#e2e8f0;">{title}</span>{reason}</td>
                  <td style="padding:12px 16px;border-bottom:1px solid #1e293b;color:#94a3b8;font-size:13px;">{s.StartTime:HH:mm}</td>
                  <td style="padding:12px 16px;border-bottom:1px solid #1e293b;color:#cbd5e1;">{s.DurationInHours:F1} h</td>
                  <td style="padding:12px 16px;border-bottom:1px solid #1e293b;"><span style="background:{statusColor}22;color:{statusColor};padding:3px 10px;border-radius:20px;font-size:12px;font-weight:600;">{s.Status}</span></td>
                  <td style="padding:12px 16px;border-bottom:1px solid #1e293b;font-weight:700;color:#10b981;">{earningsText}</td>
                </tr>
                """);
        }
        return sb.ToString();
    }

    private static string BuildFooter() => """
        <div style="background:#0b0f1a;border:1px solid #1e293b;border-top:none;border-radius:0 0 16px 16px;padding:20px 32px;text-align:center;">
          <p style="margin:0;font-size:12px;color:#334155;">Sessions Tracker · Automated nightly digest</p>
        </div>
        """;
}
