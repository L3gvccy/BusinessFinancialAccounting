using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using BusinessFinancialAccounting.Models;

namespace BusinessFinancialAccounting.Tests
{
    public static class TestDb
    {
        public static AppDbContext CreateContext(string? dbName = null)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }
    }

    public static class SessionStringExtensions
    {
        public static void SetString(this ISession session, string key, string value) =>
            session.Set(key, Encoding.UTF8.GetBytes(value));

        public static string? GetString(this ISession session, string key) =>
            session.TryGetValue(key, out var val) ? Encoding.UTF8.GetString(val) : null;
    }

    public class DictSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();
        public IEnumerable<string> Keys => _store.Keys;
        public string Id { get; } = Guid.NewGuid().ToString();
        public bool IsAvailable => true;

        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
    }

    public class InMemoryTempDataProvider : ITempDataProvider
    {
        private const string Key = "__test_tempdata__";

        public IDictionary<string, object?> LoadTempData(HttpContext context)
        {
            if (context.Items.TryGetValue(Key, out var obj) && obj is IDictionary<string, object?> dict)
            {
                context.Items.Remove(Key);
                return new Dictionary<string, object?>(dict);
            }

            return new Dictionary<string, object?>();
        }

        public void SaveTempData(HttpContext context, IDictionary<string, object?> values)
        {
            context.Items[Key] = new Dictionary<string, object?>(values);
        }
    }

    public static class ControllerTestSetup
    {
        public static DefaultHttpContext HttpWithSession(IDictionary<string, string>? initial = null)
        {
            var http = new DefaultHttpContext { Session = new DictSession() };
            if (initial != null)
            {
                foreach (var kv in initial)
                    http.Session.SetString(kv.Key, kv.Value);
            }
            return http;
        }

        public static void WireTempData(Controller controller)
        {
            var provider = new InMemoryTempDataProvider();
            controller.TempData = new TempDataDictionary(controller.HttpContext, provider);
        }
    }

    public static class TestData
    {
        public static User NewUser(int i = 1) => new User
        {
            Username = $"user{i}",
            Password = "pass",
            FullName = $"User {i}",
            Email = $"user{i}@example.com",
            Phone = $"+380000000{i}",
        };
    }
}
