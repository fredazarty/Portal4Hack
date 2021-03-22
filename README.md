# Portal4Hack
 Portal4Hack


This the code use for NFTHack by

There are two modules in this project:

The first module is A UNITY project where we implement the VR experience and develop a custom code  to :
Take the snapshot when press letter "P"
Send the snapshot  to IPFS  using "nfs.storage" service
Send generated IPFS adresse + sha256 hash and communicate with the brower
The unity project has been exporting in WebGL and integrated into module 2 : a Vue.js application

In the Vue.js application, we use nfs.storage to store metadata and the ZDK of ZORA to mint the nft.
We have a MetaMask integration that allows any user to mint his NFT base on the snapshot easily.

That was a perfect experience to understand how everything can be linked to create new experiences and new leverage for VR Artist.
