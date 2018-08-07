# AlarmsIOTSimulator
A .NET Core console app to publish simple alarm data to an Azure Event Grid topic. 

The alarm data consists of:

- status (green | amber | red)
- longtitude
- latitude
- image (a URL to an image related to the alarm)

## Pre-reqs

You will need an [Azure Event Grid topic](https://docs.microsoft.com/en-us/azure/event-grid/custom-event-quickstart-portal#create-a-custom-topic)

## Usage

On the command line run:

dotnet run <EventTopicURL> <EventResourcePath> <EventKey> <FalseImageURL> <TrueImageURL> <EventInterval (int in ms)>

where:

- EventTopicURL is the endpoint for the Event Grid Topic and can be copied from the Overview blade.
- EventResourcePath is the path to the resource and is of the form /subscriptions/(your subscription id)/resourceGroups/(your resource group name)/providers/Microsoft.EventGrid/topics/(your EventGrid topic name).
- EventKey is the key for your Event Grid Topic
- FalseImageURL is the URL to an image that can be used for a false positive event (i.e. no cause for concern).
- TrueImageURL is the URL to an image that can be used for a true positive event (i.e. cause for concern).
- EventInterval is the number of milliseconds to to pause between each Event being published. Must be greater than 0.



