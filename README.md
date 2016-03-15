# RiotWadExtractor

RiotWadExtractor allows you to extract all files from a WAD archive file used by the new Riot Client.
It automatically recognizes the most common file formats (e.g. files with a PNG header will have the ".png" file extension)

## Usage
Drag and drop a WAD file on `RiotWadExtractor.exe` or use the following command line 
```batch
RiotWadExtractor "C:/file.wad"
```
Another way would be to right click the WAD file, selecting `Open With` and selecting the RiotWadExtractor executable.

The extractor creates a new directory with the same name and in the same directory as the WAD file, containing all files.

## Issues
The file names are gibberish since WAD files do not contain the original file names.

## TODO
- Add an option to define an output directory
- Add the ability to repack/modify WAD files
- Find out the hash algorithm used for the file name hashes
- Maybe a GUI?

## Note
*This WAD file format is not related to the WAD file format used by older id Games games like Quake 1.*
