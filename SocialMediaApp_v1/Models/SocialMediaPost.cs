using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;

namespace SocialMediaApp_v1.Models;

[FirestoreData]
public class SocialMediaPost
{
    [Required]
    [FirestoreProperty]
    public string PostId { get; set; }
    
    [FirestoreProperty]
    public string PostTile { get; set; }
    
    [FirestoreProperty]
    public string PostCaption { get; set; }
    
    [FirestoreProperty]
    public string Author { get; set; }
    
    [FirestoreProperty]
    public string Date { get; set; }
    
    //TODO: Add an image/video
}