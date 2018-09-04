# AlarmsIOTSimulator
A .NET Core console app to publish simple alarm data to an Azure Event Grid topic. 

The alarm data consists of:

- device id
- status (green | amber | red)
- longtitude
- latitude
- image (a URL to an image related to the alarm)

Resultant JSON Schema:

```JSON
{
    "properties": {
        "deviceId": {
            "type": "number"
        },
        "image": {
            "type": "string"
        },
        "latitude": {
            "type": "number"
        },
        "longitude": {
            "type": "number"
        },
        "status": {
            "type": "string"
        }
    },
    "type": "object"
}
```

## Pre-reqs

You will need an [Azure Event Grid topic](https://docs.microsoft.com/en-us/azure/event-grid/custom-event-quickstart-portal#create-a-custom-topic).

## Usage

On the command line run:

`dotnet run EventTopicURL EventResourcePath EventKey FalseImageURL TrueImageURL EventInterval NumberDevices MaxLat MinLat MaxLong MinLong StatusWeighting`

where:

Required:
- EventTopicURL: the endpoint for the Event Grid Topic and can be copied from the Overview blade.
- EventResourcePath: the path to the resource and is of the form: /subscriptions/(your subscription id)/resourceGroups/(your resource group name)/providers/Microsoft.EventGrid/topics/(your EventGrid topic name).
- EventKey: the key for the Event Grid Topic
- FalseImageURL: the URL to an image that can be used for a false positive event (i.e. no cause for concern).
- TrueImageURL: the URL to an image that can be used for a true positive event (i.e. cause for concern).

Optional (must be added in this order):

- EventInterval: the number of milliseconds to pause between each event being published. Must be greater than 0. Default = 5000.
- NumberDevices: the number of simulated devices to create. Each will have a random but fixed location. Default = 20.
- MaxLat, MinLat, MaxLong, MinLong: Define the boundaries of a geographic rectangle within which the device locations will be set. All 4 are needed if used, and each must be a decimal with 6 significant places e.g. 53.024562. Default = a large rectangle covering most of England.
- StatusWeighting: Must be more than 3, the higher the proportionally more green status alerts. Default = 10.

You can also build a Docker image using the included Dockerfile such as: 

`docker build --rm -f Dockerfile -t alarmsiotsimulator:latest .`

The container can then be run with a similar command line to above:

`docker run alarmsiotsimulator EventTopicURL EventResourcePath EventKey FalseImageURL TrueImageURL EventInterval NumberDevices MaxLat MinLat MaxLong MinLong StatusWeighting`

Alternatively the [image is available on DockerHub](https://hub.docker.com/r/gdavi/alarms-iot-simulator/) to use immediately, in which case the command line will be:

`docker run gdavi/alarms-iot-simulator EventTopicURL EventResourcePath EventKey FalseImageURL TrueImageURL EventInterval NumberDevices MaxLat MinLat MaxLong MinLong StatusWeighting`