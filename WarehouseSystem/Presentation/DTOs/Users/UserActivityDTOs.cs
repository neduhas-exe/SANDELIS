namespace Presentation.DTOs.Users
{
    /// <summary>
    /// DTO vartotojo aktyvumo įrašui
    /// </summary>
    public class UserActivityDto
    {
        public int ActivityId { get; set; }
        public string Username { get; set; }
        public DateTime ActivityTime { get; set; }
        public string ActivityType { get; set; }  // Login, Logout, Action, SystemEvent
        public string Description { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string ModuleName { get; set; }     // Sistemos modulis (Products, Warehouse, etc.)
        public string ActionName { get; set; }     // Konkretus veiksmas
        public string ActionResult { get; set; }   // Success, Failure, Error
        public string ErrorDetails { get; set; }   // Jei įvyko klaida
        public string RelatedEntityId { get; set; }  // Susijusios esybės ID (pvz., produkto ID)
        public string SessionId { get; set; }

        public string ToCsvLine()
        {
            return $"{ActivityId}," +
                   $"{Username}," +
                   $"{ActivityTime:yyyy-MM-dd HH:mm:ss}," +
                   $"\"{ActivityType}\"," +
                   $"\"{Description}\"," +
                   $"\"{IpAddress}\"," +
                   $"\"{UserAgent}\"," +
                   $"\"{ModuleName}\"," +
                   $"\"{ActionName}\"," +
                   $"\"{ActionResult}\"," +
                   $"\"{ErrorDetails}\"," +
                   $"\"{RelatedEntityId}\"," +
                   $"\"{SessionId}\"";
        }

        public static string GetCsvHeader()
        {
            return "ActivityId,Username,ActivityTime,ActivityType,Description,IpAddress,UserAgent," +
                   "ModuleName,ActionName,ActionResult,ErrorDetails,RelatedEntityId,SessionId";
        }
    }

    /// <summary>
    /// DTO vartotojo sesijos informacijai
    /// </summary>
    public class UserSessionDto
    {
        public string SessionId { get; set; }
        public string Username { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string LoginStatus { get; set; }  // Success, Failed, Expired, Terminated
        public DateTime? LastActivityTime { get; set; }
        public bool IsActive { get; set; }

        public string ToCsvLine()
        {
            return $"\"{SessionId}\"," +
                   $"{Username}," +
                   $"{StartTime:yyyy-MM-dd HH:mm:ss}," +
                   $"{(EndTime.HasValue ? EndTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "")}," +
                   $"\"{IpAddress}\"," +
                   $"\"{UserAgent}\"," +
                   $"\"{LoginStatus}\"," +
                   $"{(LastActivityTime.HasValue ? LastActivityTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "")}," +
                   $"{IsActive}";
        }

        public static string GetCsvHeader()
        {
            return "SessionId,Username,StartTime,EndTime,IpAddress,UserAgent,LoginStatus,LastActivityTime,IsActive";
        }
    }

    /// <summary>
    /// DTO vartotojo aktyvumo statistikai
    /// </summary>
    public class UserActivityStatsDto
    {
        public string Username { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalLogins { get; set; }
        public int TotalActions { get; set; }
        public int SuccessfulActions { get; set; }
        public int FailedActions { get; set; }
        public TimeSpan TotalActiveTime { get; set; }
        public TimeSpan AverageSessionDuration { get; set; }
        public Dictionary<string, int> ActionsByModule { get; set; } = new();
        public List<string> MostUsedFeatures { get; set; } = new();
        public DateTime? LastLogin { get; set; }
        public DateTime? LastActivity { get; set; }
    }
}
