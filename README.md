AppStarter
==========

An application loader which manages those annoying Windows services, so that they don't have to be running all the time.

Many of today's software, especially developed by outsourcing teams, have very various quality. The abuse of Windows Service is just one of the most common problems.

One of the biggest advantage of service is that it runs even when no user has login. But some of application developers abuse or at least misuse it as a way to preload resources for their application, which could have been developed into their main program. Of course loading the resources when the main program starts might slow down the main program, but with some optimization, this delay can be siginicantly reduced. It is not fair to use user's computer resource to hold the resources in the memory just in case they need to use the program.

I hope more and more developers can do some reasonable optimization work to their app, rather than just shouting today's memory and computation power are so cheap. 

The reasons for this problem to be more common with outsourceing teams are complex, but in my opinion the following 2 points are the important reasons.

1. The quality of developers in the teams are not very stable.
2. Too high reuse rate of codes, libraries, and especially high level package. This is important for outsourcing teams because this is a very key method for their cost control. But the architecture designed for one company's project might be not that compatible with another project.

The application is not completely ready yet, and some of the important features are not finished.

At the moment, only the funcationalities works fine.
1. Start the services required for the main program.
2. Start the program and monitor its lifespan.
3. Stop the services and restore their services after the exit of the main program.
4. Auto exit of the monitor after the exit of last program.

Some of the not-implemented features which are going to be added in near future
1. Wizard to help user to create config XML and shortcut.
2. Some notification about progress, because some services might take more than 10 seconds to start or stop.
3. Downgrade the version of .Net, to make the program available to more old computers which suffer more from the loaded unnecessary services because of their limited memory.
