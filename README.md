# NetworkMonitorAlerter

This app will monitor the bandwidth that each of your processes use on your system. It will monitor upload and download and send you a notification it it surpasses an x amount of bandwidth for down and upload.

For instance you can set it to trigger on 15MB per 5 minutes (default settings) and it will show you a notification that a process hit that 'limit'.

After that you can choose to whitelist the app for an hour, a day or infinite or not whitelist it at all (which will trigger notifications again).

I made this to monitor the bandwidth usage of my processes that upload or download stuff without me knowing. If you 'close' it it keeps running in the systemtray.

It requires administrative privileges because it uses TraceEventSession with the NetworkTCPIP kernel provider.

![NetworkMonitorAlerter screenshot](https://github.com/b03tz/NetworkMonitorAlerter/blob/master/assets/screenshot.png)
![NetworkMonitorAlerter screenshot 2](https://github.com/b03tz/NetworkMonitorAlerter/blob/master/assets/screenshot2.png)
