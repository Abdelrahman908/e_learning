using System.Collections.Concurrent;

namespace e_learning.Services
{
    public class RateLimiterService
    {
        private readonly ConcurrentDictionary<string, (int Attempts, DateTime LastAttempt)> _attempts = new();

        private readonly int _maxAttempts = 5;
        private readonly TimeSpan _blockDuration = TimeSpan.FromMinutes(10);

        public bool IsBlocked(string key)
        {
            if (_attempts.TryGetValue(key, out var info))
            {
                if (info.Attempts >= _maxAttempts && DateTime.Now - info.LastAttempt < _blockDuration)
                    return true;
            }
            return false;
        }

        public void RegisterAttempt(string key)
        {
            _attempts.AddOrUpdate(key,
                _ => (1, DateTime.Now),
                (_, info) =>
                {
                    if (DateTime.Now - info.LastAttempt > TimeSpan.FromMinutes(1))
                        return (1, DateTime.Now); // Reset بعد دقيقة
                    return (info.Attempts + 1, DateTime.Now);
                });
        }
    }
}
