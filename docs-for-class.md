# RuntasticViewer



ChristDisplay data about a running trace exported from adidas running.

https://github.com/tuesd4y/run-viewer-xamarin

## Functionality

<img src="img/screenshot.png" width="200"/>

(There also is a short demo of the app on yt: https://youtu.be/aemGH54XcCA)

This Xamarin.Forms cross-platform application can be used to visualize running logs, exported from the adidas Running (formerly known as _Runtastic_) application.
As seen in the screenshot above, the application displays a given running track and charts according to the logged height and current speed over the whole running session. All this data is parsed and calculated from the exported files from _adidas Running_.

By clicking parts of the trace, we choose what parts are used for calculating the charts above. Furthermore, by clicking the `fly to trace`-Button, the map will center back on the location of the trace if the screen has been moved beforehand.

Currently, the application does not support dynamically loading JSON-files at run-time, so if you want to display a running trace different to [the built-in one](https://github.com/tuesd4y/run-viewer-xamarin/blob/master/RuntasticViewer/Assets/Resources/defaultTrace.json), you can easily download and convert your own trace by following the instructions here.

## More Info

There is also a more detailed README in this folder (and in the github repo) where I describe how to add your own traces into the app and why using Xamarin for this small project was really annoying :) 



