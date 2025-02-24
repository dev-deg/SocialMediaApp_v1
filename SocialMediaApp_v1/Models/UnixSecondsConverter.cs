using Google.Cloud.Firestore;

namespace SocialMediaApp_v1.Models;

public class UnixSecondsConverter: IFirestoreConverter<DateTimeOffset>
{
    public object ToFirestore(DateTimeOffset value)
    {
        return value.ToUnixTimeSeconds();
    }

    public DateTimeOffset FromFirestore(object value)
    {
        if (value is not long seconds) throw new ArgumentException("Value is not long");
        return DateTimeOffset.FromUnixTimeSeconds(seconds);
    }
}