Windows Console App to bulk upload in Flickr.

Features:
This will add images from a specific folder.
Support for multiple levels of folders - photosets are created based on foldernames like folder1_sub1_subsub
If the img is uploaded or added to set, the status is stored in a local sqlite db file. 
So you can stop the upload in middle and start again where you left.

Setup:
Before running this program:

Update app.config

1) flickr API key and secret.

2) folder name: if your photos are at c:\folder1\Photos
set foldertoscan : Photos and flickrfolder : c:\folder1

3) isPrivate flag

First time running

1) app will open browser to authenticate this app to access your flickr account. 

2) once authorized, this token will be saved in app.config. 
this is a one time only process. 

Everytime

Displays list of images to upload, and settings. 

You have to press y, to start upload. 

photoset created for the below list.

eg.,
c:\photos\flickrsyncfolder\folder1\img1.jpg

c:\photos\flickrsyncfolder\folder1\img2.jpg

c:\photos\flickrsyncfolder\folder1\img3.jpg

c:\photos\flickrsyncfolder\folder1\sub\img1.jpg

c:\photos\flickrsyncfolder\folder1\sub\img2.jpg

c:\photos\flickrsyncfolder\folder1\sub\img3.jpg

photosets created will be:

folder1

folder1_sub


Libraries Used :
http://flickrnet.codeplex.com/ ,
http://logging.apache.org/log4net/,
http://www.sqlite.org
