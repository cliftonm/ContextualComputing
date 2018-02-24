function wireUpValueChangeNotifier() {
    setupOnChangeEvent("contextualValue", updateField);
    // setupOnChangeEvent("contextualValueSearch", updateSearchField);
}

function setupOnChangeEvent(className, updateFunction) {
    forEachContextualValue(className, function (id) {
        var docInput = document.getElementById(id);
        docInput.onchange = function () {
            updateFunction(id);
        };
    });
}

function wireUpEvents() {
    var el;
    el = document.getElementById("newContext");
    if (el != null) { el.onclick = function () {newContext(); } }
    el = document.getElementById("searchContext");
    if (el != null) { el.onclick = function () { searchContext(); } }
}

function newContext() {
    clearInputs();
    createNewGuids();
}

function searchContext() {
    var searchList = [];

    forEachContextualValue("contextualValueSearch", function (id) {
        var docInput = document.getElementById(id);
        var val = docInput.value;

        if (val != "") {
            var contextPath = docInput.getAttribute("contextPath");
            var cvid = docInput.getAttribute("cvid");
            searchList.push({ value: val, id: cvid, typePath: contextPath });
        }
    });

    if (searchList.length > 0) {
        post("/searchContext", { searchFields: searchList }, onShowResults);
    }
}

function onShowResults(json) {
    updateHtml("searchResults", json.html);
}

function onShowSelectedSearchItem(json) {
    updateHtml("viewSelectedItem", json.html);
}

function clearInputs() {
    forEachContextualValue("contextualValue", function (id) { document.getElementById(id).value = ""; });
}

function createNewGuids() {
    var uniqueIds = getUniqueIds();
    var idMap = mapToNewIds(uniqueIds);
    assignNewIds(idMap);
}

function getUniqueIds() {
    var uniqueIds = [];

    // Other ways this might be accomplished but I'm too lazy to work out the nuances of 
    // adding an array of values uniquely to a master array.
    // https://stackoverflow.com/questions/1960473/get-all-unique-values-in-an-array-remove-duplicates
    forEachContextualValue("contextualValue", function (id) {
        var ids = id.split(".");
        for (var i = 0; i < ids.length; i++) {
            var id = ids[i];
            if (!uniqueIds.includes(id)) {
                uniqueIds.push(id);
            }
        }
    });

    return uniqueIds;
}

function mapToNewIds(uniqueIds) {
    var idMap = {};

    for (var i = 0; i < uniqueIds.length; i++) {
        idMap[uniqueIds[i]] = uuidv4();
    }

    return idMap;
}

function assignNewIds(idMap) {
    forEachContextualValue("contextualValue", function (id) {
        var oldIds = id.split(".");
        var newIds=[];

        for (var i = 0; i < oldIds.length; i++) {
            newIds.push(idMap[oldIds[i]]);
        }

        newId = newIds.join(".");
        document.getElementById(id).setAttribute("cvid", newId);
    });
}

// From SO: https://stackoverflow.com/questions/105034/create-guid-uuid-in-javascript
function uuidv4() {
    return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
        (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
    )
}

function forEachContextualValue(className, action) {
    var docInputs = document.getElementsByClassName(className);
    for (var i = 0; i < docInputs.length; i++) {
        var id = docInputs[i].id;
        action(id);
    }
}

// We don't care about the /updateField json response.
function updateField(id) {
    sendFieldChange(id, "/updateField");
}

function sendFieldChange(id, url) {
    var docInput = document.getElementById(id);
    var val = docInput.value;
    var contextPath = docInput.getAttribute("contextPath");
    var recordNumber = docInput.getAttribute("recordNumber");
    var cvid = docInput.getAttribute("cvid");
    console.log(val + " " + contextPath + "[" + recordNumber + "]");
    post(url, { value: val, id: cvid, typePath: contextPath, recordNumber: recordNumber }, getDictionaryHtmlNoIds);
}

function getDictionaryHtmlNoIds() {
    getDictionaryHtml(false);
}

function getDictionaryHtml(showIds) {
    get("/dictionaryTreeHtml?showInstanceIds=" + showIds, onDictionaryTreeHtml);
    get("/dictionaryNodesHtml", onDictionaryNodesHtml);
}

function onDictionaryTreeHtml(json) {
    updateHtml("dictionary", json.html);
}

function onDictionaryNodesHtml(json) {
    updateHtml("nodes", json.html);
}

function updateHtml(tag, b64html) {
    var html = atob(b64html);
    var el = document.getElementById(tag);
    el.innerHTML = html;
}

function get(url, callback) {
    return fetch(url, { method: "GET" }).then(function (response) {
        return response.json();
    }).then(function (jsonData) {
        callback(jsonData);
    });
}

function post(url, data, callback) {
    return fetch(url, { method: "POST", body: JSON.stringify(data) }).then(function (response) {
        return response.json();
    }).then(function (jsonData) {
        callback(jsonData);
    });
}

function populateFromLookup(lookup) {
    // Ignore "Choose Item:"
    if (lookup.selectedIndex != 0) {        
        var lookupId = lookup[lookup.selectedIndex].value;
        var records = findLookupRecords(lookupId);
        updateInputControls(records);
        sendFieldChanges(records);
    }
}

// Find all records in the lookupDictionary for the given lookup ID.
function findLookupRecords(lookupId) {
    var lookupRecords = [];

    for (var idx = 0; idx < lookupDictionary.length; idx++) {
        record = lookupDictionary[idx];
        if (record.LookupId == lookupId) {
            lookupRecords.push(record);
        }
    }

    return lookupRecords;
}

// For each record, find the input control whose ID matches the original instance path ID
// and update the value and "cvid" attribute.
function updateInputControls(records) {
    for (var idx = 0; idx < records.length; idx++) {
        var record = records[idx];
        var originalId = record.OriginalContextInstancePath.join(".");
        var docInput = document.getElementById(originalId);
        docInput.value = record.Value;
        docInput.setAttribute("cvid", record.NewContextInstancePath.join("."));
    }
}

// Inform the server of the lookup selection.
function sendFieldChanges(records) {
    for (var idx = 0; idx < records.length; idx++) {
        var record = records[idx];
        var originalId = record.OriginalContextInstancePath.join(".");
        sendFieldChange(originalId, "/updateField");
    }
}
