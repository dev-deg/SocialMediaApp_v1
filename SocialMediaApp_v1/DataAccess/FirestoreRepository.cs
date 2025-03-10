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
        
        public async Task<SocialMediaPost> GetPostById(string postId)
        {
            // Query for the specific post by ID
            Query postQuery = _db.Collection("posts").WhereEqualTo("PostId", postId);
            QuerySnapshot postSnapshot = await postQuery.GetSnapshotAsync();
    
            if (postSnapshot.Count > 0)
            {
                SocialMediaPost post = postSnapshot.Documents[0].ConvertTo<SocialMediaPost>();
                _logger.LogInformation($"Post {postId} retrieved from Firestore.");
                return post;
            }
    
            _logger.LogWarning($"Post {postId} not found.");
            return null;
        }
        
        public async Task DeletePost(string postId)
        {
            // Find the post document by ID
            Query postQuery = _db.Collection("posts").WhereEqualTo("PostId", postId);
            QuerySnapshot postSnapshot = await postQuery.GetSnapshotAsync();
    
            if (postSnapshot.Count > 0)
            {
                // Delete the document from Firestore
                await postSnapshot.Documents[0].Reference.DeleteAsync();
                _logger.LogInformation($"Post {postId} deleted from Firestore.");
            }
            else
            {
                _logger.LogWarning($"Post {postId} not found for deletion.");
            }
        }
    }
}
