function saveAsJpg(doc, file) {
  var jpgSaveOptions = new JPEGSaveOptions();
  jpgSaveOptions.quality = 9;
  doc.saveAs(file, jpgSaveOptions, true, Extension.LOWERCASE);
  $.writeln("Saved to jpg: " + file);
}

function saveAsWideJpg(doc) {
  var docName = doc.name.replace(".psd", "-wide-splash.jpg");
  var jpgFolder = ensurePngFolder(doc.path);

  var file = new File(jpgFolder + "/" + docName);
  saveAsJpg(doc, file);
  $.writeln("Saved as wide jpg to: " + file);

  file = new File(jpgFolder + "/" + doc.name.replace(".psd", "-wide-bg.jpg"));
  saveBackgroundAsJpg(doc, file);
  $.writeln("Saved as wide bg jpg to: " + file);
}

function saveAsSquareJpg(doc) {
  var docName = doc.name.replace("-s.psd", "-square-splash.jpg");
  var jpgFolder = ensurePngFolder(doc.path);
  var file = new File(jpgFolder + "/" + docName);
  saveAsJpg(doc, file);
  $.writeln("Saved as square jpg to: " + file);

  // save background only
  file = new File(
    jpgFolder + "/" + doc.name.replace("-s.psd", "-square-bg.jpg")
  );
  saveBackgroundAsJpg(doc, file);
  $.writeln("Saved as square bg jpg to: " + file);
}

function saveBackgroundAsJpg(doc, file) {
  var textLayers = [];
  var layers = doc.layers;
  for (var i = 0; i < layers.length; i++) {
    if (layers[i].kind === LayerKind.TEXT) {
      textLayers.push(layers[i]);
      layers[i].visible = false;
    }
  }

  resizeDocument(doc, 0.5);
  saveAsJpg(doc, file);

  resizeDocument(doc, 2);

  for (var i = 0; i < textLayers.length; i++) {
    textLayers[i].visible = true;
  }

  doc.save();
}

function ensurePngFolder(psdFolder) {
  var parentDir = new Folder(psdFolder).parent;
  var pngFolder = new Folder(parentDir + "/jpg");

  if (!pngFolder.exists) {
    pngFolder.create();
  }
  return pngFolder;
}

function resizeDocument(doc, percentage) {
  var originalWidth = doc.width;
  var originalHeight = doc.height;

  doc.resizeImage(
    originalWidth * percentage,
    originalHeight * percentage,
    72,
    ResampleMethod.BICUBIC
  );
}

function main() {
  try {
    var doc = app.activeDocument;
    var folder = new Folder(doc.path);
    var folderName = decodeURIComponent(folder.fsName);

    // Ensure all paths use forward slashes
    folderName = folderName.replace(/\\/g, "/").toLowerCase();

    if (folderName.indexOf("封面/psd") !== -1) {
      $.writeln("Start process file: " + decodeURIComponent(doc.name));

      if (doc.name.match(/-s\.psd$/)) {
        saveAsSquareJpg(doc);
      } else {
        saveAsWideJpg(doc);
      }
    } else if (folderName.indexOf("splashes/psd") !== -1) {
      saveToJpg(doc);
    } else {
      // alert(
      //   "Not recognized folder: \n" +
      //     folderName +
      //     " \n index: " +
      //     folderName.indexOf("封面/PSD") +
      //     " \n index: " +
      //     folderName.indexOf("My Books/splashes")
      // );
    }
  } catch (e) {
    alert("Error: " + e.message);
  }
}

main();
