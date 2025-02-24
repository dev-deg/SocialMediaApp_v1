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
            _db = FirestoreDb.Create(configuration.GetValue<string>("Authentication:Google:ProjectId"));
        }

        public async Task AddPost(SocialMediaPost post)
        {
            await _db.Collection("posts").AddAsync(post);
            _logger.LogInformation($"Post {post.PostId} added to Firestore.");
        }

        public async Task<List<SocialMediaPost>> GetPosts()
        {
            List<SocialMediaPost> posts = new List<SocialMediaPost>();
            Query allPostsQuery = _db.Collection("posts");
            QuerySnapshot postsSnapshot = await allPostsQuery.GetSnapshotAsync();
            foreach (DocumentSnapshot postSnapshot in postsSnapshot)
            {
                SocialMediaPost post = postSnapshot.ConvertTo<SocialMediaPost>();
                posts.Add(post);
            }
            _logger.LogInformation($"All {posts.Count} posts retrieved from Firestore.");
            return posts;
        }
    }
}
