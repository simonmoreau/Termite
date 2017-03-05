var viewerApp;
var model;

function showModel(urn) {
    var options = {
        env: 'AutodeskProduction',
        getAccessToken: getAccessToken,
        refreshToken: getAccessToken
    };

    var documentId = 'urn:' + urn;

    Autodesk.Viewing.Initializer(options, function onInitialized() {

        viewerApp = new Autodesk.Viewing.ViewingApplication('MyViewerDiv');

        //Configure the extension
        var config3D = {
            extensions: ["ForgeViewerExtension"]
        };

        viewerApp.registerViewer(viewerApp.k3D, Autodesk.Viewing.Private.GuiViewer3D, config3D);

        viewerApp.loadDocument(documentId, onDocumentLoadSuccess, onDocumentLoadFailure);
    });
}

/**
* Autodesk.Viewing.ViewingApplication.loadDocument() success callback.
* Proceeds with model initialization.
*/
function onDocumentLoadSuccess(doc) {

    // We could still make use of Document.getSubItemsWithProperties()
    // However, when using a ViewingApplication, we have access to the **bubble** attribute,
    // which references the root node of a graph that wraps each object from the Manifest JSON.
    var viewables = viewerApp.bubble.search({ 'type': 'geometry' });
    if (viewables.length === 0) {
        console.error('Document contains no viewables.');
        return;
    }

    document = doc;

    // Choose any of the avialble viewables
    viewerApp.selectItem(viewables[0].data, onItemLoadSuccess, onItemLoadFail);

}

/**
 * Autodesk.Viewing.ViewingApplication.loadDocument() failure callback. 
 * @param {} viewerErrorCode 
 * @returns {} 
 */
function onDocumentLoadFailure(viewerErrorCode) {
    console.error('onDocumentLoadFailure() - errorCode:' + viewerErrorCode);
}

/**
 * viewerApp.selectItem() success callback.
 * @param {} viewer 
 * @param {} item 
 * @returns {} 
 */
function onItemLoadSuccess(viewer, item) {
    console.log('onItemLoadSuccess()!');
    console.log(viewer);
    console.log(item);

    // Congratulations! The viewer is now ready to be used.
    console.log('Viewers are equal: ' + (viewer === viewerApp.getCurrentViewer()));

    model = viewer.model;

}

/**
 * viewerApp.selectItem() failure callback.
 */
function onItemLoadFail(errorCode) {
    console.error('onItemLoadFail() - errorCode:' + errorCode);
}

/**
* the JavaScript getAccessToken on client-side. 
* To retrive viewer token
*/
function getAccessToken() {
    var xmlHttp = null;
    xmlHttp = new XMLHttpRequest();
    xmlHttp.open("GET", '/api/forge/token', false /*forge viewer requires SYNC*/);
    xmlHttp.send(null);
    return xmlHttp.responseText;
}


/**
 * Create an extension
 */
function ForgeViewerExtension(viewer, options) {
    Autodesk.Viewing.Extension.call(this, viewer, options);
}

ForgeViewerExtension.prototype = Object.create(Autodesk.Viewing.Extension.prototype);
ForgeViewerExtension.prototype.constructor = ForgeViewerExtension;

/**
 * Event triggered when selecting an object
 */
ForgeViewerExtension.prototype.onSelectionEvent = function (event) {
    var currSelection = this.viewer.getSelection();
    var domElem = document.getElementById('MySelectionValue');
    domElem.innerText = currSelection.length;

    var prop = model.getProperties(currSelection[0], getPropertiesSuccess, getPropertiesFailure);
    //var properties = currSelection[0]
};

/**
 * Triggered when I found properties for a selection item
 */
function getPropertiesSuccess(parameters) {
    var domElem = document.getElementById("MyPropertiesSelected");

    var props = parameters.properties;
    var propsTable = "<table class=\"table\">";

    for (var i = 0; i < props.length; i++) {
        propsTable += "<tr><td>" + props[i].displayName + "</td><td>" + props[i].displayValue + "</td></tr>";
    }

    propsTable += "</table>";

    domElem.innerHTML = propsTable;
}


/**
 * Triggered when I found properties for a selection item
 */
function getPropertiesFailure(parameters) {
    var domElem = document.getElementById('MyPropertiesSelected');
    domElem.innerText = "failure";
}

/**
 * Create a section when clicking on a model face
 * @param {any} viewer the Forge viewer
 */
function createSection(viewer) {


}

function removeSections(viewer) {
    viewer.setCutPlanes(null);
}


/**
 * Triggered when the extension ForgeViewerExtension is loaded
 * @returns {} 
 */
ForgeViewerExtension.prototype.load = function () {

    var viewer = this.viewer;

    //Load the selection event
    this.onSelectionBinded = this.onSelectionEvent.bind(this);
    this.viewer.addEventListener(Autodesk.Viewing.SELECTION_CHANGED_EVENT, this.onSelectionBinded);

    //Load the
    var lockBtn = document.getElementById('MyAwesomeLockButton');
    lockBtn.addEventListener('change', function () {
        if (lockBtn.checked) {
            viewer.setNavigationLock(true);
        }
        else {
            viewer.setNavigationLock(false);
        }
    });

    var sectionBtn = document.getElementById("MySectionView");
    sectionBtn.addEventListener("click", function () {
        document.getElementById("MyViewerDiv").onclick = function handleMouseClick(event) {
            var position = viewer.clientToWorld(event.clientX, event.clientY, true);
            if (position != null) {
                var normal = position.face.normal;
                var intersectPoint = position.intersectPoint;

                var a = normal.x;
                var b = normal.y;
                var c = normal.z;

                var x0 = intersectPoint.x;
                var y0 = intersectPoint.y;
                var z0 = intersectPoint.z;

                var d = -a * x0 - b * y0 - c * z0;

                var plane = [new THREE.Vector4(a, b, c, d)];

                viewer.setCutPlanes(plane);
            }

            document.getElementById("MyViewerDiv").onclick = null;

        }
    });

    var removeSectionBtn = document.getElementById("removeSectionBtn");
    removeSectionBtn.addEventListener("click", function () {
        removeSections(viewer);
    });

    return true;
};

/**
 * Triggered when the extension ForgeViewerExtension is unloaded
 * @returns {} 
 */
ForgeViewerExtension.prototype.unload = function () {
    alert('ForgeViewerExtension is now unloaded!');

    //Unload the selection  event
    this.viewer.removeEventListener(Autodesk.Viewing.SELECTION_CHANGED_EVENT, this.onSelectionBinded);
    this.onSelectionBinded = null;
    return true;
};

Autodesk.Viewing.theExtensionManager.registerExtension('ForgeViewerExtension', ForgeViewerExtension);