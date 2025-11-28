using CommunityToolkit.Mvvm.Messaging.Messages;

public class QRDetectedMessage : ValueChangedMessage<string>
{
    public QRDetectedMessage(string value) : base(value)
    {
    }
}
