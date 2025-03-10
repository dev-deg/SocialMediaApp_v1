using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;
using Newtonsoft.Json.Converters;

namespace SocialMediaApp_v1.Models;

[FirestoreData]
public class SocialMediaPost
{
    [Required]
    [FirestoreProperty]
    public string PostId { get; set; }
    
    [FirestoreProperty]
    public string PostContent { get; set; }
    
    [FirestoreProperty]
    public string PostAuthor { get; set; }
    
    [FirestoreProperty(ConverterType = typeof(UnixSecondsConverter))]
    public DateTimeOffset PostDate { get; set; }
    
    [FirestoreProperty]
    public string ImageUrl { get; set; }
    
    public IFormFile Image {get; set;}
}