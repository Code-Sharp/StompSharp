namespace Stomp2
{
    public interface IDestinationStorage
    {
        IDestination Get(string destination);

    }
}