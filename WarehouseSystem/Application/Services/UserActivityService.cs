using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using WarehouseSystem.Services.Interfaces;
using Presentation.DTOs.Users;

namespace WarehouseSystem.Services
{
    public partial class UserService
    {
        private readonly string _activityFilePath;
        private readonly string _sessionFilePath;
        private static readonly ConcurrentDictionary<string, UserSessionDto> _activeSessions = new();
        
        private async Task InitializeActivityFiles()
        {
            _activityFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "user_activities.csv");
            _sessionFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "user_sessions.csv");
            
            if (!File.Exists(_activityFilePath))
            {
                await File.WriteAllTextAsync(_activityFilePath, UserActivityDto.GetCsvHeader());
            }
            if (!File.Exists(_sessionFilePath))
            {
                await File.WriteAllTextAsync(_sessionFilePath, UserSessionDto.GetCsvHeader());
            }
        }

        public async Task<UserActivityDto> LogUserActivityAsync(UserActivityDto activityDto)
        {
            try
            {
                // Sugeneruojame unikalų ID
                activityDto.ActivityId = await GetNextActivityIdAsync();
                
                // Įrašome į CSV
                var lines = await ReadAllLinesAsync(_activityFilePath);
                lines.Add(activityDto.ToCsvLine());
                await WriteAllLinesAsync(_activityFilePath, lines);

                // Atnaujiname sesijos aktyvumą
                if (!string.IsNullOrEmpty(activityDto.SessionId))
                {
                    await UpdateSessionActivityAsync(activityDto.SessionId);
                }

                return activityDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida registruojant vartotojo {Username} veiklą", activityDto.Username);
                throw;
            }
        }

        public async Task<UserSessionDto> StartUserSessionAsync(string username, string ipAddress, string userAgent)
        {
            try
            {
                var sessionDto = new UserSessionDto
                {
                    SessionId = Guid.NewGuid().ToString(),
                    Username = username,
                    StartTime = DateTime.Now,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    LoginStatus = "Success",
                    LastActivityTime = DateTime.Now,
                    IsActive = true
                };

                // Įrašome į CSV
                var lines = await ReadAllLinesAsync(_sessionFilePath);
                lines.Add(sessionDto.ToCsvLine());
                await WriteAllLinesAsync(_sessionFilePath, lines);

                // Pridedame į aktyvias sesijas
                _activeSessions.TryAdd(sessionDto.SessionId, sessionDto);

                // Registruojame prisijungimo veiklą
                await LogUserActivityAsync(new UserActivityDto
                {
                    Username = username,
                    ActivityTime = DateTime.Now,
                    ActivityType = "Login",
                    Description = "User logged in",
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    SessionId = sessionDto.SessionId,
                    ActionResult = "Success"
                });

                return sessionDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida pradedant vartotojo {Username} sesiją", username);
                throw;
            }
        }

        public async Task<bool> EndUserSessionAsync(string sessionId)
        {
            try
            {
                // Randame sesiją
                if (!_activeSessions.TryRemove(sessionId, out var session))
                {
                    return false;
                }

                session.EndTime = DateTime.Now;
                session.IsActive = false;

                // Atnaujiname CSV
                var lines = await ReadAllLinesAsync(_sessionFilePath);
                var sessionLines = lines.ToList();
                var index = sessionLines.FindIndex(l => l.StartsWith($"\"{sessionId}\""));

                if (index != -1)
                {
                    sessionLines[index] = session.ToCsvLine();
                    await WriteAllLinesAsync(_sessionFilePath, sessionLines);
                }

                // Registruojame atsijungimo veiklą
                await LogUserActivityAsync(new UserActivityDto
                {
                    Username = session.Username,
                    ActivityTime = DateTime.Now,
                    ActivityType = "Logout",
                    Description = "User logged out",
                    SessionId = sessionId,
                    ActionResult = "Success"
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida baigiant sesiją {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<bool> UpdateSessionActivityAsync(string sessionId)
        {
            if (_activeSessions.TryGetValue(sessionId, out var session))
            {
                session.LastActivityTime = DateTime.Now;
                
                // Atnaujiname CSV
                var lines = await ReadAllLinesAsync(_sessionFilePath);
                var sessionLines = lines.ToList();
                var index = sessionLines.FindIndex(l => l.StartsWith($"\"{sessionId}\""));

                if (index != -1)
                {
                    sessionLines[index] = session.ToCsvLine();
                    await WriteAllLinesAsync(_sessionFilePath, sessionLines);
                    return true;
                }
            }
            return false;
        }

        public async Task<UserSessionDto> GetActiveSessionAsync(string username)
        {
            return _activeSessions.Values.FirstOrDefault(s => s.Username == username && s.IsActive);
        }

        public async Task<UserActivityStatsDto> GetUserActivityStatsAsync(
            string username,
            DateTime startDate,
            DateTime endDate)
        {
            try
            {
                var activities = await GetUserActivitiesAsync(username, startDate, endDate);
                var sessions = await GetUserSessionsAsync(username, startDate, endDate);

                var stats = new UserActivityStatsDto
                {
                    Username = username,
                    PeriodStart = startDate,
                    PeriodEnd = endDate,
                    TotalLogins = activities.Count(a => a.ActivityType == "Login"),
                    TotalActions = activities.Count(),
                    SuccessfulActions = activities.Count(a => a.ActionResult == "Success"),
                    FailedActions = activities.Count(a => a.ActionResult == "Failure"),
                    ActionsByModule = activities.GroupBy(a => a.ModuleName)
                                              .ToDictionary(g => g.Key, g => g.Count()),
                    MostUsedFeatures = activities.GroupBy(a => a.ActionName)
                                               .OrderByDescending(g => g.Count())
                                               .Take(5)
                                               .Select(g => g.Key)
                                               .ToList(),
                    LastLogin = activities.Where(a => a.ActivityType == "Login")
                                        .OrderByDescending(a => a.ActivityTime)
                                        .FirstOrDefault()?.ActivityTime,
                    LastActivity = activities.OrderByDescending(a => a.ActivityTime)
                                          .FirstOrDefault()?.ActivityTime
                };

                // Apskaičiuojame aktyvumo laiką
                TimeSpan totalActiveTime = TimeSpan.Zero;
                foreach (var session in sessions)
                {
                    var endTime = session.EndTime ?? DateTime.Now;
                    totalActiveTime += endTime - session.StartTime;
                }
                stats.TotalActiveTime = totalActiveTime;

                if (sessions.Any())
                {
                    stats.AverageSessionDuration = TimeSpan.FromTicks(
                        (long)sessions.Average(s => 
                            (s.EndTime ?? DateTime.Now).Ticks - s.StartTime.Ticks)
                    );
                }

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant vartotojo {Username} aktyvumo statistiką", username);
                throw;
            }
        }

        public async Task<IEnumerable<UserActivityDto>> GetUserActivitiesAsync(
            string username,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var lines = await ReadAllLinesAsync(_activityFilePath);
            return lines.Skip(1)
                       .Select(ParseActivityLine)
                       .Where(a => a != null &&
                                 a.Username == username &&
                                 (!startDate.HasValue || a.ActivityTime >= startDate) &&
                                 (!endDate.HasValue || a.ActivityTime <= endDate))
                       .OrderByDescending(a => a.ActivityTime);
        }

        public async Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(
            string username,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var lines = await ReadAllLinesAsync(_sessionFilePath);
            return lines.Skip(1)
                       .Select(ParseSessionLine)
                       .Where(s => s != null &&
                                 s.Username == username &&
                                 (!startDate.HasValue || s.StartTime >= startDate) &&
                                 (!endDate.HasValue || s.StartTime <= endDate))
                       .OrderByDescending(s => s.StartTime);
        }

        private async Task<int> GetNextActivityIdAsync()
        {
            var lines = await ReadAllLinesAsync(_activityFilePath);
            return lines.Skip(1)
                       .Select(l => int.Parse(l.Split(',')[0]))
                       .DefaultIfEmpty(0)
                       .Max() + 1;
        }

        private UserActivityDto ParseActivityLine(string line)
        {
            try
            {
                var parts = line.Split(',');
                return new UserActivityDto
                {
                    ActivityId = int.Parse(parts[0]),
                    Username = parts[1],
                    ActivityTime = DateTime.Parse(parts[2]),
                    ActivityType = parts[3].Trim('"'),
                    Description = parts[4].Trim('"'),
                    IpAddress = parts[5].Trim('"'),
                    UserAgent = parts[6].Trim('"'),
                    ModuleName = parts[7].Trim('"'),
                    ActionName = parts[8].Trim('"'),
                    ActionResult = parts[9].Trim('"'),
                    ErrorDetails = parts[10].Trim('"'),
                    RelatedEntityId = parts[11].Trim('"'),
                    SessionId = parts[12].Trim('"')
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida apdorojant veiklos CSV eilutę: {Line}", line);
                return null;
            }
        }

        private UserSessionDto ParseSessionLine(string line)
        {
            try
            {
                var parts = line.Split(',');
                return new UserSessionDto
                {
                    SessionId = parts[0].Trim('"'),
                    Username = parts[1],
                    StartTime = DateTime.Parse(parts[2]),
                    EndTime = string.IsNullOrEmpty(parts[3]) ? null : DateTime.Parse(parts[3]),
                    IpAddress = parts[4].Trim('"'),
                    UserAgent = parts[5].Trim('"'),
                    LoginStatus = parts[6].Trim('"'),
                    LastActivityTime = string.IsNullOrEmpty(parts[7]) ? null : DateTime.Parse(parts[7]),
                    IsActive = bool.Parse(parts[8])
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida apdorojant sesijos CSV eilutę: {Line}", line);
                return null;
            }
        }

        // Valymo metodai
        private async Task CleanupInactiveSessions()
        {
            try
            {
                var inactivityTimeout = TimeSpan.FromHours(1); // Konfigūruojama reikšmė
                var now = DateTime.Now;

                // Valome atmintį
                var inactiveSessions = _activeSessions
                    .Where(kvp => now - (kvp.Value.LastActivityTime ?? kvp.Value.StartTime) > inactivityTimeout)
                    .ToList();

                foreach (var session in inactiveSessions)
                {
                    await EndUserSessionAsync(session.Key);
                }

                // Valome CSV
                var lines = await ReadAllLinesAsync(_sessionFilePath);
                var sessionLines = lines.ToList();
                var header = sessionLines.First();
                var updatedSessions = sessionLines.Skip(1)
                    .Select(ParseSessionLine)
                    .Where(s => s != null && 
                              (s.EndTime.HasValue || 
                               now - (s.LastActivityTime ?? s.StartTime) <= inactivityTimeout))
                    .Select(s => s.ToCsvLine());

                var newLines = new List<string> { header };
                newLines.AddRange(updatedSessions);
                await WriteAllLinesAsync(_sessionFilePath, newLines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida valant neaktyvias sesijas");
            }
        }

        // Periodinis senų duomenų archyvavimas
        private async Task ArchiveOldActivityData()
        {
            try
            {
                var archiveDate = DateTime.Now.AddMonths(-3); // Konfigūruojama reikšmė
                var activityArchivePath = _activityFilePath.Replace(".csv", $"_archive_{DateTime.Now:yyyyMMdd}.csv");
                var sessionArchivePath = _sessionFilePath.Replace(".csv", $"_archive_{DateTime.Now:yyyyMMdd}.csv");

                // Archyvuojame veiklas
                var activityLines = await ReadAllLinesAsync(_activityFilePath);
                var activityHeader = activityLines.First();
                var oldActivities = activityLines.Skip(1)
                    .Where(l => DateTime.Parse(l.Split(',')[2]) < archiveDate);
                var currentActivities = activityLines.Skip(1)
                    .Where(l => DateTime.Parse(l.Split(',')[2]) >= archiveDate);

                if (oldActivities.Any())
                {
                    var archiveLines = new List<string> { activityHeader };
                    archiveLines.AddRange(oldActivities);
                    await File.WriteAllLinesAsync(activityArchivePath, archiveLines);

                    var newLines = new List<string> { activityHeader };
                    newLines.AddRange(currentActivities);
                    await WriteAllLinesAsync(_activityFilePath, newLines);
                }

                // Archyvuojame sesijas
                var sessionLines = await ReadAllLinesAsync(_sessionFilePath);
                var sessionHeader = sessionLines.First();
                var oldSessions = sessionLines.Skip(1)
                    .Where(l => DateTime.Parse(l.Split(',')[2]) < archiveDate);
                var currentSessions = sessionLines.Skip(1)
                    .Where(l => DateTime.Parse(l.Split(',')[2]) >= archiveDate);

                if (oldSessions.Any())
                {
                    var archiveLines = new List<string> { sessionHeader };
                    archiveLines.AddRange(oldSessions);
                    await File.WriteAllLinesAsync(sessionArchivePath, archiveLines);

                    var newLines = new List<string> { sessionHeader };
                    newLines.AddRange(currentSessions);
                    await WriteAllLinesAsync(_sessionFilePath, newLines);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida archyvuojant senus aktyvumo duomenis");
            }
        }
    }
}
