using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace NoiThatCaoCap.Extensions
{
    public static class SessionExtensions
    {
        // Hàm lưu Object vào Session
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // Hàm lấy Object từ Session ra sử dụng
        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}