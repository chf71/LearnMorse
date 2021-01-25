CheckMCAnswer = function (chosenIndex) {
    if (chosenIndex == CorrectIndex) {
        $("#answer" + chosenIndex).css("background-color", "#28a745");
    }
    else {
        $("#answer" + chosenIndex).css("background-color", "#dc3545");
    }

    TransitionNext('MC');
}

CheckInputAnswer = function () {
    if (MorseInputText == MorseText) {
        $("#morseinput").css("background-color", "#28a745");

        context.suspend();

        TransitionNext('Input');
    }
}