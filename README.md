StompSharp
==========

C# (.net) Stomp Client.

Master | Provider
------ | --------
[![Build Status][MonoImgMaster]][MonoLinkMaster] | Mono CI Provided by [travis-ci][] 

[TeamCityImgMaster]:http://teamcity.codebetter.com/app/rest/builds/buildType:\(id:bt1191\)/statusIcon
[TeamCityLinkMaster]:http://teamcity.codebetter.com/viewLog.html?buildTypeId=bt1191&buildId=lastFinished&guest=1

[MonoImgMaster]:https://travis-ci.org/shanielh/StompSharp.png?branch=master
[MonoLinkMaster]:https://travis-ci.org/shanielh/StompSharp
[AppVeyorLinkMaster]:https://ci.appveyor.com/project/uhttpsharp
[AppVeyorImgMaster]:https://ci.appveyor.com/api/projects/status?id=1schhjbpx7oomrx7

[travis-ci]:https://travis-ci.org/
[AppVeyor]:http://www.appveyor.com/
[JetBrains]:http://www.jetbrains.com/
[CodeBetter]:http://codebetter.com/

## Usage

Registering to messages : 

    using (IStompClient client = new StompClient("localhost", 61613))
    {
        var autoAck = client.SubscriptionBehaviors.AutoAcknowledge;
        using (IDestination<IMessage> destination = client.GetDestination("/queue/a",autoAck))
        {
            using (destination.IncommingMessages.Subscribe(Console.WriteLine))
            {
                Console.WriteLine("Messages are written to console. Press any key to unsubscribe and exit.");
                Console.ReadKey();
            }        
        }
    }
    
Please note that you can change a subscription behavior from auto, client, client-individual.

Sending messages : 

    using (IStompClient client = new StompClient("localhost", 61613))
    {
        var autoAck = client.SubscriptionBehaviors.AutoAcknowledge;
        using (IDestination<IMessage> destination = client.GetDestination("/queue/a",autoAck))
        {
            var messageToSend = new BodyOutgoingMessage(new byte[1024]);
            await destination.SendAsync(messageToSend, NoReceiptBehavior.Default);
        }
    }
    
The task that returned from SendAsync will complete when the message is sent to the server, You can switch to `ReceiptBehavior` and then the task will be completed when a receipt is received from the server.
