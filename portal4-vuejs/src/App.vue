<template>
  <div id="app">

    <b-modal v-model="modalShow1" ref="modal-1" id="modal-1" title="PORTAL4" ok-only>
      <p class="my-4">Take a snap with  <b>'P'</b> and <b>'ESC'</b> to exit the experience</p>
    </b-modal>


    <b-modal v-model="modalShow2" ref="modal-2" id="modal-2" title="PORTAL4" ok-only>
      <p class="my-4">NFT creation for "{{ userID }} is in progress... please wait </p>
    </b-modal>

    <b-modal v-model="modalShow3" ref="modal-2" id="modal-2" title="PORTAL4" ok-only>
      <p class="my-4">Your snap will be available soon here : <a target="_blank" :href="snapURINFT">{{snapURINFT}}</a></p>
    </b-modal>


  </div>
</template>

<script>

import {NFTStorage, Blob} from 'nft.storage'

import Web3 from "web3";
import {
  constructBidShares,
  constructMediaData,
  generateMetadata,
  isMediaDataVerified, isURIHashVerified,
  sha256FromBuffer,
  Zora
} from "@zoralabs/zdk";

export default {
  name: 'App',
  data() {
    return {
      userID: "",
      modalShow1: true,
      modalShow2: false,
      modalShow3: false,
      snapURINFT: ""
    }
  },
  mounted() {
    const v = this;
    window.modal2 = function (user) {
      v.userID = user;
      v.modalShow2 = true;
    }
    window.modal3= function (link) {
      v.snapURINFT = link;
      v.modalShow2 = false;
      v.modalShow3 = true;
    }
  },
  created() {

    if (window.ethereum) {
      window.web3 = new Web3(window.ethereum);
      window.ethereum.enable();
      window.ethereum.on("accountsChanged", function () {
        location.reload();
      });
      // this.$refs['modal-1'].show()

      // const metadataJSONZ = generateMetadata('zora-20210101', {
      //   description: '',
      //   mimeType: 'text/plain',
      //   name: '',
      //   version: 'zora-20210101',é
      // })

      // const metadataHashZ = sha256FromBuffer(Buffer.from(metadataJSONZ))
      //
      // console.log('metadataHashZ => '+metadataHashZ )
      //
      // let Z = "https://ipfs.io/ipfs/bafybeifpxcq2hhbzuy2ich3duh7cjk4zk4czjl6ufbpmxep247ugwzsny4";
      // isURIHashVerified(Z,metadataHashZ,10000).then(verified => {
      //   if (verified) {
      //     console.log(Z + ' => ' + metadataHashZ + ' => OK ');
      //   } else {
      //     console.log(Z + ' => ' + metadataHashZ + ' => NOOOOOOOOO ');
      //   }
      // });


      const context = new AudioContext();

      context.resume().then(() => {
        console.log('Playback resumed successfully');
      });
      // eslint-disable-next-line no-undef
      window.createUnityInstance(document.querySelector("#unity-canvas"), {
        dataUrl: "Build/Portal4.data",
        frameworkUrl: "Build/Portal4.framework.js",
        codeUrl: "Build/Portal4.wasm",
        streamingAssetsUrl: "StreamingAssets",
        companyName: "DefaultCompany",
        productName: "Portal4",
        productVersion: "0.1",
      });
    }
    window.mintLog = function (name, snapCid, snapHash, apiKey) {
      console.log("name: " + name)
      console.log("snapCid: " + snapCid)
      console.log("snapHash: " + snapHash)
      console.log("apiKey: " + apiKey)
    }

    window.snapshotAlert = function () {
      const userID = window.web3.currentProvider.selectedAddress;
      // alert("NFT creation for " +  + " is in progress...\n ")
      window.modal2(userID);

    }
    window.mint = function (name, snapCid, snapHashBase, apiKey) {

      const snapHash = "0x" + snapHashBase.toLowerCase();
      let jsonMeta = {
        description: 'Portal4 ' + name,
        mimeType: 'image/png',
        name: name + '.png',
        version: 'zora-20210101',
      };

      const metadataJSON = generateMetadata('zora-20210101', jsonMeta)
      const metadataHash = sha256FromBuffer(Buffer.from(metadataJSON))

      const contentMeta = new Blob([metadataJSON])


      let snapURI = 'https://' + snapCid + '.ipfs.dweb.link';
      //alert("Snap will be available soon here : " + snapURI)
      window.modal3(snapURI)
      console.log("snapURI : " + snapURI)
      console.log("snapHash : " + snapHash)
      const client = new NFTStorage({token: apiKey})

      client.storeBlob(contentMeta).then(metaCid => {

            let metadataURI = 'https://' + metaCid + '.ipfs.dweb.link';

            console.log("metadataJSON : " + metadataJSON)
            console.log("metaCid : " + metaCid)
            console.log("metadataURI : " + metadataURI)
            console.log("metadataHash : " + metadataHash)


            const mediaData = constructMediaData(
                snapURI,
                metadataURI,
                snapHash,
                metadataHash
            )


            window.web3.eth.getChainId().then(chainId => {

              console.log("chainId " + chainId)

              isURIHashVerified(metadataURI, metadataHash, 10000).then(verified => {
                if (verified) {
                  console.log(metadataURI + ' => ' + metadataHash + ' => OK ');
                } else {
                  console.log(metadataURI + ' => ' + metadataHash + ' => NOOOOOOOOO ');
                }


                isURIHashVerified(snapURI, snapHash, 10000).then(verified => {
                  if (verified) {
                    console.log(snapURI + ' => ' + snapHash + ' => OK ');
                  } else {
                    console.log(snapURI + ' => ' + snapHash + ' => NOOOOOOOOO ');
                  }

                  isMediaDataVerified(mediaData, 10000).then(verified => {

                    if (!verified) {
                      console.log("OOOOOOoooops NOT Verified!")

                      throw new Error("MediaData not valid, do not mint")
                    } else {
                      console.log("Verified!")

                      const bidShares = constructBidShares(
                          10, // creator share
                          90, // owner share
                          0 // prevOwner share
                      )


                      var zora = new Zora(window.web3.provider, chainId)
                      console.log("Zora onbaord!!")

                      zora.mint(mediaData, bidShares).then(tx => {
                        console.log("wait!")

                        tx.wait(8)

                        console.log("Done!")

                      });

                    }
                  });

                });
              })
            });

          }
      );
    }
  },
  methods: {
  }


}
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}
</style>
