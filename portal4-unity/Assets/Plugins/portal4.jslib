mergeInto(LibraryManager.library, {
  
    CreateNFT: function (name_,cid_,snapHash_,apiKey_) {
    
        var name = Pointer_stringify(name_);
        var cid = Pointer_stringify(cid_);
        var snapHash = Pointer_stringify(snapHash_);
        var apiKey = Pointer_stringify(apiKey_);
        
        console.log(name);
        console.log(cid);
        console.log(snapHash);
        console.log(apiKey);
        
        window.mint(name,cid,snapHash,apiKey);
        },
        
    SnapshotAlert: function(){
       window.snapshotAlert();
    }
});