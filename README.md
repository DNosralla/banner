BannerFlow test implementation using .net core api and MongoDb.

### Prerequisites
The solution requires a running MongoDB at localhost:27017, if necessary change it in the appsettings.json file at the root of the API project.

## Running
After starting the API project a Swagger interface will be shown where it can be tested.
When submiting Html in the swagger interface first escape the html text using for example: https://www.freeformatter.com/json-escape.html

## Tests
There are XUnit tests for the Banner Controller and the Html validator.

## Implementation Details
* For better compatibility with MongoDB ObjectId the Banner Id was changed from int to a string.
* Caching was implemented for the consumption of the banners html using a MemoryCache.

