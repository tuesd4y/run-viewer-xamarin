#/bin/bash

# convert runtastic .gpx traces in current folder to geojson.
# if no argument passed, all traces are converted
# if one argument is passed, it should be the filename of one single file that should be converted 
# which also will be moved to the expected location (assumes $projdir is set to the RuntasticViewer project dir).

# assumes that the npx cli util is installed. If that is not the case, go get it 
# by running npm install -g npx (depends on npm, if you don't have that as well, we've got a problem)

if [ $# -eq 0 ]; then
    for f in *.gpx; do
      npx @tmcw/togeojson-cli "$f" > "$f".geojson
  done

elif [ $# -eq 1 ]; then
  npx @tmcw/togeojson-cli "$f" > defaultTrace.json
  cp defaultTrace "$projdir/RuntasticViewer/Assets/Resources/"
else 
  echo "Too many arguments passed"
fi

