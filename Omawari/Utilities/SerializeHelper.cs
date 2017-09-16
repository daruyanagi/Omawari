using Newtonsoft.Json;
using System.IO;

public static class SerializeHelper
{
    public static string Serialize(this object source)
    {
        return JsonConvert.SerializeObject(source, Formatting.Indented);
    }

    public static T Deserialize<T>(this string source)
    {
        return JsonConvert.DeserializeObject<T>(source);
    }
}
