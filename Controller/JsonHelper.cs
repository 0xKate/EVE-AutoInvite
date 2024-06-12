using EVEAutoInvite;
using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal static class JsonHelper
{
    // Base64 encoding methods
    public static string Base64UrlEncode(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        return Base64UrlEncode(bytes);
    }

    public static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
                      .Replace('+', '-')
                      .Replace('/', '_')
                      .TrimEnd('='); // Trim padding '=' characters
    }

    // Base64 decoding method
    public static byte[] Base64UrlDecode(string input)
    {
        // Replace URL-safe characters with standard base64 characters
        string base64 = input.Replace('-', '+').Replace('_', '/');

        // Add padding characters if necessary
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        // Convert from base64 to byte array
        return Convert.FromBase64String(base64);
    }

    // Serialization method
    public static byte[] Serialize<T>(T obj)
    {
        using (var ms = new MemoryStream())
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            serializer.WriteObject(ms, obj);
            return ms.ToArray();
        }
    }

    public static void SerializeToFile<T>(T obj, string filePath)
    {
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            serializer.WriteObject(fileStream, obj);
        }
    }

    // Deserialization methods
    public static T Deserialize<T>(string json)
    {
        var jsonBytes = Encoding.UTF8.GetBytes(json);
        return Deserialize<T>(jsonBytes);
    }

    public static T Deserialize<T>(byte[] jsonBytes)
    {
        using (var ms = new MemoryStream(jsonBytes))
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            return (T)serializer.ReadObject(ms);
        }
    }

    public static async Task<T> Deserialize<T>(HttpContent content, CancellationToken cancellationToken = default)
    {
        using (var stream = await content.ReadAsStreamAsync().WithCancellation(cancellationToken))
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            return (T)serializer.ReadObject(stream);
        }
    }

    public static T Deserialize<T>(Stream stream)
    {
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
        return (T)serializer.ReadObject(stream);
    }

    public static T DeserializeFromFile<T>(string filePath)
    {
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            return (T)serializer.ReadObject(fileStream);
        }
    }
}
