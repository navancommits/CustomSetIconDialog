function toggleSelectDiv(element, evt) {    
    var search = scForm.browser.getControl("div_Search");
    search.style.display = "none";

    var select = scForm.browser.getControl("div_Select");
    select.style.display = "";
}

function toggleSearchDiv(element, evt) {  
    var select = scForm.browser.getControl("div_Select");
    select.style.display = "none";

    var search = scForm.browser.getControl("div_Search");
    search.style.display = "";
}

//function toggleDiv(element, evt) {
//    if (element.id == "Select") {
//        var search = scForm.browser.getControl("div_search");
//        search.style.display = "none";

//        var select = scForm.browser.getControl("div_select");
//        select.style.display = "";
//    }

//    if (element.id == "Search") {
//        var select = scForm.browser.getControl("div_select");
//        select.style.display = "none";

//        var search = scForm.browser.getControl("div_search");
//        search.style.display = "";
//    }
//}

//$(function () {
//    $("[name=toggler]").click(function () {
//        $('.toHide').hide();
//        $("#div_" + $(this).val()).show('slow');
//    });
//});


function scClick(element, evt) {

  evt = scForm.lastEvent ? scForm.lastEvent : evt;
  var icon = evt.srcElement ? evt.srcElement : evt.target;
  
  var edit = scForm.browser.getControl("IconFile");
  
  if (!icon) {
    return;
  }
   
  var src = scForm.browser.isIE ? icon["sc_path"] : icon.getAttribute("sc_path");

  if (src==null && icon.tagName && icon.tagName.toLowerCase() == "img" && icon.className == "scRecentIcon") {
    src = icon.src;
  }
   
  if (src == null) {
    return;
  }
  
  var n = src.indexOf("/-/icon/");
  
  if (n >= 0) {
    src = src.substr(n + 8); 
  }
  else if (src.substr(0, 32) == "/sitecore/shell/themes/standard/") {
    src = src.substr(32);
  }
  
  n = src.toLowerCase().indexOf(scForm.Settings.Icons.CacheFolder.toLowerCase());
  if (n >= 0) {
    src = src.substr(n + scForm.Settings.Icons.CacheFolder.length);
    if (src[0] === '/') {
      src = src.substring(1);
    }
  }
  
  if (src.substr(src.length - 5, 5) == ".aspx") {
    src = src.substr(0, src.length - 5);
  }
  
  edit.value = src;
}

function filterList() {
    var input, filter, map, area, a, i, txtValue;
    input = scForm.browser.getControl('SearchText');
    filter = input.value.toUpperCase();
    ul = scForm.browser.getControl("Complete");
    li = map.getElementsByTagName('li');    
    for (i = 0; i < li.length; i++) {
        a = li[i];
        txtValue = a.alt;
        if (txtValue.toUpperCase().indexOf(filter) > -1) {
            alert("hi");
            a[i].style.display = "";
        } else {
            a[i].style.display = "none";
        }
    }
}

function scChange(element, evt) {
  var element = scForm.browser.getControl("Selector");  
  
  var id = element.options[element.selectedIndex].value + "List";
  
  var list = scForm.browser.getControl("List");
  
  var childNodes = list.childNodes;
  
  for(var n = 0; n < childNodes.length; n++) {
    var element = childNodes[n];
    
    element.style.display = (element.id == id ? "" : "none");
  }

  scUpdateControls();
}

//function scTogglerChange(element, evt) {
//    var element = scForm.browser.getControl("Toggler");

//    var visibleid = "div_" + element.options[element.selectedIndex].value;

//    var nextindex = 0;

//    if (element.selectedIndex == 0) { var nextindex = 1; }

//    var invisibleid = "div_" + element.options[nextindex].value;    

//    var visiblediv = scForm.browser.getControl(visibleid);
//    visiblediv.style.display = "visible"; 

//    var invisiblediv = scForm.browser.getControl(invisibleid);
//    invisiblediv.style.display = "none";

//    scUpdateControls();
//}

function scUpdateControls() {
  if (!scForm.browser.isIE) {
    scForm.browser.initializeFixsizeElements();
  }
}