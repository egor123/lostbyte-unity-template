mergeInto(LibraryManager.library, {
  SyncFiles: function () {
    if (window.syncFSTimeout !== undefined) {
      clearTimeout(window.syncFSTimeout);
    }

    window.syncFSTimeout = setTimeout(function () {
      window.isSyncingFS = true;
      window.syncFSTimeout = undefined;

      FS.syncfs(false, function (err) {
        window.isSyncingFS = false;
        if (err) {
          console.error("WebGL FS Sync Error: " + err);
        } else {
          console.log("WebGL FS Sync Complete.");
        }
      });
    }, 500);

    if (!window.fsBeforeUnloadAdded) {
      window.fsBeforeUnloadAdded = true;
      window.addEventListener("beforeunload", function (e) {
        if (window.syncFSTimeout !== undefined || window.isSyncingFS) {
          e.preventDefault();
          e.returnValue = "";
        }
      });
    }
  },
});
