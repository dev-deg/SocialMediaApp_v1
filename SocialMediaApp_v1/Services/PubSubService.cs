// Services/PubSubService.cs
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using SocialMediaApp_v1.Interfaces;

namespace SocialMediaApp_v1.Services;

public class PubSubService : IPubSubService
{
    private readonly TopicName _topicName;

    public PubSubService(IConfiguration configuration)
    {
        var projectId = configuration["Authentication:Google:ProjectId"];
        _topicName = TopicName.FromProjectTopic(projectId, "test-topic");
    }

    public async Task PublishMessageAsync(string message)
    {
        var publisher = await PublisherClient.CreateAsync(_topicName);
        
        try
        {
            var pubsubMessage = new PubsubMessage
            {
                Data = ByteString.CopyFromUtf8(message)
            };
            
            await publisher.PublishAsync(pubsubMessage);
        }
        finally
        {
            await publisher.ShutdownAsync(TimeSpan.FromSeconds(5));
        }
    }
}