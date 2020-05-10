# RuntasticViewer

Display data about a running trace exported from adidas running.

## Functionality

[![run-viewer demo](img/screenshot.png)](https://youtu.be/aemGH54XcCA "run-viewer demo")

(Click the image to watch a short demo on YouTube, if that doesn't work, here's the video link: https://youtu.be/aemGH54XcCA)

This Xamarin.Forms cross-platform application can be used to visualize running logs, exported from the adidas Running (formerly known as _Runtastic_) application.
As seen in the screenshot above, the application displays a given running track and charts according to the logged height and current speed over the whole running session. All this data is parsed and calculated from the exported files from _adidas Running_.

By clicking parts of the trace, we choose what parts are used for calculating the charts above. Furthermore, by clicking the `fly to trace`-Button, the map will center back on the location of the trace if the screen has been moved beforehand.

Currently, the application does not support dynamically loading JSON-files at run-time, so if you want to display a running trace different to [the built-in one](https://github.com/tuesd4y/run-viewer-xamarin/blob/master/RuntasticViewer/Assets/Resources/defaultTrace.json), you can easily download and convert your own trace by following the instructions here.

## Displaying your own traces

- head over to [runtastic.com](https://www.runtastic.com), log in to your account and click on settings in your the dropdown menu near your user icon.
- Click on `Export Data` and wait until the export is finished and you get a mail from adidas running with a download link to the export. 
- Follow that link, extract the downloaded data and open the resulting file in a terminal window.
- Since we are only concerned about the running data (and mostly GPS logs) we `cd` into `Sport-Sessions/GPS-data`
- In this folder, we can see a lot of GPS tracks in the form of `.gpx`-files. Pick one of them which you want to display. Let's call that one route.gpx
- We now need to convert that .gpx file to a geojson (`.json`) file. For one simple file, we can just copy and paste the `.gpx` file content into [an online converter](https://mapbox.github.io/togeojson/) and copy the results into a file called `defaultTrace.json`
- If you're already interested in how the trace looks, you can paste the files content into [this awesome geojson online viewer](http://geojson.io). An example trace can look like this ![geojson.io example](img/geojsonio.png)
- Last but not least, move the `defaultTrace.json` file to the `RuntasticViewer/Assets/Resources/` folder in your project directory.

* Build the project in Visual Studio or Rider and deploy it to the device of your choice!

There also is an automized version of all steps after downloading and extracting the data (except the build and deploy-step) in the file [convert.sh](https://github.com/tuesd4y/run-viewer-xamarin/blob/master/convert.sh).

This script has only been tested on macOs yet.

## Why using xamarin was a bad idea

- Xamarin.Forms makes it easy to develop cross-platform mobile (and also desktop) applications with C#. 
- Technically, layouts and all code can be defined once and then gets compiled to native code for all platforms. 
- It is also possible to include libraries, that consist of different counterparts for each platform. At least in theory, and that's where the fun part ends. 
- Let's take a look at charting libraries for instance. Event though there exist multiple different options and implementations, most of them come with a hefty price tag. A lot of packages (where similar open-source options exist for instance in the iOS and android ecosystem) come with costs north of 500$ for a library. 
- Another problem is that often the native implementation of libraries differ greatly between multiple platforms (xamarin.forms.Maps uses Google Maps on Android and Apple Maps on iOS), therefore common interfaces of only those two platforms often are only just the minimal basic stuff, since most in-depth changes would require special handling on each platform (e.g. [3d buildings on a map are easily possibly with the GoogleMaps Android SDK](https://developers.google.com/maps/documentation/android-sdk/views#3d_buildings_on_the_map) but are not supported by xamarin.forms.Maps)

## Download and Building the project yourself

This app was only really tested on iOS, and you can download the binary from [this repository's release section](https://github.com/tuesd4y/run-viewer-xamarin/releases/tag/v1.0). 

If installing that package onto a real device doesn't work, try checking out the project and building it with Visual Studio or Rider.

