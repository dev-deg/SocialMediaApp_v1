namespace SocialMediaApp_v1.Interfaces;

public interface IPubSubService
{
    Task PublishMessageAsync(string message);
}