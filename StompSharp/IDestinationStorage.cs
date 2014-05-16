namespace StompSharp
{

    public interface IDestinationStorage
    {
        IDestination Get(string destination);

    }
}