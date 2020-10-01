# Intro amazing website
WebBackgrounder is a proof-of-concept of a web-farm friendly background task 
manager meant to just work with a vanilla ASP.NET web application.

# Open Source Code of Conduct
This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

# Problem Statement
Code that runs within ASP.NET does not own the App Domain. IIS and ASP.NET are 
free to shut down the app domain at any moment.

Typically, when that happens, ASP.NET flushes the requests giving them time to 
finish what they're doing before tearing down the App Domain.

But if you're doing work on background thread that ASP.NET doesn't know about, 
it could end up tearing down the app domain in the middle of the work, leaving 
your data in a potentially invalid state.

There are ways to notify ASP.NET that work is in progress. WebBackgrounder 
demonstrates the use of such facilities.

Likewise, if you have multiple web servers in a farm, you'd like some small 
amount of coordination between them without them having to explicitly know 
about each other so they don't duplicate each other's work.

# Why not use Azure Queues or a Scheduled Task?
For an enterprise solution, using an Azure Queue, a Windows Service, or a 
system level scheduled task are much better solutions. The goal of this project 
would be to allow those to easily hook in and provide this functionality.

But for a small project, or for development environments, you still want these 
tasks to run without requiring a bunch of setup or a connection to Azure.

# What this is not
This is not a general purpose scheduling framework. There are much better ones 
out there such as hangfire.io, FluentScheduler and Quartz.net. The goal of this project is 
to handle one task only, manage a recurring task on an interval in the 
background for a web app.

The needs I have are very simple. I didn't need a high fidelity scheduler. 
Maybe later, I'll look to integrate what I've done with one of the others. 
But for now, this scratches an itch.
