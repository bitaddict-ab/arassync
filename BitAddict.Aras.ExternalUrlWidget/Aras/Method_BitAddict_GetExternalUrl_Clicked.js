// MIT License, see COPYING.TXT
// ReSharper disable ThisInGlobalContext
// ReSharper disable PossiblyUnassignedProperty

const item = parent.item;

const body =
  `<id>${item.id}</id>
   <type>${item.attributes["type"].value}</type>
   <baseurl>${aras.getBaseURL()}</baseurl>`;

const resultAML = aras.applyMethod("BitAddict_GetExternalUrl", body);
console.log(`Result: ${resultAML}`);

const resultItem = new Item("result");
resultItem.loadAML(resultAML);

if (resultItem.isError()) {
    alert(resultItem.getErrorString());
    return;
}

const url = resultItem.getResult();
console.log(`URL: ${url}`);
    
aras.bitAddictCopyToClipBoard(url);
