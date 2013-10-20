# Cloudy Azure Images #1
This repo contains the projects I created for my [blog](http://robinosborne.co.uk/?p=1498) [posts](http://robinosborne.co.uk/?p=1506) about serving auto generated resized images from Azure, Azure Blob Storage, and Azure Service bus.

A web role to resize images on demand and pop them on a serivce bus queue.

A worker role to upload the resized images to blob storage.

Another web role; this one to act as the proxy to check blob storage for a previously resized version of the requested image.
