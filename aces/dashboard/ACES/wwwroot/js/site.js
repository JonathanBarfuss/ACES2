// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Function used to copy the text found in an element with a specific id to the clipboard
function copyFunction(id) {

    var copyElement = document.getElementById(id);
    copyText = copyElement.innerHTML;  //just get the text between the tags

    var textArea = document.createElement("textarea");  //temp textarea created so the text can be selected
    textArea.value = copyText;  //add the text to the text area
    document.body.appendChild(textArea);  //add the textarea to the document
    textArea.select();  //select the text
    document.execCommand("Copy");  //copy it to the clipboard
    textArea.remove();
}