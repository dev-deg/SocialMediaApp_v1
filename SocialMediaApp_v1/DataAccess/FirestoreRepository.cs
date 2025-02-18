using Google.Cloud.Firestore;
using SocialMediaApp_v1.Models;

namespace SocialMediaApp_v1.DataAccess
{
    public class FirestoreRepository
    {
        private readonly ILogger<FirestoreRepository> _logger;
        private FirestoreDb _db;

        public FirestoreRepository(IConfiguration configuration, ILogger<FirestoreRepository> logger)
        {
            _logger = logger;
            _db = FirestoreDb.Create(configuration.GetValue<String>("Authentication:Google:ProjectId"));
        }

        public async void AddPost(SocialMediaPost post)
        {
            await _db.Collection("posts").AddAsync(post);
            _logger.LogInformation($"Post {post.PostId} added to Firestore.");
        }
    }
}
