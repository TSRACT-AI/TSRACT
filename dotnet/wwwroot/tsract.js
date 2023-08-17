function writeToScreen(targetId, message)
{
    var targetElement = document.getElementById(targetId);
    targetElement.innerHTML += message;
}
