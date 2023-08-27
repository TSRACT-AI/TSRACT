function writeToScreen(targetId, message)
{
    var converter = new showdown.Converter();
    var targetElement = document.getElementById(targetId);
    targetElement.innerHTML = converter.makeHtml(message);
}
