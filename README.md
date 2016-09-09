MyVote
------
MyVote is an app developed by [Magenic](http://www.magenic.com) as a comprehensive demo for the [Modern Apps Live!](http://www.modernappslive.com) conference held twice a year as part of [Live! 360 Dev](http://www.live360events.com).

The code in this repo has been scrubbed to remove sensitive key values for encryption and services. The following is a list of changes you'll need to make to insert your own keys into the codebase:

src/Services/MyVote.Services.AppServer.Core/appsettins.json

add in database connection string

src/Services/MyVote.Services.AppServer.Core/Startup.cs

add in oauth provider id and secret

src/Services/MyVote.Services.AppServer.Core/Constants.cs

add in secret key

src/UI/MyVote.UI.Web/Web.config

add in firebase api and domain key
