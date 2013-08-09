VegaHub
=======

A SignalR Hub Utility for data scientists to push Vega charts from F# Interactive

Building VegaHub
----------------

VegaHub relies on several packages in the ASP.NET Web Stack nightly builds. You will need to add a reference to the MyGet feed in order to successfully build and run the script.

Add this to your NuGet package sources by going to Tools -> Library Package Manager -> Package Manager Settings and creating a new package source for http://www.myget.org/F/aspnetwebstacknightly
