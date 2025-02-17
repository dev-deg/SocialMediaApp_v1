using Google.Cloud.Firestore;

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
    }
}
