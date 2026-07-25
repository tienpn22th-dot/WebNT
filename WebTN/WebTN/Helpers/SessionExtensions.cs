using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace WebTN.Helpers
{
    public static class SessionExtensions
    {
        // Lưu đối tượng (Object/List) vào Session dưới dạng chuỗi JSON
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // Đọc đối tượng từ Session và ép kiểu ngược lại
        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}